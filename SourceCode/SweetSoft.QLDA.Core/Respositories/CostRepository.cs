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
    public class CostRepository : BaseRepository<TblChiPhi>
    {
        public CostRepository(AuditManager auditManager) : base(auditManager) { }
        public DataTable SearchCost(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;

            string tenKhoanChi = parameters != null && parameters.ContainsKey("TenKhoanChi") ? parameters["TenKhoanChi"]?.ToString() : null;
            string maChiPhi = parameters != null && parameters.ContainsKey("MaChiPhi") ? parameters["MaChiPhi"]?.ToString() : null;

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenKhoanChi NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenKhoanChi)}%';
                DECLARE @maChiPhi NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(maChiPhi)}%';

                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* FROM (
                        SELECT 
                            c.IdChiPhi, c.MaChiPhi, c.TenKhoanChi, c.DonGia, c.SoLuong, c.SoTien, 
                            c.NgayTao, c.TrangThai, 
                            u.DisplayName AS NhanVienYeuCau,
                            COUNT(1) OVER() AS total_records
                        FROM TblChiPhi c
                        LEFT JOIN [dbo].[aspnet_Users] u ON c.IdNhanVienDeNghi = u.UserId 
                        WHERE (c.DaXoa = 0 OR c.DaXoa IS NULL)
                          AND c.IdDuAn = @projectId
                          AND (@tenKhoanChi = N'%%' OR c.TenKhoanChi LIKE @tenKhoanChi)
                          AND (@maChiPhi = N'%%' OR c.MaChiPhi LIKE @maChiPhi)
                          AND (@singleKeyWord = N'%%'
                                OR c.TenKhoanChi LIKE @singleKeyWord
                                OR c.MaChiPhi LIKE @singleKeyWord)
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
        public string GenerateMaChiPhi(Guid projectId)
        {
            string sql = $@"
                DECLARE @projectId VARCHAR(36) = '{projectId}';
        
                SELECT ISNULL(MAX(TRY_CAST(REPLACE(MaChiPhi, 'Cos', '') AS INT)), 0) AS MaxNumber
                FROM TblChiPhi WITH (UPDLOCK, HOLDLOCK)
                WHERE IdDuAn = @projectId
                   AND MaChiPhi LIKE 'Cos%';
            ";

            int nextNumber = 1;

            using (System.Data.IDataReader reader = new SubSonic.InlineQuery().ExecuteReader(sql))
            {
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        if (reader["MaxNumber"] != DBNull.Value)
                        {
                            nextNumber = Convert.ToInt32(reader["MaxNumber"]) + 1;
                        }
                    }
                    reader.Close();
                }
            }

            return "Cos" + nextNumber;
        }
        public bool DeleteCost(TblChiPhi item)
        {
            if (item == null) return false;
            item.DaXoa = true;
            item.Save();

            return true;
        }
    }
}
