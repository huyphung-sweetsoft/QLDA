using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Models;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
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
        public override bool IsLogin
        {
            get { return true; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // Bắt cờ "isFromProfile" truyền vào Callback để lúc Save vẫn giữ đúng luồng UI
            bool isFromProfile = CommonHelpers.QueryString("from") == "profile";
            CtrlUserDetail1.SavedHandlerCallback += (s, ev) => { LoadDataDetail(CurrentIdNhanVien, isFromProfile); upnlMainDetail.Update(); };

            if (!IsPostBack)
            {
                // 1. LẤY ID TỪ URL
                string idQuery = CommonHelpers.QueryString("id");
                Guid tempId = Guid.Empty;
                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId);
                }

                // [FIX LỖI 403]: TỰ CHECK QUYỀN BẰNG TAY (THAY CHO BASEADMINPAGE)
                bool hasViewRight = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien);

                // Nếu KHÔNG có quyền View VÀ KHÔNG phải đang tự xem chính mình -> Văng 403
                if (!hasViewRight && tempId != SweetContext.Current.UserId)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }
                var navLinks = new Dictionary<string, string>();

                if (isFromProfile)
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.Profile), GetResourceText(BackEndResourceKeys.PROFILE));
                }
                else
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.NhanVien), GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST));
                }

                // Chèn thêm chính nó vào cuối Breadcrumb (dùng javascript:void(0) để vô hiệu hóa click)
                navLinks.Add("javascript:void(0);", GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));

                Navigation1.keyValuePairUrls = navLinks;
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);

                ApplyControlsText();

                CtrlUserDetail1.CurrentMode = UserPopupMode.Employee;
                CtrlUserDetail1.InitControls();

                if (tempId != Guid.Empty)
                {
                    CurrentIdNhanVien = tempId;
                    LoadDataDetail(tempId, isFromProfile);
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

        private void LoadDataDetail(Guid idNhanVien, bool isFromProfile)
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

            // [NÚT SỬA]: Chỉ hiện khi có quyền Edit VÀ không đi từ trang Profile
            btnEditProfile.Visible = this.IsEdit && !isFromProfile;
            string lichUrl = GetRelativeClientPath(RewriteURLHelper.ViewLichCaNhan(idNhanVien));
            lnkSchedule.HRef = isFromProfile ? lichUrl + "?from=profile" : lichUrl;
            lnkSchedule.Visible = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien) || idNhanVien == SweetContext.Current.UserId;

            BindProjectData(idNhanVien);
        }

        private void BindProjectData(Guid idNhanVien)
        {
            var allProjects = ThanhVienDuAnManager.Instance.GetChiTietDuAnCuaNhanVien(idNhanVien);

            section_active_projects.Visible = true;
            section_done_projects.Visible = true;

            if (allProjects != null && allProjects.Count > 0)
            {
                // [CHUẨN HÓA ENUM]: Các dự án Đang thực hiện hoặc Chờ thực hiện
                var activeStatuses = new List<byte> {
                    (byte)DuAnStatus.DangThucHien,
                    (byte)DuAnStatus.ChoThucHien
                };
                var activeProjects = allProjects.Where(p => activeStatuses.Contains(p.TrangThai)).ToList();

                // [CHUẨN HÓA ENUM]: Các dự án thuộc Lịch sử (Hoàn thành, Tạm dừng, Kết thúc)
                var historyStatuses = new List<byte> {
                    (byte)DuAnStatus.HoanThanh,
                    (byte)DuAnStatus.TamDung,
                    (byte)DuAnStatus.KetThuc
                };
                var doneProjects = allProjects.Where(p => historyStatuses.Contains(p.TrangThai)).ToList();

                rptActiveProjects.DataSource = activeProjects;
                rptActiveProjects.DataBind();
                emptyActive.Visible = (activeProjects.Count == 0);

                rptDoneProjects.DataSource = doneProjects;
                rptDoneProjects.DataBind();
                emptyDone.Visible = (doneProjects.Count == 0);

                ltrActiveCount.Text = activeProjects.Count.ToString();
                ltrDoneCount.Text = doneProjects.Count.ToString();
                ltrCountActiveProj.Text = activeProjects.Count.ToString();
                ltrCountDoneProj.Text = doneProjects.Count.ToString();
            }
            else
            {
                rptActiveProjects.DataSource = null;
                rptActiveProjects.DataBind();
                emptyActive.Visible = true;

                rptDoneProjects.DataSource = null;
                rptDoneProjects.DataBind();
                emptyDone.Visible = true;

                ltrActiveCount.Text = "0";
                ltrDoneCount.Text = "0";
                ltrCountActiveProj.Text = "0";
                ltrCountDoneProj.Text = "0";
            }
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

        protected string GetProjectUrl(object idDuAn)
        {
            if (idDuAn == null) return "#";
            return GetRelativeClientPath(RewriteURLHelper.ProjectDetail(Guid.Parse(idDuAn.ToString())));
        }

        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();
        protected string GetTaskPriorityBadge(object tenDoUuTien, object diemDoUuTien) { return _controlHelpers.GetTaskPriorityBadge(tenDoUuTien, diemDoUuTien); }
        protected string GetTaskStatusBadge(object status) { return _controlHelpers.GetTaskStatusBadge(status); }
        protected bool HasTasks(object tasksObj)
        {
            if (tasksObj == null || tasksObj == DBNull.Value) return false;
            var list = tasksObj as System.Collections.IEnumerable;
            if (list != null) { var enumerator = list.GetEnumerator(); return enumerator.MoveNext(); }
            return false;
        }

        protected string FormatDateSafe(object dateObj, string defaultEmptyText = "")
        {
            if (dateObj == null || dateObj == DBNull.Value || string.IsNullOrWhiteSpace(dateObj.ToString())) return defaultEmptyText;
            if (DateTime.TryParse(dateObj.ToString(), out DateTime dt))
            {
                if (dt.Year <= 1900) return defaultEmptyText;
                return dt.ToString("dd/MM/yyyy");
            }
            return defaultEmptyText;
        }

        protected string FormatDateRange(object startDateObj, object endDateObj, string defaultEndText = "")
        {
            string start = FormatDateSafe(startDateObj, "?");
            string end = FormatDateSafe(endDateObj, defaultEndText);
            if (start == "?" && end == defaultEndText) return "";
            return $"{start} — {end}";
        }
    }
}