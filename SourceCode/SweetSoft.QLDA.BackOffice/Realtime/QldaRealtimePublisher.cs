using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using Microsoft.AspNet.SignalR;

namespace SweetSoft.QLDA.BackOffice.Realtime
{
    /// <summary>
    /// Chi duoc GOI TU CODE SERVER sau khi SQL transaction COMMIT.
    /// Publisher chi gui su kien "du lieu da thay doi"; KHONG ghi SQL
    /// va khong bao gio duoc su dung de quyet dinh so giay gio cong.
    /// </summary>
    public static class QldaRealtimePublisher
    {
        // Dung trong dot tich hop KhaiBaoGio SAP TOI, chua duoc goi trong dot 1.
        public static void TaskTimersStopped(Guid projectId, Guid taskId, IEnumerable<Guid> recipients)
        {
            Send("task-timers-stopped", recipients, projectId, taskId);
        }

        public static void TimeTrackingChanged(Guid projectId, Guid taskId, IEnumerable<Guid> recipients)
        {
            Send("time-tracking-changed", recipients, projectId, taskId);
        }

        // Dung sau nay cho chuong thong bao; CHUA hook ThongBaoManager.
        public static void NotificationsChanged(IEnumerable<Guid> recipients)
        {
            Send("notifications-changed", recipients, null, null);
        }

        private static void Send(string kind, IEnumerable<Guid> recipients, Guid? projectId, Guid? taskId)
        {
            if (!QldaRealtimeSettings.Enabled) return;
            if (Transaction.Current != null)
                throw new InvalidOperationException("SignalR publish phai nam SAU khi SQL transaction da commit.");
            if (recipients == null) return;
            var ids = recipients.Where(x => x != Guid.Empty).Distinct().ToArray();
            if (ids.Length == 0) return;

            var message = new
            {
                kind = kind,
                eventId = Guid.NewGuid().ToString("D"),
                projectId = projectId.HasValue ? projectId.Value.ToString("D") : null,
                taskId = taskId.HasValue ? taskId.Value.ToString("D") : null,
                occurredUtc = DateTime.UtcNow.ToString("o")
            };
            try
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<QldaRealtimeHub>();
                foreach (var id in ids)
                {
                    try { hub.Clients.Group(QldaRealtimeIdentity.UserGroup(id)).changed(message); }
                    catch (Exception ex)
                    {
                        Trace.TraceError("QLDA Realtime publish to user failed: {0}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                // Gui SignalR la best effort; nguoi dung se fetch lai qua API khi reconnect/poll.
                Trace.TraceError("QLDA Realtime: Hub context unavailable: {0}", ex);
            }
        }
    }
}
