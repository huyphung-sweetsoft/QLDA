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
                        PlaceHolder="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_SEARCH_PROJECTS) %>"
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

    <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-4 g-3 mb-3 cost-financial-kpis">
        <div class="col">
            <a href="#costProjectProfitModal" data-bs-toggle="modal"
               data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_EXPECTED_GROSS_PROFIT) %>"
               class="card h-100 border-0 shadow-sm cost-financial-kpi-card text-decoration-none text-reset">
                <div class="card-body">
                    <div class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EXPECTED_GROSS_PROFIT) %></div>
                    <div class="cost-financial-kpi-value <%= Model.GrossProfit < 0 ? "text-danger" : "text-primary" %>"
                         title="<%: FormatSignedMoney(Model.GrossProfit) %>"><%= Model.TotalContractValue > 0 ? FormatSignedMoneySummary(Model.GrossProfit) : "—" %></div>
                </div>
            </a>
        </div>
        <div class="col">
            <div class="card h-100 border-0 shadow-sm cost-financial-kpi-card">
                <div class="card-body">
                    <div class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_OVER_BUDGET_PROJECTS) %></div>
                    <div class="cost-financial-kpi-value text-danger"><%= OverBudgetProjectCount %></div>
                    <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_UNIT) %></small>
                </div>
            </div>
        </div>
        <div class="col">
            <a href="#costPendingApprovalModal" data-bs-toggle="modal"
               data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PENDING_COST) %>"
               class="card h-100 border-0 shadow-sm cost-financial-kpi-card text-decoration-none text-reset">
                <div class="card-body">
                    <div class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PENDING_COST) %></div>
                    <div class="cost-financial-kpi-value text-warning"><%= Model.PendingApprovalCostItemCount %></div>
                    <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_UNIT) %></small>
                </div>
            </a>
        </div>
        <div class="col">
            <a href="#costPaymentProjectsModal" data-bs-toggle="modal"
               data-cost-payment-status="overdue"
               data-cost-modal-title="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_OVERDUE_UNPAID) %>"
               class="card h-100 border-0 shadow-sm cost-financial-kpi-card text-decoration-none text-reset">
                <div class="card-body">
                    <div class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_OVERDUE_UNPAID) %></div>
                    <div class="cost-financial-kpi-value <%= Model.OverduePaymentCount > 0 ? "text-danger" : "text-success" %>"><%= Model.OverduePaymentCount %></div>
                    <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_UNIT) %></small>
                </div>
            </a>
        </div>
    </div>

    <div class="row g-3 mb-3 align-items-stretch cost-summary-chart-layout">
        <div class="col-12 col-xl-6 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-payment-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_COUNT_CHART) %></h5>
                    <p class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_COUNT_CHART_DESC) %></p>
                    <div id="cost-payment-chart" class="w-100"></div>
                </div>
            </div>
        </div>
        <div class="col-12 col-xl-6 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-comparison-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_COUNT_CHART) %></h5>
                    <p class="text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_COUNT_CHART_DESC) %></p>
                    <div id="cost-approval-chart" class="w-100"></div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3 align-items-stretch cost-project-finance-layout">
        <div class="col-12 d-flex flex-column">
            <div class="card w-100 border-0 shadow-sm cost-comparison-card">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_FINANCIAL_PERFORMANCE) %></h5>
                    <div class="row g-3 cost-project-analysis">
                        <div class="col-12 col-lg-5">
                            <section class="cost-project-donut-panel" aria-label="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_FINANCIAL_PERFORMANCE) %>">
                                <div id="cost-project-comparison-chart" class="w-100"></div>
                            </section>
                        </div>
                        <div class="col-12 col-lg-7">
                            <section class="cost-project-list-panel">
                                <div class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-2">
                                    <div>
                                        <h6 class="mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ALL_COMPLETED_PROJECTS) %></h6>
                                        <small class="text-muted" id="costProjectVarianceCount"><%= Model.ProjectStatistics.Count %> <%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_UNIT) %></small>
                                    </div>
                                    <div class="d-flex align-items-center gap-2">
                                        <button type="button" id="costProjectVarianceClearFilter" class="btn btn-sm btn-outline-secondary d-none"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CLEAR_FILTER) %></button>
                                        <input type="search" id="costProjectVarianceSearch" class="form-control form-control-sm cost-project-list-search"
                                            placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                            aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                                    </div>
                                </div>
                                <div class="table-responsive cost-project-list-scroll">
                                    <table class="table dashboard-data-table table-hover align-middle mb-0">
                                        <thead><tr>
                                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                            <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                            <th class="text-end text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_COST_VARIANCE) %></th>
                                        </tr></thead>
                                        <tbody id="costProjectVarianceList">
                                            <% foreach (var project in Model.ProjectStatistics) { %>
                                            <tr data-cost-project-row="true" data-cost-project-status="<%: project.CostComparisonStatus %>">
                                                <td>
                                                    <div class="fw-semibold"><%: project.ProjectCode %></div>
                                                    <div class="small text-muted"><%: project.ProjectName %></div>
                                                </td>
                                                <td class="text-nowrap"><span class="badge <%= GetProjectCostStatusCss(project.CostComparisonStatus) %>"><%= GetProjectCostStatusText(project.CostComparisonStatus) %></span></td>
                                                <td class="text-end text-nowrap fw-semibold <%= project.CostVariance.HasValue ? GetAmountCss(project.CostVariance.Value) : "text-muted" %>">
                                                    <%= project.CostVariance.HasValue ? FormatSignedMoney(project.CostVariance.Value) : "—" %>
                                                    <% if (project.CostVariancePercent.HasValue) { %>
                                                    <div class="small fw-normal"><%= FormatSignedPercent(project.CostVariancePercent) %></div>
                                                    <% } %>
                                                </td>
                                            </tr>
                                            <% } %>
                                            <tr id="costProjectVarianceEmpty" class="<%= Model.ProjectStatistics.Count == 0 ? string.Empty : "d-none" %>"><td colspan="3" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
                                        </tbody>
                                    </table>
                                </div>
                            </section>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="costProjectProfitModal" tabindex="-1" aria-labelledby="costProjectProfitModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costProjectProfitModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EXPECTED_GROSS_PROFIT) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3"><div class="input-group dashboard-list-search">
                        <span class="input-group-text"><i class="bx bx-search"></i></span>
                        <input type="search" class="form-control" data-dashboard-list-search="costProjectProfitListBody"
                            placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                            aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>" />
                    </div></div>
                    <div class="table-responsive"><table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th class="text-end text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_COST_EXPECTED) %></th>
                            <th class="text-end text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_COST_ACTUAL) %></th>
                            <th class="text-end text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_EXPECTED_GROSS_PROFIT) %></th>
                        </tr></thead>
                        <tbody id="costProjectProfitListBody">
                            <% foreach (var project in Model.ProjectStatistics) { %>
                            <tr data-search-row="true">
                                <td><div class="fw-semibold"><%: project.ProjectCode %></div><div class="small text-muted"><%: project.ProjectName %></div></td>
                                <td class="text-end text-nowrap"><%= project.HasContractValue ? FormatMoney(project.ContractValue) : "—" %></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(project.ActualCost) %></td>
                                <td class="text-end text-nowrap fw-semibold <%= !project.HasContractValue ? "text-muted" : project.GrossProfit < 0 ? "text-danger" : project.GrossProfit > 0 ? "text-success" : string.Empty %>"><%= project.HasContractValue ? FormatSignedMoney(project.GrossProfit) : "—" %></td>
                            </tr>
                            <% } %>
                            <tr data-search-empty="true" class="<%= Model.ProjectStatistics.Count == 0 ? string.Empty : "d-none" %>"><td colspan="4" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
                        </tbody>
                    </table></div>
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
                            <th><%= GetResourceText(BackEndResourceKeys.PAYMENT_NAME) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.PAYMENT_DUE_DATE) %></th>
                            <th class="text-nowrap"><%= GetResourceText(BackEndResourceKeys.PAYMENT_ACTUAL_DATE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                        </tr></thead>
                        <tbody id="costPaymentProjectsListBody">
                            <% foreach (var payment in Model.PaymentItems) { %>
                            <tr data-search-row="true" data-cost-payment-status="<%: payment.StatusCategory %>"
                                data-cost-project-id="<%= payment.ProjectId %>"
                                data-cost-received="<%= payment.IsPaid ? "1" : "0" %>"
                                data-cost-outstanding="<%= payment.IsPaid ? "0" : "1" %>"
                                data-group-project-id="<%= payment.ProjectId %>" data-group-project-code="<%: payment.ProjectCode %>"
                                data-group-project-name="<%: payment.ProjectName %>">
                                <td><div class="fw-semibold"><%: payment.ProjectCode %></div><div class="small text-muted"><%: payment.ProjectName %></div></td>
                                <td><div class="fw-semibold"><%: payment.PaymentName %></div><div class="small text-muted"><%: payment.PaymentCode %></div></td>
                                <td class="text-end text-nowrap"><%= FormatMoney(payment.Amount) %></td>
                                <td class="text-nowrap"><%= payment.DueDate.HasValue ? payment.DueDate.Value.ToString("dd/MM/yyyy") : "-" %></td>
                                <td class="text-nowrap"><%= payment.ActualPaymentDate.HasValue ? payment.ActualPaymentDate.Value.ToString("dd/MM/yyyy") : "-" %></td>
                                <td class="text-nowrap">
                                    <% if (payment.IsPaidLate) { %>
                                    <span class="badge bg-primary-subtle text-primary"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_PAID_LATE_STATUS), payment.DaysLate) %></span>
                                    <% } else if (payment.IsPaid) { %>
                                    <span class="badge bg-success-subtle text-success"><%= GetResourceText(BackEndResourceKeys.PAYMENT_PAID) %></span>
                                    <% } else if (payment.IsOverdue) { %>
                                    <span class="badge bg-danger-subtle text-danger"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_OVERDUE_UNPAID) %><%= payment.HasDueDate ? " (" + string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_DAY_COUNT), payment.DaysOverdue) + ")" : string.Empty %></span>
                                    <% } else if (!payment.HasDueDate) { %>
                                    <span class="badge bg-secondary-subtle text-secondary"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_NO_DUE_DATE) %></span>
                                    <% } else if (payment.DueDate.Value.Date == DateTime.Today) { %>
                                    <span class="badge bg-warning-subtle text-warning"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_DUE_TODAY) %></span>
                                    <% } else { %>
                                    <span class="badge bg-warning-subtle text-warning"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_NOT_DUE) %></span>
                                    <% } %>
                                </td>
                                <td class="text-center"><a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectPaymentsUrl(payment.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a></td>
                            </tr>
                            <% } %>
                            <tr data-search-empty="true" class="<%= Model.PaymentItems.Count == 0 ? string.Empty : "d-none" %>"><td colspan="7" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
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
                            <tr data-search-row="true" data-cost-status="approved" data-cost-project-id="<%= cost.ProjectId %>"
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
                                    <th><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></th>
                                    <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                                </tr>
                            </thead>
                            <tbody id="costPendingApprovalListBody" data-project-group-count-format="<%: GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT) %>">
                                <% foreach (var cost in Model.PendingApprovalCostItems) { %>
                                <tr data-search-row="true" data-cost-status="pending" data-group-project-id="<%= cost.ProjectId %>"
                                    data-group-project-code="<%: cost.ProjectCode %>" data-group-project-name="<%: cost.ProjectName %>">
                                    <td><%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %></td>
                                    <td class="fw-semibold"><%: cost.CostName %></td>
                                    <td>
                                        <div class="fw-semibold"><%: cost.ProjectCode %></div>
                                        <div class="small text-muted"><%: cost.ProjectName %></div>
                                    </td>
                                    <td class="text-nowrap">
                                        <div><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></div>
                                        <div class="small text-muted">(<%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_WAITING_DAYS), cost.DaysWaiting) %>)</div>
                                    </td>
                                    <td class="text-end text-nowrap fw-semibold"><%= FormatMoney(cost.Amount) %></td>
                                    <td class="text-nowrap"><%: string.IsNullOrWhiteSpace(cost.RequesterName) ? "-" : cost.RequesterName %></td>
                                    <td class="text-center">
                                        <a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectCostsUrl(cost.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a>
                                    </td>
                                </tr>
                                <% } %>
                                <tr data-search-empty="true" class="<%= Model.PendingApprovalCostItems.Count == 0 ? string.Empty : "d-none" %>">
                                    <td colspan="7" class="text-center text-muted py-4"><%= Model.PendingApprovalCostItems.Count == 0
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

    <div class="modal fade" id="costRejectedItemsModal" tabindex="-1" aria-labelledby="costRejectedItemsModalTitle" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="costRejectedItemsModalTitle"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_REJECTED_COST) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%= GetResourceText(BackEndResourceKeys.CLOSE) %>"></button>
                </div>
                <div class="modal-body">
                    <div class="d-flex justify-content-end mb-3">
                        <div class="input-group dashboard-list-search">
                            <span class="input-group-text"><i class="bx bx-search"></i></span>
                            <input type="search" class="form-control"
                                   placeholder="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   aria-label="<%: GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS) %>"
                                   data-dashboard-list-search="costRejectedItemsListBody" />
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                            <thead><tr>
                                <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_NAME) %></th>
                                <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></th>
                                <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                            </tr></thead>
                            <tbody id="costRejectedItemsListBody">
                                <% foreach (var cost in Model.RejectedCostItems) { %>
                                <tr data-search-row="true" data-cost-status="rejected"
                                    data-group-project-id="<%= cost.ProjectId %>" data-group-project-code="<%: cost.ProjectCode %>"
                                    data-group-project-name="<%: cost.ProjectName %>">
                                    <td><div class="fw-semibold"><%: cost.ProjectCode %></div><div class="small text-muted"><%: cost.ProjectName %></div></td>
                                    <td class="fw-semibold"><%: cost.CostName %></td>
                                    <td class="text-end text-nowrap fw-semibold"><%= FormatMoney(cost.Amount) %></td>
                                    <td class="text-nowrap"><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></td>
                                    <td><span class="badge bg-danger-subtle text-danger"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_REJECTED_COST) %></span></td>
                                    <td class="text-nowrap"><%: string.IsNullOrWhiteSpace(cost.RequesterName) ? "-" : cost.RequesterName %></td>
                                    <td class="text-center"><a class="btn btn-sm btn-outline-primary text-nowrap" href="<%: GetProjectCostsUrl(cost.ProjectId) %>"><%= GetResourceText(BackEndResourceKeys.VIEW_DETAIL) %></a></td>
                                </tr>
                                <% } %>
                                <tr data-search-empty="true" class="<%= Model.RejectedCostItems.Count == 0 ? string.Empty : "d-none" %>"><td colspan="7" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></td></tr>
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
