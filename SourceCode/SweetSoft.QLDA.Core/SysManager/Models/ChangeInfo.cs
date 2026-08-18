using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class ChangeInfo
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PropertyType { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
