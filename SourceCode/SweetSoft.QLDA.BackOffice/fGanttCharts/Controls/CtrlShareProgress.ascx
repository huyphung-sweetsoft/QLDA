<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlShareProgress.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls.CtrlShareProgress" %>
<%@ Import Namespace="System.Web" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
        <SweetSoft:ExtraModal runat="server" ID="modalSelection" Type="Primary" Title="Tạo và chia sẻ báo cáo tiến độ">
            <ContentTemplate>
                <%-- Giữ control để tương thích code-behind; không hiển thị dòng giới thiệu. --%>
                <asp:Label ID="lblPresetStatus" runat="server" CssClass="share-progress-preset-status" />

                <div class="share-progress-report-name-row">
                    <label for="shareProgressReportName"><i class="far fa-file-alt" aria-hidden="true"></i> Tên báo cáo <span class="share-progress-required">*</span></label>
                    <asp:Literal ID="litReportNameInput" runat="server" />
                    <small>Mặc định: Tên dự án · ddMMyy-HHmm. Có thể sửa tên trước khi tạo.</small>
                </div>

                <div class="share-progress-list-header">
                    <span><i class="fas fa-list-ul me-1"></i> DANH SÁCH CÔNG VIỆC THEO CẤP</span>
                    <div class="share-progress-list-tools">
                        <span class="share-progress-list-hint">Mở mũi tên để chọn cấp sâu hơn</span>
                        <div class="share-progress-preset-actions" role="group" aria-label="Điều chỉnh cấu hình công việc">
                            <button type="button" id="shareProgressUndoClient" class="share-progress-tool-btn is-disabled" aria-disabled="true" data-sp-tooltip="Khôi phục các lựa chọn theo cấu hình đã lưu gần nhất" onclick="return shareProgressUndo(event);"><i class="fas fa-undo-alt" aria-hidden="true"></i> Hoàn tác</button>
                            <button type="button" class="share-progress-tool-btn" data-sp-tooltip="Chọn công việc cấp 1 và 2; bỏ chọn những cấp sâu hơn. Chưa lưu ngay." onclick="return shareProgressRestoreDefaults(event);"><i class="fas fa-layer-group" aria-hidden="true"></i> Mặc định</button>
                            <button type="button" id="shareProgressSavePresetClient" class="share-progress-tool-btn share-progress-tool-save is-disabled" aria-disabled="true" data-sp-tooltip="Lưu các công việc đang chọn cho dự án này" onclick="return shareProgressSavePreset(event,this);"><i class="fas fa-save" aria-hidden="true"></i> Lưu</button>
                        </div>
                    </div>
                </div>
                <div class="share-progress-task-list" id="shareProgressTaskList">
                    <asp:Literal ID="ltrTaskChoices" runat="server" />
                    <asp:PlaceHolder ID="phNoTasks" runat="server" Visible="false">
                        <div class="share-progress-empty">Dự án chưa có công việc để tạo báo cáo.</div>
                    </asp:PlaceHolder>
                </div>

                <asp:Label ID="lblSelectionMessage" runat="server" CssClass="share-progress-validation" EnableViewState="false" />

            </ContentTemplate>
            <FooterTemplate>
                <div class="share-progress-report-footer is-create-only">
                    <button type="button" class="share-progress-primary-btn" onclick="return shareProgressCreateReport(event,this);"><i class="fas fa-file-alt me-1" aria-hidden="true"></i> Tạo báo cáo tiến độ</button>
                </div>
            </FooterTemplate>
        </SweetSoft:ExtraModal>

<asp:UpdatePanel ID="upShareProgress" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <%-- Event targets are inside the UpdatePanel: ASP.NET registers their AJAX lifecycle automatically.
             ExtraModal FooterTemplate may not exist yet during UserControl.OnLoad; do not register those controls manually. --%>
        <asp:LinkButton ID="btnSaveProgressPreset" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnSaveProgressPreset_Click" tabindex="-1" aria-hidden="true">Lưu cấu hình</asp:LinkButton>
        <asp:LinkButton ID="btnCreateProgress" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnCreateProgress_Click" tabindex="-1" aria-hidden="true">Tạo báo cáo</asp:LinkButton>
        <asp:LinkButton ID="btnLoadProgressHistory" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnLoadProgressHistory_Click" tabindex="-1" aria-hidden="true">Nhật ký</asp:LinkButton>
        <asp:HiddenField runat="server" ID="hdfShareProgressOutcome" />
        <asp:HiddenField runat="server" ID="hdfShareProgressErrorText" />

        <SweetSoft:ExtraModal runat="server" ID="modalCreated" Type="Primary" Title="Tạo báo cáo tiến độ thành công">
            <ContentTemplate>
                <div class="share-progress-success-heading">
                    <span class="share-progress-success-icon"><i class="fas fa-check"></i></span>
                    <div>
                        <div class="share-progress-success-title">Báo cáo đã sẵn sàng</div>
                        <div class="share-progress-success-subtitle">Nhân viên nội bộ có quyền được mở nhanh. Khách hàng cần OTP để xem báo cáo.</div>
                    </div>
                </div>

                <div class="share-progress-result-block">
                    <div class="share-progress-result-label"><i class="far fa-file-code"></i> File HTML báo cáo</div>
                    <div class="share-progress-file-row">
                        <asp:Literal ID="ltrReportFileName" runat="server" />
                        <button type="button" class="share-progress-copy-icon" data-copy-tooltip="Sao chép liên kết" aria-label="Sao chép liên kết" onclick="return copyGeneratedProgressLink(this);"><i class="far fa-copy" aria-hidden="true"></i></button>
                    </div>
                    <asp:HyperLink ID="lnkReportFile" runat="server" CssClass="share-progress-report-link" Target="_blank">
                        <i class="fas fa-external-link-alt"></i> Mở file HTML báo cáo
                    </asp:HyperLink>
                </div>

                <div class="share-progress-result-block">
                    <div class="share-progress-result-label"><i class="fas fa-shield-alt"></i> Mã OTP 6 chữ số</div>
                    <div class="share-progress-otp-row">
                        <span class="share-progress-otp-value"><asp:Literal ID="ltrOtp" runat="server" /></span>
                        <asp:HiddenField ID="hdfOtp" runat="server" />
                        <button type="button" class="share-progress-copy-icon" data-copy-tooltip="Sao chép mã OTP" aria-label="Sao chép mã OTP" onclick="return copyGeneratedProgressOtp(this);"><i class="far fa-copy" aria-hidden="true"></i></button>
                    </div>
                    <div class="share-progress-security-note">Chỉ gửi OTP cho khách hàng qua kênh riêng. OTP của báo cáo mới được lưu mã hóa để có thể sử dụng lại trong nhật ký; chỉ chia sẻ với người nhận được phép.</div>
                </div>

                <asp:Label ID="lblCreateMessage" runat="server" CssClass="share-progress-success-message" />

            </ContentTemplate>
            <FooterTemplate>
                <div class="share-progress-created-footer">
                    <button type="button" class="share-progress-primary-btn" onclick="return shareProgressSendCreatedCustomer(event);"><i class="fas fa-paper-plane"></i> Gửi khách hàng</button>
                </div>
            </FooterTemplate>
        </SweetSoft:ExtraModal>

    </ContentTemplate>
</asp:UpdatePanel>

<%-- Dựng modal Nhật ký một lần. Chỉ UpdatePanel bên TRONG được thay thế khi phân trang. --%>
<SweetSoft:ExtraModal runat="server" ID="modalProgressHistory" Type="Primary" Title="Nhật ký báo cáo tiến độ">
    <ContentTemplate>
        <div class="share-progress-history-top">
            <span><i class="fas fa-history"></i> Các báo cáo đã tạo cho dự án hiện tại</span>
            <small>5 báo cáo/trang</small>
        </div>
        <asp:UpdatePanel ID="upShareProgressHistory" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:Literal ID="ltrProgressHistory" runat="server" />
                <div class="share-progress-history-footer">
                    <asp:Label ID="lblProgressHistoryCount" runat="server" />
                    <div class="share-progress-history-nav">
                        <asp:LinkButton ID="btnHistoryPrevious" runat="server" CausesValidation="false" OnClick="btnHistoryNavigate_Click" CommandArgument="prev" CssClass="share-progress-history-page" OnClientClick="return shareProgressHistoryBeforePage(event,this);">‹ Trước</asp:LinkButton>
                        <asp:Label ID="lblProgressHistoryPage" runat="server" />
                        <asp:LinkButton ID="btnHistoryNext" runat="server" CausesValidation="false" OnClick="btnHistoryNavigate_Click" CommandArgument="next" CssClass="share-progress-history-page" OnClientClick="return shareProgressHistoryBeforePage(event,this);">Tiếp ›</asp:LinkButton>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<%-- The history dialog must never be replaced by an OTP-reset partial postback. --%>
