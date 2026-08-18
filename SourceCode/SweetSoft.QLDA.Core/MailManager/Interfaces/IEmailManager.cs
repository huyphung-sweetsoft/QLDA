using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Interfaces
{
    public interface IEmailManager
    {
        Task<bool> SendEmailAsync(EmailRequest request, bool useBackgroundThread = false);
        Task<bool> SendEmailToMultipleRecipientsAsync(EmailData emailData, bool useBackgroundThread = false);
        Task SendEmailWithTemplateAsync(Guid? refId, EmailType refType, Guid customerId,
            string toEmail, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersBody, List<Attachment> attachments = null,
            bool useBackgroundThread = true);
        Task SendEmailWithTemplateAsync(Guid? refId, EmailType refType, Guid customerId,
            string toEmail, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersSubject, Dictionary<string, string> placeholdersBody,
            List<Attachment> attachments = null, bool useBackgroundThread = true);
        Task SendEmailWithTemplateToMultipleAsync(Guid? refId, EmailType refType,
            Dictionary<Guid, string> recipients, string templateKey, EmailFormatTypes formatType,
            Dictionary<string, string> placeholdersSubject, Dictionary<string, string> placeholdersBody,
            List<Attachment> attachments = null, bool useBackgroundThread = true);
        Task SendCustomEmailAsync(Guid? refId, EmailType refType,
            Dictionary<Guid, string> recipients, string subject, string messageBody,
            List<string> ccEmails = null, List<string> bccEmails = null,
            List<Attachment> attachments = null, bool useBackgroundThread = true);
    }
}
