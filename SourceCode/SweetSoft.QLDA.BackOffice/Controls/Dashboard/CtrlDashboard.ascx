<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlDashboard.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Dashboard.CtrlDashboard" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card mt-3">
    <div class="card-body p-0">
        <div class="d-flex flex-wrap align-items-center mb-1">
            <h4 class="card-title mb-0 flex-grow-1"><%=GetResourceText(BackEndResourceKeys.RECENT_ACTIVITY_LOG) %></h4>
            <div class="ms-auto">
                <a href="<%= SweetSoft.QLDA.BackOffice.Common.RewriteURLHelper.AuditLogs %>" title="<%= GetResourceText(BackEndResourceKeys.DETAIL) %>" class="btn btn-primary btn-sm">
                    <%= GetResourceText(BackEndResourceKeys.DETAIL) %><i class="fas fa-angle-double-right ms-1"></i>
                </a>
            </div>
        </div>
        <div class="table-responsive">
            <table class="table table-sm table-bordered mb-0">
                <thead>
                    <tr class="table-warning">
                        <th class="text-center">IP</th>
                        <th class="text-center"><%= GetResourceText(BackEndResourceKeys.ACCOUNT) %></th>
                        <th class="text-center"><%= GetResourceText(BackEndResourceKeys.FUNCTION) %></th>
                        <th class="text-center"><%=GetResourceText(BackEndResourceKeys.DATE) %></th>
                        <th class="text-center"><%= GetResourceText(BackEndResourceKeys.BROWSER) %></th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Literal runat="server" ID="ltrAudits" EnableViewState="false"></asp:Literal>
                </tbody>
            </table>
        </div>
    </div>
</div>
<div runat="server" id="itemTemplate" visible="false" enableviewstate="false">
    <tr>
        <td><a href="https://whatismyipaddress.com/ip/{0}" target="_blank" title="{0}">{0}</a></td>
        <td>{1}</td>
        <td>{2}</td>
        <td class="text-end">{3}</td>
        <td>{4}</td>
    </tr>
</div>
<div runat="server" id="itemTemplateInvoice" visible="false" enableviewstate="false">
    <tr>
        <td>{0}</td>
        <td><a href="/Invoice/{1}" target="_blank" title="{2}">{2}</a></td>
        <td>{3}</td>
        <td>{4}</td>
        <td class="text-center">{5}</td>
    </tr>
</div>

<div runat="server" id="itemTemplateSell" visible="false" enableviewstate="false">
    <tr>
        <td>{0}</td>
        <td><a href="/Sell/{1}" target="_blank" title="{2}">{2}</a></td>
        <td>{3}</td>
        <td>{4}</td>
        <td>{5}</td>
        <td class="text-center">{6}</td>
    </tr>
</div>
