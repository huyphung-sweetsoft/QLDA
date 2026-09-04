using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.Hosting;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentDetail : BaseAdminUserControl
    {
        private const string DocumentVersionSavedCallbackKey =
            "DocumentDetailVersionSaved";
        private const string DocumentVersionBeforeSaveCallbackKey =
            "DocumentDetailVersionBeforeSave";

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
            if (!isBeforeSave && !isAfterSave)
            {
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || idTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            if (!CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            try
            {
                if (isBeforeSave)
                {
                    List<Guid> removedFileIds =
                        fbVersions.GetPendingRemovedFileIds();
                    DocumentManager.Instance
                        .PrepareDocumentVersionFilesForDeletion(
                            idTaiLieu,
                            removedFileIds);
                    return;
                }

                DocumentManager.Instance.SyncDocumentVersions(idTaiLieu);
                InitControls(idTaiLieu);
                upDetail.Update();
                ScriptManager.RegisterStartupScript(
                    this.Page,
                    GetType(),
                    "KeepDocumentVersionsTabOpen",
                    "var tabElement=document.querySelector('[data-bs-target=\"#document-versions\"]');"
                    + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                    true);
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

            return method == DocumentSigningMethodKeys.DigitalExternal
                ? GetResourceText(
                    BackEndResourceKeys.EXTERNAL_DIGITAL_SIGNING)
                : GetResourceText(BackEndResourceKeys.PAPER_SIGNING);
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
            return string.IsNullOrWhiteSpace(description)
                ? GetValueText(changeValue)
                : description;
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
