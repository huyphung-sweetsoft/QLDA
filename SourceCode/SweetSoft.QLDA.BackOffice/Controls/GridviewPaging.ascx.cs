using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.BackOffice.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.Core.Infrastructure;

namespace SweetSoft.QLDA.BackOffice.Controls
{
    public partial class GridviewPaging : BaseAdminUserControl
    {
        public event GridviewCustomDelegateClass.PageChangedEventHandler PageChanged;


        private int _pageIndex = 1;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { _pageIndex = value; }
        }

        private int _pageSize = 30;
        public int PageSize
        {
            get
            {
                if (ViewState["PageSize"] != null)
                    return (int)ViewState["PageSize"];
                return int.Parse(SweetContext.Current.CurrentPageSize);
            }
            set { ViewState["PageSize"] = _pageSize = value; }
        }

        private int _totalItems;
        public int TotalItems
        {
            get { return _totalItems; }
            set { _totalItems = value; }
        }

        public string GridviewID
        {
            get
            {
                object data = ViewState["GridviewID"];
                if (data != null)
                    return data.ToString();
                else
                    return string.Empty;
            }
            set { ViewState["GridviewID"] = value; }
        }

        int TotalPages
        {
            get
            {
                int num = _totalItems / PageSize;
                if (_totalItems % PageSize != 0)
                    num += 1;
                return num;
            }
        }

        List<PageItem> lstPageNumbers;
        GridviewCustomPageChangeArgs args;

        bool IsContainGuid(string ctrlname)
        {
            if (string.IsNullOrEmpty(ctrlname))
                return false;

            string secretKey = "linkpager" + GridviewID;
            if (ctrlname.ToLower().StartsWith(secretKey.ToLower()))
            {
                string guidTest = ctrlname.Substring(secretKey.Length);
                Guid temp = Guid.Empty;
                try
                {
                    temp = new Guid(guidTest);
                }
                catch (Exception ex)
                {
                    temp = Guid.Empty;
                }

                if (temp != Guid.Empty)
                    return true;
            }
            return false;
        }

