using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.SysManager;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlChiTietCongCuocHop : BaseAdminUserControl
    {
        private const int SoCuocHopMoiTrang = 5;
        private const int SoNhanVienMoiTrang = 5;
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        public Guid ProjectId
        {
            get { return ViewState["PdMeetingProjectId"] is Guid ? (Guid)ViewState["PdMeetingProjectId"] : Guid.Empty; }
            set { ViewState["PdMeetingProjectId"] = value; }
        }

        protected string ModalClientId { get { return mdlMeetingDetail.ClientID; } }
        protected string ActionButtonId { get { return btnMeetingAction.ClientID; } }

        private sealed class CuocHopRow
        {
            public Guid Id { get; set; }
            public string Ten { get; set; }
            public DateTime BatDau { get; set; }
            public DateTime KetThuc { get; set; }
            public bool LaChuTri { get; set; }
        }

        private sealed class NhanVienRow
        {
            public Guid IdNhanVien { get; set; }
            public string Ten { get; set; }
            public string Email { get; set; }
            public string Avatar { get; set; }
            public bool ChuTri { get; set; }
            public bool ThamGia { get; set; }
            public decimal? GioThucTe { get; set; }
        }

        private static string Enc(string value) { return HttpUtility.HtmlEncode(value ?? string.Empty); }
        private static string Attr(string value) { return HttpUtility.HtmlAttributeEncode(value ?? string.Empty).Replace("'", "&#39;"); }
        private static string Gio(decimal value) { return value.ToString("0.##", Vi) + " giờ"; }

        private static string GetInitials(string name)
        {
            var words = (name ?? string.Empty).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "?";
            if (words.Length == 1)
            {
                string single = words[0];
                return single.Substring(0, Math.Min(1, single.Length)).ToUpperInvariant();
            }
            return (words[0].Substring(0, 1) + words[words.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static string GetConnectionString()
        {
            ConnectionStringSettings named = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (named != null && !string.IsNullOrWhiteSpace(named.ConnectionString)) return named.ConnectionString;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) && item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    return item.ConnectionString;
            throw new ConfigurationErrorsException("Không tìm thấy chuỗi kết nối SweetSoft_QLDA.");
        }

        private bool CanViewProject(Guid userId)
        {
            if (ProjectId == Guid.Empty || userId == Guid.Empty) return false;
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
            return (pm.HasValue && pm.Value == userId) || ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(userId, ProjectId) != null;
        }

        private bool CanViewTeam()
        {
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
            return pm.HasValue && pm.Value == SweetContext.Current.UserId;
        }

        private bool ValidMember(Guid memberId)
        {
            Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
            return memberId != Guid.Empty && ((pm.HasValue && pm.Value == memberId)
                || (ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(ProjectId) ?? new List<Guid>()).Contains(memberId));
        }

        private static string SafeSearch(string search)
        {
            var trimmed = (search ?? string.Empty).Trim();
            return trimmed.Length > 100 ? trimmed.Substring(0, 100) : trimmed;
        }

        private static string LikePattern(string text)
        {
            return "%" + (text ?? string.Empty).Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
        }

        private static int PageNumber(string value)
        {
            int n;
            return int.TryParse(value, out n) ? Math.Max(1, Math.Min(100000, n)) : 1;
        }

        private static DateTime? ParseDate(string value)
        {
            DateTime result;
            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
                ? (DateTime?)result.Date : null;
        }

        private void SetEmpty(string message)
        {
            litCount.Text = "0";
            litMeetings.Text = "<div class='pmc-empty'>" + Enc(message) + "</div>";
            litMeetingPager.Text = string.Empty;
            litRosterHeader.Text = "<div class='pmc-right-head'><h5>Chi tiết giờ công</h5></div>";
            litRoster.Text = "<div class='pmc-empty'>Không có dữ liệu hiển thị.</div>";
            litRosterPager.Text = string.Empty;
            upMeeting.Update();
        }

        protected void btnMeetingAction_Click(object sender, EventArgs e)
        {
            string action = (hfAction.Value ?? string.Empty).Trim();
            string kind = (hfKind.Value ?? string.Empty).Trim();
            bool team = kind == "pdTeamDetail-meeting";
            Guid actor = SweetContext.Current.UserId;
            if ((kind != "pdPersonalMeetingDetail" && !team) || !CanViewProject(actor) || (team && !CanViewTeam()))
            {
                SetEmpty("Bạn không có quyền xem dữ liệu cuộc họp này.");
                RegisterCallback(action == "open");
                return;
            }

            Guid member = actor;
            if (team && (!Guid.TryParse(hfMember.Value, out member) || !ValidMember(member)))
            {
                SetEmpty("Nhân viên không thuộc dự án.");
                RegisterCallback(action == "open");
                return;
            }

            string term = SafeSearch(hfSearch.Value);
            hfSearch.Value = term;
            string filter = hfState.Value == "active" || hfState.Value == "ended" ? hfState.Value : "all";
            if (team) filter = "ended";
            hfState.Value = filter;

            DateTime? from = ParseDate(hfDateFrom.Value);
            DateTime? to = ParseDate(hfDateTo.Value);
            int page = PageNumber(hfMeetingPage.Value);
            int rosterPage = PageNumber(hfRosterPage.Value);
            if (action == "open" || action == "filter") { page = 1; rosterPage = 1; }
            if (action == "page" || action == "select") rosterPage = 1;

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    int total = CountMeetings(connection, member, term, filter, from, to);
                    int pages = Math.Max(1, (total + SoCuocHopMoiTrang - 1) / SoCuocHopMoiTrang);
                    page = Math.Min(page, pages);
                    List<CuocHopRow> meetings = LoadMeetingPage(connection, member, term, filter, from, to, page);

                    Guid selected;
                    bool samePage = Guid.TryParse(hfMeetingId.Value, out selected) && meetings.Any(m => m.Id == selected);
                    if (!samePage || action == "open" || action == "filter" || action == "page")
                        selected = meetings.Count > 0 ? meetings[0].Id : Guid.Empty;

                    hfMeetingPage.Value = page.ToString(CultureInfo.InvariantCulture);
                    hfMeetingId.Value = selected == Guid.Empty ? string.Empty : selected.ToString("D");

                    RenderMeetingList(meetings, total, page, selected);
                    if (selected != Guid.Empty)
                    {
                        CuocHopRow active = meetings.First(m => m.Id == selected);
                        RenderRoster(connection, active, rosterPage);
                    }
                    else
                    {
                        litRosterHeader.Text = "<div class='pmc-right-head'><h5>Chưa chọn cuộc họp</h5></div>";
                        litRoster.Text = "<div class='pmc-empty'>Không tìm thấy cuộc họp phù hợp.</div>";
                        litRosterPager.Text = string.Empty;
                    }
                }
            }
            catch (Exception exception)
            {
                SysLogger.LogError(exception, "Lỗi tải popup công cuộc họp tại DuAnDetail");
                SetEmpty("Không tải được dữ liệu cuộc họp. Vui lòng thử lại.");
            }

            upMeeting.Update();
            RegisterCallback(action == "open");
        }

        private void RegisterCallback(bool isOpen)
        {
            ScriptManager.RegisterStartupScript(upMeeting, upMeeting.GetType(), "pdMeetingLoaded", "if(window.QLDAProjectMeetingPopup){window.QLDAProjectMeetingPopup.received(" + (isOpen ? "true" : "false") + ");}", true);
        }

        private const string MeetingWhere = @" FROM dbo.TblLichHop m
WHERE m.IdDuAn=@ProjectId AND ISNULL(m.DaXoa,0)=0
AND (m.IdNguoiTao=@UserId OR EXISTS (SELECT 1 FROM dbo.TblLichHop_NhanVien nv WHERE nv.IdLichHop=m.IdLichHop AND nv.IdNhanVien=@UserId))
AND (@Search=N'' OR m.TenCuocHop LIKE @Pattern)
AND (@State='all' OR (@State='ended' AND m.ThoiGianKetThuc<=GETDATE()) OR (@State='active' AND m.ThoiGianKetThuc>GETDATE()))
AND (@DateFrom IS NULL OR m.ThoiGianBatDau>=@DateFrom)
AND (@DateTo IS NULL OR m.ThoiGianBatDau<DATEADD(day,1,@DateTo))";

        private void AddMeetingParameters(SqlCommand cmd, Guid member, string search, string filter, DateTime? from, DateTime? to)
        {
            cmd.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = ProjectId;
            cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = member;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 120).Value = search ?? string.Empty;
            cmd.Parameters.Add("@Pattern", SqlDbType.NVarChar, 140).Value = LikePattern(search ?? string.Empty);
            cmd.Parameters.Add("@State", SqlDbType.VarChar, 12).Value = filter ?? "all";
            SqlParameter pFrom = cmd.Parameters.Add("@DateFrom", SqlDbType.DateTime);
            pFrom.Value = (object)from ?? DBNull.Value;
            SqlParameter pTo = cmd.Parameters.Add("@DateTo", SqlDbType.DateTime);
            pTo.Value = (object)to ?? DBNull.Value;
        }

        private int CountMeetings(SqlConnection connection, Guid member, string search, string filter, DateTime? from, DateTime? to)
        {
            using (var command = new SqlCommand("SELECT COUNT(1)" + MeetingWhere + ";", connection))
            {
                AddMeetingParameters(command, member, search, filter, from, to);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private List<CuocHopRow> LoadMeetingPage(SqlConnection connection, Guid member, string search, string filter, DateTime? from, DateTime? to, int page)
        {
            string query = @"WITH DuLieu AS (
SELECT m.IdLichHop,m.TenCuocHop,m.ThoiGianBatDau,m.ThoiGianKetThuc,
CASE WHEN m.IdNguoiTao=@UserId THEN 1 ELSE 0 END AS LaChuTri,
ROW_NUMBER() OVER (ORDER BY CASE WHEN m.ThoiGianKetThuc>GETDATE() THEN 0 ELSE 1 END ASC,m.ThoiGianBatDau DESC,m.IdLichHop DESC) AS Stt" + MeetingWhere + @")
SELECT IdLichHop,TenCuocHop,ThoiGianBatDau,ThoiGianKetThuc,LaChuTri FROM DuLieu WHERE Stt>@Offset AND Stt<=@Limit ORDER BY Stt;";
            var rows = new List<CuocHopRow>();
            using (var command = new SqlCommand(query, connection))
            {
                AddMeetingParameters(command, member, search, filter, from, to);
                command.Parameters.Add("@Offset", SqlDbType.Int).Value = (page - 1) * SoCuocHopMoiTrang;
                command.Parameters.Add("@Limit", SqlDbType.Int).Value = page * SoCuocHopMoiTrang;
                using (SqlDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                        rows.Add(new CuocHopRow
                        {
                            Id = (Guid)reader["IdLichHop"],
                            Ten = Convert.ToString(reader["TenCuocHop"], CultureInfo.InvariantCulture),
                            BatDau = Convert.ToDateTime(reader["ThoiGianBatDau"], CultureInfo.InvariantCulture),
                            KetThuc = Convert.ToDateTime(reader["ThoiGianKetThuc"], CultureInfo.InvariantCulture),
                            LaChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) == 1
                        });
            }
            return rows;
        }

        private static string MeetingState(CuocHopRow m, DateTime now, out string cls)
        {
            if (m.KetThuc <= now) { cls = string.Empty; return "Đã kết thúc"; }
            if (m.BatDau <= now) { cls = " pmc-status-2"; return "Đang diễn ra"; }
            if (m.BatDau <= now.AddMinutes(15)) { cls = " pmc-status-1"; return "Sắp diễn ra"; }
            cls = " pmc-status-1"; return "Đã lên lịch";
        }

        private void RenderMeetingList(List<CuocHopRow> meetings, int count, int page, Guid selected)
        {
            litCount.Text = count.ToString(Vi);
            var sb = new StringBuilder();
            DateTime now = DateTime.Now;
            foreach (CuocHopRow meeting in meetings)
            {
                string cls;
                string state = MeetingState(meeting, now, out cls);
                decimal duration = Math.Max(0m, (decimal)(meeting.KetThuc - meeting.BatDau).TotalMinutes / 60m);
                sb.Append("<button type='button' class='pmc-meeting").Append(meeting.Id == selected ? " is-selected" : string.Empty)
                    .Append("' data-pmc-action='select' data-pmc-id='").Append(meeting.Id.ToString("D")).Append("'>")
                    .Append("<span class='pmc-meeting-top'><span class='pmc-meeting-name'>").Append(Enc(meeting.Ten)).Append("</span><span class='pmc-meeting-tags'>");
                if (meeting.LaChuTri)
                    sb.Append("<span class='pmc-host-chip' title='Bạn chủ trì'><i class='fas fa-crown'></i></span>");
                sb.Append("<span class='pmc-status").Append(cls).Append("'>").Append(state).Append("</span></span></span>")
                    .Append("<span class='pmc-meeting-meta'><span><i class='far fa-calendar'></i> ")
                    .Append(meeting.BatDau.ToString("dd/MM/yyyy", Vi)).Append("</span><span><i class='far fa-clock'></i> ")
                    .Append(meeting.BatDau.ToString("HH:mm", Vi)).Append(" – ").Append(meeting.KetThuc.ToString("HH:mm", Vi))
                    .Append(" (").Append(Gio(duration)).Append(")</span></span></button>");
            }
            litMeetings.Text = sb.Length > 0 ? sb.ToString() : "<div class='pmc-empty'>Không có cuộc họp phù hợp bộ lọc.</div>";
            litMeetingPager.Text = BuildPager(count, page, SoCuocHopMoiTrang, "meeting-page");
        }

        private const string RosterCte = @"WITH DanhSach AS (
SELECT DISTINCT p.IdNhanVien FROM dbo.TblLichHop_NhanVien p WHERE p.IdLichHop=@MeetingId AND p.IdNhanVien IS NOT NULL
UNION SELECT m.IdNguoiTao FROM dbo.TblLichHop m WHERE m.IdLichHop=@MeetingId AND m.IdNguoiTao IS NOT NULL
), TatCa AS (
SELECT d.IdNhanVien,
COALESCE(NULLIF(LTRIM(RTRIM(u.DisplayName)),N''),NULLIF(LTRIM(RTRIM(u.UserName)),N''),CONVERT(NVARCHAR(36),d.IdNhanVien)) AS TenNhanVien,
COALESCE(NULLIF(LTRIM(RTRIM(mu.Email)),N''),N'') AS Email,
COALESCE(NULLIF(LTRIM(RTRIM(u.Avatar)),N''),N'') AS Avatar,
CASE WHEN d.IdNhanVien=m.IdNguoiTao THEN 1 ELSE 0 END AS LaChuTri,
ISNULL(CAST(g.DaThamGiaOverride AS INT),ISNULL((SELECT MAX(CAST(ISNULL(nv.XacNhanThamGia,0) AS INT)) FROM dbo.TblLichHop_NhanVien nv WHERE nv.IdLichHop=@MeetingId AND nv.IdNhanVien=d.IdNhanVien),0)) AS XacNhanThamGia,
g.SoGioThucTe
FROM DanhSach d CROSS JOIN dbo.TblLichHop m
LEFT JOIN dbo.aspnet_Users u ON u.UserId=d.IdNhanVien
LEFT JOIN dbo.aspnet_Membership mu ON mu.UserId=d.IdNhanVien
LEFT JOIN dbo.TblLichHopGioCong g ON g.IdLichHop=@MeetingId AND g.IdNhanVien=d.IdNhanVien
WHERE m.IdLichHop=@MeetingId )";

        private static int CountRoster(SqlConnection connection, Guid meetingId)
        {
            using (var command = new SqlCommand(RosterCte + " SELECT COUNT(1) FROM TatCa;", connection))
            {
                command.Parameters.Add("@MeetingId", SqlDbType.UniqueIdentifier).Value = meetingId;
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static List<NhanVienRow> LoadRoster(SqlConnection connection, Guid meetingId, int page)
        {
            string sql = RosterCte + @", PhanTrang AS (
SELECT *,ROW_NUMBER() OVER (ORDER BY TenNhanVien,IdNhanVien) AS Stt FROM TatCa
)
SELECT IdNhanVien,TenNhanVien,Email,Avatar,LaChuTri,XacNhanThamGia,SoGioThucTe FROM PhanTrang
WHERE Stt>@Offset AND Stt<=@Limit ORDER BY Stt;";

            var rows = new List<NhanVienRow>();
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@MeetingId", SqlDbType.UniqueIdentifier).Value = meetingId;
                command.Parameters.Add("@Offset", SqlDbType.Int).Value = (page - 1) * SoNhanVienMoiTrang;
                command.Parameters.Add("@Limit", SqlDbType.Int).Value = page * SoNhanVienMoiTrang;
                using (SqlDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                        rows.Add(new NhanVienRow
                        {
                            IdNhanVien = reader["IdNhanVien"] == DBNull.Value ? Guid.Empty : (Guid)reader["IdNhanVien"],
                            Ten = Convert.ToString(reader["TenNhanVien"], CultureInfo.InvariantCulture),
                            Email = Convert.ToString(reader["Email"], CultureInfo.InvariantCulture),
                            Avatar = Convert.ToString(reader["Avatar"], CultureInfo.InvariantCulture),
                            ChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) == 1,
                            ThamGia = Convert.ToInt32(reader["XacNhanThamGia"], CultureInfo.InvariantCulture) == 1,
                            GioThucTe = reader["SoGioThucTe"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["SoGioThucTe"], CultureInfo.InvariantCulture)
                        });
            }
            return rows;
        }

        private string ResolveAvatarUrl(string avatar)
        {
            avatar = (avatar ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            if (avatar.StartsWith("~/", StringComparison.Ordinal)) return ResolveUrl(avatar);
            if (avatar.StartsWith("/", StringComparison.Ordinal) && !avatar.StartsWith("//", StringComparison.Ordinal)) return avatar;
            if (avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return avatar;
            if (!avatar.Contains(":") && !avatar.StartsWith("//", StringComparison.Ordinal)) return ResolveUrl("~/" + avatar.TrimStart('/'));
            return string.Empty;
        }

        private static string GetAvatarColor(Guid userId, string name)
        {
            string[] colors = { "#3b82f6", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#6366f1", "#ef4444", "#14b8a6" };
            int index;
            if (userId != Guid.Empty) index = userId.ToByteArray()[0] % colors.Length;
            else index = Math.Abs((name ?? string.Empty).GetHashCode()) % colors.Length;
            return colors[index];
        }

        private string BuildStaffAvatarHtml(NhanVienRow row)
        {
            string color = GetAvatarColor(row.IdNhanVien, row.Ten);
            string initials = Enc(GetInitials(row.Ten));
            string name = Attr(row.Ten);
            string url = ResolveAvatarUrl(row.Avatar);
            var sb = new StringBuilder();
            sb.Append("<span class='pmc-avatar' style='background:").Append(color).Append(";' title='").Append(name).Append("'>");
            if (!string.IsNullOrWhiteSpace(url))
            {
                sb.Append("<img src='").Append(Attr(url)).Append("' alt='").Append(name)
                    .Append("' loading='lazy' onerror=\"this.style.display='none';if(this.nextElementSibling){this.nextElementSibling.style.display='flex';}\" />");
            }
            sb.Append("<span class='pmc-avatar-fallback' style='")
                .Append(!string.IsNullOrWhiteSpace(url) ? "display:none;" : "display:flex;")
                .Append("background:").Append(color).Append(";'>").Append(initials).Append("</span></span>");
            return sb.ToString();
        }

        private void RenderRoster(SqlConnection conn, CuocHopRow meeting, int requestedPage)
        {
            int count = CountRoster(conn, meeting.Id);
            int page = Math.Min(requestedPage, Math.Max(1, (count + SoNhanVienMoiTrang - 1) / SoNhanVienMoiTrang));
            hfRosterPage.Value = page.ToString(CultureInfo.InvariantCulture);
            List<NhanVienRow> staff = LoadRoster(conn, meeting.Id, page);
            decimal duration = Math.Max(0m, (decimal)(meeting.KetThuc - meeting.BatDau).TotalMinutes / 60m);

            var h = new StringBuilder();
            h.Append("<div class='pmc-right-head'><div><h5>").Append(Enc(meeting.Ten)).Append("</h5><p>")
                .Append(meeting.BatDau.ToString("dd/MM/yyyy", Vi)).Append(" · ")
                .Append(meeting.BatDau.ToString("HH:mm", Vi)).Append(" – ").Append(meeting.KetThuc.ToString("HH:mm", Vi))
                .Append("</p></div><span class='pmc-duration'><i class='far fa-clock'></i> ")
                .Append(((decimal)(meeting.KetThuc - meeting.BatDau).TotalMinutes).ToString("0.##", Vi))
                .Append(" phút = ").Append(Gio(duration)).Append("</span></div>");
            litRosterHeader.Text = h.ToString();

            var sb = new StringBuilder();
            sb.Append("<table class='pmc-table'><thead><tr><th>Tên nhân viên</th><th>Trạng thái tham gia</th><th>Công lý thuyết</th><th>Công thực tế</th></tr></thead><tbody>");
            foreach (NhanVienRow row in staff)
            {
                decimal theoretical = row.ThamGia ? duration : 0m;
                string actual = row.GioThucTe.HasValue ? Gio(row.GioThucTe.Value) : Gio(row.ThamGia ? theoretical : 0m);
                sb.Append("<tr><td><div class='pmc-staff'>")
                    .Append(BuildStaffAvatarHtml(row))
                    .Append("<div class='pmc-staff-name'><b>").Append(Enc(row.Ten));
                if (row.ChuTri) sb.Append(" <i class='fas fa-crown pmc-host' title='Chủ trì'></i>");
                sb.Append("</b><small>").Append(Enc(string.IsNullOrWhiteSpace(row.Email) ? "Chưa cập nhật email" : row.Email))
                    .Append("</small></div></div></td><td><span class='").Append(row.ThamGia ? "pmc-yes'>Có tham gia" : "pmc-no'>Không tham gia")
                    .Append("</span></td><td class='pmc-hour'>").Append(Gio(theoretical))
                    .Append("</td><td class='pmc-hour'>").Append(Enc(actual)).Append("</td></tr>");
            }
            if (staff.Count == 0) sb.Append("<tr><td colspan='4' class='pmc-empty'>Chưa có nhân viên trong cuộc họp này.</td></tr>");
            sb.Append("</tbody></table>");
            litRoster.Text = sb.ToString();
            litRosterPager.Text = BuildPager(count, page, SoNhanVienMoiTrang, "roster-page");
        }

        private static string BuildPager(int total, int current, int size, string action)
        {
            if (total <= 0) return "<span>0 / 0</span>";
            int pages = Math.Max(1, (total + size - 1) / size);
            var sb = new StringBuilder();
            sb.Append("<button type='button' data-pmc-action='").Append(action).Append("' data-pmc-page='").Append(Math.Max(1, current - 1))
                .Append("' aria-label='Trang trước' ").Append(current <= 1 ? "disabled" : string.Empty).Append(">&#8249;</button>")
                .Append("<span>").Append((current - 1) * size + 1).Append("–").Append(Math.Min(total, current * size)).Append(" / ").Append(total).Append("</span>")
                .Append("<button type='button' data-pmc-action='").Append(action).Append("' data-pmc-page='").Append(Math.Min(pages, current + 1))
                .Append("' aria-label='Trang sau' ").Append(current >= pages ? "disabled" : string.Empty).Append(">&#8250;</button>");
            return sb.ToString();
        }
    }
}
