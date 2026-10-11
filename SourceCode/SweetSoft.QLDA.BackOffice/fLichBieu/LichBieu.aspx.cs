using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web.UI;
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
                BindDefaultWorkingHours();
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
        private void BindDefaultWorkingHours()
        {
            SettingManager settingManager = new SettingManager();

            hdfDefaultMorningStart.Value =
                settingManager.GetSettingValue(SettingKeys.DefaultMorningStartTime);

            hdfDefaultMorningEnd.Value =
                settingManager.GetSettingValue(SettingKeys.DefaultMorningEndTime);

            hdfDefaultAfternoonStart.Value =
                settingManager.GetSettingValue(SettingKeys.DefaultAfternoonStartTime);

            hdfDefaultAfternoonEnd.Value =
                settingManager.GetSettingValue(SettingKeys.DefaultAfternoonEndTime);

            // Fallback nếu hệ thống chưa có cấu hình
            if (string.IsNullOrEmpty(hdfDefaultMorningStart.Value))
                hdfDefaultMorningStart.Value = "08:00";

            if (string.IsNullOrEmpty(hdfDefaultMorningEnd.Value))
                hdfDefaultMorningEnd.Value = "12:00";

            if (string.IsNullOrEmpty(hdfDefaultAfternoonStart.Value))
                hdfDefaultAfternoonStart.Value = "13:00";

            if (string.IsNullOrEmpty(hdfDefaultAfternoonEnd.Value))
                hdfDefaultAfternoonEnd.Value = "17:00";
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
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            TblCauHinhTuanLamViec item =
                e.Item.DataItem as TblCauHinhTuanLamViec;

            if (item == null)
                return;

            Literal ltrTenThu =
                (Literal)e.Item.FindControl("ltrTenThu");

            if (ltrTenThu != null)
            {
                if (item.NgayTrongTuan == 0)
                    ltrTenThu.Text = "Chủ Nhật";
                else
                    ltrTenThu.Text = "Thứ " + (item.NgayTrongTuan + 1);
            }

            bool caSangOn =
                !string.IsNullOrWhiteSpace(item.GioBatDauSang) &&
                !string.IsNullOrWhiteSpace(item.GioKetThucSang);

            bool caChieuOn =
                !string.IsNullOrWhiteSpace(item.GioBatDauChieu) &&
                !string.IsNullOrWhiteSpace(item.GioKetThucChieu);

            ExtraCheckbox chkCaSang =
                (ExtraCheckbox)e.Item.FindControl("chkCaSang");

            ExtraCheckbox chkCaChieu =
                (ExtraCheckbox)e.Item.FindControl("chkCaChieu");

            Literal ltrTrangThaiNgay =
                (Literal)e.Item.FindControl("ltrTrangThaiNgay");

            HtmlInputGenericControl txtGioBatDauSang =
                (HtmlInputGenericControl)e.Item.FindControl("txtGioBatDauSang");

            HtmlInputGenericControl txtGioKetThucSang =
                (HtmlInputGenericControl)e.Item.FindControl("txtGioKetThucSang");

            HtmlInputGenericControl txtGioBatDauChieu =
                (HtmlInputGenericControl)e.Item.FindControl("txtGioBatDauChieu");

            HtmlInputGenericControl txtGioKetThucChieu =
                (HtmlInputGenericControl)e.Item.FindControl("txtGioKetThucChieu");

            if (chkCaSang != null)
                chkCaSang.Checked = caSangOn;

            if (chkCaChieu != null)
                chkCaChieu.Checked = caChieuOn;

            if (ltrTrangThaiNgay != null)
                ltrTrangThaiNgay.Text = caSangOn || caChieuOn ? "Ngày làm việc" : "Ngày nghỉ";

            // Chỉ đổ dữ liệu ở server. Trạng thái disabled được khởi tạo bằng JS
            // sau khi trang render để bảo đảm bật ca có thể mở ngay các ô giờ.
            if (txtGioBatDauSang != null)
                txtGioBatDauSang.Value = item.GioBatDauSang ?? string.Empty;

            if (txtGioKetThucSang != null)
                txtGioKetThucSang.Value = item.GioKetThucSang ?? string.Empty;

            if (txtGioBatDauChieu != null)
                txtGioBatDauChieu.Value = item.GioBatDauChieu ?? string.Empty;

            if (txtGioKetThucChieu != null)
                txtGioKetThucChieu.Value = item.GioKetThucChieu ?? string.Empty;
        }
        private bool IsValidShiftTimeRange(string startValue, string endValue)
        {
            if (string.IsNullOrWhiteSpace(startValue) || string.IsNullOrWhiteSpace(endValue))
                return false;

            if (!TimeSpan.TryParse(startValue, System.Globalization.CultureInfo.InvariantCulture, out TimeSpan startTime))
                return false;

            if (!TimeSpan.TryParse(endValue, System.Globalization.CultureInfo.InvariantCulture, out TimeSpan endTime))
                return false;

            return startTime < endTime;
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

                // Validate toàn bộ ca đang bật trước khi mở transaction.
                // Nhờ vậy lỗi ở một ngày không khiến dữ liệu của ngày trước đó bị cập nhật dở.
                foreach (RepeaterItem row in rptTuanLamViec.Items)
                {
                    if (row.ItemType != ListItemType.Item &&
                        row.ItemType != ListItemType.AlternatingItem)
                        continue;

                    Literal ltrTenThu = (Literal)row.FindControl("ltrTenThu");
                    string tenThu = ltrTenThu != null ? ltrTenThu.Text : "Ngày trong tuần";

                    ExtraCheckbox chkCaSang = (ExtraCheckbox)row.FindControl("chkCaSang");
                    ExtraCheckbox chkCaChieu = (ExtraCheckbox)row.FindControl("chkCaChieu");

                    HtmlInputGenericControl txtGioBatDauSang =
                        (HtmlInputGenericControl)row.FindControl("txtGioBatDauSang");
                    HtmlInputGenericControl txtGioKetThucSang =
                        (HtmlInputGenericControl)row.FindControl("txtGioKetThucSang");
                    HtmlInputGenericControl txtGioBatDauChieu =
                        (HtmlInputGenericControl)row.FindControl("txtGioBatDauChieu");
                    HtmlInputGenericControl txtGioKetThucChieu =
                        (HtmlInputGenericControl)row.FindControl("txtGioKetThucChieu");

                    if (chkCaSang != null && chkCaSang.Checked &&
                        !IsValidShiftTimeRange(txtGioBatDauSang.Value, txtGioKetThucSang.Value))
                    {
                        ShowNotify(tenThu + ": giờ ca sáng không hợp lệ. Vui lòng nhập đủ giờ và bảo đảm giờ bắt đầu nhỏ hơn giờ kết thúc.", MSGType.Error);
                        return;
                    }

                    if (chkCaChieu != null && chkCaChieu.Checked &&
                        !IsValidShiftTimeRange(txtGioBatDauChieu.Value, txtGioKetThucChieu.Value))
                    {
                        ShowNotify(tenThu + ": giờ ca chiều không hợp lệ. Vui lòng nhập đủ giờ và bảo đảm giờ bắt đầu nhỏ hơn giờ kết thúc.", MSGType.Error);
                        return;
                    }
                }

                List<TblCauHinhTuanLamViec> currentList =
                    LichBieuChungManager.Instance.GetAllCauHinhTuan();

                using (var scope = new TransactionScope())
                {
                    foreach (RepeaterItem row in rptTuanLamViec.Items)
                    {
                        if (row.ItemType != ListItemType.Item &&
                            row.ItemType != ListItemType.AlternatingItem)
                            continue;

                        HiddenField hdfIdCauHinh =
                            (HiddenField)row.FindControl("hdfIdCauHinh");

                        ExtraCheckbox chkCaSang =
                            (ExtraCheckbox)row.FindControl("chkCaSang");

                        ExtraCheckbox chkCaChieu =
                            (ExtraCheckbox)row.FindControl("chkCaChieu");

                        HtmlInputGenericControl txtGioBatDauSang =
                            (HtmlInputGenericControl)row.FindControl("txtGioBatDauSang");

                        HtmlInputGenericControl txtGioKetThucSang =
                            (HtmlInputGenericControl)row.FindControl("txtGioKetThucSang");

                        HtmlInputGenericControl txtGioBatDauChieu =
                            (HtmlInputGenericControl)row.FindControl("txtGioBatDauChieu");

                        HtmlInputGenericControl txtGioKetThucChieu =
                            (HtmlInputGenericControl)row.FindControl("txtGioKetThucChieu");

                        Guid idCauHinh = Guid.Parse(hdfIdCauHinh.Value);
                        TblCauHinhTuanLamViec obj =
                            currentList.Find(x => x.IdCauHinh == idCauHinh);

                        if (obj == null)
                            continue;

                        bool caSangOn = chkCaSang != null && chkCaSang.Checked;
                        bool caChieuOn = chkCaChieu != null && chkCaChieu.Checked;

                        // Chỉ cần có một ca bật thì ngày vẫn là ngày làm việc.
                        obj.LaNgayLamViec = caSangOn || caChieuOn;

                        obj.GioBatDauSang = caSangOn ? txtGioBatDauSang.Value : null;
                        obj.GioKetThucSang = caSangOn ? txtGioKetThucSang.Value : null;
                        obj.GioBatDauChieu = caChieuOn ? txtGioBatDauChieu.Value : null;
                        obj.GioKetThucChieu = caChieuOn ? txtGioKetThucChieu.Value : null;

                        LichBieuChungManager.Instance.UpdateCauHinhTuan(obj);
                    }

                    scope.Complete();
                }

                LichBieuChungManager.Instance.ForceRefreshCache();
                //TaskManager.Instance.SyncPendingTasksAfterScheduleChange(DateTime.Today);

                ShowSuccessSaveData();
                BindData();
                upnlTuanLamViec.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlLichNgoaiLe1.ConfirmRequest(e);
        }
        // Hàm test gọi Pop-up UserControl

    }
}