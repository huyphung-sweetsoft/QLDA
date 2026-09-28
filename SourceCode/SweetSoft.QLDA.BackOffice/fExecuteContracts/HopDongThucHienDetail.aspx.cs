using Mammoth;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fExecuteContracts
{
    public static class LoaiNoiDungHopDong
    {
        public const string SOAN_THAO = "SOAN_THAO";
        public const string TAI_FILE = "TAI_FILE";
    }

    public partial class HopDongThucHienDetail : BaseAdminPage
    {
        private const string ContractFileBeforeSaveCallbackKey = "HopDongThucHienBeforeSave";
        private const string ContractFileSavedCallbackKey = "HopDongThucHienFileSaved";
        private const string ContractClearContentConfirmCommand = "CONTRACT_CLEAR_CONTENT";

        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.Contract; }
        }

        public Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("ContractId");

                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;

                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        private Guid TempContractFileRefId
        {
            get
            {
                if (ViewState["TempContractFileRefId"] == null)
                    ViewState["TempContractFileRefId"] = Guid.NewGuid();

                return (Guid)ViewState["TempContractFileRefId"];
            }
        }

        private bool ClearContractContent
        {
            get { return ViewState["ClearContractContent"] != null && (bool)ViewState["ClearContractContent"]; }
            set { ViewState["ClearContractContent"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CONTRACT_LIST));

                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.CONTRACT_DETAIL);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Contracts, GetResourceText(BackEndResourceKeys.CONTRACT_LIST) }
                };

                ApplyControlsText();
                lbtExportPdf.Visible = this.QueryId != Guid.Empty;
                InitContractFileUploader();

                if (this.QueryId == Guid.Empty)
                {
                    if (!this.IsAdd)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    RefreshHopDongInfo();
                }
                else
                {
                    LoadHopDong(this.QueryId);
                }
            }
        }

        protected void lbtExportPdf_Click(object sender, EventArgs e)
        {
            ExportContractPdf();
        }

        public void HandleFileCallback(string key)
        {
            if (key == ContractFileBeforeSaveCallbackKey)
                return;

            if (key != ContractFileSavedCallbackKey)
                return;

            Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
            fbHopDong.LoadFile(refId, FileUploadTypes.ProjectContract);
            BindContractFiles(refId);

            TblUploadFile file = GetContractFile(refId);

            if (file == null)
            {
                if (!string.IsNullOrWhiteSpace(txtNoiDungHopDong.Text))
                {
                    ConfirmResult result = new ConfirmResult
                    {
                        CommandName = ContractClearContentConfirmCommand,
                        Value = null
                    };

                    this.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        "File hợp đồng đã bị xóa. Bạn có muốn xóa luôn nội dung soạn thảo hiện tại không?",
                        MSGButton.DeleteCancel,
                        MSGIcon.Warning
                    );

                    OpenMessageBox(msg, result, false, false);
                }

                return;
            }

            if (IsDocxFile(file))
            {
                try
                {
                    string html = ConvertDocxToHtml(file);
                    txtNoiDungHopDong.Text = html;
                    SetNoiDungHopDongToEditor(html);
                }
                catch (Exception ex)
                {
                    ShowNotify(
                        "Không thể đọc nội dung file DOCX: " + ex.Message,
                        MSGType.Error
                    );
                }
            }
            else if (IsPdfFile(file) && !string.IsNullOrWhiteSpace(txtNoiDungHopDong.Text))
            {
                ConfirmResult result = new ConfirmResult
                {
                    CommandName = ContractClearContentConfirmCommand,
                    Value = null
                };

                this.CurrentConfirmResult = result;

                MessageBox msg = new MessageBox(
                    GetResourceText(BackEndResourceKeys.NOTIFICATION),
                    "File PDF mới đã được tải lên. Bạn có muốn xóa nội dung soạn thảo hiện tại không?",
                    MSGButton.DeleteCancel,
                    MSGIcon.Warning
                );

                OpenMessageBox(msg, result, false, false);
            }
        }

        public override void DataCallback(string key, object value, object valueText)
        {
            HandleFileCallback(key);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e == null || !e.Submit || string.IsNullOrEmpty(e.CommandName))
                return;

            if (e.CommandName == ContractClearContentConfirmCommand)
            {
                ClearContractContent = true;
                txtNoiDungHopDong.Text = string.Empty;
                SetNoiDungHopDongToEditor(string.Empty);
            }
        }

        private void InitContractFileUploader()
        {
            fbHopDong.IsMultiple = false;
            fbHopDong.IsEnabled = this.IsAdd || this.IsEdit;
            fbHopDong.SingleFilePathType = FileTypes.Internal;
            fbHopDong.BeforeSaveDataCallbackKey = ContractFileBeforeSaveCallbackKey;
            fbHopDong.SaveDataCallbackKey = ContractFileSavedCallbackKey;

            fbHopDong.LoadFile(
                this.QueryId == Guid.Empty ? TempContractFileRefId : this.QueryId,
                FileUploadTypes.ProjectContract);
        }

        private void ApplyControlsText()
        {
            ddlKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            txtSoHopDong.PlaceHolder = txtTenHopDong.PlaceHolder = txtGiaTriHopDong.PlaceHolder = txtMoTa.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
        }

        private void RefreshHopDongInfo()
        {
            new ControlHelpers().BindKhachHang(ddlKhachHang);

            txtSoHopDong.Text = txtTenHopDong.Text = txtGiaTriHopDong.Text = txtNgayKy.Text =
                txtNgayHieuLuc.Text = txtNgayHetHan.Text = txtMoTa.Text = string.Empty;

            ddlKhachHang.SelectedIndex = 0;

            txtSoHopDong.Enabled = true;
            txtTenHopDong.Enabled = true;

            txtNoiDungHopDong.Text = string.Empty;
            ClearContractContent = false;
        }

        private void LoadHopDong(Guid idHopDongThucHien)
        {
            RefreshHopDongInfo();

            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(idHopDongThucHien);

            if (hopDong == null || hopDong.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            txtSoHopDong.Text = hopDong.SoHopDong;
            txtTenHopDong.Text = hopDong.TenHopDong;

            ddlKhachHang.SelectedValue = hopDong.IdKhachHang.ToString();

            txtGiaTriHopDong.Text = hopDong.GiaTriHopDong.HasValue
                ? hopDong.GiaTriHopDong.Value.ToString()
                : string.Empty;

            txtNgayKy.Text = hopDong.NgayKy.HasValue
                ? hopDong.NgayKy.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            txtNgayHieuLuc.Text = hopDong.NgayHieuLuc.HasValue
                ? hopDong.NgayHieuLuc.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            txtNgayHetHan.Text = hopDong.NgayHetHan.HasValue
                ? hopDong.NgayHetHan.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            txtMoTa.Text = hopDong.MoTa;
            txtNoiDungHopDong.Text = hopDong.NoiDungHopDong;


            bool hasLinkedDocument = HopDongThucHienManager.Instance.HasLinkedDocument(hopDong.IdHopDongThucHien);

            txtSoHopDong.Enabled = !hasLinkedDocument;
            txtTenHopDong.Enabled = !hasLinkedDocument;
            pnlContractDocumentIdentityLocked.Visible = hasLinkedDocument;

            fbHopDong.LoadFile(hopDong.IdHopDongThucHien, FileUploadTypes.ProjectContract);
            BindContractFiles(hopDong.IdHopDongThucHien);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);

                validationEngine.CheckValidControls(pnlContract.Controls);

                if (string.IsNullOrWhiteSpace(txtNgayKy.Text))
                {
                    validationEngine.AddErrorPrompt(
                        txtNgayKy.ClientID,
                        GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }

                Guid idKhachHang = Guid.Empty;

                if (!this.GetValue(ddlKhachHang, out idKhachHang) || idKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(
                        ddlKhachHang.ClientID,
                        GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }

                decimal giaTriHopDong;

                if (!decimal.TryParse(txtGiaTriHopDong.Text, out giaTriHopDong) || giaTriHopDong <= 0)
                {
                    validationEngine.AddErrorPrompt(
                        txtGiaTriHopDong.ClientID,
                        "Giá trị hợp đồng phải lớn hơn 0.");
                }

                DateTime ngayKy;

                if (!DateTime.TryParse(txtNgayKy.Text, out ngayKy))
                {
                    validationEngine.AddErrorPrompt(
                        txtNgayKy.ClientID,
                        "Ngày ký không hợp lệ.");
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                bool isAdd = this.QueryId == Guid.Empty;

                if (isAdd && !this.IsAdd)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                if (!isAdd && !this.IsEdit)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblHopDongThucHien hopDong = new TblHopDongThucHien();

                if (!isAdd)
                    hopDong.IdHopDongThucHien = this.QueryId;

                hopDong.SoHopDong = txtSoHopDong.Text.Trim();
                hopDong.TenHopDong = txtTenHopDong.Text.Trim();
                hopDong.IdKhachHang = idKhachHang;
                hopDong.GiaTriHopDong = giaTriHopDong;
                hopDong.NgayKy = ngayKy;

                DateTime tempDate;

                if (DateTime.TryParse(txtNgayHieuLuc.Text, out tempDate))
                    hopDong.NgayHieuLuc = tempDate;
                else
                    hopDong.NgayHieuLuc = null;

                if (DateTime.TryParse(txtNgayHetHan.Text, out tempDate))
                    hopDong.NgayHetHan = tempDate;
                else
                    hopDong.NgayHetHan = null;

                hopDong.MoTa = string.IsNullOrWhiteSpace(txtMoTa.Text) ? null : txtMoTa.Text.Trim();

                if (ClearContractContent)
                {
                    hopDong.NoiDungHopDong = null;
                }
                else if (hopDong.LoaiNoiDungHopDong == LoaiNoiDungHopDong.SOAN_THAO)
                {
                    hopDong.NoiDungHopDong = txtNoiDungHopDong.Text;
                }
                else
                {
                    TblUploadFile file = GetContractFile(
                        this.QueryId == Guid.Empty ? TempContractFileRefId : this.QueryId);

                    if (IsDocxFile(file))
                        hopDong.NoiDungHopDong = txtNoiDungHopDong.Text;
                    else if (IsPdfFile(file))
                        hopDong.NoiDungHopDong = txtNoiDungHopDong.Text;
                    else
                        hopDong.NoiDungHopDong = null;
                }

                hopDong = HopDongThucHienManager.Instance.CreateOrUpdate(hopDong);

                if (hopDong == null || hopDong.IdHopDongThucHien == Guid.Empty)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (isAdd)
                    LinkContractFiles(TempContractFileRefId, hopDong.IdHopDongThucHien);

                ClearContractContent = false;

                if (isAdd)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    ShowSuccessSaveData();
                }

                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Contracts), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void lbtCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Contracts), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void LinkContractFiles(Guid tempRefId, Guid contractId)
        {
            if (tempRefId == Guid.Empty || contractId == Guid.Empty)
                return;

            new SubSonic.Update(TblUploadFile.Schema)
                .Set(TblUploadFile.Columns.RefId).EqualTo(contractId)
                .Where(TblUploadFile.Columns.RefId).IsEqualTo(tempRefId)
                .And(TblUploadFile.Columns.RefType).IsEqualTo(FileUploadTypes.ProjectContract.ToString())
                .Execute();
        }

        private TblUploadFile GetContractFile(Guid refId)
        {
            if (refId == Guid.Empty)
                return null;

            return UploadManager.Instance.GetUploadFileByRefIdAndRefType(
                refId,
                FileUploadTypes.ProjectContract);
        }
        private List<TblUploadFile> GetContractFiles(Guid refId)
        {
            if (refId == Guid.Empty)
                return new List<TblUploadFile>();

            UploadManager fileManager = new UploadManager(
                SweetContext.Current,
                refId,
                FileUploadTypes.ProjectContract);

            return fileManager.TblUploadFiles ?? new List<TblUploadFile>();
        }

        private void BindContractFiles(Guid refId)
        {
            List<TblUploadFile> files = GetContractFiles(refId);

            rptContractFiles.DataSource = files;
            rptContractFiles.DataBind();

            pnlNoContractFiles.Visible = files.Count == 0;
        }

        protected string GetContractFileIcon(object value)
        {
            string ext = value == null ? string.Empty : value.ToString().ToLowerInvariant();

            switch (ext)
            {
                case ".pdf":
                    return "fas fa-file-pdf text-danger";
                case ".doc":
                case ".docx":
                    return "fas fa-file-word text-primary";
                case ".xls":
                case ".xlsx":
                    return "fas fa-file-excel text-success";
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return "fas fa-file-image text-info";
                default:
                    return "fas fa-file text-secondary";
            }
        }

        protected bool IsContractFileEditable(object value)
        {
            string ext = value == null ? string.Empty : value.ToString();

            return string.Equals(ext, ".docx", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase);
        }

        protected string FormatFileSize(object value)
        {
            if (value == null || value == DBNull.Value)
                return "-";

            long size;

            if (!long.TryParse(value.ToString(), out size))
                return "-";

            if (size < 1024)
                return size + " B";

            if (size < 1024 * 1024)
                return (size / 1024.0).ToString("0.##") + " KB";

            return (size / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }

        private string GetContractFilePhysicalPath(TblUploadFile file)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FileUrl))
                return null;

            string physicalPath = HostingEnvironment.MapPath(file.FileUrl);

            return string.IsNullOrWhiteSpace(physicalPath) ? null : physicalPath;
        }

        private bool IsDocxFile(TblUploadFile file)
        {
            if (file == null)
                return false;

            string ext = file.Ext;

            if (string.IsNullOrWhiteSpace(ext))
                ext = Path.GetExtension(file.OriginalFileName);

            return string.Equals(ext, ".docx", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPdfFile(TblUploadFile file)
        {
            if (file == null)
                return false;

            string ext = file.Ext;

            if (string.IsNullOrWhiteSpace(ext))
                ext = Path.GetExtension(file.OriginalFileName);

            return string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private string ConvertDocxToHtml(TblUploadFile file)
        {
            string physicalPath = GetContractFilePhysicalPath(file);

            if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
            {
                throw new FileNotFoundException("Không tìm thấy file DOCX.", physicalPath);
            }

            var converter = new DocumentConverter();
            var result = converter.ConvertToHtml(physicalPath);

            return result.Value;
        }

        private void SetNoiDungHopDongToEditor(string html)
        {
            string encodedHtml = HttpUtility.JavaScriptStringEncode(html ?? string.Empty);

            string script = string.Format(
                "setTimeout(function() {{ if (typeof CKEDITOR !== 'undefined' && CKEDITOR.instances['{0}']) {{ CKEDITOR.instances['{0}'].setData('{1}'); }} }}, 100);",
                txtNoiDungHopDong.ClientID,
                encodedHtml);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "SetContractContent",
                script,
                true);
        }

        private TblUploadFile GenerateContractPdf(Guid contractId, string htmlContent)
        {
            if (contractId == Guid.Empty)
                throw new Exception("Hợp đồng chưa được tạo.");

            if (string.IsNullOrWhiteSpace(htmlContent))
                throw new Exception("Hợp đồng chưa có nội dung để xuất PDF.");

            DeleteGeneratedContractPdf(contractId);

            byte[] pdfBytes = PdfManager.Instance.GeneratePdf(htmlContent);
            string fileName = string.Format("HopDongThucHien_{0}.pdf", Guid.NewGuid().ToString("N"));
            string refType = FileUploadTypes.ProjectContract.ToString();
            string relativeDirectory = string.Format("/Uploads/{0}/{1:yyyy/MM}/", refType, DateTime.Now);
            string relativePath = relativeDirectory + fileName;
            string physicalDirectory = HostingEnvironment.MapPath(relativeDirectory);
            string physicalPath = HostingEnvironment.MapPath(relativePath);

            if (string.IsNullOrEmpty(physicalDirectory) || string.IsNullOrEmpty(physicalPath))
                throw new Exception("Không xác định được đường dẫn lưu file PDF.");

            if (!Directory.Exists(physicalDirectory))
                Directory.CreateDirectory(physicalDirectory);

            using (FileStream stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                stream.Write(pdfBytes, 0, pdfBytes.Length);
            }

            var file = new TblUploadFile
            {
                CreatedDate = DateTime.UtcNow,
                OwnerId = SweetContext.Current.UserId,
                IsDeleted = false,
                Name = fileName,
                FileUrl = relativePath,
                FileType = FileTypes.Internal,
                Ext = ".pdf",
                RefId = contractId,
                RefType = refType,
                DisplayOrder = 0,
                FileSize = pdfBytes.Length,
                MimeType = "application/pdf",
                OriginalFileName = fileName,
                IsHost = true,
                IsSecretary = true,
                IsParticipant = true
            };

            return UploadManager.Instance.Create(file);
        }

        private void DeleteGeneratedContractPdf(Guid contractId)
        {
            TblUploadFile file = UploadManager.Instance.GetUploadFileByRefIdAndRefType(contractId, FileUploadTypes.ProjectContract);

            if (file == null)
                return;

            UploadManager.Instance.RemoveFiles(new List<Guid> { file.Id }, FileUploadTypes.ProjectContract);
        }

        private void ExportContractPdf()
        {
            if (QueryId == Guid.Empty)
            {
                ShowNotify("Hợp đồng chưa được tạo.", MSGType.Warning);
                return;
            }

            string htmlContent = txtNoiDungHopDong.Text;

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                ShowNotify("Hợp đồng chưa có nội dung để xuất PDF.", MSGType.Warning);
                return;
            }

            try
            {
                TblUploadFile file = GenerateContractPdf(QueryId, htmlContent);

                string physicalPath = HostingEnvironment.MapPath(file.FileUrl);

                if (string.IsNullOrEmpty(physicalPath) || !File.Exists(physicalPath))
                    throw new Exception("Không tìm thấy file PDF vừa tạo.");

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment;filename=" + file.OriginalFileName);
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.TransmitFile(physicalPath);
                Response.End();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        protected void rptContractFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Guid fileId;

            if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out fileId) || fileId == Guid.Empty)
                return;

            UploadManager fileManager = new UploadManager(SweetContext.Current, fileId);
            TblUploadFile file = fileManager.File;

            if (file == null || file.IsDeleted)
            {
                ShowNotify("Không tìm thấy file.", MSGType.Warning);
                return;
            }

            if (file.RefType != FileUploadTypes.ProjectContract.ToString())
            {
                ShowNotify("File không thuộc hợp đồng này.", MSGType.Warning);
                return;
            }

            if (file.RefId != QueryId && file.RefId != TempContractFileRefId)
            {
                ShowNotify("File không thuộc hợp đồng này.", MSGType.Warning);
                return;
            }

            if (string.Equals(e.CommandName, "Download", StringComparison.OrdinalIgnoreCase))
            {
                DownloadContractFile(file);
                return;
            }

            if (string.Equals(e.CommandName, "Edit", StringComparison.OrdinalIgnoreCase))
            {
                EditContractFile(file);
                return;
            }

            if (string.Equals(e.CommandName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                DeleteContractFile(file);
            }
        }

        private void DownloadContractFile(TblUploadFile file)
        {
            string physicalPath = GetContractFilePhysicalPath(file);

            if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
            {
                ShowNotify("Không tìm thấy file trên hệ thống.", MSGType.Warning);
                return;
            }

            Response.Clear();
            Response.ContentType = string.IsNullOrWhiteSpace(file.MimeType)
                ? "application/octet-stream"
                : file.MimeType;
            Response.AddHeader(
                "Content-Disposition",
                "attachment;filename=\"" + HttpUtility.UrlEncode(file.OriginalFileName) + "\"");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.TransmitFile(physicalPath);
            Response.End();
        }

        private void DeleteContractFile(TblUploadFile file)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            UploadManager.Instance.RemoveFiles(
                new List<Guid> { file.Id },
                FileUploadTypes.ProjectContract);

            BindContractFiles(QueryId == Guid.Empty ? TempContractFileRefId : QueryId);

            ShowNotify("Đã xóa file.", MSGType.Success);
        }

        private void EditContractFile(TblUploadFile file)
{
    if (!IsContractFileEditable(file.Ext))
        return;

    if (IsDocxFile(file))
    {
        try
        {
            string html = ConvertDocxToHtml(file);

            txtNoiDungHopDong.Text = html;
            SetNoiDungHopDongToEditor(html);
        }
        catch (Exception ex)
        {
            ShowNotify("Không thể đọc nội dung file DOCX: " + ex.Message, MSGType.Error);
        }

        return;
    }

    if (IsPdfFile(file))
    {
        ShowNotify("Chức năng chỉnh sửa PDF sẽ xử lý ở bước tiếp theo.", MSGType.Warning);
    }
}
    }
}