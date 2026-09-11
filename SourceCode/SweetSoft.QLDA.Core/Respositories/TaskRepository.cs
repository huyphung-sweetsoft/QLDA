using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;

namespace SweetSoft.QLDA.Core.Respositories
{
    internal class TaskRepository : BaseRepository<TblCongViec>
    {
        public TaskRepository(AuditManager auditManager) : base(auditManager) { }

        #region 1. Truy vấn Công việc
        public DataTable FetchByIdAndOrderASCMaCV(Guid projectId, string searchValue=null)
        {
            string searchCondition = "";
            if (!string.IsNullOrEmpty(searchValue))
            {
                string keyword = searchValue.Trim().Replace("'", "''"); 
                searchCondition = $" AND (t.MaCongViec LIKE N'%{keyword}%' OR t.TenCongViec LIKE N'%{keyword}%')";
            }
            string sql = $@"
                SELECT 
                    t.*,
                    ut.TenDoUuTien,
                    ut.DiemUuTien,
        
                    STUFF((
                        SELECT ', ' + u.DisplayName
                        FROM [dbo].[TblCongViec_NhanVien] cn
                        INNER JOIN [dbo].[aspnet_Users] u ON cn.IdNhanVien = u.UserId
                        WHERE cn.IdCongViec = t.IdCongViec
                           AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TenNhanVien,
            
                    STUFF((
                        SELECT ',' + CAST(u.UserId AS VARCHAR(50))
                        FROM [dbo].[TblCongViec_NhanVien] cn
                        INNER JOIN [dbo].[aspnet_Users] u ON cn.IdNhanVien = u.UserId
                        WHERE cn.IdCongViec = t.IdCongViec
                           AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS IdNhanVien,
                    STUFF((
                        SELECT ',' + ISNULL(u.Avatar, '')
                        FROM [dbo].[TblCongViec_NhanVien] cn
                        INNER JOIN [dbo].[aspnet_Users] u ON cn.IdNhanVien = u.UserId
                        WHERE cn.IdCongViec = t.IdCongViec
                           AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Avatars
                FROM [dbo].[TblCongViec] t
                LEFT JOIN [dbo].[TblDoUuTien] ut ON t.IdDoUuTien = ut.IdDoUuTien
                WHERE t.IdDuAn = '{projectId}'
                   AND t.DaXoa = 0
                  {searchCondition}
                ORDER BY t.MaCongViec ASC;
            ";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }

        public TblCongViec FetchById(Guid taskId)
        {
            return new Select().From(TblCongViec.Schema)
                               .Where(TblCongViec.Columns.IdCongViec).IsEqualTo(taskId)
                               .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                               .ExecuteSingle<TblCongViec>();
        }

        public DataTable GetChildTasks(Guid projectId, Guid taskId)
        {
            return new Select().From(TblCongViec.Schema)
                               .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(projectId)
                               .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                               .And(TblCongViec.Columns.IdCongViecCha).IsEqualTo(taskId)
                               .ExecuteDataSet().Tables[0];
        }

        public DataTable GetDependentTasks(Guid projectId, Guid taskId)
        {
            return new Select().From(TblCongViec.Schema)
                               .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(projectId)
                               .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                               .And(TblCongViec.Columns.IdCongViecPhuThuoc).IsEqualTo(taskId)
                               .ExecuteDataSet().Tables[0];
        }

        public TblCongViec GetFirstChildTask(Guid projectId, Guid taskId)
        {
            return new Select().From(TblCongViec.Schema)
                               .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(projectId)
                               .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                               .And(TblCongViec.Columns.IdCongViecCha).IsEqualTo(taskId)
                               .OrderAsc(TblCongViec.Columns.MaCongViec)
                               .ExecuteSingle<TblCongViec>();
        }

        public TblCongViec GetRootTaskByStageId(
    Guid idGiaiDoanDuAn)
        {
            if (idGiaiDoanDuAn == Guid.Empty)
                return null;

            return new Select()
                .From(TblCongViec.Schema)
                .Where(
                    TblCongViec.IdGiaiDoanDuAnColumn)
                .IsEqualTo(idGiaiDoanDuAn)
                .And(
                    TblCongViec.IdCongViecChaColumn)
                .IsNull()
                .And(
                    TblCongViec.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteSingle<TblCongViec>();
        }
        #endregion

        #region 2. Truy vấn Danh mục & Thành viên
        public DataTable FetchAllPrioritiesTable()
        {
            return new Select().From(TblDoUuTien.Schema)
                               .OrderAsc(TblDoUuTien.Columns.DiemUuTien)
                               .ExecuteDataSet().Tables[0];
        }

        public List<TblDoUuTien> FetchAllPrioritiesList()
        {
            return new Select().From(TblDoUuTien.Schema).ExecuteTypedList<TblDoUuTien>();
        }

        public DataTable FetchProjectMembers(Guid projectId)
        {
            return new Select().From(TblThanhVienDuAn.Schema)
                               .Where(TblThanhVienDuAn.Columns.IdDuAn).IsEqualTo(projectId)
                               .ExecuteDataSet().Tables[0];
        }
        #endregion
        #region 3. Xoa cong viec
        public void DeleteTask(TblCongViec task)
        {
            if (task == null) return;
            string sqlDelete = $@"
                WITH TaskHierarchy AS (
                    SELECT IdCongViec FROM TblCongViec WHERE IdCongViec = '{task.IdCongViec}'
            
                    UNION ALL
            
                    SELECT t.IdCongViec FROM TblCongViec t
                    INNER JOIN TaskHierarchy th ON t.IdCongViecCha = th.IdCongViec
                    WHERE t.DaXoa = 0 OR t.DaXoa IS NULL
                )
                UPDATE TblCongViec 
                SET DaXoa = 1, 
                    NgayCapNhat = GETDATE()
                WHERE IdCongViec IN (SELECT IdCongViec FROM TaskHierarchy);";
            new SubSonic.InlineQuery().Execute(sqlDelete);
        }
        #endregion
        #region 5. Lấy task phục vụ Lịch biểu cá nhân
        public DataTable GetActiveTasksByNhanVienInRange(Guid idNhanVien, DateTime start, DateTime end)
        {
            // Đã bổ sung LEFT JOIN TblDuAn để lấy TenDuAn, MaDuAn
            string sql = $@"
        SELECT DISTINCT t.*, 
               da.TenDuAn, 
               da.MaDuAn
        FROM [dbo].[TblCongViec] t
        INNER JOIN [dbo].[TblCongViec_NhanVien] cn ON cn.IdCongViec = t.IdCongViec
        LEFT JOIN [dbo].[TblDuAn] da ON t.IdDuAn = da.IdDuAn
        WHERE cn.IdNhanVien = '{idNhanVien}'
          AND t.DaXoa = 0
          AND t.TrangThai <> 2
          AND t.NgayBatDau IS NOT NULL
          AND t.NgayKetThuc IS NOT NULL
          AND t.NgayBatDau <= '{end:yyyy-MM-dd}'
          AND t.NgayKetThuc >= '{start:yyyy-MM-dd}'
        ORDER BY t.NgayBatDau;
    ";
            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            if (reader == null) return null;
            DataTable dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public DataTable GetActiveTasksByNhanViensInRange(List<Guid> idNhanViens, DateTime start, DateTime end)
        {
            if (idNhanViens == null || idNhanViens.Count == 0) return null;
            string idList = string.Join(",", idNhanViens.ConvertAll(id => $"'{id}'"));

            string sql = $@"
        SELECT DISTINCT t.*, cn.IdNhanVien AS AssignedNhanVienId
        FROM [dbo].[TblCongViec] t
        INNER JOIN [dbo].[TblCongViec_NhanVien] cn ON cn.IdCongViec = t.IdCongViec
        WHERE cn.IdNhanVien IN ({idList})
          AND t.DaXoa = 0
          AND t.TrangThai <> 2
          AND t.NgayBatDau IS NOT NULL
          AND t.NgayKetThuc IS NOT NULL
          AND t.NgayBatDau <= '{end:yyyy-MM-dd}'
          AND t.NgayKetThuc >= '{start:yyyy-MM-dd}'
        ORDER BY t.NgayBatDau;
    ";
            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            if (reader == null) return null;
            DataTable dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
        #endregion
        #region 6. Gán / Gỡ nhân viên cho Công việc
        public List<Guid> GetAssignedNhanVienIds(Guid idCongViec)
        {
            var list = new Select()
                .From(TblCongViecNhanVien.Schema)
                .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(idCongViec)
                .ExecuteTypedList<TblCongViecNhanVien>();

            List<Guid> result = new List<Guid>();
            foreach (var item in list)
            {
                result.Add(item.IdNhanVien);
            }
            return result;
        }

        public void AddAssignment(Guid idCongViec, Guid idNhanVien)
        {
            // Dùng trực tiếp Entity (ActiveRecord)
            TblCongViecNhanVien item = new TblCongViecNhanVien();
            item.IdCongViec = idCongViec;
            item.IdNhanVien = idNhanVien;
            item.NgayPhanCong = DateTime.Now;
            item.Save();
        }

        public void RemoveAssignment(Guid idCongViec, Guid idNhanVien)
        {
            // Sửa lại chuẩn cú pháp SubSonic 2.x: new Delete().From(...)
            new Delete().From(TblCongViecNhanVien.Schema)
                .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(idCongViec)
                .And(TblCongViecNhanVien.Columns.IdNhanVien).IsEqualTo(idNhanVien)
                .Execute();
        }
        #endregion
    }
}