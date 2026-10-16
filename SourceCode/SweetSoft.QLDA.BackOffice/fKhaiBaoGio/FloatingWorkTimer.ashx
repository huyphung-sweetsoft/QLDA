<%@ WebHandler Language="C#" Class="FloatingWorkTimerHandler" %>

using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

// Inline .ashx: works with Web Application projects without modifying a .csproj to compile a new handler class.
// Read-only: no Start/Stop endpoint is exposed here.
public class FloatingWorkTimerHandler : IHttpHandler, IReadOnlySessionState
{
    public bool IsReusable { get { return false; } }

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static long ToMs(DateTime local)
    {
        DateTime utc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();
        return (long)(utc - Epoch).TotalMilliseconds;
    }
    public void ProcessRequest(HttpContext context)
    {
        HttpResponse response = context.Response;
        response.ContentType = "application/json; charset=utf-8";
        response.Cache.SetCacheability(HttpCacheability.NoCache);
        response.Cache.SetNoStore();
        response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        response.TrySkipIisCustomErrors = true;

        if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
        {
            response.StatusCode = 405;
            response.AddHeader("Allow", "GET");
            response.Write("{\"error\":\"method_not_allowed\"}");
            return;
        }
        try
        {
            AspnetUser user = SweetContext.Current != null ? SweetContext.Current.User : null;
            if (user == null || user.UserId == Guid.Empty)
            {
                response.StatusCode = 401;
                response.Write("{\"error\":\"not_authenticated\"}");
                return;
            }
            if (!user.LaNhanVien)
            {
                response.StatusCode = 403;
                response.Write("{\"error\":\"not_employee\"}");
                return;
            }

            DateTime now = DateTime.Now;
            FloatingTimerSnapshot view = KhaiBaoGioManager.Instance.GetFloatingTimerSnapshot(user.UserId, now);
            if (!view.Running)
            {
                response.Write("{\"running\":false}");
                return;
            }
            var json = new
            {
                running = true,
                taskId = view.TaskId.ToString("D"),
                taskCode = view.TaskCode,
                taskName = view.TaskName,
                projectCode = view.ProjectCode,
                projectName = view.ProjectName,
                serverNow = ToMs(view.ServerNow),
                runningSince = ToMs(view.RunningSince),
                dayEnd = ToMs(view.DayEnd),
                completedSeconds = view.CompletedSeconds,
                capacitySeconds = view.CapacitySeconds,
                segments = view.Segments.Select(x => new { start = ToMs(x.BatDau), end = ToMs(x.KetThuc) }).ToArray()
            };
            response.Write(new JavaScriptSerializer().Serialize(json));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("FloatingWorkTimer: {0}", ex);
            response.StatusCode = 500;
            response.Write("{\"error\":\"server_error\"}");
        }
    }
}
