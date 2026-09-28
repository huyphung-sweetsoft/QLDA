using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fDocuments
{
    public partial class DocumentGroups : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.DocumentGroup; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect(GetRelativeClientPath(RewriteURLHelper.DocumentTypes), true);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            // Retired catalogue: no mutations from legacy confirmation postbacks.
        }
    }
}
