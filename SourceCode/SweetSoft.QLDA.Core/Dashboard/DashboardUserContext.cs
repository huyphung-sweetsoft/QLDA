using System;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardUserContext
    {
        public Guid UserId { get; set; }

        public Guid? EmployeeId { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsEmployee
        {
            get
            {
                return !IsAdmin && EmployeeId.HasValue;
            }
        }
    }
}