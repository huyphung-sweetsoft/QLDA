using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardProgressModel
    {
        public DashboardProgressModel()
        {
            TaskStatusStatistics = new List<TaskProgressStatusStatistic>();
            ProjectScheduleStatistics = new List<ProjectScheduleStatistic>();
            ProjectTaskStatistics = new List<ProjectTaskProgressStatistic>();
            TaskProgressDetails = new List<TaskProgressDetail>();
            AttentionTasks = new List<ProgressTaskInfo>();
        }

        public bool IsSingleProject { get; set; }

        public int TotalProjectCount { get; set; }

        public int TotalTaskCount { get; set; }

        public decimal OverallProgress { get; set; }

        public int CompletedTaskCount { get; set; }

        public int InProgressTaskCount { get; set; }

        public int NotStartedTaskCount { get; set; }

        public int OverdueTaskCount { get; set; }

        public int DueSoonTaskCount { get; set; }

        public int NeedsAttentionProjectCount { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<TaskProgressStatusStatistic> TaskStatusStatistics { get; set; }

        public List<ProjectScheduleStatistic> ProjectScheduleStatistics { get; set; }

        public List<ProjectTaskProgressStatistic> ProjectTaskStatistics { get; set; }

        public List<TaskProgressDetail> TaskProgressDetails { get; set; }

        public List<ProgressTaskInfo> AttentionTasks { get; set; }
    }

    public class TaskProgressStatusStatistic
    {
        public string Status { get; set; }

        public int Count { get; set; }
    }

    public class ProjectScheduleStatistic
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        public decimal ActualProgress { get; set; }

        public decimal PlannedProgress { get; set; }

        public decimal Variance { get; set; }

        public int TotalTaskCount { get; set; }

        public int CompletedTaskCount { get; set; }

        public int OverdueTaskCount { get; set; }

        public ProjectScheduleHealth Health { get; set; }
    }

    public class ProjectTaskProgressStatistic
    {
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public int CompletedCount { get; set; }

        public int InProgressCount { get; set; }

        public int NotStartedCount { get; set; }

        public int OverdueCount { get; set; }
    }

    public class ProgressTaskInfo
    {
        public Guid TaskId { get; set; }

        public string TaskCode { get; set; }

        public string TaskName { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string PriorityName { get; set; }

        public int PriorityScore { get; set; }

        public DateTime? Deadline { get; set; }

        public int Progress { get; set; }

        public int DaysToDeadline { get; set; }

        public bool IsOverdue { get; set; }
    }

    public class TaskProgressDetail
    {
        public Guid TaskId { get; set; }

        public string TaskCode { get; set; }

        public string TaskName { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string PriorityName { get; set; }

        public int PriorityScore { get; set; }

        public int Progress { get; set; }

        public string Status { get; set; }

        public int StatusCode { get; set; }

        public DateTime? Deadline { get; set; }

        public int? DaysToDeadline { get; set; }
    }

    public enum ProjectScheduleHealth
    {
        NotStarted = 0,
        OnTrack = 1,
        AtRisk = 2,
        BehindSchedule = 3,
        Overdue = 4,
        Completed = 5
    }
}
