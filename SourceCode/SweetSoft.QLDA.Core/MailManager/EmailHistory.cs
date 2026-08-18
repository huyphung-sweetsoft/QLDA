using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailHistory
    {
        public Guid Id { get; set; }
        public Guid? RefId { get; set; }
        public EmailType RefType { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SenderId { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string EmailContent { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
        public string BccEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public int NumberOfSent { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsSent { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public string ErrorMessage { get; set; }
    }
}
