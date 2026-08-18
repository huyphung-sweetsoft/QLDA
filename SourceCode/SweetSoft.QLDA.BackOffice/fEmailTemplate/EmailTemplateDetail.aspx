<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="EmailTemplateDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmailTemplate.EmailTemplateDetail" %>

<%----------------------PROGRAMER LOGS------------------------
--%>

<%@ Register Src="~/fEmailTemplate/Controls/CtrlEmailTemplateDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlEmailTemplateDetail" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="card min-h-sreen">
        <SweetSoft:Navigation runat="server" ID="Navigation1" />
        <SweetSoft:CtrlEmailTemplateDetail runat="server" id="CtrlEmailTemplateDetail1" />
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
