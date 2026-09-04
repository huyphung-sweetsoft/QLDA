<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlEmployeeDashboard.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlEmployeeDashboard" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="row">
    <!-- Filters -->
    <div class="col-12 mb-3">
        <div class="card">
            <div class="card-body">
                <div class="d-flex align-items-center justify-content-between">
                    <div>
                        <h4 class="mb-1">
                            Xin chào, <%= CurrentUserName %>
                        </h4>
                        <p class="text-muted mb-0">Tổng quan công việc cá nhân</p>
                    </div>
                    <div class="d-flex gap-2">
                        <SweetSoft:ExtraDropdown ID="ddlProject" runat="server" CssClass="form-select" SimpleInit="true" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                        <SweetSoft:ExtraDropdown ID="ddlTimeFilter" runat="server" CssClass="form-select" SimpleInit="true" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 5 KPIs -->
    <div class="col-12 mb-4">
        <div class="row row-cols-1 row-cols-md-5 g-3">
            <div class="col">
                <div class="border rounded p-3 h-100 bg-white">
                    <div class="text-muted mb-1">Công việc đang thực hiện</div>
                    <h4 class="mb-0 text-primary"><%= Model.KPIs.OngoingTaskCount %></h4>
                </div>
            </div>
            <div class="col">
                <div class="border rounded p-3 h-100 bg-white">
                    <div class="text-muted mb-1">Công việc sắp đến hạn</div>
                    <h4 class="mb-0 text-warning"><%= Model.KPIs.UpcomingDeadlineTaskCount %></h4>
                </div>
            </div>
            <div class="col">
                <div class="border rounded p-3 h-100 bg-white">
                    <div class="text-muted mb-1">Công việc quá hạn</div>
                    <h4 class="mb-0 text-danger"><%= Model.KPIs.OverdueTaskCount %></h4>
                </div>
            </div>
            <div class="col">
                <div class="border rounded p-3 h-100 bg-white">
                    <div class="text-muted mb-1">Dự án đang tham gia</div>
                    <h4 class="mb-0 text-info"><%= Model.KPIs.ActiveProjectCount %></h4>
                </div>
            </div>
            <div class="col">
                <div class="border rounded p-3 h-100 bg-white">
                    <div class="text-muted mb-1">Mức tải hiện tại</div>
                    <h4 class="mb-0 text-success"><%= Model.KPIs.WorkloadPercent %>%</h4>
                </div>
            </div>
        </div>
    </div>

    <!-- CÔNG VIỆC CỦA TÔI & CẢNH BÁO -->
    <div class="col-12 col-xl-8 mb-4 d-flex">
        <div class="card w-100 h-100">
            <div class="card-body">
                <div class="d-flex justify-content-between mb-3">
                    <h5 class="card-title mb-0 text-uppercase">Công việc của tôi</h5>
                    <a href="/Tasks.aspx" class="btn btn-sm btn-outline-primary">Xem tất cả công việc</a>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover mb-0">
                        <thead>
                            <tr>
                                <th>Công việc</th>
                                <th>Dự án</th>
                                <th>Hạn</th>
                                <th>Tiến độ</th>
                            </tr>
                        </thead>
                        <tbody>
                            <% foreach (var task in Model.MyTasks) { %>
                            <tr>
                                <td><%= task.TaskName %></td>
                                <td><%= task.ProjectCode %></td>
                                <td><%= task.Deadline.HasValue ? task.Deadline.Value.ToString("dd/MM") : "-" %></td>
                                <td>
                                    <div class="d-flex align-items-center">
                                        <div class="progress flex-grow-1" style="height: 6px; margin-right: 10px;">
                                            <div class="progress-bar" role="progressbar" style="width: <%= task.Progress %>%;" aria-valuenow="<%= task.Progress %>" aria-valuemin="0" aria-valuemax="100"></div>
                                        </div>
                                        <span class="small text-muted"><%= task.Progress %>%</span>
                                    </div>
                                </td>
                            </tr>
                            <% } %>
                            <% if(Model.MyTasks.Count == 0) { %>
                            <tr>
                                <td colspan="4" class="text-center text-muted">Không có công việc nào</td>
                            </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
    
    <div class="col-12 col-xl-4 mb-4 d-flex">
        <div class="card w-100 h-100">
            <div class="card-body">
                <h5 class="card-title mb-3 text-uppercase">Cảnh báo của tôi</h5>
                <ul class="list-group list-group-flush">
                    <% foreach (var warning in Model.MyWarnings) { %>
                    <li class="list-group-item px-0 border-0 d-flex align-items-start">
                        <i class="<%= warning.IconClass %> <%= warning.TextClass %> me-2 fs-5 mt-1"></i>
                        <span class="<%= warning.TextClass %>"><%= warning.Message %></span>
                    </li>
                    <% } %>
                    <% if(Model.MyWarnings.Count == 0) { %>
                    <li class="list-group-item px-0 border-0 text-muted">Không có cảnh báo nào</li>
                    <% } %>
                </ul>
            </div>
        </div>
    </div>

    <!-- TIẾN ĐỘ DỰ ÁN & LỊCH -->
    <div class="col-12 col-xl-8 mb-4 d-flex">
        <div class="card w-100 h-100">
            <div class="card-body">
                <h5 class="card-title mb-1 text-uppercase">Tiến độ các dự án của tôi</h5>
                <p class="text-muted mb-3">Hiển thị các dự án bạn đang tham gia</p>
                <div id="employee-project-chart-wrapper" style="height: 300px; overflow-y: auto; overflow-x: hidden;">
                    <div id="employee-project-chart"></div>
                </div>
                <script>
                    window.employeeProjectChartData = <%= ProjectChartDataJson %>;
                </script>
                <script src="/Controls/Dashboard/dashboard-employee.js" type="text/javascript"></script>
            </div>
        </div>
    </div>

    <div class="col-12 col-xl-4 mb-4 d-flex">
        <div class="card w-100 h-100">
            <div class="card-body">
                <h5 class="card-title mb-3 text-uppercase">Lịch sắp tới</h5>
                <ul class="list-group list-group-flush">
                    <% foreach (var meeting in Model.UpcomingMeetings) { %>
                    <li class="list-group-item px-0 d-flex flex-column">
                        <div class="fw-bold"><%= meeting.StartTime.ToString("HH:mm") %> - <%= meeting.Title %></div>
                        <div class="text-muted small"><%= meeting.ProjectName %> | <%= meeting.StartTime.ToString("dd/MM/yyyy") %></div>
                    </li>
                    <% } %>
                    <% if(Model.UpcomingMeetings.Count == 0) { %>
                    <li class="list-group-item px-0 text-muted border-0">Không có lịch sắp tới</li>
                    <% } %>
                </ul>
            </div>
        </div>
    </div>
</div>
