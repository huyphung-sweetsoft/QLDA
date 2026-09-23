<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlNhanVienPopup.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.Controls.CtrlNhanVienPopup" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<SweetSoft:ExtraModal
    runat="server"
    ID="dlDetail"
    Type="Primary"
    Size="ExtraLarge"
    Title="Thông tin chi tiết" 
    DefaultButton="lbtSubmit">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer p-2">
            <div class="col-lg-12">
                
                <!-- KHỐI 1: THÔNG TIN NHÂN VIÊN -->
                <fieldset class="fieldset-box mb-2" style="padding-bottom: 5px; padding-top: 5px;">
                    <legend class="text-primary fw-bold px-1 mb-0">
                        Thông tin nhân viên
                    </legend>
                    <!-- Đổi pt-2 thành pt-0 để hút khoảng trống phía trên -->
                    <div class="row pt-0 mx-0">
                        
                        <!-- CỘT TRÁI: AVATAR -->
                        <div class="col-lg-2 text-center border-end user-popup-avatar">
                            <label class="form-label fw-bold text-muted mb-2 mt-1">
                                <%= GetResourceText(BackEndResourceKeys.IMAGE) %>
                            </label>
                            <div class="avatar-upload mt-1">
                                <SweetSoft:FilesBox runat="server" ID="fbImage" />
                            </div>
                        </div>

                        <!-- CỘT PHẢI: FORM NHẬP LIỆU -->
                        <div class="col-lg-10 pt-2">
                            <div class="row px-1">
                                
                                <!-- HÀNG 1: Họ tên | Ngày sinh | Giới tính | CCCD -->
                                <div class="col-md-3 mb-2">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.FULL_NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></label>
                                    <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" SimpleInit="true">
                                        <asp:ListItem Text="-- Chọn --" Value=""></asp:ListItem>
                                        <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                                        <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                                    </SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtCCCD" Required="true"></SweetSoft:ExtraTextBox>
                                </div>

                                <!-- HÀNG 2: Email | Điện thoại | Địa chỉ (Cấp col-6 để full dòng) -->
                                <div class="col-md-3 mb-2">
                                    <label class="form-label label-valid">Email</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true" RequiredAdvanced="custom[email]"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi"></SweetSoft:ExtraTextBox>
                                </div>

                                <!-- HÀNG 3: Phòng ban | Chức danh | Ngày gia nhập | Tình trạng -->
                                <!-- Đổi mb-2 thành mb-0 ở hàng cuối cùng để hút khoảng trống phía dưới -->
                                <div class="col-md-3 mb-0">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlPhongBan" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-3 mb-0">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlChucDanh" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-3 mb-0">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE) %></label>
                                   <asp:TextBox runat="server" ID="txtNgayGiaNhap" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-md-3 mb-0 d-flex flex-column justify-content-start">
                                    <label class="form-label fw-bold text-nowrap">
                                        <%= GetResourceText(BackEndResourceKeys.WORKING_STATUS) %>
                                    </label>
                                    <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" Checked="true" />
                                </div>

                            </div>
                        </div>
                    </div>
                </fieldset>

                <!-- VÙNG TOGGLE CẤP QUYỀN -->
                <div class="mb-2 ps-2" id="divToggleAccount" runat="server">
                    <div class="form-check form-switch form-switch-md" dir="ltr">
                        <input class="form-check-input" type="checkbox" id="chkEnableAccount" runat="server" onclick="ToggleAccountInfo(this);">
                        <label class="form-check-label fw-bold text-primary fs-5" for="<%= chkEnableAccount.ClientID %>">
                            <i class="fas fa-key me-1"></i> Cấp quyền truy cập phần mềm cho nhân sự này
                        </label>
                    </div>
                </div>

                <!-- KHỐI 2: THÔNG TIN TÀI KHOẢN -->
                <fieldset class="fieldset-box mb-1" id="boxAccountInfo" runat="server" style="display: none; padding-bottom: 5px;">
                    <legend class="text-primary fw-bold px-1 mb-0"><%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %></legend>
                    <div class="row pt-2 mx-0 px-1">
                        
                        <!-- Dòng 1: Username | Role | Toggle Password -->
                        <div class="col-md-4 mb-2">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                            <div class="input-group">
                                <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="false" MaxLength="50" RequiredAdvanced="custom[username]"></SweetSoft:ExtraTextBox>
                                <a class="btn btn-warning" href="javascript:;" onclick="GenerateUserName();" title="<%= GetResourceText(BackEndResourceKeys.GENERATE) %>">
                                    <i class="fas fa-bolt"></i>
                                </a>
                            </div>
                        </div>
                        <div class="col-md-4 mb-2">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlRole" SimpleInit="true"></SweetSoft:ExtraDropdown>
                        </div>
                        <div runat="server" id="divChangePassword" visible="false" class="col-md-4 mb-2 d-flex align-items-end">
                            <div class="form-check mb-2">
                                <input class="form-check-input" type="checkbox" id="chkChangePassword" runat="server" onclick="TogglePasswordEdit(this);">
                                <label class="form-check-label text-primary fw-bold" for="<%= chkChangePassword.ClientID %>">
                                    <%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                                </label>
                            </div>
                        </div>

                        <!-- Dòng 2: Mật khẩu -->
                        <div runat="server" id="divPassword" data-selector="password" class="col-12 p-0">
                            <div class="row mx-0">
                                <div class="col-md-6 mb-2">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PASSWORD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" TextMode="Password" Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-2">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                        </div>
                    </div>
                </fieldset>

            </div>
        </div>
    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraButton
                    runat="server"
                    ID="lbtSubmit"
                    CssClass="waves-effect waves-light"
                    ButtonStyle="Primary"
                    ButtonIcon="Save"
                    IsPace="true"
                    OnClientClick="return CMSMasterJs.CheckValid();"
                    OnClick="lbtSubmit_Click">
                    Lưu thông tin
                </SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<style>
    div[data-selector="password"] { display: none !important; }
    div[data-selector="password"].show { display: block !important; }
    .user-popup-fieldset { width: auto; max-width: 100%; box-sizing: border-box; }

    /* XỬ LÝ KHÔNG GIAN DƯ THỪA TỪ LEGEND */
    fieldset.fieldset-box legend { margin-bottom: 0 !important; }

    /* CSS ĐÃ ĐIỀU CHỈNH: Triệt tiêu chiều cao ảo của Avatar */
    .user-popup-avatar { display: flex; flex-direction: column; align-items: center; justify-content: flex-start; padding-top: 5px; padding-bottom: 0 !important; }
    .avatar-upload { display: flex; justify-content: center; width: 100%; }
    
    /* Ép FilesBox và tất cả thẻ con bên trong không được phình to (min-height: 0) */
    .avatar-upload .file-box,
    .avatar-upload .file-box .card,
    .avatar-upload .file-box .card-body,
    .avatar-upload .file-box .box-body,
    .avatar-upload .file-box .dropzone,
    .avatar-upload .file-box .uploaded-content { 
        max-width: 110px; 
        margin: 0 auto !important; 
        background: transparent !important; 
        border: none !important; 
        box-shadow: none !important; 
        min-height: 0 !important; 
        padding: 0 !important; 
    }

    .avatar-upload .file-box .uploaded-content .item { border: none !important; background: transparent !important; box-shadow: none !important; padding: 0 !important; min-height: 0 !important; }
    .avatar-upload .file-box .uploaded-content .item .bg-body { background-color: transparent !important; }
    .avatar-upload .file-box .uploaded-content .img-container { border: none !important; padding: 0 !important; display: flex; justify-content: center; }
    
    /* Chỉnh lại size ảnh xuống 100px để form cực gọn */
    .avatar-upload .file-box .uploaded-content .item img { width: 100px !important; height: 100px !important; object-fit: cover !important; border-radius: 50% !important; border: 3px solid #ffffff !important; box-shadow: 0 4px 8px rgba(0,0,0,0.15) !important; transition: all 0.3s ease; }
    .avatar-upload .file-box .uploaded-content .item img:hover { transform: scale(1.02); box-shadow: 0 6px 12px rgba(0,0,0,0.2) !important; }
    
    /* Ẩn các nút rác của upload */
    .avatar-upload .file-box .form-check, 
    .avatar-upload .file-box-single .control-help { display: none !important; }
</style>

<script type="text/javascript">
    function TogglePasswordEdit(t) {
        var isChecked = $(t).is(':checked');
        var $pwdBox = $('[data-selector="password"]');

        if (isChecked) {
            $pwdBox.addClass('show');
            $('#<%= txtPassword.ClientID %>').addClass('validate[required]');
            $('#<%= txtConfirmPassword.ClientID %>').addClass('validate[required]');
        } else {
            $pwdBox.removeClass('show');
            $('#<%= txtPassword.ClientID %>').removeClass('validate[required]');
            $('#<%= txtConfirmPassword.ClientID %>').removeClass('validate[required]');
            $('#<%= txtPassword.ClientID %>').val('');
            $('#<%= txtConfirmPassword.ClientID %>').val('');
        }
    }

    function ToggleAccountInfo(t) {
        var isChecked = $(t).is(':checked');
        var $accBox = $('#<%= boxAccountInfo.ClientID %>');
        var $txtUser = $('#<%= txtUserName.ClientID %>');
        var $ddlRole = $('#<%= ddlRole.ClientID %>');
        
        if (isChecked) {
            $accBox.slideDown();
            $txtUser.addClass('validate[required]');
            $ddlRole.addClass('validate[required]');
        } else {
            $accBox.slideUp();
            $txtUser.removeClass('validate[required]');
            $ddlRole.removeClass('validate[required]');
            var chkPwd = $('#<%= chkChangePassword.ClientID %>');
            if (chkPwd.length > 0) {
                chkPwd.prop('checked', false);
                TogglePasswordEdit(chkPwd[0]);
            }
        }
    }

    function GenerateUserName() {
        var fullName = $('#<%= txtFullName.ClientID %>').val();
        if (!fullName) {
            alert('Vui lòng nhập Họ và tên trước khi tự tạo Tên đăng nhập!');
            return;
        }
        var str = fullName.toLowerCase();
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
        str = str.replace(/đ/g, "d");
        str = str.replace(/[^a-z0-9]/g, "");
        $('#<%= txtUserName.ClientID %>').val(str);
    }
</script>