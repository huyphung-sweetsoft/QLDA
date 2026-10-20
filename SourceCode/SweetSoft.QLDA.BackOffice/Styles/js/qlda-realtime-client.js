/* QLDA Realtime - stage 1. CHUA nhung vao MasterTemplate, chua auto-connect. */
(function (window) {
    'use strict';
    var listeners = [], hub = null, connection = null;
    var url = null, connectPromise = null, retryTimer = null, attempt = 0;
    var stopped = false;
    var $ = window.jQuery;

    function dispatch(message) {
        listeners.slice().forEach(function (handler) {
            try { handler(message); } catch (err) {
                if (window.console) window.console.error('QLDA realtime listener', err);
            }
        });
    }

    function reconnectLater() {
        if (stopped || retryTimer != null) return;
        var delay = Math.min(30000, 2000 * Math.pow(2, Math.min(attempt++, 4)));
        retryTimer = window.setTimeout(function () {
            retryTimer = null;
            if (!stopped) start(url);
        }, delay);
    }

    function start(hubUrl) {
        if (hubUrl) url = hubUrl;
        if (!url) url = '/qlda-realtime'; // khi cai tren virtual dir phai truyen ResolveUrl('~/qlda-realtime')
        stopped = false;
        $ = window.jQuery;
        if (!$ || typeof $.hubConnection !== 'function') {
            if (window.console) console.error('QLDA realtime: nap jquery.signalR-2.4.3.min.js sau jQuery truoc.');
            return null;
        }
        if (connectPromise) return connectPromise;
        connection = $.hubConnection(url, { useDefaultPath: false });
        hub = connection.createHubProxy('qldaRealtime');
        hub.on('changed', function (event) { dispatch(event); });
        connection.reconnecting(function () { dispatch({ kind: 'connection-reconnecting' }); });
        connection.reconnected(function () {
            attempt = 0;
            // Quan trong: reconnect => moi trang phai lay lai du lieu SQL chinh thuc.
            dispatch({ kind: 'connection-restored' });
        });
        connection.disconnected(function () {
            connectPromise = null;
            hub = null;
            connection = null;
            dispatch({ kind: 'connection-lost' });
            reconnectLater();
        });
        connectPromise = connection.start();
        connectPromise.done(function () {
            attempt = 0;
            dispatch({ kind: 'connection-restored' });
        });
        connectPromise.fail(function (error) {
            connectPromise = null;
            if (window.console) console.warn('QLDA realtime start failed', error);
            reconnectLater();
        });
        return connectPromise;
    }

    function stop() {
        stopped = true;
        if (retryTimer != null) { clearTimeout(retryTimer); retryTimer = null; }
        if (connection) connection.stop();
        connection = null; hub = null; connectPromise = null;
    }

    function on(handler) {
        if (typeof handler !== 'function') throw new TypeError('handler must be function');
        listeners.push(handler);
        return function () { listeners = listeners.filter(function (x) { return x !== handler; }); };
    }

    function invoke(name) {
        if (!hub) return null;
        return hub.invoke(name);
    }

    window.QldaRealtime = {
        start: start, stop: stop, on: on,
        ping: function () { return invoke('ping'); },
        testSelf: function () { return invoke('testSelf'); },
        testNotificationSelf: function () { return invoke('testNotificationSelf'); },
        getState: function () { return connection ? connection.state : -1; }
    };
})(window);
