using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class EmployeeDashboardModel
    {
        public EmployeeKPIs KPIs { get; set; }
        public List<EmployeeTaskInfo> MyTasks { get; set; }
        public List<ProjectProgressStatistic> MyProjects { get; set; }
        public List<EmployeeWarning> MyWarnings { get; set; }
        public List<EmployeeMeeting> UpcomingMeetings { get; set; }

        public EmployeeDashboardModel()
        {
            KPIs = new EmployeeKPIs();
            MyTasks = new List<EmployeeTaskInfo>();
            MyProjects = new List<ProjectProgressStatistic>();
            MyWarnings = new List<EmployeeWarning>();
            UpcomingMeetings = new List<EmployeeMeeting>();
        }
    }

    public class EmployeeKPIs
    {
        public int OngoingTaskCount { get; set; }
        public int UpcomingDeadlineTaskCount { get; set; }
        public int OverdueTaskCount { get; set; }
        public int ActiveProjectCount { get; set; }
        public int WorkloadPercent { get; set; } // Temporary 0
    }

    public class EmployeeTaskInfo
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public DateTime? Deadline { get; set; }
        public int Progress { get; set; }
    }

    public enum WarningType
    {
        OverdueTask,
        UpcomingTask,
        Overload,
        Meeting
    }

    public class EmployeeWarning
    {
        public WarningType Type { get; set; }
        public string Message { get; set; }
        public string IconClass { get; set; }
        public string TextClass { get; set; }
    }

    public class EmployeeMeeting
    {
        public Guid MeetingId { get; set; }
        public string Title { get; set; }
        public string ProjectName { get; set; }
        public DateTime StartTime { get; set; }
    }
}
