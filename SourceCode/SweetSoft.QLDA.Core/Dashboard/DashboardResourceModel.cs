using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardResourceFilter
    {
        public Guid? ProjectId { get; set; }

        public DateTime AnchorWeekStart { get; set; }

        public int WeekCount { get; set; }
    }

    public class DashboardResourceModel
    {
        public DashboardResourceModel()
        {
            Weeks = new List<ResourceWeekInfo>();
            Months = new List<ResourceMonthInfo>();
            EmployeeLoads = new List<ResourceEmployeeLoad>();
            AttentionEmployees = new List<ResourceEmployeeLoad>();
            TrendStatistics = new List<ResourceTrendStatistic>();
            ProjectAllocations = new List<ResourceProjectAllocation>();
        }

        public DateTime GeneratedAt { get; set; }

        public DateTime WindowStart { get; set; }

        public DateTime WindowEnd { get; set; }

        public DateTime AnchorWeekStart { get; set; }

        public DateTime AnchorWeekEnd { get; set; }

        public int TotalEmployeeCount { get; set; }

        public int AssignedEmployeeCount { get; set; }

        public int UnderloadedEmployeeCount { get; set; }

        public int BalancedEmployeeCount { get; set; }

        public int OverloadedEmployeeCount { get; set; }

        public decimal AverageUtilization { get; set; }

        public List<ResourceWeekInfo> Weeks { get; set; }

        public List<ResourceMonthInfo> Months { get; set; }

        public List<ResourceEmployeeLoad> EmployeeLoads { get; set; }

        public List<ResourceEmployeeLoad> AttentionEmployees { get; set; }

        public List<ResourceTrendStatistic> TrendStatistics { get; set; }

        public List<ResourceProjectAllocation> ProjectAllocations { get; set; }
    }

    public class ResourceWeekInfo
    {
        public ResourceWeekInfo()
        {
            Days = new List<ResourceDayInfo>();
        }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Label { get; set; }

        public bool IsAnchorWeek { get; set; }

        public List<ResourceDayInfo> Days { get; set; }
    }

    public class ResourceDayInfo
    {
        public DateTime Date { get; set; }

        public string DayLabel { get; set; }

        public bool IsToday { get; set; }
    }

    public class ResourceMonthInfo
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Label { get; set; }
    }

    public class ResourceEmployeeLoad
    {
        public ResourceEmployeeLoad()
        {
            DailyLoads = new List<ResourceDailyLoad>();
            WeeklyLoads = new List<ResourceWeeklyLoad>();
            MonthlyLoads = new List<ResourceMonthlyLoad>();
        }

        public Guid EmployeeId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public string DepartmentName { get; set; }

        public string JobTitleName { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal CapacityDays { get; set; }

        public decimal AverageUtilization { get; set; }

        public decimal MaxDailyLoad { get; set; }

        public int ActiveTaskCount { get; set; }

        public int OverloadDayCount { get; set; }

        public decimal OverAllocatedDays { get; set; }

        public ResourceLoadStatus Status { get; set; }

        public List<ResourceDailyLoad> DailyLoads { get; set; }

        public List<ResourceWeeklyLoad> WeeklyLoads { get; set; }

        public List<ResourceMonthlyLoad> MonthlyLoads { get; set; }
    }

    public class ResourceMonthlyLoad
    {
        public ResourceMonthlyLoad()
        {
            WeeklyLoads = new List<ResourceWeeklyLoad>();
        }

        public DateTime MonthStart { get; set; }

        public DateTime MonthEnd { get; set; }

        public string Label { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal CapacityDays { get; set; }

        public decimal AverageUtilization { get; set; }

        public int OverloadWeekCount { get; set; }

        public ResourceLoadStatus Status { get; set; }

        public List<ResourceWeeklyLoad> WeeklyLoads { get; set; }
    }

    public class ResourceWeeklyLoad
    {
        public ResourceWeeklyLoad()
        {
            Projects = new List<ResourceWeeklyProjectAllocation>();
            Tasks = new List<ResourceWeeklyTaskAllocation>();
            DailyLoads = new List<ResourceDailyLoad>();
        }

        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string Label { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal CapacityDays { get; set; }

        public decimal AllocationPercent { get; set; }

        public decimal OverAllocatedDays { get; set; }

        public int OverlapDayCount { get; set; }

        public ResourceLoadStatus Status { get; set; }

        public List<ResourceWeeklyProjectAllocation> Projects { get; set; }

        public List<ResourceWeeklyTaskAllocation> Tasks { get; set; }

        public List<ResourceDailyLoad> DailyLoads { get; set; }
    }

    public class ResourceWeeklyProjectAllocation
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public int TaskCount { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal AllocationPercent { get; set; }
    }

    public class ResourceWeeklyTaskAllocation
    {
        public ResourceWeeklyTaskAllocation()
        {
            ActiveDates = new List<DateTime>();
        }

        public Guid TaskId { get; set; }

        public string TaskCode { get; set; }

        public string TaskName { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal AllocationPercent { get; set; }

        public List<DateTime> ActiveDates { get; set; }
    }

    public class ResourceDailyLoad
    {
        public ResourceDailyLoad()
        {
            Tasks = new List<ResourceTaskAllocation>();
        }

        public DateTime Date { get; set; }

        public decimal AllocationPercent { get; set; }

        public List<ResourceTaskAllocation> Tasks { get; set; }
    }

    public class ResourceTaskAllocation
    {
        public Guid TaskId { get; set; }

        public string TaskCode { get; set; }

        public string TaskName { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal AllocationPercent { get; set; }
    }

    public class ResourceTrendStatistic
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string Label { get; set; }

        public decimal Utilization { get; set; }

        public bool IsForecast { get; set; }
    }

    public class ResourceProjectAllocation
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public int ResourceCount { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal CapacityDays { get; set; }

        public decimal Utilization { get; set; }

        public ResourceLoadStatus Status { get; set; }
    }

    public enum ResourceLoadStatus
    {
        Underloaded = 0,
        Balanced = 1,
        Overloaded = 2
    }
}
