using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Infrastructure;
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
    public partial class CtrlShareProgress : BaseAdminUserControl
    {
        public Guid ProjectId { get; set; }

        private static readonly object StorageLock = new object();
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 };
        private const int HistoryPageSize = 5;
        private const string ConfirmOtpCommand = "GANTT_REPORT_RESET_OTP:";
        private const string ConfirmDeleteCommand = "GANTT_REPORT_DELETE:";
        private bool _confirmedHistoryAction;

        private void OpenReportConfirmation(string id, bool delete)
        {
            Guid parsed;
            if (!Guid.TryParseExact(id, "N", out parsed) || ProjectId == Guid.Empty)
            {
                ShowNotify("Báo cáo không hợp lệ.", MSGType.Error);
                return;
            }
            string safeId = parsed.ToString("N");
            string recordPath = Server.MapPath("~/App_Data/ProgressReports/" + safeId + ".json");
            string historyPath = Path.Combine(HistoryFolder, safeId + ".json");
            if (!File.Exists(recordPath) || !File.Exists(historyPath))
            {
                ShowNotify("Báo cáo không tồn tại hoặc đã bị xóa.", MSGType.Error);
                return;
            }
            ProgressReportHistoryItem history = Serializer.Deserialize<ProgressReportHistoryItem>(File.ReadAllText(historyPath, Encoding.UTF8));
            ProgressReportRecord record = Serializer.Deserialize<ProgressReportRecord>(File.ReadAllText(recordPath, Encoding.UTF8));
            Guid projectInHistory, projectInRecord;
            if (history == null || record == null ||
                !Guid.TryParse(history.ProjectId, out projectInHistory) ||
                !Guid.TryParse(record.ProjectId, out projectInRecord) ||
                projectInHistory != ProjectId || projectInRecord != ProjectId ||
                !string.Equals(history.ReportId, safeId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.Id, safeId, StringComparison.OrdinalIgnoreCase) ||
                (delete && !CanDeleteProgressReport(history)))
            {
                ShowNotify("Bạn không có quyền thao tác báo cáo này.", MSGType.Error);
                return;
            }
            if (Page.User == null || Page.User.Identity == null || !Page.User.Identity.IsAuthenticated || SweetContext.Current.UserId == Guid.Empty)
            {
                ShowNotify("Bạn cần đăng nhập để thực hiện thao tác này.", MSGType.Error);
                return;
            }
            var result = new ConfirmResult();
            result.CommandName = (delete ? ConfirmDeleteCommand : ConfirmOtpCommand) +
                ProjectId.ToString("N") + ":" + safeId;
            this.CURRENT_PAGE.CurrentConfirmResult = result;
            string title = delete ? "Xác nhận xóa báo cáo" : "Xác nhận cấp lại OTP";
            string message = delete
                ? "Xóa báo cáo “" + HttpUtility.HtmlEncode(history.ReportName ?? "Báo cáo tiến độ") +
                  "”? Liên kết đã gửi cho khách hàng sẽ không còn sử dụng được. Không thể hoàn tác."
                : "Cấp lại OTP cho báo cáo này? Mã OTP cũ sẽ hết hiệu lực; cần gửi mã mới cho khách hàng.";
            MessageBox box = new MessageBox(title, message,
                delete ? MSGButton.DeleteCancel : MSGButton.YesNo,
                delete ? MSGIcon.Error : MSGIcon.Warning);
            OpenMessageBox(box, result, false, false);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "GanttMsgBoxStack" + Guid.NewGuid().ToString("N"),
                "if(window.shareProgressStackSystemMessageBox){window.shareProgressStackSystemMessageBox();}", true);
        }

        protected void btnRequestProgressOtpReset_Click(object sender, EventArgs e)
        {
            HiddenField id = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetRequestId");
            if (id != null) OpenReportConfirmation(id.Value, false);
        }

        protected void btnRequestProgressDelete_Click(object sender, EventArgs e)
        {
            HiddenField id = FindHistoryControl<HiddenField>(this, "hdfProgressDeleteRequestId");
            if (id != null) OpenReportConfirmation(id.Value, true);
        }

        // Called from Gantts.ConfirmRequest after user confirms with MasterTemplate MessageBox.
        public override void ConfirmRequest(ConfirmResult result)
        {
            if (result == null || !result.Submit || string.IsNullOrEmpty(result.CommandName)) return;
            bool resetOtp = result.CommandName.StartsWith(ConfirmOtpCommand, StringComparison.Ordinal);
            bool delete = result.CommandName.StartsWith(ConfirmDeleteCommand, StringComparison.Ordinal);
            if (!resetOtp && !delete) return;
            string rest = result.CommandName.Substring((resetOtp ? ConfirmOtpCommand : ConfirmDeleteCommand).Length);
            string[] parts = rest.Split(':');
            Guid requestedProject, report;
            if (parts.Length != 2 || !Guid.TryParseExact(parts[0], "N", out requestedProject) ||
                requestedProject != ProjectId || !Guid.TryParseExact(parts[1], "N", out report)) return;
            string id = report.ToString("N");
            _confirmedHistoryAction = true;
            try
            {
                if (resetOtp)
                {
                    HiddenField f = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetRequestId");
                    if (f != null) f.Value = id;
                    btnResetProgressOtp_Click(this, EventArgs.Empty);
                    upShareProgressOtpReset.Update();
                }
                else
                {
                    HiddenField f = FindHistoryControl<HiddenField>(this, "hdfProgressDeleteRequestId");
                    if (f != null) f.Value = id;
                    btnDeleteProgressReport_Click(this, EventArgs.Empty);
                    upShareProgressDelete.Update();
                }
                string code = "if(window.shareProgressApplyConfirmedAction){window.shareProgressApplyConfirmedAction('" +
                    (resetOtp ? "otp" : "delete") + "','" + id + "');}";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "GanttResult" + Guid.NewGuid().ToString("N"), code, true);
            }
            finally { _confirmedHistoryAction = false; }
        }

        private int HistoryPage
        {
            get { return ViewState["ProgressHistoryPage"] is int ? (int)ViewState["ProgressHistoryPage"] : 0; }
            set { ViewState["ProgressHistoryPage"] = Math.Max(0, value); }
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
        private static string ProtectReportOtp(string otp)
        {
            byte[] encrypted = MachineKey.Protect(Encoding.UTF8.GetBytes(otp), "SweetSoft.QLDA", "GanttReport", "OtpHistoryV1");
            if (encrypted == null) throw new CryptographicException("Không thể mã hóa OTP.");
            return Convert.ToBase64String(encrypted);
        }
        private static string UnprotectReportOtp(string protectedValue)
        {
            if (string.IsNullOrWhiteSpace(protectedValue)) return null;
            try
            {
                byte[] plain = MachineKey.Unprotect(Convert.FromBase64String(protectedValue), "SweetSoft.QLDA", "GanttReport", "OtpHistoryV1");
                string otp = plain == null ? null : Encoding.UTF8.GetString(plain);
                return otp != null && System.Text.RegularExpressions.Regex.IsMatch(otp, "^[0-9]{6}$") ? otp : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Không giải mã được OTP báo cáo: " + ex);
                return null;
            }
        }

        // History UI controls are resolved dynamically to avoid handwritten designer.cs changes.
        private static T FindHistoryControl<T>(Control parent, string id) where T : Control
        {
            if (parent == null) return null;
            if (parent.ID == id) return parent as T;
            foreach (Control child in parent.Controls)
            {
                T found = FindHistoryControl<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private string HistoryFolder
        {
            get { return Server.MapPath("~/App_Data/ProgressReports/History/" + ProjectId.ToString("N") + "/"); }
        }

        public sealed class ProgressReportHistoryItem
        {
            public string ReportId { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string ReportName { get; set; }
            public string CreatedById { get; set; }
            public string CreatedByName { get; set; }
            public DateTime CreatedUtc { get; set; }
            public int TaskCount { get; set; }
            public string[] SelectedCodes { get; set; }
            public string FileName { get; set; }
        }

        protected void btnLoadProgressHistory_Click(object sender, EventArgs e)
        {
            HistoryPage = 0;
            OpenProgressHistory(true);
        }

        protected void btnHistoryNavigate_Click(object sender, EventArgs e)
        {
            LinkButton link = sender as LinkButton;
            HistoryPage += link != null && link.CommandArgument == "next" ? 1 : -1;
            // Không mở lại modal: chỉ cập nhật list/pagination trong UpdatePanel con.
            OpenProgressHistory(false);
        }

        // Keep this inside its own tiny UpdatePanel: never refresh the history modal DOM.
        protected void btnResetProgressOtp_Click(object sender, EventArgs e)
        {
            if (!_confirmedHistoryAction) return;
            HiddenField requestId = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetRequestId");
            HiddenField status = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetStatus");
            HiddenField newOtpField = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetValue");
            HiddenField messageField = FindHistoryControl<HiddenField>(this, "hdfProgressOtpResetMessage");
            if (requestId == null || status == null || newOtpField == null || messageField == null)
                throw new InvalidOperationException("Thiếu điều khiển trả kết quả cấp lại OTP.");
            status.Value = "error";
            newOtpField.Value = string.Empty;
            messageField.Value = "Không thể cấp lại mã OTP.";
            try
            {
                if (ProjectId == Guid.Empty || Page.User == null || Page.User.Identity == null ||
                    !Page.User.Identity.IsAuthenticated || SweetContext.Current.UserId == Guid.Empty)
                    throw new UnauthorizedAccessException("Không có quyền cấp lại mã OTP.");
                Guid parsed;
                if (!Guid.TryParseExact(requestId.Value, "N", out parsed))
                    throw new InvalidOperationException("Mã báo cáo không hợp lệ.");
                string safeId = parsed.ToString("N");
                string historyPath = Path.Combine(HistoryFolder, safeId + ".json");
                string recordPath = Server.MapPath("~/App_Data/ProgressReports/" + safeId + ".json");
                string otp;
                lock (StorageLock)
                {
                    if (!File.Exists(historyPath) || !File.Exists(recordPath))
                        throw new InvalidOperationException("Không tìm thấy báo cáo trong nhật ký dự án.");
                    ProgressReportHistoryItem history = Serializer.Deserialize<ProgressReportHistoryItem>(File.ReadAllText(historyPath, Encoding.UTF8));
                    ProgressReportRecord record = Serializer.Deserialize<ProgressReportRecord>(File.ReadAllText(recordPath, Encoding.UTF8));
                    Guid hiProject, recProject;
                    if (history == null || record == null || !Guid.TryParse(history.ProjectId, out hiProject) ||
                        !Guid.TryParse(record.ProjectId, out recProject) || hiProject != ProjectId || recProject != ProjectId ||
                        !string.Equals(history.ReportId, safeId, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(record.Id, safeId, StringComparison.OrdinalIgnoreCase))
                        throw new UnauthorizedAccessException("Báo cáo không thuộc dự án đang xem.");
                    otp = GenerateOtp();
                    string salt = GenerateSalt();
                    record.OtpSalt = salt;
                    record.OtpHash = HashOtp(salt, otp);
                    record.OtpProtected = ProtectReportOtp(otp);
                    File.WriteAllText(recordPath, Serializer.Serialize(record), new UTF8Encoding(false));
                }
                status.Value = "success";
                newOtpField.Value = otp;
                messageField.Value = "Đã cấp lại OTP, mã cũ không còn hiệu lực.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Cấp lại OTP báo cáo thất bại: " + ex);
                messageField.Value = ex is UnauthorizedAccessException ? "Bạn không có quyền cấp lại OTP." :
                    (ex is InvalidOperationException ? ex.Message : "Không thể cấp lại OTP. Vui lòng thử lại.");
            }
            finally { requestId.Value = string.Empty; }
        }

        // Xoa ban HTML chia se + JSON OTP + nhat ky, khong dung vao DOM popup.
        // Quyen: nguoi tao hoac nguoi quan ly du an. Moi report duoc xac minh ProjectId tai server.
        private bool CanDeleteProgressReport(ProgressReportHistoryItem history)
        {
            if (history == null || ProjectId == Guid.Empty || Page.User == null ||
                Page.User.Identity == null || !Page.User.Identity.IsAuthenticated ||
                SweetContext.Current.UserId == Guid.Empty) return false;
            Guid creator;
            if (Guid.TryParse(history.CreatedById, out creator) && creator == SweetContext.Current.UserId)
                return true;
            Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
            return pm.HasValue && pm.Value == SweetContext.Current.UserId;
        }

        protected void btnDeleteProgressReport_Click(object sender, EventArgs e)
        {
            if (!_confirmedHistoryAction) return;
            HiddenField idField = FindHistoryControl<HiddenField>(this, "hdfProgressDeleteRequestId");
            HiddenField statusField = FindHistoryControl<HiddenField>(this, "hdfProgressDeleteStatus");
            HiddenField messageField = FindHistoryControl<HiddenField>(this, "hdfProgressDeleteMessage");
            if (idField == null || statusField == null || messageField == null)
                throw new InvalidOperationException("Thiếu điều khiển trả kết quả xóa báo cáo.");
            statusField.Value = "error";
            messageField.Value = "Không thể xóa báo cáo.";
            try
            {
                Guid parsed;
                if (!Guid.TryParseExact(idField.Value, "N", out parsed))
                    throw new InvalidOperationException("Mã báo cáo không hợp lệ.");
                string reportId = parsed.ToString("N");
                string historyPath = Path.Combine(HistoryFolder, reportId + ".json");
                string recordPath = Server.MapPath("~/App_Data/ProgressReports/" + reportId + ".json");
                string htmlPath = Server.MapPath("~/GeneratedProgressReports/BaoCaoTienDo_" + reportId + ".html");
                lock (StorageLock)
                {
                    if (!File.Exists(historyPath) || !File.Exists(recordPath))
                        throw new InvalidOperationException("Báo cáo không tồn tại trong nhật ký dự án.");
                    ProgressReportHistoryItem history = Serializer.Deserialize<ProgressReportHistoryItem>(File.ReadAllText(historyPath, Encoding.UTF8));
                    ProgressReportRecord record = Serializer.Deserialize<ProgressReportRecord>(File.ReadAllText(recordPath, Encoding.UTF8));
                    Guid historyProject, recordProject;
                    if (history == null || record == null ||
                        !Guid.TryParse(history.ProjectId, out historyProject) ||
                        !Guid.TryParse(record.ProjectId, out recordProject) ||
                        historyProject != ProjectId || recordProject != ProjectId ||
                        !string.Equals(history.ReportId, reportId, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(record.Id, reportId, StringComparison.OrdinalIgnoreCase))
                        throw new UnauthorizedAccessException("Báo cáo không thuộc dự án đang xem.");
                    if (!CanDeleteProgressReport(history))
                        throw new UnauthorizedAccessException("Chỉ người tạo hoặc quản lý dự án được xóa báo cáo.");

                    // Chuyen het file vao App_Data truoc khi xoa. Neu gap loi, khoi phuc cac file da chuyen.
                    // Cach nay ngat truy cap cong khai truoc va tranh de lai report nửa xoa tai duong dan cu.
                    string staging = Server.MapPath("~/App_Data/ProgressReports/_Deleting/" + reportId + "_" + Guid.NewGuid().ToString("N") + "/");
                    Directory.CreateDirectory(staging);
                    var moved = new List<KeyValuePair<string, string>>();
                    try
                    {
                        string[] sources = new[] { htmlPath, recordPath, historyPath };
                        for (int i = 0; i < sources.Length; i++)
                        {
                            if (!File.Exists(sources[i])) continue;
                            string destination = Path.Combine(staging, (i + 1).ToString(CultureInfo.InvariantCulture) + ".deleted");
                            File.Move(sources[i], destination);
                            moved.Add(new KeyValuePair<string, string>(sources[i], destination));
                        }
                    }
                    catch
                    {
                        for (int i = moved.Count - 1; i >= 0; i--)
                        {
                            try { if (File.Exists(moved[i].Value)) File.Move(moved[i].Value, moved[i].Key); }
                            catch (Exception rollbackError) { System.Diagnostics.Trace.TraceError("Không khôi phục được file báo cáo khi xóa lỗi: " + rollbackError); }
                        }
                        throw;
                    }
                    try { Directory.Delete(staging, true); }
                    catch (Exception cleanupError) { System.Diagnostics.Trace.TraceError("Báo cáo đã vô hiệu hóa nhưng không thể dọn file tạm: " + cleanupError); }
                }
                statusField.Value = "success";
                messageField.Value = "Đã xóa báo cáo. Liên kết và OTP cũ không còn sử dụng được.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Không thể xóa báo cáo Gantt: " + ex);
                messageField.Value = ex is UnauthorizedAccessException ? "Bạn không có quyền xóa báo cáo này." :
                    (ex is InvalidOperationException ? ex.Message : "Không thể xóa báo cáo. Vui lòng thử lại.");
            }
            finally { idField.Value = string.Empty; }
        }

        private void OpenProgressHistory(bool reopenModal)
        {
            if (ProjectId == Guid.Empty || Page.User == null || Page.User.Identity == null ||
                !Page.User.Identity.IsAuthenticated || SweetContext.Current.UserId == Guid.Empty)
            {
                ShowNotify("Bạn cần đăng nhập để xem nhật ký báo cáo của dự án.", MSGType.Error);
                return;
            }
            try
            {
                List<ProgressReportHistoryItem> entries = new List<ProgressReportHistoryItem>();
                if (Directory.Exists(HistoryFolder))
                {
                    foreach (string path in Directory.EnumerateFiles(HistoryFolder, "*.json"))
                    {
                        try
                        {
                            ProgressReportHistoryItem entry = Serializer.Deserialize<ProgressReportHistoryItem>(File.ReadAllText(path, Encoding.UTF8));
                            Guid reportId, projectId;
                            if (entry == null || !Guid.TryParseExact(entry.ReportId, "N", out reportId) ||
                                !Guid.TryParse(entry.ProjectId, out projectId) || projectId != ProjectId ||
                                !string.Equals(Path.GetFileNameWithoutExtension(path), entry.ReportId, StringComparison.OrdinalIgnoreCase)) continue;
                            entries.Add(entry);
                        }
                        catch (Exception badRecord)
                        {
                            System.Diagnostics.Trace.TraceWarning("Nhật ký báo cáo không hợp lệ: " + badRecord);
                        }
                    }
                }
                entries.Sort((x, y) => y.CreatedUtc.CompareTo(x.CreatedUtc));
                int pageCount = Math.Max(1, (entries.Count + HistoryPageSize - 1) / HistoryPageSize);
                HistoryPage = Math.Min(HistoryPage, pageCount - 1);
                StringBuilder html = new StringBuilder("<div class='share-progress-history-list'>");
                if (entries.Count == 0)
                    html.Append("<div class='share-progress-history-empty'><i class='far fa-folder-open'></i><div>Chưa có báo cáo trong nhật ký dự án.</div></div>");
                foreach (ProgressReportHistoryItem entry in entries.Skip(HistoryPage * HistoryPageSize).Take(HistoryPageSize))
                {
                    // Never trust metadata filenames for filesystem paths.
                    string filename = "BaoCaoTienDo_" + entry.ReportId + ".html";
                    string relativeUrl = VirtualPathUtility.ToAbsolute("~/GeneratedProgressReports/" + filename);
                    string fullUrl = Request.Url.GetLeftPart(UriPartial.Authority) + relativeUrl;
                    string urlAttr = HttpUtility.HtmlAttributeEncode(fullUrl).Replace("'", "&#39;");
                    bool exists = File.Exists(Server.MapPath("~/GeneratedProgressReports/" + filename));
                    string title = !string.IsNullOrWhiteSpace(entry.ReportName) ? entry.ReportName :
                        (!string.IsNullOrWhiteSpace(entry.ProjectName) ? entry.ProjectName + " · Báo cáo " + entry.CreatedUtc.ToString("ddMMyy", CultureInfo.InvariantCulture) : "Báo cáo " + entry.ReportId.Substring(0, 8));
                    string titleAttr = HttpUtility.HtmlAttributeEncode(title).Replace("'", "&#39;");
                    DateTime date = entry.CreatedUtc.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(entry.CreatedUtc, DateTimeKind.Utc) : entry.CreatedUtc.ToUniversalTime();
                    string made = date.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                    string otp = null;
                    bool canRenew = false;
                    try
                    {
                        string recordPath = Server.MapPath("~/App_Data/ProgressReports/" + entry.ReportId + ".json");
                        if (File.Exists(recordPath))
                        {
                            ProgressReportRecord record = Serializer.Deserialize<ProgressReportRecord>(File.ReadAllText(recordPath, Encoding.UTF8));
                            Guid recordProject;
                            if (record != null && Guid.TryParse(record.ProjectId, out recordProject) && recordProject == ProjectId &&
                                string.Equals(record.Id, entry.ReportId, StringComparison.OrdinalIgnoreCase))
                            {
                                canRenew = true;
                                otp = UnprotectReportOtp(record.OtpProtected);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning("Không đọc được OTP báo cáo " + entry.ReportId + ": " + ex);
                    }
                    html.Append("<div class='share-progress-history-row' data-report-id='")
                        .Append(HttpUtility.HtmlAttributeEncode(entry.ReportId))
                        .Append("' data-report-name='").Append(titleAttr).Append("'>");
                    html.Append("<div class='share-progress-history-core'><div class='share-progress-history-main'>")
                        .Append("<button type='button' class='share-progress-history-name-btn' aria-expanded='false' onclick=\"return shareProgressToggleHistoryRow(event,this);\" title='Mở hoặc thu gọn thông tin chia sẻ'>")
                        .Append("<span class='share-progress-history-name-mark'><i class='far fa-file-alt'></i></span>")
                        .Append("<span class='share-progress-history-name-text'>").Append(HttpUtility.HtmlEncode(title)).Append("</span></button>")
                        .Append("<div class='share-progress-history-meta'><span><i class='far fa-calendar-alt'></i> ")
                        .Append(made).Append("</span><span><i class='far fa-user'></i> ")
                        .Append(HttpUtility.HtmlEncode(entry.CreatedByName ?? "Không rõ người tạo"))
                        .Append("</span></div></div>");
                    // Cac thao tac cua bao cao nam chung tren hang tieu de.
                    // Nut xoa chi hien khi user co quyen; server van kiem tra lai truoc khi xoa.
                    html.Append("<div class='share-progress-history-actions' role='group' aria-label='Thao tác báo cáo'>");
                    if (exists)
                        html.Append("<a class='share-progress-history-open' href='").Append(urlAttr)
                            .Append("' target='_blank' rel='noopener noreferrer' aria-label='Mở file báo cáo' title='Mở file báo cáo'><i class='fas fa-file-export' aria-hidden='true'></i></a>");
                    else html.Append("<span class='share-progress-history-open is-missing' title='Không còn file' aria-label='Không còn file'><i class='fas fa-file-export' aria-hidden='true'></i></span>");
                    if (canRenew && CanDeleteProgressReport(entry))
                        html.Append("<button type='button' class='share-progress-history-delete' aria-label='Xóa báo cáo' title='Xóa báo cáo' data-report-id='")
                            .Append(HttpUtility.HtmlAttributeEncode(entry.ReportId))
                            .Append("' onclick='return shareProgressDeleteHistoryReport(event,this);'><i class='far fa-trash-alt' aria-hidden='true'></i></button>");
                    html.Append("</div></div><div class='share-progress-history-extra'><div class='share-progress-history-extra-inner'><div class='share-progress-history-share'>");
                    if (exists)
                    {
                        html.Append("<div class='share-progress-history-share-line'><label>Liên kết</label>")
                            .Append("<input type='hidden' class='share-progress-history-link-hidden share-progress-history-value' value='")
                            .Append(urlAttr).Append("' />")
                            .Append("<a class='share-progress-history-link-action' href='").Append(urlAttr)
                            .Append("' target='_blank' rel='noopener noreferrer'><i class='fas fa-link'></i> Mở báo cáo <i class='fas fa-external-link-alt'></i></a>")
                            .Append("<button type='button' class='share-progress-history-icon' onclick=\"return copyProgressHistoryValue(this,'link');\" title='Sao chép liên kết'><i class='far fa-copy'></i></button></div>");
                    }
                    html.Append("<div class='share-progress-history-share-line is-otp-line'><label>Mã OTP</label><span class='share-progress-history-otp-slot'>");
                    if (!string.IsNullOrEmpty(otp))
                        html.Append("<input class='share-progress-history-value is-otp' type='text' readonly value='")
                            .Append(HttpUtility.HtmlAttributeEncode(otp))
                            .Append("' aria-label='Mã OTP báo cáo'/><button type='button' class='share-progress-history-icon' onclick=\"return copyProgressHistoryValue(this,'otp');\" title='Sao chép OTP'><i class='far fa-copy'></i></button>");
                    else html.Append("<span class='share-progress-history-reissue'>Chưa có OTP khả dụng</span>");
                    html.Append("</span>");
                    if (canRenew)
                        html.Append("<button type='button' class='share-progress-history-icon share-progress-history-reset' data-report-id='")
                            .Append(HttpUtility.HtmlAttributeEncode(entry.ReportId))
                            .Append("' onclick='return shareProgressResetHistoryOtp(event,this);'><i class='fas fa-key'></i> Cấp lại OTP</button>");
                    if (exists && canRenew)
                        html.Append("<button type='button' class='share-progress-history-icon share-progress-history-send' onclick='return shareProgressSendCustomer(this);'><i class='fas fa-paper-plane'></i> Gửi khách hàng</button>");
                    // Thu gon/mo rong bang cach bam vao ten bao cao, khong can nut mui ten.
                    html.Append("</div></div>"); // dong dong OTP va vung chia se
                    html.Append("</div></div></div>"); // dong inner details, details va item
                }
                html.Append("</div>");
                Literal rows = FindHistoryControl<Literal>(this, "ltrProgressHistory");
                Label count = FindHistoryControl<Label>(this, "lblProgressHistoryCount");
                Label page = FindHistoryControl<Label>(this, "lblProgressHistoryPage");
                LinkButton prev = FindHistoryControl<LinkButton>(this, "btnHistoryPrevious");
                LinkButton next = FindHistoryControl<LinkButton>(this, "btnHistoryNext");
                ExtraModal modal = FindHistoryControl<ExtraModal>(this, "modalProgressHistory");
                // Truy xuất bằng ID để không phải khai báo thêm field trong ASCX.designer.cs.
                UpdatePanel historyPanel = FindHistoryControl<UpdatePanel>(this, "upShareProgressHistory");
                if (rows == null || count == null || page == null || prev == null || next == null || modal == null || historyPanel == null)
                    throw new InvalidOperationException("Không tìm thấy control nhật ký trong ASCX.");
                rows.Text = html.ToString();
                count.Text = entries.Count.ToString(CultureInfo.InvariantCulture) + " báo cáo";
                page.Text = "Trang " + (HistoryPage + 1) + "/" + pageCount;
                prev.Enabled = HistoryPage > 0;
                next.Enabled = HistoryPage + 1 < pageCount;
                historyPanel.Update();
                if (reopenModal) modal.OpenModal(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Không tải được nhật ký báo cáo: " + ex);
                ShowNotify("Không tải được nhật ký báo cáo. Vui lòng thử lại.", MSGType.Error);
            }
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
                string reportName = (Request.Form["shareProgressReportName"] ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(reportName)) reportName = BuildDefaultReportName(projectName, DateTime.Now);
                reportName = reportName.Replace("\r", " ").Replace("\n", " ").Trim();
                if (reportName.Length > 120) throw new InvalidOperationException("Tên báo cáo chỉ được tối đa 120 ký tự.");
                string reportId = Guid.NewGuid().ToString("N");
                string otp = GenerateOtp();
                string salt = GenerateSalt();
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
                    OtpSalt = salt,
                    OtpHash = HashOtp(salt, otp),
                    OtpProtected = ProtectReportOtp(otp),
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
                        File.WriteAllText(htmlPath, BuildOtpGateHtml(reportId), new UTF8Encoding(false));
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
            // Giu ten tuy chinh trong nhat ky; tieu de HTML luon la ten du an + thoi diem tao ban chup.
            string title = "Báo cáo tiến độ - " + projectName + " (" +
                createdUtc.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN")) + ")";
            string encodedReportTitle = HttpUtility.HtmlEncode(title);

            return @"<!doctype html>
                <html lang=""vi""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><meta name=""robots"" content=""noindex,nofollow,noarchive""><title>" + encodedReportTitle + @"</title>
                <style>
                *{box-sizing:border-box}html,body{height:100%;margin:0;font-family:Arial,""Segoe UI"",sans-serif;color:#1f2937;background:#f3f6f9}body{padding:14px 15px 16px}.report-title{font-size:22px;line-height:1.3;font-weight:700;margin:0 0 16px;color:#111827}.legend{display:flex;align-items:center;gap:19px;flex-wrap:wrap;margin-bottom:18px;color:#555;font-size:13px}.legend-item{display:inline-flex;align-items:center;gap:5px;white-space:nowrap}.swatch{width:17px;height:17px;border-radius:4px;display:inline-block;background-image:linear-gradient(-45deg,rgba(255,255,255,.18) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.18) 50%,rgba(255,255,255,.18) 75%,transparent 75%,transparent);background-size:20px 20px}.done{background-color:#34a853}.doing{background-color:#4285f4}.warning{background-color:#f4b400}.overdue{background-color:#ea4335}.todo{background:#cbd5e1;background-image:none}.today-swatch{width:2px;height:17px;background:#ea4335;display:inline-block}.dependency-swatch{font-weight:bold;color:#ff9800;font-size:17px}.gantt-viewport{height:calc(100vh - 110px);min-height:440px;overflow:auto;background:#fff;border:1px solid #ccc;border-radius:7px;box-shadow:0 4px 12px rgba(0,0,0,.04);position:relative}.gantt-body{display:flex;width:max-content;min-width:100%;min-height:100%}.task-col{width:350px;flex:0 0 350px;position:sticky;left:0;z-index:20;background:#fff;border-right:1px solid #ccc;box-shadow:2px 0 8px rgba(0,0,0,.04)}.task-col-header,.chart-header{height:54px;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:13px;background:#f8f9fa;border-bottom:1px solid #ccc;position:sticky;top:0;z-index:30}.task-row,.task-track{height:52px;min-height:52px;border-bottom:1px solid #e5e5e5;position:relative;overflow:hidden}.task-row{display:flex;align-items:center;background:#fff;padding:0 8px;font-size:13px}.task-level-1{height:40px;display:flex;align-items:center;gap:7px;flex:1;min-width:0;background:#f4ebff;border-left:4px solid #5e35b1;border-radius:6px;color:#5e35b1;font-weight:700;padding:0 10px;overflow:hidden}.task-level-1 .task-text,.task-level-child .task-text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.task-list-icon{font-size:15px;flex-shrink:0}.task-level-child{width:100%;display:flex;align-items:center;min-width:0;color:#333;white-space:nowrap}.branch{display:inline-block;width:14px;height:18px;flex:0 0 14px;border-left:1.5px solid #aaa;border-bottom:1.5px solid #aaa;margin-right:8px;transform:translateY(-6px)}.task-code{font-weight:700;margin-right:4px}.chart-col{position:relative;flex:0 0 auto}.chart-header{display:block;position:sticky;top:0;z-index:25}.tick-label{position:absolute;top:0;height:100%;display:flex;align-items:center;justify-content:center;font-size:12px;color:#555;white-space:nowrap;border-left:1px solid #e0e0e0}.chart-tracks{position:relative}.task-track{border-bottom:1px solid #e5e5e5;background-color:#fff;background-image:linear-gradient(to right,#f2f2f2 1px,transparent 1px);background-repeat:repeat-x}.bar{position:absolute;top:14px;height:24px;border-radius:5px;overflow:visible;box-shadow:0 0 0 4px rgba(0,0,0,.035);z-index:6}.bar-fill{position:absolute;inset:0;border-radius:5px;overflow:hidden;background-image:linear-gradient(-45deg,rgba(255,255,255,.19) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.19) 50%,rgba(255,255,255,.19) 75%,transparent 75%,transparent);background-size:28px 28px;animation:stripe 1.5s linear infinite}.bar-label{position:absolute;left:8px;top:0;line-height:24px;font-size:11px;font-weight:700;color:rgba(255,255,255,.97);white-space:nowrap;pointer-events:none}.bar.todo .bar-label{color:#334155}.bar.overdue{box-shadow:0 0 0 4px rgba(234,67,53,.1)}.today-line{position:absolute;top:0;bottom:0;width:2px;background:#ea4335;z-index:4;pointer-events:none}.today-label{position:absolute;top:4px;transform:translateX(-50%);color:#ea4335;background:#fff;font-size:11px;font-weight:700;white-space:nowrap;z-index:28;padding:1px 3px;pointer-events:none}.connections{position:absolute;left:0;top:0;pointer-events:none;z-index:5;overflow:visible}.dep-line{fill:none;stroke:#ff9800;stroke-width:1.6;stroke-dasharray:4 4}.empty-report{padding:35px;color:#64748b;text-align:center}@keyframes stripe{to{background-position:28px 0}}
                /* Cay cong viec: thu gon/mo rong cung luc phan ten, thanh tien do va duong phu thuoc. */
                /* Chevron nhỏ như Gantt chính, bấm toàn vùng tên task (không tạo nút vuông). */
                .report-tree-expandable{cursor:pointer;transition:background-color .16s ease,color .16s ease}
                .task-level-1.report-tree-expandable:hover{background:#ece0fb}
                .task-level-child.report-tree-expandable{min-height:40px;border-radius:5px}
                .task-level-child.report-tree-expandable:hover{background:#f6f0fe;color:#512192}
                .report-tree-expandable:focus-visible{outline:2px solid #9665cf;outline-offset:-2px}
                .report-tree-toggle{flex:0 0 15px;width:15px;height:22px;margin-right:2px;display:inline-flex;align-items:center;justify-content:center;pointer-events:none;color:#526070}
                .task-level-1 .report-tree-toggle{color:#5e35b1}
                .report-tree-chevron{width:7px;height:7px;display:block;border-right:2px solid currentColor;border-bottom:2px solid currentColor;transform:rotate(45deg) translate(-1px,-1px);transition:transform .2s ease}
                .report-tree-expandable[aria-expanded=""false""] .report-tree-chevron{transform:rotate(-45deg)}
                /* Sticky task names must sit above every timeline layer when scrolling horizontally. */
                .gantt-viewport { isolation: isolate; }
                .gantt-body { position: relative; }
                .gantt-body > .task-col { position: sticky; left: 0; z-index: 80; background: #fff; }
                .gantt-body .task-col-header { z-index: 90; }
                .gantt-body > .chart-col { position: relative; z-index: 1; }
                .gantt-body .chart-header { z-index: 10; }
                .gantt-body .chart-tracks { position: relative; z-index: 1; }
                </style></head><body>
                <h1 class=""report-title"">" + encodedReportTitle + @"</h1>
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
                // Thu gon theo ma cong viec, khong theo chi so hang; tuong thich bao cao thieu cap cha.
                var collapsedCodes={};
                var taskRowsArray=Array.prototype.slice.call(taskRows.children);
                function hasSelectedChildren(code){return report.Tasks.some(function(t){return String(t.Code||'').indexOf(code+'.')===0;});}
                function drawVisibleDependencies(){
                    Array.prototype.slice.call(svg.querySelectorAll('.dep-line')).forEach(function(line){line.parentNode.removeChild(line);});
                    report.Tasks.forEach(function(task){
                        if(!task.DependencyId)return;
                        var from=taskTrackById[task.DependencyId],to=taskTrackById[task.Id];
                        if(!from||!to||from.style.display==='none'||to.style.display==='none')return;
                        var fromBar=barById[task.DependencyId],toBar=barById[task.Id];
                        if(!fromBar||!toBar)return;
                        var sx=fromBar.offsetLeft+fromBar.offsetWidth;
                        var sy=from.offsetTop+ROW_HEIGHT/2;
                        var ex=toBar.offsetLeft;
                        var ey=to.offsetTop+ROW_HEIGHT/2;
                        var mid=Math.max(sx+12,Math.min(ex-10,(sx+ex)/2));
                        var path=document.createElementNS(svgNS,'path');
                        path.setAttribute('class','dep-line');
                        path.setAttribute('d','M '+sx+' '+sy+' L '+mid+' '+sy+' L '+mid+' '+ey+' L '+(ex-4)+' '+ey);
                        path.setAttribute('marker-end','url(#dependencyArrow)');svg.appendChild(path);
                    });
                }
                function applyTreeVisibility(){
                    var visible=0;
                    report.Tasks.forEach(function(task,i){
                        var code=String(task.Code||'');
                        var hidden=Object.keys(collapsedCodes).some(function(parent){return !!collapsedCodes[parent]&&code.indexOf(parent+'.')===0;});
                        taskRowsArray[i].style.display=hidden?'none':'';
                        var track=taskTrackById[task.Id];if(track)track.style.display=hidden?'none':'';
                        if(!hidden)visible++;
                        var actionArea=taskRowsArray[i].querySelector('.report-tree-expandable');
                        if(actionArea)actionArea.setAttribute('aria-expanded',collapsedCodes[code]?'false':'true');
                    });
                    var height=visible*ROW_HEIGHT;
                    tracks.style.height=height+'px';svg.setAttribute('height',String(height));
                    svg.setAttribute('viewBox','0 0 '+chartWidth+' '+Math.max(1,height));
                    // Cap nhat sau khi CSS da sap xep lai vi tri cac hang.
                    window.requestAnimationFrame?window.requestAnimationFrame(drawVisibleDependencies):drawVisibleDependencies();
                }
                report.Tasks.forEach(function(task,index){
                    var code=String(task.Code||'');if(!code||!hasSelectedChildren(code))return;
                    var row=taskRowsArray[index];
                    var inside=row.querySelector('.task-level-1,.task-level-child');if(!inside)return;
                    // Chi tiết có nhánh con: tên, mã và mũi tên cùng là một vùng kích hoạt.
                    inside.classList.add('report-tree-expandable');
                    inside.setAttribute('role','button');
                    inside.setAttribute('tabindex','0');
                    inside.setAttribute('aria-expanded','true');
                    inside.setAttribute('aria-label','Thu gọn hoặc mở rộng công việc '+code+' '+String(task.Name||''));
                    inside.title='Bấm để thu gọn / mở rộng công việc con';
                    var arrow=document.createElement('span');
                    arrow.className='report-tree-toggle';
                    arrow.setAttribute('aria-hidden','true');
                    arrow.innerHTML='<span class=""report-tree-chevron""></span>';
                    // Task con: giữ nét nhánh cây phía trước, mũi tên nằm sát mã giống Gantt chính.
                    var branch=inside.querySelector('.branch');
                    if(branch)inside.insertBefore(arrow,branch.nextSibling);
                    else inside.insertBefore(arrow,inside.firstChild);
                    function toggleBranch(){collapsedCodes[code]=!collapsedCodes[code];applyTreeVisibility();}
                    inside.addEventListener('click',function(e){e.preventDefault();toggleBranch();});
                    inside.addEventListener('keydown',function(e){if(e.key==='Enter'||e.key===' '){e.preventDefault();toggleBranch();}});
                });
                applyTreeVisibility();
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
