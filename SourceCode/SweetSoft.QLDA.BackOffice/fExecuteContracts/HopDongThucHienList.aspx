<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="HopDongThucHienList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fExecuteContracts.HopDongThucHienList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fExecuteContracts/Controls/CtrlHopDongThucHien.ascx" TagPrefix="SweetSoft" TagName="CtrlHopDongThucHien" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlHopDongThucHien runat="server" ID="CtrlHopDongThucHien1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin hợp đồng" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">

                <%-- Số hợp đồng --%>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Số hợp đồng</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtSoHopDong" Required="true" MaxLength="100" PlaceHolder="Nhập số hợp đồng"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <%-- Tên hợp đồng --%>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Tên hợp đồng</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenHopDong" Required="true" MaxLength="250" PlaceHolder="Nhập tên hợp đồng"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <%-- Khách hàng --%>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Khách hàng</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlKhachHang" Required="true" SimpleInit="true" ValueIsOfTypeGUID="true" PlaceHolder="Chọn khách hàng"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <%-- Giá trị hợp đồng --%>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Giá trị hợp đồng</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDong" Required="true" TextMode="Number" PlaceHolder="Nhập giá trị hợp đồng"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <%-- Ngày ký --%>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid">Ngày ký</label>
                        <asp:TextBox runat="server" ID="txtNgayKy" type="date" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <%-- Ngày hiệu lực --%>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label">Ngày hiệu lực</label>
                        <asp:TextBox runat="server" ID="txtNgayHieuLuc" type="date" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <%-- Ngày hết hạn --%>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label">Ngày hết hạn</label>
                        <asp:TextBox runat="server" ID="txtNgayHetHan" type="date" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <%-- Mô tả --%>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Mô tả</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" TextMode="MultiLine" Rows="4" MaxLength="1000" PlaceHolder="Nhập mô tả"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
            </div>
        </ContentTemplate>

        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
