using SweetSoft.QLDA.Core.FileManager;
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

    public static class DocumentCustomerStatusKeys
    {
        public const string NotSent = "CHUA_GUI";
        public const string Sent = "DA_GUI";
        public const string WaitingForReturn = "CHO_NHAN_LAI";
        public const string ReceivedBack = "DA_NHAN_LAI";
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
    }

    public static class DocumentActivityReferenceKeys
    {
        public const string Document = "TblTaiLieu";
        public const string DocumentVersion = "TblPhienBanTaiLieu";
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

        public TblTaiLieu GetCompanyDocumentById(Guid idTaiLieu)
        {
            return _repository.GetById(idTaiLieu);
        }

        public List<AspnetUser> GetAvailableEmployees()
        {
            return _repository.GetAvailableEmployees();
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

        public DataTable GetSigningHistory(Guid idTaiLieu)
        {
            return _repository.GetSigningHistory(idTaiLieu);
        }

        public DataTable GetCustomerDeliveryHistory(Guid idTaiLieu)
        {
            return _repository.GetCustomerDeliveryHistory(idTaiLieu);
        }

        public DataTable GetPhysicalStorageHistory(Guid idTaiLieu)
        {
            return _repository.GetPhysicalStorageHistory(idTaiLieu);
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
            maTaiLieu = (maTaiLieu ?? string.Empty).Trim().ToUpperInvariant();
            tenTaiLieu = (tenTaiLieu ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();
            hinhThucKy = (hinhThucKy ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

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
                item = _repository.GetById(idTaiLieu);
                if (item == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty.");
                }

                if (string.IsNullOrEmpty(maTaiLieu))
                    throw new ArgumentException("Mã hồ sơ không được để trống.");
            }
            else if (string.IsNullOrEmpty(maTaiLieu))
            {
                maTaiLieu = GenerateCompanyDocumentCode();
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

            if (_repository.IsCodeExisted(maTaiLieu, idTaiLieu))
            {
                throw new InvalidOperationException(
                    "Mã hồ sơ đã tồn tại trong danh sách hồ sơ công ty.");
            }

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();
            bool isNew = item == null;
            string historyDetails = isNew
                ? BuildDocumentCreationDetails(
                    maTaiLieu,
                    tenTaiLieu,
                    documentType,
                    idNhanVienPhuTrach,
                    canTrinhKy,
                    hinhThucKy,
                    canLuuVatLy)
                : BuildDocumentUpdateDetails(
                    item,
                    idLoaiTaiLieu,
                    idNhanVienPhuTrach,
                    maTaiLieu,
                    tenTaiLieu,
                    moTa,
                    canTrinhKy,
                    hinhThucKy,
                    canGuiKhachHang,
                    canLuuVatLy);

            if (isNew)
            {
                item = new TblTaiLieu
                {
                    IdTaiLieu = UUIDv7.NewGuid(),
                    IdDuAn = null,
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
                item.IdDuAn = null;
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

            if (isNew)
            {
                WriteDocumentHistory(
                    savedItem.IdTaiLieu,
                    DocumentActivityTypeKeys.CreateDocument,
                    DocumentActivityReferenceKeys.Document,
                    savedItem.IdTaiLieu,
                    historyDetails,
                    "Đã tạo hồ sơ công ty.",
                    currentDate);
            }
            else if (!string.IsNullOrWhiteSpace(historyDetails))
            {
                WriteDocumentHistory(
                    savedItem.IdTaiLieu,
                    DocumentActivityTypeKeys.UpdateDocument,
                    DocumentActivityReferenceKeys.Document,
                    savedItem.IdTaiLieu,
                    historyDetails,
                    "Đã cập nhật thông tin hồ sơ.",
                    currentDate);
            }

            return savedItem;
        }

        public TblPhienBanTaiLieu CreateInitialVersionFromTemplate(
            Guid idTaiLieu,
            Guid idMauTaiLieu)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ công ty.");

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
                WriteDocumentHistory(
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
                    "Đã tạo phiên bản đầu tiên từ mẫu tài liệu.",
                    currentDate);

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
                throw new InvalidOperationException("Không tìm thấy hồ sơ công ty.");

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

            foreach (TblPhienBanTaiLieu version in allVersions
                .Where(version =>
                    !version.DaXoa
                    && version.IdFileNoiDung.HasValue
                    && !activeFileIds.Contains(version.IdFileNoiDung.Value)))
            {
                version.DaXoa = true;
                version.LaPhienBanHienTai = false;
                version.NguoiCapNhat = currentUserName;
                version.NgayCapNhat = currentDate;
                _repository.UpdateDocumentVersion(version);
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
                WriteDocumentHistory(
                    idTaiLieu,
                    DocumentActivityTypeKeys.UploadVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    newVersion.IdPhienBanTaiLieu,
                    "Phiên bản: v"
                        + newVersion.SoPhienBan
                        + "; Tệp: "
                        + GetUploadFileName(file),
                    "Đã tải lên một phiên bản tài liệu mới.",
                    currentDate);
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

        public void PrepareDocumentVersionFilesForDeletion(
            Guid idTaiLieu,
            IEnumerable<Guid> fileIds)
        {
            TblTaiLieu document = _repository.GetById(idTaiLieu);
            if (document == null)
                throw new InvalidOperationException("Không tìm thấy hồ sơ công ty.");

            HashSet<Guid> removedFileIds = new HashSet<Guid>(
                (fileIds ?? Enumerable.Empty<Guid>())
                    .Where(fileId => fileId != Guid.Empty));
            if (removedFileIds.Count == 0)
                return;

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();
            List<TblPhienBanTaiLieu> allVersions = _repository
                .GetDocumentVersions(idTaiLieu, true)
                .ToList();
            Dictionary<Guid, TblUploadFile> removedFiles = _repository
                .GetDocumentVersionFiles(idTaiLieu)
                .Where(file => removedFileIds.Contains(file.Id))
                .ToDictionary(file => file.Id);

            foreach (TblPhienBanTaiLieu version in allVersions
                .Where(version =>
                    !version.DaXoa
                    && version.IdFileNoiDung.HasValue
                    && removedFileIds.Contains(
                        version.IdFileNoiDung.Value)))
            {
                Guid removedFileId = version.IdFileNoiDung.Value;
                version.IdFileNoiDung = null;
                version.LaPhienBanHienTai = false;
                version.DaXoa = true;
                version.NguoiCapNhat = currentUserName;
                version.NgayCapNhat = currentDate;
                _repository.UpdateDocumentVersion(version);

                TblUploadFile removedFile;
                removedFiles.TryGetValue(removedFileId, out removedFile);
                WriteDocumentHistory(
                    idTaiLieu,
                    DocumentActivityTypeKeys.DeleteVersion,
                    DocumentActivityReferenceKeys.DocumentVersion,
                    version.IdPhienBanTaiLieu,
                    "Phiên bản: v"
                        + version.SoPhienBan
                        + (removedFile == null
                            ? string.Empty
                            : "; Tệp: " + GetUploadFileName(removedFile)),
                    "Đã xóa một phiên bản tài liệu.",
                    currentDate);
            }

            if (document.IdFileBanChinhThuc.HasValue
                && removedFileIds.Contains(
                    document.IdFileBanChinhThuc.Value))
            {
                document.IdFileBanChinhThuc = null;
                document.NguoiCapNhat = currentUserName;
                document.NgayCapNhat = currentDate;
                _repository.Update(document);
            }

            EnsureOneCurrentVersion(
                allVersions
                    .Where(version => !version.DaXoa)
                    .ToList(),
                currentUserName,
                currentDate);
        }

        public bool DeleteCompanyDocument(Guid idTaiLieu)
        {
            TblTaiLieu item = _repository.GetById(idTaiLieu);
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
            bool isDeleted = _repository.Delete(item);
            if (isDeleted)
            {
                WriteDocumentHistory(
                    item.IdTaiLieu,
                    DocumentActivityTypeKeys.DeleteDocument,
                    DocumentActivityReferenceKeys.Document,
                    item.IdTaiLieu,
                    "Mã hồ sơ: "
                        + GetHistoryValue(item.MaTaiLieu)
                        + "; Tên hồ sơ: "
                        + GetHistoryValue(item.TenTaiLieu),
                    "Đã xóa hồ sơ công ty.",
                    item.NgayCapNhat);
            }

            return isDeleted;
        }

        private string BuildDocumentCreationDetails(
            string documentCode,
            string documentName,
            TblLoaiTaiLieu documentType,
            Guid? responsibleEmployeeId,
            bool requiresSigning,
            string signingMethod,
            bool requiresPhysicalStorage)
        {
            List<string> details = new List<string>
            {
                "Mã hồ sơ: " + GetHistoryValue(documentCode),
                "Tên hồ sơ: " + GetHistoryValue(documentName),
                "Loại tài liệu: " + GetHistoryValue(
                    documentType == null ? null : documentType.TenLoai),
                "Người phụ trách: " + GetEmployeeHistoryName(
                    responsibleEmployeeId),
                "Cần trình ký: " + GetBooleanHistoryText(requiresSigning),
                "Cần lưu bản cứng: "
                    + GetBooleanHistoryText(requiresPhysicalStorage)
            };

            if (requiresSigning)
            {
                details.Add(
                    "Hình thức ký: "
                    + GetSigningMethodHistoryText(signingMethod));
            }

            return string.Join("; ", details);
        }

        private string BuildDocumentUpdateDetails(
            TblTaiLieu currentItem,
            Guid documentTypeId,
            Guid? responsibleEmployeeId,
            string documentCode,
            string documentName,
            string description,
            bool requiresSigning,
            string signingMethod,
            bool requiresCustomerDelivery,
            bool requiresPhysicalStorage)
        {
            if (currentItem == null)
                return string.Empty;

            List<string> changes = new List<string>();
            AddHistoryChange(
                changes,
                "Mã hồ sơ",
                currentItem.MaTaiLieu,
                documentCode);
            AddHistoryChange(
                changes,
                "Tên hồ sơ",
                currentItem.TenTaiLieu,
                documentName);
            AddHistoryChange(
                changes,
                "Loại tài liệu",
                GetDocumentTypeHistoryName(currentItem.IdLoaiTaiLieu),
                GetDocumentTypeHistoryName(documentTypeId));
            AddHistoryChange(
                changes,
                "Người phụ trách",
                GetEmployeeHistoryName(currentItem.IdNhanVienPhuTrach),
                GetEmployeeHistoryName(responsibleEmployeeId));
            AddHistoryChange(
                changes,
                "Mô tả",
                currentItem.MoTa,
                description);
            AddHistoryChange(
                changes,
                "Cần trình ký",
                GetBooleanHistoryText(currentItem.CanTrinhKy),
                GetBooleanHistoryText(requiresSigning));
            AddHistoryChange(
                changes,
                "Hình thức ký",
                currentItem.CanTrinhKy
                    ? GetSigningMethodHistoryText(currentItem.HinhThucKy)
                    : "Không áp dụng",
                requiresSigning
                    ? GetSigningMethodHistoryText(signingMethod)
                    : "Không áp dụng");
            AddHistoryChange(
                changes,
                "Cần gửi khách hàng",
                GetBooleanHistoryText(currentItem.CanGuiKhachHang),
                GetBooleanHistoryText(requiresCustomerDelivery));
            AddHistoryChange(
                changes,
                "Cần lưu bản cứng",
                GetBooleanHistoryText(currentItem.CanLuuVatLy),
                GetBooleanHistoryText(requiresPhysicalStorage));

            return string.Join("; ", changes);
        }

        private string GetDocumentTypeHistoryName(Guid documentTypeId)
        {
            TblLoaiTaiLieu documentType = _documentTypeRepository
                .GetById(documentTypeId);
            return documentType == null
                ? documentTypeId.ToString()
                : documentType.TenLoai;
        }

        private string GetEmployeeHistoryName(Guid? employeeId)
        {
            if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
                return "Không có";

            AspnetUser employee = _repository.GetEmployeeById(
                employeeId.Value);
            if (employee == null)
                return employeeId.Value.ToString();

            return string.IsNullOrWhiteSpace(employee.DisplayName)
                ? employee.UserName
                : employee.DisplayName;
        }

        private void WriteDocumentHistory(
            Guid documentId,
            string activityType,
            string referenceType,
            Guid? referenceId,
            string changes,
            string description,
            DateTime? activityDate)
        {
            try
            {
                Guid currentUserId = _applicationContext == null
                    ? Guid.Empty
                    : _applicationContext.UserId;
                _repository.InsertDocumentHistory(
                    new TblLichSuTaiLieu
                    {
                        IdLichSuTaiLieu = UUIDv7.NewGuid(),
                        IdTaiLieu = documentId,
                        LoaiHanhDong = activityType,
                        LoaiThamChieu = referenceType,
                        IdThamChieu = referenceId,
                        NoiDungThayDoi = changes,
                        MoTa = description,
                        IdNhanVienThucHien = currentUserId == Guid.Empty
                            ? (Guid?)null
                            : currentUserId,
                        NguoiTao = GetCurrentUserName(),
                        NgayTao = activityDate ?? DateTime.UtcNow
                    });
            }
            catch (Exception exception)
            {
                SysLogger.LogError(
                    exception,
                    "Failed to log document activity " + activityType);
            }
        }

        private static void AddHistoryChange(
            ICollection<string> changes,
            string fieldName,
            string oldValue,
            string newValue)
        {
            string normalizedOldValue = GetHistoryValue(oldValue);
            string normalizedNewValue = GetHistoryValue(newValue);
            if (string.Equals(
                    normalizedOldValue,
                    normalizedNewValue,
                    StringComparison.Ordinal))
            {
                return;
            }

            changes.Add(
                fieldName
                + ": “"
                + normalizedOldValue
                + "” → “"
                + normalizedNewValue
                + "”");
        }

        private static string GetHistoryValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Không có";

            string normalized = value
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            return normalized.Length <= 500
                ? normalized
                : normalized.Substring(0, 497) + "...";
        }

        private static string GetBooleanHistoryText(bool value)
        {
            return value ? "Có" : "Không";
        }

        private static string GetSigningMethodHistoryText(string value)
        {
            return string.Equals(
                value,
                DocumentSigningMethodKeys.DigitalExternal,
                StringComparison.OrdinalIgnoreCase)
                ? "Ký số bên ngoài"
                : "Ký bản giấy";
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
            return string.IsNullOrWhiteSpace(file.OriginalFileName)
                ? file.Name
                : file.OriginalFileName;
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

        private string GetCurrentUserName()
        {
            if (_applicationContext == null
                || string.IsNullOrWhiteSpace(_applicationContext.UserName))
            {
                return "[System]";
            }

            return _applicationContext.UserName;
        }
    }
}
