<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlShareProgress.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls.CtrlShareProgress" %>
<%@ Import Namespace="System.Web" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
        <SweetSoft:ExtraModal runat="server" ID="modalSelection" Type="Primary" Title="Gửi tiến độ cho khách hàng">
            <ContentTemplate>
                <div class="share-progress-intro">
                    Chọn các công việc sẽ xuất hiện trong báo cáo tiến độ gửi cho khách hàng.
                    <asp:Label ID="lblPresetStatus" runat="server" />
                </div>

                <div class="share-progress-list-header">
                    <span><i class="fas fa-list-ul me-1"></i> DANH SÁCH CÔNG VIỆC THEO CẤP</span>
                    <div class="share-progress-list-tools">
                        <span class="share-progress-list-hint">Mở mũi tên để chọn cấp sâu hơn</span>
                        <div class="share-progress-preset-actions" role="group" aria-label="Điều chỉnh cấu hình công việc">
                            <button type="button" id="shareProgressUndoClient" class="share-progress-tool-btn is-disabled" aria-disabled="true" data-sp-tooltip="Khôi phục các lựa chọn theo cấu hình đã lưu gần nhất" onclick="return shareProgressUndo(event);"><i class="fas fa-undo-alt" aria-hidden="true"></i> Hoàn tác</button>
                            <button type="button" class="share-progress-tool-btn" data-sp-tooltip="Chọn công việc cấp 1 và 2; bỏ chọn những cấp sâu hơn. Chưa lưu ngay." onclick="return shareProgressRestoreDefaults(event);"><i class="fas fa-layer-group" aria-hidden="true"></i> Mặc định</button>
                            <button type="button" id="shareProgressSavePresetClient" class="share-progress-tool-btn share-progress-tool-save is-disabled" aria-disabled="true" data-sp-tooltip="Lưu các công việc đang chọn cho dự án này" onclick="return shareProgressSavePreset(event,this);"><i class="fas fa-save" aria-hidden="true"></i> Lưu</button>
                        </div>
                    </div>
                </div>
                <div class="share-progress-task-list" id="shareProgressTaskList">
                    <asp:Literal ID="ltrTaskChoices" runat="server" />
                    <asp:PlaceHolder ID="phNoTasks" runat="server" Visible="false">
                        <div class="share-progress-empty">Dự án chưa có công việc để tạo báo cáo.</div>
                    </asp:PlaceHolder>
                </div>

                <asp:Label ID="lblSelectionMessage" runat="server" CssClass="share-progress-validation" EnableViewState="false" />

            </ContentTemplate>
            <FooterTemplate>
                <button type="button" class="share-progress-primary-btn" onclick="return shareProgressCreateReport(event,this);"><i class="fas fa-file-alt me-1" aria-hidden="true"></i> Tạo báo cáo tiến độ</button>
            </FooterTemplate>
        </SweetSoft:ExtraModal>

