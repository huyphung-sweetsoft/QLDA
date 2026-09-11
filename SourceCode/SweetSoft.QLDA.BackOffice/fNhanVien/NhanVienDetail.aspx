<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="NhanVienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.NhanVienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fUsers/Controls/CtrlUserDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlUserDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .project-card {
            transition: all 0.2s ease-in-out;
            cursor: pointer;
        }
        .project-card:hover {
            border-color: #3a6ea5 !important;
            transform: translateY(-2px);
            box-shadow: 0 .5rem 1rem rgba(0,0,0,.15)!important;
        }
        .icon-box {
            width: 48px; height: 48px;
            display: flex; align-items: center; justify-content: center;
            border-radius: 12px; font-size: 20px; flex-shrink: 0;
        }
        .spec-tile {
            background-color: #f8f9fa; 
            border: 1px solid #dee2e6;
            border-radius: 0.5rem;
            padding: 1rem;
            height: 100%;
            min-width: 0;
            overflow-wrap: anywhere;
            word-break: break-word;
        }
        .col-xl-8.d-flex.flex-column,
        .col-xl-8.d-flex.flex-column .card,
        .col-xl-8.d-flex.flex-column .card-body {
            min-width: 0;
        }
        .spec-label {
            font-size: 11px;
            font-weight: 700;
            color: #6c757d; 
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 0.25rem;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upnlMainDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container-fluid px-0">
        
                <SweetSoft:Navigation ID="Navigation1" runat="server"/>

                <div class="row align-items-stretch">
            
                    <div class="col-xl-8 col-lg-7 d-flex flex-column gap-3">

                        <div class="card shadow-sm border-0">
                            <div class="card-body d-flex flex-column flex-md-row align-items-center gap-4">
                                <div class="flex-shrink-0">
                                    <img id="imgAvatar" runat="server" src="/Styles/images/user-icon.png" 
                                        class="rounded-circle border border-3 border-primary shadow-sm" 
                                        style="width: 100px; height: 100px; object-fit: cover;" 
                                        onerror="this.src='/Styles/images/user-icon.png'" />
                                </div>
                        
                                <div class="flex-grow-1 w-100 text-center text-md-start">
                                    <div class="d-flex flex-column flex-md-row align-items-center gap-2 mb-2">
                                        <h4 class="mb-0 fw-bold text-dark">
                                            <asp:Literal ID="ltrTenNhanVien" runat="server"></asp:Literal>
                                        </h4>
                                        <span class="badge bg-success rounded-pill px-3 py-1"><%= GetResourceText(BackEndResourceKeys.ACTIVE) %></span>
                                    </div>
                            
                                    <div class="row g-2 text-muted" style="font-size: 14px;">
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %>: <strong class="text-dark"><asp:Literal ID="ltrChucDanh" runat="server"></asp:Literal></strong>
                                        </div>
                                        <div class="col-sm-6">
                                             <%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %>: <strong class="text-dark"><asp:Literal ID="ltrPhongBan" runat="server"></asp:Literal></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            Email: <strong class="text-dark"><asp:Literal ID="ltrEmail" runat="server"></asp:Literal></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>: <strong class="text-dark"><asp:Literal ID="ltrPhone" runat="server"></asp:Literal></strong>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="card shadow-sm border-0 flex-grow-1">
                            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3 px-4 border-bottom">
                                <h6 class="mb-0 fw-bold text-dark">
                                    <i class="fas fa-id-card text-primary me-2"></i> <%= GetResourceText(BackEndResourceKeys.PERSONAL_AND_WORK_INFORMATION) %>
                                </h6>
                                <SweetSoft:ExtraButton runat="server" ID="btnEditProfile" 
                                    CssClass="btn-sm btn-light border text-primary fw-bold" 
                                    ButtonIcon="Edit" 
                                    OnClick="btnEditProfile_Click">
                                </SweetSoft:ExtraButton>
                            </div>
                            <div class="card-body d-flex flex-column gap-4">

                                <div class="d-flex flex-wrap border rounded bg-light">
                                    <div class="flex-fill p-3 border-end">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.JOIN_DATE) %></span>
                                        <span class="fw-bold text-dark fs-6"><asp:Literal ID="ltrNgayGiaNhap" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="flex-fill p-3 border-end">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.SENIORITY) %></span>
                                        <span class="fw-bold text-primary fs-6"><asp:Literal ID="ltrThamNien" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="flex-fill p-3">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.WORK_STATUS) %></span>
                                        <span class="fw-bold text-success fs-6"><%= GetResourceText(BackEndResourceKeys.WORKING) %></span>
                                    </div> 
                                </div>

                                <div class="row g-3">
                                    <div class="col-md-4">
                                        <div class="spec-tile bg-white">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrCCCD" runat="server"></asp:Literal></span>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile bg-white">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrNgaySinh" runat="server"></asp:Literal></span>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile bg-white">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrGioiTinh" runat="server"></asp:Literal></span>
                                        </div>
                                    </div>
                                    <div class="col-12" style="min-width: 0;">
                                        <div class="spec-tile bg-white" style="height: auto;">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></span>
                                            <span class="fw-bold text-dark d-block"
                                                  style="overflow-wrap: anywhere; word-break: break-word; white-space: normal;">
                                                <asp:Literal ID="ltrDiaChi" runat="server"></asp:Literal>
                                            </span>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>

                    </div>

                    <div class="col-xl-4 col-lg-5 d-flex flex-column gap-3 mt-3 mt-lg-0">
                        <div class="card shadow-sm border border-light project-card flex-grow-1">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PARTICIPATING) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;">
                                            <asp:Literal ID="ltrCountActiveProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.ACTIVE_PROJECTS) %>
                                        </span>
                                    </div>
                                    <div class="icon-box bg-primary bg-opacity-10 text-primary"><i class="fas fa-bolt"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end">
                                    <span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %> <i class="fas fa-arrow-right ms-1"></i></span>
                                </div>
                            </div>
                        </div>

                        <div class="card shadow-sm border border-light project-card flex-grow-1">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.COMPLETED) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;">
                                            <asp:Literal ID="ltrCountDoneProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.ACCEPTED_PROJECTS) %>
                                        </span>
                                    </div>
                                    <div class="icon-box bg-success bg-opacity-10 text-success"><i class="fas fa-check-circle"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end">
                                    <span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %> <i class="fas fa-arrow-right ms-1"></i></span>
                                </div>
                            </div>
                        </div>

                       <a id="lnkSchedule" runat="server" class="card shadow-sm border border-light project-card flex-grow-1 text-decoration-none">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;"><%= GetResourceText(BackEndResourceKeys.VIEW_WORK_SCHEDULE) %></span>
                                    </div>
                                    <div class="icon-box bg-info bg-opacity-10 text-info" style="color: #681da8 !important; background-color: #f3e8fd !important;"><i class="fas fa-calendar-alt"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end">
                                    <span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.OPEN_SCHEDULE) %> <i class="fas fa-arrow-right ms-1"></i></span>
                                </div>
                            </div>
                        </a>
                    </div>

                </div>
            </div>
            </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlUserDetail runat="server" ID="CtrlUserDetail1" />
</asp:Content>