using SubSonic;
using System;
using System.Collections.Generic;
using System.Data;

namespace SweetSoft.QLDA.Core.Utils
{
    public static class SubsonicHelpers
    {
        public static DataProvider DefaultProvider = DataService.GetInstance("DataAccessProvider");
        public static DataProvider SysProvider = DataService.GetInstance("DataAccessProvider"); // Sử dụng chung 1 DB cho việc lưu logs
        public static string SysProviderStr = "DataAccessProvider";
        public static List<string> ExecuteTypedListString(this SqlQuery qry)
        {
            List<string> list = new List<string>();
            foreach (System.Data.DataRow row in qry.ExecuteDataSet().Tables[0].Rows)
            {
                list.Add((String)row[0]);
            }
            return list;
        }
        public static List<string> ExecuteTypedListString(this InlineQuery inlineQuery, string sql)
        {
            List<string> list = new List<string>();
            if (inlineQuery == null || string.IsNullOrEmpty(sql))
                return list;
            IDataReader reader = inlineQuery.ExecuteReader(sql);
            if(reader == null)
                return list;
            DataTable dt = new DataTable();
            dt.Load(reader);
            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add((String)row[0]);
            }
            return list;
        }

        public static bool ExecuteQuery(string sql)
        {
            try
            {
                new InlineQuery().Execute(sql);
                return true;
            }
            catch (Exception exc)
            {
                return false;
            }
        }
    }
}
