using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fFilesBox;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
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
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentDetail : BaseAdminUserControl
    {
        protected void btnDocumentPermissions_Click(object sender, EventArgs e)
        {
            try {
                Guid id=Guid.Parse(hdfIdTaiLieu.Value);
                string stamp=DocumentManager.Instance.GetDocumentGrantStamp(id);
                ViewState["GrantDocumentId"]=id;
                ViewState["GrantStamp"]=stamp;
                bool canGrantOutside = DocumentManager.Instance
                    .CanGrantDocumentOutsideProject(id);
                pnlGrantExternalUsers.Visible = canGrantOutside;
                chkGrantExternalUsers.Checked = false;
                ViewState["GrantIncludeExternal"] = false;
                BindDocumentPermissionRows(id, false);
                mdlDocumentPermissions.OpenModal(true);
            } catch(Exception ex) { ShowNotify(ex.Message,MSGType.Warning); }
        }

        private void BindDocumentPermissionRows(Guid documentId)
        {
            BindDocumentPermissionRows(documentId, false);
        }

        private void BindDocumentPermissionRows(
            Guid documentId,
            bool includeExternalUsers)
        {
            var members = DocumentManager.Instance
                .GetDocumentGrantMembers(
                    documentId,
                    includeExternalUsers,
                    includeExternalUsers);
            rptDocumentPermissions.DataSource = members;
            rptDocumentPermissions.DataBind();
        }

        protected void chkGrantExternalUsers_CheckedChanged(
            object sender,
            EventArgs e)
        {
            try
            {
                Guid id;
                if (!Guid.TryParse(
                        Convert.ToString(ViewState["GrantDocumentId"]),
                        out id)
                    || id == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Hãy mở lại phần cấp quyền.");
                }

                bool canGrantOutside = DocumentManager.Instance
                    .CanGrantDocumentOutsideProject(id);
                pnlGrantExternalUsers.Visible = canGrantOutside;
                bool includeExternalUsers = chkGrantExternalUsers.Checked
                    && canGrantOutside;
                chkGrantExternalUsers.Checked = includeExternalUsers;
                ViewState["GrantIncludeExternal"] = includeExternalUsers;
                BindDocumentPermissionRows(id, includeExternalUsers);
                mdlDocumentPermissions.OpenModal(true);
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Warning);
            }
        }

        protected void btnSaveDocumentPermissions_Click(object sender, EventArgs e)
        {
            try {
                Guid id;
                if(!Guid.TryParse(Convert.ToString(ViewState["GrantDocumentId"]), out id)
                    || id == Guid.Empty)
                    throw new InvalidOperationException("Hãy mở lại phần cấp quyền.");
                var grants=new List<DocumentGrant>();
                foreach(RepeaterItem row in rptDocumentPermissions.Items) {
                    bool updateInfo=((CheckBox)row.FindControl("grantUpdateInfo")).Checked;
                    bool manageFiles=((CheckBox)row.FindControl("grantManageFiles")).Checked;
                    bool signing=((CheckBox)row.FindControl("grantSigning")).Checked;
                    bool customerDelivery=((CheckBox)row.FindControl("grantCustomerDelivery")).Checked;
                    bool physicalStorage=((CheckBox)row.FindControl("grantPhysicalStorage")).Checked;
                    bool delete=((CheckBox)row.FindControl("grantDelete")).Checked;
                    grants.Add(new DocumentGrant {
                        UserId=Guid.Parse(((HiddenField)row.FindControl("grantUserId")).Value),
                        CanView=((CheckBox)row.FindControl("grantView")).Checked
                            || updateInfo
                            || manageFiles
                            || signing
                            || customerDelivery
                            || physicalStorage
                            || delete,
                        CanUpdateInfo=updateInfo,
                        CanManageFiles=manageFiles,
                        CanSigning=signing,
                        CanCustomerDelivery=customerDelivery,
                        CanPhysicalStorage=physicalStorage,
                        CanUpdate=false,
                        CanDelete=delete
                    });
                }
                DocumentManager.Instance.SaveDocumentGrants(id,grants,Convert.ToString(ViewState["GrantStamp"]));
                ViewState.Remove("GrantDocumentId");
                mdlDocumentPermissions.CloseModal(true);
                ShowNotify("Đã lưu quyền hồ sơ.",MSGType.Success);
            } catch(Exception ex) { ShowNotify(ex.Message,MSGType.Warning); }
        }

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
        private const string ViewVersionFilesCommand =
            "VIEW_VERSION_FILES";
        private const string RestoreVersionCommand =
            "RESTORE_VERSION";
        private const string CustomerDeliverySubmissionSessionKeyPrefix =
            "DocumentCustomerDeliverySubmission:";

        /// <summary>
        /// A project detail page assigns this value on every request.  An empty
        /// value preserves the existing company-document behaviour.
        /// </summary>
        public Guid ProjectId
        {
            get
            {
                object value = ViewState["ProjectId"];
                if (value is Guid)
                    return (Guid)value;

                Guid result;
                return Guid.TryParse(Convert.ToString(value), out result)
                    ? result
                    : Guid.Empty;
            }
            set { ViewState["ProjectId"] = value; }
        }

        private bool IsProjectContext
        {
            get { return ProjectId != Guid.Empty; }
        }

        protected string DocumentScopeIconCss
        {
            get
            {
                return IsProjectContext
                    ? "fas fa-project-diagram me-1"
                    : "fas fa-building me-1";
            }
        }

        protected string DocumentScopeText
        {
            get
            {
                return GetResourceText(
                    IsProjectContext
                        ? BackEndResourceKeys.PROJECT_DOCUMENTS
                        : BackEndResourceKeys.COMPANY_DOCUMENT);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            fbVersions.UseDocumentFileSets = true;
            // Templated controls inside ExtraModal are created by the modal's
            // own Init. Rebuild the permission repeater at InitComplete so it
            // exists before ASP.NET loads posted checkbox values, without
            // dereferencing the modal footer button too early. The previous
            // OnInit access caused every detail-page postback (including file
            // uploads) to fail with NullReferenceException.
            if (Page != null)
                Page.InitComplete += Page_InitComplete;
            BindSigningSignerDropdown();
            BindCustomerDeliveryDropdowns();
            BindPhysicalStorageLocations();
        }

        private void Page_InitComplete(object sender, EventArgs e)
        {
            if (!IsPostBack
                || btnSaveDocumentPermissions == null
                || rptDocumentPermissions == null
                || hdfIdTaiLieu == null
                || Request.Form[btnSaveDocumentPermissions.UniqueID] == null)
            {
                return;
            }

            Guid postedGrantDocumentId;
            if (Guid.TryParse(
                    Request.Form[hdfIdTaiLieu.UniqueID],
                    out postedGrantDocumentId)
                && postedGrantDocumentId != Guid.Empty)
            {
                // The repeater must be bound before LoadPostData so checkbox
                // values from the permission modal are preserved.
                BindDocumentPermissionRows(
                    postedGrantDocumentId,
                    GetPostedGrantIncludeExternal());
            }
        }

        private bool GetPostedGrantIncludeExternal()
        {
            return chkGrantExternalUsers != null
                && !string.IsNullOrWhiteSpace(
                    Request.Form[chkGrantExternalUsers.UniqueID]);
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
            if (ddlCustomerDeliveryCustomer != null
                && ddlCustomerDeliveryCustomer.Items.Count == 0)
            {
                BindCustomerDeliveryDropdowns();
            }
            ConfigureCustomerDeliveryControls();
            if (ddlPhysicalStorageLocation != null
                && ddlPhysicalStorageLocation.Items.Count == 0)
            {
                BindPhysicalStorageLocations();
            }
            ConfigurePhysicalStorageControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            RegisterSigningPostBackControls();
            RegisterCustomerDeliveryPostBackControls();
            RegisterPhysicalStoragePostBackControls();
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

        private void RegisterCustomerDeliveryPostBackControls()
        {
            ScriptManager script = ScriptManager.GetCurrent(Page);
            if (script == null)
                return;

            if (btnCustomerDeliverySend != null)
                script.RegisterAsyncPostBackControl(btnCustomerDeliverySend);

            if (btnCancelCustomerDelivery != null)
                script.RegisterAsyncPostBackControl(btnCancelCustomerDelivery);

            if (btnUpdateCustomerDeliveryStatus != null)
                script.RegisterAsyncPostBackControl(
                    btnUpdateCustomerDeliveryStatus);

            if (btnCancelCustomerDeliveryStatus != null)
                script.RegisterAsyncPostBackControl(
                    btnCancelCustomerDeliveryStatus);
        }

        private void RegisterPhysicalStoragePostBackControls()
        {
            ScriptManager script = ScriptManager.GetCurrent(Page);
            if (script == null)
                return;

            if (btnPhysicalStorageSave != null)
                script.RegisterAsyncPostBackControl(btnPhysicalStorageSave);

            if (btnCancelPhysicalStorage != null)
                script.RegisterAsyncPostBackControl(btnCancelPhysicalStorage);
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

        private void ConfigureCustomerDeliveryControls()
        {
            if (btnOpenCustomerDelivery != null)
            {
                btnOpenCustomerDelivery.Text = GetResourceText(
                    BackEndResourceKeys.SEND_CUSTOMER);
            }
            if (btnCustomerDeliverySend != null)
            {
                btnCustomerDeliverySend.Text = GetResourceText(
                    BackEndResourceKeys.SEND_CUSTOMER);
            }
            if (btnCancelCustomerDelivery != null)
                btnCancelCustomerDelivery.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);
            if (btnUpdateCustomerDeliveryStatus != null)
            {
                btnUpdateCustomerDeliveryStatus.Text = GetResourceText(
                    BackEndResourceKeys.UPDATE_CUSTOMER_DELIVERY);
            }
            if (btnCancelCustomerDeliveryStatus != null)
            {
                btnCancelCustomerDeliveryStatus.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);
            }

            if (ddlCustomerDeliveryVersion != null)
            {
                ddlCustomerDeliveryVersion.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.SELECT_VALUE);
                ddlCustomerDeliveryVersion.Attributes["onchange"] =
                    GetDropdownHiddenFieldScript(
                        hdfCustomerDeliveryVersion);
            }
            if (ddlCustomerDeliveryCustomer != null)
            {
                ddlCustomerDeliveryCustomer.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.SELECT_VALUE);
                ddlCustomerDeliveryCustomer.Attributes["onchange"] =
                    GetDropdownHiddenFieldScript(
                        hdfCustomerDeliveryCustomer);
            }
            if (ddlCustomerDeliveryChannel != null)
            {
                ddlCustomerDeliveryChannel.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.SELECT_VALUE);
                ddlCustomerDeliveryChannel.Attributes["onchange"] =
                    GetDropdownHiddenFieldScript(
                        hdfCustomerDeliveryChannel);
            }
            if (ddlCustomerDeliveryStatus != null)
            {
                ddlCustomerDeliveryStatus.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.SELECT_VALUE);
                ddlCustomerDeliveryStatus.Attributes["onchange"] =
                    GetDropdownHiddenFieldScript(
                        hdfCustomerDeliveryStatus);
            }
            if (txtCustomerDeliveryRecipient != null)
            {
                txtCustomerDeliveryRecipient.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_THE_VALUE);
            }
            if (txtCustomerDeliveryEmail != null)
            {
                txtCustomerDeliveryEmail.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.TO_EMAIL);
            }
            if (txtCustomerDeliveryNote != null)
            {
                txtCustomerDeliveryNote.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_THE_VALUE);
            }
            if (chkCustomerDeliveryBeforeSigning != null)
            {
                chkCustomerDeliveryBeforeSigning.OnText = GetResourceText(
                    BackEndResourceKeys.YES);
                chkCustomerDeliveryBeforeSigning.OffText = GetResourceText(
                    BackEndResourceKeys.NO);
            }
            if (txtCustomerDeliveryStatusNote != null)
            {
                txtCustomerDeliveryStatusNote.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_THE_VALUE);
            }
        }

        private void ConfigurePhysicalStorageControls()
        {
            if (btnOpenPhysicalStorage != null)
            {
                btnOpenPhysicalStorage.Text = GetResourceText(
                    BackEndResourceKeys.STORE_PHYSICAL_COPY);
            }
            if (btnPhysicalStorageSave != null)
            {
                btnPhysicalStorageSave.Text = GetResourceText(
                    BackEndResourceKeys.STORE_PHYSICAL_COPY);
            }
            if (btnCancelPhysicalStorage != null)
            {
                btnCancelPhysicalStorage.Text = GetResourceText(
                    BackEndResourceKeys.CANCEL);
            }
            if (ddlPhysicalStorageLocation != null)
            {
                ddlPhysicalStorageLocation.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.SELECT_VALUE);
                ddlPhysicalStorageLocation.Attributes["onchange"] =
                    GetDropdownHiddenFieldScript(
                        hdfPhysicalStorageLocation)
                    + GetPhysicalStorageLocationPathScript();
            }
            if (txtPhysicalStorageCode != null)
            {
                txtPhysicalStorageCode.PlaceHolder = "LT-2026-000001";
            }
            if (txtPhysicalStorageOriginalCondition != null)
            {
                txtPhysicalStorageOriginalCondition.PlaceHolder =
                    GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            }
            if (txtPhysicalStorageNote != null)
            {
                txtPhysicalStorageNote.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_THE_VALUE);
            }
            if (chkPhysicalStorageManualCode != null)
            {
                chkPhysicalStorageManualCode.OnText = GetResourceText(
                    BackEndResourceKeys.YES);
                chkPhysicalStorageManualCode.OffText = GetResourceText(
                    BackEndResourceKeys.NO);
            }
        }

        private static string GetDropdownHiddenFieldScript(
            System.Web.UI.WebControls.HiddenField hiddenField)
        {
            if (hiddenField == null)
                return string.Empty;

            return "var valueField=document.getElementById('"
                + hiddenField.ClientID
                + "');if(valueField){valueField.value=this.value;}";
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

        private DataTable GetDocumentDetail(Guid idTaiLieu)
        {
            return IsProjectContext
                ? CreateRequestDocumentManager().GetProjectDocumentDetail(
                    idTaiLieu,
                    ProjectId)
                : DocumentManager.Instance.GetCompanyDocumentDetail(idTaiLieu);
        }

        private void EnsureDocumentActionAccess(
            Guid idTaiLieu,
            ActionKeys action)
        {
            EnsureDocumentActionAccess(idTaiLieu, action.ToString());
        }

        private void EnsureDocumentActionAccess(
            Guid idTaiLieu,
            string action)
        {
            // Always enforce the document-level ACL first.  The old company
            // branch only checked that the row existed, which meant a user
            // could keep an asynchronous postback open and call update
            // handlers without the new Document.* permission.
            DocumentManager.Instance.EnsureDocumentAccess(idTaiLieu, action);

            if (!IsProjectContext)
            {
                // Keep the company page from being used with a forged
                // project-document id during an asynchronous postback.
                TblTaiLieu companyDocument =
                    DocumentManager.Instance.GetCompanyDocumentById(idTaiLieu);
                if (companyDocument == null || companyDocument.IdDuAn.HasValue)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy hồ sơ công ty.");
                }
                return;
            }

            CreateRequestDocumentManager().EnsureProjectDocumentAccess(
                idTaiLieu,
                ProjectId,
                action);
        }

        public bool InitControls(Guid idTaiLieu)
        {
            if (idTaiLieu == Guid.Empty)
                return false;

            DataTable detail = GetDocumentDetail(idTaiLieu);
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
            bool canManageFiles = DocumentManager.Instance.CanAccessDocument(
                idTaiLieu,
                DocumentPermissionKeys.ManageFiles);
            bool canSigning = DocumentManager.Instance.CanAccessDocument(
                idTaiLieu,
                DocumentPermissionKeys.Signing);
            bool canCustomerDelivery = DocumentManager.Instance.CanAccessDocument(
                idTaiLieu,
                DocumentPermissionKeys.CustomerDelivery);
            bool canPhysicalStorage = DocumentManager.Instance.CanAccessDocument(
                idTaiLieu,
                DocumentPermissionKeys.PhysicalStorage);
            RequiresSigning = requiresSigning;
            OfficialFileId = GetGuid(document, "IdFileBanChinhThuc");

            btnDocumentPermissions.Visible = IsProjectContext && DocumentManager.Instance.CanManageDocument(idTaiLieu);
            BindHeader(document);
            BindOverview(document);
            BindOfficialFile(document);
            BindSummary(
                document,
                requiresSigning,
                requiresCustomer,
                requiresStorage);

            DataTable versionHistory = BuildVersionHistory(versions);
            BindRepeater(
                rptVersions,
                pnlVersions,
                pnlNoVersions,
                versionHistory);
            DataRow currentFileSet = versions.AsEnumerable()
                .FirstOrDefault(row => row.Field<bool>("LaPhienBanHienTai"));
            lblVersionCount.Text = currentFileSet == null
                ? "0"
                : Convert.ToInt32(currentFileSet["FileCount"]).ToString();
            BindVersionUploader(idTaiLieu, canManageFiles);

            bool showSigning = requiresSigning
                || signingHistory.Rows.Count > 0;
            phSigningTab.Visible = showSigning;
            phSigningPane.Visible = showSigning;
            pnlSigningActions.Visible = requiresSigning
                && CURRENT_PAGE.IsEdit
                && canSigning
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
            pnlCustomerActions.Visible = requiresCustomer
                && CURRENT_PAGE.IsEdit
                && canCustomerDelivery
                && versions.Rows.Count > 0;
            BindRepeater(
                rptCustomer,
                pnlCustomer,
                pnlNoCustomer,
                customerHistory);

            bool showStorage = requiresStorage
                || storageHistory.Rows.Count > 0;
            phStorageTab.Visible = showStorage;
            phStoragePane.Visible = showStorage;
            pnlPhysicalStorageActions.Visible = requiresStorage
                && CURRENT_PAGE.IsEdit
                && canPhysicalStorage;
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

            btnBack.NavigateUrl = IsProjectContext
                ? RewriteURLHelper.ProjectDocuments(ProjectId)
                : RewriteURLHelper.Documents;
            btnBack.ToolTip = btnBack.Text = GetResourceText(
                BackEndResourceKeys.BACK_TO_LIST);

            string flashKey = idTaiLieu.ToString("N");
            string uploadWarning = Convert.ToString(
                Session["DocumentInitialUploadWarning_" + flashKey]);
            string creationSuccess = Convert.ToString(
                Session["DocumentCreationSuccess_" + flashKey]);
            Session.Remove("DocumentInitialUploadWarning_" + flashKey);
            Session.Remove("DocumentCreationSuccess_" + flashKey);
            if (!string.IsNullOrWhiteSpace(uploadWarning))
                ShowNotify(uploadWarning, MSGType.Warning);
            else if (!string.IsNullOrWhiteSpace(creationSuccess))
                ShowNotify(creationSuccess, MSGType.Success);
            return true;
        }

        private void BindVersionUploader(Guid idTaiLieu, bool canManageFiles)
        {
            pnlVersionUploader.Visible = CURRENT_PAGE.IsEdit && canManageFiles;
            if (!pnlVersionUploader.Visible)
                return;

            fbVersions.IsMultiple = true;
            fbVersions.UseDocumentFileSets = true;
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

            EnsureDocumentActionAccess(
                idTaiLieu,
                isSigningResultSaved
                    ? DocumentPermissionKeys.Signing
                    : DocumentPermissionKeys.ManageFiles);

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

            CreateRequestDocumentManager().SaveDocumentFileSet(idTaiLieu,
                fbVersions.ExpectedDocumentVersionId, fbVersions.GetSubmittedDocumentFileIds());
            InitControls(idTaiLieu);
            upDetail.Update();
            KeepVersionsTabOpen();
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

        private void BindCustomerDeliveryDropdowns()
        {
            if (ddlCustomerDeliveryCustomer != null
                && ddlCustomerDeliveryCustomer.Items.Count == 0)
            {
                new ControlHelpers().BindKhachHang(
                    ddlCustomerDeliveryCustomer);
                ddlCustomerDeliveryCustomer.Items.Insert(
                    0,
                    new ListItem(
                        GetResourceText(BackEndResourceKeys.SELECT_VALUE),
                        string.Empty));
            }

            if (ddlCustomerDeliveryChannel != null
                && ddlCustomerDeliveryChannel.Items.Count == 0)
            {
                ddlCustomerDeliveryChannel.Items.Add(
                    new ListItem(
                        GetCustomerDeliveryChannelText(
                            DocumentCustomerDeliveryChannelKeys.Email),
                        DocumentCustomerDeliveryChannelKeys.Email));
                ddlCustomerDeliveryChannel.Items.Add(
                    new ListItem(
                        GetCustomerDeliveryChannelText(
                            DocumentCustomerDeliveryChannelKeys.Direct),
                        DocumentCustomerDeliveryChannelKeys.Direct));
                ddlCustomerDeliveryChannel.Items.Add(
                    new ListItem(
                        GetCustomerDeliveryChannelText(
                            DocumentCustomerDeliveryChannelKeys.Other),
                        DocumentCustomerDeliveryChannelKeys.Other));
            }

            if (ddlCustomerDeliveryStatus != null
                && ddlCustomerDeliveryStatus.Items.Count == 0)
            {
                ddlCustomerDeliveryStatus.Items.Add(
                    new ListItem(
                        GetCustomerStatusText(
                            true,
                            DocumentCustomerStatusKeys.Sent),
                        DocumentCustomerStatusKeys.Sent));
                ddlCustomerDeliveryStatus.Items.Add(
                    new ListItem(
                        GetCustomerStatusText(
                            true,
                            DocumentCustomerStatusKeys.WaitingForReturn),
                        DocumentCustomerStatusKeys.WaitingForReturn));
                ddlCustomerDeliveryStatus.Items.Add(
                    new ListItem(
                        GetCustomerStatusText(
                            true,
                            DocumentCustomerStatusKeys.ReceivedBack),
                        DocumentCustomerStatusKeys.ReceivedBack));
            }
        }

        private void BindCustomerDeliveryVersions(DataTable versions)
        {
            ddlCustomerDeliveryVersion.Items.Clear();
            ddlCustomerDeliveryVersion.Items.Add(
                new ListItem(
                    GetResourceText(BackEndResourceKeys.SELECT_VALUE),
                    string.Empty));
            if (versions == null)
                return;

            foreach (DataRow version in versions.Rows)
            {
                if (Convert.ToInt32(version["FileCount"]) != 1) continue;
                Guid? versionId = GetGuid(version, "IdPhienBanTaiLieu");
                if (!versionId.HasValue || versionId.Value == Guid.Empty)
                    continue;

                string text = "v" + GetValueText(version["SoPhienBan"]);
                if (GetBoolean(version, "LaPhienBanHienTai"))
                {
                    text += " · " + GetResourceText(
                        BackEndResourceKeys.CURRENT_VERSION);
                }

                ddlCustomerDeliveryVersion.Items.Add(
                    new ListItem(text, versionId.Value.ToString()));
            }
        }

        private void BindPhysicalStorageLocations()
        {
            if (ddlPhysicalStorageLocation == null)
                return;

            string selectedValue = ddlPhysicalStorageLocation.SelectedValue;
            List<TblNoiLuuTru> allLocations =
                DocumentStorageLocationManager.Instance.GetAll()
                ?? new List<TblNoiLuuTru>();
            Dictionary<Guid, TblNoiLuuTru> locationsById =
                allLocations
                .Where(item => item != null && item.IdNoiLuuTru != Guid.Empty)
                .GroupBy(item => item.IdNoiLuuTru)
                .ToDictionary(group => group.Key, group => group.First());

            ddlPhysicalStorageLocation.Items.Clear();
            ddlPhysicalStorageLocation.Items.Add(
                new ListItem(
                    GetResourceText(BackEndResourceKeys.SELECT_VALUE),
                    string.Empty));

            foreach (TblNoiLuuTru location in allLocations
                .Where(item => item != null
                    && item.IdNoiLuuTru != Guid.Empty
                    && item.KichHoat
                    && !item.DaXoa)
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenNoiLuuTru))
            {
                string storagePath = GetPhysicalStorageLocationPath(
                    location,
                    locationsById);
                ListItem option = new ListItem(
                    GetPhysicalStorageLocationOptionText(location),
                    location.IdNoiLuuTru.ToString());
                option.Attributes["data-storage-path"] = storagePath;
                option.Attributes["title"] = storagePath;
                ddlPhysicalStorageLocation.Items.Add(option);
            }

            if (!string.IsNullOrWhiteSpace(selectedValue)
                && ddlPhysicalStorageLocation.Items.FindByValue(
                    selectedValue) != null)
            {
                ddlPhysicalStorageLocation.SelectedValue = selectedValue;
            }

            UpdatePhysicalStorageLocationPath(
                ddlPhysicalStorageLocation.SelectedValue);
        }

        private static string GetPhysicalStorageLocationOptionText(
            TblNoiLuuTru location)
        {
            string code = (location.MaNoiLuuTru ?? string.Empty).Trim();
            string name = (location.TenNoiLuuTru ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(code))
                return string.IsNullOrWhiteSpace(name)
                    ? location.IdNoiLuuTru.ToString()
                    : name;
            return string.IsNullOrWhiteSpace(name)
                ? code
                : code + " — " + name;
        }

        private static string GetPhysicalStorageLocationPath(
            TblNoiLuuTru location,
            IDictionary<Guid, TblNoiLuuTru> locationsById)
        {
            List<string> parts = new List<string>();
            HashSet<Guid> visited = new HashSet<Guid>();
            TblNoiLuuTru current = location;

            while (current != null
                && current.IdNoiLuuTru != Guid.Empty
                && visited.Add(current.IdNoiLuuTru))
            {
                string code = (current.MaNoiLuuTru ?? string.Empty).Trim();
                string name = (current.TenNoiLuuTru ?? string.Empty).Trim();
                string text = string.IsNullOrWhiteSpace(code)
                    ? name
                    : string.IsNullOrWhiteSpace(name)
                        ? code
                        : code + " - " + name;
                if (!string.IsNullOrWhiteSpace(text))
                    parts.Insert(0, text);

                if (!current.IdNoiLuuTruCha.HasValue
                    || !locationsById.TryGetValue(
                        current.IdNoiLuuTruCha.Value,
                        out current))
                {
                    break;
                }
            }

            return parts.Count == 0
                ? location.IdNoiLuuTru.ToString()
                : string.Join("  ›  ", parts);
        }

        private string GetPhysicalStorageLocationPathScript()
        {
            if (pnlPhysicalStorageLocationPath == null
                || lblPhysicalStorageLocationPath == null)
            {
                return string.Empty;
            }

            return "var selectedOption=this.options[this.selectedIndex];"
                + "var storagePath=selectedOption?selectedOption.getAttribute('data-storage-path'):'';"
                + "var storagePathText=document.getElementById('"
                + lblPhysicalStorageLocationPath.ClientID
                + "');if(storagePathText){storagePathText.textContent=storagePath||'';}"
                + "var storagePathPanel=document.getElementById('"
                + pnlPhysicalStorageLocationPath.ClientID
                + "');if(storagePathPanel){storagePathPanel.style.display=storagePath?'block':'none';}";
        }

        private void UpdatePhysicalStorageLocationPath(string selectedValue)
        {
            if (pnlPhysicalStorageLocationPath == null
                || lblPhysicalStorageLocationPath == null)
            {
                return;
            }

            ListItem selected = string.IsNullOrWhiteSpace(selectedValue)
                ? null
                : ddlPhysicalStorageLocation.Items.FindByValue(selectedValue);
            string storagePath = selected == null
                ? string.Empty
                : selected.Attributes["data-storage-path"];

            lblPhysicalStorageLocationPath.Text =
                HttpUtility.HtmlEncode(storagePath ?? string.Empty);
            pnlPhysicalStorageLocationPath.Style["display"] =
                string.IsNullOrWhiteSpace(storagePath) ? "none" : "block";
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.Signing);
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
                DataTable detail = GetDocumentDetail(idTaiLieu);
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.Signing);
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.Signing);
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.Signing);
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.Signing);
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

        protected void btnOpenCustomerDelivery_Click(
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.CustomerDelivery);
                DataTable detail = GetDocumentDetail(idTaiLieu);
                if (detail.Rows.Count == 0
                    || !GetBoolean(detail.Rows[0], "CanGuiKhachHang"))
                {
                    ShowNotify(
                        "Hồ sơ này chưa được cấu hình gửi khách hàng.",
                        MSGType.Warning);
                    return;
                }

                DataTable versions = CreateRequestDocumentManager()
                    .GetDocumentVersions(idTaiLieu);
                if (versions.Rows.Count == 0)
                {
                    ShowNotify(
                        "Hồ sơ chưa có phiên bản để gửi khách hàng.",
                        MSGType.Warning);
                    return;
                }

                BindCustomerDeliveryDropdowns();
                BindCustomerDeliveryVersions(versions);
                if (ddlCustomerDeliveryVersion.Items.Count <= 1)
                {
                    ShowNotify("Hiện gửi khách chỉ hỗ trợ phiên bản có một file. Bộ nhiều file sẽ được hỗ trợ ở chặng tiếp theo.", MSGType.Warning);
                    return;
                }
                hdfCustomerDeliveryDocumentId.Value = idTaiLieu.ToString();
                hdfCustomerDeliveryVersion.Value = string.Empty;
                hdfCustomerDeliveryCustomer.Value = string.Empty;
                hdfCustomerDeliveryChannel.Value =
                    DocumentCustomerDeliveryChannelKeys.Email;
                hdfCustomerDeliverySubmissionToken.Value =
                    Guid.NewGuid().ToString("N");
                ddlCustomerDeliveryVersion.SelectedValue = string.Empty;
                ddlCustomerDeliveryCustomer.SelectedValue = string.Empty;
                ddlCustomerDeliveryChannel.SelectedValue =
                    DocumentCustomerDeliveryChannelKeys.Email;
                txtCustomerDeliveryRecipient.Text = string.Empty;
                txtCustomerDeliveryEmail.Text = string.Empty;
                txtCustomerDeliveryNote.Text = string.Empty;
                dtCustomerDeliveryDeadline.DateValue = null;
                chkCustomerDeliveryBeforeSigning.Checked = false;
                mdlCustomerDelivery.Title = GetResourceText(
                    BackEndResourceKeys.SEND_CUSTOMER);
                mdlCustomerDelivery.OpenModal(true);
                KeepCustomerTabOpen();
            }
            catch (Exception exc)
            {
                LogCustomerDeliveryHandlerError("open", idTaiLieu, exc);
                ShowNotify(exc.Message, MSGType.Warning);
            }
        }

        protected void btnCustomerDeliverySend_Click(
            object sender,
            EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idPhienBanTaiLieu;
            Guid idKhachHang;
            string versionValue = GetPostedDropdownValue(
                hdfCustomerDeliveryVersion,
                ddlCustomerDeliveryVersion);
            string customerValue = GetPostedDropdownValue(
                hdfCustomerDeliveryCustomer,
                ddlCustomerDeliveryCustomer);
            string channelValue = GetPostedDropdownValue(
                hdfCustomerDeliveryChannel,
                ddlCustomerDeliveryChannel);
            if (!Guid.TryParse(
                    hdfCustomerDeliveryDocumentId.Value,
                    out idTaiLieu)
                || !Guid.TryParse(versionValue, out idPhienBanTaiLieu)
                || !Guid.TryParse(customerValue, out idKhachHang)
                || idTaiLieu == Guid.Empty
                || idPhienBanTaiLieu == Guid.Empty
                || idKhachHang == Guid.Empty)
            {
                ShowNotify(
                    "Vui lòng chọn phiên bản và khách hàng cần gửi.",
                    MSGType.Warning);
                return;
            }

            Guid submissionToken;
            if (!Guid.TryParse(
                    hdfCustomerDeliverySubmissionToken.Value,
                    out submissionToken)
                || submissionToken == Guid.Empty)
            {
                ShowNotify(
                    "Phiên gửi khách hàng đã hết hiệu lực. Vui lòng mở lại biểu mẫu gửi.",
                    MSGType.Warning);
                return;
            }

            bool isSubmissionReserved = false;
            bool hasCreatedCustomerDelivery = false;
            try
            {
                if (!TryReserveCustomerDeliverySubmission(submissionToken))
                {
                    CloseCustomerDeliveryModal();
                    RefreshCustomerDeliveryDetail(idTaiLieu);
                    ShowNotify(
                        "Yêu cầu gửi khách hàng này đã được xử lý. Danh sách đã được làm mới.",
                        MSGType.Success);
                    return;
                }

                isSubmissionReserved = true;
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.CustomerDelivery);
                CreateRequestDocumentManager().SendDocumentToCustomer(
                    idTaiLieu,
                    idPhienBanTaiLieu,
                    idKhachHang,
                    txtCustomerDeliveryRecipient.Text,
                    txtCustomerDeliveryEmail.Text,
                    channelValue,
                    dtCustomerDeliveryDeadline.DateValue,
                    chkCustomerDeliveryBeforeSigning.Checked,
                    txtCustomerDeliveryNote.Text);
                hasCreatedCustomerDelivery = true;
                RefreshCustomerDeliveryDetail(idTaiLieu);
                CloseCustomerDeliveryModal();
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                if (isSubmissionReserved && !hasCreatedCustomerDelivery)
                    ReleaseCustomerDeliverySubmission(submissionToken);
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                if (isSubmissionReserved && !hasCreatedCustomerDelivery)
                    ReleaseCustomerDeliverySubmission(submissionToken);
                LogCustomerDeliveryHandlerError("send", idTaiLieu, exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelCustomerDelivery_Click(
            object sender,
            EventArgs e)
        {
            mdlCustomerDelivery.CloseModal(true);
            KeepCustomerTabOpen();
        }

        protected void rptCustomer_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (!string.Equals(
                    e.CommandName,
                    "UPDATE_CUSTOMER_DELIVERY",
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
            Guid idGuiNhanKhachHang;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || !Guid.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idGuiNhanKhachHang)
                || idTaiLieu == Guid.Empty
                || idGuiNhanKhachHang == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.CustomerDelivery);
                DataTable delivery = CreateRequestDocumentManager()
                    .GetCustomerDeliveryDetail(
                        idTaiLieu,
                        idGuiNhanKhachHang);
                if (delivery.Rows.Count == 0)
                {
                    ShowNotify(
                        "Lần gửi khách hàng đã thay đổi hoặc không còn tồn tại.",
                        MSGType.Warning);
                    return;
                }

                BindCustomerDeliveryDropdowns();
                DataRow item = delivery.Rows[0];
                string status = Convert.ToString(item["TrangThai"]);
                if (status != DocumentCustomerStatusKeys.Sent
                    && status != DocumentCustomerStatusKeys.WaitingForReturn
                    && status != DocumentCustomerStatusKeys.ReceivedBack)
                {
                    status = DocumentCustomerStatusKeys.Sent;
                }

                hdfCustomerDeliveryStatusDocumentId.Value =
                    idTaiLieu.ToString();
                hdfCustomerDeliveryStatusId.Value =
                    idGuiNhanKhachHang.ToString();
                hdfCustomerDeliveryStatus.Value = status;
                ddlCustomerDeliveryStatus.SelectedValue = status;
                txtCustomerDeliveryStatusNote.Text = Convert.ToString(
                    item["GhiChu"]);
                lblCustomerDeliveryStatusVersion.Text = "v"
                    + GetValueText(item["SoPhienBan"]);
                lblCustomerDeliveryStatusCustomer.Text = JoinNonEmpty(
                    GetValueText(item["TenKhachHang"]),
                    GetRecipientText(
                        item["TenNguoiNhan"],
                        item["EmailNguoiNhan"]));
                mdlCustomerDeliveryStatus.Title = GetResourceText(
                    BackEndResourceKeys.UPDATE_CUSTOMER_DELIVERY);
                mdlCustomerDeliveryStatus.OpenModal(true);
                KeepCustomerTabOpen();
            }
            catch (Exception exc)
            {
                LogCustomerDeliveryHandlerError(
                    "open status",
                    idTaiLieu,
                    exc);
                ShowNotify(exc.Message, MSGType.Warning);
            }
        }

        protected void btnUpdateCustomerDeliveryStatus_Click(
            object sender,
            EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idGuiNhanKhachHang;
            string status = GetPostedDropdownValue(
                hdfCustomerDeliveryStatus,
                ddlCustomerDeliveryStatus);
            if (!Guid.TryParse(
                    hdfCustomerDeliveryStatusDocumentId.Value,
                    out idTaiLieu)
                || !Guid.TryParse(
                    hdfCustomerDeliveryStatusId.Value,
                    out idGuiNhanKhachHang)
                || idTaiLieu == Guid.Empty
                || idGuiNhanKhachHang == Guid.Empty
                || string.IsNullOrWhiteSpace(status))
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.CustomerDelivery);
                CreateRequestDocumentManager().UpdateCustomerDeliveryStatus(
                    idTaiLieu,
                    idGuiNhanKhachHang,
                    status,
                    txtCustomerDeliveryStatusNote.Text);
                mdlCustomerDeliveryStatus.CloseModal(true);
                RefreshCustomerDeliveryDetail(idTaiLieu);
                ShowSuccessSaveData();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                LogCustomerDeliveryHandlerError(
                    "update status",
                    idTaiLieu,
                    exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelCustomerDeliveryStatus_Click(
            object sender,
            EventArgs e)
        {
            mdlCustomerDeliveryStatus.CloseModal(true);
            KeepCustomerTabOpen();
        }

        private string GetPostedDropdownValue(
            System.Web.UI.WebControls.HiddenField hiddenField,
            SweetSoft.QLDA.Controls.ExtraDropdown dropdown)
        {
            string value = hiddenField == null
                ? null
                : Request.Form[hiddenField.UniqueID];
            if (string.IsNullOrWhiteSpace(value) && dropdown != null)
            {
                value = Request.Form[
                    dropdown.UniqueID + dropdown.HdfValue];
            }
            if (string.IsNullOrWhiteSpace(value) && dropdown != null)
                value = dropdown.SelectedValue;

            return value;
        }

        private void RefreshCustomerDeliveryDetail(Guid idTaiLieu)
        {
            InitControls(idTaiLieu);
            upDetail.Update();
            KeepCustomerTabOpen();
        }

        private static bool TryReserveCustomerDeliverySubmission(
            Guid submissionToken)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                return true;

            string sessionKey = CustomerDeliverySubmissionSessionKeyPrefix
                + submissionToken.ToString("N");
            if (context.Session[sessionKey] != null)
                return false;

            // A modal receives one token when it opens.  Keep that token in the
            // server session after a successful request so a repeated postback
            // from the same click cannot create a second delivery record.
            context.Session[sessionKey] = DateTime.UtcNow;
            return true;
        }

        private static void ReleaseCustomerDeliverySubmission(
            Guid submissionToken)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                return;

            context.Session.Remove(
                CustomerDeliverySubmissionSessionKeyPrefix
                + submissionToken.ToString("N"));
        }

        private void CloseCustomerDeliveryModal()
        {
            if (mdlCustomerDelivery == null)
                return;

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "CloseDocumentCustomerDeliveryModal",
                "CMSMasterJs.CloseDialog('#" + mdlCustomerDelivery.ClientID
                    + "');",
                true);
        }

        private void KeepCustomerTabOpen()
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                GetType(),
                "KeepDocumentCustomerTabOpen",
                "var tabElement=document.querySelector('[data-bs-target=\"#document-customer\"]');"
                + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                true);
        }

        private static void LogCustomerDeliveryHandlerError(
            string operation,
            Guid idTaiLieu,
            Exception exception)
        {
            SysLogger.LogError(
                exception,
                "Document customer delivery {0} handler failed for document {1}",
                operation,
                idTaiLieu);
        }

        protected void btnOpenPhysicalStorage_Click(
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
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.PhysicalStorage);
                DataTable detail = GetDocumentDetail(idTaiLieu);
                if (detail.Rows.Count == 0
                    || !GetBoolean(detail.Rows[0], "CanLuuVatLy"))
                {
                    ShowNotify(
                        "Hồ sơ này chưa được cấu hình lưu bản cứng.",
                        MSGType.Warning);
                    return;
                }

                BindPhysicalStorageLocations();
                if (ddlPhysicalStorageLocation.Items.Count <= 1)
                {
                    ShowNotify(
                        "Chưa có nơi lưu trữ đang hoạt động để chọn.",
                        MSGType.Warning);
                    return;
                }

                hdfPhysicalStorageDocumentId.Value = idTaiLieu.ToString();
                hdfPhysicalStorageLocation.Value = string.Empty;
                ddlPhysicalStorageLocation.SelectedValue = string.Empty;
                UpdatePhysicalStorageLocationPath(string.Empty);
                // Suggest a type default only before any storage history exists.
                // Updating the type never relocates an already stored dossier.
                if (CreateRequestDocumentManager().GetPhysicalStorageHistory(idTaiLieu).Rows.Count == 0)
                {
                    Guid? defaultLocation = DocumentTypeManager.Instance.GetDefaultStorageLocation(
                        (Guid)detail.Rows[0]["IdLoaiTaiLieu"]);
                    string value = defaultLocation?.ToString() ?? string.Empty;
                    if (ddlPhysicalStorageLocation.Items.FindByValue(value) != null)
                    {
                        ddlPhysicalStorageLocation.SelectedValue = value;
                        hdfPhysicalStorageLocation.Value = value;
                        UpdatePhysicalStorageLocationPath(value);
                    }
                }
                chkPhysicalStorageManualCode.Checked = false;
                txtPhysicalStorageCode.Text = string.Empty;
                txtPhysicalStorageOriginalCondition.Text = string.Empty;
                txtPhysicalStorageNote.Text = string.Empty;
                mdlPhysicalStorage.Title = GetResourceText(
                    BackEndResourceKeys.STORE_PHYSICAL_COPY);
                mdlPhysicalStorage.OpenModal(true);
                KeepPhysicalStorageTabOpen();
            }
            catch (Exception exc)
            {
                LogPhysicalStorageHandlerError("open", idTaiLieu, exc);
                ShowNotify(exc.Message, MSGType.Warning);
            }
        }

        protected void btnPhysicalStorageSave_Click(
            object sender,
            EventArgs e)
        {
            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            Guid idNoiLuuTru;
            string storageLocationValue = GetPostedDropdownValue(
                hdfPhysicalStorageLocation,
                ddlPhysicalStorageLocation);
            if (!Guid.TryParse(
                    hdfPhysicalStorageDocumentId.Value,
                    out idTaiLieu)
                || !Guid.TryParse(storageLocationValue, out idNoiLuuTru)
                || idTaiLieu == Guid.Empty
                || idNoiLuuTru == Guid.Empty)
            {
                ShowNotify(
                    "Vui lòng chọn nơi lưu trữ.",
                    MSGType.Warning);
                return;
            }

            try
            {
                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.PhysicalStorage);
                DocumentPhysicalStorageOperationResult result =
                    CreateRequestDocumentManager().StoreDocumentPhysicalCopy(
                        idTaiLieu,
                        idNoiLuuTru,
                        chkPhysicalStorageManualCode.Checked,
                        txtPhysicalStorageCode.Text,
                        txtPhysicalStorageOriginalCondition.Text,
                        txtPhysicalStorageNote.Text);
                mdlPhysicalStorage.CloseModal(true);
                RefreshPhysicalStorageDetail(idTaiLieu);
                ShowNotify(
                    string.Format(
                        GetResourceText(
                            BackEndResourceKeys.PHYSICAL_STORAGE_SAVED_MESSAGE),
                        result.MaLuuTru),
                    MSGType.Success);
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                LogPhysicalStorageHandlerError("save", idTaiLieu, exc);
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnCancelPhysicalStorage_Click(
            object sender,
            EventArgs e)
        {
            mdlPhysicalStorage.CloseModal(true);
            KeepPhysicalStorageTabOpen();
        }

        private void RefreshPhysicalStorageDetail(Guid idTaiLieu)
        {
            InitControls(idTaiLieu);
            upDetail.Update();
            KeepPhysicalStorageTabOpen();
        }

        private void KeepPhysicalStorageTabOpen()
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                GetType(),
                "KeepDocumentPhysicalStorageTabOpen",
                "var tabElement=document.querySelector('[data-bs-target=\"#document-storage\"]');"
                + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                true);
        }

        private static void LogPhysicalStorageHandlerError(
            string operation,
            Guid idTaiLieu,
            Exception exception)
        {
            SysLogger.LogError(
                exception,
                "Document physical storage {0} handler failed for document {1}",
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
            bool viewVersionFiles = string.Equals(
                e.CommandName,
                ViewVersionFilesCommand,
                StringComparison.Ordinal);
            bool restoreVersion = string.Equals(
                e.CommandName,
                RestoreVersionCommand,
                StringComparison.Ordinal);
            if (!setOfficial && !clearOfficial
                && !viewVersionFiles && !restoreVersion)
                return;

            if (!CURRENT_PAGE.IsEdit && !viewVersionFiles)
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
                if (viewVersionFiles)
                {
                    EnsureDocumentActionAccess(idTaiLieu, ActionKeys.View);
                    DataTable fileRows = GetVersionFileRows(
                        idTaiLieu,
                        idPhienBanTaiLieu);
                    BindVersionFileModal(fileRows, idPhienBanTaiLieu);
                    mdlVersionFiles.OpenModal(true);
                    KeepVersionsTabOpen();
                    return;
                }

                EnsureDocumentActionAccess(idTaiLieu, DocumentPermissionKeys.ManageFiles);
                if (restoreVersion)
                {
                    DocumentFileSet restored = DocumentManager.Instance
                        .RestoreDocumentFileSet(idTaiLieu, idPhienBanTaiLieu);
                    InitControls(idTaiLieu);
                    upDetail.Update();
                    KeepVersionsTabOpen();
                    ShowNotify(
                        restored.Created
                            ? "Đã khôi phục mốc lịch sử thành phiên bản hiện tại."
                            : "Bộ file hiện tại đã giống mốc lịch sử này.",
                        MSGType.Success);
                    return;
                }

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

        private void BindVersionFileModal(
            DataTable fileRows,
            Guid versionId)
        {
            bool hasFiles = fileRows != null
                && fileRows.AsEnumerable().Any(row =>
                    Convert.ToInt32(row["FileCount"]) > 0
                    && HasValue(row["IdFile"]));
            pnlVersionFiles.Visible = hasFiles;
            pnlNoVersionFiles.Visible = !hasFiles;
            rptVersionFiles.DataSource = fileRows;
            rptVersionFiles.DataBind();

            DataRow first = fileRows == null
                ? null
                : fileRows.AsEnumerable().FirstOrDefault();
            string versionNumber = first == null
                ? versionId.ToString("D")
                : GetValueText(first["SoPhienBan"]);
            int fileCount = first == null
                ? 0
                : Convert.ToInt32(first["FileCount"]);
            lblVersionFilesSummary.Text = "Mốc v" + versionNumber
                + " · " + GetVersionFileSummary(fileCount);
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
            string content = DocumentManager.Instance.GetDocumentContent(
                (Guid)document["IdTaiLieu"]);
            litDocumentContent.Text = string.IsNullOrWhiteSpace(content)
                ? "<span class='text-muted'>Chưa có nội dung soạn trực tiếp.</span>"
                : content;
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

        private static DataTable BuildVersionHistory(DataTable versions)
        {
            if (versions == null)
                return new DataTable();

            DataTable history = versions.Clone();
            HashSet<string> versionIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in versions.Rows)
            {
                string versionId = Convert.ToString(row["IdPhienBanTaiLieu"]);
                if (string.IsNullOrWhiteSpace(versionId)
                    || versionIds.Add(versionId))
                {
                    history.ImportRow(row);
                }
            }

            return history;
        }

        private DataTable GetVersionFileRows(
            Guid idTaiLieu,
            Guid idPhienBanTaiLieu)
        {
            DataTable allRows = DocumentManager.Instance
                .GetDocumentVersions(idTaiLieu);
            DataTable selectedRows = allRows.Clone();
            foreach (DataRow row in allRows.Rows)
            {
                if (string.Equals(
                        Convert.ToString(row["IdPhienBanTaiLieu"]),
                        idPhienBanTaiLieu.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    selectedRows.ImportRow(row);
                }
            }

            return selectedRows;
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
            Guid idTaiLieu;
            return CURRENT_PAGE.IsEdit
                && IsPendingSigningStatus(value)
                && Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                && idTaiLieu != Guid.Empty
                && DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    DocumentPermissionKeys.Signing);
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
                    if (Convert.ToInt32(row["FileCount"]) != 1) return null;
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

        protected bool CanManageCustomerDelivery()
        {
            Guid idTaiLieu;
            return CURRENT_PAGE.IsEdit
                && Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                && idTaiLieu != Guid.Empty
                && DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    DocumentPermissionKeys.CustomerDelivery);
        }

        protected string GetCustomerStatusCss(object statusValue)
        {
            string status = Convert.ToString(statusValue);
            if (status == DocumentCustomerStatusKeys.Sent)
                return "badge bg-primary";
            if (status == DocumentCustomerStatusKeys.WaitingForReturn)
                return "badge bg-warning text-dark";
            if (status == DocumentCustomerStatusKeys.ReceivedBack)
                return "badge bg-success";
            return "badge bg-secondary";
        }

        protected string GetCustomerDeliveryChannelText(object value)
        {
            string channel = Convert.ToString(value);
            if (channel == DocumentCustomerDeliveryChannelKeys.Email)
            {
                return GetResourceText(
                    BackEndResourceKeys.CUSTOMER_DELIVERY_CHANNEL_EMAIL);
            }
            if (channel == DocumentCustomerDeliveryChannelKeys.Direct)
            {
                return GetResourceText(
                    BackEndResourceKeys.CUSTOMER_DELIVERY_CHANNEL_DIRECT);
            }
            if (channel == DocumentCustomerDeliveryChannelKeys.Other)
                return GetResourceText(BackEndResourceKeys.OTHER);
            return GetValueText(value);
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

        protected string GetPermissionInitial(object displayNameValue)
        {
            string displayName = Convert.ToString(displayNameValue).Trim();
            if (string.IsNullOrWhiteSpace(displayName))
                return "?";

            string[] parts = displayName
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpperInvariant();

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1))
                .ToUpperInvariant();
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

            if (string.Equals(
                    source,
                    "RESTORE",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Khôi phục";
            }

            return GetValueText(value);
        }

        protected string GetVersionFileSummary(object value)
        {
            int count;
            if (!int.TryParse(Convert.ToString(value), out count) || count <= 0)
                return "Mốc này không có file";
            return count + " file trong bộ hồ sơ";
        }

        protected bool CanRestoreVersion(object isCurrentValue)
        {
            Guid idTaiLieu;
            return CURRENT_PAGE.IsEdit
                && !Convert.ToBoolean(isCurrentValue)
                && Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                && idTaiLieu != Guid.Empty
                && DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    DocumentPermissionKeys.ManageFiles);
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
                && CanManageFiles()
                && !RequiresSigning
                && !IsOfficialVersion(fileIdValue)
                && CanOpenFile(fileUrlValue);
        }

        protected bool CanClearOfficialFile(object fileIdValue)
        {
            return CURRENT_PAGE.IsEdit
                && CanManageFiles()
                && !RequiresSigning
                && IsOfficialVersion(fileIdValue);
        }

        private bool CanManageFiles()
        {
            Guid idTaiLieu;
            return Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                && idTaiLieu != Guid.Empty
                && DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    DocumentPermissionKeys.ManageFiles);
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
            if (activityType == DocumentActivityTypeKeys.RestoreVersion)
            {
                return "Khôi phục file hồ sơ";
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
            if (activityType == DocumentActivityTypeKeys.SendCustomerDelivery)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_SEND_CUSTOMER_DELIVERY);
            }
            if (activityType == DocumentActivityTypeKeys.UpdateCustomerDelivery)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_UPDATE_CUSTOMER_DELIVERY);
            }
            if (activityType == DocumentActivityTypeKeys.StorePhysicalCopy)
            {
                return GetResourceText(
                    BackEndResourceKeys.ACTIVITY_STORE_PHYSICAL_COPY);
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
            if (referenceType
                == DocumentActivityReferenceKeys.CustomerDelivery)
            {
                return GetResourceText(
                    BackEndResourceKeys.CUSTOMER_DELIVERY_HISTORY);
            }
            if (referenceType
                == DocumentActivityReferenceKeys.PhysicalStorage)
            {
                return GetResourceText(
                    BackEndResourceKeys.PHYSICAL_STORAGE_HISTORY);
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
