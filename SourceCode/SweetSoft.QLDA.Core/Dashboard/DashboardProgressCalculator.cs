using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    internal enum DashboardProjectState
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Overdue = 3
    }

    internal enum DashboardTaskState
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Overdue = 3
    }

    /// <summary>
    /// Gom các quy tắc tính tiến độ dùng chung để Dashboard tổng quan và
    /// Dashboard tiến độ luôn xác định cùng một kết quả cho dự án, công việc.
    /// </summary>
    internal static class DashboardProgressCalculator
    {
        private const int MinimumProgressPercent = 0;
        private const int CompletedProgressPercent = 100;
        private const byte CompletedTaskStatus = 2;

        // Công việc kết thúc trong khoảng này được xem là sắp đến hạn.
        internal const int DefaultDueSoonDays = 7;

        // Chênh lệch = tiến độ thực tế - tiến độ kế hoạch.
        private const decimal BehindScheduleVarianceThreshold = -15m;
        private const decimal AtRiskVarianceThreshold = -5m;

        // Giới hạn tiến độ nhập vào trong khoảng từ 0% đến 100%.
        internal static int NormalizeProgress(int rawProgressPercent)
        {
            return Math.Max(
                MinimumProgressPercent,
                Math.Min(CompletedProgressPercent, rawProgressPercent));
        }

        /// <summary>
        /// Trạng thái Hoàn thành là nguồn xác định duy nhất cho dự án. Ngày
        /// hoàn thành thực tế chỉ là thông tin ngày tháng, không được phép
        /// ghi đè trạng thái Tạm dừng hoặc trạng thái khác.
        /// </summary>
        internal static bool IsProjectCompleted(TblDuAn project)
        {
            return project != null
                && project.TrangThai == (byte)DuAnStatus.HoanThanh;
        }

        /// <summary>
        /// Công việc có trạng thái Hoàn thành luôn được hiển thị 100%, kể cả
        /// khi phần trăm cũ trong dữ liệu chưa được đồng bộ.
        /// </summary>
        internal static int GetTaskActualProgress(TblCongViec task)
        {
            return IsTaskCompleted(task)
                ? CompletedProgressPercent
                : NormalizeProgress(task.PhanTramHoanThanh);
        }

        /// <summary>
        /// Không đếm task gốc của giai đoạn khi nó đã có task con. Task gốc chỉ
        /// là nhóm tổng hợp; giữ nó lại nếu giai đoạn chưa có task con nào.
        /// </summary>
        internal static List<TblCongViec> ExcludeStageRootTasksWithChildren(
            IEnumerable<TblCongViec> tasks)
        {
            List<TblCongViec> taskList = tasks == null
                ? new List<TblCongViec>()
                : tasks.ToList();
            HashSet<Guid> parentTaskIds = new HashSet<Guid>(
                taskList.Where(task => task.IdCongViecCha.HasValue)
                    .Select(task => task.IdCongViecCha.Value));

            return taskList.Where(task =>
                    !task.IdGiaiDoanDuAn.HasValue
                    || task.IdCongViecCha.HasValue
                    || !parentTaskIds.Contains(task.IdCongViec))
                .ToList();
        }

        /// <summary>
        /// Tiến độ thực tế là 100% khi dự án đã hoàn thành; nếu chưa thì lấy
        /// trung bình phần trăm hoàn thành của các công việc trong dự án.
        /// </summary>
        internal static decimal GetProjectActualProgress(
            TblDuAn project,
            IEnumerable<TblCongViec> tasks,
            DateTime calculationDate)
        {
            if (IsProjectCompleted(project))
            {
                return CompletedProgressPercent;
            }

            List<TblCongViec> projectTaskList = tasks == null
                ? new List<TblCongViec>()
                : tasks.ToList();

            if (project.NgayBatDau.Date > calculationDate.Date
                || projectTaskList.Count == 0)
            {
                return MinimumProgressPercent;
            }

            return Math.Round(
                Convert.ToDecimal(projectTaskList.Average(
                    GetTaskActualProgress)),
                2);
        }

        /// <summary>
        /// Tiến độ kế hoạch tăng đều từ ngày bắt đầu đến ngày dự kiến hoàn thành.
        /// </summary>
        internal static decimal GetPlannedProgress(
            TblDuAn project,
            DateTime calculationDate)
        {
            return GetPlannedProgress(project, calculationDate, null);
        }

        /// <summary>
        /// Tiến độ kế hoạch dùng ngày làm việc khi Dashboard đã có lịch tuần
        /// và ngoại lệ. Khi chưa có đủ cấu hình, giữ cách tính ngày lịch cũ.
        /// </summary>
        internal static decimal GetPlannedProgress(
            TblDuAn project,
            DateTime calculationDate,
            DashboardWorkingCalendar workingCalendar)
        {
            if (IsProjectCompleted(project))
            {
                return CompletedProgressPercent;
            }

            return GetPlannedProgress(
                project.NgayBatDau,
                project.NgayDuKienHoanThanh,
                calculationDate,
                workingCalendar);
        }

        internal static decimal GetPlannedProgress(
            DateTime projectStartDate,
            DateTime plannedEndDate,
            DateTime calculationDate)
        {
            return GetPlannedProgress(
                projectStartDate,
                plannedEndDate,
                calculationDate,
                null);
        }

        internal static decimal GetPlannedProgress(
            DateTime projectStartDate,
            DateTime plannedEndDate,
            DateTime calculationDate,
            DashboardWorkingCalendar workingCalendar)
        {
            projectStartDate = projectStartDate.Date;
            plannedEndDate = plannedEndDate.Date;
            calculationDate = calculationDate.Date;

            if (calculationDate <= projectStartDate)
            {
                return MinimumProgressPercent;
            }

            if (calculationDate >= plannedEndDate
                || plannedEndDate <= projectStartDate)
            {
                return CompletedProgressPercent;
            }

            if (workingCalendar != null)
            {
                int elapsedWorkingDays = workingCalendar.CountWorkingDays(
                    projectStartDate,
                    calculationDate);
                int totalWorkingDays = workingCalendar.CountWorkingDays(
                    projectStartDate,
                    plannedEndDate);

                // Giữ cùng quy ước với cách tính theo ngày lịch:
                // ngày bắt đầu = 0%, ngày kết thúc = 100%.
                // CountWorkingDays tính bao gồm cả hai đầu mút nên phải quy đổi
                // số ngày thành số khoảng giữa các ngày làm việc.
                if (totalWorkingDays > 1)
                {
                    int elapsedWorkingIntervals = Math.Max(
                        0,
                        elapsedWorkingDays - 1);
                    int totalWorkingIntervals = totalWorkingDays - 1;

                    return Math.Round(
                        Convert.ToDecimal(elapsedWorkingIntervals)
                            / totalWorkingIntervals
                            * CompletedProgressPercent,
                        2);
                }
            }

            decimal elapsedScheduleDays = Convert.ToDecimal(
                (calculationDate - projectStartDate).TotalDays);
            decimal plannedScheduleDays = Convert.ToDecimal(
                (plannedEndDate - projectStartDate).TotalDays);

            return Math.Round(
                (elapsedScheduleDays / plannedScheduleDays)
                    * CompletedProgressPercent,
                2);
        }

        /// <summary>
        /// Thứ tự ưu tiên trạng thái dự án: hoàn thành, chưa bắt đầu, quá hạn,
        /// sau đó mới là đang thực hiện.
        /// </summary>
        internal static DashboardProjectState GetProjectState(
            TblDuAn project,
            DateTime calculationDate)
        {
            calculationDate = calculationDate.Date;

            if (IsProjectCompleted(project))
            {
                return DashboardProjectState.Completed;
            }

            if (project.NgayBatDau.Date > calculationDate)
            {
                return DashboardProjectState.NotStarted;
            }

            if (project.NgayDuKienHoanThanh.Date < calculationDate)
            {
                return DashboardProjectState.Overdue;
            }

            return DashboardProjectState.InProgress;
        }

        /// <summary>
        /// Thứ tự ưu tiên trạng thái công việc: hoàn thành, quá hạn, đang thực
        /// hiện, rồi mới đến chưa bắt đầu.
        /// </summary>
        internal static DashboardTaskState GetTaskState(
            TblCongViec task,
            DateTime calculationDate)
        {
            if (IsTaskCompleted(task))
            {
                return DashboardTaskState.Completed;
            }

            if (IsTaskOverdue(task, calculationDate))
            {
                return DashboardTaskState.Overdue;
            }

            // TrangThai là nguồn nghiệp vụ chính:
            // 0 = chưa bắt đầu, 1 = đang thực hiện, 2 = hoàn thành.
            // Phần trăm chỉ dùng làm fallback hiển thị nếu dữ liệu cũ không
            // đồng bộ trạng thái.
            if (task != null && task.TrangThai == 1)
            {
                return DashboardTaskState.InProgress;
            }

            return NormalizeProgress(task == null
                    ? MinimumProgressPercent
                    : task.PhanTramHoanThanh)
                > MinimumProgressPercent
                ? DashboardTaskState.InProgress
                : DashboardTaskState.NotStarted;
        }

        /// <summary>
        /// TrangThai = 2 là nguồn nghiệp vụ chính để xác định công việc hoàn thành.
        /// Riêng công việc gốc đại diện cho giai đoạn được phép fallback theo
        /// NgayHoanThanhThucTe/100%, vì GiaiDoanDuAnManager hiện đồng bộ ngày
        /// hoàn thành sang root task nhưng chưa đồng bộ TrangThai.
        /// </summary>
        internal static bool IsTaskCompleted(TblCongViec task)
        {
            if (task == null)
            {
                return false;
            }

            if (task.TrangThai == CompletedTaskStatus)
            {
                return true;
            }

            bool isStageRootTask =
                task.IdGiaiDoanDuAn.HasValue
                && !task.IdCongViecCha.HasValue;

            return isStageRootTask
                && (task.NgayHoanThanhThucTe.HasValue
                    || NormalizeProgress(task.PhanTramHoanThanh)
                        >= CompletedProgressPercent);
        }

        internal static bool IsTaskOverdue(
            TblCongViec task,
            DateTime calculationDate)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date < calculationDate.Date;
        }

        // Chỉ xét công việc chưa hoàn thành có hạn từ ngày tính đến hết số ngày quy định.
        internal static bool IsTaskDueSoon(
            TblCongViec task,
            DateTime calculationDate,
            int dueSoonWindowDays = DefaultDueSoonDays)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date >= calculationDate.Date
                && task.NgayKetThuc.Value.Date
                    <= calculationDate.Date.AddDays(dueSoonWindowDays);
        }

        /// <summary>
        /// Chênh lệch bằng tiến độ thực tế trừ tiến độ kế hoạch. Dự án trễ từ
        /// 15 điểm phần trăm là chậm tiến độ; trễ trên 5 điểm hoặc có việc quá
        /// hạn là có nguy cơ.
        /// </summary>
        internal static ProjectScheduleHealth GetProjectHealth(
            TblDuAn project,
            decimal progressVariance,
            int overdueTaskCount,
            DateTime calculationDate)
        {
            DashboardProjectState projectState = GetProjectState(
                project,
                calculationDate);

            if (projectState == DashboardProjectState.Completed)
            {
                return ProjectScheduleHealth.Completed;
            }

            if (projectState == DashboardProjectState.NotStarted)
            {
                return ProjectScheduleHealth.NotStarted;
            }

            if (projectState == DashboardProjectState.Overdue)
            {
                return ProjectScheduleHealth.Overdue;
            }

            if (progressVariance <= BehindScheduleVarianceThreshold)
            {
                return ProjectScheduleHealth.BehindSchedule;
            }

            if (progressVariance < AtRiskVarianceThreshold
                || overdueTaskCount > 0)
            {
                return ProjectScheduleHealth.AtRisk;
            }

            return ProjectScheduleHealth.OnTrack;
        }
    }

}
