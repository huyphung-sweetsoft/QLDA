using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Centralizes the schedule rules shared by the overview and progress
    /// dashboards so the same project and task always receive the same result.
    /// </summary>
    internal static class DashboardProgressCalculator
    {
        internal const int DefaultDueSoonDays = 7;

        internal static int NormalizeProgress(int progress)
        {
            return Math.Max(0, Math.Min(100, progress));
        }

        internal static decimal GetProjectActualProgress(
            TblDuAn project,
            IEnumerable<TblCongViec> tasks,
            DateTime today)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return 100;
            }

            List<TblCongViec> projectTasks = tasks == null
                ? new List<TblCongViec>()
                : tasks.ToList();

            if (project.NgayBatDau.Date > today.Date
                || projectTasks.Count == 0)
            {
                return 0;
            }

            return Math.Round(
                Convert.ToDecimal(projectTasks.Average(x =>
                    NormalizeProgress(x.PhanTramHoanThanh))),
                2);
        }

        internal static decimal GetPlannedProgress(
            TblDuAn project,
            DateTime today)
        {
            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return 100;
            }

            return GetPlannedProgress(
                project.NgayBatDau,
                project.NgayDuKienHoanThanh,
                today);
        }

        internal static decimal GetPlannedProgress(
            DateTime startDate,
            DateTime endDate,
            DateTime today)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;
            today = today.Date;

            if (today <= startDate)
            {
                return 0;
            }

            if (today >= endDate || endDate <= startDate)
            {
                return 100;
            }

            decimal elapsedDays = Convert.ToDecimal(
                (today - startDate).TotalDays);
            decimal totalDays = Convert.ToDecimal(
                (endDate - startDate).TotalDays);

            return Math.Round((elapsedDays / totalDays) * 100, 2);
        }

        internal static DashboardProjectState GetProjectState(
            TblDuAn project,
            DateTime today)
        {
            today = today.Date;

            if (project.NgayHoanThanhThucTe.HasValue)
            {
                return DashboardProjectState.Completed;
            }

            if (project.NgayBatDau.Date > today)
            {
                return DashboardProjectState.NotStarted;
            }

            if (project.NgayDuKienHoanThanh.Date < today)
            {
                return DashboardProjectState.Overdue;
            }

            return DashboardProjectState.InProgress;
        }

        internal static DashboardTaskState GetTaskState(
            TblCongViec task,
            DateTime today)
        {
            if (IsTaskCompleted(task))
            {
                return DashboardTaskState.Completed;
            }

            if (IsTaskOverdue(task, today))
            {
                return DashboardTaskState.Overdue;
            }

            return NormalizeProgress(task.PhanTramHoanThanh) > 0
                ? DashboardTaskState.InProgress
                : DashboardTaskState.NotStarted;
        }

        internal static bool IsTaskCompleted(TblCongViec task)
        {
            return task.NgayHoanThanhThucTe.HasValue
                || NormalizeProgress(task.PhanTramHoanThanh) >= 100;
        }

        internal static bool IsTaskOverdue(
            TblCongViec task,
            DateTime today)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date < today.Date;
        }

        internal static bool IsTaskDueSoon(
            TblCongViec task,
            DateTime today,
            int dueSoonDays = DefaultDueSoonDays)
        {
            return !IsTaskCompleted(task)
                && task.NgayKetThuc.HasValue
                && task.NgayKetThuc.Value.Date >= today.Date
                && task.NgayKetThuc.Value.Date
                    <= today.Date.AddDays(dueSoonDays);
        }

        internal static ProjectScheduleHealth GetProjectHealth(
            TblDuAn project,
            decimal variance,
            int overdueTaskCount,
            DateTime today)
        {
            DashboardProjectState state = GetProjectState(project, today);

            if (state == DashboardProjectState.Completed)
            {
                return ProjectScheduleHealth.Completed;
            }

            if (state == DashboardProjectState.NotStarted)
            {
                return ProjectScheduleHealth.NotStarted;
            }

            if (state == DashboardProjectState.Overdue)
            {
                return ProjectScheduleHealth.Overdue;
            }

            if (variance <= -15)
            {
                return ProjectScheduleHealth.BehindSchedule;
            }

            if (variance < -5 || overdueTaskCount > 0)
            {
                return ProjectScheduleHealth.AtRisk;
            }

            return ProjectScheduleHealth.OnTrack;
        }
    }
}
