using System;
using System.Collections.Generic;
using System.Linq;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    public class EmployeeDashboardManager : BaseManager
    {
        private static readonly Lazy<EmployeeDashboardManager> LazyInstance =
            new Lazy<EmployeeDashboardManager>(() => new EmployeeDashboardManager());

        private readonly DashboardRepository _repository;

        public EmployeeDashboardManager(
            IAppContext applicationContext = null,
            DashboardRepository repository = null)
            : base(applicationContext)
        {
            _repository = repository ?? new DashboardRepository();
        }

        public static EmployeeDashboardManager Instance => LazyInstance.Value;

        public DashboardUserContext GetUserContext(Guid userId, bool isAdmin)
        {
            AspnetUser employee = _repository.GetEmployeeByUserId(userId);

            return new DashboardUserContext
            {
                UserId = userId,
                EmployeeId = employee == null
                    ? (Guid?)null
                    : employee.UserId,
                IsAdmin = isAdmin
            };
        }

        public AspnetUser GetEmployeeByUserId(Guid userId)
        {
            return _repository.GetEmployeeByUserId(userId);
        }

        public List<TblDuAn> GetProjectsForEmployee(Guid employeeId)
        {
            return _repository.GetEmployeeProjects(
                employeeId,
                new DashboardFilter(),
                false);
        }

        public EmployeeDashboardModel GetEmployeeOverview(
            Guid employeeId,
            DashboardFilter filter)
        {
            EmployeeDashboardModel model = new EmployeeDashboardModel();
            if (employeeId == Guid.Empty)
            {
                return model;
            }

            filter = filter ?? new DashboardFilter();

            List<TblDuAn> allMyProjects =
                _repository.GetEmployeeProjects(employeeId, filter, false);
            List<TblDuAn> activeProjects =
                _repository.GetEmployeeProjects(employeeId, filter, true);

            model.KPIs.ActiveProjectCount = activeProjects.Count;

            List<TblCongViec> allMyTasks =
                _repository.GetEmployeeTasks(employeeId, filter, false);
            List<TblCongViec> tasksInPeriod =
                _repository.GetEmployeeTasks(employeeId, filter, true);

            DateTime today = DateTime.Now.Date;

            model.KPIs.OngoingTaskCount = tasksInPeriod.Count(t =>
                t.NgayBatDau.HasValue &&
                t.NgayBatDau.Value.Date <= today &&
                !t.NgayHoanThanhThucTe.HasValue);

            model.KPIs.UpcomingDeadlineTaskCount = tasksInPeriod.Count(t =>
                !t.NgayHoanThanhThucTe.HasValue &&
                t.NgayKetThuc.HasValue &&
                t.NgayKetThuc.Value.Date >= today &&
                (t.NgayKetThuc.Value.Date - today).TotalDays <= 3);

            model.KPIs.OverdueTaskCount = tasksInPeriod.Count(t =>
                !t.NgayHoanThanhThucTe.HasValue &&
                t.NgayKetThuc.HasValue &&
                t.NgayKetThuc.Value.Date < today);

            // Chưa có dữ liệu định mức giờ làm việc để tính tải chính xác.
            model.KPIs.WorkloadPercent = 0;

            List<TblCongViec> topTasks = allMyTasks
                .Where(t => !t.NgayHoanThanhThucTe.HasValue)
                .OrderBy(t => t.NgayKetThuc ?? DateTime.MaxValue)
                .Take(10)
                .ToList();

            foreach (TblCongViec task in topTasks)
            {
                TblDuAn project =
                    allMyProjects.FirstOrDefault(p => p.IdDuAn == task.IdDuAn);

                model.MyTasks.Add(new EmployeeTaskInfo
                {
                    TaskId = task.IdCongViec,
                    TaskName = task.TenCongViec,
                    ProjectCode = project == null ? string.Empty : project.MaDuAn,
                    ProjectName = project == null ? string.Empty : project.TenDuAn,
                    Deadline = task.NgayKetThuc,
                    Progress = task.PhanTramHoanThanh
                });
            }

            List<Guid> activeProjectIds =
                activeProjects.Select(p => p.IdDuAn).ToList();
            List<TblCongViec> projectTasks =
                _repository.GetTasksForProjects(activeProjectIds);

            foreach (TblDuAn project in activeProjects)
            {
                List<TblCongViec> tasks = projectTasks
                    .Where(t => t.IdDuAn == project.IdDuAn)
                    .ToList();

                int progress = 0;
                if (project.NgayHoanThanhThucTe.HasValue)
                {
                    progress = 100;
                }
                else if (project.NgayBatDau.Date <= today && tasks.Count > 0)
                {
                    progress = (int)tasks.Average(t => t.PhanTramHoanThanh);
                }

                model.MyProjects.Add(new ProjectProgressStatistic
                {
                    ProjectCode = project.MaDuAn,
                    ProjectName = project.TenDuAn,
                    Progress = progress,
                    StartDate = project.NgayBatDau,
                    ExpectedEndDate = project.NgayDuKienHoanThanh
                });
            }

            model.MyProjects = model.MyProjects
                .OrderByDescending(p => p.Progress)
                .ToList();

            AddTaskWarnings(model);

            List<TblLichHop> meetings = _repository.GetUpcomingMeetings(
                employeeId,
                filter,
                today,
                5);

            foreach (TblLichHop meeting in meetings)
            {
                TblDuAn project =
                    allMyProjects.FirstOrDefault(p => p.IdDuAn == meeting.IdDuAn);

                model.UpcomingMeetings.Add(new EmployeeMeeting
                {
                    MeetingId = meeting.IdLichHop,
                    Title = meeting.TenCuocHop,
                    ProjectName = project == null
                        ? string.Empty
                        : project.TenDuAn,
                    StartTime = meeting.ThoiGianBatDau
                });
            }

            foreach (EmployeeMeeting meeting in model.UpcomingMeetings)
            {
                model.MyWarnings.Add(new EmployeeWarning
                {
                    Type = WarningType.Meeting,
                    Message = string.Format(
                        "Có cuộc họp dự án {0} lúc {1:HH:mm} ngày {1:dd/MM}",
                        meeting.ProjectName,
                        meeting.StartTime),
                    IconClass = "fe fe-calendar",
                    TextClass = "text-info"
                });
            }

            return model;
        }

        private static void AddTaskWarnings(EmployeeDashboardModel model)
        {
            if (model.KPIs.OverdueTaskCount > 0)
            {
                model.MyWarnings.Add(new EmployeeWarning
                {
                    Type = WarningType.OverdueTask,
                    Message = string.Format(
                        "{0} công việc đã quá hạn",
                        model.KPIs.OverdueTaskCount),
                    IconClass = "fe fe-alert-triangle",
                    TextClass = "text-danger"
                });
            }

            if (model.KPIs.UpcomingDeadlineTaskCount > 0)
            {
                model.MyWarnings.Add(new EmployeeWarning
                {
                    Type = WarningType.UpcomingTask,
                    Message = string.Format(
                        "{0} công việc sắp đến hạn trong 3 ngày",
                        model.KPIs.UpcomingDeadlineTaskCount),
                    IconClass = "fe fe-clock",
                    TextClass = "text-warning"
                });
            }
        }
    }
}
