<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="RoleDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.RoleDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fUsers/Controls/CtrlPermission.ascx" TagPrefix="SweetSoft" TagName="CtrlPermission" %>
<%@ Register Src="~/fUsers/Controls/CtrlUsers.ascx" TagPrefix="SweetSoft" TagName="CtrlUsers" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Nhóm người dùng" />
                <div class="flex-between flex-between-xl gap-4">
                    <div class="tabs-horizontal">
                        <ul class="nav nav-pills card-header-pills" role="tablist">
                            <li class="nav-item">
                                <a class="nav-link px-1 active" data-bs-toggle="tab" href="#overview" role="tab">
                                    <%= GetResourceText(BackEndResourceKeys.BASIC_INFORMATION) %>
                                </a>
                            </li>
                            <li runat="server" id="tagUsers" visible="false" class="nav-item" onclick="CMSMasterJs.LoadTab(this,'user-list')">
                                <a class="nav-link px-3" data-bs-toggle="tab" href="#account" role="tab">
                                    <%= GetResourceText(BackEndResourceKeys.USER_IN_ROLE) %>
                                </a>
                            </li>
                        </ul>
                    </div>
                    <div class="flex-center gap-2 mb-2 justify-content-end">
                        <SweetSoft:ExtraButton Visible="false" runat="server" ID="lbtDelete" CssClass="waves-effect waves-light" ButtonStyle="OutLineDanger" ButtonIcon="Remove" OnClick="lbtDelete_Click">Xóa</SweetSoft:ExtraButton>
                        <SweetSoft:ExtraButton Visible="false" runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="btnSave_ServerClick">Lưu</SweetSoft:ExtraButton>
                        <SweetSoft:ExtraButton runat="server" ID="lbtBack" NavigateUrl="/Roles" CssClass="btn-outline-secondary waves-effect" ButtonIcon="Reply" IsSubmit="false">Quay về danh sách</SweetSoft:ExtraButton>
                    </div>
                </div>
                <div class="card-body p-0">
                    <div class="tab-content text-muted tab-overide">
                        <div class="tab-pane active" id="overview" role="tabpanel">
                            <div class="card card-grid-view">
                                <div class="card-body pt-2 pb-3">
                                    <div class="row js-validation validationEngineContainer">
                                        <div class="col-lg-3 col-md-4 col-sm-6">
                                            <asp:UpdatePanel runat="server" ID="pnlValid" UpdateMode="Conditional">
                                                <ContentTemplate>
                                                    <fieldset class="fieldset-box">
                                                        <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.BASIC_INFORMATION) %></legend>
                                                        <div class="mb-3">
                                                            <label class="form-label label-valid"><%=GetResourceText(BackEndResourceKeys.NAME) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtRoleName" Required="true" PlaceHolder="Nhập tên phòng ban"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                                                            <SweetSoft:ExtraTextBox runat="server" ID="txtSummary" TextMode="MultiLine" PlaceHolder="Nhập mô tả" Rows="3"></SweetSoft:ExtraTextBox>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                                            <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Kích hoạt" OffText="Khóa" Checked="true" />
                                                        </div>
                                                    </fieldset>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                        <div class="col-lg-9 col-md-8 col-sm-6">
                                            <SweetSoft:CtrlPermission runat="server" ID="CtrlPermission1" />
                                        </div>
                                    </div>
                                </div>
                                <!-- end card body -->
                            </div>
                            <!-- end card -->
                        </div>
                        <div class="tab-pane" id="account">
                            <SweetSoft:CtrlUsers runat="server" ID="CtrlUsers1" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
