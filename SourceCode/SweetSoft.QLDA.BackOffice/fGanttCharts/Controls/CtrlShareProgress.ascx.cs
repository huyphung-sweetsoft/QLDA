using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fGanttCharts.Controls
{
    public partial class CtrlShareProgress : BaseAdminUserControl
    {
        public Guid ProjectId { get; set; }

        private static readonly object StorageLock = new object();
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 };

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
                if (preset == null) lblPresetStatus.Text = "Chưa có cấu hình riêng: mặc định chọn công việc cấp 1 và cấp 2.";
                else
                {
                    int extra = tasks == null ? 0 : tasks.Rows.Cast<DataRow>().Count(r => GetTaskLevel(Convert.ToString(r["MaCongViec"])) > 2 && checkedIds.Contains(Convert.ToString(r["IdCongViec"])));
                    lblPresetStatus.Text = "Đang dùng cấu hình đã lưu của dự án (" + extra.ToString(CultureInfo.InvariantCulture) + " công việc từ cấp 3 trở lên được chọn thêm).";
                }
            }
            else lblPresetStatus.Text = "Các ô đã chọn được giữ nguyên. Bấm Lưu cấu hình để ghi nhớ cho lần sau.";
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
                string[] postedIds = Request.Form.GetValues("shareProgressTaskIds") ?? new string[0];
                HashSet<string> selectedIds = new HashSet<string>(postedIds.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);
                if (selectedIds.Count == 0)
                {
                    const string message = "Vui lòng chọn ít nhất một công việc để tạo báo cáo.";
                    lblSelectionMessage.Text = message;
                    BindTaskChoices(selectedIds);
                    QueueClientModal(false, message);
                    return;
                }

                DataTable allTasks = GanttChartManager.Instance.GetGanttTasks(ProjectId);
                DataTable selectedTasks = allTasks?.Clone();
                if (selectedTasks != null && allTasks != null)
                {
                    foreach (DataRow row in allTasks.Rows)
                    {
                        string id = Convert.ToString(row["IdCongViec"]);
                        if (selectedIds.Contains(id)) selectedTasks.ImportRow(row);
                    }
                }

                if (selectedTasks == null || selectedTasks.Rows.Count == 0)
                {
                    lblSelectionMessage.Text = "Không tìm thấy các công việc đã chọn. Vui lòng mở lại danh sách và thử lại.";
                    BindTaskChoices(selectedIds);
                    QueueClientModal(false, lblSelectionMessage.Text);
                    return;
                }

                var project = DuAnManager.Instance.GetDuAnById(ProjectId);
                string projectName = !string.IsNullOrWhiteSpace(project?.TenDuAn) ? project.TenDuAn : "Dự án";
                string reportId = Guid.NewGuid().ToString("N");
                string otp = GenerateOtp();
                string salt = GenerateSalt();
                string reportHtml = BuildGanttReportHtml(projectName, selectedTasks);
                string fileName = "BaoCaoTienDo_" + reportId + ".html";
                string reportFolder = Server.MapPath("~/GeneratedProgressReports/");
                string dataFolder = Server.MapPath("~/App_Data/ProgressReports/");
                Directory.CreateDirectory(reportFolder);
                Directory.CreateDirectory(dataFolder);

                ProgressReportRecord record = new ProgressReportRecord
                {
                    Id = reportId,
                    ProjectName = projectName,
                    OtpSalt = salt,
                    OtpHash = HashOtp(salt, otp),
                    ReportHtml = reportHtml,
                    CreatedUtc = DateTime.UtcNow
                };

                string dataPath = Path.Combine(dataFolder, reportId + ".json");
                string htmlPath = Path.Combine(reportFolder, fileName);
                try
                {
                    lock (StorageLock)
                    {
                        File.WriteAllText(dataPath, Serializer.Serialize(record), new UTF8Encoding(false));
                        File.WriteAllText(htmlPath, BuildOtpGateHtml(reportId), new UTF8Encoding(false));
                    }
                }
                catch
                {
                    TryDelete(dataPath);
                    TryDelete(htmlPath);
                    throw;
                }

                string relativeReportUrl = VirtualPathUtility.ToAbsolute("~/GeneratedProgressReports/" + fileName);
                string reportUrl = Request.Url.GetLeftPart(UriPartial.Authority) + relativeReportUrl;

                ltrReportFileName.Text = "<span class=\"share-progress-file-name\">" + HttpUtility.HtmlEncode(fileName) + "</span>";
                lnkReportFile.NavigateUrl = reportUrl;
                ltrOtp.Text = HttpUtility.HtmlEncode(otp);
                hdfOtp.Value = otp;
                lblCreateMessage.Text = "Đã tạo báo cáo với " + selectedTasks.Rows.Count.ToString(CultureInfo.InvariantCulture) + " công việc. Hãy gửi đường dẫn và OTP riêng cho khách hàng.";

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

        private static string GenerateOtp()
        {
            byte[] bytes = new byte[4];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0) % 1000000U;
            return value.ToString("D6", CultureInfo.InvariantCulture);
        }

        private static string GenerateSalt()
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        internal static string HashOtp(string salt, string otp)
        {
            byte[] saltBytes = Convert.FromBase64String(salt ?? string.Empty);
            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(otp ?? string.Empty, saltBytes, 100000))
            {
                return Convert.ToBase64String(deriveBytes.GetBytes(32));
            }
        }

        private string BuildOtpGateHtml(string reportId)
        {
            string endpoint = VirtualPathUtility.ToAbsolute("~/GeneratedProgressReports/ProgressReportAccess.aspx");
            string jsReportId = HttpUtility.JavaScriptStringEncode(reportId);
            string jsEndpoint = HttpUtility.JavaScriptStringEncode(endpoint);

            return @"<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta name=""robots"" content=""noindex,nofollow,noarchive"">
<title>Mở báo cáo tiến độ</title>
<style>
*{box-sizing:border-box}body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;padding:24px;background:#f3f5f9;color:#1e293b;font-family:Arial,""Segoe UI"",sans-serif}.card{width:min(430px,100%);background:#fff;border:1px solid #e2e8f0;border-radius:14px;padding:30px;box-shadow:0 15px 45px rgba(15,23,42,.10)}.mark{display:flex;align-items:center;justify-content:center;width:48px;height:48px;margin:0 auto 15px;border-radius:12px;background:#f2eafe;color:#542e88;font-size:22px;font-weight:bold}.title{text-align:center;font-size:21px;font-weight:800;margin-bottom:8px}.sub{text-align:center;color:#64748b;font-size:13px;line-height:1.6;margin-bottom:22px}.label{display:block;font-weight:700;font-size:12px;margin-bottom:8px}.otp{width:100%;padding:13px 12px;border:1px solid #cbd5e1;border-radius:7px;text-align:center;font-size:24px;letter-spacing:8px;font-weight:800;outline:none}.otp:focus{border-color:#7c4dba;box-shadow:0 0 0 3px rgba(84,46,136,.11)}button{width:100%;margin-top:14px;padding:12px;border:0;border-radius:7px;background:#542e88;color:#fff;font-size:14px;font-weight:700;cursor:pointer}button:disabled{opacity:.65;cursor:wait}.msg{min-height:20px;margin-top:12px;font-size:12px;text-align:center;color:#b91c1c;line-height:1.5}.tiny{margin-top:18px;color:#94a3b8;font-size:11px;text-align:center}.spinner{display:none;width:21px;height:21px;border:3px solid #e9ddfb;border-top-color:#542e88;border-radius:50%;animation:spin .75s linear infinite;margin:16px auto 0}@keyframes spin{to{transform:rotate(360deg)}}
</style>
</head>
<body>
<main class=""card"">
<div class=""mark"">S</div>
<div class=""title"">Báo cáo tiến độ dự án</div>
<div class=""sub"">Nhập mã OTP 6 chữ số do người gửi cung cấp để mở khóa và xem biểu đồ Gantt.</div>
<form id=""otpForm"" autocomplete=""off""><label class=""label"" for=""otpInput"">MÃ OTP</label><input id=""otpInput"" class=""otp"" type=""password"" inputmode=""numeric"" autocomplete=""one-time-code"" pattern=""[0-9]{6}"" maxlength=""6"" minlength=""6"" required aria-label=""Mã OTP 6 chữ số""><button id=""unlockButton"" type=""submit"">Xác thực và xem báo cáo</button></form>
<div class=""spinner"" id=""spinner""></div><div class=""msg"" id=""message"" role=""status"" aria-live=""polite""></div><div class=""tiny"">SweetSoft · Báo cáo chia sẻ có xác thực</div>
</main>
<script>
(function(){
var reportId='" + jsReportId + @"';var accessUrl='" + jsEndpoint + @"';
var form=document.getElementById('otpForm'),input=document.getElementById('otpInput'),button=document.getElementById('unlockButton'),message=document.getElementById('message'),spinner=document.getElementById('spinner');
input.addEventListener('input',function(){input.value=input.value.replace(/\D/g,'').slice(0,6);});
form.addEventListener('submit',async function(e){e.preventDefault();var otp=input.value;if(!/^\d{6}$/.test(otp)){message.textContent='Vui lòng nhập đủ 6 chữ số.';return;}button.disabled=true;input.disabled=true;spinner.style.display='block';message.style.color='#64748b';message.textContent='Đang xác thực mã OTP...';
try{var response=await fetch(accessUrl,{method:'POST',headers:{'Content-Type':'application/json','Accept':'application/json'},cache:'no-store',credentials:'omit',body:JSON.stringify({reportId:reportId,otp:otp})});
var contentType=(response.headers.get('content-type')||'').toLowerCase();
if(contentType.indexOf('application/json')<0){var target=(response.url||'').toLowerCase();if(response.redirected||target.indexOf('login')>=0||target.indexOf('dangnhap')>=0){throw new Error('Máy chủ đang yêu cầu đăng nhập. Người quản trị cần cho phép truy cập ẩn danh thư mục GeneratedProgressReports.');}throw new Error('Máy chủ trả về HTML thay vì JSON (HTTP '+response.status+'). Hãy kiểm tra cấu hình xác thực báo cáo trên máy chủ.');}
var result;try{result=await response.json();}catch(parseError){throw new Error('Máy chủ trả JSON không hợp lệ. Vui lòng liên hệ người quản trị.');}
if(!response.ok||!result||!result.success){throw new Error(result&&result.message?result.message:'Không thể xác thực OTP.');}
if(typeof result.html!=='string'||!result.html){throw new Error('Báo cáo chưa có nội dung.');}document.open();document.write(result.html);document.close();}
catch(err){message.style.color='#b91c1c';message.textContent=err.message||'Không thể xác thực lúc này. Vui lòng thử lại.';button.disabled=false;input.disabled=false;input.value='';input.focus();spinner.style.display='none';}
});
})();
</script>
</body>
</html>";
        }

        private string BuildGanttReportHtml(string projectName, DataTable selectedTasks)
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
                    DependencyId = dependency,
                    Level = GetTaskLevel(Convert.ToString(row["MaCongViec"])),
                    StartDay = startDay,
                    EndDay = endDay,
                    EndDate = end.HasValue ? end.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty,
                    Status = statusValue == 2 ? "done" : statusValue == 1 ? "doing" : "todo"
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
            string encodedProjectName = HttpUtility.HtmlEncode(projectName);

            return @"<!doctype html>
                <html lang=""vi""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><meta name=""robots"" content=""noindex,nofollow,noarchive""><title>Biểu đồ Gantt - " + encodedProjectName + @"</title>
                <style>
                *{box-sizing:border-box}html,body{height:100%;margin:0;font-family:Arial,""Segoe UI"",sans-serif;color:#1f2937;background:#f3f6f9}body{padding:14px 15px 16px}.report-title{font-size:22px;line-height:1.3;font-weight:700;margin:0 0 16px;color:#111827}.legend{display:flex;align-items:center;gap:19px;flex-wrap:wrap;margin-bottom:18px;color:#555;font-size:13px}.legend-item{display:inline-flex;align-items:center;gap:5px;white-space:nowrap}.swatch{width:17px;height:17px;border-radius:4px;display:inline-block;background-image:linear-gradient(-45deg,rgba(255,255,255,.18) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.18) 50%,rgba(255,255,255,.18) 75%,transparent 75%,transparent);background-size:20px 20px}.done{background-color:#34a853}.doing{background-color:#4285f4}.warning{background-color:#f4b400}.overdue{background-color:#ea4335}.todo{background:#cbd5e1;background-image:none}.today-swatch{width:2px;height:17px;background:#ea4335;display:inline-block}.dependency-swatch{font-weight:bold;color:#ff9800;font-size:17px}.gantt-viewport{height:calc(100vh - 110px);min-height:440px;overflow:auto;background:#fff;border:1px solid #ccc;border-radius:7px;box-shadow:0 4px 12px rgba(0,0,0,.04);position:relative}.gantt-body{display:flex;width:max-content;min-width:100%;min-height:100%}.task-col{width:350px;flex:0 0 350px;position:sticky;left:0;z-index:20;background:#fff;border-right:1px solid #ccc;box-shadow:2px 0 8px rgba(0,0,0,.04)}.task-col-header,.chart-header{height:54px;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:13px;background:#f8f9fa;border-bottom:1px solid #ccc;position:sticky;top:0;z-index:30}.task-row,.task-track{height:52px;min-height:52px;border-bottom:1px solid #e5e5e5;position:relative;overflow:hidden}.task-row{display:flex;align-items:center;background:#fff;padding:0 8px;font-size:13px}.task-level-1{height:40px;display:flex;align-items:center;gap:7px;flex:1;min-width:0;background:#f4ebff;border-left:4px solid #5e35b1;border-radius:6px;color:#5e35b1;font-weight:700;padding:0 10px;overflow:hidden}.task-level-1 .task-text,.task-level-child .task-text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.task-list-icon{font-size:15px;flex-shrink:0}.task-level-child{width:100%;display:flex;align-items:center;min-width:0;color:#333;white-space:nowrap}.branch{display:inline-block;width:14px;height:18px;flex:0 0 14px;border-left:1.5px solid #aaa;border-bottom:1.5px solid #aaa;margin-right:8px;transform:translateY(-6px)}.task-code{font-weight:700;margin-right:4px}.chart-col{position:relative;flex:0 0 auto}.chart-header{display:block;position:sticky;top:0;z-index:25}.tick-label{position:absolute;top:0;height:100%;display:flex;align-items:center;justify-content:center;font-size:12px;color:#555;white-space:nowrap;border-left:1px solid #e0e0e0}.chart-tracks{position:relative}.task-track{border-bottom:1px solid #e5e5e5;background-color:#fff;background-image:linear-gradient(to right,#f2f2f2 1px,transparent 1px);background-repeat:repeat-x}.bar{position:absolute;top:14px;height:24px;border-radius:5px;overflow:visible;box-shadow:0 0 0 4px rgba(0,0,0,.035);z-index:6}.bar-fill{position:absolute;inset:0;border-radius:5px;overflow:hidden;background-image:linear-gradient(-45deg,rgba(255,255,255,.19) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.19) 50%,rgba(255,255,255,.19) 75%,transparent 75%,transparent);background-size:28px 28px;animation:stripe 1.5s linear infinite}.bar-label{position:absolute;left:8px;top:0;line-height:24px;font-size:11px;font-weight:700;color:rgba(255,255,255,.97);white-space:nowrap;pointer-events:none}.bar.todo .bar-label{color:#334155}.bar.overdue{box-shadow:0 0 0 4px rgba(234,67,53,.1)}.today-line{position:absolute;top:0;bottom:0;width:2px;background:#ea4335;z-index:4;pointer-events:none}.today-label{position:absolute;top:4px;transform:translateX(-50%);color:#ea4335;background:#fff;font-size:11px;font-weight:700;white-space:nowrap;z-index:28;padding:1px 3px;pointer-events:none}.connections{position:absolute;left:0;top:0;pointer-events:none;z-index:5;overflow:visible}.dep-line{fill:none;stroke:#ff9800;stroke-width:1.6;stroke-dasharray:4 4}.empty-report{padding:35px;color:#64748b;text-align:center}@keyframes stripe{to{background-position:28px 0}}
                /* Sticky task names must sit above every timeline layer when scrolling horizontally. */
                .gantt-viewport { isolation: isolate; }
                .gantt-body { position: relative; }
                .gantt-body > .task-col { position: sticky; left: 0; z-index: 80; background: #fff; }
                .gantt-body .task-col-header { z-index: 90; }
                .gantt-body > .chart-col { position: relative; z-index: 1; }
                .gantt-body .chart-header { z-index: 10; }
                .gantt-body .chart-tracks { position: relative; z-index: 1; }
                </style></head><body>
                <h1 class=""report-title"">Biểu đồ Gantt - " + encodedProjectName + @"</h1>
                <div class=""legend"">
                <span class=""legend-item""><span class=""swatch done""></span>Hoàn thành</span><span class=""legend-item""><span class=""swatch doing""></span>Đang thực hiện</span><span class=""legend-item""><span class=""swatch warning""></span>Sắp đến hạn</span><span class=""legend-item""><span class=""swatch overdue""></span>Trễ hạn</span><span class=""legend-item""><span class=""swatch todo""></span>Chưa bắt đầu</span><span class=""legend-item""><span class=""today-swatch""></span>Hôm nay</span><span class=""legend-item""><span class=""dependency-swatch"">➜</span>Liên kết phụ thuộc</span>
                </div>
                <div class=""gantt-viewport"" id=""ganttViewport""><div class=""gantt-body"" id=""ganttBody""><div class=""task-col"" id=""taskCol""><div class=""task-col-header"">Tên công việc</div><div id=""taskRows""></div></div><div class=""chart-col"" id=""chartCol""><div class=""chart-header"" id=""chartHeader""></div><div class=""chart-tracks"" id=""chartTracks""></div></div></div></div>
                <script>(function(){
                var encoded='" + encodedData + @"';var report=JSON.parse(decodeURIComponent(escape(atob(encoded))));var DAY_WIDTH=60;var ROW_HEIGHT=52;var NAME_WIDTH=350;var viewport=document.getElementById('ganttViewport');var taskRows=document.getElementById('taskRows');var chartCol=document.getElementById('chartCol');var header=document.getElementById('chartHeader');var tracks=document.getElementById('chartTracks');var body=document.getElementById('ganttBody');var totalDays=Math.max(1,report.TotalDays);var chartWidth=totalDays*DAY_WIDTH;var taskHeight=report.Tasks.length*ROW_HEIGHT;var pct=function(index){return index/totalDays*100;};
                function esc(value){return String(value==null?'':value).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/""/g,'&quot;').replace(/'/g,'&#39;');}
                function dateAt(dayOffset){var p=report.StartDate.split('-').map(Number);var d=new Date(p[0],p[1]-1,p[2]);d.setDate(d.getDate()+dayOffset);return d;}
                function dateText(d){return String(d.getDate()).padStart(2,'0')+'/'+String(d.getMonth()+1).padStart(2,'0');}
                function dayOffset(iso){if(!iso)return 0;var p=iso.split('-').map(Number);return Math.round((new Date(p[0],p[1]-1,p[2])-dateAt(0))/86400000);}
                function getEffectiveStatus(task){if(task.Status==='done')return 'done';if(!task.EndDate)return task.Status||'todo';var remaining=dayOffset(task.EndDate)-report.TodayIndex;if(remaining<0)return 'overdue';if(remaining<=2)return 'warning';return task.Status||'todo';}
                function barText(status){if(status==='done')return 'Hoàn thành';if(status==='doing')return 'Đang thực hiện';if(status==='warning')return 'Sắp đến hạn';if(status==='overdue')return 'Trễ hạn';return 'Chưa bắt đầu';}
                header.style.width=chartWidth+'px';tracks.style.width=chartWidth+'px';tracks.style.height=taskHeight+'px';chartCol.style.width=chartWidth+'px';body.style.width=(NAME_WIDTH+chartWidth)+'px';
                for(var i=0;i<totalDays;i++){var tick=document.createElement('div');tick.className='tick-label';tick.style.left=pct(i)+'%';tick.style.width=(100/totalDays)+'%';tick.textContent=dateText(dateAt(i));header.appendChild(tick);}
                if(!report.Tasks||report.Tasks.length===0){taskRows.innerHTML='<div class=""empty-report"">Không có công việc được chọn.</div>';return;}
                var taskTrackById={};var barById={};
                report.Tasks.forEach(function(task,index){var row=document.createElement('div');row.className='task-row';var level=Math.max(1,parseInt(task.Level||1,10));if(level===1){row.innerHTML='<div class=""task-level-1""><span class=""task-list-icon"">☷</span><span class=""task-text""><span class=""task-code"">'+esc(task.Code)+'.</span> '+esc(task.Name)+'</span></div>';}else{var indent=Math.max(0,(level-2)*26);row.innerHTML='<div class=""task-level-child"" style=""padding-left:'+(indent+8)+'px""><span class=""branch""></span><span class=""task-text""><span class=""task-code"">'+esc(task.Code)+'.</span> '+esc(task.Name)+'</span></div>';}
                taskRows.appendChild(row);
                var track=document.createElement('div');track.className='task-track';track.style.backgroundSize=(100/totalDays)+'% 100%';track.dataset.taskid=task.Id;track.style.height=ROW_HEIGHT+'px';track.style.width=chartWidth+'px';
                var start=Math.max(0,Math.min(parseInt(task.StartDay||0,10),totalDays-1));var end=Math.max(start+1,Math.min(parseInt(task.EndDay||start+1,10),totalDays));var status=getEffectiveStatus(task);var bar=document.createElement('div');bar.className='bar '+status;bar.style.left=pct(start)+'%';bar.style.width=((end-start)/totalDays*100)+'%';bar.title=barText(status)+(task.EndDate?' · Hạn: '+task.EndDate.split('-').reverse().join('/'):'');bar.innerHTML='<div class=""bar-fill""></div><span class=""bar-label"">'+esc(barText(status))+'</span>';track.appendChild(bar);tracks.appendChild(track);taskTrackById[task.Id]=track;barById[task.Id]=bar;
                });
                var svgNS='http://www.w3.org/2000/svg';var svg=document.createElementNS(svgNS,'svg');svg.setAttribute('class','connections');svg.setAttribute('width',String(chartWidth));svg.setAttribute('height',String(taskHeight));svg.setAttribute('viewBox','0 0 '+chartWidth+' '+taskHeight);svg.innerHTML='<defs><marker id=""dependencyArrow"" viewBox=""0 0 10 10"" refX=""8"" refY=""5"" markerWidth=""6"" markerHeight=""6"" orient=""auto""><path d=""M 0 0 L 10 5 L 0 10 z"" fill=""#ff9800""></path></marker></defs>';tracks.insertBefore(svg,tracks.firstChild);
                report.Tasks.forEach(function(task){if(!task.DependencyId||!taskTrackById[task.DependencyId]||!barById[task.DependencyId]||!barById[task.Id])return;var preTrack=taskTrackById[task.DependencyId],curTrack=taskTrackById[task.Id],preBar=barById[task.DependencyId],curBar=barById[task.Id];var sx=preBar.offsetLeft+preBar.offsetWidth,sy=preTrack.offsetTop+ROW_HEIGHT/2,ex=curBar.offsetLeft,ey=curTrack.offsetTop+ROW_HEIGHT/2;var mid=Math.max(sx+12,Math.min(ex-10,(sx+ex)/2));var path=document.createElementNS(svgNS,'path');path.setAttribute('class','dep-line');path.setAttribute('d','M '+sx+' '+sy+' L '+mid+' '+sy+' L '+mid+' '+ey+' L '+(ex-4)+' '+ey);path.setAttribute('marker-end','url(#dependencyArrow)');svg.appendChild(path);});
                var today=document.createElement('div');today.className='today-line';today.style.left=pct(report.TodayIndex+.5)+'%';tracks.appendChild(today);var todayLabel=document.createElement('div');todayLabel.className='today-label';todayLabel.textContent='Hôm nay';todayLabel.style.left=pct(report.TodayIndex+.5)+'%';header.appendChild(todayLabel);
                })();</script></body></html>";
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
