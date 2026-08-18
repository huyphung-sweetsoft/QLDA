using SweetSoft.QLDA.Core.iCalendar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.iCalendar
{
    internal class Test
    {
        public void Main()
        {
            // Tạo calendar đơn giản
            var generator = new IcsGenerator("Lịch công việc", "Asia/Ho_Chi_Minh");

            // Thêm cuộc họp
            generator.AddMeeting(
                "Họp team hàng tuần",
                DateTime.Now.AddDays(1),
                TimeSpan.FromHours(1),
                "Phòng họp A",
                "Thảo luận tiến độ dự án",
                "user1@company.com", "user2@company.com"
            );

            // Thêm sự kiện cả ngày
            generator.AddAllDayEvent("Nghỉ lễ", DateTime.Today.AddDays(7));

            // Thêm sự kiện lặp lại
            generator.AddRecurringEvent(
                "Standup daily",
                DateTime.Today.AddHours(9),
                DateTime.Today.AddHours(9.5),
                RecurrenceFrequency.Daily,
                count: 30
            );

            // Lưu file
            generator.SaveToFile("calendar.ics");

            // Hoặc lấy nội dung
            string icsContent = generator.GenerateIcs();
        }
        public static string CreateOutlookMeetingExample()
        {
            var generator = new IcsGenerator()
                .SetMethod("REQUEST")
                .SetTimeZone("SE Asia Standard Time");

            generator.AddOutlookMeetingRequest(
                "SweetSoft - Triển khai phần mềm quản lý phòng họp, lịch họp",
                new DateTime(2025, 8, 28, 8, 0, 0),  // 8:00 AM
                new DateTime(2025, 8, 28, 9, 30, 0), // 9:30 AM
                "nhi.le@sweetsoft.vn",
                "Nhi Le (SweetSoft)",
                "\\n", // Mô tả trống như Outlook
                null,   // Không có location
                ("huy.phung@sweetsoft.vn", "Phung Quoc Huy (SweetSoft) (huy.phung@sweetsoft.vn)"),
                ("truong.nguyen@sweetsoft.vn", "Nguyen Xuan Truong (SweetSoft) (truong.nguyen@sweetsoft.vn)"),
                ("duy.le@sweetsoft.vn", "Le Van Duy (SweetSoft) (duy.le@sweetsoft.vn)")
            );

            return generator.GenerateIcs();
        }
        /// <summary>
        /// Tạo meeting với các tham số tùy chỉnh hoàn toàn
        /// </summary>
        public static string CreateCustomMeeting(
            string title,
            DateTime startTime,
            DateTime endTime,
            string organizerEmail,
            string organizerName,
            string description = null,
            string location = null,
            List<(string email, string name)> attendees = null,
            string timeZone = "SE Asia Standard Time",
            string method = "REQUEST",
            bool hasReminder = true,
            int reminderMinutes = 15)
        {
            var generator = new IcsGenerator()
                .SetMethod(method)
                .SetTimeZone(timeZone);

            var evt = new CalendarEvent(title, startTime, endTime)
            {
                Description = description ?? "\\n",
                Location = location,
                Organizer = organizerEmail,
                OrganizerName = organizerName,
                TimeZone = timeZone,
                HasAlarm = hasReminder,
                AlarmMinutes = reminderMinutes
            };

            // Thêm attendees nếu có
            if (attendees != null)
            {
                foreach (var (email, name) in attendees)
                {
                    evt.Attendees.Add(new Attendee(email, name) { RsvpRequired = true });
                }
            }

            generator.AddEvent(evt);
            return generator.GenerateIcs();
        }
    }
}
