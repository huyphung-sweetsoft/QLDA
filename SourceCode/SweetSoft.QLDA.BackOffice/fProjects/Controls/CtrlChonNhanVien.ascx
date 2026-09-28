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
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlMemberPicker" Type="Primary" DefaultButton="btnConfirm">
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
                        <div class="member-item-row" id='mem-row-<%# Eval("UserId") %>'>
                            <div class="row-default-view">
                                <div class="member-info-group">
                                    <asp:CheckBox runat="server" ID="chkSelect" />
                                    <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                    <%# Eval("AvatarHtml") %>
                                    <div class="member-name-block">
                                        <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
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

        // Dựng lưới 6 hàng x 7 cột của tháng đang xem (st.year, st.month)
        function buildGridHtml(st) {
            var now = new Date();
            var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());

            // Tuần bắt đầu từ Thứ 2: getDay() 0=CN..6=T7 -> offset 0=T2..6=CN
            var offset = (new Date(st.year, st.month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-grid">';

            for (var i = 0; i < 42; i++) { // luôn 6 hàng để chiều cao không nhảy khi đổi tháng
                var d = new Date(st.year, st.month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!st.minKey && key >= st.minKey && key <= st.maxKey;
                var info = inRange ? st.data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== st.month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                if (key === todayKey) cls += ' today';

                // Nhãn chữ lấy từ text C# (đã qua resource), bỏ emoji đầu chuỗi vì màu ô đã thể hiện trạng thái
                var extra = '';
                if (info && info.text) {
                    var label = String(info.text).replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    extra = '<span class="mc-label">' + escAttr(label) + '</span>';
                }

                var tip = (info && info.text) ? ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"' : '';
                html += '<div class="' + cls + '"' + tip + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
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

        CMSMasterJs.TogglePickerAccordion = function (btnElement) {
            $(btnElement).closest('.member-accordion-group').toggleClass('open');
        };

        // Bấm ▲ (delta = -1) hoặc ▼ (delta = +1): lưới tháng mới trượt lên/xuống thay cho tháng cũ
        CMSMasterJs.ChangeScheduleMonth = function (btnElement, delta) {
            var cal = $(btnElement).closest('.mini-cal');
            var st = cal.data('mc');
            if (!st) return;

            var viewport = cal.find('.mc-viewport');

            // Bấm nhanh liên tiếp: chốt luôn hiệu ứng đang chạy rồi mới bắt đầu hiệu ứng mới
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
            if (reduceMotion || !newGrid[0].animate) { oldGrid.remove(); return; }

            // ▼ (tháng sau): tháng mới từ dưới trượt lên. ▲ (tháng trước): tháng mới từ trên trượt xuống.
            var dir = delta > 0 ? 1 : -1;
            var opts = { duration: 260, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'both' };

            oldGrid[0].animate([
                { transform: 'translateY(0)', opacity: 1 },
                { transform: 'translateY(' + (-dir * 40) + '%)', opacity: 0 }
            ], opts).onfinish = function () { oldGrid.remove(); };

            newGrid[0].animate([
                { transform: 'translateY(' + (dir * 40) + '%)', opacity: 0 },
                { transform: 'translateY(0)', opacity: 1 }
            ], opts);
        };

        CMSMasterJs.ToggleRowSchedule = function (btnElement, userId, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');

            // Không truyền isShow -> tự đảo trạng thái (bấm 📅 lần nữa để thu lại)
            if (typeof isShow !== 'boolean') isShow = !rowEl.hasClass('show-schedule');

            if (isShow) {
                // Đóng các dòng khác đang mở lịch (panel tự thu lại nhờ CSS transition)
                $('.member-item-row.show-schedule').not(rowEl).removeClass('show-schedule');

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

                // Khoảng ngày hợp lệ = khoảng key có trong JSON (chính là StartDate -> EndDate của dự án)
                var keys = Object.keys(data).sort();
                var minKey = keys.length ? keys[0] : null;
                var maxKey = keys.length ? keys[keys.length - 1] : null;

                // Mở ở tháng hiện tại nếu hôm nay nằm trong dự án, ngược lại mở ở tháng bắt đầu
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

                // Sau khi panel xổ xong, cuộn nhẹ để lịch không bị che khuất ở cuối danh sách
                setTimeout(function () {
                    if (rowEl.hasClass('show-schedule') && rowEl[0].scrollIntoView) {
                        rowEl[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                }, 380);

            } else {
                rowEl.removeClass('show-schedule');
            }
        };
    });
</script>