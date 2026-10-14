using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlFastCompleteTask : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;

        // Dùng trực tiếp hàm phân quyền PM mà dự án đã có.
        private bool IsPM => this.CURRENT_PAGE.IsPM;

        private Guid ProjectId
        {
            get
            {
                if (this.Page is BaseAdminPage basePage &&
                    basePage.CurrentProjectId != Guid.Empty)
                {
                    return basePage.CurrentProjectId;
                }

                return Guid.Empty;
            }
        }

        /// <summary>
        /// Mở popup báo hoàn thành nhanh.
        /// Chức năng duyệt/từ chối task Chờ duyệt được xử lý bởi CtrlFastReviewTask riêng.
        /// </summary>
        public void OpenModal(Guid taskId, string taskCode, string taskName)
        {
            TblCongViec task = TblCongViec.FetchByID(taskId);

            if (task == null)
            {
                ShowNotify("Không tìm thấy công việc cần xử lý!", MSGType.Warning);
                return;
            }

            if (task.TrangThai == (byte)TrangThaiCongViec.HoanThanh ||
                task.TrangThai == (byte)TrangThaiCongViec.HoanThanhTre)
            {
                ShowNotify("Công việc này đã hoàn thành!", MSGType.Warning);
                return;
            }

            if (task.TrangThai == (byte)TrangThaiCongViec.ChoDuyet)
            {
                ShowNotify(
                    IsPM
                        ? "Công việc đang Chờ duyệt. Vui lòng sử dụng chức năng duyệt nhanh."
                        : "Công việc đang chờ PM duyệt hoàn thành!",
                    MSGType.Warning);
                return;
            }

            if (!task.NgayBatDau.HasValue || !task.NgayKetThuc.HasValue)
            {
                ShowNotify(
                    "Công việc này chưa có ngày bắt đầu hoặc ngày kết thúc dự kiến hợp lệ!",
                    MSGType.Warning);
                return;
            }

            hdfCompleteTaskId.Value = taskId.ToString();
            hdfExpectedEndDate.Value = task.NgayKetThuc.Value.ToString("yyyy-MM-dd");

            ltrTaskName.Text = HttpUtility.HtmlEncode(
                "[" + (taskCode ?? string.Empty) + "] " + (taskName ?? string.Empty));

            ltrExpectedEndDate.Text = task.NgayKetThuc.Value.ToString("dd/MM/yyyy");
            ltrActionHeading.Text = IsPM
                ? "Xác nhận hoàn thành công việc"
                : "Gửi yêu cầu hoàn thành";
            ltrConfirmButtonText.Text = IsPM
                ? "Xác nhận hoàn thành"
                : "Gửi PM duyệt";

            // Mặc định chọn ngày hôm nay; không cho chọn trước ngày bắt đầu hoặc sau hôm nay.
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            txtActualDate.Text = todayStr;
            txtActualDate.Attributes["max"] = todayStr;
            txtActualDate.Attributes["min"] = task.NgayBatDau.Value.ToString("yyyy-MM-dd");
            txtLateReason.Text = string.Empty;

            upnlFastComplete.Update();

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "InitFastStatus",
                "setTimeout(calculateFastCompleteStatus, 150);",
                true);

            mdlFastComplete.OpenModal(true);
        }

        /// <summary>
        /// PM xác nhận trực tiếp thì hoàn thành ngay.
        /// Người dùng không phải PM gửi yêu cầu thì chuyển sang Chờ duyệt.
        /// </summary>
        protected void btnConfirmComplete_Click(object sender, EventArgs e)
        {
            if (!Guid.TryParse(hdfCompleteTaskId.Value, out Guid taskId))
            {
                ShowNotify("Không xác định được công việc cần xử lý!", MSGType.Warning);
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

                // Chặn gửi lại hoặc dùng nhầm popup khi task đã chờ PM duyệt.
                if (task.TrangThai == (byte)TrangThaiCongViec.ChoDuyet)
                {
                    ShowNotify(
                        "Công việc đang Chờ duyệt. Vui lòng sử dụng chức năng duyệt nhanh.",
                        MSGType.Warning);
                    return;
                }

                if (task.TrangThai == (byte)TrangThaiCongViec.HoanThanh ||
                    task.TrangThai == (byte)TrangThaiCongViec.HoanThanhTre)
                {
                    ShowNotify("Công việc này đã hoàn thành!", MSGType.Warning);
                    return;
                }

                if (!task.NgayBatDau.HasValue || !task.NgayKetThuc.HasValue)
                {
                    ShowNotify(
                        "Công việc chưa có ngày bắt đầu hoặc ngày kết thúc dự kiến!",
                        MSGType.Warning);
                    return;
                }

                // Input type="date" gửi giá trị theo định dạng yyyy-MM-dd.
                if (!DateTime.TryParseExact(
                    txtActualDate.Text,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime actualDate))
                {
                    ShowNotify("Ngày hoàn thành thực tế không hợp lệ!", MSGType.Warning);
                    return;
                }

                actualDate = actualDate.Date;

                DateTime today = DateTime.Today;
                DateTime startDate = task.NgayBatDau.Value.Date;
                DateTime expectedEndDate = task.NgayKetThuc.Value.Date;

                if (actualDate < startDate || actualDate > today)
                {
                    ShowNotify(
                        "Ngày thực tế phải nằm trong khoảng từ ngày bắt đầu đến hôm nay!",
                        MSGType.Warning);
                    return;
                }

                bool isLate = actualDate > expectedEndDate;
                string reason = (txtLateReason.Text ?? string.Empty).Trim();

                if (isLate && string.IsNullOrWhiteSpace(reason))
                {
                    ShowNotify(
                        "Công việc hoàn thành trễ hạn. Vui lòng nhập lý do trễ hạn!",
                        MSGType.Warning);

                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "KeepLateUI",
                        "setTimeout(calculateFastCompleteStatus, 100);",
                        true);
                    return;
                }

                // Lưu ngày nhân viên báo hoàn thành; NgayKetThuc vẫn là ngày dự kiến.
                task.NgayHoanThanhThucTe = actualDate;
                task.LyDoTre = isLate ? reason : null;

                if (IsPM)
                {
                    // PM xác nhận trực tiếp: hoàn thành ngay, đúng hạn hoặc trễ hạn.
                    task.TrangThai = isLate
                        ? (byte)TrangThaiCongViec.HoanThanhTre
                        : (byte)TrangThaiCongViec.HoanThanh;
                }
                else
                {
                    // Người không phải PM chỉ gửi yêu cầu; chưa phải hoàn thành chính thức.
                    task.TrangThai = (byte)TrangThaiCongViec.ChoDuyet;
                }

                task.NgayCapNhat = DateTime.Now;
                task.Save();

                if (IsPM)
                {
                    UpdateParentTask(task.IdCongViecCha);

                    ShowNotify(
                        isLate
                            ? "Đã xác nhận hoàn thành công việc trễ hạn!"
                            : "Đã xác nhận hoàn thành công việc đúng hạn!",
                        MSGType.Success);
                }
                else
                {
                    Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);

                    if (pmId.HasValue && pmId.Value != Guid.Empty)
                    {
                        try
                        {
                            string tieuDe = $"Yêu cầu duyệt hoàn thành cho công việc [{task.MaCongViec}] {task.TenCongViec}";
                            string noiDung =
                                $"Công việc [{task.MaCongViec}] {task.TenCongViec} " +
                                "đã được gửi yêu cầu hoàn thành. " +
                                "Vui lòng kiểm tra và xác nhận kết quả.";
                            ThongBaoManager.Instance.CreateTaskReminderNotification(
                                pmId.Value,
                                tieuDe,
                                noiDung,
                                task.IdCongViec,
                                ProjectId,
                                task.MaCongViec,
                                task.TenCongViec,
                                task.NgayKetThuc.Value);
                        }
                        catch (Exception)
                        {
                           
                        }
                    }

                    ShowNotify(
                        "Đã gửi yêu cầu hoàn thành đến PM. Công việc đang chờ duyệt!",
                        MSGType.Success);
                }

                ClosePopupAndNotifyGrid();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        private void UpdateParentTask(Guid? idCongViecCha)
        {
            if (idCongViecCha.HasValue && ProjectId != Guid.Empty)
            {
                var dictPriorities = TaskManager.Instance.GetDictPriorities();

                TaskManager.Instance.AutoSetParentPriority(
                    ProjectId,
                    idCongViecCha.Value,
                    dictPriorities);

                TaskManager.Instance.AutoSetParentTime(
                    ProjectId,
                    idCongViecCha.Value);

                TaskManager.Instance.AutoSetParentStatus(
                    ProjectId,
                    idCongViecCha.Value);
            }
        }

        private void ClosePopupAndNotifyGrid()
        {
            mdlFastComplete.CloseModal();
            ActionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
