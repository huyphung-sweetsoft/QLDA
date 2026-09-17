<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
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
    /* CSS CHO TOOLTIP TASK THÔNG MINH */
    .sched-day-card { 
        position: relative; 
        cursor: pointer; 
        overflow: visible !important; /* [QUAN TRỌNG]: Cho phép Tooltip tràn ra ngoài ô */
        -webkit-user-select: none; /* [QUAN TRỌNG]: Chống bôi đen text / Dấu nháy */
        user-select: none; 
    }
    /* Khôi phục bo góc */
    .sd-header { border-radius: 5px 5px 0 0; }
    .sd-body { border-radius: 0 0 5px 5px; }
    .custom-task-tooltip {
        position: absolute; 
        background-color: #0f172a; color: #ffffff; padding: 10px 14px; border-radius: 8px;
        font-size: 12px; white-space: nowrap; box-shadow: 0 10px 25px -5px rgba(0,0,0,0.5);
        opacity: 0; visibility: hidden; transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
        z-index: 1055; pointer-events: none; /* Cấm tương tác để không chắn click chuột */
    }
    .custom-task-tooltip::after { /* Mũi tên trỏ xuống */
        content: ''; position: absolute; top: 100%; left: 50%; margin-left: -6px;
        border-width: 6px; border-style: solid; border-color: #0f172a transparent transparent transparent;
    }
    .sched-day-card {
    position: relative;
    cursor: pointer;
    overflow: visible !important;
    -webkit-user-select: none;
    user-select: none;
    }

    .sd-header {
        border-radius: 6px 6px 0 0;
    }

    .sd-body {
        border-radius: 0 0 6px 6px;
    }

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
        height: auto;
        max-height: none;
        overflow: visible;
        box-shadow: 0 10px 30px rgba(15, 23, 42, 0.18);
        font-size: 12px;
        line-height: 1.5;
        opacity: 0;
        visibility: hidden;
        pointer-events: none;
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
    }

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

    .tooltip-task-list li:last-child {
        border-bottom: none;
        padding-bottom: 0;
    }

    .tooltip-task-list li:first-child {
        padding-top: 0;
    }

    .t-code {
        display: inline-block;
        color: #2563eb;
        font-weight: 700;
        margin-right: 6px;
    }
</style>
<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
                <div class="d-flex gap-2 align-items-center flex-wrap">
                    <button type="button" class="btn-filter-overdue" id="btnFilterOverdue" onclick="toggleOverdueFilter()">
                        <i class="fas fa-exclamation-triangle"></i> <%= GetResourceText(BackEndResourceKeys.SHOW_ONLY_OVERDUE_TASKS) %> ( <span id="lblOverdueCount" runat="server">0</span> )
                    </button>
                    <button type="button" class="btn-tool-folder" id="btnToggleTree" onclick="toggleTaskTree()" 
                            data-expand-text="<%= GetResourceText(BackEndResourceKeys.EXPAND_ALL) %>" 
                            data-collapse-text="<%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %>">
                        <i class="far fa-folder-open"></i> <span id="lblToggleText"><%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %></span>
                    </button>
                </div>
                <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
            </div>
             <div class="input-group max-w-500">
                 <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
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
                                <asp:LinkButton runat="server" ID="lbtAssign" 
                                    CommandName="ASSIGN_TASK" 
                                    CommandArgument='<%# Eval("IdCongViec") %>' 
                                    CssClass="btn-assign-task" 
                                    ToolTip= <%# GetResourceText(BackEndResourceKeys.PERSONEL_ASSIGNMENT) %>
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
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtViewSchedule" CommandName="VIEW_SCHEDULE" CssClass="btn-grid-action text-decoration-none text-info me-2"
                                ResourceKey='<%# BackEndResourceKeys.VIEW %>' ButtonIcon="fas fa-calendar-alt">
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
    <!-- MODAL XEM LỊCH BIỂU TASK -->
           <SweetSoft:ExtraModal
            runat="server"
            ID="mdlTaskSchedule"
            Type="Primary"
            DefaultButton="btnCloseTaskSchedule">

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

                            // Tạo nội dung Tooltip xịn sò
                            var tooltipHtml = "";
                            var hasTasks = (dayData.status === "busy" && dayData.tasks && dayData.tasks.length > 0);
                            
                            if (hasTasks) {
                                tooltipHtml = '<div class="custom-task-tooltip"><ul class="tooltip-task-list">';
                                for (var i = 0; i < dayData.tasks.length; i++) {
                                    tooltipHtml += '<li><span class="t-code">[' + dayData.tasks[i].code + ']</span>' + dayData.tasks[i].name + '</li>';
                                }
                                tooltipHtml += '</ul></div>';
                            }

                            // Gắn Click event nếu có task để kích hoạt cơ chế Ghim (Pin)
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

                // Hàm ghim Tooltip khi click (Chống chạm ra ngoài)
                CMSMasterJs.PinTooltip = function (element, event) {
                    event.stopPropagation(); 
                    var isPinned = $(element).hasClass('show-tooltip');
                    $('.sched-day-card').removeClass('show-tooltip'); // Gỡ ghim ô cũ
                    if (!isPinned) $(element).addClass('show-tooltip'); // Ghim ô mới
                };

                // Chạm ra ngoài màn hình -> Mất Tooltip
                $(document).on('click', function () {
                    $('.sched-day-card').removeClass('show-tooltip');
                });
            </script>
</div>
