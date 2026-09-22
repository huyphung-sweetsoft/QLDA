<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlKhachHangForm.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCustomers.Controls.CtrlKhachHangForm" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>  

<SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Customer Information"
    Size="ExtraLarge">

    <ContentTemplate>

        <div class="row js-validation validationEngineContainer">

            <!-- ========================================= -->
            <!-- CỘT TRÁI: THÔNG TIN KHÁCH HÀNG -->
            <!-- ========================================= -->
            <div class="col-lg-6 pe-lg-4">

                <div class="mb-3">
                    <h5 class="text-uppercase fw-bold mb-3">
                        Thông tin khách hàng
                    </h5>
                </div>

                <!-- Tên khách hàng -->
                <div class="mb-3">
                    <label class="form-label label-valid">
                        <%= GetResourceText(BackEndResourceKeys.CUSTOMER_NAME) %>
                    </label>

                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenKhachHang" Required="true">
                    </SweetSoft:ExtraTextBox>
                </div>

                <!-- Loại khách hàng + Mã số thuế -->
                <div class="row">
                    <div class="col-lg-6 px-2">
                        <div class="mb-3">
                            <label class="form-label label-valid">
                                <%= GetResourceText(BackEndResourceKeys.CUSTOMER_TYPE) %>
                            </label>

                            <SweetSoft:ExtraDropdown runat="server" ID="ddlLoaiKhachHang"
                                Required="true" SimpleInit="true"
                                PlaceHolder="Select the value">
                            </SweetSoft:ExtraDropdown>
                        </div>
                    </div>

                    <div class="col-lg-6 px-2">
                        <div class="mb-3">
                            <label class="form-label label-valid">
                                <%= GetResourceText(BackEndResourceKeys.TAX_CODE) %>
                            </label>

                            <SweetSoft:ExtraTextBox runat="server" ID="txtIdSoThue"
                                Required="true">
                            </SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </div>

                <!-- Số điện thoại + Email -->
                <div class="row">
                    <div class="col-lg-6 px-2">
                        <div class="mb-3">
                            <label class="form-label label-valid">
                                <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>
                            </label>

                            <SweetSoft:ExtraTextBox runat="server" ID="txtSoDienThoai"
                                Required="true">
                            </SweetSoft:ExtraTextBox>
                        </div>
                    </div>

                    <div class="col-lg-6  px-2">
                        <div class="mb-3">
                            <label class="form-label label-valid">
                                Email
                            </label>

                            <SweetSoft:ExtraTextBox runat="server" ID="txtEmail"
                                Required="true">
                            </SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </div>

                <!-- Địa chỉ -->
                <div class="mb-3">
                    <label class="form-label label-valid">
                        <%= GetResourceText(BackEndResourceKeys.ADDRESS) %>
                    </label>

                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaChi" Required="true">
                    </SweetSoft:ExtraTextBox>
                </div>

            </div>


            <!-- ========================================= -->
            <!-- CỘT PHẢI: THÔNG TIN LIÊN HỆ + TRẠNG THÁI -->
            <!-- ========================================= -->
            <div class="col-lg-6 ps-lg-4">

                <div class="mb-3">
                    <h5 class="text-uppercase fw-bold mb-3">
                        <%= GetResourceText(BackEndResourceKeys.CONTACT_INFORMATION) %>
                    </h5>
                </div>

                <!-- Người liên hệ -->
                <div class="mb-3">
                    <label class="form-label">
                        <%= GetResourceText(BackEndResourceKeys.CONTACT_PERSON) %>
                    </label>

                    <SweetSoft:ExtraTextBox runat="server" ID="txtNguoiLienHe">
                    </SweetSoft:ExtraTextBox>
                </div>

                <!-- Email liên hệ + SĐT liên hệ -->
                <div class="row">
                    <div class="col-lg-6 px-2">
                        <div class="mb-3">
                            <label class="form-label">
                                Email
                            </label>

                            <SweetSoft:ExtraTextBox runat="server" ID="txtEmailLienHe">
                            </SweetSoft:ExtraTextBox>
                        </div>
                    </div>

                    <div class="col-lg-6">
                        <div class="mb-3">
                            <label class="form-label">
                                <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>
                            </label>

                            <SweetSoft:ExtraTextBox runat="server" ID="txtSDTLienHe">
                            </SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </div>

                <!-- Trạng thái -->
                <div class="mb-3">
                    <label class="form-label">
                        <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                    </label>

                    <div>
                        <SweetSoft:ExtraCheckbox runat="server" ID="chkStatus" Checked="true"/>
                    </div>
                </div>

            </div>


            <!-- ========================================= -->
            <!-- MÔ TẢ: FULL WIDTH BÊN DƯỚI HAI CỘT -->
            <!-- ========================================= -->
            <div class="col-lg-12">

                <div class="mb-3">
                    <label class="form-label">
                        <%= GetResourceText(BackEndResourceKeys.SUMMARY) %>
                    </label>

                    <CKEditor:CKEditorControl ID="txtMoTa" Width="100%" CssClass="ck-editor"
                        Toolbar="Full" BodyId="StatucPageContent" Language="vi-VN"
                        AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/"
                        runat="server" Height="100">
                    </CKEditor:CKEditorControl>
                </div>

            </div>

        </div>

    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">

            <ContentTemplate>

                <SweetSoft:ExtraButton runat="server" ID="lbtSubmit"
                    CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save"
                    IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();"
                    OnClick="lbtSubmit_Click" Visible="false">
                    Lưu
                </SweetSoft:ExtraButton>

            </ContentTemplate>

        </asp:UpdatePanel>
    </FooterTemplate>

</SweetSoft:ExtraModal>