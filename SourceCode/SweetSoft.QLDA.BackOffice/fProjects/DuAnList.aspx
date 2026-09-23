<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.EnumHelper.Defines" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlChonNhanVien.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVien" %>
<%@ Register Src="~/Controls/CtrlQuanLyLoai.ascx" TagPrefix="SweetSoft" TagName="CtrlQuanLyLoai" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAnForm.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAnForm" %>
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
            .file-box-single{
                width:100px;
            }
            .file-box .uploaded-content .item img{
                width: 60px;
            }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project list" />
                <SweetSoft:CtrlDuAn runat="server" id="CtrlDuAn1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlDuAnForm runat="server" ID="CtrlDuAnForm1"></SweetSoft:CtrlDuAnForm>
    <SweetSoft:CtrlQuanLyLoai runat="server" ID="CtrlQuanLyLoai1" OnDataChanged="CtrlQuanLyLoai1_OnDataChanged" />
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
