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
            CtrlHopDongThucHien1.OpenContractDocumentHandlerCallback += OpenContractDocumentAction;

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
            txtTenHopDong.Enabled = true;
            pnlContractDocumentIdentityLocked.Visible = false;

            this.IdHopDongThucHien = Guid.Empty;
        }

        private void EditHopDongAction(object sender, EventArgs e)
        {
            if (!(sender is Guid idHopDongThucHien) || idHopDongThucHien == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            string idQuery = SecurityUtilities.ProtectUrlParameter(idHopDongThucHien.ToString());

            Response.Redirect(RewriteURLHelper.ContractDetail(idHopDongThucHien));

            Context.ApplicationInstance.CompleteRequest();
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
            if (!this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Response.Redirect(RewriteURLHelper.ContractDetail(Guid.Empty));
            Context.ApplicationInstance.CompleteRequest();
        }

        private void OpenContractDocumentAction(object sender, EventArgs e)
        {
            Guid idHopDongThucHien = sender is Guid
                ? (Guid)sender
                : Guid.Empty;
            if (idHopDongThucHien == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                ContractDocumentLinkResult result = HopDongThucHienManager
                    .Instance
                    .GetOrCreateProjectDocument(idHopDongThucHien);

                string url = RewriteURLHelper.ProjectDocumentDetail(
                    result.ProjectId,
                    result.DocumentId) + "?tab=versions";
                Response.Redirect(GetRelativeClientPath(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (UnauthorizedAccessException)
            {
                ShowAccessDeniedNotify();
            }
            catch (InvalidOperationException exception)
            {
                ShowNotify(exception.Message, MSGType.Warning);
            }
            catch (Exception exception)
            {
                ShowNotify(exception.Message, MSGType.Error);
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlHopDongThucHien1.ConfirmRequest(e);
        }

        #endregion
    }
}
