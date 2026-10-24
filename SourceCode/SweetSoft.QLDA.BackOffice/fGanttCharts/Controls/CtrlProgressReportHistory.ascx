<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlProgressReportHistory.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls.CtrlProgressReportHistory" %>
<%@ Import Namespace="System.Web" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<link href="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.css") %>" rel="stylesheet" type="text/css" />
<asp:UpdatePanel ID="upProgressHistoryOpen" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:LinkButton ID="btnLoadProgressHistory" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnLoadProgressHistory_Click" tabindex="-1" aria-hidden="true">Nhật ký</asp:LinkButton>
    </ContentTemplate>
</asp:UpdatePanel>
<%-- Dựng modal Nhật ký một lần. Chỉ UpdatePanel bên TRONG được thay thế khi phân trang. --%>
<SweetSoft:ExtraModal runat="server" ID="modalProgressHistory" Type="Primary" Title="Nhật ký báo cáo tiến độ">
    <ContentTemplate>
        <div class="share-progress-history-top">
            <span><i class="fas fa-history"></i> Các báo cáo đã tạo cho dự án hiện tại</span>
            <small>5 báo cáo/trang</small>
        </div>
        <%-- Ô nhập nằm NGOÀI UpdatePanel để không mất focus/con trỏ sau mỗi lần tìm bằng AJAX. --%>
        <div class="share-progress-history-search">
            <asp:Label ID="lblHistorySearch" runat="server" AssociatedControlID="txtHistorySearch" Text="Tìm theo tên báo cáo" />
            <div class="share-progress-history-search-row">
                <div class="share-progress-history-search-input-wrap">
                    <asp:TextBox ID="txtHistorySearch" runat="server" MaxLength="120" CssClass="share-progress-history-search-input" placeholder="Gõ tên để xem gợi ý, Enter để tìm..." autocomplete="off" aria-autocomplete="list" aria-controls="shareProgressHistorySuggestions" aria-expanded="false" />
                    <div id="shareProgressHistorySuggestions" class="share-progress-history-suggestions" role="listbox" aria-label="Gợi ý báo cáo" hidden="hidden"></div>
                </div>
                <button type="button" class="share-progress-history-search-button" onclick="return shareProgressHistorySearchNow(event);"><i class="fas fa-search" aria-hidden="true"></i> Tìm</button>
            </div>
        </div>
        <asp:UpdatePanel ID="upShareProgressHistory" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <%-- Nút server ẩn vẫn nằm TRONG UpdatePanel để tìm kiếm là async postback. --%>
                <asp:LinkButton ID="btnHistorySearch" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnHistorySearch_Click" tabindex="-1" aria-hidden="true">Tìm nhật ký</asp:LinkButton>
                <%-- Danh mục tên báo cáo toàn dự án, không chứa OTP, dùng cho autocomplete KHÔNG postback. --%>
                <asp:HiddenField ID="hdfHistorySearchCatalog" runat="server" />
                <asp:Literal ID="ltrProgressHistory" runat="server" />
                <div class="share-progress-history-footer">
                    <asp:Label ID="lblProgressHistoryCount" runat="server" aria-live="polite" />
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
                <script src="<%= ResolveUrl("~/fGanttCharts/Controls/progress-report-shared.js") %>" type="text/javascript"></script>
