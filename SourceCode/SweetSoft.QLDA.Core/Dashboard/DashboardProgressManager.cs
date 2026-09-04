using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class DashboardProgressManager : BaseManager
    {
        private const int DueSoonDays = 7;

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

            int completedTaskCount = allProjectTasks.Count(IsCompleted);
            int overdueTaskCount = allProjectTasks.Count(x => IsOverdue(x, today));
            int inProgressTaskCount = allProjectTasks.Count(x =>
                GetTaskState(x, today) == ProgressTaskState.InProgress);
            int notStartedTaskCount = allProjectTasks.Count(x =>
                GetTaskState(x, today) == ProgressTaskState.NotStarted);
            int dueSoonTaskCount = allProjectTasks.Count(x =>
                IsDueSoon(x, today));

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
                    ProgressTaskState state = GetTaskState(task, today);
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
                        TaskCode = task.MaCongViec,
                        TaskName = task.TenCongViec,
                        ProjectCode = project == null
                            ? string.Empty
                            : project.MaDuAn,
                        ProjectName = project == null
                            ? string.Empty
                            : project.TenDuAn,
                        PriorityName = priority == null
                            ? "Chưa đặt ưu tiên"
                            : priority.TenDoUuTien,
                        PriorityScore = priority == null
                            ? 0
                            : priority.DiemUuTien,
                        Progress = NormalizeProgress(
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
                    x.StatusCode == (int)ProgressTaskState.Completed ? 1 : 0)
                .ThenByDescending(x =>
                    x.StatusCode == (int)ProgressTaskState.Completed
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

                decimal actualProgress = GetProjectActualProgress(
                    project,
                    projectTasks,
                    today);
                decimal plannedProgress = GetPlannedProgress(project, today);
                decimal variance = Math.Round(
                    actualProgress - plannedProgress,
                    2);
                int overdueTaskCount = projectTasks.Count(x =>
                    IsOverdue(x, today));

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
                    CompletedTaskCount = projectTasks.Count(IsCompleted),
                    OverdueTaskCount = overdueTaskCount,
                    Health = GetProjectHealth(
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
                        ProjectCode = project.MaDuAn,
                        ProjectName = project.TenDuAn,
                        CompletedCount = projectTasks.Count(x =>
                            GetTaskState(x, today) == ProgressTaskState.Completed),
                        InProgressCount = projectTasks.Count(x =>
                            GetTaskState(x, today) == ProgressTaskState.InProgress),
                        NotStartedCount = projectTasks.Count(x =>
                            GetTaskState(x, today) == ProgressTaskState.NotStarted),
                        OverdueCount = projectTasks.Count(x =>
                            GetTaskState(x, today) == ProgressTaskState.Overdue)
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
                IsOverdue(x, today) || IsDueSoon(x, today)))
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
                    TaskCode = task.MaCongViec,
                    TaskName = task.TenCongViec,
                    ProjectCode = project == null ? string.Empty : project.MaDuAn,
                    ProjectName = project == null ? string.Empty : project.TenDuAn,
                    PriorityName = priority == null
                        ? "Chưa đặt ưu tiên"
                        : priority.TenDoUuTien,
                    PriorityScore = priority == null ? 0 : priority.DiemUuTien,
                    Deadline = deadline,
                    Progress = NormalizeProgress(task.PhanTramHoanThanh),
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
                    Status = "Hoàn thành",
                    Count = completed
                },
                new TaskProgressStatusStatistic
                {
                    Status = "Đang thực hiện",
                    Count = inProgress
                },
                new TaskProgressStatusStatistic
                {
                    Status = "Chưa bắt đầu",
                    Count = notStarted
                },
                new TaskProgressStatusStatistic
                {
                    Status = "Quá hạn",
                    Count = overdue
                }
            };
        }

        private static decimal GetProjectActualProgress(
            TblDuAn project,
            List<TblCongViec> tasks,
            DateTime today)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return 100;
            }

            if (project.NgayBatDau.Date > today || tasks.Count == 0)
            {
                return 0;
            }

            return Math.Round(
                Convert.ToDecimal(tasks.Average(x =>
                    NormalizeProgress(x.PhanTramHoanThanh))),
                2);
        }

        private static decimal GetPlannedProgress(
            TblDuAn project,
            DateTime today)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return 100;
            }

            DateTime startDate = project.NgayBatDau.Date;
            DateTime endDate = project.NgayDuKienHoanThanh.Date;

            if (today <= startDate)
            {
                return 0;
            }

            if (today >= endDate || endDate <= startDate)
            {
                return 100;
            }

            decimal elapsedDays = Convert.ToDecimal((today - startDate).TotalDays);
            decimal totalDays = Convert.ToDecimal((endDate - startDate).TotalDays);

            return Math.Round((elapsedDays / totalDays) * 100, 2);
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

        private static ProgressTaskState GetTaskState(
            TblCongViec task,
            DateTime today)
        {
            if (IsCompleted(task))
            {
                return ProgressTaskState.Completed;
            }

            if (IsOverdue(task, today))
            {
                return ProgressTaskState.Overdue;
            }

            return NormalizeProgress(task.PhanTramHoanThanh) > 0
                ? ProgressTaskState.InProgress
                : ProgressTaskState.NotStarted;
        }

        private static bool IsCompleted(TblCongViec task)
        {
            return task.NgayHoanThanhThucTe.HasValue
                || NormalizeProgress(task.PhanTramHoanThanh) >= 100;
        }

        private static bool IsOverdue(TblCongViec task, DateTime today)
        {
            return !IsCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date < today;
        }

        private static bool IsDueSoon(TblCongViec task, DateTime today)
        {
            return !IsCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date >= today
                && task.NgayKetThuc.Value.Date <= today.AddDays(DueSoonDays);
        }

        private static int NormalizeProgress(int progress)
        {
            return Math.Max(0, Math.Min(100, progress));
        }

        private static string GetTaskStateText(ProgressTaskState state)
        {
            switch (state)
            {
                case ProgressTaskState.Completed: return "Hoàn thành";
                case ProgressTaskState.InProgress: return "Đang thực hiện";
                case ProgressTaskState.Overdue: return "Quá hạn";
                default: return "Chưa bắt đầu";
            }
        }

        private static int GetTaskStateOrder(int statusCode)
        {
            ProgressTaskState state = (ProgressTaskState)statusCode;
            switch (state)
            {
                case ProgressTaskState.Overdue: return 0;
                case ProgressTaskState.InProgress: return 1;
                case ProgressTaskState.NotStarted: return 2;
                default: return 3;
            }
        }

        private enum ProgressTaskState
        {
            NotStarted = 0,
            InProgress = 1,
            Completed = 2,
            Overdue = 3
        }
    }
}
