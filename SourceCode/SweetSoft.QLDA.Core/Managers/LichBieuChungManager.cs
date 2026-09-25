using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
namespace SweetSoft.QLDA.Core.Managers
{
    public class LichBieuChungManager : BaseManager
    {
        private static readonly Lazy<LichBieuChungManager> _instance = new Lazy<LichBieuChungManager>(() => new LichBieuChungManager());
        public static LichBieuChungManager Instance => _instance.Value;

        private readonly CauHinhTuanLamViecRepository _tuanRepository;
        private readonly LichNgoaiLeRepository _ngoaiLeRepository;
        private readonly AuditManager _auditManager;

        public LichBieuChungManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _tuanRepository = new CauHinhTuanLamViecRepository(_auditManager);
            _ngoaiLeRepository = new LichNgoaiLeRepository(_auditManager);
        }

        #region CACHE CHỐNG TRÀN BỘ NHỚ (TỐI ƯU & THREAD-SAFE)
        private List<TblCauHinhTuanLamViec> _weekConfigCache;
        private List<TblLichNgoaiLe> _exceptionsCache;
        private DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly object _cacheLock = new object();

        public void ForceRefreshCache()
        {
            lock (_cacheLock) { _lastCacheTime = DateTime.MinValue; }
        }

        private void RefreshCacheIfNeeded()
        {
            if ((DateTime.Now - _lastCacheTime).TotalSeconds > 60 || _weekConfigCache == null || _exceptionsCache == null)
            {
                lock (_cacheLock)
                {
                    if ((DateTime.Now - _lastCacheTime).TotalSeconds > 60 || _weekConfigCache == null || _exceptionsCache == null)
                    {
                        _weekConfigCache = _tuanRepository.GetAll();
                        DateTime startDate = DateTime.Today.AddYears(-1);
                        DateTime endDate = DateTime.Today.AddYears(2);
                        _exceptionsCache = new SubSonic.Select().From(TblLichNgoaiLe.Schema)
                                             .Where(TblLichNgoaiLe.Columns.DaXoa).IsEqualTo(false)
                                             .And(TblLichNgoaiLe.Columns.NgayBatDau).IsGreaterThanOrEqualTo(startDate)
                                             .And(TblLichNgoaiLe.Columns.NgayBatDau).IsLessThanOrEqualTo(endDate)
                                             .ExecuteTypedList<TblLichNgoaiLe>();
                        _lastCacheTime = DateTime.Now;
                    }
                }
            }
        }
        #endregion

        #region NHÓM 1: QUẢN LÝ CẤU HÌNH TUẦN LÀM VIỆC MẶC ĐỊNH (LỚP 1)

        public List<TblCauHinhTuanLamViec> GetAllCauHinhTuan()
        {
            return _tuanRepository.GetAll();
        }

        public DateTime GetNextWorkingDay(DateTime date)
        {
            DateTime currentDate = date.Date;
            int safeguard = 0;
            const int MAX_LOOP = 3650; // Giới hạn 10 năm

            while (!CheckIsWorkingDay(currentDate))
            {
                currentDate = currentDate.AddDays(1);
                safeguard++;
                if (safeguard > MAX_LOOP)
                {
                    throw new InvalidOperationException("Không tìm được ngày làm việc hợp lệ. Vui lòng kiểm tra lại cấu hình Lịch biểu.");
                }
            }
            return currentDate;
        }

