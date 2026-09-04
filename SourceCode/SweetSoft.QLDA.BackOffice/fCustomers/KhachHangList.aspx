<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="KhachHangList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCustomers.KhachHangList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.EnumHelper.Defines" %>
<%@ Register Src="~/fCustomers/Controls/CtrlKhachHang.ascx" TagPrefix="SweetSoft" TagName="CtrlKhachHang" %>

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
            <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Customer list" />
            <SweetSoft:CtrlKhachHang runat="server" id="CtrlKhachHang" />
        </div>
    </div>
</div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Customer Information">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer">
            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_NAME) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenKhachHang" Required="true"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_TYPE) %></label>
                    <SweetSoft:ExtraDropdown runat="server" ID="ddlLoaiKhachHang" Required="true" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.TAX_CODE) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtIdSoThue" Required="true"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoDienThoai" Required="true"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label label-valid">Email</label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true"/>
                </div>
            </div>
            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" Required="true"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-12">
                <div class="mb-3">
                    <h5 class="text-uppercase fw-bold mb-3"><%= GetResourceText(BackEndResourceKeys.CONTACT_INFORMATION) %></h5>
                </div>
            </div>
            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTACT_PERSON) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtNguoiLienHe"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtEmailLienHe"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtSDTLienHe"></SweetSoft:ExtraTextBox>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                    <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Active" OffText="Lock" Checked="true" />
                </div>
            </div>
            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                    <CKEditor:CKEditorControl ID="txtMoTa" Width="100%" CssClass="ck-editor"
                        Toolbar="Full" BodyId="StatucPageContent" Language="vi-VN" AutoParagraph="false"
                        BasePath="~/Styles/plugins/ckeditor/" runat="server" Height="200">
                    </CKEditor:CKEditorControl>
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
</asp:Content>
