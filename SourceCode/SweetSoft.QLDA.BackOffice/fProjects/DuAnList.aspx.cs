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
        private Guid IdDuAn
        {
            get
            {
                if (ViewState["IdDuAn"] != null)
                    return (Guid)ViewState["IdDuAn"];
                return Guid.Empty;
            }
            set
            {
                ViewState["IdDuAn"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlDuAn1.NewProjectHandlerCallBack += NewProjectAction;
            if (!IsPostBack)
            {
                CtrlDuAn1.InitControls();
            }
        }


        private void RefreshProjectInfo()
        {
            new ControlHelpers().Bind
        }

        private void NewProjectAction(object sender, EventArgs e)
        {
            RefreshProjectInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
            
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {

        }
    }
}