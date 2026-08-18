<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFiles.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fFiles.Controls.CtrlFiles" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <div class="input-group max-w-500">
            <a runat="server" visible="false" class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch('search-offcanvas-medicine');" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Enter the keyword search..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>

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
                AutoGenerateColumns="false"
                CssClass="table-bordered"
                IsEnableSelectColumn="false"
                ValueField="Id"
                FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="Id" GridLines="None"
                OnNeedDataSource="grvData_NeedDataSource">
                <Columns>
                    <asp:TemplateField HeaderText="Tên" HeaderStyle-CssClass="text-center" SortExpression="Name">
                        <ItemTemplate>
                            <%# Eval("Name") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Loại tập tin" HeaderStyle-CssClass="text-center" SortExpression="RefType">
                        <ItemTemplate>
                             <%# SweetSoft.QLDA.Core.EnumHelper.EnumHelpers.ToHtmlSpanSafe<SweetSoft.QLDA.Core.FileManager.FileUploadTypes>(Eval("RefType").ToString()) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Kích thước" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end" SortExpression="FileSize">
                        <ItemTemplate>
                            <%# Convert.ToInt64(Eval("FileSize")) / 1024 %>Kb
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Đuôi mở rộng" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end" SortExpression="Ext">
                        <ItemTemplate>
                            <%# Eval("Ext") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Ngày tạo" HeaderStyle-Width="180px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end" SortExpression="CreatedDate">
                        <ItemTemplate>
                            <%# ConvertDateTimeToString(Eval("CreatedDate")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Chủ sở hữu" HeaderStyle-CssClass="text-center" SortExpression="OwnerName">
                        <ItemTemplate>
                            <%# Eval("OwnerName") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tải xuống" HeaderStyle-Width="80px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="FileUrl">
                        <ItemTemplate>
                            <a href="<%# SweetSoft.QLDA.Core.Utils.FileHelpers.IsValidPath(Eval("FileUrl").ToString()) %>" download target="_blank"><i class="fas fa-download"></i></a>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    Không có tập tin nào được tải lên!
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
