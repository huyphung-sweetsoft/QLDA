<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Captcha.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Captcha" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="mb-4 position-relative col-captcha">
    <label class="form-label mb-0"><%=GetResourceText(BackEndResourceKeys.SECURIRY_CODE) %></label>
    <asp:UpdatePanel runat="server" ID="pnlCaptcha" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row" style="margin-left:0 !important; margin-right:0 !important">
                <div class="col-sm-6 col-xs-6 col-5 colContentContact colCapcha p-0">
                    <SweetSoft:ExtraTextBox runat="server" ID="txtValidCode" 
                        PlaceHolder="Enter the value" 
                        autocomplete="off"
                        MaxLength="5" />
                </div>
                <div class="col-sm-6 col-xs-6 col-7 colContentContact colCapcha p-0">
                    <div class="d-flex align-items-center">
                        <asp:Image ID="imgCaptcha" runat="server" 
                            CssClass="imageCapcha border" 
                            onclick="refreshCaptcha()"
                            AlternateText="Security Code" />
                        <asp:ImageButton runat="server" 
                            ID="btnRefreshCode" 
                            CssClass="refresh-security" 
                            AlternateText="Refresh" 
                            ToolTip="Làm mới mã bảo vệ" 
                            ImageUrl="/Styles/images/refresh.png"
                            OnClick="ChangeCaptchaImage"
                            CausesValidation="false" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnRefreshCode" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</div>

<script type="text/javascript">
    function preventEnterKey(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            return false;
        }
        return true;
    }

    function refreshCaptcha() {
        var btnRefresh = document.getElementById('<%= btnRefreshCode.ClientID %>');
        if (btnRefresh) {
            __doPostBack('<%= btnRefreshCode.UniqueID %>', '');
        }
    }

    function focusTextbox() {
        var textbox = document.getElementById('<%= txtValidCode.ClientID %>');
        if (textbox) {
            textbox.focus();
        }
    }
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        setTimeout(focusTextbox, 100);
    });
</script>

<style>
    .imageCapcha {
        height: 33.59px;
        border-radius: 4px;
        transition: opacity 0.3s ease;
    }

        .imageCapcha:hover {
            opacity: 0.8;
            cursor:pointer;
        }

    .refresh-security {
        cursor: pointer;
        border: none;
        background: transparent;
        padding: 2px;
        border-radius: 3px;
        transition: background-color 0.3s ease;
        width: 26px;
        height: 26px;
    }
    
    .refresh-security:hover {
        background-color: transparent;
        transform: rotate(90deg);
        transition: all 0.3s ease;
    }

    .col-captcha .form-label {
        font-weight: 500;
        margin-bottom: 0.5rem;
    }
</style>
