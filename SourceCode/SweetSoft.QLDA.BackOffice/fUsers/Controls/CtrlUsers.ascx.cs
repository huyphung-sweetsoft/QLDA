using OfficeOpenXml;
using OfficeOpenXml.Style;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.ExcelManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
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
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public partial class CtrlUsers : BaseAdminUserControl
    {
        public EventHandler NewUserHandlerCallback;
        public EventHandler EditUserHandlerCallback;
        public EventHandler SendMailHandlerCallback;
        public Guid RoleId//Cái roleId này không phải là id nhóm quyền của cái acc đang đăng nhập, cách hoạt động của nó đơn giản là: Khi vào trang  quản lý nhóm quyền
        {                   //Bấm vào xem chi tiết 1 nhóm quyền nào đó, thì cái RoleId này sẽ được truyền vào cái Id của nhóm quyền đó
            get             //Nói chung, hầu hết cái RoleId này sẽ liên quan đến việc cái CtrlUsers này ngoài dùng ở trang tài khoản thì còn dùng được ở trang chi tiết nhóm quyền
            {
                if (ViewState["RoleId"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["RoleId"];
            }
            set
            {
                ViewState["RoleId"] = value;
            }
        }
        protected bool IsView
        {
            get
            {
                if (this.RoleId != Guid.Empty)
                    return false;
                return this.CURRENT_PAGE.IsView;
            }
        }
        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.User))
                    return true;
                return false;
            }
        }
        protected bool IsDelete
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.User))
                    return true;
                return false;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtCancel);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterPostBackControl(btnExport);
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchUserName.SearchTagItemText = GetResourceText(BackEndResourceKeys.USER_NAME);
            txtSearchFullName.SearchTagItemText = GetResourceText(BackEndResourceKeys.DISPLAY_NAME);
            txtSearchEmail.SearchTagItemText = "Email";
            txtSearchPhone.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONE_NUMBER);
            txtSearchCreatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_DATE);
            txtSearchCCCD.SearchTagItemText = GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchRole.SearchTagItemText = GetResourceText(BackEndResourceKeys.USER_GROUP);
            ddlSearchChucDanh.SearchTagItemText = GetResourceText(BackEndResourceKeys.CHUC_DANH);
            ddlSearchPhongBan.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONG_BAN);
            ddlSearchLaNhanVien.SearchTagItemText = GetResourceText(BackEndResourceKeys.ACCOUNT_TYPE);
            //------------------------------------------------
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);//tooltip: là cái chú thích nhỏ hiện ra khi mình hover vào cái nút, dùng kĩ thuật gán liên hoàn để gán cái chú thích này chung nội dung vs cái text hiển thị trong nút
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
            //------------------------------------------------
            txtSearchFullName.PlaceHolder = txtSearchEmail.PlaceHolder
                = txtSearchPhone.PlaceHolder = txtSearchSingle.PlaceHolder
                = txtSearchUserName.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchCreatedDate.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DATE);
            //------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.ACCOUNT),
                "Email",
                GetResourceText(BackEndResourceKeys.PHONE_NUMBER),
                GetResourceText(BackEndResourceKeys.USER_GROUP),
                GetResourceText(BackEndResourceKeys.STATUS),
                "2FA",
                GetResourceText(BackEndResourceKeys.LAST_LOGIN_DATE),
                GetResourceText(BackEndResourceKeys.ACTION),
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        #region Search + Init gridview
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
            controlHelpers.BindStatus(ddlSearchStatus);
            controlHelpers.BindRoles(ddlSearchRole);
            controlHelpers.BindLaNhanVien(ddlSearchLaNhanVien);
            controlHelpers.BindChucDanh(ddlSearchChucDanh);
            controlHelpers.BindPhongBan(ddlSearchPhongBan);
            if (this.RoleId != Guid.Empty)
                ddlSearchRole.SelectedValue = this.RoleId.ToString();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd && this.RoleId == Guid.Empty;
            grvData.Columns[5].Visible 
                = grvData.Columns[8].Visible
                = tagOther.Visible  
                = this.RoleId == null || this.RoleId == Guid.Empty; //cụm này là lí do mà trong cái role detail bị thiếu mấy cột, mấy thằng phía trên cũng làm ẩn hiện trong role detail
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);//ví dụ cho dễ: Khi search 1 thằng, bấm enter là load sang list chứa keyword này, xong nếu bấm back về danh sách cũ thì sẽ vẫn giữ cái keyword vừa tìm kiếm trên ô search
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = AspnetUser.Columns.UserName;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlButtons.Update();//thằng này với bên dưới nói chung là để đảm bảo các nút bấm được cập nhật đúng trạng thái hiển thị trên màn mà ko cần tải lại trang
            pnlSearch.Update();
        }
        private void AssignSearchColumns()
        {
            txtSearchUserName.SearchColumn = AspnetUser.Columns.UserName;
            txtSearchFullName.SearchColumn = AspnetUser.Columns.DisplayName;
            txtSearchEmail.SearchColumn = AspnetMembership.Columns.Email;
            txtSearchPhone.SearchColumn = AspnetUser.Columns.MobileAlias;
            txtSearchCCCD.SearchColumn = AspnetUser.Columns.IdCCCD; 
            ddlSearchStatus.SearchColumn = AspnetUser.Columns.IsActivated;
            ddlSearchRole.SearchColumn = AspnetRole.Columns.RoleId;
            ddlSearchLaNhanVien.SearchColumn = AspnetUser.Columns.LaNhanVien;
            ddlSearchChucDanh.SearchColumn = TblChucDanh.Columns.IdChucDanh;
            ddlSearchPhongBan.SearchColumn = TblPhongBan.Columns.IdPhongBan;
            txtSearchCreatedDate.SearchColumn = AspnetUser.Columns.LastActivityDate;
            ddlSearchRole.Enabled = this.RoleId == Guid.Empty;
        }
        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                //Giải thích kĩ thằng này 1 chút: biến sender đại diện cho cái thằng đã kích hoạt sự kiện hàm này, cụ thể là cái bảng grvData trên giao diện
                //Object là kiểu chung của hệ thống, C# nó ko hiểu đc nó là cái nút bấm, cái textbox hay cái gridview
                //Vì vậy nên mới cần dòng bên dưới này để ép cái sender về kiểu GirdView, "as": nếu ép thất bại, thì nó trả về grid=null thay vì làm lỗi
                //dòng if bên dưới là để check nếu grid == null thì dừng rồi báo lỗi ra màn hình lun
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
                if (grid.GridSearchType == GridSearchType.Single)//Single nghĩa là người dùng đang gõ vào ô tìm kiếm nhanh nên hệ thống chỉ truyền giá trị txtSearchSingle xuống hàm Search
                    //đương nhiên là nó sẽ kết hợp với cả cái đang chọn của 2 dropdown nữa
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
                    // Add RoleId to search criteria
                    if(this.RoleId != Guid.Empty)//Khi CtrlUsers này được nhúng vào RoleDetail thì cái if dưới đây sẽ đảm bảo là nhét điều kiện RoleId vào bộ lọc để chỉ lấy ra những Users thuộc nhóm quyền này
                    {
                        if (!keyValueSearchs.ContainsKey("RoleId"))
                            keyValueSearchs.Add("RoleId", this.RoleId);
                        else
                            keyValueSearchs["RoleId"] = this.RoleId;
                    }
                    dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"LaNhanVien DESC, {grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }    
                else
                {
                    //Chuỗi bắt tham số tìm kiếm, hàm GetControlValues sẽ quét qua các ô nhập nhiệu trong vùng pnlSearchDefault (2 cái dropdown), ô nào có giá trị thì nó sẽ bốc tên cột, như là idRole? và giá trị tương ứng để quăng vô 1 cái từ điển để tạo thành điều kiện lọc
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                    keyValueSearchs.AddIfNotExists(temp);
                    temp = controlHelpers.GetControlValues(pnlSearchPopup);
                    keyValueSearchs.AddIfNotExists(temp);
                    // Add RoleId to search criteria
                    if(this.RoleId != Guid.Empty)
                    {
                        if (!keyValueSearchs.ContainsKey("RoleId"))
                            keyValueSearchs.Add("RoleId", this.RoleId);
                        else
                            keyValueSearchs["RoleId"] = this.RoleId;
                    }
                    dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"LaNhanVien DESC, {grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                if (dt == null || dt.Rows.Count == 0)//Kiểm tra xem cái bảng dt vừa lấy từ database có data hay ko, nếu ko thig ẩn phân trang, ẩn nút export dữ liệu,.,...
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
                        btnExport.Visible = this.CURRENT_PAGE.IsExportExcel;//bật phân trang, bật export dữ liệu
                    }
                    else
                        ctrlGridviewPaging.Visible = btnExport.Visible = false;
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

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)//Hàm này bắt bất kì hành động nào tương tác với 1 dòng trên bảng, như xem chi tiết, xóa, sửa,..
        {
            switch (e.CommandName)
            {        
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;//lấy STT của dòng đang bấm hiện tại, thông qua Link Button nó lần ngược lên thẻ bọc ngoài là NamingCaontainer để tìm ra số thứ tự dòng
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid userId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))//ép kiểu id về đúng dạng guid, nếu lỗi database hay html thì báo lỗi
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)//Kiểm tra xem cái hàng chứa thằng đang bấm vô có phải Admin hay ko, và cái acc đăng nhập hiện tại có phải của Admin hay ko, nếu ko thì ko cho sửa, xóa j cả
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }
                    if (EditUserHandlerCallback != null && (this.RoleId == null || this.RoleId == Guid.Empty))//Mở popup để sửa nhanh
                        EditUserHandlerCallback(userId, EventArgs.Empty);
                    else
                        Response.Redirect(RewriteURLHelper.ViewUser(userId));//Hoặc đẩy sang hẳn 1 trang mới nếu chưa đki Callback
                    break;
                case "VIEW_EMP_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    rowIndex = 0;
                    if(e.CommandSource.GetType() != typeof(GridviewExtension))//Kiểm tra xem cái sự kiện click này bắt nguồn từ link button hay từ bản thân cái lưới
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))//tryparse vừa có tác dụng lấy dữ liệu, vừa kiểm tra xem cái guid đó có chuẩn GUID ko
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    Response.Redirect(RewriteURLHelper.ViewDetailEmp(userId));
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

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }

                    AspnetUser user = UserManager.Instance.GetUserById(userId);
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "USER_DELETE";
                    result.Value = user;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), user.DisplayName)
                        , MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
                case "RESET_PASSWORD":
                    if (!this.CURRENT_PAGE.IsEdit)
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

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }

                    user = UserManager.Instance.GetUserById(userId);
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    result = new ConfirmResult();
                    result.CommandName = "USER_RESET_PASSWORD";
                    result.Value = user;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_RESET_PASSWORD_FOR_ACCOUNT), user.DisplayName), MSGButton.Send, MSGIcon.Warning);
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
        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (grvData.GridSearchType == GridSearchType.Single)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearchSingle);
            else
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        #endregion

        #region Button
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewUserHandlerCallback != null)
                NewUserHandlerCallback(Guid.Empty, EventArgs.Empty);
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
            if (!this.CURRENT_PAGE.IsExportExcel)//Kiểm tra xem acc đang đăng nhập đc quyền xuất ko, ko thì gửi tbao lỗi
            {
                ShowAccessDeniedNotify();
                return;
            }
            //cái region này tương tự như cái NeedDataSource
            #region Get data 
            int totalRows = 0;
            int rowIndex = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize;
            int pageSize = rowIndex + grvData.CurrentPageSize;
            //-----------------------------------------------
            DataTable dt = null;
            if (grvData.GridSearchType == GridSearchType.Single)
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
                // Add RoleId to search criteria
                if(this.RoleId != Guid.Empty)
                {
                    if (!keyValueSearchs.ContainsKey("RoleId"))
                        keyValueSearchs.Add("RoleId", this.RoleId);
                    else
                        keyValueSearchs["RoleId"] = this.RoleId;
                }
                dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"LaNhanVien DESC, {grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            else
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                keyValueSearchs.AddIfNotExists(temp);
                temp = controlHelpers.GetControlValues(pnlSearchPopup);
                keyValueSearchs.AddIfNotExists(temp);
                // Add RoleId to search criteria
                if (this.RoleId != Guid.Empty)
                {
                    if (!keyValueSearchs.ContainsKey("RoleId"))
                        keyValueSearchs.Add("RoleId", this.RoleId);
                    else
                        keyValueSearchs["RoleId"] = this.RoleId;
                }
                dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            #endregion
            //Dưới đây là dùng class ExcelExportCore để cấu hình khung excel
            ExcelExportCore excelExportCore = new ExcelExportCore();
            string subject = GetResourceText(BackEndResourceKeys.USER_LIST);
            var options = new ExcelExportOptions
            {
                SheetName = subject,//Đặt tên cho tab excel
                ColumnStyles = new Dictionary<string, Action<ExcelRange>>()//Khai báo định dạng cho các cột đặt thù, ví dụ như thk dưới đây phải ép về kiểu dd-mm-yyyy chuẩn của Excel
                {

                    { "LastActivityDate", range =>
                        {
                            range.Style.Numberformat.Format = "dd-mmm-yyyy";
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    },
                },
                IsFixedHeader = true,//bật tính năng ghim dòng tiêu đề
                EnableZebraStripe = true,//tô màu dòng xen kẽ, như con ngựa vằn nên đặt ra zebra? hài
                ImageType = OfficeOpenXml.Drawing.ePictureType.Png,
                LogoHeight = 80,
                LogoWidth = 250,
                LogoCols = 2,
                IsLogoCenter = true,
                ColumnNames = new List<string>()//tiêu đề tiếng việt/anh của các cột tương ứng trong excel, thằng này và thằng ngay bên dưới phải = nhau về số lượng
                {
                    GetResourceText(BackEndResourceKeys.USER_NAME),
                    GetResourceText(BackEndResourceKeys.FULL_NAME),
                    "Địa chỉ email",
                    GetResourceText(BackEndResourceKeys.PHONE_NUMBER),
                    GetResourceText(BackEndResourceKeys.USER_GROUP),
                    GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD),
                    GetResourceText(BackEndResourceKeys.PHONG_BAN),
                    GetResourceText(BackEndResourceKeys.CHUC_DANH),
                    GetResourceText(BackEndResourceKeys.STATUS),
                    GetResourceText(BackEndResourceKeys.CREATED_DATE)
                },
                ShowColumns = new HashSet<string>()//Chỉ chính xác cột vật lý trong CSDL sẽ được truy xuất
                {
                    "UserName",
                    "DisplayName",
                    "Email",
                    "MobileAlias",
                    "RoleName",
                    "IdCCCD",
                    "TenPhongBan",
                    "TenChucDanh",
                    "IsActivated",
                    "LastActivityDate",
                },
                ConditionalMappingTexts = new List<ConditionalMappingText>//Dùng để dịch dữ liệu hệ thống. í dụ: Cột IsActivated mang giá trị True/False trong database, nó sẽ tự động dịch thành chữ "Active" hoặc "Inactive"
                {
                     new ConditionalMappingText
                    {
                        ColumnName = "IsActivated",
                        ValueMappings = new Dictionary<string, string>
                        {
                            { "True", GetResourceText(BackEndResourceKeys.ACTIVE) },
                            { "False", GetResourceText(BackEndResourceKeys.INACTIVE)},
                        },
                        DefaultText = GetResourceText(BackEndResourceKeys.ACTIVE)
                    },
                }
            };
            byte[] bytes = excelExportCore.ExportExcel(dt, subject, options);//nén toàn bộ data và cấu hình thành một mảng byte, filename: tên file tự động đính kèm ngày giờ hiện tại
            string filename = string.Format("{1} {0:dd-MM-yyyy HH-mm}.xlsx", DateTime.Now, Helpers.NormalizeFileName(subject));
            Response.Clear();//Xóa sạch mã html đang chuẩn bị tải lên màn hình 

            MemoryStream ms = new MemoryStream(bytes);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";//đổi contenttype thành .sheet, 
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.Flush();
            Response.End();
        }
        public override void ConfirmRequest(ConfirmResult e)//là điểm cuối trong 1 quy trình thực hiện 1 hành động nhạy cảm như xóa hoặc reset password
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("USER_DELETE"))
                    {
                        AspnetUser user = e.Value as AspnetUser;
                        if (user == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
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
                    else if (e.CommandName.Contains("USER_RESET_PASSWORD"))
                    {
                        AspnetUser user = e.Value as AspnetUser;
                        if (user == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            bool isDefault = false;
                            if (WebConfigurationManager.AppSettings["IsUsedDefaultPassword"] != null)
                                isDefault = bool.Parse(WebConfigurationManager.AppSettings["IsUsedDefaultPassword"]);

                            string password = string.Empty;
                            if (isDefault)
                                password = this.CURRENT_PAGE.DefaultPassword;
                            else
                                password = SecurityUtilities.CreateAlphaNumericString(8);

                            MembershipUser membershipUser = Membership.GetUser(user.UserName);
                            if (membershipUser == null)
                            {
                                ShowInvalidDataError();
                                return;
                            }
                            string oldPass = membershipUser.ResetPassword();
                            if (!membershipUser.ChangePassword(oldPass, password))
                            {
                                ShowNotify(GetResourceText(BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT));
                                return;
                            }
                            Membership.UpdateUser(membershipUser);
                            user.Email = membershipUser.Email;
                            user.ResetPasswordKey = string.Empty;
                            user.Save();
                            if (user != null)
                            {
                                if(SendMailHandlerCallback != null)
                                    SendMailHandlerCallback(new { User = user, Password = password }, EventArgs.Empty);
                                ShowNotify(GetResourceText(BackEndResourceKeys.THE_NEW_PASSWORD_HAS_BEEN_SENT_TO_THE_ACCOUNT_S_EMAIL_ADDRESS));
                            }
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
        #endregion
    }
}