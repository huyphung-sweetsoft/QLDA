<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="LichCaNhan.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.LichCaNhan" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* CSS CONTROL ĐIỀU HƯỚNG */
        .cal-toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px; }
        .cal-nav-group { display: flex; gap: 5px; }
        .cal-title { font-size: 20px; font-weight: 700; color: #1e293b; margin: 0; min-width: 200px; text-align: center; }
        .btn-cal { background: #ffffff; border: 1px solid #cbd5e1; padding: 6px 14px; border-radius: 6px; font-weight: 600; font-size: 13px; color: #334155; cursor: pointer; transition: all 0.2s; text-decoration: none; }
        .btn-cal:hover { background: #f1f5f9; border-color: #94a3b8; color: #1e293b; }
        .btn-cal.active { background: #2563eb; color: #ffffff; border-color: #2563eb; }

        /* LƯỚI LỊCH (GRID) */
        .cal-header-row { display: grid; grid-template-columns: repeat(7, 1fr); background: #f8fafc; border: 1px solid #e2e8f0; border-bottom: none; border-radius: 8px 8px 0 0; }
        .cal-header-cell { 
            padding: 10px; text-align: center; font-weight: 700; color: #475569; 
            font-size: 13px; border-right: 1px solid #e2e8f0; 
            min-width: 0; 
        }
        .cal-header-cell:last-child { border-right: none; }      
        
        .cal-grid { display: grid; grid-template-columns: repeat(7, 1fr); border: 1px solid #e2e8f0; border-radius: 0 0 8px 8px; overflow: hidden; }      
        .cal-cell { 
            min-height: 100px; padding: 6px; border-right: 1px solid #e2e8f0; 
            border-bottom: 1px solid #e2e8f0; background: #ffffff; transition: background 0.2s; 
            min-width: 0; 
        }
        .cal-cell:nth-child(7n) { border-right: none; }
        .cal-cell.is-clickable { cursor: pointer; }
        .cal-cell.is-clickable:hover { background: #f8fafc; box-shadow: inset 0 0 0 2px #bfdbfe; }
        
        .date-number { font-size: 14px; font-weight: 700; color: #1e293b; margin-bottom: 4px; display: inline-block; padding: 2px 6px; border-radius: 4px; }
        .date-number.is-today { background: #2563eb; color: #ffffff; }
        .date-number.other-month { color: #94a3b8; }

        /* MÀU TRẠNG THÁI LỊCH */
        .status-badge { font-size: 11px; font-weight: 600; padding: 4px 8px; border-radius: 4px; display: flex; align-items: center; gap: 4px; margin-bottom: 4px; line-height: 1.2; }
        .status-weekend { background: #f1f5f9; color: #64748b; }
        .status-holiday { background: #fef3c7; color: #b45309; border-left: 3px solid #f59e0b; }
        .status-busy { background: #fee2e2; color: #dc2626; border-left: 3px solid #ef4444; }
        .status-free { background: #dcfce7; color: #15803d; border-left: 3px solid #22c55e; }

        /* VIEW TUẦN - TASK LIST RÚT GỌN */
        .task-list-mini { list-style: none; padding: 0; margin: 0; }
        .task-item-mini { font-size: 11px; color: #334155; background: #f8fafc; border: 1px solid #e2e8f0; padding: 3px 5px; border-radius: 3px; margin-bottom: 3px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upCalendar" runat="server">
        <ContentTemplate>
            <SweetSoft:Navigation ID="Navigation1" runat="server"/>

            <div class="card p-3 shadow-sm border-0">
                <asp:HiddenField ID="hfScheduleDataJSON" runat="server" />
                <asp:HiddenField ID="hfViewMode" runat="server" Value="month" />
                <asp:HiddenField ID="hfCurrentDate" runat="server" />

                <!-- TOOLBAR ĐIỀU HƯỚNG -->
                <div class="cal-toolbar">
                    <div class="cal-nav-group">
                        <asp:LinkButton ID="btnPrev" runat="server" CssClass="btn-cal" OnClick="btnPrev_Click"><i class="fas fa-chevron-left me-1"></i> <%= GetResourceText(BackEndResourceKeys.PREVIOUS) %></asp:LinkButton>
                        <asp:LinkButton ID="btnToday" runat="server" CssClass="btn-cal" OnClick="btnToday_Click"><%= GetResourceText(BackEndResourceKeys.TODAY) %></asp:LinkButton>
                        <asp:LinkButton ID="btnNext" runat="server" CssClass="btn-cal" OnClick="btnNext_Click"><%= GetResourceText(BackEndResourceKeys.NEXT) %> <i class="fas fa-chevron-right ms-1"></i></asp:LinkButton>
                    </div>
                
                <div class="text-center">
                <h2 class="cal-title"><asp:Literal ID="litTitle" runat="server"></asp:Literal></h2>
                <h3 class="cal-title" style="font-size: 16px; color:#64748b;"><asp:Literal ID="litDateRange" runat="server"></asp:Literal></h3>
                </div>

                    <div class="cal-nav-group">
                        <asp:LinkButton ID="btnViewMonth" runat="server" CssClass="btn-cal" OnClick="btnViewMonth_Click"><%= GetResourceText(BackEndResourceKeys.MONTH) %></asp:LinkButton>
                        <asp:LinkButton ID="btnViewWeek" runat="server" CssClass="btn-cal" OnClick="btnViewWeek_Click"><%= GetResourceText(BackEndResourceKeys.WEEK) %></asp:LinkButton>
                    </div>
                </div>

                <!-- LƯỚI LỊCH -->
                <div class="cal-header-row">
                    <div class="cal-header-cell"><%= GetResourceText(BackEndResourceKeys.MONDAY) %></div>
                    <div class="cal-header-cell"><%= GetResourceText(BackEndResourceKeys.TUESDAY) %></div>
                    <div class="cal-header-cell"><%= GetResourceText(BackEndResourceKeys.WEDNESDAY) %></div>
                    <div class="cal-header-cell"><%= GetResourceText(BackEndResourceKeys.THURSDAY) %></div>
                    <div class="cal-header-cell"><%= GetResourceText(BackEndResourceKeys.FRIDAY) %></div>
                    <div class="cal-header-cell text-danger"><%= GetResourceText(BackEndResourceKeys.SATURDAY) %></div>
                    <div class="cal-header-cell text-danger"><%= GetResourceText(BackEndResourceKeys.SUNDAY) %></div>
                </div>
                <div id="calendarGrid" class="cal-grid"></div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <!-- ĐÃ FIX: Bọc nút ngầm trong UpdatePanel để chặn load lại toàn trang (Biến thành AJAX) -->
    <asp:UpdatePanel ID="upHiddenAction" runat="server">
        <ContentTemplate>
            <div style="display:none;">
                <asp:Button ID="btnOpenModalDay" runat="server" OnClick="btnOpenModalDay_Click" />
                <asp:HiddenField ID="hfSelectedDateIndex" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- CHUẨN EXTRAMODAL HỆ THỐNG -->
    <SweetSoft:ExtraModal runat="server" ID="mdlDayDetail" Type="Primary" Size="Large" FooterButtonClose="true">
        <ContentTemplate>
            <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <!-- C# sẽ nhét nội dung Dự án / Lễ tết vào thẻ Literal này -->
                    <div style="padding: 15px; max-height: 65vh; overflow-y: auto; background-color: #f8fafc; border-radius: 8px;">
                        <asp:Literal ID="litModalContent" runat="server"></asp:Literal>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        // [CHUYỂN GIAO ĐA NGÔN NGỮ CHO JAVASCRIPT]
        var calLang = {
            holiday: '<%= GetResourceText(BackEndResourceKeys.HOLIDAY) %>',
            busy: '<%= GetResourceText(BackEndResourceKeys.WORKING) %>', 
            weekend: '<%= GetResourceText(BackEndResourceKeys.WEEKEND) %>',
            free: '<%= GetResourceText(BackEndResourceKeys.FREE) %>',
            andOther: '<%= GetResourceText(BackEndResourceKeys.AND) %>',
            otherTasks: '<%= GetResourceText(BackEndResourceKeys.OTHER_TASKS) %>'
        };

        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () { RenderCalendar(); });
        $(document).ready(function () { RenderCalendar(); });

        var scheduleDataGlobal = [];

        function RenderCalendar() {
            var jsonStr = $('#<%= hfScheduleDataJSON.ClientID %>').val();
            if (!jsonStr) return;
        
            scheduleDataGlobal = JSON.parse(jsonStr);
            var viewMode = $('#<%= hfViewMode.ClientID %>').val();
            var $grid =$('#calendarGrid');
            $grid.empty();

            var todayStr = new Date().toISOString().split('T')[0];

            $.each(scheduleDataGlobal, function (index, dayData) {
                var dateObj = new Date(dayData.Ngay);
                var isToday = (dayData.Ngay.split('T')[0] === todayStr);

                // Trạng thái hiển thị (Dùng Dictionary Đa ngôn ngữ)
                var statusClass = ""; var statusText = ""; var icon = "";
                if (dayData.TrangThaiLich === "holiday") { statusClass = "status-holiday"; statusText = calLang.holiday; icon = "🎈"; }
                else if (dayData.TrangThaiLich === "busy") { statusClass = "status-busy"; statusText = calLang.busy; icon = "🔥"; }
                else if (dayData.TrangThaiLich === "weekend") { statusClass = "status-weekend"; statusText = calLang.weekend; icon = "☕"; }
                else { statusClass = "status-free"; statusText = calLang.free; icon = "✔️"; }

                // Tính năng Clickable
                var clickAttr = dayData.ChoPhepClick ? `onclick="OpenDayModal(${index})"` : "";
                var clickClass = dayData.ChoPhepClick ? "is-clickable" : "";

                // Html cho View Tuần (Hiện list task nhỏ)
                var taskHtml = "";
                if (viewMode === "week" && dayData.TrangThaiLich === "busy" && dayData.DanhSachCongViec) {
                    taskHtml += "<ul class='task-list-mini mt-1'>";
                    var limit = Math.min(dayData.DanhSachCongViec.length, 3);
                    for (var i = 0; i < limit; i++) {
                        taskHtml += `<li class="task-item-mini" title="${dayData.DanhSachCongViec[i].TenCongViec}">` +
                            `[${dayData.DanhSachCongViec[i].MaDuAn}] ${dayData.DanhSachCongViec[i].TenCongViec}</li>`;
                    }
                    if (dayData.DanhSachCongViec.length > 3) {
                        taskHtml += `<li class="task-item-mini fw-bold text-center text-muted">... ${calLang.andOther} ${dayData.DanhSachCongViec.length - 3} ${calLang.otherTasks}</li>`;
                    }
                    taskHtml += "</ul>";
                }

                // Dựng ô ngày
                var html = `
                    <div class="cal-cell ${clickClass}" ${clickAttr}>
                        <span class="date-number ${isToday ? 'is-today shadow-sm' : ''}">${dateObj.getDate()}</span>
                        <div class="status-badge ${statusClass}">${icon} ${statusText}</div>
                        ${taskHtml}
                    </div>
                `;
                $grid.append(html);
            });
        }

        // HÀM MỞ MODAL ĐÃ FIX: TRUYỀN ID CHO C# XỬ LÝ
        function OpenDayModal(dataIndex) {
            // Lưu index của ngày được chọn vào HiddenField
            $('#<%= hfSelectedDateIndex.ClientID %>').val(dataIndex);
            
            // Kích hoạt nút ngầm để PostBack về Server (gọi hàm btnOpenModalDay_Click)
            $('#<%= btnOpenModalDay.ClientID %>').click();
        }
    </script>
</asp:Content>