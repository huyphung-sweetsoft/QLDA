using System;
using System.Web.UI;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;

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
            LoadDashboard(PAGE_FUNCTION_CODE);
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
                case "employee": return ModuleKeys.DashboardEmployee;
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
                ModuleKeys.DashboardCost,
                ModuleKeys.DashboardEmployee
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
                case ModuleKeys.DashboardEmployee:
                    controlPath = "~/Controls/Dashboard/CtrlEmployeeDashboard.ascx";
                    break;
                default:
                    return;
            }

            Control dashboardControl = LoadControl(controlPath);
            dashboardContent.Controls.Add(dashboardControl);
        }
    }
}

