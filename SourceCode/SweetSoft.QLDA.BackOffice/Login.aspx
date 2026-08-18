<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/BasicTemplate.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.Login" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/TwoFA/CtrlTwoFactorAuthenticator.ascx" TagPrefix="SweetSoft" TagName="CtrlTwoFactorAuthenticator" %>
<%@ Register Src="~/Controls/Captcha.ascx" TagPrefix="SweetSoft" TagName="Captcha" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .otp-input {
            display: flex;
            justify-content: center;
            margin-bottom: 2rem;
        }

            .otp-input input {
                width: 50px;
                height: 50px;
                margin: 0 8px;
                text-align: center !important;
                font-size: 1.5rem;
                border: 2px solid #4a1387;
                border-radius: 12px;
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
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="text-center">
                <h2 class="mb-2 fw-bold text-primary">PM Quản Lý Dự Án</h2>
                <p class="text-muted mt-2" runat="server" id="hTitle">Sign in to continue to System.</p>
            </div>
            <div class="mt-4 pt-2">
                <div runat="server" id="divLogin">
                    <div class="js-validation validationEngineContainer" onkeydown="CMSMasterJs.EnterSubmit(event, this);"
                        data-enter-id="<%= lbtLogin.ClientID %>">
                        <div class="mb-4">
                            <label class="form-label" for="<%= txtUserName.ClientID %>"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" PlaceHolder="Enter the value" CssClass="ignore" Required="true"></SweetSoft:ExtraTextBox>
                        </div>
                        <div class="mb-4">
                            <label class="form-label" for="<%= txtPassword.ClientID %>"><%= GetResourceText(BackEndResourceKeys.PASSWORD) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" PlaceHolder="Enter the value" CssClass="ignore" TextMode="Password" AriaLabel="Password" Required="true"></SweetSoft:ExtraTextBox>
                        </div>
                        <SweetSoft:Captcha runat="server" ID="CtrlCaptcha" ClientId="USER_LOGIN" />
                        <div runat="server" visible="false" class="mb-4">
                            <label class="form-label" for="<%= txtPassword.ClientID %>"><%= GetResourceText(BackEndResourceKeys.LANGUAGE) %></label>
                            <div class="ms-2">
                                <asp:LinkButton runat="server" ID="lbtEN" OnClick="ChangeLanguage">
                            <img src="<%=GetRelativeClientPath("/Styles/images/lang-en.jpg") %>" />
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" ID="lbtVI" OnClick="ChangeLanguage">
                            <img src="<%=GetRelativeClientPath("/Styles/images/lang-vi.jpg") %>" />
                                </asp:LinkButton>
                            </div>
                        </div>
                        <div class="flex-between mt-3 mb-4">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="chkRememberCheck" runat="server">
                                <label class="form-check-label" for="<%= chkRememberCheck.ClientID %>">
                                    <%=GetResourceText(BackEndResourceKeys.REMEMBER_ME) %>
                                </label>
                            </div>
                            <a href="<%= GetRelativeClientPath("/forgot-password") %>">
                                <small class="text-primary"><%= GetResourceText(BackEndResourceKeys.FORGOT_PASSWORD) %>?</small>
                            </a>
                        </div>
                        <SweetSoft:ExtraButton runat="server" ID="lbtLogin" CssClass="btn btn-primary w-100" ButtonIcon="UnLock" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtLogin_Click">Login</SweetSoft:ExtraButton>
                    </div>
                </div>
                <div runat="server" id="divTwoFactorAuthentication" visible="false">
                    <div class="mb-4 relative form-two-factor-authentication validationEngineContainer">
                        <div class="otp-input">
                            <div>
                                <input type="number" maxlength="1" tabindex="1" />
                            </div>
                            <div>
                                <input type="number" maxlength="1" tabindex="2" />
                            </div>
                            <div>
                                <input type="number" maxlength="1" tabindex="3" />
                            </div>
                            <div>
                                <input type="number" maxlength="1" tabindex="4" />
                            </div>
                            <div>
                                <input type="number" maxlength="1" tabindex="5" />
                            </div>
                            <div>
                                <input type="number" maxlength="1" tabindex="6" />
                            </div>
                        </div>
                    </div>
                    <input type="hidden" runat="server" id="txtGoogleAuthentication" />
                    <SweetSoft:ExtraButton runat="server" ID="lbtVerify" CssClass="btn btn-primary w-100" ButtonIcon="UnLock" OnClientClick="return verifyOTP();" OnClick="lbtVerify_Click">Xác minh</SweetSoft:ExtraButton>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script src="<%= GetRelativeClientPath("/Styles/js/pages/pass-addon.init.js") %>"></script>
    <script>
        var LoginJs = {};
        LoginJs.init = () => {
            const inputs = document.querySelectorAll('.otp-input input');
            inputs.forEach((input, index) => {
                input.addEventListener('input', (e) => {
                    if (e.target.value.length > 1) {
                        e.target.value = e.target.value.slice(0, 1);
                    }
                    if (e.target.value.length === 1) {
                        if (index < inputs.length - 1) {
                            inputs[index + 1].focus();
                        }
                    }
                });

                input.addEventListener('keydown', (e) => {
                    if (e.key === 'Backspace' && !e.target.value) {
                        if (index > 0) {
                            inputs[index - 1].focus();
                        }
                    }
                    if (e.key === 'e') {
                        e.preventDefault();
                    }
                });
            });
            verifyOTP = () => {
                const isValid = CMSMasterJs.ValidElement('.form-two-factor-authentication');
                if (!isValid) {
                    toastr.error('<%= GetResourceText(BackEndResourceKeys.PLEASE_ENTER_A_6_DIGIT_OTP) %>');
                    return false;
                }

                const otp = Array.from(inputs).map(input => input.value).join('');
                if (otp.length === 6) {
                    $('#<%= txtGoogleAuthentication.ClientID %>').val(otp);
                    return true;
                } else {
                    toastr.error('<%= GetResourceText(BackEndResourceKeys.PLEASE_ENTER_A_6_DIGIT_OTP) %>');
                    return false;
                }
            }
        }
    </script>
</asp:Content>
