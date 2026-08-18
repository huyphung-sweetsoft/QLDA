<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="FileManagers.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fSettings.FileManagers" %>
<%--------------------PROGRAMER LOGS------------------------%>
<%--Created by:
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style type="text/css">
        .fixheight {
            height: calc(100vh - 220px) !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="File Manager" />
    <div class="row">
        <div class="col-sm-12">
            <div class="card fixheight">
                <iframe id="framefile" src="/_RFMng/Default.aspx?fm=1" frameborder="0" style="overflow: hidden; height: 100%; width: 100%"
                    height="100%" width="100%">Your browser does not support inline frames.
                </iframe>
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
