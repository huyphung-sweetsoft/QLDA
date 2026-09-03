using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class DocumentTemplates : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.DocumentTemplate; }
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

            string pageTitle = GetResourceText(
                BackEndResourceKeys.DOCUMENT_TEMPLATE_LIST);
            SetMetaTagsOgTags(pageTitle);
            Navigation1.MainTitle = pageTitle;
            Navigation1.keyValuePairUrls =
                new Dictionary<string, string>
                {
                    {
                        RewriteURLHelper.DocumentTemplates,
                        pageTitle
                    }
                };

            CtrlDocumentTemplates1.InitControls();
        }

        public override void DataCallback(
            string key,
            object value,
            object valueText)
        {
            CtrlDocumentTemplates1.HandleFileCallback(key);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlDocumentTemplates1.ConfirmRequest(e);
        }
    }
}
