using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardOverviewModel
    {
        public int AtRiskProjectCount { get; set; }
        public int ActiveProjectCount { get; set; }
        public int UpcomingMeetingCount { get; set; }
        /// <summary>
        /// Tổng số dự án.
        /// </summary>
        public int TotalProjectCount { get; set; }

        /// <summary>
        /// Tiến độ trung bình của toàn bộ công việc.
        /// </summary>
        public decimal OverallProgress { get; set; }

        /// <summary>
        /// Tổng số công việc.
        /// </summary>
        public int TotalTaskCount { get; set; }

        /// <summary>
        /// Số công việc quá hạn.
        /// </summary>
        public int OverdueTaskCount { get; set; }

        /// <summary>
        /// Tổng giá trị hợp đồng của các dự án thuộc phạm vi lọc.
        /// </summary>
        public decimal TotalContractValue { get; set; }

        /// <summary>
        /// Thời điểm lấy dữ liệu.
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        public List<ProjectStatusStatistic> ProjectStatusStatistics { get; set; }
        public List<ProjectProgressStatistic> ProjectProgressStatistics { get; set; }
        public List<ProjectAttentionStatistic> ProjectAttentionStatistics { get; set; }
        public List<UpcomingMeetingSummary> UpcomingMeetings { get; set; }
        public List<DashboardTaskSummary> TaskDetails { get; set; }
        public List<ProjectEmployeeSummary> ProjectEmployees { get; set; }
        public ResourceOverviewModel ResourceOverview { get; set; }
        public CostOverviewModel CostOverview { get; set; }

    }
    public class ProjectStatusStatistic
    {
        public byte StatusCode { get; set; }
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class ProjectProgressStatistic
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public decimal Progress { get; set; }

        public decimal PlannedProgress { get; set; }

        public decimal Variance { get; set; }

        public int OverdueTaskCount { get; set; }

        public int DueSoonTaskCount { get; set; }

        /// <summary>
        /// Số công việc đã hoàn thành trong dự án.
        /// </summary>
        public int CompletedTaskCount { get; set; }

        /// <summary>
        /// Tổng số công việc của dự án.
        /// </summary>
        public int TaskCount { get; set; }

        public ProjectScheduleHealth Health { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

    }

    /// <summary>
    /// Thông tin rút gọn dùng cho danh sách lịch họp sắp tới trên Overview.
    /// </summary>
    public class UpcomingMeetingSummary
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// Thông tin công việc rút gọn để hiển thị trong danh sách nhanh của KPI.
    /// </summary>
    public class DashboardTaskSummary
    {
        public Guid TaskId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string TaskCode { get; set; }
        public string TaskName { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        /// <summary>Trạng thái vòng đời, không xét quá hạn (0/1/2).</summary>
        public int LifecycleStatusCode { get; set; }
        public DateTime? Deadline { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
    }

    // Dữ liệu của trang tổng quan: trạng thái lưu trên dự án và cảnh báo hạn
    // được giữ riêng để một dự án không bị đếm hai lần trong phân bố trạng thái.
    public class DashboardOverviewSummary
    {
        public DateTime GeneratedAt { get; set; }
        public int ActiveCustomerCount { get; set; }
        public List<OverviewActiveCustomer> ActiveCustomers { get; set; }
        public List<OverviewProjectItem> Projects { get; set; }
        public List<DashboardTaskSummary> Tasks { get; set; }
        public List<DashboardTaskSummary> CurrentTasks { get; set; }
        public OverviewFinanceSummary FinanceSummary { get; set; }
        public List<UpcomingMeetingSummary> Meetings { get; set; }
        public List<ProjectStatusStatistic> Statuses { get; set; }
        public List<OverviewPendingCostItem> PendingCosts { get; set; }
        public List<OverviewImportantIssue> OpenIssues { get; set; }
        public List<OverviewImportantIssue> ImportantIssues { get; set; }
        public List<OverviewRecordedRisk> RecordedRisks { get; set; }
    }

    public class OverviewImportantIssue
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public Guid IssueId { get; set; }
        public string IssueCode { get; set; }
        public string IssueName { get; set; }
        public string Description { get; set; }
        public string HandlingPlan { get; set; }
        public int ImpactLevel { get; set; }
    }

    public class OverviewRecordedRisk
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public Guid RiskId { get; set; }
        public string RiskName { get; set; }
        public int? Probability { get; set; }
        public int? ImpactLevel { get; set; }
        public float? RiskScore { get; set; }
        public string PreventionPlan { get; set; }
        public string ResponsePlan { get; set; }
    }

    public class OverviewPendingCostItem
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string CostCode { get; set; }
        public string CostName { get; set; }
        public decimal Amount { get; set; }
    }

    public class OverviewFinanceSummary
    {
        public OverviewFinanceSummary()
        {
            Projects = new List<OverviewProjectFinanceItem>();
        }

        public decimal ReceivedPayment { get; set; }
        public decimal OutstandingPayment { get; set; }
        public decimal ApprovedCost { get; set; }
        public decimal PendingApprovalCost { get; set; }
        public int ProjectsWithActivityCount { get; set; }
        public List<OverviewProjectFinanceItem> Projects { get; set; }
    }

    public class OverviewProjectFinanceItem
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public decimal ReceivedPayment { get; set; }
        public decimal OutstandingPayment { get; set; }
        public decimal ApprovedCost { get; set; }
        public decimal PendingApprovalCost { get; set; }
    }

    public class OverviewActiveCustomer
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int ProjectCount { get; set; }
    }

    public class OverviewProjectItem
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public byte StatusCode { get; set; }
        public string Status { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public bool IsOverdue { get; set; }
        public int OverdueDays { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int OverdueTaskCount { get; set; }
        public int DueSoonTaskCount { get; set; }
        public int ImportantIssueCount { get; set; }
        public bool NeedsAttention { get { return IsOverdue || OverdueTaskCount > 0 || ImportantIssueCount > 0; } }
    }

    /// <summary>
    /// Thông tin nhân sự thuộc dự án để mở danh sách nhanh từ KPI.
    /// </summary>
    public class ProjectEmployeeSummary
    {
        public Guid EmployeeId { get; set; }
        public string DisplayName { get; set; }
    }
    public class ProjectAttentionStatistic
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public int RiskCount { get; set; }

        public int IssueCount { get; set; }

        public int TotalAttentionCount
        {
            get
            {
                return RiskCount + IssueCount;
            }
        }
    }



    public class ResourceOverviewModel
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public int ParticipatingEmployeeCount { get; set; }

        public int NoLoadEmployeeCount { get; set; }

        public int UnderloadedEmployeeCount { get; set; }

        public int BalancedEmployeeCount { get; set; }

        public int OverloadedEmployeeCount { get; set; }

        public List<DashboardResourceEmployeeItem> EmployeeBreakdown { get; set; }
    }

    public class DashboardResourceEmployeeItem
    {
        public Guid EmployeeId { get; set; }

        public string CategoryKey { get; set; }

        public string DisplayName { get; set; }

        public string DepartmentName { get; set; }

        public decimal AllocatedDays { get; set; }

        public decimal CapacityDays { get; set; }

        public decimal AverageUtilization { get; set; }
    }

    public class CostOverviewModel
    {
        public decimal ApprovedCost { get; set; }

        public decimal PendingApprovalCost { get; set; }

        public decimal RejectedCost { get; set; }

        public decimal TotalCostAmount
        {
            get
            {
                return ApprovedCost + PendingApprovalCost + RejectedCost;
            }
        }

        public List<DashboardOverviewCostItem> CostItems { get; set; }
    }

    public class DashboardOverviewCostItem
    {
        public Guid ProjectId { get; set; }

        public string CategoryKey { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string CostCode { get; set; }

        public string CostName { get; set; }

        public DateTime RecordedDate { get; set; }

        public decimal Amount { get; set; }
    }

}

