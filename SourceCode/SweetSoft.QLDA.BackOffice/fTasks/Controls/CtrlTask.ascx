<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlXemNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlSwapPhase.ascx" TagPrefix="SweetSoft" TagName="CtrlSwapPhase" %>
<style>
    .btn-swap-custom {
        background-color: #ffffff !important;
        color: #64748b !important;
        border: 1px solid #cbd5e1 !important;
        border-radius: 4px !important;
        font-weight: 500 !important;
        transition: all 0.2s ease !important;
        padding: 5px 12px !important;
    }
    .btn-swap-custom i {
        color: #4b1c71 !important; 
    }
    .btn-swap-custom:hover {
        background-color: #f1f5f9 !important;
        color: #475569 !important;
        border-color: #94a3b8 !important;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05) !important;
    }
    .btn-swap-custom:hover i {
        color: #3b1659 !important; 
    }
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
    .btn-assign-task.view-only { background-color: #64748b; }
    .btn-assign-task.view-only:hover { background-color: #475569; }

    .card-body {
        overflow-x: auto !important;
        -webkit-overflow-scrolling: touch;
    }
    .table-task-grid {
        table-layout: fixed !important;
        word-wrap: break-word;
        min-width: 1000px !important; 
    }
    .table-task-grid th {
        white-space: normal !important;
        word-break: break-word;
        vertical-align: middle !important;
    }
    .table-task-grid th, .table-task-grid td {
        white-space: nowrap;
    }
    .table-task-grid td:first-child {
        white-space: normal !important;
        word-break: break-word;
    }

    .btn-add-subtask-right {
        background-color: #eff6ff !important;
        color: #2563eb !important;
        border: 1px dashed #93c5fd !important;
        width: 24px;
        height: 24px;
        border-radius: 6px;
        font-size: 11px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s ease;
        text-decoration: none;
        flex-shrink: 0;
    }
    .btn-add-subtask-right:hover {
        background-color: #2563eb !important;
        color: #ffffff !important;
        border-color: #2563eb !important;
    }

    .sched-day-card { 
        position: relative; 
        cursor: pointer; 
        overflow: visible !important;
        -webkit-user-select: none; 
        user-select: none; 
    }
    .sd-header { border-radius: 5px 5px 0 0; }
    .sd-body { border-radius: 0 0 5px 5px; }
    
    .custom-task-tooltip {
        position: fixed !important;
        z-index: 999999 !important;
        background: #ffffff;
        color: #334155;
        border: 1px solid #cbd5e1;
        border-radius: 10px;
        padding: 11px 14px;
        min-width: 180px;
        max-width: 420px;
        white-space: normal;
        word-break: break-word;
        overflow-wrap: anywhere;
        box-shadow: 0 10px 30px rgba(15, 23, 42, 0.18);
        font-size: 12px;
        line-height: 1.5;
        opacity: 0;
        visibility: hidden;
        pointer-events: none;
        transition: opacity 0.2s ease;
    }
    .custom-task-tooltip::after {
        content: '';
        position: absolute;
        top: 100%;
        left: 50%;
        margin-left: -6px;
        border-width: 6px;
        border-style: solid;
        border-color: #ffffff transparent transparent transparent;
    }
    .sched-day-card:hover .custom-task-tooltip,
    .sched-day-card.show-tooltip .custom-task-tooltip {
        opacity: 1;
        visibility: visible;
        pointer-events: auto;
    }
    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 7px 0; border-bottom: 1px solid #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }

    .tooltip-task-list {
        list-style: none;
        margin: 0;
        padding: 0;
        text-align: left;
    }
    .tooltip-task-list li {
        margin: 0;
        padding: 7px 0;
        border-bottom: 1px solid #e2e8f0;
        color: #334155;
    }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }

    .t-code {
        display: inline-block;
        color: #2563eb;
        font-weight: 700;
        margin-right: 6px;
    }

    .btn-filter-overdue, .btn-tool-folder { transition: all 0.2s; }
    .btn-filter-overdue.active-filter { background-color: #fee2e2 !important; color: #ef4444 !important; border-color: #ef4444 !important; }
    .btn-tool-folder.active-filter { background-color: #e0f2fe !important; color: #0ea5e9 !important; border-color: #0ea5e9 !important; }

    .row-overdue-bg > td { background-color: #fef2f2 !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-overdue-bg:hover > td { background-color: #fee2e2 !important; }

    .row-warning-bg > td { background-color: #fffbeb !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-warning-bg:hover > td { background-color: #fef3c7 !important; } 
</style>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-3">
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
                        <SweetSoft:ExtraTextBox CssClass="border-primary input-search-filter" ID="txtSearchSingle" runat="server"></SweetSoft:ExtraTextBox>
                        <SweetSoft:ExtraButton ButtonIcon="Search" CssClass="btn-outline-primary btn-search-filter" ID="lbtSearchSingle" IsCustomClass="false" OnClick="btnSearch_ServerClick" runat="server"></SweetSoft:ExtraButton>
                    </div>
                </div>

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
                    <asp:LinkButton ID="lbtSwapPhase" runat="server" OnClick="lbtSwapPhase_Click" CssClass="btn btn-swap-custom font-mobile-small me-2 pt-1 pb-1 px-2">
                        <i class="fas fa-exchange-alt me-1"></i> Đổi vị trí
                    </asp:LinkButton>
                    <SweetSoft:ExtraButton ButtonIcon="Add" ButtonStyle="Info" CssClass="waves-effect waves-light font-mobile-small" ID="lbtAdd" OnClick="lbtAdd_Click" Visible="false" runat="server">Add new</SweetSoft:ExtraButton>
                </div>
            </div>

            <asp:Panel runat="server" ID="pnlNoTask" Visible="false" CssClass="text-center p-5 bg-white border rounded shadow-sm my-3">
                <i class="fas fa-inbox fs-1 text-muted opacity-50 mb-2"></i>
                <div class="fw-semibold text-secondary"><%= GetResourceText(BackEndResourceKeys.NO_TASK_FOR_YOU)%></div>
            </asp:Panel>

            <SweetSoft:GridviewExtension AllowSorting="false" AutoGenerateColumns="false" CssClass="table table-bordered table-task-grid table-hover align-middle w-100" DataKeyNames="IdCongViec" DataNameField="TenCongViec" GridLines="None" ID="grvData" IsEnableIndex="false" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand" OnRowDataBound="grvData_RowDataBound" ShowHeader="true" ShowHeaderWhenEmpty="true" ValueField="IdCongViec" runat="server">
                <Columns>
                    <asp:TemplateField HeaderText="TaskName" HeaderStyle-Width="28%" ItemStyle-Width="28%" HeaderStyle-CssClass="text-center">
                        <HeaderStyle Width="28%"/>
                        <ItemStyle Width="28%"/>
                        <ItemTemplate>
                            <div class="d-flex align-items-center justify-content-between py-1 px-1">
                                <div style="width: 88%; word-break: break-word; white-space: normal;">
                                    <asp:LinkButton runat="server" ID="lbtTaskName" 
                                        CommandName="ITEM_DETAIL" 
                                        CommandArgument='<%# Eval("IdCongViec") %>'
                                        CssClass="text-decoration-none text-dark fw-semibold"
                                        Visible='<%# this.IsView || this.IsEdit %>'>
                                        <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                                    </asp:LinkButton>
                                </div>

                                <div style="width: 10%; text-align: right;" class="flex-shrink-0">
                                    <asp:LinkButton runat="server" ID="lbtAddChild" 
                                        CommandName="ITEM_ADD_CHILD" 
                                        CommandArgument='<%# Eval("IdCongViec") %>'
                                        CssClass="btn-add-subtask-right"
                                        ToolTip="Thêm công việc con"
                                        Visible='<%# this.IsAdd %>'>
                                        <i class="fas fa-plus"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="150px" ItemStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <HeaderStyle Width="150px"/>
                        <ItemStyle Width="150px"/>
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

                    <asp:TemplateField HeaderText="Duration" HeaderStyle-Width="90px" ItemStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="90px"/>
                        <ItemStyle Width="90px"/>
                        <ItemTemplate>
                            <%# Eval("ThoiHanNgay") != DBNull.Value ? Eval("ThoiHanNgay") + " ngày" : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayBatDau")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayKetThuc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="ActualCompletionDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayHoanThanhThucTe")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# GetTaskStatusBadge(Eval("TrangThai"), Eval("NgayKetThuc"), Eval("NgayHoanThanhThucTe")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Dependent" HeaderStyle-Width="90px" ItemStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold">
                        <HeaderStyle Width="90px"/>
                        <ItemStyle Width="90px"/>
                        <ItemTemplate>
                            <%# GetPhuThuoc(Eval("IdCongViecPhuThuoc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="130px">
                        <HeaderStyle Width="130px"/>
                        <ItemStyle Width="130px"/>
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtDetail" 
                                CommandName="ITEM_DETAIL" 
                                CssClass="btn-grid-action text-decoration-underline me-1"
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                            </SweetSoft:SmartLinkButton>

                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsDelete %>'
                                ID="lbtDelete" 
                                CommandName="ITEM_DELETE" 
                                CssClass="btn-grid-action text-decoration-underline text-danger me-1"
                                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                ButtonIcon="fas fa-trash">
                            </SweetSoft:SmartLinkButton>
                
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtViewSchedule" 
                                CommandName="VIEW_SCHEDULE" 
                                CssClass="btn-grid-action text-decoration-none text-info"
                                ResourceKey='<%# BackEndResourceKeys.VIEW %>' 
                                ButtonIcon="fas fa-calendar-alt">
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
            
            <SweetSoft:CtrlChonNhanVienTask ID="CtrlChonNhanVienTask1" runat="server"/>
            <SweetSoft:CtrlXemNhanVienTask ID="CtrlXemNhanVienTask1" runat="server"/>
            <SweetSoft:CtrlSwapPhase ID="CtrlSwapPhase1" runat="server"/>
        </ContentTemplate>
    </asp:UpdatePanel>

    <SweetSoft:ExtraModal DefaultButton="btnCloseTaskSchedule" ID="mdlTaskSchedule" Type="Primary" runat="server">
        <ContentTemplate>
            <asp:UpdatePanel
                ID="upnlTaskSchedule"
                runat="server"
                UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <div style="font-size: 13px;
                                    color: #1e40af;
                                    background: #eff6ff;
                                    padding: 10px 12px;
                                    border-radius: 6px;
                                    border: 1px solid #bfdbfe;
                                    margin-bottom: 12px;">
                            <i class="fas fa-calendar-alt me-1"></i>
                            <%= GetResourceText(BackEndResourceKeys.EXECUTION_TIME) %>:
                            <strong>
                                <asp:Literal
                                    ID="ltrScheduleTaskName"
                                    runat="server">
                                </asp:Literal>
                            </strong>
                        </div>
                        <asp:HiddenField
                            ID="hdfSingleTaskScheduleJson"
                            runat="server" />
                        <div style="max-height:60vh;
                                    overflow-y:auto;
                                    padding:15px 5px 40px 5px;">
                            <div id="task-timeline-container"
                                 class="row-sched-timeline-grid-7col">
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <script type="text/javascript">
        window.CMSMasterJs = window.CMSMasterJs || {};
        CMSMasterJs.RenderSingleTaskSchedule = function () {
            var container = $('#task-timeline-container');
            container.empty();
            
            var jsonString = $('#<%= hdfSingleTaskScheduleJson.ClientID %>').val();
            if (!jsonString) return;

            try {
                var decodedJson = $('<textarea/>').html(jsonString).text();
                var scheduleData = JSON.parse(decodedJson);

                for (var dateKey in scheduleData) {
                    var dayData = scheduleData[dateKey];
                    var dateParts = dateKey.split('-');
                    var formattedDate = dateParts[2] + '/' + dateParts[1];

                    var tooltipHtml = "";
                    var hasTasks = (dayData.status === "busy" && dayData.tasks && dayData.tasks.length > 0);

                    if (hasTasks) {
                        tooltipHtml = '<div class="custom-task-tooltip"><ul class="tooltip-task-list">';
                        for (var i = 0; i < dayData.tasks.length; i++) {
                            tooltipHtml += '<li><span class="t-code">[' + dayData.tasks[i].code + ']</span>' + dayData.tasks[i].name + '</li>';
                        }
                        tooltipHtml += '</ul></div>';
                    }

                    var clickAttr = hasTasks ? 'onclick="CMSMasterJs.PinTooltip(this, event)"' : '';

                    var html = '<div class="sched-day-card" ' + clickAttr + '>' +
                        '<div class="sd-header">' + formattedDate + '<small>' + dayData.dayName + '</small></div>' +
                        '<div class="sd-body ' + dayData.status + '">' + dayData.displayText + '</div>' +
                        tooltipHtml +
                        '</div>';
                    container.append(html);
                }
            } catch (e) {
                console.error("Lỗi vẽ JSON Lịch biểu Task: ", e);
            }
        };
        CMSMasterJs.PinTooltip = function (element, event) {
            event.stopPropagation();
            var isPinned = $(element).hasClass('show-tooltip'); $('.sched-day-card').removeClass('show-tooltip');
            if (!isPinned) $(element).addClass('show-tooltip');
        };
        $(document).on('click', function () {
            $('.sched-day-card').removeClass('show-tooltip');
        });
    </script>
</div>