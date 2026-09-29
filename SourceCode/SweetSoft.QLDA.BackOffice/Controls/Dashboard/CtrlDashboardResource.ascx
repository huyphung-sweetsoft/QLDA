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

    <%-- TẠM THỜI: Chú thích cách hiểu số liệu để nhóm kiểm tra; xóa cả khối này khi đã chốt nội dung. --%>
    <details class="alert alert-light border mb-3" open>
        <summary class="fw-semibold" style="cursor: pointer">Chú thích số liệu (tạm thời)</summary>
        <ul class="small mb-0 mt-2 ps-3">
            <li><strong>Tổng nhân sự:</strong> khi xem tất cả dự án là nhân sự công ty; khi xem một dự án là người tham gia, được giao việc hoặc quản lý dự án đó. Một người chỉ đếm một lần.</li>
            <li><strong>% tải dự kiến:</strong> số ngày công được giao chia cho số ngày làm việc theo lịch. Một công việc kéo dài qua nhiều ngày được tính ở từng ngày; hai công việc cùng ngày tính hai ngày công. Đây không phải giờ làm thực tế.</li>
            <li><strong>Không tải / Thiếu tải / Đủ tải / Quá tải:</strong> lần lượt là 0%, trên 0% đến dưới 80%, từ 80% đến 100%, và trên 100%. Mỗi người chỉ thuộc một nhóm trong kỳ đang xét.</li>
            <li><strong>Khi xem theo tháng:</strong> KPI phân nhóm theo mức tải trung bình của tháng ghi ở tiêu đề; ô tháng khác có thể cảnh báo một tuần quá tải dù trung bình tháng không quá tải.</li>
            <li><strong>Nhân sự cần chú ý:</strong> người không tải, thiếu tải hoặc quá tải trong tuần trọng tâm; bấm vào ô tải để xem các công việc và ngày làm việc tạo ra tỷ lệ đó.</li>
        </ul>
    </details>

    <div class="resource-summary-header d-flex flex-wrap align-items-center justify-content-between gap-2 mb-2">
        <h5 class="mb-0"><%= IsMonthlyView
            ? string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_MONTH_KPI_TITLE), Model.Months.First().StartDate.ToString("MM/yyyy"))
            : GetResourceText(BackEndResourceKeys.DASHBOARD_UTILIZATION) + " · " + Model.AnchorWeekStart.ToString("dd/MM") + "–" + Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %>
        </h5>
        <button type="button" class="btn btn-link btn-sm resource-help-toggle" data-bs-toggle="collapse" data-bs-target="#resourceLoadHelp" aria-expanded="false" aria-controls="resourceLoadHelp">
            <i class="bx bx-info-circle" aria-hidden="true"></i> <%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEKLY_CALCULATION_TITLE) %>
        </button>
    </div>
    <div id="resourceLoadHelp" class="collapse mb-3"><div class="resource-method-note rounded p-2 small">
        <%= GetResourceText(IsMonthlyView
            ? BackEndResourceKeys.DASHBOARD_MONTHLY_CALCULATION_DESC
            : BackEndResourceKeys.DASHBOARD_WEEKLY_CALCULATION_DESC) %>
    </div></div>
    <div class="row g-3 mb-3 resource-kpi-grid">
        <div class="col-12 col-sm-6 col-md-4 col-xl">
            <a href="#resourceEmployeesModal" class="resource-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-resource-filter="all"
               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_EMPLOYEES) %>">
                <div class="card h-100 border-0 shadow-sm resource-kpi-card resource-kpi-total"><div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_TOTAL_EMPLOYEES) %></div>
                    <div class="d-flex align-items-end justify-content-between"><h3 class="mb-0"><%= Model.TotalEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-group"></i></span></div>
                </div></div>
            </a>
        </div>
        <div class="col-12 col-sm-6 col-md-4 col-xl">
            <a href="#resourceEmployeesModal" class="resource-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-resource-filter="noload"
               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD) %>">
                <div class="card h-100 border-0 shadow-sm resource-kpi-card resource-kpi-none"><div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD) %></div>
                    <div class="d-flex align-items-end justify-content-between"><h3 class="mb-0"><%= IsMonthlyView ? GetFocusMonthCount("noload") : Model.NoLoadEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-secondary-subtle text-secondary"><i class="bx bx-user-x"></i></span></div>
                </div></div>
            </a>
        </div>
        <div class="col-12 col-sm-6 col-md-4 col-xl">
            <a href="#resourceEmployeesModal" class="resource-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-resource-filter="underloaded"
               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_UNDERLOADED) %>">
                <div class="card h-100 border-0 shadow-sm resource-kpi-card resource-kpi-low"><div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UNDERLOADED) %> <small>(&lt;80%)</small></div>
                    <div class="d-flex align-items-end justify-content-between"><h3 class="mb-0"><%= IsMonthlyView ? GetFocusMonthCount("underloaded") : Model.UnderloadedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-success-subtle text-success"><i class="bx bx-down-arrow-alt"></i></span></div>
                </div></div>
            </a>
        </div>
        <div class="col-12 col-sm-6 col-md-4 col-xl">
            <a href="#resourceEmployeesModal" class="resource-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-resource-filter="balanced"
               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_BALANCED_LOAD) %>">
                <div class="card h-100 border-0 shadow-sm resource-kpi-card resource-kpi-balanced"><div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_BALANCED_LOAD) %> <small>(80–100%)</small></div>
                    <div class="d-flex align-items-end justify-content-between"><h3 class="mb-0"><%= IsMonthlyView ? GetFocusMonthCount("balanced") : Model.BalancedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-warning-subtle text-warning"><i class="bx bx-check-shield"></i></span></div>
                </div></div>
            </a>
        </div>
        <div class="col-12 col-sm-6 col-md-4 col-xl">
            <a href="#resourceEmployeesModal" class="resource-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-resource-filter="overloaded"
               data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %>">
                <div class="card h-100 border-0 shadow-sm resource-kpi-card resource-kpi-over"><div class="card-body">
                    <div class="resource-kpi-label"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERLOADED) %> <small>(&gt;100%)</small></div>
                    <div class="d-flex align-items-end justify-content-between"><h3 class="mb-0"><%= IsMonthlyView ? GetFocusMonthCount("overloaded") : Model.OverloadedEmployeeCount %></h3>
                        <span class="resource-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-error-circle"></i></span></div>
                </div></div>
            </a>
        </div>
    </div>

    <div class="row g-3 mb-3 align-items-stretch resource-main-grid">
        <div class="<%= IsMonthlyView ? "col-12" : "col-12 col-xl-8" %> d-flex flex-column">
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
                    <span><i class="resource-legend-swatch resource-load-none"></i><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD) %></span>
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
                            <% foreach (var week in Model.Weeks) {
                                   var load = employee.WeeklyLoads.First(x => x.WeekStart == week.StartDate); %>
                            <td class="resource-load-cell <%= week.IsAnchorWeek ? "resource-anchor-week-cell" : string.Empty %>">
                                <button
                                    type="button"
                                    class="resource-load-button resource-open-week <%= GetHeatmapCss(load.AllocationPercent) %>"
                                    data-resource-person="<%= employee.EmployeeId %>"
                                    data-resource-week="<%= week.StartDate.ToString("yyyy-MM-dd") %>"
                                    aria-label="<%: string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_EMPLOYEE_ALLOCATION), employee.DisplayName, week.Label) %>">
                                    <span class="resource-load-percent"><%= GetCellText(load.AllocationPercent) %></span>
                                    <small><%= GetWeekLoadText(load.AllocationPercent) %></small>
                                </button>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <tr data-search-empty="true" class="<%= Model.EmployeeLoads.Count == 0 ? string.Empty : "d-none" %>">
                            <td colspan="<%= Model.Weeks.Count + 1 %>" class="text-center text-muted py-5">
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
                                            aria-label="<%: employee.DisplayName + " · " + month.Label + ": " + load.AverageUtilization.ToString("0.#") + "% · " + GetMonthlyStatusText(load) %>">
                                            <strong class="resource-month-percent"><%= load.AverageUtilization.ToString("0.#") %>%</strong>
                                            <span class="badge <%= GetMonthlyStatusBadgeCss(load) %>"><%= GetMonthlyStatusText(load) %></span>
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

        <% if (!IsMonthlyView) { %>
        <div class="col-12 col-xl-4 d-flex flex-column">
            <div class="card border-0 shadow-sm w-100 h-100 flex-grow-1 resource-attention-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <div class="d-flex justify-content-between align-items-start mb-3 flex-shrink-0">
                        <div>
                            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %></h5>
                            <p class="text-muted mb-0"><%= Model.AnchorWeekStart.ToString("dd/MM") %>–<%= Model.AnchorWeekEnd.ToString("dd/MM/yyyy") %></p>
                        </div>
                        <span class="badge bg-warning-subtle text-warning"><%= Model.NoLoadEmployeeCount + Model.UnderloadedEmployeeCount + Model.OverloadedEmployeeCount %></span>
                    </div>
                    <div class="resource-attention-list flex-grow-1">
                        <% foreach (var employee in Model.AttentionEmployees) { %>
                        <button type="button" class="resource-attention-item resource-open-week d-flex align-items-start"
                            data-resource-person="<%= employee.EmployeeId %>"
                            data-resource-week="<%= Model.AnchorWeekStart.ToString("yyyy-MM-dd") %>">
                            <span class="resource-attention-dot <%= GetEmployeeStatusLoadCss(employee) %>"></span>
                            <div class="flex-grow-1 min-width-0">
                                <div class="d-flex justify-content-between gap-2">
                                    <strong class="text-truncate"><%: employee.DisplayName %></strong>
                                    <span class="badge <%= GetEmployeeStatusBadgeCss(employee) %>"><%= GetEmployeeStatusText(employee) %></span>
                                </div>
                                <div class="small text-muted mt-1"><%: GetAttentionText(employee) %></div>
                            </div>
                        </button>
                        <% } %>
                        <% if (Model.AttentionEmployees.Count == 0) { %>
                        <div class="text-center text-muted py-5"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_RESOURCE_WARNINGS) %></div>
                        <% } %>
                    </div>
                    <% if (Model.NoLoadEmployeeCount + Model.UnderloadedEmployeeCount + Model.OverloadedEmployeeCount > Model.AttentionEmployees.Count) { %>
                    <a href="#resourceEmployeesModal" class="btn btn-link btn-sm align-self-start mt-2 resource-kpi-trigger"
                        data-bs-toggle="modal" data-resource-filter="attention"
                        data-resource-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEES_NEED_ATTENTION) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_ALL) %></a>
                    <% } %>
                </div>
            </div>
        </div>
        <% } %>
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
                        <div class="input-group dashboard-list-search">
                            <span class="input-group-text"><i class="bx bx-search"></i></span>
                            <input id="resourceEmployeeSearch" type="search" class="form-control"
                                   placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0 resource-employee-table">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UTILIZATION) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALLOCATED_DAYS) %></th>
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
                                    <td><strong><%= employee.AverageUtilization.ToString("0.#") %>%</strong>
                                        <span class="badge ms-1 <%= GetEmployeeStatusBadgeCss(employee) %>"><%= GetEmployeeStatusText(employee) %></span></td>
                                    <td><%= employee.AllocatedDays.ToString("0.#") %>/<%= employee.CapacityDays.ToString("0") %> <%= GetResourceText(BackEndResourceKeys.DASHBOARD_DAY_UNIT) %></td>
                                    <td>
                                        <button type="button" class="btn btn-sm btn-outline-primary text-nowrap resource-list-open-week"
                                            data-resource-person="<%= employee.EmployeeId %>"
                                            data-resource-week="<%= Model.AnchorWeekStart.ToString("yyyy-MM-dd") %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_VIEW_WEEK) %></button>
                                    </td>
                                    <% } else {
                                           var focusLoad = GetFocusMonthLoad(employee); %>
                                    <td><strong><%= focusLoad.AverageUtilization.ToString("0.#") %>%</strong>
                                        <span class="badge ms-1 <%= focusLoad.AllocatedDays <= 0 ? "bg-secondary-subtle text-secondary" : GetStatusBadgeCss(focusLoad.Status) %>"><%= focusLoad.AllocatedDays <= 0 ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_LOAD) : GetStatusText(focusLoad.Status) %></span></td>
                                    <td><%= focusLoad.AllocatedDays.ToString("0.#") %>/<%= focusLoad.CapacityDays.ToString("0") %> <%= GetResourceText(BackEndResourceKeys.DASHBOARD_DAY_UNIT) %></td>
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

    <div id="resource-detail-backdrop" class="resource-detail-backdrop" hidden></div>
    <aside id="resource-detail-drawer" class="resource-detail-drawer" role="dialog" aria-modal="true" aria-hidden="true" aria-labelledby="resource-detail-title">
        <div class="resource-drawer-header d-flex align-items-start justify-content-between">
            <div>
                <div id="resource-detail-kicker" class="small text-muted text-uppercase"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_WEEK_ALLOCATION_DETAIL) %></div>
                <h5 id="resource-detail-title" class="mb-1 mt-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EMPLOYEE_LABEL) %></h5>
                <div id="resource-detail-subtitle" class="small text-muted"></div>
            </div>
            <button id="resource-detail-close" type="button" class="btn btn-sm btn-light" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"><i class="bx bx-x fs-4"></i></button>
        </div>
        <div class="resource-drawer-body">
            <div id="resource-detail-summary" class="resource-drawer-summary-inline"></div>
            <div id="resource-detail-formula" class="resource-drawer-formula"></div>
            <div id="resource-detail-days"></div>
        </div>
    </aside>

    <script type="text/javascript">
        window.dashboardResourceDetailData = <%= ResourceDetailData %>;
        window.dashboardResourceTexts = <%= DashboardTextsJson %>;
        window.dashboardResourceInitialDetail = <%= InitialDetailJson %>;
    </script>
</div>
