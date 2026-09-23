using System;
using System.Collections.Generic;
using System.Web.UI;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class Dashboard : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ResolveRequestedModule(); }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            PreserveMenuOnPostBack();
            CtrlProjectTabs1.ProjectId = GetProjectIdFromQuery();
            LoadDashboard(PAGE_FUNCTION_CODE);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindNavigation();
            }
        }

        private Guid GetProjectIdFromQuery()
        {
            Guid projectId;
            return Guid.TryParse(Request.QueryString["project"], out projectId)
                ? projectId
                : Guid.Empty;
        }

        private void BindNavigation()
        {
            Guid projectId = GetProjectIdFromQuery();
            Navigation1.Visible = projectId != Guid.Empty;
            if (projectId == Guid.Empty)
                return;

            string dashboardTitle = GetDashboardTitle(PAGE_FUNCTION_CODE);

            Navigation1.MainTitle = dashboardTitle;
            Navigation1.keyValuePairUrls = new Dictionary<string, string>
            {
                {
                    GetRelativeClientPath(RewriteURLHelper.Projects),
                    GetResourceText(BackEndResourceKeys.PROJECT_LIST)
                },
                { "javascript:;", dashboardTitle }
            };
        }

        private string GetDashboardTitle(ModuleKeys module)
        {
            switch (module)
            {
                case ModuleKeys.DashboardProgress:
                    return GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS);
                case ModuleKeys.DashboardCost:
                    return GetResourceText(BackEndResourceKeys.DASHBOARD_COST);
                case ModuleKeys.DashboardResource:
                    return GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE);
                default:
                    return GetResourceText(BackEndResourceKeys.DASHBOARD_OVERVIEW);
            }
        }

        /// <summary>
        /// The master menu is rendered only on the first request and its
        /// Literal disables ViewState. Dashboard filters use a full postback,
        /// so enable ViewState for that Literal only while Dashboard.aspx is
        /// active. This keeps the shared MasterPage unchanged.
        /// </summary>
        private void PreserveMenuOnPostBack()
        {
            Control menu = FindControlRecursive(Master, "ltrMenu");
            if (menu != null)
            {
                menu.EnableViewState = true;
            }
        }

        private static Control FindControlRecursive(Control root, string id)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.ID, id, StringComparison.Ordinal))
            {
                return root;
            }

            foreach (Control child in root.Controls)
            {
                Control match = FindControlRecursive(child, id);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private ModuleKeys ResolveRequestedModule()
        {
            string type = (Request.QueryString["type"] ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            switch (type)
            {
                case "overview": return ModuleKeys.DashboardOverview;
                case "resource": return ModuleKeys.DashboardResource;
                case "progress": return ModuleKeys.DashboardProgress;
                case "cost": return ModuleKeys.DashboardCost;
                case "": return ResolveDefaultModule();
                default: return ModuleKeys.None;
            }
        }

        private static ModuleKeys ResolveDefaultModule()
        {
            ModuleKeys[] candidates =
            {
                ModuleKeys.DashboardOverview,
                ModuleKeys.DashboardResource,
                ModuleKeys.DashboardProgress,
                ModuleKeys.DashboardCost
            };

            Guid userId = SweetContext.Current.UserId;
            foreach (ModuleKeys candidate in candidates)
            {
                if (SweetContext.Current.CheckFunctionPermission(userId, candidate))
                {
                    return candidate;
                }
            }

            return ModuleKeys.None;
        }

        private void LoadDashboard(ModuleKeys module)
        {
            string controlPath;

            switch (module)
            {
                case ModuleKeys.DashboardOverview:
                    controlPath = "~/Controls/Dashboard/CtrlDashboardOverview.ascx";
                    break;
                case ModuleKeys.DashboardResource:
                    controlPath = "~/Controls/Dashboard/CtrlDashboardResource.ascx";
                    break;
                case ModuleKeys.DashboardProgress:
                    controlPath = "~/Controls/Dashboard/CtrlDashboardProgress.ascx";
                    break;
                case ModuleKeys.DashboardCost:
                    controlPath = "~/Controls/Dashboard/CtrlDashboardCost.ascx";
                    break;
                default:
                    return;
            }

            Control dashboardControl = LoadControl(controlPath);
            dashboardContent.Controls.Add(dashboardControl);
        }
    }
}

