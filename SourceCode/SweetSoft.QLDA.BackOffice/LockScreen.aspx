<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/BasicTemplate.Master" AutoEventWireup="true" CodeBehind="LockScreen.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.LockScreen" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="text-center mt-4 mb-3">
        <h2 class="mb-2 fw-bold text-primary"><%= GetResourceText(BackEndResourceKeys.LOCK_SCREEN) %></h2>
        <p class="mt-2"><%=GetResourceText(BackEndResourceKeys.ENTER_YOUR_PASSWORD_TO_UNLOCK_THE_SCREEN) %></p>
    </div>
    <div class="user-thumb text-center mb-4 mt-3 pt-2">
        <img runat="server" id="imgAvatar" src="/Styles/images/user-icon.png" class="rounded-circle avatar-lg" alt="thumbnail">
        <h5 runat="server" id="tagUserName" class="font-size-4 mt-3 mb-0"></h5>
    </div>
    <div class="js-validation validationEngineContainer mt-4" onkeydown="CMSMasterJs.EnterSubmit(event, this);" data-enter-id="<%= lbtUnLock.ClientID %>">
        <div class="input-group auth-pass-inputgroup">
            <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" PlaceHolder="Enter the value" CssClass="ignore" TextMode="Password"
                TabIndex="1" AriaLabel="Password" AriaDescribedby="password-addon" Required="true"></SweetSoft:ExtraTextBox>
            <button class="btn btn-light shadow-none ms-0 border" type="button" id="password-addon"><i class="mdi mdi-eye-outline"></i></button>
        </div>
        <div class="mb-3 mt-3 text-center">
            <SweetSoft:ExtraButton runat="server" ID="lbtUnLock" CssClass="btn-primary w-50 waves-effect waves-light" TabIndex="2" ButtonIcon="UnLock" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtUnLock_Click">Mở khóa</SweetSoft:ExtraButton>
        </div>
    </div>

    <div class="mt-5 text-center">
        <p class="mb-0">
            <%=GetResourceText(BackEndResourceKeys.NOT_YOU_GO_BACK) %> <a runat="server" onserverclick="Unnamed_ServerClick" class="fw-semibold text-primary"
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
</asp:Content>

