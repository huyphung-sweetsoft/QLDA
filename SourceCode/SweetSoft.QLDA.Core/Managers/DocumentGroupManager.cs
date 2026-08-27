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
    public class DocumentGroupManager : BaseManager
    {
        private static readonly Lazy<DocumentGroupManager> _instance =
            new Lazy<DocumentGroupManager>(
                () => new DocumentGroupManager());

        private readonly DocumentGroupRepository _repository;

        public static DocumentGroupManager Instance
        {
            get { return _instance.Value; }
        }

        public DocumentGroupManager(
            IAppContext applicationContext = null)
            : base(applicationContext)
        {
            AuditManager auditManager =
                new AuditManager(GetClientInfo());

            _repository =
                new DocumentGroupRepository(auditManager);
        }

        public List<TblNhomTaiLieu> GetAll(string keyword = null)
        {
            return _repository.GetAll(keyword);
        }

        /// <summary>
        /// Tìm kiếm nhanh nhóm tài liệu, có hỗ trợ bộ lọc và phân trang.
        /// </summary>
        public DataTable SearchDocumentGroups(
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
        /// Tìm kiếm nâng cao nhóm tài liệu, có hỗ trợ phân trang.
        /// </summary>
        public DataTable SearchDocumentGroups(
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

        public TblNhomTaiLieu GetById(Guid idNhomTaiLieu)
        {
            return _repository.GetById(idNhomTaiLieu);
        }

        public TblNhomTaiLieu Save(
            Guid idNhomTaiLieu,
            string tenNhom,
            string moTa,
            int thuTuHienThi,
            bool kichHoat)
        {
            tenNhom = (tenNhom ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(tenNhom))
            {
                throw new ArgumentException(
                    "Tên nhóm tài liệu không được để trống.");
            }

            if (tenNhom.Length > 150)
            {
                throw new ArgumentException(
                    "Tên nhóm tài liệu không được vượt quá 150 ký tự.");
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

            if (_repository.IsNameExisted(
                    tenNhom,
                    idNhomTaiLieu))
            {
                throw new InvalidOperationException(
                    "Tên nhóm tài liệu đã tồn tại.");
            }

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();

            TblNhomTaiLieu item;

            if (idNhomTaiLieu == Guid.Empty)
            {
                item = new TblNhomTaiLieu
                {
                    IdNhomTaiLieu = UUIDv7.NewGuid(),
                    NguoiTao = currentUserName,
                    NgayTao = currentDate,
                    DaXoa = false
                };
            }
            else
            {
                item = _repository.GetById(idNhomTaiLieu);

                if (item == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy nhóm tài liệu.");
                }

                item.NguoiCapNhat = currentUserName;
                item.NgayCapNhat = currentDate;
            }

            item.TenNhom = tenNhom;
            item.MoTa = moTa;
            item.ThuTuHienThi = thuTuHienThi;
            item.KichHoat = kichHoat;

            if (idNhomTaiLieu == Guid.Empty)
                return _repository.Insert(item);

            return _repository.Update(item);
        }

        public bool Delete(Guid idNhomTaiLieu)
        {
            TblNhomTaiLieu item =
                _repository.GetById(idNhomTaiLieu);

            if (item == null)
                return false;

            if (_repository.IsInUse(idNhomTaiLieu))
            {
                throw new InvalidOperationException(
                    "Nhóm tài liệu đang được sử dụng bởi loại tài liệu.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;

            return _repository.Delete(item);
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
