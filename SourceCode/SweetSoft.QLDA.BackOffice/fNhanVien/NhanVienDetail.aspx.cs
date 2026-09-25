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
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SweetSoft.QLDA.Core.ScheduleManager;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class NhanVienDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE { get { return ModuleKeys.NhanVien; } }

        private Guid CurrentIdNhanVien
        {
            get { return ViewState["IdNhanVien"] != null ? (Guid)ViewState["IdNhanVien"] : Guid.Empty; }
            set { ViewState["IdNhanVien"] = value; }
        }

        public override bool IsLogin { get { return true; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            PrepareSearchControls();
            bool isFromProfile = CommonHelpers.QueryString("from") == "profile";
            CtrlNhanVienPopup1.SavedHandlerCallback += (s, ev) =>
            {
                LoadDataDetail(CurrentIdNhanVien, isFromProfile);
                upnlMainDetail.Update();
            };

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
                CtrlNhanVienPopup1.InitControls();

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

            if (dt == null || dt.Rows.Count == 0)
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
            {
                ltrNgaySinh.Text = Convert.ToDateTime(row["NgaySinh"]).ToString("dd/MM/yyyy");
            }
            else
            {
                ltrNgaySinh.Text = notUpdatedText;
            }

            ltrChucDanh.Text = !string.IsNullOrEmpty(row["TenChucDanh"].ToString()) ? row["TenChucDanh"].ToString() : notUpdatedText;
            ltrPhongBan.Text = !string.IsNullOrEmpty(row["TenPhongBan"].ToString()) ? row["TenPhongBan"].ToString() : notUpdatedText;

            ltrEmail.Text = !string.IsNullOrEmpty(row["Email"].ToString()) && !row["Email"].ToString().Contains("no-email.com") ? row["Email"].ToString() : notUpdatedText;
            ltrPhone.Text = !string.IsNullOrEmpty(row["MobileAlias"].ToString()) ? row["MobileAlias"].ToString() : notUpdatedText;

            bool isActivated = true;
            if (row.Table.Columns.Contains("IsActivated") && row["IsActivated"] != DBNull.Value)
            {
                isActivated = Convert.ToBoolean(row["IsActivated"]);
            }
            else if (row.Table.Columns.Contains("IsLockedOut") && row["IsLockedOut"] != DBNull.Value)
            {
                isActivated = !Convert.ToBoolean(row["IsLockedOut"]);
            }
            else if (row.Table.Columns.Contains("DaXoa") && row["DaXoa"] != DBNull.Value)
            {
                isActivated = !Convert.ToBoolean(row["DaXoa"]);
            }

            if (isActivated)
            {
                ltrTinhTrangCongTac.Text = string.Format("<span class=\"fw-bold text-success fs-6\">{0}</span>", GetResourceText(BackEndResourceKeys.WORKING));
            }
            else
            {
                ltrTinhTrangCongTac.Text = string.Format("<span class=\"fw-bold text-secondary fs-6\">{0}</span>", GetResourceText(BackEndResourceKeys.INACTIVE) ?? "Đã nghỉ");
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
            {
                imgAvatar.Src = avatar;
            }

            btnEditProfile.Visible = this.IsEdit && !isFromProfile;
            string lichUrl = GetRelativeClientPath(RewriteURLHelper.ViewLichCaNhan(idNhanVien));
            lnkSchedule.HRef = isFromProfile ? lichUrl + "?from=profile" : lichUrl;
            lnkSchedule.Visible = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien) || idNhanVien == SweetContext.Current.UserId;

            BindProjectList(true);
            DateTime now = DateTime.Now;
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            hdfScheduleJson.Value = GenerateScheduleJson(idNhanVien, startOfMonth, endOfMonth);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "RenderMiniCal", "renderEmployeeMonthCalendar();", true);
        }

        private void BindProjectList(bool isInitialLoad = false)
        {
            string keyword = (txtSearchDuAn.Text ?? string.Empty).Trim();
            byte? statusId = null;
            string statusFilter = ddlSearchTrangThaiDuAn.SelectedValue;

            if (!string.IsNullOrEmpty(statusFilter) && byte.TryParse(statusFilter, out byte parsedStatusId))
            {
                statusId = parsedStatusId;
            }

            List<NhanVienProjectSummaryDTO> projects = ThanhVienDuAnManager.Instance.GetDanhSachDuAnCuaNhanVien(CurrentIdNhanVien, keyword, statusId);

            if (isInitialLoad)
            {
                int totalProjectCount = ThanhVienDuAnManager.Instance.CountDuAnCuaNhanVien(CurrentIdNhanVien);
                ltrCountAllProj.Text = totalProjectCount.ToString();
            }

            if (projects != null && projects.Count > 0)
            {
                rptAllProjects.DataSource = projects;
                rptAllProjects.DataBind();
                emptyAll.Visible = false;
            }
            else
            {
                rptAllProjects.DataSource = null;
                rptAllProjects.DataBind();
                emptyAll.Visible = true;
            }

            upListDuAn.Update();
        }

        protected void rptAllProjects_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ViewProject") return;

            Guid idDuAn;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out idDuAn))
            {
                ShowNotify("Dự án không hợp lệ.", MSGType.Error);
                return;
            }

            LoadProjectDetail(idDuAn);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "OpenProjectDetailPanel", "openProjectDetailPanel();", true);
        }

        private void LoadProjectDetail(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty) return;

            NhanVienProjectDTO project = ThanhVienDuAnManager.Instance.GetChiTietDuAnCuaNhanVien(CurrentIdNhanVien, idDuAn);

            if (project == null)
            {
                ShowNotify("Không tìm thấy dự án.", MSGType.Warning);
                return;
            }

            ltrDetailTenDuAn.Text = project.TenDuAn ?? string.Empty;
            ltrDetailMaDuAn.Text = project.MaDuAn ?? string.Empty;
            ltrDetailVaiTro.Text = project.VaiTro ?? string.Empty;
            ltrDetailTrangThai.Text = GetDuAnStatusText(project.TrangThai);
            ltrDetailProjectTime.Text = FormatDateRange(project.ProjectStartDate, project.ProjectEndDate);
            ltrDetailContribution.Text = project.ContributionPercent.ToString("0.0");
            rptDetailPhases.DataSource = project.Phases;
            rptDetailPhases.DataBind();
            upProjectDetail.Update();
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
            upSearchTagDuAn.Update();
        }

        protected void btnSearchDuAn_Click(object sender, EventArgs e)
        {
            UpdateSearchTags();
            BindProjectList(false);
        }

        protected void ddlSearchTrangThaiDuAn_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateSearchTags();
            BindProjectList(false);
        }

        protected void searchTagBoxDuAn_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                if (tag.Key == ddlSearchTrangThaiDuAn.ClientID)
                {
                    ddlSearchTrangThaiDuAn.ClearSelection();
                    upnlSearchDuAn.Update();
                }
                else if (tag.Key == txtSearchDuAn.ClientID)
                {
                    txtSearchDuAn.Text = string.Empty;
                    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "ClearTxtSearchDuAn", string.Format("$('#{0}').val('');", txtSearchDuAn.ClientID), true);
                    upnlSearchDuAn.Update();
                }

                UpdateSearchTags();
                BindProjectList(false);
            }
            catch (Exception)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.ERROR_OCCURED) ?? "Có lỗi xảy ra.", MSGType.Error);
            }
        }

        protected string GetDuAnStatusText(object statusValue)
        {
            if (statusValue == null || statusValue == DBNull.Value) return string.Empty;
            string value = statusValue.ToString();
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            foreach (object item in ddlSearchTrangThaiDuAn.Items)
            {
                if (item == null) continue;
                Type itemType = item.GetType();
                var valueProperty = itemType.GetProperty("Value");
                var textProperty = itemType.GetProperty("Text");
                if (valueProperty == null || textProperty == null) continue;

                object itemValueObject = valueProperty.GetValue(item, null);
                string itemValue = itemValueObject != null ? itemValueObject.ToString() : string.Empty;
                if (!string.Equals(itemValue, value, StringComparison.OrdinalIgnoreCase)) continue;

                object itemTextObject = textProperty.GetValue(item, null);
                string itemText = itemTextObject != null ? itemTextObject.ToString() : string.Empty;
                if (!string.IsNullOrWhiteSpace(itemText))
                {
                    return itemText;
                }
                break;
            }
            return value;
        }

        private string CalculateSeniority(DateTime joinDate)
        {
            DateTime now = DateTime.Now;
            if (joinDate > now) return GetResourceText(BackEndResourceKeys.NOT_STARTED_WORKING_YET);

            int years = now.Year - joinDate.Year;
            int months = now.Month - joinDate.Month;
            if (now.Day < joinDate.Day) months--;
            if (months < 0)
            {
                years--;
                months += 12;
            }

            string yearStr = GetResourceText(BackEndResourceKeys.YEAR).ToLower();
            string monthStr = GetResourceText(BackEndResourceKeys.MONTH).ToLower();
            string dayStr = GetResourceText(BackEndResourceKeys.DAY).ToLower();

            if (years > 0 && months > 0) return string.Format("{0} {1}, {2} {3}", years, yearStr, months, monthStr);
            if (years > 0) return string.Format("{0} {1}", years, yearStr);
            if (months > 0) return string.Format("{0} {1}", months, monthStr);

            int days = (now - joinDate).Days;
            return string.Format("{0} {1}", days, dayStr);
        }

        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            if (CurrentIdNhanVien != Guid.Empty)
            {
                CtrlNhanVienPopup1.Edit(CurrentIdNhanVien);
            }
        }

        protected string GetProjectUrl(object idDuAn)
        {
            if (idDuAn == null) return "#";
            Guid projectId;
            if (!Guid.TryParse(idDuAn.ToString(), out projectId)) return "#";
            return GetRelativeClientPath(RewriteURLHelper.ProjectDetail(projectId));
        }

        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected string GetTaskPriorityBadge(object tenDoUuTien, object diemDoUuTien)
        {
            return _controlHelpers.GetTaskPriorityBadge(tenDoUuTien, diemDoUuTien);
        }

        protected string GetTaskStatusBadge(object status)
        {
            return _controlHelpers.GetTaskStatusBadge(status);
        }

        private bool IsTaskCompletedLate(object status, object plannedEndDate, object actualEndDate)
        {
            if (status == null || status == DBNull.Value)
            {
                return false;
            }

            if (!int.TryParse(status.ToString(), out int taskStatus))
            {
                return false;
            }

            // 2 = Hoàn thành
            if (taskStatus != 2)
            {
                return false;
            }

            if (plannedEndDate == null || plannedEndDate == DBNull.Value)
            {
                return false;
            }

            if (actualEndDate == null || actualEndDate == DBNull.Value)
            {
                return false;
            }

            if (!DateTime.TryParse(plannedEndDate.ToString(), out DateTime plannedEnd))
            {
                return false;
            }

            if (!DateTime.TryParse(actualEndDate.ToString(), out DateTime actualEnd))
            {
                return false;
            }

            if (plannedEnd.Year <= 1900 || actualEnd.Year <= 1900)
            {
                return false;
            }

            return actualEnd.Date > plannedEnd.Date;
        }

        protected string GetTaskStatusDisplay(object status, object plannedEndDate, object actualEndDate)
        {
            if (IsTaskCompletedLate(status, plannedEndDate, actualEndDate))
            {
                DateTime plannedEnd = Convert.ToDateTime(plannedEndDate);
                DateTime actualEnd = Convert.ToDateTime(actualEndDate);
                int lateDays = (actualEnd.Date - plannedEnd.Date).Days;

                return string.Format("Hoàn thành trễ ({0} ngày)", lateDays);
            }

            return GetTaskStatusBadge(status);
        }

        protected string GetTaskStatusWrapperClass(object status, object plannedEndDate, object actualEndDate)
        {
            if (IsTaskCompletedLate(status, plannedEndDate, actualEndDate))
            {
                return "ui-status-wrapper task-status-late";
            }

            return "ui-status-wrapper";
        }

        protected bool HasTasks(object tasksObj)
        {
            if (tasksObj == null || tasksObj == DBNull.Value) return false;
            var list = tasksObj as System.Collections.IEnumerable;
            if (list != null)
            {
                var enumerator = list.GetEnumerator();
                return enumerator.MoveNext();
            }
            return false;
        }

        protected string FormatDateSafe(object dateObj, string defaultEmptyText = "")
        {
            if (dateObj == null || dateObj == DBNull.Value || string.IsNullOrWhiteSpace(dateObj.ToString())) return defaultEmptyText;
            DateTime dt;
            if (DateTime.TryParse(dateObj.ToString(), out dt))
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
            if (start == "?" && end == defaultEndText) return string.Empty;
            return string.Format("{0} — {1}", start, end);
        }
        private string GenerateScheduleJson(Guid userId, DateTime start, DateTime end)
        {
            var lich = LichTrinhManager.Instance.LayLichTrinhNhanVien(userId, start, end);
            var dict = new Dictionary<string, object>();

            foreach (var ngay in lich)
            {
                int dow = (int)ngay.Ngay.DayOfWeek;
                string dayName = dow == 0 ? "CN" : dow == 6 ? "T7" : $"T{dow + 1}";

                string text;
                switch (ngay.TrangThaiLich)
                {
                    case "holiday":
                        text = "🎉 " + GetResourceText(BackEndResourceKeys.HOLIDAY);
                        break;
                    case "weekend":
                        text = "⬜ " + GetResourceText(BackEndResourceKeys.WEEKEND);
                        break;
                    case "busy":
                        text = $"🔴 {ngay.DanhSachCongViec.Count} {GetResourceText(BackEndResourceKeys.TASK).ToLower()}";
                        break;
                    default:
                        text = "🟢 " + GetResourceText(BackEndResourceKeys.FREE);
                        break;
                }

                dict.Add(ngay.Ngay.ToString("yyyy-MM-dd"), new { status = ngay.TrangThaiLich, dayName, text });
            }

            return JsonConvert.SerializeObject(dict);
        }
    }
}
    