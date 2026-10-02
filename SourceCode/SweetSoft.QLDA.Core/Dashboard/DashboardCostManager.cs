using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardCostManager : BaseManager
    {
        private static readonly Lazy<DashboardCostManager> LazyInstance =
            new Lazy<DashboardCostManager>(() => new DashboardCostManager());

        private readonly DashboardRepository _repository;

        public DashboardCostManager(
            IAppContext applicationContext = null,
            DashboardRepository repository = null)
            : base(applicationContext)
        {
            _repository = repository ?? new DashboardRepository();
        }

        public static DashboardCostManager Instance => LazyInstance.Value;

        public DashboardCostModel GetCostDashboard(DashboardCostFilter filter)
        {
            // Toàn bộ số liệu thu - chi của dashboard chỉ được tổng hợp từ
            // các dự án có trạng thái Hoàn thành.
            List<TblDuAn> projects = _repository.GetCompletedProjects(filter);
            List<Guid> projectIds = projects.Select(x => x.IdDuAn).ToList();

            // Chi phí thực tế chỉ gồm khoản đã duyệt. Các khoản chờ duyệt
            // được lấy riêng để hiển thị, không làm thay đổi lãi/lỗ hiện tại.
            List<TblChiPhi> approvedCosts =
                _repository.GetApprovedCostsForProjects(projectIds);
            List<TblChiPhi> pendingApprovalCosts =
                _repository.GetPendingApprovalCostsForProjects(projectIds);
            List<TblChiPhi> rejectedCosts =
                _repository.GetRejectedCostsForProjects(projectIds);
            Dictionary<Guid, string> requesterNames = BuildRequesterNames(
                approvedCosts.Concat(pendingApprovalCosts).Concat(rejectedCosts));
            List<CostItemInfo> approvedCostItems =
                BuildCostItemInfos(approvedCosts, projects, requesterNames,
                    (byte)TrangThaiChiPhi.Approved)
                    .OrderByDescending(x => x.Amount)
                    .ThenByDescending(x => x.OccurredDate)
                    .ToList();
            List<CostItemInfo> pendingApprovalCostItems =
                BuildCostItemInfos(pendingApprovalCosts, projects, requesterNames,
                    (byte)TrangThaiChiPhi.NotApproved)
                    .OrderBy(x => x.OccurredDate)
                    .ThenBy(x => x.ProjectCode)
                    .ToList();
            List<CostItemInfo> rejectedCostItems =
                BuildCostItemInfos(rejectedCosts, projects, requesterNames,
                    (byte)TrangThaiChiPhi.Rejected)
                    .OrderByDescending(x => x.OccurredDate)
                    .ToList();
            List<TblThanhToan> payments =
                _repository.GetPaymentsForProjects(projectIds);
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);
            List<OverduePaymentInfo> overduePayments = payments
                .Where(x => x.TrangThai != (byte)ThanhToanStatus.DaThanhToan
                    && !x.NgayThanhToanThucTe.HasValue
                    && x.HanThanhToan.HasValue
                    && x.HanThanhToan.Value.Date < DateTime.Today)
                .Select(x =>
                {
                    TblDuAn project = projectById[x.IdDuAn];
                    return new OverduePaymentInfo
                    {
                        ProjectId = x.IdDuAn,
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        PaymentCode = x.MaDotThanhToan,
                        PaymentName = x.TenDotThanhToan,
                        DueDate = x.HanThanhToan.Value.Date,
                        DaysOverdue = (DateTime.Today - x.HanThanhToan.Value.Date).Days,
                        Amount = x.SoTien
                    };
                })
                .OrderByDescending(x => x.DaysOverdue)
                .ThenBy(x => x.ProjectCode)
                .ToList();
            List<PaymentItemInfo> paymentItems = BuildPaymentItemInfos(
                payments,
                projectById);
            List<PaymentItemInfo> pendingPaymentItems = paymentItems
                .Where(x => !x.IsPaid)
                .OrderByDescending(x => x.IsOverdue)
                .ThenByDescending(x => x.DaysOverdue)
                .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
                .ThenBy(x => x.ProjectCode)
                .ToList();
            List<TblHopDongThucHien> contracts =
                _repository.GetContractsForProjects(projectIds);

            Dictionary<Guid, TblHopDongThucHien> contractById = contracts
                .ToDictionary(x => x.IdHopDongThucHien);

            List<ProjectCostStatistic> projectStatistics =
                BuildProjectStatistics(
                    projects,
                    approvedCosts,
                    payments,
                    contractById);

            decimal totalContractValue = contracts.Sum(x =>
                x.GiaTriHopDong ?? 0);
            decimal actualCost = approvedCosts.Sum(x => x.SoTien);
            decimal pendingApprovalCost = pendingApprovalCosts.Sum(
                x => x.SoTien);
            decimal receivedPayment = paymentItems
                .Where(x => x.IsPaid)
                .Sum(x => x.Amount);
            decimal comparableActualCost = projectStatistics
                .Where(x => x.HasContractValue)
                .Sum(x => x.ActualCost);
            decimal grossProfit = totalContractValue - comparableActualCost;

            return new DashboardCostModel
            {
                GeneratedAt = DateTime.Now,
                ProjectCount = projects.Count,
                TotalContractValue = totalContractValue,
                ActualCost = actualCost,
                PendingApprovalCost = pendingApprovalCost,
                RejectedCost = rejectedCosts.Sum(x => x.SoTien),
                PendingApprovalCostItemCount = pendingApprovalCostItems.Count,
                ApprovedCostItemCount = approvedCostItems.Count,
                RejectedCostItemCount = rejectedCostItems.Count,
                CostItemCount = approvedCostItems.Count
                    + pendingApprovalCostItems.Count
                    + rejectedCostItems.Count,
                GrossProfit = grossProfit,
                ProfitMargin = GetPercent(grossProfit, totalContractValue),
                ReceivedPayment = receivedPayment,
                OutstandingPayment = projectStatistics.Sum(
                    x => x.OutstandingPayment),
                OverduePaymentAmount = paymentItems
                    .Where(x => x.IsOverdue)
                    .Sum(x => x.Amount),
                OverduePaymentCount = paymentItems.Count(x => x.IsOverdue),
                PaidPaymentCount = paymentItems.Count(x => x.IsPaid),
                PaidLatePaymentCount = paymentItems.Count(x => x.IsPaidLate),
                PaidOnTimePaymentCount = paymentItems.Count(x =>
                    x.IsPaid && !x.IsPaidLate),
                OutstandingPaymentCount = paymentItems.Count(x => !x.IsPaid),
                DueTodayPaymentCount = paymentItems.Count(x =>
                    !x.IsPaid && !x.IsOverdue && x.HasDueDate
                    && x.DueDate.Value.Date == DateTime.Today),
                UpcomingPaymentCount = paymentItems.Count(x =>
                    !x.IsPaid && x.HasDueDate && !x.IsOverdue
                    && x.DueDate.Value.Date > DateTime.Today),
                PaymentWithoutDueDateCount = paymentItems.Count(x =>
                    !x.IsPaid && !x.IsOverdue && !x.HasDueDate),
                PaymentCount = paymentItems.Count,
                OverduePayments = overduePayments,
                PaymentCollectionRate = GetPercent(
                    receivedPayment,
                    totalContractValue),
                AverageCostPerProject = projects.Count == 0
                    ? 0
                    : Math.Round(actualCost / projects.Count, 2),
                ProjectStatistics = projectStatistics,
                CostTrendStatistics = BuildCostTrend(approvedCosts),
                LargestCostItems = approvedCostItems.Take(15).ToList(),
                ApprovedCostItems = approvedCostItems,
                PendingApprovalCostItems = pendingApprovalCostItems,
                RejectedCostItems = rejectedCostItems,
                PaymentItems = paymentItems,
                PendingPaymentItems = pendingPaymentItems
            };
        }

        public List<TblDuAn> GetProjectsForFilter()
        {
            return _repository.GetCompletedProjects(null);
        }

        private static List<ProjectCostStatistic> BuildProjectStatistics(
            List<TblDuAn> projects,
            List<TblChiPhi> costs,
            List<TblThanhToan> payments,
            Dictionary<Guid, TblHopDongThucHien> contractById)
        {
            List<ProjectCostStatistic> result =
                new List<ProjectCostStatistic>();

            foreach (TblDuAn project in projects)
            {
                TblHopDongThucHien contract = null;
                if (project.IdHopDongThucHien.HasValue)
                {
                    contractById.TryGetValue(
                        project.IdHopDongThucHien.Value,
                        out contract);
                }

                List<TblChiPhi> projectCosts = costs
                    .Where(x => x.IdDuAn == project.IdDuAn)
                    .ToList();
                List<TblThanhToan> projectPayments = payments
                    .Where(x => x.IdDuAn == project.IdDuAn)
                    .ToList();

                decimal contractValue = contract == null
                    ? 0
                    : contract.GiaTriHopDong ?? 0;
                bool hasContractValue = contract != null
                    && contract.GiaTriHopDong.HasValue;
                decimal actualCost = projectCosts.Sum(x => x.SoTien);
                decimal receivedPayment = projectPayments
                    .Where(IsPaymentPaid)
                    .Sum(x => x.SoTien);
                decimal grossProfit = contractValue - actualCost;

                result.Add(new ProjectCostStatistic
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    CompletionDate = GetEffectiveCompletionDate(project),
                    ContractNumber = contract == null
                        ? string.Empty
                        : contract.SoHopDong,
                    ContractValue = contractValue,
                    HasContractValue = hasContractValue,
                    ActualCost = actualCost,
                    CostVariance = hasContractValue
                        ? contractValue - actualCost
                        : (decimal?)null,
                    CostVariancePercent = hasContractValue && contractValue != 0
                        ? Math.Round(
                            ((contractValue - actualCost) / contractValue) * 100,
                            2)
                        : (decimal?)null,
                    CostComparisonStatus = !hasContractValue
                        ? "no-contract"
                        : actualCost > contractValue
                            ? "over"
                            : actualCost < contractValue
                                ? "under"
                                : "equal",
                    ReceivedPayment = receivedPayment,
                    OutstandingPayment = contractValue > 0
                        ? Math.Max(0, contractValue - receivedPayment)
                        : projectPayments.Where(x => !x.NgayThanhToanThucTe.HasValue)
                            .Sum(x => x.SoTien),
                    GrossProfit = hasContractValue ? grossProfit : 0,
                    ProfitMargin = hasContractValue && contractValue != 0
                        ? GetPercent(grossProfit, contractValue)
                        : 0,
                    CostItemCount = projectCosts.Count
                });
            }

            return result
                .OrderBy(x => x.CostComparisonStatus == "over" ? 0
                    : x.CostComparisonStatus == "under" ? 1
                    : x.CostComparisonStatus == "equal" ? 2 : 3)
                .ThenByDescending(x => Math.Abs(x.CostVariance ?? 0))
                .ThenBy(x => x.ProjectCode)
                .ToList();
        }

        private static List<CostTrendStatistic> BuildCostTrend(
            List<TblChiPhi> costs)
        {
            return costs
                .GroupBy(x => new
                {
                    x.NgayTao.Year,
                    x.NgayTao.Month
                })
                .Select(group => new CostTrendStatistic
                {
                    Month = new DateTime(
                        group.Key.Year,
                        group.Key.Month,
                        1),
                    Amount = group.Sum(x => x.SoTien)
                })
                .OrderBy(x => x.Month)
                .ToList();
        }

        private static List<CostItemInfo> BuildLargestCostItems(
            List<TblChiPhi> costs,
            List<TblDuAn> projects)
        {
            return BuildCostItemInfos(costs, projects)
                .OrderByDescending(x => x.Amount)
                .ThenByDescending(x => x.OccurredDate)
                .Take(15)
                .ToList();
        }

        private static List<CostItemInfo> BuildCostItemInfos(
            List<TblChiPhi> costs,
            List<TblDuAn> projects)
        {
            return BuildCostItemInfos(
                costs,
                projects,
                BuildRequesterNames(costs),
                (byte)TrangThaiChiPhi.Approved);
        }

        private static List<CostItemInfo> BuildCostItemInfos(
            List<TblChiPhi> costs,
            List<TblDuAn> projects,
            Dictionary<Guid, string> requesterNames,
            byte status)
        {
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);

            return costs
                .Select(cost =>
                {
                    TblDuAn project;
                    projectById.TryGetValue(cost.IdDuAn, out project);

                    return new CostItemInfo
                    {
                        CostId = cost.IdChiPhi,
                        ProjectId = cost.IdDuAn,
                        CostCode = cost.MaChiPhi,
                        CostName = cost.TenKhoanChi,
                        ProjectCode = project == null
                            ? string.Empty
                            : project.MaDuAn,
                        ProjectName = project == null
                            ? string.Empty
                            : project.TenDuAn,
                        OccurredDate = cost.NgayTao,
                        Amount = cost.SoTien,
                        RequesterName = cost.IdNhanVienDeNghi.HasValue
                            && requesterNames.ContainsKey(
                                cost.IdNhanVienDeNghi.Value)
                            ? requesterNames[cost.IdNhanVienDeNghi.Value]
                            : string.Empty,
                        DaysWaiting = Math.Max(0,
                            (DateTime.Today - cost.NgayTao.Date).Days),
                        Status = status
                    };
                })
                .ToList();
        }

        private static Dictionary<Guid, string> BuildRequesterNames(
            IEnumerable<TblChiPhi> costs)
        {
            Dictionary<Guid, string> names = new Dictionary<Guid, string>();
            foreach (Guid requesterId in costs
                .Where(x => x.IdNhanVienDeNghi.HasValue)
                .Select(x => x.IdNhanVienDeNghi.Value)
                .Distinct())
            {
                AspnetUser user = AspnetUser.FetchByID(requesterId);
                if (user != null)
                {
                    string name = string.IsNullOrWhiteSpace(user.DisplayName)
                        ? user.UserName
                        : user.DisplayName;
                    names[requesterId] = name ?? string.Empty;
                }
            }

            return names;
        }

        private static List<PaymentItemInfo> BuildPaymentItemInfos(
            List<TblThanhToan> payments,
            Dictionary<Guid, TblDuAn> projectById)
        {
            DateTime today = DateTime.Today;
            return payments.Select(payment =>
            {
                TblDuAn project = projectById[payment.IdDuAn];
                bool isPaid = IsPaymentPaid(payment);
                bool isOverdue = !isPaid && (
                    payment.TrangThai == (byte)ThanhToanStatus.TreHan
                    || (payment.HanThanhToan.HasValue
                        && payment.HanThanhToan.Value.Date < today));
                bool hasDueDate = payment.HanThanhToan.HasValue;
                DateTime? actualPaymentDate = payment.NgayThanhToanThucTe;
                bool isPaidLate = isPaid && actualPaymentDate.HasValue
                    && hasDueDate
                    && actualPaymentDate.Value.Date
                        > payment.HanThanhToan.Value.Date;

                return new PaymentItemInfo
                {
                    PaymentId = payment.IdThanhToan,
                    ProjectId = payment.IdDuAn,
                    PaymentCode = payment.MaDotThanhToan,
                    PaymentName = payment.TenDotThanhToan,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    Amount = payment.SoTien,
                    DueDate = payment.HanThanhToan,
                    ActualPaymentDate = actualPaymentDate,
                    CreatedAt = payment.NgayTao,
                    IsPaid = isPaid,
                    IsPaidLate = isPaidLate,
                    IsOverdue = isOverdue,
                    HasDueDate = hasDueDate,
                    DaysOverdue = isOverdue && hasDueDate
                        ? Math.Max(0, (today - payment.HanThanhToan.Value.Date).Days)
                        : 0,
                    DaysLate = isPaidLate
                        ? Math.Max(0, (actualPaymentDate.Value.Date
                            - payment.HanThanhToan.Value.Date).Days)
                        : 0,
                    DaysUntilDue = !isPaid && !isOverdue && hasDueDate
                        ? Math.Max(0, (payment.HanThanhToan.Value.Date - today).Days)
                        : 0,
                    StatusCategory = isPaid
                        ? isPaidLate ? "paid-late" : "paid-on-time"
                        : isOverdue
                            ? "overdue"
                            : hasDueDate
                                ? payment.HanThanhToan.Value.Date == today
                                    ? "due-today"
                                    : "upcoming"
                                : "no-date"
                };
            })
            .OrderByDescending(x => x.IsOverdue)
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .ThenBy(x => x.ProjectCode)
            .ToList();
        }

        private static bool IsPaymentPaid(TblThanhToan payment)
        {
            return payment.TrangThai == (byte)ThanhToanStatus.DaThanhToan
                || payment.NgayThanhToanThucTe.HasValue;
        }

        /// <summary>
        /// Dự án Hoàn thành có thể chưa được nhập ngày hoàn thành thực tế.
        /// Khi đó dùng lần cập nhật cuối, rồi đến ngày dự kiến để việc lọc và
        /// hiển thị Dashboard không loại bỏ dữ liệu hợp lệ.
        /// </summary>
        private static DateTime GetEffectiveCompletionDate(TblDuAn project)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return project.NgayHoanThanhThucTe.Value;
            }

            if (project.NgayCapNhat.HasValue)
            {
                return project.NgayCapNhat.Value;
            }

            if (project.NgayDuKienHoanThanh != DateTime.MinValue)
            {
                return project.NgayDuKienHoanThanh;
            }

            return project.NgayTao;
        }

        private static decimal GetPercent(decimal value, decimal total)
        {
            if (total == 0)
            {
                return 0;
            }

            return Math.Round((value / total) * 100, 2);
        }
    }
}
