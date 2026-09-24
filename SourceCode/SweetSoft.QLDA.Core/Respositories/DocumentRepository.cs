using Newtonsoft.Json.Linq;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Hosting;

namespace SweetSoft.QLDA.Core.Respositories
{
    public sealed class DocumentGrant
    {
        public Guid UserId { get; set; }
        public bool CanView { get; set; }
        public bool CanUpdateInfo { get; set; }
        public bool CanManageFiles { get; set; }
        public bool CanSigning { get; set; }
        public bool CanCustomerDelivery { get; set; }
        public bool CanPhysicalStorage { get; set; }
        // Kept for backwards-compatible audit payloads and old callers. New
        // UI/code writes the granular flags instead of this broad flag.
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
    public sealed class DocumentFileSet
    {
        public Guid? VersionId { get; set; }
        public List<Guid> FileIds { get; set; } = new List<Guid>();
        public bool Created { get; set; }
    }

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

        // The repository fills this only after the business transaction has
        // succeeded. DocumentManager writes it to AuditLog after the scope
        // is disposed, so a rolled-back signing operation never has a log.
        public string AuditActivityType { get; set; }

        public string AuditReferenceType { get; set; }

        public Guid? AuditReferenceId { get; set; }

        public string AuditChanges { get; set; }

        public string AuditDescription { get; set; }
    }

    public sealed class DocumentCustomerDeliveryOperationResult
    {
        public Guid IdGuiNhanKhachHang { get; set; }

        public Guid IdPhienBanTaiLieu { get; set; }

        // The repository fills this only after the business transaction has
        // succeeded. DocumentManager writes it to AuditLog after the scope
        // is disposed, so a rolled-back delivery operation never has a log.
        public string AuditActivityType { get; set; }

        public string AuditReferenceType { get; set; }

        public Guid? AuditReferenceId { get; set; }

        public string AuditChanges { get; set; }

        public string AuditDescription { get; set; }
    }

    public sealed class DocumentPhysicalStorageOperationResult
    {
        public Guid IdLuuTruVatLy { get; set; }

        public string MaLuuTru { get; set; }

        // The repository fills this only after the business transaction has
        // succeeded. DocumentManager writes it to AuditLog after the scope
        // is disposed, so a rolled-back storage operation never has a log.
        public string AuditActivityType { get; set; }

        public string AuditReferenceType { get; set; }

        public Guid? AuditReferenceId { get; set; }

        public string AuditChanges { get; set; }

        public string AuditDescription { get; set; }
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

        public bool HasGroupRight(Guid userId, string key)
        {
            return Convert.ToBoolean(ExecuteScalarObject("SELECT dbo.fn_HoSo_GroupRight(@User,@Key)",
                new Dictionary<string, object> { { "@User", userId }, { "@Key", key } }));
        }

        public Guid? ResolveUploadDocument(Guid refId, string refType)
        {
            if(refType=="DocumentVersion") return GetById(refId)==null ? (Guid?)null : refId;
            if(refType!="DocumentSigningResult") return null;
            object value=ExecuteScalarObject("SELECT IdTaiLieu FROM dbo.TblTrinhKyTaiLieu WHERE IdTrinhKyTaiLieu=@Id AND DaXoa=0",
                new Dictionary<string,object>{{"@Id",refId}});
            return value==null||value==DBNull.Value ? (Guid?)null : (Guid)value;
        }

        public bool CanAccess(Guid userId, Guid documentId, string action)
        {
            return Convert.ToBoolean(ExecuteScalarObject("SELECT dbo.fn_HoSo_CanAccess(@User,@Id,@Action)",
                new Dictionary<string, object> { { "@User", userId }, { "@Id", documentId }, { "@Action", action } }));
        }

        public bool CanAccessProject(Guid userId, Guid projectId, string action)
        {
            return Convert.ToBoolean(ExecuteScalarObject("SELECT dbo.fn_HoSo_ProjectRight(@User,@Id,@Action)",
                new Dictionary<string, object> { { "@User", userId }, { "@Id", projectId }, { "@Action", action } }));
        }

        /// <summary>
        /// Checks a permission against both the project and the concrete
        /// document.  This is intentionally different from CanAccessProject:
        /// a user outside the project may still open a dossier when the
        /// project PM has granted that specific dossier to them.
        /// </summary>
        public bool CanAccessProjectDocument(
            Guid userId,
            Guid documentId,
            Guid projectId,
            string action)
        {
            return Convert.ToBoolean(ExecuteScalarObject(
                @"SELECT CASE WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.TblTaiLieu
                        WHERE IdTaiLieu=@DocumentId
                          AND IdDuAn=@ProjectId
                          AND DaXoa=0
                    )
                    AND dbo.fn_HoSo_CanAccess(@User,@DocumentId,@Action)=1
                    THEN 1 ELSE 0 END",
                new Dictionary<string, object>
                {
                    { "@User", userId },
                    { "@DocumentId", documentId },
                    { "@ProjectId", projectId },
                    { "@Action", action }
                }));
        }

        /// <summary>
        /// Quyền mở khu vực Hồ sơ dự án dành cho PM/thành viên, hoặc người
        /// ngoài dự án đã được cấp ít nhất một hồ sơ cụ thể. Quyền trên từng
        /// hồ sơ vẫn được kiểm tra riêng bằng fn_HoSo_CanAccess.
        /// </summary>
        public bool CanEnterProjectDocumentArea(Guid userId, Guid projectId)
        {
            return Convert.ToBoolean(ExecuteScalarObject(
                "SELECT dbo.fn_HoSo_ProjectAreaAccess(@User,@Id)",
                new Dictionary<string, object>
                {
                    { "@User", userId },
                    { "@Id", projectId }
                }));
        }

        public bool HasAnyProjectAccess(Guid userId, string action)
        {
            return ExecuteScalarInt("SELECT COUNT(*) FROM dbo.TblDuAn WHERE DaXoa=0 AND dbo.fn_HoSo_ProjectRight(@User,IdDuAn,@Action)=1",
                new Dictionary<string, object> { { "@User", userId }, { "@Action", action } }) > 0;
        }

        public DataTable GetGrantMembers(Guid documentId)
        {
            return GetGrantMembers(documentId, false);
        }

        public DataTable GetGrantMembers(Guid documentId, bool includeExternalUsers)
        {
            return GetGrantMembers(documentId, includeExternalUsers, false);
        }

