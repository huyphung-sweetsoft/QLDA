/* KBG20 Doi soat: chi dung trong #kbg20Page + TEST workflow enable. Khong sua V1. */
(function (window,document) {
'use strict';
var root=document.getElementById('kbg20Page');
var modal=document.getElementById('mdlKbg20Manual');
if(!root || !modal) return;
var url=root.getAttribute('data-api-url'),projectId=root.getAttribute('data-project-id');
var token=root.getAttribute('data-request-token'),box=document.getElementById('kbg20ReconcileContent');
var btnOpen=document.getElementById('kbg20ReconcileListOpen');
var footerActions=document.getElementById('kbg20ReconcileFooterActions');
var backButton=document.getElementById('kbg20ManualBack');
var saveButton=document.getElementById('kbg20ManualSave');
function showDetailActions(visible){if(footerActions)footerActions.hidden=!visible;}

var activeTask=null,version=null,loading=false,manualItems=[],manualFilter='all',manualBusy=false;
// MasterTemplate.ShowNotify (giong ShowNotify(MSGType.Success) khi them Task).
// API luu bang fetch nen su dung doi tac client CMSMasterJs.ShowNotify cua MasterTemplate.
function showSaveNotify(wasEdit){
 var text=wasEdit
   ? 'Đã cập nhật giờ khai báo thành công. Lịch sử điều chỉnh vẫn được bảo lưu.'
   : 'Đã lưu giờ khai báo thành công.';
 if(window.CMSMasterJs&&typeof window.CMSMasterJs.ShowNotify==='function'){
   window.CMSMasterJs.ShowNotify(text,'success');
 } else if(window.toastr&&typeof window.toastr.success==='function'){
   window.toastr.success(text);
 } else {
   // Du phong neu script toast cua MasterTemplate chua nap.
   var messageBox=document.getElementById('kbg20Message');
   if(messageBox){messageBox.textContent=text;messageBox.className='kbg20-ok';}
 }
}
// ExtraModal co the co Width attribute nhung modal-dialog van bi Bootstrap gioi han max-width.
function expandManualDialog(){
 var dialog=modal.querySelector('.modal-dialog') || (modal.classList.contains('modal-dialog')?modal:null);
 if(dialog){
   dialog.style.setProperty('width','min(1160px, calc(100vw - 32px))','important');
   dialog.style.setProperty('max-width','min(1160px, calc(100vw - 32px))','important');
 }
}
function formatEditDate(milliseconds){
 if(milliseconds==null||!isFinite(Number(milliseconds))||Number(milliseconds)<=0)return 'Chưa có thời điểm ghi nhận';
 try {return new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(Number(milliseconds)));}
 catch(_){return 'Chưa có thời điểm ghi nhận';}
}
function make(tag,cls,text) {var x=document.createElement(tag);if(cls)x.className=cls;if(text!==undefined)x.textContent=text;return x;}
function add(parent,tag,cls,text){var x=make(tag,cls,text);parent.appendChild(x);return x;}
// Phut he thong do API quy doi tu TONG giay (>=30 giay lam tron len).
// Fallback cho cache API cu; khong bao gio lam tron tung phien hay tung ngay.
function systemMinutes(item,type){
 var key=type==='regular'?'systemRegularMinutes':'systemOvertimeMinutes';
 if(item[key]!==null && item[key]!==undefined)return Math.max(0,Number(item[key])||0);
 return Math.round(Math.max(0,Number(item[type])||0)/60);
}
function minutesString(minutes){minutes=Math.max(0,Math.floor(Number(minutes)||0));return String(Math.floor(minutes/60)).padStart(2,'0')+':'+String(minutes%60).padStart(2,'0');}
function systemHHmm(item,type){return minutesString(systemMinutes(item,type));}
function parseHhmm(text) {var m=/^\s*(\d{1,6}):([0-5]\d)\s*$/.exec(text||'');return m ? Number(m[1])*60+Number(m[2]) : null;}
// CHỈ dùng modal chuẩn của partner (CMSMasterJs.OpenDialog + bootstrap modal).
// Không render overlay/div dialog tự tạo. Không bỏ logic token API.
function display(){
  var jq=window.jQuery||window.$;
  if(!window.CMSMasterJs || typeof window.CMSMasterJs.OpenDialog!=='function' ||
      !jq || !jq.fn || typeof jq.fn.modal!=='function') {
    throw new Error('ExtraModal chưa được nạp từ MasterTemplate.');
  }
  expandManualDialog();
  window.CMSMasterJs.OpenDialog('#mdlKbg20Manual','Khai báo giờ thủ công');
}
function dismiss(){
  showDetailActions(false);
  var jq=window.jQuery||window.$;
  if(jq && jq.fn && jq.fn.modal)jq('#mdlKbg20Manual').modal('hide');
  activeTask=null;version=null;
}
var jq=window.jQuery||window.$;
if(jq)jq(modal).on('hidden.bs.modal',function(){activeTask=null;version=null;showDetailActions(false);});
function getJson(resp){return resp.json().then(function(x){if(!resp.ok || !x.ok)throw new Error(x.error||x.message||'Không tải được dữ liệu.');return x;});}
function get(view,task){var q='?view='+encodeURIComponent(view)+'&projectId='+encodeURIComponent(projectId);
 if(task)q+='&taskId='+encodeURIComponent(task);
 return window.fetch(url+q,{credentials:'same-origin',cache:'no-store'}).then(getJson);}
function message(text,isError){showDetailActions(false);box.textContent='';add(box,'p',isError?'kbg20-reconcile-error':'kbg20-reconcile-info',text);}
function renderList(items){activeTask=null;showDetailActions(false);box.textContent='';
 add(box,'p','kbg20-reconcile-info','Sau khi công việc hoàn thành được PM xác nhận, bạn có thể khai và chỉnh sửa giờ của chính mình. Giờ hệ thống và lịch sử chỉnh sửa được bảo lưu.');
 if(!items.length){add(box,'p','kbg20-reconcile-empty','Chưa có công việc hoàn thành nào sẵn sàng khai báo giờ trong dự án này.');return;}
 items.forEach(function(t){
  var a=add(box,'button','kbg20-reconcile-item');a.type='button';
  add(a,'strong','', '['+(t.code||'')+'] '+(t.name||''));
  add(a,'span','', 'Đợt duyệt '+t.round+' · Phiên bản '+t.version+' · '+(t.submitted?'Đã khai giờ':'Chưa khai giờ'));
  add(a,'small','', 'Hệ thống (HH:mm): Trong ca '+systemHHmm(t,'regular')+' · Tăng ca '+systemHHmm(t,'overtime'));
  a.addEventListener('click',function(){openTask(t.taskId);});
 });
}
function openList(){if(loading)return;showDetailActions(false);try{display();}catch(e){var m=document.getElementById("kbg20Message");if(m)m.textContent=e.message;return;}loading=true;message('Đang tải danh sách công việc...',false);
 get('reconcile-list').then(function(x){renderList(x.items||[]);}).catch(function(e){message(e.message,true);}).then(function(){loading=false;});}
function labeledInput(parent,title,value,id){
 var field=add(parent,'label','kbg20-reconcile-field');
 add(field,'span','',title);
 var input=add(field,'input','kbg20-time-mask');input.type='text';input.id=id;
 input.inputMode='numeric';input.maxLength=9;input.value=value;input.placeholder='00:00';
 input.autocomplete='off';input.setAttribute('aria-label',title+'; nhập các chữ số theo định dạng HH:mm');
 // Gõ liên tiếp 4 chữ số 0230 => 02:30. Không tự đổi giá trị gốc trước khi người dùng sửa.
 input.addEventListener('input',function(){
   var raw=input.value;
   if(/^\d{1,6}:[0-5]?\d?$/.test(raw))return;
   var digits=raw.replace(/\D/g,'').slice(0,8);
   if(!digits){input.value='';return;}
   if(digits.length<=2)input.value=digits;
   else if(digits.length<=4)input.value=digits.slice(0,2)+':'+digits.slice(2);
   else input.value=digits.slice(0,-2)+':'+digits.slice(-2);
 });
 input.addEventListener('keydown',function(e){
   if(e.key==='Backspace' && input.selectionStart===input.selectionEnd &&
      input.selectionStart===input.value.length && input.value.endsWith(':')){
     e.preventDefault();input.value=input.value.slice(0,-1);}
 });
 input.addEventListener('blur',function(){
   var m=/^(\d{1,6}):([0-5]?\d)$/.exec(input.value);
   if(m)input.value=m[1].padStart(2,'0')+':'+m[2].padStart(2,'0');
 });
 add(field,'small','kbg20-reconcile-format','Ví dụ: gõ 0230 → 02:30');
 return input;
}
function renderDetail(d){activeTask=d.taskId;version=d.version;box.textContent='';showDetailActions(true);
 var bar=add(box,'div','kbg20-reconcile-title');add(bar,'strong','', '['+(d.code||'')+'] '+(d.name||''));
 add(bar,'span','', 'Đợt duyệt '+d.round+' · Phiên bản '+d.version);
 var layout=add(box,'div','kbg20-reconcile-detail-layout');
 var left=add(layout,'div','kbg20-reconcile-detail-left');
 var right=add(layout,'aside','kbg20-reconcile-history-panel');
 right.setAttribute('aria-label','Lịch sử điều chỉnh giờ khai báo');
 var grid=add(left,'div','kbg20-reconcile-grid');
 var regular=add(grid,'div','kbg20-reconcile-group');add(regular,'h4','', 'Giờ trong ca');
 add(regular,'small','', 'Giờ hệ thống (HH:mm): '+systemHHmm(d,'regular'));
 var regInput=labeledInput(regular,'Thời gian khai báo (HH:mm)',
   d.declaredRegular==null?systemHHmm(d,'regular'):minutesString(d.declaredRegular),'kbg20ManualRegular');
 var ot=add(grid,'div','kbg20-reconcile-group');add(ot,'h4','', 'Tăng ca');
 add(ot,'small','', 'Giờ hệ thống (HH:mm): '+systemHHmm(d,'overtime'));
 var otInput=labeledInput(ot,'Thời gian khai báo (HH:mm)',
    d.declaredOvertime==null?systemHHmm(d,'overtime'):minutesString(d.declaredOvertime),'kbg20ManualOvertime');
 var field=add(left,'label','kbg20-reconcile-field');add(field,'span','', 'Ghi chú điều chỉnh (không bắt buộc)');
 var reason=add(field,'textarea','');reason.rows=3;reason.maxLength=2000;reason.value=d.reason||'';
 var status=add(left,'div','kbg20-reconcile-status');
 // FooterTemplate cua SweetSoft:ExtraModal giu cac nut sat nut Dong mac dinh.
 // Gan onclick (thay vi addEventListener) de khong nhan doi listener sau moi lan renderDetail.
 var back=backButton, save=saveButton;
 if(!back || !save){status.textContent='Khong tim thay nut thao tac trong FooterTemplate.';return;}
 back.onclick=openList;
 save.disabled=false;
 save.onclick=function(){
  if(loading)return;
  var r=parseHhmm(regInput.value),o=parseHhmm(otInput.value);
  if(r===null || o===null || r>52560000 || o>52560000){status.textContent='Giờ phải theo định dạng HH:mm, phút trong khoảng 00–59.';return;}
  loading=true;save.disabled=true;status.textContent='Đang lưu...';
  var form=new URLSearchParams();form.set('action','saveReconcile');form.set('projectId',projectId);
  form.set('taskId',activeTask);form.set('regularMinutes',String(r));form.set('overtimeMinutes',String(o));
  form.set('expectedVersion',String(version));form.set('reason',reason.value||'');
  window.fetch(url,{method:'POST',credentials:'same-origin',cache:'no-store',
    headers:{'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8','X-KBG20-Token':token},body:form.toString()})
    .then(getJson).then(function(){
      // Chi sau khi API xac nhan ghi thanh cong: dong ExtraModal va tai lai TAB khai bao.
      // Khong reload trang, khong mo lai popup va khong can goi reconcile chi tiet.
      loading=false;
      var wasEdit=d.declaredRegular!=null&&d.declaredOvertime!=null;
      var jq=window.jQuery||window.$;
      // Thong bao dung toast cua MasterTemplate, KHONG mo ExtraModal thu hai.
      // Khi modal dong xong moi goi ShowNotify de khong bi backdrop che.
      var modalVisible=jq&&jq.fn&&typeof jq.fn.modal==='function'&&
        (jq(modal).hasClass('show')||jq(modal).hasClass('in'));
      if(modalVisible){
        jq(modal).one('hidden.bs.modal.kbg20Saved',function(){showSaveNotify(wasEdit);});
      }
      dismiss();
      if(!modalVisible)showSaveNotify(wasEdit);
      renderTab();
    }).catch(function(e){
      // Loi POST (ke ca version conflict): giu popup va gia tri nguoi dung vua nhap.
      loading=false;save.disabled=false;
      status.textContent=e.message||'Không lưu được giờ khai báo. Vui lòng thử lại.';
    });
 };
 var history=add(right,'div','kbg20-reconcile-history');
 add(history,'h4','', 'Lịch sử điều chỉnh');
 add(history,'p','kbg20-reconcile-history-help','Đợt duyệt: mỗi lần PM xác nhận hoàn thành. Phiên bản: mỗi lần ghi nhận hoặc sửa giờ.');
 var historyList=add(history,'div','kbg20-reconcile-history-list');
 (d.history||[]).forEach(function(x){var h=add(historyList,'div','kbg20-reconcile-version');
  add(h,'strong','', 'Đợt duyệt '+x.round+' · Phiên bản '+x.version);
  add(h,'time','kbg20-reconcile-date',formatEditDate(x.createdUtc));
  add(h,'span','', 'Trong ca '+(x.regular==null?'Chưa khai':minutesString(x.regular))+' · Tăng ca '+(x.overtime==null?'Chưa khai':minutesString(x.overtime)));
  if(x.reason)add(h,'small','',x.reason);
 });
}
function openTask(taskId){if(loading)return;showDetailActions(false);try{display();}catch(e){var m=document.getElementById("kbg20Message");if(m)m.textContent=e.message;return;}loading=true;message('Đang tải khai báo giờ...',false);
 get('reconcile',taskId).then(renderDetail).catch(function(e){message(e.message,true);}).then(function(){loading=false;});}

function renderManualItems(){
 var host=document.getElementById('kbg20ManualList');if(!host)return;
 host.textContent='';
 var buttons=document.querySelectorAll('#kbg20ManualFilters [data-manual-filter]');
 buttons.forEach(function(b){var chosen=b.getAttribute('data-manual-filter')===manualFilter;
   b.classList.toggle('is-selected',chosen);b.setAttribute('aria-pressed',chosen?'true':'false');});
 var data=manualItems.filter(function(t){return manualFilter==='all'||
     (manualFilter==='saved'&&t.submitted)||(manualFilter==='missing'&&!t.submitted);});
 if(!data.length){add(host,'p','kbg20-empty','Không có công việc nào phù hợp với bộ lọc.');return;}
 var grid='kbg20-v41-manual-grid';var head=add(host,'div','kbg20-v41-manual-head '+grid);
 ['Công việc','Đợt duyệt','Giờ hệ thống','Giờ đã khai','Trạng thái','Hành động'].forEach(function(t){add(head,'span','',t);});
 data.forEach(function(t){
   var row=add(host,'article','kbg20-v41-manual-row '+grid);
   var identity=add(row,'div','kbg20-v41-manual-name');
   add(identity,'strong','', '['+(t.code||'')+'] '+(t.name||''));
   add(identity,'small','', 'Hoàn thành · Giờ của chính bạn');
   var round=add(row,'div','kbg20-v41-manual-round');add(round,'small','kbg20-v41-mobile-label','Đợt');
   add(round,'span','', 'Đợt duyệt '+t.round+' · Bản '+t.version);
   var sys=add(row,'div','kbg20-v41-manual-amount');add(sys,'small','kbg20-v41-mobile-label','Hệ thống');
   add(sys,'strong','', systemHHmm(t,'regular'));add(sys,'small','', 'Tăng ca '+systemHHmm(t,'overtime'));
   var declared=add(row,'div','kbg20-v41-manual-amount');add(declared,'small','kbg20-v41-mobile-label','Giờ khai báo');
   if(t.submitted){
     // Trong ca va tang ca la hai truong PHUT khai bao, khong lay gio he thong.
     // 00:00 la gia tri hop le, khong duoc coi la chua khai.
     if(t.declaredRegular!==null && t.declaredRegular!==undefined &&
        t.declaredOvertime!==null && t.declaredOvertime!==undefined){
       add(declared,'strong','', minutesString(Math.max(0,Number(t.declaredRegular)||0)));
       add(declared,'small','', 'Tăng ca '+minutesString(Math.max(0,Number(t.declaredOvertime)||0)));
     }else{
       // Bao ve tinh tuong thich neu API cu chua duoc cap nhat.
       add(declared,'span','', 'Chưa có dữ liệu giờ tự khai');
     }
   }else{
     add(declared,'span','', 'Chưa khai');
   }
   var status=add(row,'div','');add(status,'span',t.submitted?'kbg20-v41-manual-badge is-saved':'kbg20-v41-manual-badge is-missing',
     t.submitted?'Đã khai':'Chưa khai');
   var actions=add(row,'div','kbg20-v41-manual-actions');
   var b=add(actions,'button','kbg20-btn kbg20-v41-approval',t.submitted?'Chỉnh sửa giờ':'Khai báo giờ');
   b.type='button';b.addEventListener('click',function(){openTask(t.taskId);});
 });
}
function renderTab(){
 var host=document.getElementById('kbg20ManualList');if(!host||manualBusy)return;
 manualBusy=true;host.textContent='';add(host,'p','kbg20-empty','Đang tải công việc đã hoàn thành...');
 get('reconcile-list').then(function(x){manualItems=x.items||[];renderManualItems();})
 .catch(function(e){host.textContent='';add(host,'p','kbg20-reconcile-error',e.message||'Không tải được danh sách.');})
 .then(function(){manualBusy=false;});
}
var filterEl=document.getElementById('kbg20ManualFilters');
if(filterEl)filterEl.addEventListener('click',function(e){
 var b=e.target.closest('[data-manual-filter]');if(!b||!filterEl.contains(b))return;
 manualFilter=b.getAttribute('data-manual-filter');renderManualItems();
});

if(btnOpen)btnOpen.addEventListener('click',openList);
window.Kbg20DoiSoatUI={openList:openList,openTask:openTask,renderTab:renderTab};
var qs=new URLSearchParams(window.location.search);
if(qs.get('reconcile')==='1'){
 if(window.Kbg20PageUI && window.Kbg20PageUI.openManual)window.Kbg20PageUI.openManual();
 var taskId=qs.get('taskId');if(taskId && /^[0-9a-f-]{36}$/i.test(taskId))openTask(taskId);else openList();
}
})(window,document);
