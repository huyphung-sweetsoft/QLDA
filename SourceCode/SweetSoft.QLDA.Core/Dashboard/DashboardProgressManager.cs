using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardProgressManager : BaseManager
    {
        private static readonly Lazy<DashboardProgressManager> LazyInstance =
            new Lazy<DashboardProgressManager>(() => new DashboardProgressManager());

        private readonly DashboardRepository _repository;

        public DashboardProgressManager(
            IAppContext applicationContext = null,
            DashboardRepository repository = null)
            : base(applicationContext)
        {
            _repository = repository ?? new DashboardRepository();
        }

        public static DashboardProgressManager Instance => LazyInstance.Value;

        public DashboardProgressModel GetProgress(DashboardFilter filter)
        {
            DateTime generatedAt = DateTime.Now;
            DateTime today = generatedAt.Date;
            bool isSingleProject = filter != null
                && filter.ProjectId.HasValue;

            List<TblDuAn> projects = _repository.GetProjects(filter);
            HashSet<Guid> projectIds = new HashSet<Guid>(
                projects.Select(x => x.IdDuAn));

            List<TblCongViec> allProjectTasks = _repository
                .GetTasks(filter, false)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();

            Dictionary<Guid, TblDoUuTien> priorities = _repository
                .GetPriorities()
                .ToDictionary(x => x.IdDoUuTien);

            List<ProjectScheduleStatistic> projectStatistics =
                BuildProjectScheduleStatistics(projects, allProjectTasks, today);

            int completedTaskCount = allProjectTasks.Count(
                DashboardProgressCalculator.IsTaskCompleted);
            int overdueTaskCount = allProjectTasks.Count(x =>
                DashboardProgressCalculator.IsTaskOverdue(x, today));
            int inProgressTaskCount = allProjectTasks.Count(x =>
                DashboardProgressCalculator.GetTaskState(x, today)
                    == DashboardTaskState.InProgress);
            int notStartedTaskCount = allProjectTasks.Count(x =>
                DashboardProgressCalculator.GetTaskState(x, today)
                    == DashboardTaskState.NotStarted);
            int dueSoonTaskCount = allProjectTasks.Count(x =>
                DashboardProgressCalculator.IsTaskDueSoon(x, today));

            decimal overallProgress = projectStatistics.Count == 0
                ? 0
                : projectStatistics.Average(x => x.ActualProgress);

            return new DashboardProgressModel
            {
                IsSingleProject = isSingleProject,
                GeneratedAt = generatedAt,
                TotalProjectCount = projects.Count,
                TotalTaskCount = allProjectTasks.Count,
                OverallProgress = Math.Round(overallProgress, 2),
                CompletedTaskCount = completedTaskCount,
                InProgressTaskCount = inProgressTaskCount,
                NotStartedTaskCount = notStartedTaskCount,
                OverdueTaskCount = overdueTaskCount,
                DueSoonTaskCount = dueSoonTaskCount,
                NeedsAttentionProjectCount = projectStatistics.Count(x =>
                    x.Health == ProjectScheduleHealth.AtRisk
                    || x.Health == ProjectScheduleHealth.BehindSchedule
                    || x.Health == ProjectScheduleHealth.Overdue),
                TaskStatusStatistics = BuildTaskStatusStatistics(
                    completedTaskCount,
                    inProgressTaskCount,
                    notStartedTaskCount,
                    overdueTaskCount),
                ProjectScheduleStatistics = projectStatistics,
                ProjectTaskStatistics = BuildProjectTaskStatistics(
                    projects,
                    allProjectTasks,
                    today),
                TaskProgressDetails = isSingleProject
                    ? BuildTaskProgressDetails(
                        allProjectTasks,
                        projects,
                        priorities,
                        today)
                    : new List<TaskProgressDetail>(),
                AttentionTasks = BuildAttentionTasks(
                    allProjectTasks,
                    projects,
                    priorities,
                    today)
            };
        }

        private static List<TaskProgressDetail> BuildTaskProgressDetails(
            List<TblCongViec> tasks,
            List<TblDuAn> projects,
            Dictionary<Guid, TblDoUuTien> priorities,
            DateTime today)
        {
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);

            return tasks
                .Select(task =>
                {
                    DashboardTaskState state =
                        DashboardProgressCalculator.GetTaskState(task, today);
                    TblDuAn project;
                    projectById.TryGetValue(task.IdDuAn, out project);

                    TblDoUuTien priority = null;
                    if (task.IdDoUuTien.HasValue)
                    {
                        priorities.TryGetValue(task.IdDoUuTien.Value, out priority);
                    }

                    return new TaskProgressDetail
                    {
                        TaskId = task.IdCongViec,
                        ProjectId = task.IdDuAn,
                        TaskCode = task.MaCongViec,
                        TaskName = task.TenCongViec,
                        ProjectCode = project == null
                            ? string.Empty
                            : project.MaDuAn,
                        ProjectName = project == null
                            ? string.Empty
                            : project.TenDuAn,
                        PriorityName = priority == null
                            ? UITextsReader.GetBackEndResourceText(
                                BackEndResourceKeys.DASHBOARD_PRIORITY_NOT_SET)
                            : priority.TenDoUuTien,
                        PriorityScore = priority == null
                            ? 0
                            : priority.DiemUuTien,
                        Progress = DashboardProgressCalculator.NormalizeProgress(
                            task.PhanTramHoanThanh),
                        Status = GetTaskStateText(state),
                        StatusCode = (int)state,
                        Deadline = task.NgayKetThuc,
                        DaysToDeadline = task.NgayKetThuc.HasValue
                            ? (int?)(task.NgayKetThuc.Value.Date - today).Days
                            : null
                    };
                })
                .OrderBy(x =>
                    x.StatusCode == (int)DashboardTaskState.Completed ? 1 : 0)
                .ThenByDescending(x =>
                    x.StatusCode == (int)DashboardTaskState.Completed
                        ? 0
                        : x.PriorityScore)
                .ThenBy(x => GetTaskStateOrder(x.StatusCode))
                .ThenBy(x => x.Deadline ?? DateTime.MaxValue)
                .ThenBy(x => x.TaskCode)
                .ToList();
        }

        public List<TblDuAn> GetProjectsForFilter()
        {
            return _repository.GetProjectsForFilter();
        }

        private static List<ProjectScheduleStatistic>
            BuildProjectScheduleStatistics(
                List<TblDuAn> projects,
                List<TblCongViec> tasks,
                DateTime today)
        {
            List<ProjectScheduleStatistic> result =
                new List<ProjectScheduleStatistic>();

            foreach (TblDuAn project in projects)
            {
                List<TblCongViec> projectTasks = tasks
                    .Where(x => x.IdDuAn == project.IdDuAn)
                    .ToList();

                decimal actualProgress =
                    DashboardProgressCalculator.GetProjectActualProgress(
                    project,
                    projectTasks,
                    today);
                decimal plannedProgress =
                    DashboardProgressCalculator.GetPlannedProgress(
                        project,
                        today);
                decimal variance = Math.Round(
                    actualProgress - plannedProgress,
                    2);
                int overdueTaskCount = projectTasks.Count(x =>
                    DashboardProgressCalculator.IsTaskOverdue(x, today));

                result.Add(new ProjectScheduleStatistic
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    StartDate = project.NgayBatDau,
                    ExpectedEndDate = project.NgayDuKienHoanThanh,
                    ActualCompletionDate = project.NgayHoanThanhThucTe,
                    ActualProgress = actualProgress,
                    PlannedProgress = plannedProgress,
                    Variance = variance,
                    TotalTaskCount = projectTasks.Count,
                    CompletedTaskCount = projectTasks.Count(
                        DashboardProgressCalculator.IsTaskCompleted),
                    OverdueTaskCount = overdueTaskCount,
                    Health = DashboardProgressCalculator.GetProjectHealth(
                        project,
                        variance,
                        overdueTaskCount,
                        today)
                });
            }

            return result
                .OrderByDescending(x => GetHealthOrder(x.Health))
                .ThenBy(x => x.Variance)
                .ThenBy(x => x.ProjectCode)
                .ToList();
        }

        private static List<ProjectTaskProgressStatistic>
            BuildProjectTaskStatistics(
                List<TblDuAn> projects,
                List<TblCongViec> tasks,
                DateTime today)
        {
            return projects
                .Select(project =>
                {
                    List<TblCongViec> projectTasks = tasks
                        .Where(x => x.IdDuAn == project.IdDuAn)
                        .ToList();

                    return new ProjectTaskProgressStatistic
                    {
                        ProjectId = project.IdDuAn,
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        CompletedCount = projectTasks.Count(x =>
                            DashboardProgressCalculator.GetTaskState(x, today)
                                == DashboardTaskState.Completed),
                        InProgressCount = projectTasks.Count(x =>
                            DashboardProgressCalculator.GetTaskState(x, today)
                                == DashboardTaskState.InProgress),
                        NotStartedCount = projectTasks.Count(x =>
                            DashboardProgressCalculator.GetTaskState(x, today)
                                == DashboardTaskState.NotStarted),
                        OverdueCount = projectTasks.Count(x =>
                            DashboardProgressCalculator.GetTaskState(x, today)
                                == DashboardTaskState.Overdue)
                    };
                })
                .OrderByDescending(x => x.OverdueCount)
                .ThenBy(x => x.ProjectCode)
                .ToList();
        }

        private static List<ProgressTaskInfo> BuildAttentionTasks(
            List<TblCongViec> tasks,
            List<TblDuAn> projects,
            Dictionary<Guid, TblDoUuTien> priorities,
            DateTime today)
        {
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);

            List<ProgressTaskInfo> result = new List<ProgressTaskInfo>();

            foreach (TblCongViec task in tasks.Where(x =>
                DashboardProgressCalculator.IsTaskOverdue(x, today)
                || DashboardProgressCalculator.IsTaskDueSoon(x, today)))
            {
                TblDuAn project;
                projectById.TryGetValue(task.IdDuAn, out project);

                TblDoUuTien priority = null;
                if (task.IdDoUuTien.HasValue)
                {
                    priorities.TryGetValue(task.IdDoUuTien.Value, out priority);
                }

                DateTime deadline = task.NgayKetThuc.Value.Date;

                result.Add(new ProgressTaskInfo
                {
                    TaskId = task.IdCongViec,
                    ProjectId = task.IdDuAn,
                    TaskCode = task.MaCongViec,
                    TaskName = task.TenCongViec,
                    ProjectCode = project == null ? string.Empty : project.MaDuAn,
                    ProjectName = project == null ? string.Empty : project.TenDuAn,
                    PriorityName = priority == null
                        ? UITextsReader.GetBackEndResourceText(
                            BackEndResourceKeys.DASHBOARD_PRIORITY_NOT_SET)
                        : priority.TenDoUuTien,
                    PriorityScore = priority == null ? 0 : priority.DiemUuTien,
                    Deadline = deadline,
                    Progress = DashboardProgressCalculator.NormalizeProgress(
                        task.PhanTramHoanThanh),
                    DaysToDeadline = (deadline - today).Days,
                    IsOverdue = deadline < today
                });
            }

            return result
                .OrderByDescending(x => x.IsOverdue)
                .ThenBy(x => x.DaysToDeadline)
                .ThenByDescending(x => x.PriorityScore)
                .ThenBy(x => x.TaskCode)
                .Take(15)
                .ToList();
        }

        private static List<TaskProgressStatusStatistic>
            BuildTaskStatusStatistics(
                int completed,
                int inProgress,
                int notStarted,
                int overdue)
        {
            return new List<TaskProgressStatusStatistic>
            {
                new TaskProgressStatusStatistic
                {
                    Status = GetTaskStateText(DashboardTaskState.Completed),
                    Count = completed
                },
                new TaskProgressStatusStatistic
                {
                    Status = GetTaskStateText(DashboardTaskState.InProgress),
                    Count = inProgress
                },
                new TaskProgressStatusStatistic
                {
                    Status = GetTaskStateText(DashboardTaskState.NotStarted),
                    Count = notStarted
                },
                new TaskProgressStatusStatistic
                {
                    Status = GetTaskStateText(DashboardTaskState.Overdue),
                    Count = overdue
                }
            };
        }

        private static int GetHealthOrder(ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.Overdue: return 5;
                case ProjectScheduleHealth.BehindSchedule: return 4;
                case ProjectScheduleHealth.AtRisk: return 3;
                case ProjectScheduleHealth.NotStarted: return 2;
                case ProjectScheduleHealth.OnTrack: return 1;
                default: return 0;
            }
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

        private static int GetTaskStateOrder(int statusCode)
        {
            DashboardTaskState state = (DashboardTaskState)statusCode;
            switch (state)
            {
                case DashboardTaskState.Overdue: return 0;
                case DashboardTaskState.InProgress: return 1;
                case DashboardTaskState.NotStarted: return 2;
                default: return 3;
            }
        }
    }
}
