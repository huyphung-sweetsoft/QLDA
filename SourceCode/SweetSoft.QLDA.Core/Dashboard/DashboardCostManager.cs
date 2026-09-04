using System;
using System.Collections.Generic;
using System.Linq;
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
            List<TblDuAn> projects = _repository.GetCompletedProjects(filter);
            List<Guid> projectIds = projects.Select(x => x.IdDuAn).ToList();

            List<TblChiPhi> costs = _repository.GetCostsForProjects(projectIds);
            List<TblThanhToan> payments =
                _repository.GetPaymentsForProjects(projectIds);
            List<TblHopDongThucHien> contracts =
                _repository.GetContractsForProjects(projectIds);

            Dictionary<Guid, TblHopDongThucHien> contractById = contracts
                .ToDictionary(x => x.IdHopDongThucHien);

            List<ProjectCostStatistic> projectStatistics =
                BuildProjectStatistics(
                    projects,
                    costs,
                    payments,
                    contractById);

            decimal totalContractValue = contracts.Sum(x =>
                x.GiaTriHopDong ?? 0);
            decimal actualCost = costs.Sum(x => x.SoTien);
            decimal receivedPayment = payments
                .Where(x => x.NgayThanhToanThucTe.HasValue)
                .Sum(x => x.SoTien);
            decimal grossProfit = totalContractValue - actualCost;

            return new DashboardCostModel
            {
                GeneratedAt = DateTime.Now,
                CompletedProjectCount = projects.Count,
                TotalContractValue = totalContractValue,
                ActualCost = actualCost,
                GrossProfit = grossProfit,
                ProfitMargin = GetPercent(grossProfit, totalContractValue),
                ReceivedPayment = receivedPayment,
                OutstandingPayment = Math.Max(
                    0,
                    totalContractValue - receivedPayment),
                PaymentCollectionRate = GetPercent(
                    receivedPayment,
                    totalContractValue),
                AverageCostPerProject = projects.Count == 0
                    ? 0
                    : Math.Round(actualCost / projects.Count, 2),
                ProjectStatistics = projectStatistics,
                CostTrendStatistics = BuildCostTrend(costs),
                LargestCostItems = BuildLargestCostItems(costs, projects)
            };
        }

        public List<TblDuAn> GetCompletedProjectsForFilter()
        {
            return _repository.GetCompletedProjects(
                new DashboardCostFilter
                {
                    Period = DashboardCostPeriod.AllTime
                });
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
                    CompletionDate = project.NgayHoanThanhThucTe.Value,
                    ContractNumber = contract == null
                        ? string.Empty
                        : contract.SoHopDong,
                    ContractValue = contractValue,
                    ActualCost = actualCost,
                    ReceivedPayment = receivedPayment,
                    OutstandingPayment = Math.Max(
                        0,
                        contractValue - receivedPayment),
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
                    x.NgayPhatSinh.Year,
                    x.NgayPhatSinh.Month
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
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);

            return costs
                .OrderByDescending(x => x.SoTien)
                .ThenByDescending(x => x.NgayPhatSinh)
                .Take(15)
                .Select(cost =>
                {
                    TblDuAn project;
                    projectById.TryGetValue(cost.IdDuAn, out project);

                    return new CostItemInfo
                    {
                        CostId = cost.IdChiPhi,
                        CostCode = cost.MaKhoanChi,
                        CostName = cost.TenKhoanChi,
                        ProjectCode = project == null
                            ? string.Empty
                            : project.MaDuAn,
                        ProjectName = project == null
                            ? string.Empty
                            : project.TenDuAn,
                        OccurredDate = cost.NgayPhatSinh,
                        Amount = cost.SoTien
                    };
                })
                .ToList();
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
