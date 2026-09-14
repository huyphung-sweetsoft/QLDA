using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlChonNhanVienTask : BaseAdminUserControl
    {
        public event EventHandler OnAssignConfirmed;

        private Guid IdDuAn
        {
            get => ViewState["IdDuAn"] != null ? (Guid)ViewState["IdDuAn"] : Guid.Empty;
            set => ViewState["IdDuAn"] = value;
        }

        private Guid IdCongViec
        {
            get => ViewState["IdCongViec"] != null ? (Guid)ViewState["IdCongViec"] : Guid.Empty;
            set => ViewState["IdCongViec"] = value;
        }

        private DateTime StartDate
        {
            get => ViewState["StartDate"] != null ? (DateTime)ViewState["StartDate"] : DateTime.Today;
            set => ViewState["StartDate"] = value;
        }

        private DateTime EndDate
        {
            get => ViewState["EndDate"] != null ? (DateTime)ViewState["EndDate"] : DateTime.Today;
            set => ViewState["EndDate"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        // Bổ sung tham số taskName vào hàm
        // Bổ sung tham số idNhanVienQuanLy (Mặc định là null để an toàn)
        public void OpenPicker(Guid idDuAn, Guid idCongViec, DateTime startDate, DateTime endDate, string taskName, Guid? idNhanVienQuanLy = null)
        {
            this.IdDuAn = idDuAn;
            this.IdCongViec = idCongViec;
            this.StartDate = startDate;
            this.EndDate = endDate;

            List<Guid> nhomA = ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(idDuAn);
            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien();

            // Lấy danh sách đã gán
            List<Guid> dangGan = TaskManager.Instance.GetAssignedNhanVienIds(idCongViec);

            // FIX TỬ HUYỆT LIFECYCLE: Bắt buộc gán ViewState trước khi gọi DataBind()
            ViewState["DangGanIds"] = dangGan;

            // ÁP DỤNG SORT ĐA TẦNG CHO NHÓM DỰ ÁN (PM -> Người đã gán -> ABC)
            List<AspnetUser> projectMembers = allUsers
                .Where(u => nhomA.Contains(u.UserId))
                .OrderByDescending(u => idNhanVienQuanLy.HasValue && u.UserId == idNhanVienQuanLy.Value)
                .ThenByDescending(u => dangGan.Contains(u.UserId))
                .ThenBy(u => u.DisplayName)
                .ToList();

            // ÁP DỤNG SORT CHO NHÓM CÔNG TY (Người đã gán -> ABC)
            List<AspnetUser> otherMembers = allUsers
                .Where(u => !nhomA.Contains(u.UserId))
                .OrderByDescending(u => dangGan.Contains(u.UserId))
                .ThenBy(u => u.DisplayName)
                .ToList();

            // Đổ dữ liệu
            rptProjectMembers.DataSource = BuildDisplayList(projectMembers, startDate, endDate, idNhanVienQuanLy);
            rptProjectMembers.DataBind();
            ltrCountProj.Text = projectMembers.Count.ToString();

            rptCompanyMembers.DataSource = BuildDisplayList(otherMembers, startDate, endDate, idNhanVienQuanLy);
            rptCompanyMembers.DataBind();
            ltrCountCompany.Text = otherMembers.Count.ToString();

            // Cấu hình Title và Thông báo bằng ResourceKey
            mdlTaskMemberPicker.Title = GetResourceText(BackEndResourceKeys.ASSIGN_TASK);
            string thoiGian = $"<strong>{startDate:dd/MM/yyyy}</strong> - <strong>{endDate:dd/MM/yyyy}</strong>";
            ltrTaskInfoNote.Text = $"<div style='margin-bottom: 5px; font-size: 13px;'><i class='fas fa-tasks me-1'></i> {GetResourceText(BackEndResourceKeys.TASK)}: <strong style='color: #b91c1c;'>{taskName}</strong></div>" +
                                   $"<div style='font-size: 12px;'><i class='far fa-clock me-1'></i> {GetResourceText(BackEndResourceKeys.EXECUTION_TIME)}: {thoiGian}</div>";

            mdlTaskMemberPicker.OpenModal(true);
        }

        // Bổ sung tham số pmId để đánh dấu huy hiệu
        private List<object> BuildDisplayList(List<AspnetUser> users, DateTime start, DateTime end, Guid? pmId)
        {
            var list = new List<object>();
            foreach (var user in users)
            {
                list.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    IsPM = pmId.HasValue && user.UserId == pmId.Value, // Đánh dấu true nếu khớp ID của PM
                    ScheduleJson = GenerateScheduleJson(user.UserId, start, end)
                });
            }
            return list;
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
                        text = "🎉 " + (!string.IsNullOrEmpty(ngay.TenNgoaiLe) ? ngay.TenNgoaiLe : GetResourceText(BackEndResourceKeys.HOLIDAY));
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

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            HiddenField hdfUserId = (HiddenField)e.Item.FindControl("hdfUserId");
            CheckBox chkSelect = (CheckBox)e.Item.FindControl("chkSelect");
            if (hdfUserId == null || chkSelect == null) return;

            List<Guid> dangGan = ViewState["DangGanIds"] as List<Guid>;
            if (dangGan == null) return;

            if (Guid.TryParse(hdfUserId.Value, out Guid currentUserId) && dangGan.Contains(currentUserId))
            {
                chkSelect.Checked = true;
            }
        }

        protected void btnConfirmTaskAssign_Click(object sender, EventArgs e)
        {
            List<Guid> selectedIds = new List<Guid>();
            selectedIds.AddRange(GetSelectedIdsFromRepeater(rptProjectMembers));
            selectedIds.AddRange(GetSelectedIdsFromRepeater(rptCompanyMembers));

            TaskManager.Instance.UpdateAssignments(this.IdDuAn, this.IdCongViec, selectedIds.Distinct().ToList());

            mdlTaskMemberPicker.CloseModal();

            OnAssignConfirmed?.Invoke(this, EventArgs.Empty);
        }

        private List<Guid> GetSelectedIdsFromRepeater(Repeater rpt)
        {
            var result = new List<Guid>();
            foreach (RepeaterItem item in rpt.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem)
                    continue;

                CheckBox chkSelect = (CheckBox)item.FindControl("chkSelect");
                HiddenField hdfUserId = (HiddenField)item.FindControl("hdfUserId");

                if (chkSelect != null && hdfUserId != null && chkSelect.Checked
                    && Guid.TryParse(hdfUserId.Value, out Guid id))
                {
                    result.Add(id);
                }
            }
            return result;
        }
        
    }
}