using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.iCalendar
{
    /// <summary>
    /// Lớp chính để tạo file ICS
    /// </summary>
    public class IcsGenerator
    {
        private const string CRLF = "\r\n";
        private readonly List<CalendarEvent> _events;
        private string _calendarName;
        private string _calendarDescription;
        private string _timeZone;
        private string _method;
        private string _prodId;

        public IcsGenerator(string calendarName = "My Calendar", string timeZone = "SE Asia Standard Time",
                           string method = "REQUEST", string prodId = "-//Microsoft Corporation//Outlook 16.0 MIMEDIR//EN")
        {
            _events = new List<CalendarEvent>();
            _calendarName = calendarName;
            _timeZone = timeZone;
            _method = method;
            _prodId = prodId;
        }

        /// <summary>
        /// Thêm sự kiện vào calendar
        /// </summary>
        public IcsGenerator AddEvent(CalendarEvent calendarEvent)
        {
            if (calendarEvent == null)
                throw new ArgumentNullException(nameof(calendarEvent));

            _events.Add(calendarEvent);
            return this;
        }

        /// <summary>
        /// Thêm sự kiện đơn giản
        /// </summary>
        public IcsGenerator AddEvent(string summary, DateTime startTime, DateTime endTime,
            string description = null, string location = null)
        {
            var evt = new CalendarEvent(summary, startTime, endTime)
            {
                Description = description,
                Location = location
            };
            return AddEvent(evt);
        }

        /// <summary>
        /// Tạo nội dung ICS theo format Outlook
        /// </summary>
        public string GenerateIcs()
        {
            var sb = new StringBuilder();

            // Calendar header theo format Outlook
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine($"PRODID:{_prodId}");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine($"METHOD:{_method}");

            // Outlook specific properties
            if (_method == "REQUEST")
            {
                sb.AppendLine("X-MS-OLK-FORCEINSPECTOROPEN:TRUE");
            }

            if (!string.IsNullOrEmpty(_calendarName))
            {
                sb.AppendLine($"X-WR-CALNAME:{EscapeString(_calendarName)}");
            }

            if (!string.IsNullOrEmpty(_calendarDescription))
            {
                sb.AppendLine($"X-WR-CALDESC:{EscapeString(_calendarDescription)}");
            }

            // Add timezone information
            AddTimezoneInfo(sb);

            // Add events
            foreach (var evt in _events)
            {
                AddEventToIcs(sb, evt);
            }

            // Calendar footer
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        /// <summary>
        /// Lưu file ICS
        /// </summary>
        public void SaveToFile(string filePath)
        {
            var content = GenerateIcs();
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// Lưu file .ics vào thư mục + tên file, trả về đường dẫn file đã lưu
        /// </summary>
        /// <param name="folderPath">Đường dẫn thư mục lưu file</param>
        /// <param name="fileName">Tên file, ví dụ: "meeting.ics"</param>
        /// <returns>Đường dẫn đầy đủ của file đã lưu</returns>
        public string SaveToFile(string folderPath, string fileName)
        {
            var content = GenerateIcs();

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            File.WriteAllText(fullPath, content, Encoding.UTF8);

            return fullPath;
        }

        /// <summary>
        /// Lấy bytes của file ICS
        /// </summary>
        public byte[] GetBytes()
        {
            var content = GenerateIcs();
            return Encoding.UTF8.GetBytes(content);
        }

        /// <summary>
        /// Đặt tên và mô tả cho calendar
        /// </summary>
        public IcsGenerator SetCalendarInfo(string name, string description = null)
        {
            _calendarName = name;
            _calendarDescription = description;
            return this;
        }

        /// <summary>
        /// Đặt timezone cho calendar
        /// </summary>
        public IcsGenerator SetTimeZone(string timeZone)
        {
            _timeZone = timeZone;
            return this;
        }

        /// <summary>
        /// Đặt method cho calendar (REQUEST, PUBLISH, REPLY, etc.)
        /// </summary>
        public IcsGenerator SetMethod(string method)
        {
            _method = method;
            return this;
        }

        /// <summary>
        /// Đặt PRODID cho calendar
        /// </summary>
        public IcsGenerator SetProdId(string prodId)
        {
            _prodId = prodId;
            return this;
        }

        private void AddEventToIcs(StringBuilder sb, CalendarEvent evt)
        {
            sb.AppendLine("BEGIN:VEVENT");

            // Attendees - phải đặt trước UID theo format Outlook
            foreach (var attendee in evt.Attendees)
            {
                var attendeeLine = $"ATTENDEE;CN=\"{EscapeString(attendee.Name)}\"";
                if (attendee.RsvpRequired)
                {
                    attendeeLine += ";RSVP=TRUE";
                }
                attendeeLine += $":mailto:{attendee.Email}";

                // Wrap long lines theo chuẩn RFC (75 characters per line)
                sb.AppendLine(WrapLongLine(attendeeLine));
            }

            // Event properties
            sb.AppendLine($"CLASS:{evt.EventClass}");
            sb.AppendLine($"CREATED:{FormatDateTime(evt.CreatedTime, true)}");

            if (!string.IsNullOrEmpty(evt.Description))
            {
                sb.AppendLine($"DESCRIPTION:{EscapeString(evt.Description)}");
            }
            else
            {
                sb.AppendLine("DESCRIPTION:\\n"); // Default Outlook format
            }

            // Thời gian với timezone
            if (evt.AllDay)
            {
                sb.AppendLine($"DTSTART;VALUE=DATE:{FormatDate(evt.StartTime)}");
                sb.AppendLine($"DTEND;VALUE=DATE:{FormatDate(evt.EndTime.AddDays(1))}");
            }
            else
            {
                sb.AppendLine($"DTEND;TZID=\"{evt.TimeZone}\":{FormatDateTime(evt.EndTime)}");
                sb.AppendLine($"DTSTAMP:{FormatDateTime(evt.CreatedTime, true)}");
                sb.AppendLine($"DTSTART;TZID=\"{evt.TimeZone}\":{FormatDateTime(evt.StartTime)}");
            }

            sb.AppendLine($"LAST-MODIFIED:{FormatDateTime(evt.LastModified, true)}");

            // Organizer
            if (!string.IsNullOrEmpty(evt.Organizer))
            {
                var organizerLine = $"ORGANIZER;CN=\"{EscapeString(evt.OrganizerName ?? evt.Organizer)}\":mailto:{evt.Organizer}";
                sb.AppendLine(organizerLine);
            }

            sb.AppendLine($"PRIORITY:{(int)evt.Priority}");
            sb.AppendLine($"SEQUENCE:{evt.Sequence}");

            // Summary với language
            var summaryLine = $"SUMMARY;LANGUAGE={evt.Language}:{EscapeString(evt.Summary)}";
            sb.AppendLine(WrapLongLine(summaryLine));

            sb.AppendLine($"TRANSP:{evt.Transparency}");
            sb.AppendLine($"UID:{evt.Uid}");

            // Microsoft specific properties
            sb.AppendLine($"X-MICROSOFT-CDO-BUSYSTATUS:{evt.BusyStatus}");
            sb.AppendLine($"X-MICROSOFT-CDO-IMPORTANCE:{evt.Importance}");
            sb.AppendLine($"X-MICROSOFT-CDO-INTENDEDSTATUS:{evt.IntendedStatus}");
            sb.AppendLine($"X-MICROSOFT-DISALLOW-COUNTER:{evt.DisallowCounter.ToString().ToUpper()}");
            sb.AppendLine($"X-MS-OLK-AUTOSTARTCHECK:{evt.AutoStartCheck.ToString().ToUpper()}");
            sb.AppendLine($"X-MS-OLK-CONFTYPE:{evt.ConfType}");

            // Location nếu có
            if (!string.IsNullOrEmpty(evt.Location))
            {
                sb.AppendLine($"LOCATION:{EscapeString(evt.Location)}");
            }

            // Categories
            if (evt.Categories.Any())
            {
                sb.AppendLine($"CATEGORIES:{string.Join(",", evt.Categories.Select(EscapeString))}");
            }

            // Recurrence rule
            if (evt.Recurrence != null)
            {
                var rrule = $"RRULE:FREQ={evt.Recurrence.Frequency.ToString().ToUpper()}";

                if (evt.Recurrence.Interval > 1)
                {
                    rrule += $";INTERVAL={evt.Recurrence.Interval}";
                }

                if (evt.Recurrence.Count.HasValue)
                {
                    rrule += $";COUNT={evt.Recurrence.Count.Value}";
                }
                else if (evt.Recurrence.Until.HasValue)
                {
                    rrule += $";UNTIL={FormatDateTime(evt.Recurrence.Until.Value, true)}";
                }

                sb.AppendLine(rrule);
            }

            // Alarm/Reminder
            if (evt.HasAlarm)
            {
                sb.AppendLine("BEGIN:VALARM");
                sb.AppendLine($"TRIGGER:-PT{evt.AlarmMinutes}M");
                sb.AppendLine("ACTION:DISPLAY");
                sb.AppendLine("DESCRIPTION:Reminder");
                sb.AppendLine("END:VALARM");
            }

            sb.AppendLine("END:VEVENT");
        }

        private void AddTimezoneInfo(StringBuilder sb)
        {
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine($"TZID:{_timeZone}");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("DTSTART:16010101T000000");

            // Cấu hình timezone phổ biến
            switch (_timeZone)
            {
                case "SE Asia Standard Time":
                case "Asia/Ho_Chi_Minh":
                    sb.AppendLine("TZOFFSETFROM:+0700");
                    sb.AppendLine("TZOFFSETTO:+0700");
                    break;
                case "Tokyo Standard Time":
                case "Asia/Tokyo":
                    sb.AppendLine("TZOFFSETFROM:+0900");
                    sb.AppendLine("TZOFFSETTO:+0900");
                    break;
                case "China Standard Time":
                case "Asia/Shanghai":
                    sb.AppendLine("TZOFFSETFROM:+0800");
                    sb.AppendLine("TZOFFSETTO:+0800");
                    break;
                case "UTC":
                    sb.AppendLine("TZOFFSETFROM:+0000");
                    sb.AppendLine("TZOFFSETTO:+0000");
                    break;
                default:
                    // Default to SE Asia
                    sb.AppendLine("TZOFFSETFROM:+0700");
                    sb.AppendLine("TZOFFSETTO:+0700");
                    break;
            }

            sb.AppendLine("END:STANDARD");
            sb.AppendLine("END:VTIMEZONE");
        }

        private string WrapLongLine(string line, int maxLength = 75)
        {
            if (line.Length <= maxLength)
                return line;

            var sb = new StringBuilder();
            int currentPos = 0;

            while (currentPos < line.Length)
            {
                if (currentPos == 0)
                {
                    // Dòng đầu tiên
                    int takeLength = Math.Min(maxLength, line.Length - currentPos);
                    sb.AppendLine(line.Substring(currentPos, takeLength));
                    currentPos += takeLength;
                }
                else
                {
                    // Các dòng tiếp theo bắt đầu với tab hoặc space
                    int takeLength = Math.Min(maxLength - 1, line.Length - currentPos);
                    sb.AppendLine("\t" + line.Substring(currentPos, takeLength));
                    currentPos += takeLength;
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private string FormatDateTime(DateTime dateTime, bool isUtc = false)
        {
            if (isUtc)
            {
                return dateTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            }
            return dateTime.ToString("yyyyMMddTHHmmss");
        }

        private string FormatDate(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        private string EscapeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("\\", "\\\\")
                .Replace(",", "\\,")
                .Replace(";", "\\;")
                .Replace("\n", "\\n")
                .Replace("\r", "");
        }
    }
}
