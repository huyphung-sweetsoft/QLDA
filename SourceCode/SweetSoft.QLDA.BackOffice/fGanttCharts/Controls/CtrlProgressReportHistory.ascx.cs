using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
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
    public partial class CtrlProgressReportHistory : ProgressReportControlBase
    {
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
                // Chuẩn bị index tên của TOÀN BỘ nhật ký (không chứa OTP, link bí mật).
                // Search suggestions được lọc ngay trên client, không phát sinh postback khi gõ.
                entries.Sort((x, y) => y.CreatedUtc.CompareTo(x.CreatedUtc));
                HiddenField searchCatalog = FindHistoryControl<HiddenField>(this, "hdfHistorySearchCatalog");
                if (searchCatalog == null) throw new InvalidOperationException("Thiếu danh mục gợi ý tìm kiếm báo cáo.");
                var catalog = entries.Select(x => new
                {
                    name = !string.IsNullOrWhiteSpace(x.ReportName) ? x.ReportName :
                        (!string.IsNullOrWhiteSpace(x.ProjectName) ? x.ProjectName + " · Báo cáo " + x.CreatedUtc.ToString("ddMMyy", CultureInfo.InvariantCulture) : "Báo cáo " + x.ReportId.Substring(0, 8)),
                    date = (x.CreatedUtc.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(x.CreatedUtc, DateTimeKind.Utc) : x.CreatedUtc.ToUniversalTime())
                        .AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"))
                }).ToList();
                searchCatalog.Value = Serializer.Serialize(catalog);

                TextBox search = FindHistoryControl<TextBox>(this, "txtHistorySearch");
                string keyword = NormalizeHistorySearch(search == null ? null : search.Text);
                if (!string.IsNullOrEmpty(keyword))
                    entries = entries.Where(x => NormalizeHistorySearch(x.ReportName).Contains(keyword) ||
                        (string.IsNullOrWhiteSpace(x.ReportName) && NormalizeHistorySearch(x.ProjectName).Contains(keyword))).ToList();
                int pageCount = Math.Max(1, (entries.Count + HistoryPageSize - 1) / HistoryPageSize);
                HistoryPage = Math.Min(HistoryPage, pageCount - 1);
                StringBuilder html = new StringBuilder("<div class='share-progress-history-list'>");
                if (entries.Count == 0)
                    html.Append("<div class='share-progress-history-empty'><i class='far fa-folder-open'></i><div>" +
                        (string.IsNullOrEmpty(keyword) ? "Chưa có báo cáo trong nhật ký dự án." : "Không tìm thấy báo cáo phù hợp với từ khóa.") + "</div></div>");
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


        public void OpenModal(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn dự án trước khi xem nhật ký báo cáo.", MSGType.Warning);
                return;
            }
            ProjectId = projectId;
            HistoryPage = 0;
            TextBox search = FindHistoryControl<TextBox>(this, "txtHistorySearch");
            if (search != null) search.Text = string.Empty;
            OpenProgressHistory(true);
        }

        protected void btnHistorySearch_Click(object sender, EventArgs e)
        {
            HistoryPage = 0;
            OpenProgressHistory(false);
        }

        private static string NormalizeHistorySearch(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder(normalized.Length);
            foreach (char ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    result.Append(ch == 'đ' ? 'd' : ch);
            }
            return result.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
