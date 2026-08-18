using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fFiles
{
    public partial class FileListing : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Files;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.FILE_MANAGER));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.FILE_MANAGER);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Files, GetResourceText(BackEndResourceKeys.FILE_MANAGER) }
                };
                CtrlFiles1.InitControls();

            }
        }
    }
}