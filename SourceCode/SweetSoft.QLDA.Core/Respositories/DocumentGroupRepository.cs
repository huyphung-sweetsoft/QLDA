using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentGroupRepository : BaseRepository<TblNhomTaiLieu>
    {
        public DocumentGroupRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public List<TblNhomTaiLieu> GetAll(string keyword = null)
        {
            List<TblNhomTaiLieu> items = new Select()
                .From(TblNhomTaiLieu.Schema)
                .Where(TblNhomTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblNhomTaiLieu>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchValue = keyword.Trim();

                items = items.Where(item =>
                        ContainsIgnoreCase(item.TenNhom, searchValue)
                        || ContainsIgnoreCase(item.MoTa, searchValue))
                    .ToList();
            }

            return items
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenNhom)
                .ToList();
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

            if (parameters == null)
                parameters = new Dictionary<string, object>();

            int safeOffset = Math.Max(0, rowOffset);
            int safeEndRow = Math.Max(safeOffset + 1, endRow);

            string keyword = InlineQueryHelpers.SQLEncode(
                searchTerm,
                500);

            string tenNhom = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNhomTaiLieu.Columns.TenNhom),
                150);

            string moTa = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNhomTaiLieu.Columns.MoTa),
                500);

            string kichHoatSql = GetNullableBitSql(
                parameters,
                TblNhomTaiLieu.Columns.KichHoat);

            string safeOrderBy = GetSafeOrderBy(orderBy);

            string sql = $@"
                DECLARE @offset INT = {safeOffset};
                DECLARE @endRow INT = {safeEndRow};
                DECLARE @keyword NVARCHAR(500) = N'%{keyword}%';
                DECLARE @tenNhom NVARCHAR(150) = N'%{tenNhom}%';
                DECLARE @moTa NVARCHAR(500) = N'%{moTa}%';
                DECLARE @kichHoat BIT = {kichHoatSql};

                ;WITH SearchResult AS
                (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY {safeOrderBy}) AS RowNum,
                        n.IdNhomTaiLieu,
                        n.TenNhom,
                        n.MoTa,
                        n.ThuTuHienThi,
                        n.KichHoat,
                        n.NguoiTao,
                        n.NgayTao,
                        n.NguoiCapNhat,
                        n.NgayCapNhat,
                        COUNT(1) OVER() AS total_records
                    FROM TblNhomTaiLieu n
                    WHERE n.DaXoa = 0
                        AND
                        (
                            @keyword = N'%%'
                            OR n.TenNhom LIKE @keyword
                            OR ISNULL(n.MoTa, N'') LIKE @keyword
                        )
                        AND
                        (
                            @kichHoat IS NULL
                            OR n.KichHoat = @kichHoat
                        )
                        AND
                        (
                            @tenNhom = N'%%'
                            OR n.TenNhom LIKE @tenNhom
                        )
                        AND
                        (
                            @moTa = N'%%'
                            OR ISNULL(n.MoTa, N'') LIKE @moTa
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
            InlineQueryHelpers.GetTotal(
                ref dataTable,
                out totalRecord);

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

        public override TblNhomTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblNhomTaiLieu.Schema)
                .Where(TblNhomTaiLieu.IdNhomTaiLieuColumn).IsEqualTo(id)
                .And(TblNhomTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblNhomTaiLieu>();
        }

        public bool IsNameExisted(string name, Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            Select select = new Select();
            select.From(TblNhomTaiLieu.Schema);
            select.Where(TblNhomTaiLieu.TenNhomColumn)
                .IsEqualTo(name.Trim());
            select.And(TblNhomTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblNhomTaiLieu.IdNhomTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public bool IsInUse(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.IdNhomTaiLieuColumn).IsEqualTo(id)
                .And(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public override TblNhomTaiLieu Insert(TblNhomTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblNhomTaiLieu Update(TblNhomTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblNhomTaiLieu itemOld = GetById(itemNew.IdNhomTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblNhomTaiLieu item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblNhomTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdNhomTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblNhomTaiLieu");
                }
            });
        }

        private void LogUpdate(
            TblNhomTaiLieu itemOld,
            TblNhomTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdNhomTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblNhomTaiLieu");
                }
            });
        }

        private void LogDelete(TblNhomTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdNhomTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblNhomTaiLieu");
                }
            });
        }

        private static bool ContainsIgnoreCase(
            string source,
            string searchValue)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            return source.IndexOf(
                searchValue,
                StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static string GetParameterText(
            Dictionary<string, object> parameters,
            string key)
        {
            if (parameters == null || string.IsNullOrEmpty(key))
                return string.Empty;

            object value;
            if (!parameters.TryGetValue(key, out value) || value == null)
                return string.Empty;

            string result = value.ToString().Trim();
            return string.Equals(
                result,
                "null",
                StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : result;
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
                    && string.Equals(
                        parts[1],
                        "DESC",
                        StringComparison.OrdinalIgnoreCase))
                {
                    direction = "DESC";
                }
            }

            string sqlColumn;
            switch (columnName)
            {
                case "TenNhom":
                    sqlColumn = "n.TenNhom";
                    break;

                case "MoTa":
                    sqlColumn = "n.MoTa";
                    break;

                case "ThuTuHienThi":
                    sqlColumn = "n.ThuTuHienThi";
                    break;

                case "KichHoat":
                    sqlColumn = "n.KichHoat";
                    break;

                case "NgayTao":
                    sqlColumn = "n.NgayTao";
                    break;

                default:
                    return "n.ThuTuHienThi ASC, n.TenNhom ASC, n.IdNhomTaiLieu ASC";
            }

            return sqlColumn
                + " "
                + direction
                + ", n.IdNhomTaiLieu ASC";
        }
    }
}
