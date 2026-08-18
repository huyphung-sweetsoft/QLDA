using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Models
{
    public class EmailHistoryDto
    {
        public Guid Id { get; set; }
        public Guid RefId { get; set; }
        public string RefType { get; set; }
        public Guid CustomerId { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public List<EmailAddress> CcEmails { get; set; } = new List<EmailAddress>();
        public List<EmailAddress> BccEmails { get; set; } = new List<EmailAddress>();
        public DateTime CreatedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public int NumberOfSent { get; set; }
        public bool IsSent { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public string ErrorMessage { get; set; }
        public string CreatedUser { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
