using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;
using SweetSoft.QLDA.Core.Utils;
namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public enum UserPopupMode { SystemUser, Employee }

    public partial class CtrlUserDetail : BaseAdminUserControl
    {
        public EventHandler SavedHandlerCallback;

        public UserPopupMode CurrentMode
        {
            get => ViewState["CurrentMode"] != null ? (UserPopupMode)ViewState["CurrentMode"] : UserPopupMode.SystemUser;
            set => ViewState["CurrentMode"] = value;
        }

        private Guid DetailUserId
        {
            get => ViewState["DetailUserId"] != null ? (Guid)ViewState["DetailUserId"] : Guid.Empty;
            set => ViewState["DetailUserId"] = value;
        }

        protected bool IsEditMode => DetailUserId != Guid.Empty;

        public void InitControls()
        {
            new ControlHelpers().BindRoles(ddlRole);
            new ControlHelpers().BindChucDanh(ddlChucDanh);
            new ControlHelpers().BindPhongBan(ddlPhongBan);
        }

        private void SetUIByMode()
        {
            if (CurrentMode == UserPopupMode.SystemUser)
            {
                // Mode Tài khoản: Ẩn Nhân sự, Ẩn Công tắc, Buộc hiện Tài khoản
                boxEmployeeInfo.Visible = false;
                divToggleAccount.Visible = false;
                boxAccountInfo.Style["display"] = "block";

                txtUserName.Required = true;
            }
            else
            {
                // Mode Nhân sự: Hiện Khối Nhân sự, Hiện Công tắc
                boxEmployeeInfo.Visible = true;
                divToggleAccount.Visible = true;

                // Mặc định ẩn Khối Account nếu chưa check công tắc
                if (!chkEnableAccount.Checked)
                {
                    boxAccountInfo.Style["display"] = "none";
                    txtUserName.Required = false;
                }
                else
                {
                    boxAccountInfo.Style["display"] = "block";
                    txtUserName.Required = true;
                }
            }
        }

        public void AddNew()
        {
            this.DetailUserId = Guid.Empty;

            // Dọn rác các textbox
            txtUserName.Text = txtPhone.Text = txtFullName.Text = txtEmail.Text = string.Empty;
            txtPassword.Text = txtConfirmPassword.Text = txtCCCD.Text = txtDiaChi.Text = string.Empty;
            txtNgaySinh.Text = txtNgayGiaNhap.Text = string.Empty;

            ddlGioiTinh.SelectedIndex = ddlRole.SelectedIndex = ddlPhongBan.SelectedIndex = ddlChucDanh.SelectedIndex = 0;
            chkStatus.Checked = true;

            // XỬ LÝ NÚT GẠT: Nếu là SystemUser thì ép BẬT, Employee thì mặc định TẮT
            chkEnableAccount.Checked = CurrentMode == UserPopupMode.SystemUser;
            chkEnableAccount.Disabled = false; // Luôn mở khóa cho Admin thao tác

            txtUserName.Enabled = true; // Mở khóa TextBox & Icon Dấu sét

            // QUAN TRỌNG: ẨN HOÀN TOÀN KHỐI MẬT KHẨU Ở CHẾ ĐỘ THÊM MỚI
            divChangePassword.Visible = false;
            divPassword.Visible = false;
            divPassword.Attributes["data-edit"] = "false";

            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            // Cấp 1 Guid tạm để vượt qua khâu Validate định dạng của UploadHandler
            fbImage.LoadFile(Guid.NewGuid(), FileUploadTypes.UserAvatar);
            SetUIByMode();

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.OpenModal(true);
        }

        public void Edit(Guid userId)
        {
            if (userId == Guid.Empty) return;

            // ĐÃ SỬA: Gọi đúng chuẩn Manager của ông
            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || user.IsDeleted) return;

            this.DetailUserId = user.UserId;

            // 1. Load Khối A (Thông tin chung)
            txtFullName.Text = user.DisplayName;
            txtPhone.Text = user.MobileAlias;
            chkStatus.Checked = user.IsActivated; // HR Status

            fbImage.SingleFilePath = string.IsNullOrEmpty(user.Avatar) ? "/Styles/images/user-icon.png" : user.Avatar;
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(user.UserId, FileUploadTypes.UserAvatar);

            MembershipUser memUser = Membership.GetUser(user.UserName);
            if (memUser != null && !memUser.Email.Contains("no-email.com"))
                txtEmail.Text = memUser.Email;

            // 2. Load Khối C (Nhân sự) - Load trước để UI đồng bộ
            if (CurrentMode == UserPopupMode.Employee)
            {
                txtCCCD.Text = user.IdCCCD;
                txtDiaChi.Text = user.DiaChi;
                if (user.NgaySinh.HasValue) txtNgaySinh.Text = user.NgaySinh.Value.ToString("yyyy-MM-dd");
                if (user.NgayGiaNhap.HasValue) txtNgayGiaNhap.Text = user.NgayGiaNhap.Value.ToString("yyyy-MM-dd");
                if (user.IdPhongBan.HasValue) ddlPhongBan.SelectedValue = user.IdPhongBan.Value.ToString();
                if (user.IdChucDanh.HasValue) ddlChucDanh.SelectedValue = user.IdChucDanh.Value.ToString();
                if (!string.IsNullOrEmpty(user.GioiTinh)) ddlGioiTinh.SelectedValue = user.GioiTinh;
            }

            // ==========================================================
            // MỤC 3: XỬ LÝ TRẠNG THÁI TÀI KHOẢN (REAL vs GHOST)
            // ==========================================================
            bool hasAccount = !IsGhostAccount(user.UserName);

            if (!hasAccount)
            {
                // A. TRƯỜNG HỢP: TÀI KHOẢN MA (GHOST ACCOUNT)
                txtUserName.Text = string.Empty;   // Xóa sạch tiền tố EMP_... trên UI
                chkEnableAccount.Checked = false;  // Đang là Ghost nên chưa được cấp quyền
                chkEnableAccount.Disabled = false; // CHỐT: Cho phép Admin gạt Bật để Nâng cấp!

                txtUserName.Enabled = true; // Mở khóa TextBox & Icon Dấu sét

                divChangePassword.Visible = false; // Ẩn tính năng Đổi pass thủ công
                divPassword.Visible = false;
            }
            else
            {
                // B. TRƯỜNG HỢP: TÀI KHOẢN THẬT (REAL ACCOUNT)
                txtUserName.Text = user.UserName;
                chkEnableAccount.Checked = true;
                chkEnableAccount.Disabled = true; // KHÓA CỨNG: Cấm tắt đi để hạ cấp về Ghost

                txtUserName.Enabled = false; // KHÓA CỨNG: Không cho đổi Username nữa

                divChangePassword.Visible = true; // Hiện Checkbox cho phép đổi pass thủ công
                divPassword.Visible = true;
                divPassword.Attributes["data-edit"] = "true"; // Ẩn ô nhập pass đi, khi nào check mới hiện
            }

            // Gán Role
            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null) ddlRole.SelectedValue = role.RoleId.ToString();
            else ddlRole.SelectedIndex = 0;

            SetUIByMode();

            // JS đảm bảo ẩn pass khi mới mở form
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "HidePwd", "$('[data-selector=\"password\"]').removeClass('show');", true);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION);
            lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true, 1000);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                AspnetUser dto = IsEditMode ? UserManager.Instance.GetUserById(DetailUserId) : new AspnetUser();
                // Phân luồng theo Mode
                if (CurrentMode == UserPopupMode.SystemUser)
                {
                    dto.LaNhanVien = false;
                }
                else
                {
                    dto.LaNhanVien = true;
                    if (string.IsNullOrEmpty(txtCCCD.Text.Trim()))
                        validationEngine.AddErrorPrompt(txtCCCD.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }
                // MỤC 4: BƠM DỮ LIỆU TÀI KHOẢN VÀ VALIDATE MẬT KHẨU
                // 1. Xác định trạng thái ban đầu của user (trước khi bấm lưu)
                bool hasAccount = false;
                if (IsEditMode && dto != null)
                    hasAccount = !IsGhostAccount(dto.UserName);
                // 2. Map Dữ liệu Khối A & C
                dto.DisplayName = txtFullName.Text.Trim();
                dto.Email = string.IsNullOrEmpty(txtEmail.Text) ? $"{DateTime.UtcNow.Ticks}@no-email.com" : txtEmail.Text.Trim();
                dto.MobileAlias = txtPhone.Text.Trim();
                dto.IsActivated = chkStatus.Checked; // HR Status
                dto.Avatar = (fbImage.SingleFilePath.Contains("no-file.png")) ? "" : fbImage.SingleFilePath;
                if (dto.LaNhanVien)
                {
                    dto.IdCCCD = txtCCCD.Text.Trim();
                    dto.DiaChi = txtDiaChi.Text.Trim();
                    dto.GioiTinh = ddlGioiTinh.SelectedValue;
                    if (DateTime.TryParse(txtNgaySinh.Text, out DateTime tempDOB)) dto.NgaySinh = tempDOB;
                    if (DateTime.TryParse(txtNgayGiaNhap.Text, out DateTime tempJoin)) dto.NgayGiaNhap = tempJoin;

                    if (Guid.TryParse(ddlPhongBan.SelectedValue, out Guid idPB) && idPB != Guid.Empty) dto.IdPhongBan = idPB;
                    if (Guid.TryParse(ddlChucDanh.SelectedValue, out Guid idCD) && idCD != Guid.Empty) dto.IdChucDanh = idCD;
                }
                // 3. BƠM DỮ LIỆU TÀI KHOẢN THEO INTENT
                if (!chkEnableAccount.Checked)
                {
                    // LUỒNG A: KHÔNG CẤP QUYỀN (Tạo mới Ghost hoặc Update Ghost)
                    dto.UserName = null;
                    dto.Password = null;
                    dto.RoleId = Guid.Empty;
                }
                else
                {
                    // LUỒNG B: BẬT CẤP QUYỀN
                    if (!hasAccount)
                    {
                        // Luồng B.1: Tạo mới Real HOẶC Nâng cấp từ Ghost lên Real
                        dto.UserName = txtUserName.Text.Trim();
                        dto.Password = null; // Để null để UserManager tự sinh Pass!
                    }
                    else
                    {
                        // Luồng B.2: Cập nhật Real Account (Đã có tài khoản thật)
                        dto.UserName = txtUserName.Text.Trim();

                        // Xử lý Checkbox Đổi mật khẩu
                        if (chkChangePassword.Checked)
                        {
                            if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                                validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                            else if (txtPassword.Text.Trim().Length < 6)
                                validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PASSWORD_MUST_HAVE_MINIMUM_OF_6_CHARACTERS));

                            if (txtConfirmPassword.Text.Trim() != txtPassword.Text.Trim())
                                validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.RE_ENTER_INCORRECT_PASSWORD));

                            dto.Password = txtPassword.Text; // Lấy pass tay
                        }
                        else
                        {
                            dto.Password = null; // Báo Backend không đổi Pass
                        }
                    }
                    // Đã cấp quyền thì lấy Role
                    if (Guid.TryParse(ddlRole.SelectedValue, out Guid rId) && rId != Guid.Empty)
                        dto.RoleId = rId;
                    else
                        dto.RoleId = Guid.Empty;
                }
                // Dừng lại nếu có lỗi Validation (pass ngắn, pass ko khớp...)
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                // 4. Đẩy xuống Backend xử lý
                var result = UserManager.Instance.CreateOrUpdate(dto);
                if (result == null)
                {
                    ShowInvalidDataError();
                    return;
                }
                ShowNotify(IsEditMode ? GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY) : GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY), MSGType.Success);
                dlDetail.CloseModal();
                SavedHandlerCallback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                // Escape các ký tự đặc biệt (nháy đơn, nháy kép, xuống dòng) 
                // để tránh làm vỡ cấu trúc mã JavaScript (Syntax Error) trên trình duyệt
                string safeErrorMsg = exc.Message
                                         .Replace("'", "\\'")
                                         .Replace("\"", "\\\"")
                                         .Replace("\r", "")
                                         .Replace("\n", " ");

                // Bắt gọn lỗi từ Manager ném ra và hiển thị an toàn
                ShowNotify(safeErrorMsg, MSGType.Error);
            }
        }
        // BƯỚC 1A: Hàm nhận diện Ghost Account ở tầng UI
        private bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_");
        }

        // BƯỚC 1B: Cập nhật hàm sinh Username theo chuẩn mới (Chốt 1)
    }
}