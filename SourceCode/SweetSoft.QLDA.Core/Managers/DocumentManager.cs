using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

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

    public class DocumentManager : BaseManager
    {
        private static readonly Lazy<DocumentManager> _instance =
            new Lazy<DocumentManager>(() => new DocumentManager());

        private readonly DocumentRepository _repository;
        private readonly DocumentTypeRepository _documentTypeRepository;

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

            return isNew
                ? _repository.Insert(item)
                : _repository.Update(item);
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

        public bool DeleteCompanyDocument(Guid idTaiLieu)
        {
            TblTaiLieu item = _repository.GetById(idTaiLieu);
            if (item == null)
                return false;

            if (item.IdFileBanChinhThuc.HasValue
                || _repository.HasRelatedRecords(idTaiLieu))
            {
                throw new InvalidOperationException(
                    "Hồ sơ đã có file, phiên bản hoặc lịch sử nghiệp vụ nên không thể xóa.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Delete(item);
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
