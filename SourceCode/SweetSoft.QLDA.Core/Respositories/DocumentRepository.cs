using SubSonic;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentRepository : BaseRepository<TblTaiLieu>
    {
        public const string DocumentGroupParameter = "IdNhomTaiLieu";
        public const string HasOfficialFileParameter = "HasOfficialFile";

        public DocumentRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public DataTable SearchCompanyDocuments(
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
            string keyword = Encode(searchTerm, 500);
            string maTaiLieu = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.MaTaiLieu),
                100);
            string tenTaiLieu = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.TenTaiLieu),
                255);
            string moTa = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.MoTa),
                1000);
            string trangThaiTaiLieu = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.TrangThaiTaiLieu),
                30);
            string hinhThucKy = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.HinhThucKy),
                20);
            string trangThaiGuiKhach = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.TrangThaiGuiKhach),
                30);
            string trangThaiLuuTru = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.TrangThaiLuuTru),
                30);
            string ngayTaoFrom = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.NgayTao + "From"),
                50);
            string ngayTaoTo = Encode(
                GetParameterText(parameters, TblTaiLieu.Columns.NgayTao + "To"),
                50);

            string idNhomTaiLieuSql = GetGuidSql(
                parameters,
                DocumentGroupParameter);
            string idLoaiTaiLieuSql = GetGuidSql(
                parameters,
                TblTaiLieu.Columns.IdLoaiTaiLieu);
            string idNhanVienSql = GetGuidSql(
                parameters,
                TblTaiLieu.Columns.IdNhanVienPhuTrach);
            string canTrinhKySql = GetNullableBitSql(
                parameters,
                TblTaiLieu.Columns.CanTrinhKy);
            string canGuiKhachHangSql = GetNullableBitSql(
                parameters,
                TblTaiLieu.Columns.CanGuiKhachHang);
            string canLuuVatLySql = GetNullableBitSql(
                parameters,
                TblTaiLieu.Columns.CanLuuVatLy);
            string hasOfficialFileSql = GetNullableBitSql(
                parameters,
                HasOfficialFileParameter);
            string safeOrderBy = GetSafeOrderBy(orderBy);

            string sql = $@"
                DECLARE @offset INT = {safeOffset};
                DECLARE @endRow INT = {safeEndRow};
                DECLARE @keyword NVARCHAR(500) = N'%{keyword}%';
                DECLARE @maTaiLieu VARCHAR(100) = '%{maTaiLieu}%';
                DECLARE @tenTaiLieu NVARCHAR(255) = N'%{tenTaiLieu}%';
                DECLARE @moTa NVARCHAR(1000) = N'%{moTa}%';
                DECLARE @idNhomTaiLieu UNIQUEIDENTIFIER = {idNhomTaiLieuSql};
                DECLARE @idLoaiTaiLieu UNIQUEIDENTIFIER = {idLoaiTaiLieuSql};
                DECLARE @idNhanVien UNIQUEIDENTIFIER = {idNhanVienSql};
                DECLARE @trangThaiTaiLieu VARCHAR(30) = NULLIF('{trangThaiTaiLieu}', '');
                DECLARE @canTrinhKy BIT = {canTrinhKySql};
                DECLARE @hinhThucKy VARCHAR(20) = NULLIF('{hinhThucKy}', '');
                DECLARE @canGuiKhachHang BIT = {canGuiKhachHangSql};
                DECLARE @trangThaiGuiKhach VARCHAR(30) = NULLIF('{trangThaiGuiKhach}', '');
                DECLARE @canLuuVatLy BIT = {canLuuVatLySql};
                DECLARE @trangThaiLuuTru VARCHAR(30) = NULLIF('{trangThaiLuuTru}', '');
                DECLARE @hasOfficialFile BIT = {hasOfficialFileSql};
                DECLARE @ngayTaoFrom DATETIME = TRY_CONVERT(DATETIME, NULLIF('{ngayTaoFrom}', ''), 120);
                DECLARE @ngayTaoTo DATETIME = TRY_CONVERT(DATETIME, NULLIF('{ngayTaoTo}', ''), 120);

                ;WITH SearchResult AS
                (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY {safeOrderBy}) AS RowNum,
                        t.IdTaiLieu,
                        t.IdLoaiTaiLieu,
                        t.MaTaiLieu,
                        t.TenTaiLieu,
                        t.MoTa,
                        t.IdNhanVienPhuTrach,
                        t.CanTrinhKy,
                        t.HinhThucKy,
                        t.TrangThaiTaiLieu,
                        t.CanGuiKhachHang,
                        t.TrangThaiGuiKhach,
                        t.CanLuuVatLy,
                        t.TrangThaiLuuTru,
                        t.NguoiTao,
                        t.NgayTao,
                        t.NguoiCapNhat,
                        t.NgayCapNhat,
                        t.IdFileBanChinhThuc,
                        ISNULL(l.TenLoai, N'') AS TenLoai,
                        ISNULL(n.TenNhom, N'') AS TenNhom,
                        ISNULL(nv.TenNhanVien, N'') AS TenNhanVienPhuTrach,
                        ISNULL(u.Name, N'') AS TenFileChinhThuc,
                        ISNULL(u.OriginalFileName, N'') AS TenFileChinhThucGoc,
                        ISNULL(u.FileUrl, N'') AS FileChinhThucUrl,
                        COUNT(1) OVER() AS total_records
                    FROM TblTaiLieu t
                    LEFT JOIN TblLoaiTaiLieu l
                        ON l.IdLoaiTaiLieu = t.IdLoaiTaiLieu
                       AND l.DaXoa = 0
                    LEFT JOIN TblNhomTaiLieu n
                        ON n.IdNhomTaiLieu = l.IdNhomTaiLieu
                       AND n.DaXoa = 0
                    LEFT JOIN TblNhanVien nv
                        ON nv.IdNhanVien = t.IdNhanVienPhuTrach
                       AND nv.DaXoa = 0
                    LEFT JOIN TblUploadFile u
                        ON u.Id = t.IdFileBanChinhThuc
                       AND u.IsDeleted = 0
                    WHERE t.DaXoa = 0
                      AND t.IdDuAn IS NULL
                      AND
                      (
                          @keyword = N'%%'
                          OR t.MaTaiLieu LIKE @keyword
                          OR t.TenTaiLieu LIKE @keyword
                          OR ISNULL(t.MoTa, N'') LIKE @keyword
                          OR ISNULL(l.TenLoai, N'') LIKE @keyword
                          OR ISNULL(n.TenNhom, N'') LIKE @keyword
                          OR ISNULL(nv.TenNhanVien, N'') LIKE @keyword
                      )
                      AND (@maTaiLieu = '%%' OR t.MaTaiLieu LIKE @maTaiLieu)
                      AND (@tenTaiLieu = N'%%' OR t.TenTaiLieu LIKE @tenTaiLieu)
                      AND (@moTa = N'%%' OR ISNULL(t.MoTa, N'') LIKE @moTa)
                      AND (@idNhomTaiLieu IS NULL OR n.IdNhomTaiLieu = @idNhomTaiLieu)
                      AND (@idLoaiTaiLieu IS NULL OR t.IdLoaiTaiLieu = @idLoaiTaiLieu)
                      AND (@idNhanVien IS NULL OR t.IdNhanVienPhuTrach = @idNhanVien)
                      AND (@trangThaiTaiLieu IS NULL OR t.TrangThaiTaiLieu = @trangThaiTaiLieu)
                      AND (@canTrinhKy IS NULL OR t.CanTrinhKy = @canTrinhKy)
                      AND (@hinhThucKy IS NULL OR t.HinhThucKy = @hinhThucKy)
                      AND (@canGuiKhachHang IS NULL OR t.CanGuiKhachHang = @canGuiKhachHang)
                      AND (@trangThaiGuiKhach IS NULL OR t.TrangThaiGuiKhach = @trangThaiGuiKhach)
                      AND (@canLuuVatLy IS NULL OR t.CanLuuVatLy = @canLuuVatLy)
                      AND (@trangThaiLuuTru IS NULL OR t.TrangThaiLuuTru = @trangThaiLuuTru)
                      AND
                      (
                          @hasOfficialFile IS NULL
                          OR (@hasOfficialFile = 1 AND t.IdFileBanChinhThuc IS NOT NULL)
                          OR (@hasOfficialFile = 0 AND t.IdFileBanChinhThuc IS NULL)
                      )
                      AND (@ngayTaoFrom IS NULL OR t.NgayTao >= @ngayTaoFrom)
                      AND (@ngayTaoTo IS NULL OR t.NgayTao <= @ngayTaoTo)
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
            return SearchCompanyDocuments(
                string.Empty,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public override TblTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            TblTaiLieu item = TblTaiLieu.FetchByID(id);
            if (item == null || item.DaXoa || item.IdDuAn.HasValue)
                return null;

            return item;
        }

        public bool IsCodeExisted(string maTaiLieu, Guid excludeId)
        {
            string safeCode = Encode(maTaiLieu, 100);
            string excludeSql = excludeId == Guid.Empty
                ? "NULL"
                : "'" + excludeId + "'";
            string sql = $@"
                DECLARE @excludeId UNIQUEIDENTIFIER = {excludeSql};
                SELECT COUNT(1)
                FROM TblTaiLieu
                WHERE DaXoa = 0
                  AND IdDuAn IS NULL
                  AND MaTaiLieu = '{safeCode}'
                  AND (@excludeId IS NULL OR IdTaiLieu <> @excludeId);";

            return new InlineQuery().ExecuteScalar<int>(sql) > 0;
        }

        public List<TblNhanVien> GetAvailableEmployees()
        {
            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblNhanVien.Columns.TenNhanVien)
                .ExecuteTypedList<TblNhanVien>();
        }

        public TblNhanVien GetEmployeeById(Guid idNhanVien)
        {
            if (idNhanVien == Guid.Empty)
                return null;

            return new Select()
                .From(TblNhanVien.Schema)
                .Where(TblNhanVien.IdNhanVienColumn).IsEqualTo(idNhanVien)
                .And(TblNhanVien.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblNhanVien>();
        }

        public List<TblPhienBanTaiLieu> GetDocumentVersions(
            Guid idTaiLieu,
            bool includeDeleted)
        {
            if (idTaiLieu == Guid.Empty)
                return new List<TblPhienBanTaiLieu>();

            SqlQuery select = new Select()
                .From(TblPhienBanTaiLieu.Schema)
                .Where(TblPhienBanTaiLieu.IdTaiLieuColumn)
                .IsEqualTo(idTaiLieu);

            if (!includeDeleted)
            {
                select.And(TblPhienBanTaiLieu.DaXoaColumn)
                    .IsEqualTo(false);
            }

            return select.ExecuteTypedList<TblPhienBanTaiLieu>();
        }

        public List<TblUploadFile> GetDocumentVersionFiles(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new List<TblUploadFile>();

            return new Select()
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.RefIdColumn).IsEqualTo(idTaiLieu)
                .And(TblUploadFile.RefTypeColumn)
                .IsEqualTo(FileUploadTypes.DocumentVersion.ToString())
                .And(TblUploadFile.IsDeletedColumn).IsEqualTo(false)
                .ExecuteTypedList<TblUploadFile>();
        }

        public DataTable GetDocumentVersionsWithFiles(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT
                    p.IdPhienBanTaiLieu,
                    p.SoPhienBan,
                    p.MoTaPhienBan,
                    p.LaPhienBanHienTai,
                    p.NguoiTao,
                    p.NgayTao,
                    u.Id AS IdFile,
                    u.Name AS TenFile,
                    u.OriginalFileName AS TenFileGoc,
                    u.FileUrl
                FROM TblPhienBanTaiLieu p
                INNER JOIN TblUploadFile u
                    ON u.Id = p.IdFileNoiDung
                   AND u.IsDeleted = 0
                WHERE p.IdTaiLieu = '{idTaiLieu}'
                  AND p.DaXoa = 0
                ORDER BY
                    p.LaPhienBanHienTai DESC,
                    p.NgayTao DESC,
                    p.IdPhienBanTaiLieu DESC;";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            DataTable result = new DataTable();
            if (reader != null)
                result.Load(reader);
            return result;
        }

        public void InsertDocumentVersion(TblPhienBanTaiLieu item)
        {
            if (item != null)
                item.Save();
        }

        public void UpdateDocumentVersion(TblPhienBanTaiLieu item)
        {
            if (item != null)
                item.Save();
        }

        public bool HasSigningRecords(Guid idTaiLieu)
        {
            return HasVersionWorkflowRecords(
                idTaiLieu,
                "TblTrinhKyTaiLieu");
        }

        public bool HasCustomerDeliveryRecords(Guid idTaiLieu)
        {
            return HasVersionWorkflowRecords(
                idTaiLieu,
                "TblGuiNhanKhachHang");
        }

        public bool HasPhysicalStorageRecords(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            string sql = $@"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM TblLuuTruVatLy
                    WHERE IdTaiLieu = '{idTaiLieu}'
                      AND DaXoa = 0
                ) THEN 1 ELSE 0 END;";

            return new InlineQuery().ExecuteScalar<int>(sql) > 0;
        }

        public bool HasRelatedRecords(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            string sql = $@"
                SELECT CASE WHEN
                    EXISTS
                    (
                        SELECT 1 FROM TblPhienBanTaiLieu
                        WHERE IdTaiLieu = '{idTaiLieu}' AND DaXoa = 0
                    )
                    OR EXISTS
                    (
                        SELECT 1 FROM TblUploadFile
                        WHERE RefId = '{idTaiLieu}'
                          AND RefType = 'DocumentVersion'
                          AND IsDeleted = 0
                    )
                    OR EXISTS
                    (
                        SELECT 1 FROM TblLichSuTaiLieu
                        WHERE IdTaiLieu = '{idTaiLieu}'
                    )
                    OR EXISTS
                    (
                        SELECT 1 FROM TblLuuTruVatLy
                        WHERE IdTaiLieu = '{idTaiLieu}' AND DaXoa = 0
                    )
                    THEN 1 ELSE 0 END;";

            return new InlineQuery().ExecuteScalar<int>(sql) > 0;
        }

        private static bool HasVersionWorkflowRecords(
            Guid idTaiLieu,
            string workflowTable)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            string safeTable;
            if (string.Equals(
                    workflowTable,
                    "TblTrinhKyTaiLieu",
                    StringComparison.Ordinal))
            {
                safeTable = "TblTrinhKyTaiLieu";
            }
            else if (string.Equals(
                         workflowTable,
                         "TblGuiNhanKhachHang",
                         StringComparison.Ordinal))
            {
                safeTable = "TblGuiNhanKhachHang";
            }
            else
            {
                throw new ArgumentException(
                    "Bảng nghiệp vụ hồ sơ không hợp lệ.",
                    nameof(workflowTable));
            }

            string sql = $@"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM {safeTable} w
                    INNER JOIN TblPhienBanTaiLieu p
                        ON p.IdPhienBanTaiLieu = w.IdPhienBanTaiLieu
                       AND p.DaXoa = 0
                    WHERE p.IdTaiLieu = '{idTaiLieu}'
                      AND w.DaXoa = 0
                ) THEN 1 ELSE 0 END;";

            return new InlineQuery().ExecuteScalar<int>(sql) > 0;
        }

        public override TblTaiLieu Insert(TblTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblTaiLieu Update(TblTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblTaiLieu itemOld = GetById(itemNew.IdTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblTaiLieu item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblTaiLieu");
                }
            });
        }

        private void LogUpdate(TblTaiLieu itemOld, TblTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblTaiLieu");
                }
            });
        }

        private void LogDelete(TblTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblTaiLieu");
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
            return string.Equals(
                result,
                "null",
                StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : result;
        }

        private static string GetGuidSql(
            Dictionary<string, object> parameters,
            string key)
        {
            Guid value;
            return Guid.TryParse(GetParameterText(parameters, key), out value)
                && value != Guid.Empty
                ? "'" + value + "'"
                : "NULL";
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

        private static string Encode(string value, int maxLength)
        {
            return InlineQueryHelpers.SQLEncode(
                (value ?? string.Empty).Trim(),
                maxLength);
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
                case "MaTaiLieu":
                    sqlColumn = "t.MaTaiLieu";
                    break;
                case "TenTaiLieu":
                    sqlColumn = "t.TenTaiLieu";
                    break;
                case "TenLoai":
                    sqlColumn = "l.TenLoai";
                    break;
                case "TenNhom":
                    sqlColumn = "n.TenNhom";
                    break;
                case "TenNhanVienPhuTrach":
                    sqlColumn = "nv.TenNhanVien";
                    break;
                case "TrangThaiTaiLieu":
                    sqlColumn = "t.TrangThaiTaiLieu";
                    break;
                case "TrangThaiGuiKhach":
                    sqlColumn = "t.TrangThaiGuiKhach";
                    break;
                case "TrangThaiLuuTru":
                    sqlColumn = "t.TrangThaiLuuTru";
                    break;
                case "NgayTao":
                    sqlColumn = "t.NgayTao";
                    break;
                default:
                    return "t.NgayTao DESC, t.TenTaiLieu ASC, t.IdTaiLieu ASC";
            }

            return sqlColumn + " " + direction + ", t.IdTaiLieu ASC";
        }
    }
}
