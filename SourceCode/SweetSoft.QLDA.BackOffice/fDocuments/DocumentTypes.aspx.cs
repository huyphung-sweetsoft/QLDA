using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class DocumentTypes : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.DocumentType; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (!this.IsView)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error403),
                    true);
                return;
            }

            string pageTitle =
                GetResourceText(
                    BackEndResourceKeys.DOCUMENT_TYPE_LIST);

            SetMetaTagsOgTags(pageTitle);
            Navigation1.MainTitle = pageTitle;
            Navigation1.keyValuePairUrls =
                new Dictionary<string, string>
                {
                    {
                        RewriteURLHelper.DocumentTypes,
                        pageTitle
                    }
                };

            CtrlDocumentTypes1.InitControls();
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlDocumentTypes1.ConfirmRequest(e);
        }
    }
}
