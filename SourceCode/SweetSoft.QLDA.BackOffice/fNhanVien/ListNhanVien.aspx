<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="ListNhanVien.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.ListNhanVien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fNhanVien/Controls/CtrlNhanViens.ascx" TagPrefix="SweetSoft" TagName="CtrlNhanViens" %>
<%@ Register Src="~/fUsers/Controls/CtrlUserDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlUserDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="NhanVien List" />
                <SweetSoft:CtrlNhanViens runat="server" ID="CtrlNhanViens1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <!-- Toàn bộ popup đã được thu bé lại bằng 1 dòng này -->
    <SweetSoft:CtrlUserDetail runat="server" ID="CtrlUserDetail1" />
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            // Giữ lại hàm dọn dẹp lỗi Validation của hệ thống
            if (typeof CMSMasterJs !== 'undefined') {
                CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
            }
        });
    </script>
</asp:Content>