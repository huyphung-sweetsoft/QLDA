using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class DuAnList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Project;
            }
        }
        private Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] != null)
                    return (Guid)ViewState["IdHopDongThucHien"];
                return Guid.Empty;
            }
            set
            {
                ViewState["IdHopDongThucHien"] = value;
            }
        }
        private Guid IdDuAn
        {
            get
            {
                if (ViewState["IdDuAn"] != null)
                    return (Guid)ViewState["IdDuAn"];
                return Guid.Empty;
            }
            set
            {
                ViewState["IdDuAn"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlDuAn1.NewProjectHandlerCallBack += NewProjectAction;
            CtrlDuAn1.EditProjectHandlerCallBack += EditProjectAction;

            txtSoHopDong.EnterSubmitClientID = btnSearchHopDong.ClientID;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROJECT_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Projects, GetResourceText(BackEndResourceKeys.PROJECT_LIST) }
                };
                CtrlDuAn1.InitControls();
                ApplyControlsText();
            }

        }

        private void ApplyControlsText()
        {
            ddlKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlLoaiDuAn.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlNhanVienQuanLy.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            txtTenDuAn.PlaceHolder = txtGiaTriHopDong.PlaceHolder
                = txtSoHopDong.PlaceHolder
                = txtNgayKy.PlaceHolder
                = txtMaDuAn.PlaceHolder = "";
        }

        private void RefreshProjectInfo()
        {
            new ControlHelpers().BindLoaiDuAn(ddlLoaiDuAn);
            new ControlHelpers().BindKhachHang(ddlKhachHang);
            new ControlHelpers().BindDuAnStatus(ddlTrangThai);
            new ControlHelpers().BindNhanVien(ddlNhanVienQuanLy);
            lbtSubmit.Visible = false;
            //---------------------------------------------------
            txtMaDuAn.Enabled = false;
            txtMaDuAn.Text = txtTenDuAn.Text 
                = txtGiaTriHopDong.Text 
                = txtSoHopDong.Text 
                = txtNgayKy.Text 
                = txtMaDuAn.Text = "";
            dtNgayBatDau.DateValue = null;
            dtNgayKetThuc.DateValue = null;
            ddlTrangThai.SelectedIndex = 0;
            this.IdDuAn = Guid.Empty;
        }

        private void NewProjectAction(object sender, EventArgs e)
        {
            RefreshProjectInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
            
        }

        private void EditProjectAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid idDuAn = (Guid)sender;
            if (idDuAn == Guid.Empty)
            {
                ShowInvalidDataError(); 
                return;
            }
            RefreshProjectInfo();
            lbtSubmit.Visible = this.IsEdit;
            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(idDuAn);
            if (duAn == null || duAn.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }
            this.IdDuAn = duAn.IdDuAn;
            txtMaDuAn.Text = duAn.MaDuAn;
            txtMaDuAn.Enabled = false;
            txtTenDuAn.Text = duAn.TenDuAn;
            txtMoTa.Text = duAn.MoTa;
            ddlLoaiDuAn.SelectedValue = duAn.IdLoaiDuAn.ToString();
            ddlKhachHang.SelectedValue = duAn.IdKhachHang.ToString();
            ddlNhanVienQuanLy.SelectedValue = duAn.IdNhanVienQuanLy.ToString();
            ddlTrangThai.SelectedValue = duAn.TrangThai.ToString();
            dtNgayBatDau.DateValue = duAn.NgayBatDau;
            dtNgayKetThuc.DateValue = duAn.NgayDuKienHoanThanh;

            LoadHopDongThucHien(duAn);

            lbtSubmit.Visible = this.IsEdit;

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                Guid idLoaiDuAn = Guid.Empty;
                Guid idKhachHang = Guid.Empty;
                Guid idNhanVienQuanLy = Guid.Empty;
                byte trangThai = 0;

                if (string.IsNullOrWhiteSpace(txtTenDuAn.Text))
                {
                    validationEngine.AddErrorPrompt(txtTenDuAn.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }
                if (!GetValue(ddlNhanVienQuanLy, out idNhanVienQuanLy) || idNhanVienQuanLy == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlNhanVienQuanLy.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!GetValue(ddlKhachHang, out idKhachHang) || idKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlKhachHang.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!GetValue(ddlLoaiDuAn, out idLoaiDuAn) || idLoaiDuAn == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlLoaiDuAn.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!byte.TryParse(ddlTrangThai.SelectedValue, out trangThai) || !Enum.IsDefined(typeof(DuAnStatus), trangThai))
                {
                    validationEngine.AddErrorPrompt(ddlTrangThai.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!string.IsNullOrWhiteSpace(txtSoHopDong.Text) && IdHopDongThucHien == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(txtSoHopDong.ClientID, "Số hợp đồng không tồn tại.");
                }
                if (!dtNgayBatDau.DateValue.HasValue)
                {
                    validationEngine.AddErrorPrompt(dtNgayBatDau.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!dtNgayKetThuc.DateValue.HasValue)
                {
                    validationEngine.AddErrorPrompt(dtNgayKetThuc.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }

                if (dtNgayBatDau.DateValue.HasValue && dtNgayKetThuc.DateValue.HasValue && dtNgayKetThuc.DateValue.Value.Date < dtNgayBatDau.DateValue.Value.Date)
                {
                    validationEngine.AddErrorPrompt(dtNgayKetThuc.ClientID, "Ngày hoàn thành dự kiến phải bằng hoặc sau ngày bắt đầu.");
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                bool isAdd = true;
                DuAnManager organiztionDuAnManager = DuAnManager.Instance;
                TblDuAn duAn = new TblDuAn();
                if (this.IdDuAn != Guid.Empty)
                {
                    duAn.IdDuAn = this.IdDuAn;
                    if (!this.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    isAdd = false;
                }
                else if (!this.IsAdd)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                duAn.TenDuAn = txtTenDuAn.Text.Trim();
                duAn.MoTa = txtMoTa.Text.Trim();
                duAn.IdHopDongThucHien = this.IdHopDongThucHien == Guid.Empty ? (Guid?)null : this.IdHopDongThucHien;
                duAn.TrangThai = trangThai;
                //duAn.TrangThai = (byte)ddlTrangThai.SelectedValue;
                if (dtNgayBatDau.DateValue.HasValue)
                    duAn.NgayBatDau = dtNgayBatDau.DateValue.Value;
                if (dtNgayKetThuc.DateValue.HasValue)
                    duAn.NgayDuKienHoanThanh = dtNgayKetThuc.DateValue.Value;
                if (this.GetValue(ddlLoaiDuAn, out idLoaiDuAn) && idLoaiDuAn != Guid.Empty)
                    duAn.IdLoaiDuAn = idLoaiDuAn;
                if (this.GetValue(ddlKhachHang, out idKhachHang) && idKhachHang != Guid.Empty)
                    duAn.IdKhachHang = idKhachHang;
                if (this.GetValue(ddlNhanVienQuanLy, out idNhanVienQuanLy) && idNhanVienQuanLy != Guid.Empty)
                    duAn.IdNhanVienQuanLy = idNhanVienQuanLy;
                if (isAdd)
                { 
                    duAn = organiztionDuAnManager.CreateOrUpdate(duAn);
                    if (duAn == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    duAn = organiztionDuAnManager.CreateOrUpdate(duAn);
                    if (duAn == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }
            dlDetail.CloseModal();
            CtrlDuAn1.Rebind();
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlDuAn1.ConfirmRequest(e);
        }

        protected void txtSoHopDong_TextChanged(object sender, EventArgs e)
        {
            IdHopDongThucHien = Guid.Empty;
            txtGiaTriHopDong.Text = "";
            txtNgayKy.Text = "";

            string soHopDong = txtSoHopDong.Text.Trim();
            if (string.IsNullOrEmpty(soHopDong))
                return;
            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetBySoHopDong(soHopDong);

            if (hopDong != null)
            {
                IdHopDongThucHien = hopDong.IdHopDongThucHien;
                txtGiaTriHopDong.Text = hopDong.GiaTriHopDong.ToString();
                txtNgayKy.Text = hopDong.NgayKy.ToString();
            }
            else
                return;
            upHopDong.Update();
        }

        private void LoadHopDongThucHien(TblDuAn duAn)
        {
            this.IdHopDongThucHien = Guid.Empty;

            txtSoHopDong.Text = "";
            txtGiaTriHopDong.Text = "";
            txtNgayKy.Text = "";

            if (!duAn.IdHopDongThucHien.HasValue || duAn.IdHopDongThucHien.Value == Guid.Empty)
            {
                return;
            }

            TblHopDongThucHien hd = HopDongThucHienManager.Instance.GetHopDongById(duAn.IdHopDongThucHien.Value);

            if (hd == null)
                return;

            this.IdHopDongThucHien = hd.IdHopDongThucHien;
            txtSoHopDong.Text = hd.SoHopDong;
            txtGiaTriHopDong.Text = hd.GiaTriHopDong.ToString();
            txtNgayKy.Text = hd.NgayKy.ToString();
        }
    }
}