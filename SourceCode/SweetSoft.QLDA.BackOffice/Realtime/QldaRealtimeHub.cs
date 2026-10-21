using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SweetSoft.QLDA.BackOffice.Realtime
{
    /// <summary>
    /// Nhom chi duoc xac dinh tai server. Client KHONG co ham joinGroup(userId).
    /// Id lay tu Membership provider key theo FormsAuthentication identity,
    /// khong lay tu querystring, cookie tu tao hay ASP.NET Session.
    /// </summary>
    internal static class QldaRealtimeIdentity
    {
        public static Guid ResolveUserId(IPrincipal principal)
        {
            if (principal == null || principal.Identity == null ||
                !principal.Identity.IsAuthenticated ||
                string.IsNullOrWhiteSpace(principal.Identity.Name))
                return Guid.Empty;

            try
            {
                // SqlMembershipProvider da duoc cai trong Web.config goc.
                var member = Membership.GetUser(principal.Identity.Name, false);
                if (member == null || !(member.ProviderUserKey is Guid)) return Guid.Empty;
                return (Guid)member.ProviderUserKey;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("QLDA Realtime: Membership lookup failed: {0}", ex.Message);
                return Guid.Empty;
            }
        }

        internal static string UserGroup(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Empty user id", "userId");
            return "qlda:user:" + userId.ToString("N");
        }
    }

    [HubName("qldaRealtime")]
    [Authorize]
    public sealed class QldaRealtimeHub : Hub
    {
        private Guid RequireUserId()
        {
            var id = QldaRealtimeIdentity.ResolveUserId(Context.User);
            if (id == Guid.Empty) throw new HubException("Unauthorized realtime connection.");
            return id;
        }

        public override Task OnConnected()
        {
            return Groups.Add(Context.ConnectionId, QldaRealtimeIdentity.UserGroup(RequireUserId()));
        }

        public override Task OnReconnected()
        {
            return Groups.Add(Context.ConnectionId, QldaRealtimeIdentity.UserGroup(RequireUserId()));
        }

        // Chi de kiem tra ket noi tren TEST; khong thay business API.
        [HubMethodName("ping")]
        public object Ping()
        {
            RequireUserId();
            return new { ok = true, serverUtc = DateTime.UtcNow.ToString("o") };
        }

        // TEST ONLY: phat "notifications-changed" toi CHINH tai khoan dang nhap.
        // Khong tao DB record, khong nhan UserId tu browser va khong gui email.
        [HubMethodName("testNotificationSelf")]
        public object TestNotificationSelf()
        {
            if (!QldaRealtimeSettings.NotificationsEnabled || !QldaRealtimeSettings.AllowSelfTest)
                throw new HubException("Realtime notification self-test is disabled.");
            Guid id = RequireUserId();
            QldaRealtimePublisher.NotificationsChanged(new[] { id });
            return new { ok = true };
        }

        // TEST ONLY: tu phat tin den cac TAB/THIET BI cua CHINH user dang nhap.
        // Khong cho phep truyen userId/duAn/taskId tu client de phat tin cho nguoi khac.
        [HubMethodName("testSelf")]
        public object TestSelf()
        {
            if (!QldaRealtimeSettings.AllowSelfTest)
                throw new HubException("Realtime self-test is disabled.");
            var id = RequireUserId();
            Clients.Group(QldaRealtimeIdentity.UserGroup(id)).changed(new
            {
                kind = "diagnostic-self-test",
                eventId = Guid.NewGuid().ToString("D"),
                occurredUtc = DateTime.UtcNow.ToString("o")
            });
            return new { ok = true };
        }
    }
}
