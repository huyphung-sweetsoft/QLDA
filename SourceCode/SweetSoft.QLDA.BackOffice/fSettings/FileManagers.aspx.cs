//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using SweetSoft.QLDA.Core.Functions;

namespace SweetSoft.QLDA.BackOffice.fSettings
{
    public partial class FileManagers : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.FileManager;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.FILE_MANAGER));
                //Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                //{
                //    {"/FileManagers", GetResourceText(BackEndResourceKeys.FOLDER_MANAGEMENT) }
                //};
            }
        }
    }
}