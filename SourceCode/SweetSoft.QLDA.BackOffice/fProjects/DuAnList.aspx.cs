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
            if (!IsPostBack)
            {
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
            txtTenDuAn.Text = txtGiaTriHopDong.Text 
                = txtSoHopDong.Text 
                = txtNgayKy.Text 
                = txtMaDuAn.Text = "";
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
            {
                upHopDong.Update();
                return;
            }
        }
    }
}