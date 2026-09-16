using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class HopDongThucHienRepository : BaseRepository<TblHopDongThucHien>
    {
        public HopDongThucHienRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        #region Search paging

        public DataTable SearchPaging(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            parameters = parameters ?? new Dictionary<string, object>();

            Guid idKhachHang = GetGuidParameter(parameters, TblHopDongThucHien.Columns.IdKhachHang);
            string giaTriTu = GetDecimalSqlValue(parameters, "GiaTriHopDongTu");
            string giaTriDen = GetDecimalSqlValue(parameters, "GiaTriHopDongDen");
            string ngayKyTu = GetDateSqlValue(parameters, "NgayKyTu");
            string ngayKyDen = GetDateSqlValue(parameters, "NgayKyDen");
            string keyword = InlineQueryHelpers.SQLEncode(searchTerm ?? string.Empty);

            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @idKhachHang UNIQUEIDENTIFIER = '{idKhachHang}';
                DECLARE @giaTriTu DECIMAL(18, 2) = {giaTriTu};
                DECLARE @giaTriDen DECIMAL(18, 2) = {giaTriDen};
                DECLARE @ngayKyTu DATETIME = {ngayKyTu};
                DECLARE @ngayKyDen DATETIME = {ngayKyDen};
                DECLARE @singleKeyWord NVARCHAR(250) = N'%{keyword}%';

                SELECT *
                FROM
                (
                    SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.*
                    FROM
                    (
                        SELECT
                            hd.*,
                            kh.TenKhachHang,
                            COUNT(1) OVER() AS total_records
                        FROM dbo.TblHopDongThucHien hd
                        INNER JOIN dbo.TblKhachHang kh ON kh.IdKhachHang = hd.IdKhachHang
                        WHERE hd.DaXoa = 0
                          AND (@idKhachHang = '{Guid.Empty}' OR hd.IdKhachHang = @idKhachHang)
                          AND (@giaTriTu IS NULL OR hd.GiaTriHopDong >= @giaTriTu)
                          AND (@giaTriDen IS NULL OR hd.GiaTriHopDong <= @giaTriDen)
                          AND (@ngayKyTu IS NULL OR hd.NgayKy >= @ngayKyTu)
                          AND (@ngayKyDen IS NULL OR hd.NgayKy < DATEADD(DAY, 1, CAST(@ngayKyDen AS DATE)))
                          AND (@singleKeyWord = N'%%' OR hd.SoHopDong LIKE @singleKeyWord OR hd.TenHopDong LIKE @singleKeyWord OR kh.TenKhachHang LIKE @singleKeyWord)
                    ) AS T
                ) AS T1
                WHERE RowNum >= @startRow AND RowNum <= @endRow;";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);

            if (reader == null)
                return null;

            DataTable table = new DataTable();
            table.Load(reader);

            InlineQueryHelpers.GetTotal(ref table, out totalRecord);

            return table;
        }

        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return SearchPaging(string.Empty, parameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        #endregion

        #region CRUD

        public override TblHopDongThucHien GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.IdHopDongThucHienColumn).IsEqualTo(id)
                .And(TblHopDongThucHien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblHopDongThucHien>();
        }

        public TblHopDongThucHien GetBySoHopDong(string soHopDong)
        {
            if (string.IsNullOrWhiteSpace(soHopDong))
            {
                return null;
            }

            return new Select()
                .From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.SoHopDongColumn).IsEqualTo(soHopDong.Trim())
                // Không lọc DaXoa vì số hợp đồng phải duy nhất toàn hệ thống.
                .ExecuteSingle<TblHopDongThucHien>();
        }

        /// <summary>
        /// Returns active projects using the contract. The manager validates
        /// that exactly one project exists before it creates a contract document.
        /// </summary>
        public List<TblDuAn> GetActiveProjectsByContractId(Guid idHopDongThucHien)
        {
            if (idHopDongThucHien == Guid.Empty)
            {
                return new List<TblDuAn>();
            }

            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.IdHopDongThucHienColumn)
                .IsEqualTo(idHopDongThucHien)
                .And(TblDuAn.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteTypedList<TblDuAn>();
        }

        /// <summary>
        /// Reads the optional document link through SQL so this source can be
        /// compiled before SubSonic is regenerated for the additive column.
        /// The migration must still be installed before the feature is used.
        /// </summary>
        public Guid? GetLinkedDocumentId(Guid idHopDongThucHien)
        {
            return GetLinkedDocumentId(idHopDongThucHien, false);
        }

        /// <summary>
        /// The contract-document column is introduced by an additive migration
        /// and deliberately is not a generated SubSonic property.
        /// </summary>
        public bool HasDocumentLinkColumn()
        {
            const string sql = @"
                SELECT CASE
                    WHEN COL_LENGTH(N'dbo.TblHopDongThucHien', N'IdTaiLieu') IS NULL
                        THEN 0
                    ELSE 1
                END;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        /// <summary>
        /// Used inside a serializable transaction to prevent concurrent clicks
        /// from creating multiple canonical contract documents.
        /// </summary>
        public Guid? GetLinkedDocumentIdForUpdate(Guid idHopDongThucHien)
        {
            return GetLinkedDocumentId(idHopDongThucHien, true);
        }

        private Guid? GetLinkedDocumentId(
            Guid idHopDongThucHien,
            bool lockForUpdate)
        {
            if (idHopDongThucHien == Guid.Empty)
            {
                return null;
            }

            string tableHint = lockForUpdate
                ? " WITH (UPDLOCK, HOLDLOCK)"
                : string.Empty;
            string sql = $@"
                IF COL_LENGTH(N'dbo.TblHopDongThucHien', N'IdTaiLieu') IS NULL
                BEGIN
                    SELECT CAST(NULL AS UNIQUEIDENTIFIER) AS IdTaiLieu;
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    SELECT IdTaiLieu
                    FROM dbo.TblHopDongThucHien{tableHint}
                    WHERE IdHopDongThucHien = ''{idHopDongThucHien}''
                      AND DaXoa = 0;';

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
        /// Links a newly-created document only when the contract is still
        /// unlinked. The conditional update remains a final defensive check.
        /// </summary>
        public bool TryLinkDocument(
            Guid idHopDongThucHien,
            Guid idTaiLieu,
            string nguoiCapNhat,
            DateTime ngayCapNhat)
        {
            if (idHopDongThucHien == Guid.Empty || idTaiLieu == Guid.Empty)
            {
                return false;
            }

            string safeUser = InlineQueryHelpers.SQLEncode(
                (nguoiCapNhat ?? string.Empty).Trim());
            // The user name is embedded in a dynamic SQL string below, so it
            // needs one extra escaping pass for that string literal.
            string safeUserForDynamicSql = safeUser.Replace("'", "''");
            string safeDate = ngayCapNhat.ToString(
                "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture);
            string sql = $@"
                IF COL_LENGTH(N'dbo.TblHopDongThucHien', N'IdTaiLieu') IS NULL
                BEGIN
                    RAISERROR(N'Chưa cài cấu trúc liên kết hồ sơ cho hợp đồng.', 16, 1);
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    UPDATE dbo.TblHopDongThucHien
                    SET IdTaiLieu = ''{idTaiLieu}'',
                        NguoiCapNhat = N''{safeUserForDynamicSql}'',
                        NgayCapNhat = ''{safeDate}''
                    WHERE IdHopDongThucHien = ''{idHopDongThucHien}''
                      AND DaXoa = 0
                      AND IdTaiLieu IS NULL;
                    SELECT @@ROWCOUNT;';

                EXEC sys.sp_executesql @sql;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
        }

        /// <summary>
        /// Returns the project owning the linked document, if the document is
        /// still active. This is an integrity helper for manager validation.
        /// </summary>
        public Guid? GetLinkedDocumentProjectId(Guid idHopDongThucHien)
        {
            Guid? idTaiLieu = GetLinkedDocumentId(idHopDongThucHien);
            if (!idTaiLieu.HasValue || idTaiLieu.Value == Guid.Empty)
            {
                return null;
            }

            return new Select(TblTaiLieu.IdDuAnColumn)
                .From(TblTaiLieu.Schema)
                .Where(TblTaiLieu.IdTaiLieuColumn)
                .IsEqualTo(idTaiLieu.Value)
                .And(TblTaiLieu.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteScalar<Guid?>();
        }

        public bool IsSoHopDongExists(Guid excludedId, string soHopDong)
        {
            if (string.IsNullOrWhiteSpace(soHopDong))
            {
                return false;
            }

            Select select = new Select();
            select.From(TblHopDongThucHien.Schema)
                .Where(TblHopDongThucHien.SoHopDongColumn).IsEqualTo(soHopDong.Trim());

            if (excludedId != Guid.Empty)
            {
                select.And(TblHopDongThucHien.IdHopDongThucHienColumn).IsNotEqualTo(excludedId);
            }

            return select.ExecuteSingle<TblHopDongThucHien>() != null;
        }

        public override TblHopDongThucHien Insert(TblHopDongThucHien item)
        {
            item.Save();

            Guid id = item.IdHopDongThucHien;

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, id, item.NguoiTao).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblHopDongThucHien");
                }
            });

            return item;
        }

        public override TblHopDongThucHien Update(TblHopDongThucHien item)
        {
            Guid id = item.IdHopDongThucHien;

            TblHopDongThucHien oldItem = GetById(id);

            item.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(oldItem, item, _tableName, id, item.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log UPDATE action for TblHopDongThucHien");
                }
            });

            return item;
        }

        public override bool Delete(TblHopDongThucHien item)
        {
            if (item == null)
                return false;

            Guid id = item.IdHopDongThucHien;

            TblHopDongThucHien oldItem = GetById(id);

            if (oldItem == null)
                return false;

            oldItem.DaXoa = true;
            oldItem.NguoiCapNhat = item.NguoiCapNhat;
            oldItem.NgayCapNhat = item.NgayCapNhat;
            oldItem.Save();

            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(item, oldItem, _tableName, id, oldItem.NguoiCapNhat).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log DELETE action for TblHopDongThucHien");
                }
            });

            return true;
        }

        #endregion

        #region Parameter helpers

        private Guid GetGuidParameter(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null)
            {
                return Guid.Empty;
            }

            Guid value;
            return Guid.TryParse(Convert.ToString(parameters[key]), out value) ? value : Guid.Empty;
        }

        private string GetDecimalSqlValue(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null || string.IsNullOrWhiteSpace(Convert.ToString(parameters[key])))
            {
                return "NULL";
            }

            decimal value;
            string text = Convert.ToString(parameters[key]);

            bool parsed = decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value) || decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            return parsed ? value.ToString(CultureInfo.InvariantCulture) : "NULL";
        }

        private string GetDateSqlValue(Dictionary<string, object> parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null || string.IsNullOrWhiteSpace(Convert.ToString(parameters[key])))
            {
                return "NULL";
            }

            DateTime value;

            if (!DateTime.TryParse(Convert.ToString(parameters[key]), out value))
            {
                return "NULL";
            }

            return "'" + value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";
        }

        #endregion
    }
}
