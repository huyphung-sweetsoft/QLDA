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
        <span class="kbg-float-activity" aria-hidden="true">
            <svg class="kbg-float-clock-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">
                <circle cx="12" cy="13" r="8" />
                <path d="M12 8.5v5l3.1 1.7M9.3 2.4h5.4M12 2.4V5M18.6 5.9l1.5-1.5" />
            </svg>
            <span class="kbg-float-activity-dot"></span>
        </span>
    </button>
    <div class="kbg-float-tip" id="kbgFloatTip" role="tooltip" aria-hidden="true">
        <strong id="kbgFloatTipTitle">Các phiên giờ đang mở</strong>
        <span id="kbgFloatTipBody">Bấm để xem chi tiết theo dự án.</span>
    </div>
    <div class="kbg-float-details" id="kbgFloatDetails" aria-hidden="true" role="region" aria-label="Danh sách đồng hồ đang mở">
        <div class="kbg-float-detail-head">
            <div class="kbg-float-title-wrap">
                <span class="kbg-float-title-icon" aria-hidden="true">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 8.5v5l3.1 1.7M9.3 2.4h5.4M12 2.4V5M18.6 5.9l1.5-1.5"/></svg>
                </span>
                <div>
                    <div class="kbg-float-detail-title">Đồng hồ đang mở</div>
                    <div class="kbg-float-detail-caption" id="kbgFloatCount">Đang đồng bộ...</div>
                </div>
            </div>
            <button type="button" class="kbg-float-close" id="kbgFloatCollapse" aria-label="Thu gọn danh sách" title="Thu gọn">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" aria-hidden="true" focusable="false"><path d="M18 6L6 18M6 6l12 12"/></svg>
            </button>
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
