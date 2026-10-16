using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// Nghiệp vụ bấm giờ làm việc trong ngày (Stopwatch Time Tracking):
    /// - Không chia tách nhập liệu theo ca sáng/chiều; nhân viên bấm Bắt đầu -> Dừng -> Tiếp tục trực tiếp trên từng Task.
    /// - Tại một thời điểm chỉ có tối đa 1 Task được phép chạy.
    /// - Hệ thống tự động lấy Giờ kết thúc ca sáng và Giờ bắt đầu ca chiều làm khung nghỉ trưa để tự động trừ ra.
    /// - Sau giờ kết thúc ca chiều (VD: 17:00), hệ thống tự động dừng task đang chạy và chốt sổ tổng hợp cả ngày.
    /// </summary>
    public class KhaiBaoGioManager : BaseManager
    {
        #region Singleton

        private static readonly Lazy<KhaiBaoGioManager> _instance =
            new Lazy<KhaiBaoGioManager>(() => new KhaiBaoGioManager());

        public static KhaiBaoGioManager Instance => _instance.Value;

        private readonly KhaiBaoGioRepository _repo = new KhaiBaoGioRepository();

        public KhaiBaoGioManager(IAppContext applicationContext = null) : base(applicationContext)
        {
        }

        #endregion

        private const string SystemUser = "System";

        private static void LogToFile(Exception ex, string context)
        {
            try
            {
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/KhaiBaoGio_error.log");
                if (string.IsNullOrEmpty(path)) return;
                System.IO.File.AppendAllText(path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + context + Environment.NewLine
                    + ex + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception) { }
        }

        /// <summary>Chi tiết lỗi gốc, CHỈ hiện khi truy cập từ chính máy chủ (localhost / qlda.local) hoặc compilation debug="true".</summary>
        private static string DebugDetail(Exception ex)
        {
            try
            {
                System.Web.HttpContext ctx = System.Web.HttpContext.Current;
                if (ctx != null && (ctx.IsDebuggingEnabled || ctx.Request.IsLocal))
                    return " [" + ex.GetBaseException().Message + "]";
            }
            catch (Exception) { }
            return string.Empty;
        }

        private static byte ProjectActiveValue
        {
            get { return (byte)DuAnStatus.DangThucHien; }
        }

        #region Tiện ích định dạng & Tính toán thời gian thực làm (Trừ giờ nghỉ trưa)

        /// <summary>Định dạng số phút thành "3g45" (giữ tương thích cho các trang khác).</summary>
        public static string FormatMinutes(int minutes)
        {
            if (minutes < 0) minutes = 0;
            return (minutes / 60).ToString(CultureInfo.InvariantCulture) + "g" + (minutes % 60).ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>Định dạng số phút thành giờ thập phân như "3,6 giờ" hoặc "4,5 giờ".</summary>
        public static string FormatDecimalHours(int minutes)
        {
            if (minutes <= 0) return "0 giờ";
            double hours = Math.Round(minutes / 60.0, 1);
            return hours.ToString("0.#", new CultureInfo("vi-VN")) + " giờ";
        }

        /// <summary>Định dạng dễ đọc trên giao diện; không thay đổi phép làm tròn/lưu DB.</summary>
        public static string FormatReadableMinutes(int minutes)
        {
            if (minutes <= 0) return "0 phút";
            int h = minutes / 60;
            int m = minutes % 60;
            if (h == 0) return m.ToString(CultureInfo.InvariantCulture) + " phút";
            return h.ToString(CultureInfo.InvariantCulture) + " giờ" +
                   (m > 0 ? " " + m.ToString(CultureInfo.InvariantCulture) + " phút" : string.Empty);
        }

        /// <summary>Định dạng số giây thành đồng hồ "HH:mm:ss".</summary>
        public static string FormatStopwatch(int totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;
            int h = totalSeconds / 3600;
            int m = (totalSeconds % 3600) / 60;
            int s = totalSeconds % 60;
            return h.ToString("00", CultureInfo.InvariantCulture) + ":"
                 + m.ToString("00", CultureInfo.InvariantCulture) + ":"
                 + s.ToString("00", CultureInfo.InvariantCulture);
        }

        public static string ShiftName(byte loaiCa)
        {
            return loaiCa == 1 ? "Ca sáng" : (loaiCa == 2 ? "Ca chiều" : "Ca ngày");
        }

        /// <summary>
        /// Tính số giây làm việc thực tế giữa [start, end] bằng cách lấy phần giao với các ca làm việc trong ngày.
        /// Nhờ đó khoảng nghỉ trưa (giữa ca sáng và ca chiều) và khoảng ngoài giờ làm việc tự động bị loại bỏ.
        /// </summary>
        public static int CalculateWorkingSeconds(DateTime start, DateTime end, List<ThongTinCa> shifts)
        {
            if (end <= start || shifts == null || shifts.Count == 0) return 0;

            double totalSeconds = 0;
            foreach (ThongTinCa s in shifts)
            {
                DateTime segStart = start > s.BatDau ? start : s.BatDau;
                DateTime segEnd = end < s.KetThuc ? end : s.KetThuc;
                if (segEnd > segStart)
                {
                    totalSeconds += (segEnd - segStart).TotalSeconds;
                }
            }
            return (int)Math.Round(totalSeconds);
        }

        /// <summary>Quy đổi từ số giây thực tế ra số phút lưu DB (làm tròn chuẩn, nếu đã chạy >= 30s thì tính 1 phút).</summary>
        public static int SecondsToMinutes(int totalSeconds)
        {
            if (totalSeconds <= 0) return 0;
            return (int)Math.Round(totalSeconds / 60.0, MidpointRounding.AwayFromZero);
        }

        /// <summary>Tổng hợp danh sách ca từ CauHinhTuanLamViec thành 1 khung ngày duy nhất (kèm mốc nghỉ trưa).</summary>
        private static CaView BuildUnifiedDayShift(List<ThongTinCa> rawShifts, KbgShiftRow dbRow, DateTime now, bool hadTasks)
        {
            List<ThongTinCa> ordered = rawShifts.OrderBy(s => s.BatDau).ToList();
            ThongTinCa first = ordered.First();
            ThongTinCa last = ordered.Last();
            int totalCapacityMinutes = ordered.Sum(s => s.SoPhutCa);

            DateTime? lunchStart = null;
            DateTime? lunchEnd = null;
            string lunchText = string.Empty;

            if (ordered.Count >= 2 && ordered[1].BatDau > ordered[0].KetThuc)
            {
                lunchStart = ordered[0].KetThuc;
                lunchEnd = ordered[1].BatDau;
                lunchText = ordered[0].GioKetThuc + "–" + ordered[1].GioBatDau;
            }

            CaView cv = new CaView
            {
                LoaiCa = 1,
                TenCa = "Thời gian làm việc hôm nay",
                GioBatDau = first.GioBatDau,
                GioKetThuc = last.GioKetThuc,
                GioNghiTrua = lunchText,
                SoPhutCa = totalCapacityMinutes,
                BatDau = first.BatDau,
                KetThuc = last.KetThuc,
                NghiTruaBatDau = lunchStart,
                NghiTruaKetThuc = lunchEnd,
                PhienBan = dbRow != null ? dbRow.PhienBan : 0,
                LuuLanCuoiLuc = dbRow != null ? dbRow.LuuLanCuoiLuc : null
            };

            bool isClosedInDb = dbRow != null && dbRow.TrangThai == 1;
            if (now < cv.BatDau)
                cv.State = TrangThaiCaView.ChuaMo;
            else if (now < cv.KetThuc && !isClosedInDb)
                cv.State = TrangThaiCaView.DangMo;
            else if (dbRow != null && dbRow.TongPhut > 0)
                cv.State = TrangThaiCaView.DaChotCoKhaiBao;
            else if (hadTasks)
                cv.State = TrangThaiCaView.DaChotKhongKhaiBao;
            else
                cv.State = TrangThaiCaView.DaChotKhongCoTask;

            return cv;
        }

        #endregion

        /// <summary>
        /// Snapshot nhỏ cho widget nổi. Chỉ đọc 1 công việc đang bấm giờ của nhân viên,
        /// không tải toàn bộ GetTodayView và không thay đổi trạng thái DB.
        /// </summary>
        public FloatingTimerSnapshot GetFloatingTimerSnapshot(Guid userId, DateTime now)
        {
            FloatingTimerSnapshot result = new FloatingTimerSnapshot { ServerNow = now };
            if (userId == Guid.Empty) return result;

            List<ThongTinCa> shifts = LichBieuChungManager.Instance.GetShiftsForDate(now.Date);
            if (shifts == null || shifts.Count == 0) return result;
            List<ThongTinCa> ordered = shifts.OrderBy(x => x.BatDau).ToList();
            DateTime firstStart = ordered.First().BatDau;
            DateTime dayEnd = ordered.Last().KetThuc;
            if (now < firstStart || now >= dayEnd) return result;

            KbgRunningTimerRow running = _repo.GetRunningTimerOfDay(userId, now.Date);
            if (running == null) return result;

            int completedSeconds = 0;
            foreach (KbgFinishedTimerRow item in running.FinishedSessions)
                completedSeconds += CalculateWorkingSeconds(item.BatDauLuc, item.KetThucLuc, ordered);

            result.Running = true;
            result.TaskId = running.IdCongViec;
            result.TaskCode = running.MaCongViec ?? string.Empty;
            result.TaskName = running.TenCongViec ?? string.Empty;
            result.ProjectCode = running.MaDuAn ?? string.Empty;
            result.ProjectName = running.TenDuAn ?? string.Empty;
            result.RunningSince = running.BatDauLuc;
            result.DayEnd = dayEnd;
            result.Segments = ordered;
            result.CompletedSeconds = completedSeconds;
            result.CapacitySeconds = ordered.Sum(x => x.SoPhutCa) * 60;
            return result;
        }

        #region Đọc dữ liệu cho trang KhaiBaoGio

        public KhaiBaoGioView GetTodayView(Guid userId, DateTime now)
        {
            DateTime today = now.Date;
            LichBieuChungManager lich = LichBieuChungManager.Instance;
            KhaiBaoGioView view = new KhaiBaoGioView { Today = today, ServerNow = now };

            List<ThongTinCa> rawShifts = lich.GetShiftsForDate(today);
            if (rawShifts == null || rawShifts.Count == 0)
            {
                view.IsWorkingDay = false;
                FillDayOffInfo(lich, today, view);
                return view;
            }
            view.IsWorkingDay = true;

            List<KbgTaskRow> workable = _repo.GetWorkableTasks(userId, today, ProjectActiveValue);
            KbgShiftRow dayRow = _repo.GetDayShift(userId, today);
            List<KbgEntryRow> entries = _repo.GetEntriesOfDay(userId, today);
            List<KbgLogRow> logs = _repo.GetLogsOfDay(userId, today);
            bool hadTasks = workable.Count > 0 || entries.Count > 0;

            CaView dayShift = BuildUnifiedDayShift(rawShifts, dayRow, now, hadTasks);
            if (dayShift.NghiTruaBatDau.HasValue && dayShift.NghiTruaKetThuc.HasValue)
            {
                view.IsLunchBreakNow = now >= dayShift.NghiTruaBatDau.Value && now < dayShift.NghiTruaKetThuc.Value;
            }

            // Kiểm tra xem có task nào đang chạy không
            KbgLogRow runningLog = logs.FirstOrDefault(l => !l.KetThucLuc.HasValue);
            view.RunningTaskId = runningLog != null ? (Guid?)runningLog.IdCongViec : null;

            bool isDayOpen = dayShift.State == TrangThaiCaView.DangMo;

            // ---- Dựng danh sách Task ----
            HashSet<Guid> workableIds = new HashSet<Guid>(workable.Select(t => t.IdCongViec));
            List<TaskRowView> rows = new List<TaskRowView>();

            foreach (KbgTaskRow t in workable)
            {
                rows.Add(BuildTaskRow(t, true, today, now, lich, isDayOpen, view.RunningTaskId, rawShifts, entries, logs));
            }

            List<Guid> extraIds = entries.Select(e => e.IdCongViec)
                                         .Union(logs.Select(l => l.IdCongViec))
                                         .Distinct()
                                         .Where(id => !workableIds.Contains(id))
                                         .ToList();
            if (extraIds.Count > 0)
            {
                foreach (KbgTaskRow t in _repo.GetTaskInfos(extraIds))
                {
                    rows.Add(BuildTaskRow(t, false, today, now, lich, isDayOpen, view.RunningTaskId, rawShifts, entries, logs));
                }
            }

            // Cập nhật tổng phút & tổng giây của cả ngày
            int maxSec = dayShift.SoPhutCa * 60;
            int sumSec = rows.Sum(r => r.TongGiayHienTai);
            if (sumSec > maxSec) sumSec = maxSec;

            dayShift.TongGiayHienTai = sumSec;
            dayShift.TongPhut = Math.Min(dayShift.SoPhutCa, rows.Sum(r => r.Phut));
            foreach (TaskRowView r in rows)
            {
                dayShift.PhutTheoTask[r.IdCongViec] = r.Phut;
            }
            if (dayShift.State == TrangThaiCaView.DaChotKhongKhaiBao && dayShift.TongPhut > 0)
            {
                dayShift.State = TrangThaiCaView.DaChotCoKhaiBao;
            }

            view.Shifts.Add(dayShift);

            // ---- Giờ lịch sử (các ngày trước đã chốt) theo task ----
            Dictionary<Guid, int[]> history = _repo.GetHistoryMinutes(userId, rows.Select(r => r.IdCongViec), today);
            foreach (TaskRowView r in rows)
            {
                int[] h;
                if (history.TryGetValue(r.IdCongViec, out h))
                {
                    r.LichSuDungHanPhut = h[0];
                    r.LichSuTrePhut = h[1];
                }
            }

            // Chỉ đọc lịch sử các phiên của chính nhân viên, đã truy vấn ở đầu hàm.
            // Popup nhật ký không phát sinh truy vấn DB theo từng task.
            foreach (KbgLogRow log in logs)
            {
                view.JournalSessions.Add(new KbgJournalSessionView
                {
                    IdCongViec = log.IdCongViec,
                    BatDauLuc = log.BatDauLuc,
                    KetThucLuc = log.KetThucLuc,
                    SoGiayThucLam = CalculateWorkingSeconds(log.BatDauLuc, log.KetThucLuc ?? now, rawShifts),
                    LoaiKetThuc = log.LoaiKetThuc
                });
            }

            // Công việc đang chạy đứng đầu nhưng KHÔNG nhân đôi thẻ/nút Dừng.
            TaskRowView runningRow = rows.FirstOrDefault(r => r.TrangThaiDongHo == TrangThaiDongHoTask.DangChay);
            rows = rows.Where(r => r != runningRow)
                       .OrderBy(r => (int)r.Nhom)
                       .ThenBy(r => r.NgayKetThuc ?? DateTime.MaxValue)
                       .ThenBy(r => r.MaCongViec)
                       .ToList();

            foreach (IGrouping<NhomTaskView, TaskRowView> g in rows.GroupBy(r => r.Nhom))
            {
                List<TaskRowView> list = g.ToList();
                list[0].ShowGroupHeader = true;
                list[0].GroupTitle = GroupTitle(g.Key);
                list[0].GroupCount = list.Count;
            }
            if (runningRow != null)
            {
                runningRow.ShowGroupHeader = true;
                runningRow.GroupTitle = "Công việc đang tính giờ";
                runningRow.GroupCount = 1;
                rows.Insert(0, runningRow);
            }

            view.Tasks = rows;
            return view;
        }

        private static TaskRowView BuildTaskRow(
            KbgTaskRow t,
            bool workable,
            DateTime today,
            DateTime now,
            LichBieuChungManager lich,
            bool isDayOpen,
            Guid? runningTaskId,
            List<ThongTinCa> rawShifts,
            List<KbgEntryRow> entries,
            List<KbgLogRow> allLogs)
        {
            TaskRowView r = new TaskRowView
            {
                IdCongViec = t.IdCongViec,
                IdDuAn = t.IdDuAn,
                MaDuAn = t.MaDuAn,
                TenDuAn = t.TenDuAn,
                MaCongViec = t.MaCongViec,
                TenCongViec = t.TenCongViec,
                NgayBatDau = t.NgayBatDau,
                NgayKetThuc = t.NgayKetThuc,
                TrangThai = t.TrangThai,
                TenDoUuTien = t.TenDoUuTien,
                DiemUuTien = t.DiemUuTien
            };

            r.IsOverdue = workable && t.NgayKetThuc.HasValue && t.NgayKetThuc.Value.Date < today;
            r.Nhom = !workable ? NhomTaskView.DaKhaiKhongConHoatDong
                               : (r.IsOverdue ? NhomTaskView.QuaHan : NhomTaskView.HomNay);
            r.Editable = workable && isDayOpen;

            if (r.IsOverdue)
            {
                int tre = lich.CountWorkingDaysInRange(t.NgayKetThuc.Value.Date.AddDays(1), today);
                r.TreNgayLamViec = tre < 1 ? 1 : tre;
            }
            else if (workable && t.NgayBatDau.HasValue && t.NgayKetThuc.HasValue)
            {
                r.NgayThuMay = lich.CountWorkingDaysInRange(t.NgayBatDau.Value.Date, today);
                r.TongNgayLamViec = lich.CountWorkingDaysInRange(t.NgayBatDau.Value.Date, t.NgayKetThuc.Value.Date);
            }

            // Tính toán số giây & trạng thái đồng hồ từ bảng Nhật ký (TblKhaiBaoGioNhatKy)
            List<KbgLogRow> taskLogs = allLogs.Where(l => l.IdCongViec == t.IdCongViec).ToList();
            r.SoLanBamHomNay = taskLogs.Count;

            int completedSeconds = 0;
            KbgLogRow activeLog = null;

            foreach (KbgLogRow lg in taskLogs)
            {
                if (lg.KetThucLuc.HasValue)
                {
                    completedSeconds += CalculateWorkingSeconds(lg.BatDauLuc, lg.KetThucLuc.Value, rawShifts);
                }
                else
                {
                    activeLog = lg;
                }
            }

            // Nếu dữ liệu cũ chỉ có trong TblKhaiBaoGioChiTiet mà không có log trong TblKhaiBaoGioNhatKy
            int dbMinutes = entries.Where(e => e.IdCongViec == t.IdCongViec).Sum(e => e.SoPhut);
            if (taskLogs.Count == 0 && dbMinutes > 0)
            {
                completedSeconds = dbMinutes * 60;
            }

            r.SoGiayDaTichLuy = completedSeconds;

            int runningSeconds = 0;
            if (activeLog != null)
            {
                r.DangChayTuLuc = activeLog.BatDauLuc;
                runningSeconds = CalculateWorkingSeconds(activeLog.BatDauLuc, now, rawShifts);
            }

            r.TongGiayHienTai = completedSeconds + runningSeconds;
            r.Phut = activeLog != null ? SecondsToMinutes(r.TongGiayHienTai)
                                       : (dbMinutes > 0 ? dbMinutes : SecondsToMinutes(completedSeconds));

            // Task đang chạy đồng hồ nhưng không còn nằm trong danh sách làm việc (vd: vừa chuyển Hoàn thành,
            // vừa có task con, dự án đổi trạng thái...) VẪN phải cho bấm Dừng. Nếu không, nút Bắt đầu của mọi task khác
            // bị khóa vì 'đang có task khác chạy' mà người dùng không có cách nào dừng nó.
            if (!workable && activeLog != null && isDayOpen)
            {
                r.Editable = true;
            }

            // Xác định trạng thái nút bấm
            if (!r.Editable)
            {
                r.TrangThaiDongHo = TrangThaiDongHoTask.DaKhoa;
                r.AllowStartOrResume = false;
            }
            else if (activeLog != null)
            {
                r.TrangThaiDongHo = TrangThaiDongHoTask.DangChay;
                r.AllowStartOrResume = false; // Đang chạy thì hiển thị nút Dừng
            }
            else
            {
                r.TrangThaiDongHo = (taskLogs.Count > 0 || r.SoGiayDaTichLuy > 0)
                    ? TrangThaiDongHoTask.TamDung
                    : TrangThaiDongHoTask.ChuaBatDau;

                // Chỉ được phép bấm Bắt đầu / Tiếp tục nếu KHÔNG có task nào khác đang chạy
                r.AllowStartOrResume = !runningTaskId.HasValue || runningTaskId.Value == t.IdCongViec;
            }

            return r;
        }

        private static void FillDayOffInfo(LichBieuChungManager lich, DateTime today, KhaiBaoGioView view)
        {
            var ex = lich.GetExceptionByDate(today);
            view.DayOffReason = ex != null && !string.IsNullOrWhiteSpace(ex.TenNgoaiLe)
                ? ex.TenNgoaiLe
                : "Ngày nghỉ theo lịch làm việc";
            try
            {
                view.NextWorkingDay = lich.GetNextWorkingDay(today.AddDays(1));
            }
            catch (InvalidOperationException)
            {
                view.NextWorkingDay = null;
            }
        }

        private static string GroupTitle(NhomTaskView nhom)
        {
            switch (nhom)
            {
                case NhomTaskView.QuaHan: return "Công việc quá hạn";
                case NhomTaskView.HomNay: return "Công việc hôm nay";
                default: return "Đã ghi nhận hôm nay (công việc không còn hoạt động)";
            }
        }

        #endregion

        #region Thao tác Bấm giờ: Bắt đầu / Tiếp tục / Dừng

        /// <summary>
        /// Nhấn nút "Bắt đầu" hoặc "Tiếp tục" cho một công việc.
        /// Nếu có công việc khác đang chạy, từ chối và yêu cầu dừng công việc đó trước.
        /// </summary>
        public SaveResult StartOrResumeTask(Guid userId, Guid taskId, DateTime now)
        {
            try
            {
                if (userId == Guid.Empty) return SaveResult.Fail("Không xác định được người dùng.");
                if (taskId == Guid.Empty) return SaveResult.Fail("Công việc không hợp lệ.");

                DateTime today = now.Date;
                List<ThongTinCa> shifts = LichBieuChungManager.Instance.GetShiftsForDate(today);
                if (shifts == null || shifts.Count == 0)
                    return new SaveResult { Success = false, IsClosed = true, Message = "Hôm nay là ngày nghỉ, không thể bấm giờ." };

                List<ThongTinCa> ordered = shifts.OrderBy(s => s.BatDau).ToList();
                DateTime dayStart = ordered.First().BatDau;
                DateTime dayEnd = ordered.Last().KetThuc;
                int totalCapacityMinutes = ordered.Sum(s => s.SoPhutCa);

                if (now < dayStart)
                    return new SaveResult { Success = false, IsClosed = true, Message = "Chưa đến giờ làm việc (" + ordered.First().GioBatDau + ")." };
                if (now >= dayEnd)
                    return new SaveResult { Success = false, IsClosed = true, Message = "Đã hết giờ làm việc hôm nay (" + ordered.Last().GioKetThuc + ")." };

                KbgTaskRow task = _repo.GetWorkableTasks(userId, today, ProjectActiveValue)
                                       .FirstOrDefault(t => t.IdCongViec == taskId);
                if (task == null)
                    return SaveResult.Fail("Công việc này không còn trong danh sách làm việc hôm nay.");

                string who = userId.ToString();
                KbgShiftRow dayRow = _repo.GetDayShift(userId, today);
                if (dayRow == null)
                {
                    _repo.InsertDayShiftIfMissing(Guid.NewGuid(), userId, today,
                        ordered.First().GioBatDau, ordered.Last().GioKetThuc, totalCapacityMinutes, who, now);
                    dayRow = _repo.GetDayShift(userId, today);
                    if (dayRow == null)
                        return SaveResult.Fail("Không khởi tạo được phiếu làm việc hôm nay, vui lòng thử lại.");
                }

                if (dayRow.TrangThai != 0)
                    return new SaveResult { Success = false, IsClosed = true, Message = "Ngày làm việc hôm nay đã được chốt." };

                List<KbgLogRow> logs = _repo.GetLogsOfDay(userId, today);
                KbgLogRow running = logs.FirstOrDefault(l => !l.KetThucLuc.HasValue);

                if (running != null)
                {
                    if (running.IdCongViec == taskId)
                        return SaveResult.Ok("Công việc này đang chạy.", dayRow.TongPhut, now);

                    return SaveResult.Fail("Đang có một công việc khác đang chạy. Vui lòng bấm Dừng công việc đó trước.");
                }

                // Một batch SQL duy nhất (không TransactionScope/MSDTC), đồng thời kiểm tra 'chỉ 1 task chạy' có khóa.
                int started = _repo.StartLogAtomic(dayRow.IdKhaiBaoCa, userId, task.IdCongViec, task.IdDuAn, who, now);
                if (started == 2)
                    return SaveResult.Ok("Công việc này đang chạy.", dayRow.TongPhut, now);
                if (started == 3)
                    return SaveResult.Fail("Đang có một công việc khác đang chạy. Vui lòng bấm Dừng công việc đó trước.");
                if (started == 4)
                    return new SaveResult { Success = false, IsClosed = true, Message = "Ngày làm việc đã được chốt hoặc phiếu không còn hợp lệ." };
                if (started != 1)
                    return SaveResult.Fail("Máy chủ chưa xác nhận tạo phiên bấm giờ. Vui lòng làm mới và thử lại.");

                return SaveResult.Ok("Đã bắt đầu tính giờ cho công việc [" + task.MaCongViec + "].", dayRow.TongPhut, now);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "KhaiBaoGio: lỗi StartOrResumeTask của user " + userId + " cho task " + taskId);
                LogToFile(ex, "StartOrResumeTask user=" + userId + " task=" + taskId);
                return SaveResult.Fail("Không thể bắt đầu tính giờ do lỗi hệ thống." + DebugDetail(ex));
            }
        }

        /// <summary>
        /// Nhấn nút "Dừng" cho công việc đang chạy.
        /// Tự động tính thời gian thực làm (trừ giờ nghỉ trưa) và cập nhật bản ghi tổng hợp ngày.
        /// </summary>
        public SaveResult StopTask(Guid userId, Guid taskId, DateTime now)
        {
            try
            {
                if (userId == Guid.Empty) return SaveResult.Fail("Không xác định được người dùng.");

                DateTime today = now.Date;
                List<ThongTinCa> shifts = LichBieuChungManager.Instance.GetShiftsForDate(today);
                if (shifts == null || shifts.Count == 0)
                    return SaveResult.Fail("Hôm nay không có ca làm việc.");

                List<ThongTinCa> ordered = shifts.OrderBy(s => s.BatDau).ToList();
                DateTime dayEnd = ordered.Last().KetThuc;
                int totalCapacityMinutes = ordered.Sum(s => s.SoPhutCa);

                List<KbgLogRow> logs = _repo.GetLogsOfDay(userId, today);
                KbgLogRow running = taskId != Guid.Empty
                    ? logs.FirstOrDefault(l => !l.KetThucLuc.HasValue && l.IdCongViec == taskId)
                    : logs.FirstOrDefault(l => !l.KetThucLuc.HasValue);

                if (running == null)
                    return SaveResult.Fail("Công việc này hiện không ở trạng thái đang chạy.");

                DateTime effectiveEnd = now < dayEnd ? now : dayEnd;
                if (effectiveEnd < running.BatDauLuc) effectiveEnd = running.BatDauLuc;

                // Tính số giây thực làm của phiên vừa dừng (đã trừ nghỉ trưa)
                int sessionSeconds = CalculateWorkingSeconds(running.BatDauLuc, effectiveEnd, ordered);
                int sessionMinutes = SecondsToMinutes(sessionSeconds);

                // Tính tổng số giây của toàn bộ các phiên thuộc Task này trong hôm nay
                int previousSeconds = logs.Where(l => l.IdCongViec == running.IdCongViec && l.KetThucLuc.HasValue)
                                          .Sum(l => CalculateWorkingSeconds(l.BatDauLuc, l.KetThucLuc.Value, ordered));
                int totalTaskSeconds = previousSeconds + sessionSeconds;
                int totalTaskMinutes = Math.Min(totalCapacityMinutes, SecondsToMinutes(totalTaskSeconds));

                string who = userId.ToString();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TxOptions))
                {
                    _repo.CompleteLogAndSyncTotals(
                        running.IdNhatKy,
                        running.IdChiTiet,
                        running.IdKhaiBaoCa,
                        effectiveEnd,
                        sessionMinutes,
                        1, // 1 = Nhân viên chủ động bấm dừng
                        totalTaskMinutes,
                        totalCapacityMinutes,
                        who,
                        now);
                    scope.Complete();
                }

                KbgShiftRow updatedShift = _repo.GetDayShift(userId, today);
                int newDayTotal = updatedShift != null ? updatedShift.TongPhut : totalTaskMinutes;

                return SaveResult.Ok(
                    "Đã dừng đồng hồ · Tổng thời gian làm việc hôm nay: " + FormatDecimalHours(newDayTotal) + " (" + FormatMinutes(newDayTotal) + ").",
                    newDayTotal,
                    now);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "KhaiBaoGio: lỗi StopTask của user " + userId + " cho task " + taskId);
                LogToFile(ex, "StopTask user=" + userId + " task=" + taskId);
                return SaveResult.Fail("Không thể dừng đồng hồ do lỗi hệ thống." + DebugDetail(ex));
            }
        }

        /// <summary>Giữ lại chữ ký hàm cũ để tránh lỗi biên dịch nếu có nơi khác gọi.</summary>
        public SaveResult SaveShift(Guid userId, DateTime now, byte loaiCa, int phienBan, List<KhaiBaoGioItem> items)
        {
            return SaveResult.Fail("Hệ thống đã chuyển sang chế độ bấm giờ trực tiếp.");
        }

        #endregion

        #region Tự động dừng đồng hồ & Chốt sổ cuối ngày (Sau giờ kết thúc Ca chiều)

        private static readonly object _closeLock = new object();

        // TransactionScope mặc định là Serializable (giữ range-lock trên cả bảng, chặn mọi người dùng khác) -> dùng ReadCommitted.
        private static readonly TransactionOptions TxOptions = new TransactionOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Ngày đã chốt xong: bỏ qua trong 30 phút để không chạy lại các câu UPDATE/INSERT hàng loạt vô ích.
        private static readonly Dictionary<DateTime, DateTime> _dayDoneAt = new Dictionary<DateTime, DateTime>();
        private static DateTime _lastCloseRun = DateTime.MinValue;

        private static DateTime? GetGoLiveDate()
        {
            string raw = System.Web.Configuration.WebConfigurationManager.AppSettings["KhaiBaoGio.GoLiveDate"];
            DateTime d;
            if (!string.IsNullOrWhiteSpace(raw) &&
                DateTime.TryParseExact(raw.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d.Date;
            return null;
        }

        /// <summary>
        /// Tự động chốt các ngày làm việc đã qua giờ kết thúc ca chiều (VD: sau 17:00):
        /// 1. Tự động bấm Dừng (tại mốc 17:00, trừ giờ nghỉ trưa) cho mọi task mà nhân viên quên dừng.
        /// 2. Chốt phiếu ngày (TrangThai = 1) và phân loại Đúng hạn / Trễ hạn.
        /// 3. Tạo dòng "Không khai báo" cho nhân viên có task nhưng cả ngày không bấm giờ.
        /// </summary>
        public int CloseExpiredShifts(DateTime now)
        {
            int closed = 0;
            DateTime? goLive = GetGoLiveDate();
            LichBieuChungManager lich = LichBieuChungManager.Instance;

            for (int back = 3; back >= 0; back--)
            {
                DateTime day = now.Date.AddDays(-back);
                List<ThongTinCa> shifts = lich.GetShiftsForDate(day);
                if (shifts == null || shifts.Count == 0) continue;

                List<ThongTinCa> ordered = shifts.OrderBy(s => s.BatDau).ToList();
                DateTime dayStart = ordered.First().BatDau;
                DateTime dayEnd = ordered.Last().KetThuc;
                int totalCapacityMinutes = ordered.Sum(s => s.SoPhutCa);

                // Chỉ chốt khi đã qua giờ kết thúc của ca cuối cùng trong ngày (ca chiều)
                if (now < dayEnd) continue;

                lock (_dayDoneAt)
                {
                    DateTime doneAt;
                    if (_dayDoneAt.TryGetValue(day, out doneAt) && (now - doneAt).TotalMinutes < (back == 0 ? 30 : 720)) continue;
                }

                bool allowMissed = goLive.HasValue ? day >= goLive.Value : day == now.Date;

                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TxOptions))
                    {
                        // 1. Tự động đóng mọi phiên bấm giờ còn đang chạy của ngày đó tại đúng mốc kết thúc ca chiều
                        List<KbgLogRow> openLogs = _repo.GetOpenLogsByDay(day);
                        foreach (KbgLogRow openLog in openLogs)
                        {
                            List<KbgLogRow> userDayLogs = _repo.GetLogsOfDay(openLog.UserId, day);
                            DateTime effectiveEnd = dayEnd > openLog.BatDauLuc ? dayEnd : openLog.BatDauLuc;

                            int sessionSeconds = CalculateWorkingSeconds(openLog.BatDauLuc, effectiveEnd, ordered);
                            int sessionMinutes = SecondsToMinutes(sessionSeconds);

                            int prevSeconds = userDayLogs
                                .Where(l => l.IdCongViec == openLog.IdCongViec && l.KetThucLuc.HasValue)
                                .Sum(l => CalculateWorkingSeconds(l.BatDauLuc, l.KetThucLuc.Value, ordered));

                            int totalTaskMinutes = Math.Min(totalCapacityMinutes, SecondsToMinutes(prevSeconds + sessionSeconds));

                            _repo.CompleteLogAndSyncTotals(
                                openLog.IdNhatKy,
                                openLog.IdChiTiet,
                                openLog.IdKhaiBaoCa,
                                effectiveEnd,
                                sessionMinutes,
                                2, // 2 = Hệ thống tự động ngắt khi hết giờ làm việc buổi chiều
                                totalTaskMinutes,
                                totalCapacityMinutes,
                                SystemUser,
                                now);
                        }

                        // 2. Chốt toàn bộ phiếu ngày & Phân loại Đúng hạn / Trễ hạn
                        closed += _repo.CloseOpenDayShifts(day, SystemUser, now);
                        _repo.ClassifyClosedEntriesByDay(day, SystemUser, now);

                        // 3. Tạo dòng "Không khai báo" nếu cả ngày không làm
                        if (allowMissed)
                        {
                            _repo.InsertMissedDayShifts(
                                day,
                                ordered.First().GioBatDau,
                                ordered.Last().GioKetThuc,
                                totalCapacityMinutes,
                                ProjectActiveValue,
                                SystemUser,
                                now);
                        }

                        scope.Complete();
                    }

                    lock (_dayDoneAt) { _dayDoneAt[day] = now; }
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "KhaiBaoGio: lỗi chốt cuối ngày " + day.ToString("yyyy-MM-dd"));
                }
            }
            return closed;
        }

        public void TryCloseExpiredShiftsThrottled(DateTime now)
        {
            lock (_closeLock)
            {
                if ((now - _lastCloseRun).TotalSeconds < 60) return;
                _lastCloseRun = now;
            }

            try
            {
                CloseExpiredShifts(now);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "KhaiBaoGio: lỗi chốt cuối ngày định kỳ");
            }
        }

        #endregion
    }
}