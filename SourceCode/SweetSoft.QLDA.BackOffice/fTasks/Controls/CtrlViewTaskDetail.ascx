<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewTaskDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlViewTaskDetail" %>
<asp:UpdatePanel ID="upTaskView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlTaskView" runat="server" Type="Primary" Title="Chi tiết công việc">
            <ContentTemplate>
                
                <div class="task-view-wrapper">
                    <div class="row g-3 mb-4 mt-2">
                        
                        <div class="col-lg-8 d-flex flex-column gap-4">
                            
                            <fieldset class="task-view-card">
                                <legend class="fieldlegend"><i class="fas fa-info-circle text-primary"></i> Thông tin công việc</legend>
                                <div class="d-flex flex-column flex-md-row gap-4 align-items-md-start mt-1">
                                    
                                    <div class="flex-grow-1 d-flex flex-column mt-1">
                                        <!-- Gộp Mã và Tên thành 1 dòng to -->
                                        <asp:Label ID="lblTaskName" runat="server" CssClass="task-view-title mb-2"></asp:Label>
                                        <!-- Dòng Giai đoạn dự án -->
                                        <div class="text-muted fw-bold" style="font-size: 13px;">
                                            <i class="fas fa-layer-group me-1"></i> Thuộc giai đoạn: <asp:Label ID="lblPhaseName" runat="server" CssClass="text-dark"></asp:Label>
                                        </div>
                                    </div>
                                    
                                    <div class="task-view-meta-box">
                                        <div class="d-flex flex-column gap-2">
                                            <div class="task-meta-item meta-status">
                                                <span class="meta-label">Trạng thái</span>
                                                <asp:Label ID="lblStatus" runat="server" CssClass="meta-value"></asp:Label>
                                            </div>
                                            <div class="task-meta-item meta-priority">
                                                <span class="meta-label">Độ ưu tiên</span>
                                                <asp:Label ID="lblPriority" runat="server" CssClass="meta-value"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    
                                </div>
                            </fieldset>

                            <fieldset class="task-view-card">
                                <legend class="fieldlegend"><i class="fas fa-calendar-alt text-success"></i> Thời gian & Tiến độ</legend>
                                <div class="row g-3">
                                    <!-- HÀNG 1 -->
                                    <div class="col-md-6">
                                        <div class="task-view-field h-100">
                                            <span class="task-view-label">Ngày bắt đầu dự kiến</span>
                                            <asp:Label ID="lblStartDate" runat="server" CssClass="task-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="task-view-field h-100">
                                            <span class="task-view-label">Thời lượng (Ngày)</span>
                                            <asp:Label ID="lblDuration" runat="server" CssClass="task-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    <!-- HÀNG 2 -->
                                    <div class="col-md-6">
                                        <div class="task-view-field h-100">
                                            <span class="task-view-label">Ngày kết thúc dự kiến</span>
                                            <asp:Label ID="lblEndDate" runat="server" CssClass="task-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="task-view-field h-100">
                                            <span class="task-view-label">Ngày hoàn thành thực tế</span>
                                            <asp:Label ID="lblActualEndDate" runat="server" CssClass="task-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    <!-- HÀNG 3 (Lý do trễ hạn - Ẩn/Hiện tự động) -->
                                    <div class="col-12" id="divDelayReason" runat="server" visible="false">
                                        <div class="task-view-field" style="background-color: #fef2f2; border-color: #fecaca;">
                                            <span class="task-view-label text-danger">Lý do trễ hạn</span>
                                            <asp:Label ID="lblDelayReason" runat="server" CssClass="task-view-value text-danger"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                            
                        </div>

                        <div class="col-lg-4 d-flex flex-column">
                            <fieldset class="task-view-card flex-grow-1 task-view-assignee-card">
                                <legend class="fieldlegend"><i class="fas fa-users text-info"></i> Nhân viên thực hiện</legend>
                                <asp:Repeater ID="rptAssignees" runat="server">
                                    <HeaderTemplate><div class="task-assignee-list"></HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="task-assignee-row">
                                            <div class="task-assignee-avatar"><%# Eval("AvatarHtml") %></div>
                                            <div class="task-assignee-main">
                                                <div class="task-assignee-name"><%# Eval("DisplayName") %></div>
                                                <div class="task-assignee-email"><%# Eval("Email") %></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoAssignees" runat="server" CssClass="task-empty-assignee" Visible="false">
                                    Chưa phân công nhân viên.
                                </asp:Panel>
                            </fieldset>
                        </div>
                    </div>
                    
                    <div class="row g-3">
                        <div class="col-12 d-flex flex-column">
                            <fieldset class="task-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-align-left text-secondary"></i> Mô tả công việc</legend>
                                <div class="task-view-text-block flex-grow-1"><asp:Literal ID="ltrDescription" runat="server"></asp:Literal></div>
                            </fieldset>
                        </div>
                    </div>
                </div>
                
            </ContentTemplate>
            <FooterTemplate>
                <asp:LinkButton ID="btnCloseTaskView" runat="server" CssClass="btn btn-light" CausesValidation="false" OnClientClick="$(this).closest('.modal').modal('hide'); return false;">
                    <i class="fas fa-times me-1"></i> Đóng
                </asp:LinkButton>
            </FooterTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    #<%= mdlTaskView.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1rem auto; }
    #<%= mdlTaskView.ClientID %> .modal-body { padding: 12px 22px 18px; background: #f8fafc; }
    
    .task-view-wrapper { padding: 0; }
    
    fieldset.task-view-card { 
        position: relative; padding: 6px 18px 16px; border: 1px solid #cbd5e1; 
        border-radius: 9px; background: #fff; box-shadow: 0 1px 3px rgba(15, 23, 42, .04); margin: 0;
    }
    
    legend.fieldlegend {
        width: auto !important; display: inline-flex; align-items: center; gap: 8px;
        margin-left: 10px; margin-bottom: 0; padding: 0 8px; background: transparent !important; 
        border-bottom: none; color: #475569; font-size: 13px; font-weight: 800;
        line-height: 1.2; letter-spacing: .04em; text-transform: uppercase;
    }
    
    legend.fieldlegend i {
        width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center;
        border: 1px solid #dce7f4; border-radius: 6px; background: #f7faff; color: #64748b; font-size: 12.5px;
    }
    
    .task-view-title { display: block; color: #0f172a; font-size: 20px; font-weight: 800; line-height: 1.4; word-break: break-word; }
    
    .task-view-meta-box { flex: 0 0 240px; } 
    .task-meta-item { padding: 8px 12px; border-radius: 8px; border: 1px solid transparent; }
    .task-meta-item .meta-label { display: block; margin-bottom: 2px; font-size: 10px; font-weight: 800; text-transform: uppercase; opacity: 0.8; }
    .task-meta-item .meta-value { display: block; font-size: 13.5px; font-weight: 750; line-height: 1.3; }
    
    .meta-priority { background: #fffbeb; border-color: #fde68a; color: #b45309; } 
    .meta-status { background: #ebf8ff; border-color: #bee3f8; color: #3182ce; } 
    
    .task-view-field { padding: 9px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; }
    .task-view-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
    .task-view-value { display: block; color: #1e293b; font-size: 13.5px; font-weight: 700; line-height: 1.35; word-break: break-word; }
    
    .task-view-text-block { min-height: 90px; padding: 12px 14px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; color: #334155; font-size: 14px; font-weight: 500; line-height: 1.6; white-space: pre-wrap; word-break: break-word; }
    
    .task-view-assignee-card { display: flex; flex-direction: column; }
    .task-assignee-list { flex: 1; display: flex; flex-direction: column; }
    .task-assignee-row { display: flex; align-items: center; gap: 10px; min-width: 0; padding: 10px 0; }
    .task-assignee-row + .task-assignee-row { border-top: 1px solid #eef2f7; }
    .task-assignee-avatar { width: 40px; height: 40px; flex: 0 0 40px; }
    .task-assignee-avatar .task-person-avatar { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; object-fit: cover; color: #fff; font-size: 12px; font-weight: 800; border: 1px solid rgba(15,23,42,.05); }
    .task-assignee-main { min-width: 0; }
    .task-assignee-name { color: #1f2937; font-size: 13.5px; font-weight: 750; line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .task-assignee-email { margin-top: 3px; color: #6b7280; font-size: 11.5px; line-height: 1.25; word-break: break-all; }
    .task-empty-assignee { padding: 14px; background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 8px; color: #64748b; font-size: 13px; font-weight: 500; text-align: center; }
    
    @media (max-width: 991.98px) {
        #<%= mdlTaskView.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
        .task-view-meta-box { flex: 0 0 280px; }
    }
    @media (max-width: 767.98px) {
        #<%= mdlTaskView.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: .5rem auto; }
        .task-view-meta-box { flex: 1 1 auto; width: 100%; }
        .task-view-title { font-size: 18px; }
    }
</style>