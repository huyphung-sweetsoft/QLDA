using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Interfaces
{
    public interface ISmtpService
    {
        Task SendEmailAsync(List<EmailHistory> emailHistories, List<EmailAddress> ccEmails,
         List<EmailAddress> bccEmails, List<Attachment> attachments);
        Task SendSingleEmailAsync(EmailHistory emailHistory, List<EmailAddress> ccEmails = null,
            List<EmailAddress> bccEmails = null, List<Attachment> attachments = null);
    }

}
