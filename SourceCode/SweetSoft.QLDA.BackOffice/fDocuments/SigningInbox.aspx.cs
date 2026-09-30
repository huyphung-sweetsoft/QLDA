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
        private Guid? DetailDocumentId
        {
            get
            {
                string value = Request.QueryString["document"];
                if (string.IsNullOrEmpty(value)) return null;
                Guid id;
                if (Guid.TryParse(value, out id) && id != Guid.Empty) return id;
                throw new HttpException(400, "Đường dẫn hồ sơ trình ký không hợp lệ.");
            }
        }

        protected string DetailUrl(object id)
        {
            return "SigningInbox.aspx?document=" + HttpUtility.UrlEncode(Convert.ToString(id))
                + "&q=" + HttpUtility.UrlEncode(hdfAppliedKeyword.Value)
                + "&status=" + HttpUtility.UrlEncode(hdfAppliedStatus.Value)
                + "&project=" + HttpUtility.UrlEncode(hdfAppliedProject.Value)
                + "&page=" + HttpUtility.UrlEncode(hdfPageIndex.Value)
                + "&size=" + HttpUtility.UrlEncode(hdfPageSize.Value);
        }

        public sealed class AssignedDocumentGroup
        {
            public Guid Id { get; set; }
            public string ProjectName { get; set; }
            public string Sender { get; set; }
            public string Note { get; set; }
            public DateTime LastSent { get; set; }
            public int PendingBatches { get; set; }
            public bool Expanded { get; set; }
            public string TenTaiLieu { get; set; }
            public string MaTaiLieu { get; set; }
            public int FileCount { get; set; }
            public int TotalFileCount { get; set; }
            public int PendingCount { get; set; }
            public int SignedCount { get; set; }
            public int ChangesCount { get; set; }
            public int RecalledCount { get; set; }
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
            {
                SetMetaTagsOgTags("Hồ sơ trình ký của tôi");
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { "javascript:;", "Hồ sơ trình ký" }
                };
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ddlSigningStatus.ClearItems();
            ddlSigningStatus.AddItem("Tất cả trạng thái", "ALL");
            ddlSigningStatus.AddItem("Chờ ký", "PENDING");
            ddlSigningStatus.AddItem("Đã ký", "SIGNED");
            ddlSigningStatus.AddItem("Yêu cầu chỉnh sửa", "CHANGES");
            ddlSigningStatus.AddItem("Đã thu hồi", "RECALLED");
            txtSigningSearch.EnterSubmitClientID = btnApplyFilters.ClientID;
            // Recreate the same command controls before loading their state
            // and dispatching the postback event.
            Page.InitComplete += Page_InitComplete;
            Page.PreRenderComplete += Page_PreRenderComplete;
        }

        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // The shared pager defaults its selector to the user's saved size.
            // This page also restores sizes from the list/detail navigation URL.
            DropDownList sizeSelector = ctrlGridviewPaging.FindControl("ddlPageSize") as DropDownList;
            if (sizeSelector != null)
                sizeSelector.SelectedValue = NormalizePageSize(hdfPageSize.Value).ToString();
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
            DataTable files = RequestManager.GetAssignedSigningFiles(null);
            List<DataRowView> rows = files.DefaultView.Cast<DataRowView>()
                .ToList();
            Guid? detailId = DetailDocumentId;
            if (RequestedSigningId.HasValue)
            {
                var assigned = rows.FirstOrDefault(r => (Guid)r["IdTrinhKyTaiLieu"] == RequestedSigningId.Value);
                if (assigned == null) throw new HttpException(403,"Bạn không được giao đợt trình ký này.");
                detailId = (Guid)assigned["IdTaiLieu"];
            }
            bool detail = detailId.HasValue;
            if (detail)
            {
                rows = rows.Where(r => (Guid)r["IdTaiLieu"] == detailId.Value).ToList();
                if(rows.Count==0) throw new HttpException(403,"Bạn không được giao xử lý hồ sơ này.");
                lblDetailTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(rows[0]["TenTaiLieu"]));
                lblDetailScope.Text = HttpUtility.HtmlEncode(Convert.ToString(rows[0]["MaTaiLieu"]) + " · " + ProjectName(rows[0]));
                lnkOriginalDocument.Visible = RequestManager.CanAccessDocument(detailId.Value, ActionKeys.View);
                if (lnkOriginalDocument.Visible)
                    lnkOriginalDocument.NavigateUrl = rows[0]["IdDuAn"] == DBNull.Value
                        ? RewriteURLHelper.DocumentDetail(detailId.Value)
                        : RewriteURLHelper.ProjectDocumentDetail((Guid)rows[0]["IdDuAn"], detailId.Value);
            }
            pnlDocumentList.Visible = !detail;
            pnlDetail.Visible = detail;
            pnlDetailHeader.Visible = detail;
            ddlProject.Visible = !detail;
            lnkBackToInbox.NavigateUrl = "SigningInbox.aspx?q=" + HttpUtility.UrlEncode(Request.QueryString["q"])
                + "&status=" + HttpUtility.UrlEncode(QueryValueOrDefault("status", "PENDING"))
                + "&project=" + HttpUtility.UrlEncode(QueryValueOrDefault("project", "ALL"))
                + "&page=" + HttpUtility.UrlEncode(QueryValueOrDefault("page", "1"))
                + "&size=" + HttpUtility.UrlEncode(QueryValueOrDefault("size", DefaultPageSize().ToString()));
            ddlProject.ClearItems();
            ddlProject.AddItem("Tất cả dự án", "ALL");
            foreach(var group in rows.GroupBy(ProjectKey).OrderBy(g=>ProjectName(g.First())))
                ddlProject.AddItem(ProjectName(group.First()),group.Key);

            string keyword = ReadAppliedValue(
                hdfAppliedKeyword, detail ? "" : Request.QueryString["q"] ?? "", readPostedSnapshot).Trim();
            string statusFilter = NormalizeStatusFilter(ReadAppliedValue(
                hdfAppliedStatus, detail ? "ALL" : QueryValueOrDefault("status", "PENDING"), readPostedSnapshot));
            string projectFilter = ReadAppliedValue(hdfAppliedProject, detail ? "ALL" : QueryValueOrDefault("project", "ALL"),readPostedSnapshot);
            if (string.IsNullOrWhiteSpace(projectFilter)) projectFilter = "ALL";
            int assignedCount = rows.Count;
            if(!detail && projectFilter!="ALL") rows=rows.Where(r=>ProjectKey(r)==projectFilter).ToList();
            int pageSize = NormalizePageSize(ReadAppliedValue(
                hdfPageSize, QueryValueOrDefault("size", DefaultPageSize().ToString()), readPostedSnapshot));
            int pageIndex = NormalizePageIndex(ReadAppliedValue(
                hdfPageIndex, detail ? "1" : QueryValueOrDefault("page", "1"), readPostedSnapshot));

            List<AssignedDocumentGroup> documents = rows
                .GroupBy(row => Convert.ToString(row[detail ? "IdTrinhKyTaiLieu" : "IdTaiLieu"]))
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
                        Id = (Guid)first[detail ? "IdTrinhKyTaiLieu" : "IdTaiLieu"],
                        ProjectName = ProjectName(first),
                        Sender = Convert.ToString(first["TenNguoiGui"]),
                        Note = Convert.ToString(first["GhiChuYeuCau"]),
                        LastSent = group.Max(r=>r["NgayGui"]==DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["NgayGui"])),
                        PendingBatches = matchingFiles.Where(r=>IsPending(r["TrangThai"])).Select(r=>r["IdTrinhKyTaiLieu"]).Distinct().Count(),
                        Expanded = matchingFiles.Any(r => IsPending(r["TrangThai"]))
                            || statusFilter != "ALL" || !string.IsNullOrEmpty(keyword)
                            || (RequestedSigningId.HasValue && (Guid)first["IdTrinhKyTaiLieu"] == RequestedSigningId.Value),
                        TenTaiLieu = Convert.ToString(first["TenTaiLieu"]),
                        MaTaiLieu = Convert.ToString(first["MaTaiLieu"]),
                        FileCount = matchingFiles.Count,
                        TotalFileCount = group.Count(),
                        PendingCount = matchingFiles.Count(row => IsPending(
                            row["TrangThai"])),
                        SignedCount = matchingFiles.Count(row => IsSigned(
                            row["TrangThai"])),
                        ChangesCount = matchingFiles.Count(row => IsChangesRequested(
                            row["TrangThai"])),
                        RecalledCount = matchingFiles.Count(row => string.Equals(
                            Convert.ToString(row["TrangThai"]), "THU_HOI", StringComparison.OrdinalIgnoreCase)),
                        Files = matchingFiles
                        .OrderBy(row => IsPending(row["TrangThai"]) ? 0 : 1)
                        .ThenByDescending(row => row["NgayGui"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(row["NgayGui"]))
                        .ToList()
                    };
                })
                .Where(document => document != null)
                .OrderBy(group => RequestedSigningId.HasValue && group.Id == RequestedSigningId.Value ? 0 : 1)
                .ThenBy(group => group.PendingCount > 0 ? 0 : 1)
                .ThenByDescending(group => group.LastSent)
                .ThenBy(group => group.Id)
                .ToList();

            pnlEmpty.Visible = assignedCount == 0;
            pnlNoMatches.Visible = assignedCount > 0 && documents.Count == 0;
            pnlDocumentList.Visible = !detail && documents.Count > 0;
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
            hdfAppliedProject.Value = projectFilter;
            ddlProject.SelectedValue = projectFilter;
            if(!IsPostBack) txtSigningSearch.Text=keyword;
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

            rptAssignedDocuments.DataSource = detail ? pageDocuments : null;
            rptAssignedDocuments.DataBind();
            grvDocumentList.CurrentPageIndex = pageIndex;
            grvDocumentList.CurrentPageSize = pageSize;
            grvDocumentList.DataSource = detail ? null : pageDocuments;
            grvDocumentList.DataBind();
        }

        private static string ProjectKey(DataRowView row) { return row["IdDuAn"]==DBNull.Value ? "COMMON" : Convert.ToString(row["IdDuAn"]); }
        private static string ProjectName(DataRowView row) { return row["IdDuAn"]==DBNull.Value ? "Hồ sơ chung" : Convert.ToString(row["TenDuAn"]); }
        protected void ddlProject_SelectedValueChanged(object sender,EventArgs e)
        { hdfAppliedProject.Value=ddlProject.SelectedValue; hdfPageIndex.Value="1"; BindAssignedFiles(); }

        private string ReadAppliedValue(
            HiddenField field, string fallback, bool readPostedSnapshot)
        {
            if (IsPostBack)
                return (readPostedSnapshot ? Request.Form[field.UniqueID] : field.Value) ?? fallback;
            return string.IsNullOrEmpty(field.Value) ? fallback : field.Value;
        }

        private string QueryValueOrDefault(string name, string fallback)
        {
            string value = Request.QueryString[name];
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
                || !new[] { 10, 20, 30, 50, 100, 200, 300, 500 }.Contains(pageSize))
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
            if (string.Equals(value, "RECALLED", StringComparison.OrdinalIgnoreCase)) return "RECALLED";
            return "ALL";
        }

        private static bool MatchesStatusFilter(object status, string filter)
        {
            switch (filter)
            {
                case "PENDING": return IsPending(status);
                case "SIGNED": return IsSigned(status);
                case "CHANGES": return IsChangesRequested(status);
                case "RECALLED": return Convert.ToString(status)=="THU_HOI";
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
            hdfAppliedProject.Value = "ALL";
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
            // The shared pager owns a nested conditional UpdatePanel. Refresh
            // the parent too so rows and hidden paging state match the pager.
            RefreshAssignedFiles();
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
            // A notification opens the dossier's batches, not only the batch
            // in its URL. Authorization still comes from the assigned-user query.
            return RequestManager.GetAssignedSigningFiles(null)
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
                lnkResultSource.Text = HttpUtility.HtmlEncode(FileName(row["TenFileNguonGoc"], row["TenFileNguon"]));
                lnkResultSource.NavigateUrl = FileUrl(row["FileNguonUrl"]);
                lnkResultSource.Attributes["data-path"] = lnkResultSource.NavigateUrl;
                fbSigningResult.IsEnabled = true;
                fbSigningResult.IsMultiple = false;
                fbSigningResult.AcceptType =
                    "application/pdf,image/jpeg,image/jpg,image/png,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";
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
            if (status == "THU_HOI") return "Đã thu hồi";
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
