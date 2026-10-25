<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        // This page is deliberately independent of MasterTemplate, SweetContext,
        // CURRENT_PAGE and project permissions. It must work even if auth fails.
        bool loginExpired = string.Equals(Request.QueryString["code"], "401", StringComparison.Ordinal);
        Response.TrySkipIisCustomErrors = true;
        Response.SuppressFormsAuthenticationRedirect = true;
        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.StatusCode = loginExpired ? 401 : 403;
    }
</script>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Quyền truy cập – SweetSoft QLDA</title>
    <style>
        * { box-sizing:border-box; }
        body { margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;padding:24px;background:#f8f6fc;color:#362a45;font-family:Arial,"Segoe UI",sans-serif; }
        .kbg-error-card { width:min(100%,540px);padding:42px 36px;background:#fff;border:1px solid #e7dff0;border-radius:16px;text-align:center;box-shadow:0 10px 30px rgba(48,25,81,.06); }
        .kbg-error-code { display:inline-block;margin-bottom:12px;padding:6px 13px;background:#f5edfc;color:#5b218e;border-radius:8px;font-size:15px;font-weight:700; }
        .kbg-error-card h1 { margin:0 0 12px;font-size:23px;line-height:1.35;color:#4b2077; }
        .kbg-error-card p { margin:0 0 23px;color:#66616f;font-size:14px;line-height:1.7; }
        .kbg-error-actions { display:flex;flex-wrap:wrap;justify-content:center;gap:10px; }
        .kbg-error-actions a { display:inline-flex;align-items:center;justify-content:center;min-height:38px;padding:9px 18px;background:#5c2495;color:#fff;text-decoration:none;border-radius:8px;font-size:13px;font-weight:600; }
        .kbg-error-actions a:hover { background:#491776; }
        .kbg-error-note { display:block;margin-top:22px;color:#96909d;font-size:12px; }
    </style>
</head>
<body>
    <main class="kbg-error-card" role="main">
        <% if (Request.QueryString["code"] == "401") { %>
            <span class="kbg-error-code">401</span>
            <h1>Phiên đăng nhập không còn hợp lệ</h1>
            <p>Vui lòng đăng nhập lại để tiếp tục sử dụng tính giờ công việc.</p>
        <% } else { %>
            <span class="kbg-error-code">403</span>
            <h1>Không có quyền truy cập</h1>
            <p>Tài khoản hiện tại không được cấp quyền xem dữ liệu giờ công của dự án này. Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ người quản trị.</p>
        <% } %>
        <div class="kbg-error-actions"><a href="<%= ResolveUrl("~/") %>">Về trang chủ</a></div>
        <small class="kbg-error-note">SweetSoft QLDA</small>
    </main>
</body>
</html>
