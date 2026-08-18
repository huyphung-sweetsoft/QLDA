using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SweetSoft.QLDA.Controls.Helpers
{
    public static class DateTimeHelper
    {
        public static string ClientTimeZone
        {
            get
            {
                return string.Empty;
                //string timeZone = SettingManager.GetSettingValue(string.Format("{0}_{1}", SettingNames.MyTimeZone, SweetContext.Current.CMSUserID));
                //if (string.IsNullOrEmpty(timeZone))
                //    timeZone = SettingManager.GetSettingValue(SettingNames.MyTimeZone);
                ////if (Session["ClientTimeZone"] != null)
                ////    return Session["ClientTimeZone"] as string;
                //return timeZone;
            }
            //set
            //{
            //    Session["ClientTimeZone"] = value;
            //}
        }
        public static DateTime MinValueSQL = DateTime.Parse("1900-01-01 00:00:00.000");
        /// <summary>
        /// Common DateTime Methods.
        /// </summary>
        /// 
        public static CultureInfo ciFormatDisplay = CultureInfo.CreateSpecificCulture("en-US");

        /// <summary>
        /// Add the MinDateTimeValue is 01/01/1945
        /// </summary>
        /// <returns></returns>
        public static DateTime MinDateTimeValue = new DateTime(1975, 1, 1);
        public static DateTime MaxDateTimeValue = DateTime.MaxValue;

        public enum Quarter
        {
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4
        }

        public enum DateTimeComparison
        {
            Year,
            Month,
            Day,
            Hour,
            Minute,
            Second
        }

        public enum Month
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        #region Quarters

        public static DateTime FirstDayOfQuarter(DateTime day)
        {
            return new DateTime(day.Year, FirstMonthOfQuarter(GetQuarter(day)), 1);
        }

        public static int FirstMonthOfQuarter(int qtr)
        {
            if (qtr == 1)
            {
                return 1;
            }

            if (qtr == 2)
            {
                return 4;
            }

            if (qtr == 3)
            {
                return 7;
            }

            if (qtr == 4)
            {
                return 10;
            }

            return 0;
        }

        public static DateTime GetStartOfQuarter(int Year, Quarter Qtr)
        {
            if (Qtr == Quarter.First)    // 1st Quarter = January 1 to March 31
                return new DateTime(Year, 1, 1, 0, 0, 0, 0);
            else if (Qtr == Quarter.Second) // 2nd Quarter = April 1 to June 30
                return new DateTime(Year, 4, 1, 0, 0, 0, 0);
            else if (Qtr == Quarter.Third) // 3rd Quarter = July 1 to September 30
                return new DateTime(Year, 7, 1, 0, 0, 0, 0);
            else // 4th Quarter = October 1 to December 31
                return new DateTime(Year, 10, 1, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfLastQuarter()
        {
            if ((Month)DateTime.UtcNow.Month <= Month.March)
                //go to last quarter of previous year
                return GetStartOfQuarter(DateTime.UtcNow.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetStartOfQuarter(DateTime.UtcNow.Year,
                  GetQuarter((Month)DateTime.UtcNow.Month) - 1);
        }

        public static DateTime GetStartOfCurrentQuarter()
        {
            return GetStartOfQuarter(DateTime.UtcNow.Year,
                   GetQuarter((Month)DateTime.UtcNow.Month));
        }

        public static Quarter GetQuarter(Month Month)
        {
            if (Month <= Month.March)
                // 1st Quarter = January 1 to March 31
                return Quarter.First;
            else if ((Month >= Month.April) && (Month <= Month.June))
                // 2nd Quarter = April 1 to June 30
                return Quarter.Second;
            else if ((Month >= Month.July) && (Month <= Month.September))
                // 3rd Quarter = July 1 to September 30
                return Quarter.Third;
            else // 4th Quarter = October 1 to December 31
                return Quarter.Fourth;
        }

        public static int GetQuarter(DateTime theDate)
        {
            var i = theDate.Month;
            if (i <= 3)
            {
                return 1;
            }
            else if ((i >= 4) && (i <= 6))
            {
                return 2;
            }
            else if ((i >= 7) && (i <= 9))
            {
                return 3;
            }
            else if ((i >= 10) && (i <= 12))
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        public static string QuarterAndYearStamp(int quarter, int year)
        {
            return string.Format("Quarter (0) of the year (1)", quarter, year);
        }

        public static DateTime GetEndOfQuarter(int Year, Quarter Qtr)
        {
            if (Qtr == Quarter.First)    // 1st Quarter = January 1 to March 31
                return new DateTime(Year, 3,
                       DateTime.DaysInMonth(Year, 3), 23, 59, 59, 999);
            else if (Qtr == Quarter.Second) // 2nd Quarter = April 1 to June 30
                return new DateTime(Year, 6,
                       DateTime.DaysInMonth(Year, 6), 23, 59, 59, 999);
            else if (Qtr == Quarter.Third) // 3rd Quarter = July 1 to September 30
                return new DateTime(Year, 9,
                       DateTime.DaysInMonth(Year, 9), 23, 59, 59, 999);
            else // 4th Quarter = October 1 to December 31
                return new DateTime(Year, 12,
                       DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
        }

        public static DateTime GetEndOfCurrentQuarter()
        {
            return GetEndOfQuarter(DateTime.UtcNow.Year,
                   GetQuarter((Month)DateTime.UtcNow.Month));
        }

        public static DateTime GetEndOfLastQuarter()
        {
            if ((Month)DateTime.UtcNow.Month <= Month.March)
                //go to last quarter of previous year
                return GetEndOfQuarter(DateTime.UtcNow.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetEndOfQuarter(DateTime.UtcNow.Year,
                  GetQuarter((Month)DateTime.UtcNow.Month) - 1);
        }

        #endregion

        #region Weeks

        public static DateTime AddWeeks(DateTime d, double weeks)
        {
            return d.AddDays(weeks * 7);
        }

        /*
        public static DateTime GetStartOfCurrentWeek()
        {
            return GetStartOfWeek(DateTime.UtcNow);
        }
        */

        public static DateTime GetStartOfCurrentWeek()
        {
            int DaysToSubtract = (int)DateTime.UtcNow.DayOfWeek;
            DateTime dt =
              DateTime.UtcNow.Subtract(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        /*
        public static DateTime StartOfWeek(DateTime d)
        {
            return d.AddDays(0 - (int)d.DayOfWeek).Date;
        }

        public static DateTime GetStartOfLastWeek()
        {
            var DaysToSubtract = Convert.ToInt32(DateTime.UtcNow.DayOfWeek) + 7;
            var dt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        */
        public static DateTime GetStartOfLastWeek()
        {
            int DaysToSubtract = (int)DateTime.UtcNow.DayOfWeek + 7;
            DateTime dt = DateTime.UtcNow.Subtract(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfCurrentWorkWeek()
        {
            var dt = GetStartOfCurrentWeek().AddDays(1);
            return dt;
        }

        public static DateTime GetStartOfWeek(DateTime dayInWeek)
        {
            var DaysToSubtract = Convert.ToInt32(dayInWeek.DayOfWeek);
            DateTime dt = dayInWeek.Subtract(TimeSpan.FromDays(DaysToSubtract));
            return GetStartOfDay(dt);
        }

        public static DateTime GetStartOfWorkWeek(DateTime dayInWeek)
        {
            return GetStartOfWeek(dayInWeek).AddDays(1);
        }

        public static IEnumerable<DateTime> GetWeekends(int year)
        {
            DateTime fromDate = new DateTime(year, 1, 1);
            DateTime toDate = new DateTime(year, 12, 31);
            return GetWeekends(fromDate, toDate);
        }

        public static IEnumerable<DateTime> GetWeekends(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.DayOfWeek == DayOfWeek.Sunday)
            {
                yield return fromDate; // Add end of weekend to this result
                fromDate = GetNextDateForDay(fromDate, DayOfWeek.Saturday);
            }
            else if (fromDate.DayOfWeek != DayOfWeek.Saturday)
                fromDate = GetNextDateForDay(fromDate, DayOfWeek.Saturday);
            TimeSpan ts = toDate - fromDate; // Days from current weekend date to EOY
            int daysToAdd = ts.Days;
            for (int i = 0; i <= daysToAdd; i += 7)
            {
                yield return fromDate.AddDays(i);
                if (i + 1 < daysToAdd)
                    yield return fromDate.AddDays(i + 1);
            }
        }

        public static DateTime GetEndOfCurrentWorkWeek()
        {
            var dt = GetEndOfCurrentWeek().AddDays(-1);
            return dt;
        }

        /*
        public static DateTime EndOfWeek(DateTime d)
        {
            return d.AddDays(6 - ((int)d.DayOfWeek)).Date;
        }
        */
        public static DateTime GetEndOfWeek(DateTime dayInWeek)
        {
            return GetEndOfDay(GetStartOfWeek(dayInWeek).AddDays(6));
        }

        public static DateTime GetEndOfWorkWeek(DateTime dayInWeek)
        {
            return GetEndOfWeek(dayInWeek).AddDays(-1);
        }

        public static DateTime GetEndOfLastWeek()
        {
            DateTime dt = GetStartOfLastWeek().AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        public static DateTime GetEndOfCurrentWeek()
        {
            var dt = GetStartOfCurrentWeek().AddDays(6);
            return GetEndOfDay(dt);
        }
        /*
        public static DateTime GetEndOfCurrentWeek()
        {
            DateTime dt = GetStartOfCurrentWeek().AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }
        */
        #endregion

        #region Months

        public static DateTime GetStartOfMonth(int Month, int Year)
        {
            return new DateTime(Year, Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfLastMonth()
        {
            if (DateTime.UtcNow.Month == 1)
            {
                return GetStartOfMonth(12, DateTime.UtcNow.Year - 1);
            }
            else
            {
                return GetStartOfMonth(DateTime.UtcNow.Month - 1, DateTime.UtcNow.Year);
            }
        }

        public static DateTime GetStartOfCurrentMonth(DateTime? dt)
        {
            if (!dt.HasValue)
                dt = DateTime.UtcNow;
            return GetStartOfMonth(dt.Value.Month, dt.Value.Year);
        }

        public static DateTime StartOfMonth(DateTime d)
        {
            return new DateTime(d.Year, d.Month, 1).Date;
        }

        public static DateTime EndOfMonth(DateTime d)
        {
            return new DateTime(d.Year, d.Month, CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(d.Year, d.Month)).Date;
        }

        public static DateTime GetEndOfMonth(int Month, int Year)
        {
            if (Month < 1)
            {
                throw new ArgumentException("Month < 1", "Month");
            }

            if (Month > 12)
            {
                throw new ArgumentException("Month > 12", "Month");
            }

            if (Year < 1)
            {
                throw new ArgumentException("Year < 1", "Year");
            }

            if (Year > 9999)
            {
                throw new ArgumentException("Year > 12", "Year");
            }

            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 59, 999);
        }

        /// <summary>
        /// This method will take 2 dates as input and return the Generic List of DateTime objects. Each
        /// object represent 1 of the months between the 2 dates and is set in the dd/MM/yyyy
        /// format while
        /// the current day of the month is set to 1. For example: 01/01/2010, 01/02/2010,
        /// 01/03/2010 etc..
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<DateTime> GetMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            // Đảm bảo startDate luôn nhỏ hơn hoặc bằng endDate
            if (startDate > endDate)
            {
                DateTime temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // Tính số lượng tháng bằng cách lấy tổng số năm * 12 và cộng thêm số tháng từ startDate đến tháng cuối cùng của endDate
            int numberOfYears = endDate.Year - startDate.Year;
            int numberOfMonths = numberOfYears * 12 + (endDate.Month - startDate.Month);

            if (numberOfMonths > 0)
            {
                List<DateTime> result = new List<DateTime>();
                for (int i = 0; i < numberOfMonths; i++)
                {
                    DateTime tempDate = startDate.AddMonths(i);
                    DateTime dateToAdd = new DateTime(tempDate.Date.Year, tempDate.Month, 1);

                    result.Add(dateToAdd);
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        public static int GetMonthNumberBetweenDates(DateTime startDate, DateTime endDate)
        {
            // Đảm bảo startDate luôn nhỏ hơn hoặc bằng endDate
            if (startDate > endDate)
            {
                DateTime temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // Tính số lượng tháng bằng cách lấy tổng số năm * 12 và cộng thêm số tháng từ startDate đến tháng cuối cùng của endDate
            int numberOfYears = endDate.Year - startDate.Year;
            int numberOfMonths = numberOfYears * 12 + (endDate.Month - startDate.Month);

            if (numberOfMonths > 0)
            {
                return numberOfMonths;
            }
            else
            {
                return 0;
            }
        }

        public static string GetMonthName(int month)
        {
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            return dtfi.GetMonthName(month);
        }

        public static DateTime GetStartOfMonth(Month Month, int Year)
        {
            return new DateTime(Year, (int)Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfMonth(Month Month, int Year)
        {
            return new DateTime(Year, (int)Month,
               DateTime.DaysInMonth(Year, (int)Month), 23, 59, 59, 999);
        }

        public static DateTime GetEndOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999);
        }
        public static DateTime GetEndOfLastMonth()
        {
            if (DateTime.UtcNow.Month == 1)
                return GetEndOfMonth(12, DateTime.UtcNow.Year - 1);
            else
                return GetEndOfMonth(DateTime.UtcNow.Month - 1, DateTime.UtcNow.Year);
        }

        public static DateTime GetEndOfCurrentMonth(DateTime? dt)
        {
            if (!dt.HasValue)
                dt = DateTime.UtcNow;
            return GetEndOfMonth(dt.Value.Month, dt.Value.Year);
        }

        #endregion

        #region Days

        public static bool IsDate(string dateAsString)
        {
            var pointlessDateTime = new DateTime();
            return IsDate(dateAsString, ref pointlessDateTime);
        }

        public static bool IsDate(string dateString, ref DateTime validDate)
        {
            var isValid = true;
            try
            {
                string[] allowedFormats = new[] { DateFormat, "dd/MM/yyyy", "dd/MM/yyyy HH:mm", "MM/dd/yyyy", "MM/dd/yyyy HH:mm" };
                var culture = new System.Globalization.CultureInfo(SweetContext.Current.CurrentLanguageCode);

                isValid = DateTime.TryParseExact(
                    dateString, allowedFormats, culture,
                    System.Globalization.DateTimeStyles.None, out validDate
                );
                return isValid;
            }
            catch (Exception)
            {
                isValid = false;
                validDate = DateTime.UtcNow;
            }

            return isValid;
        }

        public static DateTime GetEndOfDay(DateTime theDate)
        {
            return new DateTime(theDate.Year, theDate.Month, theDate.Day, 23, 59, 59, 999);
        }

        public static DateTime GetStartOfDay(DateTime theDate)
        {
            return new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0, 0);
        }

        /// <summary>
        ///     Compares the two given DateTimes based on the given
        ///     comparison.
        /// 
        ///     The comparison also checks each "higher level" property.
        ///     For example if you are using <code>DateTimeComparison.Month</code>
        ///     the year will also be compared.
        /// </summary>
        public static bool IsSame(DateTime a, DateTime b, DateTimeComparison comparison)
        {

            if (a.Year != b.Year) return false;
            if (comparison == DateTimeComparison.Year) return true;

            if (a.Month != b.Month) return false;
            if (comparison == DateTimeComparison.Month) return true;

            if (a.Day != b.Day) return false;
            if (comparison == DateTimeComparison.Day) return true;

            if (a.Hour != b.Hour) return false;
            if (comparison == DateTimeComparison.Hour) return true;

            if (a.Minute != b.Minute) return false;
            if (comparison == DateTimeComparison.Minute) return true;

            return a.Second == b.Second;
        }

        /// <summary>
        ///     Compares the given <see cref="DateTime"/> to the current date and
        ///		time based on the given comparison.
        /// 
        ///     The comparison also checks each "higher level" property.
        ///     For example if you are using <code>DateTimeComparison.Month</code>
        ///     the year will also be compared.
        /// </summary>
        public static bool IsCurrent(DateTime a, DateTimeComparison comparison)
        {
            return IsSame(a, DateTime.UtcNow, comparison);
        }

        /// <summary>
        ///     Determines whether the given DateTime represents
        ///     the day after the current date (using <c>DateTime.Today</c>).
        /// </summary>
        public static bool IsTomorrow(DateTime a)
        {
            return IsSame(a, DateTime.Today.AddDays(1.0), DateTimeComparison.Day);
        }

        /// <summary>
        ///     Determines whether the given DateTime represents
        ///     the day before the current date (using <c>DateTime.Today</c>).
        /// </summary>
        public static bool IsYesterday(DateTime a)
        {
            return IsSame(a, DateTime.Today.AddDays(-1.0), DateTimeComparison.Day);
        }

        /// <summary>
        ///     Returns true if the current <see cref="DateTime"/> represents
        ///     a point in time that occurs BEFORE the point in time represented
        ///     by <paramref name="b"/>.
        /// 
        ///     The comparison also checks each "higher level" property.
        ///     For example if you are using <code>DateTimeComparison.Month</code>
        ///     the year will also be compared.
        /// </summary>
        public static bool IsBefore(DateTime a, DateTime b, DateTimeComparison comparison)
        {

            if (a.Year < b.Year) return true;
            if (a.Year > b.Year || comparison == DateTimeComparison.Year) return false;

            if (a.Month < b.Month) return true;
            if (a.Month > b.Month || comparison == DateTimeComparison.Month) return false;

            if (a.Day < b.Day) return true;
            if (a.Day > b.Day || comparison == DateTimeComparison.Day) return false;

            if (a.Hour < b.Hour) return true;
            if (a.Hour > b.Hour || comparison == DateTimeComparison.Hour) return false;

            if (a.Minute < b.Minute) return true;
            if (a.Minute > b.Minute || comparison == DateTimeComparison.Minute) return false;

            return a.Second < b.Second;
        }

        /// <summary>
        ///     Returns true if the current <see cref="DateTime"/> represents
        ///     a point in time that occurs AFTER the point in time represented
        ///     by <paramref name="b"/>.
        /// 
        ///     The comparison also checks each "higher level" property.
        ///     For example if you are using <code>DateTimeComparison.Month</code>
        ///     the year will also be compared.
        /// </summary>
        public static bool IsAfter(DateTime a, DateTime b, DateTimeComparison comparison)
        {

            if (a.Year > b.Year) return true;
            if (a.Year < b.Year || comparison == DateTimeComparison.Year) return false;

            if (a.Month > b.Month) return true;
            if (a.Month < b.Month || comparison == DateTimeComparison.Month) return false;

            if (a.Day > b.Day) return true;
            if (a.Day < b.Day || comparison == DateTimeComparison.Day) return false;

            if (a.Hour > b.Hour) return true;
            if (a.Hour < b.Hour || comparison == DateTimeComparison.Hour) return false;

            if (a.Minute > b.Minute) return true;
            if (a.Minute < b.Minute || comparison == DateTimeComparison.Minute) return false;

            return a.Second > b.Second;
        }

        public static long ConvertDateTimeToUnixTimeSecond(int day, int month, int year, int hour, int minutes, int seconds)
        {
            DateTime dt = new DateTime(year, month, day, hour, minutes, seconds);
            long timeStamp = new DateTimeOffset(dt).ToUnixTimeSeconds();
            return timeStamp;
        }

        public static DateTime ConvertTimeStampToDateTime(object unixTimeStamp)
        {
            if (unixTimeStamp == null)
                return DateTime.MinValue;
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(double.Parse(unixTimeStamp.ToString())).ToLocalTime();
            return dateTime;
        }

        public static bool IsBetween(DateTime a, DateTime startDate, DateTime endDate, DateTimeComparison comparison)
        {

            double startDiff = (a - startDate).TotalSeconds;
            double endDiff = (endDate - a).TotalSeconds;

            if (comparison == DateTimeComparison.Second)
            {
                return 0 <= startDiff && 0 <= endDiff;
            }

            // Minute
            startDiff = Math.Round(startDiff / 60);
            endDiff = Math.Round(endDiff / 60);

            if (comparison == DateTimeComparison.Minute)
            {
                return 0 <= startDiff && 0 <= endDiff;
            }

            // Hour
            startDiff = Math.Round(startDiff / 60);
            endDiff = Math.Round(endDiff / 60);

            if (comparison == DateTimeComparison.Hour)
            {
                return 0 <= startDiff && 0 <= endDiff;
            }

            // Day
            startDiff = Math.Round(startDiff / 24);
            endDiff = Math.Round(endDiff / 24);

            if (comparison == DateTimeComparison.Day)
            {
                return 0 <= startDiff && 0 <= endDiff;
            }

            throw new NotSupportedException("Cannot diff DateTimeComparison.Month or DateTimeComparison.Year");
        }

        /// <summary>
        /// Next occurrence of dayofweek after today
        /// </summary>
        public static DateTime NextDay(DateTime start, DayOfWeek dayOfWeek)
        {
            if (start.DayOfWeek > dayOfWeek)
                return start.AddDays(-(int)start.DayOfWeek + 7 + (int)dayOfWeek);
            else
                return start.AddDays(-(int)start.DayOfWeek + (int)dayOfWeek);
        }

        /// <summary>
        /// Previous occurrence of dayofweek before today
        /// </summary>
        public static DateTime PreviousDay(DateTime start, DayOfWeek dayOfWeek)
        {
            if (start.DayOfWeek < dayOfWeek)
                return start.AddDays(-(int)start.DayOfWeek - 7 + (int)dayOfWeek);
            else
                return start.AddDays(-(int)start.DayOfWeek + (int)dayOfWeek);
        }

        #endregion

        #region Years

        public static DateTime GetEndOfCurrentYear()
        {
            return GetEndOfYear(DateTime.UtcNow.Year);
        }

        public static DateTime GetEndOfLastYear()
        {
            return GetEndOfYear(DateTime.UtcNow.Year - 1);
        }

        public static DateTime GetEndOfYear(int Year)
        {
            return new DateTime(Year, 12, DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
        }

        public static DateTime GetStartOfYear(int Year)
        {
            return new DateTime(Year, 1, 1, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfCurrentYear()
        {
            return GetStartOfYear(DateTime.UtcNow.Year);
        }

        public static DateTime GetStartOfLastYear()
        {
            return GetStartOfYear(DateTime.UtcNow.Year - 1);
        }

        #endregion


        /// <summary>
        /// Add the last time into Date, ex: 23:59:59 12/06/2009
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime AddLastTimeInDate(DateTime date)
        {
            return DateTime.Parse(string.Format("{0} 23:59:59", date.ToShortDateString()));
        }

        /// <summary>
        /// Add the begin time into Date, ex: 00:00:00 12/06/2009
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime AddBeginTimeInDate(DateTime date)
        {
            return DateTime.Parse(string.Format("{0} 00:00:00", date.ToShortDateString()));
        }


        #region Converters

        public static string ToLongDateTimeString(string dateText)
        {
            return ToString(dateText, "dd MMMM yyyy HH:mm");
        }

        public static string ToLongDateTimeString(object date)
        {
            if (date != null)
            {
                string data = string.Format("{0:dd/MM/yyyy hh:mm:ss tt}", date);
                return ToLongDateTimeString(data);
            }
            else
                return string.Empty;
        }

        public static string ToLongDateTimeString(DateTime date)
        {
            return ToString(date, "dd MMMM yyyy HH:mm");
        }

        public static string ToDateTimeString(string dateText)
        {
            return ToString(dateText, "dd/MM/yyyy hh:mm:ss tt");
        }

        public static string ToDateTimeString(object date)
        {
            if (date != null)
            {
                string data = string.Format("{0:dd/MM/yyyy hh:mm:ss tt}", date);
                return ToDateTimeString(data);
            }
            else
                return string.Empty;
        }

        public static string ToDateTimeString(DateTime date)
        {
            return ToString(date, "dd/MM/yyyy hh:mm:ss tt");
        }

        public static string ToString(object date)
        {
            if (date != null)
            {
                string data = string.Format("{0:dd/MM/yyyy hh:mm:ss tt}", date);
                return ToString(data);
            }
            else
                return string.Empty;
        }

        public static string ToString(string dateText)
        {
            return ToString(dateText, "dd MMMM yyyy");
        }

        public static string ToString(DateTime date)
        {
            return ToString(date, "dd MMMM yyyy");
        }

        public static string ToString(string dateText, string format)
        {
            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(dateText, ciFormatDisplay, DateTimeStyles.None, out dt);
            if (dt != DateTime.MinValue)
                return ToString(dt, format);
            else
                return string.Empty;
        }

        public static string ToString(object date, string format)
        {
            if (date != null)
            {
                string data = string.Format("{0:dd/MM/yyyy hh:mm:ss tt}", date);
                return ToString(data, format);
            }
            else
                return string.Empty;
        }

        public static string ToString(DateTime date, string format)
        {
            return date.ToString(format, ciFormatDisplay);
        }

        public static string Tommorow()
        {
            return DateTime.UtcNow.AddDays(1).ToShortDateString();
        }

        public static string Monday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Monday).ToShortDateString();
        }

        public static string Tuesday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Tuesday).ToShortDateString();
        }

        public static string Wednesday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Wednesday).ToShortDateString();
        }

        public static string Thursday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Thursday).ToShortDateString();
        }

        public static string Friday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Friday).ToShortDateString();
        }

        public static string Saturday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Saturday).ToShortDateString();
        }

        public static string Sunday()
        {
            return GetNextDateForDay(DateTime.UtcNow, DayOfWeek.Sunday).ToShortDateString();
        }

        /// <summary>
        /// Finds the next date whose day of the week equals the specified day of the week.
        /// </summary>
        /// <param name="startDate">
        ///		The date to begin the search.
        /// </param>
        /// <param name="desiredDay">
        ///		The desired day of the week whose date will be returneed.
        /// </param>
        /// <returns>
        ///		The returned date occurs on the given date's week.
        ///		If the given day occurs before given date, the date for the
        ///		following week's desired day is returned.
        /// </returns>
        public static DateTime GetNextDateForDay(DateTime startDate, DayOfWeek desiredDay)
        {
            // Given a date and day of week,
            // find the next date whose day of the week equals the specified day of the week.
            return startDate.AddDays(DaysToAdd(startDate.DayOfWeek, desiredDay));
        }

        /// <summary>
        /// Calculates the number of days to add to the given day of
        /// the week in order to return the next occurrence of the
        /// desired day of the week.
        /// 
        /// https://angstrey.com/index.php/2009/04/25/finding-the-next-date-for-day-of-week/
        /// </summary>
        /// <param name="current">
        ///		The starting day of the week.
        /// </param>
        /// <param name="desired">
        ///		The desired day of the week.
        /// </param>
        /// <returns>
        ///		The number of days to add to <var>current</var> day of week
        ///		in order to achieve the next <var>desired</var> day of week.
        /// </returns>
        public static int DaysToAdd(DayOfWeek current, DayOfWeek desired)
        {
            int c = (int)current;
            int d = (int)desired;
            int n = (7 - c + d);

            return (n > 7) ? n % 7 : n;
        }
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        #endregion

        #region Parse
        public static string DateFormat
        {
            get
            {
                if (LanguageHelpers.CurrentLanguageId == LanguageHelpers.English)
                    return "MM/dd/yyyy";
                else
                    return "dd/MM/yyyy";
            }
        }

        public static DateTime ParseDate(object dateObject, string format, string languageCode)
        {
            CultureInfo ciLang = null;
            if (string.IsNullOrEmpty(languageCode) == false)
            {
                try
                {
                    ciLang = CultureInfo.GetCultureInfo(languageCode);
                }
                catch
                {

                }
            }

            return ParseDate(dateObject, format, ciLang);
        }

        public static DateTime ParseDate(object dateObject, string format, CultureInfo ciLang)
        {
            return ParseDate(Convert.ToString(dateObject), format, ciLang);
        }

        public static DateTime ParseDate(string date, string format, string languageCode)
        {
            CultureInfo ciLang = null;
            if (string.IsNullOrEmpty(languageCode) == false)
            {
                try
                {
                    ciLang = CultureInfo.GetCultureInfo(languageCode);
                }
                catch
                {

                }
            }
            return ParseDate(date, format, ciLang);
        }

        public static string ConvertDateTime(object dateTime, bool hasTime, bool convertUTCToSettingTime = false)
        {
            DateTime dtDateTime = DateTime.MinValue;
            if (dateTime == null)
                return string.Empty;
            try
            {
                dtDateTime = DateTime.Parse(dateTime.ToString());
            }
            catch { }
            if (convertUTCToSettingTime)
                dtDateTime = ConvertUTCToSettingTime(dtDateTime);
            else
                dtDateTime = ConvertSettingTimeToUtc(dtDateTime);
            if (dtDateTime != null && dtDateTime != DateTime.MinValue && dtDateTime != MinValueSQL)
            {
                if (hasTime)
                    return dtDateTime.ToString($"{DateFormat} HH:mm:ss");
                else
                    return dtDateTime.ToString(DateFormat);
            }
            return string.Empty;
        }

        public static DateTime ParseDate(string date, string format, CultureInfo ciLang)
        {
            DateTime dt = DateTime.MinValue;
            if (DateTime.TryParseExact(date, format, ciLang, DateTimeStyles.None, out dt) == false)
            {
                try
                {
                    dt = DateTime.ParseExact(date, format, ciLang);
                }
                catch
                {
                    dt = DateTime.MinValue;
                }
            }

            return dt;
        }

        public static DateTime ParseDate(string date, string languageCode)
        {
            CultureInfo ciLang = null;
            if (string.IsNullOrEmpty(languageCode) == false)
            {
                try
                {
                    ciLang = CultureInfo.GetCultureInfo(languageCode);
                }
                catch
                {

                }
            }

            return ParseDate(date, ciLang);
        }

        public static DateTime ParseDate(object dateObject, string languageCode)
        {
            CultureInfo ciLang = null;
            if (string.IsNullOrEmpty(languageCode) == false)
            {
                try
                {
                    ciLang = CultureInfo.GetCultureInfo(languageCode);
                }
                catch
                {

                }
            }

            if (ciLang != null)
                return ParseDate(Convert.ToString(dateObject), ciLang);
            else
                return DateTime.MinValue;
        }

        public static DateTime ParseDate(object dateObject, CultureInfo ciLang)
        {
            return ParseDate(Convert.ToString(dateObject), ciLang);
        }

        public static DateTime ParseDate(string date, CultureInfo ciLang)
        {
            DateTime dt = DateTime.MinValue;
            if (DateTime.TryParse(date, ciLang, DateTimeStyles.None, out dt) == false)
            {
                try
                {
                    dt = DateTime.Parse(date, ciLang);
                }
                catch
                {
                    dt = DateTime.MinValue;
                }
            }

            return dt;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion

        #region RegionName

        /// <summary>
        /// Turn a datetime into something speakable like "half past two"
        /// </summary>
        public static string SpokenTime(DateTimeOffset now)
        {
            return SpokenTime(now.LocalDateTime);
        }

        /// <summary>
        /// Turn a datetime into something speakable like "half past two"
        /// </summary>
        public static string SpokenTime(DateTime now)
        {
            int hour = now.Hour;
            if (hour > 12) hour -= 12;
            if (hour == 0) hour = 12;

            int minute = now.Minute;

            int closesthour = now.Hour;
            if (minute > 30) closesthour++;
            if (closesthour > 12) closesthour -= 12;

            string hourstring = hour.ToString(); if (now.Hour == 0) hourstring = "midnight";
            if (now.Hour == 12) hourstring = "midday";

            string timenow;
            if (minute < 2) timenow = string.Format("{0} o'clock", hour);
            else if (minute < 14) timenow = string.Format("{0} minutes past {1}", minute, hour);
            else if (minute < 17) timenow = string.Format("quarter past {0}", hour);
            else if (minute < 29) timenow = string.Format("{0} {1}", hour, minute);
            else if (minute < 32) timenow = string.Format("half past {0}", hour);
            else if (minute < 44) timenow = string.Format("{0} {1}", hour, minute);
            else if (minute < 47) timenow = string.Format("quarter to {0}", closesthour);
            else if (minute < 59) timenow = string.Format("{0} minutes to {1}", 60 - minute, closesthour);
            else timenow = string.Format("{0} o'clock", closesthour);

            return timenow;
        }


        /// <summary>
        /// Good afternoon, morning etc.
        /// </summary>
        public static string GreetingTime(DateTimeOffset now)
        {
            return GreetingTime(now.LocalDateTime);
        }

        /// <summary>
        /// Good afternoon, goor morning, good evening
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public static string GreetingTime(DateTime now)
        {
            string greeting = "";
            if ((now.TimeOfDay > new TimeSpan(5, 0, 0)) && (now.TimeOfDay < new TimeSpan(9, 0, 0)))
            {
                greeting = "Good morning";
            }

            if ((now.TimeOfDay > new TimeSpan(13, 0, 0)) && (now.TimeOfDay < new TimeSpan(18, 0, 0)))
                greeting = "Good afternoon";

            if ((now.TimeOfDay > new TimeSpan(18, 0, 0)) && (now.TimeOfDay < new TimeSpan(22, 0, 0)))
                greeting = "Good evening";

            return greeting;
        }

        public static string TimeAgo(this DateTime dt)
        {
            TimeSpan span = DateTime.UtcNow - dt;
            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format("{0} năm trước", years);
            }
            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                return String.Format("{0} tháng trước", months);
            }
            if (span.Days > 0)
                return String.Format("{0} ngày trước", span.Days);
            if (span.Hours > 0)
                return String.Format("{0} giờ trước", span.Hours);
            if (span.Minutes > 0)
                return String.Format("{0} phút trước", span.Minutes);
            if (span.Seconds > 5)
                return String.Format("{0} giây trước", span.Seconds);
            if (span.Seconds <= 5)
                return "vừa xong";
            return string.Empty;
        }

        /// <summary>
        /// Format a timespan the way a human would say it
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string EnglishTime(TimeSpan ts)
        {
            if (ts.TotalMinutes < 2.0) return string.Format("{0}s", (int)ts.TotalSeconds);
            if (ts.TotalMinutes < 5) return string.Format("{0}min {1}s", (int)ts.TotalMinutes, (int)ts.Seconds);
            if (ts.TotalHours < 1.5) return string.Format("{0}min", (int)ts.TotalMinutes, (int)ts.Seconds);
            if (ts.TotalDays < 2.0) return string.Format("{0}h {1}min", (int)ts.TotalHours, ts.Minutes);
            if (ts.TotalDays < 5.0) return string.Format("{0}d {0}h", (int)ts.TotalDays, ts.Hours);
            return string.Format("{0} days", (int)ts.TotalDays);
        }

        public static string EnglishAgo(TimeSpan ts)
        {
            return EnglishTime(ts) + " ago";
        }

        /// <summary>
        /// Format a time the way a human would say it, e.g. "10 minutes to 4"
        /// </summary>
        public static string SpeakableTime(DateTimeOffset now)
        {
            return SpeakableTime(now.LocalDateTime);
        }

        /// <summary>
        /// Format a time the way a human would say it, e.g. "10 minutes to 4"
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string SpeakableTime(DateTime now)
        {
            int hour = now.Hour;
            if (hour > 12) hour -= 12;
            if (hour == 0) hour = 12;

            int minute = now.Minute;

            int closesthour = now.Hour;
            if (minute > 30) closesthour++;
            if (closesthour > 12) closesthour -= 12;

            string hourstring = hour.ToString(); if (now.Hour == 0) hourstring = "midnight";
            if (now.Hour == 12) hourstring = "midday";

            string timenow;
            if (minute < 2) timenow = string.Format("{0} o'clock", hour);
            else if (minute < 14) timenow = string.Format("{0} minutes past {1}", minute, hour);
            else if (minute < 17) timenow = string.Format("quarter past {0}", hour);
            else if (minute < 29) timenow = string.Format("{0} {1}", hour, minute);
            else if (minute < 32) timenow = string.Format("half past {0}", hour);
            else if (minute < 44) timenow = string.Format("{0} {1}", hour, minute);
            else if (minute < 47) timenow = string.Format("quarter to {0}", closesthour);
            else if (minute < 59) timenow = string.Format("{0} minutes to {1}", 60 - minute, closesthour);
            else timenow = string.Format("{0} o'clock", closesthour);

            return timenow;
        }

        public static string GetReadableDate(DateTime date)
        {
            return string.Format("{0:dddd, d MMMM yyyy}", date);
        }

        public static string GetReadableTimeSince(DateTime originalTime)
        {
            TimeSpan ts = DateTime.UtcNow - originalTime;

            string message = "";

            if (ts.TotalSeconds < 60)
            {
                message = "less than a minute ago";
            }
            else if (ts.TotalMinutes < 3)
            {
                message = "a few minutes ago";
            }
            else if (ts.TotalMinutes < 60)
            {
                message = "less than an hour ago";
            }
            else if (ts.TotalDays < 1 && originalTime.Day == DateTime.UtcNow.Day)
            {
                message = "today";
            }
            else if ((ts.TotalDays < 1 && originalTime.Day < DateTime.UtcNow.Day) ||
                     (ts.TotalDays < 2 && originalTime.Day == DateTime.UtcNow.Day - 1))
            {
                message = "yesterday";
            }
            else if (ts.TotalDays < 7)
            {
                message = "less than a week ago";
            }
            else if (ts.TotalDays < 26)
            {
                message = string.Format("{0} days ago", (int)ts.TotalDays);
            }
            else if (ts.TotalDays >= 26 && ts.TotalDays <= 56)
            {
                message = "about a month ago";
            }
            else if (ts.TotalDays < 365)
            {
                int months = (int)(ts.TotalDays / 28);
                message = string.Format("about {0} months ago", months);
            }
            else
            {
                message = "more than a year ago";
            }

            return message;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves a System.TimeZoneInfo object from the registry based on its identifier.
        /// </summary>
        /// <param name="id">The time zone identifier, which corresponds to the System.TimeZoneInfo.Id property.</param>
        /// <returns>A System.TimeZoneInfo object whose identifier is the value of the id parameter.</returns>
        public static TimeZoneInfo FindTimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        /// <summary>
        ///     Converts the given <paramref name="date"/>
        ///     to a JSON compliant string.
        /// </summary>
        public static string ToJson(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        /// <summary>
        ///     Converts the given <paramref name="date"/>
        ///     to a JSON compliant string.
        /// </summary>
        public static string ToJson(DateTime? date)
        {
            return date.HasValue && date.Value != default(DateTime) ? ToJson(date.Value) : String.Empty;
        }

        public static string GetShortDateOrEmptyString(DateTime? nullableDateTime)
        {
            return nullableDateTime.HasValue ? nullableDateTime.Value.ToShortDateString() : string.Empty;
        }

        public static string MonthAndYearStamp(int month, int year)
        {
            return string.Format("(0)/(1)", month, year);
        }

        public static DateTime? ParseNullableDateTimeString(string dateString)
        {
            DateTime? result = null;
            if (!String.IsNullOrEmpty(dateString))
            {
                DateTime tempDate;

                if (DateTime.TryParse(dateString, out tempDate))
                {
                    result = tempDate;
                }
            }

            return result;
        }

        #endregion

        public static DateTimeOffset Combine(DateTimeOffset? originalDate, string timeString, int timeZoneOffsetInMinutes)
        {
            if (originalDate.HasValue)
            {
                return FromString(GetDateAsString(originalDate.Value.Date, timeString, timeZoneOffsetInMinutes));
            }
            else
            {
                throw new InvalidOperationException("original must have a value");
            }
        }

        public static DateTime Combine(DateTime originalDate, string timeString, int timeZoneOffsetInMinutes)
        {
            return FromString(GetDateAsString(originalDate, timeString, timeZoneOffsetInMinutes)).DateTime; ;
        }

        private static DateTimeOffset FromString(string dateToParse)
        {
            return DateTimeOffset.ParseExact(dateToParse, "dd/MM/yyyy h:mm tt zzz", CultureInfo.InvariantCulture);
        }

        private static string GetDateAsString(DateTime dateComponent, string timeComponent, int offsetInMinutes)
        {
            var timeSpan = TimeSpan.FromMinutes(offsetInMinutes * -1);
            var offsetString = string.Format("{0:+00;-00}{1:00}", timeSpan.Hours, timeSpan.Minutes);
            return string.Format("{0} {1} {2}", dateComponent.ToString("dd/MM/yyyy"), timeComponent, offsetString);
        }

        public static DateTime GetDateTimeFromJavascript(long jsSeconds)
        {
            return new DateTime((jsSeconds * TimeSpan.TicksPerMillisecond) + 621355968000000000, DateTimeKind.Utc);

            //DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return unixEpoch.AddSeconds(jsSeconds);
        }


        public static List<string> AllTimezone()
        {
            return new List<string>() {
    "Africa/Abidjan",
    "Africa/Accra",
    "Africa/Addis_Ababa",
    "Africa/Algiers",
    "Africa/Asmara",
    "Africa/Asmera",
    "Africa/Bamako",
    "Africa/Bangui",
    "Africa/Banjul",
    "Africa/Bissau",
    "Africa/Blantyre",
    "Africa/Brazzaville",
    "Africa/Bujumbura",
    "Africa/Cairo",
    "Africa/Casablanca",
    "Africa/Ceuta",
    "Africa/Conakry",
    "Africa/Dakar",
    "Africa/Dar_es_Salaam",
    "Africa/Djibouti",
    "Africa/Douala",
    "Africa/El_Aaiun",
    "Africa/Freetown",
    "Africa/Gaborone",
    "Africa/Harare",
    "Africa/Johannesburg",
    "Africa/Juba",
    "Africa/Kampala",
    "Africa/Khartoum",
    "Africa/Kigali",
    "Africa/Kinshasa",
    "Africa/Lagos",
    "Africa/Libreville",
    "Africa/Lome",
    "Africa/Luanda",
    "Africa/Lubumbashi",
    "Africa/Lusaka",
    "Africa/Malabo",
    "Africa/Maputo",
    "Africa/Maseru",
    "Africa/Mbabane",
    "Africa/Mogadishu",
    "Africa/Monrovia",
    "Africa/Nairobi",
    "Africa/Ndjamena",
    "Africa/Niamey",
    "Africa/Nouakchott",
    "Africa/Ouagadougou",
    "Africa/Porto-Novo",
    "Africa/Sao_Tome",
    "Africa/Timbuktu",
    "Africa/Tripoli",
    "Africa/Tunis",
    "Africa/Windhoek",
    "America/Adak",
    "America/Anchorage",
    "America/Anguilla",
    "America/Antigua",
    "America/Araguaina",
    "America/Argentina/Buenos_Aires",
    "America/Argentina/Catamarca",
    "America/Argentina/ComodRivadavia",
    "America/Argentina/Cordoba",
    "America/Argentina/Jujuy",
    "America/Argentina/La_Rioja",
    "America/Argentina/Mendoza",
    "America/Argentina/Rio_Gallegos",
    "America/Argentina/Salta",
    "America/Argentina/San_Juan",
    "America/Argentina/San_Luis",
    "America/Argentina/Tucuman",
    "America/Argentina/Ushuaia",
    "America/Aruba",
    "America/Asuncion",
    "America/Atikokan",
    "America/Atka",
    "America/Bahia",
    "America/Bahia_Banderas",
    "America/Barbados",
    "America/Belem",
    "America/Belize",
    "America/Blanc-Sablon",
    "America/Boa_Vista",
    "America/Bogota",
    "America/Boise",
    "America/Buenos_Aires",
    "America/Cambridge_Bay",
    "America/Campo_Grande",
    "America/Cancun",
    "America/Caracas",
    "America/Catamarca",
    "America/Cayenne",
    "America/Cayman",
    "America/Chicago",
    "America/Chihuahua",
    "America/Coral_Harbour",
    "America/Cordoba",
    "America/Costa_Rica",
    "America/Creston",
    "America/Cuiaba",
    "America/Curacao",
    "America/Danmarkshavn",
    "America/Dawson",
    "America/Dawson_Creek",
    "America/Denver",
    "America/Detroit",
    "America/Dominica",
    "America/Edmonton",
    "America/Eirunepe",
    "America/El_Salvador",
    "America/Ensenada",
    "America/Fort_Nelson",
    "America/Fort_Wayne",
    "America/Fortaleza",
    "America/Glace_Bay",
    "America/Godthab",
    "America/Goose_Bay",
    "America/Grand_Turk",
    "America/Grenada",
    "America/Guadeloupe",
    "America/Guatemala",
    "America/Guayaquil",
    "America/Guyana",
    "America/Halifax",
    "America/Havana",
    "America/Hermosillo",
    "America/Indiana/Indianapolis",
    "America/Indiana/Knox",
    "America/Indiana/Marengo",
    "America/Indiana/Petersburg",
    "America/Indiana/Tell_City",
    "America/Indiana/Vevay",
    "America/Indiana/Vincennes",
    "America/Indiana/Winamac",
    "America/Indianapolis",
    "America/Inuvik",
    "America/Iqaluit",
    "America/Jamaica",
    "America/Jujuy",
    "America/Juneau",
    "America/Kentucky/Louisville",
    "America/Kentucky/Monticello",
    "America/Knox_IN",
    "America/Kralendijk",
    "America/La_Paz",
    "America/Lima",
    "America/Los_Angeles",
    "America/Louisville",
    "America/Lower_Princes",
    "America/Maceio",
    "America/Managua",
    "America/Manaus",
    "America/Marigot",
    "America/Martinique",
    "America/Matamoros",
    "America/Mazatlan",
    "America/Mendoza",
    "America/Menominee",
    "America/Merida",
    "America/Metlakatla",
    "America/Mexico_City",
    "America/Miquelon",
    "America/Moncton",
    "America/Monterrey",
    "America/Montevideo",
    "America/Montreal",
    "America/Montserrat",
    "America/Nassau",
    "America/New_York",
    "America/Nipigon",
    "America/Nome",
    "America/Noronha",
    "America/North_Dakota/Beulah",
    "America/North_Dakota/Center",
    "America/North_Dakota/New_Salem",
    "America/Ojinaga",
    "America/Panama",
    "America/Pangnirtung",
    "America/Paramaribo",
    "America/Phoenix",
    "America/Port-au-Prince",
    "America/Port_of_Spain",
    "America/Porto_Acre",
    "America/Porto_Velho",
    "America/Puerto_Rico",
    "America/Rainy_River",
    "America/Rankin_Inlet",
    "America/Recife",
    "America/Regina",
    "America/Resolute",
    "America/Rio_Branco",
    "America/Rosario",
    "America/Santa_Isabel",
    "America/Santarem",
    "America/Santiago",
    "America/Santo_Domingo",
    "America/Sao_Paulo",
    "America/Scoresbysund",
    "America/Shiprock",
    "America/Sitka",
    "America/St_Barthelemy",
    "America/St_Johns",
    "America/St_Kitts",
    "America/St_Lucia",
    "America/St_Thomas",
    "America/St_Vincent",
    "America/Swift_Current",
    "America/Tegucigalpa",
    "America/Thule",
    "America/Thunder_Bay",
    "America/Tijuana",
    "America/Toronto",
    "America/Tortola",
    "America/Vancouver",
    "America/Virgin",
    "America/Whitehorse",
    "America/Winnipeg",
    "America/Yakutat",
    "America/Yellowknife",
    "Antarctica/Casey",
    "Antarctica/Davis",
    "Antarctica/DumontDUrville",
    "Antarctica/Macquarie",
    "Antarctica/Mawson",
    "Antarctica/McMurdo",
    "Antarctica/Palmer",
    "Antarctica/Rothera",
    "Antarctica/South_Pole",
    "Antarctica/Syowa",
    "Antarctica/Troll",
    "Antarctica/Vostok",
    "Arctic/Longyearbyen",
    "Asia/Aden",
    "Asia/Almaty",
    "Asia/Amman",
    "Asia/Anadyr",
    "Asia/Aqtau",
    "Asia/Aqtobe",
    "Asia/Ashgabat",
    "Asia/Ashkhabad",
    "Asia/Baghdad",
    "Asia/Bahrain",
    "Asia/Baku",
    "Asia/Bangkok",
    "Asia/Barnaul",
    "Asia/Beirut",
    "Asia/Bishkek",
    "Asia/Brunei",
    "Asia/Calcutta",
    "Asia/Chita",
    "Asia/Choibalsan",
    "Asia/Chongqing",
    "Asia/Chungking",
    "Asia/Colombo",
    "Asia/Dacca",
    "Asia/Damascus",
    "Asia/Dhaka",
    "Asia/Dili",
    "Asia/Dubai",
    "Asia/Dushanbe",
    "Asia/Gaza",
    "Asia/Harbin",
    "Asia/Hebron",
    "Asia/Ho_Chi_Minh",
    "Asia/Hong_Kong",
    "Asia/Hovd",
    "Asia/Irkutsk",
    "Asia/Istanbul",
    "Asia/Jakarta",
    "Asia/Jayapura",
    "Asia/Jerusalem",
    "Asia/Kabul",
    "Asia/Kamchatka",
    "Asia/Karachi",
    "Asia/Kashgar",
    "Asia/Kathmandu",
    "Asia/Katmandu",
    "Asia/Khandyga",
    "Asia/Kolkata",
    "Asia/Krasnoyarsk",
    "Asia/Kuala_Lumpur",
    "Asia/Kuching",
    "Asia/Kuwait",
    "Asia/Macao",
    "Asia/Macau",
    "Asia/Magadan",
    "Asia/Makassar",
    "Asia/Manila",
    "Asia/Muscat",
    "Asia/Nicosia",
    "Asia/Novokuznetsk",
    "Asia/Novosibirsk",
    "Asia/Omsk",
    "Asia/Oral",
    "Asia/Phnom_Penh",
    "Asia/Pontianak",
    "Asia/Pyongyang",
    "Asia/Qatar",
    "Asia/Qyzylorda",
    "Asia/Rangoon",
    "Asia/Riyadh",
    "Asia/Saigon",
    "Asia/Sakhalin",
    "Asia/Samarkand",
    "Asia/Seoul",
    "Asia/Shanghai",
    "Asia/Singapore",
    "Asia/Srednekolymsk",
    "Asia/Taipei",
    "Asia/Tashkent",
    "Asia/Tbilisi",
    "Asia/Tehran",
    "Asia/Tel_Aviv",
    "Asia/Thimbu",
    "Asia/Thimphu",
    "Asia/Tokyo",
    "Asia/Tomsk",
    "Asia/Ujung_Pandang",
    "Asia/Ulaanbaatar",
    "Asia/Ulan_Bator",
    "Asia/Urumqi",
    "Asia/Ust-Nera",
    "Asia/Vientiane",
    "Asia/Vladivostok",
    "Asia/Yakutsk",
    "Asia/Yekaterinburg",
    "Asia/Yerevan",
    "Atlantic/Azores",
    "Atlantic/Bermuda",
    "Atlantic/Canary",
    "Atlantic/Cape_Verde",
    "Atlantic/Faeroe",
    "Atlantic/Faroe",
    "Atlantic/Jan_Mayen",
    "Atlantic/Madeira",
    "Atlantic/Reykjavik",
    "Atlantic/South_Georgia",
    "Atlantic/St_Helena",
    "Atlantic/Stanley",
    "Australia/ACT",
    "Australia/Adelaide",
    "Australia/Brisbane",
    "Australia/Broken_Hill",
    "Australia/Canberra",
    "Australia/Currie",
    "Australia/Darwin",
    "Australia/Eucla",
    "Australia/Hobart",
    "Australia/LHI",
    "Australia/Lindeman",
    "Australia/Lord_Howe",
    "Australia/Melbourne",
    "Australia/NSW",
    "Australia/North",
    "Australia/Perth",
    "Australia/Queensland",
    "Australia/South",
    "Australia/Sydney",
    "Australia/Tasmania",
    "Australia/Victoria",
    "Australia/West",
    "Australia/Yancowinna",
    "Brazil/Acre",
    "Brazil/DeNoronha",
    "Brazil/East",
    "Brazil/West",
    "CET",
    "CST6CDT",
    "Canada/Atlantic",
    "Canada/Central",
    "Canada/East-Saskatchewan",
    "Canada/Eastern",
    "Canada/Mountain",
    "Canada/Newfoundland",
    "Canada/Pacific",
    "Canada/Saskatchewan",
    "Canada/Yukon",
    "Chile/Continental",
    "Chile/EasterIsland",
    "Cuba",
    "EET",
    "EST",
    "EST5EDT",
    "Egypt",
    "Eire",
    "Etc/GMT",
    "Etc/GMT+0",
    "Etc/GMT+1",
    "Etc/GMT+10",
    "Etc/GMT+11",
    "Etc/GMT+12",
    "Etc/GMT+2",
    "Etc/GMT+3",
    "Etc/GMT+4",
    "Etc/GMT+5",
    "Etc/GMT+6",
    "Etc/GMT+7",
    "Etc/GMT+8",
    "Etc/GMT+9",
    "Etc/GMT-0",
    "Etc/GMT-1",
    "Etc/GMT-10",
    "Etc/GMT-11",
    "Etc/GMT-12",
    "Etc/GMT-13",
    "Etc/GMT-14",
    "Etc/GMT-2",
    "Etc/GMT-3",
    "Etc/GMT-4",
    "Etc/GMT-5",
    "Etc/GMT-6",
    "Etc/GMT-7",
    "Etc/GMT-8",
    "Etc/GMT-9",
    "Etc/GMT0",
    "Etc/Greenwich",
    "Etc/UCT",
    "Etc/UTC",
    "Etc/Universal",
    "Etc/Zulu",
    "Europe/Amsterdam",
    "Europe/Andorra",
    "Europe/Astrakhan",
    "Europe/Athens",
    "Europe/Belfast",
    "Europe/Belgrade",
    "Europe/Berlin",
    "Europe/Bratislava",
    "Europe/Brussels",
    "Europe/Bucharest",
    "Europe/Budapest",
    "Europe/Busingen",
    "Europe/Chisinau",
    "Europe/Copenhagen",
    "Europe/Dublin",
    "Europe/Gibraltar",
    "Europe/Guernsey",
    "Europe/Helsinki",
    "Europe/Isle_of_Man",
    "Europe/Istanbul",
    "Europe/Jersey",
    "Europe/Kaliningrad",
    "Europe/Kiev",
    "Europe/Kirov",
    "Europe/Lisbon",
    "Europe/Ljubljana",
    "Europe/London",
    "Europe/Luxembourg",
    "Europe/Madrid",
    "Europe/Malta",
    "Europe/Mariehamn",
    "Europe/Minsk",
    "Europe/Monaco",
    "Europe/Moscow",
    "Europe/Nicosia",
    "Europe/Oslo",
    "Europe/Paris",
    "Europe/Podgorica",
    "Europe/Prague",
    "Europe/Riga",
    "Europe/Rome",
    "Europe/Samara",
    "Europe/San_Marino",
    "Europe/Sarajevo",
    "Europe/Simferopol",
    "Europe/Skopje",
    "Europe/Sofia",
    "Europe/Stockholm",
    "Europe/Tallinn",
    "Europe/Tirane",
    "Europe/Tiraspol",
    "Europe/Ulyanovsk",
    "Europe/Uzhgorod",
    "Europe/Vaduz",
    "Europe/Vatican",
    "Europe/Vienna",
    "Europe/Vilnius",
    "Europe/Volgograd",
    "Europe/Warsaw",
    "Europe/Zagreb",
    "Europe/Zaporozhye",
    "Europe/Zurich",
    "GB",
    "GB-Eire",
    "GMT",
    "GMT+0",
    "GMT-0",
    "GMT0",
    "Greenwich",
    "HST",
    "Hongkong",
    "Iceland",
    "Indian/Antananarivo",
    "Indian/Chagos",
    "Indian/Christmas",
    "Indian/Cocos",
    "Indian/Comoro",
    "Indian/Kerguelen",
    "Indian/Mahe",
    "Indian/Maldives",
    "Indian/Mauritius",
    "Indian/Mayotte",
    "Indian/Reunion",
    "Iran",
    "Israel",
    "Jamaica",
    "Japan",
    "Kwajalein",
    "Libya",
    "MET",
    "MST",
    "MST7MDT",
    "Mexico/BajaNorte",
    "Mexico/BajaSur",
    "Mexico/General",
    "NZ",
    "NZ-CHAT",
    "Navajo",
    "PRC",
    "PST8PDT",
    "Pacific/Apia",
    "Pacific/Auckland",
    "Pacific/Bougainville",
    "Pacific/Chatham",
    "Pacific/Chuuk",
    "Pacific/Easter",
    "Pacific/Efate",
    "Pacific/Enderbury",
    "Pacific/Fakaofo",
    "Pacific/Fiji",
    "Pacific/Funafuti",
    "Pacific/Galapagos",
    "Pacific/Gambier",
    "Pacific/Guadalcanal",
    "Pacific/Guam",
    "Pacific/Honolulu",
    "Pacific/Johnston",
    "Pacific/Kiritimati",
    "Pacific/Kosrae",
    "Pacific/Kwajalein",
    "Pacific/Majuro",
    "Pacific/Marquesas",
    "Pacific/Midway",
    "Pacific/Nauru",
    "Pacific/Niue",
    "Pacific/Norfolk",
    "Pacific/Noumea",
    "Pacific/Pago_Pago",
    "Pacific/Palau",
    "Pacific/Pitcairn",
    "Pacific/Pohnpei",
    "Pacific/Ponape",
    "Pacific/Port_Moresby",
    "Pacific/Rarotonga",
    "Pacific/Saipan",
    "Pacific/Samoa",
    "Pacific/Tahiti",
    "Pacific/Tarawa",
    "Pacific/Tongatapu",
    "Pacific/Truk",
    "Pacific/Wake",
    "Pacific/Wallis",
    "Pacific/Yap",
    "Poland",
    "Portugal",
    "ROC",
    "ROK",
    "Singapore",
    "Turkey",
    "UCT",
    "US/Alaska",
    "US/Aleutian",
    "US/Arizona",
    "US/Central",
    "US/East-Indiana",
    "US/Eastern",
    "US/Hawaii",
    "US/Indiana-Starke",
    "US/Michigan",
    "US/Mountain",
    "US/Pacific",
    "US/Pacific-New",
    "US/Samoa",
    "UTC",
    "Universal",
    "W-SU",
    "WET",
    "Zulu"
            };
        }


        /// <summary>
        /// Converts an Olson time zone ID to a Windows time zone ID.
        /// </summary>
        /// <param name="olsonTimeZoneId">An Olson time zone ID. See https://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html. </param>
        /// <returns>
        /// The TimeZoneInfo corresponding to the Olson time zone ID, 
        /// or null if you passed in an invalid Olson time zone ID.
        /// </returns>
        /// <remarks>
        /// See https://unicode.org/repos/cldr/trunk/common/supplemental/windowsZones.xml
        /// </remarks>
        //public static TimeZoneInfo OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
        public static string OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
        {
            var olsonWindowsTimes = new Dictionary<string, string>()
                {
                    { "Africa/Bangui", "W. Central Africa Standard Time" },
                    { "Africa/Cairo", "Egypt Standard Time" },
                    { "Africa/Casablanca", "Morocco Standard Time" },
                    { "Africa/Harare", "South Africa Standard Time" },
                    { "Africa/Johannesburg", "South Africa Standard Time" },
                    { "Africa/Lagos", "W. Central Africa Standard Time" },
                    { "Africa/Monrovia", "Greenwich Standard Time" },
                    { "Africa/Nairobi", "E. Africa Standard Time" },
                    { "Africa/Windhoek", "Namibia Standard Time" },
                    { "America/Anchorage", "Alaskan Standard Time" },
                    { "America/Argentina/San_Juan", "Argentina Standard Time" },
                    { "America/Asuncion", "Paraguay Standard Time" },
                    { "America/Bahia", "Bahia Standard Time" },
                    { "America/Bogota", "SA Pacific Standard Time" },
                    { "America/Buenos_Aires", "Argentina Standard Time" },
                    { "America/Caracas", "Venezuela Standard Time" },
                    { "America/Cayenne", "SA Eastern Standard Time" },
                    { "America/Chicago", "Central Standard Time" },
                    { "America/Chihuahua", "Mountain Standard Time (Mexico)" },
                    { "America/Cuiaba", "Central Brazilian Standard Time" },
                    { "America/Denver", "Mountain Standard Time" },
                    { "America/Fortaleza", "SA Eastern Standard Time" },
                    { "America/Godthab", "Greenland Standard Time" },
                    { "America/Guatemala", "Central America Standard Time" },
                    { "America/Halifax", "Atlantic Standard Time" },
                    { "America/Indianapolis", "US Eastern Standard Time" },
                    { "America/Indiana/Indianapolis", "US Eastern Standard Time" },
                    { "America/La_Paz", "SA Western Standard Time" },
                    { "America/Los_Angeles", "Pacific Standard Time" },
                    { "America/Mexico_City", "Mexico Standard Time" },
                    { "America/Montevideo", "Montevideo Standard Time" },
                    { "America/New_York", "Eastern Standard Time" },
                    { "America/Noronha", "UTC-02" },
                    { "America/Phoenix", "US Mountain Standard Time" },
                    { "America/Regina", "Canada Central Standard Time" },
                    { "America/Santa_Isabel", "Pacific Standard Time (Mexico)" },
                    { "America/Santiago", "Pacific SA Standard Time" },
                    { "America/Sao_Paulo", "E. South America Standard Time" },
                    { "America/St_Johns", "Newfoundland Standard Time" },
                    { "America/Tijuana", "Pacific Standard Time" },
                    { "Antarctica/McMurdo", "New Zealand Standard Time" },
                    { "Atlantic/South_Georgia", "UTC-02" },
                    { "Asia/Almaty", "Central Asia Standard Time" },
                    { "Asia/Amman", "Jordan Standard Time" },
                    { "Asia/Baghdad", "Arabic Standard Time" },
                    { "Asia/Baku", "Azerbaijan Standard Time" },
                    { "Asia/Bangkok", "Asia Standard Time" },
                    { "Asia/Beirut", "Middle East Standard Time" },
                    { "Asia/Calcutta", "India Standard Time" },
                    { "Asia/Colombo", "Sri Lanka Standard Time" },
                    { "Asia/Damascus", "Syria Standard Time" },
                    { "Asia/Dhaka", "Bangladesh Standard Time" },
                    { "Asia/Dubai", "Arabian Standard Time" },
                    { "Asia/Irkutsk", "North Asia East Standard Time" },
                    { "Asia/Jerusalem", "Israel Standard Time" },
                    { "Asia/Kabul", "Afghanistan Standard Time" },
                    { "Asia/Kamchatka", "Kamchatka Standard Time" },
                    { "Asia/Karachi", "Pakistan Standard Time" },
                    { "Asia/Katmandu", "Nepal Standard Time" },
                    { "Asia/Kolkata", "India Standard Time" },
                    { "Asia/Krasnoyarsk", "North Asia Standard Time" },
                    { "Asia/Kuala_Lumpur", "Singapore Standard Time" },
                    { "Asia/Kuwait", "Arab Standard Time" },
                    { "Asia/Magadan", "Magadan Standard Time" },
                    { "Asia/Muscat", "Arabian Standard Time" },
                    { "Asia/Novosibirsk", "N. Central Asia Standard Time" },
                    { "Asia/Oral", "West Asia Standard Time" },
                    { "Asia/Rangoon", "Myanmar Standard Time" },
                    { "Asia/Riyadh", "Arab Standard Time" },
                    { "Asia/Seoul", "Korea Standard Time" },
                    { "Asia/Shanghai", "China Standard Time" },
                    { "Asia/Singapore", "Singapore Standard Time" },
                    { "Asia/Taipei", "Taipei Standard Time" },
                    { "Asia/Tashkent", "West Asia Standard Time" },
                    { "Asia/Tbilisi", "Georgian Standard Time" },
                    { "Asia/Tehran", "Iran Standard Time" },
                    { "Asia/Tokyo", "Tokyo Standard Time" },
                    { "Asia/Ulaanbaatar", "Ulaanbaatar Standard Time" },
                    { "Asia/Vladivostok", "Vladivostok Standard Time" },
                    { "Asia/Yakutsk", "Yakutsk Standard Time" },
                    { "Asia/Yekaterinburg", "Ekaterinburg Standard Time" },
                    { "Asia/Yerevan", "Armenian Standard Time" },
                    { "Atlantic/Azores", "Azores Standard Time" },
                    { "Atlantic/Cape_Verde", "Cape Verde Standard Time" },
                    { "Atlantic/Reykjavik", "Greenwich Standard Time" },
                    { "Australia/Adelaide", "Cen. Australia Standard Time" },
                    { "Australia/Brisbane", "E. Australia Standard Time" },
                    { "Australia/Darwin", "AUS Central Standard Time" },
                    { "Australia/Hobart", "Tasmania Standard Time" },
                    { "Australia/Perth", "W. Australia Standard Time" },
                    { "Australia/Sydney", "AUS Eastern Standard Time" },
                    { "Etc/GMT", "UTC" },
                    { "Etc/GMT+11", "UTC-11" },
                    { "Etc/GMT+12", "Dateline Standard Time" },
                    { "Etc/GMT+2", "UTC-02" },
                    { "Etc/GMT-12", "UTC+12" },
                    { "Europe/Amsterdam", "W. Europe Standard Time" },
                    { "Europe/Athens", "GTB Standard Time" },
                    { "Europe/Belgrade", "Central Europe Standard Time" },
                    { "Europe/Berlin", "W. Europe Standard Time" },
                    { "Europe/Brussels", "Romance Standard Time" },
                    { "Europe/Budapest", "Central Europe Standard Time" },
                    { "Europe/Dublin", "GMT Standard Time" },
                    { "Europe/Helsinki", "FLE Standard Time" },
                    { "Europe/Istanbul", "GTB Standard Time" },
                    { "Europe/Kiev", "FLE Standard Time" },
                    { "Europe/London", "GMT Standard Time" },
                    { "Europe/Minsk", "E. Europe Standard Time" },
                    { "Europe/Moscow", "Russian Standard Time" },
                    { "Europe/Paris", "Romance Standard Time" },
                    { "Europe/Sarajevo", "Central European Standard Time" },
                    { "Europe/Warsaw", "Central European Standard Time" },
                    { "Indian/Mauritius", "Mauritius Standard Time" },
                    { "Pacific/Apia", "Samoa Standard Time" },
                    { "Pacific/Auckland", "New Zealand Standard Time" },
                    { "Pacific/Fiji", "Fiji Standard Time" },
                    { "Pacific/Guadalcanal", "Central Pacific Standard Time" },
                    { "Pacific/Guam", "West Pacific Standard Time" },
                    { "Pacific/Honolulu", "Hawaiian Standard Time" },
                    { "Pacific/Pago_Pago", "UTC-11" },
                    { "Pacific/Port_Moresby", "West Pacific Standard Time" },
                    { "Pacific/Tongatapu", "Tonga Standard Time" }
                };

            //var windowsTimeZone = default(TimeZoneInfo);
            foreach (KeyValuePair<string, string> item in olsonWindowsTimes)
            {
                if (item.Value.ToLower() == olsonTimeZoneId.ToLower())
                {
                    //try { windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById(olsonTimeZoneId); }
                    //catch (TimeZoneNotFoundException) { }
                    //catch (InvalidTimeZoneException) { }
                    return item.Key;
                }
            }

            return string.Empty;
            //return windowsTimeZone;
        }


        public static string GetMomentTimezone(string input)
        {
            if (string.IsNullOrEmpty(input) == false)
            {
                var found = AllTimezone().Where(x => x.ToLower() == input.ToLower());
                if (found != null && found.Any())
                    return found.First();
            }
            return "Etc/UTC";
        }


        public static String getDateOrdinalSuffix(int value)
        {
            int hunRem = value % 100;
            int tenRem = value % 10;

            if (hunRem - tenRem == 10)
            {
                return "th";
            }
            switch (tenRem)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static void TimeSpanToDateParts(DateTime d1, DateTime d2,
            out int years, out int months, out int days, out int hours, out int minutes)
        {
            if (d1 < d2)
            {
                var d3 = d2;
                d2 = d1;
                d1 = d3;
            }

            var span = d1 - d2;

            months = 12 * (d1.Year - d2.Year) + (d1.Month - d2.Month);

            //month may need to be decremented because the above calculates the ceiling of the months, not the floor.
            //to do so we increase d2 by the same number of months and compare.
            //(500ms fudge factor because datetimes are not precise enough to compare exactly)
            if (d1.CompareTo(d2.AddMonths(months).AddMilliseconds(-500)) <= 0)
            {
                --months;
            }

            years = months / 12;
            months -= years * 12;

            if (months == 0 && years == 0)
            {
                days = span.Days;
            }
            else
            {
                var md1 = new DateTime(d1.Year, d1.Month, d1.Day);
                var md2 = new DateTime(d2.Year, d2.Month, d1.Day);
                var mDays = (int)(md1 - md2).TotalDays;

                if (mDays > span.Days)
                {
                    mDays = (int)(md1.AddMonths(-1) - md2).TotalDays;
                }

                days = span.Days - mDays;


            }
            hours = span.Hours;
            minutes = span.Minutes;
        }


        public static double CalcTotalMinutes(TimeSpan fromHour, TimeSpan toHour)
        {
            TimeSpan result = toHour - fromHour;
            return result.TotalMinutes;
        }
        public static int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
        public static string CalculateAgeWithMonths(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int years = today.Year - birthDate.Year;
            int months = today.Month - birthDate.Month;

            if (today.Day < birthDate.Day)
            {
                months--;
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return $"{years} tuổi {months} tháng";
        }
        public static string CalculateFormattedAge(DateTime birthDate)
        {
            if (birthDate == null
                || birthDate == DateTime.MinValue
                || birthDate == DateTimeHelper.MinValueSQL
                || birthDate > DateTime.Today)
                return "---";
            DateTime today = DateTime.Today;

            int totalMonths = (today.Year - birthDate.Year) * 12 + today.Month - birthDate.Month;

            if (today.Day < birthDate.Day)
                totalMonths--;

            int years = totalMonths / 12;
            int months = totalMonths % 12;

            if (years < 3)
            {
                return $"{totalMonths} tháng";
            }
            else if (years < 6)
            {
                return $"{years} tuổi {months} tháng";
            }
            else
            {
                return $"{years} tuổi";
            }
        }

        public static (int Count, string Type) GetDateRangeDescription(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return (1, "WEEK");

            int years = endDate.Year - startDate.Year;
            if (endDate < startDate.AddYears(years))
                years--;

            if (years >= 1)
                return (years, "YEAR");

            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
            if (endDate.Day < startDate.Day)
                months--;

            if (months >= 1)
                return (months, "MONTH");

            int weeks = (int)((endDate - startDate).TotalDays / 7);
            return (weeks, "WEEK");
        }


        #region Convert Times
        private static readonly string _defaultTimeZoneId = "SE Asia Standard Time"; // hoặc "SE Asia Standard Time" trên Windows

        public static DateTime ConvertSettingTimeToUtc(DateTime settingTime, string myTimeZoneId = "")
        {
            if (settingTime == DateTime.MinValue || settingTime == MinValueSQL)
                return settingTime;

            string timeZoneId = _defaultTimeZoneId;
            if (string.IsNullOrEmpty(myTimeZoneId))
            {
                var systemConfiguration = SettingManager.Instance.GetSettingByName(SettingKeys.MyTimeZone);
                if (systemConfiguration != null)
                    timeZoneId = systemConfiguration.SettingValue;
            }
            else
                timeZoneId = myTimeZoneId;


            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var local = DateTime.SpecifyKind(settingTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(local, tz);
        }

        public static DateTime ConvertUTCToSettingTime(DateTime utcDateTime, string myTimeZoneId = "")
        {
            if (utcDateTime == DateTime.MinValue || utcDateTime == MinValueSQL)
                return utcDateTime;

            string timeZoneId = _defaultTimeZoneId;
            if (string.IsNullOrEmpty(myTimeZoneId))
            {
                var systemConfiguration = SettingManager.Instance.GetSettingByName(SettingKeys.MyTimeZone);
                if (systemConfiguration != null)
                    timeZoneId = systemConfiguration.SettingValue;
            }
            else
                timeZoneId = myTimeZoneId;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        public static double GetDifferentTime(string settingTimeZoneId)
        {
            try
            {
                if (string.IsNullOrEmpty(settingTimeZoneId))
                    return 0;
                var yourTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settingTimeZoneId);

                var now = DateTimeOffset.UtcNow;
                TimeSpan yourOffset = yourTimeZone.GetUtcOffset(now);
                return yourOffset.TotalHours;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static double GetDifferentHour(string settingTimeZoneId)
        {
            try
            {
                var yourTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settingTimeZoneId);
                var serverTimeZone = TimeZoneInfo.Local;
                var now = DateTimeOffset.UtcNow;
                TimeSpan yourOffset = yourTimeZone.GetUtcOffset(now);
                TimeSpan serverOffset = serverTimeZone.GetUtcOffset(now);
                TimeSpan different = yourOffset - serverOffset;
                return different.TotalHours;
            }
            catch
            {
                return 0;
            }
        }
        #endregion

    }
}
