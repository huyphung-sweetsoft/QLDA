<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="LichCaNhan.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.LichCaNhan" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* CSS CONTROL ĐIỀU HƯỚNG */
        .cal-toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px; }
        .cal-nav-group { display: flex; gap: 5px; }
        .cal-title { font-size: 20px; font-weight: 700; color: #1e293b; margin: 0; min-width: 200px; text-align: center; }
        .btn-cal { background: #ffffff; border: 1px solid #cbd5e1; padding: 6px 14px; border-radius: 6px; font-weight: 600; font-size: 13px; color: #334155; cursor: pointer; transition: all 0.2s; }
        .btn-cal:hover { background: #f1f5f9; border-color: #94a3b8; }
        .btn-cal.active { background: #2563eb; color: #ffffff; border-color: #2563eb; }

        /* LƯỚI LỊCH (GRID) */
        .cal-header-row { display: grid; grid-template-columns: repeat(7, 1fr); background: #f8fafc; border: 1px solid #e2e8f0; border-bottom: none; border-radius: 8px 8px 0 0; }
        .cal-header-cell { padding: 10px; text-align: center; font-weight: 700; color: #475569; font-size: 13px; border-right: 1px solid #e2e8f0; }
        .cal-header-cell:last-child { border-right: none; }
        
        .cal-grid { display: grid; grid-template-columns: repeat(7, 1fr); border: 1px solid #e2e8f0; border-radius: 0 0 8px 8px; overflow: hidden; }
        .cal-cell { min-height: 100px; padding: 6px; border-right: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; background: #ffffff; transition: background 0.2s; }
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

        /* MODAL LIST */
        .proj-group-title { font-size: 13px; font-weight: 800; color: #1e3a8a; background: #eff6ff; padding: 6px 10px; border-radius: 4px; margin-top: 10px; margin-bottom: 5px; }
                /* ================= CSS CHO MODAL CHI TIẾT ================= */
        .modal-overlay {
            display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background-color: rgba(15, 23, 42, 0.65); backdrop-filter: blur(2px);
            z-index: 9999; justify-content: center; align-items: center; padding: 20px;
        }
        .modal-overlay.active { display: flex; }
        .modal-card {
            background: #ffffff; border-radius: 10px; box-shadow: 0 15px 30px rgba(0, 0, 0, 0.2);
            display: flex; flex-direction: column; overflow: hidden; border: 1px solid #e2e8f0;
        }
        .modal-header-sweet {
            background: linear-gradient(135deg, #4c1d95, #6f42c1); color: #ffffff;
            padding: 14px 20px; display: flex; justify-content: space-between; align-items: center;
        }
        .modal-header-sweet h3 { font-size: 15px; font-weight: 700; margin: 0; }
        .modal-header-sweet button { background: none; border: none; color: #ffffff; font-size: 18px; cursor: pointer; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upCalendar" runat="server">
        <ContentTemplate>
            <div class="card p-3">
                <asp:HiddenField ID="hfScheduleDataJSON" runat="server" />
                <asp:HiddenField ID="hfViewMode" runat="server" Value="month" />
                <asp:HiddenField ID="hfCurrentDate" runat="server" />

                <!-- TOOLBAR ĐIỀU HƯỚNG -->
                <div class="cal-toolbar">
                    <div class="cal-nav-group">
                        <asp:LinkButton ID="btnPrev" runat="server" CssClass="btn-cal" OnClick="btnPrev_Click">◀ Trước</asp:LinkButton>
                        <asp:LinkButton ID="btnToday" runat="server" CssClass="btn-cal" OnClick="btnToday_Click">Hôm nay</asp:LinkButton>
                        <asp:LinkButton ID="btnNext" runat="server" CssClass="btn-cal" OnClick="btnNext_Click">Sau ▶</asp:LinkButton>
                    </div>
                    
                    <h2 class="cal-title"><asp:Literal ID="litTitle" runat="server"></asp:Literal></h2>
                    <h3 class="cal-title" style="font-size: 16px; color:#64748b;"><asp:Literal ID="litDateRange" runat="server"></asp:Literal></h3>

                    <div class="cal-nav-group">
                        <asp:LinkButton ID="btnViewMonth" runat="server" CssClass="btn-cal" OnClick="btnViewMonth_Click">Tháng</asp:LinkButton>
                        <asp:LinkButton ID="btnViewWeek" runat="server" CssClass="btn-cal" OnClick="btnViewWeek_Click">Tuần</asp:LinkButton>
                    </div>
                </div>

                <!-- LƯỚI LỊCH (Được Javascript vẽ vào đây) -->
                <div class="cal-header-row">
                    <div class="cal-header-cell">Thứ 2</div><div class="cal-header-cell">Thứ 3</div>
                    <div class="cal-header-cell">Thứ 4</div><div class="cal-header-cell">Thứ 5</div>
                    <div class="cal-header-cell">Thứ 6</div><div class="cal-header-cell text-danger">Thứ 7</div>
                    <div class="cal-header-cell text-danger">Chủ nhật</div>
                </div>
                <div id="calendarGrid" class="cal-grid"></div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <!-- MODAL CHI TIẾT NGÀY (Chuẩn SweetSoft) -->
    <div id="modalDayDetail" class="modal-overlay">
        <div class="modal-card" style="width: 650px; max-width: 95vw;">
            <div class="modal-header-sweet">
                <h3>
                    <i class="far fa-calendar-alt me-2"></i>
                    <span id="modalTitle">Chi tiết Lịch trình</span>
                </h3>
                <button type="button" onclick="closeDayModal()">✕</button>
            </div>
            
            <div class="modal-body-sweet" id="modalBodyContent" style="padding: 20px; max-height: 70vh; overflow-y: auto; background-color: #f8fafc;">
                <!-- JS sẽ nhét nội dung Dự án / Lễ tết vào đây -->
            </div>
            
            <div class="p-3 border-top bg-light d-flex justify-content-end gap-2">
                <button type="button" class="btn btn-secondary" onclick="closeDayModal()">Đóng</button>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        // Gắn sự kiện để vẽ lại lịch mỗi khi UpdatePanel load xong (Bấm Next/Prev)
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () { RenderCalendar(); });
        $(document).ready(function () { RenderCalendar(); });

        var scheduleDataGlobal = []; // Biến toàn cục lưu JSON để Modal đọc

        function RenderCalendar() {
            var jsonStr = $('#<%= hfScheduleDataJSON.ClientID %>').val();
            if (!jsonStr) return;
            
            scheduleDataGlobal = JSON.parse(jsonStr);
            var viewMode = $('#<%= hfViewMode.ClientID %>').val();
            var $grid = $('#calendarGrid');
            $grid.empty();

            var todayStr = new Date().toISOString().split('T')[0];

            $.each(scheduleDataGlobal, function (index, dayData) {
                var dateObj = new Date(dayData.Ngay);
                var isToday = (dayData.Ngay.split('T')[0] === todayStr);

                // Trạng thái hiển thị
                var statusClass = ""; var statusText = ""; var icon = "";
                if (dayData.TrangThaiLich === "holiday") { statusClass = "status-holiday"; statusText = "Nghỉ lễ"; icon = "🎈"; }
                else if (dayData.TrangThaiLich === "busy") { statusClass = "status-busy"; statusText = "Có công việc"; icon = "🔥"; }
                else if (dayData.TrangThaiLich === "weekend") { statusClass = "status-weekend"; statusText = "Cuối tuần"; icon = "☕"; }
                else { statusClass = "status-free"; statusText = "Trống"; icon = "✔️"; }

                // Tính năng Clickable
                var clickAttr = dayData.ChoPhepClick ? `onclick="OpenDayModal(${index})"` : "";
                var clickClass = dayData.ChoPhepClick ? "is-clickable" : "";

                // Html cho View Tuần (Hiện list task nhỏ)
                var taskHtml = "";
                if (viewMode === "week" && dayData.TrangThaiLich === "busy" && dayData.DanhSachCongViec) {
                    taskHtml += "<ul class='task-list-mini'>";
                    var limit = Math.min(dayData.DanhSachCongViec.length, 3);
                    for (var i = 0; i < limit; i++) {
                        taskHtml += `<li class="task-item-mini" title="${dayData.DanhSachCongViec[i].TenCongViec}">` +
                            `[${dayData.DanhSachCongViec[i].MaDuAn}] ${dayData.DanhSachCongViec[i].TenCongViec}</li>`;
                    }
                    if (dayData.DanhSachCongViec.length > 3) {
                        taskHtml += `<li class="task-item-mini fw-bold text-center">... và ${dayData.DanhSachCongViec.length - 3} việc khác</li>`;
                    }
                    taskHtml += "</ul>";
                }

                // Dựng ô ngày
                var html = `
                    <div class="cal-cell ${clickClass}" ${clickAttr}>
                        <span class="date-number ${isToday ? 'is-today' : ''}">${dateObj.getDate()}</span>
                        <div class="status-badge ${statusClass}">${icon} ${statusText}</div>
                        ${taskHtml}
                    </div>
                `;
                $grid.append(html);
            });
        }

        // Thêm hàm Đóng Modal và bắt sự kiện phím ESC
        function closeDayModal() {
            $('#modalDayDetail').removeClass('active');
        }

        $(document).on('keydown', function (e) {
            if (e.key === "Escape" && $('#modalDayDetail').hasClass('active')) {
                closeDayModal();
            }
        });

        // Hàm Mở Modal đã fix
        function OpenDayModal(dataIndex) {
            var dayData = scheduleDataGlobal[dataIndex];
            var dateObj = new Date(dayData.Ngay);

            // Format ngày chuẩn VN (DD/MM/YYYY)
            var dd = String(dateObj.getDate()).padStart(2, '0');
            var mm = String(dateObj.getMonth() + 1).padStart(2, '0');
            var yyyy = dateObj.getFullYear();
            var dateStr = dd + '/' + mm + '/' + yyyy;

            $('#modalTitle').text("Chi tiết lịch - Ngày " + dateStr);
            var $body = $('#modalBodyContent');
            $body.empty();

            if (dayData.TrangThaiLich === "holiday") {
                $body.html(`
            <div style="background-color: #fef3c7; color: #b45309; padding: 20px; border-radius: 8px; border-left: 5px solid #f59e0b; text-align: center; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
                <h4 style="margin: 0 0 10px 0; font-weight: 800;">🎈 LỊCH NGHỈ LỄ</h4>
                <p style="margin: 0; font-size: 16px; font-weight: 600;">${dayData.TenNgoaiLe}</p>
            </div>
        `);
            }
            else if (dayData.TrangThaiLich === "busy") {
                // Nhóm Task theo Dự án
                var projGroups = {};
                $.each(dayData.DanhSachCongViec, function (i, task) {
                    var pName = task.TenDuAn ? task.TenDuAn : 'Dự án khác / Không xác định';
                    if (!projGroups[pName]) projGroups[pName] = [];
                    projGroups[pName].push(task);
                });

                var html = "";
                for (var projName in projGroups) {
                    html += `<div style="background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); overflow: hidden;">`;

                    // Header Dự án
                    html += `<div style="background: linear-gradient(to right, #eff6ff, #ffffff); border-bottom: 1px solid #e2e8f0; padding: 10px 15px; color: #1e3a8a; font-weight: 800; font-size: 14px;">
                        <i class="fas fa-folder-open me-2"></i> ${projName}
                     </div>`;

                    // Danh sách Task
                    html += `<div style="padding: 10px 15px;">`;
                    $.each(projGroups[projName], function (i, task) {
                        html += `<div style="padding: 8px 0; border-bottom: 1px dashed #cbd5e1; font-size: 13px; color: #334155; display: flex; align-items: flex-start; gap: 8px;">
                            <span style="background: #e0f2fe; color: #0284c7; padding: 2px 6px; border-radius: 4px; font-weight: 700; font-size: 11px; white-space: nowrap;">
                                ${task.MaCongViec}
                            </span>
                            <span style="font-weight: 600; line-height: 1.4;">${task.TenCongViec}</span>
                         </div>`;
                    });
                    html += `</div></div>`;
                }
                $body.html(html);
            }

            // Gọi hàm mở Overlay chuẩn của công ty ông
            $('#modalDayDetail').addClass('active');
        }
    </script>
</asp:Content>