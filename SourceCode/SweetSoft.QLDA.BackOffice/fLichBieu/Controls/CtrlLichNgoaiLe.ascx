<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlLichNgoaiLe.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fLichBieu.Controls.CtrlLichNgoaiLe" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <!-- Vùng Panel cho các Dropdown lọc (tạm để trống để giữ cấu trúc) -->
        <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefault">
                    <div class="d-flex">
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <!-- Vùng Ô tìm kiếm (Nằm ngoài UpdatePanel chuẩn theo CtrlUsers) -->
        <div class="input-group max-w-500">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập từ khóa tìm kiếm..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        
        <!-- Vùng Nút thao tác (Add) bọc trong tagOther -->
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">Add new</SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    
    <!-- Vùng chứa các Tag tìm kiếm (Đồng bộ UI) -->
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
                AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table-bordered table-hover" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdNgoaiLe" GridLines="None" IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="TenNgoaiLe" HeaderText="EventName">
                        <ItemStyle CssClass="fw-bold text-primary text-center" />
                        <ItemTemplate>
                            <%# Eval("TenNgoaiLe") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayBatDau" HeaderText="FromDate">
                        <ItemTemplate>
                            <%# FormatDate(Eval("NgayBatDau")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayKetThuc" HeaderText="ToDate">
                        <ItemTemplate>
                            <%# FormatDate(Eval("NgayKetThuc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="120px" HeaderText="Action">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit %>' 
                                    ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# BackEndResourceKeys.EDIT%>' ButtonIcon="fas fa-pencil-alt">
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                    ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash">
                                </SweetSoft:SmartLinkButton>
                            </div>
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