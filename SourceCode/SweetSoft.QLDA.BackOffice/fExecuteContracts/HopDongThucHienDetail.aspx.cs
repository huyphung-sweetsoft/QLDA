using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

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

        protected Guid TempContractFileRefId
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
                lbtExportDocx.Visible = this.QueryId != Guid.Empty;
                lbtResetContent.Visible = this.IsEdit;
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

        protected void lbtExportDocx_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ExportContractDocxAsync));
        }

        protected void lbtResetContent_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid fileId;
            if (!Guid.TryParse(hdfSelectedContractFileId.Value, out fileId))
            {
                ShowNotify("Vui lòng chọn file cần khôi phục nội dung.", MSGType.Warning);
                return;
            }

            TblUploadFile file = GetContractFileById(fileId);
            if (file == null)
            {
                ShowNotify("File không tồn tại hoặc không thuộc hợp đồng đang chỉnh sửa.", MSGType.Warning);
                return;
            }

            if (LoadContractFileContent(file))
                ShowNotify("Đã khôi phục nội dung từ file được chọn.", MSGType.Success);
        }

        private async Task ExportContractDocxAsync()
        {
            if (!this.IsView)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (QueryId == Guid.Empty)
            {
                ShowNotify("Hợp đồng chưa được tạo.", MSGType.Warning);
                return;
            }

            string htmlContent = txtNoiDungHopDong.Text;

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                ShowNotify("Hợp đồng chưa có nội dung để xuất DOCX.", MSGType.Warning);
                return;
            }

            try
            {
                byte[] docxBytes;

                using (MemoryStream stream = new MemoryStream())
                {
                    using (WordprocessingDocument package = WordprocessingDocument.Create(
                        stream,
                        DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
                        true))
                    {
                        MainDocumentPart mainPart = package.AddMainDocumentPart();
                        mainPart.Document = new Document(new Body());

                        HtmlConverter converter = new HtmlConverter(mainPart);
                        await converter.ParseBody(htmlContent);

                        // Khổ giấy A4, lề 15mm.
                        mainPart.Document.Body.Append(new SectionProperties(
                            new PageSize { Width = 11906U, Height = 16838U },
                            new PageMargin
                            {
                                Top = 850,
                                Bottom = 850,
                                Left = 850,
                                Right = 850,
                                Header = 567,
                                Footer = 567,
                                Gutter = 0
                            }));

                        mainPart.Document.Save();
                    }

                    docxBytes = stream.ToArray();
                }

                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                Response.AddHeader(
                    "Content-Disposition",
                    "attachment; filename=HopDongThucHien_" + QueryId.ToString("N") + ".docx");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(docxBytes);
                Response.Flush();
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowNotify("Không thể xuất file DOCX: " + ex.Message, MSGType.Error);
            }
        }

        public void HandleFileCallback(string key)
        {
            System.Diagnostics.Debug.WriteLine("[Contract] HandleFileCallback: " + key);
            if (key == ContractFileBeforeSaveCallbackKey)
                return;

            if (key != ContractFileSavedCallbackKey)
                return;

            Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
            fbHopDong.LoadFile(refId, FileUploadTypes.ProjectContract);

            if (hdfSubmitAfterFileApply.Value == "1")
            {
                hdfSubmitAfterFileApply.Value = "0";

                ScriptManager.RegisterStartupScript(
                    this.Page,
                    this.Page.GetType(),
                    "SubmitContractAfterFileApply",
                    "document.getElementById('" + lbtSubmit.ClientID + "').click();",
                    true);
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
            fbHopDong.IsMultiple = true;
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


            fbHopDong.LoadFile(hopDong.IdHopDongThucHien, FileUploadTypes.ProjectContract);
        }

        protected void lbtLoadContractFile_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit && !this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid fileId;
            if (!Guid.TryParse(hdfSelectedContractFileId.Value, out fileId))
            {
                ShowNotify("Vui lòng chọn file hợp đồng hợp lệ.", MSGType.Warning);
                return;
            }

            TblUploadFile file = GetContractFileById(fileId);
            if (file == null)
            {
                ShowNotify("File không tồn tại hoặc không thuộc hợp đồng đang chỉnh sửa.", MSGType.Warning);
                return;
            }

            if (LoadContractFileContent(file))
                ShowNotify("Đã nạp nội dung từ file được chọn.", MSGType.Success);
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
                    hopDong.NoiDungHopDong = string.IsNullOrWhiteSpace(txtNoiDungHopDong.Text)
                        ? null
                        : txtNoiDungHopDong.Text;
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

        private TblUploadFile GetContractFileById(Guid fileId)
        {
            if (fileId == Guid.Empty)
                return null;

            Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
            TblUploadFile file = new UploadManager(SweetContext.Current, fileId).File;

            if (file == null || file.IsDeleted
                || file.RefId != refId
                || file.RefType != FileUploadTypes.ProjectContract.ToString())
                return null;

            return file;
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
            string refType = FileUploadTypes.ProjectContractPdf.ToString();
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
            TblUploadFile file = UploadManager.Instance.GetUploadFileByRefIdAndRefType(contractId, FileUploadTypes.ProjectContractPdf);

            if (file == null)
                return;

            UploadManager.Instance.RemoveFiles(new List<Guid> { file.Id }, FileUploadTypes.ProjectContractPdf);
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

        private bool LoadContractFileContent(TblUploadFile file)
        {
            if (file == null)
            {
                ShowNotify("Không tìm thấy file hợp đồng.", MSGType.Warning);
                return false;
            }

            try
            {
                string html;

                if (IsDocxFile(file))
                {
                    html = ConvertDocxToHtml(file);
                }
                else if (IsPdfFile(file))
                {
                    string physicalPath = GetContractFilePhysicalPath(file);
                    if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
                        throw new FileNotFoundException("Không tìm thấy file PDF.", physicalPath);

                    html = PdfManager.Instance.ConvertPdfToHtml(physicalPath);
                }
                else
                {
                    ShowNotify("Định dạng file này chưa hỗ trợ chuyển nội dung.", MSGType.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    ShowNotify("Không trích xuất được nội dung từ file. File có thể là bản scan hoặc không chứa lớp văn bản.", MSGType.Warning);
                    return false;
                }

                ClearContractContent = false;
                txtNoiDungHopDong.Text = html;
                SetNoiDungHopDongToEditor(html);
                return true;
            }
            catch (Exception ex)
            {
                ShowNotify("Không thể đọc nội dung file: " + ex.Message, MSGType.Error);
                return false;
            }
        }
    }
}