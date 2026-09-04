<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardOverview.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardOverview" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-overview">

    <!-- Tiêu đề & Ngày tháng -->
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERVIEW) %></h4>
            <div class="row g-2 mt-3 align-items-end">

    <div class="col-12 col-sm-6 col-md-5 col-xl-4">

        <label class="form-label mb-1 text-nowrap">
            <%= GetResourceText(BackEndResourceKeys.PROJECT_SCOPE) %>
        </label>

        <SweetSoft:ExtraDropdown
            ID="ddlProjectFilter"
            runat="server"
            CssClass="form-select"
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
                <%= GetResourceText(BackEndResourceKeys.TRACK_OVERALL_PROJECT_STATUS) %>
            </p>
        </div>
        
        <!-- Widget hiển thị ngày hiện tại -->
        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4" style="min-width: max-content;">
            <div class="bg-primary-subtle text-primary rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 48px; height: 48px;">
                <i class="bx bx-calendar fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase tracking-wide mb-1">
                    <%= DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("vi-VN")) %>
                </div>
                <div class="fw-bold fs-4 text-dark lh-1">
                    <%= DateTime.Now.ToString("dd/MM/yyyy") %>
                </div>
            </div>
        </div>
    </div>

    <!-- ========================= -->
    <!-- 5 KPI -->
    <!-- ========================= -->
    <div class="row row-cols-1 row-cols-md-2 row-cols-xl-5 g-3">

        <!-- Dự án đang triển khai -->
        <div class="col">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <div class="d-flex align-items-center">

                        <div class="flex-grow-1">
                            <p class="text-muted mb-1">
                                <% if (IsProjectView) { %>
                                    <%= GetResourceText(BackEndResourceKeys.PROJECT_STATUS) %>
                                <% } else { %>
                                    <%= GetResourceText(BackEndResourceKeys.ACTIVE_PROJECTS) %>
                                <% } %>
                            </p>

                            <h3 class="mb-0">
                                <% if (IsProjectView) { %>
                                    <%= SingleProjectStatusText %>
                                <% } else { %>
                                    <%= ActiveProjectCount %>
                                <% } %>
                            </h3>

                            <small class="text-muted">
                                <% if (!IsProjectView) { %>
                                    <%= GetResourceText(BackEndResourceKeys.PROJECTS_IN_PROGRESS) %>
                                <% } %>
                            </small>
                        </div>

                        <div class="avatar-sm">
                            <span class="avatar-title rounded-circle bg-primary-subtle text-primary">
                                <i class="bx bx-briefcase-alt-2 fs-4"></i>
                            </span>
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <!-- Lịch họp sắp tới -->
    <div class="col">
        <div class="card h-100 border-0 shadow-sm">
            <div class="card-body">
                <div class="d-flex align-items-center">

                    <div class="flex-grow-1">

                        <% if (IsProjectView) { %>
                        <p class="text-muted mb-1">Tiến độ thực tế</p>
                        <h3 class="mb-0 text-primary"><%= OverallProgress.ToString("0.##") %>%</h3>
                        <small class="text-muted">Trung bình toàn bộ công việc</small>
                        <% } else { %>
                        <p class="text-muted mb-1">
                            <%= GetResourceText(BackEndResourceKeys.UPCOMING_MEETINGS) %>
                        </p>
                        <h3 class="mb-0"><%= UpcomingMeetingCount %></h3>
                        <small class="text-muted">
                            <%= GetResourceText(BackEndResourceKeys.MEETINGS_THIS_WEEK) %>
                        </small>
                        <% } %>

                    </div>

                    <div class="avatar-sm">
                        <span class="avatar-title rounded-circle bg-info-subtle text-info">
                            <i class="bx <%= IsProjectView ? "bx-line-chart" : "bx-calendar-event" %> fs-4"></i>
                        </span>
                    </div>

                </div>
            </div>
        </div>
    </div>

        <!-- Dự án có nguy cơ trễ hạn -->
        <div class="col">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <div class="d-flex align-items-center">

                        <div class="flex-grow-1">
                            <% if (IsProjectView) { %>
                            <p class="text-muted mb-1">Tiến độ kế hoạch</p>
                            <h3 class="mb-0 text-warning"><%= SelectedProjectPlannedProgress.ToString("0.##") %>%</h3>
                            <small class="text-muted">Theo thời gian đã sử dụng</small>
                            <% } else { %>
                            <p class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.AT_RISK_PROJECTS) %>
                            </p>
                            <h3 class="mb-0"><%= AtRiskProjectRate.ToString("0.##") %>%</h3>
                            <small class="text-muted">
                                <%= GetResourceText(BackEndResourceKeys.RATE_ON_ACTIVE_PROJECTS) %>
                            </small>
                            <% } %>
                        </div>

                        <div class="avatar-sm">
                            <span class="avatar-title rounded-circle <%= IsProjectView ? "bg-warning-subtle text-warning" : "bg-info-subtle text-info" %>">
                                <i class="bx <%= IsProjectView ? "bx-time-five" : "bx-task" %> fs-4"></i>
                            </span>
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <!-- Công việc quá hạn -->
        <div class="col">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <div class="d-flex align-items-center">

                        <div class="flex-grow-1">
                            <% if (IsProjectView) { %>
                            <p class="text-muted mb-1">Độ lệch</p>
                            <h3 class="mb-0 <%= GetVarianceCss(SelectedProjectVariance) %>">
                                <%= GetVarianceText(SelectedProjectVariance) %>
                            </h3>
                            <small class="text-muted">Thực tế − Kế hoạch</small>
                            <% } else { %>
                            <p class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %>
                            </p>
                            <h3 class="mb-0"><%= OverdueTaskCount %></h3>
                            <small class="text-muted">
                                <%= GetResourceText(BackEndResourceKeys.INCOMPLETE_AND_OVERDUE) %>
                            </small>
                            <% } %>
                        </div>

                        <div class="avatar-sm">
                            <span class="avatar-title rounded-circle <%= IsProjectView ? "bg-primary-subtle text-primary" : "bg-danger-subtle text-danger" %>">
                                <i class="bx <%= IsProjectView ? "bx-git-compare" : "bx-error-circle" %> fs-4"></i>
                            </span>
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <!-- Ngân sách -->
        <div class="col">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <div class="d-flex align-items-center">

                        <div class="flex-grow-1">
                            <% if (IsProjectView) { %>
                            <p class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %>
                            </p>
                            <h3 class="mb-0 <%= OverdueTaskCount > 0 ? "text-danger" : "text-success" %>"><%= OverdueTaskCount %></h3>
                            <small class="text-muted"><%= SelectedProjectDueSoonTaskCount %> công việc sắp đến hạn</small>
                            <% } else { %>
                            <p class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.TOTAL_CONTRACT_VALUE) %>
                            </p>
                            <h3 class="mb-0"><%= TotalContractValue.ToString("#,##0") %></h3>
                            <small class="text-muted">
                                <%= GetResourceText(BackEndResourceKeys.IN_SELECTED_SCOPE) %>
                            </small>
                            <% } %>
                        </div>

                        <div class="avatar-sm">
                            <span class="avatar-title rounded-circle <%= IsProjectView ? "bg-danger-subtle text-danger" : "bg-warning-subtle text-warning" %>">
                                <i class="bx <%= IsProjectView ? "bx-error-circle" : "bx-money" %> fs-4"></i>
                            </span>
                        </div>

                    </div>
                </div>
            </div>
        </div>

    </div>
    <!-- END 5 KPI -->

