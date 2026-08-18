using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Configs
{
    public class SmtpConfigurationException : Exception
    {
        public SmtpConfigurationException(string message) : base(message) { }
        public SmtpConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
