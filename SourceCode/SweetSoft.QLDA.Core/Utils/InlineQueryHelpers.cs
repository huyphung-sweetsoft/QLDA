using SubSonic;
using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Data;
using System.Linq;

namespace SweetSoft.QLDA.Core.Utils
{
    public class InlineQueryHelpers
    {
        public static string SQLEncode(object keyword, int length = 120)
        {
            if (keyword == null || string.IsNullOrEmpty(keyword.ToString()))
                return "";

            keyword = keyword.ToString().Replace("'", "''").Replace("%", "[%]");
            keyword = keyword.ToString().Substring(0, keyword.ToString().Length < length ? keyword.ToString().Length : length);
            return keyword.ToString();
        }
        public static string SQLFullDateTime(DateTime? date)
        {
            if (date == null || date.Value == DateTime.MinValue)
                return string.Empty;

            return date.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        public static string SQLTime7(TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\.fff");
        }
        public static string SQLShortDate(DateTime? date)
        {
            if (date == null || date.Value == DateTime.MinValue)
                return string.Empty;

            return date.Value.ToString("yyyy-MM-dd");
        }
        public static string SQLStartDate(DateTime? date, bool isConvertUtc = true)
        {
            if (date == null || date.Value == DateTime.MinValue || date.Value == DateTimeHelper.MinValueSQL)
                return string.Empty;

            string strDt = date.Value.ToString("yyyy-MM-dd 00:00:00.000");
            if (isConvertUtc)
                strDt = DateTimeHelper.ConvertSettingTimeToUtc(DateTime.Parse(strDt)).ToString("yyyy-MM-dd HH:mm:ss");
            return strDt;
        }
        public static string SQLEndDate(DateTime? date, bool isConvertUtc = true)
        {
            if (date == null || date.Value == DateTime.MinValue || date.Value == DateTimeHelper.MinValueSQL)
                return string.Empty;

            string strDt = date.Value.ToString("yyyy-MM-dd 23:59:59.998");
            if (isConvertUtc)
                strDt = DateTimeHelper.ConvertSettingTimeToUtc(DateTime.Parse(strDt)).ToString("yyyy-MM-dd HH:mm:ss");
            return strDt;
        }
        public static void GetTotal(ref IDataReader iDataReader, out int total, out bool nextResult)
        {
            total = 0;
            while (iDataReader.Read())
                total = iDataReader.GetInt32(0);
            nextResult = iDataReader.NextResult();
        }
        public static void GetTotal(ref IDataReader iDataReader, out int total)
        {
            total = 0;
            if (!iDataReader.IsClosed && iDataReader.Read())
            {
                try
                {
                    int ordinal = iDataReader.GetOrdinal("total_records");
                    if (ordinal >= 0)
                    {
                        total = iDataReader.GetInt32(ordinal);
                    }
                }
                catch
                {
                    total = 0;
                }
            }
        }
        public static void GetTotal(ref DataTable dataTable, out int total)
        {
            total = 0;

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                // Danh sách tên cột ưu tiên (tùy mở rộng)
                string[] preferredColumns = { "total_records", "TotalRecords", "total_rows", "TotalRows" };

                // Tìm cột tồn tại (không phân biệt hoa thường)
                var match = dataTable.Columns
                    .Cast<DataColumn>()
                    .FirstOrDefault(col => preferredColumns.Any(p => string.Equals(col.ColumnName, p, StringComparison.OrdinalIgnoreCase)));

                if (match != null)
                {
                    object value = dataTable.Rows[0][match];
                    if (value != DBNull.Value)
                    {
                        total = Convert.ToInt32(value);
                    }
                }
            }
        }

        public static void GetValueOfColumnName<T>(ref DataTable dataTable, out T result, string columnName)
        {
            result = default;

            if (dataTable != null && dataTable.Rows.Count > 0 && !string.IsNullOrWhiteSpace(columnName))
            {
                if (dataTable.Columns.Contains(columnName))
                {
                    object value = dataTable.Rows[0][columnName];
                    if (value != DBNull.Value && value is T)
                    {
                        result = (T)value;
                    }
                    else if (value != DBNull.Value)
                    {
                        try
                        {
                            result = (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch
                        {
                            result = default;
                        }
                    }
                }
            }
        }


        public static string SQLBit(bool? value)
        {
            if (value == null)
                return "null";

            return value.Value ? "1" : "0";
        }

        public static void RunScript(string sql)
        {
            new InlineQuery().Execute(sql);
        }
    }
}
