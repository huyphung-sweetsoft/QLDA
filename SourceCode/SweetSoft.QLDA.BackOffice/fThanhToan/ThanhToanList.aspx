<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="ThanhToanList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fThanhToan.ThanhToanList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fThanhToan/Controls/CtrlThanhToan.ascx" TagPrefix="SweetSoft" TagName="CtrlThanhToan" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpMain" runat="server">

    <style type="text/css">
        .payment-amount-input {
            text-align: left !important;
        }
    </style>
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlThanhToan runat="server" ID="CtrlThanhToan1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PAYMENT_CODE) %></label>
                    <div class="input-group">
                        <asp:Label runat="server" ID="lblMaDotPrefix"
                            CssClass="input-group-text fw-semibold"
                            Style="border-top-right-radius: 0 !important; border-bottom-right-radius: 0 !important;"
                            Visible="false" />
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMaDot" Required="true" MaxLength="50" />
                    </div>
                </div>
                <div class="col-lg-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PAYMENT_NAME) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenDot" Required="true" MaxLength="255" />
                </div>
                <div class="col-lg-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PAYMENT_AMOUNT) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoTien" Required="true" IsCurrency="true"
                        CssClass="payment-amount-input" />
                </div>
                <div class="col-lg-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PAYMENT_DUE_DATE) %></label>
                    <SweetSoft:ExtraDateTime runat="server" ID="dtHanThanhToan" SingleDatePicker="true"
                        DateFormat="dd/MM/yyyy" AllowNullDate="true" Required="true" Enabled="false" />
                </div>
                <div class="col-lg-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                    <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" Required="true" />
                </div>
                <div class="col-lg-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PAYMENT_ACTUAL_DATE) %></label>
                    <SweetSoft:ExtraDateTime runat="server" ID="dtNgayThanhToan" SingleDatePicker="true"
                        DateFormat="dd/MM/yyyy" AllowNullDate="true" />
                </div>
                <div class="col-12 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NOTE) %></label>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtGhiChu" TextMode="MultiLine" Rows="3" MaxLength="1000" />
                </div>
                <div class="col-12 mb-3">
                    <label class="form-label">Đính kèm file</label>
                    <SweetSoft:FilesBox runat="server" ID="fbPaymentFiles" IsMultiple="true" IsSimpleUpload="true" />
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" ButtonStyle="Primary" ButtonIcon="Save"
                        CssClass="waves-effect waves-light" IsPace="true" Visible="false"
                        OnClientClick="return CMSMasterJs.CheckValid() && PaymentFormSubmit(this);" OnClick="lbtSubmit_Click" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <script type="text/javascript">
        function PaymentFormSubmit(button) {
            if (typeof FilesBox === "undefined")
                return true;

            if (FilesBox.UploadInProgress)
                return false;

            var fileBox = $(".file-box.simple-upload").first();
            if (!fileBox.length || !FilesBox.ValidatedFile || FilesBox.ValidatedFile.length === 0)
                return true;

            if (typeof FilesBox.SimpleUploadComplete === "function")
                return false;

            FilesBox.SimpleUploadComplete = function () {
                var saveButton = document.getElementById(button.id);
                FilesBox.SimpleUploadComplete = null;
                if (saveButton)
                    saveButton.click();
            };

            if (FilesBox.SaveSimpleUpload(fileBox) === true) {
                FilesBox.SimpleUploadComplete = null;
                return true;
            }

            return false;
        }
    </script>
</asp:Content>
