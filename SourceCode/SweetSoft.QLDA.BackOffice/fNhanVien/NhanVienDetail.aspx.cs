using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class NhanVienDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.NhanVien; }
        }

        private Guid CurrentIdNhanVien
        {
            get
            {
                if (ViewState["IdNhanVien"] != null)
                    return (Guid)ViewState["IdNhanVien"];
                return Guid.Empty;
            }
            set { ViewState["IdNhanVien"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlUserDetail1.SavedHandlerCallback += (s, ev) => { LoadDataDetail(CurrentIdNhanVien); upnlMainDetail.Update(); };
            if (!IsPostBack)
            {
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    { RewriteURLHelper.Users, GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST) }
                };

                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                ApplyControlsText();

                CtrlUserDetail1.CurrentMode = UserPopupMode.Employee;
                CtrlUserDetail1.InitControls();

                string idQuery = CommonHelpers.QueryString("id");
                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid tempId = Guid.Empty;
                    if (Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId))
                    {
                        CurrentIdNhanVien = tempId;
                        LoadDataDetail(tempId);
                    }
                    else
                    {
                        Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                    }
                }
                else
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                }
            }
        }

        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);
            btnEditProfile.Text = btnEditProfile.ToolTip = GetResourceText(BackEndResourceKeys.EDIT_INFORMATION);
            SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));
        }

        private void LoadDataDetail(Guid idNhanVien)
        {
            DataTable dt = UserManager.Instance.GetUserForDetail(idNhanVien);
            if (dt.Rows.Count == 0)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                return;
            }

            DataRow row = dt.Rows[0];
            string notUpdatedText = GetResourceText(BackEndResourceKeys.NOT_UPDATED_YET);

            ltrTenNhanVien.Text = row["DisplayName"].ToString();
            ltrCCCD.Text = !string.IsNullOrEmpty(row["IdCCCD"].ToString()) ? row["IdCCCD"].ToString() : notUpdatedText;
            ltrGioiTinh.Text = !string.IsNullOrEmpty(row["GioiTinh"].ToString()) ? row["GioiTinh"].ToString() : notUpdatedText;
            ltrDiaChi.Text = !string.IsNullOrEmpty(row["DiaChi"].ToString()) ? row["DiaChi"].ToString() : GetResourceText(BackEndResourceKeys.ADDRESS_NOT_UPDATED_YET);

            if (row["NgaySinh"] != DBNull.Value)
                ltrNgaySinh.Text = Convert.ToDateTime(row["NgaySinh"]).ToString("dd/MM/yyyy");
            else
                ltrNgaySinh.Text = notUpdatedText;

            ltrChucDanh.Text = !string.IsNullOrEmpty(row["TenChucDanh"].ToString()) ? row["TenChucDanh"].ToString() : notUpdatedText;
            ltrPhongBan.Text = !string.IsNullOrEmpty(row["TenPhongBan"].ToString()) ? row["TenPhongBan"].ToString() : notUpdatedText;
            ltrEmail.Text = !string.IsNullOrEmpty(row["Email"].ToString()) && !row["Email"].ToString().Contains("no-email.com") ? row["Email"].ToString() : notUpdatedText;
            ltrPhone.Text = !string.IsNullOrEmpty(row["MobileAlias"].ToString()) ? row["MobileAlias"].ToString() : notUpdatedText;

            if (row["NgayGiaNhap"] != DBNull.Value)
            {
                DateTime joinDate = Convert.ToDateTime(row["NgayGiaNhap"]);
                ltrNgayGiaNhap.Text = joinDate.ToString("dd/MM/yyyy");
                ltrThamNien.Text = CalculateSeniority(joinDate);
            }
            else
            {
                ltrNgayGiaNhap.Text = notUpdatedText;
                ltrThamNien.Text = GetResourceText(BackEndResourceKeys.UNIDENTIFIED);
            }

            string avatar = row["Avatar"].ToString();
            if (!string.IsNullOrEmpty(avatar))
                imgAvatar.Src = avatar;

            btnEditProfile.Visible = this.IsEdit;
            lnkSchedule.HRef = GetRelativeClientPath(RewriteURLHelper.ViewLichCaNhan(idNhanVien));
            lnkSchedule.Visible = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien);
        }

        private string CalculateSeniority(DateTime joinDate)
        {
            DateTime now = DateTime.Now;
            if (joinDate > now) return GetResourceText(BackEndResourceKeys.NOT_STARTED_WORKING_YET);

            int years = now.Year - joinDate.Year;
            int months = now.Month - joinDate.Month;

            if (now.Day < joinDate.Day) months--;
            if (months < 0) { years--; months += 12; }

            string yearStr = GetResourceText(BackEndResourceKeys.YEAR).ToLower();
            string monthStr = GetResourceText(BackEndResourceKeys.MONTH).ToLower();
            string dayStr = GetResourceText(BackEndResourceKeys.DAY).ToLower();

            if (years > 0 && months > 0) return $"{years} {yearStr}, {months} {monthStr}";
            if (years > 0) return $"{years} {yearStr}";
            if (months > 0) return $"{months} {monthStr}";

            int days = (now - joinDate).Days;
            return $"{days} {dayStr}";
        }

        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            if (CurrentIdNhanVien != Guid.Empty)
            {
                CtrlUserDetail1.Edit(CurrentIdNhanVien);
            }
        }
    }
}