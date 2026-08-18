using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.MailManager.Models
{

    public class EmailHistorySearchRequest
    {
        public Guid? RefId { get; set; }
        public EmailType? RefType { get; set; }
        public Guid? CustomerId { get; set; }
        public string SearchTerm { get; set; }
        public string Email { get; set; }
        public bool? IsSent { get; set; }
        public DateTime? SentDateFrom { get; set; }
        public DateTime? SentDateTo { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadDateFrom { get; set; }
        public DateTime? ReadDateTo { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDateFrom { get; set; }
        public DateTime? UpdatedDateTo { get; set; }
        public string OrderBy { get; set; } = "CreatedDate DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
