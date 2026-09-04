using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardOverviewModel
    {
        public decimal AtRiskProjectRate { get; set; }
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
        public ResourceOverviewModel ResourceOverview { get; set; }
        public CostOverviewModel CostOverview { get; set; }

    }
    public class ProjectStatusStatistic
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class ProjectProgressStatistic
    {
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public decimal Progress { get; set; }

        public decimal PlannedProgress { get; set; }

        public decimal Variance { get; set; }

        public int OverdueTaskCount { get; set; }

        public int DueSoonTaskCount { get; set; }

        public ProjectScheduleHealth Health { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

    }
    public class ProjectAttentionStatistic
    {
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
        public int TotalEmployeeCount { get; set; }

        public int ParticipatingEmployeeCount { get; set; }

        public int UnassignedEmployeeCount { get; set; }

        public int MultiProjectEmployeeCount { get; set; }

        public int AssignedProjectMemberCount { get; set; }

        public int UnassignedProjectMemberCount { get; set; }

        public List<ProjectResourceStatistic> ProjectResourceStatistics { get; set; }
    }

    public class ProjectResourceStatistic
    {
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public int MemberCount { get; set; }

        public int MultiProjectMemberCount { get; set; }
    }
    public class CostOverviewModel
    {
        public decimal TotalContractValue { get; set; }

        public decimal ActualCost { get; set; }

        public decimal ReceivedPayment { get; set; }

        public decimal RemainingAfterCost { get; set; }
    }

}

