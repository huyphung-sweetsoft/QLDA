using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fProjectTemplate.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fProjectTemplate
{
    public partial class ProjectTemplateList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Project;

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTemplate1.NewTemplateHandlerCallback += NewTemplateAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }

                SetMetaTagsOgTags("Quản lý mẫu dự án");
                Navigation1.MainTitle = "Quản lý mẫu dự án";
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { "javascript:;", "Mẫu dự án" }
                };

                CtrlProjectTemplate1.InitControls();
            }
        }

        private void NewTemplateAction(object sender, EventArgs e)
        {
            txtTenMau.Text = "";
            txtMoTa.Text = "";
            mdlTemplate.OpenModal(true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                TblMauDuAn mau = new TblMauDuAn
                {
                    IdMau = Guid.NewGuid(),
                    TenMau = txtTenMau.Text.Trim(),
                    MoTa = txtMoTa.Text.Trim(),
                    TrangThai = 1,
                    NgayTao = DateTime.Now,
                    NguoiTao = SweetContext.Current.UserName,
                    DaXoa = false
                };

                mau.Save();

                mdlTemplate.CloseModal();

                Response.Redirect(RewriteURLHelper.ProjectTemplateDetail(mau.IdMau), false);
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi tạo mẫu: " + ex.Message, MSGType.Error);
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlProjectTemplate1.ConfirmRequest(e);
        }
    }
}