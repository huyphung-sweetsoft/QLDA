using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    /// <summary>Dot 8: popup PM duyet gio cong TOAN TASK. Khong thay CtrlFastReviewTask.</summary>
    public partial class CtrlDuyetGioCongTask : BaseAdminUserControl
    {
        public event EventHandler Saved;
        private Kbg20PmBanDuyet Ban;
        private readonly Dictionary<Guid, string> _avatarPaths = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, string> _chucDanh = new Dictionary<Guid, string>();
        private Guid _projectPm = Guid.Empty;
        // Khong dereference CURRENT_PAGE tai Init: BaseAdminUserControl/BaseAdminPage
        // co the chua khoi tao du thong tin quyen cho den Load.
        private bool CoQuyen
        {
            get
            {
                var page = Page as BaseAdminPage;
                return page != null && (page.IsPM || page.IsAdministrator);
            }
        }

        // Khong DataBind Repeater trong Page_Init: thong tin trang/ExtraModal
        // co the chua san sang o pha Init. Nut save se khoi tao du lieu tu DB
        // va doc cac o PM nhap truc tiep tu Request.Form theo UniqueID.
        // Nho vay khong phu thuoc __EVENTTARGET hay thu tu Init cua host page.

        private bool DuocXemDuAn(Guid project)
        {
            var page = Page as BaseAdminPage;
            return page != null && project != Guid.Empty && page.CurrentProjectId == project;
        }
        protected string HienGiay(object value)
        {
            if (value == null || value == DBNull.Value) return "0:00";
            long seconds = Convert.ToInt64(value);
            long phut = Math.Max(0, (long)Math.Round(seconds / 60.0, MidpointRounding.AwayFromZero));
            return (phut / 60).ToString(CultureInfo.InvariantCulture) + ":" + (phut % 60).ToString("00", CultureInfo.InvariantCulture);
        }
        protected string HienPhut(object value)
        {
            if (value == null || value == DBNull.Value) return "Chưa báo";
            return DinhDangPhut(Convert.ToInt32(value));
        }
        protected string HienPhutBao(object value,object daRutBao)
        {
            if (daRutBao != null && Convert.ToBoolean(daRutBao)) return "Đã rút báo";
            return HienPhut(value);
        }
        private static string DinhDangPhut(long minutes)
        { return (minutes / 60).ToString(CultureInfo.InvariantCulture) + ":" + (minutes % 60).ToString("00", CultureInfo.InvariantCulture); }
        protected string GiaTriSua(object deXuat, object daChot)
        {
            if (Ban != null && Ban.DaChotCong && daChot != null && daChot != DBNull.Value)
                return DinhDangPhut(Convert.ToInt32(daChot));
            return DinhDangPhut(Convert.ToInt32(deXuat));
        }
        private long LayGiaTriPmPhut(object deXuat, object daChot)
        {
            object chosen = Ban != null && Ban.DaChotCong && daChot != null && daChot != DBNull.Value
                ? daChot : deXuat;
            if (chosen == null || chosen == DBNull.Value) return 0;
            return Math.Max(0L, Convert.ToInt64(chosen, CultureInfo.InvariantCulture));
        }
        protected string GiaTriSuaGio(object deXuat, object daChot)
        {
            return (LayGiaTriPmPhut(deXuat, daChot) / 60L).ToString(CultureInfo.InvariantCulture);
        }
        protected string GiaTriSuaPhut(object deXuat, object daChot)
        {
            return (LayGiaTriPmPhut(deXuat, daChot) % 60L).ToString("00", CultureInfo.InvariantCulture);
        }
        // Lay avatar, chuc danh, co PM cung nguon voi CtrlChonNhanVienTask.
        // Anh fallback dung 5 mau + seed GUID, KHONG phu thuoc thu tu dong.
        private void TaiDanhTinhNhanVien(Guid project)
        {
            _avatarPaths.Clear(); _chucDanh.Clear(); _projectPm = Guid.Empty;
            try
            {
                var pm = DuAnManager.Instance.LayIdNhanVienQuanLy(project);
                if (pm.HasValue) _projectPm = pm.Value;
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("KBG20 PM meta: " + ex.Message); }
            try
            {
                foreach (var user in UserManager.Instance.GetAllActiveNhanVien())
                    if (user != null && user.UserId != Guid.Empty)
                        _avatarPaths[user.UserId] = user.Avatar ?? string.Empty;
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("KBG20 PM avatar: " + ex.Message); }
            try
            {
                var filters = new Dictionary<string, object> { { "LaNhanVien", true } };
                int totalRows;
                DataTable list = UserManager.Instance.SearchUsers("", filters, "DisplayName ASC", 0, 10000, out totalRows);
                if (list != null && list.Columns.Contains("UserId") && list.Columns.Contains("TenChucDanh"))
                    foreach (DataRow r in list.Rows)
                    {
                        Guid userId;
                        if (Guid.TryParse(Convert.ToString(r["UserId"]), out userId))
                            _chucDanh[userId] = r["TenChucDanh"] == DBNull.Value ? "" : Convert.ToString(r["TenChucDanh"]);
                    }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("KBG20 PM chuc danh: " + ex.Message); }
        }
        protected string HienChucDanh(object id)
        {
            Guid userId;
            string job;
            return Guid.TryParse(Convert.ToString(id), out userId) && _chucDanh.TryGetValue(userId, out job)
                ? HttpUtility.HtmlEncode(job ?? string.Empty) : string.Empty;
        }
        protected string HienCoPm(object id)
        {
            Guid userId;
            return Guid.TryParse(Convert.ToString(id), out userId) && userId == _projectPm
                ? "<span class='qlpm-person-pm' title='PM dự án'><i class='fas fa-crown'></i> PM</span>" : string.Empty;
        }
        protected string HienTinhTrangBao(object daBao)
        {
            return daBao != null && Convert.ToBoolean(daBao)
                ? "<span class='qlpm-person-state is-reported'>Đã báo hoàn thành</span>"
                : "<span class='qlpm-person-state'>Chưa báo hoàn thành</span>";
        }
        private static string ChuCaiDaiDien(string name)
        {
            var parts = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[parts.Length-2].Substring(0,1) + parts[parts.Length-1].Substring(0,1)).ToUpperInvariant();
        }
        protected string HienAvatar(object id, object name)
        {
            Guid userId;
            Guid.TryParse(Convert.ToString(id), out userId);
            string ten = Convert.ToString(name) ?? "";
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            int index = 0;
            foreach (byte part in userId.ToByteArray()) index = (index + part) % colors.Length;
            string title = HttpUtility.HtmlAttributeEncode(ten);
            string fallback = "<span class='qlpm-avatar-fallback' style='background-color:" + colors[index] + "' title='" + title + "'>"
                + HttpUtility.HtmlEncode(ChuCaiDaiDien(ten)) + "</span>";
            string path;
            if (!_avatarPaths.TryGetValue(userId, out path) || string.IsNullOrWhiteSpace(path)
                || path.IndexOf("no-file.png", StringComparison.OrdinalIgnoreCase) >= 0
                || path.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase))
                return fallback;
            if (path.StartsWith("~", StringComparison.Ordinal)) path = Page.ResolveUrl(path);
            // Khong lay noi dung do CSDL cung cap gan truc tiep vao JavaScript onerror.
            // Fallback mau chi duoc render neu avatar khong co.
            return fallback + "<img class='qlpm-avatar-image' src='" + HttpUtility.HtmlAttributeEncode(path)
                + "' alt='" + title + "' loading='lazy' onerror=\"this.style.display='none'\" />";
        }
        private void TaiDuLieu(Guid project, Guid task, bool capNhatGiaoDien)
        {
            Ban = new Kbg20PmChotCongTaskManager().LayBanDuyet(project, task);
            TaiDanhTinhNhanVien(project);
            repNhanVien.DataSource = Ban.NhanVien;
            repNhanVien.DataBind();
            if (!capNhatGiaoDien) return;
            hfProjectId.Value = project.ToString("D");
            hfTaskId.Value = task.ToString("D");
            hfSnapshot.Value = Ban.DauDuLieu;
            hfRequestId.Value = Guid.NewGuid().ToString("D");
            pnlShell.CssClass = "qlpm-shell" + (Ban.NhanVien.Count <= 1 ? " qlpm-one" : "");
            ltrTaskName.Text = HttpUtility.HtmlEncode("[" + Ban.MaCongViec + "] " + Ban.TenCongViec);
            // Dong nhat mau va class badge voi GetTaskStatusBadge() cua CtrlTask.
            switch (Ban.TrangThaiTask)
            {
                case 0: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-todo'>Chưa bắt đầu</span>"; break;
                case 1: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-doing'>Đang thực hiện</span>"; break;
                case 2: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-done'>Hoàn thành</span>"; break;
                case 3: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-done'>Hoàn thành</span><span class='qlpm-task-overdue'>(Trễ hạn)</span>"; break;
                case 4: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-review'>Chờ duyệt</span>"; break;
                default: ltrTaskStatus.Text = "<span class='badge-pill-custom badge-status-todo'>Chưa xác định</span>"; break;
            }
            ltrPmStatus.Text = Ban.DaChotCong
                ? "<span class='qlpm-status is-final'><i class='fas fa-check-double'></i> Đã duyệt giờ công</span>"
                : "<span class='qlpm-status'><i class='fas fa-exclamation-circle'></i> Chưa duyệt giờ công</span>";
            lblCount.Text = Ban.NhanVien.Count + " nhân viên";
            lblNhanVien.Text = Ban.NhanVien.Count.ToString(CultureInfo.InvariantCulture);
            lblTongHanhChinh.Text = DinhDangPhut(Ban.TongPhutHanhChinh);
            lblTongTangCa.Text = DinhDangPhut(Ban.TongPhutTangCa);
            bool daCoBanPm = Ban.NhanVien.Any(x => x.DaCoSuKienPmChot);
            bool duocChot = CoQuyen && (Ban.TrangThaiTask==2 || Ban.TrangThaiTask==3)
                && Ban.NhanVien.Count > 0 && !Ban.CoPhienDangMo && !daCoBanPm
                && Ban.SoNhanVienChuaXacNhan==0;
            btnChotGioCong.Visible = CoQuyen;
            btnChotGioCong.Enabled = duocChot;
            btnChotGioCong.ToolTip = duocChot ? "Đủ điều kiện chốt giờ công" :
                Ban.SoNhanVienChuaXacNhan > 0 ? "Vẫn còn nhân viên chưa báo hoàn thành phần việc."
                : Ban.CoPhienDangMo ? "Vẫn còn phiên giờ đang mở trên Task."
                : Ban.DaChotCong || daCoBanPm ? "Giờ công đã được chốt, không thể chốt lại."
                : Ban.NhanVien.Count == 0 ? "Không có nhân viên hoặc lịch sử giờ cần chốt."
                : "Chỉ chốt khi Task đã hoàn thành (2/3) và dữ liệu hợp lệ.";
            btnNhacBaoHoanThanh.Visible = CoQuyen && Ban.SoNhanVienChuaXacNhan>0 && !Ban.DaChotCong;
            pnlAlert.Visible = !duocChot;
            lblAlert.Text = Ban.DaChotCong ? "Giờ công đã được PM duyệt. Chế độ xem, không ghi đè kết quả đã lưu."
                : Ban.CoPhienDangMo ? "Vẫn còn phiên giờ đang mở trên công việc này. Không thể duyệt giờ công."
                : Ban.SoNhanVienChuaXacNhan>0 ? ("Còn " + Ban.SoNhanVienChuaXacNhan + " nhân viên chưa báo hoàn thành phần việc: " +
                    string.Join(", ", Ban.NhanVien.Where(x=>!x.DaXacNhanGio).Select(x=>x.TenNhanVien)) +
                    ". Nhấn Gửi nhắc nhở để nhân viên mở trực tiếp biểu mẫu Báo hoàn thành phần việc.")
                : daCoBanPm ? "Đã phát hiện bản chốt PM trước đó. Không cho lưu một phần hoặc chốt lại."
                : Ban.NhanVien.Count == 0 ? "Chưa có nhân viên nào thuộc phạm vi chốt công của công việc."
                : "Chỉ PM/Quản trị viên mới được duyệt giờ công khi công việc đã hoàn thành.";
            lblFooter.Text = Ban.DaChotCong ? "Đã lưu kết quả chính thức" : "Chốt toàn bộ " + Ban.NhanVien.Count + " nhân viên";
            foreach (RepeaterItem item in repNhanVien.Items)
            {
                var hc = item.FindControl("txtHanhChinh") as TextBox;
                var tc = item.FindControl("txtTangCa") as TextBox;
                var hcPhut = item.FindControl("txtHanhChinhPhut") as TextBox;
                var tcPhut = item.FindControl("txtTangCaPhut") as TextBox;
                var lydo = item.FindControl("txtLyDo") as TextBox;
                if (hc != null) hc.ReadOnly = !duocChot;
                if (tc != null) tc.ReadOnly = !duocChot;
                if (hcPhut != null) hcPhut.ReadOnly = !duocChot;
                if (tcPhut != null) tcPhut.ReadOnly = !duocChot;
                if (lydo != null) lydo.ReadOnly = !duocChot;
            }
            upDuyetGioCong.Update();
            upFooterDuyetCong.Update();
        }
        public void OpenModal(Guid project, Guid task)
        {
            if (!CoQuyen) { ShowAccessDeniedNotify(); return; }
            if (!DuocXemDuAn(project)) { ShowAccessDeniedNotify(); return; }
            try
            {
                TaiDuLieu(project, task, true);
                mdlDuyetGioCong.OpenModal(true);
            }
            catch (Exception ex) { ShowNotify(ex.Message, MSGType.Error); }
        }
        protected void btnLamMoiDuyet_Click(object sender, EventArgs e)
        {
            if (!CoQuyen) { ShowAccessDeniedNotify(); return; }
            Guid project,task;
            if (!Guid.TryParse(hfProjectId.Value,out project) || !Guid.TryParse(hfTaskId.Value,out task)
                || !DuocXemDuAn(project))
            { ShowNotify("Thông tin popup không hợp lệ.",MSGType.Warning); return; }
            try { TaiDuLieu(project,task,true); mdlDuyetGioCong.OpenModal(true); }
            catch(Exception ex) { ShowNotify(ex.Message,MSGType.Error); }
        }
        protected void btnNhacBaoHoanThanh_Click(object sender, EventArgs e)
        {
            if (!CoQuyen) { ShowAccessDeniedNotify(); return; }
            Guid project, task;
            if (!Guid.TryParse(hfProjectId.Value,out project) || !Guid.TryParse(hfTaskId.Value,out task)
                || !DuocXemDuAn(project))
            { ShowNotify("Thông tin dự án/công việc không hợp lệ.", MSGType.Warning); return; }
            try
            {
                var ketQua = Kbg20TichHopTask.GuiNhacBaoHoanThanhThuCong(project, task);
                TaiDuLieu(project, task, true);
                if (ketQua.CanNhac == 0)
                    ShowNotify("Không còn nhân viên nào cần nhắc báo hoàn thành phần việc.", MSGType.Warning);
                else if (ketQua.ThatBai > 0)
                    ShowNotify("Đã gửi " + ketQua.DaGui + "/" + ketQua.CanNhac +
                        " thông báo; " + ketQua.ThatBai + " thông báo lỗi. Kiểm tra log máy chủ.", MSGType.Warning);
                else if (ketQua.DaGui > 0)
                    ShowNotify("Đã gửi ngay " + ketQua.DaGui + " thông báo nhắc báo hoàn thành phần việc.", MSGType.Success);
                else
                    ShowNotify("Không gửi được thông báo nào. Vui lòng kiểm tra log máy chủ.", MSGType.Warning);
            }
            catch(Exception ex) { ShowNotify(ex.Message,MSGType.Error); }
        }
        // PM gui 2 truong rieng, backend KHONG parse "HH:mm" nua.
        private static int DocPhut(string gioText, string phutText)
        {
            int gio, phut;
            if (!int.TryParse((gioText ?? "").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out gio) ||
                !int.TryParse((phutText ?? "").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out phut) ||
                gio < 0 || gio > 9999 || phut < 0 || phut > 59)
                throw new ArgumentException("Vui lòng nhập số nguyên ở 2 ô Giờ và Phút. Phút phải từ 00 đến 59; Giờ không quá 9.999.");
            long result = (long)gio * 60L + phut;
            if (result > 599999L) throw new ArgumentException("Số giờ công vượt giới hạn cho phép.");
            return (int)result;
        }
        // Repeater duoc bind trong Click (sau LoadPostData). Vi vay KHONG doc
        // TextBox.Text: no se la gia tri DB, khong phai gia tri PM vua nhap.
        // Doc truc tiep form key cua TextBox (UniqueID) do browser gui den.
        private string DocTruongPmTuForm(TextBox control, string tenTruong)
        {
            if (control == null)
                throw new InvalidOperationException("Thiếu ô nhập " + tenTruong + " trong biểu mẫu PM.");
            string value = Request.Form[control.UniqueID];
            if (value == null)
                throw new InvalidOperationException("Thiếu dữ liệu " + tenTruong + " trong yêu cầu. Vui lòng tải lại popup.");
            return value;
        }

        protected void btnChotGioCong_Click(object sender, EventArgs e)
        {
            if (!CoQuyen) { ShowAccessDeniedNotify(); return; }
            Guid project, task, request;
            if (!Guid.TryParse(hfProjectId.Value, out project) ||
                !Guid.TryParse(hfTaskId.Value, out task) ||
                !Guid.TryParse(hfRequestId.Value, out request) || !DuocXemDuAn(project))
            {
                ShowNotify("Thiếu thông tin công việc hoặc dự án không hợp lệ.", MSGType.Error);
                return;
            }
            try
            {
                // Load moi du lieu tu DB CHI SAU KHI da xac nhan quyen va du an.
                // Không ghi de hfSnapshot/hfRequestId cua popup dang mo.
                // DataBind tai Click KHONG lam mat form: moi gia tri doc bang Request.Form.
                TaiDuLieu(project, task, false);
                if (Ban == null || Ban.IdDuAn != project || Ban.IdCongViec != task || Ban.NhanVien == null)
                    throw new InvalidOperationException("Không tìm thấy dữ liệu đối soát của công việc này.");
                if (repNhanVien == null || repNhanVien.Items.Count == 0 ||
                    repNhanVien.Items.Count != Ban.NhanVien.Count)
                    throw new InvalidOperationException("Danh sách nhân viên của công việc đã thay đổi. Vui lòng mở lại popup.");

                var rows = new List<Kbg20PmDongDuyet>();
                var officialRows = Ban.NhanVien.ToList();
                var seen = new HashSet<Guid>();
                int index = 0;
                foreach (RepeaterItem item in repNhanVien.Items)
                {
                    var hf = item.FindControl("hfNhanVien") as HiddenField;
                    var hc = item.FindControl("txtHanhChinh") as TextBox;
                    var tc = item.FindControl("txtTangCa") as TextBox;
                    var hcPhut = item.FindControl("txtHanhChinhPhut") as TextBox;
                    var tcPhut = item.FindControl("txtTangCaPhut") as TextBox;
                    var lydo = item.FindControl("txtLyDo") as TextBox;
                    Guid expected = Guid.Empty, posted = Guid.Empty;
                    string formPerson = hf == null ? null : Request.Form[hf.UniqueID];
                    if (hf == null || !Guid.TryParse(hf.Value, out expected) ||
                        !Guid.TryParse(formPerson, out posted) || expected != posted ||
                        !seen.Add(posted) || posted != officialRows[index].IdNhanVien)
                    {
                        System.Diagnostics.Trace.TraceWarning("KBG20_PM_FORM: nhan vien khong khop o dong " + index);
                        throw new InvalidOperationException("Danh sách hoặc thứ tự nhân viên đã thay đổi. Vui lòng mở lại popup để tránh ghi nhầm giờ công.");
                    }

                    rows.Add(new Kbg20PmDongDuyet
                    {
                        IdNhanVien = posted,
                        PhutHanhChinh = DocPhut(DocTruongPmTuForm(hc, "Giờ hành chính"),
                                               DocTruongPmTuForm(hcPhut, "Phút hành chính")),
                        PhutTangCa = DocPhut(DocTruongPmTuForm(tc, "Giờ tăng ca"),
                                             DocTruongPmTuForm(tcPhut, "Phút tăng ca")),
                        LyDoDieuChinh = DocTruongPmTuForm(lydo, "Ghi chú điều chỉnh")
                    });
                    index++;
                }
                if (rows.Count != Ban.NhanVien.Count)
                    throw new InvalidOperationException("Biểu mẫu chưa có đủ thông tin nhân viên cần chốt.");

                Guid user = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
                var page = Page as BaseAdminPage;
                bool isAdministrator = page != null && page.IsAdministrator;
                var saved = new Kbg20PmChotCongTaskManager().ChotCongTapThe(
                    project, task, user, isAdministrator,
                    request, hfSnapshot.Value, rows);
                if (saved == null || !saved.DaChotCong)
                    throw new InvalidOperationException("Chưa xác nhận được trạng thái đã chốt giờ công.");

                // KBG20 Core da commit thanh cong: UI refresh hong duoc bao sai ket qua.
                mdlDuyetGioCong.CloseModal();
                hfTaskId.Value = string.Empty;
                hfProjectId.Value = string.Empty;
                ShowNotify("Đã duyệt giờ công cho toàn bộ " + saved.NhanVien.Count + " nhân viên.", MSGType.Success);
                var handler = Saved;
                if (handler != null)
                {
                    try { handler(this, EventArgs.Empty); }
                    catch (Exception refreshEx)
                    {
                        System.Diagnostics.Trace.TraceError("KBG20_PM_REFRESH_AFTER_COMMIT: " + refreshEx);
                    }
                }
            }
            catch (System.Data.DBConcurrencyException ex)
            {
                System.Diagnostics.Trace.TraceWarning("KBG20_PM_CONFLICT: " + ex);
                ShowNotify(ex.Message, MSGType.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("KBG20_PM_SAVE: " + ex);
                ShowNotify("Không thể duyệt giờ công: " + ex.Message, MSGType.Error);
            }
        }

    }
}