<asp:UpdatePanel ID="upShareProgress" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <%-- Event targets are inside the UpdatePanel: ASP.NET registers their AJAX lifecycle automatically.
             ExtraModal FooterTemplate may not exist yet during UserControl.OnLoad; do not register those controls manually. --%>
        <asp:LinkButton ID="btnSaveProgressPreset" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnSaveProgressPreset_Click" tabindex="-1" aria-hidden="true">Lưu cấu hình</asp:LinkButton>
        <asp:LinkButton ID="btnCreateProgress" runat="server" CssClass="share-progress-server-trigger" CausesValidation="false" OnClick="btnCreateProgress_Click" tabindex="-1" aria-hidden="true">Tạo báo cáo</asp:LinkButton>
        <asp:HiddenField runat="server" ID="hdfShareProgressOutcome" />
        <asp:HiddenField runat="server" ID="hdfShareProgressErrorText" />

        <SweetSoft:ExtraModal runat="server" ID="modalCreated" Type="Primary" Title="Tạo báo cáo tiến độ thành công">
            <ContentTemplate>
                <div class="share-progress-success-heading">
                    <span class="share-progress-success-icon"><i class="fas fa-check"></i></span>
                    <div>
                        <div class="share-progress-success-title">Báo cáo đã sẵn sàng</div>
                        <div class="share-progress-success-subtitle">Khách hàng cần mở đường dẫn và nhập đúng OTP mới xem được biểu đồ.</div>
                    </div>
                </div>

                <div class="share-progress-result-block">
                    <div class="share-progress-result-label"><i class="far fa-file-code"></i> File HTML báo cáo</div>
                    <div class="share-progress-file-row">
                        <asp:Literal ID="ltrReportFileName" runat="server" />
                        <button type="button" class="share-progress-copy-icon" data-copy-tooltip="Sao chép liên kết" aria-label="Sao chép liên kết" onclick="return copyGeneratedProgressLink(this);"><i class="far fa-copy" aria-hidden="true"></i></button>
                    </div>
                    <asp:HyperLink ID="lnkReportFile" runat="server" CssClass="share-progress-report-link" Target="_blank">
                        <i class="fas fa-external-link-alt"></i> Mở file HTML báo cáo
                    </asp:HyperLink>
                </div>

                <div class="share-progress-result-block">
                    <div class="share-progress-result-label"><i class="fas fa-shield-alt"></i> Mã OTP 6 chữ số</div>
                    <div class="share-progress-otp-row">
                        <span class="share-progress-otp-value"><asp:Literal ID="ltrOtp" runat="server" /></span>
                        <asp:HiddenField ID="hdfOtp" runat="server" />
                        <button type="button" class="share-progress-copy-icon" data-copy-tooltip="Sao chép mã OTP" aria-label="Sao chép mã OTP" onclick="return copyGeneratedProgressOtp(this);"><i class="far fa-copy" aria-hidden="true"></i></button>
                    </div>
                    <div class="share-progress-security-note">Gửi OTP riêng với đường dẫn báo cáo để khách hàng có thể mở nội dung.</div>
                </div>

                <asp:Label ID="lblCreateMessage" runat="server" CssClass="share-progress-success-message" />

                <div class="share-progress-modal-actions share-progress-success-actions">
                    <button type="button" class="share-progress-secondary-btn" onclick="closeGeneratedProgressModal();">Đóng</button>
                    <button type="button" class="share-progress-primary-btn" onclick="alert('Chức năng gửi khách hàng sẽ được bổ sung sau.');"><i class="fas fa-paper-plane"></i> Gửi khách hàng</button>
                </div>
            </ContentTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
                <div id="shareProgressInlineLoader" class="share-progress-loading-overlay" style="display:none;" aria-live="polite" aria-busy="true">
                    <div class="share-progress-loading-card">
                        <div class="share-progress-spinner"></div>
                        <div class="share-progress-loading-title">Đang tạo báo cáo tiến độ</div>
                        <div class="share-progress-loading-text">Đang dựng biểu đồ Gantt và bảo vệ báo cáo bằng mã OTP. Vui lòng chờ trong giây lát...</div>
                        <div class="share-progress-loading-track"><span></span></div>
                    </div>
                </div>


