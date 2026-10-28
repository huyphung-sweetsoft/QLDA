using SweetCMS.Controls.Helpers;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.BackOffice.fTasks.Controls;
using SweetSoft.QLDA.BackOffice.fMeets.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Controls;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class DuAnDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Project;
            }
        }

        protected bool IsContractView
        {
            get
            {
                if (this.IsUserRight(ActionKeys.View, ModuleKeys.Contract))
                    return true;
                return false;
            }
        }

        private Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] != null)
                    return (Guid)ViewState["IdHopDongThucHien"];

                return Guid.Empty;
            }
            set
            {
                ViewState["IdHopDongThucHien"] = value;
            }
        }

        private Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("ProjectId");
                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        private bool CanModifyProject
        {
            get
            {
                return this.IsEdit &&
                       CurrentStatusValue != (byte)DuAnStatus.HoanThanh &&
                       CurrentStatusValue != (byte)DuAnStatus.KetThuc;
            }
        }

        private AuditManager _auditManager;
        protected byte CurrentStatusValue { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Popup chi tiet tu tai data khi AJAX request, theo dung du an dang mo.
            var chiTiet = FindProjectDetailControl(this, "CtrlChiTietCong1") as CtrlChiTietCong;
            if (chiTiet != null) chiTiet.ProjectId = QueryId;
            var chiTietHop = FindProjectDetailControl(this, "CtrlChiTietCongCuocHop1") as CtrlChiTietCongCuocHop;
            if (chiTietHop != null) chiTietHop.ProjectId = QueryId;
            // Đăng ký trigger async tương tự CtrlTodaySchedule, ngoài UpdatePanel bốn bảng.
            ScriptManager managerPd = ScriptManager.GetCurrent(Page);
            Control openView = FindProjectDetailControl(this, "btnPdViewObject");
            if (managerPd != null && openView != null) managerPd.RegisterAsyncPostBackControl(openView);
            CtrlGiaiDoanDuAn1.IdDuAn = QueryId;
            CtrlLichSuDuAn1.IdDuAn = QueryId;
            pnlContract.Visible = this.IsContractView;
            CtrlDuAnForm1.SaveCompleted += CtrlDuAnForm1_SaveCompleted;
            CtrlChonNhanVien1.OnConfirmSelection += CtrlChonNhanVien1_OnConfirmSelection;
            if (_auditManager == null)
                _auditManager = new AuditManager(new Core.SysManager.Models.ClientInfo()
                {
                    UserId = SweetContext.Current.UserId,
                    IpAddress = SweetContext.Current.CurrentUserIp,
                    UserAgent = SweetContext.Current.CurrentUserAgent
                });
            CtrlProjectTabs1.ProjectId = QueryId;
            // Permission checked on EVERY postback: the tab must never be returned to regular members.
            bool canViewTeamHours = CanViewProjectTeamHours();
            phTeamHoursTab.Visible = canViewTeamHours;
            phTeamHoursPanel.Visible = canViewTeamHours;
            if (canViewTeamHours) BindProjectTeamHours();
            if (!IsPostBack)
            {
                if (ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(SweetContext.Current.UserId, QueryId) == null && !this.IsAdministrator)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Projects, GetResourceText(BackEndResourceKeys.PROJECT_LIST) }, {"javascript:", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                if (this.QueryId != Guid.Empty)
                {
                    BindData();
                    CtrlGiaiDoanDuAn1.InitControls();
                }
            }
        }

        // V15: Literal cua tab ca nhan co EnableViewState=false.
        // Moi postback deu tao lai cay control; neu event khong goi BindData
        // thi literal se rong va giao dien tab "Phan viec cua toi" bien mat.
        // Bind muon o PreRender de tranh double bind neu da co noi dung.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (QueryId != Guid.Empty && litProjectPersonal != null &&
                string.IsNullOrEmpty(litProjectPersonal.Text))
            {
                BindPersonalProjectView();
            }
        }

        protected override void BindData()
        {
            try
            {
                DataTable dt = DuAnManager.Instance.GetDetailDuAnById(QueryId);
                if (dt == null || dt.Rows.Count == 0)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                    return;
                }
                DataRow row = dt.Rows[0];
                BindProjectInformation(row);
                BindProjectTeam();
                BindRecentProjectHistory();
                BindProjectProgress();
                BindPersonalProjectView();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }


        // Từ tên công việc/cuộc họp sang popup ViewDetail: chỉ nhận loại và GUID, kiểm tra quyền tại server.
        // PM/Admin có thể xem tất cả đối tượng trong dự án; thành viên chỉ xem task được giao / họp có tham gia hoặc chủ trì.
        private bool CanOpenProjectViewObject(string kind, Guid id, Guid projectId, Guid actor)
        {
            if (id == Guid.Empty || projectId == Guid.Empty || actor == Guid.Empty) return false;
            bool isManager = CanViewProjectTeamHours();
            string sql;
            if (kind == "task")
                sql = @"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TblCongViec t
                  WHERE t.IdCongViec=@Id AND t.IdDuAn=@ProjectId AND ISNULL(t.DaXoa,0)=0
                  AND (@IsManager=1 OR EXISTS(SELECT 1 FROM dbo.TblCongViec_NhanVien n
                       WHERE n.IdCongViec=t.IdCongViec AND n.IdNhanVien=@UserId))) THEN 1 ELSE 0 END";
            else if (kind == "meet")
                sql = @"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TblLichHop m
                  WHERE m.IdLichHop=@Id AND m.IdDuAn=@ProjectId AND ISNULL(m.DaXoa,0)=0
                  AND (@IsManager=1 OR m.IdNguoiTao=@UserId OR EXISTS(SELECT 1 FROM dbo.TblLichHop_NhanVien n
                       WHERE n.IdLichHop=m.IdLichHop AND n.IdNhanVien=@UserId))) THEN 1 ELSE 0 END";
            else return false;
            using (var conn = new SqlConnection(PersonalConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 20;
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                cmd.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = projectId;
                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = actor;
                cmd.Parameters.Add("@IsManager", SqlDbType.Bit).Value = isManager;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        protected void btnPdViewObject_Click(object sender, EventArgs e)
        {
            var kindField = FindProjectDetailControl(this, "hfPdViewObjectKind") as HiddenField;
            var idField = FindProjectDetailControl(this, "hfPdViewObjectId") as HiddenField;
            string kind = kindField == null ? "" : (kindField.Value ?? "").Trim().ToLowerInvariant();
            Guid objectId;
            if ((kind != "task" && kind != "meet") || idField == null ||
                !Guid.TryParse(idField.Value, out objectId) || objectId == Guid.Empty) return;
            try
            {
                Guid actor = SweetContext.Current.UserId;
                if (!CanOpenProjectViewObject(kind, objectId, QueryId, actor))
                {
                    ShowNotify("Bạn không có quyền xem chi tiết này trong dự án.", MSGType.Error);
                    return;
                }
                if (kind == "task")
                {
                    CtrlViewTaskDetail detail = FindProjectDetailControl(Page, "CtrlTodayTaskDetail") as CtrlViewTaskDetail;
                    if (detail != null) detail.OpenModal(objectId);
                    else ShowNotify("Không tìm thấy cửa sổ chi tiết công việc trên trang.", MSGType.Error);
                }
                else
                {
                    CtrlViewMeetDetail detail = FindProjectDetailControl(Page, "CtrlTodayMeetDetail") as CtrlViewMeetDetail;
                    if (detail != null) detail.OpenModal(objectId);
                    else ShowNotify("Không tìm thấy cửa sổ chi tiết cuộc họp trên trang.", MSGType.Error);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "DuAnDetail: lỗi mở popup ViewDetail từ tên công việc/cuộc họp");
                ShowNotify("Không mở được chi tiết. Vui lòng thử lại.", MSGType.Error);
            }
        }

        // =========================================================================================
        // PHẦN VIỆC CỦA TÔI - chỉ đọc thông tin, giới hạn cả @ProjectId và @UserId.
        // Không phụ thuộc vào các bảng KhaiBaoGio: giờ công tạm hiển thị 0 theo yêu cầu.
        // =========================================================================================
        protected static Control FindProjectDetailControl(Control root, string id)
        {
            if (root == null) return null;
            if (root.ID == id) return root;
            foreach (Control child in root.Controls)
            {
                Control found = FindProjectDetailControl(child, id);
                if (found != null) return found;
            }
            return null;
        }

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

        // KBG 2.0: CHI doc ban hien tai moi nhat cua tung cong viec/nhan vien/du an.
        // So phut PM la ket qua DUY NHAT; khong tu dong su dung so giay he thong hay so phut tu khai.
        private sealed class PdDoiSoat
        {
            public Guid IdCongViec;
            public decimal? ThuongTuKhai, TangCaTuKhai;
            public decimal ThuongPm, TangCaPm;
            public bool DaPmChot;
            public decimal PhutPm { get { return DaPmChot ? ThuongPm + TangCaPm : 0m; } }
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
        private static string DsHourText(decimal minutes)
        {
            return PersonalHourValue(minutes / 60m) + " giờ";
        }


        private static string DsCompact(DataRow row)
        {
            return "Giờ công: " + PersonalHourValue(DsHour(row));
        }
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

        private static readonly string PersonalTasksSql = @"
SELECT DISTINCT t.IdCongViec,t.MaCongViec,t.TenCongViec,t.NgayBatDau,t.NgayKetThuc,
       t.ThoiHanNgay,t.TrangThai,t.PhanTramHoanThanh, {COMPLETION_DATE} AS NgayHoanThanhThucTe
FROM dbo.TblCongViec_NhanVien assign
INNER JOIN dbo.TblCongViec t ON t.IdCongViec=assign.IdCongViec
WHERE assign.IdNhanVien=@UserId AND t.IdDuAn=@ProjectId AND ISNULL(t.DaXoa,0)=0
ORDER BY t.NgayKetThuc,t.TenCongViec;";

        // Chỉ truy cập cột ngày hoàn thành nếu có THẬT trong TblCongViec.
        // Nếu DB chưa có cột phù hợp, không tự lấy ngày hiện tại để suy đoán số ngày trễ.
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

        private static DateTime PersonalMeetingEnd(DataRow row)
        {
            return PersonalDate(row, "ThoiGianKetThuc") ?? PersonalDate(row, "ThoiGianBatDau") ?? DateTime.MinValue;
        }

        // Giờ công họp tạm tính theo thời lượng lịch họp (phút / 60), chưa phải giờ tham gia thực tế.
        private static decimal PersonalMeetingHours(DataRow row)
        {
            DateTime? start = PersonalDate(row, "ThoiGianBatDau");
            DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
            if (!start.HasValue || !end.HasValue || end.Value <= start.Value) return 0m;
            return (decimal)(end.Value - start.Value).TotalMinutes / 60m;
        }

        // Số giờ ở badge: "Giờ công: 1". Thời lượng cuộc họp: "(1 giờ)".
        private static string PersonalHourValue(decimal hours)
        {
            return hours.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        private static string PersonalHourText(decimal hours)
        {
            return PersonalHourValue(hours) + " giờ";
        }

        // Đồng bộ CtrlMeet.CalendarStatusAt: 0 lịch, 1 sắp diễn ra (15 phút), 2 đang diễn ra, 3 kết thúc.
        private static int PersonalMeetingStatus(DateTime? start, DateTime? end, DateTime now)
        {
            if (end.HasValue && now >= end.Value) return 3;
            if (!start.HasValue) return 0;
            if (now >= start.Value) return 2;
            return start.Value <= now.AddMinutes(15) ? 1 : 0;
        }

        private static string PersonalMeetingBadge(DateTime? start, DateTime? end, DateTime now)
        {
            int code = PersonalMeetingStatus(start, end, now);
            string caption = code == 0 ? "Đã lên lịch" : code == 1 ? "Sắp diễn ra" : code == 2 ? "Đang diễn ra" : "Đã kết thúc";
            string beginAttr = start.HasValue ? start.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "";
            string endAttr = end.HasValue ? end.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "";
            return "<span class='pd-personal-tag pd-meet-state pd-meet-state-" + code
                + "' data-pd-meeting-status='1' data-pd-meet-start='" + beginAttr
                + "' data-pd-meet-end='" + endAttr + "'>" + caption + "</span>";
        }

        // Lịch dùng cho % tiến độ theo thời gian của từng công việc.
        // NgayTrongTuan = (int)System.DayOfWeek: Chủ nhật=0, Thứ hai=1, ..., Thứ bảy=6.
        // Cùng quy ước với lịch CtrlMeet. TblLichNgoaiLe chỉ dùng để loại ngày nghỉ,
        // KHÔNG tự bổ sung logic ngày làm bù.
        private sealed class PersonalWorkCalendar
        {
            public readonly bool[] WorkingWeekdays = new bool[7];
            public readonly HashSet<DateTime> DaysOff = new HashSet<DateTime>();

            public PersonalWorkCalendar()
            {
                for (int day = 0; day < 7; day++)
                    WorkingWeekdays[day] = day >= 1 && day <= 5;
            }

            public int CountWorkingDays(DateTime from, DateTime to)
            {
                DateTime first = from.Date;
                DateTime last = to.Date;
                if (last < first) return 0;

                // Chặn khoảng ngày bất thường để không lặp vô hạn khi dữ liệu sai.
                int total = (int)Math.Min((last - first).TotalDays, 36525d);
                int count = 0;
                for (int i = 0; i <= total; i++)
                {
                    DateTime day = first.AddDays(i);
                    if (WorkingWeekdays[(int)day.DayOfWeek] && !DaysOff.Contains(day)) count++;
                }
                return count;
            }
        }

        private static PersonalWorkCalendar LoadPersonalWorkCalendar(SqlConnection connection, DataTable tasks, DateTime today)
        {
            var calendar = new PersonalWorkCalendar();
            // Truy vấn MỘT LẦN mỗi lần bind, không query lại trên từng task.
            using (var cmd = new SqlCommand(@"SELECT NgayTrongTuan, LaNgayLamViec
FROM dbo.TblCauHinhTuanLamViec;", connection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1)) continue;
                    int day = Convert.ToInt32(reader.GetValue(0));
                    if (day >= 0 && day <= 6)
                        calendar.WorkingWeekdays[day] = Convert.ToBoolean(reader.GetValue(1));
                }
            }

            DateTime from = tasks.Rows.Cast<DataRow>()
                .Where(row => PersonalInt(row, "TrangThai") == 1)
                .Select(row => PersonalDate(row, "NgayBatDau"))
                .Where(date => date.HasValue && date.Value.Date <= today.Date)
                .Select(date => date.Value.Date)
                .DefaultIfEmpty(today.Date)
                .Min();

            // Chỉ tải ngày nghỉ đến HÔM NAY, bởi % đã qua không cần tương lai.
            using (var cmd = new SqlCommand(@"SELECT NgayBatDau, NgayKetThuc
FROM dbo.TblLichNgoaiLe
WHERE ISNULL(DaXoa, 0) = 0 AND LaNgayLamViec = 0
  AND NgayBatDau < @DenNgay AND ISNULL(NgayKetThuc, NgayBatDau) >= @TuNgay;", connection))
            {
                cmd.Parameters.Add("@TuNgay", SqlDbType.DateTime).Value = from;
                cmd.Parameters.Add("@DenNgay", SqlDbType.DateTime).Value = today.Date.AddDays(1);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0)) continue;
                        DateTime fromHoliday = Convert.ToDateTime(reader.GetValue(0)).Date;
                        DateTime toHoliday = reader.IsDBNull(1) ? fromHoliday : Convert.ToDateTime(reader.GetValue(1)).Date;
                        if (fromHoliday < from) fromHoliday = from;
                        if (toHoliday > today.Date) toHoliday = today.Date;
                        if (toHoliday < fromHoliday) continue;
                        int days = (int)Math.Min((toHoliday - fromHoliday).TotalDays, 36525d);
                        for (int i = 0; i <= days; i++) calendar.DaysOff.Add(fromHoliday.AddDays(i));
                    }
                }
            }
            return calendar;
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

        private static PersonalTaskProgressInfo GetPersonalTaskProgress(DataRow task, DateTime now, PersonalWorkCalendar calendar)
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
                // MẪU SỐ = đúng thời hạn được khai báo trên task (TblCongViec.ThoiHanNgay).
                // TỬ SỐ = số ngày làm việc [ngày bắt đầu .. hôm nay] theo cấu hình tuần
                //            trừ ngày nghỉ lễ/ngoại lệ; không cộng thêm ngày làm bù.
                int duration = PersonalInt(task, "ThoiHanNgay");
                if (duration <= 0) duration = Math.Max(1, (due.Value.Date - begin.Value.Date).Days + 1);
                DateTime through = now.Date;
                int elapsed = calendar.CountWorkingDays(begin.Value, through);
                int percent = Math.Max(0, Math.Min(100,
                    (int)Math.Round(100d * elapsed / duration, MidpointRounding.AwayFromZero)));
                // Không hiển thị 100% khi vẫn CHƯA đến hạn (dù dữ liệu thời hạn/ngày kết thúc
                // lệch nhau), tránh tình huống "còn 2 ngày" mà thanh đã đầy.
                if (now.Date < due.Value.Date && percent >= 100) percent = 99;
                info.Percent = percent;
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

        private static string PersonalMeetingDayText(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) return "Chưa xác định ngày";
            if (end.HasValue && start.Value.Date != end.Value.Date)
                return "Ngày " + start.Value.ToString("dd/MM/yyyy") + " – " + end.Value.ToString("dd/MM/yyyy");
            return "Ngày " + start.Value.ToString("dd/MM/yyyy");
        }

        private static string PersonalMeetingTimeText(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) return "Chưa xác định giờ";
            string timeRange = start.Value.ToString("HH:mm") + " – "
                + (end.HasValue ? end.Value.ToString("HH:mm") : "Chưa xác định");
            if (end.HasValue && end.Value > start.Value)
            {
                decimal durationHours = (decimal)(end.Value - start.Value).TotalMinutes / 60m;
                timeRange += " (" + PersonalHourText(durationHours) + ")";
            }
            return timeRange;
        }

        // Hover giờ công: chỉ dữ liệu của chính nhân viên đang đăng nhập, bản đối soát hiện tại.
        private sealed class PersonalMeetingLabor
        {
            public bool ThamGia;
            public decimal? GioThucTe;
            // Co loi truy van la mot trang thai rieng; khong duoc ghi nhan nham thanh 0 gio.
            public bool LoiTaiDuLieu;
        }

        private static Dictionary<Guid, PersonalMeetingLabor> LoadPersonalMeetingLabor(SqlConnection connection, Guid projectId, Guid userId)
        {
            const string sql = @"SELECT m.IdLichHop,
ISNULL((SELECT MAX(CAST(g.DaThamGia AS INT)) FROM dbo.TblLichHopGioCong g
        WHERE g.IdLichHop=m.IdLichHop AND g.IdNhanVien=@UserId),
       ISNULL((SELECT MAX(CAST(ISNULL(nv.XacNhanThamGia,0) AS INT)) FROM dbo.TblLichHop_NhanVien nv
        WHERE nv.IdLichHop=m.IdLichHop AND nv.IdNhanVien=@UserId),0)) AS ThamGia,
(SELECT MAX(g.SoGioThucTe) FROM dbo.TblLichHopGioCong g
 WHERE g.IdLichHop=m.IdLichHop AND g.IdNhanVien=@UserId) AS GioThucTe
FROM dbo.TblLichHop m WHERE m.IdDuAn=@ProjectId AND ISNULL(m.DaXoa,0)=0
AND (m.IdNguoiTao=@UserId OR EXISTS
 (SELECT 1 FROM dbo.TblLichHop_NhanVien nv WHERE nv.IdLichHop=m.IdLichHop AND nv.IdNhanVien=@UserId));";
            // Truy van phu cho tooltip: gioi han thoi gian, khong chan ca 4 bang khi SQL cham.
            DataTable data = new DataTable();
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 5;
                command.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = projectId;
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                adapter.Fill(data);
            }
            var map = new Dictionary<Guid, PersonalMeetingLabor>();
            foreach (DataRow row in data.Rows)
                map[(Guid)row["IdLichHop"]] = new PersonalMeetingLabor
                {
                    ThamGia = row["ThamGia"] != DBNull.Value && Convert.ToInt32(row["ThamGia"]) == 1,
                    GioThucTe = row["GioThucTe"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["GioThucTe"])
                };
            return map;
        }
        private static Guid PersonalGuid(DataRow row, string column)
        {
            Guid id;
            return Guid.TryParse(PersonalText(row, column), out id) ? id : Guid.Empty;
        }
        private static string PersonalLaborMinute(decimal? minutes)
        {
            return minutes.HasValue && minutes.Value > 0m
                ? minutes.Value.ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " phút"
                : "Chưa ghi nhận công";
        }
        private static string PersonalLaborHour(decimal hours)
        {
            return hours > 0m ? PersonalHourValue(hours) + " giờ công" : "Chưa ghi nhận công";
        }
        private static string PersonalLaborTooltip(string title, string[] columns, string[] values)
        {
            var b = new StringBuilder(700);
            b.Append("<div class='pd-hour-hover-title'>").Append(EncodePersonal(title)).Append("</div><div class='pd-hour-hover-scroll'><table class='pd-hour-hover-table'><thead><tr>");
            foreach (string column in columns) b.Append("<th>").Append(EncodePersonal(column)).Append("</th>");
            b.Append("</tr></thead><tbody><tr>");
            foreach (string value in values) b.Append("<td>").Append(EncodePersonal(value)).Append("</td>");
            return b.Append("</tr></tbody></table></div>").ToString();
        }
        private static string PersonalTaskLaborTooltip(PdDoiSoat ds, bool isIssue, string name)
        {
            string[] columns = { "Tự khai thường", "Tự khai tăng ca", "PM chốt thường", "PM chốt tăng ca", "Kết quả cuối cùng", "Đối soát" };
            string[] values = ds == null
                ? new[] { "Chưa ghi nhận công", "Chưa ghi nhận công", "—", "—", "Chưa ghi nhận công", "PM chưa chốt" }
                : new[] { PersonalLaborMinute(ds.ThuongTuKhai), PersonalLaborMinute(ds.TangCaTuKhai),
                    ds.DaPmChot ? PersonalLaborMinute(ds.ThuongPm) : "—",
                    ds.DaPmChot ? PersonalLaborMinute(ds.TangCaPm) : "—",
                    PersonalLaborHour(ds.PhutPm / 60m), ds.DaPmChot ? "PM đã chốt" : "PM chưa chốt" };
            string table = PersonalLaborTooltip((isIssue ? "Vấn đề: " : "Công việc: ") + name, columns, values);
            return isIssue ? table + "<div class='pd-hour-hover-note'>Công vấn đề gắn với công việc liên kết, không cộng trùng.</div>" : table;
        }
        private static string PersonalMeetingLaborTooltip(DataRow meeting, PersonalMeetingLabor data)
        {
            var columns = new[] { "Tham gia", "Công lý thuyết", "Công thực tế" };
            string[] values = data != null && data.LoiTaiDuLieu
                ? new[] { "Không tải được dữ liệu", "Không tải được dữ liệu", "Không tải được dữ liệu" }
                : data == null ? new[] { "Chưa xác nhận", "Chưa ghi nhận công", "Chưa ghi nhận công" }
                : new[] { data.ThamGia ? "Có tham gia" : "Không tham gia",
                    data.ThamGia ? PersonalHourText(PersonalMeetingHours(meeting)) : "Chưa ghi nhận công",
                    data.ThamGia && data.GioThucTe.HasValue ? PersonalLaborHour(data.GioThucTe.Value) : "Chưa ghi nhận công" };
            return PersonalLaborTooltip("Cuộc họp: " + PersonalText(meeting, "TenCuocHop"), columns, values);
        }
        private static void AppendPersonalHourAction(StringBuilder b, string kind, Guid id, string caption, string tooltip)
        {
            if (id == Guid.Empty) { b.Append("<span class='pd-personal-hours'><i class='far fa-clock'></i>").Append(EncodePersonal(caption)).Append("</span>"); return; }
            b.Append("<button type='button' class='pd-personal-hours pd-hour-hover-button' data-pd-labor-kind='").Append(kind)
                .Append("' data-pd-labor-id='").Append(id.ToString("D"))
                .Append("' aria-label='Xem đối soát và chi tiết giờ công'><i class='far fa-clock'></i>")
                .Append(EncodePersonal(caption)).Append("</button>")
                .Append("<div class='pd-hour-hover-source' hidden>").Append(tooltip).Append("</div>");
        }

        // Chỉ ghi mã lỗi an toàn ra giao diện; exception gốc luôn ở SysLogger.
        private static string PdLoadErrorCode(Exception exception)
        {
            SqlException sqlError = exception as SqlException;
            if (sqlError != null) return "SQL-" + sqlError.Number.ToString(CultureInfo.InvariantCulture);
            return exception is TimeoutException ? "TIMEOUT" : "APP-" + exception.GetType().Name;
        }

        private void BindPersonalProjectView()
        {
            Literal literal = FindProjectDetailControl(this, "litProjectPersonal") as Literal;
            if (literal == null) return;
            Guid userId = SweetContext.Current.UserId;
            if (QueryId == Guid.Empty || userId == Guid.Empty)
            {
                literal.Text = "<div class='pd-personal-empty'>Không xác định được người dùng hoặc dự án.</div>";
                return;
            }
            string buocTai = "Kết nối SQL";
            try
            {
                using (SqlConnection connection = new SqlConnection(PersonalConnectionString()))
                {
                    connection.Open();
                    buocTai = "Danh sách công việc";
                    DataTable tasks = LoadPersonalTasks(connection, QueryId, userId);
                    buocTai = "Danh sách cuộc họp";
                    DataTable meetings = PersonalQuery(connection, PersonalMeetingsSql, QueryId, userId);
                    buocTai = "Danh sách vấn đề";
                    DataTable issues = PersonalQuery(connection, PersonalIssuesSql, QueryId, userId);
                    buocTai = "Bản đối soát";
                    var snapshots = LoadDoiSoat(connection, QueryId, userId);
                    AttachDoiSoat(tasks, snapshots, "IdCongViec");
                    AttachDoiSoat(issues, snapshots, "IdCongViecBiAnhHuong");
                    buocTai = "Lịch làm việc";
                    PersonalWorkCalendar workCalendar = tasks.Rows.Count > 0
                        ? LoadPersonalWorkCalendar(connection, tasks, DateTime.Now)
                        : new PersonalWorkCalendar();

                    // Du lieu hover cuoc hop la phu tro; neu loi/timeout van phai hien du 4 bang.
                    var meetingLabor = new Dictionary<Guid, PersonalMeetingLabor>();
                    if (meetings.Rows.Count > 0)
                    {
                        try
                        {
                            meetingLabor = LoadPersonalMeetingLabor(connection, QueryId, userId);
                        }
                        catch (Exception hoverError)
                        {
                            SysLogger.LogError(hoverError, "DuAnDetail - Truy vấn giờ công hover cuộc họp thất bại; giữ nguyên danh sách cá nhân (ProjectId=" + QueryId + ")");
                            foreach (DataRow meet in meetings.Rows)
                            {
                                Guid meetId = PersonalGuid(meet, "IdLichHop");
                                if (meetId != Guid.Empty)
                                    meetingLabor[meetId] = new PersonalMeetingLabor { LoiTaiDuLieu = true };
                            }
                        }
                    }
                    buocTai = "Dựng giao diện Phần việc của tôi";
                    literal.Text = BuildPersonalHtml(tasks, meetings, issues, workCalendar, snapshots, meetingLabor);
                }
            }
            catch (Exception exception)
            {
                SysLogger.LogError(exception, "DuAnDetail - Lỗi tại bước: " + buocTai + " (ProjectId=" + QueryId + ")");
                literal.Text = "<div class='pd-personal-empty'><i class='fas fa-exclamation-circle me-2'></i>Không tải được danh sách cá nhân của dự án. "
                    + "Bước: " + EncodePersonal(buocTai) + " (" + EncodePersonal(PdLoadErrorCode(exception)) + "). Vui lòng kiểm tra log máy chủ.</div>";
            }
        }

        // =========================================================================================
        // CONG CUA THANH VIEN - PM/Admin only; query always scoped to this project + selected staff.
        // The current personal tab has NO tracked task/issue hours; meeting hours are schedule based.
        // =========================================================================================
        private bool CanViewProjectTeamHours()
        {
            if (QueryId == Guid.Empty) return false;
            if (this.IsAdministrator) return true;
            Guid? managerId = DuAnManager.Instance.LayIdNhanVienQuanLy(QueryId);
            return managerId.HasValue && managerId.Value == SweetContext.Current.UserId;
        }

        private sealed class TeamHoursPerson
        {
            public Guid Id;
            public string Name;
            public string Email;
            public string Avatar;
            public bool IsPM;
        }

        private List<TeamHoursPerson> GetProjectTeamHoursPeople()
        {
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(QueryId);
            var ids = (ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(QueryId) ?? new List<Guid>()).ToList();
            if (pmId.HasValue && pmId.Value != Guid.Empty) ids.Add(pmId.Value);
            var result = new List<TeamHoursPerson>();
            foreach (Guid id in ids.Where(x => x != Guid.Empty).Distinct())
            {
                AspnetUser user = UserManager.Instance.GetUserById(id);
                if (user == null) continue;
                string name = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                string email = string.Empty;
                try
                {
                    MembershipUser member = Membership.GetUser(user.UserName);
                    if (member != null) email = member.Email ?? string.Empty;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Team hours email lookup failed: {0}", ex.Message);
                }
                result.Add(new TeamHoursPerson
                {
                    Id = id,
                    Name = name ?? "Thành viên",
                    Email = string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email,
                    Avatar = user.Avatar,
                    IsPM = pmId.HasValue && pmId.Value == id
                });
            }
            return result.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private string TeamHoursAvatar(TeamHoursPerson person)
        {
            string color = GetAvatarBackground(person.Name);
            string fallback = "<span class='pd-team-avatar-fallback' style='background:" + color + ";'>"
                + EncodePersonal(GetInitials(person.Name)) + "</span>";
            string url = person.Avatar == null ? "" : person.Avatar.Trim();
            if (string.IsNullOrEmpty(url) || url.IndexOf("user-icon.png", StringComparison.OrdinalIgnoreCase) >= 0
                || url.IndexOf("no-file.png", StringComparison.OrdinalIgnoreCase) >= 0)
                return "<span class='pd-team-avatar'>" + fallback + "</span>";
            if (url.StartsWith("~/", StringComparison.Ordinal)) url = Page.ResolveUrl(url);
            bool safe = url.StartsWith("/", StringComparison.Ordinal) && !url.StartsWith("//", StringComparison.Ordinal)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            if (!safe) return "<span class='pd-team-avatar'>" + fallback + "</span>";
            string safeUrl = HttpUtility.HtmlEncode(url).Replace("'", "&#39;");
            return "<span class='pd-team-avatar'><img loading='lazy' src='" + safeUrl
                + "' alt='' onerror=\"this.style.display='none';this.nextElementSibling.style.display='grid';\" />"
                + "<span class='pd-team-avatar-fallback' hidden style='background:" + color + ";'>"
                + EncodePersonal(GetInitials(person.Name)) + "</span></span>";
        }

        // Theo CtrlMeetGioCongTab.RosterSql: PM override uu tien hon xac nhan cua nhan vien.
        // Chi lay cac cuoc hop DA KET THUC ma nhan vien la chu tri hoac duoc moi.
        // Khong nhan voi bang cong bang gio hop: no dung de xac nhan CO/KHONG tham gia.
        private static readonly string TeamHoursEndedMeetingsSql = @"
SELECT m.IdLichHop, m.TenCuocHop, m.ThoiGianBatDau, m.ThoiGianKetThuc,
       ISNULL(
         (SELECT MAX(CAST(g.DaThamGia AS INT))
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

        // Cong hop theo yeu cau: chi khi co tham gia, gio = THOI LUONG HOP.
        // SoGioThucTe cua CtrlMeet khong duoc su dung cho tong ly thuyet nay.
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

        private void BindProjectTeamHours()
        {
            if (!CanViewProjectTeamHours()) return;
            string buocTai = "Lấy danh sách nhân sự";
            bool daTaiDuocDanhSach = false;
            try
            {
                var people = GetProjectTeamHoursPeople();
                Guid selected;
                if (!Guid.TryParse(hfTeamHoursMember.Value, out selected)
                    || !people.Any(x => x.Id == selected)) selected = people.Count > 0 ? people[0].Id : Guid.Empty;
                hfTeamHoursMember.Value = selected == Guid.Empty ? "" : selected.ToString("D");
                StringBuilder left = new StringBuilder(people.Count * 320 + 150);
                left.Append("<div class='pd-team-actual-count' data-pd-total='").Append(people.Count).Append("' hidden></div>");
                if (people.Count == 0) left.Append("<div class='pd-team-record-empty'>Dự án chưa có nhân sự được phân công.</div>");
                foreach (var person in people)
                {
                    bool active = person.Id == selected;
                    left.Append("<button type='button' class='pd-team-person")
                        .Append(active ? " is-active" : "").Append("' data-pd-team-member='")
                        .Append(person.Id.ToString("D"))
                        .Append("' data-pd-search='")
                        .Append(HttpUtility.HtmlEncode((person.Name + " " + person.Email).ToLowerInvariant()).Replace("'", "&#39;"))
                        .Append("' aria-pressed='").Append(active ? "true" : "false")
                        .Append("'>").Append(TeamHoursAvatar(person))
                        .Append("<span class='pd-team-person-info'><span class='pd-team-person-name'>")
                        .Append(EncodePersonal(person.Name))
                        .Append("</span><span class='pd-team-person-email'>")
                        .Append(EncodePersonal(person.Email)).Append("</span></span>");
                    if (person.IsPM) left.Append("<span class='pd-team-pm'>PM</span>");
                    left.Append("</button>");
                }
                litProjectTeamMembers.Text = left.ToString();
                daTaiDuocDanhSach = true;
                var chosen = people.FirstOrDefault(x => x.Id == selected);
                if (chosen == null)
                {
                    litProjectTeamHours.Text = "<div class='pd-team-detail-head'><strong>Bảng công thành viên</strong></div><div class='pd-team-record-empty'>Chưa chọn được nhân viên.</div>";
                    return;
                }
                buocTai = "Kết nối SQL cho bảng công thành viên";
                using (SqlConnection conn = new SqlConnection(PersonalConnectionString()))
                {
                    conn.Open();
                    buocTai = "Danh sách công việc của thành viên";
                    DataTable tasks = LoadPersonalTasks(conn, QueryId, selected);
                    buocTai = "Danh sách cuộc họp của thành viên";
                    DataTable meetings = PersonalQuery(conn, TeamHoursEndedMeetingsSql, QueryId, selected);
                    buocTai = "Danh sách vấn đề của thành viên";
                    DataTable issues = PersonalQuery(conn, PersonalIssuesSql, QueryId, selected);
                    buocTai = "Bản đối soát của thành viên";
                    var snapshots = LoadDoiSoat(conn, QueryId, selected);
                    AttachDoiSoat(tasks, snapshots, "IdCongViec");
                    AttachDoiSoat(issues, snapshots, "IdCongViecBiAnhHuong");
                    buocTai = "Dựng giao diện bảng công thành viên";
                    litProjectTeamHours.Text = BuildProjectTeamHoursHtml(chosen, tasks, meetings, issues);
                }
            }
            catch (Exception exc)
            {
                SysLogger.LogError(exc, "DuAnDetail - Lỗi bảng công thành viên tại bước: " + buocTai + " (ProjectId=" + QueryId + ")");
                // Không xóa danh sách nhân sự đã tải thành công chỉ vì SQL giờ công lỗi.
                if (!daTaiDuocDanhSach)
                    litProjectTeamMembers.Text = "<div class='pd-team-record-empty'>Không tải được danh sách thành viên.</div>";
                litProjectTeamHours.Text = "<div class='pd-team-detail-head'><strong>Bảng công thành viên</strong></div>"
                    + "<div class='pd-team-record-empty'>Không tải được bảng công. Bước: "
                    + EncodePersonal(buocTai) + " (" + EncodePersonal(PdLoadErrorCode(exc)) + "). Vui lòng kiểm tra log máy chủ.</div>";
            }
        }

        protected void lbtLoadTeamHours_Click(object sender, EventArgs e)
        {
            // The server binds from the posted staff ID only after rechecking PM/Admin and project membership.
            if (!CanViewProjectTeamHours())
            {
                Response.StatusCode = 403;
                return;
            }
            // upProjectDetail re-renders the personal literal as well (EnableViewState=false).
            BindPersonalProjectView();
            upProjectDetail.Update();
        }

        private string BuildProjectTeamHoursHtml(TeamHoursPerson member, DataTable tasks, DataTable meetings, DataTable issues)
        {
            DateTime now = DateTime.Now;
            var taskRows = tasks.Rows.Cast<DataRow>().ToList();
            var meetingRows = meetings.Rows.Cast<DataRow>()
                .OrderByDescending(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MinValue).ToList();
            var issueRows = issues.Rows.Cast<DataRow>()
                .OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            // Chi cong cho nguoi duoc xac nhan tham gia theo CtrlMeet (ke ca override cua PM).
            // Tong la tong cac gia tri 2 chu so hien thi theo tung cuoc hop da ket thuc.
            decimal meetingHours = meetingRows.Sum(TeamMeetingLaborHours);
            decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows);
            decimal uniqueHours = DsUniqueSum(taskRows, issueRows);
            decimal totalHours = uniqueHours + meetingHours;
            StringBuilder sb = new StringBuilder(14000);
            sb.Append("<div class='pd-team-detail-head'><div class='pd-team-detail-person'><i class='far fa-clock'></i><span style='min-width:0'><span class='pd-team-detail-name'>Bảng công: ")
                .Append(EncodePersonal(member.Name))
                .Append("</span><span class='pd-team-detail-email'>")
                .Append(EncodePersonal(member.Email))
                .Append("</span></span></div><div class='pd-team-top-actions'>")
                .Append("<button type='button' class='pd-ds-expand' data-pd-detail-source='pdTeamFullDetail' data-pd-detail-title='Bảng công đầy đủ' data-pd-detail-sub='Chi tiết tự khai, PM chốt và kết quả của thành viên này' aria-label='Mở rộng toàn bộ bảng công' title='Mở rộng bảng công'><i class='fas fa-expand-arrows-alt'></i></button>")
                .Append("</div></div><div class='pd-team-detail-body'>");

            AppendTeamHoursSection(sb, "task", "Công việc", TeamLaborHourText(taskHours), taskRows.Count, taskRows, now);
            AppendTeamHoursSection(sb, "meeting", "Cuộc họp", TeamLaborHourText(meetingHours), meetingRows.Count, meetingRows, now);
            AppendTeamHoursSection(sb, "issue", "Vấn đề", TeamLaborHourText(issueHours), issueRows.Count, issueRows, now);
            sb.Append("<div class='pd-team-accordion is-total'><div class='pd-team-accordion-header'><span class='pd-team-accordion-title'>Tổng giờ công (PM chốt + họp)</span><strong class='pd-team-hours-value'>")
                .Append(TeamLaborHourText(totalHours))
                .Append("</strong></div></div><div class='pd-team-hours-footnote'>Tổng chỉ cộng một lần mỗi công việc đã chốt, tránh cộng trùng với vấn đề liên kết. Giờ họp vẫn tính theo các cuộc họp kết thúc đã xác nhận tham gia.</div></div>");
            return sb.ToString();
        }

        private void AppendTeamHoursSection(StringBuilder sb, string kind, string title, string hours,
            int count, List<DataRow> rows, DateTime now)
        {
            sb.Append("<div class='pd-team-accordion' data-pd-team-kind='").Append(kind).Append("'>")
                .Append("<button type='button' class='pd-team-accordion-header' data-pd-team-toggle='1' aria-expanded='false'>")
                .Append("<span class='pd-team-accordion-title'>").Append(EncodePersonal(title))
                .Append(" <span class='pd-team-accordion-count'>(").Append(count)
                .Append(")</span></span><strong class='pd-team-hours-value'>").Append(EncodePersonal(hours))
                .Append("</strong><i class='fas fa-chevron-down'></i></button>")
                .Append("<button type='button' class='pd-ds-expand pd-ds-expand-section' data-pd-detail-source='pdTeamDetail-")
                .Append(kind).Append("' data-pd-detail-title='Chi tiết ").Append(EncodePersonal(title))
                .Append("' data-pd-detail-sub='Bản hiện tại, giờ tự khai và giờ PM chốt' aria-label='Mở rộng ").Append(EncodePersonal(title))
                .Append("' title='Mở rộng ").Append(EncodePersonal(title)).Append("'><i class='fas fa-expand-arrows-alt'></i></button>")
                .Append("<div class='pd-team-accordion-content' hidden>");
            if (rows.Count == 0) sb.Append("<div class='pd-team-record-empty'>Không có ")
                    .Append(kind == "task" ? "công việc" : kind == "meeting" ? "cuộc họp" : "vấn đề")
                    .Append(" nào trong dự án.</div>");
            for (int i = 0; i < rows.Count; i++)
            {
                DataRow row = rows[i];
                string name, code = "", meta = "", state = "";
                string stateCss = "pd-team-state-neutral";
                decimal rowHours = 0m;
                if (kind == "task")
                {
                    rowHours = DsHour(row);
                    name = PersonalText(row, "TenCongViec");
                    code = PersonalText(row, "MaCongViec");
                    DateTime? due = PersonalDate(row, "NgayKetThuc");
                    meta = due.HasValue ? "Hạn " + due.Value.ToString("dd/MM/yyyy") : "Chưa có hạn";
                    int status = PersonalInt(row, "TrangThai");
                    bool taskDone = status == 2 || PersonalInt(row, "PhanTramHoanThanh") >= 100;
                    state = taskDone ? "Hoàn thành"
                        : status == 1 ? "Đang thực hiện" : status == 0 ? "Chưa bắt đầu" : "Đang xử lý";
                    stateCss = taskDone ? "pd-team-state-done"
                        : status == 1 ? "pd-team-state-active" : "pd-team-state-pending";
                }
                else if (kind == "meeting")
                {
                    name = PersonalText(row, "TenCuocHop");
                    DateTime? start = PersonalDate(row, "ThoiGianBatDau");
                    DateTime? finish = PersonalDate(row, "ThoiGianKetThuc");
                    bool joined = PersonalInt(row, "XacNhanThamGia") == 1;
                    rowHours = TeamMeetingLaborHours(row);
                    meta = (start.HasValue ? start.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa xếp lịch")
                        + (finish.HasValue ? " – " + finish.Value.ToString("HH:mm") : "")
                        + TeamMeetingDurationText(row);
                    // Tat ca rows da duoc loc ket thuc tai SQL. Badge the hien xac nhan.
                    // False tu CtrlMeet co the la "Khong tham gia" hoac chua tick Co.
                    state = joined ? "Có tham gia" : "Không tham gia";
                    stateCss = joined ? "pd-team-state-joined" : "pd-team-state-not-joined";
                }
                else
                {
                    rowHours = DsHour(row);
                    name = PersonalText(row, "TenVanDe");
                    DateTime? date = PersonalDate(row, "NgayTao");
                    meta = date.HasValue ? "Ngày " + date.Value.ToString("dd/MM/yyyy") : "Chưa có ngày";
                    int issueStatus = PersonalInt(row, "TrangThai");
                    state = PersonalIssueStatus(issueStatus);
                    stateCss = issueStatus == (int)TrangThaiVanDeEnum.Processed ? "pd-team-state-done"
                        : issueStatus == (int)TrangThaiVanDeEnum.Processing ? "pd-team-state-active"
                        : "pd-team-state-pending";
                }
                sb.Append("<div class='pd-team-record").Append(i >= 6 ? " is-extra" : "")
                    .Append("'><div class='pd-team-record-main'><div class='pd-team-record-title'>");
                if (kind == "task" || kind == "meeting")
                {
                    Guid recordId = PersonalGuid(row, kind == "task" ? "IdCongViec" : "IdLichHop");
                    sb.Append("<button type='button' class='pd-view-object-link' data-pd-view-kind='")
                        .Append(kind == "task" ? "task" : "meet")
                        .Append("' data-pd-view-id='").Append(recordId.ToString("D"))
                        .Append("' title='Xem chi tiết'>");
                    if (kind == "task" && !string.IsNullOrWhiteSpace(code))
                        sb.Append("<span class='pd-team-record-code'>")
                            .Append(EncodePersonal(code.Trim().TrimEnd('.') + ".")).Append("</span> ");
                    sb.Append(EncodePersonal(name)).Append("</button>");
                }
                else sb.Append(EncodePersonal(name));
                sb.Append("</div><div class='pd-team-record-meta'>")
                    .Append(EncodePersonal(meta)).Append("</div></div><span class='pd-team-record-aside'>")
                    .Append("<span class='pd-team-record-hours' title='")
                    .Append(kind == "meeting" ? "Giờ công họp: chỉ tính khi có xác nhận tham gia" : "Giờ công lấy từ PM chốt; chưa chốt = 0")
                    .Append("'>").Append(TeamLaborHourText(rowHours))
                    .Append("</span><span class='pd-team-record-state ").Append(stateCss)
                    .Append("'>").Append(EncodePersonal(state))
                    .Append("</span></span></div>");
            }
            if (rows.Count > 6) sb.Append("<button type='button' class='pd-team-show-all' data-pd-team-show-all='1' data-collapse-label='Xem đủ ")
                .Append(rows.Count).Append(" mục'>Xem đủ ").Append(rows.Count).Append(" mục</button>");
            sb.Append("</div></div>");
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


        private string BuildPersonalHtml(DataTable tasks, DataTable meetings, DataTable issues, PersonalWorkCalendar calendar, Dictionary<Guid, PdDoiSoat> labor, Dictionary<Guid, PersonalMeetingLabor> meetingLabor)
        {
            DateTime now = DateTime.Now;
            StringBuilder builder = new StringBuilder(32000);
            var taskRows = tasks.Rows.Cast<DataRow>()
                .OrderBy(r => { int s = PersonalInt(r, "TrangThai"); return s == 1 ? 0 : (s == 0 ? 1 : (s == 2 ? 3 : 2)); })
                .ThenBy(r => PersonalDate(r, "NgayKetThuc") ?? DateTime.MaxValue)
                .ThenBy(r => PersonalText(r, "MaCongViec"), StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var notEnded = meetings.Rows.Cast<DataRow>().Where(r => PersonalMeetingEnd(r) > now)
                .OrderBy(r => Math.Abs(((PersonalDate(r, "ThoiGianBatDau") ?? now) - now).TotalMinutes))
                .ThenBy(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MaxValue).ToList();
            var ended = meetings.Rows.Cast<DataRow>().Where(r => PersonalMeetingEnd(r) <= now)
                .OrderByDescending(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MinValue).ToList();
            var issueRows = issues.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows);
            decimal meetingHours = meetings.Rows.Cast<DataRow>().Sum(PersonalMeetingHours);
            // So doi soat gan voi IdCongViec, neu van de tham chieu cung task thi KHONG cong lap vao tong.
            decimal totalHours = DsUniqueSum(taskRows, issueRows) + meetingHours;

            builder.Append("<div class='pd-personal-grid'>");

            // Công việc
            builder.Append("<section class='pd-personal-card pd-personal-tasks' id='pdPersonalTaskCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='fas fa-tasks'></i><strong>Công việc được giao</strong></div><div class='pd-head-actions'><small>")
                   .Append(taskRows.Count).Append(" công việc</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='task' aria-pressed='false' title='Chỉ hiển thị công việc chưa hoàn thành'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (taskRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa được giao công việc nào trong dự án này.</div>");
            foreach (DataRow task in taskRows) AppendPersonalTaskCard(builder, task, now, calendar, labor);
            if (taskRows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có công việc chưa hoàn thành.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalTaskDetail' data-pd-detail-title='Chi tiết công việc được giao' data-pd-detail-sub='Danh sách công việc được phân công trực tiếp cho bạn trong dự án.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Cuộc họp
            builder.Append("<section class='pd-personal-card pd-personal-meetings' id='pdPersonalMeetingCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='far fa-calendar-alt'></i><strong>Cuộc họp của tôi</strong></div><div class='pd-head-actions'><small>")
                   .Append(meetings.Rows.Count).Append(" cuộc họp</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='meeting' aria-pressed='false' title='Chỉ hiển thị cuộc họp chưa kết thúc'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (meetings.Rows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa có cuộc họp nào trong dự án này.</div>");
            else
            {
                foreach (DataRow meet in notEnded) AppendPersonalMeeting(builder, meet, now, false, meetingLabor);
                foreach (DataRow meet in ended) AppendPersonalMeeting(builder, meet, now, true, meetingLabor);
            }
            if (meetings.Rows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có cuộc họp chưa kết thúc.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalMeetingDetail' data-pd-detail-title='Chi tiết cuộc họp của tôi' data-pd-detail-sub='Các cuộc họp bạn chủ trì hoặc tham gia trong dự án, ưu tiên hiển thị cuộc họp chưa kết thúc.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Vấn đề
            builder.Append("<section class='pd-personal-card pd-personal-issues' id='pdPersonalIssueCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='fas fa-exclamation-circle'></i><strong>Vấn đề được giao</strong></div><div class='pd-head-actions'><small>")
                   .Append(issueRows.Count).Append(" vấn đề</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='issue' aria-pressed='false' title='Chỉ hiển thị vấn đề chưa giải quyết'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (issueRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Chưa có vấn đề được phân công cho bạn.</div>");
            foreach (DataRow issue in issueRows) AppendPersonalIssueCard(builder, issue, labor);
            if (issueRows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có vấn đề chưa giải quyết.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalIssueDetail' data-pd-detail-title='Chi tiết vấn đề được giao' data-pd-detail-sub='Danh sách vấn đề bạn đang phụ trách trong dự án này.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Giờ công
            builder.Append("<section class='pd-personal-card pd-personal-time'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='far fa-clock'></i><strong>Giờ công của bạn</strong></div></div><div class='pd-card-body'><div class='pd-personal-list'><div class='pd-personal-summary'>");
            AppendSummaryRow(builder, "Tổng giờ làm công việc", taskHours, false);
            AppendSummaryRow(builder, "Tổng giờ công cuộc họp", meetingHours, false);
            AppendSummaryRow(builder, "Tổng giờ công xử lý vấn đề", issueHours, false);
            AppendSummaryRow(builder, "Tổng giờ công", totalHours, true);
            builder.Append("</div></div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalHourDetail' data-pd-detail-title='Chi tiết giờ công của bạn' data-pd-detail-sub='Công việc tính theo PM chốt, giờ họp theo quy tắc hiện tại, tránh cộng trùng với vấn đề liên kết.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");
            builder.Append("</div>");

            return builder.ToString();
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

        private static void AppendPersonalTaskCard(StringBuilder builder, DataRow task, DateTime now, PersonalWorkCalendar calendar, Dictionary<Guid, PdDoiSoat> labor)
        {
            PersonalTaskProgressInfo visual = GetPersonalTaskProgress(task, now, calendar);
            DateTime? dueDate = PersonalDate(task, "NgayKetThuc");
            builder.Append("<article class='pd-personal-item")
                   .Append(visual.Done ? " pd-task-done" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'><div class='pd-personal-item-title pd-personal-task-heading'><button type='button' class='pd-personal-task-name pd-view-object-link' data-pd-view-kind='task' data-pd-view-id='")
                   .Append(PersonalGuid(task, "IdCongViec").ToString("D"))
                   .Append("' title='Xem chi tiết công việc'>");
            string taskCode = PersonalText(task, "MaCongViec").Trim().TrimEnd('.');
            if (!string.IsNullOrEmpty(taskCode))
                builder.Append("<span class='pd-personal-task-code'>").Append(EncodePersonal(taskCode)).Append(".</span> ");
            builder.Append(EncodePersonal(task["TenCongViec"]))
                   .Append("</button><span class='pd-personal-tag ").Append(visual.StatusCss).Append("'>")
                   .Append(EncodePersonal(visual.Status)).Append("</span>");
            if (visual.LateFinished) builder.Append("<span class='pd-task-late-note'>(trễ hạn)</span>");
            builder.Append("</div><div class='pd-personal-due'>")
                   .Append(dueDate.HasValue ? "<span><i class='far fa-calendar-alt me-1'></i>Hạn " + dueDate.Value.ToString("dd/MM/yyyy") + "</span>" : "<span><i class='far fa-calendar-alt me-1'></i>Chưa có hạn</span>")
                   .Append("<span class='pd-task-percent' data-pd-progress-tooltip='Tiến độ theo thời gian' tabindex='0' aria-label='Tiến độ theo thời gian'>")
                   .Append(visual.Percent).Append("%</span></div>")
                   .Append("</div>");
            Guid taskId = PersonalGuid(task, "IdCongViec");
            PdDoiSoat taskLabor;
            labor.TryGetValue(taskId, out taskLabor);
            AppendPersonalHourAction(builder, "task", taskId, DsCompact(task),
                PersonalTaskLaborTooltip(taskLabor, false, PersonalText(task, "MaCongViec") + " · " + PersonalText(task, "TenCongViec")));
            builder.Append("</div>").Append(PersonalTaskProgressBar(visual)).Append("</article>");
        }

        private static void AppendPersonalMeeting(StringBuilder builder, DataRow meet, DateTime now, bool ended, Dictionary<Guid, PersonalMeetingLabor> meetingLabor)
        {
            DateTime? start = PersonalDate(meet, "ThoiGianBatDau");
            DateTime? finish = PersonalDate(meet, "ThoiGianKetThuc");
            builder.Append("<article class='pd-personal-item")
                   .Append(ended ? " pd-meeting-ended" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'>")
                   .Append("<div class='pd-personal-item-title pd-personal-task-heading'><button type='button' class='pd-personal-task-name pd-view-object-link' data-pd-view-kind='meet' data-pd-view-id='")
                   .Append(PersonalGuid(meet, "IdLichHop").ToString("D"))
                   .Append("' title='Xem chi tiết cuộc họp'>")
                   .Append(EncodePersonal(meet["TenCuocHop"]))
                   .Append("</button>")
                   .Append(PersonalMeetingBadge(start, finish, now))
                   .Append("</div><div class='pd-personal-schedule'>")
                   .Append("<div class='pd-personal-schedule-line'><i class='far fa-calendar-alt'></i><span>")
                   .Append(PersonalMeetingDayText(start, finish))
                   .Append("</span></div><div class='pd-personal-schedule-line'><i class='far fa-clock'></i><span>")
                   .Append(PersonalMeetingTimeText(start, finish))
                   .Append("</span></div></div></div>");
            Guid meetId = PersonalGuid(meet, "IdLichHop");
            PersonalMeetingLabor ml;
            meetingLabor.TryGetValue(meetId, out ml);
            decimal actualHours = ml != null && ml.ThamGia && ml.GioThucTe.HasValue ? ml.GioThucTe.Value : 0m;
            string laborLabel = ml != null && ml.LoiTaiDuLieu ? "Không tải được giờ công"
                : actualHours > 0m ? "Giờ công: " + PersonalHourValue(actualHours) : "Chưa ghi nhận công";
            AppendPersonalHourAction(builder, "meeting", meetId, laborLabel, PersonalMeetingLaborTooltip(meet, ml));
            builder.Append("</div></article>");
        }

        private void AppendPersonalIssueCard(StringBuilder builder, DataRow issue, Dictionary<Guid, PdDoiSoat> labor)
        {
            int status = PersonalInt(issue, "TrangThai");
            bool processed = status == (int)TrangThaiVanDeEnum.Processed;
            bool processing = status == (int)TrangThaiVanDeEnum.Processing;
            DateTime? created = PersonalDate(issue, "NgayTao");
            builder.Append("<article class='pd-personal-item")
                   .Append(processed ? " pd-issue-processed" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'>")
                   .Append("<div class='pd-personal-item-title pd-personal-task-heading'><span class='pd-personal-task-name'>")
                   .Append(EncodePersonal(issue["TenVanDe"]))
                   .Append("</span><span class='pd-personal-tag ")
                   .Append(processed ? "is-done" : processing ? "is-progress" : string.Empty)
                   .Append("'>")
                   .Append(EncodePersonal(PersonalIssueStatus(status)))
                   .Append("</span></div><div class='pd-personal-meta'>")
                   .Append("<span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(created.HasValue ? "Ngày tạo " + created.Value.ToString("dd/MM/yyyy") : "Chưa có ngày tạo")
                   .Append("</span></div>")
                   .Append("</div>");
            Guid linkedTaskId = PersonalGuid(issue, "IdCongViecBiAnhHuong");
            PdDoiSoat issueLabor;
            labor.TryGetValue(linkedTaskId, out issueLabor);
            AppendPersonalHourAction(builder, "issue", PersonalGuid(issue, "IdVanDe"), DsCompact(issue),
                PersonalTaskLaborTooltip(issueLabor, true, PersonalText(issue, "TenVanDe")));
            builder.Append("</div></article>");
        }




        protected void rptRecentProjectHistory_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = e.Item.DataItem as DataRowView;

            if (row == null)
                return;

            Label lblHistoryContent = e.Item.FindControl("lblHistoryContent") as Label;

            Label lblHistoryTime = e.Item.FindControl("lblHistoryTime") as Label;

            if (lblHistoryContent != null)
            {
                lblHistoryContent.Text = HttpUtility.HtmlEncode(BuildHistoryContent(row.Row));
            }

            if (lblHistoryTime != null)
            {
                lblHistoryTime.Text = FormatHistoryTime(row.Row);
            }
        }

        protected void lbtViewAllHistory_Click(object sender, EventArgs e)
        {
            CtrlLichSuDuAn1.IdDuAn = QueryId;

            CtrlLichSuDuAn1.OpenDrawer();
        }

        protected void lbtViewContract_Click(
    object sender,
    EventArgs e)
        {
            if (!this.IsView)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (this.IdHopDongThucHien ==
                Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(this.IdHopDongThucHien);

            if (hopDong == null)
            {
                ShowInvalidNotFoundData();
                return;
            }

            BindContractInformation(hopDong);

            Response.Redirect(RewriteURLHelper.ContractDetail(this.IdHopDongThucHien));
        }


        private void BindContractInformation(TblHopDongThucHien hopDong)
        {
            txtContractNumber.Text = hopDong.SoHopDong;
            txtContractName.Text = hopDong.TenHopDong;

            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(hopDong.IdKhachHang);
            txtContractCustomer.Text = khachHang == null ? "Chưa có" : khachHang.TenKhachHang;
            txtContractValue.Text = hopDong.GiaTriHopDong.HasValue
                ? FormatHelpers.ConvertDecimalToStringByLanguage(hopDong.GiaTriHopDong.Value, "vi-VN")
                : string.Empty;
            txtContractSignDate.Text = hopDong.NgayKy.HasValue
                ? hopDong.NgayKy.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractEffectiveDate.Text = hopDong.NgayHieuLuc.HasValue
                ? hopDong.NgayHieuLuc.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractExpiryDate.Text = hopDong.NgayHetHan.HasValue
                ? hopDong.NgayHetHan.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractDescription.Text = hopDong.MoTa;
        }

        protected void lbtEditProject_Click(object sender, EventArgs e)
        {
            Guid idDuAn = this.QueryId;

            if (idDuAn == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            CtrlDuAnForm1.OpenEdit(idDuAn);
        }

        protected void lbtThemThanhVien_Click(object sender, EventArgs e)
        {
            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(this.QueryId);

            if (duAn == null || duAn.DaXoa)
            {
                ShowInvalidDataError();
                return;
            }

            if (duAn.NgayBatDau == null || duAn.NgayDuKienHoanThanh == null)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_START_AND_END_DATE), MSGType.Error);
                return;
            }

            CtrlChonNhanVien1.StartDate = duAn.NgayBatDau;
            CtrlChonNhanVien1.EndDate = duAn.NgayDuKienHoanThanh;
            CtrlChonNhanVien1.SelectedUserIds = DuAnManager.Instance.GetMemberIds(duAn.IdDuAn);
            CtrlChonNhanVien1.IdNhanVienQuanLy = duAn.IdNhanVienQuanLy;
            CtrlChonNhanVien1.OpenPicker();
        }

        protected void CtrlChonNhanVien1_OnConfirmSelection(List<Guid> selectedIds)
        {
            try
            {
                TblDuAn duAn = DuAnManager.Instance.GetDuAnById(this.QueryId);
                if (duAn == null || duAn.DaXoa)
                {
                    ShowInvalidDataError();
                    return;
                }

                DuAnManager.Instance.CreateOrUpdate(duAn, selectedIds);

                BindData();
                upProjectDetail.Update();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        private string BuildHistoryContent(DataRow row)
        {
            string resourceKey = GetColumnText(row, "Description");

            string actor = GetColumnText(row, "ChangedBy");

            string tableName = GetColumnText(row, "TableName");

            string title = GetColumnText(row, "Title");

            if (string.IsNullOrWhiteSpace(actor) || string.Equals(actor, "[System]", StringComparison.OrdinalIgnoreCase))
            {
                actor = "Hệ thống";
            }

            if (string.IsNullOrWhiteSpace(title))
                title = "Chưa xác định";

            string entityName = GetHistoryEntityName(tableName);

            string template = string.IsNullOrWhiteSpace(resourceKey)
                    ? null
                    : GetResourceText(resourceKey);

            if (string.IsNullOrWhiteSpace(template) || string.Equals(template, resourceKey, StringComparison.OrdinalIgnoreCase))
            {
                return BuildDefaultHistoryContent(row, actor, entityName, title);
            }

            bool isContainerAction = string.Equals(resourceKey, BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER, StringComparison.OrdinalIgnoreCase) || string.Equals(resourceKey, BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER, StringComparison.OrdinalIgnoreCase);
            if (isContainerAction)
            {
                string containerName = GetResourceText(BackEndResourceKeys.PROJECT);
                return string.Format(template, actor,          // {0}: Administrator
                    title,          // {1}: Nguyễn Thị A
                    containerName);
            }

            try
            {
                return string.Format(template, actor, entityName, title);
            }
            catch (FormatException)
            {
                return BuildDefaultHistoryContent(row, actor, entityName, title);
            }
        }

        private string GetHistoryEntityName(string tableName)
        {
            switch (tableName)
            {
                case nameof(TblDuAn): return "dự án";

                case nameof(TblGiaiDoanDuAn): return "giai đoạn";

                case nameof(TblCongViec): return "công việc";

                case nameof(TblThanhVienDuAn): return "thành viên dự án";

                case nameof(TblHopDongThucHien): return "hợp đồng thực hiện";

                case nameof(TblRuiRoDuAn): return "rủi ro";

                case nameof(TblChiPhi): return "khoản chi";

                default: return "thông tin";
            }
        }

        private string BuildDefaultHistoryContent(DataRow row, string actor, string entityName, string title)
        {
            string actionType = GetColumnText(row, "ActionType");

            switch (actionType)
            {
                case "CREATE": return string.Format("{0} đã thêm {1} \"{2}\".", actor, entityName, title);

                case "UPDATE": return string.Format("{0} đã cập nhật {1} \"{2}\".", actor, entityName, title);

                case "DELETE": return string.Format("{0} đã xóa {1} \"{2}\".", actor, entityName, title);

                default: return string.Format("{0} đã thao tác trên {1} \"{2}\".", actor, entityName, title);
            }
        }

        private string FormatHistoryTime(DataRow row)
        {
            if (!row.Table.Columns.Contains("ChangedAt") || row["ChangedAt"] == null || row["ChangedAt"] == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime changedAt = Convert.ToDateTime(row["ChangedAt"]);

            return changedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        private string GetColumnText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }

        private void BindProjectProgress()
        {
            var tienDo = DuAnManager.Instance.GetDuAnTienDo(QueryId);

            if (tienDo.TienDoThoiGian.HasValue)
            {
                int phanTramThoiGian = (int)Math.Round(tienDo.TienDoThoiGian.Value);
                lblTienDoThoiGian.InnerText = phanTramThoiGian + "%";
                divTienDoThoiGian.Attributes["aria-valuenow"] = phanTramThoiGian.ToString();
                divTienDoThoiGianBar.Style["width"] = phanTramThoiGian + "%";
            }
            else
            {
                lblTienDoThoiGian.InnerText = "—";
                divTienDoThoiGian.Attributes["aria-valuenow"] = "0";
                divTienDoThoiGianBar.Style["width"] = "0%";
            }

            if (tienDo.TienDoCongViec.HasValue)
            {
                int phanTramCongViec = (int)Math.Round(tienDo.TienDoCongViec.Value);
                lblTienDoCongViec.InnerText = phanTramCongViec + "%";
                divTienDoCongViec.Attributes["aria-valuenow"] = phanTramCongViec.ToString();
                divTienDoCongViecBar.Style["width"] = phanTramCongViec + "%";
            }
            else
            {
                lblTienDoCongViec.InnerText = "—";
                divTienDoCongViec.Attributes["aria-valuenow"] = "0";
                divTienDoCongViecBar.Style["width"] = "0%";
            }
        }

        private void BindProjectTeam()
        {
            // 1. Get PM ID
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(QueryId);

            if (pmId.HasValue && pmId.Value != Guid.Empty)
            {
                var pm = UserManager.Instance.GetUserById(pmId.Value);
                if (pm != null)
                {
                    lblNhanVienQuanLy.Text = pm.DisplayName;
                    imgAvatarPM.Src = !string.IsNullOrEmpty(pm.Avatar) && !pm.Avatar.Contains("no-file.png") ? pm.Avatar : "/Styles/images/user-icon.png";
                }
                else
                {
                    lblNhanVienQuanLy.Text = "Không xác định";
                    imgAvatarPM.Src = "/Styles/images/user-icon.png";
                }
            }
            else
            {
                lblNhanVienQuanLy.Text = "Chưa có Quản lý";
                imgAvatarPM.Src = "/Styles/images/user-icon.png";
            }

            // 2. Get Members
            List<Guid> memberIds = SweetSoft.QLDA.Core.Managers.ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(QueryId);

            // Remove PM from members list if present
            if (pmId.HasValue)
            {
                memberIds.RemoveAll(id => id == pmId.Value);
            }

            // Distinct IDs to avoid duplicates
            memberIds = memberIds.Distinct().ToList();

            if (memberIds.Count == 0)
            {
                ltrThanhVienGroup.Text = "";
                return;
            }

            // Get User objects
            List<SweetSoft.QLDA.DataAccess.AspnetUser> members = new List<SweetSoft.QLDA.DataAccess.AspnetUser>();
            foreach (var id in memberIds)
            {
                var user = SweetSoft.QLDA.Core.Managers.UserManager.Instance.GetUserById(id);
                if (user != null)
                {
                    members.Add(user);
                }
            }

            // Render Avatar Group
            int maxVisible = 6;
            int hiddenCount = members.Count > maxVisible ? members.Count - maxVisible : 0;
            var visibleMembers = members.Take(maxVisible).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"d-flex align-items-center flex-wrap gap-2\">");
            sb.Append("<div class=\"avatar-group\">");
            sb.Append("<div class=\"avatar-stack-container\">");

            foreach (var member in visibleMembers)
            {
                bool hasAvatar = !string.IsNullOrEmpty(member.Avatar) && !member.Avatar.Contains("no-file.png") && !member.Avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
                string title = member.DisplayName;

                if (hasAvatar)
                {
                    string avatarUrl = member.Avatar.StartsWith("~")
                        ? Page.ResolveUrl(member.Avatar)
                        : member.Avatar;

                    string safeAvatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                    string safeTitle = HttpUtility.HtmlAttributeEncode(title);

                    sb.Append(
                        $"<img src=\"{safeAvatarUrl}\" " +
                        $"class=\"avatar-circle\" " +
                        $"style=\"object-fit: cover;\" " +
                        $"title=\"{safeTitle}\" />"
                    );
                }
                else
                {
                    sb.Append(GetFallbackAvatarHtml(title));
                }
            }

            sb.Append("</div>"); // close avatar-stack-container

            if (hiddenCount > 0)
            {
                sb.Append($"<div class=\"avatar-circle avatar-more\">+{hiddenCount}</div>");
            }

            sb.Append("</div>"); // close avatar-group
            sb.Append("</div>"); // close d-flex gap-2

            ltrThanhVienGroup.Text = sb.ToString();
        }

        private string GetFallbackAvatarHtml(string fullName)
        {
            string initials = GetInitials(fullName);
            string bgColor = GetAvatarBackground(fullName);
            string safeName = HttpUtility.HtmlAttributeEncode(fullName);

            return $"<div class=\"avatar-circle\" " +
                   $"style=\"background-color: {bgColor};\" " +
                   $"title=\"{safeName}\">{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] words = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                return words[0].Substring(0, 1).ToUpper();
            }
            else if (words.Length > 1)
            {
                return (words[0].Substring(0, 1) + words[words.Length - 1].Substring(0, 1)).ToUpper();
            }
            return "?";
        }

        private string GetAvatarBackground(string fullName)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899", "#ef4444", "#06b6d4" };
            if (string.IsNullOrWhiteSpace(fullName)) return colors[0];

            int hash = Math.Abs(fullName.GetHashCode());
            return colors[hash % colors.Length];
        }

        // Trường hợp đồng là dữ liệu liên kết, không phải cột của TblDuAn.
        // Quyền sửa hợp đồng được kiểm tra riêng để tránh ghi sai phạm vi quyền dự án.
        private bool CanEditInlineContract()
        {
            return this.IsEdit && this.IsUserRight(ActionKeys.Update, ModuleKeys.Contract) && IdHopDongThucHien != Guid.Empty;
        }

        private T ProjectInlineControl<T>(string id) where T : Control
        {
            return FindProjectDetailControl(this, id) as T;
        }

        private static string InlineDateText(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
        }

        private static bool ParseInlineMoney(string raw, out decimal? amount)
        {
            amount = null;
            if (string.IsNullOrWhiteSpace(raw)) return true;
            decimal parsed;
            if (!decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out parsed) &&
                !decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
                return false;
            if (parsed < 0 || parsed > 9999999999999999m || decimal.Round(parsed, 2) != parsed) return false;
            amount = parsed;
            return true;
        }

        private void BindProjectInlineEditor(DataRow row)
        {
            ExtraDropdown customer = ProjectInlineControl<ExtraDropdown>("ddlInlineKhachHang");
            ExtraDropdown type = ProjectInlineControl<ExtraDropdown>("ddlInlineLoaiDuAn");
            TextBox start = ProjectInlineControl<TextBox>("txtInlineNgayBatDau");
            TextBox price = ProjectInlineControl<TextBox>("txtInlineGiaTriHopDong");
            TextBox sign = ProjectInlineControl<TextBox>("txtInlineNgayKy");
            Panel actions = ProjectInlineControl<Panel>("pnlProjectInlineActions");
            if (customer == null || type == null || start == null || price == null || sign == null) return;

            if (!IsPostBack || customer.Items.Count == 0) new ControlHelpers().BindKhachHang(customer);
            if (!IsPostBack || type.Items.Count == 0) new ControlHelpers().BindLoaiDuAn(type);
            TblDuAn project = DuAnManager.Instance.GetDuAnById(QueryId);
            if (project == null) return;
            string customerId = project.IdKhachHang.ToString();
            string typeId = project.IdLoaiDuAn.ToString();
            if (customer.Items.FindByValue(customerId) != null) customer.SelectedValue = customerId;
            if (type.Items.FindByValue(typeId) != null) type.SelectedValue = typeId;
            start.Text = InlineDateText(project.NgayBatDau);

            bool editable = this.IsEdit && project.TrangThai != (byte)DuAnStatus.HoanThanh && project.TrangThai != (byte)DuAnStatus.KetThuc;
            customer.Enabled = editable;
            type.Enabled = editable;
            start.Enabled = editable;
            bool contractEditable = editable && CanEditInlineContract();
            price.Enabled = contractEditable;
            sign.Enabled = contractEditable;
            if (actions != null) actions.Visible = editable;

            if (IdHopDongThucHien != Guid.Empty)
            {
                TblHopDongThucHien contract = HopDongThucHienManager.Instance.GetHopDongById(IdHopDongThucHien);
                price.Text = contract != null && contract.GiaTriHopDong.HasValue
                    ? contract.GiaTriHopDong.Value.ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) : string.Empty;
                sign.Text = contract != null ? InlineDateText(contract.NgayKy) : string.Empty;
            }
            else
            {
                price.Text = string.Empty;
                sign.Text = string.Empty;
                price.ToolTip = sign.ToolTip = "Dự án chưa liên kết hợp đồng thực hiện.";
            }
            if (!contractEditable && IdHopDongThucHien != Guid.Empty)
                price.ToolTip = sign.ToolTip = "Cần quyền chỉnh sửa hợp đồng để thay đổi thông tin này.";
        }

        protected void btnInlineSaveProject_Click(object sender, EventArgs e)
        {
            try
            {
                TblDuAn project = DuAnManager.Instance.GetDuAnById(QueryId);
                if (project == null || project.DaXoa || !this.IsEdit ||
                    (!this.IsAdministrator && ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(SweetContext.Current.UserId, QueryId) == null) ||
                    project.TrangThai == (byte)DuAnStatus.HoanThanh ||
                    project.TrangThai == (byte)DuAnStatus.KetThuc)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                ExtraDropdown customer = ProjectInlineControl<ExtraDropdown>("ddlInlineKhachHang");
                ExtraDropdown type = ProjectInlineControl<ExtraDropdown>("ddlInlineLoaiDuAn");
                TextBox start = ProjectInlineControl<TextBox>("txtInlineNgayBatDau");
                TextBox price = ProjectInlineControl<TextBox>("txtInlineGiaTriHopDong");
                TextBox sign = ProjectInlineControl<TextBox>("txtInlineNgayKy");
                Guid customerId, typeId;
                DateTime startDate;
                if (customer == null || type == null || start == null ||
                    !Guid.TryParse(customer.SelectedValue, out customerId) || customerId == Guid.Empty ||
                    !Guid.TryParse(type.SelectedValue, out typeId) || typeId == Guid.Empty ||
                    customer.Items.FindByValue(customerId.ToString()) == null ||
                    type.Items.FindByValue(typeId.ToString()) == null ||
                    !DateTime.TryParseExact(start.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                {
                    ShowNotify("Vui lòng chọn khách hàng, loại dự án và ngày bắt đầu hợp lệ.", MSGType.Error);
                    return;
                }
                if (startDate.Date > project.NgayDuKienHoanThanh.Date)
                {
                    ShowNotify("Ngày bắt đầu không thể sau ngày hoàn thành dự kiến.", MSGType.Error);
                    return;
                }

                bool hasContract = project.IdHopDongThucHien.HasValue && project.IdHopDongThucHien.Value != Guid.Empty;
                bool updateContract = hasContract && this.IsUserRight(ActionKeys.Update, ModuleKeys.Contract);
                decimal? amount = null;
                DateTime signDate = DateTime.MinValue;
                if (updateContract && (price == null || sign == null ||
                    !ParseInlineMoney(price.Text, out amount) ||
                    (!string.IsNullOrWhiteSpace(sign.Text) &&
                     !DateTime.TryParseExact(sign.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out signDate))))
                {
                    ShowNotify("Giá trị hợp đồng hoặc ngày ký không hợp lệ.", MSGType.Error);
                    return;
                }

                // Thay đổi thuộc tính thuộc dự án và hợp đồng liên kết trong cùng một giao dịch.
                using (SqlConnection connection = new SqlConnection(PersonalConnectionString()))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = new SqlCommand(@"UPDATE dbo.TblDuAn
SET IdKhachHang=@Customer, IdLoaiDuAn=@Type, NgayBatDau=@Start
WHERE IdDuAn=@Project AND ISNULL(DaXoa,0)=0 AND TrangThai NOT IN (@Done,@Closed);", connection, transaction))
                            {
                                command.Parameters.Add("@Customer", SqlDbType.UniqueIdentifier).Value = customerId;
                                command.Parameters.Add("@Type", SqlDbType.UniqueIdentifier).Value = typeId;
                                command.Parameters.Add("@Start", SqlDbType.DateTime).Value = startDate.Date;
                                command.Parameters.Add("@Project", SqlDbType.UniqueIdentifier).Value = QueryId;
                                command.Parameters.Add("@Done", SqlDbType.TinyInt).Value = (byte)DuAnStatus.HoanThanh;
                                command.Parameters.Add("@Closed", SqlDbType.TinyInt).Value = (byte)DuAnStatus.KetThuc;
                                if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Dự án đã thay đổi hoặc không còn được phép cập nhật.");
                            }
                            if (updateContract)
                            {
                                using (SqlCommand command = new SqlCommand(@"UPDATE dbo.TblHopDongThucHien
SET GiaTriHopDong=@Price, NgayKy=@Sign
WHERE IdHopDongThucHien=@Contract
  AND EXISTS (SELECT 1 FROM dbo.TblDuAn WHERE IdDuAn=@Project AND IdHopDongThucHien=@Contract);", connection, transaction))
                                {
                                    SqlParameter money = command.Parameters.Add("@Price", SqlDbType.Decimal);
                                    money.Precision = 18; money.Scale = 2;
                                    money.Value = amount.HasValue ? (object)amount.Value : DBNull.Value;
                                    command.Parameters.Add("@Sign", SqlDbType.DateTime).Value = signDate == DateTime.MinValue ? (object)DBNull.Value : signDate.Date;
                                    command.Parameters.Add("@Contract", SqlDbType.UniqueIdentifier).Value = project.IdHopDongThucHien.Value;
                                    command.Parameters.Add("@Project", SqlDbType.UniqueIdentifier).Value = QueryId;
                                    if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không tìm thấy hợp đồng được liên kết với dự án.");
                                }
                            }
                            transaction.Commit();
                        }
                        catch { transaction.Rollback(); throw; }
                    }
                }
                BindData();
                upProjectDetail.Update();
                ShowNotify("Đã cập nhật thông tin dự án.", MSGType.Success);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Không thể cập nhật thông tin dự án trực tiếp");
                ShowNotify("Không thể lưu thông tin dự án. Vui lòng kiểm tra dữ liệu và thử lại.", MSGType.Error);
            }
        }

        private void BindProjectInformation(DataRow row)
        {
            Navigation1.MainTitle = GetDisplayText(row, "MaDuAn");
            lblTenDuAn.Text = GetDisplayText(row, "TenDuAn");
            lblKhachHang.Text = GetDisplayText(row, "TenKhachHang");
            lblLoaiDuAn.Text = GetDisplayText(row, "TenLoaiDuAn");
            lblSoHopDong.Text = GetDisplayText(row, "SoHopDong");
            lblGiaTriHopDong.Text = FormatMoney(row, "GiaTriHopDong");
            lblNgayKy.Text = FormatDate(row, "NgayKy");
            lblNgayBatDau.Text = FormatDate(row, "NgayBatDau");
            lblNgayHoanThanhDuKien.Text = FormatDate(row, "NgayDuKienHoanThanh");
            lblNgayKetThucThucTe.Text = FormatDate(row, "NgayHoanThanhThucTe");
            ltrMoTa.Text = GetHtmlText(row, "MoTa");
            lblNhanVienQuanLy.Text = GetDisplayText(row, "DisplayName");

            string avatarUrl = Convert.ToString(row["Avatar"]);
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                imgAvatarPM.Src = avatarUrl;
            }
            else
            {
                imgAvatarPM.Src = "~/Styles/images/user-icon.png";
            }

            byte trangThai = Convert.ToByte(row["TrangThai"]);
            switch ((DuAnStatus)trangThai)
            {
                case DuAnStatus.TamDung:
                    lblNgayKetThucThucTeTitle.Text = "Ngày tạm dừng";
                    break;
                case DuAnStatus.HoanThanh:
                    lblNgayKetThucThucTeTitle.Text = "Ngày hoàn thành thực tế";
                    break;
                case DuAnStatus.KetThuc:
                    lblNgayKetThucThucTeTitle.Text = "Ngày kết thúc";
                    break;
                default:
                    lblNgayKetThucThucTeTitle.Text = "Ngày hoàn thành thực tế";
                    break;
            }
            lblTrangThai.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), (DuAnStatus)trangThai));
            lblTrangThaiHead.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), (DuAnStatus)trangThai));
            string statusBadgeClass = "pd-project-status-badge " + ProjectStatusBadgeClass((DuAnStatus)trangThai);
            lblTrangThaiHead.CssClass = statusBadgeClass;
            lblTrangThai.CssClass = statusBadgeClass;
            CurrentStatusValue = trangThai;
            DuAnStatus statusEnum = (DuAnStatus)trangThai;
            //ltrCurrentStatusName.Text = EnumHelpers.GetERenderText(typeof(DuAnStatus), statusEnum);
            //iCurrentStatusIcon.Attributes["class"] = "fas fa-circle me-2 small " + GetStatusCssClass(statusEnum);
            BindStatusActions(statusEnum);

            Guid idHopDongThucHien = Guid.Empty;
            if (row.Table.Columns.Contains("IdHopDongThucHien") && row["IdHopDongThucHien"] != DBNull.Value)
            {
                Guid.TryParse(Convert.ToString(row["IdHopDongThucHien"]), out idHopDongThucHien);
            }

            this.IdHopDongThucHien = idHopDongThucHien;

            lbtViewContract.Visible = idHopDongThucHien != Guid.Empty;

            lblNoContract.Visible = idHopDongThucHien == Guid.Empty;

            lbtEditProject.Visible = CanModifyProject;

            lbtThemThanhVien.Visible = CanModifyProject;

            BindProjectInlineEditor(row);
        }

        private string GetDisplayText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "Chưa có";
            }
            return HttpUtility.HtmlEncode(value);
        }

        private string GetHtmlText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "<p class='text-muted'>Chưa có nội dung mô tả</p>";
            }
            return HttpUtility.HtmlDecode(value);
        }

        private string FormatDate(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            DateTime value = Convert.ToDateTime(row[columnName]);
            return DateTimeHelper.ConvertDateTime(value, false);
        }

        private string FormatMoney(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            decimal value = Convert.ToDecimal(row[columnName]);
            return FormatHelpers.ConvertDecimalToStringByLanguage(value, "vi-VN");
        }

        private int CalculateProgressByDay(DataRow row)
        {
            if (!row.Table.Columns.Contains("NgayBatDau") || row["NgayBatDau"] == DBNull.Value)
                return 0;
            if (!row.Table.Columns.Contains("NgayDuKienHoanThanh") || row["NgayDuKienHoanThanh"] == DBNull.Value)
                return 0;
            DateTime ngayBatDau = Convert.ToDateTime(row["NgayBatDau"]);
            DateTime ngayDuKien = Convert.ToDateTime(row["NgayDuKienHoanThanh"]);
            DateTime ngayHienTai = DateTime.Now;
            double tongNgay = (ngayDuKien - ngayBatDau).TotalDays;
            if (tongNgay <= 0) return 100;
            double ngayDaQua = (ngayHienTai - ngayBatDau).TotalDays;
            int phanTram = (int)Math.Round((ngayDaQua / tongNgay) * 100);
            return Math.Max(0, Math.Min(100, phanTram));
        }

        private void BindRecentProjectHistory()
        {
            if (rptRecentProjectHistory == null)
            {
                throw new InvalidOperationException("Không tìm thấy control rptRecentProjectHistory trong DuAnDetail.aspx.");
            }

            if (pnlEmptyRecentHistory == null)
            {
                throw new InvalidOperationException("Không tìm thấy control pnlEmptyRecentHistory trong DuAnDetail.aspx.");
            }

            DataTable history = _auditManager.GetRecentProjectHistory(QueryId, 5);

            bool hasData = history != null && history.Rows.Count > 0;

            rptRecentProjectHistory.Visible = hasData;

            pnlEmptyRecentHistory.Visible = !hasData;

            if (!hasData)
            {
                rptRecentProjectHistory.DataSource = null;

                rptRecentProjectHistory.DataBind();
                return;
            }

            rptRecentProjectHistory.DataSource = history;

            rptRecentProjectHistory.DataBind();
        }

        protected void lbtStatusDangThucHien_Click(object sender, EventArgs e)
        {
            ChangeProjectStatus(DuAnStatus.DangThucHien);
        }

        protected void lbtStatusTamDung_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            OpenStatusChangeModal(DuAnStatus.TamDung.ToString());
        }

        protected void lbtStatusHoanThanh_Click(object sender, EventArgs e)
        {
            ChangeProjectStatus(DuAnStatus.HoanThanh);
        }

        protected void lbtStatusKetThuc_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            OpenStatusChangeModal(DuAnStatus.KetThuc.ToString());
        }

        private void ChangeProjectStatus(DuAnStatus newStatus)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                DuAnManager.Instance.UpdateProjectStatus(QueryId, newStatus, txtStatusReason.Text);
                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        private void BindStatusActions(DuAnStatus currentStatus)
        {
            lbtStatusDangThucHien.Visible = false;
            lbtStatusTamDung.Visible = false;
            lbtStatusHoanThanh.Visible = false;
            lbtStatusKetThuc.Visible = false;

            if (!this.IsEdit) return;

            switch (currentStatus)
            {
                case DuAnStatus.ChuaBatDau:
                    lbtStatusDangThucHien.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;

                case DuAnStatus.DangThucHien:
                    lbtStatusTamDung.Visible = true;
                    lbtStatusHoanThanh.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;

                case DuAnStatus.TamDung:
                    lbtStatusDangThucHien.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;
            }
        }

        private static string ProjectStatusBadgeClass(DuAnStatus status)
        {
            switch (status)
            {
                case DuAnStatus.ChuaBatDau: return "pd-status-not-started";
                case DuAnStatus.DangThucHien: return "pd-status-in-progress";
                case DuAnStatus.TamDung: return "pd-status-paused";
                case DuAnStatus.HoanThanh: return "pd-status-completed";
                case DuAnStatus.KetThuc: return "pd-status-ended";
                default: return "pd-status-ended";
            }
        }

        protected string GetStatusCssClass(DuAnStatus status)
        {
            switch (status)
            {
                case DuAnStatus.ChuaBatDau: return "text-warning";
                case DuAnStatus.DangThucHien: return "text-info";
                case DuAnStatus.TamDung: return "text-secondary";
                case DuAnStatus.HoanThanh: return "text-success";
                case DuAnStatus.KetThuc: return "text-dark";
                default: return "text-primary";
            }
        }

        private void CtrlDuAnForm1_SaveCompleted(object sender, EventArgs e)
        {
            BindData();
            upProjectDetail.Update();
        }

        private void OpenStatusChangeModal(string targetStatus)
        {
            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(QueryId);
            if (duAn == null || duAn.DaXoa)
            {
                ShowInvalidNotFoundData();
                return;
            }

            hfStatusTarget.Value = targetStatus;
            hfStatusAtModalOpen.Value = duAn.TrangThai.ToString();
            txtStatusReason.Text = string.Empty;

            bool isPause = targetStatus == DuAnStatus.TamDung.ToString();
            mdlStatusChange.Title = isPause ? "Xác nhận tạm dừng dự án" : "Xác nhận kết thúc dự án";
            lblStatusDescription.InnerText = isPause
                ? "Dự án sẽ chuyển sang trạng thái Tạm dừng. Vui lòng nhập lý do."
                : "Dự án sẽ chuyển sang trạng thái Kết thúc. Vui lòng nhập lý do.";

            upStatusChange.Update();
            mdlStatusChange.OpenModal(true);
        }

        protected void lbtConfirmStatusChange_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            string target = hfStatusTarget.Value;
            DuAnStatus newStatus;

            if (!Enum.TryParse(target, out newStatus) ||
                (newStatus != DuAnStatus.TamDung && newStatus != DuAnStatus.KetThuc))
            {
                ShowInvalidDataError();
                return;
            }

            string reason = (txtStatusReason.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                ShowNotify("Vui lòng nhập lý do.", MSGType.Warning);
                mdlStatusChange.OpenModal(true);
                return;
            }

            if (reason.Length > 1000)
            {
                ShowNotify("Lý do không được vượt quá 1000 ký tự.", MSGType.Warning);
                mdlStatusChange.OpenModal(true);
                return;
            }

            try
            {
                TblDuAn duAn = DuAnManager.Instance.GetDuAnById(QueryId);

                if (duAn == null || duAn.DaXoa)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                byte statusAtModalOpen;
                if (!byte.TryParse(hfStatusAtModalOpen.Value, out statusAtModalOpen) ||
                    duAn.TrangThai != statusAtModalOpen)
                {
                    ShowNotify("Trạng thái dự án đã thay đổi. Vui lòng tải lại trang.", MSGType.Warning);
                    return;
                }

                // Chỉ cho phép hai thao tác mà modal này phục vụ.
                if (newStatus == DuAnStatus.TamDung && duAn.TrangThai != (byte)DuAnStatus.DangThucHien)
                {
                    ShowNotify("Chỉ có thể tạm dừng dự án đang thực hiện.", MSGType.Warning);
                    return;
                }

                if (newStatus == DuAnStatus.KetThuc &&
                    duAn.TrangThai != (byte)DuAnStatus.ChuaBatDau &&
                    duAn.TrangThai != (byte)DuAnStatus.DangThucHien &&
                    duAn.TrangThai != (byte)DuAnStatus.TamDung)
                {
                    ShowNotify("Không thể kết thúc dự án ở trạng thái hiện tại.", MSGType.Warning);
                    return;
                }

                // TODO: Lưu reason cùng lần cập nhật trạng thái ở DuAnManager.
                DuAnManager.Instance.UpdateProjectStatus(QueryId, newStatus, reason);

                BindData();
                upProjectDetail.Update();
                mdlStatusChange.CloseModal();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
                mdlStatusChange.OpenModal(true);
            }
        }
    }
}

