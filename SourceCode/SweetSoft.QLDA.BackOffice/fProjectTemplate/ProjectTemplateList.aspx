<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="ProjectTemplateList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectTemplate.ProjectTemplateList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjectTemplate/Controls/CtrlProjectTemplate.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTemplate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTemplate runat="server" ID="CtrlProjectTemplate1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="mdlTemplate" Type="Primary" Title="Thêm Mẫu Dự Án Mới" DefaultButton="btnSave">
        <ContentTemplate>
            <div class="row g-3 p-2 js-validation validationEngineContainer">
                <div class="col-12">
                    <label class="form-label label-valid">Tên mẫu dự án <span class="text-danger">*</span></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenMau" Required="true" PlaceHolder="VD: Xây dựng Website Bán Hàng"></SweetSoft:ExtraTextBox>
                </div>
                <div class="col-12">
                    <label class="form-label">Mô tả chi tiết</label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" TextMode="MultiLine" Rows="3" PlaceHolder="Mô tả về quy mô, mục đích của mẫu này..."></SweetSoft:ExtraTextBox>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="btnSave" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="btnSave_Click">Tạo mẫu & Tiếp tục</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server"></asp:Content>