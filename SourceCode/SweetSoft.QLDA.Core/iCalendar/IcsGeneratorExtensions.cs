using SweetSoft.QLDA.Core.iCalendar.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.iCalendar
{
    /// <summary>
    /// Lớp extension methods để dễ sử dụng hơn
    /// </summary>
    public static class IcsGeneratorExtensions
    {
        /// <summary>
        /// Tạo meeting request theo format Outlook
        /// </summary>
        public static IcsGenerator AddOutlookMeetingRequest(this IcsGenerator generator,
            string title, DateTime startTime, DateTime endTime,
            string organizerEmail, string organizerName,
            string description = null, string location = null,
            params (string email, string name)[] attendees)
        {
            var evt = new CalendarEvent(title, startTime, endTime)
            {
                Description = description ?? "\\n",
                Location = location,
                Organizer = organizerEmail,
                OrganizerName = organizerName,
                TimeZone = "SE Asia Standard Time",
                BusyStatus = "TENTATIVE",
                Importance = 1,
                IntendedStatus = "BUSY"
            };

            // Thêm attendees
            foreach (var (email, name) in attendees)
            {
                evt.Attendees.Add(new Attendee(email, name)
                {
                    RsvpRequired = true
                });
            }

            return generator.AddEvent(evt);
        }

        public static IcsGenerator AddMeeting(this IcsGenerator generator,
            string title, DateTime startTime, TimeSpan duration,
            string location = null, string description = null,
            params string[] attendeeEmails)
        {
            var evt = new CalendarEvent(title, startTime, startTime.Add(duration))
            {
                Location = location,
                Description = description
            };

            foreach (var email in attendeeEmails)
            {
                evt.Attendees.Add(new Attendee(email));
            }

            return generator.AddEvent(evt);
        }

        public static IcsGenerator AddAllDayEvent(this IcsGenerator generator,
            string title, DateTime date, string description = null)
        {
            var evt = new CalendarEvent(title, date.Date, date.Date.AddDays(1))
            {
                AllDay = true,
                Description = description
            };

            return generator.AddEvent(evt);
        }

        public static IcsGenerator AddRecurringEvent(this IcsGenerator generator,
            string title, DateTime startTime, DateTime endTime,
            RecurrenceFrequency frequency, int? count = null,
            string description = null)
        {
            var evt = new CalendarEvent(title, startTime, endTime)
            {
                Description = description,
                Recurrence = new RecurrenceRule(frequency) { Count = count }
            };

            return generator.AddEvent(evt);
        }

        public static string GenerateIcsFileName(string code, DateTime startTime, string participantName)
        {
            string meetingCode = code ?? "meeting";
            string timestamp = startTime.ToString("yyyyMMdd_HHmm");
            string nameSlug = NormalizeFileName(participantName ?? "participant");

            return $"{meetingCode}_{timestamp}_{nameSlug}.ics";
        }

        public static string NormalizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "unknown";

            string normalized = input.Normalize(NormalizationForm.FormD);
            var chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            string noDiacritics = new string(chars);
            string safe = Regex.Replace(noDiacritics, @"[^a-zA-Z0-9]+", "_");
            return safe.Trim('_').ToLowerInvariant();
        }
    }
}
