<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="KhachHangDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCustomers.KhachHangDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAn" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-12">
            <div class="card p-3 min-vh-100">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Customer detail" />
                <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start gap-3 mb-4">
                    <div>
                        <div class="d-flex flex-wrap align-items-center gap-2">
                            <h4 class="mb-0 fw-semibold text-primary">
                                <asp:Label runat="server" ID="lblTenKhachHang"></asp:Label>
                            </h4>

                            <span class="badge rounded-pill bg-light text-secondary border px-3 py-2">
                               Đang hoạt động
                            </span>
                        </div>
                         <div class="small text-muted mt-1">
                             <asp:Label runat="server" ID="lblLoaiKhachHangSubLabel"></asp:Label>
                         </div>
                    </div>
                </div>

                <div class="row g-4 align-items-start">
                    <div class="col-xl-12">
                        <section class="mb-3">
                            <h5 class="text-uppercase fw-bold mb-3">
                                THÔNG TIN KHÁCH HÀNG
                            </h5>
                            <div class="row g-4">
                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        Loại khách hàng
                                    </div>

                                    <div class="fw-semibold">
                                        <asp:Label runat="server" ID="lblLoaiKhachHang"></asp:Label>
                                    </div>
                                </div>
                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.TAX_CODE) %>
                                    </div>

                                     <asp:Label runat="server" ID="lblSoThue"></asp:Label>
                                </div>
<div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>
                                    </div>

                                    <asp:Label runat="server" ID="lblSoDienThoai"></asp:Label>
                                </div>
                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        Email
                                    </div>

                                    <asp:Label runat="server" ID="lblEmail"></asp:Label>
                                </div>

                                <div class="col-md-6 col-xl-8">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.ADDRESS) %>
                                    </div>

                                    <asp:Label runat="server" ID="lblDiaChi"></asp:Label>
                                </div>

                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.CONTACT_PERSON) %>
                                    </div>

                                    <asp:Label runat="server" ID="lblNguoiLienHe"></asp:Label>
                                </div>
                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.CONTACT_PHONE_NUMBER) %>
                                    </div>

                                    <asp:Label runat="server" ID="lblDienThoaiLienHe"></asp:Label>
                                </div>

                                <div class="col-md-6 col-xl-4">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.CONTACT_EMAIL) %>
                                    </div>

                                    <asp:Label runat="server" ID="lblEmailLienHe"></asp:Label>
                                </div>
                                <div class="col-12">
                                    <div class="font-size-8 fw-bold mb-1">
                                        <%= GetResourceText(BackEndResourceKeys.SUMMARY) %>
                                    </div>

                                    <asp:Literal runat="server" ID="ltrMoTa"></asp:Literal>
                                </div>
                            </div>
                        </section>
                        <section class="shadow-none mb-3 rounded-3 fi">
                            <div class="card-body">
<h5 class="text-uppercase fw-bold mb-3">
                                    DỰ ÁN ĐÃ ĐẦU TƯ
                                </h5>
                                <SweetSoft:CtrlDuAn runat="server" ID="CtrlDuAn1" />
                            </div>
                        </section>
                    </div>
                </div>
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
