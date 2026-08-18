using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.DataAccess
{
    partial class AspnetUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid RoleId { get; set; }
    }
}
