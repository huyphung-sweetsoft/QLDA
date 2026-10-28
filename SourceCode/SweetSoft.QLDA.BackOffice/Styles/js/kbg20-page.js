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
    // A status change is reflected without F5; keep the live ticking DOM when data has not changed.
    var visualKey = '';
    // Nhiep chinh do Master (chuong) phat moi 30 giay; 90s chi la du phong neu Master mat nhip.
    var AUTO_REFRESH_MS = 90000;
    function visualSnapshot(data) {
        // serverNow and each timer's running seconds change on every GET; they do not change actions/status.
        // A new session id / Stop / changed Task / personal report DOES change the rendered table.
        return JSON.stringify({
            projects:data.projects || [], tasks:data.tasks || [],
            timers:(data.timers || []).map(function(t){
                return [t.id,t.taskId,t.type,t.counting,t.boundary,t.startMs,t.endMs,t.status];
            }),
            workCalendar:data.workCalendar || null,
            workflowEnabled:data.workflowEnabled, earlyStart:data.earlyStart || null
        });
    }
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
    function apiFailure(message,status) {var e=new Error(message||'Không kết nối được với dịch vụ giờ công.');e.status=status||0;return e;}
    function getJson(response) {
        var contentType=response.headers.get('content-type')||'';
        if(contentType.toLowerCase().indexOf('application/json')<0)
            return Promise.reject(apiFailure('Máy chủ chưa cung cấp dữ liệu giờ công (HTTP '+response.status+'). Vui lòng liên hệ quản trị viên nếu lỗi tiếp diễn.',response.status));
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
            if(!content||!yes||!no){warningBusy=false;notify('Không mở được hộp thoại xác nhận. Thao tác chưa được thực hiện; vui lòng tải lại trang.','error');resolve(false);return;}
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
            var todayChanged=todayDataDay!==day || JSON.stringify(todayTaskTimes)!==JSON.stringify(next);
            todayTaskTimes=next;todayDataDay=day;
            // Keep filter/search and do not rebuild rows for identical history data.
            if(todayChanged && state && (taskFilter==='today' || taskFilter==='all'))renderTasks();
            else tasksEl.querySelectorAll('[data-today-task]').forEach(function(el){updateTodayLabel(el,el.getAttribute('data-today-task'),Number(el.getAttribute('data-today-type')));});
        }).catch(function(){
            if(request!==todayRequest)return;
            todayDataDay='';
            tasksEl.querySelectorAll('[data-today-task]').forEach(function(el){el.textContent='Giờ hôm nay: chưa tải được';});
        });
    }

    function overtimeAllowedBySchedule(){
        // UI chỉ gợi ý: server kiểm tra lại lịch ngay trong transaction.
        var cal=state && state.workCalendar;
        if(!cal)return false;
        var current=serverNow(),shifts=(cal.shifts||[]).concat(cal.previousShifts||[]);
        return !shifts.some(function(sh){return current>=Number(sh.startMs) && current<Number(sh.endMs);});
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
        if (!timer && isUpcoming) {
            var canEarly=!!(state.earlyStart && state.earlyStart.allowed);
            sub.textContent=canEarly?'Có thể bắt đầu sớm · không đổi ngày kế hoạch':
                'Chưa thể Start sớm · còn phần việc đến hạn';
            if(!canEarly && state.earlyStart && state.earlyStart.blockingTasks)
                sub.title=state.earlyStart.blockingTasks.join('; ');
        }
        // Giữ tooltip chi tiết cũ nếu server đã cung cấp lý do chặn Start sớm.
        if(!sub.title)sub.title=sub.textContent;
        var todaySub=row(stats,node('small','kbg20-v43-today'));todaySub.setAttribute('data-today-task',task.id);
        todaySub.setAttribute('data-today-type',String(type));
        updateTodayLabel(todaySub,task.id,type);
        if(timer){
            var title=regular?'Dừng giờ làm theo lịch':'Dừng giờ tăng ca';
            cell.insertBefore(iconAction(title,'fas fa-stop','stop',timer.id,'is-stop',!regular),stats);
        } else if(taskMayStart(task) && (regular || overtimeAllowedBySchedule()) &&
                (!isUpcoming || (state.earlyStart && state.earlyStart.allowed))){
            var resume=regular && (task.startedRegular || Number(task.regular)>0) ||
                !regular && (task.startedOvertime || Number(task.overtime)>0);
            var action=regular?'start':'overtime';
            var label=isUpcoming?'Bắt đầu sớm · '+(regular?'Giờ làm theo lịch':'Giờ tăng ca'):
                (resume?'Tiếp tục':'Bắt đầu')+(regular?' giờ làm theo lịch':' giờ tăng ca');
            if(task.status===0 && state.workflowEnabled)label+=' · Chuyển công việc sang Đang thực hiện trước khi bấm giờ';
            cell.insertBefore(iconAction(label,resume?'fas fa-redo-alt':'fas fa-play',action,task.id,
                resume?'is-resume':'is-start',!regular),stats);
        }else{
            var reason=task.personalStatus===1||task.personalStatus===3?'Phần việc này đã được báo hoàn thành':isPending(task)?'Công việc đang chờ duyệt':isFinished(task)?'Công việc đã hoàn thành':
                isUpcoming?(state.earlyStart && !state.earlyStart.allowed?
                    'Chưa thể Start sớm: còn '+state.earlyStart.remainingCount+' phần việc đến hạn chưa báo hoàn thành':
                    'Chưa đủ điều kiện bắt đầu sớm'):(!regular && !overtimeAllowedBySchedule()
                ?'Chỉ được bắt đầu tăng ca ngoài những ca làm việc theo lịch':
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
    function nguonStart(phien){
        if(!phien)return null;
        var source=Number(phien.source);
        if(source===2){
            var task=phien.triggerTaskCode?('['+phien.triggerTaskCode+'] '+(phien.triggerTaskName||'')):'công việc con (chưa có thông tin)';
            var actor=phien.triggerActor||'người kích hoạt chưa xác định';
            return {title:'Tự động',detail:'Tự động bắt đầu khi '+actor+' bắt đầu '+task+'.'};
        }
        if(source===1)return {title:'Thủ công',detail:'Phiên đồng hồ này được bắt đầu bằng thao tác thủ công của nhân viên.'};
        return null;
    }
    // Tooltip noi bo: chung cho nguon Start va badge lam som, khong dung title mac dinh.
    var kbgTip=null,kbgTipTarget=null;
    function hideKbgTip(){if(kbgTip)kbgTip.hidden=true;kbgTipTarget=null;}
    function positionKbgTip(){
        if(!kbgTip||!kbgTipTarget||kbgTip.hidden)return;
        var r=kbgTipTarget.getBoundingClientRect(),w=kbgTip.offsetWidth,h=kbgTip.offsetHeight;
        var left=Math.min(Math.max(10,r.left+(r.width-w)/2),Math.max(10,window.innerWidth-w-10));
        var up=r.top>=h+12,top=up?r.top-h-8:r.bottom+8;
        if(!up&&top+h>window.innerHeight-8)top=Math.max(8,window.innerHeight-h-8);
        kbgTip.style.left=Math.round(left)+'px';kbgTip.style.top=Math.round(top)+'px';
    }
    function showKbgTip(target,heading,detail){
        if(!kbgTip){kbgTip=node('div','kbg20-ux-smart-tooltip');kbgTip.hidden=true;document.body.appendChild(kbgTip);}
        kbgTip.textContent='';row(kbgTip,node('strong','',heading));row(kbgTip,node('p','',detail));
        kbgTipTarget=target;kbgTip.hidden=false;positionKbgTip();
    }
    window.addEventListener('scroll',positionKbgTip,true);
    window.addEventListener('resize',positionKbgTip);
    function ganTooltipNho(el,heading,detail){
        if(!el||!detail)return;
        el.removeAttribute('title');el.setAttribute('tabindex','0');
        el.setAttribute('aria-label',heading+'. '+detail);
        el.addEventListener('mouseenter',function(){showKbgTip(el,heading,detail);});
        el.addEventListener('mouseleave',function(){if(kbgTipTarget===el)hideKbgTip();});
        el.addEventListener('focus',function(){showKbgTip(el,heading,detail);});
        el.addEventListener('blur',function(){if(kbgTipTarget===el)hideKbgTip();});
    }
    function ganNhanNguon(host,phien,loai){
        var src=nguonStart(phien);if(!src)return;
        var badge=row(host,node('span','kbg20-v41-source-badge'+(Number(phien.source)===2?' is-auto':' is-manual'),src.title));
        ganTooltipNho(badge,'Nguồn bắt đầu · '+(loai||'Phiên giờ')+' · '+src.title,src.detail);
    }
    function renderTaskRow(task,isUpcoming){
        var t=timerOf(task.id,1),ot=timerOf(task.id,2);
        var item=node('article','kbg20-v41-task-row kbg20-v41-taskgrid'+
            ((t||ot)?' is-running':'')+(isPending(task)?' is-pending':''));
        item.setAttribute('aria-label',(task.code||'')+' '+(task.name||''));
        var identity=taskCell(item,'kbg20-v41-identity','Công việc');
        var name=row(identity,node('div','kbg20-v41-taskname'));
        row(name,node('strong','kbg20-v41-taskcode',task.code||'CV'));
        var fullName=task.name||'Chưa đặt tên công việc';
        var titleElement=row(name,node('strong','kbg20-v41-tasktext',fullName));
        // Dùng tooltip chung đang có (có tự chọn hướng và hỗ trợ keyboard), không dùng native title.
        ganTooltipNho(titleElement,'Tên công việc đầy đủ', '['+(task.code||'CV')+'] '+fullName);
        if(task.isEarly){
            var early=row(name,node('span','kbg20-d6-badge is-early'+(task.earlyInferred?' is-inferred':''),
                task.earlyInferred?'Bắt đầu sớm?':'Bắt đầu sớm'));
            ganTooltipNho(early,task.earlyInferred?'Có dấu hiệu bắt đầu sớm':'Bắt đầu sớm',earlyStartTooltip(task));
        } else if(state.earlyStart && state.earlyStart.allowed && task.startDay &&
                  state.workCalendar && task.startDay>state.workCalendar.day &&
                  taskMayStart(task) && !timerOf(task.id,1) && !timerOf(task.id,2)){
            row(name,node('span','kbg20-d6-badge is-can-early','Có thể bắt đầu sớm'));
        }
        var meta='';
        if(Number(task.regular)>0||Number(task.overtime)>0)meta='Giờ đã làm được bảo lưu';
        else if(isUpcoming)meta='Theo kế hoạch · chưa đến ngày bắt đầu';
        else if(task.assigned)meta='Công việc được phân công cho bạn';
        else meta='Đồng hồ đang mở từ dữ liệu trước';
        var metaLine=row(identity,node('div','kbg20-v41-identity-meta'));
        row(metaLine,node('small','kbg20-v41-task-note',meta));
        // Nguon Start la thong tin cua PHIEN, thuoc cot Cong viec, khong dat vao cot dong ho.
        if(t)ganNhanNguon(metaLine,t,'Giờ làm theo lịch');
        if(ot)ganNhanNguon(metaLine,ot,'Giờ tăng ca');
        // IdCongViec bat bien, khac MaCongViec/TenCongViec co the doi do reindex/edit.
        // Khong fallback theo code/name khi response cu thieu taskId: tranh gan nham nguon.
        var last=(state.history||[]).filter(function(p){return p.id&&p.taskId===task.id;})
           .sort(function(a,b){return Number(b.started||0)-Number(a.started||0);})[0];
        if(!t&&!ot&&last){var tag=nguonStart(last);if(tag){
            var line=row(metaLine,node('small','kbg20-v41-source-history', 'Lần gần nhất: '+tag.title.toLowerCase()));
            ganTooltipNho(line,'Nguồn bắt đầu lần gần nhất',tag.detail);
        }}
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
                // Chỉ mở popup GỬI yêu cầu Task chung sau khi đã báo xong PHẦN VIỆC cá nhân.
                // TaskList quyết định nghiệp vụ và kiểm tra lại dữ liệu, không tự gửi duyệt ở client.
                if(task.assigned && state.workflowEnabled){
                    actions.className+=' is-multi-action';
                    var sendReview=button('Gửi duyệt hoàn thành','sendTaskApproval',task.id);
                    sendReview.className+=' kbg20-dot2-row-btn is-submit-approval';
                    sendReview.title='Mở yêu cầu hoàn thành công việc trên Danh sách công việc';
                    row(actions,sendReview);
                }
            }else{
                var review=button('Xem / sửa giờ đã báo','openPersonalHours',task.id);
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
        hideKbgTip();
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
    function earlyStartTooltip(item){
        var when=item.earlyStarted?new Date(Number(item.earlyStarted)).toLocaleString('vi-VN',
            {timeZone:'Asia/Ho_Chi_Minh',hour12:false}):'không xác định';
        return 'Thời điểm bắt đầu: '+when+'. '+
            (item.earlyInferred?'Dữ liệu cũ không có ảnh chụp ngày kế hoạch tại lúc bắt đầu; so sánh với kế hoạch hiện tại ('+
                (item.earlyPlan||'không xác định')+'), nên đây chỉ là dấu hiệu tham khảo.':
                'Ngày kế hoạch được lưu tại thời điểm bắt đầu: '+(item.earlyPlan||'không xác định')+'.')+
            ' Loại phiên: '+(Number(item.earlyType)===2?'Giờ tăng ca.':'Giờ làm theo lịch.');
    }
    function historyBadgeItems(h){
        var items=[];
        if(h.isEarly)items.push({text:h.earlyInferred?'Bắt đầu sớm?':'Bắt đầu sớm',type:h.earlyInferred?'early is-inferred':'early',title:earlyStartTooltip(h)});
        var startModes=(h.startSources||[]),hasAuto=startModes.some(function(x){return Number(x.source)===2;}),hasManual=startModes.some(function(x){return Number(x.source)===1;});
        if(hasAuto)items.push({text:'Tự động',type:'source-auto',title:startModes.filter(function(x){return Number(x.source)===2;}).map(function(x){return nguonStart(x).detail;}).join(' · ')});
        if(hasManual)items.push({text:'Thủ công',type:'source-manual',title:'Có phiên được nhân viên trực tiếp bắt đầu trong ngày này.'});
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
                badges.forEach(function(flag){
                    var badge=row(wraps,node('span','kbg20-d6-badge is-'+flag.type,flag.text));
                    if(flag.title)ganTooltipNho(badge,flag.text,flag.title);
                });
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
                row(head,node('strong','',String(i+1)+'. '+(Number(x.type)===2?'Giờ tăng ca':'Giờ làm theo lịch')));
                row(head,node('span','',x.ended==null?'Phiên đang mở':'Đã kết thúc'));
                var grid=row(item,node('div','kbg20-ux-session-grid'));
                function field(name,value){var f=row(grid,node('div',''));row(f,node('small','',name));row(f,node('strong','',value));}
                field('Bắt đầu',localTime(x.started));
                var origin=nguonStart(x);if(origin){field('Nguồn bắt đầu',origin.title);var explain=row(item,node('p','kbg20-v41-source-description',origin.detail));explain.title=origin.detail;}
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
                var nextVisualKey=visualSnapshot(data);
                var visualChanged=nextVisualKey!==visualKey;
                state = data;
                lastFetch = Date.now();
                visualKey=nextVisualKey;
                if(visualChanged)refreshUi();
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
                if(!redirectAccessFailure(err))notify(err.message || 'Không tải được dữ liệu giờ công. Vui lòng kiểm tra kết nối rồi thử lại.', 'error');
            })
            .then(function(){
                loading = false;
                // Khi nhan workflow/thao tac xong trong luc polling, thuc hien refresh tiep.
                // Chi cho 1 request tai mot thoi diem, khong bo yeu cau va khong spin retry.
                if (refreshPending && !acting) refresh(true);
                else if (pendingParallelStart && !acting && !parallelChoiceBusy) {
                    // Click Start while 30s polling is fetching: defer once instead of a misleading warning.
                    var next=pendingParallelStart;pendingParallelStart=null;
                    checkOtherTimersAndStart(next.taskId,next.onProceed,next.action);
                }
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
        acting=true; visualKey=''; refreshUi();
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
                    notify((res.message || 'Thao tác đã hoàn tất.') + (res.warnings && res.warnings.length ?
                        ' Lưu ý: ' + res.warnings.join(' ') : ''),res.ok?'ok':'error');
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
            acting=false;notify(err.message || 'Không thực hiện được thao tác. Vui lòng thử lại.','error');
            if(refreshPending)refresh(true);else refreshUi();
        });
    }
    // Dot 8: preview tai server + Stop/Start CUNG 1 transaction. KHONG STOP voi fetch rieng.
    // Stop a single existing manual leaf session is a separate, explicitly requested action.
    var parallelChoiceBusy=false,parallelTransferInFlight=false,pendingParallelStart=null;
    var parallelModal=document.getElementById('mdlKbg20Parallel');
    function onParallelModalHidden(){
        // ExtraModal's header X, backdrop and Escape must release the UI lock too.
        if(parallelTransferInFlight)return;
        parallelChoiceBusy=false;
        if(refreshPending && !acting)refresh(true);
    }
    if(parallelModal){
        parallelModal.addEventListener('hidden.bs.modal',onParallelModalHidden);
        var modalJq=window.jQuery||window.$;
        if(modalJq&&modalJq.fn&&modalJq.fn.modal)
            modalJq(parallelModal).off('hidden.bs.modal.kbg20ParallelReset')
                .on('hidden.bs.modal.kbg20ParallelReset',onParallelModalHidden);
    }
    function checkOtherTimersAndStart(taskId,onProceed,action){
        if(loading && !parallelChoiceBusy && !acting){
            pendingParallelStart={taskId:taskId,onProceed:onProceed,action:action};return;
        }
        if(parallelChoiceBusy||acting){notify('Một thao tác đồng hồ đang được xử lý. Vui lòng hoàn tất thao tác hiện tại.','warning');return;}
        parallelChoiceBusy=true;
        var rootUrl=root.getAttribute('data-kbg20-floating-url')||'/fKhaiBaoGio/FloatingWorkTimer.ashx';
        Promise.all([
           window.fetch(rootUrl+'?check='+Date.now(),{credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}}).then(getJson),
           window.fetch(url+'?view=switch-preview&projectId='+encodeURIComponent(fixedProjectId)+'&taskId='+encodeURIComponent(taskId),
             {credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}}).then(getJson)
        ]).then(function(pair){
            var info=pair[0],eligibleIds=(pair[1].eligibleIds||[]).map(function(x){return String(x).toLowerCase();});
            var allow=new Set(eligibleIds);
            var others=(info.timers||[]).filter(function(x){return String(x.taskId||'').toLowerCase()!==String(taskId).toLowerCase();});
            if(!others.length){parallelChoiceBusy=false;onProceed();return;}
            var allowed=others.filter(function(x){return allow.has(String(x.id||'').toLowerCase());});
            // Never stop a server-authorized timer which was missing from the displayed list.
            // E.g. a stale floating widget response must fail safely, not silently stop an invisible timer.
            if(allowed.length!==allow.size)throw new Error('Danh sách đồng hồ vừa thay đổi. Vui lòng tải lại trước khi chọn dừng.');
            // Chi mo popup neu co dong ho thuc su duoc phep chon dung.
            // Dong ho Task cha duoc bao ve van chay; khong can buoc xac nhan lua chon rong.
            if(allowed.length===0){parallelChoiceBusy=false;onProceed();return;}
            var protectedTimers=others.filter(function(x){return !allow.has(String(x.id||'').toLowerCase());});
            var place=document.getElementById('kbg20ParallelTasks');
            var keep=document.getElementById('kbg20ParallelKeep');
            var stop=document.getElementById('kbg20ParallelStop');
            var guide=document.getElementById('kbg20ParallelGuide');
            var summary=document.getElementById('kbg20ParallelSummary');
            if(!place||!keep||!stop||!summary)throw new Error('Không mở được popup quản lý đồng hồ. Vui lòng tải lại trang.');
            place.textContent='';
            var selectedStop=new Set();
            function renderGroup(title,timers,cls){
                if(!timers.length)return;
                row(place,node('strong','kbg20-ux-parallel-heading '+cls,title));
                timers.forEach(function(t){
                    var cell=row(place,node('div','kbg20-ux-parallel-item '+cls));
                    var titleText='['+(t.code||'CV')+'] '+(t.name||'Công việc');
                    if(cls==='can-stop'&&t.id&&allow.has(String(t.id).toLowerCase())){
                        var label=row(cell,node('label','kbg20-parallel-choice'));
                        var checkbox=document.createElement('input');checkbox.type='checkbox';
                        checkbox.value=String(t.id).toLowerCase();checkbox.setAttribute('aria-label','Dừng '+titleText);
                        label.appendChild(checkbox);
                        var main=row(label,node('span','kbg20-parallel-choice-content'));
                        row(main,node('strong','',titleText));
                        row(main,node('small','',(t.projectName||'Dự án')+' · '+(Number(t.type)===2?'Giờ tăng ca':'Giờ làm theo lịch')+
                            ' · '+(t.counting?'Đang tính giờ':'Phiên vẫn đang mở')));
                        checkbox.onchange=function(){
                            if(checkbox.checked)selectedStop.add(checkbox.value);else selectedStop.delete(checkbox.value);
                            updateSummary();
                        };
                    }else{
                        row(cell,node('strong','',titleText));
                        row(cell,node('small','',(t.projectName||'Dự án')+' · '+(Number(t.type)===2?'Giờ tăng ca':'Giờ làm theo lịch')+
                            ' · Hệ thống tiếp tục theo dõi, không dừng từ popup này'));
                    }
                });
            }
            renderGroup('Các đồng hồ có thể dừng: tích chọn để dừng',allowed,'can-stop');
            renderGroup('Đồng hồ được bảo vệ — luôn tiếp tục chạy',protectedTimers,'keep');
            var task=(state.tasks||[]).find(function(x){return String(x.id||'').toLowerCase()===String(taskId).toLowerCase();});
            var taskIsNew=task&&Number(task.status)===0;
            var taskIsRunning=task&&Number(task.status)===1;
            if(!taskIsNew&&!taskIsRunning)throw new Error('Trạng thái công việc đã thay đổi. Vui lòng tải lại trang.');
            if(guide)guide.textContent=(taskIsNew?'Công việc mới chưa bắt đầu: sau khi chọn, hệ thống sẽ mở bước xác nhận bắt đầu công việc. ':
                'Công việc đang thực hiện: hệ thống sẽ chuyển đồng hồ trong cùng giao dịch. ')+
                'Chỉ những đồng hồ bạn được phép dừng mới có ô chọn. Dừng đồng hồ không có nghĩa là hoàn thành công việc.';
            function updateSummary(){
                var count=selectedStop.size,kept=allowed.length-count;
                summary.textContent='Đã chọn dừng '+count+' đồng hồ · Giữ '+(kept+protectedTimers.length)+
                    ' đồng hồ · Công việc mới sẽ bắt đầu tính giờ.';
                keep.textContent=count?'Dừng '+count+' và chạy song song với phần còn lại':'Chạy song song (giữ các đồng hồ cũ)';
                stop.disabled=allowed.length===0;
                stop.title=allowed.length?'Dừng tất cả '+allowed.length+' đồng hồ được phép rồi bắt đầu Task mới':'Không có đồng hồ thủ công nào được phép dừng';
            }
            updateSummary();
            function close(){parallelChoiceBusy=false;closeExtraModal('mdlKbg20Parallel');}
            function performSwitch(stopIds){
                if(acting||loading||parallelTransferInFlight){notify('Một thao tác đồng hồ đang được xử lý. Vui lòng thử lại.','warning');return;}
                if(stopIds.length===0){close();onProceed();return;}
                // Task 0 must run its genuine TaskList/partner Start workflow; stopping before it
                // as independent POSTs would leave old clocks stopped if workflow is cancelled.
                if(taskIsNew){close();onProceed({stopIds:stopIds,eligibleIds:eligibleIds});return;}
                keep.disabled=true;stop.disabled=true;parallelTransferInFlight=true;
                var requestId;
                try{requestId=guid();}catch(e){parallelTransferInFlight=false;close();notify(e.message,'error');return;}
                var flags={overdue:false,dependency:false};
                function perform(){
                    var body=new URLSearchParams();body.set('action','switchStart');
                    body.set('projectId',fixedProjectId);body.set('taskId',taskId);
                    body.set('requestId',requestId);body.set('hourType',action==='overtime'?'2':'1');
                    body.set('stopSessionIds',stopIds.join(','));
                    body.set('expectedSessionIds',eligibleIds.join(','));
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
                closeExtraModal('mdlKbg20Parallel');
                perform().then(function(ok){parallelTransferInFlight=false;parallelChoiceBusy=false;
                    if(ok){notify('Đã cập nhật các đồng hồ và bắt đầu công việc mới.','ok');
                        if(window.KbgFloatingTimer)window.KbgFloatingTimer.notifyChanged();refresh(true);}
                    else notify('Đã hủy chuyển đồng hồ. Các phiên đang chạy không thay đổi.','info');
                }).catch(function(e){parallelTransferInFlight=false;parallelChoiceBusy=false;
                    if(!redirectAccessFailure(e))notify('Không thể chuyển đồng hồ: '+e.message,'error');refresh(true);
                }).then(function(){keep.disabled=false;stop.disabled=allowed.length===0;});
            }
            keep.onclick=function(){performSwitch(Array.from(selectedStop));};
            stop.onclick=function(){performSwitch(eligibleIds.slice());};
            try{openExtraModal('mdlKbg20Parallel','Chọn đồng hồ khi bắt đầu công việc mới');}
            catch(e){parallelChoiceBusy=false;notify(e.message,'error');}
        }).catch(function(e){parallelChoiceBusy=false;
            if(!redirectAccessFailure(e))notify('Chưa thể kiểm tra đồng hồ đang chạy: '+e.message,'error');
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
            // Mở thẳng popup xem/sửa, KHÔNG chuyển Tab 3 phía sau ExtraModal.
            if(action==='confirmPersonalHours')window.Kbg20DoiSoatUI.openConfirm(id);
            else window.Kbg20DoiSoatUI.openTask(id);
            return;
        }
        if(action==='reportPersonal'||action==='withdrawPersonal'){
            if(!window.Kbg20DoiSoatUI){notify('Không mở được popup Báo hoàn thành phần việc. Vui lòng tải lại trang.','error');return;}
            if(action==='reportPersonal')window.Kbg20DoiSoatUI.openReport(id);
            else window.Kbg20DoiSoatUI.openWithdraw(id);
            return;
        }
        if(action==='sendTaskApproval'){
            // Không thực hiện cập nhật trạng thái trực tiếp: chuyển đến CtrlFastCompleteTask sẵn có.
            if(!task || !task.assigned || !state.workflowEnabled || task.status!==1 ||
               [1,3,5].indexOf(Number(task.personalStatus))<0){
                notify('Công việc đã thay đổi hoặc phần việc chưa được báo hoàn thành. Vui lòng làm mới dữ liệu.','error');
                refresh(true);return;
            }
            var taskListUrl=root.getAttribute('data-task-list-url');
            if(!taskListUrl){notify('Không xác định được đường dẫn Danh sách công việc.','error');return;}
            try{
                var target=new URL(taskListUrl,window.location.href);
                if(target.origin!==window.location.origin)throw new Error('Địa chỉ không hợp lệ');
                // Đường mở riêng của KBG20: không qua enum ThongBaoAction (có thể thiếu trong Core).
                target.searchParams.set('kbg20SendTaskCompletion',task.id);
                target.searchParams.delete('notificationAction');
                target.searchParams.delete('notificationTaskId');
                window.location.assign(target.href);
            }catch(err){notify('Không mở được yêu cầu duyệt hoàn thành. Vui lòng thử lại.','error');}
            return;
        }
        // Không cho action Task chung 'complete/review' đi qua bridge của Tab 1.
        if(action==='complete' || action==='review') {
            notify('Hãy duyệt hoặc xác nhận hoàn thành công việc tại Danh sách công việc.', 'error');
            return;
        }
        if(['start','overtime','stop'].indexOf(action)<0) {
            notify('Thao tác không hợp lệ. Hãy tải lại trang.', 'error');
            return;
        }
        var isWorkflow=state && state.workflowEnabled &&
            (action==='start'||action==='overtime') && task && task.status===0;
        if(isWorkflow) {
            if(!task || !window.kbg20WorkflowPostback){notify('Không mở được chức năng xử lý công việc. Vui lòng tải lại trang.','error');return;}
            if((action==='start'||action==='overtime') && task.startDay &&
                state.workCalendar && task.startDay>state.workCalendar.day) {
                if(!state.earlyStart || !state.earlyStart.allowed){
                    notify('Chưa thể bắt đầu sớm vì vẫn còn phần việc đến hạn chưa báo hoàn thành.','error');return;
                }
            }
            var late=(action==='start'||action==='overtime') && task.endDay &&
                state.workCalendar && task.endDay<state.workCalendar.day;
            if(late){
                confirmAction('Công việc quá hạn','Công việc đã quá hạn. Tiếp tục tính giờ trên Task mới?')
                .then(function(yes){if(yes)checkOtherTimersAndStart(id,function(plan){
                    window.kbg20WorkflowPostback((action==='start'||action==='overtime')?'start':action,
                        id,true,action==='overtime',plan);
                },action);});
                return;
            }
            checkOtherTimersAndStart(id,function(plan){
                window.kbg20WorkflowPostback((action==='start'||action==='overtime')?'start':action,
                    id,late,action==='overtime',plan);
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
        if(!document.hidden && !loading && !acting && Date.now()-lastFetch>AUTO_REFRESH_MS)refresh();
    },1000);
    // Mot nhip polling chung voi chuong. Khong kiem tra unread-count > 0;
    // so thong bao khong doi van phai cap nhat Task.
    function refreshWhenVisible(force) {
        if(document.hidden || !state) return;
        if(force || Date.now()-lastFetch>3000) refresh(true);
    }
    document.addEventListener('qlda:notification-poll-complete',function(){
        refreshWhenVisible(true);
    });
    // Chu dong kiem tra khi quay lai tab; khong bat them polling dinh ky.
    window.addEventListener('focus',function(){refreshWhenVisible(false);});
    window.addEventListener('pageshow',function(event){if(event.persisted)refreshWhenVisible(true);});
    document.addEventListener('visibilitychange',function(){if(!document.hidden)refreshWhenVisible(false);});
    refresh();
})(window,document);
