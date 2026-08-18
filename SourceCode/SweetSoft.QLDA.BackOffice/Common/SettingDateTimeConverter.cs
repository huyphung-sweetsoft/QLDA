using SweetCMS.Controls.Helpers;
using SweetSoft.QLDA.Controls.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class SettingDateTimeConverter : IDateTimeConverter
    {
        private static readonly string _defaultTimeZoneId = "SE Asia Standard Time"; // Time zone for Vietnam   
        private readonly string _settingZone;

        public SettingDateTimeConverter(string settingZone)
        {
            _settingZone = settingZone;
        }

        public DateTime ConvertSettingTimeToUtc(DateTime settingTime)
        {
            if (settingTime == DateTime.MinValue || settingTime == DateTimeHelper.MinValueSQL)
                return settingTime;

            string timeZoneId = _defaultTimeZoneId;
            if (string.IsNullOrEmpty(_settingZone))
            {
                var systemConfiguration = SettingManager.Instance.GetSettingByName(SettingKeys.MyTimeZone);
                if (systemConfiguration != null)
                    timeZoneId = systemConfiguration.SettingValue;
            }
            else
                timeZoneId = _settingZone;


            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var local = DateTime.SpecifyKind(settingTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(local, tz);
        }

        public DateTime ConvertUTCToSettingTime(DateTime utcDateTime)
        {
            if (utcDateTime == DateTime.MinValue || utcDateTime == DateTimeHelper.MinValueSQL)
                return utcDateTime;

            string timeZoneId = _defaultTimeZoneId;
            if (string.IsNullOrEmpty(_settingZone))
            {
                var systemConfiguration = SettingManager.Instance.GetSettingByName(SettingKeys.MyTimeZone);
                if (systemConfiguration != null)
                    timeZoneId = systemConfiguration.SettingValue;
            }
            else
                timeZoneId = _settingZone;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }
    }

}