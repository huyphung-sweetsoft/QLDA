using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardCostFilter
    {
        public Guid? ProjectId { get; set; }

        public DateTime? CompletedFrom { get; set; }

        public DateTime? CompletedTo { get; set; }

        public DashboardCostPeriod Period { get; set; }
    }

    public enum DashboardCostPeriod
    {
        AllTime = 0,
        ThisMonth = 1,
        ThisQuarter = 2,
        ThisYear = 3
    }

    public class DashboardCostModel
    {
        public DashboardCostModel()
        {
            ProjectStatistics = new List<ProjectCostStatistic>();
            CostTrendStatistics = new List<CostTrendStatistic>();
            LargestCostItems = new List<CostItemInfo>();
        }

        public int CompletedProjectCount { get; set; }

        public decimal TotalContractValue { get; set; }

        public decimal ActualCost { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal ProfitMargin { get; set; }

        public decimal ReceivedPayment { get; set; }

        public decimal OutstandingPayment { get; set; }

        public decimal PaymentCollectionRate { get; set; }

        public decimal AverageCostPerProject { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<ProjectCostStatistic> ProjectStatistics { get; set; }

        public List<CostTrendStatistic> CostTrendStatistics { get; set; }

        public List<CostItemInfo> LargestCostItems { get; set; }
    }

    public class ProjectCostStatistic
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime CompletionDate { get; set; }

        public string ContractNumber { get; set; }

        public decimal ContractValue { get; set; }

        public decimal ActualCost { get; set; }

        public decimal ReceivedPayment { get; set; }

        public decimal OutstandingPayment { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal ProfitMargin { get; set; }

        public int CostItemCount { get; set; }
    }

    public class CostTrendStatistic
    {
        public DateTime Month { get; set; }

        public decimal Amount { get; set; }
    }

    public class CostItemInfo
    {
        public Guid CostId { get; set; }

        public string CostCode { get; set; }

        public string CostName { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime OccurredDate { get; set; }

        public decimal Amount { get; set; }
    }
}
