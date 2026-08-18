using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.BackOffice.Common;
using System;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class NotFound : BaseAdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.DATA_NOT_FOUND));
                Response.StatusCode = 404;
                Response.StatusDescription = "Not Found";
            }
        }
    }
}