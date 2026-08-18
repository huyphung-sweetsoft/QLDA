//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class LockScreen : BaseAdminPage
    {
        public override bool IsLogin
        {
            get
            {
                return true;
            }
        }
        private string ReturnURL
        {
            get
            {
                try
                {
                    return CommonHelpers.QueryString("ReturnURL");
                }
                catch
                {
                    return "";
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            scriptManager.RegisterAsyncPostBackControl(lbtUnLock);
            if (!IsPostBack)
            {
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.LOCK_SCREEN));
                AspnetUser user = SweetContext.Current.User;
                if (user == null)
                    Response.Redirect(GetRelativeClientPath("/Login"), true);
                //if (!string.IsNullOrEmpty(tblorganizationuser.Avatar))
                //    imgAvatar.Src = tblorganizationuser.Avatar;
                tagUserName.InnerText = user.UserName;
                txtPassword.EnterSubmitClientID = lbtUnLock.ClientID;
                txtPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_PASSWORD);
                lbtUnLock.ToolTip = lbtUnLock.Text = GetResourceText(BackEndResourceKeys.UNLOCK);
            }
        }

        protected void lbtUnLock_Click(object sender, EventArgs e)
        {
            try
            {
                AspnetUser user = SweetContext.Current.User;
                if (user == null)
                {
                    Response.Redirect(GetRelativeClientPath("/login"), false);
                    return;
                }
                if (!Membership.ValidateUser(user.UserName, txtPassword.Text))
                {
                    ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                    validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PASSWORD_IS_INCORRECT));
                    txtPassword.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                AppCache.Remove(string.Format("ASP.NET_LockedId_{0}", user.UserName));
                AppCache.Insert(string.Format("ASP.NET_LockedId_{0}", user.UserName), false);
                if (!string.IsNullOrEmpty(ReturnURL))
                    Response.Redirect(GetRelativeClientPath(ReturnURL), false);
                else
                    Response.Redirect(GetRelativeClientPath("/Home"), false);
                return;
            }
            catch (Exception exc)
            {
                ShowSystemError();
                throw new Exception("Login", exc);
            }
        }

        protected void Unnamed_ServerClick(object sender, EventArgs e)
        {
            string userName = SweetContext.Current.UserName;
            SweetContext.ClearAdminData();
            AppCache.Remove(string.Format("ASP.NET_LockedId_{0}", userName));
            FormsAuthentication.SignOut();
            ExpireAllCookies();
            Response.Redirect(GetRelativeClientPath("/login"), true);
        }
    }
}