<asp:UpdatePanel ID="upShareProgressOtpReset" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:LinkButton ID="btnRequestProgressOtpReset" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnRequestProgressOtpReset_Click" tabindex="-1" aria-hidden="true">Yêu cầu cấp OTP</asp:LinkButton>
        <asp:LinkButton ID="btnResetProgressOtp" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnResetProgressOtp_Click" tabindex="-1" aria-hidden="true">Cấp lại OTP</asp:LinkButton>
        <asp:HiddenField ID="hdfProgressOtpResetRequestId" runat="server" />
        <asp:HiddenField ID="hdfProgressOtpResetStatus" runat="server" />
        <asp:HiddenField ID="hdfProgressOtpResetValue" runat="server" />
        <asp:HiddenField ID="hdfProgressOtpResetMessage" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
<%-- Xóa báo cáo chỉ cập nhật panel nhỏ; không render lại ExtraModal Nhật ký. --%>
<asp:UpdatePanel ID="upShareProgressDelete" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:LinkButton ID="btnRequestProgressDelete" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnRequestProgressDelete_Click" tabindex="-1" aria-hidden="true">Yêu cầu xóa báo cáo</asp:LinkButton>
        <asp:LinkButton ID="btnDeleteProgressReport" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnDeleteProgressReport_Click" tabindex="-1" aria-hidden="true">Xóa báo cáo</asp:LinkButton>
        <asp:HiddenField ID="hdfProgressDeleteRequestId" runat="server" />
        <asp:HiddenField ID="hdfProgressDeleteStatus" runat="server" />
        <asp:HiddenField ID="hdfProgressDeleteMessage" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
                <div id="shareProgressInlineLoader" class="share-progress-loading-overlay" style="display:none;" aria-live="polite" aria-busy="true">
                    <div class="share-progress-loading-card">
                        <div class="share-progress-spinner"></div>
                        <div id="shareProgressLoadingTitle" class="share-progress-loading-title">Đang tạo báo cáo tiến độ</div>
                        <div id="shareProgressLoadingText" class="share-progress-loading-text">Đang dựng biểu đồ Gantt và bảo vệ báo cáo bằng mã OTP. Vui lòng chờ trong giây lát...</div>
                        <div class="share-progress-loading-track"><span></span></div>
                    </div>
                </div>


