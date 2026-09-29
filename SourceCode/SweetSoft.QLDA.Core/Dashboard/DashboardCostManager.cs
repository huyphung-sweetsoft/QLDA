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
            List<CostItemInfo> approvedCostItems =
                BuildCostItemInfos(approvedCosts, projects)
                    .OrderByDescending(x => x.Amount)
                    .ThenByDescending(x => x.OccurredDate)
                    .ToList();
            List<TblChiPhi> pendingApprovalCosts =
                _repository.GetPendingApprovalCostsForProjects(projectIds);
            List<CostItemInfo> pendingApprovalCostItems =
                BuildCostItemInfos(pendingApprovalCosts, projects)
                    .OrderByDescending(x => x.Amount)
                    .ThenByDescending(x => x.OccurredDate)
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
            decimal receivedPayment = payments
                .Where(x => x.NgayThanhToanThucTe.HasValue)
                .Sum(x => x.SoTien);
            decimal grossProfit = totalContractValue - actualCost;

            return new DashboardCostModel
            {
                GeneratedAt = DateTime.Now,
                ProjectCount = projects.Count,
                TotalContractValue = totalContractValue,
                ActualCost = actualCost,
                PendingApprovalCost = pendingApprovalCost,
                PendingApprovalCostItemCount = pendingApprovalCostItems.Count,
                GrossProfit = grossProfit,
                ProfitMargin = GetPercent(grossProfit, totalContractValue),
                ReceivedPayment = receivedPayment,
                OutstandingPayment = projectStatistics.Sum(
                    x => x.OutstandingPayment),
                OverduePaymentAmount = overduePayments.Sum(x => x.Amount),
                OverduePaymentCount = overduePayments.Count,
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
                PendingApprovalCostItems = pendingApprovalCostItems
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
                decimal actualCost = projectCosts.Sum(x => x.SoTien);
                decimal receivedPayment = projectPayments
                    .Where(x => x.NgayThanhToanThucTe.HasValue)
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
                    ActualCost = actualCost,
                    ReceivedPayment = receivedPayment,
                    OutstandingPayment = contractValue > 0
                        ? Math.Max(0, contractValue - receivedPayment)
                        : projectPayments.Where(x => !x.NgayThanhToanThucTe.HasValue)
                            .Sum(x => x.SoTien),
                    GrossProfit = grossProfit,
                    ProfitMargin = GetPercent(grossProfit, contractValue),
                    CostItemCount = projectCosts.Count
                });
            }

            return result
                .OrderBy(x => x.ProfitMargin)
                .ThenByDescending(x => x.ActualCost)
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
                        Amount = cost.SoTien
                    };
                })
                .ToList();
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
