<%@ Page Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="RiskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.RiskList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fRisks/Controls/CtrlRisk.ascx" TagPrefix="SweetSoft" TagName="CtrlRisk" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlRisk runat="server" id="CtrlRisk1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Risk Information" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.RISK_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenRuiRo" Required="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.MONITOR) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVien" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PROBABILITY) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlXacSuat" Required="true" SimpleInit="true" PlaceHolder="Select the value" AutoPostBack="true" OnSelectedIndexChanged="ddlXacSuat_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                    </div>
               </div>
               <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true" PlaceHolder="Select the value" 
                            AutoPostBack="true" OnSelectedIndexChanged="ddlMucDoAnhHuong_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.RISK_LEVEL) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMucDoRuiRo" ReadOnly="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.MITIGATION) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachPhongNgua" TextMode="MultiLine" Rows="3" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTINGENCY) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachUngPho" TextMode="MultiLine" Rows="3" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
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

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>

