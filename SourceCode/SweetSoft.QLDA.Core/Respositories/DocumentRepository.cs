using SubSonic;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.SysManager.Models;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Hosting;

namespace SweetSoft.QLDA.Core.Respositories
{
    public sealed class DocumentVersionFileDeletionResult
    {
        public List<TblUploadFile> DeletedFiles { get; set; } =
            new List<TblUploadFile>();

        public List<TblPhienBanTaiLieu> DeletedVersions { get; set; } =
            new List<TblPhienBanTaiLieu>();

        public string WarningMessage { get; set; }
    }

    public sealed class DocumentSigningOperationResult
    {
        public Guid IdTrinhKyTaiLieu { get; set; }

        public Guid IdPhienBanTaiLieu { get; set; }

        public Guid IdFile { get; set; }
    }

    public class DocumentRepository : BaseRepository<TblTaiLieu>
    {
        public const string DocumentGroupParameter = "IdNhomTaiLieu";
        public const string HasOfficialFileParameter = "HasOfficialFile";
        public const string DocumentScopeParameter = "DocumentScope";
        public const string DocumentScopeAll = "ALL";
        public const string DocumentScopeCompany = "COMPANY";
        public const string DocumentScopeProject = "PROJECT";

        public DocumentRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public DataTable SearchDocuments(
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
            string documentScope = GetParameterText(
                    parameters,
                    DocumentScopeParameter)
                .ToUpperInvariant();

            if (documentScope != DocumentScopeCompany
                && documentScope != DocumentScopeProject)
            {
                documentScope = DocumentScopeAll;
            }

            string idNhomTaiLieuSql = GetGuidSql(
                parameters,
                DocumentGroupParameter);
            string idLoaiTaiLieuSql = GetGuidSql(
                parameters,
                TblTaiLieu.Columns.IdLoaiTaiLieu);
            string idDuAnSql = GetGuidSql(
                parameters,
                TblTaiLieu.Columns.IdDuAn);
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
                DECLARE @documentScope VARCHAR(20) = '{documentScope}';
                DECLARE @idNhomTaiLieu UNIQUEIDENTIFIER = {idNhomTaiLieuSql};
                DECLARE @idLoaiTaiLieu UNIQUEIDENTIFIER = {idLoaiTaiLieuSql};
                DECLARE @idDuAn UNIQUEIDENTIFIER = {idDuAnSql};
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
                        t.IdDuAn,
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
                        ISNULL(d.MaDuAn, '') AS MaDuAn,
                        ISNULL(d.TenDuAn, N'') AS TenDuAn,
                        ISNULL(nv.DisplayName, N'') AS TenNhanVienPhuTrach,
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
                    LEFT JOIN TblDuAn d
                        ON d.IdDuAn = t.IdDuAn
                       AND d.DaXoa = 0
                    LEFT JOIN aspnet_Users nv
                        ON nv.UserId = t.IdNhanVienPhuTrach
                       AND nv.IsDeleted = 0
                       AND nv.LaNhanVien = 1
                    LEFT JOIN TblUploadFile u
                        ON u.Id = t.IdFileBanChinhThuc
                       AND u.IsDeleted = 0
                    WHERE t.DaXoa = 0
                      AND
                      (
                          @documentScope = 'ALL'
                          OR (@documentScope = 'COMPANY' AND t.IdDuAn IS NULL)
                          OR (@documentScope = 'PROJECT' AND t.IdDuAn IS NOT NULL)
                      )
                      AND
                      (
                          @keyword = N'%%'
                          OR t.MaTaiLieu LIKE @keyword
                          OR t.TenTaiLieu LIKE @keyword
                          OR ISNULL(t.MoTa, N'') LIKE @keyword
                          OR ISNULL(l.TenLoai, N'') LIKE @keyword
                          OR ISNULL(n.TenNhom, N'') LIKE @keyword
                          OR ISNULL(d.MaDuAn, '') LIKE @keyword
                          OR ISNULL(d.TenDuAn, N'') LIKE @keyword
                          OR ISNULL(nv.DisplayName, N'') LIKE @keyword
                      )
                      AND (@maTaiLieu = '%%' OR t.MaTaiLieu LIKE @maTaiLieu)
                      AND (@tenTaiLieu = N'%%' OR t.TenTaiLieu LIKE @tenTaiLieu)
                      AND (@moTa = N'%%' OR ISNULL(t.MoTa, N'') LIKE @moTa)
                      AND (@idNhomTaiLieu IS NULL OR n.IdNhomTaiLieu = @idNhomTaiLieu)
                      AND (@idLoaiTaiLieu IS NULL OR t.IdLoaiTaiLieu = @idLoaiTaiLieu)
                      AND (@idDuAn IS NULL OR t.IdDuAn = @idDuAn)
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

        public DataTable SearchCompanyDocuments(
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            Dictionary<string, object> companyParameters = parameters == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(parameters);

            companyParameters[DocumentScopeParameter] =
                DocumentScopeCompany;
            companyParameters.Remove(TblTaiLieu.Columns.IdDuAn);

            return SearchDocuments(
                searchTerm,
                companyParameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
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

        public List<AspnetUser> GetAvailableEmployees()
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .And(AspnetUser.LaNhanVienColumn).IsEqualTo(true)
                .OrderAsc(AspnetUser.Columns.DisplayName)
                .ExecuteTypedList<AspnetUser>();
        }

        public List<AspnetUser> GetAvailableSigningUsers()
        {
            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .And(AspnetUser.IsActivatedColumn).IsEqualTo(true)
                .And(AspnetUser.LaNhanVienColumn).IsEqualTo(true)
                .OrderAsc(AspnetUser.Columns.DisplayName)
                .ExecuteTypedList<AspnetUser>();
        }

        public AspnetUser GetAvailableSigningUser(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(userId)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .And(AspnetUser.IsActivatedColumn).IsEqualTo(true)
                .And(AspnetUser.LaNhanVienColumn).IsEqualTo(true)
                .ExecuteSingle<AspnetUser>();
        }

        public List<TblDuAn> GetAvailableProjects()
        {
            return new Select()
                .From(TblDuAn.Schema)
                .Where(TblDuAn.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblDuAn.Columns.TenDuAn)
                .ExecuteTypedList<TblDuAn>();
        }

        public AspnetUser GetEmployeeById(Guid idNhanVien)
        {
            if (idNhanVien == Guid.Empty)
                return null;

            return new Select()
                .From(AspnetUser.Schema)
                .Where(AspnetUser.UserIdColumn).IsEqualTo(idNhanVien)
                .And(AspnetUser.IsDeletedColumn).IsEqualTo(false)
                .And(AspnetUser.LaNhanVienColumn).IsEqualTo(true)
                .ExecuteSingle<AspnetUser>();
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

        /// <summary>
        /// Removes document-version upload rows and updates every version FK
        /// that points at them in one database transaction. Physical files are
        /// deliberately left for the manager after the transaction commits.
        /// </summary>
        public DocumentVersionFileDeletionResult DeleteDocumentVersionFiles(
            Guid idTaiLieu,
            IEnumerable<Guid> fileIds,
            string currentUserName,
            Guid currentUserId,
            DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty)
                throw new InvalidOperationException(
                    "Không xác định được hồ sơ cần cập nhật.");

            List<Guid> requestedFileIds = (fileIds
                    ?? Enumerable.Empty<Guid>())
                .Where(fileId => fileId != Guid.Empty)
                .Distinct()
                .ToList();
            if (requestedFileIds.Count == 0)
                return new DocumentVersionFileDeletionResult();

            string safeUserName = string.IsNullOrWhiteSpace(currentUserName)
                ? "[System]"
                : currentUserName.Trim();
            if (safeUserName.Length > 150)
                safeUserName = safeUserName.Substring(0, 150);

            Dictionary<string, object> parameters;
            string fileIdSql = BuildGuidParameterList(
                requestedFileIds,
                "FileId",
                out parameters);
            parameters["@DocumentId"] = idTaiLieu;
            parameters["@RefType"] = FileUploadTypes.DocumentVersion.ToString();

            DocumentVersionFileDeletionResult result =
                new DocumentVersionFileDeletionResult();

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TimeSpan(0, 10, 0)))
            {
                DataTable documentRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            d.IdTaiLieu,
                            d.IdFileBanChinhThuc
                        FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                        WHERE d.IdTaiLieu = @DocumentId
                          AND d.IdDuAn IS NULL
                          AND d.DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty.");
                }

                Guid? officialFileId = GetNullableGuid(
                    documentRows.Rows[0],
                    "IdFileBanChinhThuc");
                if (officialFileId.HasValue
                    && requestedFileIds.Contains(officialFileId.Value))
                {
                    throw new InvalidOperationException(
                        "Không thể xóa phiên bản đang là file chính thức. Hãy bỏ chọn file chính thức trước.");
                }

                DataTable ownedFiles = ExecuteDataTable(
                    @"
                        SELECT
                            f.Id,
                            f.Name,
                            f.OriginalFileName,
                            f.FileUrl,
                            f.FileType,
                            f.Ext,
                            f.RefId,
                            f.RefType,
                            f.DisplayOrder,
                            f.FileSize,
                            f.MimeType,
                            f.OwnerId,
                            f.CreatedDate,
                            f.IsDeleted,
                            f.IsHost,
                            f.IsSecretary,
                            f.IsParticipant
                        FROM TblUploadFile f WITH (UPDLOCK, HOLDLOCK)
                        WHERE f.Id IN (" + fileIdSql + @")
                          AND f.RefId = @DocumentId
                          AND f.RefType = @RefType
                          AND f.IsDeleted = 0;",
                    parameters);
                if (ownedFiles.Rows.Count != requestedFileIds.Count)
                {
                    throw new InvalidOperationException(
                        "Danh sách tệp cần xóa không hợp lệ hoặc không thuộc hồ sơ này.");
                }

                foreach (DataRow row in ownedFiles.Rows)
                    result.DeletedFiles.Add(ToUploadFile(row));

                Dictionary<string, object> referenceParameters =
                    new Dictionary<string, object>(parameters);
                int externalReferenceCount = ExecuteScalarInt(
                    @"
                        SELECT COUNT(1)
                        FROM
                        (
                            SELECT s.IdTrinhKyTaiLieu AS ReferenceId
                            FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                            WHERE s.IdFileSauKy IN (" + fileIdSql + @")
                            UNION ALL
                            SELECT g.IdGuiNhanKhachHang AS ReferenceId
                            FROM TblGuiNhanKhachHang g WITH (UPDLOCK, HOLDLOCK)
                            WHERE g.IdFileNhanLai IN (" + fileIdSql + @")
                            UNION ALL
                            SELECT m.IdMauTaiLieu AS ReferenceId
                            FROM TblMauTaiLieu m WITH (UPDLOCK, HOLDLOCK)
                            WHERE m.IdFileMau IN (" + fileIdSql + @")
                            UNION ALL
                            SELECT d.IdTaiLieu AS ReferenceId
                            FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                            WHERE d.IdFileBanChinhThuc IN (" + fileIdSql + @")
                        ) referencesFound;",
                    referenceParameters);
                if (externalReferenceCount > 0)
                {
                    throw new InvalidOperationException(
                        "Không thể xóa tệp đang được file chính thức hoặc nghiệp vụ liên quan sử dụng.");
                }

                DataTable versionRows = ExecuteDataTable(
                    @"
                        SELECT
                            p.IdPhienBanTaiLieu,
                            p.IdTaiLieu,
                            p.SoPhienBan,
                            p.DaXoa,
                            p.LaPhienBanHienTai,
                            p.IdFileNoiDung,
                            u.Name AS FileName,
                            u.OriginalFileName,
                            u.FileUrl
                        FROM TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblUploadFile u
                            ON u.Id = p.IdFileNoiDung
                        WHERE p.IdTaiLieu = @DocumentId
                          AND p.IdFileNoiDung IN (" + fileIdSql + @")
                        ORDER BY p.NgayTao, p.IdPhienBanTaiLieu;",
                    parameters);
                foreach (DataRow row in versionRows.Rows)
                {
                    if (!GetBoolean(row, "DaXoa"))
                    {
                        result.DeletedVersions.Add(ToDocumentVersion(row));
                    }
                }

                int workflowVersionReferenceCount = ExecuteScalarInt(
                    @"
                        SELECT COUNT(1)
                        FROM
                        (
                            SELECT s.IdTrinhKyTaiLieu AS ReferenceId
                            FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                            INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                                ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                            WHERE p.IdTaiLieu = @DocumentId
                              AND p.IdFileNoiDung IN (" + fileIdSql + @")
                            UNION ALL
                            SELECT g.IdGuiNhanKhachHang AS ReferenceId
                            FROM TblGuiNhanKhachHang g WITH (UPDLOCK, HOLDLOCK)
                            INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                                ON p.IdPhienBanTaiLieu = g.IdPhienBanTaiLieu
                            WHERE p.IdTaiLieu = @DocumentId
                              AND p.IdFileNoiDung IN (" + fileIdSql + @")
                        ) workflowReferences;",
                    parameters);
                if (workflowVersionReferenceCount > 0)
                {
                    throw new InvalidOperationException(
                        "Không thể xóa phiên bản đã được sử dụng trong quá trình trình ký hoặc gửi khách hàng.");
                }

                ExecuteNonQuery(
                    @"
                        UPDATE p
                        SET p.IdFileNoiDung = NULL,
                            p.DaXoa = 1,
                            p.LaPhienBanHienTai = 0,
                            p.NguoiCapNhat = @CurrentUserName,
                            p.NgayCapNhat = @CurrentDate
                        FROM TblPhienBanTaiLieu p
                        WHERE p.IdTaiLieu = @DocumentId
                          AND p.IdFileNoiDung IN (" + fileIdSql + @");",
                    new Dictionary<string, object>(parameters)
                    {
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                object currentVersionValue = ExecuteScalarObject(
                    @"
                        SELECT TOP 1 p.IdPhienBanTaiLieu
                        FROM TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblUploadFile u
                            ON u.Id = p.IdFileNoiDung
                           AND u.IsDeleted = 0
                           AND u.RefId = @DocumentId
                           AND u.RefType = @RefType
                        WHERE p.IdTaiLieu = @DocumentId
                          AND p.DaXoa = 0
                          AND p.IdFileNoiDung IS NOT NULL
                        ORDER BY p.LaPhienBanHienTai DESC,
                                 p.NgayTao DESC,
                                 p.IdPhienBanTaiLieu DESC;",
                    parameters);
                Guid? currentVersionId = ToNullableGuid(currentVersionValue);

                Dictionary<string, object> currentParameters =
                    new Dictionary<string, object>(parameters)
                    {
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    };
                string currentVersionSql = "NULL";
                if (currentVersionId.HasValue)
                {
                    currentVersionSql = "@CurrentVersionId";
                    currentParameters["@CurrentVersionId"] =
                        currentVersionId.Value;
                }

                ExecuteNonQuery(
                    @"
                        UPDATE p
                        SET p.LaPhienBanHienTai = CASE
                                WHEN p.IdPhienBanTaiLieu = "
                        + currentVersionSql
                        + @" THEN 1
                                ELSE 0
                            END,
                            p.NguoiCapNhat = @CurrentUserName,
                            p.NgayCapNhat = @CurrentDate
                        FROM TblPhienBanTaiLieu p
                        WHERE p.IdTaiLieu = @DocumentId
                          AND p.DaXoa = 0;",
                    currentParameters);

                string actorIdSql = "NULL";
                foreach (TblPhienBanTaiLieu version in result.DeletedVersions)
                {
                    Dictionary<string, object> historyParameters =
                        new Dictionary<string, object>
                        {
                            { "@HistoryId", Guid.NewGuid() },
                            { "@DocumentId", idTaiLieu },
                            { "@ActionType", DocumentActivityTypeKeys.DeleteVersion },
                            { "@ReferenceType", DocumentActivityReferenceKeys.DocumentVersion },
                            { "@ReferenceId", version.IdPhienBanTaiLieu },
                            { "@Changes", BuildVersionDeletionDetails(version, ownedFiles) },
                            { "@Description", "Đã xóa một phiên bản tài liệu." },
                            { "@CurrentUserName", safeUserName },
                            { "@CurrentDate", currentDate }
                        };
                    if (currentUserId != Guid.Empty)
                    {
                        actorIdSql = "@ActorId";
                        historyParameters["@ActorId"] = currentUserId;
                    }

                    InsertDocumentVersionDeletionHistory(
                        historyParameters,
                        actorIdSql);
                }

                HashSet<Guid> versionFileIds = new HashSet<Guid>(
                    result.DeletedVersions
                        .Where(version => version.IdFileNoiDung.HasValue)
                        .Select(version => version.IdFileNoiDung.Value));
                foreach (TblUploadFile file in result.DeletedFiles
                    .Where(item => !versionFileIds.Contains(item.Id)))
                {
                    Dictionary<string, object> historyParameters =
                        new Dictionary<string, object>
                        {
                            { "@HistoryId", Guid.NewGuid() },
                            { "@DocumentId", idTaiLieu },
                            { "@ActionType", DocumentActivityTypeKeys.DeleteVersion },
                            { "@ReferenceType", DocumentActivityReferenceKeys.DocumentVersion },
                            { "@Changes", "Tệp không gắn phiên bản: "
                                + GetFileDisplayName(file)
                                + "; Id tệp: " + file.Id },
                            { "@Description", "Đã xóa một tệp phiên bản tài liệu." },
                            { "@CurrentUserName", safeUserName },
                            { "@CurrentDate", currentDate }
                        };
                    if (currentUserId != Guid.Empty)
                    {
                        actorIdSql = "@ActorId";
                        historyParameters["@ActorId"] = currentUserId;
                    }

                    InsertDocumentVersionDeletionHistory(
                        historyParameters,
                        actorIdSql);
                }

                int deletedCount = ExecuteNonQuery(
                    @"
                        DELETE FROM TblUploadFile
                        WHERE Id IN (" + fileIdSql + @")
                          AND RefId = @DocumentId
                          AND RefType = @RefType
                          AND IsDeleted = 0;",
                    parameters);
                if (deletedCount != requestedFileIds.Count)
                {
                    throw new InvalidOperationException(
                        "Không thể xóa đầy đủ các tệp phiên bản; dữ liệu đã được hoàn tác.");
                }

                scope.Complete();
            }

            return result;
        }

        public bool HasUploadFileReferenceByPath(
            string fileUrl,
            IEnumerable<Guid> excludedFileIds)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            List<Guid> excludedIds = (excludedFileIds
                    ?? Enumerable.Empty<Guid>())
                .Where(fileId => fileId != Guid.Empty)
                .Distinct()
                .ToList();
            Dictionary<string, object> parameters =
                new Dictionary<string, object>
                {
                    { "@FileUrl", fileUrl }
                };
            string exclusionSql = string.Empty;
            if (excludedIds.Count > 0)
            {
                Dictionary<string, object> exclusionParameters;
                exclusionSql = " AND f.Id NOT IN ("
                    + BuildGuidParameterList(
                        excludedIds,
                        "ExcludedFileId",
                        out exclusionParameters)
                    + ")";
                foreach (KeyValuePair<string, object> pair
                    in exclusionParameters)
                {
                    parameters[pair.Key] = pair.Value;
                }
            }

            return ExecuteScalarInt(
                @"
                    SELECT CASE WHEN EXISTS
                    (
                        SELECT 1
                        FROM TblUploadFile f
                        CROSS APPLY
                        (
                            SELECT REPLACE(
                                       REPLACE(
                                           REPLACE(LTRIM(RTRIM(f.FileUrl)),
                                                   N'~/', N'/'),
                                           N'\', N'/'),
                                       N'//', N'/') AS NormalizedFileUrl
                        ) normalized
                        WHERE CASE
                                  WHEN LEFT(normalized.NormalizedFileUrl, 1) = N'/'
                                  THEN normalized.NormalizedFileUrl
                                  ELSE N'/' + normalized.NormalizedFileUrl
                              END = @FileUrl"
                    + exclusionSql
                    + @"
                    ) THEN 1 ELSE 0 END;",
                parameters) > 0;
        }

        public TblPhienBanTaiLieu GetDocumentVersionById(
            Guid idTaiLieu,
            Guid idPhienBanTaiLieu)
        {
            if (idTaiLieu == Guid.Empty
                || idPhienBanTaiLieu == Guid.Empty)
            {
                return null;
            }

            return new Select()
                .From(TblPhienBanTaiLieu.Schema)
                .Where(TblPhienBanTaiLieu.IdTaiLieuColumn)
                .IsEqualTo(idTaiLieu)
                .And(TblPhienBanTaiLieu.IdPhienBanTaiLieuColumn)
                .IsEqualTo(idPhienBanTaiLieu)
                .And(TblPhienBanTaiLieu.DaXoaColumn)
                .IsEqualTo(false)
                .ExecuteSingle<TblPhienBanTaiLieu>();
        }

        public TblUploadFile GetDocumentVersionFileById(
            Guid idTaiLieu,
            Guid idFile)
        {
            if (idTaiLieu == Guid.Empty || idFile == Guid.Empty)
                return null;

            return new Select()
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.IdColumn).IsEqualTo(idFile)
                .And(TblUploadFile.RefIdColumn).IsEqualTo(idTaiLieu)
                .And(TblUploadFile.RefTypeColumn)
                .IsEqualTo(FileUploadTypes.DocumentVersion.ToString())
                .And(TblUploadFile.IsDeletedColumn).IsEqualTo(false)
                .ExecuteSingle<TblUploadFile>();
        }

