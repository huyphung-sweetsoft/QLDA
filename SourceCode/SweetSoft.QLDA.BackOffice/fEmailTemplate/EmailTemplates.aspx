<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="EmailTemplates.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmailTemplate.EmailTemplates" %>

<%----------------------PROGRAMER LOGS------------------------
--%>

<%@ Register Src="~/fEmailTemplate/Controls/CtrlTemplates.ascx" TagPrefix="SweetSoft" TagName="CtrlTemplates" %>


<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlTemplates runat="server" id="CtrlTemplates1" />
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
