using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class LichCaNhan : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.LichBieu;
        public override bool IsLogin => true;

        // Biến lưu ID người đang bị soi lịch
        private Guid TargetUserId
        {
            get => ViewState["TargetUserId"] != null ? (Guid)ViewState["TargetUserId"] : Guid.Empty;
            set => ViewState["TargetUserId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }
                InitSecurityAndTargetUser();

                // Mặc định load ngày hôm nay, chế độ "Tháng"
                hfCurrentDate.Value = DateTime.Today.ToString("yyyy-MM-dd");
                hfViewMode.Value = "month";

                LoadCalendarData();
            }
        }

        #region 1. Xử lý Bảo mật & Phân quyền
        private void InitSecurityAndTargetUser()
        {
            Guid loggedInUser = SweetContext.Current.UserId;
            string rawQueryId = Request.QueryString["Id"];
            bool isFromProfile = Request.QueryString["from"] == "profile";
            bool hasParentDetail = !string.IsNullOrEmpty(rawQueryId); // Cờ kiểm tra xem có đi qua trang Chi tiết không

            string rollbackUrl = "";

            if (hasParentDetail)
            {
                // Giải mã chuỗi bảo mật về lại GUID trần
                string plainId = SecurityUtilities.UnprotectUrlParameter(rawQueryId);

                if (Guid.TryParse(plainId, out Guid queryId))
                {
                    TargetUserId = queryId;

                    if (queryId == loggedInUser)
                    {
                        litTitle.Text = GetResourceText(BackEndResourceKeys.MY_PERSONAL_SCHEDULE);
                    }
                    else
                    {
                        bool hasViewRight = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien);
                        if (!hasViewRight)
                        {
                            // Không đủ quyền -> Đuổi ra ngoài trang lỗi 403
                            Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                            return;
                        }

                        TargetUserId = queryId;

                        AspnetUser targetUser = UserManager.Instance.GetUserById(queryId);
                        litTitle.Text = targetUser != null ?
                            string.Format(GetResourceText(BackEndResourceKeys.SCHEDULE_OF_USER), targetUser.DisplayName) :
                            GetResourceText(BackEndResourceKeys.EMPLOYEE_SCHEDULE);
                    }

                    // Đã đi qua trang Chi tiết thì Lùi 1 bước PHẢI LÀ trang Chi tiết
                    rollbackUrl = RewriteURLHelper.ViewDetailEmp(queryId);
                }
                else
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                    return;
                }
            }
            else
            {
                // Vào thẳng từ Menu -> Tự xem lịch mình, không có trang cha
                TargetUserId = loggedInUser;
                litTitle.Text = GetResourceText(BackEndResourceKeys.MY_PERSONAL_SCHEDULE);
            }

            // --- BUILD BREADCRUMB ---
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE);
            var navLinks = new Dictionary<string, string>();

            if (hasParentDetail)
            {
                if (isFromProfile)
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.Profile), GetResourceText(BackEndResourceKeys.PROFILE));
                }
                else
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.NhanVien), GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST));
                }

                if (isFromProfile && !rollbackUrl.Contains("from=profile"))
                {
                    rollbackUrl += (rollbackUrl.Contains("?") ? "&" : "?") + "from=profile";
                }
                navLinks.Add(GetRelativeClientPath(rollbackUrl), GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));
            }

            Navigation1.keyValuePairUrls = navLinks;
        }
        #endregion

        #region 2. Động cơ tính toán Ngày & Render
        private void LoadCalendarData()
        {
            if (!DateTime.TryParseExact(hfCurrentDate.Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime refDate))
            {
                refDate = DateTime.Today;
            }
            DateTime startDate, endDate;

            if (hfViewMode.Value == "month")
            {
                DateTime firstDayOfMonth = new DateTime(refDate.Year, refDate.Month, 1);
                int diffStart = (int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffStart < 0) diffStart += 7;
                startDate = firstDayOfMonth.AddDays(-diffStart);

                endDate = startDate.AddDays(41);
                litDateRange.Text = $"{GetResourceText(BackEndResourceKeys.MONTH)} {refDate.Month}/{refDate.Year}";
                btnViewMonth.CssClass = "btn-cal active";
                btnViewWeek.CssClass = "btn-cal";
            }
            else
            {
                int diffStart = (int)refDate.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffStart < 0) diffStart += 7;
                startDate = refDate.AddDays(-diffStart);
                endDate = startDate.AddDays(6);
                string tuKhoaWeek = GetResourceText(BackEndResourceKeys.WEEK);
                string tuKhoaFrom = GetResourceText(BackEndResourceKeys.FROM);
                litDateRange.Text = $"{tuKhoaWeek} {tuKhoaFrom} {startDate:dd/MM} - {endDate:dd/MM/yyyy}";
                btnViewWeek.CssClass = "btn-cal active";
                btnViewMonth.CssClass = "btn-cal";
            }

            var data = LichTrinhManager.Instance.LayLichTrinhNhanVien(TargetUserId, startDate, endDate);
            hfScheduleDataJSON.Value = JsonConvert.SerializeObject(data);
        }
        #endregion

        #region 3. Sự kiện Nút bấm (Postback ngầm qua UpdatePanel)
        protected void btnPrev_Click(object sender, EventArgs e)
        {
            DateTime curr = DateTime.Parse(hfCurrentDate.Value);
            hfCurrentDate.Value = hfViewMode.Value == "month" ? curr.AddMonths(-1).ToString("yyyy-MM-dd") : curr.AddDays(-7).ToString("yyyy-MM-dd");
            LoadCalendarData();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            DateTime curr = DateTime.Parse(hfCurrentDate.Value);
            hfCurrentDate.Value = hfViewMode.Value == "month" ? curr.AddMonths(1).ToString("yyyy-MM-dd") : curr.AddDays(7).ToString("yyyy-MM-dd");
            LoadCalendarData();
        }

        protected void btnToday_Click(object sender, EventArgs e)
        {
            hfCurrentDate.Value = DateTime.Today.ToString("yyyy-MM-dd");
            LoadCalendarData();
        }

        protected void btnViewMonth_Click(object sender, EventArgs e)
        {
            hfViewMode.Value = "month";
            LoadCalendarData();
        }

        protected void btnViewWeek_Click(object sender, EventArgs e)
        {
            hfViewMode.Value = "week";
            LoadCalendarData();
        }
        #endregion

        #region 4. Xử lý mở Modal Chi tiết bằng C# (Chuẩn ExtraModal)
        protected void btnOpenModalDay_Click(object sender, EventArgs e)
        {
            // 1. Đọc index từ JS gửi lên
            if (!int.TryParse(hfSelectedDateIndex.Value, out int dataIndex)) return;

            // 2. Phục hồi cục data JSON đang chứa danh sách lịch
            string jsonStr = hfScheduleDataJSON.Value;
            if (string.IsNullOrEmpty(jsonStr)) return;

            var scheduleList = JsonConvert.DeserializeObject<List<ThongTinLichNgay>>(jsonStr);

            if (dataIndex < 0 || dataIndex >= scheduleList.Count) return;
            var dayData = scheduleList[dataIndex];

            // 3. Set Tiêu đề cho Modal
            string dateStr = dayData.Ngay.ToString("dd/MM/yyyy");
            mdlDayDetail.Title = $"{GetResourceText(BackEndResourceKeys.SCHEDULE_DETAILS)} - {dateStr}";

            // 4. Render nội dung HTML
            string htmlContent = "";

            if (dayData.TrangThaiLich == "holiday")
            {
                string holidayTitle = GetResourceText(BackEndResourceKeys.HOLIDAY) ?? "NGÀY NGHỈ LỄ";
                htmlContent = $@"
                    <div style='background-color: #fef3c7; color: #b45309; padding: 20px; border-radius: 8px; border-left: 5px solid #f59e0b; text-align: center; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
                        <h4 style='margin: 0 0 10px 0; font-weight: 800; text-transform: uppercase;'>🎈 {holidayTitle}</h4>
                        <p style='margin: 0; font-size: 16px; font-weight: 600;'>{dayData.TenNgoaiLe}</p>
                    </div>";
            }
            else if (dayData.TrangThaiLich == "busy" && dayData.DanhSachCongViec != null)
            {
                // Gom nhóm Task theo Dự án
                var projGroups = new Dictionary<string, List<TomTatCongViec>>();
                string otherProjectText = GetResourceText(BackEndResourceKeys.OTHER_PROJECT_UNIDENTIFIED) ?? "Dự án khác";

                foreach (var task in dayData.DanhSachCongViec)
                {
                    string pName = string.IsNullOrEmpty(task.TenDuAn) ? otherProjectText : task.TenDuAn;
                    if (!projGroups.ContainsKey(pName))
                        projGroups[pName] = new List<TomTatCongViec>();

                    projGroups[pName].Add(task);
                }

                // Render HTML cho danh sách công việc
                foreach (var proj in projGroups)
                {
                    htmlContent += $@"
                        <div style='background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); overflow: hidden;'>
                            <div style='background: linear-gradient(to right, #eff6ff, #ffffff); border-bottom: 1px solid #e2e8f0; padding: 10px 15px; color: #1e3a8a; font-weight: 800; font-size: 14px;'>
                                <i class='fas fa-folder-open me-2'></i> {proj.Key}
                            </div>
                            <div style='padding: 10px 15px;'>";

                    foreach (var task in proj.Value)
                    {
                        htmlContent += $@"
                                <div style='padding: 8px 0; border-bottom: 1px dashed #cbd5e1; font-size: 13px; color: #334155; display: flex; align-items: flex-start; gap: 8px;'>
                                    <span style='background: #e0f2fe; color: #0284c7; padding: 2px 6px; border-radius: 4px; font-weight: 700; font-size: 11px; white-space: nowrap;'>
                                        {task.MaCongViec}
                                    </span>
                                    <span style='font-weight: 600; line-height: 1.4;'>{task.TenCongViec}</span>
                                </div>";
                    }
                    htmlContent += "</div></div>";
                }
            }

            // Dán HTML vào Literal
            litModalContent.Text = htmlContent;

            // 5. Cập nhật giao diện và gọi lệnh Mở Modal chuẩn
            upModal.Update();
            mdlDayDetail.OpenModal(true);
        }
        #endregion
    }
}