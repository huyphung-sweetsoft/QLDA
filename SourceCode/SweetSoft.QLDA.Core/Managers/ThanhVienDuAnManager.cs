using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class ThanhVienDuAnManager :BaseManager
    {
        private static readonly Lazy<ThanhVienDuAnManager> _instance = new Lazy<ThanhVienDuAnManager>(() => new ThanhVienDuAnManager());
        public static ThanhVienDuAnManager Instance => _instance.Value;
        private readonly ThanhVienDuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public ThanhVienDuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new ThanhVienDuAnRepository(_auditManager);
        }

        public TblThanhVienDuAn AddOrUpdate(TblThanhVienDuAn dto)
        {
            BusinessValidator.ThrowIf(dto.IdNhanVien == Guid.Empty, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIf(dto.IdVaiTroDuAn == Guid.Empty, BackEndResourceKeys.INVALID_DATA);

            TblThanhVienDuAn thanhVienDuAn = _repository.GetNhanVienIsActiveInDuAn(dto.IdNhanVien.Value, dto.IdDuAn);

            if (thanhVienDuAn != null)
            {
                thanhVienDuAn.IdVaiTroDuAn = dto.IdVaiTroDuAn;
                thanhVienDuAn.GhiChu = dto.GhiChu;
                thanhVienDuAn.NguoiCapNhat = SweetContext.Current.UserName;
                thanhVienDuAn.NgayCapNhat = DateTime.UtcNow;
                return _repository.Save(thanhVienDuAn);
            }
            else
            {
                thanhVienDuAn = dto.Clone() as TblThanhVienDuAn;
                BusinessValidator.ThrowIfNull(thanhVienDuAn, BackEndResourceKeys.INVALID_DATA);

                thanhVienDuAn.IdThanhVienDuAn = UUIDv7.NewGuid();
                thanhVienDuAn.DaXoa = false;
                thanhVienDuAn.NgayThamGia = DateTime.UtcNow;
                thanhVienDuAn.NguoiTao = SweetContext.Current.UserName;
                thanhVienDuAn.NgayTao = DateTime.UtcNow;
                thanhVienDuAn.NguoiCapNhat = null;
                thanhVienDuAn.NgayCapNhat = null;
                thanhVienDuAn = _repository.Save(thanhVienDuAn);
                BusinessValidator.ThrowIfNull(thanhVienDuAn, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return _repository.Save(thanhVienDuAn);
            }
        }
    }
}
