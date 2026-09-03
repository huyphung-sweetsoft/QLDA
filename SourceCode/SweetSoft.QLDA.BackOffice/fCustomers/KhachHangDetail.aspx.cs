using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCustomers
{
    public partial class KhachHangDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Customer;
            }
        }

        private Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("Id");
                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CUSTOMER_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.CUSTOMER);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Customers, GetResourceText(BackEndResourceKeys.CUSTOMER_LIST) },
                    {"javascript:", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                if (this.QueryId != Guid.Empty)
                {
                    BindData();
                    CtrlDuAn1.IdKhachHang = QueryId;
                    CtrlDuAn1.InitControls();
                }
            }
        }

        protected override void BindData()
        {
            try
            {
                DataTable dt = KhachHangManager.Instance.GetDetailKhachHangById(QueryId);
                if (dt == null || dt.Rows.Count == 0)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                    return;
                }
                DataRow row = dt.Rows[0];
                BindCustomerInformation(row);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void BindCustomerInformation(DataRow row)
        {
            lblTenKhachHang.Text = GetDisplayText(row, "TenKhachHang");
            lblLoaiKhachHangSubLabel.Text = GetDisplayText(row, "TenLoaiKhachHang");
            lblLoaiKhachHang.Text = GetDisplayText(row, "TenLoaiKhachHang");
            lblSoThue.Text = GetDisplayText(row, "IdSoThue");
            lblSoDienThoai.Text = GetDisplayText(row, "SoDienThoai");
            lblEmail.Text = GetDisplayText(row, "Email");
            lblDiaChi.Text = GetDisplayText(row, "DiaChi");
            lblNguoiLienHe.Text = GetDisplayText(row, "TenNguoiLienHe");
            lblDienThoaiLienHe.Text = GetDisplayText(row, "DienThoaiLienHe");
            lblEmailLienHe.Text = GetDisplayText(row, "EmailLienHe");
            ltrMoTa.Text = GetHtmlText(row, "GhiChu");
        }

        private string GetDisplayText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "Chưa có";
            }
            return HttpUtility.HtmlEncode(value);
        }

        private string GetHtmlText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "<p class='text-muted'>Chưa có nội dung mô tả</p>";
            }
            return HttpUtility.HtmlDecode(value);
        }
    }
}