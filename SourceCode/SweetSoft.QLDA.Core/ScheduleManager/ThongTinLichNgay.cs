using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ScheduleManager
{
    public class ThongTinLichNgay
    {
        public DateTime Ngay { get; set; }

        // Trạng thái: "free" (Trống), "weekend" (Cuối tuần), "holiday" (Nghỉ lễ), "busy" (Bận)
        public string TrangThaiLich { get; set; }

        // Cờ quyết định xem giao diện có được phép click bật Modal không
        public bool ChoPhepClick { get; set; }

        // Chỉ có giá trị khi TrangThaiLich == "holiday"
        public string TenNgoaiLe { get; set; }

        // Chỉ có giá trị khi TrangThaiLich == "busy"
        public List<TomTatCongViec> DanhSachCongViec { get; set; }

        // Lịch họp là loại sự kiện riêng, không được cộng vào số lượng công việc.
        public List<TomTatCuocHop> DanhSachCuocHop { get; set; }

        public ThongTinLichNgay()
        {
            DanhSachCongViec = new List<TomTatCongViec>();
            DanhSachCuocHop = new List<TomTatCuocHop>();
        }
    }

    /// <summary>Thông tin cuộc họp dùng trong lịch cá nhân, không thay đổi TblLichHop.</summary>
    public class TomTatCuocHop
    {
        public Guid IdLichHop { get; set; }
        public Guid IdDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string MaCuocHop { get; set; }
        public string TenCuocHop { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiemHop { get; set; }
        public bool LaHopOnline { get; set; }
        public Guid IdNguoiTao { get; set; }
        public string TenNguoiChuTri { get; set; }
        public bool LaChuTri { get; set; }
        public bool DaThamGia { get; set; }
    }
}
