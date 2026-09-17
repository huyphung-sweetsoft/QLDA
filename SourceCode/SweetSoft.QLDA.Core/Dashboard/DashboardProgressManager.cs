using System;
using System.Collections.Generic;
using System.Data;
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

            List<TblCongViec> rawProjectTasks = _repository
                .GetTasks(filter, false)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();
            List<TblCongViec> allProjectTasks = DashboardProgressCalculator
                .ExcludeStageRootTasksWithChildren(rawProjectTasks);

            // Lich lam viec la du lieu bo tro. Neu cau hinh lich cu chua
            // tuong thich, Dashboard van render va Calculator fallback ve ngay lich.
            DashboardWorkingCalendar workingCalendar =
                TryCreateWorkingCalendar(projects, today);

            // Du lieu lich su co the co ban ghi trung Id; khong de ToDictionary
            // lam hong toan bo Dashboard.
            Dictionary<Guid, TblDoUuTien> priorities = _repository
                .GetPriorities()
                .GroupBy(x => x.IdDoUuTien)
                .ToDictionary(x => x.Key, x => x.First());

            List<ProjectScheduleStatistic> projectStatistics =
                BuildProjectScheduleStatistics(
                    projects,
                    allProjectTasks,
                    today,
                    workingCalendar);

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
                CurrentStage = isSingleProject
                    ? GetCurrentProjectStage(
                        projects.FirstOrDefault(),
                        rawProjectTasks,
                        today)
                    : null,
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
                        Progress = DashboardProgressCalculator.GetTaskActualProgress(
                            task),
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

        private DashboardWorkingCalendar TryCreateWorkingCalendar(
            List<TblDuAn> projects,
            DateTime calculationDate)
        {
            try
            {
                return DashboardWorkingCalendarFactory.CreateForActiveProjects(
                    _repository,
                    projects,
                    calculationDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "DashboardProgress: cannot create working calendar. {0}",
                    ex);
                // Work-calendar is optional; Calculator accepts null and
                // falls back to calendar-day progress.
                return null;
            }
        }

        private static List<ProjectScheduleStatistic>
            BuildProjectScheduleStatistics(
                List<TblDuAn> projects,
                List<TblCongViec> tasks,
                DateTime today,
                DashboardWorkingCalendar workingCalendar)
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
                        today,
                        workingCalendar);
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
                    Progress = DashboardProgressCalculator.GetTaskActualProgress(
                        task),
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

        /// <summary>
        /// Lấy giai đoạn chưa hoàn thành đang bao phủ ngày hiện tại; trạng thái
        /// của công việc gốc được dùng khi dữ liệu giai đoạn chưa có ngày thực tế.
        /// Nếu không có giai đoạn đang chạy thì chọn giai đoạn chưa hoàn thành
        /// đầu tiên theo thứ tự.
        /// </summary>
        private static ProjectStageInfo GetCurrentProjectStage(
            TblDuAn project,
            IEnumerable<TblCongViec> projectTasks,
            DateTime calculationDate)
        {
            try
            {
                return GetCurrentProjectStageCore(
                    project,
                    projectTasks,
                    calculationDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "DashboardProgress: cannot read current stage. {0}",
                    ex);
                // Current stage is supplementary information. Invalid legacy
                // stage data must not make the Progress dashboard fail.
                return null;
            }
        }

        private static ProjectStageInfo GetCurrentProjectStageCore(
            TblDuAn project,
            IEnumerable<TblCongViec> projectTasks,
            DateTime calculationDate)
        {
            if (project == null
                || DashboardProgressCalculator.IsProjectCompleted(project))
            {
                return null;
            }

            DataTable stageTable = GiaiDoanDuAnManager.Instance.GetByIdDuAn(
                project.IdDuAn);
            if (stageTable == null || stageTable.Rows.Count == 0)
            {
                return null;
            }

            Dictionary<Guid, TblCongViec> rootTaskByStageId = (
                    projectTasks ?? Enumerable.Empty<TblCongViec>())
                .Where(task =>
                    task.IdDuAn == project.IdDuAn
                    && task.IdGiaiDoanDuAn.HasValue
                    && !task.IdCongViecCha.HasValue)
                .GroupBy(task => task.IdGiaiDoanDuAn.Value)
                .ToDictionary(group => group.Key, group => group.First());

            List<ProjectStageInfo> pendingStages = new List<ProjectStageInfo>();
            foreach (DataRow row in stageTable.Rows)
            {
                DateTime? actualCompletionDate = ReadStageDate(
                    row,
                    "NgayHoanThanhThucTe");
                Guid? stageId = ReadStageId(row);
                TblCongViec rootTask;
                bool isRootTaskCompleted = stageId.HasValue
                    && rootTaskByStageId.TryGetValue(
                        stageId.Value,
                        out rootTask)
                    && DashboardProgressCalculator.IsTaskCompleted(rootTask);

                if (actualCompletionDate.HasValue || isRootTaskCompleted)
                {
                    continue;
                }

                pendingStages.Add(new ProjectStageInfo
                {
                    Name = ReadStageName(row),
                    Order = ReadStageOrder(row),
                    StartDate = ReadStageDate(row, "NgayBatDau"),
                    ExpectedEndDate = ReadStageDate(
                        row,
                        "NgayDuKienHoanThanh")
                });
            }

            ProjectStageInfo activeStage = pendingStages
                .Where(stage =>
                    stage.StartDate.HasValue
                    && stage.ExpectedEndDate.HasValue
                    && stage.StartDate.Value.Date <= calculationDate.Date
                    && stage.ExpectedEndDate.Value.Date >= calculationDate.Date)
                .OrderBy(stage => stage.Order)
                .FirstOrDefault();

            if (activeStage != null)
            {
                return activeStage;
            }

            // Nếu không có giai đoạn bao phủ ngày hiện tại, chỉ xem một
            // giai đoạn chưa hoàn thành đã bắt đầu là giai đoạn hiện tại.
            // Không gắn nhãn "hiện tại" cho một giai đoạn còn ở tương lai.
            return pendingStages
                .Where(stage =>
                    stage.StartDate.HasValue
                    && stage.StartDate.Value.Date <= calculationDate.Date)
                .OrderBy(stage => stage.Order)
                .FirstOrDefault();
        }

        private static DateTime? ReadStageDate(
            DataRow row,
            string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)
                || row[columnName] == DBNull.Value)
            {
                return null;
            }

            DateTime value;
            return DateTime.TryParse(
                Convert.ToString(row[columnName]),
                out value)
                ? value.Date
                : (DateTime?)null;
        }

        private static Guid? ReadStageId(DataRow row)
        {
            if (!row.Table.Columns.Contains("IdGiaiDoanDuAn")
                || row["IdGiaiDoanDuAn"] == DBNull.Value)
            {
                return null;
            }

            Guid value;
            return Guid.TryParse(
                Convert.ToString(row["IdGiaiDoanDuAn"]),
                out value)
                ? value
                : (Guid?)null;
        }

        private static int ReadStageOrder(DataRow row)
        {
            if (!row.Table.Columns.Contains("ThuTuGiaiDoan")
                || row["ThuTuGiaiDoan"] == DBNull.Value)
            {
                return int.MaxValue;
            }

            int value;
            return int.TryParse(
                Convert.ToString(row["ThuTuGiaiDoan"]),
                out value)
                ? value
                : int.MaxValue;
        }

        private static string ReadStageName(DataRow row)
        {
            string stageName = row.Table.Columns.Contains("TenGiaiDoan")
                ? Convert.ToString(row["TenGiaiDoan"])
                : string.Empty;

            if (string.IsNullOrWhiteSpace(stageName)
                && row.Table.Columns.Contains("TenGiaiDoanTuyChinh"))
            {
                stageName = Convert.ToString(
                    row["TenGiaiDoanTuyChinh"]);
            }

            return string.IsNullOrWhiteSpace(stageName)
                ? "-"
                : stageName.Trim();
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
