using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlChonNhanVien : BaseAdminUserControl
    {
        //Bốc 2 cái datetime này từ ngày bắt đấu và ngày kết thúc
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid> SelectedUserIds { get; set; }//Danh sách id nhân viên 
        public Guid? IdNhanVienQuanLy { get; set; }

        public delegate void ConfirmSelectionHandler(List<Guid> selectedIds);
        public event ConfirmSelectionHandler OnConfirmSelection;

        protected void Page_Load(object sender, EventArgs e)//Để trống pageload, sau này có thêm RegisterAsyncButton thì quăng dô, đại khái nó load chung vs page cha, mà lúc page cha load thì chưa có data, gọi bind data sẽ gây lỗi
        {
        }

        public void OpenPicker()//gọi từ trang cha để mở pop up
        {
            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                // Dùng ResourceKey cấu hình cảnh báo
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_START_AND_END_DATE), MSGType.Error);
                return;
            }

            if (SelectedUserIds == null)
            {
                SelectedUserIds = new List<Guid>();//tạo list rỗng để tránh lỗi 
            }

            // Tiêu đề form
            mdlMemberPicker.Title = GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE);

            // Format InfoNote: "Hiển thị lịch từ {0} đến {1}"
            ltrInfoNote.Text = string.Format(GetResourceText(BackEndResourceKeys.SCHEDULE_INFO_FORMAT),
                StartDate.Value.ToString("dd/MM/yyyy"),
                EndDate.Value.ToString("dd/MM/yyyy"));

            BindData();
            mdlMemberPicker.OpenModal(true);
        }

        private void BindData()
        {
            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien();
            // MỚI: loại PM ra khỏi danh sách chọn
            if (IdNhanVienQuanLy.HasValue && IdNhanVienQuanLy.Value != Guid.Empty)
            {
                allUsers = allUsers.Where(u => u.UserId != IdNhanVienQuanLy.Value).ToList();
            }

            var listMembers = new List<object>();

            foreach (var user in allUsers)
            {
                listMembers.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    ScheduleJson = GenerateScheduleJson(user.UserId, StartDate.Value, EndDate.Value)
                });
            }

            rptCompanyMembers.DataSource = listMembers;
            rptCompanyMembers.DataBind();

            ltrCountCompany.Text = listMembers.Count.ToString();
        }

        private string GenerateScheduleJson(Guid userId, DateTime start, DateTime end)
        {
            var scheduleDict = new Dictionary<string, object>();
            DateTime currDate = start.Date;

            while (currDate <= end.Date)
            {
                string dateKey = currDate.ToString("yyyy-MM-dd");
                int dayOfWeek = (int)currDate.DayOfWeek;
                string dayName = dayOfWeek == 0 ? "CN" : dayOfWeek == 6 ? "T7" : $"T{dayOfWeek + 1}";

                string status = "free";
                // Lấy chữ "Trống" từ cấu hình
                string text = "🟢 " + GetResourceText(BackEndResourceKeys.FREE);

                bool isWorkingDay = LichBieuChungManager.Instance.CheckIsWorkingDay(currDate);

                if (!isWorkingDay)
                {
                    if (dayOfWeek == 0 || dayOfWeek == 6)
                    {
                        status = "weekend";
                        text = "⬜ " + GetResourceText(BackEndResourceKeys.WEEKEND);
                    }
                    else
                    {
                        status = "holiday";
                        text = "🎉 " + GetResourceText(BackEndResourceKeys.HOLIDAY);
                    }
                }

                scheduleDict.Add(dateKey, new { status = status, dayName = dayName, text = text });
                currDate = currDate.AddDays(1);
            }

            return JsonConvert.SerializeObject(scheduleDict);
        }

        protected void rptCompanyMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HiddenField hdfUserId = (HiddenField)e.Item.FindControl("hdfUserId");
                CheckBox chkSelect = (CheckBox)e.Item.FindControl("chkSelect");

                if (hdfUserId != null && chkSelect != null)
                {
                    Guid currentUserId = Guid.Parse(hdfUserId.Value);

                    if (SelectedUserIds != null && SelectedUserIds.Contains(currentUserId))
                    {
                        chkSelect.Checked = true;
                    }
                }
            }
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            List<Guid> tempSelectedIds = new List<Guid>();

            foreach (RepeaterItem item in rptCompanyMembers.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    CheckBox chkSelect = (CheckBox)item.FindControl("chkSelect");
                    HiddenField hdfUserId = (HiddenField)item.FindControl("hdfUserId");

                    if (chkSelect != null && hdfUserId != null && chkSelect.Checked)
                    {
                        tempSelectedIds.Add(Guid.Parse(hdfUserId.Value));
                    }
                }
            }

            mdlMemberPicker.CloseModal();

            if (OnConfirmSelection != null)
            {
                OnConfirmSelection(tempSelectedIds);
            }
        }
    }
}