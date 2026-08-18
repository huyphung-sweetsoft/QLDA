using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.MailManager;

//--------------------PROGRAMER LOGS------------------------

namespace SweetSoft.QLDA.BackOffice.fEmailTemplate
{
    public partial class EmailTemplateDetail : BaseAdminPage
    {
        #region Properties
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.EmailTemplate;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMAIL_TEMPLATE));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.EmailTemplates, GetResourceText(BackEndResourceKeys.EMAIL_TEMPLATE) },
                    {"javascript:;", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMAIL_TEMPLATE);
            }
        }
    }
}