using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class ProjectDocumentDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.ProjectDocument; }
        }

        private Guid QueryId
        {
            get
            {
                try
                {
                    string value = CommonHelpers.QueryString("Id");
                    if (string.IsNullOrWhiteSpace(value))
                        return Guid.Empty;

                    return Guid.Parse(
                        SecurityUtilities.UnprotectUrlParameter(value));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisableBrowserCache();

            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlProjectDocumentDetail1.ProjectId = CurrentProjectId;

            if (CurrentProjectId == Guid.Empty)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Projects),
                    true);
                return;
            }

            TblDuAn project = DuAnManager.Instance.GetDuAnById(
                CurrentProjectId);
            if (project == null)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error404),
                    true);
                return;
            }

            if (!IsView)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            Guid idTaiLieu = QueryId;
            if (idTaiLieu == Guid.Empty
                || !DocumentManager.Instance.CanOpenProjectDocument(
                    idTaiLieu,
                    CurrentProjectId,
                    ActionKeys.View))
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            TblTaiLieu document = DocumentManager.Instance
                .GetProjectDocumentById(idTaiLieu, CurrentProjectId);
            if (document == null)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error404),
                    true);
                return;
            }

            // Validate the document on every postback, but only rebuild the
            // navigation and child control on the initial request.
            if (IsPostBack)
                return;

            string listTitle = GetResourceText(
                BackEndResourceKeys.PROJECT_DOCUMENTS);
            string detailTitle = GetResourceText(
                BackEndResourceKeys.DOCUMENT_DETAIL);

            SetMetaTagsOgTags(document.TenTaiLieu);
            Navigation1.MainTitle = detailTitle;
            Navigation1.keyValuePairUrls =
                new Dictionary<string, string>
                {
                    {
                        RewriteURLHelper.Projects,
                        GetResourceText(BackEndResourceKeys.PROJECT_LIST)
                    },
                    {
                        RewriteURLHelper.ProjectDetail(CurrentProjectId),
                        project.TenDuAn
                    },
                    {
                        RewriteURLHelper.ProjectDocuments(CurrentProjectId),
                        listTitle
                    },
                    { "javascript:;", document.TenTaiLieu }
                };

            if (!CtrlProjectDocumentDetail1.InitControls(idTaiLieu))
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error404),
                    true);
                return;
            }

            if (string.Equals(
                    CommonHelpers.QueryString("tab"),
                    "versions",
                    StringComparison.OrdinalIgnoreCase))
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "OpenProjectDocumentVersionsTab",
                    "var tabElement=document.querySelector('[data-bs-target=\"#document-versions\"]');"
                    + "if(tabElement&&window.bootstrap){bootstrap.Tab.getOrCreateInstance(tabElement).show();}",
                    true);
            }
        }

        private void DisableBrowserCache()
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);
            Response.Cache.SetAllowResponseInBrowserHistory(false);
        }

        public override void DataCallback(
            string key,
            object value,
            object valueText)
        {
            CtrlProjectDocumentDetail1.HandleFileCallback(key);
        }
    }
}
