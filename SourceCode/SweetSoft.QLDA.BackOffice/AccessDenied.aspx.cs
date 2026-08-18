using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.BackOffice.Common;
using System;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class AccessDenied : BaseAdminPage
    {
        public override bool IsLogin
        {
            get
            {
                return true;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.NO_ACCESS_PERMISSIONS));
                Response.StatusCode = 403;
                Response.StatusDescription = "Unauthorized";
            }
        }
    }
}