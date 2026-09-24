using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;

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
        /// Tìm kiếm nhanh loại tài liệu,
        /// có hỗ trợ bộ lọc và phân trang.
        /// </summary>
        public DataTable SearchDocumentTypes(
            string searchTerm,
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchPaging(
                searchTerm,
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }
        /// <summary>
        /// Tìm kiếm nâng cao loại tài liệu,
        /// có hỗ trợ phân trang.
        /// </summary>
        public DataTable SearchDocumentTypes(
            Dictionary<string, object> parameters,
            string orderBy,
            int rowOffset,
            int endRow,
            out int totalRecord)
        {
            return _repository.SearchPaging(
                parameters,
                orderBy,
                rowOffset,
                endRow,
                out totalRecord);
        }

        /// <summary>
        /// Lấy một loại tài liệu chưa bị xóa theo khóa chính.
        /// </summary>
        public TblLoaiTaiLieu GetById(Guid idLoaiTaiLieu)
        {
            return _repository.GetById(idLoaiTaiLieu);
        }

        /// <summary>
        /// Kiểm tra tên loại hồ sơ đã tồn tại. Tham số nhóm giữ để tương thích caller cũ.
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
            bool kichHoat,
            Guid? defaultStorageLocation = null)
        {
            tenLoai = (tenLoai ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();
            hinhThucKyMacDinh =
                (hinhThucKyMacDinh ?? string.Empty).Trim();

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

            return _repository.SaveIndependentType(item, idLoaiTaiLieu == Guid.Empty, defaultStorageLocation);
        }

        public Guid? GetDefaultStorageLocation(Guid idLoaiTaiLieu)
        {
            return _repository.GetDefaultStorageLocation(idLoaiTaiLieu);
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
