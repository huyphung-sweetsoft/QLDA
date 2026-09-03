<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlKhachHang.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCustomers.Controls.CtrlKhachHang" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefault">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchStatus" runat="server"
                            Text="Trạng thái"
                            AllowClear="true"
                            AutoPostBack="true"
                            SearchColumn="KichHoat"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchCustomerType" runat="server"
                            Text="Loại khách hàng"
                            AutoPostBack="true"
                            AllowClear="true"
                            EnableSearch="true"
                            ValueIsOfTypeGUID="true"
                            SearchColumn="IdLoaiKhachHang"
                            SearchPlaceholder="Tìm loại khách hàng..."
                            NoResultsText="Không tìm thấy loại dự án"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="input-group max-w-500">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập tên khách hàng, số điện thoại, email,..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" visible="false" class="d-flex justify-content-end gap-3 w-full flex-wrap">
             <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                 <ContentTemplate>
                     <div class="d-flex">
                         <SweetSoft:ExtraButton runat="server" ID="btnExport" OnClick="btnExport_Click" ButtonStyle="OutLineInfo"
                             CssClass="waves-effect waves-light flex-btn font-mobile-small me-2" ButtonIcon="Excel" IsSubmit="false" Visible="false">Export Excel</SweetSoft:ExtraButton>
                         <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
                     </div>
                 </ContentTemplate>
                 <Triggers>
                     <asp:PostBackTrigger ControlID="btnExport" />
                 </Triggers>
             </asp:UpdatePanel>
         </div>
    </div>
    <div class="listSearchTagBox">
        <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>
<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="true"
                ShowHeader="true"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false"
                CssClass="table-bordered table-hover"
                FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdKhachHang" GridLines="None"
                IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="CustomerName" HeaderStyle-CssClass="text-center" SortExpression="TenKhachHang" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="card-link" Visible="true"
                                ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# Eval("TenKhachHang") %>'></asp:LinkButton>
                             <span runat="server" id="tagName" visible="false"><%# Eval("TenKhachHang") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="CustomerType" HeaderStyle-CssClass="text-center" SortExpression="TenLoaiKhachHang" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("TenLoaiKhachHang") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ContactPerson" HeaderStyle-CssClass="text-center" SortExpression="TenNguoiLienHe" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("TenNguoiLienHe") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ContactEmail" HeaderStyle-CssClass="text-center" SortExpression="EmailLienHe" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("EmailLienHe") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ProjectCount" HeaderStyle-CssClass="text-center" SortExpression="SoLuongDuAn" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("SoLuongDuAn") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit %>'
                                ID="lbtEdit" CommandName="ITEM_EDIT" CssClass="btn-grid-action text-decoration-underline"
                                ResourceKey='<%# BackEndResourceKeys.EDIT%>'
                                ButtonIcon="fas fa-pencil-alt"></SweetSoft:SmartLinkButton>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline ms-2 me-2"
                                ResourceKey='<%# BackEndResourceKeys.VIEW%>'
                                ButtonIcon="fas fa-eye"></SweetSoft:SmartLinkButton>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                ID="SmartLinkButton1" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                ResourceKey='<%# BackEndResourceKeys.DELETE%>'
                                ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
