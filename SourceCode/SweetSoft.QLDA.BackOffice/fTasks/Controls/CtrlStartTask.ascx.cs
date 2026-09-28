using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlStartTask : BaseAdminUserControl
    {
        public event EventHandler ActionStarted;

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
            hdfStartTaskId.Value = taskId.ToString();
            ltrTaskName.Text = $"[{taskCode}] {taskName}";

            upnlStartTask.Update();
            mdlStartTask.OpenModal(true);
        }

        protected void btnConfirmStart_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(hdfStartTaskId.Value, out Guid taskId))
            {
                try
                {
                    TblCongViec task = TblCongViec.FetchByID(taskId);
                    if (task != null)
                    {
                        task.TrangThai = 1; // 1 = Đang thực hiện
                        task.NgayCapNhat = DateTime.Now;
                        task.Save();

                        // Roll-up lên Task cha
                        if (task.IdCongViecCha.HasValue && this.ProjectId != Guid.Empty)
                        {
                            var dictPriorities = TaskManager.Instance.GetDictPriorities();
                            TaskManager.Instance.AutoSetParentPriority(this.ProjectId, task.IdCongViecCha.Value, dictPriorities);
                            TaskManager.Instance.AutoSetParentTime(this.ProjectId, task.IdCongViecCha.Value);
                            TaskManager.Instance.AutoSetParentStatus(this.ProjectId, task.IdCongViecCha.Value);
                        }

                        ShowNotify("Công việc đã chuyển sang trạng thái Đang thực hiện!", MSGType.Success);
                        mdlStartTask.CloseModal();

                        if (ActionStarted != null) ActionStarted(this, EventArgs.Empty);
                    }
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }
    }
}