        public bool HasActiveWorkflowForVersion(
            Guid idPhienBanTaiLieu)
        {
            if (idPhienBanTaiLieu == Guid.Empty)
                return false;

            bool hasSigning = new Select()
                .From(TblTrinhKyTaiLieu.Schema)
                .Where(TblTrinhKyTaiLieu.IdPhienBanTaiLieuColumn)
                .IsEqualTo(idPhienBanTaiLieu)
                .And(TblTrinhKyTaiLieu.DaXoaColumn)
                .IsEqualTo(false)
                .GetRecordCount() > 0;

            if (hasSigning)
                return true;

            return new Select()
                .From(TblGuiNhanKhachHang.Schema)
                .Where(TblGuiNhanKhachHang.IdPhienBanTaiLieuColumn)
                .IsEqualTo(idPhienBanTaiLieu)
                .And(TblGuiNhanKhachHang.DaXoaColumn)
                .IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public DataTable GetDocumentVersionsWithFiles(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT
                    p.IdPhienBanTaiLieu,
                    p.SoPhienBan,
                    p.NguonTao,
                    p.MoTaPhienBan,
                    p.LaPhienBanHienTai,
                    p.NguoiTao,
                    p.NgayTao,
                    u.Id AS IdFile,
                    u.Name AS TenFile,
                    u.OriginalFileName AS TenFileGoc,
                    u.FileUrl,
                    u.Ext,
                    u.FileSize,
                    ISNULL(NULLIF(creator.DisplayName, N''), p.NguoiTao)
                        AS TenNguoiTao
                FROM TblPhienBanTaiLieu p
                INNER JOIN TblUploadFile u
                    ON u.Id = p.IdFileNoiDung
                   AND u.IsDeleted = 0
                OUTER APPLY
                (
                    SELECT
                        CASE
                            WHEN COUNT(1) = 1
                            THEN MAX(NULLIF(account.DisplayName, N''))
                            ELSE NULL
                        END AS DisplayName
                    FROM aspnet_Users account
                    WHERE account.UserName = p.NguoiTao
                      AND account.IsDeleted = 0
                ) creator
                WHERE p.IdTaiLieu = '{idTaiLieu}'
                  AND p.DaXoa = 0
                ORDER BY
                    p.LaPhienBanHienTai DESC,
                    p.NgayTao DESC,
                    p.IdPhienBanTaiLieu DESC;";

            return ExecuteDataTable(sql);
        }

        public DataTable GetCompanyDocumentDetail(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT TOP 1
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
                    ISNULL(nv.DisplayName, N'') AS TenNhanVienPhuTrach,
                    ISNULL(u.Name, N'') AS TenFileChinhThuc,
                    ISNULL(u.OriginalFileName, N'') AS TenFileChinhThucGoc,
                    ISNULL(u.FileUrl, N'') AS FileChinhThucUrl,
                    u.FileSize AS DungLuongFileChinhThuc,
                    ISNULL(u.Ext, N'') AS PhanMoRongFileChinhThuc
                FROM TblTaiLieu t
                LEFT JOIN TblLoaiTaiLieu l
                    ON l.IdLoaiTaiLieu = t.IdLoaiTaiLieu
                   AND l.DaXoa = 0
                LEFT JOIN TblNhomTaiLieu n
                    ON n.IdNhomTaiLieu = l.IdNhomTaiLieu
                   AND n.DaXoa = 0
                LEFT JOIN aspnet_Users nv
                    ON nv.UserId = t.IdNhanVienPhuTrach
                   AND nv.IsDeleted = 0
                LEFT JOIN TblUploadFile u
                    ON u.Id = t.IdFileBanChinhThuc
                   AND u.IsDeleted = 0
                WHERE t.IdTaiLieu = '{idTaiLieu}'
                  AND t.IdDuAn IS NULL
                  AND t.DaXoa = 0;";

            return ExecuteDataTable(sql);
        }

        public DataTable GetSigningHistory(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = @"
                SELECT
                    s.IdTrinhKyTaiLieu,
                    s.IdPhienBanTaiLieu,
                    s.IdNguoiGui,
                    s.IdNguoiKy,
                    s.TenNguoiKy,
                    p.SoPhienBan,
                    s.HinhThucKy,
                    s.TrangThaiTrinhKy,
                    s.NgayGui,
                    s.NgayNhanLai,
                    s.GhiChu,
                    ISNULL(sender.DisplayName, N'') AS TenNguoiGui,
                    COALESCE(NULLIF(s.TenNguoiKy, N''),
                             NULLIF(signer.DisplayName, N''), N'')
                        AS TenNguoiKyHienThi,
                    f.Id AS IdFileSauKy,
                    ISNULL(f.Name, N'') AS TenFileSauKy,
                    ISNULL(f.OriginalFileName, N'') AS TenFileSauKyGoc,
                    ISNULL(f.FileUrl, N'') AS FileSauKyUrl
                FROM TblTrinhKyTaiLieu s
                INNER JOIN TblPhienBanTaiLieu p
                    ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                   AND p.DaXoa = 0
                INNER JOIN TblTaiLieu d
                    ON d.IdTaiLieu = p.IdTaiLieu
                   AND d.IdDuAn IS NULL
                   AND d.DaXoa = 0
                LEFT JOIN aspnet_Users sender
                    ON sender.UserId = s.IdNguoiGui
                   AND sender.IsDeleted = 0
                LEFT JOIN aspnet_Users signer
                    ON signer.UserId = s.IdNguoiKy
                   AND signer.IsDeleted = 0
                LEFT JOIN TblUploadFile f
                    ON f.Id = s.IdFileSauKy
                   AND f.IsDeleted = 0
                WHERE p.IdTaiLieu = @DocumentId
                  AND s.DaXoa = 0
                ORDER BY s.NgayGui DESC, s.IdTrinhKyTaiLieu DESC;";

            return ExecuteDataTable(
                sql,
                new Dictionary<string, object>
                {
                    { "@DocumentId", idTaiLieu }
                });
        }

        public DataTable GetSigningDetail(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu)
        {
            if (idTaiLieu == Guid.Empty || idTrinhKyTaiLieu == Guid.Empty)
                return new DataTable();

            const string sql = @"
                SELECT TOP 1
                    s.IdTrinhKyTaiLieu,
                    s.IdPhienBanTaiLieu,
                    s.IdNguoiGui,
                    s.IdNguoiKy,
                    s.TenNguoiKy,
                    s.HinhThucKy,
                    s.TrangThaiTrinhKy,
                    s.NgayGui,
                    s.NgayNhanLai,
                    s.GhiChu,
                    p.SoPhienBan,
                    f.Id AS IdFileSauKy,
                    f.FileUrl AS FileSauKyUrl,
                    f.Name AS TenFileSauKy,
                    f.OriginalFileName AS TenFileSauKyGoc
                FROM TblTrinhKyTaiLieu s
                INNER JOIN TblPhienBanTaiLieu p
                    ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                   AND p.DaXoa = 0
                INNER JOIN TblTaiLieu d
                    ON d.IdTaiLieu = p.IdTaiLieu
                   AND d.IdDuAn IS NULL
                   AND d.DaXoa = 0
                LEFT JOIN TblUploadFile f
                    ON f.Id = s.IdFileSauKy
                   AND f.IsDeleted = 0
                WHERE d.IdTaiLieu = @DocumentId
                  AND s.IdTrinhKyTaiLieu = @SigningId
                  AND s.DaXoa = 0;";

            return ExecuteDataTable(
                sql,
                new Dictionary<string, object>
                {
                    { "@DocumentId", idTaiLieu },
                    { "@SigningId", idTrinhKyTaiLieu }
                });
        }

        public DocumentSigningOperationResult SubmitDocumentSigning(
            Guid idTaiLieu,
            Guid idNguoiKy,
            string tenNguoiKy,
            string hinhThucKy,
            string ghiChu,
            Guid currentUserId,
            string currentUserName,
            DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty)
                throw new InvalidOperationException(
                    "Không xác định được hồ sơ cần trình ký.");
            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException(
                    "Tài khoản hiện tại không hợp lệ để trình ký.");

            string safeMethod = (hinhThucKy ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (safeMethod != DocumentSigningMethodKeys.Paper
                && safeMethod != DocumentSigningMethodKeys.DigitalExternal)
            {
                throw new InvalidOperationException(
                    "Hình thức ký của hồ sơ không hợp lệ.");
            }

            string safeSignerName = (tenNguoiKy ?? string.Empty).Trim();
            if (safeSignerName.Length > 150)
                throw new InvalidOperationException(
                    "Tên người ký không được vượt quá 150 ký tự.");

            string safeNote = (ghiChu ?? string.Empty).Trim();
            if (safeNote.Length > 500)
                throw new InvalidOperationException(
                    "Ghi chú không được vượt quá 500 ký tự.");

            string safeUserName = NormalizeUserName(currentUserName);
            Guid signingId = Guid.NewGuid();
            DocumentSigningOperationResult result =
                new DocumentSigningOperationResult
                {
                    IdTrinhKyTaiLieu = signingId
                };

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TimeSpan(0, 10, 0)))
            {
                DataTable documentRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            d.IdTaiLieu,
                            d.CanTrinhKy,
                            d.HinhThucKy,
                            d.TrangThaiTaiLieu
                        FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                        WHERE d.IdTaiLieu = @DocumentId
                          AND d.IdDuAn IS NULL
                          AND d.DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty.");

                DataRow document = documentRows.Rows[0];
                if (!GetBoolean(document, "CanTrinhKy"))
                    throw new InvalidOperationException(
                        "Hồ sơ này không được cấu hình trình ký.");

                string configuredMethod = Convert.ToString(
                    document["HinhThucKy"]);
                if (!string.Equals(
                        configuredMethod,
                        safeMethod,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Hình thức ký đã thay đổi. Vui lòng tải lại hồ sơ.");
                }

                DataTable senderRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1 u.UserId
                        FROM aspnet_Users u WITH (UPDLOCK, HOLDLOCK)
                        WHERE u.UserId = @SenderId
                          AND u.IsDeleted = 0
                          AND u.IsActivated = 1;",
                    new Dictionary<string, object>
                    {
                        { "@SenderId", currentUserId }
                    });
                if (senderRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Tài khoản hiện tại không còn hoạt động.");

                if (idNguoiKy != Guid.Empty)
                {
                    DataTable signerRows = ExecuteDataTable(
                        @"
                            SELECT TOP 1
                                u.UserId,
                                u.DisplayName,
                                u.UserName
                            FROM aspnet_Users u WITH (UPDLOCK, HOLDLOCK)
                            WHERE u.UserId = @SignerId
                              AND u.IsDeleted = 0
                              AND u.IsActivated = 1
                              AND u.LaNhanVien = 1;",
                        new Dictionary<string, object>
                        {
                            { "@SignerId", idNguoiKy }
                        });
                    if (signerRows.Rows.Count == 0)
                        throw new InvalidOperationException(
                            "Người ký không tồn tại hoặc đã bị khóa.");

                    string signerDisplayName = Convert.ToString(
                        signerRows.Rows[0]["DisplayName"]);
                    if (string.IsNullOrWhiteSpace(signerDisplayName))
                    {
                        signerDisplayName = Convert.ToString(
                            signerRows.Rows[0]["UserName"]);
                    }
                    if (string.IsNullOrWhiteSpace(signerDisplayName))
                    {
                        signerDisplayName = idNguoiKy.ToString();
                    }

                    safeSignerName = signerDisplayName.Trim();
                    if (safeSignerName.Length > 150)
                    {
                        safeSignerName = safeSignerName.Substring(0, 150);
                    }
                }
                else if (string.IsNullOrWhiteSpace(safeSignerName))
                {
                    throw new InvalidOperationException(
                        "Vui lòng chọn người ký.");
                }

                DataTable pendingRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1 s.IdTrinhKyTaiLieu
                        FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                            ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                           AND p.DaXoa = 0
                        WHERE p.IdTaiLieu = @DocumentId
                          AND s.DaXoa = 0
                          AND s.TrangThaiTrinhKy IN
                              (@PendingStatus, @LegacyPendingStatus);",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@PendingStatus", DocumentSigningStatusKeys.Pending },
                        { "@LegacyPendingStatus", DocumentStatusKeys.PendingSignature }
                    });
                if (pendingRows.Rows.Count > 0)
                    throw new InvalidOperationException(
                        "Hồ sơ đang có một lần trình ký chờ ký.");

