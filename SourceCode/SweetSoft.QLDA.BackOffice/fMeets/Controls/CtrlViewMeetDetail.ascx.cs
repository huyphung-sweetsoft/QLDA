using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlViewMeetDetail : BaseAdminUserControl
    {
        private readonly Dictionary<Guid, string> _meetParticipantAvatars = new Dictionary<Guid, string>();
        private static readonly CultureInfo MeetHoursCulture = CultureInfo.GetCultureInfo("vi-VN");
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
            lblMeetStart.Text = FormatMeetInterval(meet.ThoiGianBatDau, meet.ThoiGianKetThuc);
            try { SetLiteralText("lblMeetActual", ReadActualMeetInterval(meet.IdLichHop)); }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("CtrlViewMeetDetail.ReadActualMeetInterval: " + ex); SetLiteralText("lblMeetActual", "<span class='meet-view-time-missing'>Chưa thể đọc giờ thực tế</span>"); }
            lblMeetStatus.Text = GetStatusHtml(meet.TrangThai);
            ltrMeetContent.Text = ToDisplayHtml(meet.NoiDungCuocHop);

            BindParticipants(meet.IdLichHop);
            BindMeetingLabor(meet);
        }

        private static string ReadActualMeetInterval(Guid meetId)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                foreach (ConnectionStringSettings candidate in ConfigurationManager.ConnectionStrings)
                    if (candidate != null && !string.IsNullOrWhiteSpace(candidate.ConnectionString) && candidate.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    { settings = candidate; break; }
            }
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                return "<span class='meet-view-time-missing'>Chưa ghi nhận</span>";
            using (SqlConnection sql = new SqlConnection(settings.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"SELECT ThoiGianBatDauThucTe,ThoiGianKetThucThucTe FROM dbo.TblLichHop WHERE IdLichHop=@IdLichHop AND ISNULL(DaXoa,0)=0", sql))
            {
                command.Parameters.Add("@IdLichHop", SqlDbType.UniqueIdentifier).Value = meetId;
                sql.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        DateTime start = reader.GetDateTime(0), end = reader.GetDateTime(1);
                        if (end > start) return FormatMeetInterval(start, end);
                    }
                }
            }
            return "<span class='meet-view-time-missing'>Chưa ghi nhận</span>";
        }

        private void BindParticipants(Guid meetId)
        {
            _meetParticipantAvatars.Clear();
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
                        // Bỏ qua nhân viên không đọc được thông tin.
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
                    _meetParticipantAvatars[participantId] = GetAvatarHtml(displayName, user.Avatar, i, "meet-view-participant-avatar");
                }
            }
            catch
            {
                // Vẫn cho phép mở popup ngay cả khi không lấy được danh sách người tham gia.
            }
        }

        // Chỉ hiển thị. Mọi điều chỉnh giờ công vẫn thực hiện tại CtrlMeetGioCongTab.
        // FindControl tránh bắt buộc sửa file .ascx.designer.cs khi tích hợp 2 file này.
        private void BindMeetingLabor(TblLichHop meet)
        {
            Literal output = FindMeetingLaborLiteral(upMeetView.ContentTemplateContainer);
            if (output == null)
                return;

            if (meet.ThoiGianBatDau == DateTime.MinValue ||
                meet.ThoiGianKetThuc == DateTime.MinValue ||
                meet.ThoiGianKetThuc <= meet.ThoiGianBatDau)
            {
                output.Text = GetMeetLaborNotice("far fa-calendar-times", "Chưa có thời gian hợp lệ",
                    "Không thể hiển thị giờ công khi thời gian bắt đầu và kết thúc cuộc họp chưa hợp lệ.");
                return;
            }

            // Đồng nhất điều kiện khóa với tab Giờ công: chỉ lấy số liệu khi cuộc họp kết thúc.
            if (meet.ThoiGianKetThuc > DateTime.Now)
            {
                output.Text = GetMeetLaborNotice("fas fa-lock", "Chưa thể xem giờ công",
                    "Cuộc họp chưa kết thúc. Giờ công sẽ được ghi nhận sau khi cuộc họp kết thúc.");
                return;
            }

            try
            {
                List<MeetLaborRow> roster = GetMeetingLaborRows(meet.IdLichHop);
                decimal duration = GetEffectiveMeetingDuration(meet);
                output.Text = BuildMeetLaborHtml(roster, duration, meet.IdLichHop);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Trace.TraceError("CtrlViewMeetDetail.BindMeetingLabor: " + exception);
                output.Text = "<div class='meet-view-labor-error'><i class='fas fa-exclamation-circle'></i> " +
                    "Không thể đọc dữ liệu giờ công của cuộc họp. Vui lòng kiểm tra lại dữ liệu và thử mở lại.</div>";
            }
        }

        private static Literal FindMeetingLaborLiteral(Control parent)
        {
            if (parent == null) return null;
            if (parent.ID == "ltrMeetLabor") return parent as Literal;
            foreach (Control child in parent.Controls)
            {
                Literal found = FindMeetingLaborLiteral(child);
                if (found != null) return found;
            }
            return null;
        }

        private void SetLiteralText(string controlId, string value)
        {
            Literal literal = FindLiteralRecursive(upMeetView.ContentTemplateContainer, controlId);
            if (literal != null) literal.Text = value ?? string.Empty;
        }

        private static Literal FindLiteralRecursive(Control parent, string controlId)
        {
            if (parent == null || string.IsNullOrEmpty(controlId)) return null;
            if (string.Equals(parent.ID, controlId, StringComparison.Ordinal)) return parent as Literal;
            foreach (Control child in parent.Controls)
            {
                Literal found = FindLiteralRecursive(child, controlId);
                if (found != null) return found;
            }
            return null;
        }

        private sealed class MeetLaborRow
        {
            public Guid IdNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public string Email { get; set; }
            public bool LaChuTri { get; set; }
            public bool DaThamGia { get; set; }
            public decimal? SoGioThucTe { get; set; }
        }

        // Thu tu uu tien: gio ca nhan vao/ra > so gio PM nhap tay > gio hop thuc te > gio du kien.
        private static decimal GetEffectiveMeetingDuration(TblLichHop meet)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                return Math.Round((decimal)(meet.ThoiGianKetThuc - meet.ThoiGianBatDau).TotalMinutes / 60m, 2);
            using (SqlConnection connection = new SqlConnection(settings.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
SELECT ThoiGianBatDauThucTe, ThoiGianKetThucThucTe
FROM dbo.TblLichHop WHERE IdLichHop=@IdLichHop", connection))
            {
                command.Parameters.Add("@IdLichHop", SqlDbType.UniqueIdentifier).Value = meet.IdLichHop;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        DateTime start = reader.GetDateTime(0), end = reader.GetDateTime(1);
                        if (end > start) return Math.Round((decimal)(end - start).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
                    }
                }
            }
            return Math.Round((decimal)(meet.ThoiGianKetThuc - meet.ThoiGianBatDau).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
        }

        // Cùng nguồn dữ liệu và thứ tự ưu tiên với CtrlMeetGioCongTab / CtrlChiTietCongCuocHop:
        // DaThamGia của PM > XacNhanThamGia gốc. Chủ trì không bị bỏ sót.
        private static List<MeetLaborRow> GetMeetingLaborRows(Guid meetId)
        {
            const string query = @"
WITH DanhSach AS (
    SELECT DISTINCT p.IdNhanVien FROM dbo.TblLichHop_NhanVien p
    WHERE p.IdLichHop=@IdLichHop AND p.IdNhanVien IS NOT NULL
    UNION
    SELECT m.IdNguoiTao FROM dbo.TblLichHop m
    WHERE m.IdLichHop=@IdLichHop AND m.IdNguoiTao IS NOT NULL
)
SELECT d.IdNhanVien,
    COALESCE(NULLIF(LTRIM(RTRIM(u.DisplayName)),N''),
             NULLIF(LTRIM(RTRIM(u.UserName)),N''),CONVERT(NVARCHAR(36),d.IdNhanVien)) AS TenNhanVien,
    COALESCE(NULLIF(LTRIM(RTRIM(mu.Email)),N''),N'') AS Email,
    CASE WHEN d.IdNhanVien=m.IdNguoiTao THEN 1 ELSE 0 END AS LaChuTri,
    ISNULL(CAST(g.DaThamGia AS INT),
           ISNULL((SELECT MAX(CAST(ISNULL(p.XacNhanThamGia,0) AS INT))
                   FROM dbo.TblLichHop_NhanVien p
                   WHERE p.IdLichHop=@IdLichHop AND p.IdNhanVien=d.IdNhanVien),0)) AS DaThamGia,
    CAST(CASE
        WHEN g.GioVaoThucTe IS NOT NULL AND g.GioRaThucTe > g.GioVaoThucTe
          THEN ROUND(DATEDIFF(MINUTE,g.GioVaoThucTe,g.GioRaThucTe)/60.0,2)
        WHEN g.SoGioThucTe IS NOT NULL THEN g.SoGioThucTe
        ELSE ROUND(DATEDIFF(MINUTE,
          CASE WHEN m.ThoiGianBatDauThucTe IS NOT NULL AND m.ThoiGianKetThucThucTe>m.ThoiGianBatDauThucTe
               THEN m.ThoiGianBatDauThucTe ELSE m.ThoiGianBatDau END,
          CASE WHEN m.ThoiGianBatDauThucTe IS NOT NULL AND m.ThoiGianKetThucThucTe>m.ThoiGianBatDauThucTe
               THEN m.ThoiGianKetThucThucTe ELSE m.ThoiGianKetThuc END)/60.0,2)
    END AS DECIMAL(10,2)) AS SoGioThucTe
FROM DanhSach d
CROSS JOIN dbo.TblLichHop m
LEFT JOIN dbo.aspnet_Users u ON u.UserId=d.IdNhanVien
LEFT JOIN dbo.aspnet_Membership mu ON mu.UserId=d.IdNhanVien
LEFT JOIN dbo.TblLichHopGioCong g ON g.IdLichHop=@IdLichHop AND g.IdNhanVien=d.IdNhanVien
WHERE m.IdLichHop=@IdLichHop
ORDER BY TenNhanVien, d.IdNhanVien;";

            ConnectionStringSettings connection = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionString))
            {
                foreach (ConnectionStringSettings candidate in ConfigurationManager.ConnectionStrings)
                {
                    if (candidate != null && !string.IsNullOrWhiteSpace(candidate.ConnectionString) &&
                        candidate.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        connection = candidate;
                        break;
                    }
                }
            }
            if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionString))
                throw new ConfigurationErrorsException("Không tìm thấy chuỗi kết nối cơ sở dữ liệu dự án.");

            List<MeetLaborRow> rows = new List<MeetLaborRow>();
            using (SqlConnection sql = new SqlConnection(connection.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, sql))
            {
                command.Parameters.Add("@IdLichHop", SqlDbType.UniqueIdentifier).Value = meetId;
                sql.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool joined = Convert.ToInt32(reader["DaThamGia"], CultureInfo.InvariantCulture) != 0;
                        rows.Add(new MeetLaborRow
                        {
                            IdNhanVien = (Guid)reader["IdNhanVien"],
                            TenNhanVien = Convert.ToString(reader["TenNhanVien"], CultureInfo.InvariantCulture),
                            Email = Convert.ToString(reader["Email"], CultureInfo.InvariantCulture),
                            LaChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) != 0,
                            DaThamGia = joined,
                            SoGioThucTe = reader["SoGioThucTe"] == DBNull.Value ? (decimal?)null :
                                Convert.ToDecimal(reader["SoGioThucTe"], CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            return rows;
        }
        private string BuildMeetLaborHtml(List<MeetLaborRow> rows, decimal duration, Guid meetId)
        {
            const int pageSize = 5;
            int joined = 0, missing = 0;
            decimal theoryTotal = 0m, actualTotal = 0m;
            foreach (MeetLaborRow person in rows)
            {
                if (!person.DaThamGia) continue;
                joined++;
                theoryTotal += duration;
                if (person.SoGioThucTe.HasValue) actualTotal += person.SoGioThucTe.Value;
                else missing++;
            }

            int totalPages = rows.Count <= 0 ? 1 : (int)Math.Ceiling((double)rows.Count / pageSize);
            StringBuilder html = new StringBuilder(2400 + rows.Count * 600);
            html.Append("<div class='meet-view-labor-root' data-current-page='1' data-total-pages='")
                .Append(totalPages.ToString(CultureInfo.InvariantCulture)).Append("'>");
            html.Append("<div class='meet-view-labor-overview'>");
            AddMeetLaborStat(html, "Thời lượng", Math.Round(duration * 60m, 0).ToString("0", MeetHoursCulture) + " phút", "");
            AddMeetLaborStat(html, "Có tham gia", joined.ToString(CultureInfo.InvariantCulture) + "/" + rows.Count.ToString(CultureInfo.InvariantCulture), "");
            AddMeetLaborStat(html, "Tổng lý thuyết", FormatMeetLaborHours(theoryTotal), "is-violet");
            AddMeetLaborStat(html, "Tổng thực tế", missing == joined && joined > 0 ? "Chưa nhập" : FormatMeetLaborHours(actualTotal), missing == joined && joined > 0 ? "is-muted" : "is-green");
            html.Append("</div>");

            html.Append("<div class='meet-view-labor-scroll'><table class='meet-view-labor-table'>")
                .Append("<colgroup><col style='width:40%'/><col style='width:24%'/><col style='width:18%'/><col style='width:18%'/></colgroup>")
                .Append("<thead><tr><th>Nhân viên</th><th>Tham gia</th><th>Lý thuyết</th><th>Thực tế</th></tr></thead><tbody>");
            if (rows.Count == 0)
            {
                html.Append("<tr><td colspan='4'><div class='meet-view-labor-empty'>Cuộc họp chưa có nhân viên để hiển thị giờ công.</div></td></tr>");
            }
            for (int i = 0; i < rows.Count; i++)
            {
                MeetLaborRow person = rows[i];
                int page = (i / pageSize) + 1;
                string avatar;
                if (!_meetParticipantAvatars.TryGetValue(person.IdNhanVien, out avatar))
                {
                    AspnetUser user = null;
                    try { user = UserManager.Instance.GetUserById(person.IdNhanVien); }
                    catch { }
                    avatar = GetAvatarHtml(person.TenNhanVien, user != null ? user.Avatar : "", i, "meet-view-participant-avatar");
                }
                string name = HttpUtility.HtmlEncode(person.TenNhanVien ?? "—");
                string email = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(person.Email) ? "Chưa cập nhật email" : person.Email);
                string joinedClass = person.DaThamGia ? "" : " no";
                string joinedText = person.DaThamGia ? "Có tham gia" : "Không tham gia";
                string joinedIcon = person.DaThamGia ? "fas fa-check" : "fas fa-minus";
                string actualClass = "actual", actualText;
                if (!person.DaThamGia) actualText = FormatMeetLaborHours(0m);
                else if (!person.SoGioThucTe.HasValue) { actualText = "Chưa nhập"; actualClass = "missing"; }
                else actualText = FormatMeetLaborHours(person.SoGioThucTe.Value);
                html.Append("<tr data-labor-page='").Append(page.ToString(CultureInfo.InvariantCulture)).Append("'");
                if (page > 1) html.Append(" style='display:none;' ");
                html.Append("><td><div class='meet-view-labor-person'>").Append(avatar)
                    .Append("<div class='meet-view-labor-person-info'><span class='meet-view-labor-name' title='")
                    .Append(HttpUtility.HtmlAttributeEncode(person.TenNhanVien ?? "")).Append("'>").Append(name);
                if (person.LaChuTri) html.Append(" <i class='fas fa-crown' title='Chủ trì' aria-label='Chủ trì'></i>");
                html.Append("</span><span class='meet-view-labor-email' title='")
                    .Append(HttpUtility.HtmlAttributeEncode(person.Email ?? "")).Append("'>").Append(email)
                    .Append("</span></div></div></td><td><span class='meet-view-labor-join").Append(joinedClass)
                    .Append("'><i class='").Append(joinedIcon).Append("'></i>").Append(joinedText)
                    .Append("</span></td><td class='meet-view-labor-hour'>").Append(FormatMeetLaborHours(person.DaThamGia ? duration : 0m))
                    .Append("</td><td class='meet-view-labor-hour ").Append(actualClass).Append("'>").Append(actualText).Append("</td></tr>");
            }
            html.Append("</tbody></table></div>");
            if (totalPages > 1)
            {
                html.Append("<div class='meet-view-labor-pager'>")
                    .Append("<button type='button' class='meet-view-labor-page-btn prev' onclick='return SweetMeetViewLaborPager.move(this,-1);' disabled='disabled' aria-label='Trang trước'><i class='fas fa-chevron-left'></i></button>");
                for (int i = 1; i <= totalPages; i++)
                {
                    html.Append("<button type='button' class='meet-view-labor-page-btn");
                    if (i == 1) html.Append(" is-active");
                    html.Append("' data-page='").Append(i.ToString(CultureInfo.InvariantCulture)).Append("' onclick='return SweetMeetViewLaborPager.showPage(this,").Append(i.ToString(CultureInfo.InvariantCulture)).Append(");'>").Append(i.ToString(CultureInfo.InvariantCulture)).Append("</button>");
                }
                html.Append("<button type='button' class='meet-view-labor-page-btn next' onclick='return SweetMeetViewLaborPager.move(this,1);'");
                if (totalPages <= 1) html.Append(" disabled='disabled'");
                html.Append(" aria-label='Trang sau'><i class='fas fa-chevron-right'></i></button>")
                    .Append("<span class='meet-view-labor-page-status'>1 / ").Append(totalPages.ToString(CultureInfo.InvariantCulture)).Append("</span></div>");
            }
            html.Append("<div class='meet-view-labor-footnote'><span class='meet-view-labor-note'><i class='fas fa-info-circle'></i><span>");
            if (missing > 0) html.Append("Còn ").Append(missing.ToString(CultureInfo.InvariantCulture)).Append(" người chưa nhập công thực tế; tổng hiện tại chỉ tính phần đã nhập.");
            else html.Append("Dữ liệu đồng bộ với phần Giờ công; chỉnh sửa được thực hiện tại tab Giờ công.");
            html.Append("</span></span><button type='button' class='meet-view-labor-detail-btn' onclick=\"return SweetMeetViewNavigate.go('")
                .Append(meetId.ToString("D")).Append("');\"><i class='fas fa-external-link-alt' aria-hidden='true'></i>Xem chi tiết</button></div></div>");
            return html.ToString();
        }

        private static void AddMeetLaborStat(StringBuilder html, string label, string value, string className)
        {
            html.Append("<div class='meet-view-labor-stat");
            if (!string.IsNullOrEmpty(className)) html.Append(" ").Append(className);
            html.Append("'><span>").Append(HttpUtility.HtmlEncode(label)).Append("</span><strong>").Append(HttpUtility.HtmlEncode(value)).Append("</strong></div>");
        }

        // Ngày chỉ xuất hiện một lần nếu cuộc họp bắt đầu và kết thúc cùng ngày.
        // Họp qua ngày vẫn hiển thị đủ ngày kết thúc để không gây nhầm lẫn.
        private static string FormatMeetInterval(DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                return "Chưa có thời gian hợp lệ";
            string startText = start.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")) + " <span class='meet-view-time-dot' aria-hidden='true'>·</span> " + start.ToString("HH:mm");
            string endText = start.Date == end.Date ? end.ToString("HH:mm") : end.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
            return startText + " <span class='meet-view-time-dash' aria-hidden='true'>–</span> " + endText;
        }

        private static string FormatDuration(DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end <= start)
                return "—";

            decimal minutes = Math.Round((decimal)(end - start).TotalMinutes, 0, MidpointRounding.AwayFromZero);
            decimal hours = Math.Round((decimal)(end - start).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
            return minutes.ToString("0", MeetHoursCulture) + " phút = " + FormatMeetLaborHours(hours);
        }

        private static string FormatMeetLaborHours(decimal value)
        {
            return value.ToString("0.##", MeetHoursCulture) + " giờ";
        }

        private static string GetMeetLaborNotice(string icon, string title, string note)
        {
            return "<div class='meet-view-labor-pending'><i class='" + icon + "' aria-hidden='true'></i><strong>" +
                   HttpUtility.HtmlEncode(title) + "</strong><span>" + HttpUtility.HtmlEncode(note) + "</span></div>";
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
