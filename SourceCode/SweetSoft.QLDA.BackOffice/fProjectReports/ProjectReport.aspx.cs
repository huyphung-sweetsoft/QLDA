using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Data;

namespace SweetSoft.QLDA.BackOffice.fProjectReports
{
    public partial class ProjectReport : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Project;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags("Báo cáo Tiến độ Dự án");
                Navigation1.MainTitle = "Báo cáo Tiến độ";
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", "Báo cáo Tiến độ" }
                };

                LoadReportData();
            }
        }

        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReportData();
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            LoadReportData();
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            ShowNotify("Chức năng xuất PDF đang được phát triển!", MSGType.Info);
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            ShowNotify("Chức năng xuất Excel đang được phát triển!", MSGType.Info);
        }

        private void LoadReportData()
        {
            Guid projectId = CurrentProjectId;

            DataTable dtProject = ProjectReportManager.Instance.GetProjectInfo(projectId);

            if (dtProject.Rows.Count == 0) return;

            int projectStatus = dtProject.Rows[0]["TrangThai"] != DBNull.Value ? Convert.ToInt32(dtProject.Rows[0]["TrangThai"]) : 0;

            if (projectStatus == 0)
            {
                phReportContent.Visible = false;
                phNotStarted.Visible = true;
                btnExportPDF.Visible = false;
                btnExportExcel.Visible = false;
                return;
            }
            else
            {
                phReportContent.Visible = true;
                phNotStarted.Visible = false;
                btnExportPDF.Visible = true;
                btnExportExcel.Visible = true;
            }

            DateTime? projStartDate = dtProject.Rows[0]["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(dtProject.Rows[0]["NgayBatDau"]) : (DateTime?)null;
            DateTime? projEndDate = dtProject.Rows[0]["NgayHoanThanhThucTe"] != DBNull.Value ? Convert.ToDateTime(dtProject.Rows[0]["NgayHoanThanhThucTe"]) : (DateTime?)null;

            DateTime minLimitDate = projStartDate ?? new DateTime(2020, 1, 1);
            DateTime maxLimitDate = (projectStatus == 1) ? DateTime.Today : (projEndDate ?? DateTime.Today);

            txtFromDate.Attributes["min"] = minLimitDate.ToString("yyyy-MM-dd");
            txtFromDate.Attributes["max"] = maxLimitDate.ToString("yyyy-MM-dd");
            txtToDate.Attributes["min"] = minLimitDate.ToString("yyyy-MM-dd");
            txtToDate.Attributes["max"] = maxLimitDate.ToString("yyyy-MM-dd");

            DateTime? fromDate = null;
            DateTime? toDate = null;
            string periodName = ddlPeriod.SelectedItem.Text;
            DateTime today = DateTime.Today;

            switch (ddlPeriod.SelectedValue)
            {
                case "THIS_WEEK":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = today.AddDays(-1 * diff).Date;
                    toDate = fromDate.Value.AddDays(6).Date;
                    periodName = $"Tuần này ({fromDate.Value:dd/MM} - {toDate.Value:dd/MM})";
                    break;
                case "LAST_WEEK":
                    int diff2 = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = today.AddDays(-1 * diff2).AddDays(-7).Date;
                    toDate = fromDate.Value.AddDays(6).Date;
                    periodName = $"Tuần trước ({fromDate.Value:dd/MM} - {toDate.Value:dd/MM})";
                    break;
                case "THIS_MONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    periodName = $"Tháng {today.Month}/{today.Year}";
                    break;
                case "LAST_MONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    periodName = $"Tháng {fromDate.Value.Month}/{fromDate.Value.Year}";
                    break;
                case "ALL":
                    fromDate = minLimitDate;
                    toDate = maxLimitDate;
                    periodName = $"Toàn thời gian ({fromDate.Value:dd/MM/yyyy} - {toDate.Value:dd/MM/yyyy})";
                    break;
                case "CUSTOM":
                    if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
                    {
                        DateTime tempFrom, tempTo;
                        bool isValidFrom = DateTime.TryParse(txtFromDate.Text, out tempFrom);
                        bool isValidTo = DateTime.TryParse(txtToDate.Text, out tempTo);

                        if (!isValidFrom || !isValidTo)
                        {
                            ShowNotify("Định dạng ngày không hợp lệ!", MSGType.Error);
                            return;
                        }

                        fromDate = tempFrom;
                        toDate = tempTo;

                        if (fromDate > toDate)
                        {
                            ShowNotify("Thời gian không hợp lệ!", MSGType.Error);
                            return;
                        }

                        if (fromDate < minLimitDate) fromDate = minLimitDate;
                        if (toDate > maxLimitDate) toDate = maxLimitDate;

                        periodName = $"Từ {fromDate.Value:dd/MM/yyyy} đến {toDate.Value:dd/MM/yyyy}";
                    }
                    else
                    {
                        string eventTarget = Request.Form["__EVENTTARGET"] ?? "";

                        if (eventTarget.Contains("ddlPeriod"))
                        {
                            return;
                        }
                        else
                        {
                            ShowNotify("Vui lòng chọn đầy đủ thời gian!", MSGType.Warning);
                            return;
                        }
                    }
                    break;
            }

            if (ddlPeriod.SelectedValue != "CUSTOM")
            {
                if (fromDate.HasValue && fromDate.Value < minLimitDate) fromDate = minLimitDate;
                if (toDate.HasValue && toDate.Value > maxLimitDate) toDate = maxLimitDate;
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                {
                    fromDate = minLimitDate;
                    toDate = maxLimitDate;
                }
            }

            ltrReportPeriod.Text = $"Kỳ báo cáo: {periodName}";

            int totalTasks = ProjectReportManager.Instance.GetTotalTasks(projectId, fromDate, toDate);
            DataTable dtCompleted = ProjectReportManager.Instance.GetCompletedTasks(projectId, fromDate, toDate);
            DataTable dtOverdue = ProjectReportManager.Instance.GetOverdueTasks(projectId, fromDate, toDate);
            DataTable dtIssues = ProjectReportManager.Instance.GetIssues(projectId, fromDate, toDate);

            int countCompleted = dtCompleted.Rows.Count;
            int countOverdue = dtOverdue.Rows.Count;

            ltrTotalTasks.Text = totalTasks.ToString();

            if (totalTasks > 0)
            {
                ltrCompletedTasks.Text = $"{countCompleted}/{totalTasks} task ({(double)countCompleted / totalTasks * 100:0.0}%)";
                ltrOverdueTasks.Text = $"{countOverdue}/{totalTasks} task ({(double)countOverdue / totalTasks * 100:0.0}%)";
            }
            else
            {
                ltrCompletedTasks.Text = "0 task (0%)";
                ltrOverdueTasks.Text = "0 task (0%)";
            }

            ltrTotalIssues.Text = dtIssues.Rows.Count.ToString();

            rptCompletedTasks.DataSource = dtCompleted;
            rptCompletedTasks.DataBind();

            rptOverdueTasks.DataSource = dtOverdue;
            rptOverdueTasks.DataBind();

            rptIssues.DataSource = dtIssues;
            rptIssues.DataBind();
        }

        #region Helpers dùng cho file ASPX

        protected string GetPriorityText(int priority)
        {
            if (priority == 3) return "Cao (High)";
            if (priority == 2) return "Trung bình";
            return "Thấp (Low)";
        }

        protected string GetPriorityBadge(int priority)
        {
            if (priority == 3) return "badge-high";
            if (priority == 2) return "badge-med";
            return "badge-low";
        }

        protected string GetIssueStatusText(int status)
        {
            if (status == 1) return "Đang xử lý";
            if (status == 2) return "Đã xử lý";
            return "Mới tạo";
        }

        protected string GetIssueStatusBadge(int status)
        {
            if (status == 1) return "badge-warning";
            if (status == 2) return "badge-success";
            return "badge-info";
        }
        #endregion
    }
}