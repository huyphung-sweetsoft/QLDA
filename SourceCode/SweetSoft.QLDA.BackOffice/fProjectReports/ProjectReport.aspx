<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="ProjectReport.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectReports.ProjectReport" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .page-title h2 { font-size: 20px; color: #1e293b; font-weight: bold; margin: 10px 0 25px 0; text-transform: uppercase; }

        .report-layout { display: flex; flex-direction: column; gap: 25px; align-items: flex-start; }
        @media (min-width: 1200px) { .report-layout { flex-direction: row; } }

        .sidebar-left { width: 100%; display: flex; flex-direction: column; gap: 15px; }
        @media (min-width: 1200px) { .sidebar-left { width: 220px; flex-shrink: 0; position: sticky; top: 20px; } }

        .form-group-vertical { display: flex; flex-direction: column; gap: 6px; }
        .form-group-vertical label { font-size: 13.5px; font-weight: 600; color: #475569; margin: 0; }
        .form-group-vertical .form-control { width: 100%; padding: 8px 12px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 13.5px; outline: none; transition: border-color 0.2s; }
        .form-group-vertical .form-control:focus { border-color: #3b82f6; }

        .btn-filter { width: 100%; background-color: #2563eb; color: white; padding: 10px; border-radius: 6px; font-weight: 600; font-size: 14px; border: none; transition: background 0.2s; text-align: center; text-decoration: none; display: flex; justify-content: center; align-items: center; gap: 8px; }
        .btn-filter:hover { background-color: #1d4ed8; color: white; }

        .main-center { flex-grow: 1; width: 100%; min-width: 0; }

        .sidebar-right { width: 100%; display: flex; flex-direction: column; gap: 15px; }
        @media (min-width: 1200px) { .sidebar-right { width: 180px; flex-shrink: 0; position: sticky; top: 20px; } }

        .btn-export { width: 100%; display: flex; align-items: center; justify-content: center; gap: 8px; padding: 12px 15px; border-radius: 8px; font-size: 15px; font-weight: 600; border: none; transition: all 0.2s; text-decoration: none; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .btn-export-pdf { background-color: #ef4444; color: white; }
        .btn-export-pdf:hover { background-color: #dc2626; color: white; transform: translateY(-2px); box-shadow: 0 6px 12px rgba(220,38,38,0.25); }
        .btn-export-excel { background-color: #10b981; color: white; }
        .btn-export-excel:hover { background-color: #059669; color: white; transform: translateY(-2px); box-shadow: 0 6px 12px rgba(16,185,129,0.25); }
        .btn-export i { font-size: 18px; }

        .report-paper { background: #ffffff; border-radius: 12px; box-shadow: 0 10px 40px -10px rgba(0,0,0,0.08), 0 1px 3px rgba(0,0,0,0.05); padding: 45px 40px; width: 100%; border: 1px solid #f1f5f9; position: relative; }
        .report-paper::before { content: ''; position: absolute; top: 0; left: 0; right: 0; height: 5px; background: linear-gradient(90deg, #3b82f6, #8b5cf6); border-radius: 12px 12px 0 0; }
        
        .report-header { text-align: center; margin-bottom: 35px; padding-bottom: 20px; border-bottom: 1px dashed #cbd5e1; }
        .report-header h1 { font-size: 24px; color: #0f172a; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; margin: 0 0 8px 0; }
        .report-header .sub-date { font-size: 14px; color: #64748b; font-weight: 500; font-style: italic; }

        .kpi-summary-bar { display: flex; flex-wrap: wrap; justify-content: space-between; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 10px; padding: 20px 15px; margin-bottom: 40px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); gap: 15px;}
        .kpi-item { flex: 1; text-align: center; min-width: 120px; border-right: 1px solid #e2e8f0; padding: 0 10px; }
        .kpi-item:last-child { border-right: none; }
        .kpi-title { font-size: 12px; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; display: block; margin-bottom: 8px; }
        .kpi-number { font-size: 24px; font-weight: 800; display: block; line-height: 1.1; }
        
        .kpi-number.total { color: #3b82f6; }
        .kpi-number.success { color: #10b981; }
        .kpi-number.danger { color: #ef4444; }
        .kpi-number.warning { color: #f59e0b; }

        .section-title { font-size: 16px; font-weight: 700; color: #0f172a; margin-top: 35px; margin-bottom: 15px; padding-left: 12px; border-left: 4px solid #3b82f6; line-height: 1.2; }
        .section-title.danger { border-left-color: #ef4444; color: #b91c1c; }
        .section-title.warning { border-left-color: #f59e0b; color: #b45309; }

        /* ÉP BẢNG CHẾT CHIỀU RỘNG, KHÔNG CHO TẠO THANH CUỘN */
        .report-paper .table { table-layout: fixed; width: 100%; border-color: #e2e8f0; margin-bottom: 0; word-wrap: break-word; }
        .report-paper .table thead th { 
            background-color: #f8fafc; color: #475569; font-weight: 700; text-transform: uppercase; font-size: 12px; 
            letter-spacing: 0.5px; padding: 14px 12px; border-bottom: 2px solid #cbd5e1; vertical-align: middle; 
            white-space: normal; word-break: break-word; 
        }
        .report-paper .table tbody td { 
            padding: 14px 12px; color: #334155; vertical-align: middle; font-size: 13.5px; 
            white-space: normal !important; word-break: break-word; line-height: 1.5; 
        }
        .report-paper .table tbody tr:hover { background-color: #f8fafc; }

        .report-badge { padding: 5px 10px; border-radius: 6px; font-size: 12px; font-weight: 600; display: inline-block; white-space: nowrap; line-height: 1.2; }
        .badge-success { background-color: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; }
        .badge-danger { background-color: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
        .badge-warning { background-color: #fffbeb; color: #d97706; border: 1px solid #fde68a; }
        .badge-high { background-color: #fef2f2; color: #991b1b; }
        
        .empty-state { text-align: center; padding: 60px 20px; border-radius: 8px; border: 1px dashed #cbd5e1; margin-top: 20px; }
        .empty-state i { font-size: 50px; color: #94a3b8; margin-bottom: 15px; }
        .empty-state h4 { color: #334155; font-weight: 700; font-size: 18px; margin-bottom: 5px; }
        .empty-state p { color: #64748b; font-size: 14px; margin:0; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-4 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                
                <asp:UpdatePanel ID="upReport" runat="server">
                    <Triggers>
                        <asp:PostBackTrigger ControlID="btnExportPDF" />
                        <asp:PostBackTrigger ControlID="btnExportExcel" />
                    </Triggers>
                    <ContentTemplate>
                        
                        <div class="report-layout">
                            
                            <div class="sidebar-left">
                                <div class="form-group-vertical">
                                    <label><%= GetResourceText(BackEndResourceKeys.TIME_PERIOD) %>:</label>
                                    <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="form-control" 
                                        AutoPostBack="true" 
                                        OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged" 
                                        onchange="toggleCustomDates(this.value)">
                                    </asp:DropDownList>
                                </div>
                                
                                <div class="form-group-vertical custom-date-group" style="display: none;">
                                    <label><%= GetResourceText(BackEndResourceKeys.FROM_DATE) %>:</label>
                                    <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                </div>
                                
                                <div class="form-group-vertical custom-date-group mb-2" style="display: none;">
                                    <label><%= GetResourceText(BackEndResourceKeys.TO_DATE) %>:</label>
                                    <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                </div>

                                <asp:LinkButton ID="btnPreview" runat="server" CssClass="btn-filter" OnClick="btnPreview_Click">
                                    <i class="fas fa-search"></i> <%= GetResourceText(BackEndResourceKeys.GENERATE_REPORT) %>
                                </asp:LinkButton>
                            </div>

                            <div class="main-center">
                                <asp:PlaceHolder ID="phNotStarted" runat="server" Visible="false">
                                    <div class="empty-state">
                                        <i class="fas fa-folder-open"></i>
                                        <h4><%= GetResourceText(BackEndResourceKeys.PROJECT_NOT_STARTED) %></h4>
                                        <p><%= GetResourceText(BackEndResourceKeys.PROJECT_HAS_NO_DATA) %></p>
                                    </div>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="phReportContent" runat="server">
                                    <div class="report-paper">
                                        
                                        <div class="report-header">
                                            <h1><%= GetResourceText(BackEndResourceKeys.PROJECT_REPORT) %></h1>
                                            <div class="sub-date">
                                                <asp:Literal ID="ltrReportPeriod" runat="server"></asp:Literal>
                                            </div>
                                        </div>

                                        <div class="kpi-summary-bar">
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.TOTAL_TASKS) %></span>
                                                <span class="kpi-number total"><asp:Literal ID="ltrTotalTasks" runat="server">0</asp:Literal></span>
                                            </div>
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.COMPLETED) %></span>
                                                <span class="kpi-number success"><asp:Literal ID="ltrCompletedTasks" runat="server">0</asp:Literal></span>
                                            </div>
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %></span>
                                                <span class="kpi-number danger"><asp:Literal ID="ltrOverdueTasks" runat="server">0</asp:Literal></span>
                                            </div>
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.ISSUES_ARISING) %></span>
                                                <span class="kpi-number warning"><asp:Literal ID="ltrTotalIssues" runat="server">0</asp:Literal></span>
                                            </div>
                                        </div>

                                        <div class="section-title">1. <%= GetResourceText(BackEndResourceKeys.COMPLETED_TASKS_SUMMARY) %></div>
                                        <div class="mb-4">
                                            <table class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="12%"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                        <th width="38%" class="text-start"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                        <th width="35%" class="text-start"><%= GetResourceText(BackEndResourceKeys.ASSIGNEE) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.COMPLETION_DATE) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptCompletedTasks" runat="server">
                                                        <ItemTemplate>
                                                            <tr>
                                                                <td class="text-center"><strong><%# Eval("MaCongViec") %></strong></td>
                                                                <td class="text-start"><%# Eval("TenCongViec") %></td>
                                                                <td class="text-start"><%# string.IsNullOrEmpty(Convert.ToString(Eval("Assignee"))) ? "—" : Eval("Assignee") %></td>
                                                                <td class="text-center"><span class="report-badge badge-success"><%# Eval("NgayHoanThanhThucTe", "{0:dd/MM/yyyy}") %></span></td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptCompletedTasks.Items.Count == 0 %>'>
                                                                <tr><td colspan="4" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_COMPLETED_TASKS_IN_PERIOD) %></td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>

                                        <div class="section-title danger">2. <%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS_LIST) %></div>
                                        <div class="mb-4">
                                            <table class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="12%"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                                        <th width="35%" class="text-start"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                                        <th width="23%" class="text-start"><%= GetResourceText(BackEndResourceKeys.ASSIGNEE) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.DEADLINE) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptOverdueTasks" runat="server">
                                                        <ItemTemplate>
                                                            <tr>
                                                                <td class="text-center"><strong><%# Eval("MaCongViec") %></strong></td>
                                                                <td class="text-start"><%# Eval("TenCongViec") %></td>
                                                                <td class="text-start"><%# string.IsNullOrEmpty(Convert.ToString(Eval("Assignee"))) ? "—" : Eval("Assignee") %></td>
                                                                <td class="text-center fw-bold text-danger"><%# Eval("NgayKetThuc", "{0:dd/MM/yyyy}") %></td>
                                                                <td class="text-center">
                                                                    <span class="report-badge badge-danger"><%= GetResourceText(BackEndResourceKeys.OVERDUE) %> <%# Eval("DaysOverdue") %> <%= GetResourceText(BackEndResourceKeys.DAY) %></span>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptOverdueTasks.Items.Count == 0 %>'>
                                                                <tr><td colspan="5" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_OVERDUE_TASKS) %></td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>

                                        <div class="section-title warning">3. <%= GetResourceText(BackEndResourceKeys.ISSUES_LIST) %></div>
                                        <div class="mb-2">
                                            <table class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="12%"><%= GetResourceText(BackEndResourceKeys.CODE) %></th>
                                                        <th width="33%" class="text-start"><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                        <th width="25%" class="text-start"><%= GetResourceText(BackEndResourceKeys.HANDLING_PLAN) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptIssues" runat="server">
                                                        <ItemTemplate>
                                                            <tr>
                                                                <td class="text-center"><strong><%# Eval("MaVanDe") %></strong></td>
                                                                <td class="text-start"><%# Eval("TenVanDe") %></td>
                                                                <td class="text-center">
                                                                    <span class="report-badge <%# GetPriorityBadge(Convert.ToInt32(Eval("MucDoAnhHuong"))) %>"><%# GetPriorityText(Convert.ToInt32(Eval("MucDoAnhHuong"))) %></span>
                                                                </td>
                                                                <td class="text-center">
                                                                    <span class="report-badge <%# GetIssueStatusBadge(Convert.ToInt32(Eval("TrangThai"))) %>"><%# GetIssueStatusText(Convert.ToInt32(Eval("TrangThai"))) %></span>
                                                                </td>
                                                                <td class="text-start"><%# Eval("KeHoachXuLy") %></td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptIssues.Items.Count == 0 %>'>
                                                                <tr><td colspan="5" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_ISSUES) %></td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>

                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <div class="sidebar-right">
                                <asp:LinkButton ID="btnExportPDF" runat="server" CssClass="btn-export btn-export-pdf" OnClick="btnExportPDF_Click">
                                    <i class="fas fa-file-pdf"></i> <%= GetResourceText(BackEndResourceKeys.EXPORT_PDF) %>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnExportExcel" runat="server" CssClass="btn-export btn-export-excel" OnClick="btnExportExcel_Click">
                                    <i class="fas fa-file-excel"></i> <%= GetResourceText(BackEndResourceKeys.EXPORT_EXCEL) %>
                                </asp:LinkButton>
                            </div>

                        </div> 
                    </ContentTemplate>
                </asp:UpdatePanel>

            </div>
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