using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class AuditStatistics
    {
        public int TotalRecords { get; set; }
        public Dictionary<string, int> ActionTypeCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TopUsers { get; set; } = new Dictionary<string, int>();
    }
}
