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
    public partial class DocumentDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.Document; }
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

            if (!this.IsView)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            Guid idTaiLieu = QueryId;
            if (idTaiLieu == Guid.Empty
                || !DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    ActionKeys.View))
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            TblTaiLieu document =
                DocumentManager.Instance.GetCompanyDocumentById(idTaiLieu);
            if (document == null)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error404),
                    true);
                return;
            }

            // Permission validation must also run on asynchronous postbacks;
            // otherwise a stale page could keep invoking handlers after the
            // user's group/project access had been revoked.
            if (IsPostBack) return;

            string listTitle = GetResourceText(
                BackEndResourceKeys.DOCUMENT_LIST);
            string detailTitle = GetResourceText(
                BackEndResourceKeys.DOCUMENT_DETAIL);

            SetMetaTagsOgTags(document.TenTaiLieu);
            Navigation1.MainTitle = detailTitle;
            Navigation1.keyValuePairUrls =
                new Dictionary<string, string>
                {
                    { RewriteURLHelper.Documents, listTitle },
                    { "javascript:;", document.TenTaiLieu }
                };

            if (!CtrlDocumentDetail1.InitControls(idTaiLieu))
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
                    "OpenDocumentVersionsTab",
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
            CtrlDocumentDetail1.HandleFileCallback(key);
        }
    }
}
