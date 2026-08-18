using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Models;
using SweetSoft.QLDA.Core.MailManager.Repository;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.SysManager.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailHistoryService : IEmailHistoryService
    {
        private const string SystemUser = "[System]";

        private readonly ClientInfo _clientInfo;
        private readonly IAuditManager _auditManager;
        private readonly IEmailHistoryRepository _repository;
        private readonly ISettingManager _settingManager;

        public EmailHistoryService(
            ClientInfo clientInfo,
            IAuditManager auditManager = null,
            IEmailHistoryRepository repository = null,
            ISettingManager settingManager = null)
        {
            _clientInfo = clientInfo ?? new ClientInfo();
            _auditManager = auditManager ?? new AuditManager(_clientInfo);
            _repository = repository ?? new EmailHistoryRepository();
            _settingManager = settingManager;
        }

        #region IEmailHistoryService Implementation

        public TblEmailHistory GetEmailHistoryById(Guid id)
        {
            return _repository.GetById(id);
        }

        public async Task<EmailHistory> CreateEmailHistoryAsync(EmailRequest request)
        {
            ValidateEmailRequest(request);

            try
            {
                var existingHistory = request.RefId.HasValue
                    ? await _repository.GetUnsentEmailByRefAsync(request.RefId.Value, request.RefType, request.CustomerId)
                    : null;

                var persistedEntity = existingHistory == null
                    ? await CreateNewEmailHistoryInternalAsync(request)
                    : await UpdateExistingEmailHistoryInternalAsync(existingHistory, request);

                return MapToEmailHistory(persistedEntity, request);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(
                    ex,
                    "Failed to create email history for customer {CustomerId}, email {ToEmail}",
                    request.CustomerId,
                    request.ToEmail);

                throw new EmailHistoryException($"Failed to create email history: {ex.Message}", ex);
            }
        }

        public async Task<EmailHistory> GetEmailHistoryByIdAsync(Guid historyId)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(historyId);
                return entity != null ? MapToEmailHistory(entity) : null;
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get email history by ID: {HistoryId}", historyId);
                throw new EmailHistoryException($"Failed to get email history: {ex.Message}", ex);
            }
        }

        public async Task LogEmailErrorAsync(Guid? refId, EmailType refType, Guid customerId, string email, string errorMessage)
        {
            try
            {
                var request = new EmailRequest
                {
                    RefId = refId,
                    RefType = refType,
                    CustomerId = customerId,
                    ToEmail = email,
                    Subject = "Error occurred while sending email",
                    Content = "An error occurred while processing this email",
                    FromEmail = GetDefaultSenderEmail(),
                    Sender = "System",
                    CreatedBy = SystemUser
                };

                var emailHistory = await CreateEmailHistoryAsync(request);
                await UpdateEmailHistoryStatusAsync(emailHistory.Id, EmailStatus.Failed, errorMessage);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to log email error for {Email}: {ErrorMessage}", email, errorMessage);
            }
        }

        public async Task UpdateEmailHistoryStatusAsync(Guid historyId, EmailStatus status, string message = null)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(historyId);
                if (entity == null)
                {
                    SysLogger.LogError("Email history not found for ID: {HistoryId}", historyId);
                    return;
                }

                var originalEntity = entity.Clone();
                ApplyStatus(entity, status, message);

                entity.UpdatedDate = DateTime.UtcNow;
                entity.UpdatedUser = SystemUser;

                await _repository.UpdateAsync(entity);
                await _auditManager.LogChangesAsync(originalEntity, entity, nameof(TblEmailHistory), entity.Id, entity.UpdatedUser);

                SysLogger.LogDebug("Updated email history {HistoryId} to status {Status}", historyId, status);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to update email history status for ID: {HistoryId}", historyId);
                throw new EmailHistoryException($"Failed to update email history status: {ex.Message}", ex);
            }
        }

        public async Task SaveAsync(EmailHistory emailHistory)
        {
            try
            {
                var entity = MapToTblEmailHistory(emailHistory);
                if (entity.Id == Guid.Empty)
                {
                    var now = DateTime.UtcNow;
                    entity.Id = UUIDv7.NewGuid();
                    entity.CreatedDate = now;
                    entity.CreatedUser = SystemUser;
                    entity.UpdatedDate = now;
                    entity.UpdatedUser = SystemUser;
                }

                var existingEntity = entity.Id != Guid.Empty
                    ? await _repository.GetByIdAsync(entity.Id)
                    : null;

                if (existingEntity == null)
                {
                    await _repository.InsertAsync(entity);
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, entity, nameof(TblEmailHistory), entity.Id);
                }
                else
                {
                    var originalEntity = existingEntity.Clone();
                    CopyMutableFields(existingEntity, entity);
                    existingEntity.UpdatedDate = DateTime.UtcNow;
                    existingEntity.UpdatedUser = SystemUser;

                    await _repository.UpdateAsync(existingEntity);
                    await _auditManager.LogChangesAsync(originalEntity, existingEntity, nameof(TblEmailHistory), existingEntity.Id, existingEntity.UpdatedUser);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to save email history for ID: {HistoryId}", emailHistory.Id);
                throw new EmailHistoryException($"Failed to save email history: {ex.Message}", ex);
            }
        }

        public async Task<TblEmailHistory> GetUnsentEmailByRefAsync(Guid refId, EmailType emailType)
        {
            return await _repository.GetUnsentEmailByRefAsync(refId, emailType);
        }

        #endregion

        #region Public Methods

        public async Task<bool> IsEmailAlreadySentAsync(Guid refId, EmailType refType)
        {
            try
            {
                return await _repository.IsEmailSentAsync(refId, refType);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to check if email already sent for RefId: {RefId}, RefType: {RefType}", refId, refType);
                return false;
            }
        }

        public async Task<PagedResult<EmailHistoryDto>> SearchEmailHistoriesAsync(EmailHistorySearchRequest searchRequest)
        {
            ValidateSearchRequest(searchRequest);

            try
            {
                var result = await _repository.SearchPagedAsync(searchRequest);
                var emailHistories = result.Items.Select(MapToEmailHistoryDto).ToList();

                return new PagedResult<EmailHistoryDto>
                {
                    Items = emailHistories,
                    TotalRecords = result.TotalRecords,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    TotalPages = (int)Math.Ceiling((double)result.TotalRecords / searchRequest.PageSize)
                };
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to search email histories");
                throw new EmailHistoryException($"Failed to search email histories: {ex.Message}", ex);
            }
        }

        public async Task<List<EmailHistoryDto>> GetEmailHistoriesByRefAsync(Guid refId, EmailType refType)
        {
            try
            {
                var entities = await _repository.GetByRefIdAsync(refId, refType);
                return entities.Select(MapToEmailHistoryDto).ToList();
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get email histories for RefId: {RefId}, RefType: {RefType}", refId, refType);
                throw new EmailHistoryException($"Failed to get email histories: {ex.Message}", ex);
            }
        }

        public async Task<List<EmailHistoryDto>> GetEmailHistoriesByCustomerAsync(Guid customerId, int? limit = null)
        {
            try
            {
                var entities = await _repository.GetByCustomerIdAsync(customerId, limit);
                return entities.Select(MapToEmailHistoryDto).ToList();
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get email histories for customer: {CustomerId}", customerId);
                throw new EmailHistoryException($"Failed to get email histories for customer: {ex.Message}", ex);
            }
        }

        public async Task MarkEmailAsReadAsync(Guid historyId, DateTime? readDate = null)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(historyId);
                if (entity == null)
                {
                    SysLogger.LogError("Email history not found for ID: {HistoryId}", historyId);
                    return;
                }

                if (!entity.IsRead)
                {
                    var originalEntity = entity.Clone();
                    entity.IsRead = true;
                    entity.ReadDate = readDate ?? DateTime.UtcNow;
                    entity.UpdatedDate = DateTime.UtcNow;
                    entity.UpdatedUser = SystemUser;

                    await _repository.UpdateAsync(entity);
                    await _auditManager.LogChangesAsync(originalEntity, entity, nameof(TblEmailHistory), entity.Id, entity.UpdatedUser);

                    SysLogger.LogDebug("Marked email history {HistoryId} as read", historyId);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to mark email as read for ID: {HistoryId}", historyId);
                throw new EmailHistoryException($"Failed to mark email as read: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteEmailHistoryAsync(Guid historyId, string deletedBy = SystemUser)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(historyId);
                if (entity == null)
                {
                    SysLogger.LogError("Email history not found for deletion: {HistoryId}", historyId);
                    return false;
                }

                await _repository.DeleteAsync(historyId);
                await _auditManager.LogActionAsync(LogActions.Actions.DELETE, entity, nameof(TblEmailHistory), historyId, deletedBy);

                SysLogger.LogInfo("Deleted email history {HistoryId}", historyId);
                return true;
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to delete email history: {HistoryId}", historyId);
                throw new EmailHistoryException($"Failed to delete email history: {ex.Message}", ex);
            }
        }

        public async Task<EmailStatistics> GetEmailStatisticsAsync(
            Guid? customerId = null,
            Guid? refId = null,
            EmailType? refType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                return await _repository.GetEmailStatisticsAsync(customerId, refId, refType, fromDate, toDate);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to get email statistics");
                throw new EmailHistoryException($"Failed to get email statistics: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task<TblEmailHistory> CreateNewEmailHistoryInternalAsync(EmailRequest request)
        {
            var now = DateTime.UtcNow;
            var auditUser = ResolveAuditUser(request.CreatedBy);

            var entity = new TblEmailHistory
            {
                Id = UUIDv7.NewGuid(),
                RefId = request.RefId ?? Guid.Empty,
                RefType = request.RefType.ToString(),
                CustomerId = request.CustomerId,
                SenderId = request.SenderId ?? Guid.Empty,
                Sender = request.Sender ?? string.Empty,
                Subject = request.Subject ?? string.Empty,
                EmailContent = EncodeEmailContent(request.Content),
                FromEmail = request.FromEmail ?? string.Empty,
                ToEmail = request.ToEmail ?? string.Empty,
                CcEmail = SerializeEmailAddresses(request.CcEmails),
                BccEmail = SerializeEmailAddresses(request.BccEmails),
                IsSent = false,
                IsRead = false,
                NumberOfSent = 0,
                SentDate = null,
                ReadDate = null,
                ErrorMessage = string.Empty,
                CreatedDate = now,
                UpdatedDate = now,
                CreatedUser = auditUser,
                UpdatedUser = auditUser
            };

            await _repository.InsertAsync(entity);
            await _auditManager.LogActionAsync(LogActions.Actions.CREATE, entity, nameof(TblEmailHistory), entity.Id, auditUser);

            return entity;
        }

        private async Task<TblEmailHistory> UpdateExistingEmailHistoryInternalAsync(TblEmailHistory entity, EmailRequest request)
        {
            var originalEntity = entity.Clone();
            var auditUser = ResolveAuditUser(request.CreatedBy);

            if (request.RefId.HasValue)
            {
                entity.RefId = request.RefId.Value;
            }

            entity.RefType = request.RefType.ToString();
            entity.CustomerId = request.CustomerId;
            entity.SenderId = request.SenderId ?? entity.SenderId;
            entity.Sender = request.Sender ?? string.Empty;
            entity.Subject = request.Subject ?? string.Empty;
            entity.EmailContent = EncodeEmailContent(request.Content);
            entity.FromEmail = request.FromEmail ?? string.Empty;
            entity.ToEmail = request.ToEmail ?? string.Empty;
            entity.CcEmail = SerializeEmailAddresses(request.CcEmails);
            entity.BccEmail = SerializeEmailAddresses(request.BccEmails);
            entity.IsSent = false;
            entity.SentDate = null;
            entity.IsRead = false;
            entity.ReadDate = null;
            entity.ErrorMessage = string.Empty;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedUser = auditUser;

            await _repository.UpdateAsync(entity);
            await _auditManager.LogChangesAsync(originalEntity, entity, nameof(TblEmailHistory), entity.Id, entity.UpdatedUser);

            return entity;
        }

        private static void ApplyStatus(TblEmailHistory entity, EmailStatus status, string message)
        {
            switch (status)
            {
                case EmailStatus.Sent:
                    entity.IsSent = true;
                    entity.SentDate = DateTime.UtcNow;
                    entity.ErrorMessage = string.Empty;
                    break;
                case EmailStatus.Pending:
                    entity.IsSent = false;
                    entity.ErrorMessage = string.Empty;
                    entity.SentDate = null;
                    entity.NumberOfSent++;
                    break;
                case EmailStatus.Failed:
                    entity.IsSent = false;
                    entity.SentDate = null;
                    entity.ErrorMessage = NormalizeErrorMessage(message);
                    break;
                default:
                    entity.IsSent = false;
                    entity.ErrorMessage = string.Empty;
                    break;
            }
        }

        private EmailHistory MapToEmailHistory(TblEmailHistory entity, EmailRequest request = null)
        {
            var emailHistory = new EmailHistory
            {
                Id = entity.Id,
                RefId = entity.RefId,
                RefType = Enum.TryParse<EmailType>(entity.RefType, out var refType) ? refType : EmailType.None,
                CustomerId = entity.CustomerId,
                SenderId = entity.SenderId,
                Sender = entity.Sender ?? string.Empty,
                Subject = entity.Subject ?? string.Empty,
                EmailContent = entity.EmailContent ?? string.Empty,
                FromEmail = entity.FromEmail ?? string.Empty,
                ToEmail = entity.ToEmail ?? string.Empty,
                CcEmail = entity.CcEmail ?? string.Empty,
                BccEmail = entity.BccEmail ?? string.Empty,
                CreatedDate = entity.CreatedDate,
                SentDate = entity.SentDate,
                NumberOfSent = entity.NumberOfSent,
                IsSent = entity.IsSent,
                IsRead = entity.IsRead,
                ReadDate = entity.ReadDate,
                ErrorMessage = entity.ErrorMessage ?? string.Empty
            };

            if (request?.IsTracking == true && _settingManager != null)
            {
                var trackingUrl = GenerateTrackingUrl(entity.Id);
                if (!string.IsNullOrEmpty(trackingUrl))
                {
                    emailHistory.EmailContent += $"<img src='{trackingUrl}' style='display:none' alt='' />";
                }
            }

            return emailHistory;
        }

        private EmailHistoryDto MapToEmailHistoryDto(TblEmailHistory entity)
        {
            return new EmailHistoryDto
            {
                Id = entity.Id,
                RefId = entity.RefId,
                RefType = entity.RefType,
                CustomerId = entity.CustomerId,
                Sender = entity.Sender,
                Subject = entity.Subject,
                FromEmail = entity.FromEmail,
                ToEmail = entity.ToEmail,
                CcEmails = DeserializeEmailAddresses(entity.CcEmail),
                BccEmails = DeserializeEmailAddresses(entity.BccEmail),
                CreatedDate = entity.CreatedDate,
                SentDate = entity.SentDate,
                NumberOfSent = entity.NumberOfSent,
                IsSent = entity.IsSent,
                IsRead = entity.IsRead,
                ReadDate = entity.ReadDate,
                ErrorMessage = entity.ErrorMessage,
                CreatedUser = entity.CreatedUser,
                UpdatedUser = entity.UpdatedUser,
                UpdatedDate = entity.UpdatedDate
            };
        }

        private TblEmailHistory MapToTblEmailHistory(EmailHistory emailHistory)
        {
            return new TblEmailHistory
            {
                Id = emailHistory.Id,
                RefId = emailHistory.RefId ?? Guid.Empty,
                RefType = emailHistory.RefType.ToString(),
                CustomerId = emailHistory.CustomerId ?? Guid.Empty,
                SenderId = emailHistory.SenderId ?? Guid.Empty,
                Sender = emailHistory.Sender ?? string.Empty,
                Subject = emailHistory.Subject ?? string.Empty,
                EmailContent = emailHistory.EmailContent ?? string.Empty,
                FromEmail = emailHistory.FromEmail ?? string.Empty,
                ToEmail = emailHistory.ToEmail ?? string.Empty,
                CcEmail = emailHistory.CcEmail ?? string.Empty,
                BccEmail = emailHistory.BccEmail ?? string.Empty,
                CreatedDate = emailHistory.CreatedDate == default ? DateTime.UtcNow : emailHistory.CreatedDate,
                CreatedUser = SystemUser,
                UpdatedDate = DateTime.UtcNow,
                UpdatedUser = SystemUser,
                SentDate = emailHistory.SentDate,
                NumberOfSent = emailHistory.NumberOfSent,
                IsSent = emailHistory.IsSent,
                IsRead = emailHistory.IsRead,
                ReadDate = emailHistory.ReadDate,
                ErrorMessage = emailHistory.ErrorMessage ?? string.Empty
            };
        }

        private static void CopyMutableFields(TblEmailHistory target, TblEmailHistory source)
        {
            target.RefId = source.RefId;
            target.RefType = source.RefType;
            target.CustomerId = source.CustomerId;
            target.SenderId = source.SenderId;
            target.Sender = source.Sender;
            target.Subject = source.Subject;
            target.EmailContent = source.EmailContent;
            target.FromEmail = source.FromEmail;
            target.ToEmail = source.ToEmail;
            target.CcEmail = source.CcEmail;
            target.BccEmail = source.BccEmail;
            target.SentDate = source.SentDate;
            target.NumberOfSent = source.NumberOfSent;
            target.IsSent = source.IsSent;
            target.IsRead = source.IsRead;
            target.ReadDate = source.ReadDate;
            target.ErrorMessage = source.ErrorMessage;
        }

        private static string SerializeEmailAddresses(IEnumerable<EmailAddress> emailAddresses)
        {
            if (emailAddresses == null)
            {
                return string.Empty;
            }

            var list = emailAddresses.ToList();
            if (list.Count == 0)
            {
                return string.Empty;
            }

            try
            {
                return JsonConvert.SerializeObject(list);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to serialize email addresses");
                return string.Empty;
            }
        }

        private static List<EmailAddress> DeserializeEmailAddresses(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<EmailAddress>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<EmailAddress>>(jsonString) ?? new List<EmailAddress>();
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to deserialize email addresses: {JsonString}", jsonString);
                return new List<EmailAddress>();
            }
        }

        private string GenerateTrackingUrl(Guid emailHistoryId)
        {
            try
            {
                var apiHostPath = CommonHelpers.GetAPIHostPath();
                var encryptedId = SecurityUtilities.EncryptContent(emailHistoryId.ToString());
                return $"{apiHostPath}api/v1/TrackingEmail?rk={encryptedId}";
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to generate tracking URL for email history: {HistoryId}", emailHistoryId);
                return string.Empty;
            }
        }

        private static string EncodeEmailContent(string content)
        {
            return HttpUtility.HtmlEncode(content ?? string.Empty) ?? string.Empty;
        }

        private static string ResolveAuditUser(string user)
        {
            return string.IsNullOrWhiteSpace(user) ? SystemUser : user;
        }

        private static string NormalizeErrorMessage(string message)
        {
            return string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();
        }

        private string GetDefaultSenderEmail()
        {
            return _settingManager?.GetSettingValue(SettingKeys.AdministratorEmail)
                   ?? _settingManager?.GetSettingValue(SettingKeys.SmtpSenderEmail)
                   ?? string.Empty;
        }

        private void ValidateEmailRequest(EmailRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Email request must be provided");
            }

            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                throw new ArgumentException("ToEmail is required", nameof(request.ToEmail));
            }

            if (string.IsNullOrWhiteSpace(request.FromEmail))
            {
                throw new ArgumentException("FromEmail is required", nameof(request.FromEmail));
            }

            if (!RegexUtilities.IsValidEmail(request.ToEmail))
            {
                throw new ArgumentException($"Invalid ToEmail format: {request.ToEmail}");
            }

            if (!RegexUtilities.IsValidEmail(request.FromEmail))
            {
                throw new ArgumentException($"Invalid FromEmail format: {request.FromEmail}");
            }
        }

        private static void ValidateSearchRequest(EmailHistorySearchRequest searchRequest)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }

            if (searchRequest.PageNumber < 1)
            {
                throw new ArgumentException("PageNumber must be greater than 0", nameof(searchRequest.PageNumber));
            }

            if (searchRequest.PageSize < 1 || searchRequest.PageSize > 1000)
            {
                throw new ArgumentException("PageSize must be between 1 and 1000", nameof(searchRequest.PageSize));
            }
        }

        #endregion
    }
}
