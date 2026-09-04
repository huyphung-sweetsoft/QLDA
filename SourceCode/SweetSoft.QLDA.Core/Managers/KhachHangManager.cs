using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;

namespace SweetSoft.QLDA.Core.Managers
{
    public class KhachHangManager : BaseManager
    {
        private static readonly Lazy<KhachHangManager> _instance = new Lazy<KhachHangManager>(() => new KhachHangManager());

        public static KhachHangManager Instance => _instance.Value;
        private readonly KhachHangRepository _repository;
        private readonly AuditManager _auditManager;

        public KhachHangManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new KhachHangRepository(_auditManager);
        }

        public DataTable SearchKhachHangs(string searchTerms, Dictionary<string, object> parameters, string orderBy, int pageNum, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerms, parameters, orderBy, pageNum, pageSize, out totalRecord);
        }

        public TblKhachHang CreateOrUpdate(TblKhachHang dto)
        {
            // Validate input data
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenKhachHang, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            BusinessValidator.ThrowIfNullOrEmpty(dto.IdSoThue, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            BusinessValidator.ThrowIfNullOrEmpty(dto.SoDienThoai, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            BusinessValidator.ThrowIfNullOrEmpty(dto.Email, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            BusinessValidator.ThrowIfNullOrEmpty(dto.DiaChi, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE);
            BusinessValidator.ThrowIf(dto.IdLoaiKhachHang == Guid.Empty, BackEndResourceKeys.PLEASE_SELECT_THE_VALUE, nameof(dto.IdLoaiKhachHang));

            TblKhachHang khachHang;

            if (dto.IdKhachHang != Guid.Empty)
            {
                khachHang = _repository.GetById(dto.IdKhachHang);
                BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdKhachHang), ErrorCodes.NotFound);

                ObjectHelper.CopyBusinessProperties(
                    dto,
                    khachHang,
                    x => x.IdKhachHang,
                    x => x.DaXoa,
                    x => x.NguoiTao,
                    x => x.NgayTao,
                    x => x.NguoiCapNhat,
                    x => x.NgayCapNhat);
                khachHang.KichHoat = dto.KichHoat;
                khachHang.NguoiCapNhat = SweetContext.Current.UserName;
                khachHang.NgayCapNhat = DateTime.UtcNow;
                khachHang = _repository.Update(khachHang);
                BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return khachHang;
            }
            else
            {
                khachHang = dto.Clone() as TblKhachHang;
                BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.INVALID_DATA);

                khachHang.IdKhachHang = UUIDv7.NewGuid();
                khachHang.DaXoa = false;
                khachHang.NguoiTao = SweetContext.Current.UserName;
                khachHang.NgayTao = DateTime.UtcNow;

                khachHang = _repository.Insert(khachHang);
                BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return khachHang;
            }

        }

        public bool Delete(TblKhachHang dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            TblKhachHang khachHang = _repository.GetById(dto.IdKhachHang);
            BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.NOT_FOUND, nameof(dto), ErrorCodes.ServiceUnavailable);

            khachHang.DaXoa = true;
            khachHang.NguoiCapNhat = SweetContext.Current.UserName;
            khachHang.NgayCapNhat = DateTime.UtcNow;
            khachHang = _repository.Update(khachHang);
            BusinessValidator.ThrowIfNull(khachHang, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);

            return true;
        }

        public TblKhachHang GetKhachHangById(Guid id)
        {
            return _repository.GetById(id);
        }
       
        public DataTable GetDetailKhachHangById(Guid id)
        {
            return _repository.GetDetailById(id);  
        }

        public List<TblKhachHang> GetAllKhachHang()
        {
            return _repository.GetAllTblKhachHang();
        }
    }
}
