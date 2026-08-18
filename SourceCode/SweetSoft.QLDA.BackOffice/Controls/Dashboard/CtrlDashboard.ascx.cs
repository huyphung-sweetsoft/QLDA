using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.Controls.Dashboard
{
    public partial class CtrlDashboard : BaseAdminUserControl
    {
        #region RegisterCSSAndJS
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> cssLinks = new List<string>();
                cssLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/Dashboard/dashboard-style.css"));
                List<string> jsLinks = new List<string>();
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Styles/plugins/apexcharts/apexcharts.min.js"));
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/Dashboard/dashboard.js"));
                return new RegisterCSSAndJS("cpHeadVendor", "cpVendorScript", cssLinks, jsLinks);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }
        #endregion
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (!IsPostBack)
            {
                InitReports();
            }
        }


        private void InitReports(bool isForceUpdate = true)
        {
            int currentMonth = DateTime.UtcNow.Month;
            int beforeMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            int currentYear = DateTime.UtcNow.Year;
            int beforeYear = beforeMonth == 12 ? currentYear - 1 : currentYear;
            DataTable dt = this.CURRENT_PAGE._settingManager.GetTopAuditLogs();
            if (dt != null && dt.Rows.Count > 0)
            {
                string htmlLogs = string.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    htmlLogs += string.Format(itemTemplate.InnerHtml, row["IpAddress"],
                        this.CURRENT_PAGE.DisplayName(row["ChangedBy"]),
                        row["Title"],
                        this.CURRENT_PAGE.ConvertDateTimeToString(row["ChangedAt"]),
                        row["UserAgent"]);
                }
                ltrAudits.Text = htmlLogs;
            }
        }
    }
}