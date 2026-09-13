using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class ProjectDocuments : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.ProjectDocument; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisableBrowserCache();

            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlProjectDocuments1.ProjectId = CurrentProjectId;

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

            if (!IsView
                || !DocumentManager.Instance.CanAccessProjectDocument(
                    CurrentProjectId,
                    ActionKeys.View))
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            if (IsPostBack)
                return;

            string pageTitle = GetResourceText(
                BackEndResourceKeys.PROJECT_DOCUMENTS);
            SetMetaTagsOgTags(pageTitle);
            Navigation1.MainTitle = pageTitle;
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
                    { "javascript:;", pageTitle }
                };

            CtrlProjectDocuments1.InitControls();
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

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlProjectDocuments1.ConfirmRequest(e);
        }
    }
}
