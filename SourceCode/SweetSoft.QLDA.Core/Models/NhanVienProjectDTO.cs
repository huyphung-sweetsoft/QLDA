using System;
using System.Collections.Generic;
using System.Linq;

namespace SweetSoft.QLDA.Core.Models
{
    // TẦNG 1: TASK (Công việc chi tiết)
    public class NhanVienTaskDTO
    {
        public Guid IdTask { get; set; }
        public string MaTask { get; set; }
        public string TenTask { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int ThoiHanNgay { get; set; }
        public int DiemUuTien { get; set; }

        // Công thức tính Workload (W = D * P) được nhúng thẳng vào Property
        public float Workload => ThoiHanNgay * DiemUuTien;

        public int TrangThaiTask { get; set; }
        public string TrangThaiText { get; set; }
    }

    // TẦNG 2: PHASE (Giai đoạn / Task cha)
    public class NhanVienPhaseDTO
    {
        public Guid IdPhase { get; set; }
        public string TenPhase { get; set; }
        public DateTime MinStartDate { get; set; }
        public DateTime MaxEndDate { get; set; }

        // Các chỉ số Nguồn lực
        public int CapacityDays { get; set; }
        public float TotalWorkload { get; set; }

        // Bẫy toán học: Chống lỗi chia cho 0. Nếu Capacity = 0, mặc định Allocation = 0
        public float AllocationPercent => CapacityDays > 0 ? (float)Math.Round((TotalWorkload / CapacityDays) * 100, 2) : 0;

        public List<NhanVienTaskDTO> Tasks { get; set; } = new List<NhanVienTaskDTO>();
    }

    // TẦNG 3: PROJECT (Dự án)
    public class NhanVienProjectDTO
    {
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string VaiTro { get; set; }

        public DateTime MinStartDate { get; set; }
        public DateTime MaxEndDate { get; set; }

        public int TrangThai { get; set; }
        // Động cơ định tuyến Trạng thái Dự án
        public string TrangThaiDuAn => TrangThai == 2 ? "Đã hoàn thành" : "Đang thực hiện";

        public List<NhanVienPhaseDTO> Phases { get; set; } = new List<NhanVienPhaseDTO>();
    }
}