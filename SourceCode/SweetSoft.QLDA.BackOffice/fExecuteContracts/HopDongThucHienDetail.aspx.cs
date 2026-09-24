using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fFilesBox;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

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
        private const string TempContractIdKey = "TempContractId";

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

        private Guid TempContractId
        {
            get
            {
                if (ViewState[TempContractIdKey] == null)
                    ViewState[TempContractIdKey] = Guid.NewGuid();

                return (Guid)ViewState[TempContractIdKey];
            }
        }

        private Guid ContractFileRefId
        {
            get
            {
                return QueryId != Guid.Empty ? QueryId : TempContractId;
            }
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

                ApplyControlsText();
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

        private void ApplyControlsText()
        {
            ddlKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            txtSoHopDong.PlaceHolder =
                txtTenHopDong.PlaceHolder =
                txtGiaTriHopDong.PlaceHolder =
                txtMoTa.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
        }

        private void RefreshHopDongInfo()
        {
            new ControlHelpers().BindKhachHang(ddlKhachHang);

            txtSoHopDong.Text =
                txtTenHopDong.Text =
                txtGiaTriHopDong.Text =
                txtNgayKy.Text =
                txtNgayHieuLuc.Text =
                txtNgayHetHan.Text =
                txtMoTa.Text = string.Empty;

            txtNoiDungHopDong.Text = string.Empty;

            ddlKhachHang.SelectedIndex = 0;
            txtSoHopDong.Enabled = true;
            txtTenHopDong.Enabled = true;
            pnlContractDocumentIdentityLocked.Visible = false;

            rblLoaiNoiDungHopDong.ClearSelection();
            rblLoaiNoiDungHopDong.SelectedValue = LoaiNoiDungHopDong.SOAN_THAO;

            BindLoaiNoiDungHopDong();
        }

        private void LoadHopDong(Guid idHopDongThucHien)
        {
            RefreshHopDongInfo();

            TblHopDongThucHien hopDong =
                HopDongThucHienManager.Instance.GetHopDongById(idHopDongThucHien);

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

            bool hasLinkedDocument =
                HopDongThucHienManager.Instance.HasLinkedDocument(
                    hopDong.IdHopDongThucHien);

            txtSoHopDong.Enabled = !hasLinkedDocument;
            txtTenHopDong.Enabled = !hasLinkedDocument;
            pnlContractDocumentIdentityLocked.Visible = hasLinkedDocument;

            if (!string.IsNullOrEmpty(hopDong.LoaiNoiDungHopDong) &&
                rblLoaiNoiDungHopDong.Items.FindByValue(hopDong.LoaiNoiDungHopDong) != null)
            {
                rblLoaiNoiDungHopDong.SelectedValue = hopDong.LoaiNoiDungHopDong;
            }

            BindLoaiNoiDungHopDong();
            BindContractFileUploader(hopDong.IdHopDongThucHien);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine =
                    ValidationEngine.Instance(this.Page);

                validationEngine.CheckValidControls(pnlContract.Controls);

                if (string.IsNullOrWhiteSpace(txtNgayKy.Text))
                {
                    validationEngine.AddErrorPrompt(
                        txtNgayKy.ClientID,
                        GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }

                Guid idKhachHang = Guid.Empty;

                if (!this.GetValue(ddlKhachHang, out idKhachHang) ||
                    idKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(
                        ddlKhachHang.ClientID,
                        GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }

                decimal giaTriHopDong;

                if (!decimal.TryParse(txtGiaTriHopDong.Text, out giaTriHopDong) ||
                    giaTriHopDong <= 0)
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

                hopDong.MoTa = string.IsNullOrWhiteSpace(txtMoTa.Text)
                    ? null
                    : txtMoTa.Text.Trim();

                hopDong.NoiDungHopDong =
                    rblLoaiNoiDungHopDong.SelectedValue == LoaiNoiDungHopDong.SOAN_THAO
                        ? txtNoiDungHopDong.Text
                        : null;

                hopDong.LoaiNoiDungHopDong =
                    rblLoaiNoiDungHopDong.SelectedValue;

                hopDong = HopDongThucHienManager.Instance.CreateOrUpdate(hopDong);

                if (hopDong == null || hopDong.IdHopDongThucHien == Guid.Empty)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (isAdd)
                {
                    BindTemporaryContractFilesToContract(
                        hopDong.IdHopDongThucHien);

                    ShowNotify(
                        GetResourceText(
                            BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    ShowSuccessSaveData();
                }

                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Contracts),
                    false);

                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void lbtCancel_Click(object sender, EventArgs e)
        {
            if (this.QueryId == Guid.Empty)
                RemoveTemporaryContractFiles();

            Response.Redirect(
                GetRelativeClientPath(RewriteURLHelper.Contracts),
                false);

            Context.ApplicationInstance.CompleteRequest();
        }

        protected void rblLoaiNoiDungHopDong_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            BindLoaiNoiDungHopDong();
        }

        private void BindLoaiNoiDungHopDong()
        {
            string value = rblLoaiNoiDungHopDong.SelectedValue;

            pnlSoanThao.Visible =
                value == LoaiNoiDungHopDong.SOAN_THAO;

            pnlTaiFile.Visible =
                value == LoaiNoiDungHopDong.TAI_FILE;
        }

        private void InitContractFileUploader()
        {
            fbHopDong.IsMultiple = false;
            fbHopDong.IsEnabled = this.IsAdd || this.IsEdit;
            fbHopDong.SingleFilePathType = FileTypes.Internal;

            fbHopDong.LoadFile(
                ContractFileRefId,
                FileUploadTypes.ProjectContract);
        }

        private void BindContractFileUploader(Guid idHopDongThucHien)
        {
            fbHopDong.IsMultiple = false;
            fbHopDong.IsEnabled = this.IsAdd || this.IsEdit;
            fbHopDong.SingleFilePathType = FileTypes.Internal;

            fbHopDong.LoadFile(
                idHopDongThucHien,
                FileUploadTypes.ProjectContract);
        }

        public void HandleFileCallback(string key)
        {
            if (string.Equals(
                key,
                ContractFileBeforeSaveCallbackKey,
                StringComparison.Ordinal))
            {
                return;
            }

            if (!string.Equals(
                key,
                ContractFileSavedCallbackKey,
                StringComparison.Ordinal))
            {
                return;
            }

            BindContractFileUploader(ContractFileRefId);
        }

        private void BindTemporaryContractFilesToContract(Guid contractId)
        {
            if (TempContractId == Guid.Empty ||
                contractId == Guid.Empty ||
                TempContractId == contractId)
            {
                return;
            }

            new Update(TblUploadFile.Schema)
                .Set(TblUploadFile.Columns.RefId)
                .EqualTo(contractId)
                .Where(TblUploadFile.Columns.RefId)
                .IsEqualTo(TempContractId)
                .And(TblUploadFile.Columns.RefType)
                .IsEqualTo(FileUploadTypes.ProjectContract.ToString())
                .And(TblUploadFile.Columns.IsDeleted)
                .IsEqualTo(false)
                .Execute();
        }

        private void RemoveTemporaryContractFiles()
        {
            if (TempContractId == Guid.Empty)
                return;

            List<TblUploadFile> files =
                new Select()
                    .From(TblUploadFile.Schema)
                    .Where(TblUploadFile.RefIdColumn)
                    .IsEqualTo(TempContractId)
                    .And(TblUploadFile.RefTypeColumn)
                    .IsEqualTo(FileUploadTypes.ProjectContract.ToString())
                    .And(TblUploadFile.IsDeletedColumn)
                    .IsEqualTo(false)
                    .ExecuteTypedList<TblUploadFile>();

            if (files == null || files.Count == 0)
                return;

            List<Guid> fileIds = new List<Guid>();

            foreach (TblUploadFile file in files)
            {
                if (file != null && file.Id != Guid.Empty)
                    fileIds.Add(file.Id);
            }

            if (fileIds.Count > 0)
            {
                UploadManager.Instance.RemoveFiles(
                    fileIds,
                    FileUploadTypes.ProjectContract);
            }
        }
    }
}