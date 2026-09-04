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
using System.Transactions;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fMeets
{
    public partial class MeetList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Meet;
        private Guid MeetId
        {
            get => ViewState["MeetId"] != null ? (Guid)ViewState["MeetId"] : Guid.Empty;
            set => ViewState["MeetId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlMeet1.NewMeetingHandlerCallback += NewMeetingAction;
            CtrlMeet1.EditMeetingHandlerCallback += EditMeetingAction;

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
            ddlTrangThai.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
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

            if (meet.ThoiGianBatDau != DateTime.MinValue)
                txtThoiGianBatDau.Text = meet.ThoiGianBatDau.ToString("dd/MM/yyyy HH:mm");

            if (meet.ThoiGianKetThuc != DateTime.MinValue)
                txtThoiGianKetThuc.Text = meet.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm");

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

            lbtSubmit.Visible = false;
            txtTenCuocHop.Text = txtNoiDungCuocHop.Text = txtDiaDiemHop.Text = txtThoiGianBatDau.Text = txtThoiGianKetThuc.Text = "";

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
                using (var scope = new TransactionScope())
                {
                    TblLichHop meet = null;
                    bool isNew = (this.MeetId == Guid.Empty);

                    // Lấy UserID hiện tại (Kiểu Guid). Nếu Context của bác không có UserId mà lưu qua Username, thì lấy ID từ Membership
                    // Code giả định dùng SweetContext.Current.UserId, bác chỉnh lại thuộc tính cho đúng với Framework nhé!
                    Guid currentUserId = SweetContext.Current.UserId;

                    if (isNew)
                    {
                        meet = new TblLichHop();
                        meet.IdLichHop = Guid.NewGuid();
                        meet.IdDuAn = CtrlMeet1.ProjectId;
                        meet.DaXoa = false;
                        meet.NgayTao = DateTime.Now;
                        meet.IdNguoiTao = currentUserId; 
                        // meet.MaLichHop = MeetingManager.Instance.GenerateMaLichHop(CtrlMeet1.ProjectId);
                    }
                    else
                    {
                        meet = TblLichHop.FetchByID(this.MeetId);
                        if (meet == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }
                        meet.NgayCapNhat = DateTime.Now;
                        meet.IdNguoiCapNhat = currentUserId;
                    }

                    meet.TenCuocHop = txtTenCuocHop.Text.Trim();
                    meet.NoiDungCuocHop = !string.IsNullOrEmpty(txtNoiDungCuocHop.Text.Trim()) ? txtNoiDungCuocHop.Text.Trim() : null;
                    meet.DiaDiemHop = txtDiaDiemHop.Text.Trim();

                    DateTime dtStart, dtEnd;
                    if (DateTime.TryParseExact(txtThoiGianBatDau.Text.Trim(), "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out dtStart))
                        meet.ThoiGianBatDau = dtStart;
                    if (DateTime.TryParseExact(txtThoiGianKetThuc.Text.Trim(), "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out dtEnd))
                        meet.ThoiGianKetThuc = dtEnd;

                    int trangThai = 0;
                    if (this.GetValue(ddlTrangThai, out trangThai))
                        meet.TrangThai = (byte)trangThai; 

                    meet.Save();
                    scope.Complete();
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

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlMeet1.ConfirmRequest(e);
        }
    }
}