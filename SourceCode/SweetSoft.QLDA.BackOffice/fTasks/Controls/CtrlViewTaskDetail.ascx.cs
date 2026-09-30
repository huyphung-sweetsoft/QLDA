using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlViewTaskDetail : BaseAdminUserControl
    {
        public void OpenModal(Guid taskId)
        {
            if (taskId == Guid.Empty) return;

            TblCongViec task = TblCongViec.FetchByID(taskId);
            if (task == null || task.DaXoa == true) return;

            BindTask(task);
            upTaskView.Update();
            mdlTaskView.OpenModal(true);
        }

        private void BindTask(TblCongViec task)
        {
            // 1. Gộp Mã và Tên công việc lên Tiêu đề
            string maCv = HttpUtility.HtmlEncode(task.MaCongViec ?? "");
            string tenCv = HttpUtility.HtmlEncode(task.TenCongViec ?? "");
            lblTaskName.Text = string.IsNullOrEmpty(maCv) ? tenCv : $"{maCv}. {tenCv}";

            // 2. Thuộc giai đoạn (Lấy tên từ bảng TblGiaiDoanDuAn)
            lblPhaseName.Text = "—";
            if (task.IdGiaiDoanDuAn.HasValue)
            {
                TblCongViec phase = TblCongViec.FetchByID(task.IdGiaiDoanDuAn.Value);
                if (phase != null && !string.IsNullOrWhiteSpace(phase.TenCongViec))
                {
                    lblPhaseName.Text = HttpUtility.HtmlEncode(phase.TenCongViec);
                }
            }

            lblStatus.Text = GetTaskStatusText(task.TrangThai);

            // Độ ưu tiên
            if (task.IdDoUuTien.HasValue)
            {
                TblDoUuTien priority = TblDoUuTien.FetchByID(task.IdDoUuTien.Value);
                lblPriority.Text = priority != null ? HttpUtility.HtmlEncode(priority.TenDoUuTien) : "—";
            }
            else lblPriority.Text = "—";

            // 3. Xếp ô vuông vức: Ngày bắt đầu - Thời hạn - Ngày kết thúc - Hoàn thành TT
            lblStartDate.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("dd/MM/yyyy") : "—";
            lblDuration.Text = task.ThoiHanNgay.HasValue ? $"{task.ThoiHanNgay.Value} ngày" : "—";
            lblEndDate.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "—";
            lblActualEndDate.Text = task.NgayHoanThanhThucTe.HasValue ? task.NgayHoanThanhThucTe.Value.ToString("dd/MM/yyyy HH:mm") : "—";

            // 4. Nếu có Lý do trễ thì mới hiện Box đỏ ra
            if (!string.IsNullOrWhiteSpace(task.LyDoTre))
            {
                divDelayReason.Visible = true;
                lblDelayReason.Text = HttpUtility.HtmlEncode(task.LyDoTre);
            }
            else
            {
                divDelayReason.Visible = false;
            }

            ltrDescription.Text = ToDisplayHtml(task.MoTa);

            BindAssignees(task.IdCongViec);
        }

        private void BindAssignees(Guid taskId)
        {
            List<TblCongViecNhanVien> assignments = new Select()
                .From(TblCongViecNhanVien.Schema)
                .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(taskId)
                .ExecuteTypedList<TblCongViecNhanVien>();

            List<Guid> assignedIds = assignments
                .Select(x => x.IdNhanVien)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            List<AspnetUser> assignedUsers = new List<AspnetUser>();
            foreach (Guid userId in assignedIds)
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId);
                if (user != null) assignedUsers.Add(user);
            }

            assignedUsers = assignedUsers
                .OrderBy(x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : x.DisplayName)
                .ToList();

            List<object> result = new List<object>();
            for (int i = 0; i < assignedUsers.Count; i++)
            {
                AspnetUser user = assignedUsers[i];
                string displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                string email = GetUserEmail(user.UserName);

                result.Add(new
                {
                    DisplayName = displayName,
                    Email = string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email,
                    AvatarHtml = GetAvatarHtml(displayName, user.Avatar, i)
                });
            }

            rptAssignees.DataSource = result;
            rptAssignees.DataBind();
            pnlNoAssignees.Visible = result.Count == 0;
        }

        private string GetTaskStatusText(object statusObj)
        {
            if (statusObj == null || statusObj == DBNull.Value) return "Chưa bắt đầu";
            int status = Convert.ToInt32(statusObj);
            if (status == 1) return "Đang làm";
            if (status == 2) return "Hoàn thành";
            if (status == 3) return "Hoàn thành (Trễ hạn)";
            return "Chưa bắt đầu";
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return "";
            try
            {
                MembershipUser user = Membership.GetUser(userName);
                return user != null ? user.Email : "";
            }
            catch { return ""; }
        }

        private string GetAvatarHtml(string displayName, string avatar, int index)
        {
            string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
            string color = colors[index % colors.Length];
            string safeName = HttpUtility.HtmlEncode(displayName ?? "");
            string initials = HttpUtility.HtmlEncode(GetInitials(displayName));
            bool isDefaultAvatar = string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~", StringComparison.Ordinal) ? Page.ResolveUrl(avatar) : avatar;
                avatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                string fallbackHtml = $"<div class='task-person-avatar' style='background:{color};'>{initials}</div>";
                return $"<img src='{avatarUrl}' class='task-person-avatar' alt='{safeName}' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }
            return $"<div class='task-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "<span class='text-muted'>—</span>";
            return HttpUtility.HtmlEncode(value).Replace("\r\n", "<br />").Replace("\n", "<br />");
        }
    }
}