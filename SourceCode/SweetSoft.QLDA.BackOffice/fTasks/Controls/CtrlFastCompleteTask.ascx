<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastCompleteTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlFastCompleteTask" %>

<style>
    /* CSS CHO POPUP HOÀN THÀNH NHANH */
    .fc-task-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 16px; margin-bottom: 16px; }
    .fc-date-wrapper { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
    
    /* Label trạng thái động */
    .dynamic-status { font-weight: 700; padding: 8px 14px; border-radius: 6px; font-size: 14px; white-space: nowrap; transition: all 0.3s ease; border: 1px solid transparent; }
    .status-ontime { background-color: #dcfce7; color: #14532d; border-color: #4ade80; }
    .status-late { background-color: #fee2e2; color: #b91c1c; border-color: #f87171; }
    
    .fc-ref-date { font-size: 13px; color: #64748b; font-weight: 600; margin-bottom: 8px; display: block;}
    .fc-ref-date strong { color: #0f172a; }
</style>

<!-- POPUP HOÀN THÀNH NHANH -->
<SweetSoft:ExtraModal runat="server" ID="mdlFastComplete" Type="Primary" Title="Xác nhận hoàn thành công việc">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlFastComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfCompleteTaskId" runat="server" />
                <asp:HiddenField ID="hdfExpectedEndDate" runat="server" />
                
                <div class="p-2">
                    <h5 class="text-primary mb-3 text-center">
                        <i class="fas fa-check-double me-2"></i>Cập nhật tiến độ
                    </h5>
                    
                    <div class="fc-task-box text-center">
                        <p class="text-muted mb-2" style="font-size: 12.5px;">Bạn đang xác nhận hoàn thành công việc:</p>
                        <p class="fw-bold text-dark fs-6 mb-0"><asp:Literal ID="ltrTaskName" runat="server"></asp:Literal></p>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <span class="fc-ref-date"><i class="far fa-calendar-alt me-1"></i> Dự kiến hoàn thành: <strong class="text-primary"><asp:Literal ID="ltrExpectedEndDate" runat="server"></asp:Literal></strong></span>
                            
                            <label class="form-label fw-bold"><i class="far fa-calendar-check me-1"></i> Ngày hoàn thành thực tế <span class="text-danger">*</span></label>
                            <div class="fc-date-wrapper">
                                <!-- Ô chọn ngày thực tế -->
                                <div style="max-width: 200px; flex-grow: 1;">
                                    <asp:TextBox runat="server" ID="txtActualDate" CssClass="form-control fw-bold text-dark" TextMode="Date"></asp:TextBox>
                                </div>
                                <!-- Nhãn trạng thái (Js tự động đổi) -->
                                <span id="lblDynamicStatus" class="dynamic-status status-ontime"><i class="fas fa-check-circle me-1"></i>Đúng hạn</span>
                            </div>
                        </div>
                    </div>

                    <!-- Khối nhập lý do trễ (Bị ẩn mặc định, Js tự hiện nếu trễ) -->
                    <div id="divLateReason" style="display: none;" class="mb-3">
                        <label class="form-label text-danger fw-bold"><i class="fas fa-pen-alt me-1"></i> Lý do trễ hạn (Bắt buộc) <span class="text-danger">*</span></label>
                        <asp:TextBox runat="server" ID="txtLateReason" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Ngày thực tế lớn hơn dự kiến. Vui lòng nhập lý do..."></asp:TextBox>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterFastComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnConfirmComplete" runat="server" CssClass="btn btn-primary px-4" OnClick="btnConfirmComplete_Click">
                        <i class="fas fa-save me-1"></i> Xác nhận hoàn thành
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    // Logic tính toán trạng thái Real-time bằng JS
    function calculateFastCompleteStatus() {
    var actualDateStr = $('#<%= txtActualDate.ClientID %>').val();
    var expectedDateStr = $('#<%= hdfExpectedEndDate.ClientID %>').val();
    var lblStatus = $('#lblDynamicStatus');
    var divReason = $('#divLateReason');

    if (actualDateStr && expectedDateStr) {
        var actualDate = new Date(actualDateStr);
        var expectedDate = new Date(expectedDateStr);

        // Xóa giờ phút, chỉ so sánh ngày
        actualDate.setHours(0, 0, 0, 0);
        expectedDate.setHours(0, 0, 0, 0);

        if (actualDate > expectedDate) {
            // TRỄ HẠN
            lblStatus.html('<i class="fas fa-exclamation-triangle me-1"></i>Trễ hạn')
                .removeClass('status-ontime')
                .addClass('status-late');
            divReason.slideDown(200);
        } else {
            // ĐÚNG HẠN
            lblStatus.html('<i class="fas fa-check-circle me-1"></i>Đúng hạn')
                .removeClass('status-late')
                .addClass('status-ontime');
            divReason.slideUp(200);
        }
    }
}

    // Bắt sự kiện khi người dùng chọn ngày
    $(document).on('change', '#<%= txtActualDate.ClientID %>', calculateFastCompleteStatus);
</script>