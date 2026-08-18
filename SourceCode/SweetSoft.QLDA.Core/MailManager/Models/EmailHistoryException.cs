using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Models
{
    public class EmailHistoryException : Exception
    {
        public EmailHistoryException(string message) : base(message) { }
        public EmailHistoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
