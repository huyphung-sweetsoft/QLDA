using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager
{
    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsActivated { get; set; }
        public string CCEmail { get; set; }
        public string BCCEmail { get; set; }
    }
}
