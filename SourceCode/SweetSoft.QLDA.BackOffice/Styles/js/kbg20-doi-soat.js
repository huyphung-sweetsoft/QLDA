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
function parse(v){return window.Kbg20HourInput?window.Kbg20HourInput.minutes(v):null;}
function when(date){try{return new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(date));}catch(_){return '—';}}
function rand(){if(window.crypto&&window.crypto.randomUUID)return window.crypto.randomUUID();var b=new Uint8Array(16);if(!window.crypto||!window.crypto.getRandomValues)throw new Error('Trình duyệt chưa hỗ trợ mã thao tác an toàn.');window.crypto.getRandomValues(b);b[6]=(b[6]&15)|64;b[8]=(b[8]&63)|128;var s=Array.from(b,function(x){return x.toString(16).padStart(2,'0');}).join('');return s.slice(0,8)+'-'+s.slice(8,12)+'-'+s.slice(12,16)+'-'+s.slice(16,20)+'-'+s.slice(20);}
function get(view,task,employee){return window.fetch(url+'?view='+encodeURIComponent(view)+'&projectId='+encodeURIComponent(project)+(task?'&taskId='+encodeURIComponent(task):'')+(employee?'&employeeId='+encodeURIComponent(employee):''),{credentials:'same-origin',cache:'no-store'}).then(response);}
function response(r){return r.text().then(function(raw){
 var data=null;try{data=JSON.parse(raw);}catch(e){var err=new Error('Máy chủ không trả JSON (HTTP '+r.status+').');err.status=r.status;throw err;}
 if(!r.ok||!data.ok){var err2=new Error(data.error||data.message||'Không xử lý được.');err2.status=r.status;throw err2;}return data;
});}
function kbgNotify(text,type){if(window.CMSMasterJs&&typeof window.CMSMasterJs.ShowNotify==='function')
 window.CMSMasterJs.ShowNotify(text,type||'error');else if(window.toastr)window.toastr.error(text);}
function authNavigation(e){if(!e||(e.status!==401&&e.status!==403))return false;
 var uri=root.getAttribute('data-kbg20-access-error-url')||'/fKhaiBaoGio/Kbg20AccessDenied.aspx';
 window.location.replace(uri+(uri.indexOf('?')>=0?'&':'?')+'code='+e.status);return true;}

