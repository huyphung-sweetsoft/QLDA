
<%@ Control Language="C#" %>
<%-- Read-only floating timer. Authentication and employee eligibility are checked again by the handler. --%>
<% if (SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current != null &&
       SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User != null &&
       SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User.LaNhanVien) { %>
<section id="kbgFloatingTimer" class="kbg-float" hidden aria-label="Đồng hồ công việc đang chạy"
    data-status-url="<%= ResolveUrl("~/fKhaiBaoGio/FloatingWorkTimer.ashx") %>"
    data-time-page="<%= ResolveUrl("~/fKhaiBaoGio/KhaiBaoGio.aspx") %>"
    data-user-key="<%= SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current.User.UserId.ToString("N") %>">
    <button type="button" class="kbg-float-pill" id="kbgFloatToggle" aria-expanded="false"
        aria-controls="kbgFloatDetails" aria-label="Mở thông tin công việc đang tính giờ">
        <span class="kbg-float-compact">
            <span class="kbg-float-compact-line">
                <span class="kbg-float-mini-icon" aria-hidden="true"><i class="mdi mdi-timer-outline"></i></span>
                <strong data-kbg="compact-time">00:00:00</strong>
            </span>
        </span>
        <i class="mdi mdi-chevron-up kbg-float-chevron" aria-hidden="true"></i>
    </button>
    <div class="kbg-float-tip" id="kbgFloatTip" role="tooltip">
        <div class="kbg-float-tip-code" data-kbg="tooltip-code"></div>
        <div class="kbg-float-tip-name" data-kbg="tooltip-name"></div>
        <div class="kbg-float-tip-project" data-kbg="tooltip-project"></div>
    </div>
    <div class="kbg-float-details" id="kbgFloatDetails" aria-hidden="true">
        <div class="kbg-float-detail-head">
            <div class="kbg-float-detail-title"><i class="mdi mdi-timer-outline" aria-hidden="true"></i> Công việc đang tính giờ</div>
            <button type="button" class="kbg-float-close" id="kbgFloatCollapse" aria-label="Thu gọn đồng hồ"><i class="mdi mdi-chevron-down" aria-hidden="true"></i></button>
        </div>
        <div class="kbg-float-detail-sub">
            <span data-kbg="project-code">—</span>
            <span class="kbg-float-separator">/</span>
            <strong data-kbg="task-code">—</strong>
            <span data-kbg="detail-status" class="kbg-float-status">Đang chạy</span>
        </div>
        <div class="kbg-float-task-name" data-kbg="task-name"></div>
        <div class="kbg-float-project-name" data-kbg="project-name"></div>
        <div class="kbg-float-watch">
            <strong data-kbg="detail-time">00:00:00</strong>
            <span>Thời gian công việc hôm nay</span>
        </div>
        <a id="kbgFloatGoto" class="kbg-float-goto" href="<%= ResolveUrl("~/fKhaiBaoGio/KhaiBaoGio.aspx") %>">
            Đến trang Khai báo giờ <i class="mdi mdi-arrow-top-right" aria-hidden="true"></i>
        </a>
    </div>
</section>
<% } %>
