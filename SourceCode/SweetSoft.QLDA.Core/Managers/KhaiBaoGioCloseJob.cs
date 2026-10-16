using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Threading;

namespace SweetSoft.QLDA.Core.Managers
{
    public static class KhaiBaoGioCloseJob
    {
        private static readonly object _runLock = new object();

        public static void Run()
        {
            // Nếu lượt chạy trước chưa xong thì bỏ qua lượt này (thay cho DisableConcurrentExecution)
            if (!Monitor.TryEnter(_runLock)) return;

            try
            {
                KhaiBaoGioManager.Instance.CloseExpiredShifts(DateTime.Now);
            }
            catch (Exception ex)
            {
                // Ghi log và không ném lỗi ra ngoài, để Hangfire không retry dồn dập (thay cho AutomaticRetry)
                SysLogger.LogError(ex, "KhaiBaoGio: lỗi job chốt sổ cuối ngày");
            }
            finally
            {
                Monitor.Exit(_runLock);
            }
        }
    }
}