<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="ProjectReport.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectReports.ProjectReport" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        body { background-color: #f0f2f5; color: #333; }

        .page-header { background: white; padding: 18px 24px; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.08); margin-bottom: 20px; display: flex; justify-content: space-between; align-items: center; }
        .page-title h2 { font-size: 20px; color: #1e293b; font-weight: bold; margin: 0; }
        .page-title p { font-size: 13px; color: #64748b; margin-top: 4px; margin-bottom: 0; }

        .btn-group { display: flex; gap: 10px; }
        .btn-custom { border: none; padding: 8px 16px; border-radius: 6px; font-size: 13px; font-weight: 600; cursor: pointer; display: inline-flex; align-items: center; gap: 6px; text-decoration: none; }
        .btn-red { background-color: #dc2626; color: white; }
        .btn-red:hover { background-color: #b91c1c; color: white; }
        .btn-green { background-color: #16a34a; color: white; }
        .btn-green:hover { background-color: #15803d; color: white; }
        .btn-blue { background-color: #2563eb; color: white; }
        .btn-blue:hover { background-color: #1d4ed8; color: white; }

        .filter-card { background: white; padding: 16px 24px; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.08); margin-bottom: 25px; display: flex; gap: 15px; align-items: flex-end; flex-wrap: wrap; }
        .form-group-filter { display: flex; flex-direction: column; gap: 6px; }
        .form-group-filter label { font-size: 13px; font-weight: 600; color: #475569; }
        .form-group-filter .form-control { padding: 8px 12px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 13px; outline: none; transition: border-color 0.2s; }
        .form-group-filter .form-control:focus { border-color: #3b82f6; }
        
        .report-paper { background: white; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.06); padding: 35px 40px; max-width: 1000px; margin: 0 auto 40px auto; border: 1px solid #e2e8f0; }
        .report-header { text-align: center; margin-bottom: 25px; padding-bottom: 15px; border-bottom: 2px solid #3b82f6; }
        .report-header h1 { font-size: 22px; color: #1e3a8a; font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px; margin: 0; }
        .report-header .sub-date { font-size: 13.5px; color: #475569; margin-top: 6px; font-weight: 600; }

        .kpi-summary-bar { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 18px; margin-bottom: 30px; }
        .kpi-item { display: flex; flex-direction: column; gap: 4px; }
        .kpi-title { font-size: 12px; color: #64748b; font-weight: 600; text-transform: uppercase; }
        .kpi-number { font-size: 16px; font-weight: 700; color: #0f172a; }
        .kpi-number.success { color: #16a34a; }
        .kpi-number.danger { color: #dc2626; }
        .kpi-number.warning { color: #d97706; }

        .section-title { font-size: 15px; font-weight: 700; color: #1e293b; margin-top: 25px; margin-bottom: 12px; display: flex; align-items: center; gap: 8px; }
        .report-table { width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 20px; }
        .report-table th, .report-table td { padding: 10px 12px; border: 1px solid #cbd5e1; text-align: left; vertical-align: middle; }
        .report-table th { background-color: #f1f5f9; color: #334155; font-weight: 700; }

        .report-badge { padding: 3px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; display: inline-block; }
        .badge-success { background-color: #dcfce7; color: #15803d; }
        .badge-danger { background-color: #fee2e2; color: #b91c1c; }
        .badge-warning { background-color: #fef3c7; color: #b45309; }
        .badge-info { background-color: #e0f2fe; color: #0369a1; }
        .badge-high { background-color: #fee2e2; color: #991b1b; }
        .badge-med { background-color: #fef3c7; color: #92400e; }
        .badge-low { background-color: #f1f5f9; color: #475569; }
        
        /* State rỗng khi dự án chưa bắt đầu */
        .empty-state { text-align: center; padding: 60px 20px; background: white; border-radius: 8px; border: 1px dashed #cbd5e1; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
        .empty-state i { font-size: 50px; color: #94a3b8; margin-bottom: 15px; }
        .empty-state h4 { color: #334155; font-weight: 700; font-size: 18px; margin-bottom: 5px; }
        .empty-state p { color: #64748b; font-size: 14px; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <SweetSoft:Navigation runat="server" ID="Navigation1"/>
            <div class="page-header">
                <div class="page-title">
                    <h2><%= GetResourceText(BackEndResourceKeys.PROJECT_REPORT) %></h2>
                    <p>Tổng hợp số liệu công việc hoàn thành, cảnh báo trễ hạn và quản trị vấn đề phát sinh.</p>
                </div>
                <div class="btn-group">
                    <asp:LinkButton ID="btnExportPDF" runat="server" CssClass="btn-custom btn-red" OnClick="btnExportPDF_Click">
                        <i class="fas fa-file-pdf"></i> Xuất File PDF
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnExportExcel" runat="server" CssClass="btn-custom btn-green" OnClick="btnExportExcel_Click">
                        <i class="fas fa-file-excel"></i> Xuất File Excel
                    </asp:LinkButton>
                </div>
            </div>

            <asp:UpdatePanel ID="upReport" runat="server">
                <ContentTemplate>
                    
                    <!-- KHI DỰ ÁN CHƯA BẮT ĐẦU -->
                    <asp:PlaceHolder ID="phNotStarted" runat="server" Visible="false">
                        <div class="empty-state">
                            <i class="fas fa-folder-open"></i>
                            <h4>Dự án chưa bắt đầu</h4>
                            <p>Dự án này đang ở trạng thái chờ. Hệ thống chưa có dữ liệu tiến độ công việc để lập báo cáo.</p>
                        </div>
                    </asp:PlaceHolder>

                    <!-- KHI DỰ ÁN ĐANG LÀM HOẶC ĐÃ XONG -->
                    <asp:PlaceHolder ID="phReportContent" runat="server">
                        <div class="filter-card">
                            <div class="form-group-filter" style="min-width: 250px;">
                                <label>Kỳ báo cáo:</label>
                                <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged" onchange="toggleCustomDates(this.value)">
                                    <asp:ListItem Text="Tuần này" Value="THIS_WEEK"></asp:ListItem>
                                    <asp:ListItem Text="Tuần trước" Value="LAST_WEEK"></asp:ListItem>
                                    <asp:ListItem Text="Tháng này" Value="THIS_MONTH"></asp:ListItem>
                                    <asp:ListItem Text="Tháng trước" Value="LAST_MONTH"></asp:ListItem>
                                    <asp:ListItem Text="Toàn thời gian" Value="ALL" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Tùy chọn..." Value="CUSTOM"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            
                            <div class="form-group-filter custom-date-group" style="display: none;">
                                <label>Từ ngày:</label>
                                <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>
                            
                            <div class="form-group-filter custom-date-group" style="display: none;">
                                <label>Đến ngày:</label>
                                <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>

                            <asp:LinkButton ID="btnPreview" runat="server" CssClass="btn-custom btn-blue" OnClick="btnPreview_Click">
                                <i class="fas fa-search"></i> Lọc dữ liệu
                            </asp:LinkButton>
                        </div>

                        <div class="report-paper">
                            
                            <div class="report-header">
                                <h1>BÁO CÁO TIẾN ĐỘ DỰ ÁN</h1>
                                <div class="sub-date">
                                    <asp:Literal ID="ltrReportPeriod" runat="server"></asp:Literal>
                                </div>
                            </div>

                            <div class="kpi-summary-bar">
                                <div class="kpi-item">
                                    <span class="kpi-title">Tổng số Task (Trong kỳ):</span>
                                    <span class="kpi-number"><asp:Literal ID="ltrTotalTasks" runat="server">0</asp:Literal> công việc</span>
                                </div>
                                <div class="kpi-item">
                                    <span class="kpi-title">Đã hoàn thành:</span>
                                    <span class="kpi-number success"><asp:Literal ID="ltrCompletedTasks" runat="server">0</asp:Literal></span>
                                </div>
                                <div class="kpi-item">
                                    <span class="kpi-title">Trễ hạn / Tắc nghẽn:</span>
                                    <span class="kpi-number danger"><asp:Literal ID="ltrOverdueTasks" runat="server">0</asp:Literal></span>
                                </div>
                                <div class="kpi-item">
                                    <span class="kpi-title">Vấn đề phát sinh:</span>
                                    <span class="kpi-number warning"><asp:Literal ID="ltrTotalIssues" runat="server">0</asp:Literal> vấn đề</span>
                                </div>
                            </div>

                            <div class="section-title">1. Tổng hợp Task đã hoàn thành</div>
                            <table class="report-table">
                                <thead>
                                    <tr>
                                        <th width="90">Mã Task</th>
                                        <th>Tên công việc</th>
                                        <th width="150">Người thực hiện</th>
                                        <th width="120" class="text-center">Ngày hoàn thành</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptCompletedTasks" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><strong><%# Eval("MaCongViec") %></strong></td>
                                                <td><%# Eval("TenCongViec") %></td>
                                                <td><%# Eval("Assignee") %></td>
                                                <td class="text-center"><span class="report-badge badge-success"><%# Eval("NgayHoanThanhThucTe", "{0:dd/MM/yyyy}") %></span></td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:PlaceHolder runat="server" Visible='<%# rptCompletedTasks.Items.Count == 0 %>'>
                                                <tr><td colspan="4" class="text-center text-muted py-3">Không có công việc nào hoàn thành trong kỳ này.</td></tr>
                                            </asp:PlaceHolder>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>

                            <div class="section-title" style="color: #b91c1c;">2. Danh sách Task bị trễ hạn / Cần chú ý</div>
                            <table class="report-table">
                                <thead>
                                    <tr>
                                        <th width="90">Mã Task</th>
                                        <th>Tên công việc</th>
                                        <th width="150">Người phụ trách</th>
                                        <th width="110" class="text-center">Hạn chót</th>
                                        <th width="160" class="text-center">Tình trạng trễ</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptOverdueTasks" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><strong><%# Eval("MaCongViec") %></strong></td>
                                                <td><%# Eval("TenCongViec") %></td>
                                                <td><%# Eval("Assignee") %></td>
                                                <td class="text-center"><%# Eval("NgayKetThuc", "{0:dd/MM/yyyy}") %></td>
                                                <td class="text-center">
                                                    <span class="report-badge badge-danger">Trễ <%# Eval("DaysOverdue") %> ngày</span>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:PlaceHolder runat="server" Visible='<%# rptOverdueTasks.Items.Count == 0 %>'>
                                                <tr><td colspan="5" class="text-center text-muted py-3">Tuyệt vời! Không có công việc nào bị trễ hạn.</td></tr>
                                            </asp:PlaceHolder>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>

                            <div class="section-title">3. Các Vấn đề / Issue phát sinh</div>
                            <table class="report-table">
                                <thead>
                                    <tr>
                                        <th width="80">Mã</th>
                                        <th>Tiêu đề vấn đề (Issue)</th>
                                        <th width="100" class="text-center">Mức độ</th>
                                        <th width="110" class="text-center">Trạng thái</th>
                                        <th>Hướng xử lý / Ghi chú</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptIssues" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><strong><%# Eval("MaVanDe") %></strong></td>
                                                <td><%# Eval("TenVanDe") %></td>
                                                <td class="text-center">
                                                    <span class="report-badge <%# GetPriorityBadge(Convert.ToInt32(Eval("MucDoAnhHuong"))) %>"><%# GetPriorityText(Convert.ToInt32(Eval("MucDoAnhHuong"))) %></span>
                                                </td>
                                                <td class="text-center">
                                                    <span class="report-badge <%# GetIssueStatusBadge(Convert.ToInt32(Eval("TrangThai"))) %>"><%# GetIssueStatusText(Convert.ToInt32(Eval("TrangThai"))) %></span>
                                                </td>
                                                <td><%# Eval("KeHoachXuLy") %></td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:PlaceHolder runat="server" Visible='<%# rptIssues.Items.Count == 0 %>'>
                                                <tr><td colspan="5" class="text-center text-muted py-3">Không có vấn đề phát sinh.</td></tr>
                                            </asp:PlaceHolder>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>

                        </div>
                    </asp:PlaceHolder>

                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server"></asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        function toggleCustomDates(val) {
            var dateGroups = document.querySelectorAll('.custom-date-group');
            if (val === 'CUSTOM') {
                dateGroups.forEach(el => el.style.display = 'flex');
            } else {
                dateGroups.forEach(el => el.style.display = 'none');
            }
        }

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            var ddl = document.getElementById('<%= ddlPeriod.ClientID %>');
            if (ddl) toggleCustomDates(ddl.value);
        });
    </script>
</asp:Content>