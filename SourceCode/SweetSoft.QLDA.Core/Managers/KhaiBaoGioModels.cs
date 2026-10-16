using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>Trạng thái hiển thị của ngày làm việc (hoặc ca) hôm nay.</summary>
    public enum TrangThaiCaView
    {
        ChuaMo = 0,             // chưa đến giờ bắt đầu ngày làm việc
        DangMo = 1,             // đang trong khung giờ làm việc trong ngày
        DaChotCoKhaiBao = 2,    // đã qua giờ kết thúc ca chiều, tổng phút > 0
        DaChotKhongKhaiBao = 3, // đã qua giờ kết thúc ca chiều, có task nhưng tổng phút = 0
        DaChotKhongCoTask = 4   // đã qua giờ kết thúc ca chiều, không có task nào
    }

    /// <summary>Trạng thái nút bấm đồng hồ của từng Task trong ngày.</summary>
    public enum TrangThaiDongHoTask : byte
    {
        ChuaBatDau = 0, // Hiện nút "Bắt đầu" (Start)
        DangChay = 1,   // Hiện nút "Dừng" (Stop) - đồng hồ đang nhảy giây
        TamDung = 2,    // Hiện nút "Tiếp tục" (Resume) - đã có ít nhất 1 lần chạy và đã dừng
        DaKhoa = 3      // Ngày làm việc đã kết thúc hoặc chưa mở, hoặc task không còn hoạt động
    }

    /// <summary>Nhóm hiển thị của task trên trang khai báo giờ.</summary>
    public enum NhomTaskView
    {
        QuaHan = 0,
        HomNay = 1,
        DaKhaiKhongConHoatDong = 2
    }

    /// <summary>Dữ liệu thời gian chỉ đọc dành riêng cho widget nổi toàn website.</summary>
    public class FloatingTimerSnapshot
    {
        public bool Running { get; set; }
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; }
        public string TaskName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public DateTime ServerNow { get; set; }
        public DateTime RunningSince { get; set; }
        public DateTime DayEnd { get; set; }
        public int CompletedSeconds { get; set; }
        public int CapacitySeconds { get; set; }
        public List<ThongTinCa> Segments { get; set; }

        public FloatingTimerSnapshot()
        {
            Segments = new List<ThongTinCa>();
        }
    }

    /// <summary>Một ca làm việc của một ngày cụ thể (lấy từ cấu hình tuần - LichBieuChungManager).</summary>
    public class ThongTinCa
    {
        public byte LoaiCa { get; set; }            // 1 = sáng, 2 = chiều
        public string GioBatDau { get; set; }       // "HH:mm"
        public string GioKetThuc { get; set; }      // "HH:mm"
        public int SoPhutCa { get; set; }           // capacity (phút)
        public DateTime BatDau { get; set; }        // thời điểm bắt đầu ca trong ngày đã hỏi
        public DateTime KetThuc { get; set; }       // thời điểm kết thúc ca trong ngày đã hỏi
    }

    /// <summary>Kết quả trả về khi nhấn nút Bắt đầu / Tiếp tục / Dừng.</summary>
    public class SaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsConflict { get; set; }
        public bool IsClosed { get; set; }
        public int NewVersion { get; set; }
        public int TongPhut { get; set; }
        public DateTime SavedAt { get; set; }

        public static SaveResult Fail(string message)
        {
            return new SaveResult { Success = false, Message = message };
        }

        public static SaveResult Ok(string message, int tongPhut, DateTime now)
        {
            return new SaveResult
            {
                Success = true,
                Message = message,
                TongPhut = tongPhut,
                SavedAt = now
            };
        }
    }

    /// <summary>Giữ lại để tương thích nếu có nơi cũ gọi.</summary>
    public class KhaiBaoGioItem
    {
        public Guid IdCongViec { get; set; }
        public int SoPhut { get; set; }
    }

    /// <summary>Thông tin khung giờ làm việc tổng hợp của cả ngày.</summary>
    public class CaView
    {
        public byte LoaiCa { get; set; }
        public string TenCa { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }
        public string GioNghiTrua { get; set; }
        public int SoPhutCa { get; set; }
        public int TongPhut { get; set; }
        public int TongGiayHienTai { get; set; }
        public int PhienBan { get; set; }
        public TrangThaiCaView State { get; set; }
        public DateTime BatDau { get; set; }
        public DateTime KetThuc { get; set; }
        public DateTime? NghiTruaBatDau { get; set; }
        public DateTime? NghiTruaKetThuc { get; set; }
        public DateTime? LuuLanCuoiLuc { get; set; }
        public Dictionary<Guid, int> PhutTheoTask { get; set; }

        public CaView()
        {
            PhutTheoTask = new Dictionary<Guid, int>();
        }
    }

    public class TaskRowView
    {
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public int TrangThai { get; set; }
        public string TenDoUuTien { get; set; }
        public int DiemUuTien { get; set; }

        public NhomTaskView Nhom { get; set; }
        public bool ShowGroupHeader { get; set; }
        public string GroupTitle { get; set; }
        public int GroupCount { get; set; }

        /// <summary>Task có đủ điều kiện bấm giờ trong ngày hôm nay không.</summary>
        public bool Editable { get; set; }

        /// <summary>Trạng thái đồng hồ hiện tại của Task: Chưa bắt đầu / Đang chạy / Tạm dừng / Đã khóa.</summary>
        public TrangThaiDongHoTask TrangThaiDongHo { get; set; }

        /// <summary>
        /// Có cho phép nhấn nút Bắt đầu / Tiếp tục hay không.
        /// Nếu đang có 1 task khác đang chạy thì cờ này = false để khóa nút.
        /// </summary>
        public bool AllowStartOrResume { get; set; }

        /// <summary>Tổng số phút đã ghi nhận của task trong ngày hôm nay (tính cả phiên đang chạy).</summary>
        public int Phut { get; set; }

        /// <summary>Tổng số giây đã tích lũy của các phiên ĐÃ DỪNG trong ngày hôm nay.</summary>
        public int SoGiayDaTichLuy { get; set; }

        /// <summary>Tổng số giây thực tế tính đến thời điểm hiện tại (SoGiayDaTichLuy + phiên đang chạy).</summary>
        public int TongGiayHienTai { get; set; }

        /// <summary>Thời điểm bắt đầu của phiên đang chạy (nếu TrangThaiDongHo == DangChay).</summary>
        public DateTime? DangChayTuLuc { get; set; }

        /// <summary>Số lần nhân viên đã bấm chạy task này trong ngày hôm nay.</summary>
        public int SoLanBamHomNay { get; set; }

        /// <summary>Giữ lại để tương thích giao diện cũ nếu cần.</summary>
        public int PhutCaKhac { get; set; }
        public string TenCaKhac { get; set; }

        public bool IsOverdue { get; set; }
        public int TreNgayLamViec { get; set; }
        public int NgayThuMay { get; set; }
        public int TongNgayLamViec { get; set; }

        public int LichSuDungHanPhut { get; set; }
        public int LichSuTrePhut { get; set; }
    }

    /// <summary>Một phiên bấm giờ trong ngày, dùng hiển thị nhật ký chỉ đọc.</summary>
    public class KbgJournalSessionView
    {
        public Guid IdCongViec { get; set; }
        public DateTime BatDauLuc { get; set; }
        public DateTime? KetThucLuc { get; set; }
        public int SoGiayThucLam { get; set; }
        public byte? LoaiKetThuc { get; set; }
    }

    public class KhaiBaoGioView
    {
        public DateTime Today { get; set; }
        public DateTime ServerNow { get; set; }
        public bool IsWorkingDay { get; set; }
        public string DayOffReason { get; set; }
        public DateTime? NextWorkingDay { get; set; }

        /// <summary>Thông tin tổng hợp của cả ngày làm việc (chứa 1 phần tử duy nhất đại diện cho cả ngày).</summary>
        public List<CaView> Shifts { get; set; }
        public List<TaskRowView> Tasks { get; set; }
        public List<KbgJournalSessionView> JournalSessions { get; set; }

        /// <summary>Id của công việc đang chạy đồng hồ (Guid.Empty nếu không có task nào đang chạy).</summary>
        public Guid? RunningTaskId { get; set; }

        /// <summary>Có đang nằm trong khung giờ nghỉ trưa (VD: 12:00 - 13:00) hay không.</summary>
        public bool IsLunchBreakNow { get; set; }

        public KhaiBaoGioView()
        {
            Shifts = new List<CaView>();
            Tasks = new List<TaskRowView>();
            JournalSessions = new List<KbgJournalSessionView>();
        }

        /// <summary>Khung làm việc của ngày hôm nay (nếu đang trong giờ làm việc).</summary>
        public CaView OpenShift
        {
            get
            {
                foreach (CaView s in Shifts)
                {
                    if (s.State == TrangThaiCaView.DangMo) return s;
                }
                return null;
            }
        }

        /// <summary>Thông tin ngày làm việc hôm nay (bất kể chưa mở, đang mở hay đã đóng).</summary>
        public CaView DayShift
        {
            get { return Shifts.Count > 0 ? Shifts[0] : null; }
        }
    }
}