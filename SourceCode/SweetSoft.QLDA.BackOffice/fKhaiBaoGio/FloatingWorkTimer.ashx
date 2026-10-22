<%@ WebHandler Language="C#" Class="FloatingWorkTimerHandler" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

// KhaiBaoGio 2.0: chi doc tat ca phien MO cua tai khoan, khong thay doi CSDL.
// Inline .ashx: khong can code-behind, khong can them Compile vao .csproj.
public class FloatingWorkTimerHandler : IHttpHandler, IReadOnlySessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static long Ms(DateTime utc)
    {
        return (long)(DateTime.SpecifyKind(utc, DateTimeKind.Utc) - Epoch).TotalMilliseconds;
    }

    private static long? MsNullable(DateTime? utc)
    {
        return utc.HasValue ? (long?)Ms(utc.Value) : null;
    }

    private static void Write(HttpResponse response, object value)
    {
        response.Write(new JavaScriptSerializer().Serialize(value));
    }

    private static string ProjectTimeTrackingUrl(Guid projectId)
    {
        // Cung quy tac URL voi RewriteURLHelper.ProjectTasks/ProjectMeets.
        // Khong gui GUID thuan len path: projectId phai duoc bao ve nhu cac trang du an.
        string protectedId = SecurityUtilities.ProtectUrlParameter(projectId.ToString());
        return VirtualPathUtility.ToAbsolute("~/Project/" + protectedId + "/TimeTracking");
    }

    public void ProcessRequest(HttpContext context)
    {
        HttpResponse response = context.Response;
        response.ContentType = "application/json; charset=utf-8";
        response.Cache.SetCacheability(HttpCacheability.NoCache);
        response.Cache.SetNoStore();
        response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        response.TrySkipIisCustomErrors = true;
        response.SuppressFormsAuthenticationRedirect = true;

        if (context.Request.HttpMethod != "GET")
        {
            response.StatusCode = 405;
            response.AddHeader("Allow", "GET");
            Write(response, new { ok = false, error = "method_not_allowed" });
            return;
        }

        try
        {
            AspnetUser user = SweetContext.Current != null ? SweetContext.Current.User : null;
            if (user == null || user.UserId == Guid.Empty)
            {
                response.StatusCode = 401;
                Write(response, new { ok = false, error = "not_authenticated" });
                return;
            }
            if (!user.LaNhanVien)
            {
                response.StatusCode = 403;
                Write(response, new { ok = false, error = "not_employee" });
                return;
            }
            if (!KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
            {
                // Dang chay V1: khong su dung so gio/cu phieu cua V1 de hien thi nhu V2.
                Write(response, new { ok = true, running = false, disabled = true, serverNow = Ms(DateTime.UtcNow), timers = new object[0] });
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;
            var views = new KhaiBaoGio20Manager().LayDongHoHienThi(user.UserId, nowUtc);
            var projectUrls = new Dictionary<Guid, string>();
            var items = views.Select(x =>
            {
                Kbg20Phien p = x.Phien;
                string projectUrl;
                if (!projectUrls.TryGetValue(p.IdDuAn, out projectUrl))
                {
                    projectUrl = ProjectTimeTrackingUrl(p.IdDuAn);
                    projectUrls[p.IdDuAn] = projectUrl;
                }
                return new
                {
                    id = p.IdPhien.ToString("D"),
                    taskId = p.IdCongViec.ToString("D"),
                    projectId = p.IdDuAn.ToString("D"),
                    code = p.MaCongViec ?? "",
                    name = p.TenCongViec ?? "",
                    projectCode = p.MaDuAn ?? "",
                    projectName = p.TenDuAn ?? "",
                    projectUrl = projectUrl,
                    type = (byte)p.LoaiGio, // 1 = gio thuong; 2 = tang ca
                    source = p.NguonBatDau,
                    overdue = p.LaTiepTucQuaHan,
                    seconds = x.SoGiayHienTai, // tong giay tinh rieng cua phien mo
                    counting = x.DangCongGio, // phien dang mo CHUA chac dang cong gio
                    boundary = MsNullable(x.MocDungCongLucUtc)
                };
            }).ToArray();

            Write(response, new
            {
                ok = true,
                running = items.Length > 0, // phien DANG MO, ke ca dang nghi/ngoai ca
                serverNow = Ms(nowUtc),
                timers = items
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("FloatingWorkTimer V2: {0}", ex);
            response.StatusCode = 500;
            Write(response, new { ok = false, error = "server_error" });
        }
    }
}
