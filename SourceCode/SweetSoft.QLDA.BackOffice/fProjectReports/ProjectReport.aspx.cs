using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;

namespace SweetSoft.QLDA.BackOffice.fProjectReports
{
    public partial class ProjectReport : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.ProjectReport;

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            if (!IsPostBack)
            {
                BindPeriodDropdown();
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_REPORT));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROJECT_REPORT);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.PROJECT_REPORT) }
                };

                LoadReportData();
            }
        }

        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReportData();
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            LoadReportData();
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
        }

        private string RenderControlToHtml(Control control)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    control.RenderControl(htw);
                }
            }
            return sb.ToString();
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            try
            {
                string htmlBody = RenderControlToHtml(phReportContent);

                string cssStyles = @"
                    <style>
                        body { font-family: Arial, sans-serif; }
                        .report-header { text-align: center; margin-bottom: 20px; }
                        .report-header h1 { font-size: 24px; color: #0f172a; text-transform: uppercase; }
                        .sub-date { font-size: 14px; color: #64748b; font-style: italic; }
                        
                        .kpi-summary-bar { display: table; width: 100%; border-collapse: collapse; margin-bottom: 30px; border: 1px solid #cbd5e1; }
                        .kpi-item { display: table-cell; width: 25%; text-align: center; border-right: 1px solid #cbd5e1; padding: 15px; vertical-align: middle; }
                        .kpi-title { font-size: 12px; color: #64748b; display: block; margin-bottom: 5px; text-transform: uppercase; }
                        .kpi-number { font-size: 24px; font-weight: bold; display: block; }
                        
                        .kpi-number.total { color: #3b82f6; }
                        .kpi-number.success { color: #10b981; }
                        .kpi-number.danger { color: #ef4444; }
                        .kpi-number.warning { color: #f59e0b; }

                        .section-title { font-size: 16px; font-weight: bold; margin-top: 20px; margin-bottom: 10px; color: #0f172a; }
                        
                        .table { width: 100%; border-collapse: collapse; margin-bottom: 20px; font-size: 13px; }
                        .table th, .table td { border: 1px solid #cbd5e1; padding: 8px; vertical-align: middle; }
                        .table th { background-color: #f8fafc; font-weight: bold; text-align: center; }
                        .text-center { text-align: center; }
                        .text-start { text-align: left; }
                        .text-danger { color: #dc2626; }
                        .text-muted { color: #64748b; }
                        
                        .report-badge { padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: bold; display:inline-block; }
                        .badge-success { background-color: #dcfce7; color: #166534; border: 1px solid #bbf7d0;}
                        .badge-doing { background-color: #e0f2fe; color: #0369a1; border: 1px solid #bae6fd;}
                        .badge-todo { background-color: #f1f5f9; color: #475569; border: 1px solid #e2e8f0;}
                    </style>";

                string fullHtml = cssStyles + htmlBody;
                string fileName = $"BaoCaoTienDo_{DateTime.Now:ddMMyyyy_HHmm}.pdf";

                PdfManager.Instance.ExportHtmlToPdf(fullHtml, fileName, Response);
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi xuất PDF: " + ex.Message, MSGType.Error);
            }
        }

        private void BindPeriodDropdown()
        {
            ddlPeriod.Items.Clear();
            ddlPeriod.Items.Add(new ListItem(GetResourceText(BackEndResourceKeys.THIS_WEEK), "THIS_WEEK"));
            ddlPeriod.Items.Add(new ListItem(GetResourceText(BackEndResourceKeys.LAST_WEEK), "LAST_WEEK"));
            ddlPeriod.Items.Add(new ListItem(GetResourceText(BackEndResourceKeys.THIS_MONTH), "THIS_MONTH"));
            ddlPeriod.Items.Add(new ListItem(GetResourceText(BackEndResourceKeys.LAST_MONTH), "LAST_MONTH"));

            ListItem itemAll = new ListItem(GetResourceText(BackEndResourceKeys.ALL_TIME), "ALL");
            itemAll.Selected = true;
            ddlPeriod.Items.Add(itemAll);

            ddlPeriod.Items.Add(new ListItem(GetResourceText(BackEndResourceKeys.CUSTOM), "CUSTOM"));
        }

        private void LoadReportData()
        {
            Guid projectId = CurrentProjectId;

            DataTable dtProject = ProjectReportManager.Instance.GetProjectInfo(projectId);

            if (dtProject.Rows.Count == 0) return;

            int projectStatus = dtProject.Rows[0]["TrangThai"] != DBNull.Value ? Convert.ToInt32(dtProject.Rows[0]["TrangThai"]) : 0;

            if (projectStatus == 0)
            {
                phReportContent.Visible = false;
                phNotStarted.Visible = true;
                btnExportPDF.Visible = false;
                return;
            }
            else
            {
                phReportContent.Visible = true;
                phNotStarted.Visible = false;
                btnExportPDF.Visible = true;
            }

            DateTime? projStartDate = dtProject.Rows[0]["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(dtProject.Rows[0]["NgayBatDau"]) : (DateTime?)null;
            DateTime minLimitDate = projStartDate ?? new DateTime(2020, 1, 1);

            DateTime maxLimitDate = DateTime.Today;

            try
            {
                DataTable dt_AllTasks = GanttChartManager.Instance.GetGanttTasks(projectId);
                if (dt_AllTasks != null && dt_AllTasks.Rows.Count > 0)
                {
                    DateTime maxTaskDate = DateTime.MinValue;
                    foreach (DataRow row in dt_AllTasks.Rows)
                    {
                        if (row.Table.Columns.Contains("NgayKetThuc") && row["NgayKetThuc"] != DBNull.Value)
                        {
                            DateTime expectedDate = Convert.ToDateTime(row["NgayKetThuc"]).Date;
                            if (expectedDate > maxTaskDate) maxTaskDate = expectedDate;
                        }

                        if (row.Table.Columns.Contains("NgayHoanThanhThucTe") && row["NgayHoanThanhThucTe"] != DBNull.Value)
                        {
                            DateTime actualDate = Convert.ToDateTime(row["NgayHoanThanhThucTe"]).Date;
                            if (actualDate > maxTaskDate) maxTaskDate = actualDate;
                        }
                    }

                    if (maxTaskDate != DateTime.MinValue)
                    {
                        maxLimitDate = maxTaskDate;
                    }
                }
                else
                {
                    DateTime? projExpectedEndDate = dtProject.Columns.Contains("NgayKetThuc") && dtProject.Rows[0]["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(dtProject.Rows[0]["NgayKetThuc"]) : (DateTime?)null;
                    DateTime? projActualEndDate = dtProject.Columns.Contains("NgayHoanThanhThucTe") && dtProject.Rows[0]["NgayHoanThanhThucTe"] != DBNull.Value ? Convert.ToDateTime(dtProject.Rows[0]["NgayHoanThanhThucTe"]) : (DateTime?)null;

                    if (projectStatus == 1)
                        maxLimitDate = projExpectedEndDate ?? DateTime.Today.AddYears(5);
                    else
                        maxLimitDate = projActualEndDate ?? DateTime.Today;
                }
            }
            catch
            {
                maxLimitDate = DateTime.Today.AddYears(2);
            }

            if (maxLimitDate < minLimitDate)
            {
                maxLimitDate = minLimitDate.AddMonths(1);
            }

            txtFromDate.Attributes["min"] = minLimitDate.ToString("yyyy-MM-dd");
            txtFromDate.Attributes["max"] = maxLimitDate.ToString("yyyy-MM-dd");
            txtToDate.Attributes["min"] = minLimitDate.ToString("yyyy-MM-dd");
            txtToDate.Attributes["max"] = maxLimitDate.ToString("yyyy-MM-dd");

            DateTime? fromDate = null;
            DateTime? toDate = null;
            string periodName = ddlPeriod.SelectedItem.Text;
            DateTime today = DateTime.Today;

            switch (ddlPeriod.SelectedValue)
            {
                case "THIS_WEEK":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = today.AddDays(-1 * diff).Date;
                    toDate = fromDate.Value.AddDays(6).Date;
                    periodName = $"Tuần này ({fromDate.Value:dd/MM} - {toDate.Value:dd/MM})";
                    break;
                case "LAST_WEEK":
                    int diff2 = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = today.AddDays(-1 * diff2).AddDays(-7).Date;
                    toDate = fromDate.Value.AddDays(6).Date;
                    periodName = $"Tuần trước ({fromDate.Value:dd/MM} - {toDate.Value:dd/MM})";
                    break;
                case "THIS_MONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    periodName = $"Tháng {today.Month}/{today.Year}";
                    break;
                case "LAST_MONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    periodName = $"Tháng {fromDate.Value.Month}/{fromDate.Value.Year}";
                    break;
                case "ALL":
                    fromDate = minLimitDate;
                    toDate = maxLimitDate;
                    periodName = $"Toàn thời gian ({fromDate.Value:dd/MM/yyyy} - {toDate.Value:dd/MM/yyyy})";
                    break;
                case "CUSTOM":
                    if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
                    {
                        DateTime tempFrom, tempTo;
                        bool isValidFrom = DateTime.TryParse(txtFromDate.Text, out tempFrom);
                        bool isValidTo = DateTime.TryParse(txtToDate.Text, out tempTo);

                        if (!isValidFrom || !isValidTo)
                        {
                            ShowNotify("Định dạng ngày không hợp lệ!", MSGType.Error);
                            return;
                        }

                        fromDate = tempFrom;
                        toDate = tempTo;

                        if (fromDate > toDate)
                        {
                            ShowNotify("Thời gian không hợp lệ!", MSGType.Error);
                            return;
                        }

                        if (fromDate < minLimitDate) fromDate = minLimitDate;
                        if (toDate > maxLimitDate) toDate = maxLimitDate;

                        periodName = string.Format(
                            GetResourceText(BackEndResourceKeys.FROM_TO_FORMAT),
                            fromDate.Value.ToString("dd/MM/yyyy"),
                            toDate.Value.ToString("dd/MM/yyyy")
                        );
                    }
                    else
                    {
                        string eventTarget = Request.Form["__EVENTTARGET"] ?? "";
                        if (eventTarget.Contains("ddlPeriod")) return;
                        else
                        {
                            ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE), MSGType.Warning);
                            return;
                        }
                    }
                    break;
            }

            if (ddlPeriod.SelectedValue != "CUSTOM")
            {
                if (fromDate.HasValue && fromDate.Value < minLimitDate) fromDate = minLimitDate;
                if (toDate.HasValue && toDate.Value > maxLimitDate) toDate = maxLimitDate;
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                {
                    fromDate = minLimitDate;
                    toDate = maxLimitDate;
                }
            }

            ltrReportPeriod.Text = periodName.ToString();

            DataTable dtAllTasks = GanttChartManager.Instance.GetGanttTasks(projectId);
            DataTable dtCompleted = dtAllTasks.Clone();
            DataTable dtDoing = dtAllTasks.Clone();
            DataTable dtTodo = dtAllTasks.Clone();

            if (dtAllTasks != null && dtAllTasks.Rows.Count > 0)
            {
                foreach (DataRow row in dtAllTasks.Rows)
                {
                    int status = row["TrangThai"] != DBNull.Value ? Convert.ToInt32(row["TrangThai"]) : 0;

                    DateTime? taskStart = row["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(row["NgayBatDau"]) : (DateTime?)null;
                    DateTime? taskEnd = row["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(row["NgayKetThuc"]) : (DateTime?)null;

                    bool isInPeriod = false;
                    if (taskStart.HasValue && taskEnd.HasValue)
                    {
                        isInPeriod = (taskStart.Value <= toDate && taskEnd.Value >= fromDate);
                    }
                    else if (taskStart.HasValue)
                    {
                        isInPeriod = (taskStart.Value <= toDate);
                    }
                    else if (taskEnd.HasValue)
                    {
                        isInPeriod = (taskEnd.Value >= fromDate);
                    }
                    else
                    {
                        isInPeriod = true;
                    }

                    if (isInPeriod)
                    {
                        if (status == 2) dtCompleted.ImportRow(row);
                        else if (status == 1) dtDoing.ImportRow(row);
                        else dtTodo.ImportRow(row);
                    }
                }
            }

            DataTable dtIssues = ProjectReportManager.Instance.GetIssues(projectId, fromDate, toDate);

            int totalTasks = dtCompleted.Rows.Count + dtDoing.Rows.Count + dtTodo.Rows.Count;
            int countCompleted = dtCompleted.Rows.Count;
            int countTodo = dtTodo.Rows.Count;

            int countDoingNormal = 0;
            int countDoingWarning = 0;
            int countDoingOverdue = 0;
            foreach (DataRow row in dtDoing.Rows)
            {
                if (row["NgayKetThuc"] != DBNull.Value)
                {
                    DateTime endDate = Convert.ToDateTime(row["NgayKetThuc"]).Date;
                    int remainingDays = (int)(endDate - DateTime.Now.Date).TotalDays;

                    if (remainingDays < 0) countDoingOverdue++;
                    else if (remainingDays <= 2) countDoingWarning++;
                    else countDoingNormal++;
                }
                else
                {
                    countDoingNormal++;
                }
            }
            int countDoingTotal = dtDoing.Rows.Count;
            if (ddlPeriod.SelectedValue == "ALL")
            {
                phDashboard.Visible = true;
                string jsChart = $@"
                    setTimeout(function() {{
                        if (window.taskChartInstance) window.taskChartInstance.destroy();
                        var ctx1 = document.getElementById('taskStatusChart');
                        if(ctx1) {{
                            window.taskChartInstance = new Chart(ctx1, {{
                                type: 'doughnut',
                                data: {{
                                    labels: ['Hoàn thành ({countCompleted})', 'Đang làm ({countDoingTotal})', 'Chưa bắt đầu ({countTodo})'],
                                    datasets: [{{
                                        data: [{countCompleted}, {countDoingTotal}, {countTodo}],
                                        backgroundColor: ['#10b981', '#0ea5e9', '#cbd5e1'],
                                        borderWidth: 2,
                                        borderColor: '#ffffff'
                                    }}]
                                }},
                                plugins: [ChartDataLabels],
                                options: {{ 
                                    responsive: true, 
                                    maintainAspectRatio: false,
                                    layout: {{
                                        padding: {{ top: 20, bottom: 20, left: 20, right: 20 }}
                                    }},
                                    plugins: {{ 
                                        legend: {{ 
                                            position: 'right',
                                            labels: {{ boxWidth: 15, font: {{ size: 11, weight: 'bold' }}, color: '#334155' }}
                                        }},
                                        datalabels: {{
                                            color: '#1e293b',
                                            font: {{ weight: 'bold', size: 11 }},
                                            formatter: function(value, context) {{
                                                if (!value || value === 0) return ''; // Ẩn nhãn nếu giá trị bằng 0
                                                var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                                var percentage = total > 0 ? ((value / total) * 100).toFixed(1) + '%' : '0%';
                                                return value + ' task (' + percentage + ')';
                                            }},
                                            anchor: 'end',
                                            align: 'end',
                                            offset: 4
                                        }},
                                        tooltip: {{ enabled: false }}
                                    }},
                                    cutout: '55%'
                                }}
                            }});
                        }}

                        if (window.doingChartInstance) window.doingChartInstance.destroy();
                        var ctx2 = document.getElementById('doingStatusChart');
                        if(ctx2) {{
                            window.doingChartInstance = new Chart(ctx2, {{
                                type: 'doughnut',
                                data: {{
                                    labels: ['Bình thường ({countDoingNormal})', 'Sắp hạn ({countDoingWarning})', 'Trễ hạn ({countDoingOverdue})'],
                                    datasets: [{{
                                        data: [{countDoingNormal}, {countDoingWarning}, {countDoingOverdue}],
                                        backgroundColor: ['#3b82f6', '#f59e0b', '#ef4444'],
                                        borderWidth: 2,
                                        borderColor: '#ffffff'
                                    }}]
                                }},
                                plugins: [ChartDataLabels],
                                options: {{ 
                                    responsive: true, 
                                    maintainAspectRatio: false,
                                    layout: {{
                                        padding: {{ top: 20, bottom: 20, left: 20, right: 20 }}
                                    }},
                                    plugins: {{ 
                                        legend: {{ 
                                            position: 'right',
                                            labels: {{ boxWidth: 15, font: {{ size: 11, weight: 'bold' }}, color: '#334155' }}
                                        }},
                                        datalabels: {{
                                            color: '#1e293b',
                                            font: {{ weight: 'bold', size: 11 }},
                                            formatter: function(value, context) {{
                                                if (!value || value === 0) return ''; // Ẩn nhãn và không hiển thị chữ thừa khi giá trị bằng 0
                                                var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                                var percentage = total > 0 ? ((value / total) * 100).toFixed(1) + '%' : '0%';
                                                return value + ' task (' + percentage + ')';
                                            }},
                                            anchor: 'end',
                                            align: 'end',
                                            offset: 4
                                        }},
                                        tooltip: {{ enabled: false }}
                                    }},
                                    cutout: '55%'
                                }}
                            }});
                        }}
                    }}, 150);
                ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "drawDashboardCharts", jsChart, true);
            }
            else
            {
                phDashboard.Visible = false;
            }
            ltrTotalTasks.Text = totalTasks.ToString();
            ltrCompletedTasks.Text = countCompleted.ToString();
            ltrOverdueTasks.Text = countDoingOverdue.ToString();
            ltrTotalIssues.Text = dtIssues.Rows.Count.ToString();

            rptCompletedTasks.DataSource = dtCompleted;
            rptCompletedTasks.DataBind();

            rptDoingTasks.DataSource = dtDoing;
            rptDoingTasks.DataBind();

            rptTodoTasks.DataSource = dtTodo;
            rptTodoTasks.DataBind();

            dtIssues.DefaultView.Sort = "MaVanDe ASC";
            rptIssues.DataSource = dtIssues;
            rptIssues.DataBind();
        }

        #region Helpers dùng cho file ASPX

        protected string GetDeadlineClass(object ngayKetThucObj)
        {
            if (ngayKetThucObj == DBNull.Value) return "text-muted";
            DateTime endDate = Convert.ToDateTime(ngayKetThucObj).Date;
            int remainingDays = (int)(endDate - DateTime.Now.Date).TotalDays;

            if (remainingDays < 0) return "text-danger";
            if (remainingDays <= 2) return "text-warning";
            return "text-dark";
        }

        protected string GetTaskWarningHtml(object ngayKetThucObj)
        {
            if (ngayKetThucObj != DBNull.Value)
            {
                DateTime endDate = Convert.ToDateTime(ngayKetThucObj).Date;
                int remainingDays = (int)(endDate - DateTime.Now.Date).TotalDays;

                if (remainingDays < 0)
                    return "<div style='color: #dc2626; font-size: 11px; font-weight: bold;'>(Trễ hạn)</div>";
                else if (remainingDays <= 2)
                    return "<div style='color: #d97706; font-size: 11px; font-weight: bold;'>(Sắp đến hạn)</div>";
            }
            return "";
        }

        protected string GetPriorityText(int priority)
        {
            if (priority == 3) return GetResourceText(BackEndResourceKeys.HIGH);
            if (priority == 2) return GetResourceText(BackEndResourceKeys.MEDIUM);
            return GetResourceText(BackEndResourceKeys.LOW);
        }

        protected string GetPriorityBadge(int priority)
        {
            if (priority == 3) return "badge-high";
            if (priority == 2) return "badge-warning";
            return "badge-success";
        }

        protected string GetIssueStatusText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            TrangThaiVanDeEnum status = (TrangThaiVanDeEnum)Convert.ToInt32(value);
            return GetResourceText(IssueManager.Instance.GetValueForTrangThaiVanDe(status));
        }

        protected string GetIssueStatusBadge(int status)
        {
            if (status == (int)TrangThaiVanDeEnum.Processing) return "badge-info";
            if (status == (int)TrangThaiVanDeEnum.Processed) return "badge-success";

            return "badge-todo";
        }
        #endregion
    }
}