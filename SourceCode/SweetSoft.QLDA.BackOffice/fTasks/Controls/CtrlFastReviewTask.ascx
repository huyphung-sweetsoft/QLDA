<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastReviewTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlFastReviewTask" %>

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
    @media (max-width: 575.98px) { .fr-info-row { gap: 10px; } .fr-info-label { flex-basis: 40%; } }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlFastReview" Type="Primary" Title="Duyệt nhanh hoàn thành công việc" DefaultButton="btnApproveCompletion">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlReviewComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfReviewTaskId" runat="server" />

                <div class="p-2">
                    <div class="fr-task-box text-center">
                        <div class="text-muted mb-2" style="font-size: 12.5px;">Yêu cầu hoàn thành đang chờ PM duyệt</div>
                        <div class="fw-bold text-dark"><asp:Literal ID="ltrReviewTaskName" runat="server" /></div>
                    </div>

                    <div class="fr-info-box">
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="far fa-calendar-alt me-1"></i> Ngày hoàn thành dự kiến</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrExpectedEndDate" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="far fa-calendar-check me-1"></i> Ngày hoàn thành thực tế</div>
                            <div class="fr-info-value text-primary"><asp:Literal ID="ltrActualDate" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="fas fa-chart-line me-1"></i> Kết quả theo thời hạn</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrReviewResult" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="fas fa-pen-alt me-1"></i> Lý do trễ hạn</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrLateReason" runat="server" /></div>
                        </div>
                    </div>

                    <div class="fr-note">
                        <i class="fas fa-info-circle me-1"></i>
                        Kiểm tra kết quả thực tế trước khi duyệt. Chọn <strong>Duyệt hoàn thành</strong> nếu đạt yêu cầu; chọn <strong>Từ chối</strong> nếu cần người thực hiện tiếp tục chỉnh sửa. Khi từ chối, ngày hoàn thành thực tế sẽ được xóa và hạn dự kiến vẫn được giữ nguyên.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlReviewFooter" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2 flex-wrap">
                    <asp:LinkButton ID="btnRejectCompletion" runat="server" CausesValidation="false" CssClass="btn fr-btn-reject" OnClick="btnRejectCompletion_Click">
                        <i class="fas fa-undo me-1"></i>Từ chối
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnApproveCompletion" runat="server" CssClass="btn btn-primary px-4" OnClick="btnApproveCompletion_Click">
                        <i class="fas fa-check-circle me-1"></i>Duyệt hoàn thành
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>
