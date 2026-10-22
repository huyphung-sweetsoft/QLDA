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

// ============================================================================
// KHAI BAO GIO 2.0 — BO SUNG VAO CUNG FILE, KHONG XOA API 1.0.
// - 1.0 phuc vu trang WebForms cu, chua ket noi UI 2.0.
// - 2.0 la API moi, doc/ghi dung 3 bang V2; khong dual-write 3 bang cu.
// ============================================================================
namespace SweetSoft.QLDA.Core.Managers
{
    // API 2.0 doc lap voi Models cua KhaiBaoGio 1.0.
    public enum Kbg20LoaiGio : byte { Thuong = 1, TangCa = 2 }
    public enum Kbg20NguonBatDau : byte { ThuCong = 1, TuDongTuCongViecCon = 2 }
    public enum Kbg20LyDoKetThuc : byte
    {
        TuDung = 1, HetHanCongViec = 2, GuiDuyetHoanThanh = 3,
        GoPhanCong = 4, TuNgatTangCa = 5, CongViecBiKhoa = 6, QuanTriDieuChinh = 7,
        BaoXongCaNhan = 8
    }
    public class Kbg20KetQua
    {
        public bool ThanhCong { get; set; }
        public bool CanXacNhanQuaHan { get; set; }
        public bool CanXacNhanPhuThuoc { get; set; }
        public string ThongBao { get; set; }
        public Guid? IdPhien { get; set; }
        public int SoPhienTuDong { get; set; }
        public List<string> CanhBao { get; set; } = new List<string>();
        public static Kbg20KetQua Loi(string thongBao) { return new Kbg20KetQua { ThongBao = thongBao }; }
        public static Kbg20KetQua Ok(string thongBao) { return new Kbg20KetQua { ThanhCong = true, ThongBao = thongBao }; }
    }
    public class Kbg20Phien
    {
        public Guid IdPhien { get; set; }
        public Guid IdNhanVien { get; set; }
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public Kbg20LoaiGio LoaiGio { get; set; }
        public DateTime BatDauLucUtc { get; set; }
        public DateTime? KetThucLucUtc { get; set; }
        public DateTime? DaTinhDenLucUtc { get; set; }
        public bool LaTiepTucQuaHan { get; set; }
        public byte NguonBatDau { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public long SoGiayDaKetToan { get; set; }
    }
    public class Kbg20ThongKeKetToan
    {
        public int SoPhienDaXuLy { get; set; }
        public int SoPhienDaTuNgat { get; set; }
        public int SoKhoangDaGhi { get; set; }
    }
    public sealed class Kbg20DuAn
    {
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
    }
    public sealed class Kbg20CongViec
    {
        public int? ThoiHanNgay { get; set; }  // Gia tri thoi han tu TblCongViec, khong tinh lai theo lich.
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public Guid? IdCongViecCha { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public int TrangThai { get; set; }
        public bool ChoDuyet { get; set; }
        public bool LaPhanCongTrucTiep { get; set; }
        // Cần đọc từ phiên (không suy đoán từ số giây; phiên 0 giây vẫn tính là đã bắt đầu).
        public bool DaTungBatDauThuong { get; set; }
        public bool DaTungBatDauTangCa { get; set; }
        public long SoGiayThuong { get; set; }
        public long SoGiayTangCa { get; set; }
    }
    public sealed class Kbg20DongHo
    {
        public Kbg20Phien Phien { get; set; }
        public long SoGiayHienTai { get; set; }
        public bool DangCongGio { get; set; }
        public DateTime? MocDungCongLucUtc { get; set; }
    }
    // Tong gio CUA MOT NGAY dia phuong, chi trong DU AN hien tai va cua chinh nhan vien.
    // Khong tron tong gio task voi thoi gian thuc troi qua; nhieu task cung luc van cong rieng.
    public sealed class Kbg20GioTheoNgay
    {
        public Guid IdCongViec { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public long SoGiayThuong { get; set; }
        public long SoGiayTangCa { get; set; }
        public long TongSoGiay { get { return SoGiayThuong + SoGiayTangCa; } }
    }

    public sealed class Kbg20LichSuNgay
    {
        public Guid IdDuAn { get; set; }
        public DateTime NgayLam { get; set; }
        public DateTime ServerNowUtc { get; set; }
        public List<Kbg20GioTheoNgay> CongViec { get; set; } = new List<Kbg20GioTheoNgay>();
        public long SoGiayThuong { get { long n = 0; foreach (var x in CongViec) n += x.SoGiayThuong; return n; } }
        public long SoGiayTangCa { get { long n = 0; foreach (var x in CongViec) n += x.SoGiayTangCa; return n; } }
        public long TongSoGiay { get { return SoGiayThuong + SoGiayTangCa; } }
    }

    public sealed class Kbg20TrangDuAn
    {
        public Guid IdDuAn { get; set; }
        public DateTime ServerNowUtc { get; set; }
        public List<Kbg20DuAn> DuAn { get; set; } = new List<Kbg20DuAn>();
        public List<Kbg20CongViec> CongViec { get; set; } = new List<Kbg20CongViec>();
        public List<Kbg20DongHo> DongHo { get; set; } = new List<Kbg20DongHo>();
        public List<Kbg20Phien> LichSu { get; set; } = new List<Kbg20Phien>();
    }


}
