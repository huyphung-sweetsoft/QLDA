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
                        "/Controls/Dashboard/dashboard-style.css?v=47")
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
                        "/Controls/Dashboard/dashboard-cost.js?v=19"),
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

        protected int OverBudgetProjectCount
        {
            get
            {
                return Model == null || Model.ProjectStatistics == null
                    ? 0
                    : Model.ProjectStatistics.Count(x =>
                        x.CostComparisonStatus == "over");
            }
        }

        protected string FormatSignedMoney(decimal value)
        {
            return (value > 0 ? "+" : string.Empty) + FormatMoney(value);
        }

        protected string FormatSignedMoneySummary(decimal value)
        {
            string sign = value > 0 ? "+" : value < 0 ? "-" : string.Empty;
            return sign + FormatMoneySummary(Math.Abs(value));
        }

        protected string FormatSignedPercent(decimal? value)
        {
            if (!value.HasValue)
            {
                return "-";
            }

            return (value.Value > 0 ? "+" : string.Empty)
                + value.Value.ToString("0.##", CultureInfo.CurrentCulture)
                + "%";
        }

        protected string GetProjectCostStatusCss(string status)
        {
            switch (status)
            {
                case "over":
                    return "bg-danger-subtle text-danger";
                case "under":
                    return "bg-success-subtle text-success";
                case "equal":
                    return "bg-primary-subtle text-primary";
                default:
                    return "bg-secondary-subtle text-secondary";
            }
        }

        protected string GetProjectCostStatusText(string status)
        {
            switch (status)
            {
                case "over":
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_COST_OVER);
                case "under":
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_COST_UNDER);
                case "equal":
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_COST_EQUAL);
                default:
                    return GetResourceText(
                        BackEndResourceKeys.DASHBOARD_PROJECT_COST_NO_CONTRACT);
            }
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

            List<ProjectCostStatistic> comparisonProjects =
                Model.ProjectStatistics ?? new List<ProjectCostStatistic>();
            ProjectComparisonCount = comparisonProjects.Count;

            ProjectComparisonChartData = JsonConvert.SerializeObject(
                new
                {
                    projects = comparisonProjects.Select(x => new
                    {
                        code = x.ProjectCode,
                        name = x.ProjectName,
                        hasContractValue = x.HasContractValue,
                        status = x.CostComparisonStatus,
                        variance = x.CostVariance
                    }).ToList()
                }).Replace("</", "<\\/");

            CostApprovalChartData = JsonConvert.SerializeObject(new
            {
                approved = new
                {
                    amount = Model.ActualCost,
                    count = Model.ApprovedCostItemCount
                },
                pending = new
                {
                    amount = Model.PendingApprovalCost,
                    count = Model.PendingApprovalCostItemCount
                },
                rejected = new
                {
                    amount = Model.RejectedCost,
                    count = Model.RejectedCostItemCount
                },
                totalAmount = Model.ActualCost + Model.PendingApprovalCost
                    + Model.RejectedCost,
                totalCount = Model.CostItemCount
            });

            PaymentChartData = JsonConvert.SerializeObject(new
            {
                paid = new
                {
                    amount = Model.PaymentItems.Where(x => x.IsPaid
                        && !x.IsPaidLate).Sum(x => x.Amount),
                    count = Model.PaymentItems.Count(x => x.IsPaid
                        && !x.IsPaidLate)
                },
                paidLate = new
                {
                    amount = Model.PaymentItems.Where(x => x.IsPaidLate)
                        .Sum(x => x.Amount),
                    count = Model.PaidLatePaymentCount
                },
                dueToday = new
                {
                    amount = Model.PaymentItems.Where(x =>
                        x.StatusCategory == "due-today").Sum(x => x.Amount),
                    count = Model.DueTodayPaymentCount
                },
                upcoming = new
                {
                    amount = Model.PaymentItems.Where(x =>
                        x.StatusCategory == "upcoming").Sum(x => x.Amount),
                    count = Model.UpcomingPaymentCount
                },
                overdue = new
                {
                    amount = Model.PaymentItems.Where(x => x.IsOverdue)
                        .Sum(x => x.Amount),
                    count = Model.OverduePaymentCount
                },
                noDueDate = new
                {
                    amount = Model.PaymentItems.Where(x =>
                        x.StatusCategory == "no-date").Sum(x => x.Amount),
                    count = Model.PaymentWithoutDueDateCount
                },
                totalAmount = Model.PaymentItems.Sum(x => x.Amount),
                totalCount = Model.PaymentItems.Count
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
                rejectedCost = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_REJECTED_COST),
                noContractOrPayment = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_NO_CONTRACT_OR_PAYMENT),
                received = GetResourceText(
                    BackEndResourceKeys.RECEIVED_PAYMENT),
                paid = GetResourceText(
                    BackEndResourceKeys.PAYMENT_PAID),
                paidLate = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_PAID_LATE),
                paidLateStatus = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_PAID_LATE_STATUS),
                dueToday = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_DUE_TODAY),
                upcoming = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_NOT_DUE),
                noDueDate = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_NO_DUE_DATE),
                overdue = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_OVERDUE_UNPAID),
                paymentUnit = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_UNIT),
                costUnit = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COST_UNIT),
                totalPayments = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TOTAL_PAYMENT_COUNT),
                totalCosts = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TOTAL_COST_COUNT),
                totalProjects = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_TOTAL_PROJECT_COUNT),
                projectUnit = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_UNIT),
                projectCostOver = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_OVER),
                projectCostUnder = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_UNDER),
                projectCostEqual = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_EQUAL),
                projectCostNoContract = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_NO_CONTRACT),
                projectCostTotalDeviation = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_TOTAL_DEVIATION),
                projectCostAverageDeviation = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_AVERAGE_DEVIATION),
                projectCostNoComparableData = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_NO_COMPARABLE_DATA),
                totalOverrun = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_TOTAL_OVERRUN),
                totalSaving = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_TOTAL_SAVING),
                budgetComparisonEmpty = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_BUDGET_COMPARISON_EMPTY),
                projectCostExpected = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_EXPECTED),
                projectCostActual = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_ACTUAL),
                projectCostVariance = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_VARIANCE),
                projectCostVariancePercent = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROJECT_COST_VARIANCE_PERCENT),
                expectedProfit = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EXPECTED_GROSS_PROFIT),
                expectedProfitMargin = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EXPECTED_PROFIT_MARGIN),
                paymentCountChart = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PAYMENT_COUNT_CHART),
                costCountChart = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_COST_COUNT_CHART),
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