<style>
.share-progress-report-name-row { display:flex;flex-direction:column;gap:7px;margin:0 0 16px; }
.share-progress-report-name-row label { font-size:12px;font-weight:800;color:#4d276f; }
.share-progress-report-name-row label i { margin-right:6px; }
.share-progress-report-name-row small { color:#8b7c99;font-size:11px; }
.share-progress-required { color:#c43557; }
.share-progress-report-name-input { display:block;width:100%;box-sizing:border-box;padding:10px 12px;border:1px solid #d6c6e7;border-radius:8px;background:#fff;color:#30243a;font-size:13px;outline:0; }
.share-progress-report-name-input:focus { border-color:#8759bf;box-shadow:0 0 0 3px rgba(112,64,158,.12); }
.share-progress-report-footer.is-create-only { justify-content:flex-end; }
.share-progress-preset-status { display:none!important; }
.share-progress-history-top { display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:8px;margin-bottom:12px;color:#563180;font-size:13px;font-weight:750; }
.share-progress-history-top small { color:#8a7b97;font-size:11px;font-weight:500; }
/* Không cố định list 63vh: để modal tự dài đến sát viewport, rồi mới cuộn trong modal-body. */
.share-progress-history-list { display:grid;gap:9px;border:0;max-height:none!important;overflow:visible!important;padding:1px 2px 4px;background:transparent; }
/* CHỈ ĐIỀU CHỈNH KÍCH THƯỚC; KHÔNG ÉP VIỀN, HEADER, FOOTER, NÚT X CỦA EXTRAMODAL. */
div[id$="modalProgressHistory"] .modal-dialog {
    width:min(820px,calc(100vw - 32px))!important;
    max-width:min(820px,calc(100vw - 32px))!important;
}
/* Nội dung tự giãn. Chỉ cuộn khi vượt chiều cao màn hình; không cắt nút X bên ngoài. */
div[id$="modalProgressHistory"] .modal-body {
    max-height:calc(100vh - 130px);
    max-height:calc(100dvh - 130px);
    overflow-y:auto;
    overflow-x:hidden;
    overscroll-behavior-y:contain;
}
/* Không cho phần AJAX phân trang tự kéo giãn ngang. */
div[id$="modalProgressHistory"] [id$="upShareProgressHistory"] { min-width:0; }
.share-progress-history-row { position:relative;border:1px solid #e6d9f1;border-radius:12px;padding:15px 15px 13px;background:#fff;transition:border-color .18s ease,background .18s ease,box-shadow .18s ease;min-width:0; }
.share-progress-history-row:hover { border-color:#cdb7e6;background:#fefcff; }
.share-progress-history-row.is-expanded { background:#fcfaff;border-color:#d7c3ec;box-shadow:0 3px 10px rgba(71,34,115,.035); }
.share-progress-history-core { display:flex;align-items:center;justify-content:space-between;gap:12px; }
.share-progress-history-main { flex:1 1 250px;min-width:0; }
.share-progress-history-name-btn { display:inline-flex;align-items:flex-start;gap:10px;padding:0;border:0;background:transparent;color:#492071;text-align:left;cursor:pointer;font-size:15px;font-weight:800;line-height:1.48;letter-spacing:-.1px;overflow-wrap:anywhere; }
.share-progress-history-name-btn:hover { color:#763bb5; }
.share-progress-history-name-btn:focus-visible { outline:2px solid #8857c0;outline-offset:3px;border-radius:4px; }
.share-progress-history-name-mark { flex:0 0 30px;width:30px;height:30px;display:flex;align-items:center;justify-content:center;border:1px solid #e6d9f3;border-radius:8px;background:#f5eefd;color:#623299;font-size:14px; }
.share-progress-history-name-text { display:block;min-width:0;padding-top:3px; }
.share-progress-history-meta { display:flex;flex-wrap:wrap;gap:6px 14px;margin:8px 0 0 40px;color:#766982;font-size:12px;line-height:1.5; }
.share-progress-history-open { display:inline-flex;align-items:center;justify-content:center;gap:7px;padding:8px 12px;border:1px solid #d9c8ed;border-radius:8px;background:#f8f4fd;color:#56218e;font-weight:750;font-size:11px;text-decoration:none;white-space:nowrap;flex-shrink:0; }
.share-progress-history-open:hover { background:#ede4f9;color:#4e1b82; }
.share-progress-history-open.is-missing { pointer-events:none;color:#9691a0;background:#f3f2f5;border-color:#e7e5e9; }
/* CSS grid 0fr → 1fr: animation tự điều chỉnh chiều cao, không dùng height cố định. */
.share-progress-history-extra {
    display:grid;
    grid-template-rows:0fr;
    opacity:0;
    visibility:hidden;
    pointer-events:none;
    transition:grid-template-rows .26s cubic-bezier(.2,.7,.2,1),opacity .20s ease,visibility .26s;
}
.share-progress-history-extra-inner { min-height:0;overflow:hidden; }
.share-progress-history-share { margin-top:13px;padding-top:12px;border-top:1px dashed #e5d7f1; }
.share-progress-history-row.is-expanded .share-progress-history-extra {
    grid-template-rows:1fr;
    opacity:1;
    visibility:visible;
    pointer-events:auto;
    transition:grid-template-rows .26s cubic-bezier(.2,.7,.2,1),opacity .20s ease,visibility 0s;
}
.share-progress-history-row.is-expanded { transition:border-color .18s ease,background .18s ease,box-shadow .18s ease; }
/* Đổi trang chỉ làm mềm nội dung, không ẩn hoặc dựng lại modal. */
.share-progress-history-page-panel-pending .share-progress-history-list { opacity:.8;transition:opacity .14s ease; }
.share-progress-history-list { animation:shareHistoryPageIn .16s ease-out; }
@keyframes shareHistoryPageIn { from {opacity:.76;transform:translateY(2px)} to {opacity:1;transform:translateY(0)} }
.share-progress-history-share { display:grid;gap:11px; }
/* Cùng cột cho 2 hàng Liên kết / Mã OTP: Nhãn | Giá trị | Sao chép. */
.share-progress-history-share-line { display:flex;align-items:center;gap:8px;flex-wrap:wrap;min-width:0; }
.share-progress-history-share-line>label { box-sizing:border-box;flex:0 0 78px;width:78px;min-width:78px;color:#786789;font-size:11px;font-weight:750;margin:0; }
.share-progress-history-value.is-otp { box-sizing:border-box;flex:0 0 154px;width:154px;min-width:0;border:1px solid #dfd3ee;border-radius:8px;background:#fff;padding:8px 12px;color:#48236f;font-size:13px;font-weight:800;text-align:center;letter-spacing:2px; }
.share-progress-history-link-action { box-sizing:border-box;display:inline-flex;align-items:center;justify-content:center;gap:7px;flex:0 0 154px;width:154px;height:38px;color:#54228b;font-size:12px;font-weight:750;padding:7px 9px;border:1px solid #e2d4f1;border-radius:8px;background:#f8f4fe;text-decoration:none;white-space:nowrap; }
.share-progress-history-link-action:hover { background:#ede3fa;color:#42146f;text-decoration:none; }
.share-progress-history-icon { box-sizing:border-box;display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:38px;border:1px solid #dfd0f0;border-radius:8px;background:#fff;color:#642b9d;padding:7px 10px;font-size:11px;font-weight:750;cursor:pointer;white-space:nowrap; }
/* Nút copy của từng hàng có cùng kích thước, thẳng cột. */
.share-progress-history-share-line:not(.is-otp-line) > .share-progress-history-icon,
.share-progress-history-otp-slot > .share-progress-history-icon { flex:0 0 38px;width:38px;height:38px;padding:0; }
.share-progress-history-icon:hover { background:#f3eafd; }
.share-progress-history-send { margin-left:auto;border-color:#59218d;background:#59218d;color:#fff; }
.share-progress-history-send:hover { background:#44156f;color:#fff; }
.share-progress-history-reissue { color:#8b6481;font-size:11px; }
.share-progress-history-empty { padding:36px 16px;text-align:center;color:#94879c;font-size:13px; }

.share-progress-history-footer { display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:12px;padding:12px 2px 4px;color:#766984;font-size:11px; }
.share-progress-history-nav { display:flex;align-items:center;gap:9px; }
.share-progress-history-page { display:inline-flex;align-items:center;justify-content:center;padding:6px 10px;border:1px solid #e1d4ef;border-radius:7px;color:#653299;background:#fff;text-decoration:none;font-weight:750; }
.share-progress-history-page:hover { background:#f3ecfb; }
.share-progress-history-page.aspNetDisabled { pointer-events:none;opacity:.45; }
@media(max-width:580px) { .share-progress-history-core { align-items:flex-start; } .share-progress-history-open { padding:7px 9px; } .share-progress-history-meta { margin-left:0; } .share-progress-history-send { margin-left:0; } }
@media(prefers-reduced-motion:reduce) {
  .share-progress-history-extra, .share-progress-history-row.is-expanded .share-progress-history-extra { transition:none!important; }
  .share-progress-history-list { animation:none!important; }
}
/* Hàng OTP sử dụng đúng bề rộng vùng giá trị / nút copy của hàng Liên kết. */
.share-progress-history-share-line.is-otp-line { display:flex;align-items:center;flex-wrap:nowrap;gap:8px;min-width:0; }
.share-progress-history-share-line.is-otp-line > label { flex:0 0 78px;width:78px;min-width:78px; }
.share-progress-history-otp-slot { display:inline-flex;align-items:center;gap:8px;flex:0 0 auto;min-width:0; }
.share-progress-history-otp-slot .share-progress-history-value.is-otp { box-sizing:border-box;flex:0 0 154px!important;min-width:0!important;width:154px!important;max-width:154px!important;height:38px;padding:7px 8px;letter-spacing:2px; }
.share-progress-history-share-line.is-otp-line .share-progress-history-reset { flex:0 0 auto; }
.share-progress-history-share-line.is-otp-line .share-progress-history-send { flex:0 0 auto;margin-left:auto;align-self:center;white-space:nowrap; }
.share-progress-created-footer { display:flex;align-items:center;justify-content:flex-start;width:100%;margin-right:auto; }
.share-progress-created-footer .share-progress-primary-btn { white-space:nowrap; }
@media(max-width:680px) {
 .share-progress-history-share-line.is-otp-line { flex-wrap:wrap; }
 .share-progress-history-share-line.is-otp-line .share-progress-history-send { margin-left:0; }
}

.share-progress-saved-toast { position:fixed;top:84px;right:22px;z-index:21000;max-width:min(430px,calc(100vw - 40px));padding:12px 16px;border:1px solid #a8dfc3;border-left:4px solid #13996b;border-radius:8px;background:#fff;color:#116f50;box-shadow:0 8px 30px rgba(18,20,30,.1);font-size:13px;font-weight:650; }
div[id$="modalSelection"] .modal-dialog { width:min(820px,calc(100vw - 32px))!important;max-width:min(820px,calc(100vw - 32px))!important; }
.share-progress-list-tools { display:flex;align-items:center;justify-content:flex-end;gap:10px;flex-wrap:wrap;min-width:0; }
.share-progress-trigger { margin-left: auto; display: inline-flex; align-items: center; justify-content: center; gap: 8px; flex: 0 0 auto; white-space: nowrap; border: 1px solid #542e88; border-radius: 6px; padding: 8px 13px; background: #542e88; color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: 0 2px 4px rgba(84, 46, 136, .12); transition: background .18s ease, border-color .18s ease, transform .18s ease; }
.share-progress-trigger:hover { background: #43216f; border-color: #43216f; color: #fff; transform: translateY(-1px); }
.share-progress-trigger:focus { outline: 2px solid rgba(84, 46, 136, .25); outline-offset: 2px; }
.share-progress-intro { color: #475569; font-size: 13px; line-height: 1.6; margin: 0 0 16px; }
.share-progress-intro span { display: block; color: #64748b; font-size: 12px; margin-top: 3px; }
.share-progress-list-header { position:relative;z-index:8;display:flex;align-items:center;justify-content:space-between;gap:12px;padding:11px 14px;background:#f8fafc;border:1px solid #dbe3ee;border-bottom:0;color:#475569;font-size:11px;font-weight:700;letter-spacing:.2px;flex-wrap:wrap; }
.share-progress-task-list { max-height: min(58vh, 560px); overflow-x:hidden; overflow-y:auto; border: 1px solid #dbe3ee; border-radius: 0 0 6px 6px; }
.share-progress-task-option { --sp-indent:0px;display:flex;align-items:center;gap:9px;min-height:43px;padding:7px 12px 7px calc(12px + var(--sp-indent));border-bottom:1px solid #edf1f6;color:#334155;font-size:13px;transition:background .15s ease; }
.share-progress-task-option:last-child { border-bottom: 0; }
.share-progress-task-option:hover { background: #f8f5ff; }
.share-progress-task-option input { width:16px;height:16px;margin:0;accent-color:#542e88;flex:0 0 16px;cursor:pointer; }
.share-progress-task-code { color:#542e88;font-weight:750;font-variant-numeric:tabular-nums;white-space:nowrap;flex:0 0 auto; }
.share-progress-task-name { min-width:0;overflow-wrap:anywhere;line-height:1.4; }

.share-progress-list-hint { color:#7b8190;font-size:11px;font-weight:500;letter-spacing:0; }
.share-progress-task-option[hidden] { display:none!important; }
.share-progress-task-option.is-level-1 { background:#faf7ff;font-weight:650; }
.share-progress-task-option.is-level-2 { background:#fff; }
.share-progress-task-option.is-level-3 { --sp-indent:23px; }
.share-progress-task-option.is-level-4 { --sp-indent:46px; }
.share-progress-task-option.is-level-5 { --sp-indent:69px; }
.share-progress-tree-toggle { flex:0 0 22px;width:22px;height:22px;display:inline-flex;align-items:center;justify-content:center;border:1px solid #e4d8f5;border-radius:5px;color:#7042a8;background:#faf7ff;cursor:pointer;transition:background .15s ease; }
.share-progress-tree-toggle:hover { background:#efe6fb; }
.share-progress-tree-toggle i { font-size:10px;transition:transform .15s ease; }
.share-progress-tree-toggle[aria-expanded="false"] i { transform:rotate(-90deg); }
.share-progress-tree-placeholder { flex:0 0 22px;width:22px; }
.share-progress-task-label { display:flex;align-items:center;gap:8px;min-width:0;flex:1;margin:0;cursor:pointer; }
.share-progress-task-option.is-level-1 .share-progress-task-name { font-weight:700; }
.share-progress-error-toast { position:fixed;top:84px;right:22px;z-index:21000;max-width:min(430px,calc(100vw - 40px));display:flex;align-items:flex-start;gap:10px;padding:13px 16px;border:1px solid #fecaca;border-left:4px solid #dc2626;border-radius:9px;background:#fff;color:#991b1b;box-shadow:0 8px 30px rgba(18,20,30,.14);font-size:13px;line-height:1.45; }
.share-progress-error-toast button { margin-left:auto;border:0;background:transparent;color:inherit;cursor:pointer;font-size:16px; }
.share-progress-empty { padding: 26px 16px; text-align: center; color: #64748b; font-size: 13px; }
.share-progress-validation { display: block; color: #b91c1c; font-size: 12px; margin-top: 10px; }
.share-progress-modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 18px; }
.share-progress-primary-btn, .share-progress-secondary-btn, .share-progress-copy-btn { display: inline-flex; align-items: center; justify-content: center; gap: 7px; border-radius: 6px; padding: 9px 14px; font-size: 13px; font-weight: 600; cursor: pointer; text-decoration: none; transition: background .16s ease, border-color .16s ease; }
.share-progress-primary-btn { border: 1px solid #542e88; background: #542e88; color: #fff; }
.share-progress-primary-btn:hover { background: #43216f; border-color: #43216f; color: #fff; }
.share-progress-primary-btn:disabled { opacity: .65; cursor: wait; }
.share-progress-secondary-btn { border: 1px solid #cbd5e1; background: #fff; color: #334155; }
.share-progress-secondary-btn:hover { background: #f8fafc; }
.share-progress-success-heading { display: flex; align-items: center; gap: 13px; margin-bottom: 20px; }
.share-progress-success-icon { display: flex; align-items: center; justify-content: center; flex: 0 0 42px; width: 42px; height: 42px; border-radius: 50%; color: #047857; background: #dcfce7; font-size: 18px; }
.share-progress-success-title { color: #0f172a; font-size: 16px; font-weight: 700; }
.share-progress-success-subtitle { margin-top: 4px; color: #64748b; font-size: 12px; line-height: 1.5; }
.share-progress-result-block { border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px; margin-bottom: 12px; background: #fff; }
.share-progress-result-label { color: #475569; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: .25px; margin-bottom: 9px; }
.share-progress-result-label i { color: #542e88; margin-right: 5px; }
.share-progress-report-link { display: inline-flex; align-items: center; gap: 7px; margin-top: 10px; color: #542e88; font-size: 13px; font-weight: 600; text-decoration: none; }
.share-progress-report-link:hover { color: #3b82f6; text-decoration: underline; }
.share-progress-file-name { display: block; color: #334155; font-size: 12px; overflow-wrap: anywhere; }
.share-progress-otp-row { display: flex; align-items: center; flex-wrap: wrap; gap: 12px; }
.share-progress-otp-value { color: #0f172a; font-size: 26px; font-weight: 800; letter-spacing: 5px; font-variant-numeric: tabular-nums; }
.share-progress-copy-btn { border: 1px solid #ddd2f0; background: #f6f0ff; color: #542e88; }
.share-progress-copy-btn:hover { background: #eee3ff; }
.share-progress-security-note { color: #64748b; font-size: 11px; margin-top: 8px; }
.share-progress-success-message { display: block; color: #047857; font-size: 12px; margin-top: 6px; }
.share-progress-success-actions { justify-content: space-between; }
.share-progress-loading-overlay { position: fixed; inset: 0; z-index: 20000; align-items: center; justify-content: center; padding: 24px; background: rgba(15, 23, 42, .38); backdrop-filter: blur(2px); }
.share-progress-loading-card { width: min(420px, 100%); padding: 28px 26px 24px; background: #fff; border-radius: 12px; box-shadow: 0 20px 60px rgba(15,23,42,.24); text-align: center; }
.share-progress-spinner { width: 42px; height: 42px; margin: 0 auto 16px; border: 4px solid #e9ddfb; border-top-color: #542e88; border-radius: 50%; animation: shareProgressSpin .8s linear infinite; }
.share-progress-loading-title { color: #1e293b; font-size: 16px; font-weight: 700; }
.share-progress-loading-text { margin: 8px auto 16px; max-width: 330px; color: #64748b; font-size: 12px; line-height: 1.65; }
.share-progress-loading-track { height: 5px; overflow: hidden; border-radius: 999px; background: #eee7f8; }
.share-progress-loading-track span { display: block; width: 38%; height: 100%; border-radius: inherit; background: #542e88; animation: shareProgressLoad 1.25s ease-in-out infinite; }
@keyframes shareProgressSpin { to { transform: rotate(360deg); } }
@keyframes shareProgressLoad { 0% { transform: translateX(-110%); } 100% { transform: translateX(290%); } }
@media (max-width: 600px) {
.share-progress-list-header { position:relative;z-index:8;display:flex;align-items:center;justify-content:space-between;gap:12px;padding:11px 14px;background:#f8fafc;border:1px solid #dbe3ee;border-bottom:0;color:#475569;font-size:11px;font-weight:700;letter-spacing:.2px;flex-wrap:wrap; }
.share-progress-task-option { --sp-indent:0px;display:flex;align-items:center;gap:9px;min-height:43px;padding:7px 12px 7px calc(12px + var(--sp-indent));border-bottom:1px solid #edf1f6;color:#334155;font-size:13px;transition:background .15s ease; }
    .share-progress-trigger { padding: 8px 10px; font-size: 12px; }
    .share-progress-success-actions { flex-wrap: wrap; }
}
.share-progress-file-row { display:flex;align-items:center;gap:10px;min-width:0; }
.share-progress-file-row .share-progress-file-name { flex:1;min-width:0;overflow-wrap:anywhere; }
.share-progress-copy-icon { position:relative;display:inline-flex;align-items:center;justify-content:center;flex:0 0 36px;width:36px;height:36px;padding:0;border:1px solid #d8c8ee;border-radius:7px;background:#faf7ff;color:#542e88;font-size:14px;cursor:pointer;transition:background .16s ease,border-color .16s ease,color .16s ease; }
.share-progress-copy-icon:hover,.share-progress-copy-icon:focus-visible { background:#eee4fc;border-color:#ad8cdb;color:#40166d;outline:none; }
.share-progress-copy-icon::after { content:attr(data-copy-tooltip);position:absolute;bottom:calc(100% + 8px);right:0;z-index:20;padding:6px 9px;border-radius:6px;background:#302344;color:#fff;font-size:11px;line-height:1.3;font-weight:600;white-space:nowrap;pointer-events:none;opacity:0;visibility:hidden;transform:translateY(4px);transition:opacity .15s ease,transform .15s ease,visibility .15s ease; }
.share-progress-copy-icon:hover::after,.share-progress-copy-icon:focus-visible::after { opacity:1;visibility:visible;transform:translateY(0); }

.share-progress-server-trigger { display:none!important; }
.share-progress-preset-actions { display:inline-flex;align-items:center;gap:5px;flex-wrap:nowrap; }
.share-progress-tool-btn { position:relative;display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:32px;padding:6px 10px;border:1px solid #ded4ed;border-radius:7px;background:#fff;color:#58318d;font-size:12px;font-weight:700;letter-spacing:0;white-space:nowrap;cursor:pointer;transition:border-color .16s ease,background .16s ease,color .16s ease; }
.share-progress-tool-btn:hover:not(.is-disabled),.share-progress-tool-btn:focus-visible { background:#f4effc;border-color:#ae8cda;outline:none; }
.share-progress-tool-save { background:#542e88;border-color:#542e88;color:#fff; }
.share-progress-tool-save:hover:not(.is-disabled),.share-progress-tool-save:focus-visible { background:#452174;border-color:#452174;color:#fff; }
.share-progress-tool-btn.is-disabled { opacity:.46;cursor:not-allowed;background:#f6f5f8;border-color:#e1dce9;color:#888197; }
.share-progress-tool-btn::after { content:attr(data-sp-tooltip);position:absolute;z-index:200;top:calc(100% + 10px);right:0;width:max-content;max-width:min(285px,75vw);padding:8px 10px;border-radius:7px;background:#302442;color:#fff;box-shadow:0 6px 18px rgba(35,20,55,.16);font-size:11px;font-weight:500;line-height:1.45;letter-spacing:0;text-align:left;white-space:normal;opacity:0;visibility:hidden;transform:translateY(-4px);pointer-events:none;transition:opacity .16s ease,transform .16s ease,visibility .16s ease; }
.share-progress-tool-btn::before { content:"";position:absolute;z-index:201;top:calc(100% + 3px);right:16px;border-width:0 6px 7px;border-style:solid;border-color:transparent transparent #302442;opacity:0;visibility:hidden;pointer-events:none;transition:opacity .16s ease,visibility .16s ease; }
.share-progress-tool-btn:hover::after,.share-progress-tool-btn:focus-visible::after,.share-progress-tool-btn:hover::before,.share-progress-tool-btn:focus-visible::before { opacity:1;visibility:visible;transform:translateY(0); }
@media(max-width:700px) { .share-progress-list-tools { width:100%;justify-content:space-between; } .share-progress-list-hint { display:none; } .share-progress-preset-actions { width:100%;justify-content:flex-end;flex-wrap:wrap; } }
.share-progress-history-danger-actions { display:flex;justify-content:flex-end;margin-top:12px;padding-top:10px;border-top:1px dashed #eee6f4; }
.share-progress-history-delete { display:inline-flex;align-items:center;justify-content:center;gap:7px;padding:7px 10px;border:1px solid #f1bfc7;border-radius:7px;background:#fff7f8;color:#ad2940;font-size:11px;font-weight:700;cursor:pointer;transition:background .16s ease,border-color .16s ease; }
.share-progress-history-delete:hover { background:#ffe9ed;border-color:#dc8190; }
/* Nhật ký: hai hành động icon đặt cạnh nhau trên hàng tiêu đề. */
.share-progress-history-actions { display:inline-flex;align-items:center;justify-content:flex-end;gap:7px;flex:0 0 auto; }
.share-progress-history-actions .share-progress-history-open,
.share-progress-history-actions .share-progress-history-delete {
    width:36px;height:36px;min-width:36px;min-height:36px;
    padding:0;display:inline-flex;align-items:center;justify-content:center;
    border-radius:8px;font-size:14px;line-height:1;flex:0 0 36px;
    text-decoration:none;transition:background-color .16s,border-color .16s,color .16s;
}
.share-progress-history-actions .share-progress-history-open { border:1px solid #d9c8ed;background:#f8f4fd;color:#56218e; }
.share-progress-history-actions .share-progress-history-open:hover { border-color:#b893df;background:#ede4f9;color:#4e1b82; }
.share-progress-history-actions .share-progress-history-delete { border:1px solid #f1bfc7;background:#fff7f8;color:#ad2940;cursor:pointer; }
.share-progress-history-actions .share-progress-history-delete:hover { border-color:#dc8190;background:#ffe9ed;color:#9e1d36; }
.share-progress-history-actions .share-progress-history-open:focus-visible,
.share-progress-history-actions .share-progress-history-delete:focus-visible { outline:2px solid #7b43b9;outline-offset:2px; }
@media(max-width:580px) { .share-progress-history-actions { gap:5px; } .share-progress-history-actions .share-progress-history-open, .share-progress-history-actions .share-progress-history-delete { width:33px;height:33px;min-width:33px;min-height:33px;flex-basis:33px; } }

/* Popup xác nhận dùng MessageBox của MasterTemplate (server), không dựng modal riêng. */
/* Chỉ nâng layer của #modal-notify. Không can thiệp ExtraModal, viền hoặc nút X. */
#modal-notify.sp-gantt-system-confirm { z-index:21050!important; }
.modal-backdrop.sp-gantt-system-backdrop { z-index:21049!important; }
</style>

<script type="text/javascript">
    (function (w) {
        'use strict';
        var busy = false;
        var savePending = false;
        var historyPagingPending = false;
        var historyResetPending = false;
        var historyResetReportId = '';
        var historyDeletePending = false;
        var historyDeleteReportId = '';
        var savedSelection = null;
        var previousTaskList = null;
        function selectedIds() { var el = root(); if (!el) return []; return Array.prototype.map.call(el.querySelectorAll('input[name="shareProgressTaskIds"]:checked'), function (x) { return x.value; }).sort(); }
        function selectedSignature() { return JSON.stringify(selectedIds()); }
        function saveButton() { return document.getElementById("shareProgressSavePresetClient"); }
        function refreshSaveButton() { var el = root(); var btn = saveButton(); if (!el || !btn) return; if (el !== previousTaskList || savedSelection === null) { previousTaskList = el; savedSelection = selectedSignature(); } var dirty = selectedSignature() !== savedSelection; var disabled = !dirty || !selectedIds().length || savePending || busy; btn.classList.toggle("is-disabled", disabled); btn.setAttribute("aria-disabled", disabled ? "true" : "false"); var undo = document.getElementById("shareProgressUndoClient"); if (undo) { undo.classList.toggle("is-disabled", !dirty || savePending || busy); undo.setAttribute("aria-disabled", dirty && !savePending && !busy ? "false" : "true"); } }
        function acceptedSave(message) { savePending = false; savedSelection = selectedSignature(); var selector = getModal("modalSelection"); var label = selector ? selector.querySelector("[id$=\"lblPresetStatus\"]") : null; if (label) label.textContent = "Đã lưu cấu hình của dự án. Lần sau mở sẽ tự chọn đúng các công việc này."; refreshSaveButton(); showSaved(message); }
        function getModal(suffix) { return document.querySelector('[id$="' + suffix + '"]'); }
        function getField(suffix) { return document.querySelector('input[id$="' + suffix + '"]'); }
        // Quan trong: luon dung API Bootstrap de dong popup. Khong tu xoa class show / display.
        // Neu CSS/Bootstrap da bi lech trang thai tu phien ban cu, tai tao instance an toan.
        var modalSwitchPending = false;
        function modalShow(el) {
            if (!el) return false;
            try {
                if (w.bootstrap && w.bootstrap.Modal) {
                    var instance = w.bootstrap.Modal.getInstance(el);
                    if (instance && !el.classList.contains('show') && instance._isShown === true) {
                        instance.dispose();
                        instance = null;
                    }
                    if (!instance) instance = w.bootstrap.Modal.getOrCreateInstance(el);
                    instance.show();
                    return true;
                }
                if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) { w.jQuery(el).modal('show'); return true; }
            } catch (e) { if (w.console) console.error(e); }
            return false;
        }
        function modalHide(el) {
            if (!el) return;
            try {
                if (w.bootstrap && w.bootstrap.Modal) {
                    var instance = w.bootstrap.Modal.getInstance(el);
                    if (instance) instance.hide();
                    return;
                }
                if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) w.jQuery(el).modal('hide');
            } catch (e) { if (w.console) console.error(e); }
        }
        // Cho su kien dong modal ket thuc roi moi mo modal khac (Bootstrap co animation).
        // Lang nghe ca su kien native (Bootstrap 5) va jQuery (Bootstrap 4).
        function closeModalThen(el, next) {
            if (!el || !el.classList.contains('show')) { next(); return; }
            var finished = false;
            function finish() {
                if (finished) return;
                finished = true;
                el.removeEventListener('hidden.bs.modal', finish);
                if (w.jQuery && w.jQuery.fn) w.jQuery(el).off('hidden.bs.modal', finish);
                next();
            }
            el.addEventListener('hidden.bs.modal', finish);
            if (w.jQuery && w.jQuery.fn) w.jQuery(el).one('hidden.bs.modal', finish);
            modalHide(el);
            // Fallback cho theme khong phat hidden event, sau khi transition thong thuong ket thuc.
            w.setTimeout(finish, 700);
        }
        function loading(show, mode) { var el = document.getElementById('shareProgressInlineLoader'); if (!el) return; var title = document.getElementById('shareProgressLoadingTitle'); var text = document.getElementById('shareProgressLoadingText'); if (title && text) { if (mode === 'delete-report') { title.textContent = 'Đang xóa báo cáo'; text.textContent = 'Đang thu hồi liên kết và xóa báo cáo khỏi nhật ký. Vui lòng chờ trong giây lát...'; } else if (mode === 'reset-otp') { title.textContent = 'Đang cấp lại mã OTP'; text.textContent = 'Đang tạo mã OTP mới cho báo cáo và cập nhật nhật ký. Vui lòng chờ trong giây lát...'; } else { title.textContent = 'Đang tạo báo cáo tiến độ'; text.textContent = 'Đang dựng biểu đồ Gantt và bảo vệ báo cáo bằng mã OTP. Vui lòng chờ trong giây lát...'; } } el.style.display = show ? 'flex' : 'none'; }
        function showError(message) { message = message || 'Tạo báo cáo không thành công. Vui lòng thử lại.'; if (w.toastr && typeof w.toastr.error === 'function') { w.toastr.error(message); return; } var old = document.getElementById('shareProgressErrorToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressErrorToast'; toast.className = 'share-progress-error-toast'; toast.setAttribute('role', 'alert'); var icon = document.createElement('i'); icon.className = 'fas fa-exclamation-circle'; var span = document.createElement('span'); span.textContent = message; var x = document.createElement('button'); x.type = 'button'; x.textContent = '×'; x.setAttribute('aria-label', 'Đóng thông báo lỗi'); x.onclick = function () { toast.remove(); }; toast.appendChild(icon); toast.appendChild(span); toast.appendChild(x); document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 8500); }
        function showSaved(message) { message = message || 'Đã lưu cấu hình công việc cho dự án này.'; if (w.toastr && typeof w.toastr.success === 'function') { w.toastr.success(message); return; } var old = document.getElementById('shareProgressSavedToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressSavedToast'; toast.className = 'share-progress-saved-toast'; toast.setAttribute('role', 'status'); toast.textContent = message; document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 3500); }
        function root() { return document.getElementById('shareProgressTaskList'); }
        function updateTree() { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var byCode = {}; rows.forEach(function (row) { byCode[row.getAttribute('data-sp-code')] = row; }); rows.forEach(function (row) { var parent = row.getAttribute('data-sp-parent'); var visible = true; while (parent) { var p = byCode[parent]; if (p) { if (p.hidden || p.getAttribute('data-sp-expanded') !== 'true') { visible = false; break; } } var dot = parent.lastIndexOf('.'); parent = dot < 0 ? '' : parent.substring(0, dot); } row.hidden = !visible; }); }
        function toggleRow(code) { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var row = null; rows.forEach(function (r) { if (r.getAttribute('data-sp-code') === code) row = r; }); if (!row) return; var now = row.getAttribute('data-sp-expanded') !== 'true'; row.setAttribute('data-sp-expanded', now ? 'true' : 'false'); var b = row.querySelector('.share-progress-tree-toggle'); if (b) b.setAttribute('aria-expanded', now ? 'true' : 'false'); updateTree(); }
        function onResult(kind, message) {
            if (kind === 'saved') { acceptedSave(message); return; }
            if (kind === 'saveError') { savePending = false; refreshSaveButton(); showError(message || 'Không thể lưu cấu hình.'); return; }
            savePending = false; busy = false; loading(false);
            var selector = getModal('modalSelection');
            if (kind === 'success') { var nameField = document.getElementById('shareProgressReportName'); if (nameField) { nameField.value = makeDefaultName(nameField.getAttribute('data-project-name') || 'Dự án'); nameField.setAttribute('data-name-reset', '1'); } }
            if (kind !== 'success') {
                // Tao that bai thi giu popup chon cong viec, khong can dong/mo lai.
                if (selector && !selector.classList.contains('show')) modalShow(selector);
                showError(message);
                updateTree(); refreshSaveButton();
                return;
            }
            // Tao thanh cong: dong popup chon bang API Bootstrap, cho hidden roi mo popup ket qua.
            // Khong chen tay vao class/style/backdrop vi se lam lech state Bootstrap.
            modalSwitchPending = true;
            closeModalThen(selector, function () {
                modalSwitchPending = false;
                if (!modalShow(getModal('modalCreated')))
                    showError('Đã tạo báo cáo nhưng không mở được popup kết quả. Hãy kiểm tra thư viện Bootstrap.');
                updateTree(); refreshSaveButton();
            });
        }
        function consumeServerOutcome() { var result = getField('hdfShareProgressOutcome'); if (!result || !result.value) return false; var kind = result.value; result.value = ''; var msg = getField('hdfShareProgressErrorText'); var message = msg ? msg.value : ''; if (msg) msg.value = ''; onResult(kind, message); return true; }
        function onRequestError(args) { var wasPaging = historyPagingPending; historyPagingPending = false; var wasDeleting = historyDeletePending; historyDeletePending = false; historyDeleteReportId = ''; var wasSaving = savePending; var wasResetting = historyResetPending; savePending = false; historyResetPending = false; historyResetReportId = ''; busy = false; loading(false); var message = wasDeleting ? 'Không thể xóa báo cáo. Vui lòng thử lại.' : (wasSaving ? 'Không thể lưu cấu hình. Vui lòng thử lại.' : (wasResetting ? 'Không thể cấp lại OTP. Vui lòng thử lại.' : 'Máy chủ gặp lỗi khi tạo báo cáo. Vui lòng thử lại.')); try { if (args && args.get_error && args.get_error()) message += ' ' + args.get_error().message; } catch (e) { } if (args && args.set_errorHandled) args.set_errorHandled(true); if (!wasSaving && !wasResetting && !wasDeleting && !wasPaging) modalShow(getModal('modalSelection')); refreshSaveButton(); showError(message); }
        function finishProgressHistoryOtpReset() {
            if (!historyResetPending) return;
            var statusField = getField('hdfProgressOtpResetStatus');
            var otpField = getField('hdfProgressOtpResetValue');
            var msgField = getField('hdfProgressOtpResetMessage');
            var status = statusField ? statusField.value : '';
            var newOtp = otpField ? otpField.value : '';
            var message = msgField ? msgField.value : '';
            var id = historyResetReportId;
            if (statusField) statusField.value = '';
            if (otpField) otpField.value = '';
            if (msgField) msgField.value = '';
            historyResetPending = false;
            historyResetReportId = '';
            busy = false;
            loading(false);
            // The history modal is not inside the updated panel; do not hide/show it here.
            if (status !== 'success' || !/^\d{6}$/.test(newOtp)) {
                showError(message || 'Cấp lại OTP thất bại. Vui lòng thử lại.');
                return;
            }
            var row = document.querySelector('.share-progress-history-row[data-report-id="' + id + '"]');
            if (row) {
                var code = row.querySelector('input.share-progress-history-value.is-otp');
                if (code) code.value = newOtp;
                else {
                    var holder = row.querySelector('.share-progress-history-otp-slot');
                    if (holder) {
                        var input = document.createElement('input');
                        input.type = 'text'; input.readOnly = true;
                        input.className = 'share-progress-history-value is-otp';
                        input.setAttribute('aria-label', 'Mã OTP báo cáo');
                        input.value = newOtp;
                        holder.textContent = '';
                        holder.appendChild(input);
                        var copy = document.createElement('button'); copy.type = 'button';
                        copy.className = 'share-progress-history-icon';
                        copy.setAttribute('title', 'Sao chép mã OTP');
                        copy.innerHTML = '<i class="far fa-copy"></i>';
                        copy.onclick = function () { return w.copyProgressHistoryValue(copy, 'otp'); };
                        holder.appendChild(copy);
                    }
                }
            }
            showSaved('Đã cấp mã OTP mới. Mã cũ không còn hiệu lực.');
        }
        function finishProgressHistoryDelete() {
            if (!historyDeletePending) return;
            var statusField = getField('hdfProgressDeleteStatus');
            var messageField = getField('hdfProgressDeleteMessage');
            var ok = statusField && statusField.value === 'success';
            var message = messageField ? messageField.value : '';
            var id = historyDeleteReportId;
            if (statusField) statusField.value = ''; if (messageField) messageField.value = '';
            historyDeletePending = false; historyDeleteReportId = ''; busy = false; loading(false);
            if (!ok) { showError(message || 'Xóa báo cáo thất bại.'); return; }
            var row = document.querySelector('.share-progress-history-row[data-report-id="' + id + '"]');
            if (row) {
                var list = row.closest('.share-progress-history-list'); row.remove();
                if (list && !list.querySelector('.share-progress-history-row')) {
                    var empty = document.createElement('div'); empty.className = 'share-progress-history-empty';
                    empty.textContent = 'Không còn báo cáo trên trang này. Chuyển trang hoặc mở lại Nhật ký để xem các báo cáo khác.';
                    list.appendChild(empty);
                }
            }
            var modal = getModal('modalProgressHistory');
            var count = modal && modal.querySelector('[id$="lblProgressHistoryCount"]');
            if (count) { var oldCount = parseInt(count.textContent, 10); if (!isNaN(oldCount)) count.textContent = Math.max(0, oldCount - 1) + ' báo cáo'; }
            showSaved(message || 'Đã xóa báo cáo.');
        }
        function wirePageRequestManager() {
            if (!(w.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)) return;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm._shareProgressReliableHook) return;
            prm._shareProgressReliableHook = true;
            prm.add_endRequest(function (sender, args) {
                if (args.get_error && args.get_error()) { onRequestError(args); return; }
                if (historyDeletePending) { finishProgressHistoryDelete(); updateTree(); refreshSaveButton(); return; }
                if (historyResetPending) { finishProgressHistoryOtpReset(); updateTree(); refreshSaveButton(); return; }
                if (historyPagingPending) { historyPagingPending = false; syncProgressHistoryScroll(); updateTree(); refreshSaveButton(); return; }
                if (!consumeServerOutcome() && (busy || savePending)) {
                    var wasSaving = savePending; busy = false; savePending = false; loading(false);
                    if (!wasSaving) modalShow(getModal('modalSelection'));
                    showError(wasSaving ? 'Máy chủ chưa xác nhận lưu cấu hình. Vui lòng thử lại.' : 'Máy chủ không trả trạng thái tạo báo cáo. Kiểm tra UpdatePanel và log server.');
                }
                updateTree(); refreshSaveButton();
            });
        }
        function makeDefaultName(project) {
            var d = new Date(), two = function (n) { return String(n).padStart(2, '0'); };
            var stamp = two(d.getDate()) + two(d.getMonth() + 1) + two(d.getFullYear() % 100) + '-' + two(d.getHours()) + two(d.getMinutes());
            project = (project || 'Dự án').trim();
            var available = Math.max(6, 120 - stamp.length - 3);
            return project.substring(0, available).trim() + ' · ' + stamp;
        }
        w.ShareProgressUI = { openSelection: function (e) { if (e && e.preventDefault) e.preventDefault(); if (busy || modalSwitchPending) return false; updateTree(); refreshSaveButton(); if (!modalShow(getModal('modalSelection'))) showError('Không thể mở popup. Vui lòng tải lại trang và thử lại.'); return false; }, consumeServerOutcome: consumeServerOutcome, reset: function () { busy = false; loading(false); }, closeCreated: function () { modalHide(getModal('modalCreated')); } };
        w.shareProgressOpenHistory = function (e) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || modalSwitchPending) return false;
            // Nhật ký là popup riêng, chỉ tải khi bấm nút trên trang Gantt.
            var load = document.querySelector('a[id$="btnLoadProgressHistory"]');
            if (!load) { showError('Không tìm thấy nút tải nhật ký.'); return false; }
            load.click();
            return false;
        };

        // Xác nhận qua MessageBox chuẩn do MasterTemplate hiển thị.
        // Không có window.confirm() hoặc custom JS MessageBox trên trang Gantt.
        w.shareProgressDeleteHistoryReport = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || historyResetPending || historyDeletePending || historyPagingPending || !button) return false;
            var id = button.getAttribute('data-report-id') || '';
            if (!/^[a-fA-F0-9]{32}$/.test(id)) return false;
            var field = getField('hdfProgressDeleteRequestId');
            var trigger = document.querySelector('a[id$="btnRequestProgressDelete"]');
            if (!field || !trigger) { showError('Không tìm thấy điều khiển xác nhận xóa báo cáo.'); return false; }
            field.value = id;
            trigger.click();
            return false;
        };
        w.shareProgressResetHistoryOtp = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || historyResetPending || historyDeletePending || historyPagingPending || !button) return false;
            var id = button.getAttribute('data-report-id') || '';
            if (!/^[a-fA-F0-9]{32}$/.test(id)) return false;
            var field = getField('hdfProgressOtpResetRequestId');
            var trigger = document.querySelector('a[id$="btnRequestProgressOtpReset"]');
            if (!field || !trigger) { showError('Không tìm thấy điều khiển xác nhận cấp lại OTP.'); return false; }
            field.value = id;
            trigger.click();
            return false;
        };
        // Chỉ được gọi sau khi server nhận xác nhận của MessageBox chuẩn.
        // Các HiddenField kết quả đã được cập nhật qua UpdatePanel riêng, không thay DOM Nhật ký.
        w.shareProgressApplyConfirmedAction = function (kind, id) {
            if (!/^[a-fA-F0-9]{32}$/.test(id || '')) return;
            busy = true;
            if (kind === 'otp') {
                historyResetReportId = id;
                historyResetPending = true;
                finishProgressHistoryOtpReset();
            } else if (kind === 'delete') {
                historyDeleteReportId = id;
                historyDeletePending = true;
                finishProgressHistoryDelete();
            }
            syncProgressHistoryScroll();
        };
        // MasterTemplate's MessageBox is placed above the current ExtraModal without styling its border.
        w.shareProgressStackSystemMessageBox = function () {
            var masterModal = document.getElementById('modal-notify');
            if (!masterModal) return;
            masterModal.classList.add('sp-gantt-system-confirm');
            [0, 60, 200].forEach(function (delay) {
                w.setTimeout(function () {
                    if (!masterModal.classList.contains('show')) return;
                    var backdrops = document.querySelectorAll('.modal-backdrop');
                    if (backdrops.length) backdrops[backdrops.length - 1].classList.add('sp-gantt-system-backdrop');
                }, delay);
            });
        };
        // Khóa cuộn trang nền trong khi popup Nhật ký mở. Popup vẫn tự co giãn và
        // cuộn nội dung qua .modal-body khi vượt viewport. Trả CSS gốc khi đóng.
        var historyScrollState = null;
        var historyScrollObserver = null;
        function rememberHistoryPageScroll() {
            if (historyScrollState || !document.body) return;
            historyScrollState = { htmlOverflow: document.documentElement.style.overflow, bodyOverflow: document.body.style.overflow };
        }
        function historyIsShown() {
            var modal = getModal('modalProgressHistory');
            return !!modal && (modal.classList.contains('show') || modal.style.display === 'block' && modal.getAttribute('aria-hidden') !== 'true');
        }
        function fitHistoryContentToViewport() {
            var modal = getModal('modalProgressHistory');
            if (!modal || !historyIsShown()) return;
            var dialog = modal.querySelector('.modal-dialog');
            var section = modal.querySelector('.modal-body');
            if (!dialog || !section) return;
            var header = dialog.querySelector('.modal-header');
            var footer = dialog.querySelector('.modal-footer');
            var style = w.getComputedStyle(dialog);
            var margin = (parseFloat(style.marginTop) || 0) + (parseFloat(style.marginBottom) || 0);
            var available = (w.innerHeight || document.documentElement.clientHeight) - margin -
                (header ? header.offsetHeight : 0) - (footer ? footer.offsetHeight : 0) - 16;
            if (available > 100) section.style.maxHeight = Math.floor(available) + 'px';
        }
        function syncProgressHistoryScroll() {
            var html = document.documentElement, body = document.body;
            if (!html || !body) return;
            if (historyIsShown()) {
                rememberHistoryPageScroll();
                if (html.style.overflow !== 'hidden') html.style.overflow = 'hidden';
                if (body.style.overflow !== 'hidden') body.style.overflow = 'hidden';
                fitHistoryContentToViewport();
            } else if (historyScrollState) {
                html.style.overflow = historyScrollState.htmlOverflow;
                // Bootstrap may have applied overflow:hidden before our observer ran;
                // never strand the document locked after the final modal closes.
                var otherShownModal = !!document.querySelector('.modal.show');
                body.style.overflow = !otherShownModal && !body.classList.contains('modal-open') &&
                    historyScrollState.bodyOverflow === 'hidden' ? '' : historyScrollState.bodyOverflow;
                historyScrollState = null;
            }
        }
        function initProgressHistoryScrollLock() {
            var modal = getModal('modalProgressHistory');
            if (!modal) return;
            if (historyScrollObserver && historyScrollObserver.target === modal) { syncProgressHistoryScroll(); return; }
            if (historyScrollObserver) historyScrollObserver.disconnect();
            historyScrollObserver = new MutationObserver(function () { syncProgressHistoryScroll(); });
            historyScrollObserver.observe(modal, { attributes: true, attributeFilter: ['class', 'style', 'aria-hidden'] });
            historyScrollObserver.target = modal;
            modal.addEventListener('show.bs.modal', rememberHistoryPageScroll);
            modal.addEventListener('shown.bs.modal', syncProgressHistoryScroll);
            modal.addEventListener('hidden.bs.modal', syncProgressHistoryScroll);
            if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) {
                w.jQuery(modal).on('show.bs.modal.shareGanttScroll', rememberHistoryPageScroll);
                w.jQuery(modal).on('shown.bs.modal.shareGanttScroll hidden.bs.modal.shareGanttScroll', syncProgressHistoryScroll);
            }
            syncProgressHistoryScroll();
        }
        w.shareProgressSendCreatedCustomer = function (e) {
            if (e && e.preventDefault) e.preventDefault();
            var dialog = getModal('modalCreated');
            var reportLink = dialog ? dialog.querySelector('a[id$="lnkReportFile"]') : null;
            var reportName = dialog ? dialog.querySelector('.share-progress-file-name') : null;
            var otpField = getField('hdfOtp');
            var name = reportName ? reportName.textContent.trim() : 'Báo cáo tiến độ dự án';
            var url = reportLink ? reportLink.href : '';
            var otp = otpField ? otpField.value.trim() : '';
            if (!url || !/^\d{6}$/.test(otp)) {
                showError('Thiếu liên kết hoặc mã OTP để gửi khách hàng.');
                return false;
            }
            var subject = 'Báo cáo tiến độ: ' + name;
            var body = 'Xin chào,\n\nTôi gửi báo cáo tiến độ dự án để anh/chị tham khảo.\n\nBáo cáo: ' + name + '\nLiên kết: ' + url + '\nMã OTP: ' + otp + '\n\nTrân trọng.';
            w.location.href = 'mailto:?subject=' + encodeURIComponent(subject) + '&body=' + encodeURIComponent(body);
            return false;
        };
        w.shareProgressSendCustomer = function (button) {
            var row = button && button.closest('.share-progress-history-row');
            if (!row) return false;
            var name = row.getAttribute('data-report-name') || 'Báo cáo tiến độ dự án';
            var link = row.querySelector('.share-progress-history-link-hidden');
            var otp = row.querySelector('input.share-progress-history-value.is-otp');
            if (!link || !link.value || !otp || !/^\d{6}$/.test(otp.value)) {
                showError('Báo cáo chưa có đầy đủ liên kết hoặc mã OTP.'); return false;
            }
            var subject = 'Báo cáo tiến độ: ' + name;
            var body = 'Xin chào,\n\nTôi gửi báo cáo tiến độ dự án để anh/chị tham khảo.\n\nBáo cáo: ' + name + '\nLiên kết: ' + link.value + '\nMã OTP: ' + otp.value + '\n\nTrân trọng.';
            // Opens the user's email composer; does not send mail without their approval.
            w.location.href = 'mailto:?subject=' + encodeURIComponent(subject) + '&body=' + encodeURIComponent(body);
            return false;
        };
        w.shareProgressHistoryBeforePage = function (e, button) {
            if (busy || savePending || historyDeletePending || historyResetPending || historyPagingPending) {
                if (e && e.preventDefault) e.preventDefault();
                return false;
            }
            historyPagingPending = true;
            // Bootstrap modal node is stable; only the inner UpdatePanel swaps rows/footer.
            var panel = document.querySelector('[id$="upShareProgressHistory"]');
            if (panel) panel.classList.add('share-progress-history-page-panel-pending');
            return true;
        };
        w.shareProgressToggleHistoryRow = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (!button) return false;
            var row = button.closest('.share-progress-history-row');
            if (!row || busy || historyPagingPending) return false;
            var expand = !row.classList.contains('is-expanded');
            var list = row.closest('.share-progress-history-list');
            if (list) list.querySelectorAll('.share-progress-history-row.is-expanded').forEach(function (other) {
                if (other !== row) {
                    other.classList.remove('is-expanded');
                    other.querySelectorAll('.share-progress-history-name-btn').forEach(function (b) { b.setAttribute('aria-expanded', 'false'); });
                }
            });
            row.classList.toggle('is-expanded', expand);
            row.querySelectorAll('.share-progress-history-name-btn').forEach(function (b) { b.setAttribute('aria-expanded', expand ? 'true' : 'false'); });
            fitHistoryContentToViewport();
            return false;
        };
        w.copyProgressHistoryValue = function (button, kind) {
            var row = button && button.closest('.share-progress-history-share-line');
            var field = row && row.querySelector('input.share-progress-history-value');
            var value = field ? field.value : '';
            return copyText(value, button, kind === 'otp' ? 'Sao chép mã OTP' : 'Sao chép liên kết');
        };
        w.shareProgressToggleBranch = function (e, code) { if (e) { e.preventDefault(); e.stopPropagation(); } toggleRow(code); return false; };
        w.shareProgressRestoreDefaults = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy) return false; var el = root(); if (!el) return false; el.querySelectorAll("[data-sp-level]").forEach(function (row) { var check = row.querySelector("input[name=\"shareProgressTaskIds\"]"); if (check) check.checked = Number(row.getAttribute("data-sp-level")) <= 2; }); refreshSaveButton(); return false; };
        w.shareProgressUndo = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy || savedSelection === null) return false; var selected = new Set(JSON.parse(savedSelection)); var el = root(); if (el) el.querySelectorAll("input[name=\"shareProgressTaskIds\"]").forEach(function (ch) { ch.checked = selected.has(ch.value); }); refreshSaveButton(); return false; };
        w.validateProgressPresetSave = function (btn) { refreshSaveButton(); if (busy || savePending || !btn || btn.getAttribute("aria-disabled") === "true") return false; var el = root(); if (!el || !el.querySelector("input[name=\"shareProgressTaskIds\"]:checked")) { showError("Vui lòng chọn ít nhất một công việc trước khi lưu cấu hình."); return false; } savePending = true; refreshSaveButton(); return true; };
        w.shareProgressSavePreset = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.validateProgressPresetSave(btn)) return false; var link = document.querySelector("a[id$=\"btnSaveProgressPreset\"]"); if (!link) { savePending = false; refreshSaveButton(); showError("Không tìm thấy nút AJAX lưu cấu hình. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        w.showProgressCreationLoading = function (btn) { var title = document.getElementById('shareProgressReportName'); if (!title || !title.value.trim() || title.value.trim().length > 120) { showError('Nhập tên báo cáo (tối đa 120 ký tự).'); if (title) title.focus(); return false; } var el = root(); var checked = el ? el.querySelectorAll('input[name="shareProgressTaskIds"]:checked') : []; if (!checked.length) { var label = getModal('modalSelection'); var v = label ? label.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = 'Vui lòng chọn ít nhất một công việc.'; showError('Vui lòng chọn ít nhất một công việc để tạo báo cáo.'); return false; } if (busy) return false; var msg = getModal('modalSelection'); var v = msg ? msg.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = ''; busy = true; loading(true); return true; };
        w.shareProgressCreateReport = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.showProgressCreationLoading(btn)) return false; var link = document.querySelector("a[id$=\"btnCreateProgress\"]"); if (!link) { busy = false; loading(false); showError("Không tìm thấy nút AJAX tạo báo cáo. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        function copyFeedback(button, ok, original) { if (!button) return; button.setAttribute('data-copy-tooltip', ok ? 'Đã sao chép!' : 'Không thể sao chép'); button.setAttribute('aria-label', ok ? 'Đã sao chép!' : 'Không thể sao chép'); w.setTimeout(function () { if (button.isConnected) { button.setAttribute('data-copy-tooltip', original); button.setAttribute('aria-label', original); } }, 1700); if (!ok) showError('Không thể sao chép tự động. Vui lòng sao chép thủ công.'); }
        function fallbackCopyText(value) { var area = document.createElement('textarea'); area.value = value; area.readOnly = true; area.style.position = 'fixed'; area.style.left = '-9999px'; document.body.appendChild(area); area.focus(); area.select(); var ok = false; try { ok = document.execCommand('copy'); } catch (e) { } document.body.removeChild(area); return ok; }
        function copyText(value, button, hint) { if (!value) { showError('Chưa có dữ liệu để sao chép.'); return false; } if (navigator.clipboard && w.isSecureContext) { navigator.clipboard.writeText(value).then(function () { copyFeedback(button, true, hint); }, function () { copyFeedback(button, fallbackCopyText(value), hint); }); } else { copyFeedback(button, fallbackCopyText(value), hint); } return false; }
        w.copyGeneratedProgressOtp = function (button) { var field = getField('hdfOtp'); var otp = field ? field.value : ''; return copyText(otp, button, 'Sao chép mã OTP'); };
        w.copyGeneratedProgressLink = function (button) { var dialog = getModal('modalCreated'); var a = dialog ? dialog.querySelector('a[id$="lnkReportFile"]') : null; var url = a ? a.href : ''; return copyText(url, button, 'Sao chép liên kết'); };
        w.closeGeneratedProgressModal = function () { w.ShareProgressUI.closeCreated(); };
        document.addEventListener('change', function (e) { if (e.target && e.target.matches('#shareProgressTaskList input[name="shareProgressTaskIds"]')) refreshSaveButton(); });
        w.addEventListener('resize', function () { fitHistoryContentToViewport(); });
        document.addEventListener('shown.bs.modal', function (e) { if (e.target && e.target.id === 'modal-notify') { w.shareProgressStackSystemMessageBox(); syncProgressHistoryScroll(); } });
        document.addEventListener('hidden.bs.modal', function (e) { if (e.target && e.target.id === 'modal-notify') syncProgressHistoryScroll(); });
        wirePageRequestManager(); if (w.Sys && Sys.Application) Sys.Application.add_load(function () { wirePageRequestManager(); initProgressHistoryScrollLock(); if (!busy && !savePending) consumeServerOutcome(); updateTree(); refreshSaveButton(); }); else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { initProgressHistoryScrollLock(); updateTree(); consumeServerOutcome(); refreshSaveButton(); }); else { initProgressHistoryScrollLock(); updateTree(); consumeServerOutcome(); refreshSaveButton(); }
    })(window);
</script>
