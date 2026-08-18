using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class ClientInfo
    {
        public string UserName { get; set; }
        public Guid? UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
