using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.iCalendar.Utils
{
    /// <summary>
    /// Lớp đại diện cho thông tin lặp lại sự kiện
    /// </summary>
    public class RecurrenceRule
    {
        public RecurrenceFrequency Frequency { get; set; }
        public int? Count { get; set; } // Số lần lặp lại
        public DateTime? Until { get; set; } // Lặp đến ngày nào
        public int Interval { get; set; } = 1; // Khoảng cách giữa các lần lặp

        public RecurrenceRule(RecurrenceFrequency frequency)
        {
            Frequency = frequency;
        }
    }
}
