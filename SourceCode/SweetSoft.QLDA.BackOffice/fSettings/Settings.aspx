<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.Settings" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<%-----------------------------PROGRAMER LOGS-------------------------------
--%>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="System settings" />
                <div class="flex-between flex-between-xl gap-4">
                    <div class="tabs-horizontal">
                        <ul class="nav nav-pills card-header-pills" role="tablist">
                            <li class="nav-item">
                                <a class="nav-link px-2 active" data-bs-toggle="tab" href="#overview" role="tab"><%= GetResourceText(BackEndResourceKeys.OVERVIEW) %></a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link px-2" data-bs-toggle="tab" href="#company-info" role="tab">
                                    <%= GetResourceText(BackEndResourceKeys.CONTACT_INFORMATION) %>
                                </a>
                            </li>
                        </ul>
                    </div>
                    <div class="flex-center gap-2 mb-4 justify-content-end">
                        <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click">Save settings</SweetSoft:ExtraButton>
                        <SweetSoft:ExtraButton runat="server" ID="lbtClearCache" ButtonStyle="Secondary" ButtonIcon="Refresh" IsPace="true" OnClick="lbtClearCache_Click">Clear cache</SweetSoft:ExtraButton>
                    </div>
                </div>
                <div class="card-body p-0">
                    <div class="tab-content text-muted tab-overide">
                        <%--General information--%>
                        <div class="tab-pane active js-validation validationEngineContainer" id="overview" role="tabpanel">
                            <div class="card">
                                <div class="card-body pt-0">
                                    <div class="row">
                                        <div class="col-lg-16 col-md-6 col-sm-12 mt-2">
                                            <fieldset class="fieldset-box">
                                                <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.SETTINGS) %></legend>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <div class="mt-3 mt-lg-0">
                                                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.TITLE_OF_WEBSITE) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtSiteTitle" PlaceHolder="Enter the value" Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <div class="mt-3 mt-lg-0">
                                                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ADMIN_EMAIL) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtAdminEmail" PlaceHolder="Enter the value" Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <div class="mt-3">
                                                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMAIL_RECEIVED_ERROR_MESSAGE) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtEmailErrorReport" PlaceHolder="Enter the value" Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <div class="mt-3">
                                                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.NUMBER_OF_RESULTS_ON_THE_GRID) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtNumberItemOfGrid" PlaceHolder="Enter the value" Required="true"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                    </div>
                                                    <div runat="server" visible="false" class="col-lg-6">
                                                        <div class="mt-3">
                                                            <label class="form-label label-valid" title="<%= GetResourceText(BackEndResourceKeys.REQUIRED) %>"><%= GetResourceText(BackEndResourceKeys.HANDLER) %></label>
                                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlDefaultProcessor" PlaceHolder="Select value" Required="false"></SweetSoft:ExtraDropdown>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4 mt-3">
                                                        <div class="form-group">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SELECTION_PANE_ON_THE_PAGE) %></label>
                                                            <SweetSoft:ExtraCheckbox runat="server" ID="chkPreventSelection" OnText="On" OffText="Off" Checked="true" />
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4 mt-3">
                                                        <div class="form-group">
                                                            <label class="form-label"><%=GetResourceText(BackEndResourceKeys.PREVENT_RIGHT_CLICK) %></label>
                                                            <SweetSoft:ExtraCheckbox runat="server" ID="chkPreventRightClick" OnText="On" OffText="Off" Checked="true" />
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4 mt-3">
                                                        <div class="form-group">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SAVE_LOG) %></label>
                                                            <SweetSoft:ExtraCheckbox runat="server" ID="chkSaveLog" OnText="On" OffText="Off" Checked="true" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </fieldset>
                                        </div>
                                        <div class="col-lg-16 col-md-6 col-sm-12 mt-2">
                                            <fieldset class="fieldset-box">
                                                <legend class="text-primary fw-bold">SMTP</legend>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <div>
                                                            <div>
                                                                <label for="<%= txtServer.ClientID %>" class="form-label">SMTP server</label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtServer" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                            </div>

                                                            <div class="mt-3">
                                                                <label for="<%= txtAccount.ClientID %>" class="form-label">
                                                                    <%= GetResourceText(BackEndResourceKeys.ACCOUNT) %>
                                                                </label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtAccount" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                            </div>
                                                            <div class="mt-3">
                                                                <label for="<%= txtPassword.ClientID %>" class="form-label">
                                                                    <%= GetResourceText(BackEndResourceKeys.PASSWORD) %>
                                                                </label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" PlaceHolder="Enter the value" TextMode="Password" Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <!-- end col -->

                                                    <div class="col-lg-6">
                                                        <div class="mt-3 mt-lg-0">
                                                            <div>
                                                                <label for="<%= txtSenderEmail.ClientID %>" class="form-label">
                                                                    <%= GetResourceText(BackEndResourceKeys.FROM_EMAIL) %>
                                                                </label>
                                                                <SweetSoft:ExtraTextBox runat="server" ID="txtSenderEmail" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                            </div>
                                                            <div class="mt-3">
                                                                <div class="d-flex align-items-start">
                                                                    <div class="flex-grow-1 me-3">
                                                                        <label for="<%= txtPort.ClientID %>" class="form-label">Port</label>
                                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtPort" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                                    </div>
                                                                    <div class="flex-grow-1">
                                                                        <label for="<%= chkUsingSLL.ClientID %>" class="form-label"><%=GetResourceText(BackEndResourceKeys.USING_SSL) %></label>
                                                                        <SweetSoft:ExtraCheckbox runat="server" ID="chkUsingSLL" OnText="On" OffText="Off" />
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="mt-3">
                                                                <label for="<%= txtTestEmail.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.CHECK_SMTP) %></label>
                                                                <div class="input-group">
                                                                    <SweetSoft:ExtraTextBox runat="server" ID="txtTestEmail" CssClass="me-auto" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                                    <SweetSoft:ExtraButton runat="server" ID="lbtTest" CssClass="btn-warning" ButtonIcon="Envelope" IsPace="true" OnClick="lbtTest_Click">Send</SweetSoft:ExtraButton>
                                                                </div>

                                                            </div>
                                                        </div>
                                                    </div>
                                                    <!-- end col -->
                                                </div>
                                            </fieldset>
                                        </div>
                                        <div class="col-lg-12 mt-3">
                                            <span class="text-info fw-bold"><%= GetResourceText(BackEndResourceKeys.OTHER_SETTINGS) %></span>
                                            <hr class="mt-0" />
                                        </div>
                                        <div class="col-lg-12">
                                            <div class="mt-3">
                                                <label class="form-label"><%=GetResourceText(BackEndResourceKeys.INTERNAL_INFORMATION) %></label>
                                                <CKEditor:CKEditorControl ID="txtInternalAnnouncement" Width="100%" CssClass="ck-editor"
                                                    Toolbar="Full" BodyId="StaticPageContent" Language="vi-VN" AutoParagraph="false"
                                                    BasePath="~/Styles/plugins/ckeditor/" runat="server" Height="200">
                                                </CKEditor:CKEditorControl>
                                            </div>
                                        </div>
                                        <%--TimeZone--%>
                                        <div class="col-lg-12 mt-3">
                                            <span class="text-info fw-bold"><%= GetResourceText(BackEndResourceKeys.TIME_ZONE_SETTINGS) %></span>
                                            <hr class="m-0" />
                                        </div>
                                        <asp:UpdatePanel runat="server" ID="pnlTimeZone" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="mt-3">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SERVER_TIME) %></label>
                                                            <div class="d-block">
                                                                <asp:Label runat="server" ID="lbServerTime" Text="Server Time" CssClass="time-zone fw-bold" />
                                                                <asp:Label runat="server" ID="lbServerTimeZone" CssClass="time-zone fw-bold" />

                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="mt-3">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SYSTEM_TIME_ZONE) %></label>
                                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlSelectTimeZone"
                                                                SimpleAjaxInit="true"
                                                                AlowClear="false"
                                                                CloseOnSelect="true"
                                                                Width="50%" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlTimeZone_SelectedIndexChanged">
                                                            </SweetSoft:ExtraDropdown>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="mt-3">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DIFFERENCE) %></label>
                                                            <asp:Label runat="server" ID="lbDifferent" CssClass="time-zone d-block fw-bold" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <%--End TimeZone--%>
                                    </div>
                                </div>
                                <!-- end card body -->
                            </div>
                            <!-- end card -->
                        </div>

                        <%--Company information--%>
                        <div class="tab-pane js-validation validationEngineContainer" id="company-info">
                            <div class="card">
                                <div class="card-body">
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <div class="row">
                                                <div class="col-sm-6">
                                                    <div class="mt-0">
                                                        <label class="form-label"><%=GetResourceText(BackEndResourceKeys.COMPANY_NAME) %></label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyName" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-0">
                                                        <label class="form-label">Email</label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyEmail" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyPhone" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label">Hotline</label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyHotline" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label">Fax</label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyFax" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label"><%=GetResourceText(BackEndResourceKeys.TAX_CODE) %></label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtTaxCode" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label">Messenger</label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtMessengerUrl" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCompanyAddress" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="mt-3">
                                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.LINK_TO_ADDRESS) %></label>
                                                        <SweetSoft:ExtraTextBox runat="server" ID="txtLinkAddress" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
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
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        var SettingJs = {};
        SettingJs.Type = '';
        CustomTargetQuery = (params) => {
            const type = SettingJs.Type;
            var obj = {
                keyword: params.term || "",
                type: type || "",
                page: params.page || 1,
                page_limit: 10
            };
            return JSON.stringify(obj);
        }
        SettingJs.ChangeTarget = (t, s) => {
            $(`[data-selector="${s}"]`).select2("val", "");
            SettingJs.Type = $(t).val();
        }
    </script>
</asp:Content>
