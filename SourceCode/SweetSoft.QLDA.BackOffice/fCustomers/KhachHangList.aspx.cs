using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fCustomers.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCustomers
{
    public partial class KhachHangList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Customer;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlKhachHang.NewCustomerHandlerCallBack += NewCustomerAction;
            CtrlKhachHang.EditCustomerHandlerCallBack += EditCustomerAction;
            CtrlKhachHangForm1.SaveCompleted += CtrlKhachHangForm1_SaveCompleted;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CUSTOMER_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.CUSTOMER_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Customers, GetResourceText(BackEndResourceKeys.CUSTOMER_LIST) }
                };
                CtrlKhachHang.InitControls();
            }
        }


        private void NewCustomerAction(object sender, EventArgs e)
        {
            CtrlKhachHangForm1.OpenAdd();
        }

        private void EditCustomerAction(object sender, EventArgs e)
        {
            if (sender == null || !(sender is Guid))
            {
                ShowInvalidDataError();
                return;
            }

            CtrlKhachHangForm1.OpenEdit((Guid)sender);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlKhachHang.ConfirmRequest(e);
        }
        private void CtrlKhachHangForm1_SaveCompleted(object sender, EventArgs e)
        {
            CtrlKhachHang.Rebind();
        }
    }
}