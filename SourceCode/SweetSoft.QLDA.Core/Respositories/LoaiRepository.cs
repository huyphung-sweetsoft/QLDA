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

        public List<TblLoai> GetAllLoaiDuAn()
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo("DU_AN")
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }

        public List<TblLoai> GetAllLoaiKhachHang()
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo("KHACH_HANG")
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }

        public List<TblLoai> GetAllPhongBan()
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo("PHONG_BAN")
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }

        public List<TblLoai> GetAllChucDanh()
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo("CHUC_DANH")
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }
    }
}
