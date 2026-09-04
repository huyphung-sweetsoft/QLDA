<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlGiaiDoanDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlGiaiDoanDuAn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
    <div class="col-xl-12">
        <div class="card p-3 min-vh-100">
            <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project detail" />
            <%-- Tiêu đề và thao tác --%>
            <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start gap-3 mb-4">

                <div class="d-flex flex-wrap align-items-center gap-2">
                    <h4 class="mb-0 fw-semibold text-primary">
                        <asp:Label runat="server" ID="lblTenDuAn"></asp:Label>
                    </h4>

                    <span class="badge rounded-pill bg-light text-secondary border px-3 py-2">
                        Đang thực hiện
                    </span>
                </div>

                <div class="d-flex align-items-center gap-2">

                    <%-- Dropdown trạng thái --%>
                    <div class="dropdown">
                        <button
                            type="button"
                            class="btn btn-outline-secondary dropdown-toggle"
                            data-bs-toggle="dropdown"
                            aria-expanded="false">

                            <i class="fas fa-circle text-info me-2 small"></i>
                            Đang thực hiện
                        </button>

                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-circle text-warning me-2 small"></i>
                                    Chờ thực hiện
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item active" href="javascript:;">
                                    <i class="fas fa-circle text-info me-2 small"></i>
                                    Đang thực hiện
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-circle text-secondary me-2 small"></i>
                                    Tạm dừng
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-circle text-success me-2 small"></i>
                                    Hoàn thành
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-circle text-dark me-2 small"></i>
                                    Kết thúc
                                </a>
                            </li>
                        </ul>
                    </div>

                    <button
                        type="button"
                        class="btn btn-outline-secondary">
                        <i class="fas fa-pencil-alt me-1"></i>
                        Sửa
                    </button>

                    <div class="dropdown">
                        <button
                            type="button"
                            class="btn btn-outline-secondary"
                            data-bs-toggle="dropdown"
                            aria-expanded="false">
                            <i class="fas fa-ellipsis-h"></i>
                        </button>

                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-history me-2 text-muted"></i>
                                    Lịch sử hoạt động
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-project-diagram me-2 text-muted"></i>
                                    Quản lý giai đoạn
                                </a>
                            </li>

                            <li><hr class="dropdown-divider" /></li>

                            <li>
                                <a class="dropdown-item text-danger" href="javascript:;">
                                    <i class="fas fa-trash me-2"></i>
                                    Xóa dự án
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

            <div class="row g-4 align-items-start">

                <%-- Cột trái --%>
                <div class="col-xl-8">

                    <%-- Thông tin dự án --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3">
                            <%= GetResourceText(BackEndResourceKeys.PROJECT_INFORMATION) %>
                        </h5>

                        <div class="row g-4">
                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.CUSTOMER) %>
                                </div>
                                <asp:Label runat="server" ID="lblKhachHang"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %>
                                </div>
                                <asp:Label runat="server" ID="lblGiaTriHopDong"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %>
                                </div>
                                <asp:Label runat="server" ID="lblNgayKy"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %>
                                </div>
                                <asp:Label runat="server" ID="lblLoaiDuAn"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.START_DATE) %>
                                </div>
                                <asp:Label runat="server" ID="lblNgayBatDau"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    Ngày hoàn thành dự kiến
                                </div>
                                <asp:Label runat="server" ID="lblNgayHoanThanhDuKien"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    Hoàn thành thực tế
                                </div>
                                <asp:Label runat="server" ID="lblNgayHoanThanhThucTe"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                                </div>
                                <asp:Label runat="server" ID="lblTrangThai"></asp:Label>
                            </div>
                        </div>
                    </section>

                    <%-- Tiến độ --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3">
                            Tiến độ
                        </h5>

                        <div class="card border shadow-none rounded-3">
                            <div class="card-body py-2 px-3">

                                <div class="mb-4">
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="fs-6">
                                            Theo tỷ trọng ngày thực hiện
                                        </span>

                                        <strong class="small text-primary">
                                            62%
                                        </strong>
                                    </div>

                                    <div
                                        class="progress"
                                        role="progressbar"
                                        aria-valuenow="62"
                                        aria-valuemin="0"
                                        aria-valuemax="100">

                                        <div
                                            class="progress-bar bg-primary"
                                            style="width: 62%">
                                        </div>
                                    </div>
                                </div>

                                <div>
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="fs-6">
                                            Theo bình quân % hoàn thành công việc
                                        </span>

                                        <strong class="small text-primary">
                                            58%
                                        </strong>
                                    </div>

                                    <div
                                        class="progress"
                                        role="progressbar"
                                        aria-valuenow="58"
                                        aria-valuemin="0"
                                        aria-valuemax="100">

                                        <div
                                            class="progress-bar bg-primary"
                                            style="width: 58%">
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </section>

                    <%-- Mô tả --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3 ">
                            Mô tả
                        </h5>

                        <div class="card border shadow-none rounded-3">
                            <div class="card-body py-2 px-3">
                                <asp:Literal runat="server" ID="ltrMoTa"></asp:Literal>
                            </div>
                        </div>
                    </section>

                    <%-- Giai đoạn --%>
                    <section>
                        <SweetSoft:CtrlGiaiDoanDuAn runat="server" ID="CtrlGiaiDoanDuAn1" />
                    </section>
                </div>

                <%-- Cột phải --%>
                <div class="col-xl-4">

                    <%-- Phụ trách --%>
                    <section class="card border shadow-none mb-3 rounded-3">
                        <div class="card-body py-2 px-3">
                            <h5 class="text-uppercase fw-bold mb-3">
                                Phụ trách
                            </h5>

                            <div class="d-flex align-items-center gap-3">
                                <img runat="server" src="" ID="imgAvatarPM" class="rounded-circle" style="width:30px; height:30px; object-fit:cover;" alt="avatar"/>

                                <div>
                                    <div class="font-size-8 fw-bold mb-1">
                                        <asp:Label runat="server" ID="lblNhanVienQuanLy"></asp:Label>
                                    </div>

                                    <div class="small text-muted">
                                        Project Manager
                                        <i class="fas fa-external-link-alt ms-1"></i>
                                    </div>
                                </div>
                            </div>

                            <div class="mt-4">
                                <div class="font-size-8 fw-bold mb-1">
                                    Thành viên (5)
                                </div>

                                <div class="d-flex align-items-center flex-wrap gap-2">
                                    <span class="badge rounded-circle bg-light text-primary border p-3">
                                        NV
                                    </span>

                                    <span class="badge rounded-circle bg-light text-primary border p-3">
                                        LH
                                    </span>

                                    <span class="badge rounded-circle bg-light text-primary border p-3">
                                        HM
                                    </span>

                                    <span class="badge rounded-circle bg-light text-primary border p-3">
                                        +2
                                    </span>
                                </div>
                            </div>
                        </div>
                    </section>

                    <%-- Hợp đồng --%>
                    <section class="card border shadow-none mb-3 rounded-3">
                        <div class="card-body py-2 px-3">
                            <h6 class="text-uppercase fw-bold mb-3">
                                Hợp đồng thực hiện
                            </h6>

                            <a
                                href="javascript:;"
                                class="text-primary text-decoration-none">

                                <i class="fas fa-file-contract me-2"></i>
                                <asp:Label runat="server" ID="lblSoHopDong"></asp:Label>
                                <i class="fas fa-external-link-alt ms-1 small"></i>
                            </a>
                        </div>
                    </section>

                    <%-- Hoạt động gần đây --%>
                    <section class="card border shadow-none rounded-3">
                        <div class="card-body py-2 px-3">

                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <h5 class="text-uppercase fw-bold mb-3 mb-0">
                                    Hoạt động gần đây
                                </h5>

                                <a
                                    href="javascript:;"
                                    class="small text-primary text-decoration-none">
                                    Xem tất cả
                                </a>
                            </div>

                            <div class="list-group list-group-flush">

                                <div class="list-group-item px-0 py-3">
                                    <div class="d-flex gap-3">
                                        <span class="text-primary">
                                            <i class="far fa-circle"></i>
                                        </span>

                                        <div>
                                            <div class="small">
                                                Lê Việt Thắng đã cập nhật
                                                <strong>Tiến độ</strong>
                                            </div>

                                            <div class="small text-muted mt-1">
                                                2 giờ trước
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="list-group-item px-0 py-3">
                                    <div class="d-flex gap-3">
                                        <span class="text-primary">
                                            <i class="far fa-circle"></i>
                                        </span>

                                        <div>
                                            <div class="small">
                                                Nguyễn Văn Hùng đã thêm
                                                <strong>Thành viên</strong>
                                            </div>

                                            <div class="small text-muted mt-1">
                                                3 giờ trước
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="list-group-item px-0 py-3">
                                    <div class="d-flex gap-3">
                                        <span class="text-primary">
                                            <i class="far fa-circle"></i>
                                        </span>

                                        <div>
                                            <div class="small">
                                                Admin đã liên kết
                                                <strong>Hợp đồng</strong>
                                            </div>

                                            <div class="small text-muted mt-1">
                                                5 giờ trước
                                            </div>
                                        </div>
                                    </div>
                                </div>

                            </div>
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
