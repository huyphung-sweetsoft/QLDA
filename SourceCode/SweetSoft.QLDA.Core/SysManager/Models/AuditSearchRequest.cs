using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class AuditSearchRequest
    {
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public string SearchTerm { get; set; }
        public string IPAddress { get; set; }
        public string ChangedBy { get; set; }
        public string ActionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TableName { get; set; }
        public Guid? RecordId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "ChangedAt DESC";
    }
}
