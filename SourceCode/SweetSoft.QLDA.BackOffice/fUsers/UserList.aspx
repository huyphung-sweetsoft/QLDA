<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.UserList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fUsers/Controls/CtrlUsers.ascx" TagPrefix="SweetSoft" TagName="CtrlUsers" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        div[data-edit="true"] {
            display: none;
        }

            div[data-edit="true"].show {
                display: block;
            }
            .file-box-single{
                width:100px;
            }
            .file-box .uploaded-content .item img{
                width: 60px;
            }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Account list" />
                <SweetSoft:CtrlUsers runat="server" id="CtrlUsers1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Account Information" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="true" MaxLength="50" RequiredAdvanced="custom[username]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Email</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true"
                            RequiredAdvanced="custome[email]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlRole" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Active" OffText="Lock" Checked="true" />
                    </div>
                </div>
                <div runat="server" id="divImage" visible="false" class="col-lg-6 hidden d-none">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IMAGE) %></label>
                        <SweetSoft:FilesBox runat="server" ID="fbImage" />
                    </div>
                </div>
                <div runat="server" id="divChangePassword" visible="false" class="col-lg-12">
                    <div class="form-check mb-3">
                        <input class="form-check-input" type="checkbox" id="chkChangePassword" runat="server" onclick="CMSMasterJs.ChangePassword(this);">
                        <label class="form-check-label" for="<%= chkChangePassword.ClientID %>">
                            <%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                        </label>
                    </div>
                </div>
                <div runat="server" id="divPassword" data-selector="password" class="col-lg-12">
                    <div class="row">
                        <div class="col-lg-6">
                            <div class="mb-3">
                                <label for="<%= txtPassword.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.PASSWORD) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" TextMode="Password" PlaceHolder="Enter the value"
                                    Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="mb-3">
                                <label for="<%= txtConfirmPassword.ClientID %>" class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %>
                                </label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" PlaceHolder="Enter the value"
                                    Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                        OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        CMSMasterJs.ChangePassword = function (t) {
            $('[data-selector="password"]').toggleClass('show');
        }
        CMSMasterJs.HideChangePwd = function (t) {
            $('[data-selector="password"]').removeClass('show');
        }
        $(document).ready(function () {
            CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
        });
    </script>
</asp:Content>
