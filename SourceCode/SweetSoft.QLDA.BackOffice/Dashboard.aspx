<%@ Page
    Title="Thống kê"
    Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Dashboard"
%>
<%@ Register
    Src="~/fProjects/Controls/CtrlProjectTabs.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="dashboard-page-shell">
        <SweetSoft:Navigation
            runat="server"
            ID="Navigation1" />

        <SweetSoft:CtrlProjectTabs
            runat="server"
            ID="CtrlProjectTabs1" />

        <div id="dashboardContent" runat="server">
        </div>
    </div>

</asp:Content>
