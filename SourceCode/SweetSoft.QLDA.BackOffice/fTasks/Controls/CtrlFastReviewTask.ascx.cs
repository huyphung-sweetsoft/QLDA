using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.EnumHelper;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlFastReviewTask : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;
        // Chi duoc gan boi host KhaiBaoGio 2.0; CtrlTask goc van chay y nhu cu.
        public Action<Guid, Action> LuuDuyetQuaKbg20 { get; set; }

        /// <summary>
        /// Quyền PM được truyền từ CtrlTask theo quyền thực tế của trang.
        /// </summary>
        public bool IsPM { get; set; }

        private Guid ProjectId
        {
            get
            {
                object value = ViewState["ProjectId"];
                return value is Guid id ? id : Guid.Empty;
            }
        }

        public void OpenModal(Guid projectId, Guid taskId, string taskCode, string taskName)
        {
            // Giữ nguyên luồng mở popup từ nút REVIEW_COMPLETE.
            OpenModalInternal(projectId, taskId, taskCode, taskName, false);
        }

        /// <summary>
        /// Mở popup khi điều hướng từ thông báo.
        /// Dùng trực tiếp ExtraModal giống luồng mở từ nút trong danh sách,
        /// tránh lớp overlay chờ CMSMasterJs có thể che toàn bộ trang.
        /// </summary>
        public void OpenModalWhenDialogReady(
            Guid projectId,
            Guid taskId,
            string taskCode,
            string taskName)
        {
            // Chỉ luồng mở từ thông báo mới chờ CMSMasterJs.OpenDialog.
            OpenModalInternal(projectId, taskId, taskCode, taskName, true);
        }

        private void OpenModalInternal(
            Guid projectId,
            Guid taskId,
            string taskCode,
            string taskName,
            bool waitForDialogReady)
        {
            if (!IsPM)
            {
                ShowAccessDeniedNotify();
                return;
            }

            TblCongViec task = TblCongViec.FetchByID(taskId);
            if (task == null)
            {
                ShowNotify("Không tìm thấy công việc cần duyệt!", MSGType.Warning);
                return;
            }

            if (task.TrangThai != 4)
            {
                ShowNotify("Công việc không còn ở trạng thái Chờ duyệt!", MSGType.Warning);
                return;
            }

            if (!task.NgayKetThuc.HasValue || !task.NgayHoanThanhThucTe.HasValue)
            {
                ShowNotify("Công việc thiếu ngày kết thúc dự kiến hoặc ngày báo hoàn thành!", MSGType.Warning);
                return;
            }

            ViewState["ProjectId"] = projectId;
            hdfReviewTaskId.Value = task.IdCongViec.ToString();
            txtRejectReason.Text = string.Empty;

            ltrReviewTaskName.Text = HttpUtility.HtmlEncode(
                "[" + (taskCode ?? string.Empty) + "] " + (taskName ?? string.Empty));
            ltrActualDate.Text = task.NgayHoanThanhThucTe.Value.ToString("dd/MM/yyyy");
            ltrExpectedEndDate.Text = task.NgayKetThuc.Value.ToString("dd/MM/yyyy");
            ltrLateReason.Text = string.IsNullOrWhiteSpace(task.LyDoTre)
                ? "Không có"
                : HttpUtility.HtmlEncode(task.LyDoTre);

            bool isLate = task.NgayHoanThanhThucTe.Value.Date > task.NgayKetThuc.Value.Date;
            ltrReviewResult.Text = isLate
                ? "<span class=\"fr-badge fr-badge-late\"><i class=\"fas fa-exclamation-triangle me-1\"></i>Hoàn thành trễ hạn</span>"
                : "<span class=\"fr-badge fr-badge-ontime\"><i class=\"fas fa-check-circle me-1\"></i>Hoàn thành đúng hạn</span>";

            upnlReviewComplete.Update();

            if (waitForDialogReady)
            {
                RegisterOpenDialogAfterLoad(
                    task.IdCongViec,
                    mdlFastReview.ClientID,
                    "Duyệt nhanh hoàn thành công việc");
            }
            else
            {
                // Giữ nguyên cơ chế mở modal chuẩn cho thao tác trực tiếp trên bảng.
                mdlFastReview.OpenModal(true);
            }
        }

        /// <summary>
        /// Chờ hàm CMSMasterJs.OpenDialog thực sự được nạp rồi mới mở modal.
        /// Không tạo overlay toàn màn hình, nên không thể bị kẹt spinner.
        /// </summary>
        private void RegisterOpenDialogAfterLoad(Guid referenceId, string modalClientId, string modalTitle)
        {
            string selector = HttpUtility.JavaScriptStringEncode("#" + modalClientId);
            string title = HttpUtility.JavaScriptStringEncode(modalTitle ?? string.Empty);
            string key = "OpenDialogAfterCMSReady_" + referenceId.ToString("N");

            string script = @"
(function () {
    var selector = '__SELECTOR__';
    var title = '__TITLE__';
    var timer = null;
    var attempts = 0;
    var maxAttempts = 400; // 20 giây, kiểm tra mỗi 50 ms
    var lastError = null;
    var opened = false;

    function getCmsApi() {
        try {
            if (typeof CMSMasterJs !== 'undefined' && CMSMasterJs) {
                return CMSMasterJs;
            }
        } catch (e) { }

        try {
            if (window.CMSMasterJs) return window.CMSMasterJs;
        } catch (e) { }

        return null;
    }

    // OpenDialog bên trong CMSMasterJs gọi $(...).modal(...).
    // Chỉ kiểm tra có OpenDialog là chưa đủ: plugin Bootstrap jQuery
    // cũng phải được gắn vào đúng đối tượng $ mà trang đang sử dụng.
    function hasJQueryModalPlugin() {
        var dollar = null;

        try {
            if (typeof $ !== 'undefined') dollar = $;
        } catch (e) { }

        if (!dollar || !dollar.fn || typeof dollar.fn.modal !== 'function') {
            return false;
        }

        // CMSMasterJs.OpenDialog dùng toán tử $; kiểm tra đúng instance này.
        return true;
    }

    function finish() {
        if (timer !== null) {
            window.clearInterval(timer);
            timer = null;
        }
    }

    function tryOpen() {
        if (opened) {
            finish();
            return;
        }

        attempts++;
        var cms = getCmsApi();
        var modalElement = document.querySelector(selector);
        var cmsReady = cms && typeof cms.OpenDialog === 'function';
        var pluginReady = hasJQueryModalPlugin();

        // Chờ đủ cả hàm mở modal, Bootstrap jQuery plugin và DOM của modal.
        if (!cmsReady || !pluginReady || !modalElement) {
            if (attempts >= maxAttempts) {
                finish();
                if (window.console && typeof window.console.error === 'function') {
                    window.console.error(
                        'Không thể tự mở modal: quá thời gian chờ các phụ thuộc.',
                        {
                            openDialogReady: !!cmsReady,
                            jqueryModalReady: !!pluginReady,
                            modalElementFound: !!modalElement,
                            jqueryVersion: (window.jQuery && window.jQuery.fn) ? window.jQuery.fn.jquery : null,
                            jqueryModalType: (window.jQuery && window.jQuery.fn) ? typeof window.jQuery.fn.modal : 'missing'
                        });
                }
            }
            return;
        }

        try {
            cms.OpenDialog(selector, title);
            opened = true;
            finish();
        } catch (e) {
            // Nếu dependency đã được báo sẵn sàng mà OpenDialog vẫn lỗi,
            // dừng ngay để tránh gọi mở modal lặp lại và tạo nhiều lỗi console.
            lastError = e;
            finish();
            if (window.console && typeof window.console.error === 'function') {
                window.console.error(
                    'CMSMasterJs.OpenDialog đã được gọi nhưng vẫn lỗi. Kiểm tra jQuery/Bootstrap của MasterPage.',
                    lastError);
            }
        }
    }

    // Gọi ngay một lần rồi tiếp tục polling; không xoá URL hoặc điều hướng lại.
    tryOpen();
    if (!opened && attempts < maxAttempts) {
        timer = window.setInterval(tryOpen, 50);
    }
})();";

            script = script
                .Replace("__SELECTOR__", selector)
                .Replace("__TITLE__", title);

            ScriptManager.RegisterStartupScript(
                this.Page,
                this.Page.GetType(),
                key,
                script,
                true);
        }

        /// <summary>
        /// Mở popup kết quả dựa trên chính bản ghi thông báo của người đang đăng nhập.
        /// Nội dung kết quả được lưu trong TblThongBao.NoiDung để giữ đúng lịch sử,
        /// kể cả khi công việc sau đó chuyển sang trạng thái khác.
        /// </summary>
        public void OpenCompletionResultWhenDialogReady(Guid notificationId)
        {
            if (notificationId == Guid.Empty || SweetContext.Current == null ||
                SweetContext.Current.UserId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblThongBao notification = ThongBaoManager.Instance.GetById(notificationId);
            if (notification == null || notification.DaXoa == true ||
                notification.UserId != SweetContext.Current.UserId ||
                !string.Equals(notification.DuongDanLienKet,
                    "CtrlTaskCompletionResult", StringComparison.OrdinalIgnoreCase))
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (!notification.IdCongViec.HasValue || notification.IdCongViec.Value == Guid.Empty)
            {
                ShowNotify("Thông báo không liên kết với công việc hợp lệ.", MSGType.Warning);
                return;
            }

            TblCongViec task = TaskManager.Instance.FetchById(notification.IdCongViec.Value);
            string taskName = task == null
                ? "Công việc không còn tồn tại"
                : "[" + (task.MaCongViec ?? string.Empty) + "] " + (task.TenCongViec ?? string.Empty);

            bool isRejected = (notification.TieuDe ?? string.Empty)
                .IndexOf("từ chối", StringComparison.CurrentCultureIgnoreCase) >= 0;

            ltrCompletionDecisionBadge.Text = isRejected
                ? "<span class=\"fr-badge fr-badge-late\"><i class=\"fas fa-times-circle me-1\"></i>Yêu cầu hoàn thành bị từ chối</span>"
                : "<span class=\"fr-badge fr-badge-ontime\"><i class=\"fas fa-check-circle me-1\"></i>Yêu cầu hoàn thành đã được duyệt</span>";

            // Giao diện thông báo cũ vẫn vào popup TaskList; cung cấp đường đi nhanh đến tab Khai báo giờ.
            if (!isRejected && task != null && Kbg20TichHopTask.DuocKichHoat)
            {
                string khaiBaoUrl = "/fKhaiBaoGio/KhaiBaoGio.aspx?projectId=" + Convert.ToString(task.IdDuAn)
                    + "&reconcile=1&taskId=" + task.IdCongViec.ToString("D");
                ltrCompletionDecisionBadge.Text += "<div style=\"margin-top:12px\"><a class=\"btn btn-primary btn-sm\" href=\""
                    + HttpUtility.HtmlAttributeEncode(khaiBaoUrl)
                    + "\"><i class=\"far fa-clock me-1\"></i>Khai báo giờ thủ công</a></div>";
            }
            ltrCompletionResultTaskName.Text = HttpUtility.HtmlEncode(taskName);
            ltrCompletionResultDate.Text = notification.NgayTao.ToString("dd/MM/yyyy HH:mm");
            ltrCompletionResultBody.Text = HttpUtility.HtmlEncode(notification.NoiDung ?? string.Empty);
            upnlTaskCompletionResult.Update();

            // Popup kết quả cũng chờ OpenDialog được nạp trước khi gọi.
            RegisterOpenDialogAfterLoad(
                notification.IdThongBao,
                mdlTaskCompletionResult.ClientID,
                "Kết quả xét duyệt hoàn thành công việc");
        }

        private void NotifyAssignedEmployeesOfDecision(
            TblCongViec task,
            bool approved,
            bool isLate,
            string rejectionReason)
        {
            if (task == null)
                return;

            List<Guid> recipientIds = TaskManager.Instance.GetAssignedNhanVienIds(task.IdCongViec);
            if (recipientIds == null || recipientIds.Count == 0)
                return;

            string taskDisplay = "[" + (task.MaCongViec ?? string.Empty) + "] " + (task.TenCongViec ?? string.Empty);
            string title = approved
                ? "Yêu cầu hoàn thành đã được duyệt: " + taskDisplay
                : "Yêu cầu hoàn thành bị từ chối: " + taskDisplay;

            string body;
            if (approved)
            {
                body = "Kết quả: ĐÃ ĐƯỢC DUYỆT\r\n"
                    + "Công việc: " + taskDisplay + "\r\n"
                    + "Kết quả thời hạn: " + (isLate ? "Hoàn thành trễ hạn" : "Hoàn thành đúng hạn") + "\r\n"
                    + "Ngày báo hoàn thành: " + (task.NgayHoanThanhThucTe.HasValue ? task.NgayHoanThanhThucTe.Value.ToString("dd/MM/yyyy") : "Không có") + "\r\n"
                    + "Ngày kết thúc dự kiến: " + (task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "Không có") + "\r\n"
                    + (isLate
                        ? "Lý do trễ hạn: " + (string.IsNullOrWhiteSpace(task.LyDoTre) ? "Không có" : task.LyDoTre)
                        : "PM đã xác nhận công việc hoàn thành đúng hạn.");
            }
            else
            {
                body = "Kết quả: BỊ TỪ CHỐI\r\n"
                    + "Công việc: " + taskDisplay + "\r\n"
                    + "Lý do từ chối: " + rejectionReason + "\r\n"
                    + "Trạng thái công việc đã được trả về: Đang thực hiện.\r\n"
                    + "Ngày kết thúc dự kiến vẫn được giữ nguyên: "
                    + (task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "Không có");
            }

            if (approved && Kbg20TichHopTask.DuocKichHoat)
                body += "\r\nBạn có thể vào trang Tính giờ công việc > Khai báo giờ thủ công để nhập hoặc sửa số giờ tự khai của mình.";

            foreach (Guid recipientId in recipientIds.Where(x => x != Guid.Empty).Distinct())
            {
                try
                {
                    ThongBaoManager.Instance.Create(
                        userId: recipientId,
                        tieuDe: title,
                        noiDung: body,
                        loaiThongBao: ThongBaoTypes.CongViec,
                        idCongViec: task.IdCongViec,
                        idDuAn: task.IdDuAn,
                        duongDanLienKet: (approved && Kbg20TichHopTask.DuocKichHoat)
                            ? "/fKhaiBaoGio/KhaiBaoGio.aspx?projectId=" + Convert.ToString(task.IdDuAn)
                              + "&reconcile=1&taskId=" + task.IdCongViec.ToString("D")
                            : "CtrlTaskCompletionResult");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "Không gửi được thông báo kết quả duyệt công việc {0} cho user {1}: {2}",
                        task.IdCongViec, recipientId, ex);
                }
            }
        }

        protected void btnApproveCompletion_Click(object sender, EventArgs e)
        {
            if (!IsPM)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (!Guid.TryParse(hdfReviewTaskId.Value, out Guid taskId))
            {
                ShowNotify("Không xác định được công việc cần duyệt!", MSGType.Warning);
                return;
            }

            try
            {
                TblCongViec task = TblCongViec.FetchByID(taskId);
                if (task == null)
                {
                    ShowNotify("Không tìm thấy công việc!", MSGType.Warning);
                    return;
                }

                // Đọc lại trạng thái tại server, tránh duyệt task đã được người khác xử lý.
                if (task.TrangThai != 4)
                {
                    ShowNotify("Công việc không còn ở trạng thái Chờ duyệt!", MSGType.Warning);
                    return;
                }

                if (!task.NgayKetThuc.HasValue || !task.NgayHoanThanhThucTe.HasValue)
                {
                    ShowNotify("Không thể duyệt vì thiếu ngày kết thúc dự kiến hoặc ngày báo hoàn thành!", MSGType.Warning);
                    return;
                }

                // DOT 4B: giu quyen TU CHOI yeu cau cu, chi chan Duyet neu hau due chua xong.
                TaskManager.Instance.DamBaoHauDueDaHoanThanh(task.IdDuAn, task.IdCongViec);

                bool isLate = task.NgayHoanThanhThucTe.Value.Date > task.NgayKetThuc.Value.Date;
                if (isLate)
                    task.TrangThai = 3;
                else
                    task.TrangThai = 2;

                if (!isLate)
                    task.LyDoTre = null;

                task.NgayCapNhat = DateTime.Now;
                Action luuNghiepVuTask = () =>
                {
                    // DOT 4B: kiem tra lai trong callback chot gio va luu Task.
                    TaskManager.Instance.DamBaoHauDueDaHoanThanh(task.IdDuAn, task.IdCongViec);
                    task.Save();
                    UpdateParentTask(task.IdCongViecCha);
                };
                if (LuuDuyetQuaKbg20 != null)
                    LuuDuyetQuaKbg20(task.IdCongViec, luuNghiepVuTask);
                else
                    Kbg20TichHopTask.DuyetHoanThanh(ProjectId,task.IdCongViec,luuNghiepVuTask);
                NotifyAssignedEmployeesOfDecision(task, approved: true, isLate: isLate, rejectionReason: null);

                ShowNotify(
                    isLate
                        ? "Đã duyệt hoàn thành công việc trễ hạn!"
                        : "Đã duyệt hoàn thành công việc đúng hạn!",
                    MSGType.Success);

                ClosePopupAndNotifyGrid();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnRejectCompletion_Click(object sender, EventArgs e)
        {
            if (!IsPM)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (!Guid.TryParse(hdfReviewTaskId.Value, out Guid taskId))
            {
                ShowNotify("Không xác định được công việc cần trả về!", MSGType.Warning);
                return;
            }

            try
            {
                TblCongViec task = TblCongViec.FetchByID(taskId);
                if (task == null)
                {
                    ShowNotify("Không tìm thấy công việc!", MSGType.Warning);
                    return;
                }

                // Chỉ từ chối khi task vẫn đang chờ duyệt.
                if (task.TrangThai != 4)
                {
                    ShowNotify("Công việc không còn ở trạng thái Chờ duyệt!", MSGType.Warning);
                    return;
                }

                string rejectionReason = (txtRejectReason.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    ShowNotify("Vui lòng nhập lý do từ chối trước khi trả công việc về thực hiện.", MSGType.Warning);
                    return;
                }

                if (rejectionReason.Length > 1000)
                    rejectionReason = rejectionReason.Substring(0, 1000);

                task.TrangThai = 1;
                task.NgayHoanThanhThucTe = null;
                task.LyDoTre = null;
                task.NgayCapNhat = DateTime.Now;
                task.Save();

                // Giữ nguyên NgayKetThuc vì đây là hạn dự kiến.
                UpdateParentTask(task.IdCongViecCha);
                NotifyAssignedEmployeesOfDecision(task, approved: false, isLate: false, rejectionReason: rejectionReason);

                ShowNotify(
                    "Yêu cầu chưa đạt yêu cầu. Công việc đã được trả về trạng thái Đang thực hiện!",
                    MSGType.Success);

                ClosePopupAndNotifyGrid();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void UpdateParentTask(Guid? parentTaskId)
        {
            if (parentTaskId.HasValue && ProjectId != Guid.Empty)
            {
                var dictPriorities = TaskManager.Instance.GetDictPriorities();
                TaskManager.Instance.AutoSetParentPriority(ProjectId, parentTaskId.Value, dictPriorities);
                TaskManager.Instance.AutoSetParentTime(ProjectId, parentTaskId.Value);
                TaskManager.Instance.AutoSetParentStatus(ProjectId, parentTaskId.Value);
            }
        }

        private void ClosePopupAndNotifyGrid()
        {
            mdlFastReview.CloseModal();
            ActionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
