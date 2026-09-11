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
    public class ProjectReportRepository : BaseRepository<TblDuAn>
    {
        public ProjectReportRepository(AuditManager auditManager) : base(auditManager) { }
        public DataTable GetProjectInfo(Guid projectId)
        {
            string sql = $"SELECT NgayBatDau, NgayHoanThanhThucTe, TrangThai FROM TblDuAn WHERE IdDuAn = '{projectId}'";
            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql)) { dt.Load(reader); }
            return dt;
        }

        #region Helpers tạo chuỗi Filter Thời gian
        public string GetTaskDateFilter(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
            {
                string fDate = fromDate.Value.ToString("yyyy-MM-dd");
                string tDate = toDate.Value.ToString("yyyy-MM-dd");
                return $" AND ((NgayKetThuc >= '{fDate}' AND NgayKetThuc <= '{tDate}') OR (NgayHoanThanhThucTe >= '{fDate}' AND NgayHoanThanhThucTe <= '{tDate}')) ";
            }
            return string.Empty;
        }

        public string GetIssueDateFilter(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
            {
                string fDate = fromDate.Value.ToString("yyyy-MM-dd");
                string tDate = toDate.Value.ToString("yyyy-MM-dd");
                return $" AND (NgayTao >= '{fDate}' AND NgayTao <= '{tDate}') ";
            }
            return string.Empty;
        }
        #endregion
        public int GetTotalTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            string dateFilter = GetTaskDateFilter(fromDate, toDate);
            string sql = $"SELECT COUNT(1) FROM TblCongViec WHERE IdDuAn = '{projectId}' AND (DaXoa = 0 OR DaXoa IS NULL) {dateFilter}";

            return new InlineQuery().ExecuteScalar<int>(sql);
        }
        public DataTable GetCompletedTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            string dateFilter = GetTaskDateFilter(fromDate, toDate);
            string sql = $@"
                SELECT 
                    c.MaCongViec, c.TenCongViec, c.TrangThai, ISNULL(c.NgayHoanThanhThucTe, c.NgayCapNhat) as NgayHoanThanhThucTe,
                    STUFF((SELECT ', ' + u.DisplayName FROM TblCongViec_NhanVien cvn INNER JOIN aspnet_Users u ON cvn.IdNhanVien = u.UserId WHERE cvn.IdCongViec = c.IdCongViec FOR XML PATH('')), 1, 2, '') AS Assignee
                FROM TblCongViec c
                WHERE c.IdDuAn = '{projectId}' 
                  AND (c.DaXoa = 0 OR c.DaXoa IS NULL) 
                  AND c.TrangThai = 2
                  {dateFilter}
                ORDER BY NgayHoanThanhThucTe DESC";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql)) { dt.Load(reader); }
            return dt;
        }
        public DataTable GetOverdueTasks(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            string dateFilter = GetTaskDateFilter(fromDate, toDate);
            string sql = $@"
                SELECT 
                    c.MaCongViec, c.TenCongViec, c.NgayKetThuc, c.TrangThai,
                    DATEDIFF(day, c.NgayKetThuc, GETDATE()) AS DaysOverdue,
                    STUFF((SELECT ', ' + u.DisplayName FROM TblCongViec_NhanVien cvn INNER JOIN aspnet_Users u ON cvn.IdNhanVien = u.UserId WHERE cvn.IdCongViec = c.IdCongViec FOR XML PATH('')), 1, 2, '') AS Assignee
                FROM TblCongViec c
                WHERE c.IdDuAn = '{projectId}' 
                  AND (c.DaXoa = 0 OR c.DaXoa IS NULL) 
                  AND c.NgayKetThuc < CAST(GETDATE() AS DATE) 
                  AND ISNULL(c.TrangThai, 0) <> 2
                  {dateFilter}
                ORDER BY c.NgayKetThuc ASC";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql)) { dt.Load(reader); }
            return dt;
        }
        public DataTable GetIssues(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            string dateFilter = GetIssueDateFilter(fromDate, toDate);
            string sql = $@"
                SELECT 
                    v.MaVanDe, v.TenVanDe, ISNULL(v.MucDoAnhHuong, 2) AS MucDoAnhHuong, v.TrangThai, v.KeHoachXuLy
                FROM TblVanDe v
                WHERE v.IdDuAn = '{projectId}' 
                  AND (v.DaXoa = 0 OR v.DaXoa IS NULL)
                  {dateFilter}
                ORDER BY v.MaVanDe DESC";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql)) { dt.Load(reader); }
            return dt;
        }
    }
}

