//---------------------- PROGRAMMER LOG ---------------------------------------
//Change 01: Truong, 29 Oct 2024 - Update APIs
using System;
using System.Configuration;

namespace SweetSoft.QLDA.Core.Utils
{
    public static class AppSettingHelpers
    {
        public static string AppKey = "wvAyoMDuZ1dnfTHD7YGCP65WyVkenJaL";
        private static readonly AppSettingsReader settingsReader = new AppSettingsReader();
        public static bool GetSetting(string key, out Guid value) =>
            Guid.TryParse(GetSetting(key), out value);
        public static bool GetSetting(string key, out int value) =>
            int.TryParse(GetSetting(key), out value);

        public static bool GetSetting(string key, out long value) =>
            long.TryParse(GetSetting(key), out value);

        public static bool GetSetting(string key, out double value) =>
            double.TryParse(GetSetting(key), out value);

        public static bool GetSetting(string key, out bool value) =>
            bool.TryParse(GetSetting(key), out value);

        public static bool GetSetting(string key, out string value)
        {
            try
            {
                value = (string)settingsReader.GetValue(key, typeof(string));
                return true;
            }
            catch
            {
                value = string.Empty;
                return false;
            }
        }

        public static bool GetSetting(string key, out string[] values)
        {
            if (GetSetting(key, out string value))
            {
                values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }

            values = null;
            return false;
        }

        public static string GetSetting(string key)
        {
            return GetSetting(key, out string value) ? value : string.Empty;
        }

        public static T GetSetting<T>(string key)
        {
            try
            {
                var value = settingsReader.GetValue(key, typeof(string)).ToString();

                if (typeof(T) == typeof(Guid))
                    return (T)(object)Guid.Parse(value);
                //-------------------------------------
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(value);
                //-------------------------------------
                if (typeof(T) == typeof(long))
                    return (T)(object)long.Parse(value);
                //-------------------------------------
                if (typeof(T) == typeof(decimal))
                    return (T)(object)decimal.Parse(value);
                //-------------------------------------
                if (typeof(T) == typeof(bool))
                    return (T)(object)bool.Parse(value);
                //-------------------------------------
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting setting '{key}' to type {typeof(T)}: {ex.Message}");
                return default(T);
            }
        }


        public static bool GetSetting<T>(string key, out T value) where T : struct
        {
            if (typeof(T).IsEnum)
            {
                if (GetSetting(key, out string enumString) && Enum.TryParse(enumString, out T enumValue))
                {
                    value = enumValue;
                    return true;
                }
            }
            else
            {
                try
                {
                    value = (T)Convert.ChangeType(settingsReader.GetValue(key, typeof(T)), typeof(T));
                    return true;
                }
                catch
                {
                    value = default(T);
                    return false;
                }
            }

            value = default(T);
            return false;
        }
    }
}
