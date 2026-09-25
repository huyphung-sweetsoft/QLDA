using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class SigningInbox : BaseAdminPage
    {
        private const string ResultSavedCallbackKey = "SIGNING_INBOX_RESULT_SAVED";

        public sealed class AssignedDocumentGroup
        {
            public string TenTaiLieu { get; set; }
            public string MaTaiLieu { get; set; }
            public int FileCount { get; set; }
            public int PendingCount { get; set; }
            public int SignedCount { get; set; }
            public int ChangesCount { get; set; }
            public List<DataRowView> Files { get; set; }
        }

        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.DocumentSigningInbox; }
        }

        private DocumentManager RequestManager
        {
            get { return new DocumentManager(SweetContext.Current); }
        }

        private Guid? RequestedSigningId
        {
            get
            {
                string token = CommonHelpers.QueryString("Id");
                if (string.IsNullOrWhiteSpace(token))
                    return null;
                Guid id;
                try
                {
                    if (Guid.TryParse(
                        SecurityUtilities.UnprotectUrlParameter(token), out id)
                        && id != Guid.Empty)
                        return id;
                }
                catch { }
                throw new HttpException(400, "Đường dẫn trình ký không hợp lệ.");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            if (!IsPostBack)
                SetMetaTagsOgTags("File được giao ký");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ddlSigningStatus.ClearItems();
            ddlSigningStatus.AddItem("Tất cả trạng thái", "ALL");
            ddlSigningStatus.AddItem("Chờ ký", "PENDING");
            ddlSigningStatus.AddItem("Đã ký", "SIGNED");
            ddlSigningStatus.AddItem("Yêu cầu chỉnh sửa", "CHANGES");
            txtSigningSearch.EnterSubmitClientID = btnApplyFilters.ClientID;
            // Recreate the same command controls before loading their state
            // and dispatching the postback event.
            Page.InitComplete += Page_InitComplete;
        }

        private void Page_InitComplete(object sender, EventArgs e)
        {
            // Recreate the currently displayed slice before WebForms dispatches
            // row/pager events. The hidden fields preserve applied filters and
            // page state for this early lifecycle point.
            BindAssignedFiles(true);
        }

        private void BindAssignedFiles()
        {
            BindAssignedFiles(false);
        }

        private void BindAssignedFiles(bool readPostedSnapshot)
        {
            DataTable files = RequestManager.GetAssignedSigningFiles(
                RequestedSigningId);
            List<DataRowView> rows = files.DefaultView.Cast<DataRowView>()
                .ToList();
            lblPendingCount.Text = rows.Count(row => IsPending(
                row["TrangThai"])).ToString();
            lblSignedCount.Text = rows.Count(row => IsSigned(
                row["TrangThai"])).ToString();
            lblChangesCount.Text = rows.Count(row => IsChangesRequested(
                row["TrangThai"])).ToString();

            string keyword = ReadAppliedValue(
                hdfAppliedKeyword, string.Empty, readPostedSnapshot).Trim();
            string statusFilter = NormalizeStatusFilter(ReadAppliedValue(
                hdfAppliedStatus, "ALL", readPostedSnapshot));
            int pageSize = NormalizePageSize(ReadAppliedValue(
                hdfPageSize, DefaultPageSize().ToString(), readPostedSnapshot));
            int pageIndex = NormalizePageIndex(ReadAppliedValue(
                hdfPageIndex, "1", readPostedSnapshot));

            List<AssignedDocumentGroup> documents = rows
                .GroupBy(row => Convert.ToString(row["IdTaiLieu"]))
                .Select(group =>
                {
                    DataRowView first = group.First();
                    List<DataRowView> matchingFiles = group
                        .Where(row => MatchesStatusFilter(
                            row["TrangThai"], statusFilter))
                        .ToList();

                    bool documentMatches = string.IsNullOrEmpty(keyword)
                        || ContainsText(first["TenTaiLieu"], keyword)
                        || ContainsText(first["MaTaiLieu"], keyword);
                    if (!string.IsNullOrEmpty(keyword) && !documentMatches)
                    {
                        matchingFiles = matchingFiles.Where(row =>
                            ContainsText(row["TenFileNguonGoc"], keyword)
                            || ContainsText(row["TenFileNguon"], keyword)
                            || ContainsText(row["GhiChuYeuCau"], keyword)
                            || ContainsText(row["GhiChu"], keyword))
                            .ToList();
                    }

                    if (matchingFiles.Count == 0)
                        return null;

                    return new AssignedDocumentGroup
                    {
                        TenTaiLieu = Convert.ToString(first["TenTaiLieu"]),
                        MaTaiLieu = Convert.ToString(first["MaTaiLieu"]),
                        FileCount = matchingFiles.Count,
                        PendingCount = matchingFiles.Count(row => IsPending(
                            row["TrangThai"])),
                        SignedCount = matchingFiles.Count(row => IsSigned(
                            row["TrangThai"])),
                        ChangesCount = matchingFiles.Count(row => IsChangesRequested(
                            row["TrangThai"])),
                        Files = matchingFiles
                        .OrderBy(row => IsPending(row["TrangThai"]) ? 0 : 1)
                        .ThenByDescending(row => row["NgayGui"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(row["NgayGui"]))
                        .ToList()
                    };
                })
                .Where(document => document != null)
                .OrderByDescending(group => group.Files.Max(row =>
                    row["NgayGui"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(row["NgayGui"])))
                .ToList();

            pnlEmpty.Visible = rows.Count == 0;
            pnlNoMatches.Visible = rows.Count > 0 && documents.Count == 0;
            pnlFilters.Visible = true;
            rptAssignedDocuments.Visible = documents.Count > 0;

            int pageCount = documents.Count == 0
                ? 0 : (int)Math.Ceiling(documents.Count / (double)pageSize);
            if (pageCount > 0 && pageIndex > pageCount)
                pageIndex = pageCount;
            if (pageCount == 0)
                pageIndex = 1;

            int skip = (pageIndex - 1) * pageSize;
            List<AssignedDocumentGroup> pageDocuments = documents
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Keep the current slice stable across WebForms postbacks. For the
            // InitComplete bind, readPostedSnapshot uses the values from the
            // previous response; handlers then bind from these fields directly.
            hdfAppliedKeyword.Value = keyword;
            hdfAppliedStatus.Value = statusFilter;
            hdfPageIndex.Value = pageIndex.ToString();
            hdfPageSize.Value = pageSize.ToString();
            ddlSigningStatus.SelectedValue = statusFilter;

            pnlPagination.Visible = documents.Count > 0;
            ctrlGridviewPaging.Visible = documents.Count > 0;
            ctrlGridviewPaging.GridviewID = "AssignedSigningFiles";
            ctrlGridviewPaging.PageIndex = pageIndex;
            ctrlGridviewPaging.PageSize = pageSize;
            ctrlGridviewPaging.TotalItems = documents.Count;
            ctrlGridviewPaging.InitLoad();

            rptAssignedDocuments.DataSource = pageDocuments;
            rptAssignedDocuments.DataBind();
        }

        private string ReadAppliedValue(
            HiddenField field, string fallback, bool readPostedSnapshot)
        {
            string value = null;
            if (readPostedSnapshot && IsPostBack)
                value = Request.Form[field.UniqueID];
            else
                value = field.Value;

            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private int DefaultPageSize()
        {
            int pageSize;
            if (!int.TryParse(SweetContext.Current.CurrentPageSize, out pageSize))
                pageSize = 20;
            return NormalizePageSize(pageSize.ToString());
        }

        private static int NormalizePageSize(string value)
        {
            int pageSize;
            if (!int.TryParse(value, out pageSize)
                || !new[] { 10, 20, 30, 50, 100 }.Contains(pageSize))
            {
                return 20;
            }
            return pageSize;
        }

        private static int NormalizePageIndex(string value)
        {
            int pageIndex;
            return int.TryParse(value, out pageIndex) && pageIndex > 0
                ? pageIndex : 1;
        }

        private static string NormalizeStatusFilter(string value)
        {
            if (string.Equals(value, "PENDING", StringComparison.OrdinalIgnoreCase))
                return "PENDING";
            if (string.Equals(value, "SIGNED", StringComparison.OrdinalIgnoreCase))
                return "SIGNED";
            if (string.Equals(value, "CHANGES", StringComparison.OrdinalIgnoreCase))
                return "CHANGES";
            return "ALL";
        }

        private static bool MatchesStatusFilter(object status, string filter)
        {
            switch (filter)
            {
                case "PENDING": return IsPending(status);
                case "SIGNED": return IsSigned(status);
                case "CHANGES": return IsChangesRequested(status);
                default: return true;
            }
        }

        private static bool ContainsText(object value, string keyword)
        {
            return !string.IsNullOrEmpty(keyword)
                && value != null
                && value != DBNull.Value
                && Convert.ToString(value).IndexOf(
                    keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            hdfAppliedKeyword.Value = txtSigningSearch.Text.Trim();
            hdfAppliedStatus.Value = NormalizeStatusFilter(ddlSigningStatus.SelectedValue);
            hdfPageIndex.Value = "1";
            BindAssignedFiles();
        }

        protected void btnResetFilters_Click(object sender, EventArgs e)
        {
            txtSigningSearch.Text = string.Empty;
            ddlSigningStatus.SelectedValue = "ALL";
            hdfAppliedKeyword.Value = string.Empty;
            hdfAppliedStatus.Value = "ALL";
            hdfPageIndex.Value = "1";
            BindAssignedFiles();
        }

        protected void ddlSigningStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            hdfAppliedStatus.Value = NormalizeStatusFilter(
                ddlSigningStatus.SelectedValue);
            hdfPageIndex.Value = "1";
            BindAssignedFiles();
        }

        protected void ctrlGridviewPaging_PageChanged(
            object sender, GridviewCustomPageChangeArgs e)
        {
            hdfPageIndex.Value = NormalizePageIndex(
                e.CurrentPageNumber.ToString()).ToString();
            hdfPageSize.Value = NormalizePageSize(
                e.CurrentPageSize.ToString()).ToString();
            BindAssignedFiles();
        }

        protected void rptAssignedDocuments_ItemDataBound(
            object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item
                && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            AssignedDocumentGroup document =
                e.Item.DataItem as AssignedDocumentGroup;
            Repeater fileRepeater = e.Item.FindControl(
                "rptAssignedFiles") as Repeater;
            if (document == null || fileRepeater == null)
                return;

            fileRepeater.DataSource = document.Files;
            fileRepeater.DataBind();
        }

        private void RefreshAssignedFiles()
        {
            BindAssignedFiles();
            upAssignedFiles.Update();
        }

        private DataRow GetAssignedPendingFile(Guid signingFileId)
        {
            return RequestManager.GetAssignedSigningFiles(RequestedSigningId)
                .AsEnumerable()
                .FirstOrDefault(row =>
                    row.Field<Guid>("IdTrinhKyTaiLieuFile") == signingFileId
                    && IsPending(row["TrangThai"]));
        }

        protected void rptAssignedFiles_ItemCommand(
            object source, RepeaterCommandEventArgs e)
        {
            Guid signingFileId;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument),
                    out signingFileId))
            {
                ShowNotify("File trình ký không hợp lệ.", MSGType.Warning);
                return;
            }

            DataRow row = GetAssignedPendingFile(signingFileId);
            if (row == null)
            {
                ShowNotify("File này không còn chờ bạn xử lý.", MSGType.Warning);
                RefreshAssignedFiles();
                return;
            }

            Guid documentId = row.Field<Guid>("IdTaiLieu");
            if (e.CommandName == "UPLOAD_RESULT")
            {
                hdfResultDocumentId.Value = documentId.ToString();
                hdfResultFileId.Value = signingFileId.ToString();
                txtResultNote.Text = string.Empty;
                fbSigningResult.IsEnabled = true;
                fbSigningResult.IsMultiple = false;
                fbSigningResult.AcceptType =
                    "application/pdf,image/jpeg,image/jpg,image/png";
                fbSigningResult.SaveDataCallbackKey = ResultSavedCallbackKey;
                fbSigningResult.LoadFile(signingFileId,
                    FileUploadTypes.DocumentSigningResult);
                mdlSigningResult.Title = "Tải bản đã ký lên";
                OpenSigningModal(mdlSigningResult, "OpenSigningResult");
            }
            else if (e.CommandName == "REQUEST_CHANGES")
            {
                hdfChangesDocumentId.Value = documentId.ToString();
                hdfChangesFileId.Value = signingFileId.ToString();
                txtChangesReason.Text = string.Empty;
                mdlSigningChanges.Title = "Yêu cầu chỉnh sửa file";
                OpenSigningModal(mdlSigningChanges, "OpenSigningChanges");
            }
        }

        private void OpenSigningModal(ExtraModal modal, string scriptKey)
        {
            modal.UpdateContentModal();
            string title = HttpUtility.JavaScriptStringEncode(
                modal.Title ?? string.Empty);
            // Bootstrap installs $.fn.modal at DOMContentLoaded. A startup
            // script can run earlier on a full response, so use its native API
            // after the document is ready. This also runs after partial updates.
            string script = "(function(){function openSigningModal(){"
                + "var modal=document.getElementById('" + modal.ClientID + "');"
                + "if(!modal)return;"
                + "var title=modal.querySelector('.modal-title');"
                + "if(title)title.textContent='" + title + "';"
                + "bootstrap.Modal.getOrCreateInstance(modal).show();}"
                + "if(document.readyState==='loading'){"
                + "document.addEventListener('DOMContentLoaded',openSigningModal,{once:true});"
                + "}else{openSigningModal();}})();";
            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                scriptKey + modal.ClientID,
                script,
                true);
        }

        private void CloseSigningModal(ExtraModal modal, string scriptKey)
        {
            modal.UpdateContentModal();
            string script = "(function(){var modal=document.getElementById('"
                + modal.ClientID + "');"
                + "if(modal){var instance=bootstrap.Modal.getInstance(modal);"
                + "if(instance)instance.hide();}})();";
            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                scriptKey + modal.ClientID,
                script,
                true);
        }

        public override void DataCallback(
            string key, object value, object valueText)
        {
            if (key != ResultSavedCallbackKey)
                return;
            Guid documentId, signingFileId;
            if (!Guid.TryParse(hdfResultDocumentId.Value, out documentId)
                || !Guid.TryParse(hdfResultFileId.Value, out signingFileId)
                || !RequestManager.CanProcessAssignedSigningFile(
                    documentId, signingFileId))
                throw new UnauthorizedAccessException(
                    "Bạn không được giao xử lý file trình ký này.");

            fbSigningResult.LoadFile(signingFileId,
                FileUploadTypes.DocumentSigningResult);
            OpenSigningModal(mdlSigningResult, "ReopenSigningResultAfterUpload");
        }

        protected void btnConfirmSigned_Click(object sender, EventArgs e)
        {
            Guid documentId, signingFileId;
            if (!Guid.TryParse(hdfResultDocumentId.Value, out documentId)
                || !Guid.TryParse(hdfResultFileId.Value, out signingFileId))
            {
                ShowNotify("File trình ký không hợp lệ.", MSGType.Warning);
                return;
            }

            try
            {
                RequestManager.CompleteDocumentSigningFile(
                    documentId, signingFileId, txtResultNote.Text);
                CloseSigningModal(mdlSigningResult, "CloseSigningResult");
                RefreshAssignedFiles();
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
                OpenSigningModal(mdlSigningResult, "RetrySigningResult");
            }
            catch (UnauthorizedAccessException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnSendChanges_Click(object sender, EventArgs e)
        {
            Guid documentId, signingFileId;
            if (!Guid.TryParse(hdfChangesDocumentId.Value, out documentId)
                || !Guid.TryParse(hdfChangesFileId.Value, out signingFileId))
            {
                ShowNotify("File trình ký không hợp lệ.", MSGType.Warning);
                return;
            }

            try
            {
                RequestManager.RequestDocumentSigningFileChanges(
                    documentId, signingFileId, txtChangesReason.Text);
                CloseSigningModal(mdlSigningChanges, "CloseSigningChanges");
                RefreshAssignedFiles();
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
                OpenSigningModal(mdlSigningChanges, "RetrySigningChanges");
            }
            catch (UnauthorizedAccessException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelResult_Click(object sender, EventArgs e)
        {
            CloseSigningModal(mdlSigningResult, "CancelSigningResult");
        }

        protected void btnCancelChanges_Click(object sender, EventArgs e)
        {
            CloseSigningModal(mdlSigningChanges, "CancelSigningChanges");
        }

        protected string FileUrl(object value)
        {
            return FileHelpers.IsValidPath(Convert.ToString(value));
        }

        protected static string FileName(object original, object name)
        {
            string text = Convert.ToString(original);
            return string.IsNullOrWhiteSpace(text)
                ? Convert.ToString(name) : text;
        }

        protected static bool HasText(object value)
        {
            return !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        protected static bool IsPending(object value)
        {
            return string.Equals(Convert.ToString(value),
                DocumentSigningStatusKeys.Pending,
                StringComparison.OrdinalIgnoreCase);
        }

        protected static bool IsSigned(object value)
        {
            return string.Equals(Convert.ToString(value),
                DocumentSigningStatusKeys.Signed,
                StringComparison.OrdinalIgnoreCase);
        }

        protected static bool IsChangesRequested(object value)
        {
            return string.Equals(Convert.ToString(value),
                DocumentSigningStatusKeys.ChangesRequested,
                StringComparison.OrdinalIgnoreCase);
        }

        protected static string StatusText(object value)
        {
            string status = Convert.ToString(value);
            if (IsPending(status)) return "Chờ ký";
            if (status == DocumentSigningStatusKeys.Signed) return "Đã ký";
            if (status == DocumentSigningStatusKeys.ChangesRequested)
                return "Yêu cầu chỉnh sửa";
            return status;
        }

        protected static string StatusCss(object value)
        {
            string status = Convert.ToString(value);
            if (IsPending(status)) return "badge bg-info";
            if (status == DocumentSigningStatusKeys.Signed)
                return "badge bg-success";
            if (status == DocumentSigningStatusKeys.ChangesRequested)
                return "badge bg-warning text-dark";
            return "badge bg-secondary";
        }

        protected string FormatDate(object value)
        {
            return value == DBNull.Value || value == null
                ? "—" : ConvertDateTimeToString(value);
        }
    }
}
