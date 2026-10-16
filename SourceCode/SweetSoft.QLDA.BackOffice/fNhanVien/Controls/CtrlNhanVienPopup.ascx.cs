using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fNhanVien.Controls
{ 
    public partial class CtrlNhanVienPopup : BaseAdminUserControl
    {
        public EventHandler SavedHandlerCallback;

        private Guid DetailUserId
        {
            get => ViewState["DetailUserId"] != null ? (Guid)ViewState["DetailUserId"] : Guid.Empty;
            set => ViewState["DetailUserId"] = value;
        }

        protected bool IsEditMode => DetailUserId != Guid.Empty;

        private Guid TempAvatarSessionId
        {
            get
            {
                if (ViewState["TempAvatarSessionId"] == null)
                    ViewState["TempAvatarSessionId"] = Guid.NewGuid();
                return (Guid)ViewState["TempAvatarSessionId"];
            }
        }

        protected void Page_Load(object sender, EventArgs e) { }

        public void InitControls()
        {
            ApplyControlsText();
            new ControlHelpers().BindRoles(ddlRole);
            new ControlHelpers().BindChucDanh(ddlChucDanh);
            new ControlHelpers().BindPhongBan(ddlPhongBan);
            new ControlHelpers().BindTinhThanh(ddlTinhThanh);
        }

        private void ApplyControlsText()
        {
            txtFullName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtUserName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtEmail.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtPhone.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            //txtCCCD.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtDiaChi.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            //txtPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            //txtConfirmPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            ddlRole.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlPhongBan.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlChucDanh.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlTinhThanh.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlXaPhuong.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            
            //chkStatus.OnText = GetResourceText(BackEndResourceKeys.WORKING);
            //chkStatus.OffText = GetResourceText(BackEndResourceKeys.RESIGNED);
        }

        private string ExtractAvatarFromFilesBox(string currentAvatar)
        {
            string newAvatar = currentAvatar;
            bool isDeleted = false;

            foreach (string key in Request.Params.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (key.EndsWith("txtArFileRemove") && !string.IsNullOrEmpty(Request.Params[key])) isDeleted = true;
                if (key.Contains("filePath$")) newAvatar = Request.Params[key];
            }

            if (isDeleted && newAvatar == currentAvatar) return "";
            return newAvatar;
        }

        private bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_");
        }

        private void RefreshAccountInfor()
        {
            this.DetailUserId = Guid.Empty;
            ViewState["TempAvatarSessionId"] = Guid.NewGuid();

            txtUserName.Text = txtPhone.Text = txtFullName.Text = txtEmail.Text = string.Empty;
            txtDiaChi.Text = string.Empty;
            txtNgaySinh.Text = txtNgayGiaNhap.Text = string.Empty;

            ddlGioiTinh.SelectedIndex = ddlRole.SelectedIndex = ddlPhongBan.SelectedIndex = ddlChucDanh.SelectedIndex = 0;

            // Reset phần địa chỉ
            ddlTinhThanh.ClearSelection();
            new ControlHelpers().BindXaPhuong(ddlXaPhuong, "");
            upnlDiaChi.Update();
            chkEnableAccount.Checked = false;
            chkEnableAccount.Disabled = false;
            txtUserName.Enabled = true;
            btnGenUser.Visible = true;
            txtUserName.Required = false;

            boxAccountInfo.Style["display"] = "none";

            // "Biến hình" khu vực mật khẩu về trạng thái Thêm Mới
            divPasswordAutoInfo.Visible = true;
            divResetPasswordBtn.Visible = false;
            lblUserCheckMsg.Visible = false;
            upnlUserName.Update();
        }

        private void LoadDefaultAvatar()
        {
            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar);
        }

        public void AddNew()
        {
            RefreshAccountInfor();
            LoadDefaultAvatar();

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.OpenModal(true);
        }

        public void Edit(Guid userId)
        {
            if (userId == Guid.Empty) return;
            RefreshAccountInfor();

            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || user.IsDeleted) return;

            this.DetailUserId = user.UserId;

            txtFullName.Text = user.DisplayName;
            txtPhone.Text = user.MobileAlias;
            //chkStatus.Checked = user.IsActivated;
            //txtCCCD.Text = user.IdCCCD;
            txtDiaChi.Text = user.DiaChi;
            // Load Tỉnh / Xã
            ddlTinhThanh.ClearSelection();
            new ControlHelpers().BindXaPhuong(ddlXaPhuong, "");
            if (!string.IsNullOrEmpty(user.MaTinh))
            {
                ddlTinhThanh.SelectedValue = user.MaTinh;
                new ControlHelpers().BindXaPhuong(ddlXaPhuong, user.MaTinh);
                if (!string.IsNullOrEmpty(user.MaXa))
                {
                    ddlXaPhuong.SelectedValue = user.MaXa;
                }
            }
            upnlDiaChi.Update();
            if (user.NgaySinh.HasValue) txtNgaySinh.Text = user.NgaySinh.Value.ToString("yyyy-MM-dd");
            if (user.NgayGiaNhap.HasValue) txtNgayGiaNhap.Text = user.NgayGiaNhap.Value.ToString("yyyy-MM-dd");
            if (user.IdPhongBan.HasValue) ddlPhongBan.SelectedValue = user.IdPhongBan.Value.ToString();
            if (user.IdChucDanh.HasValue) ddlChucDanh.SelectedValue = user.IdChucDanh.Value.ToString();
            if (!string.IsNullOrEmpty(user.GioiTinh)) ddlGioiTinh.SelectedValue = user.GioiTinh;

            MembershipUser memUser = Membership.GetUser(user.UserName);
            if (memUser != null && !memUser.Email.Contains("no-email.com"))
                txtEmail.Text = memUser.Email;

            bool hasAccount = !IsGhostAccount(user.UserName);
            if (!hasAccount)
            {
                txtUserName.Text = string.Empty;
                chkEnableAccount.Checked = false;
                chkEnableAccount.Disabled = false;
                txtUserName.Enabled = true;
                btnGenUser.Visible = true;
                txtUserName.Required = false;
                boxAccountInfo.Style["display"] = "none";
                //divChangePassword.Visible = false;
                //divPassword.Visible = false;
                //divPassword.Attributes["data-edit"] = "false";
                divPasswordAutoInfo.Visible = true;
                divResetPasswordBtn.Visible = false;

            }
            else
            {
                txtUserName.Text = user.UserName;
                chkEnableAccount.Checked = true;
                chkEnableAccount.Disabled = true;
                txtUserName.Enabled = false;
                btnGenUser.Visible = false;
                txtUserName.Required = true;
                boxAccountInfo.Style["display"] = "block";
                //divChangePassword.Visible = true;
                //divPassword.Visible = true;
                //divPassword.Attributes["data-edit"] = "true";
                divPasswordAutoInfo.Visible = false;
                divResetPasswordBtn.Visible = true;
                lblUserCheckMsg.Visible = false;
                upnlUserName.Update();
            }

            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null) ddlRole.SelectedValue = role.RoleId.ToString();
            else ddlRole.SelectedIndex = 0;

            fbImage.SingleFilePath = string.IsNullOrEmpty(user.Avatar) ? "/Styles/images/user-icon.png" : user.Avatar;
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar);      
            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }
        protected void ddlTinhThanh_SelectedValueChanged(object sender, EventArgs e)
        {
            string maTinh = ddlTinhThanh.SelectedValue;
            new ControlHelpers().BindXaPhuong(ddlXaPhuong, maTinh);
        }
        protected void lbtResetPassword_Click(object sender, EventArgs e)
        {
            if (this.DetailUserId == Guid.Empty) return;

            AspnetUser user = UserManager.Instance.GetUserById(this.DetailUserId);
            if (user == null || IsGhostAccount(user.UserName)) return;

            ConfirmResult result = new ConfirmResult();
            result.CommandName = "USER_RESET_PASSWORD";
            result.Value = user;
            this.CURRENT_PAGE.CurrentConfirmResult = result;

            MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                , string.Format("Bạn có chắc chắn muốn cấp lại mật khẩu cho nhân viên {0}?", user.DisplayName)
                , MSGButton.Send, MSGIcon.Warning);
            OpenMessageBox(msg, result, false, false);
        }
        protected void btnGenUser_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                ScriptManager.RegisterStartupScript(this.Page, GetType(), "warnName", "alert('Vui lòng nhập Họ và tên trước khi tự tạo Tên đăng nhập!');", true);
                return;
            }

            // 1. Chuyển đổi tên thành chuỗi Username (Bỏ dấu, xóa khoảng trắng, viết thường)
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = fullName.Normalize(System.Text.NormalizationForm.FormD);
            string str = regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            str = System.Text.RegularExpressions.Regex.Replace(str, "[^a-zA-Z0-9]", "").ToLower();

            txtUserName.Text = str;

            // 2. CHỌC XUỐNG DB KIỂM TRA TRÙNG LẶP
            bool isExist = UserManager.Instance.IsUserNameExist(this.DetailUserId, str);

            // 3. HIỂN THỊ SUBTEXT TƯƠNG ỨNG
            if (isExist)
            {
                lblUserCheckMsg.Text = "<i class='fas fa-exclamation-triangle me-1'></i> Tên đăng nhập này đã bị trùng!";
                lblUserCheckMsg.CssClass = "text-danger mt-1 d-block fw-bold";
                lblUserCheckMsg.Visible = true;
            }
            else
            {
                lblUserCheckMsg.Text = "<i class='fas fa-check-circle me-1'></i> Tên đăng nhập hợp lệ.";
                lblUserCheckMsg.CssClass = "text-success mt-1 d-block fw-bold";
                lblUserCheckMsg.Visible = true;
            }

            // Ép UpdatePanel chớp để cập nhật UI
            upnlUserName.Update();
        }
        // Bắt sự kiện Đồng ý từ MessageBox
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null)
            {
                if (e.CommandName.Contains("USER_RESET_PASSWORD"))
                {
                    AspnetUser user = e.Value as AspnetUser;
                    if (user == null) return;

                    try
                    {
                        bool isDefault = false;
                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["IsUsedDefaultPassword"] != null)
                            isDefault = bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["IsUsedDefaultPassword"]);

                        string newPassword = isDefault ? this.CURRENT_PAGE.DefaultPassword : SecurityUtilities.CreateAlphaNumericString(8);

                        // 1. KÉO DTO LÊN
                        AspnetUser dto = UserManager.Instance.GetUserById(user.UserId);
                        if (dto == null) return;

                        // ==============================================================
                        // 2. VÁ NHỮNG LỖ THỦNG DỮ LIỆU ĐỂ VƯỢ QUA BUSINESS VALIDATOR
                        // ==============================================================
                        // Vá lỗ thủng 1: Email (Thủ phạm gây ra lỗi "Vui lòng nhập giá trị")
                        MembershipUser memUser = Membership.GetUser(dto.UserName);
                        if (memUser != null)
                        {
                            dto.Email = memUser.Email;
                        }

                        // Vá lỗ thủng 2: RoleId
                        AspnetRole role = RoleManager.Instance.GetRoleByUserId(dto.UserId);
                        dto.RoleId = role != null ? role.RoleId : Guid.Empty;

                        dto.LaNhanVien = true;

                        // 3. KÍCH HOẠT ĐỘNG CƠ CỦA CREATE_OR_UPDATE
                        dto.Password = newPassword;
                        var result = UserManager.Instance.CreateOrUpdate(dto);

                        // 4. THÔNG BÁO UI
                        if (result != null)
                        {
                            dlDetail.CloseModal();
                            SavedHandlerCallback?.Invoke(this, EventArgs.Empty);

                            this.CURRENT_PAGE.ShowNotify(GetResourceText(BackEndResourceKeys.THE_NEW_PASSWORD_HAS_BEEN_SENT_TO_THE_ACCOUNT_S_EMAIL_ADDRESS), MSGType.Success);
                        }
                        else
                        {
                            this.CURRENT_PAGE.ShowNotify("Hệ thống lỗi! Không thể cấp lại mật khẩu.", MSGType.Error);
                        }
                    }
                    catch (Exception exc)
                    {
                        this.CURRENT_PAGE.ShowNotify(exc.Message.Replace("'", "\\'").Replace("\n", ""), MSGType.Error);
                    }
                }
            }
        }
        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                // =========================================================
                // LOGIC CHỐT CHẶN: KIỂM TRA TRÙNG EMAIL (2 BƯỚC RÕ RÀNG)
                // =========================================================
                string inputEmail = string.IsNullOrEmpty(txtEmail.Text) ? string.Empty : txtEmail.Text.Trim();
                if (!string.IsNullOrEmpty(inputEmail))
                {
                    // BƯỚC 1: Kiểm tra xem có trùng với tài khoản QUẢN TRỊ nào không
                    if (UserManager.Instance.IsEmailExistInAdminGroup(DetailUserId, inputEmail))
                    {
                        validationEngine.AddErrorPrompt(txtEmail.ClientID, "Email bị trùng với một tài khoản quản trị");
                        validationEngine.ShowErrorPrompt();
                        return; // Ngắt luồng ngay tại đây
                    }
                    // CHẶN LỖI: Server-side validation cho Username khi bật Cấp tài khoản
                    if (chkEnableAccount.Checked && string.IsNullOrEmpty(txtUserName.Text.Trim()))
                    {
                        validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                    }
                    // BƯỚC 2: Kiểm tra xem có trùng với BẤT KỲ tài khoản nào khác không
                    // (Lúc này nếu trùng thì chắc chắn là trùng với Nhân viên khác, vì Admin đã lọt qua Bước 1)
                    if (UserManager.Instance.IsEmailExist(DetailUserId, inputEmail))
                    {
                        // Sử dụng Resource đa ngôn ngữ gốc của hệ thống ông cho đồng bộ
                        validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.EMAIL_ALREADY_EXISTS));
                        validationEngine.ShowErrorPrompt();
                        return; // Ngắt luồng ngay tại đây
                    }
                }

                AspnetUser dto = IsEditMode ? UserManager.Instance.GetUserById(DetailUserId) : new AspnetUser();
                if (dto == null) { ShowInvalidDataError(); return; }

                dto.LaNhanVien = true;

                //if (string.IsNullOrEmpty(txtCCCD.Text.Trim()))
                //    validationEngine.AddErrorPrompt(txtCCCD.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));

                bool hasAccount = false;
                if (IsEditMode) hasAccount = !IsGhostAccount(dto.UserName);

                // (Bên trong lbtSubmit_Click)
                dto.DisplayName = txtFullName.Text.Trim();
                dto.Email = inputEmail;
                dto.MobileAlias = txtPhone.Text.Trim();

                string oldAvatar = IsEditMode ? dto.Avatar : "";
                dto.Avatar = ExtractAvatarFromFilesBox(oldAvatar);

                dto.DiaChi = txtDiaChi.Text.Trim();

                // CHẶN LỖI 1: Xử lý chuỗi rỗng thành NULL
                dto.MaTinh = string.IsNullOrEmpty(ddlTinhThanh.SelectedValue) ? null : ddlTinhThanh.SelectedValue;
                dto.MaXa = string.IsNullOrEmpty(ddlXaPhuong.SelectedValue) ? null : ddlXaPhuong.SelectedValue;
                dto.GioiTinh = string.IsNullOrEmpty(ddlGioiTinh.SelectedValue) ? null : ddlGioiTinh.SelectedValue;

                // CHẶN LỖI 2: Ép Null nếu TryParse thất bại (Admin cố tình xóa trắng form)
                if (DateTime.TryParse(txtNgaySinh.Text, out DateTime tempDOB)) dto.NgaySinh = tempDOB; else dto.NgaySinh = null;
                if (DateTime.TryParse(txtNgayGiaNhap.Text, out DateTime tempJoin)) dto.NgayGiaNhap = tempJoin; else dto.NgayGiaNhap = null;
                if (Guid.TryParse(ddlPhongBan.SelectedValue, out Guid idPB) && idPB != Guid.Empty) dto.IdPhongBan = idPB; else dto.IdPhongBan = null;
                if (Guid.TryParse(ddlChucDanh.SelectedValue, out Guid idCD) && idCD != Guid.Empty) dto.IdChucDanh = idCD; else dto.IdChucDanh = null;
                if (!chkEnableAccount.Checked)
                {
                    dto.UserName = null;
                    dto.Password = null;
                    dto.RoleId = Guid.Empty;
                }
                else
                {
                    if (!hasAccount)
                    {
                        dto.UserName = txtUserName.Text.Trim();
                        dto.Password = null; // UserManager sẽ tự động sinh
                    }
                    else
                    {
                        dto.UserName = txtUserName.Text.Trim();
                        dto.Password = null; // Ta không cho đổi pass khi cập nhật nữa, chuyển qua dùng nút Cấp lại
                    }
                    if (Guid.TryParse(ddlRole.SelectedValue, out Guid rId) && rId != Guid.Empty) dto.RoleId = rId;
                    else dto.RoleId = Guid.Empty;
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                var result = UserManager.Instance.CreateOrUpdate(dto);
                if (result == null) { ShowInvalidDataError(); return; }

                if (result.Avatar != oldAvatar && !string.IsNullOrEmpty(result.Avatar))
                {
                    new SubSonic.Update(TblUploadFile.Schema)
                        .Set(TblUploadFile.Columns.RefId).EqualTo(result.UserId)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(result.Avatar)
                        .Execute();

                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(result.UserId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(oldAvatar)
                        .Execute();

                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .Execute();
                }

                if (result.UserId == SweetContext.Current.UserId)
                {
                    SweetContext.Current.User = null;
                    if (this.Page.Master is SweetSoft.QLDA.BackOffice.MasterPages.MasterTemplate master)
                        master.SetUserInfomation(result.DisplayName, result.Avatar);
                }

                ShowNotify(IsEditMode ? GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY) : GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY), MSGType.Success);
                dlDetail.CloseModal();
                SavedHandlerCallback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " "), MSGType.Error);
            }
        }
    }
}