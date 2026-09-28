using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SweetSoft.QLDA.Core.Models
{
    public class NhanVienTaskDTO
    {
        public Guid IdTask { get; set; }
        public string MaTask { get; set; }
        public string TenTask { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public DateTime? NgayHoanThanhThucTe { get; set; }
        public byte TrangThaiTask { get; set; }
        public string TrangThaiText { get; set; }

        // RAW DATA TỪ SQL
        public int ThoiHanNgay { get; set; }
        public int DiemUuTien { get; set; }
        public string MaTaskCha { get; set; }
        public string TenDoUuTien { get; set; }
        // Hệ số đóng góp lấy từ TblHeSoDongGop
        public double HeSoDongGop { get; set; }
        public string TenTaskCha { get; set; } // Hứng tên của Task Group (VD: Cụm Module Đăng nhập)
        public string TenPhaseGoc { get; set; }

        public int AssigneeCount { get; set; }
        public bool IsMyTask { get; set; }

        // ==========================================
        // PIPELINE BƯỚC 1 & 2: CẤP ĐỘ TASK
        // ==========================================
        public double Coefficient => HeSoDongGop;

        public double E_Task => ThoiHanNgay * Coefficient;

        public double E_EmployeeTask => AssigneeCount > 0 ? (E_Task / AssigneeCount) : 0;
    }

    public class NhanVienPhaseDTO
    {
        public Guid IdPhase { get; set; }
        public string MaPhase { get; set; }
        public string TenPhase { get; set; }
        public List<NhanVienTaskDTO> Tasks { get; set; } = new List<NhanVienTaskDTO>();

        // Tự động chốt ngày Min/Max của Phase dựa trên các Leaf-Task
        public DateTime? MinStartDate => Tasks.Any() ? Tasks.Min(t => t.NgayBatDau) : null;
        public DateTime? MaxEndDate => Tasks.Any() ? Tasks.Max(t => t.NgayKetThuc) : null;

        // TÍCH HỢP LICHBIEUCHUNGMANAGER (Core Logic T7, CN, Lễ)
        public int ThoiHanNgay => (MinStartDate.HasValue && MaxEndDate.HasValue)
            ? LichBieuChungManager.Instance.CountWorkingDaysInRange(MinStartDate.Value, MaxEndDate.Value)
            : 0;

        // ==========================================
        // PIPELINE BƯỚC 3 & 4: CẤP ĐỘ PHASE
        // ==========================================
        public double Total_E_All_Employees => Tasks.Sum(t => t.E_Task);

        public double Total_E_My_Employee => Tasks.Where(t => t.IsMyTask).Sum(t => t.E_EmployeeTask);

        // % Contribution cho Giai đoạn
        public double ContributionPercent => Total_E_All_Employees > 0
            ? Math.Round((Total_E_My_Employee / Total_E_All_Employees) * 100, 1)
            : 0;

        // Danh sách Task chỉ dùng để render lên UI (Lọc bỏ task của người khác)
        public List<NhanVienTaskDTO> MyTasks => Tasks.Where(t => t.IsMyTask).ToList();
    }

    public class NhanVienProjectDTO
    {
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string VaiTro { get; set; } // Trích xuất động từ TblVaiTroDuAn
        public byte TrangThai { get; set; }
        public string TrangThaiText
        {
            get
            {
                switch (TrangThai)
                {
                    case 1: return "Đang thực hiện";
                    case 2: return "Đã hoàn thành";
                    case 3: return "Tạm dừng";
                    case 4: return "Đã hủy";
                    default: return "Chưa xác định";
                }
            }
        }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; } // Đã xử lý ISNULL(NgayThucTe, NgayDuKien) từ DB

        public List<NhanVienPhaseDTO> Phases { get; set; } = new List<NhanVienPhaseDTO>();

        // ==========================================
        // PIPELINE BƯỚC 5: CẤP ĐỘ PROJECT
        // ==========================================
        public double Total_E_All_Employees => Phases.Sum(p => p.Total_E_All_Employees);

        public double Total_E_My_Employee => Phases.Sum(p => p.Total_E_My_Employee);

        // % Contribution cho Toàn bộ Dự án
        public double ContributionPercent => Total_E_All_Employees > 0
            ? Math.Round((Total_E_My_Employee / Total_E_All_Employees) * 100, 1)
            : 0;
    }
    public class NhanVienProjectSummaryDTO
    {
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string VaiTro { get; set; }
        public byte TrangThai { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public double Total_E_All_Employees { get; set; }
        public double Total_E_My_Employee { get; set; }
        public double ContributionPercent
        {
            get
            {
                return Total_E_All_Employees > 0
                    ? Math.Round(
                        (Total_E_My_Employee / Total_E_All_Employees) * 100,
                        1
                      )
                    : 0;
            }
        }
    }
}