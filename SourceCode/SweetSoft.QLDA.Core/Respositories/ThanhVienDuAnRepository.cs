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
        public DataTable GetDanhSachDuAnCuaNhanVienData(Guid idNhanVien, string keyword, byte? statusId)
        {
            string normalizedKeyword = (keyword ?? string.Empty).Trim();

            string sql = @"
                DECLARE @IdNhanVien UNIQUEIDENTIFIER = @EmpId;
                DECLARE @Keyword NVARCHAR(500) = @SearchKeyword;
                DECLARE @StatusId TINYINT = @SearchStatusId;

                ;WITH MyProjects AS (
                    SELECT da.IdDuAn, da.MaDuAn, da.TenDuAn, da.TrangThai AS TrangThaiDuAn, da.NgayBatDau AS ProjectStartDate,
                           ISNULL(da.NgayHoanThanhThucTe, da.NgayDuKienHoanThanh) AS ProjectEndDate, vt.TenVaiTro AS VaiTro, da.SuDungHeSoDongGopMacDinh
                    FROM TblDuAn da
                    INNER JOIN TblThanhVienDuAn tv ON tv.IdDuAn = da.IdDuAn
                    INNER JOIN TblVaiTroDuAn vt ON vt.IdVaiTroDuAn = tv.IdVaiTroDuAn
                    WHERE tv.IdNhanVien = @IdNhanVien AND tv.DaXoa = 0 AND da.DaXoa = 0
                      AND (@StatusId IS NULL OR da.TrangThai = @StatusId)
                      AND (@Keyword = N'' OR da.TenDuAn LIKE N'%' + @Keyword + N'%')
                ),
                AssignedLeafTasks AS (
                    SELECT DISTINCT cv.IdCongViec, cv.IdDuAn, cv.IdGiaiDoanDuAn
                    FROM TblCongViec_NhanVien cvnv
                    INNER JOIN TblCongViec cv ON cvnv.IdCongViec = cv.IdCongViec
                    INNER JOIN MyProjects mp ON cv.IdDuAn = mp.IdDuAn
                    WHERE cvnv.IdNhanVien = @IdNhanVien AND cv.DaXoa = 0 AND cv.IdGiaiDoanDuAn IS NOT NULL AND cv.IdCongViecCha IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM TblCongViec child WHERE child.IdCongViecCha = cv.IdCongViec AND child.DaXoa = 0) 
                ),
                RelevantPhases AS (
                    SELECT DISTINCT IdDuAn, IdGiaiDoanDuAn FROM AssignedLeafTasks
                ),
                RelevantLeafTasks AS (
                    SELECT cv.IdCongViec, cv.IdDuAn, cv.ThoiHanNgay, ISNULL(ut.DiemUuTien, 1) AS DiemUuTien, CASE WHEN mp.SuDungHeSoDongGopMacDinh = 1 THEN hsDefault.HeSoDongGop ELSE hsProject.HeSoDongGop END AS HeSoDongGop
                    FROM TblCongViec cv
                    INNER JOIN RelevantPhases rp ON cv.IdDuAn = rp.IdDuAn AND cv.IdGiaiDoanDuAn = rp.IdGiaiDoanDuAn
                    INNER JOIN MyProjects mp ON mp.IdDuAn = cv.IdDuAn
                    INNER JOIN TblGiaiDoanDuAn gd ON gd.IdGiaiDoanDuAn = cv.IdGiaiDoanDuAn AND gd.IdDuAn = cv.IdDuAn AND gd.DaXoa = 0
                    LEFT JOIN TblDoUuTien ut ON cv.IdDoUuTien = ut.IdDoUuTien
                    LEFT JOIN TblHeSoDongGop hsProject ON hsProject.IdDuAn = cv.IdDuAn AND hsProject.IdDoUuTien = cv.IdDoUuTien AND hsProject.DaXoa = 0
                    LEFT JOIN TblHeSoDongGop hsDefault ON hsDefault.IdDuAn IS NULL AND hsDefault.IdDoUuTien = cv.IdDoUuTien AND hsDefault.DaXoa = 0
                    WHERE cv.DaXoa = 0 AND cv.IdCongViecCha IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM TblCongViec child WHERE child.IdCongViecCha = cv.IdCongViec AND child.DaXoa = 0)
                ),
                TaskStats AS (
                    SELECT t.IdCongViec, t.IdDuAn, t.ThoiHanNgay, t.DiemUuTien, COUNT(a.IdNhanVien) AS AssigneeCount, t.HeSoDongGop,
                           MAX(CASE WHEN a.IdNhanVien = @IdNhanVien THEN 1 ELSE 0 END) AS IsMyTask
                    FROM RelevantLeafTasks t
                    LEFT JOIN TblCongViec_NhanVien a ON a.IdCongViec = t.IdCongViec
                    GROUP BY t.IdCongViec, t.IdDuAn, t.ThoiHanNgay, t.DiemUuTien, t.HeSoDongGop
                ),
                ProjectContribution AS (
                    SELECT IdDuAn,
                           SUM(CAST(ISNULL(ThoiHanNgay, 0) AS DECIMAL(18,4)) * HeSoDongGop ) AS Total_E_All_Employees,
                           SUM( CASE WHEN IsMyTask = 1 THEN ( CAST(ISNULL(ThoiHanNgay, 0) AS DECIMAL(18,4)) * HeSoDongGop ) / NULLIF(AssigneeCount, 0) ELSE 0 END ) AS Total_E_My_Employee
                    FROM TaskStats
                    GROUP BY IdDuAn
                )
                SELECT mp.IdDuAn, mp.MaDuAn, mp.TenDuAn, mp.VaiTro, mp.TrangThaiDuAn, mp.ProjectStartDate, mp.ProjectEndDate,
                       ISNULL(pc.Total_E_All_Employees, 0) AS Total_E_All_Employees,
                       ISNULL(pc.Total_E_My_Employee, 0) AS Total_E_My_Employee
                FROM MyProjects mp
                LEFT JOIN ProjectContribution pc ON pc.IdDuAn = mp.IdDuAn
                ORDER BY mp.ProjectStartDate DESC, mp.MaDuAn ASC;";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);
            cmd.AddParameter("@SearchKeyword", normalizedKeyword, DbType.String);
            cmd.AddParameter("@SearchStatusId", statusId.HasValue ? (object)statusId.Value : DBNull.Value, DbType.Byte);

            DataSet ds = DataService.GetDataSet(cmd);
            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
        }

        public DataTable GetChiTietDuAnCuaNhanVienData(Guid idNhanVien, Guid idDuAn)
        {
            const string sql = @"
                DECLARE @IdNhanVien UNIQUEIDENTIFIER = @EmpId;
                DECLARE @IdDuAn UNIQUEIDENTIFIER = @ProjectId;

                ;WITH MyProject AS (
                    SELECT da.IdDuAn, da.MaDuAn, da.TenDuAn, da.TrangThai AS TrangThaiDuAn, da.NgayBatDau AS ProjectStartDate,
                           ISNULL(da.NgayHoanThanhThucTe, da.NgayDuKienHoanThanh) AS ProjectEndDate, vt.TenVaiTro AS VaiTro, da.SuDungHeSoDongGopMacDinh
                    FROM TblDuAn da
                    INNER JOIN TblThanhVienDuAn tv ON tv.IdDuAn = da.IdDuAn
                    INNER JOIN TblVaiTroDuAn vt ON vt.IdVaiTroDuAn = tv.IdVaiTroDuAn
                    WHERE tv.IdNhanVien = @IdNhanVien AND tv.DaXoa = 0 AND da.DaXoa = 0 AND da.IdDuAn = @IdDuAn
                ),
                MyAssignedTasks AS (
                    SELECT DISTINCT cv.IdDuAn, cv.IdGiaiDoanDuAn
                    FROM TblCongViec_NhanVien cvnv
                    INNER JOIN TblCongViec cv ON cvnv.IdCongViec = cv.IdCongViec
                    WHERE cvnv.IdNhanVien = @IdNhanVien AND cv.IdDuAn = @IdDuAn AND cv.DaXoa = 0 AND cv.IdGiaiDoanDuAn IS NOT NULL AND cv.IdCongViecCha IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM TblCongViec child WHERE child.IdCongViecCha = cv.IdCongViec AND child.DaXoa = 0)
                ),
                RelevantPhases AS (
                    SELECT DISTINCT IdDuAn, IdGiaiDoanDuAn FROM MyAssignedTasks
                ),
                AllLeafTasks AS (
                    SELECT cv.IdDuAn, gd.IdGiaiDoanDuAn AS IdPhase, root.MaCongViec AS MaPhase,
                           CASE WHEN NULLIF(LTRIM(RTRIM(gd.TenGiaiDoanTuyChinh)), N'') IS NOT NULL THEN gd.TenGiaiDoanTuyChinh ELSE root.TenCongViec END AS TenPhase,
                           parent.MaCongViec AS MaTaskCha, parent.TenCongViec AS TenTaskCha,
                           cv.IdCongViec AS IdTask, cv.MaCongViec AS MaTask, cv.TenCongViec AS TenTask,
                           cv.NgayBatDau, cv.NgayKetThuc, cv.NgayHoanThanhThucTe, cv.ThoiHanNgay, cv.TrangThai AS TrangThaiTask,
                           ISNULL(ut.DiemUuTien, 1) AS DiemUuTien, ISNULL(ut.TenDoUuTien, N'Thấp') AS TenDoUuTien,  CASE WHEN mp.SuDungHeSoDongGopMacDinh = 1 THEN hsDefault.HeSoDongGop  ELSE hsProject.HeSoDongGop END AS HeSoDongGop
                    FROM TblCongViec cv
                    INNER JOIN RelevantPhases rp ON cv.IdDuAn = rp.IdDuAn AND cv.IdGiaiDoanDuAn = rp.IdGiaiDoanDuAn
                    INNER JOIN MyProject mp ON mp.IdDuAn = cv.IdDuAn
                    INNER JOIN TblGiaiDoanDuAn gd ON gd.IdGiaiDoanDuAn = cv.IdGiaiDoanDuAn AND gd.IdDuAn = cv.IdDuAn AND gd.DaXoa = 0  
                    INNER JOIN TblCongViec root ON root.IdDuAn = cv.IdDuAn AND root.IdGiaiDoanDuAn = gd.IdGiaiDoanDuAn AND root.IdCongViecCha IS NULL AND root.DaXoa = 0
                    LEFT JOIN TblCongViec parent ON cv.IdCongViecCha = parent.IdCongViec
                    LEFT JOIN TblDoUuTien ut ON cv.IdDoUuTien = ut.IdDoUuTien
                    LEFT JOIN TblHeSoDongGop hsProject ON hsProject.IdDuAn = cv.IdDuAn AND hsProject.IdDoUuTien = cv.IdDoUuTien AND hsProject.DaXoa = 0
                    LEFT JOIN TblHeSoDongGop hsDefault ON hsDefault.IdDuAn IS NULL AND hsDefault.IdDoUuTien = cv.IdDoUuTien AND hsDefault.DaXoa = 0
                    WHERE cv.IdDuAn = @IdDuAn AND cv.DaXoa = 0 AND cv.IdCongViecCha IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM TblCongViec child WHERE child.IdCongViecCha = cv.IdCongViec AND child.DaXoa = 0)
                ),
                TaskAssignees AS (
                    SELECT a.IdCongViec, COUNT(a.IdNhanVien) AS AssigneeCount, MAX(CASE WHEN a.IdNhanVien = @IdNhanVien THEN 1 ELSE 0 END) AS IsMyTask
                    FROM TblCongViec_NhanVien a
                    INNER JOIN AllLeafTasks t ON a.IdCongViec = t.IdTask
                    GROUP BY a.IdCongViec
                )
                SELECT mp.IdDuAn, mp.MaDuAn, mp.TenDuAn, mp.VaiTro, mp.TrangThaiDuAn, mp.ProjectStartDate, mp.ProjectEndDate,
                       t.IdPhase, t.MaPhase, t.TenPhase, t.MaTaskCha, t.TenTaskCha, t.IdTask, t.MaTask, t.TenTask,
                       t.NgayBatDau, t.NgayKetThuc, t.NgayHoanThanhThucTe, t.ThoiHanNgay, t.DiemUuTien, t.TenDoUuTien, t.TrangThaiTask, t.HeSoDongGop,
                       ISNULL(ta.AssigneeCount, 0) AS AssigneeCount, ISNULL(ta.IsMyTask, 0) AS IsMyTask
                FROM MyProject mp
                LEFT JOIN AllLeafTasks t ON mp.IdDuAn = t.IdDuAn
                LEFT JOIN TaskAssignees ta ON t.IdTask = ta.IdCongViec
                ORDER BY t.IdPhase, t.MaTask;";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);
            cmd.AddParameter("@ProjectId", idDuAn, DbType.Guid);

            DataSet ds = DataService.GetDataSet(cmd);
            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
        }

        public int CountDuAnCuaNhanVien(Guid idNhanVien)
        {
            const string sql = @"
                SELECT COUNT(DISTINCT da.IdDuAn)
                FROM TblDuAn da
                INNER JOIN TblThanhVienDuAn tv ON da.IdDuAn = tv.IdDuAn
                WHERE tv.IdNhanVien = @EmpId AND tv.DaXoa = 0 AND da.DaXoa = 0;";

            QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
            cmd.AddParameter("@EmpId", idNhanVien, DbType.Guid);

            object result = DataService.ExecuteScalar(cmd);
            if (result == null || result == DBNull.Value) return 0;
            return Convert.ToInt32(result);
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
