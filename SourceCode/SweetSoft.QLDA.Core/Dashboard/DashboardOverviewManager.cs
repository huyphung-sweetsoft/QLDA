using System;
using System.Collections.Generic;
using System.Linq;
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
            List<TblLichHop> meetings = _repository
                .GetMeetings(filter)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();

            decimal atRiskProjectRate = GetAtRiskProjectRate(
                projects,
                progressTasks,
                today,
                workingCalendar);
            DashboardFinancialSummary financialSummary =
                _repository.GetFinancialSummary(filter);

            List<AspnetUser> employees = _repository.GetEmployees();
            List<TblThanhVienDuAn> projectMembers =
                _repository.GetProjectMembers(null);
            List<TblCongViecNhanVien> taskAssignments =
                _repository.GetTaskAssignments(filter);

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
            CostOverviewModel costOverview = GetCostOverview(financialSummary);

            var projectProgressStats = GetProjectProgressStatistics(
                projects,
                progressTasks,
                today,
                workingCalendar);
            
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
                    GetProjectStatusStatistics(projects, today),
                ProjectProgressStatistics = projectProgressStats,
                ProjectAttentionStatistics =
    GetProjectAttentionStatistics(
        projects,
        risks,
        issues),
                ResourceOverview =
    GetResourceOverview(
        employees,
        projects,
        projectMembers,
        taskAssignments,
        allTasks),

                CostOverview = costOverview,

                ActiveProjectCount = activeProjectCount,
                UpcomingMeetingCount = upcomingMeetingCount,
                UpcomingMeetings = upcomingMeetings,

                AtRiskProjectRate = atRiskProjectRate,
            };
        }

        private static List<ProjectStatusStatistic> GetProjectStatusStatistics(
            List<TblDuAn> projects,
            DateTime today)
        {
            int completed = projects.Count(p =>
                DashboardProgressCalculator.GetProjectState(p, today)
                    == DashboardProjectState.Completed);
            int notStarted = projects.Count(p =>
                DashboardProgressCalculator.GetProjectState(p, today)
                    == DashboardProjectState.NotStarted);
            int overdue = projects.Count(p =>
                DashboardProgressCalculator.GetProjectState(p, today)
                    == DashboardProjectState.Overdue);

            int ongoing = projects.Count
                - completed
                - notStarted
                - overdue;

            return new List<ProjectStatusStatistic>
            {
                new ProjectStatusStatistic
                {
                    Status = GetProjectStateText(
                        DashboardProjectState.InProgress),
                    Count = ongoing
                },
                new ProjectStatusStatistic
                {
                    Status = GetProjectStateText(
                        DashboardProjectState.Completed),
                    Count = completed
                },
                new ProjectStatusStatistic
                {
                    Status = GetProjectStateText(
                        DashboardProjectState.NotStarted),
                    Count = notStarted
                },
                new ProjectStatusStatistic
                {
                    Status = GetProjectStateText(
                        DashboardProjectState.Overdue),
                    Count = overdue
                }
            };
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


        private static ResourceOverviewModel GetResourceOverview(
    List<AspnetUser> employees,
    List<TblDuAn> projects,
    List<TblThanhVienDuAn> projectMembers,
    List<TblCongViecNhanVien> taskAssignments,
    List<TblCongViec> tasks)
        {
            var validProjectIds = new HashSet<Guid>(
                projects.Select(x => x.IdDuAn)
            );
            var validEmployeeIds = new HashSet<Guid>(
                employees.Select(x => x.UserId));

            var members = projectMembers
                .Where(x =>
                    x.IdNhanVien.HasValue &&
                    x.IdNhanVien.Value != Guid.Empty &&
                    validEmployeeIds.Contains(x.IdNhanVien.Value) &&
                    validProjectIds.Contains(x.IdDuAn))
                .ToList();

            var participatingEmployeeIds = members
                .Select(x => x.IdNhanVien.Value)
                .Distinct()
                .ToList();

            int totalEmployeeCount = employees.Count;

            int participatingEmployeeCount =
                participatingEmployeeIds.Count;

            int unassignedEmployeeCount =
                Math.Max(0, totalEmployeeCount - participatingEmployeeCount);

            int multiProjectEmployeeCount = participatingEmployeeIds.Count(
                employeeId => projectMembers
                    .Where(x =>
                        x.IdNhanVien == employeeId &&
                        x.DaXoa == false)
                    .Select(x => x.IdDuAn)
                    .Distinct()
                    .Count() > 1);

            HashSet<Guid> validTaskIds = new HashSet<Guid>(
                tasks.Select(x => x.IdCongViec));
            HashSet<Guid> assignedEmployeeIds = new HashSet<Guid>(
                taskAssignments
                    .Where(x => validTaskIds.Contains(x.IdCongViec))
                    .Select(x => x.IdNhanVien));
            int assignedProjectMemberCount = participatingEmployeeIds.Count(
                assignedEmployeeIds.Contains);
            int unassignedProjectMemberCount = Math.Max(
                0,
                participatingEmployeeCount - assignedProjectMemberCount);

            var projectResourceStatistics = new List<ProjectResourceStatistic>();

            foreach (var project in projects)
            {
                var projectMemberIds = members
                    .Where(x => x.IdDuAn == project.IdDuAn)
                    .Select(x => x.IdNhanVien.Value)
                    .Distinct()
                    .ToList();

                int multiProjectMemberCount = projectMemberIds.Count(
                    employeeId =>
                        projectMembers
                            .Where(x => x.IdNhanVien == employeeId)
                            .Select(x => x.IdDuAn)
                            .Distinct()
                            .Count() > 1
                );

                projectResourceStatistics.Add(
                    new ProjectResourceStatistic
                    {
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        MemberCount = projectMemberIds.Count,
                        MultiProjectMemberCount = multiProjectMemberCount
                    }
                );
            }

            return new ResourceOverviewModel
            {
                TotalEmployeeCount = totalEmployeeCount,
                ParticipatingEmployeeCount = participatingEmployeeCount,
                UnassignedEmployeeCount = unassignedEmployeeCount,
                MultiProjectEmployeeCount = multiProjectEmployeeCount,
                AssignedProjectMemberCount = assignedProjectMemberCount,
                UnassignedProjectMemberCount =
                    unassignedProjectMemberCount,
      

                ProjectResourceStatistics =
                    projectResourceStatistics
                        .OrderByDescending(x => x.MemberCount)
                        .ThenBy(x => x.ProjectCode)
                        .ToList()



            };

        }

        private static CostOverviewModel GetCostOverview(
    DashboardFinancialSummary financialSummary)
        {
            decimal totalContractValue =
                financialSummary.TotalContractValue;
            decimal actualCost = financialSummary.ActualCost;

            decimal remainingAfterCost =
                totalContractValue - actualCost;

            return new CostOverviewModel
            {
                TotalContractValue = totalContractValue,
                ActualCost = actualCost,
                ReceivedPayment = financialSummary.ReceivedPayment,
                RemainingAfterCost = remainingAfterCost
            };
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
            var projectCodes = projects.ToDictionary(
                x => x.IdDuAn,
                x => x.MaDuAn);

            return meetings
                .Where(m => m.ThoiGianBatDau >= currentTime)
                .OrderBy(m => m.ThoiGianBatDau)
                .Take(5)
                .Select(m => new UpcomingMeetingSummary
                {
                    ProjectId = m.IdDuAn,
                    ProjectCode = projectCodes.ContainsKey(m.IdDuAn)
                        ? projectCodes[m.IdDuAn]
                        : string.Empty,
                    Title = m.TenCuocHop,
                    StartTime = m.ThoiGianBatDau,
                    Location = m.DiaDiemHop
                })
                .ToList();
        }


        private static decimal GetAtRiskProjectRate(
    List<TblDuAn> projects,
    List<TblCongViec> tasks,
    DateTime today,
    DashboardWorkingCalendar workingCalendar)
        {
            // Các dự án đang hoạt động:
            // - đã bắt đầu
            // - chưa hoàn thành
            var activeProjects = projects
                .Where(p =>
                    DashboardProgressCalculator.GetProjectState(p, today)
                        == DashboardProjectState.InProgress)
                .ToList();

            if (activeProjects.Count == 0)
            {
                return 0;
            }

            int atRiskCount = 0;

            foreach (var project in activeProjects)
            {
                var projectTasks = tasks
                    .Where(t =>
                        t.IdDuAn == project.IdDuAn &&
                        t.DaXoa == false)
                    .ToList();

                decimal actualProgress =
                    DashboardProgressCalculator.GetProjectActualProgress(
                        project,
                        projectTasks,
                        today);

                decimal plannedProgress =
                    DashboardProgressCalculator.GetPlannedProgress(
                        project,
                        today,
                        workingCalendar);
                decimal variance = actualProgress - plannedProgress;
                int overdueTaskCount = projectTasks.Count(x =>
                    DashboardProgressCalculator.IsTaskOverdue(x, today));
                ProjectScheduleHealth health =
                    DashboardProgressCalculator.GetProjectHealth(
                        project,
                        variance,
                        overdueTaskCount,
                        today);

                if (health == ProjectScheduleHealth.AtRisk
                    || health == ProjectScheduleHealth.BehindSchedule)
                {
                    atRiskCount++;
                }
            }

            decimal rate =
                (decimal)atRiskCount /
                activeProjects.Count *
                100;

            return Math.Round(rate, 2);
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
