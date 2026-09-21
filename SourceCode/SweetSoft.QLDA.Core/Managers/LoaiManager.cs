using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class LoaiManager : BaseManager
    {
        public static class LoaiDoiTuong
        {
            public const string KhachHang = "KHACH_HANG";
            public const string DuAn = "DU_AN";
            public const string PhongBan = "PHONG_BAN";
            public const string ChucDanh = "CHUC_DANH";
        }
        private static readonly Lazy<LoaiManager> _instance = new Lazy<LoaiManager>(() => new LoaiManager());

        public static LoaiManager Instance => _instance.Value;
        private readonly LoaiRepository _repository;
        private readonly AuditManager _auditManager;

        public LoaiManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new LoaiRepository(_auditManager);
        }

        public TblLoai GetLoaiById(Guid id)
        {
            return _repository.GetById(id);
        }

        public List<TblLoai> GetByDoiTuong(string doiTuong)
        {
            return _repository.GetByDoiTuong(doiTuong);
        }

        public TblLoai InsertLoai(TblLoai entity)
        {
            if (string.IsNullOrWhiteSpace(entity.TenLoai))
                throw new ArgumentException("Tên loại không được để trống.");

            entity.TenLoai = entity.TenLoai.Trim();

            if (_repository.IsDuplicate(entity.DoiTuong, entity.TenLoai))
                throw new InvalidOperationException($"Tên '{entity.TenLoai}' đã tồn tại trong đối tượng '{entity.DoiTuong}'.");

            entity.IdLoai = Guid.NewGuid();
            entity.NgayTao = DateTime.Now;
            if (string.IsNullOrEmpty(entity.NguoiTao))
                entity.NguoiTao = SweetContext.Current.UserName;
                
            entity.DaXoa = false;
            
            // Set max ThuTuHienThi
            var currentList = GetByDoiTuong(entity.DoiTuong);
            if (currentList.Any())
                entity.ThuTuHienThi = currentList.Max(x => x.ThuTuHienThi) + 1;
            else
                entity.ThuTuHienThi = 1;

            return _repository.Insert(entity);
        }

        public TblLoai UpdateLoai(TblLoai entity)
        {
            if (string.IsNullOrWhiteSpace(entity.TenLoai))
                throw new ArgumentException("Tên loại không được để trống.");

            entity.TenLoai = entity.TenLoai.Trim();

            if (_repository.IsDuplicate(entity.DoiTuong, entity.TenLoai, entity.IdLoai))
                throw new InvalidOperationException($"Tên '{entity.TenLoai}' đã tồn tại trong đối tượng '{entity.DoiTuong}'.");

            entity.NgayCapNhat = DateTime.Now;
            if (string.IsNullOrEmpty(entity.NguoiCapNhat))
                entity.NguoiCapNhat = SweetContext.Current.UserName;

            return _repository.Update(entity);
        }

        public bool DeleteLoai(Guid idLoai)
        {
            return _repository.DeleteLoai(idLoai);
        }
    }
}
