<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardProgress.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardProgress" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

<div class="container-fluid dashboard-progress">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3 <%= IsProjectDashboard ? "dashboard-project-heading" : string.Empty %>">
        <div class="flex-grow-1">
            <h4 class="mb-1 <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_TITLE) %></h4>

            <div class="row g-2 <%= IsProjectDashboard ? "mt-0" : "mt-3" %> align-items-end">
                <div class="col-12 col-sm-6 col-md-5 col-xl-4 <%= IsProjectDashboard ? "d-none" : string.Empty %>">
                    <label class="form-label mb-1 text-nowrap">
                        <%= GetResourceText(BackEndResourceKeys.PROJECT_SCOPE) %>
                    </label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlProjectFilter"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true"
                        OnSelectedIndexChanged="ddlProjectFilter_SelectedIndexChanged">
                    </SweetSoft:ExtraDropdown>
                </div>

                <% if (IsProjectDashboard) { %>
                <div class="col-auto d-flex align-items-end dashboard-project-tabs-inline">
                    <SweetSoft:CtrlProjectDashboardTabs
                        runat="server"
                        ID="CtrlProjectDashboardTabs1" />
                </div>
                <% } %>

                <div class="col-12 col-sm-6 col-md-4 col-xl-3 <%= IsProjectDashboard ? "ms-auto" : string.Empty %>">
                    <label class="form-label mb-1 text-nowrap <%= IsProjectDashboard ? "d-none" : string.Empty %>">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_DATE_RANGE_LABEL) %>
                        <span class="text-muted ms-1" role="img" tabindex="0"
                              title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_FILTER_DESC) %>"
                              aria-label="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_FILTER_DESC) %>">
                            <i class="bx bx-info-circle"></i>
                        </span>
                    </label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlDateRange"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true"
                        OnSelectedIndexChanged="ddlDateRange_SelectedIndexChanged">
                    </SweetSoft:ExtraDropdown>
                </div>

            </div>

        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 progress-date-card <%= IsProjectDashboard ? "dashboard-project-date-card" : string.Empty %>">
            <div class="bg-primary-subtle text-primary rounded-circle d-flex align-items-center justify-content-center me-3 progress-date-icon">
                <i class="bx bx-line-chart fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UPDATED_AT) %></div>
                <div class="fw-bold text-dark lh-1"><%= Model.GeneratedAt.ToString("HH:mm dd/MM/yyyy") %></div>
            </div>
        </div>
    </div>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-md-3 <%= Model.IsSingleProject ? "row-cols-xl-4 dashboard-kpi-grid-four" : "row-cols-xl-5" %> g-3 mb-4 progress-kpi-grid">
        <div class="col">
            <a href="#progressTaskDetailsModal" class="d-block h-100 text-decoration-none text-reset dashboard-kpi-trigger"
               data-bs-toggle="modal" aria-controls="progressTaskDetailsModal" data-task-filter="all"
               data-task-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_TASKS) %>">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_TASKS) %></p>
                        <h3 class="mb-0"><%= Model.TotalTaskCount %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-info-subtle text-info progress-kpi-icon">
                        <i class="bx bx-task fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="col">
            <a href="#progressTaskDetailsModal" class="d-block h-100 text-decoration-none text-reset dashboard-kpi-trigger"
               data-bs-toggle="modal" aria-controls="progressTaskDetailsModal" data-task-filter="completed"
               data-task-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COMPLETED_TASKS) %>">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COMPLETED_TASKS) %></p>
                        <h3 class="mb-0 text-success"><%= Model.CompletedTaskCount %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-success-subtle text-success progress-kpi-icon">
                        <i class="bx bx-check-circle fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="col">
            <a href="#progressTaskDetailsModal" class="d-block h-100 text-decoration-none text-reset dashboard-kpi-trigger"
               data-bs-toggle="modal" aria-controls="progressTaskDetailsModal" data-task-filter="in-progress"
               data-task-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS) %>">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS) %></p>
                        <h3 class="mb-0 text-info"><%= Model.InProgressTaskCount %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-info-subtle text-info progress-kpi-icon">
                        <i class="bx bx-loader-circle fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="col">
            <a href="#progressTaskDetailsModal" class="d-block h-100 text-decoration-none text-reset dashboard-kpi-trigger"
               data-bs-toggle="modal" aria-controls="progressTaskDetailsModal" data-task-filter="overdue"
               data-task-title="<%: GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %>">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %></p>
                        <h3 class="mb-0 text-danger"><%= Model.OverdueTaskCount %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-danger-subtle text-danger progress-kpi-icon">
                        <i class="bx bx-error-circle fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <% if (!Model.IsSingleProject) { %>
        <div class="col">
            <a href="#needsAttentionProjectsModal" class="d-block h-100 text-decoration-none text-reset dashboard-kpi-trigger"
               data-bs-toggle="modal" aria-controls="needsAttentionProjectsModal">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NEEDS_ATTENTION) %></p>
                        <h3 class="mb-0 text-warning"><%= Model.NeedsAttentionProjectCount %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning progress-kpi-icon">
                        <i class="bx bx-alarm-exclamation fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>
        <% } %>
    </div>

    <div class="modal fade" id="progressTaskDetailsModal" tabindex="-1" aria-labelledby="progressTaskDetailsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="progressTaskDetailsModalTitle">
                        <span id="progressTaskDetailsModalHeading"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_TASKS) %></span>
                        <span class="badge bg-primary-subtle text-primary ms-1" id="progressTaskDetailsCount"><%= AllTaskDetails.Count %></span>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <% if (AllTaskDetails.Count > 0) { %>
                    <div class="row g-2 mb-3">
                        <% if (!Model.IsSingleProject) { %>
                        <div class="col-12 col-md-5">
                            <label for="progressTaskProjectFilter" class="form-label mb-1"><%= GetResourceText(BackEndResourceKeys.PROJECT) %></label>
                            <select id="progressTaskProjectFilter" class="form-select">
                                <option value=""><%= GetResourceText(BackEndResourceKeys.ALL_PROJECTS) %></option>
                                <% foreach (var project in TaskProjectFilterOptions) { %>
                                <option value="<%= project.ProjectId %>"><%: project.ProjectCode %> - <%: project.ProjectName %></option>
                                <% } %>
                            </select>
                        </div>
                        <% } %>
                        <div class="col-12 <%= Model.IsSingleProject ? "" : "col-md-7" %>">
                            <label for="progressTaskDetailsSearch" class="form-label mb-1"><%= GetResourceText(BackEndResourceKeys.SEARCH) %></label>
                            <div class="input-group dashboard-list-search">
                                <span class="input-group-text"><i class="bx bx-search"></i></span>
                                <input type="search" id="progressTaskDetailsSearch" class="form-control" placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                            </div>
                        </div>
                    </div>
                    <% } %>
                    <div class="table-responsive" id="progressTaskDetailsTableContainer">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0" id="progressTaskDetailsTable">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></th>
                                </tr>
                            </thead>
                            <tbody id="progressTaskDetailsBody" data-project-group-count-format="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT) %>">
                                <% foreach (var task in AllTaskDetails) { %>
                                <tr data-task-status="<%= task.StatusCode %>"
                                    data-task-project-id="<%= task.ProjectId %>"
                                    data-group-project-id="<%= task.ProjectId %>"
                                    data-group-project-code="<%: task.ProjectCode %>"
                                    data-group-project-name="<%: task.ProjectName %>">
                                    <td>
                                        <div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div>
                                    </td>
                                    <td>
                                        <div class="fw-semibold"><%: task.ProjectCode %></div>
                                        <div class="small text-muted"><%: task.ProjectName %></div>
                                    </td>
                                    <td><%: task.PriorityName %></td>
                                    <td class="text-nowrap">
                                        <%= task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "-" %>
                                        <% if (task.IsDueSoon || task.StatusCode == TaskOverdueStatusCode) { %>
                                        <div class="small text-muted"><%: GetTaskDeadlineText(task) %></div>
                                        <% } %>
                                    </td>
                                    <td class="text-center"><%= task.Progress %>%</td>
                                    <td class="text-center"><span class="badge <%= GetTaskStatusBadgeCss(task) %>"><%: task.Status %></span></td>
                                    <td class="text-center">
                                        <a href="<%: GetProjectTaskDetailUrl(task.ProjectId, task.TaskId) %>" class="btn btn-sm btn-outline-primary">
                                            <%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %>
                                        </a>
                                    </td>
                                </tr>
                                <% } %>
                            </tbody>
                        </table>
                    </div>
                    <div id="progressTaskDetailsEmpty" class="text-center text-muted py-4 d-none">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_TASKS_IN_PERIOD) %>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-dark" data-bs-dismiss="modal"><i class="bx bx-x"></i><%= GetResourceText(BackEndResourceKeys.CLOSE) %></button>
                </div>
            </div>
        </div>
    </div>

    <% if (!Model.IsSingleProject) { %>
    <div class="modal fade" id="needsAttentionProjectsModal" tabindex="-1" aria-labelledby="needsAttentionProjectsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="needsAttentionProjectsModalTitle">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_NEEDS_ATTENTION) %>
                        <span class="badge bg-warning-subtle text-warning ms-1"><%= NeedsAttentionProjects.Count %></span>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <% if (NeedsAttentionProjects.Count > 0) { %>
                    <div class="d-flex justify-content-end mb-3">
                        <div class="input-group dashboard-list-search">
                            <span class="input-group-text"><i class="bx bx-search"></i></span>
                            <input type="search" class="form-control" placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" data-dashboard-list-search="progressNeedsAttentionProjectsBody" />
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ASSESSMENT) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_PROGRESS) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VARIANCE) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></th>
                                </tr>
                            </thead>
                            <tbody id="progressNeedsAttentionProjectsBody">
                                <% foreach (var project in NeedsAttentionProjects) { %>
                                <tr data-search-row="true">
                                    <td>
                                        <div class="fw-semibold"><%: project.ProjectCode %></div>
                                        <div class="small text-muted"><%: project.ProjectName %></div>
                                    </td>
                                    <td><span class="badge <%= GetHealthBadgeCss(project.Health) %>"><%= GetHealthText(project.Health) %></span></td>
                                    <td class="text-center"><%= project.ActualProgress.ToString("0.##") %>%</td>
                                    <td class="text-center"><span class="<%= GetVarianceCss(project.Health, project.Variance) %>"><%= project.Variance > 0 ? "+" : string.Empty %><%= project.Variance.ToString("0.##") %>%</span></td>
                                    <td class="text-center">
                                        <a href="<%: GetProjectDetailUrl(project.ProjectId) %>" class="btn btn-sm btn-outline-primary">
                                            <%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %>
                                        </a>
                                    </td>
                                </tr>
                                <% } %>
                                <tr data-search-empty class="d-none">
                                    <td colspan="5" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <% } else { %>
                    <div class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_PROJECTS_FILTER) %></div>
                    <% } %>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-dark" data-bs-dismiss="modal"><i class="bx bx-x"></i><%= GetResourceText(BackEndResourceKeys.CLOSE) %></button>
                </div>
            </div>
        </div>
    </div>
    <% } %>

    <script type="text/javascript">
        (function () {
            var modal = document.getElementById("progressTaskDetailsModal");
            if (!modal) return;

            modal.addEventListener("show.bs.modal", function (event) {
                var trigger = event.relatedTarget;
                if (!trigger) return;

                var activeFilter = trigger.getAttribute("data-task-filter") || "all";
                var projectFilter = trigger.getAttribute("data-task-project-id") || "";
                var statusCodeFilter = trigger.getAttribute("data-task-status-code") || "";
                var title = trigger.getAttribute("data-task-title") || "";
                var titleElement = document.getElementById("progressTaskDetailsModalHeading");
                var countElement = document.getElementById("progressTaskDetailsCount");
                var tableContainer = document.getElementById("progressTaskDetailsTableContainer");
                var emptyState = document.getElementById("progressTaskDetailsEmpty");
                var searchInput = document.getElementById("progressTaskDetailsSearch");
                var projectSelect = document.getElementById("progressTaskProjectFilter");
                var rows = modal.querySelectorAll("tbody tr[data-task-status]");
                var groups = window.DashboardProjectGroups
                    ? window.DashboardProjectGroups.create(document.getElementById("progressTaskDetailsBody"))
                    : null;
                if (groups) groups.reset();

                if (titleElement && title) {
                    titleElement.textContent = title;
                }

                if (searchInput) searchInput.value = "";
                if (projectSelect) projectSelect.value = projectFilter;
                function applyTaskFilter() {
                    var query = searchInput
                        ? (searchInput.value || "").trim().toLocaleLowerCase()
                        : "";
                    var visibleCount = 0;

                    for (var index = 0; index < rows.length; index++) {
                        var row = rows[index];
                        var status = row.getAttribute("data-task-status");
                        var rowProjectId = row.getAttribute("data-task-project-id") || "";
                        var matchesFilter = statusCodeFilter
                            ? status === statusCodeFilter
                            : activeFilter === "all"
                                || (activeFilter === "completed" && status === "2")
                                || (activeFilter === "in-progress" && status === "1")
                                || (activeFilter === "overdue" && status === "3");
                        var selectedProjectFilter = projectSelect
                            ? projectSelect.value
                            : projectFilter;
                        var matchesProject = !selectedProjectFilter
                            || rowProjectId.toLowerCase() === selectedProjectFilter.toLowerCase();
                        var matchesSearch = !query || row.textContent
                            .toLocaleLowerCase().indexOf(query) >= 0;
                        var isVisible = matchesFilter
                            && matchesProject
                            && matchesSearch;

                        row.classList.toggle("d-none", !isVisible);
                        if (isVisible) visibleCount++;
                    }

                    if (countElement) countElement.textContent = visibleCount;
                    if (groups) groups.refresh(Boolean(query));
                    if (tableContainer) tableContainer.classList.toggle("d-none", visibleCount === 0);
                    if (emptyState) emptyState.classList.toggle("d-none", visibleCount > 0);
                }

                if (searchInput) {
                    searchInput.oninput = applyTaskFilter;
                }
                if (projectSelect) {
                    projectSelect.onchange = applyTaskFilter;
                }
                applyTaskFilter();
            });
        })();
    </script>

    <div class="row g-3 mb-3 progress-summary-row">
        <div class="col-12 col-xl-8 d-flex">
            <div class="card w-100 border-0 shadow-sm overview-project-summary-card progress-schedule-card">
                <div class="card-body">
                    <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start gap-3 mb-3">
                        <div>
                            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_VS_PLAN) %></h5>
                            <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_VS_PLAN_DESC) %></p>
                            <% if (Model.IsSingleProject && Model.ProjectScheduleStatistics.Count > 0) {
                                   var selectedProject = Model.ProjectScheduleStatistics[0]; %>
                            <div class="small text-muted mt-2"><strong><%: selectedProject.ProjectCode %></strong> · <%: selectedProject.ProjectName %></div>
                            <div class="d-flex flex-wrap gap-2 mt-2">
                                <a href="<%: GetProjectGanttUrl(selectedProject.ProjectId) %>" class="btn btn-sm btn-outline-primary">
                                    <i class="bx bx-git-branch me-1"></i><%= GetResourceText(BackEndResourceKeys.GANTT_CHART) %>
                                </a>
                                <a href="<%: GetProjectReportUrl(selectedProject.ProjectId) %>" class="btn btn-sm btn-outline-primary">
                                    <i class="bx bx-bar-chart-alt-2 me-1"></i><%= GetResourceText(BackEndResourceKeys.PROJECT_REPORT) %>
                                </a>
                                <% if (Model.ProjectStages.Count > 0) { %>
                                <button type="button" class="btn btn-sm btn-outline-primary" data-bs-toggle="modal" data-bs-target="#progressStagesModal">
                                    <i class="bx bx-layer me-1"></i><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_STAGES) %>
                                    <span class="ms-1"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_STAGES_BUTTON_SUMMARY), CompletedStageCount, Model.ProjectStages.Count) %></span>
                                    <% if (OverdueStageCount > 0) { %><span class="badge bg-danger ms-1"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_STAGE_OVERDUE_COUNT), OverdueStageCount) %></span><% } %>
                                </button>
                                <% } %>
                            </div>
                            <% } %>
                        </div>
                    </div>
                    <div class="progress-schedule-legend d-flex flex-wrap justify-content-end gap-3 mb-2" aria-label="Chú giải biểu đồ tiến độ">
                        <span class="d-inline-flex align-items-center gap-1"><span class="progress-schedule-legend-marker progress-schedule-legend-planned" aria-hidden="true"></span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PLANNED_PROGRESS) %></span>
                        <span class="d-inline-flex align-items-center gap-1"><span class="progress-schedule-legend-marker progress-schedule-legend-actual" aria-hidden="true"></span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_PROGRESS) %></span>
                    </div>
                    <div id="progress-schedule-chart-wrapper" class="progress-chart-scroll">
                        <div id="progress-schedule-chart"></div>
                    </div>
                    <a id="progressProjectTaskTrigger" href="#progressTaskDetailsModal" class="d-none"
                       data-bs-toggle="modal" data-task-filter="all" aria-hidden="true" tabindex="-1"></a>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card w-100 border-0 shadow-sm progress-task-status-card">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_STATUS) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_STATUS_DESC) %></p>
                    <div id="progress-task-status-chart"></div>
                    <a id="progressTaskStatusCategoryTrigger" href="#progressTaskDetailsModal" class="d-none"
                       data-bs-toggle="modal" data-task-filter="all" aria-hidden="true" tabindex="-1"></a>
                </div>
            </div>
        </div>
    </div>

    <% if (Model.IsSingleProject && Model.ProjectStages.Count > 0) { %>
    <div class="modal fade" id="progressStagesModal" tabindex="-1" aria-labelledby="progressStagesModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <div>
                        <h5 class="modal-title" id="progressStagesModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_STAGES) %></h5>
                        <span class="small text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_STAGES_COMPLETED_COUNT), CompletedStageCount, Model.ProjectStages.Count) %></span>
                    </div>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="table-responsive"><table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PHASE) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STAGE_TASK_COMPLETION) %></th>
                        </tr></thead>
                        <tbody>
                        <% foreach (var stage in Model.ProjectStages) { %>
                        <tr>
                            <td><strong><%: stage.Name %></strong>
                                <% if (stage.Tasks.Count > 0) { %>
                                <details class="mt-1"><summary class="small text-primary" style="cursor:pointer"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), stage.Tasks.Count) %></summary>
                                    <ul class="list-unstyled mb-0 mt-2">
                                    <% foreach (var task in stage.Tasks) { %>
                                        <li class="mb-1"><a href="<%: GetProjectTaskDetailUrl(stage.ProjectId, task.TaskId) %>"><%: task.Code %> · <%: task.Name %></a></li>
                                    <% } %>
                                    </ul>
                                </details>
                                <% } %>
                            </td>
                            <td class="text-nowrap"><%= stage.StartDate.HasValue ? stage.StartDate.Value.ToString("dd/MM/yyyy") : "—" %></td>
                            <td class="text-nowrap"><%= stage.ExpectedEndDate.HasValue ? stage.ExpectedEndDate.Value.ToString("dd/MM/yyyy") : GetResourceText(BackEndResourceKeys.DASHBOARD_STAGE_NO_DEADLINE) %>
                                <% if (stage.IsOverdue) { %><small class="d-block text-danger"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_OVERDUE), stage.DaysOverdue) %></small><% } %>
                            </td>
                            <td class="text-center"><span class="badge <%= GetStageBadgeCss(stage) %>"><%= GetStageStatusText(stage) %></span></td>
                            <td class="text-center"><%= stage.CompletedTaskCount %>/<%= stage.Tasks.Count %></td>
                        </tr>
                        <% } %>
                        </tbody>
                    </table></div>
                </div>
            </div>
        </div>
    </div>
    <% } %>

    <% if (Model.IsSingleProject) { %>
    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_PROGRESS) %></h5>
                    <p class="text-muted mb-0">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_PROGRESS_ORDER_DESC) %>
                    </p>
                </div>
                <span class="badge bg-primary-subtle text-primary align-self-start mt-2 mt-md-0">
                    <%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), Model.TaskProgressDetails.Count) %>
                </span>
            </div>

            <div class="table-responsive">
                <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                            <th style="min-width: 190px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                        </tr>
                    </thead>
                    <tbody id="progressProjectTaskListBody">
                        <% foreach (var task in Model.TaskProgressDetails) { %>
                        <tr data-search-row="true">
                            <td>
                                <a href="<%: GetProjectTaskDetailUrl(task.ProjectId, task.TaskId) %>" class="text-decoration-none text-reset">
                                    <div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div>
                                </a>
                            </td>
                            <td class="text-nowrap"><%= task.StartDate.HasValue ? task.StartDate.Value.ToString("dd/MM/yyyy") : "-" %></td>
                            <td><%: task.PriorityName %></td>
                            <td class="text-nowrap">
                                <%= task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "-" %>
                                <div class="small text-muted"><%: GetTaskDeadlineText(task) %></div>
                            </td>
                            <td>
                                <div class="d-flex align-items-center">
                                    <div class="progress flex-grow-1 progress-table-bar">
                                        <div class="progress-bar <%= GetProgressBarCss(task.Progress) %>"
                                             role="progressbar"
                                             style="width: <%= task.Progress %>%;"
                                             aria-valuenow="<%= task.Progress %>"
                                             aria-valuemin="0"
                                             aria-valuemax="100"></div>
                                    </div>
                                    <span class="small ms-2"><%= task.Progress %>%</span>
                                </div>
                            </td>
                            <td class="text-center">
                                <span class="badge <%= GetTaskStatusBadgeCss(task) %>"><%: task.Status %></span>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Model.TaskProgressDetails.Count > 0) { %>
                        <tr data-search-empty class="d-none">
                            <td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                        </tr>
                        <% } %>
                        <% if (Model.TaskProgressDetails.Count == 0) { %>
                        <tr>
                            <td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_PROJECT_TASKS) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <% } %>

    <% if (!Model.IsSingleProject) { %>
    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PRIORITY_TASKS) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PRIORITY_TASKS_DESC) %></p>
                </div>
                <span class="badge bg-danger-subtle text-danger align-self-start mt-2 mt-md-0">
                    <%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), Model.AttentionTasks.Count) %>
                </span>
            </div>

            <div class="table-responsive">
                <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                            <th style="min-width: 190px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALERT) %></th>
                        </tr>
                    </thead>
                    <tbody id="progressPriorityTaskListBody">
                        <% foreach (var task in Model.AttentionTasks) { %>
                        <tr data-search-row="true">
                            <td>
                                <a href="<%: GetProjectTaskDetailUrl(task.ProjectId, task.TaskId) %>" class="text-decoration-none text-reset">
                                    <div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div>
                                </a>
                            </td>
                            <td>
                                <a href="<%: GetProjectDetailUrl(task.ProjectId) %>" class="d-block text-decoration-none text-reset">
                                    <div><%: task.ProjectCode %></div>
                                    <div class="small text-muted"><%: task.ProjectName %></div>
                                </a>
                            </td>
                            <td class="text-nowrap"><%= task.StartDate.HasValue ? task.StartDate.Value.ToString("dd/MM/yyyy") : "-" %></td>
                            <td><%: task.PriorityName %></td>
                            <td class="text-nowrap">
                                <%= task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "-" %>
                            </td>
                            <td>
                                <div class="d-flex align-items-center">
                                    <div class="progress flex-grow-1 progress-table-bar">
                                        <div class="progress-bar <%= GetProgressBarCss(task.Progress) %>"
                                             role="progressbar"
                                             style="width: <%= task.Progress %>%;"
                                             aria-valuenow="<%= task.Progress %>"
                                             aria-valuemin="0"
                                             aria-valuemax="100"></div>
                                    </div>
                                    <span class="small ms-2"><%= task.Progress %>%</span>
                                </div>
                            </td>
                            <td class="text-center">
                                <span class="badge <%= task.IsOverdue ? "bg-danger-subtle text-danger" : "bg-warning-subtle text-warning" %>">
                                    <%= GetDeadlineText(task) %>
                                </span>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Model.AttentionTasks.Count > 0) { %>
                        <tr data-search-empty class="d-none">
                            <td colspan="7" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                        </tr>
                        <% } %>
                        <% if (Model.AttentionTasks.Count == 0) { %>
                        <tr>
                            <td colspan="7" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_ATTENTION_TASKS) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <% } %>

    <script type="text/javascript">
        window.dashboardProgressProjectData = <%= ProjectProgressChartData %>;
        window.dashboardProgressTaskStatusData = <%= TaskStatusChartData %>;
        window.dashboardProgressTexts = <%= DashboardTextsJson %>;
    </script>
</div>
