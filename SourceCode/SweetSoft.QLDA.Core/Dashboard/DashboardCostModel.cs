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
            ApprovedCostItems = new List<CostItemInfo>();
            PendingApprovalCostItems = new List<CostItemInfo>();
            RejectedCostItems = new List<CostItemInfo>();
            PaymentItems = new List<PaymentItemInfo>();
            PendingPaymentItems = new List<PaymentItemInfo>();
            OverduePayments = new List<OverduePaymentInfo>();
        }

        public int ProjectCount { get; set; }

        public decimal TotalContractValue { get; set; }

        public decimal ActualCost { get; set; }

        /// <summary>
        /// Tổng giá trị các khoản chi đang chờ duyệt. Khoản này không được
        /// cộng vào chi phí thực tế cho đến khi được phê duyệt.
        /// </summary>
        public decimal PendingApprovalCost { get; set; }

        public decimal RejectedCost { get; set; }

        public int PendingApprovalCostItemCount { get; set; }

        public int ApprovedCostItemCount { get; set; }

        public int RejectedCostItemCount { get; set; }

        public int CostItemCount { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal ProfitMargin { get; set; }

        public decimal ReceivedPayment { get; set; }

        public decimal OutstandingPayment { get; set; }

        public decimal OverduePaymentAmount { get; set; }

        public int OverduePaymentCount { get; set; }

        public int PaidPaymentCount { get; set; }

        public int PaidLatePaymentCount { get; set; }

        public int PaidOnTimePaymentCount { get; set; }

        public int DueTodayPaymentCount { get; set; }

        public int OutstandingPaymentCount { get; set; }

        public int UpcomingPaymentCount { get; set; }

        public int PaymentWithoutDueDateCount { get; set; }

        public int PaymentCount { get; set; }

        public decimal PaymentCollectionRate { get; set; }

        public decimal AverageCostPerProject { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<ProjectCostStatistic> ProjectStatistics { get; set; }

        public List<CostTrendStatistic> CostTrendStatistics { get; set; }

        public List<CostItemInfo> LargestCostItems { get; set; }

        public List<CostItemInfo> ApprovedCostItems { get; set; }

        public List<CostItemInfo> PendingApprovalCostItems { get; set; }

        public List<CostItemInfo> RejectedCostItems { get; set; }

        public List<PaymentItemInfo> PaymentItems { get; set; }

        public List<PaymentItemInfo> PendingPaymentItems { get; set; }

        public List<OverduePaymentInfo> OverduePayments { get; set; }
    }

    public class ProjectCostStatistic
    {
        public Guid ProjectId { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime CompletionDate { get; set; }

        public string ContractNumber { get; set; }

        public decimal ContractValue { get; set; }

        public bool HasContractValue { get; set; }

        public decimal ActualCost { get; set; }

        /// <summary>Contract value minus approved actual costs; negative means over budget.</summary>
        public decimal? CostVariance { get; set; }

        public decimal? CostVariancePercent { get; set; }

        public string CostComparisonStatus { get; set; }

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

        public Guid ProjectId { get; set; }

        public string CostCode { get; set; }

        public string CostName { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public DateTime OccurredDate { get; set; }

        public string RequesterName { get; set; }

        public int DaysWaiting { get; set; }

        public byte Status { get; set; }

        public decimal Amount { get; set; }
    }

    public class PaymentItemInfo
    {
        public Guid PaymentId { get; set; }

        public Guid ProjectId { get; set; }

        public string PaymentCode { get; set; }

        public string PaymentName { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ActualPaymentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsPaid { get; set; }

        public bool IsPaidLate { get; set; }

        public bool IsOverdue { get; set; }

        public bool HasDueDate { get; set; }

        public int DaysOverdue { get; set; }

        public int DaysLate { get; set; }

        public int DaysUntilDue { get; set; }

        public string StatusCategory { get; set; }
    }

    public class OverduePaymentInfo
    {
        public Guid ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string PaymentCode { get; set; }
        public string PaymentName { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal Amount { get; set; }
    }
}
