
/* Floating Work Timer: read-only client; server is the source of truth. */
(function (window, document) {
    'use strict';
    if (window.KbgFloatingTimer && window.KbgFloatingTimer.__loaded) return;
    var root = document.getElementById('kbgFloatingTimer');
    if (!root) return;

    var statusUrl = root.getAttribute('data-status-url');
    var userKey = root.getAttribute('data-user-key') || '';
    var broadcastKey = 'kbg-timer-changed:' + userKey;
    var positionKey = 'kbg-float-position:' + userKey;
    var state = null;
    var offsetMs = 0;
    var inFlight = false;
    var queued = false;
    var offline = false;
    var lastShownSeconds = 0;
    var channel = null;
    var expanded = false;
    var fields = {};
    var nodeList = root.querySelectorAll('[data-kbg]');
    for (var i = 0; i < nodeList.length; i++) {
        fields[nodeList[i].getAttribute('data-kbg')] = nodeList[i];
    }
    var pill = document.getElementById('kbgFloatToggle');
    var collapseBtn = document.getElementById('kbgFloatCollapse');
    var details = document.getElementById('kbgFloatDetails');
    var tip = document.getElementById('kbgFloatTip');
    var savedPosition = null;
    var drag = null;
    var blockClickUntil = 0;
    var margin = 12;
    var gap = 10;

    function write(field, value) {
        if (fields[field]) fields[field].textContent = value == null ? '' : String(value);
    }
    function pad(num) { return (num < 10 ? '0' : '') + num; }
    function formatClock(seconds) {
        seconds = Math.max(0, Math.floor(seconds));
        return pad(Math.floor(seconds / 3600)) + ':' + pad(Math.floor((seconds % 3600) / 60)) + ':' + pad(seconds % 60);
    }
    function clamp(value, low, high) { return Math.max(low, Math.min(high, value)); }
    function viewportWidth() { return window.innerWidth || document.documentElement.clientWidth; }
    function viewportHeight() { return window.innerHeight || document.documentElement.clientHeight; }

    /* Save relative position so the timer stays in a sensible place on other screen sizes. */
    try {
        var stored = JSON.parse(window.localStorage.getItem(positionKey) || 'null');
        if (stored && typeof stored.x === 'number' && isFinite(stored.x) &&
            typeof stored.y === 'number' && isFinite(stored.y)) {
            savedPosition = { x: clamp(stored.x, 0, 1), y: clamp(stored.y, 0, 1) };
        }
    } catch (ignoreStorage) { }

    function moveTo(left, top) {
        var vw = viewportWidth();
        var vh = viewportHeight();
        var width = root.offsetWidth;
        var height = root.offsetHeight;
        var maxLeft = Math.max(margin, vw - width - margin);
        var maxTop = Math.max(margin, vh - height - margin);
        root.style.right = 'auto';
        root.style.bottom = 'auto';
        root.style.left = Math.round(clamp(left, margin, maxLeft)) + 'px';
        root.style.top = Math.round(clamp(top, margin, maxTop)) + 'px';
    }
    function rememberPosition() {
        var r = root.getBoundingClientRect();
        var xRange = Math.max(1, viewportWidth() - r.width - 2 * margin);
        var yRange = Math.max(1, viewportHeight() - r.height - 2 * margin);
        savedPosition = {
            x: clamp((r.left - margin) / xRange, 0, 1),
            y: clamp((r.top - margin) / yRange, 0, 1)
        };
        try { window.localStorage.setItem(positionKey, JSON.stringify(savedPosition)); } catch (ignoreSave) { }
    }
    function restorePosition() {
        if (!savedPosition || root.hidden) return;
        var xRange = Math.max(0, viewportWidth() - root.offsetWidth - 2 * margin);
        var yRange = Math.max(0, viewportHeight() - root.offsetHeight - 2 * margin);
        moveTo(margin + savedPosition.x * xRange, margin + savedPosition.y * yRange);
    }

    /* Keep both the expanded card and hover tooltip inside the viewport. */
    function placeOverlays() {
        if (root.hidden) return;
        var r = root.getBoundingClientRect();
        var vw = viewportWidth();
        var vh = viewportHeight();
        if (details) {
            var panelWidth = details.offsetWidth;
            var naturalHeight = details.scrollHeight;
            var above = Math.max(0, r.top - margin - gap);
            var below = Math.max(0, vh - r.bottom - margin - gap);
            var putBelow = below >= above;
            if (naturalHeight <= above && naturalHeight > below) putBelow = false;
            if (naturalHeight <= below && naturalHeight > above) putBelow = true;
            var space = Math.max(0, putBelow ? below : above);
            details.style.maxHeight = Math.max(1, Math.floor(space)) + 'px';
            var height = details.offsetHeight;
            var panelLeft = clamp(r.right - panelWidth, margin, Math.max(margin, vw - panelWidth - margin));
            var panelTop = putBelow ? r.bottom + gap : r.top - gap - height;
            panelTop = clamp(panelTop, margin, Math.max(margin, vh - height - margin));
            details.style.left = Math.round(panelLeft - r.left) + 'px';
            details.style.top = Math.round(panelTop - r.top) + 'px';
            root.classList.toggle('is-details-below', putBelow);
        }
        if (tip) {
            var tipWidth = tip.offsetWidth;
            var tipHeight = tip.offsetHeight;
            var tipAbove = r.top - margin - gap;
            var tipBelow = vh - r.bottom - margin - gap;
            var tipGoesBelow = tipAbove < tipHeight && tipBelow > tipAbove;
            var tipLeft = clamp(r.left + (r.width - tipWidth) / 2, margin,
                Math.max(margin, vw - tipWidth - margin));
            var tipTop = tipGoesBelow ? r.bottom + gap : r.top - gap - tipHeight;
            tipTop = clamp(tipTop, margin, Math.max(margin, vh - tipHeight - margin));
            tip.style.left = Math.round(tipLeft - r.left) + 'px';
            tip.style.top = Math.round(tipTop - r.top) + 'px';
            tip.style.setProperty('--kbg-tip-arrow-x', Math.round(clamp(r.left + r.width / 2 - tipLeft - 6, 14, tipWidth - 26)) + 'px');
            root.classList.toggle('is-tip-below', tipGoesBelow);
        }
    }

    function showDetails(show) {
        expanded = !!show;
        if (expanded && !root.hidden) placeOverlays();
        root.classList.toggle('is-expanded', expanded);
        pill.setAttribute('aria-expanded', expanded ? 'true' : 'false');
        details.setAttribute('aria-hidden', expanded ? 'false' : 'true');
    }
    function hide() {
        state = null;
        offline = false;
        root.classList.remove('is-offline', 'is-break');
        root.hidden = true;
        showDetails(false);
    }
    function segmentsSeconds(from, to, segments) {
        if (!(to > from) || !segments || !segments.length) return 0;
        var seconds = 0;
        for (var j = 0; j < segments.length; j++) {
            var s = segments[j];
            var beginning = Math.max(from, Number(s.start) || 0);
            var finish = Math.min(to, Number(s.end) || 0);
            if (finish > beginning) seconds += (finish - beginning) / 1000;
        }
        return Math.max(0, Math.round(seconds));
    }
    function onBreak(now, segments) {
        if (!segments || segments.length < 2) return false;
        for (var j = 0; j < segments.length - 1; j++) {
            if (now >= Number(segments[j].end) && now < Number(segments[j + 1].start)) return true;
        }
        return false;
    }
    function tick() {
        var toast = document.getElementById('liveToast');
        root.classList.toggle('is-toast-visible', !!(toast && toast.classList.contains('show')));
        if (!state || root.hidden || offline) return;
        var serverNow = Date.now() + offsetMs;
        if (serverNow >= Number(state.dayEnd)) {
            hide(); // The close job may commit later; do not display an expired running timer.
            return;
        }
        var seconds = Number(state.completedSeconds) +
            segmentsSeconds(Number(state.runningSince), Math.min(serverNow, Number(state.dayEnd)), state.segments);
        lastShownSeconds = Math.max(0, seconds);
        var formatted = formatClock(lastShownSeconds);
        write('compact-time', formatted);
        write('detail-time', formatted);
        var resting = onBreak(serverNow, state.segments);
        root.classList.toggle('is-break', resting);
        write('compact-state', resting ? 'ĐANG NGHỈ TRƯA' : 'ĐANG TÍNH GIỜ');
        write('detail-status', resting ? 'Nghỉ trưa' : 'Đang chạy');
    }
    function setSnapshot(s, startTime, finishTime) {
        if (!s || s.running !== true) { hide(); return; }
        var wasHidden = root.hidden;
        state = s;
        offline = false;
        root.classList.remove('is-offline');
        offsetMs = Number(s.serverNow) - (startTime + finishTime) / 2;
        root.hidden = false;
        write('compact-code', s.taskCode || 'Công việc');
        write('task-code', s.taskCode || 'Công việc');
        write('project-code', s.projectCode || 'Dự án');
        write('task-name', s.taskName || 'Công việc đang thực hiện');
        write('project-name', s.projectName || '');
        write('tooltip-code', (s.projectCode || 'Dự án') + ' · ' + (s.taskCode || 'Công việc'));
        write('tooltip-name', s.taskName || 'Công việc đang thực hiện');
        write('tooltip-project', s.projectName || '');
        if (wasHidden) restorePosition();
        tick();
        placeOverlays();
    }
    function onError(status) {
        if (status === 401 || status === 403) { hide(); return; }
        if (state) {
            offline = true;
            root.classList.add('is-offline');
            root.classList.remove('is-break');
            write('compact-state', 'CHƯA ĐỒNG BỘ');
            write('detail-status', 'Mất kết nối');
            write('compact-time', formatClock(lastShownSeconds));
            write('detail-time', formatClock(lastShownSeconds));
        }
    }
    function refresh() {
        if (inFlight) { queued = true; return; }
        inFlight = true;
        var startTime = Date.now();
        var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
        var timeout = controller ? window.setTimeout(function () { controller.abort(); }, 12000) : null;
        var opts = { method: 'GET', credentials: 'same-origin', cache: 'no-store', headers: { 'Accept': 'application/json' } };
        if (controller) opts.signal = controller.signal;
        window.fetch(statusUrl + '?_=' + startTime, opts).then(function (response) {
            if (!response.ok) {
                var problem = new Error('HTTP ' + response.status);
                problem.httpStatus = response.status;
                throw problem;
            }
            return response.json();
        }).then(function (data) {
            setSnapshot(data, startTime, Date.now());
        }).catch(function (error) {
            onError(error && error.httpStatus);
        }).then(function () {
            if (timeout) window.clearTimeout(timeout);
            inFlight = false;
            if (queued) { queued = false; refresh(); }
        });
    }
    function notifyChanged() {
        try { window.localStorage.setItem(broadcastKey, String(Date.now())); } catch (ignore) { }
        if (channel) { try { channel.postMessage('changed'); } catch (ignore2) { } }
        refresh();
    }

    /* A short movement becomes a drag; a normal click still expands the card. */
    pill.addEventListener('pointerdown', function (event) {
        if ((event.button !== undefined && event.button !== 0) ||
            (event.isPrimary !== undefined && !event.isPrimary)) return;
        var r = root.getBoundingClientRect();
        drag = {
            id: event.pointerId,
            startX: event.clientX,
            startY: event.clientY,
            left: r.left,
            top: r.top,
            moved: false
        };
        if (pill.setPointerCapture) {
            try { pill.setPointerCapture(event.pointerId); } catch (ignoreCapture) { }
        }
    });
    pill.addEventListener('pointermove', function (event) {
        if (!drag || drag.id !== event.pointerId) return;
        var dx = event.clientX - drag.startX;
        var dy = event.clientY - drag.startY;
        if (!drag.moved && Math.sqrt(dx * dx + dy * dy) < 5) return;
        drag.moved = true;
        if (event.cancelable) event.preventDefault();
        root.classList.add('is-dragging');
        moveTo(drag.left + dx, drag.top + dy);
        placeOverlays();
    });
    function finishDrag(event) {
        if (!drag || drag.id !== event.pointerId) return;
        if (drag.moved) {
            rememberPosition();
            blockClickUntil = Date.now() + 450;
        }
        root.classList.remove('is-dragging');
        if (pill.releasePointerCapture && pill.hasPointerCapture && pill.hasPointerCapture(event.pointerId)) {
            try { pill.releasePointerCapture(event.pointerId); } catch (ignoreRelease) { }
        }
        drag = null;
        placeOverlays();
    }
    pill.addEventListener('pointerup', finishDrag);
    pill.addEventListener('pointercancel', finishDrag);
    pill.addEventListener('click', function (event) {
        if (Date.now() < blockClickUntil) {
            event.preventDefault();
            event.stopPropagation();
            return;
        }
        showDetails(!expanded);
    });
    pill.addEventListener('pointerenter', placeOverlays);
    collapseBtn.addEventListener('click', function () { showDetails(false); pill.focus(); });
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && expanded) showDetails(false);
    });
    document.addEventListener('visibilitychange', function () { if (!document.hidden) refresh(); });
    window.addEventListener('focus', function () { refresh(); });
    window.addEventListener('resize', function () {
        if (savedPosition && !root.hidden) restorePosition();
        placeOverlays();
    });
    window.addEventListener('storage', function (event) {
        if (event.key === broadcastKey) refresh();
        if (event.key === positionKey) {
            try {
                var v = JSON.parse(event.newValue || 'null');
                if (v && isFinite(v.x) && isFinite(v.y)) {
                    savedPosition = { x: clamp(Number(v.x), 0, 1), y: clamp(Number(v.y), 0, 1) };
                    restorePosition();
                    placeOverlays();
                }
            } catch (ignorePosition) { }
        }
    });
    if (typeof window.BroadcastChannel !== 'undefined') {
        try {
            channel = new window.BroadcastChannel(broadcastKey);
            channel.onmessage = function () { refresh(); };
        } catch (ignore3) { channel = null; }
    }
    window.KbgFloatingTimer = { __loaded: true, refresh: refresh, notifyChanged: notifyChanged };
    window.setInterval(tick, 1000);
    window.setInterval(function () { if (!document.hidden) refresh(); }, 30000);
    refresh();
})(window, document);
