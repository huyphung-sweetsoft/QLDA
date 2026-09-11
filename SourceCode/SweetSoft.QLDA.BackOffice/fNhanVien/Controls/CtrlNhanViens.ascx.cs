using OfficeOpenXml;
using OfficeOpenXml.Style;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.ExcelManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fNhanVien.Controls
{
    public partial class CtrlNhanViens : BaseAdminUserControl
    {
        public EventHandler NewNhanVienHandlerCallback;
        public EventHandler EditNhanVienHandlerCallback;
        public EventHandler SendMailHandlerCallback;

        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.NhanVien);
        protected bool IsDelete => this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.NhanVien);

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindChucDanh(ddlSearchChucDanh);
            controlHelpers.BindPhongBan(ddlSearchPhongBan);
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = AspnetUser.Columns.DisplayName;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlButtons.Update();
            pnlSearch.Update();
        }

        public void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterAsyncPostBackControl(lbtCancel);
            script.RegisterPostBackControl(btnExport);
        }

        public void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenNhanVien.SearchTagItemText = GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME);
            txtSearchIdCCCD.SearchTagItemText = GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD);
            txtSearchEmail.SearchTagItemText = "Email";
            txtSearchPhone.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONE_NUMBER);
            ddlSearchChucDanh.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            ddlSearchPhongBan.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONG_BAN);

            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);

            txtSearchSingle.PlaceHolder = txtSearchTenNhanVien.PlaceHolder
                = txtSearchEmail.PlaceHolder = txtSearchPhone.PlaceHolder
                = txtSearchIdCCCD.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);

            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME),
                "Liên hệ",
                GetResourceText(BackEndResourceKeys.PHONG_BAN),
                GetResourceText(BackEndResourceKeys.CHUC_DANH),
                "Định danh",
                GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE),
                GetResourceText(BackEndResourceKeys.ACTION),
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        private void AssignSearchColumns()
        {
            txtSearchTenNhanVien.SearchColumn = AspnetUser.Columns.DisplayName;
            txtSearchIdCCCD.SearchColumn = AspnetUser.Columns.IdCCCD;
            txtSearchPhone.SearchColumn = AspnetUser.Columns.MobileAlias;
            txtSearchEmail.SearchColumn = AspnetMembership.Columns.Email;
            ddlSearchChucDanh.SearchColumn = TblChucDanh.Columns.IdChucDanh;
            ddlSearchPhongBan.SearchColumn = TblPhongBan.Columns.IdPhongBan;
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

                DataTable dt = null;
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();

                if (grid.GridSearchType == GridSearchType.Single)
                {
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
                }
                else
                {
                    keyValueSearchs.AddIfNotExists(controlHelpers.GetControlValues(pnlSearchDefault));
                    keyValueSearchs.AddIfNotExists(controlHelpers.GetControlValues(pnlSearchPopup));
                }

                // CHỐT CHẶN BẢO MẬT: BẮT BUỘC LÀ NHÂN VIÊN
                if (!keyValueSearchs.ContainsKey("LaNhanVien"))
                    keyValueSearchs.Add("LaNhanVien", true);
                else
                    keyValueSearchs["LaNhanVien"] = true;

                // CHUYỂN SANG USER MANAGER
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                else
                {
                    dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = btnExport.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
                    btnExport.Visible = this.CURRENT_PAGE.IsExportExcel;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }
                upMain.Update();
                pnlButtons.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "VIEW_DETAIL":
                    if (!this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexView = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexView].Value.ToString(), out Guid idView))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    Response.Redirect(RewriteURLHelper.ViewDetailEmp(idView), false);
                    break;

                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndex = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out Guid userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (EditNhanVienHandlerCallback != null)
                        EditNhanVienHandlerCallback(userId, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    rowIndex = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out Guid idXoa))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    // CHUYỂN SANG USER MANAGER
                    AspnetUser user = UserManager.Instance.GetUserById(idXoa);
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "NHANVIEN_DELETE";
                    result.Value = user; // Truyền đối tượng User đi
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), user.DisplayName)
                        , MSGButton.DeleteCancel, MSGIcon.Error);

                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null)
            {
                if (e.CommandName.Contains("NHANVIEN_DELETE"))
                {
                    // Ép kiểu về AspnetUser
                    AspnetUser user = e.Value as AspnetUser;
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    try
                    {
                        // Gọi UserManager xóa thay vì NhanVienManager
                        UserManager.Instance.Delete(user);
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

        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (grvData.GridSearchType == GridSearchType.Single)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearchSingle);
            else
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd) { ShowAccessDeniedNotify(); return; }
            if (NewNhanVienHandlerCallback != null) NewNhanVienHandlerCallback(Guid.Empty, EventArgs.Empty);
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            new ControlHelpers().ClearControlValues(pnlSearch.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefault, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                upnlSearchDefault.Update();
                pnlSearch.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsExportExcel)
            {
                ShowAccessDeniedNotify();
                return;
            }

            int totalRows = 0;
            int rowIndex = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize;
            int pageSize = rowIndex + grvData.CurrentPageSize;
            DataTable dt = null;
            Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
            ControlHelpers controlHelpers = new ControlHelpers();

            if (grvData.GridSearchType == GridSearchType.Single)
            {
                keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
            }
            else
            {
                keyValueSearchs.AddIfNotExists(controlHelpers.GetControlValues(pnlSearchDefault));
                keyValueSearchs.AddIfNotExists(controlHelpers.GetControlValues(pnlSearchPopup));
            }

            // Ép điều kiện LaNhanVien = true cho tính năng xuất Excel
            if (!keyValueSearchs.ContainsKey("LaNhanVien"))
                keyValueSearchs.Add("LaNhanVien", true);
            else
                keyValueSearchs["LaNhanVien"] = true;

            if (grvData.GridSearchType == GridSearchType.Single)
            {
                dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            else
            {
                dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }

            ExcelExportCore excelExportCore = new ExcelExportCore();
            string subject = GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST);

            var options = new ExcelExportOptions
            {
                SheetName = subject,
                ColumnStyles = new Dictionary<string, Action<ExcelRange>>()
                {
                    { "NgayGiaNhap", range =>
                        {
                            range.Style.Numberformat.Format = "dd-mmm-yyyy";
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    },
                },
                IsFixedHeader = true,
                EnableZebraStripe = true,
                ImageType = OfficeOpenXml.Drawing.ePictureType.Png,
                LogoHeight = 80,
                LogoWidth = 250,
                LogoCols = 2,
                IsLogoCenter = true,
                ColumnNames = new List<string>()
                {
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME),
                    "Trạng thái",
                    GetResourceText(BackEndResourceKeys.PHONG_BAN),
                    GetResourceText(BackEndResourceKeys.CHUC_DANH),
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD),
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE)
                },
                ShowColumns = new HashSet<string>()
                {
                    "DisplayName", // Đã chuyển từ TenNhanVien
                    "IsActivated",
                    "TenPhongBan",
                    "TenChucDanh",
                    "IdCCCD",
                    "NgayGiaNhap"
                }
            };

            byte[] bytes = excelExportCore.ExportExcel(dt, subject, options);
            string filename = string.Format("{1} {0:dd-MM-yyyy HH-mm}.xlsx", DateTime.Now, Helpers.NormalizeFileName(subject));
            Response.Clear();

            MemoryStream ms = new MemoryStream(bytes);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.Flush();
            Response.End();
        }
    }
}