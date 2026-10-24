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

</style>
<asp:UpdatePanel runat="server" ID="upLaborTab" UpdateMode="Conditional" ChildrenAsTriggers="true">
<ContentTemplate>
    <asp:LinkButton runat="server" ID="btnLoadLaborTab" CausesValidation="false" OnClick="btnLoadLaborTab_Click" Style="display:none">Tải giờ công</asp:LinkButton>
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
                        <asp:LinkButton runat="server" ID="btnSelectMeeting" CommandName="select" CommandArgument='<%# Eval("IdLichHop") %>' CausesValidation="false" CssClass='<%# MeetingLinkCss(Eval("IdLichHop")) %>' OnClientClick='<%# "return QLDAMeetingLaborTab.selectMeeting(\"" + Eval("IdLichHop") + "\");" %>'>
                            <span class="mlt-meeting-title-line"><strong><%#: Eval("TenCuocHop") %></strong><span class="mlt-meeting-title-badges"><%# MeetingHostBadge(Eval("LaChuTri")) %><%# MeetingEndedBadge(Eval("ThoiGianKetThuc")) %></span></span>
                            <small><i class="far fa-clock"></i> <%#: Eval("ThoiGianBatDau", "{0:dd/MM/yyyy}") %> <span class="mlt-meeting-time-divider" aria-hidden="true">·</span> <%#: Eval("ThoiGianBatDau", "{0:HH:mm}") %> – <%#: Eval("ThoiGianKetThuc", "{0:HH:mm}") %></small>
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
                    <div class="mlt-main-head-side"><div class="mlt-duration"><i class="far fa-clock"></i> <asp:Literal ID="litDuration" runat="server" /></div></div>
                    <div class="mlt-staff-tools" role="group" aria-label="Tìm kiếm và lọc nhân viên">
                        <label class="mlt-search"><i class="fas fa-search" aria-hidden="true"></i><input type="search" id="mltStaffSearch" placeholder="Tìm tên hoặc email nhân viên..." aria-label="Tìm nhân viên trong cuộc họp" autocomplete="off" maxlength="120" /></label>
                        <div class="mlt-staff-choice" role="group" aria-label="Lọc trạng thái tham gia">
                            <button type="button" data-mlt-attendance="0" aria-pressed="false">Không tham gia</button>
                            <button type="button" data-mlt-attendance="1" aria-pressed="false">Có tham gia</button>
                        </div>
                    </div>
                </div>
                <asp:HiddenField ID="hfSelectedMeetId" runat="server" />
                <asp:HiddenField ID="hfDuration" runat="server" />
                <div class="mlt-table-scroll">
                    <table class="mlt-table"><thead><tr><th style="width:35%">Tên nhân viên</th><th style="width:31%">Đã tham gia cuộc họp</th><th style="width:15%">Công lý thuyết</th><th style="width:19%">Công thực tế</th></tr></thead>
                    <tbody><asp:Repeater ID="rptLabor" runat="server" OnItemDataBound="rptLabor_ItemDataBound"><ItemTemplate>
                        <tr class="mlt-labor-row" data-duration="<%# CurrentDurationInvariant %>" data-can-edit="<%# CanEditLabor ? "1" : "0" %>" data-original-joined="<%# (bool)Eval("XacNhanThamGia") ? "1" : "0" %>">
                            <td><asp:HiddenField runat="server" ID="hfUserId" /><span id="personWrap" runat="server" class="mlt-user"><asp:Literal runat="server" ID="litAvatar" /><span class="mlt-person-copy"><span class="mlt-person-name-line"><i id="iconHost" runat="server" class="fas fa-crown mlt-person-host-icon" aria-label="Chủ trì" tabindex="0"></i><asp:Label runat="server" ID="lblName" CssClass="mlt-person-name" /><span id="tagHost" runat="server" class="mlt-host-tag">Chủ trì</span></span><asp:Label runat="server" ID="lblEmail" CssClass="mlt-person-email" /></span></span></td>
                            <td><asp:RadioButtonList runat="server" ID="rblAttendance" CssClass="mlt-join mlt-attendance-select" RepeatDirection="Horizontal" RepeatLayout="Table"><asp:ListItem Value="1">Có tham gia</asp:ListItem><asp:ListItem Value="0">Không tham gia</asp:ListItem></asp:RadioButtonList></td>
                            <td><asp:Label runat="server" ID="lblTheory" CssClass="mlt-theory" /></td>
                            <td><asp:TextBox ID="txtActual" runat="server" MaxLength="8" CssClass="mlt-actual" ToolTip="Nhập giờ thực tế" /></td>
                        </tr>
                    </ItemTemplate></asp:Repeater>
                    <asp:PlaceHolder runat="server" ID="phNoPeople" Visible="false"><tr><td colspan="4"><div class="mlt-empty"><asp:Literal ID="litNoLaborPeople" runat="server" Text="Cuộc họp này chưa có nhân viên." /></div></td></tr></asp:PlaceHolder></tbody></table>
                </div>
                <asp:Label runat="server" ID="lblLaborError" CssClass="mlt-error" EnableViewState="false" />
                <div class="mlt-main-foot">
                    <div class="mlt-total"><span>Người tham gia: <b id="mltJoinedCount"><asp:Literal ID="litJoinedCount" runat="server" /></b></span><span class="mlt-hidden-theory" aria-hidden="true"><b id="mltTheoryTotal"><asp:Literal ID="litTheoryTotal" runat="server" /></b></span></div>
                    <nav class="mlt-labor-page-footer" aria-label="Phân trang nhân viên"><asp:Literal ID="litLaborPager" runat="server" /></nav>
                    <asp:LinkButton runat="server" ID="btnSaveLabor" OnClick="btnSaveLabor_Click" CausesValidation="false" CssClass="btn mlt-save"><i class="fas fa-save"></i> Lưu giờ công</asp:LinkButton>
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlNoSelected" CssClass="mlt-empty"><i class="far fa-calendar-check"></i><span>Chọn cuộc họp ở bên trái để xem giờ công.</span></asp:Panel>
        </section>
    </div>
