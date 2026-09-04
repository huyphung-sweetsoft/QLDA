using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.DataAccess
{
    public partial class TblNhanVien
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid RoleId { get; set; }
        public string Password { get; set; }
        public bool IsActivated { get; set; }
    }
}
