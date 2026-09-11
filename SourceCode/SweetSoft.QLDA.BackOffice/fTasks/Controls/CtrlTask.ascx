<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
<style>
    /* CSS CHO AVATAR STACK CỦA OWNER */
    .avatar-group { 
        display: inline-flex !important; 
        align-items: center; 
        justify-content: center; 
        gap: 6px !important; 
        flex-wrap: nowrap !important; 
        white-space: nowrap !important; /* KHÓA CHẾT: Cấm tuyệt đối việc rớt dòng */
    }  
    .avatar-stack-container { 
        display: flex; 
        align-items: center; 
    }    
    .avatar-circle { 
        width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
        font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; 
        margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }    
    .avatar-circle:first-child { margin-left: 0; }    
    .avatar-more { 
        background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; 
    }    
    .btn-assign-task { 
        width: 26px; height: 26px; border-radius: 6px; background-color: #2563eb; color: white; 
        display: flex; align-items: center; justify-content: center; border: none; cursor: pointer; 
        text-decoration: none; font-size: 12px; transition: background 0.2s, transform 0.1s;
        flex-shrink: 0; /* Giữ nguyên hình vuông cứng, không bị bóp méo hay rớt dòng */
    }
    .btn-assign-task:hover { 
        background-color: #1d4ed8; color: white; transform: scale(1.05); 
    }
</style>
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

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="160px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <!-- Nút Gán Việc (Dấu +) -->
                                <asp:LinkButton runat="server" ID="lbtAssign" 
                                    CommandName="ASSIGN_TASK" 
                                    CommandArgument='<%# Eval("IdCongViec") %>' 
                                    CssClass="btn-assign-task" 
                                    ToolTip="Phân công nhân sự" 
                                    Visible='<%# this.IsEdit %>'>
                                    <i class="fas fa-plus"></i>
                                </asp:LinkButton>
                            </div>
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
            <SweetSoft:CtrlChonNhanVienTask runat="server" ID="CtrlChonNhanVienTask1" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
