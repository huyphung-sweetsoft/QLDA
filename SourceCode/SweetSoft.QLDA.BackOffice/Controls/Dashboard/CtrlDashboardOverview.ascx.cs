using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Dashboard;
using System.Web.UI.WebControls;
namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlDashboardOverview : BaseAdminUserControl
    {
        #region RegisterCSSAndJS
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>();
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-style.css"));

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

        protected bool IsProjectView { get; private set; }

        protected string SelectedProjectCode { get; private set; }

        protected string SelectedProjectName { get; private set; }

        protected string SingleProjectStatusText { get; private set; }

        protected decimal SelectedProjectPlannedProgress { get; private set; }

        protected decimal SelectedProjectVariance { get; private set; }

        protected DateTime? SelectedProjectStartDate { get; private set; }

        protected DateTime? SelectedProjectExpectedEndDate { get; private set; }

        protected DateTime? SelectedProjectActualCompletionDate { get; private set; }

        protected int SelectedProjectDueSoonTaskCount { get; private set; }

        protected ProjectScheduleHealth SelectedProjectHealth { get; private set; }

        protected int OpenRiskCount { get; private set; }

        protected int OpenIssueCount { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnApplyDashboardFilter.Text = GetResourceText(Core.ResourceTexts.BackEndResourceKeys.APPLY);
                LoadProjectFilter();
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

            SelectedProjectCode = string.Empty;
            SelectedProjectName = string.Empty;
            SelectedProjectPlannedProgress = 0;
            SelectedProjectVariance = 0;
            SelectedProjectStartDate = null;
            SelectedProjectExpectedEndDate = null;
            SelectedProjectActualCompletionDate = null;
            SelectedProjectDueSoonTaskCount = 0;
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
                case ProjectScheduleHealth.Completed: return "Hoàn thành";
                case ProjectScheduleHealth.OnTrack: return "Đúng tiến độ";
                case ProjectScheduleHealth.AtRisk: return "Có nguy cơ";
                case ProjectScheduleHealth.BehindSchedule: return "Chậm tiến độ";
                case ProjectScheduleHealth.Overdue: return "Quá hạn";
                default: return "Chưa bắt đầu";
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
            if (SelectedProjectActualCompletionDate.HasValue)
            {
                return "Hoàn thành ngày "
                    + SelectedProjectActualCompletionDate.Value
                        .ToString("dd/MM/yyyy");
            }

            if (!SelectedProjectStartDate.HasValue
                || !SelectedProjectExpectedEndDate.HasValue)
            {
                return "Chưa có đủ thông tin thời gian";
            }

            DateTime today = DateTime.Today;
            DateTime startDate = SelectedProjectStartDate.Value.Date;
            DateTime endDate = SelectedProjectExpectedEndDate.Value.Date;

            if (startDate > today)
            {
                return "Bắt đầu sau " + (startDate - today).Days + " ngày";
            }

            if (endDate < today)
            {
                return "Quá hạn " + (today - endDate).Days + " ngày";
            }

            if (endDate == today)
            {
                return "Kết thúc hôm nay";
            }

            return "Còn " + (endDate - today).Days + " ngày";
        }

        protected string GetProjectTimelineBadgeCss()
        {
            if (SelectedProjectActualCompletionDate.HasValue)
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

            ddlProjectFilter.Items.Add(
                new ListItem(
                    GetResourceText(Core.ResourceTexts.BackEndResourceKeys.ALL_PROJECTS),
                    "")
            );

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
        }


        protected void btnApplyDashboardFilter_Click(
    object sender,
    EventArgs e)
        {
            DashboardFilter filter = BuildDashboardFilter();

            InitDashboard(filter);
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

            if (!string.IsNullOrEmpty(
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
