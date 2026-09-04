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
    public class LoaiKhachHangRepository : BaseRepository<TblLoaiKhachHang>
    {
        public LoaiKhachHangRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public List<TblLoaiKhachHang> GetAllTblLoaiKhachHang()
        {
            return new Select()
                .From(TblLoaiKhachHang.Schema)
                .And(TblLoaiKhachHang.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblLoaiKhachHang>();
        }
    }
}
