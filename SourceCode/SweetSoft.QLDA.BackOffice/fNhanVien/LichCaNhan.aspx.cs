using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;        // Chứa UserManager
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager; // Đã chép đúng namespace theo folder
using SweetSoft.QLDA.Core.SysManager;      // Chứa SweetContext, ActionKeys
using SweetSoft.QLDA.DataAccess;           // Chứa AspnetUser
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
                InitSecurityAndTargetUser();
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
                            Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                            return;
                        }

                        AspnetUser targetUser = UserManager.Instance.GetUserById(queryId);
                        litTitle.Text = targetUser != null ?
                            string.Format(GetResourceText(BackEndResourceKeys.SCHEDULE_OF_USER), targetUser.DisplayName) :
                            GetResourceText(BackEndResourceKeys.EMPLOYEE_SCHEDULE);
                    }

                    // [QUAN TRỌNG]: Đã đi qua trang Chi tiết thì Lùi 1 bước PHẢI LÀ trang Chi tiết
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
                // Cấp 1: Nguồn gốc (Hồ sơ hoặc Danh sách)
                if (isFromProfile)
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.Profile), GetResourceText(BackEndResourceKeys.PROFILE));
                }
                else
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.NhanVien), GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST));
                }

                // Cấp 2: Lùi 1 bước về trang Chi tiết (kèm theo cờ để trang chi tiết biết đường ẩn nút Sửa)
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
                // THÁNG: Tính toán cho Grid 6 hàng x 7 cột (Đủ 42 ô để không bị vỡ Layout)
                DateTime firstDayOfMonth = new DateTime(refDate.Year, refDate.Month, 1);
                int diffStart = (int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffStart < 0) diffStart += 7; // Nếu ngày 1 là Chủ nhật (0), lùi về T2
                startDate = firstDayOfMonth.AddDays(-diffStart);
                // Luôn lấy đủ 42 ngày (6 tuần) để lưới Lịch Tháng luôn vuông vắn, không trồi sụt
                endDate = startDate.AddDays(41);
                litDateRange.Text = $"{GetResourceText(BackEndResourceKeys.MONTH)} {refDate.Month}/{refDate.Year}";
                btnViewMonth.CssClass = "btn-cal active";
                btnViewWeek.CssClass = "btn-cal";
            }
            else // Chế độ "Tuần"
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
    }
}