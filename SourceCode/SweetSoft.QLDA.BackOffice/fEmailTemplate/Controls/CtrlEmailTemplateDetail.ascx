<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlEmailTemplateDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmailTemplate.Controls.CtrlEmailTemplateDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="flex-between flex-between-xl gap-4">
    <div class="tabs-horizontal">
        <ul class="nav nav-pills card-header-pills" role="tablist">
            <li class="nav-item">
                <a class="nav-link px-2 active" data-bs-toggle="tab" href="#overview" role="tab">
                    <%= GetResourceText(BackEndResourceKeys.BASIC_INFORMATION) %>
                </a>
            </li>
        </ul>
    </div>
    <div class="flex-center gap-2 mb-4 justify-content-end">
        <SweetSoft:ExtraButton Visible="false" runat="server" ID="lbtDelete" CssClass="waves-effect waves-light" ButtonStyle="Danger" ButtonIcon="Remove" OnClick="lbtDelete_Click"></SweetSoft:ExtraButton>
        <SweetSoft:ExtraButton Visible="false" runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="btnSave_ServerClick"></SweetSoft:ExtraButton>
        <SweetSoft:ExtraButton runat="server" ID="lbtBack" NavigateUrl="/email-templates" CssClass="btn-outline-secondary waves-effect" ButtonIcon="Reply" IsSubmit="false"></SweetSoft:ExtraButton>
    </div>
</div>
<div class="card-body p-0">
    <div class="tab-content text-muted tab-overide">
        <div class="tab-pane active" id="overview" role="tabpanel">
            <div class="card card-grid-view">
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="row js-validation validationEngineContainer">
                                <div class="col-12 col-md-12">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%=GetResourceText(BackEndResourceKeys.TEMPLATE_NAME) %></label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtName" Required="true" PlaceHolder="Enter the value"
                                            MaxLength="255"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-12 col-md-12">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%=GetResourceText(BackEndResourceKeys.EMAIL_SUBJECT) %></label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSubject" Required="true" PlaceHolder="Enter the value"
                                            MaxLength="255"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-12 col-md-3">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%=GetResourceText(BackEndResourceKeys.TEMPLATE_KEY) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTemplateKey" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-12 col-md-3">
                                    <div class="mb-3">
                                        <label class="form-label label-valid">Đối tượng áp dụng</label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlEmailFormatType" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-12 col-md-4">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                        <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Active" OffText="Inactive" Checked="true" TabIndex="4" />
                                    </div>
                                </div>
                                <div class="col-12 col-md-6">
                                    <div class="mb-3">
                                        <label class="form-label">CC Email</label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtCCEmail" TabIndex="5" PlaceHolder="Enter the value" MaxLength="2000"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-12 col-md-6">
                                    <div class="mb-3">
                                        <label class="form-label">BCC Email</label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtBCCEmail" TabIndex="6" PlaceHolder="Enter the value" MaxLength="2000"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CONTENT) %></label>
                                        <CKEditor:CKEditorControl ID="txtBody" Width="100%" CssClass="ck-editor"
                                            Toolbar="Full" BodyId="StaticPageContent" Language="vi-VN" AutoParagraph="false"
                                            BasePath="~/Styles/plugins/ckeditor/" runat="server" Height="600">
                                        </CKEditor:CKEditorControl>
                                    </div>
                                </div>
                                <div class="col-lg-12" runat="server" id="divSystem" visible="false">
                                    <ul class="list-group">
                                        <li class="list-group-item">
                                            <div class="flex item-center gap-2 justify-content-between">
                                                <%= GetResourceText(BackEndResourceKeys.CREATED_BY) %> :
                                                     <asp:Label runat="server" ID="lbCreateBy"></asp:Label>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="flex item-center gap-2 justify-content-between">
                                                <%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %> :
                                                     <asp:Label runat="server" ID="lbCreatedDate"></asp:Label>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="flex item-center gap-2 justify-content-between">
                                                <%= GetResourceText(BackEndResourceKeys.UPDATED_BY) %> :
                                                     <asp:Label runat="server" ID="lbUpdatedBy"></asp:Label>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="flex item-center gap-2 justify-content-between">
                                                <%= GetResourceText(BackEndResourceKeys.UPDATED_DATE) %> : 
                                                     <asp:Label runat="server" ID="lbUpdatedDate"></asp:Label>
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
</div>
