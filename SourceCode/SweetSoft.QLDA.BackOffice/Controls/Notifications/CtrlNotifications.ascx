<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlNotifications.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Notifications.CtrlNotifications" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="dropdown d-inline-block">
    <button type="button" class="btn header-item noti-icon position-relative" id="page-header-notifications-dropdown"
        data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
        <i data-feather="bell" class="icon-lg"></i>
        <span class="badge bg-danger rounded-pill" runat="server" id="tagTotalNotification"></span>
    </button>
    <div class="dropdown-menu dropdown-menu-lg dropdown-menu-end p-0"
        aria-labelledby="page-header-notifications-dropdown">
        <div class="p-3">
            <div class="row align-items-center">
                <div class="col">
                    <h6 class="m-0 text-white"><%= GetResourceText(BackEndResourceKeys.NOTIFICATION) %></h6>
                </div>
            </div>
        </div>
        <div data-simplebar style="max-height: 230px;">
            <asp:Literal runat="server" ID="ltrNotification" EnableViewState="false"></asp:Literal>
        </div>
        <div class="p-2 border-top d-grid">
            <a class="btn btn-sm btn-link font-size-14 text-center" href="javascript:void(0)">
                <i class="mdi mdi-arrow-right-circle me-1"></i><span class="text-white"><%= GetResourceText(BackEndResourceKeys.VIEW_MORE) %></span>
            </a>
        </div>
    </div>
</div>
<div runat="server" id="itemTemplate" visible="false" enableviewstate="false">
    <a href="{0}" class="text-reset notification-item">
        <div class="d-flex">
            <div class="flex-shrink-0 avatar-sm me-3">
                <span class="avatar-title bg-success rounded-circle font-size-16">
                    <i class="fas fa-file-invoice"></i>
                </span>
            </div>
            <div class="flex-grow-1">
                <h6 class="mb-1 {1} text-white">{2}</h6>
                <div class="font-size-13 text-muted">
                    <p class="mb-1 text-white">{3}</p>
                    <p class="mb-1 text-white">{4}</p>
                    <p class="mb-0"><i class="mdi mdi-clock-outline me-1"></i><span class="text-white">{5}</span></p>
                </div>
            </div>
        </div>
    </a>
</div>
