<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlBreadcrumb.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Breadcrumb.CtrlBreadcrumb" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div>
    <asp:UpdatePanel runat="server" ID="pnlNavigator" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="flex-wrap flex-between gap-3 mb-2">
                <h3 class="title mb-0 fs-5 text-primary"><%= MainTitle %></h3>
                <div runat="server" id="divAlert" visible="false" class="custom-alert alert-info" role="alert">
                    <%= Alert %>
                </div>
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb mb-0">
                        <li class="breadcrumb-item"><a class="text-page" href="<%= GetRelativeClientPath("/") %>"><%= GetResourceText(BackEndResourceKeys.DASHBOARD) %></a></li>
                        <asp:Literal runat="server" ID="ltrNavigator" EnableViewState="false"></asp:Literal>
                    </ol>
                </nav>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<ul runat="server" id="itemTemplate" visible="false">
    <li class="breadcrumb-item {2}">
        <a href="{0}" title="{1}">{1}</a>
    </li>
</ul>
