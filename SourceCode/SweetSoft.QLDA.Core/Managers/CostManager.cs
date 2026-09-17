using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// The canonical project document attached to a cost item.
    /// </summary>
    public sealed class CostDocumentLinkResult
    {
        public Guid ProjectId { get; set; }
        public Guid DocumentId { get; set; }
        public bool IsCreated { get; set; }
    }

    public class CostManager : BaseManager
    {
        private static readonly Lazy<CostManager> _instance = new Lazy<CostManager>(() => new CostManager());
        public static CostManager Instance => _instance.Value;
        private readonly CostRepository _repository;
        private readonly AuditManager _auditManager;

        public CostManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new CostRepository(_auditManager);
        }
        public DataTable SearchCost(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchCost(projectId, searchTerm, parameters, orderBy, startRow, endRow, out totalRecord);
        }
        public string GenerateMaChiPhi(Guid projectId)
        {
            return _repository.GenerateMaChiPhi(projectId);
        }
        public string GetValueForTrangThaiChiPhi(TrangThaiChiPhi source)
        {
            switch (source)
            {
                case TrangThaiChiPhi.NotApproved:
                    return "NOT_APPROVED";
                case TrangThaiChiPhi.Approved:
                    return "APPROVED";
                case TrangThaiChiPhi.Rejected:
                    return "REJECTED";
                default:
                    return source.ToString();
            }
        }
        public void DeleteCost(TblChiPhi cost)
        {
            _repository.DeleteCost(cost);
        }
        public TblChiPhi CreateOrUpdate(TblChiPhi dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenKhoanChi, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenKhoanChi));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty || dto.IdDuAn == null, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.IdChiPhi == Guid.Empty);
            TblChiPhi result = null;

            dto.SoTien = (dto.DonGia ?? 0) * (dto.SoLuong ?? 0);

            if (isInsert)
            {
                dto.IdChiPhi = Guid.NewGuid();
                dto.DaXoa = false;
                dto.NgayTao = DateTime.Now;
                dto.IdNhanVienDeNghi = currentUserId;

                dto.MaChiPhi = GenerateMaChiPhi(dto.IdDuAn);

                if (dto.TrangThai == null) dto.TrangThai = 0;

                dto.Save();
                result = dto;
            }
            else
            {
                TblChiPhi existingCost = TblChiPhi.FetchByID(dto.IdChiPhi);
                BusinessValidator.ThrowIfNull(existingCost, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdChiPhi), ErrorCodes.NotFound);

                existingCost.TenKhoanChi = dto.TenKhoanChi;
                existingCost.MoTaChiTiet = dto.MoTaChiTiet;
                existingCost.DonGia = dto.DonGia;
                existingCost.SoLuong = dto.SoLuong;
                existingCost.SoTien = dto.SoTien;
                existingCost.TrangThai = dto.TrangThai;

                existingCost.Save();
                result = existingCost;
            }

            return result;
        }

        #region Cost document

        /// <summary>
        /// Creates one canonical project document for a cost item and returns
        /// its ids so the caller can open the document version screen. The
        /// cost name is a snapshot at creation time; later cost edits do not
        /// silently overwrite document metadata.
        /// </summary>
        public CostDocumentLinkResult GetOrCreateProjectDocument(Guid idChiPhi)
        {
            if (idChiPhi == Guid.Empty)
            {
                throw new ArgumentException(
                    "Khoản chi không hợp lệ.",
                    nameof(idChiPhi));
            }

            if (!_repository.HasDocumentLinkColumn())
            {
                throw new InvalidOperationException(
                    "Chưa cài cấu trúc liên kết hồ sơ cho chi phí. Hãy chạy Database/AddCostDocumentLink.sql trước.");
            }

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.Serializable
            };

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                options))
            {
                TblChiPhi cost = _repository.GetById(idChiPhi);
                if (cost == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy khoản chi hoặc khoản chi đã bị xóa.");
                }

                TblDuAn project = DuAnManager.Instance.GetDuAnById(cost.IdDuAn);
                if (project == null)
                {
                    throw new InvalidOperationException(
                        "Dự án của khoản chi không tồn tại hoặc đã bị xóa.");
                }

                if (!DocumentManager.Instance.CanAccessProjectDocument(
                    project.IdDuAn,
                    ActionKeys.View))
                {
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền xem hồ sơ của dự án này.");
                }

                Guid? linkedDocumentId = _repository.GetLinkedDocumentIdForUpdate(
                    cost.IdChiPhi);
                if (linkedDocumentId.HasValue)
                {
                    TblTaiLieu existingDocument = DocumentManager.Instance
                        .GetProjectDocumentById(
                            linkedDocumentId.Value,
                            project.IdDuAn);
                    if (existingDocument == null)
                    {
                        throw new InvalidOperationException(
                            "Hồ sơ liên kết với khoản chi không còn thuộc dự án hiện tại. Vui lòng kiểm tra lại dữ liệu liên kết.");
                    }

                    scope.Complete();
                    return new CostDocumentLinkResult
                    {
                        ProjectId = project.IdDuAn,
                        DocumentId = existingDocument.IdTaiLieu,
                        IsCreated = false
                    };
                }

                if (!DocumentManager.Instance.CanAccessProjectDocument(
                    project.IdDuAn,
                    ActionKeys.Create))
                {
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền tạo hồ sơ cho dự án này.");
                }

                TblLoaiTaiLieu documentType = GetConfiguredCostDocumentType();
                TblTaiLieu document = DocumentManager.Instance.SaveProjectDocument(
                    project.IdDuAn,
                    Guid.Empty,
                    documentType.IdLoaiTaiLieu,
                    project.IdNhanVienQuanLy,
                    BuildCostDocumentCode(cost),
                    BuildCostDocumentTitle(cost),
                    BuildCostDocumentDescription(cost),
                    documentType.CanTrinhKy,
                    documentType.CanTrinhKy
                        ? documentType.HinhThucKyMacDinh
                        : null,
                    documentType.CanGuiKhachHang,
                    documentType.CanLuuVatLy);

                if (!_repository.TryLinkDocument(cost.IdChiPhi, document.IdTaiLieu))
                {
                    throw new InvalidOperationException(
                        "Không thể liên kết hồ sơ vừa tạo với khoản chi. Dữ liệu đã được hoàn tác.");
                }

                WriteCostDocumentLinkAudit(cost, project, document);
                scope.Complete();

                return new CostDocumentLinkResult
                {
                    ProjectId = project.IdDuAn,
                    DocumentId = document.IdTaiLieu,
                    IsCreated = true
                };
            }
        }

        private TblLoaiTaiLieu GetConfiguredCostDocumentType()
        {
            Guid idLoaiTaiLieu = SettingManager.Instance.GetSettingValueGuid(
                SettingKeys.CostDocumentTypeId);
            if (idLoaiTaiLieu == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình loại tài liệu Chi phí. Vui lòng cấu hình Settings.CostDocumentTypeId trước.");
            }

            TblLoaiTaiLieu documentType = DocumentManager.Instance
                .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (documentType == null || documentType.DaXoa || !documentType.KichHoat)
            {
                throw new InvalidOperationException(
                    "Loại tài liệu Chi phí được cấu hình không tồn tại hoặc đang bị khóa.");
            }

            return documentType;
        }

        private static string BuildCostDocumentCode(TblChiPhi cost)
        {
            const int maxLength = 100;
            string costCode = (cost.MaChiPhi ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (string.IsNullOrEmpty(costCode))
            {
                costCode = "COST";
            }

            string suffix = "-" + cost.IdChiPhi
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();
            int costCodeLength = maxLength - "CP-".Length - suffix.Length;
            if (costCode.Length > costCodeLength)
            {
                costCode = costCode.Substring(0, costCodeLength).TrimEnd();
            }

            return "CP-" + costCode + suffix;
        }

        private static string BuildCostDocumentTitle(TblChiPhi cost)
        {
            const int maxLength = 255;
            const string prefix = "Chi phí ";
            const string separator = " – ";
            string costCode = (cost.MaChiPhi ?? string.Empty).Trim();
            string costName = (cost.TenKhoanChi ?? string.Empty).Trim();
            int costNameLength = maxLength
                - prefix.Length
                - costCode.Length
                - separator.Length;

            if (costNameLength <= 0)
            {
                return (prefix + costCode).Substring(0, maxLength);
            }

            if (costName.Length > costNameLength)
            {
                costName = costName.Substring(0, costNameLength).TrimEnd();
            }

            return prefix + costCode + separator + costName;
        }

        private static string BuildCostDocumentDescription(TblChiPhi cost)
        {
            return "Hồ sơ chi phí được tạo tự động từ khoản chi "
                + (cost.MaChiPhi ?? string.Empty).Trim()
                + ".";
        }

        private void WriteCostDocumentLinkAudit(
            TblChiPhi cost,
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
                        nameof(TblChiPhi),
                        cost.IdChiPhi,
                        SweetContext.Current.UserName,
                        document.IdTaiLieu,
                        cost.MaChiPhi,
                        "Đã tạo và liên kết hồ sơ chi phí "
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
                    "Failed to log cost-document link for "
                        + cost.IdChiPhi);
            }
        }

        #endregion
    }
}