<!-- ========================= -->
<!-- PHÂN BỐ + TIẾN ĐỘ + CẦN CHÚ Ý -->
<!-- ========================= -->

<div class="row mt-3 align-items-stretch">

<% if (!IsProjectView) { %>
    <!-- ========================= -->
    <!-- PHÂN BỐ TRẠNG THÁI -->
    <!-- ========================= -->

    <div class="col-12 col-xl-4 mb-3 d-flex">

        <div class="card dashboard-chart-card w-100 mt-0">

            <div class="card-body">

                <h5 class="card-title mb-1">
                    <%= GetResourceText(BackEndResourceKeys.PROJECT_STATUS_DISTRIBUTION) %>
                </h5>

                <p class="text-muted mb-3">
                    <%= GetResourceText(BackEndResourceKeys.PROJECT_COUNT_BY_STATUS) %>
                </p>

                <div id="project-status-chart"
                     style="width:100%; min-height:320px;">
                </div>

                <script type="text/javascript">
                    window.projectStatusChartData =
                        <%= ProjectStatusChartData %>;
                </script>

            </div>

        </div>

    </div>


    <!-- ========================= -->
    <!-- TIẾN ĐỘ DỰ ÁN -->
    <!-- ========================= -->

    <div class="col-12 col-xl-4 mb-3 d-flex">

        <div class="card dashboard-chart-card w-100 mt-0">

            <div class="card-body">

                <h5 class="card-title mb-1">
                    <%= GetResourceText(BackEndResourceKeys.PROJECT_PROGRESS) %>
                </h5>

                <p class="text-muted mb-3">
                    <%= GetResourceText(BackEndResourceKeys.PROGRESS_OF_EACH_PROJECT) %>
                </p>

                <div id="project-progress-chart-wrapper">

                    <div id="project-progress-chart"></div>

                </div>

                <script type="text/javascript">
                    window.projectProgressChartData =
                        <%= ProjectProgressChartData %>;
                </script>

            </div>

        </div>

    </div>


    <!-- ========================= -->
    <!-- DỰ ÁN CẦN CHÚ Ý -->
    <!-- ========================= -->

    <div class="col-12 col-xl-4 mb-3 d-flex">

        <div class="card dashboard-chart-card w-100 mt-0">

            <div class="card-body">

                <h5 class="card-title mb-1">
                    <%= GetResourceText(BackEndResourceKeys.PROJECTS_NEEDING_ATTENTION) %>
                </h5>

                <p class="text-muted mb-3">
                    <%= GetResourceText(BackEndResourceKeys.PROJECTS_WITH_MOST_RISKS_OR_ISSUES) %>
                </p>

                <div class="table-responsive">

                    <table class="table table-sm table-hover align-middle mb-0">

                        <thead>
                            <tr>
                                <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>

                                <th class="text-center">
                                    <%= GetResourceText(BackEndResourceKeys.RISK) %>
                                </th>

                                <th class="text-center">
                                    <%= GetResourceText(BackEndResourceKeys.ISSUE) %>
                                </th>
                            </tr>
                        </thead>

                        <tbody>

                            <% if (ProjectAttentionStatistics != null &&
                                   ProjectAttentionStatistics.Count > 0)
                            { %>

                                <% foreach (var item in ProjectAttentionStatistics)
                                { %>

                                    <tr>

                                        <td>
                                            <strong>
                                                <%: item.ProjectCode %>
                                            </strong>

                                            <div class="text-muted small">
                                                <%: item.ProjectName %>
                                            </div>
                                        </td>

                                        <td class="text-center">

                                            <% if (item.RiskCount > 0)
                                            { %>

                                                <span class="badge bg-warning-subtle text-warning">
                                                    <%= item.RiskCount %>
                                                </span>

                                            <% }
                                            else
                                            { %>

                                                <span class="text-muted">
                                                    0
                                                </span>

                                            <% } %>

                                        </td>

                                        <td class="text-center">

                                            <% if (item.IssueCount > 0)
                                            { %>

                                                <span class="badge bg-danger-subtle text-danger">
                                                    <%= item.IssueCount %>
                                                </span>

                                            <% }
                                            else
                                            { %>

                                                <span class="text-muted">
                                                    0
                                                </span>

                                            <% } %>

                                        </td>

                                    </tr>

                                <% } %>

                            <% }
                            else
                            { %>

                                <tr>
                                    <td colspan="3"
                                        class="text-center text-muted py-4">
                                        <%= GetResourceText(BackEndResourceKeys.NO_PROJECTS_WITH_RISKS_OR_ISSUES) %>
                                    </td>
                                </tr>

                            <% } %>

                        </tbody>

                    </table>

                </div>

            </div>

        </div>

    </div>
<% } else { %>
    <div class="col-12 col-xl-8 mb-3 d-flex">
        <div class="card border-0 shadow-sm w-100 mt-0 overview-project-summary-card">
            <div class="card-body">
                <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-start mb-3">
                    <div>
                        <h5 class="card-title mb-1">Tình hình dự án</h5>
                        <p class="text-muted mb-0">Thông tin và mức độ hoàn thành so với kế hoạch.</p>
                    </div>
                    <% if (!string.IsNullOrEmpty(SelectedProjectCode)) { %>
                    <div class="d-flex flex-wrap gap-2 mt-2 mt-md-0">
                        <span class="badge <%= GetProjectHealthBadgeCss(SelectedProjectHealth) %>">
                            <%= GetProjectHealthText(SelectedProjectHealth) %>
                        </span>
                        <span class="badge <%= GetProjectTimelineBadgeCss() %>">
                            <%= GetProjectTimelineText() %>
                        </span>
                    </div>
                    <% } %>
                </div>

                <% if (!string.IsNullOrEmpty(SelectedProjectCode)) { %>
                <div class="overview-project-identity p-3 rounded mb-3">
                    <div class="small text-muted mb-1"><%: SelectedProjectCode %></div>
                    <div class="fw-bold fs-5"><%: SelectedProjectName %></div>
                </div>

                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-4">
                        <div class="small text-muted mb-1">Ngày bắt đầu</div>
                        <div class="fw-semibold">
                            <%= SelectedProjectStartDate.HasValue ? SelectedProjectStartDate.Value.ToString("dd/MM/yyyy") : "-" %>
                        </div>
                    </div>
                    <div class="col-12 col-md-4">
                        <div class="small text-muted mb-1">Dự kiến hoàn thành</div>
                        <div class="fw-semibold">
                            <%= SelectedProjectExpectedEndDate.HasValue ? SelectedProjectExpectedEndDate.Value.ToString("dd/MM/yyyy") : "-" %>
                        </div>
                    </div>
                    <div class="col-12 col-md-4">
                        <div class="small text-muted mb-1">Hoàn thành thực tế</div>
                        <div class="fw-semibold">
                            <%= SelectedProjectActualCompletionDate.HasValue ? SelectedProjectActualCompletionDate.Value.ToString("dd/MM/yyyy") : "Chưa hoàn thành" %>
                        </div>
                    </div>
                </div>

                <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="fw-medium">Tiến độ thực tế</span>
                        <span class="fw-bold text-primary"><%= OverallProgress.ToString("0.##") %>%</span>
                    </div>
                    <div class="progress overview-progress-bar">
                        <div class="progress-bar <%= GetProgressBarCss(OverallProgress) %>"
                             role="progressbar"
                             style="width: <%= OverallProgress.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) %>%;"
                             aria-valuenow="<%= OverallProgress %>"
                             aria-valuemin="0"
                             aria-valuemax="100"></div>
                    </div>
                </div>

                <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="fw-medium">Tiến độ kế hoạch</span>
                        <span class="fw-bold text-warning"><%= SelectedProjectPlannedProgress.ToString("0.##") %>%</span>
                    </div>
                    <div class="progress overview-progress-bar">
                        <div class="progress-bar bg-warning"
                             role="progressbar"
                             style="width: <%= SelectedProjectPlannedProgress.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) %>%;"
                             aria-valuenow="<%= SelectedProjectPlannedProgress %>"
                             aria-valuemin="0"
                             aria-valuemax="100"></div>
                    </div>
                </div>

                <div class="overview-variance-box d-flex flex-column flex-sm-row align-items-sm-center justify-content-between p-3 rounded">
                    <div>
                        <div class="fw-semibold">Độ lệch tiến độ</div>
                        <div class="small text-muted">Tiến độ thực tế trừ tiến độ kế hoạch</div>
                    </div>
                    <div class="fs-4 fw-bold mt-2 mt-sm-0 <%= GetVarianceCss(SelectedProjectVariance) %>">
                        <%= GetVarianceText(SelectedProjectVariance) %>
                    </div>
                </div>
                <% } else { %>
                <div class="text-center text-muted py-5">Dự án không có dữ liệu trong khoảng thời gian đã chọn.</div>
                <% } %>
            </div>
        </div>
    </div>

    <div class="col-12 col-xl-4 mb-3 d-flex">
        <div class="card border-0 shadow-sm w-100 mt-0">
            <div class="card-body">
                <h5 class="card-title mb-1">
                    Cảnh báo tổng hợp
                </h5>
                <p class="text-muted mb-3">Các chỉ số cần theo dõi của dự án.</p>
                
                <div class="row g-3">
                    <div class="col-6">
                        <div class="overview-alert-card p-3 border rounded d-flex align-items-center justify-content-between <%= OverdueTaskCount > 0 ? "bg-danger-subtle border-danger" : "bg-light" %>">
                            <div>
                                <div class="small text-muted mb-1">Quá hạn</div>
                                <h3 class="mb-0 <%= OverdueTaskCount > 0 ? "text-danger" : "text-muted" %>"><%= OverdueTaskCount %></h3>
                            </div>
                            <i class="bx bx-error-circle fs-2 <%= OverdueTaskCount > 0 ? "text-danger" : "text-muted opacity-50" %>"></i>
                        </div>
                    </div>

                    <div class="col-6">
                        <div class="overview-alert-card p-3 border rounded d-flex align-items-center justify-content-between <%= SelectedProjectDueSoonTaskCount > 0 ? "bg-warning-subtle border-warning" : "bg-light" %>">
                            <div>
                                <div class="small text-muted mb-1">Sắp đến hạn</div>
                                <h3 class="mb-0 <%= SelectedProjectDueSoonTaskCount > 0 ? "text-warning" : "text-muted" %>"><%= SelectedProjectDueSoonTaskCount %></h3>
                            </div>
                            <i class="bx bx-time-five fs-2 <%= SelectedProjectDueSoonTaskCount > 0 ? "text-warning" : "text-muted opacity-50" %>"></i>
                        </div>
                    </div>

                    <div class="col-6">
                        <div class="overview-alert-card p-3 border rounded d-flex align-items-center justify-content-between <%= OpenRiskCount > 0 ? "bg-warning-subtle border-warning" : "bg-light" %>">
                            <div>
                                <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.RISK) %></div>
                                <h3 class="mb-0 <%= OpenRiskCount > 0 ? "text-warning" : "text-muted" %>"><%= OpenRiskCount %></h3>
                            </div>
                            <i class="bx bx-error fs-2 <%= OpenRiskCount > 0 ? "text-warning" : "text-muted opacity-50" %>"></i>
                        </div>
                    </div>

                    <div class="col-6">
                        <div class="overview-alert-card p-3 border rounded d-flex align-items-center justify-content-between <%= OpenIssueCount > 0 ? "bg-danger-subtle border-danger" : "bg-light" %>">
                            <div>
                                <div class="small text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.ISSUE) %></div>
                                <h3 class="mb-0 <%= OpenIssueCount > 0 ? "text-danger" : "text-muted" %>"><%= OpenIssueCount %></h3>
                            </div>
                            <i class="bx bx-bug fs-2 <%= OpenIssueCount > 0 ? "text-danger" : "text-muted opacity-50" %>"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
