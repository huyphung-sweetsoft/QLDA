using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    /// <summary>
    /// Builds planned weekly resource load from task dates, assignments and the
    /// working calendar configured in the database.
    /// </summary>
    public class DashboardResourceManager : BaseManager
    {
        private const decimal TaskDailyAllocationPercent = 100m;

        private static readonly Lazy<DashboardResourceManager> LazyInstance =
            new Lazy<DashboardResourceManager>(
                () => new DashboardResourceManager());

        private readonly DashboardRepository _repository;

        public DashboardResourceManager(
            IAppContext applicationContext = null,
            DashboardRepository repository = null)
            : base(applicationContext)
        {
            _repository = repository ?? new DashboardRepository();
        }

        public static DashboardResourceManager Instance => LazyInstance.Value;

        public DashboardResourceModel GetResourceDashboard(
            DashboardResourceFilter filter)
        {
            DashboardResourceFilter normalizedFilter = NormalizeFilter(filter);
            DateTime today = DateTime.Today;
            DateTime anchorStart = normalizedFilter.AnchorWeekStart;
            int previousWeekCount = (normalizedFilter.WeekCount - 2) / 2;
            DateTime windowStart = anchorStart.AddDays(-7 * previousWeekCount);
            DateTime lastWeekStart = windowStart
                .AddDays((normalizedFilter.WeekCount - 1) * 7);
            DateTime firstMonthStart = new DateTime(
                windowStart.Year,
                windowStart.Month,
                1);
            DateTime lastWeekCalendarEnd = lastWeekStart.AddDays(6);
            DateTime lastMonthEnd = new DateTime(
                lastWeekCalendarEnd.Year,
                lastWeekCalendarEnd.Month,
                1).AddMonths(1).AddDays(-1);
            DateTime calendarStart = GetMonday(
                new[] { firstMonthStart, anchorStart.AddDays(-35) }.Min());
            DateTime calendarEnd = GetMonday(
                new[] { lastMonthEnd, anchorStart.AddDays(18) }.Max())
                .AddDays(6);
            ResourceWorkCalendar calendar = new ResourceWorkCalendar(
                _repository.GetWorkWeekConfigurations(),
                _repository.GetCalendarExceptions(
                    calendarStart,
                    calendarEnd));
            DateTime anchorEnd = calendar.GetWeekEnd(anchorStart);
            DateTime windowEnd = calendar.GetWeekEnd(lastWeekStart);

            DashboardFilter projectFilter = new DashboardFilter
            {
                ProjectId = normalizedFilter.ProjectId
            };

            List<TblDuAn> projects = _repository
                .GetProjects(projectFilter, false);
            HashSet<Guid> projectIds = new HashSet<Guid>(
                projects.Select(x => x.IdDuAn));
            List<TblCongViec> tasks = _repository
                .GetTasks(projectFilter, false)
                .Where(x => projectIds.Contains(x.IdDuAn))
                .ToList();
            List<TblCongViecNhanVien> assignments = _repository
                .GetTaskAssignments(projectFilter);
            List<TblThanhVienDuAn> members = _repository
                .GetProjectMembers(projectFilter);
            List<AspnetUser> employees = SelectEmployees(
                _repository.GetEmployees(),
                projects,
                assignments,
                members,
                normalizedFilter.ProjectId);
            HashSet<Guid> activeEmployeeIds = new HashSet<Guid>(
                employees.Select(x => x.UserId));
            assignments = assignments
                .Where(x => activeEmployeeIds.Contains(x.IdNhanVien))
                .ToList();

            Dictionary<Guid, TblPhongBan> departments = _repository
                .GetDepartments()
                .ToDictionary(x => x.IdPhongBan);
            Dictionary<Guid, TblChucDanh> jobTitles = _repository
                .GetJobTitles()
                .ToDictionary(x => x.IdChucDanh);
            Dictionary<Guid, TblDuAn> projectById = projects
                .ToDictionary(x => x.IdDuAn);
            Dictionary<Guid, TblCongViec> taskById = tasks
                .ToDictionary(x => x.IdCongViec);

            List<DateTime> windowDays = calendar.GetWorkingDays(
                windowStart,
                windowEnd);
            List<ResourceWeekInfo> weeks = BuildWeeks(
                windowStart,
                normalizedFilter.WeekCount,
                anchorStart,
                today,
                calendar);
            List<ResourceMonthInfo> months = BuildMonthsForWindow(
                windowStart,
                windowEnd);
            List<ResourceEmployeeLoad> employeeLoads = BuildEmployeeLoads(
                employees,
                assignments,
                taskById,
                projectById,
                departments,
                jobTitles,
                windowDays,
                weeks,
                months,
                anchorStart,
                calendar);

            decimal totalCapacity = employeeLoads.Sum(x => x.CapacityDays);
            decimal totalAllocated = employeeLoads.Sum(x => x.AllocatedDays);

            return new DashboardResourceModel
            {
                GeneratedAt = DateTime.Now,
                WindowStart = windowStart,
                WindowEnd = windowEnd,
                AnchorWeekStart = anchorStart,
                AnchorWeekEnd = anchorEnd,
                TotalEmployeeCount = employeeLoads.Count,
                AssignedEmployeeCount = employeeLoads.Count(x =>
                    x.AllocatedDays > 0),
                UnderloadedEmployeeCount = employeeLoads.Count(x =>
                    x.Status == ResourceLoadStatus.Underloaded),
                BalancedEmployeeCount = employeeLoads.Count(x =>
                    x.Status == ResourceLoadStatus.Balanced),
                OverloadedEmployeeCount = employeeLoads.Count(x =>
                    x.Status == ResourceLoadStatus.Overloaded),
                AverageUtilization = totalCapacity == 0
                    ? 0
                    : Math.Round(totalAllocated / totalCapacity * 100m, 1),
                Weeks = weeks,
                Months = months,
                EmployeeLoads = employeeLoads,
                AttentionEmployees = BuildAttentionEmployees(employeeLoads),
                TrendStatistics = BuildTrend(
                    employees,
                    assignments,
                    taskById,
                    anchorStart,
                    today,
                    calendar),
                ProjectAllocations = BuildProjectAllocations(
                    projects,
                    tasks,
                    assignments,
                    anchorStart,
                    anchorEnd,
                    normalizedFilter.ProjectId,
                    calendar)
            };
        }

        public List<TblDuAn> GetProjectsForFilter()
        {
            return _repository.GetProjectsForFilter();
        }

        private static DashboardResourceFilter NormalizeFilter(
            DashboardResourceFilter filter)
        {
            int weekCount = filter == null ? 4 : filter.WeekCount;
            if (weekCount != 2 && weekCount != 4 && weekCount != 6)
            {
                weekCount = 4;
            }

            DateTime anchor = filter == null
                || filter.AnchorWeekStart == DateTime.MinValue
                    ? DateTime.Today
                    : filter.AnchorWeekStart.Date;

            return new DashboardResourceFilter
            {
                ProjectId = filter == null ? null : filter.ProjectId,
                AnchorWeekStart = GetMonday(anchor),
                WeekCount = weekCount
            };
        }

        private static List<AspnetUser> SelectEmployees(
            List<AspnetUser> allEmployees,
            List<TblDuAn> projects,
            List<TblCongViecNhanVien> assignments,
            List<TblThanhVienDuAn> members,
            Guid? projectId)
        {
            if (!projectId.HasValue)
            {
                return allEmployees
                    .OrderBy(GetEmployeeName)
                    .ToList();
            }

            HashSet<Guid> employeeIds = new HashSet<Guid>(
                assignments.Select(x => x.IdNhanVien));
            foreach (TblThanhVienDuAn member in members.Where(x =>
                x.IdNhanVien.HasValue))
            {
                employeeIds.Add(member.IdNhanVien.Value);
            }

            foreach (TblDuAn project in projects.Where(x =>
                x.IdNhanVienQuanLy.HasValue))
            {
                employeeIds.Add(project.IdNhanVienQuanLy.Value);
            }

            return allEmployees
                .Where(x => employeeIds.Contains(x.UserId))
                .OrderBy(GetEmployeeName)
                .ToList();
        }

        private static List<ResourceEmployeeLoad> BuildEmployeeLoads(
            List<AspnetUser> employees,
            List<TblCongViecNhanVien> assignments,
            Dictionary<Guid, TblCongViec> taskById,
            Dictionary<Guid, TblDuAn> projectById,
            Dictionary<Guid, TblPhongBan> departments,
            Dictionary<Guid, TblChucDanh> jobTitles,
            List<DateTime> windowDays,
            List<ResourceWeekInfo> weeks,
            List<ResourceMonthInfo> months,
            DateTime anchorStart,
            ResourceWorkCalendar calendar)
        {
            Dictionary<Guid, List<TblCongViec>> tasksByEmployee = assignments
                .GroupBy(x => x.IdNhanVien)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(x =>
                        {
                            TblCongViec task;
                            taskById.TryGetValue(x.IdCongViec, out task);
                            return task;
                        })
                        .Where(x => x != null)
                        .GroupBy(x => x.IdCongViec)
                        .Select(x => x.First())
                        .ToList());

            List<ResourceEmployeeLoad> result =
                new List<ResourceEmployeeLoad>();

            foreach (AspnetUser employee in employees)
            {
                List<TblCongViec> employeeTasks;
                if (!tasksByEmployee.TryGetValue(
                    employee.UserId,
                    out employeeTasks))
                {
                    employeeTasks = new List<TblCongViec>();
                }

                List<ResourceDailyLoad> dailyLoads = windowDays
                    .Select(day => BuildDailyLoad(
                        day,
                        employeeTasks,
                        projectById,
                        calendar))
                    .ToList();
                List<ResourceWeeklyLoad> weeklyLoads = BuildWeeklyLoads(
                    dailyLoads,
                    weeks);
                ResourceWeeklyLoad anchorLoad = weeklyLoads.First(x =>
                    x.WeekStart == anchorStart);
                List<ResourceMonthlyLoad> monthlyLoads = months
                    .Select(month => BuildMonthlyLoad(
                        month,
                        employeeTasks,
                        projectById,
                        anchorStart,
                        calendar))
                    .ToList();
                decimal allocatedDays = anchorLoad.AllocatedDays;
                decimal averageUtilization = anchorLoad.AllocationPercent;
                decimal maxDailyLoad = anchorLoad.DailyLoads.Count == 0
                    ? 0
                    : anchorLoad.DailyLoads.Max(x => x.AllocationPercent);

                TblPhongBan department = null;
                if (employee.IdPhongBan.HasValue)
                {
                    departments.TryGetValue(
                        employee.IdPhongBan.Value,
                        out department);
                }

                TblChucDanh jobTitle = null;
                if (employee.IdChucDanh.HasValue)
                {
                    jobTitles.TryGetValue(
                        employee.IdChucDanh.Value,
                        out jobTitle);
                }

                result.Add(new ResourceEmployeeLoad
                {
                    EmployeeId = employee.UserId,
                    DisplayName = GetEmployeeName(employee),
                    UserName = employee.UserName,
                    DepartmentName = department == null
                        ? string.Empty
                        : department.TenPhongBan,
                    JobTitleName = jobTitle == null
                        ? string.Empty
                        : jobTitle.TenChucDanh,
                    AllocatedDays = Math.Round(allocatedDays, 1),
                    CapacityDays = anchorLoad.CapacityDays,
                    AverageUtilization = averageUtilization,
                    MaxDailyLoad = maxDailyLoad,
                    ActiveTaskCount = anchorLoad.Tasks.Count,
                    OverloadDayCount = anchorLoad.OverlapDayCount,
                    OverAllocatedDays = anchorLoad.OverAllocatedDays,
                    Status = anchorLoad.Status,
                    DailyLoads = dailyLoads,
                    WeeklyLoads = weeklyLoads,
                    MonthlyLoads = monthlyLoads
                });
            }

            return result
                .OrderByDescending(x => x.Status == ResourceLoadStatus.Overloaded)
                .ThenByDescending(x => x.AverageUtilization)
                .ThenBy(x => x.DisplayName)
                .ToList();
        }

        private static ResourceDailyLoad BuildDailyLoad(
            DateTime day,
            List<TblCongViec> tasks,
            Dictionary<Guid, TblDuAn> projectById,
            ResourceWorkCalendar calendar)
        {
            List<TblCongViec> activeTasks = tasks
                .Where(x => IsTaskActiveOn(x, day, calendar))
                .ToList();
            ResourceDailyLoad load = new ResourceDailyLoad
            {
                Date = day,
                AllocationPercent = activeTasks.Count
                    * TaskDailyAllocationPercent
            };

            foreach (TblCongViec task in activeTasks)
            {
                TblDuAn project;
                projectById.TryGetValue(task.IdDuAn, out project);
                load.Tasks.Add(new ResourceTaskAllocation
                {
                    TaskId = task.IdCongViec,
                    TaskCode = task.MaCongViec,
                    TaskName = task.TenCongViec,
                    ProjectId = task.IdDuAn,
                    ProjectCode = project == null
                        ? string.Empty
                        : project.MaDuAn,
                    ProjectName = project == null
                        ? string.Empty
                        : project.TenDuAn,
                    StartDate = task.NgayBatDau,
                    EndDate = GetTaskEnd(task),
                    AllocationPercent = TaskDailyAllocationPercent
                });
            }

            return load;
        }

        private static List<ResourceWeeklyLoad> BuildWeeklyLoads(
            List<ResourceDailyLoad> dailyLoads,
            List<ResourceWeekInfo> weeks)
        {
            List<ResourceWeeklyLoad> result =
                new List<ResourceWeeklyLoad>();

            foreach (ResourceWeekInfo week in weeks)
            {
                List<ResourceDailyLoad> weekDays = dailyLoads
                    .Where(x => x.Date >= week.StartDate
                        && x.Date <= week.EndDate)
                    .OrderBy(x => x.Date)
                    .ToList();
                decimal allocatedDays = weekDays.Sum(x =>
                    x.AllocationPercent / 100m);
                decimal capacityDays = weekDays.Count;
                decimal allocationPercent = capacityDays == 0
                    ? 0
                    : Math.Round(
                        allocatedDays / capacityDays * 100m,
                        1);

                var taskDays = weekDays
                    .SelectMany(day => day.Tasks.Select(task => new
                    {
                        Date = day.Date,
                        Task = task
                    }))
                    .ToList();
                List<ResourceWeeklyTaskAllocation> weeklyTasks = taskDays
                    .GroupBy(x => x.Task.TaskId)
                    .Select(group =>
                    {
                        ResourceTaskAllocation task = group.First().Task;
                        List<DateTime> activeDates = group
                            .Select(x => x.Date)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();
                        decimal taskDaysCount = activeDates.Count;
                        return new ResourceWeeklyTaskAllocation
                        {
                            TaskId = task.TaskId,
                            TaskCode = task.TaskCode,
                            TaskName = task.TaskName,
                            ProjectId = task.ProjectId,
                            ProjectCode = task.ProjectCode,
                            ProjectName = task.ProjectName,
                            StartDate = task.StartDate,
                            EndDate = task.EndDate,
                            AllocatedDays = taskDaysCount,
                            AllocationPercent = capacityDays == 0
                                ? 0
                                : Math.Round(
                                    taskDaysCount / capacityDays * 100m,
                                    1),
                            ActiveDates = activeDates
                        };
                    })
                    .OrderBy(x => x.ProjectCode)
                    .ThenBy(x => x.TaskCode)
                    .ToList();
                List<ResourceWeeklyProjectAllocation> weeklyProjects =
                    weeklyTasks
                        .GroupBy(x => x.ProjectId)
                        .Select(group =>
                        {
                            ResourceWeeklyTaskAllocation task = group.First();
                            decimal projectDays = group.Sum(x =>
                                x.AllocatedDays);
                            return new ResourceWeeklyProjectAllocation
                            {
                                ProjectId = group.Key,
                                ProjectCode = task.ProjectCode,
                                ProjectName = task.ProjectName,
                                TaskCount = group.Count(),
                                AllocatedDays = projectDays,
                                AllocationPercent = capacityDays == 0
                                    ? 0
                                    : Math.Round(
                                        projectDays / capacityDays * 100m,
                                        1)
                            };
                        })
                        .OrderByDescending(x => x.AllocatedDays)
                        .ThenBy(x => x.ProjectCode)
                        .ToList();

                result.Add(new ResourceWeeklyLoad
                {
                    WeekStart = week.StartDate,
                    WeekEnd = week.EndDate,
                    Label = week.Label,
                    AllocatedDays = allocatedDays,
                    CapacityDays = capacityDays,
                    AllocationPercent = allocationPercent,
                    OverAllocatedDays = Math.Max(
                        0m,
                        allocatedDays - capacityDays),
                    OverlapDayCount = weekDays.Count(x =>
                        x.AllocationPercent > 100m),
                    Status = GetStatus(allocationPercent),
                    Projects = weeklyProjects,
                    Tasks = weeklyTasks,
                    DailyLoads = weekDays
                });
            }

            return result;
        }

        private static ResourceMonthlyLoad BuildMonthlyLoad(
            ResourceMonthInfo month,
            List<TblCongViec> employeeTasks,
            Dictionary<Guid, TblDuAn> projectById,
            DateTime anchorStart,
            ResourceWorkCalendar calendar)
        {
            List<ResourceWeekInfo> monthWeeks = BuildMonthWeeks(
                month.StartDate,
                month.EndDate,
                anchorStart,
                calendar);
            List<DateTime> calculationDays = calendar.GetWorkingDays(
                monthWeeks.First().StartDate,
                monthWeeks.Last().EndDate);
            List<ResourceDailyLoad> dailyLoads = calculationDays
                .Select(day => BuildDailyLoad(
                    day,
                    employeeTasks,
                    projectById,
                    calendar))
                .ToList();
            List<ResourceWeeklyLoad> weeklyLoads = BuildWeeklyLoads(
                dailyLoads,
                monthWeeks);
            decimal allocatedDays = dailyLoads
                .Where(x => x.Date >= month.StartDate
                    && x.Date <= month.EndDate)
                .Sum(x => x.AllocationPercent / 100m);
            decimal capacityDays = calendar.GetWorkingDays(
                month.StartDate,
                month.EndDate).Count;
            decimal utilization = capacityDays == 0
                ? 0
                : Math.Round(
                    allocatedDays / capacityDays * 100m,
                    1);

            return new ResourceMonthlyLoad
            {
                MonthStart = month.StartDate,
                MonthEnd = month.EndDate,
                Label = month.Label,
                AllocatedDays = Math.Round(allocatedDays, 1),
                CapacityDays = capacityDays,
                AverageUtilization = utilization,
                OverloadWeekCount = weeklyLoads.Count(x =>
                    x.Status == ResourceLoadStatus.Overloaded),
                Status = GetStatus(utilization),
                WeeklyLoads = weeklyLoads
            };
        }

        private static List<ResourceWeekInfo> BuildWeeks(
            DateTime windowStart,
            int weekCount,
            DateTime anchorStart,
            DateTime today,
            ResourceWorkCalendar calendar)
        {
            List<ResourceWeekInfo> result = new List<ResourceWeekInfo>();

            for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
            {
                DateTime start = windowStart.AddDays(weekIndex * 7);
                ResourceWeekInfo week = new ResourceWeekInfo
                {
                    StartDate = start,
                    EndDate = calendar.GetWeekEnd(start),
                    Label = "Tuần " + GetIsoWeekNumber(start),
                    IsAnchorWeek = start == anchorStart
                };

                foreach (DateTime date in calendar.GetWorkingDays(
                    start,
                    start.AddDays(6)))
                {
                    week.Days.Add(new ResourceDayInfo
                    {
                        Date = date,
                        DayLabel = GetVietnameseDayLabel(date),
                        IsToday = date == today
                    });
                }

                result.Add(week);
            }

            return result;
        }

        private static List<ResourceWeekInfo> BuildMonthWeeks(
            DateTime monthStart,
            DateTime monthEnd,
            DateTime anchorStart,
            ResourceWorkCalendar calendar)
        {
            List<ResourceWeekInfo> result =
                new List<ResourceWeekInfo>();
            DateTime weekStart = GetMonday(monthStart);

            while (weekStart <= monthEnd)
            {
                DateTime ownerMonth = calendar.GetWeekOwnerMonth(weekStart);
                if (ownerMonth.Month == monthStart.Month
                    && ownerMonth.Year == monthStart.Year)
                {
                    result.Add(new ResourceWeekInfo
                    {
                        StartDate = weekStart,
                        EndDate = calendar.GetWeekEnd(weekStart),
                        Label = "Tuần " + GetIsoWeekNumber(weekStart),
                        IsAnchorWeek = weekStart == anchorStart
                    });
                }

                weekStart = weekStart.AddDays(7);
            }

            return result;
        }

        private static List<ResourceMonthInfo> BuildMonthsForWindow(
            DateTime windowStart,
            DateTime windowEnd)
        {
            List<ResourceMonthInfo> result =
                new List<ResourceMonthInfo>();
            DateTime monthStart = new DateTime(
                windowStart.Year,
                windowStart.Month,
                1);
            DateTime lastMonthStart = new DateTime(
                windowEnd.Year,
                windowEnd.Month,
                1);

            while (monthStart <= lastMonthStart)
            {
                DateTime monthEnd = monthStart
                    .AddMonths(1)
                    .AddDays(-1);
                result.Add(new ResourceMonthInfo
                {
                    StartDate = monthStart,
                    EndDate = monthEnd,
                    Label = "Tháng " + monthStart.Month
                        + "/" + monthStart.Year
                });
                monthStart = monthStart.AddMonths(1);
            }

            return result;
        }

        private static List<ResourceEmployeeLoad> BuildAttentionEmployees(
            List<ResourceEmployeeLoad> employeeLoads)
        {
            return employeeLoads
                .Where(x => x.Status == ResourceLoadStatus.Overloaded
                    || x.Status == ResourceLoadStatus.Underloaded)
                .OrderByDescending(x =>
                    x.Status == ResourceLoadStatus.Overloaded)
                .ThenBy(x => x.AllocatedDays > 0 ? 1 : 0)
                .ThenByDescending(x => x.MaxDailyLoad)
                .ThenBy(x => x.DisplayName)
                .Take(10)
                .ToList();
        }

        private static List<ResourceTrendStatistic> BuildTrend(
            List<AspnetUser> employees,
            List<TblCongViecNhanVien> assignments,
            Dictionary<Guid, TblCongViec> taskById,
            DateTime anchorStart,
            DateTime today,
            ResourceWorkCalendar calendar)
        {
            List<TblCongViec> assignedTasks = assignments
                .Select(x =>
                {
                    TblCongViec task;
                    taskById.TryGetValue(x.IdCongViec, out task);
                    return task;
                })
                .Where(x => x != null)
                .ToList();
            List<ResourceTrendStatistic> result =
                new List<ResourceTrendStatistic>();
            DateTime trendStart = anchorStart.AddDays(-35);

            for (int index = 0; index < 8; index++)
            {
                DateTime weekStart = trendStart.AddDays(index * 7);
                DateTime weekEnd = calendar.GetWeekEnd(weekStart);
                List<DateTime> workingDays = calendar.GetWorkingDays(
                    weekStart,
                    weekStart.AddDays(6));
                decimal capacity = employees.Count * workingDays.Count;
                decimal allocatedDays = 0;

                foreach (DateTime day in workingDays)
                {
                    allocatedDays += assignedTasks.Count(x =>
                        IsTaskActiveOn(x, day, calendar));
                }

                result.Add(new ResourceTrendStatistic
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    Label = "T" + GetIsoWeekNumber(weekStart),
                    Utilization = capacity == 0
                        ? 0
                        : Math.Round(allocatedDays / capacity * 100m, 1),
                    IsForecast = weekStart > GetMonday(today)
                });
            }

            return result;
        }

        private static List<ResourceProjectAllocation> BuildProjectAllocations(
            List<TblDuAn> projects,
            List<TblCongViec> tasks,
            List<TblCongViecNhanVien> assignments,
            DateTime anchorStart,
            DateTime anchorEnd,
            Guid? selectedProjectId,
            ResourceWorkCalendar calendar)
        {
            Dictionary<Guid, TblCongViec> taskById = tasks
                .ToDictionary(x => x.IdCongViec);
            List<ResourceProjectAllocation> result =
                new List<ResourceProjectAllocation>();
            List<DateTime> anchorWorkingDays = calendar.GetWorkingDays(
                anchorStart,
                anchorEnd);

            foreach (TblDuAn project in projects)
            {
                List<TblCongViecNhanVien> projectAssignments = assignments
                    .Where(x => taskById.ContainsKey(x.IdCongViec)
                        && taskById[x.IdCongViec].IdDuAn == project.IdDuAn)
                    .ToList();
                HashSet<Guid> resourceIds = new HashSet<Guid>();
                decimal allocatedDays = 0;

                foreach (TblCongViecNhanVien assignment in projectAssignments)
                {
                    TblCongViec task = taskById[assignment.IdCongViec];
                    int activeDayCount = anchorWorkingDays
                        .Count(day => IsTaskActiveOn(task, day, calendar));
                    if (activeDayCount == 0)
                    {
                        continue;
                    }

                    resourceIds.Add(assignment.IdNhanVien);
                    allocatedDays += activeDayCount;
                }

                if (!selectedProjectId.HasValue && allocatedDays == 0)
                {
                    continue;
                }

                decimal capacity = resourceIds.Count
                    * anchorWorkingDays.Count;
                decimal utilization = capacity == 0
                    ? 0
                    : Math.Round(allocatedDays / capacity * 100m, 1);
                result.Add(new ResourceProjectAllocation
                {
                    ProjectId = project.IdDuAn,
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    ResourceCount = resourceIds.Count,
                    AllocatedDays = allocatedDays,
                    CapacityDays = capacity,
                    Utilization = utilization,
                    Status = GetStatus(utilization)
                });
            }

            return result
                .OrderByDescending(x => x.Status == ResourceLoadStatus.Overloaded)
                .ThenByDescending(x => x.Utilization)
                .ThenBy(x => x.ProjectCode)
                .ToList();
        }

        private static ResourceLoadStatus GetStatus(decimal utilization)
        {
            if (utilization > 100m)
            {
                return ResourceLoadStatus.Overloaded;
            }

            return utilization >= 80m
                ? ResourceLoadStatus.Balanced
                : ResourceLoadStatus.Underloaded;
        }

        private static bool IsTaskActiveOn(
            TblCongViec task,
            DateTime day,
            ResourceWorkCalendar calendar)
        {
            if (!task.NgayBatDau.HasValue || !calendar.IsWorkingDay(day))
            {
                return false;
            }

            DateTime start = task.NgayBatDau.Value.Date;
            DateTime? end = GetTaskEnd(task);
            if (!end.HasValue)
            {
                end = start;
            }

            return day.Date >= start && day.Date <= end.Value.Date;
        }

        private static DateTime? GetTaskEnd(TblCongViec task)
        {
            if (task.NgayHoanThanhThucTe.HasValue)
            {
                return task.NgayHoanThanhThucTe.Value.Date;
            }

            if (task.NgayKetThuc.HasValue)
            {
                return task.NgayKetThuc.Value.Date;
            }

            return task.NgayBatDau.HasValue
                ? task.NgayBatDau.Value.Date
                : (DateTime?)null;
        }

        private static string GetVietnameseDayLabel(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return "T2";
                case DayOfWeek.Tuesday: return "T3";
                case DayOfWeek.Wednesday: return "T4";
                case DayOfWeek.Thursday: return "T5";
                case DayOfWeek.Friday: return "T6";
                case DayOfWeek.Saturday: return "T7";
                default: return "CN";
            }
        }

        private static DateTime GetMonday(DateTime date)
        {
            int difference = (7 + (int)date.DayOfWeek
                - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-difference);
        }

        private static int GetIsoWeekNumber(DateTime date)
        {
            System.Globalization.CultureInfo culture =
                System.Globalization.CultureInfo.InvariantCulture;
            return culture.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        private static string GetEmployeeName(AspnetUser employee)
        {
            return string.IsNullOrWhiteSpace(employee.DisplayName)
                ? employee.UserName
                : employee.DisplayName;
        }
    }

    internal sealed class ResourceWorkCalendar
    {
        private readonly Dictionary<DayOfWeek, bool> _weeklyPattern;
        private readonly List<TblLichNgoaiLe> _exceptions;

        public ResourceWorkCalendar(
            IEnumerable<TblCauHinhTuanLamViec> configurations,
            IEnumerable<TblLichNgoaiLe> exceptions)
        {
            _weeklyPattern = (configurations
                    ?? Enumerable.Empty<TblCauHinhTuanLamViec>())
                .Where(x => x.NgayTrongTuan <= 6)
                .GroupBy(x => (DayOfWeek)x.NgayTrongTuan)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().LaNgayLamViec);
            _exceptions = (exceptions
                    ?? Enumerable.Empty<TblLichNgoaiLe>())
                .Where(x => !x.DaXoa)
                .OrderByDescending(GetEffectiveDate)
                .ThenByDescending(x => x.IdNgoaiLe)
                .ToList();
        }

        public bool IsWorkingDay(DateTime date)
        {
            DateTime day = date.Date;
            TblLichNgoaiLe exception = _exceptions.FirstOrDefault(x =>
                x.NgayBatDau.Date <= day
                && x.NgayKetThuc.Date >= day);
            if (exception != null)
            {
                return exception.LaNgayLamViec;
            }

            bool configuredValue;
            if (_weeklyPattern.TryGetValue(
                day.DayOfWeek,
                out configuredValue))
            {
                return configuredValue;
            }

            return day.DayOfWeek != DayOfWeek.Saturday
                && day.DayOfWeek != DayOfWeek.Sunday;
        }

        public List<DateTime> GetWorkingDays(
            DateTime start,
            DateTime end)
        {
            List<DateTime> result = new List<DateTime>();
            for (DateTime date = start.Date;
                date <= end.Date;
                date = date.AddDays(1))
            {
                if (IsWorkingDay(date))
                {
                    result.Add(date);
                }
            }

            return result;
        }

        public DateTime GetWeekEnd(DateTime weekStart)
        {
            List<DateTime> scheduledDays = GetScheduledWeekDays(weekStart);
            return scheduledDays.Count == 0
                ? weekStart.Date.AddDays(6)
                : scheduledDays.Last();
        }

        public DateTime GetWeekOwnerMonth(DateTime weekStart)
        {
            List<DateTime> scheduledDays = GetScheduledWeekDays(weekStart);
            DateTime tieBreaker = weekStart.Date.AddDays(2);
            if (scheduledDays.Count == 0)
            {
                return new DateTime(
                    tieBreaker.Year,
                    tieBreaker.Month,
                    1);
            }

            var groups = scheduledDays
                .GroupBy(x => new { x.Year, x.Month })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Month,
                    Count = group.Count(),
                    ContainsTieBreaker = group.Any(x =>
                        x.Year == tieBreaker.Year
                        && x.Month == tieBreaker.Month)
                })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.ContainsTieBreaker)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Month)
                .First();

            return new DateTime(groups.Year, groups.Month, 1);
        }

        private List<DateTime> GetScheduledWeekDays(DateTime weekStart)
        {
            List<DateTime> result = new List<DateTime>();
            for (int offset = 0; offset < 7; offset++)
            {
                DateTime date = weekStart.Date.AddDays(offset);
                bool configuredValue;
                bool isWorking = _weeklyPattern.TryGetValue(
                    date.DayOfWeek,
                    out configuredValue)
                        ? configuredValue
                        : date.DayOfWeek != DayOfWeek.Saturday
                            && date.DayOfWeek != DayOfWeek.Sunday;
                if (isWorking)
                {
                    result.Add(date);
                }
            }

            return result;
        }

        private static DateTime GetEffectiveDate(TblLichNgoaiLe exception)
        {
            return exception.NgayCapNhat
                ?? exception.NgayTao
                ?? DateTime.MinValue;
        }
    }
}
