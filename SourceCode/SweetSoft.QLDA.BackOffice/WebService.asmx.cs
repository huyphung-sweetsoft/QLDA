
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Services;

namespace SweetSoft.QLDA.BackOffice
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        private void HandlePaging(ref int pageIndex, ref int pageSize)
        {
            pageIndex = ((pageIndex - 1) * pageSize + 1);
            pageSize = (((pageIndex - 1) * pageSize) + pageSize);
        }
        //---------------------------------------------
        [WebMethod(EnableSession = true)]
        public object GetUsers(string keyword, string page, string page_limit)
        {
            int pageIndex = 0;
            int pageSize = 0;
            int.TryParse(page, out pageIndex);
            int.TryParse(page_limit, out pageSize);
            if (pageIndex > 0 && pageSize > 0)
            {
                int totalRows = 0;
                HandlePaging(ref pageIndex, ref pageSize);
                DataTable dt = UserManager.Instance.SearchUsers(keyword, Guid.Empty, " DisplayName ASC ", pageIndex, pageSize, out totalRows);
                if (dt == null || dt.Rows.Count <= 0)
                    return null;
                List<object> dic = new List<object>();
                foreach (DataRow item in dt.Rows)
                {
                    dic.Add(new
                    {
                        id = item["UserId"],
                        text = item["DisplayName"]
                    });
                }
                return new
                {
                    data = dic,
                    total = totalRows
                };
            }
            return null;
        }
    }
}
