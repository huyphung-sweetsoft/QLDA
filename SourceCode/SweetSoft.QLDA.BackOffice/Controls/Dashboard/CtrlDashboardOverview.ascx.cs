using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Dashboard;
using SweetSoft.QLDA.Core.ResourceTexts;
using System.Web.UI.WebControls;
namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlDashboardOverview : BaseAdminUserControl
    {
        private const string AllProjectsValue = "__all_projects__";

        #region RegisterCSSAndJS
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>();
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-style.css?v=3"));

                List<string> jsLinks = new List<string>();
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath(
                    "/Styles/plugins/apexcharts/apexcharts.min.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-overview.js"));

                return new RegisterCSSAndJS(
                    "cpHeadVendor", "cpVendorScript",
                    cssLinks, jsLinks);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }
        #endregion

        protected int TotalProjectCount { get; private set; }

        protected decimal OverallProgress { get; private set; }

        protected int TotalTaskCount { get; private set; }

        protected int OverdueTaskCount { get; private set; }

        protected decimal TotalContractValue { get; private set; }

        protected string ProjectStatusChartData { get; private set; }

        protected string ProjectProgressChartData { get; private set; }

        protected string DashboardTextsJson { get; private set; }
        protected int ActiveProjectCount { get; private set; }
        protected decimal AtRiskProjectRate { get; private set; }
        protected List<ProjectAttentionStatistic> ProjectAttentionStatistics
        {
            get;
            private set;
        }

        protected CostOverviewModel CostOverview { get; private set; }
        protected ResourceOverviewModel ResourceOverview { get; private set; }
        protected int UpcomingMeetingCount { get; private set; }

        protected List<UpcomingMeetingSummary> UpcomingMeetings { get; private set; }

        protected bool IsProjectView { get; private set; }

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

        protected Guid SelectedProjectId { get; private set; }

        protected string SelectedProjectCode { get; private set; }

        protected string SelectedProjectName { get; private set; }

        protected string SingleProjectStatusText { get; private set; }

        protected decimal SelectedProjectPlannedProgress { get; private set; }

        protected decimal SelectedProjectVariance { get; private set; }

        protected DateTime? SelectedProjectStartDate { get; private set; }

        protected DateTime? SelectedProjectExpectedEndDate { get; private set; }

        protected DateTime? SelectedProjectActualCompletionDate { get; private set; }

        protected int SelectedProjectDueSoonTaskCount { get; private set; }

        protected int SelectedProjectTaskCount { get; private set; }

        protected int SelectedProjectCompletedTaskCount { get; private set; }

        protected ProjectScheduleHealth SelectedProjectHealth { get; private set; }

        protected int OpenRiskCount { get; private set; }

        protected int OpenIssueCount { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlDateRange.AutoPostBack = IsProjectDashboard;
            btnApplyDashboardFilter.Visible = !IsProjectDashboard;

            if (!IsPostBack)
            {
                if (!IsProjectDashboard)
                {
                    btnApplyDashboardFilter.Text =
                        GetResourceText(Core.ResourceTexts.BackEndResourceKeys.APPLY);
                    LoadProjectFilter();
                }

                LoadDateRangeFilter();
                InitDashboard();
            }
        }


        private void InitDashboard()
        {
            InitDashboard(BuildDashboardFilter());
        }
        private void InitDashboard(DashboardFilter filter)
        {
            DashboardOverviewModel overview =
                DashboardOverviewManager.Instance.GetOverview(filter);
            IsProjectView =
    filter != null &&
    filter.ProjectId.HasValue;

            SelectedProjectId =
                filter != null && filter.ProjectId.HasValue
                    ? filter.ProjectId.Value
                    : Guid.Empty;
            SelectedProjectCode = string.Empty;
            SelectedProjectName = string.Empty;
            SelectedProjectPlannedProgress = 0;
            SelectedProjectVariance = 0;
            SelectedProjectStartDate = null;
            SelectedProjectExpectedEndDate = null;
            SelectedProjectActualCompletionDate = null;
            SelectedProjectDueSoonTaskCount = 0;
            SelectedProjectTaskCount = 0;
            SelectedProjectCompletedTaskCount = 0;
            SelectedProjectHealth = ProjectScheduleHealth.NotStarted;

            OpenRiskCount = 0;
            OpenIssueCount = 0;

            if (IsProjectView)
            {
                var project =
                    overview.ProjectProgressStatistics
                        .FirstOrDefault();

                if (project != null)
                {
                    SelectedProjectId = project.ProjectId;
                    SelectedProjectCode = project.ProjectCode;
                    SelectedProjectName = project.ProjectName;
                    SelectedProjectPlannedProgress = project.PlannedProgress;
                    SelectedProjectVariance = project.Variance;
                    SelectedProjectStartDate = project.StartDate;
                    SelectedProjectExpectedEndDate = project.ExpectedEndDate;
                    SelectedProjectActualCompletionDate =
                        project.ActualCompletionDate;
                    SelectedProjectDueSoonTaskCount =
                        project.DueSoonTaskCount;
                    SelectedProjectTaskCount = project.TaskCount;
                    SelectedProjectCompletedTaskCount =
                        project.CompletedTaskCount;
                    SelectedProjectHealth = project.Health;
                }

                if (overview.ProjectAttentionStatistics != null)
                {
                    var attention =
                        overview.ProjectAttentionStatistics
                            .FirstOrDefault();

                    if (attention != null)
                    {
                        OpenRiskCount = attention.RiskCount;
                        OpenIssueCount = attention.IssueCount;
                    }
                }

                var activeStatus = overview.ProjectStatusStatistics?.FirstOrDefault(x => x.Count > 0);
                if (activeStatus != null)
                {
                    SingleProjectStatusText = activeStatus.Status;
                }
                else
                {
                    SingleProjectStatusText = "-";
                }
            }

            TotalProjectCount =
                overview.TotalProjectCount;

            ActiveProjectCount =
                overview.ActiveProjectCount;

            OverallProgress =
                overview.OverallProgress;

            TotalTaskCount =
                overview.TotalTaskCount;

            OverdueTaskCount =
                overview.OverdueTaskCount;

            UpcomingMeetingCount =
                overview.UpcomingMeetingCount;

            UpcomingMeetings =
                overview.UpcomingMeetings ??
                new List<UpcomingMeetingSummary>();

            AtRiskProjectRate =
                overview.AtRiskProjectRate;

            TotalContractValue =
                overview.TotalContractValue;

            ProjectStatusChartData =
                BuildProjectStatusChartData(
                    overview.ProjectStatusStatistics);

            ProjectProgressChartData =
                BuildProjectProgressChartData(
                    overview.ProjectProgressStatistics);

            DashboardTextsJson = ToSafeJson(new
            {
                progress = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN),
                progressAxis = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROGRESS_AXIS),
                start = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_START),
                expected = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EXPECTED)
            });

            ProjectAttentionStatistics =
                overview.ProjectAttentionStatistics;

            ResourceOverview =
                overview.ResourceOverview;

            CostOverview =
                overview.CostOverview;
        }

        protected string GetVarianceText(decimal variance)
        {
            return (variance > 0 ? "+" : string.Empty)
                + variance.ToString("0.##")
                + "%";
        }

        protected string GetVarianceCss(decimal variance)
        {
            if (variance < 0)
            {
                return "text-danger";
            }

            if (variance > 0)
            {
                return "text-success";
            }

            return "text-muted";
        }

        protected string GetProgressBarCss(decimal progress)
        {
            if (progress >= 80)
            {
                return "bg-success";
            }

            if (progress >= 50)
            {
                return "bg-primary";
            }

            if (progress > 0)
            {
                return "bg-warning";
            }

            return "bg-secondary";
        }

        protected string GetProjectHealthText(ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.Completed:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_COMPLETED);
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
                default:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_STATUS_NOT_STARTED);
            }
        }

        protected string GetProjectHealthBadgeCss(
            ProjectScheduleHealth health)
        {
            switch (health)
            {
                case ProjectScheduleHealth.Completed:
                case ProjectScheduleHealth.OnTrack:
                    return "bg-success-subtle text-success";
                case ProjectScheduleHealth.AtRisk:
                    return "bg-warning-subtle text-warning";
                case ProjectScheduleHealth.BehindSchedule:
                case ProjectScheduleHealth.Overdue:
                    return "bg-danger-subtle text-danger";
                default:
                    return "bg-secondary-subtle text-secondary";
            }
        }

        protected string GetProjectTimelineText()
        {
            if (IsSelectedProjectCompleted())
            {
                return SelectedProjectActualCompletionDate.HasValue
                    ? string.Format(
                        GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETED_ON),
                        SelectedProjectActualCompletionDate.Value
                            .ToString("dd/MM/yyyy"))
                    : GetResourceText(
                        BackEndResourceKeys.DASHBOARD_COMPLETED_LABEL);
            }

            if (!SelectedProjectStartDate.HasValue
                || !SelectedProjectExpectedEndDate.HasValue)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_INSUFFICIENT_TIME_INFO);
            }

            DateTime today = DateTime.Today;
            DateTime startDate = SelectedProjectStartDate.Value.Date;
            DateTime endDate = SelectedProjectExpectedEndDate.Value.Date;

            if (startDate > today)
            {
                return string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_STARTS_IN_DAYS),
                    (startDate - today).Days);
            }

            if (endDate < today)
            {
                return string.Format(
                    GetResourceText(BackEndResourceKeys.DASHBOARD_OVERDUE_DAYS),
                    (today - endDate).Days);
            }

            if (endDate == today)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_ENDS_TODAY);
            }

            return string.Format(
                GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_REMAINING),
                (endDate - today).Days);
        }

        protected string GetProjectTimelineBadgeCss()
        {
            if (IsSelectedProjectCompleted())
            {
                return "bg-success-subtle text-success";
            }

            if (SelectedProjectStartDate.HasValue
                && SelectedProjectStartDate.Value.Date > DateTime.Today)
            {
                return "bg-secondary-subtle text-secondary";
            }

            if (SelectedProjectExpectedEndDate.HasValue
                && SelectedProjectExpectedEndDate.Value.Date < DateTime.Today)
            {
                return "bg-danger-subtle text-danger";
            }

            return "bg-primary-subtle text-primary";
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

        protected string GetSelectedProjectActualCompletionText()
        {
            if (SelectedProjectActualCompletionDate.HasValue)
            {
                return SelectedProjectActualCompletionDate.Value
                    .ToString("dd/MM/yyyy");
            }

            return IsSelectedProjectCompleted()
                ? GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETED_LABEL)
                : GetResourceText(BackEndResourceKeys.DASHBOARD_NOT_COMPLETED);
        }

        private bool IsSelectedProjectCompleted()
        {
            return SelectedProjectActualCompletionDate.HasValue
                || SelectedProjectHealth == ProjectScheduleHealth.Completed;
        }

        protected string GetProjectRisksUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectRisks);
        }

        protected string GetProjectIssuesUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectIssues);
        }

        protected string GetProjectMeetingsUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectMeets);
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

        private string BuildProjectStatusChartData(
            List<ProjectStatusStatistic> statistics)
        {
            if (statistics == null)
            {
                statistics = new List<ProjectStatusStatistic>();
            }

            var labels =
                statistics.Select(x => x.Status).ToList();

            var values =
                statistics.Select(x => x.Count).ToList();

            return ToSafeJson(
                new
                {
                    labels = labels,
                    values = values
                });
        }

        private string BuildProjectProgressChartData(
            List<ProjectProgressStatistic> statistics)
        {
            if (statistics == null)
            {
                statistics = new List<ProjectProgressStatistic>();
            }

            return ToSafeJson(
                statistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    detailUrl = GetProjectDetailUrl(x.ProjectId),
                    progress = x.Progress,
                    startDate = x.StartDate.ToString("dd/MM/yyyy"),
                    expectedEndDate =
                        x.ExpectedEndDate.ToString("dd/MM/yyyy")
                })
            );
        }




        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();

            ListItem allProjects = new ListItem(
                GetResourceText(
                    Core.ResourceTexts.BackEndResourceKeys.ALL_PROJECTS),
                AllProjectsValue);
            allProjects.Selected = true;
            ddlProjectFilter.Items.Add(allProjects);

            var projects =
                DashboardOverviewManager.Instance.GetProjectsForFilter();

            foreach (var project in projects)
            {
                ddlProjectFilter.Items.Add(
                    new ListItem(
                        project.MaDuAn + " - " + project.TenDuAn,
                        project.IdDuAn.ToString()
                    )
                );
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


        protected void btnApplyDashboardFilter_Click(
    object sender,
    EventArgs e)
        {
            DashboardFilter filter = BuildDashboardFilter();

            InitDashboard(filter);
        }

        protected void ddlDateRange_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildDashboardFilter());
        }

        private void LoadDateRangeFilter()
        {
            ddlDateRange.Items.Clear();
            
            ddlDateRange.Items.Add(new ListItem(GetResourceText(Core.ResourceTexts.BackEndResourceKeys.THIS_WEEK), "2"));
            
            ListItem thisMonth = new ListItem(GetResourceText(Core.ResourceTexts.BackEndResourceKeys.THIS_MONTH), "3");
            thisMonth.Selected = true;
            ddlDateRange.Items.Add(thisMonth);
            
            ddlDateRange.Items.Add(new ListItem(GetResourceText(Core.ResourceTexts.BackEndResourceKeys.THIS_QUARTER), "4"));
            
            ddlDateRange.Items.Add(new ListItem(GetResourceText(Core.ResourceTexts.BackEndResourceKeys.THIS_YEAR), "5"));
        }

        private DashboardFilter BuildDashboardFilter()
        {
            Guid? projectId = null;

            if (!IsProjectDashboard
                && ddlProjectFilter != null
                && !string.IsNullOrEmpty(
                    ddlProjectFilter.SelectedValue))
            {
                Guid parsedProjectId;

                if (Guid.TryParse(
                    ddlProjectFilter.SelectedValue,
                    out parsedProjectId))
                {
                    projectId = parsedProjectId;
                }
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
            if (ddlDateRange.SelectedValue != "")
            {
                dateRange = (DashboardDateRange)int.Parse(ddlDateRange.SelectedValue);
            }

            DateTime fromDate = GetFromDate(dateRange);
            DateTime toDate = GetToDate(dateRange);

            return new DashboardFilter
            {
                ProjectId = projectId,
                DateRange = dateRange,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        private DateTime GetFromDate(DashboardDateRange dateRange)
        {
            DateTime today = DateTime.Today;
            switch (dateRange)
            {
                case DashboardDateRange.Today: return today;
                case DashboardDateRange.ThisWeek:
                    int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-diff);
                case DashboardDateRange.ThisMonth: return new DateTime(today.Year, today.Month, 1);
                case DashboardDateRange.ThisQuarter:
                    int startMonth = ((today.Month - 1) / 3) * 3 + 1;
                    return new DateTime(today.Year, startMonth, 1);
                case DashboardDateRange.ThisYear: return new DateTime(today.Year, 1, 1);
                default: return today;
            }
        }

        private DateTime GetToDate(DashboardDateRange dateRange)
        {
            DateTime today = DateTime.Today;
            switch (dateRange)
            {
                case DashboardDateRange.Today: return today;
                case DashboardDateRange.ThisWeek:
                    int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-diff).AddDays(6);
                case DashboardDateRange.ThisMonth:
                    return new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                case DashboardDateRange.ThisQuarter:
                    int endMonth = ((today.Month - 1) / 3) * 3 + 3;
                    return new DateTime(today.Year, endMonth, DateTime.DaysInMonth(today.Year, endMonth));
                case DashboardDateRange.ThisYear: return new DateTime(today.Year, 12, 31);
                default: return today;
            }
        }

        private static string ToSafeJson(object value)
        {
            return JsonConvert.SerializeObject(value)
                .Replace("</", "<\\/");
        }

    }
}
