using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
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
            // Kích hoạt Event đăng ký AJAX PostBack
            PrepareSearchControls();

            bool isFromProfile = CommonHelpers.QueryString("from") == "profile";
            CtrlUserDetail1.SavedHandlerCallback += (s, ev) => { LoadDataDetail(CurrentIdNhanVien, isFromProfile); upnlMainDetail.Update(); };

            if (!IsPostBack)
            {
                string idQuery = CommonHelpers.QueryString("id");
                Guid tempId = Guid.Empty;
                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId);
                }

                bool hasViewRight = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien);

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

                navLinks.Add("javascript:void(0);", GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));

                Navigation1.keyValuePairUrls = navLinks;
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);

                ApplyControlsText();
                InitFilterControls();

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

        private void PrepareSearchControls()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
            {
                script.RegisterAsyncPostBackControl(btnSearchDuAn);
                script.RegisterAsyncPostBackControl(ddlSearchTrangThaiDuAn);
            }
            txtSearchDuAn.EnterSubmitClientID = btnSearchDuAn.ClientID;
        }

        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);
            btnEditProfile.Text = btnEditProfile.ToolTip = GetResourceText(BackEndResourceKeys.EDIT_INFORMATION);

            ddlSearchTrangThaiDuAn.SearchTagItemText = "Trạng thái dự án";
            txtSearchDuAn.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);

            SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));
        }

        private void InitFilterControls()
        {
            new ControlHelpers().BindDuAnStatus(ddlSearchTrangThaiDuAn);
            ddlSearchTrangThaiDuAn.ClearSelection();
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

            bool isActivated = true;
            if (row.Table.Columns.Contains("IsActivated") && row["IsActivated"] != DBNull.Value)
                isActivated = Convert.ToBoolean(row["IsActivated"]);
            else if (row.Table.Columns.Contains("IsLockedOut") && row["IsLockedOut"] != DBNull.Value)
                isActivated = !Convert.ToBoolean(row["IsLockedOut"]);
            else if (row.Table.Columns.Contains("DaXoa") && row["DaXoa"] != DBNull.Value)
                isActivated = !Convert.ToBoolean(row["DaXoa"]);

            if (isActivated)
            {
                ltrTinhTrangCongTac.Text = $"<span class=\"fw-bold text-success fs-6\">{GetResourceText(BackEndResourceKeys.WORKING)}</span>";
            }
            else
            {
                ltrTinhTrangCongTac.Text = $"<span class=\"fw-bold text-secondary fs-6\">{GetResourceText(BackEndResourceKeys.INACTIVE) ?? "Đã nghỉ"}</span>";
            }

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

            btnEditProfile.Visible = this.IsEdit && !isFromProfile;
            string lichUrl = GetRelativeClientPath(RewriteURLHelper.ViewLichCaNhan(idNhanVien));
            lnkSchedule.HRef = isFromProfile ? lichUrl + "?from=profile" : lichUrl;
            lnkSchedule.Visible = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien) || idNhanVien == SweetContext.Current.UserId;

            BindProjectData(true);
        }

        private void BindProjectData(bool isInitialLoad = false)
        {
            // =========================================================
            // TẦNG 1:
            // Không còn load toàn bộ Project rồi mới lọc bằng LINQ.
            // Keyword + Status được truyền xuống Manager để SQL xử lý.
            // =========================================================

            string keyword = (txtSearchDuAn.Text ?? string.Empty).Trim();

            byte? statusId = null;

            string statusFilter = ddlSearchTrangThaiDuAn.SelectedValue;

            if (!string.IsNullOrEmpty(statusFilter) &&
                byte.TryParse(statusFilter, out byte parsedStatusId))
            {
                statusId = parsedStatusId;
            }

            // Lấy dữ liệu đã được SQL filter sẵn.
            List<NhanVienProjectDTO> allProjects =
                ThanhVienDuAnManager.Instance.GetChiTietDuAnCuaNhanVien(
                    CurrentIdNhanVien,
                    keyword,
                    statusId
                );

            // =========================================================
            // Count tổng Project:
            // Chỉ lấy ở lần load đầu tiên.
            // Không dùng allProjects.Count nữa vì allProjects lúc này
            // có thể đã bị filter bởi keyword/status.
            // =========================================================
            if (isInitialLoad)
            {
                int totalProjectCount =
                    ThanhVienDuAnManager.Instance.CountDuAnCuaNhanVien(
                        CurrentIdNhanVien
                    );

                ltrCountAllProj.Text = totalProjectCount.ToString();
            }

            // =========================================================
            // Bind dữ liệu kết quả
            // =========================================================
            if (allProjects != null && allProjects.Count > 0)
            {
                rptAllProjects.DataSource = allProjects;
                rptAllProjects.DataBind();

                emptyAll.Visible = false;
            }
            else
            {
                rptAllProjects.DataSource = null;
                rptAllProjects.DataBind();

                emptyAll.Visible = true;
            }

            // Ép UpdatePanel của danh sách Project cập nhật UI.
            upListDuAn.Update();
        }

        #region Xử lý Sự kiện Tìm kiếm

        protected void btnSearchDuAn_Click(object sender, EventArgs e)
        {
            UpdateSearchTags();
            BindProjectData(false);
        }

        protected void ddlSearchTrangThaiDuAn_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateSearchTags();
            BindProjectData(false);
        }

        private void UpdateSearchTags()
        {
            searchTagBoxDuAn.TagItems.Clear();

            string statusValue = ddlSearchTrangThaiDuAn.SelectedValue;
            if (!string.IsNullOrEmpty(statusValue) && ddlSearchTrangThaiDuAn.SearchTagItem != null)
            {
                searchTagBoxDuAn.TagItems.Add(ddlSearchTrangThaiDuAn.SearchTagItem);
            }

            string keyword = txtSearchDuAn.Text.Trim();
            if (!string.IsNullOrEmpty(keyword) && txtSearchDuAn.SearchTagItem != null)
            {
                searchTagBoxDuAn.TagItems.Add(txtSearchDuAn.SearchTagItem);
            }

            searchTagBoxDuAn.Update();
            upSearchTagDuAn.Update(); // Bắt UpdatePanel của TagBox cập nhật lại UI
        }

        protected void searchTagBoxDuAn_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                if (tag.Key == ddlSearchTrangThaiDuAn.ClientID)
                {
                    ddlSearchTrangThaiDuAn.ClearSelection();
                    upnlSearchDuAn.Update(); // Ép UpdatePanel Dropdown xóa giao diện cũ
                }
                else if (tag.Key == txtSearchDuAn.ClientID)
                {
                    txtSearchDuAn.Text = string.Empty;
                    // Ép JS xóa mặt UI đề phòng Textbox không reset kịp
                    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "ClearTxtSearchDuAn",
                        string.Format("$('#{0}').val('');", txtSearchDuAn.ClientID), true);
                    upnlSearchDuAn.Update();
                }

                UpdateSearchTags();
                BindProjectData(false);
            }
            catch (Exception exc)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra.", MSGType.Error);
            }
        }

        #endregion

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