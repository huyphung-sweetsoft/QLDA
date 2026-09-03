using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class Documents : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.Document; }
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
                BackEndResourceKeys.DOCUMENT_LIST);
            SetMetaTagsOgTags(pageTitle);
            Navigation1.MainTitle = pageTitle;
            Navigation1.keyValuePairUrls =
                new Dictionary<string, string>
                {
                    {
                        RewriteURLHelper.Documents,
                        pageTitle
                    }
                };

            CtrlDocuments1.InitControls();
        }

        public override void DataCallback(
            string key,
            object value,
            object valueText)
        {
            CtrlDocuments1.HandleFileCallback(key);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlDocuments1.ConfirmRequest(e);
        }
    }
}
