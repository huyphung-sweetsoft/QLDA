using SweetSoft.QLDA.Core.iCalendar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.iCalendar
{
    /// <summary>
    /// Lớp đại diện cho một sự kiện calendar
    /// </summary>
    public class CalendarEvent
    {
        public string Uid { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastModified { get; set; }
        public string Organizer { get; set; }
        public string OrganizerName { get; set; }
        public List<Attendee> Attendees { get; set; }
        public EventStatus Status { get; set; }
        public EventPriority Priority { get; set; }
        public bool AllDay { get; set; }
        public RecurrenceRule Recurrence { get; set; }
        public List<string> Categories { get; set; }
        public string TimeZone { get; set; }

        // Outlook specific properties
        public string EventClass { get; set; } = "PUBLIC"; // PUBLIC, PRIVATE, CONFIDENTIAL
        public int Sequence { get; set; } = 0;
        public string Language { get; set; } = "en-us";
        public string Transparency { get; set; } = "OPAQUE"; // OPAQUE, TRANSPARENT
        public string BusyStatus { get; set; } = "TENTATIVE"; // FREE, TENTATIVE, BUSY, OOF
        public int Importance { get; set; } = 1; // 0=Low, 1=Normal, 2=High
        public string IntendedStatus { get; set; } = "BUSY"; // FREE, BUSY
        public bool DisallowCounter { get; set; } = false;
        public bool AutoStartCheck { get; set; } = false;
        public int ConfType { get; set; } = 0;
        public bool ForceInspectorOpen { get; set; } = true;
        public bool HasAlarm { get; set; } = true;
        public int AlarmMinutes { get; set; } = 15; // Nhắc nhở trước bao nhiêu phút

        public CalendarEvent()
        {
            Uid = GenerateOutlookUid();
            CreatedTime = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            Attendees = new List<Attendee>();
            Categories = new List<string>();
            Status = EventStatus.Confirmed;
            Priority = EventPriority.Normal;
            TimeZone = "SE Asia Standard Time";
        }

        public CalendarEvent(string summary, DateTime startTime, DateTime endTime) : this()
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            StartTime = startTime;
            EndTime = endTime;
        }

        private string GenerateOutlookUid()
        {
            // Generate UID theo format của Outlook
            var guid = Guid.NewGuid().ToByteArray();
            var timestamp = DateTime.UtcNow.Ticks;
            var combined = new byte[guid.Length + 8];
            Buffer.BlockCopy(guid, 0, combined, 0, guid.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, combined, guid.Length, 8);
            return "040000008200E00074C5B7101A82E00800000000" + BitConverter.ToString(combined).Replace("-", "");
        }
    }
}
