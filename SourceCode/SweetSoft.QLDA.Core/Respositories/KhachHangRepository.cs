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
    public class KhachHangRepository : BaseRepository<TblKhachHang>
    {
        public KhachHangRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public override TblKhachHang GetById(Guid id)
        {
            return new Select()
                .From(TblKhachHang.Schema)
                .Where(TblKhachHang.IdKhachHangColumn).IsEqualTo(id)
                .And(TblKhachHang.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblKhachHang>();
        }

        public List<TblKhachHang> GetAllTblKhachHang()
        {
            Select select = new Select();
            select.From(TblKhachHang.Schema);
            select.And(TblKhachHang.DaXoaColumn).IsEqualTo(false);
            return select.ExecuteTypedList<TblKhachHang>();
        }
    }
}
