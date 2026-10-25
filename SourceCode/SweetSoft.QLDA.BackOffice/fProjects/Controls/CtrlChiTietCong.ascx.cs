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
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Controls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlChiTietCong : BaseAdminUserControl
    {
        public Guid ProjectId
        {
            get { return ViewState["PdProjectId"] is Guid ? (Guid)ViewState["PdProjectId"] : Guid.Empty; }
            set { ViewState["PdProjectId"] = value; }
        }

        protected string ModalClientId { get { return mdlPersonalDetail.ClientID; } }
        protected string RequestTypeId { get { return hfDetailType.ClientID; } }
        protected string RequestStaffId { get { return hfDetailStaff.ClientID; } }
        protected string LoadButtonId { get { return btnLoadDetail.ClientID; } }

        private bool CanReadProject(Guid projectId, Guid userId)
        {
            if (projectId == Guid.Empty || userId == Guid.Empty) return false;
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            if (pmId.HasValue && pmId.Value == userId) return true;
            return ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(userId, projectId) != null;
        }

        private bool CanReadTeam(Guid projectId)
        {
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            return pmId.HasValue && pmId.Value == SweetContext.Current.UserId;
        }

        private bool IsAuthorizedTeamMember(Guid memberId, Guid projectId)
        {
            if (memberId == Guid.Empty || projectId == Guid.Empty) return false;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            if (pmId.HasValue && pmId.Value == memberId) return true;
            return (ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(projectId) ?? new List<Guid>()).Contains(memberId);
        }

        protected void btnLoadDetail_Click(object sender, EventArgs e)
        {
            Guid projectId = ProjectId;
            Guid actor = SweetContext.Current.UserId;
            string kind = (hfDetailType.Value ?? "").Trim();
            string[] personal = { "pdPersonalTaskDetail", "pdPersonalIssueDetail", "pdPersonalHourDetail" };
            string[] teams = { "pdTeamFullDetail", "pdTeamDetail-task", "pdTeamDetail-issue" };
            bool isTeam = teams.Contains(kind);
            bool isPersonal = personal.Contains(kind);
            if (!isTeam && !isPersonal || !CanReadProject(projectId, actor) || isTeam && !CanReadTeam(projectId))
            {
                Response.StatusCode = 403;
                litDetailHtml.Text = "<div class='pd-personal-empty'>Bạn không có quyền xem bảng công này.</div>";
                upDetail.Update();
                return;
            }
            Guid member = actor;
            if (isTeam)
            {
                if (!Guid.TryParse(hfDetailStaff.Value, out member) || !IsAuthorizedTeamMember(member, projectId))
                {
                    litDetailHtml.Text = "<div class='pd-personal-empty'>Nhân viên không thuộc dự án.</div>";
                    upDetail.Update();
                    return;
                }
            }
            try
            {
                using (var connection = new SqlConnection(PersonalConnectionString()))
                {
                    connection.Open();
                    litDetailHtml.Text = isTeam ? BuildTeamDetail(connection, projectId, member, kind)
                                               : BuildPersonalDetail(connection, projectId, actor, kind);
                }
            }
            catch (Exception exc)
            {
                SysLogger.LogError(exc, "Lỗi tải popup bảng công dự án");
                litDetailHtml.Text = "<div class='pd-personal-empty'>Không tải được dữ liệu chi tiết. Vui lòng thử lại.</div>";
            }
            upDetail.Update();
            // Callback runs after AJAX renders Literal: no hidden HTML sources on the page.
            ScriptManager.RegisterStartupScript(upDetail, upDetail.GetType(), "pdDetailLoaded",
                "if(window.QLDAProjectDetailPopup){window.QLDAProjectDetailPopup.received();}", true);
        }

        private string BuildPersonalDetail(SqlConnection conn, Guid projectId, Guid userId, string kind)
        {
            bool needTasks = kind == "pdPersonalTaskDetail" || kind == "pdPersonalHourDetail";
            bool needMeetings = kind == "pdPersonalHourDetail";
            bool needIssues = kind == "pdPersonalIssueDetail" || kind == "pdPersonalHourDetail";
            DataTable tasks = needTasks ? LoadPersonalTasks(conn, projectId, userId) : new DataTable();
            DataTable meetings = needMeetings ? PersonalQuery(conn, PersonalMeetingsSql, projectId, userId) : new DataTable();
            DataTable issues = needIssues ? PersonalQuery(conn, PersonalIssuesSql, projectId, userId) : new DataTable();
            if (needTasks || needIssues)
            {
                var snapshots = LoadDoiSoat(conn, projectId, userId);
                if (needTasks) AttachDoiSoat(tasks, snapshots, "IdCongViec");
                if (needIssues) AttachDoiSoat(issues, snapshots, "IdCongViecBiAnhHuong");
            }
            var taskRows = tasks.Rows.Cast<DataRow>()
                .OrderBy(r => { int s = PersonalInt(r, "TrangThai"); return s == 1 ? 0 : s == 0 ? 1 : s == 2 ? 3 : 2; })
                .ThenBy(r => PersonalDate(r, "NgayKetThuc") ?? DateTime.MaxValue)
                .ThenBy(r => PersonalText(r, "MaCongViec"), StringComparer.CurrentCultureIgnoreCase).ToList();
            DateTime now = DateTime.Now;
            var issueRows = issues.Rows.Cast<DataRow>()
                .OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            var sb = new StringBuilder(9000);
            if (kind == "pdPersonalTaskDetail")
            {
                if (taskRows.Count == 0) sb.Append("<div class='pd-personal-empty'>Bạn chưa được giao công việc nào trong dự án này.</div>");
                DsTableBegin(sb);
                foreach (DataRow task in taskRows) DsTableRow(sb, task, PersonalText(task, "TenCongViec"), PersonalText(task, "MaCongViec"), false);
                DsTableEnd(sb);
            }
            else if (kind == "pdPersonalIssueDetail")
            {
                sb.Append("<div class='pd-detail-list'>");
                if (issueRows.Count == 0) sb.Append("<div class='pd-personal-empty'>Chưa có vấn đề được phân công cho bạn.</div>");
                foreach (DataRow issue in issueRows) AppendPersonalIssueDetail(sb, issue);
                sb.Append("</div>");
                DsTableBegin(sb);
                foreach (DataRow issue in issueRows) DsTableRow(sb, issue, PersonalText(issue, "TenVanDe"), "", true);
                DsTableEnd(sb);
            }
            else
            {
                decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows);
                decimal meetingHours = meetings.Rows.Cast<DataRow>().Sum(PersonalMeetingHours);
                decimal totalHours = DsUniqueSum(taskRows, issueRows) + meetingHours;
                sb.Append("<div class='pd-detail-grid'>");
                AppendSummaryRow(sb, "Tổng giờ làm công việc", taskHours, false);
                AppendSummaryRow(sb, "Tổng giờ công cuộc họp", meetingHours, false);
                AppendSummaryRow(sb, "Tổng giờ công xử lý vấn đề", issueHours, false);
                AppendSummaryRow(sb, "Tổng giờ công", totalHours, true);
                sb.Append("</div><h5 class='pd-ds-subtitle'>Công việc: PM chốt / tự khai</h5>");
                DsTableBegin(sb);
                foreach (DataRow task in taskRows) DsTableRow(sb, task, PersonalText(task, "TenCongViec"), PersonalText(task, "MaCongViec"), false);
                DsTableEnd(sb);
                sb.Append("<h5 class='pd-ds-subtitle'>Vấn đề liên kết</h5>");
                DsTableBegin(sb);
                foreach (DataRow issue in issueRows) DsTableRow(sb, issue, PersonalText(issue, "TenVanDe"), "", true);
                DsTableEnd(sb);
                sb.Append("<p class='pd-ds-footnote'>Một công việc có thể đồng thời liên kết vấn đề; tổng giờ cuối không cộng trùng. Cuộc họp dùng quy tắc thời lượng hiện tại.</p>");
            }
            return sb.ToString();
        }

        private string BuildTeamDetail(SqlConnection conn, Guid projectId, Guid userId, string kind)
        {
            bool full = kind == "pdTeamFullDetail";
            bool needTasks = full || kind == "pdTeamDetail-task";
            bool needMeetings = full;
            bool needIssues = full || kind == "pdTeamDetail-issue";
            DataTable tasks = needTasks ? LoadPersonalTasks(conn, projectId, userId) : new DataTable();
            DataTable meetings = needMeetings ? PersonalQuery(conn, TeamHoursEndedMeetingsSql, projectId, userId) : new DataTable();
            DataTable issues = needIssues ? PersonalQuery(conn, PersonalIssuesSql, projectId, userId) : new DataTable();
            if (needTasks || needIssues)
            {
                var snapshots = LoadDoiSoat(conn, projectId, userId);
                if (needTasks) AttachDoiSoat(tasks, snapshots, "IdCongViec");
                if (needIssues) AttachDoiSoat(issues, snapshots, "IdCongViecBiAnhHuong");
            }
            var taskRows = tasks.Rows.Cast<DataRow>().ToList();
            var meetingRows = meetings.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MinValue).ToList();
            var issueRows = issues.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows), meetingHours = meetingRows.Sum(TeamMeetingLaborHours);
            decimal totalHours = DsUniqueSum(taskRows, issueRows) + meetingHours;
            var sb = new StringBuilder(12000);
            if (full)
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId);
                string name = user == null ? "Thành viên" : (string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName);
                sb.Append("<div class='pd-ds-overview'><strong>Tổng hợp giờ công: ").Append(EncodePersonal(name)).Append("</strong>")
                  .Append("<nav class='pd-ds-equation' aria-label='Đi đến bảng giờ công'>");
                AppendTeamJump(sb, "task", "Công việc", taskHours);
                sb.Append("<span class='pd-ds-equation-sign'>+</span>");
                AppendTeamJump(sb, "meeting", "Công họp", meetingHours);
                sb.Append("<span class='pd-ds-equation-sign'>+</span>");
                AppendTeamJump(sb, "issue", "Công vấn đề", totalHours - taskHours - meetingHours);
                sb.Append("<span class='pd-ds-equation-sign'>=</span><strong class='pd-ds-equation-total'>")
                  .Append(TeamLaborHourText(totalHours)).Append("</strong></nav></div>");
                AppendTeamSectionSource(sb, "task", taskRows, taskHours);
                AppendTeamSectionSource(sb, "meeting", meetingRows, meetingHours);
                AppendTeamSectionSource(sb, "issue", issueRows, issueHours);
                sb.Append("<p class='pd-ds-footnote'>Giờ PM chốt chỉ lấy từ bản đối soát hiện tại. Công vấn đề trong phép cộng là phần chưa trùng với Công việc; ở bảng Vấn đề vẫn hiện đủ giờ của công việc liên kết để đối chiếu. Cuộc họp giữ logic tham gia theo CtrlMeet.</p>");
            }
            else
            {
                string section = kind.Substring("pdTeamDetail-".Length);
                AppendTeamSectionSource(sb, section, section == "task" ? taskRows : section == "meeting" ? meetingRows : issueRows,
                    section == "task" ? taskHours : section == "meeting" ? meetingHours : issueHours);
            }
            return sb.ToString();
        }

        // Các hàm truy vấn / tính / render lấy nguyên logic đối soát V15.
        private sealed class PdDoiSoat
        {
            public Guid IdCongViec;
            public decimal? ThuongTuKhai, TangCaTuKhai;
            public decimal ThuongPm, TangCaPm;
            public bool DaPmChot;
            public decimal PhutPm { get { return DaPmChot ? ThuongPm + TangCaPm : 0m; } }
        }

        private sealed class PersonalTaskProgressInfo
        {
            public int Percent;
            public string Status;
            public string StatusCss;
            public string BarCss;
            public string Tooltip;
            public bool Done;
            public bool LateFinished;
        }

        private static readonly string PersonalTasksSql = @"
