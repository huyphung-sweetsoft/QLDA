using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
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

        // Chi dang ky nut lam async trigger; moi yeu cau tim tam chi update danh sach trai.
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptManager sm = ScriptManager.GetCurrent(Page);
            if (sm != null) sm.RegisterAsyncPostBackControl(btnMeetingAction);
        }

        private System.Web.UI.UpdatePanel FindMeetingResultsPanel(Control parent)
        {
            if (parent == null) return null;
            if (parent.ID == "upMeetingResults") return parent as System.Web.UI.UpdatePanel;
            foreach (Control child in parent.Controls)
            {
                var found = FindMeetingResultsPanel(child);
                if (found != null) return found;
            }
            return null;
        }

        public sealed class CuocHopRow
        {
            public Guid Id { get; set; }
            public string Ten { get; set; }
            public DateTime BatDau { get; set; }
            public DateTime KetThuc { get; set; }
            public DateTime? BatDauThucTe { get; set; }
            public DateTime? KetThucThucTe { get; set; }
            public bool LaChuTri { get; set; }
            public string SelectedCss { get; set; }
            public string StatusCss { get; set; }
            public string StatusText { get; set; }
            public string DateText { get; set; }
            public string TimeText { get; set; }
        }

        public sealed class NhanVienRow
        {
            public Guid IdNhanVien { get; set; }
            public string Ten { get; set; }
            public string Email { get; set; }
            public string Avatar { get; set; }
            public bool ChuTri { get; set; }
            public bool ThamGia { get; set; }
            public decimal? GioThucTe { get; set; }
            public DateTime? GioVaoThucTe { get; set; }
            public DateTime? GioRaThucTe { get; set; }
            public string GioVaoText { get; set; }
            public string GioRaText { get; set; }
            public string LyDoGioThucTe { get; set; }
            public string AvatarUrl { get; set; }
            public string AvatarColor { get; set; }
            public string Initials { get; set; }
            public string EmailDisplay { get; set; }
            public string TheoreticalText { get; set; }
            public string ActualText { get; set; }
        }

        protected static string EncodeMeetingAttr(object value) { return HttpUtility.HtmlAttributeEncode(Convert.ToString(value) ?? string.Empty).Replace("'", "&#39;"); }
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

        // Các Repeaters/HTML controls khai báo trong .ascx; không ghép chuỗi HTML ở đây.
        // Tìm theo ID để không buộc dự án phải tạo lại .designer.cs khi merge.
        private static T Ui<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            if (root.ID == id) return root as T;
            foreach (Control child in root.Controls)
            {
                T found = Ui<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private void SetPager(string prefix, int total, int current, int size)
        {
            int pages = Math.Max(1, (total + size - 1) / size);
            var prev = Ui<HtmlButton>(upMeeting, prefix + "Prev");
            var next = Ui<HtmlButton>(upMeeting, prefix + "Next");
            var text = Ui<Literal>(upMeeting, "lit" + (prefix == "pmcMeeting" ? "Meeting" : "Roster") + "PageInfo");
            if (prev != null) { prev.Disabled = total == 0 || current <= 1; prev.Attributes["data-pmc-page"] = Math.Max(1, current - 1).ToString(Vi); }
            if (next != null) { next.Disabled = total == 0 || current >= pages; next.Attributes["data-pmc-page"] = Math.Min(pages, current + 1).ToString(Vi); }
            if (text != null) text.Text = total == 0 ? "0 / 0" : ((current - 1) * size + 1).ToString(Vi) + "–" + Math.Min(total, current * size).ToString(Vi) + " / " + total.ToString(Vi);
        }

        private void SetRosterEmpty(string title, string message)
        {
            var name = Ui<Literal>(upMeeting, "litMeetingName");
            if (name != null) name.Text = Enc(title);
            var time = Ui<Literal>(upMeeting, "litMeetingTimes");
            if (time != null) time.Text = string.Empty;
            var actualMeetingTime = Ui<Literal>(upMeeting, "litMeetingActualTimes");
            if (actualMeetingTime != null) actualMeetingTime.Text = "Chưa ghi nhận";
            var duration = Ui<Literal>(upMeeting, "litMeetingDuration");
            if (duration != null) duration.Text = string.Empty;
            var roster = Ui<Repeater>(upMeeting, "rptRoster");
            if (roster != null) { roster.DataSource = new NhanVienRow[0]; roster.DataBind(); }
            var empty = Ui<PlaceHolder>(upMeeting, "phRosterEmpty");
            if (empty != null) empty.Visible = true;
            var msg = Ui<Literal>(upMeeting, "litRosterEmptyMessage");
            if (msg != null) msg.Text = Enc(message);
            SetPager("pmcRoster", 0, 1, SoNhanVienMoiTrang);
        }

        private void SetEmpty(string message)
        {
            litCount.Text = "0";
            RenderMeetingList(new List<CuocHopRow>(), 0, 1, Guid.Empty);
            var empty = Ui<HtmlGenericControl>(upMeeting, "pmcMeetingsEmpty");
            if (empty != null) empty.InnerText = message;
            SetRosterEmpty("Chi tiết giờ công", "Không có dữ liệu hiển thị.");
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
            Guid requestedMeeting = Guid.Empty;
            if (action == "open") Guid.TryParse(hfMeetingId.Value, out requestedMeeting);
            if (action == "open" || action == "filter") { page = 1; rosterPage = 1; }
            if (action == "page" || action == "select") rosterPage = 1;

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    int total = CountMeetings(connection, member, term, filter, from, to);
                    if (action == "open" && requestedMeeting != Guid.Empty)
                        page = FindMeetingPage(connection, member, term, filter, from, to, requestedMeeting);
                    int pages = Math.Max(1, (total + SoCuocHopMoiTrang - 1) / SoCuocHopMoiTrang);
                    page = Math.Min(page, pages);
                    List<CuocHopRow> meetings = LoadMeetingPage(connection, member, term, filter, from, to, page);

                    if (action == "search-preview")
                    {
                        // Chi thay danh sach va phan trang ben trai. Khong doi cuoc hop dang xem,
                        // khong reload bang phai va khong thay textbox khi dang go.
                        Guid currentSelection;
                        if (!Guid.TryParse(hfMeetingId.Value, out currentSelection)) currentSelection = Guid.Empty;
                        hfMeetingPage.Value = page.ToString(CultureInfo.InvariantCulture);
                        RenderMeetingList(meetings, total, page, currentSelection);
                        System.Web.UI.UpdatePanel results = FindMeetingResultsPanel(upMeeting);
                        if (results != null) results.Update();
                        else upMeeting.Update();
                        RegisterCallback(false);
                        return;
                    }

                    Guid selected;
                    bool samePage = Guid.TryParse(hfMeetingId.Value, out selected) && meetings.Any(m => m.Id == selected);
                    if (action == "open" && requestedMeeting != Guid.Empty && meetings.Any(m => m.Id == requestedMeeting))
                        selected = requestedMeeting;
                    else if (!samePage || action == "open" || action == "filter" || action == "page")
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
                        SetRosterEmpty("Chưa chọn cuộc họp", "Không tìm thấy cuộc họp phù hợp.");
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
            // Page overload: callback van duoc gui khi chi UpdatePanel con (list) render.
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "pdMeetingLoaded", "if(window.QLDAProjectMeetingPopup){window.QLDAProjectMeetingPopup.received(" + (isOpen ? "true" : "false") + ");}", true);
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

        // Chỉ tìm trang chứa cuộc họp trong tập đã kiểm tra quyền @ProjectId + @UserId.
        private int FindMeetingPage(SqlConnection connection, Guid member, string search, string filter, DateTime? from, DateTime? to, Guid meetingId)
        {
            if (meetingId == Guid.Empty) return 1;
            string sql = @"WITH TargetList AS (
SELECT m.IdLichHop,ROW_NUMBER() OVER (ORDER BY CASE WHEN m.ThoiGianKetThuc>GETDATE() THEN 0 ELSE 1 END ASC,m.ThoiGianBatDau DESC,m.IdLichHop DESC) AS Stt"
                + MeetingWhere + @") SELECT Stt FROM TargetList WHERE IdLichHop=@TargetMeeting;";
            using (var command = new SqlCommand(sql, connection))
            {
                AddMeetingParameters(command, member, search, filter, from, to);
                command.Parameters.Add("@TargetMeeting", SqlDbType.UniqueIdentifier).Value = meetingId;
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 1
                    : Math.Max(1, (Convert.ToInt32(value, CultureInfo.InvariantCulture) - 1) / SoCuocHopMoiTrang + 1);
            }
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
SELECT m.IdLichHop,m.TenCuocHop,m.ThoiGianBatDau,m.ThoiGianKetThuc,m.ThoiGianBatDauThucTe,m.ThoiGianKetThucThucTe,
CASE WHEN m.IdNguoiTao=@UserId THEN 1 ELSE 0 END AS LaChuTri,
ROW_NUMBER() OVER (ORDER BY CASE WHEN m.ThoiGianKetThuc>GETDATE() THEN 0 ELSE 1 END ASC,m.ThoiGianBatDau DESC,m.IdLichHop DESC) AS Stt" + MeetingWhere + @")
SELECT IdLichHop,TenCuocHop,ThoiGianBatDau,ThoiGianKetThuc,ThoiGianBatDauThucTe,ThoiGianKetThucThucTe,LaChuTri FROM DuLieu WHERE Stt>@Offset AND Stt<=@Limit ORDER BY Stt;";
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
                            BatDauThucTe = reader["ThoiGianBatDauThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ThoiGianBatDauThucTe"], CultureInfo.InvariantCulture),
                            KetThucThucTe = reader["ThoiGianKetThucThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ThoiGianKetThucThucTe"], CultureInfo.InvariantCulture),
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
            DateTime now = DateTime.Now;
            foreach (CuocHopRow meeting in meetings)
            {
                string cls;
                meeting.StatusText = MeetingState(meeting, now, out cls);
                meeting.StatusCss = cls;
                meeting.SelectedCss = meeting.Id == selected ? " is-selected" : string.Empty;
                meeting.DateText = meeting.BatDau.ToString("dd/MM/yyyy", Vi);
                decimal hours = Math.Max(0m, (decimal)(meeting.KetThuc - meeting.BatDau).TotalMinutes / 60m);
                meeting.TimeText = meeting.BatDau.ToString("HH:mm", Vi) + " – " + meeting.KetThuc.ToString("HH:mm", Vi) + " (" + Gio(hours) + ")";
            }
            Repeater list = Ui<Repeater>(upMeeting, "rptMeetings");
            if (list != null) { list.DataSource = meetings; list.DataBind(); }
            var empty = Ui<HtmlGenericControl>(upMeeting, "pmcMeetingsEmpty");
            if (empty != null) { empty.Visible = meetings.Count == 0; empty.InnerText = "Không có cuộc họp phù hợp bộ lọc."; }
            SetPager("pmcMeeting", count, page, SoCuocHopMoiTrang);
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
ISNULL(CAST(g.DaThamGia AS INT),ISNULL((SELECT MAX(CAST(ISNULL(nv.XacNhanThamGia,0) AS INT)) FROM dbo.TblLichHop_NhanVien nv WHERE nv.IdLichHop=@MeetingId AND nv.IdNhanVien=d.IdNhanVien),0)) AS XacNhanThamGia,
g.SoGioThucTe,g.GioVaoThucTe,g.GioRaThucTe
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
SELECT IdNhanVien,TenNhanVien,Email,Avatar,LaChuTri,XacNhanThamGia,SoGioThucTe,GioVaoThucTe,GioRaThucTe FROM PhanTrang
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
                            GioThucTe = reader["SoGioThucTe"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["SoGioThucTe"], CultureInfo.InvariantCulture),
                            GioVaoThucTe = reader["GioVaoThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["GioVaoThucTe"], CultureInfo.InvariantCulture),
                            GioRaThucTe = reader["GioRaThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["GioRaThucTe"], CultureInfo.InvariantCulture)
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

        private void RenderRoster(SqlConnection conn, CuocHopRow meeting, int requestedPage)
        {
            int count = CountRoster(conn, meeting.Id);
            int page = Math.Min(requestedPage, Math.Max(1, (count + SoNhanVienMoiTrang - 1) / SoNhanVienMoiTrang));
            hfRosterPage.Value = page.ToString(CultureInfo.InvariantCulture);
            List<NhanVienRow> staff = LoadRoster(conn, meeting.Id, page);
            // Cung quy tac EffectiveMeetTimes cua CtrlMeetGioCongTab:
            // chi dung gio hop thuc te khi ca hai moc co gia tri hop le.
            bool validActualMeeting = meeting.BatDauThucTe.HasValue && meeting.KetThucThucTe.HasValue
                && meeting.KetThucThucTe.Value > meeting.BatDauThucTe.Value;
            DateTime effectiveStart = validActualMeeting ? meeting.BatDauThucTe.Value : meeting.BatDau;
            DateTime effectiveEnd = validActualMeeting ? meeting.KetThucThucTe.Value : meeting.KetThuc;
            decimal duration = Math.Round(Math.Max(0m, (decimal)(effectiveEnd - effectiveStart).TotalMinutes / 60m), 2, MidpointRounding.AwayFromZero);
            var name = Ui<Literal>(upMeeting, "litMeetingName");
            if (name != null) name.Text = Enc(meeting.Ten);
            var times = Ui<Literal>(upMeeting, "litMeetingTimes");
            if (times != null) times.Text = meeting.BatDau.ToString("dd/MM/yyyy", Vi) + " · " + meeting.BatDau.ToString("HH:mm", Vi) + " – " + meeting.KetThuc.ToString("HH:mm", Vi);
            var actualMeetingTimes = Ui<Literal>(upMeeting, "litMeetingActualTimes");
            if (actualMeetingTimes != null)
            {
                actualMeetingTimes.Text = meeting.BatDauThucTe.HasValue && meeting.KetThucThucTe.HasValue && meeting.KetThucThucTe.Value > meeting.BatDauThucTe.Value
                    ? meeting.BatDauThucTe.Value.ToString("dd/MM/yyyy HH:mm", Vi) + " – " + meeting.KetThucThucTe.Value.ToString("dd/MM/yyyy HH:mm", Vi)
                    : "Chưa ghi nhận";
            }
            var hours = Ui<Literal>(upMeeting, "litMeetingDuration");
            if (hours != null) hours.Text = ((decimal)(effectiveEnd - effectiveStart).TotalMinutes).ToString("0.##", Vi) + " phút = " + Gio(duration);
            foreach (NhanVienRow row in staff)
            {
                row.AvatarColor = GetAvatarColor(row.IdNhanVien, row.Ten);
                row.AvatarUrl = ResolveAvatarUrl(row.Avatar);
                row.Initials = GetInitials(row.Ten);
                row.EmailDisplay = string.IsNullOrWhiteSpace(row.Email) ? "Chưa cập nhật email" : row.Email;
                decimal theoretical = row.ThamGia ? duration : 0m;
                row.TheoreticalText = Gio(theoretical);
                bool hasValidClock = row.GioVaoThucTe.HasValue && row.GioRaThucTe.HasValue && row.GioRaThucTe.Value > row.GioVaoThucTe.Value;
                // Dong bo cach xem gio voi CtrlMeetGioCongTab: uu tien cap moc vao/ra hop le.
                decimal actual = !row.ThamGia ? 0m : hasValidClock
                    ? Math.Round((decimal)(row.GioRaThucTe.Value - row.GioVaoThucTe.Value).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero)
                    : (row.GioThucTe ?? theoretical);
                row.ActualText = Gio(actual);
                row.GioVaoText = row.GioVaoThucTe.HasValue ? row.GioVaoThucTe.Value.ToString("dd/MM/yyyy HH:mm", Vi) : "Chưa ghi nhận";
                row.GioRaText = row.GioRaThucTe.HasValue ? row.GioRaThucTe.Value.ToString("dd/MM/yyyy HH:mm", Vi) : "Chưa ghi nhận";
                if (!row.ThamGia) row.LyDoGioThucTe = "Nhân viên không tham gia cuộc họp nên không được ghi nhận giờ công.";
                else if (hasValidClock) row.LyDoGioThucTe = "Công thực tế được tính từ thời điểm vào và ra thực tế của nhân viên.";
                else if (row.GioThucTe.HasValue) row.LyDoGioThucTe = "Công thực tế đã được ghi nhận hoặc điều chỉnh trực tiếp; chưa có đủ mốc giờ vào/ra để đối chiếu.";
                else row.LyDoGioThucTe = "Chưa có giờ vào/ra hoặc công điều chỉnh riêng; tạm hiển thị công theo thời lượng cuộc họp.";
            }
            var roster = Ui<Repeater>(upMeeting, "rptRoster");
            if (roster != null) { roster.DataSource = staff; roster.DataBind(); }
            var empty = Ui<PlaceHolder>(upMeeting, "phRosterEmpty");
            if (empty != null) empty.Visible = staff.Count == 0;
            SetPager("pmcRoster", count, page, SoNhanVienMoiTrang);
        }
    }
}
