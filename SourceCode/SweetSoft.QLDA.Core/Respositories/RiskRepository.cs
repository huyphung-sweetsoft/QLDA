using SubSonic;
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

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenRuiRo NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenRuiRo)}%';

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
                          AND (@tenRuiRo IS NULL OR r.TenRuiRo LIKE N'%' + @tenRuiRo + '%')
                          AND (@singleKeyWord = N'%%'
                                OR r.TenRuiRo LIKE @singleKeyWord
                                OR u.DisplayName LIKE @singleKeyWord) 
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
        #endregion
    }
}
