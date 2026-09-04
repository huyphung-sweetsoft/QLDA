using SweetSoft.QLDA.Core.FileManager;
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
    public class DocumentTemplateManager : BaseManager
    {
        private static readonly Lazy<DocumentTemplateManager> _instance =
            new Lazy<DocumentTemplateManager>(() => new DocumentTemplateManager());

        private readonly DocumentTemplateRepository _repository;
        private readonly DocumentTypeRepository _documentTypeRepository;

        public static DocumentTemplateManager Instance
        {
            get { return _instance.Value; }
        }

        public DocumentTemplateManager(IAppContext applicationContext = null)
            : base(applicationContext)
        {
            AuditManager auditManager = new AuditManager(GetClientInfo());
            _repository = new DocumentTemplateRepository(auditManager);
            _documentTypeRepository = new DocumentTypeRepository(auditManager);
        }

        public DataTable SearchDocumentTemplates(
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

        public DataTable SearchDocumentTemplates(
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

        public TblMauTaiLieu GetById(Guid idMauTaiLieu)
        {
            return _repository.GetById(idMauTaiLieu);
        }

        public DataTable GetAvailableTemplatesByType(Guid idLoaiTaiLieu)
        {
            return _repository.GetAvailableTemplatesByType(idLoaiTaiLieu);
        }

        public TblMauTaiLieu Save(
            Guid idMauTaiLieu,
            Guid idLoaiTaiLieu,
            string tenMau,
            string phienBanMau,
            string moTa,
            bool laMauMacDinh,
            bool kichHoat)
        {
            tenMau = (tenMau ?? string.Empty).Trim();
            phienBanMau = (phienBanMau ?? string.Empty).Trim();
            moTa = (moTa ?? string.Empty).Trim();

            if (idLoaiTaiLieu == Guid.Empty)
                throw new ArgumentException("Vui lòng chọn loại tài liệu.");
            if (string.IsNullOrEmpty(tenMau))
                throw new ArgumentException("Tên mẫu tài liệu không được để trống.");
            if (tenMau.Length > 200)
                throw new ArgumentException("Tên mẫu tài liệu không được vượt quá 200 ký tự.");
            if (string.IsNullOrEmpty(phienBanMau))
                throw new ArgumentException("Phiên bản mẫu không được để trống.");
            if (phienBanMau.Length > 20)
                throw new ArgumentException("Phiên bản mẫu không được vượt quá 20 ký tự.");
            if (moTa.Length > 500)
                throw new ArgumentException("Mô tả không được vượt quá 500 ký tự.");
            if (laMauMacDinh && !kichHoat)
                throw new InvalidOperationException("Mẫu mặc định phải ở trạng thái kích hoạt.");

            TblMauTaiLieu item = null;
            if (idMauTaiLieu != Guid.Empty)
            {
                item = _repository.GetById(idMauTaiLieu);
                if (item == null)
                    throw new InvalidOperationException("Không tìm thấy mẫu tài liệu.");
            }

            TblLoaiTaiLieu documentType = _documentTypeRepository.GetById(idLoaiTaiLieu);
            if (documentType == null)
                throw new InvalidOperationException("Loại tài liệu không tồn tại hoặc đã bị xóa.");

            bool isChangingToInactiveType = !documentType.KichHoat
                && (item == null || item.IdLoaiTaiLieu != documentType.IdLoaiTaiLieu);
            if (isChangingToInactiveType)
                throw new InvalidOperationException("Không thể chọn loại tài liệu đang bị khóa.");

            if (_repository.IsNameAndVersionExisted(
                    idLoaiTaiLieu,
                    tenMau,
                    phienBanMau,
                    idMauTaiLieu))
            {
                throw new InvalidOperationException(
                    "Tên và phiên bản mẫu đã tồn tại trong loại tài liệu đã chọn.");
            }

            if (laMauMacDinh
                && _repository.HasOtherDefault(idLoaiTaiLieu, idMauTaiLieu))
            {
                throw new InvalidOperationException(
                    "Loại tài liệu đã có mẫu mặc định. Hãy bỏ mặc định ở mẫu cũ trước.");
            }

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();

            if (item == null)
            {
                item = new TblMauTaiLieu
                {
                    IdMauTaiLieu = UUIDv7.NewGuid(),
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

            item.IdLoaiTaiLieu = idLoaiTaiLieu;
            item.TenMau = tenMau;
            item.PhienBanMau = phienBanMau;
            item.MoTa = moTa;
            item.LaMauMacDinh = laMauMacDinh;
            item.KichHoat = kichHoat;

            return idMauTaiLieu == Guid.Empty
                ? _repository.Insert(item)
                : _repository.Update(item);
        }

        public bool Delete(Guid idMauTaiLieu)
        {
            TblMauTaiLieu item = _repository.GetById(idMauTaiLieu);
            if (item == null)
                return false;

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Delete(item);
        }

        public void ClearTemplateFile(Guid idMauTaiLieu)
        {
            TblMauTaiLieu item = _repository.GetById(idMauTaiLieu);
            if (item == null || !item.IdFileMau.HasValue)
                return;

            item.IdFileMau = null;
            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            _repository.Update(item);
        }

        public void SyncTemplateFile(Guid idMauTaiLieu)
        {
            TblMauTaiLieu item = _repository.GetById(idMauTaiLieu);
            if (item == null)
                throw new InvalidOperationException("Không tìm thấy mẫu tài liệu.");

            TblUploadFile uploadFile = UploadManager.Instance
                .GetUploadFileByRefIdAndRefType(
                    idMauTaiLieu,
                    FileUploadTypes.DocumentTemplate);
            Guid? idFileMau = uploadFile == null
                ? (Guid?)null
                : uploadFile.Id;

            if (item.IdFileMau == idFileMau)
                return;

            item.IdFileMau = idFileMau;
            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;
            _repository.Update(item);
        }

        private string GetCurrentUserName()
        {
            if (_applicationContext == null
                || string.IsNullOrWhiteSpace(_applicationContext.UserName))
            {
                return "[System]";
            }

            return _applicationContext.UserName;
        }
    }
}
