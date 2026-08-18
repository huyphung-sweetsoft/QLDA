using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Configs
{
    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderEmail { get; set; }
        public int TimeoutMilliseconds { get; set; } = 30000;
        public int MaxRetryAttempts { get; set; } = 3;
        public int MaxRetryDelaySeconds { get; set; } = 300; // 5 minutes
    }
}
