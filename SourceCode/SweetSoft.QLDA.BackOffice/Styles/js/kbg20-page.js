/* KhaiBaoGio 2.0: only active on #kbg20Page. Does not modify V1 handlers. */
(function (window, document) {
    'use strict';
    var root = document.getElementById('kbg20Page');
    if (!root) return;
    var url = root.getAttribute('data-api-url');
    var token = root.getAttribute('data-request-token');
    var state = null, loading = false, acting = false, offset = 0;
    var projectSelect = document.getElementById('kbg20Project');
    var tasksEl = document.getElementById('kbg20Tasks');
    var timerEl = document.getElementById('kbg20Timers');
    var historyEl = document.getElementById('kbg20History');
    var summaryEl = document.getElementById('kbg20Summary');
    var messageEl = document.getElementById('kbg20Message');
    var searchEl = document.getElementById('kbg20Search');
    var filter = '';
    var lastFetch = 0;
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
    function projectFromUrl() {
        try { var q = new URL(window.location.href).searchParams;
            return q.get('projectId') || q.get('ProjectId') || q.get('IdDuAn') || ''; }
        catch (_) { return ''; }
    }
    function setProjectInUrl(id) {
        try {
            var u = new URL(window.location.href);
            if (id) u.searchParams.set('projectId', id);
            window.history.replaceState(window.history.state, '', u.pathname + u.search + u.hash);
        } catch (_) { }
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
    function renderProjects() {
        var original = state.projectId;
        projectSelect.textContent = '';
        if (!state.projects.length) {
            row(projectSelect, node('option', '', 'Không có dự án được phân công'));
            projectSelect.disabled = true; return;
        }
        projectSelect.disabled = false;
        state.projects.forEach(function (p) {
            var option = node('option', '', (p.code ? p.code + ' — ' : '') + p.name);
            option.value = p.id;
            row(projectSelect, option);
        });
        projectSelect.value = original;
        if (original) setProjectInUrl(original);
    }
    function renderSummary() {
        summaryEl.textContent = '';
        var visible = state.timers || [];
        var cards = [
            ['Đang mở', visible.length],
            ['Đang cộng giờ', visible.filter(function (v) { return v.counting; }).length],
            ['Đang tạm ngừng cộng', visible.filter(function (v) { return !v.counting; }).length]
        ];
        cards.forEach(function (c) { var wrap = node('div', 'kbg20-stat');
            row(wrap, node('strong', '', c[1])); row(wrap, node('span', '', c[0])); row(summaryEl, wrap);
        });
    }
    function renderTasks() {
        tasksEl.textContent = '';
        if (!state.tasks.length) {
            row(tasksEl, node('div', 'kbg20-empty', 'Không có công việc được phân công hoặc có lịch sử trong dự án này.'));
        }
        var matched = 0;
        state.tasks.forEach(function (task) {
            if (filter && (task.code + ' ' + task.name).toLocaleLowerCase('vi').indexOf(filter) < 0) return;
            matched++;
            var item = node('article', 'kbg20-card');
            var head = row(item, node('div', 'kbg20-task-head'));
            var title = row(head, node('div', 'kbg20-task-title'));
            row(title, node('strong', '', task.code || 'Công việc'));
            row(title, node('span', '', task.name || ''));
            row(head, node('span', 'kbg20-label', task.pending ? 'Chờ duyệt' :
                task.status === 2 ? 'Hoàn thành' : task.status === 3 ? 'Hoàn thành trễ' :
                task.status === 1 ? 'Đang thực hiện' : 'Chưa bắt đầu'));
            var info = row(item, node('div', 'kbg20-meta'));
            row(info, node('span', '', 'Giờ thường: ' + clock(task.regular)));
            row(info, node('span', '', 'Tăng ca: ' + clock(task.overtime)));
            row(info, node('span', '', 'Hạn: ' + (task.end || 'Chưa có')));
            var active = state.timers.filter(function (x) { return x.taskId === task.id; });
            var acts = row(item, node('div', 'kbg20-actions'));
            if (active.length) row(acts, node('span', 'kbg20-running-text', active.length + ' đồng hồ đang mở'));
            if (task.assigned && (task.status === 0 || task.status === 1) && !task.pending) {
                if (!active.some(function (x) { return x.type === 1; }))
                    row(acts, button('Bắt đầu / Tiếp tục', 'start', task.id));
                if (!active.some(function (x) { return x.type === 2; }))
                    row(acts, button('Tăng ca', 'overtime', task.id));
            }
            row(tasksEl, item);
        });
        document.getElementById('kbg20TaskCount').textContent = '(' + matched + ')';
    }
    function renderTimers() {
        timerEl.textContent = '';
        document.getElementById('kbg20OpenCount').textContent = '(' + state.timers.length + ')';
        if (!state.timers.length) {row(timerEl,node('p','kbg20-empty','Không có đồng hồ đang mở trong dự án này.'));return;}
        state.timers.forEach(function (t) {
            var item = node('article', 'kbg20-card kbg20-timer');
            var head = row(item, node('div', 'kbg20-task-head'));
            row(head,node('div','kbg20-task-title',(t.code || 'Công việc') + ' — ' + (t.name || '')));
            row(head,node('span','kbg20-label ' + (t.counting ? 'kbg20-active':'kbg20-paused'),
                t.counting ? 'Đang cộng giờ' : 'Đang nghỉ / ngoài ca'));
            row(item,node('p','kbg20-meta', (t.type === 2 ? 'Tăng ca' : 'Giờ thường') +
                (t.source === 2 ? ' · Tự động từ task con' : ' · Thủ công')));
            var timer = row(item,node('strong','kbg20-clock',clock(liveSeconds(t))));
            timer.setAttribute('data-timer-id',t.id);
            row(item,button('Dừng đồng hồ', 'stop', t.id));
            row(timerEl,item);
        });
    }
    function renderHistory() {
        historyEl.textContent='';
        if (!state.history.length) {row(historyEl,node('p','kbg20-empty','Chưa có phiên đã đóng.'));return;}
        state.history.forEach(function (h) {
            var item=row(historyEl,node('div','kbg20-history-row'));
            row(item,node('strong','',h.code));
            row(item,node('span','',h.type === 2 ? 'Tăng ca' : 'Giờ thường'));
            row(item,node('span','',localTime(h.started) + ' → ' + localTime(h.ended)));
            row(item,node('strong','',clock(h.seconds)));
        });
    }
    function refreshUi() {
        if (!state) return;
        renderProjects();renderSummary();renderTasks();renderTimers();renderHistory();
    }
    function refresh() {
        if (loading || acting) return Promise.resolve();
        loading = true;
        var id = projectSelect.value || projectFromUrl();
        var query = id ? '?projectId=' + encodeURIComponent(id) : '';
        return window.fetch(url + query, { credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'} })
            .then(getJson).then(function (data) {
                offset = Number(data.serverNow) - Date.now();
                state = data;
                lastFetch = Date.now();
                renderProjects();refreshUi();
                notify('Dữ liệu đã được đồng bộ.', 'ok');
            }).catch(function (err) {notify(err.message || 'Lỗi kết nối.', 'error');})
            .then(function(){loading=false;});
    }
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
            params.set('projectId',state.projectId);
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
    projectSelect.addEventListener('change',function(){setProjectInUrl(projectSelect.value);refresh();});
    searchEl.addEventListener('input',function(){filter=(searchEl.value||'').trim().toLocaleLowerCase('vi');if(state)renderTasks();});
    document.getElementById('kbg20Reload').addEventListener('click',refresh);
    root.addEventListener('click',function(e){
        var b=e.target.closest('[data-action]');if (!b || !root.contains(b))return;
        if(b.disabled)return;
        sendAction(b.getAttribute('data-action'),b.getAttribute('data-id'));
    });
    window.setInterval(function(){
        if (!state) return;
        var now=serverNow();
        var globalClock=document.getElementById('ksClock');
        if(globalClock) {
            try {globalClock.textContent=new Date(now).toLocaleTimeString('vi-VN',{
                timeZone:'Asia/Ho_Chi_Minh',hour12:false});} catch (_) {}
        }
        var elems=timerEl.querySelectorAll('[data-timer-id]');
        for(var i=0;i<elems.length;i++) {
            var id=elems[i].getAttribute('data-timer-id');
            var t=state.timers.find(function(x){return x.id===id;});
            if(t) elems[i].textContent=clock(liveSeconds(t));
        }
        if(!loading && !acting && Date.now()-lastFetch>30000)refresh();
    },1000);
    refresh();
})(window,document);
