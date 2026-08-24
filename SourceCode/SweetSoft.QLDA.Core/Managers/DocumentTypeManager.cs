using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Managers
{
    public static class DocumentSigningMethodKeys
    {
        public const string Paper = "GIAY";
        public const string DigitalExternal = "DIEN_TU";
    }

    public class DocumentTypeManager : BaseManager
    {
        private static readonly Lazy<DocumentTypeManager> _instance =
            new Lazy<DocumentTypeManager>(
                () => new DocumentTypeManager());

        private readonly DocumentTypeRepository _repository;
        private readonly DocumentGroupRepository _groupRepository;

        public static DocumentTypeManager Instance
        {
            get { return _instance.Value; }
        }

        public DocumentTypeManager(
            IAppContext applicationContext = null)
            : base(applicationContext)
        {
            AuditManager auditManager =
                new AuditManager(GetClientInfo());

            _repository =
                new DocumentTypeRepository(auditManager);

            _groupRepository =
                new DocumentGroupRepository(auditManager);
        }

        /// <summary>
        /// Lấy danh sách loại tài liệu chưa bị xóa.
        /// Có thể tìm theo tên, mô tả và lọc theo nhóm tài liệu.
        /// </summary>
        public List<TblLoaiTaiLieu> GetAll(
            string keyword = null,
            Guid? idNhomTaiLieu = null)
        {
            return _repository.GetAll(
                keyword,
                idNhomTaiLieu);
        }

        /// <summary>
        /// Lấy một loại tài liệu chưa bị xóa theo khóa chính.
        /// </summary>
        public TblLoaiTaiLieu GetById(Guid idLoaiTaiLieu)
        {
            return _repository.GetById(idLoaiTaiLieu);
        }

        /// <summary>
        /// Kiểm tra tên loại tài liệu đã tồn tại trong cùng nhóm chưa.
        /// excludeId dùng để bỏ qua chính bản ghi đang cập nhật.
        /// </summary>
        public bool IsNameExisted(
            string tenLoai,
            Guid idNhomTaiLieu,
            Guid excludeId)
        {
            return _repository.IsNameExisted(
                tenLoai,
                idNhomTaiLieu,
                excludeId);
        }

        public bool IsInUse(Guid idLoaiTaiLieu)
        {
            return _repository.IsInUse(idLoaiTaiLieu);
        }

        public TblLoaiTaiLieu Save(
            Guid idLoaiTaiLieu,
            Guid idNhomTaiLieu,
            string tenLoai,
            string moTa,
            bool canTrinhKy,
            string hinhThucKyMacDinh,
            bool canGuiKhachHang,
            bool canLuuVatLy,
            int thuTuHienThi,
            bool kichHoat)
        {
            tenLoai = (tenLoai ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();
            hinhThucKyMacDinh =
                (hinhThucKyMacDinh ?? string.Empty).Trim();

            if (idNhomTaiLieu == Guid.Empty)
            {
                throw new ArgumentException(
                    "Vui lòng chọn nhóm tài liệu.");
            }

            if (string.IsNullOrEmpty(tenLoai))
            {
                throw new ArgumentException(
                    "Tên loại tài liệu không được để trống.");
            }

            if (tenLoai.Length > 150)
            {
                throw new ArgumentException(
                    "Tên loại tài liệu không được vượt quá 150 ký tự.");
            }

            if (moTa.Length > 500)
            {
                throw new ArgumentException(
                    "Mô tả không được vượt quá 500 ký tự.");
            }

            if (thuTuHienThi < 0)
            {
                throw new ArgumentException(
                    "Thứ tự hiển thị không được nhỏ hơn 0.");
            }

            TblLoaiTaiLieu item = null;

            if (idLoaiTaiLieu != Guid.Empty)
            {
                item = _repository.GetById(idLoaiTaiLieu);

                if (item == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy loại tài liệu.");
                }
            }

            TblNhomTaiLieu group =
                _groupRepository.GetById(idNhomTaiLieu);

            if (group == null)
            {
                throw new InvalidOperationException(
                    "Nhóm tài liệu không tồn tại hoặc đã bị xóa.");
            }

            bool isChangingToInactiveGroup =
                !group.KichHoat
                && (item == null
                    || item.IdNhomTaiLieu != group.IdNhomTaiLieu);

            if (isChangingToInactiveGroup)
            {
                throw new InvalidOperationException(
                    "Không thể chọn nhóm tài liệu đang bị khóa.");
            }

            if (_repository.IsNameExisted(
                    tenLoai,
                    idNhomTaiLieu,
                    idLoaiTaiLieu))
            {
                throw new InvalidOperationException(
                    "Tên loại tài liệu đã tồn tại trong nhóm đã chọn.");
            }

            if (canTrinhKy
                && !IsValidSigningMethod(hinhThucKyMacDinh))
            {
                throw new ArgumentException(
                    "Hình thức ký mặc định không hợp lệ.");
            }

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();

            if (item == null)
            {
                item = new TblLoaiTaiLieu
                {
                    IdLoaiTaiLieu = UUIDv7.NewGuid(),
                    NguoiTao = currentUserName,
                    NgayTao = currentDate,
                    DaXoa = false
                };
            }
            else
            {
                item.NguoiCapNhat = currentUserName;
                item.NgayCapNhat = currentDate;
            }

            item.IdNhomTaiLieu = idNhomTaiLieu;
            item.TenLoai = tenLoai;
            item.MoTa = moTa;
            item.CanTrinhKy = canTrinhKy;
            item.HinhThucKyMacDinh = canTrinhKy
                ? hinhThucKyMacDinh
                : null;
            item.CanGuiKhachHang = canGuiKhachHang;
            item.CanLuuVatLy = canLuuVatLy;
            item.ThuTuHienThi = thuTuHienThi;
            item.KichHoat = kichHoat;

            if (idLoaiTaiLieu == Guid.Empty)
                return _repository.Insert(item);

            return _repository.Update(item);
        }

        public bool Delete(Guid idLoaiTaiLieu)
        {
            TblLoaiTaiLieu item =
                _repository.GetById(idLoaiTaiLieu);

            if (item == null)
                return false;

            if (_repository.IsInUse(idLoaiTaiLieu))
            {
                throw new InvalidOperationException(
                    "Loại tài liệu đang được tài liệu hoặc mẫu tài liệu sử dụng.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;

            return _repository.Delete(item);
        }

        private static bool IsValidSigningMethod(string signingMethod)
        {
            return string.Equals(
                       signingMethod,
                       DocumentSigningMethodKeys.Paper,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       signingMethod,
                       DocumentSigningMethodKeys.DigitalExternal,
                       StringComparison.OrdinalIgnoreCase);
        }

        private string GetCurrentUserName()
        {
            if (_applicationContext == null
                || string.IsNullOrWhiteSpace(
                    _applicationContext.UserName))
            {
                return "[System]";
            }

            return _applicationContext.UserName;
        }
    }
}
