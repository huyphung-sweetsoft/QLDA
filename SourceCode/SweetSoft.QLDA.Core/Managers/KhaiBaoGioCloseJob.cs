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
                // Khong chay dong thoi hai job writer cho cung nhom nguoi dung.
                // Chi bat V2 khi da chuyen web + widget + DB sang 2.0.
                // Mac dinh = 1.0 de khong lam thay doi he thong hien tai.
                bool dungV2 = KhaiBaoGioManager.KhaiBaoGio20DaKichHoat();
                if (dungV2)
                    new KhaiBaoGio20Manager().KetToanPhienMo(DateTime.UtcNow, 300);
                else
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