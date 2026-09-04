using System;
using SweetSoft.QLDA.Core.Dashboard;
using SweetSoft.QLDA.DataAccess;

internal static class DashboardSmokeTest
{
    private static int Main()
    {
        try
        {
            DashboardRepository repository = new DashboardRepository();
            DashboardFilter filter = new DashboardFilter
            {
                DateRange = DashboardDateRange.Custom,
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 12, 31)
            };

            int projectCount = repository.GetProjectsForFilter().Count;
            int riskCount = repository.GetRisks(filter).Count;
            int issueCount = repository.GetIssues(filter).Count;
            int meetingCount = repository.GetMeetings(filter).Count;
            int weekConfigurationCount = repository
                .GetWorkWeekConfigurations().Count;
            int calendarExceptionCount = repository
                .GetCalendarExceptions(
                    new DateTime(2026, 1, 1),
                    new DateTime(2026, 12, 31)).Count;

            int activeRecordRowCount =
                new TblKhachHangController().FetchAll().Count
                + new TblLichHopController().FetchAll().Count
                + new TblRuiRoController().FetchAll().Count
                + new TblRuiRoDuAnController().FetchAll().Count
                + new TblVanDeController().FetchAll().Count
                + new TblVanDeNhanVienController().FetchAll().Count
                + new TblCauHinhTuanLamViecController().FetchAll().Count
                + new TblLichNgoaiLeController().FetchAll().Count;

            DashboardOverviewModel overview =
                new DashboardOverviewManager(null, repository)
                    .GetOverview(filter);
            DashboardProgressModel progress =
                new DashboardProgressManager(null, repository)
                    .GetProgress(filter);
            DashboardCostModel cost =
                new DashboardCostManager(null, repository)
                    .GetCostDashboard(new DashboardCostFilter
                    {
                        Period = DashboardCostPeriod.AllTime
                    });
            DashboardResourceModel resource =
                new DashboardResourceManager(null, repository)
                    .GetResourceDashboard(new DashboardResourceFilter
                    {
                        AnchorWeekStart = new DateTime(2026, 8, 31),
                        WeekCount = 4
                    });

            Console.WriteLine(
                "Repository: projects={0}; risks={1}; issues={2}; meetings={3}; week-config={4}; exceptions={5}",
                projectCount,
                riskCount,
                issueCount,
                meetingCount,
                weekConfigurationCount,
                calendarExceptionCount);
            Console.WriteLine(
                "QLDA3 ActiveRecord models loaded {0} rows without schema errors.",
                activeRecordRowCount);
            Console.WriteLine(
                "Dashboards: overview-projects={0}; progress-tasks={1}; cost-completed={2}; resource-employees={3}; anchor-capacity={4}",
                overview.TotalProjectCount,
                progress.TotalTaskCount,
                cost.CompletedProjectCount,
                resource.TotalEmployeeCount,
                resource.EmployeeLoads.Count == 0
                    ? 0
                    : resource.EmployeeLoads[0].CapacityDays);

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
