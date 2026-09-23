using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Managers;

namespace SweetSoft.QLDA.BackOffice.fGanttCharts
{
    public partial class Gantts : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.GanttChart;

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }

                modalIssues.Title = "Thông tin công việc";
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.GANTT_CHART));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.GANTT_CHART);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.GANTT_CHART) }
                };

                this.DataBind();
                BindGanttChart(CurrentProjectId);
            }
        }

        protected string GetProjectName()
        {
            var project = DuAnManager.Instance.GetDuAnById(CurrentProjectId);
            return !string.IsNullOrEmpty(project?.TenDuAn)
                ? $"{GetResourceText(BackEndResourceKeys.GANTT_CHART)} - {project.TenDuAn}"
                : $"{GetResourceText(BackEndResourceKeys.GANTT_CHART)} {GetResourceText(BackEndResourceKeys.PROJECT_PROGRESS)}";
        }

        private void BindGanttChart(Guid projectId)
        {
            DataTable dtTasks = GanttChartManager.Instance.GetGanttTasks(projectId);

            if (dtTasks == null || dtTasks.Rows.Count == 0) return;

            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;

            foreach (DataRow row in dtTasks.Rows)
            {
                if (row["NgayBatDau"] != DBNull.Value)
                {
                    DateTime d = Convert.ToDateTime(row["NgayBatDau"]);
                    if (d < minDate) minDate = d;
                }
                if (row["NgayKetThuc"] != DBNull.Value)
                {
                    DateTime d = Convert.ToDateTime(row["NgayKetThuc"]);
                    if (d > maxDate) maxDate = d;
                }
            }

            if (minDate == DateTime.MaxValue) return;
            minDate = minDate.AddDays(-1);
            if (maxDate <= minDate) maxDate = minDate.AddDays(7);
            maxDate = maxDate.AddDays(1);

            int totalDays = (int)(maxDate - minDate).TotalDays + 1;
            int todayIndex = (int)(DateTime.Now.Date - minDate.Date).TotalDays;

            List<string> dateLabels = new List<string>();
            for (int i = 0; i < totalDays; i++)
            {
                dateLabels.Add($"'{minDate.AddDays(i).ToString("dd/MM")}'");
            }
            string jsDateLabels = "[" + string.Join(",", dateLabels) + "]";

            var taskList = new List<GanttTaskItem>();

            for (int i = 0; i < dtTasks.Rows.Count; i++)
            {
                DataRow row = dtTasks.Rows[i];
                string maCv = row["MaCongViec"]?.ToString() ?? "";

                int explicitIssueCount = Convert.ToInt32(row["IssueCount"] ?? 0);
                int totalAlerts = explicitIssueCount;

                foreach (DataRow r in dtTasks.Rows)
                {
                    string childMaCv = r["MaCongViec"]?.ToString() ?? "";
                    bool isChildOverdue = Convert.ToBoolean(r["IsOverdue"]);

                    if (isChildOverdue && (childMaCv == maCv || childMaCv.StartsWith(maCv + ".")))
                    {
                        totalAlerts++;
                    }
                }

                int level = maCv.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (level == 0) level = 1;

                bool hasChild = false;
                if (i < dtTasks.Rows.Count - 1)
                {
                    string nextMaCv = dtTasks.Rows[i + 1]["MaCongViec"]?.ToString() ?? "";
                    if (nextMaCv.StartsWith(maCv + ".")) hasChild = true;
                }

                int startDay = 0, endDay = 1;
                if (row["NgayBatDau"] != DBNull.Value)
                    startDay = (int)(Convert.ToDateTime(row["NgayBatDau"]).Date - minDate.Date).TotalDays;

                if (row["NgayKetThuc"] != DBNull.Value)
                    endDay = (int)(Convert.ToDateTime(row["NgayKetThuc"]).Date - minDate.Date).TotalDays + 1;

                if (endDay <= startDay) endDay = startDay + 1;

                string status = "todo";
                if (row["TrangThai"] != DBNull.Value)
                {
                    int dbTrangThai = Convert.ToInt32(row["TrangThai"]);
                    if (dbTrangThai == 2) status = "done";
                    else if (dbTrangThai == 1) status = "doing";
                }

                taskList.Add(new GanttTaskItem
                {
                    TaskId = row["IdCongViec"]?.ToString(),
                    DependsOn = row["IdCongViecPhuThuoc"]?.ToString() ?? "",
                    MaCongViec = maCv,
                    TenCongViec = row["TenCongViec"]?.ToString(),
                    NhanVienThucHien = row["NhanVienThucHien"]?.ToString() ?? "",
                    Level = level,
                    HasChild = hasChild,
                    StartDay = startDay,
                    EndDay = endDay,
                    StatusClass = status,
                    AlertCount = totalAlerts,
                    IssueCount = explicitIssueCount
                });
            }

            rptTaskNames.DataSource = taskList;
            rptTaskNames.DataBind();

            rptChartTracks.DataSource = taskList;
            rptChartTracks.DataBind();

            string jsConfig = $@"
                var GANTT_TOTAL_DAYS = {totalDays};
                var GANTT_TODAY_INDEX = {todayIndex};
                var GANTT_DATE_LABELS = {jsDateLabels};
            ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "GanttConfig", jsConfig, true);
        }

        protected void btnLoadIssues_Click(object sender, EventArgs e)
        {
            string taskId = hdfSelectedTaskId.Value;
            string taskCode = hdfSelectedTaskCode.Value;
            if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(taskCode)) return;

            DataTable dtAllTasks = GanttChartManager.Instance.GetGanttTasks(CurrentProjectId);
            DataTable dtTaskInfo = null;
            List<string> targetIds = new List<string>();
            string rootTaskName = "";

            if (dtAllTasks != null)
            {
                dtTaskInfo = dtAllTasks.Clone();
                dtTaskInfo.Columns.Add("RelativeLevel", typeof(int));
                int baseLevel = taskCode.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length;

                foreach (DataRow row in dtAllTasks.Rows)
                {
                    string code = row["MaCongViec"]?.ToString() ?? "";

                    if (code == taskCode || code.StartsWith(taskCode + "."))
                    {
                        DataRow newRow = dtTaskInfo.NewRow();
                        foreach (DataColumn col in dtAllTasks.Columns)
                        {
                            newRow[col.ColumnName] = row[col.ColumnName];
                        }

                        int currentLevel = code.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        newRow["RelativeLevel"] = currentLevel - baseLevel + 1;

                        dtTaskInfo.Rows.Add(newRow);
                        targetIds.Add(row["IdCongViec"]?.ToString() ?? "");

                        if (code == taskCode)
                        {
                            rootTaskName = row["TenCongViec"]?.ToString() ?? "";
                        }
                    }
                }
            }

            h6TaskTitle.InnerHtml = $"<i class='fas fa-info-circle me-1'></i> {taskCode}. {rootTaskName}";
            modalIssues.Title = "Thông tin công việc";

            rptTaskInfo.DataSource = dtTaskInfo;
            rptTaskInfo.DataBind();

            DataTable dtTaskIssues = null;
            foreach (string id in targetIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                DataTable dt = GanttChartManager.Instance.GetTaskIssues(id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dtTaskIssues == null) dtTaskIssues = dt.Clone();
                    foreach (DataRow r in dt.Rows) dtTaskIssues.ImportRow(r);
                }
            }

            bool hasIssues = dtTaskIssues != null && dtTaskIssues.Rows.Count > 0;
            phHasIssues.Visible = hasIssues;
            phNoIssues.Visible = !hasIssues;

            if (hasIssues)
            {
                rptIssues.DataSource = dtTaskIssues;
                rptIssues.DataBind();
            }
            modalIssues.OpenModal(true);
        }

        #region Helpers dùng cho file ASPX

        protected int GetTaskLevel(object maCvObj)
        {
            if (maCvObj == null || maCvObj == DBNull.Value) return 1;
            string maCv = maCvObj.ToString();
            int level = maCv.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return level == 0 ? 1 : level;
        }

        protected string GetRowClass(int level, bool hasChild)
        {
            string cls = level > 1 ? $"task-level-child level-{level}" : "";
            if (level >= 2 && hasChild)
            {
                cls += " collapsed";
            }
            return cls.Trim();
        }

        protected string GetClickEvents(bool hasChild, string maCv)
        {
            return hasChild ? $"onclick='toggleTask(\"{maCv}\", this)' style='cursor:pointer;' onmouseover='this.classList.add(\"row-hover\")' onmouseout='this.classList.remove(\"row-hover\")'" : "";
        }

        protected string GetTaskStatusText(int status)
        {
            if (status == 1) return "Đang làm";
            if (status == 2) return "Hoàn thành";
            return "Chưa bắt đầu";
        }

        protected string GetTaskStatusBadge(int status)
        {
            if (status == 1) return "gantt-badge-doing";
            if (status == 2) return "gantt-badge-done";
            return "gantt-badge-todo";
        }

        protected string GetTaskWarningHtml(object trangThaiObj, object ngayKetThucObj)
        {
            int status = trangThaiObj != DBNull.Value ? Convert.ToInt32(trangThaiObj) : 0;
            if (status == 2) return "";

            if (ngayKetThucObj != DBNull.Value)
            {
                DateTime endDate = Convert.ToDateTime(ngayKetThucObj).Date;
                int remainingDays = (int)(endDate - DateTime.Now.Date).TotalDays;

                if (remainingDays < 0)
                    return "<div class='mt-1 text-danger' style='font-size: 11px; font-weight: bold;'>(Trễ hạn)</div>";
                else if (remainingDays <= 2)
                    return "<div class='mt-1 text-warning' style='font-size: 11px; font-weight: bold;'>(Sắp đến hạn)</div>";
            }
            return "";
        }

        protected string GetIssueStatusText(int status)
        {
            if (status == 1) return "Đang xử lý";
            else return "Đã xử lý";
        }

        protected string GetIssueStatusBadge(int status)
        {
            if (status == 1) return "gantt-badge-doing";
            if (status == 2) return "gantt-badge-done";
            return "gantt-badge-todo";
        }
        #endregion
    }

    public class GanttTaskItem
    {
        public string TaskId { get; set; }
        public string DependsOn { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public string NhanVienThucHien { get; set; }
        public int Level { get; set; }
        public bool HasChild { get; set; }
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public string StatusClass { get; set; }
        public int AlertCount { get; set; }
        public int IssueCount { get; set; }
    }
}