using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class AuditConfiguration
    {
        public bool IsAuditEnabled { get; set; } = true;
        public bool ThrowOnAuditFailure { get; set; } = false;
        public int MaxChangesPerLog { get; set; } = 100;
        public TimeSpan AuditRetentionPeriod { get; set; } = TimeSpan.FromDays(365);
    }
}
