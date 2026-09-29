using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlViewMeetDetail : BaseAdminUserControl
    {
        public void OpenModal(Guid meetId)
        {
            if (meetId == Guid.Empty)
                return;

            TblLichHop meet = TblLichHop.FetchByID(meetId);
            if (meet == null || meet.DaXoa == true)
                return;

            BindMeeting(meet);
            upMeetView.Update();
            mdlMeetView.OpenModal(true);
        }

        private void BindMeeting(TblLichHop meet)
        {
            lblMeetName.Text = HtmlEncodeValue(meet.TenCuocHop);
            lblMeetRoom.Text = HtmlEncodeValue(meet.DiaDiemHop);
            lblMeetStart.Text = FormatDateTime(meet.ThoiGianBatDau);
            lblMeetEnd.Text = FormatDateTime(meet.ThoiGianKetThuc);
            lblMeetStatus.Text = GetStatusHtml(meet.TrangThai);
            lblMeetCreatedDate.Text = FormatDateTime(meet.NgayTao);
            ltrMeetContent.Text = ToDisplayHtml(meet.NoiDungCuocHop);

            BindCreator(meet.IdNguoiTao);
            BindParticipants(meet.IdLichHop);
        }

        private void BindCreator(Guid? creatorId)
        {
            string displayName = "—";
            string email = "Chưa cập nhật email";
            string avatar = "";

            if (creatorId.HasValue && creatorId.Value != Guid.Empty)
            {
                try
                {
                    AspnetUser user = UserManager.Instance.GetUserById(creatorId.Value);
                    if (user != null)
                    {
                        displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                        avatar = user.Avatar;
                        email = GetUserEmail(creatorId.Value);

                        if (string.IsNullOrWhiteSpace(email))
                            email = "Chưa cập nhật email";
                    }
                }
                catch
                {
                    // Keep fallback values when creator information is unavailable.
                }
            }

            lblMeetCreator.Text = HttpUtility.HtmlEncode(displayName);
            lblMeetCreatorEmail.Text = HttpUtility.HtmlEncode(email);
            ltrMeetCreatorAvatar.Text = GetAvatarHtml(displayName, avatar, 0);
        }

        private void BindParticipants(Guid meetId)
        {
            List<object> result = new List<object>();

            hdfMeetParticipantIds.Value = string.Empty;
            txtMeetParticipantNames.Text = string.Empty;

            try
            {
                new ControlHelpers().BindNhanVienThamGiaLichHop(
                    meetId,
                    hdfMeetParticipantIds,
                    txtMeetParticipantNames);

                HashSet<Guid> participantIds = ParseGuidList(hdfMeetParticipantIds.Value);
                List<KeyValuePair<Guid, AspnetUser>> users = new List<KeyValuePair<Guid, AspnetUser>>();

                foreach (Guid participantId in participantIds)
                {
                    try
                    {
                        AspnetUser user = UserManager.Instance.GetUserById(participantId);
                        if (user != null)
                            users.Add(new KeyValuePair<Guid, AspnetUser>(participantId, user));
                    }
                    catch
                    {
                        // Skip users that cannot be loaded.
                    }
                }

                users.Sort(delegate (KeyValuePair<Guid, AspnetUser> left, KeyValuePair<Guid, AspnetUser> right)
                {
                    string leftName = string.IsNullOrWhiteSpace(left.Value.DisplayName) ? left.Value.UserName : left.Value.DisplayName;
                    string rightName = string.IsNullOrWhiteSpace(right.Value.DisplayName) ? right.Value.UserName : right.Value.DisplayName;
                    return string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);
                });

                for (int i = 0; i < users.Count; i++)
                {
                    Guid participantId = users[i].Key;
                    AspnetUser user = users[i].Value;
                    string displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                    string email = GetUserEmail(participantId);

                    result.Add(new
                    {
                        DisplayName = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(displayName) ? "—" : displayName),
                        Email = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email),
                        AvatarHtml = GetAvatarHtml(displayName, user.Avatar, i, "meet-view-participant-avatar")
                    });
                }
            }
            catch
            {
                // Keep the empty state when participant information is unavailable.
            }

            rptMeetParticipants.DataSource = result;
            rptMeetParticipants.DataBind();
            pnlNoMeetParticipants.Visible = result.Count == 0;
        }

        private HashSet<Guid> ParseGuidList(string value)
        {
            HashSet<Guid> result = new HashSet<Guid>();

            if (string.IsNullOrWhiteSpace(value))
                return result;

            string[] rawIds = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rawIds.Length; i++)
            {
                Guid id;
                if (Guid.TryParse(rawIds[i].Trim(), out id) && id != Guid.Empty)
                    result.Add(id);
            }

            return result;
        }

        private string GetUserEmail(Guid userId)
        {
            if (userId == Guid.Empty)
                return "";

            try
            {
                MembershipUser user = Membership.GetUser(userId);
                return user != null ? user.Email : "";
            }
            catch
            {
                return "";
            }
        }

        private string GetAvatarHtml(string displayName, string avatar, int index, string cssClass = "meet-view-person-avatar")
        {
            string[] colors = { "#2563eb", "#7c3aed", "#059669", "#d97706", "#db2777" };
            string color = colors[index % colors.Length];
            string safeName = HttpUtility.HtmlAttributeEncode(displayName ?? "");
            string initials = HttpUtility.HtmlEncode(GetInitials(displayName));
            bool isDefaultAvatar = string.IsNullOrWhiteSpace(avatar)
                || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~", StringComparison.Ordinal)
                    ? Page.ResolveUrl(avatar)
                    : avatar;

                avatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                string fallbackHtml = string.Format(
                    "<div class='{0}' style='background:{1};' title='{2}'>{3}</div>",
                    cssClass,
                    color,
                    safeName,
                    initials);

                return string.Format(
                    "<img src='{0}' class='{1}' alt='{2}' title='{2}' onerror=\"this.onerror=null;this.outerHTML='{3}';\" />",
                    avatarUrl,
                    cssClass,
                    safeName,
                    HttpUtility.JavaScriptStringEncode(fallbackHtml));
            }

            return string.Format(
                "<div class='{0}' style='background:{1};' title='{2}'>{3}</div>",
                cssClass,
                color,
                safeName,
                initials);
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();

            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetStatusHtml(object value)
        {
            if (value == null || value == DBNull.Value)
                return "<span class='meet-view-status'>—</span>";

            int status = Convert.ToInt32(value);
            string text;

            try
            {
                TrangThaiCuocHopEnum enumValue = (TrangThaiCuocHopEnum)status;
                text = GetResourceText(MeetManager.Instance.GetValueForTrangThaiCuoHop(enumValue));
            }
            catch
            {
                text = status.ToString();
            }

            text = string.IsNullOrWhiteSpace(text) ? "—" : text;

            return string.Format(
                "<span class='meet-view-status meet-view-status-{0}'>{1}</span>",
                status,
                HttpUtility.HtmlEncode(text));
        }

        private string FormatDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";

            try
            {
                DateTime dateTime = Convert.ToDateTime(value);
                return dateTime == DateTime.MinValue
                    ? "—"
                    : dateTime.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return "—";
            }
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<span class='meet-view-empty'>Không có nội dung cuộc họp.</span>";

            return HttpUtility.HtmlEncode(value)
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");
        }

        private string HtmlEncodeValue(string value)
        {
            return HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "—" : value);
        }
    }
}
