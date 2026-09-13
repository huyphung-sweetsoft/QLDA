using SubSonic;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace SweetSoft.QLDA.Core.Managers
{
    public static class DocumentStatusKeys
    {
        public const string Drafting = "DANG_SOAN_THAO";
        public const string PendingSignature = "DANG_TRINH_KY";
        public const string ChangesRequested = "YEU_CAU_DIEU_CHINH";
        public const string Signed = "DA_KY";
        public const string Completed = "HOAN_TAT";
    }

    public static class DocumentSigningStatusKeys
    {
        public const string Pending = "DANG_TRINH";
        public const string ChangesRequested = "YEU_CAU_DIEU_CHINH";
        public const string Signed = "DA_KY";
    }

    public static class DocumentCustomerStatusKeys
    {
        public const string NotSent = "CHUA_GUI";
        public const string Sent = "DA_GUI";
        public const string WaitingForReturn = "CHO_NHAN_LAI";
        public const string ReceivedBack = "DA_NHAN_LAI";
    }

    public static class DocumentCustomerDeliveryChannelKeys
    {
        public const string Email = "EMAIL";
        public const string Direct = "TRUC_TIEP";
        public const string Other = "KHAC";
    }

    public static class DocumentPhysicalStorageStatusKeys
    {
        public const string NotStored = "CHUA_LUU";
        public const string Stored = "DA_LUU";
        public const string CheckedOut = "DANG_LAY_RA";
    }

    public static class DocumentScopeKeys
    {
        public const string All = DocumentRepository.DocumentScopeAll;
        public const string Company = DocumentRepository.DocumentScopeCompany;
        public const string Project = DocumentRepository.DocumentScopeProject;
    }

    public static class DocumentActivityTypeKeys
    {
        public const string CreateDocument = "TAO_HO_SO";
        public const string UpdateDocument = "CAP_NHAT_HO_SO";
        public const string DeleteDocument = "XOA_HO_SO";
        public const string CreateFromTemplate = "TAO_TU_MAU";
        public const string UploadVersion = "TAI_LEN_PHIEN_BAN";
        public const string DeleteVersion = "XOA_PHIEN_BAN";
        public const string SetOfficialFile = "CHON_FILE_CHINH_THUC";
        public const string ClearOfficialFile = "BO_CHON_FILE_CHINH_THUC";
        public const string SubmitSigning = "TRINH_KY";
        public const string RequestSigningChanges = "YEU_CAU_DIEU_CHINH_TRINH_KY";
        public const string CompleteSigning = "HOAN_TAT_TRINH_KY";
        public const string SendCustomerDelivery = "GUI_KHACH_HANG";
        public const string UpdateCustomerDelivery = "CAP_NHAT_GUI_KHACH_HANG";
        public const string StorePhysicalCopy = "LUU_BAN_CUNG";
    }

    public static class DocumentActivityReferenceKeys
    {
        public const string Document = "TblTaiLieu";
        public const string DocumentVersion = "TblPhienBanTaiLieu";
        public const string Signing = "TblTrinhKyTaiLieu";
        public const string CustomerDelivery = "TblGuiNhanKhachHang";
        public const string PhysicalStorage = "TblLuuTruVatLy";
    }

    public class DocumentManager : BaseManager
    {
        private static readonly Lazy<DocumentManager> _instance =
            new Lazy<DocumentManager>(() => new DocumentManager());

        private readonly DocumentRepository _repository;
        private readonly DocumentTypeRepository _documentTypeRepository;
        private readonly DocumentTemplateRepository _documentTemplateRepository;

        public static DocumentManager Instance
        {
            get { return _instance.Value; }
        }

        public DocumentManager(IAppContext applicationContext = null)
            : base(applicationContext)
        {
            AuditManager auditManager = new AuditManager(GetClientInfo());
            _repository = new DocumentRepository(auditManager);
            _documentTypeRepository = new DocumentTypeRepository(auditManager);
            _documentTemplateRepository =
                new DocumentTemplateRepository(auditManager);
        }

        public DataTable SearchCompanyDocuments(
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchCompanyDocuments(
                searchTerm,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public DataTable SearchDocuments(
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchDocuments(
                searchTerm,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public DataTable SearchDocuments(
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchDocuments(
                string.Empty,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        public DataTable SearchCompanyDocuments(
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchPaging(
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        /// <summary>
        /// Returns documents that belong to exactly one project.  The scope and
        /// project filter are imposed here instead of trusting values posted by
        /// the list control.
        /// </summary>
        public DataTable SearchProjectDocuments(
            Guid projectId,
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            RequireProjectDocumentAccess(projectId, ActionKeys.View);

            Dictionary<string, object> projectParameters = parameters == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(parameters);
            projectParameters[DocumentRepository.DocumentScopeParameter] =
                DocumentScopeKeys.Project;
            projectParameters[TblTaiLieu.Columns.IdDuAn] =
                projectId.ToString();

            return _repository.SearchDocuments(
                searchTerm,
                projectParameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        /// <summary>
        /// A project document is available only to an administrator, the
        /// project manager, or an active member of that project who has the
        /// requested permission of the ProjectDocument module.
        /// </summary>
        public bool CanAccessProjectDocument(
            Guid projectId,
            ActionKeys action)
        {
            Guid userId = SweetContext.Current.UserId;
            if (userId == Guid.Empty || projectId == Guid.Empty)
                return false;

            TblDuAn project = DuAnManager.Instance.GetDuAnById(projectId);
            if (project == null)
                return false;

            if (!FunctionManager.Instance.IsActionKeyExisted(
                    userId,
                    ModuleKeys.ProjectDocument,
                    action))
            {
                return false;
            }

            if (UserManager.Instance.IsAdministrator(userId)
                || project.IdNhanVienQuanLy == userId)
            {
                return true;
            }

            return new Select()
                .From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(projectId)
                .And(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(userId)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        private void RequireProjectDocumentAccess(
            Guid projectId,
            ActionKeys action)
        {
            if (!CanAccessProjectDocument(projectId, action))
                throw new UnauthorizedAccessException();
        }

        public TblTaiLieu GetCompanyDocumentById(Guid idTaiLieu)
        {
            return _repository.GetCompanyById(idTaiLieu);
        }

        public TblTaiLieu GetProjectDocumentById(
            Guid idTaiLieu,
            Guid projectId)
        {
            RequireProjectDocumentAccess(projectId, ActionKeys.View);
            return _repository.GetProjectById(idTaiLieu, projectId);
        }

        public List<AspnetUser> GetAvailableEmployees()
        {
            return _repository.GetAvailableEmployees();
        }

        public List<AspnetUser> GetAvailableSigningUsers()
        {
            return _repository.GetAvailableSigningUsers();
        }

        public List<TblDuAn> GetAvailableProjects()
        {
            return _repository.GetAvailableProjects();
        }

        public TblLoaiTaiLieu GetDocumentTypeDefaults(Guid idLoaiTaiLieu)
        {
            if (idLoaiTaiLieu == Guid.Empty)
                return null;

            return _documentTypeRepository.GetById(idLoaiTaiLieu);
        }

        public DataTable GetDocumentVersions(Guid idTaiLieu)
        {
            return _repository.GetDocumentVersionsWithFiles(idTaiLieu);
        }

        public DataTable GetCompanyDocumentDetail(Guid idTaiLieu)
        {
            return _repository.GetCompanyDocumentDetail(idTaiLieu);
        }

        public DataTable GetProjectDocumentDetail(
            Guid idTaiLieu,
            Guid projectId)
        {
            RequireProjectDocumentAccess(projectId, ActionKeys.View);
            return _repository.GetProjectDocumentDetail(
                idTaiLieu,
                projectId);
        }

        /// <summary>
        /// Used by the project detail control before it executes a shared
        /// version or signing operation.  Shared tables are still keyed by the
        /// document id, so this prevents a forged id from crossing projects.
        /// </summary>
        public void EnsureProjectDocumentAccess(
            Guid idTaiLieu,
            Guid projectId,
            ActionKeys action)
        {
            RequireProjectDocumentAccess(projectId, action);
            if (_repository.GetProjectById(idTaiLieu, projectId) == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy hồ sơ thuộc dự án hiện tại.");
            }
        }

        public DataTable GetSigningHistory(Guid idTaiLieu)
        {
            return _repository.GetSigningHistory(idTaiLieu);
        }

        public DataTable GetSigningDetail(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu)
        {
            return _repository.GetSigningDetail(
                idTaiLieu,
                idTrinhKyTaiLieu);
        }

        public DocumentSigningOperationResult SubmitDocumentSigning(
            Guid idTaiLieu,
            Guid idNguoiKy,
            string ghiChu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException(
                    "Không tìm thấy hồ sơ.");
            if (!document.CanTrinhKy)
                throw new InvalidOperationException(
                    "Hồ sơ này không được cấu hình trình ký.");

            DocumentSigningOperationResult result =
                _repository.SubmitDocumentSigning(
                    idTaiLieu,
                    idNguoiKy,
                    string.Empty,
                    document.HinhThucKy,
                    ghiChu,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WriteSigningAudit(idTaiLieu, result);
            return result;
        }

        public void RequestDocumentSigningChanges(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu,
            string reason)
        {
            DocumentSigningOperationResult result =
                _repository.RequestDocumentSigningChanges(
                    idTaiLieu,
                    idTrinhKyTaiLieu,
                    reason,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WriteSigningAudit(idTaiLieu, result);
        }

        public DocumentSigningOperationResult CompleteDocumentSigning(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu,
            string note)
        {
            DocumentSigningOperationResult result =
                _repository.CompleteDocumentSigning(
                    idTaiLieu,
                    idTrinhKyTaiLieu,
                    note,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WriteSigningAudit(idTaiLieu, result);
            return result;
        }

        public DataTable GetCustomerDeliveryHistory(Guid idTaiLieu)
        {
            return _repository.GetCustomerDeliveryHistory(idTaiLieu);
        }

        public DataTable GetCustomerDeliveryDetail(
            Guid idTaiLieu,
            Guid idGuiNhanKhachHang)
        {
            return _repository.GetCustomerDeliveryDetail(
                idTaiLieu,
                idGuiNhanKhachHang);
        }

        public DocumentCustomerDeliveryOperationResult SendDocumentToCustomer(
            Guid idTaiLieu,
            Guid idPhienBanTaiLieu,
            Guid idKhachHang,
            string tenNguoiNhan,
            string emailNguoiNhan,
            string kenhGui,
            DateTime? hanPhanHoi,
            bool choPhepGuiTruocKhiKy,
            string ghiChu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");
            if (!document.CanGuiKhachHang)
            {
                throw new InvalidOperationException(
                    "Hồ sơ này không được cấu hình gửi khách hàng.");
            }

            DocumentCustomerDeliveryOperationResult result =
                _repository.SendDocumentToCustomer(
                    idTaiLieu,
                    idPhienBanTaiLieu,
                    idKhachHang,
                    tenNguoiNhan,
                    emailNguoiNhan,
                    kenhGui,
                    hanPhanHoi,
                    choPhepGuiTruocKhiKy,
                    ghiChu,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WriteCustomerDeliveryAudit(idTaiLieu, result);
            return result;
        }

        public DocumentCustomerDeliveryOperationResult
            UpdateCustomerDeliveryStatus(
                Guid idTaiLieu,
                Guid idGuiNhanKhachHang,
                string trangThai,
                string ghiChu)
        {
            DocumentCustomerDeliveryOperationResult result =
                _repository.UpdateCustomerDeliveryStatus(
                    idTaiLieu,
                    idGuiNhanKhachHang,
                    trangThai,
                    ghiChu,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WriteCustomerDeliveryAudit(idTaiLieu, result);
            return result;
        }

        public DataTable GetPhysicalStorageHistory(Guid idTaiLieu)
        {
            return _repository.GetPhysicalStorageHistory(idTaiLieu);
        }

        public DocumentPhysicalStorageOperationResult
            StoreDocumentPhysicalCopy(
                Guid idTaiLieu,
                Guid idNoiLuuTru,
                bool nhapMaThuCong,
                string maLuuTru,
                string tinhTrangBanGoc,
                string ghiChu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");
            if (!document.CanLuuVatLy)
            {
                throw new InvalidOperationException(
                    "Hồ sơ này không được cấu hình lưu bản cứng.");
            }

            DocumentPhysicalStorageOperationResult result =
                _repository.StoreDocumentPhysicalCopy(
                    idTaiLieu,
                    idNoiLuuTru,
                    nhapMaThuCong,
                    maLuuTru,
                    tinhTrangBanGoc,
                    ghiChu,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    DateTime.UtcNow);
            WritePhysicalStorageAudit(idTaiLieu, result);
            return result;
        }

        public DataTable GetDocumentActivityHistory(Guid idTaiLieu)
        {
            return _repository.GetDocumentActivityHistory(idTaiLieu);
        }

        public TblTaiLieu SaveCompanyDocument(
            Guid idTaiLieu,
            Guid idLoaiTaiLieu,
            Guid? idNhanVienPhuTrach,
            string maTaiLieu,
            string tenTaiLieu,
            string moTa,
            bool canTrinhKy,
            string hinhThucKy,
            bool canGuiKhachHang,
            bool canLuuVatLy)
        {
            return SaveDocument(
                null,
                string.Empty,
                idTaiLieu,
                idLoaiTaiLieu,
                idNhanVienPhuTrach,
                maTaiLieu,
                tenTaiLieu,
                moTa,
                canTrinhKy,
                hinhThucKy,
                canGuiKhachHang,
                canLuuVatLy);
        }

        public TblTaiLieu SaveProjectDocument(
            Guid projectId,
            Guid idTaiLieu,
            Guid idLoaiTaiLieu,
            Guid? idNhanVienPhuTrach,
            string maTaiLieu,
            string tenTaiLieu,
            string moTa,
            bool canTrinhKy,
            string hinhThucKy,
            bool canGuiKhachHang,
            bool canLuuVatLy)
        {
            RequireProjectDocumentAccess(
                projectId,
                idTaiLieu == Guid.Empty
                    ? ActionKeys.Create
                    : ActionKeys.Update);

            TblDuAn project = DuAnManager.Instance.GetDuAnById(projectId);
            if (project == null)
            {
                throw new InvalidOperationException(
                    "Dự án không tồn tại hoặc đã bị xóa.");
            }

            return SaveDocument(
                projectId,
                project.MaDuAn,
                idTaiLieu,
                idLoaiTaiLieu,
                idNhanVienPhuTrach,
                maTaiLieu,
                tenTaiLieu,
                moTa,
                canTrinhKy,
                hinhThucKy,
                canGuiKhachHang,
                canLuuVatLy);
        }

        private TblTaiLieu SaveDocument(
            Guid? projectId,
            string projectCode,
            Guid idTaiLieu,
            Guid idLoaiTaiLieu,
            Guid? idNhanVienPhuTrach,
            string maTaiLieu,
            string tenTaiLieu,
            string moTa,
            bool canTrinhKy,
            string hinhThucKy,
            bool canGuiKhachHang,
            bool canLuuVatLy)
        {
            maTaiLieu = (maTaiLieu ?? string.Empty).Trim().ToUpperInvariant();
            tenTaiLieu = (tenTaiLieu ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();
            hinhThucKy = (hinhThucKy ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            bool isProjectDocument = projectId.HasValue;
            string documentScopeText = isProjectDocument
                ? "hồ sơ dự án"
                : "hồ sơ công ty";

            if (idLoaiTaiLieu == Guid.Empty)
                throw new ArgumentException("Vui lòng chọn loại tài liệu.");
            if (string.IsNullOrEmpty(tenTaiLieu))
                throw new ArgumentException("Tên hồ sơ không được để trống.");
            if (tenTaiLieu.Length > 255)
                throw new ArgumentException("Tên hồ sơ không được vượt quá 255 ký tự.");
            if (maTaiLieu.Length > 100)
                throw new ArgumentException("Mã hồ sơ không được vượt quá 100 ký tự.");
            if (moTa.Length > 1000)
                throw new ArgumentException("Mô tả không được vượt quá 1.000 ký tự.");
            if (canTrinhKy
                && hinhThucKy != DocumentSigningMethodKeys.Paper
                && hinhThucKy != DocumentSigningMethodKeys.DigitalExternal)
            {
                throw new ArgumentException(
                    "Vui lòng chọn hình thức ký hợp lệ.");
            }

            TblTaiLieu item = null;
            if (idTaiLieu != Guid.Empty)
            {
                item = isProjectDocument
                    ? _repository.GetProjectById(
                        idTaiLieu,
                        projectId.Value)
                    : _repository.GetCompanyById(idTaiLieu);
                if (item == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy " + documentScopeText + ".");
                }

                if (string.IsNullOrEmpty(maTaiLieu))
                    throw new ArgumentException("Mã hồ sơ không được để trống.");
            }
            else if (string.IsNullOrEmpty(maTaiLieu))
            {
                maTaiLieu = isProjectDocument
                    ? GenerateProjectDocumentCode(projectCode)
                    : GenerateCompanyDocumentCode();
            }

            TblLoaiTaiLieu documentType =
                _documentTypeRepository.GetById(idLoaiTaiLieu);
            if (documentType == null)
            {
                throw new InvalidOperationException(
                    "Loại tài liệu không tồn tại hoặc đã bị xóa.");
            }

            bool isChangingToInactiveType = !documentType.KichHoat
                && (item == null
                    || item.IdLoaiTaiLieu != documentType.IdLoaiTaiLieu);
            if (isChangingToInactiveType)
            {
                throw new InvalidOperationException(
                    "Không thể chọn loại tài liệu đang bị khóa.");
            }

            if (idNhanVienPhuTrach.HasValue
                && _repository.GetEmployeeById(
                    idNhanVienPhuTrach.Value) == null)
            {
                throw new InvalidOperationException(
                    "Người phụ trách không tồn tại hoặc đã bị xóa.");
            }

            bool isCodeExisted = isProjectDocument
                ? _repository.IsProjectCodeExisted(
                    maTaiLieu,
                    projectId.Value,
                    idTaiLieu)
                : _repository.IsCodeExisted(maTaiLieu, idTaiLieu);
            if (isCodeExisted)
            {
                throw new InvalidOperationException(
                    "Mã hồ sơ đã tồn tại trong danh sách "
                    + documentScopeText + ".");
            }

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();
            bool isNew = item == null;

            if (isNew)
            {
                item = new TblTaiLieu
                {
                    IdTaiLieu = UUIDv7.NewGuid(),
                    IdDuAn = projectId,
                    TrangThaiTaiLieu = DocumentStatusKeys.Drafting,
                    TrangThaiGuiKhach = DocumentCustomerStatusKeys.NotSent,
                    TrangThaiLuuTru =
                        DocumentPhysicalStorageStatusKeys.NotStored,
                    DaXoa = false,
                    NguoiTao = currentUserName,
                    NgayTao = currentDate
                };
            }
            else
            {
                ValidateRequirementChanges(
                    item,
                    canTrinhKy,
                    canGuiKhachHang,
                    canLuuVatLy);
                item.IdDuAn = projectId;
                item.NguoiCapNhat = currentUserName;
                item.NgayCapNhat = currentDate;
            }

            item.IdLoaiTaiLieu = idLoaiTaiLieu;
            item.IdNhanVienPhuTrach = idNhanVienPhuTrach;
            item.MaTaiLieu = maTaiLieu;
            item.TenTaiLieu = tenTaiLieu;
            item.MoTa = moTa;
            item.CanTrinhKy = canTrinhKy;
            item.HinhThucKy = canTrinhKy ? hinhThucKy : null;
            item.CanGuiKhachHang = canGuiKhachHang;
            item.CanLuuVatLy = canLuuVatLy;

            if (!canGuiKhachHang)
                item.TrangThaiGuiKhach = DocumentCustomerStatusKeys.NotSent;
            if (!canLuuVatLy)
            {
                item.TrangThaiLuuTru =
                    DocumentPhysicalStorageStatusKeys.NotStored;
            }

            TblTaiLieu savedItem = isNew
                ? _repository.Insert(item)
                : _repository.Update(item);

            return savedItem;
        }

        public TblPhienBanTaiLieu CreateInitialVersionFromTemplate(
            Guid idTaiLieu,
            Guid idMauTaiLieu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");

            if (_repository.GetDocumentVersions(idTaiLieu, false).Count > 0)
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã có phiên bản nên không thể tạo lại phiên bản đầu tiên từ mẫu.");
            }

            DataTable templateData = _documentTemplateRepository
                .GetAvailableTemplate(
                    idMauTaiLieu,
                    document.IdLoaiTaiLieu);
            if (templateData.Rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "Mẫu tài liệu không tồn tại, đã bị khóa hoặc chưa có tệp mẫu.");
            }

            DataRow template = templateData.Rows[0];
            string sourceUrl = Convert.ToString(template["FileUrl"]);
            string sourcePath = HostingEnvironment.MapPath(sourceUrl);
            if (string.IsNullOrWhiteSpace(sourcePath)
                || !File.Exists(sourcePath))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tệp vật lý của mẫu tài liệu.");
            }

            string extension = Convert.ToString(template["Ext"]);
            if (string.IsNullOrWhiteSpace(extension))
                extension = Path.GetExtension(sourcePath);
            if (!string.IsNullOrWhiteSpace(extension)
                && !extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            string relativeDirectory = "/Uploads/"
                + FileUploadTypes.DocumentVersion
                + "/"
                + DateTime.Now.ToString("yyyy/MM")
                + "/";
            string destinationDirectory = HostingEnvironment.MapPath(
                relativeDirectory);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                throw new InvalidOperationException(
                    "Không xác định được thư mục lưu phiên bản tài liệu.");
            }

            Directory.CreateDirectory(destinationDirectory);
            string destinationFileName = UUIDv7.NewGuid().ToString("N")
                + extension;
            string destinationPath = Path.Combine(
                destinationDirectory,
                destinationFileName);
            string destinationUrl = relativeDirectory
                + destinationFileName;

            File.Copy(sourcePath, destinationPath, false);

            TblUploadFile copiedFile = null;
            try
            {
                string originalFileName = Convert.ToString(
                    template["TenFileGoc"]);
                if (string.IsNullOrWhiteSpace(originalFileName))
                    originalFileName = Path.GetFileName(sourcePath);

                string displayName = Convert.ToString(template["TenFile"]);
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = Path.GetFileNameWithoutExtension(
                        originalFileName);

                FileInfo copiedFileInfo = new FileInfo(destinationPath);
                copiedFile = new UploadManager(_applicationContext).Create(
                    new TblUploadFile
                    {
                        Id = UUIDv7.NewGuid(),
                        OwnerId = _applicationContext == null
                            ? Guid.Empty
                            : _applicationContext.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        Name = displayName,
                        FileUrl = destinationUrl,
                        FileType = FileTypes.Internal,
                        Ext = extension ?? string.Empty,
                        RefId = document.IdTaiLieu,
                        RefType = FileUploadTypes.DocumentVersion.ToString(),
                        DisplayOrder = 0,
                        FileSize = copiedFileInfo.Length > int.MaxValue
                            ? int.MaxValue
                            : (int)copiedFileInfo.Length,
                        MimeType = Convert.ToString(template["MimeType"]),
                        OriginalFileName = originalFileName,
                        IsHost = true,
                        IsSecretary = true,
                        IsParticipant = true
                    });

                DateTime currentDate = DateTime.UtcNow;
                string currentUserName = GetCurrentUserName();
                string templateName = Convert.ToString(template["TenMau"]);
                string templateVersion = Convert.ToString(
                    template["PhienBanMau"]);
                TblPhienBanTaiLieu version = new TblPhienBanTaiLieu
                {
                    IdPhienBanTaiLieu = UUIDv7.NewGuid(),
                    IdTaiLieu = document.IdTaiLieu,
                    SoPhienBan = "1.0",
                    NguonTao = "TEMPLATE",
                    IdPhienBanNguon = null,
                    MoTaPhienBan = "Tạo từ mẫu "
                        + templateName
                        + (string.IsNullOrWhiteSpace(templateVersion)
                            ? string.Empty
                            : " (" + templateVersion + ")"),
                    NoiDungTrucTiep = null,
                    LaPhienBanHienTai = true,
                    DaXoa = false,
                    NguoiTao = currentUserName,
                    NgayTao = currentDate,
                    IdFileNoiDung = copiedFile.Id
                };

                _repository.InsertDocumentVersion(version);
                WriteDocumentAudit(
                    document.IdTaiLieu,
                    DocumentActivityTypeKeys.CreateFromTemplate,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    version.IdPhienBanTaiLieu,
                    "Phiên bản: v"
                        + version.SoPhienBan
                        + "; Mẫu: "
                        + templateName
                        + (string.IsNullOrWhiteSpace(templateVersion)
                            ? string.Empty
                            : " (" + templateVersion + ")")
                        + "; Tệp: "
                        + originalFileName,
                    "Đã tạo phiên bản đầu tiên từ mẫu tài liệu.");

                return version;
            }
            catch
            {
                if (copiedFile != null)
                {
                    string errorField;
                    string errorMessage;
                    new UploadManager(_applicationContext, copiedFile.Id)
                        .DeleteFile(out errorField, out errorMessage);
                }
                else if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                throw;
            }
        }

        public void SyncDocumentVersions(Guid idTaiLieu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");

            List<TblUploadFile> files = _repository
                .GetDocumentVersionFiles(idTaiLieu)
                .OrderBy(file => file.CreatedDate)
                .ThenBy(file => file.DisplayOrder)
                .ThenBy(file => file.Id)
                .ToList();
            List<TblPhienBanTaiLieu> allVersions = _repository
                .GetDocumentVersions(idTaiLieu, true)
                .OrderBy(version => version.NgayTao)
                .ThenBy(version => version.IdPhienBanTaiLieu)
                .ToList();

            HashSet<Guid> activeFileIds = new HashSet<Guid>(
                files.Select(file => file.Id));
            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();

            List<TblPhienBanTaiLieu> missingFileVersions = allVersions
                .Where(version =>
                    !version.DaXoa
                    && version.IdFileNoiDung.HasValue
                    && !activeFileIds.Contains(version.IdFileNoiDung.Value))
                .ToList();

            foreach (TblPhienBanTaiLieu version in missingFileVersions)
            {
                Guid missingFileId = version.IdFileNoiDung.Value;
                if (document.IdFileBanChinhThuc.HasValue
                    && document.IdFileBanChinhThuc.Value == missingFileId)
                {
                    throw new InvalidOperationException(
                        "Không thể đồng bộ vì file chính thức của hồ sơ đã bị mất. Vui lòng kiểm tra lại dữ liệu file.");
                }

                if (_repository.HasActiveWorkflowForVersion(
                        version.IdPhienBanTaiLieu))
                {
                    throw new InvalidOperationException(
                        "Không thể đồng bộ vì tệp của phiên bản đang được dùng trong quá trình trình ký hoặc gửi khách hàng đã bị mất.");
                }
            }

            foreach (TblPhienBanTaiLieu version in missingFileVersions)
            {
                Guid removedFileId = version.IdFileNoiDung.Value;
                version.IdFileNoiDung = null;
                version.DaXoa = true;
                version.LaPhienBanHienTai = false;
                version.NguoiCapNhat = currentUserName;
                version.NgayCapNhat = currentDate;
                _repository.UpdateDocumentVersion(version);

                WriteDocumentAudit(
                    idTaiLieu,
                    DocumentActivityTypeKeys.DeleteVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    version.IdPhienBanTaiLieu,
                    "Phiên bản: v" + version.SoPhienBan
                        + "; Id tệp: " + removedFileId,
                    "Đã xóa một phiên bản tài liệu.");
            }

            List<TblPhienBanTaiLieu> activeVersions = allVersions
                .Where(version => !version.DaXoa)
                .ToList();
            HashSet<Guid> linkedFileIds = new HashSet<Guid>(
                activeVersions
                    .Where(version => version.IdFileNoiDung.HasValue)
                    .Select(version => version.IdFileNoiDung.Value));

            foreach (TblUploadFile file in files
                .Where(file => !linkedFileIds.Contains(file.Id)))
            {
                TblPhienBanTaiLieu previousCurrent = activeVersions
                    .FirstOrDefault(version => version.LaPhienBanHienTai);
                ClearCurrentVersion(activeVersions, currentUserName, currentDate);

                TblPhienBanTaiLieu newVersion = new TblPhienBanTaiLieu
                {
                    IdPhienBanTaiLieu = UUIDv7.NewGuid(),
                    IdTaiLieu = idTaiLieu,
                    SoPhienBan = GetNextVersionNumber(allVersions),
                    NguonTao = "UPLOAD",
                    IdPhienBanNguon = previousCurrent == null
                        ? (Guid?)null
                        : previousCurrent.IdPhienBanTaiLieu,
                    MoTaPhienBan = "Tải lên file "
                        + GetUploadFileName(file),
                    NoiDungTrucTiep = null,
                    LaPhienBanHienTai = true,
                    DaXoa = false,
                    NguoiTao = currentUserName,
                    NgayTao = currentDate,
                    IdFileNoiDung = file.Id
                };

                _repository.InsertDocumentVersion(newVersion);
                WriteDocumentAudit(
                    idTaiLieu,
                    DocumentActivityTypeKeys.UploadVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    newVersion.IdPhienBanTaiLieu,
                    "Phiên bản: v"
                        + newVersion.SoPhienBan
                        + "; Tệp: "
                        + GetUploadFileName(file),
                    "Đã tải lên một phiên bản tài liệu mới.");
                allVersions.Add(newVersion);
                activeVersions.Add(newVersion);
                linkedFileIds.Add(file.Id);
                currentDate = currentDate.AddTicks(1);
            }

            EnsureOneCurrentVersion(
                activeVersions,
                currentUserName,
                DateTime.UtcNow);
        }

        public TblTaiLieu SetOfficialFile(
            Guid idTaiLieu,
            Guid idPhienBanTaiLieu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");

            if (document.CanTrinhKy)
            {
                throw new InvalidOperationException(
                    "Hồ sơ cần trình ký chỉ được chọn file chính thức từ kết quả ký.");
            }

            TblPhienBanTaiLieu version = _repository
                .GetDocumentVersionById(idTaiLieu, idPhienBanTaiLieu);
            if (version == null || !version.IdFileNoiDung.HasValue)
            {
                throw new InvalidOperationException(
                    "Phiên bản không tồn tại hoặc không có tệp nội dung.");
            }

            TblUploadFile file = _repository.GetDocumentVersionFileById(
                idTaiLieu,
                version.IdFileNoiDung.Value);
            if (file == null)
            {
                throw new InvalidOperationException(
                    "Tệp của phiên bản không tồn tại hoặc đã bị xóa.");
            }

            if (!IsDocumentFileAvailable(file))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tệp vật lý của phiên bản nên không thể chọn làm file chính thức.");
            }

            if (document.IdFileBanChinhThuc == file.Id)
                return document;

            DateTime currentDate = DateTime.UtcNow;
            document.IdFileBanChinhThuc = file.Id;
            document.NguoiCapNhat = GetCurrentUserName();
            document.NgayCapNhat = currentDate;
            TblTaiLieu savedDocument = _repository.Update(document);

            return savedDocument;
        }

        public TblTaiLieu ClearOfficialFile(
            Guid idTaiLieu,
            Guid idPhienBanTaiLieu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");

            if (document.CanTrinhKy)
            {
                throw new InvalidOperationException(
                    "File chính thức của hồ sơ cần trình ký phải được quản lý trong quá trình trình ký.");
            }

            TblPhienBanTaiLieu version = _repository
                .GetDocumentVersionById(idTaiLieu, idPhienBanTaiLieu);
            if (version == null || !version.IdFileNoiDung.HasValue)
            {
                throw new InvalidOperationException(
                    "Phiên bản không tồn tại hoặc không có tệp nội dung.");
            }

            TblUploadFile file = _repository.GetDocumentVersionFileById(
                idTaiLieu,
                version.IdFileNoiDung.Value);
            if (file == null
                || !document.IdFileBanChinhThuc.HasValue
                || document.IdFileBanChinhThuc.Value != file.Id)
            {
                throw new InvalidOperationException(
                    "Phiên bản này không còn là file chính thức của hồ sơ.");
            }

            DateTime currentDate = DateTime.UtcNow;
            document.IdFileBanChinhThuc = null;
            document.NguoiCapNhat = GetCurrentUserName();
            document.NgayCapNhat = currentDate;
            TblTaiLieu savedDocument = _repository.Update(document);

            return savedDocument;
        }

        public void PrepareDocumentVersionFilesForDeletion(
            Guid idTaiLieu,
            IEnumerable<Guid> fileIds)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ.");

            HashSet<Guid> removedFileIds = new HashSet<Guid>(
                (fileIds ?? Enumerable.Empty<Guid>())
                    .Where(fileId => fileId != Guid.Empty));
            if (removedFileIds.Count == 0)
                return;

            List<TblPhienBanTaiLieu> allVersions = _repository
                .GetDocumentVersions(idTaiLieu, true)
                .ToList();
            Dictionary<Guid, TblUploadFile> removedFiles = _repository
                .GetDocumentVersionFiles(idTaiLieu)
                .Where(file => removedFileIds.Contains(file.Id))
                .ToDictionary(file => file.Id);

            if (removedFiles.Count != removedFileIds.Count)
            {
                throw new InvalidOperationException(
                    "Danh sách tệp cần xóa không hợp lệ hoặc không thuộc hồ sơ này.");
            }

            List<TblPhienBanTaiLieu> versionsToDelete = allVersions
                .Where(version =>
                    !version.DaXoa
                    && version.IdFileNoiDung.HasValue
                    && removedFileIds.Contains(
                        version.IdFileNoiDung.Value))
                .ToList();

            if (document.IdFileBanChinhThuc.HasValue
                && removedFileIds.Contains(
                    document.IdFileBanChinhThuc.Value))
            {
                throw new InvalidOperationException(
                    "Không thể xóa phiên bản đang là file chính thức. Hãy bỏ chọn file chính thức trước.");
            }

            foreach (TblPhienBanTaiLieu version in versionsToDelete)
            {
                if (_repository.HasActiveWorkflowForVersion(
                        version.IdPhienBanTaiLieu))
                {
                    throw new InvalidOperationException(
                        "Không thể xóa phiên bản đã được sử dụng trong quá trình trình ký hoặc gửi khách hàng.");
                }
            }
        }

        /// <summary>
        /// Deletes selected document-version files through the repository
        /// transaction, then removes only safe physical files after commit.
        /// </summary>
        public DocumentVersionFileDeletionResult DeleteDocumentVersionFiles(
            Guid idTaiLieu,
            IEnumerable<Guid> fileIds)
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

            DocumentVersionFileDeletionResult result = _repository
                .DeleteDocumentVersionFiles(
                    idTaiLieu,
                    requestedFileIds,
                    GetCurrentUserName(),
                    DateTime.UtcNow);

            if (result == null)
                return new DocumentVersionFileDeletionResult();

            WriteDeletedVersionFileAudits(idTaiLieu, result);
            result.WarningMessage = CleanupDeletedDocumentVersionFiles(
                idTaiLieu,
                requestedFileIds,
                result.DeletedFiles);
            return result;
        }

        public bool DeleteCompanyDocument(Guid idTaiLieu)
        {
            TblTaiLieu item = _repository.GetCompanyById(idTaiLieu);
            if (item == null)
                return false;

            if (item.IdFileBanChinhThuc.HasValue
                || _repository.HasRelatedRecords(idTaiLieu))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã có file, phiên bản hoặc nghiệp vụ liên quan nên không thể xóa.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Delete(item);
        }

        public bool DeleteProjectDocument(
            Guid idTaiLieu,
            Guid projectId)
        {
            RequireProjectDocumentAccess(projectId, ActionKeys.Delete);

            TblTaiLieu item = _repository.GetProjectById(
                idTaiLieu,
                projectId);
            if (item == null)
                return false;

            if (item.IdFileBanChinhThuc.HasValue
                || _repository.HasRelatedRecords(idTaiLieu))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã có file, phiên bản hoặc nghiệp vụ liên quan nên không thể xóa.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Delete(item);
        }

        private void WriteSigningAudit(
            Guid documentId,
            DocumentSigningOperationResult result)
        {
            if (result == null
                || string.IsNullOrWhiteSpace(result.AuditActivityType))
            {
                return;
            }

            WriteDocumentAudit(
                documentId,
                result.AuditActivityType,
                result.AuditReferenceType,
                result.AuditReferenceId,
                result.AuditChanges,
                result.AuditDescription);
        }

        private void WriteCustomerDeliveryAudit(
            Guid documentId,
            DocumentCustomerDeliveryOperationResult result)
        {
            if (result == null
                || string.IsNullOrWhiteSpace(result.AuditActivityType))
            {
                return;
            }

            WriteDocumentAudit(
                documentId,
                result.AuditActivityType,
                result.AuditReferenceType,
                result.AuditReferenceId,
                result.AuditChanges,
                result.AuditDescription);
        }

        private void WritePhysicalStorageAudit(
            Guid documentId,
            DocumentPhysicalStorageOperationResult result)
        {
            if (result == null
                || string.IsNullOrWhiteSpace(result.AuditActivityType))
            {
                return;
            }

            WriteDocumentAudit(
                documentId,
                result.AuditActivityType,
                result.AuditReferenceType,
                result.AuditReferenceId,
                result.AuditChanges,
                result.AuditDescription);
        }

        private void WriteDocumentAudit(
            Guid documentId,
            string activityType,
            string referenceType,
            Guid? referenceId,
            string changes,
            string description)
        {
            try
            {
                _repository.WriteDocumentAuditAsync(
                        documentId,
                        activityType,
                        referenceType,
                        referenceId,
                        changes,
                        description,
                        GetClientInfo())
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                SysLogger.LogError(
                    exception,
                    "Failed to log document activity " + activityType);
            }
        }

        private void WriteDeletedVersionFileAudits(
            Guid documentId,
            DocumentVersionFileDeletionResult result)
        {
            if (result == null)
            {
                return;
            }

            Dictionary<Guid, TblUploadFile> filesById = result.DeletedFiles
                .Where(file => file != null && file.Id != Guid.Empty)
                .GroupBy(file => file.Id)
                .ToDictionary(group => group.Key, group => group.First());
            HashSet<Guid> versionFileIds = new HashSet<Guid>(
                result.DeletedVersions
                    .Where(version => version != null
                        && version.IdFileNoiDung.HasValue)
                    .Select(version => version.IdFileNoiDung.Value));

            foreach (TblPhienBanTaiLieu version in result.DeletedVersions
                .Where(item => item != null))
            {
                TblUploadFile file = null;
                if (version.IdFileNoiDung.HasValue)
                {
                    filesById.TryGetValue(
                        version.IdFileNoiDung.Value,
                        out file);
                }

                WriteDocumentAudit(
                    documentId,
                    DocumentActivityTypeKeys.DeleteVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    version.IdPhienBanTaiLieu,
                    "Phiên bản: v" + version.SoPhienBan
                        + "; Tệp: " + GetUploadFileName(file)
                        + "; Id tệp: "
                        + (version.IdFileNoiDung.HasValue
                            ? version.IdFileNoiDung.Value.ToString()
                            : string.Empty),
                    "Đã xóa một phiên bản tài liệu.");
            }

            foreach (TblUploadFile file in result.DeletedFiles
                .Where(item => item != null
                    && !versionFileIds.Contains(item.Id)))
            {
                WriteDocumentAudit(
                    documentId,
                    DocumentActivityTypeKeys.DeleteVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    null,
                    "Tệp không gắn phiên bản: "
                        + GetUploadFileName(file)
                        + "; Id tệp: " + file.Id,
                    "Đã xóa một tệp phiên bản tài liệu.");
            }
        }

        private void ValidateRequirementChanges(
            TblTaiLieu item,
            bool canTrinhKy,
            bool canGuiKhachHang,
            bool canLuuVatLy)
        {
            if (item.CanTrinhKy
                && !canTrinhKy
                && (IsSigningStatus(item.TrangThaiTaiLieu)
                    || string.Equals(
                        item.TrangThaiTaiLieu,
                        DocumentStatusKeys.Completed,
                        StringComparison.OrdinalIgnoreCase)
                    || _repository.HasSigningRecords(item.IdTaiLieu)))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã phát sinh quá trình trình ký nên không thể bỏ yêu cầu trình ký.");
            }

            if (item.CanGuiKhachHang
                && !canGuiKhachHang
                && (!string.Equals(
                        item.TrangThaiGuiKhach,
                        DocumentCustomerStatusKeys.NotSent,
                        StringComparison.OrdinalIgnoreCase)
                    || _repository.HasCustomerDeliveryRecords(
                        item.IdTaiLieu)))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã phát sinh lần gửi khách nên không thể bỏ yêu cầu gửi khách.");
            }

            if (item.CanLuuVatLy
                && !canLuuVatLy
                && (!string.Equals(
                        item.TrangThaiLuuTru,
                        DocumentPhysicalStorageStatusKeys.NotStored,
                        StringComparison.OrdinalIgnoreCase)
                    || _repository.HasPhysicalStorageRecords(
                        item.IdTaiLieu)))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã phát sinh lưu trữ bản cứng nên không thể bỏ yêu cầu lưu bản cứng.");
            }
        }

        private void ClearCurrentVersion(
            IEnumerable<TblPhienBanTaiLieu> versions,
            string currentUserName,
            DateTime currentDate)
        {
            foreach (TblPhienBanTaiLieu version in versions
                .Where(item => item.LaPhienBanHienTai))
            {
                version.LaPhienBanHienTai = false;
                version.NguoiCapNhat = currentUserName;
                version.NgayCapNhat = currentDate;
                _repository.UpdateDocumentVersion(version);
            }
        }

        private void EnsureOneCurrentVersion(
            List<TblPhienBanTaiLieu> activeVersions,
            string currentUserName,
            DateTime currentDate)
        {
            List<TblPhienBanTaiLieu> availableVersions = activeVersions
                .Where(version => !version.DaXoa)
                .OrderByDescending(version => version.NgayTao)
                .ThenByDescending(version => version.IdPhienBanTaiLieu)
                .ToList();
            if (availableVersions.Count == 0)
                return;

            TblPhienBanTaiLieu current = availableVersions
                .FirstOrDefault(version => version.LaPhienBanHienTai)
                ?? availableVersions[0];

            foreach (TblPhienBanTaiLieu version in availableVersions)
            {
                bool shouldBeCurrent =
                    version.IdPhienBanTaiLieu
                    == current.IdPhienBanTaiLieu;
                if (version.LaPhienBanHienTai == shouldBeCurrent)
                    continue;

                version.LaPhienBanHienTai = shouldBeCurrent;
                version.NguoiCapNhat = currentUserName;
                version.NgayCapNhat = currentDate;
                _repository.UpdateDocumentVersion(version);
            }
        }

        private static string GetNextVersionNumber(
            IEnumerable<TblPhienBanTaiLieu> versions)
        {
            decimal maximum = 0M;
            foreach (TblPhienBanTaiLieu version in versions)
            {
                decimal parsed;
                if (decimal.TryParse(
                        version.SoPhienBan,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out parsed)
                    && parsed > maximum)
                {
                    maximum = parsed;
                }
            }

            return (Math.Floor(maximum) + 1M)
                .ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static string GetUploadFileName(TblUploadFile file)
        {
            if (file == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(file.OriginalFileName)
                ? file.Name
                : file.OriginalFileName;
        }

        private static bool IsDocumentFileAvailable(TblUploadFile file)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FileUrl))
                return false;

            Uri absoluteUri;
            if (Uri.TryCreate(
                    file.FileUrl,
                    UriKind.Absolute,
                    out absoluteUri))
            {
                return absoluteUri.Scheme == Uri.UriSchemeHttp
                    || absoluteUri.Scheme == Uri.UriSchemeHttps;
            }

            try
            {
                string virtualPath = file.FileUrl.StartsWith(
                    "~/",
                    StringComparison.Ordinal)
                    ? file.FileUrl
                    : file.FileUrl.StartsWith("/", StringComparison.Ordinal)
                        ? file.FileUrl
                        : "/" + file.FileUrl;
                string physicalPath = HostingEnvironment.MapPath(virtualPath);
                return !string.IsNullOrWhiteSpace(physicalPath)
                    && File.Exists(physicalPath);
            }
            catch
            {
                return false;
            }
        }

        private string CleanupDeletedDocumentVersionFiles(
            Guid idTaiLieu,
            IEnumerable<Guid> deletedFileIds,
            IEnumerable<TblUploadFile> deletedFiles)
        {
            List<string> warnings = new List<string>();
            HashSet<string> attemptedPaths =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Guid> excludedFileIds = (deletedFileIds
                    ?? Enumerable.Empty<Guid>())
                .Where(fileId => fileId != Guid.Empty)
                .Distinct()
                .ToList();

            foreach (TblUploadFile file in deletedFiles
                ?? Enumerable.Empty<TblUploadFile>())
            {
                string path = null;
                try
                {
                    path = ResolveDocumentVersionPhysicalPath(file.FileUrl);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        string warning = "Không thể tự động xóa tệp vật lý "
                            + file.Id
                            + "; đường dẫn không thuộc thư mục phiên bản tài liệu.";
                        warnings.Add(warning);
                        SysLogger.LogError(
                            warning
                            + " Hồ sơ: {0}; FileUrl: {1}",
                            idTaiLieu,
                            file.FileUrl ?? string.Empty);
                        continue;
                    }

                    if (!attemptedPaths.Add(path))
                        continue;

                    bool hasOtherReference = _repository
                        .HasUploadFileReferenceByPath(
                            NormalizeDocumentVersionVirtualPath(file.FileUrl),
                            excludedFileIds);
                    if (hasOtherReference)
                    {
                        string warning = "Không xóa tệp vật lý "
                            + file.Id
                            + " vì đường dẫn đang được bản ghi tệp khác sử dụng.";
                        warnings.Add(warning);
                        SysLogger.LogError(
                            warning
                            + " Hồ sơ: {0}; Path: {1}",
                            idTaiLieu,
                            path);
                        continue;
                    }

                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (Exception exception)
                {
                    string warning = "Không thể xóa tệp vật lý "
                        + file.Id
                        + "; có thể cần xử lý thủ công.";
                    warnings.Add(warning);
                    SysLogger.LogError(
                        exception,
                        warning
                        + " Hồ sơ: {0}; Path: {1}",
                        idTaiLieu,
                        path ?? (file.FileUrl ?? string.Empty));
                }
            }

            return warnings.Count == 0
                ? null
                : string.Join(" ", warnings.Distinct());
        }

        private string ResolveDocumentVersionPhysicalPath(string fileUrl)
        {
            string virtualPath = NormalizeDocumentVersionVirtualPath(fileUrl);
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return null;
            }

            const string versionRoot = "/Uploads/DocumentVersion/";

            string rootPath = null;
            string physicalPath = null;
            try
            {
                if (_applicationContext != null)
                {
                    rootPath = _applicationContext.MapPath(versionRoot);
                    physicalPath = _applicationContext.MapPath(virtualPath);
                }

                if (string.IsNullOrWhiteSpace(rootPath))
                    rootPath = HostingEnvironment.MapPath(versionRoot);
                if (string.IsNullOrWhiteSpace(physicalPath))
                    physicalPath = HostingEnvironment.MapPath(virtualPath);
                if (string.IsNullOrWhiteSpace(rootPath)
                    || string.IsNullOrWhiteSpace(physicalPath))
                {
                    return null;
                }

                string normalizedRoot = Path.GetFullPath(rootPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;
                string normalizedPath = Path.GetFullPath(physicalPath);
                return normalizedPath.StartsWith(
                        normalizedRoot,
                        StringComparison.OrdinalIgnoreCase)
                    ? normalizedPath
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeDocumentVersionVirtualPath(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return null;

            Uri absoluteUri;
            if (Uri.TryCreate(fileUrl.Trim(), UriKind.Absolute, out absoluteUri))
                return null;

            string virtualPath = fileUrl.Trim().Replace('\\', '/');
            if (virtualPath.StartsWith("~/", StringComparison.Ordinal))
                virtualPath = virtualPath.Substring(1);
            if (!virtualPath.StartsWith("/", StringComparison.Ordinal))
                virtualPath = "/" + virtualPath;

            const string versionRoot = "/Uploads/DocumentVersion/";
            return virtualPath.StartsWith(
                    versionRoot,
                    StringComparison.OrdinalIgnoreCase)
                ? virtualPath
                : null;
        }

        private static bool IsSigningStatus(string status)
        {
            return string.Equals(
                       status,
                       DocumentStatusKeys.PendingSignature,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       status,
                       DocumentStatusKeys.ChangesRequested,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       status,
                       DocumentStatusKeys.Signed,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateCompanyDocumentCode()
        {
            return "HS-"
                + DateTime.UtcNow.ToString("yyyyMMdd")
                + "-"
                + Guid.NewGuid().ToString("N")
                    .Substring(0, 8)
                    .ToUpperInvariant();
        }

        private static string GenerateProjectDocumentCode(string projectCode)
        {
            string prefix = (projectCode ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "PROJECT";
            if (prefix.Length > 68)
                prefix = prefix.Substring(0, 68);

            return prefix
                + "-HS-"
                + DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                + "-"
                + Guid.NewGuid().ToString("N")
                    .Substring(0, 6)
                    .ToUpperInvariant();
        }

        private string GetCurrentUserName()
        {
            if (_applicationContext == null
                || string.IsNullOrWhiteSpace(_applicationContext.UserName))
            {
                return "[System]";
            }

            return _applicationContext.UserName;
        }

        private Guid GetCurrentUserId()
        {
            return _applicationContext == null
                ? Guid.Empty
                : _applicationContext.UserId;
        }
    }
}
