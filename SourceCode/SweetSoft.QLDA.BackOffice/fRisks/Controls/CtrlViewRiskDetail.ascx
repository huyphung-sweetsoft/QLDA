<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewRiskDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.Controls.CtrlViewRiskDetail" %>
<asp:UpdatePanel ID="upRiskView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlRiskView" runat="server" Type="Primary" Title="Chi tiết rủi ro">
            <ContentTemplate>
                <div class="risk-view-wrapper">
                    <div class="row g-3 mb-3 mt-0">
                        <div class="col-lg-8 d-flex flex-column gap-3">
                            <fieldset class="risk-view-card">
                                <legend class="fieldlegend"><i class="fas fa-info-circle text-danger"></i> Thông tin rủi ro</legend>

                                <div class="risk-info-top">
                                    <div class="risk-view-name-box">
                                        <span class="meta-label">Tên rủi ro</span>
                                        <asp:Label ID="lblRiskName" runat="server" CssClass="risk-view-title"></asp:Label>
                                    </div>

                                    <div class="risk-created-box">
                                        <span class="meta-label">Khởi tạo</span>
                                        <div class="create-user-info">
                                            <asp:Literal ID="ltrCreatedAvatar" runat="server"></asp:Literal>

                                            <div class="create-user-main">
                                                <asp:Label ID="lblCreatedBy" runat="server" CssClass="create-user-name"></asp:Label>
                                                <asp:Label ID="lblCreatedEmail" runat="server" CssClass="create-user-email"></asp:Label>
                                            </div>
                                        </div>

                                        <div class="create-user-time">
                                            <i class="far fa-calendar-alt"></i>
                                            <asp:Label ID="lblCreatedDate" runat="server"></asp:Label>
                                        </div>
                                    </div>
                                </div>

                                <div class="risk-info-bottom">
                                    <div class="risk-meta-item meta-status">
                                        <span class="meta-label">Xác suất xảy ra</span>
                                        <asp:Label ID="lblProbability" runat="server" CssClass="meta-value value-lg"></asp:Label>
                                    </div>

                                    <div class="risk-meta-item meta-impact">
                                        <span class="meta-label">Mức độ ảnh hưởng</span>
                                        <asp:Label ID="lblImpact" runat="server" CssClass="meta-value value-lg"></asp:Label>
                                    </div>

                                    <div class="risk-view-field">
                                        <span class="risk-view-label"><i class="fas fa-calculator text-primary"></i> Mức độ rủi ro</span>
                                        <asp:Label ID="lblRiskLevel" runat="server" CssClass="risk-view-value value-lg text-danger"></asp:Label>
                                    </div>
                                </div>
                            </fieldset>
                        </div>

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

                    <div class="row g-3">
                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="risk-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-shield-alt"></i> Kế hoạch phòng ngừa</legend>

                                <div class="risk-view-text-block flex-grow-1">
                                    <asp:Literal ID="ltrKeHoachPhongNgua" runat="server"></asp:Literal>
                                </div>
                            </fieldset>
                        </div>

                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="risk-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-fire-extinguisher"></i> Kế hoạch ứng phó</legend>

                                <div class="risk-view-text-block flex-grow-1">
                                    <asp:Literal ID="ltrKeHoachUngPho" runat="server"></asp:Literal>
                                </div>
                            </fieldset>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    #<%= mdlRiskView.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1.5rem auto 1rem !important; }
    #<%= mdlRiskView.ClientID %> .modal-body { padding: 8px 22px 12px; background: #f8fafc; }
    .risk-view-wrapper { padding: 0; }
    fieldset.risk-view-card { position: relative; padding: 6px 18px 12px; border: 1px solid #cbd5e1; border-radius: 9px; background: #fff; box-shadow: 0 1px 3px rgba(15, 23, 42, .04); margin: 0; }
    legend.fieldlegend { width: auto !important; display: inline-flex; align-items: center; gap: 8px; margin-left: 10px; margin-bottom: 0; padding: 0 8px; background: transparent !important; border-bottom: none; color: #475569; font-size: 13px; font-weight: 800; line-height: 1.2; letter-spacing: .04em; text-transform: none; }
    legend.fieldlegend i { width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid #dce7f4; border-radius: 6px; background: #f7faff; color: #64748b; font-size: 12.5px; }
    .risk-info-top { display: grid; grid-template-columns: minmax(0, 1fr) 225px; gap: 12px; margin-top: 4px; }
    .risk-info-bottom { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; margin-top: 12px; }
    .risk-view-name-box { padding: 8px 16px; background: linear-gradient(to right, #e9d5ff 0%, #ffffff 90%); border: none; border-left: 5px solid #7c3aed; border-radius: 10px; box-shadow: 0 1px 2px rgba(15, 23, 42, 0.03); }
    .risk-view-name-box .meta-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 13px; font-weight: 800; text-transform: none; letter-spacing: 0.02em; }
    .risk-view-title { display: block; color: #542e88; font-size: 20px; font-weight: 800; line-height: 1.35; word-break: break-word; }

    .risk-created-box { height: 90px; min-height: 90px; padding: 7px 10px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; display: flex; flex-direction: column; box-sizing: border-box; overflow: hidden; }
    .risk-created-box .meta-label { display: block; margin-bottom: 3px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
    .create-user-info { display: flex; align-items: center; gap: 8px; min-width: 0; }
    .create-user-info .user-display-avatar { width: 39px; height: 39px; flex: 0 0 39px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid rgba(15, 23, 42, .05); border-radius: 50%; overflow: hidden; color: #fff; background: #2563eb; font-size: 12px; font-weight: 800; object-fit: cover; }
    .create-user-main { min-width: 0; flex: 1 1 auto; }
    .create-user-name { display: block; color: #1f2937; font-size: 13px; font-weight: 750; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .create-user-email { display: block; margin-top: 2px; color: #94a3b8; font-size: 11px; font-weight: 500; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .create-user-time { display: flex; align-items: center; justify-content: flex-end; gap: 5px; margin-top: auto; padding-top: 5px; border-top: 1px dashed #eef2f7; color: #64748b; font-size: 10.5px; line-height: 1.2; white-space: nowrap; }
    .create-user-time i { color: #94a3b8; font-size: 10.5px; }

    .risk-meta-item { padding: 8px 12px; border-radius: 8px; border: 1px solid transparent; display: flex; flex-direction: column; justify-content: center; }
    .risk-meta-item .meta-label { display: block; margin-bottom: 2px; font-size: 10px; font-weight: 800; text-transform: uppercase; opacity: 0.8; }
    .risk-meta-item .meta-value { display: block; font-weight: 750; line-height: 1.3; }
    .meta-status, .meta-impact { background: #fff8e7; border-color: #f6d58a; color: #a16207; }
    .risk-info-bottom > .risk-view-field { padding: 8px 12px; border-radius: 8px; border: 1px solid #fda4af; background: #fff1f2; color: #b91c1c; }
    .risk-info-bottom > .risk-view-field .risk-view-label { color: #b91c1c; }
    .risk-info-bottom > .risk-view-field .risk-view-label i { color: #4f46e5 !important; }
    .risk-info-bottom > .risk-view-field .risk-view-value { color: #b91c1c !important; }
    .risk-view-field { padding: 8px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; }
    .risk-info-bottom .risk-view-field { display: flex; flex-direction: column; justify-content: center; }
    .risk-view-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
    .risk-view-label i { margin-right: 4px; }
    .risk-view-value { display: block; color: #1e293b; font-size: 13.5px; font-weight: 700; line-height: 1.35; word-break: break-word; }
    .value-lg { font-size: 17px !important; }

    .risk-view-text-block { min-height: 120px; padding: 10px 14px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; color: #334155; font-size: 14px; font-weight: 500; line-height: 1.6; white-space: pre-wrap; word-break: break-word; }
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
        .risk-info-top { grid-template-columns: minmax(0, 1fr) 225px; }
        .risk-created-box { height: 90px; min-height: 90px; }
    }

    @media (max-width: 767.98px) {
        #<%= mdlRiskView.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: .5rem auto; }
        .risk-info-top, .risk-info-bottom { grid-template-columns: 1fr; gap: 10px; }
        .risk-created-box { height: 90px; min-height: 90px; }
        .risk-view-title { font-size: 18px; }
        .gap-4 { gap: 1rem !important; }
    }
</style>