/* KhaiBaoGio 2.0: only active on #kbg20Page. Does not modify V1 handlers. */
(function (window, document) {
    'use strict';
    var root = document.getElementById('kbg20Page');
    if (!root) return;
    var url = root.getAttribute('data-api-url');
    var token = root.getAttribute('data-request-token');
    var state = null, loading = false, acting = false, offset = 0;
    // Dot 7A: khong bo mat yeu cau lam moi sau workflow neu dang co request GET.
    var refreshPending = false;
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
    // Errors and action results go through the SAME ShowNotify as the rest of BackOffice.
    // Authentication failures on the initial page load use a dedicated error page.
    var accessRedirecting=false,lastNotifyText='',lastNotifyAt=0;
    function notify(text,type) {
        if(!text)return;
        var severity=type==='ok'?'success':type==='error'?'error':(type||'info');
        var stamp=Date.now();
        if(lastNotifyText===text && stamp-lastNotifyAt<4500)return;
        lastNotifyText=text;lastNotifyAt=stamp;
        if(window.CMSMasterJs && typeof window.CMSMasterJs.ShowNotify==='function')
            window.CMSMasterJs.ShowNotify(text,severity);
        else if(window.toastr && typeof window.toastr[severity]==='function')
            window.toastr[severity](text);
        else if(window.console)window.console.warn('KBG20:',text);
    }
    function apiFailure(message,status) {var e=new Error(message||'Lỗi API.');e.status=status||0;return e;}
    function getJson(response) {
        var contentType=response.headers.get('content-type')||'';
        if(contentType.toLowerCase().indexOf('application/json')<0)
            return Promise.reject(apiFailure('Máy chủ trả về trang lỗi thay vì JSON (HTTP '+response.status+').',response.status));
        return response.json().then(function(obj){
            if(!response.ok||obj.ok===false)throw apiFailure(obj.error||obj.message||('HTTP '+response.status),response.status);
            return obj;
        });
    }
    function redirectAccessFailure(error){
        if(!error || (error.status!==401 && error.status!==403) || accessRedirecting)return false;
        accessRedirecting=true;
        var dest=(root.getAttribute('data-kbg20-access-error-url') || '/fKhaiBaoGio/Kbg20AccessDenied.aspx');
        window.location.replace(dest+(dest.indexOf('?')>=0?'&':'?')+'code='+error.status);
        return true;
    }
    function openExtraModal(id,title){
        if(window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'){
            window.CMSMasterJs.OpenDialog('#'+id,title);return;
        }
        var element=document.getElementById(id),jq=window.jQuery||window.$;
        if(element&&window.bootstrap&&window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(element).show();return;}
        if(jq&&jq.fn&&jq.fn.modal){jq('#'+id).modal('show');return;}
        throw new Error('Không mở được ExtraModal. Hãy tải lại trang.');
    }
    function closeExtraModal(id){
        var el=document.getElementById(id),jq=window.jQuery||window.$;
        if(el&&window.bootstrap&&window.bootstrap.Modal){var m=window.bootstrap.Modal.getInstance(el);if(m){m.hide();return;}}
        if(jq&&jq.fn&&jq.fn.modal)jq('#'+id).modal('hide');
    }
    // Mot ExtraModal dung chung canh bao quyet dinh: khong dung window.confirm.
    var warningBusy=false;
    function confirmAction(title, message){
        return new Promise(function(resolve){
            if(warningBusy){resolve(false);return;}
            warningBusy=true;
            var content=document.getElementById('kbg20StartWarningText');
            var yes=document.getElementById('kbg20StartWarningYes');
            var no=document.getElementById('kbg20StartWarningNo');
            if(!content||!yes||!no){warningBusy=false;notify('Thiếu ExtraModal xác nhận. Đã hủy thao tác.','error');resolve(false);return;}
            content.textContent=message||'Bạn có chắc chắn muốn thực hiện?';
            var decided=false;
            function finish(value){if(decided)return;decided=true;warningBusy=false;
                closeExtraModal('mdlKbg20StartWarning');resolve(value);}
            yes.onclick=function(){finish(true);};no.onclick=function(){finish(false);};
            var m=document.getElementById('mdlKbg20StartWarning'),jq=window.jQuery||window.$;
            if(jq&&m&&jq.fn&&jq.fn.modal)
                jq(m).off('hidden.bs.modal.kbg20StartWarning').one('hidden.bs.modal.kbg20StartWarning',function(){finish(false);});
            try{openExtraModal('mdlKbg20StartWarning',title);}
            catch(e){warningBusy=false;notify(e.message,'error');resolve(false);}
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
        var link = row(summary, node('button', 'kbg20-summary-link', count + ' phiên đang bật'));
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
        if (now < Number(segs[0].startMs)) return 'Chưa đến giờ làm';
        for (var i = 0; i < segs.length; i++) {
            if (now >= Number(segs[i].startMs) && now < Number(segs[i].endMs)) return 'Trong giờ làm việc';
            if (i + 1 < segs.length && now >= Number(segs[i].endMs) && now < Number(segs[i+1].startMs))
                return 'Đang nghỉ trưa';
        }
        return 'Đã hết giờ làm hôm nay';
    }
    function idleClockStatus(type) {
        if (type === 2) return 'Chưa tính giờ tăng ca';
        var status = scheduleState();
        if (status === 'Đang nghỉ trưa') return 'Đang nghỉ giữa buổi';
        if (status === 'Chưa đến giờ làm') return 'Chờ đến giờ làm';
        if (status === 'Ngày nghỉ') return 'Ngày nghỉ';
        if (status === 'Đã hết giờ làm hôm nay') return 'Ngoài giờ làm theo lịch';
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
        slot('fas fa-sun','Buổi sáng',times(morning),morning,'');
        var breakText='Không áp dụng',isBreak=false;
        if(morning.length && afternoon.length){
            var a=morning[morning.length-1],b=afternoon[0];
            if(Number(b.startMs)>Number(a.endMs)){
                breakText=a.end+'–'+b.start;
                isBreak=serverNow()>=Number(a.endMs)&&serverNow()<Number(b.startMs);
            }
        }
        slot('fas fa-coffee','Nghỉ giữa buổi',breakText,[],isBreak?' is-rest':'');
        slot('fas fa-cloud-sun','Buổi chiều',times(afternoon),afternoon,'');
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
        if (mode === 'reported') return task.personalStatus===1 || task.personalStatus===3 || task.personalStatus===5;
        if (mode === 'all') return (task.assigned || hasOpen) && !task.missing;
        var accessible = (task.assigned || hasOpen) && !task.missing;
        if (!accessible) return false;
        if (mode === 'pending') return isPending(task);
        if (!today) return false;
        if (mode === 'today') {
            // Gio thuc hom nay / phien dang chay phai xuat hien, du ngoai ngay KE HOACH.
            var entry=todayTaskTimes[task.id];
            // Co phien hoat dong 0 giay van nam trong hom nay; khong thay doi nghia vu Start som.
            var hasWorked=todayDataDay===today && !!entry;
            return hasWorked || hasOpen || (!isFinished(task) && !!task.startDay &&
                !!task.endDay && task.startDay<=today && task.endDay>=today);
        }
        if (isFinished(task)) return false;
        if (mode === 'overdue') return !!task.endDay && task.endDay < today;
        if (mode === 'upcoming') return !!task.startDay && task.startDay > today && !hasOpen;
        return false;
    }
    function setTaskFilter(mode) {
        if (['today','overdue','upcoming','open','pending','all','reported'].indexOf(mode) < 0) return;
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
        return !task.missing && task.assigned && (task.status===0 || task.status===1) && !isPending(task)
            && task.personalStatus!==1 && task.personalStatus!==3 && task.personalStatus!==4;
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
        el.textContent='Hôm nay: '+(todayDataDay?clock(todaySeconds(taskId,type)):'đang tải…');
    }
    function refreshToday(){
        if(!state||!state.workCalendar||!fixedProjectId)return;
        var day=state.workCalendar.day,request=++todayRequest;
        var q='?view=history&projectId='+encodeURIComponent(fixedProjectId)+'&day='+encodeURIComponent(day);
        window.fetch(url+q,{credentials:'same-origin',cache:'no-store'}).then(getJson).then(function(d){
            if(request!==todayRequest || d.day!==day ||
                String(d.projectId||'').toLowerCase()!==fixedProjectId.toLowerCase())return;
            var next=Object.create(null);
            (d.tasks||[]).forEach(function(t){next[t.id]={regular:Number(t.regular)||0,overtime:Number(t.overtime)||0};});
            todayTaskTimes=next;todayDataDay=day;
            // Dem va danh sach Today dung CUNG du lieu ngay thuc te.
            // Khong load lai server va khong can thiep tab hien tai.
            if(state && (taskFilter==='today' || taskFilter==='all'))renderTasks();
            else tasksEl.querySelectorAll('[data-today-task]').forEach(function(el){updateTodayLabel(el,el.getAttribute('data-today-task'),Number(el.getAttribute('data-today-type')));});
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
        // Tinh theo ke hoach THUC cua Task, khong theo tab dang xem (All/Upcoming/Today).
        // Khong cho luon nut Start khi doi bo loc.
        isUpcoming=!!(state && state.workCalendar && task.startDay &&
            task.startDay>state.workCalendar.day);
        var timer=timerOf(task.id,type),regular=type===1;
        var cell=taskCell(parent,'kbg20-v41-clock-cell'+(regular?' is-regular':' is-overtime'),regular?'Giờ làm theo lịch':'Giờ tăng ca');
        var stats=row(cell,node('div','kbg20-v41-clock-main'));
        var digits=clockValue(stats,task,type,regular);
        digits.className='kbg20-v41-clock';
        var sub=row(stats,node('small','kbg20-v41-clock-sub'));
        if(isPending(task))sub.textContent=timer?'Phiên còn mở · cần chốt':'Đã tạm dừng';
        else if(timer){
            sub.textContent=timer.counting?(regular?'Đang tính giờ làm theo lịch':'Đang tính giờ tăng ca'):'Đang mở · '+idleClockStatus(type);
            sub.setAttribute('data-timer-status',timer.id);
        } else sub.textContent=(regular?task.startedRegular:task.startedOvertime)||Number(regular?task.regular:task.overtime)>0
            ?'Tổng thời gian đã ghi':'Chưa ghi '+(regular?'giờ làm theo lịch':'giờ tăng ca');
        if (!timer && isUpcoming && regular) {
            var canEarly=!!(state.earlyStart && state.earlyStart.allowed);
            sub.textContent=canEarly?'Có thể bắt đầu sớm · không đổi ngày kế hoạch':
                'Chưa thể Start sớm · còn phần việc đến hạn';
            if(!canEarly && state.earlyStart && state.earlyStart.blockingTasks)
                sub.title=state.earlyStart.blockingTasks.join('; ');
        }
        var todaySub=row(stats,node('small','kbg20-v43-today'));todaySub.setAttribute('data-today-task',task.id);
        todaySub.setAttribute('data-today-type',String(type));
        updateTodayLabel(todaySub,task.id,type);
        if(timer){
            var title=regular?'Dừng giờ làm theo lịch':'Dừng giờ tăng ca';
            cell.insertBefore(iconAction(title,'fas fa-stop','stop',timer.id,'is-stop',!regular),stats);
        } else if(taskMayStart(task) && (regular || (!isUpcoming && overtimeAllowedBySchedule())) &&
                (!isUpcoming || (regular && state.earlyStart && state.earlyStart.allowed))){
            var resume=regular && (task.startedRegular || Number(task.regular)>0) ||
                !regular && (task.startedOvertime || Number(task.overtime)>0);
            var action=regular?'start':'overtime';
            var label=isUpcoming?'Bắt đầu sớm · Giờ làm theo lịch':
                (resume?'Tiếp tục':'Bắt đầu')+(regular?' giờ làm theo lịch':' giờ tăng ca');
            if(task.status===0 && state.workflowEnabled)label+=' · Chuyển công việc sang Đang thực hiện trước khi bấm giờ';
            cell.insertBefore(iconAction(label,resume?'fas fa-redo-alt':'fas fa-play',action,task.id,
                resume?'is-resume':'is-start',!regular),stats);
        }else{
            var reason=task.personalStatus===1||task.personalStatus===3?'Phần việc này đã được báo hoàn thành':isPending(task)?'Công việc đang chờ duyệt':isFinished(task)?'Công việc đã hoàn thành':
                isUpcoming?(regular && state.earlyStart && !state.earlyStart.allowed?
                    'Chưa thể Start sớm: còn '+state.earlyStart.remainingCount+' phần việc đến hạn chưa báo hoàn thành':
                    'Chưa tới ngày bắt đầu; tăng ca không được Start sớm'):(!regular && !overtimeAllowedBySchedule()
                ?'Chỉ có thể bắt đầu tính giờ tăng ca sau khi kết thúc buổi làm việc cuối cùng theo lịch':
                (!task.assigned?'Bạn chưa được phân công':'Chưa đủ điều kiện bắt đầu'));
            var disabled=iconAction(regular?reason:'Tăng ca · '+reason,'fas fa-play',regular?'start':'overtime',task.id,'is-disabled',!regular);
            disabled.disabled=true;cell.insertBefore(disabled,stats);
        }
        return cell;
    }
    function taskStatusCell(rowEl,task){
        var cell=taskCell(rowEl,'kbg20-v41-status-cell','Trạng thái công việc');
        var stateCls=isPending(task)?' is-waiting':task.status===1?' is-active':isFinished(task)?' is-done':' is-idle';
        row(cell,node('span','kbg20-v41-state'+stateCls,taskStatus(task)));
    }
    function personalReportCell(rowEl,task){
        var cell=taskCell(rowEl,'kbg20-v41-personal-cell','Báo cáo phần việc');
        var status=Number(task.personalStatus)||0;
        var finished=isPending(task)||isFinished(task);
        if(status===4){row(cell,node('span','kbg20-dot2-personal is-reported','Đã chốt giờ công'));}
        else if(status===5){row(cell,node('span','kbg20-dot2-personal is-reported','Đã báo hoàn thành'));}
        else if(status===1||status===3)row(cell,node('span','kbg20-dot2-personal is-reported','Đã báo hoàn thành'));
        else if(finished){row(cell,node('span','kbg20-dot2-personal is-not-reported',status===2?'Đã rút báo hoàn thành':'Chưa báo hoàn thành'));}
        else if(status===2)row(cell,node('span','kbg20-dot2-personal is-withdrawn','Đã rút báo hoàn thành'));
        else row(cell,node('span','kbg20-dot2-personal is-not-reported','Chưa báo hoàn thành'));
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
        personalReportCell(item,task);
        taskClockCell(item,task,1,isUpcoming);
        taskClockCell(item,task,2,isUpcoming);
        var actions=taskCell(item,'kbg20-v41-actions','Hành động');
        // Event5: xac nhan gio sau khi Task cho duyet, KHONG dong nghia da bao xong phan viec.
        // Neu PM tu choi va Task quay ve 1, nhan vien van duoc bao hoan thanh ca nhan.
        if(task.personalStatus===1 || task.personalStatus===3 || task.personalStatus===5){
            if(task.status===1 && !isPending(task) && task.personalStatus!==4){
                var withdrawn=button('Rút báo hoàn thành','withdrawPersonal',task.id);
                withdrawn.className+=' kbg20-dot2-row-btn is-danger';row(actions,withdrawn);
            }else{
                var review=button('Xem / sửa giờ ở Tab 3','openPersonalHours',task.id);
                review.className+=' kbg20-dot2-row-btn';row(actions,review);
            }
        }else if((isPending(task)||isFinished(task)) && state.workflowEnabled && task.personalStatus!==4){
            var confirm=button('Báo hoàn thành phần việc','reportPersonal',task.id);
            confirm.className+=' kbg20-dot2-row-btn';row(actions,confirm);
        }else if(task.status===1 && task.assigned && state.workflowEnabled && !isPending(task)
            && task.personalStatus!==4){
            var done=button(task.personalStatus===2?'Báo hoàn thành phần việc':'Báo hoàn thành phần việc','reportPersonal',task.id);
            done.title='Thông báo đã hoàn thành phần việc được giao; chỉ dừng đồng hồ trên công việc này của bạn, không ảnh hưởng đồng nghiệp.';
            done.className+=' kbg20-dot2-row-btn';row(actions,done);
        }else if(isUpcoming)row(actions,node('span','kbg20-v41-wait-label',
            state.earlyStart && state.earlyStart.allowed?'Có thể bắt đầu sớm':'Sắp tới'));
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
            upcoming:'Công việc sắp tới',open:'Công việc có đồng hồ đang bật',pending:'Công việc chờ PM duyệt',
            all:'Tất cả công việc',reported:'Phần việc đã báo hoàn thành'};
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
        if(mode!=='history' && viewMode==='history')++historyRequest;
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
    // Nhan trang thai nhat ky chi la su kien cua NGAY dang xem; khong thay
    // nhan Task chung bang nhan bao hoan thanh phan viec ca nhan.
    function historyBadgeItems(h){
        var items=[];
        if(h.isEarly)items.push({text:'Làm sớm',type:'early'});
        if(h.isOverdue)items.push({text:'Quá hạn',type:'overdue'});
        var ps=Number(h.personalStatus)||0;
        if(ps===1||ps===3||ps===5)items.push({text:'Đã báo hoàn thành',type:'reported'});
        else if(ps===2)items.push({text:'Đã rút báo hoàn thành',type:'withdrawn'});
        else if(ps===4)items.push({text:'Giờ công đã được quản lý chốt',type:'locked'});
        if(h.isOpen)items.push({text:'Phiên đang mở',type:'open'});
        if(h.hadActivity && Number(h.total)===0)
            items.push({text:'Chưa phát sinh giờ tính công',type:'zero'});
        return items;
    }
    function renderHistory() {
        historyEl.textContent='';historyTotalsEl.textContent='';
        if(!historyData)return;
        var d=historyData;
        var summary=row(historyTotalsEl,node('div','kbg20-v41-history-summary'));
        [
          {label:'Giờ làm theo lịch · Ngày đang xem',value:d.regular},
          {label:'Giờ tăng ca · Ngày đang xem',value:d.overtime},
          {label:'Tổng giờ công đã ghi',value:d.total}
        ].forEach(function(c){var x=row(summary,node('div','kbg20-v41-history-stat'));
          row(x,node('small','',c.label));row(x,node('strong','',clock(c.value)));});
        if(!d.tasks||!d.tasks.length){row(historyEl,node('p','kbg20-empty','Không có hoạt động công việc trong ngày được chọn.'));return;}
        var hdr=row(historyEl,node('div','kbg20-v41-hist-head kbg20-v41-hist-grid'));
        ['Công việc','Giờ làm theo lịch','Giờ tăng ca','Tổng giờ công','Chi tiết'].forEach(function(t){row(hdr,node('span','',t));});
        d.tasks.forEach(function(h){
            var line=row(historyEl,node('article','kbg20-v41-hist-row kbg20-v41-hist-grid'));
            var name=taskCell(line,'kbg20-v41-hist-name','Công việc');
            row(name,node('strong','',h.code||'CV'));
            row(name,node('span','',h.name||'Chưa có tên công việc'));
            if(h.deleted)row(name,node('em','kbg20-v41-deleted','Đã xóa'));
            var badges=historyBadgeItems(h);
            if(badges.length){
                var wraps=row(name,node('span','kbg20-d6-badges'));
                badges.forEach(function(flag){row(wraps,node('span','kbg20-d6-badge is-'+flag.type,flag.text));});
            }
            ['regular','overtime','total'].forEach(function(k){
                var cell=taskCell(line,'kbg20-v41-hist-value',k==='regular'?'Giờ làm theo lịch':k==='overtime'?'Giờ tăng ca':'Tổng giờ công');
                row(cell,node('strong',k==='total'?'is-total':'',clock(h[k])));
            });
            var detail=taskCell(line,'kbg20-ux-history-actions','Chi tiết');
            var see=row(detail,node('button','kbg20-btn kbg20-outline kbg20-ux-detail-btn','Xem chi tiết'));
            see.type='button';see.setAttribute('aria-label','Xem chi tiết các lần Start/Stop của '+(h.code||'công việc'));
            see.addEventListener('click',function(){openHistoryDetail(h);});
        });
    }
    // Full start/end ledger for the SELECTED day and task, server-scoped to logged-in user.
    var sessionDetailSeq=0;
    function openHistoryDetail(task){
        var selectedDay=historyDateEl.value,seq=++sessionDetailSeq;
        var heading=document.getElementById('kbg20HistoryDetailTitle');
        var panel=document.getElementById('kbg20HistoryDetailBody');
        if(!panel||!heading)return;
        heading.textContent='['+(task.code||'CV')+'] '+(task.name||'')+' · '+selectedDay.split('-').reverse().join('/');
        panel.textContent='Đang tải lịch sử Start/Stop...';
        try{openExtraModal('mdlKbg20HistoryDetails','Chi tiết các lần tính giờ');}
        catch(e){notify(e.message,'error');return;}
        var q='?view=history-sessions&projectId='+encodeURIComponent(fixedProjectId)+
            '&taskId='+encodeURIComponent(task.id)+'&day='+encodeURIComponent(selectedDay);
        window.fetch(url+q,{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}})
          .then(getJson).then(function(data){
            if(seq!==sessionDetailSeq)return;
            if(data.taskId!==task.id || data.day!==selectedDay ||
               String(data.projectId).toLowerCase()!==fixedProjectId.toLowerCase())
               throw new Error('Chi tiết trả về không khớp công việc/ngày.');
            panel.textContent='';
            if(!(data.items||[]).length){row(panel,node('p','kbg20-ux-muted','Không có lần Start/Stop nào trong ngày này.'));return;}
            var table=row(panel,node('div','kbg20-ux-session-list'));
            data.items.forEach(function(x,i){
                var item=row(table,node('section','kbg20-ux-session-item'));
                var head=row(item,node('div','kbg20-ux-session-head'));
                row(head,node('strong','',String(i+1)+'. '+(Number(x.type)===2?'Giờ tăng ca':'Giờ hành chính')));
                row(head,node('span','',x.ended==null?'Phiên đang mở':'Đã kết thúc'));
                var grid=row(item,node('div','kbg20-ux-session-grid'));
                function field(name,value){var f=row(grid,node('div',''));row(f,node('small','',name));row(f,node('strong','',value));}
                field('Bắt đầu',localTime(x.started));
                field('Kết thúc',x.ended==null?'Chưa kết thúc':localTime(x.ended));
                field('Đã kết toán trong ngày',clock(x.committedSeconds));
            });
        }).catch(function(e){if(seq!==sessionDetailSeq)return;
            if(redirectAccessFailure(e))return;
            panel.textContent='';row(panel,node('p','kbg20-ux-muted','Không tải được chi tiết phiên.'));
            notify(e.message||'Không tải được chi tiết.','error');
        });
    }
    function refreshHistory() {
        updateHistoryNavigation();
        if (viewMode !== 'history' || !fixedProjectId || !state) return;
        var day = historyDateEl.value;
        if (!/^\d{4}-\d{2}-\d{2}$/.test(day)) {
            ++historyRequest;historyLoading=false;historyData=null;
            historyTotalsEl.textContent='';historyEl.textContent='';
            historyStatusEl.textContent = 'Hãy chọn ngày cần xem.';
            return;
        }
        var maxDay = state.workCalendar && state.workCalendar.day;
        if (maxDay && day > maxDay) {
            ++historyRequest;historyLoading=false;historyData=null;
            historyTotalsEl.textContent='';historyEl.textContent='';
            historyStatusEl.textContent='Không thể xem một ngày trong tương lai.';
            historyDateEl.value = maxDay;
            updateHistoryNavigation();
            return;
        }
        var sameDay=historyData && historyData.day===day;
        var request = ++historyRequest;
        historyLoading = true;
        // Poll cung ngay giu lai noi dung cu, khong nhay bang moi 30 giay.
        // Doi ngay thi xoa du lieu cu de khong gay nham ngay/dự án.
        if(!sameDay){
            historyData=null;
            historyTotalsEl.textContent='';historyEl.textContent='';
        }
        historyStatusEl.textContent = (sameDay?'Đang cập nhật nhật ký ngày ':'Đang tải nhật ký ngày ')+day.split('-').reverse().join('/')+'...';
        var query = '?view=history&projectId=' + encodeURIComponent(fixedProjectId) + '&day=' + encodeURIComponent(day);
        window.fetch(url + query,{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}})
            .then(getJson).then(function(data){
                if(request!==historyRequest) return;
                if(String(data.projectId||'').toLowerCase()!==fixedProjectId.toLowerCase() || data.day!==day)
                    throw new Error('Nhật ký trả về không khớp dự án/ngày đã chọn.');
                historyData=data;
                historyStatusEl.textContent='Ngày ' + day.split('-').reverse().join('/') + ' · trong dự án hiện tại';
                renderHistory();
            }).catch(function(err){
                if(request!==historyRequest)return;
                if(redirectAccessFailure(err))return;
                if(!sameDay){historyData=null;historyTotalsEl.textContent='';historyEl.textContent='';}
                historyStatusEl.textContent=sameDay?'Đang hiển thị dữ liệu lần tải trước.':'Không có dữ liệu để hiển thị.';
                notify(err.message||'Không tải được nhật ký.','error');
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
    function refresh(force) {
        // Don't let 30-second polling silently race a pending parallel-timer choice.
        if (loading || acting || parallelChoiceBusy) {
            if (force === true) refreshPending = true;
            return Promise.resolve();
        }
        refreshPending = false;
        if (!fixedProjectId) { notify('Không xác định được dự án đang mở.', 'error'); return Promise.resolve(); }
        loading = true;
        var query = '?projectId=' + encodeURIComponent(fixedProjectId);
        return window.fetch(url + query, { credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'} })
            .then(getJson).then(function (data) {
                if (String(data.projectId || '').toLowerCase() !== fixedProjectId.toLowerCase())
                    throw new Error('Dữ liệu phản hồi không thuộc dự án hiện tại.');
                offset = Number(data.serverNow) - Date.now();
                // Khong mang tong gio ngay hom qua sang nhan 'Hom nay' khi qua 00:00.
                if (todayDataDay && data.workCalendar && todayDataDay !== data.workCalendar.day) {
                    todayDataDay = '';
                    todayTaskTimes = Object.create(null);
                    ++todayRequest;
                }
                state = data;
                lastFetch = Date.now();
                refreshUi();
                // Polling 30s: Tab 3 doc lai trang thai ca nhan, KHONG dong/mo lai ExtraModal.
                if(viewMode==='manual' && window.Kbg20DoiSoatUI && window.Kbg20DoiSoatUI.renderTab)
                    window.Kbg20DoiSoatUI.renderTab();
                refreshToday();
                if (!historyDateEl.value && state.workCalendar) historyDateEl.value = state.workCalendar.day;
                if (state.workCalendar) historyDateEl.max = state.workCalendar.day;
                updateHistoryNavigation();
                if (viewMode === 'history' && !historyLoading) refreshHistory();
                notify('', '');
            }).catch(function (err) {
                if(!redirectAccessFailure(err))notify(err.message || 'Lỗi kết nối.', 'error');
            })
            .then(function(){
                loading = false;
                // Khi nhan workflow/thao tac xong trong luc polling, thuc hien refresh tiep.
                // Chi cho 1 request tai mot thoi diem, khong bo yeu cau va khong spin retry.
                if (refreshPending && !acting) refresh(true);
            });
    }
    // Called only after partner Ctrl Start/Complete/Review has saved successfully,
    // closed its ExtraModal and raised ActionStarted/ActionCompleted.
    // Update V2 in place; preserve selected tab/filter/search/scroll.
    window.Kbg20RefreshAfterWorkflow = function () {
        if (window.KbgFloatingTimer && typeof window.KbgFloatingTimer.notifyChanged==='function')
            window.KbgFloatingTimer.notifyChanged();
        // Luu co doi lam moi thay vi thu toi da 25 lan roi tu bo.
        refresh(true);
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
        if (acting || loading || !state) return;
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
                        return confirmAction('Xác nhận tiếp tục quá hạn',res.message||'Công việc đã quá hạn. Tiếp tục?')
                            .then(function(yes){if(!yes)return false;flags.confirmOverdue=true;return perform();});
                    }
                    if (res.confirmDependency && !flags.confirmDependency) {
                        return confirmAction('Cảnh báo công việc phụ thuộc',res.message||'Công việc phụ thuộc chưa hoàn thành. Tiếp tục?')
                            .then(function(yes){if(!yes)return false;flags.confirmDependency=true;return perform();});
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
            if (ok === true || refreshPending) refresh(true); else refreshUi();
        }).catch(function(err){
            acting=false;notify(err.message || 'Thao tác thất bại.','error');
            if(refreshPending)refresh(true);else refreshUi();
        });
    }
    // Dot 8: preview tai server + Stop/Start CUNG 1 transaction. KHONG STOP voi fetch rieng.
    var parallelChoiceBusy=false;
    function checkOtherTimersAndStart(taskId,onProceed,action){
        if(parallelChoiceBusy||acting||loading){notify('Đang đồng bộ đồng hồ. Vui lòng thử lại sau.','warning');return;}
        parallelChoiceBusy=true;
        var rootUrl=root.getAttribute('data-kbg20-floating-url')||'/fKhaiBaoGio/FloatingWorkTimer.ashx';
        Promise.all([
           window.fetch(rootUrl+'?check='+Date.now(),{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}}).then(getJson),
           window.fetch(url+'?view=switch-preview&projectId='+encodeURIComponent(fixedProjectId)+'&taskId='+encodeURIComponent(taskId),
             {credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}}).then(getJson)
        ]).then(function(pair){
            var info=pair[0],eligibleIds=pair[1].eligibleIds||[];
            var allow=new Set(eligibleIds.map(function(x){return String(x).toLowerCase();}));
            var others=(info.timers||[]).filter(function(x){return x.taskId!==taskId;});
            if(!others.length){parallelChoiceBusy=false;onProceed();return;}
            var allowed=others.filter(function(x){return allow.has(String(x.id).toLowerCase());});
            var protectedTimers=others.filter(function(x){return !allow.has(String(x.id).toLowerCase());});
            var place=document.getElementById('kbg20ParallelTasks');
            if(!place)throw new Error('Thiếu ExtraModal đồng hồ song song.');
            place.textContent='';
            function renderGroup(title,timers,cls){
                var header=row(place,node('strong','kbg20-ux-parallel-heading '+cls,title));
                if(!timers.length)row(place,node('div','kbg20-ux-muted','Không có phiên.'));
                timers.forEach(function(t){var cell=row(place,node('div','kbg20-ux-parallel-item '+cls));
                    row(cell,node('strong','', '['+(t.code||'CV')+'] '+(t.name||'Công việc')));
                    row(cell,node('small','',(t.projectName||'Dự án')+' · '+(Number(t.type)===2?'Tăng ca':'Giờ hành chính')+
                      ' · '+(t.counting?'Đang cộng giờ':'Phiên đang mở')));
                });
            }
            renderGroup('Sẽ dừng (chỉ phiên thủ công của Task lá)',allowed,'can-stop');
            renderGroup('Luôn giữ nguyên (Task cha/phiên tự động hoặc không phù hợp)',protectedTimers,'keep');
            var keep=document.getElementById('kbg20ParallelKeep');
            var stop=document.getElementById('kbg20ParallelStop');
            var cancel=document.getElementById('kbg20ParallelCancel');
            var task=(state.tasks||[]).find(function(x){return x.id===taskId;});
            var canAtomic=task&&Number(task.status)===1 && allowed.length>0 && allowed.length===eligibleIds.length;
            stop.disabled=!canAtomic;
            stop.title=canAtomic?'Dừng đúng phiên được liệt kê và Start trong một giao dịch':'Chỉ có thể Dừng cũ + Start khi Task mới đang thực hiện và có phiên Task lá thủ công.';
            function close(){parallelChoiceBusy=false;closeExtraModal('mdlKbg20Parallel');}
            cancel.onclick=close;
            keep.onclick=function(){close();onProceed();};
            stop.onclick=function(){
                if(!canAtomic)return;
                keep.disabled=true;stop.disabled=true;cancel.disabled=true;
                var requestId;
                try{requestId=guid();}catch(e){close();notify(e.message,'error');return;}
                var flags={overdue:false,dependency:false};
                function perform(){
                    var body=new URLSearchParams();body.set('action','switchStart');
                    body.set('projectId',fixedProjectId);body.set('taskId',taskId);
                    body.set('requestId',requestId);body.set('hourType',action==='overtime'?'2':'1');
                    body.set('stopSessionIds',eligibleIds.join(','));
                    body.set('confirmOverdue',String(flags.overdue));
                    body.set('confirmDependency',String(flags.dependency));
                    return window.fetch(url,{method:'POST',credentials:'same-origin',cache:'no-store',
                        headers:{'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8',
                         'X-KBG20-Token':token,'Accept':'application/json'},body:body.toString()})
                    .then(getJson).then(function(result){
                        if(result.confirmOverdue&&!flags.overdue)return confirmAction('Xác nhận quá hạn',result.message)
                            .then(function(ok){if(!ok)return false;flags.overdue=true;return perform();});
                        if(result.confirmDependency&&!flags.dependency)return confirmAction('Cảnh báo phụ thuộc',result.message)
                            .then(function(ok){if(!ok)return false;flags.dependency=true;return perform();});
                        if(!result.success)throw new Error(result.message||'Không chuyển được đồng hồ.');
                        return true;
                    });
                }
                // Phải đóng modal trước khi mở modal cảnh báo phụ thuộc/quá hạn.
                closeExtraModal('mdlKbg20Parallel');
                perform().then(function(ok){parallelChoiceBusy=false;
                    if(ok){notify('Đã dừng các phiên thủ công được chọn và Start Task mới.','ok');
                        if(window.KbgFloatingTimer)window.KbgFloatingTimer.notifyChanged();refresh(true);}
                    else notify('Đã hủy chuyển đồng hồ; không thay đổi dữ liệu.','info');
                }).catch(function(e){parallelChoiceBusy=false;
                    if(!redirectAccessFailure(e))notify('Không chuyển được đồng hồ: '+e.message,'error');refresh(true);
                }).then(function(){keep.disabled=false;stop.disabled=false;cancel.disabled=false;});
            };
            try{openExtraModal('mdlKbg20Parallel','Xác nhận khi chạy nhiều công việc');}
            catch(e){parallelChoiceBusy=false;notify(e.message,'error');}
        }).catch(function(e){parallelChoiceBusy=false;
            if(!redirectAccessFailure(e))notify('Không kiểm tra được đồng hồ khác; chưa Start: '+e.message,'error');
        });
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
    window.Kbg20PageUI={
        openManual:function(){setMode('manual');},refreshToday:refreshToday,refresh:refresh,
        getTask:function(taskId){return state&&(state.tasks||[]).find(function(t){return t.id===taskId;});},
        getLiveMinutes:function(taskId){
            var task=state&&(state.tasks||[]).find(function(t){return t.id===taskId;});if(!task)return null;
            function seconds(kind){var timer=timerOf(taskId,kind),base=Number(kind===1?task.regular:task.overtime)||0;
                return base+(timer?Math.max(0,liveSeconds(timer)-Number(timer.seconds||0)):0);}
            return {regular:Math.floor((seconds(1)+30)/60),overtime:Math.floor((seconds(2)+30)/60)};
        }
    };
    setMode('work');

    document.getElementById('kbg20TaskFilters').addEventListener('click',function(e){
        var chip=e.target.closest('[data-task-filter]');
        if(chip && this.contains(chip))setTaskFilter(chip.getAttribute('data-task-filter'));
    });
    searchEl.addEventListener('input',function(){filter=(searchEl.value||'').trim().toLocaleLowerCase('vi');visibleCount=pageSize;if(state)renderTasks();});
    document.getElementById('kbg20Reload').addEventListener('click',function(){refresh(true);});
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
        var task=state && (state.tasks||[]).find(function(x){return x.id===id;});
        if(action==='confirmPersonalHours'||action==='openPersonalHours'){
            if(!window.Kbg20DoiSoatUI){notify('Giao diện báo hoàn thành chưa được tải.','error');return;}
            setMode('manual');
            if(action==='confirmPersonalHours')window.Kbg20DoiSoatUI.openConfirm(id);
            else window.Kbg20DoiSoatUI.openTask(id);
            return;
        }
        if(action==='reportPersonal'||action==='withdrawPersonal'){
            if(!window.Kbg20DoiSoatUI){notify('Popup bao xong chua duoc tai. Hay tai lai trang.','error');return;}
            if(action==='reportPersonal')window.Kbg20DoiSoatUI.openReport(id);
            else window.Kbg20DoiSoatUI.openWithdraw(id);
            return;
        }
        // Bao xong ca nhan chi su dung 2 action rieng o tren.
        // Khong cho action Task chung 'complete/review' di qua bridge cua Tab 1.
        if(action==='complete' || action==='review') {
            notify('Hãy thực hiện duyệt hoặc hoàn thành công việc trong TaskList.', 'error');
            return;
        }
        if(['start','overtime','stop'].indexOf(action)<0) {
            notify('Thao tác không hợp lệ. Hãy tải lại trang.', 'error');
            return;
        }
        var isWorkflow=state && state.workflowEnabled &&
            (action==='start'||action==='overtime') && task && task.status===0;
        if(isWorkflow) {
            if(!task || !window.kbg20WorkflowPostback){notify('Không mở được xử lý công việc. Hãy tải lại trang.','error');return;}
            if((action==='start'||action==='overtime') && task.startDay &&
                state.workCalendar && task.startDay>state.workCalendar.day) {
                if(action==='overtime') {notify('Chưa đến ngày bắt đầu; không được Start tăng ca sớm.','error');return;}
                if(!state.earlyStart || !state.earlyStart.allowed){
                    notify('Chưa thể Start sớm: còn công việc đến hạn chưa báo hoàn thành.','error');return;
                }
            }
            var late=(action==='start'||action==='overtime') && task.endDay &&
                state.workCalendar && task.endDay<state.workCalendar.day;
            if(late){
                confirmAction('Công việc quá hạn','Công việc đã quá hạn. Tiếp tục tính giờ trên Task mới?')
                .then(function(yes){if(yes)checkOtherTimersAndStart(id,function(){
                    window.kbg20WorkflowPostback((action==='start'||action==='overtime')?'start':action,
                        id,true,action==='overtime');
                },action);});
                return;
            }
            checkOtherTimersAndStart(id,function(){
                window.kbg20WorkflowPostback((action==='start'||action==='overtime')?'start':action,
                    id,late,action==='overtime');
            },action);
            return;
        }
        if(action==='start'||action==='overtime')checkOtherTimersAndStart(id,function(){sendAction(action,id);},action);
        else sendAction(action,id);
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
            statuses[k].textContent=thisTask && isPending(thisTask)?'Phiên còn mở — kiểm tra chốt giờ':(counting?(Number(timer.type)===2?'Đang tính giờ tăng ca':'Đang tính giờ làm theo lịch'):idleClockStatus(Number(timer.type)));
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
        if(!loading && !acting && Date.now()-lastFetch>30000)refresh();
    },1000);
    refresh();
})(window,document);
