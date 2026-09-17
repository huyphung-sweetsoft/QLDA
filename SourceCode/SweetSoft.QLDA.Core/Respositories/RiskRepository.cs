using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class RiskRepository : BaseRepository<TblRuiRoDuAn>
    {
        public RiskRepository(AuditManager auditManager) : base(auditManager) { }

        #region 1. Truy vấn
        public DataTable GetRiskById(Guid projectId, bool deleted = false)
        {
            return new Select().From(TblRuiRoDuAn.Schema).
                                Where(TblRuiRoDuAn.Columns.IdDuAn).IsEqualTo(projectId).
                                And(TblRuiRoDuAn.Columns.DaXoa).IsEqualTo(deleted).
                                ExecuteDataSet().Tables[0];
        }

        public DataTable SearchRisk(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;

            string tenRuiRo = parameters != null && parameters.ContainsKey(TblRuiRoDuAn.Columns.TenRuiRo) ? parameters[TblRuiRoDuAn.Columns.TenRuiRo]?.ToString() : null;
            string idNhanVienXuLy = parameters != null && parameters.ContainsKey(TblRuiRoDuAn.Columns.IdNhanVienXuLy) ? parameters[TblRuiRoDuAn.Columns.IdNhanVienXuLy]?.ToString() : null;
            string mucDoAnhHuong = parameters != null && parameters.ContainsKey(TblRuiRoDuAn.Columns.MucDoAnhHuong) ? parameters[TblRuiRoDuAn.Columns.MucDoAnhHuong]?.ToString() : null;
            string mucDoRuiRoStr = parameters != null && parameters.ContainsKey("MucDoRuiRo") ? parameters["MucDoRuiRo"]?.ToString() : null;
            string xacSuatMinStr = parameters != null && parameters.ContainsKey("XacSuatMin") ? parameters["XacSuatMin"]?.ToString() : null;
            string xacSuatMaxStr = parameters != null && parameters.ContainsKey("XacSuatMax") ? parameters["XacSuatMax"]?.ToString() : null;
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "TenRuiRo ASC";
            }
            string sqlIdNhanVien = string.IsNullOrEmpty(idNhanVienXuLy) ? "NULL" : $"'{idNhanVienXuLy}'";
            string sqlMucDoAnhHuong = string.IsNullOrEmpty(mucDoAnhHuong) ? "NULL" : mucDoAnhHuong;
            string sqlMucDoRuiRo = string.IsNullOrEmpty(mucDoRuiRoStr) ? "NULL" : mucDoRuiRoStr;
            string sqlXacSuatMin = string.IsNullOrEmpty(xacSuatMinStr) ? "NULL" : xacSuatMinStr;
            string sqlXacSuatMax = string.IsNullOrEmpty(xacSuatMaxStr) ? "NULL" : xacSuatMaxStr;

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenRuiRo NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenRuiRo)}%';

                DECLARE @idNhanVienXuLy VARCHAR(36) = {sqlIdNhanVien};
                DECLARE @mucDoAnhHuong INT = {sqlMucDoAnhHuong};
                DECLARE @mucDoRuiRo INT = {sqlMucDoRuiRo};
                DECLARE @xacSuatMin INT = {sqlXacSuatMin};
                DECLARE @xacSuatMax INT = {sqlXacSuatMax};

                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                        SELECT 
                            r.*,
                            u.DisplayName AS TenNhanVienXuLy,
                            COUNT(1) OVER() AS total_records
                        FROM TblRuiRo_DuAn r
                        LEFT JOIN [dbo].[aspnet_Users] u ON r.IdNhanVienXuLy = u.UserId 
                        WHERE r.DaXoa = 0 
                          AND r.IdDuAn = @projectId

                          AND (@singleKeyWord = N'%%' 
                                OR r.TenRuiRo LIKE @singleKeyWord 
                                OR u.DisplayName LIKE @singleKeyWord) 
                        
                          AND (@tenRuiRo = N'%%' OR r.TenRuiRo LIKE @tenRuiRo)
                          AND (@idNhanVienXuLy IS NULL OR r.IdNhanVienXuLy = @idNhanVienXuLy)
                          AND (@mucDoAnhHuong IS NULL OR r.MucDoAnhHuong = @mucDoAnhHuong)

                          AND (@xacSuatMin IS NULL OR r.XacSuatXayRa >= @xacSuatMin)
                          AND (@xacSuatMax IS NULL OR r.XacSuatXayRa <= @xacSuatMax)

                          AND (@mucDoRuiRo IS NULL 
                               OR (@mucDoRuiRo = 1 AND r.DiemRuiRo < 1.0)
                               OR (@mucDoRuiRo = 2 AND r.DiemRuiRo >= 1.0 AND r.DiemRuiRo < 2.0)
                               OR (@mucDoRuiRo = 3 AND r.DiemRuiRo >= 2.0 AND r.DiemRuiRo < 3.5)
                               OR (@mucDoRuiRo = 4 AND r.DiemRuiRo >= 3.5 AND r.DiemRuiRo < 4.5)
                               OR (@mucDoRuiRo = 5 AND r.DiemRuiRo >= 4.5))
                     ) AS T
                ) T1 WHERE RowNum > @startRow AND RowNum <= @endRow;";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public DataTable GetAllNhanVienDuAnById(Guid projectId)
        {
            string sql = $@"
                SELECT 
                    tvd.*, 
                    u.DisplayName AS TenNhanVien 
                FROM TblThanhVienDuAn tvd
                INNER JOIN [dbo].[aspnet_Users] u ON tvd.IdNhanVien = u.UserId 
                WHERE tvd.IdDuAn = '{projectId}'
                AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)";

            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            return dt;
        }
        public bool DeleteRisk(TblRuiRoDuAn item)
        {
            if (item == null) return false;

            item.DaXoa = true;
            item.NgayCapNhat = DateTime.Now;
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.Save();

            return true;
        }
        #endregion
    }
}
