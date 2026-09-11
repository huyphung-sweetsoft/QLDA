using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    /// <summary>
    /// Thanh điều hướng theo ngữ cảnh của một dự án.
    /// Các tab được sinh từ aspnet_Functions thay vì khai báo cứng trong markup.
    /// Mỗi tab vẫn dẫn đến một trang Web Forms độc lập để giữ nguyên lifecycle,
    /// modal và luồng nghiệp vụ sẵn có của từng chức năng.
    /// </summary>
    public partial class CtrlProjectTabs : BaseAdminUserControl
    {
        private static readonly char[] IconClassSeparators = { ' ', '\t', '\r', '\n' };

        public Guid ProjectId
        {
            get
            {
                object value = ViewState["ProjectId"];
                return value is Guid ? (Guid)value : Guid.Empty;
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (ProjectId == Guid.Empty)
            {
                Visible = false;
                base.OnPreRender(e);
                return;
            }

            BindTabs();
            Visible = ulProjectTabs.Controls.Count > 0;
            base.OnPreRender(e);
        }

        private void BindTabs()
        {
            ulProjectTabs.Controls.Clear();
            AddOverviewTab();

            IEnumerable<AspnetFunction> functions;
            try
            {
                functions = FunctionManager.Instance.GetProjectTabFunctions()
                    ?? Enumerable.Empty<AspnetFunction>();
            }
            catch
            {
                functions = Enumerable.Empty<AspnetFunction>();
            }

            foreach (AspnetFunction function in functions)
            {
                ModuleKeys moduleKey;
                if (!Enum.TryParse(function.FunctionCode, true, out moduleKey)
                    || moduleKey == ModuleKeys.None
                    || moduleKey == ModuleKeys.Project
                    || !CanView(moduleKey))
                {
                    continue;
                }

                string navigateUrl = BuildProjectFeatureUrl(function.PageUrl);
                if (string.IsNullOrEmpty(navigateUrl))
                    continue;

                AddTab(
                    moduleKey,
                    GetDisplayName(function),
                    function.Icon,
                    navigateUrl);
            }
        }

        private void AddOverviewTab()
        {
            if (!CanView(ModuleKeys.Project))
                return;

            AddTab(
                ModuleKeys.Project,
                GetResourceText(BackEndResourceKeys.OVERVIEW),
                "fas fa-info-circle",
                GetRelativeClientPath(RewriteURLHelper.ProjectDetail(ProjectId)));
        }

        private void AddTab(
            ModuleKeys moduleKey,
            string displayName,
            string iconCssClass,
            string navigateUrl)
        {
            bool isActive = CURRENT_PAGE != null
                && CURRENT_PAGE.PAGE_FUNCTION_CODE == moduleKey;

            HtmlGenericControl item = new HtmlGenericControl("li");
            item.Attributes["class"] = "nav-item";

            HtmlAnchor link = new HtmlAnchor
            {
                HRef = navigateUrl
            };
            link.Attributes["class"] = isActive
                ? "nav-link px-3 active"
                : "nav-link px-3";
            if (isActive)
                link.Attributes["aria-current"] = "page";

            HtmlGenericControl icon = new HtmlGenericControl("i");
            icon.Attributes["class"] = NormalizeIconCssClass(iconCssClass) + " me-1";

            link.Controls.Add(icon);
            link.Controls.Add(new LiteralControl(HttpUtility.HtmlEncode(displayName)));
            item.Controls.Add(link);
            ulProjectTabs.Controls.Add(item);
        }

        private string GetDisplayName(AspnetFunction function)
        {
            string displayName = GetResourceText(function.FunctionName);
            return string.IsNullOrWhiteSpace(displayName)
                ? function.FunctionCode
                : displayName;
        }

        private string BuildProjectFeatureUrl(string pageUrl)
        {
            string routeSuffix = (pageUrl ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(routeSuffix)
                || !routeSuffix.StartsWith("/", StringComparison.Ordinal)
                || routeSuffix.StartsWith("//", StringComparison.Ordinal)
                || routeSuffix.IndexOf("..", StringComparison.Ordinal) >= 0
                || routeSuffix.IndexOf('\\') >= 0)
            {
                return string.Empty;
            }

            string protectedProjectId = SecurityUtilities.ProtectUrlParameter(ProjectId.ToString());
            return GetRelativeClientPath($"/Project/{protectedProjectId}{routeSuffix}");
        }

        private static string NormalizeIconCssClass(string iconCssClass)
        {
            List<string> cssClasses = (iconCssClass ?? string.Empty)
                .Split(IconClassSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Where(item => item.All(character => char.IsLetterOrDigit(character)
                    || character == '-'
                    || character == '_'))
                .ToList();

            return cssClasses.Any(item => item.StartsWith("fa", StringComparison.OrdinalIgnoreCase))
                ? string.Join(" ", cssClasses)
                : "fas fa-folder";
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
    }
}
