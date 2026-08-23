using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
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
            lbtSubmit.Visible = false;
            //---------------------------------------------------
            txtMaDuAn.Enabled = false;
            txtTenDuAn.Text = txtGiaTriHopDong.Text 
                = txtSoHopDong.Text 
                = txtNgayKy.Text 
                = txtMaDuAn.Text = "";
            ddlLoaiDuAn.SelectedIndex = 0;
            ddlKhachHang.SelectedIndex = 0;
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