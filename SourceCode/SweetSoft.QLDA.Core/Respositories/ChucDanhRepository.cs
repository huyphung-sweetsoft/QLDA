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
    public class ChucDanhRepository : BaseRepository<TblChucDanh>
    {
        public ChucDanhRepository(AuditManager auditManager) : base(auditManager) { }
        public static List<TblChucDanh> GetListForDropdown()
        {
            return new Select().From(TblChucDanh.Schema)
                .Where(TblChucDanh.Columns.DaXoa).IsEqualTo(false)
                .And(TblChucDanh.Columns.KichHoat).IsEqualTo(true)
                .OrderAsc(TblChucDanh.Columns.ThuTuHienThi)
                .ExecuteTypedList<TblChucDanh>();
        }
    }
}
