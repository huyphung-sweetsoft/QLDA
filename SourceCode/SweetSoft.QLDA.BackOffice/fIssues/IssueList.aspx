<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="IssueList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.IssueList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fIssues/Controls/CtrlIssue.ascx" TagPrefix="SweetSoft" TagName="CtrlIssue" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlIssue runat="server" id="CtrlIssue1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin vấn đề" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
        
                <!-- HÀNG 1: Tên vấn đề -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid">Tên vấn đề</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenVanDe" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- HÀNG 2: Mức độ ảnh hưởng & Nguồn gốc vấn đề -->
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Mức độ ảnh hưởng</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Nguồn gốc vấn đề</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlNguonGocVanDe" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <!-- HÀNG 3: Công việc phát sinh (Thêm sự kiện AutoPostBack vào đây) -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Công việc phát sinh</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecPhatSinh" 
                            SimpleInit="true"
                            AutoPostBack="true" 
                            OnSelectedIndexChanged="ddlCongViecPhatSinh_SelectedIndexChanged">
                        </SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <!-- HÀNG 4: Công việc bị ảnh hưởng (Xóa sự kiện) -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Công việc bị ảnh hưởng</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecBiAnhHuong" SimpleInit="true"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <!-- HÀNG 5: Nhân viên xử lý -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Nhân viên xử lý</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNhanVien" ReadOnly="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- HÀNG 6: Mô tả chi tiết -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Mô tả chi tiết</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMoTaChiTiet" TextMode="MultiLine" Rows="3"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- HÀNG 7: Kế hoạch xử lý -->
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label">Kế hoạch xử lý</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachXuLy" TextMode="MultiLine" Rows="3"></SweetSoft:ExtraTextBox>
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