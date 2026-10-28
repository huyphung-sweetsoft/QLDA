using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlViewTaskDetail : BaseAdminUserControl
    {
        // FindControl thay vì field designer mới: an toàn khi chưa regenerate .ascx.designer.cs.
        protected CtrlChiTietCong TaskLaborControl
        {
            get { return FindControl("tvdLaborPopup") as CtrlChiTietCong; }
        }

        public void OpenModal(Guid taskId)
        {
            if (taskId == Guid.Empty) return;

            TblCongViec task = TblCongViec.FetchByID(taskId);
            if (task == null || task.DaXoa == true) return;

            BindTask(task);
            upTaskView.Update();
            mdlTaskView.OpenModal(true);
            // Lịch chỉ xem: khởi tạo lại sau khi UpdatePanel thay nội dung modal.
            ScriptManager.RegisterStartupScript(upTaskView, upTaskView.GetType(),
                "TaskDetailCalendarInit", "setTimeout(function(){if(window.QLDATaskDetailCalendar)window.QLDATaskDetailCalendar.init();},220);", true);
        }

        private void BindTask(TblCongViec task)
        {
            // 1. Gộp Mã và Tên công việc lên Tiêu đề
            string maCv = HttpUtility.HtmlEncode(task.MaCongViec ?? "");
            string tenCv = HttpUtility.HtmlEncode(task.TenCongViec ?? "");
            lblTaskName.Text = string.IsNullOrEmpty(maCv) ? tenCv : $"{maCv}. {tenCv}";
            lblTaskName.Attributes["data-tvd-code"] = task.MaCongViec ?? string.Empty;

            // 2. Thuộc giai đoạn (Lấy tên từ bảng TblGiaiDoanDuAn)
            lblPhaseName.Text = "—";
            if (task.IdGiaiDoanDuAn.HasValue)
            {
                TblCongViec phase = TblCongViec.FetchByID(task.IdGiaiDoanDuAn.Value);
                if (phase != null && !string.IsNullOrWhiteSpace(phase.TenCongViec))
                {
                    lblPhaseName.Text = HttpUtility.HtmlEncode(phase.TenCongViec);
                }
            }

            // =========================================================
            // CẬP NHẬT TRẠNG THÁI + ĐỔI MÀU GIAO DIỆN BẰNG JAVASCRIPT
            // =========================================================
            string bgColor, borderColor, textColor;
            lblStatus.Text = GetTaskStatusText(task.TrangThai, out bgColor, out borderColor, out textColor);

            string jsColor = $@"
                setTimeout(function() {{
                    var lbl = document.getElementById('{lblStatus.ClientID}');
                    if (lbl) {{
                        var box = lbl.closest('.task-meta-item');
                        if (box) {{
                            box.style.backgroundColor = '{bgColor}';
                            box.style.borderColor = '{borderColor}';
                            
                            // Ép nhãn 'TRẠNG THÁI' về màu xám đen mặc định
                            var metaLabel = box.querySelector('.meta-label');
                            if (metaLabel) metaLabel.style.color = '#475569';
                        }}
                        lbl.style.color = '{textColor}';
                    }}
                }}, 50);";
            ScriptManager.RegisterStartupScript(this, GetType(), "ColorStatusBox", jsColor, true);
            // =========================================================

            // Độ ưu tiên
            if (task.IdDoUuTien.HasValue)
            {
                TblDoUuTien priority = TblDoUuTien.FetchByID(task.IdDoUuTien.Value);
                lblPriority.Text = priority != null ? HttpUtility.HtmlEncode(priority.TenDoUuTien) : "—";
            }
            else lblPriority.Text = "—";

            // 3. Xếp ô vuông vức: Ngày bắt đầu - Thời hạn - Ngày kết thúc - Hoàn thành TT
            lblStartDate.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("dd/MM/yyyy") : "—";
            lblDuration.Text = task.ThoiHanNgay.HasValue ? $"{task.ThoiHanNgay.Value} ngày" : "—";
            lblEndDate.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "—";
            lblActualEndDate.Text = task.NgayHoanThanhThucTe.HasValue ? task.NgayHoanThanhThucTe.Value.ToString("dd/MM/yyyy HH:mm") : "—";

            // 4. Nếu có Lý do trễ thì mới hiện Box đỏ ra
            if (!string.IsNullOrWhiteSpace(task.LyDoTre))
            {
                divDelayReason.Visible = true;
                lblDelayReason.Text = HttpUtility.HtmlEncode(task.LyDoTre);
            }
            else
            {
                divDelayReason.Visible = false;
            }

            ltrDescription.Text = ToDisplayHtml(task.MoTa);

            Guid projectId;
            Guid.TryParse(Convert.ToString(task.IdDuAn), out projectId);
            BindAssignees(task.IdCongViec, projectId);
        }

        // Cùng nguyên tắc bản đối soát hiện tại với CtrlChiTietCong:
        // Kết quả cuối chỉ sử dụng phút được PM chốt; không suy đoán số giờ nếu chưa có dữ liệu.
        private sealed class TaskStaffLabor
        {
            public decimal? TuKhaiThuong;
            public decimal? TuKhaiTangCa;
            public decimal? PmChotThuong;
            public decimal? PmChotTangCa;
            public bool DaPmChot;
        }

        private static Dictionary<Guid, TaskStaffLabor> LoadStaffLabor(Guid taskId, Guid projectId)
        {
            var result = new Dictionary<Guid, TaskStaffLabor>();
            if (taskId == Guid.Empty || projectId == Guid.Empty) return result;
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) &&
                        item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    { setting = item; break; }
                }
            }
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new ConfigurationErrorsException("Chưa cấu hình kết nối dữ liệu giờ công.");

            const string sql = @"WITH HienTai AS (
    SELECT s.IdNhanVien, s.SoPhutThuongTuKhai, s.SoPhutTangCaTuKhai,
           s.SoPhutThuongPmChot, s.SoPhutTangCaPmChot, s.IdPmChot,
           ROW_NUMBER() OVER (PARTITION BY s.IdNhanVien, s.IdCongViec
             ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS rn
    FROM dbo.TblKhaiBaoGioDoiSoat s
    INNER JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
       AND t.IdDuAn=s.IdDuAn AND ISNULL(t.DaXoa,0)=0
    WHERE s.IdCongViec=@TaskId AND s.IdDuAn=@ProjectId AND s.LaBanHienTai=1
) SELECT * FROM HienTai WHERE rn=1;";
            using (var conn = new SqlConnection(setting.ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 30;
                cmd.Parameters.Add("@TaskId", SqlDbType.UniqueIdentifier).Value = taskId;
                cmd.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = projectId;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid userId = (Guid)reader["IdNhanVien"];
                        object approvedId = reader["IdPmChot"];
                        bool approved = approvedId != DBNull.Value && (Guid)approvedId != Guid.Empty;
                        result[userId] = new TaskStaffLabor
                        {
                            TuKhaiThuong = StaffMinutes(reader["SoPhutThuongTuKhai"]),
                            TuKhaiTangCa = StaffMinutes(reader["SoPhutTangCaTuKhai"]),
                            PmChotThuong = StaffMinutes(reader["SoPhutThuongPmChot"]),
                            PmChotTangCa = StaffMinutes(reader["SoPhutTangCaPmChot"]),
                            DaPmChot = approved
                        };
                    }
                }
            }
            return result;
        }

        private static decimal? StaffMinutes(object value)
        {
            return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }

        private static string StaffMinuteText(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " phút" : "—";
        }

        private static string BuildStaffLaborHtml(TaskStaffLabor item, bool loadFailed)
        {
            // Chỉ kết quả PM đã chốt mới là giờ công ghi nhận; không thay dữ liệu đối soát.
            decimal minutes = item != null && item.DaPmChot
                ? (item.PmChotThuong ?? 0m) + (item.PmChotTangCa ?? 0m) : 0m;
            if (loadFailed)
                return "<span class='tvd-staff-hour-text is-empty'>Không tải được giờ công</span>";
            if (minutes <= 0m)
                return "<span class='tvd-staff-hour-text is-empty'>Chưa ghi nhận công</span>";
            string hours = (minutes / 60m).ToString("0.##", CultureInfo.GetCultureInfo("vi-VN"));
            string note = "PM đã chốt: thường " + StaffMinuteText(item.PmChotThuong)
                + ", tăng ca " + StaffMinuteText(item.PmChotTangCa);
            return "<span class='tvd-staff-hour-text' title='"
                + HttpUtility.HtmlAttributeEncode(note) + "'><i class='far fa-clock'></i> "
                + HttpUtility.HtmlEncode(hours) + " giờ công</span>";
        }

        private static void AppendStaffMinuteCell(StringBuilder b, string label, decimal? value)
        {
            b.Append("<span class='tvd-labor-piece'><small>")
             .Append(HttpUtility.HtmlEncode(label)).Append("</small><strong>")
             .Append(StaffMinuteText(value)).Append("</strong></span>");
        }

        private void BindAssignees(Guid taskId, Guid projectId)
        {
            List<TblCongViecNhanVien> assignments = new Select()
                .From(TblCongViecNhanVien.Schema)
                .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(taskId)
                .ExecuteTypedList<TblCongViecNhanVien>();

            List<Guid> assignedIds = assignments
                .Select(x => x.IdNhanVien).Where(x => x != Guid.Empty).Distinct().ToList();

            var assignedUsers = new List<KeyValuePair<Guid, AspnetUser>>();
            foreach (Guid userId in assignedIds)
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId);
                if (user != null) assignedUsers.Add(new KeyValuePair<Guid, AspnetUser>(userId, user));
            }
            assignedUsers = assignedUsers.OrderBy(x => string.IsNullOrWhiteSpace(x.Value.DisplayName)
                ? x.Value.UserName : x.Value.DisplayName).ToList();

            Dictionary<Guid, TaskStaffLabor> snapshots;
            bool failed = false;
            try { snapshots = LoadStaffLabor(taskId, projectId); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Không tải được đối soát giờ công tại CtrlViewTaskDetail: " + ex);
                snapshots = new Dictionary<Guid, TaskStaffLabor>();
                failed = true;
            }

            // Không điều hướng sang DuAnDetail: nút của chính nhân viên mở popup chồng trực tiếp lên ViewTask.
            Guid actor = SweetContext.Current.UserId;
            if (TaskLaborControl != null) TaskLaborControl.ProjectId = projectId;

            List<object> result = new List<object>();
            for (int i = 0; i < assignedUsers.Count; i++)
            {
                Guid userId = assignedUsers[i].Key;
                AspnetUser user = assignedUsers[i].Value;
                string displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                string email = GetUserEmail(user.UserName);
                TaskStaffLabor item;
                snapshots.TryGetValue(userId, out item);
                string ownDetail = userId == actor && projectId != Guid.Empty
                    ? "<button type='button' class='tvd-staff-detail-link' data-tvd-labor-task-id='" + taskId.ToString("D") +
                      "' title='Mở bảng công công việc của bạn ngay trên cửa sổ này'><i class='fas fa-expand-alt'></i> Xem chi tiết</button>"
                    : string.Empty;
                result.Add(new
                {
                    DisplayName = displayName,
                    Email = string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email,
                    AvatarHtml = GetAvatarHtml(displayName, user.Avatar, i),
                    LaborHtml = BuildStaffLaborHtml(item, failed),
                    DetailLinkHtml = ownDetail
                });
            }

            rptAssignees.DataSource = result;
            rptAssignees.DataBind();
            pnlNoAssignees.Visible = result.Count == 0;
        }

        private string GetTaskStatusText(object statusObj, out string bgColor, out string borderColor, out string textColor)
        {
            if (statusObj == null || statusObj == DBNull.Value)
            {
                bgColor = "#f1f5f9"; borderColor = "#cbd5e1"; textColor = "#475569"; // Xám
                return "Chưa bắt đầu";
            }
            int status = Convert.ToInt32(statusObj);
            if (status == 1)
            {
                bgColor = "#e0f2fe"; borderColor = "#7dd3fc"; textColor = "#0369a1"; // Xanh biển
                return "Đang thực hiện";
            }
            if (status == 2)
            {
                bgColor = "#dcfce7"; borderColor = "#86efac"; textColor = "#15803d"; // Xanh lá
                return "Hoàn thành";
            }
            if (status == 3)
            {
                bgColor = "#dcfce7"; borderColor = "#86efac"; textColor = "#15803d"; // Khung xanh lá
                // Trả về HTML: chữ "Hoàn thành" màu xanh lá (kế thừa), chữ "(Trễ hạn)" màu đỏ
                return "Hoàn thành <span style='color: #dc2626; font-weight: bold;'>(Trễ hạn)</span>";
            }

            bgColor = "#f1f5f9"; borderColor = "#cbd5e1"; textColor = "#475569"; // Mặc định xám
            return "Chưa bắt đầu";
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return "";
            try
            {
                MembershipUser user = Membership.GetUser(userName);
                return user != null ? user.Email : "";
            }
            catch { return ""; }
        }

        private string GetAvatarHtml(string displayName, string avatar, int index)
        {
            string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
            string color = colors[index % colors.Length];
            string safeName = HttpUtility.HtmlEncode(displayName ?? "");
            string initials = HttpUtility.HtmlEncode(GetInitials(displayName));
            bool isDefaultAvatar = string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~", StringComparison.Ordinal) ? Page.ResolveUrl(avatar) : avatar;
                avatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                string fallbackHtml = $"<div class='task-person-avatar' style='background:{color};'>{initials}</div>";
                return $"<img src='{avatarUrl}' class='task-person-avatar' alt='{safeName}' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }
            return $"<div class='task-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "<span class='text-muted'>—</span>";
            return HttpUtility.HtmlEncode(value).Replace("\r\n", "<br />").Replace("\n", "<br />");
        }
    }
}