                DataTable currentVersionRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            p.IdPhienBanTaiLieu,
                            p.SoPhienBan,
                            u.Id AS IdFile,
                            u.FileUrl,
                            u.OriginalFileName,
                            u.Name AS FileName
                        FROM TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblUploadFile u WITH (UPDLOCK, HOLDLOCK)
                            ON u.Id = p.IdFileNoiDung
                           AND u.RefId = @DocumentId
                           AND u.RefType = @VersionRefType
                           AND u.IsDeleted = 0
                        WHERE p.IdTaiLieu = @DocumentId
                          AND p.DaXoa = 0
                          AND p.LaPhienBanHienTai = 1
                          AND p.IdFileNoiDung IS NOT NULL
                        ORDER BY p.NgayTao DESC, p.IdPhienBanTaiLieu DESC;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@VersionRefType", FileUploadTypes.DocumentVersion.ToString() }
                    });
                if (currentVersionRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Hồ sơ chưa có phiên bản hiện tại và tệp nội dung hợp lệ.");

                DataRow currentVersion = currentVersionRows.Rows[0];
                Guid versionId = GetGuid(
                    currentVersion,
                    "IdPhienBanTaiLieu");
                Guid fileId = GetGuid(currentVersion, "IdFile");
                if (versionId == Guid.Empty || fileId == Guid.Empty)
                    throw new InvalidOperationException(
                        "Phiên bản hiện tại không có tệp nội dung hợp lệ.");
                if (!IsFileAvailable(currentVersion["FileUrl"]))
                    throw new InvalidOperationException(
                        "Không tìm thấy tệp vật lý của phiên bản hiện tại.");

                DataTable previousAttemptRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            s.IdTrinhKyTaiLieu,
                            s.TrangThaiTrinhKy
                        FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                        WHERE s.IdPhienBanTaiLieu = @VersionId
                          AND s.DaXoa = 0
                          AND s.TrangThaiTrinhKy IN
                              (@ChangesStatus, @SignedStatus)
                        ORDER BY
                            CASE
                                WHEN s.TrangThaiTrinhKy = @SignedStatus
                                THEN 0 ELSE 1
                            END,
                            s.NgayGui DESC,
                            s.IdTrinhKyTaiLieu DESC;",
                    new Dictionary<string, object>
                    {
                        { "@VersionId", versionId },
                        { "@ChangesStatus", DocumentSigningStatusKeys.ChangesRequested },
                        { "@SignedStatus", DocumentSigningStatusKeys.Signed }
                    });
                if (previousAttemptRows.Rows.Count > 0)
                {
                    string previousStatus = Convert.ToString(
                        previousAttemptRows.Rows[0]["TrangThaiTrinhKy"]);
                    if (string.Equals(
                            previousStatus,
                            DocumentSigningStatusKeys.Signed,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Phiên bản hiện tại đã được ký; không thể trình ký lại.");
                    }

                    throw new InvalidOperationException(
                        "Phiên bản hiện tại đã bị yêu cầu điều chỉnh; vui lòng tải lên phiên bản mới trước khi trình ký lại.");
                }

                ExecuteNonQuery(
                    @"
                        INSERT INTO TblTrinhKyTaiLieu
                        (
                            IdTrinhKyTaiLieu,
                            IdPhienBanTaiLieu,
                            IdNguoiGui,
                            IdNguoiKy,
                            TenNguoiKy,
                            HinhThucKy,
                            TrangThaiTrinhKy,
                            NgayGui,
                            NgayNhanLai,
                            GhiChu,
                            DaXoa,
                            NguoiTao,
                            NgayTao,
                            NguoiCapNhat,
                            NgayCapNhat,
                            IdFileSauKy
                        )
                        VALUES
                        (
                            @SigningId,
                            @VersionId,
                            @SenderId,
                            @SignerId,
                            NULLIF(@SignerName, ''),
                            @SigningMethod,
                            @PendingStatus,
                            @CurrentDate,
                            NULL,
                            NULLIF(@Note, ''),
                            0,
                            @CurrentUserName,
                            @CurrentDate,
                            NULL,
                            NULL,
                            NULL
                        );",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", signingId },
                        { "@VersionId", versionId },
                        { "@SenderId", currentUserId },
                        { "@SignerId", idNguoiKy == Guid.Empty ? (object)DBNull.Value : idNguoiKy },
                        { "@SignerName", safeSignerName },
                        { "@SigningMethod", safeMethod },
                        { "@PendingStatus", DocumentSigningStatusKeys.Pending },
                        { "@Note", safeNote },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                int updatedDocument = ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiTaiLieu = @DocumentStatus,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND IdDuAn IS NULL
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@DocumentStatus", DocumentStatusKeys.PendingSignature },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });
                if (updatedDocument != 1)
                    throw new InvalidOperationException(
                        "Không thể cập nhật trạng thái hồ sơ trình ký.");

                InsertSigningHistory(
                    idTaiLieu,
                    DocumentActivityTypeKeys.SubmitSigning,
                    signingId,
                    "Phiên bản: v"
                        + Convert.ToString(currentVersion["SoPhienBan"])
                        + "; Người ký: "
                        + (string.IsNullOrWhiteSpace(safeSignerName)
                            ? idNguoiKy.ToString()
                            : safeSignerName)
                        + "; Hình thức ký: " + safeMethod,
                    "Đã trình ký hồ sơ.",
                    currentUserId,
                    safeUserName,
                    currentDate);

                scope.Complete();
                result.IdPhienBanTaiLieu = versionId;
                result.IdFile = fileId;
            }

            return result;
        }

        public void RequestDocumentSigningChanges(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu,
            string reason,
            Guid currentUserId,
            string currentUserName,
            DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty || idTrinhKyTaiLieu == Guid.Empty)
                throw new InvalidOperationException(
                    "Không xác định được lần trình ký.");
            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException(
                    "Tài khoản hiện tại không hợp lệ.");

            string safeReason = (reason ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(safeReason))
                throw new InvalidOperationException(
                    "Lý do yêu cầu điều chỉnh không được để trống.");
            if (safeReason.Length > 500)
                throw new InvalidOperationException(
                    "Lý do yêu cầu điều chỉnh không được vượt quá 500 ký tự.");

            string safeUserName = NormalizeUserName(currentUserName);
            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TimeSpan(0, 10, 0)))
            {
                DataTable documentRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1 d.IdTaiLieu
                        FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                        WHERE d.IdTaiLieu = @DocumentId
                          AND d.IdDuAn IS NULL
                          AND d.DaXoa = 0
                          AND d.CanTrinhKy = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty cần trình ký.");

                EnsureActiveUser(currentUserId);

                DataTable signingRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            s.IdTrinhKyTaiLieu,
                            p.SoPhienBan
                        FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                            ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                           AND p.DaXoa = 0
                        WHERE s.IdTrinhKyTaiLieu = @SigningId
                          AND s.DaXoa = 0
                          AND s.TrangThaiTrinhKy = @PendingStatus
                          AND p.IdTaiLieu = @DocumentId;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@DocumentId", idTaiLieu },
                        { "@PendingStatus", DocumentSigningStatusKeys.Pending }
                    });
                if (signingRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Lần trình ký không tồn tại, đã xử lý hoặc không thuộc hồ sơ này.");

                DataTable resultFiles = ExecuteDataTable(
                    @"
                        SELECT TOP 1 f.Id
                        FROM TblUploadFile f WITH (UPDLOCK, HOLDLOCK)
                        WHERE f.RefId = @SigningId
                          AND f.RefType = @ResultRefType
                          AND f.IsDeleted = 0;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@ResultRefType", FileUploadTypes.DocumentSigningResult.ToString() }
                    });
                if (resultFiles.Rows.Count > 0)
                    throw new InvalidOperationException(
                        "Lần trình ký đã có tệp kết quả ký; vui lòng xóa tệp đó trước khi yêu cầu điều chỉnh.");

                ExecuteNonQuery(
                    @"
                        UPDATE TblTrinhKyTaiLieu
                        SET TrangThaiTrinhKy = @ChangesStatus,
                            NgayNhanLai = @CurrentDate,
                            GhiChu = @Reason,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTrinhKyTaiLieu = @SigningId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@ChangesStatus", DocumentSigningStatusKeys.ChangesRequested },
                        { "@Reason", safeReason },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiTaiLieu = @DocumentStatus,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND IdDuAn IS NULL
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@DocumentStatus", DocumentStatusKeys.ChangesRequested },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                InsertSigningHistory(
                    idTaiLieu,
                    DocumentActivityTypeKeys.RequestSigningChanges,
                    idTrinhKyTaiLieu,
                    "Phiên bản: v"
                        + Convert.ToString(signingRows.Rows[0]["SoPhienBan"])
                        + "; Lý do: " + safeReason,
                    "Đã yêu cầu điều chỉnh hồ sơ trình ký.",
                    currentUserId,
                    safeUserName,
                    currentDate);

                scope.Complete();
            }
        }

        public DocumentSigningOperationResult CompleteDocumentSigning(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu,
            string note,
            Guid currentUserId,
            string currentUserName,
            DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty || idTrinhKyTaiLieu == Guid.Empty)
                throw new InvalidOperationException(
                    "Không xác định được lần trình ký.");
            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException(
                    "Tài khoản hiện tại không hợp lệ.");

            string safeNote = (note ?? string.Empty).Trim();
            if (safeNote.Length > 500)
                throw new InvalidOperationException(
                    "Ghi chú không được vượt quá 500 ký tự.");

            string safeUserName = NormalizeUserName(currentUserName);
            DocumentSigningOperationResult result =
                new DocumentSigningOperationResult
                {
                    IdTrinhKyTaiLieu = idTrinhKyTaiLieu
                };

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TimeSpan(0, 10, 0)))
            {
                DataTable documentRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1 d.IdTaiLieu
                        FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                        WHERE d.IdTaiLieu = @DocumentId
                          AND d.IdDuAn IS NULL
                          AND d.DaXoa = 0
                          AND d.CanTrinhKy = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty cần trình ký.");

                EnsureActiveUser(currentUserId);

                DataTable signingRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            s.IdTrinhKyTaiLieu,
                            s.IdPhienBanTaiLieu,
                            p.SoPhienBan
                        FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                            ON p.IdPhienBanTaiLieu = s.IdPhienBanTaiLieu
                           AND p.DaXoa = 0
                        WHERE s.IdTrinhKyTaiLieu = @SigningId
                          AND s.DaXoa = 0
                          AND s.TrangThaiTrinhKy = @PendingStatus
                          AND p.IdTaiLieu = @DocumentId;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@DocumentId", idTaiLieu },
                        { "@PendingStatus", DocumentSigningStatusKeys.Pending }
                    });
                if (signingRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Lần trình ký không tồn tại, đã xử lý hoặc không thuộc hồ sơ này.");

                DataTable resultFiles = ExecuteDataTable(
                    @"
                        SELECT
                            f.Id,
                            f.FileUrl,
                            f.Ext,
                            f.Name,
                            f.OriginalFileName
                        FROM TblUploadFile f WITH (UPDLOCK, HOLDLOCK)
                        WHERE f.RefId = @SigningId
                          AND f.RefType = @ResultRefType
                          AND f.IsDeleted = 0
                        ORDER BY f.CreatedDate, f.Id;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@ResultRefType", FileUploadTypes.DocumentSigningResult.ToString() }
                    });
                if (resultFiles.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Chưa có tệp kết quả ký. Vui lòng tải lên và lưu tệp trước khi xác nhận.");
                if (resultFiles.Rows.Count != 1)
                    throw new InvalidOperationException(
                        "Lần trình ký phải có đúng một tệp kết quả ký đang hoạt động.");

                DataRow resultFile = resultFiles.Rows[0];
                Guid resultFileId = GetGuid(resultFile, "Id");
                if (!IsAllowedSigningResultExtension(resultFile))
                    throw new InvalidOperationException(
                        "Tệp kết quả ký chỉ được phép có định dạng PDF, JPG, JPEG hoặc PNG.");
                if (resultFileId == Guid.Empty
                    || !IsFileAvailable(resultFile["FileUrl"]))
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy tệp vật lý của kết quả ký.");
                }

                ExecuteNonQuery(
                    @"
                        UPDATE TblTrinhKyTaiLieu
                        SET TrangThaiTrinhKy = @SignedStatus,
                            NgayNhanLai = @CurrentDate,
                            GhiChu = CASE
                                WHEN NULLIF(@Note, '') IS NULL THEN GhiChu
                                ELSE @Note
                            END,
                            IdFileSauKy = @ResultFileId,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTrinhKyTaiLieu = @SigningId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@SigningId", idTrinhKyTaiLieu },
                        { "@SignedStatus", DocumentSigningStatusKeys.Signed },
                        { "@Note", safeNote },
                        { "@ResultFileId", resultFileId },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiTaiLieu = @DocumentStatus,
                            IdFileBanChinhThuc = @ResultFileId,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND IdDuAn IS NULL
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@DocumentStatus", DocumentStatusKeys.Signed },
                        { "@ResultFileId", resultFileId },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                InsertSigningHistory(
                    idTaiLieu,
                    DocumentActivityTypeKeys.CompleteSigning,
                    idTrinhKyTaiLieu,
                    "Phiên bản: v"
                        + Convert.ToString(signingRows.Rows[0]["SoPhienBan"])
                        + "; Tệp sau ký: "
                        + GetFileDisplayName(resultFile),
                    "Đã hoàn tất ký hồ sơ.",
                    currentUserId,
                    safeUserName,
                    currentDate);

                scope.Complete();
                result.IdPhienBanTaiLieu = GetGuid(
                    signingRows.Rows[0],
                    "IdPhienBanTaiLieu");
                result.IdFile = resultFileId;
            }

            return result;
        }

        public DataTable GetCustomerDeliveryHistory(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT
                    g.IdGuiNhanKhachHang,
                    p.SoPhienBan,
                    ISNULL(k.TenKhachHang, N'') AS TenKhachHang,
                    ISNULL(actor.DisplayName, N'') AS TenNguoiThucHien,
                    g.TenNguoiNhan,
                    g.EmailNguoiNhan,
                    g.NgayGui,
                    g.HanPhanHoi,
                    g.NgayNhanLai,
                    g.KenhGui,
                    g.TrangThai,
                    g.LaBanChinhThuc,
                    g.GhiChu,
                    f.Id AS IdFileNhanLai,
                    ISNULL(f.Name, N'') AS TenFileNhanLai,
                    ISNULL(f.OriginalFileName, N'') AS TenFileNhanLaiGoc,
                    ISNULL(f.FileUrl, N'') AS FileNhanLaiUrl
                FROM TblGuiNhanKhachHang g
                INNER JOIN TblPhienBanTaiLieu p
                    ON p.IdPhienBanTaiLieu = g.IdPhienBanTaiLieu
                   AND p.DaXoa = 0
                LEFT JOIN TblKhachHang k
                    ON k.IdKhachHang = g.IdKhachHang
                   AND k.DaXoa = 0
                LEFT JOIN aspnet_Users actor
                    ON actor.UserId = g.IdNguoiThucHien
                   AND actor.IsDeleted = 0
                LEFT JOIN TblUploadFile f
                    ON f.Id = g.IdFileNhanLai
                   AND f.IsDeleted = 0
                WHERE p.IdTaiLieu = '{idTaiLieu}'
                  AND g.DaXoa = 0
                ORDER BY g.NgayGui DESC, g.IdGuiNhanKhachHang DESC;";

            return ExecuteDataTable(sql);
        }

        public DataTable GetPhysicalStorageHistory(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT
                    v.IdLuuTruVatLy,
                    v.MaLuuTru,
                    v.TrangThaiLuuTru,
                    v.TinhTrangBanGoc,
                    v.NgayLuu,
                    v.NgayLayRa,
                    v.NgayHoanTra,
                    v.LaViTriHienTai,
                    v.GhiChu,
                    ISNULL(n.MaNoiLuuTru, N'') AS MaNoiLuuTru,
                    ISNULL(n.TenNoiLuuTru, N'') AS TenNoiLuuTru,
                    ISNULL(actor.DisplayName, N'') AS TenNguoiThucHien
                FROM TblLuuTruVatLy v
                LEFT JOIN TblNoiLuuTru n
                    ON n.IdNoiLuuTru = v.IdNoiLuuTru
                   AND n.DaXoa = 0
                LEFT JOIN aspnet_Users actor
                    ON actor.UserId = v.IdNguoiThucHien
                   AND actor.IsDeleted = 0
                WHERE v.IdTaiLieu = '{idTaiLieu}'
                  AND v.DaXoa = 0
                ORDER BY
                    v.LaViTriHienTai DESC,
                    v.NgayLuu DESC,
                    v.IdLuuTruVatLy DESC;";

            return ExecuteDataTable(sql);
        }

        public DataTable GetDocumentActivityHistory(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string sql = $@"
                SELECT
                    h.IdLichSuTaiLieu,
                    h.LoaiHanhDong,
                    h.LoaiThamChieu,
                    h.NoiDungThayDoi,
                    h.MoTa,
                    h.NguoiTao,
                    h.NgayTao,
                    ISNULL(actor.DisplayName, N'') AS TenNguoiThucHien
                FROM TblLichSuTaiLieu h
                LEFT JOIN aspnet_Users actor
                    ON actor.UserId = h.IdNhanVienThucHien
                   AND actor.IsDeleted = 0
                WHERE h.IdTaiLieu = '{idTaiLieu}'
                ORDER BY h.NgayTao DESC, h.IdLichSuTaiLieu DESC;";

            return ExecuteDataTable(sql);
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

        public void InsertDocumentHistory(TblLichSuTaiLieu item)
        {
            if (item != null)
                item.Save();
        }

        // Stage 2 (simplified): opt-in helpers using the existing audit API.
        // Stage 3 will replace the legacy history call sites; do not call both for one event.
        public Task WriteDocumentAuditAsync(
            Guid documentId,
            string activityType,
            string referenceType,
            Guid? referenceId,
            string changes,
            string description,
            ClientInfo actor)
        {
            if (documentId == Guid.Empty || string.IsNullOrWhiteSpace(activityType))
                throw new ArgumentException("Hồ sơ và loại hoạt động không được để trống.");
            if (actor == null || !actor.UserId.HasValue || actor.UserId.Value == Guid.Empty
                || string.IsNullOrWhiteSpace(actor.UserName))
                throw new ArgumentException("Cần thông tin người thực hiện từ request hiện tại.", nameof(actor));

            // Capture this operation's actor, not the AuditManager cached by a singleton.
            var currentActor = new ClientInfo
            {
                UserId = actor.UserId,
                UserName = actor.UserName,
                IpAddress = actor.IpAddress,
                UserAgent = actor.UserAgent
            };
            var action = activityType == DocumentActivityTypeKeys.CreateDocument
                ? LogActions.Actions.CREATE
                : activityType == DocumentActivityTypeKeys.DeleteDocument
                    ? LogActions.Actions.DELETE
                    : LogActions.Actions.UPDATE;

            // Flat properties are required by the legacy reflection-based serializer.
            // Encode text because the shared audit screen renders Changes as HTML.
            // A structured document view must HtmlDecode once, then render as encoded text.
            var entry = new
            {
                RefId = documentId,
                LoaiHanhDong = System.Web.HttpUtility.HtmlEncode(activityType),
                LoaiThamChieu = System.Web.HttpUtility.HtmlEncode(referenceType ?? string.Empty),
                IdThamChieu = referenceId,
                NoiDungThayDoi = System.Web.HttpUtility.HtmlEncode(changes ?? string.Empty),
                MoTa = System.Web.HttpUtility.HtmlEncode(description ?? string.Empty)
            };
            return new AuditManager(currentActor).LogActionAsync(
                action, entry, nameof(TblTaiLieu), documentId, currentActor.UserName);
        }

        // Caller must authorize access to documentId first and pass its creation date in UTC.
        // A document-local query avoids the legacy GetAuditTrailAsync parameter-binding bug.
        // This returns a list, not server-side pagination across yearly tables.
        public Task<List<AuditLogDto>> GetDocumentAuditHistoryAsync(
            Guid documentId,
            DateTime fromUtc,
            DateTime? toUtc = null)
        {
            if (documentId == Guid.Empty)
                throw new ArgumentException("Hồ sơ không hợp lệ.", nameof(documentId));
            DateTime endUtc = toUtc ?? DateTime.UtcNow;
            if (fromUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc
                || fromUtc.Year < 1753 || fromUtc > endUtc)
                throw new ArgumentException("Khoảng thời gian nhật ký phải hợp lệ và dùng UTC.");

            var tables = new List<string>();
            string provider = SubsonicHelpers.SysProvider.Name;
            using (IDataReader reader = DataService.GetReader(new QueryCommand(
                "SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND name LIKE 'TblAuditLog[_]____'",
                provider)))
            {
                while (reader.Read())
                {
                    string table = Convert.ToString(reader["name"]);
                    int year;
                    if (table.Length == 16 && int.TryParse(table.Substring(12), out year)
                        && table == "TblAuditLog_" + year
                        && year >= fromUtc.Year && year <= endUtc.Year)
                        tables.Add(table);
                }
            }

            var logs = new List<AuditLogDto>();
            foreach (string table in tables)
            {
                // Table names come from metadata and have been validated; values are parameters.
                var command = new QueryCommand(
                    "SELECT Id, Title, ReferenceId, TableName, RecordId, ActionType, Changes, "
                    + "UserId, ChangedBy, ChangedAt FROM dbo.[" + table + "] "
                    + "WHERE TableName = 'TblTaiLieu' AND RecordId = @DocumentId "
                    + "AND ChangedAt >= @FromUtc AND ChangedAt <= @ToUtc", provider);
                command.Parameters.Add("@DocumentId", documentId, DbType.Guid);
                command.Parameters.Add("@FromUtc", fromUtc, DbType.DateTime);
                command.Parameters.Add("@ToUtc", endUtc, DbType.DateTime);
                using (IDataReader reader = DataService.GetReader(command))
                {
                    while (reader.Read())
                    {
                        logs.Add(new AuditLogDto
                        {
                            Id = (Guid)reader["Id"],
                            Title = Convert.ToString(reader["Title"]),
                            RefId = reader["ReferenceId"] as Guid?,
                            TableName = Convert.ToString(reader["TableName"]),
                            RecordId = reader["RecordId"] as Guid?,
                            ActionType = Convert.ToString(reader["ActionType"]),
                            Changes = Convert.ToString(reader["Changes"]),
                            UserId = reader["UserId"] as Guid?,
                            ChangedBy = Convert.ToString(reader["ChangedBy"]),
                            ChangedAt = DateTime.SpecifyKind((DateTime)reader["ChangedAt"], DateTimeKind.Utc)
                        });
                    }
                }
            }
            return Task.FromResult(logs.OrderByDescending(log => log.ChangedAt)
                .ThenByDescending(log => log.Id).ToList());
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

        private static DataTable ExecuteDataTable(string sql)
        {
            IDataReader reader = new InlineQuery().ExecuteReader(sql);
            DataTable result = new DataTable();
            if (reader != null)
                result.Load(reader);
            return result;
        }

        private static void EnsureActiveUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new InvalidOperationException(
                    "Tài khoản hiện tại không hợp lệ.");

            DataTable rows = ExecuteDataTable(
                @"
                    SELECT TOP 1 u.UserId
                    FROM aspnet_Users u WITH (UPDLOCK, HOLDLOCK)
                    WHERE u.UserId = @UserId
                      AND u.IsDeleted = 0
                      AND u.IsActivated = 1;",
                new Dictionary<string, object>
                {
                    { "@UserId", userId }
                });
            if (rows.Rows.Count == 0)
                throw new InvalidOperationException(
                    "Tài khoản hiện tại không còn hoạt động.");
        }

        private static void InsertSigningHistory(
            Guid documentId,
            string actionType,
            Guid signingId,
            string changes,
            string description,
            Guid currentUserId,
            string currentUserName,
            DateTime currentDate)
        {
            ExecuteNonQuery(
                @"
                    INSERT INTO TblLichSuTaiLieu
                    (
                        IdLichSuTaiLieu,
                        IdTaiLieu,
                        LoaiHanhDong,
                        LoaiThamChieu,
                        IdThamChieu,
                        NoiDungThayDoi,
                        MoTa,
                        IdNhanVienThucHien,
                        NguoiTao,
                        NgayTao
                    )
                    VALUES
                    (
                        @HistoryId,
                        @DocumentId,
                        @ActionType,
                        @ReferenceType,
                        @ReferenceId,
                        @Changes,
                        @Description,
                        @ActorId,
                        @CurrentUserName,
                        @CurrentDate
                    );",
                new Dictionary<string, object>
                {
                    { "@HistoryId", Guid.NewGuid() },
                    { "@DocumentId", documentId },
                    { "@ActionType", actionType },
                    { "@ReferenceType", DocumentActivityReferenceKeys.Signing },
                    { "@ReferenceId", signingId },
                    { "@Changes", changes ?? string.Empty },
                    { "@Description", description ?? string.Empty },
                    { "@ActorId", currentUserId },
                    { "@CurrentUserName", currentUserName },
                    { "@CurrentDate", currentDate }
                });
        }

        private static string NormalizeUserName(string userName)
        {
            string value = string.IsNullOrWhiteSpace(userName)
                ? "[System]"
                : userName.Trim();
            return value.Length <= 150 ? value : value.Substring(0, 150);
        }

        private static bool IsAllowedSigningResultExtension(DataRow row)
        {
            if (row == null)
                return false;

            string extension = row.Table.Columns.Contains("Ext")
                ? NormalizeFileExtension(Convert.ToString(row["Ext"]))
                : string.Empty;
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = GetPathFileExtension(row.Table.Columns.Contains(
                    "OriginalFileName")
                    ? Convert.ToString(row["OriginalFileName"])
                    : string.Empty);
            }

            if (string.IsNullOrWhiteSpace(extension)
                && row.Table.Columns.Contains("Name"))
            {
                extension = GetPathFileExtension(
                    Convert.ToString(row["Name"]));
            }

            return extension == "pdf"
                || extension == "jpg"
                || extension == "jpeg"
                || extension == "png";
        }

        private static string GetPathFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            try
            {
                return NormalizeFileExtension(Path.GetExtension(fileName));
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        private static string NormalizeFileExtension(string extension)
        {
            string value = (extension ?? string.Empty).Trim();
            if (value.StartsWith(".", StringComparison.Ordinal))
                value = value.Substring(1);
            return value.ToLowerInvariant();
        }

        private static bool IsFileAvailable(object fileUrlValue)
        {
            string fileUrl = Convert.ToString(fileUrlValue);
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            Uri absoluteUri;
            if (Uri.TryCreate(fileUrl.Trim(), UriKind.Absolute, out absoluteUri))
            {
                return absoluteUri.Scheme == Uri.UriSchemeHttp
                    || absoluteUri.Scheme == Uri.UriSchemeHttps;
            }

            try
            {
                string virtualPath = fileUrl.Trim().Replace('\\', '/');
                if (virtualPath.StartsWith("~/", StringComparison.Ordinal))
                    virtualPath = virtualPath.Substring(1);
                if (!virtualPath.StartsWith("/", StringComparison.Ordinal))
                    virtualPath = "/" + virtualPath;

                string physicalPath = HostingEnvironment.MapPath(virtualPath);
                return !string.IsNullOrWhiteSpace(physicalPath)
                    && File.Exists(physicalPath);
            }
            catch
            {
                return false;
            }
        }

        private static DataTable ExecuteDataTable(
            string sql,
            IDictionary<string, object> parameters)
        {
            using (IDataReader reader = DataService.GetReader(
                CreateCommand(sql, parameters)))
            {
                DataTable result = new DataTable();
                if (reader != null)
                    result.Load(reader);
                return result;
            }
        }

        private static int ExecuteNonQuery(
            string sql,
            IDictionary<string, object> parameters)
        {
            return DataService.ExecuteQuery(CreateCommand(sql, parameters));
        }

        private static int ExecuteScalarInt(
            string sql,
            IDictionary<string, object> parameters)
        {
            object value = ExecuteScalarObject(sql, parameters);
            return value == null || value == DBNull.Value
                ? 0
                : Convert.ToInt32(value);
        }

        private static object ExecuteScalarObject(
            string sql,
            IDictionary<string, object> parameters)
        {
            return DataService.ExecuteScalar(CreateCommand(sql, parameters));
        }

        private static QueryCommand CreateCommand(
            string sql,
            IDictionary<string, object> parameters)
        {
            QueryCommand command = new QueryCommand(
                sql,
                TblTaiLieu.Schema.Provider.Name);
            if (parameters == null)
                return command;

            foreach (KeyValuePair<string, object> pair in parameters)
            {
                command.Parameters.Add(
                    pair.Key,
                    pair.Value ?? DBNull.Value,
                    GetDbType(pair.Value));
            }

            return command;
        }

        private static DbType GetDbType(object value)
        {
            if (value == null || value == DBNull.Value)
                return DbType.String;
            if (value is Guid)
                return DbType.Guid;
            if (value is DateTime)
                return DbType.DateTime;
            if (value is bool)
                return DbType.Boolean;
            if (value is int)
                return DbType.Int32;
            if (value is long)
                return DbType.Int64;
            if (value is decimal)
                return DbType.Decimal;
            return DbType.String;
        }

        private static string BuildGuidParameterList(
            IEnumerable<Guid> values,
            string parameterPrefix,
            out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();
            List<string> parameterNames = new List<string>();
            int index = 0;
            foreach (Guid value in values ?? Enumerable.Empty<Guid>())
            {
                string parameterName = "@" + parameterPrefix + index++;
                parameterNames.Add(parameterName);
                parameters[parameterName] = value;
            }

            return string.Join(",", parameterNames);
        }

        private static void InsertDocumentVersionDeletionHistory(
            IDictionary<string, object> parameters,
            string actorIdSql)
        {
            string referenceIdSql = parameters.ContainsKey("@ReferenceId")
                ? "@ReferenceId"
                : "NULL";
            string safeActorIdSql = string.Equals(
                    actorIdSql,
                    "@ActorId",
                    StringComparison.Ordinal)
                ? actorIdSql
                : "NULL";

            ExecuteNonQuery(
                @"
                    INSERT INTO TblLichSuTaiLieu
                    (
                        IdLichSuTaiLieu,
                        IdTaiLieu,
                        LoaiHanhDong,
                        LoaiThamChieu,
                        IdThamChieu,
                        NoiDungThayDoi,
                        MoTa,
                        IdNhanVienThucHien,
                        NguoiTao,
                        NgayTao
                    )
                    VALUES
                    (
                        @HistoryId,
                        @DocumentId,
                        @ActionType,
                        @ReferenceType,
                        "
                + referenceIdSql
                + @",
                        @Changes,
                        @Description,
                        "
                + safeActorIdSql
                + @",
                        @CurrentUserName,
                        @CurrentDate
                    );",
                parameters);
        }

        private static TblUploadFile ToUploadFile(DataRow row)
        {
            return new TblUploadFile
            {
                Id = GetGuid(row, "Id"),
                Name = Convert.ToString(row["Name"]),
                OriginalFileName = Convert.ToString(row["OriginalFileName"]),
                FileUrl = Convert.ToString(row["FileUrl"]),
                FileType = Convert.ToString(row["FileType"]),
                Ext = Convert.ToString(row["Ext"]),
                RefId = GetGuid(row, "RefId"),
                RefType = Convert.ToString(row["RefType"]),
                DisplayOrder = GetInt(row, "DisplayOrder"),
                FileSize = GetInt(row, "FileSize"),
                MimeType = Convert.ToString(row["MimeType"]),
                OwnerId = GetGuid(row, "OwnerId"),
                CreatedDate = GetDateTime(row, "CreatedDate"),
                IsDeleted = GetBoolean(row, "IsDeleted"),
                IsHost = GetBoolean(row, "IsHost"),
                IsSecretary = GetBoolean(row, "IsSecretary"),
                IsParticipant = GetBoolean(row, "IsParticipant")
            };
        }

        private static TblPhienBanTaiLieu ToDocumentVersion(DataRow row)
        {
            return new TblPhienBanTaiLieu
            {
                IdPhienBanTaiLieu = GetGuid(row, "IdPhienBanTaiLieu"),
                IdTaiLieu = GetGuid(row, "IdTaiLieu"),
                SoPhienBan = Convert.ToString(row["SoPhienBan"]),
                DaXoa = GetBoolean(row, "DaXoa"),
                LaPhienBanHienTai = GetBoolean(row, "LaPhienBanHienTai"),
                IdFileNoiDung = GetNullableGuid(row, "IdFileNoiDung")
            };
        }

        private static string BuildVersionDeletionDetails(
            TblPhienBanTaiLieu version,
            DataTable ownedFiles)
        {
            string fileName = string.Empty;
            if (version != null && version.IdFileNoiDung.HasValue)
            {
                foreach (DataRow row in ownedFiles.Rows)
                {
                    if (GetGuid(row, "Id") == version.IdFileNoiDung.Value)
                    {
                        fileName = GetFileDisplayName(row);
                        break;
                    }
                }
            }

            return "Phiên bản: v"
                + (version == null ? string.Empty : version.SoPhienBan)
                + "; Tệp: " + fileName
                + "; Id tệp: "
                + (version == null || !version.IdFileNoiDung.HasValue
                    ? string.Empty
                    : version.IdFileNoiDung.Value.ToString());
        }

        private static string GetFileDisplayName(DataRow row)
        {
            string originalName = Convert.ToString(row["OriginalFileName"]);
            if (!string.IsNullOrWhiteSpace(originalName))
                return originalName;
            return Convert.ToString(row["Name"]);
        }

        private static string GetFileDisplayName(TblUploadFile file)
        {
            if (file == null)
                return string.Empty;
            return string.IsNullOrWhiteSpace(file.OriginalFileName)
                ? file.Name
                : file.OriginalFileName;
        }

        private static Guid GetGuid(DataRow row, string columnName)
        {
            Guid value;
            return row != null
                && row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                && Guid.TryParse(Convert.ToString(row[columnName]), out value)
                ? value
                : Guid.Empty;
        }

        private static Guid? GetNullableGuid(DataRow row, string columnName)
        {
            Guid value = GetGuid(row, columnName);
            return value == Guid.Empty ? (Guid?)null : value;
        }

        private static Guid? ToNullableGuid(object value)
        {
            Guid parsed;
            return value != null
                && value != DBNull.Value
                && Guid.TryParse(Convert.ToString(value), out parsed)
                && parsed != Guid.Empty
                ? parsed
                : (Guid?)null;
        }

        private static bool GetBoolean(DataRow row, string columnName)
        {
            return row != null
                && row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                && Convert.ToBoolean(row[columnName]);
        }

        private static int GetInt(DataRow row, string columnName)
        {
            return row != null
                && row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static DateTime GetDateTime(DataRow row, string columnName)
        {
            return row != null
                && row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                ? Convert.ToDateTime(row[columnName])
                : DateTime.UtcNow;
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
                case "TenDuAn":
                    sqlColumn = "d.TenDuAn";
                    break;
                case "TenNhanVienPhuTrach":
                    sqlColumn = "nv.DisplayName";
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
