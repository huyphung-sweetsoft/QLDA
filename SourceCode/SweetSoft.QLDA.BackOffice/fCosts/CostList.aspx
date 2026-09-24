<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="CostList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.CostList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Register Src="~/fCosts/Controls/CtrlCost.ascx" TagPrefix="SweetSoft" TagName="CtrlCost" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    .badge-status {
        padding: 4px 10px !important;
        border-radius: 6px !important;
        font-size: 11.5px !important;
        font-weight: 600 !important;
        display: inline-block !important;
        white-space: nowrap !important;
        line-height: 1.2 !important;
    }

    .badge-status-pending {
        background-color: #fffbeb !important;
        color: #b45309 !important;
        border: 1px solid #fde68a !important;
    }

    .badge-status-approved {
        background-color: #dcfce7 !important;
        color: #15803d !important;
        border: 1px solid #bbf7d0 !important;
    }
    .badge-status-rejected {
        background-color: #fee2e2 !important;
        color: #dc2626 !important;
        border: 1px solid #fca5a5 !important;
    }
    .record-attachments .file-actions { display: none !important; }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlCost runat="server" id="CtrlCost1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" DefaultButton="lbtSubmit">
        <ContentTemplate>
           <div class="row js-validation validationEngineContainer">
                
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.COST_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenKhoanChi" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PRICE) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtDonGia" Required="true"
                            onkeyup="calculateTotalCost(this);" onchange="calculateTotalCost(this);"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.QUANTITY) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtSoLuong" Required="true" TextMode="Number" min="1" 
                            oninput="calculateTotalCost();"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.TOTAL_AMOUNT) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTongTien" ReadOnly="true" 
                            CssClass="text-end fw-bold bg-light" PlaceHolder="0"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNhanVienYeuCau" ReadOnly="true" CssClass="bg-light"></SweetSoft:ExtraTextBox>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVienYeuCau"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label">Người tạo</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNguoiTao" ReadOnly="true" CssClass="bg-light fw-bold"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_CREATED) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNgayTao" ReadOnly="true" CssClass="bg-light"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-12" id="divLyDoTuChoi" runat="server" style="display: none;">
                    <div class="mb-3">
                        <label class="form-label text-danger fw-bold">Lý do từ chối <span class="text-danger">*</span></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtLyDoTuChoi" TextMode="MultiLine" Rows="2" CssClass="bg-white text-danger border-danger" PlaceHolder="Bắt buộc nhập lý do từ chối..."></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></label>
                        <CKEditor:CKEditorControl runat="server" ID="txtMoTaChiTiet" Width="100%"
                            CssClass="ck-editor" Toolbar="Full" Language="vi-VN"
                            AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/" Height="200" />
                        <div class="form-text">File đính kèm được tải bằng nút thư mục của khoản chi phí trong danh sách.</div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false"><%= GetResourceText(BackEndResourceKeys.SAVE) %></SweetSoft:ExtraButton>
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
            let txtDonGia = document.getElementById('<%= txtDonGia.ClientID %>');
            if (donGiaInput) {
                let val = txtDonGia.value.replace(/\D/g, '');
                if (val !== '') {
                    txtDonGia.value = parseInt(val, 10).toLocaleString('en-US'); 
                } else {
                    txtDonGia.value = '';
                }
            }
            let donGiaStr = txtDonGia.value.replace(/,/g, '');
            let soLuongStr = document.getElementById('<%= txtSoLuong.ClientID %>').value;

            let donGia = parseInt(donGiaStr, 10) || 0;
            let soLuong = parseInt(soLuongStr, 10) || 0;
            let tongTien = donGia * soLuong;
            document.getElementById('<%= txtTongTien.ClientID %>').value = tongTien.toLocaleString('en-US');
        }

        $(document).on('input', '.format-currency', function (e) {
            let val = $(this).val();
            val = val.replace(/[^0-9]/g, '');
            if (val !== '') {
                val = parseInt(val, 10).toLocaleString('en-US');
            }
            $(this).val(val);
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            function toggleRejectReason() {
                let statusVal = $('#<%= ddlTrangThai.ClientID %>').val();
                if (statusVal === '2') {
                    $('#<%= divLyDoTuChoi.ClientID %>').slideDown();
                } else {
                    $('#<%= divLyDoTuChoi.ClientID %>').slideUp();
                }
            }

            $('#<%= ddlTrangThai.ClientID %>').off('change').on('change', function () {
                toggleRejectReason();
            });

        });
    </script>
</asp:Content>
