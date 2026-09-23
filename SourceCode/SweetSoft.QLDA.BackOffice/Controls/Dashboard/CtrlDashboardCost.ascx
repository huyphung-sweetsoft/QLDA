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
       Desktop: 4 card hàng trên + 3 card hàng dưới
       Tablet: 2 cột
       Mobile: 1 cột
       ========================================================= */

    .dashboard-cost .cost-kpi-layout {
        display: grid;
        grid-template-columns: repeat(12, minmax(0, 1fr));
        gap: 1rem;
        align-items: stretch;
    }

    .dashboard-cost .cost-kpi-item {
        min-width: 0;
        display: flex;
    }

    /* 4 KPI đầu: mỗi card chiếm 3/12 cột */
    .dashboard-cost .cost-kpi-item:nth-child(-n+4) {
        grid-column: span 3;
    }

    /* 3 KPI cuối: mỗi card chiếm 4/12 cột */
    .dashboard-cost .cost-kpi-item:nth-child(n+5) {
        grid-column: span 4;
    }

    .dashboard-cost .cost-kpi-card {
        width: 100%;
        min-width: 0;
        min-height: 182px;
        margin-bottom: 0;
    }

    .dashboard-cost .cost-kpi-card .card-body {
        width: 100%;
        min-width: 0;
        padding: 1.25rem;
    }

    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 {
        min-width: 0;
        padding-right: .75rem;
    }

    /* Tiêu đề luôn dành cùng chiều cao để các giá trị thẳng hàng */
    .dashboard-cost .cost-kpi-card .card-body > .flex-grow-1 > p:first-child {
        min-height: 44px;
        margin-bottom: .25rem !important;
        line-height: 1.35;
        display: flex;
        align-items: flex-start;
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
        min-height: 40px;
        display: block;
        line-height: 1.35;
    }

    .dashboard-cost .cost-kpi-icon {
        flex: 0 0 54px;
        width: 54px;
        height: 54px;
        align-self: center;
    }

    /* Tablet / laptop nhỏ: 2 card mỗi hàng */
    @media (max-width: 1199.98px) {
        .dashboard-cost .cost-kpi-layout {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }

        .dashboard-cost .cost-kpi-item:nth-child(-n+4),
        .dashboard-cost .cost-kpi-item:nth-child(n+5) {
            grid-column: span 1;
        }

        .dashboard-cost .cost-kpi-card {
            min-height: 176px;
        }
    }

    /* Mobile: 1 card mỗi hàng */
    @media (max-width: 575.98px) {
        .dashboard-cost .cost-kpi-layout {
            grid-template-columns: 1fr;
        }

        .dashboard-cost .cost-kpi-card {
            min-height: 168px;
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
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
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

                <div class="col-12 col-sm-6 col-md-4 col-xl-3 <%= IsProjectDashboard ? "ms-auto" : string.Empty %>">
                    <label class="form-label mb-1 text-nowrap <%= IsProjectDashboard ? "d-none" : string.Empty %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETION_PERIOD) %></label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlCompletionPeriod"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true"
                        OnSelectedIndexChanged="ddlCompletionPeriod_SelectedIndexChanged">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-12 col-md-3 col-xl-auto mt-3 mt-md-0 <%= IsProjectDashboard ? "d-none" : string.Empty %>">
                    <SweetSoft:ExtraButton
                        ID="btnApplyCostFilter"
                        runat="server"
                        CssClass="w-100 px-4"
                        ButtonStyle="Primary"
                        ButtonIcon="Search"
                        OnClick="btnApplyCostFilter_Click">
                    </SweetSoft:ExtraButton>
                </div>
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

    <% if (Model.CompletedProjectCount == 0) { %>
    <div class="alert alert-info border-0 shadow-sm d-flex align-items-start mb-4" role="alert">
        <i class="bx bx-info-circle fs-3 me-3"></i>
        <div>
            <div class="fw-semibold mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_COMPLETED_PROJECTS) %></div>
            <div>
                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_COMPLETED_PROJECTS_DESC) %>
            </div>
        </div>
    </div>
    <% } %>

    <div class="cost-kpi-layout mb-4">
        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETED_PROJECTS) %></p>
                        <h3 class="mb-0 text-primary"><%= Model.CompletedProjectCount %></h3>
                        <small class="text-muted"><%= GetSelectedPeriodText() %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-primary-subtle text-primary cost-kpi-icon">
                        <i class="bx bx-check-double fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.TOTAL_CONTRACT_VALUE) %></p>
                        <h4 class="mb-0 text-info"><%= FormatMoney(Model.TotalContractValue) %></h4>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_REVENUE) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-info-subtle text-info cost-kpi-icon">
                        <i class="bx bx-file fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.ACTUAL_COST) %></p>
                        <h4 class="mb-0 text-danger"><%= FormatMoney(Model.ActualCost) %></h4>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_AVERAGE_PER_PROJECT), FormatMoney(Model.AverageCostPerProject)) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-danger-subtle text-danger cost-kpi-icon">
                        <i class="bx bx-receipt fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_GROSS_PROFIT) %></p>
                        <h4 class="mb-0 <%= GetAmountCss(Model.GrossProfit) %>"><%= FormatMoney(Model.GrossProfit) %></h4>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_MINUS_COST) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-success-subtle text-success cost-kpi-icon">
                        <i class="bx bx-line-chart fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROFIT_MARGIN) %></p>
                        <h3 class="mb-0 <%= GetAmountCss(Model.GrossProfit) %>"><%= Model.ProfitMargin.ToString("0.##") %>%</h3>
                        <small class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_ON_CONTRACT_VALUE) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning cost-kpi-icon">
                        <i class="bx bx-pie-chart-alt-2 fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.RECEIVED_PAYMENT) %></p>
                        <h4 class="mb-0 text-success"><%= FormatMoney(Model.ReceivedPayment) %></h4>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_OUTSTANDING_AMOUNT), FormatMoney(Model.OutstandingPayment)) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-success-subtle text-success cost-kpi-icon">
                        <i class="bx bx-money fs-4"></i>
                    </span>
                </div>
            </div>
        </div>

        <div class="cost-kpi-item">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1 cost-kpi-value">
                        <p class="text-muted mb-1"><%= GetResourceText(BackEndResourceKeys.NOT_APPROVED) %></p>
                        <h4 class="mb-0 text-warning"><%= FormatMoney(Model.PendingApprovalCost) %></h4>
                        <small class="text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT), Model.PendingApprovalCostItemCount) %></small>
                    </div>
                    <span class="avatar-title rounded-circle bg-warning-subtle text-warning cost-kpi-icon">
                        <i class="bx bx-time-five fs-4"></i>
                    </span>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3 align-items-stretch">
        <div class="col-12 col-xl-8 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-comparison-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_VS_ACTUAL_COST) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_VS_ACTUAL_COST_DESC) %></p>
                    <div class="cost-chart-scroll flex-grow-1">
                        <div id="cost-project-comparison-chart"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex flex-column">
            <div class="card w-100 h-100 flex-grow-1 border-0 shadow-sm cost-payment-card">
                <div class="card-body d-flex flex-column flex-grow-1">
                    <div class="flex-shrink-0">
                        <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION) %></h5>
                        <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION_DESC) %></p>
                    </div>
                    <div class="d-flex flex-column justify-content-center align-items-center flex-grow-1 py-2">
                        <div id="cost-payment-chart" class="w-100"></div>
                        <div class="text-center small text-muted mt-2">
                            <strong><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COLLECTION_RATE), Model.PaymentCollectionRate.ToString("0.##")) %></strong>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TREND) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TREND_DESC) %></p>
                    <div id="cost-trend-chart"></div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_FINANCIAL_PERFORMANCE) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_GROSS_PROFIT_FORMULA) %></p>
                </div>
                <div class="small text-muted mt-2 mt-md-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_SORT_MARGIN_ASC) %></div>
            </div>

            <div class="table-responsive">
                <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0 cost-project-table">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETION) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.TOTAL_CONTRACT_VALUE) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.ACTUAL_COST) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_GROSS_PROFIT) %></th>
                            <th class="text-center"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_MARGIN_SHORT) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_RECEIVED_OUTSTANDING) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectStatistics) { %>
                        <tr>
                            <td>
                                <a class="d-block text-decoration-none text-reset" href="<%: GetProjectDetailUrl(project.ProjectId) %>">
                                    <div class="fw-semibold"><%: project.ProjectCode %></div>
                                    <div class="small text-muted"><%: project.ProjectName %></div>
                                </a>
                            </td>
                            <td class="text-nowrap"><%= project.CompletionDate.ToString("dd/MM/yyyy") %></td>
                            <td>
                                <% if (string.IsNullOrEmpty(project.ContractNumber)) { %>
                                <span class="text-muted"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_CONTRACT) %></span>
                                <% } else { %>
                                <%: project.ContractNumber %>
                                <% } %>
                            </td>
                            <td class="text-end text-nowrap"><%= FormatMoney(project.ContractValue) %></td>
                            <td class="text-end text-nowrap">
                                <a class="d-block text-decoration-none text-reset" href="<%: GetProjectCostsUrl(project.ProjectId) %>">
                                    <div class="fw-semibold text-danger"><%= FormatMoney(project.ActualCost) %></div>
                                    <div class="small text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT), project.CostItemCount) %></div>
                                </a>
                            </td>
                            <td class="text-end text-nowrap">
                                <span class="fw-semibold <%= GetAmountCss(project.GrossProfit) %>"><%= FormatMoney(project.GrossProfit) %></span>
                            </td>
                            <td class="text-center">
                                <span class="badge <%= GetProfitBadgeCss(project.GrossProfit) %>"><%= project.ProfitMargin.ToString("0.##") %>%</span>
                            </td>
                            <td class="text-end text-nowrap">
                                <a class="d-block text-decoration-none text-reset" href="<%: GetProjectPaymentsUrl(project.ProjectId) %>">
                                    <div class="text-success"><%= FormatMoney(project.ReceivedPayment) %></div>
                                    <div class="small text-muted"><%= FormatMoney(project.OutstandingPayment) %></div>
                                </a>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Model.ProjectStatistics.Count == 0) { %>
                        <tr>
                            <td colspan="8" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_FINANCIAL_DATA) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_LARGEST_COST_ITEMS) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_LARGEST_COST_ITEMS_DESC) %></p>
                </div>
                <span class="badge bg-danger-subtle text-danger align-self-start mt-2 mt-md-0">
                    <%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT), Model.LargestCostItems.Count) %>
                </span>
            </div>

            <div class="table-responsive">
                <table class="table dashboard-data-table table-bordered table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_CODE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_NAME) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.PROJECT) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DASHBOARD_INCURRED_DATE) %></th>
                            <th class="text-end"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_AMOUNT) %></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var cost in Model.LargestCostItems) { %>
                        <tr>
                            <td>
                                <a class="text-decoration-none text-reset" href="<%: GetProjectCostsUrl(cost.ProjectId) %>">
                                    <%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %>
                                </a>
                            </td>
                            <td class="fw-semibold">
                                <a class="text-decoration-none text-reset" href="<%: GetProjectCostsUrl(cost.ProjectId) %>"><%: cost.CostName %></a>
                            </td>
                            <td>
                                <a class="d-block text-decoration-none text-reset" href="<%: GetProjectDetailUrl(cost.ProjectId) %>">
                                    <div><%: cost.ProjectCode %></div>
                                    <div class="small text-muted"><%: cost.ProjectName %></div>
                                </a>
                            </td>
                            <td class="text-nowrap"><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></td>
                            <td class="text-end text-nowrap fw-semibold text-danger"><%= FormatMoney(cost.Amount) %></td>
                        </tr>
                        <% } %>
                        <% if (Model.LargestCostItems.Count == 0) { %>
                        <tr>
                            <td colspan="5" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_NO_COST_ITEMS) %></td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
        </div>
    </div>

    <script type="text/javascript">
        window.dashboardCostProjectData = <%= ProjectComparisonChartData %>;
        window.dashboardCostTrendData = <%= CostTrendChartData %>;
        window.dashboardCostPaymentData = <%= PaymentChartData %>;
        window.dashboardCostTexts = <%= DashboardTextsJson %>;
    </script>
</div>
