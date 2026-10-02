<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewIssueDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.Controls.CtrlViewIssueDetail" %>
<asp:UpdatePanel ID="upIssueView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlIssueView" runat="server" Type="Primary" Title="Chi tiết vấn đề">
            <ContentTemplate>
                <div class="issue-view-wrapper">
                    <div class="row g-3 mb-3">
                        <div class="col-lg-8 d-flex flex-column gap-3">
                            <fieldset class="issue-view-card">
                                <legend class="fieldlegend"><i class="fas fa-info-circle text-danger"></i> Thông tin vấn đề</legend>

                                <div class="issue-info-top">
                                    <div class="issue-view-name-box">
                                        <span class="meta-label">Tên vấn đề</span>
                                        <asp:Label ID="lblIssueName" runat="server" CssClass="issue-view-title"></asp:Label>
                                    </div>

                                    <div class="issue-created-box">
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

                                <div class="issue-info-bottom">
                                    <div class="issue-view-field issue-origin-box">
                                        <span class="issue-view-label">Nguồn gốc</span>
                                        <asp:Label ID="lblOrigin" runat="server" CssClass="issue-view-value"></asp:Label>
                                    </div>

                                    <div class="issue-meta-item meta-impact">
                                        <span class="meta-label">Mức độ ảnh hưởng</span>
                                        <asp:Label ID="lblImpact" runat="server" CssClass="meta-value"></asp:Label>
                                    </div>

                                    <div class="issue-meta-item meta-status">
                                        <span class="meta-label">Trạng thái</span>
                                        <asp:Label ID="lblStatus" runat="server" CssClass="meta-value"></asp:Label>
                                    </div>
                                </div>
                            </fieldset>

                            <fieldset class="issue-view-card">
                                <legend class="fieldlegend"><i class="fas fa-link" style="color: #6d28d9"></i> Công việc liên quan</legend>

                                <div class="issue-view-related-task mb-2">
                                    <span class="issue-view-label">Vấn đề từ công việc</span>
                                    <asp:Literal ID="ltrOriginTask" runat="server"></asp:Literal>
                                </div>

                                <div class="issue-view-related-task">
                                    <span class="issue-view-label">Công việc bị ảnh hưởng</span>
                                    <asp:Literal ID="ltrAffectedTask" runat="server"></asp:Literal>
                                </div>
                            </fieldset>
                        </div>

                        <div class="col-lg-4 d-flex flex-column">
                            <fieldset class="issue-view-card flex-grow-1 issue-view-assignee-card">
                                <legend class="fieldlegend"><i class="fas fa-users"></i> Nhân viên xử lý</legend>

                                <asp:Repeater ID="rptAssignees" runat="server">
                                    <HeaderTemplate><div class="issue-assignee-list"></HeaderTemplate>

                                    <ItemTemplate>
                                        <div class="issue-assignee-row">
                                            <div class="issue-assignee-avatar"><%# Eval("AvatarHtml") %></div>

                                            <div class="issue-assignee-main">
                                                <div class="issue-assignee-name"><%# Eval("DisplayName") %></div>
                                                <div class="issue-assignee-email"><%# Eval("Email") %></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>

                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>

                                <asp:Panel ID="pnlNoAssignees" runat="server" CssClass="issue-empty-assignee" Visible="false">
                                    Chưa có nhân viên xử lý vấn đề này.
                                </asp:Panel>
                            </fieldset>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="issue-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-align-left"></i> Nội dung vấn đề</legend>

                                <div class="issue-view-text-block flex-grow-1">
                                    <asp:Literal ID="ltrDescription" runat="server"></asp:Literal>
                                </div>
                            </fieldset>
                        </div>

                        <div class="col-md-6 d-flex flex-column">
                            <fieldset class="issue-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-list"></i> Kế hoạch xử lý</legend>

                                <div class="issue-view-text-block flex-grow-1">
                                    <asp:Literal ID="ltrPlan" runat="server"></asp:Literal>
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
#<%= mdlIssueView.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1.5rem auto 1rem !important; }
#<%= mdlIssueView.ClientID %> .modal-body { padding: 8px 22px 12px; background: #f8fafc; }
.issue-view-wrapper { padding: 0; }
fieldset.issue-view-card { position: relative; padding: 6px 18px 12px; border: 1px solid #cbd5e1; border-radius: 9px; background: #fff; box-shadow: 0 1px 3px rgba(15, 23, 42, .04); margin: 0; }
legend.fieldlegend { width: auto !important; display: inline-flex; align-items: center; gap: 8px; margin-left: 10px; margin-bottom: 0; padding: 0 8px; background: transparent !important; border-bottom: none; color: #475569; font-size: 13px; font-weight: 800; line-height: 1.2; letter-spacing: .04em; text-transform: uppercase; }
legend.fieldlegend i { width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid #dce7f4; border-radius: 6px; background: #f7faff; color: #64748b; font-size: 12.5px; }
.issue-info-top { display: grid; grid-template-columns: minmax(0, 1fr) 225px; gap: 12px; margin-top: 4px; }
.issue-info-bottom { display: grid; grid-template-columns: 4fr 3fr 3fr; gap: 12px; margin-top: 12px; }
.issue-view-name-box { padding: 8px 16px; background: linear-gradient(to right, #e9d5ff 0%, #ffffff 90%); border: none; border-left: 5px solid #7c3aed; border-radius: 10px; box-shadow: 0 1px 2px rgba(15, 23, 42, 0.03); }
.issue-view-name-box .meta-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 13px; font-weight: 800; text-transform: none; letter-spacing: 0.02em; }
.issue-view-title { display: block; color: #542e88; font-size: 20px; font-weight: 800; line-height: 1.35; word-break: break-word; }

.issue-created-box { height: 90px; min-height: 90px; padding: 7px 10px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; display: flex; flex-direction: column; box-sizing: border-box; overflow: hidden; }
.issue-created-box .meta-label { display: block; margin-bottom: 3px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
.create-user-info { display: flex; align-items: center; gap: 8px; min-width: 0; }
.create-user-info .user-display-avatar { width: 39px; height: 39px; flex: 0 0 39px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid rgba(15, 23, 42, .05); border-radius: 50%; overflow: hidden; color: #fff; background: #2563eb; font-size: 12px; font-weight: 800; object-fit: cover; }
.create-user-main { min-width: 0; flex: 1 1 auto; }
.create-user-name { display: block; color: #1f2937; font-size: 13px; font-weight: 750; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.create-user-email { display: block; margin-top: 2px; color: #94a3b8; font-size: 11px; font-weight: 500; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.create-user-time { display: flex; align-items: center; justify-content: flex-end; gap: 5px; margin-top: auto; padding-top: 5px; border-top: 1px dashed #eef2f7; color: #64748b; font-size: 10.5px; line-height: 1.2; white-space: nowrap; }
.create-user-time i { color: #94a3b8; font-size: 10.5px; }

.issue-meta-item { padding: 8px 12px; border-radius: 8px; border: 1px solid transparent; }
.issue-meta-item .meta-label { display: block; margin-bottom: 2px; font-size: 10px; font-weight: 800; text-transform: uppercase; opacity: 0.8; }
.issue-meta-item .meta-value { display: block; font-size: 13.5px; font-weight: 750; line-height: 1.3; }
.meta-status { background: #ebf8ff; border-color: #bee3f8; color: #3182ce; }
.meta-impact { background: #fff5f5; border-color: #fed7d7; color: #e53e3e; }
.issue-view-field { padding: 8px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; }
.issue-view-label { display: block; margin-bottom: 4px; color: #64748b; font-size: 10.5px; font-weight: 800; text-transform: uppercase; }
.issue-view-value { display: block; color: #1e293b; font-size: 13.5px; font-weight: 700; line-height: 1.35; word-break: break-word; }
.issue-view-related-task { padding: 8px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; color: #1e293b; font-size: 13.5px; font-weight: 600; line-height: 1.4; }
.issue-view-text-block { min-height: 120px; padding: 10px 14px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; color: #334155; font-size: 14px; font-weight: 500; line-height: 1.6; white-space: pre-wrap; word-break: break-word; }

.issue-view-assignee-card { display: flex; flex-direction: column; }
.issue-assignee-list { flex: 1; display: flex; flex-direction: column; }
.issue-assignee-row { display: flex; align-items: center; gap: 10px; min-width: 0; padding: 10px 0; }
.issue-assignee-row + .issue-assignee-row { border-top: 1px solid #eef2f7; }
.issue-assignee-avatar { width: 40px; height: 40px; flex: 0 0 40px; display: flex; align-items: center; justify-content: center; }
.issue-assignee-avatar .issue-person-avatar { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; object-fit: cover; color: #fff; font-size: 12px; font-weight: 800; border: 1px solid rgba(15,23,42,.05); }
.issue-assignee-main { min-width: 0; }
.issue-assignee-name { color: #1f2937; font-size: 13.5px; font-weight: 750; line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.issue-assignee-email { margin-top: 3px; color: #6b7280; font-size: 11.5px; line-height: 1.25; word-break: break-all; }
.issue-empty-assignee { padding: 14px; background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 8px; color: #64748b; font-size: 13px; font-weight: 500; text-align: center; }

@media (max-width: 991.98px) {
    #<%= mdlIssueView.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
    .issue-info-top { grid-template-columns: minmax(0, 1fr) 225px; }
    .issue-created-box { height: 90px; min-height: 90px; }
}

@media (max-width: 767.98px) {
    #<%= mdlIssueView.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: 1rem auto !important; }
    .issue-info-top, .issue-info-bottom { grid-template-columns: 1fr; gap: 10px; }
    .issue-created-box { height: 90px; min-height: 90px; }
    .issue-view-title { font-size: 18px; }
    .gap-4 { gap: 1rem !important; }
}
</style>