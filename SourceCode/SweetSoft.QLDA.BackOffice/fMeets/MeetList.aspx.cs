using SubSonic;
using SubSonic.Sugar;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlMeet1.NewMeetingHandlerCallback += NewMeetingAction;
            CtrlMeet1.EditMeetingHandlerCallback += EditMeetingAction;
            CtrlMeet1.OpenMeetingFilesHandlerCallback += OpenMeetingFilesAction;
            fbMeetingFiles.CurrentFileIdResolver = (recordId, refType) => ProjectRecordFileAccess.GetLinkedFileId(recordId, refType.ToString());
            fbMeetingFiles.FileMutationValidator = (recordId, refType, fileId) => ProjectRecordFileAccess.CanAccess(SweetContext.Current.UserId, recordId, refType.ToString(), true) && ProjectRecordFileAccess.BelongsToRecord(recordId, refType.ToString(), fileId);

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
            btnXacNhanNhanVien.Text = GetResourceText(BackEndResourceKeys.CONFIRM);
            txtThoiGianKetThuc.PlaceHolder = "--";
            txtTenCuocHop.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtDiaDiemHop.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtGioBatDau.PlaceHolder = "Chọn giờ...";
            txtThoiLuong.PlaceHolder = "Chọn thời lượng...";
            txtNgayBatDau.PlaceHolder = "Ngày...";
            dlChonNhanVien.Title = GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE);
        }

        private void SetRecommendedMeetingStart()
        {
            DateTime now = DateTime.Now;
            DateTime recommended = now.Minute == 0 && now.Second == 0
                ? new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)
                : new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
            txtNgayBatDau.DateValue = recommended.Date;
            txtGioBatDau.Text = recommended.ToString("HH:mm");
            txtThoiLuong.Text = "60";
        }

        private void OpenMeetingFilesAction(object sender, EventArgs e)
        {
            Guid idLichHop = sender is Guid ? (Guid)sender : Guid.Empty;
            if (idLichHop == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            TblLichHop meeting = TblLichHop.FetchByID(idLichHop);
            if (meeting == null || meeting.DaXoa == true || meeting.IdDuAn != CurrentProjectId)
            {
                ShowInvalidDataError();
                return;
            }
            if (!ProjectRecordFileAccess.CanAccess(SweetContext.Current.UserId, idLichHop, FileUploadTypes.MeetingAttachment.ToString(), false))
            {
                ShowAccessDeniedNotify();
                return;
            }
            fbMeetingFiles.IsMultiple = false;
            fbMeetingFiles.IsEnabled = ProjectRecordFileAccess.CanAccess(SweetContext.Current.UserId, idLichHop, FileUploadTypes.MeetingAttachment.ToString(), true);
            fbMeetingFiles.LoadFile(idLichHop, FileUploadTypes.MeetingAttachment);
            dlMeetingFiles.OpenModal(true);
        }

        private void NewMeetingAction(object sender, EventArgs e)
        {
            RefreshMeetingInfo();
            SetRecommendedMeetingStart();
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
            BindSelectedNhanVienDisplay();

            if (meet.ThoiGianKetThuc != DateTime.MinValue)
            {
                // Lấy thời gian kết thúc làm mốc chính.
                DateTime endDateTime = meet.ThoiGianKetThuc;

                // Hiển thị thời gian kết thúc vào TextBox.
                txtThoiGianKetThuc.Text = endDateTime.ToString("dd/MM/yyyy HH:mm");

                // Tính thời lượng từ thời gian bắt đầu cũ và thời gian kết thúc.
                int thoiLuong = 0;

                if (meet.ThoiGianBatDau != DateTime.MinValue)
                {
                    thoiLuong = (int)(meet.ThoiGianKetThuc - meet.ThoiGianBatDau).TotalMinutes;
                }

                if (thoiLuong > 0)
                {
                    txtThoiLuong.Text = thoiLuong.ToString();

                    // Tính lại thời gian bắt đầu:
                    // Thời gian bắt đầu = Thời gian kết thúc - Thời lượng
                    DateTime startDateTime = endDateTime.AddMinutes(-thoiLuong);

                    txtNgayBatDau.DateValue = startDateTime.Date;
                    txtGioBatDau.Text = startDateTime.ToString("HH:mm");
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
            txtNgayBatDau.DateValue = null;
            txtGioBatDau.Text = "";
            txtThoiLuong.Text = "";
            txtThoiGianKetThuc.Text = "";
            txtNhanVienThamGia.Text = "";
            hdfNhanVienIds.Value = "";
            ClearSelectedNhanVienDisplay();
            lbtSubmit.Visible = false;
            if (ddlTrangThai.Items.Count > 0)
                ddlTrangThai.SelectedIndex = 0;
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
                bool isNew = this.MeetId == Guid.Empty;

                if (!isNew)
                    meetDto.IdLichHop = this.MeetId;

                meetDto.IdDuAn = CtrlMeet1.ProjectId;
                meetDto.TenCuocHop = txtTenCuocHop.Text.Trim();
                meetDto.NoiDungCuocHop = !string.IsNullOrEmpty(txtNoiDungCuocHop.Text.Trim())
                    ? txtNoiDungCuocHop.Text.Trim()
                    : null;
                meetDto.DiaDiemHop = txtDiaDiemHop.Text.Trim();

                if (!DateTime.TryParseExact(
                    txtThoiGianKetThuc.Text.Trim(),
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime endDateTime))
                {
                    ShowNotify("Vui lòng nhập thời gian kết thúc hợp lệ!", MSGType.Error);
                    return;
                }

                if (!int.TryParse(txtThoiLuong.Text.Trim(), out int thoiLuong) || thoiLuong <= 0)
                {
                    ShowNotify("Thời lượng phải lớn hơn 0.", MSGType.Error);
                    return;
                }

                DateTime startDateTime = endDateTime.AddMinutes(-thoiLuong);

                meetDto.ThoiGianBatDau = startDateTime;
                meetDto.ThoiGianKetThuc = endDateTime;

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

                if (isNew && this.IsEdit)
                    OpenMeetingFilesAction(savedMeet.IdLichHop, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify("Lưu thất bại: " + exc.Message, MSGType.Error);
            }
        }

        protected void btnMoPopupNhanVien_Click(object sender, EventArgs e)
        {
            DataTable dtUsers = ThanhVienDuAnManager.Instance.GetThanhVienDuAnDetail(CtrlMeet1.ProjectId);
            var list = new List<object>();

            for (int i = 0; i < dtUsers.Rows.Count; i++)
            {
                DataRow row = dtUsers.Rows[i];
                Guid userId = (Guid)row["UserId"];
                string displayName = row["DisplayName"] != DBNull.Value ? row["DisplayName"].ToString() : "";
                string email = row["Email"] != DBNull.Value ? row["Email"].ToString() : "";
                string avatar = row["Avatar"] != DBNull.Value ? row["Avatar"].ToString() : "";

                list.Add(new
                {
                    UserId = userId,
                    DisplayName = displayName,
                    Email = email,
                    AvatarHtml = GetSingleAvatarHtml(displayName, avatar, i)
                });
            }

            rptNhanVien.DataSource = list;
            rptNhanVien.DataBind();
            // Giữ txtNhanVienThamGia chỉ là text thuần để không bị request validation
            // khi postback; phần card avatar được cập nhật riêng bằng JavaScript.
            BindSelectedNhanVienDisplay();
            dlChonNhanVien.OpenModal(true);
        }

        protected void rptNhanVien_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HiddenField hdfUserId = (HiddenField)e.Item.FindControl("hdfUserId");
                CheckBox chkSelect = (CheckBox)e.Item.FindControl("chkSelect");

                if (hdfUserId != null && chkSelect != null)
                {
                    string[] selectedIds = hdfNhanVienIds.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Array.Exists(selectedIds, id => id == hdfUserId.Value))
                        chkSelect.Checked = true;
                }
            }
        }

        protected void btnXacNhanNhanVien_Click(object sender, EventArgs e)
        {
            List<string> ids = new List<string>();
            List<string> names = new List<string>();
            foreach (RepeaterItem item in rptNhanVien.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem)
                    continue;
                CheckBox chkSelect = (CheckBox)item.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    HiddenField hdfUserId = (HiddenField)item.FindControl("hdfUserId");
                    HiddenField hdfDisplayName = (HiddenField)item.FindControl("hdfDisplayName");
                    ids.Add(hdfUserId.Value);
                    names.Add(hdfDisplayName.Value);
                }
            }
            string strIds = string.Join(",", ids);
            string strNames = string.Join(", ", names);
            hdfNhanVienIds.Value = strIds;
            // TextBox chỉ lưu text thuần. Không đưa HTML vào đây vì ASP.NET Request Validation
            // sẽ chặn HTML ở postback tiếp theo (đây chính là nguyên nhân popup bị 500).
            txtNhanVienThamGia.Text = strNames;
            BindSelectedNhanVienDisplay();
            string safeIds = System.Web.HttpUtility.JavaScriptStringEncode(strIds);
            string script = $@"setTimeout(function() {{ $('#{hdfNhanVienIds.ClientID}').val('{safeIds}'); }}, 100);";
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "UpdateNhanVienIds", script, true);
            dlChonNhanVien.CloseModal();
        }

        private void BindSelectedNhanVienDisplay()
        {
            string selectedEmployeeIds = hdfNhanVienIds.Value ?? string.Empty;
            DataTable dtUsers = ThanhVienDuAnManager.Instance.GetThanhVienDuAnDetail(CtrlMeet1.ProjectId);
            HashSet<Guid> selectedIds = new HashSet<Guid>();

            foreach (string rawId in selectedEmployeeIds.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Guid id;
                if (Guid.TryParse(rawId.Trim(), out id) && id != Guid.Empty)
                    selectedIds.Add(id);
            }

            List<string> memberHtml = new List<string>();
            int avatarIndex = 0;

            for (int i = 0; i < dtUsers.Rows.Count; i++)
            {
                DataRow row = dtUsers.Rows[i];
                if (row["UserId"] == DBNull.Value)
                    continue;

                Guid userId;
                if (!Guid.TryParse(row["UserId"].ToString(), out userId) || !selectedIds.Contains(userId))
                    continue;

                string displayName = row["DisplayName"] != DBNull.Value ? row["DisplayName"].ToString() : string.Empty;
                string email = row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty;
                string avatar = row["Avatar"] != DBNull.Value ? row["Avatar"].ToString() : string.Empty;
                string avatarHtml = GetSingleAvatarHtml(displayName, avatar, avatarIndex++);

                string safeName = System.Web.HttpUtility.HtmlEncode(displayName);
                string safeEmail = System.Web.HttpUtility.HtmlEncode(email);

                memberHtml.Add(
                    "<div class='selected-meeting-member-row'>" +
                    "<div class='selected-meeting-member-avatar'>" + avatarHtml + "</div>" +
                    "<div class='selected-meeting-member-info'>" +
                    "<div class='selected-meeting-member-name'>" + safeName + "</div>" +
                    "<div class='selected-meeting-member-email'>" + safeEmail + "</div>" +
                    "</div>" +
                    "</div>");
            }

            string content = memberHtml.Count == 0
                ? "<div class='selected-meeting-members-empty'><i class='fas fa-user-friends'></i><span>Chưa có nhân viên tham gia cuộc họp.</span></div>"
                : "<div class='selected-meeting-members-list'>" + string.Join(string.Empty, memberHtml) + "</div>";

            string safeHtml = System.Web.HttpUtility.JavaScriptStringEncode(content);
            string script = $@"setTimeout(function() {{ var display = $('#selectedMeetingMembers'); if (display.length) display.html('{safeHtml}'); }}, 50);";
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "UpdateSelectedMeetingMembers", script, true);
        }

        private void ClearSelectedNhanVienDisplay()
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                this.Page.GetType(),
                "ClearSelectedMeetingMembers",
                "setTimeout(function(){ var display = $('#selectedMeetingMembers'); if(display.length) display.html(\"<div class='selected-meeting-members-empty'><i class='fas fa-user-friends'></i><span>Chưa có nhân viên tham gia cuộc họp.</span></div>\"); }, 50);",
                true);
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetSingleAvatarHtml(string name, string avatar, int index)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            string color = colors[index % colors.Length];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string safeName = System.Web.HttpUtility.HtmlAttributeEncode(name ?? string.Empty);
                string safeInitials = System.Web.HttpUtility.HtmlEncode(GetInitials(name));
                string fallbackHtml = $"<div class='single-avatar-circle' style='background-color: {color};' title='{safeName}'>{safeInitials}</div>";
                string fallbackJs = System.Web.HttpUtility.JavaScriptStringEncode(fallbackHtml);
                string safeAvatarUrl = System.Web.HttpUtility.HtmlAttributeEncode(avatarUrl);
                return $"<img src='{safeAvatarUrl}' class='single-avatar-circle' style='object-fit: cover;' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{fallbackJs}';\" />";
            }

            return $"<div class='single-avatar-circle' style='background-color: {color};'>{GetInitials(name)}</div>";
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlMeet1.ConfirmRequest(e);
        }
    }
}
