/* KBG20 - Van de: dung chung Soft Table va ExtraModal hien huu cua Task, khong nhay tab khi xem chi tiet. */
(function (window, document) {
    'use strict';
    function init() {
        var root=document.getElementById('kbg20Page'),pane=document.getElementById('gvIssuePane');
        if(!root||!pane)return;
        var projectId=root.getAttribute('data-project-id'),api=root.getAttribute('data-gv-api-url'),token=root.getAttribute('data-request-token');
        var deepId=root.getAttribute('data-gv-issue-id')||'',deepAction=new URLSearchParams(location.search).get('gvAction')||'report';
        var noticeParams=new URLSearchParams(window.location.search),pmFinalIssueId=noticeParams.get('pmFinalIssueId')||'',pmFinalNotificationId=noticeParams.get('pmFinalNotificationId')||'';
        var issueListUrl=root.getAttribute('data-issue-list-url')||'';
        var state={type:'task',filter:'active',search:'',page:1,size:8,items:[],busy:false,loading:false,
            modalId:null,modalInfo:null,modalMode:null,historyMode:'all',ledgerMode:'all',historyItems:[],ledgerItems:[],
            ledgerFilter:'all',historySeq:0,ledgerSeq:0,detailSeq:0,taskTotals:[0,0,0],issueTotals:[0,0,0],todayByIssue:{},todayDay:'',todayAt:0,todaySeq:0,workCalendar:null,serverNow:0,syncClientAt:0};
        function el(id){return document.getElementById(id);}
        function put(id,text){var x=el(id);if(x)x.textContent=text==null?'':String(text);}
        function append(p,tag,cls,text){var x=document.createElement(tag);if(cls)x.className=cls;if(text!==undefined)x.textContent=String(text);p.appendChild(x);return x;}
        function show(id,yes){var e=el(id);if(e)e.hidden=!yes;}
        function asInt(n){return Math.max(0,Math.floor(Number(n)||0));}
        function hms(n){n=asInt(n);return [Math.floor(n/3600),Math.floor(n/60)%60,n%60].map(function(v){return String(v).padStart(2,'0');}).join(':');}
        function hhmm(n){n=asInt(n);return String(Math.floor(n/60)).padStart(2,'0')+':'+String(n%60).padStart(2,'0');}
        // Gio HE THONG theo giay; gio tu khai/PM van theo phut nhu nghiep vu.
        function systemTime(seconds){return hhmm(Math.floor((asInt(seconds)+30)/60));}
        function parseHHmm(s){var m=/^\s*(\d{1,5}):([0-5]\d)\s*$/.exec(s||'');return m?Number(m[1])*60+Number(m[2]):null;}
        function vnTime(ms){if(ms==null)return 'Chưa kết thúc';try{return new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit',second:'2-digit',hour12:false}).format(new Date(Number(ms)));}catch(_){return '—';}}
        function warn(message,ok){
            if(!message)return;
            var severity=ok?'success':'error';
            if(window.CMSMasterJs&&typeof window.CMSMasterJs.ShowNotify==='function')window.CMSMasterJs.ShowNotify(message,severity);
            else if(window.toastr&&typeof window.toastr[severity]==='function')window.toastr[severity](message);
            else if(window.console&&typeof window.console[ok?'info':'error']==='function')window.console[ok?'info':'error']('KBG20 Issue:',message);
        }
        function request(params,post){var url=api,init={credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}};
            if(post){init.method='POST';init.headers['Content-Type']='application/x-www-form-urlencoded;charset=UTF-8';init.headers['X-KBG20-Token']=token;init.body=new URLSearchParams(params).toString();}
            else url+='?'+new URLSearchParams(params).toString();
            return fetch(url,init).then(function(resp){return resp.text().then(function(raw){var v;try{v=JSON.parse(raw);}catch(_){throw new Error('Máy chủ không trả JSON hợp lệ (HTTP '+resp.status+').');}if(resp.status===401||resp.status===403)throw new Error(v.error||'Phiên đăng nhập hoặc quyền truy cập đã hết hạn.');if(!resp.ok||v.ok===false)throw new Error(v.error||'HTTP '+resp.status);return v;});});
        }
        function uuid(){if(window.crypto&&window.crypto.randomUUID)return window.crypto.randomUUID();return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g,function(c){var r=Math.floor(Math.random()*16);return (c==='x'?r:(r&3|8)).toString(16);});}
        function openModal(id,title){if(window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'){window.CMSMasterJs.OpenDialog('#'+id,title);return true;}
            var x=el(id),jq=window.jQuery||window.$;if(x&&window.bootstrap&&window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(x).show();return true;}
            if(jq&&jq.fn&&jq.fn.modal){jq('#'+id).modal('show');return true;}warn('ExtraModal chưa được tải từ MasterTemplate.',false);return false;}
        function closeModal(id){var x=el(id),jq=window.jQuery||window.$;if(x&&window.bootstrap&&window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(x).hide();return;}
            if(jq&&jq.fn&&jq.fn.modal)jq('#'+id).modal('hide');}
        function actionButton(parent,text,action,id,cls,disabled,title){var b=append(parent,'button','kbg20-btn '+(cls||''),text);b.type='button';b.dataset.gvAction=action;b.dataset.gvId=id;b.disabled=!!disabled;if(title)b.title=title;return b;}
        // Trạng thái của TblVanDe chỉ gồm Đang xử lý (0) / Đã xử lý (1).
        // TrangThaiDuyetGio là QUY TRÌNH DUYỆT, hiển thị riêng, không ghi đè nhãn trạng thái Issue.
        function status(a){if(Number(a.approval)===1)return ['Chờ PM duyệt','is-pending'];return Number(a.status)===1?['Đã xử lý','is-done']:['Đang xử lý','is-active'];}
        function approvalCaption(a){if(Number(a.approval)===3)return 'Đã bị từ chối';return '';}
        function belongs(a,mode){if(mode==='all')return true;if(mode==='active')return a.status===0&&a.approval!==1;
            if(mode==='running')return !!a.running;if(mode==='reported')return !!a.reported;
            if(mode==='pending')return a.approval===1;if(mode==='done')return a.status===1||a.approval===2;return false;}
        function renderFilters(){['active','all','reported','running','pending','done'].forEach(function(key){var count=state.items.filter(function(x){return belongs(x,key);}).length;
            var target=document.querySelector('[data-gv-count="'+key+'"]');if(target)target.textContent=String(count);
            var chip=document.querySelector('#gvFilters [data-gv-filter="'+key+'"]');if(chip){var active=state.filter===key;chip.classList.toggle('is-selected',active);chip.setAttribute('aria-pressed',String(active));}});}
        function ledgerForIssue(id){return state.ledgerItems.find(function(x){return x.id===id;})||null;}
        function getFiltered(){var q=state.search.toLocaleLowerCase('vi-VN');return state.items.filter(function(x){return belongs(x,state.filter)&&(!q||((x.code||'')+' '+(x.name||'')).toLocaleLowerCase('vi-VN').indexOf(q)>=0);});}
        // Tooltip cùng cấu trúc và CSS với ganTooltipNho của Task. Không dùng native title.
        var issueTip=null,issueTipTarget=null;
        function hideIssueTip(){if(issueTip)issueTip.hidden=true;issueTipTarget=null;}
        function positionIssueTip(){if(!issueTip||!issueTipTarget||issueTip.hidden)return;
            var r=issueTipTarget.getBoundingClientRect(),w=issueTip.offsetWidth,h=issueTip.offsetHeight;
            var left=Math.min(Math.max(10,r.left+(r.width-w)/2),Math.max(10,window.innerWidth-w-10));
            var above=r.top>=h+12,top=above?r.top-h-8:r.bottom+8;
            if(!above&&top+h>window.innerHeight-8)top=Math.max(8,window.innerHeight-h-8);
            issueTip.style.left=Math.round(left)+'px';issueTip.style.top=Math.round(top)+'px';
        }
        function issueTooltip(target,heading,detail){if(!target||!detail)return;
            target.removeAttribute('title');target.tabIndex=0;target.setAttribute('aria-label',heading+'. '+detail);
            function display(){if(!issueTip){issueTip=append(document.body,'div','kbg20-ux-smart-tooltip');issueTip.hidden=true;}
                issueTip.textContent='';append(issueTip,'strong','',heading);append(issueTip,'p','',detail);
                issueTipTarget=target;issueTip.hidden=false;positionIssueTip();}
            target.addEventListener('mouseenter',display);target.addEventListener('focus',display);
            target.addEventListener('mouseleave',function(){if(issueTipTarget===target)hideIssueTip();});
            target.addEventListener('blur',function(){if(issueTipTarget===target)hideIssueTip();});
        }
        window.addEventListener('scroll',positionIssueTip,true);
        window.addEventListener('resize',positionIssueTip);
        // Giữ NGUYÊN DOM và các class iconAction / taskClockCell của Task.
        // Điểm khác duy nhất: POST đi vào API Issue và không áp thời hạn Task.
        function taskCell(parent,cls,label){var n=append(parent,'div',cls);if(label)append(n,'small','kbg20-v41-mobile-label',label);return n;}
        function issueIconAction(label,action,id,kind,overtime){
            var b=append(document.createDocumentFragment(),'button','kbg20-btn kbg20-v41-icon-action '+kind+(overtime?' is-overtime':''));
            b.type='button';b.dataset.gvAction=action;b.dataset.gvId=id;b.title=label;b.setAttribute('aria-label',label);
            var glyph=append(b,'span','kbg20-v41-glyph',kind==='is-stop'?'■':kind==='is-resume'?'↻':'▶');
            glyph.setAttribute('aria-hidden','true');
            if(overtime){var moon=append(b,'span','kbg20-v41-overtime-moon','☾');moon.setAttribute('aria-hidden','true');}
            return b;
        }
        // Cung gio ca do Task KBG20 cung cap. Nguon server, khong tu dat gio 08:00-17:00.
        function issueServerNow(){return (state.serverNow||Date.now())+Math.max(0,Date.now()-(state.syncClientAt||Date.now()));}
        function issueIntervals(){var cal=state.workCalendar||{},src=(cal.previousShifts||[]).concat(cal.shifts||[]);
            return src.map(function(x){return {a:Number(x.startMs),b:Number(x.endMs)};})
                .filter(function(x){return Number.isFinite(x.a)&&Number.isFinite(x.b)&&x.b>x.a;});}
        function inIssueShift(ms){return issueIntervals().some(function(x){return ms>=x.a&&ms<x.b;});}
        function issueStartPermitted(kind){if(!state.workCalendar)return false;
            var inside=inIssueShift(issueServerNow());return kind===1?inside:!inside;}
        // Chi tinh doan thoi gian duoc phep: thuong trong ca, tang ca ngoai ca.
        function issueLiveDelta(from,to,kind){if(to<=from)return 0;
            var spans=issueIntervals().map(function(x){return {a:Math.max(from,x.a),b:Math.min(to,x.b)};})
                .filter(function(x){return x.b>x.a;}).sort(function(a,b){return a.a-b.a;});
            var covered=0,last=from;
            spans.forEach(function(x){var a=Math.max(x.a,last);if(x.b>a)covered+=x.b-a;last=Math.max(last,x.b);});
            return Math.max(0,Math.floor((kind===1?covered:(to-from)-covered)/1000));}
        function setIssueListData(res){state.items=(res.items||[]).slice().sort(function(a,b){
                return String(a.code||'').localeCompare(String(b.code||''),'vi',{numeric:true,sensitivity:'base'}) ||
                    String(a.id||'').localeCompare(String(b.id||''));
            });state.workCalendar=res.workCalendar||null;
            state.serverNow=Number(res.serverNow)||Date.now();state.syncClientAt=Date.now();}
        function clockCell(p,a,kind){
            var regular=kind===1,running=!!a.running&&(!!a.runningOvertime===!regular),
                sec=asInt(regular?a.regularSeconds:a.overtimeSeconds),
                canStart=!!a.assigned&&a.status===0&&(a.approval===0||a.approval===3)&&!a.reported,
                allowed=canStart&&!a.running&&issueStartPermitted(kind);
            var cell=taskCell(p,'kbg20-v41-clock-cell'+(regular?' is-regular':' is-overtime'),regular?'Giờ làm theo lịch':'Giờ tăng ca');
            var stats=append(cell,'div','kbg20-v41-clock-main');
            var digits=append(stats,'strong','kbg20-v41-clock',hms(sec));
            digits.dataset.gvClock=String(kind);digits.dataset.gvBase=String(sec);
            digits.dataset.gvStarted=String(running&&a.started?a.started:0);
            digits.dataset.gvLive=String(running?asInt(a.runningLiveSeconds):0);
            digits.dataset.gvLiveKind=String(kind);
            digits.dataset.gvSync=String(state.serverNow);
            digits.dataset.gvId=a.id;
            var message=running?(issueStartPermitted(kind)?'Đang tính giờ ':'Đang mở · tạm ngừng cộng ')+(regular?'làm theo lịch':'tăng ca'):
                sec>0?'Tổng thời gian đã ghi':'Chưa ghi '+(regular?'giờ làm theo lịch':'giờ tăng ca');
            var sub=append(stats,'small','kbg20-v41-clock-sub',message);sub.title=message;
            var today=state.todayByIssue[a.id],label=append(stats,'small','kbg20-v43-today',state.todayDay?( 'Hôm nay: '+hms(asInt(today&&(regular?today.regular:today.overtime)))):'Hôm nay: đang tải…');
            label.dataset.gvTodayId=a.id;label.dataset.gvTodayKind=String(kind);
            label.dataset.gvTodayLive=String(running?asInt(a.runningTodaySeconds):0);
            var resumed=sec>0;
            var title=running?'Dừng giờ '+(regular?'làm theo lịch':'tăng ca'):
                !canStart?'Không thể bắt đầu ở trạng thái hiện tại':a.running?'Dừng đồng hồ đang chạy trước khi đổi loại giờ':!issueStartPermitted(kind)?(regular?'Ngoài ca làm việc: không thể bắt đầu giờ thường':'Chỉ được bắt đầu tăng ca ngoài ca làm việc'):
                (resumed?'Tiếp tục':'Bắt đầu')+(regular?' giờ làm theo lịch':' giờ tăng ca');
            var action=running?'stop':regular?'start':'startOvertime';
            var button=issueIconAction(title,action,a.id,running?'is-stop':allowed?(resumed?'is-resume':'is-start'):'is-disabled',!regular);
            button.disabled=!running&&!allowed;
            cell.insertBefore(button,stats);
            return cell;
        }
                function renderList(){
            var host=el('gvRows');if(!host)return;host.textContent='';renderFilters();
            var list=getFiltered(),pages=Math.max(1,Math.ceil(list.length/state.size));state.page=Math.max(1,Math.min(state.page,pages));
            put('gvCountLabel',state.items.length+' vấn đề trong dự án');put('gvPageNumber',state.page+' / '+pages);
            if(el('gvPrev'))el('gvPrev').disabled=state.page===1;
            if(el('gvNext'))el('gvNext').disabled=state.page>=pages;
            put('gvListInfo','Hiển thị '+(list.length?(state.page-1)*state.size+1:0)+'–'+Math.min(list.length,state.page*state.size)+' / '+list.length+' vấn đề · Thời gian được cộng từ các phiên xử lý.');
            if(!list.length){append(host,'p','kbg20-empty','Không có vấn đề phù hợp với bộ lọc này.');return;}
            list.slice((state.page-1)*state.size,state.page*state.size).forEach(function(a){
                var row=append(host,'article','kbg20-v41-task-row kbg20-v41-taskgrid'+(a.running?' is-running':'')+(a.approval===1?' is-pending':''));
                row.setAttribute('aria-label',(a.code||'')+' '+(a.name||''));
                var identity=taskCell(row,'kbg20-v41-identity','Vấn đề'),line=append(identity,'div','kbg20-v41-taskname');
                append(line,'strong','kbg20-v41-taskcode',a.code||'VD');
                var name=append(line,'strong','kbg20-v41-tasktext',a.name||'Chưa có tên vấn đề');
                issueTooltip(name,'Tên vấn đề đầy đủ','['+(a.code||'VD')+'] '+(a.name||''));
                var meta=append(identity,'div','kbg20-v41-identity-meta');
                append(meta,'small','kbg20-v41-task-note',a.assigned?'Vấn đề được phân công cho bạn':'Bạn có quyền quản lý');
                if(a.worked){var source=append(meta,'span','kbg20-v41-source-badge is-manual','Thủ công');
                    issueTooltip(source,'Nguồn bắt đầu · Phiên xử lý vấn đề · Thủ công','Phiên xử lý vấn đề được nhân viên chủ động bắt đầu.');}
                var stateCell=taskCell(row,'kbg20-v41-status-cell','Trạng thái vấn đề'),st=status(a),stateCls=st[1]==='is-done'?'is-done':st[1]==='is-pending'?'is-waiting':st[1]==='is-active'?'is-active':'is-idle';
                append(stateCell,'span','kbg20-v41-state '+stateCls,st[0]);
                var approvalText=approvalCaption(a);
                if(approvalText)append(stateCell,'small','gv-issue-approval-note',approvalText);
                var personal=taskCell(row,'kbg20-v41-personal-cell','Báo cáo phần việc');
                var wasWithdrawn=!a.reported&&!!a.withdrawn;
                var stateText=a.reported?'Đã báo hoàn thành':wasWithdrawn?'Đã rút báo hoàn thành':'Chưa báo hoàn thành';
                append(personal,'span','kbg20-dot2-personal '+(a.reported?'is-reported':wasWithdrawn?'is-withdrawn':'is-not-reported'),stateText);
                clockCell(row,a,1);clockCell(row,a,2);
                var actions=taskCell(row,'kbg20-v41-actions','Hành động');
                function choose(label,action,kind){var b=actionButton(actions,label,action,a.id,'kbg20-dot2-row-btn '+(kind||''));return b;}
                // Giống hàng Task đã hoàn thành: Issue Đã xử lý không có hành động ở Tab 1.
                // Việc xem kết quả giờ công thuộc Tab 3 / thông báo đã chốt, không mở sửa từ đây.
                if(Number(a.status)===1){
                    append(actions,'span','kbg20-v41-wait-label','—');
                    return;
                }
                if(a.assigned&&a.reported&&a.status===0&&(a.approval===0||a.approval===3)){
                    choose('Rút báo hoàn thành','withdraw','is-danger');
                    choose('Gửi duyệt hoàn thành','submit','is-submit-approval');
                }else if(a.assigned&&!a.reported&&(a.worked||a.approval===1||a.approval===2)){
                    choose('Báo hoàn thành phần việc','report','');
                }
                // Đúng luồng Task: khi chờ duyệt vẫn xem/điều chỉnh giờ cá nhân trong popup,
                // KHÔNG đặt tên nút theo vị trí tab và KHÔNG để cột hành động trống.
                // Quyền chỉnh sửa thực tế được kiểm tra lại tại Manager; khi khóa popup chỉ xem.
                if(a.assigned&&a.reported&&(a.approval===1||a.approval===2)){
                    choose('Xem / sửa giờ đã báo','manual','');
                }
                if(a.manager&&a.approval===1)choose('Phê duyệt hoàn thành','approve','');
                if(a.manager&&a.approval===2)choose('Duyệt giờ công','chot','');
                if(!actions.querySelector('button'))append(actions,'span','kbg20-v41-wait-label','—');
                if(actions.querySelectorAll('button').length>1)actions.classList.add('is-multi-action');
            });
            tick();
        }
        function tick(){
            var now=issueServerNow();
            document.querySelectorAll('#gvRows [data-gv-clock]').forEach(function(x){
                var total=asInt(x.dataset.gvBase),live=asInt(x.dataset.gvLive),start=Number(x.dataset.gvStarted)||0;
                var kind=Number(x.dataset.gvLiveKind);
                var delta=start?issueLiveDelta(state.serverNow,now,kind):0;
                x.textContent=hms(total+live+delta);
            });
            document.querySelectorAll('#gvRows [data-gv-today-id]').forEach(function(x){
                var id=x.dataset.gvTodayId,kind=Number(x.dataset.gvTodayKind),entry=state.todayByIssue[id];
                var committed=asInt(entry&&(kind===1?entry.regular:entry.overtime));
                var live=asInt(x.dataset.gvTodayLive);
                var timer=Array.prototype.find.call(document.querySelectorAll('#gvRows [data-gv-clock]'),function(y){
                    return y.dataset.gvId===id&&Number(y.dataset.gvLiveKind)===kind&&Number(y.dataset.gvStarted)>0;});
                var delta=timer?issueLiveDelta(state.serverNow,now,kind):0;
                x.textContent='Hôm nay: '+(state.todayDay?hms(committed+live+delta):'đang tải…');
            });
        }
        // Giống Task: lấy giờ HÔM NAY từ API nhật ký, không hiển thị số 0 giả khi API list không cấp giờ hôm nay.
        function localIssueDay(){var parts=new Intl.DateTimeFormat('en-US',{timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'}).formatToParts(new Date());
            var values={};parts.forEach(function(x){values[x.type]=x.value;});return values.year+'-'+values.month+'-'+values.day;}
        function loadIssueToday(force){var day=localIssueDay();if(!force&&state.todayDay===day&&Date.now()-state.todayAt<30000)return;
            var seq=++state.todaySeq;state.todayAt=Date.now();
            return request({view:'history',projectId:projectId,day:day},false).then(function(data){if(seq!==state.todaySeq)return;
                var result={};(data.items||[]).forEach(function(x){result[x.id]={regular:asInt(x.regular),overtime:asInt(x.overtime)};});
                state.todayDay=day;state.todayByIssue=result;
                tick();
            }).catch(function(){if(seq!==state.todaySeq)return;
                document.querySelectorAll('#gvRows [data-gv-today-id]').forEach(function(x){x.textContent='Hôm nay: chưa tải được';});});
        }
        function listVisualSnapshot(items){return JSON.stringify((items||[]).map(function(x){
            return [x.id,x.code,x.name,x.status,x.approval,x.assigned,x.manager,x.running,x.worked,
                x.reported,x.withdrawn,x.regularSeconds,x.overtimeSeconds,x.runningOvertime,x.started];
        }));}
        function refreshListView(result){
            var previous=listVisualSnapshot(state.items);
            setIssueListData(result);
            if(previous!==listVisualSnapshot(state.items))renderList();
            else{
                // Khong huy va tao lai hang moi khi polling 5s; bao toan DOM/hover/scroll.
                var rows=Object.create(null);state.items.forEach(function(a){rows[a.id]=a;});
                document.querySelectorAll('#gvRows [data-gv-clock]').forEach(function(x){
                    var a=rows[x.dataset.gvId],kind=Number(x.dataset.gvLiveKind);if(!a)return;
                    x.dataset.gvBase=String(asInt(kind===1?a.regularSeconds:a.overtimeSeconds));
                    x.dataset.gvLive=String(a.running && (a.runningOvertime===(kind===2))?asInt(a.runningLiveSeconds):0);
                    x.dataset.gvStarted=String(a.running && (a.runningOvertime===(kind===2))?a.started||0:0);
                });
                document.querySelectorAll('#gvRows [data-gv-today-id]').forEach(function(x){
                    var a=rows[x.dataset.gvTodayId],kind=Number(x.dataset.gvTodayKind);
                    if(a)x.dataset.gvTodayLive=String(a.running && (a.runningOvertime===(kind===2))?asInt(a.runningTodaySeconds):0);
                });
                tick();
            }
        }
        function loadList(){if(state.loading)return Promise.resolve();state.loading=true;return request({view:'list',projectId:projectId},false).then(function(result){refreshListView(result);loadIssueToday(false);})
            .catch(function(e){warn(e.message,false);}).then(function(){state.loading=false;});}
        function chooseType(type){state.type=type;root.classList.toggle('gv-mode-issue',type==='issue');pane.hidden=type!=='issue';
            ['gvTabTask','gvTabIssue'].forEach(function(id){var e=el(id);if(!e)return;var on=id===(type==='task'?'gvTabTask':'gvTabIssue');e.classList.toggle('is-active',on);e.setAttribute('aria-pressed',String(on));});
            if(type==='issue')loadList();}
        function execute(action,id,note,more){if(state.busy)return Promise.resolve(false);state.busy=true;
            if((action==='start'||action==='startOvertime')&&!issueStartPermitted(action==='start'?1:2)){
                state.busy=false;warn(action==='start'?'Ngoài ca làm việc theo lịch. Không thể bắt đầu giờ thường.':'Chỉ được bắt đầu tăng ca ngoài giờ theo lịch.',false);
                return Promise.resolve(false);
            }
            var args=Object.assign({action:action,projectId:projectId,vanDeId:id,operationId:uuid(),note:note||''},more||{});
            return request(args,true).then(function(res){warn(res.message||'Đã lưu giờ xử lý vấn đề.',true);return loadFresh(id);})
            .then(function(){return true;}).catch(function(e){warn(e.message,false);return false;}).then(function(ok){state.busy=false;return ok;});}
        function loadFresh(id){return request({view:'list',projectId:projectId},false).then(function(x){refreshListView(x);return loadIssueToday(true);});}
        // Issue sử dụng NGUYÊN cấu trúc, tên lớp và footer ExtraModal đang dùng bởi popup Task.
        // Dữ liệu và POST vẫn vào API Issue; tuyệt đối không phát lệnh Task từ các nút Issue.
        function sharedContent(){var box=el('kbg20ReconcileContent');if(!box)throw new Error('Thiếu ExtraModal giờ công dùng chung của Task.');return box;}
        function modalFooter(enabled,label,handler){var foot=el('kbg20ReconcileFooterActions'),back=el('kbg20ManualBack'),save=el('kbg20ManualSave');
            if(!foot||!save)return;foot.hidden=!enabled;if(back)back.hidden=true;save.disabled=false;
            if(enabled){save.textContent=label;save.onclick=handler;}else save.onclick=null;}
        function elem(parent,tag,cls,val){return append(parent,tag,cls,val);}
        function labelField(parent,label,placeholder){var l=elem(parent,'label','kbg20-ui-reason');elem(l,'span','',label);var t=elem(l,'textarea');t.rows=2;t.maxLength=2000;t.placeholder=placeholder||'';return t;}
        function twoDigits(v){return String(v).padStart(2,'0');}
        function minutePair(parent,regular,ot){var wrap=elem(parent,'div','kbg20-ui-hour-pair');[['Giờ làm theo lịch',regular],['Tăng ca',ot]].forEach(function(p){var c=elem(wrap,'div');elem(c,'span','',p[0]);elem(c,'strong','',p[1]==null?'—':hhmm(p[1]));});}
        function systemSecondsPair(parent,regular,ot){var wrap=elem(parent,'div','kbg20-ui-hour-pair');[['Giờ làm theo lịch',regular],['Tăng ca',ot]].forEach(function(p){var c=elem(wrap,'div');elem(c,'span','',p[0]);var t=elem(c,'strong','',systemTime(p[1]));t.title='Giờ hệ thống hiển thị theo giờ và phút, làm tròn như Task.';});}
        function hoursCard(parent,title,desc){var c=elem(parent,'section','kbg20-ui-hours-card');elem(c,'h4','',title);elem(c,'p','kbg20-ui-hours-caption',desc);return c;}
        function timeEntry(parent,label,value){
            if(!window.Kbg20HourInput||typeof window.Kbg20HourInput.createPair!=='function'||
                    typeof window.Kbg20HourInput.readPair!=='function')
                throw new Error('Thiếu bộ nhập Giờ/Phút chuẩn của Task (kbg20-hour-input.js).');
            var pair=window.Kbg20HourInput.createPair(parent,label,asInt(value));
            return {get value(){var n=window.Kbg20HourInput.readPair(pair.hours,pair.minutes);return n===null?'':hhmm(n);}};
        }
                function myReport(info){return (info.reports||[]).find(function(r){return r.userId===info.currentUserId;})||null;}
        function myHistory(info,me){var rows=(info.reportHistory||[]);return me?rows.filter(function(x){return x.employee===me.name;}):[];}
        function issueState(info,me){if(me&&me.chot)return ['PM đã chốt giờ công','is-locked'];if(me&&me.reported)return ['Đã báo hoàn thành','is-reported'];
            if(info.approval===1)return ['Vấn đề chờ duyệt','is-pending'];if(info.approval===3)return ['Đã từ chối · có thể xử lý tiếp','is-withdrawn'];return ['Chưa báo hoàn thành','is-pending'];}
        function issueTop(box,info,eyebrow,me){var top=elem(box,'div','kbg20-ui-top'),name=elem(top,'div','kbg20-ui-top-title');elem(name,'div','kbg20-ui-eyebrow',eyebrow);elem(name,'h3','', '['+(info.code||'VD')+'] '+(info.name||''));
            var st=issueState(info,me);elem(top,'span','kbg20-ui-badge '+st[1],st[0]);return top;}
        // Mot lich su DUY NHAT nhu Task: bao / rut / sua gio va PM chot / dieu chinh.
        // Du lieu PM co the xuat hien trong ca reportHistory va pm-final.history;
        // doi chieu su kien de khong lap card, nhung khong loai bo su kien chua doi chieu duoc.
        function historyPanel(parent,info,me,pmResult){
            var pane=elem(parent,'aside','kbg20-ui-history-panel');
            var reportRows=myHistory(info,me),pmRows=(pmResult&&pmResult.history)||[],entries=[];
            function timestamp(x){var n=Number(x);return Number.isFinite(n)?n:0;}
            function isPmKind(kind){return Number(kind)===4||Number(kind)===6;}
            reportRows.forEach(function(x,order){
                // Event PM tu reportHistory duoc giu neu endpoint pm-final khong tra ban tuong ung.
                var match=isPmKind(x.kind)&&pmRows.some(function(p){
                    var sameKind=Number(p.kind)===Number(x.kind);
                    var nearTime=Math.abs(timestamp(p.createdUtc)-timestamp(x.created))<=30000;
                    var sameRegular=x.pmRegular==null||Number(x.pmRegular)===Number(p.regularMinutes);
                    var sameOvertime=x.pmOvertime==null||Number(x.pmOvertime)===Number(p.overtimeMinutes);
                    return sameKind&&nearTime&&sameRegular&&sameOvertime;
                });
                if(!match)entries.push({source:'report',time:timestamp(x.created),order:order,item:x});
            });
            pmRows.forEach(function(x,order){entries.push({source:'pm',time:timestamp(x.createdUtc),order:reportRows.length+order,item:x});});
            entries.sort(function(a,b){return b.time-a.time||a.order-b.order;});
            var head=elem(pane,'div','kbg20-ui-history-head');
            elem(head,'strong','','Lịch sử báo hoàn thành & chỉnh giờ');
            elem(head,'span','',String(entries.length)+' lần ghi nhận');
            var list=elem(pane,'div','kbg20-ui-history-list');list.tabIndex=0;
            list.setAttribute('aria-label','Lịch sử báo giờ và PM chốt công vấn đề');
            if(!entries.length){elem(list,'p','kbg20-ui-history-empty','Chưa có lịch sử báo phần việc hoặc chốt giờ.');return;}
            var names={1:'Đã báo hoàn thành',2:'Đã rút báo hoàn thành',3:'Đã sửa giờ tự khai',4:'PM chốt công',6:'PM điều chỉnh sau chốt'};
            entries.forEach(function(entry){
                var x=entry.item,fromPm=entry.source==='pm';
                var card=elem(list,'div','kbg20-ui-history-item'),h=elem(card,'div','kbg20-ui-history-item-head');
                elem(h,'strong','',names[Number(x.kind)]||'Cập nhật giờ');
                elem(h,'time','',vnTime(fromPm?x.createdUtc:x.created));
                if(!fromPm&&x.round!=null&&x.version!=null)
                    elem(card,'div','kbg20-ui-history-version','Lần '+x.round+' · Phiên bản '+x.version);
                if(fromPm){
                    elem(card,'div','kbg20-ui-history-hours','Giờ PM chốt: '+hhmm(x.regularMinutes||0)+' làm theo lịch · '+hhmm(x.overtimeMinutes||0)+' tăng ca');
                }else if(isPmKind(x.kind)){
                    if(x.pmRegular!=null||x.pmOvertime!=null)
                        elem(card,'div','kbg20-ui-history-hours','Giờ PM chốt: '+(x.pmRegular==null?'—':hhmm(x.pmRegular))+' làm theo lịch · '+(x.pmOvertime==null?'—':hhmm(x.pmOvertime))+' tăng ca');
                }else if(x.regular!=null||x.overtime!=null){
                    elem(card,'div','kbg20-ui-history-hours','Giờ tự khai: '+hhmm(x.regular||0)+' làm theo lịch · '+hhmm(x.overtime||0)+' tăng ca');
                }
                if(x.reason||x.note)elem(card,'p','kbg20-ui-history-reason',x.reason||x.note);
            });
        }
        function inlineError(box,message){var n=box.querySelector('.kbg20-dot2-notice');if(!n)n=elem(box,'p','kbg20-dot2-notice');n.textContent=message||'';n.classList.add('is-error');}
        function afterIssueSave(info){closeModal('mdlKbg20Manual');loadList();if(state.ledgerMode!=='task')loadCross(false);if(state.historyMode!=='task')loadCross(true);
            if(window.KbgFloatingTimer&&typeof window.KbgFloatingTimer.notifyChanged==='function')window.KbgFloatingTimer.notifyChanged();}
        function saveIssue(info,kind,collect,label,box){modalFooter(true,label,function(){if(state.busy)return;var data=collect();if(!data)return;
            var btn=el('kbg20ManualSave');btn.disabled=true;
            execute(kind,info.id,data.note||'',data).then(function(ok){btn.disabled=false;if(ok)afterIssueSave(info);});});}
        function showReport(info,mode,pmResult,pmError,notificationId){var box=sharedContent(),me=myReport(info),locked=!!(me&&me.chot),reported=!!(me&&me.reported);
            // Giống Task: báo và sửa giờ dùng cùng popup; không bắt người dùng tự chuyển Tab 3.
            var withdraw=mode==='withdraw',report=mode==='report'&&!reported,editing=(mode==='manual'||mode==='report')&&reported&&!locked;
            var canReport=!!info.assigned&&!!info.worked&&!locked&&(info.approval===0||info.approval===1||info.approval===2||info.approval===3);
            var canWithdraw=reported&&!locked&&(info.approval===0||info.approval===3);
            var form=(report&&canReport)||(editing&&canReport)||(withdraw&&canWithdraw),read=!form;
            var eyebrow=withdraw?'RÚT BÁO HOÀN THÀNH':report&&!reported?'BÁO HOÀN THÀNH PHẦN VIỆC':editing?'THÔNG TIN & ĐIỀU CHỈNH GIỜ CÔNG':'THÔNG TIN GIỜ CÔNG';
            box.textContent='';modalFooter(false);issueTop(box,info,eyebrow,me);
            var work=elem(box,'div','kbg20-ui-workspace gv-issue-workspace'),left=elem(work,'div','kbg20-ui-main'),right=elem(work,'div','kbg20-ui-side');
            var sysR=Math.floor((Number(info.regularSeconds)||0)+30)/60,sysO=Math.floor((Number(info.overtimeSeconds)||0)+30)/60;
            sysR=Math.floor(sysR);sysO=Math.floor(sysO);
            var cards=elem(left,'div','kbg20-ui-cards'),sys=hoursCard(cards,'Giờ hệ thống','Thời gian hệ thống đã ghi nhận (chính xác đến giây)');systemSecondsPair(sys,info.regularSeconds,info.overtimeSeconds);
            var own=hoursCard(cards,'Giờ tôi tự khai',report&&!reported?'Mặc định theo giờ hệ thống mới nhất; có thể chỉnh.':editing?'Đây là bản tự khai đang có hiệu lực.':'Số giờ đã khai chính thức của tôi');
            var r=null,o=null;
            if(form&&!withdraw){var inputs=elem(own,'div','kbg20-ui-input-pair');r=timeEntry(inputs,'Giờ làm theo lịch',report&&!reported?sysR:(me&&me.regular!=null?me.regular:sysR));o=timeEntry(inputs,'Tăng ca',report&&!reported?sysO:(me&&me.overtime!=null?me.overtime:sysO));}
            else minutePair(own,me&&me.regular,me&&me.overtime);
            if(withdraw){var w=elem(left,'div','kbg20-ui-help is-warning');elem(w,'strong','','Rút xác nhận hoàn thành phần việc');elem(w,'p','','Thao tác giữ nguyên lịch sử và giờ hệ thống; đồng hồ không tự khởi động lại.');}
            if(report&&!reported){var w2=elem(left,'div','kbg20-ui-help');elem(w2,'p','','Sau khi báo, chỉ đồng hồ của bạn trên vấn đề này được dừng và lưu giờ; đồng nghiệp không bị ảnh hưởng.');}
            var why=form?labelField(left,withdraw?'Lý do rút báo (bắt buộc)':editing?'Lý do / ghi chú sửa giờ':'Ghi chú thêm (không bắt buộc)',withdraw?'Nhập lý do rút báo...':'Ghi chú nếu cần...'):null;
            // Share the SAME personal-hours popup for ordinary "Xem giờ" and PM-final notices.
            // Keep the personal report history; include PM's official figures and notes in this popup.
            if(pmResult && pmResult.latest){
                var latest=pmResult.latest,selected=pmResult.selected;
                if(notificationId && !pmResult.notificationMatched){
                    var missing=elem(left,'div','kbg20-ui-help is-warning');elem(missing,'strong','','Không xác định được bản chốt thuộc thông báo cũ');
                    elem(missing,'p','','Kết quả chính thức mới nhất được hiển thị bên dưới. Không tự gán số giờ mới cho thông báo cũ.');
                }
                if(notificationId && selected && selected.id!==latest.id){
                    var older=elem(left,'div','kbg20-ui-help is-warning');elem(older,'strong','','Kết quả trong thông báo đã được điều chỉnh');
                    elem(older,'p','','Trong thông báo: '+hhmm(selected.regularMinutes)+' giờ theo lịch và '+hhmm(selected.overtimeMinutes)+' giờ tăng ca, lúc '+vnTime(selected.createdUtc)+'.');
                }
                var finalCard=hoursCard(left,'Giờ quản lý đã chốt','Kết quả chính thức mới nhất · Lưu lúc '+vnTime(latest.createdUtc));
                minutePair(finalCard,latest.regularMinutes,latest.overtimeMinutes);
                var pmNote=elem(left,'div','kbg20-ui-help');elem(pmNote,'strong','','Lý do / ghi chú của quản lý');
                elem(pmNote,'p','',latest.reason||'Quản lý chốt theo dữ liệu đối soát, không có ghi chú điều chỉnh.');
                if(notificationId && selected && selected.id!==latest.id && selected.reason){
                    var oldNote=elem(left,'div','kbg20-ui-help is-warning');elem(oldNote,'strong','','Ghi chú của lần chốt trong thông báo');elem(oldNote,'p','',selected.reason);
                }

            } else if(notificationId && (!pmResult || !pmResult.latest) && !pmError){
                var noResult=elem(left,'div','kbg20-ui-help is-warning');elem(noResult,'strong','','Không tìm thấy bản PM chốt giờ');
                elem(noResult,'p','','Thông báo đã được xác thực, nhưng chưa có dữ liệu PM chốt tương ứng để hiển thị.');
            } else if(pmError){
                var fail=elem(left,'div','kbg20-ui-help is-warning');elem(fail,'strong','','Không tải được giờ quản lý đã chốt');
                elem(fail,'p','',pmError.message||'Vui lòng tải lại chi tiết để xem dữ liệu chính thức của PM.');
            }
            if(locked){var lock=elem(left,'div','kbg20-ui-help is-locked');elem(lock,'strong','','PM đã chốt giờ công');elem(lock,'p','','Giờ tự khai đã khóa sửa và rút báo.');}
            if(read&&!locked){var notice=elem(left,'div','kbg20-ui-help');elem(notice,'p','','Bạn chỉ có thể xem thông tin ở trạng thái hiện tại.');}
            historyPanel(right,info,me,pmResult);
            if(form)saveIssue(info,withdraw?'withdrawPersonal':'reportPersonal',function(){if(withdraw){if(!why.value.trim()){inlineError(left,'Bạn cần nhập lý do rút báo.');return null;}
                        return {regularMinutes:0,overtimeMinutes:0,note:why.value.trim(),expectedRound:me?asInt(me.round):0,expectedVersion:me?asInt(me.version):0};}
                var rr=parseHHmm(r.value),oo=parseHHmm(o.value);if(rr==null||oo==null||rr>525600||oo>525600){inlineError(left,'Nhập đúng số Giờ (0–9.999) và Phút (00–59).');return null;}
                return {regularMinutes:rr,overtimeMinutes:oo,note:why.value.trim(),expectedRound:me?asInt(me.round):0,expectedVersion:me?asInt(me.version):0};},withdraw?'Rút báo hoàn thành':report&&!reported?'Xác nhận tôi đã hoàn thành':'Lưu giờ tự khai',left);
        }
        // Gửi duyệt / duyệt nhanh / tính công toàn Issue thực hiện tại các ExtraModal IssueList.
        // KHÔNG POST trực tiếp những lệnh này từ KhaiBaoGio (tránh hai luồng nghiệp vụ).
        function navigateIssueWorkflow(id,mode){
            var destination={submit:'sendIssueId',approve:'reviewIssueId',chot:'pmIssueId'}[mode];
            if(!destination)return false;
            if(!/^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/i.test(id)){
                warn('Mã vấn đề không hợp lệ, không thể mở chức năng duyệt.',false);return true;
            }
            if(!issueListUrl){warn('Chưa cấu hình đường dẫn IssueList trong KhaiBaoGio.aspx.',false);return true;}
            var target;
            try{target=new URL(issueListUrl,window.location.href);}
            catch(e){warn('Đường dẫn IssueList không hợp lệ.',false);return true;}
            // URL dự án đã được RewriteURLHelper tạo ở server. Chỉ thêm action và ID vấn đề.
            target.searchParams.set(destination,id);
            window.location.assign(target.href);
            return true;
        }
        function openIssue(id,mode,notificationId){
            if(navigateIssueWorkflow(id,mode))return Promise.resolve();
            var seq=++state.detailSeq;state.modalId=id;state.modalMode=mode||'report';state.modalInfo=null;
            var box=sharedContent();box.textContent='Đang tải giờ công vấn đề...';modalFooter(false);
            if(!openModal('mdlKbg20Manual','Khai báo giờ cá nhân'))return Promise.resolve();
            return request({view:'detail',projectId:projectId,vanDeId:id},false).then(function(res){
                if(seq!==state.detailSeq)return;
                state.modalInfo=res.item;
                var me=myReport(res.item),isFinal=!!(me&&me.chot),hasNotification=!!notificationId;
                if(!isFinal&&!hasNotification){showReport(res.item,mode,null,null,'');return;}
                var query={view:'pm-final',projectId:projectId,vanDeId:id};
                if(notificationId)query.notificationId=notificationId;
                return request(query,false).then(function(result){if(seq!==state.detailSeq)return;
                    showReport(res.item,hasNotification?'read':mode,result,null,notificationId||'');
                }).catch(function(error){if(seq!==state.detailSeq)return;
                    // Explicit notifications must not silently fall back to current/latest PM figures.
                    showReport(res.item,'read',null,error,notificationId||'');
                    warn(error.message||'Không tải được kết quả chốt giờ công.',false);
                });
            }).catch(function(e){if(seq!==state.detailSeq)return;box.textContent='Không tải được giờ công vấn đề: '+e.message;warn(e.message,false);});}
        function actFromRow(e){var b=e.target.closest('[data-gv-action]');if(!b||b.disabled)return;var action=b.dataset.gvAction,id=b.dataset.gvId;
            if(action==='start'||action==='startOvertime'||action==='stop')execute(action,id,'');else openIssue(id,action);}
        // TAB 2: su dung chinh ExtraModal chi tiet gio cua Task, khong dieu huong tab.
        function openHistoryIssue(item){var box=el('kbg20HistoryDetailBody'),title=el('kbg20HistoryDetailTitle');if(!box||!title)return;
            var date=el('kbg20HistoryDate').value;title.textContent='['+(item.code||'VD')+'] '+item.name+' · '+date.split('-').reverse().join('/');
            box.textContent='Đang tải các phiên xử lý trong ngày...';openModal('mdlKbg20HistoryDetails','Chi tiết các lần tính giờ');
            var seq=++state.historySeq;
            request({view:'history-sessions',projectId:projectId,vanDeId:item.id,day:date},false).then(function(res){if(seq!==state.historySeq)return;box.textContent='';
                if(!res.items||!res.items.length){append(box,'p','kbg20-ux-muted','Không có phiên xử lý trong ngày đã chọn.');return;}
                var list=append(box,'div','kbg20-ux-session-list');res.items.forEach(function(x,i){var node=append(list,'section','kbg20-ux-session-item gv-issue-session');
                    var h=append(node,'div','kbg20-ux-session-head');append(h,'strong','',String(i+1)+'. '+(x.overtime?'Giờ tăng ca':'Giờ làm theo lịch'));
                    append(h,'span','',x.ended==null?'Phiên đang mở':'Đã kết thúc');var g=append(node,'div','kbg20-ux-session-grid');
                    function f(label,val){var cell=append(g,'div');append(cell,'small','',label);append(cell,'strong','',val);}
                    f('Bắt đầu',vnTime(x.started));
                    var origin=append(g,'div');append(origin,'small','','Nguồn bắt đầu');
                    var source=append(origin,'strong','',x.source==='manual'?'Thủ công':'Chưa xác định');
                    issueTooltip(source,'Nguồn bắt đầu','Phiên Issue được bắt đầu thủ công theo quy tắc nghiệp vụ Issue.');
                    f('Kết thúc',x.ended==null?'Chưa kết thúc':vnTime(x.ended));f('Đã kết toán trong ngày',hms(x.committedSeconds));
                });
            }).catch(function(e){if(seq===state.historySeq){box.textContent='';append(box,'p','kbg20-ux-muted','Không tải được lịch sử: '+e.message);}});
        }
        function readTaskTotals(){var elts=el('kbg20HistoryTotals');if(!elts)return [0,0,0];var vals=elts.querySelectorAll('strong');
            function parse(s){var m=/(\d+):(\d\d):(\d\d)/.exec(s||'');return m?Number(m[1])*3600+Number(m[2])*60+Number(m[3]):0;}
            return Array.prototype.slice.call(vals,0,3).map(function(v){return parse(v.textContent);}).concat([0,0,0]).slice(0,3);}
        function renderHistoryTotals(){var mode=state.historyMode,combined=el('gvHistoryTotalsCombined'),task=el('kbg20HistoryTotals');if(!combined||!task)return;
            if(mode==='task'){combined.hidden=true;task.hidden=false;return;}
            state.taskTotals=readTaskTotals();task.hidden=true;combined.hidden=false;combined.textContent='';
            var a=mode==='all'?state.taskTotals:[0,0,0],b=state.issueTotals;
            var grid=append(combined,'div','kbg20-v41-history-summary');[
                ['Giờ làm theo lịch · '+(mode==='all'?'Công việc + Vấn đề':'Vấn đề'),a[0]+b[0]],
                ['Giờ tăng ca · '+(mode==='all'?'Công việc + Vấn đề':'Vấn đề'),a[1]+b[1]],
                ['Tổng giờ công đã ghi',a[2]+b[2]]
            ].forEach(function(item){var box=append(grid,'div','kbg20-v41-history-stat');append(box,'small','',item[0]);append(box,'strong','',hms(item[1]));});
        }
        function renderHistoryIssues(){
            var host=el('gvHistoryIssues');if(!host)return;host.textContent='';var list=state.historyItems;
            if(!list.length){append(host,'p','kbg20-empty','Không có giờ xử lý vấn đề trong ngày được chọn.');return;}
            var hdr=append(host,'div','kbg20-v41-hist-head kbg20-v41-hist-grid');
            ['Vấn đề','Giờ làm theo lịch','Giờ tăng ca','Tổng giờ công','Chi tiết'].forEach(function(x){append(hdr,'span','',x);});
            list.forEach(function(x){
                var line=append(host,'article','kbg20-v41-hist-row kbg20-v41-hist-grid');
                var name=taskCell(line,'kbg20-v41-hist-name','Vấn đề');
                append(name,'strong','',x.code||'VD');append(name,'span','',x.name||'Chưa có tên vấn đề');
                var badges=append(name,'span','kbg20-d6-badges');
                var badge=append(badges,'span','kbg20-d6-badge is-source-manual','Thủ công');
                issueTooltip(badge,'Thủ công','Có phiên được nhân viên trực tiếp bắt đầu trong ngày này.');
                var ledger=ledgerForIssue(x.id);
                if(ledger){
                    var personal=issueLedgerPersonal(ledger);
                    if(personal.mode!=='empty'){
                        var personalBadge=append(badges,'span','kbg20-d6-badge '+personal.kind,personal.name);
                        if(personal.mode==='locked')issueTooltip(personalBadge,personal.name,'Giờ công đã được PM chốt và khóa chỉnh sửa.');
                    }
                }
                if(x.running)append(badges,'span','kbg20-d6-badge is-open','Phiên đang mở');
                [x.regular,x.overtime,x.seconds].forEach(function(n,i){
                    var cell=taskCell(line,'kbg20-v41-hist-value',i===0?'Giờ làm theo lịch':i===1?'Giờ tăng ca':'Tổng giờ công');
                    append(cell,'strong',i===2?'is-total':'',hms(n));
                });
                var actions=taskCell(line,'kbg20-ux-history-actions','Chi tiết');
                var detail=append(actions,'button','kbg20-btn kbg20-outline kbg20-ux-detail-btn','Xem chi tiết');
                detail.type='button';detail.setAttribute('aria-label','Xem chi tiết phiên giờ của '+(x.code||'vấn đề'));
                detail.addEventListener('click',function(){openHistoryIssue(x);});
            });
        }
                // Tab 3: cùng tên trạng thái, badge, nút và thứ tự nhánh như Task trong kbg20-doi-soat.js.
        // API Issue trả ba cờ reported/withdrawn/chot và approval, không trả t.status của Task.
        function issueLedgerPersonal(x){
            if(x.chot)return {name:'PM đã chốt giờ công',kind:'is-locked',mode:'locked'};
            if(x.withdrawn)return {name:'Đã rút báo hoàn thành',kind:'is-withdrawn',mode:'withdrawn'};
            if(x.reported)return {name:'Đã báo hoàn thành',kind:'is-reported',mode:'reported'};
            return {name:'Chưa báo hoàn thành',kind:'is-pending',mode:'empty'};
        }
        function issueCanWithdraw(x){return !!x.reported&&!x.chot&&(Number(x.approval)===0||Number(x.approval)===3);}
        function issueCannotWithdrawReason(x){
            if(Number(x.approval)===1)return 'Vấn đề đã gửi duyệt. Không thể rút báo khi đang chờ PM duyệt.';
            if(Number(x.approval)===2)return 'Vấn đề đã được PM duyệt hoàn thành. Không thể rút báo.';
            return 'Giờ công đã bị khóa hoặc trạng thái hiện tại không cho phép rút báo.';
        }
        function renderLedgerIssues(){
            var host=el('gvLedgerIssues');if(!host)return;host.textContent='';
            var list=state.ledgerItems.filter(function(x){return state.ledgerFilter==='all'||
                (state.ledgerFilter==='reported'&&x.reported)||(state.ledgerFilter==='withdrawn'&&x.withdrawn);});
            if(!list.length){elem(host,'p','kbg20-ui-empty','Chưa có vấn đề phù hợp với bộ lọc.');return;}
            var head=elem(host,'div','kbg20-ui-table-head kbg20-ui-table-grid');
            ['Vấn đề','Giờ làm theo lịch','Giờ tăng ca','Báo hoàn thành','Thao tác'].forEach(function(x){elem(head,'span','',x);});
            list.forEach(function(x){
                var row=elem(host,'article','kbg20-ui-table-row kbg20-ui-table-grid');
                var name=elem(row,'div','kbg20-ui-cell-task');
                elem(name,'strong','', '['+(x.code||'VD')+'] '+(x.name||''));
                elem(name,'span','','Lần báo '+(x.round||0)+' · Phiên bản '+(x.version||0));
                function amount(label,seconds,declared){
                    var cell=elem(row,'div','kbg20-ui-cell-times');elem(cell,'small','kbg20-v41-mobile-label',label);
                    var sy=elem(cell,'div','kbg20-ui-cell-sys');elem(sy,'span','','Hệ thống');
                    elem(sy,'strong','',systemTime(seconds));
                    var us=elem(cell,'div','kbg20-ui-cell-self');elem(us,'span','','Tự khai');
                    elem(us,'strong','',declared==null?'—':hhmm(declared));
                }
                amount('Giờ làm theo lịch',x.systemRegularSeconds,x.regular);
                amount('Giờ tăng ca',x.systemOvertimeSeconds,x.overtime);
                var stateCell=elem(row,'div','kbg20-ui-cell-status'),personal=issueLedgerPersonal(x);
                elem(stateCell,'span','kbg20-ui-badge '+personal.kind,personal.name);
                if(personal.mode==='withdrawn')elem(stateCell,'small','','Có thể báo lại khi được phép xử lý');
                else if(personal.mode==='locked')elem(stateCell,'small','','Giờ tự khai đã khóa chỉnh sửa');
                var actions=elem(row,'div','kbg20-ui-cell-actions');
                function choice(label,mode,cls,disabled,reason){
                    var b=elem(actions,'button','kbg20-ui-action '+(cls||''),label);b.type='button';
                    if(disabled){
                        b.disabled=true;b.setAttribute('aria-label',label+'. '+reason);
                        var wrap=elem(actions,'span','gv-ledger-action-lock');
                        actions.insertBefore(wrap,b);wrap.appendChild(b);
                        issueTooltip(wrap,label,reason);
                    }else b.addEventListener('click',function(){openIssue(x.id,mode);});
                    return b;
                }
                if(personal.mode==='locked'){choice('Xem giờ','read','is-neutral');}
                else if(personal.mode==='withdrawn'){choice('Báo lại','report','is-primary');}
                else if(personal.mode==='reported'){
                    choice('Sửa giờ','manual','is-primary');
                    var mayWithdraw=issueCanWithdraw(x);
                    choice('Rút báo','withdraw','is-secondary',!mayWithdraw,mayWithdraw?'':issueCannotWithdrawReason(x));
                }
                else if(asInt(x.seconds)>0){choice('Báo hoàn thành phần việc','report','is-primary');}
                else choice('Xem giờ','read','is-neutral');
            });
        }
        function loadCross(history){var mode=history?state.historyMode:state.ledgerMode;if(mode==='task')return Promise.resolve();
            var seq=history?++state.historySeq:++state.ledgerSeq;
            var p={view:history?'history':'ledger',projectId:projectId};if(history){p.day=el('kbg20HistoryDate').value;if(!p.day)return Promise.resolve();}
            return request(p,false).then(function(res){if(history){if(seq!==state.historySeq)return;state.historyItems=res.items||[];
                    state.issueTotals=state.historyItems.reduce(function(a,x){a[0]+=asInt(x.regular);a[1]+=asInt(x.overtime);a[2]+=asInt(x.seconds);return a;},[0,0,0]);renderHistoryIssues();renderHistoryTotals();}
                else{if(seq!==state.ledgerSeq)return;state.ledgerItems=res.items||[];renderLedgerIssues();
                    if(state.historyMode!=='task'&&state.historyItems.length)renderHistoryIssues();
                    if(state.type==='issue'&&state.items.length)renderList();}
            }).catch(function(e){warn('Không tải được dữ liệu vấn đề: '+e.message,false);});}
        function cross(id,mode){var history=id==='gvHistoryFilter',prefix=history?'gvHistory':'gvLedger';
            if(history)state.historyMode=mode;else state.ledgerMode=mode;
            show(history?'kbg20History':'kbg20ManualList',mode!=='issue');show(history?'gvHistoryIssues':'gvLedgerIssues',mode!=='task');
            show(prefix+'TaskTitle',mode!=='issue');show(prefix+'IssueTitle',mode!=='task');
            Array.prototype.forEach.call(el(id).querySelectorAll('[data-gv-cross]'),function(x){x.classList.toggle('is-selected',x.dataset.gvCross===mode);});
            // Dac biet KHONG an khoi tong gio, van giu nguyen chieu cao phan header.
            if(history)renderHistoryTotals();if(mode!=='task')loadCross(history);
        }
        el('gvTabTask').addEventListener('click',function(){chooseType('task');});el('gvTabIssue').addEventListener('click',function(){chooseType('issue');});
        el('gvFilters').addEventListener('click',function(e){var btn=e.target.closest('[data-gv-filter]');if(!btn)return;state.filter=btn.dataset.gvFilter;state.page=1;renderList();});
        el('gvSearch').addEventListener('input',function(){state.search=this.value.trim();state.page=1;renderList();});
        el('gvReload').addEventListener('click',loadList);el('gvPrev').addEventListener('click',function(){state.page--;renderList();});el('gvNext').addEventListener('click',function(){state.page++;renderList();});
        el('gvRows').addEventListener('click',actFromRow);
        ['gvHistoryFilter','gvLedgerFilter'].forEach(function(id){el(id).addEventListener('click',function(e){var b=e.target.closest('[data-gv-cross]');if(b)cross(id,b.dataset.gvCross);});});
        el('kbg20ManualFilters').addEventListener('click',function(e){var b=e.target.closest('[data-manual-filter]');if(b){state.ledgerFilter=b.dataset.manualFilter;renderLedgerIssues();}});
        function dayChanged(){window.setTimeout(function(){if(state.historyMode!=='task')loadCross(true);},80);}
        el('kbg20HistoryDate').addEventListener('change',dayChanged);
        ['kbg20HistoryPrev','kbg20HistoryNext','kbg20HistoryToday'].forEach(function(id){el(id).addEventListener('click',dayChanged);});
        el('kbg20TabHistory').addEventListener('click',dayChanged);
        el('kbg20TabManual').addEventListener('click',function(){if(state.ledgerMode!=='task')loadCross(false);});
        el('gvCancelConfirm').addEventListener('click',function(){closeModal('mdlGioXuLyVanDeConfirm');});
        // Task renderer repopulates totals asynchronously. Read its values without changing its DOM.
        var taskTotals=el('kbg20HistoryTotals');if(taskTotals&&window.MutationObserver){var obs=new MutationObserver(function(){if(state.historyMode!=='task')renderHistoryTotals();});
            obs.observe(taskTotals,{childList:true,subtree:true,characterData:true});}
        cross('gvHistoryFilter','all');cross('gvLedgerFilter','all');
        document.addEventListener('qlda:notification-poll-complete',function(){if(!document.hidden&&state.type==='issue')loadList();});
        document.addEventListener('visibilitychange',function(){if(!document.hidden&&state.type==='issue')loadList();});
        window.setInterval(tick,1000);
        if(/^[0-9a-f-]{36}$/i.test(pmFinalIssueId)){
            // PM-final notification uses the same personal-hour ExtraModal as Tab 3 "Xem giờ".
            // Keep the notification id so the endpoint can validate ownership and historical result.
            chooseType('issue');openIssue(pmFinalIssueId,'read',pmFinalNotificationId);
            if(window.history&&window.history.replaceState){var cleaned=new URL(window.location.href);
                cleaned.searchParams.delete('pmFinalIssueId');cleaned.searchParams.delete('pmFinalNotificationId');
                window.history.replaceState(null,'',cleaned.toString());}
        }else if(/^[0-9a-f-]{36}$/i.test(deepId)){chooseType('issue');
            var deepMode={review:'approve',approve:'approve',submit:'submit',chot:'chot',pm:'chot',report:'report',withdraw:'withdraw',manual:'manual',detail:'read'}[deepAction]||'report';
            openIssue(deepId,deepMode);
            if(window.history&&window.history.replaceState){var uri=new URL(window.location.href);uri.searchParams.delete('vanDeId');uri.searchParams.delete('gvAction');window.history.replaceState(null,'',uri.toString());}}
        // Expose small tested interface for future task/notification integrations.
        window.GioXuLyVanDeUI={chooseType:chooseType,openIssue:openIssue,refresh:loadList};
    }
    if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',init);else init();
})(window,document);
