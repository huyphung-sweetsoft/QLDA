<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="CostList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.CostList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fCosts/Controls/CtrlCost.ascx" TagPrefix="SweetSoft" TagName="CtrlCost" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
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

                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNhanVienYeuCau" ReadOnly="true" CssClass="bg-light"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_CREATED) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNgayTao" ReadOnly="true" CssClass="bg-light"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMoTaChiTiet" TextMode="MultiLine" Rows="3"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false"><%= GetResourceText(BackEndResourceKeys.SAVE) %></SweetSoft:ExtraButton>
        </FooterTemplate>
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
            // 2. Lấy giá trị để tính toán (lột bỏ dấu phẩy đi để nhân)
            let donGiaStr = txtDonGia.value.replace(/,/g, '');
            let soLuongStr = document.getElementById('<%= txtSoLuong.ClientID %>').value;

            let donGia = parseInt(donGiaStr, 10) || 0;
            let soLuong = parseInt(soLuongStr, 10) || 0;
            // 3. Tính tổng và gán vào ô Tổng tiền (cũng format dấu phẩy)
            let tongTien = donGia * soLuong;
            document.getElementById('<%= txtTongTien.ClientID %>').value = tongTien.toLocaleString('en-US');
        }
    </script>
</asp:Content>