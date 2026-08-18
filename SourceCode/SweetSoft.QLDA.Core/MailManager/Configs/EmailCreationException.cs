using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Configs
{
    public class EmailCreationException : Exception
    {
        public EmailCreationException(string message) : base(message) { }
        public EmailCreationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
