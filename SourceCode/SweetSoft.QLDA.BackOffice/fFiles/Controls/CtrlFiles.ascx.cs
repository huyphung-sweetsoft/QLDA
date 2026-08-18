using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fFiles.Controls
{
    public partial class CtrlFiles : BaseAdminUserControl
    {
        public Guid OwnerId
        {
            get
            {
                if (ViewState["OwnerId"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["OwnerId"];
            }
            set
            {
                ViewState["OwnerId"] = value;
            }
        }
        private int TotalRows
        {
            get
            {
                if (ViewState["TotalRows"] == null)
                    return 1000;
                return (int)ViewState["TotalRows"];
            }
            set
            {
                ViewState["TotalRows"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            if (!IsPostBack)
            {
                ApplyControlsText();
            }
        }
        public void InitControls()
        {
            if(this.OwnerId != Guid.Empty)
                grvData.Columns[6].Visible = false;
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, null, grvData, txtSearchSingle);
            InitGridData();
        }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            //------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                "Tên tập tin",
                "Loại tập tin",
                "Kích thước",
                "Mở rộng",
                "Ngày tạo",
                "Người tải lên",
                "Tải xuống",
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        #region Search + Init gridview
        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblUploadFile.Columns.CreatedDate;
            grvData.CurrentSortDerection = "DESC";
            grvData.Rebind();
        }
        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    this.ShowInvalidDataError();
                    return;
                }
                int totalRows = 0;
                int rowIndex = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int pageSize = rowIndex + grid.CurrentPageSize;
                //--------------------------
                string orderBy = $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}";
               
                DataTable dt = UploadManager.Instance.SearchPaging(this.OwnerId, txtSearchSingle.Text, orderBy, rowIndex, pageSize, out totalRows);
                this.TotalRows = 0;
                
                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    this.TotalRows = totalRows;
                    if (dt.Rows.Count > 0)
                    {
                        ctrlGridviewPaging.Visible = true;
                    }
                    else
                        ctrlGridviewPaging.Visible = false;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }
                //-------------------------------------------------
                upMain.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            
        }
        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }
        #endregion

        #region Button
        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();

        }
        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, null, grvData, txtSearchSingle, out searchType);
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        #endregion
    }
}