<%@ Control Language="C#" %>
<%-- Floating Timer V2: chi doc, khong code-behind, khong phu thuoc SignalR. --%>
<% if (SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current != null &&
       SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User != null &&
       SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User.LaNhanVien) { %>
<section id="kbgFloatingTimer" class="kbg-float" hidden aria-label="Các đồng hồ giờ công đang mở"
         data-status-url="<%= ResolveUrl("~/fKhaiBaoGio/FloatingWorkTimer.ashx") %>"
         data-user-key="<%= SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User.UserId.ToString("N") %>">
    <button type="button" id="kbgFloatToggle" class="kbg-float-pill" aria-expanded="false"
            aria-controls="kbgFloatDetails" aria-label="Xem các đồng hồ đang mở">
        <span class="kbg-float-activity" aria-hidden="true">⏱<span class="kbg-float-activity-dot"></span></span>
        <span class="kbg-float-compact-main">
            <strong id="kbgFloatSummary">Đang tải đồng hồ</strong>
            <small id="kbgFloatSubtitle">Giờ công việc</small>
        </span>
        <span class="kbg-float-chevron" aria-hidden="true">⌃</span>
    </button>
    <div class="kbg-float-tip" id="kbgFloatTip" role="tooltip" aria-hidden="true">
        <strong id="kbgFloatTipTitle">Các phiên giờ đang mở</strong>
        <span id="kbgFloatTipBody">Bấm để xem chi tiết theo dự án.</span>
    </div>
    <div class="kbg-float-details" id="kbgFloatDetails" aria-hidden="true" role="region" aria-label="Danh sách đồng hồ đang mở">
        <div class="kbg-float-detail-head">
            <div class="kbg-float-title-wrap">
                <span class="kbg-float-title-icon" aria-hidden="true">⏱</span>
                <div>
                    <div class="kbg-float-detail-title">Đồng hồ đang mở</div>
                    <div class="kbg-float-detail-caption" id="kbgFloatCount">Đang đồng bộ...</div>
                </div>
            </div>
            <button type="button" class="kbg-float-close" id="kbgFloatCollapse" aria-label="Thu gọn danh sách"><span aria-hidden="true">⌄</span></button>
        </div>
        <div class="kbg-float-meta" id="kbgFloatMeta">
            <span class="kbg-float-meta-live" id="kbgFloatCountLive">0 đang cộng</span>
            <span class="kbg-float-meta-paused" id="kbgFloatCountPaused">0 tạm ngừng</span>
        </div>
        <p class="kbg-float-warning" id="kbgFloatWarning" role="status" hidden>Chưa đồng bộ được dữ liệu mới. Thời gian đang hiển thị là lần cập nhật gần nhất.</p>
        <div class="kbg-float-scroll" id="kbgFloatScroll">
            <div class="kbg-float-multi-list" id="kbgFloatMultiList" aria-live="off"></div>
        </div>
        <div class="kbg-float-footnote">Mỗi phiên tính giờ độc lập, kể cả khi chạy đồng thời trên nhiều dự án.</div>
    </div>
</section>
<% } %>
