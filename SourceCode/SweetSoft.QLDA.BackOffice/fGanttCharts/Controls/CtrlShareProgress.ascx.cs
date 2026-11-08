using AngleSharp.Io;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fGanttCharts.Controls
{
    public partial class CtrlShareProgress : ProgressReportControlBase
    {
        // Snapshot do khach hang chinh sua chi de sinh Gantt, khong ghi ve TblCongViec.
        public sealed class CustomerGanttTaskEdit
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public int Status { get; set; }
            public bool IsVirtual { get; set; }
            public string Assignee { get; set; }
            public string DependencyId { get; set; }
        }

        // V8: ban Khach hang co the them/xoa/sua task ao. Tao DataTable moi de xuat HTML,
        // tuyet doi khong ghi task ao vao SQL hoac su dung gia tri lai tu DB de thay the ban xem.
        private static DataTable ApplyCustomerSnapshot(DataTable original, string payload)
        {
            if (original == null || original.Rows.Count == 0)
                throw new InvalidOperationException("Dự án chưa có dữ liệu công việc.");
            if (string.IsNullOrWhiteSpace(payload) || payload.Length > 1000000)
                throw new InvalidOperationException("Dữ liệu Gantt khách hàng rỗng hoặc quá lớn.");
            List<CustomerGanttTaskEdit> items;
            try
            {
                items = new JavaScriptSerializer { MaxJsonLength = 1200000, RecursionLimit = 100 }
                    .Deserialize<List<CustomerGanttTaskEdit>>(payload);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Không đọc được dữ liệu Gantt khách hàng.");
            }
            if (items == null || items.Count < 1 || items.Count > 500)
                throw new InvalidOperationException("Bản trình bày cần từ 1 đến 500 công việc.");
            var originalById = original.Rows.Cast<DataRow>()
                .ToDictionary(r => Convert.ToString(r["IdCongViec"]), StringComparer.OrdinalIgnoreCase);
            var submittedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var submittedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var validIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CustomerGanttTaskEdit task in items)
            {
                Guid id;
                if (task == null || !Guid.TryParse(task.Id, out id) || id == Guid.Empty || !submittedIds.Add(task.Id))
                    throw new InvalidOperationException("Mã định danh công việc ảo không hợp lệ hoặc bị trùng.");
                if (!originalById.ContainsKey(task.Id) && !task.IsVirtual)
                    throw new InvalidOperationException("Có công việc không thuộc dự án và không được đánh dấu là công việc ảo.");
                string code = (task.Code ?? "").Trim(), name = (task.Name ?? "").Trim();
                if (code.Length == 0 || code.Length > 50 || name.Length == 0 || name.Length > 250 ||
                    !System.Text.RegularExpressions.Regex.IsMatch(code, @"^[1-9]\d*(?:\.[1-9]\d*)*$") ||
                    !submittedCodes.Add(code) || (task.Assignee ?? "").Length > 250)
                    throw new InvalidOperationException("Mã hoặc tên công việc không hợp lệ, bị trùng: " + code);
                DateTime startDate, endDate;
                if (!DateTime.TryParseExact(task.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) ||
                    !DateTime.TryParseExact(task.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate) ||
                    endDate < startDate || startDate.Year < 1900 || endDate.Year > 2100 || (endDate - startDate).TotalDays > 3660)
                    throw new InvalidOperationException("Ngày của công việc " + code + " không hợp lệ.");
                if (task.Status < 0 || task.Status > 4)
                    throw new InvalidOperationException("Trạng thái của công việc " + code + " không hợp lệ.");
                validIds.Add(task.Id);
            }
            DataTable result = original.Clone();
            DataColumn idColumn = result.Columns["IdCongViec"];
            DataColumn dependencyColumn = result.Columns.Contains("IdCongViecPhuThuoc") ? result.Columns["IdCongViecPhuThuoc"] : null;
            foreach (CustomerGanttTaskEdit task in items)
            {
                if (!string.IsNullOrWhiteSpace(task.DependencyId) &&
                    (!validIds.Contains(task.DependencyId) || task.DependencyId.Equals(task.Id, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Phụ thuộc của công việc " + task.Code + " không hợp lệ.");
                DataRow row = result.NewRow();
                DataRow originalRow;
                if (originalById.TryGetValue(task.Id, out originalRow)) row.ItemArray = (object[])originalRow.ItemArray.Clone();
                else row.ItemArray = (object[])original.Rows[0].ItemArray.Clone();
                Guid taskId = Guid.Parse(task.Id);
                row["IdCongViec"] = idColumn.DataType == typeof(Guid) ? (object)taskId : task.Id;
                row["MaCongViec"] = task.Code.Trim();
                row["TenCongViec"] = task.Name.Trim();
                row["NgayBatDau"] = DateTime.ParseExact(task.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                row["NgayKetThuc"] = DateTime.ParseExact(task.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                row["TrangThai"] = task.Status;
                if (result.Columns.Contains("NhanVienThucHien")) row["NhanVienThucHien"] = (task.Assignee ?? "").Trim();
                if (dependencyColumn != null)
                {
                    row["IdCongViecPhuThuoc"] = string.IsNullOrWhiteSpace(task.DependencyId)
                        ? (object)DBNull.Value
                        : (dependencyColumn.DataType == typeof(Guid) ? (object)Guid.Parse(task.DependencyId) : task.DependencyId);
                }
                result.Rows.Add(row);
            }
            return result;
        }

        // Ten goi y ngan gon tai thoi diem bat dau; luon kiem tra lai tren server khi tao.
        protected string DefaultProgressProjectName
        {
            get
            {
                var project = ProjectId == Guid.Empty ? null : DuAnManager.Instance.GetDuAnById(ProjectId);
                return project != null && !string.IsNullOrWhiteSpace(project.TenDuAn) ? project.TenDuAn.Trim() : "Dự án";
            }
        }
        protected string DefaultProgressReportName
        {
            get { return BuildDefaultReportName(DefaultProgressProjectName, DateTime.Now); }
        }
        private static string BuildDefaultReportName(string project, DateTime localTime)
        {
            string stamp = localTime.ToString("ddMMyy-HHmm", CultureInfo.InvariantCulture);
            string name = string.IsNullOrWhiteSpace(project) ? "Dự án" : project.Trim();
            int maxProjectLength = Math.Max(6, 120 - stamp.Length - 3);
            if (name.Length > maxProjectLength) name = name.Substring(0, maxProjectLength).TrimEnd();
            return name + " · " + stamp;
        }
        // Cấu hình dùng chung trong từng dự án. Chỉ lưu khác biệt so với mặc định cấp 1–2.
        public sealed class ProgressTaskPreset
        {
            public ProgressTaskPreset() { }
            public string ProjectId { get; set; }
            public string[] AddedTaskIds { get; set; }
            public string[] RemovedDefaultTaskIds { get; set; }
            public DateTime SavedUtc { get; set; }
        }

        private string PresetPath
        {
            get { return Server.MapPath("~/App_Data/ProgressReports/Settings/" + ProjectId.ToString("N") + ".json"); }
        }

        private ProgressTaskPreset LoadPreset()
        {
            if (ProjectId == Guid.Empty) return null;
            try
            {
                string path = PresetPath;
                if (!File.Exists(path)) return null;
                lock (StorageLock)
                {
                    ProgressTaskPreset preset = Serializer.Deserialize<ProgressTaskPreset>(File.ReadAllText(path, Encoding.UTF8));
                    return preset != null && string.Equals(preset.ProjectId, ProjectId.ToString("D"), StringComparison.OrdinalIgnoreCase) ? preset : null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Không thể đọc cấu hình chọn công việc báo cáo: " + ex);
                return null;
            }
        }

        private HashSet<string> GetSelectionFromPreset(DataTable tasks, ProgressTaskPreset preset)
        {
            HashSet<string> added = new HashSet<string>(preset?.AddedTaskIds ?? new string[0], StringComparer.OrdinalIgnoreCase);
            HashSet<string> removed = new HashSet<string>(preset?.RemovedDefaultTaskIds ?? new string[0], StringComparer.OrdinalIgnoreCase);
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (tasks == null) return result;
            foreach (DataRow row in tasks.Rows)
            {
                string id = Convert.ToString(row["IdCongViec"]);
                bool isDefault = GetTaskLevel(Convert.ToString(row["MaCongViec"])) <= 2;
                if (!string.IsNullOrWhiteSpace(id) && ((isDefault && !removed.Contains(id)) || (!isDefault && added.Contains(id)))) result.Add(id);
            }
            return result;
        }

        protected override void OnPreRender(EventArgs e)
        {
            // Client opens the selector directly; pre-render available choices without a postback.
            if (!Page.IsPostBack && ProjectId != Guid.Empty) BindTaskChoices();
            Literal titleInput = FindHistoryControl<Literal>(this, "litReportNameInput");
            if (titleInput != null)
            {
                titleInput.Text = "<input id=\"shareProgressReportName\" name=\"shareProgressReportName\" type=\"text\" " +
                    "class=\"share-progress-report-name-input\" maxlength=\"120\" autocomplete=\"off\" required " +
                    "data-project-name=\"" + HttpUtility.HtmlAttributeEncode(DefaultProgressProjectName) + "\" " +
                    "value=\"" + HttpUtility.HtmlAttributeEncode(DefaultProgressReportName) + "\" " +
                    "placeholder=\"Nhập tên báo cáo\" />";
            }
            base.OnPreRender(e);
        }

        private void BindTaskChoices(ISet<string> selected = null)
        {
            DataTable tasks = GanttChartManager.Instance.GetGanttTasks(ProjectId);
            StringBuilder html = new StringBuilder();
            bool hasTasks = tasks != null && tasks.Rows.Count > 0;
            ProgressTaskPreset preset = selected == null ? LoadPreset() : null;
            HashSet<string> checkedIds = selected == null ? GetSelectionFromPreset(tasks, preset) : new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);
            if (selected == null)
            {
                lblPresetStatus.Text = string.Empty;
            }
            else lblPresetStatus.Text = string.Empty;
            if (hasTasks)
            {
                HashSet<string> taskCodes = new HashSet<string>(tasks.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["MaCongViec"])), StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in tasks.Rows)
                {
                    string id = Convert.ToString(row["IdCongViec"]);
                    string code = Convert.ToString(row["MaCongViec"]).Trim();
                    string name = Convert.ToString(row["TenCongViec"]);
                    int level = GetTaskLevel(code);
                    string parentCode = code.Contains(".") ? code.Substring(0, code.LastIndexOf('.')) : string.Empty;
                    bool hasChild = taskCodes.Any(other => other.StartsWith(code + ".", StringComparison.OrdinalIgnoreCase));
                    bool isExpanded = level == 1;
                    bool shouldCheck = checkedIds.Contains(id);
                    string safeId = HttpUtility.HtmlAttributeEncode(id);
                    string safeCode = HttpUtility.HtmlAttributeEncode(code);
                    string safeParent = HttpUtility.HtmlAttributeEncode(parentCode);
                    html.Append("<div class=\"share-progress-task-option is-level-").Append(Math.Min(level, 5)).Append("\" data-sp-code=\"").Append(safeCode)
                        .Append("\" data-sp-parent=\"").Append(safeParent).Append("\" data-sp-level=\"").Append(level.ToString(CultureInfo.InvariantCulture)).Append("\" data-sp-expanded=\"").Append(isExpanded ? "true" : "false").Append("\"");
                    if (level >= 3) html.Append(" hidden");
                    html.Append(">");
                    if (hasChild)
                        html.Append("<button type=\"button\" class=\"share-progress-tree-toggle\" aria-label=\"Mở hoặc thu gọn công việc con\" aria-expanded=\"")
                            .Append(isExpanded ? "true" : "false").Append("\" onclick=\"return shareProgressToggleBranch(event, this.closest('[data-sp-code]').getAttribute('data-sp-code'));\"><i class=\"fas fa-chevron-down\"></i></button>");
                    else html.Append("<span class=\"share-progress-tree-placeholder\"></span>");
                    html.Append("<label class=\"share-progress-task-label\"><input type=\"checkbox\" name=\"shareProgressTaskIds\" value=\"")
                        .Append(safeId).Append("\"");
                    if (shouldCheck) html.Append(" checked=\"checked\"");
                    html.Append(" /><span class=\"share-progress-task-code\">").Append(HttpUtility.HtmlEncode(code)).Append(".</span>")
                        .Append("<span class=\"share-progress-task-name\">").Append(HttpUtility.HtmlEncode(name)).Append("</span></label></div>");
                }
            }
            ltrTaskChoices.Text = html.ToString();
            phNoTasks.Visible = !hasTasks;
            btnCreateProgress.Enabled = hasTasks;
            btnSaveProgressPreset.Enabled = hasTasks;
        }

        private static int GetTaskLevel(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return 1;
            return Math.Max(1, code.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        protected void btnSaveProgressPreset_Click(object sender, EventArgs e)
        {
            hdfShareProgressOutcome.Value = string.Empty;
            hdfShareProgressErrorText.Value = string.Empty;
            try
            {
                if (ProjectId == Guid.Empty) throw new InvalidOperationException("Chưa xác định được dự án.");
                DataTable tasks = GanttChartManager.Instance.GetGanttTasks(ProjectId);
                if (tasks == null || tasks.Rows.Count == 0) throw new InvalidOperationException("Dự án chưa có công việc.");
                HashSet<string> postedIds = new HashSet<string>(Request.Form.GetValues("shareProgressTaskIds") ?? new string[0], StringComparer.OrdinalIgnoreCase);
                HashSet<string> valid = new HashSet<string>(tasks.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["IdCongViec"])), StringComparer.OrdinalIgnoreCase);
                postedIds.IntersectWith(valid);
                if (postedIds.Count == 0) throw new InvalidOperationException("Vui lòng chọn ít nhất một công việc.");
                List<string> added = new List<string>();
                List<string> removed = new List<string>();
                foreach (DataRow row in tasks.Rows)
                {
                    string id = Convert.ToString(row["IdCongViec"]);
                    bool isDefault = GetTaskLevel(Convert.ToString(row["MaCongViec"])) <= 2;
                    if (isDefault && !postedIds.Contains(id)) removed.Add(id);
                    if (!isDefault && postedIds.Contains(id)) added.Add(id);
                }
                ProgressTaskPreset preset = new ProgressTaskPreset
                {
                    ProjectId = ProjectId.ToString("D"),
                    AddedTaskIds = added.ToArray(),
                    RemovedDefaultTaskIds = removed.ToArray(),
                    SavedUtc = DateTime.UtcNow
                };
                string path = PresetPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                lock (StorageLock)
                {
                    File.WriteAllText(path, Serializer.Serialize(preset), new UTF8Encoding(false));
                }
                QueuePresetSaved(true, "Đã lưu cấu hình công việc cho dự án này.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Không thể lưu cấu hình chọn công việc báo cáo: " + ex);
                string error = "Không lưu được cấu hình: " + ex.Message;
                // Giữ nguyên modal và các ô tick hiện tại trên trình duyệt khi xảy ra lỗi.
                QueuePresetSaved(false, error);
            }
        }

        private void QueuePresetSaved(bool success, string message)
        {
            hdfShareProgressOutcome.Value = success ? "saved" : "saveError";
            hdfShareProgressErrorText.Value = message;
            upShareProgress.Update();
            ScriptManager.RegisterStartupScript(upShareProgress, GetType(), "ShareProgressPresetResult",
                "if(window.ShareProgressUI){window.ShareProgressUI.consumeServerOutcome();}", true);
        }

        protected void btnCreateProgress_Click(object sender, EventArgs e)
        {
            lblSelectionMessage.Text = string.Empty;
            hdfShareProgressOutcome.Value = string.Empty;
            hdfShareProgressErrorText.Value = string.Empty;

            try
            {
                if (ProjectId == Guid.Empty || Page.User == null || Page.User.Identity == null ||
                    !Page.User.Identity.IsAuthenticated || SweetContext.Current.UserId == Guid.Empty)
                    throw new InvalidOperationException("Không có quyền tạo báo cáo hoặc chưa xác định được dự án.");
                string reportMode = (Request.Form["ganttReportMode"] ?? "internal").Trim().ToLowerInvariant();
                if (reportMode != "internal" && reportMode != "customer")
                    throw new InvalidOperationException("Chế độ tạo báo cáo không hợp lệ.");
                // Dung toan bo task dang co tren Gantt (khong dung selection preset cu).
                DataTable allTasks = GanttChartManager.Instance.GetGanttTasks(ProjectId);
                if (allTasks == null || allTasks.Rows.Count == 0)
                    throw new InvalidOperationException("Dự án chưa có công việc để tạo báo cáo.");
                DataTable selectedTasks = reportMode == "customer"
                    ? ApplyCustomerSnapshot(allTasks, Request.Form["ganttCustomerPayload"])
                    : allTasks;
                // Payload khach hang chi co o snapshot duoc validate; khong co lenh UPDATE/Save DB.

                var project = DuAnManager.Instance.GetDuAnById(ProjectId);
                string projectName = !string.IsNullOrWhiteSpace(project?.TenDuAn) ? project.TenDuAn : "Dự án";
                string reportName = (Request.Form["shareProgressReportName"] ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(reportName)) reportName = BuildDefaultReportName(projectName, DateTime.Now);
                reportName = reportName.Replace("\r", " ").Replace("\n", " ").Trim();
                if (reportName.Length > 120) throw new InvalidOperationException("Tên báo cáo chỉ được tối đa 120 ký tự.");
                string reportId = Guid.NewGuid().ToString("N");
                DateTime reportCreatedUtc = DateTime.UtcNow;
                string reportHtml = BuildGanttReportHtml(projectName, reportCreatedUtc, selectedTasks);
                string fileName = "BaoCaoTienDo_" + reportId + ".html";
                string reportFolder = Server.MapPath("~/GeneratedProgressReports/");
                string dataFolder = Server.MapPath("~/App_Data/ProgressReports/");
                Directory.CreateDirectory(reportFolder);
                Directory.CreateDirectory(dataFolder);

                ProgressReportRecord record = new ProgressReportRecord
                {
                    Id = reportId,
                    ProjectId = ProjectId.ToString("D"),
                    ProjectName = projectName,
                    ReportName = reportName,
                    CreatedById = SweetContext.Current.UserId.ToString("D"),
                    CreatedByName = Page.User != null && Page.User.Identity != null ? Page.User.Identity.Name : string.Empty,
                    TaskCount = selectedTasks.Rows.Count,
                    OtpSalt = null,
                    OtpHash = null,
                    OtpProtected = null,
                    ReportHtml = reportHtml,
                    CreatedUtc = reportCreatedUtc
                };

                string dataPath = Path.Combine(dataFolder, reportId + ".json");
                string htmlPath = Path.Combine(reportFolder, fileName);
                string historyFile = Path.Combine(HistoryFolder, reportId + ".json");
                ProgressReportHistoryItem history = new ProgressReportHistoryItem
                {
                    ReportId = reportId,
                    ProjectId = ProjectId.ToString("D"),
                    ProjectName = projectName,
                    ReportName = reportName,
                    CreatedById = record.CreatedById,
                    CreatedByName = record.CreatedByName,
                    CreatedUtc = record.CreatedUtc,
                    TaskCount = selectedTasks.Rows.Count,
                    SelectedCodes = selectedTasks.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["MaCongViec"])).ToArray(),
                    FileName = fileName
                };
                try
                {
                    lock (StorageLock)
                    {
                        Directory.CreateDirectory(HistoryFolder);
                        File.WriteAllText(dataPath, Serializer.Serialize(record), new UTF8Encoding(false));
                        // Bao cao MOI la file HTML doc truc tiep qua lien ket, KHONG co OTP.
                        File.WriteAllText(htmlPath, reportHtml, new UTF8Encoding(false));
                        File.WriteAllText(historyFile, Serializer.Serialize(history), new UTF8Encoding(false));
                    }
                }
                catch
                {
                    TryDelete(dataPath);
                    TryDelete(htmlPath);
                    TryDelete(historyFile);
                    throw;
                }

                string relativeReportUrl = VirtualPathUtility.ToAbsolute("~/GeneratedProgressReports/" + fileName);
                string reportUrl = Request.Url.GetLeftPart(UriPartial.Authority) + relativeReportUrl;

                ltrReportFileName.Text = "<span class=\"share-progress-file-name\">" + HttpUtility.HtmlEncode(reportName) + "</span>";
                lnkReportFile.NavigateUrl = reportUrl;
                ltrOtp.Text = string.Empty;
                hdfOtp.Value = string.Empty;
                lblCreateMessage.Text = "Đã tạo báo cáo " + (reportMode == "customer" ? "khách hàng" : "nội bộ") + " với " + selectedTasks.Rows.Count.ToString(CultureInfo.InvariantCulture) + " công việc. Người có liên kết có thể mở trực tiếp, không cần OTP.";

                QueueClientModal(true, string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Tạo báo cáo tiến độ thất bại: " + ex);
                string message = "Không thể tạo báo cáo: " + ex.Message;
                lblSelectionMessage.Text = message;
                BindTaskChoices(new HashSet<string>(Request.Form.GetValues("shareProgressTaskIds") ?? new string[0], StringComparer.OrdinalIgnoreCase));
                QueueClientModal(false, message);
            }
        }

        private void QueueClientModal(bool success, string message)
        {
            // Status is read after UpdatePanel has finished replacing the DOM.
            hdfShareProgressOutcome.Value = success ? "success" : "error";
            hdfShareProgressErrorText.Value = success ? string.Empty : (message ?? string.Empty);
            if (!success) ShowNotify(message ?? "Không thể tạo báo cáo tiến độ.", MSGType.Error);
            upShareProgress.Update();
            // Full-page postback fallback; client callback is idempotent.
            ScriptManager.RegisterStartupScript(upShareProgress, GetType(), "ShareProgressResult",
                "if(window.ShareProgressUI){window.ShareProgressUI.consumeServerOutcome();}", true);
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        private string BuildOtpGateHtml(string reportId)
        {
            // Chi thay giao dien HTML/JS cua cong OTP. Viec xac thuc quyen noi bo/OTP
            // van xu ly tai ProgressReportAccess.aspx, khong dua hash/OTP vao HTML.
            string endpoint = VirtualPathUtility.ToAbsolute("~/GeneratedProgressReports/ProgressReportAccess.aspx");
            string jsReportId = HttpUtility.JavaScriptStringEncode(reportId);
            string jsEndpoint = HttpUtility.JavaScriptStringEncode(endpoint);
            string gateHtml = @"<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<meta name=""robots"" content=""noindex,nofollow,noarchive"">
<meta name=""theme-color"" content=""#f2f4f9"">
<title>Báo cáo tiến độ dự án · SweetSoft</title>
<style>
:root{color-scheme:light;--purple:#542e88;--purple-dark:#452172;--ink:#172033;--muted:#74829a;--line:#e2e8f0}
*{box-sizing:border-box}
html,body{min-height:100%;margin:0}
body{min-height:100vh;display:flex;align-items:center;justify-content:center;padding:24px;background:#f2f4f9;color:var(--ink);font-family:Arial,""Segoe UI"",sans-serif}
/* Classic centered card, like the original report access page. */
.shell{position:relative;width:min(100%,455px);background:#fff;border:1px solid #e2e8f0;border-radius:15px;box-shadow:0 17px 48px rgba(15,23,42,.10);overflow:hidden}
.inner{padding:33px 29px 0}
.brand{position:relative;display:flex;justify-content:center;margin-bottom:17px}
.brand-icon{width:49px;height:49px;border-radius:12px;display:flex;align-items:center;justify-content:center;background:#f2eafe;color:var(--purple);font-weight:800;font-size:25px;line-height:1}
.secure-tag{position:absolute;right:17px;top:17px;display:inline-flex;align-items:center;gap:5px;padding:6px 9px;background:#f5fbf6;border:1px solid #d9ebde;border-radius:999px;color:#287a4a;font-size:10px;font-weight:750;white-space:nowrap}
.secure-tag svg{width:12px;height:12px}
h1{margin:0 0 9px;color:#182234;text-align:center;font-size:22px;line-height:1.35;font-weight:800}
.desc{margin:0 auto 22px;max-width:370px;color:#6f829c;text-align:center;font-size:12.5px;line-height:1.65}
#autoState{display:flex;align-items:center;justify-content:center;gap:10px;max-width:360px;margin:0 auto 24px;padding:13px 10px;background:#faf7ff;border:1px solid #eee3f9;border-radius:9px;color:#654482;font-size:12px;line-height:1.5;text-align:center}
.spinner{width:18px;height:18px;flex:0 0 18px;border:2px solid #e1d2f2;border-top-color:var(--purple);border-radius:50%;animation:spin .7s linear infinite}
@keyframes spin{to{transform:rotate(360deg)}}
#otpForm{display:none}
.code-label{display:flex;justify-content:space-between;align-items:center;gap:8px;margin-bottom:9px;color:#182234;font-weight:750;font-size:11.5px}
.code-label small{color:#9b8aad;font-size:10px;font-weight:650;letter-spacing:.15px}
.otp-cells{display:grid;grid-template-columns:repeat(6,minmax(0,1fr));gap:8px;max-width:100%;margin-bottom:10px}
.otp-box{width:100%;min-width:0;height:54px;padding:0;border:1.5px solid #ded4eb;border-radius:8px;background:#fcfbff;color:var(--purple);font-size:23px;font-weight:800;text-align:center;outline:none;font-variant-numeric:tabular-nums;transition:border-color .16s,box-shadow .16s,background .16s}
.otp-box:hover:not(:disabled){border-color:#bda4db}
.otp-box:focus{border-color:#7341ac;box-shadow:0 0 0 3px rgba(113,55,179,.13);background:#fff}
.otp-box:disabled{opacity:.7}
.otp-cells.is-invalid .otp-box{border-color:#cf6682;background:#fff8fa}
.otp-cells.shake{animation:shake .26s ease both}
@keyframes shake{0%,100%{transform:translateX(0)}25%{transform:translateX(-3px)}75%{transform:translateX(3px)}}
.field-note{margin:0 0 19px;color:#94a0b2;font-size:10.5px;line-height:1.5;text-align:left}
.unlock{width:100%;min-height:44px;border:0;border-radius:8px;background:var(--purple);color:#fff;font-weight:750;font-size:12.5px;display:flex;justify-content:center;align-items:center;gap:9px;cursor:pointer;transition:background .16s,transform .16s}
.unlock:hover:not(:disabled){background:var(--purple-dark);transform:translateY(-1px)}
.unlock:disabled{opacity:.65;cursor:wait;transform:none}
.unlock .button-spinner{display:none;width:15px;height:15px;border:2px solid #ffffff59;border-top-color:#fff;border-radius:50%;animation:spin .7s linear infinite}
.unlock.is-loading .button-spinner{display:inline-block}
.msg{min-height:18px;margin:10px 0 2px;color:#b42342;text-align:center;font-size:11px;line-height:1.5}
.msg:empty{min-height:8px}
.bottom{padding:17px 18px 22px;margin-top:14px;color:#96a4ba;text-align:center;font-size:10.5px;line-height:1.55}
.bottom-brand{color:#6a538f;font-weight:750}
@media(max-width:480px){body{padding:16px}.inner{padding:30px 20px 0}.shell{border-radius:13px}.secure-tag{right:12px;top:13px;font-size:9px;padding:5px 7px}.otp-cells{gap:6px}.otp-box{height:49px;font-size:21px}h1{font-size:20px}}
@media(max-width:350px){.otp-cells{gap:4px}.otp-box{height:44px;font-size:19px}.secure-tag{right:9px;top:10px}}
@media(prefers-reduced-motion:reduce){.otp-box,.unlock{transition:none}.spinner,.button-spinner{animation-duration:1.5s}.otp-cells.shake{animation:none}}
</style>
</head>
<body>
<main class=""shell"" aria-labelledby=""pageTitle"">
  <span class=""secure-tag""><svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" aria-hidden=""true""><path d=""M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z""/><path d=""m9 12 2 2 4-4""/></svg> Bảo mật</span>
  <section class=""inner"">
    <div class=""brand""><span class=""brand-icon"" aria-hidden=""true"">S</span></div>
    <h1 id=""pageTitle"">Báo cáo tiến độ dự án</h1>
    <p class=""desc"" id=""instruction"">Đang kiểm tra quyền truy cập. Khách hàng nhập OTP 6 chữ số do người gửi cung cấp để xem báo cáo.</p>
    <div id=""autoState"" role=""status"" aria-live=""polite""><span class=""spinner"" aria-hidden=""true""></span><span>Đang kiểm tra quyền xem báo cáo của bạn…</span></div>
    <form id=""otpForm"" autocomplete=""off"" novalidate>
      <label class=""code-label"" for=""otp0""><span>MÃ OTP</span><small>6 CHỮ SỐ</small></label>
      <div class=""otp-cells"" id=""otpCells"" role=""group"" aria-label=""Nhập mã OTP gồm 6 chữ số"">
        <input id=""otp0"" class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""one-time-code"" maxlength=""6"" aria-label=""Chữ số OTP thứ nhất"">
        <input class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""off"" maxlength=""1"" aria-label=""Chữ số OTP thứ hai"">
        <input class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""off"" maxlength=""1"" aria-label=""Chữ số OTP thứ ba"">
        <input class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""off"" maxlength=""1"" aria-label=""Chữ số OTP thứ tư"">
        <input class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""off"" maxlength=""1"" aria-label=""Chữ số OTP thứ năm"">
        <input class=""otp-box"" type=""text"" inputmode=""numeric"" pattern=""[0-9]*"" autocomplete=""off"" maxlength=""1"" aria-label=""Chữ số OTP thứ sáu"">
      </div>
      <p class=""field-note"">Có thể dán nguyên mã OTP 6 chữ số vào một ô.</p>
      <button type=""submit"" id=""unlockButton"" class=""unlock""><span class=""button-spinner"" aria-hidden=""true""></span><span>Xác thực và xem báo cáo</span></button>
    </form>
    <div id=""message"" class=""msg"" role=""status"" aria-live=""polite""></div>
  </section>
  <footer class=""bottom""><span class=""bottom-brand"">SweetSoft</span> · Báo cáo chia sẻ có xác thực</footer>
</main>
<script>
(function(){
'use strict';
var reportId='__REPORT_ID__';
var accessUrl='__ACCESS_URL__';
var form=document.getElementById('otpForm');
var fields=Array.prototype.slice.call(document.querySelectorAll('.otp-box'));
var group=document.getElementById('otpCells');
var button=document.getElementById('unlockButton');
var message=document.getElementById('message');
var instruction=document.getElementById('instruction');
var autoState=document.getElementById('autoState');
var sending=false;
function digits(v){return String(v||'').replace(/\D/g,'').slice(0,6);}
function focusAt(i){var el=fields[Math.max(0,Math.min(i,5))];if(el){el.focus();el.select();}}
function currentCode(){return fields.map(function(el){return el.value;}).join('');}
function clearError(){message.textContent='';group.classList.remove('is-invalid','shake');}
function clearCode(){fields.forEach(function(el){el.value='';});clearError();}
function setBusy(v,automatic){sending=v;button.disabled=v;button.classList.toggle('is-loading',v&&!automatic);fields.forEach(function(el){el.disabled=v;});}
function showForm(shouldFocus){autoState.style.display='none';form.style.display='block';if(shouldFocus){focusAt(0);}}
function putDigits(start,raw){
 var value=digits(raw);if(!value){fields[start].value='';return;}
 for(var j=0;j<value.length && start+j<6;j++){fields[start+j].value=value.charAt(j);}
 var next=start+value.length;
 if(next<6)focusAt(next);else button.focus();
 clearError();
}
fields.forEach(function(field,index){
 field.addEventListener('input',function(){
   var value=digits(field.value);
   if(!value){field.value='';clearError();return;}
   if(value.length>1){field.value='';putDigits(index===0?0:index,value);return;}
   field.value=value;clearError();if(index<5)focusAt(index+1);else button.focus();
 });
 field.addEventListener('keydown',function(e){
   if(e.key==='Backspace'){
     e.preventDefault();if(field.value){field.value='';}else if(index>0){fields[index-1].value='';focusAt(index-1);}clearError();
   }else if(e.key==='Delete'){field.value='';clearError();}
   else if(e.key==='ArrowLeft' && index>0){e.preventDefault();focusAt(index-1);}
   else if(e.key==='ArrowRight' && index<5){e.preventDefault();focusAt(index+1);}
   else if(e.key==='Home'){e.preventDefault();focusAt(0);}
   else if(e.key==='End'){e.preventDefault();focusAt(5);}
 });
 field.addEventListener('paste',function(e){
   var value=digits(e.clipboardData && e.clipboardData.getData('text'));
   if(!value)return;
   e.preventDefault();putDigits(value.length===6?0:index,value);
 });
});
function markInvalid(text){message.textContent=text;group.classList.remove('shake');void group.offsetWidth;group.classList.add('is-invalid','shake');focusAt(0);}
async function openReport(otp,automatic){
 if(sending)return;
 var failed=false;
 setBusy(true,automatic);
 if(!automatic){message.textContent='Đang xác thực mã OTP…';group.classList.remove('is-invalid','shake');}
 try{
  var response=await fetch(accessUrl,{method:'POST',headers:{'Content-Type':'application/json','Accept':'application/json'},cache:'no-store',credentials:'same-origin',body:JSON.stringify({reportId:reportId,otp:otp||''})});
  var contentType=(response.headers.get('content-type')||'').toLowerCase();
  if(contentType.indexOf('application/json')<0){throw new Error('Máy chủ không trả JSON (HTTP '+response.status+'). Vui lòng liên hệ người quản trị.');}
  var result=await response.json();
  if(result && result.requiresOtp){showForm(false);instruction.textContent='Nhập OTP do người chia sẻ cung cấp để mở báo cáo tiến độ.';message.textContent='';return;}
  if(!response.ok || !result || !result.success){throw new Error(result&&result.message?result.message:'Không thể xác thực mã OTP.');}
  if(typeof result.html!=='string' || !result.html){throw new Error('Báo cáo chưa có nội dung.');}
  document.open();document.write(result.html);document.close();return;
 }catch(err){
  showForm(false);
  instruction.textContent='Nhập OTP do người chia sẻ cung cấp để mở báo cáo tiến độ.';
  clearCode();
  message.textContent=err.message||'Có lỗi xảy ra. Vui lòng thử lại.';
  group.classList.add('is-invalid','shake');
  failed=true;
 }finally{setBusy(false,automatic);if(failed)focusAt(0);}
}
form.addEventListener('submit',function(e){
 e.preventDefault();if(sending)return;
 var otp=currentCode();
 if(!/^[0-9]{6}$/.test(otp)){markInvalid('Vui lòng nhập đầy đủ 6 chữ số OTP.');return;}
 openReport(otp,false);
});
openReport('',true);
})();
</script>
</body>
</html>
";
            return gateHtml.Replace("__REPORT_ID__", jsReportId)
                           .Replace("__ACCESS_URL__", jsEndpoint);
        }

        private string BuildGanttReportHtml(string projectName, DateTime createdUtc, DataTable selectedTasks)
        {
            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;
            foreach (DataRow row in selectedTasks.Rows)
            {
                DateTime? start = GetNullableDate(row, "NgayBatDau");
                DateTime? end = GetNullableDate(row, "NgayKetThuc");
                if (start.HasValue && start.Value.Date < minDate) minDate = start.Value.Date;
                if (end.HasValue && end.Value.Date < minDate) minDate = end.Value.Date;
                if (start.HasValue && start.Value.Date > maxDate) maxDate = start.Value.Date;
                if (end.HasValue && end.Value.Date > maxDate) maxDate = end.Value.Date;
            }
            if (minDate == DateTime.MaxValue) minDate = DateTime.Today;
            if (maxDate == DateTime.MinValue || maxDate < minDate) maxDate = minDate.AddDays(7);
            minDate = minDate.AddDays(-1);
            maxDate = maxDate.AddDays(1);
            if (maxDate <= minDate) maxDate = minDate.AddDays(7);

            int totalDays = Math.Max(1, (int)(maxDate.Date - minDate.Date).TotalDays + 1);
            int todayIndex = (int)(DateTime.Today - minDate.Date).TotalDays;
            List<ProgressReportTask> taskList = new List<ProgressReportTask>();
            HashSet<string> chosenTaskIds = new HashSet<string>(selectedTasks.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["IdCongViec"])), StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in selectedTasks.Rows)
            {
                DateTime? start = GetNullableDate(row, "NgayBatDau");
                DateTime? end = GetNullableDate(row, "NgayKetThuc");
                int startDay = start.HasValue ? (int)(start.Value.Date - minDate.Date).TotalDays : 0;
                int endDay = end.HasValue ? (int)(end.Value.Date - minDate.Date).TotalDays + 1 : startDay + 1;
                if (endDay <= startDay) endDay = startDay + 1;
                startDay = Math.Max(0, Math.Min(startDay, totalDays - 1));
                endDay = Math.Max(startDay + 1, Math.Min(endDay, totalDays));

                string id = Convert.ToString(row["IdCongViec"]);
                string dependency = selectedTasks.Columns.Contains("IdCongViecPhuThuoc") && row["IdCongViecPhuThuoc"] != DBNull.Value
                    ? Convert.ToString(row["IdCongViecPhuThuoc"]) : string.Empty;
                if (!chosenTaskIds.Contains(dependency)) dependency = string.Empty;
                int statusValue = row["TrangThai"] == DBNull.Value ? 0 : Convert.ToInt32(row["TrangThai"]);

                taskList.Add(new ProgressReportTask
                {
                    Id = id,
                    Code = Convert.ToString(row["MaCongViec"]),
                    Name = Convert.ToString(row["TenCongViec"]),
                    Assignee = selectedTasks.Columns.Contains("NhanVienThucHien") ? Convert.ToString(row["NhanVienThucHien"]) : string.Empty,
                    DependencyId = dependency,
                    Level = GetTaskLevel(Convert.ToString(row["MaCongViec"])),
                    StartDay = startDay,
                    EndDay = endDay,
                    EndDate = end.HasValue ? end.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty,
                    Status = statusValue == 3 ? "done-late" : (statusValue == 2 ? "done" : statusValue == 4 ? "review" : statusValue == 1 ? "doing" : "todo")
                });
            }

            ProgressReportChartData chartData = new ProgressReportChartData
            {
                ProjectName = projectName,
                StartDate = minDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TotalDays = totalDays,
                TodayIndex = todayIndex,
                Tasks = taskList
            };
            string json = Serializer.Serialize(chartData);
            string encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            // Giu ten tuy chinh trong nhat ky; tieu de HTML luon la ten du an + thoi diem tao ban chup.
            string title = "Báo cáo tiến độ - " + projectName + " (" +
                createdUtc.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN")) + ")";
            string encodedReportTitle = HttpUtility.HtmlEncode(title);

            return @"<!doctype html>
<html lang=""vi""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><meta name=""robots"" content=""noindex,nofollow,noarchive""><title>" + encodedReportTitle + @"</title>
<style>
* { box-sizing:border-box; }
html,body { height:100%;margin:0;font-family:Arial,""Segoe UI"",sans-serif;color:#293445;background:#f3f4f8; }
body { display:flex;flex-direction:column;min-height:100vh;padding:16px 20px 20px; }
.report-top { display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:12px;margin-bottom:13px;flex:0 0 auto; }
.report-title { margin:0;color:#3f1e70;font-size:20px;font-weight:800;line-height:1.4; }
.report-help-holder { position:relative;flex:0 0 auto; }
.report-help { display:inline-flex;align-items:center;justify-content:center;width:38px;height:38px;padding:0;border:1px solid #ddcfef;border-radius:10px;background:#f8f4ff;color:#63309e;font-size:19px;font-weight:800;cursor:pointer;transition:background-color .15s ease,color .15s ease; }
.report-help:hover,.report-help.is-pinned { background:#54218d;color:#fff;border-color:#54218d; }
.report-help:focus-visible { outline:2px solid #9258ca;outline-offset:2px; }
.legend { display:none;position:absolute;right:0;top:calc(100% + 7px);width:min(570px,calc(100vw - 40px));padding:13px 17px 12px;border:1px solid #deccef;border-radius:13px;background:#fff;z-index:300;box-shadow:0 12px 30px rgba(61,38,95,.16); }
.legend.is-open { display:block; }
.legend-title { margin:0 0 10px;color:#5b268c;font-size:13px;font-weight:800; }
.legend-grid { display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:13px 11px; }
.legend-item { display:flex;align-items:center;gap:8px;color:#4b5565;font-size:12px;font-weight:650;white-space:nowrap; }
.swatch { display:inline-block;flex:0 0 14px;width:14px;height:14px;border-radius:3px; }
.swatch.done { background:#34a853; }
.swatch.doing { background:#4285f4; }
.swatch.warning { background:#f4b400; }
.swatch.overdue { background:#ea4335; }
.swatch.todo { background:#cbd5e1; }
.swatch.today { width:2px;flex-basis:2px;height:15px;margin-left:5px;margin-right:7px;border-radius:0;background:#ea4335; }
.swatch.dependency { background:none;color:#f59e0b;font-size:18px;font-weight:900; }
.legend-note { display:block;margin-top:10px;color:#8b7b9d;font-size:11px; }
.gantt-viewport { isolation:isolate;position:relative;flex:1 1 auto;min-height:360px;height:calc(100vh - 94px);overflow:auto;background:#fff;border:1px solid #ded5e8;border-radius:11px;box-shadow:0 3px 15px rgba(59,35,91,.06);scrollbar-width:thin; }
.gantt-body { display:flex;position:relative;width:max-content;min-width:100%;min-height:100%; }
.task-col { position:sticky;left:0;z-index:80;width:350px;flex:0 0 350px;background:#fff;border-right:1px solid #d4d7df;box-shadow:3px 0 8px rgba(20,21,24,.045); }
.task-col-header { position:sticky;top:0;z-index:90;height:44px;display:flex;align-items:center;justify-content:center;border-bottom:1px solid #d7d7db;background:#f8f9fb;color:#273144;font-weight:800;font-size:13px; }
.task-row { display:flex;align-items:center;height:52px;max-height:52px;padding:0 8px;border-bottom:1px solid #e4e6eb;background:#fff;font-size:13px;overflow:hidden; }
.task-row:hover { background:#faf7ff; }
.task-level-1 { display:flex;align-items:center;width:100%;min-width:0;height:40px;padding:0 10px;gap:6px;border-left:4px solid #5e35b1;border-radius:7px;background:#f3eaff;color:#6232ae;font-size:13px;font-weight:750; }
.task-level-child { display:flex;align-items:center;width:100%;min-width:0;height:40px;color:#303b4b;font-size:13px; }
.task-list-icon { flex:0 0 auto;margin-right:3px;font-size:14px; }
.task-text { flex:1 1 auto;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
.task-code { font-weight:750; }
.branch { display:inline-block;flex:0 0 14px;width:14px;height:18px;margin-right:6px;border-left:1.5px solid #aab1bb;border-bottom:1.5px solid #aab1bb;transform:translateY(-6px); }
.report-tree-expandable { cursor:pointer;transition:background-color .15s ease,color .15s ease; }
.task-level-1.report-tree-expandable:hover { background:#eadafc; }
.task-level-child.report-tree-expandable:hover { background:#f7f0ff;border-radius:6px; }
.report-tree-expandable:focus-visible { outline:2px solid #9260ca;outline-offset:-2px; }
.report-tree-toggle { display:inline-flex;align-items:center;justify-content:center;flex:0 0 14px;width:14px;height:22px;margin-right:2px;color:#7a6890;pointer-events:none; }
.report-tree-chevron { display:block;width:7px;height:7px;border-right:2px solid currentColor;border-bottom:2px solid currentColor;transform:rotate(45deg) translate(-1px,-1px);transition:transform .17s ease; }
.report-tree-expandable[aria-expanded=""false""] .report-tree-chevron { transform:rotate(-45deg); }
.chart-col { position:relative;z-index:1;flex:0 0 auto;min-height:100%; }
.chart-header { position:sticky;top:0;z-index:10;height:44px;background:#f8f9fb;border-bottom:1px solid #d7d7db; }
.tick-label { display:flex;position:absolute;top:0;height:100%;align-items:center;justify-content:center;color:#647084;font-size:12px;white-space:nowrap;border-left:1px solid #e6e9ed; }
.chart-tracks { position:relative;z-index:1;min-height:100px; }
.task-track { position:relative;height:52px;max-height:52px;overflow:hidden;border-bottom:1px solid #e4e6eb;background-color:#fff;background-repeat:repeat-x; }
.task-track.hide-bar .bar { opacity:0!important;pointer-events:none!important; }
.bar { position:absolute;top:14px;height:24px;z-index:7;border-radius:5px;cursor:default;box-shadow:0 0 0 4px rgba(0,0,0,.035);transition:filter .18s ease,box-shadow .18s ease; }
.bar:hover { filter:brightness(1.08);box-shadow:0 3px 10px rgba(0,0,0,.17); }
.bar-fill { position:absolute;inset:0;overflow:hidden;border-radius:5px; }
.bar-fill.animated-stripes { background-image:linear-gradient(-45deg,rgba(255,255,255,.17) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.17) 50%,rgba(255,255,255,.17) 75%,transparent 75%,transparent);background-size:28px 28px;animation:report-stripes 1.5s linear infinite; }
.bar.status-done .bar-fill { background-color:#34a853; }
.bar.status-doing .bar-fill { background-color:#4285f4; }
.bar.status-warning .bar-fill { background-color:#f4b400; }
.bar.status-overdue .bar-fill { background-color:#ea4335; }
.bar.status-todo .bar-fill { background-color:#cbd5e1; }
.static-text { position:absolute;top:0;left:8px;width:calc(100% - 14px);height:24px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;line-height:24px;color:#fff;font-size:13px;font-weight:800;pointer-events:none; }
.bar.status-todo .static-text { color:#334155; }
.bar.status-overdue { box-shadow:0 0 0 4px rgba(234,67,53,.10); }
.today-line { position:absolute;top:0;bottom:0;width:2px;background:#ea4335;z-index:4;pointer-events:none; }
.connections { position:absolute;left:0;top:0;z-index:5;pointer-events:none;overflow:visible; }
.dep-line { fill:none;stroke:#ff9800;stroke-width:1.5;stroke-dasharray:4 4; }
.report-tip { display:none;position:fixed;z-index:2147483000;max-width:min(330px,calc(100vw - 20px));padding:9px 13px;border:1px solid rgba(167,139,250,.4);border-radius:9px;background:#5930a1;color:#fff;font-size:12.5px;font-weight:750;line-height:1.4;white-space:normal;overflow-wrap:anywhere;pointer-events:none;box-shadow:0 9px 23px rgba(48,27,78,.2); }
.report-tip[data-kind=""todo""] { background:#526276; }
.report-tip[data-kind=""done""],.report-tip[data-kind=""late""] { background:#24884b; }
.report-tip[data-kind=""doing""] { background:#2563c9; }
.report-tip[data-kind=""warning""] { background:#ad7508; }
.report-tip[data-kind=""overdue""] { background:#d73939; }
.empty-report { padding:30px;color:#64748b;text-align:center; }
@keyframes report-stripes { to { background-position:28px 0; } }
@media(max-width:650px) { body { padding:10px; } .report-title { font-size:16px; } .task-col { width:245px;flex-basis:245px; } .legend-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } }
@media(prefers-reduced-motion:reduce) { .bar-fill.animated-stripes { animation:none; } .report-tree-chevron,.bar { transition:none; } }
</style></head><body>
<div class=""report-top""><h1 class=""report-title"">" + encodedReportTitle + @"</h1><div class=""report-help-holder"" id=""reportHelpHolder""><button type=""button"" class=""report-help"" id=""reportHelp"" aria-label=""Chú thích biểu đồ"" aria-expanded=""false"" aria-controls=""reportLegend"">?</button><section class=""legend"" id=""reportLegend"" aria-hidden=""true""><h2 class=""legend-title"">Chú thích biểu đồ</h2><div class=""legend-grid""><span class=""legend-item""><i class=""swatch done""></i>Hoàn thành</span><span class=""legend-item""><i class=""swatch doing""></i>Đang thực hiện</span><span class=""legend-item""><i class=""swatch warning""></i>Sắp đến hạn</span><span class=""legend-item""><i class=""swatch overdue""></i>Trễ hạn</span><span class=""legend-item""><i class=""swatch todo""></i>Chưa bắt đầu</span><span class=""legend-item""><i class=""swatch today""></i>Hôm nay</span><span class=""legend-item""><i class=""swatch dependency"">➜</i>Liên kết phụ thuộc</span></div><small class=""legend-note"">Rê chuột để xem. Nhấn dấu ? để ghim hoặc bỏ ghim.</small></section></div></div>
<div class=""gantt-viewport"" id=""ganttViewport""><div class=""gantt-body"" id=""ganttBody""><div class=""task-col"" id=""taskCol""><div class=""task-col-header"">Công việc</div><div id=""taskRows""></div></div><div class=""chart-col"" id=""chartCol""><div class=""chart-header"" id=""chartHeader""></div><div class=""chart-tracks"" id=""chartTracks""></div></div></div></div><div class=""report-tip"" id=""reportTip"" role=""tooltip""></div>
<script>
(function(){'use strict';
var encoded='" + encodedData + @"';var report=JSON.parse(decodeURIComponent(escape(atob(encoded))));
var ROW_HEIGHT=52,DAY_WIDTH=60,NAME_WIDTH=window.innerWidth<=650?245:350;
var viewport=document.getElementById('ganttViewport'),taskRows=document.getElementById('taskRows'),chartCol=document.getElementById('chartCol'),header=document.getElementById('chartHeader'),tracks=document.getElementById('chartTracks'),body=document.getElementById('ganttBody');
var totalDays=Math.max(1,report.TotalDays),chartWidth=Math.max(totalDays*DAY_WIDTH,Math.max(300,viewport.clientWidth-NAME_WIDTH)),pct=function(day){return day/totalDays*100;};
function esc(value){return String(value==null?'':value).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/""/g,'&quot;').replace(/'/g,'&#39;');}
function dateAt(n){var a=report.StartDate.split('-').map(Number),d=new Date(a[0],a[1]-1,a[2]);d.setDate(d.getDate()+n);return d;}
function labelDate(d){return String(d.getDate()).padStart(2,'0')+'/'+String(d.getMonth()+1).padStart(2,'0');}
function dateTip(iso){var a=String(iso||'').split('-');return a.length===3?a[2]+'/'+a[1]+'/'+a[0]:iso;}
function remainingDays(task){return task.EndDate?Math.round((Date.UTC(Number(task.EndDate.slice(0,4)),Number(task.EndDate.slice(5,7))-1,Number(task.EndDate.slice(8,10)))-Date.UTC(dateAt(report.TodayIndex).getFullYear(),dateAt(report.TodayIndex).getMonth(),dateAt(report.TodayIndex).getDate()))/86400000):null;}
function displayStatus(task){var raw=String(task.Status||'todo');if(raw==='done')return {label:'Hoàn thành',css:'done',warning:'',tip:'Hoàn thành'};if(raw==='done-late')return {label:'Hoàn thành trễ',css:'done',warning:'',tip:'Hoàn thành trễ'};var label=raw==='review'?'Chờ duyệt':raw==='doing'?'Đang thực hiện':'Chưa bắt đầu';var css=raw==='review'||raw==='doing'?'doing':'todo',warning='',kind='';var rem=remainingDays(task);if(rem!==null){if(rem<0){warning='Trễ hạn '+Math.abs(rem)+' ngày';kind='overdue';}else if(rem<=2){warning=rem===0?'Sắp đến hạn (hôm nay)':'Sắp đến hạn (còn '+rem+' ngày)';kind='warning';}if(raw==='doing'&&kind)css=kind;}return {label:label,css:css,warning:warning,tip:warning||label,kind:kind};}
var rootHelp=document.getElementById('reportHelp'),rootLegend=document.getElementById('reportLegend'),helpHolder=document.getElementById('reportHelpHolder'),legendPinned=false;
function updateLegend(){var open=legendPinned||helpHolder.matches(':hover')||helpHolder.contains(document.activeElement);rootLegend.classList.toggle('is-open',open);rootLegend.setAttribute('aria-hidden',open?'false':'true');rootHelp.setAttribute('aria-expanded',open?'true':'false');rootHelp.classList.toggle('is-pinned',legendPinned);}
helpHolder.addEventListener('mouseenter',updateLegend);helpHolder.addEventListener('mouseleave',function(){if(!legendPinned){rootHelp.blur();updateLegend();}});
rootHelp.addEventListener('click',function(){legendPinned=!legendPinned;if(!legendPinned)rootHelp.blur();updateLegend();});
document.addEventListener('click',function(e){if(!helpHolder.contains(e.target)&&!legendPinned){rootHelp.blur();updateLegend();}});
header.style.width=chartWidth+'px';tracks.style.width=chartWidth+'px';chartCol.style.width=chartWidth+'px';body.style.width=(NAME_WIDTH+chartWidth)+'px';
for(var i=0;i<totalDays;i++){var tick=document.createElement('div');tick.className='tick-label';tick.style.left=pct(i)+'%';tick.style.width=pct(1)+'%';tick.textContent=labelDate(dateAt(i));header.appendChild(tick);}
if(!report.Tasks||!report.Tasks.length){taskRows.innerHTML='<div class=""empty-report"">Không có công việc được chọn.</div>';return;}
var rowById=Object.create(null),trackById=Object.create(null),barById=Object.create(null),toggleById=Object.create(null),collapsed=Object.create(null),descendants=Object.create(null),tip=document.getElementById('reportTip');
report.Tasks.forEach(function(task){descendants[task.Code]=report.Tasks.some(function(x){return String(x.Code||'').indexOf(String(task.Code)+'.')===0;});});
function bindTip(target,message,kind){target.addEventListener('mouseenter',function(){tip.textContent=message;tip.dataset.kind=kind||'';tip.style.display='block';placeTip(target);});target.addEventListener('mousemove',function(){placeTip(target);});target.addEventListener('mouseleave',function(){tip.style.display='none';});}
function placeTip(el){if(tip.style.display==='none')return;var r=el.getBoundingClientRect(),tw=tip.offsetWidth,th=tip.offsetHeight,left=r.right+9,top=r.top+(r.height-th)/2;if(left+tw>innerWidth-8)left=Math.max(8,r.left-tw-9);left=Math.max(8,Math.min(innerWidth-tw-8,left));top=Math.max(8,Math.min(innerHeight-th-8,top));tip.style.left=Math.round(left)+'px';tip.style.top=Math.round(top)+'px';}
report.Tasks.forEach(function(task){var code=String(task.Code||''),level=Math.max(1,parseInt(task.Level||1,10)),row=document.createElement('div');row.className='task-row';row.dataset.code=code;row.dataset.id=task.Id;var content=document.createElement('div');content.className=level===1?'task-level-1':'task-level-child';if(level>1){content.style.paddingLeft=Math.min(104,Math.max(0,(level-2)*26))+'px';var branch=document.createElement('span');branch.className='branch';content.appendChild(branch);}if(descendants[code]){content.classList.add('report-tree-expandable');content.setAttribute('role','button');content.tabIndex=0;content.setAttribute('aria-expanded','true');content.setAttribute('aria-label','Thu gọn hoặc mở rộng '+code+' '+String(task.Name||''));var arrow=document.createElement('span');arrow.className='report-tree-toggle';arrow.innerHTML='<span class=""report-tree-chevron""></span>';content.appendChild(arrow);toggleById[task.Id]=content;}
if(level===1){var icon=document.createElement('span');icon.className='task-list-icon';icon.textContent='☷';content.appendChild(icon);}var label=document.createElement('span');label.className='task-text';label.innerHTML='<span class=""task-code"">'+esc(code)+'.</span> '+esc(task.Name);content.appendChild(label);row.appendChild(content);taskRows.appendChild(row);rowById[task.Id]=row;
if(descendants[code]){var toggle=function(){collapsed[code]=!collapsed[code];refreshVisibility();};content.addEventListener('click',toggle);content.addEventListener('keydown',function(e){if(e.key==='Enter'||e.key===' '){e.preventDefault();toggle();}});}
var track=document.createElement('div');track.className='task-track';track.style.width=chartWidth+'px';track.style.backgroundImage='linear-gradient(to right,#f2f2f2 1px,transparent 1px)';track.style.backgroundSize=pct(1)+'% 100%';var start=Math.max(0,Math.min(Number(task.StartDay)||0,totalDays-1)),end=Math.max(start+1,Math.min(Number(task.EndDay)||start+1,totalDays)),s=displayStatus(task);
var bar=document.createElement('div');bar.className='bar status-'+s.css+(task.Status==='done-late'?' is-completed-late':'');bar.style.left=pct(start)+'%';bar.style.width=pct(end-start)+'%';bar.dataset.ganttTip=s.tip;
var fill=document.createElement('div');fill.className='bar-fill animated-stripes';var text=document.createElement('div');text.className='static-text';text.textContent=s.label;fill.appendChild(text);bar.appendChild(fill);bindTip(bar,s.tip+(task.EndDate?' · Hạn '+dateTip(task.EndDate):'')+(task.Assignee?' · '+task.Assignee:''),task.Status==='done-late'?'late':s.kind||s.css);
track.appendChild(bar);tracks.appendChild(track);trackById[task.Id]=track;barById[task.Id]=bar;});
var todayIndex=Number(report.TodayIndex);if(Number.isFinite(todayIndex)&&todayIndex>=0&&todayIndex<totalDays){var today=document.createElement('div');today.className='today-line';today.style.left=pct(todayIndex+.5)+'%';tracks.appendChild(today);}
var ns='http://www.w3.org/2000/svg',svg=document.createElementNS(ns,'svg'),defs=document.createElementNS(ns,'defs'),marker=document.createElementNS(ns,'marker'),head=document.createElementNS(ns,'path');svg.setAttribute('class','connections');svg.setAttribute('width',chartWidth);svg.setAttribute('aria-hidden','true');marker.setAttribute('id','reportDependencyArrow');marker.setAttribute('viewBox','0 0 10 10');marker.setAttribute('refX','8');marker.setAttribute('refY','5');marker.setAttribute('markerWidth','6');marker.setAttribute('markerHeight','6');marker.setAttribute('orient','auto');head.setAttribute('d','M 0 0 L 10 5 L 0 10 z');head.setAttribute('fill','#ff9800');marker.appendChild(head);defs.appendChild(marker);svg.appendChild(defs);tracks.appendChild(svg);
function isBarVisible(id){var t=trackById[id];return !!t&&t.style.display!=='none'&&!t.classList.contains('hide-bar');}
function drawDependencies(){Array.prototype.slice.call(svg.querySelectorAll('.dep-line')).forEach(function(p){p.parentNode.removeChild(p);});report.Tasks.forEach(function(task){if(!task.DependencyId||!isBarVisible(task.DependencyId)||!isBarVisible(task.Id))return;var from=trackById[task.DependencyId],to=trackById[task.Id],a=barById[task.DependencyId],b=barById[task.Id];if(!a||!b)return;var sx=a.offsetLeft+a.offsetWidth,sy=from.offsetTop+a.offsetTop+a.offsetHeight/2,ex=b.offsetLeft,ey=to.offsetTop+b.offsetTop+b.offsetHeight/2,elbow=sx+12,path=document.createElementNS(ns,'path');path.setAttribute('class','dep-line');path.setAttribute('marker-end','url(#reportDependencyArrow)');path.setAttribute('d','M '+sx+' '+sy+' L '+elbow+' '+sy+' L '+elbow+' '+ey+' L '+(ex-3)+' '+ey);svg.appendChild(path);});}
function refreshVisibility(){var count=0;report.Tasks.forEach(function(task){var code=String(task.Code||''),hidden=Object.keys(collapsed).some(function(parent){return !!collapsed[parent]&&code.indexOf(parent+'.')===0;});rowById[task.Id].style.display=hidden?'none':'';trackById[task.Id].style.display=hidden?'none':'';trackById[task.Id].classList.toggle('hide-bar',!!descendants[code]&&!collapsed[code]);if(toggleById[task.Id])toggleById[task.Id].setAttribute('aria-expanded',collapsed[code]?'false':'true');if(!hidden)count++;});var height=count*ROW_HEIGHT;tracks.style.height=Math.max(100,height)+'px';svg.setAttribute('height',height);svg.setAttribute('viewBox','0 0 '+chartWidth+' '+Math.max(1,height));requestAnimationFrame(drawDependencies);tip.style.display='none';}
refreshVisibility();
})();
</script></body></html>
";
        }

        private static DateTime? GetNullableDate(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return Convert.ToDateTime(row[column]).Date;
        }
    }

    public class ProgressReportRecord
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string ReportName { get; set; }
        public string OtpProtected { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public int TaskCount { get; set; }
        public string ProjectName { get; set; }
        public string OtpSalt { get; set; }
        public string OtpHash { get; set; }
        public string ReportHtml { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class ProgressReportTask
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Assignee { get; set; }
        public string DependencyId { get; set; }
        public int Level { get; set; }
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
    }

    public class ProgressReportChartData
    {
        public string ProjectName { get; set; }
        public string StartDate { get; set; }
        public int TotalDays { get; set; }
        public int TodayIndex { get; set; }
        public List<ProgressReportTask> Tasks { get; set; }
    }
}
