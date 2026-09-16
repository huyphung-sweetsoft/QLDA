<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlXemNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienTask" %>

<style>
    .avatar-group { 
        display: inline-flex !important; 
        align-items: center; 
        justify-content: center; 
        gap: 6px !important; 
        flex-wrap: nowrap !important; 
        white-space: nowrap !important;
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
        flex-shrink: 0; 
    }
    .btn-assign-task:hover { 
        background-color: #1d4ed8; color: white; transform: scale(1.05); 
    }
    
    /* UI NÚT CHỈ XEM (MÀU XÁM) NẾU KHÔNG CÓ QUYỀN EDIT */
    .btn-assign-task.view-only {
        background-color: #64748b;
    }
    .btn-assign-task.view-only:hover {
        background-color: #475569;
    }

    .btn-filter-overdue, .btn-tool-folder { transition: all 0.2s; }
    .btn-filter-overdue.active-filter {
        background-color: #fee2e2 !important; color: #ef4444 !important; border-color: #ef4444 !important;
    }
    .btn-tool-folder.active-filter {
        background-color: #e0f2fe !important; color: #0ea5e9 !important; border-color: #0ea5e9 !important;
    }

    /* FIX MÀU HOVER: Tone màu Pastel cực kỳ dịu mắt và sang trọng */
    /* Quá hạn: Nền đỏ hồng pastel nhạt -> Hover đậm lên 1 chút */
    .row-overdue-bg > td { background-color: #fef2f2 !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-overdue-bg:hover > td { background-color: #fee2e2 !important; }

    /* Sắp đến hạn: Nền vàng hổ phách siêu nhạt -> Hover đậm lên 1 chút */
    .row-warning-bg > td { background-color: #fffbeb !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-warning-bg:hover > td { background-color: #fef3c7 !important; } 
</style>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            
            <!-- THANH CÔNG CỤ TRÊN CÙNG (GỘP 1 HÀNG) -->
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-3">
                
                <!-- NHÓM BÊN TRÁI: 2 Nút JS + Search -->
                <div class="d-flex gap-2 align-items-center flex-wrap flex-grow-1">
                    <button type="button" class="btn-filter-overdue" id="btnFilterOverdue" onclick="toggleOverdueFilter()">
                        <i class="fas fa-exclamation-triangle"></i> <%= GetResourceText(BackEndResourceKeys.SHOW_ONLY_OVERDUE_TASKS) %> ( <span id="lblOverdueCount" runat="server">0</span> )
                    </button>
                    
                    <button type="button" class="btn-tool-folder" id="btnToggleTree" onclick="toggleTaskTree()" 
                            data-expand-text="<%= GetResourceText(BackEndResourceKeys.EXPAND_ALL) %>" 
                            data-collapse-text="<%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %>">
                        <i class="far fa-folder-open"></i> <span id="lblToggleText"><%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %></span>
                    </button>
                    
                    <div class="input-group mb-0" style="max-width: 350px;">
                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
                    </div>
                </div>

                <!-- NHÓM BÊN PHẢI: Chú thích màu + Nút Thêm Mới -->
                <div class="d-flex gap-3 align-items-center flex-wrap">
                    <div class="d-flex align-items-center gap-3 font-mobile-small fw-medium">
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fef2f2; border: 1px solid #fca5a5; border-radius: 4px;"></span>
                            <span class="text-danger"><%= GetResourceText("OVERDUE") %></span>
                        </div>
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fffbeb; border: 1px solid #fcd34d; border-radius: 4px;"></span>
                            <span class="text-warning text-dark"><%= GetResourceText("DUE_SOON") %></span>
                        </div>
                    </div>
                    
                    <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
                </div>
                
            </div>

            <!-- BẢNG DỮ LIỆU ĐÃ ĐƯỢC ÉP FULL WIDTH BẰNG W-100 -->
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="false" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table table-bordered table-task-grid table-hover align-middle w-100"
                IsEnableSelectColumn="false" IsEnableIndex="false"
                ValueField="IdCongViec" DataNameField="TenCongViec" DataKeyNames="IdCongViec" GridLines="None"
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand" OnRowDataBound="grvData_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="TaskName" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtTaskName" 
                                CommandName="ITEM_DETAIL" 
                                CommandArgument='<%# Eval("IdCongViec") %>'
                                CssClass="text-decoration-none text-dark"
                                Visible='<%# this.IsView || this.IsEdit %>'>
                                <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="160px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtAssign" 
                                    CommandName="ASSIGN_TASK" 
                                    CommandArgument='<%# Eval("IdCongViec") %>' 
                                    CssClass='<%# this.IsEdit ? "btn-assign-task" : "btn-assign-task view-only" %>' 
                                    ToolTip='<%# GetResourceText(BackEndResourceKeys.PERSONEL_ASSIGNMENT) %>'
                                    Visible='<%# this.IsEdit || this.IsView %>'>
                                    <i class='<%# this.IsEdit ? "fas fa-plus" : "fas fa-user-friends" %>'></i>
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
                    <div class="text-center p-3 text-muted">
                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            
            <SweetSoft:CtrlChonNhanVienTask runat="server" ID="CtrlChonNhanVienTask1" />
            <SweetSoft:CtrlXemNhanVienTask runat="server" ID="CtrlXemNhanVienTask1" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
