using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardOverviewManager : BaseManager
    {
        private const int DueSoonDays = 7;

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
            List<TblRuiRoDuAn> risks = _repository.GetRisks(filter);
            List<TblVanDe> issues = _repository.GetIssues(filter);
            List<TblLichHop> meetings = _repository
                .GetMeetings(filter)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();

            decimal atRiskProjectRate = GetAtRiskProjectRate(
                projects,
                allTasks,
                today);
            DashboardFinancialSummary financialSummary =
                _repository.GetFinancialSummary(filter);

            List<AspnetUser> employees = _repository.GetEmployees();
            List<TblThanhVienDuAn> projectMembers =
                _repository.GetProjectMembers(null);
            List<TblCongViecNhanVien> taskAssignments =
                _repository.GetTaskAssignments(filter);

            int totalProjectCount = projects.Count;
            int activeProjectCount = projects.Count(p =>
                p.NgayBatDau.Date <= today &&
                p.NgayDuKienHoanThanh.Date >= today &&
                !p.NgayHoanThanhThucTe.HasValue
            );

            int upcomingMeetingCount = GetUpcomingMeetingCount(
                meetings,
                generatedAt);
            int overdueTaskCount = allTasks.Count(x =>
                IsTaskOverdue(x, today));

            decimal totalContractValue =
                financialSummary.TotalContractValue;
            CostOverviewModel costOverview = GetCostOverview(financialSummary);

            var projectProgressStats = GetProjectProgressStatistics(
                projects,
                allTasks,
                today);
            
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
                TotalTaskCount = allTasks.Count,
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

                AtRiskProjectRate = atRiskProjectRate,
            };
        }

        private static List<ProjectStatusStatistic> GetProjectStatusStatistics(
    List<TblDuAn> projects)
        {
            DateTime today = DateTime.Now.Date;

            int completed = projects.Count(p => p.NgayHoanThanhThucTe.HasValue);
            int notStarted = projects.Count(p =>
                !p.NgayHoanThanhThucTe.HasValue &&
                p.NgayBatDau.Date > today);
            int overdue = projects.Count(p =>
                !p.NgayHoanThanhThucTe.HasValue &&
                p.NgayBatDau.Date <= today &&
                p.NgayDuKienHoanThanh.Date < today);

            int ongoing = projects.Count
                - completed
                - notStarted
                - overdue;

            return new List<ProjectStatusStatistic>
    {
        new ProjectStatusStatistic
        {
            Status = "Đang thực hiện",
            Count = ongoing
        },

        new ProjectStatusStatistic
        {
            Status = "Hoàn thành",
            Count = completed
        },

        new ProjectStatusStatistic
        {
            Status = "Chưa bắt đầu",
            Count = notStarted
        },

        new ProjectStatusStatistic
        {
            Status = "Quá hạn",
            Count = overdue
        }
    };
        }


        private static List<ProjectProgressStatistic> GetProjectProgressStatistics(
    List<TblDuAn> projects,
    List<TblCongViec> tasks,
    DateTime today)
        {
            var result = new List<ProjectProgressStatistic>();

            foreach (var project in projects)
            {
                decimal progress = 0;
                var projectTasks = tasks
                    .Where(t => t.IdDuAn == project.IdDuAn)
                    .ToList();

                // Nếu dự án đã được đánh dấu là hoàn thành thực tế thì tiến độ luôn là 100%
                if (project.NgayHoanThanhThucTe.HasValue)
                {
                    progress = 100;
                }
                else if (project.NgayBatDau.Date > today)
                {
                    progress = 0; // Chưa bắt đầu
                }
                else
                {
                    if (projectTasks.Count > 0)
                    {
                        progress = Convert.ToDecimal(
                            projectTasks.Average(
                                t => Convert.ToDecimal(
                                    NormalizeProgress(t.PhanTramHoanThanh))
                            )
                        );
                    }
                }

                decimal plannedProgress = project.NgayHoanThanhThucTe.HasValue
                    ? 100
                    : GetPlannedProgress(
                        project.NgayBatDau.Date,
                        project.NgayDuKienHoanThanh.Date,
                        today);
                decimal variance = Math.Round(
                    progress - plannedProgress,
                    2);
                int overdueTaskCount = projectTasks.Count(x =>
                    IsTaskOverdue(x, today));
                int dueSoonTaskCount = projectTasks.Count(x =>
                    IsTaskDueSoon(x, today));

                result.Add(new ProjectProgressStatistic
                {
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    Progress = Math.Round(progress, 2),
                    PlannedProgress = plannedProgress,
                    Variance = variance,
                    OverdueTaskCount = overdueTaskCount,
                    DueSoonTaskCount = dueSoonTaskCount,
                    Health = GetProjectHealth(
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


        private static decimal GetAtRiskProjectRate(
    List<TblDuAn> projects,
    List<TblCongViec> tasks,
    DateTime today)
        {
            // Các dự án đang hoạt động:
            // - đã bắt đầu
            // - chưa hoàn thành
            var activeProjects = projects
                .Where(p =>
                    p.NgayBatDau.Date <= today &&
                    p.NgayDuKienHoanThanh.Date >= today &&
                    !p.NgayHoanThanhThucTe.HasValue)
                .ToList();

            if (activeProjects.Count == 0)
            {
                return 0;
            }

            int atRiskCount = 0;

            foreach (var project in activeProjects)
            {
                DateTime startDate = project.NgayBatDau.Date;
                DateTime endDate = project.NgayDuKienHoanThanh.Date;

                var projectTasks = tasks
                    .Where(t =>
                        t.IdDuAn == project.IdDuAn &&
                        t.DaXoa == false)
                    .ToList();

                decimal actualProgress = 0;

                if (projectTasks.Count > 0)
                {
                    actualProgress = projectTasks.Average(
                        t => Convert.ToDecimal(
                            NormalizeProgress(t.PhanTramHoanThanh))
                    );
                }

                decimal plannedProgress = GetPlannedProgress(
                    startDate,
                    endDate,
                    today);
                decimal variance = actualProgress - plannedProgress;
                bool hasOverdueTask = projectTasks.Any(x =>
                    IsTaskOverdue(x, today));

                if (variance < -5 || hasOverdueTask)
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

        private static decimal GetPlannedProgress(
            DateTime startDate,
            DateTime endDate,
            DateTime today)
        {
            if (today <= startDate)
            {
                return 0;
            }

            if (today >= endDate || endDate <= startDate)
            {
                return 100;
            }

            decimal elapsedDays = Convert.ToDecimal(
                (today - startDate).TotalDays);
            decimal totalDays = Convert.ToDecimal(
                (endDate - startDate).TotalDays);

            return Math.Round((elapsedDays / totalDays) * 100, 2);
        }

        private static bool IsTaskCompleted(TblCongViec task)
        {
            return task.NgayHoanThanhThucTe.HasValue
                || NormalizeProgress(task.PhanTramHoanThanh) >= 100;
        }

        private static bool IsTaskOverdue(
            TblCongViec task,
            DateTime today)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date < today;
        }

        private static bool IsTaskDueSoon(
            TblCongViec task,
            DateTime today)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date >= today
                && task.NgayKetThuc.Value.Date <= today.AddDays(DueSoonDays);
        }

        private static ProjectScheduleHealth GetProjectHealth(
            TblDuAn project,
            decimal variance,
            int overdueTaskCount,
            DateTime today)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return ProjectScheduleHealth.Completed;
            }

            if (project.NgayBatDau.Date > today)
            {
                return ProjectScheduleHealth.NotStarted;
            }

            if (project.NgayDuKienHoanThanh.Date < today)
            {
                return ProjectScheduleHealth.Overdue;
            }

            if (variance <= -15)
            {
                return ProjectScheduleHealth.BehindSchedule;
            }

            if (variance < -5 || overdueTaskCount > 0)
            {
                return ProjectScheduleHealth.AtRisk;
            }

            return ProjectScheduleHealth.OnTrack;
        }

        private static int NormalizeProgress(int progress)
        {
            return Math.Max(0, Math.Min(100, progress));
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
