<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardCost.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardCost" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/Controls/Dashboard/CtrlProjectDashboardTabs.ascx"
    TagPrefix="SweetSoft" TagName="CtrlProjectDashboardTabs" %>

<style>
    /* =========================================================
       DASHBOARD CHI PHÍ - KPI
       Desktop: 5 chỉ số cùng một hàng
       Tablet: 2 cột
       Mobile: 1 cột
       ========================================================= */

    .dashboard-cost .cost-kpi-layout {
        display: grid;
        grid-template-columns: repeat(5, minmax(0, 1fr));
        gap: 1rem;
        align-items: stretch;
    }

    .dashboard-cost .cost-kpi-item {
        min-width: 0;
        display: flex;
    }

    .dashboard-cost .cost-kpi-trigger {
        flex: 1 1 0;
        width: 100%;
        min-width: 0;
    }

    .dashboard-cost .cost-kpi-card {
        width: 100%;
        min-width: 0;
        min-height: 136px;
        margin-bottom: 0;
    }

    .dashboard-cost .cost-kpi-card .card-body {
        width: 100%;
        min-width: 0;
        padding: 1rem;
    }

    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 {
        min-width: 0;
        padding-right: .75rem;
    }

    /* Tiêu đề luôn dành cùng chiều cao để các giá trị thẳng hàng */
    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > p:first-child {
        min-height: 36px;
        margin-bottom: .25rem !important;
        line-height: 1.35;
        display: block;
    }

    /* Giá trị KPI giữ trên một dòng */
    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > h3,
    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > h4 {
        min-height: 38px;
        margin-bottom: .25rem !important;
        display: flex;
        align-items: center;
        white-space: nowrap;
        font-size: clamp(1.22rem, 1.35vw, 1.6rem);
        line-height: 1.15;
        letter-spacing: -0.02em;
    }

    /* Phần mô tả có cùng vùng hiển thị */
    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > small {
        position: absolute;
        right: 1.2rem;
        bottom: .75rem;
        left: 1.2rem;
        display: block;
        min-height: 0;
        line-height: 1.25;
    }

    .dashboard-cost .cost-kpi-icon {
        flex: 0 0 42px;
        width: 42px;
        height: 42px;
        align-self: center;
    }

    /* Tablet / laptop nhỏ: 3 card mỗi hàng, giống các dashboard khác. */
    @media (max-width: 1199.98px) {
        .dashboard-cost .cost-kpi-layout {
            grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .dashboard-cost .cost-kpi-card {
            min-height: 130px;
        }
    }

    @media (max-width: 767.98px) {
        .dashboard-cost .cost-kpi-layout {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }
    }

    /* Mobile: 1 card mỗi hàng */
    @media (max-width: 575.98px) {
        .dashboard-cost .cost-kpi-layout {
            grid-template-columns: 1fr;
        }

        .dashboard-cost .cost-kpi-card {
            min-height: 124px;
        }

        .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > p:first-child,
        .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > small {
            min-height: auto;
        }

        .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > h3,
        .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > h4 {
            white-space: normal;
        }
    }
</style>

<div class="container-fluid dashboard-cost">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3 <%= IsProjectDashboard ? "dashboard-project-heading" : string.Empty %>">
        <div class="flex-grow-1">
            <h4 class="mb-1 <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TITLE) %></h4>

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

            </div>

        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 cost-date-card <%= IsProjectDashboard ? "dashboard-project-date-card" : string.Empty %>">
            <div class="bg-success-subtle text-success rounded-circle d-flex align-items-center justify-content-center me-3 cost-date-icon">
                <i class="bx bx-wallet fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_UPDATED_AT) %></div>
                <div class="fw-bold text-dark lh-1"><%= Model.GeneratedAt.ToString("HH:mm dd/MM/yyyy") %></div>
            </div>
        </div>
    </div>

    <%-- TẠM THỜI: Chú thích cách hiểu số liệu để nhóm kiểm tra; xóa cả khối này khi đã chốt nội dung. --%>
    <details class="alert alert-light border mb-3" open>
        <summary class="fw-semibold" style="cursor: pointer">Chú thích số liệu (tạm thời)</summary>
        <ul class="small mb-0 mt-2 ps-3">
            <li><strong>Số tiền trên dashboard:</strong> là số lũy kế của các dự án có trạng thái Hoàn thành trong phạm vi đang chọn, không giới hạn theo tháng; “triệu/tỷ” là cách viết gọn, rê chuột lên số để xem số tiền đầy đủ.</li>
            <li><strong>Phải thu quá hạn:</strong> tổng tiền các đợt thanh toán chưa thu và đã qua hạn; số nhỏ bên dưới là số đợt quá hạn.</li>
            <li><strong>Đã thu:</strong> tổng các đợt có ngày thanh toán thực tế. <strong>Còn phải thu:</strong> giá trị hợp đồng trừ đã thu; dự án chưa có giá trị hợp đồng thì cộng các đợt chưa thu. Đây không đồng nghĩa toàn bộ khoản tiền đã đến hạn.</li>
            <li><strong>Chi phí đã duyệt:</strong> tổng khoản chi được duyệt, không khẳng định tiền đã được chi trả. <strong>Chi phí chờ duyệt:</strong> tổng khoản chi chưa được duyệt, chưa cộng vào chi phí đã duyệt.</li>
            <li><strong>Chi phí theo dự án:</strong> hiển thị tối đa 8 dự án có chi phí đã duyệt cao nhất, không phải tổng của mọi dự án.</li>
        </ul>
    </details>

    <div class="cost-kpi-layout mb-4">
        <div class="cost-kpi-item">
            <a href="#costOverduePaymentsModal"
               class="cost-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal"
               aria-controls="costOverduePaymentsModal">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERDUE_RECEIVABLE) %></p>
                        <h4 class="mb-0 <%= Model.OverduePaymentCount > 0 ? "text-danger" : "text-muted" %>" title="<%: FormatMoney(Model.OverduePaymentAmount) %>"><%= FormatMoneySummary(Model.OverduePaymentAmount) %></h4>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_OVERDUE_PAYMENT_COUNT), Model.OverduePaymentCount) %></small>
                    </div>
                    <span class="avatar-title rounded-circle <%= Model.OverduePaymentCount > 0 ? "bg-danger-subtle text-danger" : "bg-secondary-subtle text-secondary" %> cost-kpi-icon">
                        <i class="bx bx-error-circle fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="cost-kpi-item">
            <a href="#costPaymentProjectsModal" class="cost-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-cost-payment-filter="received"
               data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.RECEIVED_PAYMENT) %>">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.RECEIVED_PAYMENT) %></p>
                        <h4 class="mb-0 text-success" title="<%: FormatMoney(Model.ReceivedPayment) %>"><%= FormatMoneySummary(Model.ReceivedPayment) %></h4>
                    </div>
                    <span class="avatar-title rounded-circle bg-success-subtle text-success cost-kpi-icon">
                        <i class="bx bx-money fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="cost-kpi-item">
            <a href="#costPaymentProjectsModal" class="cost-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-cost-payment-filter="outstanding"
               data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_OUTSTANDING_PAYMENT) %>">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OUTSTANDING_PAYMENT) %></p>
                        <h4 class="mb-0 text-warning" title="<%: FormatMoney(Model.OutstandingPayment) %>"><%= FormatMoneySummary(Model.OutstandingPayment) %></h4>
                    </div>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning cost-kpi-icon">
                        <i class="bx bx-time-five fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="cost-kpi-item">
            <a href="#costApprovedItemsModal" class="cost-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_APPROVED_COST) %>">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_APPROVED_COST) %></p>
                        <h4 class="mb-0 text-danger" title="<%: FormatMoney(Model.ActualCost) %>"><%= FormatMoneySummary(Model.ActualCost) %></h4>
                    </div>
                    <span class="avatar-title rounded-circle bg-danger-subtle text-danger cost-kpi-icon">
                        <i class="bx bx-receipt fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

        <div class="cost-kpi-item">
            <a href="#costPendingApprovalModal" class="cost-kpi-trigger dashboard-kpi-trigger d-block h-100 text-decoration-none text-reset"
               data-bs-toggle="modal" data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PENDING_COST) %>">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PENDING_COST) %></p>
                        <h3 class="mb-0 text-warning" title="<%: FormatMoney(Model.PendingApprovalCost) %>"><%= FormatMoneySummary(Model.PendingApprovalCost) %></h3>
                    </div>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning cost-kpi-icon">
                        <i class="bx bx-hourglass fs-4"></i>
                    </span>
                </div>
            </div>
            </a>
        </div>

    </div>

    <div class="row g-3 mb-3 align-items-stretch">
        <div class="col-12 col-xl-6 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-comparison-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_APPROVAL_CHART) %></h5>
                    <p class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_APPROVAL_CHART_DESC) %></p>
                    <div class="d-flex flex-column justify-content-center align-items-center flex-grow-1 py-2">
                        <div id="cost-approval-chart" class="w-100"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-6 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-payment-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <div class="flex-shrink-0">
                        <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION) %></h5>
                        <p class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION_DESC) %></p>
                    </div>
                    <div class="d-flex flex-column justify-content-center align-items-center flex-grow-1 py-2">
                        <div id="cost-payment-chart" class="w-100"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_BY_PROJECT) %></h5>
                    <p class="text-muted mb-3"><%= GetCostByProjectDescription() %></p>
                    <div id="cost-project-comparison-chart"></div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="costPaymentProjectsModal" tabindex="-1" aria-labelledby="costPaymentProjectsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costPaymentProjectsModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3"><div class="input-group dashboard-list-search">
                        <span class="input-group-text"><i class="bx bx-search"></i></span>
                        <input type="search" class="form-control" data-dashboard-list-search="costPaymentProjectsListBody"
                            placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                            aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                    </div></div>
                    <div class="table-responsive"><table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.TOTAL_CONTRACT_VALUE) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.RECEIVED_PAYMENT) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OUTSTANDING_PAYMENT) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                        </tr></thead>
                        <tbody id="costPaymentProjectsListBody">
                            <% foreach (var project in Model.ProjectStatistics) { %>
                            <tr data-search-row="true" data-cost-received="<%= project.ReceivedPayment > 0 ? "1" : "0" %>"
                                data-cost-outstanding="<%= project.OutstandingPayment > 0 ? "1" : "0" %>">
                                <td><div class="fw-semibold"><%: project.ProjectCode %></div><div class="small text-muted"><%: project.ProjectName %></div></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(project.ContractValue) %></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(project.ReceivedPayment) %></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(project.OutstandingPayment) %></td>
                                <td class="text-center"><a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectPaymentsUrl(project.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a></td>
                            </tr>
                            <% } %>
                            <tr data-search-empty="true" class="<%= Model.ProjectStatistics.Count == 0 ? string.Empty : "d-none" %>"><td colspan="5" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
                        </tbody>
                    </table></div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="costApprovedItemsModal" tabindex="-1" aria-labelledby="costApprovedItemsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costApprovedItemsModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_APPROVED_COST) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3"><div class="input-group dashboard-list-search">
                        <span class="input-group-text"><i class="bx bx-search"></i></span>
                        <input type="search" class="form-control" data-dashboard-list-search="costApprovedItemsListBody"
                            placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                            aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                    </div></div>
                    <div class="table-responsive"><table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_CODE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_NAME) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_INCURRED_DATE) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                        </tr></thead>
                        <tbody id="costApprovedItemsListBody" data-project-group-count-format="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT) %>">
                            <% foreach (var cost in Model.ApprovedCostItems) { %>
                            <tr data-search-row="true" data-cost-project-id="<%= cost.ProjectId %>"
                                data-group-project-id="<%= cost.ProjectId %>" data-group-project-code="<%: cost.ProjectCode %>"
                                data-group-project-name="<%: cost.ProjectName %>">
                                <td><%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %></td>
                                <td class="fw-semibold"><%: cost.CostName %></td>
                                <td><div class="fw-semibold"><%: cost.ProjectCode %></div><div class="small text-muted"><%: cost.ProjectName %></div></td>
                                <td class="text-nowrap"><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(cost.Amount) %></td>
                                <td class="text-center"><a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectCostsUrl(cost.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a></td>
                            </tr>
                            <% } %>
                            <tr data-search-empty="true" class="<%= Model.ApprovedCostItems.Count == 0 ? string.Empty : "d-none" %>"><td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
                        </tbody>
                    </table></div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="costOverduePaymentsModal" tabindex="-1" aria-labelledby="costOverduePaymentsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costOverduePaymentsModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVERDUE_RECEIVABLE) %> (<%= Model.OverduePaymentCount %>)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3">
                        <div class="input-group dashboard-list-search">
                            <span class="input-group-text"><i class="bx bx-search"></i></span>
                            <input type="search" class="form-control"
                                   placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   data-dashboard-list-search="costOverduePaymentListBody" />
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.PAYMENT_NAME) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                    <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.PAYMENT_DUE_DATE) %></th>
                                    <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_STATUS_OVERDUE) %></th>
                                    <th class="text-end text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                                </tr>
                            </thead>
                            <tbody id="costOverduePaymentListBody" data-project-group-count-format="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_OVERDUE_PAYMENT_COUNT) %>">
                                <% foreach (var payment in Model.OverduePayments) { %>
                                <tr data-search-row="true" data-group-project-id="<%= payment.ProjectId %>"
                                    data-group-project-code="<%: payment.ProjectCode %>" data-group-project-name="<%: payment.ProjectName %>">
                                    <td>
                                        <div class="fw-semibold"><%: payment.PaymentCode %></div>
                                        <div class="small text-muted"><%: payment.PaymentName %></div>
                                    </td>
                                    <td><strong><%: payment.ProjectCode %></strong><br /><small><%: payment.ProjectName %></small></td>
                                    <td class="text-nowrap"><%= payment.DueDate.ToString("dd/MM/yyyy") %></td>
                                    <td class="text-danger text-nowrap"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DAYS_OVERDUE), payment.DaysOverdue) %></td>
                                    <td class="text-end text-nowrap"><%= FormatMoney(payment.Amount) %></td>
                                    <td class="text-center">
                                        <a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectPaymentsUrl(payment.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a>
                                    </td>
                                </tr>
                                <% } %>
                                <tr data-search-empty="true" class="<%= Model.OverduePayments.Count == 0 ? string.Empty : "d-none" %>">
                                    <td colspan="6" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="costPendingApprovalModal" tabindex="-1" aria-labelledby="costPendingApprovalModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costPendingApprovalModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PENDING_COST) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3">
                        <div class="input-group dashboard-list-search">
                            <span class="input-group-text"><i class="bx bx-search"></i></span>
                            <input type="search" class="form-control"
                                   placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   data-dashboard-list-search="costPendingApprovalListBody" />
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                            <thead>
                                <tr>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_CODE) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_NAME) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                    <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_INCURRED_DATE) %></th>
                                    <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                                </tr>
                            </thead>
                            <tbody id="costPendingApprovalListBody" data-project-group-count-format="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT) %>">
                                <% foreach (var cost in Model.PendingApprovalCostItems) { %>
                                <tr data-search-row="true" data-group-project-id="<%= cost.ProjectId %>"
                                    data-group-project-code="<%: cost.ProjectCode %>" data-group-project-name="<%: cost.ProjectName %>">
                                    <td><%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %></td>
                                    <td class="fw-semibold"><%: cost.CostName %></td>
                                    <td>
                                        <div class="fw-semibold"><%: cost.ProjectCode %></div>
                                        <div class="small text-muted"><%: cost.ProjectName %></div>
                                    </td>
                                    <td class="text-nowrap"><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></td>
                                    <td class="text-end text-nowrap fw-semibold"><%= FormatMoney(cost.Amount) %></td>
                                    <td class="text-center">
                                        <a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectCostsUrl(cost.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a>
                                    </td>
                                </tr>
                                <% } %>
                                <tr data-search-empty="true" class="<%= Model.PendingApprovalCostItems.Count == 0 ? string.Empty : "d-none" %>">
                                    <td colspan="6" class="text-center text-muted py-4"><%= Model.PendingApprovalCostItems.Count == 0
                                        ? GetResourceText(BackEndResourceKeys.DASHBOARD_NO_COST_ITEMS)
                                        : GetResourceText(BackEndResourceKeys.NO_DATA) %></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        window.dashboardCostProjectData = <%= ProjectComparisonChartData %>;
        window.dashboardCostApprovalData = <%= CostApprovalChartData %>;
        window.dashboardCostPaymentData = <%= PaymentChartData %>;
        window.dashboardCostTexts = <%= DashboardTextsJson %>;
    </script>
</div>
