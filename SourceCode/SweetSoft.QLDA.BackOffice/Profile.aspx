<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.Profile" %>

<%--------------------PROGRAMER LOGS------------------------%>
<%--**Created by: 
--%>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/Controls/TwoFA/CtrlTwoFactorAuthenticator.ascx" TagPrefix="SweetSoft" TagName="CtrlTwoFactorAuthenticator" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .avatar-upload .file-box .uploaded-content .item .bg-body {
            background-color: transparent !important;
        }

        .avatar-upload .file-box .uploaded-content .img-container {
            border: none !important;
        }

        ol > li::marker {
            font-weight: bold;
        }

        .otp-input {
            display: flex;
            justify-content: center;
            margin-bottom: 10px;
        }

            .otp-input input {
                width: 40px;
                height: 40px;
                margin: 0 8px;
                text-align: center !important;
                font-size: 1.5rem;
                border: 2px solid #4a1387;
                border-radius: 8px;
                transition: all 0.3s ease;
            }

                .otp-input input:focus {
                    outline: none;
                }

                .otp-input input::-webkit-outer-spin-button,
                .otp-input input::-webkit-inner-spin-button {
                    -webkit-appearance: none;
                    margin: 0;
                }

                .otp-input input[type=number] {
                    -moz-appearance: textfield;
                }

        @media (max-width: 468px) {
            .otp-input input {
                width: 40px;
                height: 40px;
                margin: 0px 4px;
            }
        }
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
                        <div class="flex-between flex-between-xl gap-4">
                            <div class="tabs-horizontal">
                                <ul class="nav nav-pills card-header-pills" role="tablist">
                                    <li class="nav-item">
                                        <a class="nav-link px-2 active" data-bs-toggle="tab" href="#overview" role="tab">
                                            <%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %>
                                        </a>
                                    </li>
                                    <li class="nav-item">
                                        <a class="nav-link px-2" data-bs-toggle="tab" href="#change-pass" role="tab">
                                            <%=GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                                        </a>
                                    </li>
                                    <li class="nav-item">
                                        <a class="nav-link px-2" data-bs-toggle="tab" href="#setup-two-factor-authentication" role="tab">
                                            <%=GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION) %>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>

                        <div class="tab-content text-muted tab-overide">
                            <div class="tab-pane active mt-2" id="overview" role="tabpanel">
                                <div class="card-grid-view">
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-xl-3">
                                               <%-- <div class="avatar-upload">
                                                    <SweetSoft:FilesBox runat="server" ID="fbImage" />
                                                </div>--%>
                                                <div class="avatar-upload">
                                                    <div runat="server" visible="false" class="avatar-edit">
                                                        <label for="imageUpload" onclick="javascript:$('#<%= imgAvatar.ClientID %>')[0].click(); ">
                                                            <i class="bx bx-edit icon-edit"></i>
                                                        </label>
                                                    </div>
                                                    <div class="avatar-preview">
                                                        <img runat="server" id="imgAvatar" data-selector="imgAvatar" src="/Styles/images/user-icon.png" alt="Avatar" data-hdf="txtAvatar" class="w-100" />
                                                        <input type="hidden" id="txtAvatar" runat="server" class="form-control" data-selector="txtAvatar" />
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xl-9 mb-4">
                                                <div class="card">
                                                    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
                                                        <ContentTemplate>
                                                            <div class="row js-update-profile validationEngineContainer">
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtUserName.ClientID %>" class="form-label label-valid">
                                                                            <%= GetResourceText(BackEndResourceKeys.USER_NAME) %>
                                                                        </label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="true" ReadOnly="true" RequiredAdvanced="custom[username]" OnKeyUp="CMSMasterJs.SetInputShowText(this,'.js-show-title')" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtFullName.ClientID %>" class="form-label label-valid">
                                                                            <%= GetResourceText(BackEndResourceKeys.FULL_NAME) %>
                                                                        </label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="clearfix"></div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtEmail.ClientID %>" class="form-label label-valid">Email</label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" RequiredAdvanced="custom[email]" PlaceHolder="Enter the value" IsEmail="true"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <div class="col-sm-6">
                                                                    <div class="mb-3">
                                                                        <label for="<%= txtPhone.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                </div>
                                                                <%--<div class="col-sm-6">
                                                        <div class="mb-3">
                                                            <label for="<%= chkEnableNotification.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.ENABLE_NOTIFICATION) %></label>
                                                            <SweetSoft:ExtraCheckbox runat="server" ID="chkEnableNotification"></SweetSoft:ExtraCheckbox>
                                                        </div>
                                                    </div>--%>

                                                                <div class="col-lg-12 text-center mt-3">
                                                                    <SweetSoft:ExtraButton runat="server" ID="lbtUpdate" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.ValidForm('.js-update-profile');" OnClick="lbtUpdate_Click"></SweetSoft:ExtraButton>
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
                            <div class="tab-pane mt-2" id="change-pass" role="tabpanel">
                                <div class="card card-grid-view">
                                    <div class="card-body">
                                        <asp:UpdatePanel runat="server" ID="upChangePassword" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <div class="row justify-content-center">
                                                    <div class="col-md-4 col-sm-6 js-change-password validationEngineContainer" runat="server" id="divChangePassword">
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtOldPassword.ClientID %>">
                                                                <%= GetResourceText(BackEndResourceKeys.OLD_PASSWORD) %>
                                                            </label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtOldPassword" TextMode="Password" PlaceHolder="Enter the value"
                                                                Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtNewPassword.ClientID %>">
                                                                <%=GetResourceText(BackEndResourceKeys.NEW_PASSWORD) %>
                                                            </label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtNewPassword" TextMode="Password" Required="true" RequiredAdvanced="minSize[6]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid" for="<%= txtConfirmPassword.ClientID %>">
                                                                <%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %>
                                                            </label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" Required="true" RequiredAdvanced="minSize[6]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="text-center">
                                                            <SweetSoft:ExtraButton runat="server" ID="lbtChangePassword" CssClass="waves-effect waves-light" ButtonStyle="Primary" Width="200px" ButtonIcon="Check" IsPace="true" OnClientClick="return CMSMasterJs.ValidForm('.js-change-password');" OnClick="lbtChangePassword_Click">Confirm</SweetSoft:ExtraButton>
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
                                    <!-- end card body -->
                                </div>
                                <!-- end card -->
                            </div>
                            <div class="tab-pane mt-2" id="setup-two-factor-authentication">
                                <div class="row">
                                    <div class="col-12 text-dark mb-4 p-2 pt-0">
                                        <div class="card">
                                            <div class="card-body p-0">
                                                <asp:UpdatePanel runat="server" ID="pnlTwoFactor" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <div class="d-flex align-items-center flex-wrap gap-2">
                                                            <h4 class="fw-bold mb-2"><%= GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION_IS) %></h4>
                                                            <span runat="server" id="spStatusTwoFA" class="mb-2"></span>
                                                        </div>
                                                        <div runat="server" id="divResetTwoFA" visible="false">
                                                            <h5 class="fw-bold mb-2"><%= GetResourceText(BackEndResourceKeys.RESET_AUTHENTICATOR_APP) %></h5>
                                                            <ol>
                                                                <li><%= GetResourceText(BackEndResourceKeys.ENTER_YOUR_CURRENT_AUTHENTICATOR_APP_CODE_TO_TURN_OFF_THIS_FEATURE) %></li>
                                                            </ol>
                                                        </div>
                                                        <div runat="server" id="divInstructionsIntegrateTwoFA">
                                                            <h6 class="fw-bold mb-2"><%= GetResourceText(BackEndResourceKeys.INSTRUCTIONS_FOR_SETUP) %></h6>
                                                            <ol>
                                                                <li>
                                                                    <p class="fw-bold mb-1"><%= GetResourceText(BackEndResourceKeys.DOWNLOAD_AUTHENTICATION_APP) %></p>
                                                                    <p class="mb-1"><%= GetResourceText(BackEndResourceKeys.WE_RECOMMEND_DOWNLOADING_GOOGLE_AUTHENTICATOR_IF_YOU_DONT_HAVE_ONE_INSTALLED) %></p>
                                                                </li>
                                                                <li>
                                                                    <p class="fw-bold mb-1"><%= GetResourceText(BackEndResourceKeys.SCAN_THIS_QRCODE_OR_COPY_THE_KEY) %></p>
                                                                    <p class="mb-1"><%= GetResourceText(BackEndResourceKeys.SCAN_THIS_QRCODE_IN_THE_AUTHENTICATION_APP_OR_COPY_THE_KEY_AND_PASTE_IT_IN_THE_AUTHENTICATION_APP) %></p>
                                                                    <div class="mb-1 align-items-center text-center">
                                                                        <img src="#" runat="server" id="imgScretKey" style="max-width: 200px; background: #f5eeee; border-radius: 10px; padding: 12px;" />
                                                                        <div class="d-block">
                                                                            <span class="fw-bold mx-2" id="spSecretKey" runat="server"></span>
                                                                            <a href="javascript:;" class="text-primary btn-copy-text" data-clipboard-action="copy" data-clipboard-target="#<%= spSecretKey.ClientID %>" title="<%= GetResourceText(BackEndResourceKeys.COPY) %>">
                                                                                <i class="fa fa-clone" aria-hidden="true"></i>
                                                                            </a>
                                                                        </div>
                                                                    </div>
                                                                </li>
                                                                <li>
                                                                    <p class="fw-bold mb-1"><%= GetResourceText(BackEndResourceKeys.COPY_AND_ENTER_6_DIGIT_CODE) %></p>
                                                                    <p class="mb-1"><%= GetResourceText(BackEndResourceKeys.AFTER_THE_QRCODE_IS_SCANNED_OR_THE_KEY_IS_ENTERED_YOUR_AUTHENTICATOR_APP_GENERATES_A_6_DIGIT_CODE_COPY_THE_CODE_THEN_COME_BACK_HERE_TO_ENTER_IT) %></p>
                                                                </li>
                                                            </ol>
                                                        </div>
                                                        <p class="fw-bold ms-3"><%= GetResourceText(BackEndResourceKeys.ENTER_THE_6_DIGIT_CODE_GENERATED_BY_YOUR_AUTHENTICATOR_APP) %></p>
                                                        <SweetSoft:CtrlTwoFactorAuthenticator runat="server" ID="CtrlToggleTwoFA" />
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
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
