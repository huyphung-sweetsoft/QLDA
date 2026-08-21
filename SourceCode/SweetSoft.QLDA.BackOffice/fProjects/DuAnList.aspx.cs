using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class DuAnList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Project;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));

                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROJECT_LIST);
                //Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                //{
                //    {RewriteURLHelper.Users, GetResourceText(BackEndResourceKeys.USER_LIST) }
                //};
                //ApplyControlsText();
                CtrlDuAn1.InitControls();
                //string projectQueryId = CommonHelpers.QueryString("idDuAn");
                //if (string.IsNullOrEmpty(userQueryId)) return;
                //Guid tempId = Guid.Empty;
                //if (!Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(userQueryId), out tempId))
                //    return;
                //EditUserAction(tempId, EventArgs.Empty);
            }
        }
    }
}