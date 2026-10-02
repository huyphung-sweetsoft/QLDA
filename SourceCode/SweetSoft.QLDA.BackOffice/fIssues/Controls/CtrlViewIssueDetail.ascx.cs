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

namespace SweetSoft.QLDA.BackOffice.fIssues.Controls
{
    public partial class CtrlViewIssueDetail : BaseAdminUserControl
    {
        public void OpenModal(Guid issueId)
        {
            if (issueId == Guid.Empty)
                return;

            TblVanDe issue = TblVanDe.FetchByID(issueId);

            if (issue == null || issue.DaXoa == true)
                return;

            BindIssue(issue);
            upIssueView.Update();
            mdlIssueView.OpenModal(true);
        }

        private void BindIssue(TblVanDe issue)
        {
            lblIssueName.Text = HttpUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(issue.TenVanDe) ? "—" : issue.TenVanDe);

            lblImpact.Text = GetMucDoAnhHuongText(issue.MucDoAnhHuong);
            lblStatus.Text = GetTrangThaiVanDeText(issue.TrangThai);
            lblOrigin.Text = GetNguonGocVanDeText(issue.NguonGocVanDe);

            BindCreatedBy(issue.NguoiTao);
            lblCreatedDate.Text = issue.NgayTao.ToString("dd/MM/yyyy HH:mm");

            ltrAffectedTask.Text = FormatRelatedTask(issue.IdCongViecBiAnhHuong);
            ltrOriginTask.Text = FormatRelatedTask(issue.IdCongViecPhatSinh);
            ltrDescription.Text = ToDisplayHtml(issue.MoTaChiTiet);
            ltrPlan.Text = ToDisplayHtml(issue.KeHoachXuLy);

            BindAssignees(issue.IdVanDe);
        }

        private void BindCreatedBy(string userName)
        {
            UserDisplayInfo creator =
                UserDisplayInfoManager.Instance.GetUserDisplayInfo(userName);

            lblCreatedBy.Text =
                HttpUtility.HtmlEncode(creator.DisplayName);

            lblCreatedEmail.Text =
                HttpUtility.HtmlEncode(creator.Email);

            ltrCreatedAvatar.Text =
                creator.AvatarHtml;
        }

        private string FormatRelatedTask(Guid? taskId)
        {
            if (!taskId.HasValue || taskId.Value == Guid.Empty)
                return "—";

            TblCongViec task = TaskManager.Instance.FetchById(taskId.Value);

            if (task == null || task.DaXoa == true)
                return "—";

            string code = HttpUtility.HtmlEncode(task.MaCongViec ?? "");
            string name = HttpUtility.HtmlEncode(task.TenCongViec ?? "");

            if (string.IsNullOrWhiteSpace(code))
                return name;

            if (string.IsNullOrWhiteSpace(name))
                return code;

            return $"<strong>[{code}]</strong> {name}";
        }

        private void BindAssignees(Guid issueId)
        {
            List<TblVanDeNhanVien> assignments = new Select()
                .From(TblVanDeNhanVien.Schema)
                .Where(TblVanDeNhanVien.Columns.IdVanDe).IsEqualTo(issueId)
                .ExecuteTypedList<TblVanDeNhanVien>();

            List<Guid> assignedIds = assignments
                .Select(x => x.IdNhanVien)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            List<AspnetUser> assignedUsers = new List<AspnetUser>();

            foreach (Guid userId in assignedIds)
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId);

                if (user != null)
                    assignedUsers.Add(user);
            }

            assignedUsers = assignedUsers
                .OrderBy(x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : x.DisplayName)
                .ThenBy(x => x.UserName)
                .ToList();

            List<object> result = new List<object>();

            for (int i = 0; i < assignedUsers.Count; i++)
            {
                AspnetUser user = assignedUsers[i];

                string displayName = string.IsNullOrWhiteSpace(user.DisplayName)
                    ? user.UserName
                    : user.DisplayName;

                string email = GetUserEmail(user.UserName);

                result.Add(new
                {
                    DisplayName = displayName,
                    Email = string.IsNullOrWhiteSpace(email)
                        ? "Chưa cập nhật email"
                        : email,
                    AvatarHtml = GetAvatarHtml(displayName, user.Avatar, i)
                });
            }

            rptAssignees.DataSource = result;
            rptAssignees.DataBind();
            pnlNoAssignees.Visible = result.Count == 0;
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";

            try
            {
                MembershipUser user = Membership.GetUser(userName);
                return user != null ? user.Email : "";
            }
            catch
            {
                return "";
            }
        }

        private string GetAvatarHtml(string displayName, string avatar, int index)
        {
            string[] colors =
            {
                "#7c3aed",
                "#2563eb",
                "#059669",
                "#d97706",
                "#db2777"
            };

            string color = colors[index % colors.Length];

            string safeName =
                HttpUtility.HtmlAttributeEncode(displayName ?? "");

            string initials =
                HttpUtility.HtmlEncode(GetInitials(displayName));

            bool isDefaultAvatar =
                string.IsNullOrWhiteSpace(avatar) ||
                avatar.EndsWith(
                    "/Styles/images/user-icon.png",
                    StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl =
                    avatar.StartsWith("~", StringComparison.Ordinal)
                        ? Page.ResolveUrl(avatar)
                        : avatar;

                avatarUrl =
                    HttpUtility.HtmlAttributeEncode(avatarUrl);

                string fallbackHtml =
                    $"<div class='issue-person-avatar' style='background:{color};'>{initials}</div>";

                return
                    $"<img src='{avatarUrl}' " +
                    $"class='issue-person-avatar' " +
                    $"alt='{safeName}' " +
                    $"title='{safeName}' " +
                    $"onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }

            return
                $"<div class='issue-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] parts = fullName
                .Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();

            return (
                parts[parts.Length - 2].Substring(0, 1) +
                parts[parts.Length - 1].Substring(0, 1)
            ).ToUpper();
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<span class='text-muted'>—</span>";

            return HttpUtility.HtmlEncode(value)
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");
        }

        private string GetMucDoAnhHuongText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";

            MucDoAnhHuonEnum mucDo =
                (MucDoAnhHuonEnum)Convert.ToInt32(value);

            return HttpUtility.HtmlEncode(
                GetResourceText(
                    IssueManager.Instance.GetValueForMucDoAnhHuong(mucDo)));
        }

        private string GetTrangThaiVanDeText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";

            TrangThaiVanDeEnum status =
                (TrangThaiVanDeEnum)Convert.ToInt32(value);

            return HttpUtility.HtmlEncode(
                GetResourceText(
                    IssueManager.Instance.GetValueForTrangThaiVanDe(status)));
        }

        private string GetNguonGocVanDeText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";

            NguonGocVanDeEnum origin =
                (NguonGocVanDeEnum)Convert.ToInt32(value);

            return HttpUtility.HtmlEncode(
                GetResourceText(
                    IssueManager.Instance.GetValueForNguonGocVanDe(origin)));
        }
    }
}