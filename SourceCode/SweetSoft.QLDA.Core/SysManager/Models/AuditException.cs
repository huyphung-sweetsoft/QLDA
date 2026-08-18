using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class AuditException : Exception
    {
        public AuditException(string message) : base(message) { }
        public AuditException(string message, Exception innerException) : base(message, innerException) { }
    }
}
