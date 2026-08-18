using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public static class ErrorMailLimiter
    {
        private static readonly ConcurrentQueue<DateTime> _errorTimes = new ConcurrentQueue<DateTime>();
        private static readonly object _lock = new object();
        private const int MaxEmails = 10;
        private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(5);

        public static bool CanSendMail()
        {
            lock (_lock)
            {
                DateTime now = DateTime.UtcNow;

                // Loại bỏ các lỗi quá thời gian quy định
                while (_errorTimes.TryPeek(out DateTime time) && (now - time) > TimeWindow)
                {
                    _errorTimes.TryDequeue(out _);
                }

                if (_errorTimes.Count >= MaxEmails)
                {
                    return false; // Vượt quá giới hạn
                }

                _errorTimes.Enqueue(now);
                return true;
            }
        }
    }
}
