using SweetSoft.QLDA.Core.EnumHelper.Defines;
using TrangThaiTask = SweetSoft.QLDA.Core.EnumHelper.TrangThaiCongViec;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

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

            // Giữ vị trí công việc trong nhóm ban đầu dù đang bấm giờ.
            // Đồng hồ và nút Dừng được hiển thị ngay trên chính thẻ công việc.
            rows = rows.OrderBy(r => (int)r.Nhom)
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
            // Tranh dual-write: trang 1.0 khong duoc ghi sau khi he thong V2 cutover.
            if (KhaiBaoGio20DaKichHoat())
                return SaveResult.Fail("Trang khai bao gio cu da ngung ghi. Vui long mo giao dien 2.0.");
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
            if (KhaiBaoGio20DaKichHoat())
                return SaveResult.Fail("Trang khai bao gio cu da ngung ghi. Vui long mo giao dien 2.0.");
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
            if (KhaiBaoGio20DaKichHoat()) return 0;
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

        // Co 1 feature flag chung cho writer va job; mac dinh false = V1.
        public static bool KhaiBaoGio20DaKichHoat()
        {
            return string.Equals(ConfigurationManager.AppSettings["KhaiBaoGio20.KichHoat"],
                "true", StringComparison.OrdinalIgnoreCase);
        }

        // =================== API 2.0 de trang WebForms moi su dung ===================
        // Giu nguyen GetTodayView / StartOrResumeTask / StopTask cua giao dien 1.0.
        // Khong tu dong chuyen sang V2 khi markup/code-behind chua duoc nang cap.
        public Kbg20KetQua BatDauGioThuong(Guid idNhanVien, Guid idDuAn, Guid idCongViec,
            Guid idThaoTac, bool chapNhanQuaHan = false, bool chapNhanPhuThuoc = false)
        {
            return new KhaiBaoGio20Manager().BatDauGioThuong(idNhanVien, idDuAn,
                idCongViec, idThaoTac, chapNhanQuaHan, chapNhanPhuThuoc);
        }

        public Kbg20KetQua BatDauTangCa(Guid idNhanVien, Guid idDuAn,
            Guid idCongViec, Guid idThaoTac)
        {
            return new KhaiBaoGio20Manager().BatDauTangCa(idNhanVien, idDuAn,
                idCongViec, idThaoTac);
        }

        public Kbg20KetQua DungPhien(Guid idNhanVien, Guid idPhien)
        {
            return new KhaiBaoGio20Manager().DungPhien(idNhanVien, idPhien);
        }

        public List<Kbg20Phien> LayCacDongHoDangHoatDong(Guid idNhanVien)
        {
            return new KhaiBaoGio20Manager().LayCacDongHoDangHoatDong(idNhanVien);
        }
    }
}

