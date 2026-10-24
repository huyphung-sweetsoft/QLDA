/* KhaiBaoGio 2.0 / DOT 2 - UX HOTFIX + runtime FIX V3
   Nguon API va nghiep vu khong thay doi. Tab 3: doi chieu he thong/tu khai,
   mot nut Sua (kem lich su), popup SweetSoft:ExtraModal hai cot. */
(function(window,document){
'use strict';
var root=document.getElementById('kbg20Page'),modal=document.getElementById('mdlKbg20Manual');
if(!root||!modal)return;
var url=root.getAttribute('data-api-url'),project=root.getAttribute('data-project-id'),token=root.getAttribute('data-request-token');
var box=document.getElementById('kbg20ReconcileContent'),footer=document.getElementById('kbg20ReconcileFooterActions');
var save=document.getElementById('kbg20ManualSave'),back=document.getElementById('kbg20ManualBack');
var list=document.getElementById('kbg20ManualList'),filter='all',all=[],busy=false,current=null,origin='work',seq=0;
function el(tag,cls,txt){var e=document.createElement(tag);if(cls)e.className=cls;if(txt!==undefined)e.textContent=txt;return e;}
function add(host,tag,cls,txt){var e=el(tag,cls,txt);host.appendChild(e);return e;}
function format(m){m=Math.max(0,Math.floor(Number(m)||0));return String(Math.floor(m/60)).padStart(2,'0')+':'+String(m%60).padStart(2,'0');}
function round(s){return Math.floor((Math.max(0,Number(s)||0)+30)/60);}
function parse(v){var a=/^\s*(\d{1,6}):([0-5]\d)\s*$/.exec(v||'');return a?Number(a[1])*60+Number(a[2]):null;}
function when(date){try{return new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(date));}catch(_){return '—';}}
function rand(){if(window.crypto&&window.crypto.randomUUID)return window.crypto.randomUUID();var b=new Uint8Array(16);if(!window.crypto||!window.crypto.getRandomValues)throw new Error('Trình duyệt chưa hỗ trợ mã thao tác an toàn.');window.crypto.getRandomValues(b);b[6]=(b[6]&15)|64;b[8]=(b[8]&63)|128;var s=Array.from(b,function(x){return x.toString(16).padStart(2,'0');}).join('');return s.slice(0,8)+'-'+s.slice(8,12)+'-'+s.slice(12,16)+'-'+s.slice(16,20)+'-'+s.slice(20);}
function get(view,task){return window.fetch(url+'?view='+encodeURIComponent(view)+'&projectId='+encodeURIComponent(project)+(task?'&taskId='+encodeURIComponent(task):''),{credentials:'same-origin',cache:'no-store'}).then(response);}
function response(r){return r.json().catch(function(){throw new Error('Máy chủ không trả dữ liệu hợp lệ. Vui lòng tải lại trang; nếu lỗi tiếp diễn, liên hệ quản trị.');}).then(function(x){
 if(!x||typeof x!=='object')throw new Error('Dữ liệu trả về không hợp lệ. Vui lòng tải lại trang.');
 if(!r.ok||!x.ok)throw new Error(x.error||x.message||'Không thực hiện được thao tác. Vui lòng thử lại.');
 return x;});}
function post(fields){var f=new URLSearchParams();Object.keys(fields).forEach(function(k){if(fields[k]!==undefined&&fields[k]!==null)f.set(k,String(fields[k]));});f.set('projectId',project);f.set('requestId',rand());return window.fetch(url,{method:'POST',credentials:'same-origin',cache:'no-store',headers:{'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8','X-KBG20-Token':token},body:f.toString()}).then(response);}
function rebindModal(){
 modal=document.getElementById('mdlKbg20Manual');
 box=document.getElementById('kbg20ReconcileContent');
 footer=document.getElementById('kbg20ReconcileFooterActions');
 save=document.getElementById('kbg20ManualSave');
 back=document.getElementById('kbg20ManualBack');
 if(back)back.onclick=hideModal;
}
function openModal(){
 /* Khung modal duoc mo bang SweetSoft:ExtraModal.OpenModal(true) tren server,
    giong CtrlChonNhanVienTask. Dung UpdatePanel de khong reload trang. */
 var prm=window.Sys&&window.Sys.WebForms&&window.Sys.WebForms.PageRequestManager&&
     window.Sys.WebForms.PageRequestManager.getInstance();
 var trigger=document.querySelector('[id$="btnKbg20OpenManual"]');
 if(prm&&trigger){
  if(prm.get_isInAsyncPostBack())return Promise.reject(new Error('Trang đang xử lý yêu cầu trước. Vui lòng chờ và thử lại.'));
  return new Promise(function(resolve,reject){
   var done=false;
   var finish=function(err){if(done)return;done=true;prm.remove_endRequest(ended);rebindModal();
     if(err)reject(err);else resolve();};
   var ended=function(sender,args){var e=args&&args.get_error&&args.get_error();if(e){
     if(args.set_errorHandled)args.set_errorHandled(true);finish(e);
   }else finish(null);};
   prm.add_endRequest(ended);
   try{trigger.click();}catch(ex){finish(ex);}
  });
 }
 /* Fallback for isolated UI demo/test; actual WebForms page uses the bridge. */
 if(window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'){
  window.CMSMasterJs.OpenDialog('#mdlKbg20Manual','Thông tin giờ công');return Promise.resolve();
 }
 return Promise.reject(new Error('Không thể mở ExtraModal qua ASP.NET AJAX.'));
}
function hideModal(){
 try{
  if(window.bootstrap&&window.bootstrap.Modal){var v=window.bootstrap.Modal.getInstance(modal);if(v){v.hide();return;}}
  var jq=window.jQuery||window.$;if(jq&&jq.fn&&jq.fn.modal){jq(modal).modal('hide');return;}
 }catch(e){if(window.console)console.error(e);}
}
function statusName(s){return s===4?'Đã được chốt công':s===2?'Đã rút báo hoàn thành':s===3?'Đã báo hoàn thành':s===1?'Đã báo hoàn thành':'Chưa báo hoàn thành';}
function historyActionName(kind){return kind===1?'Báo hoàn thành phần việc':kind===2?'Rút báo hoàn thành':kind===3?'Chỉnh sửa giờ đã báo':kind===4?'Quản lý chốt giờ công':'Thao tác giờ công';}
function statusKind(s,locked){return locked||s===4?'is-locked':s===2?'is-withdrawn':s===1||s===3?'is-reported':'is-pending';}
function canModify(t){return (t.status===1||t.status===3)&&t.status!==4&&!t.pmLocked;}
function note(text,isError){var n=box.querySelector('.kbg20-dot2-notice');if(!n)n=add(box,'p','kbg20-dot2-notice');n.textContent=text||'';n.classList.toggle('is-error',!!isError);}
function footerAction(isEditable,label){footer.hidden=!isEditable;save.disabled=false;if(isEditable)save.textContent=label||'Lưu thay đổi';if(back)back.hidden=true;}
function refreshAfterSave(){hideModal();if(window.Kbg20PageUI&&window.Kbg20PageUI.refresh)window.Kbg20PageUI.refresh();if(window.KbgFloatingTimer)window.KbgFloatingTimer.notifyChanged();renderTab();}
function actionSubmit(name,task,round,version,reader){save.onclick=function(){if(busy)return;var data=reader();if(!data)return;busy=true;save.disabled=true;note('Đang lưu thay đổi...',false);var req={action:name,taskId:task,expectedRound:round,expectedVersion:version};Object.keys(data).forEach(function(k){req[k]=data[k];});post(req).then(function(r){busy=false;refreshAfterSave();var message=r.message||'Đã lưu thành công.';if(window.CMSMasterJs&&typeof window.CMSMasterJs.ShowNotify==='function')window.CMSMasterJs.ShowNotify(message,'success');else{var n=document.getElementById('kbg20Message');if(n){n.textContent=message;n.className='kbg20-ok';}}}).catch(function(e){busy=false;save.disabled=false;note(e.message||'Không lưu được.',true);});};}
function timeField(host,title,value){var l=add(host,'label','kbg20-ui-time-field');add(l,'span','',title);var i=add(l,'input','kbg20-ui-time-input');i.type='text';i.inputMode='numeric';i.value=value;i.maxLength=9;i.placeholder='00:00';i.autocomplete='off';i.setAttribute('aria-label',title+', định dạng giờ hai chấm phút');return i;}
function statusChip(host,status,locked){var e=add(host,'span','kbg20-ui-badge '+statusKind(status,false),status===4?'Đã báo hoàn thành':statusName(status));
 if(locked||status===4)add(host,'span','kbg20-ui-locked-note','Giờ công đã được quản lý chốt');return e;}
function statPair(host,title,sys,declared){var col=add(host,'div','kbg20-ui-compare-cell');add(col,'span','kbg20-ui-compare-label',title);var a=add(col,'div','kbg20-ui-compare-values');var sysRow=add(a,'span','');add(sysRow,'small','','Hệ thống');add(sysRow,'strong','',format(sys));var dRow=add(a,'span','');add(dRow,'small','','Nhân viên báo');add(dRow,'strong','',declared==null?'—':format(declared));return col;}
function historyPanel(host,data){var pane=add(host,'aside','kbg20-ui-history-panel');var hd=add(pane,'div','kbg20-ui-history-head');add(hd,'strong','', 'Lịch sử báo hoàn thành và chỉnh giờ');add(hd,'span','',String((data.history||[]).length)+' lần ghi nhận');
 var items=add(pane,'div','kbg20-ui-history-list');items.setAttribute('tabindex','0');items.setAttribute('aria-label','Danh sách lịch sử thao tác của công việc');
 if(!data.history||!data.history.length){add(items,'p','kbg20-ui-history-empty','Chưa có lịch sử. Những lần báo hoàn thành, rút báo và sửa giờ công sẽ hiển thị tại đây.');return;}
 data.history.forEach(function(x){var item=add(items,'div','kbg20-ui-history-item');var head=add(item,'div','kbg20-ui-history-item-head');add(head,'strong','',historyActionName(x.kind));add(head,'time','',when(x.createdUtc));add(item,'div','kbg20-ui-history-version','Lần '+x.round+' · Phiên bản '+x.version);
 if(x.regular!=null||x.overtime!=null)add(item,'div','kbg20-ui-history-hours','Giờ nhân viên báo: '+(x.regular==null?'—':format(x.regular))+' theo lịch · '+(x.overtime==null?'—':format(x.overtime))+' tăng ca');if(x.reason)add(item,'p','kbg20-ui-history-reason',x.reason);});}
function infoCard(host,title,desc){var e=add(host,'section','kbg20-ui-hours-card');add(e,'h4','',title);add(e,'p','kbg20-ui-hours-caption',desc);return e;}
function displayPair(host,regular,overtime){var wrap=add(host,'div','kbg20-ui-hour-pair');var r=add(wrap,'div','');add(r,'span','', 'Giờ làm theo lịch');add(r,'strong','',regular==null?'—':format(regular));var o=add(wrap,'div','');add(o,'span','', 'Giờ tăng ca');add(o,'strong','',overtime==null?'—':format(overtime));}
function load(task,mode){if(busy)return;var id=++seq;current=task;
 openModal().then(function(){
  if(current!==task||seq!==id)return null;
  footerAction(false);box.textContent='';add(box,'p','kbg20-ui-loading','Đang tải giờ công và lịch sử...');
  return get('personal',task);
 }).then(function(d){if(!d||current!==task||seq!==id)return;renderDetail(task,d,mode);})
 .catch(function(e){if(seq!==id)return;rebindModal();box.textContent='';note(e.message||'Không tải được thông tin giờ công.',true);
  var n=document.getElementById('kbg20Message');if(n)n.textContent=e.message||'Không mở được thông tin giờ công.';});}

function renderDetail(task,data,mode){
 var taskPage=window.Kbg20PageUI&&window.Kbg20PageUI.getTask&&window.Kbg20PageUI.getTask(task),item=all.filter(function(x){return x.taskId===task;})[0];
 var name=taskPage?('['+(taskPage.code||'CV')+'] '+(taskPage.name||'')):item?('['+(item.code||'CV')+'] '+(item.name||'')):'Công việc đã chọn';
 if(mode==='report'&&(data.pmLocked||data.status===1||data.status===3))mode='read';
 if(mode==='edit'&&(!data.canEdit||data.pmLocked))mode='read';
 if(mode==='withdraw'&&(!data.canEdit||data.pmLocked))mode='read';
 var editing=mode==='edit',report=mode==='report',withdraw=mode==='withdraw',isForm=editing||report;
 box.textContent='';footerAction(false);
 var top=add(box,'div','kbg20-ui-top');var a=add(top,'div','kbg20-ui-top-title');add(a,'div','kbg20-ui-eyebrow',withdraw?'RÚT BÁO HOÀN THÀNH':report?'BÁO HOÀN THÀNH PHẦN VIỆC':editing?'SỬA GIỜ ĐÃ BÁO':'THÔNG TIN GIỜ CÔNG');add(a,'h3','',name);statusChip(top,data.status,data.pmLocked);
 var workspace=add(box,'div','kbg20-ui-workspace'),left=add(workspace,'div','kbg20-ui-main'),right=add(workspace,'div','kbg20-ui-side');
 var systemReg=Number(data.systemRegularMinutes)||0,systemOt=Number(data.systemOvertimeMinutes)||0;
 if(report&&window.Kbg20PageUI&&window.Kbg20PageUI.getLiveMinutes){var live=window.Kbg20PageUI.getLiveMinutes(task);if(live){systemReg=Math.max(systemReg,live.regular);systemOt=Math.max(systemOt,live.overtime);}}
 var cards=add(left,'div','kbg20-ui-cards');var sys=infoCard(cards,'Giờ hệ thống ghi nhận','Thời gian hệ thống đã ghi nhận');displayPair(sys,systemReg,systemOt);
 var manual=infoCard(cards,'Giờ nhân viên báo',report?'Điền sẵn theo giờ hệ thống hiện tại. Có thể điều chỉnh trước khi gửi.':editing?'Đây là số giờ đã báo và đang có hiệu lực.':'Số giờ đã báo gần nhất');
 var rInput=null,oInput=null;
 if(isForm){var rrow=add(manual,'div','kbg20-ui-input-pair');rInput=timeField(rrow,'Giờ làm theo lịch',format(report?systemReg:data.declaredRegular));oInput=timeField(rrow,'Giờ tăng ca',format(report?systemOt:data.declaredOvertime));}
 else displayPair(manual,data.declaredRegular,data.declaredOvertime);
 if(withdraw){var help=add(left,'div','kbg20-ui-help is-warning');add(help,'strong','', 'Rút báo hoàn thành phần việc?');add(help,'p','', 'Lần báo hoàn thành sẽ hết hiệu lực và người quản lý được thông báo. Giờ công đã ghi vẫn được giữ nguyên. Đồng hồ không tự bật lại.');}
 if(report){var info=add(left,'div','kbg20-ui-help');add(info,'p','', 'Khi gửi, hệ thống dừng đồng hồ của bạn trên công việc này. Số giờ bạn báo có hiệu lực ngay; thời gian của đồng nghiệp không bị ảnh hưởng.');}
 var reason=null;
 if(report||editing||withdraw){var field=add(left,'label','kbg20-ui-reason');add(field,'span','',withdraw?'Lý do rút báo (bắt buộc)':editing?'Lý do điều chỉnh giờ đã báo':'Ghi chú thêm (không bắt buộc)');reason=add(field,'textarea','');reason.rows=2;reason.maxLength=2000;reason.placeholder=withdraw?'Ví dụ: Cần bổ sung phần kiểm tra chất lượng...':editing?'Ví dụ: Điều chỉnh sau khi đối chiếu thời gian thực hiện...':'Ghi chú thêm nếu cần...';}
 if(data.pmLocked){var lock=add(left,'div','kbg20-ui-help is-locked');add(lock,'strong','', 'Đã được chốt công');add(lock,'p','', 'Quản lý đã chốt giờ công. Bạn chỉ có thể xem, không thể chỉnh sửa hoặc rút báo.');}
 historyPanel(right,data);
 if(report||editing||withdraw){footerAction(true,withdraw?'Rút báo hoàn thành':report?'Gửi báo hoàn thành':'Lưu giờ đã báo');actionSubmit(withdraw?'withdrawPersonal':report?'reportPersonal':'editPersonal',task,data.round,data.version,function(){
   if(withdraw){if(!reason.value.trim()){note('Vui lòng nhập lý do rút báo hoàn thành.',true);return null;}return {reason:reason.value.trim()};}
   var reg=parse(rInput.value),ot=parse(oInput.value);if(reg===null||ot===null||reg>52560000||ot>52560000){note('Giờ phải ở dạng HH:mm, ví dụ 07:30, phút từ 00 đến 59.',true);return null;}return {regularMinutes:reg,overtimeMinutes:ot,reason:reason.value.trim()};
  });}
}
function amountColumn(host,title,system,self){var content=add(host,'div','kbg20-ui-cell-times');add(content,'small','kbg20-v41-mobile-label',title);var sys=add(content,'div','kbg20-ui-cell-sys');add(sys,'span','', 'Hệ thống');add(sys,'strong','',format(system));var d=add(content,'div','kbg20-ui-cell-self');add(d,'span','', 'Nhân viên báo');add(d,'strong','',self==null?'—':format(self));}
function renderItems(){if(!list)return;list.textContent='';document.querySelectorAll('#kbg20ManualFilters [data-manual-filter]').forEach(function(b){var selected=b.getAttribute('data-manual-filter')===filter;b.classList.toggle('is-selected',selected);b.setAttribute('aria-pressed',String(selected));});
 var rows=all.filter(function(x){return filter==='all'||filter==='reported'&&(x.status===1||x.status===3||x.status===4)||filter==='withdrawn'&&x.status===2;});
 if(!rows.length){add(list,'p','kbg20-ui-empty','Không có bản kê giờ công phù hợp với bộ lọc. Hãy báo hoàn thành phần việc tại mục Theo dõi giờ làm để ghi nhận giờ đã báo.');return;}
 var hd=add(list,'div','kbg20-ui-table-head kbg20-ui-table-grid');['Công việc','Giờ làm theo lịch','Giờ tăng ca','Báo cáo phần việc','Hành động'].forEach(function(v){add(hd,'span','',v);});
 rows.forEach(function(t){var line=add(list,'article','kbg20-ui-table-row kbg20-ui-table-grid');var name=add(line,'div','kbg20-ui-cell-task');add(name,'strong','', '['+(t.code||'CV')+'] '+(t.name||''));add(name,'span','', 'Lần báo '+t.round+' · Phiên bản '+t.version);
 amountColumn(line,'Giờ làm theo lịch',t.systemRegularMinutes,t.declaredRegular);amountColumn(line,'Giờ tăng ca',t.systemOvertimeMinutes,t.declaredOvertime);
 var st=add(line,'div','kbg20-ui-cell-status');statusChip(st,t.status,t.pmLocked);if(t.status===2)add(st,'small','', 'Có thể tiếp tục làm việc; báo hoàn thành tại mục Theo dõi giờ làm');else if(t.pmLocked)add(st,'small','', 'Quản lý đã chốt công; không thể sửa');
 var actions=add(line,'div','kbg20-ui-cell-actions');function button(label,mode,cls){var b=add(actions,'button','kbg20-ui-action '+(cls||''),label);b.type='button';b.addEventListener('click',function(){origin='manual';load(t.taskId,mode);});}
 if(canModify(t)){button('Sửa','edit','is-primary');}
 else {var disabled=add(actions,'button','kbg20-ui-action is-disabled','Sửa');disabled.type='button';disabled.disabled=true;
 disabled.title=t.status===2?'Đã rút báo hoàn thành. Chỉ có thể báo lại ở mục Theo dõi giờ làm.':
 t.pmLocked||t.status===4?'Quản lý đã chốt công; không thể chỉnh sửa.':'Chưa có bản báo hoàn thành hợp lệ.';
 disabled.setAttribute('aria-label','Không thể sửa giờ: '+disabled.title);}
 });}
function renderTab(){if(!list||busy)return;busy=true;list.textContent='';add(list,'p','kbg20-ui-empty','Đang tải bảng kê giờ công...');get('personal-list').then(function(r){all=r.items||[];renderItems();}).catch(function(e){list.textContent='';add(list,'p','kbg20-reconcile-error',e.message||'Không tải được bảng kê giờ công.');}).then(function(){busy=false;});}
var filters=document.getElementById('kbg20ManualFilters');if(filters)filters.addEventListener('click',function(e){var b=e.target.closest('[data-manual-filter]');if(b){filter=b.getAttribute('data-manual-filter');renderItems();}});
if(back)back.onclick=hideModal;
window.Kbg20DoiSoatUI={renderTab:renderTab,openReport:function(task){origin='work';load(task,'report');},openWithdraw:function(task){origin='work';load(task,'withdraw');},openTask:function(task){origin='manual';load(task,'edit');}};
})(window,document);