SELECT DISTINCT t.IdCongViec,t.MaCongViec,t.TenCongViec,t.NgayBatDau,t.NgayKetThuc,
       t.TrangThai,t.PhanTramHoanThanh, {COMPLETION_DATE} AS NgayHoanThanhThucTe
FROM dbo.TblCongViec_NhanVien assign
INNER JOIN dbo.TblCongViec t ON t.IdCongViec=assign.IdCongViec
WHERE assign.IdNhanVien=@UserId AND t.IdDuAn=@ProjectId AND ISNULL(t.DaXoa,0)=0
ORDER BY t.NgayKetThuc,t.TenCongViec;";

        private static readonly string PersonalMeetingsSql = @"
SELECT DISTINCT m.IdLichHop,m.TenCuocHop,m.ThoiGianBatDau,m.ThoiGianKetThuc
FROM dbo.TblLichHop m
WHERE m.IdDuAn=@ProjectId AND ISNULL(m.DaXoa,0)=0 AND
      (m.IdNguoiTao=@UserId OR EXISTS
        (SELECT 1 FROM dbo.TblLichHop_NhanVien attendees
         WHERE attendees.IdLichHop=m.IdLichHop AND attendees.IdNhanVien=@UserId));";

        private static readonly string PersonalIssuesSql = @"
