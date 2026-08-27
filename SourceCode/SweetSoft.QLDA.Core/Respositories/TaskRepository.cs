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
        public DataTable FetchByIdAndOrderASCMaCV(Guid projectId)
        {
            string sql = $@"
                        SELECT 
                            t.*, 
                            nv.IdNhanVien, 
                            nv.TenNhanVien, 
                            nv.AnhDaiDien,
                            ut.TenDoUuTien,
                            ut.DiemUuTien -- Thêm cột điểm ưu tiên để xử lý CSS badge
                        FROM [dbo].[TblCongViec] t
                        LEFT JOIN [dbo].[TblCongViec_NhanVien] cn ON t.IdCongViec = cn.IdCongViec
                        LEFT JOIN [dbo].[TblNhanVien] nv ON cn.IdNhanVien = nv.IdNhanVien
                        LEFT JOIN [dbo].[TblDoUuTien] ut ON t.IdDoUuTien = ut.IdDoUuTien
                        WHERE t.IdDuAn = '{projectId}' 
                          AND t.DaXoa = 0 
                          AND (nv.DaXoa = 0 OR nv.DaXoa IS NULL)
                        ORDER BY t.MaCongViec ASC;";
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