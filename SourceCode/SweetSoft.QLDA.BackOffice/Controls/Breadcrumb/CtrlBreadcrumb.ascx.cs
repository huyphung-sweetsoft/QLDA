using SweetSoft.QLDA.BackOffice.Common;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.Controls.Breadcrumb
{
    public partial class CtrlBreadcrumb : BaseAdminUserControl
    {
        public string SubTitle
        {
            get; set;
        }
        public string MainTitle
        {
            get
            {
                if (ViewState["NAVIGATOR_TITLE"] == null)
                    return string.Empty;
                return (string)ViewState["NAVIGATOR_TITLE"];
            }
            set
            {
                ViewState["NAVIGATOR_TITLE"] = value;
                pnlNavigator.Update();
            }
        }
        public string Alert
        {
            get
            {
                if (ViewState["NAVIGATOR_ALERT"] == null)
                    return string.Empty;
                return (string)ViewState["NAVIGATOR_ALERT"];
            }
            set
            {
                ViewState["NAVIGATOR_ALERT"] = value;
                pnlNavigator.Update();
            }
        }
        public Dictionary<string, string> keyValuePairUrls
        {
            get
            {
                if (ViewState["NAVIGATOR_URLS"] == null)
                    return null;
                return (Dictionary<string, string>)ViewState["NAVIGATOR_URLS"];
            }
            set
            {
                ViewState["NAVIGATOR_URLS"] = value;
                pnlNavigator.Update();
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            BindNavigator();
        }
        public void BindNavigator()
        {
            string htmlNav = string.Empty;
            if (keyValuePairUrls != null && keyValuePairUrls.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in keyValuePairUrls)
                {
                    if (!pair.Key.Contains("javascript"))
                        htmlNav += string.Format(itemTemplate.InnerHtml, this.CURRENT_PAGE.GetRelativeClientPath(pair.Key), pair.Value, string.Empty);
                    else
                        htmlNav += string.Format(itemTemplate.InnerHtml, "javascript:;", pair.Value, string.Empty);
                }
            }
            ltrNavigator.Text = htmlNav;
            divAlert.Visible = !string.IsNullOrEmpty(Alert);
            pnlNavigator.Update();
        }
    }
}