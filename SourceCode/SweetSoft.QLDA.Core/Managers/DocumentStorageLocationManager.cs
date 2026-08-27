using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace SweetSoft.QLDA.Core.Managers
{
    public static class DocumentStorageLevelKeys
    {
        public const string Office = "VAN_PHONG";
        public const string Room = "PHONG";
        public const string Cabinet = "TU";
        public const string Shelf = "KE";
    }

    public class DocumentStorageLocationManager : BaseManager
    {
        private static readonly Lazy<DocumentStorageLocationManager>
            _instance =
                new Lazy<DocumentStorageLocationManager>(
                    () => new DocumentStorageLocationManager());

        private readonly DocumentStorageLocationRepository
            _repository;

        public static DocumentStorageLocationManager Instance
        {
            get { return _instance.Value; }
        }

        public DocumentStorageLocationManager(
            IAppContext applicationContext = null)
            : base(applicationContext)
        {
            AuditManager auditManager =
                new AuditManager(GetClientInfo());

            _repository =
                new DocumentStorageLocationRepository(
                    auditManager);
        }

        public List<TblNoiLuuTru> GetAll(
            string keyword = null)
        {
            return _repository.GetAll(keyword);
        }

        public DataTable SearchStorageLocations(
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

        public DataTable SearchStorageLocations(
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

        public TblNoiLuuTru GetById(Guid idNoiLuuTru)
        {
            return _repository.GetById(idNoiLuuTru);
        }

        public List<TblNhanVien> GetAvailableEmployees()
        {
            return _repository.GetAvailableEmployees();
        }

        public List<TblNoiLuuTru> GetAvailableParents(
            string childLevel,
            Guid currentId)
        {
            string normalizedLevel =
                NormalizeLevel(childLevel);

            string requiredParentLevel =
                GetRequiredParentLevel(normalizedLevel);

            if (string.IsNullOrEmpty(requiredParentLevel))
            {
                return new List<TblNoiLuuTru>();
            }

            List<TblNoiLuuTru> allItems =
                _repository.GetAll();

            return allItems
                .Where(item =>
                    item.KichHoat
                    && item.IdNoiLuuTru != currentId
                    && string.Equals(
                        item.CapLuuTru,
                        requiredParentLevel,
                        StringComparison.OrdinalIgnoreCase)
                    && !WouldCreateCycle(
                        currentId,
                        item.IdNoiLuuTru,
                        allItems))
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenNoiLuuTru)
                .ToList();
        }

        public TblNoiLuuTru Save(
            Guid idNoiLuuTru,
            Guid? idNoiLuuTruCha,
            string maNoiLuuTru,
            string tenNoiLuuTru,
            string capLuuTru,
            Guid? idNhanVienPhuTrach,
            string moTa,
            int thuTuHienThi,
            bool kichHoat)
        {
            maNoiLuuTru =
                (maNoiLuuTru ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            tenNoiLuuTru =
                (tenNoiLuuTru ?? string.Empty).Trim();

            capLuuTru = NormalizeLevel(capLuuTru);
            moTa = (moTa ?? string.Empty).Trim();

            if (idNoiLuuTruCha.HasValue
                && idNoiLuuTruCha.Value == Guid.Empty)
            {
                idNoiLuuTruCha = null;
            }

            if (idNhanVienPhuTrach.HasValue
                && idNhanVienPhuTrach.Value == Guid.Empty)
            {
                idNhanVienPhuTrach = null;
            }

            ValidateBasicData(
                maNoiLuuTru,
                tenNoiLuuTru,
                capLuuTru,
                moTa,
                thuTuHienThi);

            TblNoiLuuTru item = null;

            if (idNoiLuuTru != Guid.Empty)
            {
                item = _repository.GetById(idNoiLuuTru);

                if (item == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy nơi lưu trữ.");
                }
            }

            if (item != null
                && !string.Equals(
                    item.CapLuuTru,
                    capLuuTru,
                    StringComparison.OrdinalIgnoreCase)
                && _repository.HasChildren(idNoiLuuTru))
            {
                throw new InvalidOperationException(
                    "Không thể đổi cấp vì vị trí này đang có vị trí con.");
            }

            ValidateParent(
                item,
                idNoiLuuTru,
                idNoiLuuTruCha,
                capLuuTru,
                kichHoat);

            if (_repository.IsCodeExisted(
                    maNoiLuuTru,
                    idNoiLuuTru))
            {
                throw new InvalidOperationException(
                    "Mã nơi lưu trữ đã tồn tại.");
            }

            if (_repository.IsNameExisted(
                    tenNoiLuuTru,
                    idNoiLuuTruCha,
                    idNoiLuuTru))
            {
                throw new InvalidOperationException(
                    "Tên nơi lưu trữ đã tồn tại trong cùng vị trí cha.");
            }

            ValidateResponsibleEmployee(
                idNhanVienPhuTrach);

            ValidateDeactivation(
                item,
                idNoiLuuTru,
                kichHoat);

            DateTime currentDate = DateTime.UtcNow;
            string currentUserName = GetCurrentUserName();

            if (item == null)
            {
                item = new TblNoiLuuTru
                {
                    IdNoiLuuTru = UUIDv7.NewGuid(),
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

            item.IdNoiLuuTruCha = idNoiLuuTruCha;
            item.MaNoiLuuTru = maNoiLuuTru;
            item.TenNoiLuuTru = tenNoiLuuTru;
            item.CapLuuTru = capLuuTru;
            item.IdNhanVienPhuTrach =
                idNhanVienPhuTrach;
            item.MoTa = moTa;
            item.ThuTuHienThi = thuTuHienThi;
            item.KichHoat = kichHoat;

            if (idNoiLuuTru == Guid.Empty)
                return _repository.Insert(item);

            return _repository.Update(item);
        }

        public bool Delete(Guid idNoiLuuTru)
        {
            TblNoiLuuTru item =
                _repository.GetById(idNoiLuuTru);

            if (item == null)
                return false;

            if (_repository.HasChildren(idNoiLuuTru))
            {
                throw new InvalidOperationException(
                    "Không thể xóa vì nơi lưu trữ đang có vị trí con.");
            }

            if (_repository.IsInUse(idNoiLuuTru))
            {
                throw new InvalidOperationException(
                    "Không thể xóa vì nơi lưu trữ đã được tài liệu sử dụng.");
            }

            item.NguoiCapNhat = GetCurrentUserName();
            item.NgayCapNhat = DateTime.UtcNow;

            return _repository.Delete(item);
        }

        private static void ValidateBasicData(
            string code,
            string name,
            string level,
            string description,
            int displayOrder)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(
                    "Mã nơi lưu trữ không được để trống.");
            }

            if (code.Length > 50)
            {
                throw new ArgumentException(
                    "Mã nơi lưu trữ không được vượt quá 50 ký tự.");
            }

            if (!Regex.IsMatch(
                    code,
                    @"^[A-Z0-9][A-Z0-9_-]*$"))
            {
                throw new ArgumentException(
                    "Mã nơi lưu trữ chỉ được chứa chữ cái không dấu, "
                    + "chữ số, dấu gạch ngang hoặc gạch dưới.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(
                    "Tên nơi lưu trữ không được để trống.");
            }

            if (name.Length > 150)
            {
                throw new ArgumentException(
                    "Tên nơi lưu trữ không được vượt quá 150 ký tự.");
            }

            if (!IsValidLevel(level))
            {
                throw new ArgumentException(
                    "Cấp lưu trữ không hợp lệ.");
            }

            if (description.Length > 500)
            {
                throw new ArgumentException(
                    "Mô tả không được vượt quá 500 ký tự.");
            }

            if (displayOrder < 0)
            {
                throw new ArgumentException(
                    "Thứ tự hiển thị không được nhỏ hơn 0.");
            }
        }

        private void ValidateParent(
            TblNoiLuuTru currentItem,
            Guid currentId,
            Guid? parentId,
            string childLevel,
            bool isActivated)
        {
            if (string.Equals(
                    childLevel,
                    DocumentStorageLevelKeys.Office,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (parentId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Cấp Văn phòng không được có nơi lưu trữ cha.");
                }

                return;
            }

            if (!parentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Vui lòng chọn nơi lưu trữ cha.");
            }

            if (currentId != Guid.Empty
                && parentId.Value == currentId)
            {
                throw new InvalidOperationException(
                    "Một vị trí không thể là cha của chính nó.");
            }

            TblNoiLuuTru parent =
                _repository.GetById(parentId.Value);

            if (parent == null)
            {
                throw new InvalidOperationException(
                    "Nơi lưu trữ cha không tồn tại hoặc đã bị xóa.");
            }

            string requiredParentLevel =
                GetRequiredParentLevel(childLevel);

            if (!string.Equals(
                    parent.CapLuuTru,
                    requiredParentLevel,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Cấp " + GetLevelDisplayName(childLevel)
                    + " phải nằm trong cấp "
                    + GetLevelDisplayName(requiredParentLevel)
                    + ".");
            }

            bool isChangingParent =
                currentItem == null
                || currentItem.IdNoiLuuTruCha != parentId;

            if (!parent.KichHoat
                && (isActivated || isChangingParent))
            {
                throw new InvalidOperationException(
                    "Không thể chọn nơi lưu trữ cha đang bị khóa.");
            }

            if (WouldCreateCycle(
                    currentId,
                    parentId.Value,
                    _repository.GetAll()))
            {
                throw new InvalidOperationException(
                    "Quan hệ cha con tạo thành vòng lặp.");
            }
        }

        private void ValidateResponsibleEmployee(
            Guid? employeeId)
        {
            if (!employeeId.HasValue)
                return;

            if (_repository.GetEmployeeById(
                    employeeId.Value) == null)
            {
                throw new InvalidOperationException(
                    "Người phụ trách không tồn tại hoặc đã bị xóa.");
            }
        }

        private void ValidateDeactivation(
            TblNoiLuuTru currentItem,
            Guid currentId,
            bool isActivated)
        {
            if (currentItem == null
                || !currentItem.KichHoat
                || isActivated)
            {
                return;
            }

            bool hasActiveChildren =
                _repository.GetAll().Any(child =>
                    child.IdNoiLuuTruCha.HasValue
                    && child.IdNoiLuuTruCha.Value
                        == currentId
                    && child.KichHoat);

            if (hasActiveChildren)
            {
                throw new InvalidOperationException(
                    "Không thể khóa vị trí đang có vị trí con hoạt động.");
            }
        }

        private static bool WouldCreateCycle(
            Guid currentId,
            Guid candidateParentId,
            IList<TblNoiLuuTru> allItems)
        {
            if (currentId == Guid.Empty)
                return false;

            Dictionary<Guid, TblNoiLuuTru> lookup =
                allItems.ToDictionary(
                    item => item.IdNoiLuuTru);

            HashSet<Guid> visited = new HashSet<Guid>();
            Guid? cursor = candidateParentId;

            while (cursor.HasValue)
            {
                if (cursor.Value == currentId)
                    return true;

                if (!visited.Add(cursor.Value))
                    return true;

                TblNoiLuuTru current;

                if (!lookup.TryGetValue(
                        cursor.Value,
                        out current))
                {
                    break;
                }

                cursor = current.IdNoiLuuTruCha;
            }

            return false;
        }

        private static string NormalizeLevel(
            string level)
        {
            return (level ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static bool IsValidLevel(
            string level)
        {
            return string.Equals(
                       level,
                       DocumentStorageLevelKeys.Office,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       level,
                       DocumentStorageLevelKeys.Room,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       level,
                       DocumentStorageLevelKeys.Cabinet,
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       level,
                       DocumentStorageLevelKeys.Shelf,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRequiredParentLevel(
            string childLevel)
        {
            if (string.Equals(
                    childLevel,
                    DocumentStorageLevelKeys.Room,
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentStorageLevelKeys.Office;
            }

            if (string.Equals(
                    childLevel,
                    DocumentStorageLevelKeys.Cabinet,
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentStorageLevelKeys.Room;
            }

            if (string.Equals(
                    childLevel,
                    DocumentStorageLevelKeys.Shelf,
                    StringComparison.OrdinalIgnoreCase))
            {
                return DocumentStorageLevelKeys.Cabinet;
            }

            return null;
        }

        private static string GetLevelDisplayName(
            string level)
        {
            if (string.Equals(
                    level,
                    DocumentStorageLevelKeys.Office,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Văn phòng";
            }

            if (string.Equals(
                    level,
                    DocumentStorageLevelKeys.Room,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Phòng";
            }

            if (string.Equals(
                    level,
                    DocumentStorageLevelKeys.Cabinet,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Tủ";
            }

            if (string.Equals(
                    level,
                    DocumentStorageLevelKeys.Shelf,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Kệ";
            }

            return level;
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
