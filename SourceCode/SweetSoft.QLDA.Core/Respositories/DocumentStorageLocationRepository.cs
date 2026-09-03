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
    public class DocumentStorageLocationRepository
        : BaseRepository<TblNoiLuuTru>
    {
        public DocumentStorageLocationRepository(
            AuditManager auditManager)
            : base(auditManager)
        {
        }

        public List<TblNoiLuuTru> GetAll(
            string keyword = null)
        {
            List<TblNoiLuuTru> items = new Select()
                .From(TblNoiLuuTru.Schema)
                .Where(TblNoiLuuTru.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteTypedList<TblNoiLuuTru>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchValue = keyword.Trim();

                items = items
                    .Where(item =>
                        ContainsIgnoreCase(
                            item.MaNoiLuuTru,
                            searchValue)
                        || ContainsIgnoreCase(
                            item.TenNoiLuuTru,
                            searchValue)
                        || ContainsIgnoreCase(
                            item.MoTa,
                            searchValue))
                    .ToList();
            }

            return items
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenNoiLuuTru)
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
            string code = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNoiLuuTru.Columns.MaNoiLuuTru),
                50);
            string name = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNoiLuuTru.Columns.TenNoiLuuTru),
                150);
            string description = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNoiLuuTru.Columns.MoTa),
                500);
            string storageLevel = InlineQueryHelpers.SQLEncode(
                GetParameterText(
                    parameters,
                    TblNoiLuuTru.Columns.CapLuuTru),
                20);
            Guid responsibleEmployeeId = GetGuidParameter(
                parameters,
                TblNoiLuuTru.Columns.IdNhanVienPhuTrach);
            string responsibleEmployeeIdSql =
                responsibleEmployeeId == Guid.Empty
                    ? "NULL"
                    : "'" + responsibleEmployeeId + "'";
            string activeSql = GetNullableBitSql(
                parameters,
                TblNoiLuuTru.Columns.KichHoat);
            string safeOrderBy = GetSafeOrderBy(orderBy);

            string sql = $@"
                DECLARE @offset INT = {safeOffset};
                DECLARE @endRow INT = {safeEndRow};
                DECLARE @keyword NVARCHAR(500) = N'%{keyword}%';
                DECLARE @code VARCHAR(50) = '%{code}%';
                DECLARE @name NVARCHAR(150) = N'%{name}%';
                DECLARE @description NVARCHAR(500) = N'%{description}%';
                DECLARE @storageLevel VARCHAR(20) = '{storageLevel}';
                DECLARE @responsibleEmployeeId UNIQUEIDENTIFIER = {responsibleEmployeeIdSql};
                DECLARE @active BIT = {activeSql};

                ;WITH StorageTree AS
                (
                    SELECT
                        n.IdNoiLuuTru,
                        n.IdNoiLuuTruCha,
                        n.MaNoiLuuTru,
                        n.TenNoiLuuTru,
                        n.CapLuuTru,
                        n.IdNhanVienPhuTrach,
                        n.MoTa,
                        n.ThuTuHienThi,
                        n.KichHoat,
                        n.NguoiTao,
                        n.NgayTao,
                        n.NguoiCapNhat,
                        n.NgayCapNhat,
                        CAST(0 AS INT) AS Depth,
                        CAST(n.TenNoiLuuTru AS NVARCHAR(MAX)) AS StoragePath,
                        CAST(
                            RIGHT(
                                REPLICATE('0', 10)
                                + CONVERT(VARCHAR(10), n.ThuTuHienThi),
                                10)
                            + N'_' + LOWER(n.TenNoiLuuTru)
                            + N'_' + CONVERT(NVARCHAR(36), n.IdNoiLuuTru)
                            AS NVARCHAR(MAX)) AS TreeOrder
                    FROM TblNoiLuuTru n
                    WHERE n.DaXoa = 0
                        AND
                        (
                            n.IdNoiLuuTruCha IS NULL
                            OR NOT EXISTS
                            (
                                SELECT 1
                                FROM TblNoiLuuTru parent
                                WHERE parent.IdNoiLuuTru = n.IdNoiLuuTruCha
                                    AND parent.DaXoa = 0
                            )
                        )

                    UNION ALL

                    SELECT
                        child.IdNoiLuuTru,
                        child.IdNoiLuuTruCha,
                        child.MaNoiLuuTru,
                        child.TenNoiLuuTru,
                        child.CapLuuTru,
                        child.IdNhanVienPhuTrach,
                        child.MoTa,
                        child.ThuTuHienThi,
                        child.KichHoat,
                        child.NguoiTao,
                        child.NgayTao,
                        child.NguoiCapNhat,
                        child.NgayCapNhat,
                        parent.Depth + 1,
                        CAST(
                            parent.StoragePath + N' / ' + child.TenNoiLuuTru
                            AS NVARCHAR(MAX)),
                        CAST(
                            parent.TreeOrder + N'/'
                            + RIGHT(
                                REPLICATE('0', 10)
                                + CONVERT(VARCHAR(10), child.ThuTuHienThi),
                                10)
                            + N'_' + LOWER(child.TenNoiLuuTru)
                            + N'_' + CONVERT(NVARCHAR(36), child.IdNoiLuuTru)
                            AS NVARCHAR(MAX))
                    FROM TblNoiLuuTru child
                    INNER JOIN StorageTree parent
                        ON parent.IdNoiLuuTru = child.IdNoiLuuTruCha
                    WHERE child.DaXoa = 0
                ),
                SearchResult AS
                (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY {safeOrderBy}) AS RowNum,
                        storage.IdNoiLuuTru,
                        storage.IdNoiLuuTruCha,
                        storage.MaNoiLuuTru,
                        storage.TenNoiLuuTru,
                        storage.CapLuuTru,
                        storage.IdNhanVienPhuTrach,
                        storage.MoTa,
                        storage.ThuTuHienThi,
                        storage.KichHoat,
                        storage.NguoiTao,
                        storage.NgayTao,
                        storage.NguoiCapNhat,
                        storage.NgayCapNhat,
                        storage.Depth,
                        storage.StoragePath,
                        ISNULL(employee.DisplayName, N'') AS TenNhanVien,
                        COUNT(1) OVER() AS total_records
                    FROM StorageTree storage
                    LEFT JOIN aspnet_Users employee
                        ON employee.UserId = storage.IdNhanVienPhuTrach
                        AND employee.IsDeleted = 0
                        AND employee.LaNhanVien = 1
                    WHERE
                        (
                            @keyword = N'%%'
                            OR storage.MaNoiLuuTru LIKE @keyword
                            OR storage.TenNoiLuuTru LIKE @keyword
                            OR ISNULL(storage.MoTa, N'') LIKE @keyword
                            OR storage.StoragePath LIKE @keyword
                            OR ISNULL(employee.DisplayName, N'') LIKE @keyword
                        )
                        AND
                        (
                            @active IS NULL
                            OR storage.KichHoat = @active
                        )
                        AND
                        (
                            @storageLevel = ''
                            OR storage.CapLuuTru = @storageLevel
                        )
                        AND
                        (
                            @responsibleEmployeeId IS NULL
                            OR storage.IdNhanVienPhuTrach = @responsibleEmployeeId
                        )
                        AND
                        (
                            @code = '%%'
                            OR storage.MaNoiLuuTru LIKE @code
                        )
                        AND
                        (
                            @name = N'%%'
                            OR storage.TenNoiLuuTru LIKE @name
                        )
                        AND
                        (
                            @description = N'%%'
                            OR ISNULL(storage.MoTa, N'') LIKE @description
                        )
                )

                SELECT *
                FROM SearchResult
                WHERE RowNum > @offset
                    AND RowNum <= @endRow
                ORDER BY RowNum
                OPTION (MAXRECURSION 100);";

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

        public override TblNoiLuuTru GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblNoiLuuTru.Schema)
                .Where(TblNoiLuuTru.IdNoiLuuTruColumn)
                .IsEqualTo(id)
                .And(TblNoiLuuTru.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteSingle<TblNoiLuuTru>();
        }

        public bool IsCodeExisted(
            string code,
            Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            Select select = new Select();

            select
                .From(TblNoiLuuTru.Schema)
                .Where(TblNoiLuuTru.MaNoiLuuTruColumn)
                .IsEqualTo(code.Trim())
                .And(TblNoiLuuTru.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select
                    .And(TblNoiLuuTru.IdNoiLuuTruColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public bool IsNameExisted(
            string name,
            Guid? parentId,
            Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string normalizedName = name.Trim();

            return GetAll().Any(item =>
                item.IdNoiLuuTru != excludeId
                && IsSameParent(
                    item.IdNoiLuuTruCha,
                    parentId)
                && string.Equals(
                    item.TenNoiLuuTru,
                    normalizedName,
                    StringComparison.CurrentCultureIgnoreCase));
        }

        public bool HasChildren(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return new Select()
                .From(TblNoiLuuTru.Schema)
                .Where(TblNoiLuuTru.IdNoiLuuTruChaColumn)
                .IsEqualTo(id)
                .And(TblNoiLuuTru.DaXoaColumn)
                .IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public bool IsInUse(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return new Select()
                .From(TblLuuTruVatLy.Schema)
                .Where(TblLuuTruVatLy.IdNoiLuuTruColumn)
                .IsEqualTo(id)
                .And(TblLuuTruVatLy.DaXoaColumn)
                .IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public List<AspnetUser> GetAvailableEmployees()
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.IsDeletedColumn)
                .IsEqualTo(false)
                .And(AspnetUser.LaNhanVienColumn)
                .IsEqualTo(true)
                .ExecuteTypedList<AspnetUser>()
                .OrderBy(item => item.DisplayName)
                .ToList();
        }

        public AspnetUser GetEmployeeById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn)
                .IsEqualTo(id)
                .And(AspnetUser.IsDeletedColumn)
                .IsEqualTo(false)
                .And(AspnetUser.LaNhanVienColumn)
                .IsEqualTo(true)
                .ExecuteSingle<AspnetUser>();
        }

        public override TblNoiLuuTru Insert(
            TblNoiLuuTru item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);

            return item;
        }

        public override TblNoiLuuTru Update(
            TblNoiLuuTru itemNew)
        {
            if (itemNew == null)
                return null;

            TblNoiLuuTru itemOld =
                GetById(itemNew.IdNoiLuuTru);

            itemNew.Save();
            LogUpdate(itemOld, itemNew);

            return itemNew;
        }

        public override bool Delete(
            TblNoiLuuTru item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();

            LogDelete(item);

            return true;
        }

        private void LogCreate(TblNoiLuuTru item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdNoiLuuTru)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblNoiLuuTru");
                }
            });
        }

        private void LogUpdate(
            TblNoiLuuTru itemOld,
            TblNoiLuuTru itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdNoiLuuTru,
                            itemNew.NguoiCapNhat
                                ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblNoiLuuTru");
                }
            });
        }

        private void LogDelete(TblNoiLuuTru item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdNoiLuuTru)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblNoiLuuTru");
                }
            });
        }

        private static bool IsSameParent(
            Guid? firstParentId,
            Guid? secondParentId)
        {
            if (!firstParentId.HasValue
                && !secondParentId.HasValue)
            {
                return true;
            }

            return firstParentId.HasValue
                && secondParentId.HasValue
                && firstParentId.Value
                    == secondParentId.Value;
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

        private static Guid GetGuidParameter(
            Dictionary<string, object> parameters,
            string key)
        {
            Guid result;
            return Guid.TryParse(
                GetParameterText(parameters, key),
                out result)
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
                case "TreeOrder":
                    return "storage.TreeOrder ASC";

                case "TenNoiLuuTru":
                    sqlColumn = "storage.TenNoiLuuTru";
                    break;

                case "MaNoiLuuTru":
                    sqlColumn = "storage.MaNoiLuuTru";
                    break;

                case "CapLuuTru":
                    sqlColumn = "storage.CapLuuTru";
                    break;

                case "StoragePath":
                    sqlColumn = "storage.StoragePath";
                    break;

                case "TenNhanVien":
                    sqlColumn = "employee.DisplayName";
                    break;

                case "ThuTuHienThi":
                    sqlColumn = "storage.ThuTuHienThi";
                    break;

                case "KichHoat":
                    sqlColumn = "storage.KichHoat";
                    break;

                default:
                    return "storage.TreeOrder ASC";
            }

            return sqlColumn
                + " "
                + direction
                + ", storage.IdNoiLuuTru ASC";
        }
    }
}
