using SweetSoft.QLDA.Core.ExceptionHelpers;
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

namespace SweetSoft.QLDA.Core.Managers
{
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

            item = _repository.Insert(item);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return item;
        }

        private TblHopDongThucHien Update(TblHopDongThucHien dto)
        {
            TblHopDongThucHien item = _repository.GetById(dto.IdHopDongThucHien);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.NotFound);

            ObjectHelper.CopyBusinessProperties(dto, item, x => x.IdHopDongThucHien, x => x.DaXoa, x => x.NguoiTao, x => x.NgayTao, x => x.NguoiCapNhat, x => x.NgayCapNhat);

            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;

            item = _repository.Update(item);

            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return item;
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