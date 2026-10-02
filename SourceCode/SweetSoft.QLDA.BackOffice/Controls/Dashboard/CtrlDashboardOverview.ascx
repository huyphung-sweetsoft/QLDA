<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDashboardOverview.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardOverview" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

<style>
/* Căn chính giữa số tổng trong các biểu đồ donut */
.dashboard-overview-status-visual,
.dashboard-project-mini-chart {
    position: relative;
}

.dashboard-overview-status-center,
.dashboard-project-chart-center {
    position: absolute !important;
    inset: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
    margin: 0 !important;
    padding: 0 !important;
    transform: none !important;
    text-align: center;
    line-height: 1;
    pointer-events: none;
    z-index: 5;
}

.dashboard-overview-status-center strong,
.dashboard-project-chart-center strong {
    display: block;
    margin: 0;
    padding: 0;
    line-height: 1;
}

.dashboard-overview-status-center small,
.dashboard-project-chart-center small {
    display: block;
    margin-top: 5px;
    line-height: 1;
    white-space: nowrap;
}

/* Responsive riêng cho biểu đồ phân công nhân sự.
   Tránh legend dài ép donut quá hẹp ở zoom 100%. */
.dashboard-project-glance-resource .dashboard-project-chart-layout {
    display: grid !important;
    grid-template-columns: minmax(190px, 220px) minmax(0, 1fr) !important;
    align-items: center !important;
    gap: 12px !important;
    min-width: 0;
}

.dashboard-project-glance-resource .dashboard-project-mini-chart {
    width: 100% !important;
    max-width: 220px !important;
    min-width: 0 !important;
    height: 220px !important;
    flex: none !important;
    justify-self: center;
}

.dashboard-project-glance-resource .dashboard-project-chart-canvas {
    width: 100% !important;
    max-width: 220px !important;
    min-width: 0 !important;
    height: 220px !important;
}

.dashboard-project-glance-resource .dashboard-project-chart-legend {
    width: 100% !important;
    min-width: 0 !important;
    margin: 0 !important;
}

.dashboard-project-glance-resource .dashboard-project-chart-legend button {
    width: 100%;
    min-width: 0;
}

/* Màn hình điện thoại: xếp donut và chú giải thành 2 hàng. */
@media (max-width: 575.98px) {
    .dashboard-project-glance-resource .dashboard-project-chart-layout {
        grid-template-columns: 1fr !important;
        justify-items: center;
    }

    .dashboard-project-glance-resource .dashboard-project-chart-legend {
        max-width: 280px;
    }
}
</style>

