using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fLichBieu
{
    public partial class LichBieu : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.LichBieu;
            }
        }
        private Guid NgoaiLeId
        {
            get
            {
                if (ViewState["NgoaiLeId"] != null)
                    return (Guid)ViewState["NgoaiLeId"];
                return Guid.Empty;
            }
            set { ViewState["NgoaiLeId"] = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlLichNgoaiLe1.NewNgoaiLeHandlerCallback += NewNgoaiLeAction;
            CtrlLichNgoaiLe1.EditNgoaiLeHandlerCallback += EditNgoaiLeAction;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                ApplyControlsText();
                BindData();
                CtrlLichNgoaiLe1.InitControls();
            }
        }
        private void ApplyControlsText()
        {
            SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.WORKING_WEEK_CONFIG));

            Navigation1.keyValuePairUrls = new Dictionary<string, string>()
            {
                // Tạm thời cho link về trang chủ, bạn có thể cấu hình lại sau
                { RewriteURLHelper.LichBieu, GetResourceText(BackEndResourceKeys.SCHEDULE_MANAGEMENT) }
            };
            btnSaveTuan.Text = btnSaveTuan.ToolTip = GetResourceText(BackEndResourceKeys.SAVE_WEEK_CONFIG);
        }
        private void BindData()
        {
            // Lấy 7 cấu hình ngày từ Lớp Core Engine
            List<TblCauHinhTuanLamViec> lstTuan = LichBieuChungManager.Instance.GetAllCauHinhTuan();

            rptTuanLamViec.DataSource = lstTuan;
            rptTuanLamViec.DataBind();
        }
        protected void rptTuanLamViec_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                TblCauHinhTuanLamViec item = e.Item.DataItem as TblCauHinhTuanLamViec;
                if (item != null)
                {
                    Literal ltrTenThu = (Literal)e.Item.FindControl("ltrTenThu");
                    if (ltrTenThu != null)
                    {
                        if (item.NgayTrongTuan == 0)
                            ltrTenThu.Text = "Chủ Nhật";
                        else
                            ltrTenThu.Text = "Thứ " + (item.NgayTrongTuan + 1);
                    }

                    HtmlInputGenericControl txtGioBatDauSang = (HtmlInputGenericControl)e.Item.FindControl("txtGioBatDauSang");
                    HtmlInputGenericControl txtGioKetThucSang = (HtmlInputGenericControl)e.Item.FindControl("txtGioKetThucSang");
                    HtmlInputGenericControl txtGioBatDauChieu = (HtmlInputGenericControl)e.Item.FindControl("txtGioBatDauChieu");
                    HtmlInputGenericControl txtGioKetThucChieu = (HtmlInputGenericControl)e.Item.FindControl("txtGioKetThucChieu");

                    // Bơm thẳng dữ liệu từ DB (chuỗi) vào Giao diện (chuỗi)
                    txtGioBatDauSang.Value = item.GioBatDauSang;
                    txtGioKetThucSang.Value = item.GioKetThucSang;
                    txtGioBatDauChieu.Value = item.GioBatDauChieu;
                    txtGioKetThucChieu.Value = item.GioKetThucChieu;
                }
            }
        }
        private void RefreshNgoaiLeForm()
        {
            txtTenNgoaiLe.Text = string.Empty;
            txtNgayBatDau.Text = string.Empty;
            txtNgayKetThuc.Text = string.Empty;
            txtMoTa.Text = string.Empty;

            this.NgoaiLeId = Guid.Empty;
            hdfIdNgoaiLe.Value = string.Empty;
        }
        private void NewNgoaiLeAction(object sender, EventArgs e)
        {
            RefreshNgoaiLeForm();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        // Đón sự kiện bấm nút "Sửa" trên lưới
        private void EditNgoaiLeAction(object sender, EventArgs e)
        {
            if (sender == null) return;

            Guid id = (Guid)sender;
            if (id == Guid.Empty) return;

            RefreshNgoaiLeForm();

            // Lấy dữ liệu từ DB lên Modal
            TblLichNgoaiLe item = LichBieuChungManager.Instance.GetLichNgoaiLeById(id);
            if (item == null || item.DaXoa)
            {
                ShowInvalidNotFoundData();
                return;
            }

            this.NgoaiLeId = item.IdNgoaiLe;
            hdfIdNgoaiLe.Value = item.IdNgoaiLe.ToString();
            txtTenNgoaiLe.Text = item.TenNgoaiLe;
            // Lưu ý chữ y, M, d phải viết đúng hoa/thường như thế này:
            txtNgayBatDau.Text = item.NgayBatDau.ToString("yyyy-MM-dd");
            txtNgayKetThuc.Text = item.NgayKetThuc.ToString("yyyy-MM-dd");
            txtMoTa.Text = item.MoTa;

            lbtSubmit.Visible = this.IsEdit;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT);
            dlDetail.OpenModal(true);
        }

        // Bấm nút LƯU trên Modal
        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Validate Form HTML
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                DateTime fromDate = DateTime.MinValue;
                DateTime toDate = DateTime.MinValue;

                bool isFromValid = DateTime.TryParse(txtNgayBatDau.Text, out fromDate);
                bool isToValid = DateTime.TryParse(txtNgayKetThuc.Text, out toDate);

                if (isFromValid && isToValid && toDate < fromDate)
                {
                    validationEngine.AddErrorPrompt(txtNgayKetThuc.ClientID, "Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                // 2. Chuẩn bị Object DTO để ném xuống Manager
                bool isAdd = true;
                TblLichNgoaiLe item = new TblLichNgoaiLe();

                if (this.NgoaiLeId != Guid.Empty)
                {
                    if (!this.IsEdit) { ShowAccessDeniedNotify(); return; }
                    isAdd = false;
                    item.IdNgoaiLe = this.NgoaiLeId; // Truyền ID cũ xuống để Manager biết là lệnh Cập nhật
                }
                else
                {
                    if (!this.IsAdd) { ShowAccessDeniedNotify(); return; }
                    item.IdNgoaiLe = Guid.Empty; // Truyền Guid.Empty xuống để Manager biết là lệnh Thêm mới
                }

                // 3. Gán dữ liệu cơ bản
                item.TenNgoaiLe = txtTenNgoaiLe.Text.Trim();
                item.NgayBatDau = fromDate;
                item.NgayKetThuc = toDate;
                item.MoTa = txtMoTa.Text.Trim();
                item.LaNgayLamViec = false;

                // 4. Gọi MỘT HÀM DUY NHẤT (Mọi logic audit, mapping đã được Manager gánh)
                var result = LichBieuChungManager.Instance.CreateOrUpdate(item);

                if (result == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                // 5. Thông báo thành công
                if (isAdd)
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                else
                    ShowSuccessSaveData();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }

            // 6. Đóng modal và tải lại lưới
            dlDetail.CloseModal();
            CtrlLichNgoaiLe1.Rebind();
        }
        protected void btnSaveTuan_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsEdit)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                // 1. Lấy danh sách cấu hình hiện tại từ DB lên 
                // Để SubSonic biết đây là dữ liệu cũ (cần chạy lệnh UPDATE)
                List<TblCauHinhTuanLamViec> currentList = LichBieuChungManager.Instance.GetAllCauHinhTuan();

                foreach (RepeaterItem item in rptTuanLamViec.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        HiddenField hdfIdCauHinh = (HiddenField)item.FindControl("hdfIdCauHinh");
                        HiddenField hdfNgayTrongTuan = (HiddenField)item.FindControl("hdfNgayTrongTuan");
                        ExtraCheckbox chkIsWorking = (ExtraCheckbox)item.FindControl("chkIsWorking");

                        HtmlInputGenericControl txtGioBatDauSang = (HtmlInputGenericControl)item.FindControl("txtGioBatDauSang");
                        HtmlInputGenericControl txtGioKetThucSang = (HtmlInputGenericControl)item.FindControl("txtGioKetThucSang");
                        HtmlInputGenericControl txtGioBatDauChieu = (HtmlInputGenericControl)item.FindControl("txtGioBatDauChieu");
                        HtmlInputGenericControl txtGioKetThucChieu = (HtmlInputGenericControl)item.FindControl("txtGioKetThucChieu");

                        Guid idCauHinh = Guid.Parse(hdfIdCauHinh.Value);

                        // 2. Lấy chính xác Object cũ ra khỏi danh sách để cập nhật
                        TblCauHinhTuanLamViec obj = currentList.Find(x => x.IdCauHinh == idCauHinh);

                        if (obj != null)
                        {
                            obj.LaNgayLamViec = chkIsWorking.Checked;

                            if (chkIsWorking.Checked)
                            {
                                // Đẩy thẳng chuỗi từ giao diện xuống DB, nếu rỗng thì cho NULL
                                obj.GioBatDauSang = string.IsNullOrEmpty(txtGioBatDauSang.Value) ? null : txtGioBatDauSang.Value;
                                obj.GioKetThucSang = string.IsNullOrEmpty(txtGioKetThucSang.Value) ? null : txtGioKetThucSang.Value;
                                obj.GioBatDauChieu = string.IsNullOrEmpty(txtGioBatDauChieu.Value) ? null : txtGioBatDauChieu.Value;
                                obj.GioKetThucChieu = string.IsNullOrEmpty(txtGioKetThucChieu.Value) ? null : txtGioKetThucChieu.Value;
                            }
                            else
                            {
                                obj.GioBatDauSang = null;
                                obj.GioKetThucSang = null;
                                obj.GioBatDauChieu = null;
                                obj.GioKetThucChieu = null;
                            }

                            // 3. Đẩy xuống Manager (NguoiCapNhat đã được Manager lo)
                            LichBieuChungManager.Instance.UpdateCauHinhTuan(obj);
                        }
                    }
                }

                // 4. Hiển thị thông báo thành công xanh lá
                ShowSuccessSaveData();
                BindData();
                upnlTuanLamViec.Update();
            }
            catch (Exception exc)
            {
                // 5. Bắt lỗi: Nếu có trục trặc, hệ thống sẽ báo thông báo đỏ thay vì im lặng
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlLichNgoaiLe1.ConfirmRequest(e);
        }
        // Hàm test gọi Pop-up UserControl
        protected void btnTestPopup_Click(object sender, EventArgs e)
        {
            // Gọi hàm ShowTestModal từ cái Control mà ta đã nhúng
            CtrlChonNhanVien1.ShowTestModal();
        }
    }
}