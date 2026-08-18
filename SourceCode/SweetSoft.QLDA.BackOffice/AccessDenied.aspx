<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.AccessDenied" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-lg-12">
            <div class="card min-h-sreen p-2">
                <div class="text-center mb-5">
                    <h1 class="display-1 fw-semibold">4<span class="text-primary mx-2">0</span>3</h1>
                    <h4 class="text-uppercase"><%= GetResourceText(BackEndResourceKeys.NO_ACCESS_PERMISSIONS) %></h4>
                    <div class="mt-5 text-center">
                        <a class="btn btn-primary waves-effect waves-light" href="/Home"><%= GetResourceText(BackEndResourceKeys.RETURN_TO_THE_DASHBOARD) %></a>
                    </div>
                </div>
                <div>
                    <img src="/Styles/images/error-img.png" alt="" class="img-fluid">
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