        private string getPostBackArgument()
        {
            string ctrlname = Page.Request.Params["__EVENTTARGET"];
            if (ctrlname != null && ctrlname != String.Empty)
            {
                if (IsContainGuid(ctrlname))
                {
                    string value = Page.Request.Params["__EVENTARGUMENT"];
                    return value;
                }
            }

            // if __EVENTTARGET is null, the control is a button type and we need to
            // iterate over the form collection to find it
            else
            {
                string ctrlStr = String.Empty;
                foreach (string ctl in Page.Request.Form)
                {
                    //handle ImageButton they having an additional "quasi-property" in their Id which identifies
                    //mouse x and y coordinates
                    if (ctl.EndsWith(".x") || ctl.EndsWith(".y"))
                    {
                        ctrlStr = ctl.Substring(0, ctl.Length - 2);
                    }
                    else
                    {
                        ctrlStr = ctl;
                    }

                    if (IsContainGuid(ctrlStr))
                    {
                        string value = Page.Request.Params["__EVENTARGUMENT"];
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string ctr = getPostBackArgument();
                if (string.IsNullOrEmpty(ctr) == false)
                    CommandElement_Click(Convert.ToInt32(ctr));
            }
        }

        public void InitLoad()
        {
            BindCommandElement();
            SetInfo();
        }

        void Pager_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            PageChanged(this, e);
            //throw new Exception("The method or operation is not implemented.");
        }

        void SetInfo()
        {
            try
            {
                int currentStart = ((this._pageIndex - 1) * this.PageSize) + 1;
                lblCurrentPage.Text = currentStart.ToString("N0");
                int endStart = (this._pageIndex * this.PageSize);
                if (endStart > this._totalItems)
                    endStart = this._totalItems;
                //lbTotalRow.Text = endStart.ToString("N0");
                lblTotalPages.Text = this._totalItems.ToString("N0");
            }
            catch
            {
                return;
            }
        }

        private void BindCommandElement()
        {
            StringBuilder sbLink = new StringBuilder();
            LinkButton lnk = null;
            lstPageNumbers = PagingProvider.CreatePages(PageSize, TotalItems, PageIndex);
            if (lstPageNumbers != null && lstPageNumbers.Count > 0)
            {
                StringBuilder myStringBuilder = new StringBuilder();
                TextWriter myTextWriter = null;
                HtmlTextWriter myWriter = null;

                for (int i = 0, j = lstPageNumbers.Count; i < j; i++)
                {
                    PageItem item = lstPageNumbers[i];

                    if (item.CurrentPage)
                    {
                        if (item.IsFirst)
                            sbLink.Append("<li class=\"paginate_button page-item previous\"><a href=\"javascript:;\" class=\"linkPagging\"><i class=\"fas fa-angle-double-left\"></i></a></li>");
                        else if (item.Text == "«")
                            sbLink.Append("<li class=\"paginate_button page-item previous\"><a href=\"javascript:;\" class=\"linkPagging\"><i class=\" fas fa-angle-left\"></i></a></li>");
                        else if (item.Text == "»")
                            sbLink.Append("<li class=\"paginate_button page-item previous\"><a href=\"javascript:;\" class=\"linkPagging\"><i class=\"fas fa-angle-right\"></i></a></li>");
                        else if (item.IsLast)
                            sbLink.Append("<li class=\"paginate_button page-item previous\"><a href=\"javascript:;\" class=\"linkPagging\"><i class=\"fas fa-angle-double-right\"></i></a></li>");
                        else
                            sbLink.AppendFormat("<li class=\"paginate_button page-item active\"><a href=\"javascript:;\" class=\"linkPagging\">{0}</a></li>", item.Text);
                    }
                    else
                    {
                        #region RegionName

                        lnk = new LinkButton();
                        lnk.ID = "linkpager" + GridviewID + Guid.NewGuid();
                        lnk.CommandArgument = item.PageNum;
                        lnk.CommandName = "changePage";
                        string postBackReference = Page.ClientScript.GetPostBackEventReference(lnk, lnk.CommandArgument);
                        lnk.Attributes["href"] = "javascript:" + postBackReference;
                        lnk.CssClass = "linkPagging";

                        lnk.ToolTip = item.Title;

                        if (item.IsFirst)
                            lnk.Controls.Add(new Literal() { Text = "<span class='fas fa-angle-double-left'></span>" });
                        else if (item.Text == "«")
                            lnk.Controls.Add(new Literal() { Text = "<span class='fas fa-angle-left'></span>" });
                        else if (item.Text == "»")
                            lnk.Controls.Add(new Literal() { Text = "<span class='fas fa-angle-right'></span>" });
                        else if (item.IsLast)
                            lnk.Controls.Add(new Literal() { Text = "<span class='fas fa-angle-double-right'></span>" });
                        else
                            lnk.Controls.Add(new Literal() { Text = " " + item.Text });

                        myStringBuilder = new StringBuilder();
                        myTextWriter = new StringWriter(myStringBuilder);
                        myWriter = new HtmlTextWriter(myTextWriter);
                        lnk.RenderControl(myWriter);
                        string html = myTextWriter.ToString();

                        sbLink.Append("<li class=\"paginate_button page-item\">" + html + "</li>");

                        #endregion
                    }
                }
            }

            ltrLink.Text = sbLink.ToString();
        }

        protected void CommandElement_Click(int pageIndex)
        {
            args = new GridviewCustomPageChangeArgs();
            args.CurrentPageNumber = pageIndex;
            this.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            args.CurrentPageSize = this.PageSize;
            this._pageIndex = args.CurrentPageNumber;
            Pager_PageChanged(this, args);
            SetInfo();
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            ddlPageSize.SelectedValue = SweetContext.Current.CurrentPageSize;
        }
        protected void DropDownListPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            SweetContext.Current.CurrentPageSize = this.PageSize.ToString();
            CommandElement_Click(1);
        }
    }
}