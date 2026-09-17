using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Dashboard;
using SweetSoft.QLDA.Core.ResourceTexts;

namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlDashboardResource : BaseAdminUserControl
    {
        private const string AllProjectsValue = "__all_projects__";

        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-style.css?v=4")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-resource.js?v=3")
                };

                return new RegisterCSSAndJS(
                    "cpHeadVendor",
                    "cpVendorScript",
                    cssLinks,
                    jsLinks);
            }
        }

        protected DashboardResourceModel Model { get; private set; }

        protected string TrendChartData { get; private set; }

        protected string ResourceDetailData { get; private set; }

        protected string DashboardTextsJson { get; private set; }

        private DateTime AnchorWeekStart
        {
            get
            {
                object value = ViewState["ResourceAnchorWeekStart"];
                return value == null
                    ? GetMonday(DateTime.Today)
                    : (DateTime)value;
            }
            set { ViewState["ResourceAnchorWeekStart"] = value.Date; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnPreviousWeek.ToolTip = GetResourceText(
                BackEndResourceKeys.DASHBOARD_PREVIOUS_WEEK);
            btnCurrentWeek.Text = GetResourceText(
                BackEndResourceKeys.DASHBOARD_CURRENT_WEEK);
            btnNextWeek.ToolTip = GetResourceText(
                BackEndResourceKeys.DASHBOARD_NEXT_WEEK);

            if (!IsPostBack)
            {
                AnchorWeekStart = GetMonday(DateTime.Today);
                btnApplyResourceFilter.Text =
                    GetResourceText(BackEndResourceKeys.APPLY);
                LoadProjectFilter();
                LoadWeekCountFilter();
                InitDashboard(BuildResourceFilter());
            }
        }

        protected void btnApplyResourceFilter_Click(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildResourceFilter());
        }

        protected void btnPreviousWeek_Click(object sender, EventArgs e)
        {
            AnchorWeekStart = AnchorWeekStart.AddDays(-7);
            InitDashboard(BuildResourceFilter());
        }

        protected void btnCurrentWeek_Click(object sender, EventArgs e)
        {
            AnchorWeekStart = GetMonday(DateTime.Today);
            InitDashboard(BuildResourceFilter());
        }

        protected void btnNextWeek_Click(object sender, EventArgs e)
        {
            AnchorWeekStart = AnchorWeekStart.AddDays(7);
            InitDashboard(BuildResourceFilter());
        }

        protected string GetStatusText(ResourceLoadStatus status)
        {
            switch (status)
            {
                case ResourceLoadStatus.Underloaded:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_UNDERLOADED);
                case ResourceLoadStatus.Balanced:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_BALANCED_LOAD);
                case ResourceLoadStatus.Overloaded:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_OVERLOADED);
                default: return "-";
            }
        }

        protected string GetStatusBadgeCss(ResourceLoadStatus status)
        {
            switch (status)
            {
                case ResourceLoadStatus.Underloaded:
                    return "bg-success-subtle text-success";
                case ResourceLoadStatus.Balanced:
                    return "bg-warning-subtle text-warning";
                case ResourceLoadStatus.Overloaded:
                    return "bg-danger-subtle text-danger";
                default:
                    return "bg-secondary-subtle text-secondary";
            }
        }

        protected string GetMonthlyStatusText(ResourceMonthlyLoad load)
        {
            if (load.AverageUtilization > 100m)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_MONTH_OVERLOADED);
            }

            if (load.OverloadWeekCount > 0)
            {
                return string.Format(
                    GetResourceText(
                        BackEndResourceKeys.DASHBOARD_OVERLOADED_WEEK_COUNT),
                    load.OverloadWeekCount);
            }

            return GetStatusText(load.Status);
        }

        protected string GetMonthlyStatusBadgeCss(
            ResourceMonthlyLoad load)
        {
            return load.AverageUtilization > 100m
                || load.OverloadWeekCount > 0
                    ? "bg-danger-subtle text-danger"
                    : GetStatusBadgeCss(load.Status);
        }

        protected string GetMonthlySummaryCss(ResourceMonthlyLoad load)
        {
            return load.OverloadWeekCount > 0
                ? "resource-month-has-overload"
                : string.Empty;
        }

        protected string GetMonthlyStatusTitle(ResourceMonthlyLoad load)
        {
            List<string> overloadWeeks = load.WeeklyLoads
                .Where(x => x.Status == ResourceLoadStatus.Overloaded)
                .Select(x => x.Label + " "
                    + x.AllocationPercent.ToString("0.#") + "%")
                .ToList();

            return overloadWeeks.Count == 0
                ? GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_OVERLOADED_WEEKS)
                : string.Format(
                    GetResourceText(
                        BackEndResourceKeys.DASHBOARD_OVERLOADED_WEEKS),
                    string.Join(", ", overloadWeeks));
        }

        protected string GetHeatmapCss(decimal allocationPercent)
        {
            if (allocationPercent <= 0)
            {
                return "resource-load-low";
            }

            if (allocationPercent < 80)
            {
                return "resource-load-low";
            }

            if (allocationPercent <= 100)
            {
                return "resource-load-balanced";
            }

            return "resource-load-over";
        }

        protected string GetStatusLoadCss(ResourceLoadStatus status)
        {
            switch (status)
            {
                case ResourceLoadStatus.Underloaded: return "resource-load-low";
                case ResourceLoadStatus.Balanced: return "resource-load-balanced";
                case ResourceLoadStatus.Overloaded: return "resource-load-over";
                default: return "resource-load-low";
            }
        }

        protected string GetCellText(decimal allocationPercent)
        {
            return allocationPercent <= 0
                ? "—"
                : allocationPercent.ToString("0", CultureInfo.InvariantCulture)
                    + "%";
        }

        protected string GetAllocatedDaysText(ResourceWeeklyLoad load)
        {
            return load.AllocationPercent <= 0
                ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_SCHEDULE)
                : string.Format(
                    GetResourceText(
                        BackEndResourceKeys.DASHBOARD_ALLOCATED_CAPACITY),
                    load.AllocatedDays.ToString("0.#"),
                    load.CapacityDays.ToString("0"));
        }

        protected string GetEmployeeMeta(ResourceEmployeeLoad employee)
        {
            List<string> values = new List<string>();
            if (!string.IsNullOrWhiteSpace(employee.JobTitleName))
            {
                values.Add(employee.JobTitleName);
            }

            if (!string.IsNullOrWhiteSpace(employee.DepartmentName))
            {
                values.Add(employee.DepartmentName);
            }

            return values.Count == 0
                ? employee.UserName
                : string.Join(" · ", values);
        }

        protected string GetAttentionText(ResourceEmployeeLoad employee)
        {
            if (employee.Status == ResourceLoadStatus.Overloaded)
            {
                string text = string.Format(
                    GetResourceText(
                        BackEndResourceKeys.DASHBOARD_OVERLOAD_DETAIL),
                    employee.AllocatedDays.ToString("0.#") + "/"
                        + employee.CapacityDays.ToString("0"),
                    employee.OverAllocatedDays.ToString("0.#"));
                if (employee.OverloadDayCount > 0)
                {
                    text += string.Format(
                        GetResourceText(
                            BackEndResourceKeys.DASHBOARD_OVERLAP_DAYS),
                        employee.OverloadDayCount);
                }

                return text;
            }

            if (employee.AllocatedDays <= 0)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_FOCUS_WEEK_TASKS);
            }

            return string.Format(
                GetResourceText(
                    BackEndResourceKeys.DASHBOARD_LOW_WEEKLY_UTILIZATION),
                employee.AverageUtilization.ToString("0.#"));
        }

        protected string GetProjectDetailUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectDetail);
        }

        protected string GetProjectTasksUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectTasks);
        }

        protected string GetEmployeeDetailUrl(Guid employeeId)
        {
            if (employeeId == Guid.Empty)
            {
                return string.Empty;
            }

            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ViewDetailEmp(employeeId));
        }

        private string GetProjectUrl(
            Guid projectId,
            Func<Guid, string> routeBuilder)
        {
            if (projectId == Guid.Empty)
            {
                return string.Empty;
            }

            return CURRENT_PAGE.GetRelativeClientPath(
                routeBuilder(projectId));
        }

        private void InitDashboard(DashboardResourceFilter filter)
        {
            Model = DashboardResourceManager.Instance
                .GetResourceDashboard(filter);

            TrendChartData = ToSafeJson(
                Model.TrendStatistics.Select(x => new
                {
                    label = x.Label,
                    start = x.WeekStart.ToString("dd/MM"),
                    end = x.WeekEnd.ToString("dd/MM"),
                    utilization = x.Utilization,
                    forecast = x.IsForecast
                }));

            ResourceDetailData = ToSafeJson(
                Model.EmployeeLoads.Select(employee => new
                {
                    id = employee.EmployeeId,
                    name = employee.DisplayName,
                    userName = employee.UserName,
                    department = employee.DepartmentName,
                    jobTitle = employee.JobTitleName,
                    weeks = employee.WeeklyLoads.Select(week => new
                    {
                        start = week.WeekStart.ToString("yyyy-MM-dd"),
                        end = week.WeekEnd.ToString("yyyy-MM-dd"),
                        label = week.Label,
                        displayRange = week.WeekStart.ToString("dd/MM")
                            + "–" + week.WeekStart.AddDays(6)
                                .ToString("dd/MM/yyyy"),
                        allocation = week.AllocationPercent,
                        allocatedDays = week.AllocatedDays,
                        capacityDays = week.CapacityDays,
                        overAllocatedDays = week.OverAllocatedDays,
                        overlapDayCount = week.OverlapDayCount,
                        hasHoliday = week.DailyLoads.Any(day => day.IsHoliday),
                        workingDayCount = week.DailyLoads.Count(
                            day => day.IsWorkingDay),
                        projects = week.Projects.Select(project => new
                        {
                            detailUrl = GetProjectDetailUrl(project.ProjectId),
                            code = project.ProjectCode,
                            name = project.ProjectName,
                            taskCount = project.TaskCount,
                            allocatedDays = project.AllocatedDays,
                            allocation = project.AllocationPercent
                        }),
                        tasks = week.Tasks.Select(task => new
                        {
                            tasksUrl = GetProjectTasksUrl(task.ProjectId),
                            code = task.TaskCode,
                            name = task.TaskName,
                            projectCode = task.ProjectCode,
                            projectName = task.ProjectName,
                            allocation = task.AllocationPercent,
                            allocatedDays = task.AllocatedDays,
                            activeDates = task.ActiveDates.Select(date =>
                                GetDayLabel(date) + " "
                                    + date.ToString("dd/MM")),
                            start = task.StartDate.HasValue
                                ? task.StartDate.Value.ToString("dd/MM/yyyy")
                                : "-",
                            end = task.EndDate.HasValue
                                ? task.EndDate.Value.ToString("dd/MM/yyyy")
                                : "-"
                        }),
                        days = week.DailyLoads.Select(day => new
                        {
                            date = day.Date.ToString("yyyy-MM-dd"),
                            displayDate = GetDayLabel(day.Date)
                                + " " + day.Date.ToString("dd/MM"),
                            isWorkingDay = day.IsWorkingDay,
                            isHoliday = day.IsHoliday,
                            holidayName = day.HolidayName,
                            allocation = day.AllocationPercent,
                            taskCount = day.Tasks.Count,
                            tasks = day.Tasks.Select(task => new
                            {
                                tasksUrl = GetProjectTasksUrl(task.ProjectId),
                                code = task.TaskCode,
                                name = task.TaskName,
                                projectCode = task.ProjectCode,
                                projectName = task.ProjectName,
                                allocation = task.AllocationPercent
                            })
                        })
                    })
                }));

            DashboardTextsJson = ToSafeJson(new
            {
                pastAssignment = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAST_ASSIGNMENT),
                futurePlan = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_FUTURE_PLAN),
                utilization = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_UTILIZATION),
                threshold100 = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_THRESHOLD_100),
                dayFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_DAY_COUNT),
                project = GetResourceText(BackEndResourceKeys.PROJECT),
                task = GetResourceText(BackEndResourceKeys.DASHBOARD_TASK),
                taskCountFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TASK_COUNT),
                capacityFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_CAPACITY),
                excessFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EXCESS),
                overlapDaysFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_SCHEDULE_OVERLAP_DAYS),
                formula = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEKLY_LOAD_FORMULA),
                date = GetResourceText(BackEndResourceKeys.DATE),
                status = GetResourceText(BackEndResourceKeys.STATUS),
                workingDay = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WORKING_DAY),
                nonWorkingDay = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NON_WORKING_DAY),
                holidayDay = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_HOLIDAY_DAY),
                holidayDaysFormat = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_HOLIDAY_DAYS),
                noAssignmentWeek = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_ASSIGNMENT_WEEK),
                noTasksOnDay = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_TASKS_ON_DAY),
                noProjectAllocation = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_WEEK_PROJECT_ALLOCATION),
                noTasks = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_WEEK_TASKS)
            });
        }

        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();
            ListItem allProjects = new ListItem(
                GetResourceText(BackEndResourceKeys.ALL_PROJECTS),
                AllProjectsValue);
            allProjects.Selected = true;
            ddlProjectFilter.Items.Add(allProjects);

            foreach (var project in DashboardResourceManager.Instance
                .GetProjectsForFilter())
            {
                ddlProjectFilter.Items.Add(new ListItem(
                    project.MaDuAn + " - " + project.TenDuAn,
                    project.IdDuAn.ToString()));
            }
        }

        private void LoadWeekCountFilter()
        {
            ddlWeekCount.Items.Clear();
            ddlWeekCount.Items.Add(new ListItem(
                string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_COUNT),
                    2),
                "2"));
            ListItem fourWeeks = new ListItem(
                string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_COUNT),
                    4),
                "4");
            fourWeeks.Selected = true;
            ddlWeekCount.Items.Add(fourWeeks);
            ddlWeekCount.Items.Add(new ListItem(
                string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_COUNT),
                    6),
                "6"));
        }

        private DashboardResourceFilter BuildResourceFilter()
        {
            Guid? projectId = null;
            Guid parsedProjectId;
            if (!string.IsNullOrEmpty(ddlProjectFilter.SelectedValue)
                && Guid.TryParse(
                    ddlProjectFilter.SelectedValue,
                    out parsedProjectId))
            {
                projectId = parsedProjectId;
            }

            int weekCount;
            if (!int.TryParse(ddlWeekCount.SelectedValue, out weekCount))
            {
                weekCount = 4;
            }

            return new DashboardResourceFilter
            {
                ProjectId = projectId,
                AnchorWeekStart = AnchorWeekStart,
                WeekCount = weekCount
            };
        }

        private static string ToSafeJson(object value)
        {
            return JsonConvert.SerializeObject(value)
                .Replace("</", "<\\/");
        }

        private static DateTime GetMonday(DateTime date)
        {
            int difference = (7 + (int)date.DayOfWeek
                - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-difference);
        }

        private static string GetDayLabel(DateTime date)
        {
            return CultureInfo.CurrentUICulture.DateTimeFormat
                .GetAbbreviatedDayName(date.DayOfWeek);
        }
    }
}
