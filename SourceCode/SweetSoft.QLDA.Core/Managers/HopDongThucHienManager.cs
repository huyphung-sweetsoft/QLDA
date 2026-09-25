using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// The canonical project document attached to an execution contract.
    /// The result carries both ids needed to open the existing document route.
    /// </summary>
    public sealed class ContractDocumentLinkResult
    {
        public Guid ProjectId { get; set; }
        public Guid DocumentId { get; set; }
        public bool IsCreated { get; set; }
    }

    public class HopDongThucHienManager : BaseManager
    {
        private static readonly Lazy<HopDongThucHienManager> _instance = new Lazy<HopDongThucHienManager>(() => new HopDongThucHienManager());

        public static HopDongThucHienManager Instance => _instance.Value;

        private readonly HopDongThucHienRepository _repository;
        private readonly AuditManager _auditManager;

        public HopDongThucHienManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new HopDongThucHienRepository(_auditManager);
        }

        #region Search paging

        public DataTable SearchHopDongThucHiens(string searchTerm, Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, parameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        public DataTable SearchHopDongThucHiens(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(parameters, orderBy, pageNumber, pageSize, out totalRecord);
        }

        #endregion

        #region Create or update

        public TblHopDongThucHien CreateOrUpdate(TblHopDongThucHien dto)
        {
            ValidateContract(dto);
            NormalizeContract(dto);

            bool isInsert = dto.IdHopDongThucHien == Guid.Empty;

            TblHopDongThucHien duplicate = _repository.GetBySoHopDong(dto.SoHopDong);

            bool duplicateNumber = duplicate != null && duplicate.IdHopDongThucHien != dto.IdHopDongThucHien;

            BusinessValidator.ThrowIf(duplicateNumber, BackEndResourceKeys.INVALID_DATA, nameof(dto.SoHopDong), ErrorCodes.Conflict);

            if (isInsert)
                return Insert(dto);

            return Update(dto);
        }

        private TblHopDongThucHien Insert(TblHopDongThucHien dto)
        {
            TblHopDongThucHien item = dto.Clone() as TblHopDongThucHien;

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);

            item.IdHopDongThucHien = UUIDv7.NewGuid();
            item.DaXoa = false;
            item.NguoiTao = SweetContext.Current.UserName;
            item.NgayTao = DateTime.UtcNow;
            item.NguoiCapNhat = null;
            item.NgayCapNhat = null;
            item.LoaiNoiDungHopDong = dto.LoaiNoiDungHopDong;
            item.NoiDungHopDong = dto.NoiDungHopDong;

            item = _repository.Insert(item);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return item;
        }

        private TblHopDongThucHien Update(TblHopDongThucHien dto)
        {
            TblHopDongThucHien item = _repository.GetById(dto.IdHopDongThucHien);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.NotFound);

            Guid? linkedDocumentId = _repository.GetLinkedDocumentId(
                item.IdHopDongThucHien);
            if (linkedDocumentId.HasValue
                && (!string.Equals(
                        item.SoHopDong,
                        dto.SoHopDong,
                        StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(
                        item.TenHopDong,
                        dto.TenHopDong,
                        StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Không thể đổi số hoặc tên hợp đồng sau khi đã tạo hồ sơ hợp đồng liên kết.");
            }

            ObjectHelper.CopyBusinessProperties(dto, item, x => x.IdHopDongThucHien, x => x.DaXoa, x => x.NguoiTao, x => x.NgayTao, x => x.NguoiCapNhat, x => x.NgayCapNhat);

            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;

            item = _repository.Update(item);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return item;
        }

        #endregion

        #region Contract document

        /// <summary>
        /// Creates exactly one project document for the contract, then returns
        /// its project/document ids so the caller can open the existing version
        /// upload screen. A file is never uploaded directly to the contract popup.
        /// </summary>
        public ContractDocumentLinkResult GetOrCreateProjectDocument(
            Guid idHopDongThucHien)
        {
            if (idHopDongThucHien == Guid.Empty)
            {
                throw new ArgumentException(
                    "Hợp đồng không hợp lệ.",
                    nameof(idHopDongThucHien));
            }

            if (!_repository.HasDocumentLinkColumn())
            {
                throw new InvalidOperationException(
                    "Chưa cài cấu trúc liên kết hồ sơ cho hợp đồng. Hãy chạy Database/AddContractDocumentLink.sql trước.");
            }

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.Serializable
            };

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                options))
            {
                TblHopDongThucHien hopDong = _repository.GetById(
                    idHopDongThucHien);
                if (hopDong == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hợp đồng hoặc hợp đồng đã bị xóa.");
                }

                TblDuAn project = GetSingleActiveProject(
                    hopDong.IdHopDongThucHien);
                if (!DocumentManager.Instance.CanAccessProjectDocument(
                    project.IdDuAn,
                    ActionKeys.View))
                {
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền xem hồ sơ của dự án này.");
                }

                Guid? linkedDocumentId = _repository.GetLinkedDocumentIdForUpdate(
                    hopDong.IdHopDongThucHien);
                if (linkedDocumentId.HasValue)
                {
                    TblTaiLieu existingDocument = DocumentManager.Instance
                        .GetProjectDocumentById(
                            linkedDocumentId.Value,
                            project.IdDuAn);
                    if (existingDocument == null)
                    {
                        throw new InvalidOperationException(
                            "Hồ sơ liên kết với hợp đồng không còn thuộc dự án hiện tại. Vui lòng kiểm tra lại dữ liệu liên kết.");
                    }

                    scope.Complete();
                    return new ContractDocumentLinkResult
                    {
                        ProjectId = project.IdDuAn,
                        DocumentId = existingDocument.IdTaiLieu,
                        IsCreated = false
                    };
                }

                TblLoaiTaiLieu documentType = GetConfiguredContractDocumentType();
                TblTaiLieu document = DocumentManager.Instance.SaveProjectDocument(
                    project.IdDuAn,
                    Guid.Empty,
                    documentType.IdLoaiTaiLieu,
                    project.IdNhanVienQuanLy,
                    BuildContractDocumentCode(hopDong),
                    BuildContractDocumentTitle(hopDong),
                    BuildContractDocumentDescription(hopDong),
                    documentType.CanTrinhKy,
                    documentType.CanTrinhKy
                        ? documentType.HinhThucKyMacDinh
                        : null,
                    documentType.CanGuiKhachHang,
                    documentType.CanLuuVatLy);

                bool linked = _repository.TryLinkDocument(
                    hopDong.IdHopDongThucHien,
                    document.IdTaiLieu,
                    SweetContext.Current.UserName,
                    DateTime.UtcNow);
                if (!linked)
                {
                    throw new InvalidOperationException(
                        "Không thể liên kết hồ sơ vừa tạo với hợp đồng. Dữ liệu đã được hoàn tác.");
                }

                WriteContractDocumentLinkAudit(hopDong, project, document);
                scope.Complete();

                return new ContractDocumentLinkResult
                {
                    ProjectId = project.IdDuAn,
                    DocumentId = document.IdTaiLieu,
                    IsCreated = true
                };
            }
        }

        public bool HasLinkedDocument(Guid idHopDongThucHien)
        {
            return idHopDongThucHien != Guid.Empty
                && _repository.GetLinkedDocumentId(idHopDongThucHien)
                    .HasValue;
        }

        /// <summary>
        /// A contract document belongs to one project and must not be moved by
        /// changing the contract selected on a project form.
        /// </summary>
        public void EnsureLinkedDocumentBelongsToProject(
            Guid idHopDongThucHien,
            Guid idDuAn)
        {
            if (idHopDongThucHien == Guid.Empty)
            {
                return;
            }

            Guid? linkedDocumentId = _repository.GetLinkedDocumentId(
                idHopDongThucHien);
            if (!linkedDocumentId.HasValue)
            {
                return;
            }

            Guid? linkedProjectId = _repository.GetLinkedDocumentProjectId(
                idHopDongThucHien);
            if (!linkedProjectId.HasValue)
            {
                throw new InvalidOperationException(
                    "Hợp đồng đang liên kết với một hồ sơ không còn hợp lệ. Không thể thay đổi dự án cho đến khi dữ liệu được xử lý.");
            }

            if (linkedProjectId.Value != idDuAn)
            {
                throw new InvalidOperationException(
                    "Hợp đồng đã có hồ sơ thuộc một dự án khác nên không thể gán lại cho dự án này.");
            }
        }

        private TblDuAn GetSingleActiveProject(Guid idHopDongThucHien)
        {
            List<TblDuAn> projects = _repository.GetActiveProjectsByContractId(
                idHopDongThucHien);
            if (projects == null || projects.Count == 0)
            {
                throw new InvalidOperationException(
                    "Hãy gắn hợp đồng vào một dự án trước khi tạo hồ sơ hợp đồng.");
            }

            if (projects.Count > 1)
            {
                throw new InvalidOperationException(
                    "Hợp đồng đang được gắn với nhiều dự án. Vui lòng xử lý dữ liệu trước khi tạo hồ sơ hợp đồng.");
            }

            return projects[0];
        }

        private TblLoaiTaiLieu GetConfiguredContractDocumentType()
        {
            Guid idLoaiTaiLieu = SettingManager.Instance.GetSettingValueGuid(
                SettingKeys.ContractDocumentTypeId);
            if (idLoaiTaiLieu == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình loại tài liệu Hợp đồng. Vui lòng cấu hình Settings.ContractDocumentTypeId trước.");
            }

            TblLoaiTaiLieu documentType = DocumentManager.Instance
                .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (documentType == null || documentType.DaXoa || !documentType.KichHoat)
            {
                throw new InvalidOperationException(
                    "Loại tài liệu Hợp đồng được cấu hình không tồn tại hoặc đang bị khóa.");
            }

            return documentType;
        }

        private static string BuildContractDocumentCode(
            TblHopDongThucHien hopDong)
        {
            const int maxLength = 100;
            string contractNumber = (hopDong.SoHopDong ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            // A document code only has to be unique within its project, but a
            // human-created document may already use HD-{contract number}.
            // Keep that readable prefix and append a stable short id so the
            // automatic creation never collides with a normal project record.
            string suffix = "-" + hopDong.IdHopDongThucHien
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();
            int contractNumberLength = maxLength - "HD-".Length - suffix.Length;
            if (contractNumber.Length > contractNumberLength)
            {
                contractNumber = contractNumber
                    .Substring(0, contractNumberLength)
                    .TrimEnd();
            }

            return "HD-" + contractNumber + suffix;
        }

        private static string BuildContractDocumentTitle(
            TblHopDongThucHien hopDong)
        {
            const int maxLength = 255;
            string prefix = "Hợp đồng ";
            string separator = " – ";
            string contractNumber = (hopDong.SoHopDong ?? string.Empty).Trim();
            string contractName = (hopDong.TenHopDong ?? string.Empty).Trim();
            int contractNameLength = maxLength
                - prefix.Length
                - contractNumber.Length
                - separator.Length;

            if (contractNameLength <= 0)
            {
                return (prefix + contractNumber).Substring(0, maxLength);
            }

            if (contractName.Length > contractNameLength)
            {
                contractName = contractName
                    .Substring(0, contractNameLength)
                    .TrimEnd();
            }

            return prefix + contractNumber + separator + contractName;
        }

        private static string BuildContractDocumentDescription(
            TblHopDongThucHien hopDong)
        {
            return "Hồ sơ hợp đồng được tạo tự động từ hợp đồng số "
                + (hopDong.SoHopDong ?? string.Empty).Trim()
                + ".";
        }

        private void WriteContractDocumentLinkAudit(
            TblHopDongThucHien hopDong,
            TblDuAn project,
            TblTaiLieu document)
        {
            try
            {
                var auditEntry = new
                {
                    IdTaiLieu = document.IdTaiLieu,
                    IdDuAn = project.IdDuAn,
                    MaTaiLieu = document.MaTaiLieu,
                    TenTaiLieu = document.TenTaiLieu
                };

                _auditManager.LogActionAsync(
                        LogActions.Actions.UPDATE,
                        auditEntry,
                        nameof(TblHopDongThucHien),
                        hopDong.IdHopDongThucHien,
                        SweetContext.Current.UserName,
                        document.IdTaiLieu,
                        hopDong.SoHopDong,
                        "Đã tạo và liên kết hồ sơ hợp đồng "
                            + document.MaTaiLieu
                            + " với dự án "
                            + project.MaDuAn
                            + ".")
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                SysLogger.LogError(
                    exception,
                    "Failed to log contract-document link for "
                        + hopDong.IdHopDongThucHien);
            }
        }

        #endregion

        #region Validation

        private void ValidateContract(TblHopDongThucHien dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);

            BusinessValidator.ThrowIfNullOrEmpty(dto.SoHopDong, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.SoHopDong));

            BusinessValidator.ThrowIfNullOrEmpty(dto.TenHopDong, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenHopDong));

            BusinessValidator.ThrowIf(dto.IdKhachHang == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdKhachHang));

            BusinessValidator.ThrowIf(!dto.GiaTriHopDong.HasValue || dto.GiaTriHopDong.Value <= 0, BackEndResourceKeys.INVALID_DATA, nameof(dto.GiaTriHopDong));

            BusinessValidator.ThrowIf(!dto.NgayKy.HasValue, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.NgayKy));

            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(dto.IdKhachHang);

            BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdKhachHang), ErrorCodes.NotFound);

            ValidateContractDates(dto);
        }

        private void ValidateContractDates(TblHopDongThucHien dto)
        {
            DateTime ngayKy = dto.NgayKy.Value.Date;

            if (dto.NgayHieuLuc.HasValue)
            {
                BusinessValidator.ThrowIf(dto.NgayHieuLuc.Value.Date < ngayKy, BackEndResourceKeys.INVALID_DATA, nameof(dto.NgayHieuLuc));
            }

            if (dto.NgayHetHan.HasValue)
            {
                DateTime minimumExpiryDate = dto.NgayHieuLuc.HasValue ? dto.NgayHieuLuc.Value.Date : ngayKy;

                BusinessValidator.ThrowIf(dto.NgayHetHan.Value.Date < minimumExpiryDate, BackEndResourceKeys.INVALID_DATA, nameof(dto.NgayHetHan));
            }
        }

        private void NormalizeContract(TblHopDongThucHien dto)
        {
            dto.SoHopDong = dto.SoHopDong.Trim();
            dto.TenHopDong = dto.TenHopDong.Trim();
            dto.MoTa = string.IsNullOrWhiteSpace(dto.MoTa) ? null : dto.MoTa.Trim();

            if (dto.NgayKy.HasValue)
            {
                dto.NgayKy = dto.NgayKy.Value.Date;
            }

            if (dto.NgayHieuLuc.HasValue)
            {
                dto.NgayHieuLuc = dto.NgayHieuLuc.Value.Date;
            }

            if (dto.NgayHetHan.HasValue)
            {
                dto.NgayHetHan = dto.NgayHetHan.Value.Date;
            }
        }

        #endregion

        #region Get data

        public TblHopDongThucHien GetHopDongById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _repository.GetById(id);
        }

        public TblHopDongThucHien GetBySoHopDong(string soHopDong)
        {
            return _repository.GetBySoHopDong(soHopDong);
        }

        #endregion

        #region Delete

        public bool Delete(TblHopDongThucHien dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);

            TblHopDongThucHien item = _repository.GetById(dto.IdHopDongThucHien);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.NotFound);

            // Theo yêu cầu đã chốt: vẫn cho xóa mềm khi hợp đồng đang liên kết với dự án.
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;

            bool result = _repository.Delete(item);

            BusinessValidator.ThrowIf(!result, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return true;
        }

        #endregion
    }
}