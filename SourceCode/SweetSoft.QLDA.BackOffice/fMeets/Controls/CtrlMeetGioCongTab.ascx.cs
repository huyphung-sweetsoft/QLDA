using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
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
            el.Attributes["data-focus-id"] = FocusedMeetingId == Guid.Empty ? string.Empty : FocusedMeetingId.ToString("D");
            el.Attributes["data-focus-pulse"] = Convert.ToString(ViewState["GioCongFocusPulse"], CultureInfo.InvariantCulture) ?? string.Empty;
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

        // Khi di tu chi tiet cuoc hop sang tab Gio cong, loc theo ID de chon dung
        // ngay ca khi co nhieu cuoc hop trung ten.
        private Guid FocusedMeetingId
        {
            get { return ViewState["GioCongFocusedMeetId"] is Guid ? (Guid)ViewState["GioCongFocusedMeetId"] : Guid.Empty; }
            set { ViewState["GioCongFocusedMeetId"] = value; }
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
            public bool DaXacNhanChamCong { get; set; }
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
            public DateTime? GioVaoThucTe { get; set; }
            public DateTime? GioRaThucTe { get; set; }
        }

        private sealed class GioCongInput
        {
            public bool ThamGia;
            public decimal? SoGio;
            public DateTime? GioVao;
            public DateTime? GioRa;
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

        // Lay gio cuoc hop thuc te bang ADO.NET vi model SubSonic chua regenerate.
        private static void ReadMeetActual(SqlConnection connection, Guid meetId,
            out DateTime? startActual, out DateTime? endActual, out bool confirmed)
        {
            const string sql = @"SELECT ThoiGianBatDauThucTe, ThoiGianKetThucThucTe,
ISNULL(DaXacNhanChamCong,0) AS DaXacNhanChamCong
FROM dbo.TblLichHop WHERE IdLichHop=@IdLichHop";
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.Add(GuidParameter("@IdLichHop", meetId));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) throw new DataException("Khong tim thay cuoc hop.");
                    startActual = reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0);
                    endActual = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                    confirmed = reader.GetBoolean(2);
                }
            }
        }
        private static void EffectiveMeetTimes(TblLichHop meeting, DateTime? startActual, DateTime? endActual,
            out DateTime start, out DateTime end)
        {
            // Chi dung thuc te khi DU CA HAI dau moc hop le.
            start = startActual.HasValue && endActual.HasValue && endActual > startActual
                ? startActual.Value : meeting.ThoiGianBatDau;
            end = startActual.HasValue && endActual.HasValue && endActual > startActual
                ? endActual.Value : meeting.ThoiGianKetThuc;
        }
        private static decimal DurationHours(DateTime start, DateTime end)
        {
            double minutes = (end - start).TotalMinutes;
            return minutes <= 0 ? 0m : Math.Round((decimal)minutes / 60m, 2, MidpointRounding.AwayFromZero);
        }
        private static int DurationMinutes(DateTime start, DateTime end)
        {
            double minutes = (end - start).TotalMinutes;
            return minutes <= 0 ? 0 : (int)Math.Round(minutes, MidpointRounding.AwayFromZero);
        }
        private static decimal DurationHours(TblLichHop meeting)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                DateTime? actualStart, actualEnd; bool confirmed;
                ReadMeetActual(connection, meeting.IdLichHop, out actualStart, out actualEnd, out confirmed);
                DateTime start, end; EffectiveMeetTimes(meeting, actualStart, actualEnd, out start, out end);
                return DurationHours(start, end);
            }
        }
        private static int DurationMinutes(TblLichHop meeting)
        {
            return (int)Math.Round((decimal)(meeting.ThoiGianKetThuc - meeting.ThoiGianBatDau).TotalMinutes, 0);
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
           ISNULL(CAST(g.DaThamGia AS INT),
                 ISNULL((SELECT MAX(CAST(ISNULL(p.XacNhanThamGia, 0) AS INT))
                         FROM dbo.TblLichHop_NhanVien p
                         WHERE p.IdLichHop = @IdLichHop AND p.IdNhanVien = d.IdNhanVien), 0)) AS XacNhanThamGia,
           g.SoGioThucTe, g.GioVaoThucTe, g.GioRaThucTe
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
SELECT IdNhanVien, TenNhanVien, Email, Avatar, LaChuTri, XacNhanThamGia,
       SoGioThucTe, GioVaoThucTe, GioRaThucTe
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
                        DateTime? checkIn = reader["GioVaoThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["GioVaoThucTe"], CultureInfo.InvariantCulture);
                        DateTime? checkOut = reader["GioRaThucTe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["GioRaThucTe"], CultureInfo.InvariantCulture);
                        if (joined && checkIn.HasValue && checkOut.HasValue && checkOut > checkIn)
                            actual = Math.Round((decimal)(checkOut.Value - checkIn.Value).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
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
                            SoGioThucTe = actual,
                            GioVaoThucTe = checkIn,
                            GioRaThucTe = checkOut
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
            string mode, Guid userId, bool canManage, int requestedPage, string searchName, Guid focusedId,
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
   AND (@ToExclusive IS NULL OR m.ThoiGianBatDau < @ToExclusive)
   AND (@FocusId IS NULL OR m.IdLichHop=@FocusId)";
            int code = mode == "all" ? 0 : mode == "active" ? 1 : mode == "hosted" ? 3 : 2;
            Action<SqlCommand> addFilters = command =>
            {
                command.Parameters.Add("@NameSearch", SqlDbType.NVarChar, 120).Value = searchName ?? "";
                command.Parameters.Add("@NamePattern", SqlDbType.NVarChar, 260).Value = LikePattern(searchName);
                command.Parameters.Add("@FocusId", SqlDbType.UniqueIdentifier).Value = focusedId == Guid.Empty ? (object)DBNull.Value : focusedId;
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
 SELECT m.IdLichHop, m.TenCuocHop, m.ThoiGianBatDau, m.ThoiGianKetThuc, m.DaXacNhanChamCong,
        CASE WHEN m.IdNguoiTao=@UserId THEN 1 ELSE 0 END AS LaChuTri,
        ROW_NUMBER() OVER (ORDER BY m.ThoiGianBatDau DESC, m.IdLichHop DESC) AS SoThuTu" + where + @"
)
SELECT IdLichHop, TenCuocHop, ThoiGianBatDau, ThoiGianKetThuc, LaChuTri, DaXacNhanChamCong
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
                            LaChuTri = Convert.ToInt32(reader["LaChuTri"], CultureInfo.InvariantCulture) == 1,
                            DaXacNhanChamCong = Convert.ToInt32(reader["DaXacNhanChamCong"], CultureInfo.InvariantCulture) == 1
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

        protected string MeetingLaborConfirmBadge(object end, object confirmed)
        {
            DateTime finish = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            if (finish > DateTime.Now) return string.Empty;
            return Convert.ToBoolean(confirmed, CultureInfo.InvariantCulture)
                ? "<span class=\"mlt-accounted-ok\">Đã chấm công</span>"
                : "<span class=\"mlt-accounted-pending\"><i class=\"fas fa-exclamation-circle\"></i> Chưa chấm công</span>";
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
            LinkButton saveTimes = FindChildControl(this, "btnSaveMeetTimes") as LinkButton;
            LinkButton confirm = FindChildControl(this, "btnConfirmMeetLabor") as LinkButton;
            if (saveTimes != null) manager.RegisterAsyncPostBackControl(saveTimes);
            if (confirm != null) manager.RegisterAsyncPostBackControl(confirm);
            LinkButton saveEmployeeHours = FindChildControl(this, "btnSaveEmployeeHours") as LinkButton;
            if (saveEmployeeHours != null) manager.RegisterAsyncPostBackControl(saveEmployeeHours);
            foreach (RepeaterItem item in rptLabor.Items)
            {
                LinkButton openEmployeeHours = item.FindControl("btnOpenEmployeeHours") as LinkButton;
                if (openEmployeeHours != null) manager.RegisterAsyncPostBackControl(openEmployeeHours);
            }
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
            // Di chuyen tu ViewMeetDetail: uu tien xu ly theo IdLichHop
            // trong mot async postback, khong phu thuoc trang hien tai cua bang trai.
            Guid focusId;
            string focusRaw = Request.Form["QLDA_MeetLabor_FocusId"];
            if (Guid.TryParse(focusRaw, out focusId) && focusId != Guid.Empty)
            {
                TblLichHop focusMeet;
                if (!IsValidMeeting(focusId, ProjectId, out focusMeet))
                { ShowNotify("Không tìm thấy cuộc họp cần xem giờ công.", MSGType.Warning); return; }
                using (SqlConnection db = new SqlConnection(GetConnectionString()))
                {
                    db.Open();
                    if (!CanAccessMeeting(db, ProjectId, focusId, SweetContext.Current.UserId, CanManageProject))
                    { ShowNotify("Bạn không có quyền xem giờ công của cuộc họp này.", MSGType.Warning); return; }
                }
                MeetingFilter = "all";
                MeetingSearch = SafeSearch(focusMeet.TenCuocHop);
                DateFrom = null; DateTo = null;
                MeetingPage = 1; LaborPage = 1;
                FocusedMeetingId = focusId;
                SelectedMeetingId = focusId;
                ViewState["GioCongFocusPulse"] = Guid.NewGuid().ToString("N");
                BindTab();
                return;
            }
            // Filter action goes through this same stable async control to keep the unsaved-change guard.
            string filterAction = Request.Form["QLDA_MeetLabor_FilterAction"];
            string filterValue = Request.Form["QLDA_MeetLabor_FilterValue"];
            if (!string.IsNullOrWhiteSpace(filterAction))
            {
                if (filterAction == "meetingMode" || filterAction == "meetingSearch" || filterAction == "range" || filterAction == "clearRange")
                    FocusedMeetingId = Guid.Empty;
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
            FocusedMeetingId = Guid.Empty;
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
                        MeetingSearch, FocusedMeetingId, DateFrom, DateTo, out total, out actualPage);
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

            decimal duration;
            DateTime effectiveStart, effectiveEnd;
            DateTime? savedStart, savedEnd; bool isConfirmed;
            List<GioCongNhanVienRow> rows;
            int totalPeople, joinedPeople, matchingPeople;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                ReadMeetActual(connection, meetingId, out savedStart, out savedEnd, out isConfirmed);
                EffectiveMeetTimes(meeting, savedStart, savedEnd, out effectiveStart, out effectiveEnd);
                duration = DurationHours(effectiveStart, effectiveEnd);
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
            litDuration.Text = DurationMinutes(effectiveStart, effectiveEnd).ToString(CultureInfo.InvariantCulture) + " phút = " + duration.ToString("0.##", ViCulture) + " giờ công";
            TextBox timeStart = FindChildControl(this, "txtMeetStartActual") as TextBox;
            TextBox timeEnd = FindChildControl(this, "txtMeetEndActual") as TextBox;
            LinkButton saveTime = FindChildControl(this, "btnSaveMeetTimes") as LinkButton;
            LinkButton confirmButton = FindChildControl(this, "btnConfirmMeetLabor") as LinkButton;
            Literal confirmedInfo = FindChildControl(this, "litLaborConfirm") as Literal;
            HtmlGenericControl confirmStrip = FindChildControl(this, "divLaborConfirm") as HtmlGenericControl;
            Literal actualTimeSummary = FindChildControl(this, "litActualTimeRange") as Literal;
            DateTime displayStart = savedStart ?? meeting.ThoiGianBatDau;
            DateTime displayEnd = savedEnd ?? meeting.ThoiGianKetThuc;
            ViewState["GioCongEffectiveStartHm"] = effectiveStart.ToString("HH:mm", CultureInfo.InvariantCulture);
            ViewState["GioCongEffectiveEndHm"] = effectiveEnd.ToString("HH:mm", CultureInfo.InvariantCulture);
            if (timeStart != null) { timeStart.Text = displayStart.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture); timeStart.Enabled = CanEditMeeting(meeting); }
            if (timeEnd != null) { timeEnd.Text = displayEnd.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture); timeEnd.Enabled = CanEditMeeting(meeting); }
            if (saveTime != null) saveTime.Visible = CanEditMeeting(meeting);
            if (confirmedInfo != null) confirmedInfo.Text = isConfirmed ? "Đã xác nhận chấm công cuộc họp." : "Cuộc họp đã kết thúc nhưng CHƯA xác nhận chấm công.";
            if (confirmStrip != null) confirmStrip.Style["display"] = "none";
            if (actualTimeSummary != null) actualTimeSummary.Text = HttpUtility.HtmlEncode(displayStart.ToString("HH:mm", ViCulture) + " – " + displayEnd.ToString("HH:mm", ViCulture));
            if (confirmButton != null) { confirmButton.Visible = CanEditMeeting(meeting) && !isConfirmed; }
            pnlSelectedMeeting.Attributes["data-meeting-start"] = effectiveStart.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            pnlSelectedMeeting.Attributes["data-meeting-end"] = effectiveEnd.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            pnlSelectedMeeting.Attributes["data-effective-start-hm"] = effectiveStart.ToString("HH:mm", CultureInfo.InvariantCulture);
            pnlSelectedMeeting.Attributes["data-effective-end-hm"] = effectiveEnd.ToString("HH:mm", CultureInfo.InvariantCulture);
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
                ? (row.SoGioThucTe.HasValue ? row.SoGioThucTe.Value.ToString("0.##", ViCulture) : CurrentDurationInvariant.Replace('.', ','))
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
            TextBox timeIn = e.Item.FindControl("txtTimeIn") as TextBox;
            TextBox timeOut = e.Item.FindControl("txtTimeOut") as TextBox;
            LinkButton openEmployeeHours = e.Item.FindControl("btnOpenEmployeeHours") as LinkButton;
            if (openEmployeeHours != null) openEmployeeHours.Enabled = _canEditLabor && row.XacNhanThamGia;
            HiddenField mode = e.Item.FindControl("hfTimeMode") as HiddenField;
            string defaultStartHm = Convert.ToString(ViewState["GioCongEffectiveStartHm"], CultureInfo.InvariantCulture) ?? string.Empty;
            string defaultEndHm = Convert.ToString(ViewState["GioCongEffectiveEndHm"], CultureInfo.InvariantCulture) ?? string.Empty;
            if (timeIn != null) timeIn.Text = row.GioVaoThucTe.HasValue ? row.GioVaoThucTe.Value.ToString("HH:mm", CultureInfo.InvariantCulture) : defaultStartHm;
            if (timeOut != null) timeOut.Text = row.GioRaThucTe.HasValue ? row.GioRaThucTe.Value.ToString("HH:mm", CultureInfo.InvariantCulture) : defaultEndHm;
            if (mode != null) mode.Value = !row.XacNhanThamGia ? "default"
                : row.GioVaoThucTe.HasValue && row.GioRaThucTe.HasValue ? "time"
                : row.SoGioThucTe.HasValue ? "manual" : "default";
        }

        // ===== Nhập giờ vào/ra bằng ExtraModal, ghi trực tiếp đúng một nhân viên =====
        private static T EmployeeHoursControl<T>(Control root, string id) where T : Control
        {
            return FindChildControl(root, id) as T;
        }

        private bool ValidateEmployeeHoursAccess(Guid meetingId, Guid userId,
            out TblLichHop meeting, out DateTime effectiveStart, out DateTime effectiveEnd,
            out GioCongNhanVienRow member)
        {
            meeting = null; member = null;
            effectiveStart = DateTime.MinValue; effectiveEnd = DateTime.MinValue;
            if (!CanViewLabor || ProjectId == Guid.Empty || meetingId == Guid.Empty || userId == Guid.Empty ||
                SelectedMeetingId != meetingId ||
                !string.Equals(hfSelectedMeetId.Value, meetingId.ToString("D"), StringComparison.OrdinalIgnoreCase) ||
                !IsValidMeeting(meetingId, ProjectId, out meeting) || !CanEditMeeting(meeting)) return false;
            using (SqlConnection db = new SqlConnection(GetConnectionString()))
            {
                db.Open();
                if (!CanAccessMeeting(db, ProjectId, meetingId, SweetContext.Current.UserId, CanManageProject)) return false;
                DateTime? savedStart, savedEnd; bool isConfirmed;
                ReadMeetActual(db, meetingId, out savedStart, out savedEnd, out isConfirmed);
                EffectiveMeetTimes(meeting, savedStart, savedEnd, out effectiveStart, out effectiveEnd);
                var memberRows = LoadRosterPage(db, meetingId, DurationHours(effectiveStart, effectiveEnd),
                    LaborPage, StaffSearch, StaffAttendance);
                member = memberRows.FirstOrDefault(x => x.IdNhanVien == userId && x.XacNhanThamGia);
                return member != null;
            }
        }

        private void OpenEmployeeHoursExtraModal()
        {
            // Dùng đúng API của SweetSoft:ExtraModal như CtrlViewMeetDetail/CtrlTodaySchedule.
            // Không dùng Reflection: GetMethod("OpenModal", typeof(bool)) không đáng tin cậy
            // khi ExtraModal có các overload hoặc chữ ký khác với kiểu runtime.
            ExtraModal modal = FindChildControl(this, "mdlEmployeeHours") as ExtraModal;
            if (modal == null)
                throw new InvalidOperationException("Không tìm thấy ExtraModal mdlEmployeeHours. Kiểm tra khai báo control trong ASCX.");

            modal.OpenModal(true);
        }

        protected void btnOpenEmployeeHours_Click(object sender, EventArgs e)
        {
            LinkButton trigger = sender as LinkButton;
            Guid userId;
            TblLichHop meeting; DateTime start, end; GioCongNhanVienRow member;
            if (trigger == null || !Guid.TryParse(trigger.CommandArgument, out userId) ||
                !ValidateEmployeeHoursAccess(SelectedMeetingId, userId, out meeting, out start, out end, out member))
            {
                ShowNotify("Không thể chỉnh giờ. Nhân viên chưa được lưu trạng thái tham gia hoặc bạn không có quyền.", MSGType.Error);
                return;
            }
            try
            {
                var meetingField = EmployeeHoursControl<HiddenField>(this, "hfEmployeeHoursMeetId");
                var userField = EmployeeHoursControl<HiddenField>(this, "hfEmployeeHoursUserId");
                var memberName = EmployeeHoursControl<Literal>(this, "litEmployeeHoursName");
                var meetRange = EmployeeHoursControl<Literal>(this, "litEmployeeHoursMeetingTime");
                var timeIn = EmployeeHoursControl<TextBox>(this, "txtEmployeeHoursIn");
                var timeOut = EmployeeHoursControl<TextBox>(this, "txtEmployeeHoursOut");
                var error = EmployeeHoursControl<Label>(this, "lblEmployeeHoursError");
                if (meetingField == null || userField == null || memberName == null || meetRange == null || timeIn == null || timeOut == null)
                    throw new InvalidOperationException("Thiếu control nhập giờ nhân viên trong ExtraModal.");
                meetingField.Value = meeting.IdLichHop.ToString("D");
                userField.Value = userId.ToString("D");
                memberName.Text = HttpUtility.HtmlEncode(member.TenNhanVien);
                meetRange.Text = "Giờ họp: " + HttpUtility.HtmlEncode(start.ToString("dd/MM/yyyy HH:mm", ViCulture) + " – " +
                    end.ToString("dd/MM/yyyy HH:mm", ViCulture));
                timeIn.Text = (member.GioVaoThucTe ?? start).ToString("HH:mm", CultureInfo.InvariantCulture);
                timeOut.Text = (member.GioRaThucTe ?? end).ToString("HH:mm", CultureInfo.InvariantCulture);
                if (error != null) error.Text = string.Empty;
                UpdatePanel up = EmployeeHoursControl<UpdatePanel>(this, "upEmployeeHours");
                if (up != null) up.Update();
                OpenEmployeeHoursExtraModal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CtrlMeetGioCongTab.OpenEmployeeHours: Meet=" +
                    SelectedMeetingId.ToString("D") + "; Employee=" + userId.ToString("D") + "; " + ex);
                ShowNotify("Không mở được popup giờ vào/ra. Kiểm tra log CtrlMeetGioCongTab.OpenEmployeeHours trên máy chủ.", MSGType.Error);
            }
        }

        protected void btnSaveEmployeeHours_Click(object sender, EventArgs e)
        {
            HiddenField meetField = EmployeeHoursControl<HiddenField>(this, "hfEmployeeHoursMeetId");
            HiddenField userField = EmployeeHoursControl<HiddenField>(this, "hfEmployeeHoursUserId");
            TextBox timeIn = EmployeeHoursControl<TextBox>(this, "txtEmployeeHoursIn");
            TextBox timeOut = EmployeeHoursControl<TextBox>(this, "txtEmployeeHoursOut");
            Label error = EmployeeHoursControl<Label>(this, "lblEmployeeHoursError");
            Guid meetId, userId;
            if (meetField == null || userField == null || timeIn == null || timeOut == null ||
                !Guid.TryParse(meetField.Value, out meetId) || !Guid.TryParse(userField.Value, out userId))
            {
                ShowNotify("Dữ liệu nhân viên không hợp lệ. Hãy mở lại popup.", MSGType.Error); return;
            }
            TblLichHop meeting; DateTime start, end; GioCongNhanVienRow member;
            if (!ValidateEmployeeHoursAccess(meetId, userId, out meeting, out start, out end, out member))
            {
                ShowNotify("Nhân viên không còn hợp lệ hoặc không có quyền chỉnh giờ.", MSGType.Error); return;
            }
            // Chỉ nhận HH:mm (24 giờ), không chấp nhận văn bản hay giây.
            DateTime checkedIn, checkedOut;
            string rawIn = Request.Form[timeIn.UniqueID] ?? timeIn.Text;
            string rawOut = Request.Form[timeOut.UniqueID] ?? timeOut.Text;
            if (!TryEmployeeInterval(start, end, rawIn, rawOut, out checkedIn, out checkedOut))
            {
                if (error != null) error.Text = "Giờ vào/ra phải nằm trong thời gian họp và giờ ra phải sau giờ vào (HH:mm).";
                UpdatePanel update = EmployeeHoursControl<UpdatePanel>(this, "upEmployeeHours");
                if (update != null) update.Update();
                ShowNotify("Giờ vào/ra không hợp lệ hoặc nằm ngoài thời gian họp.", MSGType.Error);
                return;
            }
            decimal hours = Math.Round((decimal)(checkedOut - checkedIn).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
            try
            {
                using (SqlConnection db = new SqlConnection(GetConnectionString()))
                {
                    db.Open();
                    using (SqlTransaction tran = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        // UPDATE/INSERT một nhân viên; không sửa những người còn lại hay cờ xác nhận của cuộc họp.
                        const string sql = @"
UPDATE dbo.TblLichHopGioCong WITH (UPDLOCK, SERIALIZABLE)
   SET SoGioThucTe=@Hours, GioVaoThucTe=@CheckIn, GioRaThucTe=@CheckOut,
       DaThamGia=1, NguoiCapNhat=@UserId, NgayCapNhat=GETDATE()
 WHERE IdLichHop=@MeetId AND IdNhanVien=@EmployeeId;
IF @@ROWCOUNT=0
   INSERT INTO dbo.TblLichHopGioCong
       (IdLichHop, IdNhanVien, SoGioThucTe, DaThamGia, GioVaoThucTe, GioRaThucTe, NguoiCapNhat, NgayCapNhat)
   VALUES (@MeetId, @EmployeeId, @Hours, 1, @CheckIn, @CheckOut, @UserId, GETDATE());";
                        using (SqlCommand cmd = new SqlCommand(sql, db, tran))
                        {
                            cmd.Parameters.Add(GuidParameter("@MeetId", meetId));
                            cmd.Parameters.Add(GuidParameter("@EmployeeId", userId));
                            cmd.Parameters.Add(GuidParameter("@UserId", SweetContext.Current.UserId));
                            SqlParameter h = cmd.Parameters.Add("@Hours", SqlDbType.Decimal); h.Precision = 10; h.Scale = 2; h.Value = hours;
                            cmd.Parameters.Add("@CheckIn", SqlDbType.DateTime2).Value = checkedIn;
                            cmd.Parameters.Add("@CheckOut", SqlDbType.DateTime2).Value = checkedOut;
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                }
                BindSelectedMeeting(meetId);
                if (SaveResultField != null) SaveResultField.Value = "success";
                upLaborTab.Update();
                if (error != null) error.Text = string.Empty;
                UpdatePanel updateEmployee = EmployeeHoursControl<UpdatePanel>(this, "upEmployeeHours");
                if (updateEmployee != null) updateEmployee.Update();
                Control modal = FindChildControl(this, "mdlEmployeeHours");
                if (modal != null)
                {
                    string id = HttpUtility.JavaScriptStringEncode(modal.ClientID);
                    string script = "(function(){var el=document.getElementById('" + id + "');if(!el)return;" +
                        "try{if(window.bootstrap&&bootstrap.Modal){var m=bootstrap.Modal.getInstance(el);if(m){m.hide();return;}}" +
                        "if(window.jQuery&&jQuery.fn.modal){jQuery(el).modal('hide');return;}}catch(e){}" +
                        "el.classList.remove('show','in');el.style.display='none';})();";
                    ScriptManager.RegisterStartupScript(upLaborTab, upLaborTab.GetType(), "LaborEmployeeHoursClose", script, true);
                }
                ShowNotify("Đã lưu giờ vào/ra và cập nhật công thực tế: " + hours.ToString("0.##", ViCulture) + " giờ.", MSGType.Success);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CtrlMeetGioCongTab.SaveEmployeeHours: " + ex);
                if (error != null) error.Text = "Không lưu được giờ công. Hãy thử lại hoặc kiểm tra log server.";
                UpdatePanel up = EmployeeHoursControl<UpdatePanel>(this, "upEmployeeHours");
                if (up != null) up.Update();
                ShowNotify("Lưu giờ vào/ra thất bại. Kiểm tra log server.", MSGType.Error);
            }
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

        private static bool TryEmployeeInterval(DateTime meetStart, DateTime meetEnd,
            string startValue, string endValue, out DateTime checkIn, out DateTime checkOut)
        {
            checkIn = DateTime.MinValue; checkOut = DateTime.MinValue;
            DateTime inClock, outClock;
            if (!DateTime.TryParseExact(startValue, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out inClock) ||
                !DateTime.TryParseExact(endValue, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out outClock)) return false;
            checkIn = meetStart.Date.Add(inClock.TimeOfDay);
            while (checkIn < meetStart) checkIn = checkIn.AddDays(1);
            checkOut = checkIn.Date.Add(outClock.TimeOfDay);
            if (checkOut <= checkIn) checkOut = checkOut.AddDays(1);
            return checkIn >= meetStart && checkOut <= meetEnd && checkOut > checkIn;
        }
        private static bool TryMeetingDateTime(string raw, out DateTime value)
        {
            return DateTime.TryParseExact((raw ?? "").Trim(), "yyyy-MM-ddTHH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        // Sau khi luu chinh sua, cap nhat huy hieu o danh sach trai ngay ca khi
        // Repeater cu khong DataBind lai (tranh lam mat bo loc/vi tri cuon).
        private void MarkActiveMeetingPending()
        {
            ScriptManager.RegisterStartupScript(upLaborTab, upLaborTab.GetType(),
                "MeetLaborPending" + Guid.NewGuid().ToString("N"),
                @"setTimeout(function(){
                    var card=document.querySelector('.meet-panel-labor .mlt-meeting-link.is-active');
                    if(!card)return;
                    var ok=card.querySelector('.mlt-accounted-ok');
                    if(ok){ok.outerHTML='<span class=""mlt-accounted-pending"" title=""Cần xác nhận lại sau khi chỉnh sửa"">Chưa chấm công</span>';}
                },70);", true);
        }

        // Luu gio bat dau/ket thuc thuc te, khong tu danh dau da cham cong.
        protected void btnSaveMeetTimes_Click(object sender, EventArgs e)
        {
            Guid meetId = SelectedMeetingId; TblLichHop meet;
            if (!CanViewLabor || !IsValidMeeting(meetId, ProjectId, out meet) ||
                !CanEditMeeting(meet) || hfSelectedMeetId.Value != meetId.ToString("D"))
            { ShowSaveError("Bạn không có quyền sửa thời gian cuộc họp."); return; }
            TextBox from = FindChildControl(this, "txtMeetStartActual") as TextBox;
            TextBox to = FindChildControl(this, "txtMeetEndActual") as TextBox;
            DateTime start, end;
            string fromRaw = from == null ? "" : Request.Form[from.UniqueID] ?? from.Text;
            string toRaw = to == null ? "" : Request.Form[to.UniqueID] ?? to.Text;
            if (!TryMeetingDateTime(fromRaw, out start) || !TryMeetingDateTime(toRaw, out end) ||
                end <= start || (end - start).TotalDays > 7 || end > DateTime.Now.AddMinutes(1))
            { ShowSaveError("Giờ thực tế không hợp lệ: kết thúc phải sau bắt đầu, không quá 7 ngày và không nằm trong tương lai."); return; }
            bool timesChanged = false;
            try
            {
                using (SqlConnection db = new SqlConnection(GetConnectionString()))
                {
                    db.Open();
                    if (!CanAccessMeeting(db, ProjectId, meetId, SweetContext.Current.UserId, CanManageProject))
                    { ShowSaveError("Không có quyền lưu cuộc họp này."); return; }
                    // Gio cong mac dinh duoc luu bang NULL de tu ke thua thoi luong hop thuc te.
                    // Du lieu cu co the da luu so gio bang gio ly thuyet: chuyen nhung dong khop
                    // dung gio ly thuyet CU sang NULL; khong cham vao dong co gio vao/ra rieng,
                    // hoac so gio thu cong KHAC gio ly thuyet cu.
                    DateTime? oldSavedStart, oldSavedEnd; bool oldConfirmed;
                    ReadMeetActual(db, meetId, out oldSavedStart, out oldSavedEnd, out oldConfirmed);
                    DateTime oldStart, oldEnd;
                    EffectiveMeetTimes(meet, oldSavedStart, oldSavedEnd, out oldStart, out oldEnd);
                    decimal previousHours = DurationHours(oldStart, oldEnd);
                    decimal plannedHours = DurationHours(meet.ThoiGianBatDau, meet.ThoiGianKetThuc);
                    timesChanged = oldStart != start || oldEnd != end;
                    using (SqlTransaction tx = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        const string sql = @"UPDATE dbo.TblLichHop
SET ThoiGianBatDauThucTe=@Start, ThoiGianKetThucThucTe=@End,
    DaXacNhanChamCong=CASE WHEN @Changed=1 THEN 0 ELSE DaXacNhanChamCong END,
    IdNguoiCapNhat=@UserId, NgayCapNhat=GETDATE()
WHERE IdLichHop=@MeetId AND IdDuAn=@ProjectId AND ISNULL(DaXoa,0)=0";
                        using (SqlCommand command = new SqlCommand(sql, db, tx))
                        {
                            command.Parameters.Add(GuidParameter("@MeetId", meetId));
                            command.Parameters.Add(GuidParameter("@ProjectId", ProjectId));
                            command.Parameters.Add(GuidParameter("@UserId", SweetContext.Current.UserId));
                            command.Parameters.Add("@Start", SqlDbType.DateTime2).Value = start;
                            command.Parameters.Add("@End", SqlDbType.DateTime2).Value = end;
                            command.Parameters.Add("@Changed", SqlDbType.Bit).Value = timesChanged;
                            if (command.ExecuteNonQuery() != 1) throw new DataException("Cuoc hop khong ton tai.");
                        }
                        // Cho phep dong bo lai nhung cuoc hop da sua gio truoc khi cai ban fix,
                        // nhung khong cham vao du lieu da xac nhan neu gio khong thay doi.
                        if (timesChanged || (!oldConfirmed && previousHours != plannedHours))
                        {
                            const string setDefaults = @"
UPDATE dbo.TblLichHopGioCong
SET SoGioThucTe=NULL
WHERE IdLichHop=@MeetId
  AND (DaThamGia=1 OR (DaThamGia IS NULL AND EXISTS (
       SELECT 1 FROM dbo.TblLichHop_NhanVien nv
       WHERE nv.IdLichHop=@MeetId AND nv.IdNhanVien=TblLichHopGioCong.IdNhanVien
         AND nv.XacNhanThamGia=1)))
  AND GioVaoThucTe IS NULL AND GioRaThucTe IS NULL
  AND (SoGioThucTe=@PreviousHours OR SoGioThucTe=@PlannedHours);";
                            using (SqlCommand cmd = new SqlCommand(setDefaults, db, tx))
                            {
                                cmd.Parameters.Add(GuidParameter("@MeetId", meetId));
                                SqlParameter pOld = cmd.Parameters.Add("@PreviousHours", SqlDbType.Decimal);
                                pOld.Precision = 10; pOld.Scale = 2; pOld.Value = previousHours;
                                SqlParameter pPlanned = cmd.Parameters.Add("@PlannedHours", SqlDbType.Decimal);
                                pPlanned.Precision = 10; pPlanned.Scale = 2; pPlanned.Value = plannedHours;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                }
                BindSelectedMeeting(meetId);
                if (SaveResultField != null) SaveResultField.Value = "success";
                if (timesChanged) MarkActiveMeetingPending();
                ShowNotify(timesChanged ? "Đã cập nhật giờ họp thực tế và đồng bộ công mặc định. Hãy kiểm tra trước khi xác nhận lại." : "Đã lưu giờ họp thực tế.", MSGType.Success);
                upLaborTab.Update();
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("SaveMeetTimes: " + ex); ShowSaveError("Không thể lưu thời gian họp thực tế."); }
        }

        protected void btnConfirmMeetLabor_Click(object sender, EventArgs e)
        {
            Guid meetId = SelectedMeetingId; TblLichHop meet;
            if (!CanViewLabor || !IsValidMeeting(meetId, ProjectId, out meet) ||
                !CanEditMeeting(meet) || hfSelectedMeetId.Value != meetId.ToString("D"))
            { ShowSaveError("Bạn không có quyền xác nhận chấm công."); return; }
            try
            {
                using (SqlConnection db = new SqlConnection(GetConnectionString()))
                {
                    db.Open();
                    if (!CanAccessMeeting(db, ProjectId, meetId, SweetContext.Current.UserId, CanManageProject))
                    { ShowSaveError("Không có quyền truy cập cuộc họp này."); return; }
                    DateTime? start, end; bool alreadyConfirmed;
                    ReadMeetActual(db, meetId, out start, out end, out alreadyConfirmed);
                    if (!start.HasValue || !end.HasValue || end <= start)
                    { ShowSaveError("Hãy lưu giờ bắt đầu và kết thúc thực tế trước khi xác nhận chấm công."); return; }
                    // Khong cho xac nhan neu van con nhan vien chua luu trang thai tham gia.
                    const string pendingSql = @"WITH DanhSach AS (
 SELECT IdNhanVien FROM dbo.TblLichHop_NhanVien WHERE IdLichHop=@IdLichHop AND IdNhanVien IS NOT NULL
 UNION SELECT IdNguoiTao FROM dbo.TblLichHop WHERE IdLichHop=@IdLichHop AND IdNguoiTao IS NOT NULL)
 SELECT COUNT(1) FROM DanhSach d LEFT JOIN dbo.TblLichHopGioCong g
 ON g.IdLichHop=@IdLichHop AND g.IdNhanVien=d.IdNhanVien WHERE g.DaThamGia IS NULL";
                    using (SqlCommand pending = new SqlCommand(pendingSql, db))
                    {
                        pending.Parameters.Add(GuidParameter("@IdLichHop", meetId));
                        int count = Convert.ToInt32(pending.ExecuteScalar(), CultureInfo.InvariantCulture);
                        if (count > 0) { ShowSaveError("Còn " + count + " nhân viên chưa lưu Có/Không tham gia. Vui lòng chấm đủ trước khi xác nhận."); return; }
                    }
                    const string sql = @"UPDATE dbo.TblLichHop SET DaXacNhanChamCong=1,
 IdNguoiCapNhat=@UserId, NgayCapNhat=GETDATE()
 WHERE IdLichHop=@MeetId AND IdDuAn=@ProjectId AND ISNULL(DaXoa,0)=0";
                    using (SqlCommand cmd = new SqlCommand(sql, db))
                    {
                        cmd.Parameters.Add(GuidParameter("@MeetId", meetId));
                        cmd.Parameters.Add(GuidParameter("@ProjectId", ProjectId));
                        cmd.Parameters.Add(GuidParameter("@UserId", SweetContext.Current.UserId));
                        if (cmd.ExecuteNonQuery() != 1) throw new DataException("Cuoc hop khong ton tai.");
                    }
                }
                BindSelectedMeeting(meetId);
                if (SaveResultField != null) SaveResultField.Value = "success";
                ShowNotify("Đã xác nhận chấm công cuộc họp.", MSGType.Success);
                // Cap nhat nhan da cham cong tai danh sach cuoc hop ben trai.
                BindTab();
                upLaborTab.Update();
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("ConfirmMeetLabor: " + ex); ShowSaveError("Không thể xác nhận chấm công."); }
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

            Dictionary<Guid, GioCongInput> posted = new Dictionary<Guid, GioCongInput>();
            DateTime effStart, effEnd;
            using (SqlConnection tdb = new SqlConnection(GetConnectionString()))
            {
                tdb.Open(); DateTime? aa, bb; bool confirmed;
                ReadMeetActual(tdb, meetingId, out aa, out bb, out confirmed);
                EffectiveMeetTimes(meeting, aa, bb, out effStart, out effEnd);
            }
            foreach (RepeaterItem item in rptLabor.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem) continue;
                Guid userId;
                HiddenField idField = item.FindControl("hfUserId") as HiddenField;
                RadioButtonList attendance = item.FindControl("rblAttendance") as RadioButtonList;
                TextBox actual = item.FindControl("txtActual") as TextBox;
                HiddenField mode = item.FindControl("hfTimeMode") as HiddenField;
                TextBox timeIn = item.FindControl("txtTimeIn") as TextBox;
                TextBox timeOut = item.FindControl("txtTimeOut") as TextBox;
                if (idField == null || attendance == null || actual == null || mode == null ||
                    timeIn == null || timeOut == null || !Guid.TryParse(idField.Value, out userId) ||
                    userId == Guid.Empty || posted.ContainsKey(userId))
                { ShowSaveError("Danh sách nhân viên không hợp lệ. Vui lòng mở lại tab."); return; }
                string selected = Request.Form[attendance.UniqueID];
                if (selected != "1" && selected != "0")
                { ShowSaveError("Vui lòng chọn Có hoặc Không tham gia cho từng nhân viên."); return; }
                GioCongInput entry = new GioCongInput { ThamGia = selected == "1", SoGio = 0m };
                if (entry.ThamGia)
                {
                    string selectedMode = Request.Form[mode.UniqueID] ?? mode.Value;
                    if (selectedMode == "time")
                    {
                        string startText = Request.Form[timeIn.UniqueID] ?? timeIn.Text;
                        string endText = Request.Form[timeOut.UniqueID] ?? timeOut.Text;
                        DateTime checkIn, checkOut;
                        if (!TryEmployeeInterval(effStart, effEnd, startText, endText, out checkIn, out checkOut))
                        { ShowSaveError("Giờ vào/ra của nhân viên phải nằm trong khoảng giờ họp thực tế và ra sau khi vào (HH:mm)."); return; }
                        entry.GioVao = checkIn; entry.GioRa = checkOut;
                        entry.SoGio = Math.Round((decimal)(checkOut - checkIn).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (selectedMode == "manual")
                    {
                        decimal? hours; string raw = Request.Form[actual.UniqueID] ?? actual.Text;
                        if (!TryHours(raw, out hours))
                        { ShowSaveError("Giờ công phải từ 0 đến 9999,99, tối đa 2 chữ số thập phân."); return; }
                        entry.SoGio = hours; // Chinh tay; gio vao/ra la NULL
                    }
                    else if (selectedMode == "default") entry.SoGio = null; // Fallback dong theo gio hop thuc te
                    else { ShowSaveError("Chế độ giờ công không hợp lệ."); return; }
                }
                posted.Add(userId, entry);
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
                        foreach (KeyValuePair<Guid, GioCongInput> input in posted)
                        {
                            const string sql = @"
UPDATE dbo.TblLichHopGioCong WITH (UPDLOCK, SERIALIZABLE)
SET SoGioThucTe=@SoGioThucTe, DaThamGia=@DaThamGia,
    GioVaoThucTe=@GioVaoThucTe, GioRaThucTe=@GioRaThucTe,
    NguoiCapNhat=@NguoiCapNhat, NgayCapNhat=GETDATE()
WHERE IdLichHop=@IdLichHop AND IdNhanVien=@IdNhanVien;
IF @@ROWCOUNT = 0
    INSERT INTO dbo.TblLichHopGioCong
      (IdLichHop, IdNhanVien, SoGioThucTe, DaThamGia, GioVaoThucTe, GioRaThucTe, NguoiCapNhat, NgayCapNhat)
    VALUES
      (@IdLichHop, @IdNhanVien, @SoGioThucTe, @DaThamGia, @GioVaoThucTe, @GioRaThucTe, @NguoiCapNhat, GETDATE());";
                            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                            {
                                command.Parameters.Add(GuidParameter("@IdLichHop", meetingId));
                                command.Parameters.Add(GuidParameter("@IdNhanVien", input.Key));
                                command.Parameters.Add(GuidParameter("@NguoiCapNhat", SweetContext.Current.UserId));
                                SqlParameter hoursParameter = command.Parameters.Add("@SoGioThucTe", SqlDbType.Decimal);
                                hoursParameter.Precision = 10;
                                hoursParameter.Scale = 2;
                                hoursParameter.Value = input.Value.SoGio.HasValue
                                    ? (object)input.Value.SoGio.Value : DBNull.Value;
                                command.Parameters.Add("@DaThamGia", SqlDbType.Bit).Value = input.Value.ThamGia;
                                command.Parameters.Add("@GioVaoThucTe", SqlDbType.DateTime2).Value = input.Value.GioVao.HasValue ? (object)input.Value.GioVao.Value : DBNull.Value;
                                command.Parameters.Add("@GioRaThucTe", SqlDbType.DateTime2).Value = input.Value.GioRa.HasValue ? (object)input.Value.GioRa.Value : DBNull.Value;
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
