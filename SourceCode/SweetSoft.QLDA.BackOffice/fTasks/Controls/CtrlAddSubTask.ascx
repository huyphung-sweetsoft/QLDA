<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlAddSubTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlAddSubTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .subtask-context {
        background-color: #faf5ff; border: 1px solid #e9d5ff; border-radius: 8px;
        padding: 12px 16px; margin-bottom: 20px; border-left: 4px solid #a855f7;
    }
    .subtask-code { background: #e9d5ff; color: #7e22ce; padding: 4px 10px; border-radius: 6px; font-weight: 800; font-size: 15px;}
    .end-date-box { background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 6px; padding: 8px 12px; height: 100%; display: flex; flex-direction: column; justify-content: center; }
    .end-date-label { font-size: 11px; color: #166534; font-weight: 700; text-transform: uppercase; margin-bottom: 2px;}
    .end-date-value { font-size: 16px; color: #15803d; font-weight: 800; }
    .context-divider { border-right: 1px dashed #d8b4fe; }
    @media (max-width: 768px) {
        .context-divider { border-right: none; border-bottom: 1px dashed #d8b4fe; padding-bottom: 10px; margin-bottom: 10px; }
    }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlAddSubTask" Type="Primary" Title="Thêm Công việc con">
    <ContentTemplate>
        <asp:UpdatePanel ID="upAddSubTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3">
                    <asp:HiddenField ID="hfParentId" runat="server" />

                    <div class="subtask-context">
                        <div class="row align-items-center">
                            <div class="col-md-6 context-divider">
                                <span class="text-muted d-block mb-1" style="font-size: 11px; font-weight: 700;">
                                    <i class="fas fa-layer-group me-1"></i> THUỘC GIAI ĐOẠN
                                </span>
                                <span class="fw-bold text-dark" style="font-size: 13.5px;">
                                    <asp:Literal ID="ltrPhaseName" runat="server"></asp:Literal>
                                </span>
                            </div>
                            <div class="col-md-6 ps-md-3 mt-2 mt-md-0">
                                <span class="text-muted d-block mb-1" style="font-size: 11px; font-weight: 700;">
                                    <i class="fas fa-sitemap me-1"></i> CÔNG VIỆC CHA
                                </span>
                                <span class="fw-bold text-dark" style="font-size: 13.5px;">
                                    <asp:Literal ID="ltrParentTaskName" runat="server"></asp:Literal>
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-3">
                            <label class="form-label fw-bold">Mã CV</label>
                            <div class="subtask-code text-center"><asp:Literal ID="ltrSubTaskCode" runat="server"></asp:Literal></div>
                            <asp:TextBox ID="txtMaCv" runat="server" style="display:none;"></asp:TextBox>
                        </div>
                        <div class="col-md-9">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTenCongViec" runat="server" CssClass="form-control form-control-lg" placeholder="Nhập tên công việc con..."></asp:TextBox>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-6">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                            <asp:DropDownList ID="ddlPhuThuoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPhuThuoc_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></label>
                            <asp:DropDownList ID="ddlDoUuTien" runat="server" CssClass="form-select"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-4">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.START_DATE) %><span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtNgayBatDau" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtNgayBatDau_TextChanged"></asp:TextBox>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DURATION) %> <span class="text-danger">*</span></label>
                            <div class="input-group">
                                <asp:TextBox ID="txtThoiHan" runat="server" CssClass="form-control" TextMode="Number" min="1" AutoPostBack="true" OnTextChanged="txtThoiHan_TextChanged"></asp:TextBox>
                                <span class="input-group-text bg-light">Ngày</span>
                            </div>
                        </div>
                        <div class="col-md-5">
                            <div class="end-date-box">
                                <div class="end-date-label"><i class="far fa-calendar-check me-1"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></div>
                                <div class="end-date-value">
                                    <asp:Label ID="lblNgayKetThuc" runat="server">--/--/----</asp:Label>
                                    <asp:TextBox ID="txtNgayKetThuc" runat="server" style="display:none;"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                            <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterAddSubTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:LinkButton ID="btnSaveSubTask" runat="server" CssClass="btn btn-primary" OnClick="btnSaveSubTask_Click">
                    <i class="fas fa-save me-1"></i> <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </asp:LinkButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    function calculateEndDateForSubTask() {
        var startDateInput = $('input[id$="txtNgayBatDau"]');
        var durationInput = $('input[id$="txtThoiHan"]');
        var endDateInput = $('input[id$="txtNgayKetThuc"]');
        var endDateLabel = $('span[id$="lblNgayKetThuc"]');

        if (startDateInput.length === 0) return;

        var startDateStr = startDateInput.val();
        var durationStr = durationInput.val();
        var minDateStr = startDateInput.attr('min');

        if (startDateStr) {
            if (minDateStr && startDateStr < minDateStr) {
                startDateInput.val(minDateStr);
                startDateStr = minDateStr;
            }

            if (durationStr) {
                var days = parseInt(durationStr);
                if (!isNaN(days) && days > 0) {
                    var date = new Date(startDateStr);
                    date.setDate(date.getDate() + (days - 1));
                    var d = ("0" + date.getDate()).slice(-2);
                    var m = ("0" + (date.getMonth() + 1)).slice(-2);
                    var y = date.getFullYear();

                    endDateInput.val(y + '-' + m + '-' + d);
                    endDateLabel.text(d + '/' + m + '/' + y);
                }
            }
        }
    }

    // Gắn sự kiện thay đổi cho các input trong toàn bộ ngữ cảnh trang / modal
    $(document).off('change keyup', 'input[id$="txtNgayBatDau"], input[id$="txtThoiHan"]')
        .on('change keyup', 'input[id$="txtNgayBatDau"], input[id$="txtThoiHan"]', function () {
            calculateEndDateForSubTask();
        });

    // Kích hoạt tính toán khi DOM sẵn sàng hoặc modal load xong
    $(document).ready(function () {
        calculateEndDateForSubTask();
    });

    // Hỗ trợ tương thích với vòng đời Sys.WebForms của UpdatePanel
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            calculateEndDateForSubTask();
        });
    }
</script>