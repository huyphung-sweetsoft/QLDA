<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="KhachHangList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCustomers.KhachHangList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.EnumHelper.Defines" %>
<%@ Register Src="~/fCustomers/Controls/CtrlKhachHang.ascx" TagPrefix="SweetSoft" TagName="CtrlKhachHang" %>
<%@ Register Src="~/fCustomers/Controls/CtrlKhachHangForm.ascx" TagPrefix="SweetSoft" TagName="CtrlKhachHangForm" %>

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

        .file-box-single {
            width: 100px;
        }

        .file-box .uploaded-content .item img {
            width: 60px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Customer list" />
                <SweetSoft:CtrlKhachHang runat="server" id="CtrlKhachHang" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlKhachHangForm runat="server" ID="CtrlKhachHangForm1" />     
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>