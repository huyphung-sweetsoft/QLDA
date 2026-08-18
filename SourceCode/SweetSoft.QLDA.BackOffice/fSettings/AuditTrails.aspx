<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="AuditTrails.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.AuditTrails" %>

<%----------------------PROGRAMER LOGS------------------------
Created by: 
**Change 01: Truong, 29 Oct 2024 - Update UI
--%>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        table li {
            white-space: break-spaces;
        }

        .table-fixed-column td:last-child, .table-fixed-column th:last-child {
            position: unset !important;
            --bs-table-bg-type: initial !important;
            background-color: unset !important;
        }

        @media (max-width: 768px) {
            .grid-search {
                width: 100% !important;
            }

                .grid-search .d-inline-block {
                    display: block !important;
                }

            .d-inline-block div:first-child {
                margin-bottom: 5px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Audit logs" />
                <div class="card-header">
                    <div class="d-flex flex-wrap flex-xl-row gap-3">
                        <div class="d-block w-50 grid-search">
                            <div class="d-inline-block" style="min-width: 100px">
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlYear" PlaceHolder="Chọn năm" AlowClear="false"></SweetSoft:ExtraDropdown>
                            </div>
                            <div class="d-inline-block">
                                <div class="input-group">
                                    <a class="btn btn-primary font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                                        <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
                                    </a>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Enter the keyword search..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                                    <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div class="listSearchTagBox">
                        <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                                AllowSorting="true"
                                AutoGenerateColumns="false"
                                CssClass="table-bordered table-audit"
                                TableCustomClass="table-custom"
                                FocusBtnIcon="fas fa-compress-arrows-alt"
                                DataKeyNames="Id" GridLines="None"
                                OnNeedDataSource="grvData_NeedDataSource"
                                OnRowCommand="grvData_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="IP" HeaderStyle-CssClass="text-center" SortExpression="IpAddress" HeaderStyle-Width="120px">
                                        <ItemTemplate>
                                            <a href="https://whatismyipaddress.com/ip/<%# Eval("IpAddress")%>" target="_blank" title="<%# Eval("IpAddress")%>"><%# Eval("IpAddress")%></a>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Date" HeaderStyle-CssClass="text-center" SortExpression="ChangedAt" HeaderStyle-Width="150px">
                                        <ItemTemplate>
                                            <%# ConvertDateTimeToString(Eval("ChangedAt")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Account" HeaderStyle-CssClass="text-center" SortExpression="ChangedBy">
                                        <ItemTemplate>
                                            <%# this.DisplayName(Eval("ChangedBy")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="ActionType">
                                        <ItemTemplate>
                                            <%# SweetSoft.QLDA.Core.SysManager.LogActions.GetFullTag(Eval("ActionType").ToString()) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Function" HeaderStyle-CssClass="text-center" SortExpression="Title">
                                        <ItemTemplate>
                                            <%# Eval("Title") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Content" HeaderStyle-CssClass="text-center" SortExpression="Changes">
                                        <ItemTemplate>
                                            <div class="text-break" style="max-height: 40px; max-width: 250px; overflow: hidden">
                                                <%# Eval("Changes") %>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Browser" HeaderStyle-CssClass="text-center" SortExpression="UserAgent">
                                        <ItemTemplate>
                                            <%# Eval("UserAgent") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                                </EmptyDataTemplate>
                            </SweetSoft:GridviewExtension>
                            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
        <div class="offcanvas-header">
            <div class="flex flex-column flex-md-row align-items-center gap-3">
                <h5 class="offcanvas-title"><%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %></h5>
                <div class="d-flex align-items-center gap-3">
                    <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Success" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">Search</SweetSoft:ExtraButton>
                    <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="Warning" ButtonIcon="Refresh" OnClick="btnCancel_Click">Refresh</SweetSoft:ExtraButton>
                </div>
            </div>
            <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
        </div>
        <div class="div offcanvas-body pt-0">
            <div class="card shadow-none card-body text-muted mb-0">
                <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel runat="server" ID="pnlSearchPopup" DefaultButton="lbtSearchAdvanced">
                            <div class="row rowItem">
                                <div class="col-md-6 mb-4">
                                    <label class="form-label"><%=GetResourceText(BackEndResourceKeys.DATE) %></label>
                                    <SweetSoft:ExtraDateTime runat="server" ID="txtSearchDate" SearchColumn="ChangedAt" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                                </div>
                                <div class="col-md-6 mb-4">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.IP_ADDRESS) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSearchIPAddress" SearchColumn="IPAddress" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-4">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ACCOUNT) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchUser" PlaceHolder="Enter the value" SearchColumn="ChangedBy" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-6 mb-4">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ACTION) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchAction" PlaceHolder="Enter the value" SearchColumn="ActionType" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbtSearchSingle" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpModalMain" runat="server">
    <div class="modal fade" data-bs-backdrop="static" id="modalDelete" tabindex="-1" role="dialog"
        aria-labelledby="modalExport" aria-hidden="true">
        <div class="modal-dialog modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header modal-warning">
                    <h5 class="modal-title">Clear access logs</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-sm-6 col-xs-12">
                            <div class="mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DATE) %>:</label>
                                <SweetSoft:ExtraDateTime runat="server" ID="txtTimeDelete" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer p-2">
                    <SweetSoft:ExtraButton runat="server" ID="lbtConfirm" CssClass="btn btn-info waves-effect waves-light" OnClick="lbtConfirm_Click" ButtonStyle="Danger" ButtonIcon="Accept">Confirm</SweetSoft:ExtraButton>
                    <button type="button" class="btn btn-secondary waves-effect" data-bs-dismiss="modal">
                        <%= GetResourceText(BackEndResourceKeys.CLOSE) %>
                    </button>
                </div>
            </div>
        </div>
    </div>
    <div class="modal fade" data-bs-backdrop="static" id="modalViewLog" tabindex="-1" role="dialog" aria-labelledby="modalViewLog" aria-hidden="true">
        <div class="modal-dialog modal-dialog-scrollable modal-fullscreen">
            <div class="modal-content">
                <div class="modal-header modal-info">
                    <h5 class="modal-title"><%= GetResourceText(BackEndResourceKeys.DETAIL) %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-md-6 col-sm-12">
                            <div class="row">
                                <div class="col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label d-inline-block"><%= GetResourceText(BackEndResourceKeys.DATE) %>:</label>
                                        <span class="d-inline-block ms-1 js-log-date"></span>
                                    </div>
                                </div>
                                <div class="col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label d-inline-block"><%=GetResourceText(BackEndResourceKeys.IP_ADDRESS) %>:</label>
                                        <span class="d-inline-block ms-1 js-log-ipaddress"></span>
                                    </div>
                                </div>
                                <div class="col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label d-inline-block"><%= GetResourceText(BackEndResourceKeys.ACCOUNT) %>:</label>
                                        <span class="d-inline-block ms-1 js-log-account"></span>
                                    </div>
                                </div>
                                <div class="col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label d-inline-block"><%= GetResourceText(BackEndResourceKeys.FUNCTION) %>:</label>
                                        <span class="d-inline-block ms-1 js-log-func"></span>
                                    </div>
                                </div>
                                <div class="col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label d-inline-block"><%= GetResourceText(BackEndResourceKeys.ACTION) %>:</label>
                                        <span class="d-inline-block ms-1 js-log-action"></span>
                                    </div>
                                </div>
                                <div class="col-sm-12 col-xs-12">
                                    <div class="mb-3">
                                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.BROWSER) %>:</label><br />
                                        <span class="d-inline-block ms-1 js-log-agent"></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6 col-sm-12">
                            <div class="mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTENT) %>:</label><br />
                                <div class="js-log-changes"></div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer p-2">
                    <button type="button" class="btn btn-secondary waves-effect" data-bs-dismiss="modal">
                        <%= GetResourceText(BackEndResourceKeys.CLOSE) %>
                    </button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script src="../Styles/js/jquery.json-viewer.js"></script>
    <script>
        const LogJs = {};
        LogJs.OpenModal = () => {
            $("#modalDelete").modal('show');
            return false;
        };
        LogJs.CloseModal = () => {
            $("#modalDelete").modal('hide');
            return false;
        }
        LogJs.bindEvent = () => {
            $('#<%= grvData.ClientID%> tbody tr').click(function () {
                const $ipAddress = $(this).find('td').eq(1).html();
                const $date = $(this).find('td').eq(2).text();
                const $account = $(this).find('td').eq(3).text();
                const $action = $(this).find('td').eq(4).html();
                const $func = $(this).find('td').eq(5).text();
                const $changes = $(this).find('td').eq(6).text().trim();
                const $agent = $(this).find('td').eq(7).text();

                $('.js-log-ipaddress').html($ipAddress);
                $('.js-log-date').text($date);
                $('.js-log-account').text($account);
                $('.js-log-action').html($action);
                $('.js-log-func').text($func);
                if ($changes) {
                    const raw = JSON.parse($changes);
                    const parsed = LogJs.tryParseJSONDeep(raw);
                    $('.js-log-changes').jsonViewer(parsed, { collapsed: false, withQuotes: true });
                }
                else
                    $('.js-log-changes').text('')
                $('.js-log-agent').text($agent);

                $("#modalViewLog").modal('show');
            });
        }
        LogJs.tryParseJSONDeep = (obj) => {
            if (typeof obj === 'string') {
                try {
                    const parsed = JSON.parse(obj);
                    if (typeof parsed === 'object' && parsed !== null) {
                        return LogJs.tryParseJSONDeep(parsed);
                    }
                    return parsed;
                } catch (e) {
                    return obj; 
                }
            } else if (typeof obj === 'object' && obj !== null) {
                for (const key in obj) {
                    if (obj.hasOwnProperty(key)) {
                        obj[key] = LogJs.tryParseJSONDeep(obj[key]);
                    }
                }
            }
            return obj;
        }

        LogJs.bindEvent();
        if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined')
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(LogJs.bindEvent);
    </script>
</asp:Content>
