using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.Controls;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Controls;

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
            CtrlDuAn1.NewProjectHandlerCallBack += NewProjectAction;
            CtrlDuAn1.EditProjectHandlerCallBack += EditProjectAction;
            CtrlDuAn1.ManageProjectTypeHandlerCallBack += ManageProjectTypeAction;
            CtrlDuAnForm1.SaveCompleted += CtrlDuAnForm1_SaveCompleted;

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROJECT_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Projects, GetResourceText(BackEndResourceKeys.PROJECT_LIST) }
                };
                CtrlDuAn1.IdKhachHang = Guid.Empty;
                CtrlDuAn1.InitControls();
            }

        }


        private void NewProjectAction(object sender, EventArgs e)
        {
            CtrlDuAnForm1.OpenAdd();
            
        }

        private void EditProjectAction(object sender, EventArgs e)
        {
            if (sender == null || !(sender is Guid))
            {
                ShowInvalidDataError();
                return;
            }

            CtrlDuAnForm1.OpenEdit((Guid)sender);
        }
        private void CtrlDuAnForm1_SaveCompleted(object sender, EventArgs e)
        {
            CtrlDuAn1.Rebind();
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlDuAn1.ConfirmRequest(e);
        }

        protected void ManageProjectTypeAction(object sender, EventArgs e)
        {
            CtrlQuanLyLoai1.ShowModal(LoaiManager.LoaiDoiTuong.DuAn);
        }



    }
}