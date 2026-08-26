using SweetSoft.QLDA.Core.EnumHelper.Defines;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class DuAnManager: BaseManager
    {
        private static readonly Lazy<DuAnManager> _instance = new Lazy<DuAnManager>(() => new DuAnManager());
        public static DuAnManager Instance => _instance.Value;
        private readonly DuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public DuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new DuAnRepository(_auditManager);
        }

        public DataTable SearchDuAns(string searchTerm, Dictionary<string, object> parameters ,string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, parameters,orderBy, pageNumber, pageSize, out totalRecord);
        }

        public TblDuAn CreateOrUpdate(TblDuAn dto)
        {
            //validate input data
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenDuAn, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenDuAn));
            BusinessValidator.ThrowIf(dto.IdLoaiDuAn == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdLoaiDuAn));
            BusinessValidator.ThrowIf(dto.IdKhachHang == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdKhachHang));
            BusinessValidator.ThrowIf(dto.IdNhanVienQuanLy == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdNhanVienQuanLy));
            BusinessValidator.ThrowIf(dto.NgayDuKienHoanThanh < dto.NgayBatDau, BackEndResourceKeys.INVALID_DATA, nameof(dto.NgayDuKienHoanThanh));

            if (dto.IdHopDongThucHien.HasValue && dto.IdHopDongThucHien != Guid.Empty)
            {
                TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(dto.IdHopDongThucHien.Value);
                BusinessValidator.ThrowIfNull(hopDong, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.NotFound);

                bool hopDongDaSuDung = _repository.IsContractUsed(dto.IdHopDongThucHien.Value, dto.IdDuAn);
                BusinessValidator.ThrowIfNull(hopDongDaSuDung, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdHopDongThucHien), ErrorCodes.Conflict);
            }
            else
            {
                dto.IdHopDongThucHien = null;
            }

            TblDuAn duAn;

            if (dto.IdDuAn != Guid.Empty)
            {
                duAn = _repository.GetById(dto.IdDuAn);
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdDuAn), ErrorCodes.NotFound);

                ObjectHelper.CopyBusinessProperties(
                     dto,
                     duAn,
                     x => x.IdDuAn,
                     x => x.MaDuAn,
                     x => x.NgayHoanThanhThucTe,
                     x => x.DaXoa,
                     x => x.NguoiTao,
                     x => x.NgayTao,
                     x => x.NguoiCapNhat,
                     x => x.NgayCapNhat,
                     x => x.IdHopDongThucHien);

                Guid? idHopDong = dto.IdHopDongThucHien;
                duAn.TrangThai = dto.TrangThai;
                duAn.IdHopDongThucHien = idHopDong.HasValue && idHopDong.Value != Guid.Empty ? idHopDong : null;
                duAn.NguoiCapNhat = SweetContext.Current.UserName;
                duAn.NgayCapNhat = DateTime.UtcNow;
                duAn = _repository.Update(duAn);

                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return duAn;
            }
            else
            {
                duAn = dto.Clone() as TblDuAn;
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.INVALID_DATA);

                duAn.IdDuAn = UUIDv7.NewGuid();
                duAn.MaDuAn = GenerateProjectCode();
                BusinessValidator.ThrowIf(_repository.GetByMaDuAn(duAn.MaDuAn) != null, BackEndResourceKeys.INVALID_DATA, nameof(duAn.MaDuAn), ErrorCodes.Conflict);
                duAn.DaXoa = false;
                duAn.NguoiTao = SweetContext.Current.UserName;
                duAn.NgayTao = DateTime.UtcNow;
                duAn.NguoiCapNhat = null;
                duAn.NgayCapNhat = null;

                duAn = _repository.Insert(duAn);
                BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return duAn;
            }
        }

        public bool Delete(TblDuAn dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            TblDuAn duAn = _repository.GetById(dto.IdDuAn);
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdDuAn), ErrorCodes.NotFound);

            duAn.DaXoa = true;
            duAn.NguoiCapNhat = SweetContext.Current.UserName;
            duAn.NgayCapNhat = DateTime.UtcNow;
            duAn = _repository.Update(duAn);
            BusinessValidator.ThrowIfNull(duAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return true;
        }

        public TblDuAn GetDuAnById(Guid id)
        {
            return _repository.GetById(id);
        }

        public DataTable GetDetailDuAnById(Guid id)
        {
            return _repository.GetDetailById(id);
        }

        public string GenerateProjectCode()
        {
            return _repository.GenerateMaDuAn();
        }
    }
}
