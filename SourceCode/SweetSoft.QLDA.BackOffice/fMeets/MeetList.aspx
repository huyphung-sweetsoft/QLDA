<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlMeet runat="server" id="CtrlMeet1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin cuộc họp" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
        
                <!-- HÀNG 1: Tên cuộc họp & Trạng thái -->
                <div class="col-lg-8">
                    <div class="mb-3">
                        <label class="form-label label-valid">Tên cuộc họp</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenCuocHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid">Trạng thái</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <!-- HÀNG 2: Thời gian (Cả 2 đều bắt buộc nhập) -->
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Thời gian bắt đầu</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianBatDau" Required="true" CssClass="datepicker-time"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Thời gian kết thúc</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianKetThuc" Required="true" CssClass="datepicker-time"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- HÀNG 3: Địa điểm (Bắt buộc nhập) -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid">Địa điểm họp</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtDiaDiemHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- HÀNG 4: Nội dung -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Nội dung cuộc họp</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNoiDungCuocHop" TextMode="MultiLine" Rows="4"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server"></asp:Content>