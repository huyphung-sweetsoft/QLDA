<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.EnumHelper.Defines" %>
<%@ Register Src="~/fProjects/Controls/CtrlDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlDuAn" %>

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
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project list" />
                <SweetSoft:CtrlDuAn runat="server" id="CtrlDuAn1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Project Infomation">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_CODE) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMaDuAn"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenDuAn" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlLoaiDuAn" Required="true" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlKhachHang" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <asp:UpdatePanel runat="server" ID="upHopDong" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="row">
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_NUMBER) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtSoHopDong" OnTextChanged="txtSoHopDong_TextChanged" AutoPostBack="true" Required="false"/>
                                 </div>
                             </div>
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDong" Enabled="false" Required="false"/>
                                 </div>
                             </div>
                             <div class="col-lg-4">
                                 <div class="mb-3">
                                     <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %></label>
                                     <SweetSoft:ExtraTextBox runat="server" ID="txtNgayKy" Enabled="false" Required="false"/>
                                 </div>
                             </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></label>
                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayBatDau" SingleDatePicker="true" PlaceHolder="Select start date" />
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></label>
                        <SweetSoft:ExtraDateTime runat="server" ID="dtNgayKetThuc" SingleDatePicker="true" PlaceHolder="Select end date" />
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" PlaceHolder="Select status" />
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROJECT_MANAGEMENT) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVienQuanLy" Required="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                        <CKEditor:CKEditorControl ID="txtMoTa" Width="100%" CssClass="ck-editor"
                            Toolbar="Full" BodyId="StatucPageContent" Language="vi-VN" AutoParagraph="false"
                            BasePath="~/Styles/plugins/ckeditor/" runat="server" Height="200">
                        </CKEditor:CKEditorControl>
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
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        (function () {
            const STATUS_CHO_THUC_HIEN =
                '<%= (byte)DuAnStatus.ChoThucHien %>';

            const STATUS_DANG_THUC_HIEN =
                '<%= (byte)DuAnStatus.DangThucHien %>';

            function parseVietnameseDate(value) {
                if (!value)
                    return null;

                // Trường hợp control có thêm giờ
                value = value.trim().split(' ')[0];

                const parts =
                    value.split(/[\/\-]/);

                if (parts.length !== 3)
                    return null;

                let day;
                let month;
                let year;

                // yyyy-MM-dd
                if (parts[0].length === 4) {
                    year = parseInt(parts[0], 10);
                    month = parseInt(parts[1], 10);
                    day = parseInt(parts[2], 10);
                }
                // dd/MM/yyyy
                else {
                    day = parseInt(parts[0], 10);
                    month = parseInt(parts[1], 10);
                    year = parseInt(parts[2], 10);
                }

                if (!day || !month || !year)
                    return null;

                const result =
                    new Date(year, month - 1, day);

                // Kiểm tra ngày không hợp lệ
                if (
                    result.getFullYear() !== year ||
                    result.getMonth() !== month - 1 ||
                    result.getDate() !== day
                ) {
                    return null;
                }

                result.setHours(0, 0, 0, 0);

                return result;
            }

            function getInput(controlId) {
                const root = $('#' + controlId);

                if (root.is('input'))
                    return root;

                return root.find('input').first();
            }

            function getDropdown(controlId) {
                const root = $('#' + controlId);

                if (root.is('select'))
                    return root;

                return root.find('select').first();
            }

            function updateProjectStatus() {
                const dateInput =
                    getInput(
                        '<%= dtNgayBatDau.ClientID %>');

                const statusDropdown =
                    getDropdown(
                        '<%= ddlTrangThai.ClientID %>');

                if (
                    dateInput.length === 0 ||
                    statusDropdown.length === 0
                ) {
                    return;
                }

                const startDate =
                    parseVietnameseDate(
                        dateInput.val());

                if (!startDate)
                    return;

                const today = new Date();
                today.setHours(0, 0, 0, 0);

                const statusValue =
                    startDate > today
                        ? STATUS_CHO_THUC_HIEN
                        : STATUS_DANG_THUC_HIEN;

                statusDropdown
                    .val(statusValue)
                    .trigger('change');
            }

            function registerProjectStatusEvent() {
                const dateInput =
                    getInput(
                        '<%= dtNgayBatDau.ClientID %>');

                dateInput
                    .off('.projectStatus')
                    .on(
                        'change.projectStatus ' +
                        'input.projectStatus ' +
                        'apply.daterangepicker.projectStatus',
                        updateProjectStatus
                    );

                updateProjectStatus();
            }

            $(document).ready(function () {
                registerProjectStatusEvent();
            });

            // Đăng ký lại sau UpdatePanel PostBack
            if (typeof Sys !== 'undefined') {
                Sys.WebForms.PageRequestManager
                    .getInstance()
                    .add_endRequest(function () {
                        registerProjectStatusEvent();
                    });
            }
        })();
    </script>
</asp:Content>
