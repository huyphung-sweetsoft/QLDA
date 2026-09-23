using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    /// <summary>
    /// Điều hướng giữa bốn dashboard khi đang xem phạm vi một dự án.
    /// </summary>
    public partial class CtrlProjectDashboardTabs : BaseAdminUserControl
    {
        private static readonly char[] IconClassSeparators =
            { ' ', '\t', '\r', '\n' };

        public Guid ProjectId { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            Guid projectId = ResolveProjectId();
            if (projectId == Guid.Empty)
            {
                Visible = false;
                base.OnPreRender(e);
                return;
            }

            ProjectId = projectId;
            BindTabs(projectId);
            Visible = ulProjectDashboardTabs.Controls.Count > 0;
            base.OnPreRender(e);
        }

        private void BindTabs(Guid projectId)
        {
            ulProjectDashboardTabs.Controls.Clear();

            AddTab(
                ModuleKeys.DashboardOverview,
                GetResourceText(BackEndResourceKeys.DASHBOARD_OVERVIEW),
                "fas fa-chart-pie",
                RewriteURLHelper.DashboardOverviewForProject(projectId));

            AddTab(
                ModuleKeys.DashboardProgress,
                GetResourceText(BackEndResourceKeys.DASHBOARD_PROGRESS),
                "fas fa-tasks",
                RewriteURLHelper.DashboardProgressForProject(projectId));

            AddTab(
                ModuleKeys.DashboardCost,
                GetResourceText(BackEndResourceKeys.DASHBOARD_COST),
                "fas fa-money-bill-wave",
                RewriteURLHelper.DashboardCostForProject(projectId));

            AddTab(
                ModuleKeys.DashboardResource,
                GetResourceText(BackEndResourceKeys.DASHBOARD_RESOURCE),
                "fas fa-users",
                RewriteURLHelper.DashboardResourceForProject(projectId));
        }

        private void AddTab(
            ModuleKeys moduleKey,
            string displayName,
            string iconCssClass,
            string dashboardUrl)
        {
            if (!CanView(moduleKey))
                return;

            bool isActive = CURRENT_PAGE != null
                && CURRENT_PAGE.PAGE_FUNCTION_CODE == moduleKey;

            HtmlGenericControl item = new HtmlGenericControl("li");
            item.Attributes["class"] = "nav-item";

            HtmlAnchor link = new HtmlAnchor
            {
                HRef = GetRelativeClientPath(dashboardUrl)
            };
            link.Attributes["class"] = isActive
                ? "nav-link px-3 active"
                : "nav-link px-3";
            if (isActive)
                link.Attributes["aria-current"] = "page";

            HtmlGenericControl icon = new HtmlGenericControl("i");
            icon.Attributes["class"] = NormalizeIconCssClass(iconCssClass)
                + " me-1";

            link.Controls.Add(icon);
            link.Controls.Add(new LiteralControl(
                HttpUtility.HtmlEncode(displayName)));
            item.Controls.Add(link);
            ulProjectDashboardTabs.Controls.Add(item);
        }

        private Guid ResolveProjectId()
        {
            if (ProjectId != Guid.Empty)
                return ProjectId;

            Guid queryProjectId;
            return Guid.TryParse(
                Page.Request.QueryString["project"],
                out queryProjectId)
                ? queryProjectId
                : Guid.Empty;
        }

        private bool CanView(ModuleKeys moduleKey)
        {
            try
            {
                return CURRENT_PAGE != null
                    && CURRENT_PAGE.IsUserRight(
                        ActionKeys.View
                        | ActionKeys.Create
                        | ActionKeys.Update,
                        moduleKey);
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeIconCssClass(string iconCssClass)
        {
            List<string> cssClasses = (iconCssClass ?? string.Empty)
                .Split(IconClassSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Where(item => item.All(character => char.IsLetterOrDigit(character)
                    || character == '-'
                    || character == '_'))
                .ToList();

            return cssClasses.Any(item => item.StartsWith(
                "fa", StringComparison.OrdinalIgnoreCase))
                ? string.Join(" ", cssClasses)
                : "fas fa-folder";
        }
    }
}
