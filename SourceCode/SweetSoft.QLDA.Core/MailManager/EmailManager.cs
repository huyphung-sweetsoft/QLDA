//--------------------PROGRAMER LOGS------------------------
//Created by:
using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Interfaces;
using SweetSoft.QLDA.Core.MailManager.Repository;
using SweetSoft.QLDA.Core.SysManager.Interfaces;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailManager : BaseManager, IEmailManager
    {
        private readonly ISmtpService _smtpService;
        private readonly IEmailHistoryService _emailHistoryService;
        private readonly ISettingManager _settingManager;
        private readonly IEmailTemplateService _emailTemplateService;

        public static string BackendSenderName => "PM Quản Lý Dự Án";
        public static string FrontendSenderName => "PM Quản Lý Dự Án";

        public EmailManager(IAppContext applicationContext, IAuditManager auditManager = null) : base(applicationContext)
        {
            _settingManager = new SettingManager();
            _emailHistoryService = new EmailHistoryService(GetClientInfo(), auditManager, new EmailHistoryRepository());
            _smtpService = new SmtpService(GetClientInfo(), _settingManager, _emailHistoryService);
            _emailTemplateService = new EmailTemplateService();
        }

        // Constructor for dependency injection (optional)
        public EmailManager(IAppContext applicationContext, IAuditManager auditManager, ISmtpService smtpService, IEmailHistoryService emailHistoryService,
                           ISettingManager settingManager, IEmailTemplateService emailTemplateService) : base(applicationContext)
        {
            _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            _emailHistoryService = emailHistoryService ?? throw new ArgumentNullException(nameof(emailHistoryService));
            _settingManager = settingManager ?? throw new ArgumentNullException(nameof(settingManager));
            _emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        }
        #region Public Methods

        /// <summary>
        /// Gửi email đơn giản với một người nhận
        /// </summary>
        public async Task<bool> SendEmailAsync(EmailRequest request, bool useBackgroundThread = false)
        {
            ValidateEmailRequest(request);

            try
            {
                var recipients = new Dictionary<Guid, string>
            {
                { request.CustomerId, request.ToEmail }
            };

                var emailData = new EmailData
                {
                    RefId = request.RefId,
                    RefType = request.RefType,
                    Recipients = recipients,
                    Sender = request.Sender ?? FrontendSenderName,
                    Subject = request.Subject,
                    Content = request.Content,
                    FromEmail = request.FromEmail ?? _settingManager.GetSettingValue(SettingKeys.AdministratorEmail),
                    CcEmails = request.CcEmails ?? new List<EmailAddress>(),
                    BccEmails = request.BccEmails ?? new List<EmailAddress>(),
                    Attachments = request.Attachments ?? new List<Attachment>()
                };

                return await SendEmailInternalAsync(emailData, useBackgroundThread);
            }
            catch (Exception ex)
            {
                await _emailHistoryService.LogEmailErrorAsync(request.RefId, request.RefType,
                    request.CustomerId, request.ToEmail, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gửi email với nhiều người nhận và lưu lịch sử cho từng người
        /// </summary>
        public async Task<bool> SendEmailToMultipleRecipientsAsync(EmailData emailData, bool useBackgroundThread = false)
        {
            ValidateEmailData(emailData);

            try
            {
                return await SendEmailInternalAsync(emailData, useBackgroundThread);
            }
            catch (Exception ex)
            {
                foreach (var recipient in emailData.Recipients)
                {
                    await _emailHistoryService.LogEmailErrorAsync(emailData.RefId, emailData.RefType,
                        recipient.Key, recipient.Value, ex.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// Gửi email từ template với một người nhận
        /// </summary>
        public async Task SendEmailWithTemplateAsync(Guid? refId, EmailType refType, Guid customerId,
            string toEmail, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersBody, List<Attachment> attachments = null,
            bool useBackgroundThread = true)
        {
            await SendEmailWithTemplateAsync(refId, refType, customerId, toEmail, templateKey,
                formatType, null, placeholdersBody, attachments, useBackgroundThread);
        }

        /// <summary>
        /// Gửi email từ template với placeholder cho subject và body
        /// </summary>
        public async Task SendEmailWithTemplateAsync(Guid? refId, EmailType refType, Guid customerId,
            string toEmail, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersSubject, Dictionary<string, string> placeholdersBody,
            List<Attachment> attachments = null, bool useBackgroundThread = true)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateAsync(templateKey, formatType);
                ValidateTemplate(template);

                var recipients = new Dictionary<Guid, string>
            {
                { customerId, toEmail }
            };

                var emailData = CreateEmailDataFromTemplate(refId, refType, recipients, template,
                    placeholdersSubject, placeholdersBody, attachments);

                await SendEmailInternalAsync(emailData, useBackgroundThread);
            }
            catch (Exception ex)
            {
                await _emailHistoryService.LogEmailErrorAsync(refId, refType, customerId, toEmail, ex.Message);
                throw new EmailSendException($"Error sending template email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gửi email từ template với nhiều người nhận
        /// </summary>
        public async Task SendEmailWithTemplateToMultipleAsync(Guid? refId, EmailType refType,
            Dictionary<Guid, string> recipients, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersSubject, Dictionary<string, string> placeholdersBody,
            List<Attachment> attachments = null, bool useBackgroundThread = true)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateAsync(templateKey, formatType);
                ValidateTemplate(template);

                var emailData = CreateEmailDataFromTemplate(refId, refType, recipients, template,
                    placeholdersSubject, placeholdersBody, attachments);

                await SendEmailInternalAsync(emailData, useBackgroundThread);
            }
            catch (Exception ex)
            {
                foreach (var recipient in recipients)
                {
                    await _emailHistoryService.LogEmailErrorAsync(refId, refType,
                        recipient.Key, recipient.Value, ex.Message);
                }
                throw new EmailSendException($"Error sending template email to multiple recipients: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gửi email với nội dung tùy chỉnh
        /// </summary>
        public async Task SendCustomEmailAsync(Guid? refId, EmailType refType,
            Dictionary<Guid, string> recipients, string subject, string messageBody,
            List<string> ccEmails = null, List<string> bccEmails = null,
            List<Attachment> attachments = null, bool useBackgroundThread = true)
        {
            try
            {
                var emailData = new EmailData
                {
                    RefId = refId,
                    RefType = refType,
                    Recipients = recipients,
                    Sender = FrontendSenderName,
                    Subject = subject,
                    Content = messageBody,
                    FromEmail = _settingManager.GetSettingValue(SettingKeys.AdministratorEmail),
                    CcEmails = ParseEmailAddresses(ccEmails),
                    BccEmails = ParseEmailAddresses(bccEmails),
                    Attachments = attachments ?? new List<Attachment>()
                };

                await SendEmailInternalAsync(emailData, useBackgroundThread);
            }
            catch (Exception ex)
            {
                foreach (var recipient in recipients)
                {
                    await _emailHistoryService.LogEmailErrorAsync(refId, refType,
                        recipient.Key, recipient.Value, ex.Message);
                }
                throw new EmailSendException($"Error sending custom email: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task<bool> SendEmailInternalAsync(EmailData emailData, bool useBackgroundThread)
        {
            // Tạo lịch sử email cho từng người nhận
            var emailHistories = new List<EmailHistory>();
            foreach (var recipient in emailData.Recipients)
            {
                var history = await _emailHistoryService.CreateEmailHistoryAsync(new EmailRequest
                {
                    SenderId = _applicationContext?.UserId ?? Guid.Empty,
                    RefId = emailData.RefId,
                    RefType = emailData.RefType,
                    CustomerId = recipient.Key,
                    Sender = emailData.Sender,
                    Subject = emailData.Subject,
                    Content = emailData.Content,
                    FromEmail = emailData.FromEmail,
                    ToEmail = recipient.Value,
                    CcEmails = emailData.CcEmails,
                    BccEmails = emailData.BccEmails,
                    Attachments = emailData.Attachments
                });
                emailHistories.Add(history);
            }

            if (useBackgroundThread)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _smtpService.SendEmailAsync(emailHistories, emailData.CcEmails,
                            emailData.BccEmails, emailData.Attachments);
                    }
                    catch (Exception ex)
                    {
                        foreach (var history in emailHistories)
                        {
                            await _emailHistoryService.UpdateEmailHistoryStatusAsync(history.Id,
                                EmailStatus.Failed, ex.Message);
                        }
                    }
                });
            }
            else
            {
                await _smtpService.SendEmailAsync(emailHistories, emailData.CcEmails,
                    emailData.BccEmails, emailData.Attachments);
            }

            return true;
        }

        private EmailData CreateEmailDataFromTemplate(Guid? refId, EmailType refType,
            Dictionary<Guid, string> recipients, EmailTemplate template,
            Dictionary<string, string> placeholdersSubject, Dictionary<string, string> placeholdersBody,
            List<Attachment> attachments)
        {
            string processedBody = _emailTemplateService.ReplacePlaceholders(
                HttpUtility.HtmlDecode(template.Body), placeholdersBody);
            string processedSubject = _emailTemplateService.ReplacePlaceholders(
                template.Subject, placeholdersSubject);

            return new EmailData
            {
                RefId = refId,
                RefType = refType,
                Recipients = recipients,
                Sender = FrontendSenderName,
                Subject = processedSubject,
                Content = processedBody,
                FromEmail = _settingManager.GetSettingValue(SettingKeys.AdministratorEmail),
                CcEmails = ParseEmailAddresses(template.CCEmail, ','),
                BccEmails = ParseEmailAddresses(template.BCCEmail, ','),
                Attachments = attachments ?? new List<Attachment>()
            };
        }

        private List<EmailAddress> ParseEmailAddresses(string emailString, char separator = ',')
        {
            if (string.IsNullOrWhiteSpace(emailString))
                return new List<EmailAddress>();

            return emailString.Split(separator)
                .Select(email => email.Trim())
                .Where(email => !string.IsNullOrWhiteSpace(email) && RegexUtilities.IsValidEmail(email))
                .Select(email => new EmailAddress { Email = email })
                .ToList();
        }

        private List<EmailAddress> ParseEmailAddresses(List<string> emails)
        {
            if (emails == null || !emails.Any())
                return new List<EmailAddress>();

            return emails.Where(email => !string.IsNullOrWhiteSpace(email) && RegexUtilities.IsValidEmail(email))
                        .Select(email => new EmailAddress { Email = email.Trim() })
                        .ToList();
        }

        #endregion

        #region Validation Methods

        private void ValidateEmailRequest(EmailRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.FromEmail) || !RegexUtilities.IsValidEmail(request.FromEmail))
                throw new ArgumentException("Email address is invalid or missing", nameof(request.FromEmail));

            if (string.IsNullOrWhiteSpace(request.ToEmail))
                throw new ArgumentException("Email address is invalid or missing.", nameof(request.ToEmail));

            if (!RegexUtilities.IsValidEmail(request.ToEmail))
                throw new ArgumentException("Email address format is invalid.", nameof(request.ToEmail));
        }

        private void ValidateEmailData(EmailData emailData)
        {
            if (emailData == null)
                throw new ArgumentNullException(nameof(emailData));

            if (emailData.Recipients == null || !emailData.Recipients.Any())
                throw new ArgumentException("At least one recipient is required.", nameof(emailData.Recipients));

            foreach (var recipient in emailData.Recipients)
            {
                if (string.IsNullOrWhiteSpace(recipient.Value))
                    throw new ArgumentException($"Email address is invalid for recipient {recipient.Key}");

                if (!RegexUtilities.IsValidEmail(recipient.Value))
                    throw new ArgumentException($"Email address format is invalid for recipient {recipient.Key}: {recipient.Value}");
            }
        }

        private void ValidateTemplate(EmailTemplate template)
        {
            if (template == null)
                throw new EmailTemplateException("Email template not found");

            if (!template.IsActivated)
                throw new EmailTemplateException("Email template is inactive");
        }

        #endregion
    }

    #region Supporting Classes

    public class EmailData
    {
        public Guid? RefId { get; set; }
        public EmailType RefType { get; set; }
        public Dictionary<Guid, string> Recipients { get; set; } = new Dictionary<Guid, string>();
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string FromEmail { get; set; }
        public List<EmailAddress> CcEmails { get; set; } = new List<EmailAddress>();
        public List<EmailAddress> BccEmails { get; set; } = new List<EmailAddress>();
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

    public class EmailSendException : Exception
    {
        public EmailSendException(string message) : base(message) { }
        public EmailSendException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EmailTemplateException : Exception
    {
        public EmailTemplateException(string message) : base(message) { }
        public EmailTemplateException(string message, Exception innerException) : base(message, innerException) { }
    }

    public enum EmailStatus
    {
        Pending,
        Sent,
        Failed
    }

    #endregion
}