        public TblCauHinhTuanLamViec UpdateCauHinhTuan(TblCauHinhTuanLamViec item)
        {
            if (item == null) return null;

            // 1. TỰ TRUY VẤN ITEM CŨ BẰNG HÀM SELECT ĐỂ NÉ LỖI ÉP KIỂU GUID CỦA SUBSONIC
            TblCauHinhTuanLamViec oldItem = new SubSonic.Select()
                .From(TblCauHinhTuanLamViec.Schema)
                .Where(TblCauHinhTuanLamViec.Columns.IdCauHinh).IsEqualTo(item.IdCauHinh)
                .ExecuteSingle<TblCauHinhTuanLamViec>();

            // 2. SO SÁNH: CHỈ KÍCH HOẠT NẾU TRẠNG THÁI LÀM VIỆC/NGHỈ THỰC SỰ THAY ĐỔI
            bool isChanged = false;
            string oldStatus = "Không rõ";

            if (oldItem != null)
            {
                if (oldItem.LaNgayLamViec != item.LaNgayLamViec)
                {
                    isChanged = true;
                    oldStatus = oldItem.LaNgayLamViec ? "Làm việc" : "Nghỉ";
                }
            }
            else
            {
                // Nếu chưa có trong DB thì mặc định là có thay đổi (Thêm mới)
                isChanged = true;
            }

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            item.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

            // 3. VẪN CẬP NHẬT DATABASE CHO TOÀN BỘ 7 NGÀY NHƯ BÌNH THƯỜNG
            var result = _tuanRepository.Update(item);

            // 4. [QUAN TRỌNG] CHỈ GỬI THÔNG BÁO CHO ĐÚNG CÁI NGÀY BỊ ĐỔI TRẠNG THÁI
            if (result != null && isChanged)
            {
                ForceRefreshCache();

                string newStatus = item.LaNgayLamViec ? "Làm việc" : "Nghỉ";
                string[] days = { "Chủ Nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
                string dayName = days[item.NgayTrongTuan];

                string detailReason = $"Thay đổi cấu hình tuần: Chuyển {dayName} từ '{oldStatus}' thành '{newStatus}'.";

                System.Threading.Tasks.Task.Run(() => {
                    TaskManager.Instance.NotifyPMsOnScheduleChange(DateTime.Today, null, detailReason);
                });
            }
            return result;
        }

        #endregion

        #region NHÓM 2: QUẢN LÝ LỊCH NGOẠI LỆ - LỄ TẾT/LÀM BÙ (LỚP 2)

        public DataTable SearchLichNgoaiLePaging(string searchTerm, Dictionary<string, object> keyValueSearchs, bool? isWorkingDay, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _ngoaiLeRepository.SearchPaging(searchTerm, keyValueSearchs, isWorkingDay, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public TblLichNgoaiLe GetLichNgoaiLeById(Guid id)
        {
            return _ngoaiLeRepository.GetById(id);
        }

        public TblLichNgoaiLe CreateOrUpdate(TblLichNgoaiLe dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenNgoaiLe, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenNgoaiLe));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.IdNgoaiLe == Guid.Empty);
            TblLichNgoaiLe resultItem = null;
            DateTime affectedDate = dto.NgayBatDau;

            // 1. CHỈ BỌC TRANSACTION CHO RIÊNG VIỆC LƯU LỊCH
            using (var scope = new TransactionScope())
            {
                if (isInsert)
                {
                    dto.IdNgoaiLe = Guid.NewGuid();
                    dto.NgayTao = DateTime.Now;
                    dto.NguoiTao = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";
                    dto.DaXoa = false;
                    resultItem = _ngoaiLeRepository.Insert(dto);
                }
                else
                {
                    TblLichNgoaiLe existingItem = _ngoaiLeRepository.GetById(dto.IdNgoaiLe);
                    BusinessValidator.ThrowIfNull(existingItem, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdNgoaiLe), ErrorCodes.NotFound);

                    DateTime oldStart = existingItem.NgayBatDau;
                    affectedDate = oldStart < dto.NgayBatDau ? oldStart : dto.NgayBatDau;

                    existingItem.TenNgoaiLe = dto.TenNgoaiLe;
                    existingItem.NgayBatDau = dto.NgayBatDau;
                    existingItem.NgayKetThuc = dto.NgayKetThuc;
                    existingItem.MoTa = dto.MoTa;
                    existingItem.LaNgayLamViec = dto.LaNgayLamViec;
                    existingItem.NgayCapNhat = DateTime.Now;
                    existingItem.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";

                    resultItem = _ngoaiLeRepository.Update(existingItem);
                }
                scope.Complete(); // Commit lịch thành công
            }

