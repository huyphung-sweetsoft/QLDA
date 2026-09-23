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
        private const int TaskNotStartedStatusCode = 0;
        private const int TaskInProgressStatusCode = 1;
        private const int TaskCompletedStatusCode = 2;
        private const int TaskOverdueStatusCode = 3;

        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-style.css?v=3")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-progress.js?v=5")
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

        protected string DashboardTextsJson { get; private set; }

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

        protected Guid SelectedProjectId
        {
            get
            {
                if (Model == null
                    || !Model.IsSingleProject
                    || Model.ProjectScheduleStatistics == null
                    || Model.ProjectScheduleStatistics.Count == 0)
                {
                    return Guid.Empty;
                }

                return Model.ProjectScheduleStatistics[0].ProjectId;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlDateRange.AutoPostBack = IsProjectDashboard;
            btnApplyDashboardFilter.Visible = !IsProjectDashboard;

            if (!IsPostBack)
            {
                if (!IsProjectDashboard)
                {
                    btnApplyDashboardFilter.Text =
                        GetResourceText(BackEndResourceKeys.APPLY);
                    LoadProjectFilter();
                }

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

        protected void ddlDateRange_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildDashboardFilter());
        }

        protected string GetHealthText(ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.NotStarted:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_NOT_STARTED);
                case ProjectScheduleHealth.OnTrack:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_HEALTH_ON_TRACK);
                case ProjectScheduleHealth.AtRisk:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_HEALTH_AT_RISK);
                case ProjectScheduleHealth.BehindSchedule:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_HEALTH_BEHIND);
                case ProjectScheduleHealth.Overdue:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE);
                case ProjectScheduleHealth.Completed:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_COMPLETED);
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

        protected string GetVarianceCss(
            ProjectScheduleHealth health,
            decimal variance)
        {
            switch (health)
            {
                case ProjectScheduleHealth.AtRisk:
                    return "text-warning fw-semibold";
                case ProjectScheduleHealth.BehindSchedule:
                case ProjectScheduleHealth.Overdue:
                    return "text-danger fw-semibold";
            }

            return variance > 0
                ? "text-success fw-semibold"
                : "text-muted";
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

        protected string GetSelectedProjectVarianceCss()
        {
            if (Model == null || Model.ProjectScheduleStatistics.Count == 0)
            {
                return "text-muted";
            }

            ProjectScheduleStatistic project =
                Model.ProjectScheduleStatistics[0];
            return GetVarianceCss(project.Health, project.Variance);
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
                return string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_OVERDUE),
                    Math.Abs(task.DaysToDeadline));
            }

            if (task.DaysToDeadline == 0)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_DUE_TODAY);
            }

            return string.Format(
                GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_REMAINING),
                task.DaysToDeadline);
        }

        protected string GetTaskStatusBadgeCss(TaskProgressDetail task)
        {
            switch (task.StatusCode)
            {
                case TaskInProgressStatusCode:
                    return "bg-info-subtle text-info";
                case TaskCompletedStatusCode:
                    return "bg-success-subtle text-success";
                case TaskOverdueStatusCode:
                    return "bg-danger-subtle text-danger";
                case TaskNotStartedStatusCode:
                    return "bg-secondary-subtle text-secondary";
                default: return "bg-secondary-subtle text-secondary";
            }
        }

        protected string GetTaskDeadlineText(TaskProgressDetail task)
        {
            if (!task.Deadline.HasValue || !task.DaysToDeadline.HasValue)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_DEADLINE);
            }

            if (task.StatusCode == TaskCompletedStatusCode)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COMPLETED_LABEL);
            }

            if (task.DaysToDeadline.Value < 0)
            {
                return string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_OVERDUE),
                    Math.Abs(task.DaysToDeadline.Value));
            }

            if (task.DaysToDeadline.Value == 0)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_DUE_TODAY);
            }

            return string.Format(
                GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_REMAINING),
                task.DaysToDeadline.Value);
        }

        protected string GetProjectDetailUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectDetail);
        }

        protected string GetProjectTasksUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectTasks);
        }

        protected string GetProjectGanttUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectGanttCharts);
        }

        protected string GetProjectReportUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectReports);
        }

        protected string GetActualCompletionText(
            ProjectScheduleStatistic project)
        {
            if (project.ActualCompletionDate.HasValue)
            {
                return project.ActualCompletionDate.Value.ToString("dd/MM/yyyy");
            }

            return project.Health == ProjectScheduleHealth.Completed
                ? GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETED_LABEL)
                : GetResourceText(BackEndResourceKeys.DASHBOARD_NOT_COMPLETED);
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

        private void InitDashboard(DashboardFilter filter)
        {
            Model = DashboardProgressManager.Instance.GetProgress(filter);

            ProjectScheduleChartData = JsonConvert.SerializeObject(
                Model.ProjectScheduleStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    detailUrl = GetProjectDetailUrl(x.ProjectId),
                    actual = x.ActualProgress,
                    planned = x.PlannedProgress,
                    variance = x.Variance
                }));

            TaskStatusChartData = JsonConvert.SerializeObject(new
            {
                labels = Model.TaskStatusStatistics.Select(x => x.Status),
                values = Model.TaskStatusStatistics.Select(x => x.Count),
                tasksUrl = Model.IsSingleProject
                    ? GetProjectTasksUrl(SelectedProjectId)
                    : string.Empty
            });

            ProjectTaskChartData = JsonConvert.SerializeObject(
                Model.ProjectTaskStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    tasksUrl = GetProjectTasksUrl(x.ProjectId),
                    completed = x.CompletedCount,
                    inProgress = x.InProgressCount,
                    notStarted = x.NotStartedCount,
                    overdue = x.OverdueCount
                }));

            DashboardTextsJson = JsonConvert.SerializeObject(new
            {
                noProjectProgressData = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_PROJECT_PROGRESS_DATA),
                actual = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_ACTUAL_PROGRESS),
                planned = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PLANNED_PROGRESS),
                noTasksInPeriod = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_TASKS_IN_PERIOD),
                totalTasks = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TOTAL_TASKS),
                noProjectTaskData = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_PROJECT_TASK_DATA),
                completed = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_STATUS_COMPLETED),
                inProgress = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS),
                notStarted = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_STATUS_NOT_STARTED),
                overdue = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE)
            }).Replace("</", "<\\/");

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

            SelectProjectFromQuery();
        }

        private void SelectProjectFromQuery()
        {
            Guid projectId;
            if (!Guid.TryParse(
                Page.Request.QueryString["project"],
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
