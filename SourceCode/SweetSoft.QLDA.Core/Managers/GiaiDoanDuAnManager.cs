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
                item =
    _repository.GetById(
        dto.IdGiaiDoanDuAn);

                BusinessValidator.ThrowIfNull(
                    item,
                    BackEndResourceKeys.NOT_FOUND,
                    nameof(dto.IdGiaiDoanDuAn),
                    ErrorCodes.NotFound);

                BusinessValidator.ThrowIf(
                    item.IdDuAn != dto.IdDuAn,
                    BackEndResourceKeys.INVALID_DATA,
                    nameof(dto.IdDuAn),
                    ErrorCodes.Conflict);

                TblCongViec rootTask =
                    TaskManager.Instance
                        .GetRootTaskByStageId(
                            item.IdGiaiDoanDuAn);

                BusinessValidator.ThrowIfNull(
                    rootTask,
                    BackEndResourceKeys.NOT_FOUND,
                    nameof(item.IdGiaiDoanDuAn),
                    ErrorCodes.NotFound);

                bool hasChildTasks =
                    TaskManager.Instance
                        .CheckHasChildTasks(
                            item.IdDuAn,
                            rootTask);

                DateTime? oldStartDate =
                    item.NgayBatDau;

                /*
                 * Kiểm tra ngày bắt đầu với giai đoạn trước và sau.
                 */
                ValidateStageTimeline(
                    dto,
                    item.ThuTuGiaiDoan);

                /*
                 * Chỉ kiểm tra ngày dự kiến do người dùng nhập
                 * khi chưa có công việc con.
                 */
                if (!hasChildTasks)
                {
                    BusinessValidator.ThrowIf(
                        !dto.NgayDuKienHoanThanh.HasValue,
                        BackEndResourceKeys.PLEASE_ENTER_THE_VALUE,
                        nameof(dto.NgayDuKienHoanThanh));

                    BusinessValidator.ThrowIf(
                        dto.NgayBatDau.HasValue &&
                        dto.NgayDuKienHoanThanh.Value.Date <
                            dto.NgayBatDau.Value.Date,
                        BackEndResourceKeys.INVALID_DATA,
                        nameof(dto.NgayDuKienHoanThanh));
                }

                item.IdGiaiDoan =
                    dto.IdGiaiDoan;

                item.TenGiaiDoanTuyChinh =
                    string.IsNullOrWhiteSpace(
                        dto.TenGiaiDoanTuyChinh)
                            ? null
                            : dto.TenGiaiDoanTuyChinh.Trim();

                item.NgayBatDau =
                    dto.NgayBatDau;

                if (!hasChildTasks)
                {
                    item.NgayDuKienHoanThanh =
                        dto.NgayDuKienHoanThanh;
                }

                item.NgayHoanThanhThucTe =
                    dto.NgayHoanThanhThucTe;

                item.MoTa =
                    string.IsNullOrWhiteSpace(dto.MoTa)
                        ? null
                        : dto.MoTa.Trim();

                item.NguoiCapNhat =
                    SweetContext.Current.UserName;

                item.NgayCapNhat =
                    DateTime.UtcNow;

                string stageName =
                    GetStageDisplayName(item);

                BusinessValidator.ThrowIf(
                    string.IsNullOrWhiteSpace(stageName),
                    BackEndResourceKeys.INVALID_DATA,
                    nameof(dto.TenGiaiDoanTuyChinh));

                TblCongViec updatedStageTask =
                    TaskManager.Instance
                        .UpdateStageTask(
                            item,
                            stageName,
                            oldStartDate);

                BusinessValidator.ThrowIfNull(
                    updatedStageTask,
                    BackEndResourceKeys.SERVICE_UNAVAILABLE,
                    nameof(dto),
                    ErrorCodes.ServiceUnavailable);

                /*
                 * Nếu có công việc con, lấy ngày kết thúc
                 * đã được TaskManager tính lại.
                 */
                if (hasChildTasks)
                {
                    item.NgayDuKienHoanThanh =
                        updatedStageTask.NgayKetThuc;
                }

                /*
                 * Không gán item.ThuTuGiaiDoan.
                 */
                item =
                    _repository.Update(item);
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

                // Đồng bộ sang TblCongViec như một công việc cha
                string tenGiaiDoan = isCommonStage
                    ? GiaiDoanManager.Instance.GetById(item.IdGiaiDoan.Value)?.TenGiaiDoan
                    : item.TenGiaiDoanTuyChinh;

                CreateCongViecFromGiaiDoan(item, tenGiaiDoan ?? string.Empty);
            }

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
            return item;
        }

        public TblGiaiDoanDuAn GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public DataTable GetByIdDuAn(Guid idDuAn)
        {
            return _repository.GetByIdDuAn(idDuAn);
        }

        public int GetNextOrder(Guid idDuAn)
        {
            return _repository.GetNextOrder(idDuAn);
        }

        private void CreateCongViecFromGiaiDoan(TblGiaiDoanDuAn giaiDoan, string tenGiaiDoan)
        {
            TblCongViec congViec = new TblCongViec();

            congViec.IdCongViec = UUIDv7.NewGuid();
            congViec.IdDuAn = giaiDoan.IdDuAn;
            congViec.IdGiaiDoanDuAn = giaiDoan.IdGiaiDoanDuAn;
            congViec.IdCongViecCha = null;
            congViec.IdCongViecPhuThuoc = null;
            congViec.IdDoUuTien = null;
            congViec.MaCongViec = giaiDoan.ThuTuGiaiDoan.ToString();
            congViec.TenCongViec = tenGiaiDoan;
            congViec.MoTa = giaiDoan.MoTa;
            congViec.NgayBatDau = giaiDoan.NgayBatDau;
            congViec.NgayKetThuc = giaiDoan.NgayDuKienHoanThanh;
            congViec.NgayHoanThanhThucTe = giaiDoan.NgayHoanThanhThucTe;
            congViec.PhanTramHoanThanh = 0;
            congViec.TrangThai = (byte)0;
            congViec.DaXoa = false;
            congViec.NguoiTao = SweetContext.Current.UserName;
            congViec.NgayTao = DateTime.UtcNow;
            congViec.NguoiCapNhat = null;
            congViec.NgayCapNhat = null;

            if (giaiDoan.NgayBatDau.HasValue && giaiDoan.NgayDuKienHoanThanh.HasValue)
            {
                congViec.ThoiHanNgay =
                    (giaiDoan.NgayDuKienHoanThanh.Value.Date -
                     giaiDoan.NgayBatDau.Value.Date).Days + 1;
            }

            congViec.Save();
        }

        private void ValidateStageTimeline(
    TblGiaiDoanDuAn dto,
    int currentOrder)
        {
            BusinessValidator.ThrowIf(
                !dto.NgayBatDau.HasValue,
                BackEndResourceKeys.PLEASE_ENTER_THE_VALUE,
                nameof(dto.NgayBatDau));

            DateTime startDate =
                dto.NgayBatDau.Value.Date;

            BusinessValidator.ThrowIf(
                dto.NgayHoanThanhThucTe.HasValue &&
                dto.NgayHoanThanhThucTe.Value.Date <
                    startDate,
                BackEndResourceKeys.INVALID_DATA,
                nameof(dto.NgayHoanThanhThucTe));

            DateTime? previousStartDate =
                _repository.GetPreviousStageStartDate(
                    dto.IdDuAn,
                    currentOrder);

            BusinessValidator.ThrowIf(
                previousStartDate.HasValue &&
                startDate <
                    previousStartDate.Value.Date,
                BackEndResourceKeys.INVALID_DATA,
                nameof(dto.NgayBatDau),
                ErrorCodes.Conflict);

            DateTime? nextStartDate =
                _repository.GetNextStageStartDate(
                    dto.IdDuAn,
                    currentOrder);

            BusinessValidator.ThrowIf(
                nextStartDate.HasValue &&
                startDate >
                    nextStartDate.Value.Date,
                BackEndResourceKeys.INVALID_DATA,
                nameof(dto.NgayBatDau),
                ErrorCodes.Conflict);
        }

        private string GetStageDisplayName(
    TblGiaiDoanDuAn stage)
        {
            if (stage == null)
                return null;

            if (!string.IsNullOrWhiteSpace(
                stage.TenGiaiDoanTuyChinh))
            {
                return stage
                    .TenGiaiDoanTuyChinh
                    .Trim();
            }

            if (stage.IdGiaiDoan.HasValue &&
                stage.IdGiaiDoan.Value != Guid.Empty)
            {
                TblGiaiDoan commonStage =
                    GiaiDoanManager.Instance
                        .GetById(
                            stage.IdGiaiDoan.Value);

                if (commonStage != null &&
                    !string.IsNullOrWhiteSpace(
                        commonStage.TenGiaiDoan))
                {
                    return commonStage
                        .TenGiaiDoan
                        .Trim();
                }
            }

            return null;
        }
    }
}