// ============================================================================
// KHAI BAO GIO 2.0 — BO SUNG VAO CUNG FILE, KHONG XOA API 1.0.
// - 1.0 phuc vu trang WebForms cu, chua ket noi UI 2.0.
// - 2.0 la API moi, doc/ghi dung 3 bang V2; khong dual-write 3 bang cu.
// ============================================================================
namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// Loi 2.0: DOC/GHI bang moi, KHONG tu dong chuyen traffic cua web forms cu.
    /// Moi thoi diem SQL la UTC (datetime2(0)); lich lam viec tinh theo gio Viet Nam.
    /// </summary>
    public sealed class KhaiBaoGio20Manager
    {
        private readonly KhaiBaoGio20Repository _repo;
        private readonly TimeZoneInfo _muiGio;
        public KhaiBaoGio20Manager()
        {
            _repo = new KhaiBaoGio20Repository();
            try { _muiGio = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            catch (TimeZoneNotFoundException) { _muiGio = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        }
        private static DateTime ChuanGiay(DateTime dt)
        {
            DateTime utc = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
            return new DateTime(utc.Ticks - utc.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        }
        private DateTime GioDiaPhuong(DateTime utc) { return TimeZoneInfo.ConvertTimeFromUtc(utc, _muiGio); }
        private DateTime GioUtc(DateTime local)
        {
            return ChuanGiay(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), _muiGio));
        }
        private List<ThongTinCa> LayCa(DateTime ngay)
        {
            LichBieuChungManager lich = LichBieuChungManager.Instance;
            // Lich ngoai le chi la ngay nghi: khong doc LaNgayLamViec cu.
            if (lich.GetExceptionByDate(ngay) != null) return new List<ThongTinCa>();
            return lich.GetShiftsForDate(ngay) ?? new List<ThongTinCa>();
        }
        private bool TrongCa(DateTime local)
        {
            return LayCa(local.Date).Any(c => local >= c.BatDau && local < c.KetThuc);
        }
        private DateTime? MocHetHanUtc(KhaiBaoGio20Repository.CongViec task)
        {
            if (!task.NgayKetThuc.HasValue) return null;
            DateTime end = task.NgayKetThuc.Value.Date;
            // End date cua task la ngay, chot tai cuoi ca cuoi cung <= ngay do.
            for (int i = 0; i < 730; i++)
            {
                DateTime day = end.AddDays(-i);
                if (task.NgayBatDau.HasValue && day < task.NgayBatDau.Value.Date) break;
                var shifts = LayCa(day);
                if (shifts.Count > 0) return GioUtc(shifts.Max(x => x.KetThuc));
            }
            return GioUtc(end.AddDays(1));
        }
        private DateTime MocNgatTangCaUtc(DateTime batDauUtc)
        {
            DateTime batDau = GioDiaPhuong(batDauUtc);
            // Gio bat dau ca SOM NHAT cua ngay lam viec tiep theo.
            // Truong hop cuoi tuan/le -> tiep tuc toi ca dau tien sau ngay nghi.
            for (int i = 1; i <= 730; i++)
            {
                var ca = LayCa(batDau.Date.AddDays(i));
                if (ca.Count > 0) return GioUtc(ca.Min(s => s.BatDau));
            }
            throw new InvalidOperationException("Khong tim thay ca lam viec tiep theo de tu ngat tang ca.");
        }
        // Lay enum Task lam chuan. ChoDuyet la co cu duoc giu lam hang rao tuong thich.
        private static bool HopLe(KhaiBaoGio20Repository.CongViec task)
        {
            return task != null && !task.ChoDuyet &&
                (task.TrangThai == (int)TrangThaiTask.ChuaBatDau ||
                 task.TrangThai == (int)TrangThaiTask.DangThucHien);
        }

        private static string LoiTrangThaiKhongTheStart(KhaiBaoGio20Repository.CongViec task)
        {
            if (task == null) return "Khong tim thay cong viec.";
            if (task.TrangThai == (int)TrangThaiTask.ChoDuyet || task.ChoDuyet)
                return "Cong viec dang cho PM duyet hoan thanh; khong the bat dau phien moi.";
            if (task.TrangThai == (int)TrangThaiTask.HoanThanh ||
                task.TrangThai == (int)TrangThaiTask.HoanThanhTre)
                return "Cong viec da hoan thanh; khong the bat dau phien moi.";
            return "Trang thai cong viec hien tai khong cho phep bat dau tinh gio.";
        }
        private void KiemTraDuAn(SqlConnection con, SqlTransaction tx, Guid duAn)
        {
            using (var cmd = KhaiBaoGio20Repository.Lenh(con, tx,
                "SELECT TrangThai FROM dbo.TblDuAn WITH(HOLDLOCK) WHERE IdDuAn=@D AND ISNULL(DaXoa,0)=0"))
            {
                KhaiBaoGio20Repository.P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                object val = cmd.ExecuteScalar();
                if (val == null || Convert.ToInt32(val) != (int)SweetSoft.QLDA.Core.EnumHelper.Defines.DuAnStatus.DangThucHien)
                    throw new InvalidOperationException("Du an khong o trang thai dang thuc hien.");
            }
        }
        private List<KhaiBaoGio20Repository.CongViec> LayToTien(SqlConnection con, SqlTransaction tx,
             KhaiBaoGio20Repository.CongViec conTask)
        {
            var result = new List<KhaiBaoGio20Repository.CongViec>();
            var seen = new HashSet<Guid> { conTask.IdCongViec };
            Guid? id = conTask.IdCongViecCha;
            while (id.HasValue)
            {
                if (!seen.Add(id.Value) || seen.Count > 100)
                    throw new InvalidOperationException("Phat hien chu trinh hoac nhieu cap khong hop le trong cay cong viec.");
                var cha = _repo.LayCongViec(con, tx, id.Value);
                if (cha == null || cha.IdDuAn != conTask.IdDuAn)
                    throw new InvalidOperationException("Cay cong viec khong hop le hoac chua duoc dong bo.");
                result.Add(cha);
                id = cha.IdCongViecCha;
            }
            return result;
        }
        // API V2 hien hanh van dung transaction rieng va quy tac cu.
        public Kbg20KetQua BatDauGioThuong(Guid idNhanVien, Guid idDuAn, Guid idCongViec,
            Guid idThaoTac, bool chapNhanQuaHan = false, bool chapNhanPhuThuoc = false)
        {
            if (idNhanVien == Guid.Empty || idDuAn == Guid.Empty || idCongViec == Guid.Empty || idThaoTac == Guid.Empty)
                return Kbg20KetQua.Loi("Thieu ma nhan vien, du an, cong viec hoac thao tac.");
            DateTime nowUtc = ChuanGiay(DateTime.UtcNow), nowLocal = GioDiaPhuong(nowUtc);
            if (!TrongCa(nowLocal)) return Kbg20KetQua.Loi("Chi bat dau gio thuong trong ca lam viec hop le.");
            try
            {
                using (var con=_repo.MoKetNoi())
                using (var tx=con.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    return BatDauGioThuongChung(con,tx,idNhanVien,idDuAn,idCongViec,idThaoTac,
                        chapNhanQuaHan,chapNhanPhuThuoc,nowUtc,nowLocal,()=>tx.Commit());
            }
            catch (SqlException ex) when (ex.Number==2601 || ex.Number==2627)
            { return Kbg20KetQua.Loi("Dong ho da duoc tao boi yeu cau khac. Vui long tai lai danh sach."); }
            catch (Exception ex)
            {
                string ma=Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant();
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,"KBG20 BatDauGioThuong | Ma="+ma+" | Task="+idCongViec); }
                catch { System.Diagnostics.Trace.TraceError("KBG20 BatDauGioThuong " + ma + " " + ex); }
                return Kbg20KetQua.Loi("Khong the bat dau dong ho do loi he thong. Ma tham chieu: "+ma);
            }
        }

        // Chi dung ben trong transaction CHUNG cua CtrlStartTask khi host KhaiBaoGio 2.0.
        // Khong mo local SqlTransaction khi TransactionScope da hoat dong.
        public Kbg20KetQua BatDauGioThuongTrongTransactionScope(Guid idNhanVien,Guid idDuAn,Guid idCongViec,
            Guid idThaoTac,bool chapNhanQuaHan,bool chapNhanPhuThuoc)
        {
            if(Transaction.Current==null) throw new InvalidOperationException("Start Task + dong ho phai trong transaction chung.");
            if(idNhanVien==Guid.Empty || idDuAn==Guid.Empty || idCongViec==Guid.Empty || idThaoTac==Guid.Empty)
                throw new InvalidOperationException("Thong tin Start khong hop le.");
            DateTime now=ChuanGiay(DateTime.UtcNow),local=GioDiaPhuong(now);
            if(!TrongCa(local)) throw new InvalidOperationException("Chi bat dau gio thuong trong ca lam viec hop le.");
            using(var con=MoKetNoiTrongTransactionScope())
            {
                var result=BatDauGioThuongChung(con,null,idNhanVien,idDuAn,idCongViec,idThaoTac,
                    chapNhanQuaHan,chapNhanPhuThuoc,now,local,null);
                if(!result.ThanhCong) throw new InvalidOperationException(result.ThongBao);
                return result;
            }
        }

        private SqlConnection MoKetNoiTrongTransactionScope()
        {
            if(Transaction.Current==null) throw new InvalidOperationException("Khong co transaction.");
            var builder=new SqlConnectionStringBuilder(_repo.ChuoiKetNoi);
            if(!builder.Enlist) throw new InvalidOperationException("SQL KBG20 phai co Enlist=true.");
            var c=new SqlConnection(builder.ConnectionString);
            c.Open(); return c;
        }

        // Phan logic Start DUOC CHUYEN NGUYEN VEN tu BatDauGioThuong de DUNG CHUNG.
        // commit != null => API cu commit local transaction; null => host commit TransactionScope.
        private Kbg20KetQua BatDauGioThuongChung(SqlConnection con,SqlTransaction tx,
            Guid idNhanVien, Guid idDuAn, Guid idCongViec,Guid idThaoTac,
            bool chapNhanQuaHan,bool chapNhanPhuThuoc,DateTime nowUtc,DateTime nowLocal,Action commit)
        {
            string buocXuLy = "Start";
                    buocXuLy = "Khoa giao dich du an";
                    _repo.KhoaTheoDuAn(con, tx, idDuAn);
                    buocXuLy = "Kiem tra trang thai du an";
                    KiemTraDuAn(con, tx, idDuAn);
                    buocXuLy = "Kiem tra thao tac Start trung";
                    var lapLai = _repo.LayPhienTheoThaoTac(con, tx, idThaoTac, idNhanVien, idCongViec, Kbg20LoaiGio.Thuong);
                    if (lapLai != null)
                    {
                        if (commit != null) commit();
                        var daXuLy = Kbg20KetQua.Ok("Thao tac Start nay da duoc ghi nhan; khong tao phien trung.");
                        daXuLy.IdPhien = lapLai.IdPhien;
                        return daXuLy;
                    }
                    buocXuLy = "Doc cong viec";
                    var task = _repo.LayCongViec(con, tx, idCongViec);
                    if (task == null || task.IdDuAn != idDuAn)
                        return Kbg20KetQua.Loi("Cong viec khong ton tai hoac khong thuoc du an dang xem.");
                    if (!HopLe(task)) return Kbg20KetQua.Loi(LoiTrangThaiKhongTheStart(task));
                    buocXuLy = "Kiem tra phan cong truc tiep";
                    if (!_repo.CoPhanCong(con, tx, idCongViec, idNhanVien)) return Kbg20KetQua.Loi("Ban khong duoc phan cong truc tiep vao cong viec nay.");
                    if (task.NgayBatDau.HasValue && nowLocal.Date < task.NgayBatDau.Value.Date)
                        return Kbg20KetQua.Loi("Chua den ngay bat dau theo ke hoach.");
                    var active = _repo.LayPhienMo(con, tx, idNhanVien, idCongViec, Kbg20LoaiGio.Thuong);
                    if (active != null) { if (commit != null) commit(); var ok = Kbg20KetQua.Ok("Dong ho cong viec dang hoat dong."); ok.IdPhien = active.IdPhien; return ok; }
                    bool overdue = task.NgayKetThuc.HasValue && nowLocal.Date > task.NgayKetThuc.Value.Date;
                    if (overdue && !chapNhanQuaHan) return new Kbg20KetQua { CanXacNhanQuaHan = true, ThongBao = "Cong viec da qua han. Xac nhan tiep tuc tinh gio?" };
                    if (task.IdCongViecPhuThuoc.HasValue)
                    {
                        buocXuLy = "Kiem tra cong viec phu thuoc";
                        var dep = _repo.LayCongViec(con, tx, task.IdCongViecPhuThuoc.Value);
                        if (dep != null &&
                            dep.TrangThai != (int)TrangThaiTask.HoanThanh &&
                            dep.TrangThai != (int)TrangThaiTask.HoanThanhTre && !chapNhanPhuThuoc)
                            return new Kbg20KetQua { CanXacNhanPhuThuoc = true, ThongBao = "Cong viec phu thuoc chua hoan thanh. Xac nhan thuc hien song song?" };
                    }
                    buocXuLy = "Doc chuoi cong viec cha/phase";
                    var ancestors = LayToTien(con, tx, task);
                    var ketQua = Kbg20KetQua.Ok("Da bat dau dong ho " + task.MaCongViec + ".");
                    buocXuLy = "Tao phien cong viec duoc bam";
                    ketQua.IdPhien = _repo.TaoPhien(con, tx, idNhanVien, task, Kbg20LoaiGio.Thuong,
                             nowUtc, Kbg20NguonBatDau.ThuCong, null, null, idThaoTac, overdue);
                    buocXuLy = "Chuyen trang thai cong viec duoc bam";
                    _repo.ChuyenTrangThaiDangLam(con, tx, task.IdCongViec, nowLocal);
                    foreach (var cha in ancestors)
                    {
                        buocXuLy = "Kiem tra cong viec cha/phase " + cha.MaCongViec;
                        if (!HopLe(cha)) { ketQua.CanhBao.Add("Bo qua dong ho cha " + cha.MaCongViec + " (bi khoa/cho duyet/da hoan thanh)."); continue; }
                        if (cha.NgayBatDau.HasValue && nowLocal.Date < cha.NgayBatDau.Value.Date)
                        { ketQua.CanhBao.Add("Bo qua dong ho cha " + cha.MaCongViec + " (chua toi ngay bat dau)."); continue; }
                        DateTime? moc = MocHetHanUtc(cha);
                        if (moc.HasValue && nowUtc >= moc.Value)
                        { ketQua.CanhBao.Add("Bo qua dong ho cha " + cha.MaCongViec + " (da het han; can xu ly rieng)."); continue; }
                        buocXuLy = "Chuyen trang thai cha/phase " + cha.MaCongViec;
                        _repo.ChuyenTrangThaiDangLam(con, tx, cha.IdCongViec, nowLocal);
                        buocXuLy = "Lay nhan vien cua cha/phase " + cha.MaCongViec;
                        foreach (Guid nv in _repo.LayNguoiPhanCong(con, tx, cha.IdCongViec))
                        {
                            // Dung quy tac: neu da tung Start (bat ke da dung), KHONG tu Resume.
                            buocXuLy = "Kiem tra da Start cha/phase " + cha.MaCongViec;
                            if (_repo.DaTungBatDau(con, tx, nv, cha.IdCongViec)) continue;
                            buocXuLy = "Tao phien tu dong cha/phase " + cha.MaCongViec;
                            _repo.TaoPhien(con, tx, nv, cha, Kbg20LoaiGio.Thuong, nowUtc,
                                Kbg20NguonBatDau.TuDongTuCongViecCon, task.IdCongViec, idNhanVien, idThaoTac, false);
                            buocXuLy = "Ghi thong bao web cho cha/phase " + cha.MaCongViec;
                            _repo.TaoThongBaoWeb(con, tx, nv, cha, task, nowLocal);
                            ketQua.SoPhienTuDong++;
                        }
                    }
                    buocXuLy = "Commit transaction Start";
                    if (commit != null) commit();
                    return ketQua;
        }

        // CHI dung trong luong gui duyet / PM xac nhan task. KHONG cham vao phien task cha/con khac.
        public int ChotTatCaPhienTaskTrongTransactionScope(Guid idDuAn,Guid idCongViec)
        {
            if(Transaction.Current==null) throw new InvalidOperationException("Chot timer va doi trang thai Task phai chung transaction.");
            using(var con=MoKetNoiTrongTransactionScope())
            {
                _repo.KhoaTheoDuAn(con,null,idDuAn);
                var task=_repo.LayCongViec(con,null,idCongViec);
                if(task==null || task.IdDuAn!=idDuAn) throw new InvalidOperationException("Task khong ton tai trong du an.");
                var list=_repo.LayPhienMoCuaTask(con,null,idCongViec);
                var utc=ChuanGiay(DateTime.UtcNow);
                foreach(var phien in list)
                {
                    DateTime e=KetToanTrongGiaoDich(con,null,phien,utc,true);
                    _repo.DongPhien(con,null,phien.IdPhien,e,Kbg20LyDoKetThuc.GuiDuyetHoanThanh);
                }
                return list.Count;
            }
        }

        public Kbg20KetQua BatDauTangCa(Guid idNhanVien, Guid idDuAn, Guid idCongViec, Guid idThaoTac)
        {
            if (idNhanVien==Guid.Empty || idDuAn==Guid.Empty || idCongViec==Guid.Empty || idThaoTac==Guid.Empty)
                return Kbg20KetQua.Loi("Thong tin khong hop le.");
            DateTime now=ChuanGiay(DateTime.UtcNow), local=GioDiaPhuong(now);
            var ca=LayCa(local.Date);
            if(ca.Count==0) return Kbg20KetQua.Loi("Chua cau hinh tang ca tren ngay nghi/le.");
            if(local<ca.Max(c=>c.KetThuc)) return Kbg20KetQua.Loi("Chi duoc bat tang ca sau khi ket thuc ca cuoi cung.");
            try {
                using(var con=_repo.MoKetNoi()) using(var tx=con.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    return BatDauTangCaChung(con,tx,idNhanVien,idDuAn,idCongViec,idThaoTac,now,local,()=>tx.Commit());
            } catch(Exception ex) {
                SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,"KBG20 BatDauTangCa");
                return Kbg20KetQua.Loi("Khong bat dau duoc tang ca do loi he thong.");
            }
        }

        public Kbg20KetQua BatDauTangCaTrongTransactionScope(Guid idNhanVien,Guid idDuAn,Guid idCongViec,Guid idThaoTac)
        {
            if(Transaction.Current==null)throw new InvalidOperationException("Tang ca va trang thai Task can transaction chung.");
            if(idNhanVien==Guid.Empty || idDuAn==Guid.Empty || idCongViec==Guid.Empty || idThaoTac==Guid.Empty)
                throw new InvalidOperationException("Thieu thong tin tang ca.");
            DateTime now=ChuanGiay(DateTime.UtcNow),local=GioDiaPhuong(now);
            var ca=LayCa(local.Date);
            if(ca.Count==0 || local<ca.Max(c=>c.KetThuc))
                throw new InvalidOperationException("Chua den khung bat dau tang ca hop le.");
            using(var con=MoKetNoiTrongTransactionScope()) {
                var result=BatDauTangCaChung(con,null,idNhanVien,idDuAn,idCongViec,idThaoTac,now,local,null);
                if(!result.ThanhCong) throw new InvalidOperationException(result.ThongBao);
                return result;
            }
        }

        private Kbg20KetQua BatDauTangCaChung(SqlConnection con,SqlTransaction tx,
            Guid idNhanVien,Guid idDuAn,Guid idCongViec,Guid idThaoTac,DateTime nowUtc,DateTime local,Action commit)
        {
                    _repo.KhoaTheoDuAn(con, tx, idDuAn);
                    KiemTraDuAn(con, tx, idDuAn);
                    var lapLai = _repo.LayPhienTheoThaoTac(con, tx, idThaoTac, idNhanVien, idCongViec, Kbg20LoaiGio.TangCa);
                    if (lapLai != null)
                    {
                        if(commit!=null)commit();
                        var daXuLy = Kbg20KetQua.Ok("Thao tac Start tang ca nay da duoc ghi nhan; khong tao phien trung.");
                        daXuLy.IdPhien = lapLai.IdPhien;
                        return daXuLy;
                    }
                    var task = _repo.LayCongViec(con, tx, idCongViec);
                    if (task == null || task.IdDuAn != idDuAn)
                        return Kbg20KetQua.Loi("Cong viec khong ton tai hoac khong thuoc du an dang xem.");
                    if (!HopLe(task)) return Kbg20KetQua.Loi(LoiTrangThaiKhongTheStart(task));
                    if (!_repo.CoPhanCong(con, tx, idCongViec, idNhanVien))
                        return Kbg20KetQua.Loi("Ban khong duoc phan cong truc tiep vao cong viec nay.");
                    if (task.NgayBatDau.HasValue && local.Date < task.NgayBatDau.Value.Date)
                        return Kbg20KetQua.Loi("Chua toi ngay bat dau cong viec.");
                    var open = _repo.LayPhienMo(con, tx, idNhanVien, idCongViec, Kbg20LoaiGio.TangCa);
                    if (open != null) { if(commit!=null)commit(); var already = Kbg20KetQua.Ok("Dong ho tang ca da chay."); already.IdPhien = open.IdPhien; return already; }
                    bool overdue = task.NgayKetThuc.HasValue && local.Date > task.NgayKetThuc.Value.Date;
                    if (overdue) return Kbg20KetQua.Loi("Tang ca cho task qua han can duoc xac nhan theo quy trinh rieng.");
                    var result = Kbg20KetQua.Ok("Da bat dau tang ca.");
                    result.IdPhien = _repo.TaoPhien(con, tx, idNhanVien, task, Kbg20LoaiGio.TangCa,
                        nowUtc, Kbg20NguonBatDau.ThuCong, null, null, idThaoTac, false);
                    // Tang ca KHONG kich hoat cac dong ho cha.
                    if(commit!=null)commit(); return result;
        }

        // Lay cung AppLock voi timer truoc khi luu trang thai Task cua partner.
        public IDisposable MoKhoaDuAnTrongTransactionScope(Guid duAn)
        {
            var con=MoKetNoiTrongTransactionScope();
            try { _repo.KhoaTheoDuAn(con,null,duAn); return con; }
            catch { con.Dispose(); throw; }
        }
        public Kbg20KetQua DungPhien(Guid idNhanVien, Guid idPhien)
        {
            if (idNhanVien == Guid.Empty || idPhien == Guid.Empty) return Kbg20KetQua.Loi("Thong tin khong hop le.");
            DateTime now = ChuanGiay(DateTime.UtcNow);
            try
            {
                using (var con = _repo.MoKetNoi()) using (var tx = con.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    // Doc de biet du an TRUOC khi khoa row phien; tranh dao thu tu khoa
                    // Start: AppLock -> Session, Stop: Session -> AppLock.
                    var soBo = _repo.LayPhienSoBo(con, tx, idPhien, idNhanVien);
                    if (soBo == null) return Kbg20KetQua.Loi("Phien da dung hoac khong thuoc nhan vien.");
                    _repo.KhoaTheoDuAn(con, tx, soBo.IdDuAn);
                    var phien = _repo.LayPhienMoTheoId(con, tx, idPhien, idNhanVien);
                    if (phien == null) return Kbg20KetQua.Loi("Phien da duoc dong boi yeu cau khac.");
                    // Tam khoa dung thu cong dong ho task cha neu co con chua hoan thanh;
                    // tang ca khong bi khoa vi day la phien tu nguyen.
                    if (phien.LoaiGio == Kbg20LoaiGio.Thuong && _repo.ConHauDueChuaHoanThanh(con, tx, phien.IdCongViec))
                        return Kbg20KetQua.Loi("Chua the dung dong ho giam sat: van con cong viec con chua hoan thanh (quy tac tam thoi).");
                    DateTime mocDong = KetToanTrongGiaoDich(con, tx, phien, now, false);
                    // Neu qua moc tu ngat ma Hangfire chua kip xu ly, ghi dung THOI DIEM
                    // tu ngat thay vi ghi gio nhan vien bam Stop den muon.
                    var lyDo = Kbg20LyDoKetThuc.TuDung;
                    if (mocDong < now)
                        lyDo = phien.LoaiGio == Kbg20LoaiGio.TangCa
                            ? Kbg20LyDoKetThuc.TuNgatTangCa : Kbg20LyDoKetThuc.HetHanCongViec;
                    _repo.DongPhien(con, tx, phien.IdPhien, mocDong, lyDo);
                    tx.Commit(); return Kbg20KetQua.Ok("Da dung dung phien duoc chon, cac phien khac khong anh huong.");
                }
            }
            catch (Exception ex)
            {
                SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex, "KBG20 DungPhien");
                return Kbg20KetQua.Loi("Khong dung duoc phien do loi he thong.");
            }
        }
        // 2A: Dong phien khi go phan cong phai nam TRONG TransactionScope cua TaskManager.
        // Khong tao SqlTransaction rieng (neu dung, gio co the commit trong khi xoa phan cong rollback).
        // Tat ca cac lenh tren connection nay tu dong tham gia Transaction.Current (Enlist=true).
        public sealed class PhienDongBoPhanCong : IDisposable
        {
            private readonly KhaiBaoGio20Manager _manager;
            private readonly SqlConnection _con;
            private readonly Guid _duAn;
            private bool _disposed;

            internal PhienDongBoPhanCong(KhaiBaoGio20Manager manager, Guid duAn)
            {
                if (System.Transactions.Transaction.Current == null)
                    throw new InvalidOperationException("KBG20: can TransactionScope khi cap nhat phan cong.");
                if (!new SqlConnectionStringBuilder(manager._repo.ChuoiKetNoi).Enlist)
                    throw new InvalidOperationException("KBG20: connection string phai cho phep Enlist vao TransactionScope.");
                _manager = manager;
                _duAn = duAn;
                _con = manager._repo.MoKetNoi();
                try
                {
                    // Cung AppLock Transaction/Project voi BatDauGioThuong, BatDauTangCa, DungPhien va Hangfire.
                    manager._repo.KhoaTheoDuAn(_con, null, duAn);
                }
                catch
                {
                    _con.Dispose();
                    throw;
                }
            }

            public KetQuaGoPhanCong ChotPhien(Guid idCongViec, Guid idNhanVien, DateTime utc)
            {
                if (_disposed) throw new ObjectDisposedException("PhienDongBoPhanCong");
                if (idCongViec == Guid.Empty || idNhanVien == Guid.Empty)
                    throw new ArgumentException("Cong viec/nhan vien khong hop le.");
                var task = _manager._repo.LayCongViec(_con, null, idCongViec);
                if (task == null || task.IdDuAn != _duAn)
                    throw new InvalidOperationException("KBG20: cong viec khong thuoc du an cap nhat phan cong.");
                DateTime thoiDiemDong = ChuanGiay(utc);
                var result = new KetQuaGoPhanCong();
                foreach (var phien in _manager._repo.LayPhienMoCuaTask(_con, null, idCongViec))
                {
                    if (phien.IdNhanVien != idNhanVien) continue;
                    _manager.KetToanTrongGiaoDich(_con, null, phien, thoiDiemDong, true);
                    // KetThucLuc la luc GO NHAN VIEN, khong phai moc gio ket toan gio trong ca.
                    // Lich ca/deadline chi quyet dinh SO GIAY tinh, khong doi thoi diem go phan cong.
                    _manager._repo.DongPhien(_con, null, phien.IdPhien, thoiDiemDong,
                        Kbg20LyDoKetThuc.GoPhanCong);
                    if (phien.LoaiGio == Kbg20LoaiGio.Thuong) result.SoPhienTrongCa++;
                    else if (phien.LoaiGio == Kbg20LoaiGio.TangCa) result.SoPhienTangCa++;
                }
                return result;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _con.Dispose();
            }
        }

        public sealed class KetQuaGoPhanCong
        {
            public int SoPhienTrongCa { get; internal set; }
            public int SoPhienTangCa { get; internal set; }
            public int TongSoPhien { get { return SoPhienTrongCa + SoPhienTangCa; } }
        }

        public PhienDongBoPhanCong MoDongBoPhanCongTrongTransactionScope(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty) throw new ArgumentException("Du an khong hop le.");
            return new PhienDongBoPhanCong(this, idDuAn);
        }

        // 2B: Dong bo thoi diem hieu luc khi TaskList sua ngay / thoi han / phu thuoc.
        // KHONG doi thuat toan AutoSetDependentTime / AutoSetParentTime cua Task partner.
        // Khoa du an, ket toan duoi LICH CU, roi TaskList tu save + cascade trong CUNG TransactionScope.
        public sealed class PhienDongBoKeHoach : IDisposable
        {
            private readonly KhaiBaoGio20Manager _manager;
            private readonly SqlConnection _con;
            private readonly Guid _idDuAn;
            private List<Kbg20Phien> _phienDangMo;
            private readonly Dictionary<Guid, int> _trangThaiTaskTruocSua = new Dictionary<Guid, int>();
            private DateTime _mocHieuLuc;
            private bool _daChotLichCu;
            private bool _daXuLyLichMoi;
            private bool _disposed;

            internal PhienDongBoKeHoach(KhaiBaoGio20Manager manager, Guid idDuAn)
            {
                if (System.Transactions.Transaction.Current == null)
                    throw new InvalidOperationException("KBG20: Sua ke hoach can TransactionScope chung voi TaskList.");
                if (!new SqlConnectionStringBuilder(manager._repo.ChuoiKetNoi).Enlist)
                    throw new InvalidOperationException("KBG20: SQL connection can Enlist=true de rollback cung TaskList.");
                _manager = manager;
                _idDuAn = idDuAn;
                _con = manager._repo.MoKetNoi();
                try { manager._repo.KhoaTheoDuAn(_con, null, idDuAn); }
                catch { _con.Dispose(); throw; }
            }

            // TaskList doc object truoc khi khoa. Phai doi chieu voi DB sau AppLock de
            // tranh ghi de lich/trang thai neu mot request khac sua cung task.
            public void KiemTraBanGhiGoc(Guid idCongViec, DateTime? batDauCu,
                DateTime? ketThucCu, Guid? phuThuocCu, int trangThaiCu)
            {
                if (_disposed) throw new ObjectDisposedException("PhienDongBoKeHoach");
                if (_daChotLichCu) throw new InvalidOperationException("KBG20: Can kiem tra task truoc khi chot lich cu.");
                var task = _manager._repo.LayCongViec(_con, null, idCongViec);
                if (task == null || task.IdDuAn != _idDuAn ||
                    task.NgayBatDau != batDauCu || task.NgayKetThuc != ketThucCu ||
                    task.IdCongViecPhuThuoc != phuThuocCu || task.TrangThai != trangThaiCu)
                    throw new InvalidOperationException("Du lieu cong viec vua duoc nguoi khac cap nhat. Vui long tai lai popup truoc khi sua.");
            }

            // Chot timestamp SAU khi co AppLock, khong dung thoi gian nhan nut truoc luc cho khoa.
            // Luon dua watermark toi moc hieu luc, ngay ca neu deadline CU da cat khoang gio som hon.
            // Neu khong, khi gia han se vo tinh cong nguoc khoang gio nam sau deadline cu.
            public void ChotGioTheoLichCu()
            {
                if (_disposed) throw new ObjectDisposedException("PhienDongBoKeHoach");
                if (_daChotLichCu) throw new InvalidOperationException("KBG20: Lich cu da duoc chot.");
                _mocHieuLuc = ChuanGiay(DateTime.UtcNow);
                _phienDangMo = _manager._repo.LayPhienMoCuaDuAn(_con, null, _idDuAn);
                foreach (var phien in _phienDangMo)
                {
                    var cv = _manager._repo.LayCongViec(_con, null, phien.IdCongViec);
                    if (cv == null || cv.IdDuAn != _idDuAn)
                        throw new InvalidOperationException("KBG20: Phien mo lien ket task khong hop le, IdPhien=" + phien.IdPhien);
                    _trangThaiTaskTruocSua[phien.IdCongViec] = cv.TrangThai;
                    _manager.KetToanTrongGiaoDich(_con, null, phien, _mocHieuLuc, true);
                    _manager._repo.CapNhatDaTinh(_con, null, phien.IdPhien, _mocHieuLuc);
                }
                _daChotLichCu = true;
            }

            // Sau TaskList task.Save() + cac AutoSet* cua partner: chi xu ly tinh hop le
            // cua phien con mo, KHONG tinh lai cac khoang gio da ghi o lich cu.
            public int KiemTraVaApDungLichMoi()
            {
                if (_disposed) throw new ObjectDisposedException("PhienDongBoKeHoach");
                if (!_daChotLichCu || _daXuLyLichMoi)
                    throw new InvalidOperationException("KBG20: Sai thu tu xu ly sua ke hoach.");
                int soPhienTuDongDong = 0;
                DateTime ngayHieuLuc = _manager.GioDiaPhuong(_mocHieuLuc).Date;
                foreach (var phien in _phienDangMo)
                {
                    var task = _manager._repo.LayCongViec(_con, null, phien.IdCongViec);
                    if (task == null || task.IdDuAn != _idDuAn)
                        throw new InvalidOperationException("Task dang co phien gio da bi xoa/chuyen du an trong luc sua lich.");
                    // Roll-up khong duoc dua task dang co dong ho ve Chua bat dau.
                    // Khong sua AutoSetParentStatus cua partner; bao loi va rollback de PM xem lai.
                    if (_trangThaiTaskTruocSua[phien.IdCongViec] == (int)TrangThaiTask.DangThucHien &&
                        task.TrangThai == (int)TrangThaiTask.ChuaBatDau)
                        throw new InvalidOperationException("Khong the chuyen task [" + task.MaCongViec +
                            "] ve Chua bat dau khi van dang co phien tinh gio.");
                    // Fail closed: khong de lich moi dat task dang tinh gio vao tuong lai.
                    if (task.NgayBatDau.HasValue && task.NgayBatDau.Value.Date > ngayHieuLuc)
                        throw new InvalidOperationException("Khong the doi ngay bat dau cua cong viec [" + task.MaCongViec +
                            "] sang tuong lai vi dang co phien tinh gio. Hay xu ly dong ho truoc.");
                    // Buoc 2B khong tu dong giai quyet gui duyet/hoan thanh hay reset tien do.
                    if (!HopLe(task))
                        throw new InvalidOperationException("Cong viec [" + task.MaCongViec +
                            "] dang co phien gio nhung sau khi sua ke hoach da chuyen sang trang thai khong cho tinh gio. " +
                            "Vui long xu ly trang thai/hoan thanh bang quy trinh rieng.");
                    // Qua han sau thay doi: ket toan theo lich CU xong, dong o moc sua lich.
                    // Phien duoc chap nhan tiep tuc qua han KHONG bi dong tai day.
                    if (phien.LoaiGio == Kbg20LoaiGio.Thuong && !phien.LaTiepTucQuaHan)
                    {
                        DateTime? mocHetHan = _manager.MocHetHanUtc(task);
                        if (mocHetHan.HasValue && _mocHieuLuc >= mocHetHan.Value)
                        {
                            _manager._repo.DongPhien(_con, null, phien.IdPhien, _mocHieuLuc,
                                Kbg20LyDoKetThuc.HetHanCongViec);
                            soPhienTuDongDong++;
                        }
                    }
                    // Tang ca tu dong ngat theo ca tiep theo; sua deadline khong sinh hay hoi sinh phien.
                }
                _daXuLyLichMoi = true;
                return soPhienTuDongDong;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _con.Dispose();
            }
        }

        public PhienDongBoKeHoach MoDongBoKeHoachTrongTransactionScope(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty) throw new ArgumentException("Du an khong hop le.");
            return new PhienDongBoKeHoach(this, idDuAn);
        }

        // Chi danh cho luong hoan thanh sau khi tiep nhan code cua partner:
        // moi nguoi / 2 loai gio / cung task duoc dong chung giao dich hoan thanh.
        // KHONG goi doc lap khi submit; repo hoan thanh can dung cung SqlTransaction.
        internal void DongTatCaPhienCuaTaskTrongGiaoDich(SqlConnection con, SqlTransaction tx, Guid idCongViec, DateTime utc)
        {
            foreach (var phien in _repo.LayPhienMoCuaTask(con, tx, idCongViec))
            {
                DateTime e = KetToanTrongGiaoDich(con, tx, phien, utc, true);
                _repo.DongPhien(con, tx, phien.IdPhien, e, Kbg20LyDoKetThuc.GuiDuyetHoanThanh);
            }
        }
        private DateTime KetToanTrongGiaoDich(SqlConnection con, SqlTransaction tx,
           Kbg20Phien phien, DateTime denUtc, bool tinhDenHet)
        {
            // Khoang [daTinhDen, den): chi tinh mot lan, watermark cap nhat chung transaction.
            DateTime start = phien.DaTinhDenLucUtc ?? phien.BatDauLucUtc;
            DateTime end = denUtc;
            if (end < start) end = start;
            if (phien.KetThucLucUtc.HasValue && end > phien.KetThucLucUtc.Value)
                end = phien.KetThucLucUtc.Value;
            if (phien.LoaiGio == Kbg20LoaiGio.TangCa)
            {
                DateTime cutoff = MocNgatTangCaUtc(phien.BatDauLucUtc);
                if (end > cutoff) end = cutoff;
            }
            else if (!phien.LaTiepTucQuaHan)
            {
                var task = _repo.LayCongViec(con, tx, phien.IdCongViec);
                DateTime? cutoff = task == null ? (DateTime?)null : MocHetHanUtc(task);
                if (cutoff.HasValue && end > cutoff.Value) end = cutoff.Value;
                if (end < start) end = start;
            }
            if (end <= start) return start;
            DateTime startLocal = GioDiaPhuong(start), endLocal = GioDiaPhuong(end);
            for (DateTime day = startLocal.Date; day <= endLocal.Date; day = day.AddDays(1))
            {
                if (phien.LoaiGio == Kbg20LoaiGio.Thuong)
                {
                    foreach (var s in LayCa(day))
                    {
                        DateTime localA = startLocal > s.BatDau ? startLocal : s.BatDau;
                        DateTime localB = endLocal < s.KetThuc ? endLocal : s.KetThuc;
                        if (localB > localA) _repo.GhiKhoang(con, tx, phien.IdPhien, day, GioUtc(localA), GioUtc(localB),
                              "Ca " + s.LoaiCa + " " + s.GioBatDau + "-" + s.GioKetThuc);
                    }
                }
                else
                {
                    // Tang ca: khoang thuc duoc bam bat dau den khi dung/cutoff.
                    // Tach theo ngay dia phuong, KHONG phu thuoc lich gio thuong.
                    DateTime localA = startLocal > day ? startLocal : day;
                    DateTime localB = endLocal < day.AddDays(1) ? endLocal : day.AddDays(1);
                    if (localB > localA) _repo.GhiKhoang(con, tx, phien.IdPhien, day, GioUtc(localA), GioUtc(localB), "Tang ca");
                }
            }
            _repo.CapNhatDaTinh(con, tx, phien.IdPhien, end);
            return end;
        }
        public Kbg20ThongKeKetToan KetToanPhienMo(DateTime utcNow, int soLuong = 100)
        {
            DateTime now = ChuanGiay(utcNow);
            var result = new Kbg20ThongKeKetToan();
            foreach (var item in _repo.LayDanhSachPhienMo(Math.Max(1, Math.Min(soLuong, 500))))
            {
                try
                {
                    using (var con = _repo.MoKetNoi()) using (var tx = con.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        _repo.KhoaTheoDuAn(con, tx, item.IdDuAn);
                        var phien = _repo.LayPhienMoTheoId(con, tx, item.IdPhien, item.IdNhanVien);
                        if (phien == null) continue;
                        // Du lieu phan cong/trang thai co the da thay doi tai TaskManager.
                        // Tu dong dong phien nhu hang rao bao ve, nhung can hook tai
                        // thao tac goc de chot DUNG thoi diem nguoi dung bi go.
                        Kbg20LyDoKetThuc? lyDoBatBuoc =
                            _repo.LayLyDoDongBatBuoc(con, tx, phien.IdCongViec, phien.IdNhanVien);
                        DateTime e = KetToanTrongGiaoDich(con, tx, phien, now, false);
                        bool cutoff = lyDoBatBuoc.HasValue;
                        Kbg20LyDoKetThuc reason = lyDoBatBuoc ?? Kbg20LyDoKetThuc.HetHanCongViec;
                        if (phien.LoaiGio == Kbg20LoaiGio.TangCa)
                        {
                            if (!lyDoBatBuoc.HasValue)
                            {
                                cutoff = now >= MocNgatTangCaUtc(phien.BatDauLucUtc);
                                reason = Kbg20LyDoKetThuc.TuNgatTangCa;
                            }
                        }
                        else if (!phien.LaTiepTucQuaHan)
                        {
                            var task = _repo.LayCongViec(con, tx, phien.IdCongViec);
                            var hetHan = task == null ? (DateTime?)null : MocHetHanUtc(task);
                            if (!lyDoBatBuoc.HasValue)
                                cutoff = hetHan.HasValue && now >= hetHan.Value;
                        }
                        if (cutoff)
                        {
                            _repo.DongPhien(con, tx, phien.IdPhien, e, reason);
                            result.SoPhienDaTuNgat++;
                        }
                        tx.Commit(); result.SoPhienDaXuLy++;
                    }
                }
                catch (Exception ex) { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex, "KBG20 KetToanPhienMo " + item.IdPhien); }
            }
            return result;
        }
        public List<Kbg20Phien> LayCacDongHoDangHoatDong(Guid idNhanVien)
        {
            return _repo.LayPhienTheoNhanVien(idNhanVien);
        }

        // Thanh lich ca phuc vu giao dien V2; giu quy tac NGOAI LE chi la ngay nghi.
        public List<ThongTinCa> LayLichCaTrongNgay(DateTime ngay)
        {
            return LayCa(ngay.Date).OrderBy(x => x.BatDau).ToList();
        }

        // Tong hop THEO NGAY trong du an, khong phai 40 phien Start/Stop gan nhat.
        // Trong ngay hien tai: them phan chua ket toan cua tung phien dang mo.
        // Mot phien xuyen ngay duoc tach theo ngay dia phuong.
        public Kbg20LichSuNgay LayLichSuTheoNgay(Guid idNhanVien, Guid idDuAn, DateTime ngay)
        {
            if (idNhanVien == Guid.Empty || idDuAn == Guid.Empty)
                throw new UnauthorizedAccessException("Khong xac dinh nhan vien hoac du an.");
            if (!_repo.LayDuAnNhanVien(idNhanVien).Any(x => x.IdDuAn == idDuAn))
                throw new UnauthorizedAccessException("Ban khong co quyen xem gio trong du an nay.");
            DateTime nowUtc = ChuanGiay(DateTime.UtcNow);
            DateTime today = GioDiaPhuong(nowUtc).Date;
            DateTime localDay = ngay.Date;
            if (localDay > today || localDay < new DateTime(2000, 1, 1))
                throw new ArgumentOutOfRangeException("ngay", "Ngay xem khong hop le.");

            var kq = new Kbg20LichSuNgay { IdDuAn = idDuAn, NgayLam = localDay, ServerNowUtc = nowUtc };
            kq.CongViec = _repo.LayGioDaKetToanTrongNgay(idNhanVien, idDuAn, localDay);
            var byTask = kq.CongViec.ToDictionary(x => x.IdCongViec);
            var taskHan = new Dictionary<Guid, KhaiBaoGio20Repository.CongViec>();
            // Khong gia dinh ca ngay da chot: job co the dang chay cham, ca thuong co the mo xuyen ngay.
            foreach (var p in _repo.LayPhienTheoNhanVien(idNhanVien))
            {
                if (p.IdDuAn != idDuAn) continue;
                DateTime tuUtc = p.DaTinhDenLucUtc ?? p.BatDauLucUtc;
                if (tuUtc < p.BatDauLucUtc) tuUtc = p.BatDauLucUtc;
                DateTime denUtc = nowUtc;
                if (p.LoaiGio == Kbg20LoaiGio.TangCa)
                {
                    DateTime cutoff = MocNgatTangCaUtc(p.BatDauLucUtc);
                    if (denUtc > cutoff) denUtc = cutoff;
                }
                else if (!p.LaTiepTucQuaHan)
                {
                    KhaiBaoGio20Repository.CongViec cv;
                    if (!taskHan.TryGetValue(p.IdCongViec, out cv))
                    {
                        cv = _repo.LayMocHanCongViec(p.IdCongViec);
                        taskHan[p.IdCongViec] = cv;
                    }
                    DateTime? cutoff = cv == null ? (DateTime?)null : MocHetHanUtc(cv);
                    if (cutoff.HasValue && denUtc > cutoff.Value) denUtc = cutoff.Value;
                }
                if (denUtc <= tuUtc) continue;
                DateTime tu = GioDiaPhuong(tuUtc), den = GioDiaPhuong(denUtc);
                DateTime d0 = localDay, d1 = d0.AddDays(1);
                DateTime a = tu > d0 ? tu : d0;
                DateTime b = den < d1 ? den : d1;
                if (b <= a) continue;
                long giay = 0;
                if (p.LoaiGio == Kbg20LoaiGio.TangCa)
                    giay = (long)(b - a).TotalSeconds;
                else
                    foreach (var ca in LayCa(d0))
                    {
                        DateTime x = a > ca.BatDau ? a : ca.BatDau;
                        DateTime y = b < ca.KetThuc ? b : ca.KetThuc;
                        if (y > x) giay += (long)(y - x).TotalSeconds;
                    }
                if (giay <= 0) continue;
                Kbg20GioTheoNgay task;
                if (!byTask.TryGetValue(p.IdCongViec, out task))
                {
                    // Lay ten task tu DB; LayMocHanCongViec chi tra ngay, khong co Ma/Ten.
                    task = _repo.LayTenCongViecChoLichSu(p.IdCongViec);
                    kq.CongViec.Add(task);
                    byTask[p.IdCongViec] = task;
                }
                if (p.LoaiGio == Kbg20LoaiGio.TangCa) task.SoGiayTangCa += giay;
                else task.SoGiayThuong += giay;
            }
            kq.CongViec = kq.CongViec.OrderBy(x => x.MaCongViec).ThenBy(x => x.TenCongViec).ToList();
            return kq;
        }

        // ===== READ API V2: khong ghi DB va khong thay doi API 1.x =====
        public List<Kbg20DuAn> LayDanhSachDuAn(Guid idNhanVien)
        {
            return _repo.LayDuAnNhanVien(idNhanVien);
        }
        public Kbg20TrangDuAn LayTrangDuAn(Guid idNhanVien, Guid idDuAn)
        {
            if (idNhanVien == Guid.Empty) throw new UnauthorizedAccessException("Chua xac dinh nhan vien.");
            var ketQua = new Kbg20TrangDuAn { ServerNowUtc = ChuanGiay(DateTime.UtcNow) };
            ketQua.DuAn = _repo.LayDuAnNhanVien(idNhanVien);
            if (ketQua.DuAn.Count == 0) return ketQua;
            if (idDuAn == Guid.Empty) idDuAn = ketQua.DuAn[0].IdDuAn;
            if (!ketQua.DuAn.Any(d => d.IdDuAn == idDuAn))
                throw new UnauthorizedAccessException("Ban khong co quyen xem khai bao gio cua du an nay.");
            ketQua.IdDuAn=idDuAn;
            ketQua.CongViec=_repo.LayCongViecCuaNhanVien(idNhanVien,idDuAn);
            ketQua.DongHo=LayDongHoHienThi(idNhanVien,ketQua.ServerNowUtc)
                .Where(d=>d.Phien.IdDuAn==idDuAn).ToList();
            ketQua.LichSu=_repo.LayLichSuPhien(idNhanVien,idDuAn,40);
            // Tong he thong: da ket toan + du lieu ao chua ket toan, rieng tung task va loai gio.
            foreach(var d in ketQua.DongHo) {
                var task=ketQua.CongViec.FirstOrDefault(x=>x.IdCongViec==d.Phien.IdCongViec);
                if(task==null) continue;
                long chuaKetToan=Math.Max(0,d.SoGiayHienTai-d.Phien.SoGiayDaKetToan);
                if(d.Phien.LoaiGio==Kbg20LoaiGio.Thuong) task.SoGiayThuong+=chuaKetToan;
                else task.SoGiayTangCa+=chuaKetToan;
            }
            return ketQua;
        }
        public List<Kbg20DongHo> LayDongHoHienThi(Guid idNhanVien, DateTime utcNow)
        {
            DateTime now=ChuanGiay(utcNow);
            var result=new List<Kbg20DongHo>();
            var taskHan=new Dictionary<Guid,KhaiBaoGio20Repository.CongViec>();
            foreach(var phien in _repo.LayPhienTheoNhanVien(idNhanVien)) {
                DateTime start=phien.DaTinhDenLucUtc??phien.BatDauLucUtc;
                if(start<phien.BatDauLucUtc) start=phien.BatDauLucUtc;
                DateTime end=now;
                if(phien.KetThucLucUtc.HasValue && end>phien.KetThucLucUtc.Value)end=phien.KetThucLucUtc.Value;
                DateTime? cutoff=null;
                if(phien.LoaiGio==Kbg20LoaiGio.TangCa) {
                    cutoff=MocNgatTangCaUtc(phien.BatDauLucUtc);
                } else if(!phien.LaTiepTucQuaHan) {
                    // Chi doc han theo task, khong lay khoa ghi va cache trong cung request.
                    KhaiBaoGio20Repository.CongViec task;
                    if(!taskHan.TryGetValue(phien.IdCongViec,out task)) {
                        task=_repo.LayMocHanCongViec(phien.IdCongViec);
                        taskHan[phien.IdCongViec]=task;
                    }
                    if(task!=null) cutoff=MocHetHanUtc(task);
                }
                if(cutoff.HasValue && end>cutoff.Value)end=cutoff.Value;
                if(end<start)end=start;
                long pending=0;
                bool ticking=false;
                DateTime? boundary=null;
                if(phien.LoaiGio==Kbg20LoaiGio.TangCa) {
                    pending=Math.Max(0,(long)(end-start).TotalSeconds);
                    ticking=now<cutoff && now>=phien.BatDauLucUtc;
                    boundary=cutoff;
                } else {
                    DateTime localStart=GioDiaPhuong(start),localEnd=GioDiaPhuong(end);
                    for(DateTime day=localStart.Date;day<=localEnd.Date;day=day.AddDays(1))
                        foreach(var ca in LayCa(day)) {
                            DateTime a=localStart>ca.BatDau?localStart:ca.BatDau;
                            DateTime b=localEnd<ca.KetThuc?localEnd:ca.KetThuc;
                            if(b>a)pending+=(long)(b-a).TotalSeconds;
                        }
                    DateTime localNow=GioDiaPhuong(now);
                    if(!cutoff.HasValue || now<cutoff.Value) {
                        foreach(var ca in LayCa(localNow.Date))
                            if(localNow>=ca.BatDau && localNow<ca.KetThuc) {
                                ticking=true;boundary=GioUtc(ca.KetThuc);break;
                            }
                    }
                    if(cutoff.HasValue && (!boundary.HasValue || cutoff.Value<boundary.Value))
                        boundary=cutoff;
                }
                result.Add(new Kbg20DongHo { Phien=phien,
                    SoGiayHienTai=phien.SoGiayDaKetToan+pending,
                    DangCongGio=ticking,
                    MocDungCongLucUtc=boundary });
            }
            return result;
        }

    }
}
