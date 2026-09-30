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
using System.Linq;
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

        private Guid? ImportedContentFileId
        {
            get
            {
                object value = ViewState["ImportedContentFileId"];
                return value is Guid ? (Guid?) (Guid)value : null;
            }
            set { ViewState["ImportedContentFileId"] = value.HasValue ? (object)value.Value : null; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UpdateRestoreButtonVisibility();
            fbHopDong.FileMutationValidator = (recordId, refType, fileId) =>
            {
                Guid currentRefId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
                TblUploadFile file = TblUploadFile.FetchByID(fileId);
                return refType == FileUploadTypes.ProjectContract
                    && recordId == currentRefId
                    && (QueryId == Guid.Empty ? IsAdd : IsEdit)
                    && file != null && file.IsDeleted != true
                    && file.RefId == recordId
                    && file.RefType == FileUploadTypes.ProjectContract.ToString()
                    && (QueryId != Guid.Empty || file.OwnerId == SweetContext.Current.UserId);
            };
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
                pnlImportContractContent.Visible = this.QueryId == Guid.Empty ? this.IsAdd : this.IsEdit;
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
            if (!(QueryId == Guid.Empty ? this.IsAdd : this.IsEdit))
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid fileId;
            if (!Guid.TryParse(ddlContractContentFile.SelectedValue, out fileId))
            {
                ShowNotify("Hãy chọn file PDF hoặc DOCX trong danh sách đính kèm.", MSGType.Warning);
                dlContractFiles.OpenModal(true);
                return;
            }

            ApplyContractFileContent(fileId, false);
        }

        protected void lbtRestoreContent_Click(object sender, EventArgs e)
        {
            if (!(QueryId == Guid.Empty ? this.IsAdd : this.IsEdit))
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid fileId;
            if (ImportedContentFileId.HasValue)
            {
                fileId = ImportedContentFileId.Value;
            }
            else
            {
                Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
                List<TblUploadFile> sourceFiles = GetContractFiles(refId)
                    .Where(item => item.IsDeleted != true && (IsDocxFile(item) || IsPdfFile(item)))
                    .ToList();
                if (sourceFiles.Count != 1)
                {
                    ShowNotify(sourceFiles.Count == 0
                        ? "Chưa có file PDF hoặc DOCX để khôi phục nội dung."
                        : "Có nhiều file đính kèm. Hãy chọn đúng file gốc trong danh sách để khôi phục nội dung.",
                        MSGType.Warning);
                    if (sourceFiles.Count > 1)
                        dlContractFiles.OpenModal(true);
                    return;
                }
                fileId = sourceFiles[0].Id;
            }

            ApplyContractFileContent(fileId, true);
        }

        private void ApplyContractFileContent(Guid fileId, bool isRestore)
        {
            Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
            TblUploadFile file = GetContractFiles(refId)
                .FirstOrDefault(item => item.Id == fileId && item.IsDeleted != true);

            if (file == null)
            {
                ShowNotify("Không tìm thấy file gốc để khôi phục nội dung.", MSGType.Warning);
                if (isRestore)
                {
                    ImportedContentFileId = null;
                    BindContractContentFiles(refId);
                }
                else
                    dlContractFiles.OpenModal(true);
                return;
            }

            try
            {
                string html = string.Empty;

                if (IsDocxFile(file))
                {
                    html = ConvertDocxToHtml(file);
                }
                else if (IsPdfFile(file))
                {
                    string physicalPath = GetContractFilePhysicalPath(file);
                    html = PdfManager.Instance.ConvertPdfToHtml(physicalPath);
                }
                else
                {
                    ShowNotify("Định dạng file này chưa hỗ trợ khôi phục nội dung.", MSGType.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    ShowNotify("Không trích xuất được nội dung từ file gốc.", MSGType.Warning);
                    return;
                }

                txtNoiDungHopDong.Text = html;
                SetNoiDungHopDongToEditor(html);
                ImportedContentFileId = fileId;
                UpdateRestoreButtonVisibility();

                ShowNotify(isRestore
                    ? "Đã khôi phục nội dung từ file gốc. Bấm Lưu hợp đồng để ghi nhận."
                    : "Đã đưa nội dung file vào trình soạn thảo. Bấm Lưu hợp đồng để ghi nhận.",
                    MSGType.Success);
            }
            catch (Exception ex)
            {
                ShowNotify("Không thể khôi phục nội dung từ file gốc: " + ex.Message, MSGType.Error);
            }
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
            if (key == ContractFileBeforeSaveCallbackKey)
                return;

            if (key != ContractFileSavedCallbackKey)
                return;

            Guid refId = QueryId == Guid.Empty ? TempContractFileRefId : QueryId;
            fbHopDong.LoadFile(refId, FileUploadTypes.ProjectContract);
            BindContractContentFiles(refId);
            dlContractFiles.OpenModal(true);
        }

        public override void DataCallback(string key, object value, object valueText)
        {
            HandleFileCallback(key);
        }

        private void InitContractFileUploader()
        {
            fbHopDong.IsMultiple = true;
            fbHopDong.IsEnabled = this.QueryId == Guid.Empty ? this.IsAdd : this.IsEdit;
            fbHopDong.BeforeSaveDataCallbackKey = ContractFileBeforeSaveCallbackKey;
            fbHopDong.SaveDataCallbackKey = ContractFileSavedCallbackKey;

            Guid refId = this.QueryId == Guid.Empty ? TempContractFileRefId : this.QueryId;
            if (this.QueryId == Guid.Empty && this.IsAdd)
                Session["ContractFileDraft:" + refId.ToString("N")] = SweetContext.Current.UserId;
            fbHopDong.LoadFile(refId, FileUploadTypes.ProjectContract);
            BindContractContentFiles(refId);
        }

        private List<TblUploadFile> GetContractFiles(Guid refId)
        {
            if (refId == Guid.Empty)
                return new List<TblUploadFile>();

            return new UploadManager(SweetContext.Current, refId, FileUploadTypes.ProjectContract)
                .TblUploadFiles ?? new List<TblUploadFile>();
        }

        private void BindContractContentFiles(Guid refId)
        {
            string selectedId = ddlContractContentFile.SelectedValue;
            ddlContractContentFile.Items.Clear();
            ddlContractContentFile.Items.Add(new System.Web.UI.WebControls.ListItem("Chọn file PDF hoặc DOCX", ""));
            foreach (TblUploadFile file in GetContractFiles(refId)
                .Where(item => item.IsDeleted != true && (IsPdfFile(item) || IsDocxFile(item)))
                .OrderBy(item => item.DisplayOrder).ThenBy(item => item.CreatedDate))
            {
                ddlContractContentFile.Items.Add(new System.Web.UI.WebControls.ListItem(
                    string.IsNullOrWhiteSpace(file.OriginalFileName) ? file.Name : file.OriginalFileName,
                    file.Id.ToString()));
            }
            if (ddlContractContentFile.Items.FindByValue(selectedId) != null)
                ddlContractContentFile.SelectedValue = selectedId;
            UpdateRestoreButtonVisibility();
        }

        private void UpdateRestoreButtonVisibility()
        {
            lbtRestoreContent.Visible = ddlContractContentFile.Items.Count > 1
                && (QueryId == Guid.Empty ? IsAdd : IsEdit);
            lbtRestoreContent.OnClientClick = ImportedContentFileId.HasValue
                || ddlContractContentFile.Items.Count == 2
                ? "return confirm('Khôi phục nội dung từ file gốc? Các chỉnh sửa chưa lưu trong trình soạn thảo sẽ bị mất.');"
                : string.Empty;
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
            BindContractContentFiles(hopDong.IdHopDongThucHien);
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

                hopDong.NoiDungHopDong = txtNoiDungHopDong.Text;
                hopDong.LoaiNoiDungHopDong = LoaiNoiDungHopDong.SOAN_THAO;

                hopDong = HopDongThucHienManager.Instance.CreateOrUpdate(hopDong);

                if (hopDong == null || hopDong.IdHopDongThucHien == Guid.Empty)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (isAdd)
                {
                    LinkContractFiles(TempContractFileRefId, hopDong.IdHopDongThucHien);
                    Session.Remove("ContractFileDraft:" + TempContractFileRefId.ToString("N"));
                }


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
    }
}
