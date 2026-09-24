<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="HopDongThucHienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fExecuteContracts.HopDongThucHienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
 <div class="container-fluid">
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h4 class="card-title mb-0">Thông tin hợp đồng</h4>
                    </div>

                    <div class="card-body">
                        <asp:Panel runat="server" ID="pnlContract" CssClass="row js-validation validationEngineContainer">

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

                            <asp:Panel runat="server" ID="pnlContractDocumentIdentityLocked" CssClass="col-lg-12" Visible="false">
                                <div class="alert alert-info py-2 mb-3" role="alert">
                                    <i class="fas fa-info-circle me-1"></i>
                                    <%= GetResourceText(BackEndResourceKeys.CONTRACT_DOCUMENT_IDENTITY_LOCKED) %>
                                </div>
                            </asp:Panel>

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
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDong" Required="true" PlaceHolder="Nhập giá trị hợp đồng"></SweetSoft:ExtraTextBox>
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
                            <div class="col-lg-12">
                                <div class="mb-3">
                                    <label class="form-label label-valid">Nội dung hợp đồng</label>
                                    <asp:RadioButtonList runat="server" ID="rblLoaiNoiDungHopDong" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="d-flex gap-4" AutoPostBack="true" OnSelectedIndexChanged="rblLoaiNoiDungHopDong_SelectedIndexChanged">
                                        <asp:ListItem Text="Soạn thảo nội dung" Value="SOAN_THAO"></asp:ListItem>
                                        <asp:ListItem Text="Tải file" Value="TAI_FILE"></asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                            </div>

<asp:Panel runat="server" ID="pnlSoanThao" Visible="false">
    <div style="width:100%;">
        <label class="form-label">Nội dung hợp đồng</label>
        <CKEditor:CKEditorControl runat="server" ID="txtNoiDungHopDong" Width="100%" CssClass="ck-editor"
            Toolbar="Full" Language="vi-VN" AutoParagraph="false"
            BasePath="~/Styles/plugins/ckeditor/" Height="500">
        </CKEditor:CKEditorControl>
    </div>
</asp:Panel>

<asp:Panel runat="server" ID="pnlTaiFile">
    <div style="width:100%;">
        <label class="form-label">File hợp đồng</label>
        <SweetSoft:FilesBox runat="server" ID="fbHopDong" IsMultiple="false" />
        <div class="form-text">
            Chấp nhận file PDF, DOC, DOCX.
        </div>
    </div>
</asp:Panel>
                        </asp:Panel>

                        <div class="d-flex justify-content-end gap-2 mt-3">
                            <asp:LinkButton runat="server" ID="lbtCancel" CssClass="btn btn-light" OnClick="lbtCancel_Click" CausesValidation="false">Hủy</asp:LinkButton>
                            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click">Lưu</SweetSoft:ExtraButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
