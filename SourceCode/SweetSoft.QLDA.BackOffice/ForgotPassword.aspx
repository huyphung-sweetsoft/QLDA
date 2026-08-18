<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/BasicTemplate.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.ForgotPassword" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/Captcha.ascx" TagPrefix="SweetSoft" TagName="Captcha" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpMain" runat="server">
    <div class="text-center mt-4 mb-3">
        <h2 class="mb-2 fw-bold text-primary"><%= GetResourceText(BackEndResourceKeys.FORGOT_PASSWORD) %></h2>
        <p class="mt-2">Nhập địa chỉ email liên kết tài khoản của bạn</p>
    </div>
    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="custom-width js-validation validationEngineContainer" onkeydown="CMSMasterJs.EnterSubmit(event, this);">
                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <div class="input-group">
                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" PlaceHolder="Enter the value" Required="true" IsEmail="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <SweetSoft:Captcha runat="server" ID="Captcha1" ClientId="FORGOT-PASSWORD" />
                <div class="mb-3 text-center wrapBtnAction" style="margin-top: 50px">
                    <SweetSoft:ExtraButton runat="server" ID="lbtConfirm" CssClass="btn-primary w-100 waves-effect waves-light" ButtonIcon="Accept" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtConfirm_Click">Confirm</SweetSoft:ExtraButton>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="mt-5 text-center">
        <p class="text-page mb-0">
            <%= GetResourceText(BackEndResourceKeys.REMEMBER_PASSWORD) %> <a href="<%= GetRelativeClientPath("/login") %>" class="fw-semibold text-primary"
                title="<%= GetResourceText(BackEndResourceKeys.LOGIN) %>"><%= GetResourceText(BackEndResourceKeys.LOGIN) %></a>
        </p>
    </div>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