            // 2. SAU KHI LỊCH ĐÃ COMMIT THÌ MỚI REFRESH CACHE VÀ BẮN THÔNG BÁO
            if (resultItem != null)
            {
                ForceRefreshCache();

                DateTime tTuNgay = resultItem.NgayBatDau;
                DateTime tDenNgay = resultItem.NgayKetThuc; // ĐÃ SỬA: Bỏ dấu ? vì cột DB không cho phép null

                // ĐÃ SỬA: Bỏ .HasValue và .Value do DateTime luôn có giá trị
                string timeRange = (resultItem.NgayKetThuc.Date != resultItem.NgayBatDau.Date)
                    ? $"từ {resultItem.NgayBatDau:dd/MM/yyyy} đến {resultItem.NgayKetThuc:dd/MM/yyyy}"
                    : $"vào ngày {resultItem.NgayBatDau:dd/MM/yyyy}";

                string action = isInsert ? "Thêm mới" : "Cập nhật";
                string detailReason = $"{action} lịch nghỉ: {resultItem.TenNgoaiLe} ({timeRange}).";

                System.Threading.Tasks.Task.Run(() => {
                    // Truyền tDenNgay vào hàm nhận DateTime? vẫn hoàn toàn hợp lệ (implicit conversion)
                    TaskManager.Instance.NotifyPMsOnScheduleChange(tTuNgay, tDenNgay, detailReason);
                });
            }
            return resultItem;
        }

        public bool DeleteLichNgoaiLe(Guid id)
        {
            var item = _ngoaiLeRepository.GetById(id);
            if (item != null)
            {
                DateTime affectedDate = item.NgayBatDau;
                bool isDeleted = false;

                using (var scope = new TransactionScope())
                {
                    isDeleted = _ngoaiLeRepository.Delete(item);
                    scope.Complete(); // Commit xóa lịch thành công
                }

                if (isDeleted)
                {
                    ForceRefreshCache();

                    // Lấy thông tin để đẩy vào Email
                    DateTime tTuNgay = item.NgayBatDau;
                    DateTime? tDenNgay = item.NgayKetThuc;
                    string reason = $"Hủy lịch nghỉ lệ: {item.TenNgoaiLe}";

                    // [THAY THẾ HÀM CŨ]: Bắn thông báo ngầm
                    System.Threading.Tasks.Task.Run(() => {
                        TaskManager.Instance.NotifyPMsOnScheduleChange(tTuNgay, tDenNgay, reason);
                    });
                }
                return isDeleted;
            }
            return false;
        }
        #endregion

        #region NHÓM 3: ĐỘNG CƠ TÍNH TOÁN THỜI GIAN (CORE ENGINE)
        public TblLichNgoaiLe GetExceptionByDate(DateTime date)
        {
            RefreshCacheIfNeeded();
            if (_exceptionsCache != null)
            {
                return _exceptionsCache.Find(x => x.NgayBatDau.Date <= date.Date && x.NgayKetThuc.Date >= date.Date);
            }
            return null;
        }
        public bool CheckIsWorkingDay(DateTime date)
        {
            RefreshCacheIfNeeded();
            if (_exceptionsCache != null)
            {
                var ex = _exceptionsCache.Find(x => x.NgayBatDau.Date <= date.Date && x.NgayKetThuc.Date >= date.Date);
                if (ex != null) return ex.LaNgayLamViec;
            }
            int dayOfWeek = (int)date.DayOfWeek;
            if (_weekConfigCache != null)
            {
                var config = _weekConfigCache.Find(x => x.NgayTrongTuan == dayOfWeek);
                if (config != null) return config.LaNgayLamViec;
            }
            return false;
        }

        public DateTime CalculateTaskEndDate(DateTime ngayBatDau, int thoiHanNgay)
        {
            if (thoiHanNgay <= 0) return ngayBatDau;

            DateTime currentDate = ngayBatDau.Date;
            int remainingDays = thoiHanNgay;
            int safeguard = 0;
            const int MAX_LOOP = 3650;

            while (remainingDays > 0)
            {
                if (CheckIsWorkingDay(currentDate))
                {
                    remainingDays--;
                    if (remainingDays == 0) break;
                }
                currentDate = currentDate.AddDays(1);
                safeguard++;

                if (safeguard > MAX_LOOP)
                {
                    throw new InvalidOperationException("Thời gian kéo dài quá lâu. Vui lòng kiểm tra lại cấu hình Lịch biểu.");
                }
            }
            return currentDate;
        }

        public int CountWorkingDaysInRange(DateTime start, DateTime end)
        {
            if (end.Date < start.Date) return 0;
            int count = 0;
            DateTime curr = start.Date;
            while (curr <= end.Date)
            {
                if (CheckIsWorkingDay(curr)) count++;
                curr = curr.AddDays(1);
            }
            return count;
        }
        #endregion
    }
}