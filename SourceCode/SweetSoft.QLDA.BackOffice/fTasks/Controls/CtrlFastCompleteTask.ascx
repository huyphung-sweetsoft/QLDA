<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastCompleteTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlFastCompleteTask" %>

<style>
    .fc-task-box { background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:14px 16px;margin-bottom:16px; }
    .fc-date-wrapper { display:flex;align-items:center;gap:12px;flex-wrap:wrap; }
    .dynamic-status { font-weight:700;padding:8px 14px;border-radius:6px;font-size:14px;white-space:nowrap;transition:background-color .2s ease,color .2s ease,border-color .2s ease;border:1px solid transparent; }
    .status-ontime { background-color:#dcfce7;color:#14532d;border-color:#4ade80; }
    .status-late { background-color:#fee2e2;color:#b91c1c;border-color:#f87171; }
    .status-pending { background-color:#f1f5f9;color:#64748b;border-color:#cbd5e1; }
    .fc-ref-date { font-size:13px;color:#64748b;font-weight:600;margin-bottom:8px;display:block; }
    .fc-ref-date strong { color:#0f172a; }
    .fc-info-note { padding:12px 14px;border:1px solid #bfdbfe;background:#eff6ff;color:#1e40af;border-radius:7px;font-size:13px;line-height:1.6; }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlFastComplete" Type="Primary" Title="Xác nhận hoàn thành công việc">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlFastComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfCompleteTaskId" runat="server" />
                <asp:HiddenField ID="hdfExpectedEndDate" runat="server" />
                <div class="p-2">
                    <h5 class="text-primary mb-3 text-center"><i class="fas fa-check-double me-2"></i><asp:Literal ID="ltrActionHeading" runat="server" /></h5>
                    <div class="fc-task-box text-center">
                        <p class="text-muted mb-2" style="font-size:12.5px;">Bạn đang xác nhận hoàn thành công việc:</p>
                        <p class="fw-bold text-dark fs-6 mb-0"><asp:Literal ID="ltrTaskName" runat="server" /></p>
                    </div>
                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <span class="fc-ref-date"><i class="far fa-calendar-alt me-1"></i>Dự kiến hoàn thành: <strong class="text-primary"><asp:Literal ID="ltrExpectedEndDate" runat="server" /></strong></span>
                            <label class="form-label fw-bold"><i class="far fa-calendar-check me-1"></i>Ngày hoàn thành thực tế <span class="text-danger">*</span></label>
                            <div class="fc-date-wrapper">
                                <div style="max-width:200px;flex-grow:1;"><asp:TextBox runat="server" ID="txtActualDate" CssClass="form-control fw-bold text-dark" TextMode="Date" /></div>
                                <!-- Không mặc định 'Đúng hạn' trước khi đọc ngày từ UpdatePanel. -->
                                <span id="lblDynamicStatus" class="dynamic-status status-pending" role="status" aria-live="polite"><i class="far fa-calendar me-1"></i>Đang xác định</span>
                            </div>
                        </div>
                    </div>
                    <div id="divLateReason" class="mb-3" style="display:none;" aria-hidden="true">
                        <label class="form-label text-danger fw-bold"><i class="fas fa-pen-alt me-1"></i>Lý do trễ hạn (Bắt buộc) <span class="text-danger">*</span></label>
                        <asp:TextBox runat="server" ID="txtLateReason" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Vui lòng nhập lý do hoàn thành trễ hạn..." />
                    </div>
                    <div class="fc-info-note"><i class="fas fa-info-circle me-1"></i>Nếu người xác nhận không phải PM, công việc sẽ chuyển sang <strong>Chờ duyệt</strong>. PM sẽ kiểm tra kết quả trước khi xác nhận hoàn thành chính thức.</div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterFastComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2">
                    <asp:LinkButton ID="btnConfirmComplete" runat="server" CausesValidation="false" CssClass="btn btn-primary px-4" OnClick="btnConfirmComplete_Click" OnClientClick="return !window.calculateFastCompleteStatus || window.calculateFastCompleteStatus();"><i class="fas fa-save me-1"></i><asp:Literal ID="ltrConfirmButtonText" runat="server" /></asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    (function () {
        'use strict';
        // Một control có thể được cập nhật qua UpdatePanel mà không tải lại script.
        var modalId = '<%= mdlFastComplete.ClientID %>';
    var dateId = '<%= txtActualDate.ClientID %>';
    var expectedId = '<%= hdfExpectedEndDate.ClientID %>';

        function getModal() {
            var wrapper = document.getElementById(modalId);
            if (!wrapper) return null;
            return wrapper.classList && wrapper.classList.contains('modal') ? wrapper : (wrapper.querySelector('.modal') || wrapper);
        }
        function isIsoDate(s) { return /^\d{4}-\d{2}-\d{2}$/.test(s || ''); }
        function updateStatus() {
            var root = getModal();
            var input = document.getElementById(dateId);
            var expected = document.getElementById(expectedId);
            var label = root && root.querySelector('#lblDynamicStatus');
            var reason = root && root.querySelector('#divLateReason');
            if (!root || !input || !expected || !label || !reason) return true;

            var selected = (input.value || '').trim();
            var due = (expected.value || '').trim();
            var valid = isIsoDate(selected) && isIsoDate(due);
            // yyyy-MM-dd so sánh theo chuỗi là đúng thứ tự ngày, không phụ thuộc timezone.
            var late = valid && selected > due;
            label.classList.remove('status-pending', 'status-ontime', 'status-late');
            if (!valid) {
                label.classList.add('status-pending');
                label.innerHTML = '<i class="far fa-calendar me-1"></i>Chọn ngày hợp lệ';
            } else if (late) {
                label.classList.add('status-late');
                label.innerHTML = '<i class="fas fa-exclamation-triangle me-1"></i>Trễ hạn';
            } else {
                label.classList.add('status-ontime');
                label.innerHTML = '<i class="fas fa-check-circle me-1"></i>Đúng hạn';
            }
            reason.style.display = late ? 'block' : 'none';
            reason.setAttribute('aria-hidden', late ? 'false' : 'true');
            var reasonInput = reason.querySelector('textarea');
            if (reasonInput) reasonInput.required = late;
            return valid;
        }

        // Giữ API cũ do code-behind gọi, nhưng tìm phần tử trong đúng modal.
        window.calculateFastCompleteStatus = updateStatus;

        if (!window.__qlFastCompleteStatusEvents) {
            window.__qlFastCompleteStatusEvents = true;
            document.addEventListener('input', function (event) {
                if (event.target && event.target.id === dateId) updateStatus();
            }, true);
            document.addEventListener('change', function (event) {
                if (event.target && event.target.id === dateId) updateStatus();
            }, true);
            document.addEventListener('shown.bs.modal', function (event) {
                var modal = getModal();
                if (modal && (event.target === modal || modal.contains(event.target))) updateStatus();
            }, true);
            if (window.jQuery) {
                window.jQuery(document).on('shown.bs.modal.qlFastComplete', '#' + modalId, updateStatus);
            }
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { updateStatus(); });
            }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', updateStatus);
            else updateStatus();
        }
    })();
</script>
