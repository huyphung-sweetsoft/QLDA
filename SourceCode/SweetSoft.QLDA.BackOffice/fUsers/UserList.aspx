<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.UserList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fUsers/Controls/CtrlUsers.ascx" TagPrefix="SweetSoft" TagName="CtrlUsers" %>


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
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlUsers runat="server" id="CtrlUsers1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Account Information" DefaultButton="lbtSubmit">
    <ContentTemplate>
                <div class="row js-validation validationEngineContainer">
                
                    <!-- CÔNG TẮC PHÂN LUỒNG TÀI KHOẢN -->
                    <div class="col-lg-12 mb-3">
                        <div class="form-check form-switch form-switch-md" dir="ltr">
                            <input class="form-check-input" type="checkbox" id="chkLaNhanVien" runat="server" checked="checked" onclick="CMSMasterJs.ToggleEmployeeInfo(this);">
                            <label class="form-check-label fw-bold text-primary fs-5" for="<%= chkLaNhanVien.ClientID %>">
                                <%= GetResourceText(BackEndResourceKeys.EMPLOYEE_ACCOUNT) %>
                            </label>
                        </div>
                    </div>
                
                    <!-- KHỐI 1: THÔNG TIN TÀI KHOẢN -->
                    <div class="col-lg-12 mb-3">
                        <fieldset class="fieldset-box">
                            <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %></legend>
                            <div class="row">
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                                        <div class="input-group"> 
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="true" MaxLength="50" RequiredAdvanced="custom[username]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                            <a class="btn btn-warning" href="javascript:;" onclick="CMSMasterJs.GenerateUserName();" title="<%= GetResourceText(BackEndResourceKeys.GENERATE) %>">
                                                <i class="fas fa-bolt"></i>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %></label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label label-valid">Email</label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true"
                                            RequiredAdvanced="custom[email]" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlRole" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                        <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" OnText="Active" OffText="Lock" Checked="true" />
                                    </div>
                                </div>
                                <div runat="server" id="divImage" visible="false" class="col-lg-6">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IMAGE) %></label>
                                        <SweetSoft:FilesBox runat="server" ID="fbImage" />
                                    </div>
                                </div>
                            
                                <!-- ĐỔI MẬT KHẨU (Đã được đưa gọn vào trong Khối 1) -->
                                <div runat="server" id="divChangePassword" visible="false" class="col-lg-12">
                                    <div class="form-check mb-3">
                                        <input class="form-check-input" type="checkbox" id="chkChangePassword" runat="server" onclick="CMSMasterJs.ChangePassword(this);">
                                        <label class="form-check-label" for="<%= chkChangePassword.ClientID %>">
                                            <%= GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD) %>
                                        </label>
                                    </div>
                                </div>
                                <div runat="server" id="divPassword" data-selector="password" class="col-lg-12">
                                    <div class="row">
                                        <div class="col-lg-6">
                                            <div class="mb-3">
                                                <label for="<%= txtPassword.ClientID %>" class="form-label"><%= GetResourceText(BackEndResourceKeys.PASSWORD) %></label>
                                                <SweetSoft:ExtraTextBox runat="server" ID="txtPassword" TextMode="Password" PlaceHolder="Enter the value"
                                                    Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                            </div>
                                        </div>
                                        <div class="col-lg-6">
                                            <div class="mb-3">
                                                <label for="<%= txtConfirmPassword.ClientID %>" class="form-label">
                                                    <%= GetResourceText(BackEndResourceKeys.CONFIRM_PASSWORD) %>
                                                </label>
                                                <SweetSoft:ExtraTextBox runat="server" ID="txtConfirmPassword" TextMode="Password" PlaceHolder="Enter the value"
                                                    Autocomplete="new-password"></SweetSoft:ExtraTextBox>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div> <!-- Kết thúc dòng của Khối 1 -->
                        </fieldset>
                    </div>
                
                    <!-- KHỐI 2: THÔNG TIN NHÂN SỰ -->
                    <div class="col-lg-12 mb-3" id="boxEmployeeInfo">
                        <fieldset class="fieldset-box">
                            <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.PERSONAL_INFORMATION) %></legend>
                            <div class="row">
                                <div class="col-lg-6 mb-3">
                                    <label id="lblCCCD" class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtCCCD" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></label>
                                    <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlPhongBan" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlChucDanh" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" SimpleInit="true">
                                        <asp:ListItem Text="-- Chọn --" Value=""></asp:ListItem>
                                        <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                                        <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                                    </SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-lg-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE) %></label>
                                   <asp:TextBox runat="server" ID="txtNgayGiaNhap" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-lg-12 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ADDRESS) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                </div>
                            </div> <!-- Kết thúc dòng của Khối 2 -->
                        </fieldset>
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
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        CMSMasterJs.ChangePassword = function (t) {
            $('[data-selector="password"]').toggleClass('show');
        }
        CMSMasterJs.HideChangePwd = function (t) {
            $('[data-selector="password"]').removeClass('show');
        }
        // 1. Hàm Tắt/Mở và làm mờ Khối thông tin nhân sự
        CMSMasterJs.ToggleEmployeeInfo = function (t) {
            var isChecked = $(t).is(':checked');
            var $box = $('#boxEmployeeInfo');

            if (isChecked) {
                $box.css('opacity', '1');
                $box.css('pointer-events', 'auto');
                $('#lblCCCD').addClass('label-valid');
            } else {
                $box.css('opacity', '0.5');
                $box.css('pointer-events', 'none');
                $box.find('input[type="text"], input[type="date"]').val('');
                $box.find('select').val('').trigger('change');
                $('#lblCCCD').removeClass('label-valid');
            }
        }

        // 2. Hàm Tự sinh UserName 
        CMSMasterJs.GenerateUserName = function () {
            var fullName = $('#<%= txtFullName.ClientID %>').val();
            if (!fullName) {
                alert('Vui lòng nhập Tên hiển thị trước!');
                return;
            }
    
            var str = fullName.toLowerCase();
            str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
            str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
            str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
            str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
            str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
            str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
            str = str.replace(/đ/g, "d");
            str = str.replace(/[^a-z0-9]/g, "");
    
            $('#<%= txtUserName.ClientID %>').val(str);
        }
        $(document).ready(function () {
            CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
        });
    </script>
</asp:Content>
