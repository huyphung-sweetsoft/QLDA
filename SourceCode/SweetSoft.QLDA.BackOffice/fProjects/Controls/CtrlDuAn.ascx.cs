using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlDuAn : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterPostBackControl(btnExport);
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {

        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {

        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {

        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }


        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        //protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        //{
        //    try
        //    {
        //        MasterTemplate master = Page.Master as MasterTemplate;
        //        GridSearchType? searchType;
        //        master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
        //        pnlSearch.Update();
        //        string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
        //        ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
        //    }
        //    catch (Exception exc)
        //    {
        //        ShowNotify(exc.Message, MSGType.Error);
        //    }
        //}
    }
}