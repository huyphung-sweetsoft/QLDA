<%@ Page Language="C#" EnableViewState="false" EnableSessionState="false" ValidateRequest="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Caching" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
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
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) { SendError(405, "Phương thức yêu cầu không hợp lệ."); return; }
        try
        {
            if (Request.ContentLength <= 0 || Request.ContentLength > 2048) { SendError(400, "Yêu cầu không hợp lệ."); return; }
            string body;
            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8)) body = reader.ReadToEnd();
            OpenRequest data = Json.Deserialize<OpenRequest>(body);
            if (data == null || string.IsNullOrWhiteSpace(data.reportId) || !Regex.IsMatch(data.reportId, "^[a-fA-F0-9]{32}$") || string.IsNullOrEmpty(data.otp) || !Regex.IsMatch(data.otp, "^[0-9]{6}$")) { SendError(400, "Mã báo cáo hoặc OTP không hợp lệ."); return; }
            string key = "gantt:otp:" + data.reportId.ToLowerInvariant() + ":" + (Request.UserHostAddress ?? "unknown");
            lock (AttemptLock)
            {
                FailCount counter = HttpRuntime.Cache[key] as FailCount;
                if (counter != null && counter.Value >= 8) { SendError(429, "Bạn đã thử quá nhiều lần. Vui lòng thử lại sau 15 phút."); return; }
            }
            string path = Server.MapPath("~/App_Data/ProgressReports/" + data.reportId.ToLowerInvariant() + ".json");
            if (!File.Exists(path)) { SendError(404, "Báo cáo không tồn tại hoặc đã bị xóa."); return; }
            var record = Json.Deserialize<ProgressReportRecord>(File.ReadAllText(path, Encoding.UTF8));
            if (record == null || string.IsNullOrWhiteSpace(record.OtpSalt) || string.IsNullOrWhiteSpace(record.OtpHash) || string.IsNullOrWhiteSpace(record.ReportHtml)) { SendError(404, "Dữ liệu báo cáo không hợp lệ."); return; }
            string computedHash;
            using (var derive = new Rfc2898DeriveBytes(data.otp, Convert.FromBase64String(record.OtpSalt), 100000)) computedHash = Convert.ToBase64String(derive.GetBytes(32));
            if (!ConstantTimeEqual(computedHash, record.OtpHash))
            {
                lock (AttemptLock)
                {
                    FailCount counter = HttpRuntime.Cache[key] as FailCount;
                    if (counter == null) { counter = new FailCount(); HttpRuntime.Cache.Insert(key, counter, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration); }
                    counter.Value++;
                }
                SendError(403, "OTP không chính xác. Vui lòng kiểm tra lại.");
                return;
            }
            lock (AttemptLock) HttpRuntime.Cache.Remove(key);
            Response.StatusCode = 200;
            Response.Write(Json.Serialize(new { success = true, html = record.ReportHtml }));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Lỗi xác thực báo cáo: " + ex);
            SendError(500, "Không thể xác thực lúc này. Vui lòng thử lại.");
        }
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