SELECT DISTINCT v.IdVanDe,v.TenVanDe,v.TrangThai,v.MucDoAnhHuong,
       v.NgayTao,v.IdCongViecBiAnhHuong
FROM dbo.TblVanDe_NhanVien assign
INNER JOIN dbo.TblVanDe v ON v.IdVanDe=assign.IdVanDe
WHERE assign.IdNhanVien=@UserId AND v.IdDuAn=@ProjectId AND ISNULL(v.DaXoa,0)=0
ORDER BY v.NgayTao DESC,v.TenVanDe;";
        private static readonly string TeamHoursEndedMeetingsSql = @"
SELECT m.IdLichHop, m.TenCuocHop, m.ThoiGianBatDau, m.ThoiGianKetThuc,
       ISNULL(
         (SELECT MAX(CAST(g.DaThamGiaOverride AS INT))
          FROM dbo.TblLichHopGioCong g
          WHERE g.IdLichHop=m.IdLichHop AND g.IdNhanVien=@UserId),
         ISNULL(
           (SELECT MAX(CAST(ISNULL(p.XacNhanThamGia,0) AS INT))
            FROM dbo.TblLichHop_NhanVien p
            WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId),
           0)
       ) AS XacNhanThamGia
FROM dbo.TblLichHop m
WHERE m.IdDuAn=@ProjectId
  AND ISNULL(m.DaXoa,0)=0
  AND m.ThoiGianKetThuc<=GETDATE()
  AND (m.IdNguoiTao=@UserId OR EXISTS
      (SELECT 1 FROM dbo.TblLichHop_NhanVien p
       WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId))
