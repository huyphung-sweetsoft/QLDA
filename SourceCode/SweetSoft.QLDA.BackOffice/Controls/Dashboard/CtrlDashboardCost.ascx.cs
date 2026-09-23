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
        private const string AllCompletedProjectsValue =
            "__all_completed_projects__";

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
                        "/Controls/Dashboard/dashboard-cost.js?v=2")
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

        protected string CostTrendChartData { get; private set; }

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
            ddlCompletionPeriod.AutoPostBack = IsProjectDashboard;
            btnApplyCostFilter.Visible = !IsProjectDashboard;

            if (!IsPostBack)
            {
                if (!IsProjectDashboard)
                {
                    btnApplyCostFilter.Text =
                        GetResourceText(BackEndResourceKeys.APPLY);
                    LoadProjectFilter();
                }

                LoadCompletionPeriodFilter();
                InitDashboard(BuildCostFilter());
            }
        }

        protected void btnApplyCostFilter_Click(
            object sender,
            EventArgs e)
        {
            InitDashboard(BuildCostFilter());
        }

        protected void ddlCompletionPeriod_SelectedIndexChanged(
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

        protected string GetSelectedPeriodText()
        {
            ListItem selectedItem = ddlCompletionPeriod.SelectedItem;
            return selectedItem == null
                ? GetResourceText(BackEndResourceKeys.DASHBOARD_ALL_TIME)
                : selectedItem.Text;
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

            ProjectComparisonChartData = JsonConvert.SerializeObject(
                Model.ProjectStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
                    detailUrl = GetProjectDetailUrl(x.ProjectId),
                    contractValue = x.ContractValue,
                    actualCost = x.ActualCost,
                    grossProfit = x.GrossProfit,
                    profitMargin = x.ProfitMargin
                }));

            CostTrendChartData = JsonConvert.SerializeObject(
                Model.CostTrendStatistics.Select(x => new
                {
                    month = x.Month.ToString("MM/yyyy"),
                    amount = x.Amount
                }));

            PaymentChartData = JsonConvert.SerializeObject(new
            {
                received = Model.ReceivedPayment,
                outstanding = Model.OutstandingPayment
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
                noCompletedProjectComparison = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_COMPLETED_PROJECT_COMPARISON),
                contractValue = GetResourceText(
                    BackEndResourceKeys.TOTAL_CONTRACT_VALUE),
                actualCost = GetResourceText(
                    BackEndResourceKeys.ACTUAL_COST),
                noContractOrPayment = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_CONTRACT_OR_PAYMENT),
                received = GetResourceText(
                    BackEndResourceKeys.RECEIVED_PAYMENT),
                outstanding = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_OUTSTANDING_PAYMENT),
                noCostTrend = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_COST_TREND),
                incurredCost = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_INCURRED_COST)
            }).Replace("</", "<\\/");
        }

        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();
            ListItem allCompletedProjects = new ListItem(
                GetResourceText(
                    BackEndResourceKeys.DASHBOARD_ALL_COMPLETED_PROJECTS),
                AllCompletedProjectsValue);
            allCompletedProjects.Selected = true;
            ddlProjectFilter.Items.Add(allCompletedProjects);

            foreach (var project in
                DashboardCostManager.Instance.GetCompletedProjectsForFilter())
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

        private void LoadCompletionPeriodFilter()
        {
            ddlCompletionPeriod.Items.Clear();

            ListItem allTime = new ListItem(
                GetResourceText(BackEndResourceKeys.DASHBOARD_ALL_TIME),
                ((int)DashboardCostPeriod.AllTime).ToString(
                    CultureInfo.InvariantCulture));
            allTime.Selected = true;
            ddlCompletionPeriod.Items.Add(allTime);

            ddlCompletionPeriod.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_MONTH),
                ((int)DashboardCostPeriod.ThisMonth).ToString(
                    CultureInfo.InvariantCulture)));
            ddlCompletionPeriod.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_QUARTER),
                ((int)DashboardCostPeriod.ThisQuarter).ToString(
                    CultureInfo.InvariantCulture)));
            ddlCompletionPeriod.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_YEAR),
                ((int)DashboardCostPeriod.ThisYear).ToString(
                    CultureInfo.InvariantCulture)));
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

            DashboardCostPeriod period = DashboardCostPeriod.AllTime;
            int parsedPeriod;

            if (int.TryParse(
                ddlCompletionPeriod.SelectedValue,
                out parsedPeriod)
                && Enum.IsDefined(typeof(DashboardCostPeriod), parsedPeriod))
            {
                period = (DashboardCostPeriod)parsedPeriod;
            }

            DateTime? completedFrom;
            DateTime? completedTo;
            GetCompletionDateRange(period, out completedFrom, out completedTo);

            return new DashboardCostFilter
            {
                ProjectId = projectId,
                Period = period,
                CompletedFrom = completedFrom,
                CompletedTo = completedTo
            };
        }

        private static void GetCompletionDateRange(
            DashboardCostPeriod period,
            out DateTime? completedFrom,
            out DateTime? completedTo)
        {
            DateTime today = DateTime.Today;
            completedFrom = null;
            completedTo = null;

            switch (period)
            {
                case DashboardCostPeriod.ThisMonth:
                    completedFrom = new DateTime(today.Year, today.Month, 1);
                    completedTo = new DateTime(
                        today.Year,
                        today.Month,
                        DateTime.DaysInMonth(today.Year, today.Month));
                    break;
                case DashboardCostPeriod.ThisQuarter:
                    int startMonth = ((today.Month - 1) / 3) * 3 + 1;
                    int endMonth = startMonth + 2;
                    completedFrom = new DateTime(today.Year, startMonth, 1);
                    completedTo = new DateTime(
                        today.Year,
                        endMonth,
                        DateTime.DaysInMonth(today.Year, endMonth));
                    break;
                case DashboardCostPeriod.ThisYear:
                    completedFrom = new DateTime(today.Year, 1, 1);
                    completedTo = new DateTime(today.Year, 12, 31);
                    break;
            }
        }
    }
}
