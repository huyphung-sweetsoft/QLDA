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
    public class PhongBanRepository : BaseRepository<TblPhongBan>
    {
        public PhongBanRepository(AuditManager auditManager) : base(auditManager) { }
        public static List<TblPhongBan> GetListForDropdown()
        {
            return new Select().From(TblPhongBan.Schema)
                .Where(TblPhongBan.Columns.DaXoa).IsEqualTo(false)
                .And(TblPhongBan.Columns.KichHoat).IsEqualTo(true)
                .OrderAsc(TblPhongBan.Columns.ThuTuHienThi)
                .ExecuteTypedList<TblPhongBan>();
        }
    }
}
