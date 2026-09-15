using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fNhanVien.Controls;
using SweetSoft.QLDA.BackOffice.fUsers.Controls; // Bắt buộc phải có using này để gọi Enum Mode
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
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
            CtrlUserDetail1.SavedHandlerCallback += (s, ev) => { CtrlNhanViens1.Rebind(); };

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
                CtrlUserDetail1.CurrentMode = UserPopupMode.Employee;
                CtrlUserDetail1.InitControls();

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
            CtrlUserDetail1.AddNew();
        }

        private void EditNhanVienAction(object sender, EventArgs e)
        {
            if (sender == null) { ShowInvalidDataError(); return; }
            Guid userId = (Guid)sender;
            if (userId == Guid.Empty) { ShowInvalidDataError(); return; }

            // Truyền ID sang Control con để tự gọi DB và fill dữ liệu
            CtrlUserDetail1.Edit(userId);
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