using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;

namespace SweetSoft.QLDA.Core.Managers
{
    public class GiaiDoanDuAnManager : BaseManager
    {
        private static readonly Lazy<GiaiDoanDuAnManager> _instance = new Lazy<GiaiDoanDuAnManager>(() => new GiaiDoanDuAnManager());
        public static GiaiDoanDuAnManager Instance => _instance.Value;

        private readonly GiaiDoanDuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public GiaiDoanDuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new GiaiDoanDuAnRepository(_auditManager);
        }

        public TblGiaiDoanDuAn CreateOrUpdate(TblGiaiDoanDuAn dto)
        {
            // 1. Validate null và dự án
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            // 2. Xác định loại giai đoạn
            bool isCommonStage = dto.IdGiaiDoan.HasValue && dto.IdGiaiDoan.Value != Guid.Empty;
            bool isCustomStage = !string.IsNullOrWhiteSpace(dto.TenGiaiDoanTuyChinh);

            // Chỉ được phép chọn một trong hai: giai đoạn chung hoặc giai đoạn tùy chỉnh
            BusinessValidator.ThrowIf(isCommonStage == isCustomStage, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdGiaiDoan));

            // 3. Chuẩn hóa dữ liệu theo loại giai đoạn
            if (isCommonStage)
            {
                dto.TenGiaiDoanTuyChinh = null;

                TblGiaiDoan commonStage = GiaiDoanManager.Instance.GetById(dto.IdGiaiDoan.Value);
                BusinessValidator.ThrowIfNull(commonStage, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdGiaiDoan), ErrorCodes.NotFound);
            }
            else
            {
                dto.IdGiaiDoan = null;
                dto.TenGiaiDoanTuyChinh = dto.TenGiaiDoanTuyChinh.Trim();
                BusinessValidator.ThrowIf(dto.TenGiaiDoanTuyChinh.Length > 250, BackEndResourceKeys.INVALID_DATA, nameof(dto.TenGiaiDoanTuyChinh));
            }

            // 4. Validate thời gian
            BusinessValidator.ThrowIf(
                dto.NgayBatDau.HasValue && dto.NgayDuKienHoanThanh.HasValue &&
                dto.NgayDuKienHoanThanh.Value.Date < dto.NgayBatDau.Value.Date,
                BackEndResourceKeys.INVALID_DATA,
                nameof(dto.NgayDuKienHoanThanh));

            BusinessValidator.ThrowIf(
                dto.NgayBatDau.HasValue && dto.NgayHoanThanhThucTe.HasValue &&
                dto.NgayHoanThanhThucTe.Value.Date < dto.NgayBatDau.Value.Date,
                BackEndResourceKeys.INVALID_DATA,
                nameof(dto.NgayHoanThanhThucTe));

            // 5. Kiểm tra dự án tồn tại
            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(dto.IdDuAn);
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdDuAn), ErrorCodes.NotFound);

            // 6. Kiểm tra trùng giai đoạn
            bool duplicateStage = isCommonStage
                ? _repository.IsCommonStageExists(dto.IdDuAn, dto.IdGiaiDoan.Value, dto.IdGiaiDoanDuAn)
                : _repository.IsCustomStageExists(dto.IdDuAn, dto.TenGiaiDoanTuyChinh, dto.IdGiaiDoanDuAn);

            BusinessValidator.ThrowIf(duplicateStage, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdGiaiDoan), ErrorCodes.Conflict);

            TblGiaiDoanDuAn item;

            if (dto.IdGiaiDoanDuAn != Guid.Empty)
            {
                // Cập nhật
                item = _repository.GetById(dto.IdGiaiDoanDuAn);
                BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdGiaiDoanDuAn), ErrorCodes.NotFound);

                // Không cho chuyển giai đoạn sang dự án khác
                BusinessValidator.ThrowIf(item.IdDuAn != dto.IdDuAn, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn), ErrorCodes.Conflict);

                int order = dto.ThuTuGiaiDoan > 0 ? dto.ThuTuGiaiDoan : item.ThuTuGiaiDoan;
                bool duplicateOrder = _repository.IsOrderExists(dto.IdDuAn, order, dto.IdGiaiDoanDuAn);
                BusinessValidator.ThrowIf(duplicateOrder, BackEndResourceKeys.INVALID_DATA, nameof(dto.ThuTuGiaiDoan), ErrorCodes.Conflict);

                item.IdGiaiDoan = dto.IdGiaiDoan;
                item.TenGiaiDoanTuyChinh = dto.TenGiaiDoanTuyChinh;
                item.NgayBatDau = dto.NgayBatDau;
                item.NgayDuKienHoanThanh = dto.NgayDuKienHoanThanh;
                item.NgayHoanThanhThucTe = dto.NgayHoanThanhThucTe;
                item.ThuTuGiaiDoan = order;
                item.MoTa = string.IsNullOrWhiteSpace(dto.MoTa) ? null : dto.MoTa.Trim();
                item.NguoiCapNhat = SweetContext.Current.UserName;
                item.NgayCapNhat = DateTime.UtcNow;

                item = _repository.Update(item);
            }
            else
            {
                // Thêm mới
                item = dto.Clone() as TblGiaiDoanDuAn;
                BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);

                item.IdGiaiDoanDuAn = UUIDv7.NewGuid();

                if (item.ThuTuGiaiDoan <= 0)
                    item.ThuTuGiaiDoan = _repository.GetNextOrder(item.IdDuAn);

                bool duplicateOrder = _repository.IsOrderExists(item.IdDuAn, item.ThuTuGiaiDoan, Guid.Empty);
                BusinessValidator.ThrowIf(duplicateOrder, BackEndResourceKeys.INVALID_DATA, nameof(dto.ThuTuGiaiDoan), ErrorCodes.Conflict);

                item.DaXoa = false;
                item.NguoiTao = SweetContext.Current.UserName;
                item.NgayTao = DateTime.UtcNow;
                item.NguoiCapNhat = null;
                item.NgayCapNhat = null;

                item = _repository.Insert(item);
            }

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
            return item;
        }

        public DataTable GetByIdDuAn(Guid idDuAn)
        {
            return _repository.GetByIdDuAn(idDuAn);
        }
    }
}
