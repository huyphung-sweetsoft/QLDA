using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Helpers;
using System;
using System.Reflection;

namespace SweetSoft.QLDA.BackOffice.MasterPages
{
    public partial class BasicTemplate : System.Web.UI.MasterPage
    {
        protected string GetRelativeClientPath(string virtualPath)
        {
            //if (!virtualPath.Contains("AdminPanel"))
            //    virtualPath = string.Format("/AdminPanel/{0}", virtualPath.TrimStart('/'));
            return CommonHelpers.GetRelativeClientPath(Page, virtualPath);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                lbVersion.InnerText = version;
            }
        }

        protected BaseAdminPage CURRENT_PAGE
        {
            get
            {
                try
                {
                    if (this.Page is BaseAdminPage)
                        return (BaseAdminPage)this.Page;
                }
                catch (Exception) { }
                return null;
            }
        }

        public string GetResourceText(string messageId)
        {
            return CURRENT_PAGE.GetResourceText(messageId);
        }
    }
}