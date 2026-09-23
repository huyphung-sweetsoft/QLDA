<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="Gantts.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Gantts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        @keyframes fadeSlideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
        
        .gantt { background: #fff; border: 1px solid #ccc; border-radius: 6px; animation: fadeSlideUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards; box-shadow: 0 4px 12px rgba(0,0,0,0.05); height: 65vh; overflow: auto; position: relative; }
        .gantt-body { display: flex; width: max-content; min-width: 100%; min-height: 100%; }
        
        .task-col { width: 350px; flex-shrink: 0; background: #fff; z-index: 20; position: sticky; left: 0; border-right: 1px solid #ccc; box-shadow: 2px 0 8px rgba(0,0,0,0.04); min-height: 100%; }
        .task-col-header { height: 44px; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 13px; background: #f8f9fa; border-bottom: 1px solid #ccc; position: sticky; top: 0; z-index: 30; }

        .task-row-wrap { height: 52px; max-height: 52px; border-bottom: 1px solid #e5e5e5; display: flex; align-items: center; box-sizing: border-box; font-size: 13px; background: #fff; overflow: hidden; transition: max-height 0.35s ease-in-out, opacity 0.35s ease-in-out, background-color 0.2s, border-width 0.35s ease-in-out; opacity: 1; }
        
        .row-hover { background-color: #f9f6fd !important; }
        .row-hover .task-level-1 { transform: translateX(6px); box-shadow: 0 2px 8px rgba(94, 53, 177, 0.15); }
        .row-hover .task-level-child { transform: translateX(6px); }
        .task-level-1 { margin: 0 8px; flex: 1; height: 40px; background-color: #f4ebff; border-left: 4px solid #5e35b1; border-radius: 6px; color: #5e35b1; font-weight: bold; display: flex; align-items: center; padding-left: 10px; user-select: none; transition: transform 0.25s, box-shadow 0.25s; }
        .task-level-child { display: flex; align-items: center; width: 100%; color: #333; transition: transform 0.25s; }
        .level-2 { padding-left: 16px; } .level-3 { padding-left: 42px; } .level-4 { padding-left: 68px; } .level-5 { padding-left: 94px; }
        .tree-branch { display: inline-block; width: 14px; height: 18px; border-left: 1.5px solid #aaa; border-bottom: 1.5px solid #aaa; margin-right: 8px; transform: translateY(-6px); }
        .toggle-icon { transition: transform 0.3s; color: #666; } .task-level-1 .toggle-icon { color: #5e35b1; }
        
        .hidden-task { max-height: 0 !important; opacity: 0 !important; border-bottom-width: 0 !important; }
        
        .chart-col { flex: 1; position: relative; }
        .chart-header { height: 44px; position: relative; background: #f8f9fa; border-bottom: 1px solid #ccc; position: sticky; top: 0; z-index: 25; }
        .chart-header .tick-label { position: absolute; top: 0; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #555; white-space: nowrap; }
        .chart-header .tick-line { position: absolute; top: 0; bottom: 0; width: 1px; background: #e0e0e0; }
        .chart-tracks { position: relative; min-height: 100px; }
        .grid-line { position: absolute; top: 0; bottom: 0; width: 1px; background: #f2f2f2; }
        .task-track { height: 52px; max-height: 52px; position: relative; border-bottom: 1px solid #e5e5e5; overflow: hidden; transition: max-height 0.35s ease-in-out, border-width 0.35s ease-in-out; }
        
        .task-track.hide-bar .bar { opacity: 0 !important; pointer-events: none; }

        .bar { position: absolute; top: 14px; height: 24px; border-radius: 4px; cursor: pointer; transition: width 0.7s cubic-bezier(0.34, 1.56, 0.64, 1), transform 0.2s, box-shadow 0.2s, filter 0.2s, opacity 0.35s ease; }
        .bar:hover { transform: translateY(-2px); box-shadow: 0 4px 10px rgba(0,0,0,0.15); filter: brightness(1.08); z-index: 10; }
        .bar-fill { position: absolute; top: 0; left: 0; bottom: 0; border-radius: 4px; overflow: hidden; }
        
        @keyframes progress-stripes {
            from { background-position: 0 0; }
            to { background-position: 28px 0; }
        }

        .bar-fill.animated-stripes {
            background-image: linear-gradient(-45deg, rgba(255, 255, 255, 0.15) 25%, transparent 25%, transparent 50%, rgba(255, 255, 255, 0.15) 50%, rgba(255, 255, 255, 0.15) 75%, transparent 75%, transparent);
            background-size: 28px 28px;
            animation: progress-stripes 1.5s linear infinite; 
        }

        .bar.status-todo .bar-fill { background-color: #cbd5e1; }
        .bar.status-todo .static-text { color: #334155; }
        .bar.status-done .bar-fill { background-color: #34a853; }
        .bar.status-doing .bar-fill { background-color: #4285f4; }
        .bar.status-warning .bar-fill { background-color: #f4b400; }
        .bar.status-overdue .bar-fill { background-color: #ea4335; }
        
        @keyframes pulseOverdue { 0% { box-shadow: 0 0 0 0 rgba(234, 67, 53, 0.4); } 70% { box-shadow: 0 0 0 6px rgba(234, 67, 53, 0); } 100% { box-shadow: 0 0 0 0 rgba(234, 67, 53, 0); } }
        .bar.status-overdue { animation: pulseOverdue 2s infinite; }
        
        .static-text { 
            position: absolute; top: 0; left: 8px; color: rgba(255, 255, 255, 0.95); font-size: 11px; font-weight: bold; 
            line-height: 24px; white-space: nowrap; letter-spacing: 0.5px; z-index: 10; pointer-events: none; 
        }

        .today-line { position: absolute; top: 0; bottom: 0; width: 2px; background: #ea4335; z-index: 2; }
        .today-label { position: absolute; top: -22px; transform: translateX(-50%); font-size: 11px; color: #ea4335; font-weight: bold; white-space: nowrap; }

        #svgConnections { transition: opacity 0.3s; }
        .dep-line { fill: none; stroke: #ff9800; stroke-width: 1.5; stroke-dasharray: 4, 4; animation: dashMove 10s linear infinite; }
        @keyframes dashMove { to { stroke-dashoffset: -100; } }

        div[id*='modalIssues'] .modal-dialog { max-width: 900px !important; }

        .gantt::-webkit-scrollbar { width: 12px; height: 12px; }
        .gantt::-webkit-scrollbar-track { background: #f8f9fa; border-top: 1px solid #e5e5e5; border-left: 1px solid #e5e5e5; border-radius: 0 0 6px 6px; }
        .gantt::-webkit-scrollbar-thumb { background: #c1c1c1; border-radius: 6px; border: 3px solid #f8f9fa; }
        .gantt::-webkit-scrollbar-thumb:hover { background: #a8a8a8; }
        .gantt::-webkit-scrollbar-corner { background: #f8f9fa; }

        .gantt-badge-done { background-color: #dcfce7 !important; color: #166534 !important; border: 1px solid #bbf7d0 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-doing { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-todo { background-color: #f1f5f9 !important; color: #475569 !important; border: 1px solid #e2e8f0 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-danger { background-color: #fee2e2 !important; color: #dc2626 !important; border: 1px solid #fca5a5 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-4 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <div class="title mb-4" style="font-size: 22px; font-weight: bold;"><%= GetProjectName() %></div>

                <div class="legend mb-4" style="display: flex; gap: 20px; align-items: center; font-size: 13px; color: #555; flex-wrap: wrap;">
                    <div class="legend-item"><span class="legend-swatch" style="background-color:#34a853; background-image:linear-gradient(-45deg, rgba(255,255,255,0.15) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.15) 75%, transparent 75%, transparent); background-size: 20px 20px; width: 14px; height: 14px; border-radius: 3px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.COMPLETED) %></div>
                    <div class="legend-item"><span class="legend-swatch" style="background-color:#4285f4; background-image:linear-gradient(-45deg, rgba(255,255,255,0.15) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.15) 75%, transparent 75%, transparent); background-size: 20px 20px; width: 14px; height: 14px; border-radius: 3px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.DOING) %></div>
                    <div class="legend-item"><span class="legend-swatch" style="background-color:#f4b400; background-image:linear-gradient(-45deg, rgba(255,255,255,0.15) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.15) 75%, transparent 75%, transparent); background-size: 20px 20px; width: 14px; height: 14px; border-radius: 3px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.DUE_SOON) %></div>
                    <div class="legend-item"><span class="legend-swatch" style="background-color:#ea4335; background-image:linear-gradient(-45deg, rgba(255,255,255,0.15) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.15) 75%, transparent 75%, transparent); background-size: 20px 20px; width: 14px; height: 14px; border-radius: 3px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.OVERDUE) %></div>
                    <div class="legend-item"><span class="legend-swatch" style="background-color:#cbd5e1; width: 14px; height: 14px; border-radius: 3px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.NOT_YET_STARTED) %></div>
                    <div class="legend-item"><span class="legend-swatch" style="background:#ea4335;border-radius:0;width:2px;height:14px; display: inline-block;"></span> <%= GetResourceText(BackEndResourceKeys.TODAY) %></div>
                    <div class="legend-item"><i class="fas fa-long-arrow-alt-right" style="color:#ff9800;"></i> <%= GetResourceText(BackEndResourceKeys.DEPENDENT_LINK) %></div>
                </div>

                <div class="gantt">
                    <div class="gantt-body">
                        
                        <div class="task-col">
                            <div class="task-col-header"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></div>
                            <asp:Repeater ID="rptTaskNames" runat="server">
                                <ItemTemplate>
                                    <div class='task-row-wrap <%# GetRowClass((int)Eval("Level"), (bool)Eval("HasChild")) %>'
                                         data-id='<%# Eval("MaCongViec") %>'
                                         <%# GetClickEvents((bool)Eval("HasChild"), Eval("MaCongViec").ToString()) %>
                                         title='Người thực hiện: <%# Eval("NhanVienThucHien") %>'>

                                        <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Level") == 1 %>'>
                                            <div class='task-level-1'>
                                                <i class='fas fa-list-ul toggle-icon' style='margin-right: 8px; font-size: 14px;' runat="server" Visible='<%# (bool)Eval("HasChild") %>'></i>
                                                <%# Eval("MaCongViec") %>. <%# Eval("TenCongViec") %>
                                            </div>
                                        </asp:PlaceHolder>

                                        <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Level") > 1 %>'>
                                            <div class='tree-branch'></div>
                                            <i class='fas fa-chevron-down toggle-icon' 
                                               style='margin-right: 6px; font-size: 12px; <%# (int)Eval("Level") >= 2 && (bool)Eval("HasChild") ? "transform: rotate(-90deg);" : "" %>' 
                                               runat="server" Visible='<%# (bool)Eval("HasChild") %>'></i>
                                            <div><strong><%# Eval("MaCongViec") %>.</strong> <%# Eval("TenCongViec") %></div>
                                        </asp:PlaceHolder>
                                        
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        
                        <div class="chart-col">
                            <div class="chart-header" id="chartHeader"></div>
                            
                            <div class="chart-tracks" id="chartTracks">
                                <svg id="svgConnections" style="position:absolute;top:0;left:0;width:100%;height:100%;pointer-events:none;z-index:5;">
                                    <defs>
                                        <marker id="arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto">
                                            <path d="M 0 0 L 10 5 L 0 10 z" fill="#ff9800" />
                                        </marker>
                                    </defs>
                                    <g id="linesGroup"></g>
                                </svg>
                                
                                <asp:Repeater ID="rptChartTracks" runat="server">
                                    <ItemTemplate>
                                        <div class='task-track' 
                                             data-id='<%# Eval("MaCongViec") %>' 
                                             data-taskid='<%# Eval("TaskId") %>' 
                                             data-dependson='<%# Eval("DependsOn") %>' 
                                             data-start='<%# Eval("StartDay") %>' 
                                             data-end='<%# Eval("EndDay") %>' 
                                             data-status='<%# Eval("StatusClass") %>' 
                                             data-haschild='<%# Eval("HasChild").ToString().ToLower() %>'>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <asp:UpdatePanel ID="upIssues" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hdfSelectedTaskId" runat="server" />
            <asp:HiddenField ID="hdfSelectedTaskCode" runat="server" />
            <asp:Button ID="btnLoadIssues" runat="server" OnClick="btnLoadIssues_Click" CssClass="d-none" />

            <SweetSoft:ExtraModal runat="server" ID="modalIssues" Type="Primary" Title="Thông tin công việc">
                <ContentTemplate>
                    
                    <h6 class="fw-bold text-primary mt-2 ms-2" id="h6TaskTitle" runat="server">
                        <i class="fas fa-info-circle me-1"></i> Thông tin công việc
                    </h6>
                    <div class="table-responsive mb-4">
                        <table class="table table-hover table-bordered mb-0 align-middle" style="font-size: 13px;">
                            <thead class="table-light">
                                <tr>
                                    <th style="width: 150px;"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                    <th class="text-center" style="width: 140px;"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></th>
                                    <th class="text-center" style="width: 130px;"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptTaskInfo" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-bold">
                                                <div style='<%# "padding-left: " + ((Convert.ToInt32(Eval("RelativeLevel")) - 1) * 16) + "px;" %>'>
                                                    <asp:PlaceHolder runat="server" Visible='<%# Convert.ToInt32(Eval("RelativeLevel")) > 1 %>'>
                                                        <div class="tree-branch"></div>
                                                    </asp:PlaceHolder>
                                                    <%# Eval("MaCongViec") %>
                                                </div>
                                            </td>
                                            
                                            <td><%# Eval("TenCongViec") %></td>
                                            <td class="text-center fw-bold"><%# Convert.ToDateTime(Eval("NgayKetThuc")).ToString("dd/MM/yyyy") %></td>
                                            <td class="text-center">
                                                <span class="badge <%# GetTaskStatusBadge(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>">
                                                    <%# GetTaskStatusText(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>
                                                </span>
                                                <%# GetTaskWarningHtml(Eval("TrangThai"), Eval("NgayKetThuc")) %>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:PlaceHolder ID="phEmptyTask" runat="server" Visible='<%# rptTaskInfo.Items.Count == 0 %>'>
                                            <tr><td colspan="4" class="text-center text-muted py-2">Không có dữ liệu công việc.</td></tr>
                                        </asp:PlaceHolder>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>

                    <h6 class="fw-bold text-danger ms-2"><i class="fas fa-exclamation-triangle me-1"></i> <%= GetResourceText(BackEndResourceKeys.ISSUE) %></h6>
                    
                    <asp:PlaceHolder ID="phHasIssues" runat="server">
                        <div class="table-responsive">
                            <table class="table table-hover table-bordered mb-0 align-middle" style="font-size: 13px;">
                                <thead class="table-light">
                                    <tr>
                                        <th class="text-center" style="width: 100px;"><%= GetResourceText(BackEndResourceKeys.ISSUE_CODE) %></th>
                                        <th><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></th>
                                        <th class="text-center" style="width: 130px;"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptIssues" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="text-center fw-bold"><%# Eval("MaVanDe") %></td>
                                                <td><%# Eval("TenVanDe") %></td>
                                                <td class="text-center">
                                                    <span class="badge <%# GetIssueStatusBadge(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>">
                                                        <%# GetIssueStatusText(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>
                                                    </span>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="phNoIssues" runat="server" Visible="false">
                        <div class="ms-2 mb-3 text-muted fst-italic">Không có vấn đề!</div>
                    </asp:PlaceHolder>

                </ContentTemplate>
            </SweetSoft:ExtraModal>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        function toggleTask(maCv, element) {
            element.classList.toggle('collapsed');
            const icon = element.querySelector('.toggle-icon');
            if (icon) {
                const isCollapsed = element.classList.contains('collapsed');
                icon.style.transform = isCollapsed ? 'rotate(-90deg)' : 'rotate(0deg)';
            }
            updateVisibility();
        }

        function updateVisibility() {
            const collapsedIds = Array.from(document.querySelectorAll('.task-row-wrap.collapsed')).map(el => el.dataset.id);
            const allRows = document.querySelectorAll('.task-row-wrap');
            const allTracks = document.querySelectorAll('.task-track');
            
            const svgGroup = document.getElementById("linesGroup");
            if(svgGroup) svgGroup.innerHTML = ''; 

            allRows.forEach((row, index) => {
                const track = allTracks[index];
                const id = row.dataset.id;
                
                let isHidden = false;
                for (let cid of collapsedIds) {
                    if (id !== cid && id.startsWith(cid + ".")) { isHidden = true; break; }
                }

                if (isHidden) {
                    row.classList.add('hidden-task');
                    if (track) { 
                        track.classList.add('hidden-task'); 
                        const bar = track.querySelector('.bar'); 
                        if (bar) bar.style.width = '0%'; 
                    }
                } else {
                    const wasHidden = row.classList.contains('hidden-task');
                    row.classList.remove('hidden-task');
                    
                    if (track) {
                        track.classList.remove('hidden-task');
                        if (wasHidden) {
                            const bar = track.querySelector('.bar');
                            if (bar) { setTimeout(() => { bar.style.width = bar.dataset.targetwidth; }, 300); }
                        }
                    }
                }

                if (track) {
                    const hasChild = track.dataset.haschild === 'true'; 
                    if (hasChild) {
                        const isCollapsed = row.classList.contains('collapsed');
                        if (!isCollapsed) track.classList.add('hide-bar');
                        else track.classList.remove('hide-bar');
                    }
                }
            });

            setTimeout(drawDependencies, 400);
        }

        function drawDependencies() {
            const svgGroup = document.getElementById("linesGroup");
            if (!svgGroup) return;
            svgGroup.innerHTML = ''; 

            const tracks = document.querySelectorAll(".task-track:not(.hidden-task):not(.hide-bar)");
            tracks.forEach(curTrack => {
                const depId = curTrack.dataset.dependson;
                if (!depId || depId === "") return;

                const preTrack = document.querySelector(`.task-track[data-taskid='${depId}']`);
                if (!preTrack || preTrack.classList.contains("hidden-task") || preTrack.classList.contains("hide-bar")) return; 
                
                const curBar = curTrack.querySelector(".bar");
                const preBar = preTrack.querySelector(".bar");
                if (!curBar || !preBar || curBar.style.width === '0%' || preBar.style.width === '0%') return;

                const startX = preBar.offsetLeft + (preBar.offsetWidth / 2);
                const startY = preTrack.offsetTop + preBar.offsetTop + preBar.offsetHeight;
                
                const endX = curBar.offsetLeft;
                const endY = curTrack.offsetTop + curBar.offsetTop + (curBar.offsetHeight / 2);

                let pathData = "";
                if (endX >= startX) {
                    pathData = `M ${startX} ${startY} L ${startX} ${endY} L ${endX - 3} ${endY}`;
                } else {
                    pathData = `M ${startX} ${startY} L ${startX} ${startY + 10} L ${endX - 10} ${startY + 10} L ${endX - 10} ${endY} L ${endX - 3} ${endY}`;
                }

                const pathEl = document.createElementNS("http://www.w3.org/2000/svg", "path");
                pathEl.setAttribute("d", pathData);
                pathEl.setAttribute("class", "dep-line");
                pathEl.setAttribute("marker-end", "url(#arrow)");
                svgGroup.appendChild(pathEl);
            });
        }

        function renderGanttChart() {
            if (typeof GANTT_TOTAL_DAYS === 'undefined') return;

            const MIN_DAY_WIDTH = 60; 
            const ganttViewWidth = document.querySelector('.gantt').clientWidth;
            const chartColWidth = ganttViewWidth - 350; 
            const totalWidth = Math.max(GANTT_TOTAL_DAYS * MIN_DAY_WIDTH, chartColWidth);

            const header = document.getElementById("chartHeader");
            const chartTracks = document.getElementById("chartTracks");
            
            header.style.width = totalWidth + 'px';
            chartTracks.style.width = totalWidth + 'px';

            const pct = (idx) => (idx / GANTT_TOTAL_DAYS) * 100;

            header.innerHTML = '';
            GANTT_DATE_LABELS.forEach((label, i) => {
                const cellWidthPct = (1 / GANTT_TOTAL_DAYS) * 100;
                const tick = document.createElement("div");
                tick.className = "tick-label"; tick.style.left = pct(i) + "%"; tick.style.width = cellWidthPct + "%"; tick.textContent = label;
                header.appendChild(tick);
                const line = document.createElement("div");
                line.className = "tick-line"; line.style.left = pct(i) + "%";
                header.appendChild(line);
            });

            const tracks = document.querySelectorAll(".task-track");
            tracks.forEach((track) => {
                const start = parseFloat(track.dataset.start);
                const end = parseFloat(track.dataset.end);
                const status = track.dataset.status; 
                
                const taskId = track.dataset.taskid;
                const taskCode = track.dataset.id; 
                
                GANTT_DATE_LABELS.forEach((label, i) => {
                    const gl = document.createElement("div"); gl.className = "grid-line"; gl.style.left = pct(i) + "%";
                    track.appendChild(gl);
                });

                let effectiveClass = status;
                let tooltipStatus = status === "done" ? "<%= GetResourceText(BackEndResourceKeys.COMPLETED) %>" : (status === "doing" ? "<%= GetResourceText(BackEndResourceKeys.DOING) %>" : "<%= GetResourceText(BackEndResourceKeys.NOT_YET_STARTED) %>");
                let tooltipSuffix = "";

                if (status !== "done") {
                    const remainingDays = end - GANTT_TODAY_INDEX;
                    if (remainingDays < 0) {
                        effectiveClass = "overdue"; tooltipSuffix = ` (Trễ hạn ${Math.abs(remainingDays)} ngày)`;
                    } else if (remainingDays <= 2) {
                        effectiveClass = "warning"; tooltipSuffix = remainingDays === 0 ? " (Hạn chót hôm nay)" : ` (Còn ${remainingDays} ngày)`;
                    } else {
                        tooltipSuffix = ` (Còn ${remainingDays} ngày)`;
                    }
                }

                const targetWidth = (pct(end) - pct(start)) + "%";

                const bar = document.createElement("div");
                bar.className = "bar status-" + effectiveClass;
                bar.style.left = pct(start) + "%";
                bar.style.width = "0%";
                bar.title = tooltipStatus + tooltipSuffix;
                bar.dataset.targetwidth = targetWidth;

                const fill = document.createElement("div");
                fill.className = "bar-fill animated-stripes";
                fill.style.left = "0"; fill.style.width = "100%";

                const slideTxt = document.createElement("div");
                slideTxt.className = "static-text";
                if (effectiveClass === "doing") slideTxt.innerHTML = "<%= GetResourceText(BackEndResourceKeys.DOING) %>";
                else if (effectiveClass === "done") slideTxt.innerHTML = "<%= GetResourceText(BackEndResourceKeys.COMPLETED) %>";
                else if (effectiveClass === "overdue") slideTxt.innerHTML = "<%= GetResourceText(BackEndResourceKeys.OVERDUE) %>";
                else if (effectiveClass === "todo") slideTxt.innerHTML = "<%= GetResourceText(BackEndResourceKeys.NOT_YET_STARTED) %>";
                else if (effectiveClass === "warning") slideTxt.innerHTML = "Sắp đến hạn";

                fill.appendChild(slideTxt);
                bar.appendChild(fill);

                bar.onclick = function (e) {
                    e.stopPropagation();
                    document.getElementById('<%= hdfSelectedTaskId.ClientID %>').value = taskId;
                    document.getElementById('<%= hdfSelectedTaskCode.ClientID %>').value = taskCode;
                    document.getElementById('<%= btnLoadIssues.ClientID %>').click();
                };

                track.appendChild(bar);
                setTimeout(() => { bar.style.width = targetWidth; }, 100);
            });

            const todayLine = document.createElement("div");
            todayLine.className = "today-line";
            todayLine.style.left = pct(GANTT_TODAY_INDEX + 0.5) + "%";

            const todayLabel = document.createElement("div");
            todayLabel.className = "today-label";
            todayLabel.textContent = "<%= GetResourceText(BackEndResourceKeys.TODAY) %>";
            todayLabel.style.left = pct(GANTT_TODAY_INDEX + 0.5) + "%";

            chartTracks.appendChild(todayLine);
            chartTracks.appendChild(todayLabel);

            updateVisibility();
        }

        if (typeof Sys !== 'undefined' && Sys.Application) {
            Sys.Application.add_load(function () {
                if (document.getElementById("chartHeader").innerHTML === '') {
                    renderGanttChart();
                }
            });
        } else {
            window.addEventListener('DOMContentLoaded', renderGanttChart);
        }
    </script>
</asp:Content>