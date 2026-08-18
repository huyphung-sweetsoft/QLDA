using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public static class SyncProgressManager
    {
        private static readonly Dictionary<string, SyncProgress> _progressData = new Dictionary<string, SyncProgress>();
        private static readonly object _lock = new object();

        public static void Initialize(string moduleName)
        {
            lock (_lock)
            {
                if (!_progressData.ContainsKey(moduleName))
                {
                    _progressData[moduleName] = new SyncProgress();
                }
            }
        }
        public static void Running(string moduleName)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    var progress = _progressData[moduleName];
                    progress.IsRunning = true;
                }
            }
        }
        public static void Stop(string moduleName)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    var progress = _progressData[moduleName];
                    progress.IsRunning = false;
                }
            }
        }
        public static void Increase(string moduleName)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    var progress = _progressData[moduleName];
                    progress.ProcessedRecords += 1;
                }
            }
        }
        public static void UpdateProgress(string moduleName, int processedRecords, int totalRecords)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    var progress = _progressData[moduleName];
                    progress.ProcessedRecords = processedRecords;
                    progress.TotalRecords = totalRecords;
                }
            }
        }

        public static void LogError(string moduleName, string error)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    var progress = _progressData[moduleName];
                    progress.ErrorMessage += error + "\n";
                }
            }
        }

        public static SyncProgress GetProgress(string moduleName)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    return _progressData[moduleName];
                }

                return null;
            }
        }

        public static void ClearProgress(string moduleName)
        {
            lock (_lock)
            {
                if (_progressData.ContainsKey(moduleName))
                {
                    _progressData.Remove(moduleName);
                }
            }
        }
    }

    public class SyncProgress
    {
        public bool IsRunning { get; set; } = false;
        public int TotalRecords { get; set; } = 0;
        public int ProcessedRecords { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
    }

}
