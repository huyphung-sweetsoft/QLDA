<%@ Page Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="ProjectList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.ProjectList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project list" />
                <div class="card-header">
                    <div class="d-flex flex-column flex-xl-row gap-3">
                        <div class="input-group max-w-500">
                            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" 
                                PlaceHolder="Nhập mã hoặc tên dự án..." 
                                CssClass="border-primary input-search-filter">
                            </SweetSoft:ExtraTextBox>

                            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" 
                                CssClass="btn-outline-primary btn-search-filter" 
                                IsCustomClass="false" 
                                ButtonIcon="Search" 
                                OnClick="btnSearch_ServerClick">
                            </SweetSoft:ExtraButton>
                        </div>  
                    </div>                  
                </div>
                <div class="card-body p-0 mt-2">
                    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                                AllowSorting="true"
                                AutoGenerateColumns="false"
                                CssClass="table-bordered"
                                IsEnableSelectColumn="true"
                                ValueField="IdDuAn"
                                DataNameField="TenDuAn"
                                FocusBtnIcon="fas fa-compress-arrows-alt"
                                DataKeyNames="IdDuAn" GridLines="None"
                                OnNeedDataSource="grvData_NeedDataSource"
                                OnRowCommand="grvData_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="Mã dự án" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="MaDuAn">
                                        <ItemTemplate>
                                            <%# Eval("MaDuAn") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Tên dự án" HeaderStyle-CssClass="text-center" SortExpression="TenDuAn">
                                        <ItemTemplate>
                                            <asp:LinkButton runat="server" CssClass="card-link" Visible='<%# this.IsEdit %>'
                                                ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# Eval("TenDuAn") %>'></asp:LinkButton>
                                            <span runat="server" visible='<%# !this.IsEdit %>'><%# Eval("TenDuAn") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Ngày bắt đầu" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayBatDau">
                                        <ItemTemplate>
                                            <%# ConvertDateTimeToString(Eval("NgayBatDau")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Ngày kết thúc" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayDuKienHoanThanh">
                                        <ItemTemplate>
                                            <%# ConvertDateTimeToString(Eval("NgayDuKienHoanThanh")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Thao tác" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="120px">
                                        <ItemTemplate>
                                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline me-2"
                                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                                            </SweetSoft:SmartLinkButton>

                                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                                ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                                ButtonIcon="fas fa-trash">
                                            </SweetSoft:SmartLinkButton>
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
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server"></asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
        });
    </script>
</asp:Content>