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
                        "/Controls/Dashboard/dashboard-style.css")
                };

                List<string> jsLinks = new List<string>
                {
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Styles/plugins/apexcharts/apexcharts.min.js"),
                    CURRENT_PAGE.GetRelativeClientPath(
                        "/Controls/Dashboard/dashboard-cost.js")
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnApplyCostFilter.Text =
                    GetResourceText(BackEndResourceKeys.APPLY);
                LoadProjectFilter();
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

        protected string FormatMoney(decimal value)
        {
            return value.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN"))
                + " đ";
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
                ? "Tất cả thời gian"
                : selectedItem.Text;
        }

        private void InitDashboard(DashboardCostFilter filter)
        {
            Model = DashboardCostManager.Instance.GetCostDashboard(filter);

            ProjectComparisonChartData = JsonConvert.SerializeObject(
                Model.ProjectStatistics.Select(x => new
                {
                    code = x.ProjectCode,
                    name = x.ProjectName,
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
        }

        private void LoadProjectFilter()
        {
            ddlProjectFilter.Items.Clear();
            ListItem allCompletedProjects = new ListItem(
                "Tất cả dự án đã hoàn thành",
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
        }

        private void LoadCompletionPeriodFilter()
        {
            ddlCompletionPeriod.Items.Clear();

            ListItem allTime = new ListItem(
                "Tất cả thời gian",
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

            if (!string.IsNullOrEmpty(ddlProjectFilter.SelectedValue)
                && Guid.TryParse(
                    ddlProjectFilter.SelectedValue,
                    out parsedProjectId))
            {
                projectId = parsedProjectId;
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
