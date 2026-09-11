using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;        // Chứa UserManager
using SweetSoft.QLDA.Core.ScheduleManager; // Đã chép đúng namespace theo folder
using SweetSoft.QLDA.Core.SysManager;      // Chứa SweetContext, ActionKeys
using SweetSoft.QLDA.DataAccess;           // Chứa AspnetUser
using System;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class LichCaNhan : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.LichBieu;

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

            if (!string.IsNullOrEmpty(rawQueryId))
            {
                // Giải mã chuỗi bảo mật về lại GUID trần
                string plainId = SecurityUtilities.UnprotectUrlParameter(rawQueryId);

                if (Guid.TryParse(plainId, out Guid queryId))
                {
                    if (queryId == loggedInUser)
                    {
                        // Tự xem lịch của chính mình -> Hợp lệ
                        TargetUserId = loggedInUser;
                        litTitle.Text = "Lịch cá nhân của tôi";
                    }
                    else
                    {
                        // Đang cố xem lịch người khác -> Kiểm tra quyền Admin
                        bool isAdmin = this.IsUserRight(ActionKeys.View, ModuleKeys.User);

                        if (!isAdmin)
                        {
                            // Không đủ quyền -> Đuổi ra ngoài trang lỗi 403
                            Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                            return;
                        }

                        TargetUserId = queryId;

                        // CHUẨN KIẾN TRÚC: Gọi Manager thay vì Query DB
                        AspnetUser targetUser = UserManager.Instance.GetUserById(queryId);
                        litTitle.Text = targetUser != null ? $"Lịch công việc của {targetUser.DisplayName}" : "Lịch công việc nhân sự";
                    }
                }
                else
                {
                    // Truyền ID nhưng giải mã/parse bị lỗi -> Trang không tồn tại (404)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                    return;
                }
            }
            else
            {
                // KHÔNG truyền Params -> Mặc định xem lịch chính mình
                TargetUserId = loggedInUser;
                litTitle.Text = "Lịch cá nhân của tôi";
            }
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

                litDateRange.Text = $"Tháng {refDate.Month}/{refDate.Year}";
                btnViewMonth.CssClass = "btn-cal active";
                btnViewWeek.CssClass = "btn-cal";
            }
            else // Chế độ "Tuần"
            {
                int diffStart = (int)refDate.DayOfWeek - (int)DayOfWeek.Monday;
                if (diffStart < 0) diffStart += 7;
                startDate = refDate.AddDays(-diffStart);
                endDate = startDate.AddDays(6);

                litDateRange.Text = $"Tuần từ {startDate:dd/MM} - {endDate:dd/MM/yyyy}";
                btnViewWeek.CssClass = "btn-cal active";
                btnViewMonth.CssClass = "btn-cal";
            }

            // Gọi Core Manager
            var data = LichTrinhManager.Instance.LayLichTrinhNhanVien(TargetUserId, startDate, endDate);

            // Ép thành JSON cho Javascript
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