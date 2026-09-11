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
    public class GanttChartRepository : BaseRepository<TblCongViec>
    {
        public GanttChartRepository(AuditManager auditManager) : base(auditManager) { }
        public DataTable GetGanttTasks(Guid projectId)
        {
            string sql = $@"
                SELECT 
                    c.IdCongViec,
                    c.IdCongViecPhuThuoc, 
                    c.MaCongViec,
                    c.TenCongViec,
                    c.NgayBatDau,
                    c.NgayKetThuc,
                    c.TrangThai,
                    CAST(CASE WHEN c.NgayKetThuc < CAST(GETDATE() AS DATE) AND ISNULL(c.TrangThai, 0) <> 2 THEN 1 ELSE 0 END AS BIT) AS IsOverdue,
                    STUFF((
                        SELECT ', ' + u.DisplayName
                        FROM TblCongViec_NhanVien cvn
                        INNER JOIN aspnet_Users u ON cvn.IdNhanVien = u.UserId
                        WHERE cvn.IdCongViec = c.IdCongViec
                        FOR XML PATH('')
                    ), 1, 2, '') AS NhanVienThucHien,
                    (SELECT COUNT(IdVanDe) 
                     FROM TblVanDe 
                     WHERE IdCongViecBiAnhHuong = c.IdCongViec AND (DaXoa = 0 OR DaXoa IS NULL)) AS IssueCount
                FROM TblCongViec c
                WHERE c.IdDuAn = '{projectId}' 
                  AND (c.DaXoa = 0 OR c.DaXoa IS NULL)
                ORDER BY c.MaCongViec ASC;";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql))
            {
                dt.Load(reader);
            }
            return dt;
        }
        public DataTable GetOverdueTasks(Guid projectId, string taskCode)
        {
            string sql = $@"
                SELECT MaCongViec, TenCongViec, NgayKetThuc, TrangThai
                FROM TblCongViec
                WHERE IdDuAn = '{projectId}'
                  AND (MaCongViec = '{taskCode}' OR MaCongViec LIKE '{taskCode}.%')
                  AND NgayKetThuc < CAST(GETDATE() AS DATE)
                  AND ISNULL(TrangThai, 0) <> 2
                  AND (DaXoa = 0 OR DaXoa IS NULL)
                ORDER BY MaCongViec ASC";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql))
            {
                dt.Load(reader);
            }
            return dt;
        }
        public DataTable GetTaskIssues(string taskId)
        {
            string sql = $@"
                SELECT MaVanDe, TenVanDe, TrangThai 
                FROM TblVanDe 
                WHERE IdCongViecBiAnhHuong = '{taskId}' 
                  AND (DaXoa = 0 OR DaXoa IS NULL)
                ORDER BY MaVanDe DESC";

            DataTable dt = new DataTable();
            using (var reader = new InlineQuery().ExecuteReader(sql))
            {
                dt.Load(reader);
            }
            return dt;
        }
    }
}
