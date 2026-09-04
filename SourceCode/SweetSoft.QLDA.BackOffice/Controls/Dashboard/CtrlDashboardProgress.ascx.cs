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
    public partial class CtrlDashboardProgress : BaseAdminUserControl
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
                        "/Controls/Dashboard/dashboard-progress.js")
                };

                return new RegisterCSSAndJS(
                    "cpHeadVendor",
                    "cpVendorScript",
                    cssLinks,
                    jsLinks);
            }
        }

        protected DashboardProgressModel Model { get; private set; }

        protected string ProjectScheduleChartData { get; private set; }

        protected string TaskStatusChartData { get; private set; }

        protected string ProjectTaskChartData { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnApplyDashboardFilter.Text =
                    GetResourceText(BackEndResourceKeys.APPLY);
                LoadProjectFilter();
                LoadDateRangeFilter();
                InitDashboard(BuildDashboardFilter());
            }
        }

        protected void btnApplyDashboardFilter_Click(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildDashboardFilter());
        }

        protected string GetHealthText(ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.NotStarted: return "Chưa bắt đầu";
                case ProjectScheduleHealth.OnTrack: return "Đúng tiến độ";
                case ProjectScheduleHealth.AtRisk: return "Có nguy cơ";
                case ProjectScheduleHealth.BehindSchedule: return "Chậm tiến độ";
                case ProjectScheduleHealth.Overdue: return "Quá hạn";
                case ProjectScheduleHealth.Completed: return "Hoàn thành";
                default: return "-";
            }
        }

        protected string GetHealthBadgeCss(ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.Completed:
                case ProjectScheduleHealth.OnTrack:
                    return "bg-success-subtle text-success";
                case ProjectScheduleHealth.NotStarted:
                    return "bg-secondary-subtle text-secondary";
                case ProjectScheduleHealth.AtRisk:
                    return "bg-warning-subtle text-warning";
                default:
                    return "bg-danger-subtle text-danger";
            }
        }

        protected string GetProgressBarCss(decimal progress)
        {
            if (progress >= 80)
            {
                return "bg-success";
            }

            if (progress >= 50)
            {
                return "bg-info";
            }

            if (progress > 0)
            {
                return "bg-warning";
            }

            return "bg-secondary";
        }

        protected string GetVarianceCss(decimal variance)
        {
            if (variance < -5)
            {
                return "text-danger fw-semibold";
            }

            if (variance > 5)
            {
                return "text-success fw-semibold";
            }

            return "text-muted";
        }

        protected decimal GetSelectedProjectVariance()
        {
            if (Model == null || Model.ProjectScheduleStatistics.Count == 0)
            {
                return 0;
            }

            return Model.ProjectScheduleStatistics[0].Variance;
        }

        protected string GetSelectedProjectVarianceText()
        {
            decimal variance = GetSelectedProjectVariance();
            return GetVarianceText(variance) + "%";
        }

        protected string GetVarianceText(decimal variance)
        {
            return (variance > 0 ? "+" : string.Empty)
                + variance.ToString("0.##");
        }

        protected string GetPercentStyle(decimal progress)
        {
            decimal normalized = Math.Max(0, Math.Min(100, progress));
            return normalized.ToString("0.##", CultureInfo.InvariantCulture);
        }

        protected string GetDeadlineText(ProgressTaskInfo task)
        {
            if (task.IsOverdue)
            {
                return "Quá " + Math.Abs(task.DaysToDeadline) + " ngày";
            }

            if (task.DaysToDeadline == 0)
            {
                return "Đến hạn hôm nay";
            }

            return "Còn " + task.DaysToDeadline + " ngày";
        }

        protected string GetTaskStatusBadgeCss(TaskProgressDetail task)
        {
            switch (task.StatusCode)
            {
                case 1: return "bg-info-subtle text-info";
                case 2: return "bg-success-subtle text-success";
                case 3: return "bg-danger-subtle text-danger";
                default: return "bg-secondary-subtle text-secondary";
            }
        }

        protected string GetTaskDeadlineText(TaskProgressDetail task)
        {
            if (!task.Deadline.HasValue || !task.DaysToDeadline.HasValue)
            {
                return "Chưa đặt hạn";
            }

            if (task.StatusCode == 2)
            {
                return "Đã hoàn thành";
            }

            if (task.DaysToDeadline.Value < 0)
            {
                return "Quá " + Math.Abs(task.DaysToDeadline.Value) + " ngày";
            }

            if (task.DaysToDeadline.Value == 0)
            {
                return "Đến hạn hôm nay";
            }

            return "Còn " + task.DaysToDeadline.Value + " ngày";
        }

        private void InitDashboard(DashboardFilter filter)
        {
            Model = DashboardProgressManager.Instance.GetProgress(filter);

            ProjectScheduleChartData = JsonConvert.SerializeObject(
                Model.ProjectScheduleStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    actual = x.ActualProgress,
                    planned = x.PlannedProgress,
                    variance = x.Variance
                }));

            TaskStatusChartData = JsonConvert.SerializeObject(new
            {
                labels = Model.TaskStatusStatistics.Select(x => x.Status),
                values = Model.TaskStatusStatistics.Select(x => x.Count)
            });

            ProjectTaskChartData = JsonConvert.SerializeObject(
                Model.ProjectTaskStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    completed = x.CompletedCount,
                    inProgress = x.InProgressCount,
                    notStarted = x.NotStartedCount,
                    overdue = x.OverdueCount
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

            foreach (var project in
                DashboardProgressManager.Instance.GetProjectsForFilter())
            {
                ddlProjectFilter.Items.Add(new ListItem(
                    project.MaDuAn + " - " + project.TenDuAn,
                    project.IdDuAn.ToString()));
            }
        }

        private void LoadDateRangeFilter()
        {
            ddlDateRange.Items.Clear();
            ddlDateRange.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_WEEK),
                ((int)DashboardDateRange.ThisWeek).ToString(
                    CultureInfo.InvariantCulture)));

            ListItem thisMonth = new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_MONTH),
                ((int)DashboardDateRange.ThisMonth).ToString(
                    CultureInfo.InvariantCulture));
            thisMonth.Selected = true;
            ddlDateRange.Items.Add(thisMonth);

            ddlDateRange.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_QUARTER),
                ((int)DashboardDateRange.ThisQuarter).ToString(
                    CultureInfo.InvariantCulture)));
            ddlDateRange.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_YEAR),
                ((int)DashboardDateRange.ThisYear).ToString(
                    CultureInfo.InvariantCulture)));
        }

        private DashboardFilter BuildDashboardFilter()
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

            DashboardDateRange dateRange = DashboardDateRange.ThisMonth;
            int parsedDateRange;

            if (int.TryParse(
                ddlDateRange.SelectedValue,
                out parsedDateRange)
                && Enum.IsDefined(typeof(DashboardDateRange), parsedDateRange))
            {
                dateRange = (DashboardDateRange)parsedDateRange;
            }

            return new DashboardFilter
            {
                ProjectId = projectId,
                DateRange = dateRange,
                FromDate = GetFromDate(dateRange),
                ToDate = GetToDate(dateRange)
            };
        }

        private static DateTime GetFromDate(DashboardDateRange dateRange)
        {
            DateTime today = DateTime.Today;

            switch (dateRange)
            {
                case DashboardDateRange.Today:
                    return today;
                case DashboardDateRange.ThisWeek:
                    int diff = (7 + (int)today.DayOfWeek
                        - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-diff);
                case DashboardDateRange.ThisQuarter:
                    int startMonth = ((today.Month - 1) / 3) * 3 + 1;
                    return new DateTime(today.Year, startMonth, 1);
                case DashboardDateRange.ThisYear:
                    return new DateTime(today.Year, 1, 1);
                default:
                    return new DateTime(today.Year, today.Month, 1);
            }
        }

        private static DateTime GetToDate(DashboardDateRange dateRange)
        {
            DateTime today = DateTime.Today;

            switch (dateRange)
            {
                case DashboardDateRange.Today:
                    return today;
                case DashboardDateRange.ThisWeek:
                    int diff = (7 + (int)today.DayOfWeek
                        - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-diff).AddDays(6);
                case DashboardDateRange.ThisQuarter:
                    int endMonth = ((today.Month - 1) / 3) * 3 + 3;
                    return new DateTime(
                        today.Year,
                        endMonth,
                        DateTime.DaysInMonth(today.Year, endMonth));
                case DashboardDateRange.ThisYear:
                    return new DateTime(today.Year, 12, 31);
                default:
                    return new DateTime(
                        today.Year,
                        today.Month,
                        DateTime.DaysInMonth(today.Year, today.Month));
            }
        }
    }
}
