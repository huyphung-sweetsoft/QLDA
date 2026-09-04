using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fLichBieu.Controls
{
    public partial class CtrlLichNgoaiLe : BaseAdminUserControl
    {
        // ==========================================
        // 1. KHAI BÁO CÁC EVENT CALLBACK ĐỂ BẮN RA TRANG CHỦ
        // ==========================================
        public EventHandler NewNgoaiLeHandlerCallback;
        public EventHandler EditNgoaiLeHandlerCallback;

        // ==========================================
        // 2. PHÂN QUYỀN HIỂN THỊ NÚT BẤM
        // ==========================================
        protected bool IsEdit
        {
            get { return this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.LichBieu); }
        }
        protected bool IsDelete
        {
            get { return this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.LichBieu); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
        }

        // ==========================================
        // 3. KHỞI TẠO TIÊU ĐỀ LƯỚI & NGÔN NGỮ
        // ==========================================
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);

            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);

            // Cấu hình tiêu đề cột cho lưới (Không tính cột thao tác, lưới tự sinh)
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.EVENT_NAME),
                GetResourceText(BackEndResourceKeys.FROM_DATE),
                GetResourceText(BackEndResourceKeys.TO_DATE),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        public void InitControls()
        {
            ApplyControlsText();
            txtSearchSingle.SearchColumn = "TenNgoaiLe";
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;

            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;

            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, null, grvData, txtSearchSingle);

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "NgayBatDau"; // Mặc định xếp theo ngày bắt đầu
            grvData.CurrentSortDerection = "DESC";

            grvData.Rebind();
            pnlButtons.Update();
        }

        // ==========================================
        // 4. TRUY XUẤT DỮ LIỆU ĐỔ VÀO LƯỚI
        // ==========================================
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

                // Gọi tới Manager để truy vấn dữ liệu (Giả định bạn đã viết hàm Search trong LichBieuChungManager)
                dt = LichBieuChungManager.Instance.SearchLichNgoaiLePaging(
                    txtSearchSingle.Text,
                    false, // isWorkingDay = false (chỉ lấy ngày nghỉ lễ)
                    $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}",
                    rowIndex,
                    pageSize,
                    out totalRows);

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
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
                System.Diagnostics.Debug.WriteLine("🚨 LỖI CRASH LƯỚI: " + exc.ToString());
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        // ==========================================
        // 5. BẮT SỰ KIỆN TRÊN TỪNG DÒNG CỦA LƯỚI
        // ==========================================
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
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    Guid idNgoaiLe = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idNgoaiLe))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    // BẮN SỰ KIỆN RA TRANG CHỦ ĐỂ MỞ MODAL SỬA
                    if (EditNgoaiLeHandlerCallback != null)
                        EditNgoaiLeHandlerCallback(idNgoaiLe, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idNgoaiLe))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    // Cấu hình cảnh báo xóa
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "NGOAILE_DELETE";
                    result.Value = idNgoaiLe; // Truyền ID vào Value để lát xóa
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA)
                        , MSGButton.DeleteCancel, MSGIcon.Error);

                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        // ==========================================
        // 6. XỬ LÝ CÁC NÚT TÌM KIẾM & THÊM MỚI
        // ==========================================
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            // BẮN SỰ KIỆN RA TRANG CHỦ ĐỂ MỞ MODAL THÊM MỚI
            if (NewNgoaiLeHandlerCallback != null)
                NewNgoaiLeHandlerCallback(Guid.Empty, EventArgs.Empty);
        }

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
                master.searchTagBox_TagClosed(searchTagBox, tag, null, null, grvData, txtSearchSingle, out searchType);

                upSearchTagBox.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        // ==========================================
        // 7. NHẬN KẾT QUẢ XÁC NHẬN TỪ MESSAGE BOX ĐỂ XÓA DỮ LIỆU
        // ==========================================
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("NGOAILE_DELETE"))
                    {
                        Guid idNgoaiLe = (Guid)e.Value;
                        if (idNgoaiLe == Guid.Empty)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            // Thực thi lệnh xóa dưới Database
                            LichBieuChungManager.Instance.DeleteLichNgoaiLe(idNgoaiLe);
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
                else
                {
                    ShowInvalidNotFoundData();
                    return;
                }
            }
        }
        // Thêm hàm này vào bên trong class CtrlLichNgoaiLe
        protected string FormatDate(object dateObj)
        {
            // Nếu dữ liệu null hoặc rỗng, trả về chuỗi trống thay vì báo lỗi
            if (dateObj == null || dateObj == DBNull.Value || string.IsNullOrWhiteSpace(dateObj.ToString()))
                return string.Empty;

            DateTime dt;
            // Thử ép kiểu an toàn, nếu thành công thì format, nếu thất bại thì in nguyên chuỗi gốc
            if (DateTime.TryParse(dateObj.ToString(), out dt))
                return dt.ToString("dd/MM/yyyy");

            return dateObj.ToString();
        }
    }
}