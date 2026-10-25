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
                return mode == "all" || mode == "active" || mode == "hosted" ? mode : "ended";
            }
            set { ViewState["GioCongMeetingFilter"] = value; }
        }

        // Bộ lọc được giữ bằng ViewState qua UpdatePanel, không đổi dữ liệu khi chưa bấm Lưu.
        private string MeetingSearch { get { return Convert.ToString(ViewState["MeetLaborNameSearch"] ?? "", CultureInfo.InvariantCulture); } set { ViewState["MeetLaborNameSearch"] = (value ?? "").Trim(); } }
        private string StaffSearch { get { return Convert.ToString(ViewState["MeetLaborStaffSearch"] ?? "", CultureInfo.InvariantCulture); } set { ViewState["MeetLaborStaffSearch"] = (value ?? "").Trim(); } }
        private int StaffAttendance { get { return ViewState["MeetLaborAttendance"] is int ? (int)ViewState["MeetLaborAttendance"] : -1; } set { ViewState["MeetLaborAttendance"] = value == 0 || value == 1 ? value : -1; } }
        private DateTime? DateFrom { get { return ViewState["MeetLaborDateFrom"] is DateTime ? (DateTime?)ViewState["MeetLaborDateFrom"] : null; } set { ViewState["MeetLaborDateFrom"] = value; } }
        private DateTime? DateTo { get { return ViewState["MeetLaborDateTo"] is DateTime ? (DateTime?)ViewState["MeetLaborDateTo"] : null; } set { ViewState["MeetLaborDateTo"] = value; } }
        private static string SafeSearch(string value) { return (value ?? string.Empty).Trim().Substring(0, Math.Min((value ?? string.Empty).Trim().Length, 120)); }
        private static string LikePattern(string text) { return "%" + (text ?? "").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"; }

        private void BindFilterState()
        {
            HtmlGenericControl el = LaborRootElement;
            if (el == null) return;
            el.Attributes["data-meeting-mode"] = MeetingFilter;
            el.Attributes["data-meeting-search"] = MeetingSearch;
            el.Attributes["data-staff-search"] = StaffSearch;
            el.Attributes["data-staff-mode"] = StaffAttendance.ToString(CultureInfo.InvariantCulture);
            el.Attributes["data-date-from"] = DateFrom.HasValue ? DateFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
            el.Attributes["data-date-to"] = DateTo.HasValue ? DateTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
        }

        private const int MeetingsPerPage = 5;

        private int MeetingPage
        {
            get { return ViewState["GioCongMeetingPage"] is int && (int)ViewState["GioCongMeetingPage"] > 0 ? (int)ViewState["GioCongMeetingPage"] : 1; }
            set { ViewState["GioCongMeetingPage"] = Math.Max(1, value); }
        }

        private const int LaborPerPage = 5;
        private int LaborPage
        {
            get { return ViewState["GioCongLaborPage"] is int && (int)ViewState["GioCongLaborPage"] > 0 ? (int)ViewState["GioCongLaborPage"] : 1; }
            set { ViewState["GioCongLaborPage"] = Math.Max(1, value); }
        }

        private Guid SelectedMeetingId
        {
            get { return ViewState["GioCongSelectedMeeting"] is Guid ? (Guid)ViewState["GioCongSelectedMeeting"] : Guid.Empty; }
            set { ViewState["GioCongSelectedMeeting"] = value; }
        }

        protected bool CanEditLabor { get { return _canEditLabor; } }

        private bool CanManageProject
        {
            get
            {
                BaseAdminPage page = Page as BaseAdminPage;
                return IsPM(ProjectId) || (page != null && page.IsAdministrator);
            }
        }

        private bool CanEditMeeting(TblLichHop meeting)
        {
            return CanViewLabor && meeting != null && meeting.IdDuAn == ProjectId && meeting.DaXoa != true
                   && meeting.ThoiGianKetThuc <= DateTime.Now
                   && (CanManageProject || meeting.IdNguoiTao == SweetContext.Current.UserId);
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

        private Literal LaborPagerLiteral
        {
            get { return FindChildControl(this, "litLaborPager") as Literal; }
        }

        // Không phụ thuộc designer.cs có sinh trường cho nút lọc mới hay không.
        protected string HostedFilterClientId
        {
            get
            {
                Control control = FindChildControl(this, "btnFilterHosted");
                return control == null ? string.Empty : control.ClientID;
            }
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
            // Thong bao qua ShowNotify chung cua he thong; khong tao banner lam thay doi chieu cao bang.
            Label label = TabErrorLabel;
            if (label != null) { label.Text = string.Empty; label.Visible = false; }
            if (!string.IsNullOrWhiteSpace(message)) ShowNotify(message, MSGType.Error);
        }

        private void SetLaborError(string message)
        {
            // Chi giu control vi tuong thich WebForms; loi luu duoc thong bao trong ShowSaveError.
            Label label = LaborErrorLabel;
            if (label != null) { label.Text = string.Empty; label.Visible = false; }
        }

        private sealed class MeetingRow
        {
            public Guid IdLichHop { get; set; }
            public string TenCuocHop { get; set; }
            public DateTime ThoiGianBatDau { get; set; }
            public DateTime ThoiGianKetThuc { get; set; }
            public bool LaChuTri { get; set; }
        }

        public sealed class GioCongNhanVienRow
        {
            public Guid IdNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public string Email { get; set; }
            public string Avatar { get; set; }
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

        // Chặn truy cập trực tiếp bằng cách giả ID cuộc họp trong postback.
        // Admin/PM được xem mọi cuộc họp của dự án; nhân viên chỉ xem cuộc họp
        // do chính mình chủ trì hoặc có tên trong TblLichHop_NhanVien.
        private static bool CanAccessMeeting(SqlConnection connection, Guid projectId, Guid meetingId,
                                             Guid currentUserId, bool canManage)
        {
            const string sql = @"
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.TblLichHop m
    WHERE m.IdLichHop=@IdLichHop AND m.IdDuAn=@IdDuAn
      AND (m.DaXoa=0 OR m.DaXoa IS NULL)
      AND (@CanManage=1 OR m.IdNguoiTao=@UserId OR EXISTS (
          SELECT 1 FROM dbo.TblLichHop_NhanVien nv
          WHERE nv.IdLichHop=m.IdLichHop AND nv.IdNhanVien=@UserId))
) THEN 1 ELSE 0 END;";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdDuAn", projectId));
                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                command.Parameters.Add(GuidParameter("@UserId", currentUserId));
                command.Parameters.Add("@CanManage", SqlDbType.Bit).Value = canManage;
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
            }
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

        // Danh sách nhân viên hợp lệ, gồm người được phân công và người chủ trì.
        // SQL tổng hợp và SQL phân trang cùng dùng một nguồn để số liệu nhất quán.
        private const string RosterCteSql = @"
