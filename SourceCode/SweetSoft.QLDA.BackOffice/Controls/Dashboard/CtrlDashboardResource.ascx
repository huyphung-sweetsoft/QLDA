<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardResource.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardResource" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

<div class="container-fluid dashboard-resource">
    <div class="d-flex flex-column flex-xl-row align-items-xl-start justify-content-between mb-3 resource-dashboard-heading <%= IsProjectDashboard ? "dashboard-project-heading" : string.Empty %>">
        <div class="flex-grow-1">
            <h4 class="mb-1 <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_TITLE) %></h4>
            <div class="row g-2 <%= IsProjectDashboard ? "mt-0" : "mt-2" %> align-items-end">
                <div class="col-12 col-md-5 col-xl-4 <%= IsProjectDashboard ? "d-none" : string.Empty %>">
                    <label class="form-label mb-1 text-nowrap">
                        <%= GetResourceText(BackEndResourceKeys.PROJECT_SCOPE) %>
                    </label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlProjectFilter"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true"
                        MinimumResultsForSearch="0"
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

                <div class="d-none col-7 col-md-3 col-xl-2 <%= IsProjectDashboard ? "ms-auto" : string.Empty %>">
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

            </div>
        </div>

        <div class="resource-week-navigator bg-white border rounded shadow-sm mt-3 mt-xl-0 ms-xl-4 <%= IsProjectDashboard ? "dashboard-project-date-card dashboard-project-week-navigator" : string.Empty %>">
            <div class="small text-muted text-uppercase fw-medium mb-2"><%= GetResourceText(IsMonthlyView ? BackEndResourceKeys.DASHBOARD_FOCUS_MONTHS : BackEndResourceKeys.DASHBOARD_FOCUS_WEEK) %></div>
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
                        <% if (IsMonthlyView) { %>
                        <%= Model.Months.First().StartDate.ToString("MM/yyyy") %> – <%= Model.Months.Last().StartDate.ToString("MM/yyyy") %>
                        <% } else { %>
                        <%= Model.AnchorWeekStart.ToString("dd/MM") %> – <%= Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %>
                        <% } %>
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

    <div class="resource-summary-header d-flex flex-wrap align-items-center justify-content-between gap-2 mb-2">
        <h5 class="mb-0"><%= IsMonthlyView
            ? string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_MONTH_KPI_TITLE), Model.Months.First().StartDate.ToString("MM/yyyy"))
            : GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_RESOURCE_HEATMAP) + " · " + Model.AnchorWeekStart.ToString("dd/MM") + "–" + Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %>
        </h5>
    </div>
    <div class="row g-3 mb-3 align-items-stretch resource-insights-grid <%= IsMonthlyView ? string.Empty : "resource-insights-grid-weekly" %>">
        <div class="<%= IsMonthlyView ? "col-12" : "col-12 col-xl-5" %> d-flex flex-column">
            <div class="card border-0 shadow-sm h-100 resource-load-distribution-card">
                <div class="card-body">
                    <h5 class="card-title mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_LOAD_DISTRIBUTION) %></h5>
                    <div class="resource-load-distribution">
                        <div class="resource-load-donut"
                           style="<%= GetResourceLoadChartStyle() %>"
                           data-resource-values="<%= GetResourceLoadCount("free") %>,<%= GetResourceLoadCount("normal") %>,<%= GetResourceLoadCount("overloaded") %>">
                            <a href="#resourceEmployeesModal" class="resource-load-donut-center resource-kpi-trigger"
                               data-bs-toggle="modal" data-resource-filter="all"
                               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_EMPLOYEES) %>"
                               aria-label="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_EMPLOYEES) + ": " + Model.TotalEmployeeCount %>">
                                <strong><%= Model.TotalEmployeeCount %></strong>
                                <small><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_EMPLOYEES) %></small>
                            </a>
                        </div>
                        <div class="resource-load-legend">
                            <a href="#resourceEmployeesModal" class="resource-load-legend-item resource-kpi-trigger"
                               data-bs-toggle="modal" data-resource-filter="free"
                               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_FREE) %>">
                                <span class="resource-load-legend-marker resource-load-free-marker"></span>
                                <span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_FREE) %></span>
                            </a>
                            <a href="#resourceEmployeesModal" class="resource-load-legend-item resource-kpi-trigger"
                               data-bs-toggle="modal" data-resource-filter="normal"
                               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_NORMAL) %>">
                                <span class="resource-load-legend-marker resource-load-normal-marker"></span>
                                <span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_NORMAL) %></span>
                            </a>
                            <a href="#resourceEmployeesModal" class="resource-load-legend-item resource-kpi-trigger"
                               data-bs-toggle="modal" data-resource-filter="overloaded"
                               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %>">
                                <span class="resource-load-legend-marker resource-load-over-marker"></span>
                                <span><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %></span>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <% if (!IsMonthlyView) { %>
        <div class="col-12 col-xl-7 d-flex flex-column">
            <div class="card border-0 shadow-sm w-100 h-100 resource-attention-card">
                <div class="card-body d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-center mb-2 flex-shrink-0">
                        <div>
                            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %></h5>
                            <p class="text-muted small mb-0"><%= Model.AnchorWeekStart.ToString("dd/MM") %>–<%= Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %></p>
                            <p class="resource-attention-help small mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION_DESC) %></p>
                        </div>
                        <span class="badge bg-warning-subtle text-warning"><%= Model.NoLoadEmployeeCount + Model.OverloadedEmployeeCount %></span>
                    </div>
                    <div class="resource-attention-list flex-grow-1">
                        <% foreach (var employee in Model.AttentionEmployees) { %>
                        <details class="resource-attention-item">
                            <summary class="resource-attention-summary">
                                <span class="resource-attention-dot <%= GetEmployeeStatusLoadCss(employee) %>"></span>
                                <span class="resource-attention-person">
                                    <span class="resource-attention-person-heading">
                                        <strong class="text-truncate"><%: employee.DisplayName %></strong>
                                        <span class="badge <%= GetEmployeeStatusBadgeCss(employee) %>"><%= GetEmployeeStatusText(employee) %></span>
                                        <i class="bx bx-chevron-down resource-attention-chevron" aria-hidden="true"></i>
                                    </span>
                                    <small class="resource-attention-summary-text"><%: GetAttentionText(employee) %></small>
                                </span>
                            </summary>
                            <div class="resource-attention-details">
                                <% var overloadedDays = GetOverloadedDays(employee); %>
                                <% if (overloadedDays.Count > 0) { %>
                                    <% foreach (var overloadedDay in overloadedDays) { %>
                                    <button type="button" class="resource-attention-day resource-open-day"
                                        data-resource-person="<%= employee.EmployeeId %>"
                                        data-resource-day="<%= overloadedDay.Date.ToString("yyyy-MM-dd") %>"
                                        aria-label="<%: GetLocalizedDayName(overloadedDay.Date) + " · " + string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), overloadedDay.Tasks.Count) %>">
                                        <span class="resource-attention-day-date">
                                            <strong><%: GetLocalizedDayName(overloadedDay.Date) %></strong>
                                            <small><%= overloadedDay.Date.ToString("dd/MM") %></small>
                                        </span>
                                        <span class="badge bg-danger-subtle text-danger"><%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), overloadedDay.Tasks.Count) %></span>
                                    </button>
                                    <% } %>
                                <% } else { %>
                                    <p class="small text-muted mb-0"><%= GetAttentionText(employee) %></p>
                                <% } %>
                            </div>
                        </details>
                        <% } %>
                        <% if (Model.AttentionEmployees.Count == 0) { %>
                        <div class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_WARNINGS) %></div>
                        <% } %>
                    </div>
                    <% if (Model.NoLoadEmployeeCount + Model.OverloadedEmployeeCount > Model.AttentionEmployees.Count) { %>
                    <a href="#resourceEmployeesModal" class="btn btn-link btn-sm align-self-start mt-2 resource-kpi-trigger"
                        data-bs-toggle="modal" data-resource-filter="attention"
                        data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_ALL) %></a>
                    <% } %>
                </div>
            </div>
        </div>
        <% } %>
    </div>

    <div class="row g-3 mb-3 align-items-stretch resource-main-grid">
        <div class="col-12 d-flex flex-column">
            <% if (!IsMonthlyView) { %>
    <div class="card border-0 shadow-sm w-100 h-100 resource-heatmap-card">
        <div class="card-body">
            <div class="mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_RESOURCE_HEATMAP) %></h5>
                    <p class="text-muted mb-0">
                        <%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_HEATMAP_DESC) %>
                    </p>
                </div>
                <div class="resource-legend d-flex flex-wrap gap-3 mt-2 small">
                    <span><i class="resource-legend-swatch resource-load-none"></i><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_FREE) %>: <%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), 0) %></span>
                    <span><i class="resource-legend-swatch resource-load-normal"></i><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_NORMAL) %>: <%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), 1) %></span>
                    <span><i class="resource-legend-swatch resource-load-over"></i><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %>: <%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), ">1") %></span>
                </div>
            </div>

            <div class="resource-heatmap-scroll">
                <table class="table dashboard-data-table table-bordered resource-heatmap-table align-middle mb-0">
                    <thead>
                        <tr class="resource-week-row">
                            <th class="resource-person-column"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></th>
                            <% foreach (var day in Model.Weeks.First().Days) { %>
                            <th class="text-center resource-day-column <%= day.IsToday ? "resource-today-column" : string.Empty %>">
                                <span><%= day.DayLabel %></span>
                                <small><%= day.Date.ToString("dd/MM") %></small>
                            </th>
                            <% } %>
                        </tr>
                    </thead>
                    <tbody id="resourceHeatmapListBody">
                        <% foreach (var employee in Model.EmployeeLoads) { %>
                        <tr data-search-row="true">
                            <td class="resource-person-column">
                                <div class="fw-semibold text-dark">
                                    <button type="button" class="resource-person-open resource-open-week"
                                        data-resource-person="<%= employee.EmployeeId %>"
                                        data-resource-week="<%= Model.AnchorWeekStart.ToString("yyyy-MM-dd") %>"
                                        aria-label="<%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_EMPLOYEE_ALLOCATION), employee.DisplayName, Model.Weeks.First(x => x.IsAnchorWeek).Label) %>"><%: employee.DisplayName %></button>
                                </div>
                                <div class="small text-muted text-truncate resource-person-meta"><%: GetEmployeeMeta(employee) %></div>
                            </td>
                            <% foreach (var dayInfo in Model.Weeks.First().Days) {
                                   var load = employee.DailyLoads.First(x => x.Date == dayInfo.Date); %>
                            <td class="resource-load-cell <%= dayInfo.IsToday ? "resource-today-column" : string.Empty %>">
                                <button
                                    type="button"
                                    class="resource-load-button resource-open-day <%= load.IsWeekend ? "resource-load-weekend" : GetDayLoadCss(load) %>"
                                    data-resource-person="<%= employee.EmployeeId %>"
                                    data-resource-day="<%= load.Date.ToString("yyyy-MM-dd") %>"
                                    aria-label="<%: employee.DisplayName + " · " + load.Date.ToString("dd/MM/yyyy") + " · " + GetDayTypeText(load) + (load.IsWeekend ? string.Empty : " · " + load.Tasks.Count + " " + GetTaskCountStatusText(load.Tasks.Count)) %>">
                                    <span class="resource-day-type <%= GetDayTypeCss(load) %>"><%: GetDayTypeText(load) %></span>
                                    <% if (!load.IsWeekend) { %>
                                    <span class="resource-day-status"><%: GetTaskCountStatusText(load.Tasks.Count) %></span>
                                    <span class="resource-day-task-count"><%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_TASK_COUNT), load.Tasks.Count) %></span>
                                    <% } %>
                                </button>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <tr data-search-empty="true" class="<%= Model.EmployeeLoads.Count == 0 ? string.Empty : "d-none" %>">
                            <td colspan="8" class="text-center text-muted py-5">
                                <%= Model.EmployeeLoads.Count == 0
                                    ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_MEMBERS)
                                    : GetResourceText(BackEndResourceKeys.NO_DATA) %>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

            <% } else { %>
            <div class="card border-0 shadow-sm w-100 h-100 flex-grow-1 resource-monthly-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_SUMMARY) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_MONTHLY_LOAD_DESC) %></p>
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
                            <tbody id="resourceMonthlyListBody">
                                <% foreach (var employee in Model.EmployeeLoads) { %>
                                <tr data-search-row="true">
                                    <td class="resource-person-column">
                                        <div class="fw-semibold text-dark">
                                            <button type="button" class="resource-person-open resource-open-month"
                                                data-resource-person="<%= employee.EmployeeId %>"
                                                data-resource-month="<%= Model.Months.First().StartDate.ToString("yyyy-MM-dd") %>"
                                                aria-label="<%: employee.DisplayName + " · " + Model.Months.First().Label %>"><%: employee.DisplayName %></button>
                                        </div>
                                        <div class="small text-muted text-truncate resource-person-meta"><%: GetEmployeeMeta(employee) %></div>
                                    </td>
                                    <% foreach (var month in Model.Months) {
                                           var load = employee.MonthlyLoads.First(x => x.MonthStart == month.StartDate); %>
                                    <td class="text-center resource-month-cell <%= GetMonthlySummaryCss(load) %>">
                                        <button type="button" class="resource-month-button resource-open-month"
                                            data-resource-person="<%= employee.EmployeeId %>"
                                            data-resource-month="<%= month.StartDate.ToString("yyyy-MM-dd") %>"
                                            title="<%: GetMonthlyStatusTitle(load) %>"
                                            aria-label="<%: employee.DisplayName + " · " + month.Label + " · " + GetMonthlyStatusText(load) + " · " + GetMonthlyStatusTitle(load) %>">
                                            <strong class="badge <%= GetMonthlyStatusBadgeCss(load) %>"><%= GetMonthlyStatusText(load) %></strong>
                                            <small class="resource-month-counts"><%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_MONTH_DAILY_COUNTS), load.NoLoadDayCount, load.NormalDayCount, load.OverloadedDayCount) %></small>
                                        </button>
                                    </td>
                                    <% } %>
                                </tr>
                                <% } %>
                                <tr data-search-empty="true" class="<%= Model.EmployeeLoads.Count == 0 ? string.Empty : "d-none" %>">
                                    <td colspan="<%= Model.Months.Count + 1 %>" class="text-center text-muted py-4">
                                        <%= Model.EmployeeLoads.Count == 0
                                            ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_MONTHLY_EMPLOYEES)
                                            : GetResourceText(BackEndResourceKeys.NO_DATA) %>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
            <% } %>
        </div>

    </div>

    <div class="modal fade" id="resourceEmployeesModal" tabindex="-1" aria-labelledby="resourceEmployeesModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="resourceEmployeesModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_IN_SCOPE) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                        <strong id="resourceEmployeeListCount" class="resource-list-count"></strong>
                        <div class="resource-employee-controls">
                            <label for="resourceEmployeeStatus" class="mb-0 text-nowrap"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                            <select id="resourceEmployeeStatus" class="form-select resource-employee-status">
                                <option value="all"><%= GetResourceText(BackEndResourceKeys.ALL) %></option>
                                <option value="free"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_FREE) %></option>
                                <option value="normal"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE_NORMAL) %></option>
                                <option value="overloaded"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %></option>
                                <option value="attention"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %></option>
                            </select>
                            <div class="input-group dashboard-list-search">
                                <span class="input-group-text"><i class="bx bx-search"></i></span>
                                <input id="resourceEmployeeSearch" type="search" class="form-control"
                                       placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                       aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                            </div>
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0 resource-employee-table">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TASK) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                                </tr>
                            </thead>
                            <tbody id="resourceEmployeeListBody">
                                <% foreach (var employee in Model.EmployeeLoads) { %>
                                <tr data-resource-status="<%= IsMonthlyView ? GetFocusMonthStatusKey(employee) : GetEmployeeStatusKey(employee) %>">
                                    <td>
                                        <div class="fw-semibold"><%: employee.DisplayName %></div>
                                        <div class="small text-muted"><%: GetEmployeeMeta(employee) %></div>
                                    </td>
                                    <% if (!IsMonthlyView) { %>
                                    <td><span class="badge <%= GetEmployeeStatusBadgeCss(employee) %>"><%= GetEmployeeStatusText(employee) %></span></td>
                                    <td><%= GetPeakTaskCountText(employee.PeakDailyTaskCount) %></td>
                                    <td>
                                        <button type="button" class="btn btn-sm btn-outline-primary text-nowrap resource-list-open-week"
                                            data-resource-person="<%= employee.EmployeeId %>"
                                            data-resource-week="<%= Model.AnchorWeekStart.ToString("yyyy-MM-dd") %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_WEEK) %></button>
                                    </td>
                                    <% } else {
                                           var focusLoad = GetFocusMonthLoad(employee); %>
                                    <td><span class="badge <%= GetMonthlyStatusBadgeCss(focusLoad) %>"><%= GetMonthlyStatusText(focusLoad) %></span></td>
                                    <td><%= GetPeakTaskCountText(focusLoad.PeakDailyTaskCount) %></td>
                                    <td><button type="button" class="btn btn-sm btn-outline-primary text-nowrap resource-list-open-month"
                                        data-resource-person="<%= employee.EmployeeId %>"
                                        data-resource-month="<%= Model.Months.First().StartDate.ToString("yyyy-MM-dd") %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_MONTH_WEEKS) %></button></td>
                                    <% } %>
                                </tr>
                                <% } %>
                                <tr id="resourceEmployeeSearchEmpty" class="<%= Model.EmployeeLoads.Count == 0 ? string.Empty : "d-none" %>">
                                    <td colspan="4" class="text-center text-muted py-4"><%= Model.EmployeeLoads.Count == 0
                                        ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_MEMBERS)
                                        : GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade dashboard-extra-modal" id="resource-detail-drawer" tabindex="-1" aria-labelledby="resource-detail-kicker" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="resource-detail-kicker" class="modal-title"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_ALLOCATION_DETAIL) %></h5>
                    <button id="resource-detail-close" type="button" class="btn-close resource-detail-close-button" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"><span aria-hidden="true">×</span></button>
                </div>
                <div class="modal-body resource-drawer-body">
                    <div class="resource-detail-person mb-3">
                        <h5 id="resource-detail-title" class="mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></h5>
                        <div id="resource-detail-subtitle" class="small text-muted"></div>
                    </div>
                    <div id="resource-detail-summary" class="resource-drawer-summary-inline"></div>
                    <div id="resource-detail-formula" class="resource-drawer-formula"></div>
                    <div id="resource-detail-days"></div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        window.dashboardResourceDetailData = <%= ResourceDetailData %>;
        window.dashboardResourceTexts = <%= DashboardTextsJson %>;
        window.dashboardResourceInitialDetail = <%= InitialDetailJson %>;
    </script>
</div>
