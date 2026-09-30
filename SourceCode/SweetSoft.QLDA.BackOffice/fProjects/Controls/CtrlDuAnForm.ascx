<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDuAnForm.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlDuAnForm" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>  
<%@ Register Src="~/fProjects/Controls/CtrlChonNhanVien.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVien" %>

<SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Project Infomation" Size="Large">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_CODE) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMaDuAn"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenDuAn" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlLoaiDuAn" Required="true" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlKhachHang" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-8">
               <asp:UpdatePanel runat="server" ID="upHopDong" UpdateMode="Conditional">
                    <ContentTemplate>
                        
                            <div class="mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_NUMBER) %></label>
                                <div class="input-group">
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoHopDong" Required="false" />
                                    <asp:Button runat="server" ID="btnSearchHopDong"
                                        OnClick="btnSearchHopDong_Click"
                                        style="display:none;"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false" />
                                    <SweetSoft:ExtraButton runat="server" ID="btnChonHopDong"
                                        ButtonStyle="Secondary"
                                        ButtonIcon="Check"
                                        CausesValidation="false"
                                        OnClick="btnChonHopDong_Click"
                                        ToolTip="Chọn hợp đồng">
                                    </SweetSoft:ExtraButton>
                                </div>
                            </div>
                        
                    </ContentTemplate>
                </asp:UpdatePanel>
                </div>
                <asp:UpdatePanel runat="server" ID="upHopDongInfo" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel runat="server" ID="pnlHopDongInfo" Visible="false" CssClass="col-lg-12">
                            <div class="border rounded p-3 mb-3 position-relative">
                                <button type="button" class="btn-close position-absolute top-0 end-0 m-2"
                                    aria-label="Đóng" onclick="$(this).closest('#<%= pnlHopDongInfo.ClientID %>').hide();">
                                </button>

                                <div class="row pe-4">
                                    <div class="col-lg-3">
                                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.CONTRACT_NAME) %></small>
                                        <div class="fw-semibold">
                                            <asp:Label runat="server" ID="lblTenHopDong" />
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %></small>
                                        <div class="fw-semibold">
                                            <asp:Label runat="server" ID="lblGiaTriHopDong" />
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %></small>
                                        <div class="fw-semibold">
                                            <asp:Label runat="server" ID="lblNgayKyHopDong" />
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <small class="text-muted">Khách hàng</small>
                                        <div class="fw-semibold">
                                            <asp:Label runat="server" ID="lblKhachHangHopDong" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="col-lg-12">
                    <asp:UpdatePanel runat="server" ID="upNgayDuAn" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></label>
                                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayBatDau" SingleDatePicker="true" PlaceHolder="Select start date" />
                                    </div>
                                </div>

                                <div class="col-lg-4">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></label>
                                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayKetThuc" SingleDatePicker="true" PlaceHolder="Select end date" />
                                    </div>
                                </div>

                                <div class="col-lg-4">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" PlaceHolder="Select status" />
                                    </div>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_MANAGEMENT) %></label>
        
                        <!-- Bọc UpdatePanel và bật AutoPostBack để chạy ngầm Ajax khi đổi PM -->
                        <asp:UpdatePanel runat="server" ID="upPM" UpdateMode="Conditional">
                            <ContentTemplate>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVienQuanLy" Required="true" 
                                    PlaceHolder="Select the value"
                                    AutoPostBack="true" 
                                    OnSelectedIndexChanged="ddlNhanVienQuanLy_SelectedIndexChanged">
                                </SweetSoft:ExtraDropdown>
                            </ContentTemplate>
                        </asp:UpdatePanel>

                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %></label>
                        <asp:UpdatePanel runat="server" ID="upNhanVienThamGia" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="input-group">
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoLuongNhanVien" Enabled="false" PlaceHolder="Chưa chọn nhân viên nào" />
                                    <!-- Xóa chữ, đổi ButtonIcon thành UserPlus (tương đương person-add) -->
                                    <SweetSoft:ExtraButton runat="server" ID="btnChonNhanVien" ButtonStyle="Secondary" ButtonIcon="UserPlus"
                                        CausesValidation="false" OnClick="btnChonNhanVien_Click" ToolTip="Thêm thành viên">
                                    </SweetSoft:ExtraButton>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                        <CKEditor:CKEditorControl ID="txtMoTa" Width="100%" CssClass="ck-editor"
                            Toolbar="Full" BodyId="StatucPageContent" Language="vi-VN" AutoParagraph="false"
                            BasePath="~/Styles/plugins/ckeditor/" runat="server" Height="100">
                        </CKEditor:CKEditorControl>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                        OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

<SweetSoft:CtrlChonNhanVien runat="server" ID="CtrlChonNhanVien1"/>

<script>
    function initProjectDateSync() {
        var $startDate = $('#<%= dtNgayBatDau.ClientID %>');
        var $endDate = $('#<%= dtNgayKetThuc.ClientID %>');
        var $status = $('#<%= ddlTrangThai.ClientID %>');

        if ($startDate.length && $endDate.length) {
            // Remove previously attached handlers to avoid duplicates after UpdatePanel refresh
            $startDate.off('apply.daterangepicker.syncDates');

            $startDate.on('apply.daterangepicker.syncDates', function (ev, picker) {
                var pickerEnd = $endDate.data('daterangepicker');

                if (picker.startDate) {
                    var today = moment().startOf('day');
                    var startDate = picker.startDate.clone().startOf('day');

                    // Ngày bắt đầu > ngày hiện tại: Chưa bắt đầu (0)
                    // Ngày bắt đầu <= ngày hiện tại: Đang thực hiện (1)
                    if ($status.length) {
                        $status.val(startDate.isAfter(today, 'day') ? '0' : '1').trigger('change');
                    }

                    if (pickerEnd) {
                        pickerEnd.minDate = picker.startDate.clone();

                        // If current end date is before new start date, update it
                        if (pickerEnd.startDate && pickerEnd.startDate.isBefore(picker.startDate, 'day')) {
                            pickerEnd.setStartDate(picker.startDate.clone());
                            pickerEnd.setEndDate(picker.startDate.clone());

                            // Trigger apply manually since setStartDate doesn't fire it automatically
                            $endDate.trigger('apply.daterangepicker', pickerEnd);
                        }
                    }
                }
            });
        }
    }

    $(document).ready(function () {
        initProjectDateSync();
    });

    // Re-init after UpdatePanel refresh
    if (typeof Sys !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initProjectDateSync();
        });
    }
</script>   