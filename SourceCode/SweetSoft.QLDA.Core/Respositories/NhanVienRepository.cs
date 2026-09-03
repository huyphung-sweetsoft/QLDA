using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class NhanVienRepository : BaseRepository<AspnetUser>
    {
        public NhanVienRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public override AspnetUser GetById(Guid id)
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(id)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle <AspnetUser>();
        }

        public List<AspnetUser> GetAllTblNhanVien()
        {
            Select select = new Select();
            select.From(AspnetUser.Schema);
            select.And(AspnetUser.IsDeletedColumn).IsEqualTo(false);
            return select.ExecuteTypedList<AspnetUser>();
        }
    }
}
