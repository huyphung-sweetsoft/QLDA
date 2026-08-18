using Newtonsoft.Json;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace SweetSoft.QLDA.Core.Helpers
{
    public static class FormatHelpers
    {
        #region Constants
        public static readonly List<string> RomanNumbers = new List<string>() { "--", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX" };
        public static readonly List<string> RomanNumerals = new List<string>() { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        public static readonly List<int> Numerals = new List<int>() { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        #endregion

        #region Decimal Formatting
        public static string ConvertDecimalToStringByLanguage(decimal value, string langCode = "en-US", bool isRounding = false, bool includeEndZero = false)
        {
            if (isRounding) value = RoundUp(value, 2);

            var culture = GetCulture(langCode);

            if (value == Math.Truncate(value))
                return includeEndZero ? value.ToString("N0", culture) + ".00" : value.ToString("N0", culture);

            return value.ToString("#,##0.00", culture);
        }

        public static string ConvertPercentToStringByLanguage(decimal value, string langCode = "en-US")
            => value.ToString("#,##0.##", GetCulture(langCode));
        #endregion

        #region Double Formatting
        public static string ConvertDoubleToStringByLanguage(object value, string langCode = "en-US")
        {
            if (value == null) return string.Empty;
            return ConvertDoubleToStringByLanguage(value.ToString(), langCode);
        }

        public static string ConvertDoubleToStringByLanguage(string value, string langCode = "en-US")
        {
            if (double.TryParse(value, out var result))
                return ConvertDoubleToStringByLanguage(result, langCode);
            return value;
        }

        public static string ConvertDoubleToStringByLanguage(double value, string langCode = "en-US")
        {
            var culture = GetCulture(langCode);
            return value == Math.Truncate(value)
                ? value.ToString("N0", culture)
                : value.ToString("#,##0.00", culture);
        }
        #endregion

        #region DateTime Formatting
        public static string ConvertDateTimeToStringTimeByLanguage(object dateTime, bool? hasTime, byte? langId = null)
        {
            if (dateTime == null) return string.Empty;

            if (DateTime.TryParse(dateTime.ToString(), out var dt))
            {
                if (dt == DateTime.MinValue || dt == DateTimeHelper.MinValueSQL)
                    return string.Empty;

                var dtFormat = DateTimeHelper.ConvertUTCToSettingTime(dt);
                return dtFormat.ToString("HH:mm tt");
            }
            return string.Empty;
        }
        public static string ConvertDateTimeToStringTimeByLanguage(DateTime dateTime, string myTimeZoneId = "")
        {
            if (dateTime == null) return string.Empty;

            if (DateTime.TryParse(dateTime.ToString(), out var dt))
            {
                if (dt == DateTime.MinValue || dt == DateTimeHelper.MinValueSQL)
                    return string.Empty;

                var dtFormat = DateTimeHelper.ConvertUTCToSettingTime(dt, myTimeZoneId);
                return dtFormat.ToString("HH:mm tt");
            }
            return string.Empty;
        }
        public static string ConvertDateTimeToStringByLanguage(object dateTime, bool? hasTime, byte? langId = null)
        {
            if (dateTime == null) return string.Empty;

            if (DateTime.TryParse(dateTime.ToString(), out var dt))
                return ConvertDateTimeToStringByLanguage(dt, hasTime, langId);

            return dateTime.ToString();
        }
        public static string ConvertDateTimeToStringByLanguage(DateTime dateTime, bool? hasTime, byte? langId = null)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTimeHelper.MinValueSQL)
                return string.Empty;

            var dt = DateTimeHelper.ConvertUTCToSettingTime(dateTime);
            var format = GetDateTimeFormat(false, hasTime, langId);
            return dt.ToString(format);
        }
        public static string ConvertDateTimeToStringByLanguageAndTimeZone(DateTime dateTime, bool? hasTime, byte? langId = null, string myTimeZoneId = "")
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTimeHelper.MinValueSQL)
                return string.Empty;

            var dt = DateTimeHelper.ConvertUTCToSettingTime(dateTime, myTimeZoneId);
            var format = GetDateTimeFormat(false, hasTime, langId);
            return dt.ToString(format);
        }
        public static string ConvertDateTimeToStringByLanguage(DateTime dateTime, bool? hasMinutes, bool? hasSeconds, byte? langId = null)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTimeHelper.MinValueSQL)
                return string.Empty;

            var dt = DateTimeHelper.ConvertUTCToSettingTime(dateTime);
            var format = GetDateTimeFormat(hasMinutes, hasSeconds, langId);
            return dt.ToString(format);
        }
        public static string ConvertDateTimeToStringByLanguageForEmailTemplateAdmin(DateTime dateTime, bool? hasSeconds, bool? hasMinutes, bool? hasHour, bool? hasDesignator, byte? langId)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTimeHelper.MinValueSQL)
                return string.Empty;

            var dt = DateTimeHelper.ConvertUTCToSettingTime(dateTime);
            string format = GetDateTimeFormatForEmailTemplate(hasSeconds, hasMinutes, hasHour, hasDesignator, langId);
            return dt.ToString(format);
        }

        private static string GetDateTimeFormat(bool? hasMinutes, bool? hasSeconds, byte? langId)
        {
            string baseFormat = langId == LanguageHelpers.English ? "dd MMM yyyy" : "dd/MM/yyyy";

            if (hasSeconds == true)
                return baseFormat += " HH:mm:ss tt";
            if (hasMinutes == true)
                return baseFormat += " HH:mm";
            return baseFormat;
        }

        private static string GetDateTimeFormatForEmailTemplate(bool? hasSeconds, bool? hasMinutes, bool? hasHour, bool? hasDesignator, byte? langId)
        {
            string baseFormat = langId == LanguageHelpers.English ? "dd MMM yyyy" : "dd/MM/yyyy";
            string hourFormat = hasDesignator == true ? "hh" : "HH";

            if (hasSeconds == true)
                baseFormat += $" 'at' {hourFormat}:mm:ss";
            else if (hasMinutes == true)
                baseFormat += $" 'at' {hourFormat}:mm";
            else if (hasHour == true)
                baseFormat += $" 'at' {hourFormat}";

            if (hasDesignator == true)
                baseFormat += " tt";

            return baseFormat;
        }
        #endregion

        #region Helpers
        public static string ToRomanNumeral(int number)
        {
            var result = new StringBuilder();
            for (int i = 0; i < Numerals.Count && number > 0; i++)
            {
                while (number >= Numerals[i])
                {
                    result.Append(RomanNumerals[i]);
                    number -= Numerals[i];
                }
            }
            return result.ToString();
        }

        public static string ConvertImageToBase64(string filePath)
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            string mimeType = MIMEAssistant.GetMimeType(imageBytes);
            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }

        public static decimal RoundUp(decimal value, int decimalPlaces)
        {
            var multiplier = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static bool IsInteger(decimal number) => number % 1 == 0;

        public static string GetLangCodeById(byte? langId)
        {
            if (langId == LanguageHelpers.English) return "en-US";
            return "vi-VN";
        }

        public static CultureInfo GetCulture(string langCode)
        {
            return new CultureInfo(langCode ?? "en-US");
        }

        public static string ConvertTimeSpanToString(TimeSpan timeSpan, bool hasSeconds = false)
        {
            DateTime dateTime = DateTime.Today.Add(timeSpan);
            return dateTime.ToString(hasSeconds ? @"hh\:mm\:ss" : @"hh\:mm");
        }
        #endregion
    }

}
