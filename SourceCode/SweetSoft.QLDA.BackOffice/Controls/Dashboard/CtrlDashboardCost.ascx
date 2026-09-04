<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDashboardCost.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboardCost" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="container-fluid dashboard-cost">
    <div class="d-flex flex-column flex-lg-row align-items-lg-start justify-content-between mb-3">
        <div class="flex-grow-1">
            <h4 class="mb-1">Dashboard chi phí dự án đã hoàn thành</h4>

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
                    <label class="form-label mb-1 text-nowrap">Thời gian hoàn thành</label>
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
                Chỉ thống kê dự án đã có ngày hoàn thành thực tế. Khoảng thời gian lọc theo ngày hoàn thành dự án,
                còn chi phí và thanh toán được lấy toàn bộ vòng đời của các dự án phù hợp.
            </p>
        </div>

        <div class="d-flex align-items-center bg-white border rounded px-4 py-3 mt-3 mt-lg-0 shadow-sm ms-lg-4 cost-date-card">
            <div class="bg-success-subtle text-success rounded-circle d-flex align-items-center justify-content-center me-3 cost-date-icon">
                <i class="bx bx-wallet fs-3"></i>
            </div>
            <div>
                <div class="text-muted small fw-medium text-uppercase mb-1">Cập nhật dữ liệu</div>
                <div class="fw-bold text-dark lh-1"><%= Model.GeneratedAt.ToString("HH:mm dd/MM/yyyy") %></div>
            </div>
        </div>
    </div>

    <% if (Model.CompletedProjectCount == 0) { %>
    <div class="alert alert-info border-0 shadow-sm d-flex align-items-start mb-4" role="alert">
        <i class="bx bx-info-circle fs-3 me-3"></i>
        <div>
            <div class="fw-semibold mb-1">Chưa có dự án đã hoàn thành phù hợp với bộ lọc</div>
            <div>
                Dự án chỉ xuất hiện ở dashboard này khi trường <strong>Ngày hoàn thành thực tế</strong> đã được cập nhật.
                Dự án đang thực hiện không được cộng vào các chỉ số tài chính bên dưới.
            </div>
        </div>
    </div>
    <% } %>

    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-6 g-3 mb-4">
        <div class="col">
            <div class="card h-100 border-0 shadow-sm cost-kpi-card">
                <div class="card-body d-flex align-items-center">
                    <div class="flex-grow-1">
                        <p class="text-muted mb-1">Dự án hoàn thành</p>
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
                        <p class="text-muted mb-1">Giá trị hợp đồng</p>
                        <h4 class="mb-0 text-info"><%= FormatMoney(Model.TotalContractValue) %></h4>
                        <small class="text-muted">Doanh thu theo hợp đồng</small>
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
                        <p class="text-muted mb-1">Chi phí thực tế</p>
                        <h4 class="mb-0 text-danger"><%= FormatMoney(Model.ActualCost) %></h4>
                        <small class="text-muted">Bình quân <%= FormatMoney(Model.AverageCostPerProject) %>/dự án</small>
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
                        <p class="text-muted mb-1">Lợi nhuận gộp</p>
                        <h4 class="mb-0 <%= GetAmountCss(Model.GrossProfit) %>"><%= FormatMoney(Model.GrossProfit) %></h4>
                        <small class="text-muted">Hợp đồng − chi phí</small>
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
                        <p class="text-muted mb-1">Biên lợi nhuận</p>
                        <h3 class="mb-0 <%= GetAmountCss(Model.GrossProfit) %>"><%= Model.ProfitMargin.ToString("0.##") %>%</h3>
                        <small class="text-muted">Trên giá trị hợp đồng</small>
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
                        <p class="text-muted mb-1">Đã thu</p>
                        <h4 class="mb-0 text-success"><%= FormatMoney(Model.ReceivedPayment) %></h4>
                        <small class="text-muted">Còn <%= FormatMoney(Model.OutstandingPayment) %></small>
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
                    <h5 class="card-title mb-1">Hợp đồng so với chi phí thực tế</h5>
                    <p class="text-muted mb-3">So sánh giá trị hợp đồng và tổng chi phí của từng dự án đã hoàn thành.</p>
                    <div class="cost-chart-scroll">
                        <div id="cost-project-comparison-chart"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12 col-xl-4 d-flex">
            <div class="card w-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title mb-1">Tình hình thu tiền</h5>
                    <p class="text-muted mb-3">Số đã thu và còn phải thu so với tổng giá trị hợp đồng.</p>
                    <div id="cost-payment-chart"></div>
                    <div class="text-center small text-muted mt-2">
                        Tỷ lệ thu tiền: <strong><%= Model.PaymentCollectionRate.ToString("0.##") %>%</strong>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <h5 class="card-title mb-1">Xu hướng phát sinh chi phí</h5>
            <p class="text-muted mb-3">Tổng chi phí theo tháng trong toàn bộ vòng đời của các dự án đã hoàn thành thuộc phạm vi lọc.</p>
            <div id="cost-trend-chart"></div>
        </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
        <div class="card-body">
            <div class="d-flex flex-column flex-md-row justify-content-between mb-3">
                <div>
                    <h5 class="card-title mb-1">Hiệu quả tài chính theo dự án</h5>
                    <p class="text-muted mb-0">Lợi nhuận gộp = giá trị hợp đồng − chi phí thực tế.</p>
                </div>
                <div class="small text-muted mt-2 mt-md-0">Sắp xếp từ biên lợi nhuận thấp đến cao</div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0 cost-project-table">
                    <thead>
                        <tr>
                            <th>Dự án</th>
                            <th>Hoàn thành</th>
                            <th>Hợp đồng</th>
                            <th class="text-end">Giá trị hợp đồng</th>
                            <th class="text-end">Chi phí thực tế</th>
                            <th class="text-end">Lợi nhuận gộp</th>
                            <th class="text-center">Biên LN</th>
                            <th class="text-end">Đã thu / Còn thu</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var project in Model.ProjectStatistics) { %>
                        <tr>
                            <td>
                                <div class="fw-semibold"><%: project.ProjectCode %></div>
                                <div class="small text-muted"><%: project.ProjectName %></div>
                            </td>
                            <td class="text-nowrap"><%= project.CompletionDate.ToString("dd/MM/yyyy") %></td>
                            <td>
                                <% if (string.IsNullOrEmpty(project.ContractNumber)) { %>
                                <span class="text-muted">Chưa gắn hợp đồng</span>
                                <% } else { %>
                                <%: project.ContractNumber %>
                                <% } %>
                            </td>
                            <td class="text-end text-nowrap"><%= FormatMoney(project.ContractValue) %></td>
                            <td class="text-end text-nowrap">
                                <div class="fw-semibold text-danger"><%= FormatMoney(project.ActualCost) %></div>
                                <div class="small text-muted"><%= project.CostItemCount %> khoản chi</div>
                            </td>
                            <td class="text-end text-nowrap">
                                <span class="fw-semibold <%= GetAmountCss(project.GrossProfit) %>"><%= FormatMoney(project.GrossProfit) %></span>
                            </td>
                            <td class="text-center">
                                <span class="badge <%= GetProfitBadgeCss(project.GrossProfit) %>"><%= project.ProfitMargin.ToString("0.##") %>%</span>
                            </td>
                            <td class="text-end text-nowrap">
                                <div class="text-success"><%= FormatMoney(project.ReceivedPayment) %></div>
                                <div class="small text-muted"><%= FormatMoney(project.OutstandingPayment) %></div>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Model.ProjectStatistics.Count == 0) { %>
                        <tr>
                            <td colspan="8" class="text-center text-muted py-4">Không có dữ liệu tài chính của dự án đã hoàn thành.</td>
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
                    <h5 class="card-title mb-1">Các khoản chi lớn nhất</h5>
                    <p class="text-muted mb-0">Tối đa 15 khoản chi có giá trị cao nhất trong phạm vi dự án đã chọn.</p>
                </div>
                <span class="badge bg-danger-subtle text-danger align-self-start mt-2 mt-md-0">
                    <%= Model.LargestCostItems.Count %> khoản chi
                </span>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th>Mã khoản chi</th>
                            <th>Tên khoản chi</th>
                            <th>Dự án</th>
                            <th>Ngày phát sinh</th>
                            <th class="text-end">Số tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var cost in Model.LargestCostItems) { %>
                        <tr>
                            <td><%: string.IsNullOrEmpty(cost.CostCode) ? "-" : cost.CostCode %></td>
                            <td class="fw-semibold"><%: cost.CostName %></td>
                            <td>
                                <div><%: cost.ProjectCode %></div>
                                <div class="small text-muted"><%: cost.ProjectName %></div>
                            </td>
                            <td class="text-nowrap"><%= cost.OccurredDate.ToString("dd/MM/yyyy") %></td>
                            <td class="text-end text-nowrap fw-semibold text-danger"><%= FormatMoney(cost.Amount) %></td>
                        </tr>
                        <% } %>
                        <% if (Model.LargestCostItems.Count == 0) { %>
                        <tr>
                            <td colspan="5" class="text-center text-muted py-4">Chưa có khoản chi phù hợp với bộ lọc.</td>
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
    </script>
</div>
