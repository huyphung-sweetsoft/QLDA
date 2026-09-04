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
                        "/Controls/Dashboard/dashboard-style.css")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-resource.js")
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
                case ResourceLoadStatus.Underloaded: return "Thiếu tải";
                case ResourceLoadStatus.Balanced: return "Tải cân bằng";
                case ResourceLoadStatus.Overloaded: return "Quá tải";
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
                return "Quá tải cả tháng";
            }

            if (load.OverloadWeekCount > 0)
            {
                return load.OverloadWeekCount
                    + " tuần quá tải";
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
                ? "Không có tuần quá tải trong tháng"
                : "Các tuần quá tải: " + string.Join(", ", overloadWeeks);
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
                ? "Chưa có lịch"
                : load.AllocatedDays.ToString("0.#") + "/"
                    + load.CapacityDays.ToString("0") + " ngày";
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
                string text = "Được giao "
                    + employee.AllocatedDays.ToString("0.#") + "/"
                    + employee.CapacityDays.ToString("0")
                    + " ngày công; vượt "
                    + employee.OverAllocatedDays.ToString("0.#") + " ngày";
                if (employee.OverloadDayCount > 0)
                {
                    text += "; " + employee.OverloadDayCount
                        + " ngày chồng lịch";
                }

                return text;
            }

            if (employee.AllocatedDays <= 0)
            {
                return "Chưa có công việc trong tuần trọng tâm";
            }

            return "Mức dùng tuần chỉ "
                + employee.AverageUtilization.ToString("0.#") + "%";
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
                            + "–" + week.WeekEnd.ToString("dd/MM/yyyy"),
                        allocation = week.AllocationPercent,
                        allocatedDays = week.AllocatedDays,
                        capacityDays = week.CapacityDays,
                        overAllocatedDays = week.OverAllocatedDays,
                        overlapDayCount = week.OverlapDayCount,
                        projects = week.Projects.Select(project => new
                        {
                            code = project.ProjectCode,
                            name = project.ProjectName,
                            taskCount = project.TaskCount,
                            allocatedDays = project.AllocatedDays,
                            allocation = project.AllocationPercent
                        }),
                        tasks = week.Tasks.Select(task => new
                        {
                            code = task.TaskCode,
                            name = task.TaskName,
                            projectCode = task.ProjectCode,
                            projectName = task.ProjectName,
                            allocation = task.AllocationPercent,
                            allocatedDays = task.AllocatedDays,
                            activeDates = task.ActiveDates.Select(date =>
                                GetVietnameseDayLabel(date) + " "
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
                            displayDate = GetVietnameseDayLabel(day.Date)
                                + " " + day.Date.ToString("dd/MM"),
                            allocation = day.AllocationPercent,
                            taskCount = day.Tasks.Count
                        })
                    })
                }));
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
            ddlWeekCount.Items.Add(new ListItem("2 tuần", "2"));
            ListItem fourWeeks = new ListItem("4 tuần", "4");
            fourWeeks.Selected = true;
            ddlWeekCount.Items.Add(fourWeeks);
            ddlWeekCount.Items.Add(new ListItem("6 tuần", "6"));
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
    }
}
