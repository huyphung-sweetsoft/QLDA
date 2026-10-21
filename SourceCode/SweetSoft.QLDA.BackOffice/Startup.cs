using System;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Microsoft.AspNet.SignalR;
using SweetSoft.QLDA.BackOffice.Realtime;
using SweetSoft.QLDA.Core.Managers;
[assembly: OwinStartup(typeof(SweetSoft.QLDA.BackOffice.Startup))]
namespace SweetSoft.QLDA.BackOffice
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Realtime mac dinh TAT; chi bat tren TEST sau khi da restore NuGet + rebuild.
            // Khong thay Forms Authentication, khong can OWIN cookie middleware moi.
            if (QldaRealtimeSettings.Enabled)
            {
                app.MapSignalR("/qlda-realtime", new HubConfiguration
                {
                    EnableDetailedErrors = false,
                    EnableJavaScriptProxies = false
                });
            }

            // KBG20: hook mot lan tai server, phat sau COMMIT cho nhom nhan vien truc tiep.
            // Moi lan doi trang thai (gui duyet / PM duyet) deu di qua cung Core Manager,
            // ke ca khi dung CtrlFastCompleteTask tu TaskList thay vi tu KhaiBaoGio.
            if (QldaRealtimeSettings.KhaiBaoGioEnabled)
            {
                Kbg20TichHopTask.SauCommitDongHoTask = (projectId, taskId) =>
                {
                    var assignees = TaskManager.Instance.GetAssignedNhanVienIds(taskId);
                    QldaRealtimePublisher.TaskTimersStopped(projectId, taskId, assignees);
                };
            }
            else
            {
                Kbg20TichHopTask.SauCommitDongHoTask = null;
            }

            // Dot 3: Core chi callback UserId; BackOffice publish den nhom SignalR
            // SAU khi Insert/Read/Delete hoan tat va ambient transaction da COMMIT.
            if (QldaRealtimeSettings.NotificationsEnabled)
            {
                ThongBaoManager.SauKhiThongBaoThayDoi = userId =>
                    QldaRealtimePublisher.NotificationsChanged(new[] { userId });
            }
            else
            {
                ThongBaoManager.SauKhiThongBaoThayDoi = null;
            }

            GlobalConfiguration.Configuration.UseSqlServerStorage("SweetSoft.QLDA.BackOffice");
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            RecurringJob.AddOrUpdate("Check-Meetings-Tasks", () => NotificationJob.QuetVaGuiThongBao(), Cron.Minutely);
            RecurringJob.AddOrUpdate("KhaiBaoGio-Chot-So", () => KhaiBaoGioCloseJob.Run(), Cron.Minutely);
        }
    }
}