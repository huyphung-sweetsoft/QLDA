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
        
        .side-panel.open { right: 0; }
        
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

        .side-panel-body::-webkit-scrollbar { width: 8px; }
        .side-panel-body::-webkit-scrollbar-track { background: #f8f9fa; }
        .side-panel-body::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
        .side-panel-body::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

        .section-card { margin-bottom: 32px; }
        .sec-title { font-size: 14px; font-weight: 700; margin-bottom: 16px; display: flex; justify-content: space-between; align-items: center; color: #232220; text-transform: uppercase; }
        
        .project-box { border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 20px; overflow: hidden; background: #ffffff; box-shadow: 0 0.125rem 0.25rem rgba(0,0,0,0.05); }
        .project-box:last-child { margin-bottom: 0; }
        
        .proj-head { background: #ffffff; padding: 12px 16px; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #dee2e6; }
        .proj-title-text { font-size: 14px; font-weight: 700; color: #232220; }
        .proj-status-tag { font-size: 11px; padding: 4px 10px; border-radius: 12px; font-weight: 600; display: inline-block; }
        .proj-status-tag.doing { background: #e7eef5; color: #3a6ea5; }
        .proj-status-tag.done { background: #e6f4ea; color: #1e8e3e; }
        
        .phase-container { padding: 16px; background: #f8f9fa; }
        .phase-item { border: 1px solid #dee2e6; border-radius: 8px; padding: 16px; margin-bottom: 16px; background: #ffffff; }
        .phase-item:last-child { margin-bottom: 0; }
        .phase-header-line { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; padding-bottom: 10px; border-bottom: 1px solid #dee2e6; }
        .phase-title-name { font-weight: 700; font-size: 13px; color: #232220; }
        
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

                <div class="row align-items-stretch">
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
                                        <div class="spec-tile"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrCCCD" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrNgaySinh" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="spec-tile"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></span><span class="fw-bold text-dark"><asp:Literal ID="ltrGioiTinh" runat="server"></asp:Literal></span></div>
                                    </div>
                                    <div class="col-12" style="min-width: 0;">
                                        <div class="spec-tile" style="height: auto;"><span class="spec-label d-block"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></span><span class="fw-bold text-dark d-block" style="overflow-wrap: anywhere; word-break: break-word; white-space: normal;"><asp:Literal ID="ltrDiaChi" runat="server"></asp:Literal></span></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-xl-4 col-lg-5 d-flex flex-column gap-3 mt-3 mt-lg-0">
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

                        <a href="javascript:void(0);" onclick="openSidePanel('done')" class="card shadow-sm border border-light project-card flex-grow-1 text-decoration-none">
                            <div class="card-body d-flex flex-column justify-content-between">
                                <div class="d-flex justify-content-between align-items-start mb-3">
                                    <div>
                                        <h5 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PARTICIPATED) %></h5>
                                        <span class="text-muted" style="font-size: 13px; font-weight: 600;"><asp:Literal ID="ltrCountDoneProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.PARTICIPATED_PROJECTS) %></span>
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

            <!-- SIDE PANEL OFFCANVAS -->
            <div id="sidePanelOverlay" class="side-panel-overlay" onclick="closeSidePanel()"></div>
            <div id="projectSidePanel" class="side-panel">
                <div class="side-panel-header">
                    <h5 class="side-panel-title"><i class="fas fa-layer-group text-primary me-2"></i> <%= GetResourceText(BackEndResourceKeys.PROJECT_ALLOCATION_DETAILS) %></h5>
                    <button type="button" class="btn-close-panel" onclick="closeSidePanel()"><i class="fas fa-times"></i></button>
                </div>
                
                <div class="side-panel-body">
                    
                    <!-- NHÓM 1: ĐANG THỰC HIỆN -->
                    <div id="section_active_projects" class="section-card" runat="server" ClientIDMode="Static">
                        <div class="sec-title">
                            <span>⚡ <%= GetResourceText(BackEndResourceKeys.ACTIVE_PROJECTS).ToUpper() %> (<asp:Literal ID="ltrActiveCount" runat="server">0</asp:Literal>)</span>
                        </div>
                        
                        <div id="emptyActive" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                            <i class="far fa-folder-open mb-2" style="font-size: 24px; color: #ced4da;"></i><br />
                            <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                        </div>
                        
                        <div class="accordion" id="accordionActiveProjects">
                            <asp:Repeater ID="rptActiveProjects" runat="server">
                                <ItemTemplate>
                                    <div class="project-box mb-3 border rounded shadow-sm bg-white overflow-hidden">
                                        <div class="proj-head d-flex justify-content-between align-items-center p-3" 
                                             data-bs-toggle="collapse" 
                                             data-bs-target='#collapseActive_<%# Eval("IdDuAn") %>' 
                                             aria-expanded='<%# Container.ItemIndex == 0 ? "true" : "false" %>' 
                                             style="cursor: pointer; background: #fdfdfd; border-bottom: 1px solid #dee2e6;">
                                            <div>
                                                <a href='<%# GetProjectUrl(Eval("IdDuAn")) %>' class="proj-title-text text-decoration-none fw-bold text-dark fs-6" target="_blank"><%# Eval("TenDuAn") %> (<%# Eval("MaDuAn") %>)</a>
                                                <span style="font-size:12px;color:#6c757d;margin-left:8px; display: block; margin-top: 4px;">
                                                    <%# GetResourceText(BackEndResourceKeys.ROLE) %>: <strong><%# Eval("VaiTro") %></strong> 
                                                    <span class="mx-2">|</span> 
                                                    <%# GetResourceText(BackEndResourceKeys.PROJECT_TIME) %>: <%# FormatDateRange(Eval("ProjectStartDate"), Eval("ProjectEndDate")) %>
                                                </span>
                                            </div>
                                            <div class="text-end">
                                                <span class="badge bg-primary px-2 py-1 mb-1" style="font-size: 12px;"><%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Eval("ContributionPercent") %>%</span>
                                                <br/>
                                                <span class="proj-status-tag doing"><%# Eval("TrangThaiText") %></span>
                                            </div>
                                        </div>
                                        
                                        <div id='collapseActive_<%# Eval("IdDuAn") %>' 
                                             class='accordion-collapse collapse <%# Container.ItemIndex == 0 ? "show" : "" %>' 
                                             data-bs-parent="#accordionActiveProjects">
                                            <div class="phase-container p-3 bg-light">
                                                <asp:Repeater ID="rptPhasesActive" runat="server" DataSource='<%# Eval("Phases") %>'>
                                                    <ItemTemplate>
                                                        <div class="phase-item bg-white border rounded p-3 mb-3">
                                                            <div class="phase-header-line d-flex justify-content-between align-items-center mb-2 pb-2 border-bottom">
                                                                <div class="phase-title-name fw-bold text-dark" style="font-size: 13px;">
                                                                    <i class="far fa-folder-open text-warning me-1"></i> <%# GetResourceText(BackEndResourceKeys.PHASE) %>: <%# Eval("MaPhase") %> <%# Eval("TenPhase") %> 
                                                                    <span class="text-muted fw-normal ms-1">
                                                                        (<%# FormatDateRange(Eval("MinStartDate"), Eval("MaxEndDate")) %> | <%# GetResourceText(BackEndResourceKeys.DEADLINE) %>: <%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %>)
                                                                    </span>
                                                                </div>
                                                                <div class='<%# Convert.ToDouble(Eval("ContributionPercent")) >= 50 ? "badge bg-info text-dark" : "badge bg-light text-dark border" %>'>
                                                                    % <%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Eval("ContributionPercent") %>%
                                                                </div>
                                                            </div>
                                                            
                                                            <asp:PlaceHolder runat="server" Visible='<%# HasTasks(Eval("MyTasks")) %>'>
                                                                <table class="task-sub-table w-100">
                                                                    <thead>
                                                                        <tr class="bg-light text-muted">
                                                                            <th class="p-2" style="width: 90px;"><%# GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                                            <th class="p-2"><%# GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                                            <th class="p-2" style="width: 140px;"><%# GetResourceText(BackEndResourceKeys.TIMEFRAME) %></th>
                                                                            <th class="p-2 text-center" style="width: 90px;"><%# GetResourceText(BackEndResourceKeys.DEADLINE) %></th>
                                                                            <th class="p-2 text-center" style="width: 110px;"><%# GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                                                                            <th class="p-2 text-center" style="width: 120px;"><%# GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                                        </tr>
                                                                    </thead>
                                                                    <tbody>
                                                                        <asp:Repeater ID="rptTasksActive" runat="server" DataSource='<%# Eval("MyTasks") %>'>
                                                                            <ItemTemplate>
                                                                                <tr class="border-bottom">
                                                                                    <td class="p-2"><b><%# Eval("MaTask") %></b></td>
                                                                                    <td class="p-2">
                                                                                        <span class="d-block fw-bold text-dark"><%# Eval("TenTask") %></span>
                                                                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("TenTaskCha") != null && !string.IsNullOrEmpty(Eval("TenTaskCha").ToString()) && Eval("TenTaskCha").ToString() != Eval("TenPhaseGoc").ToString() %>'>
                                                                                            <div class="text-muted mt-1" style="font-size: 11.5px;">
                                                                                                <i class="fas fa-level-up-alt fa-rotate-90 me-1 text-secondary"></i><%# GetResourceText(BackEndResourceKeys.BELONG_TO_GROUP) %>: <%# Eval("MaTaskCha") %> - <%# Eval("TenTaskCha") %>
                                                                                            </div>
                                                                                        </asp:PlaceHolder>
                                                                                    </td>
                                                                                    <td class="p-2"><%# FormatDateRange(Eval("NgayBatDau"), Eval("NgayKetThuc")) %></td>
                                                                                    <td class="p-2 text-center"><%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %></td>
                                                                                    <td class="p-2 text-center align-middle">
                                                                                        <%# GetTaskPriorityBadge(Eval("TenDoUuTien"), Eval("DiemUuTien")) %>
                                                                                    </td>
                                                                                    <td class="p-2 text-center align-middle">
                                                                                        <%# GetTaskStatusBadge(Eval("TrangThaiTask")) %>
                                                                                    </td>
                                                                                </tr>
                                                                            </ItemTemplate>
                                                                        </asp:Repeater>
                                                                    </tbody>
                                                                </table>
                                                            </asp:PlaceHolder>

                                                            <asp:PlaceHolder runat="server" Visible='<%# !HasTasks(Eval("MyTasks")) %>'>
                                                                <div class="text-center p-2 mt-2 border rounded border-dashed bg-light">
                                                                    <span class="text-muted" style="font-size: 12px;"><i><%# GetResourceText(BackEndResourceKeys.NO_TASKS_IN_THIS_PHASE) %></i></span>
                                                                </div>
                                                            </asp:PlaceHolder>
                                                        </div>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    <!-- NHÓM 2: ĐÃ HOÀN THÀNH / LỊCH SỬ -->
                    <div id="section_done_projects" class="section-card mt-4" runat="server" ClientIDMode="Static">
                        <div class="sec-title text-success">
                            <span>✅ <%= GetResourceText(BackEndResourceKeys.PARTICIPATED).ToUpper() %> (<asp:Literal ID="ltrDoneCount" runat="server">0</asp:Literal>)</span>
                        </div>
                        
                        <div id="emptyDone" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                            <i class="far fa-folder-open mb-2" style="font-size: 24px; color: #ced4da;"></i><br />
                            <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                        </div>
                        
                        <div class="accordion" id="accordionDoneProjects">
                            <asp:Repeater ID="rptDoneProjects" runat="server">
                                <ItemTemplate>
                                    <div class="project-box mb-3 border rounded shadow-sm bg-white overflow-hidden">
                                        <div class="proj-head d-flex justify-content-between align-items-center p-3" 
                                             data-bs-toggle="collapse" 
                                             data-bs-target='#collapseDone_<%# Eval("IdDuAn") %>' 
                                             aria-expanded="false" 
                                             style="cursor: pointer; background: #fdfdfd; border-bottom: 1px solid #dee2e6;">
                                            <div>
                                                <a href='<%# GetProjectUrl(Eval("IdDuAn")) %>' class="proj-title-text text-decoration-none fw-bold text-dark fs-6" target="_blank"><%# Eval("TenDuAn") %> (<%# Eval("MaDuAn") %>)</a>
                                                <span style="font-size:12px;color:#6c757d;margin-left:8px; display: block; margin-top: 4px;">
                                                    <%# GetResourceText(BackEndResourceKeys.ROLE) %>: <strong><%# Eval("VaiTro") %></strong> 
                                                    <span class="mx-2">|</span> 
                                                    <%# GetResourceText(BackEndResourceKeys.PROJECT_TIME) %>: <%# FormatDateRange(Eval("ProjectStartDate"), Eval("ProjectEndDate")) %>
                                                </span>
                                            </div>
                                            <div class="text-end">
                                                <span class="badge bg-secondary px-2 py-1 mb-1" style="font-size: 12px;"><%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Eval("ContributionPercent") %>%</span>
                                                <br/>
                                                <span class="proj-status-tag done" style='<%# Eval("TrangThai").ToString() == "4" ? "background:#f8d7da; color:#dc3545;" : (Eval("TrangThai").ToString() == "3" ? "background:#fff3cd; color:#856404;" : "") %>'><%# Eval("TrangThaiText") %></span>
                                            </div>
                                        </div>
                                        
                                        <div id='collapseDone_<%# Eval("IdDuAn") %>' 
                                             class='accordion-collapse collapse' 
                                             data-bs-parent="#accordionDoneProjects">
                                            <div class="phase-container p-3 bg-light">
                                                <asp:Repeater ID="rptPhasesDone" runat="server" DataSource='<%# Eval("Phases") %>'>
                                                    <ItemTemplate>
                                                        <div class="phase-item bg-white border rounded p-3 mb-3">
                                                            <div class="phase-header-line d-flex justify-content-between align-items-center mb-2 pb-2 border-bottom">
                                                                <div class="phase-title-name fw-bold text-dark" style="font-size: 13px;">
                                                                    <i class="far fa-folder-open text-warning me-1"></i> <%# GetResourceText(BackEndResourceKeys.PHASE) %>: <%# Eval("MaPhase") %> <%# Eval("TenPhase") %> 
                                                                    <span class="text-muted fw-normal ms-1">
                                                                        (<%# FormatDateRange(Eval("MinStartDate"), Eval("MaxEndDate")) %> | <%# GetResourceText(BackEndResourceKeys.DEADLINE) %>: <%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %>)
                                                                    </span>
                                                                </div>
                                                                <div class='<%# Convert.ToDouble(Eval("ContributionPercent")) >= 50 ? "badge bg-info text-dark" : "badge bg-light text-dark border" %>'>
                                                                    % <%# GetResourceText(BackEndResourceKeys.CONTRIBUTION) %>: <%# Eval("ContributionPercent") %>%
                                                                </div>
                                                            </div>
                                                            
                                                            <asp:PlaceHolder runat="server" Visible='<%# HasTasks(Eval("MyTasks")) %>'>
                                                                <table class="task-sub-table w-100">
                                                                    <thead>
                                                                        <tr class="bg-light text-muted">
                                                                            <th class="p-2" style="width: 90px;"><%# GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                                            <th class="p-2"><%# GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                                            <th class="p-2" style="width: 140px;"><%# GetResourceText(BackEndResourceKeys.TIMEFRAME) %></th>
                                                                            <th class="p-2 text-center" style="width: 90px;"><%# GetResourceText(BackEndResourceKeys.DEADLINE) %></th>
                                                                            <th class="p-2 text-center" style="width: 110px;"><%# GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                                                                            <th class="p-2 text-center" style="width: 120px;"><%# GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                                        </tr>
                                                                    </thead>
                                                                    <tbody>
                                                                        <asp:Repeater ID="rptTasksDone" runat="server" DataSource='<%# Eval("MyTasks") %>'>
                                                                            <ItemTemplate>
                                                                                <tr class="border-bottom">
                                                                                    <td class="p-2"><b><%# Eval("MaTask") %></b></td>
                                                                                    <td class="p-2">
                                                                                        <span class="d-block fw-bold text-dark"><%# Eval("TenTask") %></span>
                                                                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("TenTaskCha") != null && !string.IsNullOrEmpty(Eval("TenTaskCha").ToString()) && Eval("TenTaskCha").ToString() != Eval("TenPhaseGoc").ToString() %>'>
                                                                                            <div class="text-muted mt-1" style="font-size: 11.5px;">
                                                                                                <i class="fas fa-level-up-alt fa-rotate-90 me-1 text-secondary"></i><%# GetResourceText(BackEndResourceKeys.BELONG_TO_GROUP) %>: <%# Eval("MaTaskCha") %> - <%# Eval("TenTaskCha") %>
                                                                                            </div>
                                                                                        </asp:PlaceHolder>
                                                                                    </td>
                                                                                    <td class="p-2"><%# FormatDateRange(Eval("NgayBatDau"), Eval("NgayKetThuc")) %></td>
                                                                                    <td class="p-2 text-center"><%# Eval("ThoiHanNgay") %> <%# GetResourceText(BackEndResourceKeys.DAY) %></td>
                                                                                    <td class="p-2 text-center align-middle">
                                                                                        <%# GetTaskPriorityBadge(Eval("TenDoUuTien"), Eval("DiemUuTien")) %>
                                                                                    </td>
                                                                                    <td class="p-2 text-center align-middle">
                                                                                        <%# GetTaskStatusBadge(Eval("TrangThaiTask")) %>
                                                                                    </td>
                                                                                </tr>
                                                                            </ItemTemplate>
                                                                        </asp:Repeater>
                                                                    </tbody>
                                                                </table>
                                                            </asp:PlaceHolder>

                                                            <asp:PlaceHolder runat="server" Visible='<%# !HasTasks(Eval("MyTasks")) %>'>
                                                                <div class="text-center p-2 mt-2 border rounded border-dashed bg-light">
                                                                    <span class="text-muted" style="font-size: 12px;"><i><%# GetResourceText(BackEndResourceKeys.NO_TASKS_IN_THIS_PHASE) %></i></span>
                                                                </div>
                                                            </asp:PlaceHolder>
                                                        </div>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                </div> 
            </div> 

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        function openSidePanel(targetSection) {
            var activeSection = document.getElementById('section_active_projects');
            var doneSection = document.getElementById('section_done_projects');

            if (targetSection === 'active') {
                if (activeSection) activeSection.style.display = 'block';
                if (doneSection) doneSection.style.display = 'none';
            } else if (targetSection === 'done') {
                if (activeSection) activeSection.style.display = 'none';
                if (doneSection) doneSection.style.display = 'block';
            }

            document.getElementById('sidePanelOverlay').style.display = 'block';

            setTimeout(() => {
                document.getElementById('projectSidePanel').classList.add('open');
                document.querySelector('.side-panel-body').scrollTop = 0;
            }, 10);
        }

        function closeSidePanel() {
            document.getElementById('projectSidePanel').classList.remove('open');

            setTimeout(() => {
                document.getElementById('sidePanelOverlay').style.display = 'none';
            }, 300);
        }
    </script>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlUserDetail runat="server" ID="CtrlUserDetail1" />
</asp:Content>