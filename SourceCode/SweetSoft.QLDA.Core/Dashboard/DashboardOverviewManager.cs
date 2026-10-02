using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardOverviewManager : BaseManager
    {
        private static readonly Lazy<DashboardOverviewManager> LazyInstance =
            new Lazy<DashboardOverviewManager>(() => new DashboardOverviewManager());

        private readonly DashboardRepository _repository;

        public DashboardOverviewManager(
            IAppContext applicationContext = null,
            DashboardRepository repository = null)
            : base(applicationContext)
        {
            _repository = repository ?? new DashboardRepository();
        }

        public static DashboardOverviewManager Instance => LazyInstance.Value;

        public DashboardOverviewSummary GetSimpleOverview(
            DashboardFilter filter, bool includeFinance = false,
            bool includeIssues = false, bool includeRisks = false,
            bool includeCustomers = false)
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;
            // Trang tổng quan phản ánh tình hình hiện tại, không loại dự án
            // chỉ vì ngày bắt đầu/kết thúc nằm ngoài bộ lọc thời gian.
            List<TblDuAn> projects = _repository.GetProjects(filter, false);
            HashSet<Guid> projectIds = new HashSet<Guid>(projects.Select(p => p.IdDuAn));
            Dictionary<Guid, TblDuAn> projectById = projects.ToDictionary(p => p.IdDuAn);
            HashSet<Guid> openProjectIds = new HashSet<Guid>(projects
                .Where(p => p.TrangThai != (byte)DuAnStatus.HoanThanh
                    && p.TrangThai != (byte)DuAnStatus.KetThuc)
                .Select(p => p.IdDuAn));
            DashboardFilter currentIssuesFilter = new DashboardFilter
            {
                ProjectId = filter == null ? null : filter.ProjectId
            };
            DashboardFilter upcomingMeetingsFilter = new DashboardFilter
            {
                ProjectId = filter == null ? null : filter.ProjectId
            };
            List<OverviewImportantIssue> openIssues = includeIssues
                ? _repository.GetIssues(filter)
                    .Where(i => projectIds.Contains(i.IdDuAn)
                        && i.TrangThai == (byte)TrangThaiVanDeEnum.Processing)
                    .Select(i => new OverviewImportantIssue
                    {
                        ProjectId = i.IdDuAn,
                        ProjectCode = projectById[i.IdDuAn].MaDuAn,
                        ProjectName = projectById[i.IdDuAn].TenDuAn,
                        IssueId = i.IdVanDe,
                        IssueCode = i.MaVanDe,
                        IssueName = i.TenVanDe,
                        Description = i.MoTaChiTiet,
                        HandlingPlan = i.KeHoachXuLy,
                        ImpactLevel = i.MucDoAnhHuong ?? 0
                    })
                    .OrderByDescending(i => i.ImpactLevel)
                    .ThenBy(i => i.IssueCode)
                    .ToList()
                : new List<OverviewImportantIssue>();
            List<OverviewImportantIssue> currentOpenIssues = includeIssues
                ? _repository.GetIssues(currentIssuesFilter)
                    .Where(i => projectIds.Contains(i.IdDuAn)
                        && i.TrangThai == (byte)TrangThaiVanDeEnum.Processing)
                    .Select(i => new OverviewImportantIssue
                    {
                        ProjectId = i.IdDuAn,
                        ProjectCode = projectById[i.IdDuAn].MaDuAn,
                        ProjectName = projectById[i.IdDuAn].TenDuAn,
                        IssueId = i.IdVanDe,
                        IssueCode = i.MaVanDe,
                        IssueName = i.TenVanDe,
                        Description = i.MoTaChiTiet,
                        HandlingPlan = i.KeHoachXuLy,
                        ImpactLevel = i.MucDoAnhHuong ?? 0
                    })
                    .ToList()
                : new List<OverviewImportantIssue>();
            List<OverviewRecordedRisk> recordedRisks = includeRisks
                ? _repository.GetRisks(filter)
                    .Where(r => projectIds.Contains(r.IdDuAn))
                    .Select(r => new OverviewRecordedRisk
                    {
                        ProjectId = r.IdDuAn,
                        ProjectCode = projectById[r.IdDuAn].MaDuAn,
                        ProjectName = projectById[r.IdDuAn].TenDuAn,
                        RiskId = r.IdRuiRoDuAn,
                        RiskName = r.TenRuiRo,
                        Probability = r.XacSuatXayRa,
                        ImpactLevel = r.MucDoAnhHuong,
                        RiskScore = r.DiemRuiRo,
                        PreventionPlan = r.KeHoachPhongNgua,
                        ResponsePlan = r.KeHoachUngPho
                    })
                    .OrderByDescending(r => r.RiskScore ?? 0)
                    .ThenBy(r => r.ProjectCode)
                    .ThenBy(r => r.RiskName)
                    .ToList()
                : new List<OverviewRecordedRisk>();
            List<OverviewImportantIssue> importantIssues = currentOpenIssues
                .Where(i => openProjectIds.Contains(i.ProjectId)
                    && i.ImpactLevel >= (int)MucDoAnhHuonEnum.High
                    && i.ImpactLevel <= (int)MucDoAnhHuonEnum.VeryHigh)
                .ToList();
            Dictionary<Guid, int> issueCountByProject = importantIssues
                .GroupBy(i => i.ProjectId)
                .ToDictionary(g => g.Key, g => g.Count());
            List<TblChiPhi> pendingCostRecords = includeFinance
                ? _repository.GetPendingApprovalCostsForProjects(projectIds)
                : new List<TblChiPhi>();
            List<OverviewPendingCostItem> pendingCosts = pendingCostRecords
                    .Select(cost =>
                    {
                        TblDuAn project = projectById[cost.IdDuAn];
                        return new OverviewPendingCostItem
                        {
                            ProjectId = cost.IdDuAn,
                            ProjectCode = project.MaDuAn,
                            ProjectName = project.TenDuAn,
                            CostCode = cost.MaChiPhi,
                            CostName = cost.TenKhoanChi,
                            Amount = cost.SoTien
                        };
                    })
                    .OrderByDescending(cost => cost.Amount)
                    .ToList();
            List<TblCongViec> allTasks = DashboardProgressCalculator
                .ExcludeStageRootTasksWithChildren(_repository.GetTasks(filter, false)
                    .Where(t => projectIds.Contains(t.IdDuAn)).ToList());
            List<TblCongViec> periodTasks = filter != null
                && filter.FromDate > DateTime.MinValue
                && filter.ToDate > DateTime.MinValue
                ? DashboardProgressCalculator.ExcludeStageRootTasksWithChildren(
                    _repository.GetTasks(filter, true)
                        .Where(t => projectIds.Contains(t.IdDuAn)).ToList())
                : allTasks;
            var tasksByProject = allTasks.GroupBy(t => t.IdDuAn)
                .ToDictionary(g => g.Key, g => g.ToList());
            List<OverviewProjectItem> items = projects.Select(project =>
            {
                List<TblCongViec> projectTasks;
                if (!tasksByProject.TryGetValue(project.IdDuAn, out projectTasks))
                    projectTasks = new List<TblCongViec>();
                bool isOpen = project.TrangThai != (byte)DuAnStatus.HoanThanh
                    && project.TrangThai != (byte)DuAnStatus.KetThuc;
                int overdueDays = project.TrangThai == (byte)DuAnStatus.DangThucHien
                    && project.NgayDuKienHoanThanh.Date < today
                    ? (today - project.NgayDuKienHoanThanh.Date).Days : 0;
                return new OverviewProjectItem
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    StatusCode = project.TrangThai,
                    Status = GetProjectStatusText((DuAnStatus)project.TrangThai),
                    ExpectedEndDate = project.NgayDuKienHoanThanh,
                    IsOverdue = overdueDays > 0,
                    OverdueDays = overdueDays,
                    TaskCount = projectTasks.Count,
                    CompletedTaskCount = projectTasks.Count(
                        DashboardProgressCalculator.IsTaskCompleted),
                    OverdueTaskCount = isOpen ? projectTasks.Count(t =>
                        DashboardProgressCalculator.IsTaskOverdue(t, today)) : 0,
                    DueSoonTaskCount = isOpen ? projectTasks.Count(t =>
                        DashboardProgressCalculator.IsTaskDueSoon(t, today)) : 0,
                    ImportantIssueCount = issueCountByProject.ContainsKey(project.IdDuAn)
                        ? issueCountByProject[project.IdDuAn] : 0
                };
            }).OrderBy(p => p.ProjectCode).ToList();

            List<OverviewActiveCustomer> activeCustomers = includeCustomers
                ? _repository.GetActiveCustomers()
                : new List<OverviewActiveCustomer>();

            return new DashboardOverviewSummary
            {
                GeneratedAt = now,
                ActiveCustomerCount = activeCustomers.Count,
                ActiveCustomers = activeCustomers,
                Projects = items,
                Tasks = GetTaskSummaries(periodTasks, projects, today),
                CurrentTasks = GetTaskSummaries(allTasks, projects, today),
                FinanceSummary = includeFinance
                    ? GetOverviewFinanceSummary(projects, pendingCostRecords)
                    : null,
                Meetings = GetUpcomingMeetingSummaries(
                    _repository.GetMeetings(upcomingMeetingsFilter)
                        .Where(m => projectIds.Contains(m.IdDuAn)).ToList(),
                    projects, now),
                Statuses = GetProjectStatusStatistics(projects),
                PendingCosts = pendingCosts,
                OpenIssues = openIssues,
                ImportantIssues = importantIssues,
                RecordedRisks = recordedRisks
            };
        }

        public DashboardOverviewModel GetOverview(DashboardFilter filter)
        {
            DateTime generatedAt = DateTime.Now;
            DateTime today = generatedAt.Date;

            List<TblDuAn> projects = _repository.GetProjects(filter);
            HashSet<Guid> projectIds = new HashSet<Guid>(
                projects.Select(x => x.IdDuAn));
            List<TblCongViec> allTasks = _repository
                .GetTasks(filter, false)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();
            List<TblCongViec> progressTasks = DashboardProgressCalculator
                .ExcludeStageRootTasksWithChildren(allTasks);
            DashboardWorkingCalendar workingCalendar =
                DashboardWorkingCalendarFactory.CreateForActiveProjects(
                    _repository,
                    projects,
                    today);
            List<TblRuiRoDuAn> risks = _repository.GetRisks(filter);
            List<TblVanDe> issues = _repository.GetIssues(filter);
            DashboardFilter upcomingMeetingsFilter = new DashboardFilter
            {
                ProjectId = filter == null ? null : filter.ProjectId
            };
            List<TblLichHop> meetings = _repository
                .GetMeetings(upcomingMeetingsFilter)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();

            DashboardFinancialSummary financialSummary =
                _repository.GetFinancialSummary(filter);
            List<TblChiPhi> overviewCosts =
                _repository.GetOverviewCosts(filter);

            List<AspnetUser> employees = _repository.GetEmployees();
            List<TblThanhVienDuAn> projectMembers =
                _repository.GetProjectMembers(null);

            int totalProjectCount = projects.Count;
            int activeProjectCount = projects.Count(p =>
                DashboardProgressCalculator.GetProjectState(p, today)
                    == DashboardProjectState.InProgress);

            int upcomingMeetingCount = GetUpcomingMeetingCount(
                meetings,
                generatedAt);
            List<UpcomingMeetingSummary> upcomingMeetings =
                GetUpcomingMeetingSummaries(meetings, projects, generatedAt);
            int overdueTaskCount = progressTasks.Count(x =>
                DashboardProgressCalculator.IsTaskOverdue(x, today));

            decimal totalContractValue =
                financialSummary.TotalContractValue;
            CostOverviewModel costOverview = GetCostOverview(
                overviewCosts,
                projects);

            var projectProgressStats = GetProjectProgressStatistics(
                projects,
                progressTasks,
                today,
                workingCalendar);
            List<DashboardTaskSummary> taskDetails = GetTaskSummaries(
                progressTasks,
                projects,
                today);
            List<ProjectEmployeeSummary> projectEmployees =
                GetProjectEmployeeSummaries(
                    projects,
                    projectMembers,
                    employees);
            ResourceOverviewModel resourceOverview = GetResourceOverview(
                employees,
                projects,
                projectMembers);
            DateTime anchorWeekStart = today.AddDays(
                -((7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7));
            DashboardResourceModel weeklyResourceOverview =
                DashboardResourceManager.Instance.GetResourceDashboard(
                    new DashboardResourceFilter
                    {
                        ProjectId = filter == null ? null : filter.ProjectId,
                        AnchorWeekStart = anchorWeekStart,
                        WeekCount = 4
                    });
            ApplyEmployeeLoadBreakdown(
                resourceOverview,
                weeklyResourceOverview);
            int atRiskProjectCount = projectProgressStats.Count(project =>
                project.Health == ProjectScheduleHealth.AtRisk
                || project.Health == ProjectScheduleHealth.BehindSchedule);
            
            decimal overallProgress = 0;
            if (projectProgressStats.Count > 0)
            {
                overallProgress = Convert.ToDecimal(
                    projectProgressStats.Average(p => Convert.ToDecimal(p.Progress))
                );
            }

            return new DashboardOverviewModel
            {
                TotalProjectCount = totalProjectCount,
                OverallProgress = Math.Round(overallProgress, 2),
                TotalTaskCount = progressTasks.Count,
                OverdueTaskCount = overdueTaskCount,
                TotalContractValue = totalContractValue,
                GeneratedAt = generatedAt,

                ProjectStatusStatistics =
                    GetProjectStatusStatistics(projects),
                ProjectProgressStatistics = projectProgressStats,
                ProjectAttentionStatistics =
    GetProjectAttentionStatistics(
        projects,
        risks,
        issues),
                ResourceOverview = resourceOverview,

                CostOverview = costOverview,

                ActiveProjectCount = activeProjectCount,
                UpcomingMeetingCount = upcomingMeetingCount,
                UpcomingMeetings = upcomingMeetings,
                TaskDetails = taskDetails,
                ProjectEmployees = projectEmployees,

                AtRiskProjectCount = atRiskProjectCount,
            };
        }

        private static List<ProjectStatusStatistic> GetProjectStatusStatistics(
            List<TblDuAn> projects)
        {
            var projectStatuses = new List<DuAnStatus>
            {
                DuAnStatus.ChuaBatDau,
                DuAnStatus.DangThucHien,
                DuAnStatus.TamDung,
                DuAnStatus.HoanThanh,
                DuAnStatus.KetThuc
            };

            // Thống kê theo trạng thái lưu trên dự án; quá hạn là tình trạng tiến độ riêng.
            return projectStatuses
                .Select(status => new ProjectStatusStatistic
                {
                    StatusCode = (byte)status,
                    Status = GetProjectStatusText(status),
                    Count = projects.Count(project =>
                        project.TrangThai == (byte)status)
                })
                .ToList();
        }

        private static string GetProjectStatusText(DuAnStatus status)
        {
            switch (status)
            {
                case DuAnStatus.ChuaBatDau:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_STATUS_WAITING);
                case DuAnStatus.DangThucHien:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_STATUS_IN_PROGRESS);
                case DuAnStatus.TamDung:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_STATUS_PAUSED);
                case DuAnStatus.HoanThanh:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_STATUS_COMPLETED);
                case DuAnStatus.KetThuc:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_STATUS_ENDED);
                default:
                    return string.Empty;
            }
        }


        private static List<ProjectProgressStatistic> GetProjectProgressStatistics(
    List<TblDuAn> projects,
    List<TblCongViec> tasks,
    DateTime today,
    DashboardWorkingCalendar workingCalendar)
        {
            var result = new List<ProjectProgressStatistic>();

            foreach (var project in projects)
            {
                var projectTasks = tasks
                    .Where(t => t.IdDuAn == project.IdDuAn)
                    .ToList();
                decimal progress =
                    DashboardProgressCalculator.GetProjectActualProgress(
                        project,
                        projectTasks,
                        today);
                decimal plannedProgress =
                    DashboardProgressCalculator.GetPlannedProgress(
                        project,
                        today,
                        workingCalendar);
                decimal variance = Math.Round(
                    progress - plannedProgress,
                    2);
                int overdueTaskCount = projectTasks.Count(x =>
                    DashboardProgressCalculator.IsTaskOverdue(x, today));
                int dueSoonTaskCount = projectTasks.Count(x =>
                    DashboardProgressCalculator.IsTaskDueSoon(x, today));
                int completedTaskCount = projectTasks.Count(
                    DashboardProgressCalculator.IsTaskCompleted);

                result.Add(new ProjectProgressStatistic
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    Progress = Math.Round(progress, 2),
                    PlannedProgress = plannedProgress,
                    Variance = variance,
                    OverdueTaskCount = overdueTaskCount,
                    DueSoonTaskCount = dueSoonTaskCount,
                    CompletedTaskCount = completedTaskCount,
                    TaskCount = projectTasks.Count,
                    Health = DashboardProgressCalculator.GetProjectHealth(
                        project,
                        variance,
                        overdueTaskCount,
                        today),
                    StartDate = project.NgayBatDau,
                    ExpectedEndDate = project.NgayDuKienHoanThanh,
                    ActualCompletionDate = project.NgayHoanThanhThucTe
                });
            }

            return result
    .OrderBy(x => x.Progress)
    .ThenBy(x => x.ProjectCode)
    .ToList();
        }

        private static List<DashboardTaskSummary> GetTaskSummaries(
            List<TblCongViec> tasks,
            List<TblDuAn> projects,
            DateTime today)
        {
            var projectsById = projects.ToDictionary(
                project => project.IdDuAn);

            return tasks
                .Where(task => projectsById.ContainsKey(task.IdDuAn))
                .Select(task =>
                {
                    TblDuAn project = projectsById[task.IdDuAn];

                    return new DashboardTaskSummary
                    {
                        TaskId = task.IdCongViec,
                        ProjectId = project.IdDuAn,
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        TaskCode = task.MaCongViec,
                        TaskName = task.TenCongViec,
                        Status = GetTaskStateText(
                            DashboardProgressCalculator.GetTaskState(
                                task,
                                today)),
                        StatusCode = (int)DashboardProgressCalculator
                            .GetTaskState(task, today),
                        LifecycleStatusCode = (int)DashboardProgressCalculator
                            .GetTaskLifecycleState(task),
                        Deadline = task.NgayKetThuc,
                        Progress =
                            DashboardProgressCalculator.GetTaskActualProgress(
                                task),
                        IsCompleted = DashboardProgressCalculator.IsTaskCompleted(task),
                        IsOverdue = DashboardProgressCalculator.IsTaskOverdue(
                            task,
                            today)
                    };
                })
                .OrderBy(task => task.Deadline ?? DateTime.MaxValue)
                .ThenBy(task => task.ProjectCode)
                .ThenBy(task => task.TaskCode)
                .ToList();
        }

        private OverviewFinanceSummary GetOverviewFinanceSummary(
            List<TblDuAn> projects,
            List<TblChiPhi> pendingCostRecords)
        {
            // Giữ cùng phạm vi với Dashboard Chi phí: chỉ dự án Hoàn thành.
            List<TblDuAn> completedProjects = projects
                .Where(project => project.TrangThai == (byte)DuAnStatus.HoanThanh)
                .ToList();
            List<Guid> projectIds = completedProjects
                .Select(project => project.IdDuAn).ToList();
            HashSet<Guid> completedProjectIds = new HashSet<Guid>(projectIds);
            List<TblChiPhi> approvedCosts =
                _repository.GetApprovedCostsForProjects(projectIds);
            List<TblThanhToan> payments =
                _repository.GetPaymentsForProjects(projectIds);
            Dictionary<Guid, TblHopDongThucHien> contractsById = _repository
                .GetContractsForProjects(projectIds)
                .GroupBy(contract => contract.IdHopDongThucHien)
                .ToDictionary(group => group.Key, group => group.First());
            Dictionary<Guid, List<TblThanhToan>> paymentsByProject = payments
                .GroupBy(payment => payment.IdDuAn)
                .ToDictionary(group => group.Key, group => group.ToList());
            Dictionary<Guid, decimal> approvedCostByProject = approvedCosts
                .GroupBy(cost => cost.IdDuAn)
                .ToDictionary(group => group.Key, group => group.Sum(cost => cost.SoTien));
            Dictionary<Guid, decimal> pendingCostByProject = pendingCostRecords
                .Where(cost => completedProjectIds.Contains(cost.IdDuAn))
                .GroupBy(cost => cost.IdDuAn)
                .ToDictionary(group => group.Key, group => group.Sum(cost => cost.SoTien));

            OverviewFinanceSummary summary = new OverviewFinanceSummary();
            foreach (TblDuAn project in completedProjects)
            {
                TblHopDongThucHien contract = null;
                if (project.IdHopDongThucHien.HasValue)
                {
                    contractsById.TryGetValue(
                        project.IdHopDongThucHien.Value,
                        out contract);
                }
                decimal contractValue = contract == null
                    ? 0m : contract.GiaTriHopDong ?? 0m;
                List<TblThanhToan> projectPayments;
                if (!paymentsByProject.TryGetValue(project.IdDuAn, out projectPayments))
                    projectPayments = new List<TblThanhToan>();
                decimal receivedPayment = projectPayments
                    .Where(payment => payment.NgayThanhToanThucTe.HasValue)
                    .Sum(payment => payment.SoTien);
                decimal outstandingPayment = contractValue > 0m
                    ? Math.Max(0m, contractValue - receivedPayment)
                    : projectPayments
                        .Where(payment => !payment.NgayThanhToanThucTe.HasValue)
                        .Sum(payment => payment.SoTien);
                decimal approvedCost;
                decimal pendingCost;
                if (!approvedCostByProject.TryGetValue(project.IdDuAn, out approvedCost))
                    approvedCost = 0m;
                if (!pendingCostByProject.TryGetValue(project.IdDuAn, out pendingCost))
                    pendingCost = 0m;

                summary.Projects.Add(new OverviewProjectFinanceItem
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    ReceivedPayment = receivedPayment,
                    OutstandingPayment = outstandingPayment,
                    ApprovedCost = approvedCost,
                    PendingApprovalCost = pendingCost
                });
                summary.ReceivedPayment += receivedPayment;
                summary.OutstandingPayment += outstandingPayment;
                summary.ApprovedCost += approvedCost;
                summary.PendingApprovalCost += pendingCost;
                if (receivedPayment > 0m || outstandingPayment > 0m
                    || approvedCost > 0m || pendingCost > 0m)
                {
                    summary.ProjectsWithActivityCount++;
                }
            }

            summary.Projects = summary.Projects
                .OrderBy(project => project.ProjectCode)
                .ToList();
            return summary;
        }

        private static List<ProjectEmployeeSummary> GetProjectEmployeeSummaries(
            List<TblDuAn> projects,
            List<TblThanhVienDuAn> projectMembers,
            List<AspnetUser> employees)
        {
            var projectIds = new HashSet<Guid>(
                projects.Select(project => project.IdDuAn));
            var employeesById = employees
                .GroupBy(employee => employee.UserId)
                .ToDictionary(group => group.Key, group => group.First());

            return projectMembers
                .Where(member =>
                    member.IdNhanVien.HasValue
                    && member.IdNhanVien.Value != Guid.Empty
                    && member.DaXoa == false
                    && projectIds.Contains(member.IdDuAn)
                    && employeesById.ContainsKey(member.IdNhanVien.Value))
                .Select(member => employeesById[member.IdNhanVien.Value])
                .GroupBy(employee => employee.UserId)
                .Select(group => group.First())
                .Select(employee => new ProjectEmployeeSummary
                {
                    EmployeeId = employee.UserId,
                    DisplayName = string.IsNullOrWhiteSpace(employee.DisplayName)
                        ? employee.UserName
                        : employee.DisplayName
                })
                .OrderBy(employee => employee.DisplayName)
                .ToList();
        }

        private static string GetTaskStateText(DashboardTaskState state)
        {
            switch (state)
            {
                case DashboardTaskState.Completed:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_COMPLETED);
                case DashboardTaskState.InProgress:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS);
                case DashboardTaskState.Overdue:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE);
                default:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_NOT_STARTED);
            }
        }

        private static List<ProjectAttentionStatistic> GetProjectAttentionStatistics(
    List<TblDuAn> projects,
    List<TblRuiRoDuAn> risks,
    List<TblVanDe> issues)
        {
            var result = new List<ProjectAttentionStatistic>();

            foreach (var project in projects)
            {
                int riskCount = risks.Count(r =>
                    r.IdDuAn == project.IdDuAn);

                int issueCount = issues.Count(i =>
                    i.IdDuAn == project.IdDuAn);

                // Chỉ đưa những dự án thực sự có rủi ro hoặc vấn đề
                if (riskCount == 0 && issueCount == 0)
                {
                    continue;
                }

                result.Add(new ProjectAttentionStatistic
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    RiskCount = riskCount,
                    IssueCount = issueCount
                });
            }

            return result
                .OrderByDescending(x => x.TotalAttentionCount)
                .ThenByDescending(x => x.RiskCount)
                .ThenBy(x => x.ProjectCode)
                .Take(5)
                .ToList();
        }


        private static void ApplyEmployeeLoadBreakdown(
            ResourceOverviewModel overview,
            DashboardResourceModel weeklyOverview)
        {
            if (overview == null)
            {
                return;
            }

            List<ResourceEmployeeLoad> employeeLoads = weeklyOverview == null
                ? new List<ResourceEmployeeLoad>()
                : weeklyOverview.EmployeeLoads
                    ?? new List<ResourceEmployeeLoad>();

            overview.EmployeeBreakdown = employeeLoads
                .Select(employee => new DashboardResourceEmployeeItem
                {
                    EmployeeId = employee.EmployeeId,
                    CategoryKey = GetResourceLoadCategoryKey(employee),
                    DisplayName = employee.DisplayName,
                    DepartmentName = employee.DepartmentName,
                    AllocatedDays = employee.AllocatedDays,
                    CapacityDays = employee.CapacityDays,
                    AverageUtilization = employee.AverageUtilization
                })
                .ToList();

            if (weeklyOverview != null)
            {
                overview.WeekStart = weeklyOverview.AnchorWeekStart;
                overview.WeekEnd = weeklyOverview.AnchorWeekEnd;
            }

            overview.NoLoadEmployeeCount = overview.EmployeeBreakdown.Count(
                employee => employee.CategoryKey == "no-load");
            overview.UnderloadedEmployeeCount = overview.EmployeeBreakdown.Count(
                employee => employee.CategoryKey == "underloaded");
            overview.BalancedEmployeeCount = overview.EmployeeBreakdown.Count(
                employee => employee.CategoryKey == "balanced");
            overview.OverloadedEmployeeCount = overview.EmployeeBreakdown.Count(
                employee => employee.CategoryKey == "overloaded");
        }

        private static string GetResourceLoadCategoryKey(
            ResourceEmployeeLoad employee)
        {
            if (employee == null || employee.AllocatedDays <= 0)
            {
                return "no-load";
            }

            switch (employee.Status)
            {
                case ResourceLoadStatus.Underloaded:
                    return "underloaded";
                case ResourceLoadStatus.Balanced:
                    return "balanced";
                case ResourceLoadStatus.Overloaded:
                    return "overloaded";
                default:
                    return "no-load";
            }
        }

        private static ResourceOverviewModel GetResourceOverview(
            List<AspnetUser> employees,
            List<TblDuAn> projects,
            List<TblThanhVienDuAn> projectMembers)
        {
            HashSet<Guid> validProjectIds = new HashSet<Guid>(
                projects.Select(project => project.IdDuAn));
            HashSet<Guid> validEmployeeIds = new HashSet<Guid>(
                employees.Select(employee => employee.UserId));
            int participatingEmployeeCount = projectMembers
                .Where(member => member.IdNhanVien.HasValue
                    && member.IdNhanVien.Value != Guid.Empty
                    && validEmployeeIds.Contains(member.IdNhanVien.Value)
                    && validProjectIds.Contains(member.IdDuAn))
                .Select(member => member.IdNhanVien.Value)
                .Distinct()
                .Count();

            return new ResourceOverviewModel
            {
                ParticipatingEmployeeCount = participatingEmployeeCount,
                EmployeeBreakdown = new List<DashboardResourceEmployeeItem>()
            };
        }

        private static CostOverviewModel GetCostOverview(
            List<TblChiPhi> costs,
            List<TblDuAn> projects)
        {
            var projectsById = projects
                .GroupBy(project => project.IdDuAn)
                .ToDictionary(group => group.Key, group => group.First());

            List<DashboardOverviewCostItem> costItems = costs
                .Where(cost => projectsById.ContainsKey(cost.IdDuAn))
                .Select(cost =>
                {
                    string categoryKey = GetCostCategoryKey(cost.TrangThai);
                    TblDuAn project = projectsById[cost.IdDuAn];

                    return new DashboardOverviewCostItem
                    {
                        ProjectId = cost.IdDuAn,
                        CategoryKey = categoryKey,
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        CostCode = cost.MaChiPhi,
                        CostName = cost.TenKhoanChi,
                        RecordedDate = cost.NgayTao,
                        Amount = cost.SoTien
                    };
                })
                .OrderByDescending(cost => cost.RecordedDate)
                .ThenBy(cost => cost.ProjectCode)
                .ThenBy(cost => cost.CostCode)
                .ToList();

            return new CostOverviewModel
            {
                ApprovedCost = costItems
                    .Where(cost => cost.CategoryKey == "approved")
                    .Sum(cost => cost.Amount),
                PendingApprovalCost = costItems
                    .Where(cost => cost.CategoryKey == "pending")
                    .Sum(cost => cost.Amount),
                RejectedCost = costItems
                    .Where(cost => cost.CategoryKey == "rejected")
                    .Sum(cost => cost.Amount),
                CostItems = costItems
            };
        }

        private static string GetCostCategoryKey(byte status)
        {
            if (status == (byte)TrangThaiChiPhi.NotApproved)
            {
                return "pending";
            }

            if (status == (byte)TrangThaiChiPhi.Rejected)
            {
                return "rejected";
            }

            return "approved";
        }

        private static int GetUpcomingMeetingCount(
    List<TblLichHop> meetings,
    DateTime currentTime)
        {
            return meetings.Count(m =>
                m.ThoiGianBatDau >= currentTime
            );
        }

        private static List<UpcomingMeetingSummary> GetUpcomingMeetingSummaries(
            List<TblLichHop> meetings,
            List<TblDuAn> projects,
            DateTime currentTime)
        {
            var projectsById = projects.ToDictionary(
                x => x.IdDuAn,
                x => x);

            return meetings
                .Where(m => m.ThoiGianBatDau >= currentTime)
                .OrderBy(m => m.ThoiGianBatDau)
                .Select(m => new UpcomingMeetingSummary
                {
                    ProjectId = m.IdDuAn,
                    ProjectCode = projectsById.ContainsKey(m.IdDuAn)
                        ? projectsById[m.IdDuAn].MaDuAn
                        : string.Empty,
                    ProjectName = projectsById.ContainsKey(m.IdDuAn)
                        ? projectsById[m.IdDuAn].TenDuAn
                        : string.Empty,
                    Title = m.TenCuocHop,
                    StartTime = m.ThoiGianBatDau,
                    EndTime = m.ThoiGianKetThuc,
                    Location = m.DiaDiemHop,
                    Content = m.NoiDungCuocHop
                })
                .ToList();
        }


        private static string GetProjectStateText(
            DashboardProjectState state)
        {
            switch (state)
            {
                case DashboardProjectState.Completed:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_COMPLETED);
                case DashboardProjectState.InProgress:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS);
                case DashboardProjectState.Overdue:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE);
                default:
                    return UITextsReader.GetBackEndResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_NOT_STARTED);
            }
        }

        public DashboardOverviewModel GetOverview()
        {
            return GetOverview(new DashboardFilter
            {
                ProjectId = null,
                FromDate = DateTime.MinValue,
                ToDate = DateTime.MaxValue
            });
        }


        public List<TblDuAn> GetProjectsForFilter()
        {
            return _repository.GetProjectsForFilter();
        }
    }
}
