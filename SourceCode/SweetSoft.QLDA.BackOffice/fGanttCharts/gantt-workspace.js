/* Gantt 2 che do. Du lieu khach hang chi la snapshot RAM, tuyet doi khong luu SQL. */
(function (w, d) {
    'use strict';
    if (w.GanttWorkspace) return;
    function sourceTasks() { return Array.isArray(w.GANTT_CUSTOMER_SOURCE) ? w.GANTT_CUSTOMER_SOURCE : []; }
    var edited = null, preview = null, mode = 'internal', legendPinned = false, customerCollapsed = {};
    function byId(id) { return d.getElementById(id); }
    function cloneData(data) { return JSON.parse(JSON.stringify(data)); }
    function safeText(value) { return String(value == null ? '' : value); }
    // Ban Khach hang chi luu o trinh duyet, theo tung du an + tai khoan; KHONG gui len SQL.
    function draftKey() {
        var root=byId('ganttWorkspaceToolbar');
        var project=root&&root.getAttribute('data-gantt-project')||w.location.pathname;
        var account=root&&root.getAttribute('data-gantt-account')||'';
        return 'SweetSoft_GanttCustomerDraft_v2:'+encodeURIComponent(project)+':'+encodeURIComponent(account);
    }
    function safeDraftRecords(items) {
        return Array.isArray(items)&&items.length>0&&items.length<=500&&items.every(function(t){
            return !!t&&typeof t==='object'&&typeof t.Id==='string'&&t.Id.length>0&&t.Id.length<=150&&
                typeof t.Code==='string'&&t.Code.length<=50&&typeof t.Name==='string'&&t.Name.length<=250&&
                typeof t.StartDate==='string'&&t.StartDate.length<=20&&typeof t.EndDate==='string'&&t.EndDate.length<=20&&
                [0,1,2,3,4].indexOf(Number(t.Status))!==-1&&
                (t.DependencyId==null||(typeof t.DependencyId==='string'&&t.DependencyId.length<=150));
        });
    }
    var restoringDraft=false;
    function persistDraft() {
        if(restoringDraft)return;
        var editor=byId('ganttCustomerEditor');
        var view=editor&&!editor.hidden?'editor':'preview';
        var state={version:2,mode:mode,view:view,edited:edited,preview:preview,collapsed:customerCollapsed};
        try { w.localStorage.setItem(draftKey(),JSON.stringify(state)); } catch(e) {
            // Neu trinh duyet chan localStorage, khong lam gian doan thao tac dang thuc hien.
            if(!persistDraft.warned){persistDraft.warned=true;error('Trinh duyet khong cho luu ban tam. Du lieu co the mat khi tai lai trang.');}
        }
    }
    function restoreDraft() {
        if(!byId('ganttCustomerPanel'))return;
        var state=null;
        try { var raw=w.localStorage.getItem(draftKey());if(raw&&raw.length<=1048576)state=JSON.parse(raw); }catch(e){}
        restoringDraft=true;
        try {
            if(state&&state.version===2&&safeDraftRecords(state.edited)){
                edited=cloneData(state.edited);
                edited.forEach(function(t){t.Assignee='';});
                preview=safeDraftRecords(state.preview)&&!checkData(state.preview)?cloneData(state.preview):null;
                if(preview)preview.forEach(function(t){t.Assignee='';});
                customerCollapsed=(state.collapsed&&typeof state.collapsed==='object'&&!Array.isArray(state.collapsed))?state.collapsed:{};
                renderEditor();
                switchMode(state.mode==='customer'?'customer':'internal');
                if(mode==='customer'&&state.view==='preview'&&preview){
                    renderCustomer(preview);showCustomerPreview();
                }else if(mode==='customer')showCustomerEditor();
            }else{
                switchMode('internal');
            }
        }finally{restoringDraft=false;}
    }

    function setAttrs(el, attrs) { Object.keys(attrs).forEach(function (key) { el.setAttribute(key, attrs[key]); }); return el; }
    function parseDate(s) {if(!/^\d{4}-\d{2}-\d{2}$/.test(s||''))return NaN;var a=s.split('-').map(Number),ms=Date.UTC(a[0],a[1]-1,a[2]);return new Date(ms).toISOString().slice(0,10)===s?ms:NaN;}
    // Chỉ chỉnh bản dữ liệu tạm. Không gọi TaskManager.Save()/SQL.
    var pendingRemovalId = null;
    var makeId = function () { if (w.crypto && w.crypto.randomUUID) return w.crypto.randomUUID(); return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g,function(c){var r=Math.floor(Math.random()*16);return(c==='x'?r:(r&3|8)).toString(16);}); };
    function wbsParts(code) { return String(code||'').split('.').map(Number); }
    function validCode(code) { return /^[1-9]\d*(?:\.[1-9]\d*)*$/.test(String(code||'')); }
    function wbsCompare(a,b) {var x=wbsParts(a),y=wbsParts(b);for(var i=0;i<Math.min(x.length,y.length);i++){if(x[i]!==y[i])return x[i]-y[i];}return x.length-y.length;}
    function parentCode(code) {var i=String(code).lastIndexOf('.');return i<0?'':code.slice(0,i);}
    function isAncestor(code,childCode){return !!code&&String(childCode).indexOf(code+'.')===0;}
    function sortData(data){data.sort(function(a,b){return wbsCompare(a.Code,b.Code);});return data;}
    function codeMap(data) {var map={};data.forEach(function(t){map[t.Code]=t;});return map;}
    function directChildren(data,code){return data.filter(function(t){return parentCode(t.Code)===code;});}
    // Ban Khach hang la du lieu trinh bay ao: phu thuoc chi ve mui ten, khong ep ngay/tham chieu WBS.
    function canDependOn(task,other) {
        return !!(task && other && task.Id !== other.Id);
    }
    // Du lieu trinh bay la ao: khong auto doi ngay cha/con theo lich ke hoach that.
    function rollupParentDates(data) { return data; }
    function toDate(value){return new Date(value).toISOString().slice(0,10);}
    function formatVietnamDate(iso){var a=String(iso||'').split('-');return a.length===3?a[2]+'/'+a[1]+'/'+a[0]:iso;}
    function error(msg){var el=byId('ganttCustomerErrors');if(el)el.textContent=msg||'';}
    function checkData(data){
        if(!Array.isArray(data)||!data.length)return 'Cần ít nhất một công việc.';
        if(data.length>500)return 'Bản trình bày chỉ hỗ trợ tối đa 500 công việc.';
        var ids=new Set(),codes=new Set(),allIds=new Set();
        for(var i=0;i<data.length;i++){
            var t=data[i],code=safeText(t.Code).trim(),name=safeText(t.Name).trim();
            if(!validCode(code)||code.length>30)return 'Dòng '+(i+1)+': mã cần có dạng 1, 1.1, 1.2.1...';
            if(!name||name.length>160)return '['+code+'] Tên công việc phải từ 1 đến 160 ký tự.';
            if(!t.Id||ids.has(t.Id)||codes.has(code))return 'ID hoặc mã công việc bị trùng: '+code;
            ids.add(t.Id);allIds.add(t.Id);codes.add(code);
            var start=parseDate(t.StartDate),end=parseDate(t.EndDate);
            if(!Number.isFinite(start)||!Number.isFinite(end)||end<start||end-start>3660*86400000)return '['+code+'] Ngày bắt đầu/kết thúc không hợp lệ.';
            if([0,1,2,3,4].indexOf(Number(t.Status))<0)return '['+code+'] Trạng thái không hợp lệ.';
        }
        for(var j=0;j<data.length;j++){
            var task=data[j];
            // Chi ngan tham chieu mat/hay tu phu thuoc de tranh ve duong lien ket vo nghia.
            if(task.DependencyId && (task.DependencyId===task.Id || !allIds.has(task.DependencyId)))
                return '['+task.Code+'] Công việc phụ thuộc không tồn tại hoặc trùng chính nó.';
        }
        return '';
    }
    function issueIfInvalid(){error(checkData(edited||[]));}
    function notify(msg){if(w.toastr&&w.toastr.success)w.toastr.success(msg);}
    function nextCode(parent){
        var siblings=edited.filter(function(t){return parentCode(t.Code)===parent;});
        var last=siblings.reduce(function(max,t){var part=wbsParts(t.Code);return Math.max(max,part[part.length-1]||0);},0);
        return (parent?parent+'.':'')+(last+1);
    }
    function addTask(parentId){
        if(!edited)edited=cloneData(sourceTasks());
        pendingRemovalId=null;
        if(edited.length>=500){error('Đã đạt giới hạn 500 công việc.');return false;}
        var parent=parentId?edited.find(function(t){return t.Id===parentId;}):null;
        if(parentId&&!parent){error('Không tìm thấy công việc cha.');return false;}
        var parCode=parent?parent.Code:'',code=nextCode(parCode);
        var siblings=edited.filter(function(t){return parentCode(t.Code)===parCode;});
        var prev=sortData(siblings).slice(-1)[0];
        var date=parent?parent.StartDate:(prev?toDate(parseDate(prev.EndDate)+86400000):toDate(Date.UTC(new Date().getFullYear(),new Date().getMonth(),new Date().getDate())));
        var t={Id:makeId(),Code:code,Name:parent?'Công việc mới':'Giai đoạn mới',StartDate:date,EndDate:date,Status:0,Assignee:'',DependencyId:'',IsVirtual:true};
        edited.push(t);sortData(edited);rollupParentDates(edited);renderEditor();issueIfInvalid();persistDraft();
        var tr=byId('ganttCustomerEditorRows').querySelector('[data-id="'+t.Id+'"]');if(tr){tr.scrollIntoView({block:'nearest'});var input=tr.querySelector('input[data-field="Name"]');if(input){input.focus();input.select();}}
        return false;
    }
    function reindexAfterDelete(code) {
        // Đồng bộ cách TaskManager dồn lại mã WBS sau khi xóa.
        var p=parentCode(code),last=wbsParts(code).slice(-1)[0];
        var siblings=edited.filter(function(t){return parentCode(t.Code)===p&&wbsParts(t.Code).slice(-1)[0]>last;});
        sortData(siblings);
        siblings.forEach(function(sibling){
            var old=sibling.Code,newCode=(p?p+'.':'')+(wbsParts(old).slice(-1)[0]-1);
            edited.forEach(function(t){if(t.Code===old||isAncestor(old,t.Code))t.Code=newCode+t.Code.slice(old.length);});
        });
        sortData(edited);
    }
    function removeTask(id){
        var t=edited&&edited.find(function(t){return t.Id===id;});if(!t)return false;
        var subtree=edited.filter(function(x){return x.Id===id||isAncestor(t.Code,x.Code);});
        var removalIds=new Set(subtree.map(function(x){return x.Id;}));
        // Khi xoa task ao, tu bo mui ten phu thuoc toi task do; khong chan nguoi dung.
        if(edited.length===subtree.length){error('Phải giữ ít nhất một công việc trong báo cáo.');return false;}
        if(subtree.length>1&&pendingRemovalId!==id){
            pendingRemovalId=id;error('Xóa ['+t.Code+'] sẽ xóa '+subtree.length+' công việc trong nhánh. Nhấn thùng rác lần nữa để xác nhận.');
            renderEditor();return false;
        }
        pendingRemovalId=null;
        edited=edited.filter(function(x){return !removalIds.has(x.Id);});
        edited.forEach(function(x){if(removalIds.has(x.DependencyId))x.DependencyId='';});
        reindexAfterDelete(t.Code);renderEditor();issueIfInvalid();persistDraft();
        notify('Đã xóa '+subtree.length+' công việc khỏi bản Khách hàng. Không sửa SQL.');return false;
    }
    function updateEdit(field){
        var i=Number(field.dataset.index),key=field.dataset.field;
        if(!edited||!edited[i])return;
        pendingRemovalId=null;
        var t=edited[i],value=key==='Status'?Number(field.value):field.value;
        if(key==='Code'){
            value=String(value).trim();if(value===t.Code)return;
            if(!validCode(value)){error('Mã phải theo dạng 1, 1.1 hoặc 1.2.3.');return;}
            var old=t.Code,children=edited.filter(function(x){return isAncestor(old,x.Code);});
            if(edited.some(function(x){return x!==t&&x.Code===value;})){error('Mã ['+value+'] đã tồn tại.');return;}
            if(children.some(function(x){return edited.some(function(y){return !isAncestor(old,y.Code)&&y!==t&&y.Code===value+x.Code.slice(old.length);});})) {error('Đổi mã sẽ làm trùng mã công việc con.');return;}
            t.Code=value;children.forEach(function(x){x.Code=value+x.Code.slice(old.length);});sortData(edited);
            renderEditor();issueIfInvalid();persistDraft();return;
        }
        t[key]=value;
        // Ngay, trang thai va phu thuoc do nguoi dung quyet dinh, khong tu day lich/tong hop cha.
        if(key==='DependencyId'||key==='StartDate'||key==='EndDate'||key==='Status'||key==='Name'){renderEditor();}
        issueIfInvalid();persistDraft();
    }
    function renderEditor(){
        var tbody=byId('ganttCustomerEditorRows');if(!tbody)return;
        var scroller=tbody.closest('.gantt-customer-edit-scroll'),oldTop=scroller?scroller.scrollTop:0;
        tbody.textContent='';sortData(edited||[]);
        var data=edited||[];
        data.forEach(function(t,i){
            var tr=d.createElement('tr');tr.dataset.id=t.Id;
            var columns=[['Code','text',30],['Name','text',160],['StartDate','date'],['EndDate','date'],['Status','status'],['DependencyId','dependency']];
            var children=directChildren(data,t.Code),childCount=children.length;
            columns.forEach(function(spec){
                var td=d.createElement('td'),field=d.createElement(spec[1]==='status'||spec[1]==='dependency'?'select':'input');
                if(spec[1]==='status'){[[0,'Chưa bắt đầu'],[1,'Đang thực hiện'],[2,'Hoàn thành'],[3,'Hoàn thành trễ'],[4,'Chờ duyệt']].forEach(function(o){var opt=d.createElement('option');opt.value=o[0];opt.textContent=o[1];field.appendChild(opt);});field.value=String(t.Status);}
                else if(spec[1]==='dependency'){
                    var none=d.createElement('option');none.value='';none.textContent='Không phụ thuộc';field.appendChild(none);
                    data.filter(function(x){return canDependOn(t,x);}).forEach(function(x){var opt=d.createElement('option');opt.value=x.Id;opt.textContent=x.Code;field.appendChild(opt);});field.value=t.DependencyId||'';
                    if(t.DependencyId&&!field.value){var bad=d.createElement('option');bad.value=t.DependencyId;bad.textContent='⚠ Phụ thuộc không hợp lệ';field.appendChild(bad);field.value=t.DependencyId;}
                }else {field.type=spec[1];if(spec[2])field.maxLength=spec[2];field.value=safeText(t[spec[0]]);}
                field.dataset.field=spec[0];field.dataset.index=i;
                field.setAttribute('aria-label',spec[0]+' · '+t.Code);
                // Ngay cua task cha/con deu duoc sua thoai mai; tooltip khong dung title native.
                if(spec[0]==='DependencyId'&&t.DependencyId){
                    var selected=data.find(function(x){return x.Id===t.DependencyId;});
                    if(selected)field.dataset.ganttUiTip=selected.Code+' · '+selected.Name;
                }
                field.addEventListener('change',function(){updateEdit(field);});
                // Luu ca khi dang go ten/ngay, khong can roi o nhap truoc khi F5.
                if(spec[0]==='Name'||spec[0]==='StartDate'||spec[0]==='EndDate'){
                    field.addEventListener('input',function(){if(edited&&edited[i]){edited[i][spec[0]]=field.value;persistDraft();}});
                }
                td.appendChild(field);tr.appendChild(td);
            });
            var action=d.createElement('td');action.className='gantt-task-actions';
            var add=d.createElement('button');add.type='button';add.className='gantt-row-add';add.dataset.ganttUiTip='Thêm công việc con vào ['+t.Code+']';add.setAttribute('aria-label',add.dataset.ganttUiTip);add.innerHTML='<i class="fas fa-plus" aria-hidden="true"></i>';add.onclick=function(){return addTask(t.Id);};
            var remove=d.createElement('button');remove.type='button';remove.className='gantt-row-delete';remove.dataset.ganttUiTip='Xóa ['+t.Code+'] khỏi bản trình bày';remove.dataset.ganttUiKind='danger';remove.setAttribute('aria-label',remove.dataset.ganttUiTip);remove.innerHTML='<i class="far fa-trash-alt" aria-hidden="true"></i>';remove.onclick=function(){return removeTask(t.Id);};
            if(pendingRemovalId===t.Id)remove.classList.add('is-delete-pending');
            action.appendChild(add);action.appendChild(remove);tr.appendChild(action);
            tbody.appendChild(tr);
        });
        if(scroller)scroller.scrollTop=oldTop;
        var count=byId('ganttCustomerCount');if(count)count.textContent=data.length+' công việc';
    }
        function dateLabel(day) {var date=new Date(day);return String(date.getUTCDate()).padStart(2,'0')+'/'+String(date.getUTCMonth()+1).padStart(2,'0');}
    function elt(tag,classes,text) {var e=d.createElement(tag);if(classes)e.className=classes;if(text!==undefined)e.textContent=safeText(text);return e;}
    function renderCustomer(data) {
        var panel=byId('ganttCustomerPreview');if(!panel)return;
        panel.textContent='';
        if(!data||!data.length)return;
        var starts=data.map(function(t){return parseDate(t.StartDate);}),ends=data.map(function(t){return parseDate(t.EndDate);});
        var min=Math.min.apply(null,starts.concat(ends))-86400000,max=Math.max.apply(null,starts.concat(ends))+86400000,total=Math.round((max-min)/86400000)+1;
        if(!Number.isFinite(min)||!Number.isFinite(max)||total<1||total>3650){error('Khoang ngay bieu do khong hop le hoac vuot qua 10 nam.');return;}
        var chart=elt('div','gantt'),body=elt('div','gantt-body'),left=elt('div','task-col'),right=elt('div','chart-col');
        var leftHead=elt('div','task-col-header','Công việc'),rightHead=elt('div','chart-header'),tracks=elt('div','chart-tracks');left.appendChild(leftHead);right.appendChild(rightHead);
        var cw=Math.max(total*60,Math.max(600,(panel.clientWidth||1000)-350));rightHead.style.width=cw+'px';tracks.style.width=cw+'px';
        var dayPct=function(value){return (value/total*100)+'%';};
        for(var day=0;day<total;day++) {
            var tick=elt('div','tick-label',dateLabel(min+day*86400000));tick.style.left=dayPct(day);tick.style.width=dayPct(1);rightHead.appendChild(tick);
            var line=elt('div','tick-line');line.style.left=dayPct(day);rightHead.appendChild(line);
        }
        var rowMap={},barMap={},trackMap={},toggleMap={};
        var descendants={};
        data.forEach(function(t){descendants[t.Code]=data.some(function(x){return x.Code.indexOf(t.Code+'.')===0;});});
        data.forEach(function(task){
            var code=safeText(task.Code),level=Math.max(1,code.split('.').filter(Boolean).length);
            var row=elt('div','task-row-wrap'),content=elt('div',level===1?'task-level-1':'task-level-child level-'+Math.min(5,level));
            var isParent=!!descendants[code];
            if(level>1){var branch=elt('span','tree-branch');branch.setAttribute('aria-hidden','true');content.appendChild(branch);}
            if(isParent){var toggle=elt('i','fas fa-chevron-down gantt-preview-toggle');toggle.setAttribute('aria-hidden','true');content.appendChild(toggle);toggleMap[task.Id]=toggle;}
            var name=elt('span','gantt-preview-task-name',code+'. '+safeText(task.Name));content.appendChild(name);
            content.dataset.ganttUiTip=code+' · '+safeText(task.Name);
            row.appendChild(content);left.appendChild(row);row.dataset.code=code;row.dataset.id=task.Id;
            var track=elt('div','task-track');track.dataset.code=code;track.dataset.id=task.Id;
            var b=elt('div','bar'),startDay=Math.round((parseDate(task.StartDate)-min)/86400000),endDay=Math.round((parseDate(task.EndDate)-min)/86400000)+1;
            var status=Number(task.Status),label='',css='';
            if(status===2||status===3){label=status===3?'Hoàn thành trễ':'Hoàn thành';css='done';}
            else if(status===1||status===4){label=status===4?'Chờ duyệt':'Đang thực hiện';css='doing';}
            else {label='Chưa bắt đầu';css='todo';}
            var rem=Math.round((parseDate(task.EndDate)-Date.UTC(new Date().getFullYear(),new Date().getMonth(),new Date().getDate()))/86400000);
            var deadlineWarning='',warningKind='';
            if(status!==2&&status!==3){
                if(rem<0){deadlineWarning='Trễ hạn '+Math.abs(rem)+' ngày';warningKind='overdue';}
                else if(rem<=2){deadlineWarning=rem===0?'Sắp đến hạn (hôm nay)':'Sắp đến hạn (còn '+rem+' ngày)';warningKind='warning';}
                if(status===1&&warningKind)css=warningKind;
            }
            b.classList.add('status-'+css);b.style.left=dayPct(startDay);b.style.width=dayPct(endDay-startDay);
            b.dataset.ganttTip=status===3?'Hoàn thành trễ':(deadlineWarning||label);
            b.dataset.ganttKind=status===3?'late':(warningKind||css);
            var fill=elt('div','bar-fill animated-stripes'),barText=elt('div','static-text',label);fill.style.width='100%';fill.appendChild(barText);b.appendChild(fill);
            if(status===3){b.classList.add('is-completed-late');barText.style.color='#fff';barText.style.textShadow='none';}
            track.appendChild(b);tracks.appendChild(track);
            rowMap[task.Id]=row;trackMap[task.Id]=track;barMap[task.Id]=b;
        });
        var todayIndex=Math.round((Date.UTC(new Date().getFullYear(),new Date().getMonth(),new Date().getDate())-min)/86400000);
        if(todayIndex>=0&&todayIndex<total){var today=elt('div','today-line');today.style.left=dayPct(todayIndex+.5);tracks.appendChild(today);}
        var svgNS='http://www.w3.org/2000/svg',svg=d.createElementNS(svgNS,'svg');
        setAttrs(svg,{width:String(cw),height:String(data.length*52),'aria-hidden':'true'});
        svg.style.cssText='position:absolute;top:0;left:0;pointer-events:none;z-index:8;';
        var defs=d.createElementNS(svgNS,'defs'),marker=d.createElementNS(svgNS,'marker'),head=d.createElementNS(svgNS,'path');
        setAttrs(marker,{id:'ganttCustomerDepArrow',viewBox:'0 0 10 10',refX:8,refY:5,markerWidth:6,markerHeight:6,orient:'auto'});
        setAttrs(head,{d:'M 0 0 L 10 5 L 0 10 z',fill:'#ff9800'});marker.appendChild(head);defs.appendChild(marker);svg.appendChild(defs);tracks.appendChild(svg);
        function isBarVisible(id){var tr=trackMap[id];return !!(tr&&tr.style.display!=='none'&&!tr.classList.contains('hide-bar'));}
        function drawLinks(){
            Array.prototype.slice.call(svg.querySelectorAll('path.dep-line')).forEach(function(p){p.remove();});
            data.forEach(function(t){
                if(!t.DependencyId||!isBarVisible(t.DependencyId)||!isBarVisible(t.Id))return;
                var a=barMap[t.DependencyId],b=barMap[t.Id],from=trackMap[t.DependencyId],to=trackMap[t.Id];if(!a||!b||!from||!to)return;
                var sx=a.offsetLeft+a.offsetWidth,sy=from.offsetTop+a.offsetTop+a.offsetHeight/2;
                var ex=b.offsetLeft,ey=to.offsetTop+b.offsetTop+b.offsetHeight/2;
                var elbow=sx+12;
                var path=d.createElementNS(svgNS,'path');path.setAttribute('class','dep-line');path.setAttribute('marker-end','url(#ganttCustomerDepArrow)');
                path.setAttribute('d','M '+sx+' '+sy+' L '+elbow+' '+sy+' L '+elbow+' '+ey+' L '+(ex-3)+' '+ey);svg.appendChild(path);
            });
        }
        function refreshVisibleRows(){
            data.forEach(function(task){
                var hidden=Object.keys(customerCollapsed).some(function(code){return customerCollapsed[code]&&task.Code.indexOf(code+'.')===0;});
                rowMap[task.Id].style.display=hidden?'none':'';trackMap[task.Id].style.display=hidden?'none':'';
                // Giong Gantt Noi bo: neu cha dang mo thi an thanh cua cha; dong cha thi hien thanh dai dien.
                var parent=!!descendants[task.Code];
                trackMap[task.Id].classList.toggle('hide-bar',parent&&!customerCollapsed[task.Code]);
                if(toggleMap[task.Id])toggleMap[task.Id].style.transform=customerCollapsed[task.Code]?'rotate(-90deg)':'rotate(0deg)';
            });
            svg.setAttribute('height',String(data.filter(function(task){return trackMap[task.Id].style.display!=='none';}).length*52));
            w.requestAnimationFrame(drawLinks);
        }
        data.forEach(function(task){
            if(!descendants[task.Code])return;
            var target=rowMap[task.Id];target.style.cursor='pointer';target.dataset.ganttUiTip='Thu gọn / Mở rộng ['+task.Code+']';
            target.addEventListener('click',function(){customerCollapsed[task.Code]=!customerCollapsed[task.Code];refreshVisibleRows();persistDraft();});
        });
        right.appendChild(tracks);body.appendChild(left);body.appendChild(right);chart.appendChild(body);panel.appendChild(chart);
        refreshVisibleRows();
    }
    function modeInput(value) {var el=byId('ganttReportMode');if(el)el.value=value;}
    function payloadInput(value){var el=byId('ganttCustomerPayload');if(el)el.value=value;}
    function reportButtonLabel() {
        var label=byId('ganttReportActionLabel');
        var action=byId('ganttReportAction');
        if(label) label.textContent='Tạo báo cáo';
        if(action) { action.hidden=false; action.removeAttribute('title'); action.dataset.ganttUiTip='Xuất báo cáo HTML · '+(mode==='customer'?'Khách hàng':'Nội bộ'); }
    }
    function updateCustomerViewButton() {
        var btn=byId('ganttCustomerPreviewToggle');
        var label=byId('ganttCustomerPreviewToggleLabel');
        if(!btn||!label) return;
        var isEditing=!byId('ganttCustomerEditor').hidden;
        label.textContent=isEditing?'Xem Gantt':'Chỉnh sửa';
        btn.setAttribute('aria-label',isEditing?'Xem Gantt khách hàng':'Chỉnh sửa dữ liệu khách hàng');
        btn.removeAttribute('title');btn.dataset.ganttUiTip=isEditing?'Xem biểu đồ từ dữ liệu đã sửa':'Quay lại chỉnh dữ liệu';
        var icon=btn.querySelector('i');
        if(icon) icon.className=isEditing?'fas fa-chart-bar':'fas fa-pen';
    }
    // Tinh chieu cao theo vi tri cua khung, cuon ben trong bang/Gantt.
    function fitCustomerViewport() {
        if(mode!=='customer') return;
        var surface=byId('ganttCustomerSurface'),panel=byId('ganttCustomerPanel');
        if(!surface||!panel||panel.hidden) return;
        var viewport=d.documentElement.clientHeight||w.innerHeight;
        var available=Math.floor(viewport-surface.getBoundingClientRect().top-64);
        surface.style.height=Math.max(240,available)+'px';
    }
    function queueCustomerFit() { w.requestAnimationFrame(fitCustomerViewport); }
    function showCustomerEditor() {
        byId('ganttCustomerEditor').hidden=false;
        byId('ganttCustomerPreview').hidden=true;
        updateCustomerViewButton();
        queueCustomerFit();
    }
    function showCustomerPreview() {
        byId('ganttCustomerEditor').hidden=true;
        byId('ganttCustomerPreview').hidden=false;
        updateCustomerViewButton();
        queueCustomerFit();
    }
    function switchMode(next) {
        if(next!=='internal'&&next!=='customer')return false;
        mode=next;
        d.body.classList.toggle('gantt-customer-mode',next==='customer');
        byId('ganttInternalPanel').hidden=next!=='internal';
        byId('ganttCustomerPanel').hidden=next!=='customer';
        ['internal','customer'].forEach(function(m){
            var btn=byId(m==='internal'?'ganttTabInternal':'ganttTabCustomer');
            if(!btn)return;
            btn.classList.toggle('is-active',m===next);
            btn.setAttribute('aria-selected',m===next?'true':'false');
        });
        if(next==='customer'&&!edited) {
            edited=cloneData(sourceTasks());
            edited.forEach(function(t){t.Assignee='';t.IsVirtual=false;});
            rollupParentDates(edited);renderEditor();
            error('');
            showCustomerEditor();
        }
        reportButtonLabel();
        if(next==='customer') queueCustomerFit();
        persistDraft();
        return false;
    }
    function editCustomer() {
        if(!edited) {edited=cloneData(preview||sourceTasks());edited.forEach(function(t){t.Assignee='';t.IsVirtual=!!t.IsVirtual;});rollupParentDates(edited);}
        renderEditor();
        error('');
        showCustomerEditor();persistDraft();
        return false;
    }
    function resetCustomer() {
        edited=cloneData(sourceTasks());
        edited.forEach(function(t){t.Assignee='';t.IsVirtual=false;});
        rollupParentDates(edited);renderEditor();
        preview=null;customerCollapsed={};
        error('');notify('Đã khôi phục bản Khách hàng.');
        showCustomerEditor();persistDraft();
        return false;
    }
    function applyCustomer() {
        var err=checkData(edited||[]);
        if(err){error(err);return false;}
        preview=cloneData(edited);
        preview.forEach(function(t){t.Assignee='';});
        error('');
        renderCustomer(preview);
        showCustomerPreview();persistDraft();
        return false;
    }
    function closeEditor() {return editCustomer();}
    function toggleCustomerView() { return byId('ganttCustomerEditor').hidden ? editCustomer() : applyCustomer(); }
    function openReport(e) {
        if(e&&e.preventDefault)e.preventDefault();
        // Tao bao cao la xuat HTML, khong phai nut xem truoc.
        // Dang chinh: lay du lieu dang sua. Dang xem Gantt: lay dung snapshot dang xem.
        var reportTasks=null;
        if(mode==='customer') {
            if(!edited) { edited=cloneData(sourceTasks()); edited.forEach(function(t){t.Assignee='';});renderEditor(); }
            var editing=!byId('ganttCustomerEditor').hidden;
            reportTasks=editing?edited:preview;
            if(!reportTasks) { editCustomer(); return false; }
            var invalid=checkData(reportTasks);
            if(invalid) { error(invalid);if(!editing)editCustomer();error(invalid);return false; }
        }
        modeInput(mode);
        payloadInput(mode==='customer'?JSON.stringify(reportTasks.map(function(t){var x=Object.assign({},t);x.Assignee='';return x;})):'');
        var checkboxNodes=d.querySelectorAll('#shareProgressTaskList input[name="shareProgressTaskIds"]');
        checkboxNodes.forEach(function(c){c.checked=true;});
        d.body.classList.add('gantt-report-only');
        if(w.ShareProgressUI&&w.ShareProgressUI.openSelection)w.ShareProgressUI.openSelection(e);
        else {
            var modal=d.querySelector('[id$="modalSelection"]');
            if(modal&&w.bootstrap&&w.bootstrap.Modal)w.bootstrap.Modal.getOrCreateInstance(modal).show();
        }
        return false;
    }
    var help=byId('ganttLegendHelp'),legend=byId('ganttLegendPopover');
    if(help&&legend){function legendState(show){legend.classList.toggle('is-open',show);legend.setAttribute('aria-hidden',show?'false':'true');help.setAttribute('aria-expanded',show?'true':'false');}
        help.addEventListener('mouseenter',function(){legendState(true);});help.addEventListener('mouseleave',function(){if(!legendPinned)legendState(false);});
        legend.addEventListener('mouseenter',function(){legendState(true);});legend.addEventListener('mouseleave',function(){if(!legendPinned)legendState(false);});
        help.addEventListener('click',function(e){e.stopPropagation();legendPinned=!legendPinned;help.classList.toggle('is-pinned',legendPinned);legendState(legendPinned);});
        d.addEventListener('click',function(e){if(legendPinned&&!help.contains(e.target)&&!legend.contains(e.target)){legendPinned=false;help.classList.remove('is-pinned');legendState(false);}});
    }
    // Tooltip mau theo TaskList: hien ngay, lop noi tren body, khong bi cat boi bang cuon.
    function initGanttEditorTips(){
        if(w.__ganttEditorTipsReady)return;
        w.__ganttEditorTipsReady=true;
        var tip=null,active=null;
        function node(){
            if(!tip){tip=d.createElement('div');tip.className='gantt-editor-tip';tip.setAttribute('role','tooltip');tip.setAttribute('aria-hidden','true');d.body.appendChild(tip);}
            return tip;
        }
        function hide(){active=null;if(tip){tip.style.display='none';tip.setAttribute('aria-hidden','true');}}
        function place(el){
            if(!el||!el.isConnected||!tip)return;
            var r=el.getBoundingClientRect(),width=tip.offsetWidth,height=tip.offsetHeight;
            if(r.bottom<0||r.top>w.innerHeight){hide();return;}
            var left=Math.max(8,Math.min(w.innerWidth-width-8,r.left+(r.width-width)/2));
            var below=r.top-height-10<8;
            tip.classList.toggle('is-below',below);
            tip.style.left=Math.round(left)+'px';
            tip.style.top=Math.round(below?r.bottom+10:r.top-height-10)+'px';
        }
        function targetOf(el){
            var t=el&&el.closest?el.closest('[data-gantt-ui-tip]'):null;
            if(!t||!t.dataset.ganttUiTip)return null;
            return (t.closest('#ganttCustomerPanel')||t.closest('#ganttWorkspaceToolbar'))?t:null;
        }
        function show(el){
            if(!el||el===active)return;
            active=el;var box=node();box.textContent=el.dataset.ganttUiTip;
            box.dataset.kind=el.dataset.ganttUiKind||'default';
            box.style.display='block';box.setAttribute('aria-hidden','false');
            place(el);
        }
        d.addEventListener('mouseover',function(e){var t=targetOf(e.target);if(t)show(t);else if(active)hide();});
        d.addEventListener('mousemove',function(e){if(active&&targetOf(e.target)===active)place(active);});
        d.addEventListener('mouseout',function(e){if(active&&active.contains(e.target)&&(!e.relatedTarget||!active.contains(e.relatedTarget)))hide();});
        d.addEventListener('focusin',function(e){var t=targetOf(e.target);if(t)show(t);});
        d.addEventListener('focusout',function(e){if(active&&active.contains(e.target))hide();});
        d.addEventListener('scroll',hide,true);w.addEventListener('blur',hide);
    }
    initGanttEditorTips();
    reportButtonLabel();
    w.addEventListener('resize',queueCustomerFit);
    if(w.Sys&&w.Sys.Application) w.Sys.Application.add_load(queueCustomerFit);
    w.GanttWorkspace={addTask:addTask,removeTask:removeTask,changeMode:switchMode,editCustomer:editCustomer,applyCustomer:applyCustomer,resetCustomer:resetCustomer,closeEditor:closeEditor,toggleCustomerView:toggleCustomerView,openReport:openReport};
    if(d.readyState==='loading')d.addEventListener('DOMContentLoaded',restoreDraft,{once:true});else restoreDraft();
})(window, document);
