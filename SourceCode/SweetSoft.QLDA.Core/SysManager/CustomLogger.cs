using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.iMeeting.STCKH.Core.SysManager
{
    public class CustomLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                Logger.Info($"[{typeof(T).Name}] {message}");
            }
        }
    }
}