function post(fields){var f=new URLSearchParams();Object.keys(fields).forEach(function(k){if(fields[k]!==undefined&&fields[k]!==null)f.set(k,String(fields[k]));});f.set('projectId',project);f.set('requestId',rand());return window.fetch(url,{method:'POST',credentials:'same-origin',cache:'no-store',headers:{'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8','X-KBG20-Token':token},body:f.toString()}).then(response);}
function openModal(){
 if(window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'){window.CMSMasterJs.OpenDialog('#mdlKbg20Manual','Khai báo giờ cá nhân');return;}
 if(window.bootstrap&&window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(modal).show();return;}
 var jq=window.jQuery||window.$;if(jq&&jq.fn&&jq.fn.modal){jq(modal).modal('show');return;}
 throw new Error('Không mở được ExtraModal. Vui lòng tải lại trang.');
}
function hideModal(){
 try{
  if(window.bootstrap&&window.bootstrap.Modal){var v=window.bootstrap.Modal.getInstance(modal);if(v){v.hide();return;}}
  var jq=window.jQuery||window.$;if(jq&&jq.fn&&jq.fn.modal){jq(modal).modal('hide');return;}
 }catch(e){if(window.console)console.error(e);}
}
function statusName(s,taskStatus){
 if(s===4)return 'PM đã chốt giờ công';
 if(s===2)return 'Đã rút báo hoàn thành';
 if(s===1||s===3||s===5)return 'Đã báo hoàn thành';
 return 'Chưa báo hoàn thành';
}
function statusKind(s,locked){return locked||s===4?'is-locked':s===2?'is-withdrawn':s===1||s===3||s===5?'is-reported':'is-pending';}
function canModify(t){return (t.status===1||t.status===3||t.status===5)&&!t.pmLocked;}
function note(text,isError){var n=box.querySelector('.kbg20-dot2-notice');if(!n)n=add(box,'p','kbg20-dot2-notice');n.textContent=text||'';n.classList.toggle('is-error',!!isError);}
function footerAction(isEditable,label){footer.hidden=!isEditable;save.disabled=false;if(isEditable)save.textContent=label||'Lưu thay đổi';if(back)back.hidden=true;}
function refreshAfterSave(){hideModal();if(window.Kbg20PageUI&&window.Kbg20PageUI.refresh)window.Kbg20PageUI.refresh();if(window.KbgFloatingTimer)window.KbgFloatingTimer.notifyChanged();renderTab();}
function actionSubmit(name,task,round,version,reader){save.onclick=function(){if(busy)return;var data=reader();if(!data)return;busy=true;save.disabled=true;note('Đang lưu thay đổi...',false);var req={action:name,taskId:task,expectedRound:round,expectedVersion:version};Object.keys(data).forEach(function(k){req[k]=data[k];});post(req).then(function(r){busy=false;refreshAfterSave();var message=r.message||'Đã lưu thành công.';if(window.CMSMasterJs&&typeof window.CMSMasterJs.ShowNotify==='function')window.CMSMasterJs.ShowNotify(message,'success');else{var n=document.getElementById('kbg20Message');if(n){n.textContent=message;n.className='kbg20-ok';}}}).catch(function(e){busy=false;save.disabled=false;note('Không lưu được thay đổi.',true);kbgNotify(e.message||'Không lưu được.','error');});};}
function timeField(host,title,totalMinutes){
 if(!window.Kbg20HourInput||!window.Kbg20HourInput.createPair)throw new Error('Chưa tải bộ nhập Giờ/Phút. Vui lòng tải lại trang.');
 return window.Kbg20HourInput.createPair(host,title,totalMinutes);
}
function statusChip(host,status,locked,taskStatus){return add(host,'span','kbg20-ui-badge '+statusKind(status,locked),statusName(status,taskStatus));}
function statPair(host,title,sys,declared){var col=add(host,'div','kbg20-ui-compare-cell');add(col,'span','kbg20-ui-compare-label',title);var a=add(col,'div','kbg20-ui-compare-values');var sysRow=add(a,'span','');add(sysRow,'small','','Hệ thống');add(sysRow,'strong','',format(sys));var dRow=add(a,'span','');add(dRow,'small','','Tự khai');add(dRow,'strong','',declared==null?'—':format(declared));return col;}
function historyPanel(host,data){var pane=add(host,'aside','kbg20-ui-history-panel');var hd=add(pane,'div','kbg20-ui-history-head');add(hd,'strong','', 'Lịch sử báo hoàn thành & chỉnh giờ');add(hd,'span','',String((data.history||[]).length)+' lần ghi nhận');
 var items=add(pane,'div','kbg20-ui-history-list');items.setAttribute('tabindex','0');items.setAttribute('aria-label','Danh sách lịch sử thao tác của công việc');
 if(!data.history||!data.history.length){add(items,'p','kbg20-ui-history-empty','Chưa có lịch sử. Sau khi báo hoàn thành, những lần báo, hủy xác nhận và chỉnh giờ sẽ được lưu tại đây.');return;}
 data.history.forEach(function(x){var item=add(items,'div','kbg20-ui-history-item');var head=add(item,'div','kbg20-ui-history-item-head');add(head,'strong','',x.editedAfterConfirm?'Đã điều chỉnh giờ đã báo':statusName(x.kind));add(head,'time','',when(x.createdUtc));add(item,'div','kbg20-ui-history-version','Lần '+x.round+' · Phiên bản '+x.version);
 if(x.regular!=null||x.overtime!=null)add(item,'div','kbg20-ui-history-hours','Giờ tự khai: '+(x.regular==null?'—':format(x.regular))+' hành chính · '+(x.overtime==null?'—':format(x.overtime))+' tăng ca');if(x.reason)add(item,'p','kbg20-ui-history-reason',x.reason);});}
function infoCard(host,title,desc){var e=add(host,'section','kbg20-ui-hours-card');add(e,'h4','',title);add(e,'p','kbg20-ui-hours-caption',desc);return e;}
function displayPair(host,regular,overtime){var wrap=add(host,'div','kbg20-ui-hour-pair');var r=add(wrap,'div','');add(r,'span','', 'Giờ hành chính');add(r,'strong','',regular==null?'—':format(regular));var o=add(wrap,'div','');add(o,'span','', 'Tăng ca');add(o,'strong','',overtime==null?'—':format(overtime));}
function load(task,mode,employee){if(busy)return;var id=++seq;current=task;try{openModal();}catch(e){kbgNotify(e.message,'error');return;}footerAction(false);box.textContent='';add(box,'p','kbg20-ui-loading','Đang tải giờ công và lịch sử...');
 get(employee?'pm-personal':'personal',task,employee).then(function(d){if(current!==task||seq!==id)return;renderDetail(task,d,employee?'pm-read':mode);}).catch(function(e){if(seq!==id)return;
 // Lỗi quyền xem thông báo PM chỉ là quyền THAO TÁC; không đổi trang đang xem.
 box.textContent='';note('Không tải được chi tiết.');kbgNotify(e.message||'Không tải được chi tiết.','error');});}
function renderDetail(task,data,mode){
 var taskPage=window.Kbg20PageUI&&window.Kbg20PageUI.getTask&&window.Kbg20PageUI.getTask(task),item=all.filter(function(x){return x.taskId===task;})[0];
 var name=taskPage?('['+(taskPage.code||'CV')+'] '+(taskPage.name||'')):item?('['+(item.code||'CV')+'] '+(item.name||'')):'Công việc đã chọn';
 if(mode==='pm-read'){
    var params=new URLSearchParams(window.location.search),round=Number(params.get('pmViewRound')),ver=Number(params.get('pmViewVersion'));
    var hit=(data.history||[]).find(function(x){return Number(x.round)===round && Number(x.version)===ver;});
    if(hit){data.status=hit.kind;data.declaredRegular=hit.regular;data.declaredOvertime=hit.overtime;data.selectedNote=hit.reason||'Không có';data.selectedDate=hit.createdUtc;}
    name+=(data.employeeName?' · '+data.employeeName:'');
 }
 if(mode==='report'&&(data.pmLocked||data.status===1||data.status===3||data.status===5||[1,4,2,3].indexOf(data.taskStatus)<0))mode='read';
 if(mode==='confirm'&&!data.canConfirm)mode='read';
 if(mode==='edit'&&(!data.canEdit||data.pmLocked))mode='read';
 if(mode==='withdraw'&&(!data.canWithdraw||data.pmLocked))mode='read';
 var editing=mode==='edit',report=mode==='report',confirmHours=mode==='confirm',withdraw=mode==='withdraw',pmRead=mode==='pm-read';
 var isForm=editing||report||confirmHours;
 box.textContent='';footerAction(false);
 var top=add(box,'div','kbg20-ui-top');var a=add(top,'div','kbg20-ui-top-title');add(a,'div','kbg20-ui-eyebrow',pmRead?'CHI TIẾT BÁO / RÚT · PM CHỈ XEM':confirmHours?'BÁO HOÀN THÀNH PHẦN VIỆC':withdraw?'RÚT BÁO HOÀN THÀNH':report?'BÁO HOÀN THÀNH PHẦN VIỆC':editing?'ĐIỀU CHỈNH GIỜ CÔNG':'THÔNG TIN GIỜ CÔNG');add(a,'h3','',name);statusChip(top,data.status,data.pmLocked,data.taskStatus);
 var workspace=add(box,'div','kbg20-ui-workspace'),left=add(workspace,'div','kbg20-ui-main'),right=add(workspace,'div','kbg20-ui-side');
 var systemReg=Number(data.systemRegularMinutes)||0,systemOt=Number(data.systemOvertimeMinutes)||0;
 if(report&&window.Kbg20PageUI&&window.Kbg20PageUI.getLiveMinutes){var live=window.Kbg20PageUI.getLiveMinutes(task);if(live){systemReg=Math.max(systemReg,live.regular);systemOt=Math.max(systemOt,live.overtime);}}
 var cards=add(left,'div','kbg20-ui-cards');var sys=infoCard(cards,'Giờ hệ thống','Thời gian hệ thống đã ghi nhận');displayPair(sys,systemReg,systemOt);
 var manual=infoCard(cards,pmRead?'Giờ nhân viên báo':'Giờ tôi tự khai',report||confirmHours?'Mặc định theo giờ hệ thống cuối cùng; có thể điều chỉnh.':editing?'Đây là bản báo giờ đang có hiệu lực.':'Bản giờ đã được xác nhận (chưa PM chốt).');
 var rInput=null,oInput=null;
 if(isForm){var rrow=add(manual,'div','kbg20-ui-input-pair');rInput=timeField(rrow,'Giờ hành chính',report||confirmHours?systemReg:data.declaredRegular);oInput=timeField(rrow,'Giờ tăng ca',report||confirmHours?systemOt:data.declaredOvertime);}
 else displayPair(manual,data.declaredRegular,data.declaredOvertime);
 if(withdraw){var help=add(left,'div','kbg20-ui-help is-warning');add(help,'strong','', 'Ông/bà muốn tiếp tục thực hiện công việc này?');add(help,'p','', 'Thao tác sẽ hủy xác nhận hoàn thành hiện tại, lưu lịch sử và thông báo PM. Đồng hồ sẽ không tự chạy lại; hãy chủ động bấm Bắt đầu khi tiếp tục làm.');}
 if(report){var info=add(left,'div','kbg20-ui-help');add(info,'p','', 'Báo xong phần việc chỉ dừng đồng hồ của bạn trên Task này; không dừng đồng nghiệp.');}
 if(confirmHours){var info2=add(left,'div','kbg20-ui-help is-warning');add(info2,'strong','', 'Task chung đã chờ duyệt hoặc hoàn thành');add(info2,'p','', 'Bạn báo hoàn thành phần việc và khai số giờ đã làm. Thao tác không mở đồng hồ và không thay đổi trạng thái chung của Task.');}
 if(pmRead){var only=add(left,'div','kbg20-ui-help');add(only,'strong','', 'PM chỉ xem · bản ghi từ thông báo');
   add(only,'p','', 'Thời điểm: '+(data.selectedDate?when(data.selectedDate):'Không xác định'));
   add(only,'p','', 'Lý do / ghi chú nhân viên: '+(data.selectedNote||'Không có'));
   add(only,'p','', 'Chỉ đọc. Không có chức năng sửa hoặc duyệt giờ từ popup thông báo.');}
 var reason=null;
 if(report||editing||withdraw||confirmHours){var field=add(left,'label','kbg20-ui-reason');add(field,'span','',withdraw?'Lý do rút báo (bắt buộc)':editing?'Lý do / ghi chú sửa giờ':'Ghi chú / lý do chênh lệch (nếu có)');reason=add(field,'textarea','');reason.rows=2;reason.maxLength=2000;reason.placeholder=withdraw?'Ví dụ: Tôi cần hoàn thiện thêm một số nội dung...':editing?'Ví dụ: Điều chỉnh số giờ tự khai sau khi kiểm tra...':'Ghi chú thêm nếu cần...';}
 if(data.pmLocked){var lock=add(left,'div','kbg20-ui-help is-locked');add(lock,'strong','', 'PM đã chốt giờ công');add(lock,'p','', 'Bản tự khai hiện chỉ được xem. Bạn không thể sửa hoặc rút báo hoàn thành sau khi PM đã chốt giờ.');}
 historyPanel(right,data);
 if(report||editing||withdraw||confirmHours){footerAction(true,withdraw?'Xác nhận rút báo':report?'Báo hoàn thành phần việc':confirmHours?'Báo hoàn thành phần việc':'Lưu giờ tự khai');actionSubmit(withdraw?'withdrawPersonal':report?'reportPersonal':confirmHours?'confirmPersonalHours':'editPersonal',task,data.round,data.version,function(){
   if(withdraw){if(!reason.value.trim()){note('Vui lòng nhập lý do hủy xác nhận.',true);return null;}return {reason:reason.value.trim()};}
   var reg=window.Kbg20HourInput&&window.Kbg20HourInput.validatePair(rInput),ot=window.Kbg20HourInput&&window.Kbg20HourInput.validatePair(oInput);if(reg===null||ot===null){note('Vui lòng nhập riêng số Giờ (0–9.999) và số Phút (00–59).',true);return null;}return {regularMinutes:reg,overtimeMinutes:ot,reason:reason.value.trim()};
  });}
}
function amountColumn(host,title,system,self){var content=add(host,'div','kbg20-ui-cell-times');add(content,'small','kbg20-v41-mobile-label',title);var sys=add(content,'div','kbg20-ui-cell-sys');add(sys,'span','', 'Hệ thống');add(sys,'strong','',format(system));var d=add(content,'div','kbg20-ui-cell-self');add(d,'span','', 'Tự khai');add(d,'strong','',self==null?'—':format(self));}
function renderItems(){if(!list)return;list.textContent='';document.querySelectorAll('#kbg20ManualFilters [data-manual-filter]').forEach(function(b){var selected=b.getAttribute('data-manual-filter')===filter;b.classList.toggle('is-selected',selected);b.setAttribute('aria-pressed',String(selected));});
 var rows=all.filter(function(x){return filter==='all'||filter==='reported'&&(x.status===1||x.status===3||x.status===4||x.status===5)||filter==='withdrawn'&&x.status===2;});
 if(!rows.length){add(list,'p','kbg20-ui-empty','Chưa có công việc phù hợp. Các công việc bạn báo hoàn thành sẽ hiển thị tại đây, kèm số giờ và lịch sử chỉnh sửa.');return;}
 var hd=add(list,'div','kbg20-ui-table-head kbg20-ui-table-grid');['Công việc','Giờ hành chính','Giờ tăng ca','Báo hoàn thành','Thao tác'].forEach(function(v){add(hd,'span','',v);});
 rows.forEach(function(t){var line=add(list,'article','kbg20-ui-table-row kbg20-ui-table-grid');var name=add(line,'div','kbg20-ui-cell-task');add(name,'strong','', '['+(t.code||'CV')+'] '+(t.name||''));add(name,'span','', 'Lần báo '+t.round+' · Phiên bản '+t.version);
 amountColumn(line,'Giờ hành chính',t.systemRegularMinutes,t.declaredRegular);amountColumn(line,'Giờ tăng ca',t.systemOvertimeMinutes,t.declaredOvertime);
 var st=add(line,'div','kbg20-ui-cell-status');statusChip(st,t.status,t.pmLocked,t.taskStatus);if(t.status===2 && t.taskStatus===1)add(st,'small','', 'Có thể báo lại nếu đang thực hiện');else if(t.pmLocked)add(st,'small','', 'Giờ tự khai đã khóa chỉnh sửa');
 var actions=add(line,'div','kbg20-ui-cell-actions');function button(label,mode,cls){var b=add(actions,'button','kbg20-ui-action '+(cls||''),label);b.type='button';b.addEventListener('click',function(){origin='manual';load(t.taskId,mode);});}
 var finished=t.taskStatus===4||t.taskStatus===2||t.taskStatus===3;
 if(t.pmLocked||t.status===4){button('Xem giờ','read','is-neutral');}
 else if(finished && (t.status===0||t.status===2)){button('Báo hoàn thành phần việc','report','is-primary');}
 else if(t.status===2 && t.taskStatus===1){button('Báo lại','report','is-primary');}
 else if(canModify(t)){button('Sửa giờ','edit','is-primary');if(t.taskStatus===1 && (t.status===1||t.status===3))button('Rút báo','withdraw','is-secondary');}
 else {button('Xem giờ','read','is-neutral');}
 });}
function renderTab(){if(!list||busy)return;busy=true;list.textContent='';add(list,'p','kbg20-ui-empty','Đang tải giờ tự khai...');get('personal-list').then(function(r){all=r.items||[];renderItems();}).catch(function(e){
 if(authNavigation(e))return;
 list.textContent='';add(list,'p','kbg20-ui-empty','Tạm thời không tải được bảng kê giờ công.');
 kbgNotify(e.message||'Không tải được Tab 3.','error');
}).then(function(){busy=false;});}
var filters=document.getElementById('kbg20ManualFilters');if(filters)filters.addEventListener('click',function(e){var b=e.target.closest('[data-manual-filter]');if(b){filter=b.getAttribute('data-manual-filter');renderItems();}});
if(back)back.onclick=hideModal;
window.Kbg20DoiSoatUI={renderTab:renderTab,openReport:function(task){origin='work';load(task,'report');},openWithdraw:function(task){origin='work';load(task,'withdraw');},openConfirm:function(task){origin='manual';load(task,'confirm');},openTask:function(task){origin='manual';load(task,'edit');}};
// Link thong bao PM den dung Task/nhan vien, API kiem tra quyen PM thuc su.
function fromPmNotification(){
 var q=new URLSearchParams(window.location.search);
 var employeeTask=q.get('reportTaskId')||q.get('confirmHoursTaskId');
 if(employeeTask && !window.__kbgReportReminderShown){window.__kbgReportReminderShown=true;
    window.setTimeout(function(){if(window.Kbg20PageUI&&Kbg20PageUI.openWork)Kbg20PageUI.openWork();load(employeeTask,'report');},300);return;}
 var t=q.get('pmViewTaskId'),e=q.get('pmViewEmployeeId');
 if(!t||!e)return;var sig=t+'|'+e;if(window.__kbgPmViewed===sig)return;
 window.__kbgPmViewed=sig;window.setTimeout(function(){load(t,'pm-read',e);},220);
}
if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',fromPmNotification);else fromPmNotification();
})(window,document);
