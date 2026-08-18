using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Controls.Notifications
{
    public partial class CtrlNotifications : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!IsPostBack)
            //    BindInvoiceNotification();
        }
        private int pageIndex { get; set; } = 1;
       
    }
}