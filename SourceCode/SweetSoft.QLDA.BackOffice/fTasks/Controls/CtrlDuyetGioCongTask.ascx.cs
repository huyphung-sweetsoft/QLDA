using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
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
        private bool CoQuyenDuAn(Guid project)
        {
            var page = Page as BaseAdminPage;
            if (page == null || project == Guid.Empty || !DuocXemDuAn(project)) return false;
            if (page.IsAdministrator) return true;
            if (SweetContext.Current == null || SweetContext.Current.UserId == Guid.Empty) return false;
            Guid? idPm = DuAnManager.Instance.LayIdNhanVienQuanLy(project);
            return idPm.HasValue && idPm.Value == SweetContext.Current.UserId;
        }
        private bool CoQuyen
        {
            get
            {
                var page = Page as BaseAdminPage;
                return page != null && CoQuyenDuAn(page.CurrentProjectId);
            }
        }

        // Khong DataBind Repeater trong Page_Init: thong tin trang/ExtraModal
        // co the chua san sang o pha Init. Nut save se khoi tao du lieu tu DB
        // va doc cac o PM nhap truc tiep tu Request.Form theo UniqueID.
        // Nho vay khong phu thuoc __EVENTTARGET hay thu tu Init cua host page.

        private bool DangDieuChinh { get { return ViewState["Kbg20PmDangDieuChinh"] != null && (bool)ViewState["Kbg20PmDangDieuChinh"]; } }
        private static T TimCon<T>(Control root,string id) where T:Control
        {
            if(root==null)return null;
            if(root.ID==id)return root as T;
            foreach(Control c in root.Controls){var found=TimCon<T>(c,id);if(found!=null)return found;}
            return null;
        }
        private LinkButton NutBatDauSua { get { return TimCon<LinkButton>(upFooterDuyetCong,"btnBatDauDieuChinh"); } }
        private LinkButton NutLuuSua { get { return TimCon<LinkButton>(upFooterDuyetCong,"btnLuuDieuChinh"); } }
        private bool LaQuanTri {get {var page=Page as BaseAdminPage;return page!=null && page.IsAdministrator;} }
        private Guid NguoiHienTai {get {return SweetContext.Current==null?Guid.Empty:SweetContext.Current.UserId;} }

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
            bool suaSauChot = DangDieuChinh && Ban.DaChotCong;
            bool daCoBanPm = Ban.NhanVien.Any(x => x.DaCoSuKienPmChot);
            bool duocChot = CoQuyen && (Ban.TrangThaiTask==2 || Ban.TrangThaiTask==3)
                && Ban.NhanVien.Count > 0 && !Ban.CoPhienDangMo && !daCoBanPm
                && Ban.SoNhanVienChuaXacNhan==0;
            if(NutBatDauSua!=null)NutBatDauSua.Visible=CoQuyen && Ban.DaChotCong && !suaSauChot;
            if(NutLuuSua!=null)NutLuuSua.Visible=CoQuyen && suaSauChot;
            btnChotGioCong.Visible = CoQuyen && !Ban.DaChotCong;
            btnChotGioCong.Enabled = duocChot;
            btnChotGioCong.ToolTip = duocChot ? "Đủ điều kiện chốt giờ công" :
                Ban.SoNhanVienChuaXacNhan > 0 ? "Vẫn còn nhân viên chưa báo hoàn thành phần việc."
                : Ban.CoPhienDangMo ? "Vẫn còn phiên giờ đang mở trên Task."
                : Ban.DaChotCong || daCoBanPm ? "Giờ công đã được chốt, không thể chốt lại."
                : Ban.NhanVien.Count == 0 ? "Không có nhân viên hoặc lịch sử giờ cần chốt."
                : "Chỉ chốt khi Task đã hoàn thành (2/3) và dữ liệu hợp lệ.";
            // PM/admin luon thay nut nhac nho; chi co the bam khi con nguoi chua bao va task chua duoc chot.
            btnNhacBaoHoanThanh.Visible = CoQuyen;
            btnNhacBaoHoanThanh.Enabled = CoQuyen && Ban.SoNhanVienChuaXacNhan > 0 && !Ban.DaChotCong;
            btnNhacBaoHoanThanh.ToolTip = !CoQuyen ? "Không có quyền gửi nhắc nhở."
                : Ban.DaChotCong ? "Giờ công đã được duyệt; không cần gửi nhắc nhở."
                : Ban.SoNhanVienChuaXacNhan == 0 ? "Tất cả nhân viên đã báo hoàn thành phần việc."
                : "Gửi nhắc nhở cho " + Ban.SoNhanVienChuaXacNhan + " nhân viên chưa báo hoàn thành.";
            pnlAlert.Visible = !duocChot;
            lblAlert.Text = Ban.DaChotCong ? "Giờ công đã được PM duyệt. Chế độ xem, không ghi đè kết quả đã lưu."
                : Ban.CoPhienDangMo ? "Vẫn còn phiên giờ đang mở trên công việc này. Không thể duyệt giờ công."
                : Ban.SoNhanVienChuaXacNhan>0 ? ("Còn " + Ban.SoNhanVienChuaXacNhan + " nhân viên chưa báo hoàn thành phần việc: " +
                    string.Join(", ", Ban.NhanVien.Where(x=>!x.DaXacNhanGio).Select(x=>x.TenNhanVien)) +
                    ". Nhấn Gửi nhắc nhở để nhân viên mở trực tiếp biểu mẫu Báo hoàn thành phần việc.")
                : daCoBanPm ? "Đã phát hiện bản chốt PM trước đó. Không cho lưu một phần hoặc chốt lại."
                : Ban.NhanVien.Count == 0 ? "Chưa có nhân viên nào thuộc phạm vi chốt công của công việc."
                : "Chỉ PM/Quản trị viên mới được duyệt giờ công khi công việc đã hoàn thành.";
            lblFooter.Text = suaSauChot ? "Chỉ điều chỉnh một nhân viên mỗi lần, bắt buộc nhập lý do" :
                Ban.DaChotCong ? "Đã lưu kết quả chính thức" : "Chốt toàn bộ " + Ban.NhanVien.Count + " nhân viên";
            foreach (RepeaterItem item in repNhanVien.Items)
            {
                var hc = item.FindControl("txtHanhChinh") as TextBox;
                var tc = item.FindControl("txtTangCa") as TextBox;
                var hcPhut = item.FindControl("txtHanhChinhPhut") as TextBox;
                var tcPhut = item.FindControl("txtTangCaPhut") as TextBox;
                var lydo = item.FindControl("txtLyDo") as TextBox;
                if (hc != null) hc.ReadOnly = !(duocChot || suaSauChot);
                if (tc != null) tc.ReadOnly = !(duocChot || suaSauChot);
                if (hcPhut != null) hcPhut.ReadOnly = !(duocChot || suaSauChot);
                if (tcPhut != null) tcPhut.ReadOnly = !(duocChot || suaSauChot);
                if (lydo != null)
                {
                    lydo.ReadOnly = !(duocChot || suaSauChot);
                    // Khi dieu chinh sau chot, buoc nguoi dung nhap LY DO MOI, khong ke thua ghi chu chot cu.
                    if (suaSauChot) lydo.Text = string.Empty;
                }
            }
            upDuyetGioCong.Update();
            upFooterDuyetCong.Update();
        }
        public void OpenModal(Guid project, Guid task)
        {
            if (!CoQuyenDuAn(project)) { ShowAccessDeniedNotify(); return; }
            if (!DuocXemDuAn(project)) { ShowAccessDeniedNotify(); return; }
            try
            {
                ViewState["Kbg20PmDangDieuChinh"]=false;
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
        protected void btnBatDauDieuChinh_Click(object sender,EventArgs e)
        {
            if(!CoQuyen){ShowAccessDeniedNotify();return;}
            Guid project,task;
            if(!Guid.TryParse(hfProjectId.Value,out project) || !Guid.TryParse(hfTaskId.Value,out task) || !DuocXemDuAn(project))
            {ShowNotify("Dự án hoặc công việc không hợp lệ.",MSGType.Warning);return;}
            try
            {
                var ban=new Kbg20PmChotCongTaskManager().LayBanDuyet(project,task);
                if(!ban.DaChotCong)throw new InvalidOperationException("Chưa có kết quả giờ công chính thức để điều chỉnh.");
                ViewState["Kbg20PmDangDieuChinh"]=true;
                TaiDuLieu(project,task,true);
                mdlDuyetGioCong.OpenModal(true);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}
        }
        private const string XacNhanDieuChinhPm = "KBG20_PM_CONFIRM_ADJUST";

        private void XoaYeuCauDieuChinhChoXacNhan()
        {
            foreach (string key in new[] { "Project", "Task", "Request", "Snapshot", "Employee",
                "Regular", "Overtime", "Reason", "CreatedAt" })
                ViewState.Remove("Kbg20PmPending_" + key);
        }

        protected void btnLuuDieuChinh_Click(object sender, EventArgs e)
        {
            if (!CoQuyen) { ShowAccessDeniedNotify(); return; }
            Guid project, task, request;
            if (!DangDieuChinh || !Guid.TryParse(hfProjectId.Value, out project) ||
                !Guid.TryParse(hfTaskId.Value, out task) || !Guid.TryParse(hfRequestId.Value, out request) ||
                !DuocXemDuAn(project))
            {
                ShowNotify("Phiên điều chỉnh đã hết hiệu lực, vui lòng mở lại.", MSGType.Warning);
                return;
            }
            try
            {
                TaiDuLieu(project, task, false);
                if (Ban == null || !Ban.DaChotCong || repNhanVien.Items.Count != Ban.NhanVien.Count)
                    throw new InvalidOperationException("Kết quả đã chốt không còn phù hợp. Vui lòng tải lại.");
                Kbg20PmDongDuyet changed = null;
                string tenNhanVien = string.Empty;
                int index = 0;
                foreach (RepeaterItem item in repNhanVien.Items)
                {
                    var hf = item.FindControl("hfNhanVien") as HiddenField;
                    Guid posted, expected = Ban.NhanVien[index].IdNhanVien;
                    if (hf == null || !Guid.TryParse(Request.Form[hf.UniqueID], out posted) || posted != expected)
                        throw new DBConcurrencyException("Danh sách nhân viên thay đổi. Vui lòng mở lại.");
                    var h = item.FindControl("txtHanhChinh") as TextBox;
                    var hp = item.FindControl("txtHanhChinhPhut") as TextBox;
                    var t = item.FindControl("txtTangCa") as TextBox;
                    var tp = item.FindControl("txtTangCaPhut") as TextBox;
                    var reason = item.FindControl("txtLyDo") as TextBox;
                    int a = DocPhut(DocTruongPmTuForm(h, "Giờ theo lịch"), DocTruongPmTuForm(hp, "Phút theo lịch"));
                    int b = DocPhut(DocTruongPmTuForm(t, "Giờ tăng ca"), DocTruongPmTuForm(tp, "Phút tăng ca"));
                    var original = Ban.NhanVien[index];
                    if (a != original.PhutHanhChinhPmDaChot || b != original.PhutTangCaPmDaChot)
                    {
                        if (changed != null)
                            throw new InvalidOperationException("Mỗi lần chỉ được điều chỉnh kết quả của một nhân viên.");
                        changed = new Kbg20PmDongDuyet
                        {
                            IdNhanVien = posted, PhutHanhChinh = a, PhutTangCa = b,
                            LyDoDieuChinh = DocTruongPmTuForm(reason, "Lý do điều chỉnh")
                        };
                        tenNhanVien = original.TenNhanVien;
                    }
                    index++;
                }
                if (changed == null)
                    throw new InvalidOperationException("Bạn chưa thay đổi số giờ công của nhân viên nào.");
                if (string.IsNullOrWhiteSpace(changed.LyDoDieuChinh))
                    throw new InvalidOperationException("Bạn cần ghi lý do khi điều chỉnh giờ công đã chốt.");
                if (!string.Equals(Ban.DauDuLieu, hfSnapshot.Value, StringComparison.Ordinal))
                    throw new DBConcurrencyException("Kết quả chốt đã thay đổi. Vui lòng tải lại trước khi điều chỉnh.");

                // Chua ghi SQL. Chot form da validate vao ViewState; tren postback
                // cua MessageBox Request.Form khong con nhung truong nhap gio PM.
                XoaYeuCauDieuChinhChoXacNhan();
                ViewState["Kbg20PmPending_Project"] = project.ToString("D");
                ViewState["Kbg20PmPending_Task"] = task.ToString("D");
                ViewState["Kbg20PmPending_Request"] = request.ToString("D");
                ViewState["Kbg20PmPending_Snapshot"] = hfSnapshot.Value;
                ViewState["Kbg20PmPending_Employee"] = changed.IdNhanVien.ToString("D");
                ViewState["Kbg20PmPending_Regular"] = changed.PhutHanhChinh;
                ViewState["Kbg20PmPending_Overtime"] = changed.PhutTangCa;
                ViewState["Kbg20PmPending_Reason"] = changed.LyDoDieuChinh;
                ViewState["Kbg20PmPending_CreatedAt"] = DateTime.UtcNow;

                var result = new ConfirmResult { CommandName = XacNhanDieuChinhPm };
                CURRENT_PAGE.CurrentConfirmResult = result;
                string message = "Xác nhận lưu giờ công điều chỉnh cho <strong>" +
                    HttpUtility.HtmlEncode(tenNhanVien) + "</strong>?<br/>" +
                    "Giờ theo lịch: <strong>" + DinhDangPhut(changed.PhutHanhChinh) +
                    "</strong>; giờ tăng ca: <strong>" + DinhDangPhut(changed.PhutTangCa) +
                    "</strong>.<br/>Bản chốt trước đó vẫn được giữ trong lịch sử.";
                OpenMessageBox(new MessageBox("Xác nhận điều chỉnh giờ công", message,
                    MSGButton.AcceptCancel, MSGIcon.Warning), result, false, false);
            }
            catch (Exception ex) { XoaYeuCauDieuChinhChoXacNhan(); ShowNotify(ex.Message, MSGType.Error); }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e == null || !string.Equals((e.CommandName ?? "").Trim(),
                XacNhanDieuChinhPm, StringComparison.Ordinal)) return;
            if (!e.Submit) { XoaYeuCauDieuChinhChoXacNhan(); return; }
            if (!CoQuyen) { XoaYeuCauDieuChinhChoXacNhan(); ShowAccessDeniedNotify(); return; }
            // Khoi tao truoc vi toan tu && co the khong chay cac out-param.
            Guid project = Guid.Empty, task = Guid.Empty, request = Guid.Empty, employee = Guid.Empty;
            bool valid = DangDieuChinh &&
                Guid.TryParse(Convert.ToString(ViewState["Kbg20PmPending_Project"]), out project) &&
                Guid.TryParse(Convert.ToString(ViewState["Kbg20PmPending_Task"]), out task) &&
                Guid.TryParse(Convert.ToString(ViewState["Kbg20PmPending_Request"]), out request) &&
                Guid.TryParse(Convert.ToString(ViewState["Kbg20PmPending_Employee"]), out employee) &&
                ViewState["Kbg20PmPending_Regular"] is int &&
                ViewState["Kbg20PmPending_Overtime"] is int &&
                ViewState["Kbg20PmPending_CreatedAt"] is DateTime &&
                (DateTime.UtcNow - (DateTime)ViewState["Kbg20PmPending_CreatedAt"]).TotalMinutes <= 5 &&
                DuocXemDuAn(project);
            if (!valid)
            {
                XoaYeuCauDieuChinhChoXacNhan();
                ShowNotify("Xác nhận đã hết hiệu lực. Vui lòng mở lại popup điều chỉnh.", MSGType.Warning);
                return;
            }
            var changed = new Kbg20PmDongDuyet
            {
                IdNhanVien = employee,
                PhutHanhChinh = (int)ViewState["Kbg20PmPending_Regular"],
                PhutTangCa = (int)ViewState["Kbg20PmPending_Overtime"],
                LyDoDieuChinh = Convert.ToString(ViewState["Kbg20PmPending_Reason"])
            };
            string snapshot = Convert.ToString(ViewState["Kbg20PmPending_Snapshot"]);
            XoaYeuCauDieuChinhChoXacNhan(); // Một xác nhận chỉ được thử lưu một lần.
            try
            {
                new Kbg20PmChotCongTaskManager().DieuChinhGioDaChot(project, task,
                    NguoiHienTai, LaQuanTri, request, snapshot, changed);
                ViewState["Kbg20PmDangDieuChinh"] = false;
                TaiDuLieu(project, task, true);
                Saved?.Invoke(this, EventArgs.Empty);
                ShowNotify("Đã điều chỉnh giờ công và giữ lịch sử chốt cũ. Hãy kiểm tra thông báo của nhân viên.", MSGType.Success);
            }
            catch (Exception ex) { ShowNotify(ex.Message, MSGType.Error); }
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
