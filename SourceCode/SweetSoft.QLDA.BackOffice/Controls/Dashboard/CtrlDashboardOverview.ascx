<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDashboardOverview.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardOverview" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

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
            <label class="form-label mb-1" for="<%= ddlProjectFilter.ClientID %>">Phạm vi dự án</label>
            <asp:DropDownList ID="ddlProjectFilter" runat="server"
                CssClass="form-select dashboard-overview-project-filter"
                AutoPostBack="true"
                OnSelectedIndexChanged="ddlProjectFilter_SelectedIndexChanged" />
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

    <%-- TẠM THỜI: Chú thích cách hiểu số liệu để nhóm kiểm tra; xóa cả khối này khi đã chốt nội dung. --%>
    <details class="alert alert-light border mb-3" open>
        <summary class="fw-semibold" style="cursor: pointer">Chú thích số liệu (tạm thời)</summary>
        <ul class="small mb-0 mt-2 ps-3">
            <% if (!IsProjectDashboard) { %>
            <li><strong>Dự án quá hạn:</strong> chưa hoàn thành hoặc kết thúc và đã qua ngày dự kiến hoàn thành; không phải một trạng thái dự án riêng.</li>
            <li><strong>Sắp đến hạn (7 ngày):</strong> dự án đang thực hiện, có hạn dự kiến từ hôm nay đến hết 7 ngày tới.</li>
            <li><strong>Khoản chi chờ duyệt:</strong> đếm các khoản chi chưa được duyệt, không phải số tiền đã chi.</li>
            <li><strong>Nhân sự quá tải theo lịch:</strong> đếm người được giao số ngày công dự kiến vượt số ngày làm việc của tuần này; không phải giờ làm thực tế.</li>
            <li><strong>Dự án cần xử lý:</strong> dự án quá hạn, có công việc quá hạn hoặc có vấn đề ảnh hưởng cao. Một dự án chỉ được đếm một lần.</li>
            <% } else { %>
            <li><strong>Công việc quá hạn:</strong> chưa hoàn thành và đã qua ngày kết thúc dự kiến. <strong>Đến hạn trong 7 ngày:</strong> chưa hoàn thành, hạn từ hôm nay đến hết 7 ngày tới.</li>
            <li><strong>Chi phí chờ duyệt:</strong> tổng tiền của các khoản chi chưa được duyệt; chưa tính vào “Chi phí đã duyệt”.</li>
            <li><strong>Còn phải thu:</strong> giá trị hợp đồng trừ tiền đã thu; nếu chưa có giá trị hợp đồng, dùng tổng các đợt thanh toán chưa thu.</li>
            <li><strong>Mức tải nhân sự:</strong> số ngày công được giao chia cho số ngày làm việc trong tuần này; trên 100% là quá tải theo lịch, không phải giờ làm thực tế.</li>
            <% } %>
        </ul>
    </details>

    <% if (!IsProjectDashboard) { %>
    <div class="row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-xl-5 g-3 mb-3 dashboard-overview-kpis">
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
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="overdue" data-overview-title="Dự án quá hạn">
                <span class="card-body"><span class="text-muted d-block">Dự án quá hạn</span>
                    <strong class="fs-3 text-danger"><%= OverdueProjects.Count %></strong>
                    <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-error-circle"></i></span></span>
            </button>
        </div>
        <div class="col">
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="due-soon" data-overview-title="Dự án sắp đến hạn">
                <span class="card-body"><span class="text-muted d-block">Sắp đến hạn (7 ngày)</span>
                    <strong class="fs-3 dashboard-days-left"><%= UpcomingProjects.Count %></strong>
                    <span class="dashboard-kpi-icon bg-warning-subtle dashboard-days-left"><i class="bx bx-calendar-event"></i></span></span>
            </button>
        </div>
        <div class="col">
            <% if (ShowFinanceSignal) { %>
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewPendingCostsModal">
                <span class="card-body"><span class="text-muted d-block">Khoản chi chờ duyệt</span>
                    <strong class="fs-3 dashboard-attention-text"><%= Summary.PendingCosts.Count %></strong>
                    <span class="dashboard-kpi-icon bg-warning-subtle dashboard-attention-text"><i class="bx bx-receipt"></i></span></span>
            </button>
            <% } else { %>
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="overdue-task" data-overview-title="Dự án có công việc quá hạn">
                <span class="card-body"><span class="text-muted d-block">Có công việc quá hạn</span>
                    <strong class="fs-3 dashboard-attention-text"><%= ProjectsWithOverdueTasks.Count %></strong>
                    <span class="dashboard-kpi-icon bg-warning-subtle dashboard-attention-text"><i class="bx bx-task"></i></span></span>
            </button>
            <% } %>
        </div>
        <div class="col">
            <% if (ShowResourceSignal) { %>
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewOverloadedEmployeesModal">
                <span class="card-body"><span class="text-muted d-block">Nhân sự quá tải theo lịch</span>
                    <strong class="fs-3 text-danger"><%= OverloadedEmployees.Count %></strong>
                    <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-group"></i></span></span>
            </button>
            <% } else { %>
            <button type="button" class="card border-0 shadow-sm h-100 w-100 text-start dashboard-simple-kpi"
                data-bs-toggle="modal" data-bs-target="#overviewProjectsModal" data-overview-filter="status-1" data-overview-title="Dự án đang thực hiện">
                <span class="card-body"><span class="text-muted d-block">Đang thực hiện</span>
                    <strong class="fs-3 text-primary"><%= ActiveProjectCount %></strong>
                    <span class="dashboard-kpi-icon bg-info-subtle text-info"><i class="bx bx-play-circle"></i></span></span>
            </button>
            <% } %>
        </div>
    </div>

    <div class="row g-3 mb-3 dashboard-overview-three-up">
        <div class="col-12 col-xl-5">
            <div class="card border-0 shadow-sm h-100">
                <div class="card-header"><h5 class="dashboard-overview-section-title"><i class="bx bx-pie-chart-alt-2" aria-hidden="true"></i> Trạng thái dự án</h5></div>
                <div class="card-body">
                    <div id="overviewStatusChart" class="dashboard-overview-status-chart"
                        aria-label="Biểu đồ số dự án theo trạng thái"></div>
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
                            <strong><%= status.Count %></strong>
                        </button>
                        <% } %>
                    </div>
                    <p class="small text-muted mb-0 mt-2">Chọn một phần biểu đồ để xem danh sách dự án. Quá hạn được theo dõi riêng.</p>
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
                                <% if (project.IsOverdue) { %><span class="badge bg-danger-subtle text-danger me-1">Quá hạn <%= project.OverdueDays %> ngày</span><% } %>
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

    <% if (ShowIssueSignal || ShowRiskSignal) { %>
    <div class="row g-3 mb-3 dashboard-overview-signal-row">
        <% if (ShowIssueSignal) { %>
        <div class="col-12 <%= ShowRiskSignal ? "col-lg-6" : "col-lg-12" %>">
            <button type="button" class="card h-100 w-100 text-start dashboard-project-issue-card dashboard-overview-signal-card"
                data-bs-toggle="modal" data-bs-target="#overviewAllIssuesModal">
                <span class="card-body d-flex align-items-center justify-content-between gap-3">
                    <span><strong class="d-flex align-items-center gap-2"><i class="bx bx-error-circle" aria-hidden="true"></i> Vấn đề đang xử lý</strong><small class="text-muted">Trong tất cả dự án · Bấm để xem danh sách</small></span>
                    <strong class="dashboard-project-issue-count"><%= Summary.OpenIssues.Count %></strong>
                </span>
            </button>
        </div>
        <% } %>
        <% if (ShowRiskSignal) { %>
        <div class="col-12 <%= ShowIssueSignal ? "col-lg-6" : "col-lg-12" %>">
            <button type="button" class="card h-100 w-100 text-start dashboard-project-issue-card dashboard-project-risk-card dashboard-overview-signal-card"
                data-bs-toggle="modal" data-bs-target="#overviewRisksModal">
                <span class="card-body d-flex align-items-center justify-content-between gap-3">
                    <span><strong class="d-flex align-items-center gap-2"><i class="bx bx-shield" aria-hidden="true"></i> Rủi ro đã ghi nhận</strong><small class="text-muted">Trong tất cả dự án · Bấm để xem danh sách</small></span>
                    <strong class="dashboard-project-issue-count dashboard-project-risk-count"><%= Summary.RecordedRisks.Count %></strong>
                </span>
            </button>
        </div>
        <% } %>
    </div>
    <% } %>

    <div class="row g-3 mb-3 dashboard-project-glance-row dashboard-all-projects-glance-row">
        <div class="col-12 <%= ShowFinanceSignal && ShowResourceSignal ? "col-lg-4" : (ShowFinanceSignal || ShowResourceSignal ? "col-lg-6" : "col-lg-12") %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-progress">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-task"></i> Tiến độ công việc</h5>
                    <p class="dashboard-project-glance-caption"><%= AllTasksCompletedCount %>/<%= Summary.Tasks.Count %> công việc hoàn thành · Tất cả dự án</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewAllTasksChart" class="dashboard-project-chart-canvas"
                                data-values="<%= AllTasksCompletedCount %>,<%= AllTasksInProgressCount %>,<%= AllTasksNotStartedCount %>,<%= AllTasksOverdueCount %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= Summary.Tasks.Count %></strong><small>công việc</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="completed"><i class="dashboard-chart-dot" style="background:#35a875"></i>Hoàn thành <strong><%= AllTasksCompletedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="in-progress"><i class="dashboard-chart-dot" style="background:#518cdd"></i>Đang làm <strong><%= AllTasksInProgressCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="not-started"><i class="dashboard-chart-dot" style="background:#9da9bb"></i>Chưa bắt đầu <strong><%= AllTasksNotStartedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllTasksModal" data-task-filter="overdue"><i class="dashboard-chart-dot" style="background:#ef6b60"></i>Quá hạn <strong><%= AllTasksOverdueCount %></strong></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% if (ShowFinanceSignal && AllProjectsCostSummary != null) { %>
        <div class="col-12 <%= ShowResourceSignal ? "col-lg-4" : "col-lg-6" %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-cost">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-money"></i> Thu tiền và chi phí <small>lũy kế</small></h5>
                    <p class="dashboard-project-glance-caption"><%= AllFinanceProjectsWithAmountsCount %>/<%= AllProjectsCostSummary.ProjectCount %> dự án có khoản thu hoặc phải thu</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewAllFinanceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= AllProjectsCostSummary.ReceivedPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= AllProjectsCostSummary.OutstandingPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= GetCollectionRate(AllProjectsCostSummary) %>%</strong><small>đã thu</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="received"><i class="dashboard-chart-dot" style="background:#35a875"></i>Đã thu <strong><%: FormatProjectMoney(AllProjectsCostSummary.ReceivedPayment) %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="outstanding"><i class="dashboard-chart-dot" style="background:#f2b84b"></i>Còn phải thu <strong><%: FormatProjectMoney(AllProjectsCostSummary.OutstandingPayment) %></strong></button>
                        </div>
                    </div>
                    <button type="button" class="dashboard-project-secondary-line dashboard-project-secondary-action" data-bs-toggle="modal" data-bs-target="#overviewAllFinanceModal" data-finance-filter="cost"><span>Chi phí đã duyệt</span><strong><%: FormatProjectMoney(AllProjectsCostSummary.ActualCost) %></strong></button>
                </div>
            </div>
        </div>
        <% } %>
        <% if (ShowResourceSignal && AllProjectsResourceSummary != null) { %>
        <div class="col-12 <%= ShowFinanceSignal ? "col-lg-4" : "col-lg-6" %>">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-resource">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-group"></i> Mức tải nhân sự <small>tuần này</small></h5>
                    <p class="dashboard-project-glance-caption"><%= AllProjectsResourceSummary.TotalEmployeeCount %> nhân sự trong hệ thống</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewAllResourceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= AllResourceNoLoadCount %>,<%= AllResourceUnderloadedCount %>,<%= AllResourceBalancedCount %>,<%= AllResourceOverloadedCount %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= AllProjectsResourceSummary.TotalEmployeeCount %></strong><small>nhân sự</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="0"><i class="dashboard-chart-dot" style="background:#aab6c7"></i>Không tải <strong><%= AllResourceNoLoadCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="1"><i class="dashboard-chart-dot" style="background:#52b6aa"></i>Thiếu tải <strong><%= AllResourceUnderloadedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="2"><i class="dashboard-chart-dot" style="background:#f2b84b"></i>Đủ tải <strong><%= AllResourceBalancedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewAllResourceModal" data-load-filter="3"><i class="dashboard-chart-dot" style="background:#ef6b60"></i>Quá tải <strong><%= AllResourceOverloadedCount %></strong></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
    </div>

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
    <% if (ShowResourceSignal) { %>
    <div class="modal fade" id="overviewOverloadedEmployeesModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title">Nhân sự quá tải theo lịch · <%= ResourceWeekStart.ToString("dd/MM") %>–<%= ResourceWeekStart.AddDays(6).ToString("dd/MM/yyyy") %></h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <p class="small text-muted">Mức tải dự kiến theo lịch giao việc, không phải giờ làm thực tế.</p>
                <input type="search" class="form-control mb-3 dashboard-list-search" data-overview-signal-search="overloaded-employee"
                    placeholder="Tìm nhân sự" aria-label="Tìm nhân sự quá tải" />
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table">
                    <thead><tr><th>Nhân sự</th><th class="text-end">Mức tải dự kiến</th><th class="text-end">Ngày công giao / ngày làm việc</th><th>Chi tiết</th></tr></thead>
                    <tbody>
                    <% foreach (var employee in OverloadedEmployees) { %>
                    <tr data-overview-signal-row="overloaded-employee">
                        <td><strong><%: employee.DisplayName %></strong><br /><small><%: employee.DepartmentName %></small></td>
                        <td class="text-end text-danger fw-semibold"><%= employee.AverageUtilization.ToString("0.#") %>%</td>
                        <td class="text-end"><%= employee.AllocatedDays.ToString("0.#") %> / <%= employee.CapacityDays.ToString("0.#") %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetEmployeeResourceUrl(employee.EmployeeId) %>">Xem phân bổ tuần</a></td>
                    </tr>
                    <% } %>
                    </tbody>
                </table></div>
                <p class="text-muted mb-0 <%= OverloadedEmployees.Count == 0 ? string.Empty : "d-none" %>" data-overview-signal-empty="overloaded-employee">Không có nhân sự phù hợp.</p>
            </div>
            <div class="modal-footer"><a class="btn btn-outline-primary btn-sm" href="<%: GetResourceDashboardUrl() %>">Xem dashboard nguồn lực</a></div>
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
                    <% foreach (var task in Summary.Tasks.Where(t => t.IsOverdue)) { %>
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
                <p class="text-muted mb-2 d-none" id="overviewAttentionTasksEmpty">Dự án quá hạn dự kiến.</p>
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
                    <tr class="overview-all-task-row" data-task-state="<%= task.StatusCode %>"
                        data-group-project-id="<%= task.ProjectId %>" data-group-project-code="<%: task.ProjectCode %>" data-group-project-name="<%: task.ProjectName %>"
                        data-search="<%: (task.ProjectCode + " " + task.ProjectName + " " + task.TaskCode + " " + task.TaskName).ToLowerInvariant() %>">
                        <td><strong><%: task.TaskCode %></strong><br /><small><%: task.TaskName %></small></td>
                        <td><%: task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "—" %></td>
                        <td><%: task.Status %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetTaskDetailUrl(task) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody>
                </table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewAllTasksEmpty">Không có công việc phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% if (ShowFinanceSignal && AllProjectsCostSummary != null) { %>
    <div class="modal fade" id="overviewAllFinanceModal" tabindex="-1" aria-labelledby="overviewAllFinanceTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewAllFinanceTitle">Thu tiền và chi phí theo dự án</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <p class="small text-muted mb-2">Một dự án đã thu một phần vẫn có thể còn phải thu, nên có thể xuất hiện ở cả hai danh sách.</p>
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllFinanceSearch" placeholder="Tìm dự án" aria-label="Tìm dự án" />
                    <strong class="dashboard-list-count" id="overviewAllFinanceCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Dự án</th><th class="text-end">Đã thu</th><th class="text-end">Còn phải thu</th><th class="text-end">Chi phí đã duyệt</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var project in GetAllProjectsFinanceRows()) { %>
                    <tr class="overview-all-finance-row" data-received="<%= project.ReceivedPayment > 0 ? "1" : "0" %>"
                        data-outstanding="<%= project.OutstandingPayment > 0 ? "1" : "0" %>" data-cost="<%= project.ActualCost > 0 ? "1" : "0" %>"
                        data-search="<%: (project.ProjectCode + " " + project.ProjectName).ToLowerInvariant() %>">
                        <td><strong><%: project.ProjectCode %></strong><br /><small><%: project.ProjectName %></small></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.ReceivedPayment) %></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.OutstandingPayment) %></td>
                        <td class="text-end text-nowrap"><%: FormatProjectMoney(project.ActualCost) %></td>
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
            <div class="modal-header"><h5 class="modal-title" id="overviewAllResourceTitle">Mức tải nhân sự tuần này</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <p class="small text-muted">Một nhân sự được tính một lần dù tham gia nhiều dự án. Mức tải dự kiến theo lịch giao việc.</p>
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllResourceSearch" placeholder="Tìm nhân sự hoặc dự án" aria-label="Tìm nhân sự hoặc dự án" />
                    <strong class="dashboard-list-count" id="overviewAllResourceCount"></strong>
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Nhân sự</th><th>Dự án có việc tuần này</th><th class="text-end">Mức tải</th><th class="text-end">Ngày giao / ngày làm việc</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var employee in AllProjectsResourceSummary.EmployeeLoads) { var week = GetAllProjectsWeekLoad(employee); var loadCode = GetAllProjectsWeekLoadCode(employee); var projectNames = GetAllProjectsWeekProjectNames(employee); %>
                    <tr class="overview-all-resource-row" data-load="<%= loadCode %>"
                        data-search="<%: (employee.DisplayName + " " + employee.UserName + " " + projectNames).ToLowerInvariant() %>">
                        <td><strong><%: employee.DisplayName %></strong></td>
                        <td><%: string.IsNullOrEmpty(projectNames) ? "Chưa có việc trong tuần" : projectNames %></td>
                        <td class="text-end text-nowrap"><%: GetProjectWeekLoadText(loadCode) %> · <%= week == null ? "0" : week.AllocationPercent.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %>%</td>
                        <td class="text-end text-nowrap"><%= week == null ? "0" : week.AllocatedDays.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %> / <%= week == null ? "0" : week.CapacityDays.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %> ngày</td>
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
        <span class="badge bg-danger-subtle text-danger">Dự án quá hạn <%= SelectedProject.OverdueDays %> ngày</span>
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
        <% if (ShowProjectCostSummary && ProjectCostSummary != null) { %>
        <div class="col"><button type="button" class="card w-100 h-100 text-start dashboard-simple-kpi dashboard-project-signal"
            data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="pending">
            <span class="card-body"><span class="text-muted d-block">Chi phí chờ duyệt</span><strong class="fs-3 dashboard-attention-text"><%: FormatProjectMoney(ProjectCostSummary.PendingApprovalCost) %></strong>
                <span class="dashboard-kpi-icon bg-warning-subtle dashboard-attention-text"><i class="bx bx-receipt"></i></span></span>
        </button></div>
        <% } %>
        <% if (ShowProjectResourceSummary && ProjectResourceSummary != null) { %>
        <div class="col"><button type="button" class="card w-100 h-100 text-start dashboard-simple-kpi dashboard-project-signal"
            data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="3">
            <span class="card-body"><span class="text-muted d-block">Nhân sự quá tải tuần này</span><strong class="fs-3 <%= ProjectResourceOverloadedCount > 0 ? "text-danger" : "text-dark" %>"><%= ProjectResourceOverloadedCount %></strong>
                <span class="dashboard-kpi-icon bg-danger-subtle text-danger"><i class="bx bx-group"></i></span></span>
        </button></div>
        <% } %>
    </div>
    <div class="row g-3 mb-3 dashboard-project-glance-row">
        <div class="col-12 col-lg-4">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-progress">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-task"></i> Tiến độ công việc</h5>
                    <p class="dashboard-project-glance-caption"><%= CompletedProjectTasks.Count %>/<%= SelectedProject.TaskCount %> công việc hoàn thành</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewProjectTasksChart" class="dashboard-project-chart-canvas"
                                data-values="<%= ProjectTasksCompletedCount %>,<%= ProjectTasksInProgressCount %>,<%= ProjectTasksNotStartedCount %>,<%= ProjectTasksOverdueCount %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= SelectedProject.TaskCount %></strong><small>công việc</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="completed" data-task-title="Công việc hoàn thành"><i class="dashboard-chart-dot" style="background:#35a875"></i>Hoàn thành <strong><%= ProjectTasksCompletedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="in-progress" data-task-title="Công việc đang thực hiện"><i class="dashboard-chart-dot" style="background:#518cdd"></i>Đang làm <strong><%= ProjectTasksInProgressCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="not-started" data-task-title="Công việc chưa bắt đầu"><i class="dashboard-chart-dot" style="background:#9da9bb"></i>Chưa bắt đầu <strong><%= ProjectTasksNotStartedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewTasksModal" data-task-filter="overdue" data-task-title="Công việc quá hạn"><i class="dashboard-chart-dot" style="background:#ef6b60"></i>Quá hạn <strong><%= ProjectTasksOverdueCount %></strong></button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <% if (ShowProjectCostSummary && ProjectCostSummary != null) { %>
        <div class="col-12 col-lg-4">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-cost">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-money"></i> Thu tiền và chi phí <small>lũy kế</small></h5>
                    <p class="dashboard-project-glance-caption"><%= ProjectCostSummary.TotalContractValue > 0 ? "Hợp đồng: " + FormatProjectMoney(ProjectCostSummary.TotalContractValue) : "Chưa ghi nhận giá trị hợp đồng" %></p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewProjectFinanceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= ProjectCostSummary.ReceivedPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>,<%= ProjectCostSummary.OutstandingPayment.ToString(System.Globalization.CultureInfo.InvariantCulture) %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= GetProjectCollectionRate() %>%</strong><small>đã thu</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="all"><i class="dashboard-chart-dot" style="background:#35a875"></i>Đã thu <strong><%: FormatProjectMoney(ProjectCostSummary.ReceivedPayment) %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectFinanceModal" data-finance-filter="all"><i class="dashboard-chart-dot" style="background:#f2b84b"></i>Còn phải thu <strong><%: FormatProjectMoney(ProjectCostSummary.OutstandingPayment) %></strong></button>
                        </div>
                    </div>
                    <div class="dashboard-project-secondary-line"><span>Chi phí đã duyệt</span><strong><%: FormatProjectMoney(ProjectCostSummary.ActualCost) %></strong></div>
                </div>
            </div>
        </div>
        <% } %>
        <% if (ShowProjectResourceSummary && ProjectResourceSummary != null) { %>
        <div class="col-12 col-lg-4">
            <div class="card h-100 dashboard-project-glance dashboard-project-glance-resource">
                <div class="card-body">
                    <h5 class="dashboard-project-glance-title"><i class="bx bx-group"></i> Mức tải nhân sự <small>tuần này</small></h5>
                    <p class="dashboard-project-glance-caption"><%= ProjectResourceSummary.TotalEmployeeCount %> nhân sự thuộc dự án</p>
                    <div class="dashboard-project-chart-layout">
                        <div class="dashboard-project-mini-chart">
                            <div id="overviewProjectResourceChart" class="dashboard-project-chart-canvas"
                                data-values="<%= ProjectResourceNoLoadCount %>,<%= ProjectResourceUnderloadedCount %>,<%= ProjectResourceBalancedCount %>,<%= ProjectResourceOverloadedCount %>"></div>
                            <span class="dashboard-project-chart-center"><strong><%= ProjectResourceSummary.TotalEmployeeCount %></strong><small>nhân sự</small></span>
                        </div>
                        <div class="dashboard-project-chart-legend">
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="0"><i class="dashboard-chart-dot" style="background:#aab6c7"></i>Không tải <strong><%= ProjectResourceNoLoadCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="1"><i class="dashboard-chart-dot" style="background:#52b6aa"></i>Thiếu tải <strong><%= ProjectResourceUnderloadedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="2"><i class="dashboard-chart-dot" style="background:#f2b84b"></i>Đủ tải <strong><%= ProjectResourceBalancedCount %></strong></button>
                            <button type="button" data-bs-toggle="modal" data-bs-target="#overviewProjectResourceModal" data-load-filter="3"><i class="dashboard-chart-dot" style="background:#ef6b60"></i>Quá tải <strong><%= ProjectResourceOverloadedCount %></strong></button>
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
            <button type="button" class="card h-100 w-100 text-start dashboard-project-issue-card"
                data-bs-toggle="modal" data-bs-target="#overviewOpenIssuesModal">
                <span class="card-body d-flex align-items-center justify-content-between gap-3">
                    <span><strong class="d-flex align-items-center gap-2"><i class="bx bx-error-circle" aria-hidden="true"></i> Vấn đề đang xử lý</strong><small class="text-muted">Bấm để xem danh sách</small></span>
                    <strong class="dashboard-project-issue-count"><%= Summary.OpenIssues.Count(i => i.ProjectId == SelectedProject.ProjectId) %></strong>
                </span>
            </button>
        </div>
        <% } %>
        <% if (ShowRiskSignal) { %>
        <div class="col-12 <%= ShowIssueSignal ? "col-lg-4" : "col-lg-6" %>">
            <button type="button" class="card h-100 w-100 text-start dashboard-project-issue-card dashboard-project-risk-card"
                data-bs-toggle="modal" data-bs-target="#overviewRisksModal">
                <span class="card-body d-flex align-items-center justify-content-between gap-3">
                    <span><strong class="d-flex align-items-center gap-2"><i class="bx bx-shield" aria-hidden="true"></i> Rủi ro đã ghi nhận</strong><small class="text-muted">Bấm để xem danh sách</small></span>
                    <strong class="dashboard-project-issue-count dashboard-project-risk-count"><%= Summary.RecordedRisks.Count(r => r.ProjectId == SelectedProject.ProjectId) %></strong>
                </span>
            </button>
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
                        data-in-progress="<%= task.StatusCode == 1 ? "1" : "0" %>"
                        data-not-started="<%= task.StatusCode == 0 ? "1" : "0" %>"
                        data-due-soon="<%= DueSoonProjectTasks.Contains(task) ? "1" : "0" %>"
                        data-search="<%: (task.TaskCode + " " + task.TaskName).ToLowerInvariant() %>">
                        <td><strong><%: task.TaskCode %></strong><br /><small><%: task.TaskName %></small></td>
                        <td><%: task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM/yyyy") : "—" %></td>
                        <td><%: task.Status %></td>
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
            <div class="modal-header"><h5 class="modal-title" id="overviewProjectResourceTitle">Mức tải nhân sự tuần này</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
                    <strong class="dashboard-list-count" id="overviewProjectResourceCount"></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewProjectResourceSearch" placeholder="Tìm nhân sự" aria-label="Tìm nhân sự" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0"><thead>
                    <tr><th>Nhân sự</th><th>Mức tải</th><th>Ngày giao / ngày làm việc</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var employee in ProjectResourceSummary.EmployeeLoads) { var week = GetProjectWeekLoad(employee); var loadCode = GetProjectWeekLoadCode(employee); %>
                    <tr class="overview-project-resource-row" data-load="<%= loadCode %>" data-search="<%: (employee.DisplayName + " " + employee.UserName).ToLowerInvariant() %>">
                        <td><strong><%: employee.DisplayName %></strong></td>
                        <td><%: GetProjectWeekLoadText(loadCode) %> · <%= week == null ? "0" : week.AllocationPercent.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %>%</td>
                        <td><%= week == null ? "0" : week.AllocatedDays.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %> / <%= week == null ? "0" : week.CapacityDays.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) %> ngày</td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectResourceEmployeeUrl(employee.EmployeeId) %>">Xem chi tiết</a></td>
                    </tr><% } %>
                </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewProjectResourceEmpty">Không có nhân sự phù hợp.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% if (ShowIssueSignal) { %>
    <div class="modal fade" id="overviewOpenIssuesModal" tabindex="-1" aria-labelledby="overviewOpenIssuesModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewOpenIssuesModalTitle">Vấn đề đang xử lý</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <strong class="dashboard-list-count" id="overviewOpenIssuesCount"></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewOpenIssuesSearch"
                        placeholder="Tìm mã hoặc tên vấn đề" aria-label="Tìm vấn đề" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Vấn đề</th><th>Mức ảnh hưởng</th><th>Hướng xử lý</th><th>Chi tiết</th></tr></thead><tbody>
                    <% foreach (var issue in Summary.OpenIssues.Where(i => i.ProjectId == SelectedProject.ProjectId)) { %>
                    <tr class="overview-open-issue-row" data-search="<%: (issue.IssueCode + " " + issue.IssueName + " " + issue.HandlingPlan).ToLowerInvariant() %>">
                        <td><strong><%: issue.IssueCode %></strong><br /><small><%: issue.IssueName %></small></td>
                        <td><%: GetIssueImpactText(issue.ImpactLevel) %></td>
                        <td><%: string.IsNullOrWhiteSpace(issue.HandlingPlan) ? "—" : issue.HandlingPlan %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectIssueDetailUrl(issue.ProjectId, issue.IssueId) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewOpenIssuesEmpty">Không có vấn đề đang xử lý.</p>
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
    <% if (!IsProjectDashboard && ShowIssueSignal) { %>
    <div class="modal fade" id="overviewAllIssuesModal" tabindex="-1" aria-labelledby="overviewAllIssuesModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewAllIssuesModalTitle">Vấn đề đang xử lý</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <strong class="dashboard-list-count" id="overviewAllIssuesCount"></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewAllIssuesSearch"
                        placeholder="Tìm dự án, mã hoặc tên vấn đề" aria-label="Tìm vấn đề" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Dự án</th><th>Vấn đề</th><th>Mức ảnh hưởng</th><th>Hướng xử lý</th><th>Chi tiết</th></tr></thead>
                    <tbody id="overviewAllIssuesBody">
                    <% foreach (var issue in Summary.OpenIssues) { %>
                    <tr class="overview-all-issue-row" data-group-project-id="<%= issue.ProjectId %>"
                        data-group-project-code="<%: issue.ProjectCode %>" data-group-project-name="<%: issue.ProjectName %>"
                        data-search="<%: (issue.ProjectCode + " " + issue.ProjectName + " " + issue.IssueCode + " " + issue.IssueName + " " + issue.HandlingPlan).ToLowerInvariant() %>">
                        <td><strong><%: issue.ProjectCode %></strong><br /><small><%: issue.ProjectName %></small></td>
                        <td><strong><%: issue.IssueCode %></strong><br /><small><%: issue.IssueName %></small></td>
                        <td><%: GetIssueImpactText(issue.ImpactLevel) %></td>
                        <td><%: string.IsNullOrWhiteSpace(issue.HandlingPlan) ? "—" : issue.HandlingPlan %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectIssueDetailUrl(issue.ProjectId, issue.IssueId) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewAllIssuesEmpty">Không có vấn đề đang xử lý.</p>
            </div>
        </div></div>
    </div>
    <% } %>
    <% if (ShowRiskSignal) { %>
    <div class="modal fade" id="overviewRisksModal" tabindex="-1" aria-labelledby="overviewRisksModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title" id="overviewRisksModalTitle">Rủi ro đã ghi nhận</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
            <div class="modal-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <strong class="dashboard-list-count" id="overviewRisksCount"></strong>
                    <input type="search" class="form-control dashboard-attention-search" id="overviewRisksSearch"
                        placeholder="Tìm dự án hoặc tên rủi ro" aria-label="Tìm rủi ro" />
                </div>
                <div class="table-responsive"><table class="table table-bordered table-hover align-middle dashboard-overview-table mb-0">
                    <thead><tr><th>Dự án</th><th>Rủi ro</th><th>Xác suất xảy ra</th><th>Mức ảnh hưởng</th><th>Phòng ngừa / ứng phó</th><th>Chi tiết</th></tr></thead>
                    <tbody id="overviewRisksBody">
                    <% foreach (var risk in Summary.RecordedRisks) { %>
                    <tr class="overview-recorded-risk-row" data-group-project-id="<%= risk.ProjectId %>"
                        data-group-project-code="<%: risk.ProjectCode %>" data-group-project-name="<%: risk.ProjectName %>"
                        data-search="<%: (risk.ProjectCode + " " + risk.ProjectName + " " + risk.RiskName + " " + risk.PreventionPlan + " " + risk.ResponsePlan).ToLowerInvariant() %>">
                        <td><strong><%: risk.ProjectCode %></strong><br /><small><%: risk.ProjectName %></small></td>
                        <td><%: risk.RiskName %></td>
                        <td><%: GetRiskProbabilityText(risk.Probability) %></td>
                        <td><%: risk.ImpactLevel.HasValue ? GetIssueImpactText(risk.ImpactLevel.Value) : "—" %></td>
                        <td><%: GetRiskPlanText(risk) %></td>
                        <td><a class="btn btn-outline-primary btn-sm text-nowrap" href="<%: GetProjectRiskDetailUrl(risk.ProjectId, risk.RiskId) %>">Xem chi tiết</a></td>
                    </tr>
                    <% } %>
                    </tbody></table></div>
                <p class="text-muted mb-0 py-3 d-none" id="overviewRisksEmpty">Chưa có rủi ro được ghi nhận.</p>
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
                if (row.getAttribute('data-overdue') === '1') reasons.push('Dự án quá hạn ' + overdueDays + ' ngày');
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
                        ? 'Dự án quá hạn hoặc có công việc quá hạn.'
                        : filter === 'due-soon'
                            ? 'Dự án đang thực hiện, đến hạn trong 7 ngày tới.'
                            : filter.indexOf('status-') === 0
                                ? 'Các dự án thuộc trạng thái đã chọn.'
                                : 'Toàn bộ dự án trong phạm vi hiện tại.';
            contextHeader.textContent = filter === 'overdue' ? 'Số ngày quá hạn'
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

    function bindOpenIssuesModal() {
        var modal = document.getElementById('overviewOpenIssuesModal');
        if (!modal) return;
        var input = document.getElementById('overviewOpenIssuesSearch');
        var count = document.getElementById('overviewOpenIssuesCount');
        var empty = document.getElementById('overviewOpenIssuesEmpty');
        var rows = modal.querySelectorAll('.overview-open-issue-row');
        function refresh() {
            var query = (input.value || '').trim().toLocaleLowerCase();
            var visible = 0;
            Array.prototype.forEach.call(rows, function (row) {
                var show = (row.getAttribute('data-search') || '').indexOf(query) >= 0;
                row.classList.toggle('d-none', !show);
                if (show) visible++;
            });
            count.textContent = visible + ' vấn đề';
            empty.classList.toggle('d-none', visible > 0);
        }
        modal.addEventListener('show.bs.modal', function () {
            input.value = '';
            refresh();
        });
        input.addEventListener('input', refresh);
    }

    function bindGroupedRecordModal(modalId, inputId, countId, emptyId, bodyId, rowSelector, unit) {
        var modal = document.getElementById(modalId);
        if (!modal) return;
        var input = document.getElementById(inputId);
        var count = document.getElementById(countId);
        var empty = document.getElementById(emptyId);
        var body = document.getElementById(bodyId);
        var rows = modal.querySelectorAll(rowSelector);
        var groups = body && window.DashboardProjectGroups
            ? window.DashboardProjectGroups.create(body) : null;

        function refresh() {
            var query = (input.value || '').trim().toLocaleLowerCase();
            var visible = 0;
            Array.prototype.forEach.call(rows, function (row) {
                var searchText = (row.getAttribute('data-search') || '').toLocaleLowerCase();
                var show = searchText.indexOf(query) >= 0;
                row.classList.toggle('d-none', !show);
                if (show) visible++;
            });
            if (groups) groups.refresh(Boolean(query));
            count.textContent = visible + ' ' + unit;
            empty.classList.toggle('d-none', visible > 0);
        }

        modal.addEventListener('show.bs.modal', function () {
            input.value = '';
            if (groups) groups.reset();
            refresh();
        });
        input.addEventListener('input', refresh);
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
        var labels = ['không tải', 'thiếu tải', 'đủ tải', 'quá tải'];
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
            title.textContent = filter === 'all' ? 'Mức tải nhân sự tuần này'
                : 'Nhân sự ' + labels[Number(filter)] + ' tuần này';
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
                        { 'not-started': '0', 'in-progress': '1', completed: '2', overdue: '3' }[filter];
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
    bindOpenIssuesModal();
    bindGroupedRecordModal('overviewAllIssuesModal', 'overviewAllIssuesSearch',
        'overviewAllIssuesCount', 'overviewAllIssuesEmpty', 'overviewAllIssuesBody',
        '.overview-all-issue-row', 'vấn đề');
    bindGroupedRecordModal('overviewRisksModal', 'overviewRisksSearch',
        'overviewRisksCount', 'overviewRisksEmpty', 'overviewRisksBody',
        '.overview-recorded-risk-row', 'rủi ro');
    bindProjectFinanceModal();
    bindProjectResourceModal();
    bindAllProjectsBreakdown('overviewAllTasksModal', '.overview-all-task-row', 'data-task-state',
        { all: 'Công việc của các dự án', completed: 'Công việc hoàn thành',
            'in-progress': 'Công việc đang làm', 'not-started': 'Công việc chưa bắt đầu',
            overdue: 'Công việc quá hạn' }, 'công việc');
    bindAllProjectsBreakdown('overviewAllFinanceModal', '.overview-all-finance-row', 'data-finance-filter',
        { all: 'Thu tiền và chi phí theo dự án', received: 'Đã thu theo dự án',
            outstanding: 'Còn phải thu theo dự án', cost: 'Chi phí đã duyệt theo dự án' }, 'dự án');
    bindAllProjectsBreakdown('overviewAllResourceModal', '.overview-all-resource-row', 'data-load',
        { all: 'Mức tải nhân sự tuần này', '0': 'Nhân sự không tải tuần này',
            '1': 'Nhân sự thiếu tải tuần này', '2': 'Nhân sự đủ tải tuần này',
            '3': 'Nhân sự quá tải tuần này' }, 'nhân sự');
    bindSignalModal('pending-cost');
    bindSignalModal('overloaded-employee');

    function renderProjectMiniChart(id, labels, colors, unit, isMoney) {
        var element = document.getElementById(id);
        if (!element) return;
        var layout = element.parentNode.parentNode;
        var values = (element.getAttribute('data-values') || '').split(',').map(function (value) {
            return Number(value) || 0;
        });
        var total = values.reduce(function (sum, value) { return sum + value; }, 0);
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
        var legend = layout.querySelector('.dashboard-project-chart-legend');
        var buttons = legend ? legend.querySelectorAll('button') : [];
        var chart = new ApexCharts(element, {
            chart: {
                type: 'donut', height: 170, toolbar: { show: false },
                events: { dataPointSelection: function (event, context, config) {
                    if (buttons[config.dataPointIndex]) buttons[config.dataPointIndex].click();
                } }
            },
            series: values,
            labels: labels,
            colors: colors,
            stroke: { width: 2, colors: ['#fff'] },
            legend: { show: false },
            dataLabels: {
                enabled: true,
                formatter: function (percentage, options) {
                    if (isMoney)
                        return percentage > 0 && percentage < 1
                            ? '<1%' : Math.round(percentage) + '%';
                    return values[options.seriesIndex] || '';
                },
                style: { fontSize: '10px', fontWeight: 700, colors: ['#18273f'] },
                dropShadow: { enabled: false }
            },
            tooltip: { y: { formatter: function (value) {
                return isMoney ? new Intl.NumberFormat('vi-VN').format(value) + ' đ' : value + ' ' + unit;
            } } },
            plotOptions: { pie: { expandOnClick: false, dataLabels: { minAngleToShowLabel: 8 },
                donut: { size: '58%', labels: { show: false } } } },
            states: { active: { filter: { type: 'none' } } }
        });
        chart.render();
    }

    function renderProjectMiniCharts() {
        var task = document.getElementById('overviewProjectTasksChart');
        if (task) {
            renderProjectMiniChart('overviewProjectTasksChart',
                ['Hoàn thành', 'Đang làm', 'Chưa bắt đầu', 'Quá hạn'],
                ['#35a875', '#518cdd', '#9da9bb', '#ef6b60'],
                'công việc', false);
        }
        var finance = document.getElementById('overviewProjectFinanceChart');
        if (finance) {
            renderProjectMiniChart('overviewProjectFinanceChart',
                ['Đã thu', 'Còn phải thu'], ['#35a875', '#f2b84b'],
                '', true);
        }
        var resource = document.getElementById('overviewProjectResourceChart');
        if (resource) {
            renderProjectMiniChart('overviewProjectResourceChart',
                ['Không tải', 'Thiếu tải', 'Đủ tải', 'Quá tải'],
                ['#aab6c7', '#52b6aa', '#f2b84b', '#ef6b60'],
                'nhân sự', false);
        }
        renderProjectMiniChart('overviewAllTasksChart',
            ['Hoàn thành', 'Đang làm', 'Chưa bắt đầu', 'Quá hạn'],
            ['#35a875', '#518cdd', '#9da9bb', '#ef6b60'],
            'công việc', false);
        renderProjectMiniChart('overviewAllFinanceChart',
            ['Đã thu', 'Còn phải thu'], ['#35a875', '#f2b84b'],
            '', true);
        renderProjectMiniChart('overviewAllResourceChart',
            ['Không tải', 'Thiếu tải', 'Đủ tải', 'Quá tải'],
            ['#aab6c7', '#52b6aa', '#f2b84b', '#ef6b60'],
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
                ? 'Dự án quá hạn dự kiến ' + overdueDays + ' ngày; không có công việc quá hạn hoặc vấn đề ảnh hưởng cao đang xử lý.'
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
                label: button.getAttribute('data-status-label') || '',
                count: Number(button.getAttribute('data-status-count')) || 0
            };
        });
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
                type: 'donut', height: 220, toolbar: { show: false },
                events: {
                    dataPointSelection: function (event, chartContext, config) {
                        var button = buttons[config.dataPointIndex];
                        if (button) button.click();
                    }
                }
            },
            labels: entries.map(function (entry) { return entry.label; }),
            series: entries.map(function (entry) { return entry.count; }),
            colors: ['#7896b6', '#368ed8', '#e6a32e', '#37aa80', '#9066c4'],
            stroke: { width: 2, colors: ['#fff'] },
            legend: { show: false },
            dataLabels: {
                enabled: true,
                style: { colors: ['#18273f'], fontSize: '12px', fontWeight: 700 },
                dropShadow: { enabled: false },
                formatter: function (value, options) {
                    return entries[options.seriesIndex].count;
                }
            },
            tooltip: { y: { formatter: function (value) { return value + ' dự án'; } } },
            plotOptions: {
                pie: {
                    expandOnClick: false,
                    dataLabels: { minAngleToShowLabel: 8 },
                    donut: {
                        size: '62%',
                        labels: {
                            show: true,
                            value: {
                                formatter: function (value) { return value + ' dự án'; }
                            },
                            total: {
                                show: true, showAlways: true,
                                label: 'Tổng dự án',
                                formatter: function () { return total; }
                            }
                        }
                    }
                }
            },
            states: { active: { filter: { type: 'none' } } }
        });
        statusChart.render();
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
