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
using System.Globalization;
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
                int countCompleted = GetReportCount("ReportCountCompleted");
                int countDoingTotal = GetReportCount("ReportCountDoing");
                int countTodo = GetReportCount("ReportCountTodo");
                int countDoingNormal = GetReportCount("ReportCountDoingNormal");
                int countDoingWarning = GetReportCount("ReportCountDoingWarning");
                int countDoingOverdue = GetReportCount("ReportCountDoingOverdue");
                htmlBody = htmlBody.Replace("<canvas id=\"taskStatusChart\"></canvas>", BuildDoughnutChartSvg(new[] { countCompleted, countDoingTotal, countTodo }, new[] { "Hoàn thành", "Đang làm", "Chưa bắt đầu" }, new[] { "#10b981", "#0ea5e9", "#cbd5e1" }));
                htmlBody = htmlBody.Replace("<canvas id=\"doingStatusChart\"></canvas>", BuildDoughnutChartSvg(new[] { countDoingNormal, countDoingWarning, countDoingOverdue }, new[] { "Bình thường", "Sắp hạn", "Trễ hạn" }, new[] { "#3b82f6", "#f59e0b", "#ef4444" }));
                string cssStyles = @"
<style>
html, body { margin: 0; padding: 0; background: #ffffff; font-family: Arial, sans-serif; font-size: 13px; line-height: 1.45; color: #334155; }
body { word-wrap: break-word; }
.report-paper { width: 100%; box-sizing: border-box; background: #ffffff; border: 1px solid #f1f5f9; padding: 28px 24px; position: relative; }
.report-paper:before { content: ''; display: block; height: 5px; margin: -28px -24px 22px -24px; background: linear-gradient(90deg, #3b82f6, #8b5cf6); }
.report-header { text-align: center; margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px dashed #cbd5e1; }
.report-header h1 { font-size: 24px; color: #0f172a; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; margin: 0 0 8px 0; }
.sub-date { font-size: 14px; color: #64748b; font-weight: 500; font-style: italic; }
.kpi-summary-bar { display: table; table-layout: fixed; width: 100%; border-collapse: collapse; margin-bottom: 26px; border: 1px solid #cbd5e1; }
.kpi-item { display: table-cell; width: 25%; text-align: center; border-right: 1px solid #cbd5e1; padding: 14px 8px; vertical-align: middle; }
.kpi-item:last-child { border-right: 0; }
.kpi-title { font-size: 12px; color: #64748b; display: block; margin-bottom: 5px; text-transform: uppercase; font-weight: 600; }
.kpi-number { font-size: 24px; font-weight: 800; display: block; line-height: 1.1; }
.kpi-number.total { color: #3b82f6; }
.kpi-number.success { color: #10b981; }
.kpi-number.danger { color: #ef4444; }
.kpi-number.warning { color: #f59e0b; }
.section-title { font-size: 16px; font-weight: 700; margin-top: 22px; margin-bottom: 12px; color: #0f172a; padding-left: 10px; border-left: 4px solid #3b82f6; line-height: 1.25; page-break-after: avoid; }
.section-title.success { border-left-color: #10b981; color: #047857; }
.section-title.info { border-left-color: #0ea5e9; color: #0369a1; }
.section-title.muted { border-left-color: #94a3b8; color: #475569; }
.section-title.danger { border-left-color: #ef4444; color: #dc2626; }
.section-title.dashboard { border-left-color: #8b5cf6; color: #6d28d9; margin-top: 0; }
.dashboard-container { display: table; table-layout: fixed; width: 100%; margin-bottom: 24px; }
.chart-box { display: table-cell; width: 50%; vertical-align: top; padding: 0 8px; height: 235px; }
.chart-box:first-child { padding-left: 0; }
.chart-box:last-child { padding-right: 0; }
.chart-title { font-size: 13px; font-weight: 700; text-align: center; margin-bottom: 4px; text-transform: uppercase; color: #111827; }
.pdf-chart-svg { width: 100%; height: 225px; display: block; }
.table { width: 100%; table-layout: fixed; border-collapse: collapse; margin-bottom: 20px; font-size: 13px; page-break-inside: auto; word-wrap: break-word; }
.table th, .table td { border: 1px solid #cbd5e1; padding: 8px 7px; vertical-align: middle; white-space: normal; word-break: break-word; line-height: 1.35; }
.table th { background-color: #f8fafc; font-weight: 700; text-align: center; }
.table tbody tr { page-break-inside: avoid; }
.text-center { text-align: center; }
.text-start { text-align: left; }
.text-danger { color: #dc2626; }
.text-warning { color: #d97706; }
.text-dark { color: #0f172a; }
.text-muted { color: #64748b; }
.report-badge { padding: 4px 9px; border-radius: 20px; font-size: 11px; font-weight: 700; display: inline-block; white-space: nowrap; line-height: 1.2; text-align: center; }
.badge-success { background-color: #dcfce7; color: #166534; border: 1px solid #bbf7d0; }
.badge-doing { background-color: #e0f2fe; color: #0369a1; border: 1px solid #bae6fd; }
.badge-todo { background-color: #f1f5f9; color: #475569; border: 1px solid #e2e8f0; }
.badge-danger { background-color: #fee2e2; color: #dc2626; border: 1px solid #fca5a5; }
.badge-warning { background-color: #fef08a; color: #854d0e; border: 1px solid #fde047; }
.badge-high { background-color: #fef2f2; color: #991b1b; }
.badge-med { background-color: #fffbeb; color: #d97706; }
.badge-low { background-color: #ecfdf5; color: #059669; }
.badge-info { background-color: #e0f2fe; color: #0369a1; }
.mb-2, .mb-4 { margin-bottom: 16px; }
.fw-bold { font-weight: 700; }
.ps-3 { padding-left: 12px; }
.w-100 { width: 100%; }
@page { size: A4; margin: 0; }
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
            ViewState["ReportCountCompleted"] = countCompleted;
            ViewState["ReportCountDoing"] = countDoingTotal;
            ViewState["ReportCountTodo"] = countTodo;
            ViewState["ReportCountDoingNormal"] = countDoingNormal;
            ViewState["ReportCountDoingWarning"] = countDoingWarning;
            ViewState["ReportCountDoingOverdue"] = countDoingOverdue;
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
        private int GetReportCount(string key)
        {
            object value = ViewState[key];
            if (value == null || value == DBNull.Value) return 0;
            int result;
            return int.TryParse(Convert.ToString(value), out result) ? result : 0;
        }
        private string BuildDoughnutChartSvg(int[] values, string[] labels, string[] colors)
        {
            const double centerX = 115;
            const double centerY = 118;
            const double radius = 64;
            const double labelRadius = 96;
            double total = 0;
            foreach (int value in values) total += Math.Max(0, value);
            StringBuilder svg = new StringBuilder();
            svg.Append("<svg class='pdf-chart-svg' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 520 235' preserveAspectRatio='xMidYMid meet'>");
            double circumference = 2 * Math.PI * radius;
            double cumulative = 0;
            if (total > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    double value = Math.Max(0, values[i]);
                    if (value <= 0) continue;
                    double segmentLength = circumference * value / total;
                    double offset = -cumulative;
                    svg.Append("<circle cx='").Append(FormatSvg(centerX)).Append("' cy='").Append(FormatSvg(centerY)).Append("' r='").Append(FormatSvg(radius)).Append("' fill='none' stroke='").Append(colors[i]).Append("' stroke-width='44' stroke-dasharray='").Append(FormatSvg(segmentLength)).Append(" ").Append(FormatSvg(circumference - segmentLength)).Append("' stroke-dashoffset='").Append(FormatSvg(offset)).Append("' transform='rotate(-90 115 118)'/>");
                    double midRatio = (cumulative + segmentLength / 2) / circumference;
                    double angle = midRatio * 2 * Math.PI - Math.PI / 2;
                    double labelX = centerX + Math.Cos(angle) * labelRadius;
                    double labelY = centerY + Math.Sin(angle) * labelRadius;
                    string percentage = (value / total * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%";
                    svg.Append("<text x='").Append(FormatSvg(labelX)).Append("' y='").Append(FormatSvg(labelY)).Append("' text-anchor='middle' font-family='Arial,sans-serif' font-size='11' font-weight='700' fill='#1e293b'>").Append(values[i]).Append(" task (").Append(percentage).Append(")</text>");
                    cumulative += segmentLength;
                }
            }
            for (int i = 0; i < labels.Length; i++)
            {
                double legendY = 78 + (i * 30);
                svg.Append("<rect x='272' y='").Append(FormatSvg(legendY - 9)).Append("' width='15' height='15' fill='").Append(colors[i]).Append("'/><text x='295' y='").Append(FormatSvg(legendY + 3)).Append("' font-family='Arial,sans-serif' font-size='12' font-weight='700' fill='#334155'>").Append(labels[i]).Append(" (").Append(values[i]).Append(")</text>");
            }
            svg.Append("</svg>");
            return svg.ToString();
        }
        private string FormatSvg(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
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