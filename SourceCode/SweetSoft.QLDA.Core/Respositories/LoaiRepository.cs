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
    public class LoaiRepository : BaseRepository<TblLoai>
    {
        public LoaiRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public override TblLoai GetById(Guid id)
        {
            return new Select()
                .From(TblLoai.Schema)
                .Where(TblLoai.IdLoaiColumn).IsEqualTo(id)
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoai>();
        }

        public List<TblLoai> GetByDoiTuong(string doiTuong)
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo(doiTuong)
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }
    }
}
