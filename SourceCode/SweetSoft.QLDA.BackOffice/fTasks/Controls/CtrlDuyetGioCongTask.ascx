<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDuyetGioCongTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlDuyetGioCongTask" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<style>
/* KBG20 | Compact workspace V3: all CSS rules are kept on a single line per selector. */
#<%= mdlDuyetGioCong.ClientID %> .modal-dialog{width:min(1360px,calc(100vw - 28px))!important;max-width:calc(100vw - 28px)!important;margin:18px auto!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-dialog:has(.qlpm-one){width:min(940px,calc(100vw - 28px))!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-content{max-height:calc(100dvh - 36px)!important;display:flex!important;flex-direction:column!important;overflow:hidden!important;border-radius:11px!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-header{flex:0 0 auto!important;min-height:38px!important;padding:8px 15px!important;position:relative!important;z-index:3!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-header .close,#<%= mdlDuyetGioCong.ClientID %> .modal-header .btn-close{position:relative!important;z-index:6!important;opacity:1!important;visibility:visible!important;flex-shrink:0!important;cursor:pointer!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-body{flex:1 1 auto!important;min-height:0!important;overflow:hidden!important;padding:0!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-footer{position:relative;z-index:4;flex:0 0 auto!important;min-height:50px;background:#fff!important;border-top:1px solid #e7e3ef!important;padding:8px 14px!important;display:flex!important;align-items:center;justify-content:flex-end;gap:8px;flex-wrap:wrap}
#<%= mdlDuyetGioCong.ClientID %> .modal-footer .btn{margin:0!important;min-height:33px;font-size:12px;font-weight:650;border-radius:7px}
.qlpm-shell{--qlpm-line:#e8e5ef;--qlpm-primary:#5a278f;display:flex;flex-direction:column;height:clamp(390px,68dvh,550px);max-height:calc(100dvh - 145px);min-height:0;overflow:hidden;color:#34364a;background:#fff;font-size:12.5px}
.qlpm-overview{flex:none;display:flex;align-items:center;justify-content:space-between;gap:18px;min-width:0;padding:13px 18px;background:#fcfbfe;border-bottom:1px solid var(--qlpm-line)}
.qlpm-task-top{display:flex;align-items:center;flex:1 1 52%;min-width:0}
.qlpm-task-inline{display:flex;align-items:center;gap:7px 9px;min-width:0;flex-wrap:wrap}
.qlpm-task-title{margin:0;min-width:0;max-width:100%;color:#322542;font-size:15px;font-weight:800;line-height:1.35;overflow-wrap:anywhere}
.qlpm-context{display:flex;align-items:center;flex-wrap:wrap;gap:5px}
.qlpm-shell .badge-pill-custom{display:inline-flex!important;align-items:center!important;white-space:nowrap!important;padding:3px 7px!important;border-radius:5px!important;font-size:10px!important;font-weight:700!important;line-height:1.3!important}
.qlpm-shell .badge-status-todo{background:#f1f5f9!important;color:#475569!important;border:1px solid #cbd5e1!important}
.qlpm-shell .badge-status-doing{background:#e0f2fe!important;color:#0369a1!important;border:1px solid #bae6fd!important}
.qlpm-shell .badge-status-done{background:#dcfce7!important;color:#15803d!important;border:1px solid #bbf7d0!important}
.qlpm-shell .badge-status-review{background:#fef3c7!important;color:#92400e!important;border:1px solid #fcd34d!important}
.qlpm-task-overdue{color:#bf3131;font-size:10px;font-weight:700}
.qlpm-status{display:inline-flex;align-items:center;gap:4px;padding:3px 7px;border:1px solid #e8d8f4;background:#f8f0ff;border-radius:5px;font-size:10px;font-weight:700;color:#734393;white-space:nowrap}
.qlpm-status.is-final{color:#14774b;border-color:#bfebd3;background:#ebfbf2}
.qlpm-summary{flex:0 1 48%;min-width:0;display:grid;grid-template-columns:.66fr 1fr .88fr;gap:7px;margin:0}
.qlpm-stat{display:flex;align-items:center;justify-content:space-between;gap:8px;padding:7px 10px;min-width:0;border:1px solid #eae5ef;background:#fff;border-radius:7px}
.qlpm-stat:first-child{background:#f8f5fc}
.qlpm-stat small{font-size:10px;line-height:1.35;font-weight:600;color:#6d7486}
.qlpm-stat strong{font-size:16px;line-height:1.3;color:#612794;font-weight:800;white-space:nowrap;font-variant-numeric:tabular-nums}
.qlpm-list-title{display:flex;align-items:center;justify-content:space-between;gap:12px;padding:8px 16px;border-bottom:1px solid var(--qlpm-line);flex:none;color:#633a87;background:#fff;font-size:11.5px}
.qlpm-list-title b{font-size:11.5px;font-weight:800}
.qlpm-count{font-size:10.5px;color:#868097;font-weight:600}
.qlpm-list-actions{display:flex;align-items:center;gap:5px;font-size:10.5px;color:#9790a2}
.qlpm-list-actions i{color:#8053ae}
.qlpm-list{position:relative;flex:1 1 auto;min-height:0;overflow:hidden;background:#fff}
.qlpm-employee-grid{display:flex;flex-direction:column;gap:7px;box-sizing:border-box;width:28%;height:100%;padding:11px;overflow:auto hidden;overscroll-behavior:contain;border-right:1px solid var(--qlpm-line);background:#faf9fc;scrollbar-width:thin;scrollbar-color:#d6cee3 transparent}
.qlpm-employee{display:contents}
.qlpm-employee-head{box-sizing:border-box;display:grid;grid-template-columns:34px minmax(0,1fr);grid-template-rows:auto auto;align-items:start;gap:3px 9px;min-height:80px;width:100%;text-align:left;padding:10px;border:1px solid #eae5f0;border-radius:8px;background:#fff;color:inherit;font:inherit;cursor:pointer;outline-offset:2px;transition:background .12s,border-color .12s}
.qlpm-employee-head:hover{border-color:#d5c6e5;background:#fbf8ff}
.qlpm-employee-head[aria-pressed="true"]{background:#f3ecfb;border-color:#bda0db;box-shadow:inset 3px 0 0 #6d37a3}
.qlpm-employee-head:focus-visible{outline:2px solid #7c4bb4}
.qlpm-avatar-wrap{grid-row:1;grid-column:1;position:relative;height:34px;width:34px}
.qlpm-avatar-fallback,.qlpm-avatar-image{height:34px;width:34px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-weight:800;font-size:11px;color:#fff;object-fit:cover}
.qlpm-avatar-image{position:absolute;inset:0;background:#fff}
.qlpm-person{grid-row:1;grid-column:2;display:flex;flex-direction:column;gap:2px;min-width:0}
.qlpm-person strong{font-size:12px;font-weight:800;color:#343044;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.qlpm-person small{font-size:10px;color:#8990a0;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.qlpm-person-meta{display:flex;gap:4px;flex-wrap:wrap;align-items:center;margin-top:1px}
.qlpm-person-role{color:#8290a0;font-size:10px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;max-width:172px}
.qlpm-person-pm{padding:1px 4px;font-size:9px;font-weight:750;background:#fff7e6;color:#9b6806;border:1px solid #f4dba6;border-radius:4px}
.qlpm-person-state{padding:1px 5px;border-radius:4px;border:1px solid #f6dfac;background:#fff9ec;color:#a35d10;font-size:9px;white-space:nowrap}
.qlpm-person-state.is-reported{background:#ebfaf1;border-color:#c9ecd8;color:#177c4d}
.qlpm-head-right{grid-row:2;grid-column:2;display:flex;align-items:center;gap:8px;justify-content:space-between;width:100%;min-width:0}
.qlpm-card-summary{font-size:10px;font-weight:800;color:#6f409d;white-space:nowrap;font-variant-numeric:tabular-nums}
.qlpm-edit-state{display:none;font-size:9px;color:#96601b;font-weight:700;padding:1px 5px;background:#fff8e9;border:1px solid #e9d7b9;border-radius:4px}
.qlpm-edit-state.is-active{display:inline-flex}
.qlpm-chevron{font-size:10px;color:#9a7db7;margin-left:auto;transition:transform .12s}
.qlpm-employee-head[aria-pressed="true"] .qlpm-chevron{transform:rotate(-90deg);color:#61328d}
.qlpm-employee-body{box-sizing:border-box;position:absolute;display:none;top:0;left:28%;right:0;height:100%;overflow-y:auto;min-height:0;padding:12px 20px 12px;scrollbar-width:thin;scrollbar-color:#d6cee3 transparent}
.qlpm-employee-body.is-active{display:block}
.qlpm-detail-bar{display:flex;align-items:center;justify-content:space-between;gap:12px;padding-bottom:9px;margin-bottom:10px;border-bottom:1px solid #edeaf1}
.qlpm-detail-eyebrow{display:block;margin-bottom:3px;color:#8053a9;font-size:9.5px;font-weight:800;letter-spacing:.07em;text-transform:uppercase}
.qlpm-detail-name{font-size:16px;margin:0;color:#383046;line-height:1.25;font-weight:800}
.qlpm-detail-meta{display:block;margin-top:4px;font-size:10.75px;color:#88889b}
.qlpm-detail-step{flex:none;color:#6e4890;background:#f8f4fe;border:1px solid #e9def4;padding:5px 9px;font-size:10px;border-radius:6px;font-weight:700}
.qlpm-section-heading{display:flex;align-items:center;justify-content:space-between;gap:12px;margin:0 0 10px;font-size:12px;font-weight:800;color:#4b2b70}
.qlpm-section-heading i{color:#7f51a9;margin-right:4px}
.qlpm-section-heading span{font-weight:500;font-size:10.5px;color:#93909f}
.qlpm-review-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;margin:0 0 10px}
.qlpm-time-column{min-width:0;border:1px solid #e7e2ed;border-radius:9px;padding:13px 14px;background:#fff}
.qlpm-time-title{display:block;color:#424056;font-weight:800;font-size:12px;margin-bottom:7px}
.qlpm-time-meta{display:flex;gap:6px;align-items:center;flex-wrap:wrap;margin-bottom:12px}
.qlpm-time-meta span{display:inline-flex;gap:5px;align-items:center;padding:4px 7px;background:#f7f5fa;border-radius:5px;color:#77778b;font-size:10px;white-space:nowrap}
.qlpm-time-meta b{color:#55406e;font-size:10.5px;font-weight:800}
.qlpm-time-group{display:flex;align-items:flex-end;gap:9px;width:100%}
.qlpm-part{display:flex;flex-direction:column;gap:5px;flex:1 1 0;min-width:0;color:#787588;font-size:10.5px;font-weight:650}
.qlpm-part span{padding-left:1px}
.qlpm-minutes-part{flex:0 0 79px}
.qlpm-part input{box-sizing:border-box!important;width:100%!important;min-height:42px!important;height:42px!important;padding:7px 9px!important;text-align:center!important;font-variant-numeric:tabular-nums!important;font-weight:800!important;font-size:15px!important;color:#422963!important;background:#faf9fc!important;border:1px solid #ded5ea!important;border-radius:7px!important}
.qlpm-part input:focus{background:#fff!important;border-color:#8852b6!important;box-shadow:0 0 0 3px rgba(106,62,155,.1)!important}
.qlpm-reason{padding:0;margin:0}
.qlpm-reason label{display:block;margin-bottom:7px;font-weight:750;font-size:11.5px;color:#4c435f}
.qlpm-reason label small{color:#9292a0;font-size:10.5px;font-weight:500}
.qlpm-reason textarea{box-sizing:border-box!important;width:100%;min-height:88px!important;max-height:140px!important;resize:vertical!important;padding:10px 12px!important;color:#3d3850!important;font-size:12px!important;line-height:1.5!important;border:1px solid #e2dcea!important;border-radius:8px!important;background:#fff!important}
.qlpm-reason textarea:focus{border-color:#8852b6!important;box-shadow:0 0 0 3px rgba(106,62,155,.1)!important}
.qlpm-reason.needs-reason label::after{content:'  • Cần ghi lý do';font-weight:750;color:#ae7126}
.qlpm-hint{flex:none;padding:7px 14px;border-top:1px solid var(--qlpm-line);background:#fbfafc;color:#837c8c;font-size:10.5px;line-height:1.35}
.qlpm-hint i{margin-right:3px;color:#8254a7}
.qlpm-alert-anchor{display:contents}
.qlpm-alert{flex:none;margin:8px 12px;padding:8px 10px;font-size:10.75px;line-height:1.35;border-radius:6px;background:#fff8e7;border:1px solid #edd7a5;color:#875a1a}
.qlpm-alert-suppressed{display:none!important}
.qlpm-footer-notice{flex:1 1 320px;min-width:170px;max-width:650px;margin-right:auto;color:#176944;background:#edf9f1;border:1px solid #c0e6ce;border-radius:6px;padding:7px 10px;line-height:1.35;font-weight:650;font-size:10.75px}
.qlpm-footer-notice[hidden]{display:none!important}
.qlpm-footer-total{font-size:10.5px;font-weight:650;color:#8190a0;margin-right:auto}
.qlpm-footer-notice:not([hidden])+.qlpm-footer-total{display:none!important}
.qlpm-one .qlpm-employee-grid{display:none}
.qlpm-one .qlpm-employee-body{left:0}
@media(max-width:1170px){.qlpm-overview{gap:9px}.qlpm-task-top{flex-basis:50%}.qlpm-summary{flex-basis:50%;gap:5px}.qlpm-stat{padding:6px}.qlpm-stat strong{font-size:14px}.qlpm-employee-grid{width:30%}.qlpm-employee-body{left:30%;padding:16px 18px}.qlpm-review-grid{gap:9px}.qlpm-time-column{padding:11px}}
@media(max-width:920px){.qlpm-overview{flex-wrap:wrap;padding:9px 12px}.qlpm-task-top{flex:1 1 100%}.qlpm-summary{flex:1 1 100%}.qlpm-shell{height:clamp(350px,64dvh,540px)}.qlpm-employee-grid{width:33%}.qlpm-employee-body{left:33%;padding:14px}.qlpm-review-grid{gap:8px}.qlpm-time-column{padding:9px}.qlpm-time-meta span{padding:3px 5px}.qlpm-minutes-part{flex-basis:58px}.qlpm-list-actions{display:none}}
@media(max-width:690px){#<%= mdlDuyetGioCong.ClientID %> .modal-dialog{width:calc(100vw - 12px)!important;max-width:calc(100vw - 12px)!important;margin:6px auto!important}#<%= mdlDuyetGioCong.ClientID %> .modal-content{max-height:calc(100dvh - 12px)!important}.qlpm-shell{height:calc(100dvh - 160px);min-height:300px;max-height:none}.qlpm-summary{grid-template-columns:.6fr 1fr 1fr}.qlpm-stat small{font-size:9.5px}.qlpm-employee-grid{width:100%;height:105px;overflow-x:auto;overflow-y:hidden;flex-direction:row;gap:7px;padding:8px;border-right:0;border-bottom:1px solid var(--qlpm-line)}.qlpm-employee-head{flex:0 0 235px;width:235px;min-width:235px;min-height:88px;height:88px;padding:7px}.qlpm-employee-body{top:105px;left:0;right:0;height:calc(100% - 105px);padding:12px}.qlpm-one .qlpm-employee-body{top:0;height:100%}.qlpm-review-grid{gap:7px}.qlpm-time-column{padding:8px}.qlpm-time-meta{gap:3px}.qlpm-time-meta span{font-size:9px}.qlpm-part input{height:37px!important;min-height:37px!important;font-size:13px!important}.qlpm-hint{font-size:9.5px;padding:6px 10px}.qlpm-footer-notice{flex-basis:100%;max-width:none}}
@media(max-width:430px){.qlpm-review-grid{grid-template-columns:1fr}.qlpm-stat{gap:4px}.qlpm-stat strong{font-size:12px}.qlpm-detail-name{font-size:14px}.qlpm-detail-step{display:none}.qlpm-reason textarea{min-height:76px!important}#<%= mdlDuyetGioCong.ClientID %> .modal-footer{padding:6px!important;gap:5px}#<%= mdlDuyetGioCong.ClientID %> .modal-footer .btn{font-size:10px!important;padding:5px 7px!important}}
</style>
<SweetSoft:ExtraModal ID="mdlDuyetGioCong" runat="server" Type="Primary" Title="Duyệt giờ công">
    <ContentTemplate>
        <asp:UpdatePanel ID="upDuyetGioCong" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hfProjectId" runat="server" />
                <asp:HiddenField ID="hfTaskId" runat="server" />
                <asp:HiddenField ID="hfSnapshot" runat="server" />
                <asp:HiddenField ID="hfRequestId" runat="server" />
                <asp:Panel ID="pnlShell" runat="server" CssClass="qlpm-shell">
                    <div class="qlpm-overview">
                        <div class="qlpm-task-top"><div class="qlpm-task-inline"><div class="qlpm-task-title"><asp:Literal ID="ltrTaskName" runat="server" /></div><div class="qlpm-context"><asp:Literal ID="ltrTaskStatus" runat="server" /><asp:Literal ID="ltrPmStatus" runat="server" /></div></div></div>
                        <div class="qlpm-summary">
                            <div class="qlpm-stat"><small>Nhân viên cần duyệt</small><strong><asp:Label ID="lblNhanVien" runat="server" /></strong></div>
                            <div class="qlpm-stat"><small>Tổng giờ hành chính PM duyệt</small><strong id="qlpmSumHc"><asp:Label ID="lblTongHanhChinh" runat="server" /></strong></div>
                            <div class="qlpm-stat"><small>Tổng giờ tăng ca PM duyệt</small><strong id="qlpmSumTc"><asp:Label ID="lblTongTangCa" runat="server" /></strong></div>
                        </div>
                    </div>
                    <div class="qlpm-list-title"><b><i class="fas fa-users me-2"></i>Đối chiếu theo nhân viên <asp:Label ID="lblCount" runat="server" CssClass="qlpm-count" /></b><span class="qlpm-list-actions"><i class="fas fa-info-circle"></i>Chọn nhân viên để xem và điều chỉnh chi tiết</span></div>
                    <div class="qlpm-list">
                        <div class="qlpm-employee-grid">
                            <asp:Repeater ID="repNhanVien" runat="server">
                                <ItemTemplate>
                                    <article class='qlpm-employee <%# Container.ItemIndex == 0 ? "is-open" : "" %>' data-default-hc='<%# Eval("PhutHanhChinhDeXuat") %>' data-default-tc='<%# Eval("PhutTangCaDeXuat") %>'>
                                        <asp:HiddenField ID="hfNhanVien" runat="server" Value='<%# Eval("IdNhanVien") %>' />
                                        <button class="qlpm-employee-head" type="button" aria-pressed='<%# Container.ItemIndex == 0 ? "true" : "false" %>' onclick="return QldaPmHours.select(this);" title="Xem chi tiết giờ công của nhân viên">
                                            <span class="qlpm-avatar-wrap"><%# HienAvatar(Eval("IdNhanVien"),Eval("TenNhanVien")) %></span>
                                            <span class="qlpm-person">
                                                <strong><%#: Eval("TenNhanVien") %></strong>
                                                <small><%#: Eval("Email") %></small>
                                                <span class="qlpm-person-meta"><span class="qlpm-person-role"><%# HienChucDanh(Eval("IdNhanVien")) %></span><%# HienCoPm(Eval("IdNhanVien")) %><%# HienTinhTrangBao(Eval("DaXacNhanGio")) %></span>
                                            </span>
                                            <span class="qlpm-head-right"><span class="qlpm-card-summary"><%# GiaTriSua(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %> HC · <%# GiaTriSua(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %> TC</span><span class="qlpm-edit-state">Đã điều chỉnh</span><i class="fas fa-chevron-right qlpm-chevron" aria-hidden="true"></i></span>
                                        </button>
                                        <div class="qlpm-employee-body">
                                            <div class="qlpm-detail-bar"><div><span class="qlpm-detail-eyebrow">Giờ công nhân viên</span><h3 class="qlpm-detail-name"><%#: Eval("TenNhanVien") %></h3><span class="qlpm-detail-meta"><%#: Eval("Email") %> · <%# HienChucDanh(Eval("IdNhanVien")) %></span></div><span class="qlpm-detail-step">Nhân viên <%# Container.ItemIndex + 1 %></span></div>
                                            <div class="qlpm-section-heading"><strong><i class="fas fa-user-check"></i> PM xác nhận giờ công</strong><span>Đối chiếu hệ thống và giờ nhân viên đã báo</span></div>
                                            <div class="qlpm-review-grid">
                                                <div class="qlpm-time-column"><span class="qlpm-time-title">Giờ hành chính</span><div class="qlpm-time-meta"><span>Hệ thống <b><%# HienGiay(Eval("GiayHanhChinhHeThong")) %></b></span><span>Nhân viên báo <b><%# HienPhutBao(Eval("PhutHanhChinhNhanVienBao"),Eval("DaRutBaoGanNhat")) %></b></span></div><div class="qlpm-time-group" aria-label="Giờ hành chính PM duyệt">
                                                    <label class="qlpm-part"><span>Giờ</span><asp:TextBox ID="txtHanhChinh" runat="server" CssClass="form-control qlpm-hours qlpm-part-hours" Text='<%# GiaTriSuaGio(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %>' inputmode="numeric" MaxLength="4" /></label>
                                                    <label class="qlpm-part qlpm-minutes-part"><span>Phút</span><asp:TextBox ID="txtHanhChinhPhut" runat="server" CssClass="form-control qlpm-hours qlpm-part-minutes" Text='<%# GiaTriSuaPhut(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %>' inputmode="numeric" MaxLength="2" /></label>
                                                </div></div>
                                                <div class="qlpm-time-column"><span class="qlpm-time-title">Giờ tăng ca</span><div class="qlpm-time-meta"><span>Hệ thống <b><%# HienGiay(Eval("GiayTangCaHeThong")) %></b></span><span>Nhân viên báo <b><%# HienPhutBao(Eval("PhutTangCaNhanVienBao"),Eval("DaRutBaoGanNhat")) %></b></span></div><div class="qlpm-time-group" aria-label="Giờ tăng ca PM duyệt">
                                                    <label class="qlpm-part"><span>Giờ</span><asp:TextBox ID="txtTangCa" runat="server" CssClass="form-control qlpm-hours qlpm-part-hours" Text='<%# GiaTriSuaGio(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %>' inputmode="numeric" MaxLength="4" /></label>
                                                    <label class="qlpm-part qlpm-minutes-part"><span>Phút</span><asp:TextBox ID="txtTangCaPhut" runat="server" CssClass="form-control qlpm-hours qlpm-part-minutes" Text='<%# GiaTriSuaPhut(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %>' inputmode="numeric" MaxLength="2" /></label>
                                                </div></div>
                                            </div>
                                            <div class="qlpm-reason"><label>Ghi chú điều chỉnh <small>(bắt buộc khi giờ PM duyệt khác số giờ mặc định)</small></label><asp:TextBox ID="txtLyDo" runat="server" TextMode="MultiLine" CssClass="form-control" Rows="3" Text='<%# Eval("GhiChuPmChot") %>' /></div>
                                        </div>
                                    </article>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                    <div class="qlpm-hint"><i class="fas fa-info-circle"></i> PM nhập <strong>Giờ</strong> và <strong>Phút</strong> riêng như biểu mẫu Báo hoàn thành. <strong>0 giờ 00 phút</strong> hợp lệ; giới hạn <strong>9.999 giờ 59 phút</strong>. Chốt toàn bộ nhân viên một lần, không lưu từng người riêng.</div>
                    <div class="qlpm-alert-anchor"></div>
                    <asp:Panel ID="pnlAlert" runat="server" CssClass="qlpm-alert" Visible="false"><asp:Label ID="lblAlert" runat="server" /></asp:Panel>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upFooterDuyetCong" runat="server" UpdateMode="Conditional"><ContentTemplate>
            <span class="qlpm-footer-notice" hidden="hidden" aria-live="polite"></span><asp:Label ID="lblFooter" runat="server" CssClass="qlpm-footer-total" />
            <asp:LinkButton ID="btnLamMoiDuyet" runat="server" CssClass="btn btn-outline-secondary" OnClick="btnLamMoiDuyet_Click" CausesValidation="false"><i class="fas fa-sync-alt me-1"></i> Làm mới</asp:LinkButton>
            <asp:LinkButton ID="btnNhacBaoHoanThanh" runat="server" CssClass="btn btn-outline-warning" OnClick="btnNhacBaoHoanThanh_Click" CausesValidation="false"><i class="fas fa-bell me-1"></i> Gửi nhắc nhở</asp:LinkButton>
            <asp:LinkButton ID="btnChotGioCong" runat="server" CssClass="btn btn-primary" OnClick="btnChotGioCong_Click" OnClientClick="if (!QldaPmHours.confirmOnce(this)) return false;"><i class="fas fa-check-double me-1"></i> Xác nhận duyệt giờ công</asp:LinkButton>
        </ContentTemplate></asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>
<script src="<%= ResolveUrl("~/Styles/js/kbg20-hour-input.js") %>?v=20261024p1"></script>
<script type="text/javascript">
window.QldaPmHours=window.QldaPmHours||{};
(function(){
 var api=window.QldaPmHours;
 function getModal(){return document.getElementById('<%= mdlDuyetGioCong.ClientID %>');}
 function pairFor(group){return {hours:group.querySelector('.qlpm-part-hours'),minutes:group.querySelector('.qlpm-part-minutes')};}
 function readGroup(group,validate){if(!window.Kbg20HourInput)return null;var p=pairFor(group);return validate?window.Kbg20HourInput.validatePair(p):window.Kbg20HourInput.readPair(p.hours,p.minutes);}
 function fmt(n){return Math.floor(n/60)+':'+String(n%60).padStart(2,'0');}
 function notice(message){if(window.CMSMasterJs&&typeof CMSMasterJs.ShowNotify==='function')CMSMasterJs.ShowNotify(message,'error');else window.alert(message);} 
 function syncFooterNotice(){var modal=getModal();if(!modal)return;var alert=modal.querySelector('.qlpm-alert'),footer=modal.querySelector('.modal-footer'),notice=footer?footer.querySelector('.qlpm-footer-notice'):null;if(!notice)return;var content=alert?(alert.textContent||'').trim():'';var isSuccess=content.toLowerCase().indexOf('giờ công đã được pm duyệt')!==-1;notice.hidden=!isSuccess;notice.textContent=isSuccess?content:'';if(alert)alert.classList.toggle('qlpm-alert-suppressed',isSuccess);}
 function isModalReallyOpen(element){if(!element)return false;var shown=element.classList.contains('show')||element.classList.contains('in')||element.getAttribute('aria-modal')==='true';if(!shown)return false;var style=window.getComputedStyle(element);return style.display!=='none'&&style.visibility!=='hidden';}
 function cleanOrphanBackdrop(){var modal=getModal();if(isModalReallyOpen(modal))return;var other=document.querySelectorAll('.modal');for(var i=0;i<other.length;i++){if(other[i]!==modal&&isModalReallyOpen(other[i]))return;}var all=document.querySelectorAll('.modal-backdrop');for(var j=0;j<all.length;j++)if(all[j].parentNode)all[j].parentNode.removeChild(all[j]);document.body.classList.remove('modal-open');document.body.style.removeProperty('padding-right');document.body.style.removeProperty('overflow');}
 function deferredModalCleanup(){window.setTimeout(cleanOrphanBackdrop,420);window.setTimeout(cleanOrphanBackdrop,750);}
 function installModalGuard(){var modal=getModal();if(!modal||modal._qlpmGuardInstalled)return;modal._qlpmGuardInstalled=true;modal.addEventListener('hidden.bs.modal',deferredModalCleanup);if(window.jQuery&&window.jQuery.fn)window.jQuery(modal).on('hidden.bs.modal',deferredModalCleanup);}
 function bindGlobalModalGuard(){if(api._modalGuardBound)return;api._modalGuardBound=true;document.addEventListener('click',function(e){var modal=getModal();if(!modal||!e.target||!e.target.closest)return;var clicked=e.target.closest('[data-dismiss="modal"],[data-bs-dismiss="modal"],.modal-header .close,.modal-header .btn-close');if(clicked&&modal.contains(clicked))deferredModalCleanup();},true);}

 api.activate=function(card){
   var modal=getModal();if(!modal||!card)return;
   var cards=modal.querySelectorAll('.qlpm-employee');
   for(var i=0;i<cards.length;i++){
     var active=cards[i]===card;cards[i].classList.toggle('is-open',active);
     var head=cards[i].querySelector('.qlpm-employee-head');if(head)head.setAttribute('aria-pressed',active?'true':'false');
     var detail=cards[i].querySelector('.qlpm-employee-body');if(detail)detail.classList.toggle('is-active',active);
   }
   var selected=card.querySelector('.qlpm-employee-head');
   if(selected&&selected.scrollIntoView){var area=modal.querySelector('.qlpm-employee-grid');if(area){var box=area.getBoundingClientRect(),b=selected.getBoundingClientRect();if(b.top<box.top||b.bottom>box.bottom||b.left<box.left||b.right>box.right){selected.scrollIntoView({behavior:'smooth',block:'nearest',inline:'nearest'});}}}
 };
 api.select=function(button){var card=button?button.closest('.qlpm-employee'):null;if(card)api.activate(card);return false;};
 api.toggle=api.select;
 api.confirmOnce=function(button){
   if(button.getAttribute('data-qlpm-saving')==='1')return false;
   var modal=getModal(),groups=modal?modal.querySelectorAll('.qlpm-time-group'):[];
   if(!groups.length||!window.Kbg20HourInput){notice('Chưa tải được biểu mẫu nhập Giờ/Phút. Vui lòng mở lại popup.');return false;}
   for(var i=0;i<groups.length;i++){
     var minutes=readGroup(groups[i],true);
     if(minutes===null){
       var card=groups[i].closest('.qlpm-employee');if(card)api.activate(card);
       var p=pairFor(groups[i]);
       if(p.hours&&(!p.hours.value.trim()||p.hours.classList.contains('is-invalid')))p.hours.focus();
       else if(p.minutes)p.minutes.focus();
       notice('Vui lòng nhập Giờ từ 0 đến 9.999 và Phút từ 00 đến 59 cho tất cả nhân viên.');return false;
     }
   }
   var cards=modal.querySelectorAll('.qlpm-employee');
   for(var j=0;j<cards.length;j++){
     var g=cards[j].querySelectorAll('.qlpm-time-group');if(g.length!==2)continue;
     var actualHc=readGroup(g[0],false),actualTc=readGroup(g[1],false);
     var originalHc=Number(cards[j].getAttribute('data-default-hc')),originalTc=Number(cards[j].getAttribute('data-default-tc'));
     var reason=cards[j].querySelector('.qlpm-reason textarea');
     if((actualHc!==originalHc||actualTc!==originalTc)&&reason&&!reason.value.trim()){
       api.activate(cards[j]);reason.focus();notice('PM đã điều chỉnh giờ so với đề xuất; vui lòng ghi lý do cho nhân viên tương ứng.');return false;
     }
   }
   button.setAttribute('data-qlpm-saving','1');
   window.setTimeout(function(){button.removeAttribute('data-qlpm-saving');},5000);
   // Do not disable WebForms LinkButton; the OnClick postback must always run.
   return true;
 };
 api.update=function(){
   var modal=getModal();if(!modal)return;
   var cards=modal.querySelectorAll('.qlpm-employee'),hc=0,tc=0,valid=true;
   for(var i=0;i<cards.length;i++){
     var groups=cards[i].querySelectorAll('.qlpm-time-group');if(groups.length!==2){valid=false;continue;}
     for(var g=0;g<groups.length;g++)if(window.Kbg20HourInput)window.Kbg20HourInput.bindPair(pairFor(groups[g]));
     var a=readGroup(groups[0],false),b=readGroup(groups[1],false);
     if(a===null||b===null){valid=false;continue;}
     hc+=a;tc+=b;
     var label=cards[i].querySelector('.qlpm-card-summary');if(label)label.textContent=fmt(a)+' HC · '+fmt(b)+' TC';
     var changed=a!==Number(cards[i].getAttribute('data-default-hc'))||b!==Number(cards[i].getAttribute('data-default-tc'));
     cards[i].classList.toggle('is-changed',changed);
     var pill=cards[i].querySelector('.qlpm-edit-state');if(pill)pill.classList.toggle('is-active',changed);
     var reasonBox=cards[i].querySelector('.qlpm-reason');if(reasonBox)reasonBox.classList.toggle('needs-reason',changed);
   }
   if(cards.length){var activeCard=modal.querySelector('.qlpm-employee.is-open')||cards[0];api.activate(activeCard);}
   var s=modal.querySelector('#qlpmSumHc'),t=modal.querySelector('#qlpmSumTc');
   if(s)s.textContent=valid?fmt(hc):'Kiểm tra dữ liệu';
   if(t)t.textContent=valid?fmt(tc):'Kiểm tra dữ liệu';
   syncFooterNotice();
   installModalGuard();
   bindGlobalModalGuard();
 };
 if(!api.bound){
   api.bound=true;
   document.addEventListener('input',function(e){if(e.target&&e.target.classList&&e.target.classList.contains('qlpm-hours'))api.update();});
   if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager)Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function(){api.update();deferredModalCleanup();});
 }
 if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',api.update,{once:true});else api.update();
})();
</script>
