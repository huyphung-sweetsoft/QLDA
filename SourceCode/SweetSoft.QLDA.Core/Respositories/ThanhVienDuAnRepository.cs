using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class ThanhVienDuAnRepository : BaseRepository<TblThanhVienDuAn>
    {
        public ThanhVienDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblThanhVienDuAn Save(TblThanhVienDuAn item)
        {
            item.Save();
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, Guid.Parse(item.GetColumnValue("IdDuAn").ToString())).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblDuAn");
                }
            });
            return item;
        }
        // TRUY VẤN BẢNG PHẲNG: Lấy toàn bộ Dự án, Phase và Task của 1 nhân viên
        public DataTable GetProjectDetailByNhanVien(Guid idNhanVien)
        {
            // BỌC CAST( AS VARCHAR(50)) VÀO TẤT CẢ CÁC KHÓA JOIN
            // ĐỂ NGĂN SQL SERVER BÁO LỖI KHI GẶP CHUỖI RỖNG HOẶC CHUỖI SAI FORMAT GUID
            string sql = $@"
            SELECT 
                da.IdDuAn, da.MaDuAn, da.TenDuAn, da.TrangThai AS TrangThaiDuAn,
                vt.TenVaiTro AS VaiTro,
                ISNULL(tp.IdCongViec, tc.IdCongViec) AS IdPhase,
                ISNULL(tp.TenCongViec, tc.TenCongViec) AS TenPhase,
                tc.IdCongViec AS IdTask, tc.MaCongViec AS MaTask, tc.TenCongViec AS TenTask,
                tc.NgayBatDau, tc.NgayKetThuc, tc.ThoiHanNgay, 
                ISNULL(ut.DiemUuTien, 1) AS DiemUuTien, 
                tc.TrangThai AS TrangThaiTask
            FROM [dbo].[TblThanhVienDuAn] tv
            
            INNER JOIN [dbo].[TblDuAn] da ON CAST(tv.IdDuAn AS VARCHAR(50)) = CAST(da.IdDuAn AS VARCHAR(50))
            INNER JOIN [dbo].[TblCongViec_NhanVien] cvnv ON CAST(tv.IdNhanVien AS VARCHAR(50)) = CAST(cvnv.IdNhanVien AS VARCHAR(50))
            INNER JOIN [dbo].[TblCongViec] tc ON CAST(cvnv.IdCongViec AS VARCHAR(50)) = CAST(tc.IdCongViec AS VARCHAR(50)) 
                                             AND CAST(tc.IdDuAn AS VARCHAR(50)) = CAST(da.IdDuAn AS VARCHAR(50))
            
            LEFT JOIN [dbo].[TblVaiTroDuAn] vt ON CAST(tv.IdVaiTroDuAn AS VARCHAR(50)) = CAST(vt.IdVaiTro AS VARCHAR(50))
            LEFT JOIN [dbo].[TblCongViec] tp ON CAST(tc.IdCongViecCha AS VARCHAR(50)) = CAST(tp.IdCongViec AS VARCHAR(50))
            LEFT JOIN [dbo].[TblDoUuTien] ut ON CAST(tc.IdDoUuTien AS VARCHAR(50)) = CAST(ut.IdDoUuTien AS VARCHAR(50))
            
            WHERE CAST(tv.IdNhanVien AS VARCHAR(50)) = '{idNhanVien}'
              AND tv.DaXoa = 0 
              AND da.DaXoa = 0 
              AND tc.DaXoa = 0
              AND tc.NgayBatDau IS NOT NULL 
              AND tc.NgayKetThuc IS NOT NULL
            ORDER BY da.NgayTao DESC, tp.MaCongViec, tc.NgayBatDau ASC;
            ";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            if (reader == null) return null;
            DataTable dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public List<TblThanhVienDuAn> GetByIdDuAn(Guid idDuAn)
        {
            Select select = new Select();
            select.From(TblThanhVienDuAn.Schema);
            select.Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn);
            return select.ExecuteTypedList<TblThanhVienDuAn>();
        }
        //Hàm này sẽ bổ sung thêm 1 tham số và 1 điều kiện And
        //Lý do: Giả sử khi đã chọn nhân viên A làm PM, thì khi vào danh sách nhân viên để chọn đám nhân viên thêm vào dự án,
        //sẽ tồn tại trường hợp thk nhân viên A cũng nằm trong danh sách đó, và nếu chọn nó để thêm vào thì cái vai trò của nó sẽ bị đè lên
        //Từ nhân viên A - Pm -> Nhân viên A - nhân viên
        //Nên bổ sung thêm 1 điều kiện check vai trò, lúc này vì vai trò khác nhau nên cái hàm Get này sẽ bị rỗng, và vì bị rỗng nên nó trả về null
        //Và trả về null nên nó sẽ đẩy sang thằng thêm mới thay vì cập nhật
        //T đã cài 2 lớp để tránh việc PM sẽ rơi vào cái danh sách nhân viên được chọn rồi, nên thực sự cái này ko cần đổi
        //Trong trường hợp dở người mà bug hay gì đó thk PM vẫn bị dính vào danh sách nhân viên chọn vào dự án thì thay vì nó đè vai trò của thk PM về thành nhân viên
        //Gây lỗi dự án thì nó sẽ tạo thêm 1 dòng (thêm mới ấy) thk PM này với Vai trò là nhân viên
        //Nói chung là backup, ko ảnh hưởng gì cả
        public TblThanhVienDuAn GetNhanVienIsActiveInDuAn(Guid idNhanVien, Guid idDuAn, Guid idVaiTro)
        {
            return new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(idNhanVien)
                .And(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro)//dòng thêm vào
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhVienDuAn>();
        }
        //THêm mới 3 hàm sau
        //1. Hàm này dùng đến lấy danh sách dựa vào id dự án và vai trò và có ngoại lệ (except 1 đứa),
        //mục đích là lấy danh sách với vai trò là PM để tiến hành cho chức năng đỏi PM từ nhân viên A sang nv B
        //Mỗi dự án chỉ có 1 PM nên hàm này nếu viết dạng lấy 1 cũng được nhưng viết list cho chắc để tránh trường hợp db lỗi 
        //làm tồn tại 2 PM đang hoạt động trên cùng 1 dự án. qua Manager nói rõ hơn
        //Đổi về lấy 1 nếu muốn 
        public List<TblThanhVienDuAn> GetByIdDuAnAndVaiTroExcept(Guid idDuAn, Guid idVaiTro, Guid idNhanVienGiuLai)
        {
            return new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .And(TblThanhVienDuAn.IdNhanVienColumn).IsNotEqualTo(idNhanVienGiuLai)
                .ExecuteTypedList<TblThanhVienDuAn>();
        }
        //2. Hàm này lấy idNhanVien để dùng cho chức năng edit, lấy list id đó để đánh tích vô mấy cái checkbox 
        public List<Guid> GetIdNhanVienByDuAnAndVaiTro(Guid idDuAn, Guid idVaiTro)
        {
            List<TblThanhVienDuAn> list = new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblThanhVienDuAn>();

            return list.Where(x => x.IdNhanVien.HasValue)
                .Select(x => x.IdNhanVien.Value)
                .ToList();
        }
    }
}
