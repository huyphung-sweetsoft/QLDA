using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailRequest
    {
        public Guid? SenderId { get; set; }
        public Guid? RefId { get; set; }
        public Guid CustomerId { get; set; }
        public EmailType RefType { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public bool IsTracking { get; set; }
        public List<EmailAddress> CcEmails { get; set; } = new List<EmailAddress>();
        public List<EmailAddress> BccEmails { get; set; } = new List<EmailAddress>();
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public string CreatedBy { get; set; }
    }
}
