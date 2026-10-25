<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDuyetGioCongTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlDuyetGioCongTask" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<style>
/* Scoped exclusively to CtrlDuyetGioCongTask; all CSS selectors remain compact. */
#<%= mdlDuyetGioCong.ClientID %> .modal-dialog{width:min(1260px,calc(100vw - 32px))!important;max-width:calc(100vw - 32px)!important;margin:12px auto!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-dialog:has(.qlpm-one){width:min(875px,calc(100vw - 32px))!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-content{max-height:calc(100dvh - 24px)!important;display:flex!important;flex-direction:column!important;overflow:hidden}
#<%= mdlDuyetGioCong.ClientID %> .modal-header{flex-shrink:0!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-body{min-height:0!important;overflow:hidden!important;padding:0!important;flex:1 1 auto!important}
#<%= mdlDuyetGioCong.ClientID %> .modal-footer{position:relative;z-index:5;flex-shrink:0!important;background:#fff!important;border-top:1px solid #e7e1ef!important;padding:10px 14px!important;display:flex;align-items:center;justify-content:flex-end;flex-wrap:wrap;gap:6px}
#<%= mdlDuyetGioCong.ClientID %> .modal-footer .btn{margin:0!important;min-height:34px;font-size:12px}
.qlpm-shell{--qlpm-line:#e7e2ed;display:flex;flex-direction:column;min-height:0;max-height:calc(100dvh - 141px);font-size:12.5px;color:#263246;background:#fff;overflow:hidden}
.qlpm-overview{flex:none;padding:11px 17px;background:linear-gradient(110deg,#fbf8ff,#fff);border-bottom:1px solid var(--qlpm-line)}
.qlpm-task-title{font-size:15px;font-weight:800;color:#33224e;line-height:1.35;overflow-wrap:anywhere;margin:0 0 6px}
.qlpm-context{display:flex;align-items:center;gap:7px;flex-wrap:wrap}
.qlpm-task-overdue{font-size:10.5px;font-weight:750;color:#dc2626}
.qlpm-shell .badge-pill-custom{padding:4px 8px!important;border-radius:4px!important;font-size:11px!important;font-weight:600!important;display:inline-block!important;white-space:nowrap!important;line-height:1.2!important}
.qlpm-shell .badge-status-todo{background:#f1f5f9!important;color:#475569!important;border:1px solid #cbd5e1!important}
.qlpm-shell .badge-status-doing{background:#e0f2fe!important;color:#0369a1!important;border:1px solid #bae6fd!important}
.qlpm-shell .badge-status-done{background:#dcfce7!important;color:#15803d!important;border:1px solid #bbf7d0!important}
.qlpm-shell .badge-status-review{background:#fef3c7!important;color:#92400e!important;border:1px solid #fcd34d!important}
.qlpm-pm-status{display:inline-flex;align-items:center;gap:5px;padding:3px 8px;font-size:10.5px;font-weight:750;color:#68408c;border:1px solid #e8d8f4;background:#f6effd;border-radius:5px}
.qlpm-pm-status.is-final{color:#13784b;background:#eefaf2;border-color:#c2e8d4}
.qlpm-count{font-size:11.5px;font-weight:650;color:#718096}
.qlpm-summary{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:8px;margin-top:10px}
.qlpm-stat{display:flex;align-items:center;justify-content:space-between;gap:8px;min-width:0;background:#fff;border:1px solid #ece6f3;border-radius:8px;padding:8px 12px}
.qlpm-stat small{font-size:11px;font-weight:650;color:#727987}
.qlpm-stat strong{font-size:16px;font-weight:800;color:#57228e;font-variant-numeric:tabular-nums;white-space:nowrap}
.qlpm-alert{flex:none;margin:8px 15px 2px;border-radius:7px;padding:8px 10px;background:#fff8e7;border:1px solid #efd9a7;color:#805917;font-size:11.5px;font-weight:600}
.qlpm-list-title{flex:none;display:flex;align-items:center;justify-content:space-between;gap:10px;padding:9px 17px 5px;color:#4a2774;font-weight:800;font-size:12.5px}
.qlpm-list-actions{display:flex;align-items:center;gap:8px;color:#748093;font-size:10.5px;font-weight:500}
.qlpm-collapse-all{border:1px solid #dcd0ea;border-radius:6px;padding:4px 8px;background:#fff;color:#653294;font-size:11px;font-weight:700;cursor:pointer}
.qlpm-collapse-all:hover{background:#f6effd;border-color:#b696d7}
.qlpm-list{flex:1 1 auto;min-height:100px;overflow-y:auto;overscroll-behavior:contain;padding:0 14px 11px;scrollbar-width:thin;scrollbar-color:#d6c6e7 transparent}
.qlpm-employee-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));align-items:start;gap:9px}
.qlpm-employee{min-width:0;border:1px solid #e8e1f0;border-radius:10px;overflow:hidden;background:#fff;transition:border-color .18s ease,box-shadow .18s ease}
.qlpm-employee:hover{border-color:#d8c5ec}
.qlpm-employee.is-open{border-color:#cbb3e8;box-shadow:0 3px 10px rgba(74,35,119,.045)}
.qlpm-employee-head{display:flex;align-items:center;gap:9px;width:100%;text-align:left;padding:10px 11px;border:0;background:#f9f7fc;cursor:pointer;font:inherit;color:inherit}
.qlpm-employee-head:hover{background:#f4eef9}
.qlpm-avatar-wrap{position:relative;width:36px;height:36px;flex:0 0 36px}
.qlpm-avatar-image,.qlpm-avatar-fallback{width:36px;height:36px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:800;color:#fff;object-fit:cover}
.qlpm-avatar-image{position:absolute;inset:0;background:#fff}
.qlpm-person{display:flex;flex-direction:column;flex:1;min-width:0;gap:1px}
.qlpm-person strong{font-size:12px;font-weight:800;color:#2e3545;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.qlpm-person small{color:#7b8291;font-size:10.5px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.qlpm-person-meta{display:flex;align-items:center;flex-wrap:wrap;gap:5px;margin-top:1px}
.qlpm-person-role{color:#61738b;font-size:10px;max-width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.qlpm-person-pm{color:#b17a00;font-weight:800;font-size:10px;border:1px solid #f3db9b;border-radius:5px;background:#fff7df;padding:1px 5px}
.qlpm-person-state{font-size:9.5px;color:#a35d10;border:1px solid #f6dfac;background:#fff9ec;padding:2px 5px;border-radius:5px;white-space:nowrap}
.qlpm-person-state.is-reported{color:#16784f;background:#eefbf4;border-color:#c8ecd8}
.qlpm-head-right{display:flex;flex-direction:column;align-items:flex-end;gap:4px;flex:none;max-width:40%;color:#806b95;font-size:10px}
.qlpm-card-summary{font-size:10px;color:#6d597f;font-weight:650;white-space:nowrap;font-variant-numeric:tabular-nums}
.qlpm-chevron{font-size:12px;transition:transform .15s ease}
.qlpm-employee.is-open .qlpm-chevron{transform:rotate(180deg)}
.qlpm-employee-body{display:none}
.qlpm-employee.is-open .qlpm-employee-body{display:block}
.qlpm-compare{border-collapse:collapse;width:100%;table-layout:fixed}
.qlpm-compare td,.qlpm-compare th{border-bottom:1px solid #f1edf6;padding:6px 10px;vertical-align:middle}
.qlpm-compare th{color:#778197;font-size:10.5px;font-weight:700;text-align:left;background:#fff}
.qlpm-compare .qlpm-label{width:32%;color:#5c6472;font-size:10.5px;font-weight:680}
.qlpm-compare td{font-size:11.5px;color:#354052;font-variant-numeric:tabular-nums}
.qlpm-compare .qlpm-edit-row{background:#faf6ff}
.qlpm-compare .qlpm-edit-row td{border-bottom:0}
.qlpm-time-group{display:flex;align-items:flex-end;gap:6px;max-width:230px}
.qlpm-part{display:flex;flex-direction:column-reverse;gap:3px;flex:1;min-width:0;color:#776884;font-size:9.5px;font-weight:700}
.qlpm-part span{padding-left:2px}
.qlpm-minutes-part{flex:0 0 54px}
.qlpm-hours{box-sizing:border-box!important;min-width:0;width:100%!important;max-width:none!important;height:34px!important;min-height:34px!important;padding:4px 6px!important;font-size:14px!important;font-family:inherit!important;font-weight:750!important;text-align:center!important;font-variant-numeric:tabular-nums!important;color:#4f237f!important;border:1px solid #ceb7e9!important;border-radius:7px!important;background:#fff!important;outline-offset:2px}
.qlpm-hours:focus{border-color:#8051b2!important;box-shadow:0 0 0 2px rgba(120,73,171,.14)!important}
.qlpm-hours.is-invalid{border-color:#dc3545!important;background:#fff7f7!important}
.qlpm-hours[readonly]{background:#f1f2f5!important;border-color:#e1e1e6!important;color:#596276!important}
.qlpm-reason{padding:7px 10px 9px}
.qlpm-reason label{display:block;font-size:10.5px;font-weight:700;color:#657087;margin-bottom:4px}
.qlpm-reason textarea{min-height:37px!important;max-height:100px!important;resize:vertical!important;font-size:11.5px!important;border-color:#e1d6ef!important;border-radius:7px!important}
.qlpm-hint{flex:none;padding:7px 15px;background:#faf7fd;color:#786c82;border-top:1px solid #f0eaf4;font-size:10px;line-height:1.4}
.qlpm-footer-total{font-size:11px;font-weight:650;color:#768196;margin-right:auto}
@media(max-width:900px){.qlpm-employee-grid{grid-template-columns:1fr}.qlpm-person-state{display:inline-block}.qlpm-list-actions span{display:none}}
@media(max-width:700px){#<%= mdlDuyetGioCong.ClientID %> .modal-dialog{width:calc(100vw - 14px)!important;max-width:calc(100vw - 14px)!important;margin:7px auto!important}#<%= mdlDuyetGioCong.ClientID %> .modal-content{max-height:calc(100dvh - 14px)!important}.qlpm-shell{max-height:calc(100dvh - 155px)}.qlpm-overview{padding:9px 10px}.qlpm-task-title{font-size:13px}.qlpm-summary{grid-template-columns:1fr 1fr}.qlpm-stat{padding:6px 8px}.qlpm-stat:first-child{display:none}.qlpm-stat small{font-size:9.5px}.qlpm-stat strong{font-size:13px}.qlpm-list{padding:0 7px 9px}.qlpm-list-title{padding:8px}.qlpm-compare td,.qlpm-compare th{padding:5px}.qlpm-compare .qlpm-label{width:29%}.qlpm-minutes-part{flex-basis:45px}.qlpm-time-group{gap:4px}.qlpm-hours{font-size:12px!important}#<%= mdlDuyetGioCong.ClientID %> .modal-footer{padding:7px!important}#<%= mdlDuyetGioCong.ClientID %> .modal-footer .btn{font-size:11px!important;padding:5px!important}.qlpm-hint{font-size:9px}}
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
                        <div class="qlpm-task-title"><asp:Literal ID="ltrTaskName" runat="server" /></div>
                        <div class="qlpm-context">
                            <asp:Literal ID="ltrTaskStatus" runat="server" />
                            <asp:Literal ID="ltrPmStatus" runat="server" />
                            <asp:Label ID="lblCount" runat="server" CssClass="qlpm-count" />
                        </div>
                        <div class="qlpm-summary">
                            <div class="qlpm-stat"><small>Nhân viên cần duyệt</small><strong><asp:Label ID="lblNhanVien" runat="server" /></strong></div>
                            <div class="qlpm-stat"><small>Tổng giờ hành chính PM duyệt</small><strong id="qlpmSumHc"><asp:Label ID="lblTongHanhChinh" runat="server" /></strong></div>
                            <div class="qlpm-stat"><small>Tổng giờ tăng ca PM duyệt</small><strong id="qlpmSumTc"><asp:Label ID="lblTongTangCa" runat="server" /></strong></div>
                        </div>
                    </div>
                    <asp:Panel ID="pnlAlert" runat="server" CssClass="qlpm-alert" Visible="false"><asp:Label ID="lblAlert" runat="server" /></asp:Panel>
                    <div class="qlpm-list-title">
                        <span><i class="fas fa-users me-2"></i>Đối chiếu theo nhân viên</span>
                        <div class="qlpm-list-actions"><span>Hệ thống · Nhân viên báo · PM duyệt</span><button type="button" class="qlpm-collapse-all" onclick="return QldaPmHours.toggleAll(this);">Mở tất cả</button></div>
                    </div>
                    <div class="qlpm-list">
                        <div class="qlpm-employee-grid">
                            <asp:Repeater ID="repNhanVien" runat="server">
                                <ItemTemplate>
                                    <article class='qlpm-employee <%# Container.ItemIndex == 0 ? "is-open" : "" %>' data-default-hc='<%# Eval("PhutHanhChinhDeXuat") %>' data-default-tc='<%# Eval("PhutTangCaDeXuat") %>'>
                                        <asp:HiddenField ID="hfNhanVien" runat="server" Value='<%# Eval("IdNhanVien") %>' />
                                        <button class="qlpm-employee-head" type="button" aria-expanded='<%# Container.ItemIndex == 0 ? "true" : "false" %>' onclick="return QldaPmHours.toggle(this);">
                                            <span class="qlpm-avatar-wrap"><%# HienAvatar(Eval("IdNhanVien"),Eval("TenNhanVien")) %></span>
                                            <span class="qlpm-person">
                                                <strong><%#: Eval("TenNhanVien") %></strong>
                                                <small><%#: Eval("Email") %></small>
                                                <span class="qlpm-person-meta"><span class="qlpm-person-role"><%# HienChucDanh(Eval("IdNhanVien")) %></span><%# HienCoPm(Eval("IdNhanVien")) %><%# HienTinhTrangBao(Eval("DaXacNhanGio")) %></span>
                                            </span>
                                            <span class="qlpm-head-right"><span class="qlpm-card-summary"><%# GiaTriSua(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %> HC · <%# GiaTriSua(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %> TC</span><i class="fas fa-chevron-down qlpm-chevron" aria-hidden="true"></i></span>
                                        </button>
                                        <div class="qlpm-employee-body">
                                            <table class="qlpm-compare">
                                                <thead><tr><th class="qlpm-label">Nguồn giờ công</th><th>Giờ hành chính</th><th>Giờ tăng ca</th></tr></thead>
                                                <tbody>
                                                    <tr><td class="qlpm-label">Hệ thống ghi nhận</td><td><%# HienGiay(Eval("GiayHanhChinhHeThong")) %></td><td><%# HienGiay(Eval("GiayTangCaHeThong")) %></td></tr>
                                                    <tr><td class="qlpm-label">Nhân viên báo</td><td><%# HienPhutBao(Eval("PhutHanhChinhNhanVienBao"),Eval("DaRutBaoGanNhat")) %></td><td><%# HienPhutBao(Eval("PhutTangCaNhanVienBao"),Eval("DaRutBaoGanNhat")) %></td></tr>
                                                    <tr class="qlpm-edit-row"><td class="qlpm-label"><strong>PM duyệt</strong></td>
                                                        <td><div class="qlpm-time-group" aria-label="Giờ hành chính PM duyệt">
                                                            <label class="qlpm-part"><asp:TextBox ID="txtHanhChinh" runat="server" CssClass="form-control qlpm-hours qlpm-part-hours" Text='<%# GiaTriSuaGio(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %>' inputmode="numeric" MaxLength="4" /><span>Giờ</span></label>
                                                            <label class="qlpm-part qlpm-minutes-part"><asp:TextBox ID="txtHanhChinhPhut" runat="server" CssClass="form-control qlpm-hours qlpm-part-minutes" Text='<%# GiaTriSuaPhut(Eval("PhutHanhChinhDeXuat"),Eval("PhutHanhChinhPmDaChot")) %>' inputmode="numeric" MaxLength="2" /><span>Phút</span></label>
                                                        </div></td>
                                                        <td><div class="qlpm-time-group" aria-label="Giờ tăng ca PM duyệt">
                                                            <label class="qlpm-part"><asp:TextBox ID="txtTangCa" runat="server" CssClass="form-control qlpm-hours qlpm-part-hours" Text='<%# GiaTriSuaGio(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %>' inputmode="numeric" MaxLength="4" /><span>Giờ</span></label>
                                                            <label class="qlpm-part qlpm-minutes-part"><asp:TextBox ID="txtTangCaPhut" runat="server" CssClass="form-control qlpm-hours qlpm-part-minutes" Text='<%# GiaTriSuaPhut(Eval("PhutTangCaDeXuat"),Eval("PhutTangCaPmDaChot")) %>' inputmode="numeric" MaxLength="2" /><span>Phút</span></label>
                                                        </div></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="qlpm-reason"><label>Ghi chú điều chỉnh (bắt buộc nếu khác số giờ mặc định)</label><asp:TextBox ID="txtLyDo" runat="server" TextMode="MultiLine" CssClass="form-control" Rows="2" Text='<%# Eval("GhiChuPmChot") %>' /></div>
                                        </div>
                                    </article>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                    <div class="qlpm-hint"><i class="fas fa-info-circle me-1"></i>PM nhập <strong>Giờ</strong> và <strong>Phút</strong> riêng như biểu mẫu Báo hoàn thành. 0 giờ 00 phút hợp lệ, giới hạn 9.999 giờ 59 phút. Chốt toàn bộ nhân viên một lần, không lưu từng người riêng.</div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upFooterDuyetCong" runat="server" UpdateMode="Conditional"><ContentTemplate>
            <asp:Label ID="lblFooter" runat="server" CssClass="qlpm-footer-total" />
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
 api.confirmOnce=function(button){
   if(button.getAttribute('data-qlpm-saving')==='1')return false;
   var modal=getModal(),groups=modal?modal.querySelectorAll('.qlpm-time-group'):[];
   if(!groups.length||!window.Kbg20HourInput){notice('Chưa tải được biểu mẫu nhập Giờ/Phút. Vui lòng mở lại popup.');return false;}
   for(var i=0;i<groups.length;i++){
     // Expand first, otherwise invalid field inside a collapsed card cannot receive focus.
     var minutes=readGroup(groups[i],true);
     if(minutes===null){
       var card=groups[i].closest('.qlpm-employee');
       if(card){card.classList.add('is-open');var head=card.querySelector('.qlpm-employee-head');if(head)head.setAttribute('aria-expanded','true');}
       var p=pairFor(groups[i]);if(p.hours&&!p.hours.value.trim())p.hours.focus();else if(p.minutes&&!p.minutes.value.trim())p.minutes.focus();
       notice('Vui lòng nhập Giờ từ 0 đến 9.999 và Phút từ 00 đến 59 cho tất cả nhân viên.');return false;
     }
   }
   var cards=modal.querySelectorAll('.qlpm-employee');
   for(var j=0;j<cards.length;j++){
     var g=cards[j].querySelectorAll('.qlpm-time-group');
     if(g.length!==2)continue;
     var actualHc=readGroup(g[0],false),actualTc=readGroup(g[1],false);
     var originalHc=Number(cards[j].getAttribute('data-default-hc')),originalTc=Number(cards[j].getAttribute('data-default-tc'));
     var reason=cards[j].querySelector('.qlpm-reason textarea');
     if((actualHc!==originalHc||actualTc!==originalTc)&&reason&&!reason.value.trim()){
       cards[j].classList.add('is-open');var h=cards[j].querySelector('.qlpm-employee-head');if(h)h.setAttribute('aria-expanded','true');
       reason.focus();notice('PM đã điều chỉnh giờ so với đề xuất; vui lòng ghi lý do cho nhân viên tương ứng.');return false;
     }
   }
   button.setAttribute('data-qlpm-saving','1');
   window.setTimeout(function(){button.removeAttribute('data-qlpm-saving');},5000);
   // KHONG disable LinkButton, de __doPostBack ASP.NET duoc thuc hien.
   return true;
 };
 api.toggle=function(button){var card=button.closest('.qlpm-employee');if(!card)return false;var open=card.classList.toggle('is-open');button.setAttribute('aria-expanded',open?'true':'false');return false;};
 api.toggleAll=function(button){var modal=getModal();if(!modal)return false;var cards=modal.querySelectorAll('.qlpm-employee');var open=Array.prototype.some.call(cards,function(x){return !x.classList.contains('is-open');});Array.prototype.forEach.call(cards,function(card){card.classList.toggle('is-open',open);var h=card.querySelector('.qlpm-employee-head');if(h)h.setAttribute('aria-expanded',open?'true':'false');});button.textContent=open?'Thu gọn tất cả':'Mở tất cả';return false;};
 api.update=function(){
   var modal=getModal();if(!modal)return;
   var cards=modal.querySelectorAll('.qlpm-employee'),hc=0,tc=0,valid=true;
   for(var i=0;i<cards.length;i++){
     var groups=cards[i].querySelectorAll('.qlpm-time-group');if(groups.length!==2){valid=false;continue;}
     for(var g=0;g<groups.length;g++)if(window.Kbg20HourInput)window.Kbg20HourInput.bindPair(pairFor(groups[g]));
     var a=readGroup(groups[0],false),b=readGroup(groups[1],false);
     if(a===null||b===null){valid=false;continue;}hc+=a;tc+=b;
     var label=cards[i].querySelector('.qlpm-card-summary');if(label)label.textContent=fmt(a)+' HC · '+fmt(b)+' TC';
   }
   var s=modal.querySelector('#qlpmSumHc'),t=modal.querySelector('#qlpmSumTc');
   if(s)s.textContent=valid?fmt(hc):'Kiểm tra dữ liệu';
   if(t)t.textContent=valid?fmt(tc):'Kiểm tra dữ liệu';
 };
 if(!api.bound){
   api.bound=true;
   document.addEventListener('input',function(e){if(e.target&&e.target.classList&&e.target.classList.contains('qlpm-hours'))api.update();});
   document.addEventListener('focusin',function(e){if(e.target&&e.target.classList&&e.target.classList.contains('qlpm-hours'))api.update();});
   if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager)Sys.WebForms.PageRequestManager.getInstance().add_endRequest(api.update);
 }
 if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',api.update,{once:true});else api.update();
})();
</script>
