using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SysManager.Models
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? RefId { get; set; }
        public string TableName { get; set; }
        public Guid? RecordId { get; set; }
        public string ActionType { get; set; }
        public string Changes { get; set; }
        public string ChangedBy { get; set; }
        public Guid? UserId { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
