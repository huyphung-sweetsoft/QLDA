using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.Controls;
using SweetSoft.QLDA.BackOffice.fNhanVien.Controls;
using SweetSoft.QLDA.BackOffice.fUsers.Controls; // Bắt buộc phải có using này để gọi Enum Mode
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class ListNhanVien : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.NhanVien; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlNhanViens1.NewNhanVienHandlerCallback += NewNhanVienAction;
            CtrlNhanViens1.EditNhanVienHandlerCallback += EditNhanVienAction;

            // Hứng tín hiệu lưu thành công từ Popup để báo lưới tải lại
            CtrlNhanVienPopup1.SavedHandlerCallback += (s, ev) => { CtrlNhanViens1.Rebind(); };
            // 1. ĐĂNG KÝ HỨNG SỰ KIỆN QUẢN LÝ DANH MỤC
            CtrlNhanViens1.ManagePhongBanHandlerCallback += ManagePhongBan_Action;
            CtrlNhanViens1.ManageChucDanhHandlerCallback += ManageChucDanh_Action;

            // 2. ĐĂNG KÝ HỨNG SỰ KIỆN TỪ POPUP QUẢN LÝ DANH MỤC KHI CÓ SỰ THAY ĐỔI DỮ LIỆU
            // (Giả sử thẻ CtrlQuanLyLoai trên file .aspx của ông có ID là CtrlQuanLyLoai1)
            CtrlQuanLyLoai1.OnDataChanged += CtrlQuanLyLoai_OnDataChanged;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMPLOYEE_MANAGEMENT));

                if (Navigation1 != null)
                {
                    Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST);
                    Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                    {
                        { RewriteURLHelper.NhanVien, GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST) }
                    };
                }

                // ÉP CHẾ ĐỘ: NHÂN SỰ (Nó sẽ tự động mở thêm Khối C)
                CtrlNhanVienPopup1.InitControls();
                CtrlNhanViens1.InitControls();
                string idQuery = CommonHelpers.QueryString("id");
                if (!string.IsNullOrEmpty(idQuery))
                {
                    if (Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out Guid tempId))
                    {
                        EditNhanVienAction(tempId, EventArgs.Empty);
                    }
                }
            }
        }

        #region Gọi Modal
        private void NewNhanVienAction(object sender, EventArgs e)
        {
            // Ủy quyền hoàn toàn cho Control con tự reset UI và mở Popup
            CtrlNhanVienPopup1.AddNew();
        }

        private void EditNhanVienAction(object sender, EventArgs e)
        {
            if (sender == null) { ShowInvalidDataError(); return; }
            Guid userId = (Guid)sender;
            if (userId == Guid.Empty) { ShowInvalidDataError(); return; }

            // Truyền ID sang Control con để tự gọi DB và fill dữ liệu
            CtrlNhanVienPopup1.Edit(userId);
        }
        #endregion
        #region Gọi Modal Quản lý Danh mục (Phòng ban / Chức danh)

        // Bắt sự kiện bấm nút "Quản lý phòng ban"
        private void ManagePhongBan_Action(object sender, EventArgs e)
        {
            // Mở popup Quản lý loại và truyền tham số đối tượng là "Phòng ban"
            CtrlQuanLyLoai1.ShowModal(LoaiManager.LoaiDoiTuong.PhongBan);
        }

        // Bắt sự kiện bấm nút "Quản lý chức danh"
        private void ManageChucDanh_Action(object sender, EventArgs e)
        {
            // Mở popup Quản lý loại và truyền tham số đối tượng là "Chức danh"
            CtrlQuanLyLoai1.ShowModal(LoaiManager.LoaiDoiTuong.ChucDanh);
        }

        // Bắt sự kiện khi Popup Quản lý Danh mục có thay đổi (Thêm/Sửa/Xóa thành công)
        private void CtrlQuanLyLoai_OnDataChanged(object sender, EventArgs e)
        {
            // Dựa vào thuộc tính DoiTuong của Control để biết vừa sửa cái gì
            // Từ đó gọi lại hàm Reload Dropdown tương ứng bên trong CtrlNhanViens
            if (CtrlQuanLyLoai1.DoiTuong == LoaiManager.LoaiDoiTuong.PhongBan)
            {
                CtrlNhanViens1.ReloadPhongBanDropdown();
            }
            else if (CtrlQuanLyLoai1.DoiTuong == LoaiManager.LoaiDoiTuong.ChucDanh)
            {
                CtrlNhanViens1.ReloadChucDanhDropdown();
            }

            // Rebind lại Lưới Nhân viên luôn để trường hợp Sửa tên thì lưới tự cập nhật tên mới
            CtrlNhanViens1.Rebind();
        }

        #endregion
        #region Xóa Dữ liệu
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlNhanViens1.ConfirmRequest(e);
        }

        public override void CloseRequest(ConfirmResult e)
        {
            base.CloseRequest(e);
        }
        #endregion
    }
}