WITH DanhSach AS (
    SELECT DISTINCT p.IdNhanVien
    FROM dbo.TblLichHop_NhanVien p
    WHERE p.IdLichHop = @IdLichHop AND p.IdNhanVien IS NOT NULL
    UNION
    SELECT m.IdNguoiTao
    FROM dbo.TblLichHop m
    WHERE m.IdLichHop = @IdLichHop AND m.IdNguoiTao IS NOT NULL
), TatCa AS (
    SELECT d.IdNhanVien,
           COALESCE(NULLIF(LTRIM(RTRIM(u.DisplayName)), N''),
                    NULLIF(LTRIM(RTRIM(u.UserName)), N''),
                    CONVERT(NVARCHAR(36), d.IdNhanVien)) AS TenNhanVien,
           COALESCE(NULLIF(LTRIM(RTRIM(mu.Email)), N''), N'') AS Email,
           COALESCE(NULLIF(LTRIM(RTRIM(u.Avatar)), N''), N'') AS Avatar,
           CASE WHEN d.IdNhanVien = m.IdNguoiTao THEN 1 ELSE 0 END AS LaChuTri,
           ISNULL(CAST(g.DaThamGiaOverride AS INT),
                 ISNULL((SELECT MAX(CAST(ISNULL(p.XacNhanThamGia, 0) AS INT))
                         FROM dbo.TblLichHop_NhanVien p
                         WHERE p.IdLichHop = @IdLichHop AND p.IdNhanVien = d.IdNhanVien), 0)) AS XacNhanThamGia,
           g.SoGioThucTe
    FROM DanhSach d
    CROSS JOIN dbo.TblLichHop m
    LEFT JOIN dbo.aspnet_Users u ON u.UserId = d.IdNhanVien
    LEFT JOIN dbo.aspnet_Membership mu ON mu.UserId = d.IdNhanVien
    LEFT JOIN dbo.TblLichHopGioCong g ON g.IdLichHop = @IdLichHop AND g.IdNhanVien = d.IdNhanVien
    WHERE m.IdLichHop = @IdLichHop
)
";

        private const string RosterFilterSql = @"
 WHERE (@StaffSearch=N'' OR TenNhanVien LIKE @StaffPattern OR Email LIKE @StaffPattern)
   AND (@JoinedMode=-1 OR XacNhanThamGia=@JoinedMode)";

        private static void AddRosterFilters(SqlCommand cmd, string search, int joinedMode)
        {
            cmd.Parameters.Add("@StaffSearch", SqlDbType.NVarChar, 120).Value = search ?? "";
            cmd.Parameters.Add("@StaffPattern", SqlDbType.NVarChar, 260).Value = LikePattern(search);
            cmd.Parameters.Add("@JoinedMode", SqlDbType.Int).Value = joinedMode;
        }

        private static int CountFilteredRoster(SqlConnection connection, Guid meetingId, string search, int mode)
        {
            string sql = RosterCteSql + "SELECT COUNT(1) FROM TatCa" + RosterFilterSql + ";";
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                AddRosterFilters(cmd, search, mode);
                return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        // Chỉ trả về tổng số, tổng đã tham gia cho toàn bộ cuộc họp.
        private static void LoadRosterSummary(SqlConnection connection, Guid meetingId,
            out int totalPeople, out int joinedPeople)
        {
            string sql = RosterCteSql + @"
SELECT COUNT(1) AS SoNguoi, ISNULL(SUM(XacNhanThamGia), 0) AS SoNguoiThamGia
FROM TatCa;";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new DataException("Không đọc được tổng giờ công cuộc họp.");
                    totalPeople = Convert.ToInt32(reader["SoNguoi"], CultureInfo.InvariantCulture);
                    joinedPeople = Convert.ToInt32(reader["SoNguoiThamGia"], CultureInfo.InvariantCulture);
                }
            }
        }

        // Phân trang TẠI SQL để cuộc họp có hàng trăm nhân viên vẫn chỉ nạp 6 dòng/lần.
        private static List<GioCongNhanVienRow> LoadRosterPage(SqlConnection connection,
            Guid meetingId, decimal duration, int page, string staffSearch, int attendanceMode)
        {
            string sql = RosterCteSql + @",
PhanTrang AS (
    SELECT TatCa.*, ROW_NUMBER() OVER (ORDER BY TenNhanVien ASC, IdNhanVien ASC) AS SoThuTu
    FROM TatCa" + RosterFilterSql + @"
)
SELECT IdNhanVien, TenNhanVien, Email, Avatar, LaChuTri, XacNhanThamGia, SoGioThucTe
FROM PhanTrang
WHERE SoThuTu > @Start AND SoThuTu <= @End
ORDER BY SoThuTu ASC;";
            List<GioCongNhanVienRow> results = new List<GioCongNhanVienRow>();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                AddRosterFilters(command, staffSearch, attendanceMode);
                command.Parameters.Add("@Start", SqlDbType.Int).Value = (page - 1) * LaborPerPage;
                command.Parameters.Add("@End", SqlDbType.Int).Value = page * LaborPerPage;
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
                            Email = Convert.ToString(reader["Email"], CultureInfo.InvariantCulture),
                            Avatar = Convert.ToString(reader["Avatar"], CultureInfo.InvariantCulture),
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
        // COUNT + ROW_NUMBER tại SQL; mỗi trang CHỈ nạp 5 cuộc họp, có lọc theo quyền.
        private static List<MeetingRow> LoadMeetingPage(SqlConnection connection, Guid projectId,
            string mode, Guid userId, bool canManage, int requestedPage, string searchName,
             DateTime? dateFrom, DateTime? dateTo, out int total, out int actualPage)
        {
            const string where = @" FROM dbo.TblLichHop m
WHERE m.IdDuAn=@IdDuAn AND (m.DaXoa=0 OR m.DaXoa IS NULL)
  AND (@CanManage=1 OR m.IdNguoiTao=@UserId
       OR EXISTS (SELECT 1 FROM dbo.TblLichHop_NhanVien p
                  WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId))
   AND (@Filter=0 OR (@Filter=1 AND m.ThoiGianKetThuc>GETDATE())
        OR (@Filter=2 AND m.ThoiGianKetThuc<=GETDATE())
        OR (@Filter=3 AND m.IdNguoiTao=@UserId))
   AND (@NameSearch=N'' OR m.TenCuocHop LIKE @NamePattern)
   AND (@FromDate IS NULL OR m.ThoiGianBatDau >= @FromDate)
   AND (@ToExclusive IS NULL OR m.ThoiGianBatDau < @ToExclusive)";
            int code = mode == "all" ? 0 : mode == "active" ? 1 : mode == "hosted" ? 3 : 2;
            Action<SqlCommand> addFilters = command =>
            {
                command.Parameters.Add("@NameSearch", SqlDbType.NVarChar, 120).Value = searchName ?? "";
                command.Parameters.Add("@NamePattern", SqlDbType.NVarChar, 260).Value = LikePattern(searchName);
                command.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = dateFrom.HasValue ? (object)dateFrom.Value.Date : DBNull.Value;
                command.Parameters.Add("@ToExclusive", SqlDbType.DateTime).Value = dateTo.HasValue ? (object)dateTo.Value.Date.AddDays(1) : DBNull.Value;
            };
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1)" + where, connection))
            {
                command.Parameters.Add(GuidParameter("@IdDuAn", projectId));
                command.Parameters.Add(GuidParameter("@UserId", userId));
                command.Parameters.Add("@CanManage", SqlDbType.Bit).Value = canManage;
                command.Parameters.Add("@Filter", SqlDbType.Int).Value = code;
                addFilters(command);
                total = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
            int pageCount = Math.Max(1, (total + MeetingsPerPage - 1) / MeetingsPerPage);
            actualPage = Math.Max(1, Math.Min(requestedPage, pageCount));
            List<MeetingRow> result = new List<MeetingRow>();
            if (total == 0) return result;
            string sql = @"
WITH DanhSachCoThuTu AS (
 SELECT m.IdLichHop, m.TenCuocHop, m.ThoiGianBatDau, m.ThoiGianKetThuc,
        CASE WHEN m.IdNguoiTao=@UserId THEN 1 ELSE 0 END AS LaChuTri,
        ROW_NUMBER() OVER (ORDER BY m.ThoiGianBatDau DESC, m.IdLichHop DESC) AS SoThuTu" + where + @"
)
SELECT IdLichHop, TenCuocHop, ThoiGianBatDau, ThoiGianKetThuc, LaChuTri
FROM DanhSachCoThuTu
WHERE SoThuTu > @Offset AND SoThuTu <= @Last
ORDER BY SoThuTu ASC;";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(GuidParameter("@IdDuAn", projectId));
                command.Parameters.Add(GuidParameter("@UserId", userId));
                command.Parameters.Add("@CanManage", SqlDbType.Bit).Value = canManage;
                command.Parameters.Add("@Filter", SqlDbType.Int).Value = code;
                addFilters(command);
                int offset = (actualPage - 1) * MeetingsPerPage;
                command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
                command.Parameters.Add("@Last", SqlDbType.Int).Value = offset + MeetingsPerPage;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new MeetingRow
                        {
                            IdLichHop = (Guid)reader["IdLichHop"],
                            TenCuocHop = Convert.ToString(reader["TenCuocHop"], CultureInfo.InvariantCulture),
                            ThoiGianBatDau = Convert.ToDateTime(reader["ThoiGianBatDau"], CultureInfo.InvariantCulture),
                            ThoiGianKetThuc = Convert.ToDateTime(reader["ThoiGianKetThuc"], CultureInfo.InvariantCulture),
                            LaChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) == 1
                        });
                    }
                }
            }
            return result;
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

        private static string LaborPagerButton(int page, string innerHtml, string label,
            bool disabled, bool active)
        {
            if (disabled || active)
                return "<button type=\"button\" class=\"mlt-page-button" + (active ? " is-active" : "") +
                    "\" aria-label=\"" + HttpUtility.HtmlAttributeEncode(label) + "\"" +
                    (active ? " aria-current=\"page\"" : "") + " disabled>" + innerHtml + "</button>";
            return "<button type=\"button\" class=\"mlt-page-button\" aria-label=\"" +
                HttpUtility.HtmlAttributeEncode(label) + "\" onclick=\"return QLDAMeetingLaborTab.changeLaborPage(" +
                page.ToString(CultureInfo.InvariantCulture) + ");\">" + innerHtml + "</button>";
        }

        private static string BuildLaborPagerHtml(int total, int currentPage)
        {
            if (total == 0) return "<span class=\"mlt-page-info\">0 nhân viên</span>";
            int pages = (total + LaborPerPage - 1) / LaborPerPage;
            int first = (currentPage - 1) * LaborPerPage + 1;
            int last = Math.Min(total, currentPage * LaborPerPage);
            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<span class=\"mlt-page-info\">").Append(first).Append("–").Append(last)
                .Append(" / ").Append(total).Append(" nhân viên</span><div class=\"mlt-page-controls\">");
            html.Append(LaborPagerButton(currentPage - 1, "&#8249;", "Trang nhân viên trước", currentPage == 1, false));
            System.Collections.Generic.SortedSet<int> numbers = new System.Collections.Generic.SortedSet<int>();
            numbers.Add(1); numbers.Add(pages);
            for (int p = Math.Max(1, currentPage - 1); p <= Math.Min(pages, currentPage + 1); p++) numbers.Add(p);
            int prev = 0;
            foreach (int p in numbers)
            {
                if (prev > 0 && p - prev > 1)
                    html.Append("<span class=\"mlt-page-ellipsis\" aria-hidden=\"true\">…</span>");
                html.Append(LaborPagerButton(p, p.ToString(CultureInfo.InvariantCulture),
                    "Trang nhân viên " + p, false, p == currentPage));
                prev = p;
            }
            html.Append(LaborPagerButton(currentPage + 1, "&#8250;", "Trang nhân viên sau", currentPage >= pages, false));
            html.Append("</div>");
            return html.ToString();
        }

        protected string MeetingHostBadge(object isHost)
        {
            return isHost != null && isHost != DBNull.Value && Convert.ToBoolean(isHost, CultureInfo.InvariantCulture)
                ? "<span class='mlt-meeting-host' title='Bạn là người chủ trì cuộc họp'><i class='fas fa-crown' aria-hidden='true'></i> Bạn chủ trì</span>"
                : string.Empty;
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
            LinkButton hostedFilter = FindChildControl(this, "btnFilterHosted") as LinkButton;
            if (hostedFilter != null) manager.RegisterAsyncPostBackControl(hostedFilter);
            manager.RegisterAsyncPostBackControl(btnSaveLabor);
        }

        // Được nút ẩn bên CtrlMeet.ascx gọi khi mở tab Giờ công.
        protected void btnLoadLaborTab_Click(object sender, EventArgs e)
        {
            if (SaveResultField != null) SaveResultField.Value = "idle";
            if (!CanViewLabor || ProjectId == Guid.Empty)
            {
                ShowNotify("Bạn không có quyền xem giờ công cuộc họp của dự án.", MSGType.Error);
                return;
            }
            // Filter action goes through this same stable async control to keep the unsaved-change guard.
            string filterAction = Request.Form["QLDA_MeetLabor_FilterAction"];
            string filterValue = Request.Form["QLDA_MeetLabor_FilterValue"];
            if (!string.IsNullOrWhiteSpace(filterAction))
            {
                if (filterAction == "meetingMode")
                {
                    if (filterValue != "all" && filterValue != "active" && filterValue != "ended" && filterValue != "hosted") return;
                    MeetingFilter = filterValue;
                    MeetingPage = 1; LaborPage = 1; SelectedMeetingId = Guid.Empty;
                    BindTab(); return;
                }
                if (filterAction == "meetingSearch")
                {
                    MeetingSearch = SafeSearch(filterValue);
                    MeetingPage = 1; LaborPage = 1; SelectedMeetingId = Guid.Empty;
                    BindTab(); return;
                }
                if (filterAction == "staffSearch" || filterAction == "attendance")
                {
                    if (filterAction == "staffSearch") StaffSearch = SafeSearch(filterValue);
                    else { int selectedMode; StaffAttendance = int.TryParse(filterValue, out selectedMode) ? selectedMode : -1; }
                    LaborPage = 1;
                    if (SelectedMeetingId != Guid.Empty) BindSelectedMeeting(SelectedMeetingId);
                    else BindTab();
                    upLaborTab.Update(); return;
                }
                if (filterAction == "range" || filterAction == "clearRange")
                {
                    DateTime start, end;
                    if (filterAction == "clearRange") { DateFrom = null; DateTo = null; }
                    else if (DateTime.TryParseExact(Request.Form["QLDA_MeetLabor_FilterFrom"], "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out start) &&
                        DateTime.TryParseExact(Request.Form["QLDA_MeetLabor_FilterTo"], "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out end) && start.Date <= end.Date && end.Year < 9999)
                    { DateFrom = start.Date; DateTo = end.Date; }
                    else { ShowNotify("Khoảng thời gian không hợp lệ. Ngày kết thúc phải từ ngày bắt đầu trở đi.", MSGType.Warning); return; }
                    MeetingPage = 1; LaborPage = 1; SelectedMeetingId = Guid.Empty;
                    BindTab(); return;
                }
            }
            // The hidden client-side selection field is populated by selectMeeting() above.
            // Native LinkButton clicks are intercepted and forwarded through this stable async event.
            int page;
            string requestedLaborPage = Request.Form["QLDA_MeetLabor_LaborPage"];
            if (!string.IsNullOrWhiteSpace(requestedLaborPage) &&
                int.TryParse(requestedLaborPage, out page) && page > 0)
            {
                TblLichHop selectedMeeting;
                if (IsValidMeeting(SelectedMeetingId, ProjectId, out selectedMeeting) &&
                    selectedMeeting.ThoiGianKetThuc <= DateTime.Now)
                {
                    LaborPage = page;
                    BindSelectedMeeting(SelectedMeetingId);
                    upLaborTab.Update();
                }
                return;
            }
            string requestedPage = Request.Form["QLDA_MeetLabor_Page"];
            if (!string.IsNullOrWhiteSpace(requestedPage) && int.TryParse(requestedPage, out page) && page > 0)
            {
                MeetingPage = page;
                LaborPage = 1;
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
                    (MeetingFilter == "active" && requestedMeeting.ThoiGianKetThuc > DateTime.Now) ||
                    (MeetingFilter == "hosted" && requestedMeeting.IdNguoiTao == SweetContext.Current.UserId)))
                {
                    if (SelectedMeetingId != requestedId) LaborPage = 1;
                    SelectedMeetingId = requestedId;
                }
            }
            BindTab();
        }

        protected void btnFilterMeetings_Click(object sender, EventArgs e)
        {
            if (SaveResultField != null) SaveResultField.Value = "idle";
            if (!CanViewLabor || ProjectId == Guid.Empty) return;
            LinkButton button = sender as LinkButton;
            string mode = button == null ? string.Empty : button.CommandArgument;
            if (mode != "all" && mode != "active" && mode != "ended" && mode != "hosted") return;
            if (MeetingFilter == mode && LaborLoaded) return;
            MeetingFilter = mode;
            MeetingPage = 1;
            LaborPage = 1;
            SelectedMeetingId = Guid.Empty;
            BindTab();
        }

        protected void rptMeetings_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (SaveResultField != null) SaveResultField.Value = "idle";
            if (!string.Equals(e.CommandName, "select", StringComparison.OrdinalIgnoreCase) ||
                !CanViewLabor || ProjectId == Guid.Empty) return;
            Guid meetingId;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out meetingId)) return;
            TblLichHop meeting;
            if (!IsValidMeeting(meetingId, ProjectId, out meeting)) return;
            if (MeetingFilter == "active" && meeting.ThoiGianKetThuc <= DateTime.Now) return;
            if (MeetingFilter == "ended" && meeting.ThoiGianKetThuc > DateTime.Now) return;
            if (MeetingFilter == "hosted" && meeting.IdNguoiTao != SweetContext.Current.UserId) return;
            if (SelectedMeetingId != meetingId) LaborPage = 1;
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
                    meetings = LoadMeetingPage(connection, ProjectId, MeetingFilter,
                        SweetContext.Current.UserId, CanManageProject, MeetingPage,
                        MeetingSearch, DateFrom, DateTo, out total, out actualPage);
                }
                MeetingPage = actualPage;

                Guid selected = SelectedMeetingId;
                if (!meetings.Any(m => m.IdLichHop == selected))
                    selected = meetings.Count == 0 ? Guid.Empty : meetings[0].IdLichHop;
                if (SelectedMeetingId != selected) LaborPage = 1;
                SelectedMeetingId = selected;

                btnFilterAll.CssClass = MeetingFilter == "all" ? "is-filtered" : string.Empty;
                btnFilterActive.CssClass = MeetingFilter == "active" ? "is-filtered" : string.Empty;
                btnFilterEnded.CssClass = MeetingFilter == "ended" ? "is-filtered" : string.Empty;
                LinkButton hostedFilter = FindChildControl(this, "btnFilterHosted") as LinkButton;
                if (hostedFilter != null)
                {
                    hostedFilter.CssClass = MeetingFilter == "hosted" ? "is-filtered" : string.Empty;
                    hostedFilter.Attributes["aria-pressed"] = MeetingFilter == "hosted" ? "true" : "false";
                }
                btnFilterAll.Attributes["aria-pressed"] = MeetingFilter == "all" ? "true" : "false";
                btnFilterActive.Attributes["aria-pressed"] = MeetingFilter == "active" ? "true" : "false";
                btnFilterEnded.Attributes["aria-pressed"] = MeetingFilter == "ended" ? "true" : "false";
                litMeetingCount.Text = total.ToString(CultureInfo.InvariantCulture);
                BindFilterState();
                Literal pager = MeetingPagerLiteral;
                if (pager != null) pager.Text = BuildPagerHtml(total, actualPage);
                pnlNoMeetings.Visible = total == 0;
                litNoMeetings.Text = MeetingFilter == "hosted" ? "Bạn chưa chủ trì cuộc họp nào."
                    : MeetingFilter == "all" ? "Không có cuộc họp thuộc phạm vi bạn được xem."
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

            using (SqlConnection permissionConnection = new SqlConnection(GetConnectionString()))
            {
                permissionConnection.Open();
                if (!CanViewLabor || !CanAccessMeeting(permissionConnection, ProjectId, meetingId,
                    SweetContext.Current.UserId, CanManageProject))
                {
                    SelectedMeetingId = Guid.Empty;
                    pnlSelectedMeeting.Visible = false;
                    pnlNoSelected.Visible = true;
                    if (LockedMeetingPanel != null) LockedMeetingPanel.Visible = false;
                    rptLabor.DataSource = null;
                    rptLabor.DataBind();
                    return;
                }
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
            int totalPeople, joinedPeople, matchingPeople;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                LoadRosterSummary(connection, meetingId, out totalPeople, out joinedPeople);
                matchingPeople = CountFilteredRoster(connection, meetingId, StaffSearch, StaffAttendance);
                int pageCount = Math.Max(1, (matchingPeople + LaborPerPage - 1) / LaborPerPage);
                LaborPage = Math.Min(LaborPage, pageCount);
                rows = LoadRosterPage(connection, meetingId, duration, LaborPage, StaffSearch, StaffAttendance);
            }

            pnlSelectedMeeting.Attributes["data-total-joined"] = joinedPeople.ToString(CultureInfo.InvariantCulture);
            pnlSelectedMeeting.Attributes["data-total-theory"] =
                (joinedPeople * duration).ToString(CultureInfo.InvariantCulture);
            Literal laborPager = LaborPagerLiteral;
            if (laborPager != null) laborPager.Text = BuildLaborPagerHtml(matchingPeople, LaborPage);

            hfSelectedMeetId.Value = meetingId.ToString("D");
            hfDuration.Value = duration.ToString(CultureInfo.InvariantCulture);
            ViewState["GioCongDuration"] = duration.ToString(CultureInfo.InvariantCulture);
            litMeetName.Text = HttpUtility.HtmlEncode(meeting.TenCuocHop);
            litMeetDate.Text = meeting.ThoiGianBatDau.ToString("dd/MM/yyyy", ViCulture);
            litMeetTime.Text = meeting.ThoiGianBatDau.ToString("HH:mm", ViCulture) + " – " + meeting.ThoiGianKetThuc.ToString("HH:mm", ViCulture);
            litDuration.Text = DurationMinutes(meeting).ToString(CultureInfo.InvariantCulture) + " phút = " + duration.ToString("0.##", ViCulture) + " giờ công";
            litJoinedCount.Text = joinedPeople.ToString(CultureInfo.InvariantCulture);
            litTheoryTotal.Text = (joinedPeople * duration).ToString("0.##", ViCulture) + " giờ";
            _canEditLabor = CanEditMeeting(meeting);
            btnSaveLabor.Visible = _canEditLabor && totalPeople > 0;
            phNoPeople.Visible = matchingPeople == 0;
            Literal noPeople = FindChildControl(this, "litNoLaborPeople") as Literal;
            if (noPeople != null) noPeople.Text = totalPeople == 0
                ? "Cuộc họp này chưa có nhân viên." : "Không tìm thấy nhân viên phù hợp với bộ lọc.";
            BindFilterState();
            rptLabor.DataSource = rows;
            rptLabor.DataBind();
            Label resultLabel = LaborErrorLabel;
            if (resultLabel != null) resultLabel.CssClass = "mlt-error";
            SetLaborError(string.Empty);
            SetTabError(string.Empty);
            pnlSelectedMeeting.Visible = true;
            pnlNoSelected.Visible = false;
        }

        // Đồng bộ bảng màu và cách xử lý avatar lỗi với CostList,
        // màu avatar tự tạo ổn định theo UserId (không đổi khi phân trang/lọc).
        private string BuildLaborAvatarHtml(GioCongNhanVienRow row)
        {
            string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
            string color = colors[row.IdNhanVien.ToByteArray()[0] % colors.Length];
            string name = row.TenNhanVien ?? string.Empty;
            string[] words = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = words.Length == 0 ? "?" :
                (words.Length == 1 ? words[0].Substring(0, 1) :
                words[words.Length - 2].Substring(0, 1) + words[words.Length - 1].Substring(0, 1));
            string safeInitials = HttpUtility.HtmlEncode(initials.ToUpperInvariant());
            string safeName = HttpUtility.HtmlAttributeEncode(name).Replace("'", "&#39;");
            string avatar = (row.Avatar ?? string.Empty).Trim();
            string url = string.Empty;
            if (!string.IsNullOrWhiteSpace(avatar) &&
                !avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase))
            {
                if (avatar.StartsWith("~/", StringComparison.Ordinal))
                    url = ResolveUrl(avatar);
                else if (avatar.StartsWith("/", StringComparison.Ordinal) && !avatar.StartsWith("//", StringComparison.Ordinal))
                    url = avatar;
                else if (avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    url = avatar;
                else if (!avatar.Contains(":") && !avatar.StartsWith("//", StringComparison.Ordinal))
                    url = ResolveUrl("~/" + avatar.TrimStart('/'));
            }

            string fallback = "<span class='mlt-person-initials' style='background:" + color + ";" +
                              (url.Length > 0 ? "display:none;" : "display:flex;") + "'>" + safeInitials + "</span>";
            string photo = url.Length == 0 ? string.Empty :
                "<img src='" + HttpUtility.HtmlAttributeEncode(url).Replace("'", "&#39;") + "' alt='" + safeName +
                "' loading='lazy' onerror=\"this.style.display='none';this.nextElementSibling.style.display='flex';\" />";
            return "<span class='mlt-person-avatar' style='background:" + color + ";' title='" + safeName + "'>" +
                   photo + fallback + "</span>";
        }

        protected void rptLabor_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            GioCongNhanVienRow row = (GioCongNhanVienRow)e.Item.DataItem;
            ((HiddenField)e.Item.FindControl("hfUserId")).Value = row.IdNhanVien.ToString("D");
            ((Label)e.Item.FindControl("lblName")).Text = HttpUtility.HtmlEncode(row.TenNhanVien);
            ((Label)e.Item.FindControl("lblEmail")).Text = HttpUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(row.Email) ? "Chưa cập nhật email" : row.Email);
            ((Literal)e.Item.FindControl("litAvatar")).Text = BuildLaborAvatarHtml(row);

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
            SetLaborError(string.Empty);
            ShowNotify(text, MSGType.Error);
            upLaborTab.Update();
        }

        protected void btnSaveLabor_Click(object sender, EventArgs e)
        {
            if (SaveResultField != null) SaveResultField.Value = "error";
            Guid projectId = ProjectId;
            Guid meetingId = SelectedMeetingId;
            TblLichHop meeting;
            if (!CanViewLabor ||
                !IsValidMeeting(meetingId, projectId, out meeting) ||
                !CanEditMeeting(meeting) ||
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
                    int totalPeople, joinedPeople;
                    if (!CanAccessMeeting(connection, projectId, meetingId,
                        SweetContext.Current.UserId, CanManageProject))
                    {
                        ShowSaveError("Bạn không có quyền chấm công cuộc họp này.");
                        return;
                    }
                    LoadRosterSummary(connection, meetingId, out totalPeople, out joinedPeople);
                    int matchingCount = CountFilteredRoster(connection, meetingId, StaffSearch, StaffAttendance);
                    int pageCount = Math.Max(1, (matchingCount + LaborPerPage - 1) / LaborPerPage);
                    if (LaborPage > pageCount)
                    {
                        ShowSaveError("Danh sách nhân viên đã thay đổi. Vui lòng tải lại trước khi lưu.");
                        return;
                    }
                    List<GioCongNhanVienRow> authorized = LoadRosterPage(connection, meetingId,
                        DurationHours(meeting), LaborPage, StaffSearch, StaffAttendance);
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
                SetLaborError(string.Empty);
                if (SaveResultField != null) SaveResultField.Value = "success";
                ShowNotify("Đã lưu giờ công của cuộc họp.", MSGType.Success);
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
