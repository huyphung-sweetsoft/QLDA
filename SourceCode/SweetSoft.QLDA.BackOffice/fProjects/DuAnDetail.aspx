<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlGiaiDoanDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlGiaiDoanDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlLichSuDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlLichSuDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAnForm.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAnForm" %>
<%@ Register Src="~/fProjects/Controls/CtrlChonNhanVien.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVien" %>
<%@ Register Src="~/fProjects/Controls/CtrlChiTietCong.ascx" TagPrefix="SweetSoft" TagName="CtrlChiTietCong" %>
<%@ Register Src="~/fProjects/Controls/CtrlChiTietCongCuocHop.ascx" TagPrefix="SweetSoft" TagName="CtrlChiTietCongCuocHop" %>
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
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main { flex:1 1 auto; min-width:0; gap:7px; white-space:nowrap; }
        /* Khay thu gon luon giu icon dai dien cua tung bang. */
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main > i { display:inline-flex; flex:0 0 15px; width:15px; align-items:center; justify-content:center; color:#6b34ac; font-size:13px; }
        .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main strong { display:block; min-width:0; flex:1 1 auto; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; font-size:12px; line-height:1.3; }
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
        /* Ban sao header khi ghim/bo ghim CHI gom icon + ten, khong keo theo badge/filter/nut. */
        .pd-pin-flying-head { position:fixed!important; display:flex!important; align-items:center!important; justify-content:flex-start!important; z-index:2100; margin:0!important; padding:8px 12px!important; pointer-events:none!important; box-sizing:border-box; background:#faf8fd; border:1px solid #e6dbee; border-radius:10px; box-shadow:0 8px 22px rgba(73,35,117,.12); overflow:hidden; }
        .pd-pin-flying-head .pd-head-main { display:flex; flex:1 1 auto; min-width:0; align-items:center; gap:7px; white-space:nowrap; }
        .pd-pin-flying-head .pd-head-main > i { display:inline-flex!important; flex:0 0 15px; width:15px; align-items:center; justify-content:center; color:#6b34ac; font-size:13px; }
        .pd-pin-flying-head .pd-head-main strong { min-width:0; display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; font-size:12px; line-height:1.3; }
        .pd-pin-flying-head .pd-head-actions,.pd-pin-flying-head .pd-pin-toggle { display:none!important; }
        .pd-personal-layout.has-collapsed .pd-personal-grid { min-width:0; }
        @media(max-width:1450px) { .pd-personal-grid { grid-template-columns:repeat(var(--pd-open-columns,2),minmax(0,1fr)); } }
        @media(max-width:780px) { .pd-personal-layout.has-collapsed { grid-template-columns:minmax(142px,var(--pd-rail-width,175px)) minmax(0,1fr); gap:8px; } .pd-personal-grid { grid-template-columns:1fr; } .pd-collapsed-rail .pd-personal-card.pd-unpinned .pd-head-main strong { font-size:11px; } }
        @media(max-width:550px) { .pd-personal-layout.has-collapsed { grid-template-columns:minmax(0,1fr); gap:10px; } .pd-collapsed-rail { flex-direction:column; } .pd-collapsed-rail > .pd-personal-card.pd-unpinned { max-width:240px; } }
        @media(prefers-reduced-motion:reduce) { .pd-pin-toggle,.pd-pin-toggle svg,.pd-personal-grid > .pd-personal-card { transition:none!important; animation:none!important; } }


        /* Quan ly cong thanh vien: 2 cot, khong lam gian danh sach khi mo chi tiet. */
        .pd-team-layout{display:grid;grid-template-columns:minmax(255px,31%) minmax(0,1fr);gap:14px;min-height:480px;align-items:stretch}
        .pd-team-side,.pd-team-detail{min-width:0;border:1px solid #e4deed;border-radius:13px;background:#fff;overflow:hidden;box-shadow:0 3px 14px rgba(47,31,72,.035)}
        .pd-team-side{display:flex;flex-direction:column;min-height:0}
        .pd-team-side-head,.pd-team-detail-head{display:flex;align-items:center;justify-content:space-between;gap:10px;min-height:56px;padding:12px 14px;border-bottom:1px solid #ece7f4;background:#faf8fd}
        .pd-team-side-head strong,.pd-team-detail-head strong{font-size:13.5px;color:#382b4c}
        .pd-team-side-head small,.pd-team-detail-head small{flex:none;border-radius:999px;background:#f0e7fb;color:#6330a1;font-size:10.5px;font-weight:800;padding:4px 8px}
        .pd-team-search{position:relative;margin:12px 12px 9px}
        .pd-team-search>i{position:absolute;left:12px;top:50%;transform:translateY(-50%);color:#907ca7;font-size:12px;pointer-events:none}
        .pd-team-search input{width:100%;height:36px;padding:6px 12px 6px 33px;border:1px solid #e3dbea;border-radius:8px;outline:0;background:#fff;color:#372d47;font-size:12px}
        .pd-team-search input:focus{border-color:#a67ad1;box-shadow:0 0 0 3px rgba(113,52,169,.08)}
        .pd-team-people{min-height:0;max-height:580px;overflow:auto;padding:0 9px 10px;scrollbar-width:thin;scrollbar-color:#d7c5e8 transparent}
        .pd-team-person{appearance:none;display:flex;align-items:center;gap:10px;width:100%;padding:10px;border:1px solid transparent;border-radius:10px;background:transparent;text-align:left;color:#41354f;cursor:pointer;transition:background .17s,border-color .17s;}
        .pd-team-person[hidden],.pd-team-no-match[hidden]{display:none!important}
        .pd-team-person + .pd-team-person{margin-top:4px}
        .pd-team-person:hover{background:#f7f1ff;border-color:#e6d5f7}
        .pd-team-person.is-active{background:#f1e9fc;border-color:#ceb7ed;box-shadow:inset 3px 0 #6c32a8}
        .pd-team-person:focus-visible{outline:2px solid #a06bce;outline-offset:1px}
        .pd-team-avatar{position:relative;display:grid;place-items:center;flex:0 0 38px;width:38px;height:38px;border-radius:50%;overflow:hidden;background:#fff;border:1px solid #d9caeb}
        .pd-team-avatar img{display:block;width:100%;height:100%;object-fit:cover}
        .pd-team-avatar-fallback{display:grid;place-items:center;position:absolute;inset:0;color:#fff;font-size:11px;font-weight:800}
        .pd-team-avatar-fallback[hidden]{display:none!important}
        .pd-team-person-info{flex:1;min-width:0}
        .pd-team-person-name{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:12.5px;font-weight:750;color:#342748}
        .pd-team-person-email{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;margin-top:3px;font-size:10.5px;color:#8b809a}
        .pd-team-pm{flex:none;padding:3px 5px;border-radius:5px;background:#efe3fb;color:#6a32a1;font-size:10px;font-weight:850}
        .pd-team-no-match{padding:13px;text-align:center;color:#9888a5;font-size:12px}
        .pd-team-detail{display:flex;flex-direction:column}
        .pd-team-detail-person{display:flex;align-items:center;gap:10px;min-width:0}
        .pd-team-detail-person>i{color:#6b34ac}
        .pd-team-detail-name{display:block;font-size:13.5px;font-weight:800;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:#382b4c}
        .pd-team-detail-email{display:block;font-size:11px;color:#8b809a;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
        .pd-team-detail-head small{white-space:nowrap}
        .pd-team-detail-body{flex:1;min-height:0;padding:13px;max-height:650px;overflow-y:auto;scrollbar-width:thin;scrollbar-color:#d7c5e8 transparent}
        .pd-team-hours-banner{display:flex;align-items:center;gap:7px;margin-bottom:10px;padding:9px 11px;border:1px solid #ede2f5;border-radius:9px;background:#fbf8ff;color:#79658a;font-size:11px;line-height:1.5}
        .pd-team-hours-banner i{color:#7138a8}
        .pd-team-accordion{margin-bottom:9px;border:1px solid #e8dfef;border-radius:10px;overflow:hidden;background:#fdfcff}
        .pd-team-accordion.is-total{border-color:#d8bdf9;background:#f5effc}
        .pd-team-accordion-header{appearance:none;display:flex;align-items:center;gap:10px;width:100%;min-height:54px;padding:10px 12px;border:0;background:transparent;text-align:left;cursor:pointer;color:#3e3150}
        .pd-team-accordion-header:hover{background:#f5f0fc}
        .pd-team-accordion-header:focus-visible{outline:2px solid #a57adb;outline-offset:-2px}
        .pd-team-accordion-title{flex:1;min-width:0;font-size:12.5px;font-weight:750}
        .pd-team-hours-value{flex:none;white-space:nowrap;font-size:13px;font-weight:850;color:#6630a1}
        .pd-team-accordion-header>i{flex:none;font-size:11px;color:#987ead;transition:transform .2s}
        .pd-team-accordion.is-expanded .pd-team-accordion-header>i{transform:rotate(180deg)}
        .pd-team-accordion-content{border-top:1px solid #eee8f5;background:#fff;padding:6px 10px 10px}
        .pd-team-accordion-content[hidden]{display:none!important}
        .pd-team-record{display:flex;align-items:flex-start;justify-content:space-between;gap:8px;padding:9px 2px;border-bottom:1px solid #f0ebf6}
        .pd-team-record:last-child{border-bottom:0}
        .pd-team-record-main{flex:1;min-width:0}
        .pd-team-record-title{font-size:12px;font-weight:750;color:#3a3049;line-height:1.4;overflow-wrap:anywhere}
        .pd-team-record-code{margin-right:5px;padding:2px 5px;border:1px solid #decdf5;border-radius:5px;background:#f5effe;color:#6634a2;font-size:10px}
        .pd-team-record-meta{display:flex;align-items:center;gap:8px;flex-wrap:wrap;margin-top:3px;color:#94899f;font-size:10.5px}
        .pd-team-record-state{flex:none;max-width:122px;padding:3px 6px;border-radius:6px;background:#f3eff8;color:#775b91;font-size:10px;font-weight:700;text-align:center;line-height:1.3}
        .pd-team-record.is-extra{display:none}
        .pd-team-accordion.show-all .pd-team-record.is-extra{display:flex}
        .pd-team-show-all{appearance:none;margin-top:8px;border:0;background:transparent;color:#6c35a2;font-size:11px;font-weight:800;cursor:pointer}
        .pd-team-record-empty{padding:13px 5px;text-align:center;color:#978aa3;font-size:11.5px}
        .pd-team-hours-footnote{font-size:11px;color:#80758f;line-height:1.55;padding:7px 3px 2px}
        @media(max-width:900px){.pd-team-layout{grid-template-columns:minmax(215px,34%) minmax(0,1fr);gap:10px}}
        @media(max-width:680px){.pd-team-layout{grid-template-columns:1fr}.pd-team-people{max-height:230px}.pd-team-detail-body{max-height:75vh}.pd-team-side-head,.pd-team-detail-head{min-height:50px}.pd-view-switcher{max-width:100%;flex-wrap:wrap}.pd-view-button{flex:1 1 auto;white-space:nowrap}}
        @media(prefers-reduced-motion:reduce){.pd-team-accordion-header>i,.pd-team-person{transition:none}}


        /* Chi tiet tu khai: chi hien khi cot du rong, khong day danh sach cao them luc hep. */
        .pd-personal-card{container-type:inline-size}
        .pd-personal-ds-breakdown{display:none;margin-top:5px;font-size:11px;line-height:1.35;color:#8a779a}
        @container (min-width: 430px){.pd-personal-ds-breakdown{display:block}}
        /* Doi soat: nut mo rong va bang doc theo chieu ngang, dung mau team view. */
        .pd-team-top-actions{display:flex;align-items:center;gap:8px;margin-left:auto}
        .pd-team-accordion{position:relative}
        .pd-team-accordion:not(.is-total) .pd-team-accordion-header{padding-right:12px}
        .pd-team-accordion:not(.is-total) .pd-team-hours-value{margin-right:37px}
        .pd-ds-expand{appearance:none;display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;flex:none;border:1px solid #d9c3f3;border-radius:8px;background:#fff;color:#6d32a0;cursor:pointer;transition:background .16s,color .16s,transform .16s}
        .pd-ds-expand:hover{background:#f0e7fb;color:#512084;transform:scale(1.055)}
        .pd-ds-expand:focus-visible{outline:2px solid #9461c4;outline-offset:2px}
        .pd-ds-expand-section{position:absolute;right:33px;top:12px;width:29px;height:29px}
        .pd-ds-expand i{font-size:13px}
        .pd-ds-hidden-sources[hidden]{display:none!important}
        .pd-ds-table-scroller{max-width:100%;overflow:auto;margin:12px 0 15px;border:1px solid #eadff4;border-radius:11px}
        .pd-ds-table{border-collapse:separate;border-spacing:0;width:100%;min-width:900px;color:#392e4e;font-size:12.5px}
        .pd-ds-table th{position:sticky;top:0;background:#f6f1fc;color:#5c298b;text-align:left;font-weight:750;white-space:nowrap;z-index:1}
        .pd-ds-table th,.pd-ds-table td{padding:11px 12px;border-bottom:1px solid #efe9f5;vertical-align:middle}
        .pd-ds-table tbody tr:hover{background:#fcfaff}
        .pd-ds-table tbody tr:last-child td{border-bottom:0}
        .pd-ds-name{min-width:215px;font-weight:650}
        .pd-ds-table td:not(.pd-ds-name){white-space:nowrap;font-variant-numeric:tabular-nums}
        .pd-ds-table td strong{font-weight:800;color:#603095}
        .pd-ds-status{display:inline-block;border-radius:6px;background:#f5eefc;color:#69329b;padding:4px 7px;font-size:11px;font-weight:700}
        .pd-ds-link-note{display:block;margin-top:5px;color:#937a9d;font-weight:400;max-width:350px;white-space:normal}
        .pd-ds-subtitle{margin:17px 0 7px;color:#4d237b;font-size:15px;font-weight:800}
        .pd-ds-footnote{margin-top:10px;color:#877997;font-size:12px;line-height:1.6}
        .pd-detail-modal-wrap .pd-ds-table-scroller{max-height:57vh}

        /* V10 - Popup chi tiet bang cong: rong tren desktop, chi scroll doc ben trong modal. */
        .modal.pd-work-detail-modal { overflow-y:hidden!important; padding-right:0!important; }
        .modal.pd-work-detail-modal .modal-dialog {
            width:min(1340px,calc(100vw - 40px))!important;
            max-width:calc(100vw - 40px)!important;
            margin:26px auto!important;
            min-height:0!important;
            max-height:calc(100vh - 52px)!important;
            max-height:calc(100dvh - 52px)!important;
        }
        .modal.pd-work-detail-modal .modal-content {
            display:flex!important;flex-direction:column!important;
            min-width:0;min-height:0;
            max-height:calc(100vh - 52px)!important;
            max-height:calc(100dvh - 52px)!important;
            overflow:visible!important; /* Nut X goc cua ExtraModal nam nhô ra ngoai */
        }
        .modal.pd-work-detail-modal .modal-header,
        .modal.pd-work-detail-modal .modal-footer {flex:0 0 auto;}
        .modal.pd-work-detail-modal .modal-body {
            flex:1 1 auto;min-width:0;min-height:0;
            overflow-y:auto!important;overflow-x:hidden!important;
            overscroll-behavior:contain;
            scrollbar-width:thin;scrollbar-color:#d6c1ed transparent;
        }
        /* Khong cuon doc long trong bang; cuon ngang neu man hinh qua hep. */
        .modal.pd-work-detail-modal .pd-detail-modal-wrap,
        .modal.pd-work-detail-modal #pdPersonalDetailModalBody {min-width:0;}
        .modal.pd-work-detail-modal .pd-detail-list {max-height:none!important;overflow:visible!important;}
        .modal.pd-work-detail-modal .pd-ds-table-scroller {
            max-width:100%;max-height:none!important;
            overflow-x:auto!important;overflow-y:hidden!important;
            overscroll-behavior-x:contain;
        }
        .modal.pd-work-detail-modal .pd-ds-table {min-width:990px;}
        body:has(.modal.pd-work-detail-modal.show) {overflow:hidden!important;}
        @media(max-width:700px) {
            .modal.pd-work-detail-modal .modal-dialog {
                width:calc(100vw - 20px)!important;max-width:calc(100vw - 20px)!important;
                margin:24px auto!important;
                max-height:calc(100dvh - 48px)!important;
            }
            .modal.pd-work-detail-modal .modal-content {max-height:calc(100dvh - 48px)!important;}
        }
        /* V11 - Bang cong gon: click tong gio nhay den bang, hover ma/ten mo preview */
        .pd-personal-ds-breakdown {display:none!important}
        .pd-ds-overview {display:flex;align-items:center;flex-wrap:wrap;gap:8px 15px;margin:5px 0 13px;color:#4d237b;font-size:14px}
        .pd-ds-overview>strong {font-size:14px;font-weight:800}
        .pd-ds-equation {display:inline-flex;align-items:center;flex-wrap:wrap;gap:6px;font-size:12.5px}
        .pd-ds-jump {appearance:none;display:inline-flex;align-items:center;max-width:100%;padding:5px 9px;border:1px solid #e2d1f5;border-radius:7px;background:#faf7ff;color:#602797;font-weight:750;cursor:pointer;transition:background .15s,border-color .15s}
        .pd-ds-jump:hover,.pd-ds-jump:focus-visible {background:#ede1fb;border-color:#bb98e7;outline:none}
        .pd-ds-equation-sign {color:#9683a8;font-weight:800}
        .pd-ds-equation-total {color:#52218d;font-size:13px;font-weight:850;white-space:nowrap}
        .pd-ds-section-title {scroll-margin-top:88px}
        .pd-ds-hover-trigger {display:inline;cursor:help;border-radius:4px;border-bottom:1px dashed transparent;transition:border-color .15s,color .15s}
        .pd-ds-hover-trigger:hover,.pd-ds-hover-trigger:focus-visible {color:#672d9e;border-bottom-color:#af8ad5;outline:none}
        .pd-ds-hover-template[hidden] {display:none!important}
        .pd-ds-floating-preview {position:fixed;z-index:2147483000;box-sizing:border-box;width:min(465px,calc(100vw - 24px));padding:0;pointer-events:none;filter:drop-shadow(0 10px 22px rgba(52,27,87,.16))}
        .pd-ds-floating-preview .pd-detail-item {margin:0;border:1px solid #dac7f0;border-radius:12px;background:#fff;box-shadow:none}
        .pd-ds-floating-preview .pd-personal-progress {margin-top:10px}
        .modal.pd-work-detail-modal .modal-body {padding:0!important}
        .modal.pd-work-detail-modal .pd-detail-modal-wrap {padding:12px 14px 18px}
        .modal.pd-work-detail-modal .pd-detail-modal-heading {position:sticky;top:0;z-index:5;padding:12px 14px 10px;margin:-12px -14px 12px;background:#fff;border-bottom:1px solid #eee6f5}
        .modal.pd-work-detail-modal .pd-detail-modal-heading .pd-detail-modal-sub {margin-bottom:4px}
        .modal.pd-work-detail-modal .modal-header {overflow:visible!important}
        .modal.pd-work-detail-modal .modal-dialog {overflow:visible!important}
        .modal.pd-work-detail-modal .modal-content {overflow:visible!important}

        #projectTeamHoursView .pd-team-record-aside{min-width:120px}

        /* Team hours v2: hai cột có chiều cao bằng nhau, cuộn riêng, không kéo dài trang. */
        #projectTeamHoursView { min-width:0; }
        #projectTeamHoursView .pd-team-layout {
            height:var(--pd-team-available-height, 520px);
            min-height:0;max-height:none;overflow:hidden;align-items:stretch;
            box-sizing:border-box;
        }
        #projectTeamHoursView .pd-team-side,
        #projectTeamHoursView .pd-team-detail { height:100%;min-height:0;max-height:100%; }
        /* Không để card cha kéo cao toàn trang ngoài chiều cao đo theo viewport. */
        #projectTeamHoursView { min-height:0; }
        #pdDetailPageCard:has(#projectTeamHoursView:not([hidden])) { min-height:0!important; }
        #projectTeamHoursView .pd-team-side { display:flex;flex-direction:column; }
        #projectTeamHoursView .pd-team-people {
            flex:1 1 auto;min-height:0;max-height:none;overflow-y:auto;
            overscroll-behavior:contain;
        }
        #projectTeamHoursView .pd-team-detail { display:flex;flex-direction:column; }
        #projectTeamHoursView .pd-team-detail-body {
            flex:1 1 auto;min-height:0;max-height:none;overflow-y:auto;
            overscroll-behavior:contain;
        }
        /* Tăng 1 nấc chữ tại cả hai vùng. */
        #projectTeamHoursView .pd-team-side-head strong,
        #projectTeamHoursView .pd-team-detail-head strong { font-size:14.5px; }
        #projectTeamHoursView .pd-team-side-head small,
        #projectTeamHoursView .pd-team-detail-head small { font-size:11.5px; }
        #projectTeamHoursView .pd-team-search input { font-size:13px; }
        #projectTeamHoursView .pd-team-person-name { font-size:13.5px; }
        #projectTeamHoursView .pd-team-person-email { font-size:11.5px; }
        #projectTeamHoursView .pd-team-pm { font-size:11px; }
        #projectTeamHoursView .pd-team-detail-name { font-size:14.5px; }
        #projectTeamHoursView .pd-team-detail-email { font-size:12px; }
        #projectTeamHoursView .pd-team-hours-banner,
        #projectTeamHoursView .pd-team-hours-footnote { font-size:12px; }
        #projectTeamHoursView .pd-team-accordion-title { font-size:13.5px; }
        #projectTeamHoursView .pd-team-accordion-count {
            font-size:inherit;font-weight:750;color:#79658e;
        }
        #projectTeamHoursView .pd-team-hours-value { font-size:14px; }
        #projectTeamHoursView .pd-team-record-title { font-size:13px; }
        #projectTeamHoursView .pd-team-record-meta { font-size:11.5px; }
        #projectTeamHoursView .pd-team-record-code { font-size:11px; }
        #projectTeamHoursView .pd-team-record-state { font-size:11px;max-width:138px; }
        #projectTeamHoursView .pd-team-show-all,
        #projectTeamHoursView .pd-team-record-empty { font-size:12px; }
        /* Giờ nằm TRÊN, trạng thái nằm DƯỚI (cùng mép phải) như khung mẫu.
           Không dùng width của hai cột cạnh nhau, tiết kiệm ngang cho tên công việc. */
        #projectTeamHoursView .pd-team-record { align-items:center;gap:12px; }
        #projectTeamHoursView .pd-team-record-aside {
            flex:0 0 138px;display:flex;flex-direction:column;
            align-items:flex-end;justify-content:center;
            gap:5px;min-width:0;margin-left:auto;padding:0 5px 0 0;
            text-align:right;
        }
        #projectTeamHoursView .pd-team-record-hours {
            order:0;display:block;flex:none;white-space:nowrap;
            color:#6733a0;font-size:12.5px;font-weight:400;line-height:1.25;
            font-variant-numeric:tabular-nums;
        }
        #projectTeamHoursView .pd-team-record-state {
            order:1;display:inline-flex;align-items:center;justify-content:center;
            max-width:138px;padding:3px 8px;border:1px solid transparent;
            border-radius:6px;white-space:nowrap;
            font-size:11px;font-weight:650;line-height:1.25;
        }
        /* Màu nhất quán theo loại trạng thái, không để tất cả cùng tím. */
        #projectTeamHoursView .pd-team-state-done {
            color:#18734d;background:#e6f7ed;border-color:#bfe8cf;
        }
        #projectTeamHoursView .pd-team-state-active {
            color:#186baf;background:#e7f3ff;border-color:#c1ddfa;
        }
        #projectTeamHoursView .pd-team-state-live {
            color:#11764e;background:#dff7e9;border-color:#ade5c8;
        }
        #projectTeamHoursView .pd-team-state-upcoming {
            color:#6d36aa;background:#f1e9fd;border-color:#deccf5;
        }
        #projectTeamHoursView .pd-team-state-pending {
            color:#a76912;background:#fff4df;border-color:#f6dfb2;
        }
        #projectTeamHoursView .pd-team-state-ended {
            color:#677487;background:#f1f3f7;border-color:#e0e5ec;
        }
        #projectTeamHoursView .pd-team-state-neutral {
            color:#667085;background:#f5f6f9;border-color:#e7e9ef;
        }
        /* Xac nhan tham gia hop: xanh la neu co, hong do neu khong.
           Khong nham voi trang thai "Da ket thuc" (vi tat ca cuoc hop deu da ket thuc). */
        #projectTeamHoursView .pd-team-state-joined {
            color:#16764c;background:#e7f8ee;border-color:#b9e8cc;
        }
        #projectTeamHoursView .pd-team-state-not-joined {
            color:#aa4858;background:#fff0f2;border-color:#f3cad1;
        }
        @media(max-width:680px) {
            #projectTeamHoursView .pd-team-layout { height:auto;min-height:0;max-height:none;overflow:visible; }
            #projectTeamHoursView .pd-team-side { height:230px;max-height:230px; }
            #projectTeamHoursView .pd-team-detail { height:var(--pd-team-mobile-detail-height, 420px);max-height:none; }
            #projectTeamHoursView .pd-team-people { max-height:none; }
            #projectTeamHoursView .pd-team-detail-body { max-height:none; }
            #projectTeamHoursView .pd-team-record-aside { flex:0 0 118px;min-width:0;gap:5px;padding-right:0; }
            #projectTeamHoursView .pd-team-record-hours { flex:none; }
        }
        /* Trạng thái dự án theo enum, thống nhất ở tiêu đề và vùng thông tin. */
        .pd-project-status-badge {
            display:inline-flex;align-items:center;justify-content:center;
            flex-shrink:0;min-height:28px;padding:5px 13px;
            border:1px solid transparent;border-radius:999px;
            font-size:11.5px;font-weight:750;line-height:1.2;white-space:nowrap;
        }
        .pd-status-not-started { background:#fff7e7;border-color:#f5dbad;color:#a76811; }
        .pd-status-in-progress { background:#eaf3ff;border-color:#bcd7ff;color:#2368bb; }
        .pd-status-paused { background:#fff3e0;border-color:#f2d3a1;color:#ac6813; }
        .pd-status-completed { background:#e7f8ef;border-color:#b7e7cd;color:#137649; }
        .pd-status-ended { background:#f1f2f5;border-color:#d5d8de;color:#505768; }
        @media(prefers-reduced-motion:reduce) { #projectTeamHoursView * { scroll-behavior:auto!important; } }

        /* Giữ trạng thái tab ngay cả trong khoảnh khắc UpdatePanel thay HTML.
           Màu tab/khung view dựa trên trạng thái ở <html>, không dựa vào class mặc định từ server. */
        html[data-pd-project-detail-view] #projectDetailViewSwitcher .pd-view-button {
            background:transparent !important;
            color:#6e6180 !important;
        }
        html[data-pd-project-detail-view] #projectDetailViewSwitcher .pd-view-button:hover {
            background:#eee7f8 !important;
            color:#52218d !important;
        }
        html[data-pd-project-detail-view="team"] #projectDetailViewSwitcher [data-project-view="team"],
        html[data-pd-project-detail-view="mine"] #projectDetailViewSwitcher [data-project-view="mine"],
        html[data-pd-project-detail-view="project"] #projectDetailViewSwitcher [data-project-view="project"] {
            background:#5b2499 !important;
            color:#fff !important;
        }
        html[data-pd-project-detail-view="team"] #projectDetailViewSwitcher [data-project-view="team"]:hover,
        html[data-pd-project-detail-view="mine"] #projectDetailViewSwitcher [data-project-view="mine"]:hover,
        html[data-pd-project-detail-view="project"] #projectDetailViewSwitcher [data-project-view="project"]:hover {
            background:#5b2499 !important;
            color:#fff !important;
        }
        html[data-pd-project-detail-view] #projectInfoView,
        html[data-pd-project-detail-view] #projectPersonalView,
        html[data-pd-project-detail-view] #projectTeamHoursView { display:none !important; }
        html[data-pd-project-detail-view="team"] #projectTeamHoursView,
        html[data-pd-project-detail-view="mine"] #projectPersonalView,
        html[data-pd-project-detail-view="project"] #projectInfoView { display:block !important; }

        /* CtrlMeet-compatible 4-stage meeting status, including upcoming 15-minute window. */
        .pd-meet-state { border:1px solid transparent;white-space:nowrap; }
        .pd-meet-state-0 { color:#6431a0!important;background:#f2ecfa!important;border-color:#d6c2ed!important; }
        .pd-meet-state-1 { color:#6431a0!important;background:#f2ecfa!important;border-color:#d6c2ed!important;animation:pdMeetSoonPulse 2s ease-in-out infinite; }
        .pd-meet-state-2 { color:#15803d!important;background:#dcfce7!important;border-color:#bbf7d0!important;animation:pdMeetLivePulse 1.6s ease-in-out infinite; }
        .pd-meet-state-3 { color:#64748b!important;background:#f1f5f9!important;border-color:#cbd5e1!important; }
        @keyframes pdMeetSoonPulse { 50% { box-shadow:0 0 0 3px rgba(100,49,160,.1); } }
        @keyframes pdMeetLivePulse { 50% { box-shadow:0 0 0 3px rgba(22,163,74,.14); } }
        /* Thanh tiến độ theo trạng thái từ CtrlTask, không còn mặc định màu tím cho mọi task. */
        .pd-personal-progress { position:relative;height:7px;overflow:hidden;background:#edf0f6;border-radius:100px;margin-top:9px;cursor:help; }
        .pd-personal-progress > span { display:block;height:100%;border-radius:inherit;background:#94a3b8;transition:width .35s ease; }
        .pd-task-bar-0 > span { background:#94a3b8; }
        .pd-task-bar-1 > span { background:#0ea5e9; }
        .pd-task-bar-2 > span { background:#22c55e; }
        .pd-task-bar-3 > span { background:#ef4444; }
        .pd-task-bar-4 > span { background:#f59e0b; }
        .pd-task-bar-soon > span { background:#f59e0b; }
        .pd-task-bar-late > span { background:#ef4444; }
        .pd-personal-tag.pd-task-state-0 { color:#475569;background:#f1f5f9;border:1px solid #cbd5e1; }
        .pd-personal-tag.pd-task-state-1 { color:#0369a1;background:#e0f2fe;border:1px solid #bae6fd; }
        .pd-personal-tag.pd-task-state-2,.pd-personal-tag.pd-task-state-3 { color:#15803d;background:#dcfce7;border:1px solid #bbf7d0; }
        .pd-personal-tag.pd-task-state-4 { color:#92400e;background:#fef3c7;border:1px solid #fcd34d; }
        .pd-personal-tag.pd-task-severity-soon { color:#92400e;background:#fffbeb;border-color:#fcd34d; }
        .pd-personal-tag.pd-task-severity-late { color:#b91c1c;background:#fef2f2;border-color:#fecaca; }
        .pd-task-late-note { display:inline-flex;color:#dc2626;font-size:10.5px;font-weight:700;white-space:nowrap; }
        .pd-task-percent { cursor:help;font-variant-numeric:tabular-nums; }
        /* Unified progress tooltip. HTML title is intentionally avoided to prevent native black tooltips. */
        .pd-task-percent:focus-visible,.pd-personal-progress:focus-visible { outline:2px solid #9972c2;outline-offset:3px; }
        .pd-task-progress-hint {
            --pd-hint-bg:#f1f5f9;--pd-hint-line:#cbd5e1;--pd-hint-ink:#475569;--pd-hint-accent:#94a3b8;
            position:fixed;z-index:1095;max-width:min(340px,calc(100vw - 24px));width:max-content;
            padding:10px 13px 10px 15px;box-sizing:border-box;pointer-events:none;
            border:1px solid var(--pd-hint-line);border-left:3px solid var(--pd-hint-accent);
            border-radius:10px;background:var(--pd-hint-bg);color:var(--pd-hint-ink);
            font-size:12.5px;font-weight:650;line-height:1.5;letter-spacing:0;
            text-align:left;white-space:normal;overflow-wrap:break-word;
            box-shadow:0 9px 24px rgba(35,25,60,.13);
            opacity:0;visibility:hidden;transform:translateX(6px);
            transition:opacity .17s ease,transform .17s ease,visibility .17s ease;
        }
        .pd-task-progress-hint.is-left { transform:translateX(-6px); }
        .pd-task-progress-hint.is-visible { opacity:1;visibility:visible;transform:translateX(0); }
        /* Mũi tên hướng về thanh tiến độ: ưu tiên bên phải, đảo sang trái ở sát mép màn hình. */
        .pd-task-progress-hint::after { content:'';position:absolute;left:-7px;top:var(--pd-hint-arrow-y,50%);width:12px;height:12px;
            background:var(--pd-hint-bg);border-left:2px solid var(--pd-hint-accent);border-bottom:2px solid var(--pd-hint-accent);
            transform:translateY(-50%) rotate(45deg); }
        .pd-task-progress-hint.is-left::after { left:auto;right:-7px;border:0;border-top:2px solid var(--pd-hint-accent);border-right:2px solid var(--pd-hint-accent); }
        .pd-task-progress-hint[data-pd-tone="pd-task-bar-1"] { --pd-hint-bg:#eaf7ff;--pd-hint-line:#aedffc;--pd-hint-ink:#075985;--pd-hint-accent:#0ea5e9; }
        .pd-task-progress-hint[data-pd-tone="pd-task-bar-2"] { --pd-hint-bg:#ecfdf3;--pd-hint-line:#bbf7d0;--pd-hint-ink:#166534;--pd-hint-accent:#22c55e; }
        .pd-task-progress-hint[data-pd-tone="pd-task-bar-3"],.pd-task-progress-hint[data-pd-tone="pd-task-bar-late"] { --pd-hint-bg:#fff1f2;--pd-hint-line:#fecdd3;--pd-hint-ink:#9f1239;--pd-hint-accent:#ef4444; }
        .pd-task-progress-hint[data-pd-tone="pd-task-bar-4"],.pd-task-progress-hint[data-pd-tone="pd-task-bar-soon"] { --pd-hint-bg:#fffbeb;--pd-hint-line:#fde68a;--pd-hint-ink:#92400e;--pd-hint-accent:#f59e0b; }
        @media(prefers-reduced-motion:reduce) { .pd-task-progress-hint { transition:none!important; } }
        @media(prefers-reduced-motion:reduce) { .pd-meet-state,.pd-personal-progress>span { animation:none!important;transition:none!important; } }

        /* Inline edit thông tin dự án: giữ bố cục hai cột và chiều cao gọn. */
        .pd-project-inline-fields { row-gap:14px!important; }
        .pd-project-inline-field { min-width:0; }
        .pd-project-inline-label { display:block; margin:0 0 6px; color:#38294c; font-size:13px; font-weight:750; line-height:1.3; }
        .pd-project-inline-control { width:100%; min-width:0; min-height:38px; border:1px solid #d6c8e8; border-radius:8px; background:#fff; padding:7px 11px; color:#34263e; font-size:13px; line-height:1.35; box-sizing:border-box; transition:border-color .15s,box-shadow .15s; }
        .pd-project-inline-control:focus { outline:none; border-color:#854ac5; box-shadow:0 0 0 3px rgba(111,44,171,.10); }
        .pd-project-inline-control:disabled,.pd-project-inline-control[readonly] { background:#f5f5f9; border-color:#e3dfea; color:#786e83; opacity:1; cursor:default; }
        .pd-project-inline-control ~ .select2-container { width:100%!important; }
        .pd-project-inline-fields .pd-project-inline-static { min-height:38px; display:flex; align-items:center; padding:6px 0; }
        .pd-project-inline-actions { display:flex; align-items:center; justify-content:flex-end; flex-wrap:wrap; gap:10px; margin-top:14px; }
        .pd-project-inline-note { color:#8c8098; font-size:11px; line-height:1.4; margin-right:auto; }
        .pd-project-inline-save { display:inline-flex; align-items:center; justify-content:center; gap:7px; border:1px solid #54218d; border-radius:8px; padding:8px 15px; background:#54218d; color:#fff!important; font-size:12.5px; font-weight:750; text-decoration:none!important; white-space:nowrap; }
        .pd-project-inline-save:hover { background:#421574; border-color:#421574; }
        .pd-project-inline-contract-note { display:block; margin:5px 0 0; color:#8a7f96; font-size:10.5px; }
        @media(max-width:700px) { .pd-project-inline-fields { row-gap:12px!important; } }

        /* ==========================================================
           DU AN DETAIL — DONG BO MAU THEO MEETLIST
           CHI AP DUNG CHO: Cong viec cua toi + Cong thanh vien
           Khong doi layout, JS, AJAX, quy tac tinh gio, quyen hay trang thai
           ========================================================== */
        #projectPersonalView, #projectTeamHoursView { --pd-color-primary:#54218d;--pd-color-primary-hover:#451874;--pd-color-title:#3f255e;--pd-color-text:#334155;--pd-color-muted:#64748b;--pd-color-line:#d7dfe9;--pd-color-line-soft:#e7ecf2;--pd-color-heading:#f7f9fc;--pd-color-subtle:#fafbfe;--pd-color-hover:#f8f5fd;--pd-color-selected:#f1eafd;--pd-color-selected-line:#c6a8ea;color:var(--pd-color-text); }
        /* Tab Cong viec cua toi — 4 bang: giam phu tim, tang do ro vien va hang. */
        #projectPersonalView .pd-personal-card { border:1px solid var(--pd-color-line);border-radius:11px;background:#fff;box-shadow:0 2px 8px rgba(38,50,70,.035); }
        #projectPersonalView .pd-personal-card-head { background:var(--pd-color-heading);border-bottom:1px solid var(--pd-color-line); }
        #projectPersonalView .pd-head-main strong { color:var(--pd-color-primary);font-weight:800; }
        #projectPersonalView .pd-head-main > i { color:var(--pd-color-primary); }
        #projectPersonalView .pd-head-actions small { border:1px solid #e1d4ed;border-radius:7px;background:#fff;color:#6b398e;font-weight:750; }
        #projectPersonalView .pd-compact-filter { border-color:#d9cbe7;border-radius:7px;background:#fff;color:var(--pd-color-primary); }
        #projectPersonalView .pd-compact-filter:hover { border-color:#b99cda;background:#f3ecfc; }
        #projectPersonalView .pd-compact-filter.is-filtered { background:var(--pd-color-primary);border-color:var(--pd-color-primary);color:#fff; }
        #projectPersonalView .pd-personal-list { background:#fff; }
        #projectPersonalView .pd-personal-item { background:#fff;border-bottom:1px solid var(--pd-color-line-soft);transition:background-color .16s ease; }
        #projectPersonalView .pd-personal-item:nth-of-type(even) { background:var(--pd-color-subtle); }
        #projectPersonalView .pd-personal-item:hover { background:var(--pd-color-hover); }
        #projectPersonalView .pd-personal-item-title, #projectPersonalView .pd-personal-task-name { color:var(--pd-color-title); }
        #projectPersonalView .pd-personal-task-code { border-color:#d6c0ef;border-radius:6px;background:#f5effd;color:var(--pd-color-primary); }
        #projectPersonalView .pd-personal-due, #projectPersonalView .pd-personal-meta, #projectPersonalView .pd-personal-schedule-line { color:var(--pd-color-muted); }
        #projectPersonalView .pd-personal-due i, #projectPersonalView .pd-personal-meta i, #projectPersonalView .pd-personal-schedule-line i { color:#78869a; }
        #projectPersonalView .pd-personal-hours { background:#fff;border-color:#d7c6e9;color:var(--pd-color-primary); }
        #projectPersonalView .pd-personal-card-footer { background:var(--pd-color-heading);border-top:1px solid var(--pd-color-line); }
        #projectPersonalView .pd-detail-btn { border-color:#cbb3e8;border-radius:7px;background:#fff;color:var(--pd-color-primary); }
        #projectPersonalView .pd-detail-btn:hover { border-color:#9767c9;background:#f2e9fc;color:var(--pd-color-primary-hover); }
        #projectPersonalView .pd-personal-summary { background:#fff; }
        #projectPersonalView .pd-summary-row { background:#fff;border:1px solid var(--pd-color-line);border-radius:8px;box-shadow:none; }
        #projectPersonalView .pd-summary-row strong { color:var(--pd-color-text); }
        #projectPersonalView .pd-summary-row span { color:var(--pd-color-primary); }
        #projectPersonalView .pd-summary-row.is-total { background:#f6f1fc;border-color:#c7a9e9;box-shadow:inset 3px 0 var(--pd-color-primary); }
        #projectPersonalView .pd-summary-row.is-total strong, #projectPersonalView .pd-summary-row.is-total span { color:var(--pd-color-primary); }
        #projectPersonalView .pd-personal-empty { color:#7e8998; }
        /* Trang thai task/meet/issue giu nguyen mau ngu nghia, chi dong bo vien/shape. */
        #projectPersonalView .pd-personal-tag { border-radius:7px; }
        /* Tab Cong thanh vien — dong bo 2 khung, header, nhan su, accordion, tong gio. */
        #projectTeamHoursView .pd-team-side, #projectTeamHoursView .pd-team-detail { background:#fff;border:1px solid var(--pd-color-line);border-radius:11px;box-shadow:0 2px 8px rgba(38,50,70,.035); }
        #projectTeamHoursView .pd-team-side-head, #projectTeamHoursView .pd-team-detail-head { background:var(--pd-color-heading);border-bottom:1px solid var(--pd-color-line); }
        #projectTeamHoursView .pd-team-side-head strong, #projectTeamHoursView .pd-team-detail-head strong, #projectTeamHoursView .pd-team-detail-name { color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-side-head strong i, #projectTeamHoursView .pd-team-detail-person > i { color:var(--pd-color-primary)!important; }
        #projectTeamHoursView .pd-team-side-head small, #projectTeamHoursView .pd-team-detail-head small { border:1px solid #e3d5f1;background:#fff;color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-detail-email, #projectTeamHoursView .pd-team-person-email { color:var(--pd-color-muted); }
        #projectTeamHoursView .pd-team-search input { border-color:var(--pd-color-line);background:#fff;color:var(--pd-color-text); }
        #projectTeamHoursView .pd-team-search input:focus { border-color:#9e76cd;box-shadow:0 0 0 3px rgba(84,33,141,.10); }
        #projectTeamHoursView .pd-team-search > i { color:#8290a3; }
        #projectTeamHoursView .pd-team-people { background:#fff; }
        #projectTeamHoursView .pd-team-person { border-color:transparent;border-radius:8px;background:#fff; }
        #projectTeamHoursView .pd-team-person:hover { background:var(--pd-color-hover);border-color:#e2d4ed; }
        #projectTeamHoursView .pd-team-person.is-active { background:var(--pd-color-selected);border-color:var(--pd-color-selected-line);box-shadow:inset 3px 0 var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-person-name { color:var(--pd-color-text); }
        #projectTeamHoursView .pd-team-person.is-active .pd-team-person-name { color:var(--pd-color-title); }
        #projectTeamHoursView .pd-team-pm { background:#f5effc;border:1px solid #dfccef;color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-detail-body { background:#fff; }
        #projectTeamHoursView .pd-team-hours-banner { background:#f7f9fc;border:1px solid #dde4ee;color:#66758a; }
        #projectTeamHoursView .pd-team-hours-banner i { color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-accordion { background:#fff;border:1px solid var(--pd-color-line);border-radius:9px;box-shadow:none; }
        #projectTeamHoursView .pd-team-accordion-header { background:#fff;color:var(--pd-color-text);transition:background-color .16s ease; }
        #projectTeamHoursView .pd-team-accordion-header:hover { background:var(--pd-color-hover); }
        #projectTeamHoursView .pd-team-accordion.is-expanded > .pd-team-accordion-header { background:#f7f4fc; }
        #projectTeamHoursView .pd-team-accordion-title { color:#3f3055; }
        #projectTeamHoursView .pd-team-accordion-count { color:#788394; }
        #projectTeamHoursView .pd-team-hours-value { color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-accordion-header > i { color:#78869c; }
        #projectTeamHoursView .pd-team-accordion-content { border-top:1px solid var(--pd-color-line);background:#fff; }
        #projectTeamHoursView .pd-team-accordion.is-total { background:#f6f1fc;border-color:#c7a9e9;box-shadow:inset 3px 0 var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-accordion.is-total .pd-team-accordion-header { background:transparent; }
        #projectTeamHoursView .pd-team-accordion.is-total .pd-team-accordion-title, #projectTeamHoursView .pd-team-accordion.is-total .pd-team-hours-value { color:var(--pd-color-primary);font-weight:850; }
        #projectTeamHoursView .pd-team-record { border-bottom:1px solid var(--pd-color-line-soft); }
        #projectTeamHoursView .pd-team-record:nth-child(even) { background:var(--pd-color-subtle); }
        #projectTeamHoursView .pd-team-record-title { color:var(--pd-color-text); }
        #projectTeamHoursView .pd-team-record-code { background:#f7f2ff;border-color:#dfd0f0;color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-record-meta, #projectTeamHoursView .pd-team-hours-footnote { color:var(--pd-color-muted); }
        #projectTeamHoursView .pd-team-record-hours { color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-show-all { color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-team-show-all:hover { color:var(--pd-color-primary-hover);text-decoration:underline; }
        #projectTeamHoursView .pd-ds-expand { border-color:#d0b8e8;background:#fff;color:var(--pd-color-primary); }
        #projectTeamHoursView .pd-ds-expand:hover { background:#f2eafd;border-color:#a982d6; }
        /* Bang chi tiet gio cong neu duoc mo trong panel Cong thanh vien. */
        #projectTeamHoursView .pd-ds-table-scroller { border-color:var(--pd-color-line); }
        #projectTeamHoursView .pd-ds-table th { background:var(--pd-color-heading);color:var(--pd-color-primary);border-bottom:1px solid var(--pd-color-line); }
        #projectTeamHoursView .pd-ds-table th, #projectTeamHoursView .pd-ds-table td { border-right:1px solid var(--pd-color-line-soft);border-bottom-color:var(--pd-color-line-soft); }
        #projectTeamHoursView .pd-ds-table th:last-child, #projectTeamHoursView .pd-ds-table td:last-child { border-right:0; }
        #projectTeamHoursView .pd-ds-table tbody tr:hover { background:#faf7ff; }
        /* Cai thien focus nhung khong doi event, click hay hotkey. */
        #projectPersonalView button:focus-visible, #projectTeamHoursView button:focus-visible { outline:2px solid #9368c4;outline-offset:2px; }
        @media(prefers-reduced-motion:reduce) { #projectPersonalView .pd-personal-item, #projectTeamHoursView .pd-team-accordion-header { transition:none!important; } }

        /* V18 - Can bang hai header Cong thanh vien + xoa nhan/khung luu y (UI). */
        #projectTeamHoursView .pd-team-side-head, #projectTeamHoursView .pd-team-detail-head { box-sizing:border-box;flex:0 0 72px;height:72px;min-height:72px;max-height:72px;padding:10px 14px;align-items:center; }
        #projectTeamHoursView .pd-team-side-head > strong { display:inline-flex;align-items:center;gap:7px;min-width:0;line-height:1.3; }
        #projectTeamHoursView .pd-team-detail-person { flex:1 1 auto;min-width:0;align-items:center; }
        #projectTeamHoursView .pd-team-detail-person > span { display:block;flex:1 1 auto;min-width:0!important; }
        #projectTeamHoursView .pd-team-detail-name { font-size:14px;line-height:1.3;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
        #projectTeamHoursView .pd-team-detail-email { display:block;margin-top:2px;font-size:10.75px!important;line-height:1.2;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
        #projectTeamHoursView .pd-team-top-actions { flex:0 0 auto;min-width:0;margin-left:auto; }
        #projectTeamHoursView .pd-team-detail-head .pd-team-top-actions > small, #projectPersonalView .pd-personal-time .pd-head-actions > small, #projectTeamHoursView .pd-team-hours-banner { display:none!important; }
        @media(max-width:680px) {
        #projectTeamHoursView .pd-team-side-head, #projectTeamHoursView .pd-team-detail-head { flex-basis:64px;height:64px;min-height:64px;max-height:64px;padding:9px 12px; }
        #projectTeamHoursView .pd-team-detail-name { font-size:13px; }
        #projectTeamHoursView .pd-team-detail-email { font-size:10px!important; }
        }

        /* Dong bo popup Chi tiet phan viec theo he mau bang MeetList: trang, xam sang, tim dam. */
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-modal-heading { background:#fff;border-bottom:1px solid #d7dfe9; }
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-modal-title { color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-modal-sub { color:#718096; }
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-item { border:1px solid #d7dfe9;border-radius:9px;background:#fff; }
        .modal:has(#pdPersonalDetailModalBody) .pd-personal-item-title { color:#334155; }
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-subrow { color:#64748b; }
        .modal:has(#pdPersonalDetailModalBody) .pd-detail-subrow i { color:#748298; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-overview { color:#334155; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-overview > strong, .modal:has(#pdPersonalDetailModalBody) .pd-ds-subtitle { color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-jump { border-color:#d7dfe9;border-radius:7px;background:#fff;color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-jump:hover { border-color:#bfa1db;background:#f6f1fc; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-equation-sign, .modal:has(#pdPersonalDetailModalBody) .pd-ds-footnote { color:#64748b; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-equation-total { color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table-scroller { border:1px solid #d7dfe9;border-radius:9px;background:#fff; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table { color:#334155; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table th { background:#f7f9fc;color:#54218d;border-bottom:1px solid #d7dfe9; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table th, .modal:has(#pdPersonalDetailModalBody) .pd-ds-table td { border-right:1px solid #e7ecf2;border-bottom-color:#e7ecf2; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table th:last-child, .modal:has(#pdPersonalDetailModalBody) .pd-ds-table td:last-child { border-right:none; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table tbody tr:nth-child(even) { background:#fbfcfe; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table tbody tr:hover { background:#f8f5fd; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table td strong { color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-status { background:#f2f4f8;color:#64748b;border:1px solid #d7dfe9; }
        .modal:has(#pdPersonalDetailModalBody) .pd-team-record-code { border-color:#d9c8ec;background:#f5effd;color:#54218d; }
        .modal:has(#pdPersonalDetailModalBody) .pd-summary-row { border-color:#d7dfe9;border-radius:8px;background:#fff; }
        .modal:has(#pdPersonalDetailModalBody) .pd-summary-row.is-total { border-color:#c7a9e9;background:#f6f1fc;box-shadow:inset 3px 0 #54218d; }
        .pd-ds-floating-preview .pd-detail-item { border:1px solid #d7dfe9;background:#fff; }

        /* Điều hướng từ TaskList: tô viền vàng theo đúng ID công việc, tự ngừng sau một nhịp. */
        @keyframes pdTargetLaborPulse { 0%,100% { background-color:#fff;box-shadow:inset 0 0 0 0 transparent; } 25%,65% { background-color:#fff8d6;box-shadow:inset 0 0 0 2px #f3bd26; } 45%,85% { background-color:#fff0ad;box-shadow:inset 0 0 0 3px #efac0b; } }
        .modal:has(#pdPersonalDetailModalBody) .pd-ds-table tr.pd-task-focus td { animation:pdTargetLaborPulse 1.75s ease-in-out 1 both; }
        @media(prefers-reduced-motion:reduce) { .modal:has(#pdPersonalDetailModalBody) .pd-ds-table tr.pd-task-focus td { animation:none;box-shadow:inset 0 0 0 2px #f3bd26;background:#fff8d6; } }
        /* Ba bang ca nhan: cung chieu cao tung dong, gio cong tren - trang thai duoi. Khong cham popup chi tiet. */
        #projectPersonalView .pd-personal-tasks .pd-personal-item, #projectPersonalView .pd-personal-meetings .pd-personal-item, #projectPersonalView .pd-personal-issues .pd-personal-item { box-sizing:border-box;height:116px;min-height:116px;max-height:116px;padding:9px 12px;overflow:hidden; }
        #projectPersonalView .pd-personal-tasks .pd-personal-task-header, #projectPersonalView .pd-personal-meetings .pd-personal-task-header, #projectPersonalView .pd-personal-meetings .pd-personal-meeting-heading, #projectPersonalView .pd-personal-issues .pd-personal-task-header, #projectPersonalView .pd-personal-issues .pd-personal-issue-heading { display:flex;align-items:flex-start;justify-content:space-between;flex-wrap:nowrap;gap:8px;min-width:0; }
        #projectPersonalView .pd-personal-tasks .pd-personal-task-title, #projectPersonalView .pd-personal-meetings .pd-personal-task-title, #projectPersonalView .pd-personal-meetings .pd-personal-item-title, #projectPersonalView .pd-personal-issues .pd-personal-task-title, #projectPersonalView .pd-personal-issues .pd-personal-item-title { flex:1 1 0;min-width:0; }
        #projectPersonalView .pd-personal-status-stack { display:flex;flex:0 0 auto;flex-direction:column;align-items:flex-end;justify-content:flex-start;gap:5px;min-width:0;margin-left:auto;white-space:nowrap; }
        #projectPersonalView .pd-personal-status-stack > .pd-personal-hours { margin:0;flex:none; }
        #projectPersonalView .pd-personal-status-stack > .pd-personal-tag { display:inline-flex;align-items:center;justify-content:center;margin:0;max-width:100%;font-size:10px;line-height:1.25; }
        #projectPersonalView .pd-personal-meetings .pd-personal-meeting-flags:empty, #projectPersonalView .pd-personal-issues .pd-personal-meta:empty { display:none; }
        #projectPersonalView .pd-personal-meetings .pd-personal-meeting-heading { margin-bottom:4px; }
        #projectPersonalView .pd-personal-meetings .pd-personal-meeting-line { margin-top:4px;line-height:1.35; }
        @media(max-width:560px) { #projectPersonalView .pd-personal-status-stack { gap:4px; } #projectPersonalView .pd-personal-status-stack > .pd-personal-tag { font-size:9.5px; } }

        /* Badge giờ công của phần việc: chỉ thêm hover / click, không thay layout các thẻ. */
        #projectPersonalView .pd-personal-hours.pd-hour-hover-button { appearance:none;cursor:pointer;font-family:inherit;line-height:1.35;text-align:center;transition:background .15s,border-color .15s; }
        #projectPersonalView .pd-personal-hours.pd-hour-hover-button:hover { border-color:#a979d6!important;background:#f5effe!important; }
        #projectPersonalView .pd-personal-hours.pd-hour-hover-button:focus-visible { outline:2px solid #874ebf;outline-offset:2px; }
        .pd-hour-hover-source[hidden] { display:none!important; }
        .pd-hour-hover-floating { position:fixed;z-index:10850;width:max-content;max-width:min(890px,calc(100vw - 24px));overflow:hidden;border:1px solid #ddc9ef;border-radius:12px;background:#fff;box-shadow:0 16px 42px rgba(37,23,67,.20);pointer-events:none;color:#334155;animation:pdHourQuickEnter .16s ease-out both; }
        .pd-hour-hover-title { padding:11px 14px;background:#f5effc;color:#54218d;font-size:12.5px;font-weight:800; }
        .pd-hour-hover-scroll { max-width:calc(100vw - 26px);overflow-x:auto; }
        .pd-hour-hover-table { border-collapse:collapse;table-layout:auto;width:100%;font-size:11px; }
        .pd-hour-hover-table th { padding:10px 12px;background:#f8f6fc;color:#54218d;border:1px solid #e6dff0;font-weight:800;white-space:nowrap; }
        .pd-hour-hover-table td { padding:11px 12px;color:#45546b;border:1px solid #e6eaf1;white-space:nowrap;text-align:center;font-weight:650; }
        .pd-hour-hover-table td:nth-last-child(2) { color:#5b2499;font-weight:800; }
        .pd-hour-hover-empty { padding:15px 18px;color:#7c8798;font-size:12px; }
        .pd-hour-hover-meeting-pending { display:flex;align-items:center;gap:8px;max-width:min(410px,calc(100vw - 40px));padding:12px 15px;color:#65418c;font-size:12px;font-weight:750;line-height:1.45;white-space:normal; }
        .pd-hour-hover-meeting-pending i { flex:none;color:#7d4bb6;font-size:13px; }
        .pd-hour-hover-note { padding:8px 13px;color:#7b8798;background:#faf9fc;font-size:10.5px; }
        @keyframes pdHourQuickEnter { from { opacity:0;transform:translateY(5px) scale(.99); } to { opacity:1;transform:translateY(0) scale(1); } }
    
        /* ViewDetail: tên công việc/cuộc họp có thể mở popup từ cả tab cá nhân và tab thành viên. */
        .pd-view-object-link { display:inline;max-width:100%;margin:0;padding:0;border:0;border-radius:3px;background:transparent;color:inherit;font:inherit;font-weight:inherit;text-align:left;line-height:inherit;cursor:pointer;overflow-wrap:anywhere; }
        .pd-view-object-link:hover { color:#2563eb!important;text-decoration:none!important;border-bottom-color:transparent!important; }
        .pd-view-object-link:focus-visible { outline:2px solid #6da5ef;outline-offset:2px;text-decoration:none!important; }
        /* Tên task: mã + tên cùng một kiểu chữ, không badge; vẫn cho phép bấm cả tên. */
        #projectPersonalView .pd-personal-task-code,#projectTeamHoursView .pd-team-record-code,.modal:has(#pdPersonalDetailModalBody) .pd-team-record-code,.modal:has(#pdPersonalDetailModalBody) .pd-personal-task-code { display:inline;margin:0 4px 0 0;padding:0;border:0;border-radius:0;background:transparent;box-shadow:none;color:inherit;font:inherit;line-height:inherit;vertical-align:baseline; }
        #projectPersonalView .pd-personal-task-name.pd-view-object-link:hover,#projectTeamHoursView .pd-team-record-title .pd-view-object-link:hover { color:#2563eb!important;text-decoration:none!important; }
        .pd-ds-hover-trigger.pd-view-object-link:hover,.pd-ds-hover-trigger.pd-view-object-link:focus-visible { border-bottom:0!important;text-decoration:none!important; }
</style>
    <script type="text/javascript">
        // Delegated handlers survive WebForms UpdatePanel re-rendering.
        (function () {
            if (window.__pdTaskProgressHintInstalled) return;
            window.__pdTaskProgressHintInstalled = true;
            var pending = null, target = null, hint = null;
            var delay = 0;
            var selector = '[data-pd-progress-tooltip]';

            function findTarget(node) { return node && node.closest ? node.closest(selector) : null; }
            function getHint() {
                if (hint && document.body.contains(hint)) return hint;
                hint = document.createElement('div');
                hint.id = 'pdTaskProgressHint';
                hint.className = 'pd-task-progress-hint';
                hint.setAttribute('role', 'tooltip');
                document.body.appendChild(hint);
                return hint;
            }
            function stop() {
                if (pending !== null) { clearTimeout(pending); pending = null; }
                if (target && target.getAttribute('aria-describedby') === 'pdTaskProgressHint') target.removeAttribute('aria-describedby');
                target = null;
                if (hint) hint.classList.remove('is-visible');
            }
            function place(el) {
                var anchor = el.getBoundingClientRect(), popup = hint.getBoundingClientRect();
                var vw = document.documentElement.clientWidth, vh = window.innerHeight;
                var gap = 14, edge = 10;
                var rightX = anchor.right + gap;
                var leftX = anchor.left - popup.width - gap;
                // Hiện bên phải; nếu chạm cạnh màn hình thì tự đảo sang bên trái.
                var rightFits = rightX + popup.width <= vw - edge;
                var leftFits = leftX >= edge;
                var useLeft = !rightFits && (leftFits || anchor.left > vw - anchor.right);
                var x = useLeft ? leftX : rightX;
                x = Math.max(edge, Math.min(vw - popup.width - edge, x));
                var centerY = anchor.top + anchor.height / 2;
                var y = Math.max(edge, Math.min(vh - popup.height - edge, centerY - popup.height / 2));
                hint.classList.toggle('is-left', useLeft);
                hint.style.setProperty('--pd-hint-arrow-y', Math.max(16, Math.min(popup.height - 16, centerY - y)) + 'px');
                hint.style.left = x + 'px';
                hint.style.top = y + 'px';
            }
            function show(el) {
                if (target !== el || !document.body.contains(el)) return;
                var message = el.getAttribute('data-pd-progress-tooltip');
                if (!message) return;
                var box = getHint();
                box.textContent = message;
                var tone = el.getAttribute('data-pd-progress-tone') || 'pd-task-bar-0';
                if (!/^(pd-task-bar-(?:[0-4]|late|soon))$/.test(tone)) tone = 'pd-task-bar-0';
                box.setAttribute('data-pd-tone', tone);
                box.classList.remove('is-visible');
                box.style.top = '-9999px';
                box.style.left = '0px';
                // Temporarily make the tooltip measurable but visually hidden.
                box.style.visibility = 'hidden';
                box.classList.add('is-visible');
                place(el);
                box.style.visibility = '';
                el.setAttribute('aria-describedby', box.id);
            }
            function prepare(el) {
                if (!el || target === el) return;
                stop();
                target = el;
                if (delay === 0) show(el);
                else pending = setTimeout(function () { pending = null; show(el); }, delay);
            }
            document.addEventListener('pointerover', function (event) {
                var el = findTarget(event.target);
                if (el) prepare(el);
            });
            document.addEventListener('pointerout', function (event) {
                if (target && target.contains(event.target) && !target.contains(event.relatedTarget)) stop();
            });
            document.addEventListener('focusin', function (event) { var el = findTarget(event.target); if (el) prepare(el); });
            document.addEventListener('focusout', function (event) { if (target && target.contains(event.target)) stop(); });
            // If the list scrolls, don't leave a floating tooltip detached from its row.
            document.addEventListener('scroll', stop, true);
            window.addEventListener('resize', stop);
            document.addEventListener('pointerdown', stop);
        })();
    </script>
    <script type="text/javascript">
        (function () {
            if (window.__pdMeetingStatusHeartbeatInstalled) return;
            window.__pdMeetingStatusHeartbeatInstalled = true;
            var captions = ['Đã lên lịch', 'Sắp diễn ra', 'Đang diễn ra', 'Đã kết thúc'];
            function refreshStatuses() {
                var spans = document.querySelectorAll('[data-pd-meeting-status="1"]');
                var now = new Date();
                for (var i = 0; i < spans.length; i++) {
                    var badge = spans[i], from = badge.getAttribute('data-pd-meet-start'), to = badge.getAttribute('data-pd-meet-end');
                    var start = from ? new Date(from) : null, end = to ? new Date(to) : null;
                    var status = end && now >= end ? 3 : !start || isNaN(start.getTime()) ? 0
                        : now >= start ? 2 : start.getTime() <= now.getTime() + 15 * 60000 ? 1 : 0;
                    for (var j = 0; j < 4; j++) badge.classList.remove('pd-meet-state-' + j);
                    badge.classList.add('pd-meet-state-' + status);
                    if (badge.textContent !== captions[status]) badge.textContent = captions[status];
                    var card = badge.closest('.pd-personal-meetings .pd-personal-item');
                    if (card) card.classList.toggle('pd-meeting-ended', status === 3);
                }
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', refreshStatuses);
            else refreshStatuses();
            setInterval(refreshStatuses, 60000);
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(refreshStatuses);
            }
        })();
    </script>


    <script type="text/javascript">
        (function () {
            // Sap xep gio cong + trang thai trong 3 bang ca nhan, khong thay doi noi dung/logic.
            if (window.__pdPersonalStatusUnderHoursInstalled) return;
            window.__pdPersonalStatusUnderHoursInstalled = true;
            function pickStatus(item, type) {
                var selectors = type === 'task'
                    ? ['.pd-personal-status-stack > .pd-personal-tag', '.pd-personal-task-heading > .pd-personal-tag', '.pd-personal-task-title .pd-personal-tag']
                    : type === 'meeting'
                        ? ['.pd-personal-status-stack > .pd-personal-tag', '[data-pd-meeting-status="1"]', '.pd-personal-task-heading > .pd-personal-tag', '.pd-personal-meeting-flags > .pd-personal-tag']
                        : ['.pd-personal-status-stack > .pd-personal-tag', '[data-pd-issue-status]', '.pd-personal-task-heading > .pd-personal-tag', '.pd-personal-meta > .pd-personal-tag', '.pd-personal-issue-flags > .pd-personal-tag'];
                for (var k = 0; k < selectors.length; k++) {
                    var candidates = item.querySelectorAll(selectors[k]);
                    for (var j = 0; j < candidates.length; j++) {
                        var badge = candidates[j];
                        var label = (badge.textContent || '').trim().toLowerCase();
                        // Gia tri 'Online/Truc tiep' la hinh thuc hop, khong phai trang thai.
                        if (type === 'meeting' && (label === 'online' || label === 'trực tiếp' || label === 'offline')) continue;
                        // Quá hạn là cảnh báo tiến độ, không phải trạng thái công việc.
                        if (type === 'task' && (label === 'quá hạn' || label === 'sắp hạn')) continue;
                        return badge;
                    }
                }
                return null;
            }
            function arrangePersonalStatus() {
                var view = document.getElementById('projectPersonalView');
                if (!view) return;
                var items = view.querySelectorAll('.pd-personal-tasks .pd-personal-item, .pd-personal-meetings .pd-personal-item, .pd-personal-issues .pd-personal-item');
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    var type = item.closest('.pd-personal-tasks') ? 'task'
                        : item.closest('.pd-personal-meetings') ? 'meeting' : 'issue';
                    var hours = item.querySelector('.pd-personal-hours');
                    var status = pickStatus(item, type);
                    if (!hours || !status) continue; // Khong tu tao gio cong gia khi du lieu chua co.
                    var heading = item.querySelector('.pd-personal-task-header, .pd-personal-meeting-heading, .pd-personal-issue-heading');
                    if (!heading) {
                        // Markup van de co the chi co ten va dong metadata; tao hang title/aside.
                        var title = item.querySelector('.pd-personal-item-title');
                        if (!title || !title.parentNode) continue;
                        heading = document.createElement('div');
                        heading.className = 'pd-personal-issue-heading';
                        title.parentNode.insertBefore(heading, title);
                        heading.appendChild(title);
                    }
                    var stack = heading.querySelector('.pd-personal-status-stack');
                    if (!stack) {
                        stack = document.createElement('div');
                        stack.className = 'pd-personal-status-stack';
                        heading.appendChild(stack);
                    }
                    if (hours.parentNode !== stack) stack.appendChild(hours);
                    if (status.parentNode !== stack) stack.appendChild(status);
                }
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', arrangePersonalStatus);
            else arrangePersonalStatus();
            if (window.Sys && Sys.Application) Sys.Application.add_load(arrangePersonalStatus);
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(arrangePersonalStatus);
            }
        })();
    </script>

    <script type="text/javascript">
        (function () {
            // Lưu theo dự án và duy trì qua những lần đóng/mở trình duyệt.
            var params = new URLSearchParams(window.location.search);
            var projectId = params.get('ProjectId') || window.location.search;
            var key = 'SweetSoftQLDA.ProjectDetail.LastView.v2:' + window.location.pathname + ':' + projectId;
            window.QLDAProjectDetailViewStorageKey = key;
            var chosen = 'mine'; // Lần truy cập đầu tiên.
            try {
                var saved = window.localStorage.getItem(key);
                if (saved === 'mine' || saved === 'team' || saved === 'project') chosen = saved;
            } catch (ignored) { }
            document.documentElement.setAttribute('data-pd-project-detail-view', chosen);
        })();
    </script>
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
                                        <asp:Label runat="server" ID="lblTrangThaiHead" CssClass="pd-project-status-badge"></asp:Label>
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
                                <asp:PlaceHolder runat="server" ID="phTeamHoursTab"><button type="button" id="btnProjectTeamHours" class="pd-view-button" data-project-view="team" role="tab" aria-selected="false" aria-controls="projectTeamHoursView" title="Quản lý giờ công các thành viên trong dự án - chỉ PM và Admin"><i class="far fa-clock"></i><span>Công của thành viên</span></button></asp:PlaceHolder>
                                <button type="button" id="btnProjectMyView" class="pd-view-button is-active" data-project-view="mine" role="tab" aria-selected="true" aria-controls="projectPersonalView" title="Công việc, cuộc họp và vấn đề được giao cho bạn trong dự án này"><i class="fas fa-user-check"></i><span>Phần việc của tôi</span></button>
                                <button type="button" id="btnProjectInfoView" class="pd-view-button" data-project-view="project" role="tab" aria-selected="false" aria-controls="projectInfoView" title="Xem thông tin, tiến độ và thành viên dự án"><i class="fas fa-info-circle"></i><span>Thông tin dự án</span></button>
                            </div>
                        </div>
                        <div id="projectInfoView" class="pd-view-panel" role="tabpanel" aria-labelledby="btnProjectInfoView" hidden>
                        <div class="row g-4 align-items-start">

                            <%-- Cột trái --%>
                            <div class="col-xl-8">

                                <%-- Thông tin dự án --%>
                                <section class="mb-3">

                         
                                    <div class="row g-3 pd-project-inline-fields">
                                        <div class="col-md-6 pd-project-inline-field">
                                            <label class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></label>
                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlInlineKhachHang" SimpleInit="true" CssClass="pd-project-inline-control" PlaceHolder="Chọn khách hàng" />
                                            <asp:Label runat="server" ID="lblKhachHang" Visible="false" />
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <label class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %></label>
                                            <asp:TextBox runat="server" ID="txtInlineGiaTriHopDong" CssClass="pd-project-inline-control" MaxLength="20" placeholder="Chưa có" inputmode="decimal" />
                                            <asp:Label runat="server" ID="lblGiaTriHopDong" Visible="false" />
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <label class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %></label>
                                            <asp:TextBox runat="server" ID="txtInlineNgayKy" type="date" CssClass="pd-project-inline-control" />
                                            <asp:Label runat="server" ID="lblNgayKy" Visible="false" />
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <label class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %></label>
                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlInlineLoaiDuAn" SimpleInit="true" CssClass="pd-project-inline-control" PlaceHolder="Chọn loại dự án" />
                                            <asp:Label runat="server" ID="lblLoaiDuAn" Visible="false" />
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <label class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></label>
                                            <asp:TextBox runat="server" ID="txtInlineNgayBatDau" type="date" CssClass="pd-project-inline-control" />
                                            <asp:Label runat="server" ID="lblNgayBatDau" Visible="false" />
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <span class="pd-project-inline-label">Ngày hoàn thành dự kiến</span>
                                            <div class="pd-project-inline-static"><asp:Label runat="server" ID="lblNgayHoanThanhDuKien" /></div>
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <span class="pd-project-inline-label"><asp:Label runat="server" ID="lblNgayKetThucThucTeTitle" Text="Ngày hoàn thành thực tế" /></span>
                                            <div class="pd-project-inline-static"><asp:Label runat="server" ID="lblNgayKetThucThucTe" /></div>
                                        </div>
                                        <div class="col-md-6 pd-project-inline-field">
                                            <span class="pd-project-inline-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></span>
                                            <div class="pd-project-inline-static"><asp:Label runat="server" ID="lblTrangThai" /></div>
                                        </div>
                                    </div>
                                    <asp:Panel ID="pnlProjectInlineActions" runat="server" CssClass="pd-project-inline-actions">
                                        <span class="pd-project-inline-note">Những thông tin đã khóa được giữ nguyên.</span>
                                        <asp:LinkButton ID="btnInlineSaveProject" runat="server" CausesValidation="false" CssClass="pd-project-inline-save" OnClick="btnInlineSaveProject_Click"><i class="fas fa-save"></i>Cập nhật</asp:LinkButton>
                                    </asp:Panel>
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
                        <%-- Chỉ render cho PM/Admin: cả tab và nội dung đều được ẩn phía server. --%>
                        <asp:PlaceHolder runat="server" ID="phTeamHoursPanel">
                            <div id="projectTeamHoursView" class="pd-view-panel" role="tabpanel" aria-labelledby="btnProjectTeamHours" hidden>
                                <div class="pd-team-layout">
                                    <section class="pd-team-side" aria-label="Danh sách nhân viên dự án">
                                        <div class="pd-team-side-head"><strong><i class="fas fa-users me-2" style="color:#6b34ac"></i>Nhân sự dự án</strong><small id="pdTeamMemberCount">Thành viên</small></div>
                                        <label class="pd-team-search"><i class="fas fa-search" aria-hidden="true"></i><input type="search" id="pdTeamSearch" placeholder="Tìm theo tên hoặc email..." autocomplete="off" aria-label="Tìm nhân viên" /></label>
                                        <div class="pd-team-people" id="pdTeamPeople"><asp:Literal ID="litProjectTeamMembers" runat="server" EnableViewState="false" /></div>
                                        <div class="pd-team-no-match" id="pdTeamNoMatch" hidden>Không tìm thấy nhân viên phù hợp.</div>
                                    </section>
                                    <section class="pd-team-detail" aria-label="Bảng công nhân viên được chọn">
                                        <asp:HiddenField runat="server" ID="hfTeamHoursMember" />
                                        <asp:LinkButton runat="server" ID="lbtLoadTeamHours" OnClick="lbtLoadTeamHours_Click" CausesValidation="false" Style="display:none;" />
                                        <asp:Literal ID="litProjectTeamHours" runat="server" EnableViewState="false" />
                                    </section>
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <div id="projectPersonalView" class="pd-view-panel" role="tabpanel" aria-labelledby="btnProjectMyView">
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
    <%-- V14: Popup chi tiet ca nhan / bang cong PM duoc quan ly boi control rieng. --%>
    <SweetSoft:CtrlChiTietCong runat="server" ID="CtrlChiTietCong1" />
    <SweetSoft:CtrlChiTietCongCuocHop runat="server" ID="CtrlChiTietCongCuocHop1" />
    <%-- Mở ViewDetail qua hai control có sẵn trong MasterTemplate/CtrlTodaySchedule (không render bản sao). --%>
    <asp:HiddenField runat="server" ID="hfPdViewObjectKind" />
    <asp:HiddenField runat="server" ID="hfPdViewObjectId" />
    <asp:LinkButton runat="server" ID="btnPdViewObject" CausesValidation="false" OnClick="btnPdViewObject_Click" Style="display:none" aria-hidden="true" />

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
            // Dùng chung key với đoạn script ở <head>; localStorage nhớ qua lần truy cập sau.
            var storageKey = window.QLDAProjectDetailViewStorageKey ||
                ('SweetSoftQLDA.ProjectDetail.LastView.v2:' + window.location.pathname + ':' + (new URLSearchParams(window.location.search).get('ProjectId') || window.location.search));
            var currentView = document.documentElement.getAttribute('data-pd-project-detail-view') || 'mine';
            function getRemembered() {
                try {
                    var value = window.localStorage.getItem(storageKey);
                    if (value === 'mine' || value === 'team' || value === 'project') return value;
                } catch (ignored) { }
                return currentView;
            }
            var pdHeightRaf = 0;
            // Cùng một cách tính height cho tab Công của thành viên và Phần việc của tôi.
            // Lấy tọa độ trên viewport hiện tại, dự phòng footer + padding card + khoảng thở;
            // không lấy topInDocument (dễ cao hơn vùng nhìn khi trang đã bị scroll).
            function getDetailContentHeight(node) {
                if (!node) return 520;
                var bounds = node.getBoundingClientRect();
                var footer = document.querySelector('footer, #footer, .footer, .footer-page');
                var footerHeight = footer ? Math.min(70, Math.max(24, footer.getBoundingClientRect().height)) : 30;
                var pageCard = document.getElementById('pdDetailPageCard');
                var cardPadding = pageCard ? (parseFloat(window.getComputedStyle(pageCard).paddingBottom) || 0) : 16;
                // Dự trữ 18px ngoài footer/padding để không bật scrollbar toàn trang.
                var free = Math.floor(window.innerHeight - bounds.top - footerHeight - cardPadding - 18);
                var maxHeight = window.innerWidth > 1450 ? 780 : 550;
                return Math.max(210, Math.min(maxHeight, free));
            }
            function fitPersonalCardsToViewport() {
                var view = document.getElementById('projectPersonalView');
                var grid = view && view.querySelector('.pd-personal-grid');
                if (!grid || view.hidden) return;
                if (window.matchMedia && window.matchMedia('(max-width:780px)').matches) {
                    grid.style.removeProperty('--pd-available-card-height');
                    return;
                }
                grid.style.setProperty('--pd-available-card-height', getDetailContentHeight(grid) + 'px');
            }
            function schedulePersonalHeightFit() {
                if (pdHeightRaf) window.cancelAnimationFrame(pdHeightRaf);
                pdHeightRaf = window.requestAnimationFrame(function () {
                    pdHeightRaf = 0;
                    fitPersonalCardsToViewport();
                });
            }
            var pdTeamHeightRaf = 0;
            function fitTeamHoursToViewport() {
                var view = document.getElementById('projectTeamHoursView');
                var layout = view && view.querySelector('.pd-team-layout');
                if (!layout || view.hidden) return;
                if (window.matchMedia && window.matchMedia('(max-width:680px)').matches) {
                    layout.style.removeProperty('--pd-team-available-height');
                    var detail = layout.querySelector('.pd-team-detail');
                    if (detail) detail.style.setProperty('--pd-team-mobile-detail-height', Math.max(330, Math.floor(window.innerHeight * 0.52)) + 'px');
                    return;
                }
                layout.style.setProperty('--pd-team-available-height', getDetailContentHeight(layout) + 'px');
            }
            function scheduleTeamHoursHeightFit() {
                if (pdTeamHeightRaf) window.cancelAnimationFrame(pdTeamHeightRaf);
                pdTeamHeightRaf = window.requestAnimationFrame(function () {
                    pdTeamHeightRaf = 0;
                    fitTeamHoursToViewport();
                });
            }
            function showView(value, remember) {
                var root = document.getElementById('projectDetailViewSwitcher');
                var mine = document.getElementById('projectPersonalView');
                var project = document.getElementById('projectInfoView');
                var team = document.getElementById('projectTeamHoursView');
                if (!root || !mine || !project) return;
                // Người không phải PM/Admin không có DOM tab team; tuyệt đối không khôi phục team.
                var selected = value === 'project' ? 'project' : (value === 'team' && team && root.querySelector('[data-project-view="team"]') ? 'team' : 'mine');
                currentView = selected;
                // Cập nhật trạng thái ở ngoài UpdatePanel TRƯỚC khi đổi DOM.
                // Khi AJAX render lại, CSS vẫn giữ đúng tab trong suốt lần thay HTML.
                document.documentElement.setAttribute('data-pd-project-detail-view', selected);
                var isMine = selected === 'mine';
                mine.hidden = !isMine;
                project.hidden = selected !== 'project';
                if (team) team.hidden = selected !== 'team';
                Array.prototype.forEach.call(root.querySelectorAll('[data-project-view]'), function (button) {
                    var active = button.getAttribute('data-project-view') === selected;
                    button.classList.toggle('is-active', active);
                    button.setAttribute('aria-selected', active ? 'true' : 'false');
                    button.setAttribute('tabindex', active ? '0' : '-1');
                });
                // min-vh-100 bắt toàn bộ thẻ cao ít nhất 100vh, kể cả đã có header trên trang.
                // Chỉ bỏ giới hạn này trong view cá nhân, giữ nguyên cho Thông tin dự án.
                var pageCard = document.getElementById('pdDetailPageCard');
                if (pageCard) pageCard.classList.toggle('min-vh-100', selected === 'project');
                if (isMine) schedulePersonalHeightFit();
                if (selected === 'team') scheduleTeamHoursHeightFit();
                if (remember) { try { window.localStorage.setItem(storageKey, selected); } catch (ignored) { } }
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
            document.addEventListener('click', function (event) {
                var teamMember = event.target.closest ? event.target.closest('#projectTeamHoursView [data-pd-team-member]') : null;
                if (teamMember) {
                    event.preventDefault();
                    if (teamMember.classList.contains('is-active')) return;
                    var selectedInput = document.getElementById('<%= hfTeamHoursMember.ClientID %>');
                    var loadButton = document.getElementById('<%= lbtLoadTeamHours.ClientID %>');
                    if (selectedInput && loadButton) {
                        selectedInput.value = teamMember.getAttribute('data-pd-team-member') || '';
                        Array.prototype.forEach.call(document.querySelectorAll('#pdTeamPeople [data-pd-team-member]'), function (el) {
                            var active = el === teamMember;
                            el.classList.toggle('is-active', active);
                            el.setAttribute('aria-pressed', active ? 'true' : 'false');
                        });
                        loadButton.click();
                    }
                    return;
                }
                var accordion = event.target.closest ? event.target.closest('#projectTeamHoursView [data-pd-team-toggle]') : null;
                if (accordion) {
                    event.preventDefault();
                    var section = accordion.closest('.pd-team-accordion');
                    if (!section) return;
                    var content = section.querySelector('.pd-team-accordion-content');
                    var opening = !section.classList.contains('is-expanded');
                    section.classList.toggle('is-expanded', opening);
                    accordion.setAttribute('aria-expanded', opening ? 'true' : 'false');
                    if (content) content.hidden = !opening;
                    return;
                }
                var showAll = event.target.closest ? event.target.closest('#projectTeamHoursView [data-pd-team-show-all]') : null;
                if (showAll) {
                    event.preventDefault();
                    var section = showAll.closest('.pd-team-accordion');
                    if (!section) return;
                    var expanded = !section.classList.contains('show-all');
                    section.classList.toggle('show-all', expanded);
                    showAll.textContent = expanded ? 'Thu gọn danh sách' : (showAll.getAttribute('data-collapse-label') || 'Xem tất cả');
                    return;
                }
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
                        // Animation chi mang icon + ten bang, tranh badge/filter/nut X bi co ep khi thu.
                        var ghost = null;
                        if (movingHead && before && before.width > 0) {
                            var main = movingHead.querySelector('.pd-head-main');
                            if (main) {
                                ghost = document.createElement('div');
                                ghost.className = 'pd-pin-flying-head';
                                ghost.appendChild(main.cloneNode(true));
                            }
                        }
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
                if (detailButton) {
                    event.preventDefault();
                    var detailSource = detailButton.getAttribute('data-pd-detail-source');
                    if (detailSource === 'pdPersonalMeetingDetail' || detailSource === 'pdTeamDetail-meeting') {
                        if (window.QLDAProjectMeetingPopup) window.QLDAProjectMeetingPopup.open(detailButton);
                    } else if (window.QLDAProjectDetailPopup) window.QLDAProjectDetailPopup.open(detailButton);
                    return;
                }
            });
            document.addEventListener('input', function (event) {
                if (!event.target || event.target.id !== 'pdTeamSearch') return;
                var text = String(event.target.value || '').trim().toLocaleLowerCase('vi-VN');
                var visible = 0;
                Array.prototype.forEach.call(document.querySelectorAll('#pdTeamPeople [data-pd-team-member]'), function (person) {
                    var match = (person.getAttribute('data-pd-search') || '').indexOf(text) >= 0;
                    person.hidden = !match;
                    if (match) visible++;
                });
                var empty = document.getElementById('pdTeamNoMatch');
                if (empty) empty.hidden = visible !== 0;
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
                var total = document.querySelector('#pdTeamPeople .pd-team-actual-count');
                var count = document.getElementById('pdTeamMemberCount');
                if (total && count) count.textContent = (total.getAttribute('data-pd-total') || '0') + ' người';
                showView(getRemembered(), false);
                Object.keys(pdFilters).forEach(syncPersonalFilter);
                restorePersonalPins();
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', restore);
            else restore();
            if (window.Sys && Sys.Application) Sys.Application.add_load(restore);
            window.addEventListener('resize', function () {
                layoutPersonalPins(); schedulePersonalHeightFit(); scheduleTeamHoursHeightFit();
            });
        })();
    </script>
    <script type="text/javascript">
        /* Chỉ nhận yêu cầu mở bảng công CỦA CHÍNH tài khoản đăng nhập.
           Tất cả kiểm tra quyền và lọc @UserId vẫn nằm ở CtrlChiTietCong.ascx.cs. */
        (function () {
            var args = new URLSearchParams(window.location.search);
            if (args.get('pdLabor') !== 'task') return;
            var focusId = (args.get('pdTaskFocus') || '').toLowerCase();
            if (!/^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$/.test(focusId)) focusId = '';
            var attempts = 0;
            var focusObserver = null;
            function focusTaskRow() {
                if (!focusId) return false;
                var body = document.getElementById('pdPersonalDetailModalBody');
                if (!body) return false;
                // Chỉ chọn hàng ở bảng công, không tìm theo tên vì có thể trùng tên công việc.
                var row = body.querySelector('tr[data-pd-task-id="' + focusId + '"]');
                if (!row) return false;
                if (focusObserver) { focusObserver.disconnect(); focusObserver = null; }
                // AJAX có thể render hàng trước khi ExtraModal hiện xong; chờ popup mở rồi mới cuộn/nháy.
                window.setTimeout(function () {
                    if (!row.isConnected) return;
                    var scrollHost = row.closest('.pd-ds-table-scroller');
                    if (scrollHost && scrollHost.scrollHeight > scrollHost.clientHeight)
                        scrollHost.scrollTop += row.getBoundingClientRect().top - scrollHost.getBoundingClientRect().top - 25;
                    else {
                        var modalBody = row.closest('.modal-body');
                        if (modalBody) modalBody.scrollTop += row.getBoundingClientRect().top - modalBody.getBoundingClientRect().top - 90;
                    }
                    row.classList.remove('pd-task-focus');
                    void row.offsetWidth;
                    row.classList.add('pd-task-focus');
                    row.addEventListener('animationend', function () { row.classList.remove('pd-task-focus'); }, { once: true });
                    window.setTimeout(function () { if (row.isConnected) row.classList.remove('pd-task-focus'); }, 2300);
                }, 320);
                return true;
            }
            function awaitTaskRow() {
                if (!focusId || focusTaskRow()) return;
                // UpdatePanel có thể thay toàn bộ node pdPersonalDetailModalBody, quan sát document.body để không mất sự kiện.
                if (!document.body || !window.MutationObserver) return;
                focusObserver = new MutationObserver(focusTaskRow);
                focusObserver.observe(document.body, { childList: true, subtree: true });
                window.setTimeout(function () { if (focusObserver) { focusObserver.disconnect(); focusObserver = null; } }, 10000);
            }
            function openOwnTaskLabor() {
                var ownTab = document.getElementById('btnProjectMyView');
                if (!ownTab || !window.QLDAProjectDetailPopup || typeof window.QLDAProjectDetailPopup.open !== 'function') {
                    if (++attempts < 16) window.setTimeout(openOwnTaskLabor, 200);
                    return;
                }
                ownTab.click();
                awaitTaskRow(); // Đăng ký trước khi WebForms AJAX trả bảng về.
                var trigger = document.createElement('button');
                trigger.type = 'button';
                trigger.setAttribute('data-pd-detail-source', 'pdPersonalTaskDetail');
                trigger.setAttribute('data-pd-detail-title', 'Bảng công công việc của tôi');
                trigger.setAttribute('data-pd-detail-sub', 'Giờ công từ bản đối soát hiện tại trong dự án.');
                // Lần đầu mở qua deep-link không có click trên trang đích: bật loader ExtraModal trực tiếp.
                if (window.QLDAModalLoading) window.QLDAModalLoading.expect(trigger);
                try { window.QLDAProjectDetailPopup.open(trigger); }
                catch (error) { if (window.QLDAModalLoading) window.QLDAModalLoading.end(); throw error; }
                args.delete('pdLabor');
                args.delete('pdTaskFocus');
                var clean = window.location.pathname + (args.toString() ? '?' + args.toString() : '') + window.location.hash;
                try { window.history.replaceState(window.history.state, '', clean); } catch (ignored) { }
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', openOwnTaskLabor);
            else openOwnTaskLabor();
        })();
    </script>
    <script type="text/javascript">
        /* Chỉ chuẩn hóa văn bản GIỜ CÔNG bằng 0, không can thiệp dữ liệu hoặc tính toán. */
        (function () {
            if (window.__pdEmptyLaborLabel26) return;
            window.__pdEmptyLaborLabel26 = true;
            function normalizeLabor(root) {
                if (!root) return;
                var selectors = '#projectPersonalView .pd-personal-hours,#projectPersonalView .pd-summary-row span,' +
                    '#projectTeamHoursView .pd-personal-hours,#projectTeamHoursView .pd-team-hours-banner,' +
                    '#projectTeamHoursView .pd-summary-row span,#projectTeamHoursView .pd-team-hours-value,#projectTeamHoursView .pd-ds-table td,' +
                    '#pdPersonalDetailModalBody .pd-summary-row span,#pdPersonalDetailModalBody .pd-ds-table td';
                var nodes = document.querySelectorAll(selectors);
                var zero = /^(?:(?:Giờ công|Kết quả cuối)\s*:\s*)?0(?:[.,]0+)?\s*(?:giờ(?: công)?|phút)?$/i;
                nodes.forEach(function (node) {
                    // Chỉ đổi khi toàn bộ dữ liệu văn bản là số 0 của ô giờ công.
                    // Bảng theo cột: td đầu tiên là tên task; không đụng tên task hay trạng thái.
                    if (node.matches('td')) {
                        var table = node.closest('.pd-ds-table');
                        if (!table) return;
                        var cells = node.parentElement ? Array.from(node.parentElement.children) : [];
                        var pos = cells.indexOf(node);
                        if (pos < 1 || pos > 5) return;
                    }
                    var text = (node.textContent || '').trim();
                    if (!zero.test(text)) return;
                    var textWalker = document.createTreeWalker(node, NodeFilter.SHOW_TEXT);
                    var t, list = [];
                    while ((t = textWalker.nextNode())) list.push(t);
                    if (!list.length) return;
                    list[0].nodeValue = 'Chưa ghi nhận công';
                    list.slice(1).forEach(function (x) { x.nodeValue = ''; });
                });
            }
            var pending = false;
            function requestNormalize() {
                if (pending) return;
                pending = true;
                window.requestAnimationFrame(function () { pending = false; normalizeLabor(document); });
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', requestNormalize);
            else requestNormalize();
            if (window.Sys && Sys.Application) Sys.Application.add_load(requestNormalize);
            var root = document.querySelector('#pdPersonalDetailModalBody');
            if (root && window.MutationObserver) new MutationObserver(requestNormalize).observe(root, { childList: true, subtree: true });
        })();
    </script>

    <script type="text/javascript">
        // Hover dữ liệu riêng của object; click mở ExtraModal và đánh dấu đúng dòng sau UpdatePanel.
        (function () {
            if (window.__pdPersonalHourQuickV2) return;
            window.__pdPersonalHourQuickV2 = true;
            var tooltip = null, active = null, targetFocus = null, focusObserver = null;
            function buttonOf(target) { return target && target.closest ? target.closest('#projectPersonalView .pd-personal-hours[data-pd-labor-kind]') : null; }
            function hide() { if (tooltip && tooltip.parentNode) tooltip.parentNode.removeChild(tooltip); tooltip = null;active = null; }
            function show(button) {
                if (!button || tooltip && button === active) return;
                hide();
                var card = button.closest('.pd-personal-item'), source = card && card.querySelector('.pd-hour-hover-source');
                if (!source) return;
                var el = document.createElement('div'); el.className = 'pd-hour-hover-floating'; el.setAttribute('role','tooltip');
                // Trạng thái họp có thể đổi tại chỗ theo đồng hồ 60s, không chỉ khi server render trang.
                var meetingBadge = card && card.querySelector('[data-pd-meeting-status="1"]');
                var ongoingWithoutHours = button.getAttribute('data-pd-labor-kind') === 'meeting'
                    && meetingBadge && meetingBadge.classList.contains('pd-meet-state-2')
                    && (button.textContent || '').indexOf('Chưa ghi nhận công') !== -1;
                el.innerHTML = ongoingWithoutHours
                    ? '<div class="pd-hour-hover-meeting-pending"><i class="far fa-clock" aria-hidden="true"></i><span>Cuộc họp chưa kết thúc nên chưa ghi nhận công</span></div>'
                    : source.innerHTML;
                document.body.appendChild(el); tooltip = el; active = button;
                var rect = button.getBoundingClientRect(), width = el.offsetWidth, height = el.offsetHeight;
                var gap = 10, edge = 12, vw = window.innerWidth, vh = window.innerHeight;
                // Ưu tiên góc trên-bên-phải của badge giờ công, không căn giữa trên hàng nữa.
                var rightSpace = vw - rect.right - gap - edge, leftSpace = rect.left - gap - edge;
                var x;
                if (rightSpace >= width) x = rect.right + gap;
                else if (leftSpace >= width) x = rect.left - width - gap;
                else x = Math.max(edge, Math.min(rect.right - width, vw - width - edge));
                // Nếu đủ chỗ thì đặt hẳn phía trên; nếu ở gần đầu trang thì căn đỉnh với badge.
                var y = rect.top - height - gap;
                if (y < edge) y = rect.top - 6;
                if (y + height > vh - edge) y = vh - height - edge;
                el.style.left = Math.round(Math.max(edge, Math.min(x, vw - width - edge))) + 'px';
                el.style.top = Math.round(Math.max(edge, y)) + 'px';
            }
            function stopObserve() { if(focusObserver){focusObserver.disconnect();focusObserver=null;} }
            function flash(row) {
                window.setTimeout(function(){
                    if(!row.isConnected)return;
                    var list=row.closest('.pd-ds-table-scroller');
                    if(list && list.scrollHeight>list.clientHeight) list.scrollTop+=row.getBoundingClientRect().top-list.getBoundingClientRect().top-25;
                    else {var body=row.closest('.modal-body');if(body)body.scrollTop+=row.getBoundingClientRect().top-body.getBoundingClientRect().top-90;}
                    row.classList.remove('pd-task-focus');void row.offsetWidth;row.classList.add('pd-task-focus');
                    window.setTimeout(function(){if(row.isConnected)row.classList.remove('pd-task-focus');},1950);
                },360);
            }
            function checkFocus(){
                if(!targetFocus || targetFocus.waiting)return false;
                var body=document.getElementById('pdPersonalDetailModalBody');if(!body)return false;
                var key=targetFocus.kind==='issue'?'data-pd-issue-id':'data-pd-task-id';
                var rows=body.querySelectorAll('tr['+key+']'),matched=null;
                for(var i=0;i<rows.length;i++){if((rows[i].getAttribute(key)||'').toLowerCase()===targetFocus.id){matched=rows[i];break;}}
                if(!matched)return false;
                stopObserve();targetFocus=null;flash(matched);return true;
            }
            function waitFocus(kind,id){
                stopObserve();targetFocus={kind:kind,id:id,waiting:true};
                if(window.MutationObserver && document.body){focusObserver=new MutationObserver(checkFocus);focusObserver.observe(document.body,{childList:true,subtree:true});}
                window.setTimeout(function(){stopObserve();targetFocus=null;},12000);
            }
            function openHours(button){
                var kind=button.getAttribute('data-pd-labor-kind');
                var id=(button.getAttribute('data-pd-labor-id')||'').toLowerCase();
                if(!/^[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}$/.test(id))return;
                var source=kind==='meeting'?'pdPersonalMeetingDetail':kind==='issue'?'pdPersonalIssueDetail':'pdPersonalTaskDetail';
                var api=kind==='meeting'?window.QLDAProjectMeetingPopup:window.QLDAProjectDetailPopup;
                if(!api || typeof api.open!=='function')return;
                var btn=document.createElement('button');btn.type='button';
                btn.setAttribute('data-pd-detail-source',source);
                btn.setAttribute('data-pd-detail-title',kind==='meeting'?'Chi tiết công họp của tôi':kind==='issue'?'Chi tiết vấn đề được giao':'Bảng công công việc của tôi');
                btn.setAttribute('data-pd-detail-sub','Giờ công của bạn trong dự án hiện tại.');
                if(kind==='meeting')btn.setAttribute('data-pd-focus-meeting-id',id);else waitFocus(kind,id);
                hide();
                if(window.QLDAModalLoading && typeof window.QLDAModalLoading.expect==='function')window.QLDAModalLoading.expect(btn);
                try { api.open(btn); } catch(err){stopObserve();targetFocus=null;if(window.QLDAModalLoading && typeof window.QLDAModalLoading.end==='function')window.QLDAModalLoading.end();throw err;}
            }
            document.addEventListener('mouseover',function(e){var btn=buttonOf(e.target);if(btn)show(btn);});
            document.addEventListener('mouseout',function(e){if(active && active.contains(e.target) && (!e.relatedTarget || !active.contains(e.relatedTarget)))hide();});
            document.addEventListener('focusin',function(e){var btn=buttonOf(e.target);if(btn)show(btn);});
            document.addEventListener('focusout',function(e){if(e.target===active)hide();});
            document.addEventListener('click',function(e){var btn=buttonOf(e.target);if(!btn)return;e.preventDefault();e.stopPropagation();openHours(btn);},true);
            window.addEventListener('scroll',hide,true);window.addEventListener('resize',hide);
            if(window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function(sender,args){
                hide();if(targetFocus){if(args && args.get_error && args.get_error()){stopObserve();targetFocus=null;}else{targetFocus.waiting=false;checkFocus();}}
            });
        })();
    </script>

    <script type="text/javascript">
        // Cùng luồng ExtraModal/OpenModal(Guid) như CtrlTodaySchedule; không chuyển trang.
        (function () {
            if (window.__pdViewObjectLink20261027) return;
            window.__pdViewObjectLink20261027 = true;
            var kindId = '<%= FindProjectDetailControl(this, "hfPdViewObjectKind") != null ? FindProjectDetailControl(this, "hfPdViewObjectKind").ClientID : "" %>';
            var objectId = '<%= FindProjectDetailControl(this, "hfPdViewObjectId") != null ? FindProjectDetailControl(this, "hfPdViewObjectId").ClientID : "" %>';
            var actionId = '<%= FindProjectDetailControl(this, "btnPdViewObject") != null ? FindProjectDetailControl(this, "btnPdViewObject").ClientID : "" %>';
            var guidRe = /^[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i;
            var loading = false;
            function openLink(link) {
                if (!link || loading) return;
                var kind = link.getAttribute('data-pd-view-kind'), id = link.getAttribute('data-pd-view-id') || '';
                if (kind === 'meet' && !id && link.hasAttribute('data-pd-view-current')) { var current = document.querySelector('#pmcPopupRoot input[id$="hfMeetingId"]'); id = current ? current.value : ''; }
                if (kind !== 'task' && kind !== 'meet' || !guidRe.test(id)) return;
                var kindInput = document.getElementById(kindId), idInput = document.getElementById(objectId), action = document.getElementById(actionId);
                if (!kindInput || !idInput || !action) return;
                kindInput.value = kind; idInput.value = id; loading = true;
                if (window.QLDAModalLoading && typeof window.QLDAModalLoading.expect === 'function') window.QLDAModalLoading.expect(action);
                try { action.click(); } catch (err) { loading = false; if (window.QLDAModalLoading && typeof window.QLDAModalLoading.end === 'function') window.QLDAModalLoading.end(); throw err; }
            }
            document.addEventListener('click', function (event) {
                var link = event.target.closest && event.target.closest('[data-pd-view-kind][data-pd-view-id]');
                if (!link) return;
                event.preventDefault(); event.stopPropagation(); openLink(link);
            }, true);
            document.addEventListener('keydown', function (event) {
                if (event.key !== 'Enter' && event.key !== ' ') return;
                var link = event.target.closest && event.target.closest('span[role="link"][data-pd-view-kind][data-pd-view-id]');
                if (!link) return;
                event.preventDefault(); event.stopPropagation(); openLink(link);
            }, true);
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { loading = false; });
            }
        })();
    </script>
</asp:Content>
