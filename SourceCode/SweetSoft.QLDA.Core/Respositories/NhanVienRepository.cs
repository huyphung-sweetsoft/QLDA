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
    public class NhanVienRepository : BaseRepository<TblNhanVien>
    {
        public NhanVienRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public override TblNhanVien GetById(Guid id)
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdNhanVienColumn).IsEqualTo(id)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle <TblNhanVien>();
        }

        public List<TblNhanVien> GetAllTblNhanVien()
        {
            Select select = new Select();
            select.From(TblNhanVien.Schema);
            select.And(TblNhanVien.DaXoaColumn).IsEqualTo(false);
            return select.ExecuteTypedList<TblNhanVien>();
        }
    }
}
