<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlBreadcrumb.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.Breadcrumb.CtrlBreadcrumb" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style>
.breadcrumb-title-actions{display:flex;align-items:center;flex-wrap:wrap;gap:9px;min-width:0;}
.project-functions-toggle{display:none;align-items:center;justify-content:center;gap:7px;min-height:32px;padding:6px 10px;border:1px solid #e0d5f2;border-radius:8px;color:#54218d;background:#f7f2ff;font-size:12.5px;font-weight:650;line-height:1;white-space:nowrap;cursor:pointer;transition:background-color .16s ease,border-color .16s ease,color .16s ease;}
.project-functions-toggle:hover{color:#3f176f;background:#eee4ff;border-color:#c9b2ef;}
.project-functions-toggle:focus-visible{outline:none;box-shadow:0 0 0 3px rgba(108,70,180,.16);}
.project-functions-toggle .project-functions-chevron{font-size:10px;transition:transform .18s ease;}
.project-functions-toggle[aria-expanded="true"] .project-functions-chevron{transform:rotate(180deg);}
@media(max-width:575.98px){.breadcrumb-title-actions{gap:7px;}.project-functions-toggle{min-height:30px;padding:5px 8px;font-size:12px;}}
</style>
<div>
    <asp:UpdatePanel runat="server" ID="pnlNavigator" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="flex-wrap flex-between gap-3 mb-2">
                <div class="breadcrumb-title-actions">
                    <button type="button" id="btnProjectFunctions" class="project-functions-toggle" onclick="return toggleProjectFunctions();" aria-expanded="false" aria-controls="projectFeatureTabs" aria-label="Mở hoặc ẩn danh sách chức năng dự án" title="Mở hoặc ẩn danh sách chức năng dự án">
                        <i class="fas fa-layer-group" aria-hidden="true"></i>
                        <span>Chức năng dự án</span>
                        <i class="fas fa-chevron-down project-functions-chevron" aria-hidden="true"></i>
                    </button>
                    <h3 class="title mb-0 fs-5 text-primary"><%= MainTitle %></h3>
                </div>
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
<script type="text/javascript">
    (function () {
        var projectFunctionsStorageKey = 'SweetSoft_ProjectFunctionsOpen_v1';
        function readProjectFunctionsState() {
            try { return localStorage.getItem(projectFunctionsStorageKey) === 'true'; }
            catch (error) { return false; }
        }
        function saveProjectFunctionsState(isOpen) {
            try { localStorage.setItem(projectFunctionsStorageKey, isOpen ? 'true' : 'false'); }
            catch (error) { }
        }
        function syncProjectFunctionsButton() {
            var button = document.getElementById('btnProjectFunctions');
            var tabs = document.getElementById('projectFeatureTabs') || document.querySelector('.project-feature-tabs');
            if (!button) return;
            if (!tabs) {
                button.style.display = 'none';
                return;
            }
            var isOpen = readProjectFunctionsState();
            tabs.classList.toggle('project-tabs-open', isOpen);
            button.style.display = 'inline-flex';
            button.setAttribute('aria-controls', tabs.id || 'projectFeatureTabs');
            button.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        }
        window.toggleProjectFunctions = function () {
            var button = document.getElementById('btnProjectFunctions');
            var tabs = document.getElementById('projectFeatureTabs') || document.querySelector('.project-feature-tabs');
            if (!button || !tabs) return false;
            var isOpen = !tabs.classList.contains('project-tabs-open');
            tabs.classList.toggle('project-tabs-open', isOpen);
            button.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            saveProjectFunctionsState(isOpen);
            return false;
        };
        if (window.Sys && Sys.Application) Sys.Application.add_load(syncProjectFunctionsButton);
        else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', syncProjectFunctionsButton);
        else syncProjectFunctionsButton();
    })();
</script>
    