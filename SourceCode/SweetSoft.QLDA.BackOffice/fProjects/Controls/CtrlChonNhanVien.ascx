<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChonNhanVien.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChonNhanVien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* CSS CÔ LẬP CHO GIAO DIỆN ACCORDION VÀ LỊCH BỂU TRƯỢT */
    .member-section-title { font-size: 13px; font-weight: 700; color: #1e3a8a; padding: 10px 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; margin-bottom: 6px; cursor: pointer; display: flex; justify-content: space-between; align-items: center; user-select: none; }
    .member-section-title:hover { background: #f1f5f9; }
    .member-section-title .arrow-icon { font-size: 10px; color: #64748b; transition: transform 0.25s ease; }
    .member-accordion-group.open .arrow-icon { transform: rotate(180deg); }
    .member-accordion-content { max-height: 0; overflow: hidden; transition: max-height 0.35s ease-in-out; }
    .member-accordion-group.open .member-accordion-content { max-height: 1200px; overflow-y: auto; overflow-x: hidden; }

    /* ROW NHÂN VIÊN VÀ HIỆU ỨNG SLIDE LỊCH BỂU */
    .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 8px; overflow: hidden; display: flex; flex-direction: column; align-items: stretch; }
    .member-item-row.show-schedule { border-color: #93c5fd; box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08); }
    .row-default-view { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; width: 100%; min-height: 52px; box-sizing: border-box; font-size: 13px; }
    .member-info-group { display: flex; align-items: center; gap: 10px; min-width: 0; }
    .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.25; }
    .member-email { font-size: 11px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .member-email:empty { display: none; }
    .member-empty { padding: 18px 12px; text-align: center; font-size: 12.5px; color: #64748b; }
    .member-info-group input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: #2563eb; }
    
    .btn-calendar-only { background: #ffffff; border: 1px solid #e2e8f0; width: 32px; height: 32px; border-radius: 8px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 15px; transition: all 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); }
    .btn-calendar-only:hover { background: #eff6ff; border-color: #93c5fd; transform: scale(1.1); }
    .member-item-row.show-schedule .btn-calendar-only { background: #eff6ff; border-color: #2563eb; }

    /* PANEL LỊCH XỔ XUỐNG NGAY DƯỚI DÒNG NHÂN VIÊN */
    .row-schedule-panel { max-height: 0; overflow: hidden; border-top: 1px solid transparent; transition: max-height 0.35s ease, border-color 0.35s ease; }
    .member-item-row.show-schedule .row-schedule-panel { max-height: 460px; border-top-color: #e2e8f0; }
    .row-schedule-inner { padding: 8px 12px 12px; }

    /* LỊCH THÁNG MINI (kiểu lịch Windows: T2 -> CN, chuyển tháng bằng ▲ ▼) */
    .mini-cal-wrap { flex: 1; min-width: 0; display: flex; flex-direction: column; width: 100%;}
    .mini-cal { flex: 1; min-width: 0; box-sizing: border-box; user-select: none; width: 100%;}

    .mc-header { display: flex; align-items: center; justify-content: space-between; padding: 0 2px 4px 4px; }
    .mc-title { font-size: 14px; font-weight: 700; color: #1e293b; }
    .mc-nav { display: flex; gap: 2px; }
    .mc-nav-btn { width: 30px; height: 26px; border: 0; background: transparent; border-radius: 6px; color: #64748b; font-size: 10px; line-height: 1; cursor: pointer; transition: background 0.15s, color 0.15s; }
    .mc-nav-btn:hover { background: #eff6ff; color: #2563eb; }
    .mc-nav-btn:focus-visible { outline: 2px solid #2563eb; outline-offset: 1px; }

    .mc-weekdays, .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100%;}
    .mc-weekdays span { text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
    .mc-weekdays span.mc-we { color: #94a3b8; }
    .mc-grid { grid-auto-rows: 50px; gap: 2px; }
    /* Khung cố định 6 hàng (6x50 + 5x2); tháng cũ/mới chồng lên nhau khi trượt */
    .mc-viewport { position: relative; overflow: hidden; height: 310px; }
    .mc-viewport .mc-grid { position: absolute; top: 0; left: 0; right: 0; will-change: transform, opacity; }

    .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
    .mc-day .mc-num { font-weight: 600; line-height: 1; }
    .mc-day.out-month .mc-num { color: #94a3b8; }
    /* Ngày nằm ngoài khoảng bắt đầu - kết thúc dự án: làm mờ */
    .mc-day.out-range { opacity: 0.32; }

    .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; }
    .mc-day.st-holiday { background: #fef3c7; color: #b45309; }
    .mc-day.st-weekend { background: #f1f5f9; color: #64748b; }
    .mc-day.st-free    { background: #e6f4ea; color: #137333; }
    .mc-day.today { border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }

    .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    .single-avatar-circle {
        width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
        font-size: 11px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }
    /*HIGHLIGHT NHÂN VIÊN GỐC (ĐÃ THUỘC DỰ ÁN TỪ TRƯỚC)*/
    .member-item-row.original-member {
        background-color: #f0f8ff; /* Xanh da trời rất dịu (Alice Blue) */
        border-color: #bfe0ff;     /* Viền xanh lam nhạt */
    }
    .member-item-row.original-member:hover {
        background-color: #e6f3ff;
    }
    /* Đổi màu nút lịch biểu cho hợp tone với nền xanh */
    .member-item-row.original-member .btn-calendar-only {
        background-color: #ffffff;
        border-color: #bfe0ff;
    }
    .member-item-row.original-member .btn-calendar-only:hover,
    .member-item-row.original-member.show-schedule .btn-calendar-only {
        background-color: #dbeafe;
        border-color: #3b82f6;
    }
    /* ============================================================
       TOOLTIP GLOBAL (NẰM TRÊN LAYER CAO NHẤT CỦA TRÌNH DUYỆT)
       ============================================================ */
    .mc-day { position: relative !important; overflow: visible !important; }
    .mc-day.has-tooltip { cursor: pointer; }
    .mc-day:hover, .mc-day.show-tooltip { z-index: 50; }
    
    .global-task-tooltip { 
        position: absolute; /* Định vị tuyệt đối so với toàn màn hình */
        z-index: 9999999 !important; /* Đè lên mọi Modal/Popup */
        background: #ffffff; color: #334155; 
        border: 1px solid #cbd5e1; border-radius: 10px; padding: 11px 16px; 
        min-width: 340px; max-width: 520px; width: max-content; 
        white-space: normal; word-break: break-word; overflow-wrap: anywhere; 
        box-shadow: 0 10px 40px rgba(15, 23, 42, 0.2); font-size: 12.5px; line-height: 1.5; 
        opacity: 0; visibility: hidden; pointer-events: none; 
        transition: opacity 0.15s ease, visibility 0.15s ease; 
    }
    .global-task-tooltip.show { opacity: 1; visibility: visible; pointer-events: auto; }
    
    /* Mũi tên chỉ xuống (Tự động chạy qua lại theo tọa độ JS) */
    .global-task-tooltip::after { 
        content: ''; position: absolute; top: 100%; 
        left: var(--arrow-left, 50%); /* Chạy động bằng CSS Variable */
        transform: translateX(-50%);
        border-width: 6px; border-style: solid; border-color: #ffffff transparent transparent transparent; 
    }

    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    .t-project-name { font-size: 10.5px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 3px; font-weight: 600; color: #64748b; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlMemberPicker" Type="Primary">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlMemberPicker" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
            <div class="row js-validation validationEngineContainer p-2">
    
    <div class="col-12 mb-3">
        <div style="font-size: 12px; color: #1e40af; background: #eff6ff; padding: 10px 12px; border-radius: 6px; border: 1px solid #bfdbfe;">
            <asp:Literal runat="server" ID="ltrInfoNote"></asp:Literal>
        </div>
    </div>

    <!-- TÌM KIẾM + LỌC CHỨC DANH: dùng control chuẩn như trang Nhân viên, kết quả hiện thành tag bên dưới -->
    <div class="col-12 mb-2">
        <div class="d-flex flex-column flex-md-row gap-2">
            <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchDefault">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchChucDanh" runat="server"
                            Text="Chức Danh"
                            AutoPostBack="true"
                            AllowClear="true"
                            SearchColumn="IdChucDanh"
                            EnableSearch="true"
                            ValueIsOfTypeGUID="True"
                            SearchPlaceholder="Tìm kiếm theo tên chức danh..."
                            NoResultsText="Không tìm thấy tên chức danh"
                            OnSelectedValueChanged="ddlSearchChucDanh_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Panel runat="server" DefaultButton="lbtSearchSingle" CssClass="input-group flex-grow-1">
                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
            </asp:Panel>
        </div>
        <div class="listSearchTagBox">
            <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="col-12" style="max-height: 60vh; overflow-y: auto; overflow-x: hidden;">
        <asp:UpdatePanel ID="upMemberList" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
        <!-- DANH SÁCH NHÂN VIÊN DUY NHẤT -->
        <div class="member-accordion-group open" id="accGroupCompany">
            <div class="member-section-title" style="background: #f1f5f9;" onclick="CMSMasterJs.TogglePickerAccordion(this)">
                <span>🏢 <%= GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST) %> (<asp:Literal ID="ltrCountCompany" runat="server">0</asp:Literal>)</span>
                <span class="arrow-icon">▼</span>
            </div>
            <div class="member-accordion-content">
                <asp:Repeater ID="rptCompanyMembers" runat="server" OnItemDataBound="rptCompanyMembers_ItemDataBound">
                    <ItemTemplate>
                        <div class='member-item-row <%# Convert.ToBoolean(Eval("IsOriginal")) ? "original-member" : "" %>' id='mem-row-<%# Eval("UserId") %>'>
                            <div class="row-default-view">
                                <div class="member-info-group">
                                    <asp:CheckBox runat="server" ID="chkSelect" />
                                    <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                    <%# Eval("AvatarHtml") %>
                                    <div class="member-name-block">
                                        <span>
                                            <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
                                            <!-- Render Badge cảnh báo task -->
                                        <%# Convert.ToInt32(Eval("TaskCount")) > 0 ? string.Format("<span class='badge ms-2' style='font-size:10.5px; padding:3px 6px; background-color: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd;'><i class='fas fa-tasks me-1'></i>{0}</span>", string.Format(GetResourceText(BackEndResourceKeys.DOING_X_TASKS) ?? "Đảm nhận {0} công việc", Eval("TaskCount"))) : "" %>
                                        </span>
                                        <span class="member-email"><%#: Eval("Email") %></span>
                                    </div>
                                </div>
                                <button type="button" class="btn-calendar-only" title="Xem lịch" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>')">📅</button>
                            </div>
                            
                            <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                            <div class="row-schedule-panel" id='overlay-<%# Eval("UserId") %>'>
                                <div class="row-schedule-inner">
                                    <div class="mini-cal-wrap">
                                        <div class="mini-cal" id='timeline-<%# Eval("UserId") %>'></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoData" runat="server" CssClass="member-empty" Visible="false">
                    <asp:Literal ID="ltrNoData" runat="server"></asp:Literal>
                </asp:Panel>
            </div>
        </div> 
        </ContentTemplate>
        </asp:UpdatePanel>
    </div> 
</div>
            </ContentTemplate>
        </asp:UpdatePanel>
        
    </ContentTemplate>
    
    <FooterTemplate>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraButton runat="server" ID="btnConfirm" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClick="btnConfirm_Click">
                    <%= GetResourceText(BackEndResourceKeys.CONFIRM) %>
                </SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};

    $(document).ready(function () {
        var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

        function pad(n) { return n < 10 ? '0' + n : '' + n; }
        // month: 0-11. Trả về 'yyyy-MM-dd' theo giờ local (không dùng toISOString để tránh lệch múi giờ)
        function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
        function parseKey(k) { var p = k.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }
        function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }
        function titleText(st) { return 'Tháng ' + (st.month + 1) + ', ' + st.year; }

        function buildGridHtml(st) {
            var now = new Date();
            var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
            var offset = (new Date(st.year, st.month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-grid">';

            for (var i = 0; i < 42; i++) {
                var d = new Date(st.year, st.month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!st.minKey && key >= st.minKey && key <= st.maxKey;
                var info = inRange ? st.data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== st.month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                if (key === todayKey) cls += ' today';

                var extra = '';
                var tooltipHtml = '';
                var clickAttr = '';

                if (info) {
                    var label = String(info.text || '').replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    if (label) extra = '<span class="mc-label">' + escAttr(label) + '</span>';

                    // [SỬA LẠI]: Gói dữ liệu vào attribute ẩn, KHÔNG VẼ TOOLTIP Ở ĐÂY NỮA
                    var hasTasks = (info.status === "busy" && info.tasks && info.tasks.length > 0);
                    if (hasTasks) {
                        cls += ' has-tooltip';
                        var listHtml = '<ul class="tooltip-task-list">';
                        for (var t = 0; t < info.tasks.length; t++) {
                            listHtml += '<li><div class="t-project-name"><i class="fas fa-cube me-1"></i>' + escAttr(info.tasks[t].projectName) + '</div><span class="t-code">[' + escAttr(info.tasks[t].code) + ']</span>' + escAttr(info.tasks[t].name) + '</li>';
                        }
                        listHtml += '</ul>';

                        // Nén Data vào thẻ Div của ô ngày đó
                        clickAttr = ' data-tooltip-content="' + encodeURIComponent(listHtml) + '"';
                    } else if (info.text) {
                        clickAttr = ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"';
                    }
                }

                // Dòng này đã bỏ đi biến tooltipHtml cũ
                html += '<div class="' + cls + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
            }
            return html + '</div>';
        }

        // Vẽ toàn bộ lịch (tiêu đề + thứ + lưới) - dùng lần đầu mở
        function renderMiniCalendar(cal) {
            var st = cal.data('mc');

            var html = '<div class="mc-header">' +
                '<span class="mc-title">' + titleText(st) + '</span>' +
                '<div class="mc-nav">' +
                '<button type="button" class="mc-nav-btn" title="Tháng trước" onclick="CMSMasterJs.ChangeScheduleMonth(this, -1)">&#9650;</button>' +
                '<button type="button" class="mc-nav-btn" title="Tháng sau" onclick="CMSMasterJs.ChangeScheduleMonth(this, 1)">&#9660;</button>' +
                '</div></div>';

            html += '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div>';

            html += '<div class="mc-viewport">' + buildGridHtml(st) + '</div>';
            cal.html(html);
        }

        $(document).on('click', function () {
            $('.mc-day').removeClass('show-tooltip');
        });
        CMSMasterJs.TogglePickerAccordion = function (btnElement) {
            $(btnElement).closest('.member-accordion-group').toggleClass('open');
        };

        // TRƯỢT THÁNG
        CMSMasterJs.ChangeScheduleMonth = function (btnElement, delta) {
            var cal = $(btnElement).closest('.mini-cal');
            var st = cal.data('mc');
            if (!st) return;

            var viewport = cal.find('.mc-viewport');

            // [BẬT KHÓA VÁCH NGĂN]
            viewport.addClass('is-animating');
            $('.mc-day').removeClass('show-tooltip');

            var grids = viewport.children('.mc-grid');
            if (grids.length > 1) {
                grids.not(':last').remove();
                var lastEl = grids.last()[0];
                if (lastEl.getAnimations) lastEl.getAnimations().forEach(function (a) { a.cancel(); });
            }

            var d = new Date(st.year, st.month + delta, 1);
            st.year = d.getFullYear();
            st.month = d.getMonth();
            cal.find('.mc-title').text(titleText(st));

            var oldGrid = viewport.children('.mc-grid').last();
            var newGrid = $(buildGridHtml(st));
            viewport.append(newGrid);

            var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            if (reduceMotion || !newGrid[0].animate) {
                oldGrid.remove();
                viewport.removeClass('is-animating');
                return;
            }

            var dir = delta > 0 ? 1 : -1;
            var opts = { duration: 260, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'both' };

            oldGrid[0].animate([
                { transform: 'translateY(0)', opacity: 1 },
                { transform: 'translateY(' + (-dir * 40) + '%)', opacity: 0 }
            ], opts).onfinish = function () {
                oldGrid.remove();
            };

            newGrid[0].animate([
                { transform: 'translateY(' + (dir * 40) + '%)', opacity: 0 },
                { transform: 'translateY(0)', opacity: 1 }
            ], opts).onfinish = function () {
                // [THÁO KHÓA VÁCH NGĂN]
                viewport.removeClass('is-animating');
            };
        };

        // MỞ LỊCH SỔ XUỐNG
        CMSMasterJs.ToggleRowSchedule = function (btnElement, userId, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');
            var currentPanel = rowEl.closest('.member-accordion-content');

            if (typeof isShow !== 'boolean') isShow = !rowEl.hasClass('show-schedule');

            if (isShow) {
                // Tắt các lịch cũ và đóng luôn vách ngăn của tụi nó
                currentPanel.find('.member-item-row.show-schedule').not(rowEl).removeClass('show-schedule schedule-opened');

                var jsonString = rowEl.find('input[type="hidden"][id*="hdfScheduleJson"]').val();
                var cal = rowEl.find('#timeline-' + userId);
                var data = {};

                if (jsonString) {
                    try {
                        data = JSON.parse(jsonString) || {};
                    } catch (e) {
                        console.error("Lỗi parse JSON lịch biểu: ", e);
                    }
                }

                var keys = Object.keys(data).sort();
                var minKey = keys.length ? keys[0] : null;
                var maxKey = keys.length ? keys[keys.length - 1] : null;

                var now = new Date();
                var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
                var baseDate = (minKey && todayKey >= minKey && todayKey <= maxKey) ? now
                    : (minKey ? parseKey(minKey) : now);

                cal.data('mc', {
                    data: data,
                    minKey: minKey,
                    maxKey: maxKey,
                    year: baseDate.getFullYear(),
                    month: baseDate.getMonth()
                });
                renderMiniCalendar(cal);

                rowEl.addClass('show-schedule');

                // Đợi 380ms cho rèm kéo xong -> Mở khóa Vách ngăn
                setTimeout(function () {
                    if (rowEl.hasClass('show-schedule')) {
                        rowEl.addClass('schedule-opened');
                    }
                    if (rowEl.hasClass('show-schedule') && rowEl[0].scrollIntoView) {
                        rowEl[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                }, 380);

            } else {
                rowEl.removeClass('show-schedule schedule-opened');
            }
        };
        $(document).on('click', '[id$="btnAccept"]', function () {
            // Lấy chính xác cái Popup Chọn nhân viên hiện tại
            var $pickerModal = $('#<%= mdlMemberPicker.ClientID %>');

            // Nếu phát hiện user bấm "Đồng ý" trong lúc Popup này đang mở
            if ($pickerModal.is(':visible')) {
                // Kích hoạt hàm ẩn mặc định của Bootstrap
                $pickerModal.modal('hide');
            }
        });
        // ============================================================
        // ĐỘNG CƠ TOOLTIP GLOBAL (ĐỊNH VỊ TUYỆT ĐỐI THEO TỌA ĐỘ CHUỘT)
        // ============================================================

        // Sinh ra 1 cái khung Tooltip duy nhất nằm ở ngoài cùng màn hình
        $('body').append('<div id="global-task-tooltip" class="global-task-tooltip"></div>');
        var $globalTooltip = $('#global-task-tooltip');
        var pinnedDay = null; // Biến nhớ xem ông có đang click ghim nó lại không

        function showGlobalTooltip(dayEl) {
            var $day = $(dayEl);
            var contentRaw = $day.attr('data-tooltip-content');
            if (!contentRaw) return;

            // Xả nén Data và nhét vào khung Global
            $globalTooltip.html(decodeURIComponent(contentRaw));

            // Thuật toán quét tọa độ
            var offset = $day.offset();
            var ttWidth = $globalTooltip.outerWidth();
            var ttHeight = $globalTooltip.outerHeight();
            var dayWidth = $day.outerWidth();

            // Tính toán vị trí đâm thẳng lên đỉnh đầu ô ngày
            var topPos = offset.top - ttHeight - 10;
            var leftPos = offset.left + (dayWidth / 2) - (ttWidth / 2);

            // Thuật toán thông minh: Va chạm mép trái/phải màn hình
            var windowWidth = $(window).width();
            var arrowLeft = 50; // Trọng tâm mũi tên 50%

            if (leftPos < 10) {
                var overstep = 10 - leftPos;
                leftPos = 10;
                arrowLeft = 50 - (overstep / ttWidth * 100);
            } else if (leftPos + ttWidth > windowWidth - 10) {
                var overstep = (leftPos + ttWidth) - (windowWidth - 10);
                leftPos = windowWidth - ttWidth - 10;
                arrowLeft = 50 + (overstep / ttWidth * 100);
            }

            // Bắn Tooltip bay tới tọa độ
            $globalTooltip.css({
                top: topPos + 'px',
                left: leftPos + 'px',
                '--arrow-left': arrowLeft + '%' // Dời cái mũi tên đi theo
            });

            $globalTooltip.addClass('show');
        }

        function hideGlobalTooltip() {
            $globalTooltip.removeClass('show');
        }

        // 1. Hover chuột
        $(document).on('mouseenter', '.mc-day.has-tooltip', function () {
            if (pinnedDay) return;
            showGlobalTooltip(this);
        }).on('mouseleave', '.mc-day.has-tooltip', function () {
            if (pinnedDay) return;
            hideGlobalTooltip();
        });

        // 2. Click chuột để Ghim (Thuận tiện khi copy text)
        $(document).on('click', '.mc-day.has-tooltip', function (e) {
            e.stopPropagation();
            if (pinnedDay === this) {
                pinnedDay = null;
                hideGlobalTooltip();
                $(this).removeClass('show-tooltip');
            } else {
                $('.mc-day').removeClass('show-tooltip');
                pinnedDay = this;
                $(this).addClass('show-tooltip');
                showGlobalTooltip(this);
            }
        });

        // 3. Click ra ngoài tắt ghim
        $(document).on('click', function () {
            pinnedDay = null;
            $('.mc-day').removeClass('show-tooltip');
            hideGlobalTooltip();
        });

        // 4. Nếu cuộn thanh cuộn của khung danh sách -> Tự động giấu Tooltip tránh việc nó bị trôi lơ lửng
        $('.col-12').on('scroll', function () {
            if (pinnedDay) {
                pinnedDay = null;
                $('.mc-day').removeClass('show-tooltip');
                hideGlobalTooltip();
            }
        });
    });
    (function () {
        var BASE_Z = 106000;

        // Giữ hành vi cũ: 2 popup nghiệp vụ chồng nhau (không tính MessageBox)
        function stackBusinessModals() {
            var $modals = $('.modal:visible').not('#modal-notify');
            if ($modals.length > 1) {
                $('.modal-backdrop').each(function (i) {
                    $(this).css('z-index', BASE_Z + (i * 10));
                });
                $modals.each(function (i) {
                    $(this).css('z-index', BASE_Z + (i * 10) + 5);
                });
            }
        }

        // Đẩy MessageBox hệ thống lên trên cùng, chỉ khi đang có popup khác
        function raiseNotify() {
            var notify = document.getElementById('modal-notify');
            if (!notify || !$(notify).is(':visible')) return;

            var $others = $('.modal:visible').not(notify);
            if ($others.length === 0) return; // MessageBox dùng đơn lẻ -> không đụng

            var maxZ = BASE_Z;
            $others.each(function () {
                maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(this).zIndex, 10) || 0);
            });

            notify.style.setProperty('z-index', String(maxZ + 20), 'important');
            var bds = document.querySelectorAll('.modal-backdrop');
            if (bds.length) bds[bds.length - 1].style.setProperty('z-index', String(maxZ + 15), 'important');
        }

        function raiseNotifyRepeatedly() {
            [0, 100, 250, 500, 900].forEach(function (ms) {
                setTimeout(function () { stackBusinessModals(); raiseNotify(); }, ms);
            });
        }

        // 1) Bám sự kiện Bootstrap của chính MessageBox (không phụ thuộc mốc thời gian)
        ['show.bs.modal', 'shown.bs.modal'].forEach(function (evt) {
            document.addEventListener(evt, function (e) {
                if (e.target && e.target.id === 'modal-notify') raiseNotifyRepeatedly();
            });
        });

        // 2) Trả z-index về mặc định khi MessageBox đóng
        document.addEventListener('hidden.bs.modal', function (e) {
            if (e.target && e.target.id === 'modal-notify') {
                e.target.style.removeProperty('z-index');
            }
        });

        // 3) Dự phòng: sau mỗi postback ajax
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(raiseNotifyRepeatedly);
        }
    })();
</script>