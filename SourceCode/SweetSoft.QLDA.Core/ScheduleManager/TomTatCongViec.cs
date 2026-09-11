using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ScheduleManager
{
    public class TomTatCongViec
    {
        public Guid IdCongViec { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int TrangThai { get; set; }
    }
}
