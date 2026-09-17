<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardProgress.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardProgress" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-progress">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_TITLE) %></h4>

            <div class="row g-2 mt-3 align-items-end">
                <div class="col-12 col-sm-6 col-md-5 col-xl-4">
                    <label class="form-label mb-1 text-nowrap">
                        <%= GetResourceText(BackEndResourceKeys.PROJECT_SCOPE) %>
                    </label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlProjectFilter"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-12 col-sm-6 col-md-4 col-xl-3">
                    <label class="form-label mb-1 text-nowrap">
                        <%= GetResourceText(BackEndResourceKeys.DATE_RANGE) %>
                    </label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlDateRange"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-12 col-md-3 col-xl-auto mt-3 mt-md-0">
                    <SweetSoft:ExtraButton
                        ID="btnApplyDashboardFilter"
                        runat="server"
                        CssClass="w-100 px-4"
                        ButtonStyle="Primary"
                        ButtonIcon="Search"
                        OnClick="btnApplyDashboardFilter_Click">
                    </SweetSoft:ExtraButton>
                </div>
            </div>

            <p class="text-muted mt-2 mb-0">
                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_FILTER_DESC) %>
            </p>
        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 progress-date-card">
            <div class="bg-primary-subtle text-primary rounded-circle d-flex align-items-center justify-content-center me-3 progress-date-icon">
                <i class="bx bx-line-chart fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UPDATED_AT) %></div>
                <div class="fw-bold text-dark lh-1"><%= Model.GeneratedAt.ToString("HH:mm dd/MM/yyyy") %></div>
            </div>
        </div>
    </div>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-6 g-3 mb-4">
        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AVERAGE_PROGRESS) %></p>
                        <h3 class="mb-0 text-primary"><%= Model.OverallProgress.ToString("0.##") %>%</h3>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_COUNT), Model.TotalProjectCount) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-primary-subtle text-primary progress-kpi-icon">
                        <i class="bx bx-trending-up fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_TASKS) %></p>
                        <h3 class="mb-0"><%= Model.TotalTaskCount %></h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_FILTERED_PROJECT_TASKS) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-info-subtle text-info progress-kpi-icon">
                        <i class="bx bx-task fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COMPLETED_TASKS) %></p>
                        <h3 class="mb-0 text-success"><%= Model.CompletedTaskCount %></h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_REACHED_100) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-success-subtle text-success progress-kpi-icon">
                        <i class="bx bx-check-circle fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_IN_PROGRESS) %></p>
                        <h3 class="mb-0 text-info"><%= Model.InProgressTaskCount %></h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_RANGE_1_99) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-info-subtle text-info progress-kpi-icon">
                        <i class="bx bx-loader-circle fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE) %></p>
                        <h3 class="mb-0 text-danger"><%= Model.OverdueTaskCount %></h3>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DUE_SOON_TASK_COUNT), Model.DueSoonTaskCount) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-danger-subtle text-danger progress-kpi-icon">
                        <i class="bx bx-error-circle fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <% if (Model.IsSingleProject) { %>
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VARIANCE) %></p>
                        <h3 class="mb-0 <%= GetSelectedProjectVarianceCss() %>">
                            <%= GetSelectedProjectVarianceText() %>
                        </h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_MINUS_PLAN) %></small>
                        <% } else { %>
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NEEDS_ATTENTION) %></p>
                        <h3 class="mb-0 text-warning"><%= Model.NeedsAttentionProjectCount %></h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NEEDS_ATTENTION_DESC) %></small>
                        <% } %>
                    </div>
                    <% if (Model.IsSingleProject) { %>
                    <span class="avatar-title rounded-circle bg-primary-subtle text-primary progress-kpi-icon">
                        <i class="bx bx-git-compare fs-4"></i>
                    </span>
                    <% } else { %>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning progress-kpi-icon">
                        <i class="bx bx-alarm-exclamation fs-4"></i>
                    </span>
                    <% } %>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3 progress-summary-row">
        <div class="col-12 col-xl-8 d-flex">
            <div class="card w-100 border-0 shadow-sm overview-project-summary-card progress-schedule-card">
                <div class="card-body">
                    <% if (Model.IsSingleProject && Model.ProjectScheduleStatistics.Count > 0) {
                           var selectedProject = Model.ProjectScheduleStatistics[0]; %>
                    <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-start mb-3">
                        <div>
                            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_VS_PLAN) %></h5>
                            <p class="text-muted mb-0">
                                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_VS_PLAN_DESC) %>
                            </p>
                        </div>
                        <span class="badge <%= GetHealthBadgeCss(selectedProject.Health) %> mt-2 mt-md-0">
                            <%= GetHealthText(selectedProject.Health) %>
                        </span>
                    </div>

                    <a href="<%: GetProjectDetailUrl(selectedProject.ProjectId) %>" class="d-block text-decoration-none text-reset">
                        <div class="overview-project-identity p-3 rounded mb-3">
                            <div class="small text-muted mb-1"><%: selectedProject.ProjectCode %></div>
                            <div class="fw-bold fs-5"><%: selectedProject.ProjectName %></div>
                        </div>
                    </a>

                    <% if (Model.CurrentStage != null) { %>
                    <div class="overview-project-identity px-3 py-2 rounded mb-3">
                        <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CURRENT_PROJECT_STAGE) %></div>
                        <div class="fw-semibold"><%: Model.CurrentStage.Name %></div>
                    </div>
                    <% } %>

                    <div class="d-flex flex-wrap gap-2 mb-3">
                        <a href="<%: GetProjectGanttUrl(selectedProject.ProjectId) %>" class="btn btn-sm btn-outline-primary">
                            <i class="bx bx-git-branch me-1"></i><%= GetResourceText(BackEndResourceKeys.GANTT_CHART) %>
                        </a>
                        <a href="<%: GetProjectReportUrl(selectedProject.ProjectId) %>" class="btn btn-sm btn-outline-primary">
                            <i class="bx bx-bar-chart-alt-2 me-1"></i><%= GetResourceText(BackEndResourceKeys.PROJECT_REPORT) %>
                        </a>
                    </div>

                    <div class="row g-3 mb-4">
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.START_DATE) %></div>
                            <div class="fw-semibold"><%= selectedProject.StartDate.ToString("dd/MM/yyyy") %></div>
                        </div>
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EXPECTED_COMPLETION) %></div>
                            <div class="fw-semibold"><%= selectedProject.ExpectedEndDate.ToString("dd/MM/yyyy") %></div>
                        </div>
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_COMPLETION) %></div>
                            <div class="fw-semibold"><%= GetActualCompletionText(selectedProject) %></div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <span class="fw-medium"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_PROGRESS) %></span>
                            <span class="fw-bold text-primary"><%= selectedProject.ActualProgress.ToString("0.##") %>%</span>
                        </div>
                        <div class="progress overview-progress-bar">
                            <div class="progress-bar <%= GetProgressBarCss(selectedProject.ActualProgress) %>"
                                 role="progressbar"
                                 style="width: <%= GetPercentStyle(selectedProject.ActualProgress) %>%;"
                                 aria-valuenow="<%= selectedProject.ActualProgress %>"
                                 aria-valuemin="0"
                                 aria-valuemax="100"></div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <span class="fw-medium"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PLANNED_PROGRESS) %></span>
                            <span class="fw-bold text-warning"><%= selectedProject.PlannedProgress.ToString("0.##") %>%</span>
                        </div>
                        <div class="progress overview-progress-bar">
                            <div class="progress-bar bg-warning"
                                 role="progressbar"
                                 style="width: <%= GetPercentStyle(selectedProject.PlannedProgress) %>%;"
                                 aria-valuenow="<%= selectedProject.PlannedProgress %>"
                                 aria-valuemin="0"
                                 aria-valuemax="100"></div>
                        </div>
                    </div>

                    <div class="overview-variance-box d-flex flex-column flex-sm-row align-items-sm-center justify-content-between p-3 rounded">
                        <div>
                            <div class="fw-semibold"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_VARIANCE) %></div>
                            <div class="small text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_VARIANCE_DESC) %></div>
                        </div>
                        <div class="fs-4 mt-2 mt-sm-0 <%= GetVarianceCss(selectedProject.Health, selectedProject.Variance) %>">
                            <%= GetVarianceText(selectedProject.Variance) %>%
                        </div>
                    </div>
                    <% } else { %>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_VS_PLAN) %></h5>
                    <p class="text-muted mb-3">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PLAN_TIME_DESC) %>
                    </p>
                    <div id="progress-schedule-chart-wrapper" class="progress-chart-scroll">
                        <div id="progress-schedule-chart"></div>
                    </div>
                    <% } %>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card w-100 border-0 shadow-sm progress-task-status-card">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_STATUS) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_STATUS_DESC) %></p>
                    <div id="progress-task-status-chart"></div>
                </div>
            </div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <% if (Model.IsSingleProject) { %>
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
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                            <th style="min-width: 190px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var task in Model.TaskProgressDetails) { %>
                        <tr>
                            <td>
                                <a href="<%: GetProjectTasksUrl(task.ProjectId) %>" class="text-decoration-none text-reset">
                                    <div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div>
                                </a>
                            </td>
                            <td>
                                <a href="<%: GetProjectDetailUrl(task.ProjectId) %>" class="d-block text-decoration-none text-reset">
                                    <div><%: task.ProjectCode %></div>
                                    <div class="small text-muted"><%: task.ProjectName %></div>
                                </a>
                            </td>
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
                        <% if (Model.TaskProgressDetails.Count == 0) { %>
                        <tr>
                            <td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_PROJECT_TASKS) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
            <% } else { %>
            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_TASK_STRUCTURE) %></h5>
            <p class="text-muted mb-3">
                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_TASK_STRUCTURE_DESC) %>
            </p>
            <div id="progress-project-task-chart-wrapper" class="progress-chart-scroll">
                <div id="progress-project-task-chart"></div>
            </div>
            <% } %>
        </div>
    </div>

    <% if (!Model.IsSingleProject) { %>
    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_SCHEDULE_HEALTH) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_SCHEDULE_HEALTH_DESC) %></p>
                </div>
                <div class="small text-muted mt-2 mt-md-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VARIANCE_FORMULA) %></div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PLANNED_TIME) %></th>
                            <th style="min-width: 190px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ACTUAL_PROGRESS) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PLANNED_PROGRESS) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VARIANCE) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ASSESSMENT) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectScheduleStatistics) { %>
                        <tr>
                            <td>
                                <a href="<%: GetProjectDetailUrl(project.ProjectId) %>" class="d-block text-decoration-none text-reset">
                                    <div class="fw-semibold"><%: project.ProjectCode %></div>
                                    <div class="small text-muted"><%: project.ProjectName %></div>
                                </a>
                            </td>
                            <td class="text-nowrap">
                                <%= project.StartDate.ToString("dd/MM/yyyy") %>
                                <span class="text-muted mx-1">→</span>
                                <%= project.ExpectedEndDate.ToString("dd/MM/yyyy") %>
                            </td>
                            <td>
                                <div class="d-flex align-items-center">
                                    <div class="progress flex-grow-1 progress-table-bar">
                                        <div class="progress-bar <%= GetProgressBarCss(project.ActualProgress) %>"
                                             role="progressbar"
                                             style="width: <%= project.ActualProgress.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) %>%;"
                                             aria-valuenow="<%= project.ActualProgress %>"
                                             aria-valuemin="0"
                                             aria-valuemax="100"></div>
                                    </div>
                                    <span class="small fw-semibold ms-2"><%= project.ActualProgress.ToString("0.##") %>%</span>
                                </div>
                            </td>
                            <td class="text-center"><%= project.PlannedProgress.ToString("0.##") %>%</td>
                            <td class="text-center">
                                <span class="<%= GetVarianceCss(project.Health, project.Variance) %>">
                                    <%= project.Variance > 0 ? "+" : string.Empty %><%= project.Variance.ToString("0.##") %>%
                                </span>
                            </td>
                            <td class="text-center">
                                <a href="<%: GetProjectTasksUrl(project.ProjectId) %>" class="text-decoration-none text-reset">
                                    <%= project.CompletedTaskCount %>/<%= project.TotalTaskCount %>
                                </a>
                            </td>
                            <td class="text-center">
                                <a href="<%: GetProjectTasksUrl(project.ProjectId) %>" class="text-decoration-none <%= project.OverdueTaskCount > 0 ? "text-danger fw-semibold" : "text-muted" %>">
                                    <%= project.OverdueTaskCount %>
                                </a>
                            </td>
                            <td class="text-center">
                                <span class="badge <%= GetHealthBadgeCss(project.Health) %>">
                                    <%= GetHealthText(project.Health) %>
                                </span>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Model.ProjectScheduleStatistics.Count == 0) { %>
                        <tr>
                            <td colspan="8" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_PROJECTS_FILTER) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

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
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DEADLINE) %></th>
                            <th style="min-width: 190px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS_COLUMN) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALERT) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var task in Model.AttentionTasks) { %>
                        <tr>
                            <td>
                                <a href="<%: GetProjectTasksUrl(task.ProjectId) %>" class="text-decoration-none text-reset">
                                    <div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div>
                                </a>
                            </td>
                            <td>
                                <a href="<%: GetProjectDetailUrl(task.ProjectId) %>" class="d-block text-decoration-none text-reset">
                                    <div><%: task.ProjectCode %></div>
                                    <div class="small text-muted"><%: task.ProjectName %></div>
                                </a>
                            </td>
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
                        <% if (Model.AttentionTasks.Count == 0) { %>
                        <tr>
                            <td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_ATTENTION_TASKS) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <% } %>

    <script type="text/javascript">
        window.dashboardProgressScheduleData = <%= ProjectScheduleChartData %>;
        window.dashboardProgressTaskStatusData = <%= TaskStatusChartData %>;
        window.dashboardProgressTexts = <%= DashboardTextsJson %>;
        window.dashboardProgressProjectTaskData = <%= ProjectTaskChartData %>;
        window.dashboardProgressIsSingleProject = <%= Model.IsSingleProject.ToString().ToLowerInvariant() %>;
    </script>
</div>
