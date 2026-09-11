<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="NhanVienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.NhanVienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fUsers/Controls/CtrlUserDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlUserDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .project-card { transition: all 0.2s ease-in-out; cursor: pointer; }
        .project-card:hover { border-color: #3a6ea5 !important; transform: translateY(-2px); box-shadow: 0 .5rem 1rem rgba(0,0,0,.15)!important; }
        .icon-box { width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; border-radius: 12px; font-size: 20px; flex-shrink: 0; }
        .spec-tile { background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 0.5rem; padding: 1rem; height: 100%; min-width: 0; overflow-wrap: anywhere; word-break: break-word; }
        .col-xl-8.d-flex.flex-column, .col-xl-8.d-flex.flex-column .card, .col-xl-8.d-flex.flex-column .card-body { min-width: 0; }
        .spec-label { font-size: 11px; font-weight: 700; color: #6c757d; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 0.25rem; }
        
        /* ==========================================
           CSS CHO OFFCANVAS (BẢNG TRƯỢT TỪ LỀ PHẢI)
           ========================================== */
        .side-panel-overlay {
            position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
            background: rgba(0, 0, 0, 0.5); z-index: 1040; display: none;
            backdrop-filter: blur(2px);
        }
        
        .side-panel {
            position: fixed; top: 0; right: -100%; width: 850px; max-width: 100vw; height: 100vh;
            background: #f4f5f7; box-shadow: -5px 0 25px rgba(0,0,0,0.15);
            z-index: 1050; transition: right 0.3s cubic-bezier(0.82, 0.085, 0.395, 0.895);
            display: flex; flex-direction: column;
        }
        
        .side-panel.open { right: 0; } /* Class để kích hoạt trượt ra */
        
        .side-panel-header {
            padding: 16px 24px; background: #ffffff; border-bottom: 1px solid #dee2e6;
            display: flex; justify-content: space-between; align-items: center;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02); z-index: 10;
        }
        
        .side-panel-title { font-size: 16px; font-weight: 700; color: #232220; margin: 0; text-transform: uppercase; }
        .btn-close-panel { background: none; border: none; font-size: 20px; color: #6c757d; cursor: pointer; transition: color 0.2s; }
        .btn-close-panel:hover { color: #dc3545; }
        
        .side-panel-body {
            padding: 24px; overflow-y: auto; flex-grow: 1; scroll-behavior: smooth;
        }

        /* Tùy chỉnh thanh cuộn của Panel */
        .side-panel-body::-webkit-scrollbar { width: 8px; }
        .side-panel-body::-webkit-scrollbar-track { background: #f8f9fa; }
        .side-panel-body::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
        .side-panel-body::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

        /* CSS Ruột danh sách Dự án */
        .section-card { margin-bottom: 32px; }
        .sec-title { font-size: 14px; font-weight: 700; margin-bottom: 16px; display: flex; justify-content: space-between; align-items: center; color: #232220; text-transform: uppercase; }
        
        .project-box { border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 20px; overflow: hidden; background: #ffffff; box-shadow: 0 0.125rem 0.25rem rgba(0,0,0,0.05); }
        .project-box:last-child { margin-bottom: 0; }
        
        .proj-head { background: #ffffff; padding: 12px 16px; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #dee2e6; }
        .proj-title-text { font-size: 14px; font-weight: 700; color: #232220; }
        .proj-status-tag { font-size: 11px; padding: 4px 10px; border-radius: 12px; font-weight: 600; }
        .proj-status-tag.doing { background: #e7eef5; color: #3a6ea5; }
        .proj-status-tag.done { background: #e6f4ea; color: #1e8e3e; }
        
        .phase-container { padding: 16px; background: #f8f9fa; }
        .phase-item { border: 1px solid #dee2e6; border-radius: 8px; padding: 16px; margin-bottom: 16px; background: #ffffff; }
        .phase-item:last-child { margin-bottom: 0; }
        .phase-header-line { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; padding-bottom: 10px; border-bottom: 1px dashed #dee2e6; }
        .phase-title-name { font-weight: 700; font-size: 13px; color: #232220; }
        .phase-alloc-badge { font-size: 12px; font-weight: 700; color: #3a6ea5; background: #e7eef5; padding: 4px 10px; border-radius: 12px; border: 1px solid #3a6ea5; }
        .phase-alloc-badge.over-allocated { color: #dc3545; background: #f8d7da; border-color: #dc3545; }
        .phase-capacity-strip { display: flex; gap: 20px; border: 1px solid #dee2e6; border-radius: 6px; padding: 10px 16px; margin-bottom: 12px; font-size: 12px; flex-wrap: wrap; }
        .cap-item span { color: #6c757d; }
        .cap-item b { color: #232220; font-size: 13px; }
        
        .task-sub-table { width: 100%; border-collapse: collapse; font-size: 13px; background: #ffffff; border-radius: 6px; overflow: hidden; border: 1px solid #dee2e6; }
        .task-sub-table th { background: #f8f9fa; padding: 10px 14px; text-align: left; color: #6c757d; font-weight: 600; border-bottom: 2px solid #dee2e6; }
        .task-sub-table td { padding: 10px 14px; border-bottom: 1px solid #dee2e6; vertical-align: middle; }
        .task-sub-table tr:last-child td { border-bottom: none; }
        .tag-ok { color: #1e8e3e; font-weight: 600; }
        .tag-warn { color: #b98a00; font-weight: 600; }
        .tag-done { color: #6c757d; font-weight: 600; text-decoration: line-through; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upnlMainDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container-fluid px-0">
        
                <SweetSoft:Navigation ID="Navigation1" runat="server"/>

                <!-- CẤU TRÚC 2 CỘT TỔNG QUAN -->
                <div class="row align-items-stretch">
                    <!-- CỘT TRÁI: PROFILE -->
                    <div class="col-xl-8 col-lg-7 d-flex flex-column gap-3">
                        <div class="card shadow-sm border-0">
                            <div class="card-body d-flex flex-column flex-md-row align-items-center gap-4">
                                <div class="flex-shrink-0">
                                    <img id="imgAvatar" runat="server" src="/Styles/images/user-icon.png" class="rounded-circle border border-3 border-primary shadow-sm" style="width: 100px; height: 100px; object-fit: cover;" onerror="this.src='/Styles/images/user-icon.png'" />
                                </div>
                                <div class="flex-grow-1 w-100 text-center text-md-start">
                                    <div class="d-flex flex-column flex-md-row align-items-center gap-2 mb-2">
                                        <h4 class="mb-0 fw-bold text-dark"><asp:Literal ID="ltrTenNhanVien" runat="server"></asp:Literal></h4>
                                        <span class="badge bg-success rounded-pill px-3 py-1"><%= GetResourceText(BackEndResourceKeys.ACTIVE) %></span>
                                    </div>
                                    <div class="row g-2 text-muted" style="font-size: 14px;">
                                        <div class="col-sm-6"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %>: <strong class="text-dark"><asp:Literal ID="ltrChucDanh" runat="server"></asp:Literal></strong></div>
                                        <div class="col-sm-6"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %>: <strong class="text-dark"><asp:Literal ID="ltrPhongBan" runat="server"></asp:Literal></strong></div>
                                        <div class="col-sm-6">Email: <strong class="text-dark"><asp:Literal ID="ltrEmail" runat="server"></asp:Literal></strong></div>
                                        <div class="col-sm-6"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>: <strong class="text-dark"><asp:Literal ID="ltrPhone" runat="server"></asp:Literal></strong></div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="card shadow-sm border-0 flex-grow-1">
                            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3 px-4 border-bottom">
                                <h6 class="mb-0 fw-bold text-dark"><i class="fas fa-id-card text-primary me-2"></i> <%= GetResourceText(BackEndResourceKeys.PERSONAL_AND_WORK_INFORMATION) %></h6>
                                <SweetSoft:ExtraButton runat="server" ID="btnEditProfile" CssClass="btn-sm btn-light border text-primary fw-bold" ButtonIcon="Edit" OnClick="btnEditProfile_Click"></SweetSoft:ExtraButton>
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
                                        <div class="spec-tile bg-white"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrCCCD" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile bg-white"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrNgaySinh" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile bg-white"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrGioiTinh" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-12" style="min-width: 0;">
                                        <div class="spec-tile bg-white" style="height: auto;"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></span><span class="fw-bold text-dark d-block" style="overflow-wrap: anywhere; word-break: break-word; white-space: normal;"><asp:Literal ID="ltrDiaChi" runat="server"></asp:Literal></span></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- CỘT PHẢI: MODULE SUMMARY LỊCH BIỂU & TRIGGER MỞ SIDE PANEL -->
                    <div class="col-xl-4 col-lg-5 d-flex flex-column gap-3 mt-3 mt-lg-0">
                        <!-- Gọi Javascript: openSidePanel('active') khi click -->
                        <a href="javascript:void(0);" onclick="openSidePanel('active')" class="card shadow-sm border border-light project-card flex-grow-1 text-decoration-none">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PARTICIPATING) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;"><asp:Literal ID="ltrCountActiveProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.ACTIVE_PROJECTS) %></span>
                                    </div>
                                    <div class="icon-box bg-primary bg-opacity-10 text-primary"><i class="fas fa-bolt"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end"><span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %> <i class="fas fa-arrow-right ms-1"></i></span></div>
                            </div>
                        </a>

                        <!-- Gọi Javascript: openSidePanel('done') khi click -->
                        <a href="javascript:void(0);" onclick="openSidePanel('done')" class="card shadow-sm border border-light project-card flex-grow-1 text-decoration-none">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.COMPLETED) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;"><asp:Literal ID="ltrCountDoneProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.ACCEPTED_PROJECTS) %></span>
                                    </div>
                                    <div class="icon-box bg-success bg-opacity-10 text-success"><i class="fas fa-check-circle"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end"><span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %> <i class="fas fa-arrow-right ms-1"></i></span></div>
                            </div>
                        </a>

                        <a id="lnkSchedule" runat="server" class="card shadow-sm border border-light project-card flex-grow-1 text-decoration-none">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;"><%= GetResourceText(BackEndResourceKeys.VIEW_WORK_SCHEDULE) %></span>
                                    </div>
                                    <div class="icon-box bg-info bg-opacity-10 text-info" style="color: #681da8 !important; background-color: #f3e8fd !important;"><i class="fas fa-calendar-alt"></i></div>
                                </div>
                                <div class="border-top pt-3 mt-auto text-end"><span class="text-primary fw-bold" style="font-size: 13px;"><%= GetResourceText(BackEndResourceKeys.OPEN_SCHEDULE) %> <i class="fas fa-arrow-right ms-1"></i></span></div>
                            </div>
                        </a>
                    </div>
                </div>

            </div>

            <!-- =========================================================================
                 SIDE PANEL (OFFCANVAS) HIỂN THỊ CHI TIẾT DỰ ÁN 
                 Nó được nhấc ra ngoài luồng Grid chính để làm màn hình trượt đè lên
                 ========================================================================= -->
            <div id="sidePanelOverlay" class="side-panel-overlay" onclick="closeSidePanel()"></div>
            <div id="projectSidePanel" class="side-panel">
                <div class="side-panel-header">
                    <h5 class="side-panel-title"><i class="fas fa-layer-group text-primary me-2"></i> Chi tiết phân bổ dự án</h5>
                    <button type="button" class="btn-close-panel" onclick="closeSidePanel()"><i class="fas fa-times"></i></button>
                </div>
                
                <div class="side-panel-body">
                    
                    <!-- NHÓM 1: ĐANG THỰC HIỆN -->
                    <div id="section_active_projects" class="section-card" runat="server" ClientIDMode="Static">
                        <div class="sec-title">
                            <span>⚡ <%= GetResourceText(BackEndResourceKeys.ACTIVE_PROJECTS).ToUpper() %> (<asp:Literal ID="ltrActiveCount" runat="server">0</asp:Literal>)</span>
                        </div>
                        
                        <!-- EMPTY STATE -->
                        <div id="emptyActive" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                            <i class="far fa-folder-open mb-2" style="font-size: 24px; color: #ced4da;"></i><br />
                            <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                        </div>
                        
                        <asp:Repeater ID="rptActiveProjects" runat="server">
                            <ItemTemplate>
                                <div class="project-box">
                                    <div class="proj-head">
                                        <div>
                                            <a href='<%# GetProjectUrl(Eval("IdDuAn")) %>' class="proj-title-text text-decoration-none" target="_blank"><%# Eval("TenDuAn") %> (<%# Eval("MaDuAn") %>)</a>
                                            <span style="font-size:12px;color:#6c757d;margin-left:8px; display: block; margin-top: 4px;">
                                                <%= GetResourceText(BackEndResourceKeys.ROLE) %>: <strong><%# Eval("VaiTro") %></strong> · <%= GetResourceText(BackEndResourceKeys.TIME_FRAME) %>: <%# Eval("MinStartDate", "{0:dd/MM/yyyy}") %> → <%# Eval("MaxEndDate", "{0:dd/MM/yyyy}") %>
                                            </span>
                                        </div>
                                        <span class="proj-status-tag doing"><%# Eval("TrangThaiDuAn") %></span>
                                    </div>
                                    <div class="phase-container">
                                        <asp:Repeater ID="rptPhasesActive" runat="server" DataSource='<%# Eval("Phases") %>'>
                                            <ItemTemplate>
                                                <div class="phase-item">
                                                    <div class="phase-header-line">
                                                        <div class="phase-title-name"><i class="far fa-folder-open text-warning me-1"></i> <%= GetResourceText(BackEndResourceKeys.PHASE) %>: <%# Eval("TenPhase") %> (<%# Eval("MinStartDate", "{0:dd/MM}") %> — <%# Eval("MaxEndDate", "{0:dd/MM/yyyy}") %>)</div>
                                                        <div class='<%# Convert.ToDouble(Eval("AllocationPercent")) > 100 ? "phase-alloc-badge over-allocated" : "phase-alloc-badge" %>'>
                                                            <%= GetResourceText(BackEndResourceKeys.ALLOCATION_PERCENTAGE) %> <%# Eval("AllocationPercent") %>%
                                                        </div>
                                                    </div>
                                                    <div class="phase-capacity-strip">
                                                        <div class="cap-item"><span><%= GetResourceText(BackEndResourceKeys.CAPACITY) %>:</span> <b><%# Eval("CapacityDays") %> <%= GetResourceText(BackEndResourceKeys.STANDARD_DAYS) %></b></div>
                                                        <div class="cap-item"><span><%= GetResourceText(BackEndResourceKeys.WORKLOAD) %>:</span> <b><%# Eval("TotalWorkload") %></b></div>
                                                    </div>
                                                    <table class="task-sub-table">
                                                        <thead>
                                                            <tr>
                                                                <th style="width: 90px;"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                                <th><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                                <th style="width: 140px;"><%= GetResourceText(BackEndResourceKeys.TIME_FRAME) %></th>
                                                                <th style="width: 90px; text-align: center;"><%= GetResourceText(BackEndResourceKeys.DURATION_D) %></th>
                                                                <th style="width: 90px; text-align: center;"><%= GetResourceText(BackEndResourceKeys.PRIORITY_P) %></th>
                                                                <th style="width: 120px;"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <asp:Repeater ID="rptTasksActive" runat="server" DataSource='<%# Eval("Tasks") %>'>
                                                                <ItemTemplate>
                                                                    <tr>
                                                                        <td><b><%# Eval("MaTask") %></b></td>
                                                                        <td><%# Eval("TenTask") %></td>
                                                                        <td><%# Eval("NgayBatDau", "{0:dd/MM}") %> — <%# Eval("NgayKetThuc", "{0:dd/MM/yyyy}") %></td>
                                                                        <td class="text-center"><%# Eval("ThoiHanNgay") %> <%= GetResourceText(BackEndResourceKeys.DAY).ToLower() %></td>
                                                                        <td class="text-center"><%= GetResourceText(BackEndResourceKeys.COEFFICIENT) %> <%# Eval("DiemUuTien") %></td>
                                                                        <td>
                                                                            <span class='<%# Eval("TrangThaiTask").ToString() == "2" ? "tag-done" : (Eval("TrangThaiText").ToString().Contains("Sắp tới") ? "tag-warn" : "tag-ok") %>'>
                                                                                <%# Eval("TrangThaiTask").ToString() == "2" ? "<i class='fas fa-check me-1'></i>" : "🟢" %> <%# Eval("TrangThaiText") %>
                                                                            </span>
                                                                        </td>
                                                                    </tr>
                                                                </ItemTemplate>
                                                            </asp:Repeater>
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <!-- NHÓM 2: ĐÃ HOÀN THÀNH -->
                    <div id="section_done_projects" class="section-card mt-4" runat="server" ClientIDMode="Static">
                        <div class="sec-title text-success">
                            <span>✅ <%= GetResourceText(BackEndResourceKeys.COMPLETED).ToUpper() %> (<asp:Literal ID="ltrDoneCount" runat="server">0</asp:Literal>)</span>
                        </div>
                        
                        <!-- EMPTY STATE -->
                        <div id="emptyDone" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                            <i class="far fa-folder-open mb-2" style="font-size: 24px; color: #ced4da;"></i><br />
                            <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                        </div>
                        
                        <asp:Repeater ID="rptDoneProjects" runat="server">
                            <ItemTemplate>
                                <div class="project-box" style="opacity: 0.85;">
                                    <div class="proj-head">
                                        <div>
                                            <a href='<%# GetProjectUrl(Eval("IdDuAn")) %>' class="proj-title-text text-decoration-none text-secondary" target="_blank"><%# Eval("TenDuAn") %> (<%# Eval("MaDuAn") %>)</a>
                                            <span style="font-size:12px;color:#6c757d;margin-left:8px; display: block; margin-top: 4px;"><%= GetResourceText(BackEndResourceKeys.ROLE) %>: <strong><%# Eval("VaiTro") %></strong></span>
                                        </div>
                                        <span class="proj-status-tag done"><%# Eval("TrangThaiDuAn") %></span>
                                    </div>
                                    <div class="phase-container">
                                        <asp:Repeater ID="rptPhasesDone" runat="server" DataSource='<%# Eval("Phases") %>'>
                                            <ItemTemplate>
                                                <div class="phase-item">
                                                    <div class="phase-header-line">
                                                        <div class="phase-title-name text-muted"><%# Eval("TenPhase") %></div>
                                                        <div class="phase-alloc-badge" style="background:#e6f4ea; color:#1e8e3e; border:none;"><%= GetResourceText(BackEndResourceKeys.COMPLETED_100_PERCENT) %></div>
                                                    </div>
                                                    <table class="task-sub-table text-muted">
                                                        <tbody>
                                                            <asp:Repeater ID="rptTasksDone" runat="server" DataSource='<%# Eval("Tasks") %>'>
                                                                <ItemTemplate>
                                                                    <tr>
                                                                        <td style="width: 90px;"><b><%# Eval("MaTask") %></b></td>
                                                                        <td><%# Eval("TenTask") %></td>
                                                                        <td style="width: 140px;"><%# Eval("NgayBatDau", "{0:dd/MM}") %> — <%# Eval("NgayKetThuc", "{0:dd/MM/yyyy}") %></td>
                                                                        <td style="width: 130px;"><span class="tag-done"><i class="fas fa-check-double me-1"></i> <%= GetResourceText(BackEndResourceKeys.COMPLETED) %></span></td>
                                                                    </tr>
                                                                </ItemTemplate>
                                                            </asp:Repeater>
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                </div> <!-- Kết thúc side-panel-body -->
            </div> <!-- Kết thúc side-panel -->

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        // HÀM MỞ OFFCANVAS SIDE PANEL VÀ LỌC DỮ LIỆU
        function openSidePanel(targetSection) {
            // 1. Lấy DOM của 2 Section
            var activeSection = document.getElementById('section_active_projects');
            var doneSection = document.getElementById('section_done_projects');

            // 2. Ẩn/Hiện chéo nhau tùy theo nút được bấm
            if (targetSection === 'active') {
                if (activeSection) activeSection.style.display = 'block';
                if (doneSection) doneSection.style.display = 'none';
            } else if (targetSection === 'done') {
                if (activeSection) activeSection.style.display = 'none';
                if (doneSection) doneSection.style.display = 'block';
            }

            // 3. Bật nền xám
            document.getElementById('sidePanelOverlay').style.display = 'block';

            // 4. Trượt Panel ra và cuộn panel lên đầu trang
            setTimeout(() => {
                document.getElementById('projectSidePanel').classList.add('open');
                document.querySelector('.side-panel-body').scrollTop = 0;
            }, 10);
        }

        // HÀM ĐÓNG OFFCANVAS SIDE PANEL
        function closeSidePanel() {
            // 1. Trượt Panel vào trong
            document.getElementById('projectSidePanel').classList.remove('open');

            // 2. Chờ trượt xong thì tắt nền xám
            setTimeout(() => {
                document.getElementById('sidePanelOverlay').style.display = 'none';
            }, 300);
        }
    </script>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlUserDetail runat="server" ID="CtrlUserDetail1" />
</asp:Content>