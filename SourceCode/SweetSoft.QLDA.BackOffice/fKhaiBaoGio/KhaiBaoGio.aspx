<%@ Page Title="Khai báo giờ làm" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="KhaiBaoGio.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fKhaiBaoGio.KhaiBaoGio" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .ks-page {
            --ks-primary: #5b21b6;
            --ks-primary-soft: #f3efff;
            --ks-border: #e2e8f0;
            --ks-text: #1e293b;
            --ks-muted: #64748b;
            --ks-ok: #15803d;
            --ks-ok-soft: #dcfce7;
            --ks-warn: #b45309;
            --ks-warn-soft: #fef3c7;
            --ks-danger: #b91c1c;
            --ks-danger-soft: #fee2e2;
            font-family: "Nunito", "Segoe UI", Arial, sans-serif;
            font-size: 13px;
            color: var(--ks-text);
            max-width: 1230px;
            margin: 0 auto;
            padding: 8px 4px 28px;
        }
        .ks-page * { box-sizing: border-box; }
        .ks-hidden { display: none !important; }

        /* ---- Đầu trang ---- */
        .ks-header { display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 14px; }
        .ks-header-left { display: flex; align-items: center; gap: 12px; }
        .ks-avatar { width: 44px; height: 44px; border-radius: 50%; background: var(--ks-primary); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 16px; }
        .ks-hello { font-size: 17px; font-weight: 700; color: #4b168c; }
        .ks-subtitle { color: var(--ks-muted); margin-top: 2px; }
        .ks-link { color: var(--ks-primary); font-weight: 600; text-decoration: none; padding: 6px 12px; border: 1px solid var(--ks-border); border-radius: 8px; background: #fff; }
        .ks-link:hover { background: var(--ks-primary-soft); }

        /* ---- Thẻ tổng hợp ngày làm việc ---- */
        .ks-day-card { background: #fff; border: 1px solid var(--ks-border); border-top: 4px solid #94a3b8; border-radius: 12px; padding: 16px 18px; margin-bottom: 14px; box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04); }
        .ks-day-card.is-open { border-top-color: var(--ks-primary); box-shadow: 0 4px 14px rgba(91, 33, 182, 0.08); }
        .ks-day-card.is-done { border-top-color: var(--ks-ok); }
        .ks-day-card.is-missed { border-top-color: var(--ks-danger); }

        .ks-day-top { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
        .ks-day-title { font-weight: 700; color: #4b168c; font-size: 15px; display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
        .ks-day-hours { color: var(--ks-text); font-weight: 600; background: #f1f5f9; padding: 2px 10px; border-radius: 6px; font-size: 12.5px; }
        .ks-day-lunch { color: var(--ks-muted); font-size: 12px; font-weight: 500; }

        .ks-chip { display: inline-flex; align-items: center; gap: 5px; padding: 4px 11px; border-radius: 999px; font-size: 11.5px; font-weight: 700; border: 1px solid transparent; background: #f1f5f9; color: #475569; }
        .ks-chip.is-open { background: var(--ks-primary-soft); color: var(--ks-primary); border-color: #ddd0f7; }
        .ks-chip.is-lunch { background: var(--ks-warn-soft); color: var(--ks-warn); border-color: #fde68a; }
        .ks-chip.is-done { background: var(--ks-ok-soft); color: var(--ks-ok); }
        .ks-chip.is-missed { background: var(--ks-danger-soft); color: var(--ks-danger); }

        .ks-day-stats { display: flex; align-items: baseline; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 8px; }
        .ks-day-total-big { font-size: 23px; font-weight: 800; color: var(--ks-primary); font-variant-numeric: tabular-nums; }
        .ks-day-total-sub { font-size: 13px; color: var(--ks-muted); font-weight: 600; margin-left: 6px; }
        .ks-day-countdown { color: var(--ks-muted); font-size: 12.5px; }
        .ks-day-countdown b { color: var(--ks-text); font-variant-numeric: tabular-nums; }

        .ks-progress { height: 9px; border-radius: 999px; background: #eef2f7; overflow: hidden; }
        .ks-progress-bar { height: 100%; background: linear-gradient(90deg, #6d28d9, #4f46e5); border-radius: 999px; transition: width .3s ease; }
        .ks-day-card.is-done .ks-progress-bar { background: var(--ks-ok); }
        .ks-day-card.is-missed .ks-progress-bar { background: var(--ks-danger); }

        /* ---- Thông báo nghỉ trưa / cảnh báo ---- */
        .ks-banner-lunch { display: none; align-items: center; gap: 10px; margin: 10px 14px 0; padding: 10px 14px; border-radius: 8px; background: var(--ks-warn-soft); border: 1px solid #fcd34d; color: var(--ks-warn); font-weight: 600; }
        .ks-warn { display: none; align-items: center; gap: 8px; margin: 10px 14px 0; padding: 9px 12px; border-radius: 8px; background: #fef2f2; border: 1px solid #fecaca; color: var(--ks-danger); font-weight: 600; }

        /* ---- Khung danh sách Task ---- */
        .ks-panel { background: #fff; border: 1px solid var(--ks-border); border-radius: 12px; overflow: hidden; margin-bottom: 12px; }
        .ks-panel-head { padding: 12px 16px; background: var(--ks-primary-soft); border-bottom: 1px solid var(--ks-border); display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 8px; }
        .ks-panel-title { font-weight: 700; color: #4b168c; font-size: 14px; }
        .ks-panel-sub { color: var(--ks-muted); margin-top: 2px; font-size: 12.5px; }
        .ks-panel-body { padding: 10px 14px 14px; background: #f8fafc; }
        .ks-group-title { margin: 10px 0 6px; font-weight: 700; font-size: 12px; text-transform: uppercase; letter-spacing: .03em; color: #475569; }
        .ks-group-title.is-overdue { color: var(--ks-danger); }

        /* ---- Dòng Task & Đồng hồ bấm giờ ---- */
        .ks-task { display: flex; align-items: center; justify-content: space-between; gap: 16px; background: #fff; border: 1px solid var(--ks-border); border-left: 4px solid #60a5fa; border-radius: 10px; padding: 12px 14px; margin-bottom: 8px; transition: box-shadow .2s, border-color .2s; }
        .ks-task.is-overdue { border-left-color: #ef4444; }
        .ks-task.is-extra { border-left-color: #94a3b8; opacity: .85; }
        .ks-task.is-running { border-color: #8b5cf6; border-left: 5px solid var(--ks-primary); background: #fdfcff; box-shadow: 0 4px 14px rgba(91, 33, 182, 0.12); }

        .ks-task-info { min-width: 0; flex: 1 1 auto; }
        .ks-kicker { font-size: 12px; color: var(--ks-muted); font-weight: 600; }
        .ks-task-name { font-weight: 700; font-size: 14px; margin: 3px 0 5px; word-break: break-word; color: var(--ks-text); }
        .ks-task-meta { color: var(--ks-muted); font-size: 12px; display: flex; flex-wrap: wrap; gap: 6px 12px; align-items: center; }

        .ks-badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 11px; font-weight: 600; background: #e0f2fe; color: #0369a1; }
        .ks-badge.is-todo { background: #f1f5f9; color: #475569; }
        .ks-badge.is-overdue { background: #fee2e2; color: var(--ks-danger); }
        .ks-badge.is-clock-running { background: #e7f7ed; color: #14753a; }
        .ks-badge.is-clock-paused { background: #fff4df; color: #9b5b0b; }

        /* ---- Cụm điều khiển Stopwatch bên phải Task ---- */
        .ks-task-side { flex: 0 0 auto; display: flex; flex-direction: column; align-items: flex-end; gap: 5px; min-width: 225px; }
        .ks-tracker-row { display: flex; align-items: center; gap: 10px; }

        .ks-timer-box { text-align: right; }
        .ks-stopwatch { font-family: "Consolas", "Courier New", monospace; font-size: 20px; font-weight: 800; color: var(--ks-text); letter-spacing: 0.5px; line-height: 1.1; font-variant-numeric: tabular-nums; }
        .ks-task.is-running .ks-stopwatch { color: var(--ks-primary); }
        .ks-decimal-hours { font-size: 11.5px; font-weight: 700; color: var(--ks-muted); margin-top: 2px; }
        .ks-task.is-running .ks-decimal-hours { color: var(--ks-ok); }

        .ks-action-btn { display: inline-flex; align-items: center; justify-content: center; gap: 6px; min-width: 108px; padding: 8px 14px; border-radius: 8px; font-size: 13px; font-weight: 700; border: 1px solid transparent; cursor: pointer; transition: all .15s ease; }
        .ks-action-btn.btn-start { background: var(--ks-primary); color: #fff; }
        .ks-action-btn.btn-start:hover:not(:disabled) { filter: brightness(1.1); transform: translateY(-1px); }
        .ks-action-btn.btn-resume { background: var(--ks-ok); color: #fff; }
        .ks-action-btn.btn-resume:hover:not(:disabled) { filter: brightness(1.08); transform: translateY(-1px); }
        .ks-action-btn.btn-stop { background: var(--ks-danger); color: #fff; box-shadow: 0 2px 8px rgba(185, 28, 28, 0.25); }
        .ks-action-btn.btn-stop:hover:not(:disabled) { filter: brightness(1.08); }
        .ks-action-btn:disabled { background: #e2e8f0 !important; color: #94a3b8 !important; border-color: #cbd5e1 !important; cursor: not-allowed; transform: none !important; box-shadow: none !important; }

        .ks-pulse-dot { width: 8px; height: 8px; border-radius: 50%; background: #fff; display: inline-block; animation: ksPulse 1.2s infinite; }
        @keyframes ksPulse {
            0%, 100% { opacity: 1; transform: scale(1); }
            50% { opacity: 0.35; transform: scale(0.8); }
        }

        .ks-hint { font-size: 12px; color: var(--ks-muted); text-align: right; }
        .ks-hint.is-late { color: var(--ks-danger); font-weight: 600; }
        .ks-readonly { font-size: 11.5px; color: var(--ks-muted); font-weight: 600; text-align: right; background: #f1f5f9; padding: 4px 10px; border-radius: 6px; }

        /* ---- Chân trang trạng thái ---- */
        .ks-footer { display: flex; align-items: center; justify-content: space-between; gap: 10px; flex-wrap: wrap; padding: 10px 14px; background: #fff; border: 1px solid var(--ks-border); border-radius: 12px; }
        .ks-save-status { font-weight: 600; color: var(--ks-muted); }
        .ks-save-status.is-ok { color: var(--ks-ok); }
        .ks-save-status.is-error { color: var(--ks-danger); }
        .ks-save-status.is-pending { color: var(--ks-primary); }

        .ks-empty { text-align: center; padding: 26px 14px; color: var(--ks-muted); background: #fff; border: 1px dashed var(--ks-border); border-radius: 12px; margin-bottom: 12px; }
        .ks-empty i { font-size: 26px; color: #a78bfa; display: block; margin-bottom: 8px; }
        .ks-empty b { color: var(--ks-text); }

        /* Nâng cấp nhẹ: tìm kiếm/lọc theo nhịp giao diện CtrlNhanViens, không tạo postback. */
        .ks-task.is-running { scroll-margin-top: 85px; }
        .ks-group-title.is-running-title { color: var(--ks-primary); font-size: 12.5px; }
        .ks-filter-bar { display:flex; align-items:center; flex-wrap:wrap; gap:8px; padding:11px 14px; background:#fff; border-bottom:1px solid var(--ks-border); }
        .ks-filter-select { flex:0 0 auto; min-width:134px; max-width:220px; height:36px; padding:6px 30px 6px 11px; background-color:#f4f0ff; border:1px solid #d9c7f4; border-radius:7px; color:#552095; font-size:12.5px; font-weight:600; font-family:inherit; cursor:pointer; }
        .ks-filter-select:focus,.ks-filter-input:focus { outline:2px solid rgba(91,33,182,.17); border-color:#7c3aed; }
        .ks-filter-search { min-width:190px; max-width:430px; flex:1 1 240px; display:flex; align-items:center; border:1px solid #a884d9; border-radius:7px; height:36px; background:#fff; overflow:hidden; }
        .ks-filter-input { width:100%; min-width:0; border:0; background:transparent; padding:7px 10px; font:inherit; font-size:12.5px; color:var(--ks-text); outline:0!important; }
        .ks-filter-input:focus { outline:0!important; }
        .ks-filter-search i { color:var(--ks-primary); padding:0 11px; }
        .ks-filter-clear { border:1px solid #dcd5ea; border-radius:7px; background:#fff; color:#5b21b6; font-weight:650; padding:7px 11px; min-height:36px; cursor:pointer; }
        .ks-filter-clear:hover { background:#f6f1ff; }
        .ks-filter-info { color:var(--ks-muted); font-size:12px; margin-left:auto; }
        .ks-filter-tags { display:flex; align-items:center; flex-wrap:wrap; gap:6px; padding:0 14px 9px; background:#fff; }
        .ks-filter-tags:empty { display:none; }
        .ks-filter-tag { display:inline-flex; align-items:center; gap:5px; border:1px solid #e1d4f3; border-radius:6px; padding:3px 8px; background:#f5f0fd; color:#5b218d; font-size:11.5px; }
        .ks-filter-tag button { border:0; background:transparent; color:inherit; padding:0 1px; cursor:pointer; font-size:15px; line-height:1; }
        .ks-filter-empty { display:none; padding:25px 15px; background:#fff; border:1px dashed #d7c7ec; border-radius:9px; text-align:center; color:var(--ks-muted); }
        .ks-journal-btn { border:1px solid #e0d3f1; border-radius:6px; background:#faf7ff; color:#5b21b6; font-size:11.5px; font-weight:650; padding:4px 8px; cursor:pointer; }
        .ks-journal-btn:hover { background:#eee5ff; }
        .ks-inline-notice { margin:10px 14px 0; padding:9px 12px; background:#f9f6ff; border:1px solid #e5dcf4; border-radius:8px; font-size:12.5px; }
        .ks-inline-notice:has(.ks-save-status:empty) { display:none; }
        .ks-save-status { display:block; line-height:1.45; }
        .ks-journal-mask[hidden] { display:none!important; }
        .ks-journal-mask { position:fixed; inset:0; background:rgba(22,16,40,.48); z-index:1085; display:flex; align-items:center; justify-content:center; padding:16px; }
        .ks-journal-dialog { width:min(660px,100%); max-height:calc(100vh - 32px); overflow:auto; background:#fff; border:1px solid #e0d6ed; border-radius:12px; box-shadow:0 22px 58px rgba(35,22,65,.25); font-family:"Nunito","Segoe UI",Arial,sans-serif; color:#263245; }
        .ks-journal-header { display:flex; justify-content:space-between; align-items:start; gap:12px; padding:15px 18px; border-bottom:1px solid #ece5f5; background:#f9f6ff; }
        .ks-journal-header h3 { color:#4b168c; font-size:16px; font-weight:750; margin:0 0 5px; }
        .ks-journal-header p { margin:0; color:#64748b; font-size:12px; }
        .ks-journal-close { border:0; width:30px; height:30px; border-radius:7px; background:#eee6f8; color:#5b21b6; font-size:20px; cursor:pointer; }
        .ks-journal-body { padding:16px 18px; }
        .ks-journal-table { width:100%; border-collapse:collapse; font-size:12.5px; }
        .ks-journal-table th { padding:9px 10px; color:#5b21b6; background:#f5f1fc; border-bottom:1px solid #e6ddf2; text-align:left; }
        .ks-journal-table td { padding:10px; border-bottom:1px solid #eff1f5; vertical-align:middle; }
        .ks-journal-total { display:flex; justify-content:space-between; align-items:center; gap:10px; margin-top:13px; padding:12px; border:1px solid #eee5f6; background:#faf7ff; border-radius:9px; font-size:12px; }
        .ks-journal-total strong { color:#51218a; font-size:17px; font-variant-numeric:tabular-nums; }
        .ks-journal-note { font-size:11.5px; color:#64748b; margin:9px 0 0; line-height:1.5; }
        .ks-hint-label { color:#64748b; }
        .ks-blocked-reason { background:#fff6e7; border-radius:6px; padding:4px 8px; color:#a15b0c; font-size:11.5px; text-align:right; }
        @media (max-width: 680px) {
            .ks-task { flex-direction: column; align-items: stretch; }
            .ks-task-side { align-items: flex-start; width: 100%; min-width: 0; border-top: 1px dashed var(--ks-border); padding-top: 10px; }
            .ks-tracker-row { width: 100%; justify-content: space-between; }
            .ks-timer-box, .ks-hint, .ks-readonly { text-align: left; }
            .ks-filter-select { flex:1 1 135px; max-width:none; }
            .ks-filter-search { flex-basis:100%; max-width:none; }
            .ks-filter-info { width:100%; margin-left:0; }
            .ks-journal-body { padding:10px; }
            .ks-journal-table th,.ks-journal-table td { padding:8px 5px; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="ks-page">

        <%-- ===== Đầu trang ===== --%>
        <div class="ks-header">
            <div class="ks-header-left">
                <div class="ks-avatar"><asp:Literal ID="ltrInitial" runat="server"></asp:Literal></div>
                <div>
                    <div class="ks-hello">Xin chào, <asp:Literal ID="ltrName" runat="server"></asp:Literal></div>
                    <div class="ks-subtitle"><asp:Literal ID="ltrDateText" runat="server"></asp:Literal> · Giờ hệ thống: <b id="ksClock">--:--:--</b></div>
                </div>
            </div>
            <a id="lnkDashboard" runat="server" class="ks-link"><i class="fas fa-chart-pie me-1"></i>Xem thống kê tổng quan</a>
        </div>

        <asp:Panel ID="pnlNotEmployee" runat="server" Visible="false" CssClass="ks-empty">
            <i class="fas fa-user-slash"></i>
            <b>Tài khoản này không phải tài khoản nhân viên.</b><br />
            Trang bấm giờ làm việc chỉ dành cho nhân viên.
        </asp:Panel>

        <%-- ===== Vùng chính: Thẻ tổng hợp ngày + Danh sách công việc ===== --%>
        <asp:UpdatePanel ID="upnlMain" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnTaskAction" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRefresh" EventName="Click" />
            </Triggers>
            <ContentTemplate>

                <asp:Panel ID="pnlDayOff" runat="server" Visible="false" CssClass="ks-empty">
                    <i class="fas fa-mug-hot"></i>
                    <asp:Literal ID="ltrDayOffText" runat="server"></asp:Literal>
                </asp:Panel>

                <%-- Thẻ tổng hợp thời gian làm việc cả ngày --%>
                <asp:Panel ID="pnlShifts" runat="server">
                    <asp:Repeater ID="rptShifts" runat="server" EnableViewState="false">
                        <ItemTemplate>
                            <div class="ks-day-card <%# ShiftCss((CaView)Container.DataItem) %>" id="ksDayCard">
                                <div class="ks-day-top">
                                    <div>
                                        <div class="ks-day-title">
                                            <span><i class="fas fa-business-time me-1"></i><%#: ((CaView)Container.DataItem).TenCa %></span>
                                            <span class="ks-day-hours"><%#: ((CaView)Container.DataItem).GioBatDau %> – <%#: ((CaView)Container.DataItem).GioKetThuc %></span>
                                        </div>
                                        <div class="ks-day-lunch mt-1">
                                            <%# ShiftLunchHtml((CaView)Container.DataItem) %>
                                        </div>
                                    </div>
                                    <span class="ks-chip <%# ShiftCss((CaView)Container.DataItem) %>" id="ksDayChip">
                                        <%#: ShiftChipText((CaView)Container.DataItem) %>
                                    </span>
                                </div>

                                <div class="ks-day-stats">
                                    <div>
                                        <span class="ks-day-total-big" id="ksDayTotalWatch"><%# KhaiBaoGioManager.FormatStopwatch(((CaView)Container.DataItem).TongGiayHienTai) %></span>
                                        <span class="ks-day-total-sub" id="ksDayTotalSummary">
                                            (<%# KhaiBaoGioManager.FormatReadableMinutes(((CaView)Container.DataItem).TongPhut) %> / <%# KhaiBaoGioManager.FormatReadableMinutes(((CaView)Container.DataItem).SoPhutCa) %>)
                                        </span>
                                    </div>
                                    <div class="ks-day-countdown"><%# ShiftMetaHtml((CaView)Container.DataItem) %></div>
                                </div>

                                <div class="ks-progress">
                                    <div class="ks-progress-bar" id="ksDayProgressBar" style="width: <%# ShiftPercent((CaView)Container.DataItem) %>%"></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <%-- Danh sách công việc hôm nay --%>
                <asp:Panel ID="pnlTasks" runat="server" CssClass="ks-panel">
                    <div class="ks-panel-head">
                        <div>
                            <div class="ks-panel-title"><i class="fas fa-list-check me-1"></i><asp:Literal ID="ltrTasksTitle" runat="server"></asp:Literal></div>
                            <div class="ks-panel-sub"><asp:Literal ID="ltrTasksSub" runat="server"></asp:Literal></div>
                        </div>
                    </div>

                    <div class="ks-banner-lunch" id="ksLunchBanner">
                        <i class="fas fa-utensils"></i>
                        <span>Đang trong khung giờ nghỉ trưa. Đồng hồ đang tạm ngưng cộng giờ và sẽ tự động đếm tiếp khi vào ca chiều.</span>
                    </div>

                    <div class="ks-warn" id="ksWarn">
                        <i class="fas fa-exclamation-triangle"></i>
                        <span>Hôm nay bạn chưa bấm giờ cho công việc nào. Hãy nhấn <b>Bắt đầu</b> vào công việc đang thực hiện.</span>
                    </div>

                    <div class="ks-inline-notice" role="status" aria-live="polite">
                        <asp:Label ID="lblSaveStatus" runat="server" CssClass="ks-save-status"></asp:Label>
                    </div>
                    <div class="ks-filter-bar" aria-label="Tìm kiếm và lọc công việc">
                        <select id="ksFilterStatus" class="ks-filter-select" aria-label="Lọc trạng thái tính giờ">
                            <option value="all">Tất cả trạng thái</option>
                            <option value="running">Đang chạy</option>
                            <option value="unstarted">Chưa bấm giờ</option>
                            <option value="recorded">Đã ghi nhận</option>
                            <option value="overdue">Quá hạn</option>
                        </select>
                        <select id="ksFilterProject" class="ks-filter-select" aria-label="Lọc dự án">
                            <option value="all">Tất cả dự án</option>
                        </select>
                        <label class="ks-filter-search">
                            <input id="ksSearchText" class="ks-filter-input" type="search" autocomplete="off" placeholder="Nhập mã, tên công việc hoặc dự án..." aria-label="Tìm công việc" />
                            <i class="fas fa-search" aria-hidden="true"></i>
                        </label>
                        <button type="button" class="ks-filter-clear" id="ksFilterClear"><i class="fas fa-redo-alt me-1"></i>Xóa lọc</button>
                        <span id="ksFilterCount" class="ks-filter-info" aria-live="polite"></span>
                    </div>
                    <div class="ks-filter-tags" id="ksFilterTags"></div>
                    <div class="ks-panel-body" id="ksTasks">
                        <asp:Repeater ID="rptTasks" runat="server" EnableViewState="false" OnItemDataBound="rptTasks_ItemDataBound">
                            <ItemTemplate>
                                <asp:PlaceHolder ID="phGroup" runat="server" Visible="false">
                                    <div class="ks-group-title <%# ((TaskRowView)Container.DataItem).TrangThaiDongHo == TrangThaiDongHoTask.DangChay ? "is-running-title" : (((TaskRowView)Container.DataItem).Nhom == NhomTaskView.QuaHan ? "is-overdue" : "") %>">
                                        <asp:Literal ID="ltrGroup" runat="server"></asp:Literal>
                                    </div>
                                </asp:PlaceHolder>

                                <div class="ks-task <%# TaskCss((TaskRowView)Container.DataItem) %>"
                                     data-task-id="<%# ((TaskRowView)Container.DataItem).IdCongViec.ToString("D") %>"
                                     data-project-id="<%# ((TaskRowView)Container.DataItem).IdDuAn.ToString("D") %>"
                                     data-project-name="<%# System.Web.HttpUtility.HtmlAttributeEncode(((TaskRowView)Container.DataItem).TenDuAn ?? string.Empty) %>"
                                     data-overdue="<%# ((TaskRowView)Container.DataItem).IsOverdue ? "1" : "0" %>"
                                     data-recorded="<%# ((TaskRowView)Container.DataItem).SoLanBamHomNay > 0 || ((TaskRowView)Container.DataItem).TongGiayHienTai > 0 ? "1" : "0" %>"
                                     data-base-sec="<%# ((TaskRowView)Container.DataItem).SoGiayDaTichLuy %>"
                                     data-running="<%# ((TaskRowView)Container.DataItem).TrangThaiDongHo == TrangThaiDongHoTask.DangChay ? "1" : "0" %>"
                                     data-start-ms="<%# ((TaskRowView)Container.DataItem).DangChayTuLuc.HasValue ? ToMs(((TaskRowView)Container.DataItem).DangChayTuLuc.Value) : 0 %>">

                                    <div class="ks-task-info">
                                        <div class="ks-kicker"><%#: ((TaskRowView)Container.DataItem).MaDuAn %> · <%#: ((TaskRowView)Container.DataItem).MaCongViec %> — <%#: ((TaskRowView)Container.DataItem).TenDuAn %></div>
                                        <div class="ks-task-name"><%#: ((TaskRowView)Container.DataItem).TenCongViec %></div>
                                        <div class="ks-task-meta"><%# TaskMetaHtml((TaskRowView)Container.DataItem) %></div>
                                    </div>

                                    <div class="ks-task-side">
                                        <div class="ks-tracker-row">
                                            <div class="ks-timer-box">
                                                <div class="ks-stopwatch"><%# KhaiBaoGioManager.FormatStopwatch(((TaskRowView)Container.DataItem).TongGiayHienTai) %></div>
                                                <div class="ks-decimal-hours"><%# TaskTodaySummaryHtml((TaskRowView)Container.DataItem) %></div>
                                            </div>

                                            <asp:PlaceHolder ID="phActionBtn" runat="server">
                                                <%# RenderTaskActionButton((TaskRowView)Container.DataItem) %>
                                            </asp:PlaceHolder>
                                        </div>

                                        <asp:PlaceHolder ID="phReadOnly" runat="server" Visible="false">
                                            <div class="ks-readonly"><%# TaskReadOnlyHtml((TaskRowView)Container.DataItem) %></div>
                                        </asp:PlaceHolder>

                                        <%# TaskBlockReasonHtml((TaskRowView)Container.DataItem) %>
                                        <div class="ks-hint <%# ((TaskRowView)Container.DataItem).IsOverdue ? "is-late" : "" %>">
                                            <%# TaskRecordHintHtml((TaskRowView)Container.DataItem) %>
                                        </div>
                                        <div class="ks-hint"><%# TaskHistoryHtml((TaskRowView)Container.DataItem) %></div>
                                        <button type="button" class="ks-journal-btn" data-ks-journal="<%# ((TaskRowView)Container.DataItem).IdCongViec.ToString("D") %>"><i class="far fa-clock me-1"></i>Nhật ký hôm nay</button>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <div class="ks-filter-empty" id="ksFilterEmpty"><i class="fas fa-search me-1"></i>Không tìm thấy công việc phù hợp. Hãy thử thay đổi điều kiện lọc.</div>
                        <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="ks-empty">
                            <i class="fas fa-clipboard-check"></i>
                            <b>Hôm nay bạn chưa được giao công việc nào.</b><br />
                            Khi có công việc đến hạn xử lý, chúng sẽ hiện ở đây.
                        </asp:Panel>
                    </div>
                </asp:Panel>

                <%-- Chân trang hiển thị thông báo thao tác gần nhất --%>
                <asp:Panel ID="pnlFooter" runat="server" CssClass="ks-footer">
                    <span class="text-muted" style="font-size:12px;">
                        <i class="fas fa-shield-alt me-1"></i>Tự động trừ giờ nghỉ trưa &amp; tự chốt sổ khi hết giờ làm việc buổi chiều
                    </span>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>

        <%-- Các trường ẩn & nút ẩn phục vụ thao tác Bắt đầu / Dừng / Tiếp tục và Tự làm mới --%>
        <input type="hidden" runat="server" id="hdfActionTaskId" ClientIDMode="Static" value="" />
        <input type="hidden" runat="server" id="hdfActionType" ClientIDMode="Static" value="" />
        <asp:LinkButton ID="btnTaskAction" runat="server" CssClass="ks-hidden" OnClick="btnTaskAction_Click" CausesValidation="false"></asp:LinkButton>
        <asp:LinkButton ID="btnRefresh" runat="server" CssClass="ks-hidden" OnClick="btnRefresh_Click" CausesValidation="false"></asp:LinkButton>

    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <%-- Modal chỉ đọc: lấy nhật ký từ dữ liệu GetTodayView của chính user đăng nhập. --%>
    <div id="ksJournalModal" class="ks-journal-mask" hidden role="presentation">
        <div class="ks-journal-dialog" role="dialog" aria-modal="true" aria-labelledby="ksJournalTitle" tabindex="-1">
            <div class="ks-journal-header">
                <div><h3 id="ksJournalTitle"><i class="far fa-clock me-2"></i>Nhật ký bấm giờ</h3><p id="ksJournalTask"></p></div>
                <button id="ksJournalClose" class="ks-journal-close" type="button" aria-label="Đóng nhật ký">&times;</button>
            </div>
            <div class="ks-journal-body">
                <div style="overflow-x:auto;">
                    <table class="ks-journal-table">
                        <thead><tr><th>#</th><th>Bắt đầu</th><th>Kết thúc</th><th>Thời gian thực</th><th>Ghi chú</th></tr></thead>
                        <tbody id="ksJournalRows"></tbody>
                    </table>
                </div>
                <div class="ks-journal-total"><span>Tổng thời gian của các phiên trong ngày</span><strong id="ksJournalTotal">00:00:00</strong></div>
                <p class="ks-journal-note">Thời gian thực đã trừ các khoảng nghỉ giữa ca và ngoài giờ làm việc. Số phút lưu DB có thể được làm tròn theo quy định bấm giờ.</p>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        var KhaiBaoGioJs = (function ($) {
            var cfg = null;
            var filterState = { status: 'all', project: 'all', keyword: '' };
            var journalTaskId = null;
            var journalPreviousFocus = null;
            var offset = 0;            // Độ lệch giờ máy chủ - giờ trình duyệt (ms)
            var ticker = null;
            var hooked = false;
            var bound = false;
            var pendingRefresh = false;
            var lastRefresh = Date.now();
            var fired = {};
            var requestKind = '';
            var actionInFlight = false;
            var pendingAction = null;
            var reloadPending = false;
            var requestStartedAt = 0;                       // thời điểm bắt đầu request đang chạy (cho watchdog)
            var errorCount = 0;                             // số lỗi liên tiếp
            var jitterMs = Math.floor(Math.random() * 4000); // lệch ngẫu nhiên 0-4s để mọi máy không đồng loạt gọi server
            var firedCount = {};                            // số lần đã gọi làm mới cho từng mốc biên
            var refreshEveryMs = 300000 + Math.floor(Math.random() * 60000);

            function nowMs() { return Date.now() + offset; }
            function pad(n) { return (n < 10 ? '0' : '') + n; }

            function fmtStopwatch(sec) {
                sec = Math.max(0, Math.floor(sec));
                var h = Math.floor(sec / 3600);
                var m = Math.floor((sec % 3600) / 60);
                var s = sec % 60;
                return pad(h) + ':' + pad(m) + ':' + pad(s);
            }

            function fmtDecimalHours(minutes) {
                if (minutes <= 0) return '0 giờ';
                var h = Math.round((minutes / 60.0) * 10) / 10;
                return h.toString().replace('.', ',') + ' giờ';
            }

            function fmtReadableMinutes(minutes) {
                minutes = Math.max(0, Math.floor(minutes));
                var hours = Math.floor(minutes / 60);
                var mins = minutes % 60;
                if (hours === 0) return mins + ' phút';
                return hours + ' giờ' + (mins ? ' ' + mins + ' phút' : '');
            }

            // Tính số giây thực làm giữa [startMs, endMs] bằng cách lấy phần giao với các khoảng ca làm việc
            // (Tự động cắt bỏ giờ nghỉ trưa 12:00-13:00 và thời gian ngoài khung làm việc)
            function calcWorkingSeconds(startMs, endMs) {
                if (!cfg || !cfg.segments || endMs <= startMs) return 0;
                var totalSec = 0;
                for (var i = 0; i < cfg.segments.length; i++) {
                    var seg = cfg.segments[i];
                    var s = Math.max(startMs, seg.start);
                    var e = Math.min(endMs, seg.end);
                    if (e > s) {
                        totalSec += (e - s) / 1000.0;
                    }
                }
                return Math.round(totalSec);
            }

            function isBusy() {
                if (typeof Sys === 'undefined' || !Sys.WebForms || !Sys.WebForms.PageRequestManager) return false;
                return Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack();
            }

            function triggerRefresh() {
                if (!cfg || reloadPending) return;

                // Không gửi yêu cầu làm mới chồng lên một yêu cầu khác.
                if (requestKind || actionInFlight || isBusy()) {
                    pendingRefresh = true;
                    return;
                }

                requestKind = 'refresh';
                requestStartedAt = Date.now();
                lastRefresh = Date.now();

                try {
                    __doPostBack(cfg.refreshId, '');
                } catch (e) {
                    forceReload(
                        'Không thể làm mới dữ liệu. Trang sẽ tải lại để đồng bộ trạng thái.'
                    );
                }
            }
            function sendTaskActionNow(taskId, action) {
                if (!cfg || reloadPending) return;

                // Nếu còn yêu cầu khác, không gửi chồng.
                if (requestKind || actionInFlight || isBusy()) {
                    if ((requestKind === 'refresh' ||
                        (isBusy() && !requestKind)) &&
                        !pendingAction) {

                        pendingAction = {
                            taskId: taskId,
                            action: action
                        };

                        lockActionButtons();

                        setKsStatus(
                            'Đang cập nhật dữ liệu. Thao tác sẽ được kiểm tra sau khi cập nhật xong.',
                            'pending'
                        );
                    } else {
                        setKsStatus(
                            'Hệ thống đang xử lý yêu cầu trước đó. Vui lòng đợi.',
                            'pending'
                        );
                    }

                    return;
                }

                var $btn = findActionButton(taskId, action);

                // Không thực hiện thao tác nếu nút đã biến mất hoặc bị khóa
                // theo trạng thái nghiệp vụ mới nhất.
                if (!$btn.length || $btn.prop('disabled')) {
                    setKsStatus(
                        'Trạng thái công việc vừa thay đổi. Vui lòng kiểm tra và bấm lại.',
                        'error'
                    );
                    return;
                }

                requestKind = 'action';
                actionInFlight = true;
                requestStartedAt = Date.now();

                $('#hdfActionTaskId').val(taskId);
                $('#hdfActionType').val(action);

                lockActionButtons();

                setKsStatus('Đang xử lý thao tác…', 'pending');

                try {
                    __doPostBack(cfg.actionBtnId, '');
                } catch (e) {
                    forceReload(
                        'Không thể gửi thao tác đến máy chủ. Trang sẽ tải lại để kiểm tra trạng thái.'
                    );
                }
            }
            function drainPendingRequests(attempt) {
                if (reloadPending) return;

                attempt = attempt || 0;

                // Chờ ASP.NET kết thúc hoàn toàn yêu cầu trước đó.
                if (isBusy() || requestKind) {
                    if (attempt < 20) {
                        window.setTimeout(function () {
                            drainPendingRequests(attempt + 1);
                        }, 50);
                    } else {
                        forceReload(
                            'Yêu cầu xử lý chưa hoàn tất. Trang sẽ tải lại để đồng bộ trạng thái.'
                        );
                    }

                    return;
                }

                // Ưu tiên thao tác người dùng trước yêu cầu làm mới định kỳ.
                if (pendingAction) {
                    var next = pendingAction;
                    pendingAction = null;

                    var $btn = findActionButton(next.taskId, next.action);

                    if (!$btn.length || $btn.prop('disabled')) {
                        unlockActionButtons();

                        setKsStatus(
                            'Trạng thái công việc đã thay đổi. Thao tác chưa được thực hiện; vui lòng kiểm tra và bấm lại.',
                            'error'
                        );

                        // Dữ liệu vừa nhận từ máy chủ đã được làm mới.
                        pendingRefresh = false;
                        return;
                    }

                    sendTaskActionNow(next.taskId, next.action);
                    return;
                }

                // Chỉ làm mới khi không còn thao tác người dùng đang chờ.
                if (pendingRefresh) {
                    pendingRefresh = false;
                    triggerRefresh();
                }
            }
            // Bộ lọc client: không postback, giữ trạng thái sau UpdatePanel.
            function normalizedText(text) {
                var v = String(text || '').toLowerCase();
                if (v.normalize) v = v.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
                return v.replace(/đ/g, 'd').trim();
            }
            function refreshProjectFilter() {
                var $select = $('#ksFilterProject');
                if (!$select.length) return;
                var options = {};
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this), id = $row.attr('data-project-id');
                    if (id && !options[id]) options[id] = $row.attr('data-project-name') || id;
                });
                $select.empty().append($('<option>').val('all').text('Tất cả dự án'));
                Object.keys(options).sort(function (a, b) {
                    return options[a].localeCompare(options[b], 'vi');
                }).forEach(function (id) { $select.append($('<option>').val(id).text(options[id])); });
                if (!options[filterState.project]) filterState.project = 'all';
                $select.val(filterState.project);
            }
            function renderFilterTags() {
                var $tags = $('#ksFilterTags');
                if (!$tags.length) return;
                $tags.empty();
                var addTag = function (key, text) {
                    var $tag = $('<span>').addClass('ks-filter-tag').append($('<span>').text(text));
                    $tag.append($('<button type="button">').attr('data-ks-remove-filter', key).attr('aria-label', 'Bỏ ' + text).text('×'));
                    $tags.append($tag);
                };
                if (filterState.status !== 'all') addTag('status', 'Trạng thái: ' + $('#ksFilterStatus option:selected').text());
                if (filterState.project !== 'all') addTag('project', 'Dự án: ' + $('#ksFilterProject option:selected').text());
                if (filterState.keyword) addTag('keyword', 'Từ khóa: ' + filterState.keyword);
            }
            function applyFilters() {
                var keyword = normalizedText(filterState.keyword), shown = 0, pinned = 0;
                $('#ksFilterStatus').val(filterState.status);
                $('#ksFilterProject').val(filterState.project);
                if ($('#ksSearchText').val() !== filterState.keyword) $('#ksSearchText').val(filterState.keyword);
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this);
                    var running = $row.attr('data-running') === '1';
                    var recorded = $row.attr('data-recorded') === '1';
                    var overdue = $row.attr('data-overdue') === '1';
                    var status = filterState.status;
                    var statusMatch = status === 'all' ||
                        (status === 'running' && running) ||
                        (status === 'unstarted' && !recorded) ||
                        (status === 'recorded' && recorded) ||
                        (status === 'overdue' && overdue);
                    var textMatch = !keyword || normalizedText($row.find('.ks-kicker').text() + ' ' + $row.find('.ks-task-name').text()).indexOf(keyword) !== -1;
                    var projectMatch = filterState.project === 'all' || $row.attr('data-project-id') === filterState.project;
                    var matches = statusMatch && textMatch && projectMatch;
                    // Luôn cho thấy công việc đang chạy để người dùng không bị mất nút Dừng.
                    var visible = matches || running;
                    $row.toggle(visible);
                    if (visible) shown++;
                    if (running && !matches) pinned++;
                });
                $('#ksTasks > .ks-group-title').each(function () {
                    var $items = $(this).nextUntil('.ks-group-title', '.ks-task');
                    $(this).toggle($items.filter(':visible').length > 0);
                });
                $('#ksFilterEmpty').toggle(shown === 0 && $('#ksTasks .ks-task').length > 0);
                $('#ksFilterCount').text(shown + ' công việc hiển thị' + (pinned ? ' · Công việc đang chạy luôn hiển thị' : ''));
                renderFilterTags();
            }
            function refreshFiltersAfterBind() {
                refreshProjectFilter();
                applyFilters();
            }

            // Popup nhật ký chỉ đọc, dữ liệu thuộc chính tài khoản đang đăng nhập.
            function showJournal(taskId) {
                if (!cfg || !cfg.journal) return;
                var $row = $('#ksTasks .ks-task').filter(function () { return $(this).attr('data-task-id') === taskId; }).first();
                if (!$row.length) return;
                journalTaskId = taskId;
                journalPreviousFocus = document.activeElement;
                $('#ksJournalTask').text($row.find('.ks-kicker').text() + ' · ' + $row.find('.ks-task-name').text());
                var modal = document.getElementById('ksJournalModal');
                if (!modal) return;
                modal.hidden = false;
                renderJournal();
                $('#ksJournalClose').trigger('focus');
            }
            function hideJournal() {
                var modal = document.getElementById('ksJournalModal');
                if (modal) modal.hidden = true;
                journalTaskId = null;
                if (journalPreviousFocus && typeof journalPreviousFocus.focus === 'function' && document.documentElement.contains(journalPreviousFocus)) {
                    journalPreviousFocus.focus();
                }
                journalPreviousFocus = null;
            }
            function renderJournal() {
                if (!journalTaskId || !cfg || !cfg.journal) return;
                var sessions = cfg.journal.filter(function (x) { return x.taskId === journalTaskId; });
                var $body = $('#ksJournalRows');
                $body.empty();
                var total = 0;
                var t = nowMs();
                sessions.forEach(function (session, index) {
                    var running = session.end === null || session.end === undefined;
                    var seconds = running ? calcWorkingSeconds(session.start, t) : session.seconds;
                    seconds = Math.max(0, seconds || 0);
                    total += seconds;
                    function timeLabel(ms) { var d = new Date(ms); return pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds()); }
                    var note = running ? 'Đang chạy' : (session.reason === 2 ? 'Tự động kết thúc ca' : 'Nhân viên dừng');
                    $('<tr>').append($('<td>').text(index + 1))
                        .append($('<td>').text(timeLabel(session.start)))
                        .append($('<td>').text(running ? 'Đang chạy' : timeLabel(session.end)))
                        .append($('<td>').addClass(running ? 'ks-journal-live' : '').text(fmtStopwatch(seconds)))
                        .append($('<td>').text(note))
                        .appendTo($body);
                });
                if (!sessions.length) $body.append($('<tr>').append($('<td colspan="5">').text('Chưa có phiên bấm giờ nào trong hôm nay.')));
                $('#ksJournalTotal').text(fmtStopwatch(total));
            }

            function setKsStatus(message, kind) {
                var $status = $('.ks-save-status').first();

                if (!$status.length) return;

                var cssClass = 'ks-save-status';

                if (kind) {
                    cssClass += ' is-' + kind;
                }

                $status.text(message).attr('class', cssClass);
            }


            // Chỉ khóa các nút đang có thể sử dụng.
            // Không thay đổi trạng thái disabled do quy tắc nghiệp vụ đặt ra.
            function lockActionButtons() {
                $('.ks-action-btn').each(function () {
                    var $btn = $(this);

                    if ($btn.prop('disabled')) return;

                    $btn.attr('data-ks-js-locked', '1');
                    $btn.prop('disabled', true);
                });
            }


            // Chỉ mở lại những nút được JavaScript khóa.
            // Các nút server render ở trạng thái disabled vẫn giữ nguyên.
            function unlockActionButtons() {
                $('.ks-action-btn[data-ks-js-locked="1"]').each(function () {
                    $(this)
                        .prop('disabled', false)
                        .removeAttr('data-ks-js-locked');
                });
            }


            // Tìm nút tương ứng với trạng thái thao tác mới nhất trên giao diện.
            function findActionButton(taskId, action) {
                var $found = $();

                $('.ks-action-btn').each(function () {
                    var $btn = $(this);

                    if ($btn.attr('data-task-id') === taskId &&
                        $btn.attr('data-action') === action) {
                        $found = $btn;
                        return false;
                    }
                });

                return $found;
            }


            // Nếu không thể xác định kết quả yêu cầu, tải lại trang để lấy
            // trạng thái mới nhất từ máy chủ, tránh cho giao diện tiếp tục sai lệch.
            function forceReload(message) {
                if (reloadPending) return;

                reloadPending = true;
                actionInFlight = false;
                requestKind = 'error';
                pendingAction = null;
                pendingRefresh = false;

                lockActionButtons();
                setKsStatus(message, 'error');

                window.setTimeout(function () {
                    window.location.reload();
                }, 1200);
            }
            // Watchdog: nếu cờ bận còn bật mà ASP.NET đã rảnh (mất endRequest) hoặc request treo quá lâu
            // thì tự giải phóng để nút không bị khóa vĩnh viễn.
            function watchdog() {
                if (reloadPending || !(requestKind || actionInFlight)) return;
                var age = Date.now() - requestStartedAt;
                if (!isBusy() && age > 3000) {
                    requestKind = '';
                    actionInFlight = false;
                    unlockActionButtons();
                    drainPendingRequests(0);
                } else if (isBusy() && age > 30000) {
                    try { Sys.WebForms.PageRequestManager.getInstance().abortPostBack(); } catch (e) { }
                    forceReload('Máy chủ phản hồi quá lâu. Trang sẽ tải lại để đồng bộ trạng thái.');
                }
            }

            function tick() {
                var t = nowMs();

                hookRequestManager();   // thử lại nếu lần init đầu Sys chưa sẵn sàng
                watchdog();

                // 1. Đồng hồ giờ hệ thống trên Header
                var clock = document.getElementById('ksClock');
                if (clock) {
                    var d = new Date(t);
                    clock.textContent = pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds());
                }

                // 2. Đồng hồ đếm ngược đến giờ mở / đóng ngày làm việc
                $('.ks-count').each(function () {
                    var target = parseInt($(this).attr('data-target'), 10);
                    if (target) $(this).text(fmtStopwatch((target - t) / 1000));
                });

                if (!cfg) return;

                // 3. Kiểm tra có đang trong giờ nghỉ trưa không
                var inLunch = (cfg.lunchStart > 0 && cfg.lunchEnd > 0 && t >= cfg.lunchStart && t < cfg.lunchEnd);
                var lunchBanner = document.getElementById('ksLunchBanner');
                if (lunchBanner) {
                    lunchBanner.style.display = (inLunch && cfg.isDayOpen) ? 'flex' : 'none';
                }

                // 4. Cập nhật trực tiếp đồng hồ của Task đang chạy & Tổng thời gian ngày
                var totalDaySec = 0;
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this);
                    var baseSec = parseInt($row.attr('data-base-sec'), 10) || 0;
                    var isRunning = $row.attr('data-running') === '1';
                    var startMs = parseInt($row.attr('data-start-ms'), 10) || 0;

                    var currentSec = baseSec;
                    if (isRunning && startMs > 0) {
                        currentSec += calcWorkingSeconds(startMs, t);
                        $row.find('.ks-stopwatch').text(fmtStopwatch(currentSec));
                        var mins = Math.round(currentSec / 60.0);
                        $row.find('.ks-decimal-hours').text(
                            inLunch ? 'Đang tạm dừng nghỉ trưa' : ('Đang chạy · ' + fmtReadableMinutes(mins))
                        );
                    }
                    totalDaySec += currentSec;
                });

                if (cfg.capacitySec > 0 && totalDaySec > cfg.capacitySec) {
                    totalDaySec = cfg.capacitySec;
                }

                if (cfg.isDayOpen) {
                    var dayWatch = document.getElementById('ksDayTotalWatch');
                    if (dayWatch) dayWatch.textContent = fmtStopwatch(totalDaySec);

                    var daySummary = document.getElementById('ksDayTotalSummary');
                    if (daySummary) {
                        var totalMins = Math.round(totalDaySec / 60.0);
                        daySummary.textContent = '(' + fmtReadableMinutes(totalMins) + ' / ' + fmtReadableMinutes(cfg.capacityMin) + ')';
                    }

                    var bar = document.getElementById('ksDayProgressBar');
                    if (bar && cfg.capacitySec > 0) {
                        var pct = Math.min(100, Math.round((totalDaySec * 100) / cfg.capacitySec));
                        bar.style.width = pct + '%';
                    }

                    var warn = document.getElementById('ksWarn');
                    if (warn) {
                        warn.style.display = (totalDaySec === 0 && !cfg.hasRunningTask) ? 'flex' : 'none';
                    }
                }

                // Cập nhật nhật ký khi popup đang mở (phiên đang chạy được tính theo khung ca).
                if (journalTaskId) renderJournal();

                // 5. Khi bước qua các mốc biên (Mở ca sáng, Bắt đầu nghỉ trưa, Hết nghỉ trưa, Hết ca chiều 17:00):
                // Tự động gọi về Server để mở khóa hoặc tự động chốt sổ cuối ngày.
                if (cfg.boundaries && cfg.boundaries.length) {
                    for (var i = 0; i < cfg.boundaries.length; i++) {
                        var b = cfg.boundaries[i];
                        var n = firedCount[b] || 0;
                        if (n < 3 && t >= b + 800 + jitterMs + n * 3000) {
                            firedCount[b] = n + 1;
                            triggerRefresh();
                            return;
                        }
                    }
                }

                // 6. Qua nửa đêm sang ngày mới: tải lại toàn trang
                if (cfg.midnight > 0 && t >= cfg.midnight + 1000) {
                    window.location.reload();
                    return;
                }

                // 7. Làm mới định kỳ mỗi 5 phút để giữ phiên đăng nhập và cập nhật task mới
                if (!requestKind &&
                    !actionInFlight &&
                    !isBusy() &&
                    Date.now() - lastRefresh > refreshEveryMs) {
                    triggerRefresh();
                }
            }

            function hookRequestManager() {
                if (hooked ||
                    typeof Sys === 'undefined' ||
                    !Sys.WebForms ||
                    !Sys.WebForms.PageRequestManager) {
                    return;
                }

                hooked = true;

                var prm = Sys.WebForms.PageRequestManager.getInstance();

                prm.add_endRequest(function (sender, args) {

                    // Trường hợp máy chủ trả lỗi hoặc postback AJAX thất bại.
                    if (args.get_error()) {
                        args.set_errorHandled(true);
                        errorCount++;
                        requestKind = '';
                        actionInFlight = false;
                        pendingAction = null;
                        unlockActionButtons();

                        if (errorCount >= 2) {
                            forceReload('Yêu cầu không hoàn tất. Trang sẽ tự tải lại để đồng bộ trạng thái.');
                        } else {
                            setKsStatus('Yêu cầu chưa hoàn tất, đang đồng bộ lại…', 'error');
                            window.setTimeout(triggerRefresh, 1500);
                        }
                        return;
                    }

                    errorCount = 0;

                    // Yêu cầu đã kết thúc thành công.
                    requestKind = '';
                    actionInFlight = false;

                    // Mở lại các nút bị khóa tạm bằng JavaScript.
                    // Nút disabled do máy chủ tạo ra vẫn được giữ nguyên.
                    unlockActionButtons();

                    // Xử lý thao tác hoặc yêu cầu làm mới còn chờ.
                    window.setTimeout(function () {
                        drainPendingRequests(0);
                    }, 0);
                });
            }
            function bindEvents() {
                if (bound) return;
                bound = true;

                // Sự kiện được gắn ở document nên vẫn hoạt động sau UpdatePanel.
                $(document).on('input.ks', '#ksSearchText', function () {
                    filterState.keyword = $(this).val();
                    applyFilters();
                });
                $(document).on('change.ks', '#ksFilterStatus', function () {
                    filterState.status = $(this).val(); applyFilters();
                });
                $(document).on('change.ks', '#ksFilterProject', function () {
                    filterState.project = $(this).val(); applyFilters();
                });
                $(document).on('click.ks', '#ksFilterClear', function () {
                    filterState = { status: 'all', project: 'all', keyword: '' }; applyFilters();
                });
                $(document).on('click.ks', '[data-ks-remove-filter]', function () {
                    var key = $(this).attr('data-ks-remove-filter');
                    if (key === 'keyword') filterState.keyword = '';
                    else if (key) filterState[key] = 'all';
                    applyFilters();
                });
                $(document).on('click.ks', '.ks-journal-btn', function () {
                    showJournal($(this).attr('data-ks-journal'));
                });
                $(document).on('click.ks', '#ksJournalClose', hideJournal);
                $(document).on('click.ks', '#ksJournalModal', function (e) {
                    if (e.target === this) hideJournal();
                });
                $(document).on('keydown.ks', function (e) {
                    if (e.key === 'Escape' && journalTaskId) hideJournal();
                    if (e.key === 'Tab' && journalTaskId) {
                        // Modal chỉ có một nút tương tác, giữ focus bên trong.
                        e.preventDefault(); $('#ksJournalClose').trigger('focus');
                    }
                });

                $(document).on('click.ks', '.ks-action-btn', function (e) {
                    e.preventDefault();

                    var $btn = $(this);

                    if (reloadPending || $btn.prop('disabled')) {
                        return;
                    }

                    var taskId = $btn.attr('data-task-id');
                    var action = $btn.attr('data-action');

                    if (!taskId || !action || !cfg) {
                        setKsStatus(
                            'Không xác định được công việc cần thao tác. Vui lòng tải lại trang.',
                            'error'
                        );
                        return;
                    }

                    // Không nhận thêm thao tác nếu một thao tác trước vẫn đang chạy.
                    if (actionInFlight ||
                        requestKind === 'action' ||
                        pendingAction) {

                        setKsStatus(
                            'Thao tác trước đó đang được xử lý. Vui lòng đợi.',
                            'pending'
                        );

                        return;
                    }

                    // Nếu đang làm mới dữ liệu, giữ lại lần bấm đầu tiên.
                    if (requestKind === 'refresh' ||
                        (isBusy() && !requestKind)) {

                        if (!pendingAction) {
                            pendingAction = {
                                taskId: taskId,
                                action: action
                            };

                            lockActionButtons();

                            setKsStatus(
                                'Đang cập nhật dữ liệu. Thao tác sẽ được kiểm tra và thực hiện nếu còn hợp lệ.',
                                'pending'
                            );
                        } else {
                            setKsStatus(
                                'Đang có một thao tác chờ xử lý. Vui lòng đợi.',
                                'pending'
                            );
                        }

                        return;
                    }

                    // Trường hợp yêu cầu khác đang chạy mà không phải làm mới của trang.
                    if (isBusy() || requestKind) {
                        setKsStatus(
                            'Trang đang xử lý một yêu cầu khác. Vui lòng đợi rồi bấm lại.',
                            'pending'
                        );
                        return;
                    }

                    sendTaskActionNow(taskId, action);
                });
            }

            return {
                init: function (config) {
                    cfg = config;
                    offset = cfg.serverNow - Date.now();
                    lastRefresh = Date.now();

                    // Không đặt lại pendingRefresh, pendingAction,
                    // actionInFlight hoặc requestKind ở đây.
                    // Các trạng thái này phải tồn tại xuyên suốt partial postback.

                    hookRequestManager();
                    bindEvents();
                    refreshFiltersAfterBind();
                    if (journalTaskId) {
                        // Nhật ký đang mở có thể cập nhật sau Start/Stop mà không mất modal.
                        var exists = $('#ksTasks .ks-task').filter(function () { return $(this).attr('data-task-id') === journalTaskId; }).length > 0;
                        if (exists) renderJournal(); else hideJournal();
                    }

                    if (!ticker) {
                        ticker = setInterval(tick, 1000);
                    }

                    tick();
                }
            };
        })(jQuery);
    </script>
</asp:Content>