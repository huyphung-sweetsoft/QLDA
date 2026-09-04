<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardResource.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardResource" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-resource">
    <div class="d-flex flex-column flex-xl-row align-items-xl-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1">Dashboard phân bổ nguồn lực</h4>
            <p class="text-muted mb-0">
                Tổng hợp ngày công từ thứ Hai đến thứ Sáu, phát hiện nhân sự chưa đủ tải hoặc được giao vượt quá năng lực của tuần.
            </p>

            <div class="row g-2 mt-2 align-items-end">
                <div class="col-12 col-md-5 col-xl-4">
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

                <div class="col-7 col-md-3 col-xl-2">
                    <label class="form-label mb-1 text-nowrap">Khoảng hiển thị</label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlWeekCount"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-5 col-md-auto">
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

        <div class="resource-week-navigator bg-white border rounded shadow-sm mt-3 mt-xl-0 ms-xl-4">
            <div class="small text-muted text-uppercase fw-medium mb-2">Tuần trọng tâm</div>
            <div class="d-flex align-items-center justify-content-between gap-2">
                <asp:LinkButton
                    ID="btnPreviousWeek"
                    runat="server"
                    CssClass="btn btn-outline-secondary btn-sm resource-week-button"
                    ToolTip="Tuần trước"
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
                        OnClick="btnCurrentWeek_Click">Về tuần hiện tại</asp:LinkButton>
                </div>
                <asp:LinkButton
                    ID="btnNextWeek"
                    runat="server"
                    CssClass="btn btn-outline-secondary btn-sm resource-week-button"
                    ToolTip="Tuần sau"
                    OnClick="btnNextWeek_Click">
                    <i class="bx bx-chevron-right"></i>
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <div class="alert alert-info resource-method-note d-flex align-items-start mb-3" role="alert">
        <i class="bx bx-info-circle fs-4 me-2"></i>
        <div>
            <strong>Cách tính theo tuần:</strong> mỗi ngày một công việc còn hiệu lực tương đương một ngày công;
            mức tải tuần bằng tổng ngày công được giao chia cho số ngày làm việc thực tế theo cấu hình tuần và lịch ngoại lệ trong database.
            Ví dụ tuần có 5 ngày làm việc, nhân sự được giao 5 ngày ở dự án A và 2 ngày ở dự án B thì mức tải là 7/5 ngày, tương đương 140%.
        </div>
    </div>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xxl-5 g-3 mb-3">
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label">Nhân sự trong phạm vi</div>
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
                    <div class="resource-kpi-label">Đã được phân bổ</div>
                    <div class="d-flex align-items-end justify-content-between">
                        <div><h3 class="mb-0 text-primary"><%= Model.AssignedEmployeeCount %></h3><small class="text-muted">Sử dụng chung <%= Model.AverageUtilization.ToString("0.#") %>%</small></div>
                        <span class="resource-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-user-check"></i></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm resource-kpi-card">
                <div class="card-body">
                    <div class="resource-kpi-label">Thiếu tải (&lt;80%)</div>
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
                    <div class="resource-kpi-label">Tải cân bằng (80–100%)</div>
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
                    <div class="resource-kpi-label">Quá tải (&gt;100%)</div>
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
                    <h5 class="card-title mb-1">Heatmap phân bổ nguồn lực theo tuần</h5>
                    <p class="text-muted mb-0">
                        Nhấn vào từng ô để xem dự án, công việc và các ngày tạo nên mức tải; bảng bên dưới tổng hợp riêng từng tháng xuất hiện trong phạm vi.
                    </p>
                </div>
                <div class="resource-legend d-flex flex-wrap gap-3 mt-3 mt-lg-0 small">
                    <span><i class="resource-legend-swatch resource-load-low"></i>&lt;80% Thiếu tải</span>
                    <span><i class="resource-legend-swatch resource-load-balanced"></i>80–100% Tải cân bằng</span>
                    <span><i class="resource-legend-swatch resource-load-over"></i>&gt;100% Quá tải</span>
                </div>
            </div>

            <div class="resource-heatmap-scroll">
                <table class="table resource-heatmap-table align-middle mb-0">
                    <thead>
                        <tr class="resource-week-row">
                            <th class="resource-person-column">Nhân sự</th>
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
                                <div class="fw-semibold text-dark"><%: employee.DisplayName %></div>
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
                                    aria-label="Xem phân bổ của <%: employee.DisplayName %> trong <%= week.Label %>">
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
                                Dự án chưa có thành viên, quản lý hoặc nhân sự được giao công việc.
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12 col-xl-8 d-flex">
            <div class="card border-0 shadow-sm w-100 resource-monthly-card">
                <div class="card-body">
            <h5 class="card-title mb-1">Tổng hợp mức tải theo tháng</h5>
            <p class="text-muted mb-3">
                Mỗi tháng được tính theo toàn bộ ngày làm việc của tháng. Tuần giao tháng được cảnh báo tại tháng chứa phần lớn ngày làm việc của tuần đó.
            </p>
            <div class="resource-monthly-scroll">
                <table class="table resource-monthly-table align-middle mb-0">
                    <thead>
                        <tr>
                            <th class="resource-person-column">Nhân sự</th>
                            <% foreach (var month in Model.Months) { %>
                            <th class="text-center resource-month-column"><%= month.Label %></th>
                            <% } %>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var employee in Model.EmployeeLoads) { %>
                        <tr>
                            <td class="resource-person-column">
                                <div class="fw-semibold text-dark"><%: employee.DisplayName %></div>
                                <div class="small text-muted text-truncate resource-person-meta"><%: GetEmployeeMeta(employee) %></div>
                            </td>
                            <% foreach (var month in Model.Months) {
                                   var load = employee.MonthlyLoads.First(x => x.MonthStart == month.StartDate); %>
                            <td class="text-center resource-month-cell <%= GetMonthlySummaryCss(load) %>"
                                title="<%: GetMonthlyStatusTitle(load) %>">
                                <div class="fw-semibold resource-month-percent"><%= load.AverageUtilization.ToString("0.#") %>%</div>
                                <small class="text-muted"><%= load.AllocatedDays.ToString("0.#") %>/<%= load.CapacityDays.ToString("0") %> ngày</small>
                                <div class="mt-1"><span class="badge <%= GetMonthlyStatusBadgeCss(load) %>"><%= GetMonthlyStatusText(load) %></span></div>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <% if (Model.EmployeeLoads.Count == 0) { %>
                        <tr>
                            <td colspan="<%= Model.Months.Count + 1 %>" class="text-center text-muted py-4">
                                Không có nhân sự để tổng hợp trong phạm vi đã chọn.
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card border-0 shadow-sm w-100">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div>
                            <h5 class="card-title mb-1">Nhân sự cần chú ý</h5>
                            <p class="text-muted mb-0">Ưu tiên nhân sự quá tải, sau đó đến nhóm thiếu tải.</p>
                        </div>
                        <span class="badge bg-warning-subtle text-warning"><%= Model.AttentionEmployees.Count %></span>
                    </div>
                    <div class="resource-attention-list">
                        <% foreach (var employee in Model.AttentionEmployees) { %>
                        <div class="resource-attention-item d-flex align-items-start">
                            <span class="resource-attention-dot <%= GetStatusLoadCss(employee.Status) %>"></span>
                            <div class="flex-grow-1 min-width-0">
                                <div class="d-flex justify-content-between gap-2">
                                    <span class="fw-semibold text-truncate"><%: employee.DisplayName %></span>
                                    <span class="badge <%= GetStatusBadgeCss(employee.Status) %>"><%= GetStatusText(employee.Status) %></span>
                                </div>
                                <div class="small text-muted mt-1"><%: GetAttentionText(employee) %></div>
                            </div>
                        </div>
                        <% } %>
                        <% if (Model.AttentionEmployees.Count == 0) { %>
                        <div class="text-center text-muted py-5">Không có nhân sự cần cảnh báo trong tuần.</div>
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
                    <h5 class="card-title text-uppercase mb-1">Xu hướng mức tải nguồn lực</h5>
                    <p class="text-muted mb-0">Mức dùng chung của phạm vi lọc trong 8 tuần, lấy tuần trọng tâm làm mốc.</p>
                </div>
                <div class="small text-muted mt-2 mt-sm-0">Đường nét đứt: tuần tương lai</div>
            </div>
            <div id="resource-load-trend-chart"></div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <h5 class="card-title mb-1">Phân bổ theo dự án trong tuần trọng tâm</h5>
            <p class="text-muted mb-3">
                Ngày công phân bổ là tổng số ngày task chạy của từng người; năng lực khả dụng bằng số người có lịch × 5 ngày.
            </p>
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Dự án</th>
                            <th class="text-center">Nhân sự có lịch</th>
                            <th class="text-center">Ngày công phân bổ</th>
                            <th class="text-center">Năng lực khả dụng</th>
                            <th style="min-width: 210px;">Mức sử dụng</th>
                            <th class="text-center">Đánh giá</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectAllocations) { %>
                        <tr>
                            <td>
                                <div class="fw-semibold"><%: project.ProjectCode %></div>
                                <div class="small text-muted"><%: project.ProjectName %></div>
                            </td>
                            <td class="text-center"><%= project.ResourceCount %></td>
                            <td class="text-center"><%= project.AllocatedDays.ToString("0.#") %> ngày</td>
                            <td class="text-center"><%= project.CapacityDays.ToString("0.#") %> ngày</td>
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
                        <tr><td colspan="6" class="text-center text-muted py-4">Không có dự án phát sinh phân bổ trong tuần trọng tâm.</td></tr>
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
                <div class="small text-muted text-uppercase">Chi tiết phân bổ trong tuần</div>
                <h5 id="resource-detail-title" class="mb-1 mt-1">Nhân sự</h5>
                <div id="resource-detail-subtitle" class="small text-muted"></div>
            </div>
            <button id="resource-detail-close" type="button" class="btn btn-sm btn-light" aria-label="Đóng"><i class="bx bx-x fs-4"></i></button>
        </div>
        <div class="resource-drawer-summary">
            <div>
                <span>Mức tải tuần</span>
                <small id="resource-detail-capacity" class="d-block text-muted mt-1"></small>
            </div>
            <strong id="resource-detail-load">0%</strong>
        </div>
        <div class="resource-drawer-body">
            <h6 class="resource-drawer-section-title">Phân bổ theo dự án</h6>
            <div id="resource-detail-projects"></div>
            <h6 class="resource-drawer-section-title mt-4">Chi tiết công việc</h6>
            <div id="resource-detail-tasks"></div>
        </div>
    </aside>

    <script type="text/javascript">
        window.dashboardResourceTrendData = <%= TrendChartData %>;
        window.dashboardResourceDetailData = <%= ResourceDetailData %>;
    </script>
</div>
