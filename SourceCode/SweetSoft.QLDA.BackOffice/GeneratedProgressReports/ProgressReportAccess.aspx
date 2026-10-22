<%@ Page Language="C#" EnableViewState="false" EnableSessionState="false" ValidateRequest="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Caching" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="SweetSoft.QLDA.BackOffice.fGanttCharts.Controls" %>
<script runat="server">
    private static readonly object AttemptLock = new object();
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
    private sealed class OpenRequest { public string reportId { get; set; } public string otp { get; set; } }
    private sealed class FailCount { public int Value { get; set; } }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Response.ContentType = "application/json; charset=utf-8";
        Response.TrySkipIisCustomErrors = true;
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["X-Frame-Options"] = "DENY";
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers["Allow"] = "POST";
            SendError(405, "Phương thức yêu cầu không hợp lệ."); return;
        }
        try
        {
            if (Request.ContentLength <= 0 || Request.ContentLength > 2048)
            { SendError(400, "Yêu cầu không hợp lệ."); return; }
            string body;
            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8)) body = reader.ReadToEnd();
            OpenRequest data = Json.Deserialize<OpenRequest>(body);
            if (data == null || string.IsNullOrWhiteSpace(data.reportId) || !Regex.IsMatch(data.reportId, "^[a-fA-F0-9]{32}$"))
            { SendError(400, "Mã báo cáo không hợp lệ."); return; }

            string reportId = data.reportId.ToLowerInvariant();
            string path = Server.MapPath("~/App_Data/ProgressReports/" + reportId + ".json");
            if (!File.Exists(path))
            { SendError(404, "Báo cáo không tồn tại hoặc đã bị xóa."); return; }
            ProgressReportRecord record = Json.Deserialize<ProgressReportRecord>(File.ReadAllText(path, Encoding.UTF8));
            if (record == null || !string.Equals(record.Id, reportId, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(record.ReportHtml) || string.IsNullOrWhiteSpace(record.OtpSalt) || string.IsNullOrWhiteSpace(record.OtpHash))
            { SendError(404, "Dữ liệu báo cáo không hợp lệ."); return; }

            // Only internal, active, authenticated accounts can bypass OTP.
            // A legacy report with no ProjectId MUST keep its OTP requirement.
            // Identity is validated server-side, never taken from a JS flag, email, referrer or request field.
            Guid projectId;
            if (Guid.TryParse(record.ProjectId, out projectId) && projectId != Guid.Empty && IsActiveInternalEmployee())
            {
                SendReport(record);
                return;
            }

            // The first auto-check intentionally sends an empty OTP. This does not use up attempts.
            if (string.IsNullOrEmpty(data.otp))
            {
                Response.StatusCode = 200;
                Response.Write(Json.Serialize(new { success = false, requiresOtp = true }));
                return;
            }
            if (!Regex.IsMatch(data.otp, "^[0-9]{6}$"))
            { SendError(400, "OTP phải gồm 6 chữ số."); return; }

            string ip = Request.UserHostAddress ?? "unknown";
            string key = "gantt:otp:" + reportId + ":" + ip;
            lock (AttemptLock)
            {
                FailCount counter = HttpRuntime.Cache[key] as FailCount;
                if (counter != null && counter.Value >= 8)
                { SendError(429, "Bạn đã thử quá nhiều lần. Vui lòng thử lại sau 15 phút."); return; }
            }
            string computedHash;
            using (var derive = new Rfc2898DeriveBytes(data.otp, Convert.FromBase64String(record.OtpSalt), 100000))
                computedHash = Convert.ToBase64String(derive.GetBytes(32));
            if (!ConstantTimeEqual(computedHash, record.OtpHash))
            {
                lock (AttemptLock)
                {
                    FailCount counter = HttpRuntime.Cache[key] as FailCount;
                    if (counter == null)
                    {
                        counter = new FailCount();
                        HttpRuntime.Cache.Insert(key, counter, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
                    }
                    counter.Value++;
                }
                SendError(403, "OTP không chính xác. Vui lòng kiểm tra lại.");
                return;
            }
            lock (AttemptLock) HttpRuntime.Cache.Remove(key);
            SendReport(record);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Lỗi truy cập báo cáo tiến độ: " + ex);
            SendError(500, "Không thể mở báo cáo lúc này. Vui lòng thử lại.");
        }
    }

    private static string GetConnectionString()
    {
        ConnectionStringSettings named = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
        if (named != null && !string.IsNullOrWhiteSpace(named.ConnectionString)) return named.ConnectionString;
        foreach (ConnectionStringSettings entry in ConfigurationManager.ConnectionStrings)
        {
            if (entry != null && !string.IsNullOrWhiteSpace(entry.ConnectionString) &&
                entry.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                return entry.ConnectionString;
        }
        return null;
    }

    private bool IsActiveInternalEmployee()
    {
        if (Context == null || Context.User == null || Context.User.Identity == null ||
            !Context.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(Context.User.Identity.Name)) return false;
        try
        {
            MembershipUser member = Membership.GetUser(Context.User.Identity.Name, false);
            if (member == null || !member.IsApproved || member.IsLockedOut) return false;
            Guid userId;
            if (!Guid.TryParse(Convert.ToString(member.ProviderUserKey), out userId) || userId == Guid.Empty) return false;
            string connString = GetConnectionString();
            if (string.IsNullOrWhiteSpace(connString)) return false;
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.aspnet_Users
WHERE UserId=@UserId AND UserName=@UserName
  AND ISNULL(IsDeleted,0)=0 AND IsActivated=1;", conn))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = member.UserName;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        catch (Exception ex)
        {
            // Fail closed: if identity verification cannot be completed, OTP is required.
            System.Diagnostics.Trace.TraceWarning("Không kiểm tra được tài khoản nội bộ xem báo cáo: " + ex);
            return false;
        }
    }

    private void SendReport(ProgressReportRecord record)
    {
        Response.StatusCode = 200;
        Response.Write(Json.Serialize(new { success = true, html = record.ReportHtml }));
    }
    private void SendError(int statusCode, string message)
    {
        Response.StatusCode = statusCode;
        Response.TrySkipIisCustomErrors = true;
        Response.Write(Json.Serialize(new { success = false, message = message }));
    }
    private static bool ConstantTimeEqual(string a, string b)
    {
        byte[] x = Encoding.UTF8.GetBytes(a ?? "");
        byte[] y = Encoding.UTF8.GetBytes(b ?? "");
        int diff = x.Length ^ y.Length;
        int n = Math.Max(x.Length, y.Length);
        for (int i = 0; i < n; i++) diff |= (i < x.Length ? x[i] : 0) ^ (i < y.Length ? y[i] : 0);
        return diff == 0;
    }
</script>
