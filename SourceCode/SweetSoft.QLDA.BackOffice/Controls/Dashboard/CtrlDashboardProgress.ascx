<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardProgress.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardProgress" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-progress">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1">Dashboard tiến độ dự án và công việc</h4>

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
                Tiến độ dự án được so sánh với tỷ lệ thời gian kế hoạch đã sử dụng;
                khoảng thời gian dùng để xác định các dự án thuộc phạm vi thống kê,
                còn số liệu công việc lấy toàn bộ công việc của các dự án đó.
            </p>
        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 progress-date-card">
            <div class="bg-primary-subtle text-primary rounded-circle d-flex align-items-center justify-content-center me-3 progress-date-icon">
                <i class="bx bx-line-chart fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase mb-1">Cập nhật dữ liệu</div>
                <div class="fw-bold text-dark lh-1"><%= Model.GeneratedAt.ToString("HH:mm dd/MM/yyyy") %></div>
            </div>
        </div>
    </div>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-6 g-3 mb-4">
        <div class="col">
            <div class="card h-100 border-0 shadow-sm progress-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1">Tiến độ trung bình</p>
                        <h3 class="mb-0 text-primary"><%= Model.OverallProgress.ToString("0.##") %>%</h3>
                        <small class="text-muted"><%= Model.TotalProjectCount %> dự án</small>
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
                        <p class="text-muted mb-1">Tổng công việc</p>
                        <h3 class="mb-0"><%= Model.TotalTaskCount %></h3>
                        <small class="text-muted">Của các dự án được lọc</small>
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
                        <p class="text-muted mb-1">Đã hoàn thành</p>
                        <h3 class="mb-0 text-success"><%= Model.CompletedTaskCount %></h3>
                        <small class="text-muted">Đạt 100%</small>
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
                        <p class="text-muted mb-1">Đang thực hiện</p>
                        <h3 class="mb-0 text-info"><%= Model.InProgressTaskCount %></h3>
                        <small class="text-muted">Từ 1% đến 99%</small>
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
                        <p class="text-muted mb-1">Quá hạn</p>
                        <h3 class="mb-0 text-danger"><%= Model.OverdueTaskCount %></h3>
                        <small class="text-muted"><%= Model.DueSoonTaskCount %> việc sắp đến hạn</small>
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
                        <p class="text-muted mb-1">Độ lệch</p>
                        <h3 class="mb-0 <%= GetVarianceCss(GetSelectedProjectVariance()) %>">
                            <%= GetSelectedProjectVarianceText() %>
                        </h3>
                        <small class="text-muted">Thực tế − Kế hoạch</small>
                        <% } else { %>
                        <p class="text-muted mb-1">Dự án cần chú ý</p>
                        <h3 class="mb-0 text-warning"><%= Model.NeedsAttentionProjectCount %></h3>
                        <small class="text-muted">Chậm, rủi ro hoặc quá hạn</small>
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

    <div class="row g-3 mb-3">
        <div class="col-12 col-xl-8 d-flex">
            <div class="card w-100 border-0 shadow-sm overview-project-summary-card">
                <div class="card-body">
                    <% if (Model.IsSingleProject && Model.ProjectScheduleStatistics.Count > 0) {
                           var selectedProject = Model.ProjectScheduleStatistics[0]; %>
                    <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-start mb-3">
                        <div>
                            <h5 class="card-title mb-1">Thực tế so với kế hoạch</h5>
                            <p class="text-muted mb-0">
                                Tiến độ thực hiện của dự án so với tỷ lệ thời gian kế hoạch đã sử dụng.
                            </p>
                        </div>
                        <span class="badge <%= GetHealthBadgeCss(selectedProject.Health) %> mt-2 mt-md-0">
                            <%= GetHealthText(selectedProject.Health) %>
                        </span>
                    </div>

                    <div class="overview-project-identity p-3 rounded mb-3">
                        <div class="small text-muted mb-1"><%: selectedProject.ProjectCode %></div>
                        <div class="fw-bold fs-5"><%: selectedProject.ProjectName %></div>
                    </div>

                    <div class="row g-3 mb-4">
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1">Ngày bắt đầu</div>
                            <div class="fw-semibold"><%= selectedProject.StartDate.ToString("dd/MM/yyyy") %></div>
                        </div>
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1">Dự kiến hoàn thành</div>
                            <div class="fw-semibold"><%= selectedProject.ExpectedEndDate.ToString("dd/MM/yyyy") %></div>
                        </div>
                        <div class="col-12 col-md-4">
                            <div class="small text-muted mb-1">Hoàn thành thực tế</div>
                            <div class="fw-semibold">
                                <%= selectedProject.ActualCompletionDate.HasValue
                                    ? selectedProject.ActualCompletionDate.Value.ToString("dd/MM/yyyy")
                                    : "Chưa hoàn thành" %>
                            </div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <span class="fw-medium">Tiến độ thực tế</span>
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
                            <span class="fw-medium">Tiến độ kế hoạch</span>
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
                            <div class="fw-semibold">Độ lệch tiến độ</div>
                            <div class="small text-muted">Tiến độ thực tế trừ tiến độ kế hoạch</div>
                        </div>
                        <div class="fs-4 mt-2 mt-sm-0 <%= GetVarianceCss(selectedProject.Variance) %>">
                            <%= GetVarianceText(selectedProject.Variance) %>%
                        </div>
                    </div>
                    <% } else { %>
                    <h5 class="card-title mb-1">Thực tế so với kế hoạch</h5>
                    <p class="text-muted mb-3">
                        Kế hoạch được tính theo tỷ lệ thời gian từ ngày bắt đầu đến ngày dự kiến hoàn thành.
                    </p>
                    <div id="progress-schedule-chart-wrapper" class="progress-chart-scroll">
                        <div id="progress-schedule-chart"></div>
                    </div>
                    <% } %>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card w-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1">Trạng thái công việc</h5>
                    <p class="text-muted mb-3">Phân bố toàn bộ công việc của các dự án thuộc phạm vi lọc.</p>
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
                    <h5 class="card-title mb-1">Tiến độ từng công việc</h5>
                    <p class="text-muted mb-0">
                        Công việc chưa hoàn thành được xếp theo ưu tiên từ cao xuống thấp; công việc đã hoàn thành nằm cuối danh sách.
                    </p>
                </div>
                <span class="badge bg-primary-subtle text-primary align-self-start mt-2 mt-md-0">
                    <%= Model.TaskProgressDetails.Count %> công việc
                </span>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Công việc</th>
                            <th>Dự án</th>
                            <th>Ưu tiên</th>
                            <th>Hạn hoàn thành</th>
                            <th style="min-width: 190px;">Tiến độ</th>
                            <th class="text-center">Trạng thái</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var task in Model.TaskProgressDetails) { %>
                        <tr>
                            <td><div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div></td>
                            <td>
                                <div><%: task.ProjectCode %></div>
                                <div class="small text-muted"><%: task.ProjectName %></div>
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
                            <td colspan="6" class="text-center text-muted py-4">Dự án chưa có công việc.</td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
            <% } else { %>
            <h5 class="card-title mb-1">Cơ cấu công việc theo dự án</h5>
            <p class="text-muted mb-3">
                So sánh số công việc hoàn thành, đang thực hiện, chưa bắt đầu và quá hạn giữa các dự án.
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
                    <h5 class="card-title mb-1">Sức khỏe tiến độ dự án</h5>
                    <p class="text-muted mb-0">Dự án có độ lệch âm lớn đang chậm hơn kế hoạch.</p>
                </div>
                <div class="small text-muted mt-2 mt-md-0">Độ lệch = Thực tế − Kế hoạch</div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Dự án</th>
                            <th>Thời gian kế hoạch</th>
                            <th style="min-width: 190px;">Tiến độ thực tế</th>
                            <th class="text-center">Kế hoạch</th>
                            <th class="text-center">Độ lệch</th>
                            <th class="text-center">Công việc</th>
                            <th class="text-center">Quá hạn</th>
                            <th class="text-center">Đánh giá</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectScheduleStatistics) { %>
                        <tr>
                            <td>
                                <div class="fw-semibold"><%: project.ProjectCode %></div>
                                <div class="small text-muted"><%: project.ProjectName %></div>
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
                                <span class="<%= GetVarianceCss(project.Variance) %>">
                                    <%= project.Variance > 0 ? "+" : string.Empty %><%= project.Variance.ToString("0.##") %>%
                                </span>
                            </td>
                            <td class="text-center"><%= project.CompletedTaskCount %>/<%= project.TotalTaskCount %></td>
                            <td class="text-center">
                                <span class="<%= project.OverdueTaskCount > 0 ? "text-danger fw-semibold" : "text-muted" %>">
                                    <%= project.OverdueTaskCount %>
                                </span>
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
                            <td colspan="8" class="text-center text-muted py-4">Không có dự án phù hợp với bộ lọc.</td>
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
                    <h5 class="card-title mb-1">Công việc cần ưu tiên xử lý</h5>
                    <p class="text-muted mb-0">Công việc quá hạn hoặc sẽ đến hạn trong 7 ngày tới.</p>
                </div>
                <span class="badge bg-danger-subtle text-danger align-self-start mt-2 mt-md-0">
                    <%= Model.AttentionTasks.Count %> công việc
                </span>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Công việc</th>
                            <th>Dự án</th>
                            <th>Ưu tiên</th>
                            <th>Hạn hoàn thành</th>
                            <th style="min-width: 190px;">Tiến độ</th>
                            <th class="text-center">Cảnh báo</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var task in Model.AttentionTasks) { %>
                        <tr>
                            <td><div class="fw-semibold"><%: task.TaskCode %> - <%: task.TaskName %></div></td>
                            <td>
                                <div><%: task.ProjectCode %></div>
                                <div class="small text-muted"><%: task.ProjectName %></div>
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
                            <td colspan="6" class="text-center text-muted py-4">Không có công việc quá hạn hoặc sắp đến hạn.</td>
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
        window.dashboardProgressProjectTaskData = <%= ProjectTaskChartData %>;
        window.dashboardProgressIsSingleProject = <%= Model.IsSingleProject.ToString().ToLowerInvariant() %>;
    </script>
</div>
