<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlMeetGioCongTab.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlMeetGioCongTab" %>
<style>
.mlt-layout { align-items:start;display:grid;grid-template-columns:minmax(0,30%) minmax(0,1fr);gap:14px;width:100%;max-width:100%;min-width:0;box-sizing:border-box;min-height:0;padding:10px 14px 14px;background:#f8f9fc; }
.mlt-side,.mlt-main { display:flex;flex-direction:column;min-width:0;min-height:0;width:100%;max-width:100%;height:var(--meet-labor-card-height, clamp(240px, calc(100dvh - 420px), 540px));max-height:var(--meet-labor-card-height, clamp(240px, calc(100dvh - 420px), 540px));box-sizing:border-box;background:#fff;border:1px solid #e4dcea;border-radius:12px;overflow:hidden; }
.mlt-selected-content { display:flex;flex-direction:column;flex:1 1 auto;height:100%;max-height:100%;width:100%;max-width:100%;min-width:0;min-height:0;box-sizing:border-box;overflow:hidden; }
.mlt-side-head { flex:0 0 auto;display:flex;align-items:center;justify-content:space-between;gap:9px;flex-wrap:wrap;min-width:0;padding:12px 14px 11px;border-bottom:1px solid #e9e0f1;background:#faf7ff; }
.mlt-side-head-top { display:flex;flex-direction:column;align-items:flex-start;gap:4px;width:100%;min-width:0; }
.mlt-head-range { display:block;color:#8b7b99;font-size:11px;font-weight:650;line-height:1.3;white-space:nowrap; }
.mlt-head-range:empty { display:none; }
.mlt-side-title { display:flex;gap:8px;align-items:center;color:#51218a;font-weight:800;font-size:14px; }
.mlt-meeting-count { display:inline-flex;align-items:center;justify-content:center;min-width:24px;height:22px;padding:1px 7px;margin-left:1px;border:1px solid #e1d4f2;border-radius:8px;background:#fff;color:#54218d;font-size:11px;font-weight:800;white-space:nowrap; }
.mlt-filter { display:grid;grid-template-columns:1.28fr 1fr 1fr 1.05fr;width:100%;max-width:100%;min-width:0;box-sizing:border-box;border:1px solid #dac8ee;border-radius:9px;overflow:hidden;background:#fff;color:#54218d;padding:3px;gap:3px; }
.mlt-filter a,.mlt-filter .aspNetDisabled { display:inline-flex;align-items:center;justify-content:center;min-width:0;gap:5px;padding:8px 5px;border-radius:6px;text-decoration:none;color:#6f4b91;font-size:10.5px;font-weight:750;white-space:normal;text-align:center;line-height:1.25;overflow-wrap:break-word;transition:background-color .15s,color .15s; }
.mlt-filter a:hover { background:#f4edfd; }
.mlt-filter a.is-filtered { color:#fff;background:#54218d; }
.mlt-page-footer { flex:0 0 auto;display:flex;align-items:center;justify-content:center;gap:8px;min-width:0;min-height:52px;padding:9px 12px;border-top:1px solid #e9e0f1;background:#fbf9fe; }
.mlt-page-footer .mlt-page-info,.mlt-labor-page-footer .mlt-page-info { display:none!important; }
.mlt-page-footer .mlt-page-controls,.mlt-labor-page-footer .mlt-page-controls { margin:0;justify-content:center; }
.mlt-page-info { color:#81748c;font-size:11px;font-weight:650;white-space:nowrap; }
.mlt-page-controls { display:inline-flex;align-items:center;justify-content:flex-end;flex-wrap:wrap;gap:4px;margin-left:auto; }
.mlt-page-button { display:inline-grid;place-items:center;min-width:31px;height:32px;padding:5px 8px;border:1px solid #e4d9f0;border-radius:7px;background:#fff;color:#60338c;font-size:12.5px;font-weight:750;cursor:pointer;transition:background .15s,border-color .15s; }
.mlt-page-button:hover:not([disabled]):not(.is-active) { background:#f1e9fc;border-color:#cbb6e5; }
.mlt-page-button.is-active { background:#54218d;color:#fff;border-color:#54218d;cursor:default; }
.mlt-page-button[disabled] { opacity:.42;cursor:not-allowed; }
.mlt-page-ellipsis { color:#8d7f9a;padding:0 2px;font-size:11px; }
.mlt-meeting-list { flex:1 1 auto;min-height:0;max-height:none;overflow-y:auto;scrollbar-width:thin; }
.mlt-meeting-link { display:block;padding:12px 15px;text-decoration:none!important;color:#443651;border-bottom:1px solid #eee9f3;border-left:3px solid transparent;transition:background .15s,border-color .15s; }
.mlt-meeting-link:hover { color:#54218d;background:#fbf8ff; }
.mlt-meeting-link.is-active { background:#f2ebfb;border-left-color:#6127a4;color:#4d1988; }
.mlt-meeting-link strong { display:block;line-height:1.45;font-size:14px;font-weight:750;overflow-wrap:anywhere; }
.mlt-meeting-link small { display:block;margin-top:6px;color:#84768f;font-size:12px;line-height:1.45; }
.mlt-meeting-link .mlt-done { font-size:10.5px;color:#8891a2;background:#edf0f5;border-radius:5px;padding:2px 6px;margin-left:5px; }
.mlt-main-head { flex:0 0 auto;display:flex;align-items:flex-start;justify-content:space-between;gap:10px 16px;flex-wrap:wrap;padding:12px 16px;border-bottom:1px solid #ebe3f3;background:linear-gradient(100deg,#f8f3ff,#fff); }
.mlt-main-title { color:#4d1b83;font-size:19px;font-weight:800;line-height:1.45;overflow-wrap:anywhere; }
.mlt-main-head-info { min-width:0;flex:1 1 320px; }
.mlt-main-head-side { display:flex;flex-direction:column;align-items:flex-end;justify-content:flex-start;gap:5px;flex:0 0 auto;min-width:0; }
.mlt-main-head-side .mlt-head-range { padding-right:2px;text-align:right; }
.mlt-main-meta { display:flex;align-items:center;gap:11px 20px;flex-wrap:wrap;margin-top:7px;color:#83718f;font-size:12px; }
.mlt-main-meta i { margin-right:5px;color:#8352b8; }
.mlt-duration { display:inline-flex;gap:6px;align-items:center;margin-top:0;padding:6px 10px;background:#fff;border:1px solid #e3d5ef;border-radius:8px;color:#663298;font-weight:800;font-size:11px;white-space:nowrap; }
.mlt-table-scroll { flex:1 1 auto;width:100%;max-width:100%;min-width:0;min-height:0;box-sizing:border-box;max-height:none;overflow-x:auto;overflow-y:auto;scrollbar-width:thin; }
.mlt-table { width:100%;border-collapse:separate;border-spacing:0;margin:0;table-layout:fixed; }
.mlt-table th { position:sticky;top:0;z-index:2;background:#f4eff9;color:#603487;text-align:left;white-space:nowrap;padding:11px 11px;font-size:12.5px;font-weight:800;border-bottom:1px solid #e4d6ef; }
.mlt-table td { padding:5px 11px;border-bottom:1px solid #f0eaf3;vertical-align:middle;font-size:13px;color:#4c4156; }
.mlt-table tbody tr:last-child td { border-bottom:0; }
.mlt-table tbody tr:hover td { background:#fdfbff; }
/* Moi trang (ke ca trang cuoi chi 1 nhan vien) deu dung 1 chieu cao hang nhu trang 5 nguoi.
   Gia tri tinh tu phan thuc su con lai trong .mlt-table-scroll, khong keo gian ca bang. */
.mlt-table tbody tr.mlt-labor-row { height:var(--mlt-equal-row-height, 78px); }
.mlt-user { display:flex;align-items:center;gap:6px;flex-wrap:wrap;font-weight:750;line-height:1.4;overflow-wrap:anywhere; }
.mlt-user.is-host { color:#66279d; }
.mlt-user .fa-crown { color:#daa021; }
.mlt-host-tag { font-size:10.5px;color:#99660f;background:#fff0cd;padding:2px 5px;border-radius:5px; }
.mlt-join { width:auto!important;border-collapse:collapse;border:1px solid #d7c3ec;border-radius:8px;overflow:hidden;background:#fff; }
.mlt-join td { padding:0!important;border-bottom:0!important;border-right:1px solid #d7c3ec!important;vertical-align:middle;position:relative; }
.mlt-join td:last-child { border-right:0!important; }
.mlt-join input[type=radio] { position:absolute;opacity:0;width:1px;height:1px; }
.mlt-join label { display:block;cursor:pointer;padding:7px 9px;font-weight:800;font-size:11.5px;color:#7d6396;white-space:nowrap;line-height:1.4; }
.mlt-join td:has(input:checked) label { background:#5b2499;color:#fff; }
.mlt-join input:disabled + label,.mlt-join label:has(input:disabled) { cursor:default; }
.mlt-join td:has(input:focus-visible) { outline:2px solid #8d60be;outline-offset:-2px; }
.mlt-join[data-edit='0'] label { cursor:default; }
.mlt-theory { font-weight:800;color:#5b298f;white-space:nowrap; }
.mlt-actual { width:105px;max-width:100%;padding:8px 9px;text-align:right;border:1px solid #d9c6ec;border-radius:8px;color:#572c86;font-size:13px;font-weight:750; }
.mlt-actual:focus { outline:2px solid #ac83d5;outline-offset:1px; }
.mlt-actual[readonly] { color:#9c96a4;background:#f2f2f5;border-color:#e5e4e9; }
/* Non-ended meetings: the ENTIRE right pane is locked (not just the save button). */
.mlt-locked-panel { position:relative;isolation:isolate;flex:1;min-width:0;min-height:0;width:100%;display:flex;align-items:center;justify-content:center;padding:40px 24px;overflow:hidden;box-sizing:border-box;background:linear-gradient(140deg,#f3f3f6,#faf9fc 55%,#f0eff4);text-align:center; }
.mlt-locked-panel::before,.mlt-locked-panel::after { content:"";pointer-events:none;position:absolute;z-index:-1;left:-23%;top:48%;width:146%;height:16px;border:1px solid rgba(87,88,102,.22);border-radius:100px;background:repeating-linear-gradient(90deg,#c2c4cc 0,#c2c4cc 17px,#ececf0 17px,#ececf0 25px,#999eab 25px,#999eab 28px);opacity:.42; }
.mlt-locked-panel::before { transform:rotate(24deg); }
.mlt-locked-panel::after { transform:rotate(-24deg); }
.mlt-lock-card { position:relative;z-index:1;max-width:425px;padding:26px 30px;border:1px solid #dfdce7;border-radius:18px;background:rgba(255,255,255,.96);box-shadow:0 14px 45px rgba(43,33,59,.06); }
.mlt-lock-symbol { position:relative;display:flex;align-items:center;justify-content:center;width:86px;height:86px;margin:0 auto 18px;border:2px solid #c9c4d1;border-radius:50%;background:#f5f3f7;color:#727080;font-size:30px; }
.mlt-lock-cross { position:absolute;right:-7px;bottom:-3px;display:grid;place-items:center;width:34px;height:34px;border:3px solid #fff;border-radius:50%;background:#c84b5a;color:#fff;font-size:17px; }
.mlt-lock-heading { display:block;margin:0;color:#41364d;font-size:19px;font-weight:800;line-height:1.4; }
.mlt-lock-meeting { display:block;margin:9px 0 0;color:#5c2a91;font-size:13px;font-weight:750;overflow-wrap:anywhere; }
.mlt-lock-note { display:block;margin:8px 0 0;color:#797486;font-size:12.5px;line-height:1.7; }
/* Chỉ còn một footer: tổng tham gia trái, phân trang giữa, lưu giờ công phải. */
.mlt-main-foot { flex:0 0 auto;display:grid;grid-template-columns:minmax(0,1fr) auto minmax(0,1fr);align-items:center;gap:8px;padding:11px 15px;border-top:1px solid #eae3f0;background:#faf8fc;min-height:60px;box-sizing:border-box; }
.mlt-labor-page-footer { display:flex;align-items:center;justify-content:center;min-width:0;min-height:0;margin:0;padding:0;border:0;background:transparent; }
.mlt-main-foot .mlt-total { justify-self:start; }
.mlt-main-foot .mlt-save { justify-self:end;white-space:nowrap; }
.mlt-hidden-theory { display:none!important; }
@media(max-width:640px){ .mlt-main-foot { grid-template-columns:1fr auto;row-gap:8px; } .mlt-main-foot .mlt-labor-page-footer { grid-column:1/-1;grid-row:2; } }
.mlt-total { display:flex;gap:12px;flex-wrap:wrap;color:#7c698a;font-size:13px;font-weight:700; }
.mlt-total b { color:#57258f; }
.mlt-save { display:inline-flex;align-items:center;gap:7px;min-height:36px;padding:7px 12px;background:#54218d!important;border-color:#54218d!important;color:#fff!important;font-size:12.5px!important;font-weight:800!important; }
.mlt-empty { display:grid;place-items:center;gap:9px;min-height:250px;padding:32px 18px;color:#8e859b;text-align:center;font-size:13px; }
.mlt-empty i { font-size:28px;color:#bba6d1; }
/* Empty labels must take zero height: prevents the Labor tab from sitting lower than Calendar. */
.mlt-error { display:block;color:#be3546;font-size:12px;padding:8px 14px; }
.mlt-error:empty,.mlt-success:empty { display:none!important;padding:0!important;border:0!important; }
.meet-view-shell .meet-panel-labor { padding:0!important;margin:0!important; }
.meet-view-shell .meet-panel-labor .mlt-layout { padding-top:10px; }
.mlt-success { display:block;color:#16803c;background:#f1fbf4;border-top:1px solid #d8eedf;font-size:12px;font-weight:750;padding:9px 14px; }
@media(max-width:1200px) { .mlt-layout { grid-template-columns:minmax(0,32%) minmax(0,1fr); } .mlt-table { min-width:660px; } }
@media(max-width:780px) { .mlt-layout { grid-template-columns:minmax(0,1fr); } .mlt-side,.mlt-main { height:var(--meet-labor-card-height, clamp(240px, calc(100dvh - 420px), 540px));max-height:var(--meet-labor-card-height, clamp(240px, calc(100dvh - 420px), 540px)); } .mlt-main-head { padding:12px; } .mlt-main-head-side { width:100%;justify-content:flex-start; } }

/* Hộp xác nhận riêng của giờ công, không dùng alert/confirm của browser. */
.mlt-confirm-backdrop {position:fixed;inset:0;z-index:2147480000;display:flex;align-items:center;justify-content:center;padding:20px;background:rgba(25,19,38,.55);backdrop-filter:blur(2px);}
.mlt-confirm-box {width:min(100%,440px);padding:24px;border:1px solid #e2d7ef;border-radius:15px;background:#fff;box-shadow:0 22px 70px rgba(28,12,53,.25);color:#382748;}
.mlt-confirm-title {display:flex;align-items:center;gap:10px;font-size:17px;font-weight:800;color:#52218a;}
.mlt-confirm-title i {display:grid;place-items:center;width:36px;height:36px;border-radius:10px;background:#f4ebff;}
.mlt-confirm-text {margin:15px 0 22px;color:#665778;font-size:13px;line-height:1.65;}
.mlt-confirm-actions {display:flex;flex-wrap:wrap;justify-content:flex-end;gap:8px;}
.mlt-confirm-actions button {min-height:37px;padding:8px 12px;border-radius:8px;border:1px solid #d8cbe7;cursor:pointer;font-weight:750;font-size:12px;}
.mlt-confirm-cancel {background:#fff;color:#655476;}
.mlt-confirm-discard {background:#f8f5fb;color:#54218d;}
.mlt-confirm-save {background:#54218d;color:#fff;border-color:#54218d!important;}
.mlt-confirm-actions button:disabled {opacity:.55;cursor:wait;}
@media(max-width:500px){.mlt-confirm-actions button {flex:1 1 100%;}.mlt-confirm-box{padding:18px;}}

/* Cuộc họp 5 mục/trang; nhân viên 5 mục/trang (hằng LaborPerPage bên code-behind). */
.mlt-meeting-title-line {display:flex;align-items:flex-start;justify-content:space-between;gap:6px;min-width:0;}
.mlt-meeting-title-line strong {flex:1;min-width:0;}
.mlt-meeting-host {display:inline-flex;align-items:center;gap:4px;flex-shrink:0;margin-left:auto;margin-top:1px;padding:3px 6px;border:1px solid #f2d69e;border-radius:6px;background:#fff5dc;color:#99640e;font-size:10.5px;font-weight:800;line-height:1.2;white-space:nowrap;}
.mlt-meeting-host i {font-size:11px;color:#d69a19;}
/* Badge status va badge chu tri nam ben phai ten, khong chiem dong ngay gio. */
.mlt-meeting-title-badges {display:inline-flex;align-items:center;justify-content:flex-end;gap:5px;flex:0 0 auto;max-width:60%;flex-wrap:wrap;}
.mlt-meeting-title-badges .mlt-done {display:inline-flex;align-items:center;margin:0;padding:3px 6px;border:1px solid #e2e5ec;font-size:10px;line-height:1.2;white-space:nowrap;}
.mlt-meeting-title-badges .mlt-meeting-host {margin:0;}
.mlt-meeting-time-divider {display:inline-block;margin:0 2px;color:#967da9;font-weight:800;}

/* Thông tin nhân viên: avatar + tên + email, cùng hệ màu với CostList. */
.mlt-user { display:flex;align-items:center;flex-wrap:nowrap;gap:9px;min-width:0;width:100%; }
.mlt-user .mlt-person-avatar { position:relative;display:flex;align-items:center;justify-content:center;width:40px;height:40px;min-width:40px;flex:0 0 40px;border:1px solid #c4b5fd;border-radius:50%;overflow:hidden;color:#fff;font-size:11px;font-weight:800; }
.mlt-person-avatar img { display:block;width:100%;height:100%;object-fit:cover; }
.mlt-person-initials { width:100%;height:100%;align-items:center;justify-content:center;color:#fff;font-size:11px;font-weight:800; }
.mlt-person-copy { display:flex;flex-direction:column;justify-content:center;gap:3px;flex:1;min-width:0; }
.mlt-person-name-line { display:flex;align-items:center;gap:5px;min-width:0;flex-wrap:wrap; }
.mlt-person-name { color:#334155;font-size:13.5px;font-weight:750;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;max-width:100%; }
.mlt-user.is-host .mlt-person-name { color:#632a9c; }
.mlt-person-email { color:#8390a3;font-size:11.7px;font-weight:500;line-height:1.2;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;max-width:100%; }
.mlt-user .fa-crown { color:#daa021; }
/* Chu tri: chi giu icon, khong hien chu (giu tagHost trong markup cho code-behind). */
.mlt-host-tag {display:none!important;}
.mlt-meeting-title-badges .mlt-meeting-host {
    display:inline-flex;align-items:center;justify-content:center;flex:0 0 24px;
    width:24px;height:24px;min-width:24px;margin:0;padding:0;
    border:1px solid #f2d69e;border-radius:7px;background:#fff5dc;
    color:#a66b0d;font-size:0!important;line-height:1;cursor:help;
}
.mlt-meeting-title-badges .mlt-meeting-host i {display:block;font-size:12px;color:#d59b16;line-height:1;}
.mlt-person-host-icon {flex:0 0 auto;color:#d59b16!important;font-size:14px;cursor:help;}
.mlt-meeting-host:focus-visible,.mlt-person-host-icon:focus-visible {outline:2px solid #dca742;outline-offset:2px;}
/* Tooltip noi ngoai khung scroll de khong bi cat khi hover crown. */
.mlt-host-hover-tip {
    position:fixed;z-index:2147483000;display:flex;align-items:center;gap:7px;
    width:max-content;max-width:220px;padding:8px 12px;
    border:1px solid #edcf91;border-radius:8px;background:#fff8e9;
    box-shadow:0 8px 24px rgba(132,90,14,.13);
    color:#9d660e;font-size:12px;font-weight:800;line-height:1.4;
    pointer-events:none;white-space:nowrap;
}
.mlt-host-hover-tip i {color:#d59b16;font-size:12px;}


/* Meet labor UI 2026-10-23: filters, date range and compact header. */
.mlt-side-head { position:relative;flex-direction:column;align-items:stretch;gap:9px;padding:11px 13px 12px; }
.mlt-side-head-top { flex-direction:row;align-items:center;justify-content:space-between;gap:8px; }
.mlt-side-title { min-width:0;font-size:13.2px;white-space:nowrap; }
.mlt-side-tools { display:grid;grid-template-columns:minmax(0,1fr) 147px;gap:7px;min-width:0;width:100%; }
.mlt-search { position:relative;display:flex;align-items:center;min-width:0; }
.mlt-search > i { position:absolute;left:10px;z-index:1;color:#9379b0;font-size:11px;pointer-events:none; }
.mlt-search input { width:100%;min-width:0;height:36px;padding:6px 9px 6px 29px;border:1px solid #dccded;border-radius:8px;background:#fff;color:#443451;font-size:12px;font-weight:600;line-height:1.3;font-family:inherit;outline:none;transition:border-color .15s,box-shadow .15s; }
.mlt-search input::placeholder { color:#9b91a9;font-weight:500; }
.mlt-search input:focus { border-color:#a876d7;box-shadow:0 0 0 3px rgba(124,58,173,.1); }
.mlt-filter-select { width:100%;min-width:0;height:36px;padding:5px 22px 5px 9px;border:1px solid #dccded;border-radius:8px;background:#fff;color:#54218d;font-size:11.6px;font-weight:750;outline:none;cursor:pointer; }
.mlt-range-launch { display:inline-flex;align-items:center;justify-content:center;gap:6px;flex:0 0 auto;min-width:0;max-width:47%;min-height:31px;padding:5px 9px;border:1px solid #d9c6ed;border-radius:8px;background:#fff;color:#68329b;font-size:11px;font-weight:750;white-space:nowrap;cursor:pointer; }
.mlt-range-launch.is-active { border-color:#ae7eda;background:#f3eaff; }
.mlt-range-launch:hover { background:#eee3fb; }
.mlt-filter-internal { display:none!important; }
.mlt-page-footer { justify-content:space-between;min-height:50px;gap:8px; }
.mlt-page-footer .mlt-page-info,.mlt-labor-page-footer .mlt-page-info { display:inline!important;font-size:11.2px;font-weight:700;color:#81718f;white-space:nowrap; }
.mlt-page-footer .mlt-page-controls,.mlt-labor-page-footer .mlt-page-controls { margin-left:auto;flex-wrap:nowrap; }
.mlt-labor-page-footer { width:100%;gap:8px;justify-content:space-between; }
.mlt-main-foot { grid-template-columns:minmax(0,1fr) minmax(240px,1.25fr) auto; }
.mlt-main-head { align-items:center;row-gap:10px;padding:11px 16px; }
.mlt-main-head-info { flex:1 1 0; }
.mlt-main-title-line { display:flex;align-items:baseline;flex-wrap:wrap;gap:5px 11px;min-width:0; }
.mlt-main-title { font-size:19px;line-height:1.25; }
.mlt-main-time { color:#8d7a9d;font-size:11.5px;font-weight:550;line-height:1.4;white-space:nowrap; }
.mlt-main-time i { margin-right:3px;color:#8c65b2; }
.mlt-staff-tools { display:flex;gap:9px;align-items:center;justify-content:space-between;flex:0 0 100%;min-width:0;width:100%; }
.mlt-staff-tools .mlt-search { max-width:330px;flex:1 1 220px; }
.mlt-staff-choice { display:inline-flex;flex:0 0 auto;gap:4px;padding:3px;border:1px solid #dfd0ed;border-radius:9px;background:#fff; }
.mlt-staff-choice button { border:0;border-radius:6px;background:transparent;color:#775b95;padding:7px 10px;line-height:1.2;font-size:11.5px;font-weight:750;cursor:pointer;transition:background .15s,color .15s; }
.mlt-staff-choice button:hover { background:#f4edfc; }
.meet-panel-labor .mlt-staff-choice button.is-active,
.meet-panel-labor .mlt-staff-choice button[aria-pressed="true"] { background:#54218d!important;color:#fff!important;box-shadow:0 2px 7px rgba(84,33,141,.16);font-weight:800; }
.meet-panel-labor .mlt-staff-choice button.is-active:hover,
.meet-panel-labor .mlt-staff-choice button[aria-pressed="true"]:hover { background:#441777!important;color:#fff!important; }
.meet-panel-labor .mlt-staff-choice button:focus-visible {outline:2px solid #9065cb;outline-offset:2px;}
.mlt-range-pop { position:fixed;z-index:2147480001;width:332px;max-width:calc(100vw - 20px);border:1px solid #dcccEC;border-radius:13px;background:#fff;color:#40334d;box-shadow:0 18px 42px rgba(45,19,80,.18);padding:14px; }
.mlt-range-pop[hidden] { display:none!important; }
.mlt-range-head { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:10px;font-size:13.5px;font-weight:800;color:#51218a; }
.mlt-range-close { border:0;background:transparent;color:#947baa;font-size:18px;line-height:1;cursor:pointer; }
.mlt-range-fields { display:grid;grid-template-columns:1fr 1fr;gap:7px;margin-bottom:11px; }
.mlt-range-field { min-width:0;display:flex;flex-direction:column;align-items:flex-start;gap:3px;border:1px solid #e3d8ef;background:#fbf9fe;border-radius:8px;padding:7px 9px;text-align:left;cursor:pointer; }
.mlt-range-field.is-active { border-color:#ad79e1;background:#f3eafa;box-shadow:inset 0 0 0 1px #d5b6f6; }
.mlt-range-field small { color:#8d76a2;font-size:10.5px; }
.mlt-range-field strong { color:#51218a;font-size:12px; }
.mlt-range-month { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:8px;color:#51218a;font-size:12.5px;font-weight:800; }
.mlt-range-month button { width:28px;height:28px;border:1px solid #e3d5ef;border-radius:7px;background:#fff;color:#6d399a;cursor:pointer; }
.mlt-range-days { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:3px; }
.mlt-range-weekday { text-align:center;padding:4px 0;color:#9781ab;font-size:10.2px;font-weight:750; }
.mlt-range-day { display:grid;place-items:center;aspect-ratio:1/1;min-width:0;border:1px solid transparent;border-radius:7px;background:#fff;color:#54415f;font-size:11.5px;cursor:pointer; }
.mlt-range-day:hover:not(:disabled) { background:#efe4fb;border-color:#d8c4eb; }
.mlt-range-day.is-out { opacity:.36; }
.mlt-range-day.is-between { background:#f2eafa; }
.mlt-range-day.is-picked { background:#6725a0;color:#fff;font-weight:800; }
.mlt-range-day:disabled { color:#c1bac7;cursor:not-allowed; }
.mlt-range-error { min-height:15px;margin:6px 0 0;color:#ba4242;font-size:11px; }
.mlt-range-foot { display:flex;justify-content:space-between;gap:7px;margin-top:8px; }
.mlt-range-foot button { flex:1;padding:8px 10px;border:1px solid #dbc7eb;border-radius:7px;background:#fff;color:#653396;font-size:11.5px;font-weight:800;cursor:pointer; }
.mlt-range-foot .mlt-range-apply { border-color:#54218d;background:#54218d;color:#fff; }
@media(max-width:1200px){ .mlt-main-foot { grid-template-columns:minmax(0,1fr) minmax(190px,1.4fr) auto; } }
@media(max-width:850px){ .mlt-staff-tools { flex-wrap:wrap; }.mlt-staff-tools .mlt-search { max-width:none; }.mlt-main-foot { grid-template-columns:1fr auto; }.mlt-main-foot .mlt-labor-page-footer { grid-column:1/-1;grid-row:2; } }
@media(max-width:430px){ .mlt-side-tools { grid-template-columns:minmax(0,1fr) 126px; }.mlt-range-launch { font-size:10px; }.mlt-staff-choice button { padding:7px 6px; } }

/* V25 | Dong bo he mau MeetList cho tab Gio cong, khong thay doi kich thuoc, JS hoac nghiep vu. */
/* Nen va cau truc: trang - xam xanh nhat - tim dam; trang thai tham gia/chu tri giu mau y nghia. */
.meet-panel-labor .mlt-layout { --mlt-theme-primary:#54218d;--mlt-theme-strong:#43176f;--mlt-theme-ink:#334155;--mlt-theme-muted:#718198;--mlt-theme-border:#d7dfe9;--mlt-theme-line:#e7ecf2;--mlt-theme-soft:#f7f9fc;--mlt-theme-hover:#f7f3fc;background:#f6f8fb; }
.meet-panel-labor .mlt-side,.meet-panel-labor .mlt-main { background:#fff;border:1px solid var(--mlt-theme-border);border-radius:12px;box-shadow:0 2px 10px rgba(29,40,65,.035); }
.meet-panel-labor .mlt-side-head { background:var(--mlt-theme-soft);border-bottom:1px solid var(--mlt-theme-border); }
.meet-panel-labor .mlt-side-title { color:var(--mlt-theme-primary);font-size:14.5px;letter-spacing:0;font-weight:800; }
.meet-panel-labor .mlt-side-title > i { color:var(--mlt-theme-primary);font-size:14px; }
.meet-panel-labor .mlt-meeting-count { border-color:#d7c8e9;background:#fff;color:var(--mlt-theme-primary);font-size:11px; }
.meet-panel-labor .mlt-head-range { color:var(--mlt-theme-muted); }
.meet-panel-labor .mlt-search > i { color:#7f8fa3;font-size:13px; }
.meet-panel-labor .mlt-search input { border-color:var(--mlt-theme-border);background:#fff;color:var(--mlt-theme-ink);font-size:12.5px;font-weight:550; }
.meet-panel-labor .mlt-search input::placeholder { color:#93a0b1;opacity:1;font-weight:450; }
.meet-panel-labor .mlt-search input:focus { border-color:#9d78c5;box-shadow:0 0 0 3px rgba(84,33,141,.105); }
.meet-panel-labor .mlt-filter-select { border-color:var(--mlt-theme-border);background:#fff;color:var(--mlt-theme-ink);font-size:12px;font-weight:700; }
.meet-panel-labor .mlt-filter-select:hover { border-color:#bfa8d7; }
.meet-panel-labor .mlt-filter-select:focus-visible { outline:2px solid rgba(84,33,141,.30);outline-offset:1px; }
.meet-panel-labor .mlt-range-launch { border-color:#d8c8e9;background:#fff;color:var(--mlt-theme-primary);font-size:11.5px;font-weight:750; }
.meet-panel-labor .mlt-range-launch:hover { border-color:#b79bd5;background:#f3ecfa; }
.meet-panel-labor .mlt-range-launch.is-active { border-color:#b69ad4;background:#efe7f9;color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-range-launch:focus-visible { outline:2px solid rgba(84,33,141,.30);outline-offset:2px; }
/* Danh sach: dong de doc, nen xen ke rat nhe va the dang chon co diem nhan tim. */
.meet-panel-labor .mlt-meeting-list { background:#fff;scrollbar-color:#c7d1df transparent; }
.meet-panel-labor .mlt-meeting-link { border-left-color:transparent;border-bottom-color:var(--mlt-theme-line);background:#fff;color:var(--mlt-theme-ink); }
.meet-panel-labor .mlt-meeting-link:nth-child(even):not(.is-active) { background:#fbfcfe; }
.meet-panel-labor .mlt-meeting-link:hover:not(.is-active) { background:var(--mlt-theme-hover);color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-meeting-link.is-active { background:#f0eafb;border-left-color:var(--mlt-theme-primary);color:var(--mlt-theme-primary);box-shadow:inset 0 0 0 1px rgba(84,33,141,.025); }
.meet-panel-labor .mlt-meeting-link strong { color:var(--mlt-theme-ink);font-size:13.5px;font-weight:750; }
.meet-panel-labor .mlt-meeting-link:hover strong,.meet-panel-labor .mlt-meeting-link.is-active strong { color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-meeting-link small { color:#73839a;font-size:11.75px; }
.meet-panel-labor .mlt-meeting-link small i { color:#8798ac; }
.meet-panel-labor .mlt-meeting-time-divider { color:#9aa6b6; }
.meet-panel-labor .mlt-meeting-title-badges .mlt-done { border:1px solid #dbe3ed;border-radius:6px;background:#f0f3f7;color:#697a91;font-size:10px;font-weight:700; }
/* Mau chu tri vang am, khong bien toan bo avatar thanh tim va khong thay doi tooltip JS. */
.meet-panel-labor .mlt-meeting-title-badges .mlt-meeting-host { background:#fff6e4;border-color:#f3d493;color:#ad730d; }
.meet-panel-labor .mlt-meeting-title-badges .mlt-meeting-host i,.meet-panel-labor .mlt-person-host-icon { color:#d39511!important; }
.mlt-host-hover-tip { border-color:#edcd87;background:#fff8e9;color:#99640d;box-shadow:0 9px 22px rgba(118,79,14,.14); }
/* Phan trang: mot he nut cho ca hai bang. */
.meet-panel-labor .mlt-page-footer,.meet-panel-labor .mlt-main-foot { background:var(--mlt-theme-soft);border-top-color:var(--mlt-theme-border); }
.meet-panel-labor .mlt-page-info { color:var(--mlt-theme-muted);font-size:11.5px;font-weight:700; }
.meet-panel-labor .mlt-page-button { border-color:#d7dfe9;background:#fff;color:var(--mlt-theme-primary);box-shadow:none; }
.meet-panel-labor .mlt-page-button:hover:not([disabled]):not(.is-active) { border-color:#bba3d7;background:#f3ecfa;color:var(--mlt-theme-strong); }
.meet-panel-labor .mlt-page-button.is-active { background:var(--mlt-theme-primary);border-color:var(--mlt-theme-primary);color:#fff;box-shadow:0 2px 5px rgba(84,33,141,.13); }
.meet-panel-labor .mlt-page-button:focus-visible { outline:2px solid rgba(84,33,141,.35);outline-offset:2px; }
.meet-panel-labor .mlt-page-ellipsis { color:#8190a2; }
/* Khu chi tiet: header trung tinh, tieu de tim va bang nhan vien co vach chia ro. */
.meet-panel-labor .mlt-main-head { background:var(--mlt-theme-soft);border-bottom-color:var(--mlt-theme-border); }
.meet-panel-labor .mlt-main-title { color:var(--mlt-theme-primary);font-size:19px;font-weight:800; }
.meet-panel-labor .mlt-main-time,.meet-panel-labor .mlt-main-meta { color:var(--mlt-theme-muted); }
.meet-panel-labor .mlt-main-time i,.meet-panel-labor .mlt-main-meta i { color:#8797ad; }
.meet-panel-labor .mlt-duration { border-color:#d8c7e9;background:#fff;color:var(--mlt-theme-primary);font-size:11.5px; }
.meet-panel-labor .mlt-staff-choice { border-color:#d7dfe9;background:#fff; }
.meet-panel-labor .mlt-staff-choice button { color:#697c94;font-weight:750; }
.meet-panel-labor .mlt-staff-choice button:hover:not(.is-active):not([aria-pressed='true']) { background:#f3ecfa;color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-staff-choice button.is-active,.meet-panel-labor .mlt-staff-choice button[aria-pressed='true'] { background:var(--mlt-theme-primary)!important;color:#fff!important;box-shadow:0 2px 6px rgba(84,33,141,.13); }
.meet-panel-labor .mlt-table-scroll { background:#fff;scrollbar-color:#c7d1df transparent; }
.meet-panel-labor .mlt-table th { background:#f2f5f9;border-bottom:1px solid var(--mlt-theme-border);border-right:1px solid #e6ecf3;color:var(--mlt-theme-primary);font-size:12.5px;letter-spacing:.05px; }
.meet-panel-labor .mlt-table th:last-child { border-right:0; }
.meet-panel-labor .mlt-table td { border-bottom-color:var(--mlt-theme-line);color:var(--mlt-theme-ink);font-size:13px; }
.meet-panel-labor .mlt-table tbody tr:nth-child(even) > td { background:#fbfcfe; }
.meet-panel-labor .mlt-table tbody tr:hover > td { background:#f9f5fd; }
.meet-panel-labor .mlt-user .mlt-person-avatar { border-color:#d3dce7;box-shadow:0 1px 3px rgba(25,33,50,.06); }
.meet-panel-labor .mlt-person-name { color:var(--mlt-theme-ink);font-size:13.5px;font-weight:750; }
.meet-panel-labor .mlt-user.is-host .mlt-person-name { color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-person-email { color:#8290a4;font-size:11.6px; }
.meet-panel-labor .mlt-theory { color:var(--mlt-theme-primary);font-size:13px;font-weight:800; }
/* Chon tham gia: chi doi palette; radio/checked/disabled van do ASP.NET va JS quan ly. */
.meet-panel-labor .mlt-join { border-color:#d6c7e8;border-radius:8px;background:#fff; }
.meet-panel-labor .mlt-join td { border-right-color:#d6c7e8!important; }
.meet-panel-labor .mlt-join label { color:#74618a;font-size:11.5px;font-weight:750; }
.meet-panel-labor .mlt-join[data-edit='1'] td:not(:has(input:checked)) label:hover { background:#f4effa;color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-join td:has(input:checked) label { background:var(--mlt-theme-primary);color:#fff;font-weight:800; }
.meet-panel-labor .mlt-join td:has(input:focus-visible) { outline-color:#a075cd; }
.meet-panel-labor .mlt-actual { border-color:#d2c2e5;background:#fff;color:var(--mlt-theme-primary);font-weight:750; }
.meet-panel-labor .mlt-actual:hover:not([readonly]) { border-color:#ab86d0; }
.meet-panel-labor .mlt-actual:focus { border-color:#9c72c5;outline:2px solid rgba(84,33,141,.21);outline-offset:1px; }
.meet-panel-labor .mlt-actual[readonly] { border-color:#e0e5ec;background:#f1f3f6;color:#929aa7; }
.meet-panel-labor .mlt-total { color:var(--mlt-theme-muted);font-size:12.5px;font-weight:700; }
.meet-panel-labor .mlt-total b { color:var(--mlt-theme-primary);font-weight:850; }
.meet-panel-labor .mlt-save { background:var(--mlt-theme-primary)!important;border:1px solid var(--mlt-theme-primary)!important;color:#fff!important;border-radius:7px;box-shadow:0 2px 6px rgba(84,33,141,.12);transition:background-color .16s ease,border-color .16s ease,box-shadow .16s ease; }
.meet-panel-labor .mlt-save:hover { background:var(--mlt-theme-strong)!important;border-color:var(--mlt-theme-strong)!important;box-shadow:0 4px 12px rgba(84,33,141,.17); }
.meet-panel-labor .mlt-save:focus-visible { outline:2px solid rgba(84,33,141,.32);outline-offset:2px; }
/* Trang thai va modal he thong: giu do tuong phan, khong doi logic khoa hoac xac nhan. */
.meet-panel-labor .mlt-empty { color:#77869b; }
.meet-panel-labor .mlt-empty i { color:#ad8ecb; }
.meet-panel-labor .mlt-locked-panel { background:linear-gradient(140deg,#f2f5f9,#fafbfd 60%,#eef2f6); }
.meet-panel-labor .mlt-lock-card { border-color:#d9e0e9;background:#fff; }
.meet-panel-labor .mlt-lock-heading { color:var(--mlt-theme-ink); }
.meet-panel-labor .mlt-lock-meeting { color:var(--mlt-theme-primary); }
.meet-panel-labor .mlt-lock-note { color:var(--mlt-theme-muted); }
/* Lich noi va hop xac nhan nam ngoai UpdatePanel: chi dieu chinh mau, vi tri/behavior khong doi. */
.mlt-range-pop { border-color:#d7dfe9;background:#fff;color:#334155;box-shadow:0 20px 46px rgba(31,37,55,.18); }
.mlt-range-head { color:#54218d; }
.mlt-range-close { color:#8492a6; }
.mlt-range-close:hover { color:#54218d; }
.mlt-range-field { border-color:#dce3ed;background:#f8fafc; }
.mlt-range-field.is-active { border-color:#a47fca;background:#f2ebfa;box-shadow:inset 0 0 0 1px rgba(84,33,141,.10); }
.mlt-range-field small { color:#7e8b9f; }
.mlt-range-field strong,.mlt-range-month { color:#54218d; }
.mlt-range-month button { border-color:#d7dfe9;color:#54218d;background:#fff; }
.mlt-range-month button:hover { background:#f2ebfa;border-color:#bea4d8; }
.mlt-range-weekday { color:#8090a4; }
.mlt-range-day { color:#405066; }
.mlt-range-day:hover:not(:disabled) { background:#f3edfa;border-color:#d6c6e8; }
.mlt-range-day.is-between { background:#f2ecfa; }
.mlt-range-day.is-picked { background:#54218d;color:#fff; }
.mlt-range-foot button { border-color:#d7dfe9;color:#54218d;background:#fff; }
.mlt-range-foot button:hover { background:#f3edfa; }
.mlt-range-foot .mlt-range-apply { background:#54218d;border-color:#54218d;color:#fff; }
.mlt-range-foot .mlt-range-apply:hover { background:#43176f;border-color:#43176f; }
.mlt-confirm-box { border-color:#d7dfe9;box-shadow:0 22px 64px rgba(30,29,49,.22); }
.mlt-confirm-title { color:#54218d; }
.mlt-confirm-title i { background:#f2ecfa; }
.mlt-confirm-text { color:#66758c; }
.mlt-confirm-actions button { border-color:#d7dfe9; }
.mlt-confirm-discard { background:#f7f9fc;color:#54218d; }
.mlt-confirm-save { background:#54218d;border-color:#54218d!important;color:#fff; }
.mlt-confirm-save:hover { background:#43176f; }
@media(max-width:780px) {
.meet-panel-labor .mlt-layout { background:#f6f8fb; }
}
@media(prefers-reduced-motion:reduce) {
.meet-panel-labor .mlt-meeting-link,.meet-panel-labor .mlt-save,.meet-panel-labor .mlt-page-button { transition:none!important; }
}


/* Ghi nhận giờ họp thực tế 2026-10-27: compact header, popup giờ thực tế và popup giờ vào/ra không bị che. */
.mlt-main-head { position:relative;display:grid;grid-template-columns:minmax(220px,1fr) auto auto;align-items:center;column-gap:14px;row-gap:12px;padding:13px 16px; }
.mlt-main-head-info { min-width:0; }
.mlt-main-title-line { display:flex;align-items:baseline;flex-wrap:wrap;gap:6px 11px;min-width:0; }
.mlt-main-title { font-size:18px;line-height:1.25; }
.mlt-main-time { color:#8d7a9d;font-size:11.5px;font-weight:600;line-height:1.4;white-space:nowrap; }
.mlt-main-time-tools { position:relative;display:flex;align-items:center;gap:10px;justify-self:end; }
.mlt-time-launch { box-sizing:border-box;display:inline-flex;align-items:center;gap:7px;height:var(--mlt-duration-height,34px);min-height:0;padding:3px 10px;border:1px solid #ccb7e6;border-radius:9px;background:#fff;color:#54218d;line-height:1;cursor:pointer;white-space:nowrap; }
.mlt-time-launch:hover,.mlt-time-launch.is-open { background:#faf6ff;border-color:#bda3df; }
.mlt-time-launch-icon { width:22px;height:22px;border-radius:6px;display:grid;place-items:center;background:#f4ebff;color:#5f2795;flex:0 0 22px;font-size:12px; }
.mlt-time-launch-copy { display:flex;align-items:baseline;gap:8px;white-space:nowrap; }
.mlt-time-launch-copy small { font-size:10.5px;font-weight:800;color:#7f67a0;text-transform:uppercase;letter-spacing:.02em; }
.mlt-time-launch-copy b { font-size:12.5px;font-weight:800;color:#54218d; }
.mlt-time-popover { position:fixed;right:auto;top:0;left:0;z-index:2147480400;display:none;width:min(630px,calc(100vw - 20px));max-width:calc(100vw - 20px);padding:12px;border:1px solid #dfd2ed;border-radius:12px;background:#fff;box-shadow:0 18px 42px rgba(48,23,85,.16);box-sizing:border-box; }
.mlt-time-popover::after { display:none!important;content:none!important; }
.mlt-time-popover.is-open { display:grid;grid-template-columns:1fr 1fr auto;gap:8px;align-items:end; }
.mlt-time-popover-head { grid-column:1/-1;display:flex;align-items:center;justify-content:space-between;gap:8px;color:#572d87;font-size:11px;font-weight:800; }
.mlt-time-popover-close { width:24px;height:24px;border:1px solid #deceed;border-radius:7px;background:#fff;color:#7c60a2;cursor:pointer; }
.mlt-time-field { display:flex;flex-direction:column;gap:4px;min-width:0;color:#6b5586;font-size:10.5px;font-weight:800; }
.mlt-time-field input { box-sizing:border-box;display:block;width:100%;min-width:0;height:34px;padding:6px 8px;border:1px solid #d6c6e9;border-radius:8px;background:#fff;color:#4b2878;font:inherit;font-size:11px;font-weight:700; }
.mlt-time-field input:disabled { background:#f4f2f6;color:#92889e; }
.mlt-time-save { height:34px;display:inline-flex;align-items:center;justify-content:center;gap:5px;padding:0 12px;white-space:nowrap;border:1px solid #bfa7db;border-radius:8px;background:#f3ebfd;color:#54218d;font-size:11px;font-weight:800;text-decoration:none!important; }
.mlt-time-save:hover { background:#e9ddfa; }
.mlt-main-head-side { justify-self:end;display:flex;align-items:center; }
.mlt-duration { margin-top:0; }
.mlt-staff-tools { grid-column:1/-1;display:grid;grid-template-columns:minmax(0,1fr) auto;align-items:center;gap:12px;padding-top:0; }
.mlt-staff-tools .mlt-search { max-width:none; }
.mlt-staff-choice { justify-self:end; }
.mlt-confirm-strip { display:none!important; }
.mlt-confirm-btn { display:inline-flex;align-items:center;gap:6px;min-height:36px;padding:7px 12px;border:1px solid #d6bf94;border-radius:8px;background:#fff8ee;color:#7a530f;font-size:12px;font-weight:800;text-decoration:none!important;white-space:nowrap; }
.mlt-confirm-btn:hover { background:#fff1dc; }
.mlt-actual-cell { position:relative;min-width:156px;overflow:visible; }
.mlt-actual-wrap { display:flex;align-items:center;gap:6px; }
.mlt-actual-wrap .mlt-actual { flex:1 1 auto;width:68px;min-width:0; }
.mlt-clock-toggle { display:grid;place-items:center;flex:0 0 40px;width:40px;height:38px;border:1px solid #d8c5ec;border-radius:10px;background:#faf6ff;color:#6d38a5;cursor:pointer;transition:background .15s,border-color .15s,color .15s; }
.mlt-clock-toggle:hover,.mlt-clock-toggle.is-active { color:#fff;background:#6531a0;border-color:#6531a0; }
.mlt-clock-toggle:disabled { opacity:.42;cursor:default; }
.mlt-time-editor { position:fixed;left:0;top:0;z-index:3000;display:none;width:270px;grid-template-columns:1fr 1fr;gap:7px;padding:10px;border:1px solid #dfd2ed;border-radius:12px;background:#fff;box-shadow:0 18px 42px rgba(48,23,85,.16); }
.mlt-time-editor::after { content:"";position:absolute;right:18px;top:100%;width:12px;height:12px;border-right:1px solid #dfd2ed;border-bottom:1px solid #dfd2ed;background:#fff;transform:translateY(-6px) rotate(45deg); }
.mlt-time-editor.is-below::after { top:auto;bottom:100%;border-right:0;border-bottom:0;border-left:1px solid #dfd2ed;border-top:1px solid #dfd2ed;transform:translateY(6px) rotate(45deg); }
.mlt-time-editor.is-open { display:grid; }
.mlt-time-editor.is-preview input,.mlt-time-editor.is-preview .mlt-time-editor-apply,.mlt-time-editor.is-preview .mlt-time-editor-close { pointer-events:none;opacity:.72; }
.mlt-time-editor-head { grid-column:1/-1;display:flex;align-items:center;justify-content:space-between;gap:8px;color:#572d87;font-size:10.8px;font-weight:800; }
.mlt-time-editor-head b { font-size:11.2px;color:#4f2580; }
.mlt-time-editor-close { width:24px;height:24px;border:1px solid #deceed;border-radius:7px;background:#fff;color:#7c60a2;cursor:pointer; }
.mlt-time-editor label { display:flex;flex-direction:column;gap:4px;color:#6f5489;font-size:10px;font-weight:800; }
.mlt-time-editor input { width:100%;min-width:0;box-sizing:border-box;height:31px;padding:5px 6px;border:1px solid #cdb7e6;border-radius:7px;font-size:11px;color:#4d2876;background:#fff; }
.mlt-time-editor-note { grid-column:1/-1;font-size:9.6px;color:#85738d;line-height:1.45; }
.mlt-time-editor-actions { grid-column:1/-1;display:flex;justify-content:flex-end;gap:6px; }
.mlt-time-editor-apply { min-height:29px;padding:6px 10px;border:1px solid #bb9fdb;border-radius:7px;background:#f3ebfd;color:#54218d;font-size:10.6px;font-weight:800;cursor:pointer; }
.mlt-time-editor-apply:hover { background:#e6d9fb; }
.mlt-accounted-pending { display:inline-flex;align-items:center;gap:4px;padding:2px 6px;border-radius:5px;background:#fff1d4;color:#9b6110;font-size:10px;font-weight:800;white-space:nowrap; }
.mlt-accounted-ok { display:inline-flex;align-items:center;padding:2px 6px;border-radius:5px;background:#e5f6ec;color:#247347;font-size:10px;font-weight:800;white-space:nowrap; }
.mlt-meeting-title-badges { max-width:64%; }
/* Co gon cell khi viewport thap: row height tinh theo noi dung khung, khong cuon do 5 row */
.mlt-table tbody tr.mlt-labor-row > td { box-sizing:border-box;padding-top:3px;padding-bottom:3px; }
.mlt-table tbody tr.mlt-labor-row { height:var(--mlt-equal-row-height,66px); }
.mlt-main-foot { grid-template-columns:minmax(0,1fr) minmax(240px,1fr) auto; }
.mlt-foot-actions { display:inline-flex;align-items:center;justify-content:flex-end;gap:8px;justify-self:end; }
@media(max-width:1250px) { .mlt-main-head { grid-template-columns:minmax(220px,1fr) auto; } .mlt-main-head-side { grid-column:2; } .mlt-staff-tools { grid-column:1/-1; } }
@media(max-width:900px) { .mlt-main-head { grid-template-columns:minmax(0,1fr); } .mlt-main-head-side,.mlt-main-time-tools,.mlt-staff-tools { justify-self:start;grid-column:1; } .mlt-time-popover.is-open { width:min(92vw,360px);grid-template-columns:1fr; } .mlt-staff-tools { grid-template-columns:1fr; } .mlt-staff-choice { justify-self:start; } }
/* Bo chon gio theo style MeetList (24 gio, moc phut), dung cho ca meet va nhan vien. */
.mlt-datetime-control { display:flex;align-items:center;gap:6px;min-width:0; }
.mlt-datetime-raw { position:absolute!important;width:1px!important;height:1px!important;padding:0!important;border:0!important;opacity:0!important;pointer-events:none!important; }
.mlt-datetime-control .mlt-meet-date { flex:1 1 130px;min-width:125px;height:34px;max-width:160px;padding:5px 6px;border:1px solid #d6c6e9;border-radius:7px;background:#fff;color:#54218d;font-size:11px;font-weight:700; }
.mlt-hm-trigger { display:inline-flex;align-items:center;justify-content:center;gap:7px;height:34px;min-width:40px;padding:5px 9px;border:1px solid #cdb9e5;border-radius:8px;background:#f9f6fe;color:#5d2c91;font-size:11px;font-weight:800;white-space:nowrap;cursor:pointer; }
.mlt-hm-trigger:hover,.mlt-hm-trigger:focus-visible { background:#eee5fc;border-color:#a980d8; }
.mlt-hm-trigger:disabled { opacity:.45;cursor:not-allowed; }
.mlt-time-popover { width:min(630px,calc(100vw - 20px)); }
.mlt-time-popover.is-open { display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr) auto;gap:10px;align-items:end; }
.mlt-time-popover .mlt-time-save { grid-column:3;align-self:end;justify-self:end;white-space:nowrap; }
.mlt-time-popover .mlt-datetime-control { display:flex;flex-wrap:nowrap;gap:5px;min-width:0; }
.mlt-time-popover .mlt-datetime-control .mlt-meet-date { flex:1 1 auto;min-width:0;max-width:none; }
.mlt-time-editor .mlt-staff-hour { display:flex;align-items:center;gap:4px;min-width:0; }
.mlt-time-editor .mlt-staff-hour input[type=time] { min-width:0;flex:1 1 68px;cursor:pointer;padding-right:3px; }
.mlt-time-editor .mlt-staff-hour input[type=time]::-webkit-calendar-picker-indicator { display:none; }
.mlt-time-editor .mlt-staff-hour .mlt-hm-trigger { flex:0 0 29px;min-width:29px;width:29px;height:31px;padding:4px; }
.mlt-employee-time-error { display:none;grid-column:1/-1;color:#b42338;font-size:10.5px;font-weight:650;line-height:1.4; }
.mlt-employee-time-error:not(:empty) { display:block; }
.mlt-hm-picker { position:fixed;z-index:2147480500;width:min(470px,calc(100vw - 18px));max-height:calc(100dvh - 18px);overflow:auto;box-sizing:border-box;padding:15px;border:1px solid #dfe4ed;border-radius:17px;background:#fff;color:#475569;box-shadow:0 18px 42px rgba(31,41,55,.18); }
.mlt-hm-picker[hidden] { display:none!important; }
/* Khi chọn giờ thực tế cuộc họp, hai khung nằm nối tiếp nhau, không che nhau. */
.mlt-time-popover.is-picker-stacked { overflow:visible;box-sizing:border-box; }
.mlt-time-popover.is-open { overflow:visible!important;max-height:none!important; }
@media(max-width:640px) { .mlt-time-popover.is-open { grid-template-columns:minmax(0,1fr) minmax(0,1fr); } .mlt-time-popover .mlt-time-save { grid-column:1/-1;justify-self:end; } .mlt-time-popover .mlt-datetime-control { flex-wrap:wrap; } .mlt-time-popover .mlt-datetime-control .mlt-meet-date { min-width:100px; } }

.mlt-main-head:has(.mlt-time-popover.is-picker-stacked) { z-index:2147480400; }
.mlt-hm-picker-head { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:10px; }
.mlt-hm-picker-head strong { color:#334155;font-size:13px;font-weight:800; }
.mlt-hm-picker-head span { color:#94a3b8;font-size:10px;font-weight:650; }
.mlt-hm-picked { display:grid;place-items:center;height:65px;border:1px solid #dfe6ef;border-radius:14px;background:linear-gradient(#f7f9fc,#eef2f7);color:#312e81;font-size:34px;font-weight:850;letter-spacing:.7px;font-variant-numeric:tabular-nums; }
.mlt-hm-picker-caption { margin:12px 0 7px 2px;color:#64748b;font-size:10px;font-weight:800;letter-spacing:.04em; }
.mlt-hm-hour-grid { display:grid;grid-template-columns:repeat(8,minmax(0,1fr));gap:6px; }
.mlt-hm-minute-grid { display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:8px; }
.mlt-hm-option { display:grid;place-items:center;min-width:0;min-height:42px;padding:4px;border:1px solid #dfe5ed;border-radius:10px;background:#fff;color:#53627a;font-size:12px;font-weight:750;cursor:pointer; }
.mlt-hm-option:hover { background:#eef2ff;border-color:#abb8fc;color:#4338ca; }
.mlt-hm-option.is-selected { background:#6366f1;border-color:#6366f1;color:#fff;box-shadow:0 3px 10px rgba(99,102,241,.18); }
.mlt-hm-picker-foot { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-top:13px;padding-top:12px;border-top:1px solid #e6eaf1; }
.mlt-hm-picker-foot span { font-size:10.6px;line-height:1.45;color:#64748b; }
.mlt-hm-picker-foot button { padding:8px 14px;border:1px solid #dfe5ed;border-radius:9px;background:#f8fafc;color:#475569;font-size:11px;font-weight:800;cursor:pointer; }
.mlt-hm-picker-foot button:hover { background:#eef2ff;color:#4338ca; }
@media(max-width:560px) { .mlt-hm-picker { padding:11px; } .mlt-hm-hour-grid { grid-template-columns:repeat(6,minmax(0,1fr));gap:4px; } .mlt-hm-option { min-height:35px; } .mlt-datetime-control { flex-wrap:wrap; } }

/* Popup gio vao/ra: hover chi xem, click moi ghim de sua va luu. */
.mlt-meeting-title-badges { display:inline-flex;align-items:flex-start;gap:5px;flex:0 0 auto;flex-wrap:nowrap;max-width:none; }
.mlt-meeting-title-badges .mlt-meeting-host { align-self:flex-start; }
.mlt-meeting-state-stack { display:flex;flex-direction:column;align-items:flex-end;gap:5px;min-width:0; }
.mlt-meeting-state-stack .mlt-done,.mlt-meeting-state-stack .mlt-accounted-ok,.mlt-meeting-state-stack .mlt-accounted-pending { display:inline-flex;align-items:center;gap:4px;margin:0;white-space:nowrap;font-size:10px;line-height:1.2; }
.mlt-time-editor .mlt-time-editor-actions { display:flex; }
.mlt-time-editor.is-preview .mlt-time-editor-actions,.mlt-time-editor.is-preview .mlt-time-editor-close,.mlt-time-editor:not(.is-pinned) .mlt-time-editor-actions { display:none!important; }
.mlt-time-editor.is-preview .mlt-hm-trigger,.mlt-time-editor.is-preview input[type=time] { pointer-events:none; }
.mlt-time-editor.is-pinned .mlt-time-editor-apply { display:inline-flex;align-items:center;justify-content:center;gap:5px;min-width:62px;opacity:1;pointer-events:auto; }
.mlt-time-editor .mlt-time-editor-head { grid-column:1/-1; }
.mlt-employee-time-error:not(:empty) { display:block; }
.mlt-time-editor .mlt-time-editor-apply.is-saving { opacity:.65;cursor:wait; }

/* Hai dong co dinh cho moi cuoc hop - ten + chu tri + tinh trang / ngay gio + cham cong. */
.meet-panel-labor .mlt-meeting-link { padding:7px 11px;min-height:64px;box-sizing:border-box; }
.meet-panel-labor .mlt-meeting-title-line { display:flex;align-items:center;gap:7px;flex-wrap:nowrap;min-width:0;min-height:24px; }
.meet-panel-labor .mlt-meeting-title-line strong { flex:1 1 auto;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;display:block;font-size:13px;line-height:1.25; }
.meet-panel-labor .mlt-meeting-title-badges { display:inline-flex;align-items:center;gap:5px;flex:0 0 auto;max-width:none;white-space:nowrap; }
.meet-panel-labor .mlt-meeting-subline { display:flex;align-items:center;justify-content:space-between;gap:6px;min-width:0;min-height:23px;margin-top:4px; }
.meet-panel-labor .mlt-meeting-subline small { flex:1 1 auto;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;margin:0;font-size:11.2px;line-height:1.35; }
.meet-panel-labor .mlt-meeting-labor-state { flex:0 0 auto;display:inline-flex;align-items:center;justify-content:flex-end;white-space:nowrap; }
.meet-panel-labor .mlt-meeting-labor-state .mlt-accounted-ok,.meet-panel-labor .mlt-meeting-labor-state .mlt-accounted-pending { padding:2px 5px;font-size:9.5px;line-height:1.2;margin:0; }
.meet-panel-labor .mlt-clock-open { display:inline-grid;place-items:center;flex:0 0 40px;width:40px;height:38px;border:1px solid #d8c5ec;border-radius:10px;background:#faf6ff;color:#6d38a5;text-decoration:none; }
.meet-panel-labor .mlt-clock-open:hover,.meet-panel-labor .mlt-clock-open:focus-visible { color:#fff;background:#6531a0;border-color:#6531a0;text-decoration:none; }
.meet-panel-labor .mlt-clock-open.aspNetDisabled { opacity:.4;pointer-events:none; }
.mlt-employee-modal-body { padding:8px 3px 5px; }
.mlt-modal-employee-name { display:flex;align-items:center;gap:10px;color:#54218d;font-size:17px;font-weight:800; }
.mlt-modal-employee-name i { display:grid;place-items:center;width:34px;height:34px;border-radius:9px;background:#f4eefe; }
.mlt-modal-meeting-range { display:flex;align-items:center;gap:8px;margin-top:10px;color:#64748b;font-size:12px;font-weight:650; }
.mlt-modal-meeting-range i { color:#8d65b4; }
.mlt-modal-hour-fields { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:13px;margin-top:19px; }
.mlt-modal-hour-field { display:flex;flex-direction:column;gap:7px;min-width:0;color:#604484;font-size:12px;font-weight:750; }
.mlt-modal-hour-field input { min-width:0;height:43px;font-size:15px;font-weight:750;color:#502485;border-radius:9px; }
.mlt-modal-hour-result { display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:16px;padding:12px;border:1px solid #e3d6f2;border-radius:9px;background:#faf6ff;color:#695582;font-size:12px; }
.mlt-modal-hour-result strong { color:#54218d;font-size:18px;white-space:nowrap; }
.mlt-modal-hour-error { display:block;margin-top:9px;color:#b42336;font-size:12px; }
.mlt-modal-hour-error:empty { display:none; }
@media(max-width:540px) { .mlt-modal-hour-fields { grid-template-columns:1fr; } .meet-panel-labor .mlt-meeting-subline small { font-size:10.5px; } }

.mlt-modal-time-input { position:relative;display:block;min-width:0;width:100%; }
.mlt-modal-time-input .mlt-modal-clock-text { display:block;width:100%;box-sizing:border-box;padding-right:45px!important;cursor:text;background:#fff;color:#502485;font-variant-numeric:tabular-nums;letter-spacing:.2px;box-shadow:none; }
.mlt-modal-time-input .mlt-modal-clock-text:focus { border-color:#8657c3;box-shadow:0 0 0 3px rgba(112,57,182,.1); }
.mlt-modal-time-input > button { position:absolute;right:6px;top:50%;transform:translateY(-50%);display:grid;place-items:center;width:32px;height:32px;border:0;border-radius:8px;background:transparent;color:#642d9c;cursor:pointer; }
.mlt-modal-time-input > button:hover,.mlt-modal-time-input > button:focus-visible { background:#f1eafa;color:#512183; }
.mlt-modal-clock-text.is-invalid { border-color:#db445f!important;box-shadow:0 0 0 3px rgba(219,68,95,.08)!important; }
.mlt-modal-hour-client-error { display:none;margin-top:8px;color:#b42336;font-size:11px;line-height:1.4; }
.mlt-modal-hour-client-error:not(:empty) { display:block; }
.mlt-modal-time-chooser { margin-top:12px;padding:12px;border:1px solid #e0d5ed;border-radius:12px;background:#fff; }
.mlt-modal-time-chooser[hidden] { display:none!important; }
.mlt-modal-choice-head { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:8px;color:#4d2879;font-size:13px; }
.mlt-modal-choice-head button { width:28px;height:28px;border:1px solid #e2d6ed;border-radius:7px;background:#fff;color:#684592;cursor:pointer; }
.mlt-modal-choice-current { display:grid;place-items:center;min-height:55px;border:1px solid #e2e8f0;border-radius:10px;background:#f3f6fa;color:#38287f;font-size:26px;font-weight:850;font-variant-numeric:tabular-nums; }
.mlt-modal-choice-label { margin:10px 0 6px;color:#68788f;font-size:10px;font-weight:800;letter-spacing:.03em; }
.mlt-modal-choice-hours { display:grid;grid-template-columns:repeat(8,minmax(0,1fr));gap:5px; }
.mlt-modal-choice-minutes { display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:7px; }
.mlt-modal-choice-hours button,.mlt-modal-choice-minutes button { min-width:0;min-height:34px;padding:5px;border:1px solid #dee5ee;border-radius:8px;background:#fff;color:#475569;font-size:11px;font-weight:750;cursor:pointer; }
.mlt-modal-choice-hours button:hover,.mlt-modal-choice-minutes button:hover { border-color:#aaa2ef;background:#f1edfd; }
.mlt-modal-choice-hours button.is-selected,.mlt-modal-choice-minutes button.is-selected { background:#6557ef;color:#fff;border-color:#6557ef; }
.mlt-modal-choice-foot { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-top:12px;padding-top:10px;border-top:1px solid #e4e9ef;color:#79889c;font-size:10px; }
.mlt-modal-choice-foot button { padding:7px 12px;border:1px solid #dee5ee;border-radius:8px;background:#f8fafc;color:#52627c;font-weight:700;cursor:pointer; }
@media(max-width:540px) { .mlt-modal-choice-hours { grid-template-columns:repeat(6,minmax(0,1fr)); } }
/* Spotlight 3D nam tren body, tach hoan toan khoi bang trai de khong gay reflow/overflow. */
.meet-panel-labor.mlt-focus-overlay { position:fixed!important;z-index:2147483000!important;display:block!important;pointer-events:none!important;overflow:visible!important;transform-style:preserve-3d;backface-visibility:hidden;will-change:transform,filter;animation:mltFocusFloat3D 1150ms cubic-bezier(.2,.8,.22,1) both; }
.meet-panel-labor.mlt-focus-overlay > .mlt-meeting-link { display:block!important;box-sizing:border-box;width:100%;height:100%;min-height:0;max-height:100%;overflow:hidden;background:#f2eafd;border:1px solid #d8b6fa;border-left:4px solid #6b35a4;border-radius:9px;box-shadow:0 8px 20px rgba(65,32,108,.15); }
@keyframes mltFocusFloat3D { 0% { transform:perspective(900px) translateZ(0) scale(1) rotateX(0);filter:brightness(1);opacity:1; } 34% { transform:perspective(900px) translateZ(95px) scale(1.115) rotateX(-3deg);filter:brightness(1.055);opacity:1; } 60% { transform:perspective(900px) translateZ(110px) scale(1.12) rotateX(-2deg);filter:brightness(1.065);opacity:1; } 100% { transform:perspective(900px) translateZ(0) scale(1) rotateX(0);filter:brightness(1);opacity:1; } }
@media(prefers-reduced-motion:reduce) { .meet-panel-labor.mlt-focus-overlay { animation:none!important; } }
</style>
<asp:UpdatePanel runat="server" ID="upLaborTab" UpdateMode="Conditional" ChildrenAsTriggers="true">
<ContentTemplate>
    <asp:LinkButton runat="server" ID="btnLoadLaborTab" CausesValidation="false" OnClick="btnLoadLaborTab_Click" Style="display:none">Tải giờ công</asp:LinkButton>
    <input type="hidden" name="QLDA_MeetLabor_FocusId" value="" />
    <input type="hidden" name="QLDA_MeetLabor_SelectId" value="" />
    <input type="hidden" name="QLDA_MeetLabor_Page" value="" />
    <input type="hidden" name="QLDA_MeetLabor_LaborPage" value="" />
    <input type="hidden" name="QLDA_MeetLabor_FilterAction" value="" />
    <input type="hidden" name="QLDA_MeetLabor_FilterValue" value="" />
    <input type="hidden" name="QLDA_MeetLabor_FilterFrom" value="" />
    <input type="hidden" name="QLDA_MeetLabor_FilterTo" value="" />
    <asp:HiddenField ID="hfSaveResult" runat="server" Value="idle" />
    <asp:Label ID="lblTabError" runat="server" CssClass="mlt-error" EnableViewState="false" Visible="false" />
    <div id="laborRoot" runat="server" class="mlt-layout" data-loaded="0">
        <aside class="mlt-side">
            <div class="mlt-side-head">
                <div class="mlt-side-head-top">
                    <span class="mlt-side-title"><i class="far fa-calendar-alt"></i> Danh sách cuộc họp <span class="mlt-meeting-count" aria-label="Tổng số cuộc họp">(<asp:Literal ID="litMeetingCount" runat="server" Text="0" />)</span></span>
                    <button type="button" class="mlt-range-launch" id="mltRangeOpen" aria-haspopup="dialog" aria-label="Lọc cuộc họp theo khoảng ngày"><i class="far fa-calendar-alt" aria-hidden="true"></i><span id="mltRangeOpenLabel">Chọn khoảng ngày</span></button>
                </div>
                <div class="mlt-side-tools">
                    <label class="mlt-search"><i class="fas fa-search" aria-hidden="true"></i><input type="search" id="mltMeetingSearch" placeholder="Tìm tên cuộc họp..." aria-label="Tìm theo tên cuộc họp" autocomplete="off" maxlength="120" /></label>
                    <select id="mltMeetingFilter" class="mlt-filter-select" aria-label="Lọc loại cuộc họp">
                        <option value="all">Tất cả cuộc họp</option><option value="active">Chưa kết thúc</option><option value="ended">Đã kết thúc</option><option value="hosted">Bạn chủ trì</option>
                    </select>
                </div>
                <div class="mlt-filter-internal" aria-hidden="true">
                    <asp:LinkButton ID="btnFilterAll" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="all" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('all');">Tất cả cuộc họp</asp:LinkButton>
                    <asp:LinkButton ID="btnFilterActive" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="active" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('active');">Chưa kết thúc</asp:LinkButton>
                    <asp:LinkButton ID="btnFilterEnded" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="ended" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('ended');" CssClass="is-filtered">Đã kết thúc</asp:LinkButton>
                    <asp:LinkButton ID="btnFilterHosted" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="hosted" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('hosted');"><i class="fas fa-crown" aria-hidden="true"></i> Bạn chủ trì</asp:LinkButton>
                </div>
            </div>
            <div class="mlt-meeting-list">
                <asp:Repeater ID="rptMeetings" runat="server" OnItemCommand="rptMeetings_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="btnSelectMeeting" CommandName="select" CommandArgument='<%# Eval("IdLichHop") %>' data-meet-id='<%# Eval("IdLichHop") %>' CausesValidation="false" CssClass='<%# MeetingLinkCss(Eval("IdLichHop")) %>' OnClientClick='<%# "return QLDAMeetingLaborTab.selectMeeting(\"" + Eval("IdLichHop") + "\");" %>'>
                            <span class="mlt-meeting-title-line"><strong><%#: Eval("TenCuocHop") %></strong><span class="mlt-meeting-title-badges"><%# MeetingHostBadge(Eval("LaChuTri")) %><%# MeetingEndedBadge(Eval("ThoiGianKetThuc")) %></span></span>
                            <span class="mlt-meeting-subline"><small><i class="far fa-clock"></i> <%#: Eval("ThoiGianBatDau", "{0:dd/MM/yyyy}") %> <span class="mlt-meeting-time-divider" aria-hidden="true">·</span> <%#: Eval("ThoiGianBatDau", "{0:HH:mm}") %> – <%#: Eval("ThoiGianKetThuc", "{0:HH:mm}") %></small><span class="mlt-meeting-labor-state"><%# MeetingLaborConfirmBadge(Eval("ThoiGianKetThuc"), Eval("DaXacNhanChamCong")) %></span></span>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel runat="server" ID="pnlNoMeetings" CssClass="mlt-empty" Visible="true"><i class="far fa-calendar-times"></i><span><asp:Literal ID="litNoMeetings" runat="server" Text="Chưa có cuộc họp nào trong dự án." /></span></asp:Panel>
            </div>
            <nav class="mlt-page-footer" aria-label="Phân trang cuộc họp"><asp:Literal ID="litMeetingPager" runat="server" /></nav>
        </aside>
        <section class="mlt-main">
            <asp:Panel ID="pnlLockedMeeting" runat="server" CssClass="mlt-locked-panel" Visible="false">
                <div class="mlt-lock-card" role="status" aria-live="polite">
                    <div class="mlt-lock-symbol"><i class="fas fa-lock" aria-hidden="true"></i><span class="mlt-lock-cross"><i class="fas fa-times" aria-hidden="true"></i></span></div>
                    <strong class="mlt-lock-heading">Chưa thể chấm giờ công</strong>
                    <span class="mlt-lock-meeting"><asp:Literal ID="litLockedMeetingName" runat="server" /></span>
                    <span class="mlt-lock-note">Cuộc họp chưa kết thúc. Giờ công chỉ được chỉnh sửa và lưu sau khi cuộc họp kết thúc.</span>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlSelectedMeeting" runat="server" CssClass="mlt-selected-content" Visible="false">
                <div class="mlt-main-head">
                    <div class="mlt-main-head-info"><div class="mlt-main-title-line"><div class="mlt-main-title"><asp:Literal ID="litMeetName" runat="server" /></div>
                        <span class="mlt-main-time">(<asp:Literal ID="litMeetDate" runat="server" /> · <asp:Literal ID="litMeetTime" runat="server" />)</span></div></div>
                    <div class="mlt-main-time-tools">
                        <button type="button" class="mlt-time-launch" id="mltActualTimeLaunch" aria-expanded="false" aria-haspopup="dialog"><span class="mlt-time-launch-icon"><i class="far fa-calendar-alt"></i></span><span class="mlt-time-launch-copy"><small>Giờ thực tế</small><b><asp:Literal ID="litActualTimeRange" runat="server" /></b></span></button>
                        <div class="mlt-time-popover" id="mltActualTimePopover" role="dialog" aria-label="Thiết lập giờ thực tế cuộc họp"><div class="mlt-time-popover-head"><b>Thiết lập giờ thực tế</b><button type="button" class="mlt-time-popover-close" aria-label="Đóng thiết lập giờ thực tế"><i class="fas fa-times"></i></button></div><label class="mlt-time-field">Bắt đầu thực tế <span class="mlt-datetime-control"><asp:TextBox ID="txtMeetStartActual" runat="server" TextMode="DateTimeLocal" CssClass="mlt-datetime-raw" /><input type="date" class="mlt-meet-date" data-datetime-for="start" aria-label="Ngày bắt đầu thực tế" /><button type="button" class="mlt-hm-trigger mlt-meet-hm-trigger" data-time-for="start" aria-label="Chọn giờ bắt đầu thực tế"><i class="far fa-clock"></i><span class="mlt-hm-display">--:--</span></button></span></label><label class="mlt-time-field">Kết thúc thực tế <span class="mlt-datetime-control"><asp:TextBox ID="txtMeetEndActual" runat="server" TextMode="DateTimeLocal" CssClass="mlt-datetime-raw" /><input type="date" class="mlt-meet-date" data-datetime-for="end" aria-label="Ngày kết thúc thực tế" /><button type="button" class="mlt-hm-trigger mlt-meet-hm-trigger" data-time-for="end" aria-label="Chọn giờ kết thúc thực tế"><i class="far fa-clock"></i><span class="mlt-hm-display">--:--</span></button></span></label><asp:LinkButton ID="btnSaveMeetTimes" runat="server" OnClick="btnSaveMeetTimes_Click" OnClientClick="return QLDAMeetingLaborTab.beforeSecondarySave('btnSaveMeetTimes');" CausesValidation="false" CssClass="mlt-time-save"><i class="fas fa-save"></i> Lưu giờ</asp:LinkButton></div>
                    </div>
                    <div class="mlt-main-head-side"><div class="mlt-duration"><i class="far fa-clock"></i> <asp:Literal ID="litDuration" runat="server" /></div></div>
                    <div class="mlt-staff-tools" role="group" aria-label="Tìm kiếm và lọc nhân viên">
                        <label class="mlt-search"><i class="fas fa-search" aria-hidden="true"></i><input type="search" id="mltStaffSearch" placeholder="Tìm tên hoặc email nhân viên..." aria-label="Tìm nhân viên trong cuộc họp" autocomplete="off" maxlength="120" /></label>
                        <div class="mlt-staff-choice" role="group" aria-label="Lọc trạng thái tham gia">
                            <button type="button" data-mlt-attendance="0" aria-pressed="false">Không tham gia</button>
                            <button type="button" data-mlt-attendance="1" aria-pressed="false">Có tham gia</button>
                        </div>
                    </div>
                </div>
                <div id="divLaborConfirm" runat="server" class="mlt-confirm-strip" aria-hidden="true"><span><i class="fas fa-exclamation-circle"></i> <asp:Literal ID="litLaborConfirm" runat="server" /></span></div>
                <asp:HiddenField ID="hfSelectedMeetId" runat="server" />
                <asp:HiddenField ID="hfDuration" runat="server" />
                <div class="mlt-table-scroll">
                    <table class="mlt-table"><thead><tr><th style="width:35%">Tên nhân viên</th><th style="width:31%">Đã tham gia cuộc họp</th><th style="width:15%">Công lý thuyết</th><th style="width:19%">Công thực tế</th></tr></thead>
                    <tbody><asp:Repeater ID="rptLabor" runat="server" OnItemDataBound="rptLabor_ItemDataBound"><ItemTemplate>
                        <tr class="mlt-labor-row" data-duration="<%# CurrentDurationInvariant %>" data-can-edit="<%# CanEditLabor ? "1" : "0" %>" data-original-joined="<%# (bool)Eval("XacNhanThamGia") ? "1" : "0" %>">
                            <td><asp:HiddenField runat="server" ID="hfUserId" /><span id="personWrap" runat="server" class="mlt-user"><asp:Literal runat="server" ID="litAvatar" /><span class="mlt-person-copy"><span class="mlt-person-name-line"><i id="iconHost" runat="server" class="fas fa-crown mlt-person-host-icon" aria-label="Chủ trì" tabindex="0"></i><asp:Label runat="server" ID="lblName" CssClass="mlt-person-name" /><span id="tagHost" runat="server" class="mlt-host-tag">Chủ trì</span></span><asp:Label runat="server" ID="lblEmail" CssClass="mlt-person-email" /></span></span></td>
                            <td><asp:RadioButtonList runat="server" ID="rblAttendance" CssClass="mlt-join mlt-attendance-select" RepeatDirection="Horizontal" RepeatLayout="Table"><asp:ListItem Value="1">Có tham gia</asp:ListItem><asp:ListItem Value="0">Không tham gia</asp:ListItem></asp:RadioButtonList></td>
                            <td><asp:Label runat="server" ID="lblTheory" CssClass="mlt-theory" /></td>
                            <td class="mlt-actual-cell">
                                <asp:HiddenField ID="hfTimeMode" runat="server" />
                                <div class="mlt-hidden-hour-values" style="display:none"><asp:TextBox ID="txtTimeIn" runat="server" TextMode="Time" /><asp:TextBox ID="txtTimeOut" runat="server" TextMode="Time" /></div>
                                <div class="mlt-actual-wrap">
                                    <asp:TextBox ID="txtActual" runat="server" MaxLength="8" CssClass="mlt-actual" ToolTip="Giờ công thực tế" />
                                    <asp:LinkButton runat="server" ID="btnOpenEmployeeHours" CausesValidation="false" CssClass="mlt-clock-open" CommandArgument='<%# Eval("IdNhanVien") %>' OnClick="btnOpenEmployeeHours_Click" OnClientClick='<%# "return QLDAMeetingLaborTab.beforeEmployeeOpen(\"" + Eval("IdNhanVien") + "\");" %>' ToolTip="Điều chỉnh giờ vào/ra của nhân viên"><i class="far fa-clock" aria-hidden="true"></i></asp:LinkButton>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate></asp:Repeater>
                    <asp:PlaceHolder runat="server" ID="phNoPeople" Visible="false"><tr><td colspan="4"><div class="mlt-empty"><asp:Literal ID="litNoLaborPeople" runat="server" Text="Cuộc họp này chưa có nhân viên." /></div></td></tr></asp:PlaceHolder></tbody></table>
                </div>
                <asp:Label runat="server" ID="lblLaborError" CssClass="mlt-error" EnableViewState="false" />
                <div class="mlt-main-foot">
                    <div class="mlt-total"><span>Người tham gia: <b id="mltJoinedCount"><asp:Literal ID="litJoinedCount" runat="server" /></b></span><span class="mlt-hidden-theory" aria-hidden="true"><b id="mltTheoryTotal"><asp:Literal ID="litTheoryTotal" runat="server" /></b></span></div>
                    <nav class="mlt-labor-page-footer" aria-label="Phân trang nhân viên"><asp:Literal ID="litLaborPager" runat="server" /></nav>
                    <div class="mlt-foot-actions"><asp:LinkButton ID="btnConfirmMeetLabor" runat="server" OnClick="btnConfirmMeetLabor_Click" OnClientClick="return QLDAMeetingLaborTab.beforeSecondarySave('btnConfirmMeetLabor');" CausesValidation="false" CssClass="mlt-confirm-btn"><i class="far fa-check-circle"></i> Xác nhận đã chấm công</asp:LinkButton><asp:LinkButton runat="server" ID="btnSaveLabor" OnClick="btnSaveLabor_Click" CausesValidation="false" CssClass="btn mlt-save"><i class="fas fa-save"></i> Lưu giờ công</asp:LinkButton></div>
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlNoSelected" CssClass="mlt-empty"><i class="far fa-calendar-check"></i><span>Chọn cuộc họp ở bên trái để xem giờ công.</span></asp:Panel>
        </section>
    </div>
</ContentTemplate>
</asp:UpdatePanel>

<%-- Popup ExtraModal độc lập. Không CSS vào khung modal; chỉ CSS nội dung .mlt-employee-modal-body. --%>
<SweetSoft:ExtraModal runat="server" ID="mdlEmployeeHours" Type="Primary" Title="Điều chỉnh giờ vào / ra cuộc họp">
    <ContentTemplate>
        <asp:UpdatePanel runat="server" ID="upEmployeeHours" UpdateMode="Conditional"><ContentTemplate>
            <div class="mlt-employee-modal-body">
                <asp:HiddenField runat="server" ID="hfEmployeeHoursMeetId" />
                <asp:HiddenField runat="server" ID="hfEmployeeHoursUserId" />
                <div class="mlt-modal-employee-name"><i class="far fa-user-circle" aria-hidden="true"></i><asp:Literal runat="server" ID="litEmployeeHoursName" /></div>
                <div class="mlt-modal-meeting-range"><i class="far fa-calendar-alt" aria-hidden="true"></i><asp:Literal runat="server" ID="litEmployeeHoursMeetingTime" /></div>
                <div class="mlt-modal-hour-fields">
                    <label class="mlt-modal-hour-field"><span>Giờ vào thực tế</span><span class="mlt-modal-time-input"><asp:TextBox ID="txtEmployeeHoursIn" runat="server" TextMode="SingleLine" MaxLength="5" CssClass="form-control mlt-modal-clock-text" ToolTip="Nhập HH:mm (ví dụ 09:42) hoặc bấm đồng hồ để chọn" autocomplete="off" inputmode="text" placeholder="HH:mm" /><button type="button" data-modal-hour="in" aria-label="Chọn giờ vào theo 24 giờ" title="Chọn giờ vào"><i class="far fa-clock"></i></button></span></label>
                    <label class="mlt-modal-hour-field"><span>Giờ ra thực tế</span><span class="mlt-modal-time-input"><asp:TextBox ID="txtEmployeeHoursOut" runat="server" TextMode="SingleLine" MaxLength="5" CssClass="form-control mlt-modal-clock-text" ToolTip="Nhập HH:mm (ví dụ 09:42) hoặc bấm đồng hồ để chọn" autocomplete="off" inputmode="text" placeholder="HH:mm" /><button type="button" data-modal-hour="out" aria-label="Chọn giờ ra theo 24 giờ" title="Chọn giờ ra"><i class="far fa-clock"></i></button></span></label>
                </div>
                <div id="mltModalTimeChooser" class="mlt-modal-time-chooser" hidden><div class="mlt-modal-choice-head"><strong>Chọn giờ</strong><button type="button" data-modal-close aria-label="Đóng bộ chọn giờ"><i class="fas fa-times"></i></button></div><div class="mlt-modal-choice-current">--:--</div><div class="mlt-modal-choice-label">GIỜ</div><div class="mlt-modal-choice-hours"></div><div class="mlt-modal-choice-label">PHÚT</div><div class="mlt-modal-choice-minutes"></div><div class="mlt-modal-choice-foot"><span>Có thể nhập phút bất kỳ trực tiếp trong ô giờ.</span><button type="button" data-modal-close>Xong</button></div></div>
                <div class="mlt-modal-hour-result"><span>Công thực tế sẽ ghi nhận</span><strong id="mltEmployeeHoursPreview">—</strong></div>
                <span class="mlt-modal-hour-client-error" id="mltEmployeeHoursClientError" role="alert" aria-live="polite"></span><asp:Label runat="server" ID="lblEmployeeHoursError" CssClass="mlt-modal-hour-error" EnableViewState="false" />
            </div>
        </ContentTemplate></asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:LinkButton runat="server" ID="btnSaveEmployeeHours" CausesValidation="false" CssClass="btn btn-primary" OnClick="btnSaveEmployeeHours_Click"><i class="fas fa-save" aria-hidden="true"></i> Lưu</asp:LinkButton>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    (function () {
        if (window.QLDAMeetingLaborTab) return;
        var baseline = null, baselineRows = null, lastRenderedPane = null, lastMeetingId = null;
        var pendingAfterSave = null, savingForNavigation = false, bypassGuard = false;
        var pinnedTimeRow = null, hoverTimeRow = null, hoverTimeTimer = null, timePopoverPinned = false, timePopupSubmitting = false;
        var activeHourPicker = null, pickerValue = {hour:0,minute:0}, pickerFinish = null, pickerStackedForMeet = false;
        var dialog = null;
        var filterTimer = null, focusSearchAfterReload = null;
        var rangePopup = null, rangeMonth = null, rangeTarget = 'from', draftFrom = '', draftTo = '';

        function root() { return document.querySelector('.meet-panel-labor'); }
        // Server dat bo loc vao laborRoot (ben trong .meet-panel-labor), khong phai wrapper ngoai.
        function filterStateNode() { var r = root(); return r && (r.querySelector('.mlt-layout[data-staff-mode]') || r); }
        function laborActive() { return !!document.querySelector('.meet-view-shell.meet-mode-labor'); }
        function rows() { var r = root(); return r ? r.querySelectorAll('.mlt-labor-row') : []; }
        function currentMeetingId() { var r = root(), h = r && r.querySelector('input[id$="_hfSelectedMeetId"]'); return h ? h.value : ''; }
        function editable() { var rr = rows(); return laborActive() && rr.length > 0 && rr[0].getAttribute('data-can-edit') === '1'; }
        function timeLaunch() { var r = root(); return r && r.querySelector('#mltActualTimeLaunch'); }
        function timePopover() { var r = root(); return r && r.querySelector('#mltActualTimePopover'); }
        function positionMeetTimePopover() {
            var pop = timePopover(), launch = timeLaunch();
            if (!pop || !launch || !pop.classList.contains('is-open') || pickerStackedForMeet) return;
            var pad = 10, gap = 10;
            var vw = window.innerWidth || document.documentElement.clientWidth;
            var vh = window.innerHeight || document.documentElement.clientHeight;
            pop.style.position = 'fixed';
            pop.style.right = 'auto';
            pop.style.bottom = 'auto';
            // Popup thiet lap luon giu chieu cao tu nhien, khong co thanh cuon doc.
            pop.style.removeProperty('max-height');
            pop.style.removeProperty('overflow-y');
            var rect = launch.getBoundingClientRect();
            var width = pop.getBoundingClientRect().width;
            var height = pop.getBoundingClientRect().height;
            // Canh canh phai popup voi canh phai nut de popup nam ben tren va lech trai.
            var left = Math.max(pad, Math.min(vw - width - pad, rect.right - width));
            var idealTop = rect.top - height - gap;
            var top = Math.max(pad, Math.min(vh - height - pad, idealTop));
            pop.classList.toggle('is-clamped', top + height > rect.top - 2);
            pop.style.left = Math.round(left) + 'px';
            pop.style.top = Math.round(top) + 'px';
        }
        function closeMeetTimePopover() {
            var btn = timeLaunch(), pop = timePopover();
            if (pickerStackedForMeet) closeHourPicker();
            timePopoverPinned = false;
            if (btn) { btn.classList.remove('is-open'); btn.setAttribute('aria-expanded', 'false'); }
            if (pop) {
                pop.classList.remove('is-open', 'is-clamped', 'is-picker-stacked');
                ['position','top','left','right','bottom','max-height','overflow-y','z-index'].forEach(function (name) { pop.style.removeProperty(name); });
            }
        }
        function openMeetTimePopover() {
            var btn = timeLaunch(), pop = timePopover();
            if (!btn || !pop) return;
            timePopoverPinned = true;
            btn.classList.add('is-open');
            btn.setAttribute('aria-expanded', 'true');
            pop.classList.add('is-open');
            positionMeetTimePopover();
        }
        function normalizeHours(v) {
            var s = String(v == null ? '' : v).trim().replace(',', '.');
            if (!s) return '';
            var n = Number(s);
            return isFinite(n) ? String(n) : s;
        }
        function readRows() {
            var result=[];
            Array.prototype.forEach.call(rows(),function(row){
                var id=row.querySelector('input[id$="_hfUserId"]'), yes=row.querySelector('input[type="radio"][value="1"]'), actual=row.querySelector('.mlt-actual');
                var joined=!!(yes && yes.checked);
                var mode=row.querySelector('input[id$="_hfTimeMode"]'), tin=row.querySelector('input[id$="_txtTimeIn"]'), tout=row.querySelector('input[id$="_txtTimeOut"]');
                result.push({id:id?id.value:'',joined:joined?'1':'0',raw:actual?actual.value:'',hours:joined?normalizeHours(actual&&actual.value):'0',mode:mode?mode.value:'default',tin:tin?tin.value:'',tout:tout?tout.value:''});
            });
            return result;
        }
        function compact(v) { return JSON.stringify(v.map(function(i){return [i.id,i.joined,i.hours,i.mode,i.tin,i.tout];})); }
        function setBaseline() { baselineRows=readRows();baseline=compact(baselineRows);lastRenderedPane=root()&&root().querySelector('.mlt-selected-content');lastMeetingId=currentMeetingId(); }
        function dirty() { return editable() && baseline!==null && currentMeetingId()===lastMeetingId && compact(readRows())!==baseline; }
        function fmt(v) { v=Math.round((Number(v)||0)*100)/100; return v.toLocaleString('vi-VN',{maximumFractionDigits:2})+' giờ'; }
        function syncRow(row) {
            var yes=row.querySelector('input[type="radio"][value="1"]'), joined=!!(yes&&yes.checked), isEditable=row.getAttribute('data-can-edit')==='1';
            var table=row.querySelector('.mlt-join'), theory=row.querySelector('.mlt-theory'), actual=row.querySelector('.mlt-actual');
            var duration=Number(row.getAttribute('data-duration')||0);
            if(table)table.setAttribute('data-edit',isEditable?'1':'0');
            if(theory)theory.textContent=fmt(joined?duration:0);
            var clock=row.querySelector('.mlt-clock-toggle');
            if(actual){
                actual.readOnly=!isEditable||!joined;
                if(!joined)actual.value='0';
                else {
                    var selectedMode=row.querySelector('input[id$="_hfTimeMode"]');
                    if(selectedMode&&selectedMode.value==='default'&&(!actual.value||Number(actual.value.replace(',','.'))===0))actual.value=String(duration).replace('.',',');
                }
            }
            if(clock)clock.disabled=!isEditable||!joined;
            var tin=row.querySelector('input[id$="_txtTimeIn"]'),tout=row.querySelector('input[id$="_txtTimeOut"]');
            if(tin)tin.disabled=!isEditable||!joined;
            if(tout)tout.disabled=!isEditable||!joined;
            if(!joined||!isEditable)closeTimeEditor(row,true);
        }
        function update() {
            var r = root(); if (!r) return;
            // Tổng server là của toàn cuộc họp, không phải chỉ của 5 người ở trang hiện tại.
            var panel = r.querySelector('.mlt-selected-content');
            var joined = panel ? Number(panel.getAttribute('data-total-joined') || 0) : 0;
            var total = panel ? Number(panel.getAttribute('data-total-theory') || 0) : 0;
            Array.prototype.forEach.call(rows(), function (row) {
                syncRow(row);
                var yes = row.querySelector('input[type="radio"][value="1"]');
                var now = (yes && yes.checked) ? 1 : 0;
                var original = row.getAttribute('data-original-joined') === '1' ? 1 : 0;
                var delta = now - original;
                joined += delta;
                total += delta * Number(row.getAttribute('data-duration') || 0);
            });
            var count = r.querySelector('#mltJoinedCount'), sum = r.querySelector('#mltTheoryTotal');
            if (count) count.textContent = String(joined); if (sum) sum.textContent = fmt(total);
        }
        function discardChanges(){
            if(!baselineRows)return;
            var originals={};baselineRows.forEach(function(x){originals[x.id]=x;});
            Array.prototype.forEach.call(rows(),function(row){
                var id=row.querySelector('input[id$="_hfUserId"]');var old=id&&originals[id.value];if(!old)return;
                var input=row.querySelector('input[type="radio"][value="'+old.joined+'"]');if(input)input.checked=true;
                var actual=row.querySelector('.mlt-actual');if(actual)actual.value=old.raw;
                var mode=row.querySelector('input[id$="_hfTimeMode"]'),tin=row.querySelector('input[id$="_txtTimeIn"]'),tout=row.querySelector('input[id$="_txtTimeOut"]');
                if(mode)mode.value=old.mode;if(tin)tin.value=old.tin;if(tout)tout.value=old.tout;
                var editor=row.querySelector('.mlt-time-editor'),clock=row.querySelector('.mlt-clock-toggle');
                if(editor)editor.classList.remove('is-open');if(clock){clock.classList.toggle('is-active',old.mode==='time');clock.setAttribute('aria-expanded','false');}
            });
            update();
        }
        function createDialog() {
            if (dialog && document.body.contains(dialog)) return dialog;
            dialog = document.createElement('div'); dialog.className = 'mlt-confirm-backdrop'; dialog.style.display = 'none';
            dialog.setAttribute('role', 'presentation');
            dialog.innerHTML = '<div class="mlt-confirm-box" role="dialog" aria-modal="true" aria-label="Xác nhận lưu giờ công">' +
                '<div class="mlt-confirm-title"><i class="far fa-save" aria-hidden="true"></i>Thay đổi chưa được lưu</div>' +
                '<div class="mlt-confirm-text">Bạn đã thay đổi thông tin chấm giờ công. Bạn có muốn lưu lại trước khi tiếp tục không?</div>' +
                '<div class="mlt-confirm-actions">' +
                '<button type="button" class="mlt-confirm-cancel">Ở lại</button>' +
                '<button type="button" class="mlt-confirm-discard">Không lưu</button>' +
                '<button type="button" class="mlt-confirm-save">Lưu và tiếp tục</button></div></div>';
            document.body.appendChild(dialog);
            dialog.querySelector('.mlt-confirm-cancel').addEventListener('click', closeDialog);
            dialog.querySelector('.mlt-confirm-discard').addEventListener('click', function () { var action = dialog._action; closeDialog(); discardChanges(); dispatch(action); });
            dialog.querySelector('.mlt-confirm-save').addEventListener('click', function () {
                var action = dialog._action;
                var r = root(), save = r && r.querySelector('a[id$="_btnSaveLabor"]');
                if (!save) return;
                pendingAfterSave = action; savingForNavigation = true;
                dialog.querySelector('.mlt-confirm-save').disabled = true;
                dialog.querySelector('.mlt-confirm-discard').disabled = true;
                dialog.querySelector('.mlt-confirm-cancel').disabled = true;
                save.click();
            });
            dialog.addEventListener('click', function (e) { if (e.target === dialog && !savingForNavigation) closeDialog(); });
            return dialog;
        }
        function closeDialog() { if (!dialog) return; dialog.style.display = 'none'; dialog._action = null; }
        function ask(action) {
            var d = createDialog(); d._action = action;
            Array.prototype.forEach.call(d.querySelectorAll('button'), function (b) { b.disabled = false; });
            d.style.display = 'flex'; d.querySelector('.mlt-confirm-save').focus();
        }
        function dispatch(action) {
            if (!action) return;
            bypassGuard = true;
            try {
                if (action.type === 'meeting') {
                    var r = root(), target = r && r.querySelector('input[name="QLDA_MeetLabor_SelectId"]'), page = r && r.querySelector('input[name="QLDA_MeetLabor_Page"]');
                    var button = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (target && button) { target.value = action.id; if (page) page.value = ''; button.click(); }
                } else if (action.type === 'focus') {
                    var rr = root(), target = rr && rr.querySelector('input[name="QLDA_MeetLabor_FocusId"]');
                    var loadButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (!rr || !target || !loadButton) return;
                    var state = filterStateNode();
                    var alreadyLoaded = state && state.getAttribute('data-loaded') === '1';
                    target.value = action.id;
                    // Khong mo hai AJAX request dong thoi: neu tab chua tai,
                    // changeView se tu kich hoat chinh nut btnLoadLaborTab.
                    if (window.SweetMeetCalendar && typeof window.SweetMeetCalendar.changeView === 'function')
                        window.SweetMeetCalendar.changeView('labor', true);
                    if (alreadyLoaded) loadButton.click();
                } else if (action.type === 'page') {
                    var r2 = root(), pageTarget = r2 && r2.querySelector('input[name="QLDA_MeetLabor_Page"]'), selected = r2 && r2.querySelector('input[name="QLDA_MeetLabor_SelectId"]');
                    var pageButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (pageTarget && pageButton) { pageTarget.value = String(action.page); if (selected) selected.value = ''; pageButton.click(); }
                } else if (action.type === 'employeeOpen') {
                    var targetRow = Array.prototype.find.call(rows(),function(row){var field=row.querySelector('input[id$="_hfUserId"]');return field && field.value.toLowerCase()===String(action.userId).toLowerCase();});
                    var actionLink = targetRow && targetRow.querySelector('a[id$="_btnOpenEmployeeHours"]');
                    if(actionLink) actionLink.click();
                } else if (action.type === 'laborPage') {
                    var r3 = root(), laborPageTarget = r3 && r3.querySelector('input[name="QLDA_MeetLabor_LaborPage"]');
                    var laborPageButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (laborPageTarget && laborPageButton) { laborPageTarget.value = String(action.page); laborPageButton.click(); }
                } else if (action.type === 'secondary') {
                    var rr = root();
                    if (rr && action.start !== undefined && action.end !== undefined) {
                        var startField = rr.querySelector('input[id$="_txtMeetStartActual"]');
                        var endField = rr.querySelector('input[id$="_txtMeetEndActual"]');
                        if (startField) startField.value = action.start;
                        if (endField) endField.value = action.end;
                    }
                    var secondary = rr && rr.querySelector('a[id$="_' + action.button + '"]');
                    if (secondary) secondary.click();
                } else if (action.type === 'filterRequest') {
                    var r4 = root(), filterBtn = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (!r4 || !filterBtn) return;
                    function hidden(name) { return r4.querySelector('input[name="QLDA_MeetLabor_' + name + '"]'); }
                    hidden('FilterAction').value = action.filterType;
                    hidden('FilterValue').value = action.value == null ? '' : String(action.value);
                    hidden('FilterFrom').value = action.from || '';
                    hidden('FilterTo').value = action.to || '';
                    focusSearchAfterReload = action.filterType === 'meetingSearch' ? 'mltMeetingSearch'
                        : action.filterType === 'staffSearch' ? 'mltStaffSearch' : null;
                    filterBtn.click();
                } else if (action.type === 'filter') {
                    var selectors = { all:'<%= btnFilterAll.ClientID %>', active:'<%= btnFilterActive.ClientID %>', ended:'<%= btnFilterEnded.ClientID %>', hosted:'<%= HostedFilterClientId %>' };
                    var filter = document.getElementById(selectors[action.mode]); if (filter) filter.click();
                } else if (action.type === 'view') {
                    if (window.SweetMeetCalendar && typeof window.SweetMeetCalendar.changeView === 'function') window.SweetMeetCalendar.changeView(action.view);
                } else if (action.type === 'url') {
                    window.location.assign(action.href);
                } else if (action.type === 'refresh') {
                    // GET, not a replay of the ASP.NET POST.
                    window.location.replace(window.location.pathname + window.location.search + window.location.hash);
                }
            } finally { bypassGuard = false; }
        }
        function navigate(action) { if (bypassGuard || !dirty()) { dispatch(action); return; } ask(action); }
        function selectMeeting(id) { navigate({ type: 'meeting', id: id }); return false; }
        function focusMeeting(id) {
            if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id || '')) return false;
            navigate({ type: 'focus', id: id });
            return false;
        }
        function changePage(page) {
            page = Number(page);
            if (!isFinite(page) || Math.floor(page) !== page || page < 1) return false;
            navigate({ type: 'page', page: page }); return false;
        }
        function changeLaborPage(page) {
            page = Number(page);
            if (!isFinite(page) || Math.floor(page) !== page || page < 1) return false;
            navigate({ type: 'laborPage', page: page }); return false;
        }
        function beforeFilter(mode) { if (bypassGuard) return true; if (!dirty()) return true; ask({ type: 'filter', mode: mode }); return false; }
        function beforeEmployeeOpen(userId) { if (bypassGuard || !dirty()) return true; ask({type:'employeeOpen',userId:userId}); return false; }
        function beforeSecondarySave(button) {
            if (bypassGuard || !dirty()) return true;
            var r = root(), a = r && r.querySelector('input[id$="_txtMeetStartActual"]'), b = r && r.querySelector('input[id$="_txtMeetEndActual"]');
            ask({ type: 'secondary', button: button, start: a ? a.value : '', end: b ? b.value : '' });
            return false;
        }
        function sendFilter(filterType, value, from, to) {
            if (filterTimer) { clearTimeout(filterTimer); filterTimer = null; }
            navigate({ type: 'filterRequest', filterType: filterType, value: value == null ? '' : String(value), from: from || '', to: to || '' });
        }
        function dateISO(d) {
            return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
        }
        function isoDate(s) {
            if (!/^\d{4}-\d{2}-\d{2}$/.test(s || '')) return null;
            var p = s.split('-').map(Number), d = new Date(p[0], p[1] - 1, p[2]);
            return d.getFullYear() === p[0] && d.getMonth() === p[1] - 1 && d.getDate() === p[2] ? d : null;
        }
        function prettyDate(s) { var d = isoDate(s); return d ? String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') + '/' + d.getFullYear() : 'Chọn ngày'; }
        function prettyShort(s) { var d = isoDate(s); return d ? String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') : ''; }
        function ensureCalendar() {
            if (rangePopup && document.body.contains(rangePopup)) return rangePopup;
            rangePopup = document.createElement('div');
            rangePopup.className = 'mlt-range-pop'; rangePopup.hidden = true;
            rangePopup.setAttribute('role', 'dialog'); rangePopup.setAttribute('aria-label', 'Lọc cuộc họp theo khoảng ngày');
            rangePopup.innerHTML = '<div class="mlt-range-head"><span><i class="far fa-calendar-alt"></i> Khoảng thời gian cuộc họp</span><button type="button" class="mlt-range-close" aria-label="Đóng lịch">×</button></div>' +
                '<div class="mlt-range-fields"><button type="button" class="mlt-range-field" data-target="from"><small>Từ ngày</small><strong>Chọn ngày</strong></button><button type="button" class="mlt-range-field" data-target="to"><small>Đến ngày</small><strong>Chọn ngày</strong></button></div>' +
                '<div class="mlt-range-month"><button type="button" data-month="-1" aria-label="Tháng trước">‹</button><span id="mltDateMonth"></span><button type="button" data-month="1" aria-label="Tháng sau">›</button></div>' +
                '<div class="mlt-range-days" id="mltDateDays"></div><div class="mlt-range-error" aria-live="polite"></div>' +
                '<div class="mlt-range-foot"><button type="button" data-range-clear>Xóa lọc</button><button type="button" class="mlt-range-apply" data-range-apply>Áp dụng</button></div>';
            document.body.appendChild(rangePopup);
            rangePopup.addEventListener('click', function (e) {
                // Ngày được chọn sẽ bị paintCalendar() thay DOM ngay trong click.
                // Nếu sự kiện tiếp tục nổi bọt đến document, bộ xử lý click ngoài
                // sẽ thấy target đã rời DOM và đóng lịch nhầm.
                e.stopPropagation();
                var t = e.target.closest('button'); if (!t || !rangePopup.contains(t)) return;
                if (t.classList.contains('mlt-range-close')) { hideCalendar(); return; }
                if (t.hasAttribute('data-target')) { rangeTarget = t.getAttribute('data-target'); paintCalendar(); return; }
                if (t.hasAttribute('data-month')) { rangeMonth.setMonth(rangeMonth.getMonth() + Number(t.getAttribute('data-month'))); paintCalendar(); return; }
                if (t.hasAttribute('data-date')) {
                    var val = t.getAttribute('data-date');
                    if (rangeTarget === 'from') { draftFrom = val; if (draftTo && draftTo < val) draftTo = ''; }
                    else if (!draftFrom || val >= draftFrom) draftTo = val;
                    paintCalendar(); return;
                }
                if (t.hasAttribute('data-range-clear')) { hideCalendar(); sendFilter('clearRange', ''); return; }
                if (t.hasAttribute('data-range-apply')) {
                    var err = rangePopup.querySelector('.mlt-range-error');
                    if (!draftFrom || !draftTo) { err.textContent = 'Vui lòng chọn cả Từ ngày và Đến ngày.'; return; }
                    if (draftTo < draftFrom) { err.textContent = 'Đến ngày không được trước Từ ngày.'; return; }
                    hideCalendar(); sendFilter('range', '', draftFrom, draftTo);
                }
            });
            return rangePopup;
        }
        function positionCalendar() {
            if (!rangePopup || rangePopup.hidden) return;
            var r = root(), anchor = r && r.querySelector('#mltRangeOpen'); if (!anchor) { hideCalendar(); return; }
            var rect = anchor.getBoundingClientRect();
            var w = rangePopup.offsetWidth, h = rangePopup.offsetHeight, gap = 10, pad = 10;
            // Mac dinh bung sang PHAI nut loc, khong nam phia duoi nut.
            // Chi doi sang trai neu khong du cho ngang (man hinh nho).
            var left = rect.right + gap;
            if (left + w > window.innerWidth - pad) {
                var alt = rect.left - w - gap;
                left = alt >= pad ? alt : Math.max(pad, window.innerWidth - w - pad);
            }
            // Canh theo dinh nut. Neu dung day/tran man hinh thi day len/ha xuong.
            var top = Math.max(pad, Math.min(rect.top, window.innerHeight - h - pad));
            rangePopup.style.left = Math.round(left) + 'px';
            rangePopup.style.top = Math.round(top) + 'px';
        }
        function hideCalendar() { if (rangePopup) rangePopup.hidden = true; }
        function openCalendar() {
            var r = root(); if (!r) return;
            var el = ensureCalendar();
            var stateNode = filterStateNode();
            draftFrom = stateNode && stateNode.getAttribute('data-date-from') || '';
            draftTo = stateNode && stateNode.getAttribute('data-date-to') || '';
            rangeTarget = 'from';
            var d = isoDate(draftFrom) || new Date(); rangeMonth = new Date(d.getFullYear(), d.getMonth(), 1);
            el.hidden = false; paintCalendar(); positionCalendar();
        }
        function paintCalendar() {
            var p = ensureCalendar(); if (!rangeMonth) return;
            p.querySelector('[data-target="from"] strong').textContent = prettyDate(draftFrom);
            p.querySelector('[data-target="to"] strong').textContent = prettyDate(draftTo);
            Array.prototype.forEach.call(p.querySelectorAll('.mlt-range-field'), function (field) { field.classList.toggle('is-active', field.getAttribute('data-target') === rangeTarget); });
            p.querySelector('#mltDateMonth').textContent = 'Tháng ' + (rangeMonth.getMonth() + 1) + '/' + rangeMonth.getFullYear();
            var days = p.querySelector('#mltDateDays'); days.textContent = '';
            ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].forEach(function (x) { var a = document.createElement('span'); a.className = 'mlt-range-weekday'; a.textContent = x; days.appendChild(a); });
            var offset = (rangeMonth.getDay() + 6) % 7;
            for (var i = 0; i < 42; i++) {
                var d = new Date(rangeMonth.getFullYear(), rangeMonth.getMonth(), 1 - offset + i), val = dateISO(d);
                var btn = document.createElement('button'); btn.type = 'button'; btn.className = 'mlt-range-day'; btn.textContent = d.getDate(); btn.setAttribute('data-date', val);
                if (d.getMonth() !== rangeMonth.getMonth()) btn.classList.add('is-out');
                if (draftFrom && draftTo && val >= draftFrom && val <= draftTo) btn.classList.add('is-between');
                if (val === draftFrom || val === draftTo) btn.classList.add('is-picked');
                if (rangeTarget === 'to' && draftFrom && val < draftFrom) { btn.disabled = true; btn.title = 'Đến ngày không được trước Từ ngày'; }
                days.appendChild(btn);
            }
            p.querySelector('.mlt-range-error').textContent = ''; positionCalendar();
        }
        function syncFilterControls() {
            var r = root(), state = filterStateNode(); if (!r || !state) return;
            var search = r.querySelector('#mltMeetingSearch'), staff = r.querySelector('#mltStaffSearch'), select = r.querySelector('#mltMeetingFilter');
            if (search && document.activeElement !== search) search.value = state.getAttribute('data-meeting-search') || '';
            if (staff && document.activeElement !== staff) staff.value = state.getAttribute('data-staff-search') || '';
            if (select) select.value = state.getAttribute('data-meeting-mode') || 'ended';
            var v = state.getAttribute('data-staff-mode') || '-1';
            r.querySelectorAll('[data-mlt-attendance]').forEach(function (b) { var active = b.getAttribute('data-mlt-attendance') === v; b.classList.toggle('is-active', active); b.setAttribute('aria-pressed', active ? 'true' : 'false'); });
            var from = state.getAttribute('data-date-from'), to = state.getAttribute('data-date-to'), open = r.querySelector('#mltRangeOpen'), label = r.querySelector('#mltRangeOpenLabel');
            if (label) label.textContent = from && to ? prettyShort(from) + ' – ' + prettyShort(to) : 'Chọn khoảng ngày';
            if (open) { open.classList.toggle('is-active', !!(from && to)); open.title = from && to ? prettyDate(from) + ' – ' + prettyDate(to) : 'Lọc theo khoảng thời gian'; }
            if (focusSearchAfterReload) { var target = r.querySelector('#' + focusSearchAfterReload); focusSearchAfterReload = null; if (target) { target.focus(); var pos = target.value.length; try { target.setSelectionRange(pos, pos); } catch (ignore) { } } }
        }
        document.addEventListener('click', function (e) {
            var t = e.target.closest && e.target.closest('#mltRangeOpen'); if (t) { if (rangePopup && !rangePopup.hidden) hideCalendar(); else openCalendar(); return; }
            var staff = e.target.closest && e.target.closest('.meet-panel-labor [data-mlt-attendance]');
            if (staff) {
                var r = root(), v = staff.getAttribute('data-mlt-attendance');
                if (staff.classList.contains('is-active')) v = '-1';
                // Nhap nhanh trang thai duoc chon, nhung khong thay doi giao dien
                // neu con du lieu gio cong chua luu (cho hop xac nhan xu ly).
                if (!dirty() && r) {
                    r.querySelectorAll('[data-mlt-attendance]').forEach(function (b) {
                        var selected = b.getAttribute('data-mlt-attendance') === v;
                        b.classList.toggle('is-active', selected);
                        b.setAttribute('aria-pressed', selected ? 'true' : 'false');
                    });
                }
                sendFilter('attendance', v); return;
            }
            if (rangePopup && !rangePopup.hidden && !rangePopup.contains(e.target)) hideCalendar();
        });
        document.addEventListener('change', function (e) {
            if (e.target.id === 'mltMeetingFilter') { sendFilter('meetingMode', e.target.value); }
        });
        document.addEventListener('input', function (e) {
            var type = e.target.id === 'mltMeetingSearch' ? 'meetingSearch' : e.target.id === 'mltStaffSearch' ? 'staffSearch' : null;
            if (!type) return;
            var text = e.target.value; clearTimeout(filterTimer);
            filterTimer = setTimeout(function () { var state = filterStateNode(), server = state && state.getAttribute(type === 'meetingSearch' ? 'data-meeting-search' : 'data-staff-search') || ''; if (text.trim() !== server) sendFilter(type, text); }, 530);
        });
        document.addEventListener('keydown', function (e) {
            if ((e.key === 'Enter') && (e.target.id === 'mltMeetingSearch' || e.target.id === 'mltStaffSearch')) {
                e.preventDefault(); clearTimeout(filterTimer); sendFilter(e.target.id === 'mltMeetingSearch' ? 'meetingSearch' : 'staffSearch', e.target.value);
            }
            if (e.key === 'Escape' && rangePopup && !rangePopup.hidden) hideCalendar();
        });
        window.addEventListener('resize', positionCalendar, { passive: true });
        window.addEventListener('scroll', function () { if (rangePopup && !rangePopup.hidden) positionCalendar(); }, true);

        function syncPageRanges() { /* Khoảng phân trang nay hiển thị trực tiếp trong footer. */ }
        var observedLaborViewport = null;
        var equalRowObserver = window.ResizeObserver ? new ResizeObserver(syncEqualRowHeight) : null;
        // Can chinh theo chieu cao that cua viewport, thead va 5 dong, sau khi AJAX/tab duoc render.
        // Chi cho scroll neu man hinh qua thap de hien thi day du noi dung toi thieu cua 5 dong.
        function syncEqualRowHeight() {
            var r = root(), viewport = r && r.querySelector('.mlt-table-scroll'), table = viewport && viewport.querySelector('.mlt-table');
            if (!viewport || !table || viewport.clientHeight < 50) return;
            var head = table.querySelector('thead'), body = table.querySelector('tbody');
            var rowNodes = body && body.querySelectorAll('tr.mlt-labor-row');
            if (!rowNodes || !rowNodes.length) return;
            var headerHeight = head ? Math.ceil(head.getBoundingClientRect().height) : 0;
            // Du mot trang co it hon 5 nhan vien, chieu cao hang van lay theo trang du 5 nguoi.
            var available = Math.max(0, viewport.clientHeight - headerHeight - 5);
            var rowPx = Math.floor(available / 5);
            // Thu nho hang thay vi ep muc 62px lam bang bi tran ngoai viewport.
            // 54px la kich thuoc an toan cho avatar 40px va padding tren/duoi.
            rowPx = Math.max(54, rowPx);
            table.style.setProperty('--mlt-equal-row-height', rowPx + 'px');
            // Bang table co the lam tron kich thuoc theo subpixel: do lai de bot thanh cuon du 1-3px.
            if (rowNodes.length >= 5 && table.getBoundingClientRect().height > viewport.clientHeight + 1 && rowPx > 54) {
                var excess = Math.ceil(table.getBoundingClientRect().height - viewport.clientHeight + 2);
                rowPx = Math.max(54, rowPx - Math.ceil(excess / 5));
                table.style.setProperty('--mlt-equal-row-height', rowPx + 'px');
            }
            // Khi bang da vua dung 5 hang, scrollbar khong chiem chieu ngang bang.
            // Tren man hinh qua thap van de overflow-y:auto de khong cat noi dung.
            var fits = table.getBoundingClientRect().height <= viewport.clientHeight + 1;
            viewport.style.overflowY = fits ? 'hidden' : 'auto';
        }
        function syncTimeLaunchHeight() {
            var r = root(), btn = r && r.querySelector('#mltActualTimeLaunch'), badge = r && r.querySelector('.mlt-main-head-side .mlt-duration');
            if (!btn || !badge) return;
            var px = Math.ceil(badge.getBoundingClientRect().height);
            if (px > 0) btn.style.setProperty('--mlt-duration-height', px + 'px');
        }
        function syncMeetingListHeight() {
            var r = root(), list = r && r.querySelector('.mlt-meeting-list');
            if (!list || list.clientHeight <= 0) return;
            var cards = list.querySelectorAll('.mlt-meeting-link'), count = cards.length;
            if (!count) { list.style.overflowY = 'hidden'; return; }
            // QUAN TRONG: lay kich thuoc cua MOT hang tu khung 5 hang, KHONG chia cho so
            // hang dang loc. Loc con 1 hop thi hang van cao nhu khi hien 5 hop.
            var oneRowHeight = Math.max(64, Math.min(88, Math.floor((list.clientHeight - 2) / 5)));
            Array.prototype.forEach.call(cards, function (card) {
                card.style.setProperty('height', oneRowHeight + 'px');
                card.style.setProperty('flex', '0 0 auto');
            });
            // Phan du giu trang, khong keo to row. Neu khung thap thi cho scroll noi bo.
            list.style.overflowY = (count * oneRowHeight > list.clientHeight + 1) ? 'auto' : 'hidden';
        }
        function syncMeetLaborDimensions() { syncTimeLaunchHeight(); syncEqualRowHeight(); syncMeetingListHeight(); }
        // Icon chu tri dung tooltip vang dong bo, khong dung tooltip den cua browser.
        var hostHoverTip = null, hostHoverAnchor = null;
        function hostIconOf(target) {
            return target && target.closest ? target.closest('.meet-panel-labor .mlt-meeting-host, .meet-panel-labor .mlt-person-host-icon') : null;
        }
        function hideHostTooltip() {
            if (hostHoverTip && hostHoverTip.parentNode) hostHoverTip.parentNode.removeChild(hostHoverTip);
            hostHoverTip = null; hostHoverAnchor = null;
        }
        function showHostTooltip(el) {
            if (!el || !document.body.contains(el)) return;
            if (hostHoverAnchor === el && hostHoverTip) return;
            hideHostTooltip();
            closeMeetTimePopover();
            closeHourPicker();
            initMeetingDatetimeInputs();
            pinnedTimeRow = null; hoverTimeRow = null; timePopupSubmitting = false; if (hoverTimeTimer) { clearTimeout(hoverTimeTimer); hoverTimeTimer = null; }
            var isMeeting = el.classList.contains('mlt-meeting-host');
            var label = isMeeting ? 'Bạn chủ trì' : 'Chủ trì';
            // Xoa title server de khong hien tooltip den chong tooltip moi.
            el.removeAttribute('title');
            var tip = document.createElement('div');
            tip.className = 'mlt-host-hover-tip'; tip.setAttribute('role', 'tooltip');
            var ico = document.createElement('i'); ico.className = 'fas fa-crown'; ico.setAttribute('aria-hidden', 'true');
            tip.appendChild(ico); tip.appendChild(document.createTextNode(label));
            document.body.appendChild(tip);
            var r = el.getBoundingClientRect(), w = tip.offsetWidth, h = tip.offsetHeight, margin = 8;
            var left = Math.max(margin, Math.min(window.innerWidth - w - margin, r.left + (r.width - w) / 2));
            var top = r.top - h - 9;
            if (top < margin) top = Math.min(window.innerHeight - h - margin, r.bottom + 9);
            tip.style.left = Math.round(left) + 'px'; tip.style.top = Math.round(top) + 'px';
            hostHoverAnchor = el; hostHoverTip = tip;
        }
        document.addEventListener('mouseover', function (e) { var el = hostIconOf(e.target); if (el) showHostTooltip(el); });
        document.addEventListener('mouseout', function (e) {
            var el = hostIconOf(e.target);
            if (el && !el.contains(e.relatedTarget) && el === hostHoverAnchor) hideHostTooltip();
        });
        document.addEventListener('focusin', function (e) { var el = hostIconOf(e.target); if (el) showHostTooltip(el); });
        document.addEventListener('focusout', function (e) { var el = hostIconOf(e.target); if (el && el === hostHoverAnchor) hideHostTooltip(); });
        document.addEventListener('scroll', function () { if (hostHoverTip) hideHostTooltip(); }, true);
        window.addEventListener('resize', hideHostTooltip, { passive: true });
        var lastFocusPulse = '';
        function pulseFocusedMeeting() {
            var state = filterStateNode();
            if (!state) return;
            var id = state.getAttribute('data-focus-id') || '', token = state.getAttribute('data-focus-pulse') || '';
            if (!id || !token || token === lastFocusPulse) return;
            var r = root();
            var row = r && r.querySelector('.mlt-meeting-link[data-meet-id="' + id + '"]');
            if (!row) return;
            lastFocusPulse = token;
            var list = row.closest('.mlt-meeting-list');
            if (list) {
                var bounds = list.getBoundingClientRect(), itemBounds = row.getBoundingClientRect();
                if (itemBounds.top < bounds.top) list.scrollTop -= (bounds.top - itemBounds.top) + 5;
                else if (itemBounds.bottom > bounds.bottom) list.scrollTop += (itemBounds.bottom - bounds.bottom) + 5;
            }
            if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
            window.setTimeout(function () {
                if (!document.documentElement.contains(row)) return;
                var rect = row.getBoundingClientRect();
                if (rect.width < 30 || rect.height < 20) return;
                var layer = document.createElement('div');
                layer.className = 'meet-panel-labor mlt-focus-overlay';
                layer.setAttribute('aria-hidden', 'true');
                layer.style.left = Math.round(rect.left) + 'px';
                layer.style.top = Math.round(rect.top) + 'px';
                layer.style.width = Math.round(rect.width) + 'px';
                layer.style.height = Math.round(rect.height) + 'px';
                var clone = row.cloneNode(true);
                // Ban sao dung cao rong CHINH XAC cua row that, khong lay chieu cao
                // phan khung trong; hieu ung scale chi ap dung cho layer noi.
                clone.style.height = '100%';
                clone.style.minHeight = '0';
                clone.removeAttribute('id'); clone.removeAttribute('href'); clone.removeAttribute('onclick');
                clone.removeAttribute('onmouseover'); clone.tabIndex = -1;
                layer.appendChild(clone);
                document.body.appendChild(layer);
                function cleanup() { if (layer.parentNode) layer.parentNode.removeChild(layer); }
                layer.addEventListener('animationend', cleanup, {once:true});
                window.setTimeout(cleanup, 1450);
            }, 130);
        }
        function afterRender() {
            hideHostTooltip();
            closeMeetTimePopover();
            closeHourPicker();
            initMeetingDatetimeInputs();
            pinnedTimeRow = null; hoverTimeRow = null; timePopupSubmitting = false; if (hoverTimeTimer) { clearTimeout(hoverTimeTimer); hoverTimeTimer = null; }
            var hostLabels = document.querySelectorAll('.meet-panel-labor .mlt-meeting-host');
            Array.prototype.forEach.call(hostLabels, function (el) {
                el.removeAttribute('title'); el.setAttribute('aria-label', 'Bạn chủ trì'); el.setAttribute('tabindex', '0');
            });
            hideCalendar();
            syncFilterControls();
            syncPageRanges();
            pulseFocusedMeeting();
            var r = root(), viewport = r && r.querySelector('.mlt-table-scroll');
            if (equalRowObserver && observedLaborViewport !== viewport) {
                equalRowObserver.disconnect();
                observedLaborViewport = viewport;
                if (viewport) equalRowObserver.observe(viewport);
            }
            syncMeetLaborDimensions();
            // Dua phep do ve sau khi trinh duyet tinh lai layout cua UpdatePanel va tab.
            if (window.requestAnimationFrame) requestAnimationFrame(syncMeetLaborDimensions);
            setTimeout(syncMeetLaborDimensions, 80);
            var r = root(), state = r && r.querySelector('input[id$="_hfSaveResult"]');
            var currentPane = r && r.querySelector('.mlt-selected-content');
            // Lưu SQL lỗi: giữ baseline CŨ để lần chuyển trang sau vẫn nhận ra dữ liệu chưa lưu.
            // Cập nhật tham chiếu DOM mới để không reset baseline khi ASP.NET thay UpdatePanel.
            if (state && state.value === 'error' && lastMeetingId === currentMeetingId()) {
                lastRenderedPane = currentPane;
            } else if (lastRenderedPane !== currentPane || lastMeetingId !== currentMeetingId()) {
                setBaseline();
            }
            update();
            if (!savingForNavigation) return;
            if (state && state.value === 'success') {
                var action = pendingAfterSave; pendingAfterSave = null; savingForNavigation = false; closeDialog(); setBaseline();
                // Defer next async request until the current UpdatePanel request fully ends.
                setTimeout(function () { dispatch(action); }, 40);
            } else if (state && state.value === 'error') {
                savingForNavigation = false; pendingAfterSave = null; closeDialog();
                // Data is still in the current form; user can fix the error and retry.
            }
        }
        // Khi nhân viên chuyển từ "Không tham gia" sang "Có tham gia":
        // mở Công thực tế và đề xuất đúng Công lý thuyết của cuộc họp.
        // Chỉ tự điền khi thao tác chọn Có; không ghi đè giờ đã lưu, giờ vừa nhập,
        // hoặc dữ liệu khi trang được tải lại qua UpdatePanel/phân trang.

        // Gio hop thuc te van gui raw ASP.NET TextBox (datetime-local) ve server.
        // Tach ngay va gio tren UI de dung chung picker voi gio vao/ra nhan vien.
        function padClock(n) { return ('0' + Number(n)).slice(-2); }
        function rawMeetTime(which) { var r = root(); return r && r.querySelector('input[id$="_' + (which === 'start' ? 'txtMeetStartActual' : 'txtMeetEndActual') + '"]'); }
        function initMeetingDatetimeInputs() {
            var r = root(); if (!r) return;
            ['start','end'].forEach(function(which){
                var raw = rawMeetTime(which), day = r.querySelector('.mlt-meet-date[data-datetime-for="' + which + '"]');
                var timeButton = r.querySelector('.mlt-meet-hm-trigger[data-time-for="' + which + '"]');
                if (!raw || !day || !timeButton) return;
                var val = raw.value || '';
                day.value = val.slice(0,10);
                timeButton.dataset.hm = val.slice(11,16) || '00:00';
                var display = timeButton.querySelector('.mlt-hm-display'); if (display) display.textContent = val.slice(11,16) || '--:--';
                day.disabled = raw.disabled; timeButton.disabled = raw.disabled;
            });
            Array.prototype.forEach.call(rows(),function(row){
                var tin=row.querySelector('input[id$="_txtTimeIn"]'),tout=row.querySelector('input[id$="_txtTimeOut"]');
                if (tin) tin.readOnly = true;
                if (tout) tout.readOnly = true;
            });
        }
        function syncMeetingRawTime(which) {
            var r = root(), raw = rawMeetTime(which);
            var day = r && r.querySelector('.mlt-meet-date[data-datetime-for="' + which + '"]');
            var btn = r && r.querySelector('.mlt-meet-hm-trigger[data-time-for="' + which + '"]');
            if (!raw || !day || !btn) return;
            raw.value = day.value && btn.dataset.hm ? day.value + 'T' + btn.dataset.hm : '';
        }
        function createHourPicker() {
            var el = document.getElementById('mltReusableHourPicker');
            if (el) return el;
            el=document.createElement('div'); el.id='mltReusableHourPicker'; el.className='mlt-hm-picker';
            el.hidden=true;
            el.innerHTML='<div class="mlt-hm-picker-head"><strong id="mltPickerTitle">Chọn giờ</strong><span>Chọn giờ và phút</span></div>' +
                '<div class="mlt-hm-picked" id="mltPickedTime">--:--</div>' +
                '<div class="mlt-hm-picker-caption">GIỜ</div><div class="mlt-hm-hour-grid"></div>' +
                '<div class="mlt-hm-picker-caption">PHÚT</div><div class="mlt-hm-minute-grid"></div>' +
                '<div class="mlt-hm-picker-foot"><span><i class="fas fa-lightbulb"></i> Chọn giờ rồi chọn phút 00 / 15 / 30 / 45.</span><button type="button" class="mlt-hm-done">Xong</button></div>';
            var grid=el.querySelector('.mlt-hm-hour-grid'), mins=el.querySelector('.mlt-hm-minute-grid');
            for(var h=0;h<24;h++){var bh=document.createElement('button');bh.type='button';bh.className='mlt-hm-option';bh.dataset.hour=String(h);bh.textContent=padClock(h);grid.appendChild(bh);}
            [0,15,30,45].forEach(function(m){var bm=document.createElement('button');bm.type='button';bm.className='mlt-hm-option';bm.dataset.minute=String(m);bm.textContent=padClock(m);mins.appendChild(bm);});
            el.addEventListener('click',function(e){
                var opt=e.target.closest('.mlt-hm-option');
                if(opt){
                    if(opt.hasAttribute('data-hour'))pickerValue.hour=Number(opt.dataset.hour);
                    if(opt.hasAttribute('data-minute'))pickerValue.minute=Number(opt.dataset.minute);
                    refreshPickerSelection();e.preventDefault();return;
                }
                if(e.target.closest('.mlt-hm-done')) {var fn=pickerFinish, value=padClock(pickerValue.hour)+':'+padClock(pickerValue.minute);closeHourPicker();if(fn)fn(value);e.preventDefault();}
            });
            document.body.appendChild(el);
            return el;
        }
        function refreshPickerSelection(){
            var el=activeHourPicker;if(!el)return;
            el.querySelector('#mltPickedTime').textContent=padClock(pickerValue.hour)+':'+padClock(pickerValue.minute);
            Array.prototype.forEach.call(el.querySelectorAll('.mlt-hm-option'),function(b){
                b.classList.toggle('is-selected', b.hasAttribute('data-hour')?Number(b.dataset.hour)===pickerValue.hour:Number(b.dataset.minute)===pickerValue.minute);
            });
        }
        function resetMeetPickerStack() {
            var pop = timePopover();
            if (pop) {
                pop.classList.remove('is-picker-stacked');
                pop.style.removeProperty('max-height');
                pop.style.removeProperty('overflow-y');
            }
            var picker = document.getElementById('mltReusableHourPicker');
            if (picker) picker.style.removeProperty('max-height');
            pickerStackedForMeet = false;
            positionMeetTimePopover();
        }
        function closeHourPicker(){
            if(activeHourPicker){activeHourPicker.hidden=true;activeHourPicker=null;}
            pickerFinish=null;
            if(pickerStackedForMeet)resetMeetPickerStack();
        }
        // Với giờ họp: ưu tiên hai popup XẾP DỌC; nếu viewport thấp, co chiều cao
        // picker và cho cuộn nội bộ, không đảo picker lên trên che khung thiết lập.
        function positionMeetHourPickerStack() {
            var pop=timePopover(),launch=timeLaunch(),picker=activeHourPicker;
            if(!pickerStackedForMeet||!pop||!launch||!picker||picker.hidden||!pop.classList.contains('is-open'))return;
            var vw=window.innerWidth||document.documentElement.clientWidth;
            var vh=window.innerHeight||document.documentElement.clientHeight;
            var margin=10,gap=10;
            var available=Math.max(150,vh-margin*2-gap);
            // Popup thiet lap (nho) luon hien du; chi popup chon gio (lon) moi cuon neu viewport thap.
            pop.style.removeProperty('max-height');pop.style.removeProperty('overflow-y');
            picker.style.removeProperty('max-height');
            pop.classList.add('is-picker-stacked');
            pop.style.position='fixed';pop.style.right='auto';pop.style.bottom='auto';
            pop.style.zIndex='2147480400';
            var popH=pop.getBoundingClientRect().height;
            var pickerLimit=Math.max(100,available-popH);
            picker.style.maxHeight=Math.floor(pickerLimit)+'px';
            var pickerH=picker.getBoundingClientRect().height;
            var groupH=popH+gap+pickerH;
            var launchRect=launch.getBoundingClientRect();
            var width=Math.max(pop.getBoundingClientRect().width,picker.getBoundingClientRect().width);
            var left=Math.max(margin,Math.min(vw-width-margin,launchRect.right-width));
            // Popup thiet lap o tren; bo chon gio mo NGAY ben duoi popup thiet lap.
            // Neu man hinh thap, ca cum tu dich len va chi scroll ben trong picker.
            var desiredTop=launchRect.top-popH-8;
            var top=Math.max(margin,Math.min(desiredTop,vh-groupH-margin));
            pop.classList.add('is-clamped');
            pop.style.left=Math.round(left)+'px';pop.style.top=Math.round(top)+'px';
            picker.style.left=Math.round(left)+'px';picker.style.top=Math.round(top+popH+gap)+'px';
        }
        function showHourPicker(anchor,sourceValue,label,done){
            if(activeHourPicker)closeHourPicker();
            var parts=/^(\d{2}):(\d{2})$/.exec(sourceValue||'');
            pickerValue.hour=parts?Number(parts[1]):0;pickerValue.minute=parts?Number(parts[2]):0;
            if(pickerValue.hour>23||pickerValue.minute>59){pickerValue.hour=0;pickerValue.minute=0;}
            var el=createHourPicker();activeHourPicker=el;pickerFinish=done;el.hidden=false;
            el.querySelector('#mltPickerTitle').textContent=label;
            refreshPickerSelection();
            pickerStackedForMeet=!!(anchor&&anchor.classList&&anchor.classList.contains('mlt-meet-hm-trigger')&&timePopover()&&timePopover().classList.contains('is-open'));
            if(pickerStackedForMeet){positionMeetHourPickerStack();return;}
            // Giữ nguyên hành vi của bộ chọn giờ nhân viên.
            var r=anchor.getBoundingClientRect(),w=el.offsetWidth,h=el.offsetHeight,pad=9;
            var left=Math.max(pad,Math.min(window.innerWidth-w-pad,r.left));
            var top=r.bottom+8;
            if(top+h>window.innerHeight-pad)top=r.top-h-8;
            top=Math.max(pad,Math.min(window.innerHeight-h-pad,top));
            el.style.left=Math.round(left)+'px';el.style.top=Math.round(top)+'px';
        }
        window.addEventListener('resize',function(){if(pickerStackedForMeet)positionMeetHourPickerStack();else positionMeetTimePopover();},{passive:true});
        window.addEventListener('scroll',function(){if(pickerStackedForMeet)positionMeetHourPickerStack();else positionMeetTimePopover();},{passive:true});
        document.addEventListener('change',function(e){
            var day=e.target.closest && e.target.closest('.meet-panel-labor .mlt-meet-date');
            if(day)syncMeetingRawTime(day.dataset.datetimeFor);
        });
        document.addEventListener('click',function(e){
            if(e.target.closest && e.target.closest('#mltReusableHourPicker'))return;
            var btn=e.target.closest && e.target.closest('.meet-panel-labor .mlt-hm-trigger');
            var employeeInput=e.target.closest && e.target.closest('.meet-panel-labor .mlt-staff-hour input[type="time"]');
            if(!btn && !employeeInput){if(activeHourPicker)closeHourPicker();return;}
            e.preventDefault();
            if(btn && btn.disabled)return;
            if(btn && btn.classList.contains('mlt-meet-hm-trigger')){
                var which=btn.dataset.timeFor,fieldName=which==='start'?'bắt đầu':'kết thúc';
                showHourPicker(btn,btn.dataset.hm||'00:00','Chọn giờ '+fieldName,function(v){
                    btn.dataset.hm=v;
                    var display=btn.querySelector('.mlt-hm-display');if(display)display.textContent=v;
                    syncMeetingRawTime(which);
                });
                return;
            }
            var input=employeeInput || (btn && btn.closest('.mlt-staff-hour') && btn.closest('.mlt-staff-hour').querySelector('input[type="time"]'));
            var row=input && input.closest('.mlt-labor-row');
            if(!row||input.disabled)return;
            var isIn=input.id.indexOf('txtTimeIn')>=0;
            showHourPicker(btn||input,input.value,isIn?'Chọn giờ vào':'Chọn giờ ra',function(v){
                input.value=v;
                input.dispatchEvent(new Event('input',{bubbles:true}));
                input.dispatchEvent(new Event('change',{bubbles:true}));
                var editor=row.querySelector('.mlt-time-editor');if(editor && editor.classList.contains('is-open'))placeTimeEditor(row);
            });
        });
        document.addEventListener('keydown',function(e){if(e.key==='Escape'&&activeHourPicker){e.preventDefault();closeHourPicker();}},true);

        function timeToMinutes(raw) { var m = /^(\d{2}):(\d{2})$/.exec(raw || ''); if (!m) return null; var h = Number(m[1]), minute = Number(m[2]); return h < 24 && minute < 60 ? h * 60 + minute : null; }
        function effectiveHm(kind) {
            var panel = root() && root().querySelector('.mlt-selected-content');
            return panel ? (panel.getAttribute(kind === 'start' ? 'data-effective-start-hm' : 'data-effective-end-hm') || '') : '';
        }
        function ensureEditorDefaults(row) {
            var tin = row && row.querySelector('input[id$="_txtTimeIn"]'), tout = row && row.querySelector('input[id$="_txtTimeOut"]');
            if (tin && !tin.value) tin.value = effectiveHm('start');
            if (tout && !tout.value) tout.value = effectiveHm('end');
        }
        function placeTimeEditor(row) {
            var editor = row && row.querySelector('.mlt-time-editor'), button = row && row.querySelector('.mlt-clock-toggle');
            if (!editor || !button) return;
            editor.style.left = '0px'; editor.style.top = '0px';
            var rect = button.getBoundingClientRect();
            var width = editor.offsetWidth || 270, height = editor.offsetHeight || 180, margin = 10;
            var left = Math.max(margin, Math.min(window.innerWidth - width - margin, rect.right - width));
            var top = rect.top - height - 10;
            var below = false;
            if (top < margin) { top = Math.min(window.innerHeight - height - margin, rect.bottom + 10); below = true; }
            editor.classList.toggle('is-below', below);
            editor.style.left = Math.round(left) + 'px';
            editor.style.top = Math.round(top) + 'px';
        }
        function closeTimeEditor(row, force) {
            var editor = row && row.querySelector('.mlt-time-editor'), button = row && row.querySelector('.mlt-clock-toggle');
            if (!row || !editor || !button) return;
            if (!force && editor.classList.contains('is-pinned')) return;
            editor.classList.remove('is-open', 'is-preview', 'is-pinned', 'is-below');
            editor.style.left = '0px'; editor.style.top = '0px';
            button.classList.remove('is-active');
            button.setAttribute('aria-expanded', 'false');
            if (pinnedTimeRow === row) pinnedTimeRow = null;
            if (hoverTimeRow === row) hoverTimeRow = null;
        }
        function closeAllTimeEditors(exceptRow) {
            Array.prototype.forEach.call(rows(), function (row) { if (row !== exceptRow) closeTimeEditor(row, true); });
        }
        function openTimePreview(row) {
            // Khi da ghim, mouseover khong duoc bien popup thanh preview (nut Luu se bien mat).
            if (!row || pinnedTimeRow) return;
            var editor = row.querySelector('.mlt-time-editor'), button = row.querySelector('.mlt-clock-toggle');
            if (!editor || !button || button.disabled) return;
            ensureEditorDefaults(row);
            editor.classList.add('is-open', 'is-preview');
            editor.classList.remove('is-pinned');
            button.setAttribute('aria-expanded', 'true');
            placeTimeEditor(row);
            hoverTimeRow = row;
        }
        function pinTimeEditor(row) {
            if (!row) return;
            var editor = row.querySelector('.mlt-time-editor'), button = row.querySelector('.mlt-clock-toggle'), mode = row.querySelector('input[id$="_hfTimeMode"]');
            if (!editor || !button || button.disabled) return;
            closeAllTimeEditors(row);
            ensureEditorDefaults(row);
            if (mode) mode.value = 'time';
            editor.classList.add('is-open', 'is-pinned');
            editor.classList.remove('is-preview');
            button.classList.add('is-active');
            button.setAttribute('aria-expanded', 'true');
            pinnedTimeRow = row;
            hoverTimeRow = row;
            placeTimeEditor(row);
            updateRowFromClock(row);
            var focusInput = row.querySelector('input[id$="_txtTimeIn"]'); if (focusInput) setTimeout(function () { focusInput.focus(); focusInput.select && focusInput.select(); }, 0);
        }
        function updateRowFromClock(row) {
            var mode = row.querySelector('input[id$="_hfTimeMode"]'); if (!mode || mode.value !== 'time') return false;
            var tin = row.querySelector('input[id$="_txtTimeIn"]'), tout = row.querySelector('input[id$="_txtTimeOut"]'), actual = row.querySelector('.mlt-actual');
            var a = timeToMinutes(tin && tin.value), b = timeToMinutes(tout && tout.value);
            var error = row.querySelector('.mlt-employee-time-error');
            function fail(message){if(error)error.textContent=message;return false;}
            if (a === null || b === null) return fail('Hãy chọn đủ giờ vào và giờ ra.');
            var panel=root()&&root().querySelector('.mlt-selected-content');
            var meetStart=panel&&panel.getAttribute('data-meeting-start'),meetEnd=panel&&panel.getAttribute('data-meeting-end');
            var start=meetStart?new Date(meetStart):null,end=meetEnd?new Date(meetEnd):null;
            if (!start || !end || !isFinite(start.getTime()) || !isFinite(end.getTime())) return fail('Không xác định được thời gian cuộc họp.');
            var clockIn=new Date(start.getFullYear(),start.getMonth(),start.getDate(),Math.floor(a/60),a%60);
            while (clockIn < start) clockIn.setDate(clockIn.getDate()+1);
            var clockOut=new Date(clockIn.getFullYear(),clockIn.getMonth(),clockIn.getDate(),Math.floor(b/60),b%60);
            if (clockOut<=clockIn) clockOut.setDate(clockOut.getDate()+1);
            if (clockIn<start || clockOut>end || clockOut<=clockIn) return fail('Giờ vào/ra phải nằm trong khoảng thời gian họp thực tế.');
            var minutes=Math.round((clockOut-clockIn)/60000);
            if (actual) actual.value = (Math.round(minutes / 60 * 100) / 100).toLocaleString('vi-VN',{maximumFractionDigits:2});
            if(error)error.textContent='';
            return true;
        }

        document.addEventListener('click', function (e) {
            if(e.target.closest && e.target.closest('#mltReusableHourPicker'))return;
            var launch = e.target.closest && e.target.closest('.meet-panel-labor #mltActualTimeLaunch');
            if (launch) {
                e.preventDefault();
                if (timePopover() && timePopover().classList.contains('is-open')) closeMeetTimePopover(); else openMeetTimePopover();
                return;
            }
            var closeLaunch = e.target.closest && e.target.closest('.meet-panel-labor .mlt-time-popover-close');
            if (closeLaunch) { e.preventDefault(); closeMeetTimePopover(); return; }
            var pop = timePopover(), btn = timeLaunch();
            if (pop && pop.classList.contains('is-open') && !pop.contains(e.target) && btn && !btn.contains(e.target)) closeMeetTimePopover();
        });
        window.addEventListener('resize', function () { if (pinnedTimeRow) placeTimeEditor(pinnedTimeRow); }, { passive: true });
        document.addEventListener('scroll', function () { if (pinnedTimeRow) placeTimeEditor(pinnedTimeRow); }, true);
        document.addEventListener('mouseover', function (e) {
            var button = e.target.closest && e.target.closest('.meet-panel-labor .mlt-clock-toggle');
            if (!button || button.disabled) return;
            if (hoverTimeTimer) { clearTimeout(hoverTimeTimer); hoverTimeTimer = null; }
            openTimePreview(button.closest('.mlt-labor-row'));
        });
        document.addEventListener('mouseout', function (e) {
            var area = e.target.closest && e.target.closest('.meet-panel-labor .mlt-actual-cell');
            if (!area) return;
            var row = area.closest('.mlt-labor-row');
            if (!row || row === pinnedTimeRow) return;
            var next = e.relatedTarget;
            if (next && area.contains(next)) return;
            if (hoverTimeTimer) clearTimeout(hoverTimeTimer);
            hoverTimeTimer = setTimeout(function () { closeTimeEditor(row, true); }, 90);
        });
        document.addEventListener('click', function (e) {
            if(e.target.closest && e.target.closest('#mltReusableHourPicker'))return;
            var button = e.target.closest && e.target.closest('.meet-panel-labor .mlt-clock-toggle');
            if (button && !button.disabled) {
                e.preventDefault();
                var row = button.closest('.mlt-labor-row');
                if (pinnedTimeRow === row) closeTimeEditor(row, true); else pinTimeEditor(row);
                update();
                return;
            }
            var apply = e.target.closest && e.target.closest('.meet-panel-labor .mlt-time-editor-apply');
            if (apply) {
                e.preventDefault(); e.stopPropagation();
                var applyRow = apply.closest('.mlt-labor-row');
                var editor = applyRow && applyRow.querySelector('.mlt-time-editor');
                if (!applyRow || !editor || !editor.classList.contains('is-pinned')) return;
                if (apply.classList.contains('is-saving')) return;
                var modeApply = applyRow.querySelector('input[id$="_hfTimeMode"]');
                if (modeApply) modeApply.value = 'time';
                if (!updateRowFromClock(applyRow)) return;
                update();
                var r = root(), save = r && r.querySelector('a[id$="_btnSaveLabor"],button[id$="_btnSaveLabor"],input[id$="_btnSaveLabor"]');
                var error = applyRow.querySelector('.mlt-employee-time-error');
                if (!save || save.hasAttribute('disabled') || save.getAttribute('aria-disabled') === 'true') {
                    if (error) error.textContent = 'Không tìm thấy nút lưu giờ công. Hãy tải lại tab và thử lại.';
                    return;
                }
                var requestManager = window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager && Sys.WebForms.PageRequestManager.getInstance();
                if (requestManager && requestManager.get_isInAsyncPostBack()) {
                    if (error) error.textContent = 'Hệ thống đang lưu dữ liệu khác. Hãy thử lại sau khi yêu cầu hiện tại hoàn tất.';
                    return;
                }
                apply.classList.add('is-saving'); apply.disabled = true;
                timePopupSubmitting = true;
                // Không đóng popup trước khi gửi; nếu ASP.NET báo lỗi vẫn nhìn thấy nội dung đang sửa.
                // Gọi LinkButton WebForms sẵn có, giữ nguyên async trigger và kiểm tra quyền ở server.
                try {
                    closeHourPicker();
                    save.click();
                    // Nếu không phát sinh postback, báo rõ; không giả định là đã lưu.
                    setTimeout(function () {
                        if (!document.documentElement.contains(apply)) return;
                        if (!requestManager || !requestManager.get_isInAsyncPostBack()) {
                            var state = r && r.querySelector('input[id$="_hfSaveResult"]');
                            if (!state || state.value !== 'success') {
                                apply.disabled = false; apply.classList.remove('is-saving'); timePopupSubmitting = false;
                                if (error && !error.textContent) error.textContent = 'Không nhận được phản hồi lưu giờ công. Hãy thử nút “Lưu giờ công” ở cuối bảng.';
                            }
                        }
                    }, 900);
                } catch (ex) {
                    apply.disabled = false; apply.classList.remove('is-saving'); timePopupSubmitting = false;
                    if (error) error.textContent = 'Không thể gửi yêu cầu lưu: ' + (ex && ex.message ? ex.message : 'lỗi không xác định');
                }
                return;
            }
            var closeBtn = e.target.closest && e.target.closest('.meet-panel-labor .mlt-time-editor-close');
            if (closeBtn) {
                e.preventDefault();
                var closeRow = closeBtn.closest('.mlt-labor-row');
                closeTimeEditor(closeRow, true); return;
            }
            if (pinnedTimeRow && !timePopupSubmitting) {
                var editor = pinnedTimeRow.querySelector('.mlt-time-editor'), toggle = pinnedTimeRow.querySelector('.mlt-clock-toggle');
                if (editor && !editor.contains(e.target) && toggle && !toggle.contains(e.target)) closeTimeEditor(pinnedTimeRow, true);
            }
        });
        document.addEventListener('input', function (e) {
            var target = e.target; if (!target || !target.closest) return; var row = target.closest('.meet-panel-labor .mlt-labor-row'); if (!row) return;
            var mode = row.querySelector('input[id$="_hfTimeMode"]'); if (!mode) return;
            if (target.matches('.mlt-actual')) {
                mode.value = 'manual'; closeTimeEditor(row, true);
            }
            if (target.matches('input[id$="_txtTimeIn"],input[id$="_txtTimeOut"]')) { mode.value = 'time'; updateRowFromClock(row); }
            update();
        });
        function fillTheoryOnJoin(row) {
            if (!row || row.getAttribute('data-can-edit') !== '1') return;
            var yes = row.querySelector('.mlt-join input[type="radio"][value="1"]');
            var actual = row.querySelector('.mlt-actual');
            if (!yes || !yes.checked || !actual) return;
            var hours = Number(row.getAttribute('data-duration'));
            if (!isFinite(hours) || hours < 0) return;
            // Giữ tối đa hai chữ số thập phân, dùng dấu phẩy theo giao diện tiếng Việt.
            actual.value = (Math.round(hours * 100) / 100).toLocaleString('vi-VN', {
                minimumFractionDigits: 0,
                maximumFractionDigits: 2
            });
        }
        document.addEventListener('change', function (e) {
            var target = e.target;
            if (!target.matches || !target.matches('.meet-panel-labor .mlt-join input[type="radio"]')) return;
            var row = target.closest('.mlt-labor-row');
            if (target.value === '1' && target.checked) {
                var mode = row && row.querySelector('input[id$="_hfTimeMode"]');
                if (mode && mode.value !== 'time' && mode.value !== 'manual') mode.value = 'default';
                fillTheoryOnJoin(row);
            }
            // syncRow sẽ tự mở khóa ô Công thực tế nếu được phép chỉnh và có tham gia.
            update();
        });
        // Prevent Enter in an hours input from submitting the entire WebForms page.
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.matches && e.target.matches('.meet-panel-labor .mlt-actual,.meet-panel-labor input[type=time],.meet-panel-labor input[type=datetime-local]')) { e.preventDefault(); return; }
            if (e.key === 'Escape' && dialog && dialog.style.display === 'flex' && !savingForNavigation) { e.preventDefault(); closeDialog(); return; }
            if (!laborActive() || !(e.key === 'F5' || ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'r'))) return;
            e.preventDefault();
            navigate({ type: 'refresh' });
        }, true);
        document.addEventListener('click', function (e) {
            if (!laborActive() || !dirty() || bypassGuard || savingForNavigation) return;
            var t = e.target && e.target.closest ? e.target : null; if (!t) return;
            var view = t.closest('[data-meet-view]');
            if (view && view.getAttribute('data-meet-view') !== 'labor') {
                e.preventDefault(); e.stopImmediatePropagation(); ask({ type: 'view', view: view.getAttribute('data-meet-view') }); return;
            }
            var link = t.closest('a[href]'); if (!link || link.closest('.meet-panel-labor') || link.target === '_blank' || link.hasAttribute('download')) return;
            var href = link.getAttribute('href'); if (!href || href.charAt(0) === '#' || /^javascript:/i.test(href)) return;
            var url; try { url = new URL(href, window.location.href); } catch (ignore) { return; }
            if (!/^https?:$/i.test(url.protocol) || url.origin !== window.location.origin) return;
            e.preventDefault(); e.stopImmediatePropagation(); ask({ type: 'url', href: url.href });
        }, true);
        if (window.Sys && Sys.Application) Sys.Application.add_load(afterRender);
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', afterRender); else afterRender();
        window.addEventListener('resize', syncMeetLaborDimensions, { passive: true });
        document.addEventListener('click', function (e) {
            var tab = e.target && e.target.closest && e.target.closest('[data-meet-view="labor"]');
            if (tab) setTimeout(syncMeetLaborDimensions, 80);
        });
        window.QLDAMeetingLaborTab = { refresh: afterRender, selectMeeting: selectMeeting, focusMeeting: focusMeeting, changePage: changePage, changeLaborPage: changeLaborPage, beforeFilter: beforeFilter, beforeSecondarySave: beforeSecondarySave, beforeEmployeeOpen: beforeEmployeeOpen, hasChanges: dirty };
    })();
</script>

<script type="text/javascript">
    (function () {
        function syncEmployeeHourPreview() {
            var i = document.querySelector('input[id$="_txtEmployeeHoursIn"]'), o = document.querySelector('input[id$="_txtEmployeeHoursOut"]'), el = document.getElementById('mltEmployeeHoursPreview');
            if (!i || !o || !el) return;
            var parse = function (s) { var m = /^(\d{2}):(\d{2})$/.exec(s || ''); if (!m) return null; var h = Number(m[1]), min = Number(m[2]); return h < 24 && min < 60 ? h * 60 + min : null; };
            var a = parse(i.value), b = parse(o.value); if (a === null || b === null) { el.textContent = '—'; return; }
            var minutes = b - a; if (minutes <= 0) minutes += 1440;
            el.textContent = (Math.round(minutes / 60 * 100) / 100).toLocaleString('vi-VN', { maximumFractionDigits: 2 }) + ' giờ';
        }
        document.addEventListener('input', function (e) {
            if (!e.target || !e.target.matches || !e.target.matches('input[id$="_txtEmployeeHoursIn"],input[id$="_txtEmployeeHoursOut"]')) return;
            e.target.classList.remove('is-invalid');
            var notice = document.getElementById('mltEmployeeHoursClientError'); if (notice) notice.textContent = '';
            syncEmployeeHourPreview();
        });
        if (window.Sys && Sys.Application) Sys.Application.add_load(syncEmployeeHourPreview);
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', syncEmployeeHourPreview); else syncEmployeeHourPreview();
        var modalTimeActive = null;
        function modalTimeParts(v) { var m = /^(\d{2}):(\d{2})$/.exec(v || ''); return m ? { hour: Number(m[1]), minute: Number(m[2]) } : { hour: 9, minute: 0 }; }
        function padModalTime(v) { return String(v).padStart(2, '0'); }
        function syncModalTimeGrid() {
            var panel = document.getElementById('mltModalTimeChooser'); if (!panel || !modalTimeActive) return;
            var hourList = panel.querySelector('.mlt-modal-choice-hours'), minList = panel.querySelector('.mlt-modal-choice-minutes');
            if (!hourList.children.length) { for (var h = 0; h < 24; h++) { var b = document.createElement('button'); b.type = 'button'; b.setAttribute('data-hour', String(h)); b.textContent = padModalTime(h); hourList.appendChild(b); } [0, 15, 30, 45].forEach(function (m) { var b = document.createElement('button'); b.type = 'button'; b.setAttribute('data-minute', String(m)); b.textContent = padModalTime(m); minList.appendChild(b); }); }
            var parts = modalTimeParts(modalTimeActive.value);
            panel.querySelector('.mlt-modal-choice-current').textContent = padModalTime(parts.hour) + ':' + padModalTime(parts.minute);
            panel.querySelectorAll('[data-hour]').forEach(function (b) { b.classList.toggle('is-selected', Number(b.dataset.hour) === parts.hour); });
            panel.querySelectorAll('[data-minute]').forEach(function (b) { b.classList.toggle('is-selected', Number(b.dataset.minute) === parts.minute); });
        }
        document.addEventListener('click', function (e) {
            var trigger = e.target.closest && e.target.closest('[data-modal-hour]');
            if (trigger) { e.preventDefault(); var id = trigger.dataset.modalHour === 'in' ? 'txtEmployeeHoursIn' : 'txtEmployeeHoursOut'; var input = document.querySelector('input[id$="_' + id + '"]'), panel = document.getElementById('mltModalTimeChooser'); if (!input || !panel) return; modalTimeActive = input; panel.hidden = false; syncModalTimeGrid(); return; }
            var choose = e.target.closest && e.target.closest('#mltModalTimeChooser [data-hour],#mltModalTimeChooser [data-minute]');
            if (choose && modalTimeActive) { e.preventDefault(); var parts = modalTimeParts(modalTimeActive.value); if (choose.hasAttribute('data-hour')) parts.hour = Number(choose.dataset.hour); else parts.minute = Number(choose.dataset.minute); modalTimeActive.value = padModalTime(parts.hour) + ':' + padModalTime(parts.minute); modalTimeActive.dispatchEvent(new Event('input', { bubbles: true })); syncModalTimeGrid(); return; }
            var close = e.target.closest && e.target.closest('#mltModalTimeChooser [data-modal-close]');
            if (close) { e.preventDefault(); var panel = document.getElementById('mltModalTimeChooser'); if (panel) panel.hidden = true; modalTimeActive = null; }
        });
        // Cho phep nhap tay tung phut, ke ca 09:42. Khong tu lam tron sang 00/15/30/45.
        function normalizeManualTime(value) {
            var m = /^\s*(\d{1,2}):(\d{1,2})\s*$/.exec(value || '');
            if (!m) return null;
            var h = Number(m[1]), min = Number(m[2]);
            return h >= 0 && h <= 23 && min >= 0 && min <= 59 ? padModalTime(h) + ':' + padModalTime(min) : null;
        }
        document.addEventListener('focusout', function (e) {
            var t = e.target;
            if (!t || !t.matches || !t.matches('input[id$="_txtEmployeeHoursIn"],input[id$="_txtEmployeeHoursOut"]')) return;
            var v = normalizeManualTime(t.value);
            if (v !== null && t.value !== v) { t.value = v; t.dispatchEvent(new Event('input', { bubbles: true })); }
        });
        document.addEventListener('click', function (e) {
            var save = e.target.closest && e.target.closest('a[id$="_btnSaveEmployeeHours"]');
            if (!save) return;
            var tin = document.querySelector('input[id$="_txtEmployeeHoursIn"]'), tout = document.querySelector('input[id$="_txtEmployeeHoursOut"]');
            var message = document.getElementById('mltEmployeeHoursClientError');
            if (!tin || !tout) return;
            var a = normalizeManualTime(tin.value), b = normalizeManualTime(tout.value);
            tin.classList.toggle('is-invalid', a === null); tout.classList.toggle('is-invalid', b === null);
            if (a === null || b === null) {
                e.preventDefault(); e.stopImmediatePropagation();
                if (message) message.textContent = 'Vui lòng nhập giờ dạng HH:mm (00:00–23:59). Ví dụ: 09:42.';
                (a === null ? tin : tout).focus(); return;
            }
            tin.value = a; tout.value = b;
            if (message) message.textContent = '';
            // Viec kiem tra gio nam trong khoang hop va luu SQL van do server xu ly.
        }, true);
        if (window.Sys && Sys.Application) Sys.Application.add_load(function () {
            modalTimeActive = null;
            var pop = document.getElementById('mltModalTimeChooser'); if (pop) pop.hidden = true;
            var message = document.getElementById('mltEmployeeHoursClientError'); if (message) message.textContent = '';
        });
    })();
</script>
