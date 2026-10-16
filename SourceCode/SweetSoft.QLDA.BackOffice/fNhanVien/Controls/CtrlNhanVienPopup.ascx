<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlNhanVienPopup.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.Controls.CtrlNhanVienPopup" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<SweetSoft:ExtraModal
    runat="server"
    ID="dlDetail"
    Type="Primary"
    Size="ExtraLarge"
    Title="Thông tin chi tiết" 
    DefaultButton="lbtSubmit">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer p-2">
            <div class="col-lg-12">
                
                <!-- KHỐI 1: THÔNG TIN NHÂN VIÊN -->
                <fieldset class="fieldset-box mb-2" style="padding-bottom: 5px; padding-top: 5px;">
                    <legend class="text-primary fw-bold px-1 mb-0">
                        Thông tin nhân viên
                    </legend>
                    <!-- Đổi pt-2 thành pt-0 để hút khoảng trống phía trên -->
                    <div class="row pt-0 mx-0">
                        
                        <!-- CỘT TRÁI: AVATAR -->
                        <div class="col-lg-2 text-center border-end user-popup-avatar">
                            <label class="form-label fw-bold text-muted mb-2 mt-1">
                                <%= GetResourceText(BackEndResourceKeys.IMAGE) %>
                            </label>
                            <div class="avatar-upload mt-1">
                                <SweetSoft:FilesBox runat="server" ID="fbImage" />
                            </div>
                        </div>
                       <!-- CỘT PHẢI: FORM NHẬP LIỆU -->
                        <div class="col-lg-10 pt-2">
    
                            <!-- KHỐI 1: CHỨA HÀNG 1 & HÀNG 2 (Không có UpdatePanel) -->
                            <div class="row px-1">
                                <!-- Hàng 1: Họ tên | Ngày sinh | Giới tính | Điện thoại -->
                                <div class="col-md-3 mb-2">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.FULL_NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtFullName" Required="true"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></label>
                                    <asp:TextBox runat="server" ID="txtNgaySinh" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlGioiTinh" SimpleInit="true">
                                        <asp:ListItem Text="-- Chọn --" Value=""></asp:ListItem>
                                        <asp:ListItem Text="Nam" Value="Nam"></asp:ListItem>
                                        <asp:ListItem Text="Nữ" Value="Nữ"></asp:ListItem>
                                    </SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtPhone" IsPhone="true"></SweetSoft:ExtraTextBox>
                                </div>

                                <!-- Hàng 2: Email | Ngày gia nhập | Phòng ban | Chức danh -->
                                <div class="col-md-3 mb-2">
                                    <label class="form-label label-valid">Email</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtEmail" Required="true" IsEmail="true" RequiredAdvanced="custom[email]"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE) %></label>
                                    <asp:TextBox runat="server" ID="txtNgayGiaNhap" type="date" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlPhongBan" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlChucDanh" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                            </div> <!-- ĐÓNG THẺ DÒNG SỐ 1 & 2 TẠI ĐÂY -->

                            <!-- KHỐI 2: CHỨA HÀNG 3 (Bọc nguyên khối bằng UpdatePanel, bên trong mở Row mới) -->
                            <asp:UpdatePanel ID="upnlDiaChi" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <!-- MỞ MỘT ROW MỚI Y HỆT KHỐI Ở TRÊN -->
                                    <div class="row px-1">
                                        <div class="col-md-3 mb-2">
                                            <label class="form-label">Tỉnh/Thành</label>
                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlTinhThanh" AutoPostBack="true" EnableSearch="true" OnSelectedIndexChanged="ddlTinhThanh_SelectedValueChanged"></SweetSoft:ExtraDropdown>
                                        </div>
                                        <div class="col-md-3 mb-2">
                                            <label class="form-label">Xã/Phường</label>
                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlXaPhuong" EnableSearch="true"></SweetSoft:ExtraDropdown>
                                        </div>
                                        <div class="col-md-6 mb-2">
                                            <label class="form-label">Địa chỉ chi tiết (Số nhà, Tên đường...)</label>
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi"></SweetSoft:ExtraTextBox>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>

                        </div>
                    </div>
                </fieldset>

                <!-- VÙNG TOGGLE CẤP QUYỀN -->
                <div class="mb-2 ps-2" id="divToggleAccount" runat="server">
                    <div class="form-check form-switch form-switch-md" dir="ltr">
                        <input class="form-check-input" type="checkbox" id="chkEnableAccount" runat="server" onclick="ToggleAccountInfo(this);">
                        <label class="form-check-label fw-bold text-primary fs-5" for="<%= chkEnableAccount.ClientID %>">
                            <i class="fas fa-key me-1"></i> Cấp tài khoản cho nhân viên này
                        </label>
                    </div>
                </div>
                <!-- KHỐI 2: THÔNG TIN TÀI KHOẢN -->
                <fieldset class="fieldset-box mb-1" id="boxAccountInfo" runat="server" style="display: none; padding-bottom: 5px;">
                    <legend class="text-primary fw-bold px-1 mb-0"><%= GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION) %></legend>
                    <div class="row pt-2 mx-0 px-1 d-flex align-items-start"> <!-- Đổi từ end sang start để Subtext không đẩy nút lên -->
        
                       <!-- CỘT 1 (col-4): Username -->
                        <div class="col-md-4 mb-2">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
    
                            <!-- Bọc riêng vùng này vào UpdatePanel để kiểm tra ngầm -->
                            <asp:UpdatePanel ID="upnlUserName" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div class="input-group">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtUserName" Required="false" MaxLength="50" RequiredAdvanced="custom[username]"></SweetSoft:ExtraTextBox>
                
                                        <!-- Đổi thành LinkButton để gọi C# -->
                                        <asp:LinkButton runat="server" ID="btnGenUser" CssClass="btn btn-warning" OnClick="btnGenUser_Click" ToolTip="<%= GetResourceText(BackEndResourceKeys.GENERATE) %>">
                                            <i class="fas fa-bolt"></i>
                                        </asp:LinkButton>
                                    </div>
            
                                    <!-- Subtext thông báo trùng lặp -->
                                    <asp:Label ID="lblUserCheckMsg" runat="server" CssClass="mt-1 d-block" Visible="false" style="font-size: 12px;"></asp:Label>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>

                        <!-- CỘT 2 (col-4): Role -->
                        <div class="col-md-4 mb-2">
                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlRole" SimpleInit="true"></SweetSoft:ExtraDropdown>
                        </div>

                        <!-- CỘT 3 (col-4): "Khu vực Mật Khẩu" -->
                        <div class="col-md-4 mb-2">
                            <label class="form-label">Mật khẩu</label> <!-- Cân bằng chiều cao với label 2 ô kia -->
            
                            <!-- Hiện khi Thêm Mới -->
                            <div runat="server" id="divPasswordAutoInfo" visible="true" class="w-100 text-center">
                                <button type="button" class="btn btn-secondary w-100" disabled style="opacity: 0.75;">
                                    <i class="fas fa-magic me-1"></i> Tự động cấp mật khẩu
                                </button>
                                <small class="text-muted fst-italic d-block mt-1" style="font-size: 11.5px; line-height: 1.1;">
                                    Hệ thống sẽ tự tạo và gửi qua Email
                                </small>
                            </div>
            
                            <!-- Hiện khi Cập Nhật -->
                            <div runat="server" id="divResetPasswordBtn" visible="false" class="w-100 text-center">
                                 <SweetSoft:ExtraButton runat="server" ID="lbtResetPassword" OnClick="lbtResetPassword_Click" CssClass="waves-effect waves-light w-100" ButtonStyle="Warning" ButtonIcon="UnlockAlt">
                                    Cấp lại mật khẩu
                                </SweetSoft:ExtraButton>
                            </div>
                        </div>

                    </div>
                </fieldset>

            </div>
        </div>
    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraButton
                    runat="server"
                    ID="lbtSubmit"
                    CssClass="waves-effect waves-light"
                    ButtonStyle="Primary"
                    ButtonIcon="Save"
                    IsPace="true"
                    OnClientClick="return CMSMasterJs.CheckValid();"
                    OnClick="lbtSubmit_Click">
                    Lưu thông tin
                </SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<style>
    .user-popup-fieldset { width: auto; max-width: 100%; box-sizing: border-box; }

    /* XỬ LÝ KHÔNG GIAN DƯ THỪA TỪ LEGEND */
    fieldset.fieldset-box legend { margin-bottom: 0 !important; }

    /* CSS ĐÃ ĐIỀU CHỈNH: Triệt tiêu chiều cao ảo của Avatar */
    .user-popup-avatar { display: flex; flex-direction: column; align-items: center; justify-content: flex-start; padding-top: 5px; padding-bottom: 0 !important; }
    .avatar-upload { display: flex; justify-content: center; width: 100%; }
    
    /* Ép FilesBox và tất cả thẻ con bên trong không được phình to (min-height: 0) */
    .avatar-upload .file-box,
    .avatar-upload .file-box .card,
    .avatar-upload .file-box .card-body,
    .avatar-upload .file-box .box-body,
    .avatar-upload .file-box .dropzone,
    .avatar-upload .file-box .uploaded-content { 
        max-width: 110px; 
        margin: 0 auto !important; 
        background: transparent !important; 
        border: none !important; 
        box-shadow: none !important; 
        min-height: 0 !important; 
        padding: 0 !important; 
    }

    .avatar-upload .file-box .uploaded-content .item { border: none !important; background: transparent !important; box-shadow: none !important; padding: 0 !important; min-height: 0 !important; }
    .avatar-upload .file-box .uploaded-content .item .bg-body { background-color: transparent !important; }
    .avatar-upload .file-box .uploaded-content .img-container { border: none !important; padding: 0 !important; display: flex; justify-content: center; }
    
    /* Chỉnh lại size ảnh xuống 100px để form cực gọn */
    .avatar-upload .file-box .uploaded-content .item img { width: 100px !important; height: 100px !important; object-fit: cover !important; border-radius: 50% !important; border: 3px solid #ffffff !important; box-shadow: 0 4px 8px rgba(0,0,0,0.15) !important; transition: all 0.3s ease; }
    .avatar-upload .file-box .uploaded-content .item img:hover { transform: scale(1.02); box-shadow: 0 6px 12px rgba(0,0,0,0.2) !important; }
    
    /* Ẩn các nút rác của upload */
    .avatar-upload .file-box .form-check, 
    .avatar-upload .file-box-single .control-help { display: none !important; }
</style>

<script type="text/javascript">
    // ĐÃ XÓA HÀM TogglePasswordEdit (Vì không còn dùng nữa)

    function ToggleAccountInfo(t) {
        var isChecked = $(t).is(':checked');
        var $accBox = $('#<%= boxAccountInfo.ClientID %>');
        var $txtUser = $('#<%= txtUserName.ClientID %>');
        var $ddlRole = $('#<%= ddlRole.ClientID %>');

        if (isChecked) {
            $accBox.slideDown();
            $txtUser.addClass('validate[required]');
            $ddlRole.addClass('validate[required]');
        } else {
            $accBox.slideUp();
            $txtUser.removeClass('validate[required]');
            $ddlRole.removeClass('validate[required]');
            // ĐÃ XÓA TOÀN BỘ LOGIC RÁC GỌI ĐẾN chkChangePassword CŨ Ở ĐÂY
        }
    }

    function GenerateUserName() {
        var fullName = $('#<%= txtFullName.ClientID %>').val();
        if (!fullName) {
            alert('Vui lòng nhập Họ và tên trước khi tự tạo Tên đăng nhập!');
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
    (function () {
        var BASE_Z = 106000;
        function stackBusinessModals() {
            var $modals = $('.modal:visible').not('#modal-notify');
            if ($modals.length > 1) {
                $('.modal-backdrop').each(function (i) { $(this).css('z-index', BASE_Z + (i * 10)); });
                $modals.each(function (i) { $(this).css('z-index', BASE_Z + (i * 10) + 5); });
            }
        }
        function raiseNotify() {
            var notify = document.getElementById('modal-notify');
            if (!notify || !$(notify).is(':visible')) return;
            var $others = $('.modal:visible').not(notify);
            if ($others.length === 0) return;
            var maxZ = BASE_Z;
            $others.each(function () { maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(this).zIndex, 10) || 0); });
            notify.style.setProperty('z-index', String(maxZ + 20), 'important');
            var bds = document.querySelectorAll('.modal-backdrop');
            if (bds.length) bds[bds.length - 1].style.setProperty('z-index', String(maxZ + 15), 'important');
        }
        function raiseNotifyRepeatedly() {
            [0, 100, 250, 500, 900].forEach(function (ms) { setTimeout(function () { stackBusinessModals(); raiseNotify(); }, ms); });
        }
        ['show.bs.modal', 'shown.bs.modal'].forEach(function (evt) {
            document.addEventListener(evt, function (e) { if (e.target && e.target.id === 'modal-notify') raiseNotifyRepeatedly(); });
        });
        document.addEventListener('hidden.bs.modal', function (e) {
            if (e.target && e.target.id === 'modal-notify') e.target.style.removeProperty('z-index');
        });
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(raiseNotifyRepeatedly);
        }
    })();
</script>