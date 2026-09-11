using SubSonic;
using SubSonic.Sugar;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fMeets
{
    public partial class MeetList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Meet;
        private ControlHelpers _control = new ControlHelpers();
        private Guid MeetId
        {
            get => ViewState["MeetId"] != null ? (Guid)ViewState["MeetId"] : Guid.Empty;
            set => ViewState["MeetId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlMeet1.NewMeetingHandlerCallback += NewMeetingAction;
            CtrlMeet1.EditMeetingHandlerCallback += EditMeetingAction;
            new ControlHelpers().BindNhanVienToCheckBoxList(cblNhanVien);
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.MEETING_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.MEETING_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.MEETING_LIST) }
                };
                ApplyControlsText();
                CtrlMeet1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            ddlTrangThai.PlaceHolder = "--";
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);

            txtThoiGianKetThuc.PlaceHolder = "--";

            txtTenCuocHop.PlaceHolder = txtNoiDungCuocHop.PlaceHolder = txtThoiGianBatDau.PlaceHolder =
            txtDiaDiemHop.PlaceHolder = txtThoiLuong.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            dlChonNhanVien.Title = GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE);
        }

        private void NewMeetingAction(object sender, EventArgs e)
        {
            RefreshMeetingInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        private void EditMeetingAction(object sender, EventArgs e)
        {
            if (sender == null || (Guid)sender == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            Guid idLichHop = (Guid)sender;
            RefreshMeetingInfo();
            lbtSubmit.Visible = this.IsEdit;

            TblLichHop meet = TblLichHop.FetchByID(idLichHop);
            if (meet == null || meet.DaXoa == true)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.MeetId = meet.IdLichHop;
            txtTenCuocHop.Text = meet.TenCuocHop;
            txtNoiDungCuocHop.Text = meet.NoiDungCuocHop;
            txtDiaDiemHop.Text = meet.DiaDiemHop;

            _control.BindNhanVienThamGiaLichHop(meet.IdLichHop, hdfNhanVienIds, txtNhanVienThamGia);

            if (meet.ThoiGianBatDau != DateTime.MinValue)
                txtThoiGianBatDau.DateValue = meet.ThoiGianBatDau;

            if (meet.ThoiGianKetThuc != DateTime.MinValue)
            {
                txtThoiGianKetThuc.Text = meet.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm");
                if (meet.ThoiGianBatDau != DateTime.MinValue)
                {
                    int thoiLuong = (int)(meet.ThoiGianKetThuc - meet.ThoiGianBatDau).TotalMinutes;
                    txtThoiLuong.Text = thoiLuong.ToString();
                }
            }

            if (meet.TrangThai != null)
                ddlTrangThai.SelectedValue = meet.TrangThai.ToString();

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT) ?? "Thông tin cuộc họp";

            dlDetail.OpenModal(true, IsPostBack ? 0 : 1000);
        }

        private void RefreshMeetingInfo()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindTrangThaiLichHop(ddlTrangThai);

            txtTenCuocHop.Text = txtNoiDungCuocHop.Text = txtDiaDiemHop.Text = "";

            txtThoiGianBatDau.DateValue = null;
            txtThoiLuong.Text = "";
            txtThoiGianKetThuc.Text = "";
            txtNhanVienThamGia.Text = "";
            hdfNhanVienIds.Value = "";

            lbtSubmit.Visible = false;

            if (ddlTrangThai.Items.Count > 0) ddlTrangThai.SelectedIndex = 0;

            this.MeetId = Guid.Empty;
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                TblLichHop meetDto = new TblLichHop();
                bool isNew = (this.MeetId == Guid.Empty);

                if (!isNew)
                {
                    meetDto.IdLichHop = this.MeetId;
                }

                meetDto.IdDuAn = CtrlMeet1.ProjectId;
                meetDto.TenCuocHop = txtTenCuocHop.Text.Trim();
                meetDto.NoiDungCuocHop = !string.IsNullOrEmpty(txtNoiDungCuocHop.Text.Trim()) ? txtNoiDungCuocHop.Text.Trim() : null;
                meetDto.DiaDiemHop = txtDiaDiemHop.Text.Trim();

                string strEnd = txtThoiGianKetThuc.Text.Trim();
                if (string.IsNullOrEmpty(strEnd))
                {
                    strEnd = Request.Params[txtThoiGianKetThuc.UniqueID] ?? "";
                }

                DateTime dtEnd;
                int thoiLuong = 0;

                if (DateTime.TryParse(strEnd, new System.Globalization.CultureInfo("vi-VN"), System.Globalization.DateTimeStyles.None, out dtEnd) ||
                    DateTime.TryParse(strEnd, new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out dtEnd))
                {
                    meetDto.ThoiGianKetThuc = dtEnd;
                    if (int.TryParse(txtThoiLuong.Text.Trim(), out thoiLuong))
                    {
                        meetDto.ThoiGianBatDau = dtEnd.AddMinutes(-thoiLuong);
                    }
                    else
                    {
                        meetDto.ThoiGianBatDau = dtEnd;
                    }
                }
                else
                {
                    ShowNotify($"Lỗi đọc giờ! Chuỗi Server nhận được là: '{strEnd}'", MSGType.Error);
                    return;
                }

                string nhanVienIds = hdfNhanVienIds.Value;
                TblLichHop savedMeet = MeetManager.Instance.CreateOrUpdate(meetDto, nhanVienIds);

                if (savedMeet == null)
                {
                    ShowInvalidDataError();
                    return;
                }
                ShowSuccessSaveData();
                dlDetail.CloseModal();
                CtrlMeet1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void btnMoPopupNhanVien_Click(object sender, EventArgs e)
        {
            _control.BindNhanVienToCheckBoxList(cblNhanVien);
            cblNhanVien.ClearSelection();

            string[] selectedIds = hdfNhanVienIds.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (System.Web.UI.WebControls.ListItem item in cblNhanVien.Items)
            {
                if (Array.Exists(selectedIds, id => id == item.Value))
                {
                    item.Selected = true;
                }
            }

            dlChonNhanVien.OpenModal(true);
        }

        protected void btnXacNhanNhanVien_Click(object sender, EventArgs e)
        {
            List<string> ids = new List<string>();
            List<string> names = new List<string>();

            foreach (System.Web.UI.WebControls.ListItem item in cblNhanVien.Items)
            {
                if (item.Selected)
                {
                    ids.Add(item.Value);
                    names.Add(item.Text);
                }
            }

            string strIds = string.Join(",", ids);
            string strNames = string.Join(", ", names);

            hdfNhanVienIds.Value = strIds;
            txtNhanVienThamGia.Text = strNames;

            string script = $@"
                setTimeout(function() {{
                    $('#{hdfNhanVienIds.ClientID}').val('{strIds}');
                    $('#{txtNhanVienThamGia.ClientID}').val('{strNames}');
                }}, 100);
            ";
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "UpdateUI_NhanVien", script, true);

            dlChonNhanVien.CloseModal();
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlMeet1.ConfirmRequest(e);
        }
    }
}