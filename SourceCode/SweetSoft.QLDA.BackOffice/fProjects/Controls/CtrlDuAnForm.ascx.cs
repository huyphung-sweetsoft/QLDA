using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlDuAnForm : BaseAdminUserControl
    {
        public event EventHandler SaveCompleted;
        public Guid IdDuAn
        {
            get
            {
                if (ViewState["IdDuAn"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["IdDuAn"];
            }
            set
            {
                ViewState["IdDuAn"] = value;
            }
        }

        public Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] == null)
                    return Guid.Empty;

                return (Guid)ViewState["IdHopDongThucHien"];
            }
            set
            {
                ViewState["IdHopDongThucHien"] = value;
            }
        }

        public List<Guid> SelectedMemberIds
        {
            get
            {
                if (ViewState["SelectedMemberIds"] == null)
                    return new List<Guid>();

                return (List<Guid>)ViewState["SelectedMemberIds"];
            }
            set
            {
                ViewState["SelectedMemberIds"] = value;
            }
        }

        protected bool IsAdd
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Create, ModuleKeys.Project))
                    return true;
                return false;
            }
        }

        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.Project))
                    return true;
                return false;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            //Đăng ký lắng nghe cái popup chọn nhân viên
            CtrlChonNhanVien1.OnConfirmSelection += CtrlChonNhanVien1_OnConfirmSelection;

            txtSoHopDong.EnterSubmitClientID = btnSearchHopDong.ClientID;
            ApplyControlsText();
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                Guid idLoaiDuAn = Guid.Empty;
                Guid idKhachHang = Guid.Empty;
                Guid idNhanVienQuanLy = Guid.Empty;
                byte trangThai = 0;

                if (string.IsNullOrWhiteSpace(txtTenDuAn.Text))
                {
                    validationEngine.AddErrorPrompt(txtTenDuAn.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }
                if (!Guid.TryParse(ddlNhanVienQuanLy.SelectedValue, out idNhanVienQuanLy) || idNhanVienQuanLy == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlNhanVienQuanLy.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }

                if (!Guid.TryParse(ddlKhachHang.SelectedValue, out idKhachHang) || idKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlKhachHang.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }

                if (!Guid.TryParse(ddlLoaiDuAn.SelectedValue, out idLoaiDuAn) || idLoaiDuAn == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlLoaiDuAn.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!byte.TryParse(ddlTrangThai.SelectedValue, out trangThai) || !Enum.IsDefined(typeof(DuAnStatus), trangThai))
                {
                    validationEngine.AddErrorPrompt(ddlTrangThai.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!string.IsNullOrWhiteSpace(txtSoHopDong.Text) && IdHopDongThucHien == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(txtSoHopDong.ClientID, "Số hợp đồng không tồn tại.");
                }
                if (!dtNgayBatDau.DateValue.HasValue)
                {
                    validationEngine.AddErrorPrompt(dtNgayBatDau.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }
                if (!dtNgayKetThuc.DateValue.HasValue)
                {
                    validationEngine.AddErrorPrompt(dtNgayKetThuc.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }

                if (dtNgayBatDau.DateValue.HasValue && dtNgayKetThuc.DateValue.HasValue && dtNgayKetThuc.DateValue.Value.Date < dtNgayBatDau.DateValue.Value.Date)
                {
                    validationEngine.AddErrorPrompt(dtNgayKetThuc.ClientID, "Ngày hoàn thành dự kiến phải bằng hoặc sau ngày bắt đầu.");
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                bool isAdd = true;
                DuAnManager organizationDuAnManager = DuAnManager.Instance;
                TblDuAn duAn = new TblDuAn();
                if (this.IdDuAn != Guid.Empty)
                {
                    duAn.IdDuAn = this.IdDuAn;
                    if (!this.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    isAdd = false;
                }
                else if (!this.IsAdd)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                if (DuAnManager.Instance.IsProjectCodeExists(txtMaDuAn.Text, this.IdDuAn))
                {
                    ShowNotify("Mã dự án đã tồn tại.", MSGType.Error);
                    return;
                }
                duAn.MaDuAn = txtMaDuAn.Text;
                duAn.TenDuAn = txtTenDuAn.Text.Trim();
                duAn.MoTa = txtMoTa.Text.Trim();
                duAn.IdHopDongThucHien = this.IdHopDongThucHien == Guid.Empty ? (Guid?)null : this.IdHopDongThucHien;
                duAn.TrangThai = trangThai;
                //duAn.TrangThai = (byte)ddlTrangThai.SelectedValue;
                if (dtNgayBatDau.DateValueForDisplay.HasValue)
                    duAn.NgayBatDau = dtNgayBatDau.DateValueForDisplay.Value.Date;
                if (dtNgayKetThuc.DateValueForDisplay.HasValue)
                    duAn.NgayDuKienHoanThanh = dtNgayKetThuc.DateValueForDisplay.Value.Date;
                if (Guid.TryParse(ddlLoaiDuAn.SelectedValue, out idLoaiDuAn) && idLoaiDuAn != Guid.Empty)
                {
                    duAn.IdLoaiDuAn = idLoaiDuAn;
                }

                if (Guid.TryParse(ddlKhachHang.SelectedValue, out idKhachHang) && idKhachHang != Guid.Empty)
                {
                    duAn.IdKhachHang = idKhachHang;
                }

                if (Guid.TryParse(ddlNhanVienQuanLy.SelectedValue, out idNhanVienQuanLy) && idNhanVienQuanLy != Guid.Empty)
                {
                    duAn.IdNhanVienQuanLy = idNhanVienQuanLy;
                }
                // Chốt chặn cuối: đảm bảo PM không bị lẫn vào danh sách Thành viên trước khi lưu
                this.SelectedMemberIds.Remove(idNhanVienQuanLy);
                if (isAdd)
                {
                    duAn = organizationDuAnManager.CreateOrUpdate(duAn, this.SelectedMemberIds);//truyền thêm list nhân viên được chọn để lưu vào bảng ThanhVienDuAn
                    if (duAn == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    duAn = organizationDuAnManager.CreateOrUpdate(duAn, this.SelectedMemberIds);
                    if (duAn == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }
            dlDetail.CloseModal();
            SaveCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void OpenAdd()
        {
            RefreshProjectInfo();
            txtMaDuAn.Text = DuAnManager.Instance.GenerateProjectCode();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        public void OpenEdit(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            RefreshProjectInfo();

            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(idDuAn);

            if (duAn == null || duAn.DaXoa)
            {
                Response.Redirect(
                    GetRelativeClientPath(RewriteURLHelper.Error404),
                    false);
                return;
            }

            this.IdDuAn = duAn.IdDuAn;

            txtMaDuAn.Text = duAn.MaDuAn;
            txtMaDuAn.Enabled = true;

            txtTenDuAn.Text = duAn.TenDuAn;
            txtMoTa.Text = duAn.MoTa;

            ddlLoaiDuAn.SelectedValue = duAn.IdLoaiDuAn.ToString();
            ddlKhachHang.SelectedValue = duAn.IdKhachHang.ToString();
            ddlNhanVienQuanLy.SelectedValue = duAn.IdNhanVienQuanLy.ToString();
            ddlTrangThai.SelectedValue = duAn.TrangThai.ToString();

            dtNgayBatDau.DateValue = duAn.NgayBatDau;
            dtNgayKetThuc.DateValue = duAn.NgayDuKienHoanThanh;

            LoadHopDongThucHien(duAn);

            this.SelectedMemberIds =
                DuAnManager.Instance.GetMemberIds(duAn.IdDuAn);

            UpdateMemberCountUI();

            lbtSubmit.Visible = this.IsEdit;

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);

            dlDetail.OpenModal(true);
        }

        protected void txtSoHopDong_TextChanged(object sender, EventArgs e)
        {
            IdHopDongThucHien = Guid.Empty;
            txtGiaTriHopDong.Text = "";
            txtNgayKy.Text = "";

            string soHopDong = txtSoHopDong.Text.Trim();
            if (string.IsNullOrEmpty(soHopDong))
                return;
            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetBySoHopDong(soHopDong);

            if (hopDong != null)
            {
                IdHopDongThucHien = hopDong.IdHopDongThucHien;
                txtGiaTriHopDong.Text = hopDong.GiaTriHopDong.ToString();
                txtNgayKy.Text = hopDong.NgayKy.ToString();
            }
            else
                return;
            upHopDong.Update();
        }

        protected void ddlNhanVienQuanLy_SelectedIndexChanged(object sender, EventArgs e)
        {
            Guid idPM;
            if (Guid.TryParse(ddlNhanVienQuanLy.SelectedValue, out idPM) && idPM != Guid.Empty)
            {
                // Kiểm tra xem ông PM mới này có đang nằm trong danh sách chờ lưu không
                if (this.SelectedMemberIds.Contains(idPM))
                {
                    // Xóa ngay lập tức khỏi bộ nhớ tạm
                    this.SelectedMemberIds.Remove(idPM);

                    // Cập nhật lại câu chữ "Đã chọn X nhân viên"
                    UpdateMemberCountUI();

                    // Bắn Ajax cập nhật riêng cái vùng UI của textbox số lượng
                    upNhanVienThamGia.Update();
                }
            }
        }

        protected void btnChonNhanVien_Click(object sender, EventArgs e)
        {
            if (!dtNgayBatDau.DateValue.HasValue || !dtNgayKetThuc.DateValue.HasValue)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_START_AND_END_DATE), MSGType.Error);
                return;
            }

            Guid idPM;
            Guid.TryParse(ddlNhanVienQuanLy.SelectedValue, out idPM);
            if (idPM == Guid.Empty)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE), MSGType.Error);
                return;
            }

            CtrlChonNhanVien1.StartDate = dtNgayBatDau.DateValue;
            CtrlChonNhanVien1.EndDate = dtNgayKetThuc.DateValue;
            CtrlChonNhanVien1.SelectedUserIds = this.SelectedMemberIds;
            CtrlChonNhanVien1.IdNhanVienQuanLy = idPM;
            CtrlChonNhanVien1.OpenPicker();
        }

        private void RefreshProjectInfo()
        {
            new ControlHelpers().BindLoaiDuAn(ddlLoaiDuAn);
            new ControlHelpers().BindKhachHang(ddlKhachHang);
            new ControlHelpers().BindDuAnStatus(ddlTrangThai);
            new ControlHelpers().BindNhanVien(ddlNhanVienQuanLy);
            lbtSubmit.Visible = false;
            //---------------------------------------------------
            txtMaDuAn.Enabled = true;
            txtMaDuAn.Text = txtTenDuAn.Text
                = txtGiaTriHopDong.Text
                = txtSoHopDong.Text
                = txtNgayKy.Text = "";
            this.IdHopDongThucHien = Guid.Empty;
            dtNgayBatDau.DateValue = null;
            dtNgayKetThuc.DateValue = null;
            ddlTrangThai.SelectedIndex = 0;
            ddlKhachHang.SelectedIndex = -1;
            ddlLoaiDuAn.SelectedIndex = -1;
            ddlNhanVienQuanLy.SelectedIndex = -1;
            this.IdDuAn = Guid.Empty;
            //Dọn sạch data rác của list nv trước khi nhấn nút thêm dự án
            this.SelectedMemberIds = new List<Guid>();
            UpdateMemberCountUI();
        }

        private void ApplyControlsText()
        {
            ddlKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlLoaiDuAn.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlNhanVienQuanLy.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            txtTenDuAn.PlaceHolder = txtGiaTriHopDong.PlaceHolder
                = txtSoHopDong.PlaceHolder
                = txtNgayKy.PlaceHolder
                = txtMaDuAn.PlaceHolder = "";
        }

        private void UpdateMemberCountUI()
        {
            int count = this.SelectedMemberIds.Count;
            txtSoLuongNhanVien.Text = count > 0 ? $"Đã chọn {count} nhân viên" : "";
        }

        private void LoadHopDongThucHien(TblDuAn duAn)
        {
            this.IdHopDongThucHien = Guid.Empty;

            txtSoHopDong.Text = "";
            txtGiaTriHopDong.Text = "";
            txtNgayKy.Text = "";

            if (!duAn.IdHopDongThucHien.HasValue || duAn.IdHopDongThucHien.Value == Guid.Empty)
            {
                return;
            }

            TblHopDongThucHien hd = HopDongThucHienManager.Instance.GetHopDongById(duAn.IdHopDongThucHien.Value);

            if (hd == null)
                return;

            this.IdHopDongThucHien = hd.IdHopDongThucHien;
            txtSoHopDong.Text = hd.SoHopDong;
            txtGiaTriHopDong.Text = hd.GiaTriHopDong.ToString();
            txtNgayKy.Text = hd.NgayKy.ToString();
        }

        private bool GetDropdownValue(ExtraDropdown input, out Guid result)
        {
            result = Guid.Empty;

            if (string.IsNullOrEmpty(input.SelectedValue))
                return true;

            return Guid.TryParse(input.SelectedValue, out result);
        }


        // MỚI: nhận kết quả từ popup khi bấm Xác nhận
        private void CtrlChonNhanVien1_OnConfirmSelection(List<Guid> selectedIds)
        {
            this.SelectedMemberIds = selectedIds;
            UpdateMemberCountUI();
            upNhanVienThamGia.Update();
        }
    }
}