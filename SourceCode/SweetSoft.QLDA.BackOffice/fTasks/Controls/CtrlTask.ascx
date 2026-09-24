<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlXemNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienTask" %>

<style>
    .avatar-group { 
        display: inline-flex !important; 
        align-items: center; 
        justify-content: center; 
        gap: 6px !important; 
        flex-wrap: nowrap !important; 
        white-space: nowrap !important;
    }  
    .avatar-stack-container { 
        display: flex; 
        align-items: center; 
    }    
    .avatar-circle { 
        width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
        font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; 
        margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }    
    .avatar-circle:first-child { margin-left: 0; }    
    .avatar-more { 
        background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; 
    }    
    .btn-assign-task { 
        width: 26px; height: 26px; border-radius: 6px; background-color: #2563eb; color: white; 
        display: flex; align-items: center; justify-content: center; border: none; cursor: pointer; 
        text-decoration: none; font-size: 12px; transition: background 0.2s, transform 0.1s;
        flex-shrink: 0; 
    }
    .btn-assign-task:hover { 
        background-color: #1d4ed8; color: white; transform: scale(1.05); 
    }

    /* ========================================================
       CSS CHO LỊCH MINI THÁNG & TOOLTIP
       ======================================================== */
    .mini-cal-wrap { width: 100% !important; box-sizing: border-box; }
    .mini-cal { width: 100% !important; user-select: none; }
    .mc-header { display: flex; align-items: center; justify-content: space-between; padding: 0 2px 8px 4px; }
    .mc-title { font-size: 15px; font-weight: 700; color: #1e293b; }
    .mc-nav { display: flex; gap: 4px; }
    .mc-nav-btn { width: 32px; height: 28px; border: 1px solid #cbd5e1; background: #ffffff; border-radius: 6px; color: #64748b; font-size: 11px; line-height: 1; cursor: pointer; transition: all 0.15s; }
    .mc-nav-btn:hover { background: #eff6ff; color: #2563eb; border-color: #93c5fd; }
    
    .mc-weekdays { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100% !important; margin-bottom: 4px; }
    .mc-weekdays span { text-align: center; font-size: 12px; font-weight: 700; color: #475569; padding: 4px 0; }
    .mc-weekdays span.mc-we { color: #dc2626; }
    
    .mc-viewport { position: relative; overflow: hidden; height: 330px; width: 100% !important; }
    .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); grid-auto-rows: 52px; gap: 3px; width: 100% !important; }
    .mc-viewport .mc-grid { position: absolute; top: 0; left: 0; right: 0; will-change: transform, opacity; }
    
    .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid #e2e8f0; background: #ffffff; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; position: relative; transition: all 0.2s; }
    .mc-day:hover { border-color: #94a3b8; box-shadow: inset 0 0 0 1px #94a3b8; }
    .mc-day .mc-num { font-weight: 700; line-height: 1; font-size: 13px; }
    .mc-day.out-month .mc-num { color: #94a3b8; opacity: 0.6; }
    .mc-day.out-range { opacity: 0.35; background: #f8fafc; }
    
    /* MÀU TRẠNG THÁI LỊCH MINI */
    .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; border-color: #fca5a5; }
    .mc-day.st-holiday { background: #fef3c7; color: #b45309; border-color: #fde68a; }
    .mc-day.st-weekend { background: #f1f5f9; color: #64748b; border-color: #cbd5e1; }
    .mc-day.st-free    { background: #dcfce7; color: #15803d; border-color: #bbf7d0; }
    .mc-day.today { border-color: #2563eb !important; box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.2); }
    
    .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 10px; font-weight: 700; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    /* TOOLTIP GHIM CHI TIẾT TASK */
    .custom-task-tooltip {
        position: fixed !important;
        z-index: 999999 !important;
        background: #ffffff;
        color: #334155;
        border: 1px solid #cbd5e1;
        border-radius: 10px;
        padding: 11px 14px;
        min-width: 180px;
        max-width: 420px;
        white-space: normal;
        word-break: break-word;
        overflow-wrap: anywhere;
        box-shadow: 0 10px 30px rgba(15, 23, 42, 0.18);
        font-size: 12px;
        line-height: 1.5;
        opacity: 0;
        visibility: hidden;
        pointer-events: none;
        transition: opacity 0.2s ease;
    }
    .mc-day.show-tooltip .custom-task-tooltip {
        opacity: 1;
        visibility: visible;
        pointer-events: auto;
    }
    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 7px 0; border-bottom: 1px solid #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }

    /* NÚT XEM TRẠNG THÁI KHÁC */
    .btn-assign-task.view-only { background-color: #64748b; }
    .btn-assign-task.view-only:hover { background-color: #475569; }
    .btn-filter-overdue, .btn-tool-folder { transition: all 0.2s; }
    .btn-filter-overdue.active-filter { background-color: #fee2e2 !important; color: #ef4444 !important; border-color: #ef4444 !important; }
    .btn-tool-folder.active-filter { background-color: #e0f2fe !important; color: #0ea5e9 !important; border-color: #0ea5e9 !important; }

    .row-overdue-bg > td { background-color: #fef2f2 !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-overdue-bg:hover > td { background-color: #fee2e2 !important; }
    .row-warning-bg > td { background-color: #fffbeb !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-warning-bg:hover > td { background-color: #fef3c7 !important; } 
</style>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            
            <!-- THANH CÔNG CỤ TRÊN CÙNG (GỘP 1 HÀNG) -->
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-3">
                
                <!-- NHÓM BÊN TRÁI: 2 Nút JS + Search -->
                <div class="d-flex gap-2 align-items-center flex-wrap flex-grow-1">
                    <button type="button" class="btn-filter-overdue" id="btnFilterOverdue" onclick="toggleOverdueFilter()">
                        <i class="fas fa-exclamation-triangle"></i> <%= GetResourceText(BackEndResourceKeys.SHOW_ONLY_OVERDUE_TASKS) %> ( <span id="lblOverdueCount" runat="server">0</span> )
                    </button>
                    
                    <button type="button" class="btn-tool-folder" id="btnToggleTree" onclick="toggleTaskTree()" 
                            data-expand-text="<%= GetResourceText(BackEndResourceKeys.EXPAND_ALL) %>" 
                            data-collapse-text="<%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %>">
                        <i class="far fa-folder-open"></i> <span id="lblToggleText"><%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %></span>
                    </button>
                    
                    <div class="input-group mb-0" style="max-width: 350px;">
                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
                    </div>
                </div>

                <!-- NHÓM BÊN PHẢI: Chú thích màu + Nút Thêm Mới -->
                <div class="d-flex gap-3 align-items-center flex-wrap">
                    <div class="d-flex align-items-center gap-3 font-mobile-small fw-medium">
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fef2f2; border: 1px solid #fca5a5; border-radius: 4px;"></span>
                            <span class="text-danger"><%= GetResourceText("OVERDUE") %></span>
                        </div>
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fffbeb; border: 1px solid #fcd34d; border-radius: 4px;"></span>
                            <span class="text-warning text-dark"><%= GetResourceText("DUE_SOON") %></span>
                        </div>
                    </div>
                    
                    <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
                </div>
                
            </div>

            <!-- BẢNG DỮ LIỆU ĐÃ ĐƯỢC ÉP FULL WIDTH BẰNG W-100 -->
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="false" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table table-bordered table-task-grid table-hover align-middle w-100"
                IsEnableSelectColumn="false" IsEnableIndex="false"
                ValueField="IdCongViec" DataNameField="TenCongViec" DataKeyNames="IdCongViec" GridLines="None"
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand" OnRowDataBound="grvData_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="TaskName" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtTaskName" 
                                CommandName="ITEM_DETAIL" 
                                CommandArgument='<%# Eval("IdCongViec") %>'
                                CssClass="text-decoration-none text-dark"
                                Visible='<%# this.IsView || this.IsEdit %>'>
                                <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="160px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtAssign" 
                                    CommandName="ASSIGN_TASK" 
                                    CommandArgument='<%# Eval("IdCongViec") %>' 
                                    CssClass='<%# this.IsEdit ? "btn-assign-task" : "btn-assign-task view-only" %>' 
                                    ToolTip='<%# GetResourceText(BackEndResourceKeys.PERSONEL_ASSIGNMENT) %>'
                                    Visible='<%# this.IsEdit || this.IsView %>'>
                                    <i class='<%# this.IsEdit ? "fas fa-plus" : "fas fa-user-friends" %>'></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Duration" HeaderStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("ThoiHanNgay") != DBNull.Value ? Eval("ThoiHanNgay") + " ngày" : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartDate" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayBatDau")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndDate" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayKetThuc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Priority" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetTaskPriorityBadge(Eval("TenDoUuTien"), Eval("DiemUuTien")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetTaskStatusBadge(Eval("TrangThai")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Dependent" HeaderStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold">
                        <ItemTemplate>
                            <%# GetPhuThuoc(Eval("IdCongViecPhuThuoc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                            </SweetSoft:SmartLinkButton>

                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                ButtonIcon="fas fa-trash">
                            </SweetSoft:SmartLinkButton>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtViewSchedule" CommandName="VIEW_SCHEDULE" CssClass="btn-grid-action text-decoration-none text-info me-2"
                                ResourceKey='<%# BackEndResourceKeys.VIEW %>' ButtonIcon="fas fa-calendar-alt">
                            </SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center p-3 text-muted">
                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            
            <SweetSoft:CtrlChonNhanVienTask runat="server" ID="CtrlChonNhanVienTask1" />
            <SweetSoft:CtrlXemNhanVienTask runat="server" ID="CtrlXemNhanVienTask1" />
        </ContentTemplate>
    </asp:UpdatePanel>
<!-- MODAL XEM LỊCH BIỂU TASK (GIAO DIỆN LỊCH MINI THÁNG CHUẨN XỊN) -->
<SweetSoft:ExtraModal
    runat="server"
    ID="mdlTaskSchedule"
    Type="Primary"
    Size="Large"
    FooterButtonClose="true">
    <ContentTemplate>
        <asp:UpdatePanel
            ID="upnlTaskSchedule"
            runat="server"
            UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-2">
                    <div style="font-size: 13px; color: #1e40af; background: #eff6ff; padding: 10px 12px; border-radius: 6px; border: 1px solid #bfdbfe; margin-bottom: 12px;">
                        <i class="fas fa-calendar-alt me-1"></i>
                        <%= GetResourceText(BackEndResourceKeys.EXECUTION_TIME) %>:
                        <strong>
                            <asp:Literal ID="ltrScheduleTaskName" runat="server"></asp:Literal>
                        </strong>
                    </div>
                    
                    <asp:HiddenField ID="hdfSingleTaskScheduleJson" runat="server" />
                    
                    <!-- VÙNG CHỨA LỊCH MINI THÁNG -->
                    <div class="mini-cal-wrap" style="padding: 5px 5px 20px 5px;">
                        <div class="mini-cal" id="task-schedule-minical"></div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};

    $(document).ready(function () {
        var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

        function pad(n) { return n < 10 ? '0' + n : '' + n; }
        function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
        function parseKey(k) { var p = k.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }
        function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }
        function titleText(st) { return 'Tháng ' + (st.month + 1) + ', ' + st.year; }

        // HÀM XÂY DỰNG 42 Ô LỊCH THÁNG KÈM THEO TOOLTIP THÔNG MINH
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
                    if (info.text) {
                        var label = String(info.text);
                        extra = '<span class="mc-label">' + escAttr(label) + '</span>';
                    }
                    // Cấy ghép Tooltip vào ô ngày nếu có task chi tiết
                    if (info.tasks && info.tasks.length > 0) {
                        clickAttr = ' onclick="CMSMasterJs.PinTooltip(this, event)" style="cursor: pointer;"';
                        tooltipHtml = '<div class="custom-task-tooltip"><ul class="tooltip-task-list">';
                        for (var tIdx = 0; tIdx < info.tasks.length; tIdx++) {
                            tooltipHtml += '<li><span class="t-code">[' + info.tasks[tIdx].code + ']</span>' + info.tasks[tIdx].name + '</li>';
                        }
                        tooltipHtml += '</ul></div>';
                    }
                }

                html += '<div class="' + cls + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + tooltipHtml + '</div>';
            }
            return html + '</div>';
        }

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

        CMSMasterJs.ChangeScheduleMonth = function (btnElement, delta) {
            var cal = $(btnElement).closest('.mini-cal');
            var st = cal.data('mc');
            if (!st) return;

            var viewport = cal.find('.mc-viewport');
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

        // HÀM KHỞI TẠO LỊCH KHI MODAL MỞ
        CMSMasterJs.RenderSingleTaskSchedule = function () {
            var cal = $('#task-schedule-minical');
            cal.empty();

            var jsonString = $('#<%= hdfSingleTaskScheduleJson.ClientID %>').val();
            if (!jsonString) return;

            try {
                var decodedJson = $('<textarea/>').html(jsonString).text();
                var data = JSON.parse(decodedJson) || {};

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
            } catch (e) {
                console.error("Lỗi vẽ Lịch Mini Task: ", e);
            }
        };

        // HÀM GHIM TOOLTIP KHI CLICK
        CMSMasterJs.PinTooltip = function (element, event) {
            event.stopPropagation();
            var isPinned = $(element).hasClass('show-tooltip');
            $('.mc-day').removeClass('show-tooltip');
            if (!isPinned) $(element).addClass('show-tooltip');
        };

        // CLICK RA NGOÀI ĐỂ TẮT TOOLTIP
        $(document).on('click', function () {
            $('.mc-day').removeClass('show-tooltip');
        });
</script>
</div>