<style>
.share-progress-saved-toast { position:fixed;top:84px;right:22px;z-index:21000;max-width:min(430px,calc(100vw - 40px));padding:12px 16px;border:1px solid #a8dfc3;border-left:4px solid #13996b;border-radius:8px;background:#fff;color:#116f50;box-shadow:0 8px 30px rgba(18,20,30,.1);font-size:13px;font-weight:650; }
div[id$="modalSelection"] .modal-dialog { width:min(820px,calc(100vw - 32px))!important;max-width:min(820px,calc(100vw - 32px))!important; }
.share-progress-list-tools { display:flex;align-items:center;justify-content:flex-end;gap:10px;flex-wrap:wrap;min-width:0; }
.share-progress-trigger { margin-left: auto; display: inline-flex; align-items: center; justify-content: center; gap: 8px; flex: 0 0 auto; white-space: nowrap; border: 1px solid #542e88; border-radius: 6px; padding: 8px 13px; background: #542e88; color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: 0 2px 4px rgba(84, 46, 136, .12); transition: background .18s ease, border-color .18s ease, transform .18s ease; }
.share-progress-trigger:hover { background: #43216f; border-color: #43216f; color: #fff; transform: translateY(-1px); }
.share-progress-trigger:focus { outline: 2px solid rgba(84, 46, 136, .25); outline-offset: 2px; }
.share-progress-intro { color: #475569; font-size: 13px; line-height: 1.6; margin: 0 0 16px; }
.share-progress-intro span { display: block; color: #64748b; font-size: 12px; margin-top: 3px; }
.share-progress-list-header { position:relative;z-index:8;display:flex;align-items:center;justify-content:space-between;gap:12px;padding:11px 14px;background:#f8fafc;border:1px solid #dbe3ee;border-bottom:0;color:#475569;font-size:11px;font-weight:700;letter-spacing:.2px;flex-wrap:wrap; }
.share-progress-task-list { max-height: min(58vh, 560px); overflow-x:hidden; overflow-y:auto; border: 1px solid #dbe3ee; border-radius: 0 0 6px 6px; }
.share-progress-task-option { --sp-indent:0px;display:flex;align-items:center;gap:9px;min-height:43px;padding:7px 12px 7px calc(12px + var(--sp-indent));border-bottom:1px solid #edf1f6;color:#334155;font-size:13px;transition:background .15s ease; }
.share-progress-task-option:last-child { border-bottom: 0; }
.share-progress-task-option:hover { background: #f8f5ff; }
.share-progress-task-option input { width:16px;height:16px;margin:0;accent-color:#542e88;flex:0 0 16px;cursor:pointer; }
.share-progress-task-code { color:#542e88;font-weight:750;font-variant-numeric:tabular-nums;white-space:nowrap;flex:0 0 auto; }
.share-progress-task-name { min-width:0;overflow-wrap:anywhere;line-height:1.4; }

.share-progress-list-hint { color:#7b8190;font-size:11px;font-weight:500;letter-spacing:0; }
.share-progress-task-option[hidden] { display:none!important; }
.share-progress-task-option.is-level-1 { background:#faf7ff;font-weight:650; }
.share-progress-task-option.is-level-2 { background:#fff; }
.share-progress-task-option.is-level-3 { --sp-indent:23px; }
.share-progress-task-option.is-level-4 { --sp-indent:46px; }
.share-progress-task-option.is-level-5 { --sp-indent:69px; }
.share-progress-tree-toggle { flex:0 0 22px;width:22px;height:22px;display:inline-flex;align-items:center;justify-content:center;border:1px solid #e4d8f5;border-radius:5px;color:#7042a8;background:#faf7ff;cursor:pointer;transition:background .15s ease; }
.share-progress-tree-toggle:hover { background:#efe6fb; }
.share-progress-tree-toggle i { font-size:10px;transition:transform .15s ease; }
.share-progress-tree-toggle[aria-expanded="false"] i { transform:rotate(-90deg); }
.share-progress-tree-placeholder { flex:0 0 22px;width:22px; }
.share-progress-task-label { display:flex;align-items:center;gap:8px;min-width:0;flex:1;margin:0;cursor:pointer; }
.share-progress-task-option.is-level-1 .share-progress-task-name { font-weight:700; }
.share-progress-error-toast { position:fixed;top:84px;right:22px;z-index:21000;max-width:min(430px,calc(100vw - 40px));display:flex;align-items:flex-start;gap:10px;padding:13px 16px;border:1px solid #fecaca;border-left:4px solid #dc2626;border-radius:9px;background:#fff;color:#991b1b;box-shadow:0 8px 30px rgba(18,20,30,.14);font-size:13px;line-height:1.45; }
.share-progress-error-toast button { margin-left:auto;border:0;background:transparent;color:inherit;cursor:pointer;font-size:16px; }
.share-progress-empty { padding: 26px 16px; text-align: center; color: #64748b; font-size: 13px; }
.share-progress-validation { display: block; color: #b91c1c; font-size: 12px; margin-top: 10px; }
.share-progress-modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 18px; }
.share-progress-primary-btn, .share-progress-secondary-btn, .share-progress-copy-btn { display: inline-flex; align-items: center; justify-content: center; gap: 7px; border-radius: 6px; padding: 9px 14px; font-size: 13px; font-weight: 600; cursor: pointer; text-decoration: none; transition: background .16s ease, border-color .16s ease; }
.share-progress-primary-btn { border: 1px solid #542e88; background: #542e88; color: #fff; }
.share-progress-primary-btn:hover { background: #43216f; border-color: #43216f; color: #fff; }
.share-progress-primary-btn:disabled { opacity: .65; cursor: wait; }
.share-progress-secondary-btn { border: 1px solid #cbd5e1; background: #fff; color: #334155; }
.share-progress-secondary-btn:hover { background: #f8fafc; }
.share-progress-success-heading { display: flex; align-items: center; gap: 13px; margin-bottom: 20px; }
.share-progress-success-icon { display: flex; align-items: center; justify-content: center; flex: 0 0 42px; width: 42px; height: 42px; border-radius: 50%; color: #047857; background: #dcfce7; font-size: 18px; }
.share-progress-success-title { color: #0f172a; font-size: 16px; font-weight: 700; }
.share-progress-success-subtitle { margin-top: 4px; color: #64748b; font-size: 12px; line-height: 1.5; }
.share-progress-result-block { border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px; margin-bottom: 12px; background: #fff; }
.share-progress-result-label { color: #475569; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: .25px; margin-bottom: 9px; }
.share-progress-result-label i { color: #542e88; margin-right: 5px; }
.share-progress-report-link { display: inline-flex; align-items: center; gap: 7px; margin-top: 10px; color: #542e88; font-size: 13px; font-weight: 600; text-decoration: none; }
.share-progress-report-link:hover { color: #3b82f6; text-decoration: underline; }
.share-progress-file-name { display: block; color: #334155; font-size: 12px; overflow-wrap: anywhere; }
.share-progress-otp-row { display: flex; align-items: center; flex-wrap: wrap; gap: 12px; }
.share-progress-otp-value { color: #0f172a; font-size: 26px; font-weight: 800; letter-spacing: 5px; font-variant-numeric: tabular-nums; }
.share-progress-copy-btn { border: 1px solid #ddd2f0; background: #f6f0ff; color: #542e88; }
.share-progress-copy-btn:hover { background: #eee3ff; }
.share-progress-security-note { color: #64748b; font-size: 11px; margin-top: 8px; }
.share-progress-success-message { display: block; color: #047857; font-size: 12px; margin-top: 6px; }
.share-progress-success-actions { justify-content: space-between; }
.share-progress-loading-overlay { position: fixed; inset: 0; z-index: 20000; align-items: center; justify-content: center; padding: 24px; background: rgba(15, 23, 42, .38); backdrop-filter: blur(2px); }
.share-progress-loading-card { width: min(420px, 100%); padding: 28px 26px 24px; background: #fff; border-radius: 12px; box-shadow: 0 20px 60px rgba(15,23,42,.24); text-align: center; }
.share-progress-spinner { width: 42px; height: 42px; margin: 0 auto 16px; border: 4px solid #e9ddfb; border-top-color: #542e88; border-radius: 50%; animation: shareProgressSpin .8s linear infinite; }
.share-progress-loading-title { color: #1e293b; font-size: 16px; font-weight: 700; }
.share-progress-loading-text { margin: 8px auto 16px; max-width: 330px; color: #64748b; font-size: 12px; line-height: 1.65; }
.share-progress-loading-track { height: 5px; overflow: hidden; border-radius: 999px; background: #eee7f8; }
.share-progress-loading-track span { display: block; width: 38%; height: 100%; border-radius: inherit; background: #542e88; animation: shareProgressLoad 1.25s ease-in-out infinite; }
@keyframes shareProgressSpin { to { transform: rotate(360deg); } }
@keyframes shareProgressLoad { 0% { transform: translateX(-110%); } 100% { transform: translateX(290%); } }
@media (max-width: 600px) {
.share-progress-list-header { position:relative;z-index:8;display:flex;align-items:center;justify-content:space-between;gap:12px;padding:11px 14px;background:#f8fafc;border:1px solid #dbe3ee;border-bottom:0;color:#475569;font-size:11px;font-weight:700;letter-spacing:.2px;flex-wrap:wrap; }
.share-progress-task-option { --sp-indent:0px;display:flex;align-items:center;gap:9px;min-height:43px;padding:7px 12px 7px calc(12px + var(--sp-indent));border-bottom:1px solid #edf1f6;color:#334155;font-size:13px;transition:background .15s ease; }
    .share-progress-trigger { padding: 8px 10px; font-size: 12px; }
    .share-progress-success-actions { flex-wrap: wrap; }
}
.share-progress-file-row { display:flex;align-items:center;gap:10px;min-width:0; }
.share-progress-file-row .share-progress-file-name { flex:1;min-width:0;overflow-wrap:anywhere; }
.share-progress-copy-icon { position:relative;display:inline-flex;align-items:center;justify-content:center;flex:0 0 36px;width:36px;height:36px;padding:0;border:1px solid #d8c8ee;border-radius:7px;background:#faf7ff;color:#542e88;font-size:14px;cursor:pointer;transition:background .16s ease,border-color .16s ease,color .16s ease; }
.share-progress-copy-icon:hover,.share-progress-copy-icon:focus-visible { background:#eee4fc;border-color:#ad8cdb;color:#40166d;outline:none; }
.share-progress-copy-icon::after { content:attr(data-copy-tooltip);position:absolute;bottom:calc(100% + 8px);right:0;z-index:20;padding:6px 9px;border-radius:6px;background:#302344;color:#fff;font-size:11px;line-height:1.3;font-weight:600;white-space:nowrap;pointer-events:none;opacity:0;visibility:hidden;transform:translateY(4px);transition:opacity .15s ease,transform .15s ease,visibility .15s ease; }
.share-progress-copy-icon:hover::after,.share-progress-copy-icon:focus-visible::after { opacity:1;visibility:visible;transform:translateY(0); }

.share-progress-server-trigger { display:none!important; }
.share-progress-preset-actions { display:inline-flex;align-items:center;gap:5px;flex-wrap:nowrap; }
.share-progress-tool-btn { position:relative;display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:32px;padding:6px 10px;border:1px solid #ded4ed;border-radius:7px;background:#fff;color:#58318d;font-size:12px;font-weight:700;letter-spacing:0;white-space:nowrap;cursor:pointer;transition:border-color .16s ease,background .16s ease,color .16s ease; }
.share-progress-tool-btn:hover:not(.is-disabled),.share-progress-tool-btn:focus-visible { background:#f4effc;border-color:#ae8cda;outline:none; }
.share-progress-tool-save { background:#542e88;border-color:#542e88;color:#fff; }
.share-progress-tool-save:hover:not(.is-disabled),.share-progress-tool-save:focus-visible { background:#452174;border-color:#452174;color:#fff; }
.share-progress-tool-btn.is-disabled { opacity:.46;cursor:not-allowed;background:#f6f5f8;border-color:#e1dce9;color:#888197; }
.share-progress-tool-btn::after { content:attr(data-sp-tooltip);position:absolute;z-index:200;top:calc(100% + 10px);right:0;width:max-content;max-width:min(285px,75vw);padding:8px 10px;border-radius:7px;background:#302442;color:#fff;box-shadow:0 6px 18px rgba(35,20,55,.16);font-size:11px;font-weight:500;line-height:1.45;letter-spacing:0;text-align:left;white-space:normal;opacity:0;visibility:hidden;transform:translateY(-4px);pointer-events:none;transition:opacity .16s ease,transform .16s ease,visibility .16s ease; }
.share-progress-tool-btn::before { content:"";position:absolute;z-index:201;top:calc(100% + 3px);right:16px;border-width:0 6px 7px;border-style:solid;border-color:transparent transparent #302442;opacity:0;visibility:hidden;pointer-events:none;transition:opacity .16s ease,visibility .16s ease; }
.share-progress-tool-btn:hover::after,.share-progress-tool-btn:focus-visible::after,.share-progress-tool-btn:hover::before,.share-progress-tool-btn:focus-visible::before { opacity:1;visibility:visible;transform:translateY(0); }
@media(max-width:700px) { .share-progress-list-tools { width:100%;justify-content:space-between; } .share-progress-list-hint { display:none; } .share-progress-preset-actions { width:100%;justify-content:flex-end;flex-wrap:wrap; } }
</style>

<script type="text/javascript">
    (function (w) {
        'use strict';
        var busy = false;
        var savePending = false;
        var savedSelection = null;
        var previousTaskList = null;
        function selectedIds() { var el = root(); if (!el) return []; return Array.prototype.map.call(el.querySelectorAll('input[name="shareProgressTaskIds"]:checked'), function (x) { return x.value; }).sort(); }
        function selectedSignature() { return JSON.stringify(selectedIds()); }
        function saveButton() { return document.getElementById("shareProgressSavePresetClient"); }
        function refreshSaveButton() { var el = root(); var btn = saveButton(); if (!el || !btn) return; if (el !== previousTaskList || savedSelection === null) { previousTaskList = el; savedSelection = selectedSignature(); } var dirty = selectedSignature() !== savedSelection; var disabled = !dirty || !selectedIds().length || savePending || busy; btn.classList.toggle("is-disabled", disabled); btn.setAttribute("aria-disabled", disabled ? "true" : "false"); var undo = document.getElementById("shareProgressUndoClient"); if (undo) { undo.classList.toggle("is-disabled", !dirty || savePending || busy); undo.setAttribute("aria-disabled", dirty && !savePending && !busy ? "false" : "true"); } }
        function acceptedSave(message) { savePending = false; savedSelection = selectedSignature(); var selector = getModal("modalSelection"); var label = selector ? selector.querySelector("[id$=\"lblPresetStatus\"]") : null; if (label) label.textContent = "Đã lưu cấu hình của dự án. Lần sau mở sẽ tự chọn đúng các công việc này."; refreshSaveButton(); showSaved(message); }
        function getModal(suffix) { return document.querySelector('[id$="' + suffix + '"]'); }
        function getField(suffix) { return document.querySelector('input[id$="' + suffix + '"]'); }
        function modalShow(el) { if (!el) return false; try { if (w.bootstrap && w.bootstrap.Modal) { w.bootstrap.Modal.getOrCreateInstance(el).show(); return true; } if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) { w.jQuery(el).modal('show'); return true; } } catch (e) { if (w.console) console.error(e); } return false; }
        function modalHide(el) { if (!el) return; try { if (w.bootstrap && w.bootstrap.Modal) { var m = w.bootstrap.Modal.getInstance(el); if (m) m.hide(); return; } if (w.jQuery && w.jQuery.fn && w.jQuery.fn.modal) { w.jQuery(el).modal('hide'); return; } } catch (e) { if (w.console) console.error(e); } }
        function clearOldBackdrop() { if (document.querySelector('.modal.show')) return; document.querySelectorAll('.modal-backdrop').forEach(function (el) { el.remove(); }); document.body.classList.remove('modal-open'); document.body.style.removeProperty('padding-right'); document.body.style.removeProperty('overflow'); }
        function loading(show) { var el = document.getElementById('shareProgressInlineLoader'); if (el) el.style.display = show ? 'flex' : 'none'; }
        function showError(message) { message = message || 'Tạo báo cáo không thành công. Vui lòng thử lại.'; if (w.toastr && typeof w.toastr.error === 'function') { w.toastr.error(message); return; } var old = document.getElementById('shareProgressErrorToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressErrorToast'; toast.className = 'share-progress-error-toast'; toast.setAttribute('role', 'alert'); var icon = document.createElement('i'); icon.className = 'fas fa-exclamation-circle'; var span = document.createElement('span'); span.textContent = message; var x = document.createElement('button'); x.type = 'button'; x.textContent = '×'; x.setAttribute('aria-label', 'Đóng thông báo lỗi'); x.onclick = function () { toast.remove(); }; toast.appendChild(icon); toast.appendChild(span); toast.appendChild(x); document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 8500); }
        function showSaved(message) { message = message || 'Đã lưu cấu hình công việc cho dự án này.'; if (w.toastr && typeof w.toastr.success === 'function') { w.toastr.success(message); return; } var old = document.getElementById('shareProgressSavedToast'); if (old) old.remove(); var toast = document.createElement('div'); toast.id = 'shareProgressSavedToast'; toast.className = 'share-progress-saved-toast'; toast.setAttribute('role', 'status'); toast.textContent = message; document.body.appendChild(toast); w.setTimeout(function () { if (toast.parentNode) toast.remove(); }, 3500); }
        function root() { return document.getElementById('shareProgressTaskList'); }
        function updateTree() { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var byCode = {}; rows.forEach(function (row) { byCode[row.getAttribute('data-sp-code')] = row; }); rows.forEach(function (row) { var parent = row.getAttribute('data-sp-parent'); var visible = true; while (parent) { var p = byCode[parent]; if (p) { if (p.hidden || p.getAttribute('data-sp-expanded') !== 'true') { visible = false; break; } } var dot = parent.lastIndexOf('.'); parent = dot < 0 ? '' : parent.substring(0, dot); } row.hidden = !visible; }); }
        function toggleRow(code) { var el = root(); if (!el) return; var rows = el.querySelectorAll('[data-sp-code]'); var row = null; rows.forEach(function (r) { if (r.getAttribute('data-sp-code') === code) row = r; }); if (!row) return; var now = row.getAttribute('data-sp-expanded') !== 'true'; row.setAttribute('data-sp-expanded', now ? 'true' : 'false'); var b = row.querySelector('.share-progress-tree-toggle'); if (b) b.setAttribute('aria-expanded', now ? 'true' : 'false'); updateTree(); }
        function onResult(kind, message) {
            if (kind === 'saved') { acceptedSave(message); return; }
            if (kind === 'saveError') { savePending = false; refreshSaveButton(); showError(message || 'Không thể lưu cấu hình.'); return; }
            savePending = false; busy = false; loading(false); clearOldBackdrop(); var selector = getModal('modalSelection');
            if (selector) { selector.classList.remove('show'); selector.style.display = 'none'; selector.setAttribute('aria-hidden', 'true'); selector.removeAttribute('aria-modal'); }
            w.setTimeout(function () { clearOldBackdrop(); if (kind === 'success') { if (!modalShow(getModal('modalCreated'))) showError('Đã tạo báo cáo nhưng không mở được popup kết quả. Hãy kiểm tra thư viện Bootstrap.'); } else { modalShow(getModal('modalSelection')); showError(message); } updateTree(); refreshSaveButton(); }, 60);
        }
        function consumeServerOutcome() { var result = getField('hdfShareProgressOutcome'); if (!result || !result.value) return false; var kind = result.value; result.value = ''; var msg = getField('hdfShareProgressErrorText'); var message = msg ? msg.value : ''; if (msg) msg.value = ''; onResult(kind, message); return true; }
        function onRequestError(args) { var wasSaving = savePending; savePending = false; busy = false; loading(false); var message = wasSaving ? 'Không thể lưu cấu hình. Vui lòng thử lại.' : 'Máy chủ gặp lỗi khi tạo báo cáo. Vui lòng thử lại.'; try { if (args && args.get_error && args.get_error()) message += ' ' + args.get_error().message; } catch (e) { } if (args && args.set_errorHandled) args.set_errorHandled(true); if (!wasSaving) { clearOldBackdrop(); modalShow(getModal('modalSelection')); } refreshSaveButton(); showError(message); }
        function wirePageRequestManager() { if (!(w.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)) return; var prm = Sys.WebForms.PageRequestManager.getInstance(); if (prm._shareProgressReliableHook) return; prm._shareProgressReliableHook = true; prm.add_endRequest(function (sender, args) { if (args.get_error && args.get_error()) { onRequestError(args); return; } if (!consumeServerOutcome() && (busy || savePending)) { var wasSaving = savePending; busy = false; savePending = false; loading(false); if (!wasSaving) modalShow(getModal('modalSelection')); showError(wasSaving ? 'Máy chủ chưa xác nhận lưu cấu hình. Vui lòng thử lại.' : 'Máy chủ không trả trạng thái tạo báo cáo. Kiểm tra UpdatePanel và log server.'); } updateTree(); refreshSaveButton(); }); }
        w.ShareProgressUI = { openSelection: function (e) { if (e && e.preventDefault) e.preventDefault(); if (busy) return false; updateTree(); refreshSaveButton(); if (!modalShow(getModal('modalSelection'))) showError('Không thể mở popup. Vui lòng tải lại trang và thử lại.'); return false; }, consumeServerOutcome: consumeServerOutcome, reset: function () { busy = false; loading(false); }, closeCreated: function () { modalHide(getModal('modalCreated')); } };
        w.shareProgressToggleBranch = function (e, code) { if (e) { e.preventDefault(); e.stopPropagation(); } toggleRow(code); return false; };
        w.shareProgressRestoreDefaults = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy) return false; var el = root(); if (!el) return false; el.querySelectorAll("[data-sp-level]").forEach(function (row) { var check = row.querySelector("input[name=\"shareProgressTaskIds\"]"); if (check) check.checked = Number(row.getAttribute("data-sp-level")) <= 2; }); refreshSaveButton(); return false; };
        w.shareProgressUndo = function (e) { if (e && e.preventDefault) e.preventDefault(); if (savePending || busy || savedSelection === null) return false; var selected = new Set(JSON.parse(savedSelection)); var el = root(); if (el) el.querySelectorAll("input[name=\"shareProgressTaskIds\"]").forEach(function (ch) { ch.checked = selected.has(ch.value); }); refreshSaveButton(); return false; };
        w.validateProgressPresetSave = function (btn) { refreshSaveButton(); if (busy || savePending || !btn || btn.getAttribute("aria-disabled") === "true") return false; var el = root(); if (!el || !el.querySelector("input[name=\"shareProgressTaskIds\"]:checked")) { showError("Vui lòng chọn ít nhất một công việc trước khi lưu cấu hình."); return false; } savePending = true; refreshSaveButton(); return true; };
        w.shareProgressSavePreset = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.validateProgressPresetSave(btn)) return false; var link = document.querySelector("a[id$=\"btnSaveProgressPreset\"]"); if (!link) { savePending = false; refreshSaveButton(); showError("Không tìm thấy nút AJAX lưu cấu hình. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        w.showProgressCreationLoading = function (btn) { var el = root(); var checked = el ? el.querySelectorAll('input[name="shareProgressTaskIds"]:checked') : []; if (!checked.length) { var label = getModal('modalSelection'); var v = label ? label.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = 'Vui lòng chọn ít nhất một công việc.'; showError('Vui lòng chọn ít nhất một công việc để tạo báo cáo.'); return false; } if (busy) return false; var msg = getModal('modalSelection'); var v = msg ? msg.querySelector('[id$="lblSelectionMessage"]') : null; if (v) v.textContent = ''; busy = true; loading(true); return true; };
        w.shareProgressCreateReport = function (e, btn) { if (e && e.preventDefault) e.preventDefault(); if (!w.showProgressCreationLoading(btn)) return false; var link = document.querySelector("a[id$=\"btnCreateProgress\"]"); if (!link) { busy = false; loading(false); showError("Không tìm thấy nút AJAX tạo báo cáo. Vui lòng tải lại trang."); return false; } link.click(); return false; };
        function copyFeedback(button, ok, original) { if (!button) return; button.setAttribute('data-copy-tooltip', ok ? 'Đã sao chép!' : 'Không thể sao chép'); button.setAttribute('aria-label', ok ? 'Đã sao chép!' : 'Không thể sao chép'); w.setTimeout(function () { if (button.isConnected) { button.setAttribute('data-copy-tooltip', original); button.setAttribute('aria-label', original); } }, 1700); if (!ok) showError('Không thể sao chép tự động. Vui lòng sao chép thủ công.'); }
        function fallbackCopyText(value) { var area = document.createElement('textarea'); area.value = value; area.readOnly = true; area.style.position = 'fixed'; area.style.left = '-9999px'; document.body.appendChild(area); area.focus(); area.select(); var ok = false; try { ok = document.execCommand('copy'); } catch (e) { } document.body.removeChild(area); return ok; }
        function copyText(value, button, hint) { if (!value) { showError('Chưa có dữ liệu để sao chép.'); return false; } if (navigator.clipboard && w.isSecureContext) { navigator.clipboard.writeText(value).then(function () { copyFeedback(button, true, hint); }, function () { copyFeedback(button, fallbackCopyText(value), hint); }); } else { copyFeedback(button, fallbackCopyText(value), hint); } return false; }
        w.copyGeneratedProgressOtp = function (button) { var field = getField('hdfOtp'); var otp = field ? field.value : ''; return copyText(otp, button, 'Sao chép mã OTP'); };
        w.copyGeneratedProgressLink = function (button) { var dialog = getModal('modalCreated'); var a = dialog ? dialog.querySelector('a[id$="lnkReportFile"]') : null; var url = a ? a.href : ''; return copyText(url, button, 'Sao chép liên kết'); };
        w.closeGeneratedProgressModal = function () { w.ShareProgressUI.closeCreated(); };
        document.addEventListener('change', function (e) { if (e.target && e.target.matches('#shareProgressTaskList input[name="shareProgressTaskIds"]')) refreshSaveButton(); });
        wirePageRequestManager(); if (w.Sys && Sys.Application) Sys.Application.add_load(function () { wirePageRequestManager(); if (!busy && !savePending) consumeServerOutcome(); updateTree(); refreshSaveButton(); }); else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { updateTree(); consumeServerOutcome(); refreshSaveButton(); }); else { updateTree(); consumeServerOutcome(); refreshSaveButton(); }
    })(window);
</script>
