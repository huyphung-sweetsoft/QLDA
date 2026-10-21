using System;
using System.Configuration;

namespace SweetSoft.QLDA.BackOffice.Realtime
{
    /// <summary>Switch moi, KHONG lien quan den feature flags KhaiBaoGio 2.0.</summary>
    public static class QldaRealtimeSettings
    {
        // Khong co appSetting => TAT an toan trong moi truong hien huu.
        public static bool Enabled { get { return IsTrue("QLDA.Realtime.Enabled"); } }

        // CHI bat tren TEST de kiem tra fan-out cua cung mot tai khoan.
        public static bool AllowSelfTest { get { return IsTrue("QLDA.Realtime.AllowSelfTest"); } }
        // Mo ket noi/realtime KBG20 rieng de co the rollback ma van giu Hub va Hangfire.
        // Mac dinh TAT neu khong khai bao trong Web.config.
        public static bool KhaiBaoGioEnabled { get { return Enabled && IsTrue("QLDA.Realtime.KhaiBaoGio"); } }

        // Dot 3: realtime chuong thong bao; mac dinh TAT, doc lap KhaiBaoGio.
        public static bool NotificationsEnabled { get { return Enabled && IsTrue("QLDA.Realtime.Notifications"); } }

        private static bool IsTrue(string key)
        {
            return string.Equals(ConfigurationManager.AppSettings[key], "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
