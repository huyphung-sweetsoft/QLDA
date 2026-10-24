/* Dùng chung ở CtrlShareProgress và CtrlProgressReportHistory. */
(function (w) {
        'use strict';
        // Cả hai ASCX có thể cùng nhúng script. Không đăng ký event/PRM hai lần.
        if (w.__qldaProgressReportSharedInitialized) return;
        w.__qldaProgressReportSharedInitialized = true;
        var busy = false;
        var savePending = false;
        var historyPagingPending = false;
        var historySearchInFlight = false;
        var historySearchQueued = false;
        var historySearchLastSent = null;
        var historySearchLastCompleted = null;
        var historySearchComposing = false;
        var historyResetPending = false;
        var historyResetReportId = '';
        var historyDeletePending = false;
        var historyDeleteReportId = '';
        var savedSelection = null;
        var previousTaskList = null;
        function selectedIds() { var el = root(); if (!el) return []; return Array.prototype.map.call(el.querySelectorAll('input[name="shareProgressTaskIds"]:checked'), function (x) { return x.value; }).sort(); }
        function selectedSignature() { return JSON.stringify(selectedIds()); }
        function saveButton() { return document.getElementById("shareProgressSavePresetClient"); }
        function refreshSaveButton() { var el = root(); var btn = saveButton(); if (!el || !btn) return; if (el !== previousTaskList || savedSelection === null) { previousTaskList = el; savedSelection = selectedSignature(); } var dirty = selectedSignature() !== savedSelection; var disabled = !dirty || !selectedIds().length || savePending || busy; btn.classList.toggle("is-disabled", disabled); btn.setAttribute("aria-disabled", disabled ? "true" : "false"); var undo = document.getElementById("shareProgressUndoClient"); if (undo) { undo.classList.toggle("is-disabled", !dirty || savePending || busy); undo.setAttribute("aria-disabled", dirty && !savePending && !busy ? "false" : "true"); } }
        function acceptedSave(message) { savePending = false; savedSelection = selectedSignature(); var selector = getModal("modalSelection"); var label = selector ? selector.querySelector("[id$=\"lblPresetStatus\"]") : null; if (label) label.textContent = "Đã lưu cấu hình của dự án. Lần sau mở sẽ tự chọn đúng các công việc này."; refreshSaveButton(); showSaved(message); }
        function getModal(suffix) { return document.querySelector('[id$="' + suffix + '"]'); }
        function getField(suffix) { return document.querySelector('input[id$="' + suffix + '"]'); }
        // Quan trong: luon dung API Bootstrap de dong popup. Khong tu xoa class show / display.
        // Neu CSS/Bootstrap da bi lech trang thai tu phien ban cu, tai tao instance an toan.
        var modalSwitchPending = false;
        function modalShow(el) {
            if (!el) return false;
            try {
                if (w.bootstrap && w.bootstrap.Modal) {
                    var instance = w.bootstrap.Modal.getInstance(el);
                    if (instance && !el.classList.contains('show') && instance._isShown === true) {
                        instance.dispose();
                        instance = null;
                    }
                    if (!instance) instance = w.bootstrap.Modal.getOrCreateInstance(el);
                    instance.show();
                    return true;
                }
                if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) { w.jQuery(el).modal('show'); return true; }
            } catch (e) { if (w.console) console.error(e); }
            return false;
        }
        function modalHide(el) {
            if (!el) return;
            try {
                if (w.bootstrap && w.bootstrap.Modal) {
                    var instance = w.bootstrap.Modal.getInstance(el);
                    if (instance) instance.hide();
                    return;
                }
                if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) w.jQuery(el).modal('hide');
            } catch (e) { if (w.console) console.error(e); }
        }
        // Cho su kien dong modal ket thuc roi moi mo modal khac (Bootstrap co animation).
        // Lang nghe ca su kien native (Bootstrap 5) va jQuery (Bootstrap 4).
        function closeModalThen(el, next) {
            if (!el || !el.classList.contains('show')) { next(); return; }
            var finished = false;
            function finish() {
                if (finished) return;
                finished = true;
                el.removeEventListener('hidden.bs.modal', finish);
                if (w.jQuery && w.jQuery.fn) w.jQuery(el).off('hidden.bs.modal', finish);
                next();
            }
            el.addEventListener('hidden.bs.modal', finish);
            if (w.jQuery && w.jQuery.fn) w.jQuery(el).one('hidden.bs.modal', finish);
            modalHide(el);
            // Fallback cho theme khong phat hidden event, sau khi transition thong thuong ket thuc.
            w.setTimeout(finish, 700);
        }
        function loading(show, mode) { var el = document.getElementById('shareProgressInlineLoader'); if (!el && show) { el = document.createElement('div'); el.id = 'shareProgressInlineLoader'; el.className = 'share-progress-loading-overlay'; el.style.display = 'none'; el.setAttribute('aria-live', 'polite'); el.setAttribute('aria-busy', 'true'); el.innerHTML = '<div class="share-progress-loading-card"><div class="share-progress-spinner"></div><div id="shareProgressLoadingTitle" class="share-progress-loading-title"></div><div id="shareProgressLoadingText" class="share-progress-loading-text"></div><div class="share-progress-loading-track"><span></span></div></div>'; document.body.appendChild(el); } if (!el) return; var title = document.getElementById('shareProgressLoadingTitle'); var text = document.getElementById('shareProgressLoadingText'); if (title && text) { if (mode === 'delete-report') { title.textContent = 'Đang xóa báo cáo'; text.textContent = 'Đang thu hồi liên kết và xóa báo cáo khỏi nhật ký. Vui lòng chờ trong giây lát...'; } else if (mode === 'reset-otp') { title.textContent = 'Đang cấp lại mã OTP'; text.textContent = 'Đang tạo mã OTP mới cho báo cáo và cập nhật nhật ký. Vui lòng chờ trong giây lát...'; } else { title.textContent = 'Đang tạo báo cáo tiến độ'; text.textContent = 'Đang dựng biểu đồ Gantt và bảo vệ báo cáo bằng mã OTP. Vui lòng chờ trong giây lát...'; } } el.style.display = show ? 'flex' : 'none'; }
        function showError(message) { message = message || 'Tạo báo cáo không thành công. Vui lòng thử lại.'; if (w.toastr && typeof w.toastr.error === 'function') { w.toastr.error(message); return; } var old = document.getElementById('shareProgressErrorToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressErrorToast'; toast.className = 'share-progress-error-toast'; toast.setAttribute('role', 'alert'); var icon = document.createElement('i'); icon.className = 'fas fa-exclamation-circle'; var span = document.createElement('span'); span.textContent = message; var x = document.createElement('button'); x.type = 'button'; x.textContent = '×'; x.setAttribute('aria-label', 'Đóng thông báo lỗi'); x.onclick = function () { toast.remove(); }; toast.appendChild(icon); toast.appendChild(span); toast.appendChild(x); document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 8500); }
        function showSaved(message) { message = message || 'Đã lưu cấu hình công việc cho dự án này.'; if (w.toastr && typeof w.toastr.success === 'function') { w.toastr.success(message); return; } var old = document.getElementById('shareProgressSavedToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressSavedToast'; toast.className = 'share-progress-saved-toast'; toast.setAttribute('role', 'status'); toast.textContent = message; document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 3500); }
        function root() { return document.getElementById('shareProgressTaskList'); }
        function updateTree() { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var byCode = {}; rows.forEach(function (row) { byCode[row.getAttribute('data-sp-code')] = row; }); rows.forEach(function (row) { var parent = row.getAttribute('data-sp-parent'); var visible = true; while (parent) { var p = byCode[parent]; if (p) { if (p.hidden || p.getAttribute('data-sp-expanded') !== 'true') { visible = false; break; } } var dot = parent.lastIndexOf('.'); parent = dot < 0 ? '' : parent.substring(0, dot); } row.hidden = !visible; }); }
        function toggleRow(code) { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var row = null; rows.forEach(function (r) { if (r.getAttribute('data-sp-code') === code) row = r; }); if (!row) return; var now = row.getAttribute('data-sp-expanded') !== 'true'; row.setAttribute('data-sp-expanded', now ? 'true' : 'false'); var b = row.querySelector('.share-progress-tree-toggle'); if (b) b.setAttribute('aria-expanded', now ? 'true' : 'false'); updateTree(); }
        function onResult(kind, message) {
            if (kind === 'saved') { acceptedSave(message); return; }
            if (kind === 'saveError') { savePending = false; refreshSaveButton(); showError(message || 'Không thể lưu cấu hình.'); return; }
            savePending = false; busy = false; loading(false);
            var selector = getModal('modalSelection');
            if (kind === 'success') { var nameField = document.getElementById('shareProgressReportName'); if (nameField) { nameField.value = makeDefaultName(nameField.getAttribute('data-project-name') || 'Dự án'); nameField.setAttribute('data-name-reset', '1'); } }
            if (kind !== 'success') {
                // Tao that bai thi giu popup chon cong viec, khong can dong/mo lai.
                if (selector && !selector.classList.contains('show')) modalShow(selector);
                showError(message);
                updateTree(); refreshSaveButton();
                return;
            }
            // Tao thanh cong: dong popup chon bang API Bootstrap, cho hidden roi mo popup ket qua.
            // Khong chen tay vao class/style/backdrop vi se lam lech state Bootstrap.
            modalSwitchPending = true;
            closeModalThen(selector, function () {
                modalSwitchPending = false;
                if (!modalShow(getModal('modalCreated')))
                    showError('Đã tạo báo cáo nhưng không mở được popup kết quả. Hãy kiểm tra thư viện Bootstrap.');
                updateTree(); refreshSaveButton();
            });
        }
        function consumeServerOutcome() { var result = getField('hdfShareProgressOutcome'); if (!result || !result.value) return false; var kind = result.value; result.value = ''; var msg = getField('hdfShareProgressErrorText'); var message = msg ? msg.value : ''; if (msg) msg.value = ''; onResult(kind, message); return true; }
        // AUTOCOMPLETE: GÕ = gợi ý CLIENT-SIDE, KHÔNG gọi server hoặc làm mới UpdatePanel.
        // Chỉ khi chọn gợi ý / Enter / nhấn nút Tìm mới bắt đầu tìm kiếm server.
        var historySuggestActive = -1;
        function historySearchInput() {
            var modal = getModal('modalProgressHistory');
            return modal ? modal.querySelector('input[id$="txtHistorySearch"]') : null;
        }
        function historySearchContainer() {
            var modal = getModal('modalProgressHistory');
            return modal ? modal.querySelector('.share-progress-history-search') : null;
        }
        function historySuggestionBox() {
            var modal = getModal('modalProgressHistory');
            return modal ? modal.querySelector('#shareProgressHistorySuggestions') : null;
        }
        function normalizeHistoryText(value) {
            var valueText = String(value == null ? '' : value).trim().toLowerCase();
            if (valueText.normalize) valueText = valueText.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            return valueText.replace(/đ/g, 'd');
        }
        function historyGetCatalog() {
            var modal = getModal('modalProgressHistory');
            var field = modal ? modal.querySelector('input[id$="hdfHistorySearchCatalog"]') : null;
            if (!field || !field.value) return [];
            try {
                var data = JSON.parse(field.value);
                return Array.isArray(data) ? data : [];
            } catch (e) { return []; }
        }
        function historySearchBusy(isBusy) {
            var node = historySearchContainer();
            if (!node) return;
            node.classList.toggle('is-searching', !!isBusy);
            node.setAttribute('aria-busy', isBusy ? 'true' : 'false');
        }
        function historyCloseSuggestions() {
            var box = historySuggestionBox();
            var input = historySearchInput();
            historySuggestActive = -1;
            if (box) { box.hidden = true; box.textContent = ''; }
            if (input) { input.setAttribute('aria-expanded', 'false'); input.removeAttribute('aria-activedescendant'); }
        }
        function historyPaintActiveSuggestion() {
            var box = historySuggestionBox();
            var input = historySearchInput();
            if (!box || !input) return;
            var choices = box.querySelectorAll('[data-history-suggestion]');
            choices.forEach(function (choice, i) {
                var active = i === historySuggestActive;
                choice.classList.toggle('is-active', active);
                choice.setAttribute('aria-selected', active ? 'true' : 'false');
            });
            if (historySuggestActive >= 0 && choices[historySuggestActive]) {
                input.setAttribute('aria-activedescendant', choices[historySuggestActive].id);
                choices[historySuggestActive].scrollIntoView({ block: 'nearest' });
            } else input.removeAttribute('aria-activedescendant');
        }
        function historyShowSuggestions() {
            var input = historySearchInput();
            var box = historySuggestionBox();
            if (!input || !box) return;
            var term = normalizeHistoryText(input.value);
            historySuggestActive = -1;
            box.textContent = '';
            input.removeAttribute('aria-activedescendant');
            if (!term) { historyCloseSuggestions(); return; }

            var catalog = historyGetCatalog();
            var matches = catalog.filter(function (entry) {
                return entry && normalizeHistoryText(entry.name).indexOf(term) !== -1;
            });
            var header = document.createElement('div');
            header.className = 'share-progress-history-suggestion-head';
            header.textContent = matches.length ? matches.length + ' báo cáo phù hợp · Chọn để xem' : 'Không tìm thấy tên báo cáo phù hợp';
            box.appendChild(header);
            matches.slice(0, 8).forEach(function (entry, index) {
                var option = document.createElement('button');
                option.type = 'button';
                option.id = 'spHistorySuggestion' + index;
                option.className = 'share-progress-history-suggestion-item';
                option.setAttribute('role', 'option');
                option.setAttribute('aria-selected', 'false');
                option.setAttribute('data-history-suggestion', '1');
                var name = document.createElement('span');
                name.className = 'share-progress-history-suggestion-name';
                name.textContent = entry.name || '';
                var date = document.createElement('small');
                date.textContent = entry.date || '';
                option.appendChild(name);
                option.appendChild(date);
                // Prevent textbox blur before the option click is handled.
                option.addEventListener('mousedown', function (e) { e.preventDefault(); });
                option.addEventListener('click', function () {
                    input.value = entry.name || '';
                    historyCloseSuggestions();
                    dispatchHistorySearch(true);
                });
                box.appendChild(option);
            });
            if (matches.length > 8) {
                var more = document.createElement('div');
                more.className = 'share-progress-history-suggestion-foot';
                more.textContent = 'Còn ' + (matches.length - 8) + ' kết quả · Nhấn Enter để xem tất cả';
                box.appendChild(more);
            }
            box.hidden = false;
            input.setAttribute('aria-expanded', 'true');
        }
        function historySearchManager() {
            if (!(w.Sys && w.Sys.WebForms && w.Sys.WebForms.PageRequestManager)) return null;
            return w.Sys.WebForms.PageRequestManager.getInstance();
        }
        function dispatchHistorySearch(force) {
            var input = historySearchInput();
            if (!input || !historyIsShown() || historySearchComposing) return false;
            historyCloseSuggestions();
            var prm = historySearchManager();
            if (historySearchInFlight || (prm && prm.get_isInAsyncPostBack()) ||
                historyPagingPending || historyResetPending || historyDeletePending || busy || savePending || modalSwitchPending) {
                // Only queue explicitly confirmed searches, NEVER raw typing.
                if (force) historySearchQueued = true;
                return false;
            }
            var modal = getModal('modalProgressHistory');
            var trigger = modal ? modal.querySelector('a[id$="btnHistorySearch"]') : null;
            if (!trigger) { showError('Không tìm thấy nút AJAX tìm kiếm báo cáo.'); return false; }
            historySearchLastSent = input.value;
            historySearchQueued = false;
            historySearchInFlight = true;
            historySearchBusy(true);
            try { trigger.click(); }
            catch (err) { historySearchInFlight = false; historySearchBusy(false); showError('Không gửi được yêu cầu tìm kiếm báo cáo.'); }
            return false;
        }
        function finishHistorySearch() {
            var rerun = historySearchQueued;
            historySearchInFlight = false;
            historySearchLastCompleted = historySearchLastSent;
            historySearchQueued = false;
            historySearchBusy(false);
            historyCloseSuggestions();
            if (rerun) w.setTimeout(function () { dispatchHistorySearch(true); }, 0);
        }
        w.shareProgressHistorySearchNow = function (event) {
            if (event && event.preventDefault) event.preventDefault();
            dispatchHistorySearch(true);
            return false;
        };
        document.addEventListener('input', function (event) {
            if (event.target !== historySearchInput() || historySearchComposing || event.isComposing) return;
            historyShowSuggestions(); // KHÔNG trigger.click(), KHÔNG UpdatePanel.
        });
        document.addEventListener('compositionstart', function (event) {
            if (event.target === historySearchInput()) historySearchComposing = true;
        });
        document.addEventListener('compositionend', function (event) {
            if (event.target !== historySearchInput()) return;
            historySearchComposing = false;
            historyShowSuggestions();
        });
        document.addEventListener('keydown', function (event) {
            if (event.target !== historySearchInput() || event.isComposing || historySearchComposing || event.keyCode === 229) return;
            if (event.key === 'Escape') { historyCloseSuggestions(); return; }
            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                var box = historySuggestionBox();
                if (!box || box.hidden) { historyShowSuggestions(); box = historySuggestionBox(); }
                var options = box ? box.querySelectorAll('[data-history-suggestion]') : [];
                if (!options.length) return;
                event.preventDefault();
                historySuggestActive = event.key === 'ArrowDown' ? Math.min(options.length - 1, historySuggestActive + 1) :
                    (historySuggestActive <= 0 ? options.length - 1 : historySuggestActive - 1);
                historyPaintActiveSuggestion();
                return;
            }
            if (event.key === 'Enter') {
                event.preventDefault();
                event.stopPropagation(); // Không để WebForms DefaultButton tự submit form.
                var suggestionBox = historySuggestionBox();
                var options2 = suggestionBox && !suggestionBox.hidden ? suggestionBox.querySelectorAll('[data-history-suggestion]') : [];
                if (historySuggestActive >= 0 && options2[historySuggestActive]) {
                    options2[historySuggestActive].click();
                } else dispatchHistorySearch(true);
            }
        }, true);
        document.addEventListener('click', function (event) {
            var input = historySearchInput();
            var box = historySuggestionBox();
            if (input && box && event.target !== input && !box.contains(event.target)) historyCloseSuggestions();
        });
        document.addEventListener('focusin', function (event) {
            if (event.target === historySearchInput() && event.target.value.trim()) historyShowSuggestions();
        });
        function onRequestError(args) {
            var wasSearching = historySearchInFlight;
            var wasPaging = historyPagingPending;
            var wasDeleting = historyDeletePending;
            var wasSaving = savePending;
            var wasResetting = historyResetPending;
            historyPagingPending = false; historyDeletePending = false; historyDeleteReportId = '';
            historySearchInFlight = false; historySearchQueued = false;
            historySearchBusy(false);
            savePending = false; historyResetPending = false; historyResetReportId = '';
            busy = false; loading(false);
            var message = wasSearching ? 'Không thể tìm kiếm báo cáo. Vui lòng thử lại.' :
                (wasPaging ? 'Không thể chuyển trang nhật ký. Vui lòng thử lại.' :
                (wasDeleting ? 'Không thể xóa báo cáo. Vui lòng thử lại.' :
                (wasSaving ? 'Không thể lưu cấu hình. Vui lòng thử lại.' :
                (wasResetting ? 'Không thể cấp lại OTP. Vui lòng thử lại.' : 'Máy chủ gặp lỗi khi tạo báo cáo. Vui lòng thử lại.'))));
            try { if (args && args.get_error && args.get_error()) message += ' ' + args.get_error().message; } catch (e) { }
            if (args && args.set_errorHandled) args.set_errorHandled(true);
            if (!wasSaving && !wasResetting && !wasDeleting && !wasPaging && !wasSearching) modalShow(getModal('modalSelection'));
            refreshSaveButton(); showError(message);
        }
        function finishProgressHistoryOtpReset() {
            if (!historyResetPending) return;
            var statusField = getField('hdfProgressOtpResetStatus');
            var otpField = getField('hdfProgressOtpResetValue');
            var msgField = getField('hdfProgressOtpResetMessage');
            var status = statusField ? statusField.value : '';
            var newOtp = otpField ? otpField.value : '';
            var message = msgField ? msgField.value : '';
            var id = historyResetReportId;
            if (statusField) statusField.value = '';
            if (otpField) otpField.value = '';
            if (msgField) msgField.value = '';
            historyResetPending = false;
            historyResetReportId = '';
            busy = false;
            loading(false);
            // The history modal is not inside the updated panel; do not hide/show it here.
            if (status !== 'success' || !/^\d{6}$/.test(newOtp)) {
                showError(message || 'Cấp lại OTP thất bại. Vui lòng thử lại.');
                return;
            }
            var row = document.querySelector('.share-progress-history-row[data-report-id="' + id + '"]');
            if (row) {
                var code = row.querySelector('input.share-progress-history-value.is-otp');
                if (code) code.value = newOtp;
                else {
                    var holder = row.querySelector('.share-progress-history-otp-slot');
                    if (holder) {
                        var input = document.createElement('input');
                        input.type = 'text'; input.readOnly = true;
                        input.className = 'share-progress-history-value is-otp';
                        input.setAttribute('aria-label', 'Mã OTP báo cáo');
                        input.value = newOtp;
                        holder.textContent = '';
                        holder.appendChild(input);
                        var copy = document.createElement('button'); copy.type = 'button';
                        copy.className = 'share-progress-history-icon';
                        copy.setAttribute('title', 'Sao chép mã OTP');
                        copy.innerHTML = '<i class="far fa-copy"></i>';
                        copy.onclick = function () { return w.copyProgressHistoryValue(copy, 'otp'); };
                        holder.appendChild(copy);
                    }
                }
            }
            showSaved('Đã cấp mã OTP mới. Mã cũ không còn hiệu lực.');
        }
        function finishProgressHistoryDelete() {
            if (!historyDeletePending) return;
            var statusField = getField('hdfProgressDeleteStatus');
            var messageField = getField('hdfProgressDeleteMessage');
            var ok = statusField && statusField.value === 'success';
            var message = messageField ? messageField.value : '';
            var id = historyDeleteReportId;
            if (statusField) statusField.value = ''; if (messageField) messageField.value = '';
            historyDeletePending = false; historyDeleteReportId = ''; busy = false; loading(false);
            if (!ok) { showError(message || 'Xóa báo cáo thất bại.'); return; }
            var row = document.querySelector('.share-progress-history-row[data-report-id="' + id + '"]');
            if (row) {
                var list = row.closest('.share-progress-history-list'); row.remove();
                if (list && !list.querySelector('.share-progress-history-row')) {
                    var empty = document.createElement('div'); empty.className = 'share-progress-history-empty';
                    empty.textContent = 'Không còn báo cáo trên trang này. Chuyển trang hoặc mở lại Nhật ký để xem các báo cáo khác.';
                    list.appendChild(empty);
                }
            }
            var modal = getModal('modalProgressHistory');
            var count = modal && modal.querySelector('[id$="lblProgressHistoryCount"]');
            if (count) { var oldCount = parseInt(count.textContent, 10); if (!isNaN(oldCount)) count.textContent = Math.max(0, oldCount - 1) + ' báo cáo'; }
            showSaved(message || 'Đã xóa báo cáo.');
        }
        function wirePageRequestManager() {
            if (!(w.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)) return;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm._shareProgressReliableHook) return;
            prm._shareProgressReliableHook = true;
            prm.add_endRequest(function (sender, args) {
                if (args.get_error && args.get_error()) { onRequestError(args); return; }
                if (historyDeletePending) { finishProgressHistoryDelete(); if (historySearchQueued) w.setTimeout(function () { dispatchHistorySearch(true); }, 0); updateTree(); refreshSaveButton(); return; }
                if (historyResetPending) { finishProgressHistoryOtpReset(); if (historySearchQueued) w.setTimeout(function () { dispatchHistorySearch(true); }, 0); updateTree(); refreshSaveButton(); return; }
                if (historyPagingPending) { historyPagingPending = false; if (historySearchQueued) w.setTimeout(function () { dispatchHistorySearch(true); }, 0); syncProgressHistoryScroll(); updateTree(); refreshSaveButton(); return; }
                if (historySearchInFlight) { finishHistorySearch(); syncProgressHistoryScroll(); updateTree(); refreshSaveButton(); return; }
                if (historySearchQueued) w.setTimeout(function () { dispatchHistorySearch(true); }, 0);
                if (!consumeServerOutcome() && (busy || savePending)) {
                    var wasSaving = savePending; busy = false; savePending = false; loading(false);
                    if (!wasSaving) modalShow(getModal('modalSelection'));
                    showError(wasSaving ? 'Máy chủ chưa xác nhận lưu cấu hình. Vui lòng thử lại.' : 'Máy chủ không trả trạng thái tạo báo cáo. Kiểm tra UpdatePanel và log server.');
                }
                updateTree(); refreshSaveButton();
            });
        }
        function makeDefaultName(project) {
            var d = new Date(), two = function (n) { return String(n).padStart(2, '0'); };
            var stamp = two(d.getDate()) + two(d.getMonth() + 1) + two(d.getFullYear() % 100) + '-' + two(d.getHours()) + two(d.getMinutes());
            project = (project || 'Dự án').trim();
            var available = Math.max(6, 120 - stamp.length - 3);
            return project.substring(0, available).trim() + ' · ' + stamp;
        }
        w.ShareProgressUI = { openSelection: function (e) { if (e && e.preventDefault) e.preventDefault(); if (busy || modalSwitchPending) return false; updateTree(); refreshSaveButton(); if (!modalShow(getModal('modalSelection'))) showError('Không thể mở popup. Vui lòng tải lại trang và thử lại.'); return false; }, consumeServerOutcome: consumeServerOutcome, reset: function () { busy = false; loading(false); }, closeCreated: function () { modalHide(getModal('modalCreated')); } };
        w.shareProgressOpenHistory = function (e) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || modalSwitchPending) return false;
            // Nhật ký là popup riêng, chỉ tải khi bấm nút trên trang Gantt.
            var load = document.querySelector('a[id$="btnLoadProgressHistory"]');
            if (!load) { showError('Không tìm thấy nút tải nhật ký.'); return false; }
            historySearchQueued = false;
            historySearchLastSent = null;
            historySearchLastCompleted = null;
            var input = historySearchInput();
            if (input) input.value = '';
            historySearchBusy(false);
            historyCloseSuggestions();
            load.click();
            return false;
        };

        // Xác nhận qua MessageBox chuẩn do MasterTemplate hiển thị.
        // Không có window.confirm() hoặc custom JS MessageBox trên trang Gantt.
        w.shareProgressDeleteHistoryReport = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || historyResetPending || historyDeletePending || historyPagingPending || !button) return false;
            var id = button.getAttribute('data-report-id') || '';
            if (!/^[a-fA-F0-9]{32}$/.test(id)) return false;
            var field = getField('hdfProgressDeleteRequestId');
            var trigger = document.querySelector('a[id$="btnRequestProgressDelete"]');
            if (!field || !trigger) { showError('Không tìm thấy điều khiển xác nhận xóa báo cáo.'); return false; }
            field.value = id;
            trigger.click();
            return false;
        };
        w.shareProgressResetHistoryOtp = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (busy || savePending || historyResetPending || historyDeletePending || historyPagingPending || !button) return false;
            var id = button.getAttribute('data-report-id') || '';
            if (!/^[a-fA-F0-9]{32}$/.test(id)) return false;
            var field = getField('hdfProgressOtpResetRequestId');
            var trigger = document.querySelector('a[id$="btnRequestProgressOtpReset"]');
            if (!field || !trigger) { showError('Không tìm thấy điều khiển xác nhận cấp lại OTP.'); return false; }
            field.value = id;
            trigger.click();
            return false;
        };
        // Chỉ được gọi sau khi server nhận xác nhận của MessageBox chuẩn.
        // Các HiddenField kết quả đã được cập nhật qua UpdatePanel riêng, không thay DOM Nhật ký.
        w.shareProgressApplyConfirmedAction = function (kind, id) {
            if (!/^[a-fA-F0-9]{32}$/.test(id || '')) return;
            busy = true;
            if (kind === 'otp') {
                historyResetReportId = id;
                historyResetPending = true;
                finishProgressHistoryOtpReset();
            } else if (kind === 'delete') {
                historyDeleteReportId = id;
                historyDeletePending = true;
                finishProgressHistoryDelete();
            }
            syncProgressHistoryScroll();
        };
        // MasterTemplate's MessageBox is placed above the current ExtraModal without styling its border.
        w.shareProgressStackSystemMessageBox = function () {
            var masterModal = document.getElementById('modal-notify');
            if (!masterModal) return;
            masterModal.classList.add('sp-gantt-system-confirm');
            [0, 60, 200].forEach(function (delay) {
                w.setTimeout(function () {
                    if (!masterModal.classList.contains('show')) return;
                    var backdrops = document.querySelectorAll('.modal-backdrop');
                    if (backdrops.length) backdrops[backdrops.length - 1].classList.add('sp-gantt-system-backdrop');
                }, delay);
            });
        };
        // Khóa cuộn trang nền trong khi popup Nhật ký mở. Popup vẫn tự co giãn và
        // cuộn nội dung qua .modal-body khi vượt viewport. Trả CSS gốc khi đóng.
        var historyScrollState = null;
        var historyScrollObserver = null;
        function rememberHistoryPageScroll() {
            if (historyScrollState || !document.body) return;
            historyScrollState = { htmlOverflow: document.documentElement.style.overflow, bodyOverflow: document.body.style.overflow };
        }
        function historyIsShown() {
            var modal = getModal('modalProgressHistory');
            return !!modal && (modal.classList.contains('show') || modal.style.display === 'block' && modal.getAttribute('aria-hidden') !== 'true');
        }
        function fitHistoryContentToViewport() {
            var modal = getModal('modalProgressHistory');
            if (!modal || !historyIsShown()) return;
            var dialog = modal.querySelector('.modal-dialog');
            var section = modal.querySelector('.modal-body');
            if (!dialog || !section) return;
            var header = dialog.querySelector('.modal-header');
            var footer = dialog.querySelector('.modal-footer');
            var style = w.getComputedStyle(dialog);
            var margin = (parseFloat(style.marginTop) || 0) + (parseFloat(style.marginBottom) || 0);
            var available = (w.innerHeight || document.documentElement.clientHeight) - margin -
                (header ? header.offsetHeight : 0) - (footer ? footer.offsetHeight : 0) - 16;
            if (available > 100) section.style.maxHeight = Math.floor(available) + 'px';
        }
        function syncProgressHistoryScroll() {
            var html = document.documentElement, body = document.body;
            if (!html || !body) return;
            if (historyIsShown()) {
                rememberHistoryPageScroll();
                if (html.style.overflow !== 'hidden') html.style.overflow = 'hidden';
                if (body.style.overflow !== 'hidden') body.style.overflow = 'hidden';
                fitHistoryContentToViewport();
            } else if (historyScrollState) {
                html.style.overflow = historyScrollState.htmlOverflow;
                // Bootstrap may have applied overflow:hidden before our observer ran;
                // never strand the document locked after the final modal closes.
                var otherShownModal = !!document.querySelector('.modal.show');
                body.style.overflow = !otherShownModal && !body.classList.contains('modal-open') &&
                    historyScrollState.bodyOverflow === 'hidden' ? '' : historyScrollState.bodyOverflow;
                historyScrollState = null;
            }
        }
        function initProgressHistoryScrollLock() {
            var modal = getModal('modalProgressHistory');
            if (!modal) return;
            if (historyScrollObserver && historyScrollObserver.target === modal) { syncProgressHistoryScroll(); return; }
            if (historyScrollObserver) historyScrollObserver.disconnect();
            historyScrollObserver = new MutationObserver(function () { syncProgressHistoryScroll(); });
            historyScrollObserver.observe(modal, { attributes: true, attributeFilter: ['class', 'style', 'aria-hidden'] });
            historyScrollObserver.target = modal;
            modal.addEventListener('show.bs.modal', rememberHistoryPageScroll);
            modal.addEventListener('shown.bs.modal', syncProgressHistoryScroll);
            modal.addEventListener('hidden.bs.modal', syncProgressHistoryScroll);
            if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) {
                w.jQuery(modal).on('show.bs.modal.shareGanttScroll', rememberHistoryPageScroll);
                w.jQuery(modal).on('shown.bs.modal.shareGanttScroll hidden.bs.modal.shareGanttScroll', syncProgressHistoryScroll);
            }
            syncProgressHistoryScroll();
        }
        w.shareProgressSendCreatedCustomer = function (e) {
            if (e && e.preventDefault) e.preventDefault();
            var dialog = getModal('modalCreated');
            var reportLink = dialog ? dialog.querySelector('a[id$="lnkReportFile"]') : null;
            var reportName = dialog ? dialog.querySelector('.share-progress-file-name') : null;
            var otpField = getField('hdfOtp');
            var name = reportName ? reportName.textContent.trim() : 'Báo cáo tiến độ dự án';
            var url = reportLink ? reportLink.href : '';
            var otp = otpField ? otpField.value.trim() : '';
            if (!url || !/^\d{6}$/.test(otp)) {
                showError('Thiếu liên kết hoặc mã OTP để gửi khách hàng.');
                return false;
            }
            var subject = 'Báo cáo tiến độ: ' + name;
            var body = 'Xin chào,\n\nTôi gửi báo cáo tiến độ dự án để anh/chị tham khảo.\n\nBáo cáo: ' + name + '\nLiên kết: ' + url + '\nMã OTP: ' + otp + '\n\nTrân trọng.';
            w.location.href = 'mailto:?subject=' + encodeURIComponent(subject) + '&body=' + encodeURIComponent(body);
            return false;
        };
        w.shareProgressSendCustomer = function (button) {
            var row = button && button.closest('.share-progress-history-row');
            if (!row) return false;
            var name = row.getAttribute('data-report-name') || 'Báo cáo tiến độ dự án';
            var link = row.querySelector('.share-progress-history-link-hidden');
            var otp = row.querySelector('input.share-progress-history-value.is-otp');
            if (!link || !link.value || !otp || !/^\d{6}$/.test(otp.value)) {
                showError('Báo cáo chưa có đầy đủ liên kết hoặc mã OTP.'); return false;
            }
            var subject = 'Báo cáo tiến độ: ' + name;
            var body = 'Xin chào,\n\nTôi gửi báo cáo tiến độ dự án để anh/chị tham khảo.\n\nBáo cáo: ' + name + '\nLiên kết: ' + link.value + '\nMã OTP: ' + otp.value + '\n\nTrân trọng.';
            // Opens the user's email composer; does not send mail without their approval.
            w.location.href = 'mailto:?subject=' + encodeURIComponent(subject) + '&body=' + encodeURIComponent(body);
            return false;
        };
        w.shareProgressHistoryBeforePage = function (e, button) {
            if (busy || savePending || historyDeletePending || historyResetPending || historyPagingPending || historySearchInFlight || historySearchQueued) {
                if (e && e.preventDefault) e.preventDefault();
                return false;
            }
            historyPagingPending = true;
            // Bootstrap modal node is stable; only the inner UpdatePanel swaps rows/footer.
            var panel = document.querySelector('[id$="upShareProgressHistory"]');
            if (panel) panel.classList.add('share-progress-history-page-panel-pending');
            return true;
        };
        w.shareProgressToggleHistoryRow = function (e, button) {
            if (e && e.preventDefault) e.preventDefault();
            if (!button) return false;
            var row = button.closest('.share-progress-history-row');
            if (!row || busy || historyPagingPending) return false;
            var expand = !row.classList.contains('is-expanded');
            var list = row.closest('.share-progress-history-list');
            if (list) list.querySelectorAll('.share-progress-history-row.is-expanded').forEach(function (other) {
                if (other !== row) {
                    other.classList.remove('is-expanded');
                    other.querySelectorAll('.share-progress-history-name-btn').forEach(function (b) { b.setAttribute('aria-expanded', 'false'); });
                }
            });
            row.classList.toggle('is-expanded', expand);
            row.querySelectorAll('.share-progress-history-name-btn').forEach(function (b) { b.setAttribute('aria-expanded', expand ? 'true' : 'false'); });
            fitHistoryContentToViewport();
            return false;
        };
        w.copyProgressHistoryValue = function (button, kind) {
            var row = button && button.closest('.share-progress-history-share-line');
            var field = row && row.querySelector('input.share-progress-history-value');
            var value = field ? field.value : '';
            return copyText(value, button, kind === 'otp' ? 'Sao chép mã OTP' : 'Sao chép liên kết');
        };
        w.shareProgressToggleBranch = function (e, code) { if (e) { e.preventDefault(); e.stopPropagation(); } toggleRow(code); return false; };
        w.shareProgressRestoreDefaults = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy) return false; var el = root(); if (!el) return false; el.querySelectorAll("[data-sp-level]").forEach(function (row) { var check = row.querySelector("input[name=\"shareProgressTaskIds\"]"); if (check) check.checked = Number(row.getAttribute("data-sp-level")) <= 2; }); refreshSaveButton(); return false; };
        w.shareProgressUndo = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy || savedSelection === null) return false; var selected = new Set(JSON.parse(savedSelection)); var el = root(); if (el) el.querySelectorAll("input[name=\"shareProgressTaskIds\"]").forEach(function (ch) { ch.checked = selected.has(ch.value); }); refreshSaveButton(); return false; };
        w.validateProgressPresetSave = function (btn) { refreshSaveButton(); if (busy || savePending || !btn || btn.getAttribute("aria-disabled") === "true") return false; var el = root(); if (!el || !el.querySelector("input[name=\"shareProgressTaskIds\"]:checked")) { showError("Vui lòng chọn ít nhất một công việc trước khi lưu cấu hình."); return false; } savePending = true; refreshSaveButton(); return true; };
        w.shareProgressSavePreset = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.validateProgressPresetSave(btn)) return false; var link = document.querySelector("a[id$=\"btnSaveProgressPreset\"]"); if (!link) { savePending = false; refreshSaveButton(); showError("Không tìm thấy nút AJAX lưu cấu hình. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        w.showProgressCreationLoading = function (btn) { var title = document.getElementById('shareProgressReportName'); if (!title || !title.value.trim() || title.value.trim().length > 120) { showError('Nhập tên báo cáo (tối đa 120 ký tự).'); if (title) title.focus(); return false; } var el = root(); var checked = el ? el.querySelectorAll('input[name="shareProgressTaskIds"]:checked') : []; if (!checked.length) { var label = getModal('modalSelection'); var v = label ? label.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = 'Vui lòng chọn ít nhất một công việc.'; showError('Vui lòng chọn ít nhất một công việc để tạo báo cáo.'); return false; } if (busy) return false; var msg = getModal('modalSelection'); var v = msg ? msg.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = ''; busy = true; loading(true); return true; };
        w.shareProgressCreateReport = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.showProgressCreationLoading(btn)) return false; var link = document.querySelector("a[id$=\"btnCreateProgress\"]"); if (!link) { busy = false; loading(false); showError("Không tìm thấy nút AJAX tạo báo cáo. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        function copyFeedback(button, ok, original) { if (!button) return; button.setAttribute('data-copy-tooltip', ok ? 'Đã sao chép!' : 'Không thể sao chép'); button.setAttribute('aria-label', ok ? 'Đã sao chép!' : 'Không thể sao chép'); w.setTimeout(function () { if (button.isConnected) { button.setAttribute('data-copy-tooltip', original); button.setAttribute('aria-label', original); } }, 1700); if (!ok) showError('Không thể sao chép tự động. Vui lòng sao chép thủ công.'); }
        function fallbackCopyText(value) { var area = document.createElement('textarea'); area.value = value; area.readOnly = true; area.style.position = 'fixed'; area.style.left = '-9999px'; document.body.appendChild(area); area.focus(); area.select(); var ok = false; try { ok = document.execCommand('copy'); } catch (e) { } document.body.removeChild(area); return ok; }
        function copyText(value, button, hint) { if (!value) { showError('Chưa có dữ liệu để sao chép.'); return false; } if (navigator.clipboard && w.isSecureContext) { navigator.clipboard.writeText(value).then(function () { copyFeedback(button, true, hint); }, function () { copyFeedback(button, fallbackCopyText(value), hint); }); } else { copyFeedback(button, fallbackCopyText(value), hint); } return false; }
        w.copyGeneratedProgressOtp = function (button) { var field = getField('hdfOtp'); var otp = field ? field.value : ''; return copyText(otp, button, 'Sao chép mã OTP'); };
        w.copyGeneratedProgressLink = function (button) { var dialog = getModal('modalCreated'); var a = dialog ? dialog.querySelector('a[id$="lnkReportFile"]') : null; var url = a ? a.href : ''; return copyText(url, button, 'Sao chép liên kết'); };
        w.closeGeneratedProgressModal = function () { w.ShareProgressUI.closeCreated(); };
        document.addEventListener('change', function (e) { if (e.target && e.target.matches('#shareProgressTaskList input[name="shareProgressTaskIds"]')) refreshSaveButton(); });
        w.addEventListener('resize', function () { fitHistoryContentToViewport(); });
        document.addEventListener('shown.bs.modal', function (e) { if (e.target && e.target.id === 'modal-notify') { w.shareProgressStackSystemMessageBox(); syncProgressHistoryScroll(); } });
        document.addEventListener('hidden.bs.modal', function (e) { if (e.target && e.target.id === 'modal-notify') syncProgressHistoryScroll(); });
        wirePageRequestManager(); if (w.Sys && Sys.Application) Sys.Application.add_load(function () { wirePageRequestManager(); initProgressHistoryScrollLock(); if (!busy && !savePending) consumeServerOutcome(); updateTree(); refreshSaveButton(); }); else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { initProgressHistoryScrollLock(); updateTree(); consumeServerOutcome(); refreshSaveButton(); }); else { initProgressHistoryScrollLock(); updateTree(); consumeServerOutcome(); refreshSaveButton(); }
    })(window);
