/* KhaiBaoGio 2.0: only active on #kbg20Page. Does not modify V1 handlers. */
(function (window, document) {
    'use strict';
    var root = document.getElementById('kbg20Page');
    if (!root) return;
    var url = root.getAttribute('data-api-url');
    var token = root.getAttribute('data-request-token');
    var state = null, loading = false, acting = false, offset = 0;
    var fixedProjectId = root.getAttribute('data-project-id') || '';
    var projectName = document.getElementById('kbg20ProjectName');
    var tasksEl = document.getElementById('kbg20Tasks');
    var historyEl = document.getElementById('kbg20History');
    var historyTotalsEl = document.getElementById('kbg20HistoryTotals');
    var historyStatusEl = document.getElementById('kbg20HistoryStatus');
    var historyDateEl = document.getElementById('kbg20HistoryDate');
    var historyNextEl = document.getElementById('kbg20HistoryNext');
    var scheduleEl = document.getElementById('kbg20Schedule');
    var summaryEl = document.getElementById('kbg20Summary');
    var messageEl = document.getElementById('kbg20Message');
    var searchEl = document.getElementById('kbg20Search');
    var filter = '';
    var taskFilter = 'today';
    var visibleCount = 6, pageSize = 6;
    var lastFetch = 0;
    var viewMode = 'work', historyData = null, historyRequest = 0, historyLoading = false;
    var todayTaskTimes = Object.create(null), todayDataDay = '', todayRequest = 0;
    // SignalR chi bao database da doi. Dung rendering tai cho task dang dong bo,
    // sau do GET API de lay thoi gian va trang thai chinh thuc.
    var realtimeSyncingTasks = Object.create(null);
    var realtimeRefreshPending = false, realtimeRetry = null, realtimeRetryCount = 0;
    var seenRealtimeIds = Object.create(null), seenRealtimeCount = 0;
    // Bo phan hoi GET bat dau truoc khi SignalR bao du lieu da doi.
    var realtimeEpoch = 0;


    function node(tag, className, value) {
        var x = document.createElement(tag);
        if (className) x.className = className;
        if (value !== undefined && value !== null) x.textContent = String(value);
        return x;
    }
    function row(parent, child) { parent.appendChild(child); return child; }
    function clock(total) {
        total = Math.max(0, Math.floor(Number(total) || 0));
        var h = Math.floor(total / 3600), m = Math.floor(total % 3600 / 60), s = total % 60;
        return String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0') + ':' + String(s).padStart(2, '0');
    }
    function localTime(ms) {
        if (ms == null) return 'Đang mở';
        try { return new Intl.DateTimeFormat('vi-VN', {
            timeZone: 'Asia/Ho_Chi_Minh', day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
        }).format(new Date(Number(ms))); } catch (_) { return '—'; }
    }
    function notify(text, type) {
        messageEl.textContent = text || '';
        messageEl.className = type ? 'kbg20-' + type : '';
    }
    function getJson(response) {
        var contentType = response.headers.get('content-type') || '';
        if (contentType.toLowerCase().indexOf('application/json') < 0)
            throw new Error('Máy chủ trả về nội dung không phải JSON. Kiểm tra đăng nhập và cấu hình URL.');
        return response.json().then(function (obj) {
            if (!response.ok) throw new Error(obj.error || ('HTTP ' + response.status));
            return obj;
        });
    }
    function serverNow() { return Date.now() + offset; }
    function liveSeconds(timer) {
        var base = Number(timer.seconds) || 0;
        if (!timer.counting || !state) return base;
        var end = Math.min(serverNow(), timer.boundary == null ? Number.MAX_SAFE_INTEGER : Number(timer.boundary));
        return base + Math.max(0, Math.floor((end - Number(state.serverNow)) / 1000));
    }
    function button(text, action, id) {
        var b = node('button', 'kbg20-btn ' + (action === 'stop' ? 'kbg20-stop' : ''), text);
        b.type = 'button'; b.setAttribute('data-action', action); b.setAttribute('data-id', id);
        b.disabled = acting;
        return b;
    }
    function renderProjectContext() {
        if (!state) return;
        var project = (state.projects || []).find(function (p) {
            return String(p.id || '').toLowerCase() === fixedProjectId.toLowerCase();
        });
        if (projectName) projectName.textContent = project ?
            ((project.code ? project.code + ' — ' : '') + (project.name || 'Dự án')) : 'Dự án hiện tại';
        var headerDate=document.getElementById('kbg20HeaderDate');
        if(headerDate && state.workCalendar) headerDate.textContent=vietnameseDay(state.workCalendar.day);
    }
    // So phien mo trong DU AN dang xem, khong phai tong so gio hay so task.
    function renderSummary() {
        summaryEl.textContent = '';
        var count = (state.timers || []).length;
        var summary = row(summaryEl, node('div', 'kbg20-open-hint'));
        row(summary, node('i', 'fas fa-stopwatch'));
        var pendingCount = Object.keys(realtimeSyncingTasks).length;
        var link = row(summary, node('button', 'kbg20-summary-link', pendingCount ?
            'Đang đồng bộ phiên giờ…' : count + ' phiên đang bật'));
        link.type = 'button';
        link.title = 'Xem các công việc có đồng hồ chưa Dừng trong dự án này';
        link.addEventListener('click', function () { setTaskFilter('open'); });
        row(summary, node('span', '', 'của bạn trong dự án này · bao gồm cả công việc quá hạn'));
    }
    function scheduleState() {
        var cal = state && state.workCalendar;
        if (!cal || !cal.shifts || !cal.shifts.length) return 'Ngày nghỉ';
        var segs = cal.shifts.slice().sort(function(a,b){return a.startMs-b.startMs;});
        var now = serverNow();
        if (now < Number(segs[0].startMs)) return 'Chưa đến ca';
        for (var i = 0; i < segs.length; i++) {
            if (now >= Number(segs[i].startMs) && now < Number(segs[i].endMs)) return 'Trong giờ làm việc';
            if (i + 1 < segs.length && now >= Number(segs[i].endMs) && now < Number(segs[i+1].startMs))
                return 'Đang nghỉ trưa';
        }
        return 'Đã hết ca hôm nay';
    }
    function idleClockStatus(type) {
        if (type === 2) return 'Không cộng tăng ca';
        var status = scheduleState();
        if (status === 'Đang nghỉ trưa') return 'Nghỉ giữa ca';
        if (status === 'Chưa đến ca') return 'Chờ đến ca';
        if (status === 'Ngày nghỉ') return 'Ngày nghỉ';
        if (status === 'Đã hết ca hôm nay') return 'Ngoài ca';
        return 'Tạm ngừng cộng giờ';
    }
    function setScheduleStatus(el) {
        if (!el) return;
        var current = scheduleState();
        el.textContent = current;
        el.classList.toggle('is-lunch', current === 'Đang nghỉ trưa');
        el.classList.toggle('is-in-shift', current === 'Trong giờ làm việc');
        el.classList.toggle('is-day-off', current === 'Ngày nghỉ');
    }
    function vietnameseDay(dayIso) {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(dayIso || '')) return '';
        try { return new Intl.DateTimeFormat('vi-VN', {
            weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric', timeZone: 'UTC'
        }).format(new Date(dayIso + 'T12:00:00Z')); }
        catch (_) { return dayIso.split('-').reverse().join('/'); }
    }
    function renderSchedule() {
        scheduleEl.textContent = '';
        var cal = state && state.workCalendar;
        if (!cal) return;
        var shifts = (cal.shifts || []).slice().sort(function(a,b){return a.startMs-b.startMs;});
        var morning=shifts.filter(function(x){return Number(x.type)===1;});
        var afternoon=shifts.filter(function(x){return Number(x.type)===2;});
        if(!morning.length && !afternoon.length && shifts.length) morning=shifts.slice();
        var intro=row(scheduleEl,node('div','kbg20-v41-schedule-title'));
        var caption=row(intro,node('strong',''));
        row(caption,node('i','far fa-calendar-alt'));
        row(caption,node('span','', 'Lịch làm việc hôm nay'));
        row(intro,node('small','',vietnameseDay(cal.day)));
        var slots=row(scheduleEl,node('div','kbg20-v41-shifts'));
        function times(a){return a.length ? a.map(function(x){return (x.start||'')+'–'+(x.end||'');}).join(', ') : 'Không làm việc';}
        function current(a){return a.some(function(x){return serverNow()>=Number(x.startMs)&&serverNow()<Number(x.endMs);});}
        function slot(icon,label,value,list,cls){
            var item=row(slots,node('div','kbg20-v41-shift'+(cls||'')+(current(list)?' is-current':'')));
            row(item,node('i',icon));var info=row(item,node('div',''));
            row(info,node('strong','',label));row(info,node('small','',value));
            return item;
        }
        slot('fas fa-sun','Ca sáng',times(morning),morning,'');
        var breakText='Không áp dụng',isBreak=false;
        if(morning.length && afternoon.length){
            var a=morning[morning.length-1],b=afternoon[0];
            if(Number(b.startMs)>Number(a.endMs)){
                breakText=a.end+'–'+b.start;
                isBreak=serverNow()>=Number(a.endMs)&&serverNow()<Number(b.startMs);
            }
        }
        slot('fas fa-coffee','Nghỉ trưa',breakText,[],isBreak?' is-rest':'');
        slot('fas fa-cloud-sun','Ca chiều',times(afternoon),afternoon,'');
        if(!shifts.length)row(intro,node('small','kbg20-v41-dayoff',cal.dayOffReason||'Ngày nghỉ'));
        // Keep a status node for the existing live ticker, but avoid the redundant status badge.
        var status=row(scheduleEl,node('span','kbg20-v41-status-sr',scheduleState()));
        status.id='kbg20ScheduleStatus';status.setAttribute('aria-live','off');
    }
    function getAllTasks() {
        var tasks = (state.tasks || []).slice();
        var timers = state.timers || [];
        timers.forEach(function (timer) {
            if (tasks.some(function(t){return t.id === timer.taskId;})) return;
            tasks.push({id: timer.taskId, code: timer.code, name: timer.name,
                status:-1, assigned:false, pending:false, regular:0, overtime:0,
                startedRegular:true, startedOvertime:true, missing:true});
        });
        return tasks;
    }
    function isFinished(task) { return task.status === 2 || task.status === 3; }
    function isPending(task) { return task.status === 4 || !!task.pending; }
    // V4.2: các bộ đếm và danh sách dùng CÙNG một vị từ phân loại.
    // "Hôm nay": đúng khoảng ngày dự kiến; "Sắp tới": mọi task bắt đầu sau hôm nay.
    // Các phiên vẫn còn mở ngoài khoảng ngày luôn có thể truy cập qua "Đang tính giờ".
    function taskBelongs(task, mode) {
        var today = state && state.workCalendar && state.workCalendar.day;
        var hasOpen = (state.timers || []).some(function(t){return t.taskId === task.id;});
        if (mode === 'open') return hasOpen;
        var accessible = (task.assigned || hasOpen) && !task.missing && !isFinished(task);
        if (!accessible) return false;
        if (mode === 'pending') return isPending(task);
        if (!today) return false;
        if (mode === 'overdue') return !!task.endDay && task.endDay < today;
        if (mode === 'upcoming') return !!task.startDay && task.startDay > today;
        if (mode === 'today') return !!task.startDay && !!task.endDay &&
            task.startDay <= today && task.endDay >= today;
        return false;
    }
    function setTaskFilter(mode) {
        if (['today','overdue','upcoming','open','pending'].indexOf(mode) < 0) return;
        if(taskFilter !== mode) visibleCount = pageSize;
        taskFilter = mode;
        if (state) renderTasks();
    }
    function timerOf(taskId, type) {
        return (state.timers || []).filter(function(t) {
            return t.taskId === taskId && Number(t.type) === type;
        })[0] || null;
    }
    function timeForTask(task, type) {
        var t = timerOf(task.id,type);
        var base = Number(type===1 ? task.regular : task.overtime)||0;
        return clock(base + (t ? Math.max(0,liveSeconds(t)-Number(t.seconds||0)) : 0));
    }
    function taskStatus(task) {
        if(task.missing) return 'Không còn trong danh sách';
        if(isPending(task)) return 'Chờ PM duyệt';
        if(task.status === 2) return 'Hoàn thành';
        if(task.status === 3) return 'Hoàn thành trễ';
        return task.status === 1 ? 'Đang thực hiện' : 'Chưa bắt đầu';
    }
    function taskMayStart(task) {
        return !task.missing && task.assigned && (task.status===0 || task.status===1) && !isPending(task);
    }
    // V4.2 soft table: same DOM row for idle/running/pending; actions remain bound
    // to the existing V2 endpoint and partner workflow postback.
    function iconAction(label,icon,action,id,kind,isOvertime) {
        var btn=button('',action,id);
        btn.className += ' kbg20-v41-icon-action '+kind+(isOvertime?' is-overtime':'');
        btn.title=label;btn.setAttribute('aria-label',label);
        var symbol=icon.indexOf('stop')>=0?'■':icon.indexOf('redo')>=0?'↻':'▶';
        var glyph=row(btn,node('span','kbg20-v41-glyph',symbol));
        glyph.setAttribute('aria-hidden','true');
        // The moon is a visual modifier; the button's accessible name stays explicit.
        if (isOvertime) {
            var moon=row(btn,node('span','kbg20-v41-overtime-moon','☾'));
            moon.setAttribute('aria-hidden','true');
        }
        return btn;
    }
    function taskCell(parent,cls,heading,value){
        var el=row(parent,node('div',cls));
        if(heading)row(el,node('small','kbg20-v41-mobile-label',heading));
        if(value!=null)row(el,node('span','',value));
        return el;
    }
    function clockValue(parent,task,type) {
        var el=row(parent,node('strong','kbg20-v41-clock',timeForTask(task,type)));
        el.setAttribute('data-total-task',task.id);
        el.setAttribute('data-total-type',String(type));
        return el;
    }

    // Ngày xem hôm nay lấy từ API lịch sử (theo k.NgayLam, không phải thời gian phiên Start).
    // Phần live chỉ cộng phần chưa kết toán, tránh tính giờ đã ghi hai lần.
    function todaySeconds(taskId,type){
        var t=todayTaskTimes[taskId];
        var committed=Number(t && (type===1?t.regular:t.overtime))||0;
        var open=state && (state.timers||[]).find(function(x){return x.taskId===taskId&&Number(x.type)===type;});
        return Math.max(0,committed+(open?Math.max(0,liveSeconds(open)-Number(open.seconds||0)):0));
    }
    function updateTodayLabel(el,taskId,type){
        el.textContent='Hôm nay: '+(realtimeSyncingTasks[String(taskId || '').toLowerCase()] ?
            'đang đồng bộ…' : (todayDataDay?clock(todaySeconds(taskId,type)):'đang tải…'));
    }
    function refreshToday(){
        if(!state||!state.workCalendar||!fixedProjectId)return;
        var day=state.workCalendar.day,request=++todayRequest;
        var q='?view=history&projectId='+encodeURIComponent(fixedProjectId)+'&day='+encodeURIComponent(day);
        window.fetch(url+q,{credentials:'same-origin',cache:'no-store'}).then(getJson).then(function(d){
            if(request!==todayRequest || d.day!==day)return;
            var next=Object.create(null);
            (d.tasks||[]).forEach(function(t){next[t.id]={regular:Number(t.regular)||0,overtime:Number(t.overtime)||0};});
            todayTaskTimes=next;todayDataDay=day;
            tasksEl.querySelectorAll('[data-today-task]').forEach(function(el){updateTodayLabel(el,el.getAttribute('data-today-task'),Number(el.getAttribute('data-today-type')));});
        }).catch(function(){
            if(request!==todayRequest)return;
            todayDataDay='';
            tasksEl.querySelectorAll('[data-today-task]').forEach(function(el){el.textContent='Giờ hôm nay: chưa tải được';});
        });
    }

    function overtimeAllowedBySchedule(){
        var cal=state && state.workCalendar,shifts=cal && cal.shifts||[];
        if(!shifts.length)return false;
        var lastEnd=Math.max.apply(null,shifts.map(function(sh){return Number(sh.endMs)||0;}));
        return lastEnd>0 && serverNow()>=lastEnd;
    }
    function taskClockCell(parent,task,type,isUpcoming){
        var timer=timerOf(task.id,type),regular=type===1;
        var cell=taskCell(parent,'kbg20-v41-clock-cell'+(regular?' is-regular':' is-overtime'),regular?'Giờ trong ca':'Tăng ca');
        var stats=row(cell,node('div','kbg20-v41-clock-main'));
        var digits=clockValue(stats,task,type,regular);
        digits.className='kbg20-v41-clock';
        var sub=row(stats,node('small','kbg20-v41-clock-sub'));
        if (realtimeSyncingTasks[task.id]) sub.textContent='Đã chốt · đang đồng bộ với máy chủ';
        else if(isPending(task))sub.textContent=timer?'Phiên còn mở · cần chốt':'Đã tạm dừng';
        else if(timer){
            sub.textContent=timer.counting?(regular?'Đang cộng giờ trong ca':'Đang cộng tăng ca'):'Đang mở · '+idleClockStatus(type);
            sub.setAttribute('data-timer-status',timer.id);
        } else sub.textContent=(regular?task.startedRegular:task.startedOvertime)||Number(regular?task.regular:task.overtime)>0
            ?'Giờ tích lũy':'Chưa ghi '+(regular?'giờ trong ca':'tăng ca');
        var todaySub=row(stats,node('small','kbg20-v43-today'));todaySub.setAttribute('data-today-task',task.id);
        todaySub.setAttribute('data-today-type',String(type));
        updateTodayLabel(todaySub,task.id,type);
        if(realtimeSyncingTasks[task.id]){
            var pending=iconAction('Đã chốt giờ · đang cập nhật từ máy chủ','fas fa-stop','stop',
                timer?timer.id:task.id,'is-disabled',!regular);
            pending.disabled=true;cell.insertBefore(pending,stats);
        } else if(timer){
            var title=regular?'Dừng giờ trong ca':'Dừng tăng ca';
            cell.insertBefore(iconAction(title,'fas fa-stop','stop',timer.id,'is-stop',!regular),stats);
        } else if(!isUpcoming && taskMayStart(task) && (regular || overtimeAllowedBySchedule())){
            var resume=regular && (task.startedRegular || Number(task.regular)>0) ||
                !regular && (task.startedOvertime || Number(task.overtime)>0);
            var action=regular?'start':'overtime';
            var label=(resume?'Tiếp tục':'Bắt đầu')+(regular?' giờ trong ca':' tăng ca');
            if(task.status===0 && state.workflowEnabled)label+=' · Chuyển công việc sang Đang thực hiện trước khi bấm giờ';
            cell.insertBefore(iconAction(label,resume?'fas fa-redo-alt':'fas fa-play',action,task.id,
                resume?'is-resume':'is-start',!regular),stats);
        }else{
            var reason=isPending(task)?'Đang chờ PM duyệt':isFinished(task)?'Công việc đã hoàn thành':
                isUpcoming?'Chưa tới ngày bắt đầu':(!regular && !overtimeAllowedBySchedule()
                ?'Chỉ có thể bắt đầu tăng ca sau ca làm việc cuối cùng theo lịch hiện tại':
                (!task.assigned?'Bạn chưa được phân công':'Chưa đủ điều kiện bắt đầu'));
            var disabled=iconAction(regular?reason:'Tăng ca · '+reason,'fas fa-play',regular?'start':'overtime',task.id,'is-disabled',!regular);
            disabled.disabled=true;cell.insertBefore(disabled,stats);
        }
        return cell;
    }
    function taskStatusCell(rowEl,task){
        var cell=taskCell(rowEl,'kbg20-v41-status-cell','Trạng thái');
        if (realtimeSyncingTasks[String(task.id || '').toLowerCase()]) {
            row(cell,node('span','kbg20-v41-state is-waiting','Đang đồng bộ…'));
            row(cell,node('small','kbg20-v41-substate','Đã nhận sự kiện từ máy chủ; chờ giờ và trạng thái chính thức'));
            return;
        }
        var stateCls=isPending(task)?' is-waiting':task.status===1?' is-active':isFinished(task)?' is-done':' is-idle';
        row(cell,node('span','kbg20-v41-state'+stateCls,taskStatus(task)));
        var t=timerOf(task.id,1),ot=timerOf(task.id,2);
        var sub='';
        if(isPending(task))sub=t||ot?'Cảnh báo: phiên còn mở':'Đồng hồ đã tạm dừng';
        else if(t||ot)sub='Có phiên đang mở';
        else if(task.status===0)sub='Task chưa thực hiện';
        else if(task.endDay && state.workCalendar && task.endDay<state.workCalendar.day)
            sub='Đã quá hạn dự kiến';
        else sub='Chưa mở phiên giờ';
        row(cell,node('small','kbg20-v41-substate'+((t||ot)&&!isPending(task)?' is-running':''),sub));
    }
    function renderTaskRow(task,isUpcoming){
        var t=timerOf(task.id,1),ot=timerOf(task.id,2);
        var item=node('article','kbg20-v41-task-row kbg20-v41-taskgrid'+
            ((t||ot)?' is-running':'')+(isPending(task)?' is-pending':''));
        item.setAttribute('aria-label',(task.code||'')+' '+(task.name||''));
        var identity=taskCell(item,'kbg20-v41-identity','Công việc');
        var name=row(identity,node('div','kbg20-v41-taskname'));
        row(name,node('strong','kbg20-v41-taskcode',task.code||'CV'));
        row(name,node('strong','kbg20-v41-tasktext',task.name||'Chưa đặt tên công việc'));
        var meta='';
        if(Number(task.regular)>0||Number(task.overtime)>0)meta='Giờ đã làm được bảo lưu';
        else if(isUpcoming)meta='Theo kế hoạch · chưa đến ngày bắt đầu';
        else if(task.assigned)meta='Công việc được phân công cho bạn';
        else meta='Đồng hồ đang mở từ dữ liệu trước';
        row(identity,node('small','kbg20-v41-task-note',meta));
        taskCell(item,'kbg20-v41-date','Ngày bắt đầu',task.start||'—');
        var duration=task.durationDays==null?'—':String(task.durationDays)+' ngày';
        taskCell(item,'kbg20-v41-duration','Thời hạn',duration);
        taskCell(item,'kbg20-v41-date','Kết thúc dự kiến',task.end||'—');
        taskStatusCell(item,task);
        taskClockCell(item,task,1,isUpcoming);
        taskClockCell(item,task,2,isUpcoming);
        var actions=taskCell(item,'kbg20-v41-actions','Hành động');
        if(realtimeSyncingTasks[String(task.id || '').toLowerCase()]) {
            row(actions,node('span','kbg20-v41-wait-label','Đang đồng bộ…'));
        }else if(isPending(task)){
            if(state.isPm && state.workflowEnabled){
                var rev=button('Duyệt / Từ chối','review',task.id);
                rev.className+=' kbg20-v41-approval';row(actions,rev);
            } else row(actions,node('span','kbg20-v41-wait-label','Chờ PM duyệt'));
        }else if(task.status===1 && state.workflowEnabled && (task.assigned||state.isPm)){
            var done=button(state.isPm?'Xác nhận hoàn thành':'Gửi duyệt hoàn thành','complete',task.id);
            done.title=state.isPm?'PM xác nhận hoàn thành công việc':'Gửi PM duyệt hoàn thành · chốt giờ cả nhóm';
            done.className+=' kbg20-v41-approval';row(actions,done);
        }else if(isUpcoming)row(actions,node('span','kbg20-v41-wait-label','Sắp tới'));
        else row(actions,node('span','kbg20-v41-wait-label','—'));
        return item;
    }
    function renderTasks() {
        tasksEl.textContent='';
        var tasks=getAllTasks();
        var tabs=document.querySelectorAll('#kbg20TaskFilters [data-task-filter]');
        for(var i=0;i<tabs.length;i++) {
            var mode=tabs[i].getAttribute('data-task-filter');
            var count=tasks.filter(function(t){return taskBelongs(t,mode);}).length;
            var counter=tabs[i].querySelector('[data-filter-count]');
            if(counter) counter.textContent=String(count);
            tabs[i].classList.toggle('is-selected',mode===taskFilter);
            tabs[i].setAttribute('aria-pressed',mode===taskFilter?'true':'false');
        }
        var names={today:'Công việc hôm nay',overdue:'Công việc quá hạn',
            upcoming:'Công việc sắp tới',open:'Công việc có đồng hồ đang bật',pending:'Công việc chờ PM duyệt'};
        document.getElementById('kbg20ListTitle').textContent=names[taskFilter];
        var matches=tasks.filter(function(task){return taskBelongs(task,taskFilter) && (!filter ||
            ((task.code||'')+' '+(task.name||'')).toLocaleLowerCase('vi').indexOf(filter)>=0);});
        document.getElementById('kbg20TaskCount').textContent='('+matches.length+')';
        var more=document.getElementById('kbg20LoadMoreWrap');
        // Riêng nhóm Đang bật phải luôn thấy TOÀN BỘ phiên để có thể Dừng.
        var allVisible=taskFilter==='open';
        var shown=allVisible?matches:matches.slice(0,visibleCount);
        more.hidden=allVisible || shown.length>=matches.length;
        document.getElementById('kbg20LoadMoreHint').textContent=more.hidden?'':
            'Đang xem '+shown.length+'/'+matches.length+' công việc';
        if(!matches.length) {
            var empty=taskFilter==='today' && state.workCalendar && !state.workCalendar.isWorkingDay
                ? 'Hôm nay là ngày nghỉ theo lịch. Bạn vẫn có thể xem công việc quá hạn hoặc đồng hồ đang bật.'
                : taskFilter==='open' ? 'Không có đồng hồ nào đang bật trong dự án này.'
                : taskFilter==='pending' ? 'Không có công việc nào đang chờ PM duyệt.'
                : taskFilter==='upcoming' ? 'Không có công việc nào có ngày bắt đầu sau hôm nay.'
                : 'Không có công việc phù hợp. Thử nhóm khác hoặc xóa từ khóa tìm kiếm.';
            row(tasksEl,node('div','kbg20-empty',empty));
            return;
        }
        shown.forEach(function(task){
            if(taskFilter==='upcoming') { row(tasksEl,renderTaskRow(task,true));return; }
            row(tasksEl,renderTaskRow(task,false));
        });
    }
    function setMode(mode) {
        viewMode=(mode==='history'||mode==='manual')?mode:'work';
        document.getElementById('kbg20WorkView').hidden=viewMode!=='work';
        document.getElementById('kbg20HistoryView').hidden=viewMode!=='history';
        document.getElementById('kbg20ManualView').hidden=viewMode!=='manual';
        root.classList.toggle('kbg20-history-mode',viewMode==='history');
        ['work','history','manual'].forEach(function(which){
            var b=document.getElementById('kbg20Tab'+which.charAt(0).toUpperCase()+which.slice(1));
            var selected=which===viewMode;
            b.classList.toggle('is-selected',selected);b.setAttribute('aria-selected',String(selected));
            b.tabIndex=selected?0:-1;
        });
        if(viewMode==='history'){
            if(!historyDateEl.value&&state&&state.workCalendar)historyDateEl.value=state.workCalendar.day;
            refreshHistory();
        }
        if(viewMode==='manual'&&window.Kbg20DoiSoatUI&&window.Kbg20DoiSoatUI.renderTab)
            window.Kbg20DoiSoatUI.renderTab();
    }
    // Cho phep tien den ngay hien tai, nhung khong vuot ngay theo lich may chu (VN).
    function updateHistoryNavigation(){
        var maxDay=state&&state.workCalendar&&state.workCalendar.day;
        if(historyNextEl){
            historyNextEl.disabled=!maxDay || !historyDateEl.value || historyDateEl.value>=maxDay;
            historyNextEl.setAttribute('aria-disabled',String(historyNextEl.disabled));
        }
    }
    function renderHistory() {
        historyEl.textContent='';historyTotalsEl.textContent='';
        if(!historyData)return;
        var d=historyData;
        var summary=row(historyTotalsEl,node('div','kbg20-v41-history-summary'));
        [
          {label:'Trong ca · Ngày đang xem',value:d.regular},
          {label:'Tăng ca · Ngày đang xem',value:d.overtime},
          {label:'Tổng giờ ghi nhận trong ngày',value:d.total}
        ].forEach(function(c){var x=row(summary,node('div','kbg20-v41-history-stat'));
          row(x,node('small','',c.label));row(x,node('strong','',clock(c.value)));});
        if(!d.tasks||!d.tasks.length){row(historyEl,node('p','kbg20-empty','Không có giờ đã ghi trong ngày được chọn.'));return;}
        var hdr=row(historyEl,node('div','kbg20-v41-hist-head kbg20-v41-hist-grid'));
        ['Công việc','Giờ trong ca','Tăng ca','Tổng giờ'].forEach(function(t){row(hdr,node('span','',t));});
        d.tasks.forEach(function(h){
            var line=row(historyEl,node('article','kbg20-v41-hist-row kbg20-v41-hist-grid'));
            var name=taskCell(line,'kbg20-v41-hist-name','Công việc');
            row(name,node('strong','',h.code||'CV'));
            row(name,node('span','',h.name||'Chưa có tên công việc'));
            if(h.deleted)row(name,node('em','kbg20-v41-deleted','Đã xóa'));
            ['regular','overtime','total'].forEach(function(k){
                var cell=taskCell(line,'kbg20-v41-hist-value',k==='regular'?'Trong ca':k==='overtime'?'Tăng ca':'Tổng');
                row(cell,node('strong',k==='total'?'is-total':'',clock(h[k])));
            });
        });
    }
    function refreshHistory() {
        updateHistoryNavigation();
        if (viewMode !== 'history' || !fixedProjectId || !state) return;
        var day = historyDateEl.value;
        if (!/^\d{4}-\d{2}-\d{2}$/.test(day)) {
            historyStatusEl.textContent = 'Hãy chọn ngày cần xem.';
            return;
        }
        var maxDay = state.workCalendar && state.workCalendar.day;
        if (maxDay && day > maxDay) {
            historyStatusEl.textContent='Không thể xem một ngày trong tương lai.';
            historyDateEl.value = maxDay;
            updateHistoryNavigation();
            return;
        }
        var request = ++historyRequest;
        historyLoading = true;
        historyStatusEl.textContent = 'Đang tải lịch sử ngày đã chọn...';
        historyTotalsEl.textContent = '';
        historyEl.textContent = '';
        var query = '?view=history&projectId=' + encodeURIComponent(fixedProjectId) + '&day=' + encodeURIComponent(day);
        window.fetch(url + query,{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}})
            .then(getJson).then(function(data){
                if(request!==historyRequest) return;
                if(String(data.projectId||'').toLowerCase()!==fixedProjectId.toLowerCase() || data.day!==day)
                    throw new Error('Lịch sử trả về không khớp dự án/ngày đã chọn.');
                historyData=data;
                historyStatusEl.textContent='Ngày ' + day.split('-').reverse().join('/') + ' · trong dự án hiện tại';
                renderHistory();
            }).catch(function(err){
                if(request!==historyRequest) return;
                historyData=null;
                historyStatusEl.textContent=err.message || 'Không tải được lịch sử.';
            }).then(function(){if(request===historyRequest)historyLoading=false;});
    }
    function refreshUi() {
        var reconcileBtn=document.getElementById('kbg20ReconcileListOpen');
        if(reconcileBtn)reconcileBtn.style.display='none';
        var manualTab=document.getElementById('kbg20TabManual');
        if(manualTab)manualTab.hidden=!(state && state.workflowEnabled);
        if(viewMode==='manual'&&!(state && state.workflowEnabled))setMode('work');
        if (!state) return;
        renderProjectContext();renderSchedule();renderSummary();renderTasks();
    }
    // ================================================================
    // DOT 2 - Real-time cho cac phien bi dong do gui PM duyet / PM hoan thanh.
    // Backend la nguon su that; khong luu gio vao DB tu SignalR.
    // ================================================================
    function freezeAfterServerCommit(taskId) {
        taskId = String(taskId || '').toLowerCase();
        if (!taskId) return;
        // Một event mới tăng epoch để GET cũ không ghi đè dữ liệu sau tín hiệu commit.
        realtimeEpoch++;
        if (realtimeSyncingTasks[taskId]) return;
        realtimeSyncingTasks[taskId] = true;
        if (!state) return;

        (state.timers || []).forEach(function(timer) {
            if (String(timer.taskId || '').toLowerCase() !== taskId) return;
            var elapsed = Math.max(0, liveSeconds(timer) - (Number(timer.seconds) || 0));
            var task = (state.tasks || []).find(function(x) { return String(x.id || '').toLowerCase() === taskId; });
            if (task) {
                if (Number(timer.type) === 1) task.regular = (Number(task.regular) || 0) + elapsed;
                if (Number(timer.type) === 2) task.overtime = (Number(task.overtime) || 0) + elapsed;
            }
            var today = todayTaskTimes[taskId];
            if (today) {
                if (Number(timer.type) === 1) today.regular = (Number(today.regular) || 0) + elapsed;
                if (Number(timer.type) === 2) today.overtime = (Number(today.overtime) || 0) + elapsed;
            }
            timer.seconds = liveSeconds(timer);
            timer.counting = false;
        });
        // Giữ filter, search và tab hiện tại. Không hiện đồng hồ tiếp tục nhảy.
        refreshUi();
    }
    // Đồng bộ Đợt 2: một snapshot cho tất cả cột trên UI.
    // Hai GET chạy nối tiếp; chỉ cập nhật state sau khi CẢ HAI đã thành công.
    // Không báo đã đồng bộ rồi mở nút khi lịch sử hôm nay còn ở phiên bản cũ.
    function refreshRealtimeSnapshot() {
        if (loading || acting) return Promise.resolve(false);
        if (!fixedProjectId) return Promise.resolve(false);
        loading = true;
        var epoch = realtimeEpoch;
        var affected = Object.keys(realtimeSyncingTasks);
        var query = '?projectId=' + encodeURIComponent(fixedProjectId);
        return window.fetch(url + query, {
            credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}
        }).then(getJson).then(function(data) {
            if (String(data.projectId || '').toLowerCase() !== fixedProjectId.toLowerCase())
                throw new Error('Dữ liệu task không thuộc dự án hiện tại.');
            if (!data.workCalendar || !data.workCalendar.day)
                throw new Error('API không trả về ngày làm việc.');
            // Nếu task liên quan vẫn còn phiên mở, dữ liệu nhận được chưa phản ánh
            // sự kiện chốt giờ. Tiếp tục chờ/retry, không mở thao tác từ state cũ.
            var openTimers = data.timers || [];
            var hasStaleOpen = affected.some(function(taskId) {
                return openTimers.some(function(timer) {
                    return String(timer.taskId || '').toLowerCase() === taskId;
                });
            });
            if (hasStaleOpen) throw new Error('Máy chủ vẫn trả phiên đang mở của task vừa chốt.');
            var day = data.workCalendar.day;
            var historyUrl = url + '?view=history&projectId=' + encodeURIComponent(fixedProjectId) +
                '&day=' + encodeURIComponent(day);
            return window.fetch(historyUrl, {
                credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}
            }).then(getJson).then(function(history) {
                if (String(history.projectId || '').toLowerCase() !== fixedProjectId.toLowerCase() ||
                    history.day !== day || history.ok !== true)
                    throw new Error('Lịch sử hôm nay chưa đúng dự án hoặc ngày đã chọn.');
                return { data: data, history: history, day: day };
            });
        }).then(function(snapshot) {
            if (epoch !== realtimeEpoch) return false; // event mới đến khi đang tải
            var next = Object.create(null);
            (snapshot.history.tasks || []).forEach(function(task) {
                next[task.id] = {
                    regular: Number(task.regular) || 0,
                    overtime: Number(task.overtime) || 0
                };
            });
            // Vô hiệu hóa GET lịch sử hôm nay cũ, không để ghi đè snapshot mới.
            todayRequest++;
            offset = Number(snapshot.data.serverNow) - Date.now();
            state = snapshot.data;
            todayTaskTimes = next;
            todayDataDay = snapshot.day;
            lastFetch = Date.now();
            realtimeSyncingTasks = Object.create(null);
            if (!historyDateEl.value) historyDateEl.value = snapshot.day;
            historyDateEl.max = snapshot.day;
            updateHistoryNavigation();
            refreshUi(); // trạng thái, 2 tổng giờ, giờ hôm nay, hành động và bộ lọc cùng lúc
            if (viewMode === 'history' && !historyLoading) refreshHistory();
            if (viewMode === 'manual' && window.Kbg20DoiSoatUI &&
                typeof window.Kbg20DoiSoatUI.renderTab === 'function')
                window.Kbg20DoiSoatUI.renderTab();
            notify('', '');
            return true;
        }).catch(function(err) {
            notify('Đang đồng bộ trạng thái mới. Sẽ tự thử lại nếu máy chủ chưa phản hồi: ' +
                (err.message || 'Lỗi kết nối.'), '');
            return false;
        }).then(function(ok) {
            loading = false;
            return ok;
        });
    }
    function queueRealtimeRefresh() {
        realtimeRefreshPending = true;
        if (realtimeRetry != null) return;
        function run() {
            realtimeRetry = null;
            if (!realtimeRefreshPending) return;
            if (loading || acting) {
                realtimeRetry = window.setTimeout(run, 180);
                return;
            }
            realtimeRefreshPending = false;
            refreshRealtimeSnapshot().then(function(ok) {
                if (!ok) {
                    realtimeRefreshPending = true;
                    realtimeRetryCount++;
                    realtimeRetry = window.setTimeout(run, Math.min(30000, 1500 * realtimeRetryCount));
                    return;
                }
                realtimeRetryCount = 0;
                // Nếu có event mới khi fetch, lấy thêm snapshot mới thay vì sử dụng kết quả cũ.
                if (realtimeRefreshPending) queueRealtimeRefresh();
            });
        }
        realtimeRetry = window.setTimeout(run, 90);
    }
    function onRealtimeChange(evt) {
        if (!evt || !evt.kind) return;
        if (evt.kind === 'connection-restored') {
            // Reconnect co the bo lo su kien; tai lai toan bo trang thai tu SQL.
            queueRealtimeRefresh();
            return;
        }
        if (evt.kind !== 'task-timers-stopped') return;
        if (String(evt.projectId || '').toLowerCase() !== fixedProjectId.toLowerCase()) return;
        if (evt.eventId && seenRealtimeIds[evt.eventId]) return;
        if (evt.eventId) {
            seenRealtimeIds[evt.eventId] = true;
            if (++seenRealtimeCount >= 200) {seenRealtimeCount=0;seenRealtimeIds=Object.create(null);}
        }
        freezeAfterServerCommit(evt.taskId);
        queueRealtimeRefresh();
    }
    function initializeRealtime() {
        // Neu Hub tat/khong duoc nap, KBG20 van hoat dong theo polling 30 giay.
        if (!window.QldaRealtime || typeof window.QldaRealtime.on !== 'function') return;
        window.QldaRealtime.on(onRealtimeChange);
        var hubUrl = root.getAttribute('data-realtime-url') || '/qlda-realtime';
        var connecting = window.QldaRealtime.start(hubUrl);
        if (connecting && typeof connecting.fail === 'function')
            connecting.fail(function(err) {if(window.console)console.warn('KBG20 realtime offline; polling van hoat dong.',err);});
    }
    function refresh() {
        if (loading || acting) return Promise.resolve(false);
        if (!fixedProjectId) { notify('Không xác định được dự án đang mở.', 'error'); return Promise.resolve(); }
        loading = true;
        var epoch = realtimeEpoch;
        var query = '?projectId=' + encodeURIComponent(fixedProjectId);
        return window.fetch(url + query, { credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'} })
            .then(getJson).then(function (data) {
                // Đã có event sau khi bắt đầu GET: không ghi đè bằng response cũ.
                if (epoch !== realtimeEpoch) return false;
                if (String(data.projectId || '').toLowerCase() !== fixedProjectId.toLowerCase())
                    throw new Error('Dữ liệu phản hồi không thuộc dự án hiện tại.');
                offset = Number(data.serverNow) - Date.now();
                state = data;
                lastFetch = Date.now();
                refreshUi();
                refreshToday();
                if (!historyDateEl.value && state.workCalendar) historyDateEl.value = state.workCalendar.day;
                if (state.workCalendar) historyDateEl.max = state.workCalendar.day;
                updateHistoryNavigation();
                if (viewMode === 'history' && !historyLoading) refreshHistory();
                notify('', '');
                return true;
            }).catch(function (err) {notify(err.message || 'Lỗi kết nối.', 'error');return false;})
            .then(function(ok){
                // BẮT BUỘC trả lại trạng thái cho queueRealtimeRefresh.
                // Nếu bỏ `return ok`, SignalR luôn hiểu là GET thất bại và lặp retry vô hạn.
                loading=false;
                return ok;
            });
    }
    // Called only after partner Ctrl Start/Complete/Review has saved successfully,
    // closed its ExtraModal and raised ActionStarted/ActionCompleted.
    // Update V2 in place; preserve selected tab/filter/search/scroll.
    window.Kbg20RefreshAfterWorkflow = function () {
        if (window.KbgFloatingTimer && typeof window.KbgFloatingTimer.notifyChanged==='function')
            window.KbgFloatingTimer.notifyChanged();
        // Không mất refresh sau 4 giây nếu đang có AJAX khác.
        // Đồng bộ lại cả khi event từ tài khoản vừa gửi duyệt đã tới trước popup đóng.
        queueRealtimeRefresh();
    };
    function guid() {
        if (window.crypto && window.crypto.randomUUID) return window.crypto.randomUUID();
        if (window.crypto && window.crypto.getRandomValues) {
            var b = new Uint8Array(16);window.crypto.getRandomValues(b);
            b[6] = (b[6] & 15) | 64; b[8] = (b[8] & 63) | 128;
            return Array.prototype.map.call(b,function(v){return ('0'+v.toString(16)).slice(-2);})
                .join('').replace(/^(.{8})(.{4})(.{4})(.{4})(.{12})$/, '$1-$2-$3-$4-$5');
        }
        throw new Error('Trình duyệt không hỗ trợ sinh mã thao tác an toàn.');
    }
    function sendAction(action,id) {
        // Trước đây nhấn trong lúc GET/POST đang chạy sẽ return im lặng,
        // tạo cảm giác nút Start/Stop bị hỏng.
        if (acting || loading) {
            notify('Trang đang đồng bộ dữ liệu. Vui lòng chờ thao tác hiện tại hoàn tất rồi thử lại.', '');
            return;
        }
        if (!state) {
            notify('Dữ liệu công việc chưa tải xong. Vui lòng đợi một chút.', '');
            return;
        }
        acting=true; refreshUi();
        var requestId;
        try {requestId=guid();} catch(err) {acting=false; notify(err.message,'error');refreshUi();return;}
        var flags={confirmOverdue:false,confirmDependency:false};
        function perform() {
            var params=new URLSearchParams();
            params.set('action',action);
            params.set('projectId',fixedProjectId);
            params.set('requestId',requestId);
            if(action==='stop')params.set('sessionId',id); else params.set('taskId',id);
            params.set('confirmOverdue',String(flags.confirmOverdue));
            params.set('confirmDependency',String(flags.confirmDependency));
            return window.fetch(url, {method:'POST',credentials:'same-origin',cache:'no-store',
                headers:{'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8',
                         'X-KBG20-Token':token,'Accept':'application/json'},body:params.toString()})
                .then(getJson).then(function(res) {
                    if (res.confirmOverdue && !flags.confirmOverdue) {
                        if (window.confirm(res.message || 'Công việc đã quá hạn. Tiếp tục?'))
                        {flags.confirmOverdue=true;return perform();}
                        notify('Đã hủy thao tác.','');return;
                    }
                    if (res.confirmDependency && !flags.confirmDependency) {
                        if(window.confirm(res.message || 'Công việc phụ thuộc chưa hoàn thành. Tiếp tục?'))
                        {flags.confirmDependency=true;return perform();}
                        notify('Đã hủy thao tác.','');return;
                    }
                    notify((res.message || 'Đã xử lý.') + (res.warnings && res.warnings.length ?
                        ' ' + res.warnings.join(' ') : ''),res.ok?'ok':'error');
                    if (res.ok) {
                        if(window.KbgFloatingTimer) window.KbgFloatingTimer.notifyChanged();
                        return true;
                    }
                });
        }
        perform().then(function(ok){
            acting=false;
            if (ok === true) refresh(); else refreshUi();
        }).catch(function(err){acting=false;notify(err.message || 'Thao tác thất bại.','error');refreshUi();});
    }
    // Wire the existing tab controller to the two visible tab buttons.
    // Switching views must not post back or interfere with running timers.
    var workTab = document.getElementById('kbg20TabWork');
    var historyTab = document.getElementById('kbg20TabHistory');
    var manualTab = document.getElementById('kbg20TabManual');
    workTab.addEventListener('click', function () { setMode('work'); });
    historyTab.addEventListener('click', function () { setMode('history'); });
    manualTab.addEventListener('click', function () { if(!manualTab.hidden)setMode('manual'); });
    // Accessible tab-keyboard navigation (left/right, Home/End).
    function onViewTabKeydown(e) {
        var tabs=[workTab,historyTab,manualTab].filter(function(t){return !t.hidden;});
        var i=tabs.indexOf(e.currentTarget),target;
        if(e.key==='ArrowRight')target=tabs[(i+1)%tabs.length];
        else if(e.key==='ArrowLeft')target=tabs[(i+tabs.length-1)%tabs.length];
        else if(e.key==='Home')target=tabs[0];
        else if(e.key==='End')target=tabs[tabs.length-1];
        else return;
        e.preventDefault();target.focus();setMode(target===workTab?'work':target===historyTab?'history':'manual');
    }
    workTab.addEventListener('keydown',onViewTabKeydown);
    historyTab.addEventListener('keydown',onViewTabKeydown);
    manualTab.addEventListener('keydown',onViewTabKeydown);
    window.Kbg20PageUI={openManual:function(){setMode('manual');},refreshToday:refreshToday};
    setMode('work');

    document.getElementById('kbg20TaskFilters').addEventListener('click',function(e){
        var chip=e.target.closest('[data-task-filter]');
        if(chip && this.contains(chip))setTaskFilter(chip.getAttribute('data-task-filter'));
    });
    searchEl.addEventListener('input',function(){filter=(searchEl.value||'').trim().toLocaleLowerCase('vi');visibleCount=pageSize;if(state)renderTasks();});
    document.getElementById('kbg20Reload').addEventListener('click',function(){refresh();});
    document.getElementById('kbg20LoadMore').addEventListener('click',function(){visibleCount+=pageSize;if(state)renderTasks();});
    historyDateEl.addEventListener('change',refreshHistory);
    document.getElementById('kbg20HistoryPrev').addEventListener('click',function(){
        if(!historyDateEl.value)return;
        var d=new Date(historyDateEl.value+'T12:00:00Z');
        if(!isFinite(d.getTime()))return;
        d.setUTCDate(d.getUTCDate()-1);
        historyDateEl.value=d.toISOString().slice(0,10);
        refreshHistory();
    });
    if(historyNextEl)historyNextEl.addEventListener('click',function(){
        var maxDay=state&&state.workCalendar&&state.workCalendar.day;
        if(!maxDay||!historyDateEl.value||historyDateEl.value>=maxDay)return;
        var d=new Date(historyDateEl.value+'T12:00:00Z');
        if(!isFinite(d.getTime()))return;
        d.setUTCDate(d.getUTCDate()+1);
        var next=d.toISOString().slice(0,10);
        historyDateEl.value=next>maxDay?maxDay:next;
        refreshHistory();
    });
    document.getElementById('kbg20HistoryToday').addEventListener('click',function(){
        if(state && state.workCalendar){historyDateEl.value=state.workCalendar.day;refreshHistory();}
    });
    root.addEventListener('click',function(e){
        var b=e.target.closest('[data-action]');if (!b || !root.contains(b))return;
        if(b.disabled)return;
        var action=b.getAttribute('data-action'),id=b.getAttribute('data-id');
        var taskIdForAction=id;
        if(action==='stop') {
            var stopTimer=state && (state.timers||[]).find(function(t){return t.id===id;});
            if(stopTimer)taskIdForAction=stopTimer.taskId;
        }
        if (realtimeSyncingTasks[String(taskIdForAction || '').toLowerCase()]) {
            notify('Công việc vừa được gửi duyệt hoặc hoàn thành. Đang tải trạng thái mới.','');
            return;
        }
        var task=state && (state.tasks||[]).find(function(x){return x.id===id;});
        var isWorkflow=state && state.workflowEnabled &&
           (action==='complete' || action==='review' ||
             ((action==='start'||action==='overtime') && task && task.status===0));
        if(isWorkflow) {
            if (acting) {
                notify('Đang xử lý một thao tác tính giờ khác. Vui lòng đợi hoàn tất.', '');
                return;
            }
            if(!task || !window.kbg20WorkflowPostback){notify('Không mở được xử lý công việc. Hãy tải lại trang.','error');return;}
            if((action==='start'||action==='overtime') && task.startDay &&
                state.workCalendar && task.startDay>state.workCalendar.day) {
                notify('Chưa đến ngày bắt đầu công việc.','error');return;
            }
            var late=(action==='start'||action==='overtime') && task.endDay &&
                state.workCalendar && task.endDay<state.workCalendar.day;
            if(late && !window.confirm('Công việc đã quá hạn. Bạn muốn xác nhận tiếp tục tính giờ sau khi chuyển trạng thái?'))return;
            window.kbg20WorkflowPostback((action==='start'||action==='overtime')?'start':action,
                id,late,action==='overtime');
            return;
        }
        sendAction(action,id);
    });
    window.setInterval(function(){
        if (!state) return;
        var now=serverNow();
        var globalClock=document.getElementById('ksClock');
        if(globalClock) {
            try {globalClock.textContent=new Date(now).toLocaleTimeString('vi-VN',{
                timeZone:'Asia/Ho_Chi_Minh',hour12:false});} catch (_) {}
        }
        var elems=tasksEl.querySelectorAll('[data-timer-id]');
        for(var i=0;i<elems.length;i++) {
            var id=elems[i].getAttribute('data-timer-id');
            var t=state.timers.find(function(x){return x.id===id;});
            if(t) elems[i].textContent=clock(liveSeconds(t));
        }
        var totals=tasksEl.querySelectorAll('[data-total-task]');
        for(var j=0;j<totals.length;j++) {
            var taskId=totals[j].getAttribute('data-total-task');
            var type=Number(totals[j].getAttribute('data-total-type'));
            var task=state.tasks.find(function(x){return x.id===taskId;});
            // Phien da mo van phai chay khi task tam thoi vang khoi list duoc phan cong.
            var base=task ? (Number(type===1?task.regular:task.overtime)||0) : 0;
            var open=state.timers.find(function(t){return t.taskId===taskId && t.type===type;});
            totals[j].textContent=clock(base+(open?Math.max(0,liveSeconds(open)-Number(open.seconds||0)):0));
        }
        var dayLabels=tasksEl.querySelectorAll('[data-today-task]');
        for(var a=0;a<dayLabels.length;a++) {
            if(todayDataDay)updateTodayLabel(dayLabels[a],dayLabels[a].getAttribute('data-today-task'),Number(dayLabels[a].getAttribute('data-today-type')));
        }
        var statuses=tasksEl.querySelectorAll('[data-timer-status]');
        for(var k=0;k<statuses.length;k++) {
            var timer=state.timers.find(function(t){return t.id===statuses[k].getAttribute('data-timer-status');});
            if(!timer)continue;
            var counting=timer.counting && (timer.boundary==null || serverNow()<Number(timer.boundary));
            var thisTask=(state.tasks||[]).find(function(t){return t.id===timer.taskId;});
            statuses[k].textContent=thisTask && isPending(thisTask)?'Phiên còn mở — kiểm tra chốt giờ':(counting?(Number(timer.type)===2?'Đang cộng tăng ca':'Đang cộng giờ trong ca'):idleClockStatus(Number(timer.type)));
            statuses[k].classList.toggle('kbg20-active',counting);
            statuses[k].classList.toggle('kbg20-paused',!counting);
        }
        var dayStatus = document.getElementById('kbg20ScheduleStatus');
        if (dayStatus) dayStatus.textContent=scheduleState();
        var slots=scheduleEl.querySelectorAll('.kbg20-v41-shift');
        if(slots.length && state && state.workCalendar){
           var moments=state.workCalendar.shifts||[], nowMs=serverNow();
           slots.forEach(function(x,i){
             var type=i===0?1:i===2?2:0;
             var active=moments.some(function(sh){return Number(sh.type)===type&&nowMs>=Number(sh.startMs)&&nowMs<Number(sh.endMs);});
             x.classList.toggle('is-current',active);
           });
        }
        if(!loading && !acting && !realtimeRefreshPending &&
           Object.keys(realtimeSyncingTasks).length===0 && Date.now()-lastFetch>30000)refresh();
    },1000);
    refresh();
    initializeRealtime();
})(window,document);
