<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlQuanLyLoai.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.CtrlQuanLyLoai" %>

<asp:UpdatePanel runat="server" ID="upnlMain" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal runat="server" ID="mdlQuanLyLoai" Type="Primary" Size="Large" FooterButtonClose="true" DefaultButton="btnSaveNew">
            <ContentTemplate>
                <div class="row">
                    <!-- Form thêm mới ở đầu Modal -->
                    <div class="col-12 mb-3">
                        <label class="form-label font-weight-bold">Thêm mới</label>
                        <div class="input-group">
                            <SweetSoft:ExtraTextBox runat="server" ID="txtTenLoaiNew" PlaceHolder="Nhập tên loại..." MaxLength="250"></SweetSoft:ExtraTextBox>
                            <SweetSoft:ExtraButton runat="server" ID="btnSaveNew" ButtonStyle="Primary" ButtonIcon="Add" Text="Thêm" OnClick="btnSaveNew_Click" CssClass="btn-add-new-loai"></SweetSoft:ExtraButton>
                        </div>
                        <asp:Label runat="server" ID="lblError" CssClass="text-danger mt-1 d-block" Visible="false"></asp:Label>
                        <asp:Label runat="server" ID="lblSuccess" CssClass="text-success mt-1 d-block" Visible="false"></asp:Label>
                    </div>

                    <!-- Danh sách các loại -->
                    <div class="col-12">
                        <div class="table-responsive">
                            <SweetSoft:GridviewExtension runat="server" ID="grvData"
                                AllowSorting="false"
                                ShowHeader="true"
                                ShowHeaderWhenEmpty="true"
                                AutoGenerateColumns="false"
                                DataKeyNames="IdLoai"
                                GridLines="None"
                                CssClass="table-bordered table-hover"
                                OnRowEditing="grvData_RowEditing"
                                OnRowCancelingEdit="grvData_RowCancelingEdit"
                                OnRowUpdating="grvData_RowUpdating"
                                OnRowDeleting="grvData_RowDeleting">
                                
                                <Columns>
                                    <asp:TemplateField HeaderText="Tên loại">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lblTenLoai" Text='<%# Eval("TenLoai") %>'></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtTenLoaiEdit" Text='<%# Bind("TenLoai") %>' MaxLength="250"></SweetSoft:ExtraTextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Thao tác" ItemStyle-Width="120px" ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <SweetSoft:SmartLinkButton runat="server" ID="btnEdit" CommandName="Edit" VisibleConditionKey="true" ButtonIcon="fas fa-edit" ToolTip="Sửa" CssClass="btn btn-sm btn-outline-primary"></SweetSoft:SmartLinkButton>
                                            <SweetSoft:SmartLinkButton runat="server" ID="btnDelete" CommandName="Delete" VisibleConditionKey="true" CommandArgument='<%# Eval("IdLoai") %>' ButtonIcon="fas fa-trash" ToolTip="Xóa" CssClass="btn btn-sm btn-outline-danger" OnClientClick="return confirm('Bạn có chắc chắn muốn xóa mục này?');"></SweetSoft:SmartLinkButton>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <SweetSoft:SmartLinkButton runat="server" ID="btnUpdate" CommandName="Update" VisibleConditionKey="true" ButtonIcon="fas fa-save" ToolTip="Lưu" CssClass="btn btn-sm btn-primary"></SweetSoft:SmartLinkButton>
                                            <SweetSoft:SmartLinkButton runat="server" ID="btnCancel" CommandName="Cancel" VisibleConditionKey="true" ButtonIcon="fas fa-times" ToolTip="Hủy" CssClass="btn btn-sm btn-secondary"></SweetSoft:SmartLinkButton>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                
                                <EmptyDataTemplate>
                                    <div class="text-center p-4">
                                        Không có dữ liệu
                                    </div>
                                </EmptyDataTemplate>
                            </SweetSoft:GridviewExtension>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
