<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlUserPopup.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlUserPopup" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<SweetSoft:ExtraModal
    runat="server"
    ID="dlDetail"
    Type="Primary"
    Size="ExtraLarge"
    Width="68%"
    Title="Thông tin chi tiết"
    DefaultButton="lbtSubmit">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer">
            <div class="col-lg-12 mb-3" id="boxSystemAccount" runat="server">
                <fieldset class="fieldset-box user-popup-fieldset">
                    <legend class="text-primary fw-bold">
                        <%= GetResourceText(BackEndResourceKeys.SYSTEM_ACCOUNT) %>
                    </legend>

                    <div class="row mx-0">
                        <!-- CỘT 1: AVATAR -->
                         <div class="col-lg-3 text-center border-end user-popup-avatar">
                            <label class="form-label fw-bold text-muted">
                                <%= GetResourceText(BackEndResourceKeys.IMAGE) %>
                            </label>
                            <SweetSoft:FilesBox runat="server" ID="fbImage" />
                        </div>

                        <!-- CỘT 2: FORM NHẬP LIỆU -->
                       <div class="col-lg-9">
                            <div class="row">

                                <!-- HÀNG 1: HỌ TÊN / USERNAME -->
                                <div class="col-md-6 mb-3">
                                    <label class="form-label label-valid">
                                        <%= GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %>
                                    </label>
                                    <SweetSoft:ExtraTextBox
                                        runat="server"
                                        ID="txtFullName"
                                        Required="true"
                                        PlaceHolder="Nhập họ và tên">
                                    </SweetSoft:ExtraTextBox>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label label-valid">
                                        <%= GetResourceText(BackEndResourceKeys.USER_NAME) %>
                                    </label>
                                    <div class="input-group">
                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtUserName"
                                            Required="true"
                                            MaxLength="50"
                                            RequiredAdvanced="custom[username]"
                                            PlaceHolder="Tên đăng nhập hệ thống">
                                        </SweetSoft:ExtraTextBox>

                                        <a
                                            class="btn btn-warning"
                                            href="javascript:;"
                                            onclick="GenerateUserName();"
                                            title="<%= GetResourceText(BackEndResourceKeys.GENERATE) %>">
                                            <i class="fas fa-bolt"></i>
                                        </a>
                                    </div>
                                </div>

                                <!-- HÀNG 2: EMAIL / NHÓM NGƯỜI DÙNG -->
                                <div class="col-md-6 mb-3">
                                    <label class="form-label label-valid">
                                        Email
                                    </label>
                                    <SweetSoft:ExtraTextBox
                                        runat="server"
                                        ID="txtEmail"
                                        Required="true"
                                        IsEmail="true"
                                        RequiredAdvanced="custom[email]"
                                        PlaceHolder="Nhập email cá nhân/công việc">
                                    </SweetSoft:ExtraTextBox>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label label-valid">
                                        <%= GetResourceText(BackEndResourceKeys.USER_GROUP) %>
                                    </label>
                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlRole"
                                        SimpleInit="true"
                                        PlaceHolder="Chọn nhóm quyền">
                                    </SweetSoft:ExtraDropdown>
                                </div>

                                <!-- HÀNG 3: ĐIỆN THOẠI / CHO PHÉP ĐĂNG NHẬP -->
                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>
                                    </label>
                                    <SweetSoft:ExtraTextBox
                                        runat="server"
                                        ID="txtPhone"
                                        IsPhone="true"
                                        PlaceHolder="Nhập số điện thoại">
                                    </SweetSoft:ExtraTextBox>
                                </div>

                                <div class="col-md-6 mb-3 user-popup-login-status">
                                    <label class="form-label fw-bold user-popup-nowrap">
                                        <%= GetResourceText(BackEndResourceKeys.ALLOW_LOGIN) %>
                                    </label>
                                    <SweetSoft:ExtraCheckbox
                                        runat="server"
                                        ID="chkStatus"                             
                                        Checked="true" />
                                </div>

                                <!-- NÚT GẠT ĐỔI MẬT KHẨU -->
                                <div
                                    runat="server"
                                    id="divChangePassword"
                                    visible="false"
                                    class="col-12 mb-2">
                                    <div class="form-check">
                                        <input
                                            class="form-check-input"
                                            type="checkbox"
                                            id="chkChangePassword"
                                            runat="server"
                                            onclick="TogglePasswordEdit(this);">

                                        <label
                                            class="form-check-label text-primary fw-bold"
                                            for="<%= chkChangePassword.ClientID %>">
                                            <%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                                        </label>
                                    </div>
                                </div>

                                <!-- KHUNG MẬT KHẨU -->
                                <div runat="server" id="divPassword" data-selector="password" class="col-12">
                                    <div class="row">
                                        <div class="col-md-6 mb-3">
                                            <label class="form-label label-valid">
                                                <%= GetResourceText(BackEndResourceKeys.PASSWORD) %>
                                            </label>
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" TextMode="Password" PlaceHolder="Nhập mật khẩu mới" Autocomplete="new-password">
                                            </SweetSoft:ExtraTextBox>
                                        </div>

                                        <div class="col-md-6 mb-3">
                                            <label class="form-label label-valid">
                                                <%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %>
                                            </label>
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" PlaceHolder="Xác nhận mật khẩu mới" Autocomplete="new-password">
                                            </SweetSoft:ExtraTextBox>
                                        </div>
                                    </div>
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
    /* Logic ẩn/hiện mượt mà cho khối mật khẩu */
    div[data-selector="password"] {
        display: none !important; /* Trạng thái mặc định: ẨN ĐI */
    }

    div[data-selector="password"].show {
        display: block !important; /* Khi tick chọn: HIỆN LÊN ở dạng block để giữ đúng cấu trúc lưới Bootstrap */
    }
    .user-popup-fieldset {
        width: auto;
        max-width: 100%;
        box-sizing: border-box;
    }

    /* ĐỊNH DẠNG AVATAR */
    .user-popup-avatar {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: flex-start;
        padding-top: 15px;
        padding-bottom: 25px !important; /* Đẩy viền tím phía dưới ra xa, không cho avatar chạm đáy */
    }
    /* Tiêu diệt hoàn toàn khung nền vàng và bóng mờ */
    /* Tiêu diệt tận gốc mọi lớp nền vàng/kem và viền của control FilesBox */
    .user-popup-avatar .file-box,
    .user-popup-avatar .file-box-single,
    .user-popup-avatar .file-box .uploaded-content,
    .user-popup-avatar .file-box .item,
    .user-popup-avatar .file-box > div {
        background: transparent !important;
        background-color: transparent !important; /* Ép chết màu nền cứng */
        border: none !important;
        box-shadow: none !important;
        padding: 0 !important;
    }

    .user-popup-avatar .file-box-single {
        width: 100% !important;
        margin: 0 auto;
    }

    /* 2. Cấu trúc lại khung chứa thành hình vuông 140x140 */
    .user-popup-avatar .file-box .item {
        width: 140px !important;
        height: 140px !important;
        margin: 0 auto;
        position: relative;
    }

    /* 3. Ép thẻ img thành hình tròn, vừa khít 140x140, thêm viền nổi bật */
    .user-popup-avatar .file-box .item img {
        width: 140px !important;
        height: 140px !important;
        object-fit: cover !important;
        border-radius: 50% !important;
        border: 4px solid #fff !important;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
    }

    /* Giữ nguyên checkbox nằm ngang */
    .user-popup-login-status {
        display: flex;
        flex-direction: column;
        justify-content: flex-start;
    }
    
    .user-popup-nowrap {
        white-space: nowrap;
    }
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