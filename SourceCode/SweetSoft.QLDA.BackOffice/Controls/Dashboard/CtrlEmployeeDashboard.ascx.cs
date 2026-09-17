using System;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Dashboard;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;

namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlEmployeeDashboard : BaseAdminUserControl
    {
        protected EmployeeDashboardModel Model { get; private set; }
        protected string ProjectChartDataJson { get; private set; }
        protected string DashboardTextsJson { get; private set; }
        protected string CurrentUserName { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropdowns();
                LoadData();
            }
        }

        private void BindDropdowns()
        {
            ddlTimeFilter.Items.Clear();
            ddlTimeFilter.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_MONTH), "month"));
            ddlTimeFilter.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_QUARTER), "quarter"));
            ddlTimeFilter.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.THIS_YEAR), "year"));
            ddlTimeFilter.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.DASHBOARD_ALL_TIME), "all"));
            ddlTimeFilter.SelectedValue = "year"; // Default to this year

            ddlProject.Items.Clear();
            ddlProject.Items.Add(new ListItem(
                GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EMPLOYEE_ALL_PROJECTS),
                ""));

            DashboardUserContext context = GetDashboardUserContext();
            if (!context.EmployeeId.HasValue)
            {
                return;
            }

            foreach (var project in EmployeeDashboardManager.Instance
                .GetProjectsForEmployee(context.EmployeeId.Value))
            {
                ddlProject.Items.Add(new ListItem(
                    string.Format("{0} - {1}", project.MaDuAn, project.TenDuAn),
                    project.IdDuAn.ToString()));
            }
        }

        protected void ddlFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var user = SweetContext.Current.User;
            CurrentUserName = user != null
                ? user.DisplayName
                : GetResourceText(
                    BackEndResourceKeys.DASHBOARD_EMPLOYEE_DEFAULT_NAME);

            DashboardUserContext context = GetDashboardUserContext();
            if (context.EmployeeId.HasValue)
            {
                var nhanVien = EmployeeDashboardManager.Instance
                    .GetEmployeeByUserId(context.UserId);
                if (nhanVien != null)
                {
                    CurrentUserName = nhanVien.DisplayName;
                }
            }
            
            var filter = new DashboardFilter();
            
            string time = ddlTimeFilter.SelectedValue;
            var today = DateTime.Today;
            switch(time)
            {
                case "month":
                    filter.FromDate = new DateTime(today.Year, today.Month, 1);
                    filter.ToDate = filter.FromDate.AddMonths(1).AddDays(-1);
                    break;
                case "quarter":
                    int q = (today.Month - 1) / 3 + 1;
                    filter.FromDate = new DateTime(today.Year, (q - 1) * 3 + 1, 1);
                    filter.ToDate = filter.FromDate.AddMonths(3).AddDays(-1);
                    break;
                case "year":
                    filter.FromDate = new DateTime(today.Year, 1, 1);
                    filter.ToDate = new DateTime(today.Year, 12, 31);
                    break;
                case "all":
                default:
                    filter.FromDate = DateTime.MinValue;
                    filter.ToDate = DateTime.MaxValue;
                    break;
            }

            if (!string.IsNullOrEmpty(ddlProject.SelectedValue) && Guid.TryParse(ddlProject.SelectedValue, out Guid pid))
            {
                filter.ProjectId = pid;
            }

            Model = context.EmployeeId.HasValue
                ? EmployeeDashboardManager.Instance.GetEmployeeOverview(
                    context.EmployeeId.Value,
                    filter)
                : new EmployeeDashboardModel();

            ProjectChartDataJson = JsonConvert.SerializeObject(Model.MyProjects.Select(x => new
            {
                code = x.ProjectCode,
                progress = x.Progress
            })).Replace("</", "<\\/");

            DashboardTextsJson = JsonConvert.SerializeObject(new
            {
                progress = GetResourceText(
                    BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN)
            }).Replace("</", "<\\/");
        }

        private DashboardUserContext GetDashboardUserContext()
        {
            var user = SweetContext.Current.User;
            if (user == null)
            {
                return new DashboardUserContext();
            }

            return EmployeeDashboardManager.Instance.GetUserContext(
                user.UserId,
                SweetContext.Current.IsAdministrator);
        }
    }
}
