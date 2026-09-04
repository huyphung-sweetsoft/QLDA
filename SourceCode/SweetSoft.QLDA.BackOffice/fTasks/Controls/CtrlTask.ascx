<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
                <div class="d-flex gap-2 align-items-center flex-wrap">
                    <button type="button" class="btn-filter-overdue" id="btnFilterOverdue" onclick="toggleOverdueFilter()">
                        <i class="fas fa-exclamation-triangle"></i> Chỉ hiện công việc quá hạn ( <span id="lblOverdueCount" runat="server">0</span> )
                    </button>
                    <button type="button" class="btn-tool-folder" onclick="expandAllTasks()">
                        <i class="far fa-folder-open"></i> Mở rộng tất cả
                    </button>
                    <button type="button" class="btn-tool-folder" onclick="collapseAllTasks()">
                        <i class="far fa-folder"></i> Thu gọn tất cả
                    </button>
                </div>
                <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
            </div>
             <div class="input-group max-w-500">
                 <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập từ khóa tìm kiếm..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                 <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
             </div>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="false"
                AutoGenerateColumns="false"
                CssClass="table table-bordered table-task-grid"
                IsEnableSelectColumn="false"
                IsEnableIndex="false"
                ValueField="IdCongViec"
                DataNameField="TenCongViec"
                DataKeyNames="IdCongViec"
                GridLines="None"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand"
                OnRowDataBound="grvData_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="TaskName" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtTaskName" 
                                CommandName="ITEM_DETAIL" 
                                CommandArgument='<%# Eval("IdCongViec") %>'
                                CssClass="text-decoration-none text-dark"
                                Visible='<%# this.IsEdit %>'>
                                <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                            </asp:LinkButton>
                            <span runat="server" visible='<%# !this.IsEdit %>'>
                                <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="140px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("TenNhanVien") != DBNull.Value && Eval("TenNhanVien") != null ? Eval("TenNhanVien") : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Duration" HeaderStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("ThoiHanNgay") != DBNull.Value ? Eval("ThoiHanNgay") + " ngày" : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartDate" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayBatDau")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndDate" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayKetThuc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Priority" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetTaskPriorityBadge(Eval("TenDoUuTien"), Eval("DiemUuTien")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetTaskStatusBadge(Eval("TrangThai")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Dependent" HeaderStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold">
                        <ItemTemplate>
                            <%# GetPhuThuoc(Eval("IdCongViecPhuThuoc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
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
                    <div class="text-center p-3 text-muted">
                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>