using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
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
                        "/Controls/Dashboard/dashboard-style.css?v=27")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-resource.js?v=11")
                };

                return new RegisterCSSAndJS(
                    "cpHeadVendor",
                    "cpVendorScript",
                    cssLinks,
                    jsLinks);
            }
        }

        protected DashboardResourceModel Model { get; private set; }

        protected string ResourceDetailData { get; private set; }

        protected string DashboardTextsJson { get; private set; }

        protected string InitialDetailJson { get; private set; }

        protected bool IsProjectDashboard
        {
            get
            {
                Guid projectId;
                return Guid.TryParse(
                    Page.Request.QueryString["project"],
                    out projectId);
            }
        }

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

        private DateTime AnchorMonthStart
        {
            get
            {
                object value = ViewState["ResourceAnchorMonthStart"];
                return value == null
                    ? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                    : (DateTime)value;
            }
            set { ViewState["ResourceAnchorMonthStart"] =
                new DateTime(value.Year, value.Month, 1); }
        }

        protected bool IsMonthlyView
        {
            get { return ddlWeekCount.SelectedValue == "month"; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlWeekCount.AutoPostBack = true;
            ddlProjectFilter.AutoPostBack = true;

            btnPreviousWeek.ToolTip = GetResourceText(
                IsMonthlyView
                    ? BackEndResourceKeys.DASHBOARD_PREVIOUS_MONTH
                    : BackEndResourceKeys.DASHBOARD_PREVIOUS_WEEK);
            btnCurrentWeek.Text = GetResourceText(
                IsMonthlyView
                    ? BackEndResourceKeys.DASHBOARD_CURRENT_MONTH
                    : BackEndResourceKeys.DASHBOARD_CURRENT_WEEK);
            btnNextWeek.ToolTip = GetResourceText(
                IsMonthlyView
                    ? BackEndResourceKeys.DASHBOARD_NEXT_MONTH
                    : BackEndResourceKeys.DASHBOARD_NEXT_WEEK);

            if (!IsPostBack)
            {
                DateTime requestedWeek;
                AnchorWeekStart = DateTime.TryParseExact(
                    Page.Request.QueryString["resourceWeek"],
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out requestedWeek)
                    ? GetMonday(requestedWeek)
                    : GetMonday(DateTime.Today);
                AnchorMonthStart = DateTime.Today;
                if (!IsProjectDashboard)
                {
                    LoadProjectFilter();
                }

                LoadWeekCountFilter();
                InitDashboard(BuildResourceFilter());
            }
        }

        protected void ddlProjectFilter_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildResourceFilter());
        }

        protected void ddlWeekCount_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildResourceFilter());
        }

        protected void btnPreviousWeek_Click(object sender, EventArgs e)
        {
            if (IsMonthlyView)
            {
                AnchorMonthStart = AnchorMonthStart.AddMonths(-1);
            }
            else
            {
                AnchorWeekStart = AnchorWeekStart.AddDays(-7);
            }
            InitDashboard(BuildResourceFilter());
        }

        protected void btnCurrentWeek_Click(object sender, EventArgs e)
        {
            if (IsMonthlyView)
            {
                AnchorMonthStart = DateTime.Today;
            }
            else
            {
                AnchorWeekStart = GetMonday(DateTime.Today);
            }
            InitDashboard(BuildResourceFilter());
        }

        protected void btnNextWeek_Click(object sender, EventArgs e)
        {
            if (IsMonthlyView)
            {
                AnchorMonthStart = AnchorMonthStart.AddMonths(1);
            }
            else
            {
                AnchorWeekStart = AnchorWeekStart.AddDays(7);
            }
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

        protected string GetEmployeeStatusText(ResourceEmployeeLoad employee)
        {
            return employee.AllocatedDays <= 0
                ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD)
                : GetStatusText(employee.Status);
        }

        protected string GetEmployeeStatusKey(ResourceEmployeeLoad employee)
        {
            return employee.AllocatedDays <= 0
                ? "noload"
                : employee.Status.ToString().ToLowerInvariant();
        }

        protected string GetEmployeeStatusBadgeCss(ResourceEmployeeLoad employee)
        {
            return employee.AllocatedDays <= 0
                ? "bg-secondary-subtle text-secondary"
                : GetStatusBadgeCss(employee.Status);
        }

        protected string GetEmployeeStatusLoadCss(ResourceEmployeeLoad employee)
        {
            return employee.AllocatedDays <= 0
                ? "resource-load-none"
                : GetStatusLoadCss(employee.Status);
        }

        protected ResourceMonthlyLoad GetFocusMonthLoad(
            ResourceEmployeeLoad employee)
        {
            DateTime monthStart = Model.Months.First().StartDate;
            return employee.MonthlyLoads.First(x =>
                x.MonthStart == monthStart);
        }

        protected string GetFocusMonthStatusKey(ResourceEmployeeLoad employee)
        {
            ResourceMonthlyLoad load = GetFocusMonthLoad(employee);
            return load.AllocatedDays <= 0
                ? "noload"
                : load.Status.ToString().ToLowerInvariant();
        }

        protected int GetFocusMonthCount(string statusKey)
        {
            return Model.EmployeeLoads.Count(employee =>
                GetFocusMonthStatusKey(employee) == statusKey);
        }

        protected string GetWeekLoadText(decimal allocationPercent)
        {
            if (allocationPercent <= 0)
            {
                return GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD);
            }

            return GetStatusText(allocationPercent > 100m
                ? ResourceLoadStatus.Overloaded
                : allocationPercent >= 80m
                    ? ResourceLoadStatus.Balanced
                    : ResourceLoadStatus.Underloaded);
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
            if (load.AllocatedDays <= 0)
            {
                return GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD);
            }

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
            if (load.AllocatedDays <= 0)
            {
                return "bg-secondary-subtle text-secondary";
            }

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
                return "resource-load-none";
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
            return allocationPercent.ToString("0", CultureInfo.InvariantCulture)
                + "%";
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
            if (employee.AllocatedDays <= 0)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_FOCUS_WEEK_TASKS);
            }

            return string.Format(
                GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_DAYS_COMPARISON),
                employee.AllocatedDays.ToString("0.#"),
                employee.CapacityDays.ToString("0"));
        }

        protected string GetProjectDetailUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectDetail);
        }

        protected string GetProjectTasksUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectTasks);
        }

        protected string GetProjectTaskDetailUrl(Guid projectId, Guid taskId)
        {
            string tasksUrl = GetProjectTasksUrl(projectId);
            return string.IsNullOrEmpty(tasksUrl)
                ? string.Empty
                : tasksUrl + "?taskId=" + taskId.ToString("D");
        }

        private string GetResourceWeekUrl(Guid employeeId, DateTime weekStart)
        {
            var query = HttpUtility.ParseQueryString(Page.Request.Url.Query);
            query["resourceWeek"] = weekStart.ToString("yyyy-MM-dd");
            query["resourceEmployee"] = employeeId.ToString("D");

            Guid selectedProjectId;
            if (!IsProjectDashboard
                && Guid.TryParse(ddlProjectFilter.SelectedValue,
                    out selectedProjectId))
            {
                query["resourceProject"] = selectedProjectId.ToString("D");
            }
            else
            {
                query.Remove("resourceProject");
            }

            return Page.Request.Url.AbsolutePath + "?" + query;
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

            Guid requestedEmployee;
            InitialDetailJson = ToSafeJson(!Page.IsPostBack
                && Guid.TryParse(Page.Request.QueryString["resourceEmployee"],
                    out requestedEmployee)
                && Model.EmployeeLoads.Any(x =>
                    x.EmployeeId == requestedEmployee)
                    ? (object)new
                    {
                        person = requestedEmployee,
                        week = Model.AnchorWeekStart.ToString("yyyy-MM-dd")
                    }
                    : null);

            ResourceDetailData = ToSafeJson(
                Model.EmployeeLoads.Select(employee => new
                {
                    id = employee.EmployeeId,
                    name = employee.DisplayName,
                    userName = employee.UserName,
                    department = employee.DepartmentName,
                    jobTitle = employee.JobTitleName,
                    weeks = IsMonthlyView ? null : employee.WeeklyLoads.Select(week => new
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
                        days = week.DailyLoads.Select(day => new
                        {
                            date = day.Date.ToString("yyyy-MM-dd"),
                            displayDate = GetDayLabel(day.Date)
                                + " " + day.Date.ToString("dd/MM"),
                            isWorkingDay = day.IsWorkingDay,
                            isHoliday = day.IsHoliday,
                            holidayName = day.HolidayName,
                            allocation = day.AllocationPercent,
                            tasks = day.Tasks.Select(task => new
                            {
                                tasksUrl = GetProjectTaskDetailUrl(task.ProjectId, task.TaskId),
                                code = task.TaskCode,
                                name = task.TaskName,
                                projectCode = task.ProjectCode,
                                projectName = task.ProjectName
                            })
                        })
                    }),
                    months = IsMonthlyView ? employee.MonthlyLoads.Select(month => new
                    {
                        start = month.MonthStart.ToString("yyyy-MM-dd"),
                        label = month.Label,
                        allocation = month.AverageUtilization,
                        allocatedDays = month.AllocatedDays,
                        capacityDays = month.CapacityDays,
                        status = GetMonthlyStatusText(month),
                        hasOverload = month.OverloadWeekCount > 0,
                        weeks = month.WeeklyLoads.Select(week => new
                        {
                            start = week.WeekStart.ToString("yyyy-MM-dd"),
                            displayRange = week.WeekStart.ToString("dd/MM")
                                + "–" + week.WeekStart.AddDays(6)
                                    .ToString("dd/MM/yyyy"),
                            allocation = week.AllocationPercent,
                            allocatedDays = week.AllocatedDays,
                            capacityDays = week.CapacityDays,
                            taskCount = week.Tasks.Count,
                            status = GetWeekLoadText(week.AllocationPercent),
                            statusCss = GetHeatmapCss(week.AllocationPercent),
                            detailUrl = GetResourceWeekUrl(
                                employee.EmployeeId, week.WeekStart)
                        })
                    }) : null
                }));

            DashboardTextsJson = ToSafeJson(new
            {
                employeeLabel = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL),
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
                monthFormula = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_FORMULA),
                loadCalculationNote = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_LOAD_CALCULATION_NOTE),
                noWorkingDaysLoad = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_WORKING_DAYS_LOAD),
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
                weekScheduleChange = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_SCHEDULE_CHANGE),
                weekScheduleChanged = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_SCHEDULE_CHANGED),
                weekExceptionOffNote = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_EXCEPTION_OFF_NOTE),
                weekScheduledOffNote = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_SCHEDULED_OFF_NOTE),
                weekWeekendWorkNote = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_WEEKEND_WORK_NOTE),
                noAssignmentWeek = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_ASSIGNMENT_WEEK),
                noTasksOnDay = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_TASKS_ON_DAY),
                noProjectAllocation = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_WEEK_PROJECT_ALLOCATION),
                noTasks = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_WEEK_TASKS),
                noLoad = GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD),
                underloaded = GetResourceText(BackEndResourceKeys.DASHBOARD_UNDERLOADED),
                balanced = GetResourceText(BackEndResourceKeys.DASHBOARD_BALANCED_LOAD),
                overloaded = GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED),
                weekDaysComparison = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_DAYS_COMPARISON),
                selectWeek = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_MONTH_SELECT_WEEK_DESC),
                viewWeek = GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_WEEK),
                monthSummary = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_SUMMARY),
                weekDetail = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_WEEK_ALLOCATION_DETAIL)
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

            SelectProjectFromQuery();
        }

        private void SelectProjectFromQuery()
        {
            Guid projectId;
            if (!Guid.TryParse(
                Page.Request.QueryString["project"]
                    ?? Page.Request.QueryString["resourceProject"],
                out projectId))
            {
                return;
            }

            ListItem item = ddlProjectFilter.Items.FindByValue(
                projectId.ToString());
            if (item != null)
            {
                ddlProjectFilter.SelectedValue = item.Value;
            }
        }

        private void LoadWeekCountFilter()
        {
            ddlWeekCount.Items.Clear();
            ddlWeekCount.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_BY_WEEK),
                "week"));
            ddlWeekCount.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_BY_MONTH),
                "month"));
        }

        private DashboardResourceFilter BuildResourceFilter()
        {
            Guid? projectId = null;
            Guid parsedProjectId;
            if (!IsProjectDashboard
                && ddlProjectFilter != null
                && !string.IsNullOrEmpty(ddlProjectFilter.SelectedValue)
                && Guid.TryParse(
                    ddlProjectFilter.SelectedValue,
                    out parsedProjectId))
            {
                projectId = parsedProjectId;
            }

            if (!projectId.HasValue)
            {
                Guid queryProjectId;
                if (Guid.TryParse(
                    Page.Request.QueryString["project"],
                    out queryProjectId))
                {
                    projectId = queryProjectId;
                }
            }

            return new DashboardResourceFilter
            {
                ProjectId = projectId,
                AnchorWeekStart = AnchorWeekStart,
                WeekCount = 4,
                MonthStart = AnchorMonthStart,
                MonthCount = IsMonthlyView ? 4 : 0
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
