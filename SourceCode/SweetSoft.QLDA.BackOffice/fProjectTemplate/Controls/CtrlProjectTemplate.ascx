<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlProjectTemplate.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectTemplate.Controls.CtrlProjectTemplate" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .template-name-link {
        color: #542e88 !important;
        font-size: 16px;
        font-weight: 500;
        text-decoration: none !important;
        transition: color 0.15s ease;
        display: block;
    }
    .template-name-link:hover {
        color: #3b82f6 !important;
        text-decoration: none !important;
    }
    .template-meta { 
        font-size: 12px; 
        color: #64748b; 
        margin-top: 4px; 
    }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3 justify-content-between">
        <div class="input-group max-w-500 mb-3">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter" PlaceHolder="Nhập tên mẫu..."></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>

        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Primary" ButtonIcon="Add">
                            Thêm mẫu mới
                        </SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>

<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table-bordered table-hover align-middle w-100" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdMau" ValueField="IdMau" DataNameField="TenMau" GridLines="None"
                IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="Tên mẫu công việc">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtNameDetail"
                                CommandName='<%# this.IsEdit ? "ITEM_EDIT" : "ITEM_DETAIL" %>'
                                CommandArgument='<%# Eval("IdMau") %>'
                                CssClass="template-name-link" style="cursor: pointer;">
                                <%# Eval("TenMau") %>
                            </asp:LinkButton>
                            <div class="template-meta">
                                <i class="fas fa-info-circle me-1 text-info"></i><%# string.IsNullOrEmpty(Eval("MoTa").ToString()) ? "Chưa có mô tả" : Eval("MoTa") %>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Người tạo" HeaderStyle-Width="180px" ItemStyle-Width="180px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <strong><%# Eval("NguoiTao") %></strong>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Ngày tạo" HeaderStyle-Width="150px" ItemStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("NgayTao") != DBNull.Value ? Convert.ToDateTime(Eval("NgayTao")).ToString("dd/MM/yyyy HH:mm") : "" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="120px" ItemStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server"
                                VisibleConditionKey='<%# this.IsEdit || this.IsView %>'
                                ID="lbtAction"
                                CommandName='<%# this.IsEdit ? "ITEM_EDIT" : "ITEM_DETAIL" %>'
                                CommandArgument='<%# Eval("IdMau") %>'
                                CssClass="btn-grid-action text-decoration-underline"
                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'
                                ToolTip='<%# this.IsEdit ? "Chỉnh sửa" : "Xem chi tiết" %>'>
                            </SweetSoft:SmartLinkButton>

                            <SweetSoft:SmartLinkButton runat="server"
                                VisibleConditionKey='<%# this.IsDelete %>'
                                ID="lbtDelete"
                                CommandName="ITEM_DELETE"
                                CommandArgument='<%# Eval("IdMau") %>'
                                CssClass="btn-grid-action text-decoration-underline text-danger"
                                ButtonIcon="fas fa-trash"
                                ToolTip="Xóa mẫu">
                            </SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center p-4 text-muted">
                        <i class="fas fa-box-open fs-2 mb-2 opacity-50"></i>
                        <div>Chưa có mẫu dự án nào. Hãy tạo mẫu đầu tiên!</div>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>

            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
