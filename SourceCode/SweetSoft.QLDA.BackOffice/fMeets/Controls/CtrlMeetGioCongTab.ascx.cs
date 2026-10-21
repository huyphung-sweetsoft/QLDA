using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlMeetGioCongTab : BaseAdminUserControl
    {
        private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");
        private bool _canEditLabor;
        private bool? _cachedCanEdit;

        // CtrlMeet.ascx.cs đang truyền hai thuộc tính này từ control cha.
        public Guid ProjectId
        {
            get
            {
                if (ViewState["GioCongProjectId"] is Guid)
                    return (Guid)ViewState["GioCongProjectId"];
                BaseAdminPage page = Page as BaseAdminPage;
                return page == null ? Guid.Empty : page.CurrentProjectId;
            }
            set { ViewState["GioCongProjectId"] = value; }
        }

        public bool CanViewLabor
        {
            get { return ViewState["GioCongCanView"] is bool && (bool)ViewState["GioCongCanView"]; }
            set { ViewState["GioCongCanView"] = value; }
        }

        public string LoadButtonClientId
        {
            get { return btnLoadLaborTab.ClientID; }
        }

        protected bool LaborLoaded
        {
            get { return ViewState["GioCongLoaded"] is bool && (bool)ViewState["GioCongLoaded"]; }
            set { ViewState["GioCongLoaded"] = value; }
        }

        // Ba chế độ: all (tất cả), active (chưa kết thúc), ended (đã kết thúc).
        // Mặc định hiện tất cả cuộc họp; chỉ cuộc họp đã kết thúc mới cho sửa giờ công.
        private string MeetingFilter
        {
            get
            {
                string mode = Convert.ToString(ViewState["GioCongMeetingFilter"], CultureInfo.InvariantCulture);
                return mode == "active" || mode == "ended" ? mode : "all";
            }
            set { ViewState["GioCongMeetingFilter"] = value; }
        }

        private const int MeetingsPerPage = 5;

        private int MeetingPage
        {
            get { return ViewState["GioCongMeetingPage"] is int && (int)ViewState["GioCongMeetingPage"] > 0 ? (int)ViewState["GioCongMeetingPage"] : 1; }
            set { ViewState["GioCongMeetingPage"] = Math.Max(1, value); }
        }

        private Guid SelectedMeetingId
        {
            get { return ViewState["GioCongSelectedMeeting"] is Guid ? (Guid)ViewState["GioCongSelectedMeeting"] : Guid.Empty; }
            set { ViewState["GioCongSelectedMeeting"] = value; }
        }

        protected bool CanEditLabor
        {
            get
            {
                if (!_cachedCanEdit.HasValue)
                    _cachedCanEdit = CanViewLabor && IsPM(ProjectId);
                return _cachedCanEdit.Value;
            }
        }

        protected string CurrentDurationInvariant
        {
            get { return Convert.ToString(ViewState["GioCongDuration"], CultureInfo.InvariantCulture) ?? "0"; }
        }

        // Tra cuu control theo ID thay vi phu thuoc vao designer.cs cu.
        // Phu hop voi control nam ben trong UpdatePanel/ContentTemplate.
        private static Control FindChildControl(Control parent, string id)
        {
            if (parent == null) return null;
            if (string.Equals(parent.ID, id, StringComparison.Ordinal)) return parent;
            foreach (Control child in parent.Controls)
            {
                Control result = FindChildControl(child, id);
                if (result != null) return result;
            }
            return null;
        }

        private HtmlGenericControl LaborRootElement
        {
            get { return FindChildControl(this, "laborRoot") as HtmlGenericControl; }
        }

        private Label TabErrorLabel
        {
            get { return FindChildControl(this, "lblTabError") as Label; }
        }

        private Label LaborErrorLabel
        {
            get { return FindChildControl(this, "lblLaborError") as Label; }
        }

        private Literal MeetingPagerLiteral
        {
            get { return FindChildControl(this, "litMeetingPager") as Literal; }
        }

        private HiddenField SaveResultField
        {
            get { return FindChildControl(this, "hfSaveResult") as HiddenField; }
        }

        private Panel LockedMeetingPanel
        {
            get { return FindChildControl(this, "pnlLockedMeeting") as Panel; }
        }

        private Literal LockedMeetingName
        {
            get { return FindChildControl(this, "litLockedMeetingName") as Literal; }
        }

        private void SetTabError(string message)
        {
            Label label = TabErrorLabel;
            if (label != null)
            {
                label.Text = message;
                label.Visible = !string.IsNullOrEmpty(message);
            }
        }

        private void SetLaborError(string message)
        {
            Label label = LaborErrorLabel;
            if (label != null) label.Text = message;
        }

        private sealed class MeetingRow
        {
            public Guid IdLichHop { get; set; }
            public string TenCuocHop { get; set; }
            public DateTime ThoiGianBatDau { get; set; }
            public DateTime ThoiGianKetThuc { get; set; }
        }

        public sealed class GioCongNhanVienRow
        {
            public Guid IdNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public bool LaChuTri { get; set; }
            public bool XacNhanThamGia { get; set; }
            public decimal SoGioLyThuyet { get; set; }
            public decimal? SoGioThucTe { get; set; }
        }

        private static string GetConnectionString()
        {
            ConnectionStringSettings named = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (named != null && !string.IsNullOrWhiteSpace(named.ConnectionString)) return named.ConnectionString;
            foreach (ConnectionStringSettings entry in ConfigurationManager.ConnectionStrings)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ConnectionString) &&
                    entry.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    return entry.ConnectionString;
            }
            throw new ConfigurationErrorsException("Không tìm thấy chuỗi kết nối SweetSoft_QLDA.");
        }

        private static SqlParameter GuidParameter(string name, Guid id)
        {
            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = id };
        }

        private bool IsPM(Guid projectId)
        {
            if (projectId == Guid.Empty || SweetContext.Current.UserId == Guid.Empty) return false;
            Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            return pm.HasValue && pm.Value == SweetContext.Current.UserId;
        }

        private static bool IsValidMeeting(Guid meetId, Guid projectId, out TblLichHop meeting)
        {
            meeting = meetId == Guid.Empty ? null : TblLichHop.FetchByID(meetId);
            return meeting != null && meeting.DaXoa != true && meeting.IdDuAn == projectId;
        }

        private static decimal DurationHours(TblLichHop meeting)
        {
            TimeSpan span = meeting.ThoiGianKetThuc - meeting.ThoiGianBatDau;
            return span.TotalMinutes <= 0 ? 0m : Math.Round((decimal)span.TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
        }

        private static int DurationMinutes(TblLichHop meeting)
        {
            TimeSpan span = meeting.ThoiGianKetThuc - meeting.ThoiGianBatDau;
            return span.TotalMinutes <= 0 ? 0 : (int)Math.Round(span.TotalMinutes, MidpointRounding.AwayFromZero);
        }

        private const string RosterSql = @"
WITH DanhSach AS (
    SELECT DISTINCT p.IdNhanVien
    FROM dbo.TblLichHop_NhanVien p
    WHERE p.IdLichHop = @IdLichHop AND p.IdNhanVien IS NOT NULL
    UNION
    SELECT m.IdNguoiTao
    FROM dbo.TblLichHop m
    WHERE m.IdLichHop = @IdLichHop AND m.IdNguoiTao IS NOT NULL
)
SELECT d.IdNhanVien,
       COALESCE(NULLIF(LTRIM(RTRIM(u.DisplayName)), N''),
                NULLIF(LTRIM(RTRIM(u.UserName)), N''),
                CONVERT(NVARCHAR(36), d.IdNhanVien)) AS TenNhanVien,
       CASE WHEN d.IdNhanVien = m.IdNguoiTao THEN 1 ELSE 0 END AS LaChuTri,
       ISNULL(CAST(g.DaThamGiaOverride AS INT),
              ISNULL((SELECT MAX(CAST(ISNULL(p.XacNhanThamGia, 0) AS INT))
                      FROM dbo.TblLichHop_NhanVien p
                      WHERE p.IdLichHop = @IdLichHop AND p.IdNhanVien = d.IdNhanVien), 0)) AS XacNhanThamGia,
       g.SoGioThucTe
FROM DanhSach d
CROSS JOIN dbo.TblLichHop m
LEFT JOIN dbo.aspnet_Users u ON u.UserId = d.IdNhanVien
LEFT JOIN dbo.TblLichHopGioCong g ON g.IdLichHop = @IdLichHop AND g.IdNhanVien = d.IdNhanVien
WHERE m.IdLichHop = @IdLichHop
ORDER BY TenNhanVien, d.IdNhanVien;";

        private static List<GioCongNhanVienRow> LoadRoster(SqlConnection connection, Guid meetingId, decimal duration)
        {
            List<GioCongNhanVienRow> results = new List<GioCongNhanVienRow>();
            using (SqlCommand command = new SqlCommand(RosterSql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool joined = Convert.ToInt32(reader["XacNhanThamGia"], CultureInfo.InvariantCulture) == 1;
                        decimal? actual = reader["SoGioThucTe"] == DBNull.Value
                            ? (decimal?)null : Convert.ToDecimal(reader["SoGioThucTe"], CultureInfo.InvariantCulture);
                        if (!joined) actual = 0m;
                        results.Add(new GioCongNhanVienRow
                        {
                            IdNhanVien = (Guid)reader["IdNhanVien"],
                            TenNhanVien = Convert.ToString(reader["TenNhanVien"], CultureInfo.InvariantCulture),
                            LaChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) == 1,
                            XacNhanThamGia = joined,
                            SoGioLyThuyet = joined ? duration : 0m,
                            SoGioThucTe = actual
                        });
                    }
                }
            }
            return results;
        }

        // Phân trang tại SQL Server: không tải cả danh sách lên rồi cắt ở client.
        // ROW_NUMBER tương thích các phiên bản SQL Server cũ hơn OFFSET/FETCH.
        private static List<MeetingRow> LoadMeetingPage(SqlConnection connection, Guid projectId,
            string mode, int requestedPage, out int total, out int actualPage)
        {
            const string where = @" FROM dbo.TblLichHop
WHERE IdDuAn = @IdDuAn AND (DaXoa = 0 OR DaXoa IS NULL)
  AND (@Filter = 0
       OR (@Filter = 1 AND ThoiGianKetThuc > GETDATE())
       OR (@Filter = 2 AND ThoiGianKetThuc <= GETDATE()))";

            int code = mode == "all" ? 0 : mode == "ended" ? 2 : 1;
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1)" + where, connection))
            {
                command.Parameters.Add(GuidParameter("@IdDuAn", projectId));
                command.Parameters.Add("@Filter", SqlDbType.Int).Value = code;
                total = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }

            int pageCount = Math.Max(1, (total + MeetingsPerPage - 1) / MeetingsPerPage);
            actualPage = Math.Max(1, Math.Min(requestedPage, pageCount));
            List<MeetingRow> results = new List<MeetingRow>();
            if (total == 0) return results;

            string sql = @"
