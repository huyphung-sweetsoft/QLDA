<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlUserDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlUserDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %> 
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin chi tiết" DefaultButton="lbtSubmit">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer">
            
            <!-- KHỐI A: THÔNG TIN CƠ BẢN -->
            <div class="col-lg-12 mb-3" id="boxBasicInfo" runat="server">
                <fieldset class="fieldset-box">
                    <legend class="text-primary fw-bold">Thông tin chung</legend>
                    <div class="row">
                        <div class="col-lg-8">
                            <div class="mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true" PlaceHolder="Nhập họ và tên"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="mb-3">
                                <label class="form-label label-valid">Email</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true" RequiredAdvanced="custom[email]" PlaceHolder="Nhập email cá nhân/công việc"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true" PlaceHolder="Nhập số điện thoại"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <!-- CHỐT 4: Làm rõ Tình trạng HR -->
                                    <label class="form-label fw-bold">Tình trạng công tác</label>
                                    <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Đang làm việc" OffText="Đã nghỉ" Checked="true" />
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-4 text-center">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IMAGE) %></label>
                            <SweetSoft:FilesBox runat="server" ID="fbImage" />
                        </div>
                    </div>
                </fieldset>
            </div>

            <!-- KHỐI B: THÔNG TIN NHÂN SỰ (Được đẩy lên trên) -->
            <div class="col-lg-12 mb-3" id="boxEmployeeInfo" runat="server">
                <fieldset class="fieldset-box">
                    <legend class="text-primary fw-bold">Hồ sơ Nhân sự</legend>
                    <div class="row">
                        <div class="col-lg-6 mb-3">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtCCCD" Required="false" PlaceHolder="Nhập CCCD"></SweetSoft:ExtraTextBox>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></label>
                            <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlPhongBan" SimpleInit="true" PlaceHolder="Chọn phòng ban"></SweetSoft:ExtraDropdown>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlChucDanh" SimpleInit="true" PlaceHolder="Chọn chức danh"></SweetSoft:ExtraDropdown>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" SimpleInit="true">
                                <asp:ListItem Text="-- Chọn --" Value=""></asp:ListItem>
                                <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                                <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                            </SweetSoft:ExtraDropdown>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE) %></label>
                           <asp:TextBox runat="server" ID="txtNgayGiaNhap" type="date" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="col-lg-12 mb-3">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" PlaceHolder="Nhập địa chỉ chi tiết"></SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </fieldset>
            </div>

            <!-- VÙNG TOGGLE: Nằm sát Thông tin tài khoản để logic nối tiếp nhau -->
            <div class="col-lg-12 mb-3" id="divToggleAccount" runat="server">
                <div class="form-check form-switch form-switch-md" dir="ltr">
                    <input class="form-check-input" type="checkbox" id="chkEnableAccount" runat="server" onclick="ToggleAccountInfo(this);">
                    <label class="form-check-label fw-bold text-primary fs-5" for="<%= chkEnableAccount.ClientID %>">
                        <i class="fas fa-key"></i> Cấp quyền truy cập phần mềm cho nhân sự này
                    </label>
                </div>
            </div>

            <!-- KHỐI C: THÔNG TIN TÀI KHOẢN -->
            <div class="col-lg-12 mb-3" id="boxAccountInfo" runat="server">
                <fieldset class="fieldset-box">
                    <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %></legend>
                    <div class="row">
                        <div class="col-lg-6 mb-3">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                            <div class="input-group"> 
                                <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="false" MaxLength="50" RequiredAdvanced="custom[username]" PlaceHolder="Tên đăng nhập hệ thống"></SweetSoft:ExtraTextBox>
                                <a class="btn btn-warning" href="javascript:;" onclick="GenerateUserName();" title="<%= GetResourceText(BackEndResourceKeys.GENERATE) %>">
                                    <i class="fas fa-bolt"></i>
                                </a>
                            </div>
                        </div>
                        <div class="col-lg-6 mb-3">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlRole" SimpleInit="true" PlaceHolder="Chọn nhóm quyền"></SweetSoft:ExtraDropdown>
                        </div>

                        <div runat="server" id="divChangePassword" visible="false" class="col-lg-12 mb-3">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="chkChangePassword" runat="server" onclick="TogglePasswordEdit(this);">
                               <label class="form-check-label text-primary fw-bold" for="<%= chkChangePassword.ClientID %>">
                                    <%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %> thủ công
                               </label>
                            </div>
                        </div>

                        <div runat="server" id="divPassword" data-selector="password" class="col-lg-12">
                            <div class="row">
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PASSWORD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" TextMode="Password" PlaceHolder="Nhập mật khẩu mới" Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" PlaceHolder="Xác nhận mật khẩu mới" Autocomplete="new-password"></SweetSoft:ExtraTextBox>
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
                <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                    OnClientClick="return CheckPreSubmitValid();" OnClick="lbtSubmit_Click">Lưu thông tin</SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<style>
    div[data-edit="true"] { display: none; }
    div[data-edit="true"].show { display: block; }
    .file-box-single { width: 120px; margin: 0 auto; }
    .file-box .uploaded-content .item img { width: 80px; height: 80px; object-fit: cover; border-radius: 50%; }
    .border-danger { border-color: #dc3545 !important; } /* Nổi bật vùng tạo tài khoản */
</style>

<script type="text/javascript">
    function TogglePasswordEdit(t) {
        var isChecked = $(t).is(':checked');
        var $pwdBox = $('[data-selector="password"]');
        
        if(isChecked){
            $pwdBox.addClass('show');
            $('#<%= txtPassword.ClientID %>').addClass('validate[required]');
            $('#<%= txtConfirmPassword.ClientID %>').addClass('validate[required]');
        } else {
            $pwdBox.removeClass('show');
            $('#<%= txtPassword.ClientID %>').removeClass('validate[required]');
            $('#<%= txtConfirmPassword.ClientID %>').removeClass('validate[required]');
            // Xóa text khi gạt tắt
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
        str = str.replace(/[^a-z0-9]/g, ""); // Dọn sạch, không kèm số (Chốt 1)
        $('#<%= txtUserName.ClientID %>').val(str);
    }

    function CheckPreSubmitValid() {
        if($('#<%= boxEmployeeInfo.ClientID %>').is(':visible')){
            $('#<%= txtCCCD.ClientID %>').addClass('validate[required]');
        }
        return CMSMasterJs.CheckValid();
    }
</script>