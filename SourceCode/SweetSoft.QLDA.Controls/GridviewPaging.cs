using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace SweetSoft.QLDA.Controls
{
    public class GridviewPaging : UserControl
    {
        public event PageChangeHandler PageChanged;
        public event PageChangeHandler PageSizeChanged;
        private const string _GridPageCount = "GridPageCount";
        private const string _GridBeginIndex = "GridBeginIndex";
        private const string _GridEndIndex = "GridEndIndex";
        private const string _GridCurrentPageIndex = "GridCurrentPageIndex";
        private const string _GridTotalPageIndex = "GridTotalPageIndex";
        private const string _GridTCurrentPageSize = "GridTCurrentPageSize";
        private bool cr_alwayShowPaging = false;
        public bool AlwayShowPaging { get { return cr_alwayShowPaging; } set { cr_alwayShowPaging = value; } }
        #region Properties
        public bool BindingWithFiltering { get; set; }

        public virtual int PageCount
        {
            get
            {
                int vl = 5;
                if (this.ViewState[_GridPageCount] != null)
                {
                    int u = 0;
                    if (int.TryParse(this.ViewState[_GridPageCount].ToString(), out u))
                        vl = u;
                }
                return vl;
            }
            set
            {
                this.ViewState[_GridPageCount] = value;
            }
        }

        protected virtual int PageBeginIndex
        {
            get
            {
                int vl = 1;
                if (this.ViewState[_GridBeginIndex] != null)
                {
                    int u = 1;
                    if (int.TryParse(this.ViewState[_GridBeginIndex].ToString(), out u))
                        vl = u;
                }
                return vl;
            }
            set
            {
                this.ViewState[_GridBeginIndex] = value;
            }
        }

        protected virtual int PageEndIndex
        {
            get
            {
                int vl = PageBeginIndex + PageCount - 1;
                if (this.ViewState[_GridEndIndex] != null)
                {
                    int u = 0;
                    if (int.TryParse(this.ViewState[_GridEndIndex].ToString(), out u))
                        vl = u;
                }
                return vl;
            }
            set
            {
                this.ViewState[_GridEndIndex] = value;
            }
        }

        public virtual int PageCurrentIndex
        {
            get
            {
                int vl = 1;
                if (this.ViewState[_GridCurrentPageIndex] != null)
                {
                    int u = 1;
                    if (int.TryParse(this.ViewState[_GridCurrentPageIndex].ToString(), out u))
                        vl = u;
                }
                return vl;
            }
            set
            {
                this.ViewState[_GridCurrentPageIndex] = value;
            }
        }

        protected virtual int PageTotal
        {
            get
            {
                int vl = 1;
                if (this.ViewState[_GridTotalPageIndex] != null)
                {
                    int u = 0;
                    if (int.TryParse(this.ViewState[_GridTotalPageIndex].ToString(), out u))
                        vl = u;
                }
                return vl;
            }
            set
            {
                this.ViewState[_GridTotalPageIndex] = value;
                //this.Visible = PageTotal > 1;
            }
        }

        public string FirstButtonText { get; set; }
        public string PreViousButtonText { get; set; }
        public string NextButtonText { get; set; }
        public string LastButtonText { get; set; }

        protected string GetButtonText(string cmd)
        {
            string result = string.Empty;
            switch (cmd)
            {
                case "first":
                    if (!string.IsNullOrEmpty(FirstButtonText)) result = FirstButtonText;
                    else result = "<i class='fa fa-angle-double-left'></i>";
                    break;
                case "previous":
                    if (!string.IsNullOrEmpty(PreViousButtonText)) result = PreViousButtonText;
                    else result = "<i class='fa fa-angle-left'></i>";
                    break;
                case "next":
                    if (!string.IsNullOrEmpty(NextButtonText)) result = NextButtonText;
                    else result = "<i class='fa fa-angle-right'></i>";
                    break;
                case "last":
                    if (!string.IsNullOrEmpty(LastButtonText)) result = LastButtonText;
                    else result = "<i class='fa fa-angle-double-right'></i>";
                    break;
            }
            return result;
        }
        #endregion

        public GridviewPaging() { }

        #region Virtual function
        public virtual void RenderButtonControl(int pageStart, int pageSize, int pageCount, int VirtualCount)
        {

        }
        public virtual void ChangeFirstButtonState(bool show) { }
        public virtual void ChangePreviousButtonState(bool show) { }
        public virtual void ChangeNextButtonState(bool show) { }
        public virtual void ChangeLastButtonState(bool show) { }
        protected virtual void PageChangeBegin(object sender, ExtraPagingEventArg e)
        {
            if (PageChanged != null) PageChanged(sender, e);
        }

        protected virtual void PageSizeChangeBegin(object sender, ExtraPagingEventArg e)
        {
            if (PageSizeChanged != null) PageSizeChanged(sender, e);
        }
        #endregion
    }
    public delegate void PageChangeHandler(object sender, ExtraPagingEventArg e);

    public class ExtraPagingEventArg
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class PagingItem
    {
        public string CssClass { get; set; }
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
