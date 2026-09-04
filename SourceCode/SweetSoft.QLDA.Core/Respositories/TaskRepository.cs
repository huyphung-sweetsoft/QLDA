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
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS IdNhanVien
            
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
            task.DaXoa = true;
            task.NgayCapNhat = DateTime.Now;
            task.Save();
        }
        #endregion
    }
}