using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Dashboard;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;

namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlDashboardOverview : BaseAdminUserControl
    {
        private const string AllProjectsValue = "__all_projects__";

        protected DashboardOverviewSummary Summary { get; private set; }
        protected List<OverviewProjectItem> OverdueProjects { get; private set; }
        protected List<OverviewProjectItem> ProjectsWithOverdueTasks { get; private set; }
        protected List<OverviewProjectItem> UpcomingProjects { get; private set; }
        protected List<OverviewProjectItem> PriorityProjects { get; private set; }
        protected List<DashboardTaskSummary> ProjectTasks { get; private set; }
        protected List<DashboardTaskSummary> CompletedProjectTasks { get; private set; }
        protected List<DashboardTaskSummary> OverdueProjectTasks { get; private set; }
        protected List<DashboardTaskSummary> DueSoonProjectTasks { get; private set; }
        protected OverviewProjectItem SelectedProject { get; private set; }
        protected bool ShowFinanceSignal { get; private set; }
        protected bool ShowResourceSignal { get; private set; }
        protected bool ShowCustomerSignal { get; private set; }
        protected bool ShowIssueSignal { get; private set; }
        protected bool ShowRiskSignal { get; private set; }
        protected bool ShowProjectCostSummary { get; private set; }
        protected bool ShowProjectResourceSummary { get; private set; }
        protected DashboardCostModel ProjectCostSummary { get; private set; }
        protected DashboardResourceModel ProjectResourceSummary { get; private set; }
        protected DashboardResourceModel AllProjectsResourceSummary { get; private set; }
        protected int ProjectTasksNotStartedCount { get; private set; }
        protected int ProjectTasksInProgressCount { get; private set; }
        protected int ProjectTasksCompletedCount { get; private set; }
        protected int ProjectResourceNoLoadCount { get; private set; }
        protected int ProjectResourceNormalCount { get; private set; }
        protected int ProjectResourceOverloadedCount { get; private set; }
        protected int AllTasksNotStartedCount { get; private set; }
        protected int AllTasksInProgressCount { get; private set; }
        protected int AllTasksCompletedCount { get; private set; }
        protected int AllResourceNoLoadCount { get; private set; }
        protected int AllResourceNormalCount { get; private set; }
        protected int AllResourceOverloadedCount { get; private set; }
        protected int AllFinanceProjectsWithActivityCount
        {
            get
            {
                return Summary == null || Summary.FinanceSummary == null
                    ? 0 : Summary.FinanceSummary.ProjectsWithActivityCount;
            }
        }
        protected decimal PendingCostTotal
        {
            get
            {
                return Summary == null || Summary.PendingCosts == null
                    ? 0m
                    : Summary.PendingCosts.Sum(cost => cost.Amount);
            }
        }

        protected bool IsProjectDashboard
        {
            get
            {
                Guid projectId;
                return Guid.TryParse(Page.Request.QueryString["project"], out projectId);
            }
        }

        protected string ProjectKpiColumnsClass
        {
            get
            {
                int count = 2;
                if (ShowProjectResourceSummary && ProjectResourceSummary != null)
                    count++;
                return "row-cols-xl-" + count;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            new RegisterCSSAndJS("cpHeadVendor", "cpVendorScript",
                new List<string> { CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-style.css?v=44") },
                new List<string> { CURRENT_PAGE.GetRelativeClientPath(
                    "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-project-groups.js?v=1"),
                    CURRENT_PAGE.GetRelativeClientPath(
                    "/Controls/Dashboard/dashboard-modals.js?v=1") }).Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && !IsProjectDashboard)
            {
                LoadProjectFilter();
                LoadDateRangeFilter();
            }

            DashboardFilter filter = BuildOverviewFilter();
            Guid userId = SweetContext.Current.UserId;
            ShowFinanceSignal = !IsProjectDashboard
                && SweetContext.Current.CheckFunctionPermission(
                    userId, ModuleKeys.DashboardCost);
            ShowResourceSignal = !IsProjectDashboard
                && SweetContext.Current.CheckFunctionPermission(
                    userId, ModuleKeys.DashboardResource);
            ShowCustomerSignal = !IsProjectDashboard
                && SweetContext.Current.CheckFunctionPermission(
                    userId, ModuleKeys.Customer);
            ShowIssueSignal = CURRENT_PAGE != null && CURRENT_PAGE.IsUserRight(
                ActionKeys.View, ModuleKeys.Issue);
            ShowRiskSignal = CURRENT_PAGE != null && CURRENT_PAGE.IsUserRight(
                ActionKeys.View, ModuleKeys.Risk);
            ShowProjectCostSummary = IsProjectDashboard
                && SweetContext.Current.CheckFunctionPermission(
                    userId, ModuleKeys.DashboardCost);
            ShowProjectResourceSummary = IsProjectDashboard
                && SweetContext.Current.CheckFunctionPermission(
                    userId, ModuleKeys.DashboardResource);
            Summary = DashboardOverviewManager.Instance.GetSimpleOverview(
                filter, ShowFinanceSignal, ShowIssueSignal, ShowRiskSignal,
                ShowCustomerSignal);
            Summary.Projects = Summary.Projects ?? new List<OverviewProjectItem>();
            Summary.ActiveCustomers = Summary.ActiveCustomers
                ?? new List<OverviewActiveCustomer>();
            Summary.Tasks = Summary.Tasks ?? new List<DashboardTaskSummary>();
            Summary.CurrentTasks = Summary.CurrentTasks
                ?? new List<DashboardTaskSummary>();
            Summary.Meetings = Summary.Meetings ?? new List<UpcomingMeetingSummary>();
            Summary.Statuses = Summary.Statuses ?? new List<ProjectStatusStatistic>();
            Summary.PendingCosts = Summary.PendingCosts ?? new List<OverviewPendingCostItem>();
            Summary.OpenIssues = Summary.OpenIssues ?? new List<OverviewImportantIssue>();
            Summary.ImportantIssues = Summary.ImportantIssues ?? new List<OverviewImportantIssue>();
            Summary.RecordedRisks = Summary.RecordedRisks ?? new List<OverviewRecordedRisk>();
            if (!IsProjectDashboard)
            {
                AllTasksNotStartedCount = Summary.Tasks.Count(
                    t => t.LifecycleStatusCode == 0);
                AllTasksInProgressCount = Summary.Tasks.Count(
                    t => t.LifecycleStatusCode == 1);
                AllTasksCompletedCount = Summary.Tasks.Count(
                    t => t.LifecycleStatusCode == 2);
            }

            if (ShowResourceSignal)
            {
                DashboardResourceModel resource = DashboardResourceManager.Instance
                    .GetResourceDashboard(new DashboardResourceFilter
                    {
                        AnchorWeekStart = DateTime.Today,
                        WeekCount = 2
                    });
                AllProjectsResourceSummary = resource;
                AllResourceNoLoadCount = resource.EmployeeLoads.Count(
                    employee => GetWeekLoadCode(employee, resource.AnchorWeekStart) == 0);
                AllResourceNormalCount = resource.EmployeeLoads.Count(
                    employee => GetWeekLoadCode(employee, resource.AnchorWeekStart) == 1);
                AllResourceOverloadedCount = resource.EmployeeLoads.Count(
                    employee => GetWeekLoadCode(employee, resource.AnchorWeekStart) == 2);
            }

            SelectedProject = filter.ProjectId.HasValue
                ? Summary.Projects.FirstOrDefault(p => p.ProjectId == filter.ProjectId.Value)
                : null;
            if (SelectedProject != null && ShowProjectCostSummary)
            {
                ProjectCostSummary = DashboardCostManager.Instance
                    .GetCostDashboard(new DashboardCostFilter
                    {
                        ProjectId = SelectedProject.ProjectId
                    });
            }
            if (SelectedProject != null && ShowProjectResourceSummary)
            {
                ProjectResourceSummary = DashboardResourceManager.Instance
                    .GetResourceDashboard(new DashboardResourceFilter
                    {
                        ProjectId = SelectedProject.ProjectId,
                        AnchorWeekStart = DateTime.Today,
                        WeekCount = 2
                    });
                ProjectResourceNoLoadCount = ProjectResourceSummary.EmployeeLoads
                    .Count(employee => GetProjectWeekLoadCode(employee) == 0);
                ProjectResourceNormalCount = ProjectResourceSummary.EmployeeLoads
                    .Count(employee => GetProjectWeekLoadCode(employee) == 1);
                ProjectResourceOverloadedCount = ProjectResourceSummary.EmployeeLoads
                    .Count(employee => GetProjectWeekLoadCode(employee) == 2);
            }
            OverdueProjects = Summary.Projects.Where(p => p.IsOverdue).ToList();
            ProjectsWithOverdueTasks = Summary.Projects
                .Where(p => p.OverdueTaskCount > 0).ToList();
            UpcomingProjects = Summary.Projects
                .Where(p => p.StatusCode == (byte)DuAnStatus.DangThucHien
                    && p.ExpectedEndDate.Date >= DateTime.Today
                    && p.ExpectedEndDate.Date <= DateTime.Today.AddDays(7))
                .OrderBy(p => p.ExpectedEndDate)
                .ThenBy(p => p.ProjectCode)
                .ToList();
            PriorityProjects = Summary.Projects.Where(p => p.NeedsAttention)
                .OrderByDescending(p => p.IsOverdue)
                .ThenByDescending(p => p.OverdueTaskCount)
                .ThenByDescending(p => p.ImportantIssueCount)
                .ThenByDescending(p => p.OverdueDays)
                .ThenBy(p => p.ProjectCode).ToList();
            ProjectTasks = Summary.Tasks;
            ProjectTasksNotStartedCount = ProjectTasks.Count(
                t => t.LifecycleStatusCode == 0);
            ProjectTasksInProgressCount = ProjectTasks.Count(
                t => t.LifecycleStatusCode == 1);
            ProjectTasksCompletedCount = ProjectTasks.Count(
                t => t.LifecycleStatusCode == 2);
            CompletedProjectTasks = Summary.Tasks.Where(t => t.IsCompleted).ToList();
            bool isOpenProject = SelectedProject != null
                && SelectedProject.StatusCode != (byte)DuAnStatus.HoanThanh
                && SelectedProject.StatusCode != (byte)DuAnStatus.KetThuc;
            OverdueProjectTasks = isOpenProject
                ? Summary.Tasks.Where(t => t.IsOverdue).ToList()
                : new List<DashboardTaskSummary>();
            DueSoonProjectTasks = Summary.Tasks.Where(t => !t.IsOverdue
                && isOpenProject
                && t.Deadline.HasValue
                && t.Deadline.Value.Date >= DateTime.Today
                && t.Deadline.Value.Date <= DateTime.Today.AddDays(7)
                && !t.IsCompleted).ToList();
        }

        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();
            ddlProjectFilter.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.ALL_PROJECTS), AllProjectsValue));
            foreach (var project in DashboardOverviewManager.Instance.GetProjectsForFilter())
                ddlProjectFilter.Items.Add(new ListItem(
                    project.MaDuAn + " - " + project.TenDuAn,
                    project.IdDuAn.ToString("D")));
        }

        protected void ddlProjectFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Guid projectId;
            string route = Guid.TryParse(ddlProjectFilter.SelectedValue, out projectId)
                ? RewriteURLHelper.DashboardOverviewForProject(projectId)
                : RewriteURLHelper.DashboardOverview;
            Response.Redirect(CURRENT_PAGE.GetRelativeClientPath(route), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected int ActiveProjectCount
        {
            get { return Summary.Projects.Count(p =>
                p.StatusCode == (byte)DuAnStatus.DangThucHien); }
        }

        protected string GetProjectDetailUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectDetail(projectId));
        }

        protected string GetCustomerDetailUrl(Guid customerId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.CustomerDetail(customerId));
        }

        protected string GetProjectTasksUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectTasks(projectId));
        }

        protected string GetTaskDetailUrl(DashboardTaskSummary task)
        {
            return GetProjectTasksUrl(task.ProjectId)
                + "?taskId=" + task.TaskId.ToString("D");
        }

        protected string GetProjectMeetingsUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectMeets(projectId));
        }

        protected string GetProjectCostsUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectCosts(projectId));
        }

        protected string GetProjectIssuesUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectIssues(projectId));
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

        private DashboardFilter BuildOverviewFilter()
        {
            Guid projectId;
            var filter = new DashboardFilter
            {
                ProjectId = Guid.TryParse(
                    Page.Request.QueryString["project"], out projectId)
                    ? (Guid?)projectId : null
            };

            if (!IsProjectDashboard)
            {
                DashboardDateRange dateRange = DashboardDateRange.ThisMonth;
                int parsedDateRange;
                if (ddlDateRange != null
                    && int.TryParse(
                        ddlDateRange.SelectedValue,
                        out parsedDateRange)
                    && Enum.IsDefined(
                        typeof(DashboardDateRange), parsedDateRange))
                {
                    dateRange = (DashboardDateRange)parsedDateRange;
                }

                filter.DateRange = dateRange;
                filter.FromDate = GetFromDate(dateRange);
                filter.ToDate = GetToDate(dateRange);
            }

            return filter;
        }

        protected string SelectedDateRangeText
        {
            get
            {
                return ddlDateRange != null && ddlDateRange.SelectedItem != null
                    ? ddlDateRange.SelectedItem.Text
                    : GetResourceText(BackEndResourceKeys.THIS_MONTH);
            }
        }

        private static DateTime GetFromDate(DashboardDateRange dateRange)
        {
            DateTime today = DateTime.Today;
            switch (dateRange)
            {
                case DashboardDateRange.ThisWeek:
                    int daysSinceMonday = (7 + (int)today.DayOfWeek
                        - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-daysSinceMonday);
                case DashboardDateRange.ThisQuarter:
                    int firstQuarterMonth = ((today.Month - 1) / 3) * 3 + 1;
                    return new DateTime(today.Year, firstQuarterMonth, 1);
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
                case DashboardDateRange.ThisWeek:
                    int daysSinceMonday = (7 + (int)today.DayOfWeek
                        - (int)DayOfWeek.Monday) % 7;
                    return today.AddDays(-daysSinceMonday).AddDays(6);
                case DashboardDateRange.ThisQuarter:
                    int lastQuarterMonth = ((today.Month - 1) / 3) * 3 + 3;
                    return new DateTime(
                        today.Year,
                        lastQuarterMonth,
                        DateTime.DaysInMonth(today.Year, lastQuarterMonth));
                case DashboardDateRange.ThisYear:
                    return new DateTime(today.Year, 12, 31);
                default:
                    return new DateTime(
                        today.Year,
                        today.Month,
                        DateTime.DaysInMonth(today.Year, today.Month));
            }
        }

        protected string GetProjectRisksUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.ProjectRisks(projectId));
        }

        protected string GetProjectIssueDetailUrl(Guid projectId, Guid issueId)
        {
            return GetProjectIssuesUrl(projectId)
                + "?issueId=" + issueId.ToString("D");
        }

        protected string GetProjectRiskDetailUrl(Guid projectId, Guid riskId)
        {
            return GetProjectRisksUrl(projectId)
                + "?riskId=" + riskId.ToString("D");
        }

        protected string GetRiskProbabilityText(int? probability)
        {
            if (!probability.HasValue)
                return "—";

            string level;
            switch (probability.Value)
            {
                case 10: level = "Rất thấp"; break;
                case 25: level = "Thấp"; break;
                case 50: level = "Trung bình"; break;
                case 75: level = "Cao"; break;
                case 90: level = "Rất cao"; break;
                default: level = string.Empty; break;
            }

            return string.IsNullOrEmpty(level)
                ? probability.Value.ToString(CultureInfo.CurrentCulture) + "%"
                : probability.Value.ToString(CultureInfo.CurrentCulture) + "% · " + level;
        }

        protected string GetRiskPlanText(OverviewRecordedRisk risk)
        {
            if (risk == null)
                return "—";

            List<string> plans = new List<string>();
            if (!string.IsNullOrWhiteSpace(risk.PreventionPlan))
                plans.Add("Phòng ngừa: " + risk.PreventionPlan.Trim());
            if (!string.IsNullOrWhiteSpace(risk.ResponsePlan))
                plans.Add("Ứng phó: " + risk.ResponsePlan.Trim());

            return plans.Count == 0 ? "—" : string.Join(" · ", plans);
        }

        protected string GetProjectProgressUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.DashboardProgressForProject(projectId));
        }

        protected string GetProjectCostDashboardUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.DashboardCostForProject(projectId));
        }

        protected string GetProjectResourceDashboardUrl(Guid projectId)
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.DashboardResourceForProject(projectId));
        }

        protected string FormatProjectMoney(decimal amount)
        {
            return amount.ToString("#,##0", CultureInfo.CurrentCulture) + " đ";
        }

        protected int GetProjectCollectionRate()
        {
            return GetCollectionRate(ProjectCostSummary);
        }

        protected int GetCollectionRate(DashboardCostModel costSummary)
        {
            if (costSummary == null)
                return 0;
            decimal total = costSummary.ReceivedPayment
                + costSummary.OutstandingPayment;
            return total <= 0 ? 0 : (int)Math.Round(
                costSummary.ReceivedPayment / total * 100m, 0);
        }

        protected IEnumerable<OverviewProjectFinanceItem> GetAllProjectsFinanceRows()
        {
            return Summary.FinanceSummary.Projects
                .OrderBy(project => project.ProjectCode);
        }

        protected string GetTaskLifecycleStatusText(DashboardTaskSummary task)
        {
            if (task == null)
                return string.Empty;
            switch (task.LifecycleStatusCode)
            {
                case 2:
                    return "Hoàn thành";
                case 1:
                    return "Đang làm";
                default:
                    return "Chưa bắt đầu";
            }
        }

        protected ResourceWeeklyLoad GetProjectWeekLoad(ResourceEmployeeLoad employee)
        {
            return ProjectResourceSummary == null || employee == null
                ? null
                : employee.WeeklyLoads.FirstOrDefault(week =>
                    week.WeekStart == ProjectResourceSummary.AnchorWeekStart);
        }

        protected int GetProjectWeekLoadCode(ResourceEmployeeLoad employee)
        {
            return GetWeekLoadCode(employee, ProjectResourceSummary.AnchorWeekStart);
        }

        protected ResourceWeeklyLoad GetAllProjectsWeekLoad(ResourceEmployeeLoad employee)
        {
            return employee.WeeklyLoads.FirstOrDefault(week =>
                week.WeekStart == AllProjectsResourceSummary.AnchorWeekStart);
        }

        protected int GetAllProjectsWeekLoadCode(ResourceEmployeeLoad employee)
        {
            return GetWeekLoadCode(employee, AllProjectsResourceSummary.AnchorWeekStart);
        }

        protected string GetAllProjectsWeekProjectNames(ResourceEmployeeLoad employee)
        {
            ResourceWeeklyLoad week = GetAllProjectsWeekLoad(employee);
            return week == null ? string.Empty : string.Join(", ",
                week.Projects.Select(project =>
                    project.ProjectCode + " · " + project.ProjectName));
        }

        private static int GetWeekLoadCode(ResourceEmployeeLoad employee, DateTime weekStart)
        {
            ResourceWeeklyLoad week = employee.WeeklyLoads.FirstOrDefault(
                item => item.WeekStart == weekStart);
            if (week == null || week.PeakDailyTaskCount <= 0)
                return 0;
            return week.PeakDailyTaskCount == 1 ? 1 : 2;
        }

        protected string GetProjectWeekLoadText(int code)
        {
            switch (code)
            {
                case 1: return "Bình thường";
                case 2: return "Quá tải";
                default: return "Rảnh";
            }
        }

        protected string GetProjectResourceEmployeeUrl(Guid employeeId)
        {
            return GetProjectResourceDashboardUrl(SelectedProject.ProjectId)
                + "&resourceWeek=" + ProjectResourceSummary.AnchorWeekStart.ToString("yyyy-MM-dd")
                + "&resourceEmployee=" + employeeId.ToString("D");
        }

        protected string GetIssueImpactText(int impactLevel)
        {
            switch (impactLevel)
            {
                case 1: return GetResourceText(BackEndResourceKeys.VERY_LOW);
                case 2: return GetResourceText(BackEndResourceKeys.LOW);
                case 3: return GetResourceText(BackEndResourceKeys.MEDIUM);
                case 4: return GetResourceText(BackEndResourceKeys.HIGH);
                case 5: return GetResourceText(BackEndResourceKeys.VERY_HIGH);
                default: return "—";
            }
        }

        protected string GetEmployeeResourceUrl(Guid employeeId)
        {
            DashboardResourceModel resource = IsProjectDashboard
                ? ProjectResourceSummary : AllProjectsResourceSummary;
            DateTime weekStart = resource == null
                ? DateTime.Today : resource.AnchorWeekStart;
            return GetResourceDashboardUrl()
                + "?resourceWeek=" + weekStart.ToString("yyyy-MM-dd")
                + "&resourceEmployee=" + employeeId.ToString("D");
        }

        protected string GetResourceDashboardUrl()
        {
            return CURRENT_PAGE.GetRelativeClientPath(
                RewriteURLHelper.DashboardResource);
        }

        protected string FormatMoney(decimal amount)
        {
            return amount.ToString("#,##0", CultureInfo.CurrentCulture)
                + GetResourceText(BackEndResourceKeys.DASHBOARD_CURRENCY_SUFFIX);
        }
    }
}
