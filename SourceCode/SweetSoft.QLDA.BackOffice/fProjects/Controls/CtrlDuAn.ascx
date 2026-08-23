<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDuAn.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlDuAn" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <div class="input-group max-w-500">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                 <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập mã dự án, tên dự án,..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
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
                DataKeyNames="IdDuAn" GridLines="None"
                IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="IdProject" HeaderStyle-CssClass="text-center" SortExpression="MaDuAn" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("MaDuAn") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ProjectName" HeaderStyle-CssClass="text-center" SortExpression="TenDuAn" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="card-link" Visible="true"
                                ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# Eval("TenDuAn") %>'></asp:LinkButton>
                             <span runat="server" id="tagName" visible="false"><%# Eval("TenDuAn") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="CustomerName" HeaderStyle-CssClass="text-center" SortExpression="TenKhachHang" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("TenKhachHang") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ProjectManager" HeaderStyle-CssClass="text-center" SortExpression="TenNhanVien" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("TenNhanVien") %>
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
