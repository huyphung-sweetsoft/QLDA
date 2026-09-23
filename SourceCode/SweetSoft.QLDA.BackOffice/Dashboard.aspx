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

    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlProjectTabs
                    runat="server"
                    ID="CtrlProjectTabs1" />

                <div id="dashboardContent" runat="server">
                </div>
            </div>
        </div>
    </div>

</asp:Content>
