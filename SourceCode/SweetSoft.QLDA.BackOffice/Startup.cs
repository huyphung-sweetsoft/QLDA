using System;
using Microsoft.Owin;
using Owin;
using Hangfire;
using SweetSoft.QLDA.Core.Managers;
[assembly: OwinStartup(typeof(SweetSoft.QLDA.BackOffice.Startup))]
namespace SweetSoft.QLDA.BackOffice
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("SweetSoft.QLDA.BackOffice");
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            RecurringJob.AddOrUpdate("Check-Meetings-Tasks", () => NotificationJob.QuetVaGuiThongBao(), Cron.Minutely);
        }
    }
}