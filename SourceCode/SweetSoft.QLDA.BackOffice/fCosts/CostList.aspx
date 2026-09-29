<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="CostList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.CostList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Register Src="~/fCosts/Controls/CtrlCost.ascx" TagPrefix="SweetSoft" TagName="CtrlCost" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    .record-attachments .file-actions { display: none !important; }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlCost runat="server" ID="CtrlCost1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="cost-form">

                <div class="cost-form-section">
                    <div class="cost-form-section-header">
                        <div>
                            <div class="cost-form-section-title"><i class="fas fa-receipt me-2"></i>Thông tin khoản chi</div>
                            <div class="cost-form-section-subtitle">Nhập thông tin và giá trị của khoản chi phí</div>
                        </div>
                    </div>
                    <div class="row g-3">
                        <div class="col-lg-12">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.COST_NAME) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtTenKhoanChi" Required="true" CssClass="cost-input"></SweetSoft:ExtraTextBox>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PRICE) %></label>
                            <div class="cost-input-money">
                                <SweetSoft:ExtraTextBox runat="server" ID="txtDonGia" Required="true" onkeyup="calculateTotalCost(this);" onchange="calculateTotalCost(this);" CssClass="text-end"></SweetSoft:ExtraTextBox>
                                <span>₫</span>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.QUANTITY) %></label>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtSoLuong" Required="true" TextMode="Number" min="1" oninput="calculateTotalCost();" CssClass="text-end"></SweetSoft:ExtraTextBox>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.TOTAL_AMOUNT) %></label>
                            <div class="cost-total-box">
                                <div class="cost-total-icon"><i class="fas fa-calculator"></i></div>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtTongTien" ReadOnly="true" PlaceHolder="0" CssClass="cost-total-input"></SweetSoft:ExtraTextBox>
                                <span class="cost-total-unit">₫</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="cost-form-section">
                    <div class="cost-form-section-header">
                        <div>
                            <div class="cost-form-section-title"><i class="fas fa-user-check me-2"></i>Xử lý khoản chi</div>
                            <div class="cost-form-section-subtitle">Người yêu cầu và trạng thái phê duyệt</div>
                        </div>
                    </div>
                    <div class="row g-3">
                        <div class="col-lg-6">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                            <asp:Panel runat="server" ID="pnlRequesterCard" CssClass="cost-person-card">
                                <asp:Literal runat="server" ID="litRequesterAvatar"></asp:Literal>
                                <div class="cost-person-main">
                                    <div class="cost-person-name"><asp:Literal runat="server" ID="litRequesterName"></asp:Literal></div>
                                    <div class="cost-person-email"><asp:Literal runat="server" ID="litRequesterEmail"></asp:Literal></div>
                                </div>
                            </asp:Panel>
                            <div style="display:none;">
                                <SweetSoft:ExtraTextBox runat="server" ID="txtNhanVienYeuCau" ReadOnly="true"></SweetSoft:ExtraTextBox>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVienYeuCau"></SweetSoft:ExtraDropdown>
                            </div>
                        </div>

                        <div class="col-lg-6">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai"></SweetSoft:ExtraDropdown>
                        </div>

                        <div class="col-lg-6">
                            <label class="form-label">Người tạo</label>
                            <div class="cost-readonly-field">
                                <i class="fas fa-user-edit"></i>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtNguoiTao" ReadOnly="true" CssClass="bg-transparent border-0 fw-semibold"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>

                        <div class="col-lg-6">
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_CREATED) %></label>
                            <div class="cost-readonly-field">
                                <i class="far fa-calendar-alt"></i>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtNgayTao" ReadOnly="true" CssClass="bg-transparent border-0"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>

                        <div class="col-lg-12" id="divLyDoTuChoi" runat="server" style="display:none;">
                            <div class="cost-reject-box">
                                <div class="cost-reject-title"><i class="fas fa-exclamation-circle me-1"></i>Lý do từ chối <span>*</span></div>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtLyDoTuChoi" TextMode="MultiLine" Rows="3" CssClass="border-danger text-danger" PlaceHolder="Bắt buộc nhập lý do từ chối..."></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="cost-form-section">
                    <div class="cost-form-section-header">
                        <div>
                            <div class="cost-form-section-title"><i class="fas fa-align-left me-2"></i>Nội dung chi phí</div>
                            <div class="cost-form-section-subtitle">Mô tả chi tiết và thông tin bổ sung</div>
                        </div>
                    </div>
                    <div>
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></label>
                        <CKEditor:CKEditorControl runat="server" ID="txtMoTaChiTiet" Width="100%" CssClass="ck-editor" Toolbar="Full" Language="vi-VN" AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/" Height="200" />
                        <div class="cost-form-hint"><i class="fas fa-paperclip me-1"></i>File đính kèm được tải bằng nút thư mục của khoản chi phí trong danh sách.</div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false"><%= GetResourceText(BackEndResourceKeys.SAVE) %></SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal runat="server" ID="dlCostFiles" Type="Primary" Size="Small" Title="File đính kèm chi phí">
        <ContentTemplate>
            <div class="record-attachments">
                <SweetSoft:FilesBox runat="server" ID="fbCostFiles" IsMultiple="false" MaxFileSizeBytes="10485760" />
            </div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
<script type="text/javascript">
    function calculateTotalCost(donGiaInput) {
        const txtDonGia = document.getElementById('<%= txtDonGia.ClientID %>');
        const txtSoLuong = document.getElementById('<%= txtSoLuong.ClientID %>');
        const txtTongTien = document.getElementById('<%= txtTongTien.ClientID %>');
        if (!txtDonGia || !txtSoLuong || !txtTongTien) return;
        if (donGiaInput) {
            const value = txtDonGia.value.replace(/\D/g, '');
            txtDonGia.value = value !== '' ? parseInt(value, 10).toLocaleString('en-US') : '';
        }
        const donGia = parseInt(txtDonGia.value.replace(/,/g, ''), 10) || 0;
        const soLuong = parseInt(txtSoLuong.value, 10) || 0;
        txtTongTien.value = (donGia * soLuong).toLocaleString('en-US');
    }

    $(document).on('input', '.format-currency', function () {
        let value = $(this).val().replace(/[^0-9]/g, '');
        $(this).val(value !== '' ? parseInt(value, 10).toLocaleString('en-US') : '');
    });

    Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
        const $status = $('#<%= ddlTrangThai.ClientID %>');
        const $rejectReason = $('#<%= divLyDoTuChoi.ClientID %>');
        function toggleRejectReason() {
            if ($status.val() === '2') $rejectReason.stop(true, true).slideDown(180);
            else $rejectReason.stop(true, true).slideUp(180);
        }
        $status.off('change.cost').on('change.cost', toggleRejectReason);
        toggleRejectReason();
    });
</script>
</asp:Content>
