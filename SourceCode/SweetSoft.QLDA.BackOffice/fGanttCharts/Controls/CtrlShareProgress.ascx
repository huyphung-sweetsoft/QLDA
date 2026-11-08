<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlShareProgress.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls.CtrlShareProgress" %>
<%@ Import Namespace="System.Web" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<link href="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.css") %>" rel="stylesheet" type="text/css" />
<SweetSoft:ExtraModal runat="server" ID="modalSelection" Type="Primary" Title="Tạo báo cáo tiến độ từ Gantt đang mở">
    <ContentTemplate>
        <asp:Label ID="lblPresetStatus" runat="server" CssClass="share-progress-preset-status" />
        <div class="share-progress-report-name-row">
            <label for="shareProgressReportName"><i class="far fa-file-alt" aria-hidden="true"></i> Tên báo cáo <span class="share-progress-required">*</span></label>
            <asp:Literal ID="litReportNameInput" runat="server" />
            <small>Báo cáo sẽ lấy toàn bộ công việc từ biểu đồ đang mở. Bản Khách hàng sử dụng dữ liệu chỉnh sửa tạm, không cập nhật công việc gốc.</small>
        </div>
        <%-- Giữ cây lựa chọn cũ để tương thích JS và code-behind; ẩn khi tạo trực tiếp từ tab Gantt. --%>
        <div class="share-progress-list-header">
            <span><i class="fas fa-list-ul me-1"></i> DANH SÁCH CÔNG VIỆC THEO CẤP</span>
            <div class="share-progress-list-tools">
                <span class="share-progress-list-hint">Mở mũi tên để chọn cấp sâu hơn</span>
                <div class="share-progress-preset-actions" role="group" aria-label="Điều chỉnh cấu hình công việc">
                    <button type="button" id="shareProgressUndoClient" class="share-progress-tool-btn is-disabled" aria-disabled="true" data-sp-tooltip="Hoàn tác" onclick="return shareProgressUndo(event);"><i class="fas fa-undo-alt"></i> Hoàn tác</button>
                    <button type="button" class="share-progress-tool-btn" onclick="return shareProgressRestoreDefaults(event);"><i class="fas fa-layer-group"></i> Mặc định</button>
                    <button type="button" id="shareProgressSavePresetClient" class="share-progress-tool-btn share-progress-tool-save is-disabled" aria-disabled="true" onclick="return shareProgressSavePreset(event,this);"><i class="fas fa-save"></i> Lưu</button>
                </div>
            </div>
        </div>
        <div class="share-progress-task-list" id="shareProgressTaskList"><asp:Literal ID="ltrTaskChoices" runat="server" />
            <asp:PlaceHolder ID="phNoTasks" runat="server" Visible="false"><div class="share-progress-empty">Dự án chưa có công việc để tạo báo cáo.</div></asp:PlaceHolder>
        </div>
        <asp:Label ID="lblSelectionMessage" runat="server" CssClass="share-progress-validation" EnableViewState="false" />
    </ContentTemplate>
    <FooterTemplate><div class="share-progress-report-footer is-create-only"><button type="button" class="share-progress-primary-btn" onclick="return shareProgressCreateReport(event,this);"><i class="fas fa-file-alt me-1"></i> Tạo báo cáo tiến độ</button></div></FooterTemplate>
</SweetSoft:ExtraModal>

<asp:UpdatePanel ID="upShareProgress" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:LinkButton ID="btnSaveProgressPreset" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnSaveProgressPreset_Click" tabindex="-1" aria-hidden="true">Lưu cấu hình</asp:LinkButton>
        <asp:LinkButton ID="btnCreateProgress" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnCreateProgress_Click" tabindex="-1" aria-hidden="true">Tạo báo cáo</asp:LinkButton>
        <asp:HiddenField runat="server" ID="hdfShareProgressOutcome" />
        <asp:HiddenField runat="server" ID="hdfShareProgressErrorText" />
        <SweetSoft:ExtraModal runat="server" ID="modalCreated" Type="Primary" Title="Tạo báo cáo tiến độ thành công">
            <ContentTemplate>
                <div class="share-progress-success-heading"><span class="share-progress-success-icon"><i class="fas fa-check"></i></span><div>
                    <div class="share-progress-success-title">Báo cáo đã sẵn sàng</div>
                    <div class="share-progress-success-subtitle">Người được chia sẻ liên kết có thể xem báo cáo trực tiếp, không cần OTP.</div>
                </div></div>
                <div class="share-progress-result-block">
                    <div class="share-progress-result-label"><i class="far fa-file-code"></i> File HTML báo cáo</div>
                    <div class="share-progress-file-row"><asp:Literal ID="ltrReportFileName" runat="server" />
                        <button type="button" class="share-progress-copy-icon" data-copy-tooltip="Sao chép liên kết" aria-label="Sao chép liên kết" onclick="return copyGeneratedProgressLink(this);"><i class="far fa-copy"></i></button>
                    </div>
                    <asp:HyperLink ID="lnkReportFile" runat="server" CssClass="share-progress-report-link" Target="_blank"><i class="fas fa-external-link-alt"></i> Mở file HTML báo cáo</asp:HyperLink>
                </div>
                <%-- Giữ control cũ ẩn để không phải sửa designer.cs; báo cáo mới không sinh OTP. --%>
                <div style="display:none!important" aria-hidden="true"><asp:Literal ID="ltrOtp" runat="server" /><asp:HiddenField ID="hdfOtp" runat="server" /></div>
                <asp:Label ID="lblCreateMessage" runat="server" CssClass="share-progress-success-message" />
            </ContentTemplate>
            <FooterTemplate><div class="share-progress-created-footer"><button type="button" class="share-progress-primary-btn" onclick="return shareProgressSendCreatedCustomer(event);"><i class="fas fa-paper-plane"></i> Gửi khách hàng</button></div></FooterTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
<div id="shareProgressInlineLoader" class="share-progress-loading-overlay" style="display:none;" aria-live="polite" aria-busy="true">
    <div class="share-progress-loading-card"><div class="share-progress-spinner"></div>
        <div id="shareProgressLoadingTitle" class="share-progress-loading-title">Đang tạo báo cáo tiến độ</div>
        <div id="shareProgressLoadingText" class="share-progress-loading-text">Đang dựng biểu đồ Gantt và xuất liên kết chia sẻ...</div>
        <div class="share-progress-loading-track"><span></span></div>
    </div>
</div>
<script src="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.js") %>" type="text/javascript"></script>
