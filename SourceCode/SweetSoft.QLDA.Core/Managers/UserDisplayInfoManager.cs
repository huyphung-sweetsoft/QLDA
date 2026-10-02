using SweetSoft.QLDA.DataAccess;
using System;
using System.Web;
using System.Web.Security;

namespace SweetSoft.QLDA.Core.Managers
{
    public class UserDisplayInfo
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string AvatarHtml { get; set; }
    }

    public class UserDisplayInfoManager
    {
        private static readonly Lazy<UserDisplayInfoManager> _instance = new Lazy<UserDisplayInfoManager>(() => new UserDisplayInfoManager());
        public static UserDisplayInfoManager Instance => _instance.Value;
        public UserDisplayInfo GetUserDisplayInfo(string userName)
        {
            string displayName = string.IsNullOrWhiteSpace(userName)
                ? "—"
                : userName;

            string email = "";
            string avatar = "";

            if (!string.IsNullOrWhiteSpace(userName))
            {
                try
                {
                    MembershipUser membershipUser = Membership.GetUser(userName);

                    if (membershipUser != null)
                    {
                        email = membershipUser.Email ?? "";

                        if (membershipUser.ProviderUserKey != null &&
                            Guid.TryParse(
                                membershipUser.ProviderUserKey.ToString(),
                                out Guid userId) &&
                            userId != Guid.Empty)
                        {
                            AspnetUser user =
                                UserManager.Instance.GetUserById(userId);

                            if (user != null)
                            {
                                displayName =
                                    string.IsNullOrWhiteSpace(user.DisplayName)
                                        ? user.UserName
                                        : user.DisplayName;

                                avatar = user.Avatar ?? "";
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return new UserDisplayInfo
            {
                DisplayName = displayName,
                Email = string.IsNullOrWhiteSpace(email)
                    ? "Chưa cập nhật email"
                    : email,
                AvatarHtml = GetUserAvatarHtml(displayName, avatar)
            };
        }

        public UserDisplayInfo GetUserDisplayInfo(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new UserDisplayInfo
                {
                    DisplayName = "—",
                    Email = "Chưa cập nhật email",
                    AvatarHtml = GetUserAvatarHtml("—", "")
                };
            }

            AspnetUser user = UserManager.Instance.GetUserById(userId);

            if (user == null)
            {
                return new UserDisplayInfo
                {
                    DisplayName = "—",
                    Email = "Chưa cập nhật email",
                    AvatarHtml = GetUserAvatarHtml("—", "")
                };
            }

            string displayName =
                string.IsNullOrWhiteSpace(user.DisplayName)
                    ? user.UserName
                    : user.DisplayName;

            return new UserDisplayInfo
            {
                DisplayName = displayName,
                Email = GetUserEmail(user.UserName),
                AvatarHtml = GetUserAvatarHtml(
                    displayName,
                    user.Avatar)
            };
        }

        public string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";

            try
            {
                MembershipUser user = Membership.GetUser(userName);

                return user != null
                    ? user.Email ?? ""
                    : "";
            }
            catch
            {
                return "";
            }
        }

        public string GetUserAvatarHtml(string displayName, string avatar)
        {
            const string color = "#2563eb";

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
                string avatarUrl = ResolveAvatarUrl(avatar);

                string fallbackHtml =
                    $"<div class='user-display-avatar' " +
                    $"style='background:{color};' " +
                    $"title='{safeName}'>{initials}</div>";

                return
                    $"<img src='{HttpUtility.HtmlAttributeEncode(avatarUrl)}' " +
                    $"class='user-display-avatar' " +
                    $"alt='{safeName}' " +
                    $"title='{safeName}' " +
                    $"onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }

            return
                $"<div class='user-display-avatar' " +
                $"style='background:{color};' " +
                $"title='{safeName}'>{initials}</div>";
        }

        private string ResolveAvatarUrl(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar))
                return "";

            if (avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                avatar.StartsWith("/", StringComparison.Ordinal))
            {
                return avatar;
            }

            if (avatar.StartsWith("~/", StringComparison.Ordinal))
                return VirtualPathUtility.ToAbsolute(avatar);

            return avatar;
        }

        public string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] parts = fullName
                .Trim()
                .Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0]
                    .Substring(0, 1)
                    .ToUpper();

            return (
                parts[parts.Length - 2]
                    .Substring(0, 1) +
                parts[parts.Length - 1]
                    .Substring(0, 1)
            ).ToUpper();
        }
    }
}