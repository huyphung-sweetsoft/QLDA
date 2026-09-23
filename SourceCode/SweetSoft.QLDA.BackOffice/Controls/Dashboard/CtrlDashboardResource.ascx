<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardResource.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardResource" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

<div class="container-fluid dashboard-resource">
    <div class="d-flex flex-column flex-xl-row align-items-xl-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1 <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_TITLE) %></h4>
            <div class="row g-2 mt-2 align-items-end">
                <div class="col-12 col-md-5 col-xl-4 <%= IsProjectDashboard ? "d-none" : string.Empty %>">
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

                <% if (IsProjectDashboard) { %>
                <div class="col-auto d-flex align-items-end dashboard-project-tabs-inline">
                    <SweetSoft:CtrlProjectDashboardTabs
                        runat="server"
                        ID="CtrlProjectDashboardTabs1" />
                </div>
                <% } %>

                <div class="col-7 col-md-3 col-xl-2 <%= IsProjectDashboard ? "ms-auto" : string.Empty %>">
                    <label class="form-label mb-1 text-nowrap <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DISPLAY_RANGE) %></label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlWeekCount"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true"
                        OnSelectedIndexChanged="ddlWeekCount_SelectedIndexChanged">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-5 col-md-auto <%= IsProjectDashboard ? "d-none" : string.Empty %>">
                    <SweetSoft:ExtraButton
                        ID="btnApplyResourceFilter"
                        runat="server"
                        CssClass="w-100 px-4"
                        ButtonStyle="Primary"
                        ButtonIcon="Search"
                        OnClick="btnApplyResourceFilter_Click">
                    </SweetSoft:ExtraButton>
                </div>
            </div>
        </div>

        <div class="resource-week-navigator bg-white border rounded shadow-sm mt-3 mt-xl-0 ms-xl-4 <%= IsProjectDashboard ? "dashboard-project-date-card dashboard-project-week-navigator" : string.Empty %>">
            <div class="small text-muted text-uppercase fw-medium mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_FOCUS_WEEK) %></div>
            <div class="d-flex align-items-center justify-content-between gap-2">
                <asp:LinkButton
                    ID="btnPreviousWeek"
                    runat="server"
                    CssClass="btn btn-outline-secondary btn-sm resource-week-button"
                    OnClick="btnPreviousWeek_Click">
                    <i class="bx bx-chevron-left"></i>
                </asp:LinkButton>
                <div class="text-center text-nowrap">
                    <div class="fw-semibold">
                        <%= Model.AnchorWeekStart.ToString("dd/MM") %> – <%= Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %>
                    </div>
                    <asp:LinkButton
                        ID="btnCurrentWeek"
                        runat="server"
                        CssClass="resource-today-link"
                        OnClick="btnCurrentWeek_Click">
                    </asp:LinkButton>
                </div>
                <asp:LinkButton
                    ID="btnNextWeek"
                    runat="server"
                    CssClass="btn btn-outline-secondary btn-sm resource-week-button"
                    OnClick="btnNextWeek_Click">
                    <i class="bx bx-chevron-right"></i>
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <div class="alert alert-info resource-method-note d-flex align-items-start mb-3 <%= IsProjectDashboard ? "d-none" : string.Empty %>" role="alert">
        <i class="bx bx-info-circle fs-4 me-2"></i>
        <div>
            <%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_CALCULATION_DESC) %>
        </div>
    </div>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xxl-5 g-3 mb-3">
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_IN_SCOPE) %></div>
                    <div class="d-flex align-items-end justify-content-between">
                        <h3 class="mb-0"><%= Model.TotalEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-group"></i></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALLOCATED_EMPLOYEES) %></div>
                    <div class="d-flex align-items-end justify-content-between">
                        <div><h3 class="mb-0 text-primary"><%= Model.AssignedEmployeeCount %></h3><small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_SHARED_UTILIZATION), Model.AverageUtilization.ToString("0.#")) %></small></div>
                        <span class="resource-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-user-check"></i></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UNDERLOADED) %> (&lt;80%)</div>
                    <div class="d-flex align-items-end justify-content-between">
                        <h3 class="mb-0 text-success"><%= Model.UnderloadedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-success-subtle text-success"><i class="bx bx-down-arrow-alt"></i></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_BALANCED_LOAD) %> (80–100%)</div>
                    <div class="d-flex align-items-end justify-content-between">
                        <h3 class="mb-0 text-warning"><%= Model.BalancedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-warning-subtle text-warning"><i class="bx bx-check-shield"></i></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %> (&gt;100%)</div>
                    <div class="d-flex align-items-end justify-content-between">
                        <h3 class="mb-0 text-danger"><%= Model.OverloadedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-error-circle"></i></span>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3 resource-heatmap-card">
        <div class="card-body">
            <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_RESOURCE_HEATMAP) %></h5>
                    <p class="text-muted mb-0">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_HEATMAP_DESC) %>
                    </p>
                </div>
                <div class="resource-legend d-flex flex-wrap gap-3 mt-3 mt-lg-0 small">
                    <span><i class="resource-legend-swatch resource-load-low"></i>&lt;80% <%= GetResourceText(BackEndResourceKeys.DASHBOARD_UNDERLOADED) %></span>
                    <span><i class="resource-legend-swatch resource-load-balanced"></i>80–100% <%= GetResourceText(BackEndResourceKeys.DASHBOARD_BALANCED_LOAD) %></span>
                    <span><i class="resource-legend-swatch resource-load-over"></i>&gt;100% <%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %></span>
                </div>
            </div>

            <div class="resource-heatmap-scroll">
                <table class="table dashboard-data-table table-bordered resource-heatmap-table align-middle mb-0">
                    <thead>
                        <tr class="resource-week-row">
                            <th class="resource-person-column"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></th>
                            <% foreach (var week in Model.Weeks) { %>
                            <th class="text-center resource-week-column <%= week.IsAnchorWeek ? "resource-anchor-week" : string.Empty %>">
                                <span><%= week.Label %></span>
                                <small><%= week.StartDate.ToString("dd/MM") %>–<%= week.EndDate.ToString("dd/MM") %></small>
                            </th>
                            <% } %>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var employee in Model.EmployeeLoads) { %>
                        <tr>
                            <td class="resource-person-column">
                                <div class="fw-semibold text-dark">
                                    <a class="text-decoration-none text-reset" href="<%: GetEmployeeDetailUrl(employee.EmployeeId) %>"><%: employee.DisplayName %></a>
                                </div>
                                <div class="small text-muted text-truncate resource-person-meta"><%: GetEmployeeMeta(employee) %></div>
                            </td>
                            <% foreach (var week in Model.Weeks) {
                                   var load = employee.WeeklyLoads.First(x => x.WeekStart == week.StartDate); %>
                            <td class="resource-load-cell <%= week.IsAnchorWeek ? "resource-anchor-week-cell" : string.Empty %>">
                                <button
                                    type="button"
                                    class="resource-load-button <%= GetHeatmapCss(load.AllocationPercent) %>"
                                    data-resource-person="<%= employee.EmployeeId %>"
                                    data-resource-week="<%= week.StartDate.ToString("yyyy-MM-dd") %>"
                                    aria-label="<%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_EMPLOYEE_ALLOCATION), employee.DisplayName, week.Label) %>">
                                    <span class="resource-load-percent"><%= GetCellText(load.AllocationPercent) %></span>
                                    <small><%= GetAllocatedDaysText(load) %></small>
                                </button>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <% if (Model.EmployeeLoads.Count == 0) { %>
                        <tr>
                            <td colspan="<%= Model.Weeks.Count + 1 %>" class="text-center text-muted py-5">
                                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_MEMBERS) %>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3 align-items-stretch">
        <div class="col-12 col-xl-8 d-flex flex-column">
            <div class="card border-0 shadow-sm w-100 h-100 flex-grow-1 resource-monthly-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_SUMMARY) %></h5>
                    <p class="text-muted mb-3">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_DESC) %>
                    </p>
                    <div class="resource-monthly-scroll flex-grow-1">
                        <table class="table dashboard-data-table table-bordered resource-monthly-table align-middle mb-0">
                            <thead>
                                <tr>
                                    <th class="resource-person-column"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></th>
                                    <% foreach (var month in Model.Months) { %>
                                    <th class="text-center resource-month-column"><%= month.Label %></th>
                                    <% } %>
                                </tr>
                            </thead>
                            <tbody>
                                <% foreach (var employee in Model.EmployeeLoads) { %>
                                <tr>
                                    <td class="resource-person-column">
                                        <div class="fw-semibold text-dark">
                                            <a class="text-decoration-none text-reset" href="<%: GetEmployeeDetailUrl(employee.EmployeeId) %>"><%: employee.DisplayName %></a>
                                        </div>
                                        <div class="small text-muted text-truncate resource-person-meta"><%: GetEmployeeMeta(employee) %></div>
                                    </td>
                                    <% foreach (var month in Model.Months) {
                                           var load = employee.MonthlyLoads.First(x => x.MonthStart == month.StartDate); %>
                                    <td class="text-center resource-month-cell <%= GetMonthlySummaryCss(load) %>"
                                        title="<%: GetMonthlyStatusTitle(load) %>">
                                        <div class="fw-semibold resource-month-percent"><%= load.AverageUtilization.ToString("0.#") %>%</div>
                                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_ALLOCATED_CAPACITY), load.AllocatedDays.ToString("0.#"), load.CapacityDays.ToString("0")) %></small>
                                        <div class="mt-1"><span class="badge <%= GetMonthlyStatusBadgeCss(load) %>"><%= GetMonthlyStatusText(load) %></span></div>
                                    </td>
                                    <% } %>
                                </tr>
                                <% } %>
                                <% if (Model.EmployeeLoads.Count == 0) { %>
                                <tr>
                                    <td colspan="<%= Model.Months.Count + 1 %>" class="text-center text-muted py-4">
                                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_MONTHLY_EMPLOYEES) %>
                                    </td>
                                </tr>
                                <% } %>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex flex-column">
            <div class="card border-0 shadow-sm w-100 h-100 flex-grow-1 resource-attention-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <div class="d-flex justify-content-between align-items-start mb-3 flex-shrink-0">
                        <div>
                            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %></h5>
                            <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION_DESC) %></p>
                        </div>
                        <span class="badge bg-warning-subtle text-warning"><%= Model.AttentionEmployees.Count %></span>
                    </div>
                    <div class="resource-attention-list flex-grow-1">
                        <% foreach (var employee in Model.AttentionEmployees) { %>
                        <div class="resource-attention-item d-flex align-items-start">
                            <span class="resource-attention-dot <%= GetStatusLoadCss(employee.Status) %>"></span>
                            <div class="flex-grow-1 min-width-0">
                                <div class="d-flex justify-content-between gap-2">
                                    <a class="fw-semibold text-truncate text-decoration-none text-reset" href="<%: GetEmployeeDetailUrl(employee.EmployeeId) %>"><%: employee.DisplayName %></a>
                                    <span class="badge <%= GetStatusBadgeCss(employee.Status) %>"><%= GetStatusText(employee.Status) %></span>
                                </div>
                                <div class="small text-muted mt-1"><%: GetAttentionText(employee) %></div>
                            </div>
                        </div>
                        <% } %>
                        <% if (Model.AttentionEmployees.Count == 0) { %>
                        <div class="text-center text-muted py-5"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_WARNINGS) %></div>
                        <% } %>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-sm-row justify-content-between mb-2">
                <div>
                    <h5 class="card-title text-uppercase mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_LOAD_TREND) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_LOAD_TREND_DESC) %></p>
                </div>
                <div class="small text-muted mt-2 mt-sm-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_FUTURE_WEEK_DASHED) %></div>
            </div>
            <div id="resource-load-trend-chart"></div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_ALLOCATION) %></h5>
            <p class="text-muted mb-3">
                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_ALLOCATION_DESC) %>
            </p>
            <div class="table-responsive">
                <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_SCHEDULED_EMPLOYEES) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALLOCATED_DAYS) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AVAILABLE_CAPACITY) %></th>
                            <th style="min-width: 210px;"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UTILIZATION) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ASSESSMENT) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectAllocations) { %>
                        <tr>
                            <td>
                                <a class="d-block text-decoration-none text-reset" href="<%: GetProjectDetailUrl(project.ProjectId) %>">
                                    <div class="fw-semibold"><%: project.ProjectCode %></div>
                                    <div class="small text-muted"><%: project.ProjectName %></div>
                                </a>
                            </td>
                            <td class="text-center"><%= project.ResourceCount %></td>
                            <td class="text-center"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DAY_COUNT), project.AllocatedDays.ToString("0.#")) %></td>
                            <td class="text-center"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DAY_COUNT), project.CapacityDays.ToString("0.#")) %></td>
                            <td>
                                <div class="d-flex align-items-center gap-2">
                                    <div class="progress flex-grow-1 resource-utilization-progress">
                                        <div class="progress-bar <%= project.Status == SweetSoft.QLDA.Core.Dashboard.ResourceLoadStatus.Overloaded ? "bg-danger" : project.Status == SweetSoft.QLDA.Core.Dashboard.ResourceLoadStatus.Balanced ? "bg-warning" : "bg-success" %>"
                                             role="progressbar"
                                             style="width: <%= Math.Min(project.Utilization, 100).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) %>%;"
                                             aria-valuenow="<%= project.Utilization %>"
                                             aria-valuemin="0"
                                             aria-valuemax="100"></div>
                                    </div>
                                    <strong class="text-nowrap"><%= project.Utilization.ToString("0.#") %>%</strong>
                                </div>
                            </td>
                            <td class="text-center"><span class="badge <%= GetStatusBadgeCss(project.Status) %>"><%= GetStatusText(project.Status) %></span></td>
                        </tr>
                        <% } %>
                        <% if (Model.ProjectAllocations.Count == 0) { %>
                        <tr><td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_PROJECT_ALLOCATION) %></td></tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div id="resource-detail-backdrop" class="resource-detail-backdrop" hidden></div>
    <aside id="resource-detail-drawer" class="resource-detail-drawer" aria-hidden="true" aria-labelledby="resource-detail-title">
        <div class="resource-drawer-header d-flex align-items-start justify-content-between">
            <div>
                <div class="small text-muted text-uppercase"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_ALLOCATION_DETAIL) %></div>
                <h5 id="resource-detail-title" class="mb-1 mt-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></h5>
                <div id="resource-detail-subtitle" class="small text-muted"></div>
            </div>
            <button id="resource-detail-close" type="button" class="btn btn-sm btn-light" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"><i class="bx bx-x fs-4"></i></button>
        </div>
        <div class="resource-drawer-summary">
            <div>
                <span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_LOAD) %></span>
                <small id="resource-detail-capacity" class="d-block text-muted mt-1"></small>
            </div>
            <strong id="resource-detail-load">0%</strong>
        </div>
        <div class="resource-drawer-body">
            <div id="resource-detail-formula" class="resource-drawer-formula small text-muted"></div>
            <div id="resource-detail-week-status" class="resource-drawer-week-status"></div>
            <h6 class="resource-drawer-section-title mt-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_DAILY_ALLOCATION_DETAIL) %></h6>
            <div id="resource-detail-days"></div>
            <h6 class="resource-drawer-section-title"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_ALLOCATION) %></h6>
            <div id="resource-detail-projects"></div>
            <h6 class="resource-drawer-section-title mt-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_DETAIL) %></h6>
            <div id="resource-detail-tasks"></div>
        </div>
    </aside>

    <script type="text/javascript">
        window.dashboardResourceTrendData = <%= TrendChartData %>;
        window.dashboardResourceDetailData = <%= ResourceDetailData %>;
        window.dashboardResourceTexts = <%= DashboardTextsJson %>;
    </script>
</div>
