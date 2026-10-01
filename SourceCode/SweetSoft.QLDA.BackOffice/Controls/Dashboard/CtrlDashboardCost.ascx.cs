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
    public partial class CtrlDashboardCost : BaseAdminUserControl
    {
        private const string AllProjectsValue = "__all_projects__";

        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-style.css?v=44")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-donut.js?v=1"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-project-groups.js?v=1"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-cost.js?v=10"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-modals.js?v=1")
                };

                return new RegisterCSSAndJS(
                    "cpHeadVendor",
                    "cpVendorScript",
                    cssLinks,
                    jsLinks);
            }
        }

        protected DashboardCostModel Model { get; private set; }

        protected string ProjectComparisonChartData { get; private set; }

        protected int ProjectComparisonCount { get; private set; }

        protected string CostApprovalChartData { get; private set; }

        protected string PaymentChartData { get; private set; }

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlProjectFilter.AutoPostBack = !IsProjectDashboard;

            if (!IsPostBack)
            {
                if (!IsProjectDashboard)
                {
                    LoadProjectFilter();
                }

                InitDashboard(BuildCostFilter());
            }
        }

        protected void ddlProjectFilter_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildCostFilter());
        }

        protected string FormatMoney(decimal value)
        {
            return value.ToString("#,##0", CultureInfo.CurrentCulture)
                + GetResourceText(
                    BackEndResourceKeys.DASHBOARD_CURRENCY_SUFFIX);
        }

        protected string FormatMoneySummary(decimal value)
        {
            decimal absolute = Math.Abs(value);
            if (absolute >= 1000000000m)
            {
                return (value / 1000000000m).ToString("0.##",
                    CultureInfo.CurrentCulture) + GetResourceText(
                        BackEndResourceKeys.DASHBOARD_BILLION_SUFFIX);
            }

            if (absolute >= 1000000m)
            {
                return (value / 1000000m).ToString("0.##",
                    CultureInfo.CurrentCulture) + GetResourceText(
                        BackEndResourceKeys.DASHBOARD_MILLION_SUFFIX);
            }

            return FormatMoney(value);
        }

        protected string GetAmountCss(decimal amount)
        {
            if (amount > 0)
            {
                return "text-success";
            }

            if (amount < 0)
            {
                return "text-danger";
            }

            return "text-muted";
        }

        protected string GetProfitBadgeCss(decimal grossProfit)
        {
            if (grossProfit > 0)
            {
                return "bg-success-subtle text-success";
            }

            if (grossProfit < 0)
            {
                return "bg-danger-subtle text-danger";
            }

            return "bg-secondary-subtle text-secondary";
        }

        protected string GetCostByProjectDescription()
        {
            if (ProjectComparisonCount == 0)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COST_BY_PROJECT_DESC_EMPTY);
            }

            if (ProjectComparisonCount == 1)
            {
                return GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COST_BY_PROJECT_DESC_SINGLE);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COST_BY_PROJECT_DESC),
                ProjectComparisonCount);
        }

        protected string GetProjectDetailUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectDetail);
        }

        protected string GetProjectPaymentsUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectPayments);
        }

        protected string GetProjectCostsUrl(Guid projectId)
        {
            return GetProjectUrl(projectId, RewriteURLHelper.ProjectCosts);
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

        private void InitDashboard(DashboardCostFilter filter)
        {
            Model = DashboardCostManager.Instance.GetCostDashboard(filter);

            var comparisonProjects = Model.ProjectStatistics
                .Where(x => x.ActualCost > 0)
                .OrderByDescending(x => x.ActualCost)
                .Take(8)
                .ToList();
            ProjectComparisonCount = comparisonProjects.Count;

            ProjectComparisonChartData = JsonConvert.SerializeObject(
                comparisonProjects.Select(x => new
                {
                    projectId = x.ProjectId,
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    actualCost = x.ActualCost
                })).Replace("</", "<\\/");

            CostApprovalChartData = JsonConvert.SerializeObject(new
            {
                approved = Model.ActualCost,
                pending = Model.PendingApprovalCost
            });

            PaymentChartData = JsonConvert.SerializeObject(new
            {
                received = Model.ReceivedPayment,
                outstanding = Model.OutstandingPayment,
                totalContractValue = Model.TotalContractValue
            });

            DashboardTextsJson = JsonConvert.SerializeObject(new
            {
                locale = CultureInfo.CurrentUICulture.Name,
                currencySuffix = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_CURRENCY_SUFFIX),
                billionSuffix = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_BILLION_SUFFIX),
                millionSuffix = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_MILLION_SUFFIX),
                noCostItems = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_COST_ITEMS),
                approvedCost = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_APPROVED_COST),
                pendingCost = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PENDING_COST),
                noContractOrPayment = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_CONTRACT_OR_PAYMENT),
                received = GetResourceText(
                    BackEndResourceKeys.RECEIVED_PAYMENT),
                outstanding = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_OUTSTANDING_PAYMENT),
                contractValue = GetResourceText(
                    BackEndResourceKeys.CONTRACT_VALUE),
                totalRecordedCost = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TOTAL_RECORDED_COST)
            }).Replace("</", "<\\/");
        }

        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();
            ListItem allProjects = new ListItem(
                GetResourceText(
                    BackEndResourceKeys.DASHBOARD_ALL_COMPLETED_PROJECTS),
                AllProjectsValue);
            allProjects.Selected = true;
            ddlProjectFilter.Items.Add(allProjects);

            foreach (var project in
                DashboardCostManager.Instance.GetProjectsForFilter())
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

        private DashboardCostFilter BuildCostFilter()
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

            return new DashboardCostFilter
            {
                ProjectId = projectId
            };
        }
    }
}
