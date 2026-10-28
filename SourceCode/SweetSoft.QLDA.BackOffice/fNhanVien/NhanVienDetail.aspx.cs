using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Models;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SweetSoft.QLDA.Core.ScheduleManager;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class NhanVienDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE { get { return ModuleKeys.NhanVien; } }

        private Guid CurrentIdNhanVien
        {
            get { return ViewState["IdNhanVien"] != null ? (Guid)ViewState["IdNhanVien"] : Guid.Empty; }
            set { ViewState["IdNhanVien"] = value; }
        }
        protected Guid CurrentDetailProjectId { get; set; }
        // Chỉ thay cách xưng hô trên UI, không thay phân quyền đọc báo cáo của trang.
        protected bool IsOwnEmployeeProfile { get { return CurrentIdNhanVien != Guid.Empty && CurrentIdNhanVien == SweetContext.Current.UserId; } }

        protected string GetProjectStatusCss(object rawStatus)
        {
            byte code;
            if (rawStatus == null || !byte.TryParse(Convert.ToString(rawStatus), out code)) return "";
            switch ((DuAnStatus)code)
            {
                case DuAnStatus.ChuaBatDau: return "nvd-project-not-started";
                case DuAnStatus.DangThucHien: return "nvd-project-in-progress";
                case DuAnStatus.TamDung: return "nvd-project-paused";
                case DuAnStatus.HoanThanh: return "nvd-project-complete";
                case DuAnStatus.KetThuc: return "nvd-project-ended";
                default: return "";
            }
        }
        protected string GetProjectCardInsight(object item)
        {
            var project = item as NhanVienProjectSummaryDTO;
            if (project == null) return "Chi tiết giờ công của dự án.";
            if (project.DaKetToanDuAn) return "Các công việc phát sinh giờ đã được kết toán.";
            if (project.LaDuAnKetThuc) return "Dự án đã kết thúc; giờ công vẫn còn chờ kết toán.";
            if ((DuAnStatus)project.TrangThai == DuAnStatus.ChuaBatDau) return project.CoGioHeThong ? "Có dữ liệu giờ cần kiểm tra." : "Chưa phát sinh giờ công.";
            if ((DuAnStatus)project.TrangThai == DuAnStatus.TamDung) return "Đang tạm dừng; các giờ đã ghi vẫn được bảo lưu.";
            int pending = Math.Max(0, project.SoTaskLienQuan-project.SoTaskDaChot);
            if (pending > 0) return "Còn " + pending + " công việc của nhân viên chưa được chốt giờ.";
            return "Giờ công đang được cập nhật theo tiến độ dự án.";
        }
        // UX V4: Một số giờ công trong card, không cộng ba nguồn số liệu với nhau.
        // Khi dự án đã kết toán: dùng tổng PM chốt; ngược lại: giờ hệ thống đã ghi nhận.
        // Giữ nhãn rõ ràng để người dùng không hiểu số tạm tính là giờ đã duyệt.
        protected string GetProjectListHoursLabel(object item)
        {
            var project = item as NhanVienProjectSummaryDTO;
            if (project == null) return "Giờ công của nhân viên";
            if (project.DaKetToanDuAn) return project.CoPmChot ? "Tổng giờ công đã kết toán" : "Giờ công của nhân viên";
            if (project.CoGioHeThong) return "Giờ công đã ghi nhận";
            if (project.CoPmChot) return "Giờ công đã được PM chốt";
            return "Giờ công của nhân viên";
        }

        protected string GetProjectListHoursValue(object item)
        {
            var project = item as NhanVienProjectSummaryDTO;
            if (project == null) return "—";
            if (project.DaKetToanDuAn) return project.CoPmChot ? project.GioPmChotText : "Chưa phát sinh";
            if (project.CoGioHeThong) return project.GioHeThongText;
            if (project.CoPmChot) return project.GioPmChotText;
            return "Chưa phát sinh";
        }

        protected string GetProjectListHoursNote(object item)
        {
            var project = item as NhanVienProjectSummaryDTO;
            if (project == null) return "Xem chi tiết để biết nguồn dữ liệu.";
            if (project.DaKetToanDuAn) return project.CoPmChot ? "Tổng giờ đã được PM xác nhận sau kết toán." : "Dự án đã kết toán; nhân viên chưa có giờ công được chốt.";
            if (project.CoGioHeThong)
                return project.CoPhienDangMo
                    ? "Số liệu tạm tính; chưa bao gồm đầy đủ phiên đang chạy."
                    : "Số liệu hệ thống đã ghi nhận; chưa phải tổng giờ kết toán.";
            if (project.CoPmChot) return "Mới có giờ được chốt ở một phần công việc; xem chi tiết.";
            if ((DuAnStatus)project.TrangThai == DuAnStatus.ChuaBatDau)
                return "Dự án chưa bắt đầu, chưa có giờ công ghi nhận.";
            return "Chưa có giờ được ghi nhận; xem chi tiết nếu cần.";
        }

        protected string GetTaskWorkProgressText(object taskStatus, object daChot)
        {
            bool completed = daChot != null && Convert.ToBoolean(daChot);
            if (completed) return "Giờ công đã được PM chốt";
            int st;
            if (!int.TryParse(Convert.ToString(taskStatus), out st)) return "Giờ công chưa chốt";
            if (st == 2 || st == 3) return "Công việc hoàn thành · Chờ PM chốt giờ";
            if (st == 4) return "Công việc chờ duyệt hoàn thành · Giờ chưa chốt";
            if (st == 1) return "Công việc đang thực hiện · Giờ chưa chốt";
            return "Công việc chưa bắt đầu · Giờ chưa chốt";
        }
        public override bool IsLogin { get { return true; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool isFromProfile = CommonHelpers.QueryString("from") == "profile";
            CtrlNhanVienPopup1.SavedHandlerCallback += (s, ev) =>
            {
                LoadDataDetail(CurrentIdNhanVien, isFromProfile);
                upnlMainDetail.Update();
            };

            if (!IsPostBack)
            {
                string idQuery = CommonHelpers.QueryString("id");
                Guid tempId = Guid.Empty;

                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId);
                }

                bool hasViewRight = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien);

                if (!hasViewRight && tempId != SweetContext.Current.UserId)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }

                var navLinks = new Dictionary<string, string>();
                if (isFromProfile)
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.Profile), GetResourceText(BackEndResourceKeys.PROFILE));
                }
                else
                {
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.NhanVien), GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST));
                }
                navLinks.Add("javascript:void(0);", GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));

                Navigation1.keyValuePairUrls = navLinks;
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);

                ApplyControlsText();
                CtrlNhanVienPopup1.InitControls();

                if (tempId != Guid.Empty)
                {
                    CurrentIdNhanVien = tempId;
                    LoadDataDetail(tempId, isFromProfile);
                }
                else
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                }
            }
        }

        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL);
            btnEditProfile.Text = btnEditProfile.ToolTip = GetResourceText(BackEndResourceKeys.EDIT_INFORMATION);
            SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));
        }

        // Render options from the SAME DuAnStatus enum used for the colored status badge.
        // This select is purely client-side; it never triggers a WebForms postback.
        protected string GetDuAnStatusOptionsHtml()
        {
            var html = new System.Text.StringBuilder();
            foreach (DuAnStatus status in Enum.GetValues(typeof(DuAnStatus)))
            {
                int code=Convert.ToInt32(status);
                if(code<0 || code>255)continue;
                html.Append("<option value='").Append(code).Append("'>")
                    .Append(HttpUtility.HtmlEncode(GetDuAnStatusText(code))).Append("</option>");
            }
            return html.ToString();
        }

        private void LoadDataDetail(Guid idNhanVien, bool isFromProfile)
        {
            DataTable dt = UserManager.Instance.GetUserForDetail(idNhanVien);

            if (dt == null || dt.Rows.Count == 0)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                return;
            }

            DataRow row = dt.Rows[0];
            string notUpdatedText = GetResourceText(BackEndResourceKeys.NOT_UPDATED_YET);

            ltrTenNhanVien.Text = row["DisplayName"].ToString();
            ltrGioiTinh.Text = !string.IsNullOrEmpty(row["GioiTinh"].ToString()) ? row["GioiTinh"].ToString() : notUpdatedText;
            // Địa chỉ mới đồng bộ 3 trường trong CtrlNhanVienPopup: DiaChi / MaTinh / MaXa.
            // GetUserForDetail đi qua Repository và có thể chưa SELECT MaTinh, MaXa.
            string diaChiChiTiet = row["DiaChi"] == DBNull.Value ? string.Empty : row["DiaChi"].ToString().Trim();
            string maTinh = ReadOptionalColumn(row, "MaTinh");
            string maXa = ReadOptionalColumn(row, "MaXa");
            if (!row.Table.Columns.Contains("MaTinh") || !row.Table.Columns.Contains("MaXa"))
            {
                AspnetUser nhanVien = UserManager.Instance.GetUserById(idNhanVien);
                if (nhanVien != null)
                {
                    maTinh = nhanVien.MaTinh;
                    maXa = nhanVien.MaXa;
                }
            }

            ThongTinDiaChiHanhChinh tenDiaChi =
                UserManager.Instance.GetTenDiaChiHanhChinh(maTinh, maXa);

            string tenTinh = tenDiaChi.TenTinhThanh;
            string tenXa = tenDiaChi.TenXaPhuong;
            string diaChiChuaCapNhat = GetResourceText(BackEndResourceKeys.ADDRESS_NOT_UPDATED_YET);
            if (string.IsNullOrWhiteSpace(diaChiChuaCapNhat)) diaChiChuaCapNhat = notUpdatedText;

            ltrTinhThanh.Text = EncodeOrEmptyText(tenTinh, notUpdatedText); 
            ltrXaPhuong.Text = EncodeOrEmptyText(tenXa, notUpdatedText);
            ltrDiaChi.Text = EncodeOrEmptyText(diaChiChiTiet, diaChiChuaCapNhat);

            // Tóm tắt địa chỉ chỉ xuất hiện khi có tỉnh hoặc xã, tránh lặp lại 
            // nguyên vẹn mỗi "Địa chỉ chi tiết" của bản ghi cũ.
            boxDiaChiDayDu.Visible = !string.IsNullOrWhiteSpace(tenTinh) || !string.IsNullOrWhiteSpace(tenXa);
            if (boxDiaChiDayDu.Visible)
            {
                var diaChiParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(diaChiChiTiet)) diaChiParts.Add(diaChiChiTiet);
                if (!string.IsNullOrWhiteSpace(tenXa) &&
                    diaChiChiTiet.IndexOf(tenXa, StringComparison.OrdinalIgnoreCase) < 0)
                    diaChiParts.Add(tenXa);
                if (!string.IsNullOrWhiteSpace(tenTinh) &&
                    diaChiChiTiet.IndexOf(tenTinh, StringComparison.OrdinalIgnoreCase) < 0)
                    diaChiParts.Add(tenTinh);
                ltrDiaChiDayDu.Text = HttpUtility.HtmlEncode(string.Join(", ", diaChiParts));
            }
            else
            {
                ltrDiaChiDayDu.Text = string.Empty;
            }

            if (row["NgaySinh"] != DBNull.Value)
            {
                ltrNgaySinh.Text = Convert.ToDateTime(row["NgaySinh"]).ToString("dd/MM/yyyy");
            }
            else
            {
                ltrNgaySinh.Text = notUpdatedText;
            }

            ltrChucDanh.Text = !string.IsNullOrEmpty(row["TenChucDanh"].ToString()) ? row["TenChucDanh"].ToString() : notUpdatedText;
            ltrPhongBan.Text = !string.IsNullOrEmpty(row["TenPhongBan"].ToString()) ? row["TenPhongBan"].ToString() : notUpdatedText;

            ltrEmail.Text = !string.IsNullOrEmpty(row["Email"].ToString()) && !row["Email"].ToString().Contains("no-email.com") ? row["Email"].ToString() : notUpdatedText;
            ltrPhone.Text = !string.IsNullOrEmpty(row["MobileAlias"].ToString()) ? row["MobileAlias"].ToString() : notUpdatedText;


            if (row["NgayGiaNhap"] != DBNull.Value)
            {
                DateTime joinDate = Convert.ToDateTime(row["NgayGiaNhap"]);
                ltrNgayGiaNhap.Text = joinDate.ToString("dd/MM/yyyy");
                ltrThamNien.Text = CalculateSeniority(joinDate);
            }
            else
            {
                ltrNgayGiaNhap.Text = notUpdatedText;
                ltrThamNien.Text = GetResourceText(BackEndResourceKeys.UNIDENTIFIED);
            }

            string avatar = row["Avatar"].ToString();
            if (!string.IsNullOrEmpty(avatar))
            {
                imgAvatar.Src = avatar;
            }

            btnEditProfile.Visible = this.IsEdit && !isFromProfile;
            string lichUrl = GetRelativeClientPath(RewriteURLHelper.ViewLichCaNhan(idNhanVien));
            lnkSchedule.HRef = isFromProfile ? lichUrl + "?from=profile" : lichUrl;
            lnkSchedule.Visible = this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien) || idNhanVien == SweetContext.Current.UserId;

            BindProjectList(true);
            DateTime now = DateTime.Now;
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            hdfScheduleJson.Value = GenerateScheduleJson(idNhanVien, startOfMonth, endOfMonth);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "RenderMiniCal", "renderEmployeeMonthCalendar();", true);
        }

        private void BindProjectList(bool isInitialLoad = false)
        {
            // Initial/reload only: search and status filtering are performed in the browser.
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<NhanVienProjectSummaryDTO> projects = ThanhVienDuAnManager.Instance.GetDanhSachDuAnCuaNhanVien(CurrentIdNhanVien, null, null);
            long queryMs=watch.ElapsedMilliseconds;
            ltrCountAllProj.Text=(projects==null?0:projects.Count).ToString();
            rptAllProjects.DataSource = projects;
            rptAllProjects.DataBind();
            emptyAll.Visible = projects==null || projects.Count==0;
            upListDuAn.Update();
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "NvdApplyFilterAfterReload", "nvdRestoreProjectFilter();", true);
            if(string.Equals(System.Configuration.ConfigurationManager.AppSettings["NvdPerfDiagnostics"],"true",StringComparison.OrdinalIgnoreCase))
            {
                string note=string.Format("projectBind queryAndMap={0}ms totalAfterBind={1}ms projects={2}",
                    queryMs,watch.ElapsedMilliseconds,projects==null?0:projects.Count);
                System.Diagnostics.Trace.WriteLine("[NVD PERF] "+note);
                this.Trace.Warn("NVD PERF",note);
            }
        }

        protected void btnReloadProjects_Click(object sender, EventArgs e)
        {
            BindProjectList();
            // LinkButton nam trong UpdatePanel; sau khi cap nhat du lieu, phai mo lai danh sach
            // vi DOM side panel co the vua duoc render lai. Khong F5, khong mo panel chi tiet.
            ScriptManager.RegisterStartupScript(upnlMainDetail, upnlMainDetail.GetType(),
                "NvdReopenProjectListAfterRefresh", "nvdRestoreProjectFilter();openSidePanel(true);", true);
        }

        protected void rptAllProjects_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ViewProject") return;

            Guid idDuAn;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out idDuAn))
            {
                ShowNotify("Dự án không hợp lệ.", MSGType.Error);
                return;
            }

            LoadProjectDetail(idDuAn);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "OpenProjectDetailPanel", "openProjectDetailPanel();", true);
        }

        private void LoadProjectDetail(Guid idDuAn)
        {
            if (idDuAn == Guid.Empty) return;
            CurrentDetailProjectId = idDuAn;
            NhanVienProjectDTO project = ThanhVienDuAnManager.Instance.GetChiTietDuAnCuaNhanVien(CurrentIdNhanVien, idDuAn);

            if (project == null)
            {
                ShowNotify("Không tìm thấy dự án.", MSGType.Warning);
                return;
            }

            // KBG 2.0: chỉ đọc báo cáo; không gọi HeSoDongGopManager và không sửa phiên giờ.
            ltrDetailTenDuAn.Text = HttpUtility.HtmlEncode(project.TenDuAn ?? string.Empty);
            ltrDetailMaDuAn.Text = HttpUtility.HtmlEncode(project.MaDuAn ?? string.Empty);
            ltrDetailVaiTro.Text = HttpUtility.HtmlEncode(project.VaiTro ?? string.Empty);
            ltrDetailTrangThai.Text = "<span class=\"project-status-tag " + GetProjectStatusCss(project.TrangThai) + "\">" + HttpUtility.HtmlEncode(GetDuAnStatusText(project.TrangThai)) + "</span>";
            ltrDetailProjectTime.Text = (project.ProjectActualEndDate.HasValue ? "Kết thúc thực tế: " : "Dự kiến: ") +
                FormatDateRange(project.ProjectStartDate,project.ProjectActualEndDate ?? project.ProjectEndDate);
            ltrDetailContribution.Text = RenderProjectHours(project);
            if(project.Tasks.Count==0)
                ltrDetailContribution.Text += "<div class=\"empty-detail mt-3\">Chưa có công việc được phân công hoặc phát sinh giờ công trong dự án này.</div>";

            // Repeater phang nam trong UpdatePanel; tra theo template de khong buoc sua designer.cs.
            Repeater taskRepeater = upProjectDetail.ContentTemplateContainer.FindControl("rptDetailTasks") as Repeater;
            if (taskRepeater == null)
                throw new InvalidOperationException("Khong tim thay rptDetailTasks trong upProjectDetail.");
            taskRepeater.DataSource = project.Tasks;
            taskRepeater.DataBind();
            upProjectDetail.Update();
        }

        private static string RenderProjectHours(NhanVienProjectDTO project)
        {
            Func<string,string,string,string,string> cell = (title,value,notes,css) =>
                "<div class=\"nvd-detail-metric "+css+"\"><div class=\"nvd-metric-name\">"+
                HttpUtility.HtmlEncode(title)+"</div><strong>"+HttpUtility.HtmlEncode(value)+
                "</strong><small>"+HttpUtility.HtmlEncode(notes)+"</small></div>";
            var html = new System.Text.StringBuilder();
            html.Append("<div class=\"nvd-detail-metrics\">");
            html.Append(cell("Giờ hệ thống ghi nhận",project.GioHeThongText,
                "Theo lịch "+project.GioHanhChinhText+" · Tăng ca "+project.GioTangCaText,""));
            html.Append(cell("Giờ của nhân viên được PM chốt",project.GioPmChotText,
                "Theo lịch "+project.GioPmThuongText+" · Tăng ca "+project.GioPmTangCaText,"nvd-detail-metric--final"));
            html.Append("</div><div class=\"nvd-detail-self-report\"><span>Nhân viên đã báo</span><strong>")
                .Append(HttpUtility.HtmlEncode(project.GioTuKhaiText)).Append("</strong><small>Theo lịch ")
                .Append(HttpUtility.HtmlEncode(project.GioTuKhaiThuongText)).Append(" · Tăng ca ")
                .Append(HttpUtility.HtmlEncode(project.GioTuKhaiTangCaText)).Append("</small></div>");
            int total = project.SoTaskLienQuan;
            int done = project.SoTaskDaChot;
            int pending = Math.Max(0,total-done);
            int percent = total <= 0 ? 0 : Math.Min(100,(int)Math.Round(100.0*done/total));
            html.Append("<div class=\"nvd-work-progress\"><div class=\"nvd-work-progress-head\"><strong>Tiến độ chốt giờ của nhân viên</strong><span>")
                .Append(done).Append("/").Append(total).Append(" công việc</span></div>")
                .Append("<div class=\"nvd-work-progress-track\"><span class=\"nvd-work-progress-fill\" style=\"width:")
                .Append(percent.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("%\"></span></div>")
                .Append("<div class=\"nvd-work-progress-note\">");
            if (total == 0) html.Append("Chưa có công việc liên quan phát sinh giờ công.");
            else if (pending > 0) html.Append("Còn ").Append(pending).Append(" công việc của nhân viên chưa được PM chốt. Có thể xem các dòng tương ứng ở bên dưới.");
            else html.Append("Tất cả công việc liên quan của nhân viên đã được chốt giờ.");
            html.Append("</div></div><div class=\"nvd-detail-status\"><span class=\"nvd-time-state ")
                .Append(project.TrangThaiGioCss).Append("\">")
                .Append(HttpUtility.HtmlEncode(project.TinhTrangGioText)).Append("</span>")
                .Append("<span>Toàn dự án: ").Append(HttpUtility.HtmlEncode(project.TienDoChotText)).Append("</span>");
            if(project.HienThiTyTrong)
                html.Append("<span class=\"nvd-share\">Tỷ trọng giờ công đã quyết toán: <strong>")
                    .Append(HttpUtility.HtmlEncode(project.TyTrongText)).Append("</strong></span>");
            html.Append("</div><details class=\"nvd-help-details\"><summary>Giải thích trạng thái và phạm vi giờ công</summary><div>")
                .Append(HttpUtility.HtmlEncode(project.GhiChuGioText)).Append("</div></details>");
            return html.ToString();
        }

        protected string GetDuAnStatusText(object statusValue)
        {
            byte code;
            if(statusValue==null || !byte.TryParse(Convert.ToString(statusValue),out code))return string.Empty;
            switch((DuAnStatus)code)
            {
                case DuAnStatus.ChuaBatDau:return "Chưa bắt đầu";
                case DuAnStatus.DangThucHien:return "Đang thực hiện";
                case DuAnStatus.TamDung:return "Tạm dừng";
                case DuAnStatus.HoanThanh:return "Hoàn thành";
                case DuAnStatus.KetThuc:return "Kết thúc";
                default:return ((DuAnStatus)code).ToString();
            }
        }

        private static string ReadOptionalColumn(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return string.Empty;
            return Convert.ToString(row[columnName]).Trim();
        }

        private static string EncodeOrEmptyText(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<span class=\"nvd-info-empty\">" + HttpUtility.HtmlEncode(fallback) + "</span>";
            return HttpUtility.HtmlEncode(value.Trim());
        }

        private string CalculateSeniority(DateTime joinDate)
        {
            DateTime now = DateTime.Now;
            if (joinDate > now) return GetResourceText(BackEndResourceKeys.NOT_STARTED_WORKING_YET);

            int years = now.Year - joinDate.Year;
            int months = now.Month - joinDate.Month;
            if (now.Day < joinDate.Day) months--;
            if (months < 0)
            {
                years--;
                months += 12;
            }

            string yearStr = GetResourceText(BackEndResourceKeys.YEAR).ToLower();
            string monthStr = GetResourceText(BackEndResourceKeys.MONTH).ToLower();
            string dayStr = GetResourceText(BackEndResourceKeys.DAY).ToLower();

            if (years > 0 && months > 0) return string.Format("{0} {1}, {2} {3}", years, yearStr, months, monthStr);
            if (years > 0) return string.Format("{0} {1}", years, yearStr);
            if (months > 0) return string.Format("{0} {1}", months, monthStr);

            int days = (now - joinDate).Days;
            return string.Format("{0} {1}", days, dayStr);
        }

        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            if (CurrentIdNhanVien != Guid.Empty)
            {
                CtrlNhanVienPopup1.Edit(CurrentIdNhanVien);
            }
        }

        protected string GetProjectUrl(object idDuAn)
        {
            if (idDuAn == null) return "#";
            Guid projectId;
            if (!Guid.TryParse(idDuAn.ToString(), out projectId)) return "#";
            return GetRelativeClientPath(RewriteURLHelper.ProjectDetail(projectId));
        }
        protected string GetTaskListUrl()
        {
            if (CurrentDetailProjectId == Guid.Empty) return "javascript:void(0);";
            return $"/fTasks/TaskList.aspx?ProjectId={CurrentDetailProjectId}";
        }
        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected string GetTaskPriorityBadge(object tenDoUuTien, object diemDoUuTien)
        {
            return _controlHelpers.GetTaskPriorityBadge(tenDoUuTien, diemDoUuTien);
        }

        protected string GetTaskStatusBadge(object status)
        {
            return _controlHelpers.GetTaskStatusBadge(status);
        }

        protected bool IsTaskCompletedLate(object status, object plannedEndDate, object actualEndDate)
        {
            if (status == null || status == DBNull.Value) return false;
            if (!int.TryParse(status.ToString(), out int taskStatus)) return false;

            // [FIX ĐỒNG BỘ VỚI CTRLTASK]: Ép trạng thái = 3 chính là Hoàn thành trễ hạn
            if (taskStatus == 3) return true;

            // Giữ lại logic cũ: Nếu trạng thái = 2 nhưng ngày thực tế > ngày dự kiến
            if (taskStatus == 2)
            {
                if (plannedEndDate != null && actualEndDate != null &&
                    DateTime.TryParse(plannedEndDate.ToString(), out DateTime plannedEnd) &&
                    DateTime.TryParse(actualEndDate.ToString(), out DateTime actualEnd))
                {
                    if (plannedEnd.Year > 1900 && actualEnd.Year > 1900)
                    {
                        return actualEnd.Date > plannedEnd.Date;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Badge trang thai cong viec: dung dung bang mau CtrlTask (0/1/4/2/3).
        /// Trang thai 3 su dung badge Hoan thanh mau xanh + ghi chu tre han mau do rieng.
        /// Khong su dung he so dong gop / tinh lai gio cong.
        /// </summary>
        protected string GetTaskStatusDisplay(object status, object plannedEndDate, object actualEndDate)
        {
            int code;
            if (!int.TryParse(Convert.ToString(status), out code)) code = -1;
            string css;
            string text;
            switch (code)
            {
                case 0: css = "badge-status-todo"; text = "Chưa bắt đầu"; break;
                case 1: css = "badge-status-doing"; text = "Đang thực hiện"; break;
                case 2: css = "badge-status-done"; text = "Hoàn thành"; break;
                case 3: css = "badge-status-done"; text = "Hoàn thành"; break;
                case 4: css = "badge-status-review"; text = "Chờ duyệt"; break;
                default: css = "badge-status-todo"; text = "Chưa xác định"; break;
            }
            string html = "<span class=\"nvd-task-status " + css + "\">" + HttpUtility.HtmlEncode(text) + "</span>";
            // Trường hợp dữ liệu có trạng thái 2 nhưng ngày thực tế > kế hoạch:
            // vẫn giữ cảnh báo trễ từ giao diện trước, không đổi trạng thái trong DB.
            if (code == 3 || (code == 2 && IsTaskCompletedLate(status, plannedEndDate, actualEndDate)))
            {
                int lateDays = 0;
                DateTime planned, actual;
                if (plannedEndDate != null && actualEndDate != null &&
                    plannedEndDate != DBNull.Value && actualEndDate != DBNull.Value &&
                    DateTime.TryParse(Convert.ToString(plannedEndDate), out planned) &&
                    DateTime.TryParse(Convert.ToString(actualEndDate), out actual) &&
                    planned.Year > 1900 && actual.Year > 1900)
                    lateDays = (actual.Date - planned.Date).Days;
                html += "<span class=\"nvd-task-late-note\">(Trễ hạn" +
                    (lateDays > 0 ? " · " + lateDays + " ngày" : "") + ")</span>";
            }
            return html;
        }
        // Tương thích cho những chỗ cũ còn gọi class trạng thái. UI V3 dùng GetTaskStatusDisplay.
        protected string GetTaskStatusWrapperClass(object status, object plannedEndDate, object actualEndDate)
        {
            int code;
            if (!int.TryParse(Convert.ToString(status), out code)) code = -1;
            switch (code)
            {
                case 1: return "nvd-task-status badge-status-doing";
                case 2:
                case 3: return "nvd-task-status badge-status-done";
                case 4: return "nvd-task-status badge-status-review";
                default: return "nvd-task-status badge-status-todo";
            }
        }

        protected bool HasTasks(object tasksObj)
        {
            if (tasksObj == null || tasksObj == DBNull.Value) return false;
            var list = tasksObj as System.Collections.IEnumerable;
            if (list != null)
            {
                var enumerator = list.GetEnumerator();
                return enumerator.MoveNext();
            }
            return false;
        }

        protected string FormatProjectPeriod(object start,object planned,object actual)
        {
            bool hasActual=actual!=null && actual!=DBNull.Value;
            return FormatDateRange(start,hasActual?actual:planned)+(hasActual?" · Thực tế":" · Dự kiến");
        }

        protected string FormatDateSafe(object dateObj, string defaultEmptyText = "")
        {
            if (dateObj == null || dateObj == DBNull.Value || string.IsNullOrWhiteSpace(dateObj.ToString())) return defaultEmptyText;
            DateTime dt;
            if (DateTime.TryParse(dateObj.ToString(), out dt))
            {
                if (dt.Year <= 1900) return defaultEmptyText;
                return dt.ToString("dd/MM/yyyy");
            }
            return defaultEmptyText;
        }

        protected string FormatDateRange(object startDateObj, object endDateObj, string defaultEndText = "")
        {
            string start = FormatDateSafe(startDateObj, "?");
            string end = FormatDateSafe(endDateObj, defaultEndText);
            if (start == "?" && end == defaultEndText) return string.Empty;
            return string.Format("{0} — {1}", start, end);
        }
        // Cung mot nguon lich voi LichCaNhan: task + cuoc hop cua nhan vien.
        // Chi tai data gon cho tooltip; khong mo modal hoac lay thong tin doi soat.
        private string GenerateScheduleJson(Guid userId, DateTime start, DateTime end)
        {
            var lich = LichTrinhManager.Instance.LayLichTrinhNhanVienCoCuocHop(userId, start, end);
            var dict = new Dictionary<string, object>();
            if (lich == null) return JsonConvert.SerializeObject(dict);

            foreach (var ngay in lich)
            {
                if (ngay == null) continue;
                int dow = (int)ngay.Ngay.DayOfWeek;
                string dayName = dow == 0 ? "CN" : dow == 6 ? "T7" : $"T{dow + 1}";
                var activeTasks = new List<object>();
                if (ngay.DanhSachCongViec != null)
                {
                    foreach (var t in ngay.DanhSachCongViec.Where(t => t != null))
                        activeTasks.Add(new { code = t.MaCongViec, name = t.TenCongViec, projectName = t.TenDuAn });
                }

                var meetings = new List<object>();
                if (ngay.DanhSachCuocHop != null)
                {
                    // Cung tap cuoc hop cua LichCaNhan, giu thu tu thoi gian trong ngay.
                    foreach (var meeting in ngay.DanhSachCuocHop.Where(m => m != null).OrderBy(m => m.ThoiGianBatDau))
                    {
                        DateTime from = meeting.ThoiGianBatDau > ngay.Ngay.Date
                            ? meeting.ThoiGianBatDau : ngay.Ngay.Date;
                        DateTime until = meeting.ThoiGianKetThuc < ngay.Ngay.Date.AddDays(1)
                            ? meeting.ThoiGianKetThuc : ngay.Ngay.Date.AddDays(1);
                        string toText = until == ngay.Ngay.Date.AddDays(1) ? "24:00" : until.ToString("HH:mm");
                        meetings.Add(new
                        {
                            time = from.ToString("HH:mm") + "–" + toText,
                            name = meeting.TenCuocHop,
                            projectName = meeting.TenDuAn,
                            isHost = meeting.LaChuTri
                        });
                    }
                }

                string text;
                string status = ngay.TrangThaiLich ?? "free";
                switch (status)
                {
                    case "holiday":
                        text = "🎉 " + GetResourceText(BackEndResourceKeys.HOLIDAY);
                        break;
                    case "weekend":
                        text = "⬜ " + GetResourceText(BackEndResourceKeys.WEEKEND);
                        break;
                    case "busy":
                        text = $"🔴 {activeTasks.Count} {GetResourceText(BackEndResourceKeys.TASK).ToLower()}";
                        break;
                    default:
                        // Ngay co hop khong duoc ghi 'Trong'.
                        text = meetings.Count > 0 ? "Có lịch họp" : "🟢 " + GetResourceText(BackEndResourceKeys.FREE);
                        break;
                }

                dict[ngay.Ngay.ToString("yyyy-MM-dd")] = new
                {
                    status = status,
                    dayName = dayName,
                    text = text,
                    tasks = activeTasks,
                    meetingCount = meetings.Count,
                    meetings = meetings
                };
            }
            return JsonConvert.SerializeObject(dict);
        }

    }
}
    