<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastReviewIssue.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.Controls.CtrlFastReviewIssue" %>
<style>
.fr-task-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 16px; margin-bottom: 14px; }
.fr-info-box { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 8px 16px; margin-bottom: 14px; }
.fr-info-row { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; padding: 11px 0; border-bottom: 1px solid #edf2f7; }
.fr-info-row:last-child { border-bottom: 0; }
.fr-info-label { flex: 0 0 43%; color: #64748b; font-size: 13px; font-weight: 600; }
.fr-info-value { flex: 1; min-width: 0; color: #1e293b; font-size: 13px; font-weight: 600; text-align: right; overflow-wrap: anywhere; white-space: pre-line; }
.fr-badge { display: inline-block; border-radius: 5px; padding: 5px 9px; font-size: 12px; font-weight: 700; }
.fr-badge-ontime { background: #dcfce7; color: #166534; border: 1px solid #86efac; }
.fr-badge-late { background: #fee2e2; color: #b91c1c; border: 1px solid #fca5a5; }
.fr-note { background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 7px; color: #1e40af; padding: 11px 13px; font-size: 13px; line-height: 1.6; }
.fr-btn-reject { color: #b91c1c !important; background: #fff !important; border: 1px solid #fca5a5 !important; }
.fr-btn-reject:hover { background: #fef2f2 !important; border-color: #dc2626 !important; color: #991b1b !important; }
.fr-reject-client-error { display:block; margin-top:6px; color:#b91c1c; font-size:12px; }
.fr-decision-loading-overlay { position:fixed; inset:0; z-index:2147483646; display:flex; align-items:center; justify-content:center; padding:20px; background:rgba(248,250,252,.78); backdrop-filter:blur(2px); }
.fr-decision-loading-panel { width:100%; max-width:310px; padding:24px 22px; border:1px solid #e2e8f0; border-radius:14px; background:#fff; box-shadow:0 12px 35px rgba(15,23,42,.14); text-align:center; font-family:Arial,sans-serif; }
.fr-decision-spinner { width:36px; height:36px; margin:0 auto 12px; border:4px solid #e9e1f8; border-top-color:#6f42c1; border-radius:50%; animation:frDecisionSpin .7s linear infinite; }
@keyframes frDecisionSpin { to { transform:rotate(360deg); }
}
.fr-decision-busy { opacity:.72 !important; pointer-events:none !important; }
@media (max-width: 575.98px) { .fr-info-row { gap: 10px; }
.fr-info-label { flex-basis: 40%; }
}
.fr-result-hours { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:10px; margin-bottom:12px; }
.fr-result-amount { display:flex; flex-direction:column; gap:9px; border:1px solid #e6dcfa; border-radius:10px; background:#fff; padding:13px 10px; color:#635482; }
.fr-result-amount span { font-size:12px; font-weight:650; }
.fr-result-amount strong { color:#54218d; font-size:20px; line-height:1.2; font-variant-numeric:tabular-nums; }
.fr-result-note { line-height:1.7; overflow-wrap:anywhere; }
.fr-result-legacy { background:#fff4e3; border:1px solid #f6d49f; border-radius:8px; padding:11px; margin-bottom:11px; color:#854d0e; font-size:12.5px; }
@media (max-width:575px) { .fr-result-hours { grid-template-columns:1fr; } }

.modal-dialog:has(.fr-pm-final-layout) { width:calc(100vw - 50px)!important; max-width:1400px!important; }
.modal-dialog:has(.fr-pm-final-layout) .modal-body { max-height:calc(100vh - 175px); overflow-y:auto; }
/* Ket qua PM Issue: cung cau truc 3 vung + lich su cua popup Task, khong dung cards 3 so tu ve. */
.fr-pm-final-layout .fr-info-box { display:none; }
.fr-pm-final-layout .fr-pm-result-body { display:grid; grid-template-columns:minmax(0,1.18fr) minmax(280px,.82fr); gap:14px; min-height:0; }
.fr-pm-final-layout .fr-pm-left { display:flex; flex-direction:column; gap:12px; min-width:0; }
.fr-pm-final-layout .fr-pm-cards { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:11px; }
.fr-pm-final-layout .fr-pm-card { display:flex; flex-direction:column; gap:8px; padding:13px; border:1px solid #e3d9ed; border-radius:10px; background:#fcfaff; min-width:0; }
.fr-pm-final-layout .fr-pm-cards .fr-pm-card:nth-child(2) { background:#f9f5ff; border-color:#ddceee; }
.fr-pm-final-layout .fr-pm-card h4 { margin:0; color:#51317c; font-size:13px; font-weight:800; line-height:1.4; }
.fr-pm-final-layout .fr-pm-caption { margin:0; color:#778096; font-size:10.8px; line-height:1.45; min-height:28px; }
.fr-pm-final-layout .fr-pm-hours { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:8px; margin-top:auto; }
.fr-pm-final-layout .fr-pm-hours>div { display:flex; flex-direction:column; gap:4px; min-width:0; }
.fr-pm-final-layout .fr-pm-hours span { font-size:10.5px; color:#718096; }
.fr-pm-final-layout .fr-pm-hours strong { font:750 16px/1.3 Consolas,'Courier New',monospace; color:#48257f; white-space:nowrap; font-variant-numeric:tabular-nums; }
.fr-pm-final-layout .fr-pm-note { padding:10px 12px; background:#f8f4ff; border:1px solid #e7daf5; border-radius:9px; color:#614884; font-size:11.5px; line-height:1.6; }
.fr-pm-final-layout .fr-pm-note strong { display:block; color:#543078; margin-bottom:3px; }
.fr-pm-final-layout .fr-pm-note p { margin:0; }
.fr-pm-final-layout .fr-pm-note.is-warning { background:#fff8eb; border-color:#f3daae; color:#98601a; }
.fr-pm-final-layout .fr-pm-history { display:flex; flex-direction:column; gap:8px; min-height:220px; height:100%; padding:12px; border:1px solid #e4dcec; border-radius:10px; background:#faf7ff; }
.fr-pm-final-layout .fr-pm-history-head { display:flex; align-items:center; justify-content:space-between; gap:8px; padding-bottom:7px; border-bottom:1px solid #e9dff3; }
.fr-pm-final-layout .fr-pm-history-head strong { color:#542d88; font-size:12px; }
.fr-pm-final-layout .fr-pm-history-head span { color:#7f87a0; font-size:10px; }
.fr-pm-final-layout .fr-pm-history-list { display:flex; flex-direction:column; gap:9px; max-height:335px; overflow-y:auto; scrollbar-width:thin; }
.fr-pm-final-layout .fr-pm-history-item { display:flex; flex-direction:column; gap:6px; border:1px solid #e7deef; border-radius:8px; background:#fff; padding:10px 11px; font-size:11px; color:#687287; }
.fr-pm-final-layout .fr-pm-history-item-head { display:flex; justify-content:space-between; gap:8px; }
.fr-pm-final-layout .fr-pm-history-item-head strong { color:#593584; font-weight:750; }
.fr-pm-final-layout .fr-pm-history-item-head time { color:#8290a5; font-size:10px; }
.fr-pm-final-layout .fr-pm-history-item p { margin:0; overflow-wrap:anywhere; }
@media(max-width:780px) { .fr-pm-final-layout .fr-pm-result-body { grid-template-columns:1fr; } .fr-pm-final-layout .fr-pm-history { min-height:145px; } .fr-pm-final-layout .fr-pm-history-list { max-height:200px; } }
@media(max-width:520px) { .fr-pm-final-layout .fr-pm-cards { grid-template-columns:1fr; } }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlFastReview" Type="Primary" Title="Duyệt nhanh hoàn thành vấn đề">
  <ContentTemplate><asp:UpdatePanel ID="upnlReviewComplete" runat="server" UpdateMode="Conditional"><ContentTemplate>
    <asp:HiddenField ID="hdfReviewIssueId" runat="server" />
    <asp:Panel ID="pnlReviewForm" runat="server">
    <div class="p-2">
      <div class="fr-task-box text-center"><div class="text-muted mb-2" style="font-size:12.5px">Yêu cầu hoàn thành vấn đề đang chờ PM duyệt</div><div class="fw-bold text-dark"><asp:Literal ID="ltrReviewIssueName" runat="server" /></div></div>
      <div class="fr-info-box">
        <div class="fr-info-row"><div class="fr-info-label"><i class="fas fa-user me-1"></i> Nhân viên gửi duyệt</div><div class="fr-info-value text-primary"><asp:Literal ID="ltrSender" runat="server" /></div></div>
        <div class="fr-info-row"><div class="fr-info-label"><i class="far fa-calendar-check me-1"></i> Thời gian gửi duyệt</div><div class="fr-info-value"><asp:Literal ID="ltrSentAt" runat="server" /></div></div>
        <div class="fr-info-row"><div class="fr-info-label"><i class="fas fa-users me-1"></i> Tình trạng nhân viên</div><div class="fr-info-value"><asp:Literal ID="ltrWorkers" runat="server" /></div></div>
        <div class="fr-info-row"><div class="fr-info-label"><i class="fas fa-stopwatch me-1"></i> Tổng giờ hệ thống</div><div class="fr-info-value"><asp:Literal ID="ltrTotalTime" runat="server" /></div></div>
      </div>
      <div class="fr-note"><i class="fas fa-info-circle me-1"></i> Kiểm tra kết quả xử lý trước khi duyệt hoặc từ chối. <strong>Phê duyệt hoàn thành vấn đề không tự động chốt giờ công.</strong> Nhân viên chưa báo giờ vẫn có thể được PM nhắc và xác nhận sau.</div>
      <div class="fr-reject-reason mt-3"><label class="form-label fw-semibold" for="<%= txtRejectReason.ClientID %>" style="font-size:13px;color:#334155">Lý do từ chối <span class="text-danger">(bắt buộc khi từ chối)</span></label>
      <asp:TextBox ID="txtRejectReason" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" CssClass="form-control" placeholder="Nêu rõ lý do cần tiếp tục xử lý vấn đề..." />
      <div class="form-text">Lý do được gửi tới những người được phân công hoặc đã có phiên xử lý.</div></div>
    </div>
    </asp:Panel>
    <asp:Panel ID="pnlNotificationResult" runat="server" Visible="false">
      <div class="p-2">
        <div class="fr-task-box text-center"><strong><asp:Literal ID="ltrResultIssue" runat="server" /></strong></div>
        <div class="fr-info-box"><div class="fr-info-row"><span class="fr-info-label">Kết quả</span><strong class="fr-info-value"><asp:Literal ID="ltrResultTitle" runat="server" /></strong></div>
          <div class="fr-info-row"><span class="fr-info-label">Thời điểm thông báo</span><strong class="fr-info-value"><asp:Literal ID="ltrResultWhen" runat="server" /></strong></div>
        </div>
        <div class="fr-note"><asp:Literal ID="ltrResultBody" runat="server" /></div>
      </div>
    </asp:Panel>
  </ContentTemplate></asp:UpdatePanel></ContentTemplate>
  <FooterTemplate><asp:UpdatePanel ID="upnlReviewFooter" runat="server" UpdateMode="Conditional"><ContentTemplate>
    <asp:LinkButton ID="btnRejectCompletion" runat="server" CssClass="btn fr-btn-reject" CausesValidation="false" OnClick="btnRejectCompletion_Click"><i class="fas fa-times-circle me-1"></i> Từ chối</asp:LinkButton>
    <asp:LinkButton ID="btnApproveCompletion" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnApproveCompletion_Click"><i class="fas fa-check-circle me-1"></i> Duyệt hoàn thành</asp:LinkButton>
  </ContentTemplate></asp:UpdatePanel></FooterTemplate>
</SweetSoft:ExtraModal>
