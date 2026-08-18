using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class Default : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Dashboard;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.DASHBOARD));
                ltrContent.Text = Server.HtmlDecode(_settingManager.GetSettingValue(SettingKeys.InternalAnnouncement));

            }
        }

    }
}