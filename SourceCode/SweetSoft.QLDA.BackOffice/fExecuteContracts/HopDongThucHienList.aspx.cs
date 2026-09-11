using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fExecuteContracts.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fExecuteContracts
{
    public partial class HopDongThucHienList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.Contract; }
        }

        private Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] != null)
                {
                    return (Guid)ViewState["IdHopDongThucHien"];
                }
                return Guid.Empty;
            }
            set { ViewState["IdHopDongThucHien"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlHopDongThucHien1.NewHopDongHandlerCallback += NewHopDongAction;
            CtrlHopDongThucHien1.EditHopDongHandlerCallback += EditHopDongAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                }

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CONTRACT_LIST));

                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.CONTRACT_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Contracts, GetResourceText(BackEndResourceKeys.CONTRACT_LIST) }
                };

                ApplyControlsText();

                CtrlHopDongThucHien1.InitControls();

                string idQuery = CommonHelpers.QueryString("hopDongId");

                if (string.IsNullOrEmpty(idQuery))
                    return;

                Guid tempId = Guid.Empty;

                if (!Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId))
                {
                    return;
                }

                EditHopDongAction(tempId, EventArgs.Empty);
            }
        }

        private void ApplyControlsText()
        {
            ddlKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);

            txtSoHopDong.PlaceHolder = 
                txtTenHopDong.PlaceHolder = 
                txtGiaTriHopDong.PlaceHolder = 
                txtMoTa.PlaceHolder = 
                GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
        }

        #region Modal

        private void RefreshHopDongInfo()
        {
            new ControlHelpers().BindKhachHang(ddlKhachHang);

            lbtSubmit.Visible = false;

            txtSoHopDong.Text = 
                txtTenHopDong.Text = 
                txtGiaTriHopDong.Text = 
                txtNgayKy.Text = 
                txtNgayHieuLuc.Text = 
                txtNgayHetHan.Text = 
                txtMoTa.Text = string.Empty;

            ddlKhachHang.SelectedIndex = 0;
            txtSoHopDong.Enabled = true;

            this.IdHopDongThucHien = Guid.Empty;
        }

        private void EditHopDongAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }

            Guid idHopDongThucHien = (Guid)sender;

            if (idHopDongThucHien == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            RefreshHopDongInfo();

            lbtSubmit.Visible = this.IsEdit;

            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(idHopDongThucHien);

            if (hopDong == null || hopDong.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.IdHopDongThucHien = hopDong.IdHopDongThucHien;

            txtSoHopDong.Text = hopDong.SoHopDong;
            txtTenHopDong.Text = hopDong.TenHopDong;
            ddlKhachHang.SelectedValue = hopDong.IdKhachHang.ToString();
            
            decimal giaTriHopDong;

            if (!decimal.TryParse(
                txtGiaTriHopDong.Text,
                out giaTriHopDong) ||
                giaTriHopDong <= 0)
            {
                //ValidationEngine.AddErrorPrompt(
                //    txtGiaTriHopDong.ClientID,
                //    "Giá trị hợp đồng phải lớn hơn 0.");
            }

            bool validGiaTri = decimal.TryParse(
                txtGiaTriHopDong.Text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out giaTriHopDong);

            if (!validGiaTri)
            {
                validGiaTri = decimal.TryParse(
                    txtGiaTriHopDong.Text,
                    out giaTriHopDong);
            }

            txtNgayKy.Text = hopDong.NgayKy.HasValue ? hopDong.NgayKy.Value.ToString("yyyy-MM-dd") : string.Empty;
            txtNgayHieuLuc.Text = hopDong.NgayHieuLuc.HasValue ? hopDong.NgayHieuLuc.Value.ToString("yyyy-MM-dd") : string.Empty;
            txtNgayHetHan.Text = hopDong.NgayHetHan.HasValue ? hopDong.NgayHetHan.Value.ToString("yyyy-MM-dd") : string.Empty;

            txtMoTa.Text = hopDong.MoTa;

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = "Thông tin hợp đồng";

            if (!IsPostBack)
            {
                dlDetail.OpenModal(true, 1000);
            }
            else
            {
                dlDetail.OpenModal(true);
            }
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid

                ValidationEngine validationEngine =
                    ValidationEngine.Instance(this.Page);

                validationEngine.CheckValidControls(
                    dlDetail.Controls);

                if (string.IsNullOrWhiteSpace(
                    txtNgayKy.Text))
                {
                    validationEngine.AddErrorPrompt(
                        txtNgayKy.ClientID,
                        GetResourceText(
                            BackEndResourceKeys
                                .PLEASE_ENTER_THE_VALUE));
                }

                Guid idKhachHang =
                    Guid.Empty;

                if (!this.GetValue(
                    ddlKhachHang,
                    out idKhachHang) ||
                    idKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(
                        ddlKhachHang.ClientID,
                        GetResourceText(
                            BackEndResourceKeys
                                .PLEASE_ENTER_THE_VALUE));
                }

                decimal giaTriHopDong;

                if (!decimal.TryParse(
                    txtGiaTriHopDong.Text,
                    out giaTriHopDong) ||
                    giaTriHopDong <= 0)
                {
                    validationEngine.AddErrorPrompt(
                        txtGiaTriHopDong.ClientID,
                        "Giá trị hợp đồng phải lớn hơn 0.");
                }

                DateTime ngayKy;

                if (!DateTime.TryParse(
                    txtNgayKy.Text,
                    out ngayKy))
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

                #endregion

                bool isAdd = true;
                HopDongThucHienManager organizationHopDongThucHienManager = HopDongThucHienManager.Instance;
                TblHopDongThucHien hopDong = new TblHopDongThucHien();

                if (this.IdHopDongThucHien != Guid.Empty)
                {
                    hopDong.IdHopDongThucHien = this.IdHopDongThucHien;

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

                hopDong.SoHopDong = txtSoHopDong.Text.Trim();
                hopDong.TenHopDong = txtTenHopDong.Text.Trim();
                hopDong.IdKhachHang = idKhachHang;
                hopDong.GiaTriHopDong = giaTriHopDong;
                hopDong.NgayKy = ngayKy;

                DateTime tempDate;

                if (DateTime.TryParse(txtNgayHieuLuc.Text, out tempDate))
                {
                    hopDong.NgayHieuLuc = tempDate;
                }
                else
                {
                    hopDong.NgayHieuLuc = null;
                }

                if (DateTime.TryParse(txtNgayHetHan.Text, out tempDate))
                {
                    hopDong.NgayHetHan = tempDate;
                }
                else
                {
                    hopDong.NgayHetHan = null;
                }

                hopDong.MoTa = string.IsNullOrWhiteSpace(txtMoTa.Text) ? null : txtMoTa.Text.Trim();

                hopDong = organizationHopDongThucHienManager.CreateOrUpdate(hopDong);

                if (hopDong == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (isAdd)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }

            dlDetail.CloseModal();
            CtrlHopDongThucHien1.Rebind();
        }

        #endregion

        #region Button

        private void NewHopDongAction(object sender, EventArgs e)
        {
            RefreshHopDongInfo();

            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);

            dlDetail.OpenModal(true);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlHopDongThucHien1.ConfirmRequest(e);
        }

        #endregion
    }
}