</ContentTemplate>
</asp:UpdatePanel>
<script type="text/javascript">
    (function () {
        if (window.QLDAMeetingLaborTab) return;
        var baseline = null, baselineRows = null, lastRenderedPane = null, lastMeetingId = null;
        var pendingAfterSave = null, savingForNavigation = false, bypassGuard = false;
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
        function normalizeHours(v) {
            var s = String(v == null ? '' : v).trim().replace(',', '.');
            if (!s) return '';
            var n = Number(s);
            return isFinite(n) ? String(n) : s;
        }
        function readRows() {
            var result = [];
            Array.prototype.forEach.call(rows(), function (row) {
                var id = row.querySelector('input[id$="_hfUserId"]'), yes = row.querySelector('input[type="radio"][value="1"]'), actual = row.querySelector('.mlt-actual');
                var joined = !!(yes && yes.checked);
                result.push({ id: id ? id.value : '', joined: joined ? '1' : '0', raw: actual ? actual.value : '', hours: joined ? normalizeHours(actual && actual.value) : '0' });
            });
            return result;
        }
        function compact(v) { return JSON.stringify(v.map(function (i) { return [i.id, i.joined, i.hours]; })); }
        function setBaseline() { baselineRows = readRows(); baseline = compact(baselineRows); lastRenderedPane = root() && root().querySelector('.mlt-selected-content'); lastMeetingId = currentMeetingId(); }
        function dirty() { return editable() && baseline !== null && currentMeetingId() === lastMeetingId && compact(readRows()) !== baseline; }
        function fmt(v) { v = Math.round((Number(v) || 0) * 100) / 100; return v.toLocaleString('vi-VN', { maximumFractionDigits: 2 }) + ' giờ'; }
        function syncRow(row) {
            var yes = row.querySelector('input[type="radio"][value="1"]'), joined = !!(yes && yes.checked), isEditable = row.getAttribute('data-can-edit') === '1';
            var table = row.querySelector('.mlt-join'), theory = row.querySelector('.mlt-theory'), actual = row.querySelector('.mlt-actual');
            var duration = Number(row.getAttribute('data-duration') || 0);
            if (table) table.setAttribute('data-edit', isEditable ? '1' : '0');
            if (theory) theory.textContent = fmt(joined ? duration : 0);
            if (actual) { actual.readOnly = !isEditable || !joined; if (!joined) actual.value = '0'; }
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
        function discardChanges() {
            if (!baselineRows) return;
            var originals = {}; baselineRows.forEach(function (x) { originals[x.id] = x; });
            Array.prototype.forEach.call(rows(), function (row) {
                var id = row.querySelector('input[id$="_hfUserId"]'); var old = id && originals[id.value]; if (!old) return;
                var input = row.querySelector('input[type="radio"][value="' + old.joined + '"]'); if (input) input.checked = true;
                var actual = row.querySelector('.mlt-actual'); if (actual) actual.value = old.raw;
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
                } else if (action.type === 'page') {
                    var r2 = root(), pageTarget = r2 && r2.querySelector('input[name="QLDA_MeetLabor_Page"]'), selected = r2 && r2.querySelector('input[name="QLDA_MeetLabor_SelectId"]');
                    var pageButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (pageTarget && pageButton) { pageTarget.value = String(action.page); if (selected) selected.value = ''; pageButton.click(); }
                } else if (action.type === 'laborPage') {
                    var r3 = root(), laborPageTarget = r3 && r3.querySelector('input[name="QLDA_MeetLabor_LaborPage"]');
                    var laborPageButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                    if (laborPageTarget && laborPageButton) { laborPageTarget.value = String(action.page); laborPageButton.click(); }
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
        function syncEqualRowHeight() {
            var r = root(), viewport = r && r.querySelector('.mlt-table-scroll'), table = viewport && viewport.querySelector('.mlt-table');
            if (!viewport || !table) return;
            // Noi dung bang luon su dung chieu cao TUNG hang cua trang 5 nhan vien,
            // ke ca khi trang hien tai chi co 1 hoac 2 nhan vien.
            var head = table.querySelector('thead');
            var available = viewport.clientHeight - (head ? head.getBoundingClientRect().height : 0) - 2;
            if (available <= 0) return;
            // Canh 5 dong vua khung. Man hinh qua thap thi cuon trong bang, khong cat du lieu.
            var height = Math.max(62, Math.floor(available / 5));
            table.style.setProperty('--mlt-equal-row-height', height + 'px');
        }
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
        function afterRender() {
            hideHostTooltip();
            var hostLabels = document.querySelectorAll('.meet-panel-labor .mlt-meeting-host');
            Array.prototype.forEach.call(hostLabels, function (el) {
                el.removeAttribute('title'); el.setAttribute('aria-label', 'Bạn chủ trì'); el.setAttribute('tabindex', '0');
            });
            hideCalendar();
            syncFilterControls();
            syncPageRanges();
            var r = root(), viewport = r && r.querySelector('.mlt-table-scroll');
            if (equalRowObserver && observedLaborViewport !== viewport) {
                equalRowObserver.disconnect();
                observedLaborViewport = viewport;
                if (viewport) equalRowObserver.observe(viewport);
            }
            syncEqualRowHeight();
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
            if (target.value === '1' && target.checked) fillTheoryOnJoin(row);
            // syncRow sẽ tự mở khóa ô Công thực tế nếu được phép chỉnh và có tham gia.
            update();
        });
        // Prevent Enter in an hours input from submitting the entire WebForms page.
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.matches && e.target.matches('.meet-panel-labor .mlt-actual')) { e.preventDefault(); return; }
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
        window.addEventListener('resize', syncEqualRowHeight, { passive: true });
        window.QLDAMeetingLaborTab = { refresh: afterRender, selectMeeting: selectMeeting, changePage: changePage, changeLaborPage: changeLaborPage, beforeFilter: beforeFilter, hasChanges: dirty };
    })();
</script>
