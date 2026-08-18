<%@ Page Async="true" Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        [data-layout-mode="dark"] table tbody tr, [data-layout-mode="dark"] table thead tr, [data-layout-mode="dark"] table tbody tr td a:not([type="submit"],[data-bs-toggle="dropdown"]) {
            color: #fff;
        }
    </style>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-12">
            <div class="card min-h-sreen">
                <asp:Literal runat="server" ID="ltrContent" Visible="false"></asp:Literal>
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
