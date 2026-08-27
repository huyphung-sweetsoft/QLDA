<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="ListNhanVien.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.ListNhanVien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fNhanVien/Controls/CtrlNhanViens.ascx" TagPrefix="SweetSoft" TagName="CtrlNhanViens" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        div[data-edit="true"] {
            display: none;
        }

            div[data-edit="true"].show {
                display: block;
            }
            .file-box-single{
                width:100px;
            }
            .file-box .uploaded-content .item img{
                width: 60px;
            }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="NhanVien List" />
                <SweetSoft:CtrlNhanViens runat="server" ID="CtrlNhanViens1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin nhân viên" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <!-- THÔNG TIN ĐỊNH DANH -->
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenNhanVien" Required="true" MaxLength="100" PlaceHolder="Nhập tên nhân viên"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtCCCD" Required="true" MaxLength="12" PlaceHolder="Nhập số CCCD"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- TÀI KHOẢN & LIÊN HỆ -->
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid">Email (Tài khoản hệ thống)</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true" RequiredAdvanced="custome[email]" PlaceHolder="Nhập email"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true" PlaceHolder="Nhập số điện thoại"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- TỔ CHỨC & QUYỀN HẠN -->
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlPhongBan" Required="true" SimpleInit="true" PlaceHolder="Chọn phòng ban"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlChucDanh" Required="true" SimpleInit="true" PlaceHolder="Chọn chức danh"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <!-- THỜI GIAN & ẢNH ĐẠI DIỆN -->
        <!-- DÒNG 2: THỜI GIAN -->
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label">Ngày sinh</label>
                        <!-- Dùng TextBox chuẩn của ASP.NET ép kiểu HTML5 date -->
                        <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label">Ngày gia nhập</label>
                        <!-- Dùng TextBox chuẩn của ASP.NET ép kiểu HTML5 date -->
                        <asp:TextBox runat="server" ID="txtNgayGiaNhap" type="date" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <!-- Bổ sung Giới tính và Địa chỉ -->
                 <div class="col-lg-4">
                 <div class="mb-3">
                     <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                     <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" Required="true" SimpleInit="true" PlaceHolder="Chọn giới tính">
                        <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                        <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                        <asp:ListItem Text="Khác" Value="Khác"></asp:ListItem>
                    </SweetSoft:ExtraDropdown>
                 </div>
             </div>
                <div class="col-lg-8">
                    <div class="mb-3">
                        <label class="form-label">Địa chỉ</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" MaxLength="255" PlaceHolder="Nhập địa chỉ chi tiết"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div runat="server" id="divImage" class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IMAGE) %></label>
                        <SweetSoft:FilesBox runat="server" ID="fbImage" />
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
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
        });
    </script>
</asp:Content>