<% } %>

</div>
<!-- ========================= -->
<!-- NGUỒN LỰC + CHI PHÍ -->
<!-- ========================= -->

<div class="row mt-3 align-items-stretch">

    <!-- ========================= -->
    <!-- TỔNG QUAN NGUỒN LỰC -->
    <!-- ========================= -->

    <div class="col-12 col-xl-6 mb-3 d-flex">
    <div class="card h-100 w-100">

            <div class="card-body">

                <div class="d-flex align-items-center justify-content-between mb-3">

                    <div>
                        <h5 class="card-title mb-1">
                            <% if (IsProjectView) { %>
                                <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %>
                            <% } else { %>
                                <%= GetResourceText(BackEndResourceKeys.RESOURCE_OVERVIEW) %>
                            <% } %>
                        </h5>

                        <p class="text-muted mb-0">
                            <%= GetResourceText(BackEndResourceKeys.HR_AND_MEMBER_ALLOCATION) %>
                        </p>
                    </div>

                </div>


                <!-- 4 KPI -->
                <div class="row row-cols-1 row-cols-md-2 g-3">

                    <!-- Tổng nhân sự -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <% if (IsProjectView) { %>
                                    <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %>
                                <% } else { %>
                                    <%= GetResourceText(BackEndResourceKeys.TOTAL_EMPLOYEES) %>
                                <% } %>
                            </div>

                            <h4 class="mb-0">
                                <%= IsProjectView
                                    ? ResourceOverview.ParticipatingEmployeeCount
                                    : ResourceOverview.TotalEmployeeCount %>
                            </h4>

                        </div>

                    </div>


                    <!-- Đang tham gia -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <% if (IsProjectView) { %>
                                    <%= GetResourceText(BackEndResourceKeys.ASSIGNED_TO_TASKS) %>
                                <% } else { %>
                                    <%= GetResourceText(BackEndResourceKeys.PARTICIPATING) %>
                                <% } %>
                            </div>

                            <h4 class="mb-0">
                                <%= IsProjectView
                                    ? ResourceOverview.AssignedProjectMemberCount
                                    : ResourceOverview.ParticipatingEmployeeCount %>
                            </h4>

                        </div>

                    </div>


                    <!-- Chưa phân bổ -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <% if (IsProjectView) { %>
                                    <%= GetResourceText(BackEndResourceKeys.NOT_ASSIGNED_TO_TASKS) %>
                                <% } else { %>
                                    <%= GetResourceText(BackEndResourceKeys.UNASSIGNED) %>
                                <% } %>
                            </div>

                            <h4 class="mb-0">
                                <%= IsProjectView
                                    ? ResourceOverview.UnassignedProjectMemberCount
                                    : ResourceOverview.UnassignedEmployeeCount %>
                            </h4>

                        </div>

                    </div>


                    <!-- Tham gia nhiều dự án -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.MULTI_PROJECT_MEMBERS) %>
                            </div>

                            <h4 class="mb-0">
                                <%= ResourceOverview.MultiProjectEmployeeCount %>
                            </h4>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>


    <!-- ========================= -->
    <!-- TỔNG QUAN CHI PHÍ -->
    <!-- ========================= -->

    <div class="col-12 col-xl-6 mb-3 d-flex">
    <div class="card h-100 w-100">

            <div class="card-body">

                <div class="d-flex align-items-center justify-content-between mb-3">

                    <div>
                        <h5 class="card-title mb-1">
                            <%= GetResourceText(BackEndResourceKeys.COST_OVERVIEW) %>
                        </h5>

                        <p class="text-muted mb-0">
                            <%= GetResourceText(BackEndResourceKeys.BUDGET_AND_COST_SUMMARY) %>
                        </p>
                    </div>

                </div>


                <!-- 4 KPI -->
                <div class="row row-cols-1 row-cols-md-2 g-3">

                    <!-- Ngân sách -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.TOTAL_CONTRACT_VALUE) %>
                            </div>

                            <h4 class="mb-0">
                                <%= CostOverview.TotalContractValue.ToString("#,##0") %>
                            </h4>

                        </div>

                    </div>


                    <!-- Thanh toán thực tế -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.RECEIVED_PAYMENT) %>
                            </div>

                            <h4 class="mb-0 text-success">
                                <%= CostOverview.ReceivedPayment.ToString("#,##0") %>
                            </h4>

                        </div>

                    </div>

                    <!-- Giá trị còn lại sau chi phí -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.REMAINING_AFTER_COST) %>
                            </div>

                            <h4 class="mb-0 <%= CostOverview.RemainingAfterCost < 0 ? "text-danger" : "text-primary" %>">
                                <%= CostOverview.RemainingAfterCost.ToString("#,##0") %>
                            </h4>

                        </div>

                    </div>


                    <!-- Chi phí thực tế -->
                    <div class="col">

                        <div class="border rounded p-3 h-100">

                            <div class="text-muted mb-1">
                                <%= GetResourceText(BackEndResourceKeys.ACTUAL_COST) %>
                            </div>

                            <h4 class="mb-0">
                                <%= CostOverview.ActualCost.ToString("#,##0") %>
                            </h4>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>

</div>
