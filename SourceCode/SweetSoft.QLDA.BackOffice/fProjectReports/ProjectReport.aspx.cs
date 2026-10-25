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
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Data.SqlClient;
using System.Configuration;
using SweetSoft.QLDA.DataAccess;
namespace SweetSoft.QLDA.BackOffice.fProjectReports
{
    public partial class ProjectReport : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.ProjectReport;
        private sealed class ReportPersonInfo
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Avatar { get; set; }
            public string Initials { get; set; }
            public string Color { get; set; }
            public Guid UserId { get; set; }
        }
        // Dữ liệu nhân sự theo từng request, không dùng static cache chung giữa các dự án.
        private Dictionary<string, ReportPersonInfo> _assigneeProfiles =
            new Dictionary<string, ReportPersonInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, long> _workSecondsByTaskUser = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Guid, long> _workSecondsByTask = new Dictionary<Guid, long>();
        private bool _reportHoursLoaded;

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
                int countDoingTotal = GetReportCount("ReportCountDoing");
                int countTodo = GetReportCount("ReportCountTodo");
                int countDeadlineNormal = GetReportCount("ReportCountDeadlineNormal");
                int countDeadlineWarning = GetReportCount("ReportCountDeadlineWarning");
                int countDeadlineOverdue = GetReportCount("ReportCountDeadlineOverdue");
                htmlBody = htmlBody.Replace("<canvas id=\"taskStatusChart\"></canvas>", BuildDoughnutChartSvg(new[] { countDoingTotal, countTodo }, new[] { "Đang làm", "Chưa bắt đầu" }, new[] { "#0ea5e9", "#cbd5e1" }));
                htmlBody = htmlBody.Replace("<canvas id=\"doingStatusChart\"></canvas>", BuildDoughnutChartSvg(new[] { countDeadlineNormal, countDeadlineWarning, countDeadlineOverdue }, new[] { "Bình thường", "Sắp hạn", "Trễ hạn" }, new[] { "#3b82f6", "#f59e0b", "#ef4444" }));
                string cssStyles = @"
