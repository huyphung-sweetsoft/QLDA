using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public sealed class StartTaskDependencyWarning
    {
        public TblCongViec DependencyTask { get; set; }
        public TblCongViec ParentTask { get; set; }
        public TblCongViec TargetTask { get; set; }
    }

    public partial class CtrlStartTask : BaseAdminUserControl
    {
        public event EventHandler ActionStarted;
        // Diem mo rong TUY CHON cho trang KhaiBaoGio 2.0. TaskList goc khong gan callback.
        // Callback bao phan luu Task cua partner va Start dong ho trong CUNG transaction.
        public Action<Guid, Action> LuuBatDauQuaKbg20 { get; set; }
        public Guid? IdDuAnNguonKbg20 { get; set; }

        private Guid ProjectId
        {
            get
            {
                if (IdDuAnNguonKbg20.HasValue && IdDuAnNguonKbg20.Value != Guid.Empty)
                    return IdDuAnNguonKbg20.Value;
                if (this.Page is BaseAdminPage basePage && basePage.CurrentProjectId != Guid.Empty)
                    return basePage.CurrentProjectId;
                return Guid.Empty;
            }
        }

        public void OpenModal(Guid taskId, string taskCode, string taskName, IList<StartTaskDependencyWarning> dependencyWarnings = null)
        {
            hdfStartTaskId.Value = taskId.ToString();

            // Format HTML cho tên công việc (Ticket ở trên cùng)
            ltrTaskName.Text = FormatTaskHtml(taskCode, taskName);

            bool hasWarnings = dependencyWarnings != null && dependencyWarnings.Count > 0;
            pnlDependencyWarning.Visible = hasWarnings;
            ltrDependencyWarning.Text = hasWarnings ? BuildDependencyWarningsHtml(dependencyWarnings) : string.Empty;
            upnlStartTask.Update();
            mdlStartTask.OpenModal(true);
        }

        private static string FormatTaskHtml(string taskCode, string taskName)
        {
            string code = string.IsNullOrWhiteSpace(taskCode) ? "" : $"<span class=\"st-hero-code\">{HttpUtility.HtmlEncode(taskCode)}</span>";
            string name = HttpUtility.HtmlEncode(taskName ?? string.Empty);
            return $"{code} <span class=\"st-hero-name\">{name}</span>";
        }

        private static string BuildTaskNodeTitle(TblCongViec task)
        {
            if (task == null) return string.Empty;
            string rawCode = (task.MaCongViec ?? string.Empty).Trim();
            string code = HttpUtility.HtmlEncode(rawCode);
            string name = HttpUtility.HtmlEncode(task.TenCongViec ?? string.Empty);

            if (string.IsNullOrWhiteSpace(rawCode))
                return $@"<span class=""st-node-title"">{name}</span>";

            return $@"<span class=""st-node-title""><strong>{code}</strong>. {name}</span>";
        }

        private static string BuildTaskWarningTooltip(TblCongViec task, string cssClass)
        {
            if (task == null) return "<span>không xác định</span>";
            string code = (task.MaCongViec ?? string.Empty).Trim();
            string name = HttpUtility.HtmlEncode(task.TenCongViec ?? string.Empty);

            if (string.IsNullOrWhiteSpace(code)) return "<span>chưa có mã</span>";

            return $"<span class=\"st-tooltip {cssClass}\" data-title=\"{name}\">{HttpUtility.HtmlEncode(code)}</span>";
        }

        private static string BuildDependencyWarningsHtml(IList<StartTaskDependencyWarning> warnings)
        {
            StringBuilder html = new StringBuilder();
            foreach (StartTaskDependencyWarning warning in warnings)
            {
                if (warning == null || warning.DependencyTask == null || warning.TargetTask == null)
                    continue;
                if (warning.ParentTask != null)
                    html.Append(BuildParentDependencyWarningHtml(warning.DependencyTask, warning.ParentTask, warning.TargetTask));
                else
                    html.Append(BuildDirectDependencyWarningHtml(warning.DependencyTask, warning.TargetTask));
            }
            return html.ToString();
        }

        private static string BuildDirectDependencyWarningHtml(TblCongViec dependencyTask, TblCongViec targetTask)
        {
            string dependencyTitle = BuildTaskNodeTitle(dependencyTask);
            string targetTitle = BuildTaskNodeTitle(targetTask);

            string dependencyTooltip = BuildTaskWarningTooltip(dependencyTask, "waiting");
            string targetTooltip = BuildTaskWarningTooltip(targetTask, "target");

            return $@"
<div class=""st-alert-box"">
    <div class=""st-flow-diagram no-parent"">
        <div class=""st-flow-col"">
            <div class=""st-flow-node st-waiting"">
                <span class=""st-node-kicker"">ĐIỀU KIỆN TIÊN QUYẾT</span>
                {dependencyTitle}
                <span class=""st-node-badge""><i class=""fas fa-hourglass-half""></i> Chưa hoàn thành</span>
            </div>
        </div>
        <div class=""st-flow-connector-horizontal""><i class=""fas fa-arrow-right""></i></div>
        <div class=""st-flow-col"">
            <div class=""st-flow-node st-target"">
                <span class=""st-node-kicker"">CÔNG VIỆC HIỆN TẠI</span>
                {targetTitle}
                <span class=""st-node-badge""><i class=""fas fa-play""></i> Sắp bắt đầu</span>
            </div>
        </div>
    </div>
    <div class=""st-alert-msg"">
        <i class=""fas fa-exclamation-triangle""></i>
        <div>Công việc {dependencyTooltip} chưa hoàn thành. Bạn có chắc chắn muốn bắt đầu ngay công việc {targetTooltip}?</div>
    </div>
</div>";
        }

        private static string BuildParentDependencyWarningHtml(TblCongViec dependencyTask, TblCongViec parentTask, TblCongViec childTask)
        {
            string dependencyTitle = BuildTaskNodeTitle(dependencyTask);
            string parentTitle = BuildTaskNodeTitle(parentTask);
            string childTitle = BuildTaskNodeTitle(childTask);

            string dependencyTooltip = BuildTaskWarningTooltip(dependencyTask, "waiting");
            string parentTooltip = BuildTaskWarningTooltip(parentTask, "parent");
            string childTooltip = BuildTaskWarningTooltip(childTask, "target");

            return $@"
<div class=""st-alert-box"">
    <div class=""st-flow-diagram"">
        <!-- Cột trái: Điều kiện -->
        <div class=""st-flow-col"">
            <div class=""st-flow-node st-waiting"">
                <span class=""st-node-kicker"">ĐIỀU KIỆN TIÊN QUYẾT</span>
                {dependencyTitle}
                <span class=""st-node-badge""><i class=""fas fa-hourglass-half""></i> Chưa hoàn thành</span>
            </div>
        </div>
        
        <!-- Mũi tên ngang -->
        <div class=""st-flow-connector-horizontal""><i class=""fas fa-arrow-right""></i></div>
        
        <!-- Cột phải: Gom 2 thẻ Việc Cha và Việc Con vào một cột, nằm sát nhau, đồng cỡ -->
        <div class=""st-flow-col"">
            <div class=""st-flow-node st-parent"">
                <span class=""st-node-kicker"">CÔNG VIỆC CHA</span>
                {parentTitle}
                <span class=""st-node-badge""><i class=""fas fa-sitemap""></i> Bị ảnh hưởng</span>
            </div>
            
            <div class=""st-flow-node st-target"">
                <span class=""st-node-kicker"">CÔNG VIỆC CON (ĐANG THAO TÁC)</span>
                {childTitle}
                <span class=""st-node-badge""><i class=""fas fa-play""></i> Sắp bắt đầu</span>
            </div>
        </div>
    </div>

    <div class=""st-alert-msg"">
        <i class=""fas fa-exclamation-triangle""></i>
        <div>Công việc {dependencyTooltip} chưa hoàn thành. Bắt đầu việc con {childTooltip} sẽ trực tiếp bắt đầu công việc {parentTooltip}. Bạn vẫn muốn tiếp tục?</div>
    </div>
</div>";
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
                        // Ngiu yen tung lenh cua partner; trang KBG20 chi bao giao dich o ngoai.
                        Action luuNghiepVuTask = () =>
                        {
                            task.TrangThai = 1;
                            task.NgayCapNhat = DateTime.Now;
                            task.Save();
                            if (task.IdCongViecCha.HasValue && this.ProjectId != Guid.Empty)
                            {
                                var dictPriorities = TaskManager.Instance.GetDictPriorities();
                                TaskManager.Instance.AutoSetParentPriority(this.ProjectId, task.IdCongViecCha.Value, dictPriorities);
                                TaskManager.Instance.AutoSetParentTime(this.ProjectId, task.IdCongViecCha.Value);
                                TaskManager.Instance.AutoSetParentStatus(this.ProjectId, task.IdCongViecCha.Value);
                            }
                        };
                        if (LuuBatDauQuaKbg20 != null)
                            LuuBatDauQuaKbg20(task.IdCongViec, luuNghiepVuTask);
                        else
                            luuNghiepVuTask();
                        ShowNotify("Công việc đã chuyển sang trạng thái Đang thực hiện!", MSGType.Success);
                        mdlStartTask.CloseModal();
                        if (ActionStarted != null)
                            ActionStarted(this, EventArgs.Empty);
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