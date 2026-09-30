using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fFilesBox;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.SysManager;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentDetail
    {
        protected UpdatePanel upActivity;
        protected Button btnEditDocumentInfo;
        protected CtrlDocuments documentInfoEditor;

        protected void btnEditDocumentInfo_Click(object sender, EventArgs e)
        {
            Guid documentId;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out documentId)) return;
            documentInfoEditor.ProjectId = ProjectId;
            documentInfoEditor.OpenDocumentEditor(documentId);
        }
        protected CheckBoxList cblCustomerDeliveryFiles;
        protected void btnMoreActivity_Click(object sender, EventArgs e) { LoadActivityPage(false); }
        protected void btnRefreshActivity_Click(object sender, EventArgs e) { LoadActivityPage(true); }

        private void LoadActivityPage(bool reset, bool userAction = true)
        {
            if (userAction) KeepActivityHistoryOpen();
            try
            {
                DataTable loaded = reset ? null : ViewState["LoadedActivity"] as DataTable;
                DateTime until = reset || ViewState["ActivityUntil"] == null
                    ? DateTime.UtcNow : (DateTime)ViewState["ActivityUntil"];
                var page = DocumentManager.Instance.GetDocumentActivityHistory(
                    WorkspaceDocumentId, loaded == null ? 0 : loaded.Rows.Count, 21, until);
                bool more = page.Rows.Count > 20;
                if (more) page.Rows.RemoveAt(20);
                if (loaded == null) loaded = page;
                else foreach (DataRow row in page.Rows) loaded.ImportRow(row);
                ViewState["LoadedActivity"] = loaded;
                ViewState["ActivityUntil"] = until;
                ViewState["ActivityHasMore"] = more;
                pnlActivityContent.Visible = true;
                BindLoadedActivity();
                if (userAction) upActivity.Update();
            }
            catch (Exception)
            {
                ShowNotify("Không tải được nhật ký. Vui lòng thử lại.", MSGType.Warning);
            }
        }

        private void KeepActivityHistoryOpen()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "KeepActivityHistoryOpen",
                "(function(){ var section=document.getElementById('workspaceHistories'); " +
                "if(section) section.classList.add('show'); " +
                "var y=window.documentActivityScrollY; delete window.documentActivityScrollY; " +
                "if(typeof y==='number') window.requestAnimationFrame(function(){window.scrollTo(0,y);}); })();", true);
        }

        private void BindLoadedActivity()
        {
            var loaded = ViewState["LoadedActivity"] as DataTable;
            rptActivity.DataSource = loaded;
            rptActivity.DataBind();
            pnlActivity.Visible = loaded != null && loaded.Rows.Count > 0;
            pnlNoActivity.Visible = loaded != null && loaded.Rows.Count == 0;
            btnMoreActivity.Visible = loaded != null && (bool?)ViewState["ActivityHasMore"] == true;
        }

        protected string GetWorkspaceDownloadUrl(object value)
        {
            string url = GetFileUrl(value);
            return string.IsNullOrWhiteSpace(url) ? url : url + (url.Contains("?") ? "&" : "?") + "download=1";
        }
        private Guid WorkspaceDocumentId { get { return Guid.Parse(hdfIdTaiLieu.Value); } }
        private Guid WorkspaceFileId { get { return (Guid)(ViewState["WorkspaceFileId"]??Guid.Empty); } set { ViewState["WorkspaceFileId"]=value; } }
        private Guid WorkspaceRootId { get { return (Guid)(ViewState["WorkspaceRootId"]??Guid.Empty); } set { ViewState["WorkspaceRootId"]=value; } }
        private bool WorkspaceLocked { get { return (bool)(ViewState["WorkspaceLocked"]??false); } set { ViewState["WorkspaceLocked"]=value; } }

        private void RegisterWorkspaceControls()
        {
            var script=ScriptManager.GetCurrent(Page);
            if(script==null)return;
            foreach(Control control in new Control[]{grdWorkspace,grdFileTimeline,btnWorkspaceAdd,btnWorkspaceSign,btnWorkspaceSend,
                btnWorkspaceFilter,btnWorkspaceReplace,btnWorkspaceRemove,ddlWorkspaceStatus,btnWorkspaceConfirm,btnWorkspaceCancelConfirm,
                btnMoreActivity,btnRefreshActivity,btnEditDocumentInfo})
                script.RegisterAsyncPostBackControl(control);
            // FileUpload needs multipart full postback; the other commands do not.
            script.RegisterPostBackControl(btnWorkspaceUpload);
        }

        private void CloseWorkspaceModal(ExtraModal modal)
        {
            modal.UpdateContentModal();
            string script = "(function(){function closeDossierModal(){var el=document.getElementById('"
                + modal.ClientID + "');if(el){var modal=bootstrap.Modal.getInstance(el);if(modal)modal.hide();}}"
                + "if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',closeDossierModal,{once:true});}else{closeDossierModal();}})();";
            ScriptManager.RegisterStartupScript(Page,GetType(),"Close"+modal.ClientID,script,true);
        }

        private void BindWorkspace()
        {
            var manager=DocumentManager.Instance;
            var table=manager.GetWorkspaceFiles(WorkspaceDocumentId);
            var rows=table.AsEnumerable();
            string search=(txtWorkspaceSearch.Text??"").Trim();
            if(search.Length>0)rows=rows.Where(r=>Convert.ToString(r["TenFile"]).IndexOf(search,StringComparison.CurrentCultureIgnoreCase)>=0);
            string status = ddlWorkspaceStatus.SelectedValue;
            if(!string.IsNullOrEmpty(status) && status != "ALL")rows=rows.Where(r=>Convert.ToString(r["TrangThai"])==status);
            var filtered=table.Clone();
            foreach(var row in rows)filtered.ImportRow(row);
            if(grdWorkspace.PageIndex*grdWorkspace.PageSize>=filtered.Rows.Count)grdWorkspace.PageIndex=0;
            grdWorkspace.DataSource=filtered;
            grdWorkspace.DataBind();
            lblWorkspaceCount.Text=filtered.Rows.Count+" file · Trang "+(grdWorkspace.PageIndex+1);
            btnWorkspaceAdd.Visible=manager.CanAccessDocument(WorkspaceDocumentId,DocumentPermissionKeys.ManageFiles);
            btnWorkspaceSign.Visible=manager.CanAccessDocument(WorkspaceDocumentId,DocumentPermissionKeys.Signing);
            btnWorkspaceSend.Visible=manager.CanAccessDocument(WorkspaceDocumentId,DocumentPermissionKeys.CustomerDelivery);
            upDetail.Update();
        }

        private List<Guid> SelectedWorkspaceFiles()
        {
            var ids=new List<Guid>();
            foreach(GridViewRow row in grdWorkspace.Rows)
                if(((CheckBox)row.FindControl("chkWorkspaceFile")).Checked)
                    ids.Add((Guid)grdWorkspace.DataKeys[row.RowIndex]["IdFile"]);
            if(ids.Count==0)throw new InvalidOperationException("Tích chọn file trong bảng trước khi thực hiện thao tác.");
            return ids;
        }

        protected void WorkspaceFilterChanged(object sender,EventArgs e) { grdWorkspace.PageIndex=0; BindWorkspace(); }
        protected void grdWorkspace_PageIndexChanging(object sender,GridViewPageEventArgs e) { grdWorkspace.PageIndex=e.NewPageIndex; BindWorkspace(); }
        protected string WorkspaceStatusText(object value)
        {
            string status=Convert.ToString(value);
            return status=="CHUA_TRINH"?"Chưa trình ký":status=="THU_HOI"?"Đã thu hồi":GetSigningStatusText(value);
        }

        protected string WorkspaceDeliveryFilesText(object value)
        {
            if(value==null || value==DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(value)))
                return "Lần gửi cũ: xem lịch sử thay đổi hồ sơ.";
            try
            {
                return string.Join("; ",Newtonsoft.Json.Linq.JArray.Parse(Convert.ToString(value))
                    .Select(f=>Convert.ToString(f["TenFile"])+" · v"+Convert.ToString(f["FileVersion"])
                        +((bool?)f["DaKy"]==true?" (bản đã ký)":" (bản chưa ký)")));
            }
            catch(Newtonsoft.Json.JsonException) { return "Không đọc được danh sách file của lần gửi này."; }
        }

        protected void grdWorkspace_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            if(e.CommandName!="FILE_DETAIL")return;
            try { OpenWorkspaceFile(Guid.Parse(Convert.ToString(e.CommandArgument))); }
            catch(Exception ex) { ShowNotify(ex.Message,MSGType.Warning); }
        }

        private void OpenWorkspaceFile(Guid fileId)
        {
            var row=DocumentManager.Instance.GetWorkspaceFiles(WorkspaceDocumentId).AsEnumerable().FirstOrDefault(r=>(Guid)r["IdFile"]==fileId);
            if(row==null)throw new InvalidOperationException("File đã thay đổi. Hãy tải lại danh sách.");
            WorkspaceFileId=fileId;
            pnlWorkspaceConfirm.Visible=false;
            ViewState.Remove("WorkspaceRestoreTarget");
            WorkspaceRootId=(Guid)row["IdChuoiFile"];
            WorkspaceLocked=Convert.ToBoolean(row["DaKhoa"]);
            lblWorkspaceFileName.Text=HttpUtility.HtmlEncode(Convert.ToString(row["TenFile"]));
            lnkWorkspaceView.NavigateUrl=GetFileUrl(row["FileUrl"]);
            lnkWorkspaceView.Attributes["data-path"] = lnkWorkspaceView.NavigateUrl;
            lnkWorkspaceDownload.NavigateUrl = GetWorkspaceDownloadUrl(row["FileUrl"]);
            lnkWorkspaceSigned.NavigateUrl=GetFileUrl(row["SignedFileUrl"]);
            lnkWorkspaceSigned.Attributes["data-path"] = lnkWorkspaceSigned.NavigateUrl;
            lnkWorkspaceSignedDownload.NavigateUrl = GetWorkspaceDownloadUrl(row["SignedFileUrl"]);
            lnkWorkspaceSignedDownload.Visible = HasValue(row["SignedFileUrl"]);
            lnkWorkspaceSigned.Visible=HasValue(row["SignedFileUrl"]);
            btnWorkspaceReplace.Visible=btnWorkspaceRemove.Visible=!WorkspaceLocked&&CanManageFiles();
            lblWorkspaceLocked.Visible=WorkspaceLocked;
            chkWorkspaceRecall.Visible=!WorkspaceLocked&&Convert.ToString(row["TrangThai"])==DocumentSigningStatusKeys.Pending&&CanManageFiles();
            chkWorkspaceRecall.Checked=false;
            var timeline=DocumentManager.Instance.GetFileTimeline(WorkspaceDocumentId,WorkspaceRootId);
            grdFileTimeline.DataSource=timeline;
            grdFileTimeline.DataBind();
            var lineageFiles=new HashSet<Guid>(timeline.AsEnumerable().Where(r=>r["IdFile"]!=DBNull.Value).Select(r=>(Guid)r["IdFile"]));
            var signing=DocumentManager.Instance.GetSigningHistory(WorkspaceDocumentId);
            var fileSigning=signing.Clone();
            foreach(var item in signing.AsEnumerable().Where(r=>r["IdFileNguon"]!=DBNull.Value&&lineageFiles.Contains((Guid)r["IdFileNguon"])))
                fileSigning.ImportRow(item);
            rptWorkspaceSigningHistory.DataSource=fileSigning;
            rptWorkspaceSigningHistory.DataBind();
            pnlWorkspaceSigningHistory.Visible=fileSigning.Rows.Count>0;
            OpenSigningModal(mdlWorkspaceFile,"WorkspaceFile");
        }

        protected bool WorkspaceCanRestore(object fileId)
        {
            return fileId!=null&&fileId!=DBNull.Value&&(Guid)fileId!=WorkspaceFileId&&!WorkspaceLocked&&CanManageFiles();
        }

        protected void btnWorkspaceAdd_Click(object sender,EventArgs e)
        {
            ViewState["WorkspaceReplace"]=false;
            ViewState["WorkspaceExpectedSet"]=DocumentManager.Instance.GetCurrentDocumentFileSet(WorkspaceDocumentId).VersionId;
            fuWorkspaceFiles.AllowMultiple=true;
            lblWorkspaceUploadHint.Text="Có thể chọn tối đa 10 file. Tải lên thành công sẽ lưu vào hồ sơ.";
            OpenSigningModal(mdlWorkspaceUpload,"WorkspaceUpload");
        }

        protected void btnWorkspaceReplace_Click(object sender,EventArgs e)
        {
            ViewState["WorkspaceReplace"]=true;
            ViewState["WorkspaceRecall"]=chkWorkspaceRecall.Checked;
            fuWorkspaceFiles.AllowMultiple=false;
            lblWorkspaceUploadHint.Text="Chọn một file để thay bản hiện tại. Các bản cũ được giữ trong lịch sử.";
            CloseWorkspaceModal(mdlWorkspaceFile);
            OpenSigningModal(mdlWorkspaceUpload,"WorkspaceUpload");
        }

        protected void btnWorkspaceUpload_Click(object sender,EventArgs e)
        {
            try
            {
                EnsureDocumentActionAccess(WorkspaceDocumentId,DocumentPermissionKeys.ManageFiles);
                bool replace=(bool)(ViewState["WorkspaceReplace"]??false);
                var files=fuWorkspaceFiles.PostedFiles.Cast<HttpPostedFile>().Where(f=>f.ContentLength>0).ToList();
                if(files.Count==0||files.Count>10||(replace&&files.Count!=1))
                    throw new InvalidOperationException(replace?"Chọn đúng một file mới.":"Chọn từ 1 đến 10 file.");
                var uploader=new SecureFileUploadHandler();
                foreach(var file in files)
                {
                    var validation=uploader.ValidateDocumentVersionFile(file);
                    if(!validation.Success)throw new InvalidOperationException(validation.Message);
                }
                var manager=DocumentManager.Instance;
                var current=manager.GetCurrentDocumentFileSet(WorkspaceDocumentId);
                if(replace)
                {
                    var state=manager.GetWorkspaceFiles(WorkspaceDocumentId).AsEnumerable().FirstOrDefault(r=>(Guid)r["IdFile"]==WorkspaceFileId);
                    if(state==null||Convert.ToBoolean(state["DaKhoa"]))throw new InvalidOperationException("File đã thay đổi hoặc đã ký.");
                    if(Convert.ToString(state["TrangThai"])==DocumentSigningStatusKeys.Pending && !(bool)(ViewState["WorkspaceRecall"]??false))
                        throw new InvalidOperationException("Hãy xác nhận thu hồi yêu cầu ký trong chi tiết file trước.");
                }
                else if(current.VersionId!=(Guid?)ViewState["WorkspaceExpectedSet"])
                    throw new InvalidOperationException("Hồ sơ đã thay đổi. Mở lại cửa sổ thêm file.");
                var uploaded=new List<Guid>();
                foreach(var file in files)
                {
                    var result=uploader.UploadDocumentVersionFile(WorkspaceDocumentId,file);
                    if(!result.Success||!result.FileId.HasValue)throw new InvalidOperationException(result.Message);
                    uploaded.Add(result.FileId.Value);
                }
                if(replace)manager.ChangeWorkspaceFile(WorkspaceDocumentId,WorkspaceFileId,uploaded[0],false,(bool)(ViewState["WorkspaceRecall"]??false));
                else manager.SaveDocumentFileSet(WorkspaceDocumentId,current.VersionId,current.FileIds.Concat(uploaded));
                CloseWorkspaceModal(mdlWorkspaceUpload);
                InitControls(WorkspaceDocumentId);
                ShowNotify("Đã lưu file vào hồ sơ.",MSGType.Success);
            }
            catch(Exception ex) { ShowNotify(ex.Message+" Nếu đã chọn file, vui lòng chọn lại.",MSGType.Warning); OpenSigningModal(mdlWorkspaceUpload,"WorkspaceUploadError"); }
        }

        protected void btnWorkspaceRemove_Click(object sender,EventArgs e)
        {
            ViewState.Remove("WorkspaceRestoreTarget");
            lblWorkspaceConfirm.Text="Gỡ file khỏi hồ sơ? File cũ và lịch sử vẫn được giữ.";
            pnlWorkspaceConfirm.Visible=true;
            OpenSigningModal(mdlWorkspaceFile,"WorkspaceRemoveConfirm");
        }

        protected void grdFileTimeline_RowCommand(object sender,GridViewCommandEventArgs e)
        {
            if(e.CommandName!="RESTORE_FILE")return;
            ViewState["WorkspaceRestoreTarget"]=Guid.Parse(Convert.ToString(e.CommandArgument));
            lblWorkspaceConfirm.Text="Khôi phục phiên bản này thành bản hiện tại? Các phiên bản còn lại vẫn được giữ.";
            pnlWorkspaceConfirm.Visible=true;
            OpenSigningModal(mdlWorkspaceFile,"WorkspaceRestoreConfirm");
        }

        protected void btnWorkspaceCancelConfirm_Click(object sender,EventArgs e)
        {
            pnlWorkspaceConfirm.Visible=false;
            ViewState.Remove("WorkspaceRestoreTarget");
            OpenSigningModal(mdlWorkspaceFile,"WorkspaceCancelConfirm");
        }

        protected void btnWorkspaceConfirm_Click(object sender,EventArgs e)
        {
            try
            {
                if(!pnlWorkspaceConfirm.Visible)throw new InvalidOperationException("Chọn thao tác cần xác nhận trước.");
                var target=(Guid?)ViewState["WorkspaceRestoreTarget"];
                DocumentManager.Instance.ChangeWorkspaceFile(WorkspaceDocumentId,WorkspaceFileId,target,target.HasValue,chkWorkspaceRecall.Checked);
                pnlWorkspaceConfirm.Visible=false;
                InitControls(WorkspaceDocumentId);
                if(target.HasValue)OpenWorkspaceFile(target.Value);
                else CloseWorkspaceModal(mdlWorkspaceFile);
                ShowSuccessSaveData();
            }
            catch(Exception ex)
            {
                SysLogger.LogError(ex,"Dossier file change failed for {0}",WorkspaceDocumentId);
                lblWorkspaceConfirm.Text=HttpUtility.HtmlEncode(ex.Message);
                pnlWorkspaceConfirm.Visible=true;
                OpenSigningModal(mdlWorkspaceFile,"WorkspaceConfirmError");
            }
        }
    }
}
