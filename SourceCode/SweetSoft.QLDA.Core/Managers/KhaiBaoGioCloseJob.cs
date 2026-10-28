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
                {
                    DateTime nowUtc = DateTime.UtcNow;
                    // Hai luot rieng: loi Task khong chan Issue va nguoc lai.
                    // Ca hai su dung cung app-lock du an va watermark SQL theo phien.
                    try
                    {
                        new KhaiBaoGio20Manager().KetToanPhienMo(nowUtc, 300);
                    }
                    catch (Exception ex)
                    {
                        SysLogger.LogError(ex, "KhaiBaoGio Hangfire: ket toan Task V2");
                    }
                    try
                    {
                        new GioXuLyVanDeManager().KetToanPhienMo(nowUtc, 300);
                    }
                    catch (Exception ex)
                    {
                        SysLogger.LogError(ex, "KhaiBaoGio Hangfire: ket toan Issue V2");
                    }
                }
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