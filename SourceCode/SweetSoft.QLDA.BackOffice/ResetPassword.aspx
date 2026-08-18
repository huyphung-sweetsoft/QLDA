<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/BasicTemplate.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.ResetPassword" %>

<%@ Register Src="~/Controls/Captcha.ascx" TagPrefix="SweetSoft" TagName="Captcha" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="text-center mt-4 mb-3">
        <h2 class="mb-2 fw-bold text-primary"><%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %></h2>
        <div runat="server" id="divAlert" class="alert-success text-center my-4 js-alert rounded-2 p-2" role="alert">
            Nhập mật khẩu mới của bạn
        </div>
    </div>
    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="js-validation validationEngineContainer" onkeydown="CMSMasterJs.EnterSubmit(event, this);">
                <div class="mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NEW_PASSWORD) %></label>
                    <div class="input-group auth-pass-inputgroup">
                        <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" PlaceHolder="Enter the value" TextMode="Password" AriaLabel="Password" AriaDescribedby="password-addon" Required="true"></SweetSoft:ExtraTextBox>
                        <button class="btn btn-light shadow-none ms-0 border" type="button" id="password-addon"><i class="mdi mdi-eye-outline"></i></button>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label"><%=GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %></label>
                    <div class="input-group auth-pass-inputgroup">
                        <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" PlaceHolder="Enter the value" TextMode="Password" AriaLabel="Password" AriaDescribedby="password-addon-confirm" Required="true"></SweetSoft:ExtraTextBox>
                        <button class="btn btn-light shadow-none ms-0 border" type="button" id="password-addon-confirm"><i class="mdi mdi-eye-outline"></i></button>
                    </div>
                </div>
                <SweetSoft:Captcha runat="server" ID="Captcha1" ClientId="RESET-PASSWORD" />
                <div class="alert-info alert-dismissible alert-outline text-center mt-3 js-alert-submit d-none rounded-2 p-2" role="alert">
                </div>
                <div class="mb-3 mt-3 text-center wrapBtnAction">
                    <SweetSoft:ExtraButton runat="server" ID="lbtConfirm" CssClass="btn-primary w-50 waves-effect waves-light" ButtonIcon="Accept" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtConfirm_Click">Confirm</SweetSoft:ExtraButton>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="mt-5 text-center">
        <p class="text-page mb-0">
            <%= GetResourceText(BackEndResourceKeys.REMEMBER_PASSWORD) %> <a href="<%= GetRelativeClientPath("/login") %>" class="fw-semibold"
                title="<%= GetResourceText(BackEndResourceKeys.LOGIN) %>"><%= GetResourceText(BackEndResourceKeys.LOGIN) %></a>
        </p>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script src="/Styles/js/pages/pass-addon.init.js"></script>
    <script>
        function errorToken() {
            const $el = $('.js-alert');
            $el.removeClass('.alert-success');
            $el.addClass('alert-danger');
            $el.text('<%= GetResourceText(BackEndResourceKeys.THE_REQUESTED_LINK_IS_INCORRECT_OR_HAS_EXPIRED) %>');
        }
        $(function () {
            $("#password-addon-confirm").on("click", function () { 0 < $(this).siblings("input").length && ("password" == $(this).siblings("input").attr("type") ? $(this).siblings("input").attr("type", "input") : $(this).siblings("input").attr("type", "password")) });
        })
    </script>
</asp:Content>
