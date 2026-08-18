using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Models
{
    public class EmailStatistics
    {
        public int TotalEmails { get; set; }
        public int SentEmails { get; set; }
        public int FailedEmails { get; set; }
        public int ReadEmails { get; set; }
        public int UnreadEmails { get; set; }
        public double DeliveryRate => TotalEmails > 0 ? (double)SentEmails / TotalEmails * 100 : 0;
        public double ReadRate => SentEmails > 0 ? (double)ReadEmails / SentEmails * 100 : 0;
    }
}
