/* Floating Timer V2 - independent timer sessions across projects; no SignalR dependency. */
(function (window, document) {
    'use strict';
    if (window.KbgFloatingTimer && window.KbgFloatingTimer.__loaded) return;
    var root = document.getElementById('kbgFloatingTimer');
    if (!root) return;
    var $id = function (id) { return document.getElementById(id); };
    var pill = $id('kbgFloatToggle'), details = $id('kbgFloatDetails'), tip = $id('kbgFloatTip');
    var list = $id('kbgFloatMultiList'), warning = $id('kbgFloatWarning');
    if (!pill || !details || !tip || !list || !warning) return;

    var statusUrl = root.getAttribute('data-status-url');
    var userKey = root.getAttribute('data-user-key') || '';
    var positionKey = 'kbg-float-position:' + userKey;
    var broadcastKey = 'kbg-timer-changed:' + userKey;
    var broadcastChannel = null;
    var timers = [], clockNodes = [], serverNow = 0, clockDelta = 0, snapshotAt = 0;
    var renderedStructure = ''; // Danh sach khong can dung lai DOM khi polling khong doi phien.
    var expanded = false, offline = false, inFlight = false, refreshQueued = false;
    var activeController = null, activeTimeout = null, lastRequest = 0;
    var retryTimer = null, stopped = false, savedPosition = null, drag = null, suppressClickUntil = 0;
    var margin = 12, gap = 10;

    function vw() { return window.innerWidth || document.documentElement.clientWidth; }
    function vh() { return window.innerHeight || document.documentElement.clientHeight; }
    function clamp(n, min, max) { return Math.max(min, Math.min(max, n)); }
    function el(tag, className, value) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (value !== undefined && value !== null) node.textContent = String(value);
        return node;
    }
    function formatTime(seconds) {
        var n = Math.max(0, Math.floor(Number(seconds) || 0));
        function pad(v) { return v < 10 ? '0' + v : String(v); }
        return pad(Math.floor(n / 3600)) + ':' + pad(Math.floor(n / 60) % 60) + ':' + pad(n % 60);
    }
    function text(id, value) { var node = $id(id); if (node) node.textContent = value; }
    function nowServer() { return Date.now() + clockDelta; }
    function isCounting(t, now) {
        return !!t.counting && !(t.boundary != null && now >= t.boundary);
    }
    function displayedSeconds(t, now) {
        if (!t.counting || offline) return t.seconds;
        var capped = t.boundary == null ? now : Math.min(now, t.boundary);
        return Math.max(0, t.seconds + Math.max(0, Math.floor((capped - serverNow) / 1000)));
    }
    function updateLive() {
        if (!timers.length || root.hidden) return;
        var now = nowServer(), activeCount = 0;
        for (var i = 0; i < clockNodes.length; i++) {
            var entry = clockNodes[i];
            var active = !offline && isCounting(entry.data, now);
            if (active) activeCount++;
            entry.number.textContent = formatTime(displayedSeconds(entry.data, now));
            entry.status.textContent = offline ? 'Chưa đồng bộ' : (active ? 'Đang cộng giờ' : 'Tạm ngừng cộng');
            entry.card.classList.toggle('is-counting', active);
            entry.card.classList.toggle('is-not-counting', !active);
        }
        // clockNodes chi luu tung phien, so dem tu node khop voi timer.
        text('kbgFloatCountLive', activeCount + ' đang cộng');
        text('kbgFloatCountPaused', (timers.length - activeCount) + (offline ? ' chưa đồng bộ' : ' tạm ngừng'));
        root.classList.toggle('is-no-active', activeCount === 0);
        root.classList.toggle('is-offline', offline);
        // Pill chi co icon. Thong tin day du van co qua aria-label, tooltip va panel.
        var compactLabel = timers.length === 1
            ? ('Đồng hồ ' + (timers[0].code || 'công việc') + ', ' + (timers[0].type === 2 ? 'tăng ca' : 'giờ thường'))
            : (timers.length + ' phiên giờ, ' + countProjects() + ' dự án');
        pill.setAttribute('aria-label', compactLabel + '. ' + (offline ? 'Chưa đồng bộ.' : activeCount + ' phiên đang cộng giờ.') + ' Nhấn để xem chi tiết.');
        text('kbgFloatTipBody', offline
            ? 'Mất kết nối tạm thời. Thời gian đang được giữ tại lần cập nhật gần nhất.'
            : (timers.length === 1
                ? ((timers[0].code || 'Công việc') + ' · ' + (timers[0].type === 2 ? 'Tăng ca' : 'Giờ thường') + ' · ' + formatTime(displayedSeconds(timers[0], now)))
                : (countProjects() + ' dự án · ' + activeCount + ' phiên đang cộng. Bấm để xem chi tiết.')));
    }
    function countProjects() {
        var ids = Object.create(null), count = 0;
        timers.forEach(function (t) {
            if (!ids[t.projectId]) { ids[t.projectId] = true; count++; }
        });
        return count;
    }
    function projectLink(url) {
        // URL duoc tao trong handler, chi cho phep duong dan noi bo.
        return typeof url === 'string' && url.charAt(0) === '/' && url.charAt(1) !== '/' && url.charAt(1) !== '\\' && !/[\r\n]/.test(url) ? url : null;
    }
    function renderList() {
        clockNodes = [];
        while (list.firstChild) list.removeChild(list.firstChild);
        var groups = Object.create(null), order = [];
        timers.forEach(function (t) {
            var key = t.projectId;
            if (!groups[key]) { groups[key] = []; order.push(key); }
            groups[key].push(t);
        });
        order.sort(function (a, b) {
            var x = groups[a][0], y = groups[b][0];
            return (x.projectCode || x.projectName || '').localeCompare(y.projectCode || y.projectName || '', 'vi');
        });
        order.forEach(function (key) {
            var entries = groups[key], head = entries[0];
            var group = el('section', 'kbg-float-project');
            var heading = el('div', 'kbg-float-project-head');
            var nameWrap = el('div', 'kbg-float-project-title');
            var folderIcon = el('span', 'kbg-float-folder');
            folderIcon.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" stroke-linecap="round" aria-hidden="true"><path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v10H3Z"/><path d="M3 10h18"/></svg>';
            nameWrap.appendChild(folderIcon);
            var nameText = el('div', 'kbg-float-project-label');
            nameText.appendChild(el('strong', '', head.projectCode || 'Dự án'));
            if (head.projectName) nameText.appendChild(el('span', '', head.projectName));
            nameWrap.appendChild(nameText);
            heading.appendChild(nameWrap);
            var href = projectLink(head.projectUrl);
            if (href) {
                var open = el('a', 'kbg-float-open', 'Mở trang ↗');
                open.setAttribute('href', href);
                open.setAttribute('title', 'Đến trang Tính giờ của dự án ' + (head.projectCode || ''));
                heading.appendChild(open);
            }
            group.appendChild(heading);
            entries.sort(function (a, b) {
                var task = (a.code || '').localeCompare(b.code || '', 'vi', { numeric: true });
                return task || a.type - b.type || a.id.localeCompare(b.id);
            });
            entries.forEach(function (t) {
                var card = el('article', 'kbg-float-task');
                var top = el('div', 'kbg-float-task-top');
                var label = el('div', 'kbg-float-task-name');
                label.appendChild(el('strong', '', t.code || 'Công việc'));
                label.appendChild(el('span', '', t.name || 'Chưa có tên công việc'));
                top.appendChild(label);
                var type = el('span', t.type === 2 ? 'kbg-float-type is-overtime' : 'kbg-float-type', t.type === 2 ? 'Tăng ca' : 'Giờ thường');
                top.appendChild(type);
                card.appendChild(top);
                var bottom = el('div', 'kbg-float-task-bottom');
                var number = el('strong', 'kbg-float-task-clock', formatTime(t.seconds));
                number.setAttribute('title', 'Số giây của phiên này, không phải tổng toàn bộ công việc');
                var status = el('span', 'kbg-float-task-status', 'Đang đồng bộ');
                bottom.appendChild(number);
                bottom.appendChild(status);
                card.appendChild(bottom);
                if (t.source === 2) card.appendChild(el('span', 'kbg-float-auto', 'Tự bắt đầu từ công việc con'));
                group.appendChild(card);
                clockNodes.push({ data: t, number: number, status: status, card: card });
            });
            list.appendChild(group);
        });
    }
    function structureSignature(rows) {
        // Chi metadata thay doi moi can tao lai cards; seconds/counting duoc cap nhat tren node cu.
        return JSON.stringify(rows.map(function (t) {
            return [t.id, t.projectId, t.taskId, t.projectCode, t.projectName,
                t.projectUrl, t.code, t.name, t.type, t.source, t.overdue];
        }));
    }
    function syncRenderedList() {
        var signature = structureSignature(timers);
        if (signature !== renderedStructure) {
            var scrollBox = $id('kbgFloatScroll');
            var oldScroll = scrollBox ? scrollBox.scrollTop : 0;
            renderList();
            renderedStructure = signature;
            if (scrollBox) scrollBox.scrollTop = oldScroll;
        } else {
            var byId = Object.create(null);
            timers.forEach(function (t) { byId[t.id] = t; });
            clockNodes.forEach(function (entry) { entry.data = byId[entry.data.id] || entry.data; });
        }
    }
    function showDetails(isOpen) {
        expanded = !!isOpen;
        root.classList.toggle('is-expanded', expanded);
        pill.setAttribute('aria-expanded', String(expanded));
        details.setAttribute('aria-hidden', String(!expanded));
        if (expanded) { placeOverlays(); }
    }
    function hide() {
        root.hidden = true;
        timers = []; clockNodes = []; offline = false; renderedStructure = '';
        showDetails(false);
    }
    function applySnapshot(data, sent, received) {
        if (!data || data.ok !== true || !Array.isArray(data.timers)) throw new Error('Invalid V2 snapshot');
        if (!data.timers.length) { hide(); return; }
        if (data.running === false) throw new Error('Inconsistent V2 snapshot');
        var unique = Object.create(null), next = [];
        data.timers.forEach(function (t) {
            if (!t || !t.id || !t.projectId || unique[t.id]) return;
            unique[t.id] = true;
            next.push({
                id: String(t.id), taskId: String(t.taskId || ''), projectId: String(t.projectId),
                code: String(t.code || ''), name: String(t.name || ''),
                projectCode: String(t.projectCode || ''), projectName: String(t.projectName || ''),
                projectUrl: t.projectUrl, type: Number(t.type) === 2 ? 2 : 1,
                source: Number(t.source) || 1, overdue: !!t.overdue,
                seconds: Math.max(0, Number(t.seconds) || 0), counting: t.counting === true,
                boundary: t.boundary == null ? null : Number(t.boundary)
            });
        });
        if (!next.length) { hide(); return; }
        timers = next;
        serverNow = Number(data.serverNow) || received;
        clockDelta = serverNow - (sent + received) / 2;
        snapshotAt = received;
        offline = false;
        warning.hidden = true;
        var wasHidden = root.hidden;
        root.hidden = false;
        text('kbgFloatCount', timers.length + ' phiên · ' + countProjects() + ' dự án');
        text('kbgFloatTipTitle', timers.length + ' phiên giờ đang mở');
        text('kbgFloatTipBody', countProjects() + ' dự án. Bấm để xem từng công việc và giờ thường/tăng ca.');
        syncRenderedList();
        updateLive();
        if (wasHidden) restorePosition();
        placeOverlays();
    }
    function requestRefresh(force) {
        if (stopped || !statusUrl) return;
        if (document.hidden && !force) return;
        if (inFlight) { refreshQueued = true; return; }
        // Gom cac su kien focus/visibility kich hoat sat nhau.
        if (!force && Date.now() - lastRequest < 1200) return;
        inFlight = true;
        lastRequest = Date.now();
        var sent = Date.now();
        var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
        activeController = controller;
        activeTimeout = controller ? window.setTimeout(function () { controller.abort(); }, 12000) : null;
        var options = { method: 'GET', credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json' } };
        if (controller) options.signal = controller.signal;
        window.fetch(statusUrl + '?_=' + sent, options)
            .then(function (response) {
                if (!response.ok) {
                    var problem = new Error('HTTP ' + response.status);
                    problem.status = response.status;
                    throw problem;
                }
                return response.json();
            })
            .then(function (data) { if (!stopped) applySnapshot(data, sent, Date.now()); })
            .catch(function (error) {
                if (stopped) return;
                if (error && (error.status === 401 || error.status === 403)) { hide(); return; }
                if (timers.length) {
                    var atFailure = nowServer();
                    timers.forEach(function (t) { t.seconds = displayedSeconds(t, atFailure); });
                    offline = true;
                    warning.hidden = false;
                    updateLive();
                }
            })
            .then(function () {
                if (activeTimeout) window.clearTimeout(activeTimeout);
                activeController = null; activeTimeout = null;
                inFlight = false;
                if (refreshQueued && !stopped) {
                    refreshQueued = false;
                    // Mot lan gui lai duoc cho phep khi co thay doi trong luc fetch.
                    requestRefresh(true);
                }
            });
    }
    function notifyChanged() {
        try { window.localStorage.setItem(broadcastKey, String(Date.now())); } catch (ignoreStorage) { }
        if (broadcastChannel) { try { broadcastChannel.postMessage('changed'); } catch (ignoreChannel) { } }
        requestRefresh(true);
    }

    var dragStart = null, lastSaved = null;
    try {
        lastSaved = JSON.parse(window.localStorage.getItem(positionKey) || 'null');
        if (lastSaved && isFinite(lastSaved.x) && isFinite(lastSaved.y))
            savedPosition = { x: clamp(Number(lastSaved.x), 0, 1), y: clamp(Number(lastSaved.y), 0, 1) };
    } catch (ignorePosition) { }
    function moveTo(left, top) {
        var width = root.offsetWidth, height = root.offsetHeight;
        root.style.right = 'auto'; root.style.bottom = 'auto';
        root.style.left = Math.round(clamp(left, margin, Math.max(margin, vw() - width - margin))) + 'px';
        root.style.top = Math.round(clamp(top, margin, Math.max(margin, vh() - height - margin))) + 'px';
    }
    function rememberPosition() {
        if (root.hidden) return;
        var r = root.getBoundingClientRect();
        savedPosition = {
            x: clamp((r.left - margin) / Math.max(1, vw() - r.width - margin * 2), 0, 1),
            y: clamp((r.top - margin) / Math.max(1, vh() - r.height - margin * 2), 0, 1)
        };
        try { window.localStorage.setItem(positionKey, JSON.stringify(savedPosition)); } catch (ignore) { }
    }
    function restorePosition() {
        if (!savedPosition || root.hidden) return;
        moveTo(margin + savedPosition.x * Math.max(0, vw() - root.offsetWidth - 2 * margin),
            margin + savedPosition.y * Math.max(0, vh() - root.offsetHeight - 2 * margin));
    }
    function placeOverlays() {
        if (root.hidden) return;
        var r = root.getBoundingClientRect(), above = Math.max(0, r.top - gap - margin);
        var below = Math.max(0, vh() - r.bottom - gap - margin);
        if (details) {
            var panelW = details.offsetWidth;
            // scrollHeight tu tinh du khi chua expanded, khong lam thay doi kich thuoc pill.
            var heightWanted = details.scrollHeight;
            var goesBelow = below > above;
            if (heightWanted <= above && heightWanted > below) goesBelow = false;
            if (heightWanted <= below && heightWanted > above) goesBelow = true;
            var available = Math.max(80, goesBelow ? below : above);
            details.style.maxHeight = Math.floor(available) + 'px';
            var panelH = details.offsetHeight;
            var left = clamp(r.right - panelW, margin, Math.max(margin, vw() - panelW - margin));
            var top = goesBelow ? r.bottom + gap : r.top - gap - panelH;
            top = clamp(top, margin, Math.max(margin, vh() - panelH - margin));
            details.style.left = Math.round(left - r.left) + 'px';
            details.style.top = Math.round(top - r.top) + 'px';
            root.classList.toggle('is-details-below', goesBelow);
        }
        if (tip) {
            var w = tip.offsetWidth, h = tip.offsetHeight;
            var tipBelow = r.top - gap - margin < h && below >= above;
            var tipL = clamp(r.left + (r.width - w) / 2, margin, Math.max(margin, vw() - w - margin));
            var tipT = tipBelow ? r.bottom + gap : r.top - gap - h;
            tipT = clamp(tipT, margin, Math.max(margin, vh() - h - margin));
            tip.style.left = Math.round(tipL - r.left) + 'px';
            tip.style.top = Math.round(tipT - r.top) + 'px';
            tip.style.setProperty('--kbg-tip-arrow-x', Math.round(clamp(r.left + r.width / 2 - tipL - 5, 14, w - 26)) + 'px');
            root.classList.toggle('is-tip-below', tipBelow);
        }
    }
    pill.addEventListener('pointerdown', function (event) {
        if ((event.button !== undefined && event.button !== 0) || (event.isPrimary !== undefined && !event.isPrimary)) return;
        var r = root.getBoundingClientRect();
        drag = { id: event.pointerId, x: event.clientX, y: event.clientY, left: r.left, top: r.top, moved: false };
        if (pill.setPointerCapture) { try { pill.setPointerCapture(event.pointerId); } catch (ignore) { } }
    });
    pill.addEventListener('pointermove', function (event) {
        if (!drag || drag.id !== event.pointerId) return;
        var dx = event.clientX - drag.x, dy = event.clientY - drag.y;
        if (!drag.moved && Math.sqrt(dx * dx + dy * dy) < 5) return;
        drag.moved = true;
        if (event.cancelable) event.preventDefault();
        root.classList.add('is-dragging');
        moveTo(drag.left + dx, drag.top + dy);
        placeOverlays();
    });
    function endDrag(event) {
        if (!drag || drag.id !== event.pointerId) return;
        if (drag.moved) { rememberPosition(); suppressClickUntil = Date.now() + 450; }
        root.classList.remove('is-dragging');
        if (pill.releasePointerCapture && pill.hasPointerCapture && pill.hasPointerCapture(event.pointerId)) {
            try { pill.releasePointerCapture(event.pointerId); } catch (ignore) { }
        }
        drag = null; placeOverlays();
    }
    pill.addEventListener('pointerup', endDrag);
    pill.addEventListener('pointercancel', endDrag);
    pill.addEventListener('click', function (event) {
        if (Date.now() < suppressClickUntil) { event.preventDefault(); event.stopPropagation(); return; }
        showDetails(!expanded);
    });
    pill.addEventListener('pointerenter', placeOverlays);
    var close = $id('kbgFloatCollapse');
    if (close) close.addEventListener('click', function () { showDetails(false); pill.focus(); });
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape' && expanded) showDetails(false); });
    window.addEventListener('resize', function () { restorePosition(); placeOverlays(); });
    document.addEventListener('visibilitychange', function () { if (!document.hidden) requestRefresh(false); });
    window.addEventListener('focus', function () { requestRefresh(false); });
    window.addEventListener('storage', function (event) {
        if (event.key === broadcastKey) requestRefresh(true);
        if (event.key === positionKey) {
            try {
                var p = JSON.parse(event.newValue || 'null');
                if (p && isFinite(p.x) && isFinite(p.y)) {
                    savedPosition = { x: clamp(Number(p.x), 0, 1), y: clamp(Number(p.y), 0, 1) };
                    restorePosition(); placeOverlays();
                }
            } catch (ignore) { }
        }
    });
    if (typeof window.BroadcastChannel !== 'undefined') {
        try {
            broadcastChannel = new window.BroadcastChannel(broadcastKey);
            broadcastChannel.onmessage = function () { requestRefresh(true); };
        } catch (ignore) { }
    }
    window.addEventListener('pagehide', function () {
        stopped = true;
        if (activeController) activeController.abort();
        if (activeTimeout) window.clearTimeout(activeTimeout);
        if (broadcastChannel) broadcastChannel.close();
    });
    window.addEventListener('pageshow', function (event) {
        // BFCache: pageshow co the goi lai cung mot window da pagehide.
        if (event.persisted) {
            stopped = false;
            if (typeof window.BroadcastChannel !== 'undefined') {
                try { broadcastChannel = new window.BroadcastChannel(broadcastKey); broadcastChannel.onmessage = function () { requestRefresh(true); }; } catch (ignore) { }
            }
            requestRefresh(true);
        }
    });
    window.KbgFloatingTimer = { __loaded: true, refresh: function () { requestRefresh(true); }, notifyChanged: notifyChanged };
    window.setInterval(updateLive, 1000);
    window.setInterval(function () { if (!document.hidden && !stopped) requestRefresh(false); }, 30000);
    requestRefresh(true);
})(window, document);
