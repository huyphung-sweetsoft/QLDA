using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fExecuteContracts.Controls
{
    public partial class CtrlHopDongThucHien : BaseAdminUserControl
    {
        public EventHandler NewHopDongHandlerCallback;
        public EventHandler EditHopDongHandlerCallback;

        protected bool IsView
        {
            get
            {
                return this.CURRENT_PAGE.IsView;
            }
        }

        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.Contract))
                {
                    return true;
                }

                return false;
            }
        }

        protected bool IsDelete
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.Contract))
                {
                    return true;
                }

                return false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script =
                ScriptManager.GetCurrent(this.Page);

            script.RegisterAsyncPostBackControl(
                lbtSearchSingle);
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();

            ControlHelpers controlHelpers =
                new ControlHelpers();

            controlHelpers.BindKhachHang(
                ddlSearchKhachHang);

            txtSearchSingle.EnterSubmitClientID =
                lbtSearchSingle.ClientID;

            lbtAdd.Visible =
                this.CURRENT_PAGE.IsAdd;

            MasterTemplate master =
                Page.Master as MasterTemplate;

            master.LoadSessionLastSearch(
                searchTagBox,
                null,
                grvData,
                txtSearchSingle);

            grvData.CurrentPageSize =
                Convert.ToInt32(
                    SweetContext.Current
                        .CurrentPageSize);

            grvData.CurrentSortExpression =
                TblHopDongThucHien.Columns
                    .SoHopDong;

            grvData.CurrentSortDerection =
                "ASC";

            grvData.Rebind();

            pnlButtons.Update();
            upnlSearchDefault.Update();
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected void grvData_NeedDataSource(
    object sender,
    ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid =
                    sender as GridviewExtension;

                if (grid == null)
                {
                    this.ShowInvalidDataError();
                    return;
                }

                int totalRows = 0;

                int rowIndex =
                    (grid.CurrentPageIndex - 1) *
                    grid.CurrentPageSize;

                int pageSize =
                    rowIndex +
                    grid.CurrentPageSize;

                DataTable dt = null;

                Dictionary<string, object>
                    keyValueSearchs =
                        new Dictionary<string, object>();

                ControlHelpers controlHelpers =
                    new ControlHelpers();

                keyValueSearchs =
                    controlHelpers.GetControlValues(
                        pnlSearchDefault);

                dt =
                    HopDongThucHienManager
                        .Instance
                        .SearchHopDongThucHiens(
                            txtSearchSingle.Text,
                            keyValueSearchs,
                            string.Format(
                                "{0} {1}",
                                grid.CurrentSortExpression,
                                grid.CurrentSortDerection),
                            rowIndex,
                            pageSize,
                            out totalRows);

                if (dt == null ||
                    dt.Rows.Count == 0)
                {
                    grvData.DataSource =
                        null;

                    grvData.DataBind();

                    ctrlGridviewPaging.Visible =
                        false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        ctrlGridviewPaging.Visible =
                            true;
                    }
                    else
                    {
                        ctrlGridviewPaging.Visible =
                            false;
                    }

                    grvData.VirtualItemCount =
                        totalRows;

                    grvData.DataSource =
                        dt;

                    grvData.DataBind();

                    ctrlGridviewPaging.PageIndex =
                        grvData.CurrentPageIndex;

                    ctrlGridviewPaging.PageSize =
                        grvData.CurrentPageSize;

                    ctrlGridviewPaging.TotalItems =
                        totalRows;

                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
                pnlButtons.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(
                    exc.Message,
                    MSGType.Error);
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndex = 0;

                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                    {
                        rowIndex = ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex;
                    }
                    else
                    {
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    }

                    Guid idHopDongThucHien = Guid.Empty;

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idHopDongThucHien))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (EditHopDongHandlerCallback != null)
                    {
                        EditHopDongHandlerCallback(idHopDongThucHien, EventArgs.Empty);
                    }

                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    rowIndex = 0;

                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                    {
                        rowIndex = ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex;
                    }
                    else
                    {
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    }

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idHopDongThucHien))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(idHopDongThucHien);

                    if (hopDong == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "HOP_DONG_THUC_HIEN_DELETE";
                    result.Value = hopDong;

                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), hopDong.TenHopDong),
                        MSGButton.DeleteCancel,
                        MSGIcon.Error);

                    OpenMessageBox(msg, result, false, false);

                    break;
            }
        }

        protected void btnSearch_ServerClick(
    object sender,
    EventArgs e)
        {
            MasterTemplate master =
                Page.Master as MasterTemplate;

            master.btnSearchSingle_Click(
                searchTagBox,
                grvData,
                txtSearchSingle);

            upSearchTagBox.Update();
        }


        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (NewHopDongHandlerCallback != null)
            {
                NewHopDongHandlerCallback(Guid.Empty, EventArgs.Empty);
            }
        }

        protected void searchTagBox_TagClosed(
    object sender,
    SearchTagItem tag)
        {
            try
            {
                MasterTemplate master =
                    Page.Master as MasterTemplate;

                GridSearchType? searchType;

                master.searchTagBox_TagClosed(
                    searchTagBox,
                    tag,
                    pnlSearchDefault,
                    null,
                    grvData,
                    txtSearchSingle,
                    out searchType);

                upnlSearchDefault.Update();

                string script =
                    string.Format(
                        "$('#{0}').val('');",
                        txtSearchSingle.ClientID);

                ScriptManager.RegisterClientScriptBlock(
                    this.Page,
                    GetType(),
                    "UpdateTxtSearch",
                    script,
                    true);
            }
            catch (Exception exc)
            {
                ShowNotify(
                    exc.Message,
                    MSGType.Error);
            }
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText =
                GetResourceText(
                    BackEndResourceKeys.KEYWORD);

            ddlSearchKhachHang.SearchTagItemText =
                "Khách hàng";

            txtGiaTriHopDongTu.SearchTagItemText =
                "Giá trị từ";

            txtGiaTriHopDongDen.SearchTagItemText =
                "Giá trị đến";

            txtNgayKyTu.SearchTagItemText =
                "Ngày ký từ";

            txtNgayKyDen.SearchTagItemText =
                "Ngày ký đến";

            lbtAdd.ToolTip =
                lbtAdd.Text =
                    GetResourceText(
                        BackEndResourceKeys.ADD_NEW);

            txtSearchSingle.PlaceHolder =
                GetResourceText(
                    BackEndResourceKeys
                        .ENTER_SEARCH_KEYWORDS);

            List<string> lstTableHeader =
                new List<string>
                {
            GetResourceText(BackEndResourceKeys.INDEX),
            "Số hợp đồng",
            "Tên hợp đồng",
            "Khách hàng",
            "Giá trị hợp đồng",
            "Ngày ký",
            "Ngày hiệu lực",
            "Ngày hết hạn",
            GetResourceText(
                BackEndResourceKeys.ACTION)
                };

            grvData.HeaderTexts =
                lstTableHeader;
        }

        private void AssignSearchColumns()
        {
            ddlSearchKhachHang.SearchColumn = TblHopDongThucHien.Columns.IdKhachHang;
            txtGiaTriHopDongTu.SearchColumn = "GiaTriHopDongTu";
            txtGiaTriHopDongDen.SearchColumn = "GiaTriHopDongDen";
            txtNgayKyTu.SearchColumn = "NgayKyTu";
            txtNgayKyDen.SearchColumn = "NgayKyDen";
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("HOP_DONG_THUC_HIEN_DELETE"))
                    {
                        TblHopDongThucHien hopDong = e.Value as TblHopDongThucHien;

                        if (hopDong == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            HopDongThucHienManager.Instance.Delete(hopDong);
                            ShowSuccessDeleteData();

                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
                        }
                        catch (Exception exc)
                        {
                            ShowNotify(exc.Message, MSGType.Error);
                        }
                    }
                }
            }

        }
    }
}