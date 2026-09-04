using System;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardFilter
    {
        public Guid? ProjectId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DashboardDateRange DateRange { get; set; }
    }

    public enum DashboardDateRange
    {
        Today = 1,
        ThisWeek = 2,
        ThisMonth = 3,
        ThisQuarter = 4,
        ThisYear = 5,
        Custom = 6
    }
}