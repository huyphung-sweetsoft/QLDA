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

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlXemNhanVienMeet : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e) { }

        public void OpenModal(Guid idCuocHop, DateTime startDate, DateTime endDate, string tenCuocHop, Guid? idNguoiChuTri = null)
        {
            List<Guid> assignedIds = MeetManager.Instance.GetNhanVienCuocHop(idCuocHop);

            List<AspnetUser> allUsers = UserManager.Instance.GetAllActiveNhanVien();

            List<AspnetUser> assignedUsers = allUsers
                .Where(u => assignedIds.Contains(u.UserId))
                .OrderByDescending(u => idNguoiChuTri.HasValue && u.UserId == idNguoiChuTri.Value)
                .ThenBy(u => u.DisplayName)
                .ToList();

            if (assignedUsers.Count > 0)
            {
                rptAssignedMembers.DataSource = BuildDisplayList(assignedUsers, startDate, endDate, idNguoiChuTri);
                rptAssignedMembers.DataBind();
                rptAssignedMembers.Visible = true;
                divEmpty.Visible = false;
            }
            else
            {
                rptAssignedMembers.Visible = false;
                divEmpty.Visible = true;
            }

            mdlViewMeetMember.Title = "Chi tiết thành viên tham gia";
            string thoiGian = $"<strong>{startDate:dd/MM/yyyy}</strong> - <strong>{endDate:dd/MM/yyyy}</strong>";
            ltrMeetInfoNote.Text = $"<div style='margin-bottom: 5px; font-size: 13px;'><i class='fas fa-handshake me-1'></i> Cuộc họp: <strong style='color: #b91c1c;'>{tenCuocHop}</strong></div>" +
                                   $"<div style='font-size: 12px;'><i class='far fa-clock me-1'></i> Thời gian diễn ra: {thoiGian}</div>";

            mdlViewMeetMember.OpenModal(true);
        }

        private List<object> BuildDisplayList(List<AspnetUser> users, DateTime start, DateTime end, Guid? hostId)
        {
            var list = new List<object>();
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                string avatarPath = user.Avatar;

                list.Add(new
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    IsHost = hostId.HasValue && user.UserId == hostId.Value,
                    AvatarHtml = GetSingleAvatarHtml(user.DisplayName, avatarPath, i),
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
                    case "holiday": text = "🎉 " + (!string.IsNullOrEmpty(ngay.TenNgoaiLe) ? ngay.TenNgoaiLe : GetResourceText(BackEndResourceKeys.HOLIDAY)); break;
                    case "weekend": text = "⬜ " + GetResourceText(BackEndResourceKeys.WEEKEND); break;
                    case "busy": text = $"🔴 {ngay.DanhSachCongViec.Count} {GetResourceText(BackEndResourceKeys.TASK).ToLower()}"; break;
                    default: text = "🟢 " + GetResourceText(BackEndResourceKeys.FREE); break;
                }
                dict.Add(ngay.Ngay.ToString("yyyy-MM-dd"), new { status = ngay.TrangThaiLich, dayName, text });
            }
            return JsonConvert.SerializeObject(dict);
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetSingleAvatarHtml(string name, string avatar, int index)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            string color = colors[index % colors.Length];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string fallbackHtml = $"<div class=\\'single-avatar-circle\\' style=\\'background-color: {color};\\'>{GetInitials(name)}</div>";
                return $"<img src='{avatarUrl}' class='single-avatar-circle' style='object-fit: cover;' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
            }
            else
            {
                return $"<div class='single-avatar-circle' style='background-color: {color};'>{GetInitials(name)}</div>";
            }
        }
    }
}