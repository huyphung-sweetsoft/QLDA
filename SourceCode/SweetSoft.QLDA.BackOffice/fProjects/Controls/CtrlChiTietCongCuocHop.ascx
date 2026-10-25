<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChiTietCongCuocHop.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChiTietCongCuocHop" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<style>
.pmc-layout { display:grid;grid-template-columns:minmax(315px,34%) minmax(0,1fr);gap:14px;height:min(650px,calc(100dvh - 274px));min-height:270px;min-width:0; }
.pmc-pane { display:flex;flex-direction:column;min-height:0;min-width:0;height:100%;background:#fff;border:1px solid #e8dff1;border-radius:12px;overflow:hidden; }
.pmc-toolbar { flex:0 0 auto;padding:12px 13px;border-bottom:1px solid #eae2f3;background:#faf8fd; }
.pmc-title-row { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:10px;color:#54218d;font-size:13.5px;font-weight:800; }
.pmc-total { border:1px solid #e2d6f3;border-radius:8px;background:#fff;padding:3px 8px;color:#672c9d;font-size:11px;white-space:nowrap; }
.pmc-search-row { display:grid;grid-template-columns:minmax(0,1fr) 134px;gap:8px; }
.pmc-search { position:relative;min-width:0; }
.pmc-search i { position:absolute;left:11px;top:50%;transform:translateY(-50%);color:#8f76a5;font-size:14px;pointer-events:none; }
.pmc-search input { width:100%;height:36px;border:1px solid #ded1ed;border-radius:8px;padding:7px 8px 7px 32px;background:#fff;color:#40304f;font-size:12px;outline:none; }
.pmc-search input:focus,.pmc-filter:focus,.pmc-range input:focus { border-color:#b68bdc;box-shadow:0 0 0 2px #f0e5fc; }
.pmc-filter { width:100%;height:36px;border:1px solid #ded1ed;border-radius:8px;padding:6px 8px;background:#fff;color:#5e3883;font-size:11.5px;font-weight:700; }
.pmc-range-toggle { display:inline-flex;align-items:center;gap:6px;margin-top:9px;padding:5px 8px;background:#fff;border:1px solid #e4d6f3;border-radius:7px;color:#6f478f;font-size:11px;font-weight:700;cursor:pointer; }
.pmc-range[hidden] { display:none!important; }
.pmc-range { display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-top:8px; }
.pmc-range label { display:flex;flex-direction:column;gap:3px;font-size:10.5px;color:#89768f; }
.pmc-range input { width:100%;min-width:0;height:32px;padding:4px 5px;border:1px solid #e3d6ee;border-radius:7px;color:#57416b;font-size:11px; }
.pmc-list { flex:1 1 auto;min-height:0;overflow-y:auto;overscroll-behavior:contain;scrollbar-width:thin; }
.pmc-meeting { display:block;width:100%;padding:12px 13px;text-align:left;background:#fff;border:0;border-left:3px solid transparent;border-bottom:1px solid #eee8f4;color:#45374d;cursor:pointer;transition:background .14s,border-color .14s; }
.pmc-meeting:hover { background:#fbf8ff; }
.pmc-meeting.is-selected { background:#f4edfd;border-left-color:#61279b; }
.pmc-meeting-top { display:flex;align-items:center;justify-content:space-between;gap:8px; }
.pmc-meeting-name { font-size:12.5px;font-weight:800;min-width:0;overflow-wrap:anywhere; }
.pmc-status { flex:0 0 auto;font-size:10px;font-weight:750;padding:3px 6px;border-radius:6px;background:#edf0f6;color:#66748c;white-space:nowrap; }
.pmc-status-1 { background:#f0e8fb;color:#63318d; }
.pmc-status-2 { background:#e6f8ed;color:#137e45; }
.pmc-meeting-meta { display:flex;flex-direction:column;gap:4px;margin-top:8px;color:#8c7d98;font-size:11px; }
.pmc-meeting-meta i { width:13px;color:#8662ac; }
.pmc-page { flex:0 0 auto;display:flex;align-items:center;justify-content:center;gap:7px;padding:9px;border-top:1px solid #ebe3f2;background:#faf9fd; }
.pmc-page button { display:inline-grid;place-items:center;width:29px;height:29px;background:#fff;border:1px solid #ded1ef;border-radius:7px;color:#682d9b;font-size:12px;font-weight:800;cursor:pointer; }
.pmc-page button[disabled] { opacity:.35;cursor:default; }
.pmc-page span { font-size:11.5px;color:#7a648f;font-weight:700;white-space:nowrap; }
.pmc-right-head { flex:0 0 auto;display:flex;align-items:flex-start;justify-content:space-between;gap:12px;padding:13px 14px 12px;background:#faf7fe;border-bottom:1px solid #e9ddf2; }
.pmc-right-head h5 { margin:0;color:#4f2589;font-size:17px;font-weight:800;line-height:1.35;overflow-wrap:anywhere; }
.pmc-right-head p { margin:5px 0 0;color:#897b99;font-size:11px; }
.pmc-duration { flex-shrink:0;white-space:nowrap;padding:6px 9px;border:1px solid #e1d1f3;border-radius:8px;background:#fff;color:#623296;font-size:11px;font-weight:800; }
.pmc-roster { flex:1 1 auto;min-height:0;overflow:auto;overscroll-behavior:contain;scrollbar-width:thin; }
.pmc-table { width:100%;min-width:685px;border-collapse:separate;border-spacing:0; }
.pmc-table th { position:sticky;top:0;z-index:2;padding:11px 10px;background:#f5effa;border-bottom:1px solid #e7ddf1;color:#60348a;font-weight:800;font-size:11.5px;text-align:left;white-space:nowrap; }
.pmc-table td { padding:10px;border-bottom:1px solid #f0eaf4;color:#544365;font-size:12px;vertical-align:middle; }
.pmc-table tr:last-child td { border-bottom:none; }
.pmc-staff { display:flex;align-items:center;gap:8px;min-width:0; }
.pmc-avatar { display:grid;place-items:center;width:33px;height:33px;flex:0 0 33px;border-radius:50%;background:#7945c1;color:#fff;font-weight:800;font-size:11px; }
.pmc-staff-name { display:flex;flex-direction:column;min-width:0;gap:3px; }
.pmc-staff-name b { font-size:12px;white-space:nowrap; }
.pmc-staff-name small { color:#8890a0;font-size:10.5px;white-space:nowrap; }
.pmc-host { color:#d7a22c;margin-left:5px; }
.pmc-yes,.pmc-no { display:inline-flex;align-items:center;padding:5px 8px;border-radius:7px;font-size:10.5px;font-weight:800;white-space:nowrap; }
.pmc-yes { background:#e7f8ee;color:#147f46; }
.pmc-no { background:#f1f1f5;color:#7c7e89; }
.pmc-hour { font-weight:750;color:#5b2499;white-space:nowrap; }
.pmc-empty { padding:20px;text-align:center;color:#8d8397;font-size:12px; }
.pmc-popup-modal { overflow-y:hidden!important;padding-right:0!important; }
.pmc-popup-modal .modal-dialog { width:min(1400px,calc(100vw - 36px))!important;max-width:calc(100vw - 36px)!important;margin:24px auto!important;max-height:calc(100dvh - 48px)!important;overflow:visible!important; }
.pmc-popup-modal .modal-content { display:flex!important;flex-direction:column!important;max-height:calc(100dvh - 48px)!important;min-height:0;overflow:visible!important; }
.pmc-popup-modal .modal-header,.pmc-popup-modal .modal-footer { flex:0 0 auto;overflow:visible!important; }
.pmc-popup-modal .modal-body { flex:1 1 auto;min-height:0;overflow:hidden!important;padding:13px!important; }
.pmc-popup-modal .pd-detail-modal-wrap { min-width:0;min-height:0; }
.pmc-popup-modal .pd-detail-modal-heading { margin-bottom:10px; }
body:has(.pmc-popup-modal.show) { overflow:hidden!important; }
@media(max-width:940px) { .pmc-layout { grid-template-columns:1fr;grid-template-rows:minmax(200px,40%) minmax(0,1fr);height:calc(100dvh - 275px);gap:9px; } .pmc-right-head h5 { font-size:14px; } .pmc-search-row { grid-template-columns:minmax(0,1fr) 115px; } .pmc-duration { white-space:normal;text-align:right; } }
@media(max-width:540px) { .pmc-popup-modal .modal-dialog { width:calc(100vw - 16px)!important;max-width:calc(100vw - 16px)!important;margin:16px auto!important;max-height:calc(100dvh - 32px)!important; } .pmc-popup-modal .modal-content { max-height:calc(100dvh - 32px)!important; } .pmc-popup-modal .modal-body { padding:7px!important; } .pmc-layout { height:calc(100dvh - 265px);min-height:220px; } }

/* V18 - MeetList palette, only this meeting popup: keep all interactions, statuses and sizes. */
.pmc-pane { border-color:#d7dfe9;box-shadow:0 1px 4px rgba(38,50,70,.025); }
.pmc-toolbar,.pmc-right-head { background:#f7f9fc;border-bottom-color:#d7dfe9; }
.pmc-title-row,.pmc-right-head h5 { color:#54218d; }
.pmc-total,.pmc-duration { color:#54218d;background:#fff;border-color:#d7dfe9; }
.pmc-search input,.pmc-filter,.pmc-range input { color:#334155;border-color:#d7dfe9;background:#fff; }
.pmc-search i { color:#8190a3; }
.pmc-filter { color:#54218d; }
.pmc-range-toggle { color:#54218d;border-color:#d7dfe9;background:#fff; }
.pmc-range-toggle:hover,.pmc-page button:not([disabled]):hover { background:#f4effb;border-color:#baa0da; }
.pmc-meeting { color:#334155;border-bottom-color:#e7ecf2; }
.pmc-meeting:nth-child(even):not(.is-selected) { background:#fbfcfe; }
.pmc-meeting:hover { background:#f8f5fd; }
.pmc-meeting.is-selected { background:#f1eafd;border-left-color:#54218d; }
.pmc-meeting-name { color:#334155; }
.pmc-meeting.is-selected .pmc-meeting-name { color:#54218d; }
.pmc-meeting-meta,.pmc-right-head p { color:#64748b; }
.pmc-meeting-meta i { color:#7d8da2; }
.pmc-page { background:#f7f9fc;border-top-color:#d7dfe9; }
.pmc-page button { border-color:#d7dfe9;color:#54218d;background:#fff; }
.pmc-page span { color:#64748b; }
.pmc-table th { background:#f7f9fc;color:#54218d;border-bottom-color:#d7dfe9;border-right:1px solid #e7ecf2; }
.pmc-table th:last-child { border-right:none; }
.pmc-table td { color:#334155;border-bottom-color:#e7ecf2; }
.pmc-table tbody tr:nth-child(even) { background:#fbfcfe; }
.pmc-table tbody tr:hover { background:#f8f5fd; }
.pmc-staff-name b { color:#334155; }
.pmc-staff-name small { color:#64748b; }
.pmc-hour { color:#54218d; }
.pmc-popup-modal .pd-detail-modal-title { color:#54218d; }
.pmc-popup-modal .pd-detail-modal-sub { color:#64748b; }
/* V19 | Meeting labor popup: restrained, compact MeetList-inspired interface; scope: pmc only. */
.pmc-popup-modal #pmcPopupRoot { --pmc-ink:#334155;--pmc-purple:#54218d;--pmc-border:#d7dfe9;--pmc-muted:#64748b;--pmc-soft:#f7f9fc;box-sizing:border-box;color:var(--pmc-ink); }
.pmc-popup-modal #pmcPopupRoot *, .pmc-popup-modal #pmcPopupRoot *::before, .pmc-popup-modal #pmcPopupRoot *::after { box-sizing:border-box; }
.pmc-popup-modal .modal-body { padding:14px 16px 16px!important; }
.pmc-popup-modal #pmcPopupRoot.pd-detail-modal-wrap { padding:0;min-width:0;min-height:0; }
.pmc-popup-modal #pmcPopupRoot .pd-detail-modal-heading { margin:0 0 13px!important;padding:0 1px 12px!important;border-bottom:1px solid #e5e9f0;background:#fff; }
.pmc-popup-modal #pmcPopupRoot .pd-detail-modal-title { margin:0 0 3px;color:#54218d;font-size:18px;font-weight:800;line-height:1.35; }
.pmc-popup-modal #pmcPopupRoot .pd-detail-modal-sub { margin:0;color:#728198;font-size:12px;line-height:1.45; }
.pmc-popup-modal #pmcPopupRoot .pmc-layout { grid-template-columns:minmax(340px,34%) minmax(0,1fr);gap:14px;height:min(642px,calc(100dvh - 246px));min-height:270px; }
.pmc-popup-modal #pmcPopupRoot .pmc-pane { border:1px solid var(--pmc-border);border-radius:11px;background:#fff;box-shadow:0 2px 10px rgba(38,50,70,.035); }
.pmc-popup-modal #pmcPopupRoot .pmc-toolbar { padding:13px 14px 12px;border-bottom:1px solid var(--pmc-border);background:var(--pmc-soft); }
.pmc-popup-modal #pmcPopupRoot .pmc-title-row { display:flex;align-items:center;justify-content:space-between;gap:8px;flex-wrap:wrap;margin-bottom:11px;color:var(--pmc-purple);font-size:13px;line-height:1.3; }
.pmc-popup-modal #pmcPopupRoot .pmc-list-title { display:inline-flex;align-items:center;gap:8px;min-width:0;font-weight:800;white-space:nowrap;font-size:15px;line-height:1.2; }
.pmc-popup-modal #pmcPopupRoot .pmc-list-title i { font-size:14px; }
.pmc-popup-modal #pmcPopupRoot .pmc-toolbar-actions { display:inline-flex;align-items:center;gap:8px;margin-left:auto;flex:0 0 auto; }
.pmc-popup-modal #pmcPopupRoot .pmc-total { display:inline-flex;align-items:center;justify-content:center;padding:4px 7px;min-height:29px;font-size:11px;border:1px solid #ddcee9;border-radius:7px;background:#fff;color:var(--pmc-purple);font-weight:750; }
.pmc-popup-modal #pmcPopupRoot .pmc-range-toggle { display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:34px;margin:0;padding:6px 11px;border:1px solid #ddd0ed;border-radius:8px;background:#fff;color:var(--pmc-purple);font-size:13px;font-weight:800;white-space:nowrap;transition:background-color .15s,border-color .15s; }
.pmc-popup-modal #pmcPopupRoot .pmc-range-toggle:hover { background:#f1eafa;border-color:#b99bd7; }
.pmc-popup-modal #pmcPopupRoot .pmc-search-row { grid-template-columns:minmax(0,1fr) 120px;gap:8px; }
.pmc-popup-modal #pmcPopupRoot .pmc-search input, .pmc-popup-modal #pmcPopupRoot .pmc-filter { height:40px;border:1px solid #d6dfe9;border-radius:8px;background:#fff;color:#334155;font-size:13.5px; }
.pmc-popup-modal #pmcPopupRoot .pmc-search input::placeholder { color:#94a3b8;font-size:13.5px; }
.pmc-popup-modal #pmcPopupRoot .pmc-filter { font-size:13.5px;font-weight:700;padding:7px 10px; }
.pmc-popup-modal #pmcPopupRoot .pmc-search input:focus-visible, .pmc-popup-modal #pmcPopupRoot .pmc-filter:focus-visible, .pmc-popup-modal #pmcPopupRoot .pmc-range-toggle:focus-visible { outline:2px solid rgba(125,78,185,.4);outline-offset:1px;box-shadow:none; }
.pmc-popup-modal #pmcPopupRoot .pmc-range { margin-top:10px;gap:9px; }
.pmc-popup-modal #pmcPopupRoot .pmc-range label { font-size:11px;font-weight:650;color:#64748b; }
.pmc-popup-modal #pmcPopupRoot .pmc-range input { width:100%;height:35px;padding:5px 7px;border:1px solid #d7dfe9;border-radius:7px;background:#fff;color:#334155;font-size:11px; }
.pmc-popup-modal #pmcPopupRoot .pmc-list { min-height:0;background:#fff;overflow-x:hidden;overflow-y:auto;scrollbar-width:thin;scrollbar-color:#cbd5e1 transparent; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting { position:relative;padding:13px 14px 12px;border-bottom:1px solid #e7ecf2;border-left:3px solid transparent;background:#fff;color:#334155;transition:background .14s ease,border-color .14s ease; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting:nth-child(even):not(.is-selected) { background:#fcfdff; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting:hover { background:#f7f4fb; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting.is-selected { background:#f1ebfa!important;border-left-color:#54218d;border-bottom-color:#e4d9f0; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-top { align-items:flex-start;gap:9px; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-name { color:#334155;font-size:13px;font-weight:750;line-height:1.42; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting.is-selected .pmc-meeting-name { color:#54218d;font-weight:800; }
.pmc-popup-modal #pmcPopupRoot .pmc-status { padding:4px 7px;border:1px solid #d7dfe9;border-radius:6px;background:#f2f5f9;color:#64748b;font-size:10.5px;font-weight:700;line-height:1.2; }
.pmc-popup-modal #pmcPopupRoot .pmc-status-1 { border-color:#ddcaf2;background:#f3ecfc;color:#64309b; }
.pmc-popup-modal #pmcPopupRoot .pmc-status-2 { border-color:#bce8d0;background:#e5f8ed;color:#17764b; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta { gap:6px;margin-top:9px;color:#718198;font-size:11.5px;line-height:1.35; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta i { width:17px;margin-right:2px;color:#8494a7; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head { min-height:79px;align-items:center;padding:13px 15px;border-bottom:1px solid var(--pmc-border);background:var(--pmc-soft); }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head h5 { max-width:100%;color:var(--pmc-purple);font-size:16px;font-weight:800;line-height:1.3; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head p { margin:5px 0 0;color:#718198;font-size:11.5px;line-height:1.4; }
.pmc-popup-modal #pmcPopupRoot .pmc-duration { padding:7px 10px;border:1px solid #d8c5ef;border-radius:7px;background:#fff;color:#54218d;font-size:11px;font-weight:800;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-roster { min-width:0;min-height:0;overflow-x:auto;overflow-y:auto;scrollbar-width:thin;scrollbar-color:#cbd5e1 transparent;background:#fff; }
.pmc-popup-modal #pmcPopupRoot .pmc-table { table-layout:fixed;width:100%;min-width:645px;margin:0;border-collapse:separate;border-spacing:0; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th { position:sticky;top:0;z-index:3;height:44px;padding:10px 12px;background:#f5f7fb!important;border-bottom:1px solid #d7dfe9;border-right:1px solid #e8edf3;color:#54218d;font-size:11px;font-weight:800;letter-spacing:.1px;text-align:left;vertical-align:middle;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-table td { height:67px;max-height:82px;padding:10px 12px;border-bottom:1px solid #e9eef4;color:#334155;font-size:12px;vertical-align:middle; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th:first-child { width:44%; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th:nth-child(2) { width:22%; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th:nth-child(3) { width:17%; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th:nth-child(4) { width:17%; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th:last-child { border-right:0; }
.pmc-popup-modal #pmcPopupRoot .pmc-table tbody tr:nth-child(even) { background:#fbfcff; }
.pmc-popup-modal #pmcPopupRoot .pmc-table tbody tr:hover { background:#f6f3fb; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff { display:flex;align-items:center;gap:11px;min-width:0;max-width:100%;overflow:hidden; }
.pmc-popup-modal #pmcPopupRoot .pmc-avatar { position:relative;display:inline-flex;align-items:center;justify-content:center;flex:0 0 36px;width:36px!important;height:36px!important;min-width:36px;min-height:36px;max-width:36px;max-height:36px;overflow:hidden;border:1px solid rgba(84,33,141,.12);border-radius:50%!important;background:#7244b7;color:#fff;font-size:11px;font-weight:800;line-height:1;isolation:isolate; }
/* Quan trọng: giới hạn tuyệt đối IMG trong bảng, kể cả khi server render img TRỰC TIẾP chứ không bọc pmc-avatar. */
.pmc-popup-modal #pmcRoster img, #pmcPopupRoot #pmcRoster img { display:inline-block!important;position:static!important;width:36px!important;height:36px!important;min-width:36px!important;min-height:36px!important;max-width:36px!important;max-height:36px!important;object-fit:cover!important;object-position:center!important;border-radius:50%!important;vertical-align:middle!important;flex:0 0 36px!important;float:none!important;transform:none!important;margin:0!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-avatar img { position:absolute!important;inset:0!important;width:100%!important;height:100%!important;min-width:0!important;min-height:0!important;max-width:100%!important;max-height:100%!important;object-fit:cover!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff-name { display:flex;flex:1 1 auto;flex-direction:column;gap:3px;min-width:0;overflow:hidden; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff-name b { display:block;max-width:100%;font-size:12px;font-weight:750;color:#334155;line-height:1.38;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff-name small { display:block;max-width:100%;font-size:10.5px;line-height:1.35;color:#8592a5;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.pmc-popup-modal #pmcPopupRoot .pmc-host { font-size:12px;color:#d69c11; }
.pmc-popup-modal #pmcPopupRoot .pmc-yes, .pmc-popup-modal #pmcPopupRoot .pmc-no { display:inline-flex;align-items:center;justify-content:center;max-width:100%;min-height:25px;padding:4px 9px;border:1px solid transparent;border-radius:6px;font-size:10.5px;font-weight:700;line-height:1.2;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-yes { background:#e6f7ed;border-color:#bde7ce;color:#19764e; }
.pmc-popup-modal #pmcPopupRoot .pmc-no { background:#f2f4f7;border-color:#e4e9ef;color:#677589; }
.pmc-popup-modal #pmcPopupRoot .pmc-hour { color:#54218d;font-size:12px;font-weight:750;font-variant-numeric:tabular-nums;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-page { min-height:51px;padding:10px 12px;gap:9px;border-top:1px solid var(--pmc-border);background:#f7f9fc; }
.pmc-popup-modal #pmcPopupRoot .pmc-page button { width:29px;height:29px;border:1px solid #d7dfe9;border-radius:7px;background:#fff;color:#54218d;box-shadow:none;transition:background-color .14s,border-color .14s; }
.pmc-popup-modal #pmcPopupRoot .pmc-page button:not([disabled]):hover { background:#efe8f8;border-color:#bfa7dd; }
.pmc-popup-modal #pmcPopupRoot .pmc-page span { color:#64748b;font-size:11.5px;font-weight:750; }
.pmc-popup-modal #pmcPopupRoot .pmc-empty { padding:36px 12px;color:#8190a1;font-size:12px;line-height:1.5; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting:focus-visible, .pmc-popup-modal #pmcPopupRoot .pmc-page button:focus-visible { outline:2px solid #966bc6;outline-offset:-2px; }
@media(max-width:1120px) {
.pmc-popup-modal #pmcPopupRoot .pmc-layout { grid-template-columns:minmax(280px,35%) minmax(0,1fr);gap:10px; }
.pmc-popup-modal #pmcPopupRoot .pmc-toolbar { padding:12px 10px; }
.pmc-popup-modal #pmcPopupRoot .pmc-toolbar-actions { gap:6px; }
.pmc-popup-modal #pmcPopupRoot .pmc-range-toggle { padding:5px 8px;font-size:12px; }
}
@media(max-width:940px) {
.pmc-popup-modal #pmcPopupRoot .pmc-layout { grid-template-columns:minmax(0,1fr);grid-template-rows:minmax(190px,41%) minmax(0,1fr);height:calc(100dvh - 234px); }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head { min-height:64px; }
.pmc-popup-modal #pmcPopupRoot .pmc-toolbar { padding:10px 12px; }
}
@media(max-width:540px) {
.pmc-popup-modal .modal-body { padding:9px!important; }
.pmc-popup-modal #pmcPopupRoot .pd-detail-modal-title { font-size:16px; }
.pmc-popup-modal #pmcPopupRoot .pmc-layout { height:calc(100dvh - 216px);grid-template-rows:minmax(205px,44%) minmax(0,1fr);gap:8px; }
.pmc-popup-modal #pmcPopupRoot .pmc-title-row { gap:6px; }
.pmc-popup-modal #pmcPopupRoot .pmc-list-title { font-size:13px; }
.pmc-popup-modal #pmcPopupRoot .pmc-range-toggle { font-size:11px; }
.pmc-popup-modal #pmcPopupRoot .pmc-search-row { grid-template-columns:minmax(0,1fr) 108px; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head { gap:8px;padding:10px 11px; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head h5 { font-size:14px; }
.pmc-popup-modal #pmcPopupRoot .pmc-duration { padding:6px;font-size:9.5px;white-space:normal; }
}
@media(prefers-reduced-motion:reduce) {
.pmc-popup-modal #pmcPopupRoot .pmc-meeting, .pmc-popup-modal #pmcPopupRoot .pmc-page button { transition:none!important; }
}
/* V21 | Chi tiet cuoc hop: giu nguyen kich thuoc popup, tang do de doc va chia deu phan rong. */
.pmc-popup-modal #pmcPopupRoot .pmc-search-row { grid-template-columns:minmax(0,1fr) 158px;gap:9px; }
.pmc-popup-modal #pmcPopupRoot .pmc-filter { min-width:0;width:100%;padding:7px 25px 7px 10px;font-size:13.5px;text-overflow:clip;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-list { display:flex;flex-direction:column;overflow-x:hidden;overflow-y:auto;scrollbar-width:none; }
.pmc-popup-modal #pmcPopupRoot .pmc-list::-webkit-scrollbar { display:none;width:0;height:0; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting { display:flex;flex-direction:column;justify-content:center;flex:1 0 auto;min-height:68px;padding:9px 13px; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-top { align-items:center;gap:8px; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-name { min-width:0;font-size:13.5px;line-height:1.35; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta { display:flex;flex-direction:row;align-items:center;flex-wrap:nowrap;gap:6px;margin-top:8px;font-size:12px;line-height:1.45;white-space:nowrap; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta>span { display:inline-flex;align-items:center;gap:4px;min-width:0; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta>span:first-child::after { content:'·';margin-left:3px;color:#9c8dab;font-weight:800; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta>span:nth-child(2)>i { display:none; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta i { width:auto;min-width:12px;margin:0;font-size:12px; }
.pmc-popup-modal #pmcPopupRoot .pmc-meeting .fa-crown { display:inline-flex;align-items:center;justify-content:center;flex:0 0 auto;margin:0 2px;padding:3px;border:1px solid #f0d6a6;border-radius:6px;background:#fff7e8;color:#c8870d!important;font-size:13px;line-height:1;cursor:help; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head h5 { font-size:18px; }
.pmc-popup-modal #pmcPopupRoot .pmc-right-head p { font-size:12.5px; }
.pmc-popup-modal #pmcPopupRoot .pmc-duration { font-size:12px; }
.pmc-popup-modal #pmcPopupRoot .pmc-roster { overflow-x:auto;overflow-y:auto;scrollbar-width:none; }
.pmc-popup-modal #pmcPopupRoot .pmc-roster::-webkit-scrollbar { display:none;width:0;height:0; }
.pmc-popup-modal #pmcPopupRoot .pmc-table th { height:49px;padding:12px 14px;font-size:13px;line-height:1.3; }
.pmc-popup-modal #pmcPopupRoot .pmc-table td { height:auto;max-height:none;padding:12px 14px;font-size:13.5px;line-height:1.4; }
.pmc-popup-modal #pmcPopupRoot .pmc-table tbody tr { height:var(--pmc-roster-row-height,86px); }
.pmc-popup-modal #pmcPopupRoot .pmc-staff { gap:11px; }
.pmc-popup-modal #pmcPopupRoot .pmc-avatar { width:40px!important;height:40px!important;min-width:40px!important;min-height:40px!important;max-width:40px!important;max-height:40px!important;flex:0 0 40px;font-size:12px; }
.pmc-popup-modal #pmcPopupRoot #pmcRoster img { width:40px!important;height:40px!important;min-width:40px!important;min-height:40px!important;max-width:40px!important;max-height:40px!important;object-fit:cover!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-avatar img { position:absolute!important;inset:0!important;width:100%!important;height:100%!important;min-width:0!important;min-height:0!important;max-width:100%!important;max-height:100%!important;object-fit:cover!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff-name b { font-size:13.5px;line-height:1.35; }
.pmc-popup-modal #pmcPopupRoot .pmc-staff-name small { font-size:11.75px;line-height:1.35; }
.pmc-popup-modal #pmcPopupRoot .pmc-host,.pmc-popup-modal #pmcPopupRoot .pmc-staff .fa-crown { display:inline-flex;align-items:center;justify-content:center;vertical-align:middle;color:#c8870d!important;font-size:14px;cursor:help; }
.pmc-popup-modal #pmcPopupRoot .pmc-host:focus-visible,.pmc-popup-modal #pmcPopupRoot .pmc-meeting .fa-crown:focus-visible { outline:2px solid #d59a27;outline-offset:3px;border-radius:4px; }
.pmc-popup-modal #pmcPopupRoot .pmc-yes,.pmc-popup-modal #pmcPopupRoot .pmc-no { min-height:29px;padding:6px 10px;font-size:12px; }
.pmc-popup-modal #pmcPopupRoot .pmc-hour { font-size:13.5px;font-weight:800; }
.pmc-host-tooltip { position:fixed;z-index:2147483000;display:inline-flex;align-items:center;gap:8px;max-width:min(220px,calc(100vw - 20px));padding:8px 11px;border:1px solid #e9c987;border-radius:8px;background:#fff8e8;color:#95620b;box-shadow:0 9px 24px rgba(111,75,9,.16);font-size:12px;font-weight:800;line-height:1.35;white-space:nowrap;pointer-events:none; }
.pmc-host-tooltip i { color:#ce921c;font-size:12px; }
@media(max-width:1120px) { .pmc-popup-modal #pmcPopupRoot .pmc-search-row { grid-template-columns:minmax(0,1fr) 150px; } }
@media(max-width:940px) { .pmc-popup-modal #pmcPopupRoot .pmc-table th { font-size:12.5px; } .pmc-popup-modal #pmcPopupRoot .pmc-table td { font-size:13px; } }
@media(max-width:540px) { .pmc-popup-modal #pmcPopupRoot .pmc-search-row { grid-template-columns:minmax(0,1fr) 139px; } .pmc-popup-modal #pmcPopupRoot .pmc-meeting { min-height:64px;padding:7px 9px; } .pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta { font-size:11.3px;gap:4px; } .pmc-popup-modal #pmcPopupRoot .pmc-meeting-meta>span:nth-child(2) { white-space:nowrap; } }
/* V23 | Tim tam phia trai: khong doi avatar, crown, tooltip hay kich thuoc popup. */
.pmc-popup-modal #pmcPopupRoot [id$="upMeetingResults"] { display:flex;flex:1 1 auto;flex-direction:column;min-height:0;min-width:0;overflow:hidden; }
.pmc-popup-modal #pmcPopupRoot .pmc-list .pmc-meeting[hidden] { display:none!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-list.is-live-preview .pmc-meeting { flex:0 0 auto; }
.pmc-popup-modal #pmcPopupRoot .pmc-live-search-note { display:block;padding:15px 12px;color:#79658c;font-size:12px;line-height:1.5;text-align:center; }
.pmc-popup-modal #pmcPopupRoot .pmc-live-search-note[hidden] { display:none!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-search.is-live-searching::after { content:"";position:absolute;top:50%;right:12px;width:14px;height:14px;margin-top:-7px;border:2px solid #e9ddf5;border-top-color:#54218d;border-radius:50%;animation:pmcSearchSpin .75s linear infinite;pointer-events:none; }
@keyframes pmcSearchSpin { to { transform:rotate(360deg); } }
@media(prefers-reduced-motion:reduce) { .pmc-popup-modal #pmcPopupRoot .pmc-search.is-live-searching::after { animation:none; } }

/* V24 - Can giua 2 cot gio: tieu de + gia tri, khong thay doi cot ten/trang thai. */
.pmc-popup-modal #pmcPopupRoot .pmc-table th:nth-child(3), .pmc-popup-modal #pmcPopupRoot .pmc-table th:nth-child(4) { text-align:center!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-table tbody td:nth-child(3), .pmc-popup-modal #pmcPopupRoot .pmc-table tbody td:nth-child(4) { text-align:center!important; }
.pmc-popup-modal #pmcPopupRoot .pmc-table tbody td.pmc-hour { font-variant-numeric:tabular-nums; }
.pmc-popup-modal #pmcPopupRoot .pmc-range-toggle.is-active { border-color:#ae87d9;background:#f4ebff;color:#54218d; }
/* Lich noi duoc ve vao document.body: click ngay khong bi an do UpdatePanel/overflow. */
.pmc-date-pop { position:fixed;z-index:2147482900;display:flex;flex-direction:column;gap:0;box-sizing:border-box;width:400px;max-width:calc(100vw - 20px);max-height:calc(100dvh - 20px);padding:16px;border:1px solid #dec9f0;border-radius:15px;background:#fff;color:#453451;box-shadow:0 18px 46px rgba(45,19,80,.2);overflow-y:auto;overscroll-behavior:contain;font-family:inherit; }
.pmc-date-pop[hidden] { display:none!important; }
.pmc-date-pop *, .pmc-date-pop *::before, .pmc-date-pop *::after { box-sizing:border-box; }
.pmc-date-head { display:flex;align-items:center;justify-content:space-between;gap:10px;margin:0 0 15px;color:#54218d;font-size:15px;font-weight:800; }
.pmc-date-head i { margin-right:5px;font-size:14px; }
.pmc-date-close { display:grid;place-items:center;width:27px;height:27px;padding:0;border:0;border-radius:6px;background:transparent;color:#9779b1;font-size:23px;line-height:1;cursor:pointer; }
.pmc-date-close:hover { background:#f5eefb;color:#54218d; }
.pmc-date-fields { display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);gap:8px;margin-bottom:13px; }
.pmc-date-field { display:flex;flex-direction:column;align-items:flex-start;justify-content:center;gap:5px;min-height:67px;padding:10px 11px;border:1px solid #e2d6ef;border-radius:9px;background:#fcfaff;color:#563a70;text-align:left;cursor:pointer; }
.pmc-date-field.is-active { border-color:#b985e7;background:#f2eafd;box-shadow:inset 0 0 0 1px #d4b1f4; }
.pmc-date-field small { color:#8b78a0;font-size:11px;line-height:1.2; }
.pmc-date-field strong { color:#54218d;font-size:13px;font-weight:800;line-height:1.3; }
.pmc-date-month { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:9px;color:#54218d;font-size:13.5px;font-weight:800; }
.pmc-date-month button { display:grid;place-items:center;width:34px;height:34px;border:1px solid #e3d5ef;border-radius:8px;background:#fff;color:#7037a0;font-size:19px;cursor:pointer; }
.pmc-date-month button:hover { background:#f3eafd;border-color:#c5a3e5; }
.pmc-date-days { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:4px;margin:0 0 6px; }
.pmc-date-weekday { display:grid;place-items:center;min-height:28px;color:#9480a8;font-size:11px;font-weight:750; }
.pmc-date-day { display:grid;place-items:center;aspect-ratio:1/1;min-width:0;padding:0;border:1px solid transparent;border-radius:8px;background:#fff;color:#554564;font-size:12.5px;cursor:pointer; }
.pmc-date-day:hover:not(:disabled) { background:#f2e9fd;border-color:#dac5ef; }
.pmc-date-day.is-out { color:#c2b9ca; }
.pmc-date-day.is-between { background:#f3eafd;color:#5b2e8a; }
.pmc-date-day.is-picked { background:#63269c;color:#fff;font-weight:800; }
.pmc-date-day.is-picked:hover:not(:disabled) { background:#512184;color:#fff; }
.pmc-date-day:disabled { opacity:.42;cursor:not-allowed; }
.pmc-date-error { display:block;min-height:18px;margin:5px 0 8px;color:#b42341;font-size:11px;font-weight:600; }
.pmc-date-foot { display:grid;grid-template-columns:1fr 1fr;gap:9px;margin-top:7px; }
.pmc-date-foot button { min-height:39px;padding:9px 10px;border:1px solid #d7c0ec;border-radius:8px;background:#fff;color:#652f9a;font-size:13px;font-weight:800;cursor:pointer; }
.pmc-date-foot button:hover { background:#f3ebfc; }
.pmc-date-foot .pmc-date-apply { border-color:#54218d;background:#54218d;color:#fff; }
.pmc-date-foot .pmc-date-apply:hover { background:#441777;color:#fff; }
.pmc-date-pop button:focus-visible { outline:2px solid #9564c5;outline-offset:2px; }
@media(max-width:540px) { .pmc-date-pop { padding:12px;width:360px; } .pmc-date-head { font-size:14px;margin-bottom:10px; } .pmc-date-day { font-size:12px; } }
</style>
<SweetSoft:ExtraModal ID="mdlMeetingDetail" runat="server" Type="Primary" Title="Chi tiết cuộc họp" Width="92%">
    <ContentTemplate>
        <div class="pd-detail-modal-wrap" id="pmcPopupRoot">
            <div class="pd-detail-modal-heading"><h4 class="pd-detail-modal-title" id="pmcPopupTitle">Chi tiết cuộc họp</h4><p class="pd-detail-modal-sub" id="pmcPopupSub">Danh sách cuộc họp và giờ công của nhân viên tham gia.</p></div>
            <asp:UpdatePanel ID="upMeeting" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                <ContentTemplate>
                    <asp:HiddenField ID="hfAction" runat="server" /><asp:HiddenField ID="hfKind" runat="server" /><asp:HiddenField ID="hfMember" runat="server" />
                    <asp:HiddenField ID="hfSearch" runat="server" /><asp:HiddenField ID="hfState" runat="server" /><asp:HiddenField ID="hfDateFrom" runat="server" /><asp:HiddenField ID="hfDateTo" runat="server" />
                    <asp:HiddenField ID="hfMeetingPage" runat="server" /><asp:HiddenField ID="hfRosterPage" runat="server" /><asp:HiddenField ID="hfMeetingId" runat="server" />
                    <asp:LinkButton ID="btnMeetingAction" runat="server" OnClick="btnMeetingAction_Click" CausesValidation="false" Style="display:none" />
                    <div class="pmc-layout" id="pmcLayout">
                        <section class="pmc-pane" aria-label="Danh sách cuộc họp">
                            <div class="pmc-toolbar">
                                <div class="pmc-title-row"><span class="pmc-list-title"><i class="far fa-calendar-alt"></i> Danh sách cuộc họp</span><div class="pmc-toolbar-actions"><span class="pmc-total" style="display:none;"><asp:Literal ID="litCount" runat="server" /></span><button type="button" class="pmc-range-toggle" id="pmcRangeToggle" aria-haspopup="dialog" aria-expanded="false" title="Chọn khoảng ngày"><i class="far fa-calendar"></i><span class="pmc-range-text">Chọn ngày</span></button></div></div>
                                <div class="pmc-search-row"><div class="pmc-search"><i class="fas fa-search"></i><input type="search" id="pmcSearchInput" placeholder="Tìm tên cuộc họp..." autocomplete="off" /></div>
                                    <select class="pmc-filter" id="pmcStateFilter" aria-label="Lọc trạng thái"><option value="all">Tất cả</option><option value="active">Chưa kết thúc</option><option value="ended">Đã kết thúc</option></select></div>

                            </div>
                            <asp:UpdatePanel ID="upMeetingResults" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                <ContentTemplate>
                                    <div class="pmc-list" id="pmcList"><asp:Literal ID="litMeetings" runat="server" EnableViewState="false" /></div>
                                    <div class="pmc-page" id="pmcMeetingPager"><asp:Literal ID="litMeetingPager" runat="server" EnableViewState="false" /></div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </section>
                        <section class="pmc-pane" aria-label="Nhân viên tham gia và giờ công">
                            <asp:Literal ID="litRosterHeader" runat="server" EnableViewState="false" />
                            <div class="pmc-roster" id="pmcRoster"><asp:Literal ID="litRoster" runat="server" EnableViewState="false" /></div>
                            <div class="pmc-page" id="pmcRosterPager"><asp:Literal ID="litRosterPager" runat="server" EnableViewState="false" /></div>
                        </section>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<script type="text/javascript">
    (function () {
        var modalId ='<%= ModalClientId %>';
    var selector = { action: 'hfAction', kind: 'hfKind', member: 'hfMember', search: 'hfSearch', state: 'hfState', from: 'hfDateFrom', to: 'hfDateTo', page: 'hfMeetingPage', rosterPage: 'hfRosterPage', meeting: 'hfMeetingId' };
    var busy = false, debounceHandle = null, openRequest = false;
    var requestKind = '', liveInFlight = false, liveSent = '', liveCompleted = null, liveDraft = null, liveQueued = false, liveComposing = false;
    var pendingFilter = false, pendingSelection = null, pendingPage = null;
    function el(name) { var m = modal(); return m ? m.querySelector('input[id$="' + selector[name] + '"]') : null; }
    function modal() { var n = document.getElementById(modalId); return n && n.classList && n.classList.contains('modal') ? n : (n ? n.querySelector('.modal') : null); }
    function root() { return document.getElementById('pmcPopupRoot'); }
    function val(name, v) { var e = el(name); if (e) e.value = v == null ? '' : String(v); }
    function get(name) { var e = el(name); return e ? e.value : ''; }
    function searchInput() { var r = root(); return r ? r.querySelector('#pmcSearchInput') : null; }
    function clearLiveTimer() { if (debounceHandle !== null) { clearTimeout(debounceHandle); debounceHandle = null; } }
    function normalizeText(value) { var s = String(value == null ? '' : value).toLowerCase().trim(); if (s.normalize) s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); return s.replace(/đ/g, 'd'); }
    function liveState() { var r = root(); return r ? r.querySelector('.pmc-search') : null; }
    function busyIndicator(on) { var box = liveState(); if (box) box.classList.toggle('is-live-searching', !!on); }
    function previewNote(list) { var note = list.querySelector('.pmc-live-search-note'); if (!note) { note = document.createElement('div'); note.className = 'pmc-live-search-note'; note.setAttribute('role', 'status'); list.appendChild(note); } return note; }
    function previewFilter() {
        var r = root(), list = r && r.querySelector('#pmcList'), input = searchInput(); if (!list || !input) return;
        var text = normalizeText(input.value), rows = list.querySelectorAll('.pmc-meeting'), shown = 0;
        for (var i = 0; i < rows.length; i++) {
            var title = rows[i].querySelector('.pmc-meeting-name');
            var yes = !text || normalizeText(title ? title.textContent : '').indexOf(text) !== -1;
            rows[i].hidden = !yes; if (yes) shown++;
        }
        var pending = !!text && (liveInFlight || liveQueued || liveCompleted === null || liveCompleted !== input.value);
        var note = previewNote(list);
        // Preview tung ky tu tren cac dong hien co; khi server tra ve se bo sung ket qua tu cac trang khac.
        note.hidden = !(pending && shown === 0);
        if (!note.hidden) note.textContent = 'Đang tìm thêm cuộc họp phù hợp trong dự án...';
        list.classList.toggle('is-live-preview', pending);
        busyIndicator(liveInFlight || liveQueued);
    }
    function submit(action) {
        if (busy) return false;
        val('action', action);
        var btn = document.getElementById('<%= ActionButtonId %>'); if (!btn) return false;
            busy = true; requestKind = action;
            try { btn.click(); } catch (error) { busy = false; requestKind = ''; if (window.console) console.error(error); }
            return false;
        }
        function syncFilters() {
            var input = searchInput(), select = document.getElementById('pmcStateFilter');
            if (input) { var text = liveDraft !== null ? liveDraft : get('search'); if (input.value !== text) input.value = text; }
            if (select) select.value = get('state') || 'all';
            var toggle = document.getElementById('pmcRangeToggle'), from = get('from'), to = get('to');
            if (toggle) {
                toggle.classList.toggle('is-active', !!(from || to));
                toggle.title = from && to ? 'Từ ' + prettyDate(from) + ' đến ' + prettyDate(to) : 'Chọn khoảng ngày';
                var label = toggle.querySelector('.pmc-range-text');
                if (label) label.textContent = from && to ? prettyShort(from) + ' – ' + prettyShort(to) : 'Chọn ngày';
            }
        }
        function requestFilters() {
            clearLiveTimer(); liveQueued = false;
            if (busy) { pendingFilter = true; return false; }
            var input = searchInput(), select = document.getElementById('pmcStateFilter');
            liveDraft = input ? input.value : ''; val('search', liveDraft); val('state', select ? select.value : 'all'); val('page', '1'); val('rosterPage', '1');
            return submit('filter');
        }
        function dispatchLive() {
            clearLiveTimer(); if (liveComposing) return false;
            var input = searchInput(); if (!input) return false;
            liveDraft = input.value;
            if (busy) { liveQueued = true; previewFilter(); return false; }
            if (liveCompleted !== null && liveDraft === liveCompleted) { liveQueued = false; previewFilter(); return false; }
            val('search', liveDraft); val('page', '1');
            liveSent = liveDraft; liveInFlight = true; liveQueued = false;
            previewFilter(); return submit('search-preview');
        }
        function scheduleLive() {
            clearLiveTimer();
            debounceHandle = setTimeout(function () { debounceHandle = null; dispatchLive(); }, 260);
        }
        function onLiveInput() { var input = searchInput(); if (!input) return; liveDraft = input.value; previewFilter(); scheduleLive(); }
        // Floating date-range calendar (same interaction as CtrlMeetGioCongTab).
        // Draft selections do not change the filter until Apply or Clear is clicked.
        var datePop = null, dateMonth = null, dateFrom = '', dateTo = '', dateTarget = 'from';
        function dateISO(date) { return date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0'); }
        function parseISO(value) {
            if (!/^\d{4}-\d{2}-\d{2}$/.test(value || '')) return null;
            var a = value.split('-').map(Number), d = new Date(a[0], a[1] - 1, a[2]);
            return d.getFullYear() === a[0] && d.getMonth() === a[1] - 1 && d.getDate() === a[2] ? d : null;
        }
        function prettyDate(value) { var d = parseISO(value); return d ? String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') + '/' + d.getFullYear() : 'Chọn ngày'; }
        function prettyShort(value) { var d = parseISO(value); return d ? String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') : ''; }
        function hideDatePop() {
            if (datePop) datePop.hidden = true;
            var trigger = document.getElementById('pmcRangeToggle'); if (trigger) trigger.setAttribute('aria-expanded', 'false');
        }
        function ensureDatePop() {
            if (datePop && document.body.contains(datePop)) return datePop;
            datePop = document.createElement('div'); datePop.className = 'pmc-date-pop'; datePop.hidden = true;
            datePop.setAttribute('role', 'dialog'); datePop.setAttribute('aria-label', 'Lọc cuộc họp theo khoảng ngày');
            datePop.innerHTML = '<div class="pmc-date-head"><span><i class="far fa-calendar-alt"></i> Khoảng thời gian cuộc họp</span><button type="button" class="pmc-date-close" aria-label="Đóng lịch">×</button></div>' +
                '<div class="pmc-date-fields"><button type="button" class="pmc-date-field" data-date-field="from"><small>Từ ngày</small><strong>Chọn ngày</strong></button><button type="button" class="pmc-date-field" data-date-field="to"><small>Đến ngày</small><strong>Chọn ngày</strong></button></div>' +
                '<div class="pmc-date-month"><button type="button" data-date-month="-1" aria-label="Tháng trước">‹</button><span class="pmc-date-month-label"></span><button type="button" data-date-month="1" aria-label="Tháng sau">›</button></div>' +
                '<div class="pmc-date-days"></div><div class="pmc-date-error" aria-live="polite"></div>' +
                '<div class="pmc-date-foot"><button type="button" data-date-clear>Xóa lọc</button><button type="button" class="pmc-date-apply" data-date-apply>Áp dụng</button></div>';
            document.body.appendChild(datePop);
            datePop.addEventListener('click', function (event) {
                // Stop propagation before repainting date cells: clicking a date must never close popup.
                event.stopPropagation();
                var btn = event.target.closest('button'); if (!btn || !datePop.contains(btn)) return;
                if (btn.classList.contains('pmc-date-close')) { hideDatePop(); return; }
                if (btn.hasAttribute('data-date-field')) { dateTarget = btn.getAttribute('data-date-field'); paintDatePop(); return; }
                if (btn.hasAttribute('data-date-month')) { dateMonth.setMonth(dateMonth.getMonth() + Number(btn.getAttribute('data-date-month'))); paintDatePop(); return; }
                if (btn.hasAttribute('data-date-value')) {
                    var value = btn.getAttribute('data-date-value');
                    if (dateTarget === 'from') {
                        dateFrom = value;
                        if (dateTo && dateTo < value) dateTo = '';
                        dateTarget = 'to';
                    } else if (!dateFrom || value >= dateFrom) { dateTo = value; }
                    paintDatePop(); return;
                }
                if (btn.hasAttribute('data-date-clear')) {
                    dateFrom = ''; dateTo = ''; val('from', ''); val('to', ''); hideDatePop(); syncFilters(); requestFilters(); return;
                }
                if (btn.hasAttribute('data-date-apply')) {
                    var error = datePop.querySelector('.pmc-date-error');
                    if (!dateFrom || !dateTo) { error.textContent = 'Vui lòng chọn cả Từ ngày và Đến ngày.'; return; }
                    if (dateTo < dateFrom) { error.textContent = 'Đến ngày không được trước Từ ngày.'; return; }
                    val('from', dateFrom); val('to', dateTo); hideDatePop(); syncFilters(); requestFilters();
                }
            });
            return datePop;
        }
        function positionDatePop() {
            if (!datePop || datePop.hidden) return;
            var trigger = document.getElementById('pmcRangeToggle'); if (!trigger) { hideDatePop(); return; }
            var rect = trigger.getBoundingClientRect(), width = datePop.offsetWidth, height = datePop.offsetHeight, pad = 10, gap = 10;
            var left = rect.right + gap;
            if (left + width > window.innerWidth - pad) {
                var alt = rect.left - width - gap;
                left = alt >= pad ? alt : Math.max(pad, window.innerWidth - width - pad);
            }
            var top = Math.max(pad, Math.min(rect.top, window.innerHeight - height - pad));
            datePop.style.left = Math.round(left) + 'px'; datePop.style.top = Math.round(top) + 'px';
        }
        function paintDatePop() {
            var pop = ensureDatePop(); if (!dateMonth) return;
            pop.querySelector('[data-date-field="from"] strong').textContent = prettyDate(dateFrom);
            pop.querySelector('[data-date-field="to"] strong').textContent = prettyDate(dateTo);
            pop.querySelectorAll('.pmc-date-field').forEach(function (field) { field.classList.toggle('is-active', field.getAttribute('data-date-field') === dateTarget); });
            pop.querySelector('.pmc-date-month-label').textContent = 'Tháng ' + (dateMonth.getMonth() + 1) + '/' + dateMonth.getFullYear();
            var days = pop.querySelector('.pmc-date-days'); days.textContent = '';
            ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].forEach(function (label) { var node = document.createElement('span'); node.className = 'pmc-date-weekday'; node.textContent = label; days.appendChild(node); });
            var offset = (dateMonth.getDay() + 6) % 7;
            for (var i = 0; i < 42; i++) {
                var date = new Date(dateMonth.getFullYear(), dateMonth.getMonth(), 1 - offset + i), value = dateISO(date);
                var button = document.createElement('button'); button.type = 'button'; button.className = 'pmc-date-day'; button.textContent = String(date.getDate()); button.setAttribute('data-date-value', value);
                if (date.getMonth() !== dateMonth.getMonth()) button.classList.add('is-out');
                if (dateFrom && dateTo && value >= dateFrom && value <= dateTo) button.classList.add('is-between');
                if (value === dateFrom || value === dateTo) button.classList.add('is-picked');
                if (dateTarget === 'to' && dateFrom && value < dateFrom) { button.disabled = true; button.title = 'Đến ngày không được trước Từ ngày'; }
                days.appendChild(button);
            }
            pop.querySelector('.pmc-date-error').textContent = ''; positionDatePop();
        }
        function openDatePop() {
            var pop = ensureDatePop(); dateFrom = get('from'); dateTo = get('to'); dateTarget = 'from';
            var reference = parseISO(dateFrom) || new Date(); dateMonth = new Date(reference.getFullYear(), reference.getMonth(), 1);
            pop.hidden = false; var trigger = document.getElementById('pmcRangeToggle'); if (trigger) trigger.setAttribute('aria-expanded', 'true');
            paintDatePop(); positionDatePop();
        }
        function toggleDatePop() { if (datePop && !datePop.hidden) hideDatePop(); else openDatePop(); }
        function connect() {
            var r = root(); if (!r) return; syncFilters();
            var search = searchInput(), state = document.getElementById('pmcStateFilter'), toggle = document.getElementById('pmcRangeToggle');
            if (search && !search.__pmcBound) {
                search.__pmcBound = true;
                search.addEventListener('input', function (e) { if (!liveComposing && !e.isComposing) onLiveInput(); });
                search.addEventListener('compositionstart', function () { liveComposing = true; });
                search.addEventListener('compositionend', function () { liveComposing = false; onLiveInput(); });
                search.addEventListener('keydown', function (e) { if (e.key === 'Enter' && !liveComposing && !e.isComposing) { e.preventDefault(); e.stopPropagation(); dispatchLive(); } });
            }
            if (state && !state.__pmcBound) { state.__pmcBound = true; state.addEventListener('change', requestFilters); }
            if (toggle && !toggle.__pmcBound) { toggle.__pmcBound = true; toggle.addEventListener('click', function (event) { event.preventDefault(); event.stopPropagation(); toggleDatePop(); }); }
            if (!r.__pmcClickBound) {
                r.__pmcClickBound = true; r.addEventListener('click', function (event) {
                    var b = event.target.closest ? event.target.closest('[data-pmc-action]') : null; if (!b) return; event.preventDefault();
                    var act = b.getAttribute('data-pmc-action');
                    if (busy) { if (act === 'select') pendingSelection = b.getAttribute('data-pmc-id'); else if (act === 'meeting-page' || act === 'roster-page') pendingPage = { type: act, page: b.getAttribute('data-pmc-page') }; return; }
                    if (act === 'select') { val('meeting', b.getAttribute('data-pmc-id')); val('rosterPage', '1'); submit('select'); return; }
                    if (act === 'meeting-page') { val('page', b.getAttribute('data-pmc-page')); val('rosterPage', '1'); submit('page'); return; }
                    if (act === 'roster-page') { val('rosterPage', b.getAttribute('data-pmc-page')); submit('roster-page'); }
                });
            }
        }
        function show() { var n = modal(); if (!n) return; n.classList.add('pmc-popup-modal'); if (window.bootstrap && bootstrap.Modal) { var m = bootstrap.Modal.getInstance ? bootstrap.Modal.getInstance(n) : null; (m || new bootstrap.Modal(n)).show(); } else if (window.jQuery && window.jQuery.fn.modal) window.jQuery(n).modal('show'); }
        function open(button) {
            if (busy || !button) return false; var type = button.getAttribute('data-pd-detail-source'); if (type !== 'pdPersonalMeetingDetail' && type !== 'pdTeamDetail-meeting') return false;
            var member = ''; if (type === 'pdTeamDetail-meeting') { var h = document.querySelector('#projectTeamHoursView input[type="hidden"][id$="hfTeamHoursMember"]'); member = h ? h.value : ''; if (!member) return false; }
            var title = document.getElementById('pmcPopupTitle'), sub = document.getElementById('pmcPopupSub');
            if (title) title.textContent = button.getAttribute('data-pd-detail-title') || 'Chi tiết cuộc họp';
            if (sub) sub.textContent = button.getAttribute('data-pd-detail-sub') || 'Danh sách cuộc họp và giờ công của nhân viên tham gia.';
            hideDatePop(); clearLiveTimer(); liveDraft = null; liveCompleted = null; liveInFlight = false; liveQueued = false; pendingSelection = null; pendingPage = null; pendingFilter = false;
            val('kind', type); val('member', member); val('search', ''); val('state', type === 'pdTeamDetail-meeting' ? 'ended' : 'all'); val('from', ''); val('to', ''); val('page', '1'); val('rosterPage', '1'); val('meeting', ''); openRequest = true; return submit('open');
        }
        // V21: chieu cao hang theo phan dien tich con lai, khong doi kich thuoc popup.
        var pmcLayoutFrame = 0, pmcHostTip = null, pmcHostActive = null, pmcObserver = null;
        function pmcFitRoster() {
            var r = root(), panel = r && r.querySelector('#pmcRoster');
            if (!panel) return;
            var table = panel.querySelector('.pmc-table'), head = table && table.querySelector('thead');
            var rows = table && table.querySelectorAll('tbody tr');
            if (!rows || !rows.length) return;
            var available = Math.max(0, panel.clientHeight - (head ? head.offsetHeight : 49) - 2);
            var perRow = available / rows.length;
            // 5 nhan vien: can deu chieu cao. Trang co it nguoi: khong keo mot hang qua cao.
            var target = Math.max(74, Math.min(96, Math.floor(perRow)));
            r.style.setProperty('--pmc-roster-row-height', target + 'px');
        }
        function pmcScheduleFit() {
            if (pmcLayoutFrame) return;
            pmcLayoutFrame = (window.requestAnimationFrame || function (fn) { return window.setTimeout(fn, 16); })(function () { pmcLayoutFrame = 0; pmcFitRoster(); });
        }
        function pmcHideHostTip() { if (pmcHostTip && pmcHostTip.parentNode) pmcHostTip.parentNode.removeChild(pmcHostTip); pmcHostTip = null; pmcHostActive = null; }
        function pmcHostIcon(node) {
            if (!node || !node.closest) return null;
            var icon = node.closest('.pmc-host,.pmc-staff .fa-crown,.pmc-meeting .fa-crown');
            var r = root(); return icon && r && r.contains(icon) ? icon : null;
        }
        function pmcPrepareHostIcons() {
            var r = root(); if (!r) return;
            var icons = r.querySelectorAll('.pmc-host,.pmc-staff .fa-crown,.pmc-meeting .fa-crown');
            for (var i = 0; i < icons.length; i++) {
                var icon = icons[i];
                if (icon.hasAttribute('data-pmc-host-label')) continue;
                var name = icon.closest('.pmc-meeting') ? 'Bạn chủ trì' : 'Chủ trì';
                icon.setAttribute('data-pmc-host-label', name);
                icon.setAttribute('aria-label', name);
                icon.setAttribute('role', 'img');
                if (!icon.hasAttribute('tabindex')) icon.setAttribute('tabindex', '0');
                // Loai tooltip title mac dinh de khong hien trung 2 tooltip.
                if (icon.hasAttribute('title')) icon.removeAttribute('title');
            }
        }
        function pmcShowHostTip(icon) {
            if (!icon) return;
            if (icon === pmcHostActive && pmcHostTip) return;
            pmcHideHostTip();
            var label = icon.getAttribute('data-pmc-host-label') || 'Chủ trì';
            var box = document.createElement('div'); box.className = 'pmc-host-tooltip'; box.setAttribute('role', 'tooltip');
            var crown = document.createElement('i'); crown.className = 'fas fa-crown'; crown.setAttribute('aria-hidden', 'true');
            box.appendChild(crown); box.appendChild(document.createTextNode(label)); document.body.appendChild(box);
            pmcHostTip = box; pmcHostActive = icon;
            var a = icon.getBoundingClientRect(), b = box.getBoundingClientRect(), pad = 9;
            var x = a.right + 10; if (x + b.width > window.innerWidth - pad) x = a.left - b.width - 10;
            if (x < pad) x = Math.max(pad, Math.min(a.left, window.innerWidth - b.width - pad));
            var y = Math.max(pad, Math.min(a.top + (a.height - b.height) / 2, window.innerHeight - b.height - pad));
            box.style.left = Math.round(x) + 'px'; box.style.top = Math.round(y) + 'px';
        }
        function pmcUiEnhance() {
            pmcHideHostTip(); pmcPrepareHostIcons(); pmcScheduleFit();
            var r = root(); if (!r) return;
            if (window.ResizeObserver) {
                if (pmcObserver) pmcObserver.disconnect();
                pmcObserver = new ResizeObserver(pmcScheduleFit);
                var panel = r.querySelector('#pmcRoster'); if (panel) pmcObserver.observe(panel);
            }
            if (window.__pmcHostTooltipGlobalBound21) return;
            window.__pmcHostTooltipGlobalBound21 = true;
            // Listener gan vao document: khong bi mat hoac trung sau UpdatePanel.
            document.addEventListener('mouseover', function (e) { var t = pmcHostIcon(e.target); if (t) pmcShowHostTip(t); });
            document.addEventListener('mouseout', function (e) { if (pmcHostActive && pmcHostActive.contains(e.target) && (!e.relatedTarget || !pmcHostActive.contains(e.relatedTarget))) pmcHideHostTip(); });
            document.addEventListener('focusin', function (e) { var t = pmcHostIcon(e.target); if (t) pmcShowHostTip(t); });
            document.addEventListener('focusout', function (e) { if (e.target === pmcHostActive) pmcHideHostTip(); });
            document.addEventListener('click', function (e) { if (e.target.closest && e.target.closest('#pmcRangeToggle')) window.setTimeout(pmcScheduleFit, 0); });
            document.addEventListener('scroll', pmcHideHostTip, true);
            document.addEventListener('click', function (event) {
                if (datePop && !datePop.hidden && !datePop.contains(event.target) && !(event.target.closest && event.target.closest('#pmcRangeToggle'))) hideDatePop();
            });
            document.addEventListener('keydown', function (event) { if (event.key === 'Escape' && datePop && !datePop.hidden) { hideDatePop(); event.stopPropagation(); } });
            window.addEventListener('scroll', function () { if (datePop && !datePop.hidden) positionDatePop(); }, true);
            window.addEventListener('resize', function () { pmcHideHostTip(); pmcScheduleFit(); positionDatePop(); });
            document.addEventListener('hidden.bs.modal', function (e) { if (e.target === modal()) { pmcHideHostTip(); hideDatePop(); } });
            document.addEventListener('shown.bs.modal', function (e) { if (e.target === modal()) pmcScheduleFit(); });
        }
        function received(isOpen) {
            var finished = requestKind;
            busy = false; requestKind = '';
            if (finished === 'search-preview') {
                liveInFlight = false; liveCompleted = liveSent;
                // Danh sach duoc render boi UpdatePanel nho; o search va bang phai van giu DOM cu.
                connect(); previewFilter(); pmcPrepareHostIcons();
                if (pendingFilter) { pendingFilter = false; requestFilters(); return; }
                if (pendingSelection) { var id = pendingSelection; pendingSelection = null; val('meeting', id); val('rosterPage', '1'); submit('select'); return; }
                if (pendingPage) { var next = pendingPage; pendingPage = null; if (next.type === 'meeting-page') { val('page', next.page); val('rosterPage', '1'); submit('page'); } else { val('rosterPage', next.page); submit('roster-page'); } return; }
                if (liveDraft !== null && liveDraft !== liveCompleted) { liveQueued = true; dispatchLive(); return; }
                liveQueued = false; previewFilter(); pmcScheduleFit(); return;
            }
            liveInFlight = false; liveQueued = false; liveCompleted = get('search');
            if (finished === 'filter') pendingFilter = false;
            connect(); syncFilters(); pmcUiEnhance();
            if (isOpen || openRequest) { openRequest = false; show(); var n = modal(), body = n ? n.querySelector('.modal-body') : null; if (body) body.scrollTop = 0; window.setTimeout(pmcScheduleFit, 120); } else pmcScheduleFit();
            if (finished === 'filter' || finished === 'open') previewFilter();
        }
        function failed() { busy = false; requestKind = ''; openRequest = false; liveInFlight = false; liveQueued = false; busyIndicator(false); }
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager && !window.__pmcEndRequestBound) { window.__pmcEndRequestBound = true; Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { if (busy && args.get_error()) failed(); }); }
        window.QLDAProjectMeetingPopup = { open: open, received: received, failed: failed };
    })();
</script>
