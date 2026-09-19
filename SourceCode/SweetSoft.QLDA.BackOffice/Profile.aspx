<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.Profile" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/Controls/TwoFA/CtrlTwoFactorAuthenticator.ascx" TagPrefix="SweetSoft" TagName="CtrlTwoFactorAuthenticator" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .avatar-upload { display: flex; justify-content: center; }
        .avatar-upload .file-box { max-width: 160px; margin: 0 auto; }
        .avatar-upload .file-box .uploaded-content .item { border: none !important; background: transparent !important; box-shadow: none !important; padding: 0 !important; }
        .avatar-upload .file-box .uploaded-content .item .bg-body { background-color: transparent !important; }
        .avatar-upload .file-box .uploaded-content .img-container { border: none !important; padding: 0 !important; display: flex; justify-content: center; }
        .avatar-upload .file-box .uploaded-content .item img { width: 150px !important; height: 150px !important; object-fit: cover !important; border-radius: 50% !important; border: 4px solid #ffffff !important; box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important; transition: all 0.3s ease; }
        .avatar-upload .file-box .uploaded-content .item img:hover { transform: scale(1.02); box-shadow: 0 6px 15px rgba(0,0,0,0.2) !important; }
        .avatar-upload .file-box .form-check { display: none !important; }
        
        ol > li::marker { font-weight: bold; }
        .otp-input { display: flex; justify-content: center; margin-bottom: 10px; }
        .otp-input input { width: 40px; height: 40px; margin: 0 8px; text-align: center !important; font-size: 1.5rem; border: 2px solid #4a1387; border-radius: 8px; transition: all 0.3s ease; }
        .otp-input input:focus { outline: none; }
        .otp-input input::-webkit-outer-spin-button, .otp-input input::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
        .otp-input input[type=number] { -moz-appearance: textfield; }
        @media (max-width: 468px) { .otp-input input { width: 40px; height: 40px; margin: 0px 4px; } }
    </style>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <div class="wrapperBgProfile">
                    <div class="wrapperBg"></div>
                    <div class="wrapperFormContent">
                        <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Hồ sơ" />
                        
                        <!-- ĐÃ SỬA: Dùng flexbox để căn 2 khối ra 2 lề trái/phải -->
                        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                            
                            <!-- Khối bên trái: Các Tabs (Sẽ nằm sát nhau như cũ) -->
                            <div class="tabs-horizontal">
                                <ul class="nav nav-pills card-header-pills" role="tablist">
                                    <li class="nav-item">
                                        <a class="nav-link px-3 active" data-bs-toggle="tab" href="#overview" role="tab">
                                            <%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %>
                                        </a>
                                    </li>
                                    
                                    <asp:PlaceHolder ID="plhPersonalTab" runat="server">
                                        <li class="nav-item">
                                            <a class="nav-link px-3" data-bs-toggle="tab" href="#personal-info" role="tab">
                                                <%= GetResourceText(BackEndResourceKeys.PERSONAL_AND_WORK_INFORMATION) %>
                                            </a>
                                        </li>
                                    </asp:PlaceHolder>
                                    
                                    <li class="nav-item">
                                        <a class="nav-link px-3" data-bs-toggle="tab" href="#change-pass" role="tab">
                                            <%=GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                                        </a>
                                    </li>
                                    
                                    <li class="nav-item">
                                        <a class="nav-link px-3" data-bs-toggle="tab" href="#setup-two-factor-authentication" role="tab">
                                            <%=GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION) %>
                                        </a>
                                    </li>
                                </ul>
                            </div>

                            <!-- Khối bên phải: Nút chuyển trang -->
                            <asp:PlaceHolder ID="plhEmployeeLink" runat="server" Visible="false">
                                <div>
                                    <a id="lnkGoToWorkProfile" runat="server" class="btn btn-primary fw-bold shadow-sm" style="border-radius: 6px;">
                                        <i class="fas fa-user-tie me-1"></i>  <%=GetResourceText(BackEndResourceKeys.PERSONAL_INFORMATION_DETAILS) %>
                                    </a>
                                </div>
                            </asp:PlaceHolder>

                        </div>

                        <div class="tab-content text-muted tab-overide">
                            
                            <!-- TAB 1: THÔNG TIN TÀI KHOẢN -->
                            <div class="tab-pane active mt-2" id="overview" role="tabpanel">
                                <div class="card-grid-view">
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-xl-3 text-center">
                                                <div class="avatar-upload" style="margin-top: 15px;">
                                                    <SweetSoft:FilesBox runat="server" ID="fbImage" />
                                                </div>
                                            </div>
                                            <div class="col-xl-9 mb-4">
                                                <div class="card shadow-none border">
                                                    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
                                                        <ContentTemplate>
                                                            <div class="row js-update-account validationEngineContainer p-3">
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtUserName.ClientID %>" class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="true" ReadOnly="true"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtFullName.ClientID %>" class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.FULL_NAME) %></label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtEmail.ClientID %>" class="form-label label-valid">Email</label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" RequiredAdvanced="custom[email]" IsEmail="true"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtPhone.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtPhone"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-lg-12 text-end mt-2">
                                                                    <SweetSoft:ExtraButton runat="server" ID="lbtUpdate" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.ValidForm('.js-update-account');" OnClick="lbtUpdate_Click"></SweetSoft:ExtraButton>
                                                                </div>
                                                            </div>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- TAB 2: THÔNG TIN CÁ NHÂN -->
                            <asp:PlaceHolder ID="plhPersonalContent" runat="server">
                                <div class="tab-pane mt-2" id="personal-info" role="tabpanel">
                                    <div class="card card-grid-view shadow-none border">
                                        <div class="card-body">
                                            <asp:UpdatePanel runat="server" ID="upnlPersonalInfo" UpdateMode="Conditional">
                                                <ContentTemplate>
                                                    <div class="row js-update-personal validationEngineContainer p-3">
                                                        <div class="col-md-4 col-sm-6">
                                                            <div class="mb-3">
                                                                <label for="<%= txtCCCD.ClientID %>" class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtCCCD" Required="true"></SweetSoft:ExtraTextBox>
                                                            </div>
                                                        </div>
                                                        <div class="col-md-4 col-sm-6">
                                                            <div class="mb-3">
                                                                <label for="<%= txtNgaySinh.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></label>
                                                                <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                                                            </div>
                                                        </div>
                                                        <div class="col-md-4 col-sm-6">
                                                            <div class="mb-3">
                                                                <label for="<%= ddlGioiTinh.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                                                                <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" SimpleInit="true">
                                                                    <asp:ListItem Text="-- Chọn --" Value=""></asp:ListItem>
                                                                    <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                                                                    <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                                                                    <asp:ListItem Text="Khác" Value="Khác"></asp:ListItem>
                                                                </SweetSoft:ExtraDropdown>
                                                            </div>
                                                        </div>
                                                        <div class="col-md-12">
                                                            <div class="mb-3">
                                                                <label for="<%= txtDiaChi.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" TextMode="MultiLine" Rows="2"></SweetSoft:ExtraTextBox>
                                                            </div>
                                                        </div>
                                                        <div class="col-lg-12 text-end mt-2">
                                                            <SweetSoft:ExtraButton runat="server" ID="lbtUpdatePersonal" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.ValidForm('.js-update-personal');" OnClick="lbtUpdatePersonal_Click"></SweetSoft:ExtraButton>
                                                        </div>
                                                    </div>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>

                            <!-- TAB 3: ĐỔI MẬT KHẨU -->
                            <div class="tab-pane mt-2" id="change-pass" role="tabpanel">
                                <div class="card card-grid-view shadow-none border">
                                    <div class="card-body">
                                        <asp:UpdatePanel runat="server" ID="upChangePassword" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <div class="row justify-content-center p-3">
                                                    <div class="col-md-5 col-sm-8 js-change-password validationEngineContainer" runat="server" id="divChangePassword">
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtOldPassword.ClientID %>"><%= GetResourceText(BackEndResourceKeys.OLD_PASSWORD) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtOldPassword" TextMode="Password" Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtNewPassword.ClientID %>"><%=GetResourceText(BackEndResourceKeys.NEW_PASSWORD) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtNewPassword" TextMode="Password" Required="true" RequiredAdvanced="minSize[6]"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtConfirmPassword.ClientID %>"><%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" Required="true" RequiredAdvanced="minSize[6]"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="text-center mt-4">
                                                            <SweetSoft:ExtraButton runat="server" ID="lbtChangePassword" CssClass="waves-effect waves-light" ButtonStyle="Primary" Width="200px" ButtonIcon="Check" IsPace="true" OnClientClick="return CMSMasterJs.ValidForm('.js-change-password');" OnClick="lbtChangePassword_Click"></SweetSoft:ExtraButton>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-4 col-sm-6" runat="server" id="divTwoFAForChangePassword" visible="false">
                                                        <h3 class="fw-bold mb-2"><%= GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION) %></h3>
                                                        <SweetSoft:CtrlTwoFactorAuthenticator runat="server" ID="CtrlTwoFAForChangePassword" />
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- TAB 4: 2FA -->
                            <div class="tab-pane mt-2" id="setup-two-factor-authentication">
                                <div class="row">
                                    <div class="col-12 text-dark mb-4 p-2 pt-0">
                                        <div class="card shadow-none border">
                                            <div class="card-body p-4">
                                                <asp:UpdatePanel runat="server" ID="pnlTwoFactor" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <div class="d-flex align-items-center flex-wrap gap-2 mb-4 border-bottom pb-3">
                                                            <h5 class="fw-bold mb-0 text-primary"><i class="fas fa-shield-alt me-2"></i><%= GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION_IS) %></h5>
                                                            <span runat="server" id="spStatusTwoFA" class="ms-2"></span>
                                                        </div>
                                                        <div runat="server" id="divResetTwoFA" visible="false">
                                                            <h6 class="fw-bold mb-2"><%= GetResourceText(BackEndResourceKeys.RESET_AUTHENTICATOR_APP) %></h6>
                                                            <ul class="text-muted">
                                                                <li><%= GetResourceText(BackEndResourceKeys.ENTER_YOUR_CURRENT_AUTHENTICATOR_APP_CODE_TO_TURN_OFF_THIS_FEATURE) %></li>
                                                            </ul>
                                                        </div>
                                                        <div runat="server" id="divInstructionsIntegrateTwoFA">
                                                            <h6 class="fw-bold mb-3"><%= GetResourceText(BackEndResourceKeys.INSTRUCTIONS_FOR_SETUP) %></h6>
                                                            <ol class="text-muted ps-3">
                                                                <li class="mb-3">
                                                                    <span class="fw-bold text-dark"><%= GetResourceText(BackEndResourceKeys.DOWNLOAD_AUTHENTICATION_APP) %></span><br/>
                                                                    <small><%= GetResourceText(BackEndResourceKeys.WE_RECOMMEND_DOWNLOADING_GOOGLE_AUTHENTICATOR_IF_YOU_DONT_HAVE_ONE_INSTALLED) %></small>
                                                                </li>
                                                                <li class="mb-3">
                                                                    <span class="fw-bold text-dark"><%= GetResourceText(BackEndResourceKeys.SCAN_THIS_QRCODE_OR_COPY_THE_KEY) %></span><br/>
                                                                    <small><%= GetResourceText(BackEndResourceKeys.SCAN_THIS_QRCODE_IN_THE_AUTHENTICATION_APP_OR_COPY_THE_KEY_AND_PASTE_IT_IN_THE_AUTHENTICATION_APP) %></small>
                                                                    <div class="mt-3 mb-2 align-items-center">
                                                                        <img src="#" runat="server" id="imgScretKey" style="max-width: 180px; background: #fff; border: 1px solid #dee2e6; border-radius: 8px; padding: 10px; box-shadow: 0 0.125rem 0.25rem rgba(0,0,0,0.075);" />
                                                                        <div class="d-inline-block align-top ms-3 mt-3">
                                                                            <span class="fw-bold d-block mb-1 text-dark">Secret Key:</span>
                                                                            <span class="badge bg-light text-dark border p-2" style="font-size:14px;" id="spSecretKey" runat="server"></span>
                                                                            <a href="javascript:;" class="btn btn-sm btn-outline-primary ms-2 btn-copy-text" data-clipboard-action="copy" data-clipboard-target="#<%= spSecretKey.ClientID %>" title="<%= GetResourceText(BackEndResourceKeys.COPY) %>">
                                                                                <i class="fa fa-clone"></i> Copy
                                                                            </a>
                                                                        </div>
                                                                    </div>
                                                                </li>
                                                                <li class="mb-3">
                                                                    <span class="fw-bold text-dark"><%= GetResourceText(BackEndResourceKeys.COPY_AND_ENTER_6_DIGIT_CODE) %></span><br/>
                                                                    <small><%= GetResourceText(BackEndResourceKeys.AFTER_THE_QRCODE_IS_SCANNED_OR_THE_KEY_IS_ENTERED_YOUR_AUTHENTICATOR_APP_GENERATES_A_6_DIGIT_CODE_COPY_THE_CODE_THEN_COME_BACK_HERE_TO_ENTER_IT) %></small>
                                                                </li>
                                                            </ol>
                                                        </div>
                                                        <div class="bg-light p-3 rounded border text-center mt-4">
                                                            <p class="fw-bold mb-3"><%= GetResourceText(BackEndResourceKeys.ENTER_THE_6_DIGIT_CODE_GENERATED_BY_YOUR_AUTHENTICATOR_APP) %></p>
                                                            <SweetSoft:CtrlTwoFactorAuthenticator runat="server" ID="CtrlToggleTwoFA" />
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server"></asp:Content>