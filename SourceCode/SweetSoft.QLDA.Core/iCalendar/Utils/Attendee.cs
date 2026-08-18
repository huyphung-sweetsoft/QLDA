using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.iCalendar.Utils
{
    /// <summary>
    /// Lớp đại diện cho một người tham gia
    /// </summary>
    public class Attendee
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } = "REQ-PARTICIPANT"; // REQ-PARTICIPANT, OPT-PARTICIPANT, NON-PARTICIPANT
        public string Status { get; set; } = "NEEDS-ACTION"; // NEEDS-ACTION, ACCEPTED, DECLINED, TENTATIVE
        public bool RsvpRequired { get; set; } = true; // RSVP=TRUE/FALSE

        public Attendee(string email, string name = null)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? email;
        }
    }
}
