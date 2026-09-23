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
            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            item.NguoiCapNhat = currentUserId != Guid.Empty ? currentUserId.ToString() : "System";
            var result = _tuanRepository.Update(item);

            // [FIX KIẾN TRÚC]: Chỉ update DB và Cache, KHÔNG gọi Sync ở đây để tránh lặp 7 lần!
            if (result != null) ForceRefreshCache();
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

            // 2. SAU KHI LỊCH ĐÃ COMMIT THÌ MỚI REFRESH CACHE VÀ SYNC TASK
            if (resultItem != null)
            {
                ForceRefreshCache();
                //TaskManager.Instance.SyncPendingTasksAfterScheduleChange(affectedDate);
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
                    //TaskManager.Instance.SyncPendingTasksAfterScheduleChange(affectedDate);
                }
                return isDeleted;
            }
            return false;
        }
        #endregion

        #region NHÓM 3: ĐỘNG CƠ TÍNH TOÁN THỜI GIAN (CORE ENGINE)

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