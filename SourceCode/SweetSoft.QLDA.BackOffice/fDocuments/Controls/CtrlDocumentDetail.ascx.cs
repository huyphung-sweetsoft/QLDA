using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fFilesBox;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentDetail : BaseAdminUserControl
    {
        private const string DocumentVersionSavedCallbackKey =
            "DocumentDetailVersionSaved";
        private const string DocumentVersionBeforeSaveCallbackKey =
            "DocumentDetailVersionBeforeSave";
        private const string SigningResultSavedCallbackKey =
            "DocumentDetailSigningResultSaved";
        private const string SetOfficialFileCommand =
            "SET_OFFICIAL_FILE";
        private const string ClearOfficialFileCommand =
            "CLEAR_OFFICIAL_FILE";
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            fbVersions.FileDeletionRequested +=
                FbVersions_FileDeletionRequested;
            BindSigningSignerDropdown();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (ddlSubmitSigningSigner != null
                && ddlSubmitSigningSigner.Items.Count == 0)
            {
                BindSigningSignerDropdown();
            }
            ConfigureSigningControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            RegisterSigningPostBackControls();
            base.OnPreRender(e);
        }

        private void RegisterSigningPostBackControls()
        {
            ScriptManager script = ScriptManager.GetCurrent(Page);
            if (script == null)
                return;

            if (btnSubmitSigning != null)
                script.RegisterAsyncPostBackControl(btnSubmitSigning);

            if (btnCancelSubmitSigning != null)
                script.RegisterAsyncPostBackControl(btnCancelSubmitSigning);

            if (btnCompleteSigning != null)
                script.RegisterAsyncPostBackControl(btnCompleteSigning);

            if (btnCancelSigningResult != null)
                script.RegisterAsyncPostBackControl(btnCancelSigningResult);

            if (btnRequestSigningChanges != null)
                script.RegisterAsyncPostBackControl(btnRequestSigningChanges);

            if (btnCancelSigningChanges != null)
                script.RegisterAsyncPostBackControl(btnCancelSigningChanges);
        }

        private void ConfigureSigningControls()
        {
            if (btnOpenSubmitSigning == null)
                return;

            btnOpenSubmitSigning.Text = GetResourceText(
                BackEndResourceKeys.SUBMIT_FOR_SIGNING);
            if (btnSubmitSigning != null)
                btnSubmitSigning.Text = GetResourceText(
                    BackEndResourceKeys.SUBMIT_FOR_SIGNING);
            if (btnCancelSubmitSigning != null)
                btnCancelSubmitSigning.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);
            if (btnCompleteSigning != null)
                btnCompleteSigning.Text = GetResourceText(
                    BackEndResourceKeys.CONFIRM_SIGNED);
            if (btnCancelSigningResult != null)
                btnCancelSigningResult.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);
            if (btnRequestSigningChanges != null)
                btnRequestSigningChanges.Text = GetResourceText(
                    BackEndResourceKeys.REQUEST_CHANGES);
            if (btnCancelSigningChanges != null)
                btnCancelSigningChanges.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);

            if (ddlSubmitSigningSigner != null)
            {
                ddlSubmitSigningSigner.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
                ddlSubmitSigningSigner.Attributes["onchange"] =
                    "var signerField=document.getElementById('"
                    + hdfSubmitSigningSigner.ClientID
                    + "');if(signerField){signerField.value=this.value;}";
            }

            if (fbSigningResult != null)
            {
                fbSigningResult.IsMultiple = false;
                fbSigningResult.AcceptType =
                    "application/pdf,image/jpeg,image/jpg,image/png";
            }
        }

        private Guid? OfficialFileId
        {
            get
            {
                Guid value;
                return Guid.TryParse(
                    Convert.ToString(ViewState["OfficialFileId"]),
                    out value)
                    ? value
                    : (Guid?)null;
            }
            set
            {
                ViewState["OfficialFileId"] = value.HasValue
                    ? value.Value.ToString()
                    : string.Empty;
            }
        }

        private bool RequiresSigning
        {
            get
            {
                return ViewState["RequiresSigning"] != null
                    && Convert.ToBoolean(ViewState["RequiresSigning"]);
            }
            set { ViewState["RequiresSigning"] = value; }
        }

        public bool InitControls(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            DataTable detail = DocumentManager.Instance
                .GetCompanyDocumentDetail(idTaiLieu);
            if (detail.Rows.Count == 0)
                return false;

            DataRow document = detail.Rows[0];
            hdfIdTaiLieu.Value = idTaiLieu.ToString();
            DataTable versions = DocumentManager.Instance
                .GetDocumentVersions(idTaiLieu);
            DataTable signingHistory = DocumentManager.Instance
                .GetSigningHistory(idTaiLieu);
            DataTable customerHistory = DocumentManager.Instance
                .GetCustomerDeliveryHistory(idTaiLieu);
            DataTable storageHistory = DocumentManager.Instance
                .GetPhysicalStorageHistory(idTaiLieu);
            DataTable activityHistory = DocumentManager.Instance
                .GetDocumentActivityHistory(idTaiLieu);

            bool requiresSigning = GetBoolean(document, "CanTrinhKy");
            bool requiresCustomer = GetBoolean(
                document,
                "CanGuiKhachHang");
            bool requiresStorage = GetBoolean(document, "CanLuuVatLy");
            RequiresSigning = requiresSigning;
            OfficialFileId = GetGuid(document, "IdFileBanChinhThuc");

            BindHeader(document);
            BindOverview(document);
            BindOfficialFile(document);
            BindSummary(
                document,
                requiresSigning,
                requiresCustomer,
                requiresStorage);

            BindRepeater(
                rptVersions,
                pnlVersions,
                pnlNoVersions,
                versions);
            lblVersionCount.Text = versions.Rows.Count.ToString();
            BindVersionUploader(idTaiLieu);

            bool showSigning = requiresSigning
                || signingHistory.Rows.Count > 0;
            phSigningTab.Visible = showSigning;
            phSigningPane.Visible = showSigning;
            pnlSigningActions.Visible = requiresSigning
                && CURRENT_PAGE.IsEdit
                && CanSubmitCurrentVersion(versions, signingHistory);
            BindRepeater(
                rptSigning,
                pnlSigning,
                pnlNoSigning,
                signingHistory);

            bool showCustomer = requiresCustomer
                || customerHistory.Rows.Count > 0;
            phCustomerTab.Visible = showCustomer;
            phCustomerPane.Visible = showCustomer;
            BindRepeater(
                rptCustomer,
                pnlCustomer,
                pnlNoCustomer,
                customerHistory);

            bool showStorage = requiresStorage
                || storageHistory.Rows.Count > 0;
            phStorageTab.Visible = showStorage;
            phStoragePane.Visible = showStorage;
            BindRepeater(
                rptStorage,
                pnlStorage,
                pnlNoStorage,
                storageHistory);

            BindRepeater(
                rptActivity,
                pnlActivity,
                pnlNoActivity,
                activityHistory);

            btnBack.NavigateUrl = RewriteURLHelper.Documents;
            btnBack.ToolTip = btnBack.Text = GetResourceText(
                BackEndResourceKeys.BACK_TO_LIST);
            return true;
        }

        private void BindVersionUploader(Guid idTaiLieu)
        {
            pnlVersionUploader.Visible = CURRENT_PAGE.IsEdit;
            if (!pnlVersionUploader.Visible)
                return;

            fbVersions.IsMultiple = true;
            fbVersions.IsEnabled = true;
            fbVersions.BeforeSaveDataCallbackKey =
                DocumentVersionBeforeSaveCallbackKey;
            fbVersions.SaveDataCallbackKey =
                DocumentVersionSavedCallbackKey;
            fbVersions.LoadFile(
                idTaiLieu,
                FileUploadTypes.DocumentVersion);
        }

        public void HandleFileCallback(string key)
        {
            bool isBeforeSave = string.Equals(
                key,
                DocumentVersionBeforeSaveCallbackKey,
                StringComparison.Ordinal);
            bool isAfterSave = string.Equals(
                key,
                DocumentVersionSavedCallbackKey,
                StringComparison.Ordinal);
            bool isSigningResultSaved = string.Equals(
                key,
                SigningResultSavedCallbackKey,
                StringComparison.Ordinal);
            if (!isBeforeSave && !isAfterSave && !isSigningResultSaved)
            {
                return;
            }

            if (isSigningResultSaved)
            {
                Guid signingId;
                if (!Guid.TryParse(
                        hdfSigningResultId.Value,
                        out signingId)
                    || signingId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Không xác định được lần trình ký cần cập nhật.");
                }

                fbSigningResult.LoadFile(
                    signingId,
                    FileUploadTypes.DocumentSigningResult);
                mdlSigningResult.UpdateContentModal();
                KeepSigningTabOpen();
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || idTaiLieu == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Không xác định được hồ sơ cần cập nhật.");
            }

            if (!CURRENT_PAGE.IsEdit)
            {
                throw new InvalidOperationException(
                    GetResourceText(
                        BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
            }

            if (isBeforeSave)
            {
                List<Guid> removedFileIds =
                    fbVersions.GetPendingRemovedFileIds();
                CreateRequestDocumentManager()
                    .PrepareDocumentVersionFilesForDeletion(
                        idTaiLieu,
                        removedFileIds);
                return;
            }

            CreateRequestDocumentManager().SyncDocumentVersions(idTaiLieu);
            InitControls(idTaiLieu);
            upDetail.Update();
            KeepVersionsTabOpen();
        }

        private void FbVersions_FileDeletionRequested(
            object sender,
            FileDeletionRequestedEventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                throw new InvalidOperationException(
                    GetResourceText(
                        BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || idTaiLieu == Guid.Empty
                || e == null
                || e.RefId != idTaiLieu
                || e.RefType != FileUploadTypes.DocumentVersion)
            {
                throw new InvalidOperationException(
                    "Danh sách tệp cần xóa không thuộc hồ sơ hiện tại.");
            }

            if (e.FileIds == null || e.FileIds.Count == 0)
            {
                e.Handled = true;
                e.Succeeded = true;
                return;
            }

            DocumentVersionFileDeletionResult result =
                CreateRequestDocumentManager()
                    .DeleteDocumentVersionFiles(idTaiLieu, e.FileIds);
            e.Handled = true;
            e.Succeeded = true;
            e.WarningMessage = result == null
                ? null
                : result.WarningMessage;
        }

        private DocumentManager CreateRequestDocumentManager()
        {
            return new DocumentManager(SweetContext.Current);
        }

        private void BindSigningSignerDropdown()
        {
            if (ddlSubmitSigningSigner == null
                || ddlSubmitSigningSigner.Items.Count > 0)
            {
                return;
            }

            ddlSubmitSigningSigner.Items.Clear();
            List<AspnetUser> users = CreateRequestDocumentManager()
                .GetAvailableSigningUsers();
            foreach (AspnetUser user in users ?? new List<AspnetUser>())
            {
                if (user == null || user.UserId == Guid.Empty)
                    continue;

                string displayName = string.IsNullOrWhiteSpace(user.DisplayName)
                    ? user.UserName
                    : user.DisplayName;
                ddlSubmitSigningSigner.Items.Add(
                    new ListItem(
                        string.IsNullOrWhiteSpace(displayName)
                            ? user.UserId.ToString()
                            : displayName,
                        user.UserId.ToString()));
            }
        }

        protected void btnOpenSubmitSigning_Click(
            object sender,
            EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || idTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                DataTable versions = CreateRequestDocumentManager()
                    .GetDocumentVersions(idTaiLieu);
                DataRow currentVersion = null;
                foreach (DataRow row in versions.Rows)
                {
                    if (row.Table.Columns.Contains("LaPhienBanHienTai")
                        && row["LaPhienBanHienTai"] != DBNull.Value
                        && Convert.ToBoolean(row["LaPhienBanHienTai"]))
                    {
                        currentVersion = row;
                        break;
                    }
                }

                if (currentVersion == null)
                {
                    ShowNotify(
                        "Hồ sơ chưa có phiên bản hiện tại để trình ký.",
                        MSGType.Warning);
                    return;
                }

                hdfSubmitSigningDocumentId.Value = idTaiLieu.ToString();
                lblSubmitSigningVersion.Text = "v"
                    + GetValueText(currentVersion["SoPhienBan"]);
                DataTable detail = CreateRequestDocumentManager()
                    .GetCompanyDocumentDetail(idTaiLieu);
                lblSubmitSigningMethod.Text = detail.Rows.Count == 0
                    ? string.Empty
                    : GetSigningMethodText(detail.Rows[0]["HinhThucKy"]);
                txtSubmitSigningNote.Text = string.Empty;
                if (ddlSubmitSigningSigner.Items.Count == 0)
                    BindSigningSignerDropdown();
                ddlSubmitSigningSigner.SelectedValue = string.Empty;
                hdfSubmitSigningSigner.Value = string.Empty;
                mdlSubmitSigning.Title = GetResourceText(
                    BackEndResourceKeys.SUBMIT_FOR_SIGNING);
                mdlSubmitSigning.OpenModal(true);
                KeepSigningTabOpen();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
        }

        protected void btnSubmitSigning_Click(object sender, EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idNguoiKy;
            string signerValue = GetPostedSigningSignerValue();
            if (!Guid.TryParse(
                    hdfSubmitSigningDocumentId.Value,
                    out idTaiLieu)
                || idTaiLieu == Guid.Empty
                || !Guid.TryParse(
                    signerValue,
                    out idNguoiKy)
                || idNguoiKy == Guid.Empty)
            {
                ShowNotify(
                    GetResourceText(
                        BackEndResourceKeys.SIGNING_SIGNER_REQUIRED),
                    MSGType.Warning);
                return;
            }

            try
            {
                CreateRequestDocumentManager().SubmitDocumentSigning(
                    idTaiLieu,
                    idNguoiKy,
                    txtSubmitSigningNote.Text);
                mdlSubmitSigning.CloseModal(true);
                RefreshSigningDetail(idTaiLieu);
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                LogSigningHandlerError(
                    "submit",
                    idTaiLieu,
                    exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private string GetPostedSigningSignerValue()
        {
            string value = Request.Form[hdfSubmitSigningSigner.UniqueID];
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Request.Form[
                    ddlSubmitSigningSigner.UniqueID
                    + ddlSubmitSigningSigner.HdfValue];
            }

            if (string.IsNullOrWhiteSpace(value))
                value = ddlSubmitSigningSigner.SelectedValue;

            return value;
        }

        protected void btnCancelSubmitSigning_Click(object sender, EventArgs e)
        {
            mdlSubmitSigning.CloseModal(true);
            KeepSigningTabOpen();
        }

        protected void rptSigning_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (!string.Equals(
                    e.CommandName,
                    "CONFIRM_SIGNED",
                    StringComparison.Ordinal)
                && !string.Equals(
                    e.CommandName,
                    "REQUEST_CHANGES",
                    StringComparison.Ordinal))
            {
                return;
            }

            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idTrinhKyTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || !Guid.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idTrinhKyTaiLieu)
                || idTaiLieu == Guid.Empty
                || idTrinhKyTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                DataTable detail = CreateRequestDocumentManager()
                    .GetSigningDetail(idTaiLieu, idTrinhKyTaiLieu);
                if (detail.Rows.Count == 0
                    || !IsPendingSigningStatus(
                        detail.Rows[0]["TrangThaiTrinhKy"]))
                {
                    ShowNotify(
                        "Lần trình ký đã thay đổi hoặc không còn chờ xử lý.",
                        MSGType.Warning);
                    return;
                }

                if (string.Equals(
                        e.CommandName,
                        "CONFIRM_SIGNED",
                        StringComparison.Ordinal))
                {
                    PrepareSigningResultForm(idTaiLieu, idTrinhKyTaiLieu);
                }
                else
                {
                    PrepareSigningChangesForm(idTaiLieu, idTrinhKyTaiLieu);
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
        }

        private void PrepareSigningResultForm(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu)
        {
            hdfSigningResultId.Value = idTrinhKyTaiLieu.ToString();
            txtSigningResultNote.Text = string.Empty;
            fbSigningResult.IsEnabled = true;
            fbSigningResult.IsMultiple = false;
            fbSigningResult.AcceptType =
                "application/pdf,image/jpeg,image/jpg,image/png";
            fbSigningResult.SaveDataCallbackKey =
                SigningResultSavedCallbackKey;
            fbSigningResult.BeforeSaveDataCallbackKey = null;
            fbSigningResult.LoadFile(
                idTrinhKyTaiLieu,
                FileUploadTypes.DocumentSigningResult);
            mdlSigningResult.Title = GetResourceText(
                BackEndResourceKeys.CONFIRM_SIGNED)
                + " / "
                + GetResourceText(BackEndResourceKeys.SIGNING_RESULT_FILE);
            mdlSigningResult.OpenModal(true);
            KeepSigningTabOpen();
        }

        private void PrepareSigningChangesForm(
            Guid idTaiLieu,
            Guid idTrinhKyTaiLieu)
        {
            hdfSigningChangesId.Value = idTrinhKyTaiLieu.ToString();
            txtSigningChangeReason.Text = string.Empty;
            mdlSigningChanges.Title = GetResourceText(
                BackEndResourceKeys.REQUEST_CHANGES);
            mdlSigningChanges.OpenModal(true);
            KeepSigningTabOpen();
        }

        protected void btnCompleteSigning_Click(object sender, EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idTrinhKyTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || !Guid.TryParse(
                    hdfSigningResultId.Value,
                    out idTrinhKyTaiLieu)
                || idTaiLieu == Guid.Empty
                || idTrinhKyTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                CreateRequestDocumentManager().CompleteDocumentSigning(
                    idTaiLieu,
                    idTrinhKyTaiLieu,
                    txtSigningResultNote.Text);
                mdlSigningResult.CloseModal(true);
                RefreshSigningDetail(idTaiLieu);
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                LogSigningHandlerError(
                    "complete",
                    idTaiLieu,
                    exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelSigningResult_Click(object sender, EventArgs e)
        {
            mdlSigningResult.CloseModal(true);
            KeepSigningTabOpen();
        }

        protected void btnRequestSigningChanges_Click(object sender, EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idTrinhKyTaiLieu;
            string reason = (txtSigningChangeReason.Text ?? string.Empty).Trim();
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || !Guid.TryParse(
                    hdfSigningChangesId.Value,
                    out idTrinhKyTaiLieu)
                || idTaiLieu == Guid.Empty
                || idTrinhKyTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                ShowNotify(
                    GetResourceText(
                        BackEndResourceKeys.SIGNING_CHANGE_REASON_REQUIRED),
                    MSGType.Warning);
                return;
            }

            try
            {
                CreateRequestDocumentManager()
                    .RequestDocumentSigningChanges(
                        idTaiLieu,
                        idTrinhKyTaiLieu,
                        reason);
                mdlSigningChanges.CloseModal(true);
                RefreshSigningDetail(idTaiLieu);
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                LogSigningHandlerError(
                    "request changes",
                    idTaiLieu,
                    exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelSigningChanges_Click(object sender, EventArgs e)
        {
            mdlSigningChanges.CloseModal(true);
            KeepSigningTabOpen();
        }

        private void RefreshSigningDetail(Guid idTaiLieu)
        {
            InitControls(idTaiLieu);
            upDetail.Update();
            KeepSigningTabOpen();
        }

        private void KeepSigningTabOpen()
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                GetType(),
                "KeepDocumentSigningTabOpen",
                "var tabElement=document.querySelector('[data-bs-target=\"#document-signing\"]');"
                + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                true);
        }

        private static void LogSigningHandlerError(
            string operation,
            Guid idTaiLieu,
            Exception exception)
        {
            SysLogger.LogError(
                exception,
                "Document signing {0} handler failed for document {1}",
                operation,
                idTaiLieu);
        }

        protected void rptVersions_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            bool setOfficial = string.Equals(
                e.CommandName,
                SetOfficialFileCommand,
                StringComparison.Ordinal);
            bool clearOfficial = string.Equals(
                e.CommandName,
                ClearOfficialFileCommand,
                StringComparison.Ordinal);
            if (!setOfficial && !clearOfficial)
                return;

            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idPhienBanTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || !Guid.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idPhienBanTaiLieu)
                || idTaiLieu == Guid.Empty
                || idPhienBanTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                if (setOfficial)
                {
                    DocumentManager.Instance.SetOfficialFile(
                        idTaiLieu,
                        idPhienBanTaiLieu);
                }
                else
                {
                    DocumentManager.Instance.ClearOfficialFile(
                        idTaiLieu,
                        idPhienBanTaiLieu);
                }

                InitControls(idTaiLieu);
                upDetail.Update();
                KeepVersionsTabOpen();
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void KeepVersionsTabOpen()
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                GetType(),
                "KeepDocumentVersionsTabOpen",
                "var tabElement=document.querySelector('[data-bs-target=\"#document-versions\"]');"
                + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                true);
        }

        private void BindHeader(DataRow document)
        {
            lblDocumentName.Text = GetValueText(document["TenTaiLieu"]);
            lblDocumentCode.Text = GetValueText(document["MaTaiLieu"]);
            lblDocumentStatus.Text = GetDocumentStatusText(
                document["TrangThaiTaiLieu"]);
            lblDocumentStatus.CssClass = GetDocumentStatusCss(
                document["TrangThaiTaiLieu"]);
        }

        private void BindOverview(DataRow document)
        {
            lblDocumentGroup.Text = GetValueText(document["TenNhom"]);
            lblDocumentType.Text = GetValueText(document["TenLoai"]);
            lblResponsibleEmployee.Text = GetValueText(
                document["TenNhanVienPhuTrach"]);
            lblCreatedBy.Text = GetActorText(
                null,
                document["NguoiTao"]);
            lblCreatedDate.Text = FormatDate(document["NgayTao"]);
            lblUpdatedDate.Text = FormatDate(document["NgayCapNhat"]);
            lblDescription.Text = GetValueText(document["MoTa"]);
        }

        private void BindOfficialFile(DataRow document)
        {
            bool hasOfficialFile = HasValue(
                    document["IdFileBanChinhThuc"])
                && !string.IsNullOrWhiteSpace(
                    Convert.ToString(document["FileChinhThucUrl"]));

            pnlOfficialFile.Visible = hasOfficialFile;
            pnlNoOfficialFile.Visible = !hasOfficialFile;
            if (!hasOfficialFile)
                return;

            lnkOfficialFile.NavigateUrl = GetFileUrl(
                document["FileChinhThucUrl"]);
            lnkOfficialFile.Text = GetFileName(
                document["TenFileChinhThucGoc"],
                document["TenFileChinhThuc"]);

            string extension = Convert.ToString(
                document["PhanMoRongFileChinhThuc"]);
            string size = FormatFileSize(
                document["DungLuongFileChinhThuc"]);
            lblOfficialFileMeta.Text = JoinNonEmpty(extension, size);
        }

        private void BindSummary(
            DataRow document,
            bool requiresSigning,
            bool requiresCustomer,
            bool requiresStorage)
        {
            lblSigningSummary.Text = requiresSigning
                ? JoinNonEmpty(
                    GetSigningMethodText(document["HinhThucKy"]),
                    GetDocumentStatusText(
                        document["TrangThaiTaiLieu"]))
                : GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);

            lblCustomerSummary.Text = requiresCustomer
                ? GetCustomerStatusText(
                    true,
                    document["TrangThaiGuiKhach"])
                : GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);

            lblStorageSummary.Text = requiresStorage
                ? GetPhysicalStorageStatusText(
                    true,
                    document["TrangThaiLuuTru"])
                : GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);
        }

        private static void BindRepeater(
            System.Web.UI.WebControls.Repeater repeater,
            System.Web.UI.WebControls.Panel dataPanel,
            System.Web.UI.WebControls.Panel emptyPanel,
            DataTable data)
        {
            bool hasData = data != null && data.Rows.Count > 0;
            dataPanel.Visible = hasData;
            emptyPanel.Visible = !hasData;
            repeater.DataSource = data;
            repeater.DataBind();
        }

        protected string GetDocumentStatusText(object value)
        {
            string status = Convert.ToString(value);
            if (status == DocumentStatusKeys.Drafting)
                return GetResourceText(BackEndResourceKeys.DRAFTING);
            if (status == DocumentStatusKeys.PendingSignature)
                return GetResourceText(BackEndResourceKeys.PENDING_SIGNATURE);
            if (status == DocumentStatusKeys.ChangesRequested)
                return GetResourceText(BackEndResourceKeys.CHANGES_REQUESTED);
            if (status == DocumentStatusKeys.Signed)
                return GetResourceText(BackEndResourceKeys.SIGNED);
            if (status == DocumentStatusKeys.Completed)
                return GetResourceText(BackEndResourceKeys.COMPLETED);
            return GetValueText(value);
        }

        protected string GetDocumentStatusCss(object value)
        {
            string status = Convert.ToString(value);
            if (status == DocumentStatusKeys.Signed
                || status == DocumentStatusKeys.Completed)
            {
                return "badge bg-success";
            }

            if (status == DocumentStatusKeys.PendingSignature)
                return "badge bg-info";
            if (status == DocumentStatusKeys.ChangesRequested)
                return "badge bg-warning text-dark";
            return "badge bg-secondary";
        }

        protected string GetSigningMethodText(object value)
        {
            string method = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(method))
                return GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);

            return string.Equals(
                    method,
                    DocumentSigningMethodKeys.DigitalExternal,
                    StringComparison.OrdinalIgnoreCase)
                ? GetResourceText(
                    BackEndResourceKeys.EXTERNAL_DIGITAL_SIGNING)
                : GetResourceText(BackEndResourceKeys.PAPER_SIGNING);
        }

        protected string GetSigningStatusText(object value)
        {
            string status = Convert.ToString(value);
            if (IsPendingSigningStatus(status))
            {
                return GetResourceText(
                    BackEndResourceKeys.SIGNING_PENDING);
            }

            if (string.Equals(
                    status,
                    DocumentSigningStatusKeys.ChangesRequested,
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetResourceText(
                    BackEndResourceKeys.SIGNING_REQUEST_CHANGES);
            }

            if (string.Equals(
                    status,
                    DocumentSigningStatusKeys.Signed,
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetResourceText(
                    BackEndResourceKeys.SIGNING_COMPLETED);
            }

            return GetValueText(value);
        }

        protected string GetSigningStatusCss(object value)
        {
            string status = Convert.ToString(value);
            if (IsPendingSigningStatus(status))
                return "badge bg-info";
            if (string.Equals(
                    status,
                    DocumentSigningStatusKeys.ChangesRequested,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "badge bg-warning text-dark";
            }
            if (string.Equals(
                    status,
                    DocumentSigningStatusKeys.Signed,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "badge bg-success";
            }

            return "badge bg-secondary";
        }

        protected bool CanManagePendingSigning(object value)
        {
            return CURRENT_PAGE.IsEdit && IsPendingSigningStatus(value);
        }

        private static bool IsPendingSigningStatus(object value)
        {
            string status = Convert.ToString(value);
            return string.Equals(
                       status,
                       DocumentSigningStatusKeys.Pending,
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       status,
                       DocumentStatusKeys.PendingSignature,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanSubmitCurrentVersion(
            DataTable versions,
            DataTable signingHistory)
        {
            Guid? currentVersionId = GetCurrentVersionId(versions);
            if (!currentVersionId.HasValue)
                return false;

            if (signingHistory == null)
                return true;

            foreach (DataRow row in signingHistory.Rows)
            {
                Guid? signingVersionId = GetGuid(
                    row,
                    "IdPhienBanTaiLieu");
                if (!signingVersionId.HasValue
                    || signingVersionId.Value != currentVersionId.Value)
                {
                    continue;
                }

                string status = Convert.ToString(
                    row["TrangThaiTrinhKy"]);
                if (IsPendingSigningStatus(status)
                    || string.Equals(
                        status,
                        DocumentSigningStatusKeys.ChangesRequested,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        status,
                        DocumentSigningStatusKeys.Signed,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static Guid? GetCurrentVersionId(DataTable versions)
        {
            if (versions == null)
                return null;

            foreach (DataRow row in versions.Rows)
            {
                if (GetBoolean(row, "LaPhienBanHienTai"))
                {
                    Guid? versionId = GetGuid(
                        row,
                        "IdPhienBanTaiLieu");
                    if (versionId.HasValue && versionId.Value != Guid.Empty)
                        return versionId;
                }
            }

            return null;
        }

        protected string GetCustomerStatusText(
            object requiredValue,
            object statusValue)
        {
            if (!Convert.ToBoolean(requiredValue))
                return GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);

            string status = Convert.ToString(statusValue);
            if (status == DocumentCustomerStatusKeys.NotSent)
                return GetResourceText(BackEndResourceKeys.NOT_SENT);
            if (status == DocumentCustomerStatusKeys.Sent)
                return GetResourceText(BackEndResourceKeys.SENT);
            if (status == DocumentCustomerStatusKeys.WaitingForReturn)
                return GetResourceText(BackEndResourceKeys.WAITING_FOR_RETURN);
            if (status == DocumentCustomerStatusKeys.ReceivedBack)
                return GetResourceText(BackEndResourceKeys.RECEIVED_BACK);
            return GetValueText(statusValue);
        }

        protected string GetPhysicalStorageStatusText(
            object requiredValue,
            object statusValue)
        {
            if (!Convert.ToBoolean(requiredValue))
                return GetResourceText(BackEndResourceKeys.NOT_APPLICABLE);

            string status = Convert.ToString(statusValue);
            if (status == DocumentPhysicalStorageStatusKeys.NotStored)
                return GetResourceText(BackEndResourceKeys.NOT_STORED);
            if (status == DocumentPhysicalStorageStatusKeys.Stored)
                return GetResourceText(BackEndResourceKeys.STORED);
            if (status == DocumentPhysicalStorageStatusKeys.CheckedOut)
                return GetResourceText(BackEndResourceKeys.CHECKED_OUT);
            return GetValueText(statusValue);
        }

        protected string GetFileName(
            object originalNameValue,
            object fileNameValue)
        {
            string originalName = Convert.ToString(originalNameValue);
            return string.IsNullOrWhiteSpace(originalName)
                ? GetValueText(fileNameValue)
                : originalName;
        }

        protected string GetVersionSourceText(object value)
        {
            string source = Convert.ToString(value);
            if (string.Equals(
                    source,
                    "TEMPLATE",
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetResourceText(
                    BackEndResourceKeys.DOCUMENT_TEMPLATE);
            }

            if (string.Equals(
                    source,
                    "UPLOAD",
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetResourceText(BackEndResourceKeys.UPLOAD);
            }

            return GetValueText(value);
        }

        protected string GetFileUrl(object value)
        {
            return FileHelpers.IsValidPath(Convert.ToString(value));
        }

        protected bool CanOpenFile(object value)
        {
            string path = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(path))
                return false;

            Uri absoluteUri;
            if (Uri.TryCreate(path, UriKind.Absolute, out absoluteUri))
            {
                return absoluteUri.Scheme == Uri.UriSchemeHttp
                    || absoluteUri.Scheme == Uri.UriSchemeHttps;
            }

            try
            {
                string virtualPath = path.StartsWith("/", StringComparison.Ordinal)
                    ? path
                    : "/" + path;
                string physicalPath = HostingEnvironment.MapPath(virtualPath);
                return !string.IsNullOrWhiteSpace(physicalPath)
                    && File.Exists(physicalPath);
            }
            catch
            {
                return false;
            }
        }

        protected bool IsOfficialVersion(object fileIdValue)
        {
            Guid fileId;
            return OfficialFileId.HasValue
                && Guid.TryParse(Convert.ToString(fileIdValue), out fileId)
                && OfficialFileId.Value == fileId;
        }

        protected bool CanSetOfficialFile(
            object fileIdValue,
            object fileUrlValue)
        {
            return CURRENT_PAGE.IsEdit
                && !RequiresSigning
                && !IsOfficialVersion(fileIdValue)
                && CanOpenFile(fileUrlValue);
        }

        protected bool CanClearOfficialFile(object fileIdValue)
        {
            return CURRENT_PAGE.IsEdit
                && !RequiresSigning
                && IsOfficialVersion(fileIdValue);
        }

        protected string FormatFileSize(object value)
        {
            if (!HasValue(value))
                return "—";

            long fileSize;
            if (!long.TryParse(Convert.ToString(value), out fileSize))
                return "—";

            return Helpers.FileSizeFormatter.FormatSize(fileSize);
        }

        protected string FormatDate(object value)
        {
            if (!HasValue(value))
                return "—";

            return ConvertDateTimeToString(value);
        }

        protected string GetDateRange(object fromValue, object toValue)
        {
            string from = FormatDate(fromValue);
            string to = FormatDate(toValue);
            return to == "—" ? from : from + " → " + to;
        }

        protected string GetStorageDateText(
            object storedDate,
            object checkedOutDate,
            object returnedDate)
        {
            string result = FormatDate(storedDate);
            if (HasValue(checkedOutDate))
                result += " · " + FormatDate(checkedOutDate);
            if (HasValue(returnedDate))
                result += " · " + FormatDate(returnedDate);
            return result;
        }

        protected string GetRecipientText(object nameValue, object emailValue)
        {
            return JoinNonEmpty(
                Convert.ToString(nameValue),
                Convert.ToString(emailValue));
        }

        protected string GetStorageLocationText(
            object codeValue,
            object nameValue)
        {
            string code = Convert.ToString(codeValue);
            string name = Convert.ToString(nameValue);
            if (string.IsNullOrWhiteSpace(code))
                return GetValueText(name);
            if (string.IsNullOrWhiteSpace(name))
                return code;
            return code + " · " + name;
        }

        protected string GetActorText(object displayNameValue, object userValue)
        {
            string displayName = Convert.ToString(displayNameValue);
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            string userName = Convert.ToString(userValue);
            if (string.IsNullOrWhiteSpace(userName))
                return "—";

            string resolvedName = CURRENT_PAGE.DisplayName(userName);
            return string.IsNullOrWhiteSpace(resolvedName)
                ? userName
                : resolvedName;
        }

        protected string GetActivityDescription(
            object descriptionValue,
            object changeValue)
        {
            string description = Convert.ToString(descriptionValue);
            string changes = Convert.ToString(changeValue);
            if (string.IsNullOrWhiteSpace(description))
                return GetValueText(changes);
            if (string.IsNullOrWhiteSpace(changes))
                return description;

            return description + " " + changes;
        }

        protected string GetActivityTypeText(object value)
        {
            string activityType = Convert.ToString(value);
            if (activityType == DocumentActivityTypeKeys.CreateDocument)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_CREATE_DOCUMENT);
            }
            if (activityType == DocumentActivityTypeKeys.UpdateDocument)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_UPDATE_DOCUMENT);
            }
            if (activityType == DocumentActivityTypeKeys.DeleteDocument)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_DELETE_DOCUMENT);
            }
            if (activityType == DocumentActivityTypeKeys.CreateFromTemplate)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_CREATE_FROM_TEMPLATE);
            }
            if (activityType == DocumentActivityTypeKeys.UploadVersion)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_UPLOAD_VERSION);
            }
            if (activityType == DocumentActivityTypeKeys.DeleteVersion)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_DELETE_VERSION);
            }
            if (activityType == DocumentActivityTypeKeys.SetOfficialFile)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_SET_OFFICIAL_FILE);
            }
            if (activityType == DocumentActivityTypeKeys.ClearOfficialFile)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_CLEAR_OFFICIAL_FILE);
            }
            if (activityType == DocumentActivityTypeKeys.SubmitSigning)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_SUBMIT_SIGNING);
            }
            if (activityType == DocumentActivityTypeKeys.RequestSigningChanges)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_REQUEST_SIGNING_CHANGES);
            }
            if (activityType == DocumentActivityTypeKeys.CompleteSigning)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_COMPLETE_SIGNING);
            }

            return GetValueText(value);
        }

        protected string GetActivityReferenceText(object value)
        {
            string referenceType = Convert.ToString(value);
            if (referenceType == DocumentActivityReferenceKeys.Document)
                return GetResourceText(BackEndResourceKeys.DOCUMENT);
            if (referenceType
                == DocumentActivityReferenceKeys.DocumentVersion)
            {
                return GetResourceText(BackEndResourceKeys.VERSION);
            }
            if (referenceType == DocumentActivityReferenceKeys.Signing)
            {
                return GetResourceText(BackEndResourceKeys.SIGNING_HISTORY);
            }

            return GetValueText(value);
        }

        protected string GetYesNoText(object value)
        {
            return HasValue(value) && Convert.ToBoolean(value)
                ? GetResourceText(BackEndResourceKeys.YES)
                : GetResourceText(BackEndResourceKeys.NO);
        }

        protected string GetValueText(object value)
        {
            if (!HasValue(value))
                return "—";

            string result = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(result) ? "—" : result;
        }

        protected bool HasValue(object value)
        {
            return value != null
                && value != DBNull.Value
                && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static bool GetBoolean(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                && Convert.ToBoolean(row[columnName]);
        }

        private static Guid? GetGuid(DataRow row, string columnName)
        {
            Guid value;
            return row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                && Guid.TryParse(Convert.ToString(row[columnName]), out value)
                ? value
                : (Guid?)null;
        }

        private static string JoinNonEmpty(params string[] values)
        {
            return string.Join(
                " · ",
                Array.FindAll(
                    values,
                    value => !string.IsNullOrWhiteSpace(value)
                        && value != "—"));
        }
    }
}
