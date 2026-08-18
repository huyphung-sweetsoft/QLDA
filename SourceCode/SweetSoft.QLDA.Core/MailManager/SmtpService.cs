using Newtonsoft.Json;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Configs;
using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.SysManager.Interfaces;
using SweetSoft.QLDA.Core.SysManager.Models;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class SmtpService : ISmtpService
    {
        private readonly ClientInfo _clientInfo;
        private readonly ISettingManager _settingManager;
        private readonly IEmailHistoryService _emailHistoryService;
        private readonly SmtpConfiguration _smtpConfig;

        public SmtpService(ClientInfo clientInfo,
                         ISettingManager settingManager,
                          IEmailHistoryService emailHistoryService)
        {
            _clientInfo = clientInfo ?? new ClientInfo();
            _settingManager = settingManager ?? throw new ArgumentNullException(nameof(settingManager));
            _emailHistoryService = emailHistoryService ?? throw new ArgumentNullException(nameof(emailHistoryService));
            _smtpConfig = LoadSmtpConfiguration();
        }

        #region Public Methods

        public async Task SendEmailAsync(List<EmailHistory> emailHistories,
            List<EmailAddress> ccEmails, List<EmailAddress> bccEmails, List<Attachment> attachments)
        {
            if (emailHistories == null || !emailHistories.Any())
            {
                SysLogger.LogError("No email histories provided for sending");
                return;
            }

            var tasks = emailHistories.Select(async emailHistory =>
            {
                try
                {
                    await SendSingleEmailAsync(emailHistory, ccEmails, bccEmails, attachments);
                    await UpdateEmailHistorySuccess(emailHistory.Id);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to send email for history ID: {HistoryId}", emailHistory.Id);
                    await HandleEmailFailure(emailHistory, ex);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task SendSingleEmailAsync(EmailHistory emailHistory,
            List<EmailAddress> ccEmails = null, List<EmailAddress> bccEmails = null,
            List<Attachment> attachments = null)
        {
            if (emailHistory == null)
                throw new ArgumentNullException(nameof(emailHistory));

            ValidateEmailHistory(emailHistory);

            try
            {
                using (var smtpClient = CreateSmtpClient())
                {
                    using (var message = await CreateMailMessageAsync(emailHistory, ccEmails, bccEmails, attachments))
                    {
                        await smtpClient.SendMailAsync(message);

                        SysLogger.LogInfo("Email sent successfully to {ToEmail} with subject: {Subject}",
                            emailHistory.ToEmail, emailHistory.Subject);
                    }
                }

            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to send email to {ToEmail}", emailHistory.ToEmail);
                throw new EmailSendException($"Failed to send email to {emailHistory.ToEmail}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Methods

        private SmtpClient CreateSmtpClient()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
                ServicePointManager.Expect100Continue = true;

                var smtpClient = new SmtpClient
                {
                    Host = _smtpConfig.Host,
                    Port = _smtpConfig.Port,
                    EnableSsl = _smtpConfig.EnableSsl,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = _smtpConfig.TimeoutMilliseconds,
                    Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password)
                };

                return smtpClient;
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to create SMTP client");
                throw new SmtpConfigurationException("Failed to create SMTP client", ex);
            }
        }

        private async Task<MailMessage> CreateMailMessageAsync(EmailHistory emailHistory,
            List<EmailAddress> ccEmails, List<EmailAddress> bccEmails, List<Attachment> attachments)
        {
            try
            {
                var message = new MailMessage
                {
                    From = CreateMailAddress(_smtpConfig.SenderEmail, emailHistory.Sender),
                    ReplyTo = CreateMailAddress(emailHistory.FromEmail, emailHistory.Sender),
                    Subject = emailHistory.Subject,
                    Body = ProcessEmailContent(emailHistory.EmailContent),
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                // Add primary recipient
                message.To.Add(CreateMailAddress(emailHistory.ToEmail));

                // Add CC recipients
                await AddRecipientsAsync(message.CC, ccEmails, "CC");

                // Add BCC recipients
                await AddRecipientsAsync(message.Bcc, bccEmails, "BCC");

                // Add attachments
                AddAttachments(message, attachments);

                return message;
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to create mail message for {ToEmail}", emailHistory.ToEmail);
                throw new EmailCreationException($"Failed to create mail message: {ex.Message}", ex);
            }
        }

        private MailAddress CreateMailAddress(string email, string displayName = null)
        {
            if (!RegexUtilities.IsValidEmail(email))
                throw new ArgumentException($"Invalid email address: {email}");

            if (string.IsNullOrWhiteSpace(displayName))
                return new MailAddress(email);

            // Encode display name for UTF-8 support
            var encodedName = $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName))}?=";
            return new MailAddress(email, encodedName);
        }

        private Task AddRecipientsAsync(MailAddressCollection collection,
            List<EmailAddress> recipients, string recipientType)
        {
            if (recipients == null || !recipients.Any())
                return Task.CompletedTask;

            var validRecipients = recipients.Where(r => !string.IsNullOrWhiteSpace(r?.Email) &&
                                                       RegexUtilities.IsValidEmail(r.Email))
                                          .ToList();

            foreach (var recipient in validRecipients)
            {
                try
                {
                    var displayName = string.IsNullOrWhiteSpace(recipient.Name) ? recipient.Email : recipient.Name;
                    collection.Add(CreateMailAddress(recipient.Email, displayName));
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to add {RecipientType} recipient {Email}",
                        recipientType, recipient.Email);
                }
            }

            SysLogger.LogDebug("Added {Count} {RecipientType} recipients", validRecipients.Count, recipientType);
            return Task.CompletedTask;
        }

        private void AddAttachments(MailMessage message, List<Attachment> attachments)
        {
            if (attachments == null || !attachments.Any())
                return;

            foreach (var attachment in attachments.Where(a => a != null))
            {
                try
                {
                    message.Attachments.Add(attachment);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to add attachment: {AttachmentName}",
                        attachment.Name ?? "Unknown");
                }
            }

            SysLogger.LogDebug("Added {AttachmentCount} attachments to email",
                message.Attachments.Count);
        }

        private string ProcessEmailContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            try
            {
                return HttpUtility.HtmlDecode(content);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to decode email content, using original");
                return content;
            }
        }

        private async Task HandleEmailFailure(EmailHistory emailHistory, Exception exception)
        {
            try
            {
                var shouldRetry = ShouldRetryEmail(emailHistory, exception);

                if (shouldRetry)
                {
                    await ScheduleEmailRetry(emailHistory, exception);
                }
                else
                {
                    await UpdateEmailHistoryFailure(emailHistory.Id, exception.Message);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Error handling email failure for history ID: {HistoryId}",
                    emailHistory.Id);
            }
        }

        private bool ShouldRetryEmail(EmailHistory emailHistory, Exception exception)
        {
            // Don't retry for authentication or configuration errors
            if (exception is SmtpException smtpEx)
            {
                var statusCode = smtpEx.StatusCode;
                if (statusCode == SmtpStatusCode.MailboxBusy ||
                    statusCode == SmtpStatusCode.TransactionFailed ||
                    statusCode == SmtpStatusCode.InsufficientStorage)
                {
                    return emailHistory.NumberOfSent < _smtpConfig.MaxRetryAttempts;
                }

                // Don't retry for permanent failures
                if (statusCode == SmtpStatusCode.MailboxUnavailable ||
                    statusCode == SmtpStatusCode.UserNotLocalWillForward ||
                    statusCode == SmtpStatusCode.ExceededStorageAllocation)
                {
                    return false;
                }
            }

            return emailHistory.NumberOfSent < _smtpConfig.MaxRetryAttempts;
        }

        private async Task ScheduleEmailRetry(EmailHistory emailHistory, Exception exception)
        {
            try
            {
                var retryDelay = CalculateRetryDelay(emailHistory.NumberOfSent);

                await _emailHistoryService.UpdateEmailHistoryStatusAsync(
                    emailHistory.Id,
                    EmailStatus.Pending,
                    $"Retry scheduled after {retryDelay.TotalSeconds} seconds. Error: {exception.Message}");

                // Schedule retry using Task.Delay with exponential backoff
                _ = Task.Delay(retryDelay).ContinueWith(async _ =>
                {
                    await RetryEmailSend(emailHistory.Id);
                });

                SysLogger.LogInfo("Email retry scheduled for history ID: {HistoryId}, attempt: {AttemptNumber}",
                    emailHistory.Id, emailHistory.NumberOfSent + 1);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to schedule email retry for history ID: {HistoryId}",
                    emailHistory.Id);
            }
        }

        private async Task RetryEmailSend(Guid historyId)
        {
            try
            {
                var emailHistoryService = new EmailHistoryService(_clientInfo);
                var emailHistory = await emailHistoryService.GetEmailHistoryByIdAsync(historyId);

                if (emailHistory == null)
                {
                    SysLogger.LogError("Cannot retry email. History not found for ID: {HistoryId}", historyId);
                    return;
                }

                emailHistory.NumberOfSent++;
                await emailHistoryService.SaveAsync(emailHistory);

                // Parse CC and BCC emails from stored JSON
                var ccEmails = ParseStoredEmailAddresses(emailHistory.CcEmail);
                var bccEmails = ParseStoredEmailAddresses(emailHistory.BccEmail);

                await SendSingleEmailAsync(emailHistory, ccEmails, bccEmails);
                await UpdateEmailHistorySuccess(historyId);

                SysLogger.LogInfo("Email retry successful for history ID: {HistoryId}", historyId);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Email retry failed for history ID: {HistoryId}", historyId);
                await UpdateEmailHistoryFailure(historyId, ex.Message);
            }
        }

        private List<EmailAddress> ParseStoredEmailAddresses(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return new List<EmailAddress>();

            try
            {
                return JsonConvert.DeserializeObject<List<EmailAddress>>(jsonString) ?? new List<EmailAddress>();
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to parse stored email addresses: {JsonString}", jsonString);
                return new List<EmailAddress>();
            }
        }

        private TimeSpan CalculateRetryDelay(int attemptNumber)
        {
            // Exponential backoff: 1s, 2s, 4s, 8s, 16s...
            var baseDelaySeconds = Math.Pow(2, attemptNumber);
            var maxDelaySeconds = _smtpConfig.MaxRetryDelaySeconds;

            var delaySeconds = Math.Min(baseDelaySeconds, maxDelaySeconds);
            return TimeSpan.FromSeconds(delaySeconds);
        }

        private async Task UpdateEmailHistorySuccess(Guid historyId)
        {
            await _emailHistoryService.UpdateEmailHistoryStatusAsync(
                historyId, EmailStatus.Sent, "Email sent successfully");
        }

        private async Task UpdateEmailHistoryFailure(Guid historyId, string errorMessage)
        {
            await _emailHistoryService.UpdateEmailHistoryStatusAsync(
                historyId, EmailStatus.Failed, errorMessage);
        }

        private SmtpConfiguration LoadSmtpConfiguration()
        {
            try
            {
                return new SmtpConfiguration
                {
                    Host = _settingManager.GetSettingValue(SettingKeys.SmtpMailServerAddress),
                    Port = _settingManager.GetSettingValueInt(SettingKeys.SmtpPort, 25),
                    EnableSsl = _settingManager.GetSettingValueBoolean(SettingKeys.SmtpUsingSSL),
                    Username = _settingManager.GetSettingValue(SettingKeys.SmtpSenderAccount),
                    Password = _settingManager.GetSettingValueDecryptAES(SettingKeys.SmtpSenderPassword),
                    SenderEmail = _settingManager.GetSettingValue(SettingKeys.SmtpSenderEmail),
                    TimeoutMilliseconds = _settingManager.GetSettingValueInt(SettingKeys.SmtpTimeoutMilliseconds, 30000),
                    MaxRetryAttempts = _settingManager.GetSettingValueInt(SettingKeys.SmtpMaxRetryAttempts, 3),
                    MaxRetryDelaySeconds = _settingManager.GetSettingValueInt(SettingKeys.SmtpMaxRetryDelaySeconds, 300)
                };
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Failed to load SMTP configuration");
                throw new SmtpConfigurationException("Failed to load SMTP configuration", ex);
            }
        }

        private void ValidateEmailHistory(EmailHistory emailHistory)
        {
            if (string.IsNullOrWhiteSpace(emailHistory.ToEmail))
                throw new ArgumentException("ToEmail is required", nameof(emailHistory));

            if (string.IsNullOrWhiteSpace(emailHistory.FromEmail))
                throw new ArgumentException("FromEmail is required", nameof(emailHistory));

            if (!RegexUtilities.IsValidEmail(emailHistory.ToEmail))
                throw new ArgumentException($"Invalid email address: {emailHistory.ToEmail}", nameof(emailHistory));

            if (string.IsNullOrWhiteSpace(emailHistory.Subject))
                throw new ArgumentException("Subject is required", nameof(emailHistory));
        }

        #endregion
    }
}
