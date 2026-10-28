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
    public class ThanhVienDuAnRepository : BaseRepository<TblThanhVienDuAn> {
        public ThanhVienDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public TblThanhVienDuAn Insert(TblThanhVienDuAn item, string description = null)
        {
            Guid id = Guid.Parse(item.GetColumnValue("IdThanhVienDuAn").ToString());
            Guid idDuAn = Guid.Parse(item.GetColumnValue("IdDuAn").ToString());
            item.Save();
            Task.Run(async () => {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao, idDuAn, GetNhanVienDisplayName(item.IdNhanVien.Value), description).ConfigureAwait(false);
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
        // Lấy cả dự án đã tham gia và dự án có lịch sử KBG 2.0.
        // KHÔNG JOIN số giờ vào danh sách dự án để tránh nhân bản tổng do nhiều vai trò / task.
        private const string SqlDuAnCuaNhanVien = @"
;WITH ScopeDuAn AS (
 SELECT d.IdDuAn,d.MaDuAn,d.TenDuAn,d.TrangThai AS TrangThaiDuAn,
        d.NgayBatDau AS ProjectStartDate,d.NgayDuKienHoanThanh AS ProjectEndDate,
        d.NgayHoanThanhThucTe AS ProjectActualEndDate,
        CASE WHEN EXISTS(SELECT 1 FROM dbo.TblThanhVienDuAn tv
                WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId AND ISNULL(tv.DaXoa,0)=0)
             THEN 1 ELSE 0 END AS IsCurrentMember,
        (SELECT TOP (1) v.TenVaiTro FROM dbo.TblThanhVienDuAn tv
         LEFT JOIN dbo.TblVaiTroDuAn v ON v.IdVaiTroDuAn=tv.IdVaiTroDuAn
         WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId
         ORDER BY CASE WHEN ISNULL(tv.DaXoa,0)=0 THEN 0 ELSE 1 END,tv.NgayTao DESC) AS VaiTro
 FROM dbo.TblDuAn d
 WHERE ISNULL(d.DaXoa,0)=0 AND
 (EXISTS (SELECT 1 FROM dbo.TblThanhVienDuAn tv WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=d.IdDuAn AND p.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat x WHERE x.IdDuAn=d.IdDuAn AND x.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblCongViec t JOIN dbo.TblCongViec_NhanVien a ON a.IdCongViec=t.IdCongViec
            WHERE t.IdDuAn=d.IdDuAn AND a.IdNhanVien=@EmpId))
)
SELECT * FROM ScopeDuAn WHERE
(@SearchStatusId IS NULL OR TrangThaiDuAn=@SearchStatusId)
AND (@SearchKeyword=N'' OR TenDuAn LIKE N'%'+@SearchKeyword+N'%' OR MaDuAn LIKE N'%'+@SearchKeyword+N'%')
ORDER BY ProjectStartDate DESC,MaDuAn ASC";

        public DataTable GetDanhSachDuAnCuaNhanVienData(Guid idNhanVien, string keyword, byte? statusId)
        {
            QueryCommand cmd=new QueryCommand(SqlDuAnCuaNhanVien, DataService.Provider.Name);
            cmd.AddParameter("@EmpId",idNhanVien,DbType.Guid);
            cmd.AddParameter("@SearchKeyword",(keyword??string.Empty).Trim(),DbType.String);
            cmd.AddParameter("@SearchStatusId",statusId.HasValue?(object)statusId.Value:DBNull.Value,DbType.Byte);
            DataSet ds=DataService.GetDataSet(cmd);
            return ds!=null && ds.Tables.Count>0?ds.Tables[0]:new DataTable();
        }

        // Chi tiet cong viec theo danh sach PHANG: truc tiep duoc giao HOAC co lich su gio/doi soat.
        // Khong phu thuoc TblGiaiDoanDuAn, IdGiaiDoanDuAn, ma WBS hay cay cha-con.
        public DataTable GetChiTietDuAnCuaNhanVienData(Guid idNhanVien, Guid idDuAn)
        {
            const string sql = @"
;WITH Project AS (
 SELECT d.IdDuAn,d.MaDuAn,d.TenDuAn,d.TrangThai AS TrangThaiDuAn,
        d.NgayBatDau AS ProjectStartDate,d.NgayDuKienHoanThanh AS ProjectEndDate,
        d.NgayHoanThanhThucTe AS ProjectActualEndDate,
        CASE WHEN EXISTS (SELECT 1 FROM dbo.TblThanhVienDuAn tv
                          WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId AND ISNULL(tv.DaXoa,0)=0)
             THEN 1 ELSE 0 END AS IsCurrentMember,
        (SELECT TOP (1) v.TenVaiTro FROM dbo.TblThanhVienDuAn tv
         LEFT JOIN dbo.TblVaiTroDuAn v ON v.IdVaiTroDuAn=tv.IdVaiTroDuAn
         WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId
         ORDER BY CASE WHEN ISNULL(tv.DaXoa,0)=0 THEN 0 ELSE 1 END,tv.NgayTao DESC) AS VaiTro
 FROM dbo.TblDuAn d WHERE d.IdDuAn=@ProjectId AND ISNULL(d.DaXoa,0)=0 AND
 (EXISTS(SELECT 1 FROM dbo.TblThanhVienDuAn tv WHERE tv.IdDuAn=d.IdDuAn AND tv.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=d.IdDuAn AND p.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=d.IdDuAn AND s.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblCongViec t JOIN dbo.TblCongViec_NhanVien a ON a.IdCongViec=t.IdCongViec
            WHERE t.IdDuAn=d.IdDuAn AND a.IdNhanVien=@EmpId))
), MyTasks AS (
 SELECT t.IdDuAn,t.IdCongViec,t.MaCongViec,t.TenCongViec,t.NgayBatDau,t.NgayKetThuc,
        t.NgayHoanThanhThucTe,t.ThoiHanNgay,t.TrangThai,t.LyDoTre,t.IdDoUuTien,
        ISNULL(t.DaXoa,0) AS IsArchivedTask,
        CASE WHEN EXISTS(SELECT 1 FROM dbo.TblCongViec_NhanVien a
                         WHERE a.IdCongViec=t.IdCongViec AND a.IdNhanVien=@EmpId)
             THEN 1 ELSE 0 END AS IsCurrentAssignee
 FROM dbo.TblCongViec t
 WHERE t.IdDuAn=@ProjectId AND (
     EXISTS(SELECT 1 FROM dbo.TblCongViec_NhanVien a
            WHERE a.IdCongViec=t.IdCongViec AND a.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
            WHERE p.IdDuAn=t.IdDuAn AND p.IdCongViec=t.IdCongViec AND p.IdNhanVien=@EmpId)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
            WHERE s.IdDuAn=t.IdDuAn AND s.IdCongViec=t.IdCongViec AND s.IdNhanVien=@EmpId))
)
SELECT d.*,t.IdCongViec AS IdTask,t.MaCongViec AS MaTask,t.TenCongViec AS TenTask,
       t.NgayBatDau,t.NgayKetThuc,t.NgayHoanThanhThucTe,
       ISNULL(t.ThoiHanNgay,0) AS ThoiHanNgay,
       ISNULL(u.DiemUuTien,1) AS DiemUuTien,ISNULL(u.TenDoUuTien,N'Thấp') AS TenDoUuTien,
       t.TrangThai AS TrangThaiTask,t.LyDoTre,t.IsCurrentAssignee,t.IsArchivedTask
FROM Project d
LEFT JOIN MyTasks t ON t.IdDuAn=d.IdDuAn
LEFT JOIN dbo.TblDoUuTien u ON u.IdDoUuTien=t.IdDoUuTien
ORDER BY t.NgayKetThuc,t.MaCongViec,t.IdCongViec;";
            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);
            cmd.AddParameter("@ProjectId", idDuAn, DbType.Guid);
            DataSet ds = DataService.GetDataSet(cmd);
            return ds != null && ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        /// <summary>
        /// Một dòng cho mỗi người/công việc, bao gồm người đã bị gỡ phân công.
        /// Giờ hệ thống chỉ lấy bảng KhoangTinh (KHÔNG tính lại từ thời gian mở phiên).
        /// Sự kiện gần nhất quyết định trạng thái PM chốt; dữ liệu tự khai xét theo lượt hiện tại.
        /// </summary>
        // Gui ID du an da loc vao SQL, khong tong hop gio cua cac du an khong hien tren UI.
        // Tat ca tham so GUID duoc parameterize, khong noi GUID truc tiep vao SQL.
        public DataTable GetGioCongNhanVienV2TheoDanhSachDuAnData(Guid idNhanVien, IEnumerable<Guid> idDuAns)
        {
            var scope = idDuAns == null ? null : idDuAns.Where(x => x != Guid.Empty).Distinct().ToList();
            if (scope != null && scope.Count == 0) return new DataTable();
            if (scope != null && scope.Count > 1900)
                throw new ArgumentException("Danh sach du an vuot gioi han tham so SQL Server.", "idDuAns");
            string scopePredicate = scope == null ? string.Empty :
                " AND d.IdDuAn IN (" + string.Join(",", Enumerable.Range(0, scope.Count).Select(i => "@NvdScope" + i)) + ")";
            string sql = @"
;WITH DuAnScope AS (
 SELECT d.IdDuAn FROM dbo.TblDuAn d WHERE ISNULL(d.DaXoa,0)=0
 /*NVD_PROJECT_SCOPE*/
 AND (EXISTS(SELECT 1 FROM dbo.TblThanhVienDuAn v WHERE v.IdDuAn=d.IdDuAn AND v.IdNhanVien=@EmpId)
 OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=d.IdDuAn AND p.IdNhanVien=@EmpId)
 OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=d.IdDuAn AND s.IdNhanVien=@EmpId)
 OR EXISTS(SELECT 1 FROM dbo.TblCongViec t JOIN dbo.TblCongViec_NhanVien a ON a.IdCongViec=t.IdCongViec
           WHERE t.IdDuAn=d.IdDuAn AND a.IdNhanVien=@EmpId))
), Tasks AS (
 SELECT t.IdDuAn,t.IdCongViec,t.TrangThai,t.LanThucHien,t.DaXoa
 FROM dbo.TblCongViec t INNER JOIN DuAnScope d ON d.IdDuAn=t.IdDuAn
 WHERE ISNULL(t.DaXoa,0)=0 OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=t.IdDuAn AND p.IdCongViec=t.IdCongViec)
  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=t.IdDuAn AND s.IdCongViec=t.IdCongViec)
), People AS (
 SELECT t.IdDuAn,t.IdCongViec,a.IdNhanVien FROM Tasks t JOIN dbo.TblCongViec_NhanVien a ON a.IdCongViec=t.IdCongViec
 UNION
 SELECT p.IdDuAn,p.IdCongViec,p.IdNhanVien FROM dbo.TblKhaiBaoGioPhien p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 UNION
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
 WHERE s.LoaiSuKien IN(0,1,2,3,5)
), TimeByPerson AS (
 SELECT p.IdDuAn,p.IdCongViec,p.IdNhanVien,
 SUM(CASE WHEN p.LoaiGio=1 THEN CAST(k.SoGiay AS bigint) ELSE CAST(0 AS bigint) END) AS Thuong,
 SUM(CASE WHEN p.LoaiGio=2 THEN CAST(k.SoGiay AS bigint) ELSE CAST(0 AS bigint) END) AS TangCa,
 MAX(CASE WHEN p.KetThucLuc IS NULL THEN 1 ELSE 0 END) AS DangMo
 FROM dbo.TblKhaiBaoGioPhien p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
 GROUP BY p.IdDuAn,p.IdCongViec,p.IdNhanVien
), LastEvent AS (
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien,s.LoaiSuKien,s.IdPmChot,
 s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LanThucHien,
 ROW_NUMBER() OVER(PARTITION BY s.IdDuAn,s.IdCongViec,s.IdNhanVien
 ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS rn
 FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
), LastReport AS (
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien,s.LoaiSuKien,s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,s.LanThucHien,
 ROW_NUMBER() OVER(PARTITION BY s.IdDuAn,s.IdCongViec,s.IdNhanVien
 ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) rn
 FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
 WHERE s.LoaiSuKien IN(1,2,3,5)
)
SELECT t.IdDuAn,t.IdCongViec,t.TrangThai AS TrangThaiTask,
       p.IdNhanVien,ISNULL(x.Thuong,0) AS GiayThuong,ISNULL(x.TangCa,0) AS GiayTangCa,
       ISNULL(x.DangMo,0) AS CoPhienDangMo,
       CASE WHEN x.IdNhanVien IS NOT NULL THEN 1 ELSE 0 END AS CoPhien,
       CASE WHEN r.LanThucHien=t.LanThucHien AND r.LoaiSuKien IN(1,3,5) THEN r.SoPhutThuongTuKhai END AS PhutThuongTuKhai,
       CASE WHEN r.LanThucHien=t.LanThucHien AND r.LoaiSuKien IN(1,3,5) THEN r.SoPhutTangCaTuKhai END AS PhutTangCaTuKhai,
       CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0
           THEN e.SoPhutThuongPmChot END AS PhutThuongPmChot,
       CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0
           THEN e.SoPhutTangCaPmChot END AS PhutTangCaPmChot,
       CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0 THEN 1 ELSE 0 END AS DaPmChot,
       CASE WHEN EXISTS (SELECT 1 FROM LastEvent outsideEvent WHERE outsideEvent.IdDuAn=t.IdDuAn
           AND outsideEvent.IdCongViec=t.IdCongViec AND outsideEvent.rn=1 AND outsideEvent.LoaiSuKien=4
           AND NOT EXISTS(SELECT 1 FROM People authorized WHERE authorized.IdDuAn=t.IdDuAn
                 AND authorized.IdCongViec=t.IdCongViec AND authorized.IdNhanVien=outsideEvent.IdNhanVien))
           THEN 1 ELSE 0 END AS CoPmChotNgoaiDanhSach
FROM People p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
LEFT JOIN TimeByPerson x ON x.IdDuAn=p.IdDuAn AND x.IdCongViec=p.IdCongViec AND x.IdNhanVien=p.IdNhanVien
LEFT JOIN LastEvent e ON e.IdDuAn=p.IdDuAn AND e.IdCongViec=p.IdCongViec AND e.IdNhanVien=p.IdNhanVien AND e.rn=1
LEFT JOIN LastReport r ON r.IdDuAn=p.IdDuAn AND r.IdCongViec=p.IdCongViec AND r.IdNhanVien=p.IdNhanVien AND r.rn=1
ORDER BY t.IdDuAn,t.IdCongViec,p.IdNhanVien";
            sql = sql.Replace("/*NVD_PROJECT_SCOPE*/", scopePredicate);
            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);
            if (scope != null)
                for (int i = 0; i < scope.Count; i++)
                    cmd.AddParameter("@NvdScope" + i, scope[i], DbType.Guid);
            DataSet ds = DataService.GetDataSet(cmd);
            return ds!=null&&ds.Tables.Count>0?ds.Tables[0]:new DataTable();
        }

        /// <summary>
        /// UX V6: summary-only query, one row per project. Do not join report history for
        /// every person or recorded seconds for other persons when rendering list cards.
        /// Keeps the SAME person scope, latest-event order and LanThucHien rules as detail.
        /// Project reconciliation still checks ALL people, not just @EmpId.
        /// The IDs come from the already-authorized list query, never directly from client input.
        /// </summary>
        public DataTable GetTongHopGioCongNhanVienV2DanhSachData(Guid idNhanVien, IEnumerable<Guid> idDuAns)
        {
            var scope = idDuAns == null ? new List<Guid>() : idDuAns.Where(x => x != Guid.Empty).Distinct().ToList();
            if (scope.Count == 0) return new DataTable();
            if (scope.Count > 1900) throw new ArgumentException("Danh sach du an vuot gioi han tham so SQL Server.", "idDuAns");
            string parameters = string.Join(",", Enumerable.Range(0,scope.Count).Select(i => "@NvdScope"+i));
            string sql = @"
;WITH DuAnScope AS (
 SELECT IdDuAn FROM dbo.TblDuAn WHERE ISNULL(DaXoa,0)=0 AND IdDuAn IN (/*SCOPE_IDS*/)
), Tasks AS (
 SELECT t.IdDuAn,t.IdCongViec,t.TrangThai,t.LanThucHien
 FROM dbo.TblCongViec t JOIN DuAnScope d ON d.IdDuAn=t.IdDuAn
 WHERE ISNULL(t.DaXoa,0)=0
    OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=t.IdDuAn AND p.IdCongViec=t.IdCongViec)
    OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=t.IdDuAn AND s.IdCongViec=t.IdCongViec)
), People AS (
 SELECT t.IdDuAn,t.IdCongViec,a.IdNhanVien FROM Tasks t JOIN dbo.TblCongViec_NhanVien a ON a.IdCongViec=t.IdCongViec
 UNION
 SELECT p.IdDuAn,p.IdCongViec,p.IdNhanVien FROM dbo.TblKhaiBaoGioPhien p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 UNION
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
 WHERE s.LoaiSuKien IN(0,1,2,3,5)
), LatestEvent AS (
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien,s.LoaiSuKien,s.IdPmChot,
 s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LanThucHien,
 ROW_NUMBER() OVER(PARTITION BY s.IdDuAn,s.IdCongViec,s.IdNhanVien
 ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS rn
 FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
), LatestReportEmp AS (
 SELECT s.IdDuAn,s.IdCongViec,s.IdNhanVien,s.LoaiSuKien,s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,s.LanThucHien,
 ROW_NUMBER() OVER(PARTITION BY s.IdDuAn,s.IdCongViec,s.IdNhanVien
 ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS rn
 FROM dbo.TblKhaiBaoGioDoiSoat s JOIN Tasks t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
 WHERE s.IdNhanVien=@EmpId AND s.LoaiSuKien IN(1,2,3,5)
), EmpTime AS (
 SELECT p.IdDuAn,p.IdCongViec,
 SUM(CASE WHEN p.LoaiGio=1 THEN CAST(k.SoGiay AS bigint) ELSE CAST(0 AS bigint) END) AS GiayThuong,
 SUM(CASE WHEN p.LoaiGio=2 THEN CAST(k.SoGiay AS bigint) ELSE CAST(0 AS bigint) END) AS GiayTangCa
 FROM dbo.TblKhaiBaoGioPhien p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
 WHERE p.IdNhanVien=@EmpId
 GROUP BY p.IdDuAn,p.IdCongViec
), OpenSessions AS (
 SELECT p.IdDuAn,p.IdCongViec FROM dbo.TblKhaiBaoGioPhien p
 JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 WHERE p.KetThucLuc IS NULL GROUP BY p.IdDuAn,p.IdCongViec
), PersonState AS (
 SELECT p.IdDuAn,p.IdCongViec,p.IdNhanVien,
 CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0 THEN 1 ELSE 0 END AS DaPmChot,
 CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0
           THEN CAST(e.SoPhutThuongPmChot AS bigint)+CAST(e.SoPhutTangCaPmChot AS bigint) ELSE CAST(0 AS bigint) END AS PmTong,
 CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0 THEN e.SoPhutThuongPmChot END AS PmThuong,
 CASE WHEN e.LoaiSuKien=4 AND e.IdPmChot IS NOT NULL AND e.LanThucHien=t.LanThucHien
           AND e.SoPhutThuongPmChot>=0 AND e.SoPhutTangCaPmChot>=0 THEN e.SoPhutTangCaPmChot END AS PmTangCa
 FROM People p JOIN Tasks t ON t.IdDuAn=p.IdDuAn AND t.IdCongViec=p.IdCongViec
 LEFT JOIN LatestEvent e ON e.IdDuAn=p.IdDuAn AND e.IdCongViec=p.IdCongViec AND e.IdNhanVien=p.IdNhanVien AND e.rn=1
), AlienPm AS (
 SELECT e.IdDuAn,e.IdCongViec
 FROM LatestEvent e WHERE e.rn=1 AND e.LoaiSuKien=4
 AND NOT EXISTS(SELECT 1 FROM People p WHERE p.IdDuAn=e.IdDuAn AND p.IdCongViec=e.IdCongViec AND p.IdNhanVien=e.IdNhanVien)
 GROUP BY e.IdDuAn,e.IdCongViec
), TaskCounts AS (
 SELECT t.IdDuAn,t.IdCongViec,t.TrangThai,
 COUNT(s.IdNhanVien) AS SoNguoi,
 SUM(CASE WHEN s.DaPmChot=1 THEN 1 ELSE 0 END) AS SoNguoiDaChot,
 SUM(ISNULL(s.PmTong,0)) AS PhutPm,
 MAX(CASE WHEN o.IdCongViec IS NULL THEN 0 ELSE 1 END) AS CoPhienDangMo,
 MAX(CASE WHEN a.IdCongViec IS NULL THEN 0 ELSE 1 END) AS CoPmNgoaiDanhSach
 FROM Tasks t
 LEFT JOIN PersonState s ON s.IdDuAn=t.IdDuAn AND s.IdCongViec=t.IdCongViec
 LEFT JOIN OpenSessions o ON o.IdDuAn=t.IdDuAn AND o.IdCongViec=t.IdCongViec
 LEFT JOIN AlienPm a ON a.IdDuAn=t.IdDuAn AND a.IdCongViec=t.IdCongViec
 GROUP BY t.IdDuAn,t.IdCongViec,t.TrangThai
), TaskFinal AS (
 SELECT *, CASE WHEN TrangThai IN(2,3) AND SoNguoi>0 AND SoNguoiDaChot=SoNguoi
                         AND CoPhienDangMo=0 AND CoPmNgoaiDanhSach=0 THEN 1 ELSE 0 END AS DaChotTask
 FROM TaskCounts
), ProjectTotals AS (
 SELECT IdDuAn,COUNT(*) AS SoTaskToanDuAnCanChot,
 SUM(DaChotTask) AS SoTaskToanDuAnDaChot,MAX(CoPhienDangMo) AS CoPhienDangMo,
 SUM(CASE WHEN DaChotTask=1 THEN PhutPm ELSE 0 END) AS TongPhutPmChotDuAn
 FROM TaskFinal GROUP BY IdDuAn
), EmpRows AS (
 SELECT s.IdDuAn,s.IdCongViec,s.DaPmChot,s.PmThuong,s.PmTangCa,t.DaChotTask,
 r.LoaiSuKien AS LoaiBao,r.LanThucHien AS LanBao,cur.LanThucHien AS LanHienTai,
 r.SoPhutThuongTuKhai,r.SoPhutTangCaTuKhai,
 et.GiayThuong,et.GiayTangCa,CASE WHEN et.IdCongViec IS NULL THEN 0 ELSE 1 END AS CoPhien
 FROM PersonState s JOIN TaskFinal t ON t.IdDuAn=s.IdDuAn AND t.IdCongViec=s.IdCongViec
 JOIN Tasks cur ON cur.IdDuAn=s.IdDuAn AND cur.IdCongViec=s.IdCongViec
 LEFT JOIN LatestReportEmp r ON r.IdDuAn=s.IdDuAn AND r.IdCongViec=s.IdCongViec AND r.IdNhanVien=@EmpId AND r.rn=1
 LEFT JOIN EmpTime et ON et.IdDuAn=s.IdDuAn AND et.IdCongViec=s.IdCongViec
 WHERE s.IdNhanVien=@EmpId
), EmpTotals AS (
 SELECT IdDuAn,COUNT(*) AS SoTaskLienQuan,
 SUM(ISNULL(GiayThuong,0)) AS GiayThuongHeThong,SUM(ISNULL(GiayTangCa,0)) AS GiayTangCaHeThong,
 MAX(CoPhien) AS CoGioHeThong,
 SUM(CASE WHEN LanBao=LanHienTai AND LoaiBao IN(1,3,5)
              AND SoPhutThuongTuKhai IS NOT NULL AND SoPhutTangCaTuKhai IS NOT NULL THEN 1 ELSE 0 END) AS SoTaskCoTuKhai,
 SUM(CASE WHEN LanBao=LanHienTai AND LoaiBao IN(1,3,5)
              AND SoPhutThuongTuKhai IS NOT NULL AND SoPhutTangCaTuKhai IS NOT NULL THEN CAST(SoPhutThuongTuKhai AS bigint) ELSE CAST(0 AS bigint) END) AS PhutThuongTuKhai,
 SUM(CASE WHEN LanBao=LanHienTai AND LoaiBao IN(1,3,5)
              AND SoPhutThuongTuKhai IS NOT NULL AND SoPhutTangCaTuKhai IS NOT NULL THEN CAST(SoPhutTangCaTuKhai AS bigint) ELSE CAST(0 AS bigint) END) AS PhutTangCaTuKhai,
 SUM(CASE WHEN DaChotTask=1 AND DaPmChot=1 THEN 1 ELSE 0 END) AS SoTaskDaChot,
 SUM(CASE WHEN DaChotTask=1 AND DaPmChot=1 THEN CAST(ISNULL(PmThuong,0) AS bigint) ELSE CAST(0 AS bigint) END) AS PhutThuongPmChot,
 SUM(CASE WHEN DaChotTask=1 AND DaPmChot=1 THEN CAST(ISNULL(PmTangCa,0) AS bigint) ELSE CAST(0 AS bigint) END) AS PhutTangCaPmChot
 FROM EmpRows GROUP BY IdDuAn
)
SELECT d.IdDuAn,ISNULL(p.SoTaskToanDuAnCanChot,0) AS SoTaskToanDuAnCanChot,
 ISNULL(p.SoTaskToanDuAnDaChot,0) AS SoTaskToanDuAnDaChot,ISNULL(p.CoPhienDangMo,0) AS CoPhienDangMo,
 ISNULL(p.TongPhutPmChotDuAn,0) AS TongPhutPmChotDuAn,
 ISNULL(e.SoTaskLienQuan,0) AS SoTaskLienQuan,ISNULL(e.GiayThuongHeThong,0) AS GiayThuongHeThong,
 ISNULL(e.GiayTangCaHeThong,0) AS GiayTangCaHeThong,ISNULL(e.CoGioHeThong,0) AS CoGioHeThong,
 ISNULL(e.SoTaskCoTuKhai,0) AS SoTaskCoTuKhai,ISNULL(e.PhutThuongTuKhai,0) AS PhutThuongTuKhai,
 ISNULL(e.PhutTangCaTuKhai,0) AS PhutTangCaTuKhai,ISNULL(e.SoTaskDaChot,0) AS SoTaskDaChot,
 ISNULL(e.PhutThuongPmChot,0) AS PhutThuongPmChot,ISNULL(e.PhutTangCaPmChot,0) AS PhutTangCaPmChot
FROM DuAnScope d LEFT JOIN ProjectTotals p ON p.IdDuAn=d.IdDuAn
LEFT JOIN EmpTotals e ON e.IdDuAn=d.IdDuAn;";
            sql=sql.Replace("/*SCOPE_IDS*/",parameters);
            QueryCommand cmd=new QueryCommand(sql,DataService.Provider.Name);
            cmd.AddParameter("@EmpId",idNhanVien,DbType.Guid);
            for(int i=0;i<scope.Count;i++)cmd.AddParameter("@NvdScope"+i,scope[i],DbType.Guid);
            DataSet ds=DataService.GetDataSet(cmd);
            return ds!=null&&ds.Tables.Count>0?ds.Tables[0]:new DataTable();
        }

        // Tuong thich cho ma hien tai dang goi theo mot du an (hoac null = tat ca).
        public DataTable GetGioCongNhanVienV2Data(Guid idNhanVien, Guid? idDuAn)
        {
            return GetGioCongNhanVienV2TheoDanhSachDuAnData(idNhanVien,
                idDuAn.HasValue ? (IEnumerable<Guid>)new[] { idDuAn.Value } : null);
        }

        public int CountDuAnCuaNhanVien(Guid idNhanVien)
        {
            DataTable dt=GetDanhSachDuAnCuaNhanVienData(idNhanVien,null,null);
            return dt.Rows.Count;
        }

        public TblThanhVienDuAn Update(TblThanhVienDuAn thanhVienDuAn, string description = null)
        {
            Guid id = Guid.Parse(thanhVienDuAn.GetColumnValue("IdThanhVienDuAn").ToString());
            Guid idDuAn = Guid.Parse(thanhVienDuAn.GetColumnValue("IdDuAn").ToString());
            TblThanhVienDuAn itemOld = GetById(id);
            thanhVienDuAn.Save();
            string updatedBy = string.Empty;
            try
            {
                updatedBy = thanhVienDuAn.GetColumnValue("NguoiCapNhat")?.ToString();
            }
            catch { }
            Task.Run(async () => {
                try
                {
                    await _auditManager.LogChangesAsync(itemOld, thanhVienDuAn, _tableName, id, updatedBy, idDuAn, GetNhanVienDisplayName(thanhVienDuAn.IdNhanVien.Value), description).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblDuAn");
                }
            });
            return thanhVienDuAn;
        }

        public override TblThanhVienDuAn GetById(Guid id)
        {
            return new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdThanhVienDuAnColumn).IsEqualTo(id).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).ExecuteSingle<TblThanhVienDuAn>();
        }

        public List<TblThanhVienDuAn> GetByIdDuAn(Guid idDuAn)
        {
            Select select = new Select();
            select.From(TblThanhVienDuAn.Schema);
            select.Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn);
            return select.ExecuteTypedList<TblThanhVienDuAn>();
        }
        //Hàm này sẽ bổ sung thêm 1 tham số và 1 điều kiện And
        //Lý do: Giả sử khi đã chọn nhân viên A làm PM, thì khi vào danh sách nhân viên để chọn đám nhân viên thêm vào dự án, //sẽ tồn tại trường hợp thk nhân viên A cũng nằm trong danh sách đó, và nếu chọn nó để thêm vào thì cái vai trò của nó sẽ bị đè lên
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
                .And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhVienDuAn>();
        }

        public TblThanhVienDuAn GetNhanVienIsActiveInDuAn(Guid idNhanVien, Guid idDuAn)
        {
            return new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(idNhanVien)
                .And(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblThanhVienDuAn>();
        }
        //THêm mới 3 hàm sau
        //1. Hàm này dùng đến lấy danh sách dựa vào id dự án và vai trò và có ngoại lệ (except 1 đứa), //mục đích là lấy danh sách với vai trò là PM để tiến hành cho chức năng đỏi PM từ nhân viên A sang nv B
        //Mỗi dự án chỉ có 1 PM nên hàm này nếu viết dạng lấy 1 cũng được nhưng viết list cho chắc để tránh trường hợp db lỗi 
        //làm tồn tại 2 PM đang hoạt động trên cùng 1 dự án. qua Manager nói rõ hơn
        //Đổi về lấy 1 nếu muốn 
        public List<TblThanhVienDuAn> GetByIdDuAnAndVaiTroExcept(Guid idDuAn, Guid idVaiTro, Guid idNhanVienGiuLai)
        {
            return new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn).And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).And(TblThanhVienDuAn.IdNhanVienColumn).IsNotEqualTo(idNhanVienGiuLai).ExecuteTypedList<TblThanhVienDuAn>();
        }
        //2. Hàm này lấy idNhanVien để dùng cho chức năng edit, lấy list id đó để đánh tích vô mấy cái checkbox 
        public List<Guid> GetIdNhanVienByDuAnAndVaiTro(Guid idDuAn, Guid idVaiTro)
        {
            List<TblThanhVienDuAn> list = new Select().From(TblThanhVienDuAn.Schema).Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(idDuAn).And(TblThanhVienDuAn.IdVaiTroDuAnColumn).IsEqualTo(idVaiTro).And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false).ExecuteTypedList<TblThanhVienDuAn>();

            return list.Where(x => x.IdNhanVien.HasValue).Select(x => x.IdNhanVien.Value).ToList();
        }

        public string GetNhanVienDisplayName( Guid idNhanVien)
        {
            string sql = $@"
        DECLARE @idNhanVien UNIQUEIDENTIFIER = '{idNhanVien}';

        SELECT TOP 1
            COALESCE
            ( NULLIF( LTRIM(RTRIM(DisplayName)), N''
                ), UserName
            )
        FROM dbo.aspnet_Users
        WHERE UserId = @idNhanVien;";

            return new InlineQuery().ExecuteScalar<string>(sql);
        }
        public DataTable GetThanhVienDuAnDetail(Guid idDuAn)
        {
            string sql = $@"
                DECLARE @idDuAn VARCHAR(36) = '{idDuAn}';

                SELECT 
                    u.UserId, 
                    u.DisplayName, 
                    u.Avatar, 
                    m.Email 
                FROM TblThanhVienDuAn tv
                INNER JOIN [dbo].[aspnet_Users] u ON tv.IdNhanVien = u.UserId
                INNER JOIN [dbo].[aspnet_Membership] m ON u.UserId = m.UserId
                WHERE tv.IdDuAn = @idDuAn 
                  AND tv.DaXoa = 0 
                  AND u.IsDeleted = 0 
                  AND u.IsActivated = 1
                ORDER BY u.DisplayName ASC;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }
    }
}
