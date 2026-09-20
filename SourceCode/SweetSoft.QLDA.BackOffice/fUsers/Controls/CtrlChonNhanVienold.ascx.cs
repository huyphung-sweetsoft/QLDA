using SweetSoft.QLDA.BackOffice.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public partial class CtrlChonNhanVien : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
        public void ShowTestModal()
        {
            mdlMemberPicker.Title = "Test Giao Diện Chọn Nhân Viên";
            mdlMemberPicker.OpenModal(true);
        }
        protected void btnSave_Click (object sender, EventArgs e)
        {

        }
    }
}