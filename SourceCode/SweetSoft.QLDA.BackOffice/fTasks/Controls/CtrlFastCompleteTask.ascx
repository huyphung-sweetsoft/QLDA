<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastCompleteTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlFastCompleteTask" %>

<style>
    .fc-task-box {
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        padding: 14px 16px;
        margin-bottom: 16px;
    }

    .fc-date-wrapper {
        display: flex;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
    }

    .dynamic-status {
        font-weight: 700;
        padding: 8px 14px;
        border-radius: 6px;
        font-size: 14px;
        white-space: nowrap;
        transition: all 0.3s ease;
        border: 1px solid transparent;
    }

    .status-ontime {
        background-color: #dcfce7;
        color: #14532d;
        border-color: #4ade80;
    }

    .status-late {
        background-color: #fee2e2;
        color: #b91c1c;
        border-color: #f87171;
    }

    .fc-ref-date {
        font-size: 13px;
        color: #64748b;
        font-weight: 600;
        margin-bottom: 8px;
        display: block;
    }

    .fc-ref-date strong {
        color: #0f172a;
    }

    .fc-info-note {
        padding: 12px 14px;
        border: 1px solid #bfdbfe;
        background: #eff6ff;
        color: #1e40af;
        border-radius: 7px;
        font-size: 13px;
        line-height: 1.6;
    }
</style>

<!-- POPUP BÁO HOÀN THÀNH NHANH -->
<SweetSoft:ExtraModal
    runat="server"
    ID="mdlFastComplete"
    Type="Primary"
    Title="Xác nhận hoàn thành công việc">

    <ContentTemplate>
        <asp:UpdatePanel
            ID="upnlFastComplete"
            runat="server"
            UpdateMode="Conditional">

            <ContentTemplate>
                <asp:HiddenField ID="hdfCompleteTaskId" runat="server" />
                <asp:HiddenField ID="hdfExpectedEndDate" runat="server" />

                <div class="p-2">
                    <h5 class="text-primary mb-3 text-center">
                        <i class="fas fa-check-double me-2"></i>
                        <asp:Literal ID="ltrActionHeading" runat="server" />
                    </h5>

                    <div class="fc-task-box text-center">
                        <p class="text-muted mb-2" style="font-size: 12.5px;">
                            Bạn đang xác nhận hoàn thành công việc:
                        </p>
                        <p class="fw-bold text-dark fs-6 mb-0">
                            <asp:Literal ID="ltrTaskName" runat="server" />
                        </p>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <span class="fc-ref-date">
                                <i class="far fa-calendar-alt me-1"></i>
                                Dự kiến hoàn thành:
                                <strong class="text-primary">
                                    <asp:Literal ID="ltrExpectedEndDate" runat="server" />
                                </strong>
                            </span>

                            <label class="form-label fw-bold">
                                <i class="far fa-calendar-check me-1"></i>
                                Ngày hoàn thành thực tế
                                <span class="text-danger">*</span>
                            </label>

                            <div class="fc-date-wrapper">
                                <div style="max-width: 200px; flex-grow: 1;">
                                    <asp:TextBox
                                        runat="server"
                                        ID="txtActualDate"
                                        CssClass="form-control fw-bold text-dark"
                                        TextMode="Date" />
                                </div>

                                <span id="lblDynamicStatus" class="dynamic-status status-ontime">
                                    <i class="fas fa-check-circle me-1"></i>Đúng hạn
                                </span>
                            </div>
                        </div>
                    </div>

                    <!-- Chỉ hiện khi ngày thực tế trễ hơn ngày dự kiến -->
                    <div id="divLateReason" class="mb-3" style="display: none;">
                        <label class="form-label text-danger fw-bold">
                            <i class="fas fa-pen-alt me-1"></i>
                            Lý do trễ hạn (Bắt buộc)
                            <span class="text-danger">*</span>
                        </label>

                        <asp:TextBox
                            runat="server"
                            ID="txtLateReason"
                            CssClass="form-control"
                            TextMode="MultiLine"
                            Rows="3"
                            placeholder="Vui lòng nhập lý do hoàn thành trễ hạn..." />
                    </div>

                    <div class="fc-info-note">
                        <i class="fas fa-info-circle me-1"></i>
                        Nếu người xác nhận không phải PM, công việc sẽ chuyển sang
                        <strong>Chờ duyệt</strong>. PM sẽ kiểm tra kết quả trước khi
                        xác nhận hoàn thành chính thức.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel
            ID="upnlFooterFastComplete"
            runat="server"
            UpdateMode="Conditional">

            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2">
                    <asp:LinkButton
                        ID="btnConfirmComplete"
                        runat="server"
                        CausesValidation="false"
                        CssClass="btn btn-primary px-4"
                        OnClick="btnConfirmComplete_Click">
                        <i class="fas fa-save me-1"></i>
                        <asp:Literal ID="ltrConfirmButtonText" runat="server" />
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    // Cập nhật nhãn đúng hạn/trễ hạn theo ngày được chọn.
    function calculateFastCompleteStatus() {
        var actualDateStr = $('#<%= txtActualDate.ClientID %>').val();
        var expectedDateStr = $('#<%= hdfExpectedEndDate.ClientID %>').val();
        var lblStatus = $('#lblDynamicStatus');
        var divReason = $('#divLateReason');

        if (!actualDateStr || !expectedDateStr) {
            return;
        }

        // Ghép giờ 00:00 để so sánh ngày, tránh lệch do múi giờ.
        var actualDate = new Date(actualDateStr + 'T00:00:00');
        var expectedDate = new Date(expectedDateStr + 'T00:00:00');

        if (actualDate > expectedDate) {
            lblStatus
                .html('<i class="fas fa-exclamation-triangle me-1"></i>Trễ hạn')
                .removeClass('status-ontime')
                .addClass('status-late');

            divReason.stop(true, true).slideDown(200);
        } else {
            lblStatus
                .html('<i class="fas fa-check-circle me-1"></i>Đúng hạn')
                .removeClass('status-late')
                .addClass('status-ontime');

            divReason.stop(true, true).slideUp(200);
        }
    }

    $(document).on(
        'change',
        '#<%= txtActualDate.ClientID %>',
        calculateFastCompleteStatus
    );
</script>
