using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCosts.Controls
{
    public partial class CtrlViewCostDetail : BaseAdminUserControl
    {
        private readonly ControlHelpers _control = new ControlHelpers();

        public void OpenModal(Guid costId)
        {
            if (costId == Guid.Empty)
                return;

            TblChiPhi cost = TblChiPhi.FetchByID(costId);

            if (cost == null || cost.DaXoa == true)
                return;

            BindCost(cost);
            upCostView.Update();
            mdlCostView.OpenModal(true);
        }

        private void BindCost(TblChiPhi cost)
        {
            lblCostName.Text = HtmlEncodeValue(cost.TenKhoanChi);
            lblUnitPrice.Text = FormatNumber(cost.DonGia);
            lblQuantity.Text = cost.SoLuong.HasValue ? cost.SoLuong.Value.ToString("N0") : "—";
            lblTotal.Text = cost.SoTien.ToString("N0");
            lblCreatedDate.Text = cost.NgayTao.ToString("dd/MM/yyyy HH:mm");
            BindCreatedBy(cost.IdNguoiTao);

            ltrDescription.Text = ToDisplayHtml(cost.MoTaChiTiet);

            BindPerson(
                cost.IdNhanVienDeNghi,
                lblRequesterName,
                lblRequesterEmail,
                ltrRequesterAvatar,
                0);

            BindStatus(cost.TrangThai);

            pnlReject.Visible = cost.TrangThai == 2;
            lblRejectReason.Text =
                cost.TrangThai == 2
                    ? ToDisplayHtml(cost.LyDoTuChoi)
                    : string.Empty;
        }

        private void BindCreatedBy(Guid? userId)
        {
            lblCreatorName.Text = "—";
            lblCreatorEmail.Text = "Chưa cập nhật email";
            ltrCreatorAvatar.Text = string.Empty;

            if (!userId.HasValue || userId.Value == Guid.Empty)
                return;

            try
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId.Value);

                if (user == null)
                    return;

                UserDisplayInfo creator =
                    UserDisplayInfoManager.Instance.GetUserDisplayInfo(user.UserName);

                if (creator == null)
                    return;

                lblCreatorName.Text =
                    HttpUtility.HtmlEncode(
                        string.IsNullOrWhiteSpace(creator.DisplayName)
                            ? user.UserName
                            : creator.DisplayName);

                lblCreatorEmail.Text =
                    HttpUtility.HtmlEncode(
                        string.IsNullOrWhiteSpace(creator.Email)
                            ? "Chưa cập nhật email"
                            : creator.Email);

                ltrCreatorAvatar.Text = creator.AvatarHtml;
            }
            catch
            {
            }
        }

        private void BindPerson(
            Guid? userId,
            Literal nameControl,
            Literal emailControl,
            Literal avatarControl,
            int avatarIndex)
        {
            string displayName = "—";
            string email = "Chưa cập nhật email";
            string avatar = "";

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                try
                {
                    AspnetUser user =
                        UserManager.Instance.GetUserById(userId.Value);

                    if (user != null)
                    {
                        displayName =
                            string.IsNullOrWhiteSpace(user.DisplayName)
                                ? user.UserName
                                : user.DisplayName;

                        email = GetUserEmail(user.UserName);

                        if (string.IsNullOrWhiteSpace(email))
                            email = "Chưa cập nhật email";

                        avatar = user.Avatar;
                    }
                }
                catch
                {
                }
            }

            nameControl.Text =
                HttpUtility.HtmlEncode(displayName);

            emailControl.Text =
                HttpUtility.HtmlEncode(email);

            avatarControl.Text =
                GetAvatarHtml(displayName, avatar, avatarIndex);
        }

        private void BindStatus(byte? status)
        {
            _control.BindTrangThaiChiPhi(ddlStatusSource);

            string value =
                status.HasValue
                    ? status.Value.ToString()
                    : string.Empty;

            ListItem item =
                ddlStatusSource.Items.FindByValue(value);

            string text =
                item != null
                    ? item.Text
                    : "—";

            string css =
                GetStatusCss(status, text);

            lblStatus.Text =
                $"<span class=\"cost-view-status {css}\">{HttpUtility.HtmlEncode(text)}</span>";
        }

        private string GetStatusCss(byte? status, string text)
        {
            if (status == 2)
                return "status-rejected";

            if (status == 1)
                return "status-approved";

            if (status == 0)
                return "status-pending";

            string normalized =
                (text ?? string.Empty).ToLowerInvariant();

            if (normalized.Contains("từ chối") ||
                normalized.Contains("tuchoi"))
            {
                return "status-rejected";
            }

            if (normalized.Contains("duyệt") ||
                normalized.Contains("duyet"))
            {
                return "status-approved";
            }

            if (normalized.Contains("chưa") ||
                normalized.Contains("cho duyệt") ||
                normalized.Contains("pending"))
            {
                return "status-pending";
            }

            return "status-neutral";
        }

        private string FormatNumber(decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString("#,##0")
                : "—";
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";

            try
            {
                MembershipUser user =
                    Membership.GetUser(userName);

                return user != null
                    ? user.Email
                    : "";
            }
            catch
            {
                return "";
            }
        }

        private string GetAvatarHtml(
            string displayName,
            string avatar,
            int index)
        {
            string[] colors =
            {
                "#7c3aed",
                "#2563eb",
                "#059669",
                "#d97706",
                "#db2777"
            };

            string color =
                colors[index % colors.Length];

            string safeName =
                HttpUtility.HtmlAttributeEncode(
                    displayName ?? "");

            string initials =
                HttpUtility.HtmlEncode(
                    GetInitials(displayName));

            bool isDefaultAvatar =
                string.IsNullOrWhiteSpace(avatar) ||
                avatar.EndsWith(
                    "/Styles/images/user-icon.png",
                    StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl =
                    avatar.StartsWith(
                        "~",
                        StringComparison.Ordinal)
                        ? Page.ResolveUrl(avatar)
                        : avatar;

                avatarUrl =
                    HttpUtility.HtmlAttributeEncode(
                        avatarUrl);

                string fallbackHtml =
                    $"<div class='cost-view-avatar' style='background:{color};'>{initials}</div>";

                return
                    $"<img src='{avatarUrl}' " +
                    $"class='cost-view-avatar' " +
                    $"alt='{safeName}' " +
                    $"title='{safeName}' " +
                    $"onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }

            return
                $"<div class='cost-view-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] parts =
                fullName
                    .Trim()
                    .Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return parts[0]
                    .Substring(0, 1)
                    .ToUpperInvariant();
            }

            return (
                parts[parts.Length - 2]
                    .Substring(0, 1) +
                parts[parts.Length - 1]
                    .Substring(0, 1)
            ).ToUpperInvariant();
        }

        private string HtmlEncodeValue(string value)
        {
            return HttpUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(value)
                    ? "—"
                    : value);
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<span class=\"text-muted\">—</span>";

            return HttpUtility.HtmlEncode(value)
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");
        }
    }
}