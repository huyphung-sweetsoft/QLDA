<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAn" %>

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
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project list" />
                <SweetSoft:CtrlDuAn runat="server" id="CtrlDuAn1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Project Infomation">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_IDENTIFIED) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMaDuAn" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenDuAn" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlLoaiDuAn" Required="true" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlKhachHang" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <asp:UpdatePanel runat="server" ID="upHopDong" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="row">
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_NUMBER) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtSoHopDong" OnTextChanged="txtSoHopDong_TextChanged" AutoPostBack="true" Required="false"/>
                                 </div>
                             </div>
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDong" Enabled="false" Required="false"/>
                                 </div>
                             </div>
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtNgayKy" Enabled="false" Required="false"/>
                                 </div>
                             </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></label>
                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayBatDau" SingleDatePicker="true" PlaceHolder="Select start date" />
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></label>
                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayKetThuc" SingleDatePicker="true" PlaceHolder="Select end date" />
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" PlaceHolder="Select status" />
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
