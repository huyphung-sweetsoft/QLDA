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

        public ThongTinLichNgay()
        {
            DanhSachCongViec = new List<TomTatCongViec>();
        }
    }
}
