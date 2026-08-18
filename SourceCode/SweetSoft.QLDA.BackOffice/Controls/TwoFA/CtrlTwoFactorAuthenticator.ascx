<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTwoFactorAuthenticator.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.TwoFA.CtrlTwoFactorAuthenticator" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="mb-4 relative form-two-factor-authentication validationEngineContainer" runat="server" id="divTwoFA" enableviewstate="false">
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
<div class="text-center">
    <SweetSoft:ExtraButton runat="server" ID="lbtVerify" CssClass="btn btn-primary" TabIndex="3" ButtonIcon="UnLock" OnClick="lbtVerify_Click">Xác minh</SweetSoft:ExtraButton>
</div>
<div>
    <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="upHiddenField">
        <ContentTemplate>
            <input type="hidden" runat="server" id="txtGoogleAuthentication" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>