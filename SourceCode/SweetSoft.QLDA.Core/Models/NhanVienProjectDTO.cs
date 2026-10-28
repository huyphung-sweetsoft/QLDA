using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SweetSoft.QLDA.Core.EnumHelper.Defines;

namespace SweetSoft.QLDA.Core.Models
{
    /// <summary>Chỉ chứa dữ liệu đọc của KhaiBaoGio 2.0; không dùng hệ số đóng góp.</summary>
    public static class NhanVienGioCongFormat
    {
        public static string Giay(long value)
        {
            if (value < 0) value = 0;
            long total = (long)Math.Round(value / 60.0, MidpointRounding.AwayFromZero);
            return Phut(total);
        }
        public static string Phut(long value)
        {
            if (value < 0) value = 0;
            return (value / 60).ToString(CultureInfo.InvariantCulture) + "g" + (value % 60).ToString("00", CultureInfo.InvariantCulture);
        }
    }

    public class NhanVienGioCongSummaryDTO
    {
        public long GiayThuongHeThong { get; set; }
        public long GiayTangCaHeThong { get; set; }
        public long PhutThuongTuKhai { get; set; }
        public long PhutTangCaTuKhai { get; set; }
        public long PhutThuongPmChot { get; set; }
        public long PhutTangCaPmChot { get; set; }
        public long TongPhutPmChotDuAn { get; set; }
        public int SoTaskLienQuan { get; set; }
        public int SoTaskCoTuKhai { get; set; }
        public int SoTaskDaChot { get; set; }
        public int SoTaskToanDuAnCanChot { get; set; }
        public int SoTaskToanDuAnDaChot { get; set; }
        public bool CoPhienDangMo { get; set; }
        public byte TrangThaiDuAnCode { get; set; }
        public bool DaKetToanDuAn { get; set; }
        public bool LaDuAnKetThuc { get; set; }
        public bool CoGioHeThong { get; set; }
        public bool CoTuKhai { get; set; }
        public bool CoPmChot { get; set; }
        public long TongGiayHeThong { get { return GiayThuongHeThong + GiayTangCaHeThong; } }
        public string GioHeThongText { get { return CoGioHeThong ? NhanVienGioCongFormat.Giay(TongGiayHeThong) : "Chưa phát sinh"; } }
        public string GioHanhChinhText { get { return NhanVienGioCongFormat.Giay(GiayThuongHeThong); } }
        public string GioTangCaText { get { return NhanVienGioCongFormat.Giay(GiayTangCaHeThong); } }
        public string GioTuKhaiText { get { return CoTuKhai ? NhanVienGioCongFormat.Phut(PhutThuongTuKhai + PhutTangCaTuKhai) : "Chưa tự khai"; } }
        public string GioPmChotText { get { return CoPmChot ? NhanVienGioCongFormat.Phut(PhutThuongPmChot + PhutTangCaPmChot) : "Chưa chốt"; } }
        public string GioTuKhaiThuongText { get { return CoTuKhai ? NhanVienGioCongFormat.Phut(PhutThuongTuKhai) : "—"; } }
        public string GioTuKhaiTangCaText { get { return CoTuKhai ? NhanVienGioCongFormat.Phut(PhutTangCaTuKhai) : "—"; } }
        public string GioPmThuongText { get { return CoPmChot ? NhanVienGioCongFormat.Phut(PhutThuongPmChot) : "—"; } }
        public string GioPmTangCaText { get { return CoPmChot ? NhanVienGioCongFormat.Phut(PhutTangCaPmChot) : "—"; } }
        public bool HienThiTyTrong { get { return DaKetToanDuAn && LaDuAnKetThuc && TongPhutPmChotDuAn > 0; } }
        public string TyTrongText { get { return HienThiTyTrong ? (100.0 * (PhutThuongPmChot + PhutTangCaPmChot) / TongPhutPmChotDuAn).ToString("0.0", new CultureInfo("vi-VN")) + "%" : "—"; } }
        public string TienDoChotText { get { return SoTaskToanDuAnDaChot + "/" + SoTaskToanDuAnCanChot + " công việc đã chốt"; } }
        public string TinhTrangGioText
        {
            get
            {
                if (DaKetToanDuAn) return "Đã kết toán";
                if (LaDuAnKetThuc) return SoTaskToanDuAnCanChot == 0 ? "Không phát sinh công" : "Chờ kết toán";
                if ((DuAnStatus)TrangThaiDuAnCode == DuAnStatus.ChuaBatDau) return CoGioHeThong ? "Có giờ phát sinh" : "Chưa phát sinh";
                if ((DuAnStatus)TrangThaiDuAnCode == DuAnStatus.TamDung) return "Tạm dừng ghi nhận";
                if (CoPhienDangMo) return "Đang ghi nhận";
                if (SoTaskToanDuAnCanChot == 0 && !CoGioHeThong && !CoTuKhai) return "Chưa phát sinh";
                return "Số liệu tạm tính";
            }
        }
        public string TrangThaiGioCss { get { return DaKetToanDuAn ? "nvd-time-final" : LaDuAnKetThuc ? "nvd-time-pending" : "nvd-time-running"; } }
        public string GhiChuGioText
        {
            get
            {
                if (DaKetToanDuAn) return "Dự án đã kết thúc; các công việc liên quan đã được chốt giờ công.";
                if ((DuAnStatus)TrangThaiDuAnCode == DuAnStatus.ChuaBatDau) return CoGioHeThong ? "Dự án chưa bắt đầu nhưng có phiên giờ đã ghi; cần đối chiếu dữ liệu." : "Dự án chưa bắt đầu và chưa có giờ hệ thống được ghi nhận.";
                if ((DuAnStatus)TrangThaiDuAnCode == DuAnStatus.TamDung) return "Dự án tạm dừng; giờ đã phát sinh được giữ lại nhưng chưa phải tổng cuối cùng.";
                if (LaDuAnKetThuc) return "Dự án đã kết thúc nhưng giờ công chưa được kết toán đầy đủ. Không thể tính tỷ trọng chính thức.";
                return "Chỉ tính giờ hệ thống đã kết toán từ các khoảng tính; phiên đang mở có thể chưa được tính đủ. Giờ nhân viên báo và giờ PM chốt là hai nguồn đối chiếu riêng, không cộng vào giờ hệ thống.";
            }
        }
    }

