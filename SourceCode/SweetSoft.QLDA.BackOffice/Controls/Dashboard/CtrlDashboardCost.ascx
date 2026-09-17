<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardCost.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardCost" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-cost">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TITLE) %></h4>

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
                    <label class="form-label mb-1 text-nowrap"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COMPLETION_PERIOD) %></label>
                    <SweetSoft:ExtraDropdown
                        ID="ddlCompletionPeriod"
                        runat="server"
                        CssClass="form-select"
                        EmptyItemValue="-1"
                        SimpleInit="true">
                    </SweetSoft:ExtraDropdown>
                </div>

                <div class="col-12 col-md-3 col-xl-auto mt-3 mt-md-0">
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

            <p class="text-muted mt-2 mb-0">
                <%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_FILTER_DESC) %>
            </p>
        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 cost-date-card">
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

    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-6 g-3 mb-4">
        <div class="col">
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

        <div class="col">
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

        <div class="col">
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

        <div class="col">
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

        <div class="col">
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

        <div class="col">
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
    </div>

    <div class="row g-3 mb-3">
        <div class="col-12 col-xl-8 d-flex">
            <div class="card w-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_VS_ACTUAL_COST) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_CONTRACT_VS_ACTUAL_COST_DESC) %></p>
                    <div class="cost-chart-scroll">
                        <div id="cost-project-comparison-chart"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card w-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION) %></h5>
                    <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PAYMENT_SITUATION_DESC) %></p>
                    <div id="cost-payment-chart"></div>
                    <div class="text-center small text-muted mt-2">
                        <strong><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COLLECTION_RATE), Model.PaymentCollectionRate.ToString("0.##")) %></strong>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TREND) %></h5>
            <p class="text-muted mb-3"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_COST_TREND_DESC) %></p>
            <div id="cost-trend-chart"></div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_PROJECT_FINANCIAL_PERFORMANCE) %></h5>
                    <p class="text-muted mb-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_GROSS_PROFIT_FORMULA) %></p>
                </div>
                <div class="small text-muted mt-2 mt-md-0"><%= GetResourceText(BackEndResourceKeys.DASHBOARD_SORT_MARGIN_ASC) %></div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0 cost-project-table">
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
                                <div class="fw-semibold text-danger"><%= FormatMoney(project.ActualCost) %></div>
                                <div class="small text-muted"><%= string.Format(GetResourceText(BackEndResourceKeys.DASHBOARD_COST_ITEM_COUNT), project.CostItemCount) %></div>
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

    <div class="card border-0 shadow-sm mb-3">
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
                <table class="table table-hover align-middle mb-0">
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
                            <td><%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %></td>
                            <td class="fw-semibold"><%: cost.CostName %></td>
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

    <script type="text/javascript">
        window.dashboardCostProjectData = <%= ProjectComparisonChartData %>;
        window.dashboardCostTrendData = <%= CostTrendChartData %>;
        window.dashboardCostPaymentData = <%= PaymentChartData %>;
        window.dashboardCostTexts = <%= DashboardTextsJson %>;
    </script>
</div>
