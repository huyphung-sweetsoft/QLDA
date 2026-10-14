using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlFastReviewTask : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;

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
            mdlFastReview.OpenModal(true);
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

                bool isLate = task.NgayHoanThanhThucTe.Value.Date > task.NgayKetThuc.Value.Date;
                if (isLate)
                    task.TrangThai = 3;
                else
                    task.TrangThai = 2;

                if (!isLate)
                    task.LyDoTre = null;

                task.NgayCapNhat = DateTime.Now;
                task.Save();

                UpdateParentTask(task.IdCongViecCha);

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

                task.TrangThai = 1;
                task.NgayHoanThanhThucTe = null;
                task.LyDoTre = null;
                task.NgayCapNhat = DateTime.Now;
                task.Save();

                // Giữ nguyên NgayKetThuc vì đây là hạn dự kiến.
                UpdateParentTask(task.IdCongViecCha);

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