<div class="container-fluid dashboard-overview dashboard-overview-simple">
    <div class="dashboard-overview-heading mb-3 <%= IsProjectDashboard ? "dashboard-project-heading" : string.Empty %>">
        <div class="dashboard-overview-heading-main">
            <% if (IsProjectDashboard) { %>
            <SweetSoft:CtrlProjectDashboardTabs runat="server" ID="CtrlProjectDashboardTabs1" />
            <% } else { %>
            <h4 class="mb-0">Thống kê tổng quan</h4>
            <% } %>
        </div>
        <% if (!IsProjectDashboard) { %>
        <div class="dashboard-overview-filter">
            <div class="dashboard-overview-filter-field">
                <label class="form-label mb-1" for="<%= ddlProjectFilter.ClientID %>">Phạm vi dự án</label>
                <asp:DropDownList ID="ddlProjectFilter" runat="server"
                    CssClass="form-select dashboard-overview-project-filter"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlProjectFilter_SelectedIndexChanged" />
            </div>
            <div class="dashboard-overview-filter-field dashboard-overview-time-filter">
                <label class="form-label mb-1" for="<%= ddlDateRange.ClientID %>">Khoảng thời gian</label>
                <asp:DropDownList ID="ddlDateRange" runat="server"
                    CssClass="form-select dashboard-overview-date-range"
                    AutoPostBack="true" />
            </div>
        </div>
        <% } %>
        <div class="dashboard-overview-date-card <%= IsProjectDashboard ? "dashboard-project-date-card" : string.Empty %>">
            <span class="dashboard-overview-date-icon"><i class="bx bx-calendar fs-3"></i></span>
            <span>
                <span class="d-block text-muted small text-uppercase"><%: Summary.GeneratedAt.ToString("dddd", System.Globalization.CultureInfo.CurrentUICulture) %></span>
                <strong><%: Summary.GeneratedAt.ToString("dd/MM/yyyy") %></strong>
            </span>
        </div>
    </div>

    <% if (!IsProjectDashboard) { %>
    <div class="row row-cols-1 row-cols-sm-2 <%= ShowCustomerSignal ? "row-cols-xl-4" : "row-cols-xl-3" %> g-3 mb-3 dashboard-overview-kpis">
        <div class="col">
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="all" data-overview-title="Tất cả dự án">
                <span class="card-body"><span class="text-muted d-block">Tổng dự án</span>
                    <strong class="fs-3 text-dark"><%= Summary.Projects.Count %></strong>
                    <span class="dashboard-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-briefcase-alt-2"></i></span></span>
            </button>
        </div>
        <div class="col">
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="overdue" data-overview-title="Dự án trễ hạn">
                <span class="card-body"><span class="text-muted d-block">Dự án trễ hạn</span>
                    <strong class="fs-3 text-danger"><%= OverdueProjects.Count %></strong>
                    <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-error-circle"></i></span></span>
            </button>
        </div>
        <div class="col">
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="due-soon" data-overview-title="Dự án đến hạn trong 7 ngày tới">
                <span class="card-body"><span class="text-muted d-block">Đến hạn trong 7 ngày tới</span>
                    <strong class="fs-3 dashboard-days-left"><%= UpcomingProjects.Count %></strong>
                    <span class="dashboard-kpi-icon bg-warning-subtle dashboard-days-left"><i class="bx bx-calendar-event"></i></span></span>
            </button>
        </div>
        <% if (ShowCustomerSignal) { %>
        <div class="col">
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewActiveCustomersModal">
                <span class="card-body"><span class="text-muted d-block">Khách hàng đang hợp tác</span>
                    <strong class="fs-3 text-primary"><%= Summary.ActiveCustomerCount %></strong>
                    <span class="dashboard-kpi-icon bg-primary-subtle text-primary"><i class="bx bx-user-check"></i></span></span>
            </button>
        </div>
        <% } %>
    </div>

    <% if (ShowCustomerSignal) { %>
    <div class="modal fade" id="overviewActiveCustomersModal" tabindex="-1" aria-labelledby="overviewActiveCustomersTitle" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="overviewActiveCustomersTitle">Khách hàng đang hợp tác (<%= Summary.ActiveCustomerCount %>)</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button>
            </div>
            <div class="modal-body">
                <input type="search" class="form-control mb-3 dashboard-list-search"
                    data-overview-signal-search="active-customer"
                    placeholder="Tìm tên khách hàng" aria-label="Tìm khách hàng đang hợp tác" />
                <div class="table-responsive">
                    <table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                        <thead><tr><th>Khách hàng</th><th class="text-center">Dự án liên quan</th></tr></thead>
                        <tbody>
                        <% foreach (var customer in Summary.ActiveCustomers) { %>
                        <tr data-overview-signal-row="active-customer">
                            <td><a class="fw-semibold text-decoration-none" href="<%: GetCustomerDetailUrl(customer.CustomerId) %>"><%: customer.CustomerName %></a></td>
                            <td class="text-center"><%= customer.ProjectCount %></td>
                        </tr>
                        <% } %>
                        </tbody>
                    </table>
                </div>
                <p class="text-muted mb-0 py-3 <%= Summary.ActiveCustomers.Count == 0 ? string.Empty : "d-none" %>"
                    data-overview-signal-empty="active-customer">Chưa có khách hàng đang hợp tác.</p>
            </div>
        </div></div>
    </div>
    <% } %>

    <div class="row g-3 mb-3 dashboard-overview-three-up">
        <div class="col-12 col-xl-5">
            <div class="card border-0 shadow-sm h-100">
                <div class="card-header"><h5 class="dashboard-overview-section-title"><i class="bx bx-pie-chart-alt-2" aria-hidden="true"></i> Trạng thái dự án</h5></div>
                <div class="card-body">
                    <div class="dashboard-overview-status-layout">
                        <div class="dashboard-overview-status-visual" style="position:relative!important;">
                            <div id="overviewStatusChart" class="dashboard-overview-status-chart"
                                aria-label="Biểu đồ số dự án theo trạng thái"></div>
                            <span class="dashboard-overview-status-center" style="position:absolute!important;top:50%!important;left:50%!important;right:auto!important;bottom:auto!important;width:auto!important;height:auto!important;margin:0!important;padding:0!important;transform:translate(-50%,-50%)!important;flex-direction:column!important;align-items:center!important;justify-content:center!important;text-align:center!important;line-height:1!important;pointer-events:none!important;z-index:20!important;<%= Summary.Projects.Count == 0 ? "display:none!important;" : "display:flex!important;" %>">
                                <strong><%= Summary.Projects.Count %></strong><small>dự án</small>
                            </span>
                        </div>
                        <div class="dashboard-overview-status-legend">
                            <% foreach (var status in Summary.Statuses) { %>
                            <button type="button" class="dashboard-status-legend-button"
                                data-status-code="<%= status.StatusCode %>"
                                data-status-label="<%: status.Status %>"
                                data-status-count="<%= status.Count %>"
                                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal"
                                data-overview-filter="status-<%= status.StatusCode %>"
                                data-overview-title="<%: status.Status %>">
                                <span class="dashboard-status-dot" aria-hidden="true"></span>
                                <span class="dashboard-status-name"><%: status.Status %></span>
                            </button>
                            <% } %>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-12 col-xl-4">
            <div class="card border-0 shadow-sm h-100 dashboard-attention-card">
                <div class="card-header">
                    <h5 class="dashboard-overview-section-title"><i class="bx bx-error-circle" aria-hidden="true"></i> Dự án cần xử lý <span class="dashboard-attention-count">(<%= PriorityProjects.Count %>)</span></h5>
                </div>
                <div class="card-body">
                    <% if (PriorityProjects.Count == 0) { %>
                    <p class="text-muted m-3">Chưa có dự án, công việc quá hạn hoặc vấn đề ảnh hưởng cao đang xử lý.</p>
                    <% } else { %>
                    <div class="dashboard-attention-scroll" tabindex="0" aria-label="Danh sách dự án cần xử lý, cuộn để xem thêm">
                        <% foreach (var project in PriorityProjects) { %>
                        <button type="button" class="dashboard-attention-item" data-bs-toggle="modal"
                            data-bs-target="#overviewAttentionTasksModal"
                            data-project-id="<%= project.ProjectId %>"
                            data-project-title="<%: project.ProjectCode + " · " + project.ProjectName %>"
                            data-project-url="<%: GetProjectDetailUrl(project.ProjectId) %>"
                            data-project-overdue-days="<%= project.OverdueDays %>">
                            <div class="d-flex justify-content-between align-items-start gap-2">
                                <div class="dashboard-attention-project">
                                    <strong><%: project.ProjectCode %></strong>
                                    <span class="d-block small text-muted"><%: project.ProjectName %></span>
                                </div>
                                <span class="dashboard-attention-open flex-shrink-0">Xem lý do <i class="bx bx-chevron-right" aria-hidden="true"></i></span>
                            </div>
                            <div class="mt-2">
                                <% if (project.IsOverdue) { %><span class="badge bg-danger-subtle text-danger me-1">Trễ hạn <%= project.OverdueDays %> ngày</span><% } %>
                                <% if (project.OverdueTaskCount > 0) { %><span class="badge bg-warning-subtle text-dark"><%= project.OverdueTaskCount %> việc quá hạn</span><% } %>
                                <% if (project.ImportantIssueCount > 0) { %><span class="badge bg-danger-subtle text-danger"><%= project.ImportantIssueCount %> vấn đề ảnh hưởng cao</span><% } %>
                            </div>
                        </button>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </div>
        </div>
        <div class="col-12 col-xl-3">
            <div class="card border-0 shadow-sm h-100 dashboard-meeting-card">
                <div class="card-header"><h5 class="dashboard-overview-section-title"><i class="bx bx-calendar-event" aria-hidden="true"></i> Lịch họp sắp tới</h5></div>
                <div class="card-body">
                    <% if (Summary.Meetings.Count == 0) { %><p class="text-muted mb-0">Chưa có lịch họp sắp tới.</p><% } %>
                    <div class="dashboard-meetings-scroll" tabindex="0" aria-label="Lịch họp sắp tới, cuộn để xem thêm">
                    <% foreach (var meeting in Summary.Meetings) { %>
                    <button type="button" class="dashboard-overview-meeting mb-2" data-bs-toggle="modal"
                        data-bs-target="#overviewMeetingModal"
                        data-meeting-title="<%: meeting.Title %>"
                        data-meeting-project="<%: meeting.ProjectCode + " · " + meeting.ProjectName %>"
                        data-meeting-start="<%: meeting.StartTime.ToString("dd/MM/yyyy HH:mm") %>"
                        data-meeting-end="<%: meeting.EndTime.ToString("dd/MM/yyyy HH:mm") %>"
                        data-meeting-location="<%: meeting.Location %>"
                        data-meeting-content="<%: meeting.Content %>"
                        data-meeting-url="<%: GetProjectMeetingsUrl(meeting.ProjectId) %>">
                        <strong class="d-block"><%: meeting.Title %></strong>
                        <span class="small text-muted"><%: meeting.StartTime.ToString("dd/MM HH:mm") %> · <%: meeting.ProjectCode %></span>
                    </button>
                    <% } %>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3 dashboard-project-glance-row dashboard-all-projects-glance-row">
        <div class="col-12 <%= ShowFinanceSignal && Summary.FinanceSummary != null ? "col-lg-4" : "col-lg-12" %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-progress">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-task"></i> Tiến độ công việc</h5>
                    <p class="dashboard-project-glance-caption"><%= AllTasksCompletedCount %>/<%= Summary.Tasks.Count %> công việc thuộc kỳ <%= SelectedDateRangeText %> đã hoàn thành</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart" style="position:relative!important;">
                            <div id="overviewAllTasksChart" class="dashboard-project-chart-canvas"
                                data-values="<%= AllTasksCompletedCount %>,<%= AllTasksInProgressCount %>,<%= AllTasksNotStartedCount %>"></div>
                            <span class="dashboard-project-chart-center" style="position:absolute!important;top:50%!important;left:50%!important;right:auto!important;bottom:auto!important;width:auto!important;height:auto!important;margin:0!important;padding:0!important;transform:translate(-50%,-50%)!important;display:flex!important;flex-direction:column!important;align-items:center!important;justify-content:center!important;text-align:center!important;line-height:1!important;pointer-events:none!important;z-index:20!important;"><strong><%= Summary.Tasks.Count %></strong><small>công việc</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="completed"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Hoàn thành</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="in-progress"><i class="dashboard-chart-dot" style="background:#518cdd"></i><span>Đang làm</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="not-started"><i class="dashboard-chart-dot" style="background:#8592a6"></i><span>Chưa bắt đầu</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% if (ShowFinanceSignal && Summary.FinanceSummary != null) { %>
        <div class="col-12 col-lg-8">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-cost">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-money"></i> Thu chi <small>lũy kế</small></h5>
                    <p class="dashboard-project-glance-caption"><%= Summary.FinanceSummary.Projects.Count == 0 ? "Chưa có dự án hoàn thành trong phạm vi" : "Tổng hợp thu chi · " + AllFinanceProjectsWithActivityCount + "/" + Summary.FinanceSummary.Projects.Count + " dự án hoàn thành có phát sinh" %></p>
                    <div class="dashboard-finance-chart-layout">
                        <div id="overviewAllFinanceChart" class="dashboard-finance-chart"
                            data-values="<%= Summary.FinanceSummary.ReceivedPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= Summary.FinanceSummary.OutstandingPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= Summary.FinanceSummary.ApprovedCost.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= Summary.FinanceSummary.PendingApprovalCost.ToString(System.Globalization.CultureInfo.InvariantCulture) %>"></div>
                        <div class="dashboard-finance-chart-legend" aria-label="Chú giải thu chi">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="received"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Đã thu</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="outstanding"><i class="dashboard-chart-dot" style="background:#e2a52e"></i><span>Còn phải thu</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="cost"><i class="dashboard-chart-dot" style="background:#518cdd"></i><span>Chi đã duyệt</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="pending"><i class="dashboard-chart-dot" style="background:#e45d53"></i><span>Chi chờ duyệt</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
    </div>

    <% if (ShowIssueSignal || ShowRiskSignal || (ShowResourceSignal && AllProjectsResourceSummary != null)) { %>
    <div class="row g-3 mb-3 dashboard-overview-signal-row">
        <% if (ShowIssueSignal) { %>
        <div class="col-12 <%= ShowRiskSignal && ShowResourceSignal ? "col-lg-4" : (ShowRiskSignal || ShowResourceSignal ? "col-lg-6" : "col-lg-12") %>">
            <section class="card h-100 dashboard-project-issue-card dashboard-overview-record-card">
                <div class="card-header dashboard-overview-record-header">
                    <h5 class="dashboard-overview-section-title"><i class="bx bx-error-circle" aria-hidden="true"></i> Vấn đề đang xử lý <span class="dashboard-project-issue-count"><%= Summary.OpenIssues.Count %></span></h5>
                </div>
                <div class="card-body">
                    <% if (Summary.OpenIssues.Count == 0) { %>
                    <p class="text-muted mb-0 py-3">Chưa có vấn đề đang xử lý.</p>
                    <% } else { %>
                    <div class="dashboard-overview-records" tabindex="0" aria-label="Danh sách vấn đề đang xử lý">
                        <% foreach (var issue in Summary.OpenIssues) { %>
                        <button type="button" class="dashboard-overview-record-item" data-bs-toggle="modal" data-bs-target="#overviewIssueDetailModal"
                            data-issue-code="<%: issue.IssueCode %>" data-issue-name="<%: issue.IssueName %>"
                            data-issue-project="<%: issue.ProjectCode + " · " + issue.ProjectName %>"
                            data-issue-impact="<%: GetIssueImpactText(issue.ImpactLevel) %>"
                            data-issue-description="<%: issue.Description %>" data-issue-plan="<%: issue.HandlingPlan %>">
                            <span class="dashboard-overview-record-copy">
                                <strong><%: issue.IssueCode %> · <%: issue.IssueName %></strong>
                                <small><%: issue.ProjectCode %> · <%: issue.ProjectName %></small>
                                <% if (!string.IsNullOrWhiteSpace(issue.HandlingPlan)) { %><small class="text-muted">Hướng xử lý: <%: issue.HandlingPlan %></small><% } %>
                            </span>
                            <span class="badge bg-warning-subtle text-dark flex-shrink-0"><%: GetIssueImpactText(issue.ImpactLevel) %></span>
                        </button>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </section>
        </div>
        <% } %>
        <% if (ShowRiskSignal) { %>
        <div class="col-12 <%= ShowIssueSignal && ShowResourceSignal ? "col-lg-4" : (ShowIssueSignal || ShowResourceSignal ? "col-lg-6" : "col-lg-12") %>">
            <section class="card h-100 dashboard-project-issue-card dashboard-project-risk-card dashboard-overview-record-card">
                <div class="card-header dashboard-overview-record-header">
                    <h5 class="dashboard-overview-section-title"><i class="bx bx-shield" aria-hidden="true"></i> Rủi ro đã ghi nhận <span class="dashboard-project-issue-count dashboard-project-risk-count"><%= Summary.RecordedRisks.Count %></span></h5>
                </div>
                <div class="card-body">
                    <% if (Summary.RecordedRisks.Count == 0) { %>
                    <p class="text-muted mb-0 py-3">Chưa có rủi ro được ghi nhận.</p>
                    <% } else { %>
                    <div class="dashboard-overview-records" tabindex="0" aria-label="Danh sách rủi ro đã ghi nhận">
                        <% foreach (var risk in Summary.RecordedRisks) { %>
                        <button type="button" class="dashboard-overview-record-item" data-bs-toggle="modal" data-bs-target="#overviewRiskDetailModal"
                            data-risk-name="<%: risk.RiskName %>" data-risk-project="<%: risk.ProjectCode + " · " + risk.ProjectName %>"
                            data-risk-probability="<%: GetRiskProbabilityText(risk.Probability) %>"
                            data-risk-impact="<%: risk.ImpactLevel.HasValue ? GetIssueImpactText(risk.ImpactLevel.Value) : "—" %>"
                            data-risk-score="<%: risk.RiskScore.HasValue ? risk.RiskScore.Value.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture) : "—" %>"
                            data-risk-prevention="<%: risk.PreventionPlan %>" data-risk-response="<%: risk.ResponsePlan %>">
                            <span class="dashboard-overview-record-copy">
                                <strong><%: risk.RiskName %></strong>
                                <small><%: risk.ProjectCode %> · <%: risk.ProjectName %></small>
                                <small class="text-muted">Xác suất: <%: GetRiskProbabilityText(risk.Probability) %> · Ảnh hưởng: <%: risk.ImpactLevel.HasValue ? GetIssueImpactText(risk.ImpactLevel.Value) : "—" %></small>
                            </span>
                            <i class="bx bx-chevron-right text-muted" aria-hidden="true"></i>
                        </button>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </section>
        </div>
        <% } %>
        <% if (ShowResourceSignal && AllProjectsResourceSummary != null) { %>
        <div class="col-12 <%= ShowIssueSignal && ShowRiskSignal ? "col-lg-4" : (ShowIssueSignal || ShowRiskSignal ? "col-lg-6" : "col-lg-12") %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-resource">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-group"></i> Tình hình phân công công việc <small>tuần này</small></h5>
                    <p class="dashboard-project-glance-caption"><%= AllProjectsResourceSummary.TotalEmployeeCount %> nhân sự trong hệ thống</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart" style="position:relative!important;">
                            <div id="overviewAllResourceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= AllResourceNoLoadCount %>,<%= AllResourceNormalCount %>,<%= AllResourceOverloadedCount %>"></div>
                            <span class="dashboard-project-chart-center" style="position:absolute!important;top:50%!important;left:50%!important;right:auto!important;bottom:auto!important;width:auto!important;height:auto!important;margin:0!important;padding:0!important;transform:translate(-50%,-50%)!important;display:flex!important;flex-direction:column!important;align-items:center!important;justify-content:center!important;text-align:center!important;line-height:1!important;pointer-events:none!important;z-index:20!important;"><strong><%= AllProjectsResourceSummary.TotalEmployeeCount %></strong><small>nhân sự</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" title="Rảnh" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="0"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Rảnh</span></button>
                            <button type="button" title="Bình thường" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="1"><i class="dashboard-chart-dot" style="background:#efb63e"></i><span>Bình thường</span></button>
                            <button type="button" title="Quá tải" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="2"><i class="dashboard-chart-dot" style="background:#ef6d63"></i><span>Quá tải</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
    </div>
    <% } %>

    <% if (ShowFinanceSignal) { %>
    <div class="modal fade" id="overviewPendingCostsModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title">Khoản chi chờ duyệt (<%= Summary.PendingCosts.Count %>)</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <input type="search" class="form-control mb-3 dashboard-list-search" data-overview-signal-search="pending-cost"
                    placeholder="Tìm khoản chi hoặc dự án" aria-label="Tìm khoản chi chờ duyệt" />
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table">
                    <thead><tr><th>Khoản chi</th><th>Dự án</th><th class="text-end">Số tiền</th><th>Chi tiết</th></tr></thead>
                    <tbody id="overviewPendingCostsBody" data-project-group-count-format="{0} khoản chi">
                    <% foreach (var cost in Summary.PendingCosts) { %>
                    <tr data-overview-signal-row="pending-cost" data-group-project-id="<%= cost.ProjectId %>"
                        data-group-project-code="<%: cost.ProjectCode %>" data-group-project-name="<%: cost.ProjectName %>">
                        <td><strong><%: cost.CostCode %></strong><br /><small><%: cost.CostName %></small></td>
                        <td><strong><%: cost.ProjectCode %></strong><br /><small><%: cost.ProjectName %></small></td>
                        <td class="text-end text-nowrap"><%= FormatMoney(cost.Amount) %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectCostsUrl(cost.ProjectId) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody>
                </table></div>
                <p class="text-muted mb-0 <%= Summary.PendingCosts.Count == 0 ? string.Empty : "d-none" %>" data-overview-signal-empty="pending-cost">Không có khoản chi phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <div class="modal fade" id="overviewProjectsModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewProjectsModalTitle">Danh sách dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <p class="text-muted mb-0" id="overviewProjectsDescription"></p>
                    <strong class="dashboard-list-count" id="overviewProjectsCount"></strong>
                </div>
                <input type="search" class="form-control mb-3" id="overviewProjectsSearch" placeholder="Tìm mã hoặc tên dự án" aria-label="Tìm dự án" />
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table">
                    <thead><tr><th>Dự án</th><th>Trạng thái</th><th>Hạn dự kiến</th><th id="overviewProjectsContextHeader">Công việc</th><th>Chi tiết</th></tr></thead>
                    <tbody>
                        <% foreach (var project in Summary.Projects) { %>
                        <tr class="overview-project-row" data-status="<%= project.StatusCode %>"
                            data-overdue="<%= project.IsOverdue ? "1" : "0" %>"
                            data-overdue-task="<%= project.OverdueTaskCount > 0 ? "1" : "0" %>"
                            data-due-soon="<%= UpcomingProjects.Contains(project) ? "1" : "0" %>"
                            data-overdue-days="<%= project.OverdueDays %>"
                            data-days-left="<%= (project.ExpectedEndDate.Date - DateTime.Today).Days %>"
                            data-overdue-task-count="<%= project.OverdueTaskCount %>"
                            data-completed-count="<%= project.CompletedTaskCount %>"
                            data-task-count="<%= project.TaskCount %>"
                            data-search="<%: (project.ProjectCode + " " + project.ProjectName).ToLowerInvariant() %>">
                            <td><strong><%: project.ProjectCode %></strong><br /><small><%: project.ProjectName %></small></td>
                            <td><%: project.Status %></td>
                            <td><%: project.ExpectedEndDate.ToString("dd/MM/yyyy") %></td>
                            <td class="overview-project-context"></td>
                            <td><a class="btn btn-outline-primary btn-sm" href="<%: GetProjectDetailUrl(project.ProjectId) %>">Xem chi tiết</a></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table></div>
                <p class="text-muted mb-0 d-none" id="overviewProjectsEmpty">Không có dự án phù hợp.</p>
            </div>
        </div></div>
    </div>
    <div class="modal fade" id="overviewAttentionTasksModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><div><h5 class="modal-title mb-1" id="overviewAttentionTasksTitle">Lý do cần xử lý</h5>
                <span class="small text-muted" id="overviewAttentionTasksProject"></span></div>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAttentionTasksSearch"
                        placeholder="Tìm công việc hoặc vấn đề" aria-label="Tìm việc cần xử lý" />
                    <strong class="dashboard-list-count" id="overviewAttentionTasksCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table dashboard-attention-task-table">
                    <colgroup><col class="dashboard-attention-task-name" /><col class="dashboard-attention-task-date" /><col class="dashboard-attention-task-action" /></colgroup>
                    <thead><tr><th>Việc cần xử lý</th><th>Hạn / ảnh hưởng</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var task in Summary.CurrentTasks.Where(t => t.IsOverdue)) { %>
                    <tr class="overview-attention-task-row" data-project-id="<%= task.ProjectId %>" data-attention-kind="task"
                        data-search="<%: (task.TaskCode + " " + task.TaskName).ToLowerInvariant() %>">
                        <td><span class="badge bg-warning-subtle text-dark me-1">Công việc quá hạn</span><strong><%: task.TaskCode %></strong><br /><small><%: task.TaskName %></small></td>
                        <td><%: task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "—" %></td>
                        <td><a class="btn btn-outline-primary btn-sm" href="<%: GetTaskDetailUrl(task) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    <% foreach (var issue in Summary.ImportantIssues) { %>
                    <tr class="overview-attention-task-row" data-project-id="<%= issue.ProjectId %>" data-attention-kind="issue"
                        data-search="<%: (issue.IssueCode + " " + issue.IssueName + " " + issue.HandlingPlan).ToLowerInvariant() %>">
                        <td><span class="badge bg-danger-subtle text-danger me-1">Vấn đề đang xử lý</span><strong><%: issue.IssueCode %></strong><br /><small><%: issue.IssueName %></small>
                            <% if (!string.IsNullOrWhiteSpace(issue.HandlingPlan)) { %><div class="small text-muted mt-1">Hướng xử lý: <%: issue.HandlingPlan %></div><% } %>
                        </td>
                        <td class="text-danger text-nowrap"><%: GetIssueImpactText(issue.ImpactLevel) %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectIssuesUrl(issue.ProjectId) %>">Mở danh sách vấn đề</a></td>
                    </tr>
                    <% } %>
                    </tbody></table></div>
                <p class="text-muted mb-2 d-none" id="overviewAttentionTasksEmpty">Dự án trễ hạn dự kiến.</p>
            </div>
            <div class="modal-footer"><a id="overviewAttentionProjectLink" class="btn btn-outline-primary btn-sm" href="#">Xem dự án</a></div>
        </div></div>
    </div>
    <div class="modal fade" id="overviewAllTasksModal" tabindex="-1" aria-labelledby="overviewAllTasksTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewAllTasksTitle">Công việc của các dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllTasksSearch" placeholder="Tìm công việc hoặc dự án" aria-label="Tìm công việc hoặc dự án" />
                    <strong class="dashboard-list-count" id="overviewAllTasksCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Công việc</th><th>Hạn</th><th>Trạng thái</th><th>Chi tiết</th></tr></thead>
                    <tbody id="overviewAllTasksBody" data-project-group-count-format="{0} công việc">
                    <% foreach (var task in Summary.Tasks) { %>
                    <tr class="overview-all-task-row" data-task-lifecycle="<%= task.LifecycleStatusCode %>"
                        data-group-project-id="<%= task.ProjectId %>" data-group-project-code="<%: task.ProjectCode %>" data-group-project-name="<%: task.ProjectName %>"
                        data-search="<%: (task.ProjectCode + " " + task.ProjectName + " " + task.TaskCode + " " + task.TaskName).ToLowerInvariant() %>">
                        <td><strong><%: task.TaskCode %></strong><br /><small><%: task.TaskName %></small></td>
                        <td><%: task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "—" %></td>
                        <td><%: GetTaskLifecycleStatusText(task) %><% if (task.IsOverdue) { %><br /><small class="text-danger">Quá hạn</small><% } %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetTaskDetailUrl(task) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody>
                </table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewAllTasksEmpty">Không có công việc phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% if (ShowFinanceSignal && Summary.FinanceSummary != null) { %>
    <div class="modal fade" id="overviewAllFinanceModal" tabindex="-1" aria-labelledby="overviewAllFinanceTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewAllFinanceTitle">Thu tiền và chi phí theo dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <p class="small text-muted mb-2">Chỉ thống kê dự án đã Hoàn thành. Một dự án đã thu một phần vẫn có thể còn phải thu, nên có thể xuất hiện ở cả hai danh sách.</p>
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllFinanceSearch" placeholder="Tìm dự án" aria-label="Tìm dự án" />
                    <strong class="dashboard-list-count" id="overviewAllFinanceCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Dự án</th><th class="text-end">Đã thu</th><th class="text-end">Còn phải thu</th><th class="text-end">Chi phí đã duyệt</th><th class="text-end">Chi phí chờ duyệt</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var project in GetAllProjectsFinanceRows()) { %>
                    <tr class="overview-all-finance-row" data-received="<%= project.ReceivedPayment > 0 ? "1" : "0" %>"
                        data-outstanding="<%= project.OutstandingPayment > 0 ? "1" : "0" %>" data-cost="<%= project.ApprovedCost > 0 ? "1" : "0" %>" data-pending="<%= project.PendingApprovalCost > 0 ? "1" : "0" %>"
                        data-search="<%: (project.ProjectCode + " " + project.ProjectName).ToLowerInvariant() %>">
                        <td><strong><%: project.ProjectCode %></strong><br /><small><%: project.ProjectName %></small></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.ReceivedPayment) %></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.OutstandingPayment) %></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.ApprovedCost) %></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.PendingApprovalCost) %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectCostDashboardUrl(project.ProjectId) %>">Xem chi tiết</a></td>
                    </tr><% } %>
                    </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewAllFinanceEmpty">Không có dự án phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% if (ShowResourceSignal && AllProjectsResourceSummary != null) { %>
    <div class="modal fade" id="overviewAllResourceModal" tabindex="-1" aria-labelledby="overviewAllResourceTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewAllResourceTitle">Tình hình phân công công việc tuần này</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllResourceSearch" placeholder="Tìm nhân sự hoặc dự án" aria-label="Tìm nhân sự hoặc dự án" />
                    <strong class="dashboard-list-count" id="overviewAllResourceCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Nhân sự</th><th>Dự án có việc tuần này</th><th class="text-end">Trạng thái</th><th class="text-end">Nhiều việc nhất trong một ngày</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var employee in AllProjectsResourceSummary.EmployeeLoads) { var week = GetAllProjectsWeekLoad(employee); var loadCode = GetAllProjectsWeekLoadCode(employee); var projectNames = GetAllProjectsWeekProjectNames(employee); %>
                    <tr class="overview-all-resource-row" data-load="<%= loadCode %>"
                        data-search="<%: (employee.DisplayName + " " + employee.UserName + " " + projectNames).ToLowerInvariant() %>">
                        <td><strong><%: employee.DisplayName %></strong></td>
                        <td><%: string.IsNullOrEmpty(projectNames) ? "Chưa có việc trong tuần" : projectNames %></td>
                        <td class="text-end"><%: GetProjectWeekLoadText(loadCode) %></td>
                        <td class="text-end text-nowrap"><%= week == null ? 0 : week.PeakDailyTaskCount %> công việc</td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetEmployeeResourceUrl(employee.EmployeeId) %>">Xem chi tiết</a></td>
                    </tr><% } %>
                    </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewAllResourceEmpty">Không có nhân sự phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% } else if (SelectedProject != null) { %>
    <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
        <strong class="me-2"><%: SelectedProject.ProjectCode %> · <%: SelectedProject.ProjectName %></strong>
        <span class="badge bg-primary-subtle text-primary">Trạng thái: <%: SelectedProject.Status %></span>
        <% if (SelectedProject.IsOverdue) { %>
        <span class="badge bg-danger-subtle text-danger">Dự án trễ hạn <%= SelectedProject.OverdueDays %> ngày</span>
        <% } %>
        <span class="text-muted small ms-auto">Hạn dự kiến: <%: SelectedProject.ExpectedEndDate.ToString("dd/MM/yyyy") %></span>
    </div>
    <div class="row row-cols-1 row-cols-sm-2 <%= ProjectKpiColumnsClass %> g-3 mb-3 dashboard-project-alerts">
        <div class="col"><button type="button" class="card w-100 h-100 text-start dashboard-simple-kpi dashboard-project-signal"
            data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="overdue" data-task-title="Công việc quá hạn">
            <span class="card-body"><span class="text-muted d-block">Công việc quá hạn</span><strong class="fs-3 text-danger"><%= SelectedProject.OverdueTaskCount %></strong>
                <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-error-circle"></i></span></span>
        </button></div>
        <div class="col"><button type="button" class="card w-100 h-100 text-start dashboard-simple-kpi dashboard-project-signal"
            data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="due-soon" data-task-title="Công việc đến hạn trong 7 ngày">
            <span class="card-body"><span class="text-muted d-block">Đến hạn trong 7 ngày</span><strong class="fs-3 dashboard-days-left"><%= DueSoonProjectTasks.Count %></strong>
                <span class="dashboard-kpi-icon bg-warning-subtle dashboard-days-left"><i class="bx bx-calendar-event"></i></span></span>
        </button></div>
        <% if (ShowProjectResourceSummary && ProjectResourceSummary != null) { %>
        <div class="col"><button type="button" class="card w-100 h-100 text-start dashboard-simple-kpi dashboard-project-signal"
            data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="2">
            <span class="card-body"><span class="text-muted d-block">Nhân sự quá tải tuần này</span><strong class="fs-3 <%= ProjectResourceOverloadedCount > 0 ? "text-danger" : "text-dark" %>"><%= ProjectResourceOverloadedCount %></strong>
                <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-group"></i></span></span>
        </button></div>
        <% } %>
    </div>
    <div class="row g-3 mb-3 dashboard-project-glance-row">
        <div class="col-12 <%= ShowProjectCostSummary && ProjectCostSummary != null && ShowProjectResourceSummary && ProjectResourceSummary != null ? "col-lg-4" : (ShowProjectCostSummary && ProjectCostSummary != null || ShowProjectResourceSummary && ProjectResourceSummary != null ? "col-lg-6" : "col-lg-12") %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-progress">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-task"></i> Tiến độ công việc</h5>
                    <p class="dashboard-project-glance-caption"><%= CompletedProjectTasks.Count %>/<%= SelectedProject.TaskCount %> công việc hoàn thành</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart" style="position:relative!important;">
                            <div id="overviewProjectTasksChart" class="dashboard-project-chart-canvas"
                                data-values="<%= ProjectTasksCompletedCount %>,<%= ProjectTasksInProgressCount %>,<%= ProjectTasksNotStartedCount %>"></div>
                            <span class="dashboard-project-chart-center" style="position:absolute!important;top:50%!important;left:50%!important;right:auto!important;bottom:auto!important;width:auto!important;height:auto!important;margin:0!important;padding:0!important;transform:translate(-50%,-50%)!important;display:flex!important;flex-direction:column!important;align-items:center!important;justify-content:center!important;text-align:center!important;line-height:1!important;pointer-events:none!important;z-index:20!important;"><strong><%= SelectedProject.TaskCount %></strong><small>công việc</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="completed" data-task-title="Công việc hoàn thành"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Hoàn thành</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="in-progress" data-task-title="Công việc đang thực hiện"><i class="dashboard-chart-dot" style="background:#518cdd"></i><span>Đang làm</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="not-started" data-task-title="Công việc chưa bắt đầu"><i class="dashboard-chart-dot" style="background:#8592a6"></i><span>Chưa bắt đầu</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% if (ShowProjectCostSummary && ProjectCostSummary != null) { %>
        <div class="col-12 <%= ShowProjectResourceSummary && ProjectResourceSummary != null ? "col-lg-4" : "col-lg-6" %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-cost">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-money"></i> Thu chi <small>lũy kế</small></h5>
                    <p class="dashboard-project-glance-caption"><%= ProjectCostSummary.ProjectCount == 0 ? "Chỉ thống kê dự án đã hoàn thành" : ProjectCostSummary.TotalContractValue > 0 ? "Hợp đồng: " + FormatProjectMoney(ProjectCostSummary.TotalContractValue) : "Chưa ghi nhận giá trị hợp đồng" %></p>
                    <div class="dashboard-finance-chart-layout">
                        <div id="overviewProjectFinanceChart" class="dashboard-finance-chart"
                            data-values="<%= ProjectCostSummary.ReceivedPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= ProjectCostSummary.OutstandingPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= ProjectCostSummary.ActualCost.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= ProjectCostSummary.PendingApprovalCost.ToString(System.Globalization.CultureInfo.InvariantCulture) %>"></div>
                        <div class="dashboard-finance-chart-legend" aria-label="Chú giải thu chi">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="all"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Đã thu</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="all"><i class="dashboard-chart-dot" style="background:#e2a52e"></i><span>Còn phải thu</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="approved"><i class="dashboard-chart-dot" style="background:#518cdd"></i><span>Chi đã duyệt</span></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="pending"><i class="dashboard-chart-dot" style="background:#e45d53"></i><span>Chi chờ duyệt</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
        <% if (ShowProjectResourceSummary && ProjectResourceSummary != null) { %>
        <div class="col-12 <%= ShowProjectCostSummary && ProjectCostSummary != null ? "col-lg-4" : "col-lg-6" %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-resource">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-group"></i> Tình hình phân công công việc <small>tuần này</small></h5>
                    <p class="dashboard-project-glance-caption"><%= ProjectResourceSummary.TotalEmployeeCount %> nhân sự thuộc dự án</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart" style="position:relative!important;">
                            <div id="overviewProjectResourceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= ProjectResourceNoLoadCount %>,<%= ProjectResourceNormalCount %>,<%= ProjectResourceOverloadedCount %>"></div>
                            <span class="dashboard-project-chart-center" style="position:absolute!important;top:50%!important;left:50%!important;right:auto!important;bottom:auto!important;width:auto!important;height:auto!important;margin:0!important;padding:0!important;transform:translate(-50%,-50%)!important;display:flex!important;flex-direction:column!important;align-items:center!important;justify-content:center!important;text-align:center!important;line-height:1!important;pointer-events:none!important;z-index:20!important;"><strong><%= ProjectResourceSummary.TotalEmployeeCount %></strong><small>nhân sự</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" title="Rảnh" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="0"><i class="dashboard-chart-dot" style="background:#35a875"></i><span>Rảnh</span></button>
                            <button type="button" title="Bình thường" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="1"><i class="dashboard-chart-dot" style="background:#efb63e"></i><span>Bình thường</span></button>
                            <button type="button" title="Quá tải" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="2"><i class="dashboard-chart-dot" style="background:#ef6d63"></i><span>Quá tải</span></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
    </div>
    <div class="row g-3 mb-3 dashboard-overview-signal-row">
        <% if (ShowIssueSignal) { %>
        <div class="col-12 <%= ShowRiskSignal ? "col-lg-4" : "col-lg-6" %>">
            <section class="card h-100 dashboard-project-issue-card dashboard-overview-record-card">
                <div class="card-header dashboard-overview-record-header">
                    <h5 class="dashboard-overview-section-title"><i class="bx bx-error-circle" aria-hidden="true"></i> Vấn đề đang xử lý <span class="dashboard-project-issue-count"><%= Summary.OpenIssues.Count(i => i.ProjectId == SelectedProject.ProjectId) %></span></h5>
                </div>
                <div class="card-body">
                    <% if (!Summary.OpenIssues.Any(i => i.ProjectId == SelectedProject.ProjectId)) { %>
                    <p class="text-muted mb-0 py-3">Chưa có vấn đề đang xử lý.</p>
                    <% } else { %>
                    <div class="dashboard-overview-records" tabindex="0" aria-label="Danh sách vấn đề đang xử lý của dự án">
                        <% foreach (var issue in Summary.OpenIssues.Where(i => i.ProjectId == SelectedProject.ProjectId)) { %>
                        <button type="button" class="dashboard-overview-record-item" data-bs-toggle="modal" data-bs-target="#overviewIssueDetailModal"
                            data-issue-code="<%: issue.IssueCode %>" data-issue-name="<%: issue.IssueName %>"
                            data-issue-project="<%: issue.ProjectCode + " · " + issue.ProjectName %>"
                            data-issue-impact="<%: GetIssueImpactText(issue.ImpactLevel) %>"
                            data-issue-description="<%: issue.Description %>" data-issue-plan="<%: issue.HandlingPlan %>">
                            <span class="dashboard-overview-record-copy">
                                <strong><%: issue.IssueCode %> · <%: issue.IssueName %></strong>
                                <small>Mức ảnh hưởng: <%: GetIssueImpactText(issue.ImpactLevel) %></small>
                                <% if (!string.IsNullOrWhiteSpace(issue.HandlingPlan)) { %><small class="text-muted">Hướng xử lý: <%: issue.HandlingPlan %></small><% } %>
                            </span>
                            <i class="bx bx-chevron-right text-muted" aria-hidden="true"></i>
                        </button>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </section>
        </div>
        <% } %>
        <% if (ShowRiskSignal) { %>
        <div class="col-12 <%= ShowIssueSignal ? "col-lg-4" : "col-lg-6" %>">
            <section class="card h-100 dashboard-project-issue-card dashboard-project-risk-card dashboard-overview-record-card">
                <div class="card-header dashboard-overview-record-header">
                    <h5 class="dashboard-overview-section-title"><i class="bx bx-shield" aria-hidden="true"></i> Rủi ro đã ghi nhận <span class="dashboard-project-issue-count dashboard-project-risk-count"><%= Summary.RecordedRisks.Count(r => r.ProjectId == SelectedProject.ProjectId) %></span></h5>
                </div>
                <div class="card-body">
                    <% if (!Summary.RecordedRisks.Any(r => r.ProjectId == SelectedProject.ProjectId)) { %>
                    <p class="text-muted mb-0 py-3">Chưa có rủi ro được ghi nhận.</p>
                    <% } else { %>
                    <div class="dashboard-overview-records" tabindex="0" aria-label="Danh sách rủi ro đã ghi nhận của dự án">
                        <% foreach (var risk in Summary.RecordedRisks.Where(r => r.ProjectId == SelectedProject.ProjectId)) { %>
                        <button type="button" class="dashboard-overview-record-item" data-bs-toggle="modal" data-bs-target="#overviewRiskDetailModal"
                            data-risk-name="<%: risk.RiskName %>" data-risk-project="<%: risk.ProjectCode + " · " + risk.ProjectName %>"
                            data-risk-probability="<%: GetRiskProbabilityText(risk.Probability) %>"
                            data-risk-impact="<%: risk.ImpactLevel.HasValue ? GetIssueImpactText(risk.ImpactLevel.Value) : "—" %>"
                            data-risk-score="<%: risk.RiskScore.HasValue ? risk.RiskScore.Value.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture) : "—" %>"
                            data-risk-prevention="<%: risk.PreventionPlan %>" data-risk-response="<%: risk.ResponsePlan %>">
                            <span class="dashboard-overview-record-copy">
                                <strong><%: risk.RiskName %></strong>
                                <small>Xác suất: <%: GetRiskProbabilityText(risk.Probability) %> · Ảnh hưởng: <%: risk.ImpactLevel.HasValue ? GetIssueImpactText(risk.ImpactLevel.Value) : "—" %></small>
                            </span>
                            <i class="bx bx-chevron-right text-muted" aria-hidden="true"></i>
                        </button>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </section>
        </div>
        <% } %>
        <div class="col-12 <%= ShowIssueSignal && ShowRiskSignal ? "col-lg-4" : (ShowIssueSignal || ShowRiskSignal ? "col-lg-6" : "col-lg-12") %>">
            <div class="card h-100 dashboard-project-meeting-card">
                <div class="card-header"><h5 class="dashboard-overview-section-title"><i class="bx bx-calendar-event" aria-hidden="true"></i> Lịch họp sắp tới</h5></div>
                <div class="card-body">
                    <% if (Summary.Meetings.Count == 0) { %><p class="text-muted mb-0">Chưa có lịch họp sắp tới.</p><% } %>
                    <div class="dashboard-meetings-scroll" tabindex="0" aria-label="Lịch họp sắp tới, cuộn để xem thêm">
                    <% foreach (var meeting in Summary.Meetings) { %>
                    <button type="button" class="dashboard-overview-meeting mb-2" data-bs-toggle="modal"
                        data-bs-target="#overviewMeetingModal"
                        data-meeting-title="<%: meeting.Title %>"
                        data-meeting-project="<%: meeting.ProjectCode + " · " + meeting.ProjectName %>"
                        data-meeting-start="<%: meeting.StartTime.ToString("dd/MM/yyyy HH:mm") %>"
                        data-meeting-end="<%: meeting.EndTime.ToString("dd/MM/yyyy HH:mm") %>"
                        data-meeting-location="<%: meeting.Location %>"
                        data-meeting-content="<%: meeting.Content %>"
                        data-meeting-url="<%: GetProjectMeetingsUrl(meeting.ProjectId) %>">
                        <strong class="d-block"><%: meeting.Title %></strong>
                        <span class="small text-muted"><%: meeting.StartTime.ToString("dd/MM HH:mm") %></span>
                    </button>
                    <% } %>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal fade" id="overviewTasksModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewTasksModalTitle">Công việc của dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="text-end mb-2"><strong class="dashboard-list-count" id="overviewTasksCount"></strong></div>
                <input type="search" class="form-control mb-3" id="overviewTasksSearch" placeholder="Tìm mã hoặc tên công việc" aria-label="Tìm công việc" />
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table"><thead>
                    <tr><th>Công việc</th><th>Hạn</th><th>Trạng thái</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var task in ProjectTasks) { %>
                    <tr class="overview-task-row" data-overdue="<%= OverdueProjectTasks.Contains(task) ? "1" : "0" %>"
                        data-completed="<%= task.IsCompleted ? "1" : "0" %>"
                        data-in-progress="<%= task.LifecycleStatusCode == 1 ? "1" : "0" %>"
                        data-not-started="<%= task.LifecycleStatusCode == 0 ? "1" : "0" %>"
                        data-due-soon="<%= DueSoonProjectTasks.Contains(task) ? "1" : "0" %>"
                        data-search="<%: (task.TaskCode + " " + task.TaskName).ToLowerInvariant() %>">
                        <td><strong><%: task.TaskCode %></strong><br /><small><%: task.TaskName %></small></td>
                        <td><%: task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "—" %></td>
                        <td><%: GetTaskLifecycleStatusText(task) %><% if (OverdueProjectTasks.Contains(task)) { %><br /><small class="text-danger">Quá hạn</small><% } %></td>
                        <td><a class="btn btn-outline-primary btn-sm" href="<%: GetTaskDetailUrl(task) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                </tbody></table></div><p class="text-muted mb-0 d-none" id="overviewTasksEmpty">Không có công việc phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% if (ShowProjectCostSummary && ProjectCostSummary != null) { %>
    <div class="modal fade" id="overviewProjectFinanceModal" tabindex="-1" aria-labelledby="overviewProjectFinanceTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewProjectFinanceTitle">Thu tiền và chi phí của dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="row g-2 mb-3 dashboard-project-finance-facts">
                    <div class="col-6 col-lg-3"><span>Giá trị hợp đồng</span><strong><%: FormatProjectMoney(ProjectCostSummary.TotalContractValue) %></strong></div>
                    <div class="col-6 col-lg-3"><span>Đã thu</span><strong><%: FormatProjectMoney(ProjectCostSummary.ReceivedPayment) %></strong></div>
                    <div class="col-6 col-lg-3"><span>Còn phải thu</span><strong><%: FormatProjectMoney(ProjectCostSummary.OutstandingPayment) %></strong></div>
                    <div class="col-6 col-lg-3"><span>Chi phí đã duyệt</span><strong><%: FormatProjectMoney(ProjectCostSummary.ActualCost) %></strong></div>
                </div>
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
                    <strong>Khoản chi <span class="dashboard-list-count" id="overviewProjectFinanceCount"></span></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewProjectFinanceSearch" placeholder="Tìm mã hoặc tên khoản chi" aria-label="Tìm khoản chi" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0"><thead>
                    <tr><th>Khoản chi</th><th>Ngày ghi nhận</th><th>Trạng thái</th><th>Số tiền</th></tr></thead><tbody>
                    <% foreach (var cost in ProjectCostSummary.PendingApprovalCostItems) { %>
                    <tr class="overview-project-cost-row" data-cost-status="pending" data-search="<%: (cost.CostCode + " " + cost.CostName).ToLowerInvariant() %>">
                        <td><strong><%: cost.CostCode %></strong><br /><small><%: cost.CostName %></small></td>
                        <td><%: cost.OccurredDate.ToString("dd/MM/yyyy") %></td><td>Chờ duyệt</td><td><%: FormatProjectMoney(cost.Amount) %></td>
                    </tr><% } %>
                    <% foreach (var cost in ProjectCostSummary.ApprovedCostItems) { %>
                    <tr class="overview-project-cost-row" data-cost-status="approved" data-search="<%: (cost.CostCode + " " + cost.CostName).ToLowerInvariant() %>">
                        <td><strong><%: cost.CostCode %></strong><br /><small><%: cost.CostName %></small></td>
                        <td><%: cost.OccurredDate.ToString("dd/MM/yyyy") %></td><td>Đã duyệt</td><td><%: FormatProjectMoney(cost.Amount) %></td>
                    </tr><% } %>
                </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewProjectFinanceEmpty">Không có khoản chi phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% if (ShowProjectResourceSummary && ProjectResourceSummary != null) { %>
    <div class="modal fade" id="overviewProjectResourceModal" tabindex="-1" aria-labelledby="overviewProjectResourceTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewProjectResourceTitle">Tình hình phân công công việc tuần này</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
                    <strong class="dashboard-list-count" id="overviewProjectResourceCount"></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewProjectResourceSearch" placeholder="Tìm nhân sự" aria-label="Tìm nhân sự" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0"><thead>
                    <tr><th>Nhân sự</th><th>Trạng thái</th><th>Nhiều việc nhất trong một ngày</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var employee in ProjectResourceSummary.EmployeeLoads) { var week = GetProjectWeekLoad(employee); var loadCode = GetProjectWeekLoadCode(employee); %>
                    <tr class="overview-project-resource-row" data-load="<%= loadCode %>" data-search="<%: (employee.DisplayName + " " + employee.UserName).ToLowerInvariant() %>">
                        <td><strong><%: employee.DisplayName %></strong></td>
                        <td><%: GetProjectWeekLoadText(loadCode) %></td>
                        <td><%= week == null ? 0 : week.PeakDailyTaskCount %> công việc</td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectResourceEmployeeUrl(employee.EmployeeId) %>">Xem chi tiết</a></td>
                    </tr><% } %>
                </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewProjectResourceEmpty">Không có nhân sự phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% } else { %>
    <div class="alert alert-warning mb-0">Không tìm thấy dự án trong phạm vi đang xem.</div>
    <% } %>
    <div class="modal fade" id="overviewMeetingModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewMeetingTitle">Lịch họp</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <dl class="dashboard-meeting-details mb-3">
                    <dt>Dự án</dt><dd id="overviewMeetingProject"></dd>
                    <dt>Bắt đầu</dt><dd id="overviewMeetingStart"></dd>
                    <dt>Kết thúc</dt><dd id="overviewMeetingEnd"></dd>
                    <dt>Địa điểm</dt><dd id="overviewMeetingLocation"></dd>
                    <dt>Nội dung</dt><dd id="overviewMeetingContent"></dd>
                </dl>
                <a class="btn btn-outline-primary btn-sm" id="overviewMeetingLink" href="#">Xem lịch họp của dự án</a>
            </div>
        </div></div>
    </div>
    <% if (ShowIssueSignal) { %>
    <div class="modal fade" id="overviewIssueDetailModal" tabindex="-1" aria-labelledby="overviewIssueDetailTitle" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewIssueDetailTitle">Chi tiết vấn đề</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <h6 id="overviewIssueDetailName" class="mb-3"></h6>
                <dl class="dashboard-meeting-details dashboard-overview-detail-list mb-0">
                    <dt>Mã vấn đề</dt><dd id="overviewIssueDetailCode"></dd>
                    <dt>Dự án</dt><dd id="overviewIssueDetailProject"></dd>
                    <dt>Trạng thái</dt><dd>Đang xử lý</dd>
                    <dt>Mức ảnh hưởng</dt><dd id="overviewIssueDetailImpact"></dd>
                    <dt>Mô tả</dt><dd id="overviewIssueDetailDescription"></dd>
                    <dt>Hướng xử lý</dt><dd id="overviewIssueDetailPlan"></dd>
                </dl>
            </div>
        </div></div>
    </div>
    <% } %>
    <% if (ShowRiskSignal) { %>
    <div class="modal fade" id="overviewRiskDetailModal" tabindex="-1" aria-labelledby="overviewRiskDetailTitle" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewRiskDetailTitle">Chi tiết rủi ro</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <h6 id="overviewRiskDetailName" class="mb-3"></h6>
                <dl class="dashboard-meeting-details dashboard-overview-detail-list mb-0">
                    <dt>Dự án</dt><dd id="overviewRiskDetailProject"></dd>
                    <dt>Xác suất xảy ra</dt><dd id="overviewRiskDetailProbability"></dd>
                    <dt>Mức ảnh hưởng</dt><dd id="overviewRiskDetailImpact"></dd>
                    <dt>Điểm rủi ro</dt><dd id="overviewRiskDetailScore"></dd>
                    <dt>Kế hoạch phòng ngừa</dt><dd id="overviewRiskDetailPrevention"></dd>
                    <dt>Kế hoạch ứng phó</dt><dd id="overviewRiskDetailResponse"></dd>
                </dl>
            </div>
        </div></div>
    </div>
    <% } %>
</div>
<script type="text/javascript">
    (function () {
        function bindSignalModal(kind) {
            var input = document.querySelector('[data-overview-signal-search="' + kind + '"]');
            if (!input) return;
            var modal = input.closest('.modal');
            var rows = modal.querySelectorAll('[data-overview-signal-row="' + kind + '"]');
            var empty = modal.querySelector('[data-overview-signal-empty="' + kind + '"]');
            var groups = null;
            function refresh() {
                if (kind === 'pending-cost' && !groups && window.DashboardProjectGroups)
                    groups = window.DashboardProjectGroups.create(document.getElementById('overviewPendingCostsBody'));
                var query = (input.value || '').trim().toLocaleLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var show = row.textContent.toLocaleLowerCase().indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) visible++;
                });
                if (groups) groups.refresh(Boolean(query));
                empty.classList.toggle('d-none', visible > 0);
            }
            modal.addEventListener('show.bs.modal', function () {
                if (groups) groups.reset();
                input.value = '';
                refresh();
            });
            input.addEventListener('input', refresh);
        }

        function bindProjectModal() {
            var modal = document.getElementById('overviewProjectsModal');
            if (!modal) return;
            var input = document.getElementById('overviewProjectsSearch');
            var title = document.getElementById('overviewProjectsModalTitle');
            var description = document.getElementById('overviewProjectsDescription');
            var count = document.getElementById('overviewProjectsCount');
            var contextHeader = document.getElementById('overviewProjectsContextHeader');
            var empty = document.getElementById('overviewProjectsEmpty');
            var rows = modal.querySelectorAll('.overview-project-row');
            var filter = 'all';

            function matches(row) {
                if (filter.indexOf('status-') === 0)
                    return row.getAttribute('data-status') === filter.substring(7);
                if (filter === 'attention')
                    return row.getAttribute('data-overdue') === '1'
                        || row.getAttribute('data-overdue-task') === '1';
                return filter === 'all' || row.getAttribute('data-' + filter) === '1';
            }

            function describeRow(row) {
                var done = row.getAttribute('data-completed-count');
                var total = row.getAttribute('data-task-count');
                var overdueDays = row.getAttribute('data-overdue-days');
                var overdueTasks = row.getAttribute('data-overdue-task-count');
                var daysLeft = row.getAttribute('data-days-left');
                var cell = row.querySelector('.overview-project-context');
                cell.className = 'overview-project-context';
                if (filter === 'overdue') {
                    cell.textContent = 'Quá hạn ' + overdueDays + ' ngày';
                    cell.classList.add('text-danger', 'fw-semibold');
                } else if (filter === 'overdue-task') {
                    cell.textContent = overdueTasks + ' công việc quá hạn';
                    cell.classList.add('dashboard-attention-text');
                } else if (filter === 'attention') {
                    var reasons = [];
                    if (row.getAttribute('data-overdue') === '1') reasons.push('Dự án trễ hạn ' + overdueDays + ' ngày');
                    if (row.getAttribute('data-overdue-task') === '1') reasons.push(overdueTasks + ' việc quá hạn');
                    cell.textContent = reasons.join(' · ');
                    cell.classList.add('dashboard-attention-text');
                } else if (filter === 'due-soon') {
                    cell.textContent = daysLeft === '0' ? 'Đến hạn hôm nay' : 'Còn ' + daysLeft + ' ngày';
                    cell.classList.add('dashboard-days-left', 'fw-semibold');
                } else {
                    cell.textContent = done + '/' + total + ' việc hoàn thành';
                }
            }

            function refresh() {
                var query = (input.value || '').trim().toLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var show = matches(row) && (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) { describeRow(row); visible++; }
                });
                count.textContent = visible + ' dự án';
                empty.classList.toggle('d-none', visible > 0);
            }

            function select(trigger) {
                if (!trigger) return;
                filter = trigger.getAttribute('data-overview-filter') || 'all';
                title.textContent = trigger.getAttribute('data-overview-title') || 'Danh sách dự án';
                description.textContent = filter === 'overdue'
                    ? 'Hạn dự kiến đã qua, dự án chưa hoàn thành hoặc kết thúc.'
                    : filter === 'overdue-task'
                        ? 'Dự án có ít nhất một công việc chưa hoàn thành đã quá hạn.'
                        : filter === 'attention'
                            ? 'Dự án trễ hạn hoặc có công việc quá hạn.'
                            : filter === 'due-soon'
                                ? 'Dự án đang thực hiện, đến hạn trong 7 ngày tới.'
                                : filter.indexOf('status-') === 0
                                    ? 'Các dự án thuộc trạng thái đã chọn.'
                                    : 'Toàn bộ dự án trong phạm vi hiện tại.';
                contextHeader.textContent = filter === 'overdue' ? 'Số ngày trễ hạn'
                    : filter === 'overdue-task' ? 'Công việc quá hạn'
                        : filter === 'attention' ? 'Lý do cần xử lý'
                            : filter === 'due-soon' ? 'Thời gian còn lại'
                                : 'Công việc hoàn thành';
                input.value = '';
                refresh();
            }

            Array.prototype.forEach.call(document.querySelectorAll('[data-bs-target="#overviewProjectsModal"]'), function (trigger) {
                trigger.addEventListener('click', function () { select(trigger); });
            });
            modal.addEventListener('show.bs.modal', function (event) { select(event.relatedTarget); });
            input.addEventListener('input', refresh);
        }

        function bindTaskModal() {
            var modal = document.getElementById('overviewTasksModal');
            if (!modal) return;
            var input = document.getElementById('overviewTasksSearch');
            var title = document.getElementById('overviewTasksModalTitle');
            var count = document.getElementById('overviewTasksCount');
            var empty = document.getElementById('overviewTasksEmpty');
            var rows = modal.querySelectorAll('.overview-task-row');
            var filter = 'all';
            function refresh() {
                var query = (input.value || '').trim().toLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var belongs = filter === 'all' || (filter === 'attention'
                        ? row.getAttribute('data-overdue') === '1' || row.getAttribute('data-due-soon') === '1'
                        : row.getAttribute('data-' + filter) === '1');
                    var show = belongs && (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) visible++;
                });
                count.textContent = visible + ' công việc';
                empty.classList.toggle('d-none', visible > 0);
            }
            function select(trigger) {
                if (!trigger) return;
                filter = trigger.getAttribute('data-task-filter') || 'all';
                title.textContent = trigger.getAttribute('data-task-title') || 'Danh sách công việc';
                input.value = '';
                refresh();
            }
            Array.prototype.forEach.call(document.querySelectorAll('[data-bs-target="#overviewTasksModal"]'), function (trigger) {
                trigger.addEventListener('click', function () { select(trigger); });
            });
            modal.addEventListener('show.bs.modal', function (event) { select(event.relatedTarget); });
            input.addEventListener('input', refresh);
        }

        function bindRecordDetailModal(modalId, fields) {
            var modal = document.getElementById(modalId);
            if (!modal) return;
            modal.addEventListener('show.bs.modal', function (event) {
                var trigger = event.relatedTarget;
                if (!trigger) return;
                fields.forEach(function (field) {
                    var target = document.getElementById(field.id);
                    if (!target) return;
                    var value = trigger.getAttribute(field.attribute);
                    target.textContent = value && value.trim() ? value : '—';
                });
            });
        }

        function bindProjectFinanceModal() {
            var modal = document.getElementById('overviewProjectFinanceModal');
            if (!modal) return;
            var input = document.getElementById('overviewProjectFinanceSearch');
            var count = document.getElementById('overviewProjectFinanceCount');
            var empty = document.getElementById('overviewProjectFinanceEmpty');
            var title = document.getElementById('overviewProjectFinanceTitle');
            var rows = modal.querySelectorAll('.overview-project-cost-row');
            var filter = 'all';
            function refresh() {
                var query = (input.value || '').trim().toLocaleLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var show = (filter === 'all' || row.getAttribute('data-cost-status') === filter)
                        && (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) visible++;
                });
                count.textContent = '(' + visible + ')';
                empty.classList.toggle('d-none', visible > 0);
            }
            modal.addEventListener('show.bs.modal', function (event) {
                filter = event.relatedTarget && event.relatedTarget.getAttribute('data-finance-filter') || 'all';
                title.textContent = filter === 'pending' ? 'Chi phí chờ duyệt' : 'Thu tiền và chi phí của dự án';
                input.value = '';
                refresh();
            });
            input.addEventListener('input', refresh);
        }

        function bindProjectResourceModal() {
            var modal = document.getElementById('overviewProjectResourceModal');
            if (!modal) return;
            var input = document.getElementById('overviewProjectResourceSearch');
            var count = document.getElementById('overviewProjectResourceCount');
            var empty = document.getElementById('overviewProjectResourceEmpty');
            var title = document.getElementById('overviewProjectResourceTitle');
            var rows = modal.querySelectorAll('.overview-project-resource-row');
            var labels = ['Rảnh', 'Bình thường', 'Quá tải'];
            var filter = 'all';
            function refresh() {
                var query = (input.value || '').trim().toLocaleLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var show = (filter === 'all' || row.getAttribute('data-load') === filter)
                        && (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) visible++;
                });
                count.textContent = visible + ' nhân sự';
                empty.classList.toggle('d-none', visible > 0);
            }
            modal.addEventListener('show.bs.modal', function (event) {
                filter = event.relatedTarget && event.relatedTarget.getAttribute('data-load-filter') || 'all';
                title.textContent = filter === 'all' ? 'Tình hình phân công công việc tuần này'
                    : 'Nhân sự: ' + labels[Number(filter)] + ' · tuần này';
                input.value = '';
                refresh();
            });
            input.addEventListener('input', refresh);
        }

        function bindAllProjectsBreakdown(modalId, rowClass, filterAttribute, labels, unit) {
            var modal = document.getElementById(modalId);
            if (!modal) return;
            var input = modal.querySelector('input[type="search"]');
            var title = modal.querySelector('.modal-title');
            var count = modal.querySelector('.dashboard-list-count');
            var empty = modal.querySelector('.modal-body > p:last-child');
            var rows = modal.querySelectorAll(rowClass);
            var body = modalId === 'overviewAllTasksModal'
                ? document.getElementById('overviewAllTasksBody') : null;
            var groups = body && window.DashboardProjectGroups
                ? window.DashboardProjectGroups.create(body) : null;
            var filter = 'all';
            function refresh() {
                var query = (input.value || '').trim().toLocaleLowerCase();
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var matchesFilter = filter === 'all';
                    if (modalId === 'overviewAllTasksModal')
                        matchesFilter = matchesFilter || row.getAttribute(filterAttribute) ===
                            { 'not-started': '0', 'in-progress': '1', completed: '2' }[filter];
                    else if (modalId === 'overviewAllFinanceModal')
                        matchesFilter = matchesFilter || row.getAttribute('data-' + filter) === '1';
                    else
                        matchesFilter = matchesFilter || row.getAttribute(filterAttribute) === filter;
                    var show = matchesFilter
                        && (row.getAttribute('data-search') || '').toLocaleLowerCase().indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) visible++;
                });
                if (groups) groups.refresh(Boolean(query));
                count.textContent = visible + ' ' + unit;
                empty.classList.toggle('d-none', visible > 0);
            }
            modal.addEventListener('show.bs.modal', function (event) {
                filter = event.relatedTarget && event.relatedTarget.getAttribute(
                    modalId === 'overviewAllTasksModal' ? 'data-task-filter'
                        : modalId === 'overviewAllFinanceModal' ? 'data-finance-filter' : 'data-load-filter') || 'all';
                title.textContent = labels[filter] || labels.all;
                input.value = '';
                if (groups) groups.reset();
                refresh();
            });
            input.addEventListener('input', refresh);
        }

        bindProjectModal();
        bindTaskModal();
        bindRecordDetailModal('overviewIssueDetailModal', [
            { id: 'overviewIssueDetailCode', attribute: 'data-issue-code' },
            { id: 'overviewIssueDetailName', attribute: 'data-issue-name' },
            { id: 'overviewIssueDetailProject', attribute: 'data-issue-project' },
            { id: 'overviewIssueDetailImpact', attribute: 'data-issue-impact' },
            { id: 'overviewIssueDetailDescription', attribute: 'data-issue-description' },
            { id: 'overviewIssueDetailPlan', attribute: 'data-issue-plan' }
        ]);
        bindRecordDetailModal('overviewRiskDetailModal', [
            { id: 'overviewRiskDetailName', attribute: 'data-risk-name' },
            { id: 'overviewRiskDetailProject', attribute: 'data-risk-project' },
            { id: 'overviewRiskDetailProbability', attribute: 'data-risk-probability' },
            { id: 'overviewRiskDetailImpact', attribute: 'data-risk-impact' },
            { id: 'overviewRiskDetailScore', attribute: 'data-risk-score' },
            { id: 'overviewRiskDetailPrevention', attribute: 'data-risk-prevention' },
            { id: 'overviewRiskDetailResponse', attribute: 'data-risk-response' }
        ]);
        bindProjectFinanceModal();
        bindProjectResourceModal();
        bindAllProjectsBreakdown('overviewAllTasksModal', '.overview-all-task-row', 'data-task-lifecycle',
            {
                all: 'Công việc của các dự án', completed: 'Công việc hoàn thành',
                'in-progress': 'Công việc đang làm', 'not-started': 'Công việc chưa bắt đầu'
            }, 'công việc');
        bindAllProjectsBreakdown('overviewAllFinanceModal', '.overview-all-finance-row', 'data-finance-filter',
            {
                all: 'Thu tiền và chi phí theo dự án', received: 'Đã thu theo dự án',
                outstanding: 'Còn phải thu theo dự án', cost: 'Chi phí đã duyệt theo dự án',
                pending: 'Chi phí chờ duyệt theo dự án'
            }, 'dự án');
        bindAllProjectsBreakdown('overviewAllResourceModal', '.overview-all-resource-row', 'data-load',
            {
                all: 'Tình hình phân công công việc tuần này', '0': 'Nhân sự rảnh tuần này',
                '1': 'Nhân sự bình thường tuần này', '2': 'Nhân sự quá tải tuần này'
            }, 'nhân sự');
        bindSignalModal('pending-cost');
        bindSignalModal('overloaded-employee');
        bindSignalModal('active-customer');

        var smallSliceLabelAngle = 14;

        function formatChartShare(percent) {
            if (percent > 0 && percent < 1) return '<1%';
            return Math.round(percent) + '%';
        }

        function scheduleSmallPieLabels(element, chart, values, colors) {
            window.setTimeout(function () {
                placeSmallPieLabels(element, chart, values, colors);
            }, 0);
        }

        // Reposition only narrow-slice labels outside the ring; ApexCharts 3.x has no external pie-label option.
        function placeSmallPieLabels(element, chart, values, colors) {
            var svg = element && element.querySelector('svg');
            var globals = chart && chart.w && chart.w.globals;
            if (!svg || !globals) return;

            // Center totals are positioned by CSS relative to the chart wrapper.
            // ApexCharts grid dimensions can exclude padding, so using them here
            // shifts the total away from the actual donut center.
            Array.prototype.forEach.call(svg.querySelectorAll('[data-dashboard-pie-connector]'), function (connector) {
                connector.parentNode.removeChild(connector);
            });

            var labels = Array.prototype.slice.call(svg.querySelectorAll('text.apexcharts-pie-label'));
            var total = values.reduce(function (sum, value) { return sum + value; }, 0);
            var gridWidth = Number(globals.gridWidth) || element.clientWidth;
            var gridHeight = Number(globals.gridHeight) || element.clientHeight;
            var diameter = Math.min(gridWidth, gridHeight);
            var centerX = gridWidth / 2;
            var centerY = diameter / 2;
            var radius = Number(globals.radialSize) || diameter / 2.05;
            var config = chart.w.config.plotOptions.pie;
            var startAngle = Number(config.startAngle) || 0;
            var fullAngle = Math.abs(Number(config.endAngle) - startAngle) || 360;
            var donutRadius = radius * (parseInt(config.donut.size, 10) || 65) / 100;
            var dataLabelRadius = (radius + donutRadius) / 2 + (Number(config.dataLabels.offset) || 0);
            var sweepAngle = 0;
            var labelIndex = 0;
            var external = [];

            values.forEach(function (value, index) {
                if (value <= 0) return;
                var sliceAngle = total ? value / total * fullAngle : 0;
                var label = labels[labelIndex++];
                if (!label) {
                    sweepAngle += sliceAngle;
                    return;
                }

                var angleDegrees = startAngle + sweepAngle + sliceAngle / 2;
                var angle = (angleDegrees - 90) * Math.PI / 180;
                var xDirection = Math.cos(angle);
                var yDirection = Math.sin(angle);
                label.setAttribute('x', centerX + xDirection * dataLabelRadius);
                label.setAttribute('y', centerY + yDirection * dataLabelRadius);
                label.setAttribute('text-anchor', 'middle');
                label.style.fill = '';
                label.classList.remove('dashboard-pie-external-label');

                if (sliceAngle < smallSliceLabelAngle) {
                    external.push({
                        label: label,
                        color: colors[index] || '#6b7280',
                        xDirection: xDirection,
                        yDirection: yDirection,
                        x: centerX + xDirection * (radius + 20),
                        y: centerY + yDirection * (radius + 20),
                        side: xDirection >= 0 ? 'right' : 'left'
                    });
                }
                sweepAngle += sliceAngle;
            });

            ['left', 'right'].forEach(function (side) {
                var items = external.filter(function (item) { return item.side === side; })
                    .sort(function (left, right) { return left.y - right.y; });
                var minY = 12;
                var maxY = gridHeight - 8;
                var minGap = 16;
                items.forEach(function (item, index) {
                    item.y = Math.max(minY, item.y);
                    if (index > 0) item.y = Math.max(item.y, items[index - 1].y + minGap);
                });
                if (items.length && items[items.length - 1].y > maxY) {
                    var shiftUp = items[items.length - 1].y - maxY;
                    items.forEach(function (item) { item.y -= shiftUp; });
                }
            });

            external.forEach(function (item) {
                var labelWidth = Math.max(14, (item.label.textContent || '').length * 6.5);
                var anchor = item.side === 'right' ? 'start' : 'end';
                var x = item.x;
                var rightLimit = Math.max(gridWidth, element.clientWidth) + 20;
                if (anchor === 'start' && x + labelWidth > rightLimit) {
                    x = rightLimit - labelWidth;
                } else if (anchor === 'end' && x - labelWidth < 6) {
                    x = 6;
                    anchor = 'start';
                }
                var lineEndX = x + (anchor === 'start' ? -3 : 3);
                var startX = centerX + item.xDirection * (radius + 2);
                var startY = centerY + item.yDirection * (radius + 2);
                var bendX = centerX + item.xDirection * (radius + 10);
                var bendY = centerY + item.yDirection * (radius + 10);
                var connector = svg.ownerDocument.createElementNS('http://www.w3.org/2000/svg', 'path');
                connector.setAttribute('data-dashboard-pie-connector', 'true');
                connector.setAttribute('class', 'dashboard-pie-external-connector');
                connector.setAttribute('d', 'M ' + startX + ' ' + startY
                    + ' L ' + bendX + ' ' + bendY
                    + ' L ' + lineEndX + ' ' + item.y);
                connector.setAttribute('fill', 'none');
                connector.setAttribute('stroke', item.color);
                connector.setAttribute('stroke-width', '1.25');
                connector.setAttribute('stroke-linecap', 'round');
                item.label.parentNode.insertBefore(connector, item.label);
                item.label.setAttribute('x', x);
                item.label.setAttribute('y', item.y);
                item.label.setAttribute('text-anchor', anchor);
                item.label.style.fill = '#45546a';
                item.label.classList.add('dashboard-pie-external-label');
            });
        }

        function renderProjectMiniChart(id, labels, colors, unit, isMoney) {
            var element = document.getElementById(id);
            if (!element) return;
            var layout = element.parentNode.parentNode;
            var values = (element.getAttribute('data-values') || '').split(',').map(function (value) {
                return Number(value) || 0;
            });
            var total = values.reduce(function (sum, value) { return sum + value; }, 0);
            var isResourceChart = id === 'overviewAllResourceChart' || id === 'overviewProjectResourceChart';
            var chartHeight = isResourceChart ? 220 : 250;
            var legend = layout.querySelector('.dashboard-project-chart-legend');
            var buttons = legend ? legend.querySelectorAll('button') : [];
            if (!total) {
                layout.classList.add('is-empty');
                element.innerHTML = '<div class="dashboard-project-chart-empty"><i class="bx bx-info-circle" aria-hidden="true"></i><span>'
                    + (isMoney ? 'Chưa có dữ liệu thu tiền' : 'Chưa có dữ liệu') + '</span></div>';
                return;
            }
            if (typeof ApexCharts === 'undefined') {
                layout.classList.add('is-empty');
                element.innerHTML = '<div class="dashboard-project-chart-empty"><i class="bx bx-error-circle" aria-hidden="true"></i><span>Không tải được biểu đồ</span></div>';
                return;
            }
            var chart = new ApexCharts(element, {
                chart: {
                    type: 'donut', height: chartHeight, toolbar: { show: false },
                    events: {
                        dataPointSelection: function (event, context, config) {
                            if (buttons[config.dataPointIndex]) buttons[config.dataPointIndex].click();
                        },
                        mounted: function (context) { scheduleSmallPieLabels(element, context, values, colors); },
                        updated: function (context) { scheduleSmallPieLabels(element, context, values, colors); },
                        resized: function (context) { scheduleSmallPieLabels(element, context, values, colors); }
                    }
                },
                series: values,
                labels: labels,
                colors: colors,
                stroke: { width: 2, colors: ['#fff'] },
                legend: { show: false },
                dataLabels: {
                    enabled: true,
                    formatter: function (percentage, options) {
                        return isResourceChart
                            ? String(values[options.seriesIndex] || 0)
                            : formatChartShare(percentage);
                    },
                    style: { fontSize: '10px', fontWeight: 800, colors: ['#fff'] },
                    dropShadow: { enabled: true, top: 1, left: 0, blur: 2, color: '#1b293e', opacity: .55 }
                },
                tooltip: {
                    y: {
                        formatter: function (value) {
                            return isMoney ? new Intl.NumberFormat('vi-VN').format(value) + ' đ' : value + ' ' + unit;
                        }
                    }
                },
                plotOptions: {
                    pie: {
                        expandOnClick: false,
                        customScale: isResourceChart ? 0.88 : 1,
                        dataLabels: { offset: isResourceChart ? -6 : 0, minAngleToShowLabel: 0 },
                        donut: { size: '64%', labels: { show: false } }
                    }
                },
                states: { active: { filter: { type: 'none' } } }
            });
            chart.render().then(function () {
                scheduleSmallPieLabels(element, chart, values, colors);
            });
        }

        function renderFinanceBreakdownChart(id) {
            var element = document.getElementById(id);
            if (!element) return;
            var layout = element.parentNode;
            var legend = layout.querySelector('.dashboard-finance-chart-legend');
            var buttons = legend ? legend.querySelectorAll('button') : [];
            var values = (element.getAttribute('data-values') || '').split(',').map(function (value) {
                return Number(value) || 0;
            });
            while (values.length < 4) values.push(0);
            var receivedTotal = values[0] + values[1];
            var costTotal = values[2] + values[3];
            var total = receivedTotal + costTotal;
            if (!total) {
                layout.classList.add('is-empty');
                element.innerHTML = '<div class="dashboard-finance-chart-empty"><i class="bx bx-info-circle" aria-hidden="true"></i><span>Chưa có dữ liệu thu chi</span></div>';
                return;
            }
            if (typeof ApexCharts === 'undefined') {
                layout.classList.add('is-empty');
                element.innerHTML = '<div class="dashboard-finance-chart-empty"><i class="bx bx-error-circle" aria-hidden="true"></i><span>Không tải được biểu đồ</span></div>';
                return;
            }

            var shares = [
                receivedTotal ? values[0] / receivedTotal * 100 : 0,
                receivedTotal ? values[1] / receivedTotal * 100 : 0,
                costTotal ? values[2] / costTotal * 100 : 0,
                costTotal ? values[3] / costTotal * 100 : 0
            ];
            var colors = ['#35a875', '#e2a52e', '#518cdd', '#e45d53'];
            var names = ['Đã thu', 'Còn phải thu', 'Chi đã duyệt', 'Chi chờ duyệt'];
            var chart = new ApexCharts(element, {
                chart: {
                    type: 'bar',
                    height: 160,
                    stacked: true,
                    stackType: '100%',
                    toolbar: { show: false },
                    events: {
                        dataPointSelection: function (event, context, config) {
                            if (buttons[config.seriesIndex]) buttons[config.seriesIndex].click();
                        }
                    }
                },
                series: [
                    { name: names[0], data: [shares[0], 0] },
                    { name: names[1], data: [shares[1], 0] },
                    { name: names[2], data: [0, shares[2]] },
                    { name: names[3], data: [0, shares[3]] }
                ],
                colors: colors,
                plotOptions: {
                    bar: { horizontal: true, barHeight: '48%', borderRadius: 4 }
                },
                dataLabels: { enabled: false },
                xaxis: {
                    categories: ['Thu', 'Chi'],
                    min: 0,
                    max: 100,
                    tickAmount: 4,
                    labels: {
                        formatter: function (value) { return Math.round(value) + '%'; },
                        style: { colors: '#64748b', fontSize: '11px' }
                    }
                },
                yaxis: { labels: { style: { colors: '#475569', fontSize: '12px', fontWeight: 600 } } },
                grid: { borderColor: '#e8edf4', strokeDashArray: 3, padding: { left: 4, right: 8, top: -8, bottom: -8 } },
                legend: { show: false },
                tooltip: {
                    shared: false,
                    intersect: true,
                    custom: function (options) {
                        var seriesIndex = options.seriesIndex;
                        var categoryIndex = options.dataPointIndex;
                        var amount = values[seriesIndex] || 0;
                        var share = shares[seriesIndex] || 0;
                        if (!amount || !share) return '';
                        var category = categoryIndex === 0 ? 'Thu' : 'Chi';
                        var money = new Intl.NumberFormat('vi-VN').format(amount) + ' đ';
                        return '<div class="dashboard-finance-tooltip"><strong>' + category + ' · '
                            + names[seriesIndex] + '</strong><span>' + money + '</span><small>'
                            + formatChartShare(share) + ' trong tổng ' + category.toLocaleLowerCase()
                            + '</small></div>';
                    }
                },
                states: { active: { filter: { type: 'none' } } }
            });
            chart.render();
        }

        function renderProjectMiniCharts() {
            var task = document.getElementById('overviewProjectTasksChart');
            if (task) {
                renderProjectMiniChart('overviewProjectTasksChart',
                    ['Hoàn thành', 'Đang làm', 'Chưa bắt đầu'],
                    ['#35a875', '#518cdd', '#8592a6'],
                    'công việc', false);
            }
            var finance = document.getElementById('overviewProjectFinanceChart');
            if (finance) {
                renderFinanceBreakdownChart('overviewProjectFinanceChart');
            }
            var resource = document.getElementById('overviewProjectResourceChart');
            if (resource) {
                renderProjectMiniChart('overviewProjectResourceChart',
                    ['Rảnh', 'Bình thường', 'Quá tải'],
                    ['#35a875', '#efb63e', '#ef6d63'],
                    'nhân sự', false);
            }
            renderProjectMiniChart('overviewAllTasksChart',
                ['Hoàn thành', 'Đang làm', 'Chưa bắt đầu'],
                ['#35a875', '#518cdd', '#8592a6'],
                'công việc', false);
            renderFinanceBreakdownChart('overviewAllFinanceChart');
            renderProjectMiniChart('overviewAllResourceChart',
                ['Rảnh', 'Bình thường', 'Quá tải'],
                ['#35a875', '#efb63e', '#ef6d63'],
                'nhân sự', false);
        }
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', renderProjectMiniCharts);
        } else {
            renderProjectMiniCharts();
        }

        function bindAttentionTasksModal() {
            var modal = document.getElementById('overviewAttentionTasksModal');
            if (!modal) return;
            var input = document.getElementById('overviewAttentionTasksSearch');
            var rows = modal.querySelectorAll('.overview-attention-task-row');
            var projectId = '';
            var overdueDays = 0;
            var projectTitle = document.getElementById('overviewAttentionTasksProject');
            var count = document.getElementById('overviewAttentionTasksCount');
            var empty = document.getElementById('overviewAttentionTasksEmpty');
            var projectLink = document.getElementById('overviewAttentionProjectLink');
            var table = modal.querySelector('.dashboard-attention-task-table');
            function refresh() {
                var query = (input.value || '').trim().toLowerCase();
                var visible = 0;
                var total = 0;
                var taskCount = 0;
                var issueCount = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var belongs = row.getAttribute('data-project-id') === projectId;
                    if (belongs) total++;
                    var show = belongs && (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                    row.classList.toggle('d-none', !show);
                    if (show) {
                        visible++;
                        if (row.getAttribute('data-attention-kind') === 'issue') issueCount++;
                        else taskCount++;
                    }
                });
                count.textContent = taskCount + ' công việc quá hạn · '
                    + issueCount + ' vấn đề đang xử lý';
                empty.textContent = total === 0
                    ? 'Dự án trễ hạn dự kiến ' + overdueDays + ' ngày; không có công việc quá hạn hoặc vấn đề ảnh hưởng cao đang xử lý.'
                    : 'Không có mục phù hợp với từ khóa.';
                empty.classList.toggle('d-none', visible > 0);
                table.classList.toggle('d-none', visible === 0);
            }
            modal.addEventListener('show.bs.modal', function (event) {
                var trigger = event.relatedTarget;
                if (!trigger) return;
                projectId = trigger.getAttribute('data-project-id') || '';
                overdueDays = Number(trigger.getAttribute('data-project-overdue-days')) || 0;
                projectTitle.textContent = trigger.getAttribute('data-project-title') || '';
                projectLink.href = trigger.getAttribute('data-project-url') || '#';
                input.value = '';
                refresh();
            });
            input.addEventListener('input', refresh);
        }

        function bindMeetingModal() {
            var modal = document.getElementById('overviewMeetingModal');
            if (!modal) return;
            modal.addEventListener('show.bs.modal', function (event) {
                var trigger = event.relatedTarget;
                if (!trigger) return;
                var fields = {
                    overviewMeetingTitle: 'title',
                    overviewMeetingProject: 'project',
                    overviewMeetingStart: 'start',
                    overviewMeetingEnd: 'end',
                    overviewMeetingLocation: 'location',
                    overviewMeetingContent: 'content'
                };
                Object.keys(fields).forEach(function (id) {
                    document.getElementById(id).textContent =
                        trigger.getAttribute('data-meeting-' + fields[id]) || 'Chưa có thông tin';
                });
                document.getElementById('overviewMeetingLink').href =
                    trigger.getAttribute('data-meeting-url') || '#';
            });
        }

        bindAttentionTasksModal();
        bindMeetingModal();

        var statusChart;
        function renderStatusChart() {
            var element = document.getElementById('overviewStatusChart');
            if (!element) return;
            var buttons = document.querySelectorAll('.dashboard-status-legend-button');
            var entries = Array.prototype.map.call(buttons, function (button) {
                return {
                    statusCode: Number(button.getAttribute('data-status-code')) || 0,
                    label: button.getAttribute('data-status-label') || '',
                    count: Number(button.getAttribute('data-status-count')) || 0
                };
            });
            var statusColors = { 1: '#368ed8', 2: '#e6a32e', 3: '#37aa80', 4: '#9066c4' };
            var total = entries.reduce(function (sum, entry) { return sum + entry.count; }, 0);
            if (!total) {
                element.innerHTML = '<p class="text-muted text-center py-5 mb-0">Chưa có dự án để thống kê.</p>';
                return;
            }
            if (typeof ApexCharts === 'undefined') {
                element.innerHTML = '<p class="text-muted text-center py-5 mb-0">Không tải được biểu đồ.</p>';
                return;
            }
            statusChart = new ApexCharts(element, {
                chart: {
                    type: 'donut', height: 270, toolbar: { show: false },
                    events: {
                        dataPointSelection: function (event, chartContext, config) {
                            var button = buttons[config.dataPointIndex];
                            if (button) button.click();
                        },
                        mounted: function (context) { scheduleSmallPieLabels(element, context, entries.map(function (entry) { return entry.count; }), entries.map(function (entry) { return statusColors[entry.statusCode] || '#7896b6'; })); },
                        updated: function (context) { scheduleSmallPieLabels(element, context, entries.map(function (entry) { return entry.count; }), entries.map(function (entry) { return statusColors[entry.statusCode] || '#7896b6'; })); },
                        resized: function (context) { scheduleSmallPieLabels(element, context, entries.map(function (entry) { return entry.count; }), entries.map(function (entry) { return statusColors[entry.statusCode] || '#7896b6'; })); }
                    }
                },
                labels: entries.map(function (entry) { return entry.label; }),
                series: entries.map(function (entry) { return entry.count; }),
                colors: entries.map(function (entry) { return statusColors[entry.statusCode] || '#7896b6'; }),
                stroke: { width: 2, colors: ['#fff'] },
                legend: { show: false },
                dataLabels: {
                    enabled: true,
                    style: { colors: ['#fff'], fontSize: '11px', fontWeight: 800 },
                    dropShadow: { enabled: true, top: 1, left: 0, blur: 2, color: '#1b293e', opacity: .55 },
                    formatter: function (value) { return formatChartShare(value); }
                },
                tooltip: { y: { formatter: function (value) { return value + ' dự án'; } } },
                plotOptions: {
                    pie: {
                        expandOnClick: false,
                        dataLabels: { minAngleToShowLabel: 0 },
                        donut: { size: '64%', labels: { show: false } }
                    }
                },
                states: { active: { filter: { type: 'none' } } }
            });
            statusChart.render().then(function () {
                scheduleSmallPieLabels(element, statusChart,
                    entries.map(function (entry) { return entry.count; }),
                    entries.map(function (entry) { return statusColors[entry.statusCode] || '#7896b6'; }));
            });
        }
        var projectModal = document.getElementById('overviewProjectsModal');
        if (projectModal) {
            projectModal.addEventListener('hidden.bs.modal', function () {
                if (!statusChart) return;
                statusChart.destroy();
                statusChart = null;
                document.getElementById('overviewStatusChart').innerHTML = '';
                renderStatusChart();
            });
        }
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', renderStatusChart);
        } else {
            renderStatusChart();
        }
    })();
</script>
