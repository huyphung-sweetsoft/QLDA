<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastCompleteIssue.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.Controls.CtrlFastCompleteIssue" %>
<style>
.fc-task-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 16px; margin-bottom: 16px; }
.fc-date-wrapper { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
.dynamic-status { font-weight: 700; padding: 8px 14px; border-radius: 6px; font-size: 14px; white-space: nowrap; transition: all 0.3s ease; border: 1px solid transparent; }
.status-ontime { background-color: #dcfce7; color: #14532d; border-color: #4ade80; }
.status-late { background-color: #fee2e2; color: #b91c1c; border-color: #f87171; }
.fc-ref-date { font-size: 13px; color: #64748b; font-weight: 600; margin-bottom: 8px; display: block; }
.fc-ref-date strong { color: #0f172a; }
.fc-info-note { padding: 12px 14px; border: 1px solid #bfdbfe; background: #eff6ff; color: #1e40af; border-radius: 7px; font-size: 13px; line-height: 1.6; }
.fc-issue-notice{font-size:12.5px;color:#4d647f;line-height:1.6;margin-bottom:12px}
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlFastComplete" Type="Primary" Title="Gửi yêu cầu hoàn thành vấn đề">
  <ContentTemplate>
    <asp:UpdatePanel ID="upnlFastComplete" runat="server" UpdateMode="Conditional"><ContentTemplate>
      <asp:HiddenField ID="hdfCompleteIssueId" runat="server" />
      <div class="p-2">
        <h5 class="text-primary mb-3 text-center"><i class="fas fa-check-double me-2"></i> Gửi yêu cầu hoàn thành vấn đề</h5>
        <div class="fc-task-box text-center"><p class="text-muted mb-2" style="font-size:12.5px">Bạn đang gửi yêu cầu hoàn thành vấn đề:</p><p class="fw-bold text-dark fs-6 mb-0"><asp:Literal ID="ltrIssueName" runat="server" /></p></div>
        <div class="fc-info-note mb-3"><i class="fas fa-info-circle me-1"></i> Bạn cần báo hoàn thành phần việc cá nhân trước. Gửi duyệt sẽ dừng tất cả đồng hồ đang chạy trực tiếp trên vấn đề, giữ nguyên giờ đã ghi, thông báo người chưa báo giờ và gửi yêu cầu cho PM. Duyệt hoàn thành không đồng nghĩa chốt giờ công.</div>
        <asp:Panel ID="pnlWarning" runat="server" CssClass="alert alert-warning mb-3" Visible="false"><asp:Literal ID="ltrWarning" runat="server" /></asp:Panel>
        <label class="form-label fw-bold" for="<%= txtGhiChu.ClientID %>">Ghi chú xử lý <span class="text-muted fw-normal">(không bắt buộc)</span></label>
        <asp:TextBox ID="txtGhiChu" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="2000" placeholder="Nội dung đã xử lý hoặc ghi chú gửi quản lý..." />
      </div>
    </ContentTemplate></asp:UpdatePanel>
  </ContentTemplate>
  <FooterTemplate><asp:UpdatePanel ID="upFooterComplete" runat="server" UpdateMode="Conditional"><ContentTemplate>
    <asp:LinkButton ID="btnConfirmComplete" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnConfirmComplete_Click"><i class="fas fa-paper-plane me-1"></i> Gửi PM duyệt</asp:LinkButton>
  </ContentTemplate></asp:UpdatePanel></FooterTemplate>
</SweetSoft:ExtraModal>
