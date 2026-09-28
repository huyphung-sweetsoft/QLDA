using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlFastCompleteTask : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;

        private Guid ProjectId
        {
            get
            {
                if (this.Page is BaseAdminPage basePage && basePage.CurrentProjectId != Guid.Empty)
                    return basePage.CurrentProjectId;
                return Guid.Empty;
            }
        }

        public void OpenModal(Guid taskId, string taskCode, string taskName)
        {
            TblCongViec task = TblCongViec.FetchByID(taskId);
            if (task == null || !task.NgayBatDau.HasValue || !task.NgayKetThuc.HasValue)
            {
                ShowNotify("Công việc này chưa có ngày bắt đầu hoặc ngày kết thúc hợp lệ!", MSGType.Warning);
                return;
            }

            hdfCompleteTaskId.Value = taskId.ToString();

            // Lưu định dạng YYYY-MM-DD ẩn đi để Javascript đọc và so sánh
            hdfExpectedEndDate.Value = task.NgayKetThuc.Value.ToString("yyyy-MM-dd");

            ltrTaskName.Text = $"[{taskCode}] {taskName}";
            ltrExpectedEndDate.Text = task.NgayKetThuc.Value.ToString("dd/MM/yyyy");

            // Thiết lập giá trị cho ô chọn ngày thực tế
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            string minDateStr = task.NgayBatDau.Value.ToString("yyyy-MM-dd");

            txtActualDate.Text = todayStr; // Auto fill là ngày hôm nay
            txtActualDate.Attributes["max"] = todayStr; // Không được vượt quá hôm nay
            txtActualDate.Attributes["min"] = minDateStr; // Không được nhỏ hơn ngày bắt đầu

            txtLateReason.Text = string.Empty;

            // Chạy hàm Javascript để tự đánh giá trạng thái ngay lúc vừa mở form lên
            ScriptManager.RegisterStartupScript(this, GetType(), "InitFastStatus", "setTimeout(calculateFastCompleteStatus, 150);", true);

            upnlFastComplete.Update();
            mdlFastComplete.OpenModal(true);
        }

        protected void btnConfirmComplete_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(hdfCompleteTaskId.Value, out Guid taskId))
            {
                if (!DateTime.TryParse(txtActualDate.Text, out DateTime actualDate))
                {
                    ShowNotify("Ngày hoàn thành thực tế không hợp lệ!", MSGType.Warning);
                    return;
                }

                try
                {
                    TblCongViec task = TblCongViec.FetchByID(taskId);
                    if (task != null && task.NgayKetThuc.HasValue && task.NgayBatDau.HasValue)
                    {
                        // Ràng buộc bảo mật phía Server
                        if (actualDate.Date < task.NgayBatDau.Value.Date || actualDate.Date > DateTime.Now.Date)
                        {
                            ShowNotify("Ngày thực tế phải nằm trong khoảng từ Ngày bắt đầu đến Hôm nay!", MSGType.Warning);
                            return;
                        }

                        // So sánh Thực tế vs Dự kiến
                        bool isLate = actualDate.Date > task.NgayKetThuc.Value.Date;

                        if (isLate)
                        {
                            string reason = txtLateReason.Text.Trim();
                            if (string.IsNullOrEmpty(reason))
                            {
                                ShowNotify("Tiến độ bị trễ so với dự kiến. Vui lòng nhập lý do trễ hạn!", MSGType.Warning);
                                ScriptManager.RegisterStartupScript(this, GetType(), "KeepLateUI", "setTimeout(calculateFastCompleteStatus, 100);", true);
                                return;
                            }
                            task.TrangThai = 3; // Hoàn thành trễ hạn
                            task.LyDoTre = reason;
                        }
                        else
                        {
                            task.TrangThai = 2; // Hoàn thành đúng hạn
                            task.LyDoTre = null;
                        }

                        task.NgayHoanThanhThucTe = actualDate;
                        task.NgayCapNhat = DateTime.Now;
                        task.Save();

                        // Tự động cập nhật thời gian & trạng thái cho Task cha (Roll-up)
                        UpdateParentTask(task.IdCongViecCha);

                        ShowNotify(task.TrangThai == 3 ? "Đã xác nhận hoàn thành công việc (Trễ hạn)!" : "Công việc hoàn thành đúng tiến độ!", MSGType.Success);
                        mdlFastComplete.CloseModal();

                        // Bắn sự kiện ra ngoài để load lại Grid
                        if (ActionCompleted != null) ActionCompleted(this, EventArgs.Empty);
                    }
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }

        // Hàm hỗ trợ Roll-up lên Task cha
        private void UpdateParentTask(Guid? idCongViecCha)
        {
            if (idCongViecCha.HasValue && this.ProjectId != Guid.Empty)
            {
                var dictPriorities = TaskManager.Instance.GetDictPriorities();
                TaskManager.Instance.AutoSetParentPriority(this.ProjectId, idCongViecCha.Value, dictPriorities);
                TaskManager.Instance.AutoSetParentTime(this.ProjectId, idCongViecCha.Value);
                TaskManager.Instance.AutoSetParentStatus(this.ProjectId, idCongViecCha.Value);
            }
        }
    }
}