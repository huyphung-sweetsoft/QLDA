<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlShareProgress.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls.CtrlShareProgress" %>
<%@ Import Namespace="System.Web" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<link href="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.css") %>" rel="stylesheet" type="text/css" />
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

<div id="shareProgressInlineLoader" class="share-progress-loading-overlay" style="display:none;" aria-live="polite" aria-busy="true">
                    <div class="share-progress-loading-card">
                        <div class="share-progress-spinner"></div>
                        <div id="shareProgressLoadingTitle" class="share-progress-loading-title">Đang tạo báo cáo tiến độ</div>
                        <div id="shareProgressLoadingText" class="share-progress-loading-text">Đang dựng biểu đồ Gantt và bảo vệ báo cáo bằng mã OTP. Vui lòng chờ trong giây lát...</div>
                        <div class="share-progress-loading-track"><span></span></div>
                    </div>
                </div>


<script src="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.js") %>" type="text/javascript"></script>
