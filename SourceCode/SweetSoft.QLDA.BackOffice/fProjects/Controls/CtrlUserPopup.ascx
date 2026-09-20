<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlUserPopup.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlUserPopup" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<SweetSoft:ExtraModal runat="server" ID="mdlUserDetail" Type="Primary" DefaultButton="btnSaveUser">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlUserDetail" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3" style="max-height: 75vh; overflow-y: auto;">
                    <asp:HiddenField ID="hfUserId" runat="server" />
                    
                    <h6 class="fw-bold text-primary mb-4 pb-2 border-bottom">
                        <i class="fas fa-desktop me-2"></i><%= GetResourceText(BackEndResourceKeys.SYSTEM_ACCOUNT_INFO) ?? "Thông tin tài khoản quản trị" %>
                    </h6>

                    <div class="row">
                        <!-- ==========================
                             CỘT TRÁI: AVATAR 
                             ========================== -->
                        <div class="col-md-4 col-xl-3 text-center border-end mb-3 mb-md-0">
                            <div class="mb-3">
                                <img id="imgAvatarPreview" runat="server" src="/Styles/images/user-icon.png" 
                                     class="rounded-circle border border-2 border-primary shadow-sm" 
                                     style="width: 120px; height: 120px; object-fit: cover;" 
                                     onerror="this.src='/Styles/images/user-icon.png'" />
                            </div>
                            <div class="mb-2 text-start">
                                <asp:FileUpload ID="fuAvatar" runat="server" CssClass="form-control form-control-sm" accept="image/*" onchange="CMSMasterJs.PreviewAvatar(this);" />
                            </div>
                            <small class="text-muted d-block text-start" style="font-size: 11px;">Hỗ trợ: JPG, PNG, GIF</small>
                        </div>

                        <!-- ==========================
                             CỘT PHẢI: THÔNG TIN (GRID) 
                             ========================== -->
                        <div class="col-md-8 col-xl-9">
                            <div class="row g-3">
                                <!-- Hàng 1 -->
                                <div class="col-md-6">
                                    <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control" MaxLength="50" placeholder="admin.abc"></asp:TextBox>
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.USER_GROUP) %> <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"></asp:DropDownList>
                                </div>

                                <!-- Hàng 2 -->
                                <div class="col-md-6">
                                    <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtDisplayName" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox>
                                </div>

                                <!-- Hàng 3 -->
                                <div class="col-md-6">
                                    <label class="form-label fw-bold">Email</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                </div>
                                <div class="col-md-6 d-flex align-items-end">
                                    <div class="form-check form-switch mb-2 d-flex align-items-center px-0">
                                        <asp:CheckBox ID="chkAllowLogin" runat="server" Checked="true" CssClass="form-check-input ms-0 me-2 mt-0" style="width: 2.5em; height: 1.25em; cursor: pointer;" />
                                        <label class="form-check-label fw-bold" style="cursor: pointer;" for="<%= chkAllowLogin.ClientID %>">
                                            <%= GetResourceText(BackEndResourceKeys.ALLOW_LOGIN) ?? "Cho phép đăng nhập" %>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnSaveUser" />
            </Triggers>
        </asp:UpdatePanel>
    </ContentTemplate>
    
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterUser" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <button type="button" class="btn btn-secondary waves-effect waves-light" data-bs-dismiss="modal">
                    <%= GetResourceText(BackEndResourceKeys.CLOSE) %>
                </button>
                <asp:LinkButton ID="btnSaveUser" runat="server" CssClass="btn btn-primary waves-effect waves-light" 
                                CausesValidation="false" OnClick="btnSaveUser_Click">
                    <i class="fas fa-save me-1"></i> <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </asp:LinkButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};
    CMSMasterJs.PreviewAvatar = function (input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#<%= imgAvatarPreview.ClientID %>').attr('src', e.target.result);
            }
            reader.readAsDataURL(input.files[0]);
        }
    };
</script>