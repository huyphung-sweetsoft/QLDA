using System;
using System.Web;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public static class NotificationHelper
    {
        public static string BuildNotificationLink(
            string loaiThongBao,
            Guid? idDuAn,
            Guid? idCongViec)
        {
            switch (loaiThongBao)
            {
                case ThongBaoTypes.CongViec:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectTasks(idDuAn.Value);
                    return RewriteURLHelper.Projects;

                case ThongBaoTypes.DuAn:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectDetail(idDuAn.Value);
                    return RewriteURLHelper.Projects;

                case ThongBaoTypes.VanDe:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectIssues(idDuAn.Value);
                    return RewriteURLHelper.Projects;

                case ThongBaoTypes.LichHop:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectMeets(idDuAn.Value);
                    return RewriteURLHelper.Projects;

                case ThongBaoTypes.TaiLieu:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        if (idCongViec.HasValue && idCongViec.Value != Guid.Empty)
                            return RewriteURLHelper.ProjectDocumentDetail(idDuAn.Value, idCongViec.Value);

                        return RewriteURLHelper.ProjectDocuments(idDuAn.Value);
                    }
                    else if (idCongViec.HasValue && idCongViec.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.DocumentDetail(idCongViec.Value);
                    }
                    return RewriteURLHelper.Documents;

                case ThongBaoTypes.RuiRo:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectRisks(idDuAn.Value);
                    return RewriteURLHelper.Projects;

                case ThongBaoTypes.ChiPhi:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                        return RewriteURLHelper.ProjectCosts(idDuAn.Value);
                    return RewriteURLHelper.Projects;
            }

            return string.Empty;
        }

        // Tương thích với các chỗ gọi đã truyền action 4 tham số.
        public static string BuildNotificationLink(
            string loaiThongBao,
            Guid? idDuAn,
            Guid? idCongViec,
            string duongDanLienKet)
        {
            return BuildNotificationLink(
                loaiThongBao,
                idDuAn,
                idCongViec,
                duongDanLienKet,
                null);
        }

        // Overload dùng cho thông báo có nội dung kết quả riêng, chẳng hạn duyệt/từ chối.
        public static string BuildNotificationLink(
            string loaiThongBao,
            Guid? idDuAn,
            Guid? idCongViec,
            string duongDanLienKet,
            Guid? idThongBao)
        {
            string url = BuildNotificationLink(loaiThongBao, idDuAn, idCongViec);
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(duongDanLienKet))
                return url;

            string actionName = duongDanLienKet.Trim();

            // Loai thong bao VAN_DE co action rieng, KHONG dua vao enum ThongBaoAction Task.
            if (string.Equals(loaiThongBao, ThongBaoTypes.VanDe, StringComparison.OrdinalIgnoreCase)
                && actionName.StartsWith("GioXuLyVanDe:", StringComparison.OrdinalIgnoreCase))
            {
                Guid issueId;
                string rest=actionName.Substring("GioXuLyVanDe:".Length);
                string[] parts=rest.Split(':');
                if (Guid.TryParse(parts[0], out issueId) && issueId != Guid.Empty)
                {
                    // ProjectId in KhaiBaoGio is protected by the existing project URL rewrite.
                    // Never send an unprotected Guid in ?ProjectId: that query does not resolve.
                    if (!idDuAn.HasValue || idDuAn.Value == Guid.Empty) return url;
                    string issueAction=parts.Length>1?parts[1]:"detail";
                    if (string.Equals(issueAction,"pm-final",StringComparison.OrdinalIgnoreCase)
                        && idThongBao.HasValue && idThongBao.Value!=Guid.Empty)
                    {
                        string pmUrl=AppendQueryParameter(
                            RewriteURLHelper.ProjectDetail(idDuAn.Value).TrimEnd('/') + "/TimeTracking",
                            "pmFinalIssueId",issueId.ToString("D"));
                        return AppendQueryParameter(pmUrl,"pmFinalNotificationId",idThongBao.Value.ToString("D"));
                    }
                    if (string.Equals(issueAction,"review",StringComparison.OrdinalIgnoreCase))
                        return AppendQueryParameter(RewriteURLHelper.ProjectIssues(idDuAn.Value),
                            "reviewIssueId",issueId.ToString("D"));
                    if (string.Equals(issueAction,"detail",StringComparison.OrdinalIgnoreCase) &&
                        idThongBao.HasValue && idThongBao.Value!=Guid.Empty)
                    {
                        string result=AppendQueryParameter(RewriteURLHelper.ProjectIssues(idDuAn.Value),
                            "resultIssueId",issueId.ToString("D"));
                        return AppendQueryParameter(result,"notificationId",idThongBao.Value.ToString("D"));
                    }
                    string dest=AppendQueryParameter(
                        RewriteURLHelper.ProjectDetail(idDuAn.Value).TrimEnd('/') + "/TimeTracking",
                        "vanDeId", issueId.ToString("D"));
                    return AppendQueryParameter(dest,"gvAction",
                        string.Equals(issueAction,"report",StringComparison.OrdinalIgnoreCase)?"report":"detail");
                }
                return url;
            }

            // Action này dùng ID bản ghi thông báo để mở popup kết quả lịch sử.
            // Chấp nhận tường minh để tránh rơi về URL trang Tasks nếu enum ở project
            // chưa được cập nhật đồng bộ, vốn là nguyên nhân popup không tự mở.
            if (!string.Equals(
                    actionName,
                    "CtrlTaskCompletionResult",
                    StringComparison.OrdinalIgnoreCase))
            {
                ThongBaoAction action;
                if (!Enum.TryParse(actionName, true, out action) ||
                    !Enum.IsDefined(typeof(ThongBaoAction), action))
                {
                    // Tương thích dữ liệu cũ nếu cột này trước đây chứa URL đầy đủ.
                    if (Uri.TryCreate(actionName, UriKind.Absolute, out Uri absolute) &&
                        (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
                        return actionName;

                    // Keep legacy internal URLs. Previously the bell returned them verbatim.
                    // Do not accept protocol-relative URLs (//...), backslashes or javascript:.
                    if (actionName.StartsWith("~/", StringComparison.Ordinal))
                        return VirtualPathUtility.ToAbsolute(actionName);
                    if (actionName.StartsWith("/", StringComparison.Ordinal)
                        && !actionName.StartsWith("//", StringComparison.Ordinal)
                        && actionName.IndexOf('\\') < 0)
                        return actionName;
                    return url;
                }

                actionName = action.ToString();
            }

            url = AppendQueryParameter(url, "notificationAction", actionName);

            if (idCongViec.HasValue && idCongViec.Value != Guid.Empty)
                url = AppendQueryParameter(url, "notificationTaskId", idCongViec.Value.ToString());

            if (idThongBao.HasValue && idThongBao.Value != Guid.Empty)
                url = AppendQueryParameter(url, "notificationId", idThongBao.Value.ToString());

            return url;
        }

        private static string AppendQueryParameter(string url, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            int hashIndex = url.IndexOf('#');
            string fragment = hashIndex >= 0 ? url.Substring(hashIndex) : string.Empty;
            string baseUrl = hashIndex >= 0 ? url.Substring(0, hashIndex) : url;

            string separator;
            if (!baseUrl.Contains("?"))
                separator = "?";
            else if (baseUrl.EndsWith("?") || baseUrl.EndsWith("&"))
                separator = string.Empty;
            else
                separator = "&";

            return baseUrl + separator
                + HttpUtility.UrlEncode(name) + "="
                + HttpUtility.UrlEncode(value) + fragment;
        }
    }
}