ORDER BY m.ThoiGianBatDau DESC, m.IdLichHop DESC;";

        private static string EncodePersonal(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty);
        }

        private static string PersonalText(DataRow row, string column)
        {
            return row != null && row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToString(row[column]) : string.Empty;
        }

        private static DateTime? PersonalDate(DataRow row, string column)
        {
            DateTime result;
            return DateTime.TryParse(PersonalText(row, column), out result) ? (DateTime?)result : null;
        }

        private static int PersonalInt(DataRow row, string column)
        {
            int result;
            return int.TryParse(PersonalText(row, column), out result) ? result : 0;
        }

        private static string PersonalConnectionString()
        {
            ConnectionStringSettings named = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (named != null && !string.IsNullOrWhiteSpace(named.ConnectionString)) return named.ConnectionString;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) &&
                    item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    return item.ConnectionString;
            }
            throw new ConfigurationErrorsException("Chưa tìm thấy connection string của database SweetSoft_QLDA.");
        }

        private static DataTable PersonalQuery(SqlConnection connection, string sql, Guid projectId, Guid userId)
        {
            DataTable table = new DataTable();
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 30;
                command.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = projectId;
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                adapter.Fill(table);
            }
            return table;
        }

        private static Dictionary<Guid, PdDoiSoat> LoadDoiSoat(SqlConnection conn, Guid projectId, Guid userId)
        {
            const string sql = @"WITH HienTai AS (
    SELECT s.IdCongViec, s.SoPhutThuongTuKhai, s.SoPhutTangCaTuKhai,
        s.SoPhutThuongPmChot, s.SoPhutTangCaPmChot, s.IdPmChot,
        ROW_NUMBER() OVER (PARTITION BY s.IdCongViec
           ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS rn
    FROM dbo.TblKhaiBaoGioDoiSoat AS s
    INNER JOIN dbo.TblCongViec AS t ON t.IdCongViec=s.IdCongViec
      AND t.IdDuAn=s.IdDuAn AND ISNULL(t.DaXoa,0)=0
    WHERE s.IdDuAn=@ProjectId AND s.IdNhanVien=@UserId AND s.LaBanHienTai=1
) SELECT * FROM HienTai WHERE rn=1;";
            var result = new Dictionary<Guid, PdDoiSoat>();
            DataTable table = PersonalQuery(conn, sql, projectId, userId);
            foreach (DataRow r in table.Rows)
            {
                Guid id = (Guid)r["IdCongViec"];
                bool chot = r["IdPmChot"] != DBNull.Value &&
                            (Guid)r["IdPmChot"] != Guid.Empty;
                result[id] = new PdDoiSoat
                {
                    IdCongViec = id,
                    DaPmChot = chot,
                    ThuongTuKhai = r["SoPhutThuongTuKhai"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SoPhutThuongTuKhai"]),
                    TangCaTuKhai = r["SoPhutTangCaTuKhai"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SoPhutTangCaTuKhai"]),
                    ThuongPm = r["SoPhutThuongPmChot"] == DBNull.Value ? 0 : Convert.ToDecimal(r["SoPhutThuongPmChot"]),
                    TangCaPm = r["SoPhutTangCaPmChot"] == DBNull.Value ? 0 : Convert.ToDecimal(r["SoPhutTangCaPmChot"])
                };
            }
            return result;
        }
        private static void AttachDoiSoat(DataTable table, Dictionary<Guid, PdDoiSoat> byTask, string idColumn)
        {
            string[] names = { "DsThuongTuKhai", "DsTangCaTuKhai", "DsThuongPm", "DsTangCaPm", "DsPhutPm", "DsDaChot", "DsIdCongViec" };
            foreach (string name in names)
                if (!table.Columns.Contains(name)) table.Columns.Add(name, name == "DsDaChot" ? typeof(bool) : name == "DsIdCongViec" ? typeof(Guid) : typeof(decimal));
            foreach (DataRow row in table.Rows)
            {
                Guid id = row.Table.Columns.Contains(idColumn) && row[idColumn] != DBNull.Value ? (Guid)row[idColumn] : Guid.Empty;
                row["DsIdCongViec"] = id == Guid.Empty ? (object)DBNull.Value : id;
                PdDoiSoat item;
                if (!byTask.TryGetValue(id, out item)) { row["DsDaChot"] = false; row["DsPhutPm"] = 0m; continue; }
                row["DsDaChot"] = item.DaPmChot;
                row["DsPhutPm"] = item.PhutPm;
                row["DsThuongPm"] = item.DaPmChot ? item.ThuongPm : 0m;
                row["DsTangCaPm"] = item.DaPmChot ? item.TangCaPm : 0m;
                if (item.ThuongTuKhai.HasValue) row["DsThuongTuKhai"] = item.ThuongTuKhai.Value;
                if (item.TangCaTuKhai.HasValue) row["DsTangCaTuKhai"] = item.TangCaTuKhai.Value;
            }
        }
        private static decimal DsMinute(DataRow row, string field)
        {
            return row.Table.Columns.Contains(field) && row[field] != DBNull.Value ? Convert.ToDecimal(row[field]) : 0m;
        }
        private static decimal DsHour(DataRow row) { return DsMinute(row, "DsPhutPm") / 60m; }
        private static string DsOptionalMinute(DataRow row, string column)
        {
            return !row.Table.Columns.Contains(column) || row[column] == DBNull.Value
                ? "—" : Convert.ToDecimal(row[column]).ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " phút";
        }
        private static string DsResultNote(DataRow row)
        {
            return row.Table.Columns.Contains("DsDaChot") && row["DsDaChot"] != DBNull.Value && Convert.ToBoolean(row["DsDaChot"])
                ? "PM đã chốt" : "PM chưa chốt";
        }
        private static string DsCompact(DataRow row)
        {
            return "Giờ công: " + PersonalHourValue(DsHour(row));
        }
        private static void DsTableBegin(StringBuilder sb)
        {
            sb.Append("<div class='pd-ds-table-scroller'><table class='pd-ds-table'><thead><tr>")
                .Append("<th>Công việc / Vấn đề</th><th>Tự khai thường (phút)</th><th>Tự khai tăng ca (phút)</th>")
                .Append("<th>PM chốt thường (phút)</th><th>PM chốt tăng ca (phút)</th><th>Kết quả cuối cùng</th><th>Đối soát</th>")
                .Append("</tr></thead><tbody>");
        }
        private static void DsTableRow(StringBuilder sb, DataRow row, string name, string code, bool isIssue)
        {
            sb.Append("<tr><td class='pd-ds-name'>");
            if (!isIssue) sb.Append("<span class='pd-ds-hover-trigger' tabindex='0' aria-label='Xem nhanh công việc: ")
                .Append(HttpUtility.HtmlAttributeEncode(name).Replace("'", "&#39;"))
                .Append("'>");
            if (!string.IsNullOrWhiteSpace(code)) sb.Append("<span class='pd-team-record-code'>").Append(EncodePersonal(code)).Append("</span> ");
            sb.Append(EncodePersonal(name));
            if (!isIssue)
            {
                sb.Append("</span><div class='pd-ds-hover-template' hidden>");
                AppendPersonalTaskDetail(sb, row, DateTime.Now);
                sb.Append("</div>");
            }
            if (isIssue) sb.Append("<small class='pd-ds-link-note'>Liên kết công việc bị ảnh hưởng; không phải phiên giờ vấn đề độc lập</small>");
            sb.Append("</td><td>").Append(EncodePersonal(DsOptionalMinute(row, "DsThuongTuKhai")))
                .Append("</td><td>").Append(EncodePersonal(DsOptionalMinute(row, "DsTangCaTuKhai")))
                .Append("</td><td>").Append(EncodePersonal(DsResultNote(row) == "PM đã chốt" ? DsOptionalMinute(row, "DsThuongPm") : "—"))
                .Append("</td><td>").Append(EncodePersonal(DsResultNote(row) == "PM đã chốt" ? DsOptionalMinute(row, "DsTangCaPm") : "—"))
                .Append("</td><td><strong>").Append(EncodePersonal(PersonalHourValue(DsHour(row)) + " giờ công"))
                .Append("</strong></td><td><span class='pd-ds-status'>").Append(EncodePersonal(DsResultNote(row)))
                .Append("</span></td></tr>");
        }
        private static void DsTableEnd(StringBuilder sb) { sb.Append("</tbody></table></div>"); }
        private static decimal DsSum(List<DataRow> rows)
        {
            // Nhieu van de co the cung tham chieu 1 cong viec: chi tinh 1 lan trong nhom.
            return DsUniqueSum(rows, new List<DataRow>());
        }
        private static decimal DsUniqueSum(List<DataRow> tasks, List<DataRow> issues)
        {
            var minutes = new Dictionary<Guid, decimal>();
            foreach (DataRow row in tasks.Concat(issues))
            {
                if (!row.Table.Columns.Contains("DsIdCongViec") || row["DsIdCongViec"] == DBNull.Value) continue;
                Guid id = (Guid)row["DsIdCongViec"];
                if (!minutes.ContainsKey(id)) minutes[id] = DsMinute(row, "DsPhutPm");
            }
            return minutes.Values.Sum() / 60m;
        }
        private static DataTable LoadPersonalTasks(SqlConnection connection, Guid projectId, Guid userId)
        {
            string actualDateExpr = "CAST(NULL AS DATETIME)";
            const string columnSql = @"SELECT TOP (1) COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='TblCongViec'
  AND COLUMN_NAME IN ('NgayHoanThanhThucTe', 'NgayKetThucThucTe', 'NgayHoanThanh', 'ThoiGianHoanThanh')
ORDER BY CASE COLUMN_NAME
    WHEN 'NgayHoanThanhThucTe' THEN 0
    WHEN 'NgayKetThucThucTe' THEN 1
    WHEN 'NgayHoanThanh' THEN 2
    ELSE 3 END";
            using (SqlCommand command = new SqlCommand(columnSql, connection))
            {
                object column = command.ExecuteScalar();
                if (column != null && column != DBNull.Value)
                {
                    string selected = Convert.ToString(column);
                    string[] allowed = { "NgayHoanThanhThucTe", "NgayKetThucThucTe", "NgayHoanThanh", "ThoiGianHoanThanh" };
                    if (allowed.Contains(selected)) actualDateExpr = "t.[" + selected + "]";
                }
            }
            return PersonalQuery(connection, PersonalTasksSql.Replace("{COMPLETION_DATE}", actualDateExpr), projectId, userId);
        }

        private static decimal PersonalMeetingHours(DataRow row)
        {
            DateTime? start = PersonalDate(row, "ThoiGianBatDau");
            DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
            if (!start.HasValue || !end.HasValue || end.Value <= start.Value) return 0m;
            return (decimal)(end.Value - start.Value).TotalMinutes / 60m;
        }
        private static string PersonalHourValue(decimal hours)
        {
            return hours.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        private static string PersonalHourText(decimal hours)
        {
            return PersonalHourValue(hours) + " giờ";
        }
        private static int PersonalWeekdaysBetween(DateTime from, DateTime to)
        {
            if (to.Date <= from.Date) return 0;
            DateTime first = from.Date;
            DateTime last = to.Date;
            int days = (int)Math.Min((last - first).TotalDays, 36525d);
            int fullWeeks = days / 7;
            int result = fullWeeks * 5;
            DateTime remainStart = first.AddDays(fullWeeks * 7);
            for (int i = 0; i < days % 7; i++)
            {
                DayOfWeek d = remainStart.AddDays(i).DayOfWeek;
                if (d != DayOfWeek.Saturday && d != DayOfWeek.Sunday) result++;
            }
            return result;
        }

        private static PersonalTaskProgressInfo GetPersonalTaskProgress(DataRow task, DateTime now)
        {
            int status = PersonalInt(task, "TrangThai");
            DateTime? begin = PersonalDate(task, "NgayBatDau");
            DateTime? due = PersonalDate(task, "NgayKetThuc");
            DateTime? completed = PersonalDate(task, "NgayHoanThanhThucTe");
            var info = new PersonalTaskProgressInfo();
            info.Done = status == 2 || status == 3;
            info.LateFinished = status == 3;
            info.Status = status == 0 ? "Chưa bắt đầu" : status == 1 ? "Đang thực hiện"
                : status == 2 ? "Hoàn thành" : status == 3 ? "Hoàn thành" : status == 4 ? "Chờ duyệt" : "Chưa xác định";
            info.StatusCss = status == 0 ? "pd-task-state-0" : status == 1 ? "pd-task-state-1"
                : status == 2 ? "pd-task-state-2" : status == 3 ? "pd-task-state-3" : "pd-task-state-4";
            info.BarCss = status == 0 ? "pd-task-bar-0" : status == 1 ? "pd-task-bar-1"
                : status == 2 ? "pd-task-bar-2" : status == 3 ? "pd-task-bar-3" : "pd-task-bar-4";

            if (info.Done || status == 4) info.Percent = 100;
            else if (status == 0) info.Percent = 0;
            else if (status == 1 && begin.HasValue && due.HasValue)
            {
                int total = PersonalWeekdaysBetween(begin.Value, due.Value);
                int elapsed = PersonalWeekdaysBetween(begin.Value, now.Date < due.Value.Date ? now.Date : due.Value.Date);
                info.Percent = total > 0 ? Math.Max(0, Math.Min(100, (int)Math.Round(100d * elapsed / total, MidpointRounding.AwayFromZero)))
                    : (now.Date >= due.Value.Date ? 100 : 0);
            }
            else info.Percent = Math.Max(0, Math.Min(100, PersonalInt(task, "PhanTramHoanThanh")));

            // Tooltips show the task state only; deadline information is for running tasks.
            if (info.Done) info.Tooltip = "Hoàn thành";
            else if (status == 4) info.Tooltip = "Chờ duyệt";
            else if (status == 0) info.Tooltip = "Chưa bắt đầu";
            else if (status != 1) info.Tooltip = "Chưa xác định";
            else if (!due.HasValue) info.Tooltip = "Đang thực hiện. Chưa có hạn dự kiến";
            else
            {
                int daysLeft = (due.Value.Date - now.Date).Days;
                if (daysLeft < 0)
                {
                    info.Tooltip = "Đang thực hiện. Trễ hạn " + (-daysLeft) + " ngày";
                    info.StatusCss += " pd-task-severity-late";
                    info.BarCss = "pd-task-bar-late";
                }
                else if (daysLeft == 0)
                {
                    info.Tooltip = "Đang thực hiện. Đến hạn hôm nay";
                    info.StatusCss += " pd-task-severity-soon";
                    info.BarCss = "pd-task-bar-soon";
                }
                else if (daysLeft <= 2)
                {
                    info.Tooltip = "Đang thực hiện. Sắp hạn, còn " + daysLeft + " ngày";
                    info.StatusCss += " pd-task-severity-soon";
                    info.BarCss = "pd-task-bar-soon";
                }
                else info.Tooltip = "Đang thực hiện. Còn " + daysLeft + " ngày đến hạn";
            }
            return info;
        }

        private static string PersonalTaskProgressBar(PersonalTaskProgressInfo info)
        {
            string tooltip = HttpUtility.HtmlAttributeEncode(info.Tooltip ?? "").Replace("'", "&#39;");
            return "<div class='pd-personal-progress " + info.BarCss + "' data-pd-progress-tooltip='" + tooltip
                + "' data-pd-progress-tone='" + info.BarCss + "' tabindex='0' aria-label='" + tooltip
                + "' role='progressbar' aria-valuemin='0' aria-valuemax='100' aria-valuenow='" + info.Percent
                + "'><span style='width:" + info.Percent + "%'></span></div>";
        }

        private static decimal TeamMeetingLaborHours(DataRow meeting)
        {
            return PersonalInt(meeting, "XacNhanThamGia") == 1
                ? Math.Round(PersonalMeetingHours(meeting), 2, MidpointRounding.AwayFromZero)
                : 0m;
        }

        private static string TeamLaborHourText(decimal hours)
        {
            return PersonalHourValue(hours) + " giờ công";
        }

        private static string TeamMeetingDurationText(DataRow meeting)
        {
            decimal duration = Math.Round(PersonalMeetingHours(meeting), 2, MidpointRounding.AwayFromZero);
            return " (" + PersonalHourValue(duration) + " giờ)";
        }
        private static void AppendTeamJump(StringBuilder sb, string kind, string name, decimal hours)
        {
            sb.Append("<button type='button' class='pd-ds-jump' data-pd-ds-jump='").Append(kind)
                .Append("' title='Đi đến bảng ").Append(EncodePersonal(name)).Append("'>")
                .Append(EncodePersonal(name)).Append(" (")
                .Append(EncodePersonal(PersonalHourText(hours))).Append(")</button>");
        }
        private static void AppendTeamSectionSource(StringBuilder sb, string kind, List<DataRow> rows, decimal hours)
        {
            string noun = kind == "task" ? "công việc" : kind == "meeting" ? "cuộc họp" : "vấn đề";
            string label = kind == "task" ? "Công việc" : kind == "meeting" ? "Cuộc họp" : "Vấn đề";
            sb.Append("<h5 class='pd-ds-subtitle pd-ds-section-title' data-pd-detail-section='")
                .Append(kind).Append("'>").Append(label)
                .Append(" (").Append(rows.Count).Append(" ").Append(noun)
                .Append(" – Tổng giờ công: ").Append(EncodePersonal(PersonalHourText(hours)))
                .Append(")</h5>");
            if (kind == "meeting")
            {
                sb.Append("<div class='pd-ds-table-scroller'><table class='pd-ds-table'><thead><tr><th>Cuộc họp</th><th>Thời gian</th><th>Thời lượng</th><th>Xác nhận</th><th>Kết quả</th></tr></thead><tbody>");
                foreach (DataRow row in rows)
                {
                    bool joined = PersonalInt(row, "XacNhanThamGia") == 1;
                    DateTime? start = PersonalDate(row, "ThoiGianBatDau");
                    DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
                    sb.Append("<tr><td>").Append(EncodePersonal(PersonalText(row, "TenCuocHop")))
                        .Append("</td><td>").Append(EncodePersonal(start.HasValue ? start.Value.ToString("dd/MM/yyyy HH:mm") : "—"))
                        .Append("</td><td>").Append(EncodePersonal(PersonalHourText(PersonalMeetingHours(row))))
                        .Append("</td><td>").Append(joined ? "Có tham gia" : "Không tham gia")
                        .Append("</td><td><strong>").Append(TeamLaborHourText(TeamMeetingLaborHours(row)))
                        .Append("</strong></td></tr>");
                }
                sb.Append("</tbody></table></div>");
            }
            else
            {
                DsTableBegin(sb);
                foreach (DataRow row in rows)
                    DsTableRow(sb, row, PersonalText(row, kind == "task" ? "TenCongViec" : "TenVanDe"), kind == "task" ? PersonalText(row, "MaCongViec") : "", kind == "issue");
                DsTableEnd(sb);
            }
        }

        private string PersonalIssueStatus(int value)
        {
            try
            {
                TrangThaiVanDeEnum status = (TrangThaiVanDeEnum)value;
                string key = IssueManager.Instance.GetValueForTrangThaiVanDe(status);
                string name = GetResourceText(key);
                return string.IsNullOrWhiteSpace(name) ? "Chưa xác định" : name;
            }
            catch { return "Chưa xác định"; }
        }

        private static void AppendSummaryRow(StringBuilder builder, string title, decimal hours, bool isTotal)
        {
            builder.Append("<div class='pd-summary-row")
                   .Append(isTotal ? " is-total" : string.Empty)
                   .Append("'><strong>")
                   .Append(HttpUtility.HtmlEncode(title))
                   .Append("</strong><span>")
                   .Append(PersonalHourText(hours))
                   .Append("</span></div>");
        }

        private static void AppendPersonalTaskDetail(StringBuilder builder, DataRow task, DateTime now)
        {
            PersonalTaskProgressInfo visual = GetPersonalTaskProgress(task, now);
            DateTime? dueDate = PersonalDate(task, "NgayKetThuc");
            string tooltip = HttpUtility.HtmlAttributeEncode(visual.Tooltip).Replace("'", "&#39;");
            builder.Append("<div class='pd-detail-item'><div class='pd-detail-row'><div class='pd-personal-item-title'><span class='pd-personal-task-code'>")
                   .Append(EncodePersonal(task["MaCongViec"]))
                   .Append("</span>").Append(EncodePersonal(task["TenCongViec"]))
                   .Append("</div><div class='pd-personal-meeting-flags'><span class='pd-personal-tag ")
                   .Append(visual.StatusCss).Append("'>").Append(EncodePersonal(visual.Status)).Append("</span>");
            if (visual.LateFinished) builder.Append("<span class='pd-task-late-note'>(trễ hạn)</span>");
            builder.Append("<span class='pd-personal-hours'><i class='far fa-clock'></i>").Append(EncodePersonal(DsCompact(task))).Append("</span></div></div><div class='pd-detail-subrow'><span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(dueDate.HasValue ? "Hạn " + dueDate.Value.ToString("dd/MM/yyyy") : "Chưa có hạn")
                   .Append("</span><span class='pd-task-percent' data-pd-progress-tooltip='").Append(tooltip).Append("' data-pd-progress-tone='").Append(visual.BarCss).Append("' tabindex='0' aria-label='").Append(tooltip).Append("'>Tiến độ ")
                   .Append(visual.Percent).Append("%</span></div>")
                   .Append(PersonalTaskProgressBar(visual)).Append("</div>");
        }

        private void AppendPersonalIssueDetail(StringBuilder builder, DataRow issue)
        {
            int status = PersonalInt(issue, "TrangThai");
            bool processed = status == (int)TrangThaiVanDeEnum.Processed;
            bool processing = status == (int)TrangThaiVanDeEnum.Processing;
            DateTime? created = PersonalDate(issue, "NgayTao");
            builder.Append("<div class='pd-detail-item'><div class='pd-detail-row'><div class='pd-personal-item-title'>")
                   .Append(EncodePersonal(issue["TenVanDe"]))
                   .Append("</div><div class='pd-personal-meeting-flags'><span class='pd-personal-tag ")
                   .Append(processed ? "is-done" : processing ? "is-progress" : string.Empty)
                   .Append("'>")
                   .Append(EncodePersonal(PersonalIssueStatus(status)))
                   .Append("</span><span class='pd-personal-hours'><i class='far fa-clock'></i>").Append(EncodePersonal(DsCompact(issue))).Append("</span></div></div>")
                   .Append("<div class='pd-detail-subrow'><span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(created.HasValue ? "Ngày tạo " + created.Value.ToString("dd/MM/yyyy") : "Chưa có ngày tạo")
                   .Append("</span></div></div>");
        }
    }
}
