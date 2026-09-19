using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class CostRepository : BaseRepository<TblChiPhi>
    {
        public CostRepository(AuditManager auditManager) : base(auditManager) { }

        public override TblChiPhi GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return new Select()
                .From(TblChiPhi.Schema)
                .Where(TblChiPhi.IdChiPhiColumn).IsEqualTo(id)
                .And(TblChiPhi.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblChiPhi>();
        }

        /// <summary>
        /// Reads the optional cost-document link through SQL so this source
        /// remains compilable before SubSonic is regenerated for the additive
        /// IdTaiLieu column. The migration must still be installed before use.
        /// </summary>
        public bool HasDocumentLinkColumn()
        {
            const string sql = @"
                SELECT CASE
                    WHEN COL_LENGTH(N'dbo.TblChiPhi', N'IdTaiLieu') IS NULL
                        THEN 0
                    ELSE 1
                END;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        public Guid? GetLinkedDocumentId(Guid idChiPhi)
        {
            return GetLinkedDocumentId(idChiPhi, false);
        }

        /// <summary>
        /// Takes an update lock while a canonical cost document is being
        /// created so concurrent clicks cannot create duplicate documents.
        /// </summary>
        public Guid? GetLinkedDocumentIdForUpdate(Guid idChiPhi)
        {
            return GetLinkedDocumentId(idChiPhi, true);
        }

        private Guid? GetLinkedDocumentId(Guid idChiPhi, bool lockForUpdate)
        {
            if (idChiPhi == Guid.Empty)
            {
                return null;
            }

            string tableHint = lockForUpdate
                ? " WITH (UPDLOCK, HOLDLOCK)"
                : string.Empty;
            string sql = $@"
                IF COL_LENGTH(N'dbo.TblChiPhi', N'IdTaiLieu') IS NULL
                BEGIN
                    SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS IdTaiLieu;
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    SELECT IdTaiLieu
                    FROM dbo.TblChiPhi{tableHint}
                    WHERE IdChiPhi = ''{idChiPhi}''
                      AND (DaXoa = 0 OR DaXoa IS NULL);';

                EXEC sys.sp_executesql @sql;";

            using (IDataReader reader = new InlineQuery().ExecuteReader(sql))
            {
                if (reader == null || !reader.Read()
                    || reader["IdTaiLieu"] == DBNull.Value)
                {
                    return null;
                }

                Guid idTaiLieu;
                return Guid.TryParse(
                    Convert.ToString(reader["IdTaiLieu"]),
                    out idTaiLieu)
                    ? (Guid?)idTaiLieu
                    : null;
            }
        }

        /// <summary>
        /// Links a newly-created document only if the cost item is still
        /// unlinked. The conditional update is the last defence against a
        /// duplicate link after the serializable read above.
        /// </summary>
        public bool TryLinkDocument(Guid idChiPhi, Guid idTaiLieu)
        {
            if (idChiPhi == Guid.Empty || idTaiLieu == Guid.Empty)
            {
                return false;
            }

            string sql = $@"
                IF COL_LENGTH(N'dbo.TblChiPhi', N'IdTaiLieu') IS NULL
                BEGIN
                    RAISERROR(N'Chưa cài cấu trúc liên kết hồ sơ cho chi phí.', 16, 1);
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    UPDATE dbo.TblChiPhi
                    SET IdTaiLieu = ''{idTaiLieu}''
                    WHERE IdChiPhi = ''{idChiPhi}''
                      AND (DaXoa = 0 OR DaXoa IS NULL)
                      AND IdTaiLieu IS NULL;
                    SELECT @@ROWCOUNT;';

                EXEC sys.sp_executesql @sql;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        public DataTable SearchCost(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            totalRecord = 0;

            string tenKhoanChi = parameters != null && parameters.ContainsKey("TenKhoanChi") ? parameters["TenKhoanChi"]?.ToString() : null;
            string maChiPhi = parameters != null && parameters.ContainsKey("MaChiPhi") ? parameters["MaChiPhi"]?.ToString() : null;

            string idNhanVienDeNghi = parameters != null && parameters.ContainsKey("IdNhanVienDeNghi") ? parameters["IdNhanVienDeNghi"]?.ToString() : null;
            string soTienMinStr = parameters != null && parameters.ContainsKey("SoTienMin") ? parameters["SoTienMin"]?.ToString() : null;
            string soTienMaxStr = parameters != null && parameters.ContainsKey("SoTienMax") ? parameters["SoTienMax"]?.ToString() : null;

            string trangThaiStr = parameters != null && parameters.ContainsKey("TrangThai") ? parameters["TrangThai"]?.ToString() : null;

            string sqlIdNhanVien = string.IsNullOrEmpty(idNhanVienDeNghi) ? "NULL" : $"'{InlineQueryHelpers.SQLEncode(idNhanVienDeNghi)}'";
            string sqlSoTienMin = string.IsNullOrEmpty(soTienMinStr) ? "NULL" : soTienMinStr.Replace(",", "");
            string sqlSoTienMax = string.IsNullOrEmpty(soTienMaxStr) ? "NULL" : soTienMaxStr.Replace(",", "");

            string sqlTrangThai = string.IsNullOrEmpty(trangThaiStr) ? "NULL" : trangThaiStr;

            string sql = $@"
                DECLARE @startRow INT = {startRow};
                DECLARE @endRow INT = {endRow};
                DECLARE @projectId VARCHAR(36) = '{projectId}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                DECLARE @tenKhoanChi NVARCHAR(255) = N'%{InlineQueryHelpers.SQLEncode(tenKhoanChi)}%';
                DECLARE @maChiPhi NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(maChiPhi)}%';

                DECLARE @idNhanVienDeNghi VARCHAR(36) = {sqlIdNhanVien};
                DECLARE @soTienMin DECIMAL(18,2) = {sqlSoTienMin};
                DECLARE @soTienMax DECIMAL(18,2) = {sqlSoTienMax};
        
                -- Khai báo biến SQL cho Trạng thái
                DECLARE @trangThai INT = {sqlTrangThai};

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
                  
                          AND (@idNhanVienDeNghi IS NULL OR c.IdNhanVienDeNghi = @idNhanVienDeNghi)
                          AND (@soTienMin IS NULL OR c.SoTien >= @soTienMin)
                          AND (@soTienMax IS NULL OR c.SoTien <= @soTienMax)
                  
                          AND (@trangThai IS NULL OR c.TrangThai = @trangThai)

                    ) AS T
                ) T1 WHERE RowNum > @startRow AND RowNum <= @endRow;
            ";

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
