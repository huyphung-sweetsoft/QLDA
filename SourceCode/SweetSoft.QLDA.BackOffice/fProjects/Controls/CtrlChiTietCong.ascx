<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChiTietCong.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChiTietCong" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<SweetSoft:ExtraModal runat="server" ID="mdlPersonalDetail" Type="Primary" Title="Chi tiết phần việc" Width="92%">
    <ContentTemplate>
        <div class="pd-detail-modal-wrap">
            <div class="pd-detail-modal-heading">
                <h4 id="pdPersonalDetailTitle" class="pd-detail-modal-title">Chi tiết</h4>
                <p id="pdPersonalDetailSub" class="pd-detail-modal-sub">Xem thêm thông tin trong phạm vi dự án hiện tại.</p>
            </div>
            <asp:UpdatePanel ID="upDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:HiddenField ID="hfDetailType" runat="server" />
                    <asp:HiddenField ID="hfDetailStaff" runat="server" />
                    <asp:LinkButton ID="btnLoadDetail" runat="server" CausesValidation="false"
                        OnClick="btnLoadDetail_Click" Style="display:none" aria-hidden="true" />
                    <div id="pdPersonalDetailModalBody"><asp:Literal ID="litDetailHtml" runat="server" EnableViewState="false" /></div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<script type="text/javascript">
(function () {
    if (window.QLDAProjectDetailPopup && window.QLDAProjectDetailPopup.version === 16) return;
    var clientId = '<%= ModalClientId %>';
    var typeId = '<%= RequestTypeId %>';
    var staffId = '<%= RequestStaffId %>';
    var triggerId = '<%= LoadButtonId %>';
    var pending = false;
    var preview = null, activeTrigger = null;
    function modalNode() {
        var el = document.getElementById(clientId);
        return el && el.classList && el.classList.contains('modal') ? el : (el ? el.querySelector('.modal') : null);
    }
    function showModal() {
        var modal = modalNode();
        if (!modal) return;
        if (window.bootstrap && window.bootstrap.Modal) {
            var instance = window.bootstrap.Modal.getInstance ? window.bootstrap.Modal.getInstance(modal) : null;
            (instance || new window.bootstrap.Modal(modal)).show();
        } else if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
            window.jQuery(modal).modal('show');
        }
    }
    function hidePreview() {
        if (preview && preview.parentNode) preview.parentNode.removeChild(preview);
        preview = null; activeTrigger = null;
    }
    function open(button) {
        if (!button || pending) return false;
        var source = button.getAttribute('data-pd-detail-source') || '';
        var valid = /^(pdPersonal(Task|Issue|Hour)Detail|pdTeamFullDetail|pdTeamDetail-(task|issue))$/.test(source);
        if (!valid) return false;
        var isTeam = source.indexOf('pdTeam') === 0;
        // JS never authorizes a member: C# checks PM/Admin and project membership again.
        var member = '';
        if (isTeam) {
            var memberInput = document.querySelector('#projectTeamHoursView input[type="hidden"][id$="hfTeamHoursMember"]');
            member = memberInput ? memberInput.value : '';
            if (!member) return false;
        }
        var title = document.getElementById('pdPersonalDetailTitle');
        var sub = document.getElementById('pdPersonalDetailSub');
        if (title) title.textContent = button.getAttribute('data-pd-detail-title') || 'Chi tiết';
        if (sub) sub.textContent = button.getAttribute('data-pd-detail-sub') || '';
        var t = document.getElementById(typeId), s = document.getElementById(staffId), load = document.getElementById(triggerId);
        if (!t || !s || !load) return false;
        t.value = source; s.value = member;
        hidePreview();
        pending = true;
        load.click(); // asynchronous UpdatePanel request: only loads when clicked.
        return false;
    }
    function received() {
        pending = false;
        hidePreview();
        var content = document.getElementById('pdPersonalDetailModalBody');
        var modal = modalNode();
        if (modal && content) modal.classList.toggle('pd-work-detail-modal', !!content.querySelector('.pd-ds-table'));
        showModal();
        var body = modal && modal.querySelector('.modal-body');
        if (body) body.scrollTop = 0;
    }
    function showPreview(trigger) {
        if (!trigger || trigger === activeTrigger) return;
        hidePreview();
        var template = trigger.parentElement && trigger.parentElement.querySelector('.pd-ds-hover-template');
        if (!template) return;
        var floating = document.createElement('div');
        floating.className = 'pd-ds-floating-preview'; floating.setAttribute('role','tooltip');
        floating.innerHTML = template.innerHTML; document.body.appendChild(floating);
        preview = floating; activeTrigger = trigger;
        var rect = trigger.getBoundingClientRect();
        var width = floating.getBoundingClientRect().width, height = floating.getBoundingClientRect().height;
        var left = rect.right + 12;
        if (left + width > window.innerWidth - 10) left = rect.left - width - 12;
        if (left < 10) left = Math.max(10,(window.innerWidth - width) / 2);
        floating.style.left = left + 'px';
        floating.style.top = Math.max(10,Math.min(rect.top,window.innerHeight - height - 10)) + 'px';
    }
    function jump(button) {
        var modalBody = button.closest('.modal-body'), scope = document.getElementById('pdPersonalDetailModalBody');
        if (!modalBody || !scope) return;
        var type = button.getAttribute('data-pd-ds-jump');
        if (!/^(task|meeting|issue)$/.test(type || '')) return;
        var target = scope.querySelector('[data-pd-detail-section="' + type + '"]');
        if (!target) return;
        var modal = modalNode(), heading = modal ? modal.querySelector('.pd-detail-modal-heading') : null;
        var reserved = heading ? heading.getBoundingClientRect().height + 18 : 18;
        var offset = target.getBoundingClientRect().top - modalBody.getBoundingClientRect().top;
        modalBody.scrollTo({top:Math.max(0,modalBody.scrollTop + offset - reserved),behavior:'smooth'});
    }
    if (!window.__pdPopupHandlers16) {
        window.__pdPopupHandlers16 = true;
        document.addEventListener('mouseover',function(e) {
            var t=e.target.closest && e.target.closest('#pdPersonalDetailModalBody .pd-ds-hover-trigger');
            if(t) showPreview(t);
        });
        document.addEventListener('mouseout',function(e){
            if(activeTrigger && activeTrigger.contains(e.target) && (!e.relatedTarget || !activeTrigger.contains(e.relatedTarget))) hidePreview();
        });
        document.addEventListener('focusin',function(e){
            var t=e.target.closest && e.target.closest('#pdPersonalDetailModalBody .pd-ds-hover-trigger');
            if(t) showPreview(t);
        });
        document.addEventListener('focusout',function(e){if(e.target===activeTrigger)hidePreview();});
        document.addEventListener('scroll',hidePreview,true);
        document.addEventListener('click',function(e){
            var b=e.target.closest && e.target.closest('#pdPersonalDetailModalBody [data-pd-ds-jump]');
            if(b){e.preventDefault();jump(b);}
        });
        document.addEventListener('hidden.bs.modal',function(e){if(e.target===modalNode())hidePreview();});
        // In case AJAX fails, don't leave the popup permanently locked.
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm=Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function(sender,args){
                if (pending && args.get_error()) {
                    pending=false;
                    var body=document.getElementById('pdPersonalDetailModalBody');
                    if(body) body.textContent='Không tải được dữ liệu chi tiết. Vui lòng thử lại.';
                    // Let global error handler own the ASP.NET AJAX failure; no alert.
                }
            });
        }
    }
    window.QLDAProjectDetailPopup = {version:16,open:open,received:received};
})();
</script>