    public class NhanVienTaskDTO : NhanVienGioCongSummaryDTO
    {
        public Guid IdTask { get; set; }
        public Guid IdPhase { get; set; }
        public string MaTask { get; set; }
        public string TenTask { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public DateTime? NgayHoanThanhThucTe { get; set; }
        public byte TrangThaiTask { get; set; }
        public string LyDoTreHan { get; set; }
        public int ThoiHanNgay { get; set; }
        public int DiemUuTien { get; set; }
        public string MaTaskCha { get; set; }
        public string TenTaskCha { get; set; }
        public string TenDoUuTien { get; set; }
        public string TenPhaseGoc { get; set; }
        public bool IsMyTask { get; set; }
        public bool IsCurrentAssignee { get; set; }
        public bool IsArchivedTask { get; set; }
        public bool DaChotTask { get; set; }
        public string TrangThaiChotText { get { return DaChotTask ? "Đã chốt công" : (TrangThaiTask == 2 || TrangThaiTask == 3 ? "Chờ chốt công" : "Chưa hoàn tất"); } }
    }

    public class NhanVienPhaseDTO : NhanVienGioCongSummaryDTO
    {
        public Guid IdPhase { get; set; }
        public string MaPhase { get; set; }
        public string TenPhase { get; set; }
        public List<NhanVienTaskDTO> Tasks { get; set; } = new List<NhanVienTaskDTO>();
        public List<NhanVienTaskDTO> MyTasks { get { return Tasks.Where(x => x.IsMyTask).ToList(); } }
        public DateTime? MinStartDate { get { return Tasks.Where(x => x.NgayBatDau.HasValue).Select(x => x.NgayBatDau).DefaultIfEmpty().Min(); } }
        public DateTime? MaxEndDate { get { return Tasks.Where(x => x.NgayKetThuc.HasValue).Select(x => x.NgayKetThuc).DefaultIfEmpty().Max(); } }
    }

    public class NhanVienProjectSummaryDTO : NhanVienGioCongSummaryDTO
    {
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string VaiTro { get; set; }
        public byte TrangThai { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public DateTime? ProjectActualEndDate { get; set; }
        public bool IsCurrentMember { get; set; }
        public string ProjectEndLabel { get { return ProjectActualEndDate.HasValue ? "Kết thúc thực tế" : "Kết thúc dự kiến"; } }
        public string TrangThaiText { get { return ((DuAnStatus)TrangThai).ToString(); } }
    }

    public class NhanVienProjectDTO : NhanVienProjectSummaryDTO
    {
        public List<NhanVienTaskDTO> Tasks { get; set; } = new List<NhanVienTaskDTO>();
        public List<NhanVienPhaseDTO> Phases { get; set; } = new List<NhanVienPhaseDTO>();
    }
}
