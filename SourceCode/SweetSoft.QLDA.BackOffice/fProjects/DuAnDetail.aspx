<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlGiaiDoanDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlGiaiDoanDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlLichSuDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlLichSuDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAnForm.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAnForm" %>
<%@ Register Src="~/fProjects/Controls/CtrlChonNhanVien.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVien" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>

        /* CSS CHO AVATAR STACK CỦA OWNER VÀ THÀNH VIÊN */
        .avatar-group { display: inline-flex !important; align-items: center; justify-content: center; flex-wrap: nowrap !important; white-space: nowrap !important; }
        .avatar-stack-container { display: flex; align-items: center; }
        .avatar-circle { width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
        .avatar-circle:first-child { margin-left: 0; }
        .avatar-more { background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; }

        /* Toolbar 2 vùng: trái là tên dự án + nút thao tác; phải là 2 nút chuyển view. */
        .pd-project-toolbar { display:grid; grid-template-columns:minmax(0,1fr) auto; align-items:stretch; gap:14px; margin:6px 0 18px; }
        .pd-project-main { display:flex; align-items:center; justify-content:space-between; gap:18px; min-width:0; padding:0; }
        .pd-project-main-left { display:flex; align-items:center; gap:12px; flex-wrap:wrap; min-width:0; flex:1 1 auto; }
        .pd-project-identity { display:flex; align-items:center; flex-wrap:wrap; gap:8px 10px; min-width:0; }
        .pd-project-identity h4 { margin:0; font-size:clamp(17px,1.35vw,20px); line-height:1.3; overflow-wrap:anywhere; }
        .pd-project-identity .badge { flex-shrink:0; font-size:11px; font-weight:700; border-radius:999px; padding:.5rem .95rem !important; }
        .pd-project-actions { display:flex; align-items:center; justify-content:flex-end; flex-wrap:wrap; gap:7px; min-width:0; flex:0 0 auto; }
        .pd-project-actions .btn { white-space:nowrap; }
        .pd-project-actions .pd-status-buttons { display:flex; align-items:center; flex-wrap:wrap; gap:7px; }
        .pd-project-actions .pd-status-buttons .btn { margin:0 !important; }
        .pd-view-switcher { display:inline-flex; align-items:stretch; gap:0; align-self:start; border:1px solid #e4d8f3; border-radius:12px; overflow:hidden; background:#f7f3fc; box-shadow:0 2px 8px rgba(58,37,85,.04); }
        .pd-view-button { display:inline-flex; align-items:center; justify-content:center; gap:8px; min-width:190px; padding:14px 18px; border:0; border-radius:0; background:transparent; color:#6e6180; font-size:14px; font-weight:800; cursor:pointer; transition:background .17s,color .17s; }
        .pd-view-button + .pd-view-button { border-left:1px solid #2f2f2f; }
        .pd-view-button:hover { background:#eee7f8; color:#52218d; }
        .pd-view-button.is-active { background:#5b2499; color:#fff; }
        .pd-view-button:focus-visible { outline:2px solid #a777e0; outline-offset:-2px; }
        .pd-view-panel[hidden] { display:none !important; }

        /* Personal view: 4 bảng, 2 bảng / 1 hàng. */
        .pd-personal-grid { display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:18px; align-items:stretch; }
        .pd-personal-card { min-width:0; display:flex; flex-direction:column; height:438px; border:1px solid #e4e2ec; border-radius:16px; background:#fff; overflow:hidden; box-shadow:0 3px 14px rgba(47,31,72,.035); }
        .pd-personal-card-head { display:flex; align-items:center; justify-content:space-between; gap:12px; padding:14px 16px; min-height:62px; background:#faf8fd; border-bottom:1px solid #ebe5f1; }
        .pd-head-main { display:flex; align-items:center; gap:9px; min-width:0; }
        .pd-head-main i { color:#6b34ac; font-size:15px; }
        .pd-head-main strong { color:#372b4d; font-size:14px; font-weight:800; overflow-wrap:anywhere; }
        .pd-head-actions { display:flex; align-items:center; justify-content:flex-end; gap:8px; flex-wrap:wrap; }
        .pd-head-actions small { margin:0; padding:4px 8px; border-radius:999px; background:#eee5f8; color:#642b98; font-size:11px; font-weight:800; }
        .pd-detail-btn { appearance:none; border:1px solid #ddcff1; border-radius:8px; background:#fff; padding:5px 10px; color:#6d329e; font-size:11px; font-weight:800; cursor:pointer; transition:all .16s ease; }
        .pd-detail-btn:hover { background:#f6efff; border-color:#c9b3e9; }
        .pd-filter-group { display:inline-flex; align-items:center; gap:2px; padding:3px; border:1px solid #e3d8f3; border-radius:9px; background:#fff; }
        .pd-filter-group button { appearance:none; border:0; border-radius:7px; padding:5px 8px; background:transparent; color:#806a92; font-size:10.5px; font-weight:750; white-space:nowrap; cursor:pointer; transition:background .15s ease,color .15s ease; }
        .pd-filter-group button:hover { background:#f3ebfa; color:#54218d; }
        .pd-filter-group button.is-active { background:#5b2499; color:#fff; }
        .pd-card-body { flex:1; min-height:0; display:flex; flex-direction:column; }
        .pd-personal-list { flex:1; min-height:0; overflow-y:auto; overscroll-behavior:contain; scrollbar-width:thin; scrollbar-color:#d9cce8 transparent; }
        .pd-personal-item { min-height:62px; padding:13px 16px; border-bottom:1px solid #f0edf4; }
        .pd-personal-item:last-child { border-bottom:0; }
        .pd-personal-item:hover { background:#fbf9ff; }
        .pd-personal-item-title { color:#342b42; font-weight:750; font-size:13.5px; line-height:1.45; overflow-wrap:anywhere; }
        .pd-personal-task-code { display:inline-flex; align-items:center; margin-right:7px; padding:3px 7px; border:1px solid #d9c7fa; border-radius:6px; background:#f4eefe; color:#632caa; font-size:11px; font-weight:800; vertical-align:middle; }
        .pd-personal-tag { display:inline-flex; align-items:center; gap:5px; padding:4px 8px; border-radius:7px; background:#f1f1f5; color:#667085; font-size:11px; font-weight:700; }
        .pd-personal-tag.is-progress { background:#e5f5ff; color:#18709b; }
        .pd-personal-tag.is-done { background:#e5f8ec; color:#19754e; }
        .pd-personal-tag.is-overdue { background:#fff0ef; color:#bf5146; }
        .pd-personal-tag.is-upcoming { background:#f0eaff; color:#7334b2; }
        .pd-personal-hours { flex-shrink:0; display:inline-flex; align-items:center; gap:5px; white-space:nowrap; border:1px solid #e4d2f7; border-radius:8px; background:#f9f4fe; padding:5px 8px; color:#6d329e; font-size:11.5px; font-weight:800; }
        .pd-personal-task-header { display:flex; justify-content:space-between; align-items:flex-start; gap:8px; }
        .pd-personal-task-title { flex:1; min-width:0; }
        .pd-personal-task-heading { display:flex; align-items:center; flex-wrap:wrap; gap:6px 7px; }
        .pd-personal-task-heading .pd-personal-task-code { margin:0; }
        .pd-personal-task-name { min-width:0; }
        .pd-personal-due { display:flex; flex-wrap:wrap; align-items:center; gap:7px; margin-top:8px; color:#72697f; font-size:12px; }
        .pd-personal-progress { height:5px; border-radius:12px; background:#eceaf2; overflow:hidden; margin-top:10px; }
        .pd-personal-progress span { display:block; height:100%; border-radius:12px; background:linear-gradient(90deg,#8e58ce,#6530a4); }
        .pd-personal-meta { display:flex; align-items:center; gap:8px; flex-wrap:wrap; margin-top:8px; color:#82788f; font-size:11.8px; line-height:1.5; }
        .pd-personal-no-code { display:block; }
        .pd-personal-meeting-heading { display:flex; align-items:flex-start; justify-content:space-between; gap:8px; margin-bottom:7px; }
        .pd-personal-meeting-flags { display:inline-flex; align-items:center; justify-content:flex-end; flex-wrap:wrap; gap:5px; }
        .pd-personal-meeting-flags .pd-personal-tag { padding:3px 6px; font-size:10px; }
        .pd-personal-meeting-line { display:flex; align-items:flex-start; gap:7px; margin-top:7px; color:#82788f; font-size:11.5px; line-height:1.45; min-width:0; }
        .pd-personal-meeting-line i { margin-top:3px; flex:0 0 13px; color:#8b7b98; }
        .pd-personal-meeting-line span { overflow-wrap:anywhere; min-width:0; }
        .pd-personal-empty { padding:22px 16px; color:#948b9e; font-size:12.5px; text-align:center; }
        .pd-card-placeholder { min-height:62px; border-bottom:1px dashed #f0edf4; }
        .pd-personal-summary { padding:14px 16px; display:grid; grid-template-columns:1fr; gap:10px; }
        .pd-summary-row { display:flex; align-items:center; justify-content:space-between; gap:12px; padding:11px 12px; min-height:56px; border:1px solid #eee6f7; border-radius:10px; background:#fcfbfe; }
        .pd-summary-row strong { color:#372b4d; font-size:13px; }
        .pd-summary-row span { color:#5a2494; font-weight:800; }
        .pd-summary-row.is-total { background:#f5effd; border-color:#dcccfa; }
        .pd-personal-tasks.is-open-only .pd-task-done { display:none !important; }
        .pd-task-open-empty, .pd-issue-open-empty, .pd-meeting-open-empty { display:none; }
        .pd-personal-tasks.is-open-only .pd-task-open-empty { display:block; }
        .pd-personal-meetings.is-open-only .pd-meeting-open-empty { display:block; }
        .pd-personal-meetings.is-open-only .pd-meeting-ended { display:none !important; }
        .pd-personal-issues.is-open-only .pd-issue-processed { display:none !important; }
        .pd-personal-issues.is-open-only .pd-issue-open-empty { display:block; }

        /* Popup chi tiết */
        .pd-detail-modal-wrap { min-height:120px; }
        .pd-detail-modal-title { margin:0 0 4px; color:#452271; font-size:18px; font-weight:800; }
        .pd-detail-modal-sub { margin:0 0 14px; color:#8a7c99; font-size:12px; }
        .pd-detail-list { display:flex; flex-direction:column; gap:10px; max-height:68vh; overflow:auto; }
        .pd-detail-item { padding:14px 16px; border:1px solid #ece5f5; border-radius:12px; background:#fff; }
        .pd-detail-row { display:flex; align-items:flex-start; justify-content:space-between; gap:12px; flex-wrap:wrap; }
        .pd-detail-subrow { display:flex; align-items:center; gap:10px; flex-wrap:wrap; margin-top:9px; color:#7f738e; font-size:12px; }
        .pd-detail-grid { display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:12px; }
        .pd-detail-grid .pd-summary-row { min-height:68px; }

        @media(max-width:1480px) { .pd-project-main { flex-wrap:wrap; } }
        @media(max-width:1280px) { .pd-personal-grid { grid-template-columns:1fr 1fr; } }
        @media(max-width:1100px) { .pd-project-toolbar { grid-template-columns:1fr; } .pd-view-switcher { width:100%; justify-self:start; } .pd-view-button { flex:1 1 0; min-width:0; } .pd-personal-grid { grid-template-columns:1fr; } .pd-personal-card { height:auto; min-height:420px; } }
        @media(max-width:700px) { .pd-project-main { flex-direction:column; align-items:flex-start; } .pd-project-actions { justify-content:flex-start; } .pd-view-switcher { width:100%; } .pd-detail-grid { grid-template-columns:1fr; } }


        /* Nâng cấp 4 cột: 3 bản ghi trong khung, nút xem chi tiết ở chân bảng. */
        .pd-project-toolbar { align-items:center; gap:14px; }
        .pd-project-main { gap:12px; }
        .pd-view-switcher { align-self:center; border-radius:9px; }
        .pd-view-button { min-width:0; padding:8px 11px; gap:6px; font-size:12px; line-height:1.3; }
        .pd-view-button i { font-size:12px; }
        .pd-personal-grid { grid-template-columns:minmax(0,1.04fr) minmax(0,1.18fr) minmax(0,.97fr) minmax(0,.96fr); align-items:stretch; gap:13px; }
        .pd-personal-card { height:408px; min-height:0; border-radius:13px; }
        .pd-personal-card-head { height:61px; min-height:61px; padding:9px 11px; gap:7px; }
        .pd-head-main { gap:6px; flex:1 1 auto; }
        .pd-head-main strong { font-size:12.5px; line-height:1.35; }
        .pd-head-main i { font-size:13px; }
        .pd-head-actions { gap:5px; flex:0 0 auto; }
        .pd-head-actions small { padding:3px 6px; font-size:10px; white-space:nowrap; }
        .pd-compact-filter { appearance:none; display:inline-flex; align-items:center; justify-content:center; gap:5px; padding:5px 7px; min-height:29px; border:1px solid #dfd0ed; border-radius:7px; background:#fff; color:#6b329c; font-size:10.5px; font-weight:800; white-space:nowrap; cursor:pointer; transition:background .16s,border-color .16s; }
        .pd-compact-filter:hover { background:#f4ecfc; border-color:#c9aae7; }
        .pd-compact-filter.is-filtered { background:#5b2499; border-color:#5b2499; color:#fff; }
        .pd-compact-filter:focus-visible { outline:2px solid #9363ce; outline-offset:2px; }
        .pd-compact-filter i { font-size:9px; opacity:.8; }
        .pd-personal-list { flex:1; min-height:0; max-height:none; overflow-y:auto; }
        .pd-personal-item { min-height:78px; padding:12px 12px; }
        .pd-personal-task-heading { gap:4px 5px; }
        .pd-personal-item-title { font-size:12.5px; }
        .pd-personal-tag { font-size:10px; padding:3px 6px; }
        .pd-personal-hours { padding:4px 6px; font-size:10px; }
        .pd-personal-meeting-heading { flex-wrap:wrap; }
        .pd-personal-meeting-flags { justify-content:flex-start; }
        .pd-personal-meeting-line { font-size:11px; margin-top:6px; }
        .pd-card-placeholder { display:none!important; }
        .pd-personal-empty { padding:22px 12px; line-height:1.5; }
        .pd-filter-empty[hidden] { display:none!important; }
        .pd-personal-card-footer { flex:0 0 44px; min-height:44px; display:flex; justify-content:flex-end; align-items:center; padding:7px 12px; border-top:1px solid #ebe5f1; background:#fcfbff; }
        .pd-detail-btn { display:inline-flex; align-items:center; gap:5px; min-height:28px; padding:5px 10px; font-size:11px; }
        .pd-personal-summary { padding:12px; gap:7px; }
        .pd-summary-row { min-height:52px; padding:9px 10px; }
        .pd-summary-row strong { font-size:12px; }
        .pd-summary-row span { font-size:12px; white-space:nowrap; }
        .pd-personal-tasks.is-open-only .pd-task-done { display:none!important; }
        .pd-personal-meetings.is-open-only .pd-meeting-ended { display:none!important; }
        .pd-personal-issues.is-open-only .pd-issue-processed { display:none!important; }
        .pd-personal-tasks.is-open-only .pd-task-open-empty, .pd-personal-meetings.is-open-only .pd-meeting-open-empty, .pd-personal-issues.is-open-only .pd-issue-open-empty { display:none!important; }
        @media(max-width:1450px) { .pd-personal-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } .pd-personal-card { height:408px; } }
        @media(max-width:780px) { .pd-personal-grid { grid-template-columns:minmax(0,1fr); } .pd-personal-card { height:408px; min-height:0; } .pd-project-toolbar { grid-template-columns:minmax(0,1fr); } .pd-view-switcher { width:auto; justify-self:start; } }
        @media(max-width:560px) { .pd-view-switcher { width:100%; } .pd-view-button { flex:1 1 50%; padding:9px 7px; } .pd-head-main strong { font-size:12px; } .pd-head-actions small { display:none; } }

        /* Tận dụng chiều cao trống: tối ưu ~5 bản ghi ở màn hình desktop thay vì ép 408px. */
        .pd-personal-grid { --pd-available-card-height:600px; align-items:stretch; }
        .pd-personal-grid .pd-personal-card { height:var(--pd-available-card-height); }
        .pd-personal-card .pd-personal-list { min-height:0; max-height:none; overflow-y:auto; }
        .pd-personal-card .pd-personal-card-footer { margin-top:auto; }
        .pd-personal-meetings .pd-personal-item { padding-top:6px; padding-bottom:6px; }
        .pd-personal-meetings .pd-personal-meeting-heading { margin-bottom:4px; }
        .pd-personal-meetings .pd-personal-meeting-line { margin-top:4px; line-height:1.35; }
        @media(max-width:1450px) { .pd-personal-grid .pd-personal-card { height:550px; } }
        @media(max-width:780px) { .pd-personal-grid .pd-personal-card { height:520px; } }


        /* Thẻ họp / vấn đề: đồng bộ hàng tiêu đề + giờ công với thẻ công việc. */
        .pd-personal-meetings .pd-personal-item,
        .pd-personal-issues .pd-personal-item { padding:11px 12px; min-height:84px; }
        .pd-personal-meetings .pd-personal-task-header,
        .pd-personal-issues .pd-personal-task-header { align-items:flex-start; }
        .pd-personal-meetings .pd-personal-task-title,
        .pd-personal-issues .pd-personal-task-title { min-width:0; }
        .pd-personal-meetings .pd-personal-task-heading,
        .pd-personal-issues .pd-personal-task-heading { row-gap:5px; }
        .pd-personal-meetings .pd-personal-hours,
        .pd-personal-issues .pd-personal-hours { white-space:nowrap; }
        .pd-personal-schedule { display:flex; flex-direction:column; gap:5px; margin-top:8px; }
        .pd-personal-schedule-line { display:flex; align-items:center; gap:7px; min-width:0; color:#81748f; font-size:11.8px; line-height:1.35; }
        .pd-personal-schedule-line i { flex:0 0 14px; width:14px; text-align:center; color:#8d77a4; }
        .pd-personal-schedule-line span { min-width:0; overflow-wrap:anywhere; }
        .pd-personal-issues .pd-personal-meta { margin-top:8px; }
        .pd-detail-item .pd-personal-schedule { margin-top:10px; }

        /* Khay ghim: mặc định 4 bảng như cũ, chỉ hiện khay trái khi có bảng bỏ ghim. */
        .pd-personal-layout { display:grid; grid-template-columns:0 minmax(0,1fr); align-items:start; gap:0; min-width:0; }
        .pd-personal-layout.has-collapsed { grid-template-columns:var(--pd-rail-width,196px) minmax(0,1fr); gap:13px; }
        .pd-collapsed-rail { display:flex; flex-direction:column; gap:8px; box-sizing:border-box; min-width:0; width:100%; padding:0; border:0 solid transparent; border-radius:13px; background:transparent; visibility:hidden; opacity:0; overflow:hidden; pointer-events:none; }
        .pd-personal-layout.has-collapsed .pd-collapsed-rail { visibility:visible; opacity:1; pointer-events:auto; padding:7px; border:1px solid #e7dbee; background:#faf7ff; box-shadow:0 3px 12px rgba(65,33,103,.035); }
        .pd-personal-grid { display:grid; grid-template-columns:repeat(var(--pd-open-columns,4),minmax(0,1fr)); align-items:stretch; gap:13px; min-width:0; }
        .pd-personal-grid > .pd-personal-card { width:auto; min-width:0; max-width:none; height:var(--pd-available-card-height,600px); transition:box-shadow .22s ease; }
        .pd-collapsed-rail > .pd-personal-card.pd-unpinned { display:block; width:100%; height:52px!important; min-height:52px!important; max-height:52px!important; overflow:hidden; box-shadow:0 2px 7px rgba(83,33,142,.035); }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-personal-card-head { display:flex; align-items:center; justify-content:space-between; gap:5px; height:50px; min-height:50px; padding:7px 7px 7px 9px; border-bottom:0; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main { flex:1 1 auto; min-width:0; white-space:nowrap; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main i { display:none; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main strong { display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; font-size:12px; line-height:1.3; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-actions { display:none!important; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-card-body,.pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-personal-card-footer { display:none!important; }
        .pd-pin-toggle { appearance:none; flex:0 0 28px; width:28px; height:28px; display:inline-flex; align-items:center; justify-content:center; margin-left:auto; padding:0; border:1px solid #d9c3f3; border-radius:8px; background:#f6efff; color:#6731a0; cursor:pointer; transition:background .18s,border-color .18s,color .18s,transform .26s,box-shadow .18s; }
        .pd-pin-toggle:hover { background:#ede0fc; border-color:#ae88df; box-shadow:0 2px 8px rgba(83,33,142,.15); transform:translateY(-1px); }
        .pd-pin-toggle svg { display:block; width:15px; height:15px; color:inherit; transition:transform .33s cubic-bezier(.2,.85,.3,1); }
        .pd-pin-toggle[aria-pressed="false"] { background:#fff; border-color:#ded6e7; color:#90839f; }
        .pd-pin-toggle[aria-pressed="false"] svg { transform:rotate(-45deg); }
        .pd-pin-toggle[aria-pressed="true"] svg { transform:rotate(0); }
        .pd-pin-toggle:focus-visible { outline:2px solid #9163d1; outline-offset:2px; }
        .pd-pin-toggle.pd-pin-bounce { animation:pdPinBounce .38s ease; }
        @keyframes pdPinBounce { 0% { transform:scale(1); } 35% { transform:scale(1.15) rotate(-8deg); } 70% { transform:scale(.95) rotate(4deg); } 100% { transform:scale(1); } }
        .pd-pin-flying-head { position:fixed!important; z-index:2100; margin:0!important; pointer-events:none!important; box-sizing:border-box; background:#faf8fd; border:1px solid #e6dbee; border-radius:10px; box-shadow:0 8px 22px rgba(73,35,117,.17); overflow:hidden; }
        .pd-personal-layout.has-collapsed .pd-personal-grid { min-width:0; }
        @media(max-width:1450px) { .pd-personal-grid { grid-template-columns:repeat(var(--pd-open-columns,2),minmax(0,1fr)); } }
        @media(max-width:780px) { .pd-personal-layout.has-collapsed { grid-template-columns:minmax(142px,var(--pd-rail-width,175px)) minmax(0,1fr); gap:8px; } .pd-personal-grid { grid-template-columns:1fr; } .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main strong { font-size:11px; } }
        @media(max-width:550px) { .pd-personal-layout.has-collapsed { grid-template-columns:minmax(0,1fr); gap:10px; } .pd-collapsed-rail { flex-direction:column; } .pd-collapsed-rail > .pd-personal-card.pd-unpinned { max-width:240px; } }
        @media(prefers-reduced-motion:reduce) { .pd-pin-toggle,.pd-pin-toggle svg,.pd-personal-grid > .pd-personal-card { transition:none!important; animation:none!important; } }

    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div id="pdDetailPageCard" class="card p-3 min-vh-100">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project detail" />
           
                <asp:UpdatePanel runat="server" ID="upProjectDetail" UpdateMode="Conditional">
                    <ContentTemplate><%-- Tiêu đề và thao tác --%>
                        <SweetSoft:CtrlProjectTabs
                            runat="server"
                            ID="CtrlProjectTabs1" />

                        <%-- Toolbar 2 vùng: trái là tên dự án + thao tác; phải là nút chuyển chế độ xem. --%>
                        <div class="pd-project-toolbar">
                            <div class="pd-project-main">
                                <div class="pd-project-main-left">
                                    <div class="pd-project-identity">
                                        <h4 class="fw-semibold text-primary"><asp:Label runat="server" ID="lblTenDuAn"></asp:Label></h4>
                                        <span class="badge rounded-pill bg-light text-secondary border px-3 py-2"><asp:Label runat="server" ID="lblTrangThaiHead"></asp:Label></span>
                                    </div>
                                </div>
                                <div class="pd-project-actions">
                                    <asp:Panel runat="server" ID="pnlStatusActions" CssClass="pd-status-buttons">
                                        <asp:LinkButton runat="server" ID="lbtStatusDangThucHien" CssClass="btn btn-sm btn-outline-info" OnClick="lbtStatusDangThucHien_Click" CausesValidation="false"><i class="fas fa-play me-1"></i>Đang thực hiện</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="lbtStatusTamDung" CssClass="btn btn-sm btn-outline-warning" OnClick="lbtStatusTamDung_Click" CausesValidation="false"><i class="fas fa-pause me-1"></i>Tạm dừng</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="lbtStatusHoanThanh" CssClass="btn btn-sm btn-outline-success" OnClick="lbtStatusHoanThanh_Click" CausesValidation="false"><i class="fas fa-check me-1"></i>Hoàn thành</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="lbtStatusKetThuc" CssClass="btn btn-sm btn-outline-danger" OnClick="lbtStatusKetThuc_Click" CausesValidation="false"><i class="fas fa-times me-1"></i>Kết thúc</asp:LinkButton>
                                    </asp:Panel>
                                    <asp:UpdatePanel runat="server" ID="upEditProject" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:LinkButton runat="server" ID="lbtEditProject" CssClass="btn btn-outline-secondary" OnClick="lbtEditProject_Click" CausesValidation="false"><i class="fas fa-pencil-alt me-1"></i>Sửa</asp:LinkButton>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                            <div id="projectDetailViewSwitcher" class="pd-view-switcher" role="tablist" aria-label="Chế độ xem dự án">
                                <button type="button" id="btnProjectMyView" class="pd-view-button" data-project-view="mine" role="tab" aria-selected="false" aria-controls="projectPersonalView" title="Công việc, cuộc họp và vấn đề được giao cho bạn trong dự án này"><i class="fas fa-user-check"></i><span>Phần việc của tôi</span></button>
                                <button type="button" id="btnProjectInfoView" class="pd-view-button is-active" data-project-view="project" role="tab" aria-selected="true" aria-controls="projectInfoView" title="Xem thông tin, tiến độ và thành viên dự án"><i class="fas fa-info-circle"></i><span>Thông tin dự án</span></button>
                            </div>
                        </div>
                        <div id="projectInfoView" class="pd-view-panel" role="tabpanel" aria-labelledby="btnProjectInfoView">
                        <div class="row g-4 align-items-start">

                            <%-- Cột trái --%>
                            <div class="col-xl-8">

                                <%-- Thông tin dự án --%>
                                <section class="mb-3">

                         
                                    <div class="row g-4">
                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.CUSTOMER) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblKhachHang"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblGiaTriHopDong"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblNgayKy"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblLoaiDuAn"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.START_DATE) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblNgayBatDau"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                Ngày hoàn thành dự kiến
                                            </div>
                                            <asp:Label runat="server" ID="lblNgayHoanThanhDuKien"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <asp:Label runat="server" ID="lblNgayKetThucThucTeTitle" Text="Ngày hoàn thành thực tế"></asp:Label>
                                            </div>
                                            <asp:Label runat="server" ID="lblNgayKetThucThucTe"></asp:Label>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="font-size-8 fw-bold mb-1">
                                                <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                                            </div>
                                            <asp:Label runat="server" ID="lblTrangThai"></asp:Label>
                                        </div>
                                    </div>
                                </section>

                                <%-- Tiến độ --%>
                                <section class="mb-3">
                                    <h5 class="text-uppercase fw-bold mb-3">
                                        Tiến độ
                                    </h5>

                                    <div class="card border shadow-none rounded-3">
                                        <div class="card-body py-2 px-3">

                                            <div class="mb-4">
                                                <div class="d-flex justify-content-between align-items-center mb-2">
                                                    <span class="fs-6">
                                                        Theo tỷ trọng ngày thực hiện
                                                    </span>

                                                    <strong runat="server" id="lblTienDoThoiGian" class="small text-primary">
                                                    </strong>
                                                </div>

                                                <div runat="server" id="divTienDoThoiGian" class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100">
                                                    <div runat="server" id="divTienDoThoiGianBar" class="progress-bar bg-primary">
                                                    </div>
                                                </div>
                                            </div>

                                            <div>
                                                <div class="d-flex justify-content-between align-items-center mb-2">
                                                    <span class="fs-6">
                                                        Theo bình quân % hoàn thành công việc
                                                    </span>

                                                    <strong runat="server" id="lblTienDoCongViec" class="small text-primary">
                                                    </strong>
                                                </div>

                                                <div runat="server" id="divTienDoCongViec" class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100">
                                                    <div runat="server" id="divTienDoCongViecBar" class="progress-bar bg-primary">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </section>

                                <%-- Mô tả --%>
                                <section class="mb-3">
                                    <h5 class="text-uppercase fw-bold mb-3 ">
                                        Mô tả
                                    </h5>

                                    <div class="card border shadow-none rounded-3">
                                        <div class="card-body py-2 px-3">
                                            <asp:Literal runat="server" ID="ltrMoTa"></asp:Literal>
                                        </div>
                                    </div>
                                </section>

                                <%-- Giai đoạn --%>
                                <section>
                                    <SweetSoft:CtrlGiaiDoanDuAn runat="server" ID="CtrlGiaiDoanDuAn1" />
                                </section>
                            </div>

                            <%-- Cột phải --%>
                            <div class="col-xl-4">

                                <%-- Phụ trách --%>
                                <section class="card border shadow-none mb-3 rounded-3">
                                    <div class="card-body py-2 px-3">
                                        <h5 class="text-uppercase fw-bold mb-3">
                                            Phụ trách
                                        </h5>

                                        <div class="d-flex align-items-center gap-3">
                                            <img runat="server" src="" ID="imgAvatarPM" class="rounded-circle" style="width:30px; height:30px; object-fit:cover;" alt="avatar"/>

                                            <div>
                                                <div class="font-size-8 fw-bold mb-1">
                                                    <asp:Label runat="server" ID="lblNhanVienQuanLy"></asp:Label>
                                                </div>

                                                <div class="small text-muted">
                                                    Project Manager
                                                    <i class="fas fa-external-link-alt ms-1"></i>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="mt-2 d-flex align-items-center gap-2">
                                            <asp:Literal ID="ltrThanhVienGroup" runat="server"></asp:Literal>

                                            <asp:LinkButton runat="server" ID="lbtThemThanhVien"
                                                CssClass="btn btn-sm btn-outline-secondary rounded-circle d-flex align-items-center justify-content-center"
                                                Style="width:30px;height:30px;"
                                                OnClick="lbtThemThanhVien_Click"
                                                CausesValidation="false"
                                                ToolTip="Thêm thành viên">
                                                <i class="fas fa-plus"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </section>

                                <%-- Hợp đồng --%>
                                <asp:Panel
                                    runat="server"
                                    ID="pnlContract"
                                    VisibleConditionKey='<%# this.IsContractView %>'>

                                    <section class="card border shadow-none mb-3 rounded-3">
                                        <div class="card-body py-2 px-3">
                                            <h6 class="text-uppercase fw-bold mb-3">
                                                Hợp đồng thực hiện
                                            </h6>

                                            <asp:UpdatePanel
                                                runat="server"
                                                ID="upnlOpenContract"
                                                UpdateMode="Conditional"
                                                RenderMode="Inline">

                                                <ContentTemplate>
                                                    <asp:LinkButton
                                                        runat="server"
                                                        ID="lbtViewContract"
                                                        CausesValidation="false"
                                                        CssClass="text-primary text-decoration-none"
                                                        OnClick="lbtViewContract_Click">

                                                        <i class="fas fa-file-contract me-2"></i>

                                                        <asp:Label
                                                            runat="server"
                                                            ID="lblSoHopDong">
                                                        </asp:Label>

                                                        <i class="fas fa-external-link-alt ms-1 small"></i>
                                                    </asp:LinkButton>

                                                    <asp:Label
                                                        runat="server"
                                                        ID="lblNoContract"
                                                        CssClass="text-muted"
                                                        Visible="false">
                                                        Chưa có hợp đồng thực hiện
                                                    </asp:Label>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                    </section>

                                </asp:Panel>

                                <%-- Hoạt động gần đây --%>
                                <section class="card border shadow-none rounded-3">
                                    <div class="card-body py-2 px-3">

                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                            <h5 class="text-uppercase fw-bold mb-3 mb-0">
                                                Hoạt động gần đây
                                            </h5>

                                            <asp:UpdatePanel runat="server" ID="upnlOpenProjectHistory" UpdateMode="Conditional" RenderMode="Inline">

                                                <ContentTemplate>
                                                    <asp:LinkButton runat="server" ID="lbtViewAllHistory" CausesValidation="false" CssClass="small text-primary text-decoration-none" OnClick="lbtViewAllHistory_Click">

                                                        Xem tất cả
                                                    </asp:LinkButton>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>

                                        <div class="list-group list-group-flush">

                                            <asp:Repeater runat="server" ID="rptRecentProjectHistory" OnItemDataBound="rptRecentProjectHistory_ItemDataBound">

                                                <ItemTemplate>
                                                    <div class="list-group-item px-0 py-3">
                                                        <div class="d-flex gap-3">
                                                            <span class="text-primary pt-1">
                                                                <i class="far fa-circle"></i>
                                                            </span>

                                                            <div class="flex-grow-1">
                                                                <asp:Label runat="server" ID="lblHistoryContent" CssClass="small">
                                                                </asp:Label>

                                                                <div class="small text-muted mt-1">
                                                                    <asp:Label runat="server" ID="lblHistoryTime">
                                                                    </asp:Label>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>

                                            <asp:Panel runat="server" ID="pnlEmptyRecentHistory" Visible="false" CssClass="text-muted small py-3">

                                                Chưa có hoạt động nào.
                                            </asp:Panel>

                                        </div>
                                    </div>
                                </section>
                            </div>
                        </div> <%-- /row g-4 --%>
                        </div> <%-- /projectInfoView --%>
                        <div id="projectPersonalView" class="pd-view-panel" role="tabpanel" aria-labelledby="btnProjectMyView" hidden>
                            <%-- Dynamic UI only - no new designer.cs field needed. --%>
                            <asp:Literal runat="server" ID="litProjectPersonal" EnableViewState="false" />
                        </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    </div>
</div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="mdlPersonalDetail" Type="Primary" Title="Chi tiết phần việc" Width="92%">
        <ContentTemplate>
            <div class="pd-detail-modal-wrap">
                <h4 id="pdPersonalDetailTitle" class="pd-detail-modal-title">Chi tiết</h4>
                <p id="pdPersonalDetailSub" class="pd-detail-modal-sub">Xem thêm thông tin trong phạm vi dự án hiện tại.</p>
                <div id="pdPersonalDetailModalBody"></div>
            </div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="mdlStatusChange" Type="Primary">
        <ContentTemplate>
            <asp:UpdatePanel ID="upStatusChange" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <asp:HiddenField ID="hfStatusTarget" runat="server" />
                        <asp:HiddenField ID="hfStatusAtModalOpen" runat="server" />
                        <p class="text-muted" id="lblStatusDescription" runat="server"></p>
                        <div class="mb-3">
                            <label class="form-label fw-bold" for="<%= txtStatusReason.ClientID %>">
                                Lý do <span class="text-danger">*</span>
                            </label>
                            <asp:TextBox ID="txtStatusReason" runat="server" CssClass="form-control"
                                TextMode="MultiLine" Rows="4" MaxLength="1000"
                                placeholder="Nhập lý do..."></asp:TextBox>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel ID="upStatusChangeFooter" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:LinkButton ID="lbtConfirmStatusChange" runat="server"
                        CssClass="btn btn-primary"
                        CausesValidation="false"
                        OnClick="lbtConfirmStatusChange_Click">
                        <i class="fas fa-check me-1"></i> Xác nhận
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:CtrlLichSuDuAn runat="server" ID="CtrlLichSuDuAn1" />
    <SweetSoft:CtrlDuAnForm runat="server" ID="CtrlDuAnForm1" />
    <SweetSoft:CtrlChonNhanVien runat="server" ID="CtrlChonNhanVien1" />
    <SweetSoft:ExtraModal
    runat="server"
    ID="dlContractDetail"
    Type="Primary"
    Title="Thông tin hợp đồng thực hiện">

    <ContentTemplate>
        <div class="row">

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Số hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractNumber"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Tên hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractName"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Khách hàng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractCustomer"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Giá trị hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractValue"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày ký
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractSignDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày hiệu lực
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractEffectiveDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày hết hạn
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractExpiryDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label">
                        Mô tả
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractDescription"
                        TextMode="MultiLine"
                        Rows="4"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        (function () {
            if (window.ProjectDetailViewSwitchBound) return;
            window.ProjectDetailViewSwitchBound = true;
            var storageKey = 'qlda-project-detail-view:' + window.location.pathname + window.location.search;
            function getRemembered() { try { return window.sessionStorage.getItem(storageKey) === 'mine' ? 'mine' : 'project'; } catch (ex) { return 'project'; } }
            var pdHeightRaf = 0;
            function fitPersonalCardsToViewport() {
                var view = document.getElementById('projectPersonalView');
                var grid = view && view.querySelector('.pd-personal-grid');
                if (!grid || view.hidden) return;
                if (!window.matchMedia || !window.matchMedia('(min-width:1451px)').matches) {
                    grid.style.removeProperty('--pd-available-card-height');
                    return;
                }
                // Phần còn lại đến footer hệ thống; không tính lại theo vị trí cuộn hiện tại.
                var topInDocument = grid.getBoundingClientRect().top + (window.pageYOffset || document.documentElement.scrollTop || 0);
                var footer = document.querySelector('footer, #footer, .footer, .footer-page');
                var footerHeight = footer ? Math.min(55, Math.max(24, footer.getBoundingClientRect().height)) : 28;
                var safeGap = footerHeight + 18;
                var available = Math.floor(window.innerHeight - topInDocument - safeGap);
                // Màn hình thấp vẫn dùng được; màn hình cao không tạo bảng quá dài.
                var height = Math.max(350, Math.min(780, available));
                grid.style.setProperty('--pd-available-card-height', height + 'px');
            }
            function schedulePersonalHeightFit() {
                if (pdHeightRaf) window.cancelAnimationFrame(pdHeightRaf);
                pdHeightRaf = window.requestAnimationFrame(function () { pdHeightRaf = 0; fitPersonalCardsToViewport(); });
            }
            function showView(value, remember) {
                var root = document.getElementById('projectDetailViewSwitcher');
                var mine = document.getElementById('projectPersonalView');
                var project = document.getElementById('projectInfoView');
                if (!root || !mine || !project) return;
                var isMine = value === 'mine';
                mine.hidden = !isMine;
                project.hidden = isMine;
                Array.prototype.forEach.call(root.querySelectorAll('[data-project-view]'), function (button) {
                    var active = button.getAttribute('data-project-view') === (isMine ? 'mine' : 'project');
                    button.classList.toggle('is-active', active);
                    button.setAttribute('aria-selected', active ? 'true' : 'false');
                    button.setAttribute('tabindex', active ? '0' : '-1');
                });
                // min-vh-100 bắt toàn bộ thẻ cao ít nhất 100vh, kể cả đã có header trên trang.
                // Chỉ bỏ giới hạn này trong view cá nhân, giữ nguyên cho Thông tin dự án.
                var pageCard = document.getElementById('pdDetailPageCard');
                if (pageCard) pageCard.classList.toggle('min-vh-100', !isMine);
                if (isMine) schedulePersonalHeightFit();
                if (remember) { try { window.sessionStorage.setItem(storageKey, isMine ? 'mine' : 'project'); } catch (ex) {} }
            }
            function findModalByClientId(clientId) {
                var direct = document.getElementById(clientId);
                var modal = direct && direct.classList && direct.classList.contains('modal') ? direct : (direct ? direct.querySelector('.modal') : null);
                return modal;
            }
            function openBootstrapModal(clientId) {
                var modal = findModalByClientId(clientId);
                if (!modal) return false;
                if (window.bootstrap && window.bootstrap.Modal) {
                    (window.bootstrap.Modal.getInstance && window.bootstrap.Modal.getInstance(modal) || new window.bootstrap.Modal(modal)).show();
                } else if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
                    window.jQuery(modal).modal('show');
                }
                return false;
            }
            var pdFilters = { task: 'all', meeting: 'all', issue: 'all' };
            var pdFilterSettings = {
                task: { id: 'pdPersonalTaskCard', hiddenClass: 'pd-task-done', text: 'Chưa hoàn thành', empty: 'Không có công việc chưa hoàn thành.' },
                meeting: { id: 'pdPersonalMeetingCard', hiddenClass: 'pd-meeting-ended', text: 'Chưa kết thúc', empty: 'Không có cuộc họp chưa kết thúc.' },
                issue: { id: 'pdPersonalIssueCard', hiddenClass: 'pd-issue-processed', text: 'Chưa giải quyết', empty: 'Không có vấn đề chưa giải quyết.' }
            };
            function syncPersonalFilter(type) {
                var setting = pdFilterSettings[type];
                if (!setting) return;
                var card = document.getElementById(setting.id);
                if (!card) return;
                var onlyOpen = pdFilters[type] === 'open';
                card.classList.toggle('is-open-only', onlyOpen);
                var button = card.querySelector('[data-pd-toggle-filter]');
                if (button) {
                    var label = button.querySelector('[data-pd-filter-label]');
                    if (label) label.textContent = onlyOpen ? setting.text : 'Tất cả';
                    button.classList.toggle('is-filtered', onlyOpen);
                    button.setAttribute('aria-pressed', onlyOpen ? 'true' : 'false');
                    button.setAttribute('title', onlyOpen ? 'Chuyển về tất cả' : 'Chỉ hiển thị ' + setting.text.toLowerCase());
                }
                var records = card.querySelectorAll('[data-pd-record]');
                var visibleCount = 0;
                Array.prototype.forEach.call(records, function (row) {
                    if (!onlyOpen || !row.classList.contains(setting.hiddenClass)) visibleCount++;
                });
                var empty = card.querySelector('.pd-filter-empty');
                if (empty) empty.hidden = !onlyOpen || records.length === 0 || visibleCount !== 0;
            }
            function openPersonalDetail(button) {
                var sourceId = button.getAttribute('data-pd-detail-source');
                var source = sourceId ? document.getElementById(sourceId) : null;
                var body = document.getElementById('pdPersonalDetailModalBody');
                var title = document.getElementById('pdPersonalDetailTitle');
                var sub = document.getElementById('pdPersonalDetailSub');
                if (!source || !body) return false;
                if (title) title.textContent = button.getAttribute('data-pd-detail-title') || 'Chi tiết';
                if (sub) sub.textContent = button.getAttribute('data-pd-detail-sub') || 'Xem thêm thông tin trong phạm vi dự án hiện tại.';
                body.innerHTML = source.innerHTML;
                return openBootstrapModal('<%= mdlPersonalDetail.ClientID %>');
            }
            document.addEventListener('click', function (event) {
                var viewButton = event.target.closest ? event.target.closest('#projectDetailViewSwitcher [data-project-view]') : null;
                if (viewButton) { event.preventDefault(); showView(viewButton.getAttribute('data-project-view'), true); return; }
                var filterButton = event.target.closest ? event.target.closest('[data-pd-toggle-filter]') : null;
                if (filterButton) {
                    event.preventDefault();
                    var type = filterButton.getAttribute('data-pd-toggle-filter');
                    if (Object.prototype.hasOwnProperty.call(pdFilters, type)) {
                        pdFilters[type] = pdFilters[type] === 'all' ? 'open' : 'all';
                        syncPersonalFilter(type);
                    }
                    return;
                }
                var pinButton = event.target.closest ? event.target.closest('[data-pd-pin-card]') : null;
                if (pinButton) {
                    event.preventDefault();
                    var pinName = pinButton.getAttribute('data-pd-pin-card');
                    if (Object.prototype.hasOwnProperty.call(pdPins, pinName)) {
                        // Đo header trước khi chuyển bảng sang khay hoặc trả về vị trí cũ.
                        var movingCard = document.querySelector('#projectPersonalView ' + pdPinSelectors[pinName]);
                        var movingHead = movingCard && movingCard.querySelector('.pd-personal-card-head');
                        var before = movingHead ? movingHead.getBoundingClientRect() : null;
                        var ghost = movingHead && before && before.width > 0 ? movingHead.cloneNode(true) : null;
                        var oldRects = snapshotOpenCards();
                        pdPins[pinName] = !pdPins[pinName];
                        savePinnedCards();
                        applyPersonalPin(pinName, true);
                        layoutPersonalPins();
                        animatePersonalPin(movingHead, before, ghost, oldRects);
                        schedulePersonalHeightFit();
                    }
                    return;
                }
                var detailButton = event.target.closest ? event.target.closest('[data-pd-detail-source]') : null;
                if (detailButton) { event.preventDefault(); openPersonalDetail(detailButton); return; }
            });
            // Ghim/bỏ ghim: control cá nhân được render lại sau UpdatePanel, nên lắp nút idempotent.
            var pdPinStorageKey = 'qlda-project-pins:' + window.location.pathname + window.location.search;
            var pdPinNames = { task: 'Công việc được giao', meeting: 'Cuộc họp của tôi', issue: 'Vấn đề được giao' };
            var pdPinSelectors = { task: '#pdPersonalTaskCard', meeting: '#pdPersonalMeetingCard', issue: '#pdPersonalIssueCard' };
            function readPinnedCards() {
                var result = { task: true, meeting: true, issue: true };
                try {
                    var saved = JSON.parse(window.sessionStorage.getItem(pdPinStorageKey) || '{}');
                    Object.keys(result).forEach(function (key) { if (saved[key] === false) result[key] = false; });
                } catch (ignored) { }
                return result;
            }
            var pdPins = readPinnedCards();
            function savePinnedCards() {
                try { window.sessionStorage.setItem(pdPinStorageKey, JSON.stringify(pdPins)); } catch (ignored) { }
            }
            var pdPinOrder = [
                { key: 'task', selector: '#pdPersonalTaskCard' },
                { key: 'meeting', selector: '#pdPersonalMeetingCard' },
                { key: 'issue', selector: '#pdPersonalIssueCard' },
                { key: 'hours', selector: '.pd-personal-time' }
            ];
            function ensurePersonalDock() {
                var view = document.getElementById('projectPersonalView');
                var grid = view && view.querySelector('.pd-personal-grid');
                if (!grid) return null;
                var layout = grid.parentElement;
                if (!layout || !layout.classList.contains('pd-personal-layout')) {
                    layout = document.createElement('div');
                    layout.className = 'pd-personal-layout';
                    grid.parentNode.insertBefore(layout, grid);
                    layout.appendChild(grid);
                }
                var rail = layout.querySelector('.pd-collapsed-rail');
                if (!rail) {
                    rail = document.createElement('aside');
                    rail.className = 'pd-collapsed-rail';
                    rail.setAttribute('aria-label', 'Các bảng đã bỏ ghim');
                    layout.insertBefore(rail, grid);
                }
                return { layout: layout, rail: rail, grid: grid };
            }
            function pinCard(key) { return document.querySelector('#projectPersonalView ' + pdPinSelectors[key]); }
            function applyPersonalPin(key, animate) {
                var card = pinCard(key);
                if (!card) return;
                var pinned = pdPins[key] !== false;
                var head = card.querySelector('.pd-personal-card-head');
                if (!head) return;
                var button = head.querySelector('[data-pd-pin-card]');
                if (!button) {
                    button = document.createElement('button');
                    button.type = 'button';
                    button.className = 'pd-pin-toggle';
                    button.setAttribute('data-pd-pin-card', key);
                    button.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 3h8l-1.5 6 3.5 4v2H6v-2l3.5-4L8 3Z"/><path d="M12 15v6"/></svg>';
                    head.appendChild(button);
                }
                card.classList.toggle('pd-unpinned', !pinned);
                button.setAttribute('aria-pressed', pinned ? 'true' : 'false');
                var tip = pinned ? 'Bỏ ghim và đưa bảng ' + pdPinNames[key] + ' vào khay bên trái' : 'Ghim và mở rộng bảng ' + pdPinNames[key];
                button.title = tip;
                button.setAttribute('aria-label', tip);
                if (animate) {
                    button.classList.remove('pd-pin-bounce');
                    void button.offsetWidth;
                    button.classList.add('pd-pin-bounce');
                    window.setTimeout(function () { button.classList.remove('pd-pin-bounce'); }, 420);
                }
            }
            function layoutPersonalPins() {
                var dock = ensurePersonalDock();
                if (!dock) return;
                // Chuyển chính thẻ bảng, không sao chép dữ liệu; các bộ lọc và nút chi tiết giữ nguyên.
                var unpinned = pdPinOrder.filter(function (x) { return x.key !== 'hours' && pdPins[x.key] === false; });
                var opened = pdPinOrder.filter(function (x) { return x.key === 'hours' || pdPins[x.key] !== false; });
                var railWidth = 200;
                unpinned.forEach(function (entry) {
                    var card = document.querySelector('#projectPersonalView ' + entry.selector);
                    var label = card && card.querySelector('.pd-head-main strong');
                    if (label) railWidth = Math.max(railWidth, (label.scrollWidth || 118) + 78);
                });
                railWidth = Math.min(252, Math.max(198, Math.ceil(railWidth)));
                dock.layout.style.setProperty('--pd-rail-width', railWidth + 'px');
                dock.layout.classList.toggle('has-collapsed', unpinned.length > 0);
                dock.rail.setAttribute('aria-hidden', unpinned.length ? 'false' : 'true');
                // Khay luôn đứng sát trái; mỗi hàng đúng một header thu gọn.
                function keepOrdered(target, entries) {
                    var previous = null;
                    entries.forEach(function (entry) {
                        var card = document.querySelector('#projectPersonalView ' + entry.selector);
                        if (!card) return;
                        var expectedNext = previous ? previous.nextSibling : target.firstChild;
                        if (card.parentNode !== target || expectedNext !== card) target.insertBefore(card, expectedNext);
                        previous = card;
                    });
                }
                keepOrdered(dock.rail, unpinned);
                keepOrdered(dock.grid, opened);
                var count = opened.length;
                var maxColumns = window.innerWidth > 1450 ? 4 : window.innerWidth > 780 ? 2 : 1;
                dock.grid.style.setProperty('--pd-open-columns', Math.max(1, Math.min(count, maxColumns)));
            }
            function snapshotOpenCards() {
                var rects = [];
                var dock = ensurePersonalDock();
                if (!dock) return rects;
                Array.prototype.forEach.call(dock.grid.querySelectorAll('.pd-personal-card'), function (card) {
                    var rect = card.getBoundingClientRect();
                    if (rect.width > 0) rects.push({ card: card, left: rect.left, top: rect.top });
                });
                return rects;
            }
            function animatePersonalPin(head, before, ghost, oldRects) {
                if (!head || !before || !ghost || !before.width || window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
                var after = head.getBoundingClientRect();
                if (!after.width) return;
                var duration = 430;
                // Di chuyển các bảng còn ghim êm hơn khi diện tích thay đổi.
                if (typeof Element !== 'undefined' && Element.prototype.animate) {
                    oldRects.forEach(function (item) {
                        if (item.card.contains(head) || item.card.classList.contains('pd-unpinned') || !item.card.isConnected) return;
                        var last = item.card.getBoundingClientRect();
                        var dx = item.left - last.left, dy = item.top - last.top;
                        if (Math.abs(dx) + Math.abs(dy) > 2) item.card.animate([{ transform: 'translate(' + dx + 'px,' + dy + 'px)' }, { transform: 'translate(0,0)' }], { duration: duration, easing: 'cubic-bezier(.2,.8,.2,1)' });
                    });
                }
                ghost.classList.add('pd-pin-flying-head');
                ghost.setAttribute('aria-hidden', 'true');
                ghost.style.left = before.left + 'px';
                ghost.style.top = before.top + 'px';
                ghost.style.width = before.width + 'px';
                ghost.style.height = before.height + 'px';
                document.body.appendChild(ghost);
                head.style.visibility = 'hidden';
                var cleanup = function () { ghost.remove(); if (head.isConnected) head.style.visibility = ''; };
                if (ghost.animate) {
                    var animation = ghost.animate([
                        { left: before.left + 'px', top: before.top + 'px', width: before.width + 'px', height: before.height + 'px', opacity: 1 },
                        { left: after.left + 'px', top: after.top + 'px', width: after.width + 'px', height: after.height + 'px', opacity: 1 }
                    ], { duration: duration, easing: 'cubic-bezier(.2,.8,.2,1)', fill: 'forwards' });
                    animation.addEventListener('finish', cleanup, { once: true });
                    animation.addEventListener('cancel', cleanup, { once: true });
                } else {
                    ghost.style.transition = 'left .43s ease,top .43s ease,width .43s ease,height .43s ease';
                    window.requestAnimationFrame(function () { ghost.style.left = after.left + 'px'; ghost.style.top = after.top + 'px'; ghost.style.width = after.width + 'px'; ghost.style.height = after.height + 'px'; });
                    window.setTimeout(cleanup, duration + 30);
                }
            }
            function restorePersonalPins() {
                var hoursCard = document.querySelector('#projectPersonalView .pd-personal-time');
                if (hoursCard) {
                    hoursCard.classList.remove('pd-unpinned');
                    var formerButton = hoursCard.querySelector('[data-pd-pin-card]');
                    if (formerButton) formerButton.remove();
                }
                Object.keys(pdPinSelectors).forEach(function (key) { applyPersonalPin(key, false); });
                layoutPersonalPins();
            }
            function restore() {
                showView(getRemembered(), false);
                Object.keys(pdFilters).forEach(syncPersonalFilter);
                restorePersonalPins();
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', restore);
            else restore();
            if (window.Sys && Sys.Application) Sys.Application.add_load(restore);
            window.addEventListener('resize', function () { layoutPersonalPins(); schedulePersonalHeightFit(); });
        })();
    </script>
</asp:Content>