        /// <summary>
        /// Returns the employees that can be selected in the dossier ACL.
        /// When onlyExternalUsers is true, project members are deliberately
        /// excluded so the popup can switch between two clear audiences.
        /// </summary>
        public DataTable GetGrantMembers(
            Guid documentId,
            bool includeExternalUsers,
            bool onlyExternalUsers)
        {
            return ExecuteDataTable(@"SELECT DISTINCT
                    u.UserId,
                    ISNULL(u.DisplayName,u.UserName) AS DisplayName,
                    CASE WHEN m.IdNhanVien IS NULL THEN 0 ELSE 1 END AS IsProjectMember,
                    CASE WHEN t.IdNhanVienPhuTrach=u.UserId THEN 1 ELSE 0 END AS IsResponsibleDefault,
                    CASE
                        WHEN t.IdNhanVienPhuTrach=u.UserId
                         AND dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.View')=1
                        THEN 1
                        ELSE ISNULL(g.CanView,0)
                    END AS CanView,
                    CASE
                        WHEN t.IdNhanVienPhuTrach=u.UserId
                         AND dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update')=1
                        THEN 1
                        ELSE ISNULL(g.CanUpdateInfo,0)
                    END AS CanUpdateInfo,
                    ISNULL(g.CanManageFiles,0) AS CanManageFiles,
                    ISNULL(g.CanSigning,0) AS CanSigning,
                    ISNULL(g.CanCustomerDelivery,0) AS CanCustomerDelivery,
                    ISNULL(g.CanPhysicalStorage,0) AS CanPhysicalStorage,
                    ISNULL(g.CanUpdate,0) AS CanUpdate,
                    ISNULL(g.CanDelete,0) AS CanDelete,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.View') AS MaxView,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update') AS MaxUpdateInfo,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update') AS MaxManageFiles,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update') AS MaxSigning,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update') AS MaxCustomerDelivery,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Update') AS MaxPhysicalStorage,
                    dbo.fn_HoSo_GroupRight(u.UserId,'ProjectDocument.Delete') AS MaxDelete
                FROM dbo.TblTaiLieu t
                JOIN dbo.TblDuAn d
                    ON d.IdDuAn=t.IdDuAn
                   AND d.DaXoa=0
                JOIN dbo.aspnet_Users u
                    ON u.IsDeleted=0
                   AND u.IsActivated=1
                   AND u.IsAnonymous=0
                   AND u.LaNhanVien=1
                LEFT JOIN dbo.TblThanhVienDuAn m
                    ON m.IdDuAn=t.IdDuAn
                   AND m.IdNhanVien=u.UserId
                   AND m.DaXoa=0
                LEFT JOIN dbo.TblTaiLieuQuyen g
                    ON g.IdTaiLieu=t.IdTaiLieu
                   AND g.UserId=u.UserId
                WHERE t.IdTaiLieu=@Id
                  AND t.DaXoa=0
                  AND (d.IdNhanVienQuanLy IS NULL OR u.UserId<>d.IdNhanVienQuanLy)
                  AND
                  (
                      (
                          @OnlyExternal=1
                          AND m.IdNhanVien IS NULL
                          AND
                          (
                              t.IdNhanVienPhuTrach IS NULL
                              OR t.IdNhanVienPhuTrach<>u.UserId
                          )
                      )
                      OR
                      (
                          @OnlyExternal=0
                          AND
                          (
                              m.IdNhanVien IS NOT NULL
                              OR t.IdNhanVienPhuTrach=u.UserId
                              OR @IncludeExternal=1
                          )
                      )
                  )
                ORDER BY DisplayName", new Dictionary<string, object>
                {
                    { "@Id", documentId },
                    { "@IncludeExternal", includeExternalUsers ? 1 : 0 },
                    { "@OnlyExternal", onlyExternalUsers ? 1 : 0 }
                });
        }

        public string GetGrantStamp(Guid documentId)
        {
            var rows = ExecuteDataTable("SELECT UserId,CanView,CanUpdateInfo,CanManageFiles,CanSigning,CanCustomerDelivery,CanPhysicalStorage,CanUpdate,CanDelete,UpdatedAt FROM dbo.TblTaiLieuQuyen WHERE IdTaiLieu=@Id ORDER BY UserId",
                new Dictionary<string, object> { { "@Id", documentId } });
            string values = string.Join("|", rows.AsEnumerable().Select(r => string.Join(",",r.ItemArray.Select(v => v is DateTime ? ((DateTime)v).ToString("O") : Convert.ToString(v)))));
            using(var hash=System.Security.Cryptography.SHA256.Create())
                return Convert.ToBase64String(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(values)));
        }

        public void SaveGrants(Guid actor, Guid documentId, IEnumerable<DocumentGrant> grants, string expectedStamp)
        {
            var items=grants.ToList();
            if(items.Select(x=>x.UserId).Distinct().Count()!=items.Count) throw new InvalidOperationException("Nhân viên bị trùng.");
            using(var scope=new TransactionScope(TransactionScopeOption.Required,TimeSpan.FromMinutes(2))) {
                ExecuteScalarInt("SELECT COUNT(*) FROM dbo.TblTaiLieu WITH(UPDLOCK,HOLDLOCK) WHERE IdTaiLieu=@Id",new Dictionary<string,object>{{"@Id",documentId}});
                if(!CanAccess(actor,documentId,"Manage")) throw new UnauthorizedAccessException("Bạn không được cấp quyền hồ sơ này.");
                if(GetGrantStamp(documentId)!=expectedStamp) throw new InvalidOperationException("Quyền đã thay đổi. Hãy đóng và mở lại popup.");
                bool canGrantOutsideProject = HasGroupRight(actor, "Document.View")
                    && HasGroupRight(actor, "Document.Update");
                var members=GetGrantMembers(documentId, canGrantOutsideProject)
                    .AsEnumerable()
                    .ToDictionary(r=>(Guid)r["UserId"]);
                foreach(var item in items) {
                    DataRow member;
                    if(!members.TryGetValue(item.UserId,out member)) throw new InvalidOperationException("Nhân viên không còn thuộc dự án hoặc không nằm trong danh sách được cấp quyền.");
                    if (!canGrantOutsideProject
                        && !Convert.ToBoolean(member["IsProjectMember"]))
                    {
                        /* Người phụ trách ngoài dự án vẫn có quyền mặc định
                           theo hồ sơ, nhưng PM dự án không được tạo/sửa ACL
                           riêng cho một người ngoài. */
                        continue;
                    }
                    bool hasGranularUpdate = item.CanUpdateInfo
                        || item.CanManageFiles
                        || item.CanSigning
                        || item.CanCustomerDelivery
                        || item.CanPhysicalStorage;
                    if((hasGranularUpdate||item.CanDelete)&&!item.CanView) throw new InvalidOperationException("Quyền thao tác/xóa phải kèm quyền xem.");
                    if((item.CanView&&!Convert.ToBoolean(member["MaxView"]))
                        || (item.CanUpdateInfo&&!Convert.ToBoolean(member["MaxUpdateInfo"]))
                        || (item.CanManageFiles&&!Convert.ToBoolean(member["MaxManageFiles"]))
                        || (item.CanSigning&&!Convert.ToBoolean(member["MaxSigning"]))
                        || (item.CanCustomerDelivery&&!Convert.ToBoolean(member["MaxCustomerDelivery"]))
                        || (item.CanPhysicalStorage&&!Convert.ToBoolean(member["MaxPhysicalStorage"]))
                        || (item.CanDelete&&!Convert.ToBoolean(member["MaxDelete"])))
                        throw new InvalidOperationException("Quyền cấp vượt quyền nhóm hiện tại. Hãy mở lại popup.");
                    ExecuteNonQuery(@"UPDATE dbo.TblTaiLieuQuyen
                        SET CanView=@V,CanUpdateInfo=@I,CanManageFiles=@F,CanSigning=@S,
                            CanCustomerDelivery=@C,CanPhysicalStorage=@P,CanUpdate=0,
                            CanDelete=@D,UpdatedBy=@Actor,UpdatedAt=SYSUTCDATETIME()
                        WHERE IdTaiLieu=@Id AND UserId=@User;
                        IF @@ROWCOUNT=0 INSERT dbo.TblTaiLieuQuyen
                            (IdTaiLieu,UserId,CanView,CanUpdateInfo,CanManageFiles,CanSigning,
                             CanCustomerDelivery,CanPhysicalStorage,CanUpdate,CanDelete,UpdatedBy,UpdatedAt)
                            VALUES(@Id,@User,@V,@I,@F,@S,@C,@P,0,@D,@Actor,SYSUTCDATETIME());",
                        new Dictionary<string,object>{{"@Id",documentId},{"@User",item.UserId},{"@Actor",actor},
                            {"@V",item.CanView},{"@I",item.CanUpdateInfo},{"@F",item.CanManageFiles},
                            {"@S",item.CanSigning},{"@C",item.CanCustomerDelivery},
                            {"@P",item.CanPhysicalStorage},{"@D",item.CanDelete}});
                }
                scope.Complete();
            }
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
                      AND dbo.fn_HoSo_CanAccess('{SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.UserId:D}',t.IdTaiLieu,'View')=1
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
                           OR EXISTS
                           (
                               SELECT 1
                               FROM TblLuuTruVatLy storage
                               WHERE storage.IdTaiLieu = t.IdTaiLieu
                                 AND storage.DaXoa = 0
                                 AND storage.LaViTriHienTai = 1
                                 AND storage.MaLuuTru LIKE @keyword
                           )
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
            if (item == null || item.DaXoa)
                return null;

            return item;
        }

        public TblTaiLieu GetCompanyById(Guid id)
        {
            TblTaiLieu item = GetById(id);
            return item != null && !item.IdDuAn.HasValue
                ? item
                : null;
        }

        public TblTaiLieu GetProjectById(Guid id, Guid projectId)
        {
            if (projectId == Guid.Empty)
                return null;

            TblTaiLieu item = GetById(id);
            return item != null
                && item.IdDuAn.HasValue
                && item.IdDuAn.Value == projectId
                ? item
                : null;
        }

        /// <summary>
        /// Checks the optional execution-contract link without relying on a
        /// regenerated SubSonic property.  The dynamic branch keeps normal
        /// document operations compatible until the additive migration has
        /// been installed in an environment.
        /// </summary>
        public bool IsDocumentLinkedToActiveContract(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            string sql = $@"
                IF COL_LENGTH(N'dbo.TblHopDongThucHien', N'IdTaiLieu') IS NULL
                BEGIN
                    SELECT CAST(0 AS INT);
                    RETURN;
                END;

                DECLARE @sql NVARCHAR(MAX) = N'
                    SELECT CASE WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.TblHopDongThucHien
                        WHERE IdTaiLieu = ''{idTaiLieu}''
                          AND DaXoa = 0
                    ) THEN 1 ELSE 0 END;';

                EXEC sys.sp_executesql @sql;";

            return new InlineQuery().ExecuteScalar<int>(sql) == 1;
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

        public bool IsProjectCodeExisted(
            string maTaiLieu,
            Guid projectId,
            Guid excludeId)
        {
            if (projectId == Guid.Empty)
                return false;

            string safeCode = Encode(maTaiLieu, 100);
            string excludeSql = excludeId == Guid.Empty
                ? "NULL"
                : "'" + excludeId + "'";
            string sql = $@"
                DECLARE @excludeId UNIQUEIDENTIFIER = {excludeSql};
                SELECT COUNT(1)
                FROM TblTaiLieu
                WHERE DaXoa = 0
                  AND IdDuAn = '{projectId}'
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
        private DocumentVersionFileDeletionResult LegacyDeleteDocumentVersionFiles(
            Guid idTaiLieu,
            IEnumerable<Guid> fileIds,
            string currentUserName,
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
                          AND d.DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ.");
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

        public DocumentFileSet GetCurrentDocumentFileSet(Guid documentId)
        {
            DataTable rows = ExecuteDataTable(@"
                SELECT IdPhienBanTaiLieu, IdFileNoiDung, DanhSachFileJson
                FROM dbo.TblPhienBanTaiLieu
                WHERE IdTaiLieu=@DocumentId AND DaXoa=0 AND LaPhienBanHienTai=1;",
                new Dictionary<string, object> { { "@DocumentId", documentId } });
            if (rows.Rows.Count > 1)
                throw new InvalidOperationException("Hồ sơ có nhiều phiên bản hiện tại; cần kiểm tra dữ liệu.");
            return rows.Rows.Count == 0 ? new DocumentFileSet() : new DocumentFileSet
            {
                VersionId = (Guid)rows.Rows[0]["IdPhienBanTaiLieu"],
                FileIds = ReadDocumentFileIds(rows.Rows[0])
            };
        }

        public List<Guid> GetDocumentVersionFileIds(
            Guid documentId,
            Guid versionId)
        {
            if (documentId == Guid.Empty || versionId == Guid.Empty)
                return new List<Guid>();

            DataTable rows = ExecuteDataTable(@"
                SELECT TOP 1 IdFileNoiDung, DanhSachFileJson
                FROM dbo.TblPhienBanTaiLieu
                WHERE IdTaiLieu=@DocumentId
                  AND IdPhienBanTaiLieu=@VersionId
                  AND DaXoa=0;",
                new Dictionary<string, object>
                {
                    { "@DocumentId", documentId },
                    { "@VersionId", versionId }
                });

            return rows.Rows.Count == 0
                ? new List<Guid>()
                : ReadDocumentFileIds(rows.Rows[0]);
        }

        private static List<Guid> ReadDocumentFileIds(DataRow row)
        {
            if (row["DanhSachFileJson"] == DBNull.Value)
                return row["IdFileNoiDung"] == DBNull.Value ? new List<Guid>()
                    : new List<Guid> { (Guid)row["IdFileNoiDung"] };
            JArray items;
            try { items = JArray.Parse(Convert.ToString(row["DanhSachFileJson"])); }
            catch (Newtonsoft.Json.JsonException)
            { throw new InvalidOperationException("Danh sách file của phiên bản không hợp lệ."); }
            var result = new List<Guid>();
            foreach (JToken item in items)
            {
                Guid id;
                if (item.Type != JTokenType.String || !Guid.TryParse((string)item, out id)
                    || id == Guid.Empty || result.Contains(id))
                    throw new InvalidOperationException("Danh sách file chứa ID không hợp lệ hoặc bị trùng.");
                result.Add(id);
            }
            return result;
        }

        public DocumentFileSet SaveDocumentFileSet(
            Guid documentId,
            Guid? expectedVersionId,
            IEnumerable<Guid> fileIds,
            string userName,
            DateTime now)
        {
            return SaveDocumentFileSet(
                documentId,
                expectedVersionId,
                fileIds,
                userName,
                now,
                null);
        }

        public DocumentFileSet SaveDocumentFileSet(
            Guid documentId,
            Guid? expectedVersionId,
            IEnumerable<Guid> fileIds,
            string userName,
            DateTime now,
            string description)
        {
            return SaveDocumentFileSet(
                documentId,
                expectedVersionId,
                fileIds,
                userName,
                now,
                description,
                null);
        }

        public DocumentFileSet SaveDocumentFileSet(
            Guid documentId,
            Guid? expectedVersionId,
            IEnumerable<Guid> fileIds,
            string userName,
            DateTime now,
            string description,
            string source)
        {
            var ids = (fileIds ?? Enumerable.Empty<Guid>()).ToList();
            if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
                throw new InvalidOperationException("Danh sách file không hợp lệ hoặc bị trùng.");
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TimeSpan(0, 2, 0)))
            {
                var parameters = new Dictionary<string, object> { { "@DocumentId", documentId } };
                if (ExecuteScalarInt(@"SELECT COUNT(*) FROM dbo.TblTaiLieu WITH (UPDLOCK,HOLDLOCK)
                    WHERE IdTaiLieu=@DocumentId AND DaXoa=0;", parameters) != 1)
                    throw new InvalidOperationException("Không tìm thấy hồ sơ.");
                DocumentFileSet current = GetCurrentDocumentFileSet(documentId);
                // The dossier lock serializes writers; equal content also makes request retries harmless.
                if (new HashSet<Guid>(current.FileIds).SetEquals(ids))
                {
                    scope.Complete();
                    return current;
                }
                if (current.VersionId != expectedVersionId)
                    throw new InvalidOperationException("Hồ sơ đã có phiên bản mới. Vui lòng tải lại trang trước khi lưu.");

                var ownedIds = new HashSet<Guid>(GetDocumentVersionFiles(documentId).Select(f => f.Id));
                if (ids.Any(id => !ownedIds.Contains(id)))
                    throw new InvalidOperationException("File không tồn tại hoặc không thuộc hồ sơ này.");

                decimal maximum = 0;
                foreach (var version in GetDocumentVersions(documentId, true))
                {
                    decimal number;
                    if (decimal.TryParse(version.SoPhienBan, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out number)) maximum = Math.Max(maximum, number);
                }
                string nextNumber = (Math.Floor(maximum) + 1).ToString("0.0", CultureInfo.InvariantCulture);
                if (nextNumber.Length > 20) throw new InvalidOperationException("Số phiên bản vượt giới hạn.");
                Guid newId = Guid.NewGuid();
                parameters["@VersionId"] = newId;
                parameters["@PreviousId"] = (object)current.VersionId ?? DBNull.Value;
                parameters["@Number"] = nextNumber;
                parameters["@Json"] = new JArray(ids.Select(id => id.ToString("D"))).ToString(Newtonsoft.Json.Formatting.None);
                // Legacy single-file workflows remain valid only for a singleton snapshot.
                parameters["@SingleFileId"] = ids.Count == 1 ? (object)ids[0] : DBNull.Value;
                parameters["@User"] = string.IsNullOrWhiteSpace(userName) ? "[System]"
                    : userName.Substring(0, Math.Min(userName.Length, 150));
                parameters["@Now"] = now;
                parameters["@Description"] = string.IsNullOrWhiteSpace(description)
                    ? "Lưu bộ hồ sơ gồm " + ids.Count + " file."
                    : description.Substring(0, Math.Min(description.Length, 500));
                parameters["@Source"] = string.IsNullOrWhiteSpace(source)
                    ? "UPLOAD"
                    : source.Substring(0, Math.Min(source.Length, 50));
                ExecuteNonQuery(@"
                    UPDATE dbo.TblPhienBanTaiLieu SET LaPhienBanHienTai=0,
                        NguoiCapNhat=@User, NgayCapNhat=@Now
                    WHERE IdTaiLieu=@DocumentId AND LaPhienBanHienTai=1;
                    INSERT dbo.TblPhienBanTaiLieu
                        (IdPhienBanTaiLieu,IdTaiLieu,SoPhienBan,NguonTao,IdPhienBanNguon,
                         MoTaPhienBan,LaPhienBanHienTai,DaXoa,NguoiTao,NgayTao,IdFileNoiDung,DanhSachFileJson)
                    VALUES (@VersionId,@DocumentId,@Number,@Source,@PreviousId,
                        @Description,1,0,@User,@Now,@SingleFileId,@Json);", parameters);
                scope.Complete();
                return new DocumentFileSet { VersionId = newId, FileIds = ids, Created = true };
            }
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
                    fileCount.FileCount,
                    u.Id AS IdFile,
                    u.Name AS TenFile,
                    u.OriginalFileName AS TenFileGoc,
                    u.FileUrl,
                    u.Ext,
                    u.FileSize,
                    ISNULL(NULLIF(creator.DisplayName, N''), p.NguoiTao)
                        AS TenNguoiTao
                FROM TblPhienBanTaiLieu p
                CROSS APPLY (SELECT COALESCE(p.DanhSachFileJson,
                    CASE WHEN p.IdFileNoiDung IS NULL THEN N'[]'
                    ELSE N'[""' + CONVERT(nvarchar(36),p.IdFileNoiDung) + N'""]' END) AS FileJson) snapshot
                CROSS APPLY (SELECT COUNT(*) AS FileCount FROM OPENJSON(snapshot.FileJson)) fileCount
                OUTER APPLY OPENJSON(snapshot.FileJson) member
                LEFT JOIN TblUploadFile u
                    ON u.Id = TRY_CONVERT(uniqueidentifier, member.value)
                   AND u.RefId = p.IdTaiLieu AND u.RefType = 'DocumentVersion'
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
            return GetDocumentDetail(idTaiLieu, null);
        }

        public DataTable GetProjectDocumentDetail(
            Guid idTaiLieu,
            Guid projectId)
        {
            if (projectId == Guid.Empty)
                return new DataTable();

            return GetDocumentDetail(idTaiLieu, projectId);
        }

        private DataTable GetDocumentDetail(
            Guid idTaiLieu,
            Guid? projectId)
        {
            if (idTaiLieu == Guid.Empty)
                return new DataTable();

            string scopeCondition = projectId.HasValue
                ? "AND t.IdDuAn = @ProjectId"
                : "AND t.IdDuAn IS NULL";

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
                  {scopeCondition}
                  AND t.DaXoa = 0;";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (projectId.HasValue)
                parameters["@ProjectId"] = projectId.Value;

            return ExecuteDataTable(sql, parameters);
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
                          AND d.DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ.");

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

                result.AuditActivityType =
                    DocumentActivityTypeKeys.SubmitSigning;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.Signing;
                result.AuditReferenceId = signingId;
                result.AuditChanges = "Phiên bản: v"
                    + Convert.ToString(currentVersion["SoPhienBan"])
                    + "; Người ký: "
                    + (string.IsNullOrWhiteSpace(safeSignerName)
                        ? idNguoiKy.ToString()
                        : safeSignerName)
                    + "; Hình thức ký: " + safeMethod;
                result.AuditDescription = "Đã trình ký hồ sơ.";

                scope.Complete();
                result.IdPhienBanTaiLieu = versionId;
                result.IdFile = fileId;
            }

            return result;
        }

        public DocumentSigningOperationResult RequestDocumentSigningChanges(
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
                          AND d.DaXoa = 0
                          AND d.CanTrinhKy = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ cần trình ký.");

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
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@DocumentStatus", DocumentStatusKeys.ChangesRequested },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                result.AuditActivityType =
                    DocumentActivityTypeKeys.RequestSigningChanges;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.Signing;
                result.AuditReferenceId = idTrinhKyTaiLieu;
                result.AuditChanges = "Phiên bản: v"
                    + Convert.ToString(signingRows.Rows[0]["SoPhienBan"])
                    + "; Lý do: " + safeReason;
                result.AuditDescription =
                    "Đã yêu cầu điều chỉnh hồ sơ trình ký.";

                scope.Complete();
            }

            return result;
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
                          AND d.DaXoa = 0
                          AND d.CanTrinhKy = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ cần trình ký.");

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
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@DocumentStatus", DocumentStatusKeys.Signed },
                        { "@ResultFileId", resultFileId },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                result.AuditActivityType =
                    DocumentActivityTypeKeys.CompleteSigning;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.Signing;
                result.AuditReferenceId = idTrinhKyTaiLieu;
                result.AuditChanges = "Phiên bản: v"
                    + Convert.ToString(signingRows.Rows[0]["SoPhienBan"])
                    + "; Tệp sau ký: "
                    + GetFileDisplayName(resultFile);
                result.AuditDescription = "Đã hoàn tất ký hồ sơ.";

                scope.Complete();
                result.IdPhienBanTaiLieu = GetGuid(
                    signingRows.Rows[0],
                    "IdPhienBanTaiLieu");
                result.IdFile = resultFileId;
            }

            return result;
        }

        public DataTable GetCustomerDeliveryDetail(
            Guid idTaiLieu,
            Guid idGuiNhanKhachHang)
        {
            if (idTaiLieu == Guid.Empty
                || idGuiNhanKhachHang == Guid.Empty)
            {
                return new DataTable();
            }

            return ExecuteDataTable(
                @"
                    SELECT TOP 1
                        g.IdGuiNhanKhachHang,
                        g.IdPhienBanTaiLieu,
                        p.SoPhienBan,
                        g.IdKhachHang,
                        ISNULL(k.TenKhachHang, N'') AS TenKhachHang,
                        g.TenNguoiNhan,
                        g.EmailNguoiNhan,
                        g.KenhGui,
                        g.TrangThai,
                        g.HanPhanHoi,
                        g.GhiChu
                    FROM TblGuiNhanKhachHang g
                    INNER JOIN TblPhienBanTaiLieu p
                        ON p.IdPhienBanTaiLieu = g.IdPhienBanTaiLieu
                       AND p.DaXoa = 0
                    LEFT JOIN TblKhachHang k
                        ON k.IdKhachHang = g.IdKhachHang
                       AND k.DaXoa = 0
                    WHERE g.IdGuiNhanKhachHang = @DeliveryId
                      AND g.DaXoa = 0
                      AND p.IdTaiLieu = @DocumentId;",
                new Dictionary<string, object>
                {
                    { "@DeliveryId", idGuiNhanKhachHang },
                    { "@DocumentId", idTaiLieu }
                });
        }

        public DocumentCustomerDeliveryOperationResult
            SendDocumentToCustomer(
                Guid idTaiLieu,
                Guid idPhienBanTaiLieu,
                Guid idKhachHang,
                string tenNguoiNhan,
                string emailNguoiNhan,
                string kenhGui,
                DateTime? hanPhanHoi,
                bool choPhepGuiTruocKhiKy,
                string ghiChu,
                Guid currentUserId,
                string currentUserName,
                DateTime currentDate)
        {
            // This is the business delivery record. SMTP delivery is intentionally
            // handled separately, so this operation never implies an email was
            // transmitted by the system.
            if (idTaiLieu == Guid.Empty
                || idPhienBanTaiLieu == Guid.Empty
                || idKhachHang == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Thông tin gửi khách hàng không hợp lệ.");
            }

            string safeRecipient = (tenNguoiNhan ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(safeRecipient))
            {
                throw new InvalidOperationException(
                    "Vui lòng nhập người nhận.");
            }
            if (safeRecipient.Length > 150)
            {
                throw new InvalidOperationException(
                    "Người nhận không được vượt quá 150 ký tự.");
            }

            string safeEmail = (emailNguoiNhan ?? string.Empty).Trim();
            if (safeEmail.Length > 256)
            {
                throw new InvalidOperationException(
                    "Email người nhận không được vượt quá 256 ký tự.");
            }
            if (!string.IsNullOrWhiteSpace(safeEmail)
                && !RegexUtilities.IsValidEmail(safeEmail))
            {
                throw new InvalidOperationException(
                    "Email người nhận không hợp lệ.");
            }

            string safeChannel = (kenhGui ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (!IsCustomerDeliveryChannel(safeChannel))
            {
                throw new InvalidOperationException(
                    "Kênh gửi khách hàng không hợp lệ.");
            }
            if (safeChannel == DocumentCustomerDeliveryChannelKeys.Email
                && string.IsNullOrWhiteSpace(safeEmail))
            {
                throw new InvalidOperationException(
                    "Vui lòng nhập email người nhận khi chọn kênh Email.");
            }

            string safeNote = (ghiChu ?? string.Empty).Trim();
            if (safeNote.Length > 500)
            {
                throw new InvalidOperationException(
                    "Ghi chú không được vượt quá 500 ký tự.");
            }
            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException("Tài khoản hiện tại không hợp lệ.");

            string safeUserName = NormalizeUserName(currentUserName);
            Guid deliveryId = Guid.NewGuid();
            DocumentCustomerDeliveryOperationResult result =
                new DocumentCustomerDeliveryOperationResult
                {
                    IdGuiNhanKhachHang = deliveryId,
                    IdPhienBanTaiLieu = idPhienBanTaiLieu
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
                            d.IdFileBanChinhThuc
                        FROM TblTaiLieu d WITH (UPDLOCK, HOLDLOCK)
                        WHERE d.IdTaiLieu = @DocumentId
                          AND d.DaXoa = 0
                          AND d.CanGuiKhachHang = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ cần gửi khách hàng hoặc hồ sơ chưa được cấu hình gửi khách hàng.");
                }

                EnsureActiveUser(currentUserId);

                DataTable customerRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            k.IdKhachHang,
                            k.TenKhachHang
                        FROM TblKhachHang k WITH (UPDLOCK, HOLDLOCK)
                        WHERE k.IdKhachHang = @CustomerId
                          AND k.DaXoa = 0
                          AND k.KichHoat = 1;",
                    new Dictionary<string, object>
                    {
                        { "@CustomerId", idKhachHang }
                    });
                if (customerRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Khách hàng không tồn tại hoặc đã ngừng hoạt động.");
                }

                DataTable versionRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            p.IdPhienBanTaiLieu,
                            p.SoPhienBan,
                            p.IdFileNoiDung,
                            f.Id AS IdFile,
                            f.FileUrl
                        FROM TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblUploadFile f WITH (UPDLOCK, HOLDLOCK)
                            ON f.Id = p.IdFileNoiDung
                           AND f.RefId = @DocumentId
                           AND f.RefType = @VersionRefType
                           AND f.IsDeleted = 0
                        WHERE p.IdPhienBanTaiLieu = @VersionId
                          AND p.IdTaiLieu = @DocumentId
                          AND p.DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@VersionId", idPhienBanTaiLieu },
                        {
                            "@VersionRefType",
                            FileUploadTypes.DocumentVersion.ToString()
                        }
                    });
                if (versionRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Phiên bản được chọn không tồn tại hoặc không thuộc hồ sơ này.");
                }

                DataRow document = documentRows.Rows[0];
                DataRow version = versionRows.Rows[0];
                Guid versionFileId = GetGuid(version, "IdFile");
                if (versionFileId == Guid.Empty
                    || !IsFileAvailable(version["FileUrl"]))
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy tệp vật lý của phiên bản được chọn.");
                }

                DataTable signedRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1 s.IdTrinhKyTaiLieu
                        FROM TblTrinhKyTaiLieu s WITH (UPDLOCK, HOLDLOCK)
                        WHERE s.IdPhienBanTaiLieu = @VersionId
                          AND s.DaXoa = 0
                          AND s.TrangThaiTrinhKy = @SignedStatus;",
                    new Dictionary<string, object>
                    {
                        { "@VersionId", idPhienBanTaiLieu },
                        { "@SignedStatus", DocumentSigningStatusKeys.Signed }
                    });
                bool isSignedVersion = signedRows.Rows.Count > 0;
                bool isPreSigningDelivery = GetBoolean(
                    document,
                    "CanTrinhKy") && !isSignedVersion;
                if (isPreSigningDelivery && !choPhepGuiTruocKhiKy)
                {
                    throw new InvalidOperationException(
                        "Hồ sơ yêu cầu trình ký; vui lòng chọn phiên bản đã ký hoặc bật tùy chọn gửi trước khi ký.");
                }

                Guid officialFileId = GetGuid(document, "IdFileBanChinhThuc");
                // A version explicitly sent before it is signed must never be
                // recorded as an official customer delivery, even when it was
                // temporarily selected as the document's official file.
                bool isOfficialVersion = !isPreSigningDelivery
                    && (isSignedVersion
                    || (officialFileId != Guid.Empty
                        && officialFileId == versionFileId));

                ExecuteNonQuery(
                    @"
                        INSERT INTO TblGuiNhanKhachHang
                        (
                            IdGuiNhanKhachHang,
                            IdPhienBanTaiLieu,
                            IdKhachHang,
                            IdNguoiThucHien,
                            TenNguoiNhan,
                            EmailNguoiNhan,
                            NgayGui,
                            HanPhanHoi,
                            NgayNhanLai,
                            KenhGui,
                            TrangThai,
                            LaBanChinhThuc,
                            GhiChu,
                            DaXoa,
                            NguoiTao,
                            NguoiCapNhat,
                            NgayCapNhat,
                            IdFileNhanLai
                        )
                        VALUES
                        (
                            @DeliveryId,
                            @VersionId,
                            @CustomerId,
                            @CurrentUserId,
                            @Recipient,
                            NULLIF(@RecipientEmail, ''),
                            @CurrentDate,
                            @ResponseDeadline,
                            NULL,
                            @Channel,
                            @SentStatus,
                            @IsOfficialVersion,
                            NULLIF(@Note, ''),
                            0,
                            @CurrentUserName,
                            @CurrentUserName,
                            @CurrentDate,
                            NULL
                        );",
                    new Dictionary<string, object>
                    {
                        { "@DeliveryId", deliveryId },
                        { "@VersionId", idPhienBanTaiLieu },
                        { "@CustomerId", idKhachHang },
                        { "@CurrentUserId", currentUserId },
                        { "@Recipient", safeRecipient },
                        { "@RecipientEmail", safeEmail },
                        { "@CurrentDate", currentDate },
                        {
                            "@ResponseDeadline",
                            hanPhanHoi.HasValue
                                ? (object)hanPhanHoi.Value
                                : DBNull.Value
                        },
                        { "@Channel", safeChannel },
                        { "@SentStatus", DocumentCustomerStatusKeys.Sent },
                        { "@IsOfficialVersion", isOfficialVersion },
                        { "@Note", safeNote },
                        { "@CurrentUserName", safeUserName }
                    });

                ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiGuiKhach = @SentStatus,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@SentStatus", DocumentCustomerStatusKeys.Sent },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                string customerName = Convert.ToString(
                    customerRows.Rows[0]["TenKhachHang"]);
                result.AuditActivityType =
                    DocumentActivityTypeKeys.SendCustomerDelivery;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.CustomerDelivery;
                result.AuditReferenceId = deliveryId;
                result.AuditChanges = "Phiên bản: v"
                    + Convert.ToString(version["SoPhienBan"])
                    + "; Khách hàng: " + customerName
                    + "; Người nhận: " + safeRecipient
                    + "; Kênh: " + safeChannel
                    + "; Bản gửi: " + (isOfficialVersion
                        ? "Chính thức"
                        : "Chưa ký/chưa chính thức")
                    + (isPreSigningDelivery
                        ? "; Gửi trước khi ký: Có"
                        : string.Empty)
                    + (hanPhanHoi.HasValue
                        ? "; Hạn phản hồi: "
                            + hanPhanHoi.Value.ToString("dd/MM/yyyy")
                        : string.Empty);
                result.AuditDescription =
                    isPreSigningDelivery
                        ? "Đã ghi nhận gửi hồ sơ cho khách hàng trước khi ký."
                        : "Đã ghi nhận gửi hồ sơ cho khách hàng.";

                scope.Complete();
            }

            return result;
        }

        public DocumentCustomerDeliveryOperationResult
            UpdateCustomerDeliveryStatus(
                Guid idTaiLieu,
                Guid idGuiNhanKhachHang,
                string trangThai,
                string ghiChu,
                Guid currentUserId,
                string currentUserName,
                DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty || idGuiNhanKhachHang == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Không xác định được lần gửi khách hàng.");
            }
            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException("Tài khoản hiện tại không hợp lệ.");

            string safeStatus = (trangThai ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (!IsCustomerDeliveryStatus(safeStatus))
            {
                throw new InvalidOperationException(
                    "Trạng thái gửi khách hàng không hợp lệ.");
            }

            string safeNote = (ghiChu ?? string.Empty).Trim();
            if (safeNote.Length > 500)
            {
                throw new InvalidOperationException(
                    "Ghi chú không được vượt quá 500 ký tự.");
            }

            string safeUserName = NormalizeUserName(currentUserName);
            DocumentCustomerDeliveryOperationResult result =
                new DocumentCustomerDeliveryOperationResult
                {
                    IdGuiNhanKhachHang = idGuiNhanKhachHang
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
                          AND d.DaXoa = 0
                          AND d.CanGuiKhachHang = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ cần cập nhật gửi khách hàng.");
                }

                EnsureActiveUser(currentUserId);

                DataTable deliveryRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            g.IdGuiNhanKhachHang,
                            g.IdPhienBanTaiLieu,
                            g.TrangThai,
                            p.SoPhienBan,
                            ISNULL(k.TenKhachHang, N'') AS TenKhachHang,
                            g.TenNguoiNhan
                        FROM TblGuiNhanKhachHang g WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN TblPhienBanTaiLieu p WITH (UPDLOCK, HOLDLOCK)
                            ON p.IdPhienBanTaiLieu = g.IdPhienBanTaiLieu
                           AND p.DaXoa = 0
                        LEFT JOIN TblKhachHang k
                            ON k.IdKhachHang = g.IdKhachHang
                           AND k.DaXoa = 0
                        WHERE g.IdGuiNhanKhachHang = @DeliveryId
                          AND g.DaXoa = 0
                          AND p.IdTaiLieu = @DocumentId;",
                    new Dictionary<string, object>
                    {
                        { "@DeliveryId", idGuiNhanKhachHang },
                        { "@DocumentId", idTaiLieu }
                    });
                if (deliveryRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Lần gửi khách hàng không tồn tại hoặc không thuộc hồ sơ này.");
                }

                ExecuteNonQuery(
                    @"
                        UPDATE TblGuiNhanKhachHang
                        SET TrangThai = @Status,
                            NgayNhanLai = CASE
                                WHEN @Status = @ReceivedBackStatus
                                THEN ISNULL(NgayNhanLai, @CurrentDate)
                                ELSE NULL
                            END,
                            GhiChu = CASE
                                WHEN NULLIF(@Note, '') IS NULL THEN GhiChu
                                ELSE @Note
                            END,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdGuiNhanKhachHang = @DeliveryId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DeliveryId", idGuiNhanKhachHang },
                        { "@Status", safeStatus },
                        {
                            "@ReceivedBackStatus",
                            DocumentCustomerStatusKeys.ReceivedBack
                        },
                        { "@Note", safeNote },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiGuiKhach = @Status,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@Status", safeStatus },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                DataRow delivery = deliveryRows.Rows[0];
                result.IdPhienBanTaiLieu = GetGuid(
                    delivery,
                    "IdPhienBanTaiLieu");
                result.AuditActivityType =
                    DocumentActivityTypeKeys.UpdateCustomerDelivery;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.CustomerDelivery;
                result.AuditReferenceId = idGuiNhanKhachHang;
                result.AuditChanges = "Phiên bản: v"
                    + Convert.ToString(delivery["SoPhienBan"])
                    + "; Khách hàng: "
                    + Convert.ToString(delivery["TenKhachHang"])
                    + "; Trạng thái: " + safeStatus
                    + (string.IsNullOrWhiteSpace(safeNote)
                        ? string.Empty
                        : "; Ghi chú: " + safeNote);
                result.AuditDescription =
                    "Đã cập nhật trạng thái gửi khách hàng.";

                scope.Complete();
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

        public DocumentPhysicalStorageOperationResult
            StoreDocumentPhysicalCopy(
                Guid idTaiLieu,
                Guid idNoiLuuTru,
                bool nhapMaThuCong,
                string maLuuTru,
                string tinhTrangBanGoc,
                string ghiChu,
                Guid currentUserId,
                string currentUserName,
                DateTime currentDate)
        {
            if (idTaiLieu == Guid.Empty || idNoiLuuTru == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Thông tin lưu bản cứng không hợp lệ.");
            }

            string safeManualStorageCode = NormalizePhysicalStorageCode(
                maLuuTru);
            if (nhapMaThuCong)
            {
                if (string.IsNullOrWhiteSpace(safeManualStorageCode))
                {
                    throw new InvalidOperationException(
                        "Vui lòng nhập mã lưu trữ.");
                }
                if (safeManualStorageCode.Length > 100)
                {
                    throw new InvalidOperationException(
                        "Mã lưu trữ không được vượt quá 100 ký tự.");
                }
                if (!Regex.IsMatch(
                        safeManualStorageCode,
                        @"^[A-Z0-9][A-Z0-9_-]*$"))
                {
                    throw new InvalidOperationException(
                        "Mã lưu trữ chỉ được gồm chữ không dấu, số, dấu gạch ngang hoặc gạch dưới.");
                }
            }

            string safeOriginalCopyCondition =
                (tinhTrangBanGoc ?? string.Empty).Trim();
            if (safeOriginalCopyCondition.Length > 255)
            {
                throw new InvalidOperationException(
                    "Tình trạng bản gốc không được vượt quá 255 ký tự.");
            }

            string safeNote = (ghiChu ?? string.Empty).Trim();
            if (safeNote.Length > 500)
            {
                throw new InvalidOperationException(
                    "Ghi chú không được vượt quá 500 ký tự.");
            }

            if (currentUserId == Guid.Empty)
                throw new InvalidOperationException("Tài khoản hiện tại không hợp lệ.");

            string safeUserName = NormalizeUserName(currentUserName);
            Guid physicalStorageId = Guid.NewGuid();
            DocumentPhysicalStorageOperationResult result =
                new DocumentPhysicalStorageOperationResult
                {
                    IdLuuTruVatLy = physicalStorageId
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
                          AND d.DaXoa = 0
                          AND d.CanLuuVatLy = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });
                if (documentRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ cần lưu bản cứng hoặc hồ sơ chưa được cấu hình lưu bản cứng.");
                }

                EnsureActiveUser(currentUserId);

                DataTable storageLocationRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            n.IdNoiLuuTru,
                            n.MaNoiLuuTru,
                            n.TenNoiLuuTru
                        FROM TblNoiLuuTru n WITH (UPDLOCK, HOLDLOCK)
                        WHERE n.IdNoiLuuTru = @StorageLocationId
                          AND n.DaXoa = 0
                          AND n.KichHoat = 1;",
                    new Dictionary<string, object>
                    {
                        { "@StorageLocationId", idNoiLuuTru }
                    });
                if (storageLocationRows.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Nơi lưu trữ không tồn tại hoặc đã ngừng hoạt động.");
                }

                DataTable currentStorageRows = ExecuteDataTable(
                    @"
                        SELECT TOP 1
                            v.IdLuuTruVatLy,
                            v.MaLuuTru
                        FROM TblLuuTruVatLy v WITH (UPDLOCK, HOLDLOCK)
                        WHERE v.IdTaiLieu = @DocumentId
                          AND v.DaXoa = 0
                          AND v.LaViTriHienTai = 1
                        ORDER BY v.NgayLuu DESC, v.IdLuuTruVatLy DESC;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu }
                    });

                bool hasCurrentStorage = currentStorageRows.Rows.Count > 0;
                string storageCode;
                if (hasCurrentStorage)
                {
                    storageCode = Convert.ToString(
                        currentStorageRows.Rows[0]["MaLuuTru"]);
                    if (string.IsNullOrWhiteSpace(storageCode))
                    {
                        throw new InvalidOperationException(
                            "Bản ghi lưu trữ hiện tại chưa có mã lưu trữ. Vui lòng kiểm tra lại dữ liệu cũ.");
                    }

                    if (nhapMaThuCong
                        && !string.Equals(
                            storageCode,
                            safeManualStorageCode,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Hồ sơ đã được cấp mã lưu trữ. Khi cập nhật vị trí, mã lưu trữ phải được giữ nguyên.");
                    }
                }
                else if (nhapMaThuCong)
                {
                    EnsurePhysicalStorageCodeIsAvailable(
                        safeManualStorageCode);
                    storageCode = safeManualStorageCode;
                }
                else
                {
                    storageCode = GetNextPhysicalStorageCode(currentDate);
                }

                ExecuteNonQuery(
                    @"
                        UPDATE TblLuuTruVatLy
                        SET LaViTriHienTai = 0,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND DaXoa = 0
                          AND LaViTriHienTai = 1;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                ExecuteNonQuery(
                    @"
                        INSERT INTO TblLuuTruVatLy
                        (
                            IdLuuTruVatLy,
                            IdTaiLieu,
                            IdNoiLuuTru,
                            IdNguoiThucHien,
                            MaLuuTru,
                            TrangThaiLuuTru,
                            TinhTrangBanGoc,
                            NgayLuu,
                            NgayLayRa,
                            NgayHoanTra,
                            LaViTriHienTai,
                            GhiChu,
                            DaXoa,
                            NguoiTao,
                            NgayTao,
                            NguoiCapNhat,
                            NgayCapNhat
                        )
                        VALUES
                        (
                            @PhysicalStorageId,
                            @DocumentId,
                            @StorageLocationId,
                            @CurrentUserId,
                            @StorageCode,
                            @StoredStatus,
                            NULLIF(@OriginalCopyCondition, ''),
                            @CurrentDate,
                            NULL,
                            NULL,
                            1,
                            NULLIF(@Note, ''),
                            0,
                            @CurrentUserName,
                            @CurrentDate,
                            @CurrentUserName,
                            @CurrentDate
                        );",
                    new Dictionary<string, object>
                    {
                        { "@PhysicalStorageId", physicalStorageId },
                        { "@DocumentId", idTaiLieu },
                        { "@StorageLocationId", idNoiLuuTru },
                        { "@CurrentUserId", currentUserId },
                        { "@StorageCode", storageCode },
                        {
                            "@StoredStatus",
                            DocumentPhysicalStorageStatusKeys.Stored
                        },
                        {
                            "@OriginalCopyCondition",
                            safeOriginalCopyCondition
                        },
                        { "@Note", safeNote },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                ExecuteNonQuery(
                    @"
                        UPDATE TblTaiLieu
                        SET TrangThaiLuuTru = @StoredStatus,
                            NguoiCapNhat = @CurrentUserName,
                            NgayCapNhat = @CurrentDate
                        WHERE IdTaiLieu = @DocumentId
                          AND DaXoa = 0;",
                    new Dictionary<string, object>
                    {
                        { "@DocumentId", idTaiLieu },
                        {
                            "@StoredStatus",
                            DocumentPhysicalStorageStatusKeys.Stored
                        },
                        { "@CurrentUserName", safeUserName },
                        { "@CurrentDate", currentDate }
                    });

                DataRow storageLocation = storageLocationRows.Rows[0];
                string locationCode = Convert.ToString(
                    storageLocation["MaNoiLuuTru"]);
                string locationName = Convert.ToString(
                    storageLocation["TenNoiLuuTru"]);
                result.MaLuuTru = storageCode;
                result.AuditActivityType =
                    DocumentActivityTypeKeys.StorePhysicalCopy;
                result.AuditReferenceType =
                    DocumentActivityReferenceKeys.PhysicalStorage;
                result.AuditReferenceId = physicalStorageId;
                result.AuditChanges = "Mã lưu trữ: " + storageCode
                    + "; Nơi lưu trữ: " + locationCode
                    + (string.IsNullOrWhiteSpace(locationName)
                        ? string.Empty
                        : " - " + locationName)
                    + (string.IsNullOrWhiteSpace(safeOriginalCopyCondition)
                        ? string.Empty
                        : "; Tình trạng bản gốc: "
                            + safeOriginalCopyCondition)
                    + (string.IsNullOrWhiteSpace(safeNote)
                        ? string.Empty
                        : "; Ghi chú: " + safeNote);
                result.AuditDescription = hasCurrentStorage
                    ? "Đã cập nhật vị trí lưu bản cứng."
                    : "Đã ghi nhận lưu bản cứng.";

                scope.Complete();
            }

            return result;
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
            DataTable history = CreateDocumentActivityHistoryTable();
            if (idTaiLieu == Guid.Empty)
                return history;

            try
            {
                DateTime earliestAuditDate = new DateTime(
                    1753,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);
                List<AuditLogDto> auditLogs = GetDocumentAuditHistoryAsync(
                        idTaiLieu,
                        earliestAuditDate)
                    .GetAwaiter()
                    .GetResult();

                foreach (AuditLogDto auditLog in auditLogs)
                    AddDocumentAuditHistoryRow(history, auditLog);
            }
            catch (Exception exception)
            {
                SysLogger.LogError(
                    exception,
                    "Failed to load AuditLog history for document "
                    + idTaiLieu);
            }

            return history;
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

        // Document-only business events such as versioning and signing use
        // this helper. Basic TblTaiLieu CRUD is still logged by Insert,
        // Update and Delete below.
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
            // Capture this operation's actor, not the AuditManager cached by a singleton.
            var currentActor = new ClientInfo
            {
                UserId = actor != null
                    && actor.UserId.HasValue
                    && actor.UserId.Value != Guid.Empty
                        ? actor.UserId
                        : (Guid?)null,
                UserName = actor == null || string.IsNullOrWhiteSpace(actor.UserName)
                    ? "[System]"
                    : actor.UserName.Trim(),
                IpAddress = actor == null ? string.Empty : actor.IpAddress,
                UserAgent = actor == null ? string.Empty : actor.UserAgent
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

        private static DataTable CreateDocumentActivityHistoryTable()
        {
            DataTable history = new DataTable();
            history.Columns.Add("IdLichSuTaiLieu", typeof(Guid));
            history.Columns.Add("LoaiHanhDong", typeof(string));
            history.Columns.Add("LoaiThamChieu", typeof(string));
            history.Columns.Add("NoiDungThayDoi", typeof(string));
            history.Columns.Add("MoTa", typeof(string));
            history.Columns.Add("NguoiTao", typeof(string));
            history.Columns.Add("NgayTao", typeof(DateTime));
            history.Columns.Add("TenNguoiThucHien", typeof(string));
            return history;
        }

        private static void AddDocumentAuditHistoryRow(
            DataTable history,
            AuditLogDto auditLog)
        {
            if (history == null || auditLog == null)
                return;

            JObject auditChanges = ParseAuditChanges(auditLog.Changes);
            string activityType = GetAuditText(
                auditChanges,
                "LoaiHanhDong");
            string referenceType = GetAuditText(
                auditChanges,
                "LoaiThamChieu");
            string changes = GetAuditText(
                auditChanges,
                "NoiDungThayDoi");
            string description = GetAuditText(auditChanges, "MoTa");

            if (string.IsNullOrWhiteSpace(activityType))
            {
                activityType = GetDocumentActivityType(auditLog.ActionType);
                referenceType = DocumentActivityReferenceKeys.Document;
                description = GetGenericDocumentAuditDescription(
                    auditLog.ActionType,
                    auditLog.Title);
                changes = BuildGenericDocumentAuditChanges(
                    auditChanges,
                    auditLog.ActionType);
            }

            DataRow row = history.NewRow();
            row["IdLichSuTaiLieu"] = auditLog.Id;
            row["LoaiHanhDong"] = activityType ?? string.Empty;
            row["LoaiThamChieu"] = referenceType ?? string.Empty;
            row["NoiDungThayDoi"] = changes ?? string.Empty;
            row["MoTa"] = description ?? string.Empty;
            row["NguoiTao"] = auditLog.ChangedBy ?? string.Empty;
            row["NgayTao"] = auditLog.ChangedAt;
            row["TenNguoiThucHien"] = string.Empty;
            history.Rows.Add(row);
        }

        private static JObject ParseAuditChanges(string changes)
        {
            if (string.IsNullOrWhiteSpace(changes))
                return null;

            try
            {
                return JObject.Parse(changes);
            }
            catch
            {
                return null;
            }
        }

        private static string GetAuditText(JObject changes, string key)
        {
            if (changes == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            JToken value = changes[key];
            if (value == null || value.Type == JTokenType.Null)
                return string.Empty;

            return System.Web.HttpUtility.HtmlDecode(
                Convert.ToString(value));
        }

        private static string GetDocumentActivityType(string actionType)
        {
            if (string.Equals(
                    actionType,
                    LogActions.Actions.CREATE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentActivityTypeKeys.CreateDocument;
            }
            if (string.Equals(
                    actionType,
                    LogActions.Actions.DELETE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentActivityTypeKeys.DeleteDocument;
            }
            if (string.Equals(
                    actionType,
                    LogActions.Actions.UPDATE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentActivityTypeKeys.UpdateDocument;
            }

            return actionType ?? string.Empty;
        }

        private static string GetGenericDocumentAuditDescription(
            string actionType,
            string title)
        {
            if (string.Equals(
                    actionType,
                    LogActions.Actions.CREATE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Đã tạo hồ sơ.";
            }
            if (string.Equals(
                    actionType,
                    LogActions.Actions.DELETE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Đã xóa hồ sơ.";
            }
            if (string.Equals(
                    actionType,
                    LogActions.Actions.UPDATE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Đã cập nhật thông tin hồ sơ.";
            }

            return string.IsNullOrWhiteSpace(title)
                ? "Đã cập nhật nhật ký hồ sơ."
                : title;
        }

        private static string BuildGenericDocumentAuditChanges(
            JObject auditChanges,
            string actionType)
        {
            if (auditChanges == null)
                return string.Empty;

            if (string.Equals(
                    actionType,
                    LogActions.Actions.CREATE.ToString(),
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    actionType,
                    LogActions.Actions.DELETE.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                List<string> details = new List<string>();
                AddGenericAuditDetail(
                    details,
                    "Mã hồ sơ",
                    GetAuditText(auditChanges, "MaTaiLieu"));
                AddGenericAuditDetail(
                    details,
                    "Tên hồ sơ",
                    GetAuditText(auditChanges, "TenTaiLieu"));
                return string.Join("; ", details);
            }

            List<string> changes = new List<string>();
            foreach (JProperty property in auditChanges.Properties())
            {
                JObject change = property.Value as JObject;
                if (change == null)
                    continue;

                string oldValue = GetAuditTokenText(change["OldValue"]);
                string newValue = GetAuditTokenText(change["NewValue"]);
                AddGenericDocumentChange(
                    changes,
                    property.Name,
                    oldValue,
                    newValue);
            }

            return string.Join("; ", changes);
        }

        private static void AddGenericAuditDetail(
            ICollection<string> details,
            string label,
            string value)
        {
            if (details == null || string.IsNullOrWhiteSpace(value))
                return;

            details.Add(label + ": " + FormatAuditValue(value));
        }

        private static void AddGenericDocumentChange(
            ICollection<string> changes,
            string propertyName,
            string oldValue,
            string newValue)
        {
            if (changes == null || string.IsNullOrWhiteSpace(propertyName))
                return;

            if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
                return;

            if (propertyName == "NoiDungHtml")
            {
                changes.Add("Nội dung hồ sơ đã thay đổi.");
                return;
            }

            if (string.Equals(
                    propertyName,
                    "NguoiCapNhat",
                    StringComparison.Ordinal)
                || string.Equals(
                    propertyName,
                    "NgayCapNhat",
                    StringComparison.Ordinal)
                || string.Equals(
                    propertyName,
                    "DaXoa",
                    StringComparison.Ordinal))
            {
                return;
            }

            if (string.Equals(
                    propertyName,
                    "IdFileBanChinhThuc",
                    StringComparison.Ordinal))
            {
                changes.Add("File chính thức đã thay đổi.");
                return;
            }

            if (string.Equals(
                    propertyName,
                    "IdLoaiTaiLieu",
                    StringComparison.Ordinal))
            {
                changes.Add("Loại tài liệu đã thay đổi.");
                return;
            }

            if (string.Equals(
                    propertyName,
                    "IdNhanVienPhuTrach",
                    StringComparison.Ordinal))
            {
                changes.Add("Người phụ trách đã thay đổi.");
                return;
            }

            if (string.Equals(
                    propertyName,
                    "TrangThaiTaiLieu",
                    StringComparison.Ordinal)
                || string.Equals(
                    propertyName,
                    "TrangThaiGuiKhach",
                    StringComparison.Ordinal)
                || string.Equals(
                    propertyName,
                    "TrangThaiLuuTru",
                    StringComparison.Ordinal))
            {
                changes.Add("Trạng thái hồ sơ đã thay đổi.");
                return;
            }

            string label = GetDocumentAuditFieldLabel(propertyName);
            if (string.IsNullOrWhiteSpace(label))
                return;

            changes.Add(
                label
                + ": “"
                + FormatAuditValue(oldValue)
                + "” → “"
                + FormatAuditValue(newValue)
                + "”");
        }

        private static string GetDocumentAuditFieldLabel(string propertyName)
        {
            switch (propertyName)
            {
                case "MaTaiLieu":
                    return "Mã hồ sơ";
                case "TenTaiLieu":
                    return "Tên hồ sơ";
                case "MoTa":
                    return "Mô tả";
                case "CanTrinhKy":
                    return "Cần trình ký";
                case "HinhThucKy":
                    return "Hình thức ký";
                case "CanGuiKhachHang":
                    return "Cần gửi khách hàng";
                case "CanLuuVatLy":
                    return "Cần lưu bản cứng";
                default:
                    return string.Empty;
            }
        }

        private static string GetAuditTokenText(JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
                return string.Empty;

            return System.Web.HttpUtility.HtmlDecode(Convert.ToString(value));
        }

        private static string FormatAuditValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Không có";
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                return "Có";
            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                return "Không";

            string normalized = value.Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            return normalized.Length <= 500
                ? normalized
                : normalized.Substring(0, 497) + "...";
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

        private static string NormalizeUserName(string userName)
        {
            string value = string.IsNullOrWhiteSpace(userName)
                ? "[System]"
                : userName.Trim();
            return value.Length <= 150 ? value : value.Substring(0, 150);
        }

        private static bool IsCustomerDeliveryChannel(string value)
        {
            return value == DocumentCustomerDeliveryChannelKeys.Email
                || value == DocumentCustomerDeliveryChannelKeys.Direct
                || value == DocumentCustomerDeliveryChannelKeys.Other;
        }

        private static bool IsCustomerDeliveryStatus(string value)
        {
            return value == DocumentCustomerStatusKeys.Sent
                || value == DocumentCustomerStatusKeys.WaitingForReturn
                || value == DocumentCustomerStatusKeys.ReceivedBack;
        }

        private static string NormalizePhysicalStorageCode(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static void EnsurePhysicalStorageCodeIsAvailable(
            string storageCode)
        {
            int recordCount = ExecuteScalarInt(
                @"
                    SELECT COUNT(1)
                    FROM TblLuuTruVatLy v WITH (UPDLOCK, HOLDLOCK)
                    WHERE v.DaXoa = 0
                      AND UPPER(LTRIM(RTRIM(v.MaLuuTru))) = @StorageCode;",
                new Dictionary<string, object>
                {
                    { "@StorageCode", storageCode }
                });
            if (recordCount > 0)
            {
                throw new InvalidOperationException(
                    "Mã lưu trữ đã được sử dụng. Vui lòng nhập mã khác.");
            }
        }

        private static string GetNextPhysicalStorageCode(
            DateTime currentDate)
        {
            string prefix = "LT-" + currentDate.ToString(
                "yyyy",
                CultureInfo.InvariantCulture) + "-";

            // UPDLOCK + HOLDLOCK runs inside the surrounding TransactionScope.
            // It serializes generation for one year, so two concurrent users do
            // not receive the same next number through this application flow.
            int lastSequence = ExecuteScalarInt(
                @"
                    SELECT ISNULL(
                        MAX(
                            TRY_CONVERT(
                                INT,
                                SUBSTRING(
                                    v.MaLuuTru,
                                    LEN(@StorageCodePrefix) + 1,
                                    100)
                            )
                        ),
                        0
                    )
                    FROM TblLuuTruVatLy v WITH (UPDLOCK, HOLDLOCK)
                    WHERE v.DaXoa = 0
                      AND v.MaLuuTru LIKE @StorageCodePrefix + '%'
                      AND SUBSTRING(
                            v.MaLuuTru,
                            LEN(@StorageCodePrefix) + 1,
                            100
                          ) NOT LIKE '%[^0-9]%';",
                new Dictionary<string, object>
                {
                    { "@StorageCodePrefix", prefix }
                });

            if (lastSequence == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Không thể tự sinh thêm mã lưu trữ cho năm hiện tại.");
            }

            return prefix + (lastSequence + 1).ToString(
                "D6",
                CultureInfo.InvariantCulture);
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

        private static string GetFileDisplayName(DataRow row)
        {
            string originalName = Convert.ToString(row["OriginalFileName"]);
            if (!string.IsNullOrWhiteSpace(originalName))
                return originalName;
            return Convert.ToString(row["Name"]);
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

        public string GetDocumentContent(Guid documentId)
        {
            var rows = ExecuteDataTable(
                "SELECT NoiDungHtml,MoTa FROM dbo.TblTaiLieu WHERE IdTaiLieu=@Id AND DaXoa=0;",
                new Dictionary<string, object> { { "@Id", documentId } });
            if (rows.Rows.Count == 0) return string.Empty;
            var row = rows.Rows[0];
            // Only NULL falls back to legacy description. An explicitly cleared editor stays empty.
            if (row["NoiDungHtml"] != DBNull.Value) return Convert.ToString(row["NoiDungHtml"]);
            return System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["MoTa"]))
                .Replace("\r\n", "\n").Replace("\n", "<br />");
        }

        // null means an old caller did not edit content; empty string explicitly clears it.
        public TblTaiLieu SaveWithContent(TblTaiLieu item, bool isNew, string content)
        {
            var old = isNew ? null : GetById(item.IdTaiLieu);
            string before = content == null ? null : GetDocumentContent(item.IdTaiLieu);
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 2, 0))) {
                item.Save();
                if (content != null)
                    ExecuteNonQuery("UPDATE dbo.TblTaiLieu SET NoiDungHtml=@Content WHERE IdTaiLieu=@Id;",
                        new Dictionary<string, object> { { "@Id", item.IdTaiLieu }, { "@Content", content } });
                scope.Complete();
            }
            if (isNew) LogCreate(item); else LogUpdate(old, item);
            if (content != null && before != content)
                ExecuteAuditSafely(() => _auditManager.LogChangesAsync(
                    new { NoiDungHtml = System.Web.HttpUtility.HtmlEncode(before) },
                    new { NoiDungHtml = System.Web.HttpUtility.HtmlEncode(content) }, _tableName,
                    item.IdTaiLieu, item.NguoiCapNhat ?? item.NguoiTao ?? string.Empty),
                    "Failed to log document content change");
            return item;
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
            if (item == null)
                return;

            ExecuteAuditSafely(
                () => _auditManager.LogActionAsync(
                    LogActions.Actions.CREATE,
                    item,
                    _tableName,
                    item.IdTaiLieu,
                    item.NguoiTao ?? string.Empty),
                "Failed to log CREATE action for TblTaiLieu");
        }

        private void LogUpdate(TblTaiLieu itemOld, TblTaiLieu itemNew)
        {
            if (itemOld == null || itemNew == null)
                return;

            ExecuteAuditSafely(
                () => _auditManager.LogChangesAsync(
                    itemOld,
                    itemNew,
                    _tableName,
                    itemNew.IdTaiLieu,
                    itemNew.NguoiCapNhat ?? string.Empty),
                "Failed to log changes for TblTaiLieu");
        }

        private void LogDelete(TblTaiLieu item)
        {
            if (item == null)
                return;

            ExecuteAuditSafely(
                () => _auditManager.LogActionAsync(
                    LogActions.Actions.DELETE,
                    item,
                    _tableName,
                    item.IdTaiLieu,
                    item.NguoiCapNhat ?? item.NguoiTao ?? string.Empty),
                "Failed to log DELETE action for TblTaiLieu");
        }

        private static void ExecuteAuditSafely(
            Func<Task> auditOperation,
            string errorMessage)
        {
            try
            {
                if (auditOperation != null)
                    auditOperation().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                SysLogger.LogError(exception, errorMessage);
            }
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
