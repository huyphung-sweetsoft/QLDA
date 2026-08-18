using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public static class BusinessExceptionNotifierManager
    {
        private static readonly List<IExceptionNotifier> _notifiers = new List<IExceptionNotifier>();
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void InitializeFromConfig()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                var enableEmail = bool.TryParse(ConfigurationManager.AppSettings["EnableExceptionEmail"], out var eEmail) && eEmail;
                var enableFile = bool.TryParse(ConfigurationManager.AppSettings["EnableExceptionLogFile"], out var eFile) && eFile;

                if (enableEmail)
                {
                    _notifiers.Add(new EmailExceptionNotifier());
                }

                if (enableFile)
                {
                    _notifiers.Add(new FileExceptionNotifier());
                }

                _initialized = true;
            }
        }

        public static void Notify(BusinessException ex)
        {
            if (!_initialized)
            {
                InitializeFromConfig(); // fallback nếu chưa init
            }

            foreach (var notifier in _notifiers)
            {
                try
                {
                    notifier.Notify(ex);
                }
                catch (Exception innerEx)
                {
                    Debug.WriteLine($"[BusinessExceptionNotifier] Error in notifier: {innerEx}");
                }
            }
        }
    }
}