<style>
html, body { margin: 0; padding: 0; background: #ffffff; font-family: Arial, sans-serif; font-size: 13px; line-height: 1.45; color: #111827; }
body { word-wrap: break-word; }
.report-paper { width: 100%; box-sizing: border-box; background: #ffffff; border: 1px solid #f1f5f9; padding: 28px 24px; position: relative; }
.report-paper:before { content: ''; display: block; height: 5px; margin: -28px -24px 22px -24px; background: linear-gradient(90deg, #3b82f6, #8b5cf6); }
.report-header { text-align: center; margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px dashed #cbd5e1; }
.report-header h1 { font-size: 24px; color: #0f172a; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; margin: 0 0 8px 0; }
.sub-date { font-size: 14px; color: #64748b; font-weight: 500; font-style: italic; }
.kpi-summary-bar { display: table; table-layout: fixed; width: 100%; border-collapse: collapse; margin-bottom: 26px; border: 1px solid #cbd5e1; }
.kpi-item { display: table-cell; width: 33.333333%; text-align: center; border-right: 1px solid #cbd5e1; padding: 14px 8px; vertical-align: middle; }
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
.task-code-name { line-height: 1.45; }
.task-code-chip { display: inline-block; vertical-align: middle; min-width: 30px; margin-right: 6px; padding: 2px 5px; border-radius: 4px; background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; font-size: 11px; font-weight: 700; text-align: center; line-height: 1.3; }
.task-name-text { display: inline; vertical-align: middle; color: #334155; font-weight: 500; }
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
.report-assignee-compact { display: none !important; }
.report-assignee-expanded { display: block !important; border: 0; padding: 0; }
.report-person-row { display: block; margin: 3px 0 6px; font-size: 12px; line-height: 1.4; }
.report-person-identity { display: block; }
.report-person-avatar { display: none; }
.report-person-copy { display: inline; }
.report-person-name { display: inline; font-weight: 700; }
.report-person-email { display: none; }
.report-person-hours { display: inline; margin-left: 8px; color: #64748b; font-weight: 600; }
.report-task-hours { display: inline; margin-left: 6px; font-size: 11px; color: #64748b; }
.report-planned-period { display:block;line-height:1.5; }
.report-period-line {display:block;}
.report-period-label {color:#64748b;font-size:10px;margin-right:5px;}
.report-period-date {font-size:11px;font-weight:bold;}
.report-assignee-more,.report-assignee-tooltip { display: none !important; }
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
            if (dtAllTasks == null)
                dtAllTasks = new DataTable();

            DataTable dtDoing = dtAllTasks.Clone();
            DataTable dtTodo = dtAllTasks.Clone();
            if (dtAllTasks.Rows.Count > 0)
            {
                foreach (DataRow row in dtAllTasks.Rows)
                {
                    int status = row["TrangThai"] != DBNull.Value ? Convert.ToInt32(row["TrangThai"]) : -1;
                    DateTime? taskStart = row["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(row["NgayBatDau"]) : (DateTime?)null;
                    DateTime? taskEnd = row["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(row["NgayKetThuc"]) : (DateTime?)null;
                    bool isInPeriod;

                    if (taskStart.HasValue && taskEnd.HasValue)
                        isInPeriod = taskStart.Value <= toDate && taskEnd.Value >= fromDate;
                    else if (taskStart.HasValue)
                        isInPeriod = taskStart.Value <= toDate;
                    else if (taskEnd.HasValue)
                        isInPeriod = taskEnd.Value >= fromDate;
                    else
                        isInPeriod = true;

                    // Only active tasks are included: 1 = doing, 0 = not started.
                    // Completed tasks (2) and unknown statuses are intentionally excluded.
                    if (isInPeriod)
                    {
                        if (status == 1)
                            dtDoing.ImportRow(row);
                        else if (status == 0)
                            dtTodo.ImportRow(row);
                    }
                }
            }

            // Both active-task tables use expected end date, earliest first; rows without a date go last.
            dtDoing = SortTasksByExpectedEndDate(dtDoing);
            dtTodo = SortTasksByExpectedEndDate(dtTodo);

            // The report only includes issues which are currently being processed.
            DataTable dtIssues = FilterProcessingIssues(ProjectReportManager.Instance.GetIssues(projectId, fromDate, toDate));
            int totalTasks = dtDoing.Rows.Count + dtTodo.Rows.Count;
            int countTodo = dtTodo.Rows.Count;
            int countDoingTotal = dtDoing.Rows.Count;
            int countDeadlineNormal = 0;
            int countDeadlineWarning = 0;
            int countDeadlineOverdue = 0;

            // Deadline chart and overdue KPI include both in-progress and not-started tasks.
            CountDeadlineStatuses(dtDoing, ref countDeadlineNormal, ref countDeadlineWarning, ref countDeadlineOverdue);
            CountDeadlineStatuses(dtTodo, ref countDeadlineNormal, ref countDeadlineWarning, ref countDeadlineOverdue);
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
                                    labels: ['Đang làm ({countDoingTotal})', 'Chưa bắt đầu ({countTodo})'],
                                    datasets: [{{
                                        data: [{countDoingTotal}, {countTodo}],
                                        backgroundColor: ['#0ea5e9', '#cbd5e1'],
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
                                            labels: {{ boxWidth: 15, font: {{ size: 11, weight: 'bold' }}, color: '#111827' }},
                                            onClick: function (event, legendItem, legend) {{
                                                var chart = legend.chart;
                                                var dataset = chart.data.datasets[legendItem.datasetIndex || 0];
                                                // Không điều hướng khi số lượng của mục chú thích bằng 0.
                                                if (!dataset || Number(dataset.data[legendItem.index]) <= 0) return;
                                                var targetId = legendItem.index === 0 ? 'doingTasksSection' : 'todoTasksSection';
                                                if (window.scrollToReportSection) window.scrollToReportSection(targetId);
                                            }},
                                            onHover: function (event, legendItem, legend) {{
                                                var dataset = legend.chart.data.datasets[legendItem.datasetIndex || 0];
                                                var target = event.native && event.native.target;
                                                if (target) target.style.cursor = dataset && Number(dataset.data[legendItem.index]) > 0 ? 'pointer' : 'not-allowed';
                                            }},
                                            onLeave: function (event) {{
                                                var target = event.native && event.native.target;
                                                if (target) target.style.cursor = 'default';
                                            }}
                                        }},
                                        datalabels: {{
                                            color: '#000000',
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
                                    cutout: '55%',
                                    onClick: function (event, activeElements, chart) {{
                                    if (!activeElements || !activeElements.length) return;
                                    var point = activeElements[0];
                                    var dataset = chart.data.datasets[point.datasetIndex];
                                    if (!dataset || Number(dataset.data[point.index]) <= 0) return;
                                    var targetId = point.index === 0 ? 'doingTasksSection' : 'todoTasksSection';
                                    if (window.scrollToReportSection) window.scrollToReportSection(targetId);
                                    }},
                                    onHover: function (event, activeElements, chart) {{
                                        var point = activeElements && activeElements.length ? activeElements[0] : null;
                                        var dataset = point && chart.data.datasets[point.datasetIndex];
                                        var target = event.native && event.native.target;
                                        if (target) target.style.cursor = dataset && Number(dataset.data[point.index]) > 0 ? 'pointer' : 'default';
                                    }}
                                }}
                            }});
                        }}
                        if (window.doingChartInstance) window.doingChartInstance.destroy();
                        var ctx2 = document.getElementById('doingStatusChart');
                        if(ctx2) {{
                            window.doingChartInstance = new Chart(ctx2, {{
                                type: 'doughnut',
                                data: {{
                                    labels: ['Bình thường ({countDeadlineNormal})', 'Sắp hạn ({countDeadlineWarning})', 'Trễ hạn ({countDeadlineOverdue})'],
                                    datasets: [{{
                                        data: [{countDeadlineNormal}, {countDeadlineWarning}, {countDeadlineOverdue}],
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
                                            labels: {{ boxWidth: 15, font: {{ size: 11, weight: 'bold' }}, color: '#111827' }},
                                            onClick: function (event, legendItem, legend) {{
                                                var dataset = legend.chart.data.datasets[legendItem.datasetIndex || 0];
                                                // Mục có giá trị 0 không thể bấm để cuộn báo cáo.
                                                if (!dataset || Number(dataset.data[legendItem.index]) <= 0) return;
                                                var states = ['normal', 'soon', 'overdue'];
                                                if (window.scrollToTaskDeadline) window.scrollToTaskDeadline(states[legendItem.index]);
                                            }},
                                            onHover: function (event, legendItem, legend) {{
                                                var dataset = legend.chart.data.datasets[legendItem.datasetIndex || 0];
                                                var target = event.native && event.native.target;
                                                if (target) target.style.cursor = dataset && Number(dataset.data[legendItem.index]) > 0 ? 'pointer' : 'not-allowed';
                                            }},
                                            onLeave: function (event) {{
                                                var target = event.native && event.native.target;
                                                if (target) target.style.cursor = 'default';
                                            }}
                                        }},
                                        datalabels: {{
                                            color: '#000000',
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
                                    cutout: '55%',
                                    onClick: function (event, activeElements, chart) {{
                                    if (!activeElements || !activeElements.length) return;
                                    var point = activeElements[0];
                                    var dataset = chart.data.datasets[point.datasetIndex];
                                    if (!dataset || Number(dataset.data[point.index]) <= 0) return;
                                    var states = ['normal', 'soon', 'overdue'];
                                    if (window.scrollToTaskDeadline) window.scrollToTaskDeadline(states[point.index]);
                                    }},
                                    onHover: function (event, activeElements, chart) {{
                                        var point = activeElements && activeElements.length ? activeElements[0] : null;
                                        var dataset = point && chart.data.datasets[point.datasetIndex];
                                        var target = event.native && event.native.target;
                                        if (target) target.style.cursor = dataset && Number(dataset.data[point.index]) > 0 ? 'pointer' : 'default';
                                    }}
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
            ltrOverdueTasks.Text = countDeadlineOverdue.ToString();
            ltrTotalIssues.Text = dtIssues.Rows.Count.ToString();
            ViewState["ReportCountDoing"] = countDoingTotal;
            ViewState["ReportCountTodo"] = countTodo;
            ViewState["ReportCountDeadlineNormal"] = countDeadlineNormal;
            ViewState["ReportCountDeadlineWarning"] = countDeadlineWarning;
            ViewState["ReportCountDeadlineOverdue"] = countDeadlineOverdue;
            BindReportAssigneeProfiles(dtDoing, dtTodo);
            LoadReportWorkHours(projectId, fromDate, toDate);
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
            // Dùng các path SVG dạng cung thay cho stroke-dasharray. Một số HTML-to-PDF
            // renderer xử lý dashoffset của circle không ổn định, khiến biểu đồ bị khuyết lát.
            const double centerX = 130;
            const double centerY = 112;
            const double outerRadius = 82;
            const double innerRadius = 42;
            const double labelRadius = 104;

            double total = 0;
            int positiveSliceCount = 0;
            int singleSliceIndex = -1;
            for (int i = 0; i < values.Length; i++)
            {
                double value = Math.Max(0, values[i]);
                total += value;
                if (value > 0)
                {
                    positiveSliceCount++;
                    singleSliceIndex = i;
                }
            }

            StringBuilder svg = new StringBuilder();
            svg.Append("<svg xmlns='http://www.w3.org/2000/svg' width='520' height='225' viewBox='0 0 520 225' version='1.1'>");

            if (total > 0 && positiveSliceCount == 1)
            {
                // Trường hợp chỉ có một trạng thái có dữ liệu: vẽ vòng tròn đầy đủ,
                // không dùng cung 360 độ (SVG không vẽ được arc có điểm đầu = điểm cuối).
                svg.Append("<circle cx='").Append(FormatSvg(centerX))
                   .Append("' cy='").Append(FormatSvg(centerY))
                   .Append("' r='").Append(FormatSvg(outerRadius))
                   .Append("' fill='").Append(colors[singleSliceIndex]).Append("'/>");
                svg.Append("<circle cx='").Append(FormatSvg(centerX))
                   .Append("' cy='").Append(FormatSvg(centerY))
                   .Append("' r='").Append(FormatSvg(innerRadius))
                   .Append("' fill='#ffffff'/>");
            }
            else if (total > 0)
            {
                double currentAngle = -Math.PI / 2.0;
                for (int i = 0; i < values.Length; i++)
                {
                    double value = Math.Max(0, values[i]);
                    if (value <= 0) continue;

                    double sweepAngle = (value / total) * (2.0 * Math.PI);
                    double endAngle = currentAngle + sweepAngle;
                    int largeArc = sweepAngle > Math.PI ? 1 : 0;

                    double outerStartX = centerX + Math.Cos(currentAngle) * outerRadius;
                    double outerStartY = centerY + Math.Sin(currentAngle) * outerRadius;
                    double outerEndX = centerX + Math.Cos(endAngle) * outerRadius;
                    double outerEndY = centerY + Math.Sin(endAngle) * outerRadius;
                    double innerEndX = centerX + Math.Cos(endAngle) * innerRadius;
                    double innerEndY = centerY + Math.Sin(endAngle) * innerRadius;
                    double innerStartX = centerX + Math.Cos(currentAngle) * innerRadius;
                    double innerStartY = centerY + Math.Sin(currentAngle) * innerRadius;

                    string path = "M " + FormatSvg(outerStartX) + " " + FormatSvg(outerStartY)
                        + " A " + FormatSvg(outerRadius) + " " + FormatSvg(outerRadius) + " 0 " + largeArc + " 1 " + FormatSvg(outerEndX) + " " + FormatSvg(outerEndY)
                        + " L " + FormatSvg(innerEndX) + " " + FormatSvg(innerEndY)
                        + " A " + FormatSvg(innerRadius) + " " + FormatSvg(innerRadius) + " 0 " + largeArc + " 0 " + FormatSvg(innerStartX) + " " + FormatSvg(innerStartY)
                        + " Z";

                    svg.Append("<path d='").Append(path)
                       .Append("' fill='").Append(colors[i])
                       .Append("' stroke='#ffffff' stroke-width='1.5' stroke-linejoin='round'/>");
                    currentAngle = endAngle;
                }
            }

            // Nhãn phần trăm đặt ngoài vòng tròn, tránh đè trực tiếp lên các lát biểu đồ.
            if (total > 0)
            {
                double currentAngle = -Math.PI / 2.0;
                for (int i = 0; i < values.Length; i++)
                {
                    double value = Math.Max(0, values[i]);
                    if (value <= 0) continue;

                    double sweepAngle = (value / total) * (2.0 * Math.PI);
                    double middleAngle = positiveSliceCount == 1
                        ? Math.PI / 2.0
                        : currentAngle + (sweepAngle / 2.0);

                    double labelX = centerX + Math.Cos(middleAngle) * labelRadius;
                    double labelY = centerY + Math.Sin(middleAngle) * labelRadius;
                    double horizontalDirection = Math.Cos(middleAngle);
                    string textAnchor = horizontalDirection > 0.2 ? "start" : (horizontalDirection < -0.2 ? "end" : "middle");

                    // Căn nhãn ra phía ngoài vòng tròn để chữ không nằm đè lên lát màu.
                    // Với nhãn phía trái, giữ điểm cuối nằm sát ngoài mép vòng; nếu hướng thẳng
                    // sang trái thì đưa nhãn xuống dưới biểu đồ để không bị cắt ở mép SVG.
                    if (textAnchor == "end" && labelX < 95) labelX = 95;
                    if (textAnchor == "end" && Math.Abs(labelY - centerY) < outerRadius * 0.55)
                        labelY = centerY + outerRadius + 18;
                    if (labelX < 8) labelX = 8;
                    if (labelX > 235) labelX = 235;
                    if (labelY < 18) labelY = 18;
                    if (labelY > 214) labelY = 214;

                    string percentage = (value / total * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%";
                    svg.Append("<text x='").Append(FormatSvg(labelX))
                       .Append("' y='").Append(FormatSvg(labelY))
                       .Append("' text-anchor='").Append(textAnchor)
                       .Append("' dominant-baseline='middle' font-family='Arial,sans-serif' font-size='11' font-weight='700' fill='#111827'>")
                       .Append(values[i]).Append(" task (").Append(percentage).Append(")</text>");

                    currentAngle += sweepAngle;
                }
            }

            for (int i = 0; i < labels.Length; i++)
            {
                double legendY = 78 + (i * 30);
                svg.Append("<rect x='315' y='").Append(FormatSvg(legendY - 9))
                   .Append("' width='15' height='15' fill='").Append(colors[i]).Append("'/>");
                svg.Append("<text x='338' y='").Append(FormatSvg(legendY + 3))
                   .Append("' font-family='Arial,sans-serif' font-size='12' font-weight='700' fill='#111827'>")
                   .Append(labels[i]).Append(" (").Append(values[i]).Append(")</text>");
            }

            svg.Append("</svg>");
            return svg.ToString();
        }
        private string FormatSvg(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        // Tên trong GetGanttTasks là chuỗi; tra cứu thông tin nhân sự dự án một lần
        // cho toàn bộ báo cáo. Không tự gán email/avatar nếu hai tài khoản trùng tên.
        private void BindReportAssigneeProfiles(DataTable doing, DataTable todo)
        {
            _assigneeProfiles = new Dictionary<string, ReportPersonInfo>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> requestedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataTable dt in new[] { doing, todo })
            {
                if (dt == null || !dt.Columns.Contains("NhanVienThucHien")) continue;
                foreach (DataRow row in dt.Rows)
                {
                    foreach (string name in SplitAssigneeNames(row["NhanVienThucHien"]))
                        requestedNames.Add(name);
                }
            }
            if (requestedNames.Count == 0) return;
            try
            {
                // Cùng nguồn thành viên/Avatar/Email với ControlHelpers.BindNhanVienDuAnKemAvatar (CostList).
                DataTable members = RiskManager.Instance.GetAllNhanVienDuAnById(CurrentProjectId);
                if (members == null || !members.Columns.Contains("IdNhanVien")) return;
                HashSet<string> ambiguousNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var seenIds = new HashSet<Guid>();
                int idx = 0;
                foreach (DataRow member in members.Rows)
                {
                    Guid userId;
                    if (member["IdNhanVien"] == DBNull.Value ||
                        !Guid.TryParse(Convert.ToString(member["IdNhanVien"]), out userId) ||
                        userId == Guid.Empty || !seenIds.Add(userId)) continue;
                    string displayName = RowText(member, "TenNhanVien");
                    string email = RowText(member, "Email");
                    string avatar = RowText(member, "Avatar");
                    AspnetUser user = null;
                    // Nếu có sẵn tên khớp thì đọc thông tin thiếu; nếu tên không khớp
                    // vẫn kiểm tra UserName (một số báo cáo liệt kê username).
                    try { user = UserManager.Instance.GetUserById(userId); }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("ProjectReport user: " + ex.Message); }
                    string userName = user != null ? (user.UserName ?? "") : "";
                    if (string.IsNullOrWhiteSpace(displayName) && user != null)
                        displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? userName : user.DisplayName;
                    if (!requestedNames.Contains(displayName) && !requestedNames.Contains(userName)) continue;
                    if (string.IsNullOrWhiteSpace(avatar) && user != null) avatar = user.Avatar;
                    if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(userName))
                    {
                        try { var membership = Membership.GetUser(userName); if (membership != null) email = membership.Email; }
                        catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("ProjectReport membership: " + ex.Message); }
                    }
                    string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
                    var info = new ReportPersonInfo
                    {
                        Name = displayName,
                        UserId = userId,
                        Email = string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email,
                        Avatar = ReportAvatarUrl(avatar),
                        Initials = ReportInitials(displayName),
                        Color = colors[(idx++) % colors.Length]
                    };
                    // Cả tên hiển thị và username đều có thể là khóa của chuỗi báo cáo.
                    foreach (string key in new[] { displayName, userName })
                    {
                        if (string.IsNullOrWhiteSpace(key) || !requestedNames.Contains(key)) continue;
                        if (ambiguousNames.Contains(key)) continue;
                        if (_assigneeProfiles.ContainsKey(key)) { _assigneeProfiles.Remove(key); ambiguousNames.Add(key); }
                        else _assigneeProfiles[key] = info;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ProjectReport loading assignees: " + ex);
                _assigneeProfiles.Clear();
            }
        }
        private static string RowText(DataRow row, string name)
        {
            return row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToString(row[name]).Trim() : "";
        }
        private static List<string> SplitAssigneeNames(object value)
        {
            string raw = value == null || value == DBNull.Value ? "" : Convert.ToString(value);
            return raw.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim()).Where(n => n.Length > 0).ToList();
        }
        private string ReportAvatarUrl(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("user-icon.png", StringComparison.OrdinalIgnoreCase)) return "";
            if (avatar.StartsWith("~", StringComparison.Ordinal)) return ResolveUrl(avatar);
            Uri uri;
            if (Uri.TryCreate(avatar, UriKind.Absolute, out uri))
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ? avatar : "";
            if (avatar.StartsWith("//", StringComparison.Ordinal)) return "";
            return avatar;
        }
        private static string ReportInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
        private static string ReportEncode(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
        private static string ReportAttr(string s) { return HttpUtility.HtmlAttributeEncode(s ?? ""); }
        private ReportPersonInfo ResolveReportPerson(string rawName)
        {
            ReportPersonInfo existing;
            if (_assigneeProfiles.TryGetValue(rawName, out existing)) return existing;
            return new ReportPersonInfo
            {
                Name = rawName,
                Email = "Chưa cập nhật email",
                Avatar = "",
                Initials = ReportInitials(rawName),
                Color = "#7c3aed"
            };
        }
        private static string RenderPersonIdentity(ReportPersonInfo person)
        {
            var html = new StringBuilder(220);
            bool withImage = !string.IsNullOrWhiteSpace(person.Avatar);
            html.Append("<div class='report-person-identity'>");
            html.Append("<span class='report-person-avatar").Append(withImage ? "" : " is-fallback")
                .Append("' style='--report-avatar-bg:").Append(person.Color).Append("'>");
            if (withImage)
                html.Append("<img src='").Append(ReportAttr(person.Avatar))
                    .Append("' alt='' loading='lazy' onerror=\"this.style.display='none';this.parentNode.classList.add('is-fallback');\" />");
            html.Append("<span class='report-avatar-fallback'>").Append(ReportEncode(person.Initials)).Append("</span></span>");
            html.Append("<span class='report-person-copy'><span class='report-person-name'>")
                .Append(ReportEncode(person.Name)).Append("</span><span class='report-person-email'>")
                .Append(ReportEncode(person.Email)).Append("</span></span></div>");
            return html.ToString();
        }
        // Một hoặc hai người: luôn hiển thị hồ sơ đầy đủ, không có nút đóng/mở.
        // Từ ba người: giữ danh sách thu gọn +N và bấm được cả ô.
        // PDF vẫn dùng report-assignee-expanded để in đầy đủ dữ liệu nhân sự.
        protected string RenderReportAssignees(object taskIdValue, object namesValue, bool showWorkHours)
        {
            List<string> names = SplitAssigneeNames(namesValue);
            if (names.Count == 0) return "—";
            Guid taskId = ParseReportTaskId(taskIdValue);
            bool alwaysExpanded = names.Count <= 2;
            var html = new StringBuilder(450);
            if (alwaysExpanded)
            {
                html.Append("<div class='report-assignee-box is-static is-expanded' aria-label='Danh sách người thực hiện' data-report-hours='")
                    .Append(showWorkHours ? "1" : "0").Append("'>");
            }
            else
            {
                html.Append("<div class='report-assignee-box' role='button' tabindex='0' aria-expanded='false' aria-label='Xem danh sách người thực hiện' data-report-hours='")
                    .Append(showWorkHours ? "1" : "0").Append("'>");
                html.Append("<div class='report-assignee-compact'>");
                // Avatar + tên + email được dùng chung nguồn RenderPersonIdentity với bản mở rộng.
                // Bọc bằng chip nhỏ giúp luôn vừa hàng với nút +N.
                for (int i = 0; i < 2; i++)
                {
                    var person = ResolveReportPerson(names[i]);
                    html.Append("<span class='report-person-chip'>")
                        .Append(RenderPersonIdentity(person))
                        .Append("</span>");
                }
                string caption = "+" + (names.Count - 2).ToString(CultureInfo.InvariantCulture);
                html.Append("<span class='report-assignee-expanded-title'>")
                    .Append(names.Count.ToString(CultureInfo.InvariantCulture)).Append(" người thực hiện</span>");
                html.Append("<span class='report-assignee-more' aria-hidden='true' data-collapsed-label='")
                    .Append(ReportAttr(caption)).Append("'>").Append(ReportEncode(caption)).Append("</span>");
                html.Append("</div>");
            }
            html.Append("<div class='report-assignee-expanded'>");
            foreach (string name in names)
            {
                var person = ResolveReportPerson(name);
                html.Append("<div class='report-person-row'>").Append(RenderPersonIdentity(person));
                html.Append("<span class='report-person-hours'>")
                    .Append(ReportEncode(GetPersonWorkHours(taskId, person.UserId)))
                    .Append("</span>");
                html.Append("</div>");
            }
            html.Append("</div></div>");
            return html.ToString();
        }

        protected string RenderReportPlannedPeriod(object startValue, object endValue)
        {
            string start = FormatReportDate(startValue);
            string end = FormatReportDate(endValue);
            return "<div class='report-planned-period'>" +
                "<div class='report-period-line'><span class='report-period-label'>Bắt đầu</span><strong class='report-period-date'>" + ReportEncode(start) + "</strong></div>" +
                "<div class='report-period-line is-deadline'><span class='report-period-label'>Dự kiến</span><strong class='report-period-date " + GetDeadlineClass(endValue) + "'>" + ReportEncode(end) + "</strong></div>" +
                "</div>";
        }
        private static string FormatReportDate(object value)
        {
            return value == null || value == DBNull.Value ? "—" : Convert.ToDateTime(value).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
        private static Guid ParseReportTaskId(object value)
        {
            Guid id;
            return value != null && value != DBNull.Value && Guid.TryParse(Convert.ToString(value), out id) ? id : Guid.Empty;
        }
        private static string FormatReportWorkSeconds(long seconds)
        {
            if (seconds <= 0) return "Chưa có công";
            double hour = Math.Round(seconds / 3600d, 2, MidpointRounding.AwayFromZero);
            return hour <= 0 ? "<0,01 giờ công" : hour.ToString("0.##", new CultureInfo("vi-VN")) + " giờ công";
        }
        protected string GetTaskWorkHoursHtml(object taskIdValue)
        {
            Guid task = ParseReportTaskId(taskIdValue);
            long seconds;
            string label = !_reportHoursLoaded ? "— giờ công" : FormatReportWorkSeconds(_workSecondsByTask.TryGetValue(task, out seconds) ? seconds : 0);
            return "<i class='far fa-clock' aria-hidden='true'></i>" + ReportEncode(label);
        }
        private string GetPersonWorkHours(Guid task, Guid user)
        {
            long seconds;
            if (!_reportHoursLoaded || task == Guid.Empty || user == Guid.Empty) return "— giờ công";
            string key = task.ToString("N") + ":" + user.ToString("N");
            return FormatReportWorkSeconds(_workSecondsByTaskUser.TryGetValue(key, out seconds) ? seconds : 0);
        }

        // Chỉ đọc các khoảng đã kết toán, một lần mỗi báo cáo; nhóm theo task và nhân sự.
        // Nếu hệ thống chưa triển khai KhaiBaoGio20, tự dùng bảng khai báo giờ cũ.
        private void LoadReportWorkHours(Guid projectId, DateTime? fromDate, DateTime? toDate)
        {
            _reportHoursLoaded = false;
            _workSecondsByTaskUser.Clear();
            _workSecondsByTask.Clear();
            if (projectId == Guid.Empty) return;
            try
            {
                string connectionName = ConfigurationManager.AppSettings["KhaiBaoGio20.TenKetNoi"];
                if (string.IsNullOrWhiteSpace(connectionName)) return;
                ConnectionStringSettings config = ConfigurationManager.ConnectionStrings[connectionName];
                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString)) return;
                using (SqlConnection connection = new SqlConnection(config.ConnectionString))
                {
                    connection.Open();
                    bool hasNew;
                    bool hasLegacy;
                    using (var check = new SqlCommand(@"SELECT
                        CASE WHEN OBJECT_ID(N'dbo.TblKhaiBaoGioPhien',N'U') IS NOT NULL
                                  AND OBJECT_ID(N'dbo.TblKhaiBaoGioKhoangTinh',N'U') IS NOT NULL THEN 1 ELSE 0 END,
                        CASE WHEN OBJECT_ID(N'dbo.TblKhaiBaoGioCa',N'U') IS NOT NULL
                                  AND OBJECT_ID(N'dbo.TblKhaiBaoGioChiTiet',N'U') IS NOT NULL THEN 1 ELSE 0 END", connection))
                    using (SqlDataReader reader = check.ExecuteReader())
                    {
                        if (!reader.Read()) return;
                        hasNew = Convert.ToInt32(reader.GetValue(0)) == 1;
                        hasLegacy = Convert.ToInt32(reader.GetValue(1)) == 1;
                    }
                    if (!hasNew && !hasLegacy) return;
                    // Khi mới triển khai schema 2.0 nhưng chưa có phiên của dự án,
                    // vẫn đọc nhật ký bảng cũ; không đánh tráo 'chưa ghi nhận' thành 0.
                    if (hasNew && hasLegacy)
                    {
                        using (var probe = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien WHERE IdDuAn=@DuAn) THEN 1 ELSE 0 END", connection))
                        {
                            probe.Parameters.Add("@DuAn", SqlDbType.UniqueIdentifier).Value = projectId;
                            hasNew = Convert.ToInt32(probe.ExecuteScalar()) == 1;
                        }
                    }
                    string sql = hasNew
                        ? @"SELECT p.IdCongViec,p.IdNhanVien,
                                  ISNULL(SUM(CONVERT(bigint,k.SoGiay)),0) AS TongGiay
                            FROM dbo.TblKhaiBaoGioPhien p
                            INNER JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                            WHERE p.IdDuAn=@DuAn
                              AND (@TuNgay IS NULL OR k.NgayLam>=@TuNgay)
                              AND (@DenNgay IS NULL OR k.NgayLam<=@DenNgay)
                            GROUP BY p.IdCongViec,p.IdNhanVien"
                        : @"SELECT ct.IdCongViec,ca.UserId,
                                  ISNULL(SUM(CONVERT(bigint,ct.SoPhut)),0)*60 AS TongGiay
                            FROM dbo.TblKhaiBaoGioChiTiet ct
                            INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa=ct.IdKhaiBaoCa
                            WHERE ct.IdDuAn=@DuAn AND ISNULL(ct.DaXoa,0)=0 AND ISNULL(ca.DaXoa,0)=0
                              AND (@TuNgay IS NULL OR ca.NgayLam>=@TuNgay)
                              AND (@DenNgay IS NULL OR ca.NgayLam<=@DenNgay)
                            GROUP BY ct.IdCongViec,ca.UserId";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 30;
                        command.Parameters.Add("@DuAn", SqlDbType.UniqueIdentifier).Value = projectId;
                        command.Parameters.Add("@TuNgay", SqlDbType.Date).Value = fromDate.HasValue ? (object)fromDate.Value.Date : DBNull.Value;
                        command.Parameters.Add("@DenNgay", SqlDbType.Date).Value = toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(0) || reader.IsDBNull(1)) continue;
                                Guid taskId = reader.GetGuid(0);
                                Guid personId = reader.GetGuid(1);
                                long seconds = reader.IsDBNull(2) ? 0L : Math.Max(0L, Convert.ToInt64(reader.GetValue(2)));
                                string key = taskId.ToString("N") + ":" + personId.ToString("N");
                                long previous;
                                _workSecondsByTaskUser.TryGetValue(key, out previous);
                                _workSecondsByTaskUser[key] = previous + seconds;
                                _workSecondsByTask.TryGetValue(taskId, out previous);
                                _workSecondsByTask[taskId] = previous + seconds;
                            }
                        }
                    }
                    _reportHoursLoaded = true;
                }
            }
            catch (Exception ex)
            {
                _reportHoursLoaded = false;
                _workSecondsByTask.Clear();
                _workSecondsByTaskUser.Clear();
                System.Diagnostics.Trace.TraceWarning("ProjectReport giờ công: " + ex.Message);
            }
        }

        #region Helpers dùng cho file ASPX
        protected string GetDeadlineClass(object ngayKetThucObj)
        {
            if (ngayKetThucObj == null || ngayKetThucObj == DBNull.Value) return "text-muted";
            DateTime endDate = Convert.ToDateTime(ngayKetThucObj).Date;
            int remainingDays = (int)(endDate - DateTime.Today).TotalDays;
            if (remainingDays < 0) return "text-danger";
            if (remainingDays <= 2) return "text-warning";
            return "text-dark";
        }

        protected string GetDeadlineState(object ngayKetThucObj)
        {
            if (ngayKetThucObj == null || ngayKetThucObj == DBNull.Value) return "normal";
            DateTime endDate = Convert.ToDateTime(ngayKetThucObj).Date;
            int remainingDays = (int)(endDate - DateTime.Today).TotalDays;
            if (remainingDays < 0) return "overdue";
            if (remainingDays <= 2) return "soon";
            return "normal";
        }

        protected string GetTaskWarningHtml(object ngayKetThucObj)
        {
            string deadlineState = GetDeadlineState(ngayKetThucObj);
            if (deadlineState == "overdue")
                return "<div><span class='report-badge badge-danger deadline-label'>Trễ hạn</span></div>";
            if (deadlineState == "soon")
                return "<div><span class='report-badge badge-warning deadline-label'>Sắp hạn</span></div>";
            return "";
        }

        private DataTable SortTasksByExpectedEndDate(DataTable source)
        {
            if (source == null || source.Rows.Count < 2 || !source.Columns.Contains("NgayKetThuc"))
                return source;

            DataTable sortable = source.Copy();
            const string hasEndDateColumn = "__HasExpectedEndDate";
            if (sortable.Columns.Contains(hasEndDateColumn))
                sortable.Columns.Remove(hasEndDateColumn);
            sortable.Columns.Add(hasEndDateColumn, typeof(bool));
            foreach (DataRow row in sortable.Rows)
                row[hasEndDateColumn] = row["NgayKetThuc"] != DBNull.Value;

            sortable.DefaultView.Sort = hasEndDateColumn + " DESC, NgayKetThuc ASC";
            string[] originalColumns = new string[source.Columns.Count];
            for (int i = 0; i < source.Columns.Count; i++)
                originalColumns[i] = source.Columns[i].ColumnName;
            return sortable.DefaultView.ToTable(false, originalColumns);
        }

        private void CountDeadlineStatuses(DataTable tasks, ref int normal, ref int warning, ref int overdue)
        {
            if (tasks == null || !tasks.Columns.Contains("NgayKetThuc")) return;
            foreach (DataRow row in tasks.Rows)
            {
                string state = GetDeadlineState(row["NgayKetThuc"]);
                if (state == "overdue") overdue++;
                else if (state == "soon") warning++;
                else normal++;
            }
        }

        private DataTable FilterProcessingIssues(DataTable source)
        {
            if (source == null) return new DataTable();
            DataTable filtered = source.Clone();
            if (!source.Columns.Contains("TrangThai")) return filtered;

            foreach (DataRow row in source.Rows)
            {
                if (row["TrangThai"] != DBNull.Value &&
                    Convert.ToInt32(row["TrangThai"]) == (int)TrangThaiVanDeEnum.Processing)
                {
                    filtered.ImportRow(row);
                }
            }
            return filtered;
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