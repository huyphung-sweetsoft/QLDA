<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="NhanVienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.NhanVienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fNhanVien/Controls/CtrlNhanVienPopup.ascx" TagPrefix="SweetSoft" TagName="CtrlNhanVienPopup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* ============================================================
           GENERAL
           ============================================================ */
        .icon-box { width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; border-radius: 12px; font-size: 20px; flex-shrink: 0; }
        .spec-tile { background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 0.5rem; padding: 1rem; height: 100%; min-width: 0; overflow-wrap: anywhere; word-break: break-word; }
        .col-xl-8.d-flex.flex-column, .col-xl-8.d-flex.flex-column .card, .col-xl-8.d-flex.flex-column .card-body { min-width: 0; }
        .spec-label { font-size: 11px; font-weight: 700; color: #6c757d; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 0.25rem; }

        /* ============================================================
           PROJECT LIST SIDE PANEL
           ============================================================ */
        .side-panel-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); z-index: 1040; display: none; backdrop-filter: blur(2px); }
        
        /* [ĐÃ SỬA]: TĂNG WIDTH TỪ 850px LÊN 1100px ĐỂ BẢNG TASK RỘNG RÃI HƠN */
        .side-panel { position: fixed; top: 0; right: -100%; width: 1100px; max-width: 100vw; height: 100vh; background: #f4f5f7; box-shadow: -5px 0 25px rgba(0, 0, 0, 0.15); z-index: 1050; transition: right 0.3s cubic-bezier(0.82, 0.085, 0.395, 0.895); display: flex; flex-direction: column; }
        
        .side-panel.open { right: 0; }
        .side-panel-header { padding: 16px 24px; background: #ffffff; border-bottom: 1px solid #dee2e6; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02); z-index: 10; flex-shrink: 0; }
        .side-panel-title { font-size: 16px; font-weight: 700; color: #232220; margin: 0; text-transform: uppercase; }
        .btn-close-panel { background: none; border: none; font-size: 20px; color: #6c757d; cursor: pointer; transition: color 0.2s; }
        .btn-close-panel:hover { color: #dc3545; }
        .side-panel-body { padding: 24px; overflow-y: auto; flex-grow: 1; scroll-behavior: smooth; }
        .side-panel-body::-webkit-scrollbar { width: 8px; }
        .side-panel-body::-webkit-scrollbar-track { background: #f8f9fa; }
        .side-panel-body::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
        .side-panel-body::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

        /* ============================================================
           PROJECT LIST
           ============================================================ */
        .section-card { margin-bottom: 32px; }
        .project-box { border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 16px; overflow: hidden; background: #ffffff; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.05); transition: all 0.2s ease; }
        .project-box:hover { border-color: #3a6ea5; box-shadow: 0 0.4rem 1rem rgba(0, 0, 0, 0.08); transform: translateY(-1px); }
        .project-box:last-child { margin-bottom: 0; }
        .project-summary-head { background: #ffffff; padding: 16px 18px; }
        .project-title { color: #232220; font-size: 15px; font-weight: 700; line-height: 1.4; }
        .project-code { color: #6c757d; font-size: 12px; font-weight: 500; }
        .project-meta { font-size: 12px; color: #6c757d; margin-top: 5px; }
        .project-meta strong { color: #343a40; }
        .project-status-tag { display: inline-flex; align-items: center; justify-content: center; padding: 5px 11px; border-radius: 12px; font-size: 11px; font-weight: 700; background: #f3f6f9; color: #5e6278; border: 1px solid #e1e6eb; }
        .project-contribution { font-size: 12px; font-weight: 700; color: #3a6ea5; background: #e9f2fb; padding: 5px 10px; border-radius: 12px; display: inline-flex; align-items: center; justify-content: center; }
        .btn-project-detail { border-radius: 6px; font-size: 12px; font-weight: 700; }

        /* ============================================================
           PROJECT DETAIL SIDE PANEL
           ============================================================ */
        .project-detail-overlay { z-index: 1060; }
        .project-detail-panel { z-index: 1070; background: #f4f5f7; }
        .detail-project-header { background: #ffffff; border: 1px solid #e0e4e8; border-radius: 10px; padding: 18px; margin-bottom: 18px; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.03); }
        .detail-project-title { font-size: 18px; font-weight: 700; color: #232220; }
        .detail-project-code { color: #6c757d; font-size: 12px; }
        .detail-info-item { background: #f8f9fa; border: 1px solid #e5e7eb; border-radius: 7px; padding: 10px 12px; height: 100%; display: flex; flex-direction: column; justify-content: center; }
        .detail-info-label { display: block; font-size: 10px; font-weight: 700; color: #7e8299; text-transform: uppercase; margin-bottom: 4px; }
        .detail-info-value { display: block; font-size: 13px; font-weight: 700; color: #343a40; overflow-wrap: anywhere; }

        /* ============================================================
           PHASE / TASK
           ============================================================ */
        .phase-item { border: 1px solid #e4e6ef; border-radius: 8px; padding: 16px; margin-bottom: 16px; background: #ffffff; box-shadow: 0 0 10px 0 rgba(82, 63, 105, 0.02); }
        .phase-item:last-child { margin-bottom: 0; }
        .phase-header-line { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 16px; background: #f4f6f9; border-left: 4px solid #6f42c1; padding: 12px 16px; border-radius: 6px; }
        .phase-title-name { font-weight: 700; font-size: 13px; color: #232220; min-width: 0; }
        .phase-title-name span { font-weight: 400; }
        .task-sub-table { width: 100%; border-collapse: collapse; font-size: 13px; background: #ffffff; border-radius: 6px; overflow: hidden; border: 1px solid #e4e6ef; }
        .task-sub-table th { background: #f8f9fa; padding: 10px 12px; text-align: left; color: #6c757d; font-weight: 600; border-bottom: 2px solid #e4e6ef; }
        .task-sub-table td { padding: 10px 12px; border-bottom: 1px solid #e4e6ef; vertical-align: middle; }
        .task-sub-table tr:last-child td { border-bottom: none; }
        .task-row:hover { background-color: #f8fafd; }
        .task-row td:first-child { border-left: 3px solid transparent; transition: all 0.2s ease; }
        .task-row[data-status="0"] td:first-child { border-left-color: #b5b5c3; }
        .task-row[data-status="1"] td:first-child { border-left-color: #3699ff; }
        .task-row[data-status="2"] td:first-child { border-left-color: #1bc5bd; }
        .task-row[data-status="3"] td:first-child { border-left-color: #ffa800; }
        .ui-badge { padding: 4px 10px; border-radius: 12px; font-size: 11px; font-weight: 700; display: inline-flex; align-items: center; white-space: nowrap; }
        .ui-badge[data-prio="Cao"] { background: #ffe2e5; color: #f64e60; }
        .ui-badge[data-prio="Trung bình"] { background: #fff4de; color: #ffa800; }
        .ui-badge[data-prio="Thấp"] { background: #e1f0ff; color: #3699ff; }
        .ui-status-wrapper { padding: 4px 10px; border-radius: 12px; font-size: 11px; font-weight: 700; display: inline-flex; align-items: center; white-space: nowrap; }
        .ui-status-wrapper[data-status="0"] { background: #f3f6f9; color: #7e8299; }
        .ui-status-wrapper[data-status="1"] { background: #e1f0ff; color: #3699ff; }
        .ui-status-wrapper[data-status="2"] { background: #c9f7f5; color: #1bc5bd; }
        .ui-status-wrapper[data-status="3"] { background: #fff4de; color: #ffa800; }
        .ui-status-wrapper span { background: transparent !important; padding: 0 !important; color: inherit !important; font-size: inherit !important; font-weight: inherit !important; border: none !important; }
        .ui-status-wrapper.task-status-late { background: #ffe2e5; color: #dc3545; border: 1px solid #ffc5cc; }
        .empty-detail { background: #ffffff; border: 1px dashed #d7dce1; border-radius: 8px; padding: 30px 20px; text-align: center; color: #7e8299; }

        /* ============================================================
           STATIC MINI CALENDAR (LỊCH THÁNG TĨNH)
           ============================================================ */
        .mc-weekdays, .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100%; }
        .mc-weekdays span { text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
        .mc-weekdays span.mc-we { color: #94a3b8; }
        .mc-grid { grid-auto-rows: 50px; gap: 2px; }
        
        .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
        .mc-day .mc-num { font-weight: 600; line-height: 1; }
        .mc-day.out-month .mc-num { color: #94a3b8; opacity: 0.85; } /* Tăng từ 0.4 lên 0.85 để dễ nhìn hơn */
        .mc-day.out-range { opacity: 0.7; } /* Tăng từ 0.32 lên 0.7 */

        .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; }
        .mc-day.st-holiday { background: #fef3c7; color: #b45309; }
        .mc-day.st-weekend { background: #f1f5f9; color: #64748b; }
        .mc-day.st-free    { background: #e6f4ea; color: #137333; }
        .mc-day.today { border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }

        .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        /* ============================================================
           MOBILE
           ============================================================ */
        @media (max-width: 767.98px) {
            .side-panel { width: 100vw; }
            .side-panel-body { padding: 14px; }
            .phase-header-line { flex-direction: column; align-items: flex-start; }
            .task-sub-table { font-size: 12px; display: block; overflow-x: auto; }
            .project-summary-head { padding: 14px; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upnlMainDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container-fluid px-0">
                <SweetSoft:Navigation ID="Navigation1" runat="server" />

                <!-- ====================================================
                     MAIN LAYOUT
                     ==================================================== -->
                <div class="row align-items-stretch">

                    <!-- =================================================
                         LEFT COLUMN
                         ================================================= -->
                    <div class="col-xl-8 col-lg-7 d-flex flex-column gap-3">

                        <!-- ================================
                             EMPLOYEE SUMMARY
                             ================================ -->
                        <div class="card shadow-sm border-0">
                            <div class="card-body d-flex flex-column flex-md-row align-items-center gap-4">
                                <div class="flex-shrink-0">
                                    <img id="imgAvatar" runat="server" src="/Styles/images/user-icon.png" class="rounded-circle border border-3 border-primary shadow-sm" style="width:100px;height:100px;object-fit:cover;" onerror="this.src='/Styles/images/user-icon.png'" />
                                </div>
                                <div class="flex-grow-1 w-100 text-center text-md-start">
                                    <div class="d-flex flex-column flex-md-row align-items-center gap-2 mb-2">
                                        <h4 class="mb-0 fw-bold text-dark">
                                            <asp:Literal ID="ltrTenNhanVien" runat="server" />
                                        </h4>
                                        <span class="badge bg-success rounded-pill px-3 py-1"><%= GetResourceText(BackEndResourceKeys.ACTIVE) %></span>
                                    </div>
                                    <div class="row g-2 text-muted" style="font-size:14px;">
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %>: <strong class="text-dark"><asp:Literal ID="ltrChucDanh" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %>: <strong class="text-dark"><asp:Literal ID="ltrPhongBan" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            Email: <strong class="text-dark"><asp:Literal ID="ltrEmail" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>: <strong class="text-dark"><asp:Literal ID="ltrPhone" runat="server" /></strong>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- ================================
                             PERSONAL / WORK INFO
                             ================================ -->
                        <div class="card shadow-sm border-0 flex-grow-1">
                            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3 px-4 border-bottom">
                                <h6 class="mb-0 fw-bold text-dark">
                                    <i class="fas fa-id-card text-primary me-2"></i><%= GetResourceText(BackEndResourceKeys.PERSONAL_AND_WORK_INFORMATION) %>
                                </h6>
                                <SweetSoft:ExtraButton runat="server" ID="btnEditProfile" CssClass="btn-sm btn-light border text-primary fw-bold" ButtonIcon="Edit" OnClick="btnEditProfile_Click"></SweetSoft:ExtraButton>
                            </div>
                            <div class="card-body d-flex flex-column gap-4">
                                <div class="d-flex flex-wrap border rounded bg-light">
                                    <div class="flex-fill p-3 border-end">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.JOIN_DATE) %></span>
                                        <span class="fw-bold text-dark fs-6"><asp:Literal ID="ltrNgayGiaNhap" runat="server" /></span>
                                    </div>
                                    <div class="flex-fill p-3 border-end">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.SENIORITY) %></span>
                                        <span class="fw-bold text-primary fs-6"><asp:Literal ID="ltrThamNien" runat="server" /></span>
                                    </div>
                                    <div class="flex-fill p-3">
                                        <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.WORK_STATUS) %></span>
                                        <asp:Literal ID="ltrTinhTrangCongTac" runat="server" />
                                    </div>
                                </div>

                                <div class="row g-3">
                                    <div class="col-md-4">
                                        <div class="spec-tile">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrCCCD" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrNgaySinh" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></span>
                                            <span class="fw-bold text-dark"><asp:Literal ID="ltrGioiTinh" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="col-12" style="min-width:0;">
                                        <div class="spec-tile" style="height:auto;">
                                            <span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></span>
                                            <span class="fw-bold text-dark d-block" style="overflow-wrap:anywhere;word-break:break-word;white-space:normal;"><asp:Literal ID="ltrDiaChi" runat="server" /></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

<!-- =================================================
                         RIGHT COLUMN (CHIA 2 KHỐI RÕ RÀNG)
                         ================================================= -->
                    <div class="col-xl-4 col-lg-5 d-flex flex-column mt-3 mt-lg-0 gap-3">
                        
                        <!-- Box 1: Tổng quan dự án (Clickable) -->
                        <div class="card shadow-sm border bg-white mb-0" 
                             style="cursor:pointer; border-color:#e0e4e8 !important;" 
                             onclick="openSidePanel();"
                             onmouseover="this.style.borderColor='#3a6ea5';"
                             onmouseout="this.style.borderColor='#e0e4e8';">
                            <div class="card-body p-4 d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center gap-3">
                                    <div class="icon-box shadow-sm text-primary" style="background-color:#e9f2fb;">
                                        <i class="fas fa-layer-group"></i>
                                    </div>
                                    <div>
                                        <h6 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PROJECT_ALLOCATION_DETAILS) %></h6>
                                        <span class="text-muted" style="font-size:12px;">
                                            <asp:Literal ID="ltrCountAllProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.PROJECT).ToLower() %>
                                        </span>
                                    </div>
                                </div>
                                <i class="fas fa-chevron-right text-muted"></i>
                            </div>
                        </div>

                        <!-- Box 2: Lịch tĩnh tháng hiện tại -->
                        <div class="card shadow-sm border flex-grow-1 bg-white" style="border-color:#e0e4e8 !important;">
                            <!-- Header: Đã dời nút Mở Lịch Biểu lên ngang hàng với Tiêu đề -->
                            <div class="card-header bg-white border-0 pt-4 pb-0 px-4 d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center gap-3">
                                    <div class="icon-box shadow-sm" style="color:#681da8; background-color:#f3e8fd;">
                                        <i class="fas fa-calendar-alt"></i>
                                    </div>
                                    <div>
                                        <!-- GỌI ĐÚNG BACKEND RESOURCE KEY CHUẨN -->
                                        <h6 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.WORK_SCHEDULE_THIS_MONTH) %></h6>
                                        <span class="text-muted" style="font-size:12px;">
                                            <%= GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE) %>
                                        </span>
                                    </div>
                                </div>
                                
                                <!-- Nút liên kết ra trang Lịch biểu đầy đủ -->
                                <a id="lnkSchedule" runat="server" class="text-primary fw-bold text-decoration-none" style="font-size:13px; transition:all .2s;"
                                   onmouseover="this.style.color='#681da8';" onmouseout="this.style.color='#3a6ea5';">
                                    <%= GetResourceText(BackEndResourceKeys.OPEN_SCHEDULE) %> <i class="fas fa-arrow-right ms-1"></i>
                                </a>
                            </div>
                            
                            <!-- Body chứa Grid Lịch -->
                            <div class="card-body p-4 pt-3 pb-4">
                                <asp:HiddenField ID="hdfScheduleJson" runat="server" ClientIDMode="Static" />
                                <div class="mini-cal-wrap">
                                    <div class="mini-cal" id="employeeMonthCalendar"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- =====================================================
                     PROJECT LIST SIDE PANEL
                     ===================================================== -->
                <div id="sidePanelOverlay" class="side-panel-overlay" onclick="closeSidePanel();"></div>

                <div id="projectSidePanel" class="side-panel">
                    <div class="side-panel-header">
                        <h5 class="side-panel-title">
                            <i class="fas fa-layer-group text-primary me-2"></i><%= GetResourceText(BackEndResourceKeys.PROJECT_ALLOCATION_DETAILS) %>
                        </h5>
                        <button type="button" class="btn-close-panel" onclick="closeSidePanel();">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <div class="side-panel-body">
                        <!-- ================================================
                             SEARCH / FILTER
                             ================================================ -->
                        <asp:UpdatePanel runat="server" ID="upnlSearchDuAn" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThaiDuAn" runat="server" Text="Trạng thái dự án" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai" EnableSearch="false" CausesValidation="false" OnSelectedValueChanged="ddlSearchTrangThaiDuAn_SelectedValueChanged" CssClass="border-radius-1" Style="min-width:180px;"></SweetSoft:BootstrapDropdown>
                                    <asp:Panel runat="server" DefaultButton="btnSearchDuAn" CssClass="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchDuAn" CssClass="border-primary input-search-filter" PlaceHolder="Nhập tên dự án..." CausesValidation="false"></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="btnSearchDuAn" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchDuAn_Click" CausesValidation="false" IsSubmit="false"></SweetSoft:ExtraButton>
                                    </asp:Panel>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>

                        <!-- ================================================
                             SEARCH TAGS
                             ================================================ -->
                        <div class="listSearchTagBox">
                            <asp:UpdatePanel ID="upSearchTagDuAn" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <SweetSoft:ExtraSearchBox ID="searchTagBoxDuAn" runat="server" OnTagClosed="searchTagBoxDuAn_TagClosed"></SweetSoft:ExtraSearchBox>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>

                        <!-- ================================================
                             PROJECT LIST
                             ================================================ -->
                        <asp:UpdatePanel ID="upListDuAn" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div id="section_all_projects" class="section-card" runat="server" ClientIDMode="Static">
                                    <div id="emptyAll" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                                        <i class="far fa-folder-open mb-2" style="font-size:24px;color:#ced4da;"></i><br />
                                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                                    </div>

                                    <asp:Repeater ID="rptAllProjects" runat="server" OnItemCommand="rptAllProjects_ItemCommand">
                                        <ItemTemplate>
                                            <div class="project-box">
                                                <!-- PROJECT SUMMARY -->
                                                <div class="project-summary-head">
                                                    <div class="d-flex justify-content-between align-items-start gap-3">
                                                        <div class="flex-grow-1 min-w-0">
                                                            <div class="project-title"><%# Eval("TenDuAn") %></div>
                                                            <div class="project-code"><%# Eval("MaDuAn") %></div>
                                                            <div class="project-meta">
                                                                <%# GetResourceText(BackEndResourceKeys.ROLE) %>: <strong><%# Eval("VaiTro") %></strong> <span class="mx-2">|</span>
                                                                <%# GetResourceText(BackEndResourceKeys.PROJECT_TIME) %>: <strong><%# FormatDateRange(Eval("ProjectStartDate"), Eval("ProjectEndDate")) %></strong>
                                                            </div>
                                                        </div>
                                                        <div class="text-end flex-shrink-0">
                                                            <div class="project-contribution">
                                                                <%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Convert.ToDouble(Eval("ContributionPercent")).ToString("0.0") %>%
                                                            </div>
                                                            <div class="mt-2">
                                                                <span class="project-status-tag"><%# GetDuAnStatusText(Eval("TrangThai")) %></span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="text-end mt-3">
                                                        <asp:LinkButton ID="btnViewProject" runat="server" CommandName="ViewProject" CommandArgument='<%# Eval("IdDuAn") %>' CssClass="btn btn-sm btn-outline-primary btn-project-detail">
                                                            <i class="fas fa-eye me-1"></i>Xem chi tiết
                                                        </asp:LinkButton>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>

                <!-- =====================================================
                     PROJECT DETAIL PANEL
                     ===================================================== -->
                <div id="projectDetailOverlay" class="side-panel-overlay project-detail-overlay" onclick="closeProjectDetailPanel();"></div>

                <div id="projectDetailPanel" class="side-panel project-detail-panel">
                    <div class="side-panel-header">
                        <h5 class="side-panel-title">
                            <i class="fas fa-project-diagram text-primary me-2"></i>Chi tiết dự án
                        </h5>
                        <button type="button" class="btn-close-panel" onclick="closeProjectDetailPanel();">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <div class="side-panel-body">
                        <asp:UpdatePanel ID="upProjectDetail" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <!-- =========================================
                                     PROJECT INFO (ĐÃ SỬA CẤU TRÚC 4 Ô)
                                     ========================================= -->
                                <div class="detail-project-header">
                                    <div class="mb-3">
                                        <div class="detail-project-title"><asp:Literal ID="ltrDetailTenDuAn" runat="server" /></div>
                                        <div class="detail-project-code mt-1"><asp:Literal ID="ltrDetailMaDuAn" runat="server" /></div>
                                    </div>
                                    <div class="row g-2">
                                        <div class="col-md-6">
                                            <div class="detail-info-item">
                                                <span class="detail-info-label"><%= GetResourceText(BackEndResourceKeys.ROLE) %></span>
                                                <span class="detail-info-value"><asp:Literal ID="ltrDetailVaiTro" runat="server" /></span>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="detail-info-item">
                                                <span class="detail-info-label"><%= GetResourceText(BackEndResourceKeys.PROJECT_TIME) %></span>
                                                <span class="detail-info-value"><asp:Literal ID="ltrDetailProjectTime" runat="server" /></span>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="detail-info-item">
                                                <span class="detail-info-label"><%= GetResourceText(BackEndResourceKeys.CONTRIBUTION) %></span>
                                                <span class="detail-info-value text-primary"><asp:Literal ID="ltrDetailContribution" runat="server" />%</span>
                                            </div>
                                        </div>
                                        <!-- ĐÃ DỜI TRẠNG THÁI XUỐNG ĐÂY ĐỂ LẤP CHỖ TRỐNG -->
                                        <div class="col-md-6">
                                            <div class="detail-info-item">
                                                <span class="detail-info-label"><%= GetResourceText(BackEndResourceKeys.STATUS) ?? "Trạng thái" %></span>
                                                <span class="detail-info-value">
                                                    <span class="project-status-tag" style="margin: 0; padding: 3px 10px; font-size: 12px;">
                                                        <asp:Literal ID="ltrDetailTrangThai" runat="server" />
                                                    </span>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- =========================================
                                     DETAIL PHASES
                                     ========================================= -->
                                <asp:Repeater ID="rptDetailPhases" runat="server">
                                    <ItemTemplate>
                                        <div class="phase-item">
                                            <div class="phase-header-line">
                                                <div class="phase-title-name">
                                                    <i class="far fa-folder-open text-warning me-1"></i>
                                                    <%# GetResourceText(BackEndResourceKeys.PHASE) %>: <%# Eval("MaPhase") %> <%# Eval("TenPhase") %>
                                                    <span class="text-muted fw-normal ms-1">
                                                        (<%# FormatDateRange(Eval("MinStartDate"), Eval("MaxEndDate")) %> | <%# GetResourceText(BackEndResourceKeys.DEADLINE) %>: <%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %>)
                                                    </span>
                                                </div>
                                                <div class="badge bg-light text-dark border">
                                                    <%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Convert.ToDouble(Eval("ContributionPercent")).ToString("0.0") %>%
                                                </div>
                                            </div>

                                            <table class="task-sub-table">
                                                <!-- ĐÃ TỐI ƯU LẠI WIDTH CỦA CÁC CỘT ĐỂ NHƯỜNG CHỖ CHO TÊN TASK -->
                                                <thead>
                                                    <tr>
                                                        <th style="width:70px;"><%# GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                        <th style="min-width:260px;"><%# GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                        <th class="text-center" style="width:95px;">Ngày bắt đầu</th>
                                                        <th class="text-center" style="width:80px;"><%# GetResourceText(BackEndResourceKeys.DEADLINE) %></th>
                                                        <th class="text-center" style="width:105px;">Ngày kết thúc dự kiến</th>
                                                        <th class="text-center" style="width:105px;">Ngày kết thúc thực tế</th>
                                                        <th class="text-center" style="width:90px;"><%# GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                                                        <th class="text-center" style="width:115px;"><%# GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptDetailTasks" runat="server" DataSource='<%# Eval("MyTasks") %>'>
                                                        <ItemTemplate>
                                                            <tr class="task-row" data-status='<%# Eval("TrangThaiTask") %>'>
                                                                <td><strong><%# Eval("MaTask") %></strong></td>
                                                                <td>
                                                                    <span class="d-block fw-bold text-dark"><%# Eval("TenTask") %></span>
                                                                    <asp:PlaceHolder runat="server" Visible='<%# Eval("TenTaskCha") != null && !string.IsNullOrEmpty(Eval("TenTaskCha").ToString()) && Eval("TenTaskCha").ToString() != Eval("TenPhaseGoc").ToString() %>'>
                                                                        <div class="text-muted mt-1" style="font-size:11.5px;">
                                                                            <i class="fas fa-level-up-alt fa-rotate-90 me-1 text-secondary"></i>
                                                                            <%# GetResourceText(BackEndResourceKeys.BELONG_TO_GROUP) %>: <%# Eval("MaTaskCha") %> - <%# Eval("TenTaskCha") %>
                                                                        </div>
                                                                    </asp:PlaceHolder>
                                                                </td>
                                                                <td class="text-center"><%# FormatDateSafe(Eval("NgayBatDau"), "—") %></td>
                                                                <td class="text-center"><%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %></td>
                                                                <td class="text-center"><%# FormatDateSafe(Eval("NgayKetThuc"), "—") %></td>
                                                                <td class="text-center"><%# FormatDateSafe(Eval("NgayHoanThanhThucTe"), "—") %></td>
                                                                <td class="text-center"><span class="ui-badge" data-prio='<%# Eval("TenDoUuTien") %>'><%# Eval("TenDoUuTien") %></span></td>
                                                                <td class="text-center">
                                                                    <div class='<%# GetTaskStatusWrapperClass(Eval("TrangThaiTask"), Eval("NgayKetThuc"), Eval("NgayHoanThanhThucTe")) %>' data-status='<%# Eval("TrangThaiTask") %>'>
                                                                        <%# GetTaskStatusDisplay(Eval("TrangThaiTask"), Eval("NgayKetThuc"), Eval("NgayHoanThanhThucTe")) %>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        function openSidePanel() {
            var overlay = document.getElementById('sidePanelOverlay');
            var panel = document.getElementById('projectSidePanel');
            if (!overlay || !panel) return;
            overlay.style.display = 'block';
            setTimeout(function () {
                panel.classList.add('open');
                var body = panel.querySelector('.side-panel-body');
                if (body) { body.scrollTop = 0; }
            }, 10);
        }

        function closeSidePanel() {
            var panel = document.getElementById('projectSidePanel');
            var overlay = document.getElementById('sidePanelOverlay');
            if (!panel || !overlay) return;
            panel.classList.remove('open');
            setTimeout(function () { overlay.style.display = 'none'; }, 300);
        }

        function openProjectDetailPanel() {
            var overlay = document.getElementById('projectDetailOverlay');
            var panel = document.getElementById('projectDetailPanel');
            if (!overlay || !panel) return;
            overlay.style.display = 'block';
            setTimeout(function () {
                panel.classList.add('open');
                var body = panel.querySelector('.side-panel-body');
                if (body) { body.scrollTop = 0; }
            }, 10);
        }

        function closeProjectDetailPanel() {
            var panel = document.getElementById('projectDetailPanel');
            var overlay = document.getElementById('projectDetailOverlay');
            if (!panel || !overlay) return;
            panel.classList.remove('open');
            setTimeout(function () { overlay.style.display = 'none'; }, 300);
        }
        // ==========================================
        // VẼ LỊCH LÀM VIỆC TĨNH CỦA THÁNG HIỆN TẠI
        // ==========================================
        function renderEmployeeMonthCalendar() {
            var hiddenField = document.getElementById('hdfScheduleJson');
            var calDiv = document.getElementById('employeeMonthCalendar');

            // Chốt chặn: Tránh báo lỗi nếu UpdatePanel chưa render kịp các thẻ này
            if (!hiddenField || !calDiv) return;

            var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
            function pad(n) { return n < 10 ? '0' + n : '' + n; }
            function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
            function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }

            var jsonString = hiddenField.value;
            var data = {};
            if (jsonString) {
                try { data = JSON.parse(jsonString) || {}; } catch (e) { console.error("Lỗi parse Lịch", e); }
            }

            var now = new Date();
            var year = now.getFullYear();
            var month = now.getMonth();
            var todayKey = toKey(year, month, now.getDate());

            // Rút khoảng Min/Max từ JSON
            var keys = Object.keys(data).sort();
            var minKey = keys.length ? keys[0] : null;
            var maxKey = keys.length ? keys[keys.length - 1] : null;

            var offset = (new Date(year, month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div><div class="mc-grid">';

            // Vẽ cố định 6 hàng = 42 ô
            for (var i = 0; i < 42; i++) {
                var d = new Date(year, month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!minKey && key >= minKey && key <= maxKey;
                var info = inRange ? data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                if (key === todayKey) cls += ' today';

                var extra = '';
                if (info && info.text) {
                    var label = String(info.text).replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    extra = '<span class="mc-label">' + escAttr(label) + '</span>';
                }

                var tip = (info && info.text) ? ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"' : '';
                html += '<div class="' + cls + '"' + tip + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
            }
            html += '</div>';

            calDiv.innerHTML = html;
        }

        // 1. Chạy lần đầu khi load trang (F5)
        $(document).ready(function () {
            renderEmployeeMonthCalendar();
        });

        // 2. Chạy lại MỖI KHI UpdatePanel load xong ngầm (Đóng/Mở popup, lọc trạng thái...)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
                renderEmployeeMonthCalendar();
            });
        }
    </script>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlNhanVienPopup runat="server" ID="CtrlNhanVienPopup1" />
</asp:Content>