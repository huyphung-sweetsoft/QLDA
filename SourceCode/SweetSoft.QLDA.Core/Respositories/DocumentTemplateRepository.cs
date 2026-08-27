using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentTemplateRepository : BaseRepository<TblMauTaiLieu>
    {
        public const string HasTemplateFileParameter = "HasTemplateFile";

        public DocumentTemplateRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public DataTable SearchPaging(
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            totalRecord = 0;
            parameters = parameters ?? new Dictionary<string, object>();

            int safeOffset = Math.Max(0, rowOffset);
            int safeEndRow = Math.Max(safeOffset + 1, endRow);
            string keyword = InlineQueryHelpers.SQLEncode(searchTerm, 500);
            string tenMau = InlineQueryHelpers.SQLEncode(
                GetParameterText(parameters, TblMauTaiLieu.Columns.TenMau),
                200);
            string phienBanMau = InlineQueryHelpers.SQLEncode(
                GetParameterText(parameters, TblMauTaiLieu.Columns.PhienBanMau),
                20);
            string moTa = InlineQueryHelpers.SQLEncode(
                GetParameterText(parameters, TblMauTaiLieu.Columns.MoTa),
                500);

            Guid idLoaiTaiLieu = GetGuidParameter(
                parameters,
                TblMauTaiLieu.Columns.IdLoaiTaiLieu);
            string idLoaiTaiLieuSql = idLoaiTaiLieu == Guid.Empty
                ? "NULL"
                : "'" + idLoaiTaiLieu + "'";
            string kichHoatSql = GetNullableBitSql(
                parameters,
                TblMauTaiLieu.Columns.KichHoat);
            string laMauMacDinhSql = GetNullableBitSql(
                parameters,
                TblMauTaiLieu.Columns.LaMauMacDinh);
            string hasTemplateFileSql = GetNullableBitSql(
                parameters,
                HasTemplateFileParameter);
            string safeOrderBy = GetSafeOrderBy(orderBy);

            string sql = $@"
                DECLARE @offset INT = {safeOffset};
                DECLARE @endRow INT = {safeEndRow};
                DECLARE @keyword NVARCHAR(500) = N'%{keyword}%';
                DECLARE @tenMau NVARCHAR(200) = N'%{tenMau}%';
                DECLARE @phienBanMau VARCHAR(20) = '%{phienBanMau}%';
                DECLARE @moTa NVARCHAR(500) = N'%{moTa}%';
                DECLARE @idLoaiTaiLieu UNIQUEIDENTIFIER = {idLoaiTaiLieuSql};
                DECLARE @kichHoat BIT = {kichHoatSql};
                DECLARE @laMauMacDinh BIT = {laMauMacDinhSql};
                DECLARE @hasTemplateFile BIT = {hasTemplateFileSql};

                ;WITH SearchResult AS
                (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY {safeOrderBy}) AS RowNum,
                        m.IdMauTaiLieu,
                        m.IdLoaiTaiLieu,
                        m.TenMau,
                        m.PhienBanMau,
                        m.MoTa,
                        m.LaMauMacDinh,
                        m.KichHoat,
                        m.NguoiTao,
                        m.NgayTao,
                        m.NguoiCapNhat,
                        m.NgayCapNhat,
                        m.IdFileMau,
                        ISNULL(l.TenLoai, N'') AS TenLoai,
                        ISNULL(n.TenNhom, N'') AS TenNhom,
                        ISNULL(u.Name, N'') AS TenFile,
                        ISNULL(u.OriginalFileName, N'') AS TenFileGoc,
                        ISNULL(u.FileUrl, N'') AS FileUrl,
                        ISNULL(u.Ext, N'') AS PhanMoRong,
                        COUNT(1) OVER() AS total_records
                    FROM TblMauTaiLieu m
                    LEFT JOIN TblLoaiTaiLieu l
                        ON l.IdLoaiTaiLieu = m.IdLoaiTaiLieu
                        AND l.DaXoa = 0
                    LEFT JOIN TblNhomTaiLieu n
                        ON n.IdNhomTaiLieu = l.IdNhomTaiLieu
                        AND n.DaXoa = 0
                    LEFT JOIN TblUploadFile u
                        ON u.Id = m.IdFileMau
                        AND u.IsDeleted = 0
                    WHERE m.DaXoa = 0
                        AND
                        (
                            @keyword = N'%%'
                            OR m.TenMau LIKE @keyword
                            OR m.PhienBanMau LIKE @keyword
                            OR ISNULL(m.MoTa, N'') LIKE @keyword
                            OR ISNULL(l.TenLoai, N'') LIKE @keyword
                            OR ISNULL(n.TenNhom, N'') LIKE @keyword
                            OR ISNULL(u.Name, N'') LIKE @keyword
                            OR ISNULL(u.OriginalFileName, N'') LIKE @keyword
                        )
                        AND (@idLoaiTaiLieu IS NULL OR m.IdLoaiTaiLieu = @idLoaiTaiLieu)
                        AND (@kichHoat IS NULL OR m.KichHoat = @kichHoat)
                        AND (@laMauMacDinh IS NULL OR m.LaMauMacDinh = @laMauMacDinh)
                        AND (@tenMau = N'%%' OR m.TenMau LIKE @tenMau)
                        AND (@phienBanMau = '%%' OR m.PhienBanMau LIKE @phienBanMau)
                        AND (@moTa = N'%%' OR ISNULL(m.MoTa, N'') LIKE @moTa)
                        AND
                        (
                            @hasTemplateFile IS NULL
                            OR (@hasTemplateFile = 1 AND m.IdFileMau IS NOT NULL)
                            OR (@hasTemplateFile = 0 AND m.IdFileMau IS NULL)
                        )
                )

                SELECT *
                FROM SearchResult
                WHERE RowNum > @offset
                    AND RowNum <= @endRow
                ORDER BY RowNum;";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            if (reader == null)
                return null;

            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            InlineQueryHelpers.GetTotal(ref dataTable, out totalRecord);
            return dataTable;
        }

        public override DataTable SearchPaging(
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return SearchPaging(
                string.Empty,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public override TblMauTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblMauTaiLieu.Schema)
                .Where(TblMauTaiLieu.IdMauTaiLieuColumn).IsEqualTo(id)
                .And(TblMauTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblMauTaiLieu>();
        }

        public bool IsNameAndVersionExisted(
            Guid idLoaiTaiLieu,
            string tenMau,
            string phienBanMau,
            Guid excludeId)
        {
            Select select = new Select();
            select.From(TblMauTaiLieu.Schema);
            select.Where(TblMauTaiLieu.IdLoaiTaiLieuColumn)
                .IsEqualTo(idLoaiTaiLieu);
            select.And(TblMauTaiLieu.TenMauColumn)
                .IsEqualTo(tenMau);
            select.And(TblMauTaiLieu.PhienBanMauColumn)
                .IsEqualTo(phienBanMau);
            select.And(TblMauTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblMauTaiLieu.IdMauTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public bool HasOtherDefault(Guid idLoaiTaiLieu, Guid excludeId)
        {
            Select select = new Select();
            select.From(TblMauTaiLieu.Schema);
            select.Where(TblMauTaiLieu.IdLoaiTaiLieuColumn)
                .IsEqualTo(idLoaiTaiLieu);
            select.And(TblMauTaiLieu.LaMauMacDinhColumn)
                .IsEqualTo(true);
            select.And(TblMauTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblMauTaiLieu.IdMauTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public override TblMauTaiLieu Insert(TblMauTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblMauTaiLieu Update(TblMauTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblMauTaiLieu itemOld = GetById(itemNew.IdMauTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblMauTaiLieu item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblMauTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdMauTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log CREATE action for TblMauTaiLieu");
                }
            });
        }

        private void LogUpdate(TblMauTaiLieu itemOld, TblMauTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdMauTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log changes for TblMauTaiLieu");
                }
            });
        }

        private void LogDelete(TblMauTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdMauTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log DELETE action for TblMauTaiLieu");
                }
            });
        }

        private static string GetParameterText(
            Dictionary<string, object> parameters,
            string key)
        {
            object value;
            if (parameters == null
                || string.IsNullOrEmpty(key)
                || !parameters.TryGetValue(key, out value)
                || value == null)
            {
                return string.Empty;
            }

            string result = value.ToString().Trim();
            return string.Equals(result, "null", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : result;
        }

        private static Guid GetGuidParameter(
            Dictionary<string, object> parameters,
            string key)
        {
            Guid result;
            return Guid.TryParse(GetParameterText(parameters, key), out result)
                ? result
                : Guid.Empty;
        }

        private static string GetNullableBitSql(
            Dictionary<string, object> parameters,
            string key)
        {
            string value = GetParameterText(parameters, key);
            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return "1";
            }

            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            return "NULL";
        }

        private static string GetSafeOrderBy(string orderBy)
        {
            string columnName = string.Empty;
            string direction = "ASC";

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                string[] parts = orderBy.Trim().Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    columnName = parts[0];
                if (parts.Length > 1
                    && string.Equals(parts[1], "DESC", StringComparison.OrdinalIgnoreCase))
                {
                    direction = "DESC";
                }
            }

            string sqlColumn;
            switch (columnName)
            {
                case "TenMau":
                    sqlColumn = "m.TenMau";
                    break;
                case "PhienBanMau":
                    sqlColumn = "m.PhienBanMau";
                    break;
                case "TenLoai":
                    sqlColumn = "l.TenLoai";
                    break;
                case "TenNhom":
                    sqlColumn = "n.TenNhom";
                    break;
                case "LaMauMacDinh":
                    sqlColumn = "m.LaMauMacDinh";
                    break;
                case "TenFile":
                    sqlColumn = "u.Name";
                    break;
                case "KichHoat":
                    sqlColumn = "m.KichHoat";
                    break;
                case "NgayTao":
                    sqlColumn = "m.NgayTao";
                    break;
                default:
                    return "m.NgayTao DESC, m.TenMau ASC, m.IdMauTaiLieu ASC";
            }

            return sqlColumn + " " + direction + ", m.IdMauTaiLieu ASC";
        }
    }
}
