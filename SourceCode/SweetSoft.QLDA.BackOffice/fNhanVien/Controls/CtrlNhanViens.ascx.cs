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
        //Khai báo mấy cái callback, vai trò của mấy cái callback này là để giao tiếp giữa ascx và aspx, thay vì để ascx xử lí mấy cái sự kiện thì bắn tín hiệu qua cho aspx để nó xử lý
        public EventHandler NewNhanVienHandlerCallback;
        public EventHandler EditNhanVienHandlerCallback;
        public EventHandler SendMailHandlerCallback;
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
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.NhanVien))//Gọi IsUserRight để kiểm tra xem dùng hiện tại có hành động ActionKeys (update hay bla bla)
                    return true;                                                          //trên Module tương ứng hay không
                return false;
            }
        }
        protected bool IsDelete
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.NhanVien))
                    return true;
                return false;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }
        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }
        //Thiết lập cấu hình ban đầu khi mở trang
        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            //Thằng helper gọi đống bind để lấy data quăng vô 2 cái dropdown
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindChucDanh(ddlSearchChucDanh);
            controlHelpers.BindPhongBan(ddlSearchPhongBan);
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;//gán sự kiện gắn nút Enter để tìm lun, nói chung là làm thay cho việc nhấn dô cái icon kính lúp nếu lười
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            //do chưa còn dùng ở đâu nữa ko nên cái ẩn cột bên User chưa cần bê qua
            MasterTemplate master = Page.Master as MasterTemplate; //ép kiểu giao diện khung của trang về lớp mastertemplate để dùng ké hàm tiện ích của nó
            //master.LoadSessionLastSearch(searchTagBox, pnlSearchP) -- đang viết dở do chưa cần dùng tới
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);//thiết lập số dòng hiển thị trên 1 trang của view theo cấu hình mặc định trong hệ thống
            grvData.CurrentSortExpression = TblNhanVien.Columns.TenNhanVien;//gán tên cột sắp xếp mặt định là cột thêm nhân viên
            grvData.CurrentSortDerection = "ASC";// gán sort mặc định tăng dần từ A đến Z
            grvData.Rebind();//bắn tín hiệu kích hoạt cái NeedDataSource để truy vấn CSDL và vẽ dữ liệu lên bảng lần đầu
            pnlButtons.Update();
            pnlSearch.Update();
        }
        public void RegisterAsyncButton()//Đăng kí mấy nút quăng dô Ajax, nhấn dô thì chỉ load lại phần ruột thôi
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterAsyncPostBackControl(lbtCancel);
            script.RegisterPostBackControl(btnExport);
            //còn nhiều nữa, bổ sung sau
        }
        //thiết lập đống tiếng việt
        public void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenNhanVien.SearchTagItemText = GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME);
            txtSearchIdCCCD.SearchTagItemText = GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD);
            txtSearchEmail.SearchTagItemText = "Email";
            txtSearchPhone.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONE_NUMBER);
            ddlSearchChucDanh.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            ddlSearchPhongBan.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONG_BAN);
            //đống trên là cấu hình cho mấy cái dropdown và ô search nâng cao. search single blabla
            //dưới đây là cấu hình tooltip cho đống nút
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
            //Cấu hình đống placeholder
            txtSearchSingle.PlaceHolder = txtSearchTenNhanVien.PlaceHolder
                = txtSearchEmail.PlaceHolder = txtSearchPhone.PlaceHolder
                = txtSearchIdCCCD.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),             // Cột 0: Số thứ tự (GridView tự sinh)
                GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME),     // Cột 1: Nhân viên (Gồm Ảnh, Tên, Giới tính)
                "Liên hệ",                                              // Cột 2: Gồm Email và SĐT
                GetResourceText(BackEndResourceKeys.PHONG_BAN),         // Cột 3: Phòng ban
                GetResourceText(BackEndResourceKeys.CHUC_DANH),         // Cột 4: Chức danh
                "Định danh",                                            // Cột 5: Gồm CCCD và Ngày sinh
                GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE), // Cột 6: Ngày gia nhập
                GetResourceText(BackEndResourceKeys.ACTION),            // Cột 7: Thao tác (Nút Sửa/Xóa)
            };
            grvData.HeaderTexts = lstTableHeader;

        }
        private void AssignSearchColumns()//chả biết nói j vs hàm này cả, nói chung trỏ thẻ vào cột trong db đi
        {
            txtSearchTenNhanVien.SearchColumn = TblNhanVien.Columns.TenNhanVien;
            txtSearchIdCCCD.SearchColumn = TblNhanVien.Columns.IdCCCD;
            txtSearchPhone.SearchColumn = AspnetUser.Columns.MobileAlias;
            txtSearchEmail.SearchColumn = AspnetMembership.Columns.Email;
            ddlSearchChucDanh.SearchColumn = TblChucDanh.Columns.IdChucDanh;
            ddlSearchPhongBan.SearchColumn = TblPhongBan.Columns.IdPhongBan;
        }
        // Bắt sự kiện bấm Sửa / Xóa trên từng dòng 
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

                //--------------------------------------------
                DataTable dt = null;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();

                    // Lấy giá trị từ 2 dropdown (Phòng Ban, Chức Danh)
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);

                    // Truyền từ khóa ô tìm kiếm nhanh (txtSearchSingle) và bộ lọc vào Manager
                    dt = NhanVienManager.Instance.SearchNhanVien(txtSearchSingle.Text, keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();

                    // 1. Gom điều kiện từ vùng Default bên ngoài
                    var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                    keyValueSearchs.AddIfNotExists(temp);

                    // 2. Gom tiếp điều kiện từ vùng Popup (Offcanvas) bên trong
                    temp = controlHelpers.GetControlValues(pnlSearchPopup);
                    keyValueSearchs.AddIfNotExists(temp);

                    // Gửi toàn bộ đi tìm kiếm
                    dt = NhanVienManager.Instance.SearchNhanVien(keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }

                // Xử lý hiển thị GridView
                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = btnExport.Visible = false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        ctrlGridviewPaging.Visible = true;
                        btnExport.Visible = this.CURRENT_PAGE.IsExportExcel;
                    }
                    else
                    {
                        ctrlGridviewPaging.Visible = btnExport.Visible = false;
                    }

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

                    int rowIndexView = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexView = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndexView = Convert.ToInt32(e.CommandArgument);

                    Guid idView = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexView].Value.ToString(), out idView))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    // Chuyển hướng thẳng sang trang Detail vừa tạo (Kèm ID đã mã hóa bảo mật)
                    string detailUrl = $"~/fNhanVien/NhanVienDetail.aspx?id={SecurityUtilities.ProtectUrlParameter(idView.ToString())}";
                    Response.Redirect(detailUrl, false);
                    break;
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    Guid idNhanVien = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idNhanVien))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    // Gọi Callback nếu trang cha có đăng ký (mở Popup), ngược lại chuyển trang.
                    if (EditNhanVienHandlerCallback != null)
                        EditNhanVienHandlerCallback(idNhanVien, EventArgs.Empty);
                    else
                    {
                        // Nếu dự án đã có class RewriteURLHelper cho NhanVien thì gọi, nếu chưa bạn có thể dùng Response.Redirect truyền URL trực tiếp
                        Response.Redirect(RewriteURLHelper.ViewNhanVien(idNhanVien));
                    }
                    break;
                
                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idNhanVien))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    // Lấy thông tin nhân viên để hiển thị tên lên thông báo xác nhận xóa
                    TblNhanVien nhanVien = NhanVienManager.Instance.GetNhanVienById(idNhanVien);
                    if (nhanVien == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "NHANVIEN_DELETE"; // Đổi tên command để bắt ở ConfirmRequest
                    result.Value = nhanVien;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), nhanVien.TenNhanVien)
                        , MSGButton.DeleteCancel, MSGIcon.Error);

                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        // Xử lý xác nhận xóa từ popup modal 
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("NHANVIEN_DELETE"))
                    {
                        TblNhanVien nhanVien = e.Value as TblNhanVien;
                        if (nhanVien == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            NhanVienManager.Instance.Delete(nhanVien); // Bạn cần viết hàm Delete trong Manager
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

        // Lọc nhanh khi chọn dropdown Phòng ban / Chức danh
        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (grvData.GridSearchType == GridSearchType.Single)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearchSingle);
            else
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        //protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)//Khi bấm tìm kiếm, sang trang, xóa bla bla thì nó kích hoạt cái grvData.Rebind() và cái này sẽ tự động kích hoạt cái dưới đây, để đổ data vào, đúng hơn là cập nhật bảng với các thông số hiện tại
        //{
        //    GridviewExtension grid = sender as GridviewExtension;
        //    if (grid == null) return;
        //    //cái này là lquan tới phân trang, 
        //    int rowIndex = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;//tính số hàng phải bỏ qua để hiển thị cho cái trang hiện tại, ví dụ đang ở trang 3, mỗi trang 10 dòng, thì sẽ bỏ qua 3-1 *10 = 20 dòng data đầu
        //    int pageSize = rowIndex + grid.CurrentPageSize;//lấy data từ dòng 21 đến dòng 30, kiểu dị
        //    //cái này lquan đến tìm kiếm
        //    Dictionary<string, object> searchParams = new Dictionary<string, object>();

        //    string keyword = txtSearchSingle.Text.Trim();
        //    if (!string.IsNullOrEmpty(keyword))
        //    {
        //        // Tên key đặt trùng với key bạn bắt trong NhanVienRepository (thường là "Keyword" hoặc "TenNhanVien")
        //        searchParams.Add("Keyword", keyword);
        //    }
        //    int totalRows = 0;
        //    string sortOrder = $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}";//tự động thay đổi khi kích vô cái tên cột đã set, như đây là cột TenNhanVie ASC thành TenNhanVien DESC
        //                                                                                   // 2. Truyền Dictionary vào hàm
        //    DataTable dt = NhanVienManager.Instance.SearchNhanVien(searchParams, sortOrder, rowIndex, pageSize, out totalRows);

        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        grvData.VirtualItemCount = totalRows;
        //        grvData.DataSource = dt;
        //        grvData.DataBind();

        //        ctrlGridviewPaging.Visible = true;
        //        ctrlGridviewPaging.TotalItems = totalRows;
        //        ctrlGridviewPaging.PageIndex = grid.CurrentPageIndex;
        //        ctrlGridviewPaging.PageSize = grid.CurrentPageSize;
        //        ctrlGridviewPaging.InitLoad();
        //    }
        //    else
        //    {
        //        grvData.DataSource = null;
        //        grvData.DataBind();
        //        ctrlGridviewPaging.Visible = false;
        //    }

        //    upMain.Update();
        //}
        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewNhanVienHandlerCallback != null)
                NewNhanVienHandlerCallback(Guid.Empty, EventArgs.Empty);
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
            // 1. Kiểm tra quyền xuất Excel của tài khoản hiện tại
            if (!this.CURRENT_PAGE.IsExportExcel)
            {
                ShowAccessDeniedNotify();
                return;
            }

            #region Get data 
            int totalRows = 0;
            int rowIndex = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize;
            int pageSize = rowIndex + grvData.CurrentPageSize;
            //-----------------------------------------------
            DataTable dt = null;

            // 2. Tái sử dụng logic lấy dữ liệu y hệt hàm grvData_NeedDataSource
            if (grvData.GridSearchType == GridSearchType.Single)
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);

                dt = NhanVienManager.Instance.SearchNhanVien(txtSearchSingle.Text, keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            else
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                keyValueSearchs.AddIfNotExists(temp);
                temp = controlHelpers.GetControlValues(pnlSearchPopup);
                keyValueSearchs.AddIfNotExists(temp);

                dt = NhanVienManager.Instance.SearchNhanVien(keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            #endregion

            // Cấu hình giao diện và cột hiển thị trên file Excel
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
                // Tên hiển thị trên dòng tiêu đề (Header) của file Excel
                ColumnNames = new List<string>()
                {
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME),
                    GetResourceText(BackEndResourceKeys.PHONG_BAN),
                    GetResourceText(BackEndResourceKeys.CHUC_DANH),
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD),
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_JOINDATE)
                },
                ShowColumns = new HashSet<string>()
                {
                    "TenNhanVien",
                    "TenPhongBan",
                    "TenChucDanh",
                    "IdCCCD",
                    "NgayGiaNhap"
                }
            };

            // 4. Sinh file bytes và trả về cho trình duyệt
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