WITH DanhSachCoThuTu AS (
 SELECT IdLichHop, TenCuocHop, ThoiGianBatDau, ThoiGianKetThuc,
        ROW_NUMBER() OVER (ORDER BY ThoiGianBatDau DESC, IdLichHop DESC) AS SoThuTu" + where + @"
)
SELECT IdLichHop, TenCuocHop, ThoiGianBatDau, ThoiGianKetThuc
FROM DanhSachCoThuTu
WHERE SoThuTu > @Offset AND SoThuTu <= @Last
ORDER BY SoThuTu ASC;";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdDuAn", projectId));
                command.Parameters.Add("@Filter", SqlDbType.Int).Value = code;
                int skip = (actualPage - 1) * MeetingsPerPage;
                command.Parameters.Add("@Offset", SqlDbType.Int).Value = skip;
                command.Parameters.Add("@Last", SqlDbType.Int).Value = skip + MeetingsPerPage;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new MeetingRow
                        {
                            IdLichHop = (Guid)reader["IdLichHop"],
                            TenCuocHop = Convert.ToString(reader["TenCuocHop"], CultureInfo.InvariantCulture),
                            ThoiGianBatDau = Convert.ToDateTime(reader["ThoiGianBatDau"], CultureInfo.InvariantCulture),
                            ThoiGianKetThuc = Convert.ToDateTime(reader["ThoiGianKetThuc"], CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            return results;
        }

        private static string PagerButton(int page, string innerHtml, string label,
            bool disabled, bool active)
        {
            if (disabled || active)
                return "<button type=\"button\" class=\"mlt-page-button" + (active ? " is-active" : "") +
                    "\" aria-label=\"" + HttpUtility.HtmlAttributeEncode(label) + "\"" +
                    (active ? " aria-current=\"page\"" : "") + " disabled>" + innerHtml + "</button>";
            return "<button type=\"button\" class=\"mlt-page-button\" aria-label=\"" +
                HttpUtility.HtmlAttributeEncode(label) + "\" onclick=\"return QLDAMeetingLaborTab.changePage(" +
                page.ToString(CultureInfo.InvariantCulture) + ");\">" + innerHtml + "</button>";
        }

        private static string BuildPagerHtml(int total, int currentPage)
        {
            if (total == 0)
                return "<span class=\"mlt-page-info\">0 cuộc họp</span>";

            int pages = (total + MeetingsPerPage - 1) / MeetingsPerPage;
            int first = (currentPage - 1) * MeetingsPerPage + 1;
            int last = Math.Min(total, currentPage * MeetingsPerPage);
            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<span class=\"mlt-page-info\">")
                .Append(first).Append("–").Append(last).Append(" / ").Append(total)
                .Append("</span><div class=\"mlt-page-controls\">");
            html.Append(PagerButton(currentPage - 1, "&#8249;", "Trang trước", currentPage == 1, false));

            // Số trang xung quanh trang hiện tại, kèm trang đầu/cuối nếu danh sách dài.
            System.Collections.Generic.SortedSet<int> pageNumbers = new System.Collections.Generic.SortedSet<int>();
            pageNumbers.Add(1);
            pageNumbers.Add(pages);
            for (int p = Math.Max(1, currentPage - 1); p <= Math.Min(pages, currentPage + 1); p++)
                pageNumbers.Add(p);
            int prev = 0;
            foreach (int p in pageNumbers)
            {
                if (prev > 0 && p - prev > 1)
                    html.Append("<span class=\"mlt-page-ellipsis\" aria-hidden=\"true\">…</span>");
                html.Append(PagerButton(p, p.ToString(CultureInfo.InvariantCulture), "Trang " + p, false, p == currentPage));
                prev = p;
            }
            html.Append(PagerButton(currentPage + 1, "&#8250;", "Trang sau", currentPage >= pages, false));
            html.Append("</div>");
            return html.ToString();
        }

        protected string MeetingLinkCss(object id)
        {
            Guid meetingId;
            return Guid.TryParse(Convert.ToString(id, CultureInfo.InvariantCulture), out meetingId) && meetingId == SelectedMeetingId
                ? "mlt-meeting-link is-active" : "mlt-meeting-link";
        }

        protected string MeetingTimeText(object start, object end)
        {
            DateTime begin = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            DateTime finish = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            return begin.ToString("dd/MM/yyyy HH:mm", ViCulture) + " – " + finish.ToString("HH:mm", ViCulture);
        }

        protected string MeetingEndedBadge(object end)
        {
            DateTime finish = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            return finish <= DateTime.Now ? "<span class=\"mlt-done\">Đã kết thúc</span>" : string.Empty;
        }

        // Dùng nút tải GIỮ NGUYÊN ClientID để chọn cuộc họp bất kể Repeater bị DataBind.
        // Các nút lọc/lưu cũng cố định, được đăng ký async trên mọi request.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager manager = ScriptManager.GetCurrent(Page);
            if (manager == null || !manager.SupportsPartialRendering) return;

            manager.RegisterAsyncPostBackControl(btnLoadLaborTab);
            manager.RegisterAsyncPostBackControl(btnFilterAll);
            manager.RegisterAsyncPostBackControl(btnFilterActive);
            manager.RegisterAsyncPostBackControl(btnFilterEnded);
            manager.RegisterAsyncPostBackControl(btnSaveLabor);
        }

        // Được nút ẩn bên CtrlMeet.ascx gọi khi mở tab Giờ công.
        protected void btnLoadLaborTab_Click(object sender, EventArgs e)
        {
            if (!CanViewLabor || ProjectId == Guid.Empty)
            {
                ShowNotify("Bạn không có quyền xem giờ công cuộc họp của dự án.", MSGType.Error);
                return;
            }
            // The hidden client-side selection field is populated by selectMeeting() above.
            // Native LinkButton clicks are intercepted and forwarded through this stable async event.
            int page;
            string requestedPage = Request.Form["QLDA_MeetLabor_Page"];
            if (!string.IsNullOrWhiteSpace(requestedPage) && int.TryParse(requestedPage, out page) && page > 0)
            {
                MeetingPage = page;
                SelectedMeetingId = Guid.Empty;
                BindTab();
                return;
            }
            Guid requestedId;
            string requested = Request.Form["QLDA_MeetLabor_SelectId"];
            if (!string.IsNullOrWhiteSpace(requested) && Guid.TryParse(requested, out requestedId))
            {
                TblLichHop requestedMeeting;
                if (IsValidMeeting(requestedId, ProjectId, out requestedMeeting) &&
                    (MeetingFilter == "all" ||
                    (MeetingFilter == "ended" && requestedMeeting.ThoiGianKetThuc <= DateTime.Now) ||
                    (MeetingFilter == "active" && requestedMeeting.ThoiGianKetThuc > DateTime.Now)))
                    SelectedMeetingId = requestedId;
            }
            BindTab();
        }

        protected void btnFilterMeetings_Click(object sender, EventArgs e)
        {
            if (!CanViewLabor || ProjectId == Guid.Empty) return;
            LinkButton button = sender as LinkButton;
            string mode = button == null ? string.Empty : button.CommandArgument;
            if (mode != "all" && mode != "active" && mode != "ended") return;
            if (MeetingFilter == mode && LaborLoaded) return;
            MeetingFilter = mode;
            MeetingPage = 1;
            SelectedMeetingId = Guid.Empty;
            BindTab();
        }

        protected void rptMeetings_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "select", StringComparison.OrdinalIgnoreCase) ||
                !CanViewLabor || ProjectId == Guid.Empty) return;
            Guid meetingId;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out meetingId)) return;
            TblLichHop meeting;
            if (!IsValidMeeting(meetingId, ProjectId, out meeting)) return;
            if (MeetingFilter == "active" && meeting.ThoiGianKetThuc <= DateTime.Now) return;
            if (MeetingFilter == "ended" && meeting.ThoiGianKetThuc > DateTime.Now) return;
            SelectedMeetingId = meetingId;
            BindTab();
        }

        private void BindTab()
        {
            try
            {
                List<MeetingRow> meetings;
                int total, actualPage;
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    meetings = LoadMeetingPage(connection, ProjectId, MeetingFilter, MeetingPage,
                        out total, out actualPage);
                }
                MeetingPage = actualPage;

                Guid selected = SelectedMeetingId;
                if (!meetings.Any(m => m.IdLichHop == selected))
                    selected = meetings.Count == 0 ? Guid.Empty : meetings[0].IdLichHop;
                SelectedMeetingId = selected;

                btnFilterAll.CssClass = MeetingFilter == "all" ? "is-filtered" : string.Empty;
                btnFilterActive.CssClass = MeetingFilter == "active" ? "is-filtered" : string.Empty;
                btnFilterEnded.CssClass = MeetingFilter == "ended" ? "is-filtered" : string.Empty;
                btnFilterAll.Attributes["aria-pressed"] = MeetingFilter == "all" ? "true" : "false";
                btnFilterActive.Attributes["aria-pressed"] = MeetingFilter == "active" ? "true" : "false";
                btnFilterEnded.Attributes["aria-pressed"] = MeetingFilter == "ended" ? "true" : "false";
                litMeetingCount.Text = total.ToString(CultureInfo.InvariantCulture);
                Literal pager = MeetingPagerLiteral;
                if (pager != null) pager.Text = BuildPagerHtml(total, actualPage);
                pnlNoMeetings.Visible = total == 0;
                litNoMeetings.Text = MeetingFilter == "all" ? "Chưa có cuộc họp nào trong dự án."
                    : MeetingFilter == "ended" ? "Chưa có cuộc họp đã kết thúc."
                    : "Chưa có cuộc họp chưa kết thúc.";
                rptMeetings.DataSource = meetings;
                rptMeetings.DataBind();

                if (selected == Guid.Empty)
                {
                    if (LockedMeetingPanel != null) LockedMeetingPanel.Visible = false;
                    pnlSelectedMeeting.Visible = false;
                    pnlNoSelected.Visible = true;
                    rptLabor.DataSource = null;
                    rptLabor.DataBind();
                }
                else
                {
                    BindSelectedMeeting(selected);
                }
                LaborLoaded = true;
                if (LaborRootElement != null) LaborRootElement.Attributes["data-loaded"] = "1";
                upLaborTab.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CtrlMeetGioCongTab.BindTab: " + ex);
                SetTabError("Không thể tải danh sách hoặc giờ công cuộc họp. Kiểm tra bảng SQL TblLichHopGioCong và log máy chủ.");
                LaborLoaded = false;
                if (LaborRootElement != null) LaborRootElement.Attributes["data-loaded"] = "0";
                upLaborTab.Update();
            }
        }

        private void BindSelectedMeeting(Guid meetingId)
        {
            TblLichHop meeting;
            if (!IsValidMeeting(meetingId, ProjectId, out meeting))
            {
                SelectedMeetingId = Guid.Empty;
                if (LockedMeetingPanel != null) LockedMeetingPanel.Visible = false;
                pnlSelectedMeeting.Visible = false;
                pnlNoSelected.Visible = true;
                return;
            }

            // Upcoming or ongoing meeting: do not even load/edit attendance rows;
            // render the full right pane as a read-only chained lock state.
            bool isEnded = meeting.ThoiGianKetThuc <= DateTime.Now;
            if (LockedMeetingPanel != null) LockedMeetingPanel.Visible = !isEnded;
            pnlNoSelected.Visible = false;
            if (!isEnded)
            {
                Literal name = LockedMeetingName;
                if (name != null) name.Text = HttpUtility.HtmlEncode(meeting.TenCuocHop);
                pnlSelectedMeeting.Visible = false;
                btnSaveLabor.Visible = false;
                rptLabor.DataSource = null;
                rptLabor.DataBind();
                SetLaborError(string.Empty);
                SetTabError(string.Empty);
                return;
            }

            decimal duration = DurationHours(meeting);
            List<GioCongNhanVienRow> rows;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                rows = LoadRoster(connection, meetingId, duration);
            }

            hfSelectedMeetId.Value = meetingId.ToString("D");
            hfDuration.Value = duration.ToString(CultureInfo.InvariantCulture);
            ViewState["GioCongDuration"] = duration.ToString(CultureInfo.InvariantCulture);
            litMeetName.Text = HttpUtility.HtmlEncode(meeting.TenCuocHop);
            litMeetDate.Text = meeting.ThoiGianBatDau.ToString("dd/MM/yyyy", ViCulture);
            litMeetTime.Text = meeting.ThoiGianBatDau.ToString("HH:mm", ViCulture) + " – " + meeting.ThoiGianKetThuc.ToString("HH:mm", ViCulture);
            litDuration.Text = DurationMinutes(meeting).ToString(CultureInfo.InvariantCulture) + " phút = " + duration.ToString("0.##", ViCulture) + " giờ công";
            litJoinedCount.Text = rows.Count(r => r.XacNhanThamGia).ToString(CultureInfo.InvariantCulture);
            litTheoryTotal.Text = rows.Sum(r => r.SoGioLyThuyet).ToString("0.##", ViCulture) + " giờ";
            _canEditLabor = CanEditLabor;
            btnSaveLabor.Visible = _canEditLabor && rows.Count > 0;
            phNoPeople.Visible = rows.Count == 0;
            rptLabor.DataSource = rows;
            rptLabor.DataBind();
            Label resultLabel = LaborErrorLabel;
            if (resultLabel != null) resultLabel.CssClass = "mlt-error";
            SetLaborError(string.Empty);
            SetTabError(string.Empty);
            pnlSelectedMeeting.Visible = true;
            pnlNoSelected.Visible = false;
        }

        protected void rptLabor_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            GioCongNhanVienRow row = (GioCongNhanVienRow)e.Item.DataItem;
            ((HiddenField)e.Item.FindControl("hfUserId")).Value = row.IdNhanVien.ToString("D");
            ((Label)e.Item.FindControl("lblName")).Text = HttpUtility.HtmlEncode(row.TenNhanVien);

            HtmlGenericControl wrap = (HtmlGenericControl)e.Item.FindControl("personWrap");
            HtmlGenericControl icon = (HtmlGenericControl)e.Item.FindControl("iconHost");
            HtmlGenericControl tag = (HtmlGenericControl)e.Item.FindControl("tagHost");
            wrap.Attributes["class"] = row.LaChuTri ? "mlt-user is-host" : "mlt-user";
            icon.Visible = row.LaChuTri;
            tag.Visible = row.LaChuTri;

            RadioButtonList attendance = (RadioButtonList)e.Item.FindControl("rblAttendance");
            attendance.SelectedValue = row.XacNhanThamGia ? "1" : "0";
            attendance.Enabled = _canEditLabor;
            ((Label)e.Item.FindControl("lblTheory")).Text = row.SoGioLyThuyet.ToString("0.##", ViCulture) + " giờ";

            TextBox actual = (TextBox)e.Item.FindControl("txtActual");
            actual.Text = row.XacNhanThamGia
                ? (row.SoGioThucTe.HasValue ? row.SoGioThucTe.Value.ToString("0.##", ViCulture) : string.Empty)
                : "0";
            // PM phải có ReadOnly=false trên server để không đánh mất giá trị POST.
            // Chỉ khóa ở HTML khi nhân viên đang được đánh dấu Không tham gia.
            actual.ReadOnly = !_canEditLabor;
            if (_canEditLabor && !row.XacNhanThamGia)
                actual.Attributes["readonly"] = "readonly";
            else
                actual.Attributes.Remove("readonly");
            actual.Attributes["inputmode"] = "decimal";
            actual.Attributes["autocomplete"] = "off";
        }

        private static bool TryHours(string value, out decimal? hours)
        {
            hours = null;
            if (string.IsNullOrWhiteSpace(value)) return true;
            decimal parsed;
            if (!decimal.TryParse(value.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                                  CultureInfo.InvariantCulture, out parsed)) return false;
            if (parsed < 0m || parsed > 9999.99m || decimal.Round(parsed, 2) != parsed) return false;
            hours = parsed;
            return true;
        }

        private void ShowSaveError(string text)
        {
            if (SaveResultField != null) SaveResultField.Value = "error";
            Label resultLabel = LaborErrorLabel;
            if (resultLabel != null) resultLabel.CssClass = "mlt-error";
            SetLaborError(HttpUtility.HtmlEncode(text));
            upLaborTab.Update();
        }

        protected void btnSaveLabor_Click(object sender, EventArgs e)
        {
            if (SaveResultField != null) SaveResultField.Value = "error";
            Guid projectId = ProjectId;
            Guid meetingId = SelectedMeetingId;
            TblLichHop meeting;
            if (!CanViewLabor || !IsPM(projectId) ||
                !IsValidMeeting(meetingId, projectId, out meeting) ||
                !string.Equals(hfSelectedMeetId.Value, meetingId.ToString("D"), StringComparison.OrdinalIgnoreCase))
            {
                ShowSaveError("Không có quyền lưu hoặc cuộc họp đã thay đổi. Vui lòng tải lại tab.");
                return;
            }

            // Enforce the lock in the backend too; client-side overlay is only presentation.
            if (meeting.ThoiGianKetThuc > DateTime.Now)
            {
                ShowSaveError("Cuộc họp chưa kết thúc. Không thể chỉnh sửa hoặc lưu giờ công.");
                return;
            }

            Dictionary<Guid, Tuple<bool, decimal?>> posted = new Dictionary<Guid, Tuple<bool, decimal?>>();
            foreach (RepeaterItem item in rptLabor.Items)
            {
                Guid userId;
                HiddenField idField = (HiddenField)item.FindControl("hfUserId");
                RadioButtonList attendance = (RadioButtonList)item.FindControl("rblAttendance");
                TextBox actual = (TextBox)item.FindControl("txtActual");
                if (idField == null || attendance == null || actual == null ||
                    !Guid.TryParse(idField.Value, out userId) || userId == Guid.Empty || posted.ContainsKey(userId))
                {
                    ShowSaveError("Danh sách nhân viên không hợp lệ. Vui lòng mở lại tab.");
                    return;
                }

                // Đọc giá trị mới trực tiếp từ form thay vì hidden field của phiên bản popup cũ.
                string selected = Request.Form[attendance.UniqueID];
                if (selected != "1" && selected != "0")
                {
                    ShowSaveError("Vui lòng chọn Có hoặc Không tham gia cho từng nhân viên.");
                    return;
                }
                bool joined = selected == "1";
                decimal? hours = 0m;
                if (joined)
                {
                    string raw = Request.Form[actual.UniqueID] ?? actual.Text;
                    if (!TryHours(raw, out hours))
                    {
                        ShowSaveError("Giờ công thực tế phải từ 0 đến 9999,99 và tối đa 2 chữ số thập phân.");
                        return;
                    }
                }
                posted.Add(userId, Tuple.Create(joined, hours));
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    List<GioCongNhanVienRow> authorized = LoadRoster(connection, meetingId, DurationHours(meeting));
                    if (!new HashSet<Guid>(posted.Keys).SetEquals(authorized.Select(r => r.IdNhanVien)))
                    {
                        ShowSaveError("Danh sách nhân viên đã thay đổi. Vui lòng tải lại tab trước khi lưu.");
                        return;
                    }

                    using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        foreach (KeyValuePair<Guid, Tuple<bool, decimal?>> input in posted)
                        {
                            const string sql = @"
UPDATE dbo.TblLichHopGioCong WITH (UPDLOCK, SERIALIZABLE)
SET SoGioThucTe=@SoGioThucTe, DaThamGiaOverride=@DaThamGiaOverride,
    NguoiCapNhat=@NguoiCapNhat, NgayCapNhat=GETDATE()
WHERE IdLichHop=@IdLichHop AND IdNhanVien=@IdNhanVien;
IF @@ROWCOUNT = 0
    INSERT INTO dbo.TblLichHopGioCong
      (IdLichHop, IdNhanVien, SoGioThucTe, DaThamGiaOverride, NguoiCapNhat, NgayCapNhat)
    VALUES
      (@IdLichHop, @IdNhanVien, @SoGioThucTe, @DaThamGiaOverride, @NguoiCapNhat, GETDATE());";
                            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                            {
                                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                                command.Parameters.Add(GuidParameter("@IdNhanVien", input.Key));
                                command.Parameters.Add(GuidParameter("@NguoiCapNhat", SweetContext.Current.UserId));
                                SqlParameter hoursParameter = command.Parameters.Add("@SoGioThucTe", SqlDbType.Decimal);
                                hoursParameter.Precision = 10;
                                hoursParameter.Scale = 2;
                                hoursParameter.Value = input.Value.Item2.HasValue
                                    ? (object)input.Value.Item2.Value : DBNull.Value;
                                command.Parameters.Add("@DaThamGiaOverride", SqlDbType.Bit).Value = input.Value.Item1;
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }

                // Chỉ nạp lại bảng nhân viên của cuộc họp đang chọn.
                // Không DataBind lại rptMeetings / thanh lọc, để các nút chọn cuộc họp
                // không bị tái tạo liên tục sau khi bấm Lưu.
                BindSelectedMeeting(meetingId);
                Label resultLabel = LaborErrorLabel;
                if (resultLabel != null)
                {
                    resultLabel.CssClass = "mlt-success";
                    resultLabel.Text = "Đã lưu giờ công của cuộc họp.";
                }
                if (SaveResultField != null) SaveResultField.Value = "success";
                upLaborTab.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CtrlMeetGioCongTab.Save: " + ex);
                ShowSaveError("Không thể lưu giờ công. Kiểm tra cấu hình/bảng TblLichHopGioCong và log máy chủ.");
            }
        }
    }
}
