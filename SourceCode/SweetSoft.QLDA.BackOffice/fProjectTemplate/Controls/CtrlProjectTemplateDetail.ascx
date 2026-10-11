<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlProjectTemplateDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectTemplate.Controls.CtrlProjectTemplateDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .template-detail-card .card-body { overflow-x: auto !important; -webkit-overflow-scrolling: touch; }
    .template-detail-grid { min-width: 860px !important; table-layout: fixed !important; }
    .template-detail-grid thead th { white-space: normal !important; word-break: normal !important; overflow-wrap: break-word; background-color: #f8fafc !important; color: #475569 !important; font-weight: 700 !important; }
    .template-detail-grid tbody td { vertical-align: middle !important; font-size: 13px; color: #334155; }
    .template-detail-grid tbody td:nth-child(2), .template-detail-grid tbody td:nth-child(4) { white-space: nowrap; }
    .template-task-name-wrap { 
        display: flex; 
        align-items: center; 
        flex-wrap: nowrap !important; /* Ép không cho rớt dòng */
        min-width: 0; 
        line-height: 1.45; 
        padding-top: 6px; 
        padding-bottom: 6px; 
    }    
    .template-task-branch { flex: 0 0 auto; margin-right: 6px; color: #94a3b8; font-family: monospace; font-size: 14px; user-select: none; }
    .template-task-name-link { min-width: 0; text-decoration: none !important; word-break: break-word; transition: all .15s ease; }
    .phase-title {
        background: linear-gradient(90deg, #f3e8ff 0%, #faf5ff 72%, #ffffff 100%) !important;
        border-left: 4px solid #7c3aed !important; 
        padding: 6px 12px !important; 
        border-radius: 0 7px 7px 0 !important;
        font-size: 15px;
        font-weight: 700;
        color: #4c1d95 !important; 
        display: inline-flex !important; /* Đổi từ flex thành inline-flex để nằm chung hàng với dấu + */
        align-items: center;
        width: fit-content; /* Hộp màu ôm sát vào chữ chứ không dư ra */
    }
    .phase-title i {
        color: #6d28d9 !important; 
        font-size: 14px !important; 
        margin-right: 7px;
    }
    .subtask-title {
        font-size: 14.5px;
        color: #334155 !important; 
        font-weight: 600;
    }
    .phase-title:hover, 
    .subtask-title:hover { 
        text-decoration: none !important;
    }
    .task-code-prefix { 
        color: inherit !important; 
        font-weight: 700; 
        margin-right: 4px; 
    }
    .template-detail-grid > thead > tr > th:first-child, .template-detail-grid > tbody > tr > td:first-child { display: none !important; }
    .template-task-duration { display: inline-flex; align-items: center; justify-content: center; min-width: 72px; padding: 5px 9px; border: 1px solid #dbeafe; border-radius: 7px; background: #eff6ff; color: #1d4ed8; font-weight: 700; white-space: nowrap; }
    .template-action-cell { background-color: #fffbeb !important; border-left: 1px solid #fef08a !important; }
    .btn-template-add-child { width: 24px; height: 24px; display: inline-flex; align-items: center; justify-content: center; flex: 0 0 24px; margin-left: 10px; border: 1px dashed #93c5fd; border-radius: 6px; background: #eff6ff; color: #2563eb; text-decoration: none !important; transition: all .18s ease; }
    .btn-template-add-child:hover { background: #2563eb; border-color: #2563eb; color: #fff !important; }
    .template-empty { padding: 32px 16px; text-align: center; color: #94a3b8; }
    .template-empty i { display: block; margin-bottom: 8px; font-size: 28px; opacity: .55; }
    .sweet-tooltip { position: relative; display: inline-block; }
    .sweet-tooltip::after { content: attr(data-tooltip); position: absolute; bottom: 100%; left: 50%; transform: translateX(-50%) translateY(5px); background-color: #8b5cf6; color: #ffffff; padding: 6px 12px; border-radius: 6px; font-size: 13px; font-weight: 600; white-space: nowrap; box-shadow: 0 4px 10px rgba(139, 92, 246, 0.25); opacity: 0; visibility: hidden; transition: all 0.2s cubic-bezier(0.2, 0.8, 0.2, 1); z-index: 9999; pointer-events: none; }
    .sweet-tooltip::before { content: ''; position: absolute; bottom: 100%; left: 50%; transform: translateX(-50%) translateY(10px); border-width: 5px; border-style: solid; border-color: #8b5cf6 transparent transparent transparent; opacity: 0; visibility: hidden; transition: all 0.2s cubic-bezier(0.2, 0.8, 0.2, 1); z-index: 9999; }
    .sweet-tooltip:hover::after { opacity: 1; visibility: visible; transform: translateX(-50%) translateY(-6px); }
    .sweet-tooltip:hover::before { opacity: 1; visibility: visible; transform: translateX(-50%) translateY(-1px); }

    @media (max-width: 767.98px) { .template-detail-grid { min-width: 780px !important; } }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3 justify-content-between align-items-xl-center">
        <div class="input-group max-w-500 mb-3 mb-xl-0">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter" PlaceHolder="Nhập mã hoặc tên công việc..."></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>

        <div class="d-flex justify-content-end">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">
                        Thêm giai đoạn
                    </SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" AllowSorting="false"
                CssClass="table table-bordered table-hover template-detail-grid align-middle w-100"
                FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdChiTietMau" ValueField="IdChiTietMau" DataNameField="TenCongViec" GridLines="None"
                IsEnableIndex="false" IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">

                <Columns>
                    <asp:TemplateField HeaderText="Tên công việc">
                        <ItemTemplate>
                            <div class="template-task-name-wrap" style='<%# "padding-left:" + GetTaskIndent(Eval("MaCongViec")) + "px;" %>'>
                                <span class="template-task-branch"><%# GetTaskBranch(Eval("MaCongViec")) %></span>
                                
                                <asp:LinkButton runat="server" ID="lbtTaskName"
                                    CommandName="ITEM_DETAIL"
                                    CommandArgument='<%# Eval("IdChiTietMau") %>'
                                    CssClass='<%# GetTaskNameCssClass(Eval("MaCongViec")) %>'
                                    Visible='<%# this.IsView || this.IsEdit %>'>
                                    <%# (!string.IsNullOrEmpty(Convert.ToString(Eval("MaCongViec"))) && !Convert.ToString(Eval("MaCongViec")).Contains(".")) ? "<i class='fas fa-flag'></i>" : "" %>
                                    <span class='<%# GetTaskPrefixCssClass(Eval("MaCongViec")) %>'><%# Eval("MaCongViec") %>.</span> <%# Eval("TenCongViec") %>
                                </asp:LinkButton>

                                <asp:LinkButton runat="server" ID="lbtAddChild"
                                    CommandName="ITEM_ADD_CHILD"
                                    CommandArgument='<%# Eval("IdChiTietMau") %>'
                                    CssClass="btn-template-add-child"
                                    ToolTip="Thêm công việc con"
                                    Visible='<%# this.IsAdd %>'>
                                    <i class="fas fa-plus"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Thời hạn" HeaderStyle-Width="100px" ItemStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <span class="template-task-duration">
                                <%# Eval("ThoiHanNgay") != DBNull.Value && Eval("ThoiHanNgay") != null ? Eval("ThoiHanNgay") + " ngày" : "—" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Phụ thuộc" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtPhuThuoc"
                                CommandName='<%# this.IsEdit ? "ITEM_EDIT" : "ITEM_DETAIL" %>'
                                CommandArgument='<%# Eval("IdCongViecPhuThuoc") %>'
                                CssClass="template-task-name-link sweet-tooltip"
                                data-tooltip='<%# GetPhuThuocName(Eval("IdCongViecPhuThuoc")) %>'
                                Visible='<%# Eval("IdCongViecPhuThuoc") != DBNull.Value && Eval("IdCongViecPhuThuoc") != null %>'>
                                <%# GetPhuThuocCode(Eval("IdCongViecPhuThuoc")) %>
                            </asp:LinkButton>
                            
                            <asp:Literal runat="server" Visible='<%# Eval("IdCongViecPhuThuoc") == DBNull.Value || Eval("IdCongViecPhuThuoc") == null %>' Text="—"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="130px" ItemStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="template-action-cell text-center">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" 
                                 VisibleConditionKey='<%# this.IsView || this.IsEdit %>'
                                 ID="lbtDetail" 
                                 CommandName='<%# this.IsEdit ? "ITEM_EDIT" : "ITEM_DETAIL" %>' 
                                 CommandArgument='<%# Eval("IdChiTietMau") %>'
                                 CssClass="btn-grid-action text-decoration-underline me-1"
                                 ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                 ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                             </SweetSoft:SmartLinkButton>
                          
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsDelete %>'
                                ID="lbtDelete" 
                                CommandName="ITEM_DELETE" 
                                CommandArgument='<%# Eval("IdChiTietMau") %>'
                                CssClass="btn-grid-action text-decoration-underline text-danger me-1"
                                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                ButtonIcon="fas fa-trash">
                            </SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <div class="template-empty">
                        <i class="fas fa-inbox"></i>
                        <div>Chưa có công việc nào trong mẫu này.</div>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>

            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>