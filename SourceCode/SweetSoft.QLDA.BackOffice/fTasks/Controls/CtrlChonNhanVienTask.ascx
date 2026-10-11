<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChonNhanVienTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlChonNhanVienTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* ========================================================
       1. KIẾN TRÚC LAYOUT TỔNG
       ======================================================== */
    .task-modal-body {
        display: flex;
        flex-direction: column;
        /* GIẢM XUỐNG 80vh VÀ TRỪ HAO ĐỦ LỚN ĐỂ KHÔNG BAO GIỜ KHUẤT NÚT LƯU */
        height: calc(100vh - 160px); 
        max-height: 80vh;
        min-height: 500px;
        min-width: 0;
        box-sizing: border-box;
        overflow: hidden;
    }
    
    .task-modal-header-info {
        flex-shrink: 0; 
        margin-bottom: 12px;
    }
    
    .info-row-custom {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: nowrap !important;
        gap: 10px;
        overflow: hidden;
    }
    .info-row-custom > div { margin-bottom: 0 !important; white-space: nowrap; text-overflow: ellipsis; overflow: hidden; }

    .task-member-columns {
        display: grid;
        grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
        gap: 16px;
        flex: 1 1 0;
        min-height: 0;
        min-width: 0;
        overflow: hidden; 
        padding-bottom: 5px;
    }

    @media (max-width: 991px) {
        .task-member-columns {
            grid-template-columns: minmax(0, 1fr);
            grid-template-rows: minmax(0, 1fr) minmax(0, 1fr);
            overflow: hidden;
        }
    }

    /* ========================================================
       2. PANEL DANH SÁCH & BỘ LỌC TÌM KIẾM
       ======================================================== */
    .member-list-panel {
        display: flex;
        flex-direction: column;
        min-height: 0;
        min-width: 0;
        background: #ffffff;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        overflow: hidden;
    }

    .panel-header {
        padding: 10px 12px;
        font-size: 13px;
        font-weight: 700;
        color: #1e3a8a;
        background: #f1f5f9;
        border-bottom: 1px solid #e2e8f0;
        flex-shrink: 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }
    .panel-header.company-header {
        color: #9d174d;
        background: #fdf2f8;
        border-bottom-color: #fbcfe8;
    }

    .panel-filter {
        padding: 10px 10px 0 10px;
        flex-shrink: 0;
        background: #ffffff;
        border-bottom: 1px solid #f1f5f9;
    }

    .panel-list {
        flex: 1 1 0;
        min-height: 0;
        min-width: 0;
        overflow-y: auto;
        overflow-x: hidden;
        box-sizing: border-box;
        padding: 10px;
        background: #f8fafc;
    }
    .panel-list > div { display: block; min-height: 0; }
    .panel-list::-webkit-scrollbar { width: 6px; }
    .panel-list::-webkit-scrollbar-track { background: transparent; }
    .panel-list::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
    .panel-list::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

    /* ========================================================
       3. ROW NHÂN VIÊN VÀ LỊCH BIỂU (GIỮ NGUYÊN BẢN TỐT NHẤT)
       ======================================================== */
    .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 6px; margin-bottom: 6px; overflow: hidden; display: flex; flex-direction: column; align-items: stretch; }
    .member-item-row:last-child { margin-bottom: 0; }
    .member-item-row.show-schedule { border-color: #93c5fd; box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08); }
    .row-default-view { display: flex; align-items: center; justify-content: space-between; padding: 6px 10px; width: 100%; min-height: 46px; box-sizing: border-box; font-size: 13px; }
    .member-info-group { display: flex; align-items: center; gap: 10px; min-width: 0; }
    .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.2; }
    .member-email { font-size: 11px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .member-email:empty { display: none; }
    .member-info-group input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: #2563eb; }
    
    .btn-calendar-only { background: #ffffff; border: 1px solid #e2e8f0; width: 28px; height: 28px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 13px; transition: all 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); }
    .btn-calendar-only:hover { background: #eff6ff; border-color: #93c5fd; transform: scale(1.1); }
    .member-item-row.show-schedule .btn-calendar-only { background: #eff6ff; border-color: #2563eb; }
    .single-avatar-circle { width: 26px; height: 26px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }

    /* TRẢ LẠI LỊCH BỂU GỐC: RỘNG RÃI VÀ CHIỀU CAO CHUẨN */
    .row-schedule-panel { max-height: 0; overflow: hidden; border-top: 1px solid transparent; transition: max-height 0.35s ease, border-color 0.35s ease; }
    .member-item-row.show-schedule .row-schedule-panel { max-height: 460px; border-top-color: #e2e8f0; } 
    .row-schedule-inner { padding: 8px 12px 12px; }
    .row-schedule-panel, .row-schedule-inner, .mini-cal-wrap, .mini-cal { width: 100% !important; box-sizing: border-box; }
    
    /* VỀ LẠI 310PX GỐC */
    .mc-viewport { position: relative; overflow: hidden; height: 310px; width: 100% !important; }
    .mc-weekdays, .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100% !important; }

    .mini-cal-wrap { flex: 1; min-width: 0; display: flex; flex-direction: column; }
    .mini-cal { flex: 1; min-width: 0; user-select: none; }
    .mc-header { display: flex; align-items: center; justify-content: space-between; padding: 0 2px 4px 4px; }
    .mc-title { font-size: 14px; font-weight: 700; color: #1e293b; }
    .mc-nav { display: flex; gap: 2px; }
    .mc-nav-btn { width: 30px; height: 26px; border: 0; background: transparent; border-radius: 6px; color: #64748b; font-size: 10px; line-height: 1; cursor: pointer; transition: background 0.15s, color 0.15s; }
    .mc-nav-btn:hover { background: #eff6ff; color: #2563eb; }
    .mc-nav-btn:focus-visible { outline: 2px solid #2563eb; outline-offset: 1px; }
    .mc-weekdays span { text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
    .mc-weekdays span.mc-we { color: #94a3b8; }
    
    /* VỀ LẠI 50PX CHO MỖI DÒNG LỊCH GỐC */
    .mc-grid { grid-auto-rows: 50px; gap: 2px; }
    .mc-viewport .mc-grid { position: absolute; top: 0; left: 0; right: 0; will-change: transform, opacity; }
    
    .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
    .mc-day .mc-num { font-weight: 600; line-height: 1; }
    .mc-day.out-month .mc-num { color: #94a3b8; }
    .mc-day.out-range { opacity: 0.32; }
    .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; }
    .mc-day.st-holiday { background: #fef3c7; color: #b45309; }
    .mc-day.st-weekend { background: #f1f5f9; color: #64748b; }
    .mc-day.st-free    { background: #e6f4ea; color: #137333; }
    .mc-day.today { border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }
    
    .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
   /* HIGHLIGHT NHÂN VIÊN GỐC CỦA CÔNG VIỆC */
    .member-item-row.original-member {
        background-color: #f0f8ff; 
        border-color: #bfe0ff;     
    }
    .member-item-row.original-member:hover {
        background-color: #e6f3ff;
    }
    
    /* [BỔ SUNG BƯỚC 1]: ÉP NỀN TRONG SUỐT CHO VÙNG CHỨA LỊCH ĐỂ TRÀN MÀU XANH NHẠT */
    .member-item-row.original-member .row-schedule-panel,
    .member-item-row.original-member .row-schedule-inner,
    .member-item-row.original-member .mini-cal-wrap,
    .member-item-row.original-member .mini-cal,
    .member-item-row.original-member .mc-header,
    .member-item-row.original-member .mc-weekdays,
    .member-item-row.original-member .mc-viewport {
        background-color: transparent !important;
    }

    .member-item-row.original-member .btn-calendar-only {
        background-color: #ffffff;
        border-color: #bfe0ff;
    }
    .member-item-row.original-member .btn-calendar-only:hover,
    .member-item-row.original-member.show-schedule .btn-calendar-only {
        background-color: #dbeafe;
        border-color: #3b82f6;
    }
    /* TOOLTIP CHI TIẾT CÔNG VIỆC TRONG MINI CALENDAR*/
    .mc-day { position: relative !important; overflow: visible !important; }
    .mc-day.has-tooltip { cursor: pointer; }
    .mc-day:hover, .mc-day.show-tooltip { z-index: 50; }
    
    .custom-task-tooltip { 
        position: absolute !important; 
        bottom: calc(100% + 5px); left: 50%; transform: translateX(-50%); 
        margin-bottom: 8px; z-index: 999999 !important; background: #ffffff; color: #334155; 
        border: 1px solid #cbd5e1; border-radius: 10px; padding: 11px 16px; 
        min-width: 340px; 
        max-width: 520px; 
        width: max-content; /* Tự động bung rộng chiều ngang theo nội dung để giảm chiều cao */        
        white-space: normal; word-break: break-word; overflow-wrap: anywhere; 
        box-shadow: 0 10px 40px rgba(15, 23, 42, 0.2); font-size: 12.5px; line-height: 1.5; 
        opacity: 0; visibility: hidden; pointer-events: none; transition: opacity 0.2s ease, visibility 0.2s ease; 
    }
    .custom-task-tooltip::after { content: ''; position: absolute; top: 100%; left: 50%; margin-left: -6px; border-width: 6px; border-style: solid; border-color: #ffffff transparent transparent transparent; }
    .mc-day:hover .custom-task-tooltip, .mc-day.show-tooltip .custom-task-tooltip { opacity: 1; visibility: visible; pointer-events: auto; }
    
    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    
    .t-project-name { font-size: 10.5px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 3px; font-weight: 600; color: #64748b; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }
    
    /* Thuật toán chống tràn lề trái phải cho các ngày đầu/cuối tuần */
    .mc-day:nth-child(7n + 1) .custom-task-tooltip,
    .mc-day:nth-child(7n + 2) .custom-task-tooltip { left: -10px !important; transform: none !important; }
    .mc-day:nth-child(7n + 1) .custom-task-tooltip::after,
    .mc-day:nth-child(7n + 2) .custom-task-tooltip::after { left: 24px !important; margin-left: 0 !important; }
    .mc-day:nth-child(7n + 6) .custom-task-tooltip,
    .mc-day:nth-child(7n) .custom-task-tooltip { left: auto !important; right: -10px !important; transform: none !important; }
    .mc-day:nth-child(7n + 6) .custom-task-tooltip::after,
    .mc-day:nth-child(7n) .custom-task-tooltip::after { left: auto !important; right: 24px !important; margin-left: 0 !important; }
    .member-item-row.schedule-opened,
    .member-item-row.schedule-opened .row-schedule-panel,
    .member-item-row.schedule-opened .mc-viewport {
        overflow: visible !important;
        z-index: 50; 
    }
    .member-item-row.schedule-opened .mc-viewport.is-animating {
        overflow: hidden !important;
    }
    .mc-day:nth-child(-n+14) .custom-task-tooltip { 
        bottom: auto !important; 
        top: calc(100% + 5px) !important; 
        margin-bottom: 0; 
        margin-top: 8px; 
    }   
    .mc-day:nth-child(-n+14) .custom-task-tooltip::after { 
        top: auto !important; 
        bottom: 100% !important; 
        border-color: transparent transparent #ffffff transparent; 
    }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlTaskMemberPicker" Type="Primary" Size="ExtraLarge">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlMemberPicker" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                
                <div class="js-validation validationEngineContainer p-2 task-modal-body">

                    <!-- HEADER CỐ ĐỊNH -->
                    <div class="task-modal-header-info">
                        <div class="info-row-custom" style="background: #eff6ff; padding: 8px 12px; border-radius: 6px; border: 1px solid #bfdbfe; margin-bottom: 8px;">
                            <asp:Literal runat="server" ID="ltrTaskInfoNote"></asp:Literal>
                        </div>
                        <div style="font-size: 12px; color: #9a3412; background: #fff7ed; padding: 8px 12px; border-radius: 6px; border: 1px solid #fdba74;">
                            <i class="fas fa-exclamation-triangle me-1"></i> <%= GetResourceText(BackEndResourceKeys.AUTO_ADD_MEMBER_WARNING_MSG) %>
                        </div>
                    </div>

                    <!-- GRID 2 CỘT TỰ CO GIÃN -->
                    <div class="task-member-columns">

                        <!-- CỘT 1: NHÂN SỰ DỰ ÁN -->
                        <div class="member-list-panel" id="panelProject">
                            <div class="panel-header">
                                <span>📁 <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %></span>
                                <span class="badge bg-primary rounded-pill">
                                    <asp:UpdatePanel ID="upCountProj" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                                        <ContentTemplate><asp:Literal ID="ltrCountProj" runat="server">0</asp:Literal></ContentTemplate>
                                    </asp:UpdatePanel>
                                </span>
                            </div>
                            
                            <!-- BỘ LỌC ĐƯỢC TÁCH RỜI NHƯ BÊN CHỌN NHÂN VIÊN GỐC -->
                            <div class="panel-filter">
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <asp:UpdatePanel runat="server" ID="upnlSearchProj" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Panel runat="server" ID="pnlSearchProj">
                                                <SweetSoft:BootstrapDropdown ID="ddlChucDanhProj" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlChucDanhProj_SelectedValueChanged">
                                                </SweetSoft:BootstrapDropdown>
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    
                                    <asp:Panel runat="server" DefaultButton="lbtSearchProj" CssClass="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchProj" CssClass="border-primary input-search-filter" PlaceHolder="Nhập từ khóa tìm kiếm..."></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchProj" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchProj_ServerClick"></SweetSoft:ExtraButton>
                                    </asp:Panel>
                                </div>
                                <div class="listSearchTagBox">
                                    <asp:UpdatePanel ID="upSearchTagProj" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <SweetSoft:ExtraSearchBox ID="searchTagBoxProj" runat="server" OnTagClosed="searchTagBoxProj_TagClosed"></SweetSoft:ExtraSearchBox>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                            
                            <!-- VÙNG CÓ THANH CUỘN -->
                            <div class="panel-list">
                                <asp:UpdatePanel ID="upListProj" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Repeater ID="rptProjectMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                                            <ItemTemplate>
                                                <div class='member-item-row <%# Convert.ToBoolean(Eval("IsOriginal")) ? "original-member" : "" %>' id='mem-row-<%# Eval("UserId") %>'>
                                                    <div class="row-default-view">
                                                        <!-- Cụm thông tin bên trái (Thêm flex-grow-1 để nó đẩy phần lịch sang phải) -->
                                                        <div class="member-info-group flex-grow-1">
                                                            <asp:CheckBox runat="server" ID="chkSelect" />
                                                            <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                                            <%# Eval("AvatarHtml") %>
                                                            <div class="member-name-block">
                                                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                                                    <span class="fw-bold text-dark me-1"><%#: Eval("DisplayName") %></span>
                                                                    <%# Convert.ToBoolean(Eval("IsPM")) ? "<span class='badge bg-danger' style='font-size: 9.5px; padding: 2px 5px;'>PM</span>" : "" %>
                                                                    <%# Convert.ToBoolean(Eval("IsInactive")) ? "<span class='badge bg-secondary' style='font-size: 9.5px; padding: 2px 5px;'>Đã nghỉ</span>" : "" %>
                                                                </div>
                                                                <span class="member-email"><%#: Eval("Email") %></span>
                                                            </div>
                                                        </div>
    
                                                       <!-- Cụm cảnh báo & Lịch biểu nằm dạt sang phải -->
                                                        <div class="d-flex align-items-center gap-2 flex-shrink-0">
        
                                                            <%# Convert.ToBoolean(Eval("IsOriginal")) ? 
                                                                "<span class='badge px-2 py-1' style='font-size: 11.5px; background-color: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd;'><i class='fas fa-user-check me-1'></i>Đã phân công</span>" 
                                                                : (Convert.ToInt32(Eval("BusyDays")) > 0 ? 
                                                                    string.Format("<span class='badge bg-warning text-dark px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-briefcase me-1'></i>Bận {0} ngày</span>", Eval("BusyDays")) 
                                                                    : "<span class='badge bg-success px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-check-circle me-1'></i>Trống lịch</span>") %>
        
                                                            <button type="button" class="btn-calendar-only" title="Xem lịch" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>')">📅</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                                                    <div class="row-schedule-panel" id='overlay-<%# Eval("UserId") %>'>
                                                        <div class="row-schedule-inner">
                                                            <div class="mini-cal-wrap">
                                                                <div class="mini-cal" id='timeline-<%# Eval("UserId") %>'></div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <asp:Panel ID="pnlNoDataProj" runat="server" CssClass="member-empty" Visible="false">
                                            <asp:Literal ID="ltrNoDataProj" runat="server"></asp:Literal>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>

                        <!-- CỘT 2: NHÂN SỰ KHÁC TRONG CÔNG TY -->
                        <div class="member-list-panel" id="panelCompany">
                            <div class="panel-header company-header">
                                <span>🏢 <%= GetResourceText(BackEndResourceKeys.OTHER_EMPLOYEES) %></span>
                                <span class="badge bg-danger rounded-pill">
                                    <asp:UpdatePanel ID="upCountCompany" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                                        <ContentTemplate><asp:Literal ID="ltrCountCompany" runat="server">0</asp:Literal></ContentTemplate>
                                    </asp:UpdatePanel>
                                </span>
                            </div>
                            
                            <!-- BỘ LỌC ĐƯỢC TÁCH RỜI NHƯ BÊN CHỌN NHÂN VIÊN GỐC -->
                            <div class="panel-filter">
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <asp:UpdatePanel runat="server" ID="upnlSearchCompany" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Panel runat="server" ID="pnlSearchCompany">
                                                <SweetSoft:BootstrapDropdown ID="ddlChucDanhCompany" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlChucDanhCompany_SelectedValueChanged">
                                                </SweetSoft:BootstrapDropdown>
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    
                                    <asp:Panel runat="server" DefaultButton="lbtSearchCompany" CssClass="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchCompany" CssClass="border-primary input-search-filter" PlaceHolder="Nhập từ khóa tìm kiếm..."></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchCompany" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchCompany_ServerClick"></SweetSoft:ExtraButton>
                                    </asp:Panel>
                                </div>
                                <div class="listSearchTagBox">
                                    <asp:UpdatePanel ID="upSearchTagCompany" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <SweetSoft:ExtraSearchBox ID="searchTagBoxCompany" runat="server" OnTagClosed="searchTagBoxCompany_TagClosed"></SweetSoft:ExtraSearchBox>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                            
                            <!-- VÙNG CÓ THANH CUỘN -->
                            <div class="panel-list">
                                <asp:UpdatePanel ID="upListCompany" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Repeater ID="rptCompanyMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                                            <ItemTemplate>
                                                <div class="member-item-row" id='mem-row-<%# Eval("UserId") %>'>
                                                    <div class="row-default-view">
                                                        <!-- Cụm thông tin bên trái (Thêm flex-grow-1 để nó đẩy phần lịch sang phải) -->
                                                        <div class="member-info-group flex-grow-1">
                                                            <asp:CheckBox runat="server" ID="chkSelect" />
                                                            <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                                            <%# Eval("AvatarHtml") %>
                                                            <div class="member-name-block">
                                                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                                                    <span class="fw-bold text-dark me-1"><%#: Eval("DisplayName") %></span>
                                                                    <%# Convert.ToBoolean(Eval("IsPM")) ? "<span class='badge bg-danger' style='font-size: 9.5px; padding: 2px 5px;'>PM</span>" : "" %>
                                                                    <%# Convert.ToBoolean(Eval("IsInactive")) ? "<span class='badge bg-secondary' style='font-size: 9.5px; padding: 2px 5px;'>Đã nghỉ</span>" : "" %>
                                                                </div>
                                                                <span class="member-email"><%#: Eval("Email") %></span>
                                                            </div>
                                                        </div>
    
                                                       <!-- Cụm cảnh báo & Lịch biểu nằm dạt sang phải -->
                                                        <div class="d-flex align-items-center gap-2 flex-shrink-0">
        
                                                            <%# Convert.ToBoolean(Eval("IsOriginal")) ? 
                                                                "<span class='badge px-2 py-1' style='font-size: 11.5px; background-color: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd;'><i class='fas fa-user-check me-1'></i>Đã phân công</span>" 
                                                                : (Convert.ToInt32(Eval("BusyDays")) > 0 ? 
                                                                    string.Format("<span class='badge bg-warning text-dark px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-briefcase me-1'></i>Bận {0} ngày</span>", Eval("BusyDays")) 
                                                                    : "<span class='badge bg-success px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-check-circle me-1'></i>Trống lịch</span>") %>
        
                                                            <button type="button" class="btn-calendar-only" title="Xem lịch" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>')">📅</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                                                    <div class="row-schedule-panel" id='overlay-<%# Eval("UserId") %>'>
                                                        <div class="row-schedule-inner">
                                                            <div class="mini-cal-wrap">
                                                                <div class="mini-cal" id='timeline-<%# Eval("UserId") %>'></div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <asp:Panel ID="pnlNoDataCompany" runat="server" CssClass="member-empty" Visible="false">
                                            <asp:Literal ID="ltrNoDataCompany" runat="server"></asp:Literal>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>

                    </div> <!-- End Grid -->
                </div> <!-- End Modal Body -->

            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel ID="upnlTaskMemberFooter" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraButton runat="server" ID="btnConfirmTaskAssign" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClick="btnConfirmTaskAssign_Click">
                    <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>   
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};

    $(document).ready(function () {
        var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

        function pad(n) { return n < 10 ? '0' + n : '' + n; }
        function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
        function parseKey(k) { var p = k.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }
        function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }
        function titleText(st) { return 'Tháng ' + (st.month + 1) + ', ' + st.year; }

        function buildGridHtml(st) {
            var now = new Date();
            var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
            var offset = (new Date(st.year, st.month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-grid">';

            for (var i = 0; i < 42; i++) {
                var d = new Date(st.year, st.month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!st.minKey && key >= st.minKey && key <= st.maxKey;
                var info = inRange ? st.data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== st.month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                if (key === todayKey) cls += ' today';

                var extra = '';
                var tooltipHtml = '';
                var clickAttr = '';

                if (info) {
                    var label = String(info.text || '').replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    if (label) extra = '<span class="mc-label">' + escAttr(label) + '</span>';

                    // [VẼ TOOLTIP]: Quét mảng tasks trong JSON và dựng giao diện Tooltip Đa Dự Án
                    var hasTasks = (info.status === "busy" && info.tasks && info.tasks.length > 0);
                    if (hasTasks) {
                        cls += ' has-tooltip';
                        clickAttr = ' onclick="CMSMasterJs.PinTooltip(this, event)"'; // Cho phép click để Ghim Tooltip

                        tooltipHtml = '<div class="custom-task-tooltip"><ul class="tooltip-task-list">';
                        for (var t = 0; t < info.tasks.length; t++) {
                            // Ghép icon khối lập phương và Tên Dự Án lên trên Mã và Tên Task
                            tooltipHtml += '<li><div class="t-project-name"><i class="fas fa-cube me-1"></i>' + escAttr(info.tasks[t].projectName) + '</div><span class="t-code">[' + escAttr(info.tasks[t].code) + ']</span>' + escAttr(info.tasks[t].name) + '</li>';
                        }
                        tooltipHtml += '</ul></div>';
                    } else if (info.text) {
                        clickAttr = ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"';
                    }
                }

                html += '<div class="' + cls + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + tooltipHtml + '</div>';
            }
            return html + '</div>';
        }
        // Bật tính năng Click để ghim Tooltip (PM dễ dàng bôi đen copy Tên Task)
        CMSMasterJs.PinTooltip = function (element, event) {
            event.stopPropagation();
            var isPinned = $(element).hasClass('show-tooltip');
            $('.mc-day').removeClass('show-tooltip');
            if (!isPinned) {
                $(element).addClass('show-tooltip');
            }
        };

        // Bấm ra khoảng không bất kỳ sẽ tự tắt Tooltip
        $(document).on('click', function () {
            $('.mc-day').removeClass('show-tooltip');
        });
        function renderMiniCalendar(cal) {
            var st = cal.data('mc');
            var html = '<div class="mc-header">' +
                '<span class="mc-title">' + titleText(st) + '</span>' +
                '<div class="mc-nav">' +
                '<button type="button" class="mc-nav-btn" title="Tháng trước" onclick="CMSMasterJs.ChangeScheduleMonth(this, -1)">&#9650;</button>' +
                '<button type="button" class="mc-nav-btn" title="Tháng sau" onclick="CMSMasterJs.ChangeScheduleMonth(this, 1)">&#9660;</button>' +
                '</div></div>';

            html += '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div>';

            html += '<div class="mc-viewport">' + buildGridHtml(st) + '</div>';
            cal.html(html);
        }

        // BẢN NÂNG CẤP: CHỜ TRƯỢT XONG MỚI BỎ VÁCH NGĂN
        CMSMasterJs.ToggleRowSchedule = function (btnElement, userId, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');
            var currentPanel = rowEl.closest('.member-list-panel');

            if (typeof isShow !== 'boolean') isShow = !rowEl.hasClass('show-schedule');

            if (isShow) {
                // Tắt các lịch cũ và đóng luôn vách ngăn của tụi nó
                currentPanel.find('.member-item-row.show-schedule').not(rowEl).removeClass('show-schedule schedule-opened');

                var jsonString = rowEl.find('input[type="hidden"][id*="hdfScheduleJson"]').val();
                var cal = rowEl.find('#timeline-' + userId);
                var data = {};

                if (jsonString) {
                    try {
                        data = JSON.parse(jsonString) || {};
                    } catch (e) {
                        console.error("Lỗi parse JSON lịch biểu: ", e);
                    }
                }

                var keys = Object.keys(data).sort();
                var minKey = keys.length ? keys[0] : null;
                var maxKey = keys.length ? keys[keys.length - 1] : null;

                var now = new Date();
                var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
                var baseDate = (minKey && todayKey >= minKey && todayKey <= maxKey) ? now
                    : (minKey ? parseKey(minKey) : now);

                cal.data('mc', {
                    data: data,
                    minKey: minKey,
                    maxKey: maxKey,
                    year: baseDate.getFullYear(),
                    month: baseDate.getMonth()
                });
                renderMiniCalendar(cal);

                // Bắt đầu kéo rèm xuống
                rowEl.addClass('show-schedule');

                // Đợi 380ms cho rèm kéo xong -> Mở khóa Vách ngăn để Tooltip đâm thủng trần
                setTimeout(function () {
                    if (rowEl.hasClass('show-schedule')) {
                        rowEl.addClass('schedule-opened'); // [CHÌA KHÓA NẰM Ở ĐÂY]
                    }
                    if (rowEl.hasClass('show-schedule') && rowEl[0].scrollIntoView) {
                        rowEl[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                }, 380);

            } else {
                rowEl.removeClass('show-schedule schedule-opened');
            }
        };

        // BẢN NÂNG CẤP: KHÓA VÁCH NGĂN LẠI KHI ĐANG TRƯỢT THÁNG
        CMSMasterJs.ChangeScheduleMonth = function (btnElement, delta) {
            var cal = $(btnElement).closest('.mini-cal');
            var st = cal.data('mc');
            if (!st) return;

            var viewport = cal.find('.mc-viewport');

            // [BẬT KHÓA VÁCH NGĂN]: Chống tràn lịch ra ngoài & Xóa Tooltip đang ghim
            viewport.addClass('is-animating');
            $('.mc-day').removeClass('show-tooltip');

            var grids = viewport.children('.mc-grid');
            if (grids.length > 1) {
                grids.not(':last').remove();
                var lastEl = grids.last()[0];
                if (lastEl.getAnimations) lastEl.getAnimations().forEach(function (a) { a.cancel(); });
            }

            var d = new Date(st.year, st.month + delta, 1);
            st.year = d.getFullYear();
            st.month = d.getMonth();
            cal.find('.mc-title').text(titleText(st));

            var oldGrid = viewport.children('.mc-grid').last();
            var newGrid = $(buildGridHtml(st));
            viewport.append(newGrid);

            var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            if (reduceMotion || !newGrid[0].animate) {
                oldGrid.remove();
                viewport.removeClass('is-animating'); // Tháo khóa nếu không có animation
                return;
            }

            var dir = delta > 0 ? 1 : -1;
            var opts = { duration: 260, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'both' };

            oldGrid[0].animate([
                { transform: 'translateY(0)', opacity: 1 },
                { transform: 'translateY(' + (-dir * 40) + '%)', opacity: 0 }
            ], opts).onfinish = function () {
                oldGrid.remove();
            };

            newGrid[0].animate([
                { transform: 'translateY(' + (dir * 40) + '%)', opacity: 0 },
                { transform: 'translateY(0)', opacity: 1 }
            ], opts).onfinish = function () {
                // [THÁO KHÓA VÁCH NGĂN]: Trượt lịch xong thì mở lại để Tooltip ngoi lên
                viewport.removeClass('is-animating');
            };
        };
        // Xử lý Z-Index để MessageBox không bị chìm dưới Popup
        (function () {
            var BASE_Z = 106000;

            function stackBusinessModals() {
                var $modals = $('.modal:visible').not('#modal-notify');
                if ($modals.length > 1) {
                    $('.modal-backdrop').each(function (i) {
                        $(this).css('z-index', BASE_Z + (i * 10));
                    });
                    $modals.each(function (i) {
                        $(this).css('z-index', BASE_Z + (i * 10) + 5);
                    });
                }
            }

            function raiseNotify() {
                var notify = document.getElementById('modal-notify');
                if (!notify || !$(notify).is(':visible')) return;

                var $others = $('.modal:visible').not(notify);
                if ($others.length === 0) return;

                var maxZ = BASE_Z;
                $others.each(function () {
                    maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(this).zIndex, 10) || 0);
                });

                notify.style.setProperty('z-index', String(maxZ + 20), 'important');
                var bds = document.querySelectorAll('.modal-backdrop');
                if (bds.length) bds[bds.length - 1].style.setProperty('z-index', String(maxZ + 15), 'important');
            }

            function raiseNotifyRepeatedly() {
                [0, 100, 250, 500, 900].forEach(function (ms) {
                    setTimeout(function () { stackBusinessModals(); raiseNotify(); }, ms);
                });
            }

            ['show.bs.modal', 'shown.bs.modal'].forEach(function (evt) {
                document.addEventListener(evt, function (e) {
                    if (e.target && e.target.id === 'modal-notify') raiseNotifyRepeatedly();
                });
            });

            document.addEventListener('hidden.bs.modal', function (e) {
                if (e.target && e.target.id === 'modal-notify') {
                    e.target.style.removeProperty('z-index');
                }
            });

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(raiseNotifyRepeatedly);
            }

            // Tự động đóng Popup chọn nhân sự khi bấm "Đồng ý" trên MessageBox
            $(document).on('click', '[id$="btnAccept"]', function () {
                var $pickerModal = $('#<%= mdlTaskMemberPicker.ClientID %>');
            if ($pickerModal.is(':visible')) {
                $pickerModal.modal('hide');
            }
        });
    })();
    });
</script>