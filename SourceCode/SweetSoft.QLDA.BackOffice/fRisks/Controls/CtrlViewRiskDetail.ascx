<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewRiskDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.Controls.CtrlViewRiskDetail" %>
<asp:UpdatePanel ID="upRiskView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlRiskView" runat="server" Type="Primary" Title="Chi tiết rủi ro">
            <ContentTemplate>
                
                <div class="risk-view-wrapper">
                    <!-- VÙNG 1: TRÊN -->
                    <div class="row g-3 mb-4 mt-2">
                        
                        <!-- Cột Trái -->
                        <div class="col-lg-8 d-flex flex-column gap-4">
                            
                            <!-- Card 1: Thông tin rủi ro -->
                            <fieldset class="risk-view-card">
                                <legend class="fieldlegend"><i class="fas fa-info-circle text-danger"></i> Thông tin rủi ro</legend>
                                <div class="d-flex flex-column flex-md-row gap-4 align-items-md-start mt-1">
                                    
                                    <div class="flex-grow-1 d-flex flex-column gap-3 mt-1">
                                        <asp:Label ID="lblRiskName" runat="server" CssClass="risk-view-title"></asp:Label>
                                        <div class="risk-view-field">
                                            <span class="risk-view-label"><i class="fas fa-calculator text-primary"></i> Mức độ rủi ro</span>
                                            <asp:Label ID="lblRiskLevel" runat="server" CssClass="risk-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    
                                    <div class="risk-view-meta-box">
                                        <div class="d-flex flex-column gap-2">
                                            <div class="risk-meta-item meta-status">
                                                <span class="meta-label">Xác suất xảy ra</span>
                                                <asp:Label ID="lblProbability" runat="server" CssClass="meta-value"></asp:Label>
                                            </div>
                                            <div class="risk-meta-item meta-impact">
                                                <span class="meta-label">Mức độ ảnh hưởng</span>
                                                <asp:Label ID="lblImpact" runat="server" CssClass="meta-value"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    
                                </div>
                            </fieldset>

                            <!-- Card 2: Khởi tạo -->
                            <fieldset class="risk-view-card">
                                <legend class="fieldlegend"><i class="fas fa-clock"></i> Khởi tạo</legend>
                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <div class="risk-view-field">
                                            <span class="risk-view-label">Người tạo</span>
                                            <asp:Label ID="lblCreatedBy" runat="server" CssClass="risk-view-value"></asp:Label>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="risk-view-field">
                                            <span class="risk-view-label">Ngày tạo</span>
                                            <asp:Label ID="lblCreatedDate" runat="server" CssClass="risk-view-value"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                            
                        </div>

                        <!-- Cột Phải: Nhân viên xử lý -->
                        <div class="col-lg-4 d-flex flex-column">
                            <fieldset class="risk-view-card flex-grow-1 risk-view-assignee-card">
                                <legend class="fieldlegend"><i class="fas fa-user-shield"></i> Nhân viên xử lý</legend>
                                <asp:Repeater ID="rptAssignees" runat="server">
                                    <HeaderTemplate><div class="risk-assignee-list"></HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="risk-assignee-row">
                                            <div class="risk-assignee-avatar"><%# Eval("AvatarHtml") %></div>
                                            <div class="risk-assignee-main">
                                                <div class="risk-assignee-name"><%# Eval("DisplayName") %></div>
                                                <div class="risk-assignee-email"><%# Eval("Email") %></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoAssignees" runat="server" CssClass="risk-empty-assignee" Visible="false">
                                    Chưa có nhân viên theo dõi rủi ro này.
                                </asp:Panel>
                            </fieldset>
                        </div>
                    </div>
                    
                    <!-- VÙNG 2: DƯỚI -->
                    <div class="row g-3">
                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="risk-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-shield-alt"></i> Kế hoạch phòng ngừa</legend>
                                <div class="risk-view-text-block flex-grow-1"><asp:Literal ID="ltrKeHoachPhongNgua" runat="server"></asp:Literal></div>
                            </fieldset>
                        </div>
                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="risk-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-fire-extinguisher"></i> Kế hoạch ứng phó</legend>
                                <div class="risk-view-text-block flex-grow-1"><asp:Literal ID="ltrKeHoachUngPho" runat="server"></asp:Literal></div>
                            </fieldset>
                        </div>
                    </div>
                </div>
                
            </ContentTemplate>
            <FooterTemplate>
                <asp:LinkButton ID="btnCloseRiskView" runat="server" CssClass="btn btn-light" CausesValidation="false" OnClientClick="$(this).closest('.modal').modal('hide'); return false;">
                    <i class="fas fa-times me-1"></i> Đóng
                </asp:LinkButton>
            </FooterTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    #<%= mdlRiskView.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1rem auto; }
    #<%= mdlRiskView.ClientID %> .modal-body { padding: 12px 22px 18px; background: #f8fafc; }
    
    .risk-view-wrapper { padding: 0; }
    
    fieldset.risk-view-card { 
        position: relative; 
        padding: 6px 18px 16px; 
        border: 1px solid #cbd5e1; 
        border-radius: 9px; 
        background: #fff; 
        box-shadow: 0 1px 3px rgba(15, 23, 42, .04); 
        margin: 0;
    }
    
    legend.fieldlegend {
        width: auto !important; 
        display: inline-flex;
        align-items: center;
        gap: 8px;
        margin-left: 10px; 
        margin-bottom: 0;
        padding: 0 8px; 
        background: transparent !important; 
        border-bottom: none; 
        color: #475569;
        font-size: 13px;
        font-weight: 800;
        line-height: 1.2;
        letter-spacing: .04em;
        text-transform: uppercase;
    }
    
    legend.fieldlegend i {
        width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center;
        border: 1px solid #dce7f4; border-radius: 6px; background: #f7faff; color: #64748b; font-size: 12.5px;
    }
    
    .risk-view-title { display: block; color: #542e88; font-size: 20px; font-weight: 800; line-height: 1.4; word-break: break-word; }
    
    .risk-view-meta-box { flex: 0 0 240px; } 
    .risk-meta-item { padding: 8px 12px; border-radius: 8px; border: 1px solid transparent; }
    .risk-meta-item .meta-label { display: block; margin-bottom: 2px; font-size: 10px; font-weight: 800; text-transform: uppercase; opacity: 0.8; }
    .risk-meta-item .meta-value { display: block; font-size: 13.5px; font-weight: 750; line-height: 1.3; }
    
    .meta-impact { background: #fff5f5; border-color: #fed7d7; color: #e53e3e; } 
    .meta-status { background: #ebf8ff; border-color: #bee3f8; color: #3182ce; } 
    
    .risk-view-field { padding: 9px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; }
    .risk-view-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
    .risk-view-value { display: block; color: #1e293b; font-size: 13.5px; font-weight: 700; line-height: 1.35; word-break: break-word; }
    
    .risk-view-text-block { min-height: 130px; padding: 12px 14px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; color: #334155; font-size: 14px; font-weight: 500; line-height: 1.6; white-space: pre-wrap; word-break: break-word; }
    
    .risk-view-assignee-card { display: flex; flex-direction: column; }
    .risk-assignee-list { flex: 1; display: flex; flex-direction: column; }
    .risk-assignee-row { display: flex; align-items: center; gap: 10px; min-width: 0; padding: 10px 0; }
    .risk-assignee-row + .risk-assignee-row { border-top: 1px solid #eef2f7; }
    .risk-assignee-avatar { width: 40px; height: 40px; flex: 0 0 40px; }
    .risk-assignee-avatar .risk-person-avatar { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; object-fit: cover; color: #fff; font-size: 12px; font-weight: 800; border: 1px solid rgba(15,23,42,.05); }
    .risk-assignee-main { min-width: 0; }
    .risk-assignee-name { color: #1f2937; font-size: 13.5px; font-weight: 750; line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .risk-assignee-email { margin-top: 3px; color: #6b7280; font-size: 11.5px; line-height: 1.25; word-break: break-all; }
    .risk-empty-assignee { padding: 14px; background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 8px; color: #64748b; font-size: 13px; font-weight: 500; text-align: center; }
    
    @media (max-width: 991.98px) {
        #<%= mdlRiskView.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
        .risk-view-meta-box { flex: 0 0 280px; }
    }
    @media (max-width: 767.98px) {
        #<%= mdlRiskView.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: .5rem auto; }
        .risk-view-meta-box { flex: 1 1 auto; width: 100%; }
        .risk-view-title { font-size: 18px; }
        .gap-4 { gap: 1rem !important; }
